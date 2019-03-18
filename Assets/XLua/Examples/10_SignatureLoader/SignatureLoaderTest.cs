using UnityEngine;
using System.Collections;
using XLua;
using System.IO;

namespace XLuaTest
{
    public class SignatureLoaderTest : MonoBehaviour
    {
        public static string PUBLIC_KEY = "BgIAAACkAABSU0ExAAQAAAEAAQBVDDC5QJ+0uSCJA+EysIC9JBzIsd6wcXa+FuTGXcsJuwyUkabwIiT2+QEjP454RwfSQP8s4VZE1m4npeVD2aDnY4W6ZNJe+V+d9Drt9b+9fc/jushj/5vlEksGBIIC/plU4ZaR6/nDdMIs/JLvhN8lDQthwIYnSLVlPmY1Wgyatw==";

        // Use this for initialization
        void Start()
        {
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
        void Update()
        {

        }
    }
}
