#if !XLUA_GENERAL
using UnityEngine;
#endif
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;
using System.IO;

[LuaCallCSharp]
public class NoContClass{
	public int key1;
	public int key2;
	public bool key3;
	public int add (int a, int b)
	{
		return a + b;
	}
	public static int d;
	public static int dec(int a, int b)
	{
		return a - b;
	}
}

namespace testLuaCallCS
{
	[LuaCallCSharp]
	public class OneParamContClass{
		public int key1;
		public int key2;
		public bool key3;
		public int add (int a, int b)
		{
			return a + b;
		}
		public OneParamContClass(int a)
		{
			key1 = a;
			key2 = 1;
		}
	}

	[LuaCallCSharp]
	public class MultiContClass{
		public int key1;
		public int key2;
		public bool key3;
		public int add (int a, int b)
		{
			return a + b;
		}
		public MultiContClass()
		{
			key1 = 1;
			key2 = 1;
		}
		
		public MultiContClass(int a)
		{
			key1 = a;
			key2 = 1;
		}
		public static int d;
		public static int dec(int a, int b)
		{
			return a - b;
		}
	}

	[LuaCallCSharp]
	public class TwoParamsContClass{
		public int key1;
		public int key2;
		public bool key3;
		public int add (int a, int b)
		{
			return a + b;
		}
		public TwoParamsContClass(int a, int b)
		{
			key1 = a;
			key2 = b;
		}
	}

	[LuaCallCSharp]
	public class OverClassA : MultiContClass
	{
		public int key3;
		public int sub(int a, int b)
		{
			return a - b;
		}
	}

	[LuaCallCSharp]
	public class OverClassB : MultiContClass
	{
		public int key4;
		public int sub(int a, int b)
		{
			return a - b;
		}
		public static bool bValue;
		public static void Set(bool flag)
		{
			bValue = flag;
		}

		protected internal int sum(int a, int b, int c)
		{
			return a + b + c;
		}
	}

	[LuaCallCSharp]
	public class OverClassC : OverClassB
	{
		public int key5;
		public int div(int a, int b)
		{
			if (b != 0) {
				return a / b;
			} else {
				return 0;
			}
		}

		public int sub(int a, int b)
		{
			return a - b + 1;
		}
	}

	public class NoGenOverClassA : MultiContClass
	{
		public int key4;
		public int sub(int a, int b)
		{
			return a - b;
		}
		public static bool bValue;
		public static void Set(bool flag)
		{
			bValue = flag;
		}
	}

	[LuaCallCSharp]
	public class OverClassCDeriveNGA : NoGenOverClassA
	{
		public int key5;
		public int div(int a, int b)
		{
			if (b != 0) {
				return a / b;
			} else {
				return 0;
			}
		}
	}

	[LuaCallCSharp]
	public abstract class abstractFatherClass
	{
		public abstract int add(int a, int b);
		public int a;
	}

	[LuaCallCSharp]
	public class ChildCalss:abstractFatherClass
	{
		public override int add(int a, int b)
		{
			return a + b;
		}
		public void setA( int value)
		{
			a = value;
		}
		public int getA()
		{
			return a;
		}
	}
	
	[LuaCallCSharp]
	public static class StaticTestClass
	{
		public static int n = 0;
		
		public static void Add()
		{
			n++;
		}
	}

}
