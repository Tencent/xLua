/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using System.Collections;
using XLua;

public class InvokeLua : MonoBehaviour
{
    [CSharpCallLua]
    public interface ICalc
    {
        int Add(int a, int b);
        int Mult { get; set; }
    }

    [CSharpCallLua]
    public delegate ICalc CalcNew(int mult, params string[] arg);

    private string script = @"
                local calc_mt = {
                    __index = {
                        Add = function(self, a, b)
                            return (a + b) * self.Mult
                        end
                    }
                }

                Calc = {
	                New = function (mult, ...)
                        print(...)
                        return setmetatable({Mult = mult}, calc_mt)
                    end
                }
	        ";
    // Use this for initialization
    void Start()
    {
        LuaEnv luaenv = new LuaEnv();
        luaenv.DoString(script);
        CalcNew calc_new = luaenv.Global.GetInPath<CalcNew>("Calc.New");
        ICalc calc = calc_new(10, "hi", "john"); //constructor
        Debug.Log("sum(*10) =" + calc.Add(1, 2));
        calc.Mult = 100;
        Debug.Log("sum(*100)=" + calc.Add(1, 2));
        luaenv.Dispose();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
