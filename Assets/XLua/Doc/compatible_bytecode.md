# 通用字节码

不少项目希望把项目的 lua 源码通过 luac 编译后加载，但官方 lua 有个缺陷：字节码是分 32 位和 64 位版本的，换句话你 32 位 lua 环境只能跑 32 位 luac 编译出来的东西。

为此，xLua 尝试对 lua 源码做了少许改造，可以编译一份字节码，跨平台使用。

## 注意事项

* 1、如果你做了本文所描述的改动，你的 xLua 将加载不了官方 luac 所编译的字节码；

* 2、截至 2018/9/14，已知此改法在一个上线一个多月的项目正常运行，但不代表此改法在任何情况都没问题。

## 操作指南

### 1、编译 xlua 的 Plugins

修改各平台编译脚本，在 cmake 命令加上 `-DLUAC_COMPATIBLE_FORMAT=ON` 参数，以 make_win64_lua53.bat 为例，修改后是这样的：

```bash
mkdir build64 & pushd build64
cmake -DLUAC_COMPATIBLE_FORMAT=ON -G "Visual Studio 14 2015 Win64" ..
popd
cmake --build build64 --config Release
md plugin_lua53\Plugins\x86_64
copy /Y build64\Release\xlua.dll plugin_lua53\Plugins\x86_64\xlua.dll
pause
```

用修改后的编译脚本重新编译各平台的 xlua 库，并覆盖原 Plugins 目录下对应文件。

## 2、编译能生成兼容格式的luac（后续只能用这特定的luac和步骤1的Plugins配套使用）

到[这里](../../../build/luac/)，如果你想编译 Windows 版本的，执行 make_win64.bat，如果你要编译 Mac 或者 Linux 的，用make_unix.sh

## 3、加载字节码

通过 CustomLoader 加载即可，CustomLoader 的详细情况请看教程。这个步骤常犯的错误是用某种Encoding去加载二进制文件，这会破坏lua字节码文件格式。谨记得以二进制方式加载。

## PS: OpCode修改

有项目想修改为专用格式的字节码，直接在 lua 源码（目前是lua-5.3.5）上修改后，重新执行上述1、2操作步骤即可。
