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

using System.Collections.Generic;
using System;

namespace XLua
{
	public class ObjectTranslatorPool
	{
#if !SINGLE_ENV
        private Dictionary<RealStatePtr, WeakReference> translators = new Dictionary<RealStatePtr, WeakReference>();
        RealStatePtr lastPtr = default(RealStatePtr);
#endif
        ObjectTranslator lastTranslator = default(ObjectTranslator);

        public static ObjectTranslatorPool Instance
		{
			get
			{
				return InternalGlobals.objectTranslatorPool;
			}
		}

#if UNITY_EDITOR || XLUA_GENERAL
        public static ObjectTranslator FindTranslator(RealStatePtr L)
        {
            return InternalGlobals.objectTranslatorPool.Find(L);
        }
#endif

        public ObjectTranslatorPool ()
		{
		}
		
		public void Add (RealStatePtr L, ObjectTranslator translator)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (this)
#endif
            {
                lastTranslator = translator;
#if !SINGLE_ENV
                var ptr = LuaAPI.xlua_gl(L);
                lastPtr = ptr;
                translators.Add(ptr , new WeakReference(translator));
#endif
            }
        }

		public ObjectTranslator Find (RealStatePtr L)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (this)
#endif
            {
#if SINGLE_ENV
                return lastTranslator;
#else
                var ptr = LuaAPI.xlua_gl(L);
                if (lastPtr == ptr) return lastTranslator;
                if (translators.ContainsKey(ptr))
                {
                    lastPtr = ptr;
                    lastTranslator = translators[ptr].Target as ObjectTranslator;
                    return lastTranslator;
                }
                
                return null;
#endif
            }
        }
		
		public void Remove (RealStatePtr L)
		{
#if THREAD_SAFE || HOTFIX_ENABLE
            lock (this)
#endif
            {
#if SINGLE_ENV
                lastTranslator = default(ObjectTranslator);
#else
                var ptr = LuaAPI.xlua_gl(L);
                if (!translators.ContainsKey (ptr))
                    return;
                
                if (lastPtr == ptr)
                {
                    lastPtr = default(RealStatePtr);
                    lastTranslator = default(ObjectTranslator);
                }

                translators.Remove(ptr);
#endif
            }
        }
    }
}

