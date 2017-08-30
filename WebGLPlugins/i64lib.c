/*
 *Tencent is pleased to support the open source community by making xLua available.
 *Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 *Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 *http://opensource.org/licenses/MIT
 *Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#define LUA_LIB

#include "i64lib.h"
#include <string.h>
#include <math.h>
#include <stdlib.h>

#if ( defined (_WIN32) ||  defined (_WIN64) ) && !defined (__MINGW32__) && !defined (__MINGW64__)

#if !defined(PRId64)
# if __WORDSIZE == 64  
#  define PRId64    "ld"   
# else  
#  define PRId64    "lld"  
# endif 
#endif

#if !defined(PRIu64)
# if __WORDSIZE == 64  
#  define PRIu64    "lu"   
# else  
#  define PRIu64    "llu"  
# endif
#endif

#else
#include <inttypes.h>
#endif

#if LUA_VERSION_NUM == 501

#define INT64_META_REF 8

enum IntegerType {
	Int,
	UInt,
	Num
};

typedef struct {
	int fake_id;
	int8_t type;
    union {
		int64_t i64;
		uint64_t u64;
	} data;
} Integer64;

LUALIB_API void lua_pushint64(lua_State* L, int64_t n) {
	Integer64* p = (Integer64*)lua_newuserdata(L, sizeof(Integer64));
	p->fake_id = -1;
	p->data.i64 = n;
	p->type = Int;
	lua_rawgeti(L, LUA_REGISTRYINDEX, INT64_META_REF);
	lua_setmetatable(L, -2);            
}

LUALIB_API int lua_isint64(lua_State* L, int pos) {
	int equal;
    Integer64* p = (Integer64*)lua_touserdata(L, pos);

    if (p != NULL) {
        if (lua_getmetatable(L, pos)) {            
			lua_rawgeti(L, LUA_REGISTRYINDEX, INT64_META_REF);
            equal = lua_rawequal(L, -1, -2);
            lua_pop(L, 2);  

            return equal && (p->type == Int);
        }
    }

    return 0;
}

LUALIB_API int lua_isint64_or_uint64(lua_State* L, int pos) {
	int equal;
    Integer64* p = (Integer64*)lua_touserdata(L, pos);

    if (p != NULL) {
        if (lua_getmetatable(L, pos)) {            
			lua_rawgeti(L, LUA_REGISTRYINDEX, INT64_META_REF);
            equal = lua_rawequal(L, -1, -2);
            lua_pop(L, 2);  

            return equal;
        }
    }

    return 0;
}

LUALIB_API int64_t lua_toint64(lua_State* L, int pos) {
    int64_t n = 0;
    int type = lua_type(L, pos);
    
    switch(type) {
        case LUA_TNUMBER:
            n = (int64_t)lua_tonumber(L, pos);
            break;
        case LUA_TUSERDATA:
		    if (lua_isint64_or_uint64(L, pos)) {
				n = ((Integer64*)lua_touserdata(L, pos))->data.i64;
			}
			break;
        default:
            break;
    }
    
    return n;
}

#if defined(UINT_ESPECIALLY)
LUALIB_API void lua_pushuint64(lua_State* L, uint64_t n) {
	Integer64* p = (Integer64*)lua_newuserdata(L, sizeof(Integer64));
	p->fake_id = -1;
	p->data.u64 = n;
	p->type = UInt;
	lua_rawgeti(L, LUA_REGISTRYINDEX, INT64_META_REF);
	lua_setmetatable(L, -2);            
}


LUALIB_API int lua_isuint64(lua_State* L, int pos) {
	int equal;
    Integer64* p = (Integer64*)lua_touserdata(L, pos);

    if (p != NULL) {
        if (lua_getmetatable(L, pos)) {            
			lua_rawgeti(L, LUA_REGISTRYINDEX, INT64_META_REF);
            equal = lua_rawequal(L, -1, -2);
            lua_pop(L, 2);  

            return equal && (p->type == UInt);
        }
    }

    return 0;
}

LUALIB_API uint64_t lua_touint64(lua_State* L, int pos) {
    uint64_t n = 0;
    int type = lua_type(L, pos);
    
    switch(type) {
        case LUA_TNUMBER:
            n = (int64_t)lua_tonumber(L, pos);
            break;
        case LUA_TUSERDATA:
		    if (lua_isint64_or_uint64(L, pos)) {
				n = ((Integer64*)lua_touserdata(L, pos))->data.u64;
			}
			break;
        default:
            break;
    }
    
    return n;
}
#else
LUALIB_API void lua_pushuint64(lua_State* L, uint64_t n) {
	lua_pushint64(L, (int64_t)n);
}

LUALIB_API int lua_isuint64(lua_State* L, int pos) {
    return lua_isint64(L, pos);
}

LUALIB_API uint64_t lua_touint64(lua_State* L, int pos) {
    return (uint64_t)lua_toint64(L, pos);
}
#endif

LUALIB_API int lua_isinteger64(lua_State* L, int pos) {
	int equal;
    Integer64* p = (Integer64*)lua_touserdata(L, pos);

    if (p != NULL) {
        if (lua_getmetatable(L, pos)) {            
			lua_rawgeti(L, LUA_REGISTRYINDEX, INT64_META_REF);
            equal = lua_rawequal(L, -1, -2);
            lua_pop(L, 2);  

            return equal;
        }
    }

    return 0;
}

static Integer64 lua_checkinteger64(lua_State* L, int pos) {
    Integer64 n = {0};
    int type = lua_type(L, pos);
    
    switch(type) {
        case LUA_TNUMBER:
            n.data.i64 = (int64_t)lua_tonumber(L, pos);
			n.type = Num;
            break;
        case LUA_TUSERDATA:
		    if (lua_isinteger64(L, pos)) {
				n = *(Integer64*)lua_touserdata(L, pos);
				break;
			} else {
				luaL_typerror(L, pos, "Integer64");
				return n;
			}
        default:
            luaL_typerror(L, pos, "Integer64");
			return n;
    }
    
    return n;
}

static int int64_add(lua_State* L) {
    Integer64 lhs = lua_checkinteger64(L, 1);    
    Integer64 rhs = lua_checkinteger64(L, 2);
	
	if (lhs.type != rhs.type && lhs.type != Num && rhs.type != Num) {
		return luaL_error(L, "type not match, lhs is %s, rhs is %s", lhs.type == Int ? "Int64" : "UInt64", rhs.type == Int ? "Int64" : "UInt64");
	} else if (lhs.type == UInt || rhs.type == UInt) {
		lua_pushuint64(L, lhs.data.u64 + rhs.data.u64);
	} else {
		lua_pushint64(L, lhs.data.i64 + rhs.data.i64);
	}
    return 1;
}

static int int64_sub(lua_State* L) {
    Integer64 lhs = lua_checkinteger64(L, 1);    
    Integer64 rhs = lua_checkinteger64(L, 2);
	
    if (lhs.type != rhs.type && lhs.type != Num && rhs.type != Num) {
		return luaL_error(L, "type not match, lhs is %s, rhs is %s", lhs.type == Int ? "Int64" : "UInt64", rhs.type == Int ? "Int64" : "UInt64");
	} else if (lhs.type == UInt || rhs.type == UInt) {
		lua_pushuint64(L, lhs.data.u64 - rhs.data.u64);
	} else {
		lua_pushint64(L, lhs.data.i64 - rhs.data.i64);
	}
    return 1;
}


static int int64_mul(lua_State* L) {
    Integer64 lhs = lua_checkinteger64(L, 1);    
    Integer64 rhs = lua_checkinteger64(L, 2);
	
    if (lhs.type != rhs.type && lhs.type != Num && rhs.type != Num) {
		return luaL_error(L, "type not match, lhs is %s, rhs is %s", lhs.type == Int ? "Int64" : "UInt64", rhs.type == Int ? "Int64" : "UInt64");
	} else if (lhs.type == UInt || rhs.type == UInt) {
		lua_pushuint64(L, lhs.data.u64 * rhs.data.u64);
	} else {
		lua_pushint64(L, lhs.data.i64 * rhs.data.i64);
	}
    return 1;    
}

static int int64_div(lua_State* L) {
    Integer64 lhs = lua_checkinteger64(L, 1);    
    Integer64 rhs = lua_checkinteger64(L, 2);
	
	if (rhs.data.i64 == 0) {
        return luaL_error(L, "div by zero");
    }
	
    if (lhs.type != rhs.type && lhs.type != Num && rhs.type != Num) {
		return luaL_error(L, "type not match, lhs is %s, rhs is %s", lhs.type == Int ? "Int64" : "UInt64", rhs.type == Int ? "Int64" : "UInt64");
	} else if (lhs.type == UInt || rhs.type == UInt) {
		lua_pushuint64(L, lhs.data.u64 / rhs.data.u64);
	} else {
		lua_pushint64(L, lhs.data.i64 / rhs.data.i64);
	}
    return 1;
}

static int int64_mod(lua_State* L) {
    Integer64 lhs = lua_checkinteger64(L, 1);    
    Integer64 rhs = lua_checkinteger64(L, 2);

    if (rhs.data.i64 == 0) {
        return luaL_error(L, "mod by zero");
    }

    if (lhs.type != rhs.type && lhs.type != Num && rhs.type != Num) {
		return luaL_error(L, "type not match, lhs is %s, rhs is %s", lhs.type == Int ? "Int64" : "UInt64", rhs.type == Int ? "Int64" : "UInt64");
	} else if (lhs.type == UInt || rhs.type == UInt) {
		lua_pushuint64(L, lhs.data.u64 % rhs.data.u64);
	} else {
		lua_pushint64(L, lhs.data.i64 % rhs.data.i64);
	}
    return 1;
}

static int int64_unm(lua_State* L) {
    Integer64 lhs = lua_checkinteger64(L, 1); 
    lua_pushint64(L, -lhs.data.i64);
    return 1;
}

static int int64_pow(lua_State* L) {
    Integer64 lhs = lua_checkinteger64(L, 1);    
    Integer64 rhs = lua_checkinteger64(L, 2);

	if (lhs.type != rhs.type && lhs.type != Num && rhs.type != Num) {
		return luaL_error(L, "type not match, lhs is %s, rhs is %s", lhs.type == Int ? "Int64" : "UInt64", rhs.type == Int ? "Int64" : "UInt64");
	} else if (lhs.type == UInt || rhs.type == UInt) {
		lua_pushuint64(L, pow(lhs.data.u64, rhs.data.u64));
	} else {
		lua_pushint64(L, pow(lhs.data.i64, rhs.data.i64));
	}
    return 1;
}

static int int64_eq(lua_State* L) {
    Integer64 lhs = lua_checkinteger64(L, 1);     
    Integer64 rhs = lua_checkinteger64(L, 2); 
    
    if (lhs.type != rhs.type && lhs.type != Num && rhs.type != Num) {
		return luaL_error(L, "type not match, lhs is %s, rhs is %s", lhs.type == Int ? "Int64" : "UInt64", rhs.type == Int ? "Int64" : "UInt64");
	} else if (lhs.type == UInt || rhs.type == UInt) {
		lua_pushboolean(L, lhs.data.u64 == rhs.data.u64);
	} else {
		lua_pushboolean(L, lhs.data.i64 == rhs.data.i64);
	}
    return 1;
}

static int int64_lt(lua_State* L) {
    Integer64 lhs = lua_checkinteger64(L, 1); 
    Integer64 rhs = lua_checkinteger64(L, 2);
	
	if (lhs.type != rhs.type && lhs.type != Num && rhs.type != Num) {
		return luaL_error(L, "type not match, lhs is %s, rhs is %s", lhs.type == Int ? "Int64" : "UInt64", rhs.type == Int ? "Int64" : "UInt64");
	} else if (lhs.type == UInt || rhs.type == UInt) {
		lua_pushboolean(L, lhs.data.u64 < rhs.data.u64);
	} else {
		lua_pushboolean(L, lhs.data.i64 < rhs.data.i64);
	}
    return 1;
}

static int int64_le(lua_State* L) {
	Integer64 lhs = lua_checkinteger64(L, 1); 
    Integer64 rhs = lua_checkinteger64(L, 2);
	
	if (lhs.type != rhs.type && lhs.type != Num && rhs.type != Num) {
		return luaL_error(L, "type not match, lhs is %s, rhs is %s", lhs.type == Int ? "Int64" : "UInt64", rhs.type == Int ? "Int64" : "UInt64");
	} else if (lhs.type == UInt || rhs.type == UInt) {
		lua_pushboolean(L, lhs.data.u64 <= rhs.data.u64);
	} else {
		lua_pushboolean(L, lhs.data.i64 <= rhs.data.i64);
	}
    return 1;
}

static int int64_tostring(lua_State* L) { 
    char temp[72];
	Integer64 lhs = lua_checkinteger64(L, 1); 
	if (lhs.type == UInt) {
		sprintf(temp, "%"PRIu64"U", lhs.data.u64);
	} else {
		sprintf(temp, "%"PRId64, lhs.data.i64);
	}		
    lua_pushstring(L, temp);
    return 1;
}

#endif

#if LUA_VERSION_NUM == 503
LUALIB_API void lua_pushint64(lua_State* L, int64_t n) {
	lua_pushinteger(L, n);
}
LUALIB_API void lua_pushuint64(lua_State* L, uint64_t n) {
	lua_pushinteger(L, n);
}

LUALIB_API int lua_isint64(lua_State* L, int pos) {
	return lua_isinteger(L, pos);
}

LUALIB_API int lua_isuint64(lua_State* L, int pos) {
	return lua_isinteger(L, pos);
}

LUALIB_API int64_t lua_toint64(lua_State* L, int pos) {
	return lua_tointeger(L, pos);
}

LUALIB_API uint64_t lua_touint64(lua_State* L, int pos) {
	return lua_tointeger(L, pos);
}
#endif

static int uint64_tostring(lua_State* L) {
	char temp[72];
	uint64_t n = lua_touint64(L, 1);
#if ( defined (_WIN32) ||  defined (_WIN64) ) && !defined (__MINGW32__) && !defined (__MINGW64__)
	sprintf_s(temp, sizeof(temp), "%"PRIu64, n);
#else
	snprintf(temp, sizeof(temp), "%"PRIu64, n);
#endif
	
	lua_pushstring(L, temp);
	
	return 1;
}

static int uint64_compare(lua_State* L) {
	uint64_t lhs = lua_touint64(L, 1);
	uint64_t rhs = lua_touint64(L, 2);
	lua_pushinteger(L, lhs == rhs ? 0 : (lhs < rhs ? -1 : 1));
	return 1;
}

static int uint64_divide(lua_State* L) {
	uint64_t lhs = lua_touint64(L, 1);
	uint64_t rhs = lua_touint64(L, 2);
	if (rhs == 0) {
        return luaL_error(L, "div by zero");
    }
	lua_pushuint64(L, lhs / rhs);
	return 1;
}

static int uint64_remainder(lua_State* L) {
	uint64_t lhs = lua_touint64(L, 1);
	uint64_t rhs = lua_touint64(L, 2);
	if (rhs == 0) {
        return luaL_error(L, "div by zero");
    }
	lua_pushuint64(L, lhs % rhs);
	return 1;
}

LUALIB_API int uint64_parse(lua_State* L)
{
    const char* str = lua_tostring(L, 1);
    lua_pushuint64(L, strtoull(str, NULL, 0));
    return 1;
}

LUALIB_API int luaopen_i64lib(lua_State* L)
{
#if LUA_VERSION_NUM == 501
    lua_newtable(L);
	
    lua_pushcfunction(L, int64_add);
    lua_setfield(L, -2, "__add");

    lua_pushcfunction(L, int64_sub);
    lua_setfield(L, -2, "__sub");

    lua_pushcfunction(L, int64_mul);
    lua_setfield(L, -2, "__mul");

    lua_pushcfunction(L, int64_div);
    lua_setfield(L, -2, "__div");

    lua_pushcfunction(L, int64_mod);
    lua_setfield(L, -2, "__mod");

    lua_pushcfunction(L, int64_unm);
    lua_setfield(L, -2, "__unm");

    lua_pushcfunction(L, int64_pow);
    lua_setfield(L, -2, "__pow");    

    lua_pushcfunction(L, int64_tostring);
    lua_setfield(L, -2, "__tostring");        

    lua_pushcfunction(L, int64_eq);
    lua_setfield(L, -2, "__eq");  

    lua_pushcfunction(L, int64_lt);
    lua_setfield(L, -2, "__lt"); 

    lua_pushcfunction(L, int64_le);
    lua_setfield(L, -2, "__le");
	
	lua_rawseti(L, LUA_REGISTRYINDEX, INT64_META_REF);
#endif
    lua_newtable(L);
	
	lua_pushcfunction(L, uint64_tostring);
	lua_setfield(L, -2, "tostring");
	
	lua_pushcfunction(L, uint64_compare);
	lua_setfield(L, -2, "compare");
	
	lua_pushcfunction(L, uint64_divide);
	lua_setfield(L, -2, "divide");
	
	lua_pushcfunction(L, uint64_remainder);
	lua_setfield(L, -2, "remainder");
	
	lua_pushcfunction(L, uint64_parse);
	lua_setfield(L, -2, "parse");
	
	lua_setglobal(L, "uint64");
	return 0;
}

