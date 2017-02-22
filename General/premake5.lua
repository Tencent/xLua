
solution "XLua"
    configurations {
        "Debug", "Release"
    }

    location ("./" .. (_ACTION or ""))
    debugdir (".")
    debugargs {  }

    platforms { "Any CPU" }

configuration "Debug"
    symbols "On"
    defines { "_DEBUG", "DEBUG", "TRACE" }
configuration "Release"
    flags { "Optimize" }
configuration "vs*"
    defines { "" }

project "XLua.Mini"
language "C#"
kind "SharedLib"
framework "3.5"
targetdir "./Tools"

files
{
    "../Assets/XLua/Src/*.cs",
    "../Assets/XLua/Src/TemplateEngine/*.cs",
}

defines
{
	"XLUA_GENERAL",
	"UNITY_STANDALONE_WIN",
}

links
{
    "System",
    "System.Core",
    "../Assets/Plugins/x86_64/xlua.dll",
}

project "XLuaGenerate"
language "C#"
kind "ConsoleApp"
framework "4.0"
targetdir "./Tools"

files
{
    "./Src/XLuaGenerate.cs",
    "../Assets/XLua/Src/Editor/Generator.cs",
}

defines
{
    "XLUA_GENERAL",
    "UNITY_STANDALONE_WIN",
}

links
{
    "System",
    "System.Core",
    "XLua.Mini",
}

project "XLuaHotfixInject"
language "C#"
kind "ConsoleApp"
framework "3.5"
targetdir "./Tools"

files
{
    "./Src/XLuaHotfixInject.cs",
    "../Assets/XLua/Src/Editor/Hotfix.cs",
}

defines
{
    "HOTFIX_ENABLE",
    "XLUA_GENERAL",
    "UNITY_STANDALONE_WIN",
}

links
{
    --"C:/Program Files/Unity/Editor/Data/Mono/lib/mono/unity/mscorlib.dll",
    "C:/Program Files/Unity/Editor/Data/Mono/lib/mono/unity/System.dll",
    "C:/Program Files/Unity/Editor/Data/Mono/lib/mono/unity/System.Core.dll",
    "Lib/Mono.Cecil.dll",
    "Lib/Mono.Cecil.Mdb.dll",
    "Lib/Mono.Cecil.Pdb.dll",
}

project "XLuaTest"
language "C#"
kind "ConsoleApp"
framework "4.0"
targetdir "./Bin"

files
{
    "./Src/XLuaTest.cs",
}

defines
{
    "XLUA_GENERAL",
    "UNITY_STANDALONE_WIN",
}

links
{
    "System",
    "System.Core",
    "XLua.Mini",
}

project "XLuaTestGenCode"
language "C#"
kind "ConsoleApp"
framework "4.0"
targetdir "./Bin"

files
{
    "./Src/XLuaTest.cs",
    "../Assets/XLua/Src/*.cs",
    "../Assets/XLua/Src/TemplateEngine/*.cs",
    "./Gen1/*.cs",
}

defines
{
    "XLUA_GENERAL",
    "HOTFIX_ENABLE",
    "UNITY_STANDALONE_WIN",
}

links
{
    "System",
    "System.Core",
    "../Assets/Plugins/x86_64/xlua.dll",
}


project "XLuaUnitTest"
language "C#"
kind "ConsoleApp"
framework "4.0"
targetdir "./Bin"

files
{
    "./Src/XLuaUnitTest.cs",
    "../Test/UnitTest/xLuaTest/**.cs",
}

defines
{
    "XLUA_GENERAL",
    "UNITY_STANDALONE_WIN",
}

links
{
    "System",
    "System.Core",
    "XLua.Mini",
}

project "XLuaUnitTestGenCode"
language "C#"
kind "ConsoleApp"
framework "4.0"
targetdir "./Bin"

files
{
    "./Src/XLuaUnitTest.cs",
    "../Test/UnitTest/xLuaTest/**.cs",
    "../Assets/XLua/Src/*.cs",
    "../Assets/XLua/Src/TemplateEngine/*.cs",
    "./Gen2/*.cs",
}

defines
{
    "XLUA_GENERAL",
    "UNITY_STANDALONE_WIN",
}

links
{
    "System",
    "System.Core",
}
