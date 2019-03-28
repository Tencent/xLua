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

namespace XLuaTest
{
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
                            self:notify({name = index, value = value})
                        end,
                        
                        add_PropertyChanged = function(self, delegate)
	                        if self.notifylist == nil then
		                        self.notifylist = {}
	                        end
	                        table.insert(self.notifylist, delegate)
                            print('add',delegate)
                        end,
                                                
                        remove_PropertyChanged = function(self, delegate)
                            for i=1, #self.notifylist do
		                        if CS.System.Object.Equals(self.notifylist[i], delegate) then
			                        table.remove(self.notifylist, i)
			                        break
		                        end
	                        end
                            print('remove', delegate)
                        end,

                        notify = function(self, evt)
	                        if self.notifylist ~= nil then
		                        for i=1, #self.notifylist do
			                        self.notifylist[i](self, evt)
		                        end
	                        end	
                        end,
                    }
                }

                Calc = {
	                New = function (mult, ...)
                        print(...)
                        return setmetatable({Mult = mult, list = {'aaaa','bbbb','cccc'}}, calc_mt)
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
            calc[1] = "dddd";
            Debug.Log("list[1]=" + calc[1]);

            calc.PropertyChanged -= Notify;

            calc[1] = "eeee";
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
}