#if !XLUA_GENERAL
using UnityEngine;
using System.Collections;
using XLua;

public class Main : MonoBehaviour {
	LuaEnv luaenv;

	void Awake(){
		 var prefab = Resources.Load<GameObject>("LuaEnvStarter");
            if (prefab)
            {
                var go = Instantiate(prefab);

            }
            else
            {
                Debug.LogError("Not generate wraps");
            }
	}
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