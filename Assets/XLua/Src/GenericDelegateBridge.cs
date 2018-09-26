/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using LuaAPI = XLua.LuaDLL.Lua;

namespace XLua {
	public partial class DelegateBridge : DelegateBridgeBase {
		public void Action() {
#if THREAD_SAFE || HOTFIX_ENABLE
			lock(luaEnv.luaEnvLock) {
#endif
				var L = luaEnv.L;
                var translator = luaEnv.translator;
                int oldTop = LuaAPI.lua_gettop(L);
                int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
                LuaAPI.lua_getref(L, luaReference);
                int error = LuaAPI.lua_pcall(L, 0, 0, errFunc);
                if (error != 0)
                    luaEnv.ThrowExceptionFromError(oldTop);
                LuaAPI.lua_settop(L, oldTop);
#if THREAD_SAFE || HOTFIX_ENABLE
			}
#endif
		}

		public void Action<T1>(T1 p1) {
#if THREAD_SAFE || HOTFIX_ENABLE
			lock(luaEnv.luaEnvLock) {
#endif
				var L = luaEnv.L;
                var translator = luaEnv.translator;
                int oldTop = LuaAPI.lua_gettop(L);
                int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
                LuaAPI.lua_getref(L, luaReference);
				translator.PushByType(L, p1);
                int error = LuaAPI.lua_pcall(L, 1, 0, errFunc);
                if (error != 0)
                    luaEnv.ThrowExceptionFromError(oldTop);
                LuaAPI.lua_settop(L, oldTop);
#if THREAD_SAFE || HOTFIX_ENABLE
			}
#endif
		}

		public void Action<T1, T2>(T1 p1, T2 p2) {
#if THREAD_SAFE || HOTFIX_ENABLE
			lock(luaEnv.luaEnvLock) {
#endif
				var L = luaEnv.L;
                var translator = luaEnv.translator;
                int oldTop = LuaAPI.lua_gettop(L);
                int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
                LuaAPI.lua_getref(L, luaReference);
				translator.PushByType(L, p1);
				translator.PushByType(L, p2);
                int error = LuaAPI.lua_pcall(L, 2, 0, errFunc);
                if (error != 0)
                    luaEnv.ThrowExceptionFromError(oldTop);
                LuaAPI.lua_settop(L, oldTop);
#if THREAD_SAFE || HOTFIX_ENABLE
			}
#endif
		}

		public void Action<T1, T2, T3>(T1 p1, T2 p2, T3 p3) {
#if THREAD_SAFE || HOTFIX_ENABLE
			lock(luaEnv.luaEnvLock) {
#endif
				var L = luaEnv.L;
                var translator = luaEnv.translator;
                int oldTop = LuaAPI.lua_gettop(L);
                int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
                LuaAPI.lua_getref(L, luaReference);
				translator.PushByType(L, p1);
				translator.PushByType(L, p2);
				translator.PushByType(L, p3);
                int error = LuaAPI.lua_pcall(L, 3, 0, errFunc);
                if (error != 0)
                    luaEnv.ThrowExceptionFromError(oldTop);
                LuaAPI.lua_settop(L, oldTop);
#if THREAD_SAFE || HOTFIX_ENABLE
			}
#endif
		}

		public void Action<T1, T2, T3, T4>(T1 p1, T2 p2, T3 p3, T4 p4) {
#if THREAD_SAFE || HOTFIX_ENABLE
			lock(luaEnv.luaEnvLock) {
#endif
				var L = luaEnv.L;
                var translator = luaEnv.translator;
                int oldTop = LuaAPI.lua_gettop(L);
                int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
                LuaAPI.lua_getref(L, luaReference);
				translator.PushByType(L, p1);
				translator.PushByType(L, p2);
				translator.PushByType(L, p3);
				translator.PushByType(L, p4);
                int error = LuaAPI.lua_pcall(L, 4, 0, errFunc);
                if (error != 0)
                    luaEnv.ThrowExceptionFromError(oldTop);
                LuaAPI.lua_settop(L, oldTop);
#if THREAD_SAFE || HOTFIX_ENABLE
			}
#endif
		}


		public TResult Func<TResult>() {
#if THREAD_SAFE || HOTFIX_ENABLE
			lock(luaEnv.luaEnvLock) {
#endif
				var L = luaEnv.L;
				var translator = luaEnv.translator;
				int oldTop = LuaAPI.lua_gettop(L);
				int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				LuaAPI.lua_getref(L, luaReference);
				int error = LuaAPI.lua_pcall(L, 0, 1, errFunc);
				if (error != 0)
					luaEnv.ThrowExceptionFromError(oldTop);
				TResult ret;
				try {
					translator.Get(L, -1, out ret);
				} catch (Exception e) {
					throw e;
				} finally {
					LuaAPI.lua_settop(L, oldTop);
				}
				return ret;
#if THREAD_SAFE || HOTFIX_ENABLE
			}
#endif
		}

		public TResult Func<T1, TResult>(T1 p1) {
#if THREAD_SAFE || HOTFIX_ENABLE
			lock(luaEnv.luaEnvLock) {
#endif
				var L = luaEnv.L;
				var translator = luaEnv.translator;
				int oldTop = LuaAPI.lua_gettop(L);
				int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				LuaAPI.lua_getref(L, luaReference);
				translator.PushByType(L, p1);
				int error = LuaAPI.lua_pcall(L, 1, 1, errFunc);
				if (error != 0)
					luaEnv.ThrowExceptionFromError(oldTop);
				TResult ret;
				try {
					translator.Get(L, -1, out ret);
				} catch (Exception e) {
					throw e;
				} finally {
					LuaAPI.lua_settop(L, oldTop);
				}
				return ret;
#if THREAD_SAFE || HOTFIX_ENABLE
			}
#endif
		}

		public TResult Func<T1, T2, TResult>(T1 p1, T2 p2) {
#if THREAD_SAFE || HOTFIX_ENABLE
			lock(luaEnv.luaEnvLock) {
#endif
				var L = luaEnv.L;
				var translator = luaEnv.translator;
				int oldTop = LuaAPI.lua_gettop(L);
				int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				LuaAPI.lua_getref(L, luaReference);
				translator.PushByType(L, p1);
				translator.PushByType(L, p2);
				int error = LuaAPI.lua_pcall(L, 2, 1, errFunc);
				if (error != 0)
					luaEnv.ThrowExceptionFromError(oldTop);
				TResult ret;
				try {
					translator.Get(L, -1, out ret);
				} catch (Exception e) {
					throw e;
				} finally {
					LuaAPI.lua_settop(L, oldTop);
				}
				return ret;
#if THREAD_SAFE || HOTFIX_ENABLE
			}
#endif
		}

		public TResult Func<T1, T2, T3, TResult>(T1 p1, T2 p2, T3 p3) {
#if THREAD_SAFE || HOTFIX_ENABLE
			lock(luaEnv.luaEnvLock) {
#endif
				var L = luaEnv.L;
				var translator = luaEnv.translator;
				int oldTop = LuaAPI.lua_gettop(L);
				int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				LuaAPI.lua_getref(L, luaReference);
				translator.PushByType(L, p1);
				translator.PushByType(L, p2);
				translator.PushByType(L, p3);
				int error = LuaAPI.lua_pcall(L, 3, 1, errFunc);
				if (error != 0)
					luaEnv.ThrowExceptionFromError(oldTop);
				TResult ret;
				try {
					translator.Get(L, -1, out ret);
				} catch (Exception e) {
					throw e;
				} finally {
					LuaAPI.lua_settop(L, oldTop);
				}
				return ret;
#if THREAD_SAFE || HOTFIX_ENABLE
			}
#endif
		}

		public TResult Func<T1, T2, T3, T4, TResult>(T1 p1, T2 p2, T3 p3, T4 p4) {
#if THREAD_SAFE || HOTFIX_ENABLE
			lock(luaEnv.luaEnvLock) {
#endif
				var L = luaEnv.L;
				var translator = luaEnv.translator;
				int oldTop = LuaAPI.lua_gettop(L);
				int errFunc = LuaAPI.load_error_func(L, luaEnv.errorFuncRef);
				LuaAPI.lua_getref(L, luaReference);
				translator.PushByType(L, p1);
				translator.PushByType(L, p2);
				translator.PushByType(L, p3);
				translator.PushByType(L, p4);
				int error = LuaAPI.lua_pcall(L, 4, 1, errFunc);
				if (error != 0)
					luaEnv.ThrowExceptionFromError(oldTop);
				TResult ret;
				try {
					translator.Get(L, -1, out ret);
				} catch (Exception e) {
					throw e;
				} finally {
					LuaAPI.lua_settop(L, oldTop);
				}
				return ret;
#if THREAD_SAFE || HOTFIX_ENABLE
			}
#endif
		}
	}
}

