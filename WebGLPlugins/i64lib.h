/*
 *Tencent is pleased to support the open source community by making xLua available.
 *Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 *Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 *http://opensource.org/licenses/MIT
 *Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#ifndef I64LIB_H
#define I64LIB_H

#include <stdint.h>
#include "lua.h"
#include "lauxlib.h"
#include "lualib.h"

#ifdef __cplusplus
#if __cplusplus
extern "C"{
#endif
#endif /* __cplusplus */

LUALIB_API void lua_pushint64(lua_State* L, int64_t n);
LUALIB_API void lua_pushuint64(lua_State* L, uint64_t n);

LUALIB_API int lua_isint64(lua_State* L, int pos);
LUALIB_API int lua_isuint64(lua_State* L, int pos);

LUALIB_API int64_t lua_toint64(lua_State* L, int pos);
LUALIB_API uint64_t lua_touint64(lua_State* L, int pos);

#ifdef __cplusplus
#if __cplusplus
}
#endif
#endif /* __cplusplus */ 


#endif