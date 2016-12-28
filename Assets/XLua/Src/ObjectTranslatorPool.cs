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
	internal class ObjectTranslatorPool
	{
		private static volatile ObjectTranslatorPool instance = new ObjectTranslatorPool ();		
		private Dictionary<RealStatePtr, WeakReference> translators = new Dictionary<RealStatePtr, WeakReference>();
		
		public static ObjectTranslatorPool Instance
		{
			get
			{
				return instance;
			}
		}
		
		public ObjectTranslatorPool ()
		{
		}
		
		public void Add (RealStatePtr L, ObjectTranslator translator)
		{
			translators.Add(L , new WeakReference(translator));			
		}

        RealStatePtr lastState = default(RealStatePtr);
        ObjectTranslator lastTranslator = default(ObjectTranslator);

		public ObjectTranslator Find (RealStatePtr L)
		{
            if (lastState == L) return lastTranslator;
            if (translators.ContainsKey(L))
            {
                lastState = L;
                lastTranslator = translators[L].Target as ObjectTranslator;
                return lastTranslator;
            }

			RealStatePtr main = Utils.GetMainState (L);

            if (translators.ContainsKey(main))
            {
                lastState = L;
                lastTranslator = translators[main].Target as ObjectTranslator;
                translators[L] = new WeakReference(lastTranslator);
                return lastTranslator;
            }
			
			return null;
		}
		
		public void Remove (RealStatePtr L)
		{
			if (!translators.ContainsKey (L))
				return;
			
            if (lastState == L)
            {
                lastState = default(RealStatePtr);
                lastTranslator = default(ObjectTranslator);
            }
            ObjectTranslator translator = translators[L].Target as ObjectTranslator;
            List<RealStatePtr> toberemove = new List<RealStatePtr>();

            foreach(var kv in translators)
            {
                if ((kv.Value.Target as ObjectTranslator) == translator)
                {
                    toberemove.Add(kv.Key);
                }
            }

            foreach (var ls in toberemove)
            {
                translators.Remove(ls);
            }
        }
    }
}

