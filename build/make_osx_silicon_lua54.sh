mkdir -p build_osx_54_silicon && cd build_osx_54_silicon
cmake -DBUILD_SILICON=ON -DLUA_VERSION=5.4.1 -GXcode ../
cd ..
cmake --build build_osx_54_silicon --config Release
mkdir -p plugin_lua54/Plugins/arm64
cp build_osx_54_silicon/Release/libxlua.dylib plugin_lua54/Plugins/arm64/

