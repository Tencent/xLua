using UnityEngine;
using System.Collections;
using XLua;

[CSharpCallLua]
public delegate int TestOutDelegate(LuaTable calc, int a, out double b, ref string c);

[Hotfix(HotfixFlag.stateful)]
public class HotfixCalc
{
    public int Add(int a, int b)
    {
        return a - b;
    }

    public Vector3 Add(Vector3 a, Vector3 b)
    {
        return a - b;
    }

    public int TestOut(int a, out double b, ref string c)
    {
        b = a + 2;
        c = "wrong version";
        return a + 3;
    }
}

public class NoHotfixCalc
{
    public int Add(int a, int b)
    {
        return a + b;
    }

    public Vector3 Add(Vector3 a, Vector3 b)
    {
        return a - b;
    }

    public int TestOut(int a, out double b, ref string c)
    {
        b = a + 2;
        c = "wrong version";
        return a + 3;
    }
}


public class HotfixTest2 : MonoBehaviour {

	// Use this for initialization
	void Start () {
        LuaEnv luaenv = new LuaEnv();
        luaenv.DoString(@"
            xlua.hotfix(CS.HotfixCalc, 'XLuaConstructor', function()
                    local obj = {}
                    print('constructor called obj=', obj)
                    return obj
                end)
        ");
        HotfixCalc calc = new HotfixCalc();
        NoHotfixCalc ordinaryCalc = new NoHotfixCalc();

        int CALL_TIME = 100 * 1000 * 1000 ;
        var start = System.DateTime.Now;
        for (int i = 0; i < CALL_TIME; i++)
        {
            calc.Add(2, 1);
        }
        var d1 = (System.DateTime.Now - start).TotalMilliseconds;
        Debug.Log("Hotfix using:" + d1);

        start = System.DateTime.Now;
        for (int i = 0; i < CALL_TIME; i++)
        {
            ordinaryCalc.Add(2, 1);
        }
        var d2 = (System.DateTime.Now - start).TotalMilliseconds;
        Debug.Log("No Hotfix using:" + d2);

        Debug.Log("drop:" + ((d1 - d2) / d1));

        Debug.Log("Before Fix: 2 + 1 = " + calc.Add(2, 1));
        Debug.Log("Before Fix: Vector3(2, 3, 4) + Vector3(1, 2, 3) = " + calc.Add(new Vector3(2, 3, 4), new Vector3(1, 2, 3)));
        luaenv.DoString(@"
            xlua.hotfix(CS.HotfixCalc, 'Add', function(self, a, b)
                return a + b
            end)
        ");
        Debug.Log("After Fix: 2 + 1 = " + calc.Add(2, 1));
        Debug.Log("After Fix: Vector3(2, 3, 4) + Vector3(1, 2, 3) = " + calc.Add(new Vector3(2, 3, 4), new Vector3(1, 2, 3)));

        double num;
        string str = "hehe";
        int ret = calc.TestOut(100, out num, ref str);
        Debug.Log("ret = " + ret + ", num = " + num + ", str = " + str);

        luaenv.DoString(@"
            xlua.hotfix(CS.HotfixCalc, 'TestOut', function(self, a, c)
                    print('TestOut', self, a, c)
                    return a + 10, a + 20, 'right version'
                end)
        ");
        str = "hehe";
        ret = calc.TestOut(100, out num, ref str);
        Debug.Log("ret = " + ret + ", num = " + num + ", str = " + str);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
