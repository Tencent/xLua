/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System;
using UnityEngine;
using XLua;

[CSharpCallLua]
public delegate void EventHandler<TEvent>(object sender, TEvent e) where TEvent : EventArgs;

[CSharpCallLua]
public class PropertyChangedEventArgs : EventArgs
{
    public string name;
    public object value;
}

public class InvokeLua : MonoBehaviour
{
    [CSharpCallLua]
    public interface ICalc
    {
        event EventHandler<PropertyChangedEventArgs> PropertyChanged;

        int Add(int a, int b);
        int Mult { get; set; }

        object this[int index] { get; set; }
    }

    [CSharpCallLua]
    public delegate ICalc CalcNew(int mult, params string[] args);

    private string script = @"
                local calc_mt = {
                    __index = {
                        Add = function(self, a, b)
                            return (a + b) * self.Mult
                        end,
                        
                        get_Item = function(self, index)
                            return self.list[index + 1]
                        end,

                        set_Item = function(self, index, value)
                            self.list[index + 1] = value
                            if self.notify ~= nil then
                                self.notify(self, {name = index, value = value})
                            end
                        end,
                        
                        add_PropertyChanged = function(self, delegate)
                            self.notify = delegate
                        end,
                                                
                        remove_PropertyChanged = function(self, delegate)
                            self.notify = nil
                        end,
                    }
                }

                Calc = {
	                New = function (mult, ...)
                        print(...)
                        return setmetatable({Mult = mult, list = {'a','b','c'}}, calc_mt)
                    end
                }
	        ";
    // Use this for initialization
    void Start()
    {
        LuaEnv luaenv = new LuaEnv();
        Test(luaenv);//调用了带可变参数的delegate，函数结束都不会释放delegate，即使置空并调用GC
        luaenv.Dispose();
    }

    void Test(LuaEnv luaenv)
    {
        luaenv.DoString(script);
        CalcNew calc_new = luaenv.Global.GetInPath<CalcNew>("Calc.New");
        ICalc calc = calc_new(10, "hi", "john"); //constructor
        Debug.Log("sum(*10) =" + calc.Add(1, 2));
        calc.Mult = 100;
        Debug.Log("sum(*100)=" + calc.Add(1, 2));

        Debug.Log("list[0]=" + calc[0]);
        Debug.Log("list[1]=" + calc[1]);
        
        calc.PropertyChanged += Notify;

        calc[1] = "d";
        Debug.Log("list[1]=" + calc[1]);

        calc.PropertyChanged -= Notify;

        calc[1] = "e";
        Debug.Log("list[1]=" + calc[1]);
    }

    void Notify(object sender, PropertyChangedEventArgs e)
    {
        Debug.Log(string.Format("{0} has property changed {1}={2}", sender, e.name, e.value));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
