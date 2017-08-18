#if !XLUA_GENERAL
using UnityEngine;
using System.Collections;
using XLua;

public class Main : MonoBehaviour {
	LuaEnv luaenv;
	// Use this for initialization
	void Start () {
		luaenv = LuaEnvSingleton.Instance;
		luaenv.DoString ("require 'main'");
	}
	
	// Update is called once per frame
	void Update () {
		luaenv.GC ();
	}
}
#endif