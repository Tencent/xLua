set CUR_DIR=%~dp0
cd %CUR_DIR%

del /s/q buildnx64
mkdir buildnx64 & pushd buildnx64
rem fix for io_tmpfile & os_tmpname
echo #if !__ASSEMBLER__ > switch_fix.h
echo static inline struct _IO_FILE* tmpfile(){ return 0; } >> switch_fix.h
echo static inline char* tmpnam(char* n){ return 0; } >> switch_fix.h
echo #endif >> switch_fix.h
popd

pushd luajit-2.1.0b3
rem use cygwin64 to compile because msvc treat zero-length array as error
rem define LUAJIT_USE_SYSMALLOC as no mmap for switch
for /F "tokens=* USEBACKQ" %%F IN (`cygpath %NINTENDO_SDK_ROOT%`) DO set NINTENDO_SDK_ROOT_CYG=%%F
set COMPILER="%NINTENDO_SDK_ROOT_CYG%/Compilers/NX/bin/nx-clang.exe"
set "SWITCH_CFLAGS=-m64 -mcpu=cortex-a57+fp+simd+crypto+crc -fno-common -fno-short-enums -ffunction-sections -fdata-sections -fPIC -fms-extensions"
set "LUAJIT_CFLAGS=-DLJ_TARGET_CONSOLE -DLUAJIT_USE_SYSMALLOC -DLUAJIT_DISABLE_JIT -DLUAJIT_ENABLE_GC64 -DLUAJIT_DISABLE_FFI"
set "FIX_FLAGS=-includeswitch_fix.h -I%CUR_DIR%buildnx64"
bash -c "make clean"
bash -c "make -C src TARGET_CC=\"%COMPILER% %SWITCH_CFLAGS% %FIX_FLAGS%\" TARGET_LD=%COMPILER% BUILDMODE=static TARGET_SYS=switch CFLAGS=\"%LUAJIT_CFLAGS%\" libluajit.a"
popd

pushd buildnx64
set "NINTENDO_SDK_ROOT_CMAKE=%NINTENDO_SDK_ROOT:\=/%"
cmake -DCMAKE_C_COMPILER="%NINTENDO_SDK_ROOT_CMAKE%/Compilers/NX/nx/aarch64/bin/clang.exe" ^
	-DCMAKE_CXX_COMPILER="%NINTENDO_SDK_ROOT_CMAKE%/Compilers/NX/nx/aarch64/bin/clang++.exe" ^
	-G "Unix Makefiles" -DCMAKE_SYSTEM_NAME=Switch ^
	-DUSING_LUAJIT=ON ^
	..
popd
cmake --build buildnx64 --config Release
mkdir plugin_luajit\Plugins\Switch
copy /Y buildnx64\libxlua.a plugin_luajit\Plugins\Switch\libxlua.a
copy /Y luajit-2.1.0b3\src\libluajit.a plugin_luajit\Plugins\Switch\libluajit.a

rem may need to set package.cpath = "" in lua
rem as any read attempt to undefined location will crash
