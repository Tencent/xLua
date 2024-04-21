using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using XLua;

namespace XLuaTest
{
    [Hotfix]
    public partial class HotfixAsyncAwaitTest : MonoBehaviour
    {
        [Hotfix]
        [ContextMenu("Method1")]
        public void Method1()
        {
            Debug.Log($"Method1 in C#!");
        }

        [Hotfix]
        [ContextMenu("AsyncMethod1")]
#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        public async void AsyncMethod1()
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        {
            Debug.Log($"AsyncMethod1 in C#!");
        }

        [Hotfix]
        [ContextMenu("AsyncMethod2")]
        public async void AsyncMethod2()
        {
            Debug.Log($"AsyncMethod2 in C#!    001");
            await Task.Delay(1000);
            Debug.Log($"AsyncMethod2 in C#!    002");
        }

        [Hotfix]
        [ContextMenu("AsyncMethod3")]
        public async void AsyncMethod3()
        {
            Debug.Log($"AsyncMethod3 in C#!    001");
            await Task.Delay(1000);
            Debug.Log($"AsyncMethod3 in C#!    002");
            await MyTask();
            Debug.Log($"AsyncMethod3 in C#!    003");
        }

        [Hotfix]
        [ContextMenu("AsyncMethod4")]
        public async void AsyncMethod4()
        {
            Debug.Log($"AsyncMethod4 in C#!    001");
            var result = await MyTask1();
            Debug.Log($"AsyncMethod4 in C#!    002    MyTask1 Result:{result}");
        }

        [Hotfix]
        [ContextMenu("MyTask")]
        public async Task MyTask()
        {
            Debug.Log($"MyTask in C#!    001");
            await Task.Delay(1000);
            Debug.Log($"MyTask in C#!    002");
            return;
        }

        [Hotfix]
        [ContextMenu("MyTask1")]
        public async Task<int> MyTask1()
        {
            Debug.Log($"MyTask1 in C#!    001");
            await Task.Delay(1000);
            Debug.Log($"MyTask1 in C#!    002");
            return 9999;
        }

        [Hotfix]
        [ContextMenu("MyTask2")]
        public Task<int> MyTask2()
        {
            Debug.Log($"MyTask2 in C#!    001");
            return Task.FromResult(9998);
        }
    }

    public partial class HotfixAsyncAwaitTest
    {
        public TextAsset Method1Lua;
        public TextAsset AsyncMethod1Lua;
        public TextAsset AsyncMethod2Lua;
        public TextAsset AsyncMethod3Lua;
        public TextAsset AsyncMethod4Lua;
        public TextAsset MyTaskLua;
        public TextAsset MyTask1Lua;
        public TextAsset MyTask2Lua;

        internal static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!

        [ContextMenu("DoHotfix Method1")]
        public void DoHotfixMethod1()
        {
            // 执行脚本
            luaEnv.DoString(Method1Lua.text);
        }

        [ContextMenu("DoHotfix AsyncMethod1")]
        public void DoHotfixAsyncMethod1()
        {
            // 执行脚本
            luaEnv.DoString(AsyncMethod1Lua.text);
        }

        [ContextMenu("DoHotfix AsyncMethod2")]
        public void DoHotfixAsyncMethod2()
        {
            // 执行脚本
            luaEnv.DoString(AsyncMethod2Lua.text);
        }

        [ContextMenu("DoHotfix AsyncMethod3")]
        public void DoHotfixAsyncMethod3()
        {
            // 执行脚本
            luaEnv.DoString(AsyncMethod3Lua.text);
        }

        [ContextMenu("DoHotfix AsyncMethod4")]
        public void DoHotfixAsyncMethod4()
        {
            // 执行脚本
            luaEnv.DoString(AsyncMethod4Lua.text);
        }

        [ContextMenu("DoHotfix MyTask")]
        public void DoHotfixMyTask()
        {
            // 执行脚本
            luaEnv.DoString(MyTaskLua.text);
        }

        [ContextMenu("DoHotfix MyTask1")]
        public void DoHotfixMyTask1()
        {
            // 执行脚本
            luaEnv.DoString(MyTask1Lua.text);
        }

        [ContextMenu("DoHotfix MyTask2")]
        public void DoHotfixMyTask2()
        {
            // 执行脚本
            luaEnv.DoString(MyTask2Lua.text);
        }
    }
}





