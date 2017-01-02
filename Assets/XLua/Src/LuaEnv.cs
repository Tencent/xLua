/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif


namespace XLua
{
    using System;
    using System.Collections.Generic;

    public class LuaEnv : IDisposable
    {
        internal RealStatePtr L;

        private LuaTable _G;

        internal ObjectTranslator translator;

        internal int errorFuncRef = -1;

#if THREAD_SAFT || HOTFIX_ENABLE
        internal object luaEnvLock = new object();
#endif

        public LuaEnv()
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock(luaEnvLock)
            {
#endif
                LuaIndexes.LUA_REGISTRYINDEX = LuaAPI.xlua_get_registry_index(); ;
                // Create State
                L = LuaAPI.luaL_newstate();

                //Init Base Libs
                LuaAPI.luaopen_xlua(L);
                LuaAPI.luaopen_i64lib(L);
                LuaAPI.luaopen_perflib(L);

                translator = new ObjectTranslator(this, L);
                translator.createFunctionMetatable(L);
                translator.OpenLib(L);
                ObjectTranslatorPool.Instance.Add(L, translator);

                LuaAPI.lua_atpanic(L, StaticLuaCallbacks.Panic);

                LuaAPI.lua_pushstdcallcfunction(L, StaticLuaCallbacks.Print);
                LuaAPI.lua_setglobal(L, "print");

                //template engine lib register
                TemplateEngine.LuaTemplate.OpenLib(L);

                AddSearcher(StaticLuaCallbacks.LoadBuiltinLib, 2); // just after the preload searcher
                AddSearcher(StaticLuaCallbacks.LoadFromCustomLoaders, 3);
                AddSearcher(StaticLuaCallbacks.LoadFromResource, 4);
                AddSearcher(StaticLuaCallbacks.LoadFromStreamingAssetsPath, -1);
                DoString(init_xlua, "Init");
                init_xlua = null;

                AddBuildin("socket.core", StaticLuaCallbacks.LoadSocketCore);
                AddBuildin("socket", StaticLuaCallbacks.LoadSocketCore);

                LuaAPI.lua_newtable(L); //metatable of indexs and newindexs functions
                LuaAPI.xlua_pushasciistring(L, "__index");
                LuaAPI.lua_pushstdcallcfunction(L, StaticLuaCallbacks.MetaFuncIndex);
                LuaAPI.lua_rawset(L, -3);

                LuaAPI.xlua_pushasciistring(L, Utils.LuaIndexsFieldName);
                LuaAPI.lua_newtable(L);
                LuaAPI.lua_pushvalue(L, -3);
                LuaAPI.lua_setmetatable(L, -2);
                LuaAPI.lua_rawset(L, LuaIndexes.LUA_REGISTRYINDEX);

                LuaAPI.xlua_pushasciistring(L, Utils.LuaNewIndexsFieldName);
                LuaAPI.lua_newtable(L);
                LuaAPI.lua_pushvalue(L, -3);
                LuaAPI.lua_setmetatable(L, -2);
                LuaAPI.lua_rawset(L, LuaIndexes.LUA_REGISTRYINDEX);

                LuaAPI.xlua_pushasciistring(L, Utils.LuaClassIndexsFieldName);
                LuaAPI.lua_newtable(L);
                LuaAPI.lua_pushvalue(L, -3);
                LuaAPI.lua_setmetatable(L, -2);
                LuaAPI.lua_rawset(L, LuaIndexes.LUA_REGISTRYINDEX);

                LuaAPI.xlua_pushasciistring(L, Utils.LuaClassNewIndexsFieldName);
                LuaAPI.lua_newtable(L);
                LuaAPI.lua_pushvalue(L, -3);
                LuaAPI.lua_setmetatable(L, -2);
                LuaAPI.lua_rawset(L, LuaIndexes.LUA_REGISTRYINDEX);

                LuaAPI.lua_pop(L, 1); // pop metatable of indexs and newindexs functions

                LuaAPI.xlua_pushasciistring(L, "xlua_main_thread");
                LuaAPI.lua_pushthread(L);
                LuaAPI.lua_rawset(L, LuaIndexes.LUA_REGISTRYINDEX);

                translator.Alias(typeof(Type), "System.MonoType");

                LuaAPI.lua_getglobal(L, "_G");
                translator.Get(L, -1, out _G);
                LuaAPI.lua_pop(L, 1);

                errorFuncRef = LuaAPI.get_error_func_ref(L);

                if (initers != null)
                {
                    for (int i = 0; i < initers.Count; i++)
                    {
                        initers[i](this, translator);
                    }
                }

                translator.CreateArrayMetatable(L);
                translator.CreateDelegateMetatable(L);
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        private static List<Action<LuaEnv, ObjectTranslator>> initers = null;

        public static void AddIniter(Action<LuaEnv, ObjectTranslator> initer)
        {
            if (initers == null)
            {
                initers = new List<Action<LuaEnv, ObjectTranslator>>();
            }
            initers.Add(initer);
        }

        public LuaTable Global
        {
            get
            {
                return _G;
            }
        }

        public T LoadString<T>(string chunk, string chunkName = "chunk", LuaTable env = null)
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                if (typeof(T) != typeof(LuaFunction) && !typeof(T).IsSubclassOf(typeof(Delegate)))
                {
                    throw new InvalidOperationException(typeof(T).Name + " is not a delegate type nor LuaFunction");
                }

                int oldTop = LuaAPI.lua_gettop(L);

                if (LuaAPI.luaL_loadbuffer(L, chunk, chunkName) != 0)
                    ThrowExceptionFromError(oldTop);

                if (env != null)
                {
                    env.push(L);
                    LuaAPI.lua_setfenv(L, -2);
                }

                T result = (T)translator.GetObject(L, -1, typeof(T));
                LuaAPI.lua_settop(L, oldTop);

                return result;
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        public LuaFunction LoadString(string chunk, string chunkName = "chunk", LuaTable env = null)
        {
            return LoadString<LuaFunction>(chunk, chunkName, env);
        }

        public object[] DoString(string chunk, string chunkName = "chunk", LuaTable env = null)
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                int oldTop = LuaAPI.lua_gettop(L);
                int errFunc = LuaAPI.load_error_func(L, errorFuncRef);
                if (LuaAPI.luaL_loadbuffer(L, chunk, chunkName) == 0)
                {
                    if (env != null)
                    {
                        env.push(L);
                        LuaAPI.lua_setfenv(L, -2);
                    }

                    if (LuaAPI.lua_pcall(L, 0, -1, errFunc) == 0)
                    {
                        LuaAPI.lua_remove(L, errFunc);
                        return translator.popValues(L, oldTop);
                    }
                    else
                        ThrowExceptionFromError(oldTop);
                }
                else
                    ThrowExceptionFromError(oldTop);

                return null;
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        private void AddSearcher(LuaCSFunction searcher, int index)
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                //insert the loader
                LuaAPI.xlua_getloaders(L);
                if (!LuaAPI.lua_istable(L, -1))
                {
                    throw new Exception("Can not set searcher!");
                }
                uint len = LuaAPI.xlua_objlen(L, -1);
                index = index < 0 ? (int)(len + index + 2) : index;
                for (int e = (int)len + 1; e > index; e--)
                {
                    LuaAPI.xlua_rawgeti(L, -1, e - 1);
                    LuaAPI.xlua_rawseti(L, -2, e);
                }
                LuaAPI.lua_pushstdcallcfunction(L, searcher);
                LuaAPI.xlua_rawseti(L, -2, index);
                LuaAPI.lua_pop(L, 1);
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        public void Alias(Type type, string alias)
        {
            translator.Alias(type, alias);
        }

        int last_check_point = 0;

        int max_check_per_tick = 20;

        static bool ObjectValidCheck(object obj)
        {
            return (!(obj is UnityEngine.Object)) ||  ((obj as UnityEngine.Object) != null);
        }

        Func<object, bool> object_valid_checker = new Func<object, bool>(ObjectValidCheck);

        public void Tick()
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                lock (refQueue)
                {
                    while (refQueue.Count > 0)
                    {
                        GCAction gca = refQueue.Dequeue();
                        translator.ReleaseLuaBase(L, gca.Reference, gca.IsDelegate);
                    }
                }
                last_check_point = translator.objects.Check(last_check_point, max_check_per_tick, object_valid_checker, translator.reverseMap);
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        //兼容API
        public void GC()
        {
            Tick();
        }

        public LuaTable NewTable()
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                int oldTop = LuaAPI.lua_gettop(L);

                LuaAPI.lua_newtable(L);
                LuaTable returnVal = (LuaTable)translator.GetObject(L, -1, typeof(LuaTable));

                LuaAPI.lua_settop(L, oldTop);
                return returnVal;
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
        }

        public virtual void Dispose(bool dispose)
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                if (disposed) return;
                Tick();

                LuaAPI.lua_close(L);

                ObjectTranslatorPool.Instance.Remove(L);
                if (translator != null)
                {
                    translator = null;
                }

                L = IntPtr.Zero;

                disposed = true;
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        public void ThrowExceptionFromError(int oldTop)
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                object err = translator.GetObject(L, -1);
                LuaAPI.lua_settop(L, oldTop);

                // A pre-wrapped exception - just rethrow it (stack trace of InnerException will be preserved)
                Exception ex = err as Exception;
                if (ex != null) throw ex;

                // A non-wrapped Lua error (best interpreted as a string) - wrap it and throw it
                if (err == null) err = "Unknown Lua Error";
                throw new LuaException(err.ToString());
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        internal struct GCAction
        {
            public int Reference;
            public bool IsDelegate;
        }

        Queue<GCAction> refQueue = new Queue<GCAction>();

        internal void equeueGCAction(GCAction action)
        {
            lock (refQueue)
            {
                refQueue.Enqueue(action);
            }
        }

        private string init_xlua = @" 
            local metatable = {}
            local rawget = rawget
            local setmetatable = setmetatable
            local import_type = xlua.import_type
            local load_assembly = xlua.load_assembly

            function metatable:__index(key) 
                local fqn = rawget(self,'.fqn')
                fqn = ((fqn and fqn .. '.') or '') .. key

                local obj = import_type(fqn)

                if obj == nil then
                    -- It might be an assembly, so we load it too.
                    obj = { ['.fqn'] = fqn }
                    setmetatable(obj, metatable)
                elseif obj == true then
                    return rawget(self, key)
                end

                -- Cache this lookup
                rawset(self, key, obj)
                return obj
            end

            -- A non-type has been called; e.g. foo = System.Foo()
            function metatable:__call(...)
                error('No such type: ' .. rawget(self,'.fqn'), 2)
            end

            CS = CS or {}
            setmetatable(CS, metatable)

            typeof = function(t) return t.UnderlyingSystemType end
            cast = xlua.cast
            if not setfenv or not getfenv then
                local function getfunction(level)
                    local info = debug.getinfo(level + 1, 'f')
                    return info and info.func
                end

                function setfenv(fn, env)
                  if type(fn) == 'number' then fn = getfunction(fn + 1) end
                  local i = 1
                  while true do
                    local name = debug.getupvalue(fn, i)
                    if name == '_ENV' then
                      debug.upvaluejoin(fn, i, (function()
                        return env
                      end), 1)
                      break
                    elseif not name then
                      break
                    end

                    i = i + 1
                  end

                  return fn
                end

                function getfenv(fn)
                  if type(fn) == 'number' then fn = getfunction(fn + 1) end
                  local i = 1
                  while true do
                    local name, val = debug.getupvalue(fn, i)
                    if name == '_ENV' then
                      return val
                    elseif not name then
                      break
                    end
                    i = i + 1
                  end
                end
            end

            xlua.hotfix = function(cs, field, func)
                local tbl = (type(field) == 'table') and field or {[field] = func}
                for k, v in pairs(tbl) do
                    local cflag = ''
                    if k == '.ctor' then
                        cflag = '_c'
                        k = 'ctor'
                    end
                    xlua.access(cs, cflag .. '__Hitfix0_'..k, v) -- at least one
                    pcall(function()
                        for i = 1, 99 do
                            xlua.access(cs, '__Hitfix'..i..'_'..k, v)
                        end
                    end)
                end
            end
            ";

        public delegate byte[] CustomLoader(ref string filepath);

        internal List<CustomLoader> customLoaders = new List<CustomLoader>();

        //loader : CustomLoader， filepath参数：（ref类型）输入是require的参数，如果需要支持调试，需要输出真实路径。
        //                        返回值：如果返回null，代表加载该源下无合适的文件，否则返回UTF8编码的byte[]
        public void AddLoader(CustomLoader loader)
        {
            customLoaders.Add(loader);
        }

        internal Dictionary<string, LuaCSFunction> buildin_initer = new Dictionary<string, LuaCSFunction>();

        public void AddBuildin(string name, LuaCSFunction initer)
        {
            if (!initer.Method.IsStatic || !Attribute.IsDefined(initer.Method, typeof(MonoPInvokeCallbackAttribute)))
            {
                throw new Exception("initer must be static and has MonoPInvokeCallback Attribute!");
            }
            buildin_initer.Add(name, initer);
        }

        //The garbage-collector pause controls how long the collector waits before starting a new cycle. 
        //Larger values make the collector less aggressive. Values smaller than 100 mean the collector 
        //will not wait to start a new cycle. A value of 200 means that the collector waits for the total 
        //memory in use to double before starting a new cycle.
        public int GcPause
        {
            get
            {
#if THREAD_SAFT || HOTFIX_ENABLE
                lock (luaEnvLock)
                {
#endif
                    int val = LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETPAUSE, 200);
                    LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETPAUSE, val);
                    return val;
#if THREAD_SAFT || HOTFIX_ENABLE
                }
#endif
            }
            set
            {
#if THREAD_SAFT || HOTFIX_ENABLE
                lock (luaEnvLock)
                {
#endif
                    LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETPAUSE, value);
#if THREAD_SAFT || HOTFIX_ENABLE
                }
#endif
            }
        }

        //The step multiplier controls the relative speed of the collector relative to memory allocation. 
        //Larger values make the collector more aggressive but also increase the size of each incremental 
        //step. Values smaller than 100 make the collector too slow and can result in the collector never 
        //finishing a cycle. The default, 200, means that the collector runs at "twice" the speed of memory 
        //allocation.
        public int GcStepmul
        {
            get
            {
#if THREAD_SAFT || HOTFIX_ENABLE
                lock (luaEnvLock)
                {
#endif
                    int val = LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETSTEPMUL, 200);
                    LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETSTEPMUL, val);
                    return val;
#if THREAD_SAFT || HOTFIX_ENABLE
                }
#endif
            }
            set
            {
#if THREAD_SAFT || HOTFIX_ENABLE
                lock (luaEnvLock)
                {
#endif
                    LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSETSTEPMUL, value);
#if THREAD_SAFT || HOTFIX_ENABLE
                }
#endif
            }
        }

        public void FullGc()
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCCOLLECT, 0);
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        public void StopGc()
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSTOP, 0);
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        public void RestartGc()
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCRESTART, 0);
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        public bool GcStep(int data)
        {
#if THREAD_SAFT || HOTFIX_ENABLE
            lock (luaEnvLock)
            {
#endif
                return LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCSTEP, data) != 0;
#if THREAD_SAFT || HOTFIX_ENABLE
            }
#endif
        }

        public int Memroy
        {
            get
            {
#if THREAD_SAFT || HOTFIX_ENABLE
                lock (luaEnvLock)
                {
#endif
                    return LuaAPI.lua_gc(L, LuaGCOptions.LUA_GCCOUNT, 0);
#if THREAD_SAFT || HOTFIX_ENABLE
                }
#endif
            }
        }
    }
}
