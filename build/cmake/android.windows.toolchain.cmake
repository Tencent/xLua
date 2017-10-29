cmake_minimum_required(VERSION 3.6)

macro(Highlight_Error error_msg)
	message("==================")
	message("Error: ${error_msg}")
	message("==================")
	message(FATAL_ERROR "")
endmacro()

if(NOT DEFINED ENV{ANDROID_NDK})
	Highlight_Error("not defined environment variable %ANDROID_NDK%")
else()
	set(ANDROID_NDK $ENV{ANDROID_NDK})
endif()
file(TO_CMAKE_PATH "${ANDROID_NDK}" ANDROID_NDK)

if(ANDROID_TOOLCHAIN_NAME AND NOT ANDROID_TOOLCHAIN)
	if(ANDROID_TOOLCHAIN_NAME MATCHES "-clang([0-9].[0-9])?$")
		set(ANDROID_TOOLCHAIN clang)
	elseif(ANDROID_TOOLCHAIN_NAME MATCHES "-[0-9].[0-9]$")
		set(ANDROID_TOOLCHAIN gcc)
	endif()
endif()

if(ANDROID_ABI STREQUAL "armeabi-v7a with NEON")
	set(ANDROID_ABI armeabi-v7a)
	set(ANDROID_ARM_NEON TRUE)
elseif(ANDROID_TOOLCHAIN_NAME AND NOT ANDROID_ABI)
	if(ANDROID_TOOLCHAIN_NAME MATCHES "^arm-linux-androideabi-")
		set(ANDROID_ABI armeabi-v7a)
	elseif(ANDROID_TOOLCHAIN_NAME MATCHES "^aarch64-linux-android-")
		set(ANDROID_ABI arm64-v8a)
	elseif(ANDROID_TOOLCHAIN_NAME MATCHES "^x86-")
		set(ANDROID_ABI x86)
	elseif(ANDROID_TOOLCHAIN_NAME MATCHES "^x86_64-")
		set(ANDROID_ABI x86_64)
	elseif(ANDROID_TOOLCHAIN_NAME MATCHES "^mipsel-linux-android-")
		set(ANDROID_ABI mips)
	elseif(ANDROID_TOOLCHAIN_NAME MATCHES "^mips64el-linux-android-")
		set(ANDROID_ABI mips64)
	endif()
endif()
if(ANDROID_NATIVE_API_LEVEL AND NOT ANDROID_PLATFORM)
	if(ANDROID_NATIVE_API_LEVEL MATCHES "^android-[0-9]+$")
		set(ANDROID_PLATFORM ${ANDROID_NATIVE_API_LEVEL})
	elseif(ANDROID_NATIVE_API_LEVEL MATCHES "^[0-9]+$")
		set(ANDROID_PLATFORM android-${ANDROID_NATIVE_API_LEVEL})
	endif()
endif()
if(DEFINED ANDROID_APP_PIE AND NOT DEFINED ANDROID_PIE)
	set(ANDROID_PIE "${ANDROID_APP_PIE}")
endif()
if(ANDROID_STL_FORCE_FEATURES AND NOT DEFINED ANDROID_CPP_FEATURES)
	set(ANDROID_CPP_FEATURES "rtti exceptions")
endif()
if(DEFINED ANDROID_NO_UNDEFINED AND NOT DEFINED ANDROID_ALLOW_UNDEFINED_SYMBOLS)
	if(ANDROID_NO_UNDEFINED)
		set(ANDROID_ALLOW_UNDEFINED_SYMBOLS FALSE)
	else()
		set(ANDROID_ALLOW_UNDEFINED_SYMBOLS TRUE)
	endif()
endif()
if(DEFINED ANDROID_SO_UNDEFINED AND NOT DEFINED ANDROID_ALLOW_UNDEFINED_SYMBOLS)
	set(ANDROID_ALLOW_UNDEFINED_SYMBOLS "${ANDROID_SO_UNDEFINED}")
endif()
if(DEFINED ANDROID_FORCE_ARM_BUILD AND NOT ANDROID_ARM_MODE)
	if(ANDROID_FORCE_ARM_BUILD)
		set(ANDROID_ARM_MODE arm)
	else()
		set(ANDROID_ARM_MODE thumb)
	endif()
endif()
if(DEFINED ANDROID_NOEXECSTACK AND NOT DEFINED ANDROID_DISABLE_NO_EXECUTE)
	if(ANDROID_NOEXECSTACK)
		set(ANDROID_DISABLE_NO_EXECUTE FALSE)
	else()
		set(ANDROID_DISABLE_NO_EXECUTE TRUE)
	endif()
endif()
if(DEFINED ANDROID_RELRO AND NOT DEFINED ANDROID_DISABLE_RELRO)
	if(ANDROID_RELRO)
		set(ANDROID_DISABLE_RELRO FALSE)
	else()
		set(ANDROID_DISABLE_RELRO TRUE)
	endif()
endif()
if(NDK_CCACHE AND NOT ANDROID_CCACHE)
	set(ANDROID_CCACHE "${NDK_CCACHE}")
endif()

# Default values for configurable variables.
if(NOT ANDROID_TOOLCHAIN)
	set(ANDROID_TOOLCHAIN clang)
endif()
if(NOT ANDROID_ABI)
	set(ANDROID_ABI armeabi-v7a)
endif()
if(ANDROID_PLATFORM MATCHES "^android-([0-8]|10|11)$")
	set(ANDROID_PLATFORM android-9)
elseif(ANDROID_PLATFORM STREQUAL android-20)
	set(ANDROID_PLATFORM android-19)
elseif(NOT ANDROID_PLATFORM)
	set(ANDROID_PLATFORM android-9)
endif()
string(REPLACE "android-" "" ANDROID_PLATFORM_LEVEL ${ANDROID_PLATFORM})
if(ANDROID_ABI MATCHES "64(-v8a)?$" AND ANDROID_PLATFORM_LEVEL LESS 21)
	set(ANDROID_PLATFORM android-21)
	set(ANDROID_PLATFORM_LEVEL 21)
endif()
if(NOT ANDROID_STL)
	set(ANDROID_STL gnustl_static)
endif()
if(NOT DEFINED ANDROID_PIE)
	if(ANDROID_PLATFORM_LEVEL LESS 16)
		set(ANDROID_PIE FALSE)
	else()
		set(ANDROID_PIE TRUE)
	endif()
endif()
if(NOT ANDROID_ARM_MODE)
	set(ANDROID_ARM_MODE thumb)
endif()

# Export configurable variables for the try_compile() command.
set(CMAKE_TRY_COMPILE_PLATFORM_VARIABLES
	ANDROID_TOOLCHAIN
	ANDROID_ABI
	ANDROID_PLATFORM
	ANDROID_STL
	ANDROID_PIE
	ANDROID_CPP_FEATURES
	ANDROID_ALLOW_UNDEFINED_SYMBOLS
	ANDROID_ARM_MODE
	ANDROID_ARM_NEON
	ANDROID_DISABLE_NO_EXECUTE
	ANDROID_DISABLE_RELRO
	ANDROID_DISABLE_FORMAT_STRING_CHECKS
	ANDROID_CCACHE)

# Standard cross-compiling stuff.
set(ANDROID TRUE)
set(CMAKE_SYSTEM_NAME Android)
set(CMAKE_SYSTEM_VERSION ${ANDROID_PLATFORM_LEVEL})
set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_PACKAGE ONLY)

# ABI.
set(CMAKE_ANDROID_ARCH_ABI ${ANDROID_ABI})
if(ANDROID_ABI MATCHES "^armeabi(-v7a)?$")
	set(ANDROID_SYSROOT_ABI arm)
	set(ANDROID_TOOLCHAIN_NAME arm-linux-androideabi)
	set(ANDROID_TOOLCHAIN_ROOT ${ANDROID_TOOLCHAIN_NAME})
	if(ANDROID_ABI STREQUAL armeabi)
		set(CMAKE_SYSTEM_PROCESSOR armv5te)
		set(ANDROID_LLVM_TRIPLE armv5te-none-linux-androideabi)
	elseif(ANDROID_ABI STREQUAL armeabi-v7a)
		set(CMAKE_SYSTEM_PROCESSOR armv7-a)
		set(ANDROID_LLVM_TRIPLE armv7-none-linux-androideabi)
	endif()
elseif(ANDROID_ABI STREQUAL arm64-v8a)
	set(ANDROID_SYSROOT_ABI arm64)
	set(CMAKE_SYSTEM_PROCESSOR aarch64)
	set(ANDROID_TOOLCHAIN_NAME aarch64-linux-android)
	set(ANDROID_TOOLCHAIN_ROOT ${ANDROID_TOOLCHAIN_NAME})
	set(ANDROID_LLVM_TRIPLE aarch64-none-linux-android)
elseif(ANDROID_ABI STREQUAL x86)
	set(ANDROID_SYSROOT_ABI x86)
	set(CMAKE_SYSTEM_PROCESSOR i686)
	set(ANDROID_TOOLCHAIN_NAME i686-linux-android)
	set(ANDROID_TOOLCHAIN_ROOT ${ANDROID_ABI})
	set(ANDROID_LLVM_TRIPLE i686-none-linux-android)
elseif(ANDROID_ABI STREQUAL x86_64)
	set(ANDROID_SYSROOT_ABI x86_64)
	set(CMAKE_SYSTEM_PROCESSOR x86_64)
	set(ANDROID_TOOLCHAIN_NAME x86_64-linux-android)
	set(ANDROID_TOOLCHAIN_ROOT ${ANDROID_ABI})
	set(ANDROID_LLVM_TRIPLE x86_64-none-linux-android)
elseif(ANDROID_ABI STREQUAL mips)
	set(ANDROID_SYSROOT_ABI mips)
	set(CMAKE_SYSTEM_PROCESSOR mips)
	set(ANDROID_TOOLCHAIN_NAME mipsel-linux-android)
	set(ANDROID_TOOLCHAIN_ROOT ${ANDROID_TOOLCHAIN_NAME})
	set(ANDROID_LLVM_TRIPLE mipsel-none-linux-android)
elseif(ANDROID_ABI STREQUAL mips64)
	set(ANDROID_SYSROOT_ABI mips64)
	set(CMAKE_SYSTEM_PROCESSOR mips64)
	set(ANDROID_TOOLCHAIN_NAME mips64el-linux-android)
	set(ANDROID_TOOLCHAIN_ROOT ${ANDROID_TOOLCHAIN_NAME})
	set(ANDROID_LLVM_TRIPLE mips64el-none-linux-android)
else()
	message(FATAL_ERROR "Invalid Android ABI: ${ANDROID_ABI}.")
endif()

# STL.
set(ANDROID_STL_STATIC_LIBRARIES)
set(ANDROID_STL_SHARED_LIBRARIES)
if(ANDROID_STL STREQUAL system)
	set(ANDROID_STL_STATIC_LIBRARIES
		supc++)
elseif(ANDROID_STL STREQUAL stlport_static)
	set(ANDROID_STL_STATIC_LIBRARIES
		stlport_static)
elseif(ANDROID_STL STREQUAL stlport_shared)
	set(ANDROID_STL_SHARED_LIBRARIES
		stlport_shared)
elseif(ANDROID_STL STREQUAL gnustl_static)
	set(ANDROID_STL_STATIC_LIBRARIES
		gnustl_static)
elseif(ANDROID_STL STREQUAL gnustl_shared)
	set(ANDROID_STL_STATIC_LIBRARIES
		supc++)
	set(ANDROID_STL_SHARED_LIBRARIES
		gnustl_shared)
elseif(ANDROID_STL STREQUAL c++_static)
	set(ANDROID_STL_STATIC_LIBRARIES
		c++_static
		c++abi
		unwind
		android_support)
elseif(ANDROID_STL STREQUAL c++_shared)
	set(ANDROID_STL_STATIC_LIBRARIES
		unwind)
	set(ANDROID_STL_SHARED_LIBRARIES
		c++_shared)
elseif(ANDROID_STL STREQUAL none)
else()
	message(FATAL_ERROR "Invalid Android STL: ${ANDROID_STL}.")
endif()

# Sysroot.
set(CMAKE_SYSROOT "${ANDROID_NDK}/platforms/${ANDROID_PLATFORM}/arch-${ANDROID_SYSROOT_ABI}")

# Toolchain.
if(CMAKE_HOST_SYSTEM_NAME STREQUAL Linux)
	set(ANDROID_HOST_TAG linux-x86_64)
elseif(CMAKE_HOST_SYSTEM_NAME STREQUAL Darwin)
	set(ANDROID_HOST_TAG darwin-x86_64)
elseif(CMAKE_HOST_SYSTEM_NAME STREQUAL Windows)
	set(ANDROID_HOST_TAG windows-x86_64)
endif()
set(ANDROID_TOOLCHAIN_ROOT "${ANDROID_NDK}/toolchains/${ANDROID_TOOLCHAIN_ROOT}-4.9/prebuilt/${ANDROID_HOST_TAG}")
set(ANDROID_TOOLCHAIN_PREFIX "${ANDROID_TOOLCHAIN_ROOT}/bin/${ANDROID_TOOLCHAIN_NAME}-")
if(CMAKE_HOST_SYSTEM_NAME STREQUAL Windows)
	set(ANDROID_TOOLCHAIN_SUFFIX .exe)
endif()
if(ANDROID_TOOLCHAIN STREQUAL clang)
	set(ANDROID_LLVM_TOOLCHAIN_PREFIX "${ANDROID_NDK}/toolchains/llvm-3.6/prebuilt/${ANDROID_HOST_TAG}/bin/")
	set(ANDROID_C_COMPILER   "${ANDROID_LLVM_TOOLCHAIN_PREFIX}clang${ANDROID_TOOLCHAIN_SUFFIX}")
	set(ANDROID_CXX_COMPILER "${ANDROID_LLVM_TOOLCHAIN_PREFIX}clang++${ANDROID_TOOLCHAIN_SUFFIX}")
	# Clang can fail to compile if CMake doesn't correctly supply the target and
	# external toolchain, but to do so, CMake needs to already know that the
	# compiler is clang. Tell CMake that the compiler is really clang, but don't
	# use CMakeForceCompiler, since we still want compile checks. We only want
	# to skip the compiler ID detection step.
	set(CMAKE_C_COMPILER_ID_RUN TRUE)
	set(CMAKE_CXX_COMPILER_ID_RUN TRUE)
	set(CMAKE_C_COMPILER_ID Clang)
	set(CMAKE_CXX_COMPILER_ID Clang)
	set(CMAKE_C_COMPILER_TARGET   ${ANDROID_LLVM_TRIPLE})
	set(CMAKE_CXX_COMPILER_TARGET ${ANDROID_LLVM_TRIPLE})
	set(CMAKE_C_COMPILER_EXTERNAL_TOOLCHAIN   "${ANDROID_TOOLCHAIN_ROOT}")
	set(CMAKE_CXX_COMPILER_EXTERNAL_TOOLCHAIN "${ANDROID_TOOLCHAIN_ROOT}")
elseif(ANDROID_TOOLCHAIN STREQUAL gcc)
	set(ANDROID_C_COMPILER   "${ANDROID_TOOLCHAIN_PREFIX}gcc${ANDROID_TOOLCHAIN_SUFFIX}")
	set(ANDROID_CXX_COMPILER "${ANDROID_TOOLCHAIN_PREFIX}g++${ANDROID_TOOLCHAIN_SUFFIX}")
else()
	message(FATAL_ERROR "Invalid Android toolchain: ${ANDROID_TOOLCHAIN}.")
endif()

if(NOT IS_DIRECTORY "${ANDROID_NDK}/platforms/${ANDROID_PLATFORM}")
	message("${ANDROID_NDK}/platforms/${ANDROID_PLATFORM}")
	message(FATAL_ERROR "Invalid Android platform: ${ANDROID_PLATFORM}.")
elseif(NOT IS_DIRECTORY "${CMAKE_SYSROOT}")
	message(FATAL_ERROR "Invalid Android sysroot: ${CMAKE_SYSROOT}.")
endif()

set(ANDROID_COMPILER_FLAGS)
set(ANDROID_COMPILER_FLAGS_CXX)
set(ANDROID_COMPILER_FLAGS_DEBUG)
set(ANDROID_COMPILER_FLAGS_RELEASE)
set(ANDROID_LINKER_FLAGS)
set(ANDROID_LINKER_FLAGS_EXE)

# Generic flags.
list(APPEND ANDROID_COMPILER_FLAGS
	-g
	-DANDROID
	-ffunction-sections
	-funwind-tables
	-fstack-protector-strong
	-no-canonical-prefixes)
list(APPEND ANDROID_COMPILER_FLAGS_CXX
	-fno-exceptions
	-fno-rtti)
list(APPEND ANDROID_LINKER_FLAGS
	-Wl,--build-id
	-Wl,--warn-shared-textrel
	-Wl,--fatal-warnings)
list(APPEND ANDROID_LINKER_FLAGS_EXE
	-Wl,--gc-sections
	-Wl,-z,nocopyreloc)

# Debug and release flags.
list(APPEND ANDROID_COMPILER_FLAGS_DEBUG
	-O0)
if(ANDROID_ABI MATCHES "^armeabi")
	list(APPEND ANDROID_COMPILER_FLAGS_RELEASE
		-Os)
else()
	list(APPEND ANDROID_COMPILER_FLAGS_RELEASE
		-O2)
endif()
list(APPEND ANDROID_COMPILER_FLAGS_RELEASE
	-DNDEBUG)
if(ANDROID_TOOLCHAIN STREQUAL clang)
	list(APPEND ANDROID_COMPILER_FLAGS_DEBUG
		-fno-limit-debug-info)
endif()

# Toolchain and ABI specific flags.
if(ANDROID_ABI STREQUAL armeabi)
	list(APPEND ANDROID_COMPILER_FLAGS
		-march=armv5te
		-mtune=xscale
		-msoft-float)
endif()
if(ANDROID_ABI STREQUAL armeabi-v7a)
	list(APPEND ANDROID_COMPILER_FLAGS
		-march=armv7-a
		-mfloat-abi=softfp
		-mfpu=vfpv3-d16)
	list(APPEND ANDROID_LINKER_FLAGS
		-Wl,--fix-cortex-a8)
endif()
if(ANDROID_ABI MATCHES "^armeabi" AND ANDROID_TOOLCHAIN STREQUAL clang)
	# Disable integrated-as for better compatibility.
	list(APPEND ANDROID_COMPILER_FLAGS
		-fno-integrated-as)
endif()
if(ANDROID_ABI STREQUAL mips AND ANDROID_TOOLCHAIN STREQUAL clang)
	list(APPEND ANDROID_COMPILER_FLAGS
		-mips32)
endif()

# STL specific flags.
if(ANDROID_STL STREQUAL system)
	set(ANDROID_STL_PREFIX gnu-libstdc++/4.9)
	set(CMAKE_CXX_STANDARD_INCLUDE_DIRECTORIES
		"${ANDROID_NDK}/sources/cxx-stl/system/include")
elseif(ANDROID_STL MATCHES "^stlport_")
	set(ANDROID_STL_PREFIX stlport)
	set(CMAKE_CXX_STANDARD_INCLUDE_DIRECTORIES
		"${ANDROID_NDK}/sources/cxx-stl/${ANDROID_STL_PREFIX}/stlport"
		"${ANDROID_NDK}/sources/cxx-stl/gabi++/include")
elseif(ANDROID_STL MATCHES "^gnustl_")
	set(ANDROID_STL_PREFIX gnu-libstdc++/4.9)
	set(CMAKE_CXX_STANDARD_INCLUDE_DIRECTORIES
		"${ANDROID_NDK}/sources/cxx-stl/${ANDROID_STL_PREFIX}/include"
		"${ANDROID_NDK}/sources/cxx-stl/${ANDROID_STL_PREFIX}/libs/${ANDROID_ABI}/include"
		"${ANDROID_NDK}/sources/cxx-stl/${ANDROID_STL_PREFIX}/include/backward")
elseif(ANDROID_STL MATCHES "^c\\+\\+_")
	set(ANDROID_STL_PREFIX llvm-libc++)
	if(ANDROID_ABI MATCHES "^armeabi")
		list(APPEND ANDROID_LINKER_FLAGS
			-Wl,--exclude-libs,libunwind.a)
	else()
		list(REMOVE_ITEM ANDROID_STL_STATIC_LIBRARIES
			unwind)
	endif()
	list(APPEND ANDROID_COMPILER_FLAGS_CXX
		-std=c++11)
	if(ANDROID_TOOLCHAIN STREQUAL gcc)
		list(APPEND ANDROID_COMPILER_FLAGS_CXX
			-fno-strict-aliasing)
	endif()
	set(CMAKE_CXX_STANDARD_INCLUDE_DIRECTORIES
		"${ANDROID_NDK}/sources/cxx-stl/${ANDROID_STL_PREFIX}/include"
		"${ANDROID_NDK}/sources/android/support/include"
		"${ANDROID_NDK}/sources/cxx-stl/${ANDROID_STL_PREFIX}abi/include")
endif()
set(ANDROID_CXX_STANDARD_LIBRARIES)
foreach(library ${ANDROID_STL_STATIC_LIBRARIES})
	list(APPEND ANDROID_CXX_STANDARD_LIBRARIES
		"${ANDROID_NDK}/sources/cxx-stl/${ANDROID_STL_PREFIX}/libs/${ANDROID_ABI}/lib${library}.a")
endforeach()
foreach(library ${ANDROID_STL_SHARED_LIBRARIES})
	list(APPEND ANDROID_CXX_STANDARD_LIBRARIES
		"${ANDROID_NDK}/sources/cxx-stl/${ANDROID_STL_PREFIX}/libs/${ANDROID_ABI}/lib${library}.so")
endforeach()
if(ANDROID_ABI STREQUAL armeabi AND NOT ANDROID_STL MATCHES "^(none|system)$")
	list(APPEND ANDROID_CXX_STANDARD_LIBRARIES
		-latomic)
endif()
set(CMAKE_C_STANDARD_LIBRARIES_INIT "-lm")
set(CMAKE_CXX_STANDARD_LIBRARIES_INIT "${CMAKE_C_STANDARD_LIBRARIES_INIT}")
if(ANDROID_CXX_STANDARD_LIBRARIES)
	string(REPLACE ";" "\" \"" ANDROID_CXX_STANDARD_LIBRARIES "\"${ANDROID_CXX_STANDARD_LIBRARIES}\"")
	set(CMAKE_CXX_STANDARD_LIBRARIES_INIT "${CMAKE_CXX_STANDARD_LIBRARIES_INIT} ${ANDROID_CXX_STANDARD_LIBRARIES}")
endif()

# Configuration specific flags.
if(ANDROID_PIE)
	set(CMAKE_POSITION_INDEPENDENT_CODE TRUE)
	list(APPEND ANDROID_LINKER_FLAGS_EXE
		-pie
		-fPIE)
endif()
if(ANDROID_CPP_FEATURES)
	separate_arguments(ANDROID_CPP_FEATURES)
	foreach(feature ${ANDROID_CPP_FEATURES})
		if(NOT ${feature} MATCHES "^(rtti|exceptions)$")
			message(FATAL_ERROR "Invalid Android C++ feature: ${feature}.")
		endif()
		list(APPEND ANDROID_COMPILER_FLAGS_CXX
			-f${feature})
	endforeach()
	string(REPLACE ";" " " ANDROID_CPP_FEATURES "${ANDROID_CPP_FEATURES}")
endif()
if(NOT ANDROID_ALLOW_UNDEFINED_SYMBOLS)
	list(APPEND ANDROID_LINKER_FLAGS
		-Wl,--no-undefined)
endif()
if(ANDROID_ABI MATCHES "armeabi")
	if(ANDROID_ARM_MODE STREQUAL thumb)
		list(APPEND ANDROID_COMPILER_FLAGS
			-mthumb)
	elseif(ANDROID_ARM_MODE STREQUAL arm)
		list(APPEND ANDROID_COMPILER_FLAGS
			-marm)
	else()
		message(FATAL_ERROR "Invalid Android ARM mode: ${ANDROID_ARM_MODE}.")
	endif()
	if(ANDROID_ABI STREQUAL armeabi-v7a AND ANDROID_ARM_NEON)
		list(APPEND ANDROID_COMPILER_FLAGS
			-mfpu=neon)
	endif()
endif()
if(ANDROID_DISABLE_NO_EXECUTE)
	list(APPEND ANDROID_COMPILER_FLAGS
		-Wa,--execstack)
	list(APPEND ANDROID_LINKER_FLAGS
		-Wl,-z,execstack)
else()
	list(APPEND ANDROID_COMPILER_FLAGS
		-Wa,--noexecstack)
	list(APPEND ANDROID_LINKER_FLAGS
		-Wl,-z,noexecstack)
endif()
if(ANDROID_TOOLCHAIN STREQUAL clang)
	# CMake automatically forwards all compiler flags to the linker,
	# and clang doesn't like having -Wa flags being used for linking.
	# To prevent CMake from doing this would require meddling with
	# the CMAKE_<LANG>_COMPILE_OBJECT rules, which would get quite messy.
	list(APPEND ANDROID_LINKER_FLAGS
		-Qunused-arguments)
endif()
if(ANDROID_DISABLE_RELRO)
	list(APPEND ANDROID_LINKER_FLAGS
		-Wl,-z,norelro -Wl,-z,lazy)
else()
	list(APPEND ANDROID_LINKER_FLAGS
		-Wl,-z,relro -Wl,-z,now)
endif()
if(ANDROID_DISABLE_FORMAT_STRING_CHECKS)
	list(APPEND ANDROID_COMPILER_FLAGS
		-Wno-error=format-security)
else()
	list(APPEND ANDROID_COMPILER_FLAGS
		-Wformat -Werror=format-security)
endif()

# Convert these lists into strings.
string(REPLACE ";" " " ANDROID_COMPILER_FLAGS         "${ANDROID_COMPILER_FLAGS}")
string(REPLACE ";" " " ANDROID_COMPILER_FLAGS_CXX     "${ANDROID_COMPILER_FLAGS_CXX}")
string(REPLACE ";" " " ANDROID_COMPILER_FLAGS_DEBUG   "${ANDROID_COMPILER_FLAGS_DEBUG}")
string(REPLACE ";" " " ANDROID_COMPILER_FLAGS_RELEASE "${ANDROID_COMPILER_FLAGS_RELEASE}")
string(REPLACE ";" " " ANDROID_LINKER_FLAGS           "${ANDROID_LINKER_FLAGS}")
string(REPLACE ";" " " ANDROID_LINKER_FLAGS_EXE       "${ANDROID_LINKER_FLAGS_EXE}")

if(ANDROID_CCACHE)
	set(CMAKE_C_COMPILER_LAUNCHER   "${ANDROID_CCACHE}")
	set(CMAKE_CXX_COMPILER_LAUNCHER "${ANDROID_CCACHE}")
endif()
set(CMAKE_C_COMPILER        "${ANDROID_C_COMPILER}")
set(CMAKE_CXX_COMPILER      "${ANDROID_CXX_COMPILER}")
set(_CMAKE_TOOLCHAIN_PREFIX "${ANDROID_TOOLCHAIN_PREFIX}")

# Set or retrieve the cached flags.
# This is necessary in case the user sets/changes flags in subsequent
# configures. If we included the Android flags in here, they would get
# overwritten.
set(CMAKE_C_FLAGS ""
	CACHE STRING "Flags used by the compiler during all build types.")
set(CMAKE_CXX_FLAGS ""
	CACHE STRING "Flags used by the compiler during all build types.")
set(CMAKE_C_FLAGS_DEBUG ""
	CACHE STRING "Flags used by the compiler during debug builds.")
set(CMAKE_CXX_FLAGS_DEBUG ""
	CACHE STRING "Flags used by the compiler during debug builds.")
set(CMAKE_C_FLAGS_RELEASE ""
	CACHE STRING "Flags used by the compiler during release builds.")
set(CMAKE_CXX_FLAGS_RELEASE ""
	CACHE STRING "Flags used by the compiler during release builds.")
set(CMAKE_MODULE_LINKER_FLAGS ""
	CACHE STRING "Flags used by the linker during the creation of modules.")
set(CMAKE_SHARED_LINKER_FLAGS ""
	CACHE STRING "Flags used by the linker during the creation of dll's.")
set(CMAKE_EXE_LINKER_FLAGS ""
	CACHE STRING "Flags used by the linker.")

set(CMAKE_C_FLAGS             "${ANDROID_COMPILER_FLAGS} ${CMAKE_C_FLAGS}")
set(CMAKE_CXX_FLAGS           "${ANDROID_COMPILER_FLAGS} ${ANDROID_COMPILER_FLAGS_CXX} ${CMAKE_CXX_FLAGS}")
set(CMAKE_C_FLAGS_DEBUG       "${ANDROID_COMPILER_FLAGS_DEBUG} ${CMAKE_C_FLAGS_DEBUG}")
set(CMAKE_CXX_FLAGS_DEBUG     "${ANDROID_COMPILER_FLAGS_DEBUG} ${CMAKE_CXX_FLAGS_DEBUG}")
set(CMAKE_C_FLAGS_RELEASE     "${ANDROID_COMPILER_FLAGS_RELEASE} ${CMAKE_C_FLAGS_RELEASE}")
set(CMAKE_CXX_FLAGS_RELEASE   "${ANDROID_COMPILER_FLAGS_RELEASE} ${CMAKE_CXX_FLAGS_RELEASE}")
set(CMAKE_SHARED_LINKER_FLAGS "${ANDROID_LINKER_FLAGS} ${CMAKE_SHARED_LINKER_FLAGS}")
set(CMAKE_MODULE_LINKER_FLAGS "${ANDROID_LINKER_FLAGS} ${CMAKE_MODULE_LINKER_FLAGS}")
set(CMAKE_EXE_LINKER_FLAGS    "${ANDROID_LINKER_FLAGS} ${ANDROID_LINKER_FLAGS_EXE} ${CMAKE_EXE_LINKER_FLAGS}")

if(CMAKE_C_FLAGS)
	message("CMAKE_C_FLAGS = ${CMAKE_C_FlAGS}")
endif()
if(CMAKE_CXX_FLAGS)
	message("CMAKE_CXX_FLAGS = ${CMAKE_CXX_FLAGS}")
endif()
if(CMAKE_C_FLAGS_DEBUG)
	message("CMAKE_C_FLAGS_DEBUG = ${CMAKE_C_FLAGS_DEBUG}")
endif()
if(CMAKE_CXX_FLAGS_DEBUG)
	message("CMAKE_CXX_FLAGS_DEBUG = ${CMAKE_CXX_FLAGS_DEBUG}")
endif()
if(CMAKE_SHARED_LINKER_FLAGS)
	message("CMAKE_SHARED_LINKDER_FLAGS = ${CMAKE_SHARED_LINKER_FLAGS}")
endif()
if(CMAKE_MODULE_LINKER_FLAGS)
	message("CMAKE_MODULE_LINKER_FLAGS = ${CMAKE_MODULE_LINKER_FLAGS}")
endif()
if(CMAKE_EXE_LINKER_FLAGS)
	message("CMAKE_EXE_LINKER_FLAGS = ${CMAKE_EXE_LINKER_FLAGS}")
endif()

# Compatibility for read-only variables.
# Read-only variables for compatibility with the other toolchain file.
# We'll keep these around for the existing projects that still use them.
# TODO: All of the variables here have equivalents in our standard set of
# configurable variables, so we can remove these once most of our users migrate
# to those variables.
set(ANDROID_NATIVE_API_LEVEL ${ANDROID_PLATFORM_LEVEL})
if(ANDROID_ALLOW_UNDEFINED_SYMBOLS)
	set(ANDROID_SO_UNDEFINED TRUE)
else()
	set(ANDROID_NO_UNDEFINED TRUE)
endif()
set(ANDROID_FUNCTION_LEVEL_LINKING TRUE)
set(ANDROID_GOLD_LINKER TRUE)
if(NOT ANDROID_DISABLE_NO_EXECUTE)
	set(ANDROID_NOEXECSTACK TRUE)
endif()
if(NOT ANDROID_DISABLE_RELRO)
	set(ANDROID_RELRO TRUE)
endif()
if(ANDROID_ARM_MODE STREQUAL arm)
	set(ANDROID_FORCE_ARM_BUILD TRUE)
endif()
if(ANDROID_CPP_FEATURES MATCHES "rtti"
		AND ANDROID_CPP_FEATURES MATCHES "exceptions")
	set(ANDROID_STL_FORCE_FEATURES TRUE)
endif()
if(ANDROID_CCACHE)
	set(NDK_CCACHE "${ANDROID_CCACHE}")
endif()
if(ANDROID_TOOLCHAIN STREQUAL clang)
	set(ANDROID_TOOLCHAIN_NAME ${ANDROID_TOOLCHAIN_NAME}-clang)
else()
	set(ANDROID_TOOLCHAIN_NAME ${ANDROID_TOOLCHAIN_NAME}-4.9)
endif()
set(ANDROID_NDK_HOST_X64 TRUE)
set(ANDROID_NDK_LAYOUT RELEASE)
if(ANDROID_ABI STREQUAL armeabi)
	set(ARMEABI TRUE)
elseif(ANDROID_ABI STREQUAL armeabi-v7a)
	set(ARMEABI_V7A TRUE)
	if(ANDROID_ARM_NEON)
		set(NEON TRUE)
	endif()
elseif(ANDROID_ABI STREQUAL arm64-v8a)
	set(ARM64_V8A TRUE)
elseif(ANDROID_ABI STREQUAL x86)
	set(X86 TRUE)
elseif(ANDROID_ABI STREQUAL x86_64)
	set(X86_64 TRUE)
elseif(ANDROID_ABI STREQUAL mips)
	set(MIPS TRUE)
elseif(ANDROID_ABI STREQUAL MIPS64)
	set(MIPS64 TRUE)
endif()
set(ANDROID_NDK_HOST_SYSTEM_NAME ${ANDROID_HOST_TAG})
set(ANDROID_NDK_ABI_NAME ${ANDROID_ABI})
set(ANDROID_ARCH_NAME ${ANDROID_SYSROOT_ABI})
set(ANDROID_SYSROOT "${CMAKE_SYSROOT}")
set(TOOL_OS_SUFFIX ${ANDROID_TOOLCHAIN_SUFFIX})
if(ANDROID_TOOLCHAIN STREQUAL clang)
	set(ANDROID_COMPILER_IS_CLANG TRUE)
endif()
