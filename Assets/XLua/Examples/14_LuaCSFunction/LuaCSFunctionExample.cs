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
                CS.XLuaTest.LuaCSFunc_Reflection():MemberCall('reflection member call')
                CS.XLuaTest.LuaCSFunc_Reflection():ExLog('reflection ex call')
                CS.XLuaTest.LuaCSFunc_GenCode.DebugLog('gencode static call')
                CS.XLuaTest.LuaCSFunc_GenCode():MemberCall('gencode member call')
                CS.XLuaTest.LuaCSFunc_GenCode():MemberCall('gencode ex call')

            ";
            luaenv.DoString(lua);
            //定义 LUACSFUNC_TRY_CATCH 宏，Catch错误并打印lua栈
            luaenv.DoString("CS.XLuaTest.LuaCSFunction_TryCatch():MemberCall('Error member call')");
            luaenv.DoString("CS.XLuaTest.LuaCSFunction_TryCatch.DebugLog('Error static call')");

        }

    }
    public class LuaCSFunc_Reflection
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

    [LuaCallCSharp]
    public class LuaCSFunction_TryCatch
    {
        string name = "LuaCSFunc_TryCatch";

        [LuaCSFunction]
        public static int DebugLog(IntPtr L)
        {
            var str = LuaAPI.lua_tostring(L, 1);
            throw new Exception(str);
        }

        [LuaCSFunction]
        public int MemberCall(IntPtr L)
        {
            var str = LuaAPI.lua_tostring(L, 1);
            throw new Exception(name + ":" + str);
        }
    }

    [LuaCallCSharp]
    public static class Extension
    {
        [LuaCSFunction]
        public static int ExLog(this LuaCSFunc_GenCode obj, IntPtr L)
        {
            return obj.MemberCall(L);
        }

        [LuaCSFunction]
        public static int ExLog(this LuaCSFunc_Reflection obj, IntPtr L)
        {
            return obj.MemberCall(L);
        }
    }

}