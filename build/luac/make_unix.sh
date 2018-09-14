mkdir -p build_unix && cd build_unix
cmake -DLUAC_COMPATIBLE_FORMAT=ON ../
cd ..
cmake --build build_unix --config Release

