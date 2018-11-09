using System.Text;
using UnityEngine;
using XLua;

namespace XLuaTest
{
    public class RawByteArray : RawObject
    {
        public byte[] m_Target;
        public object Target { get { return m_Target; } }

        public RawByteArray(int length)
        {
            m_Target = new byte[length];
        }
        public RawByteArray(byte[] data)
        {
            m_Target = data;
        }

        public byte this[int index]
        {
            get { return m_Target[index]; }
            set { m_Target[index] = value; }
        }
    }

    public class RawObjectTest : MonoBehaviour
    {
        public static void PrintType(object o)
        {
            Debug.Log("type:" + o.GetType() + ", value:" + o);
        }

        public static byte[] BytesFromString(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        // Use this for initialization
        void Start()
        {
            LuaEnv luaenv = new LuaEnv();
            //直接传1234到一个object参数，xLua将选择能保留最大精度的long来传递
            luaenv.DoString("CS.XLuaTest.RawObjectTest.PrintType(1234)");
            //通过一个继承RawObject的类，能实现指明以一个int来传递
            luaenv.DoString("CS.XLuaTest.RawObjectTest.PrintType(CS.XLua.Cast.Int32(1234))");
            //直接传递byte[]，lua会作为string处理
            luaenv.DoString("CS.XLuaTest.RawObjectTest.PrintType(CS.XLuaTest.RawObjectTest.BytesFromString('string from lua'))");
            //通过继承RawObject的RawByteArray，实现string以byte[]方式传递
            luaenv.DoString("CS.XLuaTest.RawObjectTest.PrintType(CS.XLuaTest.RawByteArray('string from lua'))");
            luaenv.Dispose();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
