set CUR_DIR=%~dp0
cd %CUR_DIR%

del /s/q buildnx64
mkdir buildnx64 & pushd buildnx64
rem fix for io_tmpfile & os_tmpname
echo #if !__ASSEMBLER__ > switch_fix.h
echo static inline struct _IO_FILE* tmpfile(){ return 0; } >> switch_fix.h
echo static inline char* tmpnam(char* n){ return 0; } >> switch_fix.h
echo #endif >> switch_fix.h

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
