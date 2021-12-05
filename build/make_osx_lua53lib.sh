mkdir -p build_osx && cd build_osx
cmake -GXcode ../
cmake -DMAKE_OSX_LIB=ON
cd ..
cmake --build build_osx --config Release
mkdir -p plugin_lua53/Plugins/MacOS_lib/
cp build_osx/Release/libxlua.a plugin_lua53/Plugins/MacOS_lib/

