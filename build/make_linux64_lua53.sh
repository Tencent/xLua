mkdir -p build_linux64 && cd build_linux64
cmake ../
cd ..
cmake --build build_linux64 --config Release
mkdir -p plugin_lua53/Plugins/x86_64/
cp build_linux64/libxlua.so plugin_lua53/Plugins/x86_64/libxlua.so 
