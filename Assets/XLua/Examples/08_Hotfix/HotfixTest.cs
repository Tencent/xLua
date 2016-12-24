using UnityEngine;
using System.Collections;
using XLua;

[Hotfix]
public class HotfixTest : MonoBehaviour {
    LuaEnv luaenv = new LuaEnv();

    int tick = 0;

    // Use this for initialization
    void Start () {
    }

    // Update is called once per frame
    void Update () {
	    if (++tick % 50 == 0)
        {
            Debug.Log(">>>>>>>>Update in C#, tick = " + tick);
        }
	}

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 100, 300, 150), "Hotfix"))
        {
            luaenv.DoString(@"
                local tick = 0
                xlua.hotfix(CS.HotfixTest, 'Update', function()
                    tick = tick + 1
                    if (tick % 50) == 0 then
                        print('<<<<<<<<Update in lua, tick = ' .. tick)
                    end
                end)
            ");
        }
    }
}
