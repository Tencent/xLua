require("ltest.init")

-- for test case
CMyTestCaseGenCode = TestCase:new()
function CMyTestCaseGenCode:new(oo)
    local o = oo or {}
    o.count = 1
    
    setmetatable(o, self)
    self.__index = self
    return o
end

function CMyTestCaseGenCode.SetUpTestCase(self)
    self.count = 1 + self.count
	print("CMyTestCaseGenCode.SetUpTestCase")
end

function CMyTestCaseGenCode.TearDownTestCase(self)
    self.count = 1 + self.count
	print("CMyTestCaseGenCode.TearDownTestCase")
end


function CMyTestCaseGenCode.SetUp(self)
    self.count = 1 + self.count
	print("CMyTestCaseGenCode.SetUp")
end

function CMyTestCaseGenCode.TearDown(self)
    self.count = 1 + self.count
	print("CMyTestCaseGenCode.TearDown")
end


function CMyTestCaseGenCode.CaseAccess1(self)
    self.count = 1 + self.count
    if CS.LuaTestCommon.IsIOSPlatform() then
        return
    end
	local fileInfo = CS.System.IO.FileInfo("abc")
	ASSERT_EQ(false, fileInfo.Exists)
end

function CMyTestCaseGenCode.CaseAccess2(self)
    self.count = 1 + self.count
    if CS.LuaTestCommon.IsIOSPlatform() then
        return
    end
    if CS.LuaTestCommon.IsXLuaGeneral() then return end
	local resultPath = CS.LuaTestCommon.resultPath
	resultPath = resultPath.."ltest_case_list_co.txt"
	local fileInfo = CS.System.IO.FileInfo(resultPath)
	local access = CS.System.Security.AccessControl.AccessControlSections.None
	local ret, error = pcall(function() fileInfo:GetAccessControl(access) end)
	ASSERT_EQ(false, ret)
end