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
            throw new InvalidCastException("This delegate must add to CSharpCallLua: " + type);
        }
    }

    public partial class DelegateBridge : DelegateBridgeBase
    {
        public static object DelegateBridgeLock = new object();

        internal static bool Gen_Flag = false;

        public DelegateBridge(int reference, LuaEnv luaenv) : base(reference, luaenv)
        {
        }
    }
}
