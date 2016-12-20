/*
 *Tencent is pleased to support the open source community by making xLua available.
 *Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 *Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 *http://opensource.org/licenses/MIT
 *Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#define LUA_LIB

#include "lua.h"
#include "lualib.h"
#include "lauxlib.h"

#include <string.h>
#include <stdint.h>
#include "i64lib.h"

/*
** stdcall C function support
*/

static int tag = 0;
static const char *const hooknames[] = {"call", "return", "line", "count", "tail return"};
static int hook_index = -1;

LUA_API void *xlua_tag () 
{
	return &tag;
}

LUA_API int xlua_get_registry_index() {
	return LUA_REGISTRYINDEX;
}

LUA_API int xlua_tocsobj_safe(lua_State *L,int index) {
	int *udata = (int *)lua_touserdata (L,index);
	if (udata != NULL) {
		if (lua_getmetatable(L,index)) {
		    lua_pushlightuserdata(L, &tag);
			lua_rawget(L,-2);
			if (!lua_isnil (L,-1)) {
				lua_pop (L, 2);
				return *udata;
			}
			lua_pop (L, 2);
		}
	}
	return -1;
}

LUA_API int xlua_tocsobj_fast (lua_State *L,int index) {
	int *udata = (int *)lua_touserdata (L,index);

	if(udata!=NULL) 
		return *udata;
	return -1;
}

#if LUA_VERSION_NUM == 501
#undef lua_getglobal
LUA_API int lua_getglobal (lua_State *L, const char *name) {
	lua_getfield(L, LUA_GLOBALSINDEX, name);
	return 0;
}

#undef lua_setglobal
LUA_API void lua_setglobal (lua_State *L, const char *name) {
	lua_setfield(L, LUA_GLOBALSINDEX, name);
}

LUA_API int lua_isinteger (lua_State *L, int idx) {
	return 0;
}

LUA_API uint32_t xlua_objlen (lua_State *L, int idx) {
	return lua_objlen (L, idx);
}

LUA_API uint32_t xlua_touint (lua_State *L, int idx) {
	return (uint32_t)lua_tonumber(L, idx);
}

LUA_API void xlua_pushuint (lua_State *L, uint32_t n) {
	lua_pushnumber(L, n);
}
#endif

#if LUA_VERSION_NUM ==503
LUA_API int lua_setfenv(lua_State *L, int idx)
{
    int type = lua_type(L, idx);
    if(type == LUA_TUSERDATA || type == LUA_TFUNCTION)
    {
        lua_setupvalue(L, idx, 1);
        return 1;
    }
    else
    {
        return 0;
    }
}

LUA_API uint32_t xlua_objlen (lua_State *L, int idx) {
	return (uint32_t)lua_rawlen (L, idx);
}

LUA_API uint32_t xlua_touint (lua_State *L, int idx) {
	return lua_isinteger(L, idx) ? (uint32_t)lua_tointeger(L, idx) : (uint32_t) lua_tonumber(L, idx);
}

LUA_API void xlua_pushuint (lua_State *L, uint32_t n) {
	lua_pushinteger(L, n);
}

#undef lua_insert
LUA_API void lua_insert (lua_State *L, int idx) {
    lua_rotate(L, idx, 1);
}

#undef lua_remove
LUA_API void lua_remove (lua_State *L, int idx) {
	lua_rotate(L, idx, -1);
	lua_pop(L, 1);
}

#undef lua_replace
LUA_API void lua_replace (lua_State *L, int idx) {
	lua_copy(L, -1, idx);
	lua_pop(L, 1);
}

#undef lua_pcall
LUA_API int lua_pcall (lua_State *L, int nargs, int nresults, int errfunc) {
	return lua_pcallk(L, nargs, nresults, errfunc, 0, NULL);
}

#undef lua_tonumber
LUA_API lua_Number lua_tonumber (lua_State *L, int idx) {
	return lua_tonumberx(L, idx, NULL);
}

#endif

#if LUA_VERSION_NUM < 503
#define lua_absindex(L, index) ((index > 0 || index <= LUA_REGISTRYINDEX) ? index : lua_gettop(L) + index + 1)
#endif

LUA_API void xlua_getloaders (lua_State *L) {
	lua_getglobal(L, "package");
#if LUA_VERSION_NUM == 501
    lua_getfield(L, -1, "loaders");
#else
	lua_getfield(L, -1, "searchers");
#endif
	lua_remove(L, -2);
}

LUA_API void xlua_rawgeti (lua_State *L, int idx, int64_t n) {
	lua_rawgeti(L, idx, (lua_Integer)n);
}

LUA_API void xlua_rawseti (lua_State *L, int idx, lua_Integer n) {
	lua_rawseti(L, idx, (lua_Integer)n);
}

LUA_API int xlua_tointeger (lua_State *L, int idx) {
	return (int)lua_tointeger(L, idx);
}

LUA_API void xlua_pushinteger (lua_State *L, int n) {
	lua_pushinteger(L, n);
}

LUA_API void xlua_pushlstring (lua_State *L, const char *s, int len) {
	lua_pushlstring(L, s, len);
}

LUALIB_API int xluaL_loadbuffer (lua_State *L, const char *buff, int size,
                                const char *name) {
	return luaL_loadbuffer(L, buff, size, name);
}

static int c_lua_gettable(lua_State* L) {    
    lua_gettable(L, 1);    
    return 1;
}

LUA_API int xlua_pgettable(lua_State* L, int idx) {
    int top = lua_gettop(L);
    idx = lua_absindex(L, idx);
    lua_pushcfunction(L, c_lua_gettable);
    lua_pushvalue(L, idx);
    lua_pushvalue(L, top);
    lua_remove(L, top);
    return lua_pcall(L, 2, 1, 0);
}

static int c_lua_gettable_bypath(lua_State* L) {
	size_t len = 0;
	const char * pos = NULL;
	const char * path = lua_tolstring(L, 2, &len);
	lua_pushvalue(L, 1);
	do {
		pos = strchr(path, '.');
		if (NULL == pos) {
			lua_pushlstring(L, path, len);
		} else {
			lua_pushlstring(L, path, pos - path);
			len = len - (pos - path + 1);
			path = pos + 1;
		}
		lua_gettable(L, -2);
		if (lua_type(L, -1) != LUA_TTABLE) {
			if (NULL != pos) { // not found in path
				lua_pushnil(L);
			}
			break;
		}
		lua_remove(L, -2);
	} while(pos);
    return 1;
}

LUA_API int xlua_pgettable_bypath(lua_State* L, int idx, const char *path) {
	idx = lua_absindex(L, idx);
	lua_pushcfunction(L, c_lua_gettable_bypath);
	lua_pushvalue(L, idx);
	lua_pushstring(L, path);
	return lua_pcall(L, 2, 1, 0);
}

static int c_lua_settable(lua_State* L) {
    lua_settable(L, 1);
    return 0;
}

LUA_API int xlua_psettable(lua_State* L, int idx) {
    int top = lua_gettop(L);
    idx = lua_absindex(L, idx);
    lua_pushcfunction(L, c_lua_settable);
    lua_pushvalue(L, idx);
    lua_pushvalue(L, top - 1);
    lua_pushvalue(L, top);
    lua_remove(L, top);
    lua_remove(L, top - 1);
    return lua_pcall(L, 3, 0, 0);
}

static int c_lua_settable_bypath(lua_State* L) {
    size_t len = 0;
	const char * pos = NULL;
	const char * path = lua_tolstring(L, 2, &len);
	lua_pushvalue(L, 1);
	do {
		pos = strchr(path, '.');
		if (NULL == pos) { // last
			lua_pushlstring(L, path, len);
			lua_pushvalue(L, 3);
			lua_settable(L, -3);
			lua_pop(L, 1);
			break;
		} else {
			lua_pushlstring(L, path, pos - path);
			len = len - (pos - path + 1);
			path = pos + 1;
		}
		lua_gettable(L, -2);
		if (lua_type(L, -1) != LUA_TTABLE) {
			return luaL_error(L, "can not set value to %s", lua_tostring(L, 2));
		}
		lua_remove(L, -2);
	} while(pos);
    return 0;
}

LUA_API int xlua_psettable_bypath(lua_State* L, int idx, const char *path) {
    int top = lua_gettop(L);
    idx = lua_absindex(L, idx);
    lua_pushcfunction(L, c_lua_settable_bypath);
    lua_pushvalue(L, idx);
    lua_pushstring(L, path);
    lua_pushvalue(L, top);
    lua_remove(L, top);
    return lua_pcall(L, 3, 0, 0);
}

LUA_API int xlua_tryget_cachedud(lua_State *L, int key, int cache_ref) {
	lua_rawgeti(L, LUA_REGISTRYINDEX, cache_ref);
	lua_rawgeti(L, -1, key);
	if (!lua_isnil(L, -1))
	{
		lua_remove(L, -2);
		return 1;
	}
	lua_pop(L, 2);
	return 0;
}

static void cacheud(lua_State *L, int key, int cache_ref) {
	lua_rawgeti(L, LUA_REGISTRYINDEX, cache_ref);
	lua_pushvalue(L, -2);
	lua_rawseti(L, -2, key);
	lua_pop(L, 1);
}


LUA_API void xlua_pushcsobj(lua_State *L, int key, int meta_ref, int need_cache, int cache_ref) {
	int* pointer = (int*)lua_newuserdata(L, sizeof(int));
	*pointer = key;
	
	if (need_cache) cacheud(L, key, cache_ref);

    lua_rawgeti(L, LUA_REGISTRYINDEX, meta_ref);

	lua_setmetatable(L, -2);
}

void print_top(lua_State *L) {
	lua_getglobal(L, "print");
	lua_pushvalue(L, -2);
	lua_call(L, 1, 0);
}

void print_str(lua_State *L, char *str) {
	lua_getglobal(L, "print");
	lua_pushstring(L, str);
	lua_call(L, 1, 0);
}

void print_value(lua_State *L,  char *str, int idx) {
	idx = lua_absindex(L, idx);
	lua_getglobal(L, "print");
	lua_pushstring(L, str);
	lua_pushvalue(L, idx);
	lua_call(L, 2, 0);
}

//upvalue --- [1]: methods, [2]:getters, [3]:csindexer, [4]:base, [5]:indexfuncs, [6]:arrayindexer, [7]:baseindex
//param   --- [1]: obj, [2]: key
LUA_API int obj_indexer(lua_State *L) {	
	if (!lua_isnil(L, lua_upvalueindex(1))) {
		lua_pushvalue(L, 2);
		lua_gettable(L, lua_upvalueindex(1));
		if (!lua_isnil(L, -1)) {//has method
			return 1;
		}
		lua_pop(L, 1);
	}
	
	if (!lua_isnil(L, lua_upvalueindex(2))) {
		lua_pushvalue(L, 2);
		lua_gettable(L, lua_upvalueindex(2));
		if (!lua_isnil(L, -1)) {//has getter
			lua_pushvalue(L, 1);
			lua_call(L, 1, 1);
			return 1;
		}
		lua_pop(L, 1);
	}
	
	
	if (!lua_isnil(L, lua_upvalueindex(6)) && lua_type(L, 2) == LUA_TNUMBER) {
		lua_pushvalue(L, lua_upvalueindex(6));
		lua_pushvalue(L, 1);
		lua_pushvalue(L, 2);
		lua_call(L, 2, 1);
		return 1;
	}
	
	if (!lua_isnil(L, lua_upvalueindex(3))) {
		lua_pushvalue(L, lua_upvalueindex(3));
		lua_pushvalue(L, 1);
		lua_pushvalue(L, 2);
		lua_call(L, 2, 2);
		if (lua_toboolean(L, -2)) {
			return 1;
		}
		lua_pop(L, 2);
	}
	
	if (!lua_isnil(L, lua_upvalueindex(4))) {
		lua_pushvalue(L, lua_upvalueindex(4));
		while(!lua_isnil(L, -1)) {
			lua_pushvalue(L, -1);
			lua_gettable(L, lua_upvalueindex(5));
			if (!lua_isnil(L, -1)) // found
			{
				lua_replace(L, lua_upvalueindex(7)); //baseindex = indexfuncs[base]
				lua_pop(L, 1);
				break;
			}
			lua_pop(L, 1);
			lua_getfield(L, -1, "BaseType");
			lua_remove(L, -2);
		}
		lua_pushnil(L);
		lua_replace(L, lua_upvalueindex(4));//base = nil
	}
	
	if (!lua_isnil(L, lua_upvalueindex(7))) {
		lua_settop(L, 2);
		lua_pushvalue(L, lua_upvalueindex(7));
		lua_insert(L, 1);
		lua_call(L, 2, 1);
		return 1;
	} else {
		return 0;
	}
}

LUA_API int gen_obj_indexer(lua_State *L) {
	lua_pushnil(L);
	lua_pushcclosure(L, obj_indexer, 7);
	return 0;
}

//upvalue --- [1]:setters, [2]:csnewindexer, [3]:base, [4]:newindexfuncs, [5]:arrayindexer, [6]:basenewindex
//param   --- [1]: obj, [2]: key, [3]: value
LUA_API int obj_newindexer(lua_State *L) {
	if (!lua_isnil(L, lua_upvalueindex(1))) {
		lua_pushvalue(L, 2);
		lua_gettable(L, lua_upvalueindex(1));
		if (!lua_isnil(L, -1)) {//has setter
			lua_pushvalue(L, 1);
			lua_pushvalue(L, 3);
			lua_call(L, 2, 0);
			return 0;
		}
		lua_pop(L, 1);
	}
	
	if (!lua_isnil(L, lua_upvalueindex(2))) {
		lua_pushvalue(L, lua_upvalueindex(2));
		lua_pushvalue(L, 1);
		lua_pushvalue(L, 2);
		lua_pushvalue(L, 3);
		lua_call(L, 3, 1);
		if (lua_toboolean(L, -1)) {
			return 0;
		}
	}
	
	if (!lua_isnil(L, lua_upvalueindex(5)) && lua_type(L, 2) == LUA_TNUMBER) {
		lua_pushvalue(L, lua_upvalueindex(5));
		lua_pushvalue(L, 1);
		lua_pushvalue(L, 2);
		lua_pushvalue(L, 3);
		lua_call(L, 3, 0);
		return 0;
	}
	
	if (!lua_isnil(L, lua_upvalueindex(3))) {
		lua_pushvalue(L, lua_upvalueindex(3));
		while(!lua_isnil(L, -1)) {
			lua_pushvalue(L, -1);
			lua_gettable(L, lua_upvalueindex(4));

			if (!lua_isnil(L, -1)) // found
			{
				lua_replace(L, lua_upvalueindex(6)); //basenewindex = newindexfuncs[base]
				lua_pop(L, 1);
				break;
			}
			lua_pop(L, 1);
			lua_getfield(L, -1, "BaseType");
			lua_remove(L, -2);
		}
		lua_pushnil(L);
		lua_replace(L, lua_upvalueindex(3));//base = nil
	}
	
	if (!lua_isnil(L, lua_upvalueindex(6))) {
		lua_settop(L, 3);
		lua_pushvalue(L, lua_upvalueindex(6));
		lua_insert(L, 1);
		lua_call(L, 3, 0);
		return 0;
	} else {
		return luaL_error(L, "cannot set %s, no suck field", lua_tostring(L, 2));
	}
}

LUA_API int gen_obj_newindexer(lua_State *L) {
	lua_pushnil(L);
	lua_pushcclosure(L, obj_newindexer, 6);
	return 0;
}

//upvalue --- [1]:getters, [2]:feilds, [3]:base, [4]:indexfuncs, [5]:baseindex
//param   --- [1]: obj, [2]: key
LUA_API int cls_indexer(lua_State *L) {	
	if (!lua_isnil(L, lua_upvalueindex(1))) {
		lua_pushvalue(L, 2);
		lua_gettable(L, lua_upvalueindex(1));
		if (!lua_isnil(L, -1)) {//has getter
			lua_call(L, 0, 1);
			return 1;
		}
	}
	
	if (!lua_isnil(L, lua_upvalueindex(2))) {
		lua_pushvalue(L, 2);
		lua_rawget(L, lua_upvalueindex(2));
		if (!lua_isnil(L, -1)) {//has feild
			return 1;
		}
		lua_pop(L, 1);
	}
	
	if (!lua_isnil(L, lua_upvalueindex(3))) {
		lua_pushvalue(L, lua_upvalueindex(3));
		while(!lua_isnil(L, -1)) {
			lua_pushvalue(L, -1);
			lua_gettable(L, lua_upvalueindex(4));
			if (!lua_isnil(L, -1)) // found
			{
				lua_replace(L, lua_upvalueindex(5)); //baseindex = indexfuncs[base]
				lua_pop(L, 1);
				break;
			}
			lua_pop(L, 1);
			lua_getfield(L, -1, "BaseType");
			lua_remove(L, -2);
		}
		lua_pushnil(L);
		lua_replace(L, lua_upvalueindex(3));//base = nil
	}
	
	if (!lua_isnil(L, lua_upvalueindex(5))) {
		lua_settop(L, 2);
		lua_pushvalue(L, lua_upvalueindex(5));
		lua_insert(L, 1);
		lua_call(L, 2, 1);
		return 1;
	} else {
		lua_pushnil(L);
		return 1;
	}
}

LUA_API int gen_cls_indexer(lua_State *L) {
	lua_pushnil(L);
	lua_pushcclosure(L, cls_indexer, 5);
	return 0;
}

//upvalue --- [1]:setters, [2]:base, [3]:indexfuncs, [4]:baseindex
//param   --- [1]: obj, [2]: key, [3]: value
LUA_API int cls_newindexer(lua_State *L) {	
	if (!lua_isnil(L, lua_upvalueindex(1))) {
		lua_pushvalue(L, 2);
		lua_gettable(L, lua_upvalueindex(1));
		if (!lua_isnil(L, -1)) {//has setter
		    lua_pushvalue(L, 3);
			lua_call(L, 1, 0);
			return 0;
		}
	}
	
	if (!lua_isnil(L, lua_upvalueindex(2))) {
		lua_pushvalue(L, lua_upvalueindex(2));
		while(!lua_isnil(L, -1)) {
			lua_pushvalue(L, -1);
			lua_gettable(L, lua_upvalueindex(3));
			if (!lua_isnil(L, -1)) // found
			{
				lua_replace(L, lua_upvalueindex(4)); //baseindex = indexfuncs[base]
				lua_pop(L, 1);
				break;
			}
			lua_pop(L, 1);
			lua_getfield(L, -1, "BaseType");
			lua_remove(L, -2);
		}
		lua_pushnil(L);
		lua_replace(L, lua_upvalueindex(2));//base = nil
	}
	
	if (!lua_isnil(L, lua_upvalueindex(4))) {
		lua_settop(L, 3);
		lua_pushvalue(L, lua_upvalueindex(4));
		lua_insert(L, 1);
		lua_call(L, 3, 0);
		return 0;
	} else {
		return luaL_error(L, "no static field %s", lua_tostring(L, 2));
	}
}

LUA_API int gen_cls_newindexer(lua_State *L) {
	lua_pushnil(L);
	lua_pushcclosure(L, cls_newindexer, 4);
	return 0;
}

LUA_API int errorfunc(lua_State *L) {
	lua_getglobal(L, "debug");
	lua_getfield(L, -1, "traceback");
	lua_remove(L, -2);
	lua_pushvalue(L, 1);
	lua_pushnumber(L, 2);
	lua_call(L, 2, 1);
    return 1;
}

LUA_API int get_error_func_ref(lua_State *L) {
	lua_pushcclosure(L, errorfunc, 0);
	return luaL_ref(L, LUA_REGISTRYINDEX);
}

LUA_API int load_error_func(lua_State *L, int ref) {
	lua_rawgeti(L, LUA_REGISTRYINDEX, ref);
	return lua_gettop(L);
}

static void hook(lua_State *L, lua_Debug *ar)
{
	int event;
	
	lua_pushlightuserdata(L, &hook_index);
	lua_rawget(L, LUA_REGISTRYINDEX);

	event = ar->event;
	lua_pushstring(L, hooknames[event]);
  
	lua_getinfo(L, "nS", ar);
	if (*(ar->what) == 'C') {
		lua_pushfstring(L, "[?%s]", ar->name);
	} else {
		lua_pushfstring(L, "%s:%d", ar->short_src, ar->linedefined > 0 ? ar->linedefined : 0);
	}

	lua_call(L, 2, 0);
}

static void call_ret_hook(lua_State *L) {
	lua_Debug ar;
	
	if (lua_gethook(L)) {
		lua_getstack(L, 0, &ar);
		lua_getinfo(L, "n", &ar);
		
		lua_pushlightuserdata(L, &hook_index);
		lua_rawget(L, LUA_REGISTRYINDEX);
		
		if (lua_type(L, -1) != LUA_TFUNCTION){
			lua_pop(L, 1);
			return;
        }
		
		lua_pushliteral(L, "return");
		lua_pushfstring(L, "[?%s]", ar.name);
		lua_pushliteral(L, "[C#]");
		
		lua_sethook(L, 0, 0, 0);
		lua_call(L, 3, 0);
		lua_sethook(L, hook, LUA_MASKCALL | LUA_MASKRET, 0);
	}
}

static int profiler_set_hook(lua_State *L) {
	if (lua_isnoneornil(L, 1)) {
		lua_pushlightuserdata(L, &hook_index);
		lua_pushnil(L);
		lua_rawset(L, LUA_REGISTRYINDEX);
			
		lua_sethook(L, 0, 0, 0);
	} else {
		luaL_checktype(L, 1, LUA_TFUNCTION);
		lua_pushlightuserdata(L, &hook_index);
		lua_pushvalue(L, 1);
		lua_rawset(L, LUA_REGISTRYINDEX);
		lua_sethook(L, hook, LUA_MASKCALL | LUA_MASKRET, 0);
	}
	return 0;
}

static int csharp_function_wrap(lua_State *L) {
	lua_CFunction fn = (lua_CFunction)lua_tocfunction(L, lua_upvalueindex(1));
    int ret = fn(L);    
    
    if (lua_toboolean(L, lua_upvalueindex(2)))
    {
        lua_pushboolean(L, 0);
        lua_replace(L, lua_upvalueindex(2));
        return lua_error(L);
    }
    
	if (lua_gethook(L)) {
		call_ret_hook(L);
	}
	
    return ret;
}

LUA_API void xlua_push_csharp_function(lua_State* L, lua_CFunction fn, int n)
{ 
    lua_pushcfunction(L, fn);
	if (n > 0) {
		lua_insert(L, -1 - n);
	}
	lua_pushboolean(L, 0);
	if (n > 0) {
		lua_insert(L, -1 - n);
	}
    lua_pushcclosure(L, csharp_function_wrap, 2 + (n > 0 ? n : 0));
}

LUALIB_API int xlua_upvalueindex(int n) {
	return lua_upvalueindex(2 + n);
}

LUALIB_API int xlua_csharp_str_error(lua_State* L, const char* msg)
{
    lua_pushboolean(L, 1);
    lua_replace(L, lua_upvalueindex(2));
    lua_pushstring(L, msg);
    return 1;
}

LUALIB_API int xlua_csharp_error(lua_State* L)
{
    lua_pushboolean(L, 1);
    lua_replace(L, lua_upvalueindex(2));
    return 1;
}

typedef struct {
	int fake_id;
    unsigned int len;
	char data[1];
} CSharpStruct;

LUA_API void *xlua_pushstruct(lua_State *L, unsigned int size, int meta_ref) {
	CSharpStruct *css = (CSharpStruct *)lua_newuserdata(L, size + sizeof(int) + sizeof(unsigned int));
	css->fake_id = -1;
	css->len = size;
    lua_rawgeti(L, LUA_REGISTRYINDEX, meta_ref);
	lua_setmetatable(L, -2);
	return css;
}

LUA_API int xlua_gettypeid(lua_State *L, int idx) {
	int type_id = -1;
	if (lua_type(L, idx) == LUA_TUSERDATA) {
		if (lua_getmetatable (L, idx)) {
			lua_rawgeti(L, -1, 1);
			if (lua_type(L, -1) == LUA_TNUMBER) {
				type_id = (int)lua_tointeger(L, -1);
			}
			lua_pop(L, 2);
		}
	}
	return type_id;
}

#define PACK_UNPACK_OF(type) \
LUALIB_API int xlua_pack_##type(void *p, int offset, type field) {\
	CSharpStruct *css = (CSharpStruct *)p;\
	if (css->fake_id != -1 || css->len < offset + sizeof(field)) {\
		return 0;\
	} else {\
		memcpy((&(css->data[0]) + offset), &field, sizeof(field));\
		return 1;\
	}\
}\
\
LUALIB_API int xlua_unpack_##type(void *p, int offset, type *pfield) { \
	CSharpStruct *css = (CSharpStruct *)p;\
	if (css->fake_id != -1 || css->len < offset + sizeof(*pfield)) {\
		return 0;\
	} else {\
		memcpy(pfield, (&(css->data[0]) + offset), sizeof(*pfield));\
		return 1;\
	}\
}\

PACK_UNPACK_OF(int8_t);
PACK_UNPACK_OF(int16_t);
PACK_UNPACK_OF(int32_t);
PACK_UNPACK_OF(int64_t);
PACK_UNPACK_OF(float);
PACK_UNPACK_OF(double);

LUALIB_API int xlua_pack_float2(void *p, int offset, float f1, float f2) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 2) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		pos[0] = f1;
		pos[1] = f2;
		return 1;
	}
}

LUALIB_API int xlua_unpack_float2(void *p, int offset, float *f1, float *f2) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 2) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		*f1 = pos[0];
		*f2 = pos[1];
		return 1;
	}
}

LUALIB_API int xlua_pack_float3(void *p, int offset, float f1, float f2, float f3) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 3) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		pos[0] = f1;
		pos[1] = f2;
		pos[2] = f3;
		return 1;
	}
}

LUALIB_API int xlua_unpack_float3(void *p, int offset, float *f1, float *f2, float *f3) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 3) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		*f1 = pos[0];
		*f2 = pos[1];
		*f3 = pos[2];
		return 1;
	}
}

LUALIB_API int xlua_pack_float4(void *p, int offset, float f1, float f2, float f3, float f4) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 4) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		pos[0] = f1;
		pos[1] = f2;
		pos[2] = f3;
		pos[3] = f4;
		return 1;
	}
}

LUALIB_API int xlua_unpack_float4(void *p, int offset, float *f1, float *f2, float *f3, float *f4) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 4) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		*f1 = pos[0];
		*f2 = pos[1];
		*f3 = pos[2];
		*f4 = pos[3];
		return 1;
	}
}

LUALIB_API int xlua_pack_float5(void *p, int offset, float f1, float f2, float f3, float f4, float f5) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 5) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		pos[0] = f1;
		pos[1] = f2;
		pos[2] = f3;
		pos[3] = f4;
		pos[4] = f5;
		return 1;
	}
}

LUALIB_API int xlua_unpack_float5(void *p, int offset, float *f1, float *f2, float *f3, float *f4, float *f5) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 5) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		*f1 = pos[0];
		*f2 = pos[1];
		*f3 = pos[2];
		*f4 = pos[3];
		*f5 = pos[4];
		return 1;
	}
}

LUALIB_API int xlua_pack_float6(void *p, int offset, float f1, float f2, float f3, float f4, float f5, float f6) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 6) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		pos[0] = f1;
		pos[1] = f2;
		pos[2] = f3;
		pos[3] = f4;
		pos[4] = f5;
		pos[5] = f6;
		return 1;
	}
}

LUALIB_API int xlua_unpack_float6(void *p, int offset, float *f1, float *f2, float *f3, float *f4, float *f5, float *f6) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < offset + sizeof(float) * 6) {
		return 0;
	} else {
		float *pos = (float *)(&(css->data[0]) + offset);
		*f1 = pos[0];
		*f2 = pos[1];
		*f3 = pos[2];
		*f4 = pos[3];
		*f5 = pos[4];
		*f6 = pos[5];
		return 1;
	}
}

LUALIB_API int xlua_pack_decimal(void *p, int offset, const int * decimal) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < sizeof(int) * 4) {
		return 0;
	} else {
		int *pos = (int *)(&(css->data[0]) + offset);
		pos[0] = decimal[0];
		pos[1] = decimal[1];
		pos[2] = decimal[2];
		pos[3] = decimal[3];
		return 1;
	}
}

typedef struct tagDEC {
    uint16_t    wReserved;
    uint8_t     scale;
    uint8_t     sign;
    int         Hi32;
    uint64_t    Lo64;
} DECIMAL;

LUALIB_API int xlua_unpack_decimal(void *p, int offset, uint8_t *scale, uint8_t *sign, int *hi32, uint64_t *lo64) {
	CSharpStruct *css = (CSharpStruct *)p;
	if (css->fake_id != -1 || css->len < sizeof(int) * 4) {
		return 0;
	} else {
		DECIMAL *dec = (DECIMAL *)(&(css->data[0]) + offset);
		*scale = dec->scale;
		*sign = dec->sign;
		*hi32 = dec->Hi32;
		*lo64 = dec->Lo64;
		return 1;
	}
}

LUA_API int xlua_is_eq_str(lua_State *L, int idx, const char* str, int str_len) {
	size_t lmsg;
    const char *msg;
	if (lua_type(L, idx) == LUA_TSTRING) {
        msg = lua_tolstring(L, idx, &lmsg);
		return (lmsg == str_len) && (memcmp(msg, str, lmsg) == 0);
	} else {
		return 0;
	}
}

#define T_INT8   0
#define T_UINT8  1
#define T_INT16  2
#define T_UINT16 3
#define T_INT32  4
#define T_UINT32 5
#define T_INT64  6
#define T_UINT64 7
#define T_FLOAT  8
#define T_DOUBLE 9

static const uint8_t size_of[10] = {
	sizeof(int8_t),
	sizeof(uint8_t),
	sizeof(int16_t),
	sizeof(uint16_t),
	sizeof(int32_t),
	sizeof(uint32_t),
	sizeof(int64_t),
	sizeof(uint64_t),
	sizeof(float),
	sizeof(double),
};

static int is_cs_data(lua_State *L, int idx) {
	if (LUA_TUSERDATA == lua_type(L, idx) && lua_getmetatable(L, idx)) {
		lua_pushlightuserdata(L, &tag);
		lua_rawget(L,-2);
		if (!lua_isnil (L,-1)) {
			lua_pop (L, 2);
			return 1;
		}
		lua_pop (L, 2);
	}
	return 0;
}

static int css_access(lua_State *L) {
	int offset = lua_tointeger(L, lua_upvalueindex(1));
	int type = lua_tointeger(L, lua_upvalueindex(2));
	int top = lua_gettop(L);
	int16_t i16 = 0;
	uint16_t u16 = 0;
	int32_t i32 = 0;
	uint32_t u32 = 0;
	int64_t i64 = 0;
	uint64_t u64 = 0;
	float f = 0;
	double d = 0;
	CSharpStruct *css = (CSharpStruct *)lua_touserdata(L, 1);
	if (!is_cs_data(L, 1) || css->fake_id != -1 || css->len < offset + size_of[type]) {
		luaL_error(L, "invalid c# struct!"); 
	}
	
	if (top >= 2) { // set
		switch(type) {
		case T_INT8:
		    *((int8_t *)(&(css->data[0]) + offset)) = (int8_t)lua_tointeger(L, 2);
			break;
		case T_UINT8:
		    *((uint8_t *)(&(css->data[0]) + offset)) = (uint8_t)lua_tointeger(L, 2);
			break;
		case T_INT16:
		    i16 = (int16_t)lua_tointeger(L, 2);
			memcpy((&(css->data[0]) + offset), &i16, sizeof(i16));
			break;
		case T_UINT16:
		    u16 = (uint16_t)lua_tointeger(L, 2);
			memcpy((&(css->data[0]) + offset), &u16, sizeof(u16));
			break;
		case T_INT32:
		    i32 = (int32_t)lua_tointeger(L, 2);
			memcpy((&(css->data[0]) + offset), &i32, sizeof(i32));
			break;
		case T_UINT32:
		    u32 = xlua_touint(L, 2);
			memcpy((&(css->data[0]) + offset), &u32, sizeof(u32));
			break;
		case T_INT64:
		    i64 = lua_toint64(L, 2);
			memcpy((&(css->data[0]) + offset), &i64, sizeof(i64));
			break;
		case T_UINT64:
		    u64 = lua_touint64(L, 2);
			memcpy((&(css->data[0]) + offset), &u64, sizeof(u64));
			break;
		case T_FLOAT:
		    f = lua_tonumber(L, 2);
			memcpy((&(css->data[0]) + offset), &f, sizeof(f));
			break;
		case T_DOUBLE:
		    d = lua_tonumber(L, 2);
			memcpy((&(css->data[0]) + offset), &d, sizeof(d));
			break;
		default:
		    return luaL_error(L, "unknow tag[%d]", type);
		}
		return 0;
	} else {
		switch(type) {
		case T_INT8:
		    lua_pushinteger(L, *((int8_t *)(&(css->data[0]) + offset)));
			break;
		case T_UINT8:
		    lua_pushinteger(L, *((uint8_t *)(&(css->data[0]) + offset)));
			break;
		case T_INT16:
			memcpy(&i16, (&(css->data[0]) + offset), sizeof(i16));
		    lua_pushinteger(L, i16);
			break;
		case T_UINT16:
			memcpy(&u16, (&(css->data[0]) + offset), sizeof(u16));
		    lua_pushinteger(L, u16);
			break;
		case T_INT32:
			memcpy(&i32, (&(css->data[0]) + offset), sizeof(i32));
		    lua_pushinteger(L, i32);
			break;
		case T_UINT32:
			memcpy(&u32, (&(css->data[0]) + offset), sizeof(u32));
		    xlua_pushuint(L, u32);
			break;
		case T_INT64:
			memcpy(&i64, (&(css->data[0]) + offset), sizeof(i64));
		    lua_pushint64(L, i64);
			break;
		case T_UINT64:
			memcpy(&u64, (&(css->data[0]) + offset), sizeof(u64));
		    lua_pushint64(L, u64);
			break;
		case T_FLOAT:
			memcpy(&f, (&(css->data[0]) + offset), sizeof(f));
		    lua_pushnumber(L, f);
			break;
		case T_DOUBLE:
			memcpy(&d, (&(css->data[0]) + offset), sizeof(d));
		    lua_pushnumber(L, d);
			break;
		default:
		    return luaL_error(L, "unknow tag[%d]", type);
		}
		
		return 1;
	}
}

LUA_API int gen_css_access(lua_State *L) {
	int offset = lua_tointeger(L, 1);
	int type = lua_tointeger(L, 2);
	if (offset < 0) {
		return luaL_error(L, "offset must larger than 0");
	}
	if (type < T_INT8 || type > T_DOUBLE) {
		return luaL_error(L, "unknow tag[%d]", type);
	}
	lua_pushcclosure(L, css_access, 2);
	return 1;
}

LUA_API int css_clone(lua_State *L) {
	CSharpStruct *from = (CSharpStruct *)lua_touserdata(L, 1);
	if (!is_cs_data(L, 1) || from->fake_id != -1) {
		return luaL_error(L, "invalid c# struct!");
	}
	
	CSharpStruct *to = (CSharpStruct *)lua_newuserdata(L, from->len + sizeof(int) + sizeof(unsigned int));
	to->fake_id = -1;
	to->len = from->len;
	memcpy(&(to->data[0]), &(from->data[0]), from->len);
    lua_getmetatable(L, 1);
	lua_setmetatable(L, -2);
	return 1;
}

static const luaL_Reg xlualib[] = {
	{"sethook", profiler_set_hook},
	{"genaccessor", gen_css_access},
	{"structclone", css_clone},
	{NULL, NULL}
};

LUA_API void luaopen_xlua(lua_State *L) {
	luaL_openlibs(L);
	
#if LUA_VERSION_NUM == 503
	luaL_newlib(L, xlualib);
	lua_setglobal(L, "xlua");
#else
	luaL_register(L, "xlua", xlualib);
    lua_pop(L, 1);
#endif
}

