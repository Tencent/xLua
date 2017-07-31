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

namespace XLua
{
    public partial class LuaFunction : LuaBase
    {
        public LuaFunction(int reference, LuaEnv luaenv) : base(reference, luaenv)
        {
        }

        //Action和Func是方便使用的无gc api，如果需要用到out，ref参数，建议使用delegate
        //如果需要其它个数的Action和Func， 这个类声明为partial，可以自己加
        public void Action<T>(T a)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
                var L = luaEnv.L;
                var translator = luaEnv.translator;
                int oldTop = LuaAPI.lua_gettop(L);
                int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
                LuaAPI.lua_getref(L, luaReference);
                translator.PushByType(L, a);
                int error = LuaAPI.lua_pcall(L, 1, 0, errFunc);
                if (error != 0)
                    luaEnv.ThrowExceptionFromError(oldTop);
                LuaAPI.lua_settop(L, oldTop);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public TResult Func<T, TResult>(T a)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
                var L = luaEnv.L;
                var translator = luaEnv.translator;
                int oldTop = LuaAPI.lua_gettop(L);
                int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
                LuaAPI.lua_getref(L, luaReference);
                translator.PushByType(L, a);
                int error = LuaAPI.lua_pcall(L, 1, 1, errFunc);
                if (error != 0)
                    luaEnv.ThrowExceptionFromError(oldTop);
                TResult ret;
                try
                {
                    translator.Get(L, -1, out ret);
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    LuaAPI.lua_settop(L, oldTop);
                }
                return ret;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public void Action<T1, T2>(T1 a1, T2 a2)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
                var L = luaEnv.L;
                var translator = luaEnv.translator;
                int oldTop = LuaAPI.lua_gettop(L);
                int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
                LuaAPI.lua_getref(L, luaReference);
                translator.PushByType(L, a1);
                translator.PushByType(L, a2);
                int error = LuaAPI.lua_pcall(L, 2, 0, errFunc);
                if (error != 0)
                    luaEnv.ThrowExceptionFromError(oldTop);
                LuaAPI.lua_settop(L, oldTop);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public TResult Func<T1, T2, TResult>(T1 a1, T2 a2)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
                var L = luaEnv.L;
                var translator = luaEnv.translator;
                int oldTop = LuaAPI.lua_gettop(L);
                int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
                LuaAPI.lua_getref(L, luaReference);
                translator.PushByType(L, a1);
                translator.PushByType(L, a2);
                int error = LuaAPI.lua_pcall(L, 2, 1, errFunc);
                if (error != 0)
                    luaEnv.ThrowExceptionFromError(oldTop);
                TResult ret;
                try
                {
                    translator.Get(L, -1, out ret);
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    LuaAPI.lua_settop(L, oldTop);
                }
                return ret;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        //deprecated
        public object[] Call(object[] args, Type[] returnTypes)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
                int nArgs = 0;
                var L = luaEnv.L;
                var translator = luaEnv.translator;
                int oldTop = LuaAPI.lua_gettop(L);

                int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
                LuaAPI.lua_getref(L, luaReference);
                if (args != null)
                {
                    nArgs = args.Length;
                    for (int i = 0; i < args.Length; i++)
                    {
                        translator.PushAny(L, args[i]);
                    }
                }
                int error = LuaAPI.lua_pcall(L, nArgs, -1, errFunc);
                if (error != 0)
                    luaEnv.ThrowExceptionFromError(oldTop);

                LuaAPI.lua_remove(L, errFunc);
                if (returnTypes != null)
                    return translator.popValues(L, oldTop, returnTypes);
                else
                    return translator.popValues(L, oldTop);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        //deprecated
        public object[] Call(params object[] args)
        {
            return Call(args, null);
        }

        public T Cast<T>()
        {
            if (!typeof(T).IsSubclassOf(typeof(Delegate)))
            {
                throw new InvalidOperationException(typeof(T).Name + " is not a delegate type");
            }
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
                var L = luaEnv.L;
                var translator = luaEnv.translator;
                push(L);
                T ret = (T)translator.GetObject(L, -1, typeof(T));
                LuaAPI.lua_pop(luaEnv.L, 1);
                return ret;
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        public void SetEnv(LuaTable env)
        {
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (luaEnv.luaEnvLock)
            {
#endif
                var L = luaEnv.L;
                int oldTop = LuaAPI.lua_gettop(L);
                push(L);
                env.push(L);
                LuaAPI.lua_setfenv(L, -2);
                LuaAPI.lua_settop(L, oldTop);
#if THREAD_SAFE || HOTFIX_ENABLE
            }
#endif
        }

        internal override void push(RealStatePtr L)
        {
            LuaAPI.lua_getref(L, luaReference);
        }

        public override string ToString()
        {
            return "function :" + luaReference;
        }
    }

}
