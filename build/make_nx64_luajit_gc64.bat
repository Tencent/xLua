set CUR_DIR=%~dp0
cd %CUR_DIR%

pushd luajit-2.1.0b3
rem use mingw to compile because msvc treat zero-length array as error
rem define LJ_TARGET_PS4 to work around tmpnam & tmpfile
rem define LUAJIT_USE_SYSMALLOC as no mmap for switch
for /F "tokens=* USEBACKQ" %%F IN (`cygpath %NINTENDO_SDK_ROOT%`) DO set NINTENDO_SDK_ROOT_CYG=%%F
set COMPILER="%NINTENDO_SDK_ROOT_CYG%/Compilers/NX/bin/nx-clang.exe"
set "SWITCH_CFLAGS=-m64 -mcpu=cortex-a57+fp+simd+crypto+crc -fno-common -fno-short-enums -ffunction-sections -fdata-sections -fPIC -fms-extensions"
set "LUAJIT_CFLAGS=\"-DLJ_TARGET_CONSOLE -DLUAJIT_USE_SYSMALLOC -DLJ_TARGET_PS4 -DLUAJIT_DISABLE_JIT -DLUAJIT_ENABLE_GC64 -DLUAJIT_DISABLE_FFI -DLUAJIT_NO_UNWIND\""
bash -c "make clean"
bash -c "make -C src TARGET_CC=\"%COMPILER% %SWITCH_CFLAGS%\" TARGET_LD=%COMPILER% BUILDMODE=static TARGET_SYS=switch CFLAGS=%LUAJIT_CFLAGS% libluajit.a"
popd

del /s/q buildnx64
mkdir buildnx64 & pushd buildnx64
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
