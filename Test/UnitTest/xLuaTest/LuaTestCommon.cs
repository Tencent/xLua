using XLua;
#if !XLUA_GENERAL
using UnityEngine;
#endif

using System.Collections.Generic;
using System;
public class LuaEnvSingleton  {
	
	static private LuaEnv instance = null;
	static public LuaEnv Instance
	{
		get
		{
			if(instance == null)
			{
				instance = new LuaEnv();
#if XLUA_GENERAL
                instance.DoString("package.path = package.path..';../Test/UnitTest/xLuaTest/CSharpCallLua/Resources/?.lua.txt;../Test/UnitTest/StreamingAssets/?.lua'");
#endif
            }

            return instance;
		}
	}
}

[LuaCallCSharp]
public class LuaTestCommon
{
#if     UNITY_IOS || UNITY_IPHONE
    public static string resultPath = Application.persistentDataPath + "/";
	public static string xxxtdrfilepath = Application.dataPath + "/Raw" + "/testxxx.tdr";
	public static string xxxtdr2filepath = Application.dataPath + "/Raw" + "/testxxx2.tdr";
	public static bool android_platform = false;
#elif   UNITY_ANDROID
    public static string resultPath = "/sdcard/luatest/";
	public static string xxxtdrfilepath = Application.streamingAssetsPath + "/testxxx.tdr";
	public static string xxxtdr2filepath = Application.streamingAssetsPath + "/testxxx2.tdr";
	public static bool android_platform = true;
#elif   UNITY_EDITOR || UNITY_WSA
    public static string resultPath = Application.dataPath + "/xLuaTest/";
	public static string xxxtdrfilepath = Application.dataPath + "/StreamingAssets" + "/testxxx.tdr";
	public static string xxxtdr2filepath = Application.dataPath + "/StreamingAssets" + "/testxxx2.tdr";
	public static bool android_platform = false;
#elif XLUA_GENERAL
    public static string resultPath = ".";
    public static bool android_platform = false;
    public static string xxxtdrfilepath = "../Test/UnitTest/StreamingAssets" + "/testxxx.tdr";
#endif

    public static bool IsXLuaGeneral()
    {
#if XLUA_GENERAL
        return true;
#else
        return false;
#endif
    }

    public static bool IsMacPlatform()
	{
#if UNITY_EDITOR
        string os = System.Environment.OSVersion.ToString();
        if (os.Contains("Unix"))
        {
            return true;
        }
        else
        {
            return false;
        }
#else
        return false;
#endif
	}

	public static bool IsIOSPlatform()
	{
#if UNITY_IOS || UNITY_IPHONE
		return true;
#else
		return false;
#endif
	}

    public static void Log(string str)
    {
#if XLUA_GENERAL
        System.Console.WriteLine(str);
#else
        UnityEngine.Debug.Log(str);
#endif
    }
}

#if !XLUA_GENERAL
//注意：用户自己代码不建议在这里配置，建议通过标签来声明!!
public class TestCaseGenConfig
{

    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
    [LuaCallCSharp]
    public List<Type> LuaCallCSharp
    {
        get
        {
            return new List<Type>()
            {
                typeof(UnityEngine.TextAsset),
            };
        }
    }
}
#endif
