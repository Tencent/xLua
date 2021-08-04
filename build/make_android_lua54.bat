for /f %%a in ('dir /a:d /b %ANDROID_SDK%\cmake\') do set cmake_version=%%a
set cmake_bin=%ANDROID_SDK%\cmake\%cmake_version%\bin\cmake.exe
set ninja_bin=%ANDROID_SDK%\cmake\%cmake_version%\bin\ninja.exe
set android_platform=android-28
set cmake_toolchain_file=%ANDROID_NDK%\build\cmake\android.toolchain.cmake

mkdir build_v7a
%cmake_bin% -H.\ -B.\build_v7a -G Ninja -DANDROID_ABI=armeabi-v7a -DLUA_VERSION=5.4.1 -DANDROID_NDK=%ANDROID_NDK% -DCMAKE_BUILD_TYPE=Relase -DCMAKE_MAKE_PROGRAM=%ninja_bin% -DCMAKE_TOOLCHAIN_FILE=%cmake_toolchain_file% "-DCMAKE_CXX_FLAGS=-std=c++11 -fexceptions" -DCMAKE_SYSTEM_NAME=Android -DANDROID_PLATFORM=%android_platform%
%ninja_bin% -C .\build_v7a
mkdir .\plugin_lua54\Plugins\Android\Libs\armeabi-v7a
move .\build_v7a\libxlua.so .\plugin_lua54\Plugins\Android\Libs\armeabi-v7a\libxlua.so

mkdir build_armv8
%cmake_bin% -H.\ -B.\build_armv8 -G Ninja -DANDROID_ABI=arm64-v8a -DLUA_VERSION=5.4.1 -DANDROID_NDK=%ANDROID_NDK% -DCMAKE_BUILD_TYPE=Relase -DCMAKE_MAKE_PROGRAM=%ninja_bin% -DCMAKE_TOOLCHAIN_FILE=%cmake_toolchain_file% "-DCMAKE_CXX_FLAGS=-std=c++11 -fexceptions" -DCMAKE_SYSTEM_NAME=Android -DANDROID_PLATFORM=%android_platform%
%ninja_bin% -C .\build_armv8
mkdir .\plugin_lua54\Plugins\Android\Libs\arm64-v8a
move .\build_armv8\libxlua.so .\plugin_lua54\Plugins\Android\Libs\arm64-v8a\libxlua.so

mkdir build_x86
%cmake_bin% -H.\ -B.\build_x86 -G Ninja -DANDROID_ABI=x86 -DLUA_VERSION=5.4.1 -DANDROID_NDK=%ANDROID_NDK% -DCMAKE_BUILD_TYPE=Relase -DCMAKE_MAKE_PROGRAM=%ninja_bin% -DCMAKE_TOOLCHAIN_FILE=%cmake_toolchain_file% "-DCMAKE_CXX_FLAGS=-std=c++11 -fexceptions" -DCMAKE_SYSTEM_NAME=Android -DANDROID_PLATFORM=%android_platform%
%ninja_bin% -C .\build_x86
mkdir .\plugin_lua54\Plugins\Android\Libs\x86
move .\build_x86\libxlua.so .\plugin_lua54\Plugins\Android\Libs\x86\libxlua.so

echo "compile success"
pause