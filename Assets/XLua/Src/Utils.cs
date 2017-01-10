/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;

#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

namespace XLua
{
    public static partial class Utils
    {
        public static bool LoadField(RealStatePtr L, int idx, string field_name)
        {
            LuaAPI.xlua_pushasciistring(L, field_name);
            LuaAPI.lua_rawget(L, idx);
            return !LuaAPI.lua_isnil(L, -1);
        }

        public static RealStatePtr GetMainState(RealStatePtr L)
        {
            RealStatePtr ret = default(RealStatePtr);
            LuaAPI.xlua_pushasciistring(L, "xlua_main_thread");
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            if (LuaAPI.lua_isthread(L, -1))
            {
                ret = LuaAPI.lua_tothread(L, -1);
            }
            LuaAPI.lua_pop(L, 1);
            return ret;
        }

        public static IEnumerable<Type> GetAllTypes()
        {
            return from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                          where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder)
                                          from type in assembly.GetExportedTypes()
                                          where !type.IsGenericTypeDefinition
                                          select type;
        }

        static LuaCSFunction genFieldGetter(Type type, FieldInfo field)
        {
            if (field.IsStatic)
            {
                return (RealStatePtr L) =>
                {
                    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                    translator.PushAny(L, field.GetValue(null));
                    return 1;
                };
            }
            else
            {
                return (RealStatePtr L) =>
                {
                    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                    object obj = translator.FastGetCSObj(L, 1);
                    if (obj == null || !type.IsInstanceOfType(obj))
                    {
                        return LuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while get field " + field);
                    }

                    translator.PushAny(L, field.GetValue(obj));
                    return 1;
                };
            }
        }

        static LuaCSFunction genFieldSetter(Type type, FieldInfo field)
        {
            if (field.IsStatic)
            {
                return (RealStatePtr L) =>
                {
                    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                    object val = translator.GetObject(L, 1, field.FieldType);
                    if (field.FieldType.IsValueType && val == null)
                    {
                        return LuaAPI.luaL_error(L, type.Name + "." + field.Name + " Expected type " + field.FieldType);
                    }
                    field.SetValue(null, val);
                    return 0;
                };
            }
            else
            {
                return (RealStatePtr L) =>
                {
                    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);

                    object obj = translator.FastGetCSObj(L, 1);
                    if (obj == null || !type.IsInstanceOfType(obj))
                    {
                        return LuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while set field " + field);
                    }

                    object val = translator.GetObject(L, 2, field.FieldType);
                    if (field.FieldType.IsValueType && val == null)
                    {
                        return LuaAPI.luaL_error(L, type.Name + "." + field.Name + " Expected type " + field.FieldType);
                    }
                    field.SetValue(obj, val);
                    return 0;
                };
            }
        }

        static LuaCSFunction genPropGetter(Type type, PropertyInfo prop, bool is_static)
        {
            if (is_static)
            {
                return (RealStatePtr L) =>
                {
                    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                    try
                    {
                        translator.PushAny(L, prop.GetValue(null, null));
                    }
                    catch(Exception e)
                    {
                        return LuaAPI.luaL_error(L, "try to get " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                    }
                    return 1;
                };
            }
            else
            {
                return (RealStatePtr L) =>
                {
                    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                    object obj = translator.FastGetCSObj(L, 1);
                    if (obj == null || !type.IsInstanceOfType(obj))
                    {
                        return LuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while get prop " + prop);
                    }

                    try
                    {
                        translator.PushAny(L, prop.GetValue(obj, null));
                    }
                    catch (Exception e)
                    {
                        return LuaAPI.luaL_error(L, "try to get " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                    }

                    return 1;
                };
            }
        }

        static LuaCSFunction genPropSetter(Type type, PropertyInfo prop, bool is_static)
        {
            if (is_static)
            {
                return (RealStatePtr L) =>
                {
                    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                    object val = translator.GetObject(L, 1, prop.PropertyType);
                    if (prop.PropertyType.IsValueType && val == null)
                    {
                        return LuaAPI.luaL_error(L, type.Name + "." + prop.Name + " Expected type " + prop.PropertyType);
                    }
                    try
                    { 
                        prop.SetValue(null, val, null);
                    }
                    catch (Exception e)
                    {
                        return LuaAPI.luaL_error(L, "try to set " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                    }
                    return 0;
                };
            }
            else
            {
                return (RealStatePtr L) =>
                {
                    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);

                    object obj = translator.FastGetCSObj(L, 1);
                    if (obj == null || !type.IsInstanceOfType(obj))
                    {
                        return LuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while set prop " + prop);
                    }

                    object val = translator.GetObject(L, 2, prop.PropertyType);
                    if (prop.PropertyType.IsValueType && val == null)
                    {
                        return LuaAPI.luaL_error(L, type.Name + "." + prop.Name + " Expected type " + prop.PropertyType);
                    }
                    try
                    {
                        prop.SetValue(obj, val, null);
                    }
                    catch (Exception e)
                    {
                        return LuaAPI.luaL_error(L, "try to set " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                    }
                    return 0;
                };
            }
        }

        static Dictionary<string, string> support_op = new Dictionary<string, string>()
        {
            { "op_Addition", "__add" },
            { "op_Subtraction", "__sub" },
            { "op_Multiply", "__mul" },
            { "op_Division", "__div" },
            { "op_Equality", "__eq" },
            { "op_UnaryNegation", "__unm" },
            { "op_LessThan", "__lt" },
            { "op_LessThanOrEqual", "__le" },
            { "op_Modulus", "__mod" }
        };

        static LuaCSFunction genItemGetter(Type type, PropertyInfo[] props)
        {
            Type[] params_type = new Type[props.Length];
            for(int i = 0; i < props.Length; i++)
            {
                params_type[i] = props[i].GetIndexParameters()[0].ParameterType;
            }
            object[] arg = new object[1] { null };
            return (RealStatePtr L) =>
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                object obj = translator.FastGetCSObj(L, 1);
                if (obj == null || !type.IsInstanceOfType(obj))
                {
                    return LuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while get prop " + props[0].Name);
                }

                for (int i = 0; i < props.Length; i++)
                {
                    if (!translator.Assignable(L, 2, params_type[i]))
                    {
                        continue;
                    }
                    else
                    {
                        PropertyInfo prop = props[i];
                        try
                        {
                            object index = translator.GetObject(L, 2, params_type[i]);
                            arg[0] = index;
                            object ret = prop.GetValue(obj, arg);
                            LuaAPI.lua_pushboolean(L, true);
                            translator.PushAny(L, ret);
                            return 2;
                        }
                        catch (Exception e)
                        {
                            return LuaAPI.luaL_error(L, "try to get " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                        }
                    }
                }

                LuaAPI.lua_pushboolean(L, false);
                return 1;
            };
        }

        static LuaCSFunction genItemSetter(Type type, PropertyInfo[] props)
        {
            Type[] params_type = new Type[props.Length];
            for (int i = 0; i < props.Length; i++)
            {
                params_type[i] = props[i].GetIndexParameters()[0].ParameterType;
            }
            object[] arg = new object[1] { null };
            return (RealStatePtr L) =>
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                object obj = translator.FastGetCSObj(L, 1);
                if (obj == null || !type.IsInstanceOfType(obj))
                {
                    return LuaAPI.luaL_error(L, "Expected type " + type + ", but got " + (obj == null ? "null" : obj.GetType().ToString()) + ", while set prop " + props[0].Name);
                }

                for (int i = 0; i < props.Length; i++)
                {
                    if (!translator.Assignable(L, 2, params_type[i]))
                    {
                        continue;
                    }
                    else
                    {
                        PropertyInfo prop = props[i];
                        try
                        {
                            arg[0] = translator.GetObject(L, 2, params_type[i]);
                            object val = translator.GetObject(L, 3, prop.PropertyType);
                            if (val == null)
                            {
                                return LuaAPI.luaL_error(L, type.Name + "." + prop.Name + " Expected type " + prop.PropertyType);
                            }
                            prop.SetValue(obj, val, arg);
                            LuaAPI.lua_pushboolean(L, true);
                            
                            return 1;
                        }
                        catch (Exception e)
                        {
                            return LuaAPI.luaL_error(L, "try to set " + type + "." + prop.Name + " throw a exception:" + e + ",stack:" + e.StackTrace);
                        }
                    }
                }

                LuaAPI.lua_pushboolean(L, false);
                return 1;
            };
        }

        static LuaCSFunction genEnumCastFrom(Type type)
        {
            return (RealStatePtr L) =>
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                return translator.TranslateToEnumToTop(L, type, 1);
            };
        }

        static Dictionary<Type, IEnumerable<MethodInfo>> extension_method_map = null;
        static IEnumerable<MethodInfo> GetExtensionMethodsOf(Type type_to_be_extend)
        {
            if (extension_method_map == null)
            {
                List<Type> type_def_extention_method = new List<Type>();

                IEnumerator<Type> enumerator = GetAllTypes().GetEnumerator();
                
                while(enumerator.MoveNext())
                {
                    Type type = enumerator.Current;
                    if (type.IsDefined(typeof(ExtensionAttribute), false)  && (
                            type.IsDefined(typeof(ReflectionUseAttribute), false)
#if UNITY_EDITOR
                            || type.IsDefined(typeof(LuaCallCSharpAttribute), false)
#endif
                        ))
                    {
                        type_def_extention_method.Add(type);
                    }
                    else if(!type.IsInterface && typeof(ReflectionConfig).IsAssignableFrom(type))
                    {
                        var tmp = (Activator.CreateInstance(type) as ReflectionConfig).ReflectionUse;
                        if (tmp != null)
                        {
                            type_def_extention_method.AddRange(tmp
                                .Where(t => t.IsDefined(typeof(ExtensionAttribute), false)));
                        }
                    }
#if UNITY_EDITOR
                    else if (!type.IsInterface && typeof(GenConfig).IsAssignableFrom(type))
                    {
                        var tmp = (Activator.CreateInstance(type) as GenConfig).CSharpCallLua;
                        if (tmp != null)
                        {
                            type_def_extention_method.AddRange(tmp
                            .Where(t => t.IsDefined(typeof(ExtensionAttribute), false)));
                        }
                    }
#endif
                    if (!type.IsAbstract || !type.IsSealed) continue;

                    var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var field = fields[i];
                        if ((field.IsDefined(typeof(ReflectionUseAttribute), false)
#if UNITY_EDITOR
                            || field.IsDefined(typeof(LuaCallCSharpAttribute), false)
#endif
                            ) && (typeof(IEnumerable<Type>)).IsAssignableFrom(field.FieldType))
                        {
                            type_def_extention_method.AddRange((field.GetValue(null) as IEnumerable<Type>)
                                .Where(t => t.IsDefined(typeof(ExtensionAttribute), false)));
                        }
                    }

                    var props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < props.Length; i++)
                    {
                        var prop = props[i];
                        if ((prop.IsDefined(typeof(ReflectionUseAttribute), false)
#if UNITY_EDITOR
                            || prop.IsDefined(typeof(LuaCallCSharpAttribute), false)
#endif
                            ) && (typeof(IEnumerable<Type>)).IsAssignableFrom(prop.PropertyType))
                        {
                            type_def_extention_method.AddRange((prop.GetValue(null, null) as IEnumerable<Type>)
                                .Where(t => t.IsDefined(typeof(ExtensionAttribute), false)));
                        }
                    }
                }
                enumerator.Dispose();

                extension_method_map = (from type in type_def_extention_method
                                        from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                                        where !method.ContainsGenericParameters && method.IsDefined(typeof(ExtensionAttribute), false)
                                        group method by method.GetParameters()[0].ParameterType).ToDictionary(g => g.Key, g => g as IEnumerable<MethodInfo>);
            }
            IEnumerable<MethodInfo> ret = null;
            extension_method_map.TryGetValue(type_to_be_extend, out ret);
            return ret;
        }

        struct MethodKey
        {
            public string Name;
            public bool IsStatic;
        }

        static void makeReflectionWrap(RealStatePtr L, Type type, int cls_field, int cls_getter, int cls_setter,
            int obj_field, int obj_getter, int obj_setter, int obj_meta, out LuaCSFunction item_getter, out LuaCSFunction item_setter, bool private_access = false)
        {
            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            BindingFlags flag = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | (private_access ? BindingFlags.NonPublic : BindingFlags.Public);
            FieldInfo[] fields = type.GetFields(flag);

            for (int i = 0; i < fields.Length; ++i)
            {
                FieldInfo field = fields[i];
                if (field.IsStatic && (field.IsInitOnly || field.IsLiteral))
                {
                    LuaAPI.xlua_pushasciistring(L, field.Name);
                    translator.PushAny(L, field.GetValue(null));
                    LuaAPI.lua_rawset(L, cls_field);
                }
                else
                {
                    LuaAPI.xlua_pushasciistring(L, field.Name);
                    translator.PushFixCSFunction(L, genFieldGetter(type, field));
                    LuaAPI.lua_rawset(L, field.IsStatic ? cls_getter : obj_getter);

                    LuaAPI.xlua_pushasciistring(L, field.Name);
                    translator.PushFixCSFunction(L, genFieldSetter(type, field));
                    LuaAPI.lua_rawset(L, field.IsStatic ? cls_setter : obj_setter);
                }
            }

            EventInfo[] events = type.GetEvents(flag);
            for (int i = 0; i < events.Length; ++i)
            {
                EventInfo eventInfo = events[i];
                LuaAPI.xlua_pushasciistring(L, eventInfo.Name);
                translator.PushFixCSFunction(L, translator.methodWrapsCache.GetEventWrap(type, eventInfo.Name));
                bool is_static = (eventInfo.GetAddMethod() != null) ? eventInfo.GetAddMethod().IsStatic : eventInfo.GetRemoveMethod().IsStatic;
                LuaAPI.lua_rawset(L, is_static ? cls_field : obj_field);
            }

            Dictionary<string, PropertyInfo> prop_map = new Dictionary<string, PropertyInfo>();
            List<PropertyInfo> items = new List<PropertyInfo>();
            PropertyInfo[] props = type.GetProperties(flag);
            for (int i = 0; i < props.Length; ++i)
            {
                PropertyInfo prop = props[i];
                if (prop.Name == "Item")
                {
                    items.Add(prop);
                }
                else
                {
                    prop_map.Add(prop.Name, prop);
                }
            }

            var item_array = items.ToArray();
            item_getter = item_array.Length > 0 ? genItemGetter(type, item_array) : null;
            item_setter = item_array.Length > 0 ? genItemSetter(type, item_array) : null; ;
            MethodInfo[] methods = type.GetMethods(flag);
            Dictionary<MethodKey, List<MemberInfo>> pending_methods = new Dictionary<MethodKey, List<MemberInfo>>();
            for (int i = 0; i < methods.Length; ++i)
            {
                MethodInfo method = methods[i];
                string method_name = method.Name;

                MethodKey method_key = new MethodKey { Name = method_name, IsStatic = method.IsStatic };
                List<MemberInfo> overloads;
                if (pending_methods.TryGetValue(method_key, out overloads))
                {
                    overloads.Add(method);
                    continue;
                }

                PropertyInfo prop = null;
                if (method_name.StartsWith("add_") || method_name.StartsWith("remove_")
                    || method_name == "get_Item" || method_name == "set_Item")
                {
                    continue;
                }

                if (method_name.StartsWith("op_")) // 操作符
                {
                    if (support_op.ContainsKey(method_name))
                    {
                        if (overloads == null)
                        {
                            overloads = new List<MemberInfo>();
                            pending_methods.Add(method_key, overloads);
                        }
                        overloads.Add(method);
                    }
                    continue;
                }
                else if (method_name.StartsWith("get_") && prop_map.TryGetValue(method.Name.Substring(4), out prop)) // getter of property
                {
                    LuaAPI.xlua_pushasciistring(L, prop.Name);
                    translator.PushFixCSFunction(L, genPropGetter(type, prop, method.IsStatic));
                    LuaAPI.lua_rawset(L, method.IsStatic ? cls_getter : obj_getter);
                }
                else if (method_name.StartsWith("set_") && prop_map.TryGetValue(method.Name.Substring(4), out prop)) // setter of property
                {
                    LuaAPI.xlua_pushasciistring(L, prop.Name);
                    translator.PushFixCSFunction(L, genPropSetter(type, prop, method.IsStatic));
                    LuaAPI.lua_rawset(L, method.IsStatic ? cls_setter : obj_setter);
                }
                else if (method_name == ".ctor" && method.IsConstructor)
                {
                    continue;
                }
                else
                {
                    if (overloads == null)
                    {
                        overloads = new List<MemberInfo>();
                        pending_methods.Add(method_key, overloads);
                    }
                    overloads.Add(method);
                }
            }

            foreach (var kv in pending_methods)
            {
                if (kv.Key.Name.StartsWith("op_")) // 操作符
                {
                    LuaAPI.xlua_pushasciistring(L, support_op[kv.Key.Name]);
                    translator.PushFixCSFunction(L,
                        new LuaCSFunction(translator.methodWrapsCache._GenMethodWrap(type, kv.Key.Name, kv.Value.ToArray()).Call));
                    LuaAPI.lua_rawset(L, obj_meta);
                }
                else
                {
                    LuaAPI.xlua_pushasciistring(L, kv.Key.Name);
                    translator.PushFixCSFunction(L,
                        new LuaCSFunction(translator.methodWrapsCache._GenMethodWrap(type, kv.Key.Name, kv.Value.ToArray()).Call));
                    LuaAPI.lua_rawset(L, kv.Key.IsStatic ? cls_field : obj_field);
                }
            }

            IEnumerable<MethodInfo> extend_methods = GetExtensionMethodsOf(type);
            if (extend_methods != null)
            {
                foreach (var kv in (from extend_method in extend_methods select (MemberInfo)extend_method into member group member by member.Name))
                {
                    LuaAPI.xlua_pushasciistring(L, kv.Key);
                    translator.PushFixCSFunction(L, new LuaCSFunction(translator.methodWrapsCache._GenMethodWrap(type, kv.Key, kv).Call));
                    LuaAPI.lua_rawset(L, obj_field);
                }
            }
        }

        public static void loadUpvalue(RealStatePtr L, Type type, string metafunc, int num)
        {
            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            LuaAPI.xlua_pushasciistring(L, metafunc);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            translator.Push(L, type);
            LuaAPI.lua_rawget(L, -2);
            for (int i = 1; i <= num; i++)
            {
                LuaAPI.lua_getupvalue(L, -i, i);
                if (LuaAPI.lua_isnil(L, -1))
                {
                    LuaAPI.lua_pop(L, 1);
                    LuaAPI.lua_newtable(L);
                    LuaAPI.lua_pushvalue(L, -1);
                    LuaAPI.lua_setupvalue(L, -i - 2, i);
                }
            }
            for (int i = 0; i < num; i++)
            {
                LuaAPI.lua_remove(L, -num - 1);
            }
        }

        public static void MakePrivateAccessible(RealStatePtr L, Type type)
        {
            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            int oldTop = LuaAPI.lua_gettop(L);

            LuaAPI.luaL_getmetatable(L, type.FullName);
            if (LuaAPI.lua_isnil(L, -1))
            {
                LuaAPI.lua_settop(L, oldTop);
                throw new Exception("can not find the metatable for " + type);
            }
            int obj_meta = LuaAPI.lua_gettop(L);

            LoadCSTable(L, type);
            if (LuaAPI.lua_isnil(L, -1))
            {
                LuaAPI.lua_settop(L, oldTop);
                throw new Exception("can not find the class for " + type);
            }
            int cls_field = LuaAPI.lua_gettop(L);

            loadUpvalue(L, type, Utils.LuaIndexsFieldName, 2);
            int obj_getter = LuaAPI.lua_gettop(L);
            int obj_field = obj_getter - 1;

            loadUpvalue(L, type, Utils.LuaNewIndexsFieldName, 1);
            int obj_setter = LuaAPI.lua_gettop(L);

            loadUpvalue(L, type, Utils.LuaClassIndexsFieldName, 1);
            int cls_getter = LuaAPI.lua_gettop(L);

            loadUpvalue(L, type, Utils.LuaClassNewIndexsFieldName, 1);
            int cls_setter = LuaAPI.lua_gettop(L);

            LuaCSFunction item_getter;
            LuaCSFunction item_setter;
            makeReflectionWrap(L, type, cls_field, cls_getter, cls_setter, obj_field, obj_getter, obj_setter, obj_meta,
                out item_getter, out item_setter, true);
            LuaAPI.lua_settop(L, oldTop);
        }

        public static void ReflectionWrap(RealStatePtr L, Type type)
        {
            int top_enter = LuaAPI.lua_gettop(L);
            ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            //create obj meta table
            LuaAPI.luaL_getmetatable(L, type.FullName);
            if (LuaAPI.lua_isnil(L, -1))
            {
                LuaAPI.lua_pop(L, 1);
                LuaAPI.luaL_newmetatable(L, type.FullName);
            }
            LuaAPI.lua_pushlightuserdata(L, LuaAPI.xlua_tag());
            LuaAPI.lua_pushnumber(L, 1);
            LuaAPI.lua_rawset(L, -3);
            int obj_meta = LuaAPI.lua_gettop(L);

            LuaAPI.lua_newtable(L);
            int cls_meta = LuaAPI.lua_gettop(L);

            LuaAPI.lua_newtable(L);
            int obj_field = LuaAPI.lua_gettop(L);
            LuaAPI.lua_newtable(L);
            int obj_getter = LuaAPI.lua_gettop(L);
            LuaAPI.lua_newtable(L);
            int obj_setter = LuaAPI.lua_gettop(L);
            LuaAPI.lua_newtable(L);
            int cls_field = LuaAPI.lua_gettop(L);
            LuaAPI.lua_newtable(L);
            int cls_getter = LuaAPI.lua_gettop(L);
            LuaAPI.lua_newtable(L);
            int cls_setter = LuaAPI.lua_gettop(L);

            LuaCSFunction item_getter;
            LuaCSFunction item_setter;
            makeReflectionWrap(L, type, cls_field, cls_getter, cls_setter, obj_field, obj_getter, obj_setter, obj_meta,
                out item_getter, out item_setter);

            // init obj metatable
            LuaAPI.xlua_pushasciistring(L, "__gc");
            LuaAPI.lua_pushstdcallcfunction(L, translator.metaFunctions.GcMeta);
            LuaAPI.lua_rawset(L, obj_meta);

            LuaAPI.xlua_pushasciistring(L, "__tostring");
            LuaAPI.lua_pushstdcallcfunction(L, translator.metaFunctions.ToStringMeta);
            LuaAPI.lua_rawset(L, obj_meta);

            LuaAPI.xlua_pushasciistring(L, "__index");
            LuaAPI.lua_pushvalue(L, obj_field);
            LuaAPI.lua_pushvalue(L, obj_getter);
            translator.PushFixCSFunction(L, item_getter);
            translator.PushAny(L, type.BaseType);
            LuaAPI.xlua_pushasciistring(L, Utils.LuaIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaAPI.lua_pushnil(L);
            LuaAPI.gen_obj_indexer(L);
            //store in lua indexs function tables
            LuaAPI.xlua_pushasciistring(L, Utils.LuaIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            translator.Push(L, type);
            LuaAPI.lua_pushvalue(L, -3);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.lua_pop(L, 1);
            LuaAPI.lua_rawset(L, obj_meta); // set __index

            LuaAPI.xlua_pushasciistring(L, "__newindex");
            LuaAPI.lua_pushvalue(L, obj_setter);
            translator.PushFixCSFunction(L, item_setter);
            translator.Push(L, type.BaseType);
            LuaAPI.xlua_pushasciistring(L, Utils.LuaNewIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaAPI.lua_pushnil(L);
            LuaAPI.gen_obj_newindexer(L);
            //store in lua newindexs function tables
            LuaAPI.xlua_pushasciistring(L, Utils.LuaNewIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            translator.Push(L, type);
            LuaAPI.lua_pushvalue(L, -3);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.lua_pop(L, 1);
            LuaAPI.lua_rawset(L, obj_meta); // set __newindex
            //finish init obj metatable

            LuaAPI.xlua_pushasciistring(L, "UnderlyingSystemType");
            translator.PushAny(L, type);
            LuaAPI.lua_rawset(L, cls_field);

            if (type != null && type.IsEnum)
            {
                LuaAPI.xlua_pushasciistring(L, "__CastFrom");
                translator.PushFixCSFunction(L, genEnumCastFrom(type));
                LuaAPI.lua_rawset(L, cls_field);
            }

            //set cls_field to namespace
            SetCSTable(L, type, cls_field);
            //finish set cls_field to namespace

            //init class meta
            LuaAPI.xlua_pushasciistring(L, "__index");
            LuaAPI.lua_pushvalue(L, cls_getter);
            LuaAPI.lua_pushvalue(L, cls_field);
            translator.Push(L, type.BaseType);
            LuaAPI.xlua_pushasciistring(L, Utils.LuaClassIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaAPI.gen_cls_indexer(L);
            //store in lua indexs function tables
            LuaAPI.xlua_pushasciistring(L, Utils.LuaClassIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            translator.Push(L, type);
            LuaAPI.lua_pushvalue(L, -3);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.lua_pop(L, 1);
            LuaAPI.lua_rawset(L, cls_meta); // set __index 

            LuaAPI.xlua_pushasciistring(L, "__newindex");
            LuaAPI.lua_pushvalue(L, cls_setter);
            translator.Push(L, type.BaseType);
            LuaAPI.xlua_pushasciistring(L, Utils.LuaClassNewIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaAPI.gen_cls_newindexer(L);
            //store in lua newindexs function tables
            LuaAPI.xlua_pushasciistring(L, Utils.LuaClassNewIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            translator.Push(L, type);
            LuaAPI.lua_pushvalue(L, -3);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.lua_pop(L, 1);
            LuaAPI.lua_rawset(L, cls_meta); // set __newindex

            LuaCSFunction constructor = translator.methodWrapsCache.GetConstructorWrap(type);
            if (constructor == null)
            {
                constructor = (RealStatePtr LL) =>
                {
                    return LuaAPI.luaL_error(LL, "No constructor for " + type);
                };
            }

            LuaAPI.xlua_pushasciistring(L, "__call");
            translator.PushFixCSFunction(L, constructor);
            LuaAPI.lua_rawset(L, cls_meta);

            LuaAPI.lua_pushvalue(L, cls_meta);
            LuaAPI.lua_setmetatable(L, cls_field);

            LuaAPI.lua_pop(L, 8);

            System.Diagnostics.Debug.Assert(top_enter == LuaAPI.lua_gettop(L));
        }

        //meta: -4, method:-3, getter: -2, setter: -1
        public static void BeginObjectRegister(Type type, RealStatePtr L, ObjectTranslator translator, int meta_count, int method_count, int getter_count,
            int setter_count, int type_id = -1)
        {
            if (type == null)
            {
                if (type_id == -1) throw new Exception("Fatal: must provide a type of type_id");
                LuaAPI.xlua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, type_id);
            }
            else
            {
                LuaAPI.luaL_getmetatable(L, type.FullName);
                if (LuaAPI.lua_isnil(L, -1))
                {
                    LuaAPI.lua_pop(L, 1);
                    LuaAPI.luaL_newmetatable(L, type.FullName);
                }
            }
            LuaAPI.lua_pushlightuserdata(L, LuaAPI.xlua_tag());
            LuaAPI.lua_pushnumber(L, 1);
            LuaAPI.lua_rawset(L, -3);

            if ((type == null || !translator.HasCustomOp(type)) && type != typeof(decimal))
            {
                LuaAPI.xlua_pushasciistring(L, "__gc"); 
                LuaAPI.lua_pushstdcallcfunction(L, translator.metaFunctions.GcMeta);
                LuaAPI.lua_rawset(L, -3);
            }

            LuaAPI.xlua_pushasciistring(L, "__tostring");
            LuaAPI.lua_pushstdcallcfunction(L, translator.metaFunctions.ToStringMeta);
            LuaAPI.lua_rawset(L, -3);

            if (method_count == 0)
            {
                LuaAPI.lua_pushnil(L);
            }
            else
            {
                LuaAPI.lua_createtable(L, 0, method_count);
            }

            if (getter_count == 0)
            {
                LuaAPI.lua_pushnil(L);
            }
            else
            {
                LuaAPI.lua_createtable(L, 0, getter_count);
            }

            if (setter_count == 0)
            {
                LuaAPI.lua_pushnil(L);
            }
            else
            {
                LuaAPI.lua_createtable(L, 0, setter_count);
            }
        }

        static int abs_idx(int top, int idx)
        {
            return idx > 0 ? idx : top + idx + 1;
        }

        public static readonly int OBJ_META_IDX = -4;
        public static readonly int METHOD_IDX = -3;
        public static readonly int GETTER_IDX = -2;
        public static readonly int SETTER_IDX = -1;

        public static void EndObjectRegister(Type type, RealStatePtr L, ObjectTranslator translator, LuaCSFunction csIndexer,
            LuaCSFunction csNewIndexer, Type base_type, LuaCSFunction arrayIndexer, LuaCSFunction arrayNewIndexer)
        {
            int top = LuaAPI.lua_gettop(L);
            int meta_idx = abs_idx(top, OBJ_META_IDX);
            int method_idx = abs_idx(top, METHOD_IDX);
            int getter_idx = abs_idx(top, GETTER_IDX);
            int setter_idx = abs_idx(top, SETTER_IDX);

            //begin index gen
            LuaAPI.xlua_pushasciistring(L, "__index");
            LuaAPI.lua_pushvalue(L, method_idx);
            LuaAPI.lua_pushvalue(L, getter_idx);

            if (csIndexer == null)
            {
                LuaAPI.lua_pushnil(L);
            }
            else
            {
                LuaAPI.lua_pushstdcallcfunction(L, csIndexer);
            }

            translator.Push(L, type == null ? base_type : type.BaseType);

            LuaAPI.xlua_pushasciistring(L, Utils.LuaIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            if (arrayIndexer == null)
            {
                LuaAPI.lua_pushnil(L);
            }
            else
            {
                LuaAPI.lua_pushstdcallcfunction(L, arrayIndexer);
            }

            LuaAPI.gen_obj_indexer(L);

            if (type != null)
            {
                LuaAPI.xlua_pushasciistring(L, Utils.LuaIndexsFieldName);
                LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);//store in lua indexs function tables
                translator.Push(L, type);
                LuaAPI.lua_pushvalue(L, -3);
                LuaAPI.lua_rawset(L, -3);
                LuaAPI.lua_pop(L, 1);
            }

            LuaAPI.lua_rawset(L, meta_idx);
            //end index gen

            //begin newindex gen
            LuaAPI.xlua_pushasciistring(L, "__newindex");
            LuaAPI.lua_pushvalue(L, setter_idx);

            if (csNewIndexer == null)
            {
                LuaAPI.lua_pushnil(L);
            }
            else
            {
                LuaAPI.lua_pushstdcallcfunction(L, csNewIndexer);
            }

            translator.Push(L, type == null ? base_type : type.BaseType);

            LuaAPI.xlua_pushasciistring(L, Utils.LuaNewIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);

            if (arrayNewIndexer == null)
            {
                LuaAPI.lua_pushnil(L);
            }
            else
            {
                LuaAPI.lua_pushstdcallcfunction(L, arrayNewIndexer);
            }

            LuaAPI.gen_obj_newindexer(L);

            if (type != null)
            {
                LuaAPI.xlua_pushasciistring(L, Utils.LuaNewIndexsFieldName);
                LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);//store in lua newindexs function tables
                translator.Push(L, type);
                LuaAPI.lua_pushvalue(L, -3);
                LuaAPI.lua_rawset(L, -3);
                LuaAPI.lua_pop(L, 1);
            }

            LuaAPI.lua_rawset(L, meta_idx);
            //end new index gen
            LuaAPI.lua_pop(L, 4);
        }

        public static void RegisterFunc(RealStatePtr L, int idx, string name, LuaCSFunction func)
        {
            idx = abs_idx(LuaAPI.lua_gettop(L), idx);
            LuaAPI.xlua_pushasciistring(L, name);
            LuaAPI.lua_pushstdcallcfunction(L, func);
            LuaAPI.lua_rawset(L, idx);
        }

        public static void RegisterObject(RealStatePtr L, ObjectTranslator translator, int idx, string name, object obj)
        {
            idx = abs_idx(LuaAPI.lua_gettop(L), idx);
            LuaAPI.xlua_pushasciistring(L, name);
            translator.PushAny(L, obj);
            LuaAPI.lua_rawset(L, idx);
        }

        public static void BeginClassRegister(Type type, RealStatePtr L, LuaCSFunction creator, int class_field_count,
            int static_getter_count, int static_setter_count)
        {
            LuaAPI.lua_createtable(L, 0, class_field_count);

            int cls_table = LuaAPI.lua_gettop(L);

            SetCSTable(L, type, cls_table);

            LuaAPI.lua_createtable(L, 0, 3);
            int meta_table = LuaAPI.lua_gettop(L);
            if (creator != null)
            {
                LuaAPI.xlua_pushasciistring(L, "__call");
                LuaAPI.lua_pushstdcallcfunction(L, creator);
                LuaAPI.lua_rawset(L, -3);
            }

            if (static_getter_count == 0)
            {
                LuaAPI.lua_pushnil(L);
            }
            else
            {
                LuaAPI.lua_createtable(L, 0, static_getter_count);
            }
            
            if (static_setter_count == 0)
            {
                LuaAPI.lua_pushnil(L);
            }
            else
            {
                LuaAPI.lua_createtable(L, 0, static_setter_count);
            }
            LuaAPI.lua_pushvalue(L, meta_table);
            LuaAPI.lua_setmetatable(L, cls_table);
        }

        public static readonly int CLS_IDX = -4;
        public static readonly int CLS_META_IDX = -3;
        public static readonly int CLS_GETTER_IDX = -2;
        public static readonly int CLS_SETTER_IDX = -1;

        public static void EndClassRegister(Type type, RealStatePtr L, ObjectTranslator translator)
        {
            int top = LuaAPI.lua_gettop(L);
            int cls_idx = abs_idx(top, CLS_IDX);
            int cls_getter_idx = abs_idx(top, CLS_GETTER_IDX);
            int cls_setter_idx = abs_idx(top, CLS_SETTER_IDX);
            int cls_meta_idx = abs_idx(top, CLS_META_IDX);
   
            //begin cls index
            LuaAPI.xlua_pushasciistring(L, "__index");
            LuaAPI.lua_pushvalue(L, cls_getter_idx);
            LuaAPI.lua_pushvalue(L, cls_idx);
            translator.Push(L, type.BaseType);
            LuaAPI.xlua_pushasciistring(L, Utils.LuaClassIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaAPI.gen_cls_indexer(L);

            LuaAPI.xlua_pushasciistring(L, Utils.LuaClassIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);//store in lua indexs function tables
            translator.Push(L, type);
            LuaAPI.lua_pushvalue(L, -3);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.lua_pop(L, 1);

            LuaAPI.lua_rawset(L, cls_meta_idx);
            //end cls index

            //begin cls newindex
            LuaAPI.xlua_pushasciistring(L, "__newindex");
            LuaAPI.lua_pushvalue(L, cls_setter_idx);
            translator.Push(L, type.BaseType);
            LuaAPI.xlua_pushasciistring(L, Utils.LuaClassNewIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaAPI.gen_cls_newindexer(L);

            LuaAPI.xlua_pushasciistring(L, Utils.LuaClassNewIndexsFieldName);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);//store in lua newindexs function tables
            translator.Push(L, type);
            LuaAPI.lua_pushvalue(L, -3);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.lua_pop(L, 1);

            LuaAPI.lua_rawset(L, cls_meta_idx);
            //end cls newindex

            LuaAPI.lua_pop(L, 4);
        }

        static List<string> getPathOfType(Type type)
        {
            List<string> path = new List<string>();

            if (type.Namespace != null)
            {
                path.AddRange(type.Namespace.Split(new char[] { '.' }));
            }

            string class_name = type.ToString().Substring(type.Namespace == null ? 0 : type.Namespace.Length + 1);

            if (type.IsNested)
            {
                path.AddRange(class_name.Split(new char[] { '+' }));
            }
            else
            {
                path.Add(class_name);
            }
            return path;
        }

        public static void LoadCSTable(RealStatePtr L, Type type)
        {
            int oldTop = LuaAPI.lua_gettop(L);
            LuaAPI.lua_getglobal(L, "CS");

            List<string> path = getPathOfType(type);

            for (int i = 0; i < path.Count; ++i)
            {
                LuaAPI.xlua_pushasciistring(L, path[i]);
                if (0 != LuaAPI.xlua_pgettable(L, -2))
                {
                    LuaAPI.lua_settop(L, oldTop);
                    LuaAPI.lua_pushnil(L);
                    return;
                }
                if (!LuaAPI.lua_istable(L, -1) && i < path.Count - 1)
                {
                    LuaAPI.lua_settop(L, oldTop);
                    LuaAPI.lua_pushnil(L);
                    return;
                }
                LuaAPI.lua_remove(L, -2);
            }
        }

        public static void SetCSTable(RealStatePtr L, Type type, int cls_table)
        {
            int oldTop = LuaAPI.lua_gettop(L);
            cls_table = abs_idx(oldTop, cls_table);
            LuaAPI.lua_getglobal(L, "CS");

            List<string> path = getPathOfType(type);

            for (int i = 0; i < path.Count - 1; ++i)
            {
                LuaAPI.xlua_pushasciistring(L, path[i]);
                if (0 != LuaAPI.xlua_pgettable(L, -2))
                {
                    LuaAPI.lua_settop(L, oldTop);
                    throw new Exception("SetCSTable for [" + type + "] error: " + LuaAPI.lua_tostring(L, -1));
                }
                if (LuaAPI.lua_isnil(L, -1))
                {
                    LuaAPI.lua_pop(L, 1);
                    LuaAPI.lua_createtable(L, 0, 0);
                    LuaAPI.xlua_pushasciistring(L, path[i]);
                    LuaAPI.lua_pushvalue(L, -2);
                    LuaAPI.lua_rawset(L, -4);
                }
                else if (!LuaAPI.lua_istable(L, -1))
                {
                    LuaAPI.lua_settop(L, oldTop);
                    throw new Exception("SetCSTable for [" + type + "] error: ancestors is not a table!");
                }
                LuaAPI.lua_remove(L, -2);
            }

            LuaAPI.xlua_pushasciistring(L, path[path.Count -1]);
            LuaAPI.lua_pushvalue(L, cls_table);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.lua_pop(L, 1);
        }

        public static string LuaIndexsFieldName = "LuaIndexs";

        public static string LuaNewIndexsFieldName = "LuaNewIndexs";

        public static string LuaClassIndexsFieldName = "LuaClassIndexs";

        public static string LuaClassNewIndexsFieldName = "LuaClassNewIndexs";

        public static bool IsParamsMatch(MethodInfo delegateMethod, MethodInfo bridgeMethod)
        {
            if (delegateMethod == null || bridgeMethod == null)
            {
                return false;
            }
            if (delegateMethod.ReturnType != bridgeMethod.ReturnType)
            {
                return false;
            }
            ParameterInfo[] delegateParams = delegateMethod.GetParameters();
            ParameterInfo[] bridgeParams = bridgeMethod.GetParameters();
            if (delegateParams.Length != bridgeParams.Length)
            {
                return false;
            }

            for (int i = 0; i < delegateParams.Length; i++)
            {
                if (delegateParams[i].ParameterType != bridgeParams[i].ParameterType || delegateParams[i].IsOut != bridgeParams[i].IsOut)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
