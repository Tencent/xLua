# 通用字节码

不少项目希望把项目的lua源码通过luac编译后加载，但官方lua有个缺陷：字节码是分32位和64位版本的，换句话你32位lua环境只能跑32位luac编译出来的东西。

为此，xLua尝试对lua源码做了少许改造，可以编译一份字节码，跨平台使用。

## 注意事项

* 1、如果你做了本文所描述的改动，你的xLua将加载不了官方luac所编译的字节码；

* 2、截至2018/9/14，已知此改法在一个上线一个多月的项目正常运行，但不代表此改法在任何情况都没问题。

## 操作指南

### 1、编译xlua的Plugins

修改各平台编译脚本，在cmake命令加上-DLUAC_COMPATIBLE_FORMAT=ON参数，以make_win64_lua53.bat为例，修改后是这样的：

~~~bash
mkdir build64 & pushd build64
cmake -DLUAC_COMPATIBLE_FORMAT=ON -G "Visual Studio 14 2015 Win64" ..
popd
cmake --build build64 --config Release
md plugin_lua53\Plugins\x86_64
copy /Y build64\Release\xlua.dll plugin_lua53\Plugins\x86_64\xlua.dll
pause
~~~

用修改后的编译脚本重新编译各平台的xlua库，并覆盖原Plugins目录下对应文件。

## 2、编译能生成兼容格式的luac（后续只能用这特定的luac和步骤1的Plugins配套使用）

到[这里](build/luac/)，如果你想编译window版本的，执行make_win64.bat，如果你要编译mac或者linux的，用make_unix.sh

## 3、加载字节码

通过CustomLoader加载即可，CustomLoader的详细情况请看教程。


## PS: OpCode修改

有项目想修改为专用格式的字节码，直接在lua源码（目前是lua-5.3.5）上修改后，重新执行上述1、2操作步骤即可。
