#if !XLUA_GENERAL
using UnityEngine;
#endif
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;

public class TestCSCallLua
#if !XLUA_GENERAL
    : MonoBehaviour
#endif
{
    // Use this for initialization
    void Start()
    {
		TCForTestCSCallLua testSuite = new TCForTestCSCallLua();
		testSuite.testDoString2LoadLua_Step_1();
		testSuite.testDoString2LoadLua_Step_2 ();
		testSuite.testDoString2LoadLua_Step_3 ();
		testSuite.testRequire2LoadLua_Step_1_3 ();
		testSuite.testRequire2LoadLua_Step_4 ();
		testSuite.testRequire2LoadLua_Step_5 ();
		testSuite.testRequire2LoadLua_Step_6 ();
		testSuite.testRequire2LoadLua_Step_7 ();
		testSuite.testAddLoader2LoadLua_Step_1_2 ();
		testSuite.testAddLoader2LoadLua_Step_3 ();
		testSuite.testAddLoader2LoadLua_Step_6 ();
		testSuite.testAddLoader2LoadLua_Step_7 ();
		testSuite.testGetBasicDataTypeBool_Step_1 ();
		testSuite.testGetBasicDataTypeString_Step_2 ();
		testSuite.testGetBasicDataTypeNumberToByte ();
		testSuite.testGetBasicDataTypeNumberToSByte ();
		testSuite.testGetBasicDataTypeNumberToShort ();
		testSuite.testGetBasicDataTypeNumberToUShort ();
		testSuite.testGetBasicDataTypeNumberToInt ();
		testSuite.testGetBasicDataTypeNumberToUInt ();
		testSuite.testGetBasicDataTypeNumberToLong ();
		testSuite.testGetBasicDataTypeNumberToULong ();
		testSuite.testGetBasicDataTypeNumberToDouble ();
		testSuite.testGetBasicDataTypeNumberToChar ();
		testSuite.testGetBasicDataTypeNumberToFloat();
		testSuite.testGetBasicDataTypeNumberToDecimal ();
		testSuite.testGetBasicDataType_Step_4 ();
		testSuite.testGetBasicDataType_Step_5 ();
		testSuite.testGetTableToClass_Step_1 ();
		testSuite.testGetTableToClass_Step_2 ();
		testSuite.testGetTableToClass_Step_3 ();
		testSuite.testGetTableToClass_Step_4 ();
		testSuite.testGetTableToClass_Step_5 ();
		testSuite.testGetTableToClass_Step_1_1 ();
		testSuite.testGetTableToClass_Step_1_3 ();
		testSuite.testGetTableToClass_Step_1_4 ();
		testSuite.testGetTableToInterface_Step_6 ();
		testSuite.testGetTableToInterface_Step_7 ();
		testSuite.testGetTableToInterface_Step_8 ();
		testSuite.testGetTableToInterface_Step_9 ();
		testSuite.testGetTableToInterface_Step_6_1 ();
		testSuite.testGetTableToDic_Step_10 ();
		testSuite.testGetTableToDic_Step_11_1 ();
		testSuite.testGetTableToDic_Step_11_2 ();
		testSuite.testGetTableToDic_Step_11_3 ();
		testSuite.testGetTableToList_Step_12 ();
		testSuite.testGetTableToList_Step_13_1_int ();
		testSuite.testGetTableToList_Step_13_1_string ();
		testSuite.testGetTableToList_Step_13_2 ();
		testSuite.testGetTableToLuaTable_Step_14 ();
		testSuite.testGetFuncToDelegate_Step_1 ();
		testSuite.testGetFuncToDelegate_Step_2_1 ();
		testSuite.testGetFuncToDelegate_Step_2_2 ();
		testSuite.testGetFuncToDelegate_Step_2_3 ();
		testSuite.testGetFuncToDelegate_Step_2_4 ();
		testSuite.testGetFuncToDelegate_Step_3 ();
		testSuite.testGetFuncToDelegate_Step_5 ();
		testSuite.testGetFuncToDelegate_Step_5_1 ();
		testSuite.testGetFuncToDelegate_Step_5_1_0 ();
		//testSuite.testGetFuncToDelegate_Step_5_2 (); // 在ios il2cpp上运行错误，注释掉
		testSuite.testGetFuncToDelegate_Step_6 ();
		testSuite.testGetFuncToLuaFunc_Step_8 ();
		testSuite.testGetFuncToLuaFunc_Step_9_1 ();
		testSuite.testGetFuncToLuaFunc_Step_10 ();
		testSuite.testGetFuncToLuaFunc_Step_12 ();
		testSuite.testGetFuncToLuaFunc_Step_13 ();
    }

    // Update is called once per frame
    void Update()
    {
		if (TCForTestCSCallLua.luaEnv != null)
        {
			TCForTestCSCallLua.luaEnv.GC();
        }
    }
}
