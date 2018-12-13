#!/usr/bin/bash
NINTENDO_SDK_ROOT_CYG=`cygpath $NINTENDO_SDK_ROOT`
COMPILER_DIR="$NINTENDO_SDK_ROOT_CYG/Compilers/NX"
COMPILER=$COMPILER_DIR/bin/nx-clang.exe
SWITCH_CFLAGS="-m64 -mcpu=cortex-a57+fp+simd+crypto+crc -fno-common -fno-short-enums -ffunction-sections -fdata-sections -fPIC -fms-extensions"
make clean
make TARGET_CC="$COMPILER $SWITCH_CFLAGS" TARGET_LD="$COMPILER" BUILDMODE=static TARGET_SYS=switch CFLAGS="-DLJ_TARGET_CONSOLE -DLUAJIT_USE_SYSMALLOC"
