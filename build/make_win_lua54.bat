mkdir build64 & pushd build64
cmake -DLUA_VERSION=5.4.1 -G "Visual Studio 15 2017 Win64" ..
popd
cmake --build build64 --config Release
md plugin_lua54\Plugins\x86_64
copy /Y build64\Release\xlua.dll plugin_lua54\Plugins\x86_64\xlua.dll

mkdir build32 & pushd build32
cmake -DLUA_VERSION=5.4.1 -G "Visual Studio 15 2017" ..
popd
cmake --build build32 --config Release
md plugin_lua54\Plugins\x86
copy /Y build32\Release\xlua.dll plugin_lua54\Plugins\x86\xlua.dll

pause