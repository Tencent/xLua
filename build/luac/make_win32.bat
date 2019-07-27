mkdir build32 & pushd build32
cmake -DLUAC_COMPATIBLE_FORMAT=ON -G "Visual Studio 14 2015" ..
IF %ERRORLEVEL% NEQ 0 cmake -DLUAC_COMPATIBLE_FORMAT=ON -G "Visual Studio 15 2017" ..
popd
cmake --build build32 --config Release
pause