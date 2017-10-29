using XLua;
using System;
#if !XLUA_GENERAL
using UnityEngine;
#endif

public enum LuaTestTypeReflect
{
	ABC = 0,
	DEF,
	GHI,
	JKL
};

public enum FirstPushEnumReflect
{
	E1
};
[CSharpCallLua]
public delegate int TestDelegateReflect(int x);
[CSharpCallLua]
public delegate int TestEvtHandler1Reflect(float y);
[CSharpCallLua]
public delegate int TestEvtHandler2Reflect(byte y, float z);

public class LuaTestObjReflect
{
	public static LuaEnv luaEnv = LuaEnvSingletonForTest.Instance;
    public int testVar { get; set; }
    public int[] testArr = new int[3];
    public static LuaTestObjReflect operator + (LuaTestObjReflect a, LuaTestObjReflect b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a.testVar + b.testVar;
        return ret;
    }

    public static LuaTestObjReflect operator +(LuaTestObjReflect a, int b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a.testVar + b;
        return ret;
    }

    public static LuaTestObjReflect operator +(int a, LuaTestObjReflect b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a + b.testVar;
        return ret;
    }

    public static LuaTestObjReflect operator - (LuaTestObjReflect a, LuaTestObjReflect b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a.testVar - b.testVar;
        return ret;
    }

    public static LuaTestObjReflect operator -(LuaTestObjReflect a, int b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a.testVar - b;
        return ret;
    }

    public static LuaTestObjReflect operator -(int a, LuaTestObjReflect b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a - b.testVar;
        return ret;
    }

    public static LuaTestObjReflect operator * (LuaTestObjReflect a, LuaTestObjReflect b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a.testVar * b.testVar;
        return ret;
    }

    public static LuaTestObjReflect operator *(LuaTestObjReflect a, int b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a.testVar * b;
        return ret;
    }

    public static LuaTestObjReflect operator *(int a, LuaTestObjReflect b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a * b.testVar;
        return ret;
    }


    public static LuaTestObjReflect operator /(LuaTestObjReflect a, LuaTestObjReflect b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a.testVar / b.testVar;
        return ret;
    }

    public static LuaTestObjReflect operator /(LuaTestObjReflect a, int b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a.testVar / b;
        return ret;
    }

    public static LuaTestObjReflect operator /(int a, LuaTestObjReflect b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a / b.testVar;
        return ret;
    }

    public static LuaTestObjReflect operator % (LuaTestObjReflect a, LuaTestObjReflect b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a.testVar % b.testVar;
        return ret;
    }

    public static LuaTestObjReflect operator %(int a, LuaTestObjReflect b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a % b.testVar;
        return ret;
    }

    public static LuaTestObjReflect operator %(LuaTestObjReflect a, int b)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a.testVar % b;
        return ret;
    }

    public static LuaTestObjReflect operator - (LuaTestObjReflect a)
    {
        LuaTestObjReflect ret = new LuaTestObjReflect();
        ret.testVar = a.testVar * (-1);
        return ret;
    }

    public static bool operator < (LuaTestObjReflect a, LuaTestObjReflect b)
    {
        return (a.testVar < b.testVar);
    }


    public static bool operator <= (LuaTestObjReflect a, LuaTestObjReflect b)
    {
        return (a.testVar <= b.testVar);
    }



    public static bool operator > (LuaTestObjReflect a, LuaTestObjReflect b)
    {
        return (a.testVar > b.testVar);
    }



    public static bool operator >=(LuaTestObjReflect a, LuaTestObjReflect b)
    {
        return (a.testVar >= b.testVar);
    }

   

    public int this[int i]
    {
        get
        {
            return testArr[i];
        }
        set
        {
            testArr[i] = value;
        }
    }  

    
    public event TestEvtHandler1Reflect TestEvent1;
    public event TestEvtHandler2Reflect TestEvent2;

    public int CallEvent(float y)
    {
        return TestEvent1(y);
    }

	public static event TestEvtHandler1Reflect TestStaticEvent1;
	
	public static int CallStaticEvent(float y)
	{
		return TestStaticEvent1(y);
	}

	public static int DefaultParaFuncSingle(int i, string str = "abc")
	{
		return i;
	}

	public static int DefaultParaFuncMulti(int i, string str = "abc", double d = 0, byte c = 97)
	{
		return (i + 1);
	}

	public static int VariableParamFunc(int i, params string[] strs)
	{
		return 0;
	}

	public static int TestEnumFunc(LuaTestTypeReflect x)
	{
		return (int)x;
	}

	public static string TestGetType(Type x)
	{
		return x.ToString ();
	}

	public static ulong ulX1 = 1;
	public static ulong ulX2 = 1;
	public static long lY1 = 1;
	public static long lY2 = 1;
	public static Int64 i64Z1 = 1;
	public static Int64 i64Z2 = 1;
	public static long lY3 = 1;
	public static long lY4 = 123;
	public static long lY5 = 12345;
	public static long lY6 = 54321;

	public static void Gen64BitInt()
	{
		ulX1 = ulX2 = 1;
		lY1 = lY2 = lY3 = 1;
		i64Z1 = i64Z2 = 1;
		ulX1 = ulX1 << 62;
		ulX2 = ulX2 << 61;
		lY1 = lY1 << 62;
		lY2 = lY2 << 61;
		i64Z1 = i64Z1 << 62;
		i64Z2 = i64Z2 << 61;
		lY3 = lY3 << 50;

	}

	public static ITestLuaClassReflect CreateTestLuaObj()
	{
		return new TestLuaClassReflect ();
	}

    public static TestDelegateReflect csDelegate;
    public static TestDelegateReflect csDelegate1;
    public static TestDelegateReflect csDelegate2;
    public static TestDelegateReflect csDelegate3;
    public static TestDelegateReflect csDelegate4;

    public static TestDelegateReflect csDelegate11;
    public static TestDelegateReflect csDelegate12;
    public static TestDelegateReflect csDelegate13;

	public static int initNumber = 5;

	public static int CalcAdd(int x)
	{
		initNumber += x;
		return initNumber;
	}

	public static int calcadd(int x)
	{
		initNumber += x;
		return initNumber;
	}

	public static int CalcDel(int x)
	{
		initNumber -= x;
		return initNumber;
	}

	public static int CalcMul(int x)
	{
		initNumber *= x;
		return initNumber;
	}

	public static void GenDelegate()
	{
        csDelegate1 = new TestDelegateReflect(CalcAdd);
        csDelegate2 = new TestDelegateReflect(CalcDel);
        csDelegate3 = new TestDelegateReflect(CalcMul);
	}

    public static int OverLoad1(int x, int y)
    {
        return 1;
    }

    public static int OverLoad1(int x)
    {
        return 2;
    }

	public static int OverLoad1(params int[] vars)
	{
		int ret = 0;
		foreach (int var in vars) 
		{ 
			ret += var; 
		} 
		return ret;
	}

	public static int OverLoad2(int x, float y)
    {
        return 3;
    }

    public static int OverLoad2(string x, string y)
    {
        return 4;
    }

    public static int OverLoad3(int x)
    {
        return 5;
    }

    public static int OverLoad3(short y)
    {
        return 6;
    }


    public static void OutRefFunc1(int x, out int y, ref int z)
    {
        y = 100;
    }

    public static void OutRefFunc2(ref int x, int y, out int z)
    {
        z = 200;
    }

    public static void OutRefFunc3(int x, out int y, ref int z)
    {
        y = 300;
    }

    public static int OutRefFunc4(int x, out int y, ref int z)
    {
        y = 400;
        return y;
    }

    public static int OutRefFunc5(ref int x, int y, out int z)
    {
        z = 500;
        return z;
    }

    public static int OutRefFunc6(int x, int y)
    {
        return 600;
    }

    public static void OutRefFunc11(ITestLuaClassReflect x, out ITestLuaClassReflect y, ref ITestLuaClassReflect z)
    {
        y = CreateTestLuaObj();
        y.cmpTarget = 100;
    }

    public static void OutRefFunc12(ref ITestLuaClassReflect x, ITestLuaClassReflect y, out ITestLuaClassReflect z)
    {
        z = CreateTestLuaObj();
        z.cmpTarget = 200;
    }

    public static void OutRefFunc13(ITestLuaClassReflect x, out ITestLuaClassReflect y, ref ITestLuaClassReflect z)
    {
        y = CreateTestLuaObj();
        y.cmpTarget = 300;
    }

    public static int OutRefFunc14(ITestLuaClassReflect x, out ITestLuaClassReflect y, ref ITestLuaClassReflect z)
    {
        y = CreateTestLuaObj();
        y.cmpTarget = 400;
        return y.cmpTarget;
    }

    public static int OutRefFunc15(ref ITestLuaClassReflect x, ITestLuaClassReflect y, out ITestLuaClassReflect z)
    {
        z = CreateTestLuaObj();
        z.cmpTarget = 500;
        return z.cmpTarget;
    }

    public static int OutRefFunc16(ITestLuaClassReflect x, ITestLuaClassReflect y)
    {
        return 600;
    }

    public static void OutRefFunc21(TestDelegateReflect x, out TestDelegateReflect y, ref TestDelegateReflect z)
    {
        y = new TestDelegateReflect(CalcAdd);
    }

    public static void OutRefFunc22(ref TestDelegateReflect x, TestDelegateReflect y, out TestDelegateReflect z)
    {
        z = new TestDelegateReflect(CalcAdd);
    }

    public static void OutRefFunc23(TestDelegateReflect x, out TestDelegateReflect y, ref TestDelegateReflect z)
    {
        y = new TestDelegateReflect(CalcAdd);
    }

    public static int OutRefFunc24(TestDelegateReflect x, out TestDelegateReflect y, ref TestDelegateReflect z)
    {
        y = new TestDelegateReflect(CalcAdd);
        return y(1);
    }

    public static int OutRefFunc25(ref TestDelegateReflect x, TestDelegateReflect y, out TestDelegateReflect z)
    {
        z = new TestDelegateReflect(CalcAdd);
        return z(1);
    }

    public static int OutRefFunc26(TestDelegateReflect x, TestDelegateReflect y)
    {
        return 600;
    }

	public int Sum(int a, int b, int c)
	{
		return a + b + c;
	}
	
	public static int Sum(int a, int b)
	{
		return a + b;
	}

	public void GenericMethod<T>()
	{
		LuaTestCommon.Log("GenericMethod<" + typeof(T) +">");
	}

    public IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(100);

    public IntPtr GetPtr()
    {
        byte[] abc = new byte[] { 97, 98, 99 };
        System.Runtime.InteropServices.Marshal.Copy(abc, 0, ptr, abc.Length);
        return ptr;
    }

    public byte PrintPtr(IntPtr p)
    {
        LuaTestCommon.Log("p == ptr?" + (p == ptr));
        byte[] abc = new byte[] { 0, 0, 0 };
        System.Runtime.InteropServices.Marshal.Copy(p, abc, 0, abc.Length);
        LuaTestCommon.Log(string.Format("{0},{1},{2}", abc[0], abc[1], abc[2]));
        return abc[0];
    }
    public int VariableParamFuncDefault(int d, int i = 1, params string[] strs)
    {
        return i + d;
    }

    public static double StaticVariableParamFuncDefault(double d, double i = 1.0, params string[] strs)
    {
        return i + d;
    }
	
	public static byte[] FuncReturnByteArray()
	{
		byte[] abc = new byte[] { 97, 98, 99 };
		return abc;
	}
	
	public static byte FuncReturnByte()
    {
        byte abc = 97;
        return abc;
    }
	
	public static int[] FuncReturnIntArray()
	{
		int[] abc = new int[] { 97, 98, 99 };
		return abc;
	}

    public static int FuncReturnInt()
    {
        int abc = 97;
        return abc;
    }

#if !XLUA_GENERAL
    public static LayerMask TestImplicit()
    {
        return new LayerMask();
    }
#endif

    public static int VariableParamFunc(int i, params int[] strs)
    {
        return 0;
    }

	public static int VariableParamFunc2(params int[] strs)
    {
        return strs.Length;
    }
	
    public static string FirstPushEnumFunc(int i)
	{
        string luaScript = @"
        function first_push_reflect(t,obj)
	        if t==1 then
		        if obj == CS.FirstPushEnumReflect.E1 then
			        return 1
		        else
			        return 2
		        end
	        elseif t==2 then
		        if obj == CS.FirstPushEnumReflect.self then
			        return 3
		        else
			        return 4
		        end
	        else
		        return 5
	        end
        end";
        luaEnv.DoString(luaScript);
		LuaFunction f1 = luaEnv.Global.Get<LuaFunction>("first_push_reflect");
        LuaTestCommon.Log("LuaFunction<" + f1);
		object[] ret = f1.Call(i, FirstPushEnumReflect.E1);
		return ret[0].ToString();
	}
}

public class TestCastClassReflect
{
	public bool TestFunc1()
	{
		return true;

	}
}

public interface ITestLuaClassReflect
{
	bool TestFunc1();
    int cmpTarget{ set; get; }
}

internal class TestLuaClassReflect : ITestLuaClassReflect
{
	public bool TestFunc1()
	{
		return true;
	}
    public int cmpTarget { set; get; }
}

public class  TestChineseStringReflect
{

	public string GetShortChinString()
	{
		return short_simple_string;
	}

	public string GetLongChineString()
	{
		return long_simple_string;
	}

	public string GetCombineString()
	{
		return combine_string;
	}

	public string GetComplexString()
	{
		return complex_string;
	}

	public string GetHuoxingString()
	{
		return huoxing_string;
	}

	public string short_simple_string = "中文字符串";
	public string long_simple_string = "为Unity3D增加Lua脚本编程的能力，进而提供代码逻辑增量更新的可能，支持lua的所有基本类型，哈哈哈哈";
	public string combine_string = "中文字符串.* ? [ ] ^ $~`!@#$%^&()_-+=[];',““〈〉〖【℃ ＄ ¤№ ☆ ★■ⅷ②㈣12345abc";
	public string complex_string = "繁體國陸";
	public string huoxing_string = "吙煋呅僦媞這樣孒";
}

public class TestUlongAndLongTypeReflect
{
	public ulong UlongMax
	{

		get { return ulong_max; }
		set { ulong_max=value; }
	}

	public ulong UlongMin
	{
		
		get { return ulong_min; }
		set { ulong_min=value; }
	}

	public ulong UlongMid
	{
		
		get { return ulong_mid; }
		set { ulong_mid=value; }
	}

	public ulong UlongAdd()
	{
		return ulong_max - 1000 + 1;
	}

	public long LongMax
	{
		
		get { return long_max; }
		set { long_max=value; }
	}
	
	public long LongMin
	{
		
		get { return long_min; }
		set { long_min=value; }
	}
	
	public long LongMid
	{
		
		get { return long_mid; }
		set { long_mid=value; }
	}

	public long LongAdd()
	{
		return long_max - 1000 + 1;
	}

	ulong ulong_max = System.UInt64.MaxValue;
	ulong ulong_min = System.UInt64.MinValue;
	ulong ulong_mid = 9223372036854775808;
	long long_max = System.Int64.MaxValue;
	long long_min = System.Int64.MinValue;
	long long_mid = 4611686018427387904;
}

public struct EmployeestructReflect
{
	private string name;
	public string Name
	{
		get { return name; }
		set { name = value; }
	}
	
	private int age;
	public int Age
	{
		get { return age; }
		set { age = value; }
	}
	
	private int salary;
	private int annual_bonus;

	public int Salary
	{
		get { return salary; }
		set { salary = value; }
	}

	public int AnnualBonus
	{
		get { return annual_bonus; }
		set { annual_bonus = value; }
	}

}

public struct HasConstructStructReflect{
	public int x;
	public int y;
	public string z;
	public HasConstructStructReflect(int x, int y, string z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
}

public class CClassReflect
{
	public CClassReflect(int x, int y, string z)
	{
		c_struct = new HasConstructStructReflect(x, y, z);
	}

	public HasConstructStructReflect CConStruct
	{
		get { return c_struct; }
		set { c_struct = value; }
	}
	
	public void Add(HasConstructStructReflect invar, ref HasConstructStructReflect refvar, out HasConstructStructReflect outvar)
	{
		outvar = invar;
		outvar.x = invar.x + refvar.x;
		refvar.x = invar.x;
	}

	public int VariableParamFunc(params HasConstructStructReflect[] structs)
	{
		int ret = 0;
		foreach (HasConstructStructReflect var in structs) 
		{ 
			ret += var.x; 
		} 
		return ret;
	}
	
	private HasConstructStructReflect c_struct;
}

public class BClassReflect:CClassReflect
{
	public BClassReflect(int x, int y, string z):base(x, y, z)
	{
		b_struct = new HasConstructStructReflect(x, y, z);
	}

	public HasConstructStructReflect BConStruct
	{
		get { return b_struct; }
		set { b_struct = value; }
	}
	
	public void Sub(HasConstructStructReflect invar, ref HasConstructStructReflect refvar, out HasConstructStructReflect outvar)
	{
		outvar = invar;
		outvar.x = invar.x - refvar.x;
		refvar.x = invar.x;
		LuaTestCommon.Log ("refvar.x:" + refvar.x + ",refvar.y:" + refvar.y + ", refvar.z:" + refvar.z);
	}
	
	private HasConstructStructReflect b_struct;
}

public class AClassReflect:BClassReflect
{
	public AClassReflect(int x, int y , string z):base(x, y, z)
	{
		LuaTestCommon.Log ("AClass Constructor");
	}

    public int Div(int x, int y)
    {
		if (y == 0) {
			return 0;
		}
		else 
		{
			return x / y;
		}
	}
}

namespace TestExtensionMethodReflect
{
	[ReflectionUse]
	public static class TestExtensionMethodForStructReflect
	{
		public static void PrintSalary(this EmployeestructReflect i)
		{
			LuaTestCommon.Log("Salary:" + i.Salary);
		}
		
		public static int GetIncomeForOneYear(this EmployeestructReflect i)
		{
			return i.Salary * 12 + i.AnnualBonus;
		}
		
		public static int Add(this EmployeestructReflect i, EmployeestructReflect d)
		{
			return i.Salary * 12 + i.AnnualBonus + d.Salary * 12 + d.AnnualBonus;
		}
		
		public static void Sub(this EmployeestructReflect i, EmployeestructReflect d, out EmployeestructReflect e, ref EmployeestructReflect a)
		{
			e = d;
			e.Salary = 10000;
			a.Salary = i.Salary - d.Salary;
			LuaTestCommon.Log ("e.name:" + e.Name +", e.salary:"+ e.Salary);
			LuaTestCommon.Log ("a.name:" + a.Name +", a.salary:"+ a.Salary);
		}
	}
	
    [ReflectionUse]
	public static class TestExtensionMethodForClassReflect
	{
		public static void PrintAllString(this TestChineseStringReflect i)
		{
			LuaTestCommon.Log("GetLongChineString:" + i.GetLongChineString());
		}

		public static int GetLongStringLength(this TestChineseStringReflect i)
		{
			return i.GetLongChineString ().Length;
		}

		public static int Add(this TestChineseStringReflect i, TestChineseStringReflect d)
		{
			return i.GetLongChineString ().Length + d.GetLongChineString ().Length;
		}

		public static void Replace(this TestChineseStringReflect i , TestChineseStringReflect d, out TestChineseStringReflect e, ref TestChineseStringReflect a)
		{
			e = i;
			a.combine_string = i.short_simple_string;
			a.short_simple_string = i.short_simple_string + d.short_simple_string;
		}
	}
}

public abstract class EmployeeTemplateReflect
{
	public void GetSalary()
	{
		GetBasicSalary();
		AddBonus();
	}
	public abstract int GetBasicSalary();
	public abstract int AddBonus();
}

public class ManagerReflect : EmployeeTemplateReflect
{
	public override int GetBasicSalary()
	{
		//Console.WriteLine("Get Manager Basic Salary");
		return 1;
	}
	
	public override int AddBonus()
	{
		//Console.WriteLine("Add Manager Bonus");
		return 2;
	}
}

public class TableAutoTransSimpleClassReflect
{
    public TableAutoTransSimpleClassReflect()
    {
    }
    public TableAutoTransSimpleClassReflect(int x, string y, long z)
    {
        IntVar = x;
        StringVar = y;
        LongVar = z;
    }
    public int IntVar
    {

        get { return x; }

        set { x = value; }
    }

    public string StringVar
    {

        get { return y; }

        set { y = value; }
    }

    public long LongVar
    {

        get { return z; }

        set { z = value; }
    }

    public int x;
    public string y;
    public long z;
}
public class TableAutoTransComplexClassReflect
{
    public TableAutoTransComplexClassReflect()
    {
    }

    public TableAutoTransComplexClassReflect(int A, TableAutoTransSimpleClassReflect B)
    {
        IntVar = A;
        ClassVar = B;
    }
    public int IntVar
    {

        get { return A; }

        set { A = value; }
    }

    public TableAutoTransSimpleClassReflect ClassVar
    {

        get { return B; }

        set { B = value; }
    }

    public int A;
    public TableAutoTransSimpleClassReflect B;
}

public struct TableAutoTransSimpleStructReflect
{
    public byte a;
}


public struct TableAutoTransComplexStructReflect
{
    public int a;
    public int b;
    public decimal c;
    public TableAutoTransSimpleStructReflect d;
}
public class TestTableAutoTransClassReflect
{
    public TableAutoTransSimpleClassReflect SimpleClassMethod(TableAutoTransSimpleClassReflect p)
    {
        return p;
    }

    public TableAutoTransComplexClassReflect ComplexClassMethod(TableAutoTransComplexClassReflect p)
    {
        return p;
    }

    public TableAutoTransSimpleStructReflect SimpleStructMethod(TableAutoTransSimpleStructReflect p)
    {
        return p;
    }

    public TableAutoTransComplexStructReflect ComplexStructMethod(TableAutoTransComplexStructReflect p)
    {
        return p;
    }

    public int OneListMethod(int[] args)
    {
        int value = 0;
        for (int i = 0; i < args.Length; i++)
        {
            value += args[i];
        }
        return value;
    }

    public int TwoDimensionListMethod(int[][] args)
    {
        int value = 0;
        for (int i = 0; i < args.Length; i++)
        {
            foreach (int j in args[i])
            {
                value += j;
            }
        }
        return value;
    }
}


//验证构造函数支持refer，out修饰符，add@2017.05.09 for v2.1.7
public class ReferTestClassReflect
{
    public ReferTestClassReflect(int x, ref int y, out string z)
    {
        var_x = x;
        var_y = y;
        y = y - 1;
        z = "test1";
        var_z = z;
    }

    public ReferTestClassReflect(int x, out string z)
    {
        var_x = x;
        var_y = 10;
        z = "test3";
        var_z = z;
    }

    public int Get_X_Y_ADD()
    {
        return var_x + var_y;
    }

    private int var_x;
    private int var_y;
    private string var_z;
}