
require("luaCallCs") 
require("luaCallCsReflect") 
require("csCallLua")
require("genCode")
--require("luaTdrTest")
function islua53() return not not math.type end
-- for test case
CMyTestEnv = TestEnvironment:new()
function CMyTestEnv:new(oo)
    local o = oo or {}
    setmetatable(o, self)
    self.__index = self
    return o
end

function CMyTestEnv.SetUp(self)
	print("CMyTestEnv.SetUp")
end

function CMyTestEnv.TearDown(self)
	print("CMyTestEnv.TearDown")
end

co = coroutine.create(function()
	local resultPath = CS.LuaTestCommon.resultPath
	local tInitPara = {
		--ltest_filter = "CMyTestCaseLuaCallCS.*:CMyTestCase3.*",
		ltest_list_tests = resultPath.."ltest_case_list_co.txt",
		ltest_list_falied = resultPath.."ltest_case_failed_co.txt",
	}
	InitLTest(tInitPara)

	AddLTestSuite(CMyTestCaseLuaCallCS:new(), "CMyTestCaseLuaCallCS", "Case")
	AddLTestSuite(CMyTestCaseLuaCallCSReflect:new(), "CMyTestCaseLuaCallCSReflect", "Case")
	--AddLTestSuite(CMyTestCaseGenCode:new(), "CMyTestCaseGenCode", "Case")
	AddLTestSuite(CMyTestCaseCSCallLua:new(), "CMyTestCaseCSCallLua", "test")
	--AddLTestSuite(CMyTestCaseLuaTdr:new(), "CMyTestCaseLuaTdr", "Case")
	
	RunAllTests(CMyTestEnv:new())

	local t = GetRunStatInfo()
	--print(t.iFailedNum)
	coroutine.yield()
end)

function main()


    print(coroutine.resume(co));
	
	local resultPath = CS.LuaTestCommon.resultPath
	local tInitPara = {
		--ltest_filter = "CMyTestCaseLuaCallCS.*:CMyTestCase3.*",
		ltest_list_tests = resultPath.."ltest_case_list.txt",
		ltest_list_falied = resultPath.."ltest_case_failed.txt",
	}
	InitLTest(tInitPara)

	AddLTestSuite(CMyTestCaseLuaCallCS:new(), "CMyTestCaseLuaCallCS", "Case")
	AddLTestSuite(CMyTestCaseLuaCallCSReflect:new(), "CMyTestCaseLuaCallCSReflect", "Case")
	--AddLTestSuite(CMyTestCaseGenCode:new(), "CMyTestCaseGenCode", "Case")
	AddLTestSuite(CMyTestCaseCSCallLua:new(), "CMyTestCaseCSCallLua", "test")
	--AddLTestSuite(CMyTestCaseLuaTdr:new(), "CMyTestCaseLuaTdr", "Case")
	RunAllTests(CMyTestEnv:new())
	
	print('--------------------------------------------------------')

	local t = GetRunStatInfo()
	--print(t.iFailedNum)

end   

main()

local ret = islua53()
print("islua53")
print(tostring(ret))