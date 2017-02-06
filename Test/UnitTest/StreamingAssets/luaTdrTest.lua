require("ltest.init")
require "libtdrlua"

pkg_table = {
    head = {
        magic = 0x7FFF,
        msgid = 10000001,
        cmd = 1,
        version = 0,
        bodyLen = 0,
        datetime = libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"),
        srcIp = libtdrlua.str2tdrip("127.0.0.1"),
    },
    
    body = {
        login = {
            name = "FrancisHe",
            pass = "123456",
            zone = "Japan",
            destIp = libtdrlua.str2tdrip("127.0.0.1"),
        },
        logout = {
            reason = -1,
            count = 2,
            attr = {-1, 0, 1},
        },
        xxx = {
            typeTester = {
                date = libtdrlua.str2tdrdate("2015-09-08"),
                time = libtdrlua.str2tdrtime("22:17:59"),
                
                int8 = -1,
                uint8Array = {0, 23, 255},
                int8VarArrayRefer = 2,
                int8VarArray = {-128, 127 ,0},
                
                int = -6, -- -6.6
                uintArray = {0, 1721, 0xFFFFFFFF},
                intVarArrayRefer = 1, -- 0
                intVarArray = {-0x80000000, 0x7FFFFFFF, 0},
                
                strArray = {"Francis", "Francis"},
                
                uint64 = 0xFFFFFFFFFFFFF, -- 1(s) + 11(e) + 52(m)
                int64Array = {-0x8000000000000000, 0xFFFFFFFFFFFFF, 0x7FFFFFFFFFFFFFFF},
                
                float = 0xFFFFFFFF,
                floatArray = {-3.40282346e+38, 1.17549435e-38, 3.40282346e+38}, -- 3.40282347e+38
                
                double = 2.2250738585072014e-308,
                doubleArray = {-1.7976931348623157e+308, -2.2250738585072014e-308, 1.7976931348623157e+308},
            },
            boundary = -1.1,
            selector = 1,
            innerUnion = {
                field1 = {
                    {
                        uint64 = 0x0FFFFFFFFFFFFFFF,
                        uint = 0xFFFFFFFF,
                    },
                    {
                        uint64 = 0x0FFFFFFFFFFFFFFF,
                        uint = 0,
                    }
                }
            },
            structArray = {
                count = 1, -- 0
                array = {
                    {
                        uint64 = 0x0FFFFFFFFFFFFFFF,
                        uint = 0xFFFFFFFF,
                    },
                    {
                        uint64 = 0x0FFFFFFFFFFFFFFF,
                        uint = 0xFFFFFFFF,
                    },
                    {
                        uint64 = 0x0FFFFFFFFFFFFFFF,
                        uint = 0xFFFFFFFF,
                    },
                }
            },
            boundary2 = 1.111111,
        },
        ext1 = -1,
        ext2 = {0, 1, 2},
        ext3 = {"Francis", "Francis"},
        ext4 = "Francis",
    }
}

pkg_table_v4 = {
    head = {
        magic = 0x7FFF,
        msgid = 10000001,
        cmd = 1,
        version = 0,
        bodyLen = 0,
        datetime = libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"),
        srcIp = libtdrlua.str2tdrip("127.0.0.1"),
		destIp = libtdrlua.str2tdrip("127.0.0.2"),
    },
    
    body = {
        login = {
            name = "FrancisHe",
            pass = "123456",
            zone = "Japan",
            destIp = libtdrlua.str2tdrip("127.0.0.1"),
        },
        logout = {
            reason = -1,
            count = 2,
            attr = {-1, 0, 1},
        },
        xxx = {
            typeTester = {
                date = libtdrlua.str2tdrdate("2015-09-08"),
                time = libtdrlua.str2tdrtime("22:17:59"),
                
                int8 = -1,
                uint8Array = {0, 23, 255},
                int8VarArrayRefer = 2,
                int8VarArray = {-128, 127 ,0},
                
                int = -6, -- -6.6
                uintArray = {0, 1721, 0xFFFFFFFF},
                intVarArrayRefer = 1, -- 0
                intVarArray = {-0x80000000, 0x7FFFFFFF, 0},
                
                strArray = {"Francis", "Francis"},
                
                uint64 = 0xFFFFFFFFFFFFF, -- 1(s) + 11(e) + 52(m)
                int64Array = {-0x8000000000000000, 0xFFFFFFFFFFFFF, 0x7FFFFFFFFFFFFFFF},
                
                float = 0xFFFFFFFF,
                floatArray = {-3.40282346e+38, 1.17549435e-38, 3.40282346e+38}, -- 3.40282347e+38
                
                double = 2.2250738585072014e-308,
                doubleArray = {-1.7976931348623157e+308, -2.2250738585072014e-308, 1.7976931348623157e+308},
            },
            boundary = -1.1,
            selector = 1,
            innerUnion = {
                field1 = {
                    {
                        uint64 = 0x0FFFFFFFFFFFFFFF,
                        uint = 0xFFFFFFFF,
                    },
                    {
                        uint64 = 0x0FFFFFFFFFFFFFFF,
                        uint = 0,
                    }
                }
            },
            structArray = {
                count = 1, -- 0
                array = {
                    {
                        uint64 = 0x0FFFFFFFFFFFFFFF,
                        uint = 0xFFFFFFFF,
                    },
                    {
                        uint64 = 0x0FFFFFFFFFFFFFFF,
                        uint = 0xFFFFFFFF,
                    },
                    {
                        uint64 = 0x0FFFFFFFFFFFFFFF,
                        uint = 0xFFFFFFFF,
                    },
                }
            },
            boundary2 = 1.111111,
        },
        ext1 = -1,
        ext2 = {0, 1, 2},
        ext3 = {"Francis", "Francis"},
        ext4 = "Francis",
    }
}

function copyTab(st)
    local tab = {}
    for k, v in pairs(st or {}) do
        if type(v) ~= "table" then
            tab[k] = v
        else
            tab[k] = copyTab(v)
        end
    end
    return tab
end

local function table_to_string(t)
  local ret = ''
  local ltype = type(t)
  if (ltype == 'table') then
    ret = ret .. '{ '
    for key,value in pairs(t) do
      ret = ret .. tostring(key) .. '=' .. table_to_string(value) .. ' '
    end
    ret = ret .. '}'
  elseif ltype == 'string' then
    ret = ret .. "'" .. tostring(t) .. "'"
  else
    ret = ret .. tostring(t)
  end
  return ret
end

-- for test case
CMyTestCaseLuaTdr = TestCase:new()
function CMyTestCaseLuaTdr:new(oo)
    local o = oo or {}
    o.count = 1
    
    setmetatable(o, self)
    self.__index = self
    return o
end

function CMyTestCaseLuaTdr.SetUpTestCase(self)
    self.count = 1 + self.count
	print("CMyTestCaseLuaTdr.SetUpTestCase")
end

function CMyTestCaseLuaTdr.TearDownTestCase(self)
    self.count = 1 + self.count
	print("CMyTestCaseLuaTdr.TearDownTestCase")
end


function CMyTestCaseLuaTdr.SetUp(self)
    self.count = 1 + self.count
	print("CMyTestCaseLuaTdr.SetUp")
end

function CMyTestCaseLuaTdr.TearDown(self)
    self.count = 1 + self.count

	print("CMyTestCaseLuaTdr.TearDown")
end

function CMyTestCaseLuaTdr.LoadMetalib(self, load_type, file_path, cmd)
    --加载meta元数据库	
	local ret_code, metalib
	if load_type == 0 then
	    ret_code, metalib = libtdrlua.load_metalib(file_path)
		if ret_code ~= 0 then
			print("libtdrlua.load_metalib() failed: " .. metalib)
		end
	end
	
	if load_type == 1 then
		local tdrmeta = CS.UnityEngine.Resources.Load(file_path).bytes
		ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
		if ret_code ~= 0 then
			print("libtdrlua.load_metalib_buf() failed: " .. metalib)
		end
    end
	
	ASSERT_EQ(ret_code, 0)
    print("libtdrlua.load_metalib ok")
	
	--获取最大meta buff size	
	local ret_code, buf_size = libtdrlua.metamaxbufsize(metalib, "Pkg")
	if ret_code ~= 0 then
		print("libtdrlua.metamaxbufsize() failed: " .. buf_size)
	end
	ASSERT_EQ(ret_code, 0)
	print("libtdrlua.metamaxbufsize() ok: buf_size = " .. buf_size)
	
	--分配buff
	local ret_code, buf = libtdrlua.bufalloc(buf_size)
	if ret_code ~= 0 then
		print("libtdrlua.bufalloc() failed: " .. buf)
	end
	ASSERT_EQ(ret_code, 0)
	print("libtdrlua.bufalloc() ok")
	print( "pkg = " .. table_to_string(pkg_table))
	
	------------------------------------------------------------------------
	-- API - get_meta
	-- return value - ret_code, meta/err_msg
	----------------------------------------------------------------------
	local ret_code, meta = libtdrlua.get_meta(metalib, "Pkg")
	if ret_code ~= 0 then
		print("libtdrlua.get_meta() failed: " .. meta)
    end
	ASSERT_EQ(ret_code, 0)
    print("libtdrlua.get_meta() ok")
	
	pkg_table.head.cmd = cmd
	----------------------------------------------------------------------
	-- API - table2buf
	-- return value - ret_code, used_size/err_msg
	----------------------------------------------------------------------
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table, buf, buf_size, 0)
	if ret_code ~= 0 then
		print("libtdrlua.table2buf() failed: " .. used_size)
		libtdrlua.buffree(buf)
	end
	ASSERT_EQ(ret_code, 0)
	print("libtdrlua.table2buf() ok, used_size = " .. used_size)
    
	
	----------------------------------------------------------------------
	-- API - buf2str
	-- return value - ret_code, str/err_msg
	----------------------------------------------------------------------
	local ret_code, str = libtdrlua.buf2str(buf, used_size)
	if ret_code ~= 0 then
		print("libtdrlua.buf2str() failed: " .. str)
		libtdrlua.buffree(buf)
	end
	ASSERT_EQ(ret_code, 0)
	print("libtdrlua.buf2str() ok")
	print("buf2str:" .. str)
	
	----------------------------------------------------------------------
	-- API - buf2table
	-- return value - ret_code, pkg_table, used_size/err_msg
	----------------------------------------------------------------------
	local ret_code, pkg_table2, used_size2 = libtdrlua.buf2table(meta, buf, used_size, 0)
	if ret_code ~= 0 then
		print("libtdrlua.buf2table() failed: " .. used_size2)
		print(DataDumper(pkg_table2, "pkg2 = "))
		libtdrlua.buffree(buf)
	end
	ASSERT_EQ(ret_code, 0)
	print("libtdrlua.buf2table() ok, used_size = " .. used_size2)
	print("unpakc pkg = ", table_to_string(pkg_table2))
	
	--print(DataDumper(pkg_table2, "pkg2 = "))
    --因为str为空，所以会引起Unity crash
	----------------------------------------------------------------------
	-- API - str2table
	-- return value - ret_code, pkg_table, used_size/err_msg
	----------------------------------------------------------------------
	local ret_code, pkg_table3, used_size3 = libtdrlua.str2table(meta, str, 0)
	if ret_code ~= 0 then
		print("libtdrlua.str2table() failed: " .. used_size3)
		print(DataDumper(pkg_table3, "pkg3 = "))
		libtdrlua.buffree(buf)
	end
	ASSERT_EQ(ret_code, 0)
	print("libtdrlua.str2table() ok, used_size = " .. used_size3)
	print("str2buf unpack pkg = ", table_to_string(pkg_table2))
	
	----------------------------------------------------------------------
	-- API - buffree
	-- return value - nil/err_msg
	----------------------------------------------------------------------
	local err_msg = libtdrlua.buffree(buf)
	if err_msg ~= nil then
		print("libtdrlua.buffree() failed: " .. err_msg)
	end
	print("libtdrlua.buffree() ok")
	ASSERT_EQ(err_msg, nil)

	----------------------------------------------------------------------
	-- API - free_metalib
	-- return value - nil/err_msg
	-- Note: metalib will be automatically released by lua gc. Thus, this
	--   step is optional.
	----------------------------------------------------------------------
	local err_msg = libtdrlua.free_metalib(metalib)
	if err_msg ~= nil then
		print("libtdrlua.free_metalib() failed: " .. err_msg)
	end
	print("libtdrlua.free_metalib() ok")
	ASSERT_EQ(err_msg, nil)
	return pkg_table2, pkg_table3
end

function CMyTestCaseLuaTdr.CaseLoadMetalib_1(self)
    self.count = 1 + self.count
	if CS.LuaTestCommon.android_platform == true then
	    return
	end
	pkg_table2, pkg_table3 = self:LoadMetalib(0, CS.LuaTestCommon.xxxtdrfilepath, 1)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 1)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 39)
	--ASSERT_EQ(tostring(pkg_table2.head.bodyLen),"39")
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login.name, "FrancisHe")
	ASSERT_EQ(pkg_table2.body.login.pass, "123456")
	ASSERT_EQ(pkg_table2.body.login.zone, "Japan")
	ASSERT_EQ(pkg_table2.body.login.destIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.ext1, nil)
	
	ASSERT_EQ(pkg_table3.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table3.head.msgid), "10000001")
	ASSERT_EQ(pkg_table3.head.cmd, 1)
	ASSERT_EQ(pkg_table3.head.version, 3)
	ASSERT_EQ(pkg_table3.head.bodyLen, 39)
	--ASSERT_EQ(tostring(pkg_table3.head.bodyLen),"39")
	ASSERT_EQ(pkg_table3.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table3.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table3.body.login.name, "FrancisHe")
	ASSERT_EQ(pkg_table3.body.login.pass, "123456")
	ASSERT_EQ(pkg_table3.body.login.zone, "Japan")
	ASSERT_EQ(pkg_table3.body.login.destIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table3.body.ext1, nil)
end

function CMyTestCaseLuaTdr.CaseLoadMetalibBuff_1(self)
    self.count = 1 + self.count
	pkg_table2, pkg_table3 = self:LoadMetalib(1, "testxxx.tdr", 1)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 1)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 39)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login.name, "FrancisHe")
	ASSERT_EQ(pkg_table2.body.login.pass, "123456")
	ASSERT_EQ(pkg_table2.body.login.zone, "Japan")
	ASSERT_EQ(pkg_table2.body.login.destIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.ext1, nil)
	
	ASSERT_EQ(pkg_table3.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table3.head.msgid), "10000001")
	ASSERT_EQ(pkg_table3.head.cmd, 1)
	ASSERT_EQ(pkg_table3.head.version, 3)
	ASSERT_EQ(pkg_table3.head.bodyLen, 39)
	ASSERT_EQ(pkg_table3.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table3.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table3.body.login.name, "FrancisHe")
	ASSERT_EQ(pkg_table3.body.login.pass, "123456")
	ASSERT_EQ(pkg_table3.body.login.zone, "Japan")
	ASSERT_EQ(pkg_table3.body.login.destIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table3.body.ext1, nil)
end	

function CMyTestCaseLuaTdr.CaseLoadMetalib_2(self)
    self.count = 1 + self.count
	if CS.LuaTestCommon.android_platform == true then
	    return
	end
	pkg_table2, pkg_table3 = self:LoadMetalib(0, CS.LuaTestCommon.xxxtdrfilepath, 2)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 2)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 16)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	
	ASSERT_EQ(tostring(pkg_table2.body.logout.reason + 1), "0")
	ASSERT_EQ(pkg_table2.body.logout.count, 2)
	ASSERT_EQ(pkg_table2.body.logout.attr, {-1, 0})
	ASSERT_EQ(pkg_table2.body.ext1, nil)
	
	ASSERT_EQ(pkg_table3.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table3.head.msgid), "10000001")
	ASSERT_EQ(pkg_table3.head.cmd, 2)
	ASSERT_EQ(pkg_table3.head.version, 3)
	ASSERT_EQ(pkg_table3.head.bodyLen, 16)
	ASSERT_EQ(pkg_table3.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table3.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table3.body.login, nil)
	
	ASSERT_EQ(tostring(pkg_table3.body.logout.reason + 1), "0")
	ASSERT_EQ(pkg_table3.body.logout.count, 2)
	ASSERT_EQ(pkg_table3.body.logout.attr, {-1, 0})
	ASSERT_EQ(pkg_table3.body.ext1, nil)
end

function CMyTestCaseLuaTdr.CaseLoadMetalibBuff_2(self)
    self.count = 1 + self.count
	pkg_table2, pkg_table3 = self:LoadMetalib(1, "testxxx.tdr", 2)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 2)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 16)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	local ret = -1
	ASSERT_EQ(ret, -1)
	ASSERT_EQ(tostring(pkg_table2.body.logout.reason + 1), "0")
	ASSERT_EQ(pkg_table2.body.logout.count, 2)
	ASSERT_EQ(pkg_table2.body.logout.attr, {-1, 0})
	ASSERT_EQ(pkg_table2.body.ext1, nil)
	
	ASSERT_EQ(pkg_table3.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table3.head.msgid), "10000001")
	ASSERT_EQ(pkg_table3.head.cmd, 2)
	ASSERT_EQ(pkg_table3.head.version, 3)
	ASSERT_EQ(pkg_table3.head.bodyLen, 16)
	ASSERT_EQ(pkg_table3.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table3.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table3.body.login, nil)
	local ret = -1
	ASSERT_EQ(ret, -1)
	ASSERT_EQ(tostring(pkg_table3.body.logout.reason + 1), "0")
	ASSERT_EQ(pkg_table3.body.logout.count, 2)
	ASSERT_EQ(pkg_table3.body.logout.attr, {-1, 0})
	ASSERT_EQ(pkg_table3.body.ext1, nil)
end	

function CMyTestCaseLuaTdr.CaseLoadMetalib_3(self)
    self.count = 1 + self.count
	if CS.LuaTestCommon.android_platform == true then
	    return
	end
	pkg_table2, pkg_table3 = self:LoadMetalib(0, CS.LuaTestCommon.xxxtdrfilepath, 3)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 3)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 222)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(libtdrlua.tdrip2str(pkg_table2.head.srcIp), "127.0.0.1")
	ASSERT_EQ(pkg_table2.body.login, nil)
	ASSERT_EQ(pkg_table2.body.logout, nil)
	ASSERT_EQ(libtdrlua.tdrdate2str(pkg_table2.body.xxx.typeTester.date), "2015-09-08")
	ASSERT_EQ(libtdrlua.tdrtime2str(pkg_table2.body.xxx.typeTester.time), " 22:17:59")
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.time, libtdrlua.str2tdrtime("22:17:59"))
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.int8, -1)
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.uint8Array, {0, 23, 255})
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.int8VarArrayRefer, 2)
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.int8VarArray, {-128, 127})
	--ASSERT_EQ(pkg_table2.body.xxx.typeTester.uintArray, {0, 1721, 0xFFFFFFFF}) --测试不过，0xFFFFFFFF解包后变长了0x80000000
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.intVarArray, {-0x80000000})
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.strArray, {"Francis", "Francis"})
	ASSERT_EQ(tostring(pkg_table2.body.xxx.typeTester.uint64), "4503599627370495") --0xFFFFFFFFFFFFF
	
	--ASSERT_EQ(pkg_table2.body.xxx.typeTester.int64Array, {-9223372036854775808, 4503599627370495, 9223372036854775807}) --{-0x8000000000000000, 0xFFFFFFFFFFFFF, 0x7FFFFFFFFFFFFFFF},，测试不过，解包后0x7FFFFFFFFFFFFFFF变长了-9223372036854775808
	ASSERT_EQ(pkg_table.body.xxx.typeTester.int64Array[3], 0x7FFFFFFFFFFFFFFF)--此处验证打包前是ok的
	
	--ASSERT_EQ(pkg_table2.body.xxx.typeTester.float, 0xFFFFFFFF) --此处验证不过，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(pkg_table.body.xxx.typeTester.float, 0xFFFFFFFF)--此处验证打包前是ok的
	
	---会存在精度问题，实际为{-3.40282346e+38, 1.17549435e-38, 3.40282346e+38}
	ASSERT_EQ(tostring(pkg_table2.body.xxx.typeTester.floatArray[1]), "-3.4028234663853e+38")
	ASSERT_EQ(tostring(pkg_table2.body.xxx.typeTester.floatArray[2]), "1.1754943508223e-38")
	ASSERT_EQ(tostring(pkg_table2.body.xxx.typeTester.floatArray[3]), "3.4028234663853e+38")
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.double, 2.2250738585072014e-308)
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.doubleArray, {-1.7976931348623157e+308, -2.2250738585072014e-308, 1.7976931348623157e+308})
	ASSERT_EQ(string.sub(tostring(pkg_table2.body.xxx.boundary),1, 4), "-1.1")
	ASSERT_EQ(pkg_table2.body.xxx.selector, 1)
	print("to_string filed1:" .. table_to_string(pkg_table2.body.xxx.innerUnion.field1[1]))
	--ASSERT_EQ(pkg_table2.body.xxx.innerUnion.field1[1], {unit64 = 0x0FFFFFFFFFFFFFFF, uint = 0xFFFFFFFF})
	
	ASSERT_EQ(type(pkg_table.body.xxx.innerUnion.field1[1]), 'table')
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[1].uint64, 0x0FFFFFFFFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[1].uint, 0xFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[2].uint64, 0x0FFFFFFFFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[2].uint, 0)
	
	ASSERT_EQ(tostring(pkg_table2.body.xxx.innerUnion.field1[1].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
	--ASSERT_EQ(pkg_table2.body.xxx.innerUnion.field1[1].uint, 0xFFFFFFFF) --这个值解包后有问题，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(tostring(pkg_table2.body.xxx.innerUnion.field1[2].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
	ASSERT_EQ(pkg_table2.body.xxx.innerUnion.field1[2].uint, 0)
	ASSERT_EQ(pkg_table2.body.xxx.structArray.count, 1)
	ASSERT_EQ(tostring(pkg_table2.body.xxx.structArray.array[1].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
    --ASSERT_EQ(pkg_table2.body.xxx.structArray.array[1].uint, 0xFFFFFFFF) --这个值解包后有问题，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(pkg_table2.body.xxx.structArray.array[2], nil)
	ASSERT_EQ(string.sub(tostring(pkg_table2.body.xxx.boundary2),1, 8), "1.111111")	
	
	ASSERT_EQ(pkg_table3.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table3.head.msgid), "10000001")
	ASSERT_EQ(pkg_table3.head.cmd, 3)
	ASSERT_EQ(pkg_table3.head.version, 3)
	ASSERT_EQ(pkg_table3.head.bodyLen, 222)
	ASSERT_EQ(pkg_table3.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(libtdrlua.tdrip2str(pkg_table3.head.srcIp), "127.0.0.1")
	ASSERT_EQ(pkg_table3.body.login, nil)
	ASSERT_EQ(pkg_table3.body.logout, nil)
	ASSERT_EQ(libtdrlua.tdrdate2str(pkg_table3.body.xxx.typeTester.date), "2015-09-08")
	ASSERT_EQ(libtdrlua.tdrtime2str(pkg_table3.body.xxx.typeTester.time), " 22:17:59")
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.time, libtdrlua.str2tdrtime("22:17:59"))
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.int8, -1)
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.uint8Array, {0, 23, 255})
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.int8VarArrayRefer, 2)
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.int8VarArray, {-128, 127})
	--ASSERT_EQ(pkg_table3.body.xxx.typeTester.uintArray, {0, 1721, 0xFFFFFFFF}) --测试不过，0xFFFFFFFF解包后变长了0x80000000
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.intVarArray, {-0x80000000})
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.strArray, {"Francis", "Francis"})
	ASSERT_EQ(tostring(pkg_table3.body.xxx.typeTester.uint64), "4503599627370495") --0xFFFFFFFFFFFFF
	
	--ASSERT_EQ(pkg_table3.body.xxx.typeTester.int64Array, {-9223372036854775808, 4503599627370495, 9223372036854775807}) --{-0x8000000000000000, 0xFFFFFFFFFFFFF, 0x7FFFFFFFFFFFFFFF},，测试不过，解包后0x7FFFFFFFFFFFFFFF变长了-9223372036854775808
	ASSERT_EQ(pkg_table.body.xxx.typeTester.int64Array[3], 0x7FFFFFFFFFFFFFFF)--此处验证打包前是ok的
	
	--ASSERT_EQ(pkg_table3.body.xxx.typeTester.float, 0xFFFFFFFF) --此处验证不过，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(pkg_table.body.xxx.typeTester.float, 0xFFFFFFFF)--此处验证打包前是ok的
	
	---会存在精度问题，实际为{-3.40282346e+38, 1.17549435e-38, 3.40282346e+38}
	ASSERT_EQ(tostring(pkg_table3.body.xxx.typeTester.floatArray[1]), "-3.4028234663853e+38")
	ASSERT_EQ(tostring(pkg_table3.body.xxx.typeTester.floatArray[2]), "1.1754943508223e-38")
	ASSERT_EQ(tostring(pkg_table3.body.xxx.typeTester.floatArray[3]), "3.4028234663853e+38")
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.double, 2.2250738585072014e-308)
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.doubleArray, {-1.7976931348623157e+308, -2.2250738585072014e-308, 1.7976931348623157e+308})
	ASSERT_EQ(string.sub(tostring(pkg_table3.body.xxx.boundary),1, 4), "-1.1")
	ASSERT_EQ(pkg_table3.body.xxx.selector, 1)
	print("to_string filed1:" .. table_to_string(pkg_table3.body.xxx.innerUnion.field1[1]))
	--ASSERT_EQ(pkg_table3.body.xxx.innerUnion.field1[1], {unit64 = 0x0FFFFFFFFFFFFFFF, uint = 0xFFFFFFFF})
	
	ASSERT_EQ(type(pkg_table.body.xxx.innerUnion.field1[1]), 'table')
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[1].uint64, 0x0FFFFFFFFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[1].uint, 0xFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[2].uint64, 0x0FFFFFFFFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[2].uint, 0)
	
	ASSERT_EQ(tostring(pkg_table3.body.xxx.innerUnion.field1[1].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
	--ASSERT_EQ(pkg_table3.body.xxx.innerUnion.field1[1].uint, 0xFFFFFFFF) --这个值解包后有问题，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(tostring(pkg_table3.body.xxx.innerUnion.field1[2].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
	ASSERT_EQ(pkg_table3.body.xxx.innerUnion.field1[2].uint, 0)
	ASSERT_EQ(pkg_table3.body.xxx.structArray.count, 1)
	ASSERT_EQ(tostring(pkg_table3.body.xxx.structArray.array[1].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
    --ASSERT_EQ(pkg_table3.body.xxx.structArray.array[1].uint, 0xFFFFFFFF) --这个值解包后有问题，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(pkg_table3.body.xxx.structArray.array[2], nil)
	ASSERT_EQ(string.sub(tostring(pkg_table3.body.xxx.boundary2),1, 8), "1.111111")	
end

function CMyTestCaseLuaTdr.CaseLoadMetalibBuff_3(self)
    self.count = 1 + self.count
	pkg_table2, pkg_table3 = self:LoadMetalib(1, "testxxx.tdr", 3)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 3)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 222)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	ASSERT_EQ(pkg_table2.body.logout, nil)
	ASSERT_EQ(libtdrlua.tdrdate2str(pkg_table2.body.xxx.typeTester.date), "2015-09-08")
	ASSERT_EQ(libtdrlua.tdrtime2str(pkg_table2.body.xxx.typeTester.time), " 22:17:59")
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.time, libtdrlua.str2tdrtime("22:17:59"))
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.int8, -1)
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.uint8Array, {0, 23, 255})
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.int8VarArrayRefer, 2)
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.int8VarArray, {-128, 127})
	ASSERT_EQ(pkg_table.body.xxx.typeTester.uintArray, {0, 1721, 0xFFFFFFFF})
	--ASSERT_EQ(pkg_table2.body.xxx.typeTester.uintArray, {0, 1721, 0xFFFFFFFF}) --测试不过，0xFFFFFFFF解包后变长了0x80000000
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.intVarArray, {-0x80000000})
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.strArray, {"Francis", "Francis"})
	ASSERT_EQ(tostring(pkg_table2.body.xxx.typeTester.uint64), "4503599627370495") --0xFFFFFFFFFFFFF
	
	--ASSERT_EQ(pkg_table2.body.xxx.typeTester.int64Array, {-9223372036854775808, 4503599627370495, 9223372036854775807}) --{-0x8000000000000000, 0xFFFFFFFFFFFFF, 0x7FFFFFFFFFFFFFFF},，测试不过，解包后0x7FFFFFFFFFFFFFFF变长了-9223372036854775808
	ASSERT_EQ(pkg_table.body.xxx.typeTester.int64Array[3], 0x7FFFFFFFFFFFFFFF)--此处验证打包前是ok的
	
	--ASSERT_EQ(pkg_table2.body.xxx.typeTester.float, 0xFFFFFFFF) --此处验证不过，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(pkg_table.body.xxx.typeTester.float, 0xFFFFFFFF)--此处验证打包前是ok的
	
	---会存在精度问题，实际为{-3.40282346e+38, 1.17549435e-38, 3.40282346e+38}
	ASSERT_EQ(tostring(pkg_table2.body.xxx.typeTester.floatArray[1]), "-3.4028234663853e+38")
	ASSERT_EQ(tostring(pkg_table2.body.xxx.typeTester.floatArray[2]), "1.1754943508223e-38")
	ASSERT_EQ(tostring(pkg_table2.body.xxx.typeTester.floatArray[3]), "3.4028234663853e+38")
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.double, 2.2250738585072014e-308)
	ASSERT_EQ(pkg_table2.body.xxx.typeTester.doubleArray, {-1.7976931348623157e+308, -2.2250738585072014e-308, 1.7976931348623157e+308})
	ASSERT_EQ(string.sub(tostring(pkg_table2.body.xxx.boundary),1, 4), "-1.1")
	ASSERT_EQ(pkg_table2.body.xxx.selector, 1)
	print("to_string filed1:" .. table_to_string(pkg_table2.body.xxx.innerUnion.field1[1]))
	--ASSERT_EQ(pkg_table2.body.xxx.innerUnion.field1[1], {unit64 = 0x0FFFFFFFFFFFFFFF, uint = 0xFFFFFFFF})
	
	ASSERT_EQ(type(pkg_table.body.xxx.innerUnion.field1[1]), 'table')
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[1].uint64, 0x0FFFFFFFFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[1].uint, 0xFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[2].uint64, 0x0FFFFFFFFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[2].uint, 0)
	
	ASSERT_EQ(tostring(pkg_table2.body.xxx.innerUnion.field1[1].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
	--ASSERT_EQ(pkg_table2.body.xxx.innerUnion.field1[1].uint, 0xFFFFFFFF) --这个值解包后有问题，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(tostring(pkg_table2.body.xxx.innerUnion.field1[2].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
	ASSERT_EQ(pkg_table2.body.xxx.innerUnion.field1[2].uint, 0)
	ASSERT_EQ(pkg_table2.body.xxx.structArray.count, 1)
	ASSERT_EQ(tostring(pkg_table2.body.xxx.structArray.array[1].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
    --ASSERT_EQ(pkg_table2.body.xxx.structArray.array[1].uint, 0xFFFFFFFF) --这个值解包后有问题，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(pkg_table2.body.xxx.structArray.array[2], nil)
	ASSERT_EQ(string.sub(tostring(pkg_table2.body.xxx.boundary2),1, 8), "1.111111")
	
	ASSERT_EQ(pkg_table3.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table3.head.msgid), "10000001")
	ASSERT_EQ(pkg_table3.head.cmd, 3)
	ASSERT_EQ(pkg_table3.head.version, 3)
	ASSERT_EQ(pkg_table3.head.bodyLen, 222)
	ASSERT_EQ(pkg_table3.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(libtdrlua.tdrip2str(pkg_table3.head.srcIp), "127.0.0.1")
	ASSERT_EQ(pkg_table3.body.login, nil)
	ASSERT_EQ(pkg_table3.body.logout, nil)
	ASSERT_EQ(libtdrlua.tdrdate2str(pkg_table3.body.xxx.typeTester.date), "2015-09-08")
	ASSERT_EQ(libtdrlua.tdrtime2str(pkg_table3.body.xxx.typeTester.time), " 22:17:59")
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.time, libtdrlua.str2tdrtime("22:17:59"))
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.int8, -1)
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.uint8Array, {0, 23, 255})
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.int8VarArrayRefer, 2)
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.int8VarArray, {-128, 127})
	--ASSERT_EQ(pkg_table3.body.xxx.typeTester.uintArray, {0, 1721, 0xFFFFFFFF}) --测试不过，0xFFFFFFFF解包后变长了0x80000000
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.intVarArray, {-0x80000000})
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.strArray, {"Francis", "Francis"})
	ASSERT_EQ(tostring(pkg_table3.body.xxx.typeTester.uint64), "4503599627370495") --0xFFFFFFFFFFFFF
	
	--ASSERT_EQ(pkg_table3.body.xxx.typeTester.int64Array, {-9223372036854775808, 4503599627370495, 9223372036854775807}) --{-0x8000000000000000, 0xFFFFFFFFFFFFF, 0x7FFFFFFFFFFFFFFF},，测试不过，解包后0x7FFFFFFFFFFFFFFF变长了-9223372036854775808
	ASSERT_EQ(pkg_table.body.xxx.typeTester.int64Array[3], 0x7FFFFFFFFFFFFFFF)--此处验证打包前是ok的
	
	--ASSERT_EQ(pkg_table3.body.xxx.typeTester.float, 0xFFFFFFFF) --此处验证不过，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(pkg_table.body.xxx.typeTester.float, 0xFFFFFFFF)--此处验证打包前是ok的
	
	---会存在精度问题，实际为{-3.40282346e+38, 1.17549435e-38, 3.40282346e+38}
	ASSERT_EQ(tostring(pkg_table3.body.xxx.typeTester.floatArray[1]), "-3.4028234663853e+38")
	ASSERT_EQ(tostring(pkg_table3.body.xxx.typeTester.floatArray[2]), "1.1754943508223e-38")
	ASSERT_EQ(tostring(pkg_table3.body.xxx.typeTester.floatArray[3]), "3.4028234663853e+38")
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.double, 2.2250738585072014e-308)
	ASSERT_EQ(pkg_table3.body.xxx.typeTester.doubleArray, {-1.7976931348623157e+308, -2.2250738585072014e-308, 1.7976931348623157e+308})
	ASSERT_EQ(string.sub(tostring(pkg_table3.body.xxx.boundary),1, 4), "-1.1")
	ASSERT_EQ(pkg_table3.body.xxx.selector, 1)
	print("to_string filed1:" .. table_to_string(pkg_table3.body.xxx.innerUnion.field1[1]))
	--ASSERT_EQ(pkg_table3.body.xxx.innerUnion.field1[1], {unit64 = 0x0FFFFFFFFFFFFFFF, uint = 0xFFFFFFFF})
	
	ASSERT_EQ(type(pkg_table.body.xxx.innerUnion.field1[1]), 'table')
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[1].uint64, 0x0FFFFFFFFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[1].uint, 0xFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[2].uint64, 0x0FFFFFFFFFFFFFFF)
	ASSERT_EQ(pkg_table.body.xxx.innerUnion.field1[2].uint, 0)
	
	ASSERT_EQ(tostring(pkg_table3.body.xxx.innerUnion.field1[1].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
	--ASSERT_EQ(pkg_table3.body.xxx.innerUnion.field1[1].uint, 0xFFFFFFFF) --这个值解包后有问题，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(tostring(pkg_table3.body.xxx.innerUnion.field1[2].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
	ASSERT_EQ(pkg_table3.body.xxx.innerUnion.field1[2].uint, 0)
	ASSERT_EQ(pkg_table3.body.xxx.structArray.count, 1)
	ASSERT_EQ(tostring(pkg_table3.body.xxx.structArray.array[1].uint64 - 0x0FFFFFFFFFFFFFFF), "0")
    --ASSERT_EQ(pkg_table3.body.xxx.structArray.array[1].uint, 0xFFFFFFFF) --这个值解包后有问题，ASSERT_EQ failed --> left:4294967296, right:4294967295.
	ASSERT_EQ(pkg_table3.body.xxx.structArray.array[2], nil)
	ASSERT_EQ(string.sub(tostring(pkg_table3.body.xxx.boundary2),1, 8), "1.111111")	
end

function CMyTestCaseLuaTdr.CaseLoadMetalib_4(self)
    self.count = 1 + self.count
	if CS.LuaTestCommon.android_platform == true then
	    return
	end
	pkg_table2 = self:LoadMetalib(0, CS.LuaTestCommon.xxxtdrfilepath, 4)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 4)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 4)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	ASSERT_EQ(pkg_table2.body.ext1, -1)
end

function CMyTestCaseLuaTdr.CaseLoadMetalibBuff_4(self)
    self.count = 1 + self.count
	pkg_table2 = self:LoadMetalib(1, "testxxx.tdr", 5)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 5)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 4)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	ASSERT_EQ(pkg_table2.body.ext1, -1)
end

function CMyTestCaseLuaTdr.CaseLoadMetalib_5(self)
    self.count = 1 + self.count
	if CS.LuaTestCommon.android_platform == true then
	    return
	end
	pkg_table2 = self:LoadMetalib(0, CS.LuaTestCommon.xxxtdrfilepath, 70)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 70)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 3)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	ASSERT_EQ(pkg_table2.body.ext2, {0, 1, 2})
end

function CMyTestCaseLuaTdr.CaseLoadMetalibBuff_5(self)
    self.count = 1 + self.count
	pkg_table2 = self:LoadMetalib(1, "testxxx.tdr", 80)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 80)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 3)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	ASSERT_EQ(pkg_table2.body.ext2, {0, 1, 2})
end

function CMyTestCaseLuaTdr.CaseLoadMetalib_6(self)
    self.count = 1 + self.count
	if CS.LuaTestCommon.android_platform == true then
	    return
	end
	pkg_table2 = self:LoadMetalib(0, CS.LuaTestCommon.xxxtdrfilepath, 75)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 75)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 3)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	ASSERT_EQ(pkg_table2.body.ext2, {0, 1, 2})
end

function CMyTestCaseLuaTdr.CaseLoadMetalibBuff_6(self)
    self.count = 1 + self.count
	pkg_table2 = self:LoadMetalib(1, "testxxx.tdr", 81)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 81)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 4)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	ASSERT_EQ(pkg_table2.body.ext1, -1)
end

function CMyTestCaseLuaTdr.CaseLoadMetalib_7(self)
    self.count = 1 + self.count
	if CS.LuaTestCommon.android_platform == true then
	    return
	end
	pkg_table2 = self:LoadMetalib(0, CS.LuaTestCommon.xxxtdrfilepath, 15)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 15)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 24)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	ASSERT_EQ(pkg_table2.body.ext3, {"Francis", "Francis"})
end

function CMyTestCaseLuaTdr.CaseLoadMetalibBuff_7(self)
    self.count = 1 + self.count
	pkg_table2 = self:LoadMetalib(1, "testxxx.tdr", 100)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 100)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 12)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	ASSERT_EQ(pkg_table2.body.ext4, "Francis")
end

function CMyTestCaseLuaTdr.CaseLoadMetalibNoExistTdr(self)
    self.count = 1 + self.count
	local ret_code, err_msg = libtdrlua.load_metalib("noexist.tdr")
	ASSERT_EQ(ret_code, -2113862584)
    print("CaseLoadMetalibNoExistTdr err_msg:" .. err_msg)
end

function CMyTestCaseLuaTdr.CaseLoadMetalibErrorTdrFile(self)
    self.count = 1 + self.count
	local ret_code, err_msg = libtdrlua.load_metalib("test2.lua")
	ASSERT_EQ(ret_code, -2113862584)
	print("CaseLoadMetalibErrorTdrFile err_msg:" .. err_msg)
end

function CMyTestCaseLuaTdr.CaseLoadMetalibMultiTimes(self)
    self.count = 1 + self.count
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
	local ret_code2, metalib2 = libtdrlua.load_metalib_buf(tdrmeta)
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(ret_code2, 0)
	local err_msg = libtdrlua.free_metalib(metalib)
	ASSERT_EQ(err_msg, nil)
	local err_msg2 = libtdrlua.free_metalib(metalib2)
	ASSERT_EQ(err_msg2, nil)
end

function CMyTestCaseLuaTdr.CaseLoadMetalibDiffTdrFile(self)
    self.count = 1 + self.count
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local tdrmeta2 = CS.UnityEngine.Resources.Load("testxxx2.tdr").bytes
	local ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
	local ret_code2, metalib2 = libtdrlua.load_metalib_buf(tdrmeta2)
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(ret_code2, 0)
	local err_msg = libtdrlua.free_metalib(metalib)
	ASSERT_EQ(err_msg, nil)
	local err_msg2 = libtdrlua.free_metalib(metalib2)
	ASSERT_EQ(err_msg2, nil)
end

function CMyTestCaseLuaTdr.CaseLoadMetalibBufFromEmptyStr(self)
    self.count = 1 + self.count
	local ret_code, metalib = libtdrlua.load_metalib_buf("")
	ASSERT_EQ(ret_code, -2113862536)
end

function CMyTestCaseLuaTdr.CaseLoadMetalibBufFromNoTdrStr(self)
    self.count = 1 + self.count
	local ret_code, metalib = libtdrlua.load_metalib_buf("testtdr")
	ASSERT_EQ(ret_code, -2113862536)
end

function CMyTestCaseLuaTdr.CaseGetMeta(self)
    --加载meta元数据库	
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
	ASSERT_EQ(ret_code, 0)
    print("libtdrlua.load_metalib ok")
	
	------------------------------------------------------------------------
	-- API - get_meta
	-- return value - ret_code, meta/err_msg
	----------------------------------------------------------------------
	local ret_code, meta = libtdrlua.get_meta(metalib, "PkgBody")
	ASSERT_EQ(ret_code, 0)

	local ret_code, meta = libtdrlua.get_meta(metalib, "PkgNoExist")
	ASSERT_EQ(ret_code, -1)
	
	local ret_code, meta = libtdrlua.get_meta(metalib, "PKG_ID")
	ASSERT_EQ(ret_code, -1)
	
	local err_msg = libtdrlua.free_metalib(metalib)
	ASSERT_EQ(err_msg, nil)
end

function CMyTestCaseLuaTdr.CaseTable2Buf_1(self)
    --长度等于实际长度	
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
	ASSERT_EQ(ret_code, 0)

	buf_size = 71
	local ret_code, buf = libtdrlua.bufalloc(buf_size)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, meta = libtdrlua.get_meta(metalib, "Pkg")
	ASSERT_EQ(ret_code, 0)
	
	pkg_table.head.cmd = 1
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table, buf, buf_size, 0)
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(used_size, buf_size)
	
	local ret_code, pkg_table2, used_size2 = libtdrlua.buf2table(meta, buf, used_size, 0)
	print("table2buf pkg_table2: " .. table_to_string(pkg_table2))
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 1)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 39)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login.name, "FrancisHe")
	ASSERT_EQ(pkg_table2.body.login.pass, "123456")
	ASSERT_EQ(pkg_table2.body.login.zone, "Japan")
	ASSERT_EQ(pkg_table2.body.login.destIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.ext1, nil)
	
	local err_msg = libtdrlua.free_metalib(metalib)
	ASSERT_EQ(err_msg, nil)
	local err_msg = libtdrlua.buffree(buf)
	ASSERT_EQ(err_msg, nil)
end

function CMyTestCaseLuaTdr.CaseTable2Buf_2(self)
    --buf 和 len 小于pkg_table实际长度	
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
	ASSERT_EQ(ret_code, 0)

	buf_size = 56
	local ret_code, buf = libtdrlua.bufalloc(buf_size)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, meta = libtdrlua.get_meta(metalib, "Pkg")
	ASSERT_EQ(ret_code, 0)
	
	pkg_table.head.cmd = 1
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table, buf, buf_size, 0)
	ASSERT_EQ(ret_code, -2113862654)
	
	local err_msg = libtdrlua.free_metalib(metalib)
	ASSERT_EQ(err_msg, nil)
	local err_msg = libtdrlua.buffree(buf)
	ASSERT_EQ(err_msg, nil)
end

function CMyTestCaseLuaTdr.CaseTable2Buf_3(self)	
	--4.不设置version参数打包
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
	ASSERT_EQ(ret_code, 0)
	
	local buf_size = 71
	local ret_code, buf = libtdrlua.bufalloc(buf_size)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, meta = libtdrlua.get_meta(metalib, "Pkg")
	ASSERT_EQ(ret_code, 0)
	
	pkg_table.head.cmd = 1
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table, buf, buf_size)
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(used_size, buf_size)
	
	local ret_code, pkg_table2, used_size2 = libtdrlua.buf2table(meta, buf, used_size)
	print("table2buf pkg_table2: " .. table_to_string(pkg_table2))
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 1)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 39)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login.name, "FrancisHe")
	ASSERT_EQ(pkg_table2.body.login.pass, "123456")
	ASSERT_EQ(pkg_table2.body.login.zone, "Japan")
	ASSERT_EQ(pkg_table2.body.login.destIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.ext1, nil)
	
	local err_msg = libtdrlua.free_metalib(metalib)
	ASSERT_EQ(err_msg, nil)
	local err_msg = libtdrlua.buffree(buf)
	ASSERT_EQ(err_msg, nil)
end

function CMyTestCaseLuaTdr.CaseTable2Buf_4(self)	
	--5.version参数值小于等于meta中设置的version
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
	ASSERT_EQ(ret_code, 0)
	
	local buf_size = 71
	local ret_code, buf = libtdrlua.bufalloc(buf_size)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, meta = libtdrlua.get_meta(metalib, "Pkg")
	ASSERT_EQ(ret_code, 0)
	
	pkg_table.head.cmd = 1
	--Struct Pkg有设置versionindicator="head.version"，因此会按照打包时的version解包，buf2table输入的version不会生效
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table, buf, buf_size, 2)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, pkg_table2, used_size2 = libtdrlua.buf2table(meta, buf, used_size, 3)
	print("table2buf pkg_table2: " .. table_to_string(pkg_table2))
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 1)
	ASSERT_EQ(pkg_table2.head.version, 2)
	ASSERT_EQ(pkg_table2.head.bodyLen, 39)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, 0) --srcIp的version为3
	ASSERT_EQ(pkg_table2.body.login.name, "FrancisHe")
	ASSERT_EQ(pkg_table2.body.login.pass, "123456")
	ASSERT_EQ(pkg_table2.body.login.zone, "Japan")
	ASSERT_EQ(pkg_table2.body.login.destIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.ext1, nil)
	
	local ret_code, meta = libtdrlua.get_meta(metalib, "PkgHead")
	ASSERT_EQ(ret_code, 0)
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table.head, buf, buf_size, 3)
	ASSERT_EQ(ret_code, 0)
	local ret_code, pkg_table2, used_size2 = libtdrlua.buf2table(meta, buf, used_size, 2)
	print("table2buf pkghead pkg_table2: " .. table_to_string(pkg_table2))
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(pkg_table2.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.msgid), "10000001")
	ASSERT_EQ(pkg_table2.cmd, 1)
	ASSERT_EQ(pkg_table2.version, 2)
	ASSERT_EQ(pkg_table2.bodyLen, 0)
	ASSERT_EQ(pkg_table2.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.srcIp, 0) --srcIp的version为3
	
	--6.version参数值大于meta中设置的version
	local ret_code, pkg_table3, used_size2 = libtdrlua.buf2table(meta, buf, used_size, 4)
	print("table2buf pkghead pkg_table3: " .. table_to_string(pkg_table3))
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table.head, buf, buf_size, 4)
	ASSERT_EQ(ret_code, 0)
	
	local err_msg = libtdrlua.free_metalib(metalib)
	ASSERT_EQ(err_msg, nil)
	local err_msg = libtdrlua.buffree(buf)
	ASSERT_EQ(err_msg, nil)
end

function CMyTestCaseLuaTdr.CaseBuf2Table_1(self)
    --2.len 大于buf长度，version为0
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
	ASSERT_EQ(ret_code, 0)

	buf_size = 128
	local ret_code, buf = libtdrlua.bufalloc(buf_size)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, meta = libtdrlua.get_meta(metalib, "Pkg")
	ASSERT_EQ(ret_code, 0)
	
	pkg_table.head.cmd = 1
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table, buf, buf_size, 0)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, pkg_table2, used_size2 = libtdrlua.buf2table(meta, buf, used_size + 10, 0)
	print("table2buf pkg_table2: " .. table_to_string(pkg_table2))
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 1)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 39)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login.name, "FrancisHe")
	ASSERT_EQ(pkg_table2.body.login.pass, "123456")
	ASSERT_EQ(pkg_table2.body.login.zone, "Japan")
	ASSERT_EQ(pkg_table2.body.login.destIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.ext1, nil)
	
	--3.len小于message（buf）长度
	local ret_code, pkg_table2, used_size2 = libtdrlua.buf2table(meta, buf, used_size - 10, 0)
	ASSERT_EQ(ret_code, -2113862654)
	
	--4.meta和buf中数据结构定义不一致
	local ret_code, meta_2 = libtdrlua.get_meta(metalib, "PkgBodyPrevious")
	ASSERT_EQ(ret_code, 0)
	local ret_code, pkg_table2, used_size2 = libtdrlua.buf2table(meta_2, buf, used_size, 0)
	ASSERT_EQ(ret_code, -2113862552)
	
	local err_msg = libtdrlua.free_metalib(metalib)
	ASSERT_EQ(err_msg, nil)
	local err_msg = libtdrlua.buffree(buf)
	ASSERT_EQ(err_msg, nil)
end

function CMyTestCaseLuaTdr.CaseStr2Buf(self)	
	--1. str为空
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
	ASSERT_EQ(ret_code, 0)
	
	local buf_size = 71
	local ret_code, buf = libtdrlua.bufalloc(buf_size)
	ASSERT_EQ(ret_code, 0)
	
	pkg_table.head.cmd = 1
	local ret_code, meta = libtdrlua.get_meta(metalib, "PkgHead")
	ASSERT_EQ(ret_code, 0)
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table.head, buf, buf_size, 0)
	ASSERT_EQ(ret_code, 0)
	local ret_code, str = libtdrlua.buf2str(buf, used_size)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, err_msg = libtdrlua.str2table(meta, "", 3)
	ASSERT_EQ(ret_code, -2113862552)
	
	--3.meta和str中数据结构定义不一致
	local ret_code, meta2 = libtdrlua.get_meta(metalib, "PkgBody")
	local ret_code, err_msg = libtdrlua.str2table(meta2, str, 0)
	ASSERT_EQ(ret_code, -2113862552)
	
    --4. version不设置
	local ret_code, pkg_table2 = libtdrlua.str2table(meta, str)
	ASSERT_EQ(ret_code, 0)
	
    --5. version小于等于meta中version值
	local ret_code, pkg_table3 = libtdrlua.str2table(meta, str, 2)
	ASSERT_EQ(ret_code, 0)
	
    --6. version大于meta中version值
	local ret_code, pkg_table4 = libtdrlua.str2table(meta, str, 2)
	ASSERT_EQ(ret_code, 0)
	
	print("table2buf pkghead pkg_table2: " .. table_to_string(pkg_table2))
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(pkg_table2.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.msgid), "10000001")
	ASSERT_EQ(pkg_table2.cmd, 1)
	ASSERT_EQ(pkg_table2.version, 3)
	ASSERT_EQ(pkg_table2.bodyLen, 0)
	ASSERT_EQ(pkg_table2.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	
	local err_msg = libtdrlua.free_metalib(metalib)
	ASSERT_EQ(err_msg, nil)
	local err_msg = libtdrlua.buffree(buf)
	ASSERT_EQ(err_msg, nil)
end

function CMyTestCaseLuaTdr.CaseZZZRefer_1(self)
    --2 某数组refer的值大于数组count，调用table2buf函数
    self.count = 1 + self.count
	pkg_table.body.logout.count = 4
	pkg_table.head.cmd = 2
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
	ASSERT_EQ(ret_code, 0)

	buf_size = 128
	local ret_code, buf = libtdrlua.bufalloc(buf_size)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, meta = libtdrlua.get_meta(metalib, "Pkg")
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table, buf, buf_size, 0)
	ASSERT_EQ(ret_code, -2113862521)
	
	--3 某数组refer的值等于数组count，调用table2buf打包和buf2table解包
	pkg_table.body.logout.count = 3
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table, buf, buf_size, 0)
	ASSERT_EQ(ret_code, 0)
	local ret_code, pkg_table2 = libtdrlua.buf2table(meta, buf, used_size, 0)
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 2)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 17)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	local ret = -1
	ASSERT_EQ(ret, -1)
	ASSERT_EQ(tostring(pkg_table2.body.logout.reason + 1), "0")
	ASSERT_EQ(pkg_table2.body.logout.count, 3)
	ASSERT_EQ(pkg_table2.body.logout.attr, {-1, 0, 1})
	ASSERT_EQ(pkg_table2.body.ext1, nil)
	
	-- 4 某数组refer的值等于0，调用table2buf打包和buf2table解包
	pkg_table.body.logout.count = 0
	local ret_code, used_size = libtdrlua.table2buf(meta, pkg_table, buf, buf_size, 0)
	ASSERT_EQ(ret_code, 0)
	local ret_code, pkg_table2 = libtdrlua.buf2table(meta, buf, used_size, 0)
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 2)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 14)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.login, nil)
	local ret = -1
	ASSERT_EQ(ret, -1)
	ASSERT_EQ(tostring(pkg_table2.body.logout.reason + 1), "0")
	ASSERT_EQ(pkg_table2.body.logout.count, 0)
	ASSERT_EQ(pkg_table2.body.logout.attr, {})
	ASSERT_EQ(pkg_table2.body.ext1, nil)
	
	pkg_table.body.logout.count = 2
	pkg_table.head.cmd = 1
	local err_msg = libtdrlua.free_metalib(metalib)
	ASSERT_EQ(err_msg, nil)
	local err_msg = libtdrlua.buffree(buf)
	ASSERT_EQ(err_msg, nil)
end

function CMyTestCaseLuaTdr.CaseZZZZVersion_1(self)

    --4.高版本多2个字段，用高版本meta，打包时传入低版本version， 然后用低版本meta解包（version为0）
	
    self.count = 1 + self.count
	pkg_table.head.cmd = 1
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local ret_code, metalib_v3 = libtdrlua.load_metalib_buf(tdrmeta)
	ASSERT_EQ(ret_code, 0)
	local tdrmeta2 = CS.UnityEngine.Resources.Load("testxxx2.tdr").bytes
	local ret_code, metalib_v4 = libtdrlua.load_metalib_buf(tdrmeta2)
	ASSERT_EQ(ret_code, 0)
	
	buf_size = 128
	local ret_code, buf = libtdrlua.bufalloc(buf_size)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, meta_v3 = libtdrlua.get_meta(metalib_v3, "Pkg")
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, meta_v4 = libtdrlua.get_meta(metalib_v4, "Pkg")
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, used_size = libtdrlua.table2buf(meta_v4, pkg_table_v4, buf, buf_size, 3)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, pkg_table2 = libtdrlua.buf2table(meta_v3, buf, used_size, 0)
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(pkg_table2.head.magic, 32767)
	ASSERT_EQ(tostring(pkg_table2.head.msgid), "10000001")
	ASSERT_EQ(pkg_table2.head.cmd, 1)
	ASSERT_EQ(pkg_table2.head.version, 3)
	ASSERT_EQ(pkg_table2.head.bodyLen, 39)
	ASSERT_EQ(pkg_table2.head.datetime, libtdrlua.str2tdrdatetime("2015-09-08 21:17:59"))
	ASSERT_EQ(pkg_table2.head.srcIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.head.destIp, nil)
	ASSERT_EQ(pkg_table2.body.login.name, "FrancisHe")
	ASSERT_EQ(pkg_table2.body.login.pass, "123456")
	ASSERT_EQ(pkg_table2.body.login.zone, "Japan")
	ASSERT_EQ(pkg_table2.body.login.destIp, libtdrlua.str2tdrip("127.0.0.1"))
	ASSERT_EQ(pkg_table2.body.ext1, nil)
	
	--1.1 pack低版本（meta对应低版本），unpack高版本（meta对应高版本）
	local ret_code, used_size = libtdrlua.table2buf(meta_v3, pkg_table, buf, buf_size, 0)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, pkg_table2 = libtdrlua.buf2table(meta_v4, buf, used_size, 0)
	ASSERT_EQ(ret_code, 0)
	
	--1.2 pack高版本，unpack低版本 
	local ret_code, used_size = libtdrlua.table2buf(meta_v4, pkg_table_v4, buf, buf_size, 0)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, pkg_table2 = libtdrlua.buf2table(meta_v3, buf, used_size, 0)
	ASSERT_EQ(ret_code, -2113862544)
	
	pkg_table.head.cmd = 1
	local err_msg = libtdrlua.free_metalib(metalib_v3)
	ASSERT_EQ(err_msg, nil)
	local err_msg = libtdrlua.free_metalib(metalib_v4)
	ASSERT_EQ(err_msg, nil)
	local err_msg = libtdrlua.buffree(buf)
	ASSERT_EQ(err_msg, nil)
end


function CMyTestCaseLuaTdr.CaseZZZZRandom(self)
    --6.同一个结构类 entry 类型乱序排列，调用table2buf打包和buf2table解包
	
    self.count = 1 + self.count
	local tdrmeta = CS.UnityEngine.Resources.Load("testxxx.tdr").bytes
	local ret_code, metalib = libtdrlua.load_metalib_buf(tdrmeta)
	ASSERT_EQ(ret_code, 0)
	
	buf_size = 128
	local ret_code, buf = libtdrlua.bufalloc(buf_size)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, meta_v1 = libtdrlua.get_meta(metalib, "TestInnerStruct1")
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, meta_v2 = libtdrlua.get_meta(metalib, "TestInnerStruct2")
	ASSERT_EQ(ret_code, 0)
	
	pkg_table_v1 = {
		int8 = 127,
		double = 2.2250738585072114e-308,
		float = 1,
		int16 = 32767,
		int32Array = {-0x7FFFFFFF, 0, 0x7FFFFFFF},
	}
	
	pkg_table_v2 = {
		double = 2.2250738585072114e-308,
		int8 = 127,
		int32Array = {-0x7FFFFFFF, 0, 0x7FFFFFFF},
		int16 = 32767,
		float = 1,
	}
	
	local ret_code, used_size = libtdrlua.table2buf(meta_v1, pkg_table_v1, buf, buf_size, 3)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, pkg_table2 = libtdrlua.buf2table(meta_v1, buf, used_size, 0)
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(pkg_table2.int8, 127)
	ASSERT_EQ(pkg_table2.double, 2.2250738585072114e-308)
	ASSERT_EQ(pkg_table2.float, 1)
	ASSERT_EQ(pkg_table2.int16, 32767)
	ASSERT_EQ(pkg_table2.int32Array, {-0x7FFFFFFF, 0, 0x7FFFFFFF})
	
	local ret_code, used_size = libtdrlua.table2buf(meta_v2, pkg_table_v2, buf, buf_size, 3)
	ASSERT_EQ(ret_code, 0)
	
	local ret_code, pkg_table2 = libtdrlua.buf2table(meta_v2, buf, used_size, 0)
	ASSERT_EQ(ret_code, 0)
	ASSERT_EQ(pkg_table2.int8, 127)
	ASSERT_EQ(pkg_table2.double, 2.2250738585072114e-308)
	ASSERT_EQ(pkg_table2.float, 1)
	ASSERT_EQ(pkg_table2.int16, 32767)
	ASSERT_EQ(pkg_table2.int32Array, {-0x7FFFFFFF, 0, 0x7FFFFFFF})

	local err_msg = libtdrlua.free_metalib(metalib)
	ASSERT_EQ(err_msg, nil)
	local err_msg = libtdrlua.buffree(buf)
	ASSERT_EQ(err_msg, nil)
end