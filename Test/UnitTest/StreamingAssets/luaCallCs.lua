require("ltest.init")

gTestNumber = 1

function islua53() return not not math.type end
	
-- for test case
CMyTestCaseLuaCallCS = TestCase:new()
function CMyTestCaseLuaCallCS:new(oo)
    local o = oo or {}
    o.count = 1
    
    setmetatable(o, self)
    self.__index = self
    return o
end

function CMyTestCaseLuaCallCS.SetUpTestCase(self)
    self.count = 1 + self.count
	print("CMyTestCaseLuaCallCS.SetUpTestCase")
end

function CMyTestCaseLuaCallCS.TearDownTestCase(self)
    self.count = 1 + self.count
	print("CMyTestCaseLuaCallCS.TearDownTestCase")
end


function CMyTestCaseLuaCallCS.SetUp(self)
    self.count = 1 + self.count
	print("CMyTestCaseLuaCallCS.SetUp")
end

function CMyTestCaseLuaCallCS.TearDown(self)
    self.count = 1 + self.count

	print("CMyTestCaseLuaCallCS.TearDown")
end


function CMyTestCaseLuaCallCS.CaseDefaultParamFunc1(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.DefaultParaFuncSingle(100, "abc")
	ASSERT_EQ(ret, 100)
end

function CMyTestCaseLuaCallCS.CaseDefaultParamFunc2(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.DefaultParaFuncSingle(100)
	ASSERT_EQ(ret, 100)
end

function CMyTestCaseLuaCallCS.CaseDefaultParamFunc3(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.DefaultParaFuncMulti(100, "")
	ASSERT_EQ(ret, 101)
end

function CMyTestCaseLuaCallCS.CaseDefaultParamFunc4(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.DefaultParaFuncMulti(100, "efg", 1)
	ASSERT_EQ(ret, 101)
end

function CMyTestCaseLuaCallCS.CaseDefaultParamFunc5(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.DefaultParaFuncMulti(100, "efg", 1, 98)
	ASSERT_EQ(ret, 101)
end

function CMyTestCaseLuaCallCS.CaseDefaultParamFunc6(self)
    self.count = 1 + self.count
	--if (CS.LuaTestCommon.IsMacPlatform() == false) then
    if (true) then
		local ret, error = pcall(function() CS.LuaTestObj.DefaultParaFuncMulti(100, "efg", 1, 98, 0) end)
		ASSERT_EQ(ret, false)
    else
        ASSERT_EQ(true, false)
	end
end

function CMyTestCaseLuaCallCS.CaseVariableParamFunc1(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.VariableParamFunc(0)
	ASSERT_EQ(ret, 0)
end

function CMyTestCaseLuaCallCS.CaseVariableParamFunc2(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.VariableParamFunc(0, "a")
	ASSERT_EQ(ret, 0)
end

function CMyTestCaseLuaCallCS.CaseVariableParamFunc3(self)
    self.count = 1 + self.count
	--if (CS.LuaTestCommon.IsMacPlatform() == false) then
    if (true) then
		local ret, error = pcall(function() CS.LuaTestObj.VariableParamFunc(0, "a", 1, "b") end)
		ASSERT_EQ(ret, true)
    else
        ASSERT_EQ(true, false)
	end
end

function CMyTestCaseLuaCallCS.CaseLuaAccessEnum1(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.TestEnumFunc(CS.LuaTestType.DEF)
	ASSERT_EQ(ret, 1)
end

--[[ 2016.11.25 新版本不支持整数直接当枚举用
function CMyTestCaseLuaCallCS.CaseLuaAccessEnum2(self)
    self.count = 1 + self.count
	local enumValue = -1
	local ret = CS.LuaTestObj.TestEnumFunc(enumValue)
	ASSERT_EQ(ret, -1)
end
]]

function CMyTestCaseLuaCallCS.CaseLuaAccessEnum3(self)
    self.count = 1 + self.count
	local enumValue = CS.LuaTestType.__CastFrom(2)
	local ret = CS.LuaTestObj.TestEnumFunc(enumValue)
	ASSERT_EQ(ret, 2)
end

function CMyTestCaseLuaCallCS.CaseLuaAccessEnum4(self)
    self.count = 1 + self.count
	local enumValue = CS.LuaTestType.__CastFrom(4)
	local ret = CS.LuaTestObj.TestEnumFunc(enumValue)
	ASSERT_EQ(ret, 4)
end

function CMyTestCaseLuaCallCS.CaseLuaAccessEnum5(self)
    self.count = 1 + self.count
	local enumValue = CS.LuaTestType.__CastFrom("GHI")
	local ret = CS.LuaTestObj.TestEnumFunc(enumValue)
	ASSERT_EQ(ret, 2)
end

function CMyTestCaseLuaCallCS.CaseLuaAccessEnum6(self)
    self.count = 1 + self.count
	--if (CS.LuaTestCommon.IsMacPlatform() == false and CS.LuaTestCommon.IsIOSPlatform() == false) then
    if (true) then
		local ret, error = pcall(function() CS.LuaTestObj.TestEnumFunc(CS.LuaTestType.__CastFrom("BCD")) end)
		ASSERT_EQ(ret, false)
    else
        ASSERT_EQ(true, false)
	end
end

function CMyTestCaseLuaCallCS.CaseGetType1(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.TestGetType(typeof(CS.LuaTestObj))
	ASSERT_EQ(ret, "LuaTestObj")
end

function CMyTestCaseLuaCallCS.CaseGetType2(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.TestGetType(typeof(CS.System.String))
	ASSERT_EQ(ret, "System.String")
end

function CMyTestCaseLuaCallCS.CaseGetType3(self)
    self.count = 1 + self.count
    if CS.LuaTestCommon.IsXLuaGeneral() then return end
	local ret = CS.LuaTestObj.TestGetType(typeof(CS.UnityEngine.Vector3))
	ASSERT_EQ(ret, "UnityEngine.Vector3")
end

function CMyTestCaseLuaCallCS.CaseGetType4(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.TestGetType(typeof(CS.System.Int16))
	ASSERT_EQ(ret, "System.Int16")
end

function CMyTestCaseLuaCallCS.CaseGetType5(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.TestGetType(typeof(CS.XLua.LuaTable))
	ASSERT_EQ(ret, "XLua.LuaTable")
end

function CMyTestCaseLuaCallCS.CaseGetType6(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.TestGetType(typeof(CS.XLua.LuaFunction))
	ASSERT_EQ(ret, "XLua.LuaFunction")
end

--[[ v2.1.0版本中LuaUserData类已去掉
function CMyTestCaseLuaCallCS.CaseGetType7(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.TestGetType(typeof(CS.XLua.LuaUserData))
	ASSERT_EQ(ret, "XLua.LuaUserData")
end
]]

function CMyTestCaseLuaCallCS.Case64BitInt1(self)
    self.count = 1 + self.count
	CS.LuaTestObj.Gen64BitInt()
	local x1 = CS.LuaTestObj.ulX1
	local x2 = CS.LuaTestObj.ulX2
	local ret = x1 + x2
	ASSERT_EQ("6917529027641081856", tostring(ret))
	ret  = x2 - x1
	--ASSERT_EQ("16140901064495857664", tostring(ret))
	ASSERT_EQ("-2305843009213693952", tostring(ret))
	x1 = CS.LuaTestObj.lY1
	x2 = CS.LuaTestObj.lY2
	ret = x1 + x2
	ASSERT_EQ("6917529027641081856", tostring(ret))
	ret = x2 - x1
	ASSERT_EQ("-2305843009213693952", tostring(ret))
	x1 = CS.LuaTestObj.i64Z1
	x2 = CS.LuaTestObj.i64Z2
	ret = x1 + x2
	ASSERT_EQ("6917529027641081856", tostring(ret))
	ret = x2 - x1
	ASSERT_EQ("-2305843009213693952", tostring(ret))
end

function CMyTestCaseLuaCallCS.Case64BitInt2(self)
    self.count = 1 + self.count
	CS.LuaTestObj.Gen64BitInt()
	local x1 = CS.LuaTestObj.ulX1
	local x2 = CS.LuaTestObj.ulX2
	ASSERT_EQ(true, x2 < x1)
	ASSERT_EQ(true, x2 <= x1)
	ASSERT_EQ(false, x1 <= x2)
	ASSERT_EQ(false, x1 < x2)
end

function CMyTestCaseLuaCallCS.Case64BitInt3(self)
    self.count = 1 + self.count
	CS.LuaTestObj.Gen64BitInt()
	local y = CS.LuaTestObj.lY3
	y = y * 100
	ASSERT_EQ(tostring(y), "112589990684262400")
	y = y / 10000
	if islua53() then
	    ASSERT_EQ(tostring(y), "11258999068426.0")
	else
		ASSERT_EQ(tostring(y), "11258999068426")
	end
	y = y % 1000
	if islua53() then
	    ASSERT_EQ(tostring(y), "426.240234375")
	else
		ASSERT_EQ(tostring(y), "426")
	end
end

function CMyTestCaseLuaCallCS.Case64BitInt4(self)
    self.count = 1 + self.count
	CS.LuaTestObj.Gen64BitInt()
	local ret = CS.LuaTestObj.lY3 * CS.LuaTestObj.lY4
	ASSERT_EQ(tostring(ret), "138485688541642752")
	ret = CS.LuaTestObj.lY3 / CS.LuaTestObj.lY5
	if islua53() then
		ASSERT_EQ(tostring(ret), "91202908614.226")
	else
	    ASSERT_EQ(tostring(ret), "91202908614")
	end
	ret = CS.LuaTestObj.lY3 % CS.LuaTestObj.lY6
	ASSERT_EQ(tostring(ret), "52636")
end

function CMyTestCaseLuaCallCS.CaseCast1(self)
    self.count = 1 + self.count
	local castObj = CS.TestCastClass()
	cast(castObj, typeof(CS.TestCastClass))
	ASSERT_EQ(true, castObj:TestFunc1())
end

function CMyTestCaseLuaCallCS.CaseCast2(self)
	self.count = 1 + self.count
	local castObj = CS.LuaTestObj.CreateTestLuaObj()
	cast(castObj, typeof(CS.TestLuaClass))
	ASSERT_EQ(true, castObj:TestFunc1())
end

function LuaFunc1(x)
	return x + 1
end

function LuaFunc2(x)
	return x * 2
end	

function CMyTestCaseLuaCallCS.CaseDelgate1(self)
	self.count = 1 + self.count
	
	CS.LuaTestObj.initNumber = 5
	CS.LuaTestObj.GenDelegate()
	local luaDelgateLink = CS.LuaTestObj.csDelegate
	luaDelgateLink = CS.LuaTestObj.csDelegate1
	luaDelgateLink = luaDelgateLink + CS.LuaTestObj.csDelegate2
	luaDelgateLink = luaDelgateLink + CS.LuaTestObj.csDelegate3
	local ret = luaDelgateLink(1)
	ASSERT_EQ(5, ret)
	luaDelgateLink = luaDelgateLink + LuaFunc1
	ret = luaDelgateLink(1)
	ASSERT_EQ(2, ret)
	CS.LuaTestObj.csDelegate4 = LuaFunc2
	luaDelgateLink = luaDelgateLink + CS.LuaTestObj.csDelegate4
	ret = luaDelgateLink(2)
	ASSERT_EQ(4, ret)
	luaDelgateLink = luaDelgateLink - LuaFunc1
	ret = luaDelgateLink(10)
	ASSERT_EQ(20 ,ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate4
	ret = luaDelgateLink(19)
	ASSERT_EQ(1900, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate3
	ret = luaDelgateLink(2)
	ASSERT_EQ(1900, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate2
	ret = luaDelgateLink(3)
	ASSERT_EQ(1903, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate2
	ret = luaDelgateLink(4)
	ASSERT_EQ(1907, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate1
    local ret, error = pcall(function() luaDelgateLink(5) end)
    ASSERT_EQ(false, ret)

	CS.LuaTestObj.initNumber = 5
	luaDelgateLink = CS.LuaTestObj.csDelegate3
	ret = luaDelgateLink(1)
	ASSERT_EQ(5, ret)
	luaDelgateLink = luaDelgateLink + CS.LuaTestObj.csDelegate1
	ret = luaDelgateLink(1)
	ASSERT_EQ(6, ret)
	luaDelgateLink = luaDelgateLink + CS.LuaTestObj.csDelegate1
	ret = luaDelgateLink(1)
	ASSERT_EQ(8, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate1
	ret = luaDelgateLink(1)
	ASSERT_EQ(9, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate1
	ret = luaDelgateLink(1)
	ASSERT_EQ(9, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate3
	CS.LuaTestObj.initNumber = 1
	local function LuaFunc4(x)
		CS.LuaTestObj.initNumber = CS.LuaTestObj.initNumber * 2
		return CS.LuaTestObj.initNumber
	end
	luaDelgateLink = CS.LuaTestObj.csDelegate1
	luaDelgateLink = luaDelgateLink + LuaFunc4
	ret = luaDelgateLink(1)
	ASSERT_EQ(ret, 4)
	luaDelgateLink = luaDelgateLink + LuaFunc4
	ret = luaDelgateLink(1)
	ASSERT_EQ(ret, 20)
	luaDelgateLink = luaDelgateLink - LuaFunc4
	ret = luaDelgateLink(1)
	ASSERT_EQ(ret, 42)
	luaDelgateLink = luaDelgateLink - LuaFunc4
	ret = luaDelgateLink(1)
	ASSERT_EQ(ret, 43)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate1
	
	CS.LuaTestObj.initNumber = 1907
	luaDelgateLink = LuaFunc1
	ret = luaDelgateLink(1)
	ASSERT_EQ(2, ret)	
	luaDelgateLink = luaDelgateLink + CS.LuaTestObj.csDelegate1
	ret = luaDelgateLink(2)
	ASSERT_EQ(1909, ret)
	luaDelgateLink = luaDelgateLink + CS.LuaTestObj.csDelegate4
	ret = luaDelgateLink(3)
	ASSERT_EQ(6, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate1
	ret = luaDelgateLink(4)
	ASSERT_EQ(8, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate1
	ret = luaDelgateLink(5)
	ASSERT_EQ(10, ret)
	luaDelgateLink = luaDelgateLink - LuaFunc1
	ret = luaDelgateLink(6)
	ASSERT_EQ(12, ret)
	luaDelgateLink = luaDelgateLink - LuaFunc1
	ret = luaDelgateLink(7)
	ASSERT_EQ(14, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObj.csDelegate4
    local ret, error = pcall(function() luaDelgateLink(8) end)
    ASSERT_EQ(false, ret)
end

function EvtFunc11(y)
	gTestNumber = y + gTestNumber
	return gTestNumber
end

function EvtFunc12(y)
	gTestNumber = y + 1 + gTestNumber
	return gTestNumber
end

function CMyTestCaseLuaCallCS.CaseEvent1(self)
	self.count = 1 + self.count

	gTestNumber = 1
	local testObj = CS.LuaTestObj()
	testObj:TestEvent1('+', EvtFunc11)
	local ret = testObj:CallEvent(1)
	ASSERT_EQ(2, ret)
	testObj:TestEvent1('+', EvtFunc12)
	local ret = testObj:CallEvent(2)
	ASSERT_EQ(7, ret)
	testObj:TestEvent1('+', EvtFunc12)
	local ret = testObj:CallEvent(1)
	ASSERT_EQ(12, ret)
	testObj:TestEvent1('-', EvtFunc12)
	local ret = testObj:CallEvent(1)
	ASSERT_EQ(15, ret)
	testObj:TestEvent1('-', EvtFunc12)
	local ret = testObj:CallEvent(1)
	ASSERT_EQ(16, ret)
	testObj:TestEvent1('-', EvtFunc12)
	local ret = testObj:CallEvent(1)
	ASSERT_EQ(17, ret)
	testObj:TestEvent1('-', EvtFunc11)
end

function CMyTestCaseLuaCallCS.CaseCalc1(self)
	self.count = 1 + self.count
	local a = CS.LuaTestObj()
	a.testVar = 100
	local b = CS.LuaTestObj()
	b.testVar = 200
	local ret = a + b
	ASSERT_EQ(ret.testVar, 300)
	ret = a - b
	ASSERT_EQ(ret.testVar, -100)
	ret = a * b
	ASSERT_EQ(ret.testVar, 20000)
	ret = b / a
	ASSERT_EQ(ret.testVar, 2)
	ret = b % a
	ASSERT_EQ(ret.testVar, 0)
	ret = a < b
	ASSERT_EQ(true, ret)
	ret = a <= b
	ASSERT_EQ(true, ret)
	ret = -a
	ASSERT_EQ(ret.testVar, -100)
	local c = CS.LuaTestObj()
	c.testArr[0] = 1000
	ret = c[0]
	ASSERT_EQ(1000, ret)
	c[1] = 10000
	ret = c.testArr[1]
	ASSERT_EQ(10000, ret)
	
	a.testVar = 100
	b.testVar = 200
	c.testVar = 300
	local d = CS.LuaTestObj()
	d.testVar = 20
	local e = CS.LuaTestObj()
	e.testVar = 7
	ret = (a + b) * c / d % e
	ASSERT_EQ(ret.testVar, 6)
end

function CMyTestCaseLuaCallCS.CaseCalc2(self)
	self.count = 1 + self.count
	local a = CS.LuaTestObj()
	a.testVar = 100
	local ret = 300 + a + 200
	ASSERT_EQ(ret.testVar, 600)
	ret = 100 - a - 400
	ASSERT_EQ(ret.testVar, -400)
	ret = 2 * a * 3
	ASSERT_EQ(600, ret.testVar)
	ret = 20000 / a / 2
	ASSERT_EQ(100, ret.testVar)

	
end

function CMyTestCaseLuaCallCS.CaseOverLoad1(self)
	self.count = 1 + self.count
	local ret = CS.LuaTestObj.OverLoad1(1, 2)
	ASSERT_EQ(ret, 1)
	ret = CS.LuaTestObj.OverLoad2("1", "2")
	ASSERT_EQ(ret, 4);
	ret = CS.LuaTestObj.OverLoad3(1)
	ASSERT_EQ(ret, 5);
end

function CMyTestCaseLuaCallCS.CaseOutRef1(self)
	self.count = 1 + self.count
	local a = 2
	local b
	a, b = CS.LuaTestObj.OutRefFunc1(1, a, b)
	ASSERT_EQ(a, 100)
	b, a = CS.LuaTestObj.OutRefFunc2(b, 1, a)
	ASSERT_EQ(a, 200)
	a, b = CS.LuaTestObj.OutRefFunc3(1, a, b)
	ASSERT_EQ(a, 300)
	ret, a, b = CS.LuaTestObj.OutRefFunc4(1, a, b)
	ASSERT_EQ(a, 400)
	ASSERT_EQ(ret, 400)
	ret, b, a = CS.LuaTestObj.OutRefFunc5(b, 1, a)
	ASSERT_EQ(a, 500)
	ASSERT_EQ(ret, 500)
	ret = CS.LuaTestObj.OutRefFunc6(1, 2)
	ASSERT_EQ(ret, 600)
end

function CMyTestCaseLuaCallCS.CaseOutRef2(self)
	self.count = 1 + self.count
	local a = CS.LuaTestObj.CreateTestLuaObj()
	local b = CS.LuaTestObj.CreateTestLuaObj()
	local c = CS.LuaTestObj.CreateTestLuaObj()
	a, b = CS.LuaTestObj.OutRefFunc11(c, a, b)
	ASSERT_EQ(a.cmpTarget, 100)
	b, a = CS.LuaTestObj.OutRefFunc12(b, c, a)
	ASSERT_EQ(a.cmpTarget, 200)
	a, b = CS.LuaTestObj.OutRefFunc13(c, a, b)
	ASSERT_EQ(a.cmpTarget, 300)
	ret, a, b = CS.LuaTestObj.OutRefFunc14(c, a, b)
	ASSERT_EQ(a.cmpTarget, 400)
	ASSERT_EQ(ret, 400)
	ret, b, a = CS.LuaTestObj.OutRefFunc15(b, c, a)
	ASSERT_EQ(a.cmpTarget, 500)
	ASSERT_EQ(ret, 500)
	ret = CS.LuaTestObj.OutRefFunc16(a, b)
	ASSERT_EQ(ret, 600)
end

function CMyTestCaseLuaCallCS.CaseOutRef3(self)
	self.count = 1 + self.count
	local a = CS.LuaTestObj.csDelegate11
	local b = CS.LuaTestObj.csDelegate12
	local c = CS.LuaTestObj.csDelegate13
	CS.LuaTestObj.initNumber = 1
	
	a, b = CS.LuaTestObj.OutRefFunc21(c, a, b)
	ASSERT_EQ(a(1), 2)
	b, a = CS.LuaTestObj.OutRefFunc22(b, c, a)
	ASSERT_EQ(a(1), 3)
	a, b = CS.LuaTestObj.OutRefFunc23(c, a, b)
	ASSERT_EQ(a(1), 4)
	ret, a, b = CS.LuaTestObj.OutRefFunc24(c, a, b)
	ASSERT_EQ(a(1), 6)
	ASSERT_EQ(ret, 5)
	ret, b, a = CS.LuaTestObj.OutRefFunc25(b, c, a)
	ASSERT_EQ(a(1), 8)
	ASSERT_EQ(ret, 7)
	ret = CS.LuaTestObj.OutRefFunc26(a, b)
	ASSERT_EQ(ret, 600)
end

function CMyTestCaseLuaCallCS.CaseNewClass1And5(self)
	self.count = 1 + self.count
	local class = CS.NoContClass()
	local ret = class:add(1,2)
	ASSERT_EQ(ret, 3)	
end

function CMyTestCaseLuaCallCS.CaseNewClass2(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.OneParamContClass(2)
	local ret = class:add(1,2)
	ASSERT_EQ(ret, 3)	
	ASSERT_EQ(class.key1, 2)
	ASSERT_EQ(class.key2, 1)
end

function CMyTestCaseLuaCallCS.CaseNewClass3(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.TwoParamsContClass(2, 3)
	local ret = class:add(1,2)
	ASSERT_EQ(ret, 3)	
	ASSERT_EQ(class.key1, 2)
	ASSERT_EQ(class.key2, 3)
end

function CMyTestCaseLuaCallCS.CaseNewClass4(self)
	self.count = 1 + self.count
	local class1 = CS.testLuaCallCS.MultiContClass(2)
	local class2 = CS.testLuaCallCS.MultiContClass()
	local ret = class1:add(1,2)
	ASSERT_EQ(ret, 3)	
	ASSERT_EQ(class1.key1, 2)
	ASSERT_EQ(class1.key2, 1)
	ret = class2:add(2,2)
	ASSERT_EQ(ret, 4)	
	ASSERT_EQ(class2.key1, 1)
	ASSERT_EQ(class2.key2, 1)
end

function CMyTestCaseLuaCallCS.CaseNewClass6(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.OverClassA()
	local ret = class:add(1,2)
	ASSERT_EQ(ret, 3)
	ret = class:sub(9, 1)
	ASSERT_EQ(ret, 8)
	ASSERT_EQ(class.key1, 1)
	ASSERT_EQ(class.key2, 1)
	ASSERT_EQ(class.key3, 0)
end

function CMyTestCaseLuaCallCS.CaseNewClass7(self)
	self.count = 1 + self.count
	CS.testLuaCallCS.StaticTestClass.n = 0
	CS.testLuaCallCS.StaticTestClass:Add()
	ASSERT_EQ(CS.testLuaCallCS.StaticTestClass.n, 1)
end

function CMyTestCaseLuaCallCS.CaseVisitStaticMemFunc_1(self)
	self.count = 1 + self.count
	CS.testLuaCallCS.MultiContClass.d = 3
	local ret = CS.testLuaCallCS.MultiContClass.d
	ASSERT_EQ(ret, 3)
	ret = CS.testLuaCallCS.MultiContClass.dec(10, 1)
	ASSERT_EQ(ret, 9)
end

function CMyTestCaseLuaCallCS.CaseVisitStaticMemFunc_2(self)
	self.count = 1 + self.count
	CS.NoContClass.d = 4
	local ret = CS.NoContClass.d
	ASSERT_EQ(ret, 4)
	ret = CS.NoContClass.dec(10, 1)
	ASSERT_EQ(ret, 9)
end

function CMyTestCaseLuaCallCS.CaseVisitStaticMemFunc_3(self)
	self.count = 1 + self.count
	CS.testLuaCallCS.StaticTestClass.n = 2
	CS.testLuaCallCS.StaticTestClass:Add()
	ASSERT_EQ(CS.testLuaCallCS.StaticTestClass.n, 3)
end

function CMyTestCaseLuaCallCS.CaseVisitClassMemFunc_1(self)
	self.count = 1 + self.count
	local class = CS.NoContClass()
	local ret = class:add(1,2)
	ASSERT_EQ(ret, 3)
	class.key3 = false;
	ASSERT_EQ(class.key3, false)
end

function CMyTestCaseLuaCallCS.CaseVisitClassMemFunc_2(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.OneParamContClass(2)
	local ret = class:add(1,2)
	ASSERT_EQ(ret, 3)	
	class.key1 = 3
	class.key2 = 4
	ASSERT_EQ(class.key1, 3)
	ASSERT_EQ(class.key2, 4)
end

function CMyTestCaseLuaCallCS.CaseVisitFatherClassMemFunc_1_1(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.OverClassC()
	CS.testLuaCallCS.OverClassC.bValue = true
	ASSERT_EQ(CS.testLuaCallCS.OverClassC.bValue, true)	
	CS.testLuaCallCS.OverClassC.Set(false)
	ASSERT_EQ(CS.testLuaCallCS.OverClassC.bValue, false)
	
	CS.testLuaCallCS.OverClassC.d = 3
	ASSERT_EQ(CS.testLuaCallCS.OverClassC.d, 3)	
	local ret = CS.testLuaCallCS.OverClassC.dec(10, 1)
	ASSERT_EQ(ret, 9)
	class.key4 = 5
	class.key1 = 1
	class.key3 = false
	ret = class:sub(10, 1)
	ASSERT_EQ(ret, 10)
	ASSERT_EQ(class.key4 , 5)
	ASSERT_EQ(class.key1 , 1)
	ASSERT_EQ(class.key3 , false)
	ASSERT_EQ(class:add(1,2), 3)
	--ASSERT_EQ(class:sum(1, 2, 3), 6)
end

function CMyTestCaseLuaCallCS.CaseVisitFatherClassMemFunc_1_2(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.OverClassB()
	
	CS.testLuaCallCS.OverClassB.d = 3
	ASSERT_EQ(CS.testLuaCallCS.OverClassB.d, 3)	
	local ret = CS.testLuaCallCS.OverClassB.dec(10, 1)
	ASSERT_EQ(ret, 9)
	class.key1 = 2
	class.key2 = 3
	ret = class:add(10, 1)
	ASSERT_EQ(ret, 11)
	ASSERT_EQ(class.key1, 2)
	ASSERT_EQ(class.key2, 3)
	ret = class:sub(10, 1)
	ASSERT_EQ(ret, 9)
end

function CMyTestCaseLuaCallCS.CaseVisitFatherClassMemFunc_2(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.OverClassCDeriveNGA()
	--[[CS.testLuaCallCS.OverClassCDeriveNGA.bValue = true
	ASSERT_EQ(CS.testLuaCallCS.OverClassCDeriveNGA.bValue, true)	
	CS.testLuaCallCS.OverClassCDeriveNGA.Set(false)
	ASSERT_EQ(CS.testLuaCallCS.OverClassCDeriveNGA.bValue, false)]]
	
	CS.testLuaCallCS.OverClassCDeriveNGA.d = 3
	ASSERT_EQ(CS.testLuaCallCS.OverClassCDeriveNGA.d, 3)	
	local ret = CS.testLuaCallCS.OverClassCDeriveNGA.dec(10, 1)
	ASSERT_EQ(ret, 9)
	--class.key4 = 5
	class.key1 = 1
	class.key3 = false
	--ret = class:sub(10, 1)
	--ASSERT_EQ(ret, 9)
	--ASSERT_EQ(class.key4 , 5)
	ASSERT_EQ(class.key1 , 1)
	ASSERT_EQ(class.key3 , false)
	ASSERT_EQ(class:add(1,2), 3)
end

function CMyTestCaseLuaCallCS.CaseVisitFatherClassMemFunc_3(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.ChildCalss()

	local ret = class:add(10, 1)
	ASSERT_EQ(ret, 11)
	
	class.a = 3
	ASSERT_EQ(class:getA() , 3)
	class:setA(4)
	ASSERT_EQ(class:getA() , 4)
	
end

function CMyTestCaseLuaCallCS.CaseGetChineseString(self)
	self.count = 1 + self.count
	local class = CS.TestChineseString()

	local ret = class:GetShortChinString()
	ASSERT_EQ(ret, "中文字符串")
	
	ASSERT_EQ(class:GetLongChineString() , "为Unity3D增加Lua脚本编程的能力，进而提供代码逻辑增量更新的可能，支持lua的所有基本类型，哈哈哈哈")
	ASSERT_EQ(class:GetCombineString() , "中文字符串.* ? [ ] ^ $~`!@#$%^&()_-+=[];',““〈〉〖【℃ ＄ ¤№ ☆ ★■ⅷ②㈣12345abc")
	ASSERT_EQ(class:GetComplexString() , "繁體國陸")
	ASSERT_EQ(class:GetHuoxingString() , "吙煋呅僦媞這樣孒")
	
end

function CMyTestCaseLuaCallCS.CaseVisitUlong(self)
	self.count = 1 + self.count
	local class = CS.TestUlongAndLongType()

	local ulong_max = class.UlongMax
	--ASSERT_EQ(tostring(ulong_max), "18446744073709551615")
	ASSERT_EQ(tostring(ulong_max), "-1") --V2.1.1
	--ASSERT_EQ(tostring(class:UlongAdd()), "18446744073709550616")
	ASSERT_EQ(tostring(class:UlongAdd()), "-1000") --V2.1.1
	local ulong_min = class.UlongMin
	ASSERT_EQ(tostring(ulong_min), "0")
	
	local ulong_mid = class.UlongMid
	--ASSERT_EQ(tostring(ulong_mid), "9223372036854775808")
	ASSERT_EQ(tostring(ulong_mid), "-9223372036854775808") --v2.1.1
	class.UlongMax = 1844674407370955
	ulong_max = class.UlongMax
	ASSERT_EQ(tostring(ulong_max), "1844674407370955")
	
	class.UlongMin = 100
	ulong_min = class.UlongMin
	ASSERT_EQ(tostring(ulong_min), "100")
	
	local ulong_add = ulong_min + ulong_mid
	--ASSERT_EQ(tostring(ulong_add), "9223372036854775908")
	ASSERT_EQ(tostring(ulong_add), "-9223372036854775708") --V2.1.1
end

function CMyTestCaseLuaCallCS.CaseVisitlong(self)
	self.count = 1 + self.count
	local class = CS.TestUlongAndLongType()

	local long_max = class.LongMax
	ASSERT_EQ(tostring(long_max), "9223372036854775807")
	
	ASSERT_EQ(tostring(class:LongAdd()), "9223372036854774808")
	
	local long_min = class.LongMin
	ASSERT_EQ(tostring(long_min), "-9223372036854775808")
	
	local long_mid = class.LongMid
	ASSERT_EQ(tostring(long_mid), "4611686018427387904")
	
	class.LongMax = 461168601842738
	long_max = class.LongMax
	ASSERT_EQ(tostring(long_max), "461168601842738")
	
	class.LongMin = 100
	long_min = class.LongMin
	ASSERT_EQ(tostring(long_min), "100")
	
	local long_add = long_min + long_mid
	ASSERT_EQ(tostring(long_add), "4611686018427388004")
	
	class.UlongMin = 100
	ulong_min = class.UlongMin
	ASSERT_EQ(tostring(ulong_min), "100")
	
	--local ret,errMessage = pcall(ulong_min + long_min)
	--ASSERT_EQ(errMessage, "type not match, lhs is UInt64, rhs is Int64")
end

function CMyTestCaseLuaCallCS.CaseVisitExtensionMethodForClass(self)
	self.count = 1 + self.count
    local class = CS.TestChineseString()
    class:PrintAllString()
	
	local length = class:GetLongStringLength()
	ASSERT_EQ(54, length)
	
	length = class:Add(class)
	ASSERT_EQ(108, length)
	local class_a = CS.TestChineseString()
	class_a.short_simple_string = "啊"
	ASSERT_EQ(class_a:GetShortChinString() , "啊")
	
	local class_d = CS.TestChineseString()
	class_d.short_simple_string = "ha"
	local ret1
	local ret2
	ret1, ret2 = class:Replace(class_d, class_a)
	ASSERT_EQ(ret1:GetShortChinString(), "中文字符串")
	ASSERT_EQ(ret2:GetCombineString(), "中文字符串")
	ASSERT_EQ(ret2:GetShortChinString(), "中文字符串ha")
end

function CMyTestCaseLuaCallCS.CaseVisitExtensionMethodForStruct(self)
	self.count = 1 + self.count
    local employ_1 = CS.Employeestruct()
	employ_1.Name = "HONGFANG"
	employ_1.Age = 30
	employ_1.Salary = 12000
	employ_1.AnnualBonus = 30000
    employ_1:PrintSalary()
	
	local income = employ_1:GetIncomeForOneYear()
	ASSERT_EQ(income, 174000)
	
	local employ_2 = CS.Employeestruct()
	employ_2.Name = "xiaojun"
	employ_2.Age =25
	employ_2.Salary = 5000
	employ_2.AnnualBonus = 10000
    employ_2:PrintSalary()
	
	local cost = employ_1:Add(employ_2)
	ASSERT_EQ(cost, 244000)	
	
	local employ_3 = CS.Employeestruct()
	employ_3.Name = "xiaojuan"
	employ_3.Age =25
	employ_3.Salary = 5000
	employ_3.AnnualBonus = 10000
    employ_3:PrintSalary()
	
	local ret1
	local ret2
	ret1, ret2 = employ_1:Sub(employ_2, employ_3)
	ASSERT_EQ(ret1.Name, "xiaojun")
	ASSERT_EQ(ret1.Salary, 10000)
	ASSERT_EQ(ret2.Name, "xiaojuan")
	ASSERT_EQ(ret2.Salary, 7000)
end

function CMyTestCaseLuaCallCS.CaseVisitNoGenCodeStruct_1(self)
	self.count = 1 + self.count
    local struct_1 = CS.NoGenCodeStruct()
	
	local ret = struct_1:Add(2, 3)
	struct_1.Byte = 10;
	struct_1.Char = 97;
	struct_1.Decimal = 12.33333333;
	struct_1.Double = 10.111111111111;
	struct_1.Float = 11.120;
	struct_1.IntVar1 = 12345678;
	struct_1.IntVar2 = -12345678;
	struct_1.Long = CS.LongStatic.LONG_MAX;
	struct_1.ULong = CS.LongStatic.ULONG_MAX;
	struct_1.Short = 123;
	struct_1.String = "just for test";
	struct_1.UInt = 12345;
	struct_1.UShort = 255;
	struct_1.IncludeStruct = CS.ConStruct (1, 2, "haha");
	ASSERT_EQ(ret, 5)
	ASSERT_EQ(struct_1.Byte, 10)
	ASSERT_EQ(tostring(struct_1.Char), "97")
	ASSERT_EQ(string.sub(tostring(struct_1.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(struct_1.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(struct_1.Float), 1, 6), "11.119") --精度损失
	ASSERT_EQ(struct_1.IntVar1, 12345678)
	ASSERT_EQ(struct_1.IntVar2, -12345678)
	ASSERT_EQ(tostring(struct_1.Long), "9223372036854775807")
	ASSERT_EQ(tostring(struct_1.ULong), "-1")
	ASSERT_EQ(struct_1.Short, 123)
	ASSERT_EQ(struct_1.String, "just for test")
	ASSERT_EQ(struct_1.UInt, 12345)
	ASSERT_EQ(struct_1.UShort, 255)	
	ASSERT_EQ(struct_1.IncludeStruct.x, 1)
	ASSERT_EQ(struct_1.IncludeStruct.y, 2)
	ASSERT_EQ(struct_1.IncludeStruct.z, "haha")
	
	
	local struct_2 = CS.ConStruct (2, 2, "haha");
	ASSERT_EQ(struct_2.x, 2)
	ASSERT_EQ(struct_2.y, 2)
	ASSERT_EQ(struct_2.z, "haha")
	
	struct_1.String = nil;
	ASSERT_EQ(struct_1.String, nil)
end

function CMyTestCaseLuaCallCS.CaseVisitNoGenCodeStruct_2(self)
    --2.struct作为类静态/成员属性
	self.count = 1 + self.count
    CS.NoGenCodeBaseClass.struct_var2 = CS.ConStruct(33333333, 44444444, "test") --分开赋值会失败
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var2.x, 33333333)
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var2.y, 44444444)
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var2.z, "test")
	
	
	--[[值传递所以修改副本，不会改动原本
	local ret = CS.NoGenCodeBaseClass.struct_var1:Add(2, 3)
	CS.NoGenCodeBaseClass.struct_var1.Byte = 10;
	CS.NoGenCodeBaseClass.struct_var1.Char = 97;
	CS.NoGenCodeBaseClass.struct_var1.Decimal = 12.33333333;
	CS.NoGenCodeBaseClass.struct_var1.Double = 10.111111111111;
	CS.NoGenCodeBaseClass.struct_var1.Float = 11.1234;
	CS.NoGenCodeBaseClass.struct_var1.IntVar1 = 12345678;
	CS.NoGenCodeBaseClass.struct_var1.IntVar2 = -12345678;
	CS.NoGenCodeBaseClass.struct_var1.Long = CS.LongStatic.LONG_MAX;
	CS.NoGenCodeBaseClass.struct_var1.ULong = CS.LongStatic.ULONG_MAX;
	CS.NoGenCodeBaseClass.struct_var1.Short = 123;
	CS.NoGenCodeBaseClass.struct_var1.String = "just for test";
	CS.NoGenCodeBaseClass.struct_var1.UInt = 12345;
	CS.NoGenCodeBaseClass.struct_var1.UShort = 255;
	CS.NoGenCodeBaseClass.struct_var1.IncludeStruct = CS.ConStruct (1, 2, "haha");
	ASSERT_EQ(ret, 5)
	
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var1.Byte, 10)
	ASSERT_EQ(tostring(CS.NoGenCodeBaseClass.struct_var1.Char), "97")
	ASSERT_EQ(string.sub(tostring(CS.NoGenCodeBaseClass.struct_var1.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var1.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(CS.NoGenCodeBaseClass.struct_var1.Float), 1, 7), "11.1234")
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var1.IntVar1, 12345678)
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var1.IntVar2, -12345678)
	ASSERT_EQ(tostring(CS.NoGenCodeBaseClass.struct_var1.Long), "9223372036854775807")
	ASSERT_EQ(tostring(CS.NoGenCodeBaseClass.struct_var1.ULong), "18446744073709551615")
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var1.Short, 123)
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var1.String, "just for test")
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var1.UInt, 12345)
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var1.UShort, 255)
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var1.IncludeStruct.x, 1)
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var1.IncludeStruct.y, 2)
	ASSERT_EQ(CS.NoGenCodeBaseClass.struct_var1.IncludeStruct.z, "haha")
	
	local struct_2 = CS.NoGenCodeBaseClass.GetStaticVar();
	ASSERT_EQ(struct_2.Byte, 10)
	ASSERT_EQ(tostring(struct_2.Char), "97")
	ASSERT_EQ(string.sub(tostring(struct_2.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(struct_2.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(struct_2.Float), 1, 7), "11.1234")
	ASSERT_EQ(struct_2.IntVar1, 12345678)
	ASSERT_EQ(struct_2.IntVar2, -12345678)
	ASSERT_EQ(tostring(struct_2.Long), "9223372036854775807")
	ASSERT_EQ(tostring(struct_2.ULong), "18446744073709551615")
	ASSERT_EQ(struct_2.Short, 123)
	ASSERT_EQ(struct_2.String, "just for test")
	ASSERT_EQ(struct_2.UInt, 12345)
	ASSERT_EQ(struct_2.UShort, 255)
	ASSERT_EQ(struct_2.IncludeStruct.x, 1)
	ASSERT_EQ(struct_2.IncludeStruct.y, 2)
	ASSERT_EQ(struct_2.IncludeStruct.z, "haha")
	]]
end

function CMyTestCaseLuaCallCS.CaseVisitNoGenCodeStruct_3_5_9(self)
    --3.struct作为类静态/成员方法输入
	--5.struct作为类静态/成员方法输入输出，ref修饰
	--9.struct作为重载方法的输入（in）,输出（out），输入输出（ref）
	self.count = 1 + self.count
    	
	local struct_1 = CS.NoGenCodeStruct()
	
	struct_1.Byte = 10;
	struct_1.Char = 97;
	struct_1.Decimal = 12.33333333;
	struct_1.Double = 10.111111111111;
	struct_1.Float = 11.1234;
	struct_1.IntVar1 = 12345678;
	struct_1.IntVar2 = -12345678;
	struct_1.Long = CS.LongStatic.LONG_MAX;
	struct_1.ULong = CS.LongStatic.ULONG_MAX;
	struct_1.Short = 123;
	struct_1.String = "just for test";
	struct_1.UInt = 12345;
	struct_1.UShort = 256;
	struct_1.IncludeStruct = CS.ConStruct (2, 2, "haha");
	
	local class_1 = CS.NoGenCodeBaseClass();
	local ret1, ret2 = class_1:SetStruct(struct_1)
	ASSERT_EQ(ret1.Byte, 10)
	ASSERT_EQ(tostring(ret1.Char), "97")
	ASSERT_EQ(string.sub(tostring(ret1.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(ret1.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(ret1.Float), 1, 7), "11.1233") --精度损失
	ASSERT_EQ(ret1.IntVar1, 12345678)
	ASSERT_EQ(ret1.IntVar2, -12345678)
	ASSERT_EQ(tostring(ret1.Long), "9223372036854775807")
	--ASSERT_EQ(tostring(ret1.ULong), "18446744073709551615")
	ASSERT_EQ(tostring(ret1.ULong), "-1")
	ASSERT_EQ(ret1.Short, 123)
	ASSERT_EQ(ret1.String, "just for test")
	ASSERT_EQ(ret1.UInt, 12345)
	ASSERT_EQ(ret1.UShort, 256)	
	ASSERT_EQ(ret1.IncludeStruct.x, 2)
	ASSERT_EQ(ret1.IncludeStruct.y, 2)
	ASSERT_EQ(ret1.IncludeStruct.z, "haha")
	
	ASSERT_EQ(ret2.Byte, 10)
	ASSERT_EQ(tostring(ret2.Char), "97")
	ASSERT_EQ(string.sub(tostring(ret2.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(ret2.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(ret2.Float), 1, 7), "11.1233")  --精度损失
	ASSERT_EQ(ret2.IntVar1, 12345678)
	ASSERT_EQ(ret2.IntVar2, -12345678)
	ASSERT_EQ(tostring(ret2.Long), "9223372036854775807")
	--ASSERT_EQ(tostring(ret2.ULong), "18446744073709551615")
	ASSERT_EQ(tostring(ret2.ULong), "-1")
	ASSERT_EQ(ret2.Short, 123)
	ASSERT_EQ(ret2.String, "just for test")
	ASSERT_EQ(ret2.UInt, 12345)
	ASSERT_EQ(ret2.UShort, 256)	
	ASSERT_EQ(ret2.IncludeStruct.x, 2)
	ASSERT_EQ(ret2.IncludeStruct.y, 2)
	ASSERT_EQ(ret2.IncludeStruct.z, "haha")
end

function CMyTestCaseLuaCallCS.CaseVisitNoGenCodeStruct_4(self)
    --4.struct作为类静态/成员方法输出
	self.count = 1 + self.count
	
	local class_1 = CS.NoGenCodeBaseClass();
	local ret1, ret2 = class_1:InitStruct()
	ASSERT_EQ(ret1.Byte, 10)
	ASSERT_EQ(tostring(ret1.Char), "97")
	ASSERT_EQ(string.sub(tostring(ret1.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(ret1.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(ret1.Float), 1, 7), "11.1233") --精度损失
	ASSERT_EQ(ret1.IntVar1, 12345678)
	ASSERT_EQ(ret1.IntVar2, -12345678)
	ASSERT_EQ(tostring(ret1.Long), "9223372036854775807")
	--ASSERT_EQ(tostring(ret1.ULong), "18446744073709551615")
	ASSERT_EQ(tostring(ret1.ULong), "-1")
	ASSERT_EQ(ret1.Short, 123)
	ASSERT_EQ(ret1.String, "just for test")
	ASSERT_EQ(ret1.UInt, 12345)
	ASSERT_EQ(ret1.UShort, 255)
	ASSERT_EQ(ret1.IncludeStruct.x, 1)
	ASSERT_EQ(ret1.IncludeStruct.y, 2)
	ASSERT_EQ(ret1.IncludeStruct.z, "haha")
	ASSERT_EQ(ret2, 0)
end

function CMyTestCaseLuaCallCS.CaseVisitNoGenCodeStruct_6(self)
    --6.struct作为一个类静态/成员方法输出，修改属性后，并作为另一个类静态/成员方法输入
	self.count = 1 + self.count
	
	local class_1 = CS.NoGenCodeBaseClass();
	local ret1, ret2 = class_1:InitStruct()
	ret1.Byte = 100
	ret1.Char = 98
	ret1.Decimal = 22.33333333
	ret1.Double = 20.111111111111
	ret1.Float = 21.1234
	ret1.IntVar1 = 22345678
	ret1.IntVar2 = -22345678
	ret1.Long = 9223372036854
	ret1.ULong = 18446744073709
	ret1.Short = 223
	ret1.String = "lua call c#"
	ret1.UInt = 22345
	ret1.UShort = 155
	ret1.IncludeStruct = CS.ConStruct (2, 2, "hahaTEST");
	
	local ret3, ret4 = class_1:SetStruct(ret1)
	ASSERT_EQ(ret3.Byte, 100)
	ASSERT_EQ(tostring(ret3.Char), "98")
	ASSERT_EQ(string.sub(tostring(ret3.Decimal), 1, 11), "22.33333333")
	ASSERT_EQ(ret3.Double, 20.111111111111)
	ASSERT_EQ(string.sub(tostring(ret3.Float), 1, 7), "21.1233") --精度损失
	ASSERT_EQ(ret3.IntVar1, 22345678)
	ASSERT_EQ(ret3.IntVar2, -22345678)
	ASSERT_EQ(tostring(ret3.Long), "9223372036854")
	ASSERT_EQ(tostring(ret3.ULong), "18446744073709")
	ASSERT_EQ(ret3.Short, 223)
	ASSERT_EQ(ret3.String, "lua call c#")
	ASSERT_EQ(ret3.UInt, 22345)
	ASSERT_EQ(ret3.UShort, 155)	
	ASSERT_EQ(ret3.IncludeStruct.x, 2)
	ASSERT_EQ(ret3.IncludeStruct.y, 2)
	ASSERT_EQ(ret3.IncludeStruct.z, "hahaTEST")
	
	ASSERT_EQ(ret4.Byte, 100)
	ASSERT_EQ(tostring(ret4.Char), "98")
	ASSERT_EQ(string.sub(tostring(ret4.Decimal), 1, 11), "22.33333333")
	ASSERT_EQ(ret4.Double, 20.111111111111)
	ASSERT_EQ(string.sub(tostring(ret4.Float), 1, 7), "21.1233") --精度损失
	ASSERT_EQ(ret4.IntVar1, 22345678)
	ASSERT_EQ(ret4.IntVar2, -22345678)
	ASSERT_EQ(tostring(ret4.Long), "9223372036854")
	ASSERT_EQ(tostring(ret4.ULong), "18446744073709")
	ASSERT_EQ(ret4.Short, 223)
	ASSERT_EQ(ret4.String, "lua call c#")
	ASSERT_EQ(ret4.UInt, 22345)
	ASSERT_EQ(ret4.UShort, 155)	
	ASSERT_EQ(ret4.IncludeStruct.x, 2)
	ASSERT_EQ(ret4.IncludeStruct.y, 2)
	ASSERT_EQ(ret4.IncludeStruct.z, "hahaTEST")
end

function CMyTestCaseLuaCallCS.CaseVisitNoGenCodeStruct_7(self)
    --7.struct作为父类静态/成员属性，lua中通过子类实例访问，包括获取和设置
	self.count = 1 + self.count
	
	local class_1 = CS.NoGenCodeDrivedClass();
	local ret1, ret2 = class_1:InitStruct()
	ret1.Byte = 100
	ret1.Char = 98
	ret1.Decimal = 22.33333333
	ret1.Double = 20.111111111111
	ret1.Float = 21.1234
	ret1.IntVar1 = 22345678
	ret1.IntVar2 = -22345678
	ret1.Long = 9223372036854
	ret1.ULong = 18446744073709
	ret1.Short = 223
	ret1.String = "lua call c#"
	ret1.UInt = 22345
	ret1.UShort = 155
	ret1.IncludeStruct = CS.ConStruct (2, 2, "hahaTEST");
	
	local ret3, ret4 = class_1:SetStruct(ret1)
	ASSERT_EQ(ret3.Byte, 100)
	ASSERT_EQ(tostring(ret3.Char), "98")
	ASSERT_EQ(string.sub(tostring(ret3.Decimal), 1, 11), "22.33333333")
	ASSERT_EQ(ret3.Double, 20.111111111111)
	ASSERT_EQ(string.sub(tostring(ret3.Float), 1, 7), "21.1233")  --精度损失
	ASSERT_EQ(ret3.IntVar1, 22345678)
	ASSERT_EQ(ret3.IntVar2, -22345678)
	ASSERT_EQ(tostring(ret3.Long), "9223372036854")
	ASSERT_EQ(tostring(ret3.ULong), "18446744073709")
	ASSERT_EQ(ret3.Short, 223)
	ASSERT_EQ(ret3.String, "lua call c#")
	ASSERT_EQ(ret3.UInt, 22345)
	ASSERT_EQ(ret3.UShort, 155)	
	ASSERT_EQ(ret3.IncludeStruct.x, 2)
	ASSERT_EQ(ret3.IncludeStruct.y, 2)
	ASSERT_EQ(ret3.IncludeStruct.z, "hahaTEST")
	
	ASSERT_EQ(ret4.Byte, 100)
	ASSERT_EQ(tostring(ret4.Char), "98")
	ASSERT_EQ(string.sub(tostring(ret4.Decimal), 1, 11), "22.33333333")
	ASSERT_EQ(ret4.Double, 20.111111111111)
	ASSERT_EQ(string.sub(tostring(ret4.Float), 1, 7), "21.1233") --精度损失
	ASSERT_EQ(ret4.IntVar1, 22345678)
	ASSERT_EQ(ret4.IntVar2, -22345678)
	ASSERT_EQ(tostring(ret4.Long), "9223372036854")
	ASSERT_EQ(tostring(ret4.ULong), "18446744073709")
	ASSERT_EQ(ret4.Short, 223)
	ASSERT_EQ(ret4.String, "lua call c#")
	ASSERT_EQ(ret4.UInt, 22345)
	ASSERT_EQ(ret4.UShort, 155)	
	ASSERT_EQ(ret4.IncludeStruct.x, 2)
	ASSERT_EQ(ret4.IncludeStruct.y, 2)
	ASSERT_EQ(ret4.IncludeStruct.z, "hahaTEST")
end

function CMyTestCaseLuaCallCS.CaseVisitGenCodeStruct_10(self)
	self.count = 1 + self.count
    local struct_1 = CS.GenCodeStruct()
	
	local ret = struct_1:Add(2, 3)
	struct_1.Byte = 10;
	struct_1.Char = 97;
	struct_1.Decimal = 12.33333333;
	struct_1.Double = 10.111111111111;
	struct_1.Float = 11.1234;
	struct_1.IntVar1 = 12345678;
	struct_1.IntVar2 = -12345678;
	struct_1.Long = CS.LongStatic.LONG_MAX;
	struct_1.ULong = CS.LongStatic.ULONG_MAX;
	struct_1.Short = 123;
	struct_1.String = "just for test";
	struct_1.UInt = 12345;
	struct_1.UShort = 255;
	struct_1.IncludeStruct = CS.HasConstructStruct (1, 2, "haha");
	ASSERT_EQ(ret, 5)
	ASSERT_EQ(struct_1.Byte, 10)
	ASSERT_EQ(tostring(struct_1.Char), "97")
	ASSERT_EQ(string.sub(tostring(struct_1.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(struct_1.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(struct_1.Float), 1, 7), "11.1233") --精度损失
	ASSERT_EQ(struct_1.IntVar1, 12345678)
	ASSERT_EQ(struct_1.IntVar2, -12345678)
	ASSERT_EQ(tostring(struct_1.Long), "9223372036854775807")
	--ASSERT_EQ(tostring(struct_1.ULong), "18446744073709551615")
	ASSERT_EQ(tostring(struct_1.ULong), "-1") -- V2.1.1
	ASSERT_EQ(struct_1.Short, 123)
	ASSERT_EQ(struct_1.String, "just for test")
	ASSERT_EQ(struct_1.UInt, 12345)
	ASSERT_EQ(struct_1.UShort, 255)	
	ASSERT_EQ(struct_1.IncludeStruct.x, 1)
	ASSERT_EQ(struct_1.IncludeStruct.y, 2)
	ASSERT_EQ(struct_1.IncludeStruct.z, "haha")
end

function CMyTestCaseLuaCallCS.CaseVisitGenCodeStruct_11(self)
	self.count = 1 + self.count
    	
	local ret = CS.GenCodeBaseClass.struct_var1:Add(2, 3)
	ASSERT_EQ(CS.GenCodeBaseClass.GBS, 1)
	CS.GenCodeBaseClass.GBS = 2;
	ASSERT_EQ(CS.GenCodeBaseClass.GBS, 2)
	CS.GenCodeBaseClass.GBS = 1;
	CS.GenCodeBaseClass.struct_var2 = CS.HasConstructStruct(33333333, 44444444, "test") --分开赋值会失败
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var2.x, 33333333)
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var2.y, 44444444)
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var2.z, "test")
	
	--[[值传递所以修改副本，不会改动原本
	CS.GenCodeBaseClass.struct_var1.Byte = 10;
	CS.GenCodeBaseClass.struct_var1.Char = 97;
	CS.GenCodeBaseClass.struct_var1.Decimal = 12.33333333;
	CS.GenCodeBaseClass.struct_var1.Double = 10.111111111111;
	CS.GenCodeBaseClass.struct_var1.Float = 11.1234;
	CS.GenCodeBaseClass.struct_var1.IntVar1 = 12345678;
	CS.GenCodeBaseClass.struct_var1.IntVar2 = -12345678;
	CS.GenCodeBaseClass.struct_var1.Long = CS.LongStatic.LONG_MAX;
	CS.GenCodeBaseClass.struct_var1.ULong = CS.LongStatic.ULONG_MAX;
	CS.GenCodeBaseClass.struct_var1.Short = 123;
	CS.GenCodeBaseClass.struct_var1.String = "just for test";
	CS.GenCodeBaseClass.struct_var1.UInt = 12345;
	CS.GenCodeBaseClass.struct_var1.UShort = 255;
	CS.GenCodeBaseClass.struct_var1.IncludeStruct = CS.HasConstructStruct (1, 2, "haha");
	ASSERT_EQ(ret, 5)
	
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var1.Byte, 10)
	ASSERT_EQ(tostring(CS.GenCodeBaseClass.struct_var1.Char), "97")
	ASSERT_EQ(string.sub(tostring(CS.GenCodeBaseClass.struct_var1.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var1.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(CS.GenCodeBaseClass.struct_var1.Float), 1, 7), "11.1234")
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var1.IntVar1, 12345678)
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var1.IntVar2, -12345678)
	ASSERT_EQ(tostring(CS.GenCodeBaseClass.struct_var1.Long), "9223372036854775807")
	ASSERT_EQ(tostring(CS.GenCodeBaseClass.struct_var1.ULong), "18446744073709551615")
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var1.Short, 123)
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var1.String, "just for test")
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var1.UInt, 12345)
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var1.UShort, 255)
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var1.IncludeStruct.x, 1)
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var1.IncludeStruct.y, 2)
	ASSERT_EQ(CS.GenCodeBaseClass.struct_var1.IncludeStruct.z, "haha")
	
	local struct_2 = CS.GenCodeBaseClass.GetStaticVar();
	ASSERT_EQ(struct_2.Byte, 10)
	ASSERT_EQ(tostring(struct_2.Char), "97")
	ASSERT_EQ(string.sub(tostring(struct_2.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(struct_2.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(struct_2.Float), 1, 7), "11.1234")
	ASSERT_EQ(struct_2.IntVar1, 12345678)
	ASSERT_EQ(struct_2.IntVar2, -12345678)
	ASSERT_EQ(tostring(struct_2.Long), "9223372036854775807")
	ASSERT_EQ(tostring(struct_2.ULong), "18446744073709551615")
	ASSERT_EQ(struct_2.Short, 123)
	ASSERT_EQ(struct_2.String, "just for test")
	ASSERT_EQ(struct_2.UInt, 12345)
	ASSERT_EQ(struct_2.UShort, 255)
	ASSERT_EQ(struct_2.IncludeStruct.x, 1)
	ASSERT_EQ(struct_2.IncludeStruct.y, 2)
	ASSERT_EQ(struct_2.IncludeStruct.z, "haha")]]
end


function CMyTestCaseLuaCallCS.CaseVisitGenCodeStruct_12_14_18(self)
	self.count = 1 + self.count
    	
	local struct_1 = CS.GenCodeStruct()
	
	struct_1.Byte = 10;
	struct_1.Char = 97;
	struct_1.Decimal = 12.33333333;
	struct_1.Double = 10.111111111111;
	struct_1.Float = 11.1234;
	struct_1.IntVar1 = 12345678;
	struct_1.IntVar2 = -12345678;
	struct_1.Long = CS.LongStatic.LONG_MAX;
	struct_1.ULong = CS.LongStatic.ULONG_MAX;
	struct_1.Short = 123;
	struct_1.String = "just for test";
	struct_1.UInt = 12345;
	struct_1.UShort = 256;
	struct_1.IncludeStruct = CS.HasConstructStruct (2, 2, "haha");
	
	local class_1 = CS.GenCodeBaseClass();
	local ret1, ret2 = class_1:SetStruct(struct_1)
	ASSERT_EQ(ret1.Byte, 10)
	ASSERT_EQ(tostring(ret1.Char), "97")
	ASSERT_EQ(string.sub(tostring(ret1.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(ret1.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(ret1.Float), 1, 7), "11.1233") --精度损失
	ASSERT_EQ(ret1.IntVar1, 12345678)
	ASSERT_EQ(ret1.IntVar2, -12345678)
	ASSERT_EQ(tostring(ret1.Long), "9223372036854775807")
	--ASSERT_EQ(tostring(ret1.ULong), "18446744073709551615")
	ASSERT_EQ(tostring(ret1.ULong), "-1") --v2.1.1
	ASSERT_EQ(ret1.Short, 123)
	ASSERT_EQ(ret1.String, "just for test")
	ASSERT_EQ(ret1.UInt, 12345)
	ASSERT_EQ(ret1.UShort, 256)
	ASSERT_EQ(ret1.IncludeStruct.x, 2)
	ASSERT_EQ(ret1.IncludeStruct.y, 2)
	ASSERT_EQ(ret1.IncludeStruct.z, "haha")
	
	ASSERT_EQ(ret2.Byte, 10)
	ASSERT_EQ(tostring(ret2.Char), "97")
	ASSERT_EQ(string.sub(tostring(ret2.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(ret2.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(ret2.Float), 1, 7), "11.1233") --精度损失
	ASSERT_EQ(ret2.IntVar1, 12345678) 
	ASSERT_EQ(ret2.IntVar2, -12345678)
	ASSERT_EQ(tostring(ret2.Long), "9223372036854775807")
	--ASSERT_EQ(tostring(ret2.ULong), "18446744073709551615")
	ASSERT_EQ(tostring(ret2.ULong), "-1") --V2.1.1
	ASSERT_EQ(ret2.Short, 123)
	ASSERT_EQ(ret2.String, "just for test")
	ASSERT_EQ(ret2.UInt, 12345)
	ASSERT_EQ(ret2.UShort, 256)	
	ASSERT_EQ(ret2.IncludeStruct.x, 2)
	ASSERT_EQ(ret2.IncludeStruct.y, 2)
	ASSERT_EQ(ret2.IncludeStruct.z, "haha")
end

function CMyTestCaseLuaCallCS.CaseVisitGenCodeStruct_13(self)
	self.count = 1 + self.count
	
	local class_1 = CS.GenCodeBaseClass();
	local ret1, ret2 = class_1:InitStruct()
	ASSERT_EQ(ret1.Byte, 100)
	ASSERT_EQ(tostring(ret1.Char), "98")
	ASSERT_EQ(string.sub(tostring(ret1.Decimal), 1, 11), "22.33333333")
	ASSERT_EQ(ret1.Double, 20.111111111111)
	ASSERT_EQ(string.sub(tostring(ret1.Float), 1, 7), "21.1233") --精度损失
	ASSERT_EQ(ret1.IntVar1, 22345678)
	ASSERT_EQ(ret1.IntVar2, -22345678)
	ASSERT_EQ(tostring(ret1.Long), "-9223372036854775808")
	ASSERT_EQ(tostring(ret1.ULong), "0")
	ASSERT_EQ(ret1.Short, 223)
	ASSERT_EQ(ret1.String, "2just for test")
	ASSERT_EQ(ret1.UInt, 22345)
	ASSERT_EQ(ret1.UShort, 128)
	ASSERT_EQ(ret1.IncludeStruct.x, 2)
	ASSERT_EQ(ret1.IncludeStruct.y, 2)
	ASSERT_EQ(ret1.IncludeStruct.z, "2haha")
	ASSERT_EQ(ret2, 0)
end

function CMyTestCaseLuaCallCS.CaseVisitGenCodeStruct_15(self)
	self.count = 1 + self.count
	
	local class_1 = CS.GenCodeBaseClass();
	local ret1, ret2 = class_1:InitStruct()
	ret1.Byte = 100
	ret1.Char = 98
	ret1.Decimal = 22.33333333
	ret1.Double = 20.111111111111
	ret1.Float = 21.1234
	ret1.IntVar1 = 22345678
	ret1.IntVar2 = -22345678
	ret1.Long = 9223372036854
	ret1.ULong = 18446744073709
	ret1.Short = 223
	ret1.String = "lua call c#"
	ret1.UInt = 22345
	ret1.UShort = 155
	ret1.IncludeStruct = CS.HasConstructStruct (2, 2, "hahaTEST");
	
	local ret3, ret4 = class_1:SetStruct(ret1)
	ASSERT_EQ(ret3.Byte, 100)
	ASSERT_EQ(tostring(ret3.Char), "98")
	ASSERT_EQ(string.sub(tostring(ret3.Decimal), 1, 11), "22.33333333")
	ASSERT_EQ(ret3.Double, 20.111111111111)
	ASSERT_EQ(string.sub(tostring(ret3.Float), 1, 7), "21.1233") --精度损失
	ASSERT_EQ(ret3.IntVar1, 22345678)
	ASSERT_EQ(ret3.IntVar2, -22345678)
	ASSERT_EQ(tostring(ret3.Long), "9223372036854")
	ASSERT_EQ(tostring(ret3.ULong), "18446744073709")
	ASSERT_EQ(ret3.Short, 223)
	ASSERT_EQ(ret3.String, "lua call c#")
	ASSERT_EQ(ret3.UInt, 22345)
	ASSERT_EQ(ret3.UShort, 155)	
	ASSERT_EQ(ret3.IncludeStruct.x, 2)
	ASSERT_EQ(ret3.IncludeStruct.y, 2)
	ASSERT_EQ(ret3.IncludeStruct.z, "hahaTEST")
	
	ASSERT_EQ(ret4.Byte, 100)
	ASSERT_EQ(tostring(ret4.Char), "98")
	ASSERT_EQ(string.sub(tostring(ret4.Decimal), 1, 11), "22.33333333")
	ASSERT_EQ(ret4.Double, 20.111111111111)
	ASSERT_EQ(string.sub(tostring(ret4.Float), 1, 7), "21.1233") --精度损失
	ASSERT_EQ(ret4.IntVar1, 22345678)
	ASSERT_EQ(ret4.IntVar2, -22345678)
	ASSERT_EQ(tostring(ret4.Long), "9223372036854")
	ASSERT_EQ(tostring(ret4.ULong), "18446744073709")
	ASSERT_EQ(ret4.Short, 223)
	ASSERT_EQ(ret4.String, "lua call c#")
	ASSERT_EQ(ret4.UInt, 22345)
	ASSERT_EQ(ret4.UShort, 155)	
	ASSERT_EQ(ret4.IncludeStruct.x, 2)
	ASSERT_EQ(ret4.IncludeStruct.y, 2)
	ASSERT_EQ(ret4.IncludeStruct.z, "hahaTEST")
end


function CMyTestCaseLuaCallCS.CaseVisitGenCodeStruct_17(self)
	self.count = 1 + self.count
	
	local class_1 = CS.GenCodeDrivedClass();
	local ret1, ret2 = class_1:InitStruct()
	ret1.Byte = 100
	ret1.Char = 98
	ret1.Decimal = 22.33333333
	ret1.Double = 20.111111111111
	ret1.Float = 11.1234
	ret1.IntVar1 = 22345678
	ret1.IntVar2 = -22345678
	ret1.Long = 9223372036854
	ret1.ULong = 18446744073709
	ret1.Short = 223
	ret1.String = "lua call c#"
	ret1.UInt = 22345
	ret1.UShort = 155
	ret1.IncludeStruct = CS.HasConstructStruct (2, 2, "hahaTEST");
	
	local ret3, ret4 = class_1:SetStruct(ret1)
	ASSERT_EQ(ret3.Byte, 100)
	ASSERT_EQ(tostring(ret3.Char), "98")
	ASSERT_EQ(string.sub(tostring(ret3.Decimal), 1, 11), "22.33333333")
	ASSERT_EQ(ret3.Double, 20.111111111111)
	ASSERT_EQ(string.sub(tostring(ret3.Float), 1, 7), "11.1233") --精度损失
	ASSERT_EQ(ret3.IntVar1, 22345678)
	ASSERT_EQ(ret3.IntVar2, -22345678)
	ASSERT_EQ(tostring(ret3.Long), "9223372036854")
	ASSERT_EQ(tostring(ret3.ULong), "18446744073709")
	ASSERT_EQ(ret3.Short, 223)
	ASSERT_EQ(ret3.String, "lua call c#")
	ASSERT_EQ(ret3.UInt, 22345)
	ASSERT_EQ(ret3.UShort, 155)	
	ASSERT_EQ(ret3.IncludeStruct.x, 2)
	ASSERT_EQ(ret3.IncludeStruct.y, 2)
	ASSERT_EQ(ret3.IncludeStruct.z, "hahaTEST")
	
	ASSERT_EQ(ret4.Byte, 100)
	ASSERT_EQ(tostring(ret4.Char), "98")
	ASSERT_EQ(string.sub(tostring(ret4.Decimal), 1, 11), "22.33333333")
	ASSERT_EQ(ret4.Double, 20.111111111111)
	ASSERT_EQ(string.sub(tostring(ret4.Float), 1, 7), "11.1233") --精度损失
	ASSERT_EQ(ret4.IntVar1, 22345678)
	ASSERT_EQ(ret4.IntVar2, -22345678)
	ASSERT_EQ(tostring(ret4.Long), "9223372036854")
	ASSERT_EQ(tostring(ret4.ULong), "18446744073709")
	ASSERT_EQ(ret4.Short, 223)
	ASSERT_EQ(ret4.String, "lua call c#")
	ASSERT_EQ(ret4.UInt, 22345)
	ASSERT_EQ(ret4.UShort, 155)	
	ASSERT_EQ(ret4.IncludeStruct.x, 2)
	ASSERT_EQ(ret4.IncludeStruct.y, 2)
	ASSERT_EQ(ret4.IncludeStruct.z, "hahaTEST")
end

function CMyTestCaseLuaCallCS.CaseVisitStruct_19(self)
    --19.struct从反射接口返回，作为生成代码接口输入/输入输出
	self.count = 1 + self.count
	
	local class_1 = CS.NoGenCodeBaseClass();
	local ret1, ret2 = class_1:InitStruct()
	ASSERT_EQ(ret1.Byte, 10)
	ASSERT_EQ(tostring(ret1.Char), "97")
	ASSERT_EQ(string.sub(tostring(ret1.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(ret1.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(ret1.Float), 1, 7), "11.1233")  --精度损失
	ASSERT_EQ(ret1.IntVar1, 12345678)
	ASSERT_EQ(ret1.IntVar2, -12345678)
	ASSERT_EQ(tostring(ret1.Long), "9223372036854775807")
	--ASSERT_EQ(tostring(ret1.ULong), "18446744073709551615")
	ASSERT_EQ(tostring(ret1.ULong), "-1")
	ASSERT_EQ(ret1.Short, 123)
	ASSERT_EQ(ret1.String, "just for test")
	ASSERT_EQ(ret1.UInt, 12345)
	ASSERT_EQ(ret1.UShort, 255)
	ASSERT_EQ(ret1.IncludeStruct.x, 1)
	ASSERT_EQ(ret1.IncludeStruct.y, 2)
	ASSERT_EQ(ret1.IncludeStruct.z, "haha")
	ASSERT_EQ(ret2, 0)
	
	local class_2 = CS.GenCodeBaseClass();
	local ret3, ret4 = class_2:SetStruct(ret1)
	ASSERT_EQ(ret3, 0)
	ASSERT_EQ(ret4.Byte, 10)
	ASSERT_EQ(tostring(ret4.Char), "97")
	ASSERT_EQ(string.sub(tostring(ret4.Decimal), 1, 11), "12.33333333")
	ASSERT_EQ(ret4.Double, 10.111111111111)
	ASSERT_EQ(string.sub(tostring(ret4.Float), 1, 7), "11.1233")  --精度损失
	ASSERT_EQ(ret4.IntVar1, 12345678)
	ASSERT_EQ(ret4.IntVar2, -12345678)
	ASSERT_EQ(tostring(ret4.Long), "9223372036854775807")
	--ASSERT_EQ(tostring(ret4.ULong), "18446744073709551615")
	ASSERT_EQ(tostring(ret4.ULong), "-1")
	ASSERT_EQ(ret4.Short, 123)
	ASSERT_EQ(ret4.String, "just for test")
	ASSERT_EQ(ret4.UInt, 12345)
	ASSERT_EQ(ret4.UShort, 255)
	ASSERT_EQ(ret4.IncludeStruct.x, 1)
	ASSERT_EQ(ret4.IncludeStruct.y, 2)
	ASSERT_EQ(ret4.IncludeStruct.z, "haha")
end

function CMyTestCaseLuaCallCS.CaseVisitStruct_20(self)
    --20.struct从生成代码接口返回，作为反射接口输入/输入输出
	self.count = 1 + self.count
	
	local class_1 = CS.GenCodeBaseClass();
	local ret1, ret2 = class_1:InitStruct()
	ASSERT_EQ(ret1.Byte, 100)
	ASSERT_EQ(tostring(ret1.Char), "98")
	ASSERT_EQ(string.sub(tostring(ret1.Decimal), 1, 11), "22.33333333")
	ASSERT_EQ(ret1.Double, 20.111111111111)
	ASSERT_EQ(string.sub(tostring(ret1.Float), 1, 7), "21.1233") --精度损失
	ASSERT_EQ(ret1.IntVar1, 22345678)
	ASSERT_EQ(ret1.IntVar2, -22345678)
	ASSERT_EQ(tostring(ret1.Long), "-9223372036854775808")
	ASSERT_EQ(tostring(ret1.ULong), "0")
	ASSERT_EQ(ret1.Short, 223)
	ASSERT_EQ(ret1.String, "2just for test")
	ASSERT_EQ(ret1.UInt, 22345)
	ASSERT_EQ(ret1.UShort, 128)
	ASSERT_EQ(ret1.IncludeStruct.x, 2)
	ASSERT_EQ(ret1.IncludeStruct.y, 2)
	ASSERT_EQ(ret1.IncludeStruct.z, "2haha")
	ASSERT_EQ(ret2, 0)
	
	local class_2 = CS.NoGenCodeBaseClass();
	local ret3, ret4 = class_2:SetStruct(ret1)
	ASSERT_EQ(ret3, 0)
	ASSERT_EQ(tostring(ret4.Char), "98")
	ASSERT_EQ(string.sub(tostring(ret4.Decimal), 1, 11), "22.33333333")
	ASSERT_EQ(ret4.Double, 20.111111111111)
	ASSERT_EQ(string.sub(tostring(ret4.Float), 1, 7), "21.1233") --精度损失
	ASSERT_EQ(ret4.IntVar1, 22345678)
	ASSERT_EQ(ret4.IntVar2, -22345678)
	ASSERT_EQ(tostring(ret4.Long), "-9223372036854775808")
	ASSERT_EQ(tostring(ret4.ULong), "0")
	ASSERT_EQ(ret4.Short, 223)
	ASSERT_EQ(ret4.String, "2just for test")
	ASSERT_EQ(ret4.UInt, 22345)
	ASSERT_EQ(ret4.UShort, 128)
	ASSERT_EQ(ret4.IncludeStruct.x, 2)
	ASSERT_EQ(ret4.IncludeStruct.y, 2)
	ASSERT_EQ(ret4.IncludeStruct.z, "2haha")
	ASSERT_EQ(ret2, 0)
end

function CMyTestCaseLuaCallCS.CaseVisitStruct_21(self)
    --21.A继承B，B继承C， A,C生成代码，B不生成代码--->B也需要生成代码，或者A,B,C全部不生成代码
	self.count = 1 + self.count
	
	--22.A的实例访问B，C的struct类型属性
	local class_a = CS.AClass(1, 2, "haha")
	ASSERT_EQ(class_a.BConStruct.x, 1)
	ASSERT_EQ(class_a.BConStruct.y, 2)
	ASSERT_EQ(class_a.BConStruct.z, "haha")
	ASSERT_EQ(class_a.CConStruct.x, 1)
	ASSERT_EQ(class_a.CConStruct.y, 2)
	ASSERT_EQ(class_a.CConStruct.z, "haha")
	
	--23.A的实例调用B的输入，输出，输入输出为struct类型的方法
	local struct_1 = CS.HasConstructStruct(2, 3, "TEST")
	ASSERT_EQ(struct_1.x, 2)
	ASSERT_EQ(struct_1.y, 3)
	ASSERT_EQ(struct_1.z, "TEST")
	local struct_2 = CS.HasConstructStruct(10, 1, "heihei")
	local ret1, ret2 = class_a:Sub(struct_1, struct_2)
	ASSERT_EQ(struct_2.x, 2)
	ASSERT_EQ(ret1.x, 2)
	ASSERT_EQ(ret1.y, 1)
	ASSERT_EQ(ret1.z, "heihei")
	ASSERT_EQ(ret2.x, -8)
	ASSERT_EQ(ret2.y, 3)
	ASSERT_EQ(ret2.z, "TEST")
	
	--24.A的实例调用C的输入，输出，输入输出为struct类型的方法
	local ret3, ret4 = class_a:Add(struct_1, struct_2)
	ASSERT_EQ(struct_2.x, 2)
	ASSERT_EQ(ret3.x, 2)
	ASSERT_EQ(ret3.y, 1)
	ASSERT_EQ(ret3.z, "heihei")
    ASSERT_EQ(ret4.x, 4)
	ASSERT_EQ(ret4.y, 3)
	ASSERT_EQ(ret4.z, "TEST")
end

function CMyTestCaseLuaCallCS.CaseVisitStructVaribleParam(self)
    --可变函数参数为struct
	self.count = 1 + self.count
	
	local struct_1 = CS.HasConstructStruct(2, 3, "TEST")
	local struct_2 = CS.HasConstructStruct(10, 1, "heihei")
	local class_c = CS.CClass(1, 2, "haha")
	local ret = class_c:VariableParamFunc(struct_1, struct_2)
	ASSERT_EQ(ret, 12)
end

function CMyTestCaseLuaCallCS.CaseVisitFloat1(self)

    self.count = 1 + self.count
	
	local struct_2 = CS.NoGen2FloatStruct(1.17549435e-38, 2.125)
	ASSERT_EQ(string.sub(tostring(struct_2.a), 1, 10) , "1.17549435")
	ASSERT_EQ(string.sub(tostring(struct_2.a), -4, -1) , "e-38")
	ASSERT_EQ(string.sub(tostring(struct_2.b), 1, 5), "2.125")
end

function CMyTestCaseLuaCallCS.CaseVisitStructFloat1(self)
    --struct有2个float属性，不生成代码，会有精度损失
	self.count = 1 + self.count
	
	local struct_2 = CS.NoGen2FloatStruct(1.234, 2.125)
	ASSERT_EQ(string.sub(tostring(struct_2.a), 1, 5), "1.233")
	ASSERT_EQ(string.sub(tostring(struct_2.b), 1, 5), "2.125")
	
	local struct_3 = CS.NoGen3FloatStruct(struct_2.a, struct_2.b, struct_2.a + struct_2.b)
	
	ASSERT_EQ(string.sub(tostring(struct_3.a), 1, 5), "1.233")
	ASSERT_EQ(string.sub(tostring(struct_3.b), 1, 5), "2.125")
	ASSERT_EQ(string.sub(tostring(struct_3.c), 1, 5), "3.358")
	
	local struct_4 = CS.NoGen4FloatStruct(struct_3.a, struct_3.b, struct_3.a + struct_3.b, struct_3.c)
	
	ASSERT_EQ(string.sub(tostring(struct_4.a), 1, 5), "1.233")
	ASSERT_EQ(string.sub(tostring(struct_4.b), 1, 5), "2.125")
	ASSERT_EQ(string.sub(tostring(struct_4.c), 1, 5), "3.358")
	ASSERT_EQ(string.sub(tostring(struct_4.d), 1, 5), "3.358")
	
	local struct_5 = CS.NoGen5FloatStruct(struct_2.a - struct_2.b, 2.125, 3.123, 4.500, 5.333)
	ASSERT_EQ(string.sub(tostring(struct_5.a), 1, 6), "-0.891")
	ASSERT_EQ(string.sub(tostring(struct_5.b), 1, 5), "2.125")
	ASSERT_EQ(string.sub(tostring(struct_5.c), 1, 5), "3.122")
	ASSERT_EQ(string.sub(tostring(struct_5.d), 1, 5), "4.5")
	ASSERT_EQ(string.sub(tostring(struct_5.e), 1, 5), "5.333")
	
	local struct_6 = CS.NoGen6FloatStruct(struct_2.a * struct_2.b, struct_2.a/2, 3.123, 4.500, 5.333, 2.1)
	ASSERT_EQ(string.sub(tostring(struct_6.a), 1, 5), "2.622")
	ASSERT_EQ(string.sub(tostring(struct_6.b), 1, 6), "0.6169")
	ASSERT_EQ(string.sub(tostring(struct_6.c), 1, 5), "3.122")
	ASSERT_EQ(string.sub(tostring(struct_6.d), 1, 5), "4.5")
	ASSERT_EQ(string.sub(tostring(struct_6.e), 1, 5), "5.333")
	ASSERT_EQ(string.sub(tostring(struct_6.f), 1, 5), "2.099")
end

function CMyTestCaseLuaCallCS.CaseVisitStructFloatInClass(self)
	--class包含多个struct类型属性（struct有多个float属性），生成代码和不生成代码各种参数传递，会有精度损失
	self.count = 1 + self.count
	local class_1 = CS.TestNoGenFloatStructClass()
	local class_2 = CS.TestGenFloatStructClass()
	
	local no_gen_struct_2 = CS.NoGen2FloatStruct(1.234, 2.125)
	no_gen_struct_2.a = 1.234
	no_gen_struct_2.b = 2.125
	
	local no_gen_struct_3 = CS.NoGen3FloatStruct(970.12, 1012.34, 1111.45)
	no_gen_struct_3.a = 970.12
	no_gen_struct_3.b = 1012.34
	no_gen_struct_3.c = 1111.45
	
	local no_gen_struct_4 = CS.NoGen4FloatStruct(1.0, 1.0, 1.0, 2.0)
	no_gen_struct_4.a = 1018.2
	no_gen_struct_4.b = -972.1
	no_gen_struct_4.c = -100.2
	no_gen_struct_4.d = 999.2
	
	local no_gen_struct_5 = CS.NoGen5FloatStruct(1.2, 1.2, 1.2, 1.3, 1.3)
	no_gen_struct_5.a = 2.120
	no_gen_struct_5.b = 2.125
	no_gen_struct_5.c = 3.123
	no_gen_struct_5.d = 4.500
	no_gen_struct_5.e = 5.333
	
	local no_gen_struct_6 = CS.NoGen6FloatStruct(1.1, 1.1, 1.1, 1.1, 1.1, 1.2)
	no_gen_struct_6.a = 10.11
	no_gen_struct_6.b = 120.22
	no_gen_struct_6.c = 3.123
	no_gen_struct_6.d = 4.500
	no_gen_struct_6.e = 5.333
	no_gen_struct_6.f = 2.1
	
	local gen_struct_2 = CS.Gen2FloatStruct(1.0, 1.2)
	gen_struct_2.a = 1.234
	gen_struct_2.b = 2.125
	
	local gen_struct_3 = CS.Gen3FloatStruct(1.3, 1.3, 1.3)
	gen_struct_3.a = 970.12
	gen_struct_3.b = 1012.34
	gen_struct_3.c = 1111.45
	
	local gen_struct_4 = CS.Gen4FloatStruct(1.0, 1.0, 1.0, 2.0)
	gen_struct_4.a = 1018.2
	gen_struct_4.b = -972.1
	gen_struct_4.c = -100.2
	gen_struct_4.d = 999.2
	
	local gen_struct_5 = CS.Gen5FloatStruct(1.2, 1.2, 1.2, 1.3, 1.3)
	gen_struct_5.a = 2.120
	gen_struct_5.b = 2.125
	gen_struct_5.c = 3.123
	gen_struct_5.d = 4.500
	gen_struct_5.e = 5.333
	
	local gen_struct_6 = CS.Gen6FloatStruct(1.1, 1.1, 1.1, 1.1, 1.1, 1.2)
	gen_struct_6.a = 10.11
	gen_struct_6.b = 120.22
	gen_struct_6.c = 3.123
	gen_struct_6.d = 4.500
	gen_struct_6.e = 5.333
	gen_struct_6.f = 2.1
	
	class_1.Struct2 = no_gen_struct_2;
	class_1.Struct3 = no_gen_struct_3;
	class_1.Struct4 = no_gen_struct_4;
	class_1.Struct5 = no_gen_struct_5;
	class_1.Struct6 = no_gen_struct_6;
	ASSERT_EQ(string.sub(tostring(class_1.Struct2.a), 1, 5), "1.233")
	ASSERT_EQ(string.sub(tostring(class_1.Struct2.b), 1, 5), "2.125")
	ASSERT_EQ(string.sub(tostring(class_1.Struct3.a), 1, 6), "970.11")
	ASSERT_EQ(string.sub(tostring(class_1.Struct3.b), 1, 7), "1012.34")
	ASSERT_EQ(string.sub(tostring(class_1.Struct3.c), 1, 7), "1111.44")
	ASSERT_EQ(string.sub(tostring(class_1.Struct4.a), 1, 7), "1018.20")
	ASSERT_EQ(string.sub(tostring(class_1.Struct4.b), 1, 7), "-972.09")
	ASSERT_EQ(string.sub(tostring(class_1.Struct4.c), 1, 7), "-100.19")
	ASSERT_EQ(string.sub(tostring(class_1.Struct4.d), 1, 6), "999.20")
	ASSERT_EQ(string.sub(tostring(class_1.Struct5.a), 1, 5), "2.119")
	ASSERT_EQ(string.sub(tostring(class_1.Struct5.b), 1, 5), "2.125")
	ASSERT_EQ(string.sub(tostring(class_1.Struct5.c), 1, 5), "3.122")
	ASSERT_EQ(string.sub(tostring(class_1.Struct5.d), 1, 5), "4.5")
	ASSERT_EQ(string.sub(tostring(class_1.Struct5.e), 1, 5), "5.333")
	ASSERT_EQ(string.sub(tostring(class_1.Struct6.a), 1, 6), "10.109")
	ASSERT_EQ(string.sub(tostring(class_1.Struct6.b), 1, 7), "120.220")
	ASSERT_EQ(string.sub(tostring(class_1.Struct6.c), 1, 5), "3.122")
	ASSERT_EQ(string.sub(tostring(class_1.Struct6.d), 1, 5), "4.5")
	ASSERT_EQ(string.sub(tostring(class_1.Struct6.e), 1, 5), "5.333")
	ASSERT_EQ(string.sub(tostring(class_1.Struct6.f), 1, 5), "2.099")
	
	class_2.Struct2 = gen_struct_2;
	class_2.Struct3 = gen_struct_3;
	class_2.Struct4 = gen_struct_4;
	class_2.Struct5 = gen_struct_5;
	class_2.Struct6 = gen_struct_6;
	ASSERT_EQ(string.sub(tostring(class_2.Struct2.a), 1, 5), "1.233")
	ASSERT_EQ(string.sub(tostring(class_2.Struct2.b), 1, 5), "2.125")
	ASSERT_EQ(string.sub(tostring(class_2.Struct3.a), 1, 6), "970.11")
	ASSERT_EQ(string.sub(tostring(class_2.Struct3.b), 1, 7), "1012.34")
	ASSERT_EQ(string.sub(tostring(class_2.Struct3.c), 1, 7), "1111.44")
	ASSERT_EQ(string.sub(tostring(class_2.Struct4.a), 1, 7), "1018.20")
	ASSERT_EQ(string.sub(tostring(class_2.Struct4.b), 1, 7), "-972.09")
	ASSERT_EQ(string.sub(tostring(class_2.Struct4.c), 1, 7), "-100.19")
	ASSERT_EQ(string.sub(tostring(class_2.Struct4.d), 1, 6), "999.20")
	ASSERT_EQ(string.sub(tostring(class_2.Struct5.a), 1, 5), "2.119")
	ASSERT_EQ(string.sub(tostring(class_2.Struct5.b), 1, 5), "2.125")
	ASSERT_EQ(string.sub(tostring(class_2.Struct5.c), 1, 5), "3.122")
	ASSERT_EQ(string.sub(tostring(class_2.Struct5.d), 1, 5), "4.5")
	ASSERT_EQ(string.sub(tostring(class_2.Struct5.e), 1, 5), "5.333")
	ASSERT_EQ(string.sub(tostring(class_2.Struct6.a), 1, 6), "10.109")
	ASSERT_EQ(string.sub(tostring(class_2.Struct6.b), 1, 7), "120.220")
	ASSERT_EQ(string.sub(tostring(class_2.Struct6.c), 1, 5), "3.122")
	ASSERT_EQ(string.sub(tostring(class_2.Struct6.d), 1, 5), "4.5")
	ASSERT_EQ(string.sub(tostring(class_2.Struct6.e), 1, 5), "5.333")
	ASSERT_EQ(string.sub(tostring(class_2.Struct6.f), 1, 5), "2.099")
	
	local ret1, ret2 = class_1:Add2(no_gen_struct_2, gen_struct_2)
	ASSERT_EQ(string.sub(tostring(ret1.a), 1, 5), "2.467")
	ASSERT_EQ(string.sub(tostring(ret1.b), 1, 5), "4.25")
	ASSERT_EQ(string.sub(tostring(ret2.a), 1, 5), "2.467")
	ASSERT_EQ(string.sub(tostring(ret2.b), 1, 5), "4.25")
	
end

function CMyTestCaseLuaCallCS.CaseVisitGenStructFloat1(self)
    --struct有2个float属性，生成代码，会有精度损失
	self.count = 1 + self.count
	
	local struct_2 = CS.Gen2FloatStruct(1.234, 2.125)
	ASSERT_EQ(string.sub(tostring(struct_2.a), 1, 5), "1.233")
	ASSERT_EQ(string.sub(tostring(struct_2.b), 1, 5), "2.125")
	
	local struct_3 = CS.Gen3FloatStruct(struct_2.a, struct_2.b, struct_2.a + struct_2.b)
	
	ASSERT_EQ(string.sub(tostring(struct_3.a), 1, 5), "1.233")
	ASSERT_EQ(string.sub(tostring(struct_3.b), 1, 5), "2.125")
	ASSERT_EQ(string.sub(tostring(struct_3.c), 1, 5), "3.358")
	
	local struct_4 = CS.Gen4FloatStruct(struct_3.a, struct_3.b, struct_3.a + struct_3.b, struct_3.c)
	
	ASSERT_EQ(string.sub(tostring(struct_4.a), 1, 5), "1.233")
	ASSERT_EQ(string.sub(tostring(struct_4.b), 1, 5), "2.125")
	ASSERT_EQ(string.sub(tostring(struct_4.c), 1, 5), "3.358")
	ASSERT_EQ(string.sub(tostring(struct_4.d), 1, 5), "3.358")
	
	local struct_5 = CS.Gen5FloatStruct(struct_2.a - struct_2.b, 2.125, 3.123, 4.500, 5.333)
	ASSERT_EQ(string.sub(tostring(struct_5.a), 1, 6), "-0.891")
	ASSERT_EQ(string.sub(tostring(struct_5.b), 1, 5), "2.125")
	ASSERT_EQ(string.sub(tostring(struct_5.c), 1, 5), "3.122")
	ASSERT_EQ(string.sub(tostring(struct_5.d), 1, 5), "4.5")
	ASSERT_EQ(string.sub(tostring(struct_5.e), 1, 5), "5.333")
	
	local struct_6 = CS.Gen6FloatStruct(struct_2.a * struct_2.b, struct_2.a/2, 3.123, 4.500, 5.333, 2.1)
	ASSERT_EQ(string.sub(tostring(struct_6.a), 1, 5), "2.622")
	ASSERT_EQ(string.sub(tostring(struct_6.b), 1, 6), "0.6169")
	ASSERT_EQ(string.sub(tostring(struct_6.c), 1, 5), "3.122")
	ASSERT_EQ(string.sub(tostring(struct_6.d), 1, 5), "4.5")
	ASSERT_EQ(string.sub(tostring(struct_6.e), 1, 5), "5.333")
	ASSERT_EQ(string.sub(tostring(struct_6.f), 1, 5), "2.099")
end

function CMyTestCaseLuaCallCS.CaseVisitStructInt1(self)
    --struct有2个int属性，不生成代码
	self.count = 1 + self.count
	
	local struct_2 = CS.NoGen2IntStruct(2, 3)
	ASSERT_EQ(struct_2.a, 2)
	ASSERT_EQ(struct_2.b, 3)
end

function CMyTestCaseLuaCallCS.CaseVisitStructIntString1(self)
    --struct有2个int属性， 1个string属性，不生成代码
	self.count = 1 + self.count
	
	local struct_2 = CS.ConStruct(2, 3, "haha")
	ASSERT_EQ(struct_2.x, 2)
	ASSERT_EQ(struct_2.y, 3)
	ASSERT_EQ(struct_2.z, "haha")
end

function CMyTestCaseLuaCallCS.CaseReflectEvent1(self)
	self.count = 1 + self.count

	gTestNumber = 1
	local testObj = CS.TestReflectEventClass()
	testObj:TestEvent1('+', EvtFunc11)
	local ret = testObj:CallEvent(1)
	ASSERT_EQ(2, ret)
	testObj:TestEvent1('+', EvtFunc12)
	local ret = testObj:CallEvent(2)
	ASSERT_EQ(7, ret)
	testObj:TestEvent1('+', EvtFunc12)
	local ret = testObj:CallEvent(1)
	ASSERT_EQ(12, ret)
	testObj:TestEvent1('-', EvtFunc12)
	local ret = testObj:CallEvent(1)
	ASSERT_EQ(15, ret)
	testObj:TestEvent1('-', EvtFunc12)
	local ret = testObj:CallEvent(1)
	ASSERT_EQ(16, ret)
	testObj:TestEvent1('-', EvtFunc12)
	local ret = testObj:CallEvent(1)
	ASSERT_EQ(17, ret)
	testObj:TestEvent1('-', EvtFunc11)
end

function CMyTestCaseLuaCallCS.CaseEventStatic(self)
	self.count = 1 + self.count

	gTestNumber = 1
	CS.LuaTestObj.TestStaticEvent1('+', EvtFunc11)
	local ret = CS.LuaTestObj.CallStaticEvent(1)
	ASSERT_EQ(2, ret)
	CS.LuaTestObj.TestStaticEvent1('+', EvtFunc12)
	local ret = CS.LuaTestObj.CallStaticEvent(2)
	ASSERT_EQ(7, ret)
	CS.LuaTestObj.TestStaticEvent1('+', EvtFunc12)
	local ret = CS.LuaTestObj.CallStaticEvent(1)
	ASSERT_EQ(12, ret)
	CS.LuaTestObj.TestStaticEvent1('-', EvtFunc12)
	local ret = CS.LuaTestObj.CallStaticEvent(1)
	ASSERT_EQ(15, ret)
	CS.LuaTestObj.TestStaticEvent1('-', EvtFunc12)
	local ret = CS.LuaTestObj.CallStaticEvent(1)
	ASSERT_EQ(16, ret)
	CS.LuaTestObj.TestStaticEvent1('-', EvtFunc12)
	local ret = CS.LuaTestObj.CallStaticEvent(1)
	ASSERT_EQ(17, ret)
	CS.LuaTestObj.TestStaticEvent1('-', EvtFunc11)
end

function CMyTestCaseLuaCallCS.CaseUpLowerMethod(self)
	self.count = 1 + self.count
	CS.LuaTestObj.initNumber = 5
	local ret = CS.LuaTestObj.CalcAdd(1)
	ASSERT_EQ(6, ret)
	
	local ret = CS.LuaTestObj.calcadd(2)
	ASSERT_EQ(8, ret)
end

function CMyTestCaseLuaCallCS.CaseUpLowerMethod(self)
	self.count = 1 + self.count

	local ret = CS.LuaTestObj.OverLoad1(1, 2, 3, 5)
	ASSERT_EQ(11, ret)
end

function CMyTestCaseLuaCallCS.CaseVisitStaticPusherStruct_1(self)
	self.count = 1 + self.count
    local struct_1 = CS.StaticPusherStructA(97, -100)
	local struct_2 = CS.StaticPusherStructB(-32525, 65535, -2147483647, 4294967295)
	ASSERT_EQ(struct_1.byteVar, 97)
	ASSERT_EQ(struct_1.sbyteVar, -100)
	ASSERT_EQ(struct_2.shortVar, -32525)
	ASSERT_EQ(struct_2.ushortVar, 65535)
	ASSERT_EQ(struct_2.intVar, -2147483647)
	ASSERT_EQ(struct_2.uintVar, 4294967295)
	local struct_all = CS.StaticPusherStructAll()
	struct_all.structA = struct_1
	struct_all.structB = struct_2
	struct_all.longVar = -4294967297
	struct_all.ulongVar = 4294967297
	struct_all.floatVar = 1.123456
	struct_all.doubleVar = 1.7976931348623157
	ASSERT_EQ(struct_all.structA.byteVar, 97)
	ASSERT_EQ(struct_all.structA.sbyteVar, -100)
	ASSERT_EQ(struct_all.structB.shortVar, -32525)
	ASSERT_EQ(struct_all.structB.ushortVar, 65535)
	ASSERT_EQ(struct_all.structB.intVar, -2147483647)
	ASSERT_EQ(struct_all.structB.uintVar, 4294967295)
	ASSERT_EQ(tostring(struct_all.longVar + 4294967297), "0")
	ASSERT_EQ(tostring(struct_all.ulongVar), "4294967297")
	ASSERT_EQ(string.sub(tostring(struct_all.floatVar), 1, 8), "1.123456")
	ASSERT_EQ(string.sub(tostring(struct_all.doubleVar), 1, 14), "1.797693134862")
	
	local struct_all_2 = CS.StaticPusherStructAll()
	struct_all_2.structA =  CS.StaticPusherStructA(0, 100)
	struct_all_2.structB = CS.StaticPusherStructB(-1, 2, 214748364, 429496729)
	struct_all_2.longVar = 4294967298
	struct_all_2.ulongVar = 4294967297
	struct_all_2.floatVar = 2.12345
	struct_all_2.doubleVar = 2.7976931348
	local class_1 = CS.NoGenCodeBaseClass()
	local class_2 = CS.GenCodeBaseClass()
	
	local ret1, ret2 = class_1:SetStaticPusherStruct(struct_all_2, struct_all)
	print("test ok")
	ASSERT_EQ(struct_all.structA.byteVar, 98)
	ASSERT_EQ(struct_all.structA.sbyteVar, 101)
	ASSERT_EQ(struct_all.structB.shortVar, -1)
	ASSERT_EQ(struct_all.structB.ushortVar, 2)
	ASSERT_EQ(struct_all.structB.intVar, -2147483646)
	ASSERT_EQ(struct_all.structB.uintVar, 0)
	ASSERT_EQ(tostring(struct_all.longVar + 4294967298), "0")
	ASSERT_EQ(tostring(struct_all.ulongVar), "4294967296")
	ASSERT_EQ(string.sub(tostring(struct_all.floatVar), 1, 8), "0.123456")
	ASSERT_EQ(string.sub(tostring(struct_all.doubleVar), 1, 14), "0.797693134862")
	ASSERT_EQ(ret1.structA.byteVar, 98)
	ASSERT_EQ(ret1.structA.sbyteVar, 101)
	ASSERT_EQ(ret1.structB.shortVar, -1)
	ASSERT_EQ(ret1.structB.ushortVar, 2)
	ASSERT_EQ(ret1.structB.intVar, -2147483646)
	ASSERT_EQ(ret1.structB.uintVar, 0)
	ASSERT_EQ(tostring(ret1.longVar + 4294967298), "0")
	ASSERT_EQ(tostring(ret1.ulongVar), "4294967296")
	ASSERT_EQ(string.sub(tostring(ret1.floatVar), 1, 8), "0.123456")
	ASSERT_EQ(string.sub(tostring(ret1.doubleVar), 1, 14), "0.797693134862")
	local ret3 = class_1.static_pushstruct_var;
	ASSERT_EQ(ret3.structA.byteVar, 10)
	ASSERT_EQ(ret3.structA.sbyteVar, 100)
	ASSERT_EQ(ret3.structB.shortVar, -32525)
	ASSERT_EQ(ret3.structB.ushortVar, 2)
	ASSERT_EQ(ret3.structB.intVar, -1932735283)
	ASSERT_EQ(ret3.structB.uintVar, 429496728)
	ASSERT_EQ(tostring(ret3.longVar), "1")
	ASSERT_EQ(tostring(ret3.ulongVar), "8589934594")
	ASSERT_EQ(string.sub(tostring(ret3.floatVar), 1, 8), "3.246906")
	ASSERT_EQ(string.sub(tostring(ret3.doubleVar), 1, 14), "4.595386269662")
	ASSERT_EQ(ret2.structA.byteVar, 10)
	ASSERT_EQ(ret2.structA.sbyteVar, 100)
	ASSERT_EQ(ret2.structB.shortVar, -32525)
	ASSERT_EQ(ret2.structB.ushortVar, 2)
	ASSERT_EQ(ret2.structB.intVar, -1932735283)
	ASSERT_EQ(ret2.structB.uintVar, 429496728)
	ASSERT_EQ(tostring(ret2.longVar), "1")
	ASSERT_EQ(tostring(ret2.ulongVar), "8589934594")
	ASSERT_EQ(string.sub(tostring(ret2.floatVar), 1, 8), "3.246906")
	ASSERT_EQ(string.sub(tostring(ret2.doubleVar), 1, 14), "4.595386269662")
	
	
	local ret1, ret2 = class_2:SetStaticPusherStruct(struct_all_2, struct_all)
	print("test ok")
	ASSERT_EQ(struct_all.structA.byteVar, 98)
	ASSERT_EQ(struct_all.structA.sbyteVar, 101)
	ASSERT_EQ(struct_all.structB.shortVar, -1)
	ASSERT_EQ(struct_all.structB.ushortVar, 2)
	ASSERT_EQ(struct_all.structB.intVar, -2147483645)
	ASSERT_EQ(struct_all.structB.uintVar, 1)
	ASSERT_EQ(tostring(struct_all.longVar + 4294967299), "0")
	ASSERT_EQ(tostring(struct_all.ulongVar), "4294967295")
	ASSERT_EQ(string.sub(tostring(struct_all.floatVar), 1, 8), "-0.87654")
	ASSERT_EQ(string.sub(tostring(struct_all.doubleVar), 1, 14), "-0.20230686513")
	ASSERT_EQ(ret1.structA.byteVar, 98)
	ASSERT_EQ(ret1.structA.sbyteVar, 101)
	ASSERT_EQ(ret1.structB.shortVar, -1)
	ASSERT_EQ(ret1.structB.ushortVar, 2)
	ASSERT_EQ(ret1.structB.intVar, -2147483645)
	ASSERT_EQ(ret1.structB.uintVar, 1)
	ASSERT_EQ(tostring(ret1.longVar + 4294967299), "0")
	ASSERT_EQ(tostring(ret1.ulongVar), "4294967295")
	ASSERT_EQ(string.sub(tostring(ret1.floatVar), 1, 8), "-0.87654")
	ASSERT_EQ(string.sub(tostring(ret1.doubleVar), 1, 14), "-0.20230686513")
	local ret3 = class_2.static_pushstruct_var;
	ASSERT_EQ(ret3.structA.byteVar, 97)
	ASSERT_EQ(ret3.structA.sbyteVar, 100)
	ASSERT_EQ(ret3.structB.shortVar, -1)
	ASSERT_EQ(ret3.structB.ushortVar, 2)
	ASSERT_EQ(ret3.structB.intVar, -1932735282)
	ASSERT_EQ(ret3.structB.uintVar, 429496729)
	ASSERT_EQ(tostring(ret3.longVar), "0")
	ASSERT_EQ(tostring(ret3.ulongVar), "8589934593")
	ASSERT_EQ(string.sub(tostring(ret3.floatVar), 1, 8), "2.246906")
	ASSERT_EQ(string.sub(tostring(ret3.doubleVar), 1, 14), "3.595386269662")
	ASSERT_EQ(ret2.structA.byteVar, 97)
	ASSERT_EQ(ret2.structA.sbyteVar, 100)
	ASSERT_EQ(ret2.structB.shortVar, -1)
	ASSERT_EQ(ret2.structB.ushortVar, 2)
	ASSERT_EQ(ret2.structB.intVar, -1932735282)
	ASSERT_EQ(ret2.structB.uintVar, 429496729)
	ASSERT_EQ(tostring(ret2.longVar), "0")
	ASSERT_EQ(tostring(ret2.ulongVar), "8589934593")
	ASSERT_EQ(string.sub(tostring(ret2.floatVar), 1, 8), "2.246906")
	ASSERT_EQ(string.sub(tostring(ret2.doubleVar), 1, 14), "3.595386269662")
end

function CMyTestCaseLuaCallCS.CaseGetStaticSum(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.Sum(1, 2)
	ASSERT_EQ(ret, 3)
end

function CMyTestCaseLuaCallCS.CaseGetSum(self)
    self.count = 1 + self.count
	local class = CS.LuaTestObj()
	local ret = class:Sum(1, 2, 6)
	ASSERT_EQ(ret, 9)
end

function CMyTestCaseLuaCallCS.CaseVisitTemplateMethod(self)
    self.count = 1 + self.count
	ASSERT_EQ(CS.EmployeeTemplate.GetBasicSalary, nil)
	ASSERT_EQ(CS.EmployeeTemplate.AddBonus, nil)
	local class = CS.Manager()
	ASSERT_EQ(1, class:GetBasicSalary())   
end

function CMyTestCaseLuaCallCS.CaseVisitGenericMethod(self)
	self.count = 1 + self.count
	local class = CS.LuaTestObj()
	local a = "abc"
	if (true) then
		local ret, error = pcall(function() class:GenericMethod(a) end)
		ASSERT_EQ(ret, false)
    else
        ASSERT_EQ(true, false)
	end
end

function CMyTestCaseLuaCallCS.CaseVisitIntPtr(self)
    self.count = 1 + self.count
	local class = CS.LuaTestObj()
	local ptr = class.ptr
	print(ptr)
	local ptr1 = class:GetPtr()
    print(ptr1)
    local bytevar = class:PrintPtr(ptr1)
	ASSERT_EQ(bytevar, 97)
end

function CMyTestCaseLuaCallCS.CaseVisitVarAndDefaultFunc1(self)
    self.count = 1 + self.count
	local class = CS.LuaTestObj()

	local ret = class:VariableParamFuncDefault(1)
	ASSERT_EQ(ret, 2)
	
	local ret = CS.LuaTestObj.StaticVariableParamFuncDefault(1.0)
	ASSERT_EQ(ret, 2.0)
end

function CMyTestCaseLuaCallCS.CaseVisitVarAndDefaultFunc2(self)
    self.count = 1 + self.count
	local class = CS.LuaTestObj()

	local ret = class:VariableParamFuncDefault(1, 2, "john", "che")
	ASSERT_EQ(ret, 3)
	
	local ret = CS.LuaTestObj.StaticVariableParamFuncDefault(1.0, 2.0, "john", "che")
	ASSERT_EQ(ret, 3.0)
end

function CMyTestCaseLuaCallCS.CaseFuncReturnByteArray(self)
    self.count = 1 + self.count

	local ret = CS.LuaTestObj.FuncReturnByteArray()
	ASSERT_EQ(ret, "abc")
end

function CMyTestCaseLuaCallCS.CaseFuncReturnByte(self)
    self.count = 1 + self.count

	local ret = CS.LuaTestObj.FuncReturnByte()
	ASSERT_EQ(ret, 97)
end

function CMyTestCaseLuaCallCS.CaseFuncReturnIntArray(self)
    self.count = 1 + self.count

	local ret = CS.LuaTestObj.FuncReturnIntArray()
	ASSERT_EQ(type(ret), "userdata")
end

function CMyTestCaseLuaCallCS.CaseFuncReturnInt(self)
    self.count = 1 + self.count
	
	local ret = CS.LuaTestObj.FuncReturnInt()
	ASSERT_EQ(ret, 97)
end

function CMyTestCaseLuaCallCS.CaseFuncUint64Tostring(self)
    self.count = 1 + self.count
	local uint64Var = 1234567890
	local ret = uint64.tostring(uint64Var)
	ASSERT_EQ(ret, "1234567890")
	local ret = uint64.tostring(CS.LongStatic.ULONG_MAX)
	ASSERT_EQ(ret, "18446744073709551615")
	local ret = uint64.tostring(0)
	ASSERT_EQ(ret, "0")
	local ret = uint64.tostring(CS.LongStatic.LONG_MAX)
	ASSERT_EQ(ret, "9223372036854775807")
	local ret = uint64.tostring(-1)
	ASSERT_EQ(ret, uint64.tostring(CS.LongStatic.ULONG_MAX))
end

function CMyTestCaseLuaCallCS.CaseFuncUint64divide(self)
    self.count = 1 + self.count
	local uint64Var = 1234567890
	local ret = uint64.divide(uint64Var, 10)
	ASSERT_EQ(tostring(ret), "123456789")
	local ret = uint64.divide(CS.LongStatic.ULONG_MAX, 10)
	ASSERT_EQ(tostring(ret), "1844674407370955161")
	local ret = uint64.divide(CS.LongStatic.LONG_MAX, 100)
	ASSERT_EQ(tostring(ret), "92233720368547758")
	local ret = uint64.divide(CS.LongStatic.ULONG_MAX, -1)
	ASSERT_EQ(tostring(ret), "1")
	local ret = uint64.divide(0, CS.LongStatic.ULONG_MAX)
	ASSERT_EQ(tostring(ret), "0")
	if (true) then
		local ret, error = pcall(function() uint64.divide(CS.LongStatic.ULONG_MAX, 0) end)
		ASSERT_EQ(ret, false)
    else
        ASSERT_EQ(true, false)
	end
end

function CMyTestCaseLuaCallCS.CaseFuncUint64compare(self)
    self.count = 1 + self.count
	local uint64Var = 1234567890
	local ret = uint64.compare(uint64Var, uint64Var)
	ASSERT_EQ(ret, 0)
	local ret = uint64.compare(CS.LongStatic.ULONG_MAX, uint64Var)
	ASSERT_EQ(ret, 1)
	local ret = uint64.compare(CS.LongStatic.LONG_MAX, CS.LongStatic.LONG_MAX)
	ASSERT_EQ(ret, 0)
	local ret = uint64.compare(CS.LongStatic.ULONG_MAX, -1)
	ASSERT_EQ(ret, 0)
	local ret = uint64.compare(0, CS.LongStatic.ULONG_MAX)
	ASSERT_EQ(ret, -1)
end

function CMyTestCaseLuaCallCS.CaseFuncUint64remainder(self)
    self.count = 1 + self.count
	local uint64Var = 1234567890
	local ret = uint64.remainder(uint64Var, uint64Var)
	ASSERT_EQ(tostring(ret), "0")
	local ret = uint64.remainder(CS.LongStatic.ULONG_MAX, uint64Var)
	ASSERT_EQ(tostring(ret), "834183465")
	local ret = uint64.remainder(CS.LongStatic.LONG_MAX, CS.LongStatic.LONG_MAX)
	ASSERT_EQ(tostring(ret), "0")
	local ret = uint64.remainder(CS.LongStatic.ULONG_MAX, -1)
	ASSERT_EQ(tostring(ret), "0")
	local ret = uint64.remainder(0, CS.LongStatic.ULONG_MAX)
	ASSERT_EQ(tostring(ret), "0")
	if (true) then
		local ret, error = pcall(function() uint64.remainder(CS.LongStatic.ULONG_MAX, 0) end)
		ASSERT_EQ(ret, false)
    else
        ASSERT_EQ(true, false)
	end
end

function CMyTestCaseLuaCallCS.CaseTableAutoTransSimpleClassMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClass()
	local ret = class:SimpleClassMethod({x=97, y="123", z=100000000000})
	ASSERT_EQ(ret.x, 97)
	ASSERT_EQ(ret.y, "123")
	ASSERT_EQ(tostring(ret.z), "100000000000")
end

function CMyTestCaseLuaCallCS.CaseTableAutoTransComplexClassMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClass()
	local ret = class:ComplexClassMethod({A=97, B={x=97, y="123", z=100000000000}})
	ASSERT_EQ(ret.IntVar, 97)
	ASSERT_EQ(ret.ClassVar.IntVar, 97)
	ASSERT_EQ(ret.ClassVar.StringVar, "123")
	ASSERT_EQ(tostring(ret.ClassVar.LongVar), "100000000000")
end

function CMyTestCaseLuaCallCS.CaseTableAutoTransSimpleStructMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClass()
	local ret = class:SimpleStructMethod({a=97})
	ASSERT_EQ(ret.a, 97)
end

function CMyTestCaseLuaCallCS.CaseTableAutoTransComplexStructMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClass()
	local ret = class:ComplexStructMethod({a=-101, b=1000, c=1.000000132, d={a=97}})
	ASSERT_EQ(ret.a, -101)
	ASSERT_EQ(ret.b, 1000)
	ASSERT_EQ(string.sub(tostring(ret.c), 1, 11), "1.000000132")
	ASSERT_EQ(ret.d.a, 97)
end

function CMyTestCaseLuaCallCS.CaseTableAutoTransOneListMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClass()
	local ret = class:OneListMethod({1, 2, 3, 4, 5})
	ASSERT_EQ(ret, 15)
end

function CMyTestCaseLuaCallCS.CaseTableAutoTransTwoDimensionListMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClass()
	local ret = class:TwoDimensionListMethod({{1, 2, 3},{4, 5, 6}})
	ASSERT_EQ(ret, 21)
end

function CMyTestCaseLuaCallCS.CaseTestImplicit(self)
	self.count = 1 + self.count
    if CS.LuaTestCommon.IsXLuaGeneral() then return end
	local ret = CS.LuaTestObj.TestImplicit():GetType()
	ASSERT_EQ(ret, typeof(CS.UnityEngine.LayerMask))
end

function CMyTestCaseLuaCallCS.CaseVariableParamFunc2_1_4(self)
    self.count = 1 + self.count
	local ret, err = pcall(function() CS.LuaTestObj.VariableParamFunc(0, CS.LuaTestObj()) end)
	ASSERT_EQ(ret, false)
	ASSERT_TRUE(err:find("invalid arguments"))
end

function CMyTestCaseLuaCallCS.CaseFirstPushEnum(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.FirstPushEnumFunc(1)
	ASSERT_EQ(ret, "1")
	local ret = CS.LuaTestObj.FirstPushEnumFunc(2)
	ASSERT_EQ(ret, "4")
end

function CMyTestCaseLuaCallCS.CaseReferTestClass(self)
	self.count = 1 + self.count
	local int_x = 10
	local int_y = 12
	local str_z = "abc"
	local class1, ret_y, ret_z = CS.ReferTestClass(int_x, int_y)
	local ret = class1:Get_X_Y_ADD()
	ASSERT_EQ(ret, 22)
	ASSERT_EQ(ret_y, 11)
	ASSERT_EQ(ret_z, "test1")

	local class3, ret_z = CS.ReferTestClass(int_x)
	local ret = class3:Get_X_Y_ADD()
	ASSERT_EQ(ret, 20)
	ASSERT_EQ(ret_z, "test3")
end

function CMyTestCaseLuaCallCS.CaseVariableParamFuncNoParam(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.VariableParamFunc2()
	ASSERT_EQ(ret, 0)
	local ret = CS.LuaTestObj.VariableParamFunc2("abc", "haha")
	ASSERT_EQ(ret, 2)
end