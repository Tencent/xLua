cd luajit-2.1.0b3
make clean
make CC="gcc -m32"
cd ..
mkdir -p build_linux32_lj && cd build_linux32_lj
cmake -DUSING_LUAJIT=ON -DCMAKE_C_FLAGS=-m32 -DCMAKE_CXX_FLAGS=-m32 -DCMAKE_SHARED_LINKER_FLAGS=-m32 ../
cd ..
cmake --build build_linux32_lj --config Release

