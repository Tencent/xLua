
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
}

links
{
    "System",
    "System.Core",
}

project "XLuaGenerate"
language "C#"
kind "ConsoleApp"
framework "4.0"
targetdir "./Tools"

files
{
    "./Src/XLuaGenerate.cs",
    "./Src/XLuaTemplates.Designer.cs",
    "./Src/XLuaTemplates.resx",
    "../Assets/XLua/Src/Editor/Generator.cs",
    "../Assets/XLua/Src/Editor/Template/*.txt",
}

defines
{
    "XLUA_GENERAL",
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


project "KeyPairsGen"
language "C#"
kind "ConsoleApp"
framework "3.5"
targetdir "./Tools"

files
{
    "./Src/KeyPairsGen.cs",
}

defines
{
}

links
{
    "System",
    "System.Core",
}

project "FilesSignature"
language "C#"
kind "ConsoleApp"
framework "3.5"
targetdir "./Tools"

files
{
    "./Src/FilesSignature.cs",
}

defines
{
}

links
{
    "System",
    "System.Core",
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
}

links
{
    "System",
    "System.Core",
    "XLua.Mini",
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
}

links
{
    "System",
    "System.Core",
    "XLua.Mini",
}

solution "XLuaGenTest"
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
}

links
{
    "System",
    "System.Core",
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
}

links
{
    "System",
    "System.Core",
}
