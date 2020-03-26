/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#if !XLUA_GENERAL
using UnityEngine;
using UnityEditor;
#endif
using System.Collections.Generic;
using System.IO;
using XLua;
using System;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CSObjectWrapEditor
{
    public static class GeneratorConfig
    {
#if XLUA_GENERAL
        public static string common_path = "./Gen/";
#else
        public static string common_path = Application.dataPath + "/XLua/Gen/";
#endif

        static GeneratorConfig()
        {
            foreach(var type in (from type in XLua.Utils.GetAllTypes()
            where type.IsAbstract && type.IsSealed
            select type))
            {
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType == typeof(string) && field.IsDefined(typeof(GenPathAttribute), false))
                    {
                        common_path = field.GetValue(null) as string;
                        if (!common_path.EndsWith("/"))
                        {
                            common_path = common_path + "/";
                        }
                    }
                }

                foreach (var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (prop.PropertyType == typeof(string) && prop.IsDefined(typeof(GenPathAttribute), false))
                    {
                        common_path = prop.GetValue(null, null) as string;
                        if (!common_path.EndsWith("/"))
                        {
                            common_path = common_path + "/";
                        }
                    }
                }
            }
        }
    }

    public struct CustomGenTask
    {
        public LuaTable Data;
        public TextWriter Output;
    }

    public struct UserConfig
    {
        public IEnumerable<Type> LuaCallCSharp;
        public IEnumerable<Type> CSharpCallLua;
        public IEnumerable<Type> ReflectionUse;
    }

    public class GenCodeMenuAttribute : Attribute
    {

    }

    public class GenPathAttribute : Attribute
    {

    }

    public struct XLuaTemplate
    {
        public string name;
        public string text;
    }

    public struct XLuaTemplates
    {
        public XLuaTemplate LuaClassWrap;
        public XLuaTemplate LuaDelegateBridge;
        public XLuaTemplate LuaDelegateWrap;
        public XLuaTemplate LuaEnumWrap;
        public XLuaTemplate LuaInterfaceBridge;
        public XLuaTemplate LuaRegister;
        public XLuaTemplate LuaWrapPusher;
        public XLuaTemplate PackUnpack;
        public XLuaTemplate TemplateCommon;
    }

    public static class Generator
    {
        static LuaEnv luaenv = new LuaEnv();
        static List<string> OpMethodNames = new List<string>() { "op_Addition", "op_Subtraction", "op_Multiply", "op_Division", "op_Equality", "op_UnaryNegation", "op_LessThan", "op_LessThanOrEqual", "op_Modulus",
            "op_BitwiseAnd", "op_BitwiseOr", "op_ExclusiveOr", "op_OnesComplement", "op_LeftShift", "op_RightShift"};
        private static XLuaTemplates templateRef;

        static Generator()
        {
#if !XLUA_GENERAL
            TemplateRef template_ref = ScriptableObject.CreateInstance<TemplateRef>();

            templateRef = new XLuaTemplates()
            {
#if GEN_CODE_MINIMIZE
                LuaClassWrap = { name = template_ref.LuaClassWrapGCM.name, text = template_ref.LuaClassWrapGCM.text },
#else
                LuaClassWrap = { name = template_ref.LuaClassWrap.name, text = template_ref.LuaClassWrap.text },
#endif
                LuaDelegateBridge = { name = template_ref.LuaDelegateBridge.name, text = template_ref.LuaDelegateBridge.text },
                LuaDelegateWrap = { name = template_ref.LuaDelegateWrap.name, text = template_ref.LuaDelegateWrap.text },
#if GEN_CODE_MINIMIZE
                LuaEnumWrap = { name = template_ref.LuaEnumWrapGCM.name, text = template_ref.LuaEnumWrapGCM.text },
#else
                LuaEnumWrap = { name = template_ref.LuaEnumWrap.name, text = template_ref.LuaEnumWrap.text },
#endif
                LuaInterfaceBridge = { name = template_ref.LuaInterfaceBridge.name, text = template_ref.LuaInterfaceBridge.text },
#if GEN_CODE_MINIMIZE
                LuaRegister = { name = template_ref.LuaRegisterGCM.name, text = template_ref.LuaRegisterGCM.text },
#else
                LuaRegister = { name = template_ref.LuaRegister.name, text = template_ref.LuaRegister.text },
#endif
                LuaWrapPusher = { name = template_ref.LuaWrapPusher.name, text = template_ref.LuaWrapPusher.text },
                PackUnpack = { name = template_ref.PackUnpack.name, text = template_ref.PackUnpack.text },
                TemplateCommon = { name = template_ref.TemplateCommon.name, text = template_ref.TemplateCommon.text },
            };
#endif
            luaenv.AddLoader((ref string filepath) =>
            {
                if (filepath == "TemplateCommon")
                {
                    return Encoding.UTF8.GetBytes(templateRef.TemplateCommon.text);
                }
                else
                {
                    return null;
                }
            });
        }

        static bool IsOverride(MethodBase method)
        {
            var m = method as MethodInfo;
            return m != null && !m.IsConstructor && m.IsVirtual && (m.GetBaseDefinition().DeclaringType != m.DeclaringType);
        }

        static int OverloadCosting(MethodBase mi)
        {
            int costing = 0;

            if (!mi.IsStatic)
            {
                costing++;
            }

            foreach (var paraminfo in mi.GetParameters())
            {
                if ((!paraminfo.ParameterType.IsPrimitive ) && (paraminfo.IsIn || !paraminfo.IsOut))
                {
                    costing++;
                }
            }
            costing = costing * 10000 + (mi.GetParameters().Length + (mi.IsStatic ? 0 : 1));
            return costing;
        }

        static IEnumerable<Type> type_has_extension_methods = null;

        static IEnumerable<MethodInfo> GetExtensionMethods(Type extendedType)
        {
            if (type_has_extension_methods == null)
            {
                var gen_types = LuaCallCSharp;

                type_has_extension_methods = from type in gen_types
                                             where type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                    .Any(method => isDefined(method, typeof(ExtensionAttribute)))
                                             select type;
            }
            return from type in type_has_extension_methods
                   where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                        where isSupportedExtensionMethod(method, extendedType)
                        select method;
        }

        static bool isSupportedExtensionMethod(MethodBase method, Type extendedType)
        {
            if (!isDefined(method, typeof(ExtensionAttribute)))
                return false;
            var methodParameters = method.GetParameters();
            if (methodParameters.Length < 1)
                return false;

            var hasValidGenericParameter = false;
            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameterType = methodParameters[i].ParameterType;
                if (i == 0)
                {
                    if (parameterType.IsGenericParameter)
                    {
                        var parameterConstraints = parameterType.GetGenericParameterConstraints();
                        if (parameterConstraints.Length == 0) return false;
                        bool firstParamMatch = false;
                        foreach (var parameterConstraint in parameterConstraints)
                        {
                            if (parameterConstraint != typeof(ValueType) && parameterConstraint.IsAssignableFrom(extendedType))
                            {
                                firstParamMatch = true;
                            }
                        }
                        if (!firstParamMatch) return false;

                        hasValidGenericParameter = true;
                    }
                    else if (parameterType != extendedType)
                        return false;
                }
                else if (parameterType.IsGenericParameter)
                {
                    var parameterConstraints = parameterType.GetGenericParameterConstraints();
                    if (parameterConstraints.Length == 0) return false;
                    foreach (var parameterConstraint in parameterConstraints)
                    {
                        if (!parameterConstraint.IsClass || (parameterConstraint == typeof(ValueType)) || Generator.hasGenericParameter(parameterConstraint))
                            return false;
                    }
                    hasValidGenericParameter = true;
                }
            }
            return hasValidGenericParameter || !method.ContainsGenericParameters;
        }

        static bool IsDoNotGen(Type type, string name)
        {
            return DoNotGen.ContainsKey(type) && DoNotGen[type].Contains(name);
        }

        static void getClassInfo(Type type, LuaTable parameters)
        {
            parameters.Set("type", type);

            var constructors = new List<MethodBase>();
            var constructor_def_vals = new List<int>();
            if (!type.IsAbstract)
            {
                foreach (var con in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase).Cast<MethodBase>()
                    .Where(constructor => !isMethodInBlackList(constructor) && !isObsolete(constructor)))
                {
                    int def_count = 0;
                    constructors.Add(con);
                    constructor_def_vals.Add(def_count);

                    var ps = con.GetParameters();
                    for (int i = ps.Length - 1; i >= 0; i--)
                    {
                        if (ps[i].IsOptional ||
                            (ps[i].IsDefined(typeof(ParamArrayAttribute), false) && i > 0 && ps[i - 1].IsOptional))
                        {
                            def_count++;
                            constructors.Add(con);
                            constructor_def_vals.Add(def_count);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            parameters.Set("constructors", constructors);
            parameters.Set("constructor_def_vals", constructor_def_vals);

            List<string> extension_methods_namespace = new List<string>();
            var extension_methods = type.IsInterface ? new MethodInfo[0]:GetExtensionMethods(type).ToArray();
            foreach(var extension_method in extension_methods)
            {
                if (extension_method.DeclaringType.Namespace != null
                    && extension_method.DeclaringType.Namespace != "System.Collections.Generic"
                    && extension_method.DeclaringType.Namespace != "XLua")
                {
                    extension_methods_namespace.Add(extension_method.DeclaringType.Namespace);
                }
            }
            parameters.Set("namespaces", extension_methods_namespace.Distinct().ToList());

            List<LazyMemberInfo> lazyMemberInfos = new List<LazyMemberInfo>();

            //warnning: filter all method start with "op_"  "add_" "remove_" may  filter some ordinary method
            parameters.Set("methods", type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly)
                .Where(method => !isDefined(method, typeof (ExtensionAttribute)) || method.GetParameters()[0].ParameterType.IsInterface || method.DeclaringType != type)
                .Where(method => !method.IsSpecialName 
                    || (
                         ((method.Name == "get_Item" && method.GetParameters().Length == 1) || (method.Name == "set_Item" && method.GetParameters().Length == 2)) 
                         && method.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(string))
                       )
                 ) 
                .Concat(extension_methods)
                .Where(method => !IsDoNotGen(type, method.Name))
                .Where(method => !isMethodInBlackList(method) && (!method.IsGenericMethod || extension_methods.Contains(method) || isSupportedGenericMethod(method)) && !isObsolete(method))
                .GroupBy(method => (method.Name + ((method.IsStatic && (!isDefined(method, typeof (ExtensionAttribute)) || method.GetParameters()[0].ParameterType.IsInterface)) ? "_xlua_st_" : "")), (k, v) =>
                {
                    var overloads = new List<MethodBase>();
                    List<int> def_vals = new List<int>();
                    bool isOverride = false;
                    foreach (var overload in v.Cast<MethodBase>().OrderBy(mb => OverloadCosting(mb)))
                    {
                        int def_count = 0;
                        overloads.Add(overload);
                        def_vals.Add(def_count);

                        if (!isOverride)
                        {
                            isOverride = IsOverride(overload);
                        }

                        var ps = overload.GetParameters();
                        for (int i = ps.Length - 1; i >=0; i--)
                        {
                            if(ps[i].IsOptional ||
                            (ps[i].IsDefined(typeof(ParamArrayAttribute), false) && i > 0 && ps[i - 1].IsOptional))
                            {
                                def_count++;
                                overloads.Add(overload);
                                def_vals.Add(def_count);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    return new {
                        Name = k,
                        IsStatic = overloads[0].IsStatic && (!isDefined(overloads[0], typeof(ExtensionAttribute)) || overloads[0].GetParameters()[0].ParameterType.IsInterface),
                        Overloads = overloads,
                        DefaultValues = def_vals
                    };
                }).ToList());

            parameters.Set("getters", type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly)
                
                .Where(prop => prop.GetIndexParameters().Length == 0 && prop.CanRead && (prop.GetGetMethod() != null)  && prop.Name != "Item" && !isObsolete(prop) && !isObsolete(prop.GetGetMethod()) && !isMemberInBlackList(prop) && !isMemberInBlackList(prop.GetGetMethod())).Select(prop => new { prop.Name, IsStatic = prop.GetGetMethod().IsStatic, ReadOnly = false, Type = prop.PropertyType })
                .Concat(
                    type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly)
                    .Where(field => !isObsolete(field) && !isMemberInBlackList(field))
                    .Select(field => new { field.Name, field.IsStatic, ReadOnly = field.IsInitOnly || field.IsLiteral, Type = field.FieldType })
                ).Where(info => !IsDoNotGen(type, info.Name))/*.Where(getter => !typeof(Delegate).IsAssignableFrom(getter.Type))*/.ToList());

            parameters.Set("setters", type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly)
                .Where(prop => prop.GetIndexParameters().Length == 0 && prop.CanWrite && (prop.GetSetMethod() != null) && prop.Name != "Item" && !isObsolete(prop) && !isObsolete(prop.GetSetMethod()) && !isMemberInBlackList(prop) && !isMemberInBlackList(prop.GetSetMethod())).Select(prop => new { prop.Name, IsStatic = prop.GetSetMethod().IsStatic, Type = prop.PropertyType, IsProperty = true })
                .Concat(
                    type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly)
                    .Where(field => !isObsolete(field) && !isMemberInBlackList(field) && !field.IsInitOnly && !field.IsLiteral)
                    .Select(field => new { field.Name, field.IsStatic, Type = field.FieldType, IsProperty = false })
                ).Where(info => !IsDoNotGen(type, info.Name))/*.Where(setter => !typeof(Delegate).IsAssignableFrom(setter.Type))*/.ToList());

            parameters.Set("operators", type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly)
                .Where(method => OpMethodNames.Contains(method.Name))
                .GroupBy(method => method.Name, (k, v) => new { Name = k, Overloads = v.Cast<MethodBase>().OrderBy(mb => mb.GetParameters().Length).ToList() }).ToList());

            var indexers = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(prop => prop.GetIndexParameters().Length > 0);

            parameters.Set("indexers", indexers.Where(prop => prop.CanRead && (prop.GetGetMethod() != null)).Select(prop => prop.GetGetMethod())
                .Where(method => method.GetParameters().Length == 1 && !method.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(string)))
                .ToList());

            parameters.Set("newindexers", indexers.Where(prop => prop.CanWrite && (prop.GetSetMethod() != null)).Select(prop => prop.GetSetMethod())
                .Where(method => method.GetParameters().Length == 2 && !method.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(string)))
                .ToList());

            parameters.Set("events", type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly).Where(e => !isObsolete(e) && !isMemberInBlackList(e))
                .Where(ev=> ev.GetAddMethod() != null || ev.GetRemoveMethod() != null)
                .Where(ev => !IsDoNotGen(type, ev.Name))
                .Select(ev => new { IsStatic = ev.GetAddMethod() != null? ev.GetAddMethod().IsStatic: ev.GetRemoveMethod().IsStatic, ev.Name,
                    CanSet = false, CanAdd = ev.GetRemoveMethod() != null, CanRemove = ev.GetRemoveMethod() != null, Type = ev.EventHandlerType})
                .ToList());
            
            parameters.Set("lazymembers", lazyMemberInfos);
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly)
                .Where(m => IsDoNotGen(type, m.Name))
                .GroupBy(m=>m.Name).Select(g => g.First())
                )
            {
                switch(member.MemberType)
                {
                    case MemberTypes.Method:
                        MethodBase mb = member as MethodBase;
                        lazyMemberInfos.Add(new LazyMemberInfo
                        {
                            Index = mb.IsStatic ? "CLS_IDX" : "METHOD_IDX",
                            Name = member.Name,
                            MemberType = "LazyMemberTypes.Method",
                            IsStatic = mb.IsStatic ? "true" : "false"
                        });
                        break;
                    case MemberTypes.Event:
                        EventInfo ev = member as EventInfo;
                        if (ev.GetAddMethod() == null && ev.GetRemoveMethod() == null) break;
                        bool eventIsStatic = ev.GetAddMethod() != null ? ev.GetAddMethod().IsStatic : ev.GetRemoveMethod().IsStatic;
                        lazyMemberInfos.Add(new LazyMemberInfo {
                            Index = eventIsStatic ? "CLS_IDX" : "METHOD_IDX",
                            Name = member.Name,
                            MemberType = "LazyMemberTypes.Event",
                            IsStatic = eventIsStatic ? "true" : "false"
                        });
                        break;
                    case MemberTypes.Field:
                        FieldInfo field = member as FieldInfo;
                        lazyMemberInfos.Add(new LazyMemberInfo
                        {
                            Index = field.IsStatic ? "CLS_GETTER_IDX" : "GETTER_IDX",
                            Name = member.Name,
                            MemberType = "LazyMemberTypes.FieldGet",
                            IsStatic = field.IsStatic ? "true" : "false"
                        });
                        lazyMemberInfos.Add(new LazyMemberInfo
                        {
                            Index = field.IsStatic ? "CLS_SETTER_IDX" : "SETTER_IDX",
                            Name = member.Name,
                            MemberType = "LazyMemberTypes.FieldSet",
                            IsStatic = field.IsStatic ? "true" : "false"
                        });
                        break;
                    case MemberTypes.Property:
                        PropertyInfo prop = member as PropertyInfo;
                        if (prop.Name != "Item" || prop.GetIndexParameters().Length == 0)
                        {
                            if (prop.CanRead && prop.GetGetMethod() != null)
                            {
                                var isStatic = prop.GetGetMethod().IsStatic;
                                lazyMemberInfos.Add(new LazyMemberInfo
                                {
                                    Index = isStatic ? "CLS_GETTER_IDX" : "GETTER_IDX",
                                    Name = member.Name,
                                    MemberType = "LazyMemberTypes.PropertyGet",
                                    IsStatic = isStatic ? "true" : "false"
                                });
                            }
                            if (prop.CanWrite && prop.GetSetMethod() != null)
                            {
                                var isStatic = prop.GetSetMethod().IsStatic;
                                lazyMemberInfos.Add(new LazyMemberInfo
                                {
                                    Index = isStatic ? "CLS_SETTER_IDX" : "SETTER_IDX",
                                    Name = member.Name,
                                    MemberType = "LazyMemberTypes.PropertySet",
                                    IsStatic = isStatic ? "true" : "false"
                                });
                            }
                        }
                        break;
                }
            }
        }

        class LazyMemberInfo
        {
            public string Index;
            public string Name;
            public string MemberType;
            public string IsStatic;
        }

        static void getInterfaceInfo(Type type, LuaTable parameters)
        {
            parameters.Set("type", type);

            var itfs = new Type[] { type }.Concat(type.GetInterfaces());
            parameters.Set("methods", itfs.SelectMany(i => i.GetMethods())
                .Where(method => !method.IsSpecialName && !method.IsGenericMethod && !method.Name.StartsWith("op_") && !method.Name.StartsWith("add_") && !method.Name.StartsWith("remove_")) //GenericMethod can not be invoke becuase not static info available!
                    .ToList());

            parameters.Set("propertys", itfs.SelectMany(i => i.GetProperties())
                .Where(prop => (prop.CanRead || prop.CanWrite) && prop.Name != "Item")
                    .ToList());

            parameters.Set("events", itfs.SelectMany(i => i.GetEvents()).ToList());

            parameters.Set("indexers", itfs.SelectMany(i => i.GetProperties())
                .Where(prop => (prop.CanRead || prop.CanWrite) && prop.Name == "Item")
                    .ToList());
        }

        static bool isObsolete(MemberInfo mb)
        {
            if (mb == null) return false;
            ObsoleteAttribute oa = GetCustomAttribute(mb, typeof(ObsoleteAttribute)) as ObsoleteAttribute;
#if XLUA_GENERAL && !XLUA_ALL_OBSOLETE || XLUA_JUST_EXCLUDE_ERROR
            return oa != null && oa.IsError;
#else
            return oa != null;
#endif
        }

        static bool isObsolete(Type type)
        {
            if (type == null) return false;
            if (isObsolete(type as MemberInfo))
            {
                return true;
            }
            return (type.DeclaringType != null) ? isObsolete(type.DeclaringType) : false;
        }

        static bool isMemberInBlackList(MemberInfo mb)
        {
            if (isDefined(mb, typeof(BlackListAttribute))) return true;
            if (mb is FieldInfo && (mb as FieldInfo).FieldType.IsPointer) return true;
            if (mb is PropertyInfo && (mb as PropertyInfo).PropertyType.IsPointer) return true;

            foreach(var filter in memberFilters)
            {
                if (filter(mb))
                {
                    return true;
                }
            }

            foreach (var exclude in BlackList)
            {
                if (mb.DeclaringType.ToString() == exclude[0] && mb.Name == exclude[1])
                {
                    return true;
                }
            }

            return false;
        }

        static bool isMethodInBlackList(MethodBase mb)
        {
            if (isDefined(mb, typeof(BlackListAttribute))) return true;

            //指针目前不支持，先过滤
            if (mb.GetParameters().Any(pInfo => pInfo.ParameterType.IsPointer)) return true;
            if (mb is MethodInfo && (mb as MethodInfo).ReturnType.IsPointer) return true;

            foreach (var filter in memberFilters)
            {
                if (filter(mb))
                {
                    return true;
                }
            }

            foreach (var exclude in BlackList)
            {
                if (mb.DeclaringType.ToString() == exclude[0] && mb.Name == exclude[1])
                {
                    var parameters = mb.GetParameters();
                    if (parameters.Length != exclude.Count - 2)
                    {
                        continue;
                    }
                    bool paramsMatch = true;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType.ToString() != exclude[i + 2])
                        {
                            paramsMatch = false;
                            break;
                        }
                    }
                    if (paramsMatch) return true;
                }
            }
            return false;
        }

        static Dictionary<string, LuaFunction> templateCache = new Dictionary<string, LuaFunction>();
        static void GenOne(Type type, Action<Type, LuaTable> type_info_getter, XLuaTemplate templateAsset, StreamWriter textWriter)
        {
            if (isObsolete(type)) return;
            LuaFunction template;
            if (!templateCache.TryGetValue(templateAsset.name, out template))
            {
                template = XLua.TemplateEngine.LuaTemplate.Compile(luaenv, templateAsset.text);
                templateCache[templateAsset.name] = template;
            }

            LuaTable type_info = luaenv.NewTable();
            LuaTable meta = luaenv.NewTable();
            meta.Set("__index", luaenv.Global);
            type_info.SetMetaTable(meta);
            meta.Dispose();

            type_info_getter(type, type_info);

            try
            {
                string genCode = XLua.TemplateEngine.LuaTemplate.Execute(template, type_info);
                //string filePath = save_path + type.ToString().Replace("+", "").Replace(".", "").Replace("`", "").Replace("&", "").Replace("[", "").Replace("]", "").Replace(",", "") + file_suffix + ".cs";
                textWriter.Write(genCode);
                textWriter.Flush();
            }
            catch (Exception e)
            {
#if XLUA_GENERAL
                System.Console.WriteLine("Error: gen wrap file fail! err=" + e.Message + ", stack=" + e.StackTrace);
#else
                Debug.LogError("gen wrap file fail! err=" + e.Message + ", stack=" + e.StackTrace);
#endif
            }
            finally
            {
                type_info.Dispose();
            }
        }

        static void GenEnumWrap(IEnumerable<Type> types, string save_path)
        {
            string filePath = save_path + "EnumWrap.cs";
            StreamWriter textWriter = new StreamWriter(filePath, false, Encoding.UTF8);
            
            GenOne(null, (type, type_info) =>
            {
                var type2fields = luaenv.NewTable();
                foreach(var _type in types)
                    type2fields.Set(_type, _type.GetFields(BindingFlags.Public | BindingFlags.Static).Where(x => !isMemberInBlackList(x)).ToArray());
                type_info.Set("type2fields", type2fields);
                type_info.Set("types", types.ToList());
            }, templateRef.LuaEnumWrap, textWriter);

            textWriter.Close();
        }

        static string NonmalizeName(string name)
        {
            return name.Replace("+", "_").Replace(".", "_").Replace("`", "_").Replace("&", "_").Replace("[", "_").Replace("]", "_").Replace(",", "_");
        }

        static void GenInterfaceBridge(IEnumerable<Type> types, string save_path)
        {
            foreach (var wrap_type in types)
            {
                if (!wrap_type.IsInterface) continue;

                string filePath = save_path + NonmalizeName(wrap_type.ToString()) + "Bridge.cs";
                StreamWriter textWriter = new StreamWriter(filePath, false, Encoding.UTF8);
                GenOne(wrap_type, (type, type_info) =>
                {
                    getInterfaceInfo(type, type_info);
                }, templateRef.LuaInterfaceBridge, textWriter);
                textWriter.Close();
            }
        }

        class ParameterInfoSimulation
        {
            public string Name;
            public bool IsOut;
            public bool IsIn;
            public Type ParameterType;
            public bool IsParamArray;
        }

        class MethodInfoSimulation
        {
            public Type ReturnType;
            public ParameterInfoSimulation[] ParameterInfos;

            public int HashCode;

            public ParameterInfoSimulation[]  GetParameters()
            {
                return ParameterInfos;
            }

            public Type DeclaringType = null;
            public string DeclaringTypeName = null;
        }

        static MethodInfoSimulation makeMethodInfoSimulation(MethodInfo method)
        {
            int hashCode = method.ReturnType.GetHashCode();

            List<ParameterInfoSimulation> paramsExpect = new List<ParameterInfoSimulation>();

            foreach (var param in method.GetParameters())
            {
                if (param.IsOut)
                {
                    hashCode++;
                }
                hashCode += param.ParameterType.GetHashCode();
                paramsExpect.Add(new ParameterInfoSimulation()
                {
                    Name = param.Name,
                    IsOut = param.IsOut,
                    IsIn = param.IsIn,
                    ParameterType = param.ParameterType,
                    IsParamArray = param.IsDefined(typeof(System.ParamArrayAttribute), false)
                });
            }

            return new MethodInfoSimulation()
            {
                ReturnType = method.ReturnType,
                HashCode = hashCode,
                ParameterInfos = paramsExpect.ToArray(),
                DeclaringType = method.DeclaringType
            };
        }

        static bool isNotPublic(Type type)
        {
            if (type.IsByRef || type.IsArray)
            {
                return isNotPublic(type.GetElementType());
            }
            else
            {
                if ((!type.IsNested && !type.IsPublic) || (type.IsNested && !type.IsNestedPublic))
                {
                    return true;
                }
                if (type.IsGenericType)
                {
                    foreach (var ga in type.GetGenericArguments())
                    {
                        if (isNotPublic(ga))
                        {
                            return true;
                        }
                    }
                }
                if (type.IsNested)
                {
                    var parent = type.DeclaringType;
                    while (parent != null)
                    {
                        if ((!parent.IsNested && !parent.IsPublic) || (parent.IsNested && !parent.IsNestedPublic))
                        {
                            return true;
                        }
                        if (parent.IsNested)
                        {
                            parent = parent.DeclaringType;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                return false;
            }
        }

        static bool hasGenericParameter(Type type)
        {
            if (type.IsByRef || type.IsArray)
            {
                return hasGenericParameter(type.GetElementType());
            }
            if (type.IsGenericType)
            {
                foreach (var typeArg in type.GetGenericArguments())
                {
                    if (hasGenericParameter(typeArg))
                    {
                        return true;
                    }
                }
                return false;
            }
            return type.IsGenericParameter;
        }

        static MethodInfoSimulation makeHotfixMethodInfoSimulation(MethodBase hotfixMethod, HotfixFlag hotfixType)
        {
            bool ignoreValueType = hotfixType.HasFlag(HotfixFlag.ValueTypeBoxing);
            //ignoreValueType = true;

            Type retTypeExpect = (hotfixMethod.IsConstructor ? typeof(void) : (hotfixMethod as MethodInfo).ReturnType);
            int hashCode = retTypeExpect.GetHashCode();
            List<ParameterInfoSimulation> paramsExpect = new List<ParameterInfoSimulation>();
            if (!hotfixMethod.IsStatic) // add self
            {
                paramsExpect.Add(new ParameterInfoSimulation()
                {
                    Name = "self",
                    IsOut = false,
                    IsIn = true,
                    ParameterType = (hotfixMethod.DeclaringType.IsValueType && !ignoreValueType) ? hotfixMethod.DeclaringType : typeof(object),
                    IsParamArray = false
                });
                hashCode += paramsExpect[0].ParameterType.GetHashCode();
            }

            foreach (var param in hotfixMethod.GetParameters())
            {
                var paramExpect = new ParameterInfoSimulation()
                {
                    Name = param.Name,
                    IsOut = param.IsOut,
                    IsIn = param.IsIn,
                    ParameterType = (param.ParameterType.IsByRef || (param.ParameterType.IsValueType && !ignoreValueType)
                      || param.IsDefined(typeof(System.ParamArrayAttribute), false)) ? param.ParameterType : typeof(object),
                    IsParamArray = param.IsDefined(typeof(System.ParamArrayAttribute), false)
                };
                if (param.IsOut)
                {
                    hashCode++;
                }
                hashCode += paramExpect.ParameterType.GetHashCode();
                paramsExpect.Add(paramExpect);
            }

            return new MethodInfoSimulation()
            {
                HashCode = hashCode,
                ReturnType = retTypeExpect,
                ParameterInfos = paramsExpect.ToArray()
            };
        }

        class MethodInfoSimulationComparer : IEqualityComparer<MethodInfoSimulation>
        {
            public bool Equals(MethodInfoSimulation x, MethodInfoSimulation y)
            {
                if (object.ReferenceEquals(x, y)) return true;
                if (x == null || y == null)
                {
                    return false;
                }
                if (x.ReturnType != y.ReturnType)
                {
                    return false;
                }
                var xParams = x.GetParameters();
                var yParams = y.GetParameters();
                if (xParams.Length != yParams.Length)
                {
                    return false;
                }

                for (int i = 0; i < xParams.Length; i++)
                {
                    if (xParams[i].ParameterType != yParams[i].ParameterType || xParams[i].IsOut != yParams[i].IsOut)
                    {
                        return false;
                    }
                }

                var lastPos = xParams.Length - 1;
                return lastPos < 0 || xParams[lastPos].IsParamArray == yParams[lastPos].IsParamArray;
            }
            public int GetHashCode(MethodInfoSimulation obj)
            {
                return obj.HashCode;
            }
        }

        static bool injectByGeneric(MethodBase method, HotfixFlag hotfixType)
        {
            bool ignoreValueType = hotfixType.HasFlag(HotfixFlag.ValueTypeBoxing);
            //ignoreValueType = true;

            if (!method.IsConstructor && (isNotPublic((method as MethodInfo).ReturnType) || hasGenericParameter((method as MethodInfo).ReturnType))) return true;

            if (!method.IsStatic 
                &&(((method.DeclaringType.IsValueType && !ignoreValueType) && isNotPublic(method.DeclaringType)) || hasGenericParameter(method.DeclaringType)))
            {
                return true;
            }

            foreach (var param in method.GetParameters())
            {
                if ((((param.ParameterType.IsValueType && !ignoreValueType) 
                    || param.ParameterType.IsByRef || param.IsDefined(typeof(System.ParamArrayAttribute), false)) && isNotPublic(param.ParameterType)) 
                    || hasGenericParameter(param.ParameterType))
                    return true;
            }
            return false;
        }

        static bool HasFlag(this HotfixFlag toCheck, HotfixFlag flag)
        {
            return (toCheck != HotfixFlag.Stateless) && ((toCheck & flag) == flag);
        }

        static void GenDelegateBridge(IEnumerable<Type> types, string save_path, IEnumerable<Type> hotfix_check_types)
        {
            string filePath = save_path + "DelegatesGensBridge.cs";
            StreamWriter textWriter = new StreamWriter(filePath, false, Encoding.UTF8);
            types = types.Where(type => !type.GetMethod("Invoke").GetParameters().Any(paramInfo => paramInfo.ParameterType.IsGenericParameter));
            var hotfxDelegates = new List<MethodInfoSimulation>();
            var comparer = new MethodInfoSimulationComparer();

            var bindingAttrOfMethod = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic;
            var bindingAttrOfConstructor = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic;
            foreach (var type in (from type in hotfix_check_types where isDefined(type, typeof(HotfixAttribute)) select type))
            {
                var ca = GetCustomAttribute(type, typeof(HotfixAttribute));
#if XLUA_GENERAL
                var hotfixType = (HotfixFlag)Convert.ToInt32(ca.GetType().GetProperty("Flag").GetValue(ca, null));
#else
                var hotfixType = (ca as HotfixAttribute).Flag;
#endif
                HotfixCfg[type] = hotfixType;
            }
            foreach (var kv in HotfixCfg)
            {
                if (kv.Key.Name.Contains("<") || kv.Value.HasFlag(HotfixFlag.Inline))
                {
                    continue;
                }
                bool ignoreProperty = kv.Value.HasFlag(HotfixFlag.IgnoreProperty);
                bool ignoreNotPublic = kv.Value.HasFlag(HotfixFlag.IgnoreNotPublic);
                bool ignoreCompilerGenerated = kv.Value.HasFlag(HotfixFlag.IgnoreCompilerGenerated);
                if (ignoreCompilerGenerated && isDefined(kv.Key, typeof(CompilerGeneratedAttribute)))
                {
                    continue;
                }
                //ignoreProperty = true;
                hotfxDelegates.AddRange(kv.Key.GetMethods(bindingAttrOfMethod)
                    .Where(method => method.GetMethodBody() != null)
                    .Where(method => !method.Name.Contains("<"))
                    .Where(method => !ignoreCompilerGenerated || !isDefined(method, typeof(CompilerGeneratedAttribute)))
                    .Where(method => !ignoreNotPublic || method.IsPublic)
                    .Where(method => !ignoreProperty || !method.IsSpecialName || (!method.Name.StartsWith("get_") && !method.Name.StartsWith("set_")))
                    .Cast<MethodBase>()
                    .Concat(kv.Key.GetConstructors(bindingAttrOfConstructor).Cast<MethodBase>())
                    .Where(method => !injectByGeneric(method, kv.Value))
                    .Select(method => makeHotfixMethodInfoSimulation(method, kv.Value)));
            }
            hotfxDelegates = hotfxDelegates.Distinct(comparer).ToList();
            for(int i = 0; i < hotfxDelegates.Count; i++)
            {
                hotfxDelegates[i].DeclaringTypeName = "__Gen_Hotfix_Delegate" + i;
            }

            var delegates_groups = types.Select(delegate_type => makeMethodInfoSimulation(delegate_type.GetMethod("Invoke")))
                .Where(d => d.DeclaringType.FullName != null)
                .Concat(hotfxDelegates)
                .GroupBy(d => d, comparer).Select((group) => new { Key = group.Key, Value = group.ToList()});
            GenOne(typeof(DelegateBridge), (type, type_info) =>
            {
                type_info.Set("delegates_groups", delegates_groups.ToList());
            }, templateRef.LuaDelegateBridge, textWriter);
            textWriter.Close();
        }

        static void GenWrapPusher(IEnumerable<Type> types, string save_path)
        {
            string filePath = save_path + "WrapPusher.cs";
            StreamWriter textWriter = new StreamWriter(filePath, false, Encoding.UTF8);
            var emptyMap = new Dictionary<Type, Type>();
            GenOne(typeof(ObjectTranslator), (type, type_info) =>
            {
                type_info.Set("purevaluetypes", types
                     .Where(t => t.IsEnum || (!t.IsPrimitive && SizeOf(t) != -1))
                     .Select(t => new {
                         Type = t,
                         Size = SizeOf(t),
                         Flag = t.IsEnum ? OptimizeFlag.Default : OptimizeCfg[t],
                         FieldInfos = (t.IsEnum || OptimizeCfg[t] == OptimizeFlag.Default) ? null : getXluaTypeInfo(t, emptyMap).FieldInfos
                     }).ToList());
                type_info.Set("tableoptimzetypes", types.Where(t => !t.IsEnum && SizeOf(t) == -1)
                     .Select(t => new { Type = t, Fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) })
                     .ToList());
            }, templateRef.LuaWrapPusher, textWriter);
            textWriter.Close();
        }

        static void GenWrap(IEnumerable<Type> types, string save_path)
        {
            types = types.Where(type=>!type.IsEnum);

#if GENERIC_SHARING
            types = types.GroupBy(t => t.IsGenericType ? t.GetGenericTypeDefinition() : t).Select(g => g.Key);
#endif

            var typeMap = types.ToDictionary(type => {
                //Debug.Log("type:" + type);
                return type.ToString();
            });

            foreach (var wrap_type in types)
            {
                string filePath = save_path + NonmalizeName(wrap_type.ToString()) + "Wrap.cs";
                StreamWriter textWriter = new StreamWriter(filePath, false, Encoding.UTF8);
                if (wrap_type.IsEnum)
                {
                    GenOne(wrap_type, (type, type_info) =>
                    {
                        type_info.Set("type", type);
                        type_info.Set("fields", type.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static)
                            .Where(field => !isObsolete(field))
                            .ToList());
                    }, templateRef.LuaEnumWrap, textWriter);
                }
                else if (typeof(Delegate).IsAssignableFrom(wrap_type))
                {


                    GenOne(wrap_type, (type, type_info) =>
                    {
                        type_info.Set("type", type);
                        type_info.Set("delegate", type.GetMethod("Invoke"));
                    }, templateRef.LuaDelegateWrap, textWriter);

                }
                else
                {
                    GenOne(wrap_type, (type, type_info) =>
                    {
                        if (type.BaseType != null && typeMap.ContainsKey(type.BaseType.ToString()))
                        {
                            type_info.Set("base", type.BaseType);
                        }
                        getClassInfo(type, type_info);
                    }, templateRef.LuaClassWrap, textWriter);
                }
                textWriter.Close();
            }
        }

#if !XLUA_GENERAL
        static void clear(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                AssetDatabase.DeleteAsset(path.Substring(path.IndexOf("Assets") + "Assets".Length));

                AssetDatabase.Refresh();
            }
        }
#endif

        class DelegateByMethodDecComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return XLua.Utils.IsParamsMatch(x.GetMethod("Invoke"), y.GetMethod("Invoke"));
            }
            public int GetHashCode(Type obj)
            {
                int hc = 0;
                var method = obj.GetMethod("Invoke");
                hc += method.ReturnType.GetHashCode();
                foreach (var pi in method.GetParameters())
                {
                    hc += pi.ParameterType.GetHashCode();
                }
                return hc;
            }
        }

        public static void GenDelegateBridges(IEnumerable<Type> hotfix_check_types)
        {
            var delegate_types = CSharpCallLua.Where(type => typeof(Delegate).IsAssignableFrom(type));

            GenDelegateBridge(delegate_types, GeneratorConfig.common_path, hotfix_check_types);
        }

        public static void GenEnumWraps()
        {
            var enum_types = LuaCallCSharp.Where(type => type.IsEnum).Distinct();

            GenEnumWrap(enum_types, GeneratorConfig.common_path);
        }

        static MethodInfo makeGenericMethodIfNeeded(MethodInfo method)
        {
            if (!method.ContainsGenericParameters) return method;

            var genericArguments = method.GetGenericArguments();
            var constraintedArgumentTypes = new Type[genericArguments.Length];
            for (var i = 0; i < genericArguments.Length; i++)
            {
                var argumentType = genericArguments[i];
                var parameterConstraints = argumentType.GetGenericParameterConstraints();
                Type parameterConstraint = parameterConstraints[0];
                foreach(var type in argumentType.GetGenericParameterConstraints())
                {
                    if (parameterConstraint.IsAssignableFrom(type))
                    {
                        parameterConstraint = type;
                    }
                }
                
                constraintedArgumentTypes[i] = parameterConstraint;
            }
            return method.MakeGenericMethod(constraintedArgumentTypes);
        }

        public static void GenLuaRegister(bool minimum = false)
        {
            var wraps = minimum ? new List<Type>() : LuaCallCSharp;

            var itf_bridges = CSharpCallLua.Where(t => t.IsInterface);

            string filePath = GeneratorConfig.common_path + "XLuaGenAutoRegister.cs";
            StreamWriter textWriter = new StreamWriter(filePath, false, Encoding.UTF8);

            var lookup = LuaCallCSharp.Distinct().ToDictionary(t => t);

            var extension_methods_from_lcs = (from t in LuaCallCSharp
                                    where isDefined(t, typeof(ExtensionAttribute))
                                    from method in t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                    where isDefined(method, typeof(ExtensionAttribute)) && !isObsolete(method)
                                    where !method.ContainsGenericParameters || isSupportedGenericMethod(method)
                                    select makeGenericMethodIfNeeded(method))
                                    .Where(method => !lookup.ContainsKey(method.GetParameters()[0].ParameterType));

            var extension_methods = (from t in ReflectionUse
                                     where isDefined(t, typeof(ExtensionAttribute))
                                     from method in t.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                     where isDefined(method, typeof(ExtensionAttribute)) && !isObsolete(method)
                                     where !method.ContainsGenericParameters || isSupportedGenericMethod(method)
                                     select makeGenericMethodIfNeeded(method)).Concat(extension_methods_from_lcs);
            GenOne(typeof(DelegateBridgeBase), (type, type_info) =>
            {
#if GENERIC_SHARING
                type_info.Set("wraps", wraps.Where(t=>!t.IsGenericType).ToList());
                var genericTypeGroups = wraps.Where(t => t.IsGenericType).GroupBy(t => t.GetGenericTypeDefinition());

                var typeToArgsList = luaenv.NewTable();
                foreach (var genericTypeGroup in genericTypeGroups)
                {
                    var argsList = luaenv.NewTable();
                    int i = 1;
                    foreach(var genericType in genericTypeGroup)
                    {
                        argsList.Set(i++, genericType.GetGenericArguments());
                    }
                    typeToArgsList.Set(genericTypeGroup.Key, argsList);
                    argsList.Dispose();
                }

                type_info.Set("generic_wraps", typeToArgsList);
                typeToArgsList.Dispose();
#else
                type_info.Set("wraps", wraps.ToList());
#endif

                type_info.Set("itf_bridges", itf_bridges.ToList());
                type_info.Set("extension_methods", extension_methods.ToList());
            }, templateRef.LuaRegister, textWriter);
            textWriter.Close();
        }

        public static void AllSubStruct(Type type, Action<Type> cb)
        {
            if (!type.IsPrimitive && type != typeof(decimal))
            {
                cb(type);
                foreach(var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    AllSubStruct(fieldInfo.FieldType, cb);
                }

                foreach(var propInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if ((AdditionalProperties.ContainsKey(type) && AdditionalProperties[type].Contains(propInfo.Name))
                        || isDefined(propInfo, typeof(AdditionalPropertiesAttribute)))
                    {
                        AllSubStruct(propInfo.PropertyType, cb);
                    }
                }
            }
        }

        class XluaFieldInfo
        {
            public string Name;
            public Type Type;
            public bool IsField;
            public int Size;
        }

        class XluaTypeInfo
        {
            public Type Type;
            public List<XluaFieldInfo> FieldInfos;
            public List<List<XluaFieldInfo>> FieldGroup;
            public bool IsRoot;
        }

        static XluaTypeInfo getXluaTypeInfo(Type t, Dictionary<Type, Type> set)
        {
            var fs = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Select(fi => new XluaFieldInfo { Name = fi.Name, Type = fi.FieldType, IsField = true, Size = SizeOf(fi.FieldType) })
                        .Concat(t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(prop => {
                            return (AdditionalProperties.ContainsKey(t) && AdditionalProperties[t].Contains(prop.Name))
                                || isDefined(prop, typeof(AdditionalPropertiesAttribute));
                        })
                        .Select(prop => new XluaFieldInfo { Name = prop.Name, Type = prop.PropertyType, IsField = false, Size = SizeOf(prop.PropertyType) }));
            int float_field_count = 0;
            bool only_float = true;
            foreach (var f in fs)
            {
                if (f.Type == typeof(float))
                {
                    float_field_count++;
                }
                else
                {
                    only_float = false;
                    break;
                }
            }
            List<List<XluaFieldInfo>> grouped_field = null;
            if (only_float && float_field_count > 1)
            {
                grouped_field = new List<List<XluaFieldInfo>>();
                List<XluaFieldInfo> group = null;
                foreach (var f in fs)
                {
                    if (group == null) group = new List<XluaFieldInfo>();
                    group.Add(f);
                    if (group.Count >= 6)
                    {
                        grouped_field.Add(group);
                        group = null;
                    }
                }
                if (group != null) grouped_field.Add(group);
            }
            return new XluaTypeInfo { Type = t, FieldInfos = fs.ToList(), FieldGroup = grouped_field, IsRoot = set.ContainsKey(t) };
        }

        public static void GenPackUnpack(IEnumerable<Type> types, string save_path)
        {
            var set = types.ToDictionary(type => type);
            List<Type> all_types = new List<Type>();

            foreach(var type in types)
            {
                AllSubStruct(type, (t) =>
                {
                    all_types.Add(t);
                });
            }

            string filePath = save_path + "PackUnpack.cs";
            StreamWriter textWriter = new StreamWriter(filePath, false, Encoding.UTF8);
            GenOne(typeof(CopyByValue), (type, type_info) =>
            {
                type_info.Set("type_infos", all_types.Distinct().Select(t => getXluaTypeInfo(t, set)).ToList());
            }, templateRef.PackUnpack, textWriter);
            textWriter.Close();
        }

        //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
        public static List<Type> LuaCallCSharp = null;

        //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
        public static List<Type> CSharpCallLua = null;

        //黑名单
        public static List<List<string>> BlackList = null;

        public static List<Type> GCOptimizeList = null;

        public static Dictionary<Type, List<string>> AdditionalProperties = null;

        public static List<Type> ReflectionUse = null;

        public static Dictionary<Type, HotfixFlag> HotfixCfg = null;

        public static Dictionary<Type, OptimizeFlag> OptimizeCfg = null;

        public static Dictionary<Type, HashSet<string>> DoNotGen = null;

        public static List<string> assemblyList = null;

        public static List<Func<MemberInfo, bool>> memberFilters = null;

        static void AddToList(List<Type> list, Func<object> get, object attr)
        {
            object obj = get();
            if (obj is Type)
            {
                list.Add(obj as Type);
            }
            else if (obj is IEnumerable<Type>)
            {
                list.AddRange(obj as IEnumerable<Type>);
            }
            else
            {
                throw new InvalidOperationException("Only field/property with the type IEnumerable<Type> can be marked " + attr.GetType().Name);
            }
#if XLUA_GENERAL
            if (attr != null && attr.GetType().ToString() == typeof(GCOptimizeAttribute).ToString())
            {
                var flag = (OptimizeFlag)Convert.ToInt32(attr.GetType().GetProperty("Flag").GetValue(attr, null));
#else
            if (attr is GCOptimizeAttribute)
            {
                var flag = (attr as GCOptimizeAttribute).Flag;
#endif
                if (obj is Type)
                {
                    OptimizeCfg.Add(obj as Type, flag);
                }
                else if (obj is IEnumerable<Type>)
                {
                    foreach(var type in (obj as IEnumerable<Type>))
                    {
                        OptimizeCfg.Add(type, flag);
                    }
                }
            }
        }

        static bool isDefined(MemberInfo test, Type type)
        {
#if XLUA_GENERAL
            return test.GetCustomAttributes(false).Any(ca => ca.GetType().ToString() == type.ToString());
#else
            return test.IsDefined(type, false);
#endif
        }

        static object GetCustomAttribute(MemberInfo test, Type type)
        {
#if XLUA_GENERAL
            return test.GetCustomAttributes(false).FirstOrDefault(ca => ca.GetType().ToString() == type.ToString());
#else
            return test.GetCustomAttributes(type, false).FirstOrDefault();
#endif
        }

        static void MergeCfg(MemberInfo test, Type cfg_type, Func<object> get_cfg)
        {
            if (isDefined(test, typeof(LuaCallCSharpAttribute)))
            {
                object ccla = GetCustomAttribute(test, typeof(LuaCallCSharpAttribute));
                AddToList(LuaCallCSharp, get_cfg, ccla);
#if !XLUA_GENERAL
#pragma warning disable 618
                if (ccla != null && (((ccla as LuaCallCSharpAttribute).Flag & GenFlag.GCOptimize) != 0))
#pragma warning restore 618
                {
                    AddToList(GCOptimizeList, get_cfg, ccla);
                }
#endif
            }
            if (isDefined(test, typeof(CSharpCallLuaAttribute)))
            {
                AddToList(CSharpCallLua, get_cfg, GetCustomAttribute(test, typeof(CSharpCallLuaAttribute)));
            }
            if (isDefined(test, typeof(GCOptimizeAttribute)))
            {
                AddToList(GCOptimizeList, get_cfg, GetCustomAttribute(test, typeof(GCOptimizeAttribute)));
            }
            if (isDefined(test, typeof(ReflectionUseAttribute)))
            {
                AddToList(ReflectionUse, get_cfg, GetCustomAttribute(test, typeof(ReflectionUseAttribute)));
            }
            if (isDefined(test, typeof(HotfixAttribute)))
            {
                object cfg = get_cfg();
                if (cfg is IEnumerable<Type>)
                {
                    var ca = GetCustomAttribute(test, typeof(HotfixAttribute));
#if XLUA_GENERAL
                    var hotfixType = (HotfixFlag)Convert.ToInt32(ca.GetType().GetProperty("Flag").GetValue(ca, null));
#else
                    var hotfixType = (ca as HotfixAttribute).Flag;
#endif
                    foreach (var type in cfg as IEnumerable<Type>)
                    {
                        if (!HotfixCfg.ContainsKey(type) && !isObsolete(type) 
                            && !type.IsEnum && !typeof(Delegate).IsAssignableFrom(type)
                            && (!type.IsGenericType || type.IsGenericTypeDefinition) 
                            && (type.Namespace == null || (type.Namespace != "XLua" && !type.Namespace.StartsWith("XLua.")))
                            && (assemblyList.Contains(type.Module.Assembly.GetName().Name)))
                        {
                            HotfixCfg.Add(type, hotfixType);
                        }
                    }
                }
            }
            if (isDefined(test, typeof(BlackListAttribute))
                        && (typeof(List<List<string>>)).IsAssignableFrom(cfg_type))
            {
                BlackList.AddRange(get_cfg() as List<List<string>>);
            }
            if (isDefined(test, typeof(BlackListAttribute)) && typeof(Func<MemberInfo, bool>).IsAssignableFrom(cfg_type))
            {
                memberFilters.Add(get_cfg() as Func<MemberInfo, bool>);
            }

            if (isDefined(test, typeof(AdditionalPropertiesAttribute))
                        && (typeof(Dictionary<Type, List<string>>)).IsAssignableFrom(cfg_type))
            {
                var cfg = get_cfg() as Dictionary<Type, List<string>>;
                foreach (var kv in cfg)
                {
                    if (!AdditionalProperties.ContainsKey(kv.Key))
                    {
                        AdditionalProperties.Add(kv.Key, kv.Value);
                    }
                }
            }

            if (isDefined(test, typeof(DoNotGenAttribute))
                        && (typeof(Dictionary<Type, List<string>>)).IsAssignableFrom(cfg_type))
            {
                var cfg = get_cfg() as Dictionary<Type, List<string>>;
                foreach (var kv in cfg)
                {
                    HashSet<string> set;
                    if (!DoNotGen.TryGetValue(kv.Key, out set))
                    {
                        set = new HashSet<string>();
                        DoNotGen.Add(kv.Key, set);
                    }
                    set.UnionWith(kv.Value);
                }
            }
        }

        static bool IsPublic(Type type)
        {
            if (type.IsPublic || type.IsNestedPublic)
            {
                if (type.DeclaringType != null)
                {
                    return IsPublic(type.DeclaringType);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public static void GetGenConfig(IEnumerable<Type> check_types)
        {
            LuaCallCSharp = new List<Type>();

            CSharpCallLua = new List<Type>();

            GCOptimizeList = new List<Type>();

            AdditionalProperties = new Dictionary<Type, List<string>>();

            ReflectionUse = new List<Type>();

            BlackList = new List<List<string>>()
            {
            };

            HotfixCfg = new Dictionary<Type, HotfixFlag>();

            OptimizeCfg = new Dictionary<Type, OptimizeFlag>();

            DoNotGen = new Dictionary<Type, HashSet<string>>();

#if UNITY_EDITOR && HOTFIX_ENABLE
            assemblyList = HotfixConfig.GetHotfixAssembly().Select(a => a.GetName().Name).ToList();
#else
            assemblyList = new List<string>();
#endif
            memberFilters = new List<Func<MemberInfo, bool>>();

            foreach (var t in check_types)
            {
                MergeCfg(t, null, () => t);

                if (!t.IsAbstract || !t.IsSealed) continue;

                var fields = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    MergeCfg(field, field.FieldType, () => field.GetValue(null));
                }

                var props = t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                for (int i = 0; i < props.Length; i++)
                {
                    var prop = props[i];
                    MergeCfg(prop, prop.PropertyType, () => prop.GetValue(null, null));
                }
            }
            LuaCallCSharp = LuaCallCSharp.Distinct()
                .Where(type=> IsPublic(type) && !isObsolete(type) && !type.IsGenericTypeDefinition)
                .Where(type => !typeof(Delegate).IsAssignableFrom(type))
                .Where(type => !type.Name.Contains("<"))
                .ToList();
            CSharpCallLua = CSharpCallLua.Distinct()
                .Where(type => IsPublic(type) && !isObsolete(type) && !type.IsGenericTypeDefinition)
                .Where(type => type != typeof(Delegate) && type != typeof(MulticastDelegate))
                .ToList();
            GCOptimizeList = GCOptimizeList.Distinct()
                .Where(type => IsPublic(type) && !isObsolete(type) && !type.IsGenericTypeDefinition)
                .ToList();
            ReflectionUse = ReflectionUse.Distinct()
                .Where(type => !isObsolete(type) && !type.IsGenericTypeDefinition)
                .ToList();
        }

        static Dictionary<Type, int> type_size = new Dictionary<Type, int>()
        {
            { typeof(byte), 1 },
            { typeof(sbyte), 1 },
            { typeof(short), 2 },
            { typeof(ushort), 2 },
            { typeof(int), 4 },
            { typeof(uint), 4 },
            { typeof(long), 8 },
            { typeof(ulong), 8 },
            { typeof(float), 4 },
            { typeof(double), 8 },
            { typeof(decimal), 16 }
        };

        static int SizeOf(Type type)
        {
            if (type_size.ContainsKey(type))
            {
                return type_size[type];
            }

            if (!CopyByValue.IsStruct(type))
            {
                return -1;
            }

            int size = 0;
            foreach(var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                int t_size = SizeOf(fieldInfo.FieldType);
                if (t_size == -1)
                {
                    size = -1;
                    break;
                }
                size += t_size;
            }
            if (size != -1)
            {
                foreach (var propInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if ((AdditionalProperties.ContainsKey(type) && AdditionalProperties[type].Contains(propInfo.Name)) || isDefined(propInfo, typeof(AdditionalPropertiesAttribute)))
                    {
                        int t_size = SizeOf(propInfo.PropertyType);
                        if (t_size == -1)
                        {
                            size = -1;
                            break;
                        }
                        size += t_size;
                    }
                }
            }

            if (!type_size.ContainsKey(type))
            {
                type_size.Add(type, size);
            }

            return size;
        }

        public static void Gen(IEnumerable<Type> wraps, IEnumerable<Type> gc_optimze_list, IEnumerable<Type> itf_bridges, string save_path)
        {
            templateCache.Clear();
            Directory.CreateDirectory(save_path);
            GenWrap(wraps, save_path);
            GenWrapPusher(gc_optimze_list.Concat(wraps.Where(type=>type.IsEnum)).Distinct(), save_path);
            GenPackUnpack(gc_optimze_list.Where(type => !type.IsPrimitive && SizeOf(type) != -1), save_path);
            GenInterfaceBridge(itf_bridges, save_path);
        }

        public static void GenCodeForClass(bool minimum = false)
        {
            var warp_types = minimum ? new List<Type>() : LuaCallCSharp;
            var itf_bridges_types = CSharpCallLua.Where(type => type.IsInterface).Distinct();

            Gen(warp_types, GCOptimizeList, itf_bridges_types, GeneratorConfig.common_path);
        }

#if XLUA_GENERAL
        static bool IsExtensionMethod(MethodInfo method)
        {
            return isDefined(method, typeof(ExtensionAttribute));
        }

        static bool IsDelegate(Type type)
        {
            return type.BaseType != null && type.BaseType.ToString() == "System.MulticastDelegate";
        }

        public static void GenAll(XLuaTemplates templates, IEnumerable<Type> all_types)
        {
            var start = DateTime.Now;
            Directory.CreateDirectory(GeneratorConfig.common_path);
            templateRef = templates;
            GetGenConfig(all_types.Where(type => !type.IsGenericTypeDefinition));
            luaenv.DoString("require 'TemplateCommon'");
            luaenv.Global.Set("IsExtensionMethod", new Func<MethodInfo, bool>(IsExtensionMethod));
            luaenv.Global.Set("IsDelegate", new Func<Type, bool>(IsDelegate));
            var gen_push_types_setter = luaenv.Global.Get<LuaFunction>("SetGenPushAndUpdateTypes");
            gen_push_types_setter.Call(GCOptimizeList.Where(t => !t.IsPrimitive && SizeOf(t) != -1).Concat(LuaCallCSharp.Where(t => t.IsEnum)).Distinct().ToList());
            var xlua_classes_setter = luaenv.Global.Get<LuaFunction>("SetXLuaClasses");
            xlua_classes_setter.Call(XLua.Utils.GetAllTypes().Where(t => t.Namespace == "XLua").ToList());
            GenDelegateBridges(all_types);
            GenEnumWraps();
            GenCodeForClass();
            GenLuaRegister();
            Console.WriteLine("finished! use " + (DateTime.Now - start).TotalMilliseconds + " ms");
            luaenv.Dispose();
        }
#endif

#if !XLUA_GENERAL
        static void callCustomGen()
        {
            foreach (var method in (from type in XLua.Utils.GetAllTypes()
                               from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                               where method.IsDefined(typeof(GenCodeMenuAttribute), false) select method))
            {
                method.Invoke(null, new object[] { });
            }
        }

        [MenuItem("XLua/Generate Code", false, 1)]
        public static void GenAll()
        {
#if UNITY_2018 && (UNITY_EDITOR_WIN || UNITY_EDITOR_OSX)
            if (File.Exists("./Tools/MonoBleedingEdge/bin/mono.exe"))
            {
                GenUsingCLI();
                return;
            }
#endif
            var start = DateTime.Now;
            Directory.CreateDirectory(GeneratorConfig.common_path);
            GetGenConfig(XLua.Utils.GetAllTypes());
            luaenv.DoString("require 'TemplateCommon'");
            var gen_push_types_setter = luaenv.Global.Get<LuaFunction>("SetGenPushAndUpdateTypes");
            gen_push_types_setter.Call(GCOptimizeList.Where(t => !t.IsPrimitive && SizeOf(t) != -1).Concat(LuaCallCSharp.Where(t => t.IsEnum)).Distinct().ToList());
            var xlua_classes_setter = luaenv.Global.Get<LuaFunction>("SetXLuaClasses");
            xlua_classes_setter.Call(XLua.Utils.GetAllTypes().Where(t => t.Namespace == "XLua").ToList());
            GenDelegateBridges(XLua.Utils.GetAllTypes(false));
            GenEnumWraps();
            GenCodeForClass();
            GenLuaRegister();
            callCustomGen();
            Debug.Log("finished! use " + (DateTime.Now - start).TotalMilliseconds + " ms");
            AssetDatabase.Refresh();
        }

#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN
        public static void GenUsingCLI()
        {
#if UNITY_EDITOR_OSX
            var monoPath = "./Tools/MonoBleedingEdge/bin/mono";
#else
            var monoPath = "./Tools/MonoBleedingEdge/bin/mono.exe";
#endif

            var args = new List<string>()
            {
                "./Tools/XLuaGenerate.exe",
                "./Library/ScriptAssemblies/Assembly-CSharp.dll",
                "./Library/ScriptAssemblies/Assembly-CSharp-Editor.dll",
                GeneratorConfig.common_path
            };

            var searchPaths = new List<string>();
            foreach (var path in
                (from asm in AppDomain.CurrentDomain.GetAssemblies() select asm.ManifestModule.FullyQualifiedName)
                 .Distinct())
            {
                try
                {
                    searchPaths.Add(Path.GetDirectoryName(path));
                }
                catch { }
            }
            args.AddRange(searchPaths.Distinct());

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = monoPath;
            process.StartInfo.Arguments = "\"" + string.Join("\" \"", args.ToArray()) + "\"";
            process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            while (!process.StandardError.EndOfStream)
            {
                Debug.LogError(process.StandardError.ReadLine());
            }

            while (!process.StandardOutput.EndOfStream)
            {
                Debug.Log(process.StandardOutput.ReadLine());
            }

            process.WaitForExit();
            GetGenConfig(XLua.Utils.GetAllTypes());
            callCustomGen();
            AssetDatabase.Refresh();
        }
#endif

        [MenuItem("XLua/Clear Generated Code", false, 2)]
        public static void ClearAll()
        {
            clear(GeneratorConfig.common_path);
        }

        public delegate IEnumerable<CustomGenTask> GetTasks(LuaEnv lua_env, UserConfig user_cfg);

        public static void CustomGen(string template_src, GetTasks get_tasks)
        {
            GetGenConfig(XLua.Utils.GetAllTypes());

            LuaFunction template = XLua.TemplateEngine.LuaTemplate.Compile(luaenv,
                template_src);
            foreach (var gen_task in get_tasks(luaenv, new UserConfig() {
                LuaCallCSharp = LuaCallCSharp,
                CSharpCallLua = CSharpCallLua,
                ReflectionUse = ReflectionUse
            }))
            {
                LuaTable meta = luaenv.NewTable();
                meta.Set("__index", luaenv.Global);
                gen_task.Data.SetMetaTable(meta);
                meta.Dispose();

                try
                {
                    string genCode = XLua.TemplateEngine.LuaTemplate.Execute(template, gen_task.Data);
                    gen_task.Output.Write(genCode);
                    gen_task.Output.Flush();
                }
                catch (Exception e)
                {
                    Debug.LogError("gen file fail! template=" + template_src + ", err=" + e.Message + ", stack=" + e.StackTrace);
                }
                finally
                {
                    gen_task.Output.Close();
                }
            }
        }

#endif

        private static bool isSupportedGenericMethod(MethodInfo method)
        {

            if (!method.ContainsGenericParameters)
                return true;
            var methodParameters = method.GetParameters();
            var _hasGenericParameter = false;
            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameterType = methodParameters[i].ParameterType;
                if (!isSupportGenericParameter(parameterType, true, ref _hasGenericParameter))
                    return false;
            }
            return _hasGenericParameter;
        }
        private static bool isSupportGenericParameter(Type parameterType,bool checkConstraint, ref bool _hasGenericParameter)
        {

            if (parameterType.IsGenericParameter)
            {
                if (!checkConstraint)
                    return false;
                var parameterConstraints = parameterType.GetGenericParameterConstraints();
                if (parameterConstraints.Length == 0) return false;
                foreach (var parameterConstraint in parameterConstraints)
                {
                    if (!parameterConstraint.IsClass || (parameterConstraint == typeof(ValueType)) || Generator.hasGenericParameter(parameterConstraint))
                        return false;
                }
                _hasGenericParameter = true;
            }
            else if(parameterType.IsGenericType)
            {
                foreach (var argument in parameterType.GetGenericArguments())
                {
                    if (!isSupportGenericParameter(argument,false, ref _hasGenericParameter))
                        return false;
                }
            }
            return true;
        }
#if !XLUA_GENERAL
        [UnityEditor.Callbacks.PostProcessBuild(1)]
        public static void CheckGenrate(BuildTarget target, string pathToBuiltProject)
        {
            if (EditorApplication.isCompiling || Application.isPlaying)
            {
                return;
            }
            if (!DelegateBridge.Gen_Flag)
            {
                throw new InvalidOperationException("Code has not been genrated, may be not work in phone!");
            }
        }
#endif
    }
}
