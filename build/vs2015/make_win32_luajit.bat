@echo off
if exist "%VS140COMNTOOLS%" (
	set VCVARS="%VS140COMNTOOLS%..\..\VC\bin\"
	goto build
	)  else (goto missing)


:build

@set ENV32="%VCVARS%vcvars32.bat"

call "%ENV32%"

echo Swtich to x86 build env
cd %~dp0\luajit-2.1.0b3\src
call msvcbuild_mt.bat static
cd ..\..

goto :buildxlua

:missing
echo Can't find Visual Studio 2015.
pause
goto :eof

:buildxlua
mkdir build_lj32 & pushd build_lj32
cmake -DUSING_LUAJIT=ON -G "Visual Studio 14 2015" ..
IF %ERRORLEVEL% NEQ 0 cmake -DUSING_LUAJIT=ON -G "Visual Studio 15 2017" ..
popd
cmake --build build_lj32 --config Release
md plugin_luajit\Plugins\x86
copy /Y build_lj32\Release\xlua.dll plugin_luajit\Plugins\x86\xlua.dll
pause