using UnityEngine;
using System.Collections;
using XLua;

[Hotfix]
public class HotfixTest : MonoBehaviour {
    LuaEnv luaenv = new LuaEnv();

    public int tick = 0; //如果是private的，在lua设置xlua.private_accessible(CS.HotfixTest)后即可访问

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
                xlua.hotfix(CS.HotfixTest, 'Update', function(self)
                    self.tick = self.tick + 1
                    if (self.tick % 50) == 0 then
                        print('<<<<<<<<Update in lua, tick = ' .. self.tick)
                    end
                end)
            ");
        }
    }
}
