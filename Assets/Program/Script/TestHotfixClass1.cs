using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Obsolete("测试Obsolete对于类的影响")]
public class TestHotfixClass1 : MonoBehaviour
{
    public class Data
    {
        public Color color;

        /// <summary>
        /// 这个构造函数的参数，不能是int时会导致xlua注入失败，提示：Error:can not find delegate for DNA.UI.PanelCommonModalMaskController/OpenData..ctor! try re-genertate code.
        /// </summary>
        /// <param name="color"></param>
        public Data(Color color)
        {
            this.color = color;
        }
    }
}