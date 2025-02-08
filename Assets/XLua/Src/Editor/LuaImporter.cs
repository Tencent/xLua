#if UNITY_2018_1_OR_NEWER
using UnityEngine;
using UnityEditor;

using System.IO;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace XLua
{
    [ScriptedImporter(2, new[] { "lua" })]
    public class LuaImporter : ScriptedImporter
    {
        private const string IconGUID = "0eb86212f60529a4b9e9deeeebde247c";
        
        const string Tag = "LuaImporter";

        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = LuaAsset.Create(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset("LuaCode", asset, LoadIconTexture());
            ctx.SetMainObject(asset);
        }

        private static Texture2D LoadIconTexture()
        {
            return AssetDatabase.LoadAssetAtPath(
                        AssetDatabase.GUIDToAssetPath(IconGUID), 
                        typeof(Texture2D)
                    ) as Texture2D;
        }
    }
}

#endif