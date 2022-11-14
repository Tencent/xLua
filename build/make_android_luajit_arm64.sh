#if [ -z "$ANDROID_NDK" ]; then
    export ANDROID_NDK=~/android-ndk-r15c
#fi

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
SRCDIR=$DIR/luajit-2.1.0b3
# ANDROID_NDK=~/android-ndk-r10e

OS=`uname -s`
PREBUILT_PLATFORM=linux-x86_64
if [[ "$OS" == "Darwin" ]]; then
    PREBUILT_PLATFORM=darwin-x86_64
fi

NDKABI=21


echo "Building arm64-v8a lib"
NDKVER=$ANDROID_NDK/toolchains/aarch64-linux-android-4.9
NDKP=$NDKVER/prebuilt/$PREBUILT_PLATFORM/bin/aarch64-linux-android-
NDKARCH="-DLJ_ABI_SOFTFP=0 -DLJ_ARCH_HASFPU=1 -DLUAJIT_ENABLE_GC64=1"  
NDKF="--sysroot $ANDROID_NDK/platforms/android-$NDKABI/arch-arm64"
cd "$SRCDIR"
make clean
make HOST_CC="gcc -m64" CROSS=$NDKP TARGET_SYS=Linux TARGET_FLAGS="$NDKF $NDKARCH"

cd "$DIR"
mkdir -p build_lj_v8a && cd build_lj_v8a
cmake -DUSING_LUAJIT=ON -DANDROID_ABI=arm64-v8a -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-21 ../
cd "$DIR"
cmake --build build_lj_v8a --config Release
mkdir -p plugin_luajit/Plugins/Android/libs/arm64-v8a/
cp build_lj_v8a/libxlua.so plugin_luajit/Plugins/Android/libs/arm64-v8a/libxlua.so


echo "Building armv7 lib"
NDKVER=$ANDROID_NDK/toolchains/arm-linux-androideabi-4.9
NDKP=$NDKVER/prebuilt/$PREBUILT_PLATFORM/bin/arm-linux-androideabi-
NDKARCH="-march=armv7-a -mfloat-abi=softfp -Wl,--fix-cortex-a8"
NDKF="--sysroot $ANDROID_NDK/platforms/android-$NDKABI/arch-arm"
cd "$SRCDIR"
make clean
make HOST_CC="gcc -m32" CROSS=$NDKP TARGET_SYS=Linux TARGET_FLAGS="$NDKF $NDKARCH"

cd "$DIR"
mkdir -p build_lj_v7a && cd build_lj_v7a
cmake -DUSING_LUAJIT=ON -DANDROID_ABI=armeabi-v7a -DCMAKE_TOOLCHAIN_FILE=../cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-4.9 -DANDROID_NATIVE_API_LEVEL=android-21 ../
cd "$DIR"
cmake --build build_lj_v7a --config Release
mkdir -p plugin_luajit/Plugins/Android/libs/armeabi-v7a/
cp build_lj_v7a/libxlua.so plugin_luajit/Plugins/Android/libs/armeabi-v7a/libxlua.so

echo "Building x86 lib"
NDKVER=$ANDROID_NDK/toolchains/x86-4.9
NDKP=$NDKVER/prebuilt/$PREBUILT_PLATFORM/bin/i686-linux-android-
NDKF="--sysroot $ANDROID_NDK/platforms/android-$NDKABI/arch-x86"
cd "$SRCDIR"
make clean
make HOST_CC="gcc -m32" CROSS=$NDKP TARGET_SYS=Linux TARGET_FLAGS="$NDKF"

cd "$DIR"
mkdir -p build_lj_x86 && cd build_lj_x86
cmake -DUSING_LUAJIT=ON -DANDROID_ABI=x86 -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=x86-clang -DANDROID_NATIVE_API_LEVEL=android-21 ../
cd "$DIR"
cmake --build build_lj_x86 --config Release
mkdir -p plugin_luajit/Plugins/Android/libs/x86/
cp build_lj_x86/libxlua.so plugin_luajit/Plugins/Android/libs/x86/libxlua.so

