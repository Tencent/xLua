require("ltest.init")

gTestNumber = 1
function islua53() return not not math.type end

-- for test case
CMyTestCaseLuaCallCSReflect = TestCase:new()
function CMyTestCaseLuaCallCSReflect:new(oo)
    local o = oo or {}
    o.count = 1
    
    setmetatable(o, self)
    self.__index = self
    return o
end

function CMyTestCaseLuaCallCSReflect.SetUpTestCase(self)
    self.count = 1 + self.count
	print("CMyTestCaseLuaCallCSReflect.SetUpTestCase")
end

function CMyTestCaseLuaCallCSReflect.TearDownTestCase(self)
    self.count = 1 + self.count
	print("CMyTestCaseLuaCallCSReflect.TearDownTestCase")
end


function CMyTestCaseLuaCallCSReflect.SetUp(self)
    self.count = 1 + self.count
	print("CMyTestCaseLuaCallCSReflect.SetUp")
end

function CMyTestCaseLuaCallCSReflect.TearDown(self)
    self.count = 1 + self.count

	print("CMyTestCaseLuaCallCSReflect.TearDown")
end


function CMyTestCaseLuaCallCSReflect.CaseDefaultParamFunc1(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.DefaultParaFuncSingle(100, "abc")
	ASSERT_EQ(ret, 100)
end

function CMyTestCaseLuaCallCSReflect.CaseDefaultParamFunc2(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.DefaultParaFuncSingle(100)
	ASSERT_EQ(ret, 100)
end

function CMyTestCaseLuaCallCSReflect.CaseDefaultParamFunc3(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.DefaultParaFuncMulti(100, "")
	ASSERT_EQ(ret, 101)
end

function CMyTestCaseLuaCallCSReflect.CaseDefaultParamFunc4(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.DefaultParaFuncMulti(100, "efg", 1)
	ASSERT_EQ(ret, 101)
end

function CMyTestCaseLuaCallCSReflect.CaseDefaultParamFunc5(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.DefaultParaFuncMulti(100, "efg", 1, 98)
	ASSERT_EQ(ret, 101)
end

function CMyTestCaseLuaCallCSReflect.CaseDefaultParamFunc6(self)
    self.count = 1 + self.count
	--if (CS.LuaTestCommon.IsMacPlatform() == false) then
    if (true) then
		local ret, error = pcall(function() CS.LuaTestObjReflect.DefaultParaFuncMulti(100, "efg", 1, 98, 0) end)
		ASSERT_EQ(ret, false)
    else
        ASSERT_EQ(true, false)
	end
end

function CMyTestCaseLuaCallCSReflect.CaseVariableParamFunc1(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.VariableParamFunc(0)
	ASSERT_EQ(ret, 0)
end

function CMyTestCaseLuaCallCSReflect.CaseVariableParamFunc2(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.VariableParamFunc(0, "a")
	ASSERT_EQ(ret, 0)
end

function CMyTestCaseLuaCallCSReflect.CaseVariableParamFunc3(self)
    self.count = 1 + self.count
	--if (CS.LuaTestCommon.IsMacPlatform() == false) then
    if (true) then
		local ret, error = pcall(function() CS.LuaTestObjReflect.VariableParamFunc(0, "a", 1, "b") end)
		ASSERT_EQ(ret, true)
    else
        ASSERT_EQ(true, false)
	end
end

function CMyTestCaseLuaCallCSReflect.CaseLuaAccessEnum1(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.TestEnumFunc(CS.LuaTestTypeReflect.DEF)
	ASSERT_EQ(ret, 1)
end
--[[ 2016.11.25 新版本不支持整数直接当枚举用
function CMyTestCaseLuaCallCSReflect.CaseLuaAccessEnum2(self)
    self.count = 1 + self.count
	local enumValue = -1
	local ret = CS.LuaTestObjReflect.TestEnumFunc(enumValue)
	ASSERT_EQ(ret, -1)
end
]]
function CMyTestCaseLuaCallCSReflect.CaseLuaAccessEnum3(self)
    self.count = 1 + self.count
	local enumValue = CS.LuaTestTypeReflect.__CastFrom(2)
	local ret = CS.LuaTestObjReflect.TestEnumFunc(enumValue)
	ASSERT_EQ(ret, 2)
end

function CMyTestCaseLuaCallCSReflect.CaseLuaAccessEnum4(self)
    self.count = 1 + self.count
	local enumValue = CS.LuaTestTypeReflect.__CastFrom(4)
	local ret = CS.LuaTestObjReflect.TestEnumFunc(enumValue)
	ASSERT_EQ(ret, 4)
end

function CMyTestCaseLuaCallCSReflect.CaseLuaAccessEnum5(self)
    self.count = 1 + self.count
	local enumValue = CS.LuaTestTypeReflect.__CastFrom("GHI")
	local ret = CS.LuaTestObjReflect.TestEnumFunc(enumValue)
	ASSERT_EQ(ret, 2)
end

function CMyTestCaseLuaCallCSReflect.CaseLuaAccessEnum6(self)
    self.count = 1 + self.count
	--if (CS.LuaTestCommon.IsMacPlatform() == false and CS.LuaTestCommon.IsIOSPlatform() == false) then
    if (true) then
		local ret, error = pcall(function() CS.LuaTestObjReflect.TestEnumFunc(CS.LuaTestTypeReflect.__CastFrom("BCD")) end)
		ASSERT_EQ(ret, false)
    else
        ASSERT_EQ(true, false)
	end
end

function CMyTestCaseLuaCallCSReflect.CaseGetType1(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.TestGetType(typeof(CS.LuaTestObjReflect))
	ASSERT_EQ(ret, "LuaTestObjReflect")
end

function CMyTestCaseLuaCallCSReflect.CaseGetType2(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.TestGetType(typeof(CS.System.String))
	ASSERT_EQ(ret, "System.String")
end

function CMyTestCaseLuaCallCSReflect.CaseGetType3(self)
    self.count = 1 + self.count
    if CS.LuaTestCommon.IsXLuaGeneral() then return end
	local ret = CS.LuaTestObjReflect.TestGetType(typeof(CS.UnityEngine.Vector3))
	ASSERT_EQ(ret, "UnityEngine.Vector3")
end

function CMyTestCaseLuaCallCSReflect.CaseGetType4(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.TestGetType(typeof(CS.System.Int16))
	ASSERT_EQ(ret, "System.Int16")
end

function CMyTestCaseLuaCallCSReflect.CaseGetType5(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.TestGetType(typeof(CS.XLua.LuaTable))
	ASSERT_EQ(ret, "XLua.LuaTable")
end

function CMyTestCaseLuaCallCSReflect.CaseGetType6(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.TestGetType(typeof(CS.XLua.LuaFunction))
	ASSERT_EQ(ret, "XLua.LuaFunction")
end

--[[ v2.1.0中已将LuaUserData类去掉
function CMyTestCaseLuaCallCSReflect.CaseGetType7(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.TestGetType(typeof(CS.XLua.LuaUserData))
	ASSERT_EQ(ret, "XLua.LuaUserData")
end
]]

function CMyTestCaseLuaCallCSReflect.Case64BitInt1(self)
    self.count = 1 + self.count
	CS.LuaTestObjReflect.Gen64BitInt()
	local x1 = CS.LuaTestObjReflect.ulX1
	local x2 = CS.LuaTestObjReflect.ulX2
	local ret = x1 + x2
	ASSERT_EQ("6917529027641081856", tostring(ret))
	ret  = x2 - x1
	--ASSERT_EQ("16140901064495857664", tostring(ret))
	ASSERT_EQ("-2305843009213693952", tostring(ret))
	x1 = CS.LuaTestObjReflect.lY1
	x2 = CS.LuaTestObjReflect.lY2
	ret = x1 + x2
	ASSERT_EQ("6917529027641081856", tostring(ret))
	ret = x2 - x1
	ASSERT_EQ("-2305843009213693952", tostring(ret))
	x1 = CS.LuaTestObjReflect.i64Z1
	x2 = CS.LuaTestObjReflect.i64Z2
	ret = x1 + x2
	ASSERT_EQ("6917529027641081856", tostring(ret))
	ret = x2 - x1
	ASSERT_EQ("-2305843009213693952", tostring(ret))
end

function CMyTestCaseLuaCallCSReflect.Case64BitInt2(self)
    self.count = 1 + self.count
	CS.LuaTestObjReflect.Gen64BitInt()
	local x1 = CS.LuaTestObjReflect.ulX1
	local x2 = CS.LuaTestObjReflect.ulX2
	ASSERT_EQ(true, x2 < x1)
	ASSERT_EQ(true, x2 <= x1)
	ASSERT_EQ(false, x1 <= x2)
	ASSERT_EQ(false, x1 < x2)
end

function CMyTestCaseLuaCallCSReflect.Case64BitInt3(self)
    self.count = 1 + self.count
	CS.LuaTestObjReflect.Gen64BitInt()
	local y = CS.LuaTestObjReflect.lY3
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

function CMyTestCaseLuaCallCSReflect.Case64BitInt4(self)
    self.count = 1 + self.count
	CS.LuaTestObjReflect.Gen64BitInt()
	local ret = CS.LuaTestObjReflect.lY3 * CS.LuaTestObjReflect.lY4
	ASSERT_EQ(tostring(ret), "138485688541642752")
	ret = CS.LuaTestObjReflect.lY3 / CS.LuaTestObjReflect.lY5
	if islua53() then
		ASSERT_EQ(tostring(ret), "91202908614.226")
	else
	    ASSERT_EQ(tostring(ret), "91202908614")
	end
	ret = CS.LuaTestObjReflect.lY3 % CS.LuaTestObjReflect.lY6
	ASSERT_EQ(tostring(ret), "52636")
end

function CMyTestCaseLuaCallCSReflect.CaseCast1(self)
    self.count = 1 + self.count
	local castObj = CS.TestCastClassReflect()
	cast(castObj, typeof(CS.TestCastClassReflect))
	ASSERT_EQ(true, castObj:TestFunc1())
end

function CMyTestCaseLuaCallCSReflect.CaseCast2(self)
	self.count = 1 + self.count
	local castObj = CS.LuaTestObjReflect.CreateTestLuaObj()
	cast(castObj, typeof(CS.TestLuaClassReflect))
	ASSERT_EQ(true, castObj:TestFunc1())
end

function LuaFunc1(x)
	return x + 1
end

function LuaFunc2(x)
	return x * 2
end	

function CMyTestCaseLuaCallCSReflect.CaseDelgate1(self)
	self.count = 1 + self.count
	
	CS.LuaTestObjReflect.initNumber = 5
	CS.LuaTestObjReflect.GenDelegate()
	local luaDelgateLink = CS.LuaTestObjReflect.csDelegate
	luaDelgateLink = CS.LuaTestObjReflect.csDelegate1
	luaDelgateLink = luaDelgateLink + CS.LuaTestObjReflect.csDelegate2
	luaDelgateLink = luaDelgateLink + CS.LuaTestObjReflect.csDelegate3
	local ret = luaDelgateLink(1)
	ASSERT_EQ(5, ret)
	luaDelgateLink = luaDelgateLink + LuaFunc1
	ret = luaDelgateLink(1)
	ASSERT_EQ(2, ret)
	CS.LuaTestObjReflect.csDelegate4 = LuaFunc2
	luaDelgateLink = luaDelgateLink + CS.LuaTestObjReflect.csDelegate4
	ret = luaDelgateLink(2)
	ASSERT_EQ(4, ret)
	luaDelgateLink = luaDelgateLink - LuaFunc1
	ret = luaDelgateLink(10)
	ASSERT_EQ(20 ,ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate4
	ret = luaDelgateLink(19)
	ASSERT_EQ(1900, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate3
	ret = luaDelgateLink(2)
	ASSERT_EQ(1900, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate2
	ret = luaDelgateLink(3)
	ASSERT_EQ(1903, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate2
	ret = luaDelgateLink(4)
	ASSERT_EQ(1907, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate1
    local ret, error = pcall(function() luaDelgateLink(5) end)
    ASSERT_EQ(false, ret)

	CS.LuaTestObjReflect.initNumber = 5
	luaDelgateLink = CS.LuaTestObjReflect.csDelegate3
	ret = luaDelgateLink(1)
	ASSERT_EQ(5, ret)
	luaDelgateLink = luaDelgateLink + CS.LuaTestObjReflect.csDelegate1
	ret = luaDelgateLink(1)
	ASSERT_EQ(6, ret)
	luaDelgateLink = luaDelgateLink + CS.LuaTestObjReflect.csDelegate1
	ret = luaDelgateLink(1)
	ASSERT_EQ(8, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate1
	ret = luaDelgateLink(1)
	ASSERT_EQ(9, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate1
	ret = luaDelgateLink(1)
	ASSERT_EQ(9, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate3
	CS.LuaTestObjReflect.initNumber = 1
	local function LuaFunc4(x)
		CS.LuaTestObjReflect.initNumber = CS.LuaTestObjReflect.initNumber * 2
		return CS.LuaTestObjReflect.initNumber
	end
	luaDelgateLink = CS.LuaTestObjReflect.csDelegate1
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
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate1
	
	CS.LuaTestObjReflect.initNumber = 1907
	luaDelgateLink = LuaFunc1
	ret = luaDelgateLink(1)
	ASSERT_EQ(2, ret)	
	luaDelgateLink = luaDelgateLink + CS.LuaTestObjReflect.csDelegate1
	ret = luaDelgateLink(2)
	ASSERT_EQ(1909, ret)
	luaDelgateLink = luaDelgateLink + CS.LuaTestObjReflect.csDelegate4
	ret = luaDelgateLink(3)
	ASSERT_EQ(6, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate1
	ret = luaDelgateLink(4)
	ASSERT_EQ(8, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate1
	ret = luaDelgateLink(5)
	ASSERT_EQ(10, ret)
	luaDelgateLink = luaDelgateLink - LuaFunc1
	ret = luaDelgateLink(6)
	ASSERT_EQ(12, ret)
	luaDelgateLink = luaDelgateLink - LuaFunc1
	ret = luaDelgateLink(7)
	ASSERT_EQ(14, ret)
	luaDelgateLink = luaDelgateLink - CS.LuaTestObjReflect.csDelegate4
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

function CMyTestCaseLuaCallCSReflect.CaseEvent1(self)
	self.count = 1 + self.count

	gTestNumber = 1
	local testObj = CS.LuaTestObjReflect()
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

function CMyTestCaseLuaCallCSReflect.CaseCalc1(self)
	self.count = 1 + self.count
	local a = CS.LuaTestObjReflect()
	a.testVar = 100
	local b = CS.LuaTestObjReflect()
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
	local c = CS.LuaTestObjReflect()
	c.testArr[0] = 1000
	ret = c[0]
	ASSERT_EQ(1000, ret)
	c[1] = 10000
	ret = c.testArr[1]
	ASSERT_EQ(10000, ret)
	
	a.testVar = 100
	b.testVar = 200
	c.testVar = 300
	local d = CS.LuaTestObjReflect()
	d.testVar = 20
	local e = CS.LuaTestObjReflect()
	e.testVar = 7
	ret = (a + b) * c / d % e
	ASSERT_EQ(ret.testVar, 6)
end

function CMyTestCaseLuaCallCSReflect.CaseCalc2(self)
	self.count = 1 + self.count
	local a = CS.LuaTestObjReflect()
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

function CMyTestCaseLuaCallCSReflect.CaseOverLoad1(self)
	self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.OverLoad1(1, 2)
	ASSERT_EQ(ret, 1)
	ret = CS.LuaTestObjReflect.OverLoad2("1", "2")
	ASSERT_EQ(ret, 4);
	ret = CS.LuaTestObjReflect.OverLoad3(1)
	ASSERT_EQ(ret, 5);
end

function CMyTestCaseLuaCallCSReflect.CaseOutRef1(self)
	self.count = 1 + self.count
	local a = 2
	local b
	a, b = CS.LuaTestObjReflect.OutRefFunc1(1, a, b)
	ASSERT_EQ(a, 100)
	b, a = CS.LuaTestObjReflect.OutRefFunc2(b, 1, a)
	ASSERT_EQ(a, 200)
	a, b = CS.LuaTestObjReflect.OutRefFunc3(1, a, b)
	ASSERT_EQ(a, 300)
	ret, a, b = CS.LuaTestObjReflect.OutRefFunc4(1, a, b)
	ASSERT_EQ(a, 400)
	ASSERT_EQ(ret, 400)
	ret, b, a = CS.LuaTestObjReflect.OutRefFunc5(b, 1, a)
	ASSERT_EQ(a, 500)
	ASSERT_EQ(ret, 500)
	ret = CS.LuaTestObjReflect.OutRefFunc6(1, 2)
	ASSERT_EQ(ret, 600)
end

function CMyTestCaseLuaCallCSReflect.CaseOutRef2(self)
	self.count = 1 + self.count
	local a = CS.LuaTestObjReflect.CreateTestLuaObj()
	local b = CS.LuaTestObjReflect.CreateTestLuaObj()
	local c = CS.LuaTestObjReflect.CreateTestLuaObj()
	a, b = CS.LuaTestObjReflect.OutRefFunc11(c, a, b)
	ASSERT_EQ(a.cmpTarget, 100)
	b, a = CS.LuaTestObjReflect.OutRefFunc12(b, c, a)
	ASSERT_EQ(a.cmpTarget, 200)
	a, b = CS.LuaTestObjReflect.OutRefFunc13(c, a, b)
	ASSERT_EQ(a.cmpTarget, 300)
	ret, a, b = CS.LuaTestObjReflect.OutRefFunc14(c, a, b)
	ASSERT_EQ(a.cmpTarget, 400)
	ASSERT_EQ(ret, 400)
	ret, b, a = CS.LuaTestObjReflect.OutRefFunc15(b, c, a)
	ASSERT_EQ(a.cmpTarget, 500)
	ASSERT_EQ(ret, 500)
	ret = CS.LuaTestObjReflect.OutRefFunc16(a, b)
	ASSERT_EQ(ret, 600)
end

function CMyTestCaseLuaCallCSReflect.CaseOutRef3(self)
	self.count = 1 + self.count
	local a = CS.LuaTestObjReflect.csDelegate11
	local b = CS.LuaTestObjReflect.csDelegate12
	local c = CS.LuaTestObjReflect.csDelegate13
	CS.LuaTestObjReflect.initNumber = 1
	
	a, b = CS.LuaTestObjReflect.OutRefFunc21(c, a, b)
	ASSERT_EQ(a(1), 2)
	b, a = CS.LuaTestObjReflect.OutRefFunc22(b, c, a)
	ASSERT_EQ(a(1), 3)
	a, b = CS.LuaTestObjReflect.OutRefFunc23(c, a, b)
	ASSERT_EQ(a(1), 4)
	ret, a, b = CS.LuaTestObjReflect.OutRefFunc24(c, a, b)
	ASSERT_EQ(a(1), 6)
	ASSERT_EQ(ret, 5)
	ret, b, a = CS.LuaTestObjReflect.OutRefFunc25(b, c, a)
	ASSERT_EQ(a(1), 8)
	ASSERT_EQ(ret, 7)
	ret = CS.LuaTestObjReflect.OutRefFunc26(a, b)
	ASSERT_EQ(ret, 600)
end

function CMyTestCaseLuaCallCSReflect.CaseNewClass1And5(self)
	self.count = 1 + self.count
	local class = CS.NoContClass()
	local ret = class:add(1,2)
	ASSERT_EQ(ret, 3)	
end

function CMyTestCaseLuaCallCSReflect.CaseNewClass2(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.OneParamContClass(2)
	local ret = class:add(1,2)
	ASSERT_EQ(ret, 3)	
	ASSERT_EQ(class.key1, 2)
	ASSERT_EQ(class.key2, 1)
end

function CMyTestCaseLuaCallCSReflect.CaseNewClass3(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.TwoParamsContClass(2, 3)
	local ret = class:add(1,2)
	ASSERT_EQ(ret, 3)	
	ASSERT_EQ(class.key1, 2)
	ASSERT_EQ(class.key2, 3)
end

function CMyTestCaseLuaCallCSReflect.CaseNewClass4(self)
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

function CMyTestCaseLuaCallCSReflect.CaseNewClass6(self)
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

function CMyTestCaseLuaCallCSReflect.CaseNewClass7(self)
	self.count = 1 + self.count
	CS.testLuaCallCS.StaticTestClass.n = 0
	CS.testLuaCallCS.StaticTestClass:Add()
	ASSERT_EQ(CS.testLuaCallCS.StaticTestClass.n, 1)
end


function CMyTestCaseLuaCallCSReflect.CaseVisitStaticMemFunc_1(self)
	self.count = 1 + self.count
	CS.testLuaCallCS.MultiContClass.d = 3
	local ret = CS.testLuaCallCS.MultiContClass.d
	ASSERT_EQ(ret, 3)
	ret = CS.testLuaCallCS.MultiContClass.dec(10, 1)
	ASSERT_EQ(ret, 9)
end

function CMyTestCaseLuaCallCSReflect.CaseVisitStaticMemFunc_2(self)
	self.count = 1 + self.count
	CS.NoContClass.d = 4
	local ret = CS.NoContClass.d
	ASSERT_EQ(ret, 4)
	ret = CS.NoContClass.dec(10, 1)
	ASSERT_EQ(ret, 9)
end

function CMyTestCaseLuaCallCSReflect.CaseVisitStaticMemFunc_3(self)
	self.count = 1 + self.count
	CS.testLuaCallCS.StaticTestClass.n = 2
	CS.testLuaCallCS.StaticTestClass:Add()
	ASSERT_EQ(CS.testLuaCallCS.StaticTestClass.n, 3)
end

function CMyTestCaseLuaCallCSReflect.CaseVisitClassMemFunc_1(self)
	self.count = 1 + self.count
	local class = CS.NoContClass()
	local ret = class:add(1,2)
	ASSERT_EQ(ret, 3)
	class.key3 = false;
	ASSERT_EQ(class.key3, false)
end

function CMyTestCaseLuaCallCSReflect.CaseVisitClassMemFunc_2(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.OneParamContClass(2)
	local ret = class:add(1,2)
	ASSERT_EQ(ret, 3)	
	class.key1 = 3
	class.key2 = 4
	ASSERT_EQ(class.key1, 3)
	ASSERT_EQ(class.key2, 4)
end

function CMyTestCaseLuaCallCSReflect.CaseVisitFatherClassMemFunc_1_1(self)
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

function CMyTestCaseLuaCallCSReflect.CaseVisitFatherClassMemFunc_1_2(self)
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

function CMyTestCaseLuaCallCSReflect.CaseVisitFatherClassMemFunc_2(self)
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

function CMyTestCaseLuaCallCSReflect.CaseVisitFatherClassMemFunc_3(self)
	self.count = 1 + self.count
	local class = CS.testLuaCallCS.ChildCalss()

	local ret = class:add(10, 1)
	ASSERT_EQ(ret, 11)
	
	class.a = 3
	ASSERT_EQ(class:getA() , 3)
	class:setA(4)
	ASSERT_EQ(class:getA() , 4)
	
end

function CMyTestCaseLuaCallCSReflect.CaseGetChineseString(self)
	self.count = 1 + self.count
	local class = CS.TestChineseStringReflect()

	local ret = class:GetShortChinString()
	ASSERT_EQ(ret, "中文字符串")
	
	ASSERT_EQ(class:GetLongChineString() , "为Unity3D增加Lua脚本编程的能力，进而提供代码逻辑增量更新的可能，支持lua的所有基本类型，哈哈哈哈")
	ASSERT_EQ(class:GetCombineString() , "中文字符串.* ? [ ] ^ $~`!@#$%^&()_-+=[];',““〈〉〖【℃ ＄ ¤№ ☆ ★■ⅷ②㈣12345abc")
	ASSERT_EQ(class:GetComplexString() , "繁體國陸")
	ASSERT_EQ(class:GetHuoxingString() , "吙煋呅僦媞這樣孒")
	
end

function CMyTestCaseLuaCallCSReflect.CaseVisitUlong(self)
	self.count = 1 + self.count
	local class = CS.TestUlongAndLongTypeReflect()

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

function CMyTestCaseLuaCallCSReflect.CaseVisitlong(self)
	self.count = 1 + self.count
	local class = CS.TestUlongAndLongTypeReflect()

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

function CMyTestCaseLuaCallCSReflect.CaseVisitExtensionMethodForClass(self)
	self.count = 1 + self.count
    local class = CS.TestChineseStringReflect()
    class:PrintAllString()
	
	local length = class:GetLongStringLength()
	ASSERT_EQ(54, length)
	
	length = class:Add(class)
	ASSERT_EQ(108, length)
	local class_a = CS.TestChineseStringReflect()
	class_a.short_simple_string = "啊"
	ASSERT_EQ(class_a:GetShortChinString() , "啊")
	
	local class_d = CS.TestChineseStringReflect()
	class_d.short_simple_string = "ha"
	local ret1
	local ret2
	ret1, ret2 = class:Replace(class_d, class_a)
	ASSERT_EQ(ret1:GetShortChinString(), "中文字符串")
	ASSERT_EQ(ret2:GetCombineString(), "中文字符串")
	ASSERT_EQ(ret2:GetShortChinString(), "中文字符串ha")
end

function CMyTestCaseLuaCallCSReflect.CaseVisitExtensionMethodForStruct(self)
	self.count = 1 + self.count
    local employ_1 = CS.EmployeestructReflect()
	employ_1.Name = "HONGFANG"
	employ_1.Age = 30
	employ_1.Salary = 12000
	employ_1.AnnualBonus = 30000
    employ_1:PrintSalary()
	
	local income = employ_1:GetIncomeForOneYear()
	ASSERT_EQ(income, 174000)
	
	local employ_2 = CS.EmployeestructReflect()
	employ_2.Name = "xiaojun"
	employ_2.Age =25
	employ_2.Salary = 5000
	employ_2.AnnualBonus = 10000
    employ_2:PrintSalary()
	
	local cost = employ_1:Add(employ_2)
	ASSERT_EQ(cost, 244000)	
	
	local employ_3 = CS.EmployeestructReflect()
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

function CMyTestCaseLuaCallCSReflect.CaseVisitStruct_21(self)
    --21.A继承B，B继承C， A,C生成代码，B不生成代码--->B也需要生成代码，或者A,B,C全部不生成代码
	self.count = 1 + self.count
	
	--22.A的实例访问B，C的struct类型属性
	local class_a = CS.AClassReflect(1, 2, "haha")
	ASSERT_EQ(class_a.BConStruct.x, 1)
	ASSERT_EQ(class_a.BConStruct.y, 2)
	ASSERT_EQ(class_a.BConStruct.z, "haha")
	ASSERT_EQ(class_a.CConStruct.x, 1)
	ASSERT_EQ(class_a.CConStruct.y, 2)
	ASSERT_EQ(class_a.CConStruct.z, "haha")
	
	--23.A的实例调用B的输入，输出，输入输出为struct类型的方法
	local struct_1 = CS.HasConstructStructReflect(2, 3, "TEST")
	ASSERT_EQ(struct_1.x, 2)
	ASSERT_EQ(struct_1.y, 3)
	ASSERT_EQ(struct_1.z, "TEST")
	local struct_2 = CS.HasConstructStructReflect(10, 1, "heihei")
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

function CMyTestCaseLuaCallCSReflect.CaseVisitStructVaribleParam(self)
    --可变函数参数为struct
	self.count = 1 + self.count
	
	local struct_1 = CS.HasConstructStructReflect(2, 3, "TEST")
	local struct_2 = CS.HasConstructStructReflect(10, 1, "heihei")
	local class_c = CS.CClassReflect(1, 2, "haha")
	local ret = class_c:VariableParamFunc(struct_1, struct_2)
	ASSERT_EQ(ret, 12)
end

function CMyTestCaseLuaCallCSReflect.CaseEventStatic(self)
	self.count = 1 + self.count

	gTestNumber = 1
	CS.LuaTestObjReflect.TestStaticEvent1('+', EvtFunc11)
	local ret = CS.LuaTestObjReflect.CallStaticEvent(1)
	ASSERT_EQ(2, ret)
	CS.LuaTestObjReflect.TestStaticEvent1('+', EvtFunc12)
	local ret = CS.LuaTestObjReflect.CallStaticEvent(2)
	ASSERT_EQ(7, ret)
	CS.LuaTestObjReflect.TestStaticEvent1('+', EvtFunc12)
	local ret = CS.LuaTestObjReflect.CallStaticEvent(1)
	ASSERT_EQ(12, ret)
	CS.LuaTestObjReflect.TestStaticEvent1('-', EvtFunc12)
	local ret = CS.LuaTestObjReflect.CallStaticEvent(1)
	ASSERT_EQ(15, ret)
	CS.LuaTestObjReflect.TestStaticEvent1('-', EvtFunc12)
	local ret = CS.LuaTestObjReflect.CallStaticEvent(1)
	ASSERT_EQ(16, ret)
	CS.LuaTestObjReflect.TestStaticEvent1('-', EvtFunc12)
	local ret = CS.LuaTestObjReflect.CallStaticEvent(1)
	ASSERT_EQ(17, ret)
	CS.LuaTestObjReflect.TestStaticEvent1('-', EvtFunc11)
end

function CMyTestCaseLuaCallCSReflect.CaseUpLowerMethod(self)
	self.count = 1 + self.count
	CS.LuaTestObjReflect.initNumber = 5
	local ret = CS.LuaTestObjReflect.CalcAdd(1)
	ASSERT_EQ(6, ret)
	
	local ret = CS.LuaTestObjReflect.calcadd(2)
	ASSERT_EQ(8, ret)
end

function CMyTestCaseLuaCallCSReflect.CaseUpLowerMethod(self)
	self.count = 1 + self.count

	local ret = CS.LuaTestObjReflect.OverLoad1(1, 2, 3, 5)
	ASSERT_EQ(11, ret)
end

function CMyTestCaseLuaCallCSReflect.CaseGetStaticSum(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObj.Sum(1, 2)
	ASSERT_EQ(ret, 3)
end

function CMyTestCaseLuaCallCSReflect.CaseGetSum(self)
    self.count = 1 + self.count
	local class = CS.LuaTestObj()
	local ret = class:Sum(1, 2, 6)
	ASSERT_EQ(ret, 9)
end


function CMyTestCaseLuaCallCSReflect.CaseVisitTemplateMethod(self)
    self.count = 1 + self.count
	ASSERT_EQ(CS.EmployeeTemplateReflect.GetBasicSalary, nil)
	ASSERT_EQ(CS.EmployeeTemplateReflect.AddBonus, nil)
	local class = CS.ManagerReflect()
	ASSERT_EQ(1, class:GetBasicSalary())
end

function CMyTestCaseLuaCallCSReflect.CaseVisitGenericMethod(self)
	self.count = 1 + self.count
	local class = CS.LuaTestObjReflect()
	local a = "abc"
	if (true) then
		local ret, error = pcall(function() class:GenericMethod(a) end)
		ASSERT_EQ(ret, false)
    else
        ASSERT_EQ(true, false)
	end
end

function CMyTestCaseLuaCallCSReflect.CaseVisitIntPtr(self)
    self.count = 1 + self.count
	local class = CS.LuaTestObjReflect()
	local ptr = class.ptr
	print(ptr)
	local ptr1 = class:GetPtr()
    print(ptr1)
    local bytevar = class:PrintPtr(ptr1)
	ASSERT_EQ(bytevar, 97)
end


function CMyTestCaseLuaCallCSReflect.CaseVisitVarAndDefaultFunc1(self)
    self.count = 1 + self.count
	local class = CS.LuaTestObjReflect()

	local ret = class:VariableParamFuncDefault(1)
	ASSERT_EQ(ret, 2)
	
	local ret = CS.LuaTestObjReflect.StaticVariableParamFuncDefault(1.0)
    ASSERT_EQ(ret, 2.0)
end

function CMyTestCaseLuaCallCSReflect.CaseVisitVarAndDefaultFunc2(self)
    self.count = 1 + self.count
	local class = CS.LuaTestObjReflect()

	local ret = class:VariableParamFuncDefault(1, 2, "john", "che")
	ASSERT_EQ(ret, 3)
	
	local ret = CS.LuaTestObjReflect.StaticVariableParamFuncDefault(1.0, 2.0, "john", "che")
	ASSERT_EQ(ret, 3.0)
end

function CMyTestCaseLuaCallCSReflect.CaseFuncReturnByteArray(self)
    self.count = 1 + self.count

	local ret = CS.LuaTestObjReflect.FuncReturnByteArray()
	ASSERT_EQ(ret, "abc")
end

function CMyTestCaseLuaCallCSReflect.CaseFuncReturnByte(self)
    self.count = 1 + self.count

	local ret = CS.LuaTestObjReflect.FuncReturnByte()
	ASSERT_EQ(ret, 97)
end

function CMyTestCaseLuaCallCSReflect.CaseFuncReturnIntArray(self)
    self.count = 1 + self.count

	local ret = CS.LuaTestObjReflect.FuncReturnIntArray()
	ASSERT_EQ(type(ret), "userdata")
end

function CMyTestCaseLuaCallCSReflect.CaseFuncReturnInt(self)
    self.count = 1 + self.count
	
	local ret = CS.LuaTestObjReflect.FuncReturnInt()
	ASSERT_EQ(ret, 97)
end

function CMyTestCaseLuaCallCSReflect.CaseTableAutoTransSimpleClassMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClassReflect()
	local ret = class:SimpleClassMethod({x=97, y="123", z=100000000000})
	ASSERT_EQ(ret.x, 97)
	ASSERT_EQ(ret.y, "123")
	ASSERT_EQ(tostring(ret.z), "100000000000")
end

function CMyTestCaseLuaCallCSReflect.CaseTableAutoTransComplexClassMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClassReflect()
	local ret = class:ComplexClassMethod({A=97, B={x=97, y="123", z=100000000000}})
	ASSERT_EQ(ret.IntVar, 97)
	ASSERT_EQ(ret.ClassVar.IntVar, 97)
	ASSERT_EQ(ret.ClassVar.StringVar, "123")
	ASSERT_EQ(tostring(ret.ClassVar.LongVar), "100000000000")
end

function CMyTestCaseLuaCallCSReflect.CaseTableAutoTransSimpleStructMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClassReflect()
	local ret = class:SimpleStructMethod({a=97})
	ASSERT_EQ(ret.a, 97)
end

function CMyTestCaseLuaCallCSReflect.CaseTableAutoTransComplexStructMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClassReflect()
	local ret = class:ComplexStructMethod({a=-101, b=1000, c=1.000000132, d={a=97}})
	ASSERT_EQ(ret.a, -101)
	ASSERT_EQ(ret.b, 1000)
	ASSERT_EQ(string.sub(tostring(ret.c), 1, 11), "1.000000132")
	ASSERT_EQ(ret.d.a, 97)
end

function CMyTestCaseLuaCallCSReflect.CaseTableAutoTransOneListMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClassReflect()
	local ret = class:OneListMethod({1, 2, 3, 4, 5})
	ASSERT_EQ(ret, 15)
end

function CMyTestCaseLuaCallCSReflect.CaseTableAutoTransTwoDimensionListMethod(self)
    self.count = 1 + self.count
	local class = CS.TestTableAutoTransClassReflect()
	local ret = class:TwoDimensionListMethod({{1, 2, 3},{4, 5, 6}})
	ASSERT_EQ(ret, 21)
end

function CMyTestCaseLuaCallCSReflect.CaseTestImplicit(self)
	self.count = 1 + self.count
    if CS.LuaTestCommon.IsXLuaGeneral() then return end
	local ret = CS.LuaTestObjReflect.TestImplicit():GetType()
	ASSERT_EQ(ret, typeof(CS.UnityEngine.LayerMask))
end

function CMyTestCaseLuaCallCSReflect.CaseVariableParamFunc2_1_4(self)
    self.count = 1 + self.count
	local ret, err = pcall(function() CS.LuaTestObjReflect.VariableParamFunc(0, CS.LuaTestObjReflect()) end)
	ASSERT_EQ(ret, false)
	ASSERT_TRUE(err:find("invalid arguments"))
end

function CMyTestCaseLuaCallCSReflect.CaseFirstPushEnum(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.FirstPushEnumFunc(1)
	ASSERT_EQ(ret, "1")
	local ret = CS.LuaTestObjReflect.FirstPushEnumFunc(2)
	ASSERT_EQ(ret, "4")
end

function CMyTestCaseLuaCallCSReflect.CaseReferTestClass(self)
	self.count = 1 + self.count
	local int_x = 10
	local int_y = 12
	local str_z = "abc"
	local class1, ret_y, ret_z = CS.ReferTestClassReflect(int_x, int_y)
	local ret = class1:Get_X_Y_ADD()
	ASSERT_EQ(ret, 22)
	ASSERT_EQ(ret_y, 11)
	ASSERT_EQ(ret_z, "test1")
	
	local class3, ret_z = CS.ReferTestClassReflect(int_x)
	local ret = class3:Get_X_Y_ADD()
	ASSERT_EQ(ret, 20)
	ASSERT_EQ(ret_z, "test3")
end

function CMyTestCaseLuaCallCSReflect.CaseVariableParamFuncNoParam(self)
    self.count = 1 + self.count
	local ret = CS.LuaTestObjReflect.VariableParamFunc2()
	ASSERT_EQ(ret, 0)
	local ret = CS.LuaTestObjReflect.VariableParamFunc2("abc", "haha")
	ASSERT_EQ(ret, 2)
end