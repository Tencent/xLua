set CUR_DIR=%~dp0
cd %CUR_DIR%
del /s/q buildnx64
mkdir buildnx64 & pushd buildnx64

rem fix for io_tmpfile
echo struct lua_State; > switch_fix.h
echo inline static int io_tmpfile (struct lua_State *L) {return 0;} >> switch_fix.h
echo #define io_tmpfile(x) io_tmpfile_INVALID(x) >> switch_fix.h
rem fix for os_tmpname
echo inline static int os_tmpname (struct lua_State *L) {return 0;} >> switch_fix.h
echo #define os_tmpname(x) os_tmpname_INVALID(x) >> switch_fix.h

set "NINTENDO_SDK_ROOT_CMAKE=%NINTENDO_SDK_ROOT:\=/%"
cmake -DCMAKE_C_COMPILER="%NINTENDO_SDK_ROOT_CMAKE%/Compilers/NX/nx/aarch64/bin/clang.exe" ^
	-DCMAKE_CXX_COMPILER="%NINTENDO_SDK_ROOT_CMAKE%/Compilers/NX/nx/aarch64/bin/clang++.exe" ^
	-G "Unix Makefiles" -DCMAKE_SYSTEM_NAME=Switch ^
	-DCMAKE_C_FLAGS="-includeswitch_fix.h -I%CUR_DIR%buildnx64" ^
	..
popd
cmake --build buildnx64 --config Release
mkdir plugin_lua53\Plugins\Switch
copy /Y buildnx64\libxlua.a plugin_lua53\Plugins\Switch\libxlua.a

rem may need to set package.cpath = "" in lua
rem as any read attempt to undefined location will crash
