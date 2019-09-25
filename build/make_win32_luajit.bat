@echo off

call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\VC\Auxiliary\Build\vcvars32.bat"

echo Swtich to x86 build env
cd %~dp0\luajit-2.1.0b3\src
call msvcbuild_mt.bat static
cd ..\..

mkdir build_lj32 & pushd build_lj32
cmake -DUSING_LUAJIT=ON -G "Visual Studio 15 2017" ..
IF %ERRORLEVEL% NEQ 0 cmake -DUSING_LUAJIT=ON -G "Visual Studio 15 2017" ..
popd
cmake --build build_lj32 --config Release
md plugin_luajit\Plugins\x86
copy /Y build_lj32\Release\xlua.dll plugin_luajit\Plugins\x86\xlua.dll
pause