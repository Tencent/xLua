--[[
//////////////////////////////////////////////////////////////////////////
// date : 2013-5-4
// auth : macroli(idehong@gmail.com)
// ver  : 0.3
// desc : ltest - lua tester 
//////////////////////////////////////////////////////////////////////////    
--]]
------------------------------------------------------------------------------

--module(..., package.seeall)

VERSION = "0.51"

-- for test environment
TestEnvironment = {}
function TestEnvironment:new(oo)
    local o = oo or {}
    setmetatable(o, self)
    self.__index = self
    return o
end

function TestEnvironment.privateName(self)
	return "env_macro"	
end

function TestEnvironment.SetUp(self)	
end

function TestEnvironment.TearDown(self)	
end


-- for test case
TestCase = {}
function TestCase:new(oo)
    local o = oo or {}
    setmetatable(o, self)
    self.__index = self
    return o
end

function TestCase.privateName(self)
	return "case_macro"	
end

function TestCase.SetUpTestCase(self)	
end

function TestCase.TearDownTestCase(self)	
end

function TestCase.SetUp(self)	
end

function TestCase.TearDown(self)	
end

require("ltest.loutput")
require("ltest.lassert")


-- for assert
local _atER = true 						-- for event result
local _atStopLv = 2 					-- for stop level
local _atStopTip = "macro_error_stop"  	-- for stop level
local _atErrLv = 6 						-- for error level
local _atMgr = false

function ASSERT_EQ(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atER = _atMgr:AssertEQ(v1, v2):PCR(bNoP, _atErrLv, "ASSERT_EQ failed", v1, v2); return _atMgr
end

function ASSERT_LT(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atER = _atMgr:AssertLT(v1, v2):PCR(bNoP, _atErrLv, "ASSERT_LT failed", v1, v2); return _atMgr
end

function ASSERT_LE(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atER = _atMgr:AssertLE(v1, v2):PCR(bNoP, _atErrLv, "ASSERT_LE failed", v1, v2); return _atMgr
end

function ASSERT_GT(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atER = _atMgr:AssertGT(v1, v2):PCR(bNoP, _atErrLv, "ASSERT_GT failed", v1, v2); return _atMgr
end

function ASSERT_GE(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atER = _atMgr:AssertGE(v1, v2):PCR(bNoP, _atErrLv, "ASSERT_GE failed", v1, v2); return _atMgr
end

function ASSERT_NE(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atER = _atMgr:AssertNE(v1, v2):PCR(bNoP, _atErrLv, "ASSERT_NE failed", v1, v2); return _atMgr
end

function ASSERT_NEAR(v1, v2, nearValue, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atER = _atMgr:AssertNear(v1, v2, nearValue):PCR(bNoP, _atErrLv, "ASSERT_NEAR failed", v1, v2); return _atMgr
end

function ASSERT_TRUE(v1, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atER = _atMgr:AssertTrue(v1):PCR1(bNoP, _atErrLv, "ASSERT_TRUE failed", v1); return _atMgr
end

function ASSERT_FALSE(v1, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atER = _atMgr:AssertFalse(v1):PCR1(bNoP, _atErrLv, "ASSERT_FALSE failed", v1); return _atMgr
end


function EXCEPT_EQ(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atMgr:AssertEQ(v1, v2):PCR(bNoP, _atErrLv, "EXCEPT_EQ failed", v1, v2); return _atMgr
end

function EXCEPT_LT(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atMgr:AssertLT(v1, v2):PCR(bNoP, _atErrLv, "EXCEPT_LT failed", v1, v2); return _atMgr
end

function EXCEPT_LE(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atMgr:AssertLE(v1, v2):PCR(bNoP, _atErrLv, "EXCEPT_LE failed", v1, v2); return _atMgr
end

function EXCEPT_GT(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atMgr:AssertGT(v1, v2):PCR(bNoP, _atErrLv, "EXCEPT_GT failed", v1, v2); return _atMgr
end

function EXCEPT_GE(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atMgr:AssertGE(v1, v2):PCR(bNoP, _atErrLv, "EXCEPT_GE failed", v1, v2); return _atMgr
end

function EXCEPT_NE(v1, v2, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atMgr:AssertNE(v1, v2):PCR(bNoP, _atErrLv, "EXCEPT_NE failed", v1, v2); return _atMgr
end

function EXCEPT_NEAR(v1, v2, nearValue, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atMgr:AssertNear(v1, v2, nearValue):PCR(bNoP, _atErrLv, "EXCEPT_NEAR failed", v1, v2); return _atMgr
end

function EXCEPT_TRUE(v1, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atMgr:AssertTrue(v1):PCR1(bNoP, _atErrLv, "EXCEPT_TRUE failed", v1); return _atMgr
end

function EXCEPT_FALSE(v1, bNoP)
	if not _atMgr then return end
	if not _atER then error(_atStopTip, _atStopLv) end
	_atMgr:AssertFalse(v1):PCR1(bNoP, _atErrLv, "EXCEPT_FALSE failed", v1); return _atMgr
end


-- for test mgr
local TestMgr = {}
function TestMgr:new(oo)
    local o = {
    	data = {
	    	oOut = false, generalGlobalName="global",
	    	
	    	-- suite seq	    	
	    	generalSuiteNo=2,  
	    	
	    	-- global suite value
	    	global={ prop={name="global", generalNo=1,bSys=true, oCls=false, no=1, costTime=0, totalNum=0, okNum=0, failedNum=0,}, list={}},

			-- key for name, value = {prop={}, list={}}
	    	all_suite = {}, 
	    	
	    	-- filter case name
	    	notcase = {privateName=true, SetUpTestCase=true, TearDownTestCase=true, SetUp=true, TearDown=true, TestCase=true, new=new},
    	},
    	
    	-- user use para
    	para = {
    		ltest_list_tests=false,	 	-- List the names of all tests instead of running them.
    		ltest_repeat=false,			-- Run the tests repeatedly; use a negative count to repeat forever.
    		ltest_filter=false,			-- Run only the tests whose name matches

    		ltest_list_falied=false,	-- Generate an TXT report in the given directory or with the given file name
    		ltest_output=false,			-- Generate an XML report in the given directory or with the given file name
    		ltest_silence=false,		-- if true then silence output
    	},
    	
    	-- stat info
    	stat = {},
    }
    
    setmetatable(o, self)
    self.__index = self
    return o
end

function TestMgr.Init(self, tPara)	
	if tPara and tPara.ltest_list_tests then self.para.ltest_list_tests = tPara.ltest_list_tests end
	if tPara and tPara.ltest_repeat then self.para.ltest_repeat = tPara.ltest_repeat end
	if tPara and tPara.ltest_filter then self.para.ltest_filter = tPara.ltest_filter end
	if tPara and tPara.ltest_list_falied then self.para.ltest_list_falied = tPara.ltest_list_falied end
	if tPara and tPara.ltest_output then self.para.ltest_output = tPara.ltest_output end
	if tPara and tPara.ltest_silence then self.ltest_silence = tPara.ltest_silence end
	
	local tOutputOther = {}
	if self.para.ltest_list_tests then
		local oTmp = CAllCaseListOutPut:new({filename=self.para.ltest_list_tests})
		table.insert(tOutputOther, oTmp)
	end
	if self.para.ltest_list_falied then
		local oTmp = CFailedCaseListOutPut:new({filename=self.para.ltest_list_falied})
		table.insert(tOutputOther, oTmp)
	end
	
	self.data.oOutput = CmdTestOutPut:new({outer = tOutputOther, silence=self.ltest_silence})
	_atMgr = CAssertMgr:new({oOutput=self.data.oOutput})
	
	self.data.all_suite[self.data.generalGlobalName] = self.data.global
end

function TestMgr.Fini(self)	
	self.data.all_suite = {}
	self.data.generalSuiteNo = 2
	self.data.global={ prop={name="global", generalNo=1,bSys=true, oCls=false, no=1, costTime=0, totalNum=0, okNum=0, failedNum=0,}, list={}}
end

function TestMgr.AddSuite(self, strSuiteName, oCls, filterCaseName)	
	if not oCls or  TestCase:privateName() ~= oCls:privateName() or self.data.notcase[strSuiteName] then return false end
	local strFind = "Test"
	if filterCaseName and type(filterCaseName) == "string" then strFind = filterCaseName end
		
	local tSuite = self:addRealSuite(strSuiteName, oCls)
	if not tSuite then return false end
	
	local tCaseKey = {}
	for k, v in pairs(oCls) do
		if type(k) == "string" and "__" ~= string.sub(k, 1, 2) and type(v) == "function" and string.find(k, strFind) and not self.data.notcase[k] then
			table.insert(tCaseKey, k)
		end
	end
	for k, v in pairs(oCls.__index) do
		if type(k) == "string" and "__" ~= string.sub(k, 1, 2) and type(v) == "function" and string.find(k, strFind) and not self.data.notcase[k] then
			table.insert(tCaseKey, k)
		end
	end
	
	table.sort(tCaseKey)
	for k, v in pairs(tCaseKey) do
		self:addRealCase(tSuite, v, oCls[v], nil)
	end
	
	return true
end

function TestMgr.AddCase(self, strCaseName, oFun, tPara, strSuiteName, oCls)
	if type(oFun) ~= "function" or type(strCaseName) ~= type("") then return false end	
	
	local tSuite  = false
	if strSuiteName then 
		tSuite = self:addRealSuite(strSuiteName, oCls)
	else
		tSuite = self:addRealSuite(self.data.generalGlobalName, oCls)	
	end
	
	if tSuite then return self:addRealCase(tSuite, strCaseName, oFun, tPara) else return false	end
end

function TestMgr.Run(self, oEnv)
	local oOutput = self.data.oOutput
	if self.para.ltest_filter and type(self.para.ltest_filter) == type("") then 
		oOutput:FilterInfo("Note: ltest filter = %s\r\n", self.para.ltest_filter)
	end
		
	local iCount, iGroup, tSuiteCaseKey = self:countAllCase(self.para.ltest_filter)
	oOutput:BeginGroupSuite( iCount, iGroup )
	
	local bUseEnv = (oEnv and TestEnvironment:privateName() == oEnv:privateName())
	if bUseEnv then	 oEnv:SetUp(); oOutput:BeginGroupEnv() end

	local tFailedList = self:runAllSuite(oOutput, tSuiteCaseKey)
	
	if bUseEnv then	oEnv:TearDown(); oOutput:EndGroupEnv() end	
	
	local iTotalTime = 0
	for k, v in pairs(self.data.all_suite) do
		iTotalTime = iTotalTime + v.prop.costTime
	end

	oOutput:EndGroupSuite(iTotalTime)
	oOutput:StaticInfo()
	
	self.data.stat = oOutput:GetStaticInfo()
	self:Fini()
	return 0
end

function TestMgr.GetStatInfo(self)
	return self.data.stat
end

function TestMgr.countAllCase(self, strFilter)	
	-- parset filter
	local tFilter = {}
	local iPosStart, iPosEnd = 1, 1
	while strFilter do
		iPosEnd = string.find(strFilter, ":", iPosStart)
		if not iPosEnd then 
			table.insert(tFilter, string.sub(strFilter, iPosStart, #strFilter - 1) ) 		
			break
		end
		table.insert(tFilter, string.sub(strFilter, iPosStart, iPosEnd - 3) )  
		iPosStart = iPosEnd + 1
	end
	
	-- filter case
	local iCount, iGroup = 0, 0
	local tSuiteKey = {}
	local bUsed = true
	for k, v in pairs(self.data.all_suite) do
		local tCaseKey = {}
		for kk, vv in pairs(v.list) do
			if tFilter and #tFilter > 0 then
				bUsed = false
				for wk, kf in pairs(tFilter) do
					if bUsed then break end
					if k == self.data.generalGlobalName then
						for w in  string.gmatch(k .. "." .. vv.label, kf) do bUsed = true; break end
					else
						for w in  string.gmatch(vv.label, kf) do bUsed = true; break end
					end
				end
			end
			if bUsed then table.insert(tCaseKey, {name=kk, id=vv.no}) end
		end
		if #tCaseKey > 0 then 
			table.sort(tCaseKey, function(a, b) return a.id < b.id end)
			table.insert(tSuiteKey, {name=k, id=v.prop.no, list=tCaseKey}) 
			iCount = iCount + #tCaseKey
			iGroup = iGroup + 1
		end
	end
	table.sort(tSuiteKey, function(a, b) return a.id < b.id end)
	return iCount, iGroup, tSuiteKey
end	

function TestMgr.runAllSuite(self, oOutput, tSuiteCaseKey)	
	local tFailedList = {}		
	for k, v in ipairs(tSuiteCaseKey) do
		self:runGroupSuite(tFailedList, self.data.all_suite[v.name], v.list, oOutput)
	end
	return tFailedList
end


function TestMgr.runGroupSuite(self, tFailedList, tSuite, tCaseKey, oOutput)
	oOutput:BeginSuite( #tCaseKey, tSuite.prop.name )	
	if tSuite.prop.oCls then tSuite.prop.oCls:SetUpTestCase() end
	
	local bExecResult = false
	for k, v in ipairs(tCaseKey) do
		local oTmpCase = tSuite.list[v.name]		
		oTmpCase.result, oTmpCase.costTime = self:runCase(oTmpCase, tSuite, oOutput)
		tSuite.prop.costTime = oTmpCase.costTime + tSuite.prop.costTime
		
		if not oTmpCase.result then
			tSuite.prop.failedNum = 1 + tSuite.prop.failedNum
			table.insert(tFailedList, oTmpCase.label)	
		else
			tSuite.prop.okNum = 1 + tSuite.prop.okNum
		end
	end		

	if tSuite.prop.oCls then tSuite.prop.oCls:TearDownTestCase() end
	oOutput:EndSuite( #tCaseKey, tSuite.prop.name,  tSuite.prop.costTime, tSuite.prop.failedNum)
end



function TestMgr.runCase(self, oTmpCase, tSuite, oOutput)
	-- for assert step over
	_atER = true
	local bExecResult, bResult, strError = false, false, ""
	
	local iItemTime = os.clock()
	oOutput:BeginCase(oTmpCase.label)
				
	if tSuite.prop.oCls then 
		tSuite.prop.oCls:SetUp() 		
		if oTmpCase.para  and #oTmpCase.para > 0 then
			bResult, strError = pcall(function () oTmpCase.fun(tSuite.prop.oCls, unpack( oTmpCase.para) ) end)
		else
			bResult, strError = pcall(function () oTmpCase.fun(tSuite.prop.oCls) end)		
		end
	else	
		if oTmpCase.para and #oTmpCase.para > 0 then
			bResult, strError = pcall(function () oTmpCase.fun( unpack( oTmpCase.para) ) end)
		else
			bResult, strError = pcall(function () oTmpCase.fun() end)		
		end	
	end
	
	if not bResult or not _atMgr:GetResult() then 
		if strError and not (not _atER and string.find(strError, _atStopTip)) then oOutput:FailedTxt(strError) end
	else
		bExecResult = true
	end
	
	if tSuite.prop.oCls then tSuite.prop.oCls:TearDown() end		
	iItemTime = os.clock() - iItemTime	
	oOutput:EndCase(true, bExecResult, iItemTime, oTmpCase.label)

	return bExecResult, iItemTime
end


function TestMgr.addRealSuite(self, strSuiteName, cCls)	
	if oCls and (type(oCls) ~= type({}) or type(oCls.privateName) ~= type(print) or TestCase:privateName() ~= oCls:privateName()) then return end
	if not strSuiteName or type(strSuiteName) ~= type("") or self.data.notcase[strSuiteName] then return end	

	local tSuiteGroup = self.data.all_suite
	if not tSuiteGroup[strSuiteName] then  
		tSuiteGroup[strSuiteName] = {prop={name=strSuiteName, generalNo=1, bSys=false, oCls=cCls, no=self.data.generalSuiteNo, costTime=0, totalNum=0, okNum=0, failedNum=0,}, list={}} 
		self.data.generalSuiteNo = 1 + self.data.generalSuiteNo
	end
	return tSuiteGroup[strSuiteName] 
end
	
function TestMgr.addRealCase(self, tSuite, strCaseName, oFun, tPara)
	if strCaseName and self.data.notcase[strCaseName] then return false end
	
	local tSuiteProp = tSuite.prop
	local tCurrSuite = tSuite.list
	
	if tCurrSuite[strCaseName] then  return false end
	local strLabel = strCaseName
	if not tSuiteProp.bSys then strLabel = tSuiteProp.name .. "." .. strCaseName end
	tCurrSuite[strCaseName] = {fun=oFun, para=tPara, label=strLabel, result=true, costTime=0, no=tSuiteProp.generalNo,}
	tSuiteProp.generalNo = 1 + tSuiteProp.generalNo
	tSuiteProp.totalNum = 1 + tSuiteProp.totalNum
	return true
end


local _oTestMgr = false
--------------------------------------------------------------------------------
-- for init help function
function InitLTest(tPara)
	if not _oTestMgr then _oTestMgr = TestMgr:new() else _oTestMgr:Fini() end
	_oTestMgr:Init(tPara)
	return _oTestMgr
end

-- add all case of suite(oCls) by match ¡®filterCaseName¡¯
-- oCls 			: <table>,  for suite class, the mt must is ltest.TestCase
-- strSuiteName 	: <string>, for suite name
-- filterCaseName 	: <string>, math case name
-- return true if add success else return false
function AddLTestSuite(oCls, strSuiteName, filterCaseName)
	if not _oTestMgr then return false end
	return _oTestMgr:AddSuite(strSuiteName, oCls, filterCaseName)
end

-- add  a case of suite(oCls)
-- oFun 			: <function>,  for case
-- strCaseName 		: <string>, for case name
-- tPara 			: <table>, for oFun para
-- strSuiteName 	: <string>, for suite name
-- oCls 			: <table>,  for suite class, the mt must is ltest.TestCase
-- return true if add success else return false
function AddLTestCase(oFun, strCaseName, tPara, strSuiteName, oCls)
	if not _oTestMgr then return false end
	return _oTestMgr:AddCase(strCaseName, oFun, tPara, strSuiteName, oCls)
end


-- add  a group case of suite(oCls)
-- oFun 			: <function>,  for case
-- strCaseName 		: <string>, for case name
-- tGroupPara 		: <table>, for oFun para, like {{},{},...}
-- strSuiteName 	: <string>, for suite name
-- oCls 			: <table>,  for suite class, the mt must is ltest.TestCase
-- return true if add success else return false
function AddLTestGroupCase(oFun, strCaseName, tGroupPara, strSuiteName, oCls)
	if not _oTestMgr then return false end
	for i, v in ipairs(tGroupPara) do
		_oTestMgr:AddCase(strCaseName .. "/" .. i, oFun, v, strSuiteName, oCls)
	end
end


-- run all test case
-- oEnv : <function>,  the mt must is ltest.TestEnvironment
-- return 0 if success
function RunAllTests(oEnv)
	if not _oTestMgr then return false end
	return _oTestMgr:Run(oEnv)
end

-- return total_suite£¬total_case£¬ total_faild
function GetRunStatInfo()
	if not _oTestMgr then return end	
	return _oTestMgr:GetStatInfo()
end
