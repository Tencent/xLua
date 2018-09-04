/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using System.Collections;
using System;
using XLua;
using System.Collections.Generic;

namespace Tutorial
{
	[LuaCallCSharp]
	public class BaseClass
	{
		public static void BSFunc()
		{
			Debug.Log("Driven Static Func, BSF = " + BSF);
		}

		public static int BSF = 1;

		public void BMFunc()
		{
			Debug.Log("Driven Member Func, BMF = " + BMF);
		}

		public int BMF { get; set; }
	}

	public struct Param1
	{
		public int x;
		public string y;
	}

	[LuaCallCSharp]
	public enum TestEnum
	{
		E1,
		E2
	}

	[LuaCallCSharp]
	public class PrivateOverrideClass
	{

		public void TestFunc(int i)
		{
			Debug.Log("TestFunc(int i), i = " + i);
		}

		public void TestFunc(string i)
		{
			Debug.Log("TestFunc(string i), i = " + i);
		}

		private void TestFunc(double i)
		{
			Debug.Log("TestFunc(double i), i = " + i);
		}

		public void TestFunc3(int i)
		{
			Debug.Log("TestFunc(int i), i = " + i);
		}

		public void TestFunc2(string i)
		{
			Debug.Log("TestFunc(string i), i = " + i);
		}

		private void TestFunc2(double i)
		{
			Debug.Log("TestFunc(double i), i = " + i);
		}
	}

	[LuaCallCSharp]
	public class DrivenClass : BaseClass
	{
		[LuaCallCSharp]
		public enum TestEnumInner
		{
			E3,
			E4
		}

		public void DMFunc()
		{
			Debug.Log("Driven Member Func, DMF = " + DMF);
		}

		public int DMF { get; set; }

		public double ComplexFunc(Param1 p1, ref int p2, out string p3, Action luafunc, out Action csfunc)
		{
			Debug.Log("P1 = {x=" + p1.x + ",y=" + p1.y + "},p2 = " + p2);
			luafunc();
			p2 = p2 * p1.x;
			p3 = "hello " + p1.y;
			csfunc = () =>
			{
				Debug.Log("csharp callback invoked!");
			};
			return 1.23;
		}

		public void TestFunc(int i)
		{
			Debug.Log("TestFunc(int i)");
		}

		public void TestFunc(string i)
		{
			Debug.Log("TestFunc(string i)");
		}

		public static DrivenClass operator +(DrivenClass a, DrivenClass b)
		{
			DrivenClass ret = new DrivenClass();
			ret.DMF = a.DMF + b.DMF;
			return ret;
		}

		public void DefaultValueFunc(int a = 100, string b = "cccc", string c = null)
		{
			UnityEngine.Debug.Log("DefaultValueFunc: a=" + a + ",b=" + b + ",c=" + c);
		}

		public void VariableParamsFunc(int a, params string[] strs)
		{
			UnityEngine.Debug.Log("VariableParamsFunc: a =" + a);
			foreach (var str in strs)
			{
				UnityEngine.Debug.Log("str:" + str);
			}
		}

		public TestEnum EnumTestFunc(TestEnum e)
		{
			Debug.Log("EnumTestFunc: e=" + e);
			return TestEnum.E2;
		}

		public Action<string> TestDelegate = (param) =>
		{
			Debug.Log("TestDelegate in c#:" + param);
		};

		public event Action TestEvent;

		public void CallEvent()
		{
			TestEvent();
		}

		public ulong TestLong(long n)
		{
			return (ulong)(n + 1);
		}

		class InnerCalc : ICalc
		{
			public int add(int a, int b)
			{
				return a + b;
			}

			public int id = 100;
		}

		public ICalc GetCalc()
		{
			return new InnerCalc();
		}

		public void GenericMethod<T>()
		{
			Debug.Log("GenericMethod<" + typeof(T) + ">");
		}
	}

	[LuaCallCSharp]
	public interface ICalc
	{
		int add(int a, int b);
	}

	[LuaCallCSharp]
	public static class DrivenClassExtensions
	{
		public static int GetSomeData(this DrivenClass obj)
		{
			Debug.Log("GetSomeData ret = " + obj.DMF);
			return obj.DMF;
		}

		public static int GetSomeBaseData(this BaseClass obj)
		{
			Debug.Log("GetSomeBaseData ret = " + obj.BMF);
			return obj.BMF;
		}

		public static void GenericMethodOfString(this DrivenClass obj)
		{
			obj.GenericMethod<string>();
		}
	}
}

public class LuaCallCs : MonoBehaviour
{
	LuaEnv luaenv = null;
	string script = @"
        function demo()
            --new C#对象
            local newGameObj = CS.UnityEngine.GameObject()
            local newGameObj2 = CS.UnityEngine.GameObject('helloworld')
            print(newGameObj, newGameObj2)
        
            --访问静态属性，方法
            local GameObject = CS.UnityEngine.GameObject
            print('UnityEngine.Time.deltaTime:', CS.UnityEngine.Time.deltaTime) --读静态属性
            CS.UnityEngine.Time.timeScale = 0.5 --写静态属性
            print('helloworld', GameObject.Find('helloworld')) --静态方法调用

            --访问成员属性，方法
            local DrivenClass = CS.Tutorial.DrivenClass
            local testobj = DrivenClass()
            testobj.DMF = 1024--设置成员属性
            print(testobj.DMF)--读取成员属性
            testobj:DMFunc()--成员方法

            --基类属性，方法
            print(DrivenClass.BSF)--读基类静态属性
            DrivenClass.BSF = 2048--写基类静态属性
            DrivenClass.BSFunc();--基类静态方法
            print(testobj.BMF)--读基类成员属性
            testobj.BMF = 4096--写基类成员属性
            testobj:BMFunc()--基类方法调用

            --复杂方法调用
            local ret, p2, p3, csfunc = testobj:ComplexFunc({x=3, y = 'john'}, 100, function()
               print('i am lua callback')
            end)
            print('ComplexFunc ret:', ret, p2, p3, csfunc)
            csfunc()

           --重载方法调用
           testobj:TestFunc(100)
           testobj:TestFunc('hello')
		   
		   --xlua.private_accessible访问私有方法
		   local PrivateOverrideClass = CS.Tutorial.PrivateOverrideClass
		   local priObj=PrivateOverrideClass()
		   priObj:TestFunc(100.0)
	       priObj:TestFunc('hello')
		   xlua.private_accessible(PrivateOverrideClass);
		   priObj:TestFunc(100.0)
		   priObj:TestFunc('hello')

           --操作符
           local testobj2 = DrivenClass()
           testobj2.DMF = 2048
           print('(testobj + testobj2).DMF = ', (testobj + testobj2).DMF)

           --默认值
           testobj:DefaultValueFunc(1)
           testobj:DefaultValueFunc(3, 'hello', 'john')

           --可变参数
           testobj:VariableParamsFunc(5, 'hello', 'john')

           --Extension methods
           print(testobj:GetSomeData()) 
           print(testobj:GetSomeBaseData()) --访问基类的Extension methods
           testobj:GenericMethodOfString()  --通过Extension methods实现访问泛化方法

           --枚举类型
           local e = testobj:EnumTestFunc(CS.Tutorial.TestEnum.E1)
           print(e, e == CS.Tutorial.TestEnum.E2)
           print(CS.Tutorial.TestEnum.__CastFrom(1), CS.Tutorial.TestEnum.__CastFrom('E1'))
           print(CS.Tutorial.DrivenClass.TestEnumInner.E3)
           assert(CS.Tutorial.BaseClass.TestEnumInner == nil)

           --Delegate
           testobj.TestDelegate('hello') --直接调用
           local function lua_delegate(str)
               print('TestDelegate in lua:', str)
           end
           testobj.TestDelegate = lua_delegate + testobj.TestDelegate --combine，这里演示的是C#delegate作为右值，左值也支持
           testobj.TestDelegate('hello')
           testobj.TestDelegate = testobj.TestDelegate - lua_delegate --remove
           testobj.TestDelegate('hello')

           --事件
           local function lua_event_callback1() print('lua_event_callback1') end
           local function lua_event_callback2() print('lua_event_callback2') end
           testobj:TestEvent('+', lua_event_callback1)
           testobj:CallEvent()
           testobj:TestEvent('+', lua_event_callback2)
           testobj:CallEvent()
           testobj:TestEvent('-', lua_event_callback1)
           testobj:CallEvent()
           testobj:TestEvent('-', lua_event_callback2)

           --64位支持
           local l = testobj:TestLong(11)
           print(type(l), l, l + 100, 10000 + l)

           --typeof
           newGameObj:AddComponent(typeof(CS.UnityEngine.ParticleSystem))

           --cast
           local calc = testobj:GetCalc()
           print('assess instance of InnerCalc via reflection', calc:add(1, 2))
           assert(calc.id == 100)
           cast(calc, typeof(CS.Tutorial.ICalc))
           print('cast to interface ICalc', calc:add(1, 2))
           assert(calc.id == nil)
       end

       demo()

       --协程下使用
       local co = coroutine.create(function()
           print('------------------------------------------------------')
           demo()
       end)
       assert(coroutine.resume(co))
    ";

	// Use this for initialization
	void Start()
	{
		luaenv = new LuaEnv();
		luaenv.DoString(script);
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
