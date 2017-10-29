mkdir -p build_lj_osx && cd build_lj_osx
cmake -DUSING_LUAJIT=ON  -GXcode ../
cd ..
cmake --build build_lj_osx --config Release
mkdir -p plugin_luajit/Plugins/xlua.bundle/Contents/MacOS/
cp build_lj_osx/Release/xlua.bundle/Contents/MacOS/xlua plugin_luajit/Plugins/xlua.bundle/Contents/MacOS/xlua

