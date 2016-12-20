/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;

public class CSCallLua : MonoBehaviour {
    LuaEnv luaenv = null;
    string script = @"
        a = 1
        b = 'hello world'
        c = true

        d = {
           f1 = 12, f2 = 34, 
           1, 2, 3,
           add = function(self, a, b) 
              print('d.add called')
              return a + b 
           end
        }

        function e()
            print('i am e')
        end

        function f(a, b)
            print('a', a, 'b', b)
            return 1, {f1 = 1024}
        end
        
        function ret_e()
            print('ret_e called')
            return e
        end
    ";

    public class DClass
    {
        public int f1;
        public int f2;
    }
    
    [CSharpCallLua]
    public interface ItfD
    {
        int f1 { get; set; }
        int f2 { get; set; }
        int add(int a, int b);
    }

    [CSharpCallLua]
    public delegate int FDelegate(int a, string b, out DClass c);

    [CSharpCallLua]
    public delegate Action GetE();

    // Use this for initialization
    void Start()
    {
        luaenv = new LuaEnv();
        luaenv.DoString(script);

        Debug.Log("_G.a = " + luaenv.Global.Get<int>("a"));
        Debug.Log("_G.b = " + luaenv.Global.Get<string>("b"));
        Debug.Log("_G.c = " + luaenv.Global.Get<bool>("c"));


        DClass d = luaenv.Global.Get<DClass>("d");//映射到有对应字段的class，by value
        Debug.Log("_G.d = {f1=" + d.f1 + ", f2=" + d.f2 + "}");

        Dictionary<string, double> d1 = luaenv.Global.Get<Dictionary<string, double>>("d");//映射到Dictionary<string, double>，by value
        Debug.Log("_G.d = {f1=" + d1["f1"] + ", f2=" + d1["f2"] + "}, d.Count=" + d1.Count);

        List<double> d2 = luaenv.Global.Get<List<double>>("d"); //映射到List<double>，by value
        Debug.Log("_G.d.len = " + d2.Count);

        ItfD d3 = luaenv.Global.Get<ItfD>("d"); //映射到interface实例，by ref，这个要求interface加到生成列表，否则会返回null，建议用法
        d3.f2 = 1000;
        Debug.Log("_G.d = {f1=" + d3.f1 + ", f2=" + d3.f2 + "}");
        Debug.Log("_G.d:add(1, 2)=" + d3.add(1, 2));

        LuaTable d4 = luaenv.Global.Get<LuaTable>("d");//映射到LuaTable，by ref
        Debug.Log("_G.d = {f1=" + d4.Get<int>("f1") + ", f2=" + d4.Get<int>("f2") + "}");


        Action e = luaenv.Global.Get<Action>("e");//映射到一个delgate，要求delegate加到生成列表，否则返回null，建议用法
        e();

        FDelegate f = luaenv.Global.Get<FDelegate>("f");
        DClass d_ret;
        int f_ret = f(100, "John", out d_ret);//lua的多返回值映射：从左往右映射到c#的输出参数，输出参数包括返回值，out参数，ref参数
        Debug.Log("ret.d = {f1=" + d_ret.f1 + ", f2=" + d_ret.f2 + "}, ret=" + f_ret);

        GetE ret_e = luaenv.Global.Get<GetE>("ret_e");//delegate可以返回更复杂的类型，甚至是另外一个delegate
        e = ret_e();
        e();

        LuaFunction d_e = luaenv.Global.Get<LuaFunction>("e");
        d_e.Call();

    }

    // Update is called once per frame
    void Update()
    {
        if (luaenv != null)
        {
            luaenv.Tick();
        }
    }

    void OnDestroy()
    {
        luaenv.Dispose();
    }
}
