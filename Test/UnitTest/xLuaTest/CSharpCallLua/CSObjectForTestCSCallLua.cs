#if !XLUA_GENERAL
using UnityEngine;
#endif
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;
using System.IO;

public class tableValue1ClassEqual{
	public int key1;
	public int key2;
	public bool key3;
	public List<int> tableValueInclude;
}

public class tableValue1ClassLess{
	public int key1;
	public int key2;
	public tableValue1ClassLess()
	{
	}
}

public class tableValue1ClassMore{
	public int key1;
	public int key2;
	public bool key3;
	public int key4;
}

public class tableValue1ClassPrivate{
	public int key1;
	private int key2;
	public bool key3;
	public int Get()
	{
		return key2;
	}
	public void Set(int value)
	{
		this.key2 = value;
	}
}

public class tableValue1ClassParamConstucter{
	public int key1;
	public int key2;
	public bool key3;
	public tableValue1ClassParamConstucter(int key1, int key2, bool key3)
	{
		this.key1 = key1;
		this.key2 = key2;
		this.key3 = key3;
	}
}

public class tableValue1ClassTwoConstructer{
	public int key1;
	public int key2;
	public bool key3;
	public tableValue1ClassTwoConstructer()
	{
	}

	public tableValue1ClassTwoConstructer(int key1, int key2, bool key3)
	{
		this.key1 = key1;
		this.key2 = key2;
		this.key3 = key3;
	}
}

public class tableValue1ClassException{
	public int key1;
	public int key2;
	public bool key3;
	public tableValue1ClassException()
	{
		throw new Exception("constructor throw exception."); 
	}
}

public class tableValue2Class
{
	public bool kv1;
	public string kv3;
	public int kv2;
}

public class tableValue4Class
{
	public int k1;
	public int k2;
	public int k3;
	public int k4;
	public int k5;
}

[CSharpCallLua]
public interface tableVarIncludeInf
{
	int ikey1 { get; set; }
	int ikey2 { get; set; }
}

[CSharpCallLua]
public interface tableValue1InfEqual
{
	int key1 { get; set; }
	int key2 { get; set; }
	bool key3 { get; set; }
	tableVarIncludeInf tableVarInclude {get; set; }
	int sub(int a, int b);
}

[CSharpCallLua]
public interface tableValue1InfMore
{
	int key1 { get; set; }
	int key2 { get; set; }
	bool key3 { get; set; }
	string key4 { get; set;}
	int sub(int a, int b);
}

[CSharpCallLua]
public interface tableValue1InfLess
{
	int key1 { get; set; }
	int key2 { get; set; }
	int sub(int a, int b);
}

[CSharpCallLua]
public interface tableValue1InfTypeDiff
{
	string key1 { get; set; }
	int key2 { get; set; }
	int sub(int a, int b);
}

[CSharpCallLua]
public interface tableValue2Inf
{
	bool kv1 { get; set; }
	string kv2 { get; set; }
	int kv3 { get; set; }
}

[CSharpCallLua]
public delegate void FuncSelfINcreaseDelegate();

[CSharpCallLua]
public delegate int FuncAddTableDelegate(LuaTable a);

[CSharpCallLua]
public delegate int FuncAdd2elegate(Dictionary<string, int> a);

[CSharpCallLua]
public delegate Action GetFuncIncreaseDelegate();

[CSharpCallLua]
public delegate bool FuncReturnMultivaluesDelegate1(out string str, out tableValue4Class table);

[CSharpCallLua]
public delegate void FuncReturnMultivaluesDelegate2(out bool update, out string str, out tableValue4Class table);

[CSharpCallLua]
public delegate void FuncReturnMultivaluesDelegate3(out bool update, out string str, out tableValue4Class table, out int a);

[CSharpCallLua]
public delegate bool FuncReturnMultivaluesDelegate4(out string str);

[CSharpCallLua]
public delegate void FuncReturnMultivaluesDelegate31(out bool update, out string str, out tableValue4Class table, out tableValue4Class a);

[CSharpCallLua]
public delegate void FuncMultiParamsDelegate(ref bool a, ref int b, ref string c, out int d);

/*
[CSharpCallLua]
public delegate Action FuncMultiParams2Delegate(ref bool a, int b, out int c);
*/

[CSharpCallLua]
public delegate void FuncMultiParams3Delegate(out int c, int b, ref bool a);

[CSharpCallLua]
public delegate void FuncMultiParams3Delegate2(int b, out int c, ref bool a);

[CSharpCallLua]
public delegate int FucnVarParamsDelegate(int a, int b);

[CSharpCallLua]
public delegate string FucnReadFileDelegate(string filename);

[CSharpCallLua]
public delegate System.Object FuncReturnObjectDelegate(int type);
