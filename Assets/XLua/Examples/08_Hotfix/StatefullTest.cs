using UnityEngine;

namespace XLuaTest
{
    [XLua.Hotfix]
    public class StatefullTest
    {
        public StatefullTest()
        {

        }

        public StatefullTest(int a, int b)
        {
            if (a > 0)
            {
                return;
            }

            Debug.Log("a=" + a);
            if (b > 0)
            {
                return;
            }
            else
            {
                if (a + b > 0)
                {
                    return;
                }
            }
            Debug.Log("b=" + b);
        }

        public int AProp
        {
            get;
            set;
        }

        public event System.Action<int, double> AEvent;

        public int this[string field]
        {
            get
            {
                return 1;
            }
            set
            {
            }
        }

        public void Start()
        {

        }

        void Update()
        {

        }

        public void GenericTest<T>(T a)
        {

        }

        static public void StaticFunc(int a, int b)
        {
        }
        static public void StaticFunc(string a, int b, int c)
        {
        }

        ~StatefullTest()
        {
            Debug.Log("~StatefullTest");
        }
    }
}

