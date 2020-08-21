using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using LuaAPI = XLua.LuaDLL.Lua;
namespace XLuaTest
{
    public class LuaCSFunctionExample : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            LuaEnv luaenv = new LuaEnv();
            string lua = @"
                CS.XLuaTest.LuaCSFunc_Reflection.DebugLog('reflection static call')
                CS.XLuaTest.LuaCSFunc_GenCode.DebugLog('gencode static call')
                CS.XLuaTest.LuaCSFunc_Reflection():MemberCall('reflection member call')
                CS.XLuaTest.LuaCSFunc_GenCode():MemberCall('gencode member call')
            ";
            luaenv.DoString(lua);
        }

    }
    
    class LuaCSFunc_Reflection
    {
        string name = "LuaCSFunc_Reflection";

        [LuaCSFunction]
        public static int DebugLog(IntPtr L)
        {
            var str = LuaAPI.lua_tostring(L, 1);
            Debug.Log(str);
            return 0;
        }

        [LuaCSFunction]
        public int MemberCall(IntPtr L)
        {
            var str = LuaAPI.lua_tostring(L, 1);
            Debug.Log(name + ":" + str);
            return 0;
        }
    }

    [LuaCallCSharp]
    public class LuaCSFunc_GenCode
    {
        string name = "LuaCSFunc_GenCode";

        [LuaCSFunction]
        public static int DebugLog(IntPtr L)
        {
            var str = LuaAPI.lua_tostring(L, 1);
            Debug.Log(str);
            return 0;
        }

        [LuaCSFunction]
        public int MemberCall(IntPtr L)
        {
            var str = LuaAPI.lua_tostring(L, 1);
            Debug.Log(name+":"+str);
            return 0;
        }
    }
}