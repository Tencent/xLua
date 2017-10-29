for /f %%a in ('dir /a:d /b %ANDROID_SDK%\cmake\') do set cmake_version=%%a
set cmake_bin=%ANDROID_SDK%\cmake\%cmake_version%\bin\cmake.exe
set ninja_bin=%ANDROID_SDK%\cmake\%cmake_version%\bin\ninja.exe

mkdir build_v7a
%cmake_bin% -H.\ -B.\build_v7a "-GAndroid Gradle - Ninja" -DANDROID_ABI=armeabi-v7a -DANDROID_NDK=%ANDROID_NDK% -DCMAKE_BUILD_TYPE=Relase -DCMAKE_MAKE_PROGRAM=%ninja_bin% -DCMAKE_TOOLCHAIN_FILE=.\cmake\android.windows.toolchain.cmake "-DCMAKE_CXX_FLAGS=-std=c++11 -fexceptions"
%ninja_bin% -C .\build_v7a
mkdir .\plugin_lua53\Plugins\Android\Libs\armeabi-v7a
move .\build_v7a\libxlua.so .\plugin_lua53\Plugins\Android\Libs\armeabi-v7a\libxlua.so

mkdir build_android_x86
%cmake_bin% -H.\ -B.\build_android_x86 "-GAndroid Gradle - Ninja" -DANDROID_ABI=x86 -DANDROID_NDK=%ANDROID_NDK% -DCMAKE_BUILD_TYPE=Relase -DCMAKE_MAKE_PROGRAM=%ninja_bin% -DCMAKE_TOOLCHAIN_FILE=.\cmake\android.windows.toolchain.cmake "-DCMAKE_CXX_FLAGS=-std=c++11 -fexceptions"
%ninja_bin% -C .\build_android_x86
mkdir .\plugin_lua53\Plugins\Android\Libs\x86
move .\build_android_x86\libxlua.so .\plugin_lua53\Plugins\Android\Libs\x86\libxlua.so

echo "compile success"
pause