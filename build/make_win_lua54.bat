mkdir build64_54 & pushd build64_54
cmake -DLUA_VERSION=5.4.1 -G "Visual Studio 15 2017 Win64" ..
popd
cmake --build build64_54 --config Release
md plugin_lua54\Plugins\x86_64
copy /Y build64_54\Release\xlua.dll plugin_lua54\Plugins\x86_64\xlua.dll

mkdir build32_54 & pushd build32_54
cmake -DLUA_VERSION=5.4.1 -G "Visual Studio 15 2017" ..
popd
cmake --build build32_54 --config Release
md plugin_lua54\Plugins\x86
copy /Y build32_54\Release\xlua.dll plugin_lua54\Plugins\x86\xlua.dll

pause