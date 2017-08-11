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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace XLua
{
    public abstract class DelegateBridgeBase : LuaBase
    {
        private Type firstKey = null;

        private Delegate firstValue = null;

        private Dictionary<Type, Delegate> bindTo = null;

        protected int errorFuncRef;

        public DelegateBridgeBase(int reference, LuaEnv luaenv) : base(reference, luaenv)
        {
            errorFuncRef = luaenv.errorFuncRef;
        }

        public bool TryGetDelegate(Type key, out Delegate value)
        {
            if(key == firstKey)
            {
                value = firstValue;
                return true;
            }
            if (bindTo != null)
            {
                return bindTo.TryGetValue(key, out value);
            }
            value = null;
            return false;
        }

        public void AddDelegate(Type key, Delegate value)
        {
            if (key == firstKey)
            {
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            }

            if (firstKey == null && bindTo == null) // nothing 
            {
                firstKey = key;
                firstValue = value;
            }
            else if (firstKey != null && bindTo == null) // one key existed
            {
                bindTo = new Dictionary<Type, Delegate>();
                bindTo.Add(firstKey, firstValue);
                firstKey = null;
                firstValue = null;
                bindTo.Add(key, value);
            }
            else
            {
                bindTo.Add(key, value);
            }
        }

        public virtual Delegate GetDelegateByType(Type type)
        {
            return null;
        }
    }

    public static class HotfixDelegateBridge
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool xlua_get_hotfix_flag(int idx);

        
        [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_set_hotfix_flag(int idx, bool flag);
#else
        public static bool xlua_get_hotfix_flag(int idx)
        {
            return (idx < DelegateBridge.DelegateBridgeList.Length) && (DelegateBridge.DelegateBridgeList[idx] != null);
        }
#endif

        public static DelegateBridge Get(int idx)
        {
            return DelegateBridge.DelegateBridgeList[idx];
        }

        public static void Set(int idx, DelegateBridge val)
        {
            if (idx >= DelegateBridge.DelegateBridgeList.Length)
            {
                DelegateBridge[] newList = new DelegateBridge[idx + 1];
                for (int i = 0; i < DelegateBridge.DelegateBridgeList.Length; i++)
                {
                    newList[i] = DelegateBridge.DelegateBridgeList[i];
                }
                DelegateBridge.DelegateBridgeList = newList;
            }
            DelegateBridge.DelegateBridgeList[idx] = val;
#if UNITY_IPHONE && !UNITY_EDITOR
            xlua_set_hotfix_flag(idx, val != null);
#endif
        }
    }

    public partial class DelegateBridge : DelegateBridgeBase
    {
        internal static DelegateBridge[] DelegateBridgeList = new DelegateBridge[0];

        public static bool Gen_Flag = false;

        public DelegateBridge(int reference, LuaEnv luaenv) : base(reference, luaenv)
        {
        }


#if HOTFIX_ENABLE

        private int _oldTop = 0;
        private Stack<int> _stack = new Stack<int>();

        public void InvokeSessionStart()
        {
            System.Threading.Monitor.Enter(luaEnv.luaEnvLock);
            var L = luaEnv.L;
            _stack.Push(_oldTop);
            _oldTop = LuaAPI.lua_gettop(L);
            LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
            LuaAPI.lua_getref(L, luaReference);
        }

        public void Invoke(int nRet)
        {
            int error = LuaAPI.lua_pcall(luaEnv.L, LuaAPI.lua_gettop(luaEnv.L) - _oldTop - 2, nRet, _oldTop + 1);
            if (error != 0)
            {
                var lastOldTop = _oldTop;
                InvokeSessionEnd();
                luaEnv.ThrowExceptionFromError(lastOldTop);
            }
        }

        public void InvokeSessionEnd()
        {
            LuaAPI.lua_settop(luaEnv.L, _oldTop);
            _oldTop = _stack.Pop();
            System.Threading.Monitor.Exit(luaEnv.luaEnvLock);
        }

        public TResult InvokeSessionEndWithResult<TResult>()
        {
            if (LuaAPI.lua_gettop(luaEnv.L) < _oldTop + 2)
            {
                InvokeSessionEnd();
                throw new InvalidOperationException("no result!");
            }

            try
            {
                TResult ret;
                luaEnv.translator.Get(luaEnv.L, _oldTop + 2, out ret);
                return ret;
            }
            finally
            {
                InvokeSessionEnd();
            }
        }

        public void InParam<T>(T p)
        {
            try
            {
                luaEnv.translator.PushByType(luaEnv.L, p);
            }
            catch (Exception e)
            {
                InvokeSessionEnd();
                throw e;
            }
        }

        public void InParams<T>(T[] ps)
        {
            try
            {
                for (int i = 0; i < ps.Length; i++)
                {
                    luaEnv.translator.PushByType<T>(luaEnv.L, ps[i]);
                }
            }
            catch (Exception e)
            {
                InvokeSessionEnd();
                throw e;
            }
        }

        //pos start from 0
        public void OutParam<TResult>(int pos, out TResult ret)
        {
            if (LuaAPI.lua_gettop(luaEnv.L) < _oldTop + 2 + pos)
            {
                InvokeSessionEnd();
                throw new InvalidOperationException("no result in " + pos);
            }

            try
            {
                luaEnv.translator.Get(luaEnv.L, _oldTop + 2 + pos, out ret);
            }
            catch (Exception e)
            {
                InvokeSessionEnd();
                throw e;
            }
        }
#endif
    }
}
