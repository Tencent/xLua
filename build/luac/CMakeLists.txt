# Tencent is pleased to support the open source community by making xLua available.
# Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
# Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
# http://opensource.org/licenses/MIT
# Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

cmake_minimum_required(VERSION 2.8)

if ( WIN32 AND NOT CYGWIN AND NOT ( CMAKE_SYSTEM_NAME STREQUAL "WindowsStore" ) AND NOT ANDROID)
	set(CMAKE_C_FLAGS_RELEASE "${CMAKE_C_FLAGS_RELEASE} /MT" CACHE STRING "")
	set(CMAKE_C_FLAGS_DEBUG "${CMAKE_C_FLAGS_DEBUG} /MTd" CACHE STRING "")
	set(CMAKE_CXX_FLAGS_RELEASE "${CMAKE_CXX_FLAGS_RELEASE} /MT" CACHE STRING "")
	set(CMAKE_CXX_FLAGS_DEBUG "${CMAKE_CXX_FLAGS_DEBUG} /MTd" CACHE STRING "")
endif ()

project(luac)


find_path(LUAC_PROJECT_DIR NAMES SConstruct
    PATHS 
    ${CMAKE_SOURCE_DIR}
    NO_DEFAULT_PATH
    )

MARK_AS_ADVANCED(LUAC_PROJECT_DIR)

set(LUA_SRC_PATH ../lua-5.3.5/src)


set ( LUA_IDSIZE 120 CACHE NUMBER "gives the maximum size for the description of the source." )

configure_file ( ${LUA_SRC_PATH}/luaconf.h.in ${CMAKE_CURRENT_BINARY_DIR}/luaconf.h )

include_directories(
	${CMAKE_SOURCE_DIR}
	${LUA_SRC_PATH}
	${CMAKE_CURRENT_BINARY_DIR}
)

set ( LUA_CORE ${LUA_SRC_PATH}/lapi.c ${LUA_SRC_PATH}/lcode.c ${LUA_SRC_PATH}/lctype.c ${LUA_SRC_PATH}/ldebug.c ${LUA_SRC_PATH}/ldo.c ${LUA_SRC_PATH}/ldump.c
  ${LUA_SRC_PATH}/lfunc.c ${LUA_SRC_PATH}/lgc.c ${LUA_SRC_PATH}/llex.c ${LUA_SRC_PATH}/lmem.c ${LUA_SRC_PATH}/lobject.c ${LUA_SRC_PATH}/lopcodes.c ${LUA_SRC_PATH}/lparser.c
  ${LUA_SRC_PATH}/lstate.c ${LUA_SRC_PATH}/lstring.c ${LUA_SRC_PATH}/ltable.c ${LUA_SRC_PATH}/ltm.c ${LUA_SRC_PATH}/lundump.c ${LUA_SRC_PATH}/lvm.c ${LUA_SRC_PATH}/lzio.c )
set ( LUA_LIB ${LUA_SRC_PATH}/lauxlib.c ${LUA_SRC_PATH}/lbaselib.c ${LUA_SRC_PATH}/lbitlib.c ${LUA_SRC_PATH}/lcorolib.c ${LUA_SRC_PATH}/ldblib.c
  ${LUA_SRC_PATH}/liolib.c ${LUA_SRC_PATH}/lmathlib.c ${LUA_SRC_PATH}/loslib.c ${LUA_SRC_PATH}/lstrlib.c ${LUA_SRC_PATH}/ltablib.c ${LUA_SRC_PATH}/linit.c
  ${LUA_SRC_PATH}/lutf8lib.c ${LUA_SRC_PATH}/loadlib.c )
set ( LUAC ${LUA_SRC_PATH}/luac.c )
set ( LUA ${LUA_SRC_PATH}/lua.c )

macro(source_group_by_dir proj_dir source_files)
    if(MSVC OR APPLE)
        get_filename_component(sgbd_cur_dir ${proj_dir} ABSOLUTE)
        foreach(sgbd_file ${${source_files}})
			get_filename_component(sgbd_abs_file ${sgbd_file} ABSOLUTE)
            file(RELATIVE_PATH sgbd_fpath ${sgbd_cur_dir} ${sgbd_abs_file})
            string(REGEX REPLACE "\(.*\)/.*" \\1 sgbd_group_name ${sgbd_fpath})
            string(COMPARE EQUAL ${sgbd_fpath} ${sgbd_group_name} sgbd_nogroup)
            string(REPLACE "/" "\\" sgbd_group_name ${sgbd_group_name})
            if(sgbd_nogroup)
                set(sgbd_group_name "\\")
            endif(sgbd_nogroup)
            source_group(${sgbd_group_name} FILES ${sgbd_file})
        endforeach(sgbd_file)
    endif(MSVC OR APPLE)
endmacro(source_group_by_dir)

source_group_by_dir(${CMAKE_CURRENT_SOURCE_DIR} LUA_CORE)
source_group_by_dir(${CMAKE_CURRENT_SOURCE_DIR} LUA_LIB)
source_group_by_dir(${CMAKE_CURRENT_SOURCE_DIR} LUAC)

add_executable (luac ${LUA_CORE} ${LUA_LIB} ${LUAC})
add_executable (lua ${LUA_CORE} ${LUA_LIB} ${LUA})

if (LUAC_COMPATIBLE_FORMAT)
target_compile_definitions (luac PRIVATE LUAC_COMPATIBLE_FORMAT)
target_compile_definitions (lua PRIVATE LUAC_COMPATIBLE_FORMAT)
endif ()

