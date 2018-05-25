/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#if HOTFIX_ENABLE
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.IO;

#if INJECT_WITHOUT_TOOL || XLUA_GENERAL
using Mono.Cecil;
using Mono.Cecil.Cil;
#endif

#if INJECT_WITHOUT_TOOL || !XLUA_GENERAL
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Diagnostics;
#endif

namespace XLua
{
    public static class HotfixConfig
    {
        //返回-1表示没有标签
        static int getHotfixType(MemberInfo memberInfo)
        {
            foreach (var ca in memberInfo.GetCustomAttributes(false))
            {
                var ca_type = ca.GetType();
                if (ca_type.ToString() == "XLua.HotfixAttribute")
                {
                    return (int)(ca_type.GetProperty("Flag").GetValue(ca, null));
                }
            }
            return -1;
        }

        static void mergeConfig(Dictionary<string, int> hotfixCfg, MemberInfo test, Type cfg_type, Func<IEnumerable<Type>> get_cfg)
        {
            int hotfixType = getHotfixType(test);
            if (-1 == hotfixType || !typeof(IEnumerable<Type>).IsAssignableFrom(cfg_type))
            {
                return;
            }

            foreach (var type in get_cfg())
            {
                if (!type.IsDefined(typeof(ObsoleteAttribute), false)
                    && !type.IsEnum && !typeof(Delegate).IsAssignableFrom(type)
                    && (!type.IsGenericType || type.IsGenericTypeDefinition))
                {
                    string key = type.FullName.Replace('+', '/');
                    if (!hotfixCfg.ContainsKey(key) && (type.Namespace == null || (type.Namespace != "XLua" && !type.Namespace.StartsWith("XLua."))))
                    {
                        hotfixCfg.Add(key, hotfixType);
                    }
                }
            }
        }

        public static void GetConfig(Dictionary<string, int> hotfixCfg, IEnumerable<Type> cfg_check_types)
        {
            if (cfg_check_types != null)
            {
                foreach (var type in cfg_check_types.Where(t => !t.IsGenericTypeDefinition && t.IsAbstract && t.IsSealed))
                {
                    var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                    foreach (var field in type.GetFields(flags))
                    {
                        mergeConfig(hotfixCfg, field, field.FieldType, () => field.GetValue(null) as IEnumerable<Type>);
                    }
                    foreach (var prop in type.GetProperties(flags))
                    {
                        mergeConfig(hotfixCfg, prop, prop.PropertyType, () => prop.GetValue(null, null) as IEnumerable<Type>);
                    }
                }
            }
        }
    }
}
#if XLUA_GENERAL || INJECT_WITHOUT_TOOL

namespace XLua
{
    public static class Hotfix
    {
        static TypeReference objType = null;
        static TypeReference luaTableType = null;

        static TypeDefinition delegateBridgeType = null;
        static MethodDefinition delegateBridgeGetter = null;
        static MethodReference hotfixFlagGetter = null;

        static MethodDefinition invokeSessionStart = null;
        static MethodDefinition functionInvoke = null;
        static MethodDefinition invokeSessionEnd = null;
        static MethodDefinition invokeSessionEndWithResult = null;
        static MethodDefinition inParam = null;
        static MethodDefinition inParams = null;
        static MethodDefinition outParam = null;

        internal static Dictionary<string, int> hotfixCfg;

        static List<MethodDefinition> bridgeIndexByKey;

        static void init(AssemblyDefinition assembly, IEnumerable<string> search_directorys)
        {
#if XLUA_GENERAL
            objType = assembly.MainModule.ImportReference(typeof(object));
#else
            objType = assembly.MainModule.Import(typeof(object));
#endif

            luaTableType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.LuaTable");
            delegateBridgeType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.DelegateBridge");
            delegateBridgeGetter = assembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixDelegateBridge")
                .Methods.Single(m => m.Name == "Get");
            hotfixFlagGetter = assembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixDelegateBridge")
                .Methods.Single(m => m.Name == "xlua_get_hotfix_flag");

            //luaFunctionType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.LuaFunction");
            invokeSessionStart = delegateBridgeType.Methods.Single(m => m.Name == "InvokeSessionStart");
            functionInvoke = delegateBridgeType.Methods.Single(m => m.Name == "Invoke");
            invokeSessionEnd = delegateBridgeType.Methods.Single(m => m.Name == "InvokeSessionEnd");
            invokeSessionEndWithResult = delegateBridgeType.Methods.Single(m => m.Name == "InvokeSessionEndWithResult");
            inParam = delegateBridgeType.Methods.Single(m => m.Name == "InParam");
            inParams = delegateBridgeType.Methods.Single(m => m.Name == "InParams");
            outParam = delegateBridgeType.Methods.Single(m => m.Name == "OutParam");

            bridgeIndexByKey = new List<MethodDefinition>();

            var resolver = assembly.MainModule.AssemblyResolver as BaseAssemblyResolver;
            foreach (var path in
                (from asm in AppDomain.CurrentDomain.GetAssemblies() select asm.ManifestModule.FullyQualifiedName)
                 .Distinct())
            {
                try
                {
                    resolver.AddSearchDirectory(System.IO.Path.GetDirectoryName(path));
                }
                catch(Exception)
                {

                }
            }

            if (search_directorys != null)
            {
                foreach(var directory in search_directorys)
                {
                    resolver.AddSearchDirectory(directory);
                }
            }
        }

        static List<MethodDefinition> hotfix_bridges = null;

        static bool isSameType(TypeReference left, TypeReference right)
        {
            return left.FullName == right.FullName
                && left.Module.Assembly.FullName == right.Module.Assembly.FullName
                && left.Module.FullyQualifiedName == right.Module.FullyQualifiedName;
        }

        [Flags]
        enum HotfixFlagInTool
        {
            Stateless = 0,
            Stateful = 1,
            ValueTypeBoxing = 2,
            IgnoreProperty = 4,
            IgnoreNotPublic = 8,
            Inline = 16,
            IntKey = 32
        }

        static bool HasFlag(this HotfixFlagInTool toCheck, HotfixFlagInTool flag)
        {
            return (toCheck != HotfixFlagInTool.Stateless) && ((toCheck & flag) == flag);
        }

        static bool findHotfixDelegate(AssemblyDefinition assembly, MethodDefinition method, out MethodReference invoke, HotfixFlagInTool hotfixType)
        {
            bool isStateful = hotfixType.HasFlag(HotfixFlagInTool.Stateful);
            bool ignoreValueType = hotfixType.HasFlag(HotfixFlagInTool.ValueTypeBoxing);

            for (int i = 0; i < hotfix_bridges.Count; i++)
            {
                MethodDefinition hotfix_bridge = hotfix_bridges[i];
                var returnType = (isStateful && method.IsConstructor && !method.IsStatic) ? luaTableType : method.ReturnType;
                if (isSameType(returnType, hotfix_bridge.ReturnType))
                {
                    var parametersOfDelegate = hotfix_bridge.Parameters;
                    int compareOffset = 0;
                    if (!method.IsStatic)
                    {
                        var typeOfSelf = (isStateful && !method.IsConstructor) ? luaTableType :
                            ((!ignoreValueType && method.DeclaringType.IsValueType) ? method.DeclaringType : objType);
                        if ((parametersOfDelegate.Count == 0) || parametersOfDelegate[0].ParameterType.IsByReference || !isSameType(typeOfSelf, parametersOfDelegate[0].ParameterType))
                        {
                            continue;
                        }
                        compareOffset++;
                    }
                    if (method.Parameters.Count != (parametersOfDelegate.Count - compareOffset))
                    {
                        continue;
                    }
                    bool paramMatch = true;
                    for (int j = 0; j < method.Parameters.Count; j++)
                    {
                        var param_left = method.Parameters[j];
                        var param_right = parametersOfDelegate[compareOffset++];
                        if (param_left.IsOut != param_right.IsOut
                            || param_left.ParameterType.IsByReference != param_right.ParameterType.IsByReference)
                        {
                            paramMatch = false;
                            break;
                        }
                        if (!ignoreValueType && param_left.ParameterType.IsValueType != param_right.ParameterType.IsValueType)
                        {
                            paramMatch = false;
                            break;
                        }
						bool isparam = param_left.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.Name == "ParamArrayAttribute") != null;
						var type_left = (isparam || param_left.ParameterType.IsByReference || (!ignoreValueType && param_left.ParameterType.IsValueType)) ? param_left.ParameterType : objType;
                        if (!isSameType(type_left, param_right.ParameterType))
                        {
                            paramMatch = false;
                            break;
                        }
                    }

                    if (!paramMatch)
                    {
                        continue;
                    }
                    invoke = hotfix_bridge;
                    return true;
                }
            }
            invoke = null;
            return false;
        }

        static bool hasGenericParameter(TypeReference type)
        {
            if (type.HasGenericParameters)
            {
                return true;
            }
            if (type.IsByReference)
            {
                return hasGenericParameter(((ByReferenceType)type).ElementType);
            }
            if (type.IsArray)
            {
                return hasGenericParameter(((ArrayType)type).ElementType);
            }
            if (type.IsGenericInstance)
            {
                foreach (var typeArg in ((GenericInstanceType)type).GenericArguments)
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

        static bool isNoPublic(AssemblyDefinition assembly, TypeReference type)
        {
            if (type.IsByReference)
            {
                return isNoPublic(assembly, ((ByReferenceType)type).ElementType);
            }
            if (type.IsArray)
            {
                return isNoPublic(assembly, ((ArrayType)type).ElementType);
            }
            else
            {
                if (type.IsGenericInstance)
                {
                    foreach (var typeArg in ((GenericInstanceType)type).GenericArguments)
                    {
                        if (isNoPublic(assembly, typeArg))
                        {
                            return true;
                        }
                    }
                }

                var resolveType = type.Resolve();
                if ((!type.IsNested && !resolveType.IsPublic) || (type.IsNested && !resolveType.IsNestedPublic))
                {
                    return true;
                }
                if (type.IsNested)
                {
                    var parent = type.DeclaringType;
                    while (parent != null)
                    {
                        var resolveParent = parent.Resolve();
                        if ((!parent.IsNested && !resolveParent.IsPublic) || (parent.IsNested && !resolveParent.IsNestedPublic))
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

        static bool genericInOut(AssemblyDefinition assembly, MethodDefinition method, HotfixFlagInTool hotfixType)
        {
            bool isStateful = hotfixType.HasFlag(HotfixFlagInTool.Stateful);
            bool ignoreValueType = hotfixType.HasFlag(HotfixFlagInTool.ValueTypeBoxing);

            if (hasGenericParameter(method.ReturnType) || isNoPublic(assembly, method.ReturnType))
            {
                return true;
            }
            var parameters = method.Parameters;

            if (!method.IsStatic && (!isStateful || method.IsConstructor)
                && (hasGenericParameter(method.DeclaringType) || ((!ignoreValueType && method.DeclaringType.IsValueType) && isNoPublic(assembly, method.DeclaringType))))
            {
                    return true;
            }
            for (int i = 0; i < parameters.Count; i++)
            {
                if ( hasGenericParameter(parameters[i].ParameterType) || (((!ignoreValueType && parameters[i].ParameterType.IsValueType) || parameters[i].ParameterType.IsByReference) && isNoPublic(assembly, parameters[i].ParameterType)))
                {
                    return true;
                }
            }
            return false;
        }

        static bool injectType(AssemblyDefinition assembly, TypeReference hotfixAttributeType, TypeDefinition type)
        {
            foreach(var nestedTypes in type.NestedTypes)
            {
                if (!injectType(assembly, hotfixAttributeType, nestedTypes))
                {
                    return false;
                }
            }
            if (type.Name.Contains("<") || type.IsInterface || type.Methods.Count == 0) // skip anonymous type and interface
            {
                return true;
            }
            CustomAttribute hotfixAttr = type.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == hotfixAttributeType);
            HotfixFlagInTool hotfixType;
            if (hotfixAttr != null)
            {
                hotfixType = (HotfixFlagInTool)(int)hotfixAttr.ConstructorArguments[0].Value;
            }
            else
            {
                if (!hotfixCfg.ContainsKey(type.FullName))
                {
                    return true;
                }
                hotfixType = (HotfixFlagInTool)hotfixCfg[type.FullName];
            }

            bool isStateful = hotfixType.HasFlag(HotfixFlagInTool.Stateful);
            bool ignoreProperty = hotfixType.HasFlag(HotfixFlagInTool.IgnoreProperty);
            bool ignoreNotPublic = hotfixType.HasFlag(HotfixFlagInTool.IgnoreNotPublic);
            bool isInline = hotfixType.HasFlag(HotfixFlagInTool.Inline);
            bool isIntKey = hotfixType.HasFlag(HotfixFlagInTool.IntKey);
            if (isIntKey && type.HasGenericParameters)
            {
                throw new InvalidOperationException(type.FullName + " is generic definition, can not be mark as IntKey!");
            }
            //isIntKey = !type.HasGenericParameters;

            FieldReference stateTable = null;
            if (isStateful)
            {
                if (type.IsAbstract && type.IsSealed)
                {
                    throw new InvalidOperationException(type.FullName + " is static, can not be mark as Stateful!");
                }
                var stateTableDefinition = new FieldDefinition("__Hotfix_xluaStateTable", Mono.Cecil.FieldAttributes.Private, luaTableType);
                type.Fields.Add(stateTableDefinition);
                stateTable = stateTableDefinition.GetGeneric();
            }

            foreach (var method in type.Methods)
            {
                if (ignoreNotPublic && !method.IsPublic)
                {
                    continue;
                }
                if (ignoreProperty && method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                {
                    continue;
                }
                if (method.Name != ".cctor" && !method.IsAbstract && !method.IsPInvokeImpl && method.Body != null && !method.Name.Contains("<"))
                {
                    //Debug.Log(method);
                    if ((isInline || method.HasGenericParameters || genericInOut(assembly, method, hotfixType)) 
                        ? !injectGenericMethod(assembly, method, hotfixType, stateTable) :
                        !injectMethod(assembly, method, hotfixType, stateTable))
                    {
                        return false;
                    }
                }
            }

            List<MethodDefinition> toAdd = new List<MethodDefinition>();
            foreach (var method in type.Methods)
            {
                if (ignoreNotPublic && !method.IsPublic)
                {
                    continue;
                }
                if (ignoreProperty && method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                {
                    continue;
                }
                if (method.Name != ".cctor" && !method.IsAbstract && !method.IsPInvokeImpl && method.Body != null && !method.Name.Contains("<"))
                {
                    var proxyMethod = tryAddBaseProxy(type, method);
                    if (proxyMethod != null) toAdd.Add(proxyMethod);
                }
            }

            foreach(var md in toAdd)
            {
                type.Methods.Add(md);
            }

            return true;
        }

#if !XLUA_GENERAL
        [PostProcessScene]
        [UnityEditor.MenuItem("XLua/Hotfix Inject In Editor", false, 3)]
        public static void HotfixInject()
        {
            if (EditorApplication.isCompiling || Application.isPlaying)
            {
                return;
            }
            HotfixInject("./Library/ScriptAssemblies/Assembly-CSharp.dll", null, CSObjectWrapEditor.GeneratorConfig.common_path + "Resources/hotfix_id_map.lua.txt", Utils.GetAllTypes());
            AssetDatabase.Refresh();
        }
#endif
        
        public static void Config(IEnumerable<Type> cfg_check_types)
        {
            if (cfg_check_types != null)
            {
                if (hotfixCfg == null)
                {
                    hotfixCfg = new Dictionary<string, int>();
                }
                HotfixConfig.GetConfig(hotfixCfg, cfg_check_types);
            }
        }

        public static void HotfixInject(string inject_assembly_path, IEnumerable<string> search_directorys, string id_map_file_path, IEnumerable<Type> cfg_check_types = null)
        {
            AssemblyDefinition assembly = null;
            try
            {
#if HOTFIX_SYMBOLS_DISABLE
                assembly = AssemblyDefinition.ReadAssembly(inject_assembly_path);
#else
                var readerParameters = new ReaderParameters { ReadSymbols = true };
                assembly = AssemblyDefinition.ReadAssembly(inject_assembly_path, readerParameters);
#endif
                init(assembly, search_directorys);

                if (assembly.MainModule.Types.Any(t => t.Name == "__XLUA_GEN_FLAG"))
                {
                    Info("had injected!");
                    return;
                }

                assembly.MainModule.Types.Add(new TypeDefinition("__XLUA_GEN", "__XLUA_GEN_FLAG", Mono.Cecil.TypeAttributes.Class,
                    objType));

                Config(cfg_check_types);

                //var hotfixDelegateAttributeType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixDelegateAttribute");
                hotfix_bridges = (from method in delegateBridgeType.Methods
                                    where method.Name.StartsWith("__Gen_Delegate_Imp")
                                    select method).ToList();

                var hotfixAttributeType = assembly.MainModule.Types.Single(t => t.FullName == "XLua.HotfixAttribute");
                foreach (var type in (from module in assembly.Modules from type in module.Types select type))
                {
                    if (!injectType(assembly, hotfixAttributeType, type))
                    {
                        return;
                    }
                }
#if HOTFIX_SYMBOLS_DISABLE
                assembly.Write(inject_assembly_path);
                Info("hotfix inject finish!(no symbols)");
#else
                var writerParameters = new WriterParameters { WriteSymbols = true };
                assembly.Write(inject_assembly_path, writerParameters);
                Directory.CreateDirectory(Path.GetDirectoryName(id_map_file_path));
                OutputIntKeyMapper(new FileStream(id_map_file_path, FileMode.Create, FileAccess.Write));
                File.Copy(id_map_file_path, id_map_file_path + "." + DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                Info("hotfix inject finish!");
#endif
            }
            catch(Exception e)
            {
                Error("Exception! " + e);
            }
            finally
            {
                if (assembly != null)
                {
                    Clean(assembly);
                }
            }
        }

        static void Info(string info)
        {
#if XLUA_GENERAL
            System.Console.WriteLine(info);
#else
            UnityEngine.Debug.Log(info);
#endif
        }

        static void Error(string info)
        {
#if XLUA_GENERAL
            System.Console.WriteLine("Error:" + info);
#else
            UnityEngine.Debug.LogError(info);
#endif
        }

        static void Clean(AssemblyDefinition assembly)
		{
			if (assembly.MainModule.SymbolReader != null)
			{
				assembly.MainModule.SymbolReader.Dispose();
			}
		}

        static OpCode[] ldargs = new OpCode[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };

        static readonly int MAX_OVERLOAD = 100;

        static string getDelegateName(MethodDefinition method)
        {
            string fieldName = method.Name;
            if (fieldName.StartsWith("."))
            {
                fieldName = fieldName.Substring(1);
            }
            string ccFlag = method.IsConstructor ? "_c" : "";
            string luaDelegateName = null;
            var type = method.DeclaringType;
            for (int i = 0; i < MAX_OVERLOAD; i++)
            {
                string tmp = ccFlag + "__Hotfix" + i + "_" + fieldName;
                if (!type.Fields.Any(f => f.Name == tmp)) // injected
                {
                    luaDelegateName = tmp;
                    break;
                }
            }
            return luaDelegateName;
        }

        static Instruction findNextRet(Mono.Collections.Generic.Collection<Instruction> instructions, Instruction pos)
        {
            bool posFound = false;
            for(int i = 0; i < instructions.Count; i++)
            {
                if (posFound && instructions[i].OpCode == OpCodes.Ret)
                {
                    return instructions[i];
                }
                else if (instructions[i] == pos)
                {
                    posFound = true;
                }
            }
            return null;
        }

        static MethodDefinition findOverride(TypeDefinition type, MethodReference vmethod)
        {
            foreach (var method in type.Methods)
            {
                if (method.Name == vmethod.Name && method.IsVirtual && !method.IsAbstract && (method.ReturnType.FullName == vmethod.ReturnType.FullName) && method.Parameters.Count == vmethod.Parameters.Count)
                {
                    bool isParamsMatch = true;
                    for (int i = 0; i < method.Parameters.Count; i++)
                    {
                        if (method.Parameters[i].Attributes != vmethod.Parameters[i].Attributes
                            || (method.Parameters[i].ParameterType.FullName != vmethod.Parameters[i].ParameterType.FullName))
                        {
                            isParamsMatch = false;
                            break;
                        }
                    }
                    if (isParamsMatch) return method;
                }
            }
            return null;
        }

        static MethodReference _findBase(TypeReference type, MethodDefinition method)
        {
            TypeDefinition td = type.Resolve();
            if (td == null)
            {
                return null;
            }
            //if (type.IsGenericInstance && 
            //    (method.Module.Assembly.FullName != type.Module.Assembly.FullName || method.Module.Assembly.FullName != td.Module.Assembly.FullName
            //    || method.Module.FullyQualifiedName != type.Module.FullyQualifiedName || method.Module.FullyQualifiedName != td.Module.FullyQualifiedName))
            //{
            //    return _findBase(td.BaseType, method);
            //}
            var m = findOverride(td, method);
            if (m != null)
            {
                if (type.IsGenericInstance)
                {
                    var reference = new MethodReference(m.Name, tryImport(method.DeclaringType, m.ReturnType), tryImport(method.DeclaringType, type))
                    {
                        HasThis = m.HasThis,
                        ExplicitThis = m.ExplicitThis,
                        CallingConvention = m.CallingConvention
                    };
                    foreach (var parameter in m.Parameters)
                        reference.Parameters.Add(new ParameterDefinition(tryImport(method.DeclaringType, parameter.ParameterType)));
                    foreach (var generic_parameter in m.GenericParameters)
                        reference.GenericParameters.Add(new GenericParameter(generic_parameter.Name, reference));
                    return reference;
                }
                else
                {
                    return m;
                }
            }
            return _findBase(td.BaseType, method);
        }

        static MethodReference findBase(TypeDefinition type, MethodDefinition method)
        {
            if (method.IsVirtual && !method.IsNewSlot) //表明override
            {
                try
                {
                    return _findBase(type.BaseType, method);
                }
                catch { }
            }
            return null;
        }

        const string BASE_RPOXY_PERFIX = "<>xLuaBaseProxy_";

        static TypeReference tryImport(TypeReference type, TypeReference toImport)
        {
            if (type.Module.Assembly.FullName == toImport.Module.Assembly.FullName
                && type.Module.FullyQualifiedName == toImport.Module.FullyQualifiedName)
            {
                return toImport;
            }
            else
            {
                return type.Module.ImportReference(toImport);
            }
        }

        static MethodReference tryImport(TypeReference type, MethodReference toImport)
        {
            if (type.Module.Assembly.FullName == toImport.Module.Assembly.FullName
                && type.Module.FullyQualifiedName == toImport.Module.FullyQualifiedName)
            {
                return toImport;
            }
            else
            {
                return type.Module.ImportReference(toImport);
            }
        }

        static MethodDefinition tryAddBaseProxy(TypeDefinition type, MethodDefinition method)
        {
            var mbase = findBase(type, method);
            if (mbase != null)
            {
                var module = type.Module;
                var proxyMethod = new MethodDefinition(BASE_RPOXY_PERFIX + method.Name, Mono.Cecil.MethodAttributes.Public, tryImport(type, method.ReturnType));
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    proxyMethod.Parameters.Add(new ParameterDefinition("P" + i, method.Parameters[i].IsOut ? Mono.Cecil.ParameterAttributes.Out : Mono.Cecil.ParameterAttributes.None, tryImport(type, method.Parameters[i].ParameterType)));
                }
                var instructions = proxyMethod.Body.Instructions;
                var processor = proxyMethod.Body.GetILProcessor();
                int paramCount = method.Parameters.Count + 1;
                for (int i = 0; i < paramCount; i++)
                {
                    if (i < ldargs.Length)
                    {
                        instructions.Add(processor.Create(ldargs[i]));
                    }
                    else if (i < 256)
                    {
                        instructions.Add(processor.Create(OpCodes.Ldarg_S, (byte)i));
                    }
                    else
                    {
                        instructions.Add(processor.Create(OpCodes.Ldarg, (short)i));
                    }
                    if (i == 0 && type.IsValueType)
                    {
                        instructions.Add(Instruction.Create(OpCodes.Ldobj, type));
                        instructions.Add(Instruction.Create(OpCodes.Box, type));
                    }
                }
                instructions.Add(Instruction.Create(OpCodes.Call, tryImport(type, mbase)));
                instructions.Add(Instruction.Create(OpCodes.Ret));
                return proxyMethod;
            }
            return null;
        }

        static bool injectMethod(AssemblyDefinition assembly, MethodDefinition method, HotfixFlagInTool hotfixType, FieldReference stateTable)
        {
            var type = method.DeclaringType;
            
            bool isFinalize = (method.Name == "Finalize" && method.IsSpecialName);

            MethodReference invoke = null;

            int param_count = method.Parameters.Count + (method.IsStatic ? 0 : 1);

            if (!findHotfixDelegate(assembly, method, out invoke, hotfixType))
            {
                Error("can not find delegate for " + method.DeclaringType + "." + method.Name + "! try re-genertate code.");
                return false;
            }

            if (invoke == null)
            {
                throw new Exception("unknow exception!");
            }

            FieldReference fieldReference = null;
            VariableDefinition injection = null;
            bool isIntKey = hotfixType.HasFlag(HotfixFlagInTool.IntKey) && !type.HasGenericParameters;
            //isIntKey = !type.HasGenericParameters;

            if (!isIntKey)
            {
                injection = new VariableDefinition(delegateBridgeType);
                method.Body.Variables.Add(injection);

                var luaDelegateName = getDelegateName(method);
                if (luaDelegateName == null)
                {
                    Error("too many overload!");
                    return false;
                }

                FieldDefinition fieldDefinition = new FieldDefinition(luaDelegateName, Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Private,
                    delegateBridgeType);
                type.Fields.Add(fieldDefinition);
                fieldReference = fieldDefinition.GetGeneric();
            }

            bool isStateful = hotfixType.HasFlag(HotfixFlagInTool.Stateful);
            bool ignoreValueType = hotfixType.HasFlag(HotfixFlagInTool.ValueTypeBoxing);
            bool statefulConstructor = isStateful && method.IsConstructor && !method.IsStatic;

            var insertPoint = method.Body.Instructions[0];
            var processor = method.Body.GetILProcessor();

            if (method.IsConstructor)
            {
                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            while (insertPoint != null)
            {
                if (isIntKey)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldc_I4, bridgeIndexByKey.Count));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Call, hotfixFlagGetter));
                }
                else
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldsfld, fieldReference));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stloc, injection));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                }
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Brfalse, insertPoint));

                if (statefulConstructor)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_0));
                }
                if (isIntKey)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldc_I4, bridgeIndexByKey.Count));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Call, delegateBridgeGetter));
                }
                else
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                }

                for (int i = 0; i < param_count; i++)
                {
                    if (i < ldargs.Length)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(ldargs[i]));
                    }
                    else if (i < 256)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_S, (byte)i));
                    }
                    else
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg, (short)i));
                    }
                    if (i == 0 && isStateful && !method.IsStatic && !method.IsConstructor)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldfld, stateTable));
                    }
                    else if (i == 0 && !method.IsStatic && type.IsValueType)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldobj, type));
                        
                    }
                    if (ignoreValueType)
                    {
                        TypeReference paramType;
                        if (method.IsStatic)
                        {
                            paramType = method.Parameters[i].ParameterType;
                        }
                        else
                        {
                            paramType = (i == 0) ? type : method.Parameters[i - 1].ParameterType;
                        }
                        if (paramType.IsValueType)
                        {
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Box, paramType));
                        }
                    }
                }

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Call, invoke));
                if (statefulConstructor)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stfld, stateTable));
                }
                if (isFinalize && isStateful)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_0));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldnull));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stfld, stateTable));
                }
                if (!method.IsConstructor && !isFinalize)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ret));
                }

                if (!method.IsConstructor)
                {
                    break;
                }
                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            if (isFinalize)
            {
                if (method.Body.ExceptionHandlers.Count == 0)
                {
                    throw new InvalidProgramException("Finalize has not try-catch? Type :" + method.DeclaringType);
                }
                method.Body.ExceptionHandlers[0].TryStart = method.Body.Instructions[0];
            }
            if (isIntKey)
            {
                bridgeIndexByKey.Add(method);
            }
            return true;
        }

        static MethodReference MakeGenericMethod(this MethodReference self, params TypeReference[] arguments)
        {
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceMethod(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        static FieldReference GetGeneric(this FieldDefinition definition)
        {
            if (definition.DeclaringType.HasGenericParameters)
            {
                var declaringType = new GenericInstanceType(definition.DeclaringType);
                foreach (var parameter in definition.DeclaringType.GenericParameters)
                {
                    declaringType.GenericArguments.Add(parameter);
                }
                return new FieldReference(definition.Name, definition.FieldType, declaringType);
            }

            return definition;
        }

        public static TypeReference GetGeneric(this TypeDefinition definition)
        {
            if (definition.HasGenericParameters)
            {
                var genericInstanceType = new GenericInstanceType(definition);
                foreach (var parameter in definition.GenericParameters)
                {
                    genericInstanceType.GenericArguments.Add(parameter);
                }
                return genericInstanceType;
            }

            return definition;
        }

        static bool injectGenericMethod(AssemblyDefinition assembly, MethodDefinition method, HotfixFlagInTool hotfixType, FieldReference stateTable)
        {
            var type = method.DeclaringType;
            
            bool isFinalize = (method.Name == "Finalize" && method.IsSpecialName);
            bool isIntKey = hotfixType.HasFlag(HotfixFlagInTool.IntKey) && !type.HasGenericParameters;
            //isIntKey = !type.HasGenericParameters;

            FieldReference fieldReference = null;
            VariableDefinition injection = null;
            if (!isIntKey)
            {
                var luaDelegateName = getDelegateName(method);
                if (luaDelegateName == null)
                {
                    Error("too many overload!");
                    return false;
                }

                FieldDefinition fieldDefinition = new FieldDefinition(luaDelegateName, Mono.Cecil.FieldAttributes.Static | Mono.Cecil.FieldAttributes.Private,
                    delegateBridgeType);
                type.Fields.Add(fieldDefinition);

                fieldReference = fieldDefinition.GetGeneric();
            }

            injection = new VariableDefinition(delegateBridgeType);
            method.Body.Variables.Add(injection);

            int param_start = method.IsStatic ? 0 : 1;
            int param_count = method.Parameters.Count + param_start;
            var insertPoint = method.Body.Instructions[0];
            var processor = method.Body.GetILProcessor();

            if (method.IsConstructor)
            {
                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            bool isStateful = hotfixType.HasFlag(HotfixFlagInTool.Stateful);

            while (insertPoint != null)
            {
                if (isIntKey)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldc_I4, bridgeIndexByKey.Count));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Call, hotfixFlagGetter));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Brfalse, insertPoint));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldc_I4, bridgeIndexByKey.Count));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Call, delegateBridgeGetter));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stloc, injection));
                }
                else
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldsfld, fieldReference));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stloc, injection));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Brfalse, insertPoint));
                }

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, invokeSessionStart));

                bool statefulConstructor = isStateful && method.IsConstructor && !method.IsStatic;

                TypeReference returnType = statefulConstructor ? luaTableType : method.ReturnType;

                bool isVoid = returnType.FullName == "System.Void";

                int outCout = 0;

                for (int i = 0; i < param_count; i++)
                {
                    if (i == 0 && !method.IsStatic)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_0));
                        if (isStateful && !method.IsConstructor)
                        {
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldfld, stateTable));
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(inParam, luaTableType)));
                        }
                        else
                        {
                            if (type.IsValueType)
                            {
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldobj, method.DeclaringType.GetGeneric()));
                            }
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(inParam, method.DeclaringType.GetGeneric())));
                        }
                    }
                    else
                    {
                        var param = method.Parameters[i - param_start];
                        if (param.ParameterType.IsByReference)
                        {
                            outCout++;
                        }
                        if (!param.IsOut)
                        {
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));

                            if (i < ldargs.Length)
                            {
                                processor.InsertBefore(insertPoint, processor.Create(ldargs[i]));
                            }
                            else if (i < 256)
                            {
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_S, (byte)i));
                            }
                            else
                            {
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg, (short)i));
                            }

                            var paramType = param.ParameterType;

                            if (param.ParameterType.IsByReference)
                            {
                                paramType = ((ByReferenceType)paramType).ElementType;
                                if (paramType.IsValueType || paramType.IsGenericParameter)
                                {
                                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldobj, paramType));
                                }
                                else
                                {
                                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldind_Ref));
                                }
                            }
                            if (i == param_count - 1 && param.CustomAttributes.Any(ca => ca.AttributeType.FullName == "System.ParamArrayAttribute"))
                            {
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(inParams, ((ArrayType)paramType).ElementType)));
                            }
                            else
                            {
                                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(inParam, paramType)));
                            }
                        }
                    }
                }

                int outStart = (isVoid ? 0 : 1);

                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldc_I4, outCout + outStart));
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, functionInvoke));

                int outPos = outStart;
                for (int i = 0; i < method.Parameters.Count; i++)
                {
                    if (method.Parameters[i].ParameterType.IsByReference)
                    {
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldc_I4, outPos));
                        int arg_pos = param_start + i;
                        if (arg_pos < ldargs.Length)
                        {
                            processor.InsertBefore(insertPoint, processor.Create(ldargs[arg_pos]));
                        }
                        else if (arg_pos < 256)
                        {
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_S, (byte)arg_pos));
                        }
                        else
                        {
                            processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg, (short)arg_pos));
                        }
                        processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(outParam,
                            ((ByReferenceType)method.Parameters[i].ParameterType).ElementType)));
                        outPos++;
                    }
                }
                if (statefulConstructor)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_0));
                }
                processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldloc, injection));
                if (isVoid)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, invokeSessionEnd));
                }
                else
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Callvirt, MakeGenericMethod(invokeSessionEndWithResult, returnType)));
                }
                if (statefulConstructor)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stfld, stateTable));
                }
                if (isFinalize && isStateful)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldarg_0));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ldnull));
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Stfld, stateTable));
                }
                if (!method.IsConstructor && !isFinalize)
                {
                    processor.InsertBefore(insertPoint, processor.Create(OpCodes.Ret));
                }

                if (!method.IsConstructor)
                {
                    break;
                }
                insertPoint = findNextRet(method.Body.Instructions, insertPoint);
            }

            if (isFinalize)
            {
                method.Body.ExceptionHandlers[0].TryStart = method.Body.Instructions[0];
            }

            return true;
        }

        public static void OutputIntKeyMapper(Stream output)
        {
            using (StreamWriter writer = new StreamWriter(output))
            {
                writer.WriteLine("return {");
                var data = bridgeIndexByKey
                    .Select((md, idx) => new { Method = md, Index = idx})
                    .GroupBy(info => info.Method.DeclaringType)
                    .ToDictionary(group => group.Key, group =>
                    {
                        return group.GroupBy(info => info.Method.Name).ToDictionary(group_by_name => group_by_name.Key, group_by_name => group_by_name.Select(info => info.Index.ToString()).ToArray());
                    });
                foreach(var kv in data)
                {
                    writer.WriteLine("    [\"" + kv.Key.FullName.Replace('/', '+') + "\"] = {");
                    foreach(var kv2 in kv.Value)
                    {
                        writer.WriteLine("        [\"" + kv2.Key + "\"] = {");
                        writer.WriteLine("            " + string.Join(",", kv2.Value));
                        writer.WriteLine("        },");
                    }
                    writer.WriteLine("    },");
                }
                writer.WriteLine("}");
            }
        }
    }
}
#else

namespace XLua
{
    public static class Hotfix
    {
        [PostProcessScene]
        [MenuItem("XLua/Hotfix Inject In Editor", false, 3)]
        public static void HotfixInject()
        {
            if (EditorApplication.isCompiling || Application.isPlaying)
            {
                return;
            }

#if UNITY_EDITOR_OSX
			var mono_path = Path.Combine(Path.GetDirectoryName(typeof(UnityEngine.Debug).Module.FullyQualifiedName),
				"../MonoBleedingEdge/bin/mono");
			if(!File.Exists(mono_path))
			{
				mono_path = Path.Combine(Path.GetDirectoryName(typeof(UnityEngine.Debug).Module.FullyQualifiedName),
					"../../MonoBleedingEdge/bin/mono");
			}
			if(!File.Exists(mono_path))
			{
				UnityEngine.Debug.LogError("can not find mono!");
			}
#elif UNITY_EDITOR_WIN
            var mono_path = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
                "Data/MonoBleedingEdge/bin/mono.exe");
#endif
            var inject_tool_path = "./Tools/XLuaHotfixInject.exe";
            if (!File.Exists(inject_tool_path))
            {
                UnityEngine.Debug.LogError("please install the Tools");
                return;
            }

            var assembly_csharp_path = "./Library/ScriptAssemblies/Assembly-CSharp.dll";
            var id_map_file_path = CSObjectWrapEditor.GeneratorConfig.common_path + "Resources/hotfix_id_map.lua.txt";
            var hotfix_cfg_in_editor = CSObjectWrapEditor.GeneratorConfig.common_path + "hotfix_cfg_in_editor.data";

            Dictionary<string, int> editor_cfg = new Dictionary<string, int>();
            Assembly editor_assembly = typeof(Hotfix).Assembly;
            HotfixConfig.GetConfig(editor_cfg, Utils.GetAllTypes().Where(t => t.Assembly == editor_assembly));

            if (!Directory.Exists(CSObjectWrapEditor.GeneratorConfig.common_path))
            {
                Directory.CreateDirectory(CSObjectWrapEditor.GeneratorConfig.common_path);
            }
			
            using (BinaryWriter writer = new BinaryWriter(new FileStream(hotfix_cfg_in_editor, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(editor_cfg.Count);
                foreach(var kv in editor_cfg)
                {
                    writer.Write(kv.Key);
                    writer.Write(kv.Value);
                }
            }

            List<string> args = new List<string>() { inject_tool_path, assembly_csharp_path, id_map_file_path, hotfix_cfg_in_editor};

            foreach (var path in
                (from asm in AppDomain.CurrentDomain.GetAssemblies() select asm.ManifestModule.FullyQualifiedName)
                 .Distinct())
            {
                try
                {
                    args.Add(System.IO.Path.GetDirectoryName(path));
                }
                catch (Exception)
                {

                }
            }

            Process hotfix_injection = new Process();
            hotfix_injection.StartInfo.FileName = mono_path;
			hotfix_injection.StartInfo.Arguments = "\"" + String.Join("\" \"", args.Distinct().ToArray()) + "\"";
            hotfix_injection.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            hotfix_injection.StartInfo.RedirectStandardOutput = true;
            hotfix_injection.StartInfo.UseShellExecute = false;
            hotfix_injection.StartInfo.CreateNoWindow = true;
            hotfix_injection.Start();
            hotfix_injection.WaitForExit();
            File.Delete(hotfix_cfg_in_editor);
            UnityEngine.Debug.Log(hotfix_injection.StandardOutput.ReadToEnd());
            AssetDatabase.Refresh();
        }
    }
}
#endif
#endif
