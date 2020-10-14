mkdir -p build_linux64 && cd build_linux64
cmake -DLUA_VERSION=5.4.1 ../
cd ..
cmake --build build_linux64 --config Release
mkdir -p plugin_lua54/Plugins/x86_64/
cp build_linux64/libxlua.so plugin_lua54/Plugins/x86_64/libxlua.so 

mkdir -p build_linux32 && cd build_linux32
cmake -DLUA_VERSION=5.4.1 -DCMAKE_C_FLAGS=-m32 -DCMAKE_CXX_FLAGS=-m32 -DCMAKE_SHARED_LINKER_FLAGS=-m32 ../
cd ..
cmake --build build_linux32 --config Release
mkdir -p plugin_lua54/Plugins/x86/
cp build_linux32/libxlua.so plugin_lua54/Plugins/x86/libxlua.so 

