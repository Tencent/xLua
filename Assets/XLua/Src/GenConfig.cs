/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using System.Collections.Generic;
using System;

namespace XLua
{
    //注意：用户自己代码不建议在这里配置，建议通过标签来声明!!
    public interface GenConfig 
    {
        //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
        List<Type> LuaCallCSharp { get; }

        //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
        List<Type> CSharpCallLua { get; }

        //黑名单
        List<List<string>> BlackList { get; }
    }

    public interface GCOptimizeConfig
    {
        List<Type> TypeList { get; }
        Dictionary<Type, List<string>> AdditionalProperties { get; }
    }

    public interface ReflectionConfig
    {
        List<Type> ReflectionUse { get; }
    }
}