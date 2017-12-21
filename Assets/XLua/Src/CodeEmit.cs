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
        private MethodInfo LuaAPI_lua_type = typeof(LuaAPI).GetMethod("lua_type");
        private MethodInfo ObjectTranslator_GetObject = typeof(ObjectTranslator).GetMethod("GetObject", new Type[] { typeof(RealStatePtr),
               typeof(int), typeof(Type)});
        private MethodInfo ObjectTranslator_GetParams = typeof(ObjectTranslator).GetMethod("GetParams", new Type[] { typeof(RealStatePtr), typeof(int) });
        private MethodInfo ObjectTranslator_Update = typeof(ObjectTranslator).GetMethod("Update");
        private MethodInfo LuaAPI_lua_pushvalue = typeof(LuaAPI).GetMethod("lua_pushvalue");
        private MethodInfo LuaAPI_lua_remove = typeof(LuaAPI).GetMethod("lua_remove");
        private MethodInfo LuaAPI_lua_pushstring = typeof(LuaAPI).GetMethod("lua_pushstring", new Type[] { typeof(RealStatePtr), typeof(string)});
        private MethodInfo LuaAPI_lua_gettop = typeof(LuaAPI).GetMethod("lua_gettop");
        private MethodInfo LuaAPI_xlua_pgettable = typeof(LuaAPI).GetMethod("xlua_pgettable");
        private MethodInfo LuaAPI_xlua_psettable = typeof(LuaAPI).GetMethod("xlua_psettable");
        private MethodInfo LuaAPI_lua_pop = typeof(LuaAPI).GetMethod("lua_pop");
        private MethodInfo LuaAPI_lua_settop = typeof(LuaAPI).GetMethod("lua_settop");
        private MethodInfo LuaAPI_luaL_error = typeof(LuaAPI).GetMethod("luaL_error");
        private MethodInfo LuaAPI_xlua_is_eq_str = typeof(LuaAPI).GetMethod("xlua_is_eq_str", new Type[] {
        typeof(RealStatePtr), typeof(int), typeof(string)});

        private MethodInfo LuaAPI_xlua_pushinteger = typeof(LuaAPI).GetMethod("xlua_pushinteger");
        private MethodInfo LuaAPI_lua_pushint64 = typeof(LuaAPI).GetMethod("lua_pushint64");
        private MethodInfo LuaAPI_lua_pushnumber = typeof(LuaAPI).GetMethod("lua_pushnumber");
        private MethodInfo LuaAPI_xlua_pushuint = typeof(LuaAPI).GetMethod("xlua_pushuint");
        private MethodInfo LuaAPI_lua_pushuint64 = typeof(LuaAPI).GetMethod("lua_pushuint64");
        private MethodInfo LuaAPI_lua_pushboolean = typeof(LuaAPI).GetMethod("lua_pushboolean");
        private MethodInfo LuaAPI_lua_pushbytes = typeof(LuaAPI).GetMethod("lua_pushstring", new Type[] { typeof(RealStatePtr), typeof(byte[]) });
        private MethodInfo LuaAPI_lua_pushlightuserdata = typeof(LuaAPI).GetMethod("lua_pushlightuserdata");
        private MethodInfo ObjectTranslator_PushDecimal = typeof(ObjectTranslator).GetMethod("PushDecimal");
        private MethodInfo ObjectTranslator_GetDecimal = typeof(ObjectTranslator).GetMethod("GetDecimal");

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

        private void emitPush(ILGenerator il, Type type, short dataPos, bool isParam, LocalBuilder L, LocalBuilder translator, bool isArg)
        {
            var paramElemType = type.IsByRef ? type.GetElementType() : type;
            var ldd = isArg ? OpCodes.Ldarg : OpCodes.Ldloc;
            MethodInfo pusher;
            if (fixPush.TryGetValue(paramElemType, out pusher))
            {
                il.Emit(OpCodes.Ldloc, L);
                il.Emit(ldd, dataPos);

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
                il.Emit(ldd, dataPos);
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
                il.Emit(ldd, dataPos);
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

                emitMethodImpl(to_be_impl, method_builder.GetILGenerator(), false);

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

        private void EmitGetObject(ILGenerator il, int offset, Type type, LocalBuilder L, LocalBuilder translator, LocalBuilder offsetBase, bool isParam = false)
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
            else if (type == typeof(decimal))
            {
                il.Emit(OpCodes.Callvirt, ObjectTranslator_GetDecimal);
            }
            else
            {
                if (isParam)
                {
                    il.Emit(OpCodes.Callvirt, ObjectTranslator_GetParams.MakeGenericMethod(new Type[] { type.GetElementType() }));
                }
                else
                {
                    il.Emit(OpCodes.Ldtoken, type);
                    il.Emit(OpCodes.Call, Type_GetTypeFromHandle); // typeof(type)
                    il.Emit(OpCodes.Callvirt, ObjectTranslator_GetObject);
                }
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

            foreach(var member in (new Type[] { to_be_impl }.Concat(to_be_impl.GetInterfaces()).SelectMany(i=> i.GetMembers())))
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

                    emitMethodImpl(method, method_builder.GetILGenerator(), true);
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
                            emitEmptyMethod(getter_buildler.GetILGenerator(), property.PropertyType);
                            prop_builder.SetGetMethod(getter_buildler);
                        }
                        if (property.CanWrite)
                        {
                            var setter_buildler = defineImplementMethod(impl_type_builder, property.GetSetMethod(),
                                MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig);
                            emitEmptyMethod(setter_buildler.GetILGenerator(), property.PropertyType);
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

                        EmitGetObject(il, -1, property.PropertyType, L, translator, null);
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
                        emitPush(il, property.PropertyType, 1, false, L, translator, true);

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
                        emitEmptyMethod(add_buildler.GetILGenerator(), typeof(void));
                        event_builder.SetAddOnMethod(add_buildler);
                    }
                    if (event_info.GetRemoveMethod() != null)
                    {
                        var remove_buildler = defineImplementMethod(impl_type_builder, event_info.GetRemoveMethod(),
                            MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig);
                        emitEmptyMethod(remove_buildler.GetILGenerator(), typeof(void));
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

        private void emitEmptyMethod(ILGenerator il, Type returnType)
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

        ConstructorInfo decimalConstructor = typeof(decimal).GetConstructor(new Type[] {
            typeof(int), typeof(int), typeof(int), typeof(bool), typeof(byte)
        });

        private void emitLiteralLoad(ILGenerator il, Type type, object obj, int localIndex)
        {
            if (!type.IsValueType && ReferenceEquals(obj, null))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if(type.IsPrimitive || type.IsEnum)
            {
                if (type.IsEnum)
                {
                    type = Enum.GetUnderlyingType(type);
                }
                if (typeof(bool) == type)
                {
                    if ((bool)obj == true)
                    {
                        il.Emit(OpCodes.Ldc_I4_1);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4_0);
                    }
                }
                else if (typeof(uint) == type)
                {
                    il.Emit(OpCodes.Ldc_I4, (int)Convert.ToUInt32(obj));
                }
                else if(typeof(byte) == type || typeof(sbyte) == type || typeof(short) == type ||
                    typeof(ushort) == type || typeof(int) == type || typeof(char) == type)
                {
                    il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(obj));
                }
                else if (typeof(long) == type)
                {
                    il.Emit(OpCodes.Ldc_I8, Convert.ToInt64(obj));
                }
                else if (typeof(ulong) == type)
                {
                    il.Emit(OpCodes.Ldc_I8, (long)Convert.ToUInt64(obj));
                }
                else if (typeof(IntPtr) == type || typeof(IntPtr) == type)
                {
                    il.Emit(OpCodes.Ldloca, localIndex);
                    il.Emit(OpCodes.Initobj, type);
                    il.Emit(OpCodes.Ldloc, localIndex);
                }
                else if (typeof(float) == type)
                {
                    il.Emit(OpCodes.Ldc_R4, Convert.ToSingle(obj));
                }
                else if (typeof(double) == type)
                {
                    il.Emit(OpCodes.Ldc_R8, Convert.ToDouble(obj));
                }
                else
                {
                    throw new Exception(type + " is not primitive or enum!");
                }
            }
            else if (type == typeof(string))
            {
                il.Emit(OpCodes.Ldstr, obj as string);
            }
            else if (type == typeof(decimal))
            {
                var buffer = decimal.GetBits(Convert.ToDecimal(obj));
                il.Emit(OpCodes.Ldc_I4, buffer[0]);
                il.Emit(OpCodes.Ldc_I4, buffer[1]);
                il.Emit(OpCodes.Ldc_I4, buffer[2]);
                //UnityEngine.Debug.Log(string.Format("{0}.{1}.{2}.{3}--{4}", buffer[0], buffer[1], buffer[2], buffer[3], obj));
                il.Emit(OpCodes.Ldc_I4, (buffer[3] & 0x80000000) == 0 ? 0 : 1);
                il.Emit(OpCodes.Ldc_I4, (buffer[3] >> 16) & 0xFF);
                il.Emit(OpCodes.Newobj, decimalConstructor);
            }
            else if (type.IsValueType) 
            {
                il.Emit(OpCodes.Ldloca, localIndex);
                il.Emit(OpCodes.Initobj, type);
                il.Emit(OpCodes.Ldloc, localIndex);
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
            }
        }

        private void emitMethodImpl(MethodInfo to_be_impl, ILGenerator il, bool isObj)
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
                    emitPush(il, ptype, (short)(i + 1), isParam, L, translator, true);
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
                EmitGetObject(il, offset++, to_be_impl.ReturnType, L, translator, err_func);
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
                    EmitGetObject(il, offset++, pelemtype, L, translator, err_func);
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

        private MethodInfo ObjectTranslatorPool_FindTranslator = typeof(ObjectTranslatorPool).GetMethod("FindTranslator");
        private Type[] parameterTypeOfWrap = new Type[] { typeof(RealStatePtr) };
        private MethodInfo ObjectTranslator_Assignable = typeof(ObjectTranslator).GetMethod("Assignable", new Type[] { typeof(RealStatePtr),
               typeof(int), typeof(Type)});

        private MethodInfo Utils_BeginObjectRegister = typeof(Utils).GetMethod("BeginObjectRegister");
        private MethodInfo Utils_EndObjectRegister = typeof(Utils).GetMethod("EndObjectRegister");
        private MethodInfo Utils_BeginClassRegister = typeof(Utils).GetMethod("BeginClassRegister");
        private MethodInfo Utils_EndClassRegister = typeof(Utils).GetMethod("EndClassRegister");
        private MethodInfo Utils_RegisterFunc = typeof(Utils).GetMethod("RegisterFunc");
        private MethodInfo Utils_RegisterObject = typeof(Utils).GetMethod("RegisterObject");

        private ConstructorInfo LuaCSFunction_Constructor = typeof(LuaCSFunction).GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });

        private MethodInfo String_Concat = typeof(string).GetMethod("Concat", new Type[] { typeof(object), typeof(object) });

        void checkType(ILGenerator il, Type type, LocalBuilder translator, int argPos, Label endOfBlock, bool isVParam, bool isDefault)
        {
            Label endOfCheckType = il.DefineLabel();

            if (isVParam || isDefault)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, argPos);
                il.Emit(OpCodes.Call, LuaAPI_lua_type);
                il.Emit(OpCodes.Ldc_I4_M1);
                il.Emit(OpCodes.Beq, endOfCheckType);
            }

            il.Emit(OpCodes.Ldloc, translator);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, argPos);
            il.Emit(OpCodes.Ldtoken, isVParam ? type.GetElementType() : type);
            il.Emit(OpCodes.Call, Type_GetTypeFromHandle); // typeof(DeclaringType)
            il.Emit(OpCodes.Callvirt, ObjectTranslator_Assignable);
            il.Emit(OpCodes.Brfalse, endOfBlock);

            il.MarkLabel(endOfCheckType);
        }

        void emitRegisterFunc(ILGenerator il, MethodBuilder method, int index, string name)
        {
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, index);
            il.Emit(OpCodes.Ldstr, name);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldftn, method);
            il.Emit(OpCodes.Newobj, LuaCSFunction_Constructor);
            il.Emit(OpCodes.Call, Utils_RegisterFunc);
        }

        void emitCatchBlock(ILGenerator il, LocalBuilder ex, LocalBuilder wrapRet, Label retPoint, Label exceptionBlock)
        {
            il.BeginCatchBlock(typeof(Exception));
            il.Emit(OpCodes.Stloc, ex);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, "c# exception:");
            il.Emit(OpCodes.Ldloc, ex);
            il.Emit(OpCodes.Call, String_Concat);
            il.Emit(OpCodes.Call, LuaAPI_luaL_error);
            il.Emit(OpCodes.Stloc, wrapRet);
            il.Emit(OpCodes.Leave, retPoint);
            il.Emit(OpCodes.Leave, exceptionBlock);

            il.EndExceptionBlock();
        }

        public MethodBuilder emitFieldWrap(TypeBuilder typeBuilder, FieldInfo field, bool genGetter)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(field.Name + (genID++), MethodAttributes.Static, typeof(int), parameterTypeOfWrap);
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "L");

            ILGenerator il = methodBuilder.GetILGenerator();

            LocalBuilder L = il.DeclareLocal(typeof(RealStatePtr));
            LocalBuilder translator = il.DeclareLocal(typeof(ObjectTranslator));
            LocalBuilder fieldStore = il.DeclareLocal(field.FieldType);
            LocalBuilder wrapRet = il.DeclareLocal(typeof(int));
            LocalBuilder ex = il.DeclareLocal(typeof(Exception));

            Label exceptionBlock = il.BeginExceptionBlock();
            Label retPoint = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Stloc, L);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ObjectTranslatorPool_FindTranslator);
            il.Emit(OpCodes.Stloc, translator);

            if (genGetter)
            {
                if (!field.IsStatic)
                {
                    EmitGetObject(il, 1, field.DeclaringType, L, translator, null);
                    il.Emit(OpCodes.Ldfld, field);
                }
                else
                {
                    il.Emit(OpCodes.Ldsfld, field);
                }
                il.Emit(OpCodes.Stloc, fieldStore);
                emitPush(il, field.FieldType, 2, false, L, translator, false);
            }
            else
            {
                if (!field.IsStatic)
                {
                    EmitGetObject(il, 1, field.DeclaringType, L, translator, null);
                    LocalBuilder self = null;
                    if (field.DeclaringType.IsValueType)
                    {
                        self = il.DeclareLocal(field.DeclaringType);
                        il.Emit(OpCodes.Stloc, self);
                        il.Emit(OpCodes.Ldloca, self);
                    }
                    EmitGetObject(il, 2, field.FieldType, L, translator, null);
                    il.Emit(OpCodes.Stfld, field);
                    if (self != null)
                    {
                        emitUpdateIfNeeded(il, L, translator, field.DeclaringType, 1, self.LocalIndex);
                    }
                }
                else
                {
                    EmitGetObject(il, 1, field.FieldType, L, translator, null);
                    il.Emit(OpCodes.Stsfld, field);
                }
            }

            il.Emit(OpCodes.Leave, exceptionBlock);

            emitCatchBlock(il, ex, wrapRet, retPoint, exceptionBlock);

            il.Emit(genGetter ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(retPoint);
            il.Emit(OpCodes.Ldloc, wrapRet);
            il.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        public MethodBuilder emitPropertyWrap(TypeBuilder typeBuilder, PropertyInfo prop, MethodInfo op, bool genGetter)
        {
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(prop.Name + (genID++), MethodAttributes.Static, typeof(int), parameterTypeOfWrap);
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "L");

            ILGenerator il = methodBuilder.GetILGenerator();

            LocalBuilder L = il.DeclareLocal(typeof(RealStatePtr));
            LocalBuilder translator = il.DeclareLocal(typeof(ObjectTranslator));
            LocalBuilder propStore = il.DeclareLocal(prop.PropertyType);
            LocalBuilder wrapRet = il.DeclareLocal(typeof(int));
            LocalBuilder ex = il.DeclareLocal(typeof(Exception));

            Label exceptionBlock = il.BeginExceptionBlock();
            Label retPoint = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Stloc, L);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ObjectTranslatorPool_FindTranslator);
            il.Emit(OpCodes.Stloc, translator);

            if (genGetter)
            {
                if (!op.IsStatic)
                {
                    EmitGetObject(il, 1, prop.DeclaringType, L, translator, null);
                    if (prop.DeclaringType.IsValueType)
                    {
                        var self = il.DeclareLocal(prop.DeclaringType);
                        il.Emit(OpCodes.Stloc, self);
                        il.Emit(OpCodes.Ldloca, self);
                        il.Emit(OpCodes.Call, op);
                        emitUpdateIfNeeded(il, L, translator, prop.DeclaringType, 1, self.LocalIndex);
                    }
                    else
                    {
                        il.Emit(OpCodes.Callvirt, op);
                    }
                }
                else
                {
                    il.Emit(OpCodes.Call, op);
                }
                il.Emit(OpCodes.Stloc, propStore);
                emitPush(il, prop.PropertyType, (short)propStore.LocalIndex, false, L, translator, false);
            }
            else
            {
                if (!op.IsStatic)
                {
                    EmitGetObject(il, 1, prop.DeclaringType, L, translator, null);
                    LocalBuilder self = null;
                    if (prop.DeclaringType.IsValueType)
                    {
                        self = il.DeclareLocal(prop.DeclaringType);
                        il.Emit(OpCodes.Stloc, self);
                        il.Emit(OpCodes.Ldloca, self);
                    }
                    EmitGetObject(il, 2, prop.PropertyType, L, translator, null);
                    il.Emit(prop.DeclaringType.IsValueType ? OpCodes.Call : OpCodes.Callvirt, op);
                    if (self != null)
                    {
                        emitUpdateIfNeeded(il, L, translator, prop.DeclaringType, 1, self.LocalIndex);
                    }
                }
                else
                {
                    EmitGetObject(il, 1, prop.PropertyType, L, translator, null);
                    il.Emit(OpCodes.Call, op);
                }
            }

            il.Emit(OpCodes.Leave, exceptionBlock);

            emitCatchBlock(il, ex, wrapRet, retPoint, exceptionBlock);

            il.Emit(genGetter ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(retPoint);
            il.Emit(OpCodes.Ldloc, wrapRet);
            il.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        public Type EmitTypeWrap(Type toBeWrap)
        {
            TypeBuilder wrapTypeBuilder = CodeEmitModule.DefineType(toBeWrap.Name + "Wrap" + (genID++), TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);

            var methodBuilder = wrapTypeBuilder.DefineMethod("__Register", MethodAttributes.Static | MethodAttributes.Public, null, parameterTypeOfWrap);
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "L");

            ILGenerator il = methodBuilder.GetILGenerator();

            LocalBuilder translator = il.DeclareLocal(typeof(ObjectTranslator));

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ObjectTranslatorPool_FindTranslator);
            il.Emit(OpCodes.Stloc, translator);

            var instanceFlag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var staticFlag = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

            var instanceFields = toBeWrap.GetFields(instanceFlag);
            var instanceProperties = toBeWrap.GetProperties(instanceFlag);
            var extensionMethods = Utils.GetExtensionMethodsOf(toBeWrap);
            var instanceMethods = toBeWrap.GetMethods(instanceFlag)
                .Concat(extensionMethods == null ? Enumerable.Empty<MethodInfo>() : Utils.GetExtensionMethodsOf(toBeWrap))
                .Where(m => Utils.IsSupportedMethod(m))
                .Where(m => !m.IsSpecialName).GroupBy(m => m.Name).ToList();
            var supportOperators = toBeWrap.GetMethods(staticFlag)
                .Where(m => m.IsSpecialName && InternalGlobals.supportOp.ContainsKey(m.Name))
                .GroupBy(m => m.Name);

            //begin obj
            il.Emit(OpCodes.Ldtoken, toBeWrap);
            il.Emit(OpCodes.Call, Type_GetTypeFromHandle); // typeof(type)
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc, translator);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldc_I4_M1);
            il.Emit(OpCodes.Call, Utils_BeginObjectRegister);

            foreach(var field in instanceFields)
            {
                emitRegisterFunc(il, emitFieldWrap(wrapTypeBuilder, field, true), Utils.GETTER_IDX, field.Name);
                emitRegisterFunc(il, emitFieldWrap(wrapTypeBuilder, field, false), Utils.SETTER_IDX, field.Name);
            }

            List<MethodBase> itemGetter = new List<MethodBase>();
            List<MethodBase> itemSetter = new List<MethodBase>();

            foreach(var prop in instanceProperties)
            {
                var getter = prop.GetGetMethod();
                if (getter != null && getter.IsPublic)
                {
                    if (prop.Name == "Item" && prop.GetIndexParameters().Length > 0)
                    {
                        if (!prop.GetIndexParameters()[0].ParameterType.IsAssignableFrom(typeof(string)))
                        {
                            itemGetter.Add(getter);
                        }
                    }
                    else
                    {
                        emitRegisterFunc(il, emitPropertyWrap(wrapTypeBuilder, prop, getter, true), Utils.GETTER_IDX, prop.Name);
                    }
                }

                var setter = prop.GetSetMethod();
                if (setter != null && setter.IsPublic)
                {
                    if (prop.Name == "Item" && prop.GetIndexParameters().Length > 0)
                    {
                        if (!prop.GetIndexParameters()[0].ParameterType.IsAssignableFrom(typeof(string)))
                        {
                            itemSetter.Add(setter);
                        }
                    }
                    else
                    {
                        emitRegisterFunc(il, emitPropertyWrap(wrapTypeBuilder, prop, setter, false), Utils.SETTER_IDX, prop.Name);
                    }
                }
            }

            foreach (var group in instanceMethods)
            {
                emitRegisterFunc(il, emitMethodWrap(wrapTypeBuilder, group.Cast<MethodBase>().ToList(), false, toBeWrap), Utils.METHOD_IDX, group.Key);
            }

            foreach (var group in supportOperators)
            {
                emitRegisterFunc(il, emitMethodWrap(wrapTypeBuilder, group.Cast<MethodBase>().ToList(), false, toBeWrap), Utils.OBJ_META_IDX, InternalGlobals.supportOp[group.Key]);
            }

            foreach (var ev in toBeWrap.GetEvents(instanceFlag))
            {
                emitRegisterFunc(il, emitEventWrap(wrapTypeBuilder, ev), Utils.METHOD_IDX, ev.Name);
            }

            //end obj
            il.Emit(OpCodes.Ldtoken, toBeWrap);
            il.Emit(OpCodes.Call, Type_GetTypeFromHandle); // typeof(type)
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc, translator);
            il.Emit(OpCodes.Ldnull);
            if (itemGetter.Count > 0)
            {
                il.Emit(OpCodes.Ldftn, emitMethodWrap(wrapTypeBuilder, itemGetter, true, toBeWrap));
                il.Emit(OpCodes.Newobj, LuaCSFunction_Constructor);
            }
            il.Emit(OpCodes.Ldnull);
            if (itemSetter.Count > 0)
            {
                il.Emit(OpCodes.Ldftn, emitMethodWrap(wrapTypeBuilder, itemSetter, true, toBeWrap));
                il.Emit(OpCodes.Newobj, LuaCSFunction_Constructor);
            }
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Call, Utils_EndObjectRegister);

            // begin class
            il.Emit(OpCodes.Ldtoken, toBeWrap);
            il.Emit(OpCodes.Call, Type_GetTypeFromHandle); // typeof(type)
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldftn, emitMethodWrap(wrapTypeBuilder,
                toBeWrap.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                .Cast<MethodBase>().ToList(), false, toBeWrap, toBeWrap.ToString() + " constructor"));
            il.Emit(OpCodes.Newobj, LuaCSFunction_Constructor);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Call, Utils_BeginClassRegister);

            var staticMethods = toBeWrap.GetMethods(staticFlag)
                .Where(m => Utils.IsSupportedMethod(m))
                .Where(m => !m.IsSpecialName).GroupBy(m => m.Name);

            var staticFields = toBeWrap.GetFields(staticFlag);

            var staticProperties = toBeWrap.GetProperties(staticFlag);

            foreach (var group in staticMethods)
            {
                emitRegisterFunc(il, emitMethodWrap(wrapTypeBuilder, group.Cast<MethodBase>().ToList(), false, toBeWrap), Utils.CLS_IDX, group.Key);
            }

            foreach (var ev in toBeWrap.GetEvents(staticFlag))
            {
                emitRegisterFunc(il, emitEventWrap(wrapTypeBuilder, ev), Utils.CLS_IDX, ev.Name);
            }

            foreach (var prop in staticProperties)
            {
                var getter = prop.GetGetMethod();
                if (getter != null && getter.IsPublic)
                {
                    emitRegisterFunc(il, emitPropertyWrap(wrapTypeBuilder, prop, getter, true), Utils.GETTER_IDX, prop.Name);
                }

                var setter = prop.GetSetMethod();
                if (setter != null && setter.IsPublic)
                {
                    emitRegisterFunc(il, emitPropertyWrap(wrapTypeBuilder, prop, setter, false), Utils.SETTER_IDX, prop.Name);
                }
            }

            foreach (var field in staticFields)
            {
                if (field.IsInitOnly || field.IsLiteral)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldloc, translator);
                    il.Emit(OpCodes.Ldc_I4, Utils.CLS_IDX);
                    il.Emit(OpCodes.Ldstr, field.Name);
                    if (field.IsLiteral)
                    {
                        LocalBuilder literalStore = il.DeclareLocal(field.FieldType);
                        emitLiteralLoad(il, field.FieldType, field.GetValue(null), literalStore.LocalIndex);
                        il.Emit(OpCodes.Stloc, literalStore);
                        il.Emit(OpCodes.Ldloc, literalStore);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldsfld, field);
                    }
                    if (field.FieldType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, field.FieldType);
                    }
                    il.Emit(OpCodes.Call, Utils_RegisterObject);
                }
                else
                {
                    emitRegisterFunc(il, emitFieldWrap(wrapTypeBuilder, field, true), Utils.CLS_GETTER_IDX, field.Name);
                    emitRegisterFunc(il, emitFieldWrap(wrapTypeBuilder, field, false), Utils.CLS_SETTER_IDX, field.Name);
                }
            }

            //end class
            il.Emit(OpCodes.Ldtoken, toBeWrap);
            il.Emit(OpCodes.Call, Type_GetTypeFromHandle); // typeof(type)
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldloc, translator);
            il.Emit(OpCodes.Call, Utils_EndClassRegister);

            il.Emit(OpCodes.Ret);

            return wrapTypeBuilder.CreateType();
        }

        MethodBuilder emitEventWrap(TypeBuilder typeBuilder, EventInfo ev)
        {
            var addEvent = ev.GetAddMethod();
            var removeEvent = ev.GetRemoveMethod();

            if (addEvent == null && removeEvent == null)
            {
                return null;
            }

            bool isStatic = addEvent != null ? addEvent.IsStatic : removeEvent.IsStatic;

            var methodBuilder = typeBuilder.DefineMethod(ev.Name + (genID++), MethodAttributes.Static, typeof(int), parameterTypeOfWrap);
            methodBuilder.DefineParameter(1, ParameterAttributes.None, "L");

            ILGenerator il = methodBuilder.GetILGenerator();

            LocalBuilder wrapRet = il.DeclareLocal(typeof(int));
            LocalBuilder ex = il.DeclareLocal(typeof(Exception));
            LocalBuilder L = il.DeclareLocal(typeof(RealStatePtr));
            LocalBuilder translator = il.DeclareLocal(typeof(ObjectTranslator));
            LocalBuilder callback = il.DeclareLocal(ev.EventHandlerType);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Stloc, L);

            Label exceptionBlock = il.BeginExceptionBlock();
            Label retPoint = il.DefineLabel();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ObjectTranslatorPool_FindTranslator);
            il.Emit(OpCodes.Stloc, translator);

            EmitGetObject(il, isStatic ? 2 : 3, ev.EventHandlerType, L, translator, null);
            il.Emit(OpCodes.Stloc, callback);
            il.Emit(OpCodes.Ldloc, callback);
            Label ifBlock = il.DefineLabel();
            il.Emit(OpCodes.Brtrue, ifBlock);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, string.Format("#{0}, need {1}", isStatic ? 2 : 3, ev.EventHandlerType));
            il.Emit(OpCodes.Call, LuaAPI_luaL_error);
            il.Emit(OpCodes.Stloc, wrapRet);
            il.Emit(OpCodes.Leave, retPoint);
            il.MarkLabel(ifBlock);

            if (addEvent != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(isStatic ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_2);
                il.Emit(OpCodes.Ldstr, "+");
                il.Emit(OpCodes.Call, LuaAPI_xlua_is_eq_str);
                ifBlock = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, ifBlock);

                if (!isStatic)
                {
                    EmitGetObject(il, 1, ev.DeclaringType, L, translator, null);
                    if (ev.DeclaringType.IsValueType)
                    {
                        var self = il.DeclareLocal(ev.DeclaringType);
                        il.Emit(OpCodes.Stloc, self);
                        il.Emit(OpCodes.Ldloca, self);
                    }
                }
                il.Emit(OpCodes.Ldloc, callback);
                il.Emit(OpCodes.Call, addEvent);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Leave, retPoint);
                il.MarkLabel(ifBlock);
            }

            if (removeEvent != null)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(isStatic ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_2);
                il.Emit(OpCodes.Ldstr, "-");
                il.Emit(OpCodes.Call, LuaAPI_xlua_is_eq_str);
                ifBlock = il.DefineLabel();
                il.Emit(OpCodes.Brfalse, ifBlock);

                if (!isStatic)
                {
                    EmitGetObject(il, 1, ev.DeclaringType, L, translator, null);
                    if (ev.DeclaringType.IsValueType)
                    {
                        var self = il.DeclareLocal(ev.DeclaringType);
                        il.Emit(OpCodes.Stloc, self);
                        il.Emit(OpCodes.Ldloca, self);
                    }
                }
                il.Emit(OpCodes.Ldloc, callback);
                il.Emit(OpCodes.Call, removeEvent);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Leave, retPoint);
                il.MarkLabel(ifBlock);
            }

            il.Emit(OpCodes.Leave, exceptionBlock);

            emitCatchBlock(il, ex, wrapRet, retPoint, exceptionBlock);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldstr, "invalid arguments to " + ev.DeclaringType + "." + ev.Name + "!");
            il.Emit(OpCodes.Call, LuaAPI_luaL_error);
            il.Emit(OpCodes.Ret);

            il.MarkLabel(retPoint);
            il.Emit(OpCodes.Ldloc, wrapRet);
            il.Emit(OpCodes.Ret);

            return methodBuilder;
        }

        void emitUpdateIfNeeded(ILGenerator il, LocalBuilder L, LocalBuilder translator, Type type, int luaIndex, int localIndex)
        {
            if (type.IsValueType && !type.IsPrimitive && !type.IsEnum && type != typeof(decimal))
            {
                //UnityEngine.Debug.LogWarning("-----------------emit update:" + type);
                il.Emit(OpCodes.Ldloc, translator);
                il.Emit(OpCodes.Ldloc, L);
                il.Emit(OpCodes.Ldc_I4, luaIndex);
                il.Emit(OpCodes.Ldloc, localIndex);
                il.Emit(OpCodes.Box, type);
                il.Emit(OpCodes.Callvirt, ObjectTranslator_Update);
            }
        }

        //private MethodInfo UnityEngine_Debug_Log = typeof(UnityEngine.Debug).GetMethod("Log", new Type[] { typeof(object)});
        int firstDefaultValue(MethodBase method)
        {
            var parameters = method.GetParameters();
            for(int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsOptional) return i;
            }
            return -1;
        }

        MethodBuilder emitMethodWrap(TypeBuilder typeBuilder, List<MethodBase> methodsToCall, bool isIndexer, Type declaringType, string methodDesciption = null)
        {
            string wrapName = (methodsToCall.Count > 0 ? methodsToCall[0].Name : "Constructor");
            var methodBuilder = typeBuilder.DefineMethod(wrapName + (genID++), MethodAttributes.Static, typeof(int), parameterTypeOfWrap);
            methodBuilder.DefineParameter(1,  ParameterAttributes.None, "L");

            bool needCheckParameterType = (methodsToCall.Count > 1)  || isIndexer;

            if (methodsToCall.Count == 0 || methodsToCall[0].IsConstructor)
            {
                needCheckParameterType = true;
            }

            if (methodsToCall.Count == 1 && firstDefaultValue(methodsToCall[0]) != -1)
            {
                needCheckParameterType = true;
            }

            ILGenerator il = methodBuilder.GetILGenerator();

            LocalBuilder wrapRet = il.DeclareLocal(typeof(int));
            LocalBuilder ex = il.DeclareLocal(typeof(Exception));
            LocalBuilder L = il.DeclareLocal(typeof(RealStatePtr));
            LocalBuilder translator = il.DeclareLocal(typeof(ObjectTranslator));
            LocalBuilder top = il.DeclareLocal(typeof(int));

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Stloc, L);

            Label exceptionBlock = il.BeginExceptionBlock();
            Label retPoint = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ObjectTranslatorPool_FindTranslator);
            il.Emit(OpCodes.Stloc, translator);

            if (needCheckParameterType)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, LuaAPI_lua_gettop);
                il.Emit(OpCodes.Stloc, top);
            }

            for (int i = 0; i < methodsToCall.Count; i++)
            {
                var method = methodsToCall[i];
                if ((method is MethodInfo) && method.ContainsGenericParameters)
                {
                    method = Utils.MakeGenericMethodWithConstraints(method as MethodInfo);
                }
                bool isStatic = method.IsStatic;
                var paramInfos = method.GetParameters();
                int minInParamCount = 0;
                int maxInParamCount = 0;
                int outParamCount = 0;
                bool hasParams = paramInfos.Length > 0 && paramInfos[paramInfos.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);
                bool hasOptional = false;

                LocalBuilder methodReturn = null;

                for (int j = 0; j < paramInfos.Length; j++)
                {
                    if (!paramInfos[j].IsOut)
                    {
                        if (!paramInfos[j].IsOptional && (!hasParams || j != paramInfos.Length - 1))
                        {
                            minInParamCount++;
                        }
                        maxInParamCount++;
                    }
                    if (paramInfos[j].IsOptional)
                    {
                        hasOptional = true;
                    }
                    if (paramInfos[j].ParameterType.IsByRef)
                    {
                        outParamCount++;
                    }
                }

                Label endOfBlock = il.DefineLabel();

                if (needCheckParameterType)
                {
                    il.Emit(OpCodes.Ldloc, top);
                    il.Emit(OpCodes.Ldc_I4, minInParamCount + (isStatic ? 0 : 1));
                    il.Emit((hasParams || hasOptional) ? OpCodes.Blt : OpCodes.Bne_Un, endOfBlock);

                    if (hasOptional && !hasParams)
                    {
                        il.Emit(OpCodes.Ldloc, top);
                        il.Emit(OpCodes.Ldc_I4, maxInParamCount + (isStatic ? 0 : 1));
                        il.Emit(OpCodes.Bgt, endOfBlock);
                    }
                    
                    if (!isStatic && !method.IsConstructor)
                    {
                        checkType(il, method.DeclaringType, translator, 1, endOfBlock, false, false);
                    }

                    int argPos = isStatic ? 1 : 2;

                    for (int j = 0; j < paramInfos.Length; j++)
                    {
                        var paramInfo = paramInfos[j];
                        if (!paramInfo.IsOut)
                        {
                            var rawParamType = paramInfo.ParameterType;
                            if (rawParamType.IsByRef)
                            {
                                rawParamType = rawParamType.GetElementType();
                            }
                            checkType(il, rawParamType, translator, argPos++, endOfBlock, 
                                hasParams && (j == paramInfos.Length - 1), paramInfo.IsOptional);
                        }
                    }
                }

                int luaPos = isStatic ? 1 : 2;

                int argStoreStart = -1;

                for (int j = 0; j < paramInfos.Length; j++)
                {
                    var paramInfo = paramInfos[j];
                    var paramRawType = paramInfo.ParameterType.IsByRef ? paramInfo.ParameterType.GetElementType() :
                        paramInfo.ParameterType;
                    var argStore = il.DeclareLocal(paramRawType);
                    if (paramInfo.IsOptional)
                    {
                        //UnityEngine.Debug.Log(paramInfo.Name + "," + paramRawType + "," + paramInfo.DefaultValue);
                        emitLiteralLoad(il, paramRawType, paramInfo.DefaultValue, argStore.LocalIndex);
                        il.Emit(OpCodes.Stloc, argStore);
                    }
                    //UnityEngine.Debug.LogWarning(declaringType.Name + "." + method.Name + "." + paramInfos[j].Name + " pos(d):" + argStore.LocalIndex + ", pt:" + paramRawType + ", j:" + j);
                    if (argStoreStart == -1)
                    {
                        argStoreStart = argStore.LocalIndex;
                    }
                    
                }
                for (int j = 0; j < paramInfos.Length; j++)
                {
                    var paramInfo = paramInfos[j];
                    var paramRawType = paramInfo.ParameterType.IsByRef ? paramInfo.ParameterType.GetElementType() :
                        paramInfo.ParameterType;
                    if (!paramInfo.IsOut)
                    {
                        Label endOfGetValue = il.DefineLabel();
                        if (paramInfo.IsOptional)
                        {
                            il.Emit(OpCodes.Ldarg_0);
                            il.Emit(OpCodes.Ldc_I4, luaPos);
                            il.Emit(OpCodes.Call, LuaAPI_lua_type);
                            il.Emit(OpCodes.Ldc_I4_M1);
                            il.Emit(OpCodes.Beq, endOfGetValue);
                        }
                        EmitGetObject(il, luaPos++, paramRawType, L, translator, null, hasParams && (j == paramInfos.Length - 1));
                        il.Emit(OpCodes.Stloc, argStoreStart + j);
                        il.MarkLabel(endOfGetValue);
                    }
                }

                LocalBuilder valueTypeTmp = null;

                if (!isStatic && (!method.IsConstructor || method.DeclaringType.IsValueType))
                {
                    if (!method.IsConstructor)
                    {
                        EmitGetObject(il, 1, method.DeclaringType, L, translator, null);
                    }
                    if (method.DeclaringType.IsValueType)
                    {
                        if (method.IsConstructor)
                        {
                            methodReturn = il.DeclareLocal(method.DeclaringType);
                            il.Emit(OpCodes.Ldloca, methodReturn);
                        }
                        else
                        {
                            valueTypeTmp = il.DeclareLocal(method.DeclaringType);
                            il.Emit(OpCodes.Stloc, valueTypeTmp);
                            il.Emit(OpCodes.Ldloca, valueTypeTmp);
                        }
                    }
                }

                for (int j = 0; j < paramInfos.Length; j++)
                {
                    //UnityEngine.Debug.LogWarning(declaringType.Name + "." + method.Name + "." + paramInfos[j].Name +" pos:" + (argStoreStart + j) + ", op:" + (paramInfos[j].ParameterType.IsByRef ? OpCodes.Ldloca : OpCodes.Ldloc) + ", j:" + j );
                    il.Emit(paramInfos[j].ParameterType.IsByRef ? OpCodes.Ldloca : OpCodes.Ldloc, argStoreStart + j);
                }

                if (method.IsConstructor)
                {
                    if (method.DeclaringType.IsValueType)
                    {
                        il.Emit(OpCodes.Call, method as ConstructorInfo);
                    }
                    else
                    {
                        il.Emit(OpCodes.Newobj, method as ConstructorInfo);
                    }
                }
                else
                {
                    il.Emit(isStatic ? OpCodes.Call : OpCodes.Callvirt, method as MethodInfo);
                }

                if (valueTypeTmp != null)
                {
                    emitUpdateIfNeeded(il, L, translator, method.DeclaringType, 1, valueTypeTmp.LocalIndex);
                }

                if (isIndexer)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Call, LuaAPI_lua_pushboolean);
                }

                bool hasReturn = false;

                MethodInfo methodInfo = method as MethodInfo;
                if (methodInfo == null || methodInfo.ReturnType != typeof(void))
                {
                    hasReturn = true;
                    Type returnType = methodInfo == null ? method.DeclaringType : methodInfo.ReturnType;
                    if (methodReturn == null)
                    {
                        methodReturn = il.DeclareLocal(returnType);
                        il.Emit(OpCodes.Stloc, methodReturn);
                    }
                    emitPush(il, returnType, (short)methodReturn.LocalIndex, false, L, translator, false);
                }

                int luaIndex = isStatic ? 1 : 2;
                for (int j = 0; j < paramInfos.Length; j++)
                {
                    if (paramInfos[j].ParameterType.IsByRef)
                    {
                        var rawParamType = paramInfos[j].ParameterType.GetElementType();
                        emitPush(il, rawParamType,
                            (short)(argStoreStart + j), false, L, translator, false);
                        if (!paramInfos[j].IsOut)
                        {
                            emitUpdateIfNeeded(il, L, translator, rawParamType, luaIndex, argStoreStart + j);
                        }
                    }
                    if (!paramInfos[j].IsOut)
                    {
                        luaIndex++;
                    }
                }

                il.Emit(OpCodes.Ldc_I4, outParamCount + (hasReturn ? 1 : 0) + (isIndexer ? 1 : 0));
                il.Emit(OpCodes.Stloc, wrapRet);
                il.Emit(OpCodes.Leave, retPoint);
                //il.Emit(OpCodes.Ret);

                if (needCheckParameterType)
                {
                    il.MarkLabel(endOfBlock);
                }
            }

            if (declaringType.IsValueType && (methodsToCall.Count == 0 || methodsToCall[0].IsConstructor))
            {
                Label endOfBlock = il.DefineLabel();
                il.Emit(OpCodes.Ldloc, top);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Bne_Un, endOfBlock);

                var methodReturn = il.DeclareLocal(declaringType);

                il.Emit(OpCodes.Ldloca, methodReturn);
                il.Emit(OpCodes.Initobj, declaringType);
                emitPush(il, declaringType, (short)methodReturn.LocalIndex, false, L, translator, false);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Stloc, wrapRet);
                il.Emit(OpCodes.Leave_S, retPoint);
                il.MarkLabel(endOfBlock);
            }

            il.Emit(OpCodes.Leave, exceptionBlock);
            emitCatchBlock(il, ex, wrapRet, retPoint, exceptionBlock);

            if (needCheckParameterType)
            {
                if (isIndexer)
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Call, LuaAPI_lua_pushboolean);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Ret);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    if (methodDesciption == null)
                    {
                        if (methodsToCall.Count > 0)
                        {
                            methodDesciption = declaringType + "." + methodsToCall[0].Name;
                        }
                        else
                        {
                            methodDesciption = "unknow method in " + declaringType;
                        }
                    }
                    il.Emit(OpCodes.Ldstr, "invalid arguments to " + methodDesciption + "!");
                    il.Emit(OpCodes.Call, LuaAPI_luaL_error);
                    il.Emit(OpCodes.Ret);
                }
            }

            il.MarkLabel(retPoint);
            il.Emit(OpCodes.Ldloc, wrapRet);
            il.Emit(OpCodes.Ret);

            return methodBuilder;
        }
    }
}

#endif
