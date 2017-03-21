mkdir build_uwp & pushd build_uwp
cmake -G "Visual Studio 14 2015" -DCMAKE_SYSTEM_NAME=WindowsStore -DCMAKE_SYSTEM_VERSION=10.0 ..
popd
cmake --build build_uwp --config Release
pause
