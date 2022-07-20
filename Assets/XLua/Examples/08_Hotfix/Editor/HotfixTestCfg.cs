using System;
using System.Collections.Generic;
using XLua;

namespace XLuaTest.Editor
{
    public static class HotfixTestCfg
    {
        [Hotfix]
        public static List<Type> by_field = new List<Type>()
        {
            typeof(HotfixTest),
            typeof(HotfixCalc),
            typeof(GenericClass<>),
            typeof(InnerTypeTest),
            typeof(BaseTest),
            typeof(StructTest),
            typeof(GenericStruct<>),
            typeof(StatefullTest)
        };

    }
}