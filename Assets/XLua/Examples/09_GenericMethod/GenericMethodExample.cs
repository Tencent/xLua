using UnityEngine;
using XLua;

namespace XLuaTest
{

    public class GenericMethodExample : MonoBehaviour
    {
        private const string script = @"
        local foo1 = CS.XLuaTest.Foo1Child()
        local foo2 = CS.XLuaTest.Foo2Child()

        local obj = CS.UnityEngine.GameObject()
        foo1:PlainExtension()
        foo1:Extension1()
        foo1:Extension2(obj) -- overload1
        foo1:Extension2(foo2) -- overload2
        
        local foo = CS.XLuaTest.Foo()
        foo:Test1(foo1)
        foo:Test2(foo1,foo2,obj)
";
        private LuaEnv env;

        private void Start()
        {
            env = new LuaEnv();
            env.DoString(script);
        }

        private void Update()
        {
            if (env != null)
                env.Tick();
        }

        private void OnDestroy()
        {
            env.Dispose();
        }
    }
}
