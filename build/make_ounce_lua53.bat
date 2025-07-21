set CUR_DIR=%~dp0
cd %CUR_DIR%

del /s/q buildounce
mkdir buildounce & pushd buildounce
rem fix for io_tmpfile & os_tmpname
echo #if !__ASSEMBLER__ > ounce_fix.h
echo static inline struct _IO_FILE* tmpfile(){ return 0; } >> ounce_fix.h
echo static inline char* tmpnam(char* n){ return 0; } >> ounce_fix.h
echo #endif >> ounce_fix.h

set "NINTENDO_SDK_ROOT_CMAKE=%NINTENDO_SDK_ROOT:\=/%"
cmake -DCMAKE_C_COMPILER="%NINTENDO_SDK_ROOT_CMAKE%/Compilers/NintendoClang/bin/clang.exe" ^
	-DCMAKE_CXX_COMPILER="%NINTENDO_SDK_ROOT_CMAKE%/Compilers/NintendoClang/bin/clang++.exe" ^
	-DBUILD_TESTING=OFF ^
	-DCMAKE_C_COMPILER_WORKS=TRUE ^
	-DCMAKE_CXX_COMPILER_WORKS=TRUE ^
	-G "Unix Makefiles" -DCMAKE_SYSTEM_NAME=Ounce ^
	-DCMAKE_C_FLAGS="-includeounce_fix.h -I%CUR_DIR%buildounce" ^
	..
popd
cmake --build buildounce --config Release
mkdir plugin_lua53\Plugins\Ounce
copy /Y buildounce\libxlua.a plugin_lua53\Plugins\Ounce\libxlua.a

rem may need to set package.cpath = "" in lua
rem as any read attempt to undefined location will crash
