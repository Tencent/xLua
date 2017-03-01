using XLua;
using System.Collections.Generic;
using System;

public static class XLuaTestCfg
{
    [LuaCallCSharp]
    public static List<Type> lua_call_cs = new List<Type>()
    {
        typeof(AccessByGenGode),
    };

    [Hotfix]
    public static List<Type> hotfix
    {
        get
        {
            return new List<Type>() { typeof(CalcByConfig) };
        }
    }
}

[LuaCallCSharp]
[Hotfix]
public class Calc
{
    public int Add(int a, int b)
    {
        return a - b;
    }
}

public class CalcByConfig
{
    public int Add(int a, int b)
    {
        return a * b;
    }
}

[LuaCallCSharp]
[GCOptimize]
public struct Point
{
    public Point(float _x, float _y)
    {
        x = _x;
        y = _y;
    }
    public float x;
    public float y;
}

public class AccessByGenGode
{
    public void Print(Point pos)
    {
        Console.WriteLine("by gen code: x=" + pos.x + ",y=" + pos.y);
    }
}

public class AccessByReflection
{
    public void Print(Point pos)
    {
        Console.WriteLine("by reflection: x=" + pos.x + ",y=" + pos.y);
    }
}

public class XLuaTest
{
    [CSharpCallLua]
    public delegate double LuaMax(double a, double b);

    public static void Main()
    {
        LuaEnv luaenv = new LuaEnv();
        luaenv.DoString("CS.System.Console.WriteLine('hello world')");
        
        var max = luaenv.Global.GetInPath<LuaMax>("math.max");
        Console.WriteLine("max:" + max(32, 12));

        luaenv.Global.Set("obj1", new AccessByGenGode());
        luaenv.Global.Set("obj2", new AccessByReflection());

        luaenv.DoString(@"
            local p = CS.Point(3, 4)
            print('-----------------------------')
            obj2:Print(p)
            print('-----------------------------')
            obj1:Print(p)
            print('-----------------------------')
        ");


        var calc = new Calc();
        luaenv.Global.Set("calc", calc);
        luaenv.DoString("print(calc:Add(2, 4))");


        try
        {
            Console.WriteLine("2 + 4 =" +calc.Add(2, 4));
            luaenv.DoString(@"
                xlua.hotfix(CS.Calc, 'Add', function(self, a, b)
                    return a + b
                end)
            ");
            Console.WriteLine("2 + 4 =" + calc.Add(2, 4));

            CalcByConfig calc2 = new CalcByConfig();
            Console.WriteLine("2 + 4 =" + calc2.Add(2, 4));
            luaenv.DoString(@"
                xlua.hotfix(CS.CalcByConfig, 'Add', function(self, a, b)
                    return a + b
                end)
            ");
            Console.WriteLine("2 + 4 =" + calc2.Add(2, 4));

            // Õ∑≈hotfix
            luaenv.DoString(@"
                xlua.hotfix(CS.Calc, 'Add', nil)
                xlua.hotfix(CS.CalcByConfig, 'Add', nil)
            ");
        }
        catch(Exception e)
        {
            Console.WriteLine("Hotfix exception:" + e);
        }
        max = null;
        luaenv.Dispose();
    }
}