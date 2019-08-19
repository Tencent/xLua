/*
 *Tencent is pleased to support the open source community by making xLua available.
 *Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 *Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 *http://opensource.org/licenses/MIT
 *Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#define LUA_LIB

#include "lua.h"
#include "lauxlib.h"
#include "lualib.h"
#include <stdio.h>
#include <string.h>

#define ROOT_TABLE 1
#define MARKED_TABLE 2

#define RT_GLOBAL 1
#define RT_REGISTRY 2
#define RT_UPVALUE 3
#define RT_LOCAL 4

#if defined(_MSC_VER) && _MSC_VER < 1900

#define snprintf c99_snprintf
#define vsnprintf c99_vsnprintf

__inline int c99_vsnprintf(char *outBuf, size_t size, const char *format, va_list ap)
{
    int count = -1;

    if (size != 0)
        count = _vsnprintf_s(outBuf, size, _TRUNCATE, format, ap);
    if (count == -1)
        count = _vscprintf(format, ap);

    return count;
}

__inline int c99_snprintf(char *outBuf, size_t size, const char *format, ...)
{
    int count;
    va_list ap;

    va_start(ap, format);
    count = c99_vsnprintf(outBuf, size, format, ap);
    va_end(ap);

    return count;
}

#endif

#if LUA_VERSION_NUM == 501
static void lua_rawsetp(lua_State *L, int idx, const void *p) {
	if (idx < 0) {
		idx += lua_gettop(L) + 1;
	}
	lua_pushlightuserdata(L, (void *)p);
	lua_insert(L, -2);
	lua_rawset(L, idx);
}

static void lua_rawgetp(lua_State *L, int idx, const void *p) {
	if (idx < 0) {
		idx += lua_gettop(L) + 1;
	}
	lua_pushlightuserdata(L, (void *)p);
	lua_rawget(L, idx);
}

#endif

#if LUA_VERSION_NUM == 503
#define lua_objlen(L,i)		lua_rawlen(L, (i))
#endif

static void make_root(lua_State *L, const void *p, const char *name, int type, const char *used_in, int need_stat) {
	lua_rawgetp(L, ROOT_TABLE, p);
	if (lua_isnil(L, -1)) {
		lua_pop(L, 1);
		lua_newtable(L); // -- root
		lua_newtable(L); // root.used_in
		if (used_in != NULL) {
			lua_pushboolean(L, 1);
			lua_setfield(L, -2, used_in);
		}
		lua_setfield(L, -2, "used_in");
		if (need_stat) {
		    lua_pushstring(L, name);
		    lua_setfield(L, -2, "name");
		    lua_pushnumber(L, type);
		    lua_setfield(L, -2, "type");
		}
		
		lua_pushvalue(L, -1);
		lua_rawsetp(L, ROOT_TABLE, p); //ROOT_TABLE[p] = root
	} else {
		if (used_in != NULL) {
			lua_getfield(L, -1, "used_in");
			lua_pushboolean(L, 1);
			lua_setfield(L, -2, used_in);
			lua_pop(L, 1);
		}
	}
}

//static void print_top(lua_State *L) {
//	lua_getglobal(L, "print");
//	lua_pushvalue(L, -2);
//	lua_call(L, 1, 0);
//}
//
//static void print_str(lua_State *L, const char *str) {
//	lua_getglobal(L, "print");
//	lua_pushstring(L, str);
//	lua_call(L, 1, 0);
//}

static int is_marked(lua_State *L, const void *p) {
	lua_rawgetp(L, MARKED_TABLE, p);
	if (lua_isnil(L, -1)) {
		lua_pop(L, 1);
		return 0;
	} else {
		lua_pop(L, 1);
		return 1;
	}
}

static void marked(lua_State *L, const void *p, int len) {
	lua_pushnumber(L, len);
    lua_rawsetp(L, MARKED_TABLE, p);
}

static void mark_object(lua_State *L, lua_State *dL);

static void mark_table(lua_State *L, lua_State *dL) {
	const void *p = lua_topointer(L, -1);
	int len = 0;
	
	if (!is_marked(dL, p)) {
		marked(dL, p, 0);

		lua_pushnil(L);
		while (lua_next(L, -2) != 0) {
			++len;
			mark_object(L, dL);
			lua_pop(L, 1);
			mark_object(L, dL);
		}
		
		marked(dL, p, len);
	}
}

static void mark_function(lua_State *L, lua_State *dL) {
	const void *p = lua_topointer(L, -1);
	int i;
	lua_Debug ar;
	char used_in[128];
	const char *name;
	
	
	if (!is_marked(dL, p)) {
		marked(dL, p, 0); //已经在table里头算了
		
		lua_pushvalue(L, -1);
		lua_getinfo(L, ">S", &ar);
		snprintf(used_in, sizeof(used_in) - 1, "%s:%d~%d", ar.short_src, ar.linedefined, ar.lastlinedefined);
		used_in[sizeof(used_in) - 1] = 0;
		
		for (i=1;;i++) {
			name = lua_getupvalue(L,-1,i);
			if (name == NULL)
				break;
			p = lua_topointer(L, -1);
			
			if (*name != '\0' && LUA_TTABLE == lua_type(L, -1)) {
				make_root(dL, p, name, RT_UPVALUE, used_in, 1);
				lua_insert(dL, MARKED_TABLE);
				mark_object(L, dL);
				lua_remove(dL, MARKED_TABLE);
			} else if (LUA_TFUNCTION == lua_type(L, -1)) {
				mark_function(L, dL);
			}
			lua_pop(L, 1);
		}
	}
}

static void mark_object(lua_State *L, lua_State *dL) {
	switch (lua_type(L, -1)) {
	case LUA_TTABLE:
		mark_table(L, dL);
		break;
	case LUA_TFUNCTION:
		mark_function(L, dL);
		break;
	default:
		break;
	}
}

static void make_report(lua_State* L, lua_State* dL) {
	int size = 0;
	int i = 0;
	luaL_Buffer b;
	
	lua_newtable(L);
	
	lua_pushnil(dL);
	while (lua_next(dL, ROOT_TABLE) != 0) {
		lua_getfield(dL, -1, "name");
		if (lua_isnil(dL, -1)) {
			lua_pop(dL, 2);
			continue;
		} else {
			lua_pop(dL, 1);
		}
		
		lua_newtable(L);
		size = 0;
		
		lua_pushnil(dL);
		while (lua_next(dL, -2) != 0) {
			if (LUA_TLIGHTUSERDATA == lua_type(dL, -2)) { 
				size += (int)lua_tointeger(dL, -1);
			} 
			
			lua_pop(dL, 1);
		}
		lua_pushnumber(L, size);
		lua_setfield(L, -2, "size");
		
		lua_pushfstring(L, "%p", lua_touserdata(dL, -2));
		lua_setfield(L, -2, "pointer");
		
		lua_getfield(dL, -1, "name");
		lua_pushstring(L, lua_tostring(dL, -1));
		lua_pop(dL, 1);
		lua_setfield(L, -2, "name");
		
		lua_getfield(dL, -1, "type");
		lua_pushnumber(L, lua_tonumber(dL, -1));
		lua_pop(dL, 1);
		lua_setfield(L, -2, "type");
		
		lua_getfield(dL, -1, "used_in");
		luaL_buffinit(L, &b);
		lua_pushnil(dL);
		while (lua_next(dL, -2) != 0) {
			lua_pop(dL, 1);
			luaL_addstring(&b, lua_tostring(dL, -1));
			luaL_addchar(&b, ';');
		}
		luaL_pushresult(&b);
		lua_pop(dL, 1);
		lua_setfield(L, -2, "used_in");
		
		++i;
		lua_rawseti(L, -2, i);
		
		lua_pop(dL, 1);
	}
}

static int mark_root_table(lua_State* L, lua_State* dL, int type) {
	int len = 0;
	
	lua_pushnil(L);
	while (lua_next(L, -2) != 0) {
		++len;
		if (LUA_TTABLE == lua_type(L, -1)) {
			lua_pushvalue(L, -2);
			
			make_root(dL, lua_topointer(L, -2), lua_tostring(L, -1), type, NULL, 1);
			lua_pop(L, 1);
			mark_table(L, dL);
			lua_pop(dL, 1);
		} else {
		    make_root(dL, lua_topointer(L, -1), "FUNCTION", type, NULL, 0);
			mark_object(L, dL);
			lua_pop(dL, 1);
		}
		lua_pop(L, 1);
		
		make_root(dL, lua_topointer(L, -1), "[KEY]", type, NULL, LUA_TTABLE == lua_type(L, -1));
		mark_object(L, dL);
		lua_pop(dL, 1);
	}
	
	return len;
}

static int snapshot(lua_State* L) {
	lua_State *dL = luaL_newstate();
	int len;
	const void * p;
	lua_newtable(dL);
	
#if LUA_VERSION_NUM == 503
	lua_rawgeti(L, LUA_REGISTRYINDEX, LUA_RIDX_GLOBALS);
#else
	lua_pushvalue(L, LUA_GLOBALSINDEX);
#endif
	mark_root_table(L, dL, RT_GLOBAL);
	lua_pop(L, 1);
	
	lua_pushvalue(L, LUA_REGISTRYINDEX);
	p = lua_topointer(L, -1);
	len = mark_root_table(L, dL, RT_REGISTRY);
	lua_pop(L, 1);
	
	make_report(L, dL);
	
	lua_newtable(L);
	lua_pushstring(L, "[REGISTRY Level 1]");
	lua_setfield(L, -2, "name");
	lua_pushnumber(L, RT_REGISTRY);
	lua_setfield(L, -2, "type");
	lua_pushnumber(L, len);
	lua_setfield(L, -2, "size");
	lua_pushfstring(L, "%p", p);
	lua_setfield(L, -2, "pointer");
	lua_pushstring(L, "");
	lua_setfield(L, -2, "used_in");
	lua_rawseti(L, -2, lua_objlen(L, -2) + 1);
	
	lua_close(dL);
	
	return 1;
}

static const luaL_Reg preflib[] = {
	{"snapshot", snapshot},
	{NULL, NULL}
};

LUALIB_API int luaopen_perflib(lua_State* L)
{
#if LUA_VERSION_NUM == 503
	luaL_newlib(L, preflib);
	lua_setglobal(L, "perf");
#else
	luaL_register(L, "perf", preflib);
    lua_pop(L, 1);
#endif
	return 0;
}
