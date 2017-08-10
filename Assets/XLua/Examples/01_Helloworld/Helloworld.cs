/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using XLua;
using System;
using System.Reflection;
using System.Linq;

using System.Collections.Generic;

public class BBClass
{
    public void AB()
    {
        UnityEngine.Debug.Log("BBClass.AB");
    }

    public virtual void CD()
    {
        UnityEngine.Debug.Log("BBClass.CD");
    }
}

public class HelloCall
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public int Add(int a)
    {
        return a;
    }
}

public class DDClass : BBClass
{
    public void AB()
    {
        UnityEngine.Debug.Log("DDClass.AB");
    }


    public override void CD()
    {
        UnityEngine.Debug.Log("DDClass.CD");
    }
}

[CSharpCallLua]

public delegate void AAA(System.IntPtr p, System.UIntPtr p2, out object o);

public delegate void MyAction();

public class TestTestTest
{
    ~TestTestTest()
    {
        Debug.Log("TestTestTest");

    }
}

public interface IHehe
{
    void Foo();
}

[LuaCallCSharp]
public struct Hehe : IHehe
{
    public void Foo()
    {

    }
}

[LuaCallCSharp]
public static class TestExtension
{
    public static void AddInput<U, V>(this U p1, V p2) where U :struct, IHehe where V :struct, IHehe
    {

    }

    public static void Des<U>(this U p1) where U : struct, IHehe
    {

    }
}

public class ABCDE
{
    public int AAA;

    public void Foo(int a)
    {
        Debug.Log("Abc.Foo :" + a + ", AAA" + a);
    }

    public static void SFoo(int a)
    {
        Debug.Log("Abc.SFoo :" + a);
    }

    void Bar(double b)
    {
        
    }

    public static Delegate CreateDelegate(Type type, object firstArgument, MethodInfo method)
    {
        return Delegate.CreateDelegate(type, firstArgument, method);
    }
}

public static class CFG
{
    [LuaCallCSharp]
    public static List<Type> a = new List<Type>()
    {
        typeof(Delegate),
        typeof(ABCDE)
    };
}

public delegate void MyDelegate(int a);

[LuaCallCSharp]
public class MyInt
{
    private int m_value;

    public int Value
    {
        get { return m_value; }
        set { m_value = value; }
    }

    public MyInt(int value)
    {
        m_value = value;
    }

    public static MyInt operator &(MyInt a, MyInt b)
    {
        MyInt c = new MyInt(0);
        c.Value = a.Value & b.Value;
        return c;
    }

    public static MyInt operator |(MyInt a, MyInt b)
    {
        MyInt c = new MyInt(0);
        c.Value = a.Value | b.Value;
        return c;
    }

    public static MyInt operator ^(MyInt a, MyInt b)
    {
        MyInt c = new MyInt(0);
        c.Value = a.Value ^ b.Value;
        return c;
    }

    public static MyInt operator ~(MyInt a)
    {
        MyInt c = new MyInt(0);
        c.Value = ~a.Value;
        return c;
    }

    public static MyInt operator <<(MyInt a, int s)
    {
        MyInt c = new MyInt(0);
        c.Value = a.Value << s;
        return c;
    }

    public static MyInt operator >>(MyInt a, int s)
    {
        MyInt c = new MyInt(0);
        c.Value = a.Value >> s;
        return c;
    }
}

enum LargeEnum
{
    A = 1
}

[LuaCallCSharp]
public class Helloworld : MonoBehaviour {
    static public string CCC;
    public const string a = "test";

    public static int? AAA = null;

    public static void ToMyType(object number, ref object CNum)
    {

    }

    public static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
    {
        return true;
    }

    public static bool SetClass<T>(ref T currentValue, T newValue) where T : class
    {
        return true;
    }

    public int PP { get; private set; }

    [Obsolete("hehe")]
    public void T1() { }

    [Obsolete("hehe2", true)]
    public void T2() { }

    public System.Action act;

    public MyDelegate md;

    public void TestMd()
    {
        md(100);
    }

    public void Foo()
    {
        if (XLua.HotfixDelegateBridge.xlua_get_hotfix_flag(0))
        {
            Debug.Log("Bar");
        }
        Debug.Log("Foo");
    }

    public void CallCC(LuaTable scriptEnv)
    {
        Action cc = scriptEnv.Get<Action>("cc");
        cc();
        cc = null;
    }
	// Use this for initialization
	void Start () {
        LuaEnv luaenv = new LuaEnv();
        Debug.Log(typeof(Dictionary<string, UnityEngine.Object>).AssemblyQualifiedName);
        foreach(var t in typeof(DG.Tweening.Core.TweenerCore<,,>).GetGenericArguments())
        {
            Debug.Log(t);
            foreach(var c in t.GetGenericParameterConstraints())
            {
                Debug.Log("c------" + c);
            }

            Debug.Log("========" + t.GenericParameterAttributes);
        }

        Debug.Log("++++++++++++++++++++++++++++++++++");

        foreach (var t in typeof(ITestConstrains<>).GetGenericArguments())
        {
            Debug.Log(t);
            foreach (var c in t.GetGenericParameterConstraints())
            {
                Debug.Log("c------" + c);
            }
            
            Debug.Log("========" + t.GenericParameterAttributes);
        }
        //typeof(ABCDE).GetMethod()
        luaenv.Global.Set("self", this);
        luaenv.DoString(@"
local m = typeof(CS.ABCDE):GetMethod('Foo', {typeof(CS.System.Int32)})
print(m)
m = typeof(CS.ABCDE):GetMethod('Bar', {typeof(CS.System.Double)})
print(m)
m = typeof(CS.ABCDE):GetMethod('Bar', CS.System.Reflection.BindingFlags.Public | CS.System.Reflection.BindingFlags.NonPublic
  | CS.System.Reflection.BindingFlags.Instance | CS.System.Reflection.BindingFlags.Static)
print(m)

m = typeof(CS.ABCDE):GetMethod('Bar', CS.System.Reflection.BindingFlags.Public | CS.System.Reflection.BindingFlags.NonPublic
  | CS.System.Reflection.BindingFlags.Instance | CS.System.Reflection.BindingFlags.Static, nil, {typeof(CS.System.Double)}, nil)
print(m)

print((CS.MyInt(2) & CS.MyInt(3)).Value)
print((CS.MyInt(2) | CS.MyInt(3)).Value)
print((CS.MyInt(2)  ~ CS.MyInt(3)).Value)
print((~ CS.MyInt(3)).Value)
print((CS.MyInt(3) >> 1).Value)
print((CS.MyInt(3) << 1).Value)

print(CS.System.Reflection.BindingFlags.Public, CS.System.Reflection.BindingFlags.NonPublic)
print(CS.System.Reflection.BindingFlags.Public | CS.System.Reflection.BindingFlags.NonPublic)

local util = require 'xlua.util'

local abcd = CS.ABCDE()

local d = util.createdelegate(CS.MyDelegate, abcd, CS.ABCDE, 'Foo', {typeof(CS.System.Int32)})
local d2 = util.createdelegate(CS.MyDelegate, nil, CS.ABCDE, 'SFoo', {typeof(CS.System.Int32)})
self.md = d + d2
self:TestMd()

--print(CS.System.Type.GetType('System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[UnityEngine.Object, UnityEngine, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'))
--print(CS.System.Type.GetType('System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[UnityEngine.Object, UnityEngine]], mscorlib'))
--print(CS.System.Type.GetType('System.Collections.Generic.Dictionary`2[System.String,[UnityEngine.Object, UnityEngine]]'))
--print(xlua.getmetatable('System.Collections.Generic.Dictionary`2[System.String,[UnityEngine.Object, UnityEngine]]'))
--xlua.getmetatable(CS.System.Collections.Generic['Dictionary`2[System.String,[UnityEngine.Object, UnityEngine]]'])
");
        
        

        //typeof(ABCDE).GetMethod()
        luaenv.Dispose();
        /*var m = typeof(TestExtension).GetMethod("AddInput");
        foreach(var p in m.GetParameters())
        {
            foreach(var c in p.Get)
        }*/

        //LuaEnv luaenv = new LuaEnv();
        //luaenv.DoString("local a = CS.UnityEngine.Quaternion(0,0,0) ");
        //luaenv.Global.Set("self", this);
        //Debug.Log(luaenv.Global.ContainsKey("self"));
        //Debug.Log(luaenv.Global.ContainsKey("self1"));
        //for(int i = 0; i< 100000;i++)
        //{
        //    luaenv.Global.ContainsKey("self");
        //    luaenv.Global.ContainsKey("self1");
        //}
        //Debug.Log(luaenv.Global.ContainsKey("self"));
        //Debug.Log(luaenv.Global.ContainsKey("self1"));

        /*
        LuaTable scriptEnv = luaenv.NewTable();
        LuaTable meta = luaenv.NewTable();
        meta.Set("__index", luaenv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();*/

        //var tg = new TestGC(444);
        //luaenv.Global.Set("a", tg);
        //luaenv.Global.Set<string, object>("a", null);
        //tg = null;

        //new Int32();
        //luaenv.Global.Set("self", this);
        //luaenv.Global.Set("l", new List<int>());
        //luaenv.Global.Set("d1", new Dictionary<int,int>());
        //luaenv.Global.Set("d2", new Dictionary<string, int>());
        //luaenv.Global.Set("d3", new Dictionary<string, string>());
        /*luaenv.DoString(@"
CS.UnityEngine.Debug.Log('hello world')
--local o = CS.UnityEngine.GameObject.Find('Main Camera')
--o:StopAnimation()
--print(self.PP)
--local util = require 'xlua.util'
--xlua.private_accessible(CS.Helloworld)
--self.PP = 10
--print(self.PP)

--l:Add(1)
--d1:Add(2, 5)
--d2:Add('h', 4)
--d3:Add('j', 'o')
--iprint('hehe', 1, 2, 3)
");*/
        //UnityEngine.GameObject.

        //LuaFunction geta = luaenv.Global.Get<LuaFunction>("ReturnA");
        //Debug.Log(geta.Call()[0]);
        //LuaTable tbl = luaenv.NewTable();
        //geta.SetEnv(tbl);
        //Debug.Log(geta.Call()[0]);


        //Action cc = scriptEnv.Get<Action>("cc");
        //cc();
        //cc = null;
        //CallCC(scriptEnv);

        //LuaFunction cc = luaenv.Global.Get<LuaFunction>("cc");
        //luaenv.PrintTop();
        //cc.Call(1);
        //cc.Call(3214);
        //luaenv.PrintTop();
        //cc.Call(2435423);
        //luaenv.PrintTop();
        //cc.Call(43151);
        //luaenv.PrintTop();
        //cc.Dispose();

        //scriptEnv.Dispose();

        //System.GC.Collect();
        //System.GC.WaitForPendingFinalizers();
        //luaenv.Tick();
        //luaenv.FullGc();
        //System.GC.Collect();
        //System.GC.WaitForPendingFinalizers();
        ////luaenv.FullGc();
        //Debug.Log("-------------------");
        //luaenv.Dispose();
        //UnityEngine.Debug.Log("a:" + (typeof(int) == typeof(int?)));
        //var t = new UnsafeTest();
        //t.UnsafeSet(2, 10);
        //Debug.Log(t.UnsafeGet(2));

        //BBClass bb = new DDClass();
        //bb.AB();
        //bb.CD();
        //(bb as DDClass).AB();
        //(bb as DDClass).CD();
        //foreach(var type in Assembly.Load("Assembly-CSharp").GetTypes())
        //{
        //    var bindingAttrOfMethod = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic;
        //    // 
        //    if (type.Namespace != null && type.Namespace.StartsWith("XLua") && 
        //        type.GetConstructors(bindingAttrOfMethod).Any(m => m.Name == ".cctor" && m.IsSpecialName))
        //    {
        //        Debug.LogWarning(type.ToString() + " ok??");
        //        foreach (var method in (type.GetMethods(bindingAttrOfMethod).Where(m => m.IsStatic && !m.IsConstructor)))
        //        {
        //            Debug.Log("type:" + method.DeclaringType + ",method:" + method);
        //        }
        //    }
        //    else
        //    {
        //        //Debug.Log(type.ToString() + " ok");
        //    }
        //}

        //Array arr = new int[] { 1 };
        //Debug.Log(arr.GetValue(1));
        //var type = typeof(Dictionary<,>);
        //Debug.Log("B:" + type.IsGenericTypeDefinition);
        //foreach(var garg in type.GetGenericArguments())
        //{
        //    Debug.Log(garg + ",p:"+ garg.DeclaringType+ ",m:" + garg.DeclaringMethod);
        //}

        //Debug.Log("B:" + typeof(System.Collections.Generic.IEqualityComparer<>));
        //Debug.Log("B:" + typeof(Dictionary<int, int>));
    }
	
	// Update is called once per frame
	void Update () {
	    if (getInjection(1) != null)
        {
            var injection = getInjection(1) as MyAction;
            injection();
        }
	}

    static object[] injectionList = null;

    object getInjection(int idx)
    {
        if (injectionList == null || idx >= injectionList.Length)
        {
            return null;
        }
        else
        {
            return injectionList[idx];
        }
    }


}

[Hotfix]
public class ObjectPool
{
    public static ObjectPool get_Instance()
    {
        return null;
    }
}



