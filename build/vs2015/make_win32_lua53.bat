mkdir build32 & pushd build32
cmake -G "Visual Studio 14 2015" ..
IF %ERRORLEVEL% NEQ 0 cmake -G "Visual Studio 15 2017" ..
popd
cmake --build build32 --config Release
md plugin_lua53\Plugins\x86
copy /Y build32\Release\xlua.dll plugin_lua53\Plugins\x86\xlua.dll
pause