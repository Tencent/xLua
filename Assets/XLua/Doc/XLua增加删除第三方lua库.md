## What&Why

XLua目前内置的扩展库：

* 针对luajit的64位整数支持；
* 函数调用耗时以及内存泄漏定位工具；
* 用于支持ZeroBraneStudio的luasocket库；
* tdr 4 lua；

随着使用项目的增加以及项目使用的深入程度，仅有这几个扩展已经没法满足项目组了，而由于各个项目对扩展差异化比较大，以及手机平台对安装包大小的敏感，XLua是无法通过预集成去满足这些需求，这也是这篇教程的由来。

这篇教程，将以lua-rapidjson为例，一步步的讲述怎么往xLua添加c/c++扩展，当然，会添加了，自然删除也就会了，项目组可以自行删除不需要用到的预集成扩展。

## How

分三步

1. 修改build文件、工程设置，把要集成的扩展编译到XLua Plugin里头；
2. 调用xLua的C# API，使得扩展可以被按需（在lua代码里头require的时候）加载；
3. 可选，如果你的扩展里头需要用到64位整数，你可以通过XLua的64位扩展库来实现和C#的配合。

### 一、添加扩展&编译

准备工作

1. 把xLua的C源码包解压到你Unity工程的Assets同级目录下。

    下载lua-rapidjson代码，按你的习惯放置。本教程是把rapidjson头文件放到$UnityProj\build\lua-rapidjson\include目录下，而扩展的源码rapidjson.cpp放到$UnityProj\build\lua-rapidjson\source目录下（注：$UnityProj指的是你工程的目录）

2. 在CMakeLists.txt加入扩展

    xLua的各平台Plugins编译使用cmake编译，好处是所有平台的编译都写在一个makefile，大部分编译处理逻辑是跨平台的。
    
    xLua配套的CMakeLists.txt为第三方扩展提供了扩展点（都是list）：
    
    1. THIRDPART_INC：第三方扩展的头文件搜索路径。
    2. THIRDPART_SRC：第三方扩展的源代码。
    3. THIRDPART_LIB：第三方扩展依赖的库。

    如下是rapidjson的加法

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

    完整代码请见附件。

3. 各平台编译

    所有编译脚本都是按这个方式命名：make_平台_lua版本.后缀。

    比如windows 64位lua53版本是make_win64_lua53.bat，android的luajit版本是make_android_luajit.sh，要编译哪个版本就执行相应的脚本即可。

    执行完编译脚本会自动拷贝到plugin_lua53或者plugin_luajit目录，前者是lua53版本放置路径，后者是luajit。

    配套的android脚本是在linux下使用的，脚本开头的NDK路径要根据实际情况修改。

### 二、C#侧集成

所有lua的C扩展库都会提供个luaopen_xxx的函数，xxx是动态库的名字，比如lua-rapidjson库该函数是luaopen_rapidjson，这类函数由lua虚拟机在加载动态库时自动调用，而在手机平台，由于ios的限制我们加载不了动态库，而是直接编译进进程里头。

为此，XLua提供了一个API来替代这功能（LuaEnv的成员方法）：

    public void AddBuildin(string name, LuaCSFunction initer)

参数：

    name：buildin模块的名字，require时输入的参数；
    initer：初始化函数，原型是这样的public delegate int lua_CSFunction(IntPtr L)，必须是静态函数，而且带MonoPInvokeCallbackAttribute属性修饰，这个api会检查这两个条件。

我们以luaopen_rapidjson的调用来看看怎么使用。

扩展LuaDLL.Lua类，用pinvoke把luaopen_rapidjson导出到C#，然后写一个符合lua_CSFunction定义的静态函数，你可以在里头做写初始化工作，比如luaopen_rapidjson的调用，以下是完整代码：

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

然后调用AddBuildin：

    luaenv.AddBuildin("rapidjson", LuaDLL.Lua.LoadRapidJson);

然后就ok了，在lua代码中试试该扩展：

    local rapidjson = require('rapidjson')
    local t = rapidjson.decode('{"a":123}')
    print(t.a)
    t.a = 456
    local s = rapidjson.encode(t)
    print('json', s)

### 三、64位改造

把i64lib.h文件include到需要64位改造的文件里头。
该头文件的API就以下几个：

    //往栈上放一个int64/uint64
    void lua_pushint64(lua_State* L, int64_t n);
    void lua_pushuint64(lua_State* L, uint64_t n);
    //判断栈上pos位置是否是int64/uint64
    int lua_isint64(lua_State* L, int pos);
    int lua_isuint64(lua_State* L, int pos);
    //从栈上pos位置取一个int64/uint64
    int64_t lua_toint64(lua_State* L, int pos);
    uint64_t lua_touint64(lua_State* L, int pos);

这些API的使用依情况而定，可以看看本文附带的附件（rapidjson.cpp文件）

编译工程相关修改