#if !XLUA_GENERAL
using UnityEngine;
#endif
using System.Collections;
using System.Collections.Generic;
using XLua;
using System;
using System.IO;

public class LuaEnvSingletonForTest  {
	
	static private LuaEnv instance = null;
	static public LuaEnv Instance
	{
		get
		{
			if(instance == null)
			{
				instance = new LuaEnv();
#if XLUA_GENERAL
                instance.DoString("package.path = package.path..';../Test/UnitTest/xLuaTest/CSharpCallLua/Resources/?.lua.txt;../Test/UnitTest/StreamingAssets/?.lua'");
#endif
            }
			
			return instance;
		}
	}
    
}


public static class GenClass
{
    [CSharpCallLua]
    public static List<Type> mymodule_lua_call_cs_list = new List<Type>()
    {
        typeof(System.Action),
    };
}

public struct Pedding
{
    public byte c;
}

[GCOptimize]
[LuaCallCSharp]
public struct TestStruct
{
    public TestStruct(int p1, int p2)
    {
        a = p1;
        b = p2;
        c = p2;
        e.c = (byte)p1;
    }
    public int a;
    public int b;
    public decimal c;
    public Pedding e;
}

[LuaCallCSharp]
public class TCForTestCSCallLua{
	public static LuaEnv luaEnv = LuaEnvSingletonForTest.Instance;
	string msg = "";

	public void LOG(string text)
	{
		msg += text;
	}

    string script = @"
		bValue1 = false
		bValue2 = true
		strValueEmpty = ''
		strValueLong = string.rep('a', 2^10)
		strValueShort = 'boo123'
		strValueExp = '.* ? [ ] ^ $~`!@#$%^&()_-+=[];\',““〈〉〖【℃ ＄ ¤№ ☆ ★■ⅷ②㈣'
		strValue5 = '\r\na'
		strValue6 = [[
         <html>
		    <head>
		        <title>test</title>
		    </head>
		    <body>
			this is just a test lua multi lines string
		    </body>
		</html>]]
		strValueChin = '中文字符串'
		strValueComp = '繁體國陸'
		strValueHoX = '吙煋呅僦媞這樣孒'
		sbyteValueMax = 127
		sbyteValueMin = -128
		byteValueMax = 255
		byteValueMin = 0
		shortValueMax = 32767
		shortValueMin= -32768
		ushortValueMax = 65535
		ushortValueMid = 32768
		intValueMax = 2147483647
		intValueMin = -2147483648
		uintValueMax = 4294967295
		uintValueMid = 2147483648
		longValue = 42949672960
		longValue2 = -42949672960
		ulongValueMax = 42949672961111
		ulongValueMin = 0
		doubleValue = 3.14159265
		doubleValue2 = -3.14159265
		floatValue = 3.14159265
		floatValue2 = -3.14
		charValue = 97
		charValue2 = 65
		decValue = 12.3111111111
		decValue2 = -12.3111111111
		intValue3 = 0
		intZero = 0

		tableValue1 = {
		key1 = 100000, key2 = 10, key3 = true,
		'red', 'yellow', 'green',
		sub = function(self, a, b) 
		              print('tableValue1.sub called')
		              return a - b 
		      end,
		tableValueInclude = {
		1,2,3,4,5,6,7,8,9,0
		},
		tableVarInclude = {ikey1 = 10, ikey2 = 12}
		}

		tableValue2 = {
		kv1 = true, kv2 = 'monday', kv3 = 1
		}

		tableValue3 = {
		'apple', 'banana', 'orange', 'kiwi', 'grape', 'lemon', 'strawberry', 'pear'
		}

		tableValue4 = {
		k1= 1, k2= 10, k3 = 100, k4 = 1000, k5 = 10000
		}

		tableValue5 = {
		[100] =  100, [101] = 1000, [102] = 10000
		}

		tableValue6 = {1,2,3}

		tableValue7 = {
		key1 = 'abc',  key2 = 10}

		function func_self_increase()
			print('func_self_inscrease called')
			intValue3 = intValue3 + 1;
		end

		function func_add_table(a)
		    local sum = 0  
		    for i, v in ipairs(a) do  
		        sum = sum + v  
		    end  
		    return sum
		end

		function func_add_2(a)
		    local sum = 0  
		    for i, v in pairs(a) do  
		        sum = sum + v  
		    end  
		    return sum
		end  

		function func_return_multivalues()
			local update = false
			local strShort = 'false'
			print('intValue3=', intValue3)
			if intValue3 > 0
			then
			print('in if, intValue3=', intValue3)
				update = true
				strShort = 'true'
			end
			return update, strShort, {k1 = 11, k2 = 12}
		end

		function func_return_multivalues2()
			local update = false
			local strShort = 'false'
			print('intValue3=', intValue3)
			if intValue3 > 0
			then
			print('in if, intValue3=', intValue3)
				update = true
				strShort = 'true'
			end
			return update, strShort, {k1 = 11, k2 = 12}
		end

		function func_return_func()
			print('func_return_func called')
			return func_self_increase
		end

		function func_closure()
			local i = 0
		    return function()
		              i = i + 1
		              return i
				   end
		end

		function func_multi_params(a, b, c)
		    if a == true
			then
				b = b + 1
				c = string.lower(c)
			end
		    return a, b, c
		end

		function func_multi_params2(a, b)
			local sum = 0
		    if a == true
			then
				sum = b + 1
			end
		    return func_self_increase, a, sum
		end 

		function func_multi_params3(b, a)
			local sum = 0
		    if a == true
			then
				sum = b + 1 
			end
		    return sum, a
		end

		function func_multi_params4(b, a)
			local sum = 0
		    if a == true
			then
				sum = b + 1 
			end
		    return sum, a
		end 

		function func_varparams(...)
 			local arg={...}
			local sum = 0  
		    print(select('#', ...))
			for key, value in ipairs(arg)
			do
				sum = sum + value
				--print(value)
				--sum = sum + 1
			end
		    return sum
		end

		function func_readfile(file_name)
			local hFile = io.open(file_name, r)
			if hFile
			then
				local strContent = hFile:read('*all')
				print(strContent)
				return strContent
			else
				print('file is not exist')
			end
		return nil
		end
      
        function func_return_object(type)
            if type == 0 then
				local ret = io.open(CS.LuaTestCommon.xxxtdrfilepath, r)
				print('open A.lua.txt ret is ', ret)
				return ret
			end
            if type == 1 then
				return CS.LongStatic.LONG_MAX
			end
			if type == 2 then
				return CS.LongStatic.ULONG_MAX
			end
			return 0
		end
    ";
	
	public struct TestResult
	{
		public bool result;
		public string msg;
		public override string ToString()
		{
			return "Result: " + result.ToString () + ", Msg:" + msg;
		}
	}

	public TCForTestCSCallLua()
	{

	}

	public void setResult(bool result, string msg, out TestResult resultStruct)
	{
		resultStruct.result = result;
		resultStruct.msg = msg;
	}

	public void updateResult(bool result, string msg, ref TestResult resultStruct)
	{
		if (resultStruct.result == true) {
			resultStruct.msg = msg;
		} else {
			resultStruct.msg = resultStruct.msg + "\r\n" + msg;
		}
		resultStruct.result = result;
	}

	public byte[] AddLoader1(ref string filename)
	{
		if (filename == "InMemory")
		{
			string script = "return {ccc = 9999}";
			return System.Text.Encoding.UTF8.GetBytes(script);
		}
		return null;
	}

	public byte[] AddLoader2(ref string filename)
	{
		if (filename == "InFile") {
			string script = "filestring = 'addloader2'";
			return System.Text.Encoding.UTF8.GetBytes (script);
		} else if (filename == "other") {
			string script = "error('filename is error')";
			return System.Text.Encoding.UTF8.GetBytes (script);
		} else if (filename == "error_lua") {
			string script = "maths:abs(-10)";
			return System.Text.Encoding.UTF8.GetBytes (script);
		}
		return null;
	}

	public string listToString(List<int> a)
	{
		string result = "";
		foreach (int i in a) {
			result += i.ToString() + ",";
		}
		return result.TrimEnd(',');
	}

	public string listToStr(List<string> a)
	{
		string result = "";
		foreach (string i in a) {
			result += i + ",";
		}
		return result.TrimEnd(',');
	}
	
	public TestResult testDoString2LoadLua_Step_1()
	{
		/*
		 */
		string caseName = "testDoString2LoadLua_Step_1: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			string luaScript = @"
            --call standard lib
			require 'math'
			a = math.abs(-10)
			print('abs: ', a)
			";

			luaEnv.DoString (luaScript);
			setResult (true, "pass", out result);
		}  
		catch(Exception e)
		{  
			setResult(false, e.Message , out result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testDoString2LoadLua_Step_2()
	{
		/*
		 *
		 */
		string caseName = "testDoString2LoadLua_Step_2: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			string luaScript = @"
            checkFlag = false;
			a  = 100
			b = 1000
			if a < b then
				error('error raise')
			end
			";
			luaEnv.DoString (luaScript);
			setResult(false, "syntax error should be found, but there is no exception.", out result);
		}  
		catch(Exception e)
		{  
			setResult (true, "pass" , out result);
			LOG (e.Message);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testDoString2LoadLua_Step_3()
	{
		/*
		 *
		 */
		string caseName = "testDoString2LoadLua_Step_3: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			string luaScript = @"
            --call standard lib
			a = maths.abs(-10)
			print('abs: ', a)
			";
			luaEnv.DoString (luaScript);
			setResult(false, "exception should be raised, but there is no exception.", out result);
		}  
		catch(Exception e)
		{  
			setResult (true, "pass" , out result);
			LOG (e.Message);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testRequire2LoadLua_Step_1_3()
	{
		/*
		 */
		string caseName = "testRequire2LoadLua_Step_1_3: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			luaEnv.DoString("require 'A'");
			luaEnv.DoString("require 'testlua.B';require 'D'");
			luaEnv.DoString("require 'empty'");
			// luaEnv.DoString ("require 'testlua.main'");
			setResult (true, "pass", out result);
		}  
		catch(Exception e)
		{  
			setResult(false, e.Message , out result);
		}
		string dValue = luaEnv.Global.Get<string> ("strValue");
		LOG ("_G.strValue = " + dValue);
		if (dValue != "D.lua.txt") {
			setResult (false, result.msg + ", require should require d from Resources, but strValue=" + dValue, out result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testRequire2LoadLua_Step_4()
	{
		/*
		 */
		string caseName = "testRequire2LoadLua_Step_4: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			luaEnv.DoString ("require 'testlua.main'");
			setResult (true, "pass", out result);
		}  
		catch(Exception e)
		{  
			setResult(false, e.Message , out result);
		}
		string dValue = luaEnv.Global.Get<string> ("strValue");
		LOG ("_G.strValue = " + dValue);
		if (dValue != "D.lua.txt") {
			setResult (false, result.msg + ", require should require d from Resources, but strValue=" + dValue, out result);
		}

		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testRequire2LoadLua_Step_5()
	{
		/*
		 */
		string caseName = "testRequire2LoadLua_Step_5: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			luaEnv.DoString ("require 'E'");
			setResult(false, "E.lua in Resources path, but be loaded, no exception be raised.", out result);
		}  
		catch(Exception e)
		{  
			setResult (true, "pass" , out result);
			LOG (e.Message);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testRequire2LoadLua_Step_6()
	{
		/*
		 */
		string caseName = "testRequire2LoadLua_Step_6: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			luaEnv.DoString ("require 'F'");
			setResult(false, "F.lua deesn't exist, but no exception be raised.", out result);
		}  
		catch(Exception e)
		{  
			setResult (true, "pass" , out result);
			LOG (e.Message);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testRequire2LoadLua_Step_7()
	{
		/*
		 */
		string caseName = "testRequire2LoadLua_Step_7: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			luaEnv.DoString ("require 'error'");
			setResult(false, "there is syntax errors in error.lua, but no exception be raised.", out result);
		}  
		catch(Exception e)
		{  
			setResult (true, "pass" , out result);
			LOG (e.Message);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testAddLoader2LoadLua_Step_1_2()
	{
		/*
		 */
		string caseName = "testAddLoader2LoadLua_Step_1_2: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			luaEnv.AddLoader(AddLoader1);
			luaEnv.DoString("inmemory = require('InMemory').ccc");
			luaEnv.AddLoader(AddLoader2);
			luaEnv.DoString("require('InFile')");
			luaEnv.AddLoader(AddLoader2);
			luaEnv.DoString("require('InFile')");
			setResult (true, "pass", out result);
		}  
		catch(Exception e)
		{  
			setResult(false, e.Message , out result);
		}
		LuaTestCommon.Log ("InMemory.ccc=" + luaEnv.Global.Get<int> ("inmemory"));
        LuaTestCommon.Log("InFile.filestring=" + luaEnv.Global.Get<string>("filestring"));

		LOG (caseName + result.ToString());
		return result;
	} 

	public TestResult testAddLoader2LoadLua_Step_3()
	{
		/*
		 */
		string caseName = "testAddLoader2LoadLua_Step_3: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			luaEnv.AddLoader(AddLoader2);
			luaEnv.DoString(" require('other')");
			setResult(false, "AddLoader2 should raise exception, but it doesn't.", out result);
		}  
		catch(Exception e)
		{  
			setResult (true, "pass" , out result);
			LOG (e.Message);
		}
		
		LOG (caseName + result.ToString());
		return result;
	} 

	public TestResult testAddLoader2LoadLua_Step_6()
	{
		/*
		 */
		string caseName = "testAddLoader2LoadLua_Step_6: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			luaEnv.AddLoader(AddLoader2);
			luaEnv.DoString(" require('error_lua')");
			setResult(false, "AddLoader2 should raise exception, but it doesn't.", out result);
		}  
		catch(Exception e)
		{  
			setResult (true, "pass", out result);
			LOG (e.Message);
		}
		
		LOG (caseName + result.ToString());
		return result;
	} 

	public TestResult testAddLoader2LoadLua_Step_7()
	{
		/*
		 */
		string caseName = "testAddLoader2LoadLua_Step_7: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			luaEnv.AddLoader(AddLoader2);
            luaEnv.DoString(@"package.loaded['InFil'] = nil");
			luaEnv.DoString("require('InFile')");
			setResult (true, "pass", out result);
		}  
		catch(Exception e)
		{  
			setResult(false, e.Message , out result);
		}
		string filestring = luaEnv.Global.Get<string> ("filestring");
        if (filestring != "addloader2")
        {
            setResult(false, "shoulde load loader defined, but loadfile in Resources.", out result);
		}
		LuaTestCommon.Log ("InFile.filestring=" + filestring);
		
		LOG (caseName + result.ToString());
		return result;
	} 

	public TestResult testGetBasicDataTypeBool_Step_1()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeBool_step_1: ";
		LOG ("*************" + caseName);
		TestResult result;

		luaEnv.DoString(script);

		bool falseVar = luaEnv.Global.Get<bool> ("bValue1");
		bool trueVar = luaEnv.Global.Get<bool> ("bValue2");
		LOG ("bValue1=" + falseVar + "; bValue2=" + trueVar);
		if (falseVar == false && trueVar == true) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "fail", out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeString_Step_2()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeString_step_2: ";
		LOG ("*************" + caseName);
		TestResult result;

		luaEnv.DoString(script);
		
		string emptyStr = luaEnv.Global.Get<string> ("strValueEmpty");
		string longStr = luaEnv.Global.Get<string> ("strValueLong");
		string shortStr = luaEnv.Global.Get<string> ("strValueShort");
		string expStr = luaEnv.Global.Get<string> ("strValueExp");
		string zhuanyiStr = luaEnv.Global.Get<string> ("strValue5"); 
		string linesStr = luaEnv.Global.Get<string> ("strValue6"); 
		string chinStr = luaEnv.Global.Get<string> ("strValueChin"); 
		string compStr = luaEnv.Global.Get<string> ("strValueComp"); 
		string huoxingStr = luaEnv.Global.Get<string> ("strValueHoX"); 

		LOG ("strValueEmpty=" + emptyStr + "; strValueLong length=" + longStr.Length);
		LOG ("strValueShort=" + shortStr + "; trValueExp=" + expStr);
		LOG ("strValue5=" + zhuanyiStr + "; strValue6=" + linesStr);
		LOG ("strValueChin=" + chinStr + "; strValueComp=" + compStr);
		LOG ("strValueHoX=" + huoxingStr);

		if (emptyStr == "") {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).empty string isn't empty, is " + emptyStr, out result);
		}
		if (longStr.Length != 1024) {
			updateResult(false, "(2). long string's length should be 1024, but is " + longStr.Length, ref result);
		}
		if (shortStr != "boo123") {
			updateResult(false, "(3). short string should be bool123 but is " + shortStr, ref result);
		}
		if (expStr != ".* ? [ ] ^ $~`!@#$%^&()_-+=[];\',““〈〉〖【℃ ＄ ¤№ ☆ ★■ⅷ②㈣") {
			updateResult(false, "(4). complex string is wrong, which is  " + expStr, ref result);
		}
		if (zhuanyiStr != "\r\na") {
			updateResult(false, "(5).zhuanyi string is wrong, which is  " + zhuanyiStr, ref result);
		}
		if (linesStr.Split('\n').Length !=8) {
			updateResult(false, "(6).multi lines string should have 8 liens, but it is  " + linesStr.Split('\n').Length, ref result);
		}
		if (chinStr != "中文字符串") {
			updateResult(false, "(7).chinese string is wrong, which is " + chinStr, ref result);
		}
		if (compStr != "繁體國陸") {
			updateResult(false, "(8).fanti string is wrong, which is " + compStr, ref result);
		}
		if (huoxingStr != "吙煋呅僦媞這樣孒") {
			updateResult(false, "(9).huoxing string is wrong, which is " + huoxingStr, ref result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToSByte()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToSByte: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> sbyte
		sbyte sbyteValueMax = luaEnv.Global.Get<sbyte> ("sbyteValueMax");
		sbyte sbyteValueMin = luaEnv.Global.Get<sbyte> ("sbyteValueMin");
		sbyte sbyteOverMax = luaEnv.Global.Get<sbyte> ("intValueMax");
		sbyte sbyteOverMin = luaEnv.Global.Get<sbyte> ("intValueMin");
		LOG ("sbyteValueMax=" + sbyteValueMax + "; sbyteValueMin=" + sbyteValueMin);
		LOG ("sbyteOverMax=" + sbyteOverMax + "; sbyteOverMin=" + sbyteOverMin);
		
		if (sbyteValueMax == System.SByte.MaxValue) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).sbyteValueMax, is " + sbyteValueMax, out result);
		}
		if (sbyteValueMin != System.SByte.MinValue) {
			updateResult(false, "(2).sbyteValueMin, is " + sbyteValueMin, ref result);
		}
		if (sbyteOverMax != -1) {
			updateResult(false, "(3).sbyteOverMax, is " + sbyteOverMax, ref result);
		}
		if (sbyteOverMin != 0) {
			updateResult(false, "(4).sbyteOverMin, is " + sbyteOverMin, ref result);
		}

		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToByte()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToByte: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> byte
		byte byteValueMax = luaEnv.Global.Get<byte> ("byteValueMax");
		byte byteValueMin = luaEnv.Global.Get<byte> ("byteValueMin");
		byte byteOverMax = luaEnv.Global.Get<byte> ("intValueMax");
		byte byteOverMin = luaEnv.Global.Get<byte> ("intValueMin");
		LOG ("byteValueMax=" + byteValueMax + "; byteValueMin=" + byteValueMin);
		LOG ("byteOverMax=" + byteOverMax + "; byteOverMin=" + byteOverMin);
		
		if (byteValueMax == System.Byte.MaxValue) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).byteValueMax, is " + byteValueMax, out result);
		}
		if (byteValueMin != System.Byte.MinValue) {
			updateResult(false, "(2).byteValueMin, is " + byteValueMin, ref result);
		}
		if (byteOverMax != 255) {
			updateResult(false, "(3).byteOverMax, is " + byteOverMax, ref result);
		}
		if (byteOverMin != 0) {
			updateResult(false, "(4).sbyteOverMin, is " + byteOverMin, ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToShort()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToShort: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> short
		short shortValueMax = luaEnv.Global.Get<short> ("shortValueMax");
		short shortValueMin = luaEnv.Global.Get<short> ("shortValueMin");

		LOG ("shortValueMax=" + shortValueMax + "; shortValueMin=" + shortValueMin);
		
		if (shortValueMax == System.Int16.MaxValue) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).shortValueMax, is " + shortValueMax, out result);
		}
		if (shortValueMin != System.Int16.MinValue) {
			updateResult(false, "(2).shortValueMin, is " + shortValueMin, ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToUShort()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToUShort: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> ushort
		ushort ushortValueMax = luaEnv.Global.Get<ushort> ("ushortValueMax");
		ushort ushortValueMid = luaEnv.Global.Get<ushort> ("ushortValueMid");
		
		LOG ("ushortValueMax=" + ushortValueMax + "; shortValueMin=" + ushortValueMid);
		
		if (ushortValueMax == System.UInt16.MaxValue) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).ushortValueMax, is " + ushortValueMax, out result);
		}
		if (ushortValueMid != 32768) {
			updateResult(false, "(2).ushortValueMid, is " + ushortValueMid, ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToInt()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToInt: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> int
		int intValueMax = luaEnv.Global.Get<int> ("intValueMax");
		int intValueMin = luaEnv.Global.Get<int> ("intValueMin");
		int intZero = luaEnv.Global.Get<int> ("intZero");
		
		LOG ("intValueMax=" + intValueMax + "; intValueMin=" + intValueMin);

		if (intValueMax == System.Int32.MaxValue) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).intValueMax, is " + intValueMax, out result);
		}
		if (intValueMin != System.Int32.MinValue) {
			updateResult(false, "(2).intValueMin, is " + intValueMin, ref result);
		}
		if (intZero != 0) {
			updateResult(false, "(3).intZero, is " + intZero, ref result);
		}

		try {
			int intValueOverMax = luaEnv.Global.Get<int> ("longValue");
			int intValueOverMin = luaEnv.Global.Get<int> ("uintValueMax");
			LOG ("intValueOverMax=" + intValueOverMax + "; intValueOverMin=" + intValueOverMin);
		} catch(Exception e) {
			updateResult(false, "it shouldn't raise exception, but e.msg:" + e.Message, ref result);
		}
	
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToUInt()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToUInt: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> uint
		uint uintValueMax = luaEnv.Global.Get<uint> ("uintValueMax");
		uint uintValueMid = luaEnv.Global.Get<uint> ("uintValueMid");
		
		LOG ("uintValueMax=" + uintValueMax + "; uintValueMid=" + uintValueMid);
		
		if (uintValueMax == System.UInt32.MaxValue) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).uintValueMax, is " + uintValueMax, out result);
		}
		if (uintValueMid != 2147483648) {
			updateResult(false, "(2).uintValueMid, is " + uintValueMid, ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToLong()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToLong: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> long
		long longValue = luaEnv.Global.Get<long> ("longValue");
		long longValue2 = luaEnv.Global.Get<long> ("longValue2");
		
		LOG ("longValue=" + longValue + "; longValue2=" + longValue2);
        LuaTestCommon.Log("longValue=" + longValue + "; longValue2=" + longValue2);
		
		if (longValue == 42949672960) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).longValue, is " + longValue, out result);
		}
		if (longValue2 != -42949672960) {
			updateResult(false, "(2).longValue2, is " + longValue2, ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToULong()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToULong: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> ulong
		ulong ulongValueMax = luaEnv.Global.Get<ulong> ("ulongValueMax");
		ulong ulongValueMin = luaEnv.Global.Get<ulong> ("ulongValueMin");
		
		LOG ("ulongValueMax=" + ulongValueMax + "; ulongValueMin=" + ulongValueMin);
		
		if (ulongValueMax == 42949672961111) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).ulongValueMax, is " + ulongValueMax, out result);
		}
		if (ulongValueMin != 0) {
			updateResult(false, "(2).ulongValueMin, is " + ulongValueMin, ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToDouble()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToDouble: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> double
		double doubleValue = luaEnv.Global.Get<double> ("doubleValue");
		double doubleValue2 = luaEnv.Global.Get<double> ("doubleValue2");
		
		LOG ("doubleValue=" + doubleValue + "; doubleValue2=" + doubleValue2);
		
		if (doubleValue == 3.14159265) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).doubleValue, is " + doubleValue, out result);
		}
		if (doubleValue2 != -3.14159265) {
			updateResult(false, "(2).doubleValue2, is " + doubleValue2, ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToChar()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToChar: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> char
		char charValue = luaEnv.Global.Get<char> ("charValue");
		char charValue2 = luaEnv.Global.Get<char> ("charValue2");
		
		LOG ("charValue=" + charValue + "; charValue2=" + charValue2);
		
		if (charValue == 'a') {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).charValue, is " + charValue, out result);
		}
		if (charValue2 != 'A') {
			updateResult(false, "(2).charValue2, is " + charValue2, ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToFloat()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToFloat: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> float
		float floatValue = luaEnv.Global.Get<float> ("floatValue");
		float floatValue2 = luaEnv.Global.Get<float> ("floatValue2");
		
		LOG ("floatValue=" + floatValue.ToString("F8") + "; charValue2=" + floatValue2);

		if (System.Math.Abs(floatValue - 3.141593) < 0.000001) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).floatValue, is " + floatValue, out result);
		}
		if (System.Math.Abs(floatValue + 3.14) < 0.000001) {
			updateResult(false, "(2).floatValue2, is " + floatValue2, ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataTypeNumberToDecimal()
	{
		/*
		 */
		string caseName = "testGetBasicDataTypeNumberToDecimal: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		// number -> decimal
		decimal decValue = luaEnv.Global.Get<decimal> ("decValue");
		decimal decValue2 = luaEnv.Global.Get<decimal> ("decValue2");
		decimal expectValue = 12.3111111111m;
		decimal expectValue2 = -12.3111111111m;

		LOG ("decValue=" + decValue + "; decValue2=" + decValue2);
		
		if (decValue == expectValue) {
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).decValue, is " + decValue, out result);
		}
		if (decValue2 != expectValue2) {
			updateResult(false, "(2).decValue2, is " + decValue2, ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetBasicDataType_Step_4()
	{
		/*
		 */
		string caseName = "testGetBasicDataType_Step_4: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			luaEnv.DoString(script);
			int noExistInt = luaEnv.Global.Get<int> ("noExistValue");
			string noExistString = luaEnv.Global.Get<string> ("noExistString");
			LOG ("noExistInt=" + noExistInt + "; noExistString=" + noExistString);
			//setResult (false, "no exist value should raise exception", out result);
			//setResult(false, "there is no var defined, should raise exception, but it doesn't.", out result);
			setResult (false, "v2.1.3 change,noExistValue,  only value type and nil value will raise exception", out result);
		}  
		catch(Exception e)
		{  
			LOG(e.Message);
			setResult (true, "pass", out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	} 

	public TestResult testGetBasicDataType_Step_5()
	{
		/*
		 */
		string caseName = "testGetBasicDataType_Step_5: ";
		LOG ("*************" + caseName);
		TestResult result;
		try  
		{  
			luaEnv.DoString(script);
			luaEnv.DoString("str2int='123'");
			int string2Int = luaEnv.Global.Get<int> ("str2int");
			/*string number2str = luaEnv.Global.Get<string>("byteValueMax");
			LOG ("string2Int=" + string2Int + ", number2str=" + number2str);

			int bool2int = luaEnv.Global.Get<int> ("bValue2");
			string bool2str = luaEnv.Global.Get<string> ("bValue1");
			LOG ("bool2int=" + bool2int + ", bool2str=" + bool2str);*/
			//setResult (false, "type is not same, should raise exception", out result);
			// setResult(false, "there is no var defined, should raise exception, but it doesn't.", out result);
			setResult (true, "pass", out result);
		}  
		catch(Exception e)
		{  
			LOG(e.Message);
			setResult (false, "v2.1.3 change, type is not same, only value type and nil value will raise exception", out result);
		}

		LOG (caseName + result.ToString());
		return result;
	} 

	public TestResult testGetTableToClass_Step_1()
	{
		/*
		 */
		string caseName = "testGetTableToClass_Step_1: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);

		tableValue1ClassEqual tb1Class = luaEnv.Global.Get<tableValue1ClassEqual> ("tableValue1");
		List<int> tableValueIncludeList = new List<int>(){1,2,3,4,5,6,7,8,9,0};
		LOG ("tableValue1.key1 = " + tb1Class.key1 + "; tableValue1.key2= " + tb1Class.key2);
		LOG ("tableValue1.key3 = " + tb1Class.key3 + "; tableValue1.tableValueInclude= " + 
		           listToString(tb1Class.tableValueInclude));
	
		if (tb1Class.key1 == 100000){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).tableValue1.key1 should be 100000, but int fact is : " + tb1Class.key1 + "", out result);
		}
		if (tb1Class.key2 != 10) {
			updateResult(false, "(2).tableValue1.key2 should be 10, but in fact is " + tb1Class.key2 , ref result);
		}
		if (tb1Class.key3 != true) {
			updateResult(false, "(3).tableValue1.key3 should be true, but in fact is " + tb1Class.key3 , ref result);
		}
		if (tb1Class.tableValueInclude.Count != tableValueIncludeList.Count || 
		    tb1Class.tableValueInclude[0] != tableValueIncludeList[0]) {
			updateResult(false, "(4).tableValue1.tableValueInclude should be {1,2,3,4,5,6,7,8,9,0}," +
			             "but in fact is " +  listToString(tb1Class.tableValueInclude), ref result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToClass_Step_2()
	{
		/*
		 */
		string caseName = "testGetTableToClass_Step_2: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		tableValue1ClassLess tb1Class = luaEnv.Global.Get<tableValue1ClassLess> ("tableValue1");
		LOG ("tableValue1.key1 = " + tb1Class.key1 + "; tableValue1.key2= " + tb1Class.key2);
		
		if (tb1Class.key1 == 100000){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).tableValue1.key1 should be 100000, but int fact is : " + tb1Class.key1 + "", out result);
		}
		if (tb1Class.key2 != 10) {
			updateResult(false, "(2).tableValue1.key2 should be 10, but in fact is " + tb1Class.key2 , ref result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToClass_Step_3()
	{
		/*
		 */
		string caseName = "testGetTableToClass_Step_3: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		tableValue1ClassMore tb1Class = luaEnv.Global.Get<tableValue1ClassMore> ("tableValue1");

		LOG ("tableValue1.key1 = " + tb1Class.key1 + "; tableValue1.key2 = " + tb1Class.key2);
		LOG ("tableValue1.key3 = " + tb1Class.key3 + "; tableValue1ClassMore.key4 = " +  tb1Class.key4);
		
		if (tb1Class.key1 == 100000){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).tableValue1.key1 should be 100000, but int fact is : " + tb1Class.key1 + "", out result);
		}
		if (tb1Class.key2 != 10) {
			updateResult(false, "(2).tableValue1.key2 should be 10, but in fact is " + tb1Class.key2 , ref result);
		}
		if (tb1Class.key3 != true) {
			updateResult(false, "(3).tableValue1.key3 should be true, but in fact is " + tb1Class.key3 , ref result);
		}
		if (tb1Class.key4 != 0) {
			updateResult(false, "(4).key4 should be 0 default, but in fact is " + tb1Class.key4 , ref result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToClass_Step_4()
	{
		/*
		 */
		string caseName = "testGetTableToClass_Step_4: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		tableValue1ClassMore tb1Class = luaEnv.Global.Get<tableValue1ClassMore> ("tableValue1");
		
		LOG ("tableValue1.key1 = " + tb1Class.key1 + "; tableValue1.key2 = " + tb1Class.key2);
		LOG ("tableValue1.key3 = " + tb1Class.key3 + "; tableValue1ClassMore.key4 = " +  tb1Class.key4);

		if (tb1Class.key1 == 100000){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).tableValue1.key1 should be 100000, but int fact is : " + tb1Class.key1 + "", out result);
		}

		tb1Class.key1 = 100;
		tableValue1ClassMore tb1Class2 = luaEnv.Global.Get<tableValue1ClassMore> ("tableValue1");
		LOG ("tableValue1.key1 = " + tb1Class2.key1);
		if (tb1Class2.key1 != 100000){
			updateResult(false, "(2).tableValue1.key1 should be 100000, but int fact is" + tb1Class2.key1 , ref result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToClass_Step_5()
	{
		/*
		 */
		string caseName = "testGetTableToClass_Step_5: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		tableValue1ClassPrivate tb1Class = luaEnv.Global.Get<tableValue1ClassPrivate> ("tableValue1");
		
		LOG ("tableValue1.key1 = " + tb1Class.key1 + "; tableValue1.key2 = " + tb1Class.Get());
		LOG ("tableValue1.key3 = " + tb1Class.key3);
		
		if (tb1Class.key1 == 100000){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).tableValue1.key1 should be 100000, but int fact is : " + tb1Class.key1 + "", out result);
		}
		if (tb1Class.Get () != 0){
			updateResult(false, "(2).private key2 should be 0, but int fact is " + tb1Class.Get () , ref result);
		}
		if (tb1Class.key3 != true) {
			updateResult(false, "(3).tableValue1.key3 should be true, but in fact is " + tb1Class.key3 , ref result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToClass_Step_1_1()
	{
		/*
		 */
		string caseName = "testGetTableToClass_Step_1_1: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		try{

			tableValue1ClassParamConstucter tb1Class = luaEnv.Global.Get<tableValue1ClassParamConstucter> ("tableValue1");
			// LOG ("tableValue1.key1 = " + tb1Class.key1 + "; tableValue1.key2 = " + tb1Class.key2);
			// LOG ("tableValue1.key3 = " + tb1Class.key3);
			setResult (true, "pass", out result);
		} catch(Exception e){
			setResult (false, "v2.1.3change, only value type and nil value will raise exception ,class has no non-param constructor will not", out result);
			LOG (e.Message);
		}

		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToClass_Step_1_3()
	{
		/*
		 */
		string caseName = "testGetTableToClass_Step_1_3: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		tableValue1ClassTwoConstructer tb1Class = luaEnv.Global.Get<tableValue1ClassTwoConstructer> ("tableValue1");
		
		LOG ("tableValue1.key1 = " + tb1Class.key1 + "; tableValue1.key2 = " + tb1Class.key2);
		LOG ("tableValue1.key3 = " + tb1Class.key3 );
		
		if (tb1Class.key1 == 100000){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).tableValue1.key1 should be 100000, but int fact is : " + tb1Class.key1 + "", out result);
		}
		if (tb1Class.key2 != 10) {
			updateResult(false, "(2).tableValue1.key2 should be 10, but in fact is " + tb1Class.key2 , ref result);
		}
		if (tb1Class.key3 != true) {
			updateResult(false, "(3).tableValue1.key3 should be true, but in fact is " + tb1Class.key3 , ref result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToClass_Step_1_4()
	{
		/*
		 */
		string caseName = "testGetTableToClass_Step_1_4: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		try{
			tableValue1ClassException tb1Class = luaEnv.Global.Get<tableValue1ClassException> ("tableValue1");
			setResult (false, "class constructor should raise exception, but it doesnpt", out result);
		} catch(Exception e){
			setResult (true, "pass", out result);
			LOG (e.Message);
		}

		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToInterface_Step_6()
	{

		string caseName = "testGetTableToInterface_Step_6: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		tableValue1InfEqual tb1Inf = luaEnv.Global.Get<tableValue1InfEqual> ("tableValue1");
		LOG ("tableValue1.key1 = " + tb1Inf.key1 + "; tableValue1.key2 = " + tb1Inf.key2);
		LOG ("tableValue1.key3 = " + tb1Inf.key3 );
		LOG ("tableValue1.tableVarInclude.ikey1 = " + tb1Inf.tableVarInclude.ikey1 +
		           "; tableValue1.tableVarInclude.ikey2 = " + tb1Inf.tableVarInclude.ikey2);

		if (tb1Inf.key1 == 100000){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).tableValue1.key1 should be 100000, but int fact is : " + tb1Inf.key1 + "", out result);
		}
		if (tb1Inf.key2 != 10) {
			updateResult(false, "(2).tableValue1.key2 should be 10, but in fact is " + tb1Inf.key2 , ref result);
		}
		if (tb1Inf.key3 != true) {
			updateResult(false, "(3).tableValue1.key3 should be true, but in fact is " + tb1Inf.key3 , ref result);
		}
		if (tb1Inf.tableVarInclude.ikey1 != 10) {
			updateResult(false, "(4).tableValue1.tableVarInclude.ikey1 should be 10, but in fact is " + tb1Inf.tableVarInclude.ikey1 , ref result);
		}
		if (tb1Inf.tableVarInclude.ikey2 != 12) {
			updateResult(false, "(5).tableValue1.tableVarInclude.ikey2 should be 12, but in fact is " + tb1Inf.tableVarInclude.ikey2, ref result);
		}
		int subResult = tb1Inf.sub (1000, 100);
		if (subResult != 900 ) {
			updateResult(false, "(6).sub result is error, 1000 - 100 =  " + subResult , ref result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToInterface_Step_7()
	{
		/*
		 */
		string caseName = "testGetTableToInterface_Step_7: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		tableValue1InfLess tb1Inf = luaEnv.Global.Get<tableValue1InfLess> ("tableValue1");
		LOG ("tableValue1.key1 = " + tb1Inf.key1 + "; tableValue1.key2 = " + tb1Inf.key2);
		
		if (tb1Inf.key1 == 100000){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).tableValue1.key1 should be 100000, but int fact is : " + tb1Inf.key1 + "", out result);
		}
		if (tb1Inf.key2 != 10) {
			updateResult(false, "(2).tableValue1.key2 should be 10, but in fact is " + tb1Inf.key2 , ref result);
		}

		int subResult = tb1Inf.sub (1000, 100);
		if (subResult != 900 ) {
			updateResult(false, "(3).sub result is error, 1000 - 100 =  " + subResult , ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToInterface_Step_8()
	{
		/*
		 */
		string caseName = "testGetTableToInterface_Step_8: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		tableValue1InfMore tb1Inf = luaEnv.Global.Get<tableValue1InfMore> ("tableValue1");
		LOG ("tableValue1InfMore.key4 = " + tb1Inf.key4);
		
		if (tb1Inf.key4 == null){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "(1).key4 should be '' default, but int fact is : " + tb1Inf.key4 + "", out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToInterface_Step_9()
	{
		/*
		 */
		string caseName = "testGetTableToInterface_Step_9: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		tableValue2Inf tb1Inf = luaEnv.Global.Get<tableValue2Inf> ("tableValue2");
		LOG ("tableValue2.kv1 = " + tb1Inf.kv1 + "; tableValue2.kv2 = " + tb1Inf.kv2);
		LOG ("tableValue2.kv3 = " + tb1Inf.kv3 );

		tb1Inf.kv1 = false;
		tb1Inf.kv2 = "test";
		tb1Inf.kv3 = 3;
		tableValue2Inf tb1Inf2 = luaEnv.Global.Get<tableValue2Inf> ("tableValue2");
		LOG ("tableValue2.kv1 = " + tb1Inf2.kv1 + "; tableValue2.kv2 = " + tb1Inf2.kv2);
		LOG ("tableValue2.kv3 = " + tb1Inf2.kv3 );

		if (tb1Inf2.kv1 == false && tb1Inf2.kv2 == "test" && tb1Inf2.kv3 == 3){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "by ref, kv1, kv2, kv3 = " + tb1Inf2.kv1 +
			           ", " + tb1Inf2.kv2 + ", " + tb1Inf2.kv3, out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToInterface_Step_6_1()
	{
		/*
		 */
		string caseName = "testGetTableToInterface_Step_6_1: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		try{
			tableValue1InfTypeDiff tb1Inf = luaEnv.Global.Get<tableValue1InfTypeDiff> ("tableValue1");
			LOG ("tableValue1.key1 = " + tb1Inf.key1 + "; tableValue1.key2 = " + tb1Inf.key2);

			tableValue1InfLess tb2Inf = luaEnv.Global.Get<tableValue1InfLess> ("tableValue7");
			LOG ("tableValue1.key1 = " + tb2Inf.key1 + "; tableValue2.key2 = " + tb2Inf.key2);
			setResult (true, "pass", out result);

		} catch(Exception e){

			setResult (false, "class constructor should raise exception, but it doesnpt", out result);
			LOG (e.Message);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToDic_Step_10()
	{
		/*
		 */
		string caseName = "testGetTableToDic_Step_10: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		Dictionary<string, int> dict = luaEnv.Global.Get<Dictionary<string, int>> ("tableValue4");
		LOG ("tableValue4.k1 = " + dict["k1"] + "; tableValue4.k2 = " + dict["k2"]);
		LOG ("tableValue4.k3 = " + dict["k3"] + "; tableValue4.k4 = " + dict["k4"]);
		LOG ("tableValue4.k5 = " + dict["k5"]);
		
		if (dict["k1"]  == 1 && dict["k2"] == 10 && dict["k3"] == 100 && dict["k4"] == 1000
		    && dict["k5"] == 10000){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "tableValue4 should be k1= 1, k2= 10, k3 = 100, k4 = 1000, k5 = 10000", out result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToDic_Step_11_1()
	{
		/*
		 */
		string caseName = "testGetTableToDic_Step_11_1: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		Dictionary<int, string> dict = luaEnv.Global.Get<Dictionary<int, string>> ("tableValue4");
		LOG ("dict.Count = " + dict.Count);
		
		if (dict.Count == 0){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "dict should be {}", out result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToDic_Step_11_2()
	{
		/*
		 */
		string caseName = "testGetTableToDic_Step_11_2: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		Dictionary<string, int> dict = luaEnv.Global.Get<Dictionary<string, int>> ("tableValue1");
		LOG ("tableValue1.key1 = " + dict["key1"] + ", tableValue1.key2 = " + dict["key2"]);
		LOG ("dict.Count = " + dict.Count);

		if (dict.Count == 2 && dict["key1"] == 100000 && dict["key2"] == 10){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "dict should have key1 = 100000, key2 = 10", out result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToDic_Step_11_3()
	{
		/*
		 */
		string caseName = "testGetTableToDic_Step_11_3: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		Dictionary<int, int> dict = luaEnv.Global.Get<Dictionary<int, int>> ("tableValue3");
		LOG ("dict.Count = " + dict.Count);
		
		if (dict.Count == 0){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "dict should {}", out result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToList_Step_12()
	{
		/*
		 */
		string caseName = "testGetTableToList_Step_12: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		List<string> listVar = luaEnv.Global.Get<List<string>> ("tableValue3");
		LOG ("listVar = " + listToStr(listVar));
		
		if (listVar.Count == 8){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "listVar should be {'apple', 'banana', 'orange', 'kiwi', " +
				"'grape', 'lemon', 'strawberry', 'pear'}", out result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToList_Step_13_1_int()
	{
		/*
		 */
		string caseName = "testGetTableToList_Step_13_1_int: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		List<int> listVar = luaEnv.Global.Get<List<int>> ("tableValue3");
		LOG ("listVar = " + listToString(listVar));
		
		if (listVar.Count == 0){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "listVar should be {}", out result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToList_Step_13_1_string()
	{
		/*
		 */
		string caseName = "testGetTableToList_Step_13_1_string: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		List<string> listVar = luaEnv.Global.Get<List<string>> ("tableValue6");
		LOG ("listVar.count = " + listVar.Count);
		
		if (listVar.Count == 0){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "listVar should be {}", out result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToList_Step_13_2()
	{
		/*
		 */
		string caseName = "testGetTableToList_Step_13_2: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		List<string> listVar = luaEnv.Global.Get<List<string>> ("tableValue1");
		LOG ("listVar.count = " + listVar.Count + ", listVar = " + listToStr(listVar));
		
		if (listVar.Count == 3){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "listVar should be {'red', 'yellow', 'green''}", out result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetTableToLuaTable_Step_14()
	{
		/*
		 */
		string caseName = "testGetTableToLuaTable_Step_14: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		LuaTable table = luaEnv.Global.Get<LuaTable> ("tableValue1");
		LOG ("tableValue1.key1 = " + table.Get<int>("key1"));
		LOG ("tableValue1.key2 = " + table.Get<int>("key2"));
		LOG ("tableValue1.key3 = " + table.Get<bool>("key3"));
		LOG ("tableValue1.1 = " + table[1]);
		LOG ("tableValue1.2 = " + table[2]);
		LOG ("tableValue1.3 = " + table[3]);
		LOG ("tableValue1.sub = " + Convert.ToInt32(table.Get<LuaFunction>("sub").Call (table, 100, 10)[0]));

		LOG ("tableValue1.tableValueInclude.count = " + table.Get<List<int>>("tableValueInclude").Count);
		LOG ("tableValue1.tableVarInclude.count = " + table.Get<Dictionary<string, int>>("tableVarInclude").Count);
		if (table.Get<int>("key1") == 100000){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "tablevalue1 is error", out result);
		}

		table ["key1"] = 100;
		LuaTable table2 = luaEnv.Global.Get<LuaTable> ("tableValue1");
		LOG ("tableValue1.key1 = " + table2.Get<int>("key1"));
		if (Convert.ToInt32(table2["key1"]) != 100) {
			updateResult(false, "modify no change.", ref result);
		}
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToDelegate_Step_1()
	{
		/*
		 */
		string caseName = "testGetFuncToDelegate_Step_1: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FuncSelfINcreaseDelegate delegate1 = luaEnv.Global.Get<FuncSelfINcreaseDelegate> ("func_self_increase");
		delegate1 ();
		int intValue3 = luaEnv.Global.Get<int> ("intValue3");
		LOG ("intValues = " + intValue3);
		if (intValue3 == 1){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "intValue3 + 1 = 1, but is " + intValue3, out result);
		}
	
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToDelegate_Step_2_1()
	{
		/*
		 */
		string caseName = "testGetFuncToDelegate_Step_2_1: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FuncReturnMultivaluesDelegate2 delegate1 = luaEnv.Global.Get<FuncReturnMultivaluesDelegate2> ("func_return_multivalues");
		string str;
		tableValue4Class table;
		bool update;
		delegate1(out update, out str, out table);

		LOG ("str = " + str + ", update = " + update + ", {k1 = " + 
		           table.k1 + ", k2 = " + table.k2 + "}");

		if (update == false && str == "false" && table.k1 == 11 && table.k2 == 12){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "function return error, should be false, 'false', {k1=11, k2=12}", out result);
		}

		FuncSelfINcreaseDelegate delegate2 = luaEnv.Global.Get<FuncSelfINcreaseDelegate> ("func_self_increase");
		delegate2 ();
		// int intValue3 = luaEnv.Global.Get<int> ("intValue3");
		// intValue3 = 3;

		delegate1(out update, out str, out table);
		LOG ("get again, str = " + str + ", update = " + update + ", {k1 = " + 
		           table.k1 + ", k2 = " + table.k2 + "}");
		if (update != true || str != "true" || table.k1 != 11 || table.k2 != 12){
			updateResult (false, "function return error, should be true, 'true', {k1=11, k2=12}", ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToDelegate_Step_2_2()
	{
		/*
		 */
		string caseName = "testGetFuncToDelegate_Step_2_2: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FuncReturnMultivaluesDelegate1 delegate1 = luaEnv.Global.Get<FuncReturnMultivaluesDelegate1> ("func_return_multivalues");
		string str;
		tableValue4Class table;
		bool update = delegate1(out str, out table);

		if (update == false && str == "false" && table.k1 == 11 && table.k2 == 12){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "function return error, should be false, 'false', {k1=11, k2=12}", out result);
		}

		FuncSelfINcreaseDelegate delegate2 = luaEnv.Global.Get<FuncSelfINcreaseDelegate> ("func_self_increase");
		delegate2 ();
		update = delegate1(out str, out table);

		LOG ("get again, str = " + str + ", update = " + update + ", {k1 = " + 
		           table.k1 + ", k2 = " + table.k2 + "}");
		if (update != true || str != "true" || table.k1 != 11 || table.k2 != 12){
			updateResult (false, "function return error, should be true, 'true', {k1=11, k2=12}", ref result);
		}

		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToDelegate_Step_2_3()
	{
		/*
		 */
		string caseName = "testGetFuncToDelegate_Step_2_3: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FuncReturnMultivaluesDelegate3 delegate1 = luaEnv.Global.Get<FuncReturnMultivaluesDelegate3> ("func_return_multivalues");
		int a;
		string str;
		tableValue4Class table;
		bool update;
		delegate1(out update, out str, out table, out a);
		
		LOG ("str = " + str + ", update = " + update + ", {k1 = " + 
		           table.k1 + ", k2 = " + table.k2 + "}");
		
		if (update == false && str == "false" && table.k1 == 11 && table.k2 == 12 && a == 0){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "function return error, should be false, 'false', {k1=11, k2=12}", out result);
		}

		FuncReturnMultivaluesDelegate31 delegate2 = luaEnv.Global.Get<FuncReturnMultivaluesDelegate31> ("func_return_multivalues2");
		tableValue4Class noreturn;
		delegate2(out update, out str, out table, out noreturn);
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToDelegate_Step_2_4()
	{
		/*
		 */
		string caseName = "testGetFuncToDelegate_Step_2_4: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FuncReturnMultivaluesDelegate4 delegate1 = luaEnv.Global.Get<FuncReturnMultivaluesDelegate4> ("func_return_multivalues");
		string str;
		bool update = delegate1(out str);
		LOG ("str = " + str + ", update = " + update);

		if (update == false && str == "false"){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "function return error, should be false, 'false'", out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}
	
	public TestResult testGetFuncToDelegate_Step_3()
	{
		/*
		 */
		string caseName = "testGetFuncToDelegate_Step_3: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		GetFuncIncreaseDelegate delegate1 = luaEnv.Global.Get<GetFuncIncreaseDelegate> ("func_return_func");
        Action e = delegate1();
		e ();
		int intValue3 = luaEnv.Global.Get<int>("intValue3");
		if ( intValue3 == 1){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "intValue3 should be 1, but is" + intValue3, out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToDelegate_Step_5()
	{
		/*
		 */
		string caseName = "testGetFuncToDelegate_Step_5: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FuncMultiParamsDelegate delegate1 = luaEnv.Global.Get<FuncMultiParamsDelegate> ("func_multi_params");
		bool a = true;
		int b = 2;
		string c = "ABC";
		int d;
		delegate1(ref a, ref b, ref c, out d);
		LOG ("a = " + a + ", b = " + b + ", c = " + c);
		if (a == true && b == 3 && c == "abc"){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "function return error, should be true, 3, 'abc'", out result);
		}
		
		a = false;
		b = 2;
		c = "ABC";
		delegate1(ref a, ref b, ref c, out d);
		if (a != false || b != 2 || c != "ABC") {
			updateResult (false, "function return error, should be false, 2, 'ABC'", ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	FuncMultiParams3Delegate2 delegate1;

	public TestResult testGetFuncToDelegate_Step_5_1()
	{
		/*
		 */
		string caseName = "testGetFuncToDelegate_Step_5_1: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		delegate1 = luaEnv.Global.Get<FuncMultiParams3Delegate2> ("func_multi_params3");
		bool a = true;
		int b = 5;
		int sum;
		delegate1(b, out sum, ref a);

		LOG ("sum = " + sum + ", a = " + a);
		if (a == true && sum == 6 ){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "should be sum = 6, a = true", out result);
		}

		try {
			FuncMultiParams3Delegate delegate2 = luaEnv.Global.Get<FuncMultiParams3Delegate> ("func_multi_params3");
		}		
		catch (Exception e){
            updateResult(false, "FuncMultiParams3Delegate to same function raise exception.", ref result);
			LOG(e.Message);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToDelegate_Step_5_1_0()
	{
		/*
		 */
		string caseName = "testGetFuncToDelegate_Step_5_1_0: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FuncMultiParams3Delegate2 delegate1 = luaEnv.Global.Get<FuncMultiParams3Delegate2> ("func_multi_params3");
		bool a = true;
		int b = 5;
		int sum;
		delegate1(b, out sum, ref a);
		
		LOG ("sum = " + sum + ", a = " + a);
		if (a == true && sum == 6 ){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "should be sum = 6, a = true", out result);
		}
		
		FuncMultiParams3Delegate delegate2 = luaEnv.Global.Get<FuncMultiParams3Delegate> ("func_multi_params4");
		a = false;
		delegate2(out sum, b, ref a);
		
		LOG ("FuncMultiParams3Delegate, sum = " + sum + ", a = " + a);
		
		if (a != false || sum != 0) {
			updateResult (false, "FuncMultiParams3Delegate, should be sum = 0, a = false", ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

/*
	public TestResult testGetFuncToDelegate_Step_5_2()
	{


		string caseName = "testGetFuncToDelegate_Step_5_2: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FuncMultiParams2Delegate delegate1 = luaEnv.Global.Get<FuncMultiParams2Delegate> ("func_multi_params2");
		bool a = true;
		int b = 5;
		int sum;
		Action d;
		d = delegate1(ref a, b, out sum);

		LOG ("sum = " + sum + ", a = " + a);
		d();
		int intValue3 = luaEnv.Global.Get<int> ("intValue3");

		if (a == true && sum == 6 && intValue3 == 1){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "should be sum = 6, a = true, intValue3 = 1", out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}
*/

	public TestResult testGetFuncToDelegate_Step_6()
	{
		/*
		 */
		string caseName = "testGetFuncToDelegate_Step_6: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FucnVarParamsDelegate delegate1 = luaEnv.Global.Get<FucnVarParamsDelegate> ("func_varparams");
		int sum = delegate1(6, 4);
		
		LOG ("sum = " + sum );
		
		if ( sum == 10){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "should be sum = 10", out result);
		}

		sum = delegate1(10, 5);
		
		LOG ("sum = " + sum );
		if (sum != 15) {
			updateResult (false, "more input params, should be sum = 15", ref result);
		}

		LOG (caseName + result.ToString());
		return result;
	}
	
	public TestResult testGetFuncToLuaFunc_Step_8()
	{
		/*
		 */
		string caseName = "testGetFuncToLuaFunc_Step_8: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		LuaFunction func1 = luaEnv.Global.Get<LuaFunction> ("func_self_increase");
		func1.Call ();
		int intValue3 = luaEnv.Global.Get<int> ("intValue3");
		LOG ("intValues = " + intValue3);
		if (intValue3 == 1){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "intValue3 + 1 = 1, but is " + intValue3, out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToLuaFunc_Step_9_1()
	{
		/*
		 */
		string caseName = "testGetFuncToLuaFunc_Step_9_1: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		LuaFunction func1 = luaEnv.Global.Get<LuaFunction> ("func_return_multivalues");

		object[] returns = func1.Call();
		string str = (string)returns[1];
		LuaTable table = (LuaTable)returns[2];
		bool update = (bool)returns[0];
		
		LOG ("str = " + str + ", update = " + update + ", {k1 = " + 
		           table["k1"] + ", k2 = " + table["k2"] + "}");
		
		if (update == false && str == "false" && Convert.ToInt32(table["k1"]) == 11 && Convert.ToInt32(table["k2"]) == 12){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "function return error, should be false, 'false', {k1=11, k2=12}", out result);
		}
		
		FuncSelfINcreaseDelegate delegate2 = luaEnv.Global.Get<FuncSelfINcreaseDelegate> ("func_self_increase");
		delegate2 ();
		// int intValue3 = luaEnv.Global.Get<int> ("intValue3");
		// intValue3 = 3;
		
		returns = func1.Call();
		str = (string)returns[1];
		table = (LuaTable)returns[2];
		update = (bool)returns[0];
		LOG ("get again, str = " + str + ", update = " + update + ", {k1 = " + 
		           table["k1"] + ", k2 = " + table["k2"] + "}");
		if (update != true || str != "true" || Convert.ToInt32(table["k1"]) != 11 || Convert.ToInt32(table["k2"]) != 12){
			updateResult (false, "function return error, should be true, 'true', {k1=11, k2=12}", ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToLuaFunc_Step_10()
	{
		/*
		 */
		string caseName = "testGetFuncToLuaFunc_Step_10: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		LuaFunction func1 = luaEnv.Global.Get<LuaFunction> ("func_return_func");
		LuaFunction e = (LuaFunction)func1.Call ()[0];
		e.Call ();

		int intValue3 = luaEnv.Global.Get<int>("intValue3");
		if ( intValue3 == 1){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "intValue3 should be 1, but is" + intValue3, out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToLuaFunc_Step_12()
	{
		/*
		 */
		string caseName = "testGetFuncToLuaFunc_Step_12: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		LuaFunction func1 = luaEnv.Global.Get<LuaFunction> ("func_multi_params");
		bool a = true;
		int b = 2;
		string c = "ABC";
		int d;
		object[] returns = func1.Call(a, b, c);
		a = (bool)returns [0];
		b = Convert.ToInt32(returns [1]);
		c = (string)returns [2];
		LOG ("a = " + a + ", b = " + b + ", c = " + c);
		if (a == true && b == 3 && c == "abc"){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "function return error, should be true, 3, 'abc'", out result);
		}
		
		a = false;
		b = 2;
		c = "ABC";
		returns = func1.Call(a, b, c);
		a = (bool)returns [0];
		b = Convert.ToInt32(returns [1]);
		c = (string)returns [2];
		if (a != false || b != 2 || c != "ABC") {
			updateResult (false, "function return error, should be false, 2, 'ABC'", ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}


	public TestResult testGetFuncToLuaFunc_Step_13()
	{
		/*
		 */
		string caseName = "testGetFuncToLuaFunc_Step_13: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		LuaFunction func1 = luaEnv.Global.Get<LuaFunction> ("func_varparams");
		object[] returns = func1.Call(6, 4);
		int sum = Convert.ToInt32(returns [0]);
		LOG ("sum = " + sum );
		
		if ( sum == 10){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "should be sum = 10", out result);
		}
		
		returns = func1.Call(10, 1, 4);
		sum = Convert.ToInt32(returns [0]);
		LOG ("sum = " + sum );
		
		LOG ("sum = " + sum );
		if (sum != 15) {
			updateResult (false, "more input params, should be sum = 15", ref result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	// 补充用例C#以返回值为object的delegate调用Lua函数，
	// (1)而在lua函数侧返回的是long，ulong，都能正确返回。
	// (2)在lua函数返回别的地方创建的userdata，比如io.open返回的文件句柄，在c#那接收到的是null
	public TestResult testGetFuncToDelegate_Step_7_1()
	{

		string caseName = "testGetFuncToDelegate_Step_7_1: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FuncReturnObjectDelegate delegate1 = luaEnv.Global.Get<FuncReturnObjectDelegate> ("func_return_object");
		System.Object ret = delegate1(0);
		
		LuaTestCommon.Log ("ret= " + ret );
		
		if ( ret == null){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "should be ret = null", out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToDelegate_Step_7_2()
	{
		
		string caseName = "testGetFuncToDelegate_Step_7_2: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FuncReturnObjectDelegate delegate1 = luaEnv.Global.Get<FuncReturnObjectDelegate> ("func_return_object");
		System.Object ret = delegate1(1);
		
		LuaTestCommon.Log ("ret= " + ret );
		
		if ( Convert.ToInt64(ret)== System.Int64.MaxValue){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "should be ret = System.Int64.MaxValue", out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

	public TestResult testGetFuncToDelegate_Step_7_3()
	{
		
		string caseName = "testGetFuncToDelegate_Step_7_3: ";
		LOG ("*************" + caseName);
		TestResult result;
		
		luaEnv.DoString(script);
		
		FuncReturnObjectDelegate delegate1 = luaEnv.Global.Get<FuncReturnObjectDelegate> ("func_return_object");
		System.Object ret = delegate1(2);
		
		LuaTestCommon.Log ("ret= " + ret );
		
		//if ( Convert.ToUInt64(ret) == System.UInt64.MaxValue){
		if ( Convert.ToInt64(ret) == -1){
			setResult (true, "pass", out result);
		} else {
			setResult (false, "should be ret = System.UInt64.MaxValue", out result);
		}
		
		LOG (caseName + result.ToString());
		return result;
	}

    public TestResult testLuaTableGetSetKeyValue_BasicType_int_1()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_int_1: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        int ret = luaEnv.Global.Get<int>("intValueMax");

        LuaTestCommon.Log("ret= " + ret);

        if (ret == System.Int32.MaxValue)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "(1).intValueMax, is " + ret, out result);
        }
        luaEnv.Global.Set("intValueMax", 12345);
        ret = luaEnv.Global.Get<int>("intValueMax");

        LuaTestCommon.Log("ret= " + ret);

        if (ret == 12345)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2).after set intValueMax=12345, get is " + ret, ref result);
        }
        ret = 0;
        luaEnv.Global.Get("intValueMax", out ret);
        LuaTestCommon.Log("out ret= " + ret);
        if (ret == 12345)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(3).after set intValueMax=12345, get is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_BasicType_int_2()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_int_2: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        int ret;
        luaEnv.Global.Set(123, 12345);
        ret = luaEnv.Global.Get<int>(123);

        LuaTestCommon.Log("ret= " + ret);

        if (ret == 12345)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "(1).after set 123=12345, get is " + ret,  out result);
        }
        ret = 0;
        luaEnv.Global.Get(123, out ret);
        LuaTestCommon.Log("out ret= " + ret);
        if (ret == 12345)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2).after set intValueMax=12345, get is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_BasicType_string_1()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_string_1: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        string ret;
        luaEnv.Global.Set("strValueChin", "中文字符串mix12345587");
        luaEnv.Global.Get("strValueChin", out ret);

        LuaTestCommon.Log("ret= " + ret);

        if (ret == "中文字符串mix12345587")
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + ret, out result);
        }
        
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_BasicType_sbyte()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_sbyte: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        sbyte ret;
        luaEnv.Global.Set((sbyte)12, (sbyte)23);
        luaEnv.Global.Get((sbyte)12, out ret);

        LuaTestCommon.Log("ret= " + ret);

        if (ret == (sbyte)23)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + ret, out result);
        }
        luaEnv.Global.Get("sbyteValueMin", out ret);
        if (ret == -128)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2) get sbyteValueMin is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_BasicType_byte()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_byte: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        byte ret;
        luaEnv.Global.Set((byte)255, (byte)23);
        luaEnv.Global.Get((byte)255, out ret);

        LuaTestCommon.Log("ret= " + ret);

        if (ret == (byte)23)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + ret, out result);
        }
        luaEnv.Global.Get("byteValueMax", out ret);
        if (ret == (byte)255)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2) get byteValueMax is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_BasicType_short()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_short: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        short ret;
        luaEnv.Global.Set((short)256, (short)512);
        luaEnv.Global.Get((short)256, out ret);

        LuaTestCommon.Log("ret= " + ret);

        if (ret == 512)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + ret, out result);
        }
        luaEnv.Global.Get("shortValueMax", out ret);
        if (ret == 32767)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2) get shortValueMax is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_BasicType_ushort()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_ushort: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        ushort ret;
        luaEnv.Global.Set((ushort)1024, (ushort)32768);
        luaEnv.Global.Get((ushort)1024, out ret);

        LuaTestCommon.Log("ret= " + ret);

        if (ret == 32768)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + ret, out result);
        }
        luaEnv.Global.Get("ushortValueMax", out ret);
        if (ret == 65535)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2) get ushortValueMax is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_BasicType_long()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_long: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        long ret;
        luaEnv.Global.Set("test", (long)3276800000);
        luaEnv.Global.Get("test", out ret);

        LuaTestCommon.Log("ret= " + ret);

        if (ret == 3276800000)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + ret, out result);
        }
        luaEnv.Global.Get("longValue2", out ret);
        if (ret == -42949672960)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2) get longValue2 is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_BasicType_ulong()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_ulong: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        ulong ret;
        luaEnv.Global.Set("test", (ulong)42949672960);
        luaEnv.Global.Get("test", out ret);

        LuaTestCommon.Log("ret= " + ret);

        if (ret == 42949672960)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + ret, out result);
        }
        luaEnv.Global.Get("ulongValueMax", out ret);
        if (ret == 42949672961111)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2) get ulongValueMax is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }
    public TestResult testLuaTableGetSetKeyValue_BasicType_double()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_double: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        double ret;
        luaEnv.Global.Set((double)0.0000001, (double)1.000124587);
        luaEnv.Global.Get((double)0.0000001, out ret);

        LuaTestCommon.Log("ret= " + ret);

        if (ret == 1.000124587)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + ret, out result);
        }
        luaEnv.Global.Get("doubleValue2", out ret);
        if (ret == -3.14159265)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2) get doubleValue2 is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_BasicType_float()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_float: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        float ret;
        luaEnv.Global.Set((float)3.14, (float)3.15);
        luaEnv.Global.Get((float)3.14, out ret);

        LuaTestCommon.Log("ret= " + ret);

        if (System.Math.Abs(ret - 3.15) < 0.000001)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + ret, out result);
        }
        ret = 0;
        luaEnv.Global.Get("floatValue2", out ret);
        if (System.Math.Abs(ret + 3.14) < 0.000001)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2) get floatValue2 is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_BasicType_char()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_char: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        char ret;
        luaEnv.Global.Set('a', 'b');
        luaEnv.Global.Get('a', out ret);

        LuaTestCommon.Log("ret= " + ret);

        if (ret == 'b')
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + ret, out result);
        }
        luaEnv.Global.Get("charValue", out ret);
        if (ret == 'a')
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2) get charValue is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_BasicType_decimal()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_decimal: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        decimal key = -32132143143100109.00010001010M;
        decimal value = 0.0000001M;
        decimal ret;
        luaEnv.Global.Set(123.01, value);
        luaEnv.Global.Get(123.01, out ret);

        LuaTestCommon.Log("ret= " + ret);

        if (ret == value)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + ret, out result);
        }
        luaEnv.Global.Get("decValue2", out ret);
        if (ret == -12.3111111111M)
        {
            updateResult(true, "pass", ref result);
        }
        else
        {
            updateResult(false, "(2) get decValue2 is " + ret, ref result);
        }
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_struct()
    {

        string caseName = "testLuaTableGetSetKeyValue_BasicType_struct: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        TestStruct mystruct = new TestStruct(5, 6); // custom complex value type
        TestStruct mystruct2;
        luaEnv.Global.Set(123, mystruct);
        luaEnv.Global.Get(123, out mystruct2);

        LuaTestCommon.Log("ret= " + mystruct2);

        if (mystruct.a == mystruct2.a)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "ret is " + mystruct2.a, out result);
        }
 
        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_class()
    {

        string caseName = "testLuaTableGetSetKeyValue_class: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        tableValue1ClassPrivate tb1Class = luaEnv.Global.Get<tableValue1ClassPrivate>("tableValue1");
        tableValue1ClassPrivate tb2Class;
        luaEnv.Global.Set(tb1Class.key1, tb1Class);
        luaEnv.Global.Get(tb1Class.key1, out tb2Class);
        LOG("tableValue1.key1 = " + tb2Class.key1 + "; tableValue1.key2 = " + tb2Class.Get());
        LOG("tableValue1.key3 = " + tb2Class.key3);

        if (tb2Class.key1 == 100000)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "(1).tableValue1.key1 should be 100000, but int fact is : " + tb2Class.key1 + "", out result);
        }
        if (tb2Class.Get() != 0)
        {
            updateResult(false, "(2).private key2 should be 0, but int fact is " + tb2Class.Get(), ref result);
        }
        if (tb2Class.key3 != true)
        {
            updateResult(false, "(3).tableValue1.key3 should be true, but in fact is " + tb2Class.key3, ref result);
        }

        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_interface()
    {

        string caseName = "testLuaTableGetSetKeyValue_interface: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        tableValue1InfEqual tb1Inf = luaEnv.Global.Get<tableValue1InfEqual>("tableValue1");
        tb1Inf.key3 = false;
        tableValue1InfEqual tb2Inf;
        LOG("tableValue1.key1 = " + tb1Inf.key1 + "; tableValue1.key2 = " + tb1Inf.key2);
        LOG("tableValue1.key3 = " + tb1Inf.key3);
        LOG("tableValue1.tableVarInclude.ikey1 = " + tb1Inf.tableVarInclude.ikey1 +
                   "; tableValue1.tableVarInclude.ikey2 = " + tb1Inf.tableVarInclude.ikey2);

        luaEnv.Global.Set("tableValue1", tb1Inf);
        luaEnv.Global.Get("tableValue1", out tb2Inf);

        if (tb2Inf.key1 == 100000)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "(1).tableValue1.key1 should be 100000, but int fact is : " + tb2Inf.key1 + "", out result);
        }
        if (tb2Inf.key2 != 10)
        {
            updateResult(false, "(2).tableValue1.key2 should be 10, but in fact is " + tb2Inf.key2, ref result);
        }
        if (tb2Inf.key3 != false)
        {
            updateResult(false, "(3).tableValue1.key3 should be true, but in fact is " + tb2Inf.key3, ref result);
        }
        if (tb2Inf.tableVarInclude.ikey1 != 10)
        {
            updateResult(false, "(4).tableValue1.tableVarInclude.ikey1 should be 10, but in fact is " + tb2Inf.tableVarInclude.ikey1, ref result);
        }
        if (tb2Inf.tableVarInclude.ikey2 != 12)
        {
            updateResult(false, "(5).tableValue1.tableVarInclude.ikey2 should be 12, but in fact is " + tb2Inf.tableVarInclude.ikey2, ref result);
        }
        int subResult = tb2Inf.sub(1000, 100);
        if (subResult != 900)
        {
            updateResult(false, "(6).sub result is error, 1000 - 100 =  " + subResult, ref result);
        }

        LOG(caseName + result.ToString());
        return result;
    }
    public TestResult testLuaTableGetSetKeyValue_dict()
    {

        string caseName = "testLuaTableGetSetKeyValue_dict: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        Dictionary<string, int> dict = luaEnv.Global.Get<Dictionary<string, int>>("tableValue1");
        Dictionary<string, int> dict2;

        LOG("tableValue1.key1 = " + dict["key1"] + ", tableValue1.key2 = " + dict["key2"]);
        LOG("dict.Count = " + dict.Count);

        luaEnv.Global.Set("tableValue1", dict);
        luaEnv.Global.Get("tableValue1", out dict2);

        LOG("tableValue1.key1 = " + dict2["key1"] + ", tableValue1.key2 = " + dict2["key2"]);
        LOG("dict.Count = " + dict2.Count);

        if (dict2.Count == 2 && dict2["key1"] == 100000 && dict2["key2"] == 10)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "dict should have key1 = 100000, key2 = 10", out result);
        }

        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_list()
    {

        string caseName = "testLuaTableGetSetKeyValue_list: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        List<string> listVar = luaEnv.Global.Get<List<string>>("tableValue3");
        List<string> listVar2;
        LOG("listVar = " + listToStr(listVar));
        listVar[0] = "test";
        luaEnv.Global.Set("tableValue3", listVar);
        luaEnv.Global.Get("tableValue3", out listVar2);

        if (listVar2.Count == 8)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "listVar2 should be {'test', 'banana', 'orange', 'kiwi', " +
                "'grape', 'lemon', 'strawberry', 'pear'}", out result);
        }

        LOG(caseName + result.ToString());
        return result;
    }

    public TestResult testLuaTableGetSetKeyValue_delegate()
    {

        string caseName = "testLuaTableGetSetKeyValue_delegate: ";
        LOG("*************" + caseName);
        TestResult result;

        luaEnv.DoString(script);

        FuncSelfINcreaseDelegate delegate1 = luaEnv.Global.Get<FuncSelfINcreaseDelegate>("func_self_increase");
        FuncSelfINcreaseDelegate delegate2;

        luaEnv.Global.Set("test_delegate", delegate1);
        luaEnv.Global.Get("test_delegate", out delegate2);
        delegate2();
        int intValue3 = luaEnv.Global.Get<int>("intValue3");

        LOG("intValues = " + intValue3);
        if (intValue3 == 1)
        {
            setResult(true, "pass", out result);
        }
        else
        {
            setResult(false, "intValue3 + 1 = 1, but is " + intValue3, out result);
        }

        LOG(caseName + result.ToString());
        return result;
    }

}
