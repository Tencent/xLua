using XLua;
using System.Collections.Generic;
using System;

public class XLuaUnitTest
{

    public static void Main()
    {
        LuaEnv luaenv = LuaEnvSingleton.Instance;
        luaenv.DoString("require 'main'");
    }
}