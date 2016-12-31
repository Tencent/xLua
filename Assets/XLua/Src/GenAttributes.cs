/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using System.Collections.Generic;

namespace XLua
{
    public enum GenFlag
    {
        No = 0,
        GCOptimize = 1
    }

    //如果你要生成Lua调用CSharp的代码，加这个标签
    public class LuaCallCSharpAttribute : Attribute
    {
        GenFlag flag;
        public GenFlag Flag {
            get
            {
                return flag;
            }
        }

        public LuaCallCSharpAttribute(GenFlag flag = GenFlag.No)
        {
            this.flag = flag;
        }
    }

    //生成CSharp调用Lua，加这标签
    //[AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface)]
    public class CSharpCallLuaAttribute : Attribute
    {
    }

    //如果某属性、方法不需要生成，加这个标签
    public class BlackListAttribute : Attribute
    {

    }

    //如果想对struct生成免GC代码，加这个标签
    public class GCOptimizeAttribute : Attribute
    {

    }

    //如果想在反射下使用，加这个标签
    public class ReflectionUseAttribute : Attribute
    {

    }

    public class AdditionalPropertiesAttribute : Attribute
    {

    }

    public enum HotfixFlag
    {
        Stateless = 0,
        Stateful = 1,
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotfixAttribute : Attribute
    {
        HotfixFlag flag;
        public HotfixFlag Flag
        {
            get
            {
                return flag;
            }
        }

        public HotfixAttribute(HotfixFlag e = HotfixFlag.Stateless)
        {
            flag = e;
        }
    }

    [AttributeUsage(AttributeTargets.Delegate)]
    internal class HotfixDelegateAttribute : Attribute
    {
    }

    public static class SysGenConfig
    {
        [GCOptimize]
        static List<Type> GCOptimize
        {
            get
            {
                return new List<Type>() {
                    typeof(UnityEngine.Vector2),
                    typeof(UnityEngine.Vector3),
                    typeof(UnityEngine.Vector4),
                    typeof(UnityEngine.Color),
                    typeof(UnityEngine.Quaternion),
                    typeof(UnityEngine.Ray),
                    typeof(UnityEngine.Bounds),
                    typeof(UnityEngine.Ray2D),
                };
            }
        }

        [AdditionalProperties]
        static Dictionary<Type, List<string>> AdditionalProperties
        {
            get
            {
                return new Dictionary<Type, List<string>>()
                {
                    { typeof(UnityEngine.Ray), new List<string>() { "origin", "direction" } },
                    { typeof(UnityEngine.Ray2D), new List<string>() { "origin", "direction" } },
                    { typeof(UnityEngine.Bounds), new List<string>() { "center", "extents" } },
                };
            }
        }
    }
}


