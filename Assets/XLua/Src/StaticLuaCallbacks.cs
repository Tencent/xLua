/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

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
    using System;
    using System.IO;
    using System.Reflection;

    public partial class StaticLuaCallbacks
    {
        internal LuaCSFunction GcMeta, ToStringMeta, EnumAndMeta, EnumOrMeta;

        internal LuaCSFunction StaticCSFunctionWraper, FixCSFunctionWraper;

        internal LuaCSFunction DelegateCtor;

        public StaticLuaCallbacks()
        {
            GcMeta = new LuaCSFunction(StaticLuaCallbacks.LuaGC);
            ToStringMeta = new LuaCSFunction(StaticLuaCallbacks.ToString);
            EnumAndMeta = new LuaCSFunction(EnumAnd);
            EnumOrMeta = new LuaCSFunction(EnumOr);
            StaticCSFunctionWraper = new LuaCSFunction(StaticLuaCallbacks.StaticCSFunction);
            FixCSFunctionWraper = new LuaCSFunction(StaticLuaCallbacks.FixCSFunction);
            DelegateCtor = new LuaCSFunction(DelegateConstructor);
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int EnumAnd(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                object left = translator.FastGetCSObj(L, 1);
                object right = translator.FastGetCSObj(L, 2);
                Type typeOfLeft = left.GetType();
                if (!typeOfLeft.IsEnum() || typeOfLeft != right.GetType())
                {
                    return LuaAPI.luaL_error(L, "invalid argument for Enum BitwiseAnd");
                }
                translator.PushAny(L, Enum.ToObject(typeOfLeft, Convert.ToInt64(left) & Convert.ToInt64(right)));
                return 1;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in Enum BitwiseAnd:" + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int EnumOr(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                object left = translator.FastGetCSObj(L, 1);
                object right = translator.FastGetCSObj(L, 2);
                Type typeOfLeft = left.GetType();
                if (!typeOfLeft.IsEnum() || typeOfLeft != right.GetType())
                {
                    return LuaAPI.luaL_error(L, "invalid argument for Enum BitwiseOr");
                }
                translator.PushAny(L, Enum.ToObject(typeOfLeft, Convert.ToInt64(left) | Convert.ToInt64(right)));
                return 1;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in Enum BitwiseOr:" + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int StaticCSFunction(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                LuaCSFunction func = (LuaCSFunction)translator.FastGetCSObj(L, LuaAPI.xlua_upvalueindex(1));
                return func(L);
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in StaticCSFunction:" + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        static int FixCSFunction(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                int idx = LuaAPI.xlua_tointeger(L, LuaAPI.xlua_upvalueindex(1));
                LuaCSFunction func = (LuaCSFunction)translator.GetFixCSFunction(idx);
                return func(L);
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in FixCSFunction:" + e);
            }
        }

#if GEN_CODE_MINIMIZE
        [MonoPInvokeCallback(typeof(LuaDLL.CSharpWrapperCaller))]
        internal static int CSharpWrapperCallerImpl(RealStatePtr L, int funcidx, int top)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                return translator.CallCSharpWrapper(L, funcidx, top);
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception:" + e);
            }
        }
#endif

#if GEN_CODE_MINIMIZE
        public static int DelegateCall(RealStatePtr L, int top)
#else
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int DelegateCall(RealStatePtr L)
#endif
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                object objDelegate = translator.FastGetCSObj(L, 1);
                if (objDelegate == null || !(objDelegate is Delegate))
                {
                    return LuaAPI.luaL_error(L, "trying to invoke a value that is not delegate nor callable");
                }
                return translator.methodWrapsCache.GetDelegateWrap(objDelegate.GetType())(L);
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in DelegateCall:" + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int LuaGC(RealStatePtr L)
        {
            try
            {
                int udata = LuaAPI.xlua_tocsobj_safe(L, 1);
                if (udata != -1)
                {
                    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                    translator.collectObject(udata);
                }
                return 0;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in LuaGC:" + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int ToString(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                object obj = translator.FastGetCSObj(L, 1);
                translator.PushAny(L, obj != null ? (obj.ToString() + ": " + obj.GetHashCode()) : "<invalid c# object>");
                return 1;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in ToString:" + e);
            }
        }

#if GEN_CODE_MINIMIZE
        public static int DelegateCombine(RealStatePtr L, int top)
#else
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int DelegateCombine(RealStatePtr L)
#endif
        {
            try
            {
                var translator = ObjectTranslatorPool.Instance.Find(L);
                Type type = translator.FastGetCSObj(L, LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TUSERDATA ? 1 : 2).GetType();
                Delegate d1 = translator.GetObject(L, 1, type) as Delegate;
                Delegate d2 = translator.GetObject(L, 2, type) as Delegate;
                if (d1 == null || d2 == null)
                {
                    return LuaAPI.luaL_error(L, "one parameter must be a delegate, other one must be delegate or function");
                }
                translator.PushAny(L, Delegate.Combine(d1, d2));
                return 1;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in DelegateCombine:" + e);
            }
        }

#if GEN_CODE_MINIMIZE
        public static int DelegateRemove(RealStatePtr L, int top)
#else
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int DelegateRemove(RealStatePtr L)
#endif
        {
            try
            {
                var translator = ObjectTranslatorPool.Instance.Find(L);
                Delegate d1 = translator.FastGetCSObj(L, 1) as Delegate;
                if (d1 == null)
                {
                    return LuaAPI.luaL_error(L, "#1 parameter must be a delegate");
                }
                Delegate d2 = translator.GetObject(L, 2, d1.GetType()) as Delegate;
                if (d2 == null)
                {
                    return LuaAPI.luaL_error(L, "#2 parameter must be a delegate or a function ");
                }
                translator.PushAny(L, Delegate.Remove(d1, d2));
                return 1;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in DelegateRemove:" + e);
            }
        }

        static bool tryPrimitiveArrayGet(Type type, RealStatePtr L, object obj, int index)
        {
            bool ok = true;

            if (type == typeof(int[]))
            {
                int[] array = obj as int[];
                LuaAPI.xlua_pushinteger(L, array[index]);
            }
            else if (type == typeof(float[]))
            {
                float[] array = obj as float[];
                LuaAPI.lua_pushnumber(L, array[index]);
            }
            else if (type == typeof(double[]))
            {
                double[] array = obj as double[];
                LuaAPI.lua_pushnumber(L, array[index]);
            }
            else if (type == typeof(bool[]))
            {
                bool[] array = obj as bool[];
                LuaAPI.lua_pushboolean(L, array[index]);
            }
            else if (type == typeof(long[]))
            {
                long[] array = obj as long[];
                LuaAPI.lua_pushint64(L, array[index]);
            }
            else if (type == typeof(ulong[]))
            {
                ulong[] array = obj as ulong[];
                LuaAPI.lua_pushuint64(L, array[index]);
            }
            else if (type == typeof(sbyte[]))
            {
                sbyte[] array = obj as sbyte[];
                LuaAPI.xlua_pushinteger(L, array[index]);
            }
            else if (type == typeof(short[]))
            {
                short[] array = obj as short[];
                LuaAPI.xlua_pushinteger(L, array[index]);
            }
            else if (type == typeof(ushort[]))
            {
                ushort[] array = obj as ushort[];
                LuaAPI.xlua_pushinteger(L, array[index]);
            }
            else if (type == typeof(char[]))
            {
                char[] array = obj as char[];
                LuaAPI.xlua_pushinteger(L, array[index]);
            }
            else if (type == typeof(uint[]))
            {
                uint[] array = obj as uint[];
                LuaAPI.xlua_pushuint(L, array[index]);
            }
            else if (type == typeof(IntPtr[]))
            {
                IntPtr[] array = obj as IntPtr[];
                LuaAPI.lua_pushlightuserdata(L, array[index]);
            }
            else if (type == typeof(decimal[]))
            {
                decimal[] array = obj as decimal[];
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                translator.PushDecimal(L, array[index]);
            }
            else if (type == typeof(string[]))
            {
                string[] array = obj as string[];
                LuaAPI.lua_pushstring(L, array[index]);
            }
            else
            {
                ok = false;
            }
            return ok;
        }

#if GEN_CODE_MINIMIZE
        public static int ArrayIndexer(RealStatePtr L, int top)
#else
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int ArrayIndexer(RealStatePtr L)
#endif
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                System.Array array = (System.Array)translator.FastGetCSObj(L, 1);

                if (array == null)
                {
                    return LuaAPI.luaL_error(L, "#1 parameter is not a array!");
                }

                int i = LuaAPI.xlua_tointeger(L, 2);

                if (i >= array.Length)
                {
                    return LuaAPI.luaL_error(L, "index out of range! i =" + i + ", array.Length=" + array.Length);
                }

                Type type = array.GetType();
                if (tryPrimitiveArrayGet(type, L, array, i))
                {
                    return 1;
                }

                if (InternalGlobals.genTryArrayGetPtr != null)
                {
                    try
                    {
                        if (InternalGlobals.genTryArrayGetPtr(type, L, translator, array, i))
                        {
                            return 1;
                        }
                    }
                    catch (Exception e)
                    {
                        return LuaAPI.luaL_error(L, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
                    }
                }

                object ret = array.GetValue(i);
                translator.PushAny(L, ret);

                return 1;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in ArrayIndexer:" + e);
            }
        }


        public static bool TryPrimitiveArraySet(Type type, RealStatePtr L, object obj, int array_idx, int obj_idx)
        {
            bool ok = true;

            LuaTypes lua_type = LuaAPI.lua_type(L, obj_idx);

            if (type == typeof(int[]) && lua_type == LuaTypes.LUA_TNUMBER)
            {
                int[] array = obj as int[];
                array[array_idx] = LuaAPI.xlua_tointeger(L, obj_idx);
            }
            else if (type == typeof(float[]) && lua_type == LuaTypes.LUA_TNUMBER)
            {
                float[] array = obj as float[];
                array[array_idx] = (float)LuaAPI.lua_tonumber(L, obj_idx);
            }
            else if (type == typeof(double[]) && lua_type == LuaTypes.LUA_TNUMBER)
            {
                double[] array = obj as double[];
                array[array_idx] = LuaAPI.lua_tonumber(L, obj_idx); ;
            }
            else if (type == typeof(bool[]) && lua_type == LuaTypes.LUA_TBOOLEAN)
            {
                bool[] array = obj as bool[];
                array[array_idx] = LuaAPI.lua_toboolean(L, obj_idx);
            }
            else if (type == typeof(long[]) && LuaAPI.lua_isint64(L, obj_idx))
            {
                long[] array = obj as long[];
                array[array_idx] = LuaAPI.lua_toint64(L, obj_idx);
            }
            else if (type == typeof(ulong[]) && LuaAPI.lua_isuint64(L, obj_idx))
            {
                ulong[] array = obj as ulong[];
                array[array_idx] = LuaAPI.lua_touint64(L, obj_idx);
            }
            else if (type == typeof(sbyte[]) && lua_type == LuaTypes.LUA_TNUMBER)
            {
                sbyte[] array = obj as sbyte[];
                array[array_idx] = (sbyte)LuaAPI.xlua_tointeger(L, obj_idx);
            }
            else if (type == typeof(short[]) && lua_type == LuaTypes.LUA_TNUMBER)
            {
                short[] array = obj as short[];
                array[array_idx] = (short)LuaAPI.xlua_tointeger(L, obj_idx);
            }
            else if (type == typeof(ushort[]) && lua_type == LuaTypes.LUA_TNUMBER)
            {
                ushort[] array = obj as ushort[];
                array[array_idx] = (ushort)LuaAPI.xlua_tointeger(L, obj_idx);
            }
            else if (type == typeof(char[]) && lua_type == LuaTypes.LUA_TNUMBER)
            {
                char[] array = obj as char[];
                array[array_idx] = (char)LuaAPI.xlua_tointeger(L, obj_idx);
            }
            else if (type == typeof(uint[]) && lua_type == LuaTypes.LUA_TNUMBER)
            {
                uint[] array = obj as uint[];
                array[array_idx] = LuaAPI.xlua_touint(L, obj_idx);
            }
            else if (type == typeof(IntPtr[]) && lua_type == LuaTypes.LUA_TLIGHTUSERDATA)
            {
                IntPtr[] array = obj as IntPtr[];
                array[array_idx] = LuaAPI.lua_touserdata(L, obj_idx);
            }
            else if (type == typeof(decimal[]))
            {
                decimal[] array = obj as decimal[];
                if (lua_type == LuaTypes.LUA_TNUMBER)
                {
                    array[array_idx] = (decimal)LuaAPI.lua_tonumber(L, obj_idx);
                }

                if (lua_type == LuaTypes.LUA_TUSERDATA)
                {
                    ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                    if (translator.IsDecimal(L, obj_idx))
                    {
                        translator.Get(L, obj_idx, out array[array_idx]);
                    }
                    else
                    {
                        ok = false;
                    }
                }
                else
                {
                    ok = false;
                }
            }
            else if (type == typeof(string[]) && lua_type == LuaTypes.LUA_TSTRING)
            {
                string[] array = obj as string[];
                array[array_idx] = LuaAPI.lua_tostring(L, obj_idx);
            }
            else
            {
                ok = false;
            }
            return ok;
        }

#if GEN_CODE_MINIMIZE
        public static int ArrayNewIndexer(RealStatePtr L, int top)
#else
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int ArrayNewIndexer(RealStatePtr L)
#endif
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                System.Array array = (System.Array)translator.FastGetCSObj(L, 1);

                if (array == null)
                {
                    return LuaAPI.luaL_error(L, "#1 parameter is not a array!");
                }

                int i = LuaAPI.xlua_tointeger(L, 2);

                if (i >= array.Length)
                {
                    return LuaAPI.luaL_error(L, "index out of range! i =" + i + ", array.Length=" + array.Length);
                }

                Type type = array.GetType();
                if (TryPrimitiveArraySet(type, L, array, i, 3))
                {
                    return 0;
                }

                if (InternalGlobals.genTryArraySetPtr != null)
                {
                    try
                    {
                        if (InternalGlobals.genTryArraySetPtr(type, L, translator, array, i, 3))
                        {
                            return 0;
                        }
                    }
                    catch (Exception e)
                    {
                        return LuaAPI.luaL_error(L, "c# exception:" + e.Message + ",stack:" + e.StackTrace);
                    }
                }

                object val = translator.GetObject(L, 3, type.GetElementType());
                array.SetValue(val, i);

                return 0;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in ArrayNewIndexer:" + e);
            }
        }

#if GEN_CODE_MINIMIZE
        public static int ArrayLength(RealStatePtr L, int top)
#else
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int ArrayLength(RealStatePtr L)
#endif
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                System.Array array = (System.Array)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, array.Length);
                return 1;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in ArrayLength:" + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int MetaFuncIndex(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                Type type = translator.FastGetCSObj(L, 2) as Type;
                if (type == null)
                {
                    return LuaAPI.luaL_error(L, "#2 param need a System.Type!");
                }
                //UnityEngine.Debug.Log("============================load type by __index:" + type);
                //translator.TryDelayWrapLoader(L, type);
                translator.GetTypeId(L, type);
                LuaAPI.lua_pushvalue(L, 2);
                LuaAPI.lua_rawget(L, 1);
                return 1;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in MetaFuncIndex:" + e);
            }
        }


        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        internal static int Panic(RealStatePtr L)
        {
            string reason = String.Format("unprotected error in call to Lua API ({0})", LuaAPI.lua_tostring(L, -1));
            throw new LuaException(reason);
        }

#if !XLUA_GENERAL
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        internal static int Print(RealStatePtr L)
        {
            try
            {
                int n = LuaAPI.lua_gettop(L);
                string s = String.Empty;

                if (0 != LuaAPI.xlua_getglobal(L, "tostring"))
                {
                    return LuaAPI.luaL_error(L, "can not get tostring in print:");
                }

                for (int i = 1; i <= n; i++)
                {
                    LuaAPI.lua_pushvalue(L, -1);  /* function to be called */
                    LuaAPI.lua_pushvalue(L, i);   /* value to print */
                    if (0 != LuaAPI.lua_pcall(L, 1, 1, 0))
                    {
                        return LuaAPI.lua_error(L);
                    }
                    s += LuaAPI.lua_tostring(L, -1);

                    if (i != n) s += "\t";

                    LuaAPI.lua_pop(L, 1);  /* pop result */
                }
                UnityEngine.Debug.Log("LUA: " + s);
                return 0;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in print:" + e);
            }
        }
#endif

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        internal static int LoadSocketCore(RealStatePtr L)
        {
            return LuaAPI.luaopen_socket_core(L);
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        internal static int LoadCS(RealStatePtr L)
        {
            LuaAPI.xlua_pushasciistring(L, LuaEnv.CSHARP_NAMESPACE);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        internal static int LoadBuiltinLib(RealStatePtr L)
        {
            try
            {
                string builtin_lib = LuaAPI.lua_tostring(L, 1);

                LuaEnv self = ObjectTranslatorPool.Instance.Find(L).luaEnv;

                LuaCSFunction initer;

                if (self.buildin_initer.TryGetValue(builtin_lib, out initer))
                {
                    LuaAPI.lua_pushstdcallcfunction(L, initer);
                }
                else
                {
                    LuaAPI.lua_pushstring(L, string.Format(
                        "\n\tno such builtin lib '{0}'", builtin_lib));
                }
                return 1;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in LoadBuiltinLib:" + e);
            }
        }

#if !XLUA_GENERAL
        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        internal static int LoadFromResource(RealStatePtr L)
        {
            try
            {
                string filename = LuaAPI.lua_tostring(L, 1).Replace('.', '/') + ".lua";

                // Load with Unity3D resources
                UnityEngine.TextAsset file = (UnityEngine.TextAsset)UnityEngine.Resources.Load(filename);
                if (file == null)
                {
                    LuaAPI.lua_pushstring(L, string.Format(
                        "\n\tno such resource '{0}'", filename));
                }
                else
                {
                    if (LuaAPI.xluaL_loadbuffer(L, file.bytes, file.bytes.Length, "@" + filename) != 0)
                    {
                        return LuaAPI.luaL_error(L, String.Format("error loading module {0} from resource, {1}",
                            LuaAPI.lua_tostring(L, 1), LuaAPI.lua_tostring(L, -1)));
                    }
                }

                return 1;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in LoadFromResource:" + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        internal static int LoadFromStreamingAssetsPath(RealStatePtr L)
        {
            try
            {
                string filename = LuaAPI.lua_tostring(L, 1).Replace('.', '/') + ".lua";
                var filepath = UnityEngine.Application.streamingAssetsPath + "/" + filename;
#if UNITY_ANDROID && !UNITY_EDITOR
                UnityEngine.WWW www = new UnityEngine.WWW(filepath);
                while (true)
                {
                    if (www.isDone || !string.IsNullOrEmpty(www.error))
                    {
                        System.Threading.Thread.Sleep(50); //�Ƚ�hacker������
                        if (!string.IsNullOrEmpty(www.error))
                        {
                            LuaAPI.lua_pushstring(L, string.Format(
                               "\n\tno such file '{0}' in streamingAssetsPath!", filename));
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning("load lua file from StreamingAssets is obsolete, filename:" + filename);
                            if (LuaAPI.xluaL_loadbuffer(L, www.bytes, www.bytes.Length , "@" + filename) != 0)
                            {
                                return LuaAPI.luaL_error(L, String.Format("error loading module {0} from streamingAssetsPath, {1}",
                                    LuaAPI.lua_tostring(L, 1), LuaAPI.lua_tostring(L, -1)));
                            }
                        }
                        break;
                    }
                }
#else
                if (File.Exists(filepath))
                {
                    // string text = File.ReadAllText(filepath);
                    var bytes = File.ReadAllBytes(filepath);

                    UnityEngine.Debug.LogWarning("load lua file from StreamingAssets is obsolete, filename:" + filename);
                    if (LuaAPI.xluaL_loadbuffer(L, bytes, bytes.Length, "@" + filename) != 0)
                    {
                        return LuaAPI.luaL_error(L, String.Format("error loading module {0} from streamingAssetsPath, {1}",
                            LuaAPI.lua_tostring(L, 1), LuaAPI.lua_tostring(L, -1)));
                    }
                }
                else
                {
                    LuaAPI.lua_pushstring(L, string.Format(
                        "\n\tno such file '{0}' in streamingAssetsPath!", filename));
                }
#endif
                return 1;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in LoadFromStreamingAssetsPath:" + e);
            }
        }
#endif

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        internal static int LoadFromCustomLoaders(RealStatePtr L)
        {
            try
            {
                string filename = LuaAPI.lua_tostring(L, 1);

                LuaEnv self = ObjectTranslatorPool.Instance.Find(L).luaEnv;

                foreach (var loader in self.customLoaders)
                {
                    string real_file_path = filename;
                    byte[] bytes = loader(ref real_file_path);
                    if (bytes != null)
                    {
                        if (LuaAPI.xluaL_loadbuffer(L, bytes, bytes.Length, "@" + real_file_path) != 0)
                        {
                            return LuaAPI.luaL_error(L, String.Format("error loading module {0} from CustomLoader, {1}",
                                LuaAPI.lua_tostring(L, 1), LuaAPI.lua_tostring(L, -1)));
                        }
                        return 1;
                    }
                }
                LuaAPI.lua_pushstring(L, string.Format(
                    "\n\tno such file '{0}' in CustomLoaders!", filename));
                return 1;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in LoadFromCustomLoaders:" + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int LoadAssembly(RealStatePtr L)
        {
#if UNITY_WSA && !UNITY_EDITOR
            return LuaAPI.luaL_error(L, "xlua.load_assembly no support in uwp!");
#else
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                string assemblyName = LuaAPI.lua_tostring(L, 1);

                Assembly assembly = null;

                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch (BadImageFormatException)
                {
                    // The assemblyName was invalid.  It is most likely a path.
                }

                if (assembly == null)
                {
                    assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyName));
                }

                if (assembly != null && !translator.assemblies.Contains(assembly))
                {
                    translator.assemblies.Add(assembly);
                }
                return 0;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in xlua.load_assembly:" + e);
            }
#endif
        }


        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int ImportType(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                string className = LuaAPI.lua_tostring(L, 1);
                Type type = translator.FindType(className);
                if (type != null)
                {
                    if (translator.GetTypeId(L, type) >= 0)
                    {
                        LuaAPI.lua_pushboolean(L, true);
                    }
                    else
                    {
                        return LuaAPI.luaL_error(L, "can not load type " + type);
                    }
                }
                else
                {
                    LuaAPI.lua_pushnil(L);
                }
                return 1;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in xlua.import_type:" + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int ImportGenericType(RealStatePtr L)
        {
            try
            {
                int top = LuaAPI.lua_gettop(L);
                if (top < 2) return LuaAPI.luaL_error(L, "import generic type need at lease 2 arguments");
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                string className = LuaAPI.lua_tostring(L, 1);
                if (className.EndsWith("<>")) className = className.Substring(0, className.Length - 2);
                Type genericDef = translator.FindType(className + "`" + (top - 1));
                if (genericDef == null || !genericDef.IsGenericTypeDefinition)
                {
                    LuaAPI.lua_pushnil(L);
                }
                else
                {
                    Type[] typeArguments = new Type[top - 1];
                    for(int i = 2; i <= top; i++)
                    {

                        typeArguments[i - 2] = getType(L, translator, i);
                        if (typeArguments[i - 2] == null)
                        {
                            return LuaAPI.luaL_error(L, "param need a type");
                        }
                    }
                    Type genericInc = genericDef.MakeGenericType(typeArguments);
                    translator.GetTypeId(L, genericInc);
                    translator.PushAny(L, genericInc);
                }

                return 1;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in xlua.import_type:" + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int Cast(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                Type type;
                translator.Get(L, 2, out type);

                if (type == null)
                {
                    return LuaAPI.luaL_error(L, "#2 param[" + LuaAPI.lua_tostring(L, 2) + "]is not valid type indicator");
                }
                LuaAPI.luaL_getmetatable(L, type.FullName);
                if (LuaAPI.lua_isnil(L, -1))
                {
                    return LuaAPI.luaL_error(L, "no gen code for " + LuaAPI.lua_tostring(L, 2));
                }
                LuaAPI.lua_setmetatable(L, 1);
                return 0;
            }
            catch (System.Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in xlua.cast:" + e);
            }
        }

        static Type getType(RealStatePtr L, ObjectTranslator translator, int idx)
        {
            if (LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TTABLE)
            {
                LuaTable tbl;
                translator.Get(L, idx, out tbl);
                return tbl.Get<Type>("UnderlyingSystemType");
            }
            else if (LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TSTRING)
            {
                string className = LuaAPI.lua_tostring(L, idx);
                return translator.FindType(className);
            }
            else if (translator.GetObject(L, idx) is Type)
            {
                return translator.GetObject(L, idx) as Type;
            }
            else
            {
                return null;
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int XLuaAccess(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                Type type = getType(L, translator, 1);
                object obj = null;
                if (type == null && LuaAPI.lua_type(L, 1) == LuaTypes.LUA_TUSERDATA)
                {
                    obj = translator.SafeGetCSObj(L, 1);
                    if (obj == null)
                    {
                        return LuaAPI.luaL_error(L, "xlua.access, #1 parameter must a type/c# object/string");
                    }
                    type = obj.GetType();
                }

                if (type == null)
                {
                    return LuaAPI.luaL_error(L, "xlua.access, can not find c# type");
                }

                string fieldName = LuaAPI.lua_tostring(L, 2);

                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

                if (LuaAPI.lua_gettop(L) > 2) // set
                {
                    var field = type.GetField(fieldName, bindingFlags);
                    if (field != null)
                    {
                        field.SetValue(obj, translator.GetObject(L, 3, field.FieldType));
                        return 0;
                    }
                    var prop = type.GetProperty(fieldName, bindingFlags);
                    if (prop != null)
                    {
                        prop.SetValue(obj, translator.GetObject(L, 3, prop.PropertyType), null);
                        return 0;
                    }
                }
                else
                {
                    var field = type.GetField(fieldName, bindingFlags);
                    if (field != null)
                    {
                        translator.PushAny(L, field.GetValue(obj));
                        return 1;
                    }
                    var prop = type.GetProperty(fieldName, bindingFlags);
                    if (prop != null)
                    {
                        translator.PushAny(L, prop.GetValue(obj, null));
                        return 1;
                    }
                }
                return LuaAPI.luaL_error(L, "xlua.access, no field " + fieldName);
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in xlua.access: " + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int XLuaPrivateAccessible(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                Type type = getType(L, translator, 1); ;
                if (type == null)
                {
                    return LuaAPI.luaL_error(L, "xlua.private_accessible, can not find c# type");
                }

                while(type != null)
                {
                    translator.PrivateAccessible(L, type);
                    type = type.BaseType;
                }
                return 0;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in xlua.private_accessible: " + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int XLuaMetatableOperation(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                Type type = getType(L, translator, 1);
                if (type == null)
                {
                    return LuaAPI.luaL_error(L, "xlua.metatable_operation, can not find c# type");
                }

                bool is_first = false;
                int type_id = translator.getTypeId(L, type, out is_first);

                var param_num = LuaAPI.lua_gettop(L);

                if (param_num == 1) //get
                {
                    LuaAPI.xlua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, type_id);
                    return 1;
                }
                else if (param_num == 2) //set
                {
                    if (LuaAPI.lua_type(L, 2) != LuaTypes.LUA_TTABLE)
                    {
                        return LuaAPI.luaL_error(L, "argument #2 must be a table");
                    }
                    LuaAPI.lua_pushnumber(L, type_id);
                    LuaAPI.xlua_rawseti(L, 2, 1);
                    LuaAPI.xlua_rawseti(L, LuaIndexes.LUA_REGISTRYINDEX, type_id);
                    return 0;
                }
                else
                {
                    return LuaAPI.luaL_error(L, "invalid argument num for xlua.metatable_operation: " + param_num);
                }
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in xlua.metatable_operation: " + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int DelegateConstructor(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                Type type = getType(L, translator, 1);
                if (type == null || !typeof(Delegate).IsAssignableFrom(type))
                {
                    return LuaAPI.luaL_error(L, "delegate constructor: #1 argument must be a Delegate's type");
                }
                translator.PushAny(L, translator.GetObject(L, 2, type));
                return 1;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in delegate constructor: " + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int ToFunction(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                MethodBase m;
                translator.Get(L, 1, out m);
                if (m == null)
                {
                    return LuaAPI.luaL_error(L, "ToFunction: #1 argument must be a MethodBase");
                }
                translator.PushFixCSFunction(L,
                        new LuaCSFunction(translator.methodWrapsCache._GenMethodWrap(m.DeclaringType, m.Name, new MethodBase[] { m }).Call));
                return 1;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in ToFunction: " + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int GenericMethodWraper(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                MethodInfo genericMethod;
                translator.Get(L, LuaAPI.xlua_upvalueindex(1), out genericMethod);
                int n = LuaAPI.lua_gettop(L);
                Type[] typeArguments = new Type[n];
                for(int i = 0; i < n; i++)
                {
                    Type type = getType(L, translator, i + 1);
                    if (type == null)
                    {
                        return LuaAPI.luaL_error(L, "param #" + (i + 1) + " is not a type");
                    }
                    typeArguments[i] = type;
                }
                var method = genericMethod.MakeGenericMethod(typeArguments);
                translator.PushFixCSFunction(L,
                        new LuaCSFunction(translator.methodWrapsCache._GenMethodWrap(method.DeclaringType, method.Name, new MethodBase[] { method }).Call));
                return 1;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in GenericMethodWraper: " + e);
            }
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int GetGenericMethod(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                Type type = getType(L, translator, 1);
                if (type == null)
                {
                    return LuaAPI.luaL_error(L, "xlua.get_generic_method, can not find c# type");
                }
                string methodName = LuaAPI.lua_tostring(L, 2);
                if (string.IsNullOrEmpty(methodName))
                {
                    return LuaAPI.luaL_error(L, "xlua.get_generic_method, #2 param need a string");
                }
                System.Collections.Generic.List<MethodInfo> matchMethods = new System.Collections.Generic.List<MethodInfo>();
                var allMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                for(int i = 0; i < allMethods.Length; i++)
                {
                    var method = allMethods[i];
                    if (method.Name == methodName && method.IsGenericMethodDefinition)
                    {
                        matchMethods.Add(method);
                    }
                }

                int methodIdx = 0;

                if (matchMethods.Count == 0)
                {
                    LuaAPI.lua_pushnil(L);
                }
                else
                {
                    if (LuaAPI.lua_isinteger(L, 3))
                    {
                        methodIdx = LuaAPI.xlua_tointeger(L, 3);
                    }
                    translator.PushAny(L, matchMethods[methodIdx]);
                    LuaAPI.lua_pushstdcallcfunction(L, GenericMethodWraper, 1);
                }
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in xlua.get_generic_method: " + e);
            }
            return 1;
        }

        [MonoPInvokeCallback(typeof(LuaCSFunction))]
        public static int ReleaseCsObject(RealStatePtr L)
        {
            try
            {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
                translator.ReleaseCSObj(L, 1);
                return 0;
            }
            catch (Exception e)
            {
                return LuaAPI.luaL_error(L, "c# exception in ReleaseCsObject: " + e);
            }
        }
    }
}