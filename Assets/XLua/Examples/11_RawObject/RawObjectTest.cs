using UnityEngine;
using XLua;

namespace XLuaTest
{
    public class IntObject : RawObject
    {
        int mTarget;

        public IntObject(int i)
        {
            mTarget = i;
        }

        public object Target
        {
            get
            {
                return mTarget;
            }
        }
    }

    public class RawObjectTest : MonoBehaviour
    {
        public static void PrintType(object o)
        {
            Debug.Log("type:" + o.GetType() + ", value:" + o);
        }

        // Use this for initialization
        void Start()
        {
            LuaEnv luaenv = new LuaEnv();
            //直接传1234到一个object参数，xLua将选择能保留最大精度的long来传递
            luaenv.DoString("CS.XLuaTest.RawObjectTest.PrintType(1234)");
            //通过一个继承RawObject的类，能实现指明以一个int来传递
            luaenv.DoString("CS.XLuaTest.RawObjectTest.PrintType(CS.XLuaTest.IntObject(1234))");
            luaenv.Dispose();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
