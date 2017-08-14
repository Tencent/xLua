/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#if UNITY_EDITOR || XLUA_GENERAL
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System;
using System.Linq;

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
    public class CodeEmit
    {
        private ModuleBuilder codeEmitModule = null;
        private ulong genID = 0;

        private MethodInfo LuaEnv_ThrowExceptionFromError = typeof(LuaEnv).GetMethod("ThrowExceptionFromError");
        private FieldInfo LuaBase_luaEnv = typeof(LuaBase).GetField("luaEnv", BindingFlags.NonPublic | BindingFlags.Instance);
        private MethodInfo DelegateBridgeBase_errorFuncRef_getter = typeof(LuaBase).GetProperty("_errorFuncRef", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
        private MethodInfo LuaAPI_load_error_func = typeof(LuaAPI).GetMethod("load_error_func");
        private MethodInfo LuaBase_translator_getter  = typeof(LuaBase).GetProperty("_translator", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
        private FieldInfo LuaBase_luaReference = typeof(LuaBase).GetField("luaReference", BindingFlags.NonPublic | BindingFlags.Instance);
        private MethodInfo LuaAPI_lua_getref = typeof(LuaAPI).GetMethod("lua_getref");
        private MethodInfo Type_GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });
        private MethodInfo ObjectTranslator_PushAny = typeof(ObjectTranslator).GetMethod("PushAny");
        private MethodInfo ObjectTranslator_PushParams = typeof(ObjectTranslator).GetMethod("PushParams");
        private MethodInfo LuaBase_L_getter = typeof(LuaBase).GetProperty("_L", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
        private MethodInfo LuaAPI_lua_pcall = typeof(LuaAPI).GetMethod("lua_pcall");
        private MethodInfo ObjectTranslator_GetObject = typeof(ObjectTranslator).GetMethod("GetObject", new Type[] { typeof(RealStatePtr),
               typeof(int), typeof(Type)});
        private MethodInfo LuaAPI_lua_pushvalue = typeof(LuaAPI).GetMethod("lua_pushvalue");
        private MethodInfo LuaAPI_lua_remove = typeof(LuaAPI).GetMethod("lua_remove");
        private MethodInfo LuaAPI_lua_pushstring = typeof(LuaAPI).GetMethod("lua_pushstring", new Type[] { typeof(RealStatePtr), typeof(string)});
        private MethodInfo LuaAPI_lua_gettop = typeof(LuaAPI).GetMethod("lua_gettop");
        private MethodInfo LuaAPI_xlua_pgettable = typeof(LuaAPI).GetMethod("xlua_pgettable");
        private MethodInfo LuaAPI_xlua_psettable = typeof(LuaAPI).GetMethod("xlua_psettable");
        private MethodInfo LuaAPI_lua_pop = typeof(LuaAPI).GetMethod("lua_pop");
        private MethodInfo LuaAPI_lua_settop = typeof(LuaAPI).GetMethod("lua_settop");

        private MethodInfo LuaAPI_xlua_pushinteger = typeof(LuaAPI).GetMethod("xlua_pushinteger");
        private MethodInfo LuaAPI_lua_pushint64 = typeof(LuaAPI).GetMethod("lua_pushint64");
        private MethodInfo LuaAPI_lua_pushnumber = typeof(LuaAPI).GetMethod("lua_pushnumber");
        private MethodInfo LuaAPI_xlua_pushuint = typeof(LuaAPI).GetMethod("xlua_pushuint");
        private MethodInfo LuaAPI_lua_pushuint64 = typeof(LuaAPI).GetMethod("lua_pushuint64");
        private MethodInfo LuaAPI_lua_pushboolean = typeof(LuaAPI).GetMethod("lua_pushboolean");
        private MethodInfo LuaAPI_lua_pushbytes = typeof(LuaAPI).GetMethod("lua_pushstring", new Type[] { typeof(RealStatePtr), typeof(byte[]) });
        private MethodInfo LuaAPI_lua_pushlightuserdata = typeof(LuaAPI).GetMethod("lua_pushlightuserdata");
        private MethodInfo ObjectTranslator_PushDecimal = typeof(ObjectTranslator).GetMethod("PushDecimal");

        private Dictionary<Type, MethodInfo> fixPush;

        private MethodInfo LuaAPI_xlua_tointeger = typeof(LuaAPI).GetMethod("xlua_tointeger");
        private MethodInfo LuaAPI_lua_tonumber = typeof(LuaAPI).GetMethod("lua_tonumber");
        private MethodInfo LuaAPI_lua_tostring = typeof(LuaAPI).GetMethod("lua_tostring");
        private MethodInfo LuaAPI_lua_toboolean = typeof(LuaAPI).GetMethod("lua_toboolean");
        private MethodInfo LuaAPI_lua_tobytes = typeof(LuaAPI).GetMethod("lua_tobytes");
        private MethodInfo LuaAPI_lua_touserdata = typeof(LuaAPI).GetMethod("lua_touserdata");
        private MethodInfo LuaAPI_xlua_touint = typeof(LuaAPI).GetMethod("xlua_touint");
        private MethodInfo LuaAPI_lua_touint64 = typeof(LuaAPI).GetMethod("lua_touint64");
        private MethodInfo LuaAPI_lua_toint64 = typeof(LuaAPI).GetMethod("lua_toint64");

        private Dictionary<Type, MethodInfo> typedCaster;
        private Dictionary<Type, MethodInfo> fixCaster;

        public CodeEmit()
        {
            fixPush = new Dictionary<Type, MethodInfo>()
            {
                {typeof(byte), LuaAPI_xlua_pushinteger},
                {typeof(char), LuaAPI_xlua_pushinteger},
                {typeof(short), LuaAPI_xlua_pushinteger},
                {typeof(int), LuaAPI_xlua_pushinteger},
                {typeof(long), LuaAPI_lua_pushint64},
                {typeof(sbyte), LuaAPI_xlua_pushinteger},
                {typeof(float), LuaAPI_lua_pushnumber},
                {typeof(ushort), LuaAPI_xlua_pushinteger},
                {typeof(uint), LuaAPI_xlua_pushuint},
                {typeof(ulong), LuaAPI_lua_pushuint64},
                {typeof(double), LuaAPI_lua_pushnumber},
                {typeof(string), LuaAPI_lua_pushstring},
                {typeof(byte[]), LuaAPI_lua_pushbytes},
                {typeof(bool), LuaAPI_lua_pushboolean},
                {typeof(IntPtr), LuaAPI_lua_pushlightuserdata},
            };

            fixCaster = new Dictionary<Type, MethodInfo>()
            {
                {typeof(double), LuaAPI_lua_tonumber},
                {typeof(string), LuaAPI_lua_tostring},
                {typeof(bool), LuaAPI_lua_toboolean},
                {typeof(byte[]), LuaAPI_lua_tobytes},
                {typeof(IntPtr), LuaAPI_lua_touserdata},
                {typeof(uint), LuaAPI_xlua_touint},
                {typeof(ulong), LuaAPI_lua_touint64},
                {typeof(int), LuaAPI_xlua_tointeger},
                {typeof(long), LuaAPI_lua_toint64},
            };

            typedCaster = new Dictionary<Type, MethodInfo>()
            {
                {typeof(byte), LuaAPI_xlua_tointeger},
                {typeof(char), LuaAPI_xlua_tointeger},
                {typeof(short), LuaAPI_xlua_tointeger},
                {typeof(sbyte), LuaAPI_xlua_tointeger},
                {typeof(float), LuaAPI_lua_tonumber},
                {typeof(ushort), LuaAPI_xlua_tointeger},
            };
        }

        private void genPush(ILGenerator il, Type type, short argPos, bool isParam, LocalBuilder L, LocalBuilder translator)
        {
            var paramElemType = type.IsByRef ? type.GetElementType() : type;
            MethodInfo pusher;
            if (fixPush.TryGetValue(paramElemType, out pusher))
            {
                il.Emit(OpCodes.Ldloc, L);
                il.Emit(OpCodes.Ldarg, argPos);

                if (type.IsByRef)
                {
                    if (paramElemType.IsValueType)
                    {
                        il.Emit(OpCodes.Ldobj, paramElemType);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldind_Ref);
                    }
                }

                il.Emit(OpCodes.Call, pusher);
            }
            else if (paramElemType == typeof(decimal))
            {
                il.Emit(OpCodes.Ldloc, translator);
                il.Emit(OpCodes.Ldloc, L);
                il.Emit(OpCodes.Ldarg, argPos);
                if (type.IsByRef)
                {
                    il.Emit(OpCodes.Ldobj, paramElemType);
                }
                il.Emit(OpCodes.Callvirt, ObjectTranslator_PushDecimal);
            }
            else
            {
                il.Emit(OpCodes.Ldloc, translator);
                il.Emit(OpCodes.Ldloc, L);
                il.Emit(OpCodes.Ldarg, argPos);
                if (type.IsByRef)
                {
                    if (paramElemType.IsValueType)
                    {
                        il.Emit(OpCodes.Ldobj, paramElemType);
                        il.Emit(OpCodes.Box, paramElemType);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldind_Ref);
                    }
                }
                else if (type.IsValueType)
                {
                    il.Emit(OpCodes.Box, type);
                }
                if (isParam)
                {
                    il.Emit(OpCodes.Callvirt, ObjectTranslator_PushParams);
                }
                else
                {
                    il.Emit(OpCodes.Callvirt, ObjectTranslator_PushAny);
                }
            }
        }

        private ModuleBuilder CodeEmitModule
        {
            get
            {
                if (codeEmitModule == null)
                {
                    var assemblyName = new AssemblyName();
                    assemblyName.Name = "XLuaCodeEmit";
                    codeEmitModule = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run)
                        .DefineDynamicModule("XLuaCodeEmit");
                }
                return codeEmitModule;
            }
        }

        public Type EmitDelegateImpl(IEnumerable<IGrouping<MethodInfo, Type>> groups)
        {
            TypeBuilder impl_type_builder = CodeEmitModule.DefineType("XLuaGenDelegateImpl" + (genID++), TypeAttributes.Public, typeof(DelegateBridgeBase));

            MethodBuilder get_deleate_by_type = impl_type_builder.DefineMethod("GetDelegateByType", MethodAttributes.Public
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                    | MethodAttributes.Virtual
                    | MethodAttributes.Final,
                    typeof(System.Delegate), new Type[] { typeof(System.Type) });

            ILGenerator get_deleate_by_type_il = get_deleate_by_type.GetILGenerator();

            foreach (var group in groups)
            {
                var to_be_impl = group.Key;

                var method_builder = defineImplementMethod(impl_type_builder, to_be_impl, to_be_impl.Attributes, "Invoke" + (genID++));

                genImpl(to_be_impl, method_builder.GetILGenerator(), false);

                foreach(var dt in group)
                {
                    Label end_of_if = get_deleate_by_type_il.DefineLabel();
                    get_deleate_by_type_il.Emit(OpCodes.Ldarg_1);
                    get_deleate_by_type_il.Emit(OpCodes.Ldtoken, dt);
                    get_deleate_by_type_il.Emit(OpCodes.Call, Type_GetTypeFromHandle); // typeof(type)
                    get_deleate_by_type_il.Emit(OpCodes.Bne_Un, end_of_if);

                    get_deleate_by_type_il.Emit(OpCodes.Ldarg_0);
                    get_deleate_by_type_il.Emit(OpCodes.Ldftn, method_builder);
                    get_deleate_by_type_il.Emit(OpCodes.Newobj, dt.GetConstructor(new Type[] { typeof(object), typeof(IntPtr) }));
                    get_deleate_by_type_il.Emit(OpCodes.Ret);
                    get_deleate_by_type_il.MarkLabel(end_of_if);
                }
            }

            // Constructor
            var ctor_param_types = new Type[] { typeof(int), typeof(LuaEnv) };
            ConstructorInfo parent_ctor = typeof(DelegateBridgeBase).GetConstructor(ctor_param_types);
            var ctor_builder = impl_type_builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, ctor_param_types);
            var ctor_il = ctor_builder.GetILGenerator();
            ctor_il.Emit(OpCodes.Ldarg_0);
            ctor_il.Emit(OpCodes.Ldarg_1);
            ctor_il.Emit(OpCodes.Ldarg_2);
            ctor_il.Emit(OpCodes.Call, parent_ctor);
            ctor_il.Emit(OpCodes.Ret);

            // end of GetDelegateByType
            get_deleate_by_type_il.Emit(OpCodes.Ldnull);
            get_deleate_by_type_il.Emit(OpCodes.Ret);

            impl_type_builder.DefineMethodOverride(get_deleate_by_type, typeof(DelegateBridgeBase).GetMethod("GetDelegateByType"));


            return impl_type_builder.CreateType();
        }

        private void genGetObjectCall(ILGenerator il, int offset, Type type, LocalBuilder L, LocalBuilder translator, LocalBuilder offsetBase)
        {
            if (!fixCaster.ContainsKey(type) && !typedCaster.ContainsKey(type))
            {
                il.Emit(OpCodes.Ldloc, translator); // translator
            }
            il.Emit(OpCodes.Ldloc, L); // L
            if (offsetBase != null)
            {
                il.Emit(OpCodes.Ldloc, offsetBase); // err_func
                il.Emit(OpCodes.Ldc_I4, offset);
                il.Emit(OpCodes.Add);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, offset);
            }

            MethodInfo caster;
            
            if (fixCaster.TryGetValue(type, out caster))
            {
                il.Emit(OpCodes.Call, caster);
            }
            else if (typedCaster.TryGetValue(type, out caster))
            {
                il.Emit(OpCodes.Call, caster);
                if (type == typeof(byte))
                {
                    il.Emit(OpCodes.Conv_U1);
                }
                else if(type == typeof(char))
                {
                    il.Emit(OpCodes.Conv_U2);
                }
                else if (type == typeof(short))
                {
                    il.Emit(OpCodes.Conv_I2);
                }
                else if (type == typeof(sbyte))
                {
                    il.Emit(OpCodes.Conv_I1);
                }
                else if (type == typeof(ushort))
                {
                    il.Emit(OpCodes.Conv_U2);
                }
                else if (type == typeof(float))
                {
                    il.Emit(OpCodes.Conv_R4);
                }
                else
                {
                    throw new InvalidProgramException(type + " is not a type need cast");
                }
            }
            else
            {
                il.Emit(OpCodes.Ldtoken, type);
                il.Emit(OpCodes.Call, Type_GetTypeFromHandle); // typeof(type)
                il.Emit(OpCodes.Callvirt, ObjectTranslator_GetObject);
                if (type.IsValueType)
                {
                    Label not_null = il.DefineLabel();
                    Label null_done = il.DefineLabel();
                    LocalBuilder local_new = il.DeclareLocal(type);

                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Brtrue_S, not_null);

                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Ldloca, local_new);
                    il.Emit(OpCodes.Initobj, type);
                    il.Emit(OpCodes.Ldloc, local_new);
                    il.Emit(OpCodes.Br_S, null_done);

                    il.MarkLabel(not_null);
                    il.Emit(OpCodes.Unbox_Any, type);
                    il.MarkLabel(null_done);
                }
                else if (type != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, type);
                }
            }
        }

        HashSet<Type> gen_interfaces = new HashSet<Type>();

        public void SetGenInterfaces(List<Type> gen_interfaces)
        {
            gen_interfaces.ForEach((item) =>
            {
                if (!this.gen_interfaces.Contains(item))
                {
                    this.gen_interfaces.Add(item);
                }
            });
        }

        public Type EmitInterfaceImpl(Type to_be_impl)
        {
            if (!to_be_impl.IsInterface)
            {
                throw new InvalidOperationException("interface expected, but got " + to_be_impl);
            }

            if (!gen_interfaces.Contains(to_be_impl))
            {
                throw new InvalidCastException("This type must add to CSharpCallLua: " + to_be_impl);
            }

            TypeBuilder impl_type_builder = CodeEmitModule.DefineType("XLuaGenInterfaceImpl" + (genID++), TypeAttributes.Public | TypeAttributes.Class, typeof(LuaBase), new Type[] { to_be_impl});

            foreach(var member in to_be_impl.GetMembers())
            {
                if (member.MemberType == MemberTypes.Method)
                {
                    MethodInfo method = member as MethodInfo;
                    if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_") ||
                        method.Name.StartsWith("add_") || method.Name.StartsWith("remove_"))
                    {
                        continue;
                    }

                    var method_builder = defineImplementMethod(impl_type_builder, method,
                        MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual);

                    genImpl(method, method_builder.GetILGenerator(), true);
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo property = member as PropertyInfo;
                    PropertyBuilder prop_builder = impl_type_builder.DefineProperty(property.Name, property.Attributes, property.PropertyType, Type.EmptyTypes);
                    if (property.Name == "Item")
                    {
                        if (property.CanRead)
                        {
                            var getter_buildler = defineImplementMethod(impl_type_builder, property.GetGetMethod(), 
                                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig);
                            genEmptyMethod(getter_buildler.GetILGenerator(), property.PropertyType);
                            prop_builder.SetGetMethod(getter_buildler);
                        }
                        if (property.CanWrite)
                        {
                            var setter_buildler = defineImplementMethod(impl_type_builder, property.GetSetMethod(),
                                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig);
                            genEmptyMethod(setter_buildler.GetILGenerator(), property.PropertyType);
                            prop_builder.SetSetMethod(setter_buildler);
                        }
                        continue;
                    }
                    if (property.CanRead)
                    {
                        MethodBuilder getter_buildler = impl_type_builder.DefineMethod("get_" + property.Name, 
                            MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                            property.PropertyType, Type.EmptyTypes);

                        ILGenerator il = getter_buildler.GetILGenerator();

                        LocalBuilder L = il.DeclareLocal(typeof(RealStatePtr));
                        LocalBuilder oldTop = il.DeclareLocal(typeof(int));
                        LocalBuilder translator = il.DeclareLocal(typeof(ObjectTranslator));
                        LocalBuilder ret = il.DeclareLocal(property.PropertyType);

                        // L = LuaBase.L;
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Callvirt, LuaBase_L_getter);
                        il.Emit(OpCodes.Stloc, L);

                        //oldTop = LuaAPI.lua_gettop(L);
                        il.Emit(OpCodes.Ldloc, L);
                        il.Emit(OpCodes.Call, LuaAPI_lua_gettop);
                        il.Emit(OpCodes.Stloc, oldTop);

                        //translator = LuaBase.translator;
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Callvirt, LuaBase_translator_getter);
                        il.Emit(OpCodes.Stloc, translator);

                        //LuaAPI.lua_getref(L, luaReference);
                        il.Emit(OpCodes.Ldloc, L);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, LuaBase_luaReference);
                        il.Emit(OpCodes.Call, LuaAPI_lua_getref);

                        //LuaAPI.lua_pushstring(L, "xxx");
                        il.Emit(OpCodes.Ldloc, L);
                        il.Emit(OpCodes.Ldstr, property.Name);
                        il.Emit(OpCodes.Call, LuaAPI_lua_pushstring);

                        //LuaAPI.xlua_pgettable(L, -2)
                        il.Emit(OpCodes.Ldloc, L);
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
                        il.Emit(OpCodes.Call, LuaAPI_xlua_pgettable);
                        Label gettable_no_exception = il.DefineLabel();
                        il.Emit(OpCodes.Brfalse, gettable_no_exception);

                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, LuaBase_luaEnv);
                        il.Emit(OpCodes.Ldloc, oldTop);
                        il.Emit(OpCodes.Callvirt, LuaEnv_ThrowExceptionFromError);
                        il.MarkLabel(gettable_no_exception);

                        genGetObjectCall(il, -1, property.PropertyType, L, translator, null);
                        il.Emit(OpCodes.Stloc, ret);

                        //LuaAPI.lua_pop(L, 2);
                        il.Emit(OpCodes.Ldloc, L);
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)2);
                        il.Emit(OpCodes.Call, LuaAPI_lua_pop);

                        il.Emit(OpCodes.Ldloc, ret);
                        il.Emit(OpCodes.Ret);

                        prop_builder.SetGetMethod(getter_buildler);
                    }
                    if (property.CanWrite)
                    {
                        MethodBuilder setter_builder = impl_type_builder.DefineMethod("set_" + property.Name, 
                            MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, 
                            null, new Type[] { property.PropertyType });

                        ILGenerator il = setter_builder.GetILGenerator();

                        LocalBuilder L = il.DeclareLocal(typeof(RealStatePtr));
                        LocalBuilder oldTop = il.DeclareLocal(typeof(int));
                        LocalBuilder translator = il.DeclareLocal(typeof(ObjectTranslator));

                        // L = LuaBase.L;
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Callvirt, LuaBase_L_getter);
                        il.Emit(OpCodes.Stloc, L);

                        //oldTop = LuaAPI.lua_gettop(L);
                        il.Emit(OpCodes.Ldloc, L);
                        il.Emit(OpCodes.Call, LuaAPI_lua_gettop);
                        il.Emit(OpCodes.Stloc, oldTop);

                        //translator = LuaBase.translator;
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Callvirt, LuaBase_translator_getter);
                        il.Emit(OpCodes.Stloc, translator);

                        //LuaAPI.lua_getref(L, luaReference);
                        il.Emit(OpCodes.Ldloc, L);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, LuaBase_luaReference);
                        il.Emit(OpCodes.Call, LuaAPI_lua_getref);

                        //LuaAPI.lua_pushstring(L, "xxx");
                        il.Emit(OpCodes.Ldloc, L);
                        il.Emit(OpCodes.Ldstr, property.Name);
                        il.Emit(OpCodes.Call, LuaAPI_lua_pushstring);

                        //translator.Push(L, value);
                        genPush(il, property.PropertyType, 1, false, L, translator);

                        //LuaAPI.xlua_psettable(L, -2)
                        il.Emit(OpCodes.Ldloc, L);
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)-3);
                        il.Emit(OpCodes.Call, LuaAPI_xlua_psettable);
                        Label settable_no_exception = il.DefineLabel();
                        il.Emit(OpCodes.Brfalse, settable_no_exception);

                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, LuaBase_luaEnv);
                        il.Emit(OpCodes.Ldloc, oldTop);
                        il.Emit(OpCodes.Callvirt, LuaEnv_ThrowExceptionFromError);
                        il.MarkLabel(settable_no_exception);

                        //LuaAPI.lua_pop(L, 1);
                        il.Emit(OpCodes.Ldloc, L);
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)1);
                        il.Emit(OpCodes.Call, LuaAPI_lua_pop);

                        il.Emit(OpCodes.Ret);

                        prop_builder.SetSetMethod(setter_builder);

                    }
                }
                else if(member.MemberType == MemberTypes.Event)
                {
                    
                    EventInfo event_info = member as EventInfo;
                    EventBuilder event_builder = impl_type_builder.DefineEvent(event_info.Name, event_info.Attributes, event_info.EventHandlerType);
                    if (event_info.GetAddMethod() != null)
                    {
                        var add_buildler = defineImplementMethod(impl_type_builder, event_info.GetAddMethod(),
                            MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig);
                        genEmptyMethod(add_buildler.GetILGenerator(), typeof(void));
                        event_builder.SetAddOnMethod(add_buildler);
                    }
                    if (event_info.GetRemoveMethod() != null)
                    {
                        var remove_buildler = defineImplementMethod(impl_type_builder, event_info.GetRemoveMethod(),
                            MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig);
                        genEmptyMethod(remove_buildler.GetILGenerator(), typeof(void));
                        event_builder.SetRemoveOnMethod(remove_buildler);
                    }
                }
            }
            

            // Constructor
            var ctor_param_types = new Type[] { typeof(int), typeof(LuaEnv) };
            ConstructorInfo parent_ctor = typeof(LuaBase).GetConstructor(ctor_param_types);
            var ctor_builder = impl_type_builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, ctor_param_types);
            var ctor_il = ctor_builder.GetILGenerator();
            ctor_il.Emit(OpCodes.Ldarg_0);
            ctor_il.Emit(OpCodes.Ldarg_1);
            ctor_il.Emit(OpCodes.Ldarg_2);
            ctor_il.Emit(OpCodes.Call, parent_ctor);
            ctor_il.Emit(OpCodes.Ret);

            return impl_type_builder.CreateType();
        }

        private void genEmptyMethod(ILGenerator il, Type returnType)
        {
            if(returnType != typeof(void))
            {
                if (returnType.IsValueType)
                {
                    LocalBuilder local_new = il.DeclareLocal(returnType);
                    il.Emit(OpCodes.Ldloca, local_new);
                    il.Emit(OpCodes.Initobj, returnType);
                    il.Emit(OpCodes.Ldloc, local_new);
                }
                else
                {
                    il.Emit(OpCodes.Ldnull);
                }
            }
            il.Emit(OpCodes.Ret);
        }

        private MethodBuilder defineImplementMethod(TypeBuilder type_builder, MethodInfo to_be_impl, MethodAttributes attributes, string methodName = null)
        {
            var parameters = to_be_impl.GetParameters();

            Type[] param_types = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; ++i)
            {
                param_types[i] = parameters[i].ParameterType;
            }

            var method_builder = type_builder.DefineMethod(methodName == null ? to_be_impl.Name : methodName, attributes, to_be_impl.ReturnType, param_types);
            for (int i = 0; i < parameters.Length; ++i)
            {
                method_builder.DefineParameter(i + 1, parameters[i].Attributes, parameters[i].Name);
            }
            return method_builder;
        }

        private void genImpl(MethodInfo to_be_impl, ILGenerator il, bool isObj)
        {
            var parameters = to_be_impl.GetParameters();

            LocalBuilder L = il.DeclareLocal(typeof(RealStatePtr));//RealStatePtr L;  0
            LocalBuilder err_func = il.DeclareLocal(typeof(int));//int err_func; 1
            LocalBuilder translator = il.DeclareLocal(typeof(ObjectTranslator));//ObjectTranslator translator; 2
            LocalBuilder ret = null;
            bool has_return = to_be_impl.ReturnType != typeof(void);
            if (has_return)
            {
                ret = il.DeclareLocal(to_be_impl.ReturnType); //ReturnType ret; 3
            }

            // L = LuaBase.L;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, LuaBase_L_getter);
            il.Emit(OpCodes.Stloc, L);

            //err_func =LuaAPI.load_error_func(L, errorFuncRef);
            il.Emit(OpCodes.Ldloc, L);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, DelegateBridgeBase_errorFuncRef_getter);
            il.Emit(OpCodes.Call, LuaAPI_load_error_func);
            il.Emit(OpCodes.Stloc, err_func);

            //translator = LuaBase.translator;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, LuaBase_translator_getter);
            il.Emit(OpCodes.Stloc, translator);

            //LuaAPI.lua_getref(L, luaReference);
            il.Emit(OpCodes.Ldloc, L);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, LuaBase_luaReference);
            il.Emit(OpCodes.Call, LuaAPI_lua_getref);

            if (isObj)
            {
                //LuaAPI.lua_pushstring(L, "xxx");
                il.Emit(OpCodes.Ldloc, L);
                il.Emit(OpCodes.Ldstr, to_be_impl.Name);
                il.Emit(OpCodes.Call, LuaAPI_lua_pushstring);

                //LuaAPI.xlua_pgettable(L, -2)
                il.Emit(OpCodes.Ldloc, L);
                il.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
                il.Emit(OpCodes.Call, LuaAPI_xlua_pgettable);
                Label gettable_no_exception = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, gettable_no_exception);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, LuaBase_luaEnv);
                il.Emit(OpCodes.Ldloc, err_func);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Callvirt, LuaEnv_ThrowExceptionFromError);
                il.MarkLabel(gettable_no_exception);

                //LuaAPI.lua_pushvalue(L, -2);
                il.Emit(OpCodes.Ldloc, L);
                il.Emit(OpCodes.Ldc_I4_S, (sbyte)-2);
                il.Emit(OpCodes.Call, LuaAPI_lua_pushvalue);

                //LuaAPI.lua_remove(L, -3);
                il.Emit(OpCodes.Ldloc, L);
                il.Emit(OpCodes.Ldc_I4_S, (sbyte)-3);
                il.Emit(OpCodes.Call, LuaAPI_lua_remove);
            }

            int in_param_count = 0;
            int out_param_count = 0;
            bool has_params = false;
            //translator.PushAny(L, param_in)
            for (int i = 0; i < parameters.Length; ++i)
            {
                var pinfo = parameters[i];
                if (!pinfo.IsOut)
                {
                    var ptype = pinfo.ParameterType;
                    bool isParam = pinfo.IsDefined(typeof(ParamArrayAttribute), false);
                    genPush(il, ptype, (short)(i + 1), isParam, L, translator);
                    if (isParam)
                    {
                        has_params = true;
                    }
                    else
                    {
                        ++in_param_count;
                    }
                }

                if (pinfo.ParameterType.IsByRef)
                {
                    ++out_param_count;
                }
            }

            il.Emit(OpCodes.Ldloc, L);
            il.Emit(OpCodes.Ldc_I4, in_param_count + (isObj ? 1 : 0));
            if (has_params)
            {
                Label l1 = il.DefineLabel();

                il.Emit(OpCodes.Ldarg, (short)parameters.Length);
                il.Emit(OpCodes.Brfalse, l1);

                il.Emit(OpCodes.Ldarg, (short)parameters.Length);
                il.Emit(OpCodes.Ldlen);
                il.Emit(OpCodes.Add);
                il.MarkLabel(l1);
            }
            il.Emit(OpCodes.Ldc_I4, out_param_count + (has_return ? 1 : 0));
            il.Emit(OpCodes.Ldloc, err_func);
            il.Emit(OpCodes.Call, LuaAPI_lua_pcall);
            Label no_exception = il.DefineLabel();
            il.Emit(OpCodes.Brfalse, no_exception);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, LuaBase_luaEnv);
            il.Emit(OpCodes.Ldloc, err_func);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Callvirt, LuaEnv_ThrowExceptionFromError);
            il.MarkLabel(no_exception);

            int offset = 1;
            if (has_return)
            {
                genGetObjectCall(il, offset++, to_be_impl.ReturnType, L, translator, err_func);
                il.Emit(OpCodes.Stloc, ret);
            }

            for (int i = 0; i < parameters.Length; ++i)
            {
                var pinfo = parameters[i];
                var ptype = pinfo.ParameterType;
                if (ptype.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg, (short)(i + 1));
                    var pelemtype = ptype.GetElementType();
                    genGetObjectCall(il, offset++, pelemtype, L, translator, err_func);
                    if (pelemtype.IsValueType)
                    {
                        il.Emit(OpCodes.Stobj, pelemtype);
                    }
                    else
                    {
                        il.Emit(OpCodes.Stind_Ref);
                    }

                }
            }

            if (has_return)
            {
                il.Emit(OpCodes.Ldloc, ret);
            }

            //LuaAPI.lua_settop(L, err_func - 1);
            il.Emit(OpCodes.Ldloc, L);
            il.Emit(OpCodes.Ldloc, err_func);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Call, LuaAPI_lua_settop);

            il.Emit(OpCodes.Ret);
        }
    }
}

#endif
