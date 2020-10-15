cd luajit-2.1.0b3
make clean
make CFLAGS=-fPIC
cd ..
mkdir -p build_linux64_lj && cd build_linux64_lj
cmake -DUSING_LUAJIT=ON ../
cd ..
cmake --build build_linux64_lj --config Release
mkdir -p plugin_luajit/Plugins/x86_64/
cp build_linux64_lj/libxlua.so plugin_luajit/Plugins/x86_64/libxlua.so 

