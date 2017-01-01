using UnityEngine;
using System.Collections;

[XLua.Hotfix(XLua.HotfixFlag.Stateful)]
public class StatefullTest {
    public int AProp
    {
        get;
        set;
    }

    public event System.Action<int, double> AEvent;

    public int this[string field]
    {
        get
        {
            return 1;
        }
        set
        {
        }
    }

    public void Start () {
	
	}
	
	void Update () {
	
	}

    public void GenericTest<T>(T a)
    {

    }

    static public void StaticFunc(int a, int b)
    {
    }
    static public void StaticFunc(string a, int b, int c)
    {
    }

    ~StatefullTest()
    {
        Debug.Log("~StatefullTest");
    }
}

