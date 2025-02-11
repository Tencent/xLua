using System;
using UnityEngine;

namespace XLua
{
    public partial class LuaAsset : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private string _code;

        public string Code => _code;
        
        /**
         * 保留 “text” 属性兼容以往 TextAsset 的写法
         * Retain the 'text' property for compatibility with the previous TextAsset usage.
         **/
        public string text => _code;

        public override string ToString() => _code;
        
        public static LuaAsset Create(string code)
        {
            var asset = CreateInstance<LuaAsset>();
            asset._code = code;
            return asset;
        }
        
        // In the future, we may be able to add other extended features here.
    }
}