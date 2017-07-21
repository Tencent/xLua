using XLua;
using System;
#if !XLUA_GENERAL
using UnityEngine;
#endif

[LuaCallCSharp]
public class LongStatic
{
    public static long LONG_MAX = System.Int64.MaxValue;
    public static ulong ULONG_MAX = System.UInt64.MaxValue;
}

[LuaCallCSharp]
public enum LuaTestType
{
	ABC = 0,
	DEF,
	GHI,
	JKL
};

[LuaCallCSharp]
public enum FirstPushEnum
{
	E1
};

[CSharpCallLua]
public delegate int TestDelegate(int x);
[CSharpCallLua]
public delegate int TestEvtHandler1(float y);
[CSharpCallLua]
public delegate int TestEvtHandler2(byte y, float z);

[LuaCallCSharp]
public class LuaTestObj
{
	public static LuaEnv luaEnv = LuaEnvSingletonForTest.Instance;
    public int testVar { get; set; }
    public int[] testArr = new int[3];
    public static LuaTestObj operator + (LuaTestObj a, LuaTestObj b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a.testVar + b.testVar;
        return ret;
    }

    public static LuaTestObj operator +(LuaTestObj a, int b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a.testVar + b;
        return ret;
    }

    public static LuaTestObj operator +(int a, LuaTestObj b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a + b.testVar;
        return ret;
    }

    public static LuaTestObj operator - (LuaTestObj a, LuaTestObj b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a.testVar - b.testVar;
        return ret;
    }

    public static LuaTestObj operator -(LuaTestObj a, int b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a.testVar - b;
        return ret;
    }

    public static LuaTestObj operator -(int a, LuaTestObj b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a - b.testVar;
        return ret;
    }

    public static LuaTestObj operator * (LuaTestObj a, LuaTestObj b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a.testVar * b.testVar;
        return ret;
    }

    public static LuaTestObj operator *(LuaTestObj a, int b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a.testVar * b;
        return ret;
    }

    public static LuaTestObj operator *(int a, LuaTestObj b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a * b.testVar;
        return ret;
    }


    public static LuaTestObj operator /(LuaTestObj a, LuaTestObj b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a.testVar / b.testVar;
        return ret;
    }

    public static LuaTestObj operator /(LuaTestObj a, int b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a.testVar / b;
        return ret;
    }

    public static LuaTestObj operator /(int a, LuaTestObj b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a / b.testVar;
        return ret;
    }

    public static LuaTestObj operator % (LuaTestObj a, LuaTestObj b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a.testVar % b.testVar;
        return ret;
    }

    public static LuaTestObj operator %(int a, LuaTestObj b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a % b.testVar;
        return ret;
    }

    public static LuaTestObj operator %(LuaTestObj a, int b)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a.testVar % b;
        return ret;
    }

    public static LuaTestObj operator - (LuaTestObj a)
    {
        LuaTestObj ret = new LuaTestObj();
        ret.testVar = a.testVar * (-1);
        return ret;
    }

    public static bool operator < (LuaTestObj a, LuaTestObj b)
    {
        return (a.testVar < b.testVar);
    }


    public static bool operator <= (LuaTestObj a, LuaTestObj b)
    {
        return (a.testVar <= b.testVar);
    }



    public static bool operator > (LuaTestObj a, LuaTestObj b)
    {
        return (a.testVar > b.testVar);
    }



    public static bool operator >=(LuaTestObj a, LuaTestObj b)
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

    
    public event TestEvtHandler1 TestEvent1;
    public event TestEvtHandler2 TestEvent2;

	public static event TestEvtHandler1 TestStaticEvent1;

    public int CallEvent(float y)
    {
        return TestEvent1(y);
    }

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
	
	public static int VariableParamFunc2(params int[] strs)
    {
        return strs.Length;
    }
	
	public static int TestEnumFunc(LuaTestType x)
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

	public static ITestLuaClass CreateTestLuaObj()
	{
		return new TestLuaClass ();
	}

    public static TestDelegate csDelegate;
    public static TestDelegate csDelegate1;
    public static TestDelegate csDelegate2;
    public static TestDelegate csDelegate3;
    public static TestDelegate csDelegate4;

    public static TestDelegate csDelegate11;
    public static TestDelegate csDelegate12;
    public static TestDelegate csDelegate13;

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
        csDelegate1 = new TestDelegate(CalcAdd);
        csDelegate2 = new TestDelegate(CalcDel);
        csDelegate3 = new TestDelegate(CalcMul);
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

    public static void OutRefFunc11(ITestLuaClass x, out ITestLuaClass y, ref ITestLuaClass z)
    {
        y = CreateTestLuaObj();
        y.cmpTarget = 100;
    }

    public static void OutRefFunc12(ref ITestLuaClass x, ITestLuaClass y, out ITestLuaClass z)
    {
        z = CreateTestLuaObj();
        z.cmpTarget = 200;
    }

    public static void OutRefFunc13(ITestLuaClass x, out ITestLuaClass y, ref ITestLuaClass z)
    {
        y = CreateTestLuaObj();
        y.cmpTarget = 300;
    }

    public static int OutRefFunc14(ITestLuaClass x, out ITestLuaClass y, ref ITestLuaClass z)
    {
        y = CreateTestLuaObj();
        y.cmpTarget = 400;
        return y.cmpTarget;
    }

    public static int OutRefFunc15(ref ITestLuaClass x, ITestLuaClass y, out ITestLuaClass z)
    {
        z = CreateTestLuaObj();
        z.cmpTarget = 500;
        return z.cmpTarget;
    }

    public static int OutRefFunc16(ITestLuaClass x, ITestLuaClass y)
    {
        return 600;
    }

    public static void OutRefFunc21(TestDelegate x, out TestDelegate y, ref TestDelegate z)
    {
        y = new TestDelegate(CalcAdd);
    }

    public static void OutRefFunc22(ref TestDelegate x, TestDelegate y, out TestDelegate z)
    {
        z = new TestDelegate(CalcAdd);
    }

    public static void OutRefFunc23(TestDelegate x, out TestDelegate y, ref TestDelegate z)
    {
        y = new TestDelegate(CalcAdd);
    }

    public static int OutRefFunc24(TestDelegate x, out TestDelegate y, ref TestDelegate z)
    {
        y = new TestDelegate(CalcAdd);
        return y(1);
    }

    public static int OutRefFunc25(ref TestDelegate x, TestDelegate y, out TestDelegate z)
    {
        z = new TestDelegate(CalcAdd);
        return z(1);
    }

    public static int OutRefFunc26(TestDelegate x, TestDelegate y)
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
	
	public static string FirstPushEnumFunc(int i)
	{
        string luaScript = @"
        function first_push(t,obj)
	        if t==1 then
		        if obj == CS.FirstPushEnum.E1 then
			        return 1
		        else
			        return 2
		        end
	        elseif t==2 then
		        if obj == CS.FirstPushEnum.self then
			        return 3
		        else
			        return 4
		        end
	        else
		        return 5
	        end
        end";
        luaEnv.DoString(luaScript);
		LuaFunction f1 = luaEnv.Global.Get<LuaFunction>("first_push");
        LuaTestCommon.Log("LuaFunction<" + f1);
        object[] ret = f1.Call(i, FirstPushEnum.E1);
        return ret[0].ToString();
	}
}

[LuaCallCSharp]
public class TestCastClass
{
	public bool TestFunc1()
	{
		return true;

	}
}

[LuaCallCSharp]
public interface ITestLuaClass
{
	bool TestFunc1();
    int cmpTarget{ set; get; }
}

internal class TestLuaClass : ITestLuaClass
{
	public bool TestFunc1()
	{
		return true;
	}
    public int cmpTarget { set; get; }
}

[LuaCallCSharp]
public class  TestChineseString
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

public class TestUlongAndLongType
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

[LuaCallCSharp]
public struct Employeestruct
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


public struct ConStruct{
	public int x;
	public int y;
	public string z;
	public ConStruct(int x, int y, string z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
}

[GCOptimize]
[LuaCallCSharp]
public struct StaticPusherStructA
{
	public byte byteVar;
	public sbyte sbyteVar;
	public StaticPusherStructA(byte a, sbyte b)
	{
		this.byteVar = a;
		this.sbyteVar = b;
	}
}


[GCOptimize]
[LuaCallCSharp]
public struct StaticPusherStructB
{
	public short shortVar;
	public ushort ushortVar;
	public int intVar;
	public uint uintVar;
	public StaticPusherStructB(short a, ushort b, int c, uint d)
	{
		this.shortVar = a;
		this.ushortVar = b;
		this.intVar = c;
		this.uintVar = d;
	}
}

[GCOptimize]
[LuaCallCSharp]
public struct StaticPusherStructAll
{
	public long longVar;
	public ulong ulongVar;
	public float floatVar;
	public double doubleVar;
	public StaticPusherStructA structA;
	public StaticPusherStructB structB;
}

public interface ITest
{
	int Add(int x,int y);
}

public struct NoGenCodeStruct:ITest{

	private byte byte_var;
	private short short_var;
	private ushort ushort_var;
	private int int_var1;
	private int int_var2;
	private uint uint_var;
	private long long_var;
	private ulong ulong_var;
	private double double_var;
	private float float_var;
	private char char_var;
	private decimal decimal_var;
	private string string_var;

	public byte Byte
	{
		get { return byte_var; }
		set { byte_var = value; }
	}

	public short Short
	{
		get { return short_var; }
		set { short_var = value; }
	}

	public ushort UShort
	{
		get { return ushort_var; }
		set { ushort_var = value; }
	}

	public int IntVar1
	{
		get { return int_var1; }
		set { int_var1 = value; }
	}

	public int IntVar2
	{
		get { return int_var2; }
		set { int_var2 = value; }
	}

	public uint UInt
	{
		get { return uint_var; }
		set { uint_var = value; }
	}

	public long Long
	{
		get { return long_var; }
		set { long_var = value; }
	}

	public ulong ULong
	{
		get { return ulong_var; }
		set { ulong_var = value; }
	}

	public double Double
	{
		get { return double_var; }
		set { double_var = value; }
	}

	public float Float
	{
		get { return float_var; }
		set { float_var = value; }
	}

	public char Char
	{
		get { return char_var; }
		set { char_var = value; }
	}
	
	public decimal Decimal
	{
		get { return decimal_var; }
		set { decimal_var = value; }
	}
	
	public string String
	{
		get { return string_var; }
		set { string_var = value; }
	}

	public int Add(int x, int y)
	{
		return x + y;
	}

	private ConStruct con;

	public ConStruct IncludeStruct
	{
		get { return con; }
		set { con = value; }
	}
}

public class NoGenCodeBaseClass
{
	public NoGenCodeStruct InitStruct(out int add){
		struct_var.Byte = 10;
		struct_var.Char = 'a';
		struct_var.Decimal = 12.33333333m;
		struct_var.Double = 10.111111111111;
		struct_var.Float = 11.1234f;
		struct_var.IntVar1 = 12345678;
		struct_var.IntVar2 = -12345678;
		struct_var.Long = System.Int64.MaxValue;
		struct_var.ULong = System.UInt64.MaxValue;
		struct_var.Short = 123;
		struct_var.String = "just for test";
		struct_var.UInt = 12345;
		struct_var.UShort = 255;
		struct_var.IncludeStruct = new ConStruct (1, 2, "haha");
		add = struct_var.Add (struct_var.IntVar1, struct_var.IntVar2);
		return struct_var;
	}

	public void SetStruct(ref NoGenCodeStruct var, out NoGenCodeStruct value)
	{
		struct_var.Byte = var.Byte;
		struct_var.Char = var.Char;
		struct_var.Decimal = var.Decimal;
		struct_var.Double = var.Double;
		struct_var.Float = var.Float;
		struct_var.IntVar1 = var.IntVar1;
		struct_var.IntVar2 = var.IntVar2;
		struct_var.Long = var.Long;
		struct_var.ULong = var.ULong;
		struct_var.Short = var.Short;
		struct_var.String = var.String;
		struct_var.UInt = var.UInt;
		struct_var.UShort = var.UShort;
		struct_var.IncludeStruct = new ConStruct(var.IncludeStruct.x, var.IncludeStruct.y, var.IncludeStruct.z);
		value = struct_var;
	}

	public NoGenCodeStruct GetStruct(){
		return struct_var;
	}

	public void SetStruct(GenCodeStruct var, out int add, out NoGenCodeStruct value)
	{
		struct_var.Byte = var.Byte;
		struct_var.Char = var.Char;
		struct_var.Decimal = var.Decimal;
		struct_var.Double = var.Double;
		struct_var.Float = var.Float;
		struct_var.IntVar1 = var.IntVar1;
		struct_var.IntVar2 = var.IntVar2;
		struct_var.Long = var.Long;
		struct_var.ULong = var.ULong;
		struct_var.Short = var.Short;
		struct_var.String = var.String;
		struct_var.UInt = var.UInt;
		struct_var.UShort = var.UShort;
		struct_var.IncludeStruct = new ConStruct(var.IncludeStruct.x, var.IncludeStruct.y, var.IncludeStruct.z);
		add = struct_var.Add (var.IntVar1, var.IntVar2);
		value = struct_var;
		LuaTestCommon.Log ("NoGenCodeStruct Byte:" + struct_var.Byte);
		LuaTestCommon.Log ("NoGenCodeStruct Char:" + struct_var.Char);
		LuaTestCommon.Log ("NoGenCodeStruct Decimal:" + struct_var.Decimal);
		LuaTestCommon.Log ("NoGenCodeStruct Double:" + struct_var.Double);
		LuaTestCommon.Log ("NoGenCodeStruct Float:" + struct_var.Float);
		LuaTestCommon.Log ("NoGenCodeStruct IntVar1:" + struct_var.IntVar1);
		LuaTestCommon.Log ("NoGenCodeStruct IntVar2:" + struct_var.IntVar2);
		LuaTestCommon.Log ("NoGenCodeStruct Long:" + struct_var.Long);
		LuaTestCommon.Log ("NoGenCodeStruct ULong:" + struct_var.ULong);
		LuaTestCommon.Log ("NoGenCodeStruct Short:" + struct_var.Short);
		LuaTestCommon.Log ("NoGenCodeStruct String:" + struct_var.String);
		LuaTestCommon.Log ("NoGenCodeStruct UInt:" + struct_var.UInt);
		LuaTestCommon.Log ("NoGenCodeStruct UShort:" + struct_var.UShort);
		LuaTestCommon.Log ("NoGenCodeStruct IncludeStruct.x:" + struct_var.IncludeStruct.x + ", IncludeStruct.y:" 
		           + struct_var.IncludeStruct.y + ", IncludeStruct.z:" + struct_var.IncludeStruct.z);
	}

	public static NoGenCodeStruct GetStaticVar()
	{
		LuaTestCommon.Log ("static struct byte:" + struct_var1.Byte);
		return struct_var1;
	}

	public void SetStaticPusherStruct(StaticPusherStructAll inVar, ref StaticPusherStructAll refVar, out StaticPusherStructAll outVar)
	{
		static_pushstruct_var.longVar = inVar.longVar + refVar.longVar;
		static_pushstruct_var.ulongVar = inVar.ulongVar + refVar.ulongVar;
		static_pushstruct_var.floatVar = inVar.floatVar + refVar.floatVar;
		static_pushstruct_var.doubleVar = inVar.doubleVar + refVar.doubleVar;
		static_pushstruct_var.structA.byteVar = 10;
		static_pushstruct_var.structA.sbyteVar = 100;
		static_pushstruct_var.structB.intVar = inVar.structB.intVar + refVar.structB.intVar;
		static_pushstruct_var.structB.uintVar = inVar.structB.uintVar + refVar.structB.uintVar;
		static_pushstruct_var.structB.shortVar = refVar.structB.shortVar;
		static_pushstruct_var.structB.ushortVar = inVar.structB.ushortVar;
		outVar = static_pushstruct_var;
		refVar.longVar = refVar.longVar - 1;
		refVar.ulongVar = refVar.ulongVar - 1;
		refVar.floatVar = refVar.floatVar - 1.0f;
		refVar.doubleVar = refVar.doubleVar - 1.0;
		refVar.structA.byteVar = 98;
		refVar.structA.sbyteVar = 101;
		refVar.structB.intVar = refVar.structB.intVar + 1;
		refVar.structB.uintVar = refVar.structB.uintVar + 1;
		refVar.structB.shortVar = inVar.structB.shortVar;
		refVar.structB.ushortVar = inVar.structB.ushortVar;
	}

	private NoGenCodeStruct struct_var;
	public static NoGenCodeStruct struct_var1;
	public static ConStruct struct_var2 = new ConStruct (3, 4, "enen");
	public static int GBS = 1;
	public StaticPusherStructAll static_pushstruct_var;
}

public class NoGenCodeDrivedClass:NoGenCodeBaseClass
{
	public ConStruct SimpleConStruct
	{
		get { return simple_struct; }
		set { simple_struct = value; }
	}

	public int Add(int x, int y)
	{
		return x + y;
	}

	public void Add(ConStruct invar, ref ConStruct refvar, out ConStruct outvar)
	{
		outvar = invar;
		outvar.x = invar.x + refvar.x;
		refvar.x = invar.x;
	}

	private ConStruct simple_struct;
}


[LuaCallCSharp]
public struct HasConstructStruct{
	public int x;
	public int y;
	public string z;
	public HasConstructStruct(int x, int y, string z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
}

[LuaCallCSharp]
public interface IGenCodeTest
{
	int Add(int x,int y);
}

[LuaCallCSharp]
public struct GenCodeStruct:IGenCodeTest{
	
	private byte byte_var;
	private short short_var;
	private ushort ushort_var;
	private int int_var1;
	private int int_var2;
	private uint uint_var;
	private long long_var;
	private ulong ulong_var;
	private double double_var;
	private float float_var;
	private char char_var;
	private decimal decimal_var;
	private string string_var;
	
	public byte Byte
	{
		get { return byte_var; }
		set { byte_var = value; }
	}
	
	public short Short
	{
		get { return short_var; }
		set { short_var = value; }
	}
	
	public ushort UShort
	{
		get { return ushort_var; }
		set { ushort_var = value; }
	}
	
	public int IntVar1
	{
		get { return int_var1; }
		set { int_var1 = value; }
	}
	
	public int IntVar2
	{
		get { return int_var2; }
		set { int_var2 = value; }
	}
	
	public uint UInt
	{
		get { return uint_var; }
		set { uint_var = value; }
	}
	
	public long Long
	{
		get { return long_var; }
		set { long_var = value; }
	}
	
	public ulong ULong
	{
		get { return ulong_var; }
		set { ulong_var = value; }
	}
	
	public double Double
	{
		get { return double_var; }
		set { double_var = value; }
	}
	
	public float Float
	{
		get { return float_var; }
		set { float_var = value; }
	}
	
	public char Char
	{
		get { return char_var; }
		set { char_var = value; }
	}
	
	public decimal Decimal
	{
		get { return decimal_var; }
		set { decimal_var = value; }
	}
	
	public string String
	{
		get { return string_var; }
		set { string_var = value; }
	}
	
	public int Add(int x, int y)
	{
		return x + y;
	}
	
	private HasConstructStruct con;
	
	public HasConstructStruct IncludeStruct
	{
		get { return con; }
		set { con = value; }
	}
}

[LuaCallCSharp]
public class GenCodeBaseClass
{
	public GenCodeStruct InitStruct(out int add){
		struct_var.Byte = 100;
		struct_var.Char = 'b';
		struct_var.Decimal = 22.33333333m;
		struct_var.Double = 20.111111111111;
		struct_var.Float = 21.1234f;
		struct_var.IntVar1 = 22345678;
		struct_var.IntVar2 = -22345678;
		struct_var.Long = System.Int64.MinValue;
		struct_var.ULong = System.UInt64.MinValue;
		struct_var.Short = 223;
		struct_var.String = "2just for test";
		struct_var.UInt = 22345;
		struct_var.UShort = 128;
		struct_var.IncludeStruct = new HasConstructStruct (2, 2, "2haha");
		add = struct_var.Add (struct_var.IntVar1, struct_var.IntVar2);
		return struct_var;
	}

	public void SetStruct(ref GenCodeStruct var, out GenCodeStruct value)
	{
		struct_var.Byte = var.Byte;
		struct_var.Char = var.Char;
		struct_var.Decimal = var.Decimal;
		struct_var.Double = var.Double;
		struct_var.Float = var.Float;
		struct_var.IntVar1 = var.IntVar1;
		struct_var.IntVar2 = var.IntVar2;
		struct_var.Long = var.Long;
		struct_var.ULong = var.ULong;
		struct_var.Short = var.Short;
		struct_var.String = var.String;
		struct_var.UInt = var.UInt;
		struct_var.UShort = var.UShort;
		struct_var.IncludeStruct = new HasConstructStruct(var.IncludeStruct.x, var.IncludeStruct.y, var.IncludeStruct.z);
		value = struct_var;
	}
	
	public GenCodeStruct GetStruct(){
		return struct_var;
	}
	
	public void SetStruct(NoGenCodeStruct var, out int add, out GenCodeStruct value)
	{
		struct_var.Byte = var.Byte;
		struct_var.Char = var.Char;
		struct_var.Decimal = var.Decimal;
		struct_var.Double = var.Double;
		struct_var.Float = var.Float;
		struct_var.IntVar1 = var.IntVar1;
		struct_var.IntVar2 = var.IntVar2;
		struct_var.Long = var.Long;
		struct_var.ULong = var.ULong;
		struct_var.Short = var.Short;
		struct_var.String = var.String;
		struct_var.UInt = var.UInt;
		struct_var.UShort = var.UShort;
		struct_var.IncludeStruct = new HasConstructStruct(var.IncludeStruct.x, var.IncludeStruct.y, var.IncludeStruct.z);
		add = struct_var.Add (var.IntVar1, var.IntVar2);
		value = struct_var;
		LuaTestCommon.Log ("GenCodeStruct Byte:" + struct_var.Byte);
		LuaTestCommon.Log ("GenCodeStruct Char:" + struct_var.Char);
		LuaTestCommon.Log ("GenCodeStruct Decimal:" + struct_var.Decimal);
		LuaTestCommon.Log ("GenCodeStruct Double:" + struct_var.Double);
		LuaTestCommon.Log ("GenCodeStruct Float:" + struct_var.Float);
		LuaTestCommon.Log ("GenCodeStruct IntVar1:" + struct_var.IntVar1);
		LuaTestCommon.Log ("GenCodeStruct IntVar2:" + struct_var.IntVar2);
		LuaTestCommon.Log ("GenCodeStruct Long:" + struct_var.Long);
		LuaTestCommon.Log ("GenCodeStruct ULong:" + struct_var.ULong);
		LuaTestCommon.Log ("GenCodeStruct Short:" + struct_var.Short);
		LuaTestCommon.Log ("GenCodeStruct String:" + struct_var.String);
		LuaTestCommon.Log ("GenCodeStruct UInt:" + struct_var.UInt);
		LuaTestCommon.Log ("GenCodeStruct UShort:" + struct_var.UShort);
		LuaTestCommon.Log ("GenCodeStruct IncludeStruct.x:" + struct_var.IncludeStruct.x + ", IncludeStruct.y:" 
		           + struct_var.IncludeStruct.y + ", IncludeStruct.z:" + struct_var.IncludeStruct.z);
	}

    public void SetStaticPusherStruct(StaticPusherStructAll inVar, ref StaticPusherStructAll refVar, out StaticPusherStructAll outVar)
    {
        static_pushstruct_var.longVar = inVar.longVar + refVar.longVar;
        static_pushstruct_var.ulongVar = inVar.ulongVar + refVar.ulongVar;
        static_pushstruct_var.floatVar = inVar.floatVar + refVar.floatVar;
        static_pushstruct_var.doubleVar = inVar.doubleVar + refVar.doubleVar;
        static_pushstruct_var.structA.byteVar = 97;
        static_pushstruct_var.structA.sbyteVar = 100;
        static_pushstruct_var.structB.intVar = inVar.structB.intVar + refVar.structB.intVar;
        static_pushstruct_var.structB.uintVar = inVar.structB.uintVar + refVar.structB.uintVar;
        static_pushstruct_var.structB.shortVar = refVar.structB.shortVar;
        static_pushstruct_var.structB.ushortVar = inVar.structB.ushortVar;
        outVar = static_pushstruct_var;
        refVar.longVar = refVar.longVar - 1;
        refVar.ulongVar = refVar.ulongVar - 1;
        refVar.floatVar = refVar.floatVar - 1.0f;
        refVar.doubleVar = refVar.doubleVar - 1.0;
        refVar.structA.byteVar = 98;
        refVar.structA.sbyteVar = 101;
        refVar.structB.intVar = refVar.structB.intVar + 1;
        refVar.structB.uintVar = refVar.structB.uintVar + 1;
        refVar.structB.shortVar = inVar.structB.shortVar;
        refVar.structB.ushortVar = inVar.structB.ushortVar;
    }
	
	private GenCodeStruct struct_var;
    public StaticPusherStructAll static_pushstruct_var;
	public static GenCodeStruct struct_var1;
	public static HasConstructStruct struct_var2 = new HasConstructStruct (4, 5, "enen");
	public static int GBS = 1;
}

[LuaCallCSharp]
public class GenCodeDrivedClass:GenCodeBaseClass
{
	public HasConstructStruct SimpleConStruct
	{
		get { return simple_struct; }
		set { simple_struct = value; }
	}
	
	public int Add(int x, int y)
	{
		return x + y;
	}
	
	public void Add(HasConstructStruct invar, ref HasConstructStruct refvar, out HasConstructStruct outvar)
	{
		outvar = invar;
		outvar.x = invar.x + refvar.x;
		refvar.x = invar.x;
	}
	
	private HasConstructStruct simple_struct;
}

[LuaCallCSharp]
public class CClass
{
	public CClass(int x, int y, string z)
	{
		c_struct = new HasConstructStruct(x, y, z);
	}

	public HasConstructStruct CConStruct
	{
		get { return c_struct; }
		set { c_struct = value; }
	}
	
	public void Add(HasConstructStruct invar, ref HasConstructStruct refvar, out HasConstructStruct outvar)
	{
		outvar = invar;
		outvar.x = invar.x + refvar.x;
		refvar.x = invar.x;
	}

	public int VariableParamFunc(params HasConstructStruct[] structs)
	{
		int ret = 0;
		foreach (HasConstructStruct var in structs) 
		{ 
			ret += var.x; 
		} 
		return ret;
	}
	
	private HasConstructStruct c_struct;
}

public class BClass:CClass
{
	public BClass(int x, int y, string z):base(x, y, z)
	{
		b_struct = new HasConstructStruct(x, y, z);
	}

	public HasConstructStruct BConStruct
	{
		get { return b_struct; }
		set { b_struct = value; }
	}
	
	public void Sub(HasConstructStruct invar, ref HasConstructStruct refvar, out HasConstructStruct outvar)
	{
		outvar = invar;
		outvar.x = invar.x - refvar.x;
		refvar.x = invar.x;
		LuaTestCommon.Log ("refvar.x:" + refvar.x + ",refvar.y:" + refvar.y + ", refvar.z:" + refvar.z);
	}
	
	private HasConstructStruct b_struct;
}

[LuaCallCSharp]
public class AClass:BClass
{
	public AClass(int x, int y , string z):base(x, y, z)
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

public struct NoGen2FloatStruct{
	public float a;
	public float b;
	public NoGen2FloatStruct(float a, float b)
	{
		this.a = a;
		this.b = b;
	}
}

public struct NoGen3FloatStruct{
	public float a;
	public float b;
	public float c;
	public NoGen3FloatStruct(float a, float b, float c)
	{
		this.a = a;
		this.b = b;
		this.c = c;
	}
}

public struct NoGen4FloatStruct{
	public float a;
	public float b;
	public float c;
	public float d;
	public NoGen4FloatStruct(float a, float b, float c, float d)
	{
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
	}
}

public struct NoGen5FloatStruct{
	public float a;
	public float b;
	public float c;
	public float d;
	public float e;
	public NoGen5FloatStruct(float a, float b, float c, float d, float e)
	{
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
		this.e = e;
	}
}

public struct NoGen6FloatStruct{
	public float a;
	public float b;
	public float c;
	public float d;
	public float e;
	public float f;
	public NoGen6FloatStruct(float a, float b, float c, float d, float e, float f)
	{
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
		this.e = e;
		this.f = f;
	}
}

public class TestNoGenFloatStructClass {

	public NoGen2FloatStruct Struct2{
		set { struct_2 = value; }
		get { return struct_2; }
	}

	public NoGen3FloatStruct Struct3{
		set { struct_3 = value; }
		get { return struct_3; }
	}

	public NoGen4FloatStruct Struct4{
		set { struct_4 = value; }
		get { return struct_4; }
	}

	public NoGen5FloatStruct Struct5{
		set { struct_5 = value; }
		get { return struct_5; }
	}

	public NoGen6FloatStruct Struct6{
		set { struct_6 = value; }
		get { return struct_6; }
	}

	public void Add2(ref NoGen2FloatStruct x, ref Gen2FloatStruct y)
	{
		x.a = x.a + y.a;
		x.b = x.b + y.b;
		y.a = x.a;
		y.b = x.b;
	}

	public void Sub3(ref NoGen3FloatStruct x, ref Gen3FloatStruct y)
	{
		x.a = x.a - y.a;
		x.b = x.b - y.b;
		x.c = x.c - y.c;
		y.a = x.a;
		y.b = x.b;
		y.a = x.c;
	}

	public void Multiply4(ref NoGen4FloatStruct x, ref Gen4FloatStruct y)
	{
		x.a = x.a * y.a;
		x.b = x.b + y.b;
		x.c = x.c - y.c;
		x.d = x.d * y.d;
		y.a = x.a;
		y.b = x.b;
		y.c = x.c;
		y.d = x.d;
	}

	public void All5(ref NoGen5FloatStruct x, ref Gen5FloatStruct y)
	{
		x.a = x.a * y.a;
		x.b = x.b + y.b;
		x.c = x.c - y.c;
		x.d = x.d / y.d;
		x.e = y.e + x.e;
		y.a = x.a;
		y.b = x.b;
		y.c = x.c;
		y.d = x.d;
		y.e = x.e;
	}

	public void All6(ref NoGen6FloatStruct x, ref Gen6FloatStruct y)
	{
		x.a = x.a * y.a;
		x.b = x.b + y.b;
		x.c = x.c - y.c;
		x.d = x.d / y.d;
		x.e = y.e + x.e;
		x.f = x.f - y.f;
		y.a = x.a;
		y.b = x.b;
		y.c = x.c;
		y.d = x.d;
		y.e = x.e;
		y.f = x.f;
	}

	private NoGen2FloatStruct struct_2;
	private NoGen3FloatStruct struct_3;
	private NoGen4FloatStruct struct_4;
	private NoGen5FloatStruct struct_5;
	private NoGen6FloatStruct struct_6; 
}

[GCOptimize]
[LuaCallCSharp]
public struct Gen2FloatStruct{
	public float a;
	public float b;
	public Gen2FloatStruct(float a, float b)
	{
		this.a = a;
		this.b = b;
	}
}

[GCOptimize]
[LuaCallCSharp]
public struct Gen3FloatStruct{
	public float a;
	public float b;
	public float c;
	public Gen3FloatStruct(float a, float b, float c)
	{
		this.a = a;
		this.b = b;
		this.c = c;
	}
}

[GCOptimize]
[LuaCallCSharp]
public struct Gen4FloatStruct{
	public float a;
	public float b;
	public float c;
	public float d;
	public Gen4FloatStruct(float a, float b, float c, float d)
	{
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
	}
}

[GCOptimize]
[LuaCallCSharp]
public struct Gen5FloatStruct{
	public float a;
	public float b;
	public float c;
	public float d;
	public float e;
	public Gen5FloatStruct(float a, float b, float c, float d, float e)
	{
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
		this.e = e;
	}
}

[GCOptimize]
[LuaCallCSharp]
public struct Gen6FloatStruct{
	public float a;
	public float b;
	public float c;
	public float d;
	public float e;
	public float f;
	public Gen6FloatStruct(float a, float b, float c, float d, float e, float f)
	{
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
		this.e = e;
		this.f = f;
	}
}

[LuaCallCSharp]
public class TestGenFloatStructClass {
	
	public Gen2FloatStruct Struct2{
		set { struct_2 = value; }
		get { return struct_2; }
	}
	
	public Gen3FloatStruct Struct3{
		set { struct_3 = value; }
		get { return struct_3; }
	}
	
	public Gen4FloatStruct Struct4{
		set { struct_4 = value; }
		get { return struct_4; }
	}
	
	public Gen5FloatStruct Struct5{
		set { struct_5 = value; }
		get { return struct_5; }
	}
	
	public Gen6FloatStruct Struct6{
		set { struct_6 = value; }
		get { return struct_6; }
	}
	
	public void Add2(ref NoGen2FloatStruct x, ref Gen2FloatStruct y)
	{
		x.a = x.a + y.a;
		x.b = x.b + y.b;
		y.a = x.a;
		y.b = x.b;
	}
	
	public void Sub3(ref NoGen3FloatStruct x, ref Gen3FloatStruct y)
	{
		x.a = x.a - y.a;
		x.b = x.b - y.b;
		x.c = x.c - y.c;
		y.a = x.a;
		y.b = x.b;
		y.a = x.c;
	}
	
	public void Multiply4(ref NoGen4FloatStruct x, ref Gen4FloatStruct y)
	{
		x.a = x.a * y.a;
		x.b = x.b + y.b;
		x.c = x.c - y.c;
		x.d = x.d * y.d;
		y.a = x.a;
		y.b = x.b;
		y.c = x.c;
		y.d = x.d;
	}
	
	public void All5(ref NoGen5FloatStruct x, ref Gen5FloatStruct y)
	{
		x.a = x.a * y.a;
		x.b = x.b + y.b;
		x.c = x.c - y.c;
		x.d = x.d / y.d;
		x.e = y.e + x.e;
		y.a = x.a;
		y.b = x.b;
		y.c = x.c;
		y.d = x.d;
		y.e = x.e;
	}
	
	public void All6(ref NoGen6FloatStruct x, ref Gen6FloatStruct y)
	{
		x.a = x.a * y.a;
		x.b = x.b + y.b;
		x.c = x.c - y.c;
		x.d = x.d / y.d;
		x.e = y.e + x.e;
		x.f = x.f - y.f;
		y.a = x.a;
		y.b = x.b;
		y.c = x.c;
		y.d = x.d;
		y.e = x.e;
		y.f = x.f;
	}
	
	private Gen2FloatStruct struct_2;
	private Gen3FloatStruct struct_3;
	private Gen4FloatStruct struct_4;
	private Gen5FloatStruct struct_5;
	private Gen6FloatStruct struct_6; 
}

public struct NoGen2IntStruct{
	public int a;
	public int b;
	public NoGen2IntStruct(int a, int b)
	{
		this.a = a;
		this.b = b;
	}
}
[CSharpCallLua]
public delegate int TestReflectEvtHandler1(float y);
public class TestReflectEventClass{

	public event TestReflectEvtHandler1 TestEvent1;
	
	public int CallEvent(float y)
	{
		return TestEvent1(y);
	}
}

namespace TestExtensionMethod
{
	[LuaCallCSharp]
	public static class TestExtensionMethodForStruct
	{
		public static void PrintSalary(this Employeestruct i)
		{
			LuaTestCommon.Log("Salary:" + i.Salary);
		}
		
		public static int GetIncomeForOneYear(this Employeestruct i)
		{
			return i.Salary * 12 + i.AnnualBonus;
		}
		
		public static int Add(this Employeestruct i, Employeestruct d)
		{
			return i.Salary * 12 + i.AnnualBonus + d.Salary * 12 + d.AnnualBonus;
		}
		
		public static void Sub(this Employeestruct i, Employeestruct d, out Employeestruct e, ref Employeestruct a)
		{
			e = d;
			e.Salary = 10000;
			a.Salary = i.Salary - d.Salary;
			LuaTestCommon.Log ("e.name:" + e.Name +", e.salary:"+ e.Salary);
			LuaTestCommon.Log ("a.name:" + a.Name +", a.salary:"+ a.Salary);


		}
	}

	[LuaCallCSharp]
	public static class TestExtensionMethodFOrClass
	{
		public static void PrintAllString(this TestChineseString i)
		{
			LuaTestCommon.Log("GetLongChineString:" + i.GetLongChineString());
		}

		public static int GetLongStringLength(this TestChineseString i)
		{
			return i.GetLongChineString ().Length;
		}

		public static int Add(this TestChineseString i, TestChineseString d)
		{
			return i.GetLongChineString ().Length + d.GetLongChineString ().Length;
		}

		public static void Replace(this TestChineseString i , TestChineseString d, out TestChineseString e, ref TestChineseString a)
		{
			e = i;
			a.combine_string = i.short_simple_string;
			a.short_simple_string = i.short_simple_string + d.short_simple_string;
		}
	}
}

[LuaCallCSharp]
public abstract class EmployeeTemplate
{
	public void GetSalary()
	{
		GetBasicSalary();
		AddBonus();
	}

	public abstract int GetBasicSalary();
	public abstract int AddBonus();
}

[LuaCallCSharp]
public class Manager : EmployeeTemplate
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

[GCOptimize]
[LuaCallCSharp]
public class TableAutoTransSimpleClass
{
    public TableAutoTransSimpleClass()
    {
    }
    public TableAutoTransSimpleClass(int x, string y, long z)
    {
        IntVar = x;
        StringVar = y;
        LongVar = z;
    }
    public int IntVar{
    
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

[GCOptimize]
[LuaCallCSharp]
public class TableAutoTransComplexClass
{
    public TableAutoTransComplexClass()
    {
    }

    public TableAutoTransComplexClass(int A, TableAutoTransSimpleClass B )
    {
        IntVar = A;
        ClassVar = B;
    }
    public int IntVar
    {

        get { return A; }

        set { A = value; }
    }

    public TableAutoTransSimpleClass ClassVar
    {

        get { return B; }

        set { B = value; }
    }

    public int A;
    public TableAutoTransSimpleClass B;
}

[GCOptimize]
[LuaCallCSharp]
public struct TableAutoTransSimpleStruct
{
    public byte a;
}

[GCOptimize]
[LuaCallCSharp]
public struct TableAutoTransComplexStruct
{
    public int a;
    public int b;
    public decimal c;
    public TableAutoTransSimpleStruct d;
}

[GCOptimize]
[LuaCallCSharp]
public class TestTableAutoTransClass
{
    public TableAutoTransSimpleClass SimpleClassMethod(TableAutoTransSimpleClass p)
    {
        return p;
    }

    public TableAutoTransComplexClass ComplexClassMethod(TableAutoTransComplexClass p)
    {
        return p;
    }

    public TableAutoTransSimpleStruct SimpleStructMethod(TableAutoTransSimpleStruct p)
    {
        return p;
    }

    public TableAutoTransComplexStruct ComplexStructMethod(TableAutoTransComplexStruct p)
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
[LuaCallCSharp]
public class ReferTestClass
{
    public ReferTestClass(int x, ref int y, out string z)
    {
        var_x = x;
        var_y = y;
        y = y - 1;
        z = "test1";
        var_z = z;
    }

    public ReferTestClass(int x, out string z)
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