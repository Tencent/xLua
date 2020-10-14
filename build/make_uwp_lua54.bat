mkdir build_uwp_54 & pushd build_uwp_54
cmake -DLUA_VERSION=5.4.1 -G "Visual Studio 15 2017" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
popd
cmake --build build_uwp_54 --config Release
md plugin_lua54\Plugins\WSA\x86
copy /Y build_uwp_54\Release\xlua.dll plugin_lua54\Plugins\WSA\x86\xlua.dll

mkdir build_uwp64_54 & pushd build_uwp64_54
cmake -DLUA_VERSION=5.4.1 -G "Visual Studio 15 2017 Win64" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
popd
cmake --build build_uwp64_54 --config Release
md plugin_lua54\Plugins\WSA\x64
copy /Y build_uwp64_54\Release\xlua.dll plugin_lua54\Plugins\WSA\x64\xlua.dll

mkdir build_uwp_arm_54 & pushd build_uwp_arm_54
cmake -DLUA_VERSION=5.4.1 -G "Visual Studio 15 2017 ARM" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
popd
cmake --build build_uwp_arm_54 --config Release
md plugin_lua54\Plugins\WSA\ARM
copy /Y build_uwp_arm_54\Release\xlua.dll plugin_lua54\Plugins\WSA\ARM\xlua.dll

mkdir build_uwp_arm64_54 & pushd build_uwp_arm64_54
cmake -DLUA_VERSION=5.4.1 -G "Visual Studio 15 2017" -A ARM64 -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
popd
cmake --build build_uwp_arm64_54 --config Release
md plugin_lua54\Plugins\WSA\ARM64
copy /Y build_uwp_arm64_54\Release\xlua.dll plugin_lua54\Plugins\WSA\ARM64\xlua.dll

pause
