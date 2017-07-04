using UnityEngine;
using System.Collections;
using XLua;
using System.Collections.Generic;

[Hotfix]
public class HotfixAfter : MonoBehaviour {
    LuaEnv luaenv = new LuaEnv();

    // Use this for initialization
    void Start () {
		Test ();
    }

    // Update is called once per frame
    void Update () {
	  
	}

	public void Test()
	{
		Debug.Log("this is old function");
	}

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 100, 300, 150), "Hotfix"))
        {
            luaenv.DoString(@"
                xlua.hotfix_after(CS.HotfixAfter, 'Test', function(self)
        			print('this is after lua content')
                end)
            ");

			Test ();
        }
    }
}
