using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace XLuaTest
{

    [LuaCallCSharp]
    public class Foo1Parent
    {
    }

    [LuaCallCSharp]
    public class Foo2Parent
    {
    }

    [LuaCallCSharp]
    public class Foo1Child : Foo1Parent
    {
    }

    [LuaCallCSharp]
    public class Foo2Child : Foo2Parent
    {
    }

    [LuaCallCSharp]
    public class Foo
    {
        #region Supported methods

        public void Test1<T>(T a) where T : Foo1Parent
        {
            Debug.Log(string.Format("Test1<{0}>", typeof(T)));
        }

        public T1 Test2<T1, T2>(T1 a, T2 b, GameObject c) where T1 : Foo1Parent where T2 : Foo2Parent
        {
            Debug.Log(string.Format("Test2<{0},{1}>", typeof(T1), typeof(T2)), c);
            return a;
        }

        #endregion

        #region Unsupported methods

        /// <summary>
        /// 不支持生成lua的泛型方法（没有泛型约束）
        /// </summary>
        public void UnsupportedMethod1<T>(T a)
        {
            Debug.Log("UnsupportedMethod1");
        }

        /// <summary>
        /// 不支持生成lua的泛型方法（缺少带约束的泛型参数）
        /// </summary>
        public void UnsupportedMethod2<T>() where T : Foo1Parent
        {
            Debug.Log(string.Format("UnsupportedMethod2<{0}>", typeof(T)));
        }

        /// <summary>
        /// 不支持生成lua的泛型方法（泛型约束必须为class）
        /// </summary>
        public void UnsupportedMethod3<T>(T a) where T : IDisposable
        {
            Debug.Log(string.Format("UnsupportedMethod3<{0}>", typeof(T)));
        }

        #endregion
    }

    [LuaCallCSharp]
    public static class FooExtension
    {
        public static void PlainExtension(this Foo1Parent a)
        {
            Debug.Log("PlainExtension");
        }

        public static T Extension1<T>(this T a) where T : Foo1Parent
        {
            Debug.Log(string.Format("Extension1<{0}>", typeof(T)));
            return a;
        }

        public static T Extension2<T>(this T a, GameObject b) where T : Foo1Parent
        {
            Debug.Log(string.Format("Extension2<{0}>", typeof(T)), b);
            return a;
        }

        public static void Extension2<T1, T2>(this T1 a, T2 b) where T1 : Foo1Parent where T2 : Foo2Parent
        {
            Debug.Log(string.Format("Extension2<{0},{1}>", typeof(T1), typeof(T2)));
        }

        public static T UnsupportedExtension<T>(this GameObject obj) where T : Component
        {
            return obj.GetComponent<T>();
        }
    }
}