#-*- coding:utf-8 -*-

import os
import sys
import shutil
import argparse
import glob

def checkResult(r):
    if(r != 0):
        sys.exit(1)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="XLua build tool on windows platform.")
    parser.add_argument("-n", "--ndk", default = os.environ.get("ANDROID_NDK"), help="Android NDK path")
    parser.add_argument("-s", "--sdk", default = os.environ.get("ANDROID_SDK"), help="Android SDK path")
    args = parser.parse_args()

    print("Android SDK Path = %s"%(args.sdk))
    print("Android NDK Path = %s"%(args.ndk))

    cmake_version_folder = glob.glob(args.sdk + "/cmake/*")
    if(len(cmake_version_folder) == 0):
        print("Can not find cmake module in Android SDK! Please install it by using Android SDK Manager.")
        sys.exit(1)
    cmake_path = cmake_version_folder[-1] + "/bin/cmake"
    ninja_path = cmake_version_folder[-1] + "/bin/ninja"

    if(not os.path.exists("build_v7a")):
        os.mkdir("build_v7a")
    ret = os.system("%s -H./ -B./build_v7a \"-GAndroid Gradle - Ninja\" -DANDROID_ABI=armeabi-v7a -DANDROID_NDK=%s -DCMAKE_BUILD_TYPE=Relase -DCMAKE_MAKE_PROGRAM=%s -DCMAKE_TOOLCHAIN_FILE=./cmake/android.windows.toolchain.cmake \"-DCMAKE_CXX_FLAGS=-std=c++11 -fexceptions\""%(cmake_path, args.ndk, ninja_path))
    checkResult(ret)
    ret = os.system("%s -C ./build_v7a"%(ninja_path))
    checkResult(ret)

    if(not os.path.exists("./plugin_lua53/Plugins/Android/libs/armeabi-v7a/")):
        os.makedirs("./plugin_lua53/Plugins/Android/libs/armeabi-v7a/")
    shutil.move("./build_v7a/libxlua.so", "./plugin_lua53/Plugins/Android/libs/armeabi-v7a/libxlua.so")

    if(not os.path.exists("build_android_x86")):
        os.mkdir("build_android_x86")
    ret = os.system("%s -H./ -B./build_android_x86 \"-GAndroid Gradle - Ninja\" -DANDROID_ABI=x86 -DANDROID_NDK=%s -DCMAKE_BUILD_TYPE=Relase -DCMAKE_MAKE_PROGRAM=%s -DCMAKE_TOOLCHAIN_FILE=./cmake/android.windows.toolchain.cmake \"-DCMAKE_CXX_FLAGS=-std=c++11 -fexceptions\""%(cmake_path, args.ndk, ninja_path))
    checkResult(ret)
    ret = os.system("%s -C ./build_android_x86"%(ninja_path))
    checkResult(ret)

    if(not os.path.exists("./plugin_lua53/Plugins/Android/libs/x86/")):
        os.makedirs("./plugin_lua53/Plugins/Android/libs/x86/")
    shutil.move("./build_android_x86/libxlua.so", "./plugin_lua53/Plugins/Android/libs/x86/libxlua.so")

    print("Compile success.")