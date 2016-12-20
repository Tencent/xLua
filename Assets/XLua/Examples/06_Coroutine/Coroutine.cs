using UnityEngine;
using XLua;
using System.Collections.Generic;
using System.Collections;
using System;

public class Coroutine : MonoBehaviour {
    LuaEnv luaenv = null;
    // Use this for initialization
    void Start()
    {
        luaenv = new LuaEnv();
        luaenv.DoString("require 'coruntine_test'");
    }

    // Update is called once per frame
    void Update()
    {
        if (luaenv != null)
        {
            luaenv.Tick();
        }
    }

    void OnDestroy()
    {
        luaenv.Dispose();
    }
}
