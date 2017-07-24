using UnityEngine;
using System.Collections;
using XLua;
using System.IO;
using System;

[LuaCallCSharp]
public static class TestUtils
{
    public static bool IsAndroid()
    {
#if UNITY_ANDROID
        return true;
#else
        return false;
#endif
    }
}

[CSharpCallLua]
public delegate void PerfTest(int load);

public class PerfMain : MonoBehaviour {
	string resultPath = "";

	LuaEnv luaenv;

	StreamWriter sw;

    System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

	// Use this for initialization
	void Start () {
#if UNITY_ANDROID && !UNITY_EDITOR
	    resultPath = "/sdcard/testResult_android.log";
#elif UNITY_IPHONE || UNITY_IOS
	    resultPath = Application.persistentDataPath + "/testResult_iOS.log";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
        resultPath = Application.dataPath + "/../testResult_windows.log";
#else
        resultPath = "";
#endif
        var start = Time.realtimeSinceStartup;
        var startMem = System.GC.GetTotalMemory(true);
        luaenv = new LuaEnv();
        Debug.Log("start cost: " + (Time.realtimeSinceStartup - start));
        var endMem = System.GC.GetTotalMemory(true);
        Debug.Log("startMem: " + startMem + ", endMem: " + endMem + ", " + "cost mem: " + (endMem - startMem));
        luaenv.DoString("require 'luaTest'");
    }

    // Update is called once per frame
    void Update () {
	
	}

	void OnGUI()
	{
		if (GUI.Button (new Rect (10, 100, 300, 150), "Start")) {
            FileStream fs = new FileStream(resultPath, FileMode.Create);
            sw = new StreamWriter(fs);

            StartCSCallLua();
			StartLuaCallCS ();
			StartAddRemoveCB ();
			StartCSCallLuaCB ();
			StartConstruct ();

			sw.Close ();
		}
	}

    //------------------------------------------------------------------------------------------------------

    const double TEST_MIN_DURATION = 800;
    const double TEST_DURATION = 1000;

    private int PerformentTest(string title, int load, PerfTest execute)
    {
        stopWatch.Reset();
        stopWatch.Start();
        execute(load);
        stopWatch.Stop();

        /*int load_added = 0;

        if (stopWatch.ElapsedMilliseconds < (TEST_MIN_DURATION))
        {
            double dur_added = TEST_DURATION - stopWatch.ElapsedMilliseconds;
            load_added = (int)(load * (dur_added / stopWatch.ElapsedMilliseconds));
        }

        if (load_added > 0)
        {
            stopWatch.Start();
            execute(load_added);
            stopWatch.Stop();
        }

        int cps = CPS(load + load_added, stopWatch.ElapsedMilliseconds);*/

        int cps = CPS(load, stopWatch.ElapsedMilliseconds);

        if (title != null)
        {
            string log = title + cps + ", elapsed :" + stopWatch.ElapsedMilliseconds;
            Debug.Log(log);
            sw.WriteLine(log);
        }

        return cps;
    }

	private void StartCSCallLua()
	{
        int LOOP_TIMES = 1000000;
        Debug.Log ("C# call lua :");
		sw.WriteLine ("C# call lua :");

		FuncBasePara funcBaseParm = luaenv.Global.Get<FuncBasePara>("FuncBasePara");
        PerformentTest("C# call lua : base parameter function :", LOOP_TIMES, loop_times =>
        {
            for (int i = 0; i < loop_times; i++)
            {
                funcBaseParm(i);
            }
        });

        FuncClassPara funcClassPara = luaenv.Global.Get<FuncClassPara> ("FuncClassPara");
		ParaClass paraClass = new ParaClass ();
        PerformentTest("C# call lua : class parameter function :", LOOP_TIMES, loop_times =>
        {
            for (int i = 0; i < loop_times; i++)
            {
                funcClassPara(paraClass);
            }
        });

        FuncStructPara funcStructPara = luaenv.Global.Get<FuncStructPara> ("FuncStructPara");
		ParaStruct paraStruct = new ParaStruct ();
        PerformentTest("C# call lua : struct parameter function :", LOOP_TIMES, loop_times =>
        {
            for (int i = 0; i < loop_times; i++)
            {
                funcStructPara(paraStruct);
            }
        });

        FuncTwoBasePara funcTwoBasePara = luaenv.Global.Get<FuncTwoBasePara> ("FuncTwoBasePara");
        PerformentTest("C# call lua : two base parameter function :", LOOP_TIMES, loop_times =>
        {
            for (int i = 0; i < loop_times; i++)
            {
                funcTwoBasePara(i, i);
            }
        });

        sw.WriteLine ("C# access lua table : ");

		ITableAccess iTAccess = luaenv.Global.Get<ITableAccess> ("luaTable");
        PerformentTest("C# access lua table : access member, get : ", LOOP_TIMES, loop_times =>
        {
            for (int i = 0; i < loop_times; i++)
            {
                int x = iTAccess.id;
            }
        });

        PerformentTest("C# access lua table : access member, set : ", LOOP_TIMES, loop_times =>
        {
            for (int i = 0; i < loop_times; i++)
            {
                iTAccess.id = 0;
            }
        });

        PerformentTest("C# access lua table : access member function : ", LOOP_TIMES, loop_times =>
        {
            for (int i = 0; i < loop_times; i++)
            {
                iTAccess.func();
            }
        });

    }

	private void StartLuaCallCS()
	{
        int LOOP_TIMES = 1000000;

        Debug.Log ("lua call C# member : ");
		sw.WriteLine ("lua call C# member : ");

		PerfTest func = luaenv.Global.Get<PerfTest> ("LuaAccessCSBaseMember_get");
        PerformentTest("lua call C# member : base member, get : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest>("LuaAccessCSBaseMember_set");
        PerformentTest("lua call C# member : base member, set : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest> ("LuaAccessCSClassMember_get");
        PerformentTest("lua call C# member : class member, get : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest>("LuaAccessCSClassMember_set");
        PerformentTest("lua call C# member : class member, set : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaAccessStructMember_get");
        PerformentTest("lua call C# member : struct member, get : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest>("LuaAccessStructMember_set");
        PerformentTest("lua call C# member : struct member, set : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaAccessVec3Member_get");
        PerformentTest("lua call C# member : vector3 member, get : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest>("LuaAccessVec3Member_set");
        PerformentTest("lua call C# member : vector3 member, set : ", LOOP_TIMES, func);

		Debug.Log ("lua call C# member funtion : ");
		sw.WriteLine ("lua call C# member funtion : ");

		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSBaseMemberFunc");
        PerformentTest("lua call C# member funtion : base parameter member function : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSClassMemberFunc");
        PerformentTest("lua call C# member funtion : class parameter member function : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStructMemberFunc");
        PerformentTest("lua call C# member funtion : struct parameter member function : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSVec3MemberFunc");
        PerformentTest("lua call C# member funtion : vector3 parameter member function : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSInMemberFunc");
        PerformentTest("lua call C# member funtion : input parameter member function : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSOutMemberFunc");
        PerformentTest("lua call C# member funtion : output parameter member function : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSInOutMemberFunc");
        PerformentTest("lua call C# member funtion : in & output parameter member function : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSTwoMemberFunc");
        PerformentTest("lua call C# member funtion : two parameter member function : ", LOOP_TIMES, func);

		Debug.Log ("lua call static memeber : ");
		sw.WriteLine ("lua call static memeber :");

		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticBaseMember_get");
        PerformentTest("lua call C# static member : base member, get : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest>("LuaAccessCSStaticBaseMember_set");
        PerformentTest("lua call C# static member : base member, set : ", LOOP_TIMES, func);
		
		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticClassMember_get");
        PerformentTest("lua call C# static member : class member, get : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest>("LuaAccessCSStaticClassMember_set");
        PerformentTest("lua call C# static member : class member, set : ", LOOP_TIMES, func);
		
		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticStructMember_get");
        PerformentTest("lua call C# static member : struct member, get : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest>("LuaAccessCSStaticStructMember_set");
        PerformentTest("lua call C# static member : struct member, set : ", LOOP_TIMES, func);
		
		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticVec3Member_get");
        PerformentTest("lua call C# static member : vector3 member, get : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest>("LuaAccessCSStaticVec3Member_set");
        PerformentTest("lua call C# static member : vector3 member, set : ", LOOP_TIMES, func);

		Debug.Log ("lua call C# static member funtion : ");
		sw.WriteLine ("lua call C# member funtion : ");
		
		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticBaseMemberFunc");
        PerformentTest("lua call C# static member funtion : base parameter member function : ", LOOP_TIMES, func);
		
		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticClassMemberFunc");
        PerformentTest("lua call C# static member funtion : class parameter member function : ", LOOP_TIMES, func);
		
		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticStructMemberFunc");
        PerformentTest("lua call C# static member funtion : struct parameter member function : ", LOOP_TIMES, func);
		
		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticVec3MemberFunc");
        PerformentTest("lua call C# static member funtion : vector3 parameter member function : ", LOOP_TIMES, func);
		
		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticInMemberFunc");
        PerformentTest("lua call C# static member funtion : input parameter member function : ", LOOP_TIMES, func);
		
		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticOutMemberFunc");
        PerformentTest("lua call C# static member funtion : output parameter member function : ", LOOP_TIMES, func);
		
		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticInOutMemberFunc");
        PerformentTest("lua call C# static member funtion : in & output parameter member function : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaAccessCSStaticTwoMemberFunc");
        PerformentTest("lua call C# static member funtion : two parameter member function : ", LOOP_TIMES, func);

        Debug.Log("lua call C# array & num : ");
        sw.WriteLine("lua call C# array & enum : ");

        func = luaenv.Global.Get<PerfTest>("LuaAccessCSEnumFunc_get");
        PerformentTest("lua call C# member : enum member, get : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest>("LuaAccessCSEnumFunc_set");
        PerformentTest("lua call C# member : enum member, set : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest>("LuaAccessCSArrayFunc_get");
        PerformentTest("lua call C# member : array member, get : ", LOOP_TIMES, func);

        func = luaenv.Global.Get<PerfTest>("LuaAccessCSArrayFunc_set");
        PerformentTest("lua call C# member : array member, set : ", LOOP_TIMES, func);
	}

	private void StartConstruct()
	{
        int LOOP_TIMES = 1000000;
        Debug.Log ("lua call construct :");
        sw.WriteLine("lua call construct :");
        PerfTest func = luaenv.Global.Get<PerfTest> ("LuaConstructClass");
        PerformentTest("lua construct class : ", LOOP_TIMES, func);
		
		func = luaenv.Global.Get<PerfTest> ("LuaConstructStruct");
        PerformentTest("lua construct struct : ", LOOP_TIMES, func);
	}

	private void StartAddRemoveCB()
	{
        int LOOP_TIMES = 200000;
        Debug.Log ("lua add & remove callback : ");
		sw.WriteLine ("lua add & remove call back : ");

		PerfTest func = luaenv.Global.Get<PerfTest> ("LuaAddRemoveCB");
        PerformentTest("lua add & remove callback : ", LOOP_TIMES, func);
	}

	private void StartCSCallLuaCB()
	{
        int LOOP_TIMES = 1000000;
        Debug.Log ("C# call lua callbak :");
		sw.WriteLine ("C# call lua callbak :");

		PerfTest func = luaenv.Global.Get<PerfTest> ("LuaBaseParaCB");
        PerformentTest("invoke base param callback : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaClassParaCB");
        PerformentTest("invoke class param callback : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaStructParaCB");
        PerformentTest("invoke struct param callback : ", LOOP_TIMES, func);

		func = luaenv.Global.Get<PerfTest> ("LuaVec3ParaCB");
        PerformentTest("invoke vector3 param callback : ", LOOP_TIMES, func);
	}
//------------------------------------------------------------------------------------------------------

	private int CPS(int loop_times, double ms)
	{
		return (int)(((double)loop_times) * 1000.0 / ms);
	}

//------------------------------------------------------------------------------------------------------
    [CSharpCallLua]
	public delegate void FuncBasePara(int x);
    [CSharpCallLua]
	public delegate void FuncClassPara(ParaClass x);
    [CSharpCallLua]
	public delegate void FuncStructPara(ParaStruct x);
    [CSharpCallLua]
    public delegate void FuncTwoBasePara(int x, int y);

}

[CSharpCallLua]
public delegate void BaseParaEventHandler(int x);
[CSharpCallLua]
public delegate void ClassParaEventHandler(ParaClass x);
[CSharpCallLua]
public delegate void StructParaEventHandler(ParaStruct x);
[CSharpCallLua]
public delegate void Vec3ParamEventHandler(Vector3 x);
[CSharpCallLua]
public delegate void NullEventHandler();

[LuaCallCSharp]
public class ParaClass
{}

[GCOptimize]
[LuaCallCSharp]
public struct ParaStruct
{}

[CSharpCallLua]
public interface ITableAccess
{
	int id { get; set;}
	void func();
}

[LuaCallCSharp]
public class ClassLuaCallCS
{
    public int[] array = new int[5];

    [LuaCallCSharp]
    public enum LuaEnum
    {
        ONE,
        TWO,
        THREE,
        FOUR,
        FIVE
    };

    public LuaEnum enumParam;


	public event BaseParaEventHandler BaseParaEvent;
	public event ClassParaEventHandler ClassParaEvent;
	public event StructParaEventHandler StructParaEvent;
	public event Vec3ParamEventHandler Vec3ParaEvent;
	public event NullEventHandler NullEvent;

	public int id;
	public ParaClass paraClass = new ParaClass ();
	public ParaStruct paraStruct = new ParaStruct();
	public Vector3 vec3Member;

	public void funcBaseParam(int x)
	{}

	public void funcClassParam(ParaClass x)
	{}

	public void funcStructParam(ParaStruct x)
	{}

	public void funcVec3Param(Vector3 x)
	{}

	public void funcInParam(ref int x)
	{}

	public void funcOutParam(out int x)
	{
		x = 0;
	}

	public void funcInOutParam(ref int x, out int y)
	{
		y = 0;
	}

	public void funcTwoParam(int x, int y)
	{
	}

	public static int sId;
	public static ParaClass sParamClass = new ParaClass();
	public static ParaStruct sParamStruct = new ParaStruct();
	public static Vector3 sParamVec3;

	public static void sFuncBaseParam(int x)
	{}
	
	public static void sFuncClassParam(ParaClass x)
	{}
	
	public static void sFuncStructParam(ParaStruct x)
	{}
	
	public static void sFuncVec3Param(Vector3 x)
	{}
	
	public static void sFuncInParam(ref int x)
	{}
	
	public static void sFuncOutParam(out int x)
	{
		x = 0;
	}
	
	public static void sFuncInOutParam(ref int x, out int y)
	{
		y = 0;
	}

	public static void sFuncTwoParam(int x, int y)
	{

	}

	public void InvokeBaseParaCB()
	{
		for (int i = 0; i < 1000000; i++) {
			BaseParaEvent(0);
		}
	}

	public void InvokeClassParaCB()
	{
		ParaClass paraCls = new ParaClass ();
		for (int i = 0; i < 1000000; i++) {
			ClassParaEvent(paraCls);
		}
	}

	public void InvokeStructParaCB()
	{
		ParaStruct paraStruct = new ParaStruct ();
		for (int i = 0; i < 1000000; i++) {
			StructParaEvent(paraStruct);
		}
	}

	public void InvokeVec3ParaCB()
	{
		Vector3 paraVec3 = new Vector3 (0, 0, 0);
		for (int i = 0; i < 1000000; i++) {
			Vec3ParaEvent(paraVec3);
		}
	}
}
