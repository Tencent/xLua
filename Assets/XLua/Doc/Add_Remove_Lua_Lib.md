## What & Why

XLua's currently built-in extension libraries:

* LuaJIT support for 64-bit integers;
* Positioning tool for function call times and memory leaks;
* LuaSocket library for supporting ZeroBraneStudio;
* tdr 4 lua;

With the increasing extensiveness and intensiveness of project use, the current extension libraries have been unable to meet the project team needs. Since various projects require very different extension libraries, and since mobile phone platforms are sensitive to the size of the installation package, xLua is unable to meet these needs through pre-integration. That is why we are offering this tutorial.

In this tutorial, we will use lua-rapidjson as an example to explain step by step how to add C/C++ extensions to xLua. Once you know how to add them, you will also know how to delete them naturally. The project team can delete those pre-integrated extensions if they are not used any more.

## How it is done

There are three steps:

1. Modify the build file and project settings. Compile the extensions you want to integrate into the XLua Plugin directory.
2. Call the C# APIs on xLua so that the extensions can be loaded as needed (when required in the Lua code).
3. (Optional) If you need to use 64-bit integers in your extensions, you can use xLua's 64-bit extension library to work with C#.

### First, add extensions & compile.

Preparations

1. Extract the xLua’s C source code package to the same level directory as the Assets of your Unity project.

   Download the lua-rapidjson code and place it anywhere you like. In this tutorial, we place the rapidjson header file in the $UnityProj\build\lua-rapidjson\include directory, and place the extended source code rapidjson.cpp in the $UnityProj\build\lua-rapidjson\source directory (Note: $UnityProj refers to your project directory).

2. Add extensions to CMakeLists.txt

   xLua’s platform Plugins are compiled using CMake. The advantage of this is that the compilations of all platforms are written in a makefile, and most compilation processing logic is cross-platform.

   XLua's CMakeLists.txt provides extension points (all lists) for third-party extensions:
   1. THIRDPART_INC: Third-party extension header search path.
   2. THIRDPART_SRC: Third-party extended source code.
   3. THIRDPART_LIB: The library on which third-party extensions rely.

   The following is added with RapidJSON:

       #begin lua-rapidjson
       set (RAPIDJSON_SRC lua-rapidjson/source/rapidjson.cpp)
       set_property(
       SOURCE ${RAPIDJSON_SRC}
       APPEND
       PROPERTY COMPILE_DEFINITIONS
       LUA_LIB
       )
       list(APPEND THIRDPART_INC  lua-rapidjson/include)
       set (THIRDPART_SRC ${THIRDPART_SRC} ${RAPIDJSON_SRC})
       #end lua-rapidjson

   See the attachment for the complete code.

3. Compile platforms

   All compiled scripts are named with this format: make_platform_lua version.extension name.

   For example, the name of the Windows 64-bit Lua 5.3 version is make_win64_lua53.bat, and the name of the Android LuaJIT version is make_android_luajit.sh. you can execute the corresponding script to compile the target version.

   The compiled scripts are automatically copied to the plugin_lua53 or plugin_luajit directory. The former is for Lua 5.3 and the latter for LuaJIT.

   The supporting Android script is used on Linux. The NDK path at the beginning of the script must be modified accordingly.

### Second, C# side integration:

Each C extension library on Lua will provide a function, luaopen_xxx, where xxx is the name of the dynamic library. For example, the function for the Lua-RapidJSON library is luaopen_rapidjson. Such functions are automatically called by the Lua virtual machine when loading the dynamic library. In the mobile platform, we cannot load the dynamic library due to iOS restrictions. They are compiled directly into the process instead.

For this purpose, xLua provides an API to replace this feature (LuaEnv's member methods):

    public void AddBuildin(string name, LuaCSFunction initer)

Parameters:

    Name: name of the buildin module, a parameter entered during require; 
    initer: the initialization function; Its prototype is public delegate int lua_CSFunction(IntPtr L); This must be a static function and be modified with the property MonoPInvokeCallbackProperty; This API will check these two conditions.

We use calling luaopen_rapidjson to show how to use it.

Extend the LuaDLL.Lua type, export luaopen_rapidjson to C# via pinvoke, and then write a static function that satisfies the definition of lua_CSFunction. You can write initialization work in it, such as calling luaopen_rapidjson. Here is the complete code:

    namespace LuaDLL
    {
    public partial class Lua
    {
    [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
    public static extern int luaopen_rapidjson(System.IntPtr L);
    
    [MonoPInvokeCallback(typeof(LuaDLL.lua_CSFunction))]
    public static int LoadRapidJson(System.IntPtr L)
    {
    return luaopen_rapidjson(L);
    }
    }
    }

Then call AddBuildin:

    luaenv.AddBuildin("rapidjson", LuaDLL.Lua.LoadRapidJson);

After this, it should work properly. Try the extension in the Lua code:

    local rapidjson = require('rapidjson')
    local t = rapidjson.decode('{"a":123}')
    print(t.a)
    t.a = 456
    local s = rapidjson.encode(t)
    print('json', s)

### Third, 64-bit transformation

Include the i64lib.h file in a file that requires a 64-bit transformation. 
The header file include these APIs:

    //Place an int64 on stack/uint64
    void lua_pushint64(lua_State* L, int64_t n);
    void lua_pushuint64(lua_State* L, uint64_t n);
    //Judge whether int64 is at the pos position on stack/uint64
    int lua_isint64(lua_State* L, int pos);
    int lua_isuint64(lua_State* L, int pos);
    //Get an int64 from the pos position on stack/uint64
    int64_t lua_toint64(lua_State* L, int pos);
    uint64_t lua_touint64(lua_State* L, int pos);

The usage of these APIs varies depending on the actual situation. See the attached file (rapidjson.cpp file).

Compile project related modifications

