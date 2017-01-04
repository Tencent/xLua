require("ltest.init")

function islua53() return not not math.type end
-- for test case
CMyTestCaseCSCallLua = TestCase:new()
function CMyTestCaseCSCallLua:new(oo)
    local o = oo or {}
    o.count = 1
    
    setmetatable(o, self)
    self.__index = self
    return o
end

function CMyTestCaseCSCallLua.SetUpTestCase(self)
    self.count = 1 + self.count
	self.tcForTestCSCallLuaObj = CS.TCForTestCSCallLua()
	print("CMyTestCaseCSCallLua.SetUpTestCase")
end

function CMyTestCaseCSCallLua.TearDownTestCase(self)
    self.count = 1 + self.count
	print("CMyTestCaseCSCallLua.TearDownTestCase")
end


function CMyTestCaseCSCallLua.SetUp(self)
    self.count = 1 + self.count
	print("CMyTestCaseCSCallLua.SetUp")
end

function CMyTestCaseCSCallLua.TearDown(self)
    self.count = 1 + self.count
	print("CMyTestCaseCSCallLua.TearDown")
end

function CMyTestCaseCSCallLua.testDoString2LoadLua_Step_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testDoString2LoadLua_Step_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testDoString2LoadLua_Step_2(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testDoString2LoadLua_Step_2()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testDoString2LoadLua_Step_3(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testDoString2LoadLua_Step_3()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testRequire2LoadLua_Step_1_3(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testRequire2LoadLua_Step_1_3()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testRequire2LoadLua_Step_4(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testRequire2LoadLua_Step_4()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end


function CMyTestCaseCSCallLua.testRequire2LoadLua_Step_5(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testRequire2LoadLua_Step_5()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end 

function CMyTestCaseCSCallLua.testRequire2LoadLua_Step_6(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testRequire2LoadLua_Step_6()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testRequire2LoadLua_Step_7(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testRequire2LoadLua_Step_7()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testAddLoader2LoadLua_Step_1_2(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testAddLoader2LoadLua_Step_1_2()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testAddLoader2LoadLua_Step_3(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testAddLoader2LoadLua_Step_3()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testAddLoader2LoadLua_Step_6(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testAddLoader2LoadLua_Step_6()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testAddLoader2LoadLua_Step_7(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testAddLoader2LoadLua_Step_7()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeBool_Step_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeBool_Step_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeString_Step_2(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeString_Step_2()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToByte(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToByte()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToSByte(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToSByte()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToShort(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToShort()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToUShort(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToUShort()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToInt(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToInt()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToUInt(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToUInt()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToLong(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToLong()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToULong(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToULong()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToDouble(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToDouble()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToChar(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToChar()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToFloat(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToFloat()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataTypeNumberToDecimal(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataTypeNumberToDecimal()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataType_Step_4(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataType_Step_4()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetBasicDataType_Step_5(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetBasicDataType_Step_5()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToClass_Step_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToClass_Step_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToClass_Step_2(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToClass_Step_2()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToClass_Step_3(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToClass_Step_3()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToClass_Step_4(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToClass_Step_4()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToClass_Step_5(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToClass_Step_5()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToClass_Step_1_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToClass_Step_1_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToClass_Step_1_3(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToClass_Step_1_3()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToClass_Step_1_4(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToClass_Step_1_4()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToInterface_Step_6(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToInterface_Step_6()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToInterface_Step_7(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToInterface_Step_7()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToInterface_Step_8(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToInterface_Step_8()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToInterface_Step_9(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToInterface_Step_9()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToInterface_Step_6_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToInterface_Step_6_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToDic_Step_10(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToDic_Step_10()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToDic_Step_11_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToDic_Step_11_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToDic_Step_11_2(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToDic_Step_11_2()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToDic_Step_11_3(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToDic_Step_11_3()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToList_Step_12(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToList_Step_12()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToList_Step_13_1_int(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToList_Step_13_1_int()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToList_Step_13_1_string(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToList_Step_13_1_string()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToList_Step_13_2(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToList_Step_13_2()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetTableToLuaTable_Step_14(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetTableToLuaTable_Step_14()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end


function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_2_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_2_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_2_2(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_2_2()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_2_3(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_2_3()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_2_4(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_2_4()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_3(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_3()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_5(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_5()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_5_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_5_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_5_1_0(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_5_1_0()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

--[[ ios上il2cpp编译有错，注释掉
function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_5_2(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_5_2()
	ASSERT_EQ(ret.result, true)
end
]]

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_6(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_6()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToLuaFunc_Step_8(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToLuaFunc_Step_8()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToLuaFunc_Step_9_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToLuaFunc_Step_9_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToLuaFunc_Step_10(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToLuaFunc_Step_10()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToLuaFunc_Step_12(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToLuaFunc_Step_12()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToLuaFunc_Step_13(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToLuaFunc_Step_13()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_7_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_7_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_7_2(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_7_2()
	print(ret.msg)
	if islua53() then
		--ASSERT_EQ(false, false) --该用例在lua5.3无意义
		ASSERT_EQ(ret.result, true)
	else
		ASSERT_EQ(ret.result, true)
	end	
end

function CMyTestCaseCSCallLua.testGetFuncToDelegate_Step_7_3(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testGetFuncToDelegate_Step_7_3()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_int_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_int_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_int_2(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_int_2()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_string_1(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_string_1()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_sbyte(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_sbyte()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_byte(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_byte()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_short(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_short()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_ushort(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_ushort()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_long(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_long()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_ulong(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_ulong()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_double(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_double()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_float(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_float()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_char(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_char()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_BasicType_decimal(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_BasicType_decimal()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_struct(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_struct()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_class(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_class()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_interface(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_interface()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_dict(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_dict()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_list(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_list()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end

function CMyTestCaseCSCallLua.testLuaTableGetSetKeyValue_delegate(self)
    self.count = 1 + self.count
	local ret = self.tcForTestCSCallLuaObj:testLuaTableGetSetKeyValue_delegate()
	print(ret.msg)
	ASSERT_EQ(ret.result, true)
end