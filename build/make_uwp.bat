mkdir build_uwp & pushd build_uwp
cmake -G "Visual Studio 14 2015" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
IF %ERRORLEVEL% NEQ 0 cmake -G "Visual Studio 15 2017" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
popd
cmake --build build_uwp --config Release
md plugin_lua53\Plugins\WSA\x86
copy /Y build_uwp\Release\xlua.dll plugin_lua53\Plugins\WSA\x86\xlua.dll

mkdir build_uwp64 & pushd build_uwp64
cmake -G "Visual Studio 14 2015 Win64" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
IF %ERRORLEVEL% NEQ 0 cmake -G "Visual Studio 15 2017 Win64" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
popd
cmake --build build_uwp64 --config Release
md plugin_lua53\Plugins\WSA\x64
copy /Y build_uwp64\Release\xlua.dll plugin_lua53\Plugins\WSA\x64\xlua.dll

mkdir build_uwp_arm & pushd build_uwp_arm
cmake -G "Visual Studio 14 2015 ARM" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
IF %ERRORLEVEL% NEQ 0 cmake -G "Visual Studio 15 2017 ARM" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
popd
cmake --build build_uwp_arm --config Release
md plugin_lua53\Plugins\WSA\ARM
copy /Y build_uwp_arm\Release\xlua.dll plugin_lua53\Plugins\WSA\ARM\xlua.dll

pause
