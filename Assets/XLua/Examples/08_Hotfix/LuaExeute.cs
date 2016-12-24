using UnityEngine;
using System.IO;
using XLua;

public class LuaExeute : MonoBehaviour
{
    static LuaEnv luaenv = new LuaEnv();

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        luaenv.Tick();
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 100, 300, 150), "Execute Lua"))
        {
            luaenv.DoString(File.ReadAllText(Application.dataPath + "/XLua/Examples/08_Hotfix/hotfix.lua"));
        }
    }
}
