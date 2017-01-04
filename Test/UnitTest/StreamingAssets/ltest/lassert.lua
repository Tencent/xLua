--[[
//////////////////////////////////////////////////////////////////////////
// date : 2013-5-4
// auth : macroli(idehong@gmail.com)
// ver  : 0.3
// desc : assert and except 
//////////////////////////////////////////////////////////////////////////    
--]]
------------------------------------------------------------------------------

--module(..., package.seeall)

-- for assert base
local assert_mt_base = {}
assert_mt_base.__eq = function(v1, v2)
	return type(v1.data) == type(v2.data) and  v1.data == v2.data	
end

assert_mt_base.__lt = function (v1, v2)
	return false
end

assert_mt_base.__le = function (v1, v2)
	return false
end

assert_mt_base.__tostring = function (v1)
	return tostring(v1.data)
end

local CBaseAssert = {}
function CBaseAssert:new(oo)
    local o = oo or {}
    setmetatable(o, assert_mt_base)
    return o
end


local assert_mt_default = {}
assert_mt_default.__eq = function(v1, v2)
	return type(v1.data) == type(v2.data) and  v1.data == v2.data	
end

assert_mt_default.__lt = function (v1, v2)
	return type(v1.data) == type(v2.data) and  v1.data < v2.data
end

assert_mt_default.__le = function (v1, v2)
	return type(v1.data) == type(v2.data) and  v1.data <= v2.data
end

assert_mt_default.__tostring = function (v1)
	return tostring(v1.data)
end


local CNumberAssert = {}
function CNumberAssert:new(oo)
    local o = oo or {}
    setmetatable(o, assert_mt_default)
    return o
end


local CStringAssert = {}
function CStringAssert:new(oo)
    local o = oo or {}
    setmetatable(o, assert_mt_default)
    return o
end

local _strCmpField = ""

-- for less than, equal than, great than result
local _iTableLT, _iTableEQ, _iTableGT = -1, 0, 1

-- compare function for table
local function _cmpTable(left, right, iCurLevel, iMaxLevel)
	if iCurLevel >= iMaxLevel then return _iTableLT end
	
	local tmpTypeTab = type({})	
	if tmpTypeTab ~= type(left) or tmpTypeTab ~= type(right) then return _iTableLT end
	
	local tK1 = {}
	for k, v in pairs(left) do table.insert(tK1, k) end
	table.sort(tK1, function(a, b) return tostring(a) < tostring(b) end)	

	local tK2 = {}
	for k, v in pairs(right) do table.insert(tK2, k) end
	table.sort(tK2, function(a, b) return tostring(a) < tostring(b) end)	
	
	local iLen1, iLen2 = #tK1, #tK2
	local iLenMin = math.min(iLen1, iLen2)
	
	local tmp1, tmp2, tmpTyp1, tmpTyp2 = 0, 0, 0, 0
	local tmpTypeStr = type("")
	local tmpTypeInt = type(0)
	
	local iTmpResult = _iTableEQ
	for i = 1, iLenMin do
		tmp1 = left[ tK1[i] ]
		if tmp1 == nil then return _iTableLT end 
		
		tmp2 = right[ tK2[i] ]
		if tmp2 == nil then return _iTableGT end

		_strCmpField = string.format("level:%d, field:%s vs :%s", iCurLevel, tK1[i] , tK1[i] )
		
		tmpTyp1, tmpTyp2 = type(tmp1), type(tmp2)		
		if tmpTyp1 ~= tmpTyp2 then return _iTableLT end
		
		if tmpTyp1 == tmpTypeStr or tmpTyp1 == tmpTypeInt then
			if tmp1 < tmp2 then return _iTableLT  end
			if tmp1 > tmp2 then return _iTableGT end
		elseif tmpTyp1 == tmpTypeTab then
			iTmpResult = _cmpTable(tmp1, tmp2, iCurLevel + 1, iMaxLevel) 
			if iTmpResult ~= _iTableEQ then return iTmpResult end			
		else
			return _iTableLT 
		end			 
	end
	
	_strCmpField = string.format("level:%d, num: %d vs %d", iCurLevel, iLen1, iLen2)
	
	if iLen1 < iLen2 then return _iTableLT end
	if iLen1 > iLen2 then return _iTableGT end
	
	return _iTableEQ		
end


-- concat function for table
local function _strTable(left, strTip, iCurLevel, iMaxLevel)
	if iCurLevel >= iMaxLevel then return strTip end
	
	local tmpTypeTab = type({})	
	if tmpTypeTab ~= type(left) then return strTip end
	
	local tK1 = {}
	for k, v in pairs(left) do table.insert(tK1, k) end
	table.sort(tK1, function(a, b) return tostring(a) < tostring(b) end)	

	local iLen1 = #tK1
	local tmp1, tmpTyp1 = 0, 0	
	for i = 1, iLen1 do
		tmp1 = left[ tK1[i] ]
		tmpTyp1 = type(tmp1)
		if tmpTyp1 == tmpTypeTab then
			strTip = strTip .. _strTable(tmp1, strTip .. string.format("%s={", tostring(tK1[i]) ), iCurLevel + 1, iMaxLevel) .. "},"
		else
			strTip = strTip .. 	string.format("%s=%s,", tostring(tK1[i]), tostring(tmp1))
		end	 
	end
	
	return strTip		
end

-- for table recursion level number
local _iTableLevel = 5
local assert_mt_table = {}
assert_mt_table.__eq = function(v1, v2)
	local mt1 = getmetatable(v1.data)
	local mt2 = getmetatable(v1.data)

	if mt1 and mt2 and mt1["__eq"] and mt2["__eq"] then
		return v1.data == v2.data
	else
		return type(v1.data) == type(v2.data) and _cmpTable(v1.data, v2.data, 0, _iTableLevel) == _iTableEQ
	end
end

assert_mt_table.__lt = function (v1, v2)
	local mt1 = getmetatable(v1.data)
	local mt2 = getmetatable(v1.data)

	if mt1 and mt2 and mt1["__lt"] and mt2["__lt"] then
		return v1.data < v2.data
	else
		return type(v1.data) == type(v2.data) and _cmpTable(v1.data, v2.data, 0, _iTableLevel) == _iTableLT
	end
end

assert_mt_table.__le = function (v1, v2)
	local mt1 = getmetatable(v1.data)
	local mt2 = getmetatable(v1.data)

	if mt1 and mt2 and mt1["__le"] and mt2["__le"] then
		return v1.data <= v2.data
	else
		return type(v1.data) == type(v2.data) and _cmpTable(v1.data, v2.data, 0, _iTableLevel) <= _iTableEQ
	end
end

assert_mt_table.__tostring = function (v1)
	local mt1 = getmetatable(v1.data)

	if mt1 and mt1["__tostring"] then
		return tostring(v1.data)
	else
		return _strTable(v1.data, "{", 0, _iTableLevel) .. "}"
	end
end


local CTableAssert = {}
function CTableAssert:new(oo)
    local o = oo or {}
    setmetatable(o, assert_mt_table)
    return o
end


-- for test assert and except
CAssertMgr = {}
function CAssertMgr:new(oo)
    local o = oo or {}
    o.result = true
    o.at = {}
    
    o.at[tostring(type(nil)) 	  ] = {ld={}, rd={},}
    local oTmp = o.at[tostring(type(nil))]    
    oTmp.left  = CBaseAssert:new(oTmp.ld)
    oTmp.right = CBaseAssert:new(oTmp.rd)
	
    o.at[tostring(type(1)) 	  ] = {ld={}, rd={},}
    local oTmp = o.at[tostring(type(1))]    
    oTmp.left  = CNumberAssert:new(oTmp.ld)
    oTmp.right = CNumberAssert:new(oTmp.rd)

    o.at[tostring(type("")) 	  ] = {ld={}, rd={},}
    local oTmp = o.at[tostring(type(""))]    
    oTmp.left  = CStringAssert:new(oTmp.ld)
    oTmp.right = CStringAssert:new(oTmp.rd)
    
    o.at[tostring(type(true)) 	  ] = {ld={}, rd={},}
    local oTmp = o.at[tostring(type(true))]    
    oTmp.left  = CBaseAssert:new(oTmp.ld)
    oTmp.right = CBaseAssert:new(oTmp.rd)    
 
    o.at[tostring(type({})) 	  ] = {ld={}, rd={},}
    local oTmp = o.at[tostring(type({}))]    
    oTmp.left  = CTableAssert:new(oTmp.ld)
    oTmp.right = CTableAssert:new(oTmp.rd)    

    o.at[tostring(type(print)) 	  ] = {ld={}, rd={},}
    local oTmp = o.at[tostring(type(print))]    
    oTmp.left  = CBaseAssert:new(oTmp.ld)
    oTmp.right = CBaseAssert:new(oTmp.rd)    

    o.at["thread"] = {ld={}, rd={},}
    local oTmp = o.at["thread"]    
    oTmp.left  = CBaseAssert:new(oTmp.ld)
    oTmp.right = CBaseAssert:new(oTmp.rd)    

    o.at["userdata"] = {ld={}, rd={},}
    local oTmp = o.at["userdata"]    
    oTmp.left  = CBaseAssert:new(oTmp.ld)
    oTmp.right = CBaseAssert:new(oTmp.rd)    
   
    setmetatable(o, self)
    self.__index = self
    return o
end

function CAssertMgr.GetResult(self)
	return self.result
end

function CAssertMgr.helpWrapper(self, v1, v2)
	self.result = false
	_strCmpField = ""
	local tmp = self.at[type(v1)]
	if not tmp then return end
	tmp.ld.data = v1
	tmp.rd.data = v2
	return tmp.left, tmp.right, type(v1)
end

function CAssertMgr.CheckResult(self, lv, strTip)
	if not self:GetResult() then error(strTip, lv) end
end

function CAssertMgr.PCRTip(self, bNoPrintDetail, strTip, lv, v1, iParaNum)
	local tmp = self.at[type(v1)]
	l = tmp.left
	r = tmp.right
 
 	local strTmp = "" 
	while true do
		if type(v1) == type({}) and bNoPrintDetail then	
			strTmp = string.format("%s --> field=%s.", strTip, _strCmpField)
			break
		end
		
		if iParaNum > 1 then
			if _strCmpField and #_strCmpField > 0 then
				strTmp = string.format("%s --> field=%s, left:%s, right:%s.", strTip, _strCmpField, tostring(l), tostring(r))
			else
				strTmp = string.format("%s --> left:%s, right:%s.", strTip, tostring(l), tostring(r))
			end
		else
			if _strCmpField and #_strCmpField > 0 then
				strTmp = string.format("%s --> field=%s, value:%s.", strTip, _strCmpField, tostring(l))
			else
				strTmp = string.format("%s --> value:%s.", strTip, tostring(l))
			end
		end
		break
	end
	return strTmp	
end

-- PCR = PCallCheckResult1
function CAssertMgr.PCR1(self, bNoPrintDetail, lv, strTip, v1)
	local bRet = self:GetResult() 
	if not bRet then 
		local strTmp = self:PCRTip(bNoPrintDetail, strTip, lv,  v1, 1)
		local bResult, strError = pcall(function () self:CheckResult(lv, strTmp) end)
		if not bResult and strError then self:print("\t" .. strError) end
	end
	return bRet		
end

function CAssertMgr.PCR(self, bNoPrintDetail, lv, strTip, v1, v2)
	local bRet = self:GetResult() 
	if not bRet then 
		local strTmp = self:PCRTip(bNoPrintDetail, strTip, lv, v1, 2)		
		local bResult, strError = pcall(function () self:CheckResult(lv, strTmp) end)
		if not bResult and strError then self:print("\t" .. strError) end
	end	
	return bRet		
end

function CAssertMgr.print(self, ...)
	if not self:GetResult() then self.oOutput:FailedTxt(...) end
	return self
end


function CAssertMgr.AssertEQ(self, v1, v2)
	local l, r, t = self:helpWrapper(v1, v2)
	self.result = l == r
	return self
end

function CAssertMgr.AssertLT(self, v1, v2)
	local l, r, t = self:helpWrapper(v1, v2)
	self.result = l < r
	return self
end

function CAssertMgr.AssertLE(self, v1, v2)
	local l, r,t = self:helpWrapper(v1, v2)
	self.result = l <= r
	return self
end

function CAssertMgr.AssertGT(self, v1, v2)
	local l, r, t = self:helpWrapper(v1, v2)
	self.result = l > r
	return self	
end

function CAssertMgr.AssertGE(self, v1, v2)
	local l, r, t = self:helpWrapper(v1, v2)
	self.result = l >= r
	return self	
end

function CAssertMgr.AssertNE(self, v1, v2)
	self.result = false
	local l, r, t = self:helpWrapper(v1, v2)
	if type(v1) == type(v2) then 		
		self.result = l ~= r
	end
	return self	
end

function CAssertMgr.AssertNear(self, v1, v2, nearValue)
	self.result = false
	local l, r, t = self:helpWrapper(v1, v2)	
	if type(v1) == type(v2) and type(v1) == type(0.1) then
		if v1 > v2 then self.result =  (v1 - v2 < nearValue) end
		if v1 < v2 then self.result =  (v2 - v1 < nearValue) end
	end
	return self	
end

function CAssertMgr.AssertTrue(self, v1)
	local l, r, t = self:helpWrapper(v1, v1)
	if v1 then self.result = true else self.result = false end
	return self	
end

function CAssertMgr.AssertFalse(self, v1)
	self:AssertTrue(v1)
	self.result = not self.result 
	return self		
end
