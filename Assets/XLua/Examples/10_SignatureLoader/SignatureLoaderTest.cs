using UnityEngine;
using System.Collections;
using XLua;
using System.IO;

public class SignatureLoaderTest : MonoBehaviour {
    public static string PUBLIC_KEY = "<RSAKeyValue><Modulus>7ubRuOTj3WZzD+qjlpZTV/BT/itYZytcXzrVjlxqtotq7ajB6DlVmLxOEn9ikw2HQKVrvedFMjSZipJHXDHsg9zClLV8QI3e21gfy7T4wYpQONWca+GygxMAuYslrd/P9o+TmcQXWRZHB+AFia59JAoQoIOOz/vtAnJ+s6xf7Pk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

    // Use this for initialization
    void Start () {
        LuaEnv luaenv = new LuaEnv();
#if UNITY_EDITOR
        luaenv.AddLoader(new SignatureLoader(PUBLIC_KEY, (ref string filepath) =>
        {
            filepath = Application.dataPath + "/XLua/Examples/10_SignatureLoader/" + filepath.Replace('.', '/') + ".lua";
            if (File.Exists(filepath))
            {
                return File.ReadAllBytes(filepath);
            }
            else
            {
                return null;
            }
        }));
#else //为了让手机也能测试
        luaenv.AddLoader(new SignatureLoader(PUBLIC_KEY, (ref string filepath) =>
        {
            filepath = filepath.Replace('.', '/') + ".lua";
            TextAsset file = (TextAsset)Resources.Load(filepath);
            if (file != null)
            {
                return file.bytes;
            }
            else
            {
                return null;
            }
        }));
#endif
        luaenv.DoString(@"
            require 'signatured1'
            require 'signatured2'
        ");
        luaenv.Dispose();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
