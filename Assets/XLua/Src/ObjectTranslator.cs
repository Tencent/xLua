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
    using System.Collections;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    class ReferenceEqualsComparer : IEqualityComparer<object>
    {
        public new bool Equals(object o1, object o2)
        {
            return object.ReferenceEquals(o1, o2);
        }
        public int GetHashCode(object obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }

#pragma warning disable 414
    public class MonoPInvokeCallbackAttribute : System.Attribute
    {
        private Type type;
        public MonoPInvokeCallbackAttribute(Type t) { type = t; }
    }
#pragma warning restore 414

    public enum LuaTypes
    {
        LUA_TNONE = -1,
        LUA_TNIL = 0,
        LUA_TNUMBER = 3,
        LUA_TSTRING = 4,
        LUA_TBOOLEAN = 1,
        LUA_TTABLE = 5,
        LUA_TFUNCTION = 6,
        LUA_TUSERDATA = 7,
        LUA_TTHREAD = 8,
        LUA_TLIGHTUSERDATA = 2
    }

    public enum LuaGCOptions
    {
        LUA_GCSTOP = 0,
        LUA_GCRESTART = 1,
        LUA_GCCOLLECT = 2,
        LUA_GCCOUNT = 3,
        LUA_GCCOUNTB = 4,
        LUA_GCSTEP = 5,
        LUA_GCSETPAUSE = 6,
        LUA_GCSETSTEPMUL = 7,
    }

    public enum LuaThreadStatus
    {
        LUA_RESUME_ERROR = -1,
        LUA_OK = 0,
        LUA_YIELD = 1,
        LUA_ERRRUN = 2,
        LUA_ERRSYNTAX = 3,
        LUA_ERRMEM = 4,
        LUA_ERRERR = 5,
    }

    sealed class LuaIndexes
    {
        public static int LUA_REGISTRYINDEX = -10000;
    }

    public partial class ObjectTranslator
	{
        internal MethodWrapsCache methodWrapsCache;
        internal ObjectCheckers objectCheckers;
        internal ObjectCasters objectCasters;

        internal readonly ObjectPool objects = new ObjectPool();
        //public readonly Dictionary<int, object> objects = new Dictionary<int, object>();
        // object to object #
        //Fix bug by john, struct equals is by value, blow will print
        //local v1=Vector3(1,1,1) 
        //local v2=Vector3(1,1,1) 
        //v1.x = 100 
        //print(v1.x, v2.x) 
        internal readonly Dictionary<object, int> reverseMap = new Dictionary<object, int>(new ReferenceEqualsComparer());
		internal LuaEnv luaEnv;
		internal StaticLuaCallbacks metaFunctions;
		internal List<Assembly> assemblies;
		private LuaCSFunction importTypeFunction,loadAssemblyFunction, castFunction;
        //延迟加载
        private readonly Dictionary<Type, Action<RealStatePtr>> delayWrap = new Dictionary<Type, Action<RealStatePtr>>();

        private readonly Dictionary<Type, Func<int, LuaEnv, LuaBase>> interfaceBridgeCreators = new Dictionary<Type, Func<int, LuaEnv, LuaBase>>();

        //无法访问的类，比如声明成internal，可以用其接口、基类的生成代码来访问
        private readonly Dictionary<Type, Type> aliasCfg = new Dictionary<Type, Type>();

        public void DelayWrapLoader(Type type, Action<RealStatePtr> loader)
        {
            delayWrap[type] = loader;
        }

        public void AddInterfaceBridgeCreator(Type type, Func<int, LuaEnv, LuaBase> creator)
        {
            interfaceBridgeCreators.Add(type, creator);
        }

        Dictionary<Type, bool> loaded_types = new Dictionary<Type, bool>();
        public bool TryDelayWrapLoader(RealStatePtr L, Type type)
        {
            if (loaded_types.ContainsKey(type)) return true;
            loaded_types.Add(type, true);

            LuaAPI.luaL_newmetatable(L, type.FullName); //先建一个metatable，因为加载过程可能会需要用到
            LuaAPI.lua_pop(L, 1);

            Action<RealStatePtr> loader;
            int top = LuaAPI.lua_gettop(L);
            if (delayWrap.TryGetValue(type, out loader))
            {
                delayWrap.Remove(type);
                loader(L);
            }
            else
            {
                Utils.ReflectionWrap(L, type);
#if NOT_GEN_WARNING
                UnityEngine.Debug.LogWarning(string.Format("{0} not gen, using reflection instead", type));
#endif
            }
            if (top != LuaAPI.lua_gettop(L))
            {
                throw new Exception("top change, before:" + top + ", after:" + LuaAPI.lua_gettop(L));
            }

            foreach (var nested_type in type.GetNestedTypes())
            {
                if ((!nested_type.IsAbstract && typeof(Delegate).IsAssignableFrom(nested_type))
                    || nested_type.IsGenericTypeDefinition)
                {
                    continue;
                }
                TryDelayWrapLoader(L, nested_type);
            }
            
            return true;
        }
        
        public void Alias(Type type, string alias)
        {
            Type alias_type = FindType(alias);
            if (alias_type == null)
            {
                throw new ArgumentException("Can not find " + alias);
            }
            aliasCfg[alias_type] = type;
        }

        public int cacheRef;

        public ObjectTranslator(LuaEnv luaenv,RealStatePtr L)
		{
            assemblies = new List<Assembly>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                assemblies.Add(assembly);
            }

			this.luaEnv=luaenv;
            objectCasters = new ObjectCasters(this);
            objectCheckers = new ObjectCheckers(this);
            methodWrapsCache = new MethodWrapsCache(this, objectCheckers, objectCasters);
			metaFunctions=new StaticLuaCallbacks();

            importTypeFunction = new LuaCSFunction(StaticLuaCallbacks.ImportType);
            loadAssemblyFunction = new LuaCSFunction(StaticLuaCallbacks.LoadAssembly);
            castFunction = new LuaCSFunction(StaticLuaCallbacks.Cast);

            LuaAPI.lua_newtable(L);
            LuaAPI.lua_newtable(L);
            LuaAPI.xlua_pushasciistring(L, "__mode");
            LuaAPI.xlua_pushasciistring(L, "v");
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.lua_setmetatable(L, -2);
            cacheRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);

            if (initers != null)
            {
                for (int i = 0; i < initers.Count; i++)
                {
                    initers[i](this);
                }
            }

            initCSharpCallLua();
        }

        private static List<Action<ObjectTranslator>> initers = null;

        public static void AddIniter(Action<ObjectTranslator> initer)
        {
            if (initers == null) initers = new List<Action<ObjectTranslator>>();
            initers.Add(initer);
        }

        enum LOGLEVEL{
            NO,
            INFO,
            WARN,
            ERROR
        }

        Type delegate_birdge_type;

#if UNITY_EDITOR
        class CompareByArgRet : IEqualityComparer<MethodInfo>
        {
            public bool Equals(MethodInfo x, MethodInfo y)
            {
                return Utils.IsParamsMatch(x, y);
            }
            public int GetHashCode(MethodInfo method)
            {
                int hc = 0;
                hc += method.ReturnType.GetHashCode();
                foreach (var pi in method.GetParameters())
                {
                    hc += pi.ParameterType.GetHashCode();
                }
                return hc;
            }
        }
#endif

        void initCSharpCallLua()
        {
            delegate_birdge_type = typeof(DelegateBridge);
#if UNITY_EDITOR
            if (!DelegateBridge.Gen_Flag)
            {
                List<Type> cs_call_lua = new List<Type>();
                foreach (var type in Utils.GetAllTypes())
                {
                    if (!type.IsInterface && typeof(GenConfig).IsAssignableFrom(type))
                    {
                        var cfg = Activator.CreateInstance(type) as GenConfig;
                        if (cfg.CSharpCallLua != null)
                        {
                            cs_call_lua.AddRange(cfg.CSharpCallLua);
                        }
                    }
                    else if(type.IsDefined(typeof(CSharpCallLuaAttribute), false))
                    {
                        cs_call_lua.Add(type);
                    }

                    if (!type.IsAbstract || !type.IsSealed) continue;

                    var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var field = fields[i];
                        if (field.IsDefined(typeof(CSharpCallLuaAttribute), false) && (typeof(IEnumerable<Type>)).IsAssignableFrom(field.FieldType))
                        {
                            cs_call_lua.AddRange(field.GetValue(null) as IEnumerable<Type>);
                        }
                    }

                    var props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    for (int i = 0; i < props.Length; i++)
                    {
                        var prop = props[i];
                        if (prop.IsDefined(typeof(CSharpCallLuaAttribute), false) && (typeof(IEnumerable<Type>)).IsAssignableFrom(prop.PropertyType))
                        {
                            cs_call_lua.AddRange(prop.GetValue(null, null) as IEnumerable<Type>);
                        }
                    }
                }
                IEnumerable<IGrouping<MethodInfo, Type>> groups = (from type in cs_call_lua
                              where typeof(Delegate).IsAssignableFrom(type)
                              select type).GroupBy(t => t.GetMethod("Invoke"), new CompareByArgRet());

                ce.SetGenInterfaces(cs_call_lua.Where(type=>type.IsInterface).ToList());
                delegate_birdge_type = ce.EmitDelegateImpl(groups);
            }
#endif
        }

#if UNITY_EDITOR
        CodeEmit ce = new CodeEmit();
#endif
        
        Dictionary<int, WeakReference> delegate_bridges = new Dictionary<int, WeakReference>();
        public Delegate CreateDelegateBridge(RealStatePtr L, Type delegateType, int idx)
        {
            LuaAPI.lua_pushvalue(L, idx);
            LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
            if (!LuaAPI.lua_isnil(L, -1))
            {
                int referenced = LuaAPI.xlua_tointeger(L, -1);
                LuaAPI.lua_pop(L, 1);

                if (delegate_bridges[referenced].IsAlive)
                {
                    DelegateBridgeBase exist_bridge = delegate_bridges[referenced].Target as DelegateBridgeBase;
                    Delegate exist_delegate;
                    if (exist_bridge.TryGetDelegate(delegateType, out exist_delegate))
                    {
                        return exist_delegate;
                    }
                    else
                    {
                        exist_delegate = exist_bridge.GetDelegateByType(delegateType);
                        exist_bridge.AddDelegate(delegateType, exist_delegate);
                        return exist_delegate;
                    }
                }
            }
            else
            {
                LuaAPI.lua_pop(L, 1);
            }

            LuaAPI.lua_pushvalue(L, idx);
            int reference = LuaAPI.luaL_ref(L);
            LuaAPI.lua_pushvalue(L, idx);
            LuaAPI.lua_pushnumber(L, reference);
            LuaAPI.lua_rawset(L, LuaIndexes.LUA_REGISTRYINDEX);
            DelegateBridgeBase bridge;
            try
            {
#if UNITY_EDITOR
                if (!DelegateBridge.Gen_Flag)
                {
                    bridge = Activator.CreateInstance(delegate_birdge_type, new object[] { reference, luaEnv }) as DelegateBridgeBase;
                }
                else
#endif
                {
                    bridge = new DelegateBridge(reference, luaEnv);
                }
            }
            catch(Exception e)
            {
                LuaAPI.lua_pushvalue(L, idx);
                LuaAPI.lua_pushnil(L);
                LuaAPI.lua_rawset(L, LuaIndexes.LUA_REGISTRYINDEX);
                LuaAPI.lua_pushnil(L);
                LuaAPI.xlua_rawseti(L, LuaIndexes.LUA_REGISTRYINDEX, reference);
                throw e;
            }
            try {
                var ret = bridge.GetDelegateByType(delegateType);
                bridge.AddDelegate(delegateType, ret);
                delegate_bridges[reference] = new WeakReference(bridge);
                return ret;
            }
            catch(Exception e)
            {
                bridge.Dispose();
                throw e;
            }
        }

        public void RemoveDelegateBridge(int reference)
        {
            if (delegate_bridges.ContainsKey(reference))
            {
                delegate_bridges.Remove(reference);
            }
        }

        public void ReleaseLuaBase(RealStatePtr L, int reference, bool is_delegate)
        {
            if(is_delegate)
            {
                LuaAPI.xlua_rawgeti(L, LuaIndexes.LUA_REGISTRYINDEX, reference);
                if (LuaAPI.lua_isnil(L, -1))
                {
                    LuaAPI.lua_pop(L, 1);
                }
                else
                {
                    LuaAPI.lua_pushvalue(L, -1);
                    LuaAPI.lua_rawget(L, LuaIndexes.LUA_REGISTRYINDEX);
                    if (LuaAPI.lua_type(L, -1) == LuaTypes.LUA_TNUMBER && LuaAPI.xlua_tointeger(L, -1) == reference) //
                    {
                        //UnityEngine.Debug.LogWarning("release delegate ref = " + luaReference);
                        LuaAPI.lua_pop(L, 1);// pop LUA_REGISTRYINDEX[func]
                        LuaAPI.lua_pushnil(L);
                        LuaAPI.lua_rawset(L, LuaIndexes.LUA_REGISTRYINDEX); // LUA_REGISTRYINDEX[func] = nil
                    }
                    else //another Delegate ref the function before the GC tick
                    {
                        LuaAPI.lua_pop(L, 2); // pop LUA_REGISTRYINDEX[func] & func
                    }
                }

                LuaAPI.lua_unref(L, reference);
                delegate_bridges.Remove(reference);
            }
            else
            {
                LuaAPI.lua_unref(L, reference);
            }
        }

		public object CreateInterfaceBridge(RealStatePtr L, Type interfaceType, int idx)
        {
            Func<int, LuaEnv, LuaBase> creator;

            if (!interfaceBridgeCreators.TryGetValue(interfaceType, out creator))
            {
#if UNITY_EDITOR
                var bridgeType = ce.EmitInterfaceImpl(interfaceType);
                creator = (int reference, LuaEnv luaenv) =>
                {
                    return Activator.CreateInstance(bridgeType, new object[] { reference, luaEnv }) as LuaBase;
                };
                interfaceBridgeCreators.Add(interfaceType, creator);
#else
                throw new InvalidCastException("This interface must add to CSharpCallLua: " + interfaceType);
#endif
            }
            LuaAPI.lua_pushvalue(L, idx);
            return creator(LuaAPI.luaL_ref(L), luaEnv);
        }

        int common_array_meta = -1;
        public void CreateArrayMetatable(RealStatePtr L)
        {
            Utils.BeginObjectRegister(null, L, this, 0, 0, 1, 0, common_array_meta);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Length", StaticLuaCallbacks.ArrayLength);
            Utils.EndObjectRegister(null, L, this, null, null,
                 typeof(System.Array), StaticLuaCallbacks.ArrayIndexer, StaticLuaCallbacks.ArrayNewIndexer);
        }

        int common_delegate_meta = -1;
        public void CreateDelegateMetatable(RealStatePtr L)
        {
            Utils.BeginObjectRegister(null, L, this, 3, 0, 0, 0, common_delegate_meta);
            Utils.RegisterFunc(L, Utils.OBJ_META_IDX, "__call", StaticLuaCallbacks.DelegateCall);
            Utils.RegisterFunc(L, Utils.OBJ_META_IDX, "__add", StaticLuaCallbacks.DelegateCombine);
            Utils.RegisterFunc(L, Utils.OBJ_META_IDX, "__sub", StaticLuaCallbacks.DelegateRemove);
            Utils.EndObjectRegister(null, L, this, null, null,
                 typeof(System.MulticastDelegate), null, null);
        }

		public void OpenLib(RealStatePtr L)
		{
            LuaAPI.lua_getglobal(L, "xlua");
            LuaAPI.xlua_pushasciistring(L, "import_type");
			LuaAPI.lua_pushstdcallcfunction(L,importTypeFunction);
			LuaAPI.lua_rawset(L, -3);
            LuaAPI.xlua_pushasciistring(L, "cast");
            LuaAPI.lua_pushstdcallcfunction(L, castFunction);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.xlua_pushasciistring(L, "load_assembly");
			LuaAPI.lua_pushstdcallcfunction(L,loadAssemblyFunction);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.xlua_pushasciistring(L, "access");
            LuaAPI.lua_pushstdcallcfunction(L, StaticLuaCallbacks.XLuaAccess);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.xlua_pushasciistring(L, "private_accessible");
            LuaAPI.lua_pushstdcallcfunction(L, StaticLuaCallbacks.XLuaPrivateAccessible);
            LuaAPI.lua_rawset(L, -3);
            LuaAPI.lua_pop(L, 1);

            LuaAPI.lua_createtable(L, 1, 4); // 4 for __gc, __tostring, __index, __newindex
            common_array_meta = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaAPI.lua_createtable(L, 1, 4); // 4 for __gc, __tostring, __index, __newindex
            common_delegate_meta = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
        }
		
		internal void createFunctionMetatable(RealStatePtr L)
		{
			LuaAPI.lua_newtable(L);
			LuaAPI.xlua_pushasciistring(L,"__gc");
			LuaAPI.lua_pushstdcallcfunction(L,metaFunctions.GcMeta);
			LuaAPI.lua_rawset(L,-3);
            LuaAPI.lua_pushlightuserdata(L, LuaAPI.xlua_tag());
            LuaAPI.lua_pushnumber(L, 1);
            LuaAPI.lua_rawset(L, -3);

            LuaAPI.lua_pushvalue(L, -1);
            int type_id = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
            LuaAPI.lua_pushnumber(L, type_id);
            LuaAPI.xlua_rawseti(L, -2, 1);
            LuaAPI.lua_pop(L, 1);

            typeIdMap.Add(typeof(LuaCSFunction), type_id);
        }
		
		internal Type FindType(string className)
		{
			foreach(Assembly assembly in assemblies)
			{
                Type klass = assembly.GetType(className);
				if(klass!=null)
				{
					return klass;
				}
			}
			return null;
		}

        bool hasMethod(Type type, string methodName)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                if (method.Name == methodName)
                {
                    return true;
                }
            }
            return false;
        }
		
		internal void collectObject(int obj_index_to_collect)
		{
			object o;
			
			if (objects.TryGetValue(obj_index_to_collect, out o))
			{
				objects.Remove(obj_index_to_collect);
                
                if (o != null)
                {
                    int obj_index;
                    //lua gc是先把weak table移除后再调用__gc，这期间同一个对象可能再次push到lua，关联到新的index
                    bool is_enum = o.GetType().IsEnum;
                    if ((is_enum ? enumMap.TryGetValue(o, out obj_index) : reverseMap.TryGetValue(o, out obj_index))
                        && obj_index == obj_index_to_collect)
                    {
                        if (is_enum)
                        {
                            enumMap.Remove(o);
                        }
                        else
                        {
                            reverseMap.Remove(o);
                        }
                    }
                }
			}
		}
		
		int addObject(object obj, bool is_valuetype, bool is_enum)
		{
            int index = objects.Add(obj);
            if (is_enum)
            {
                enumMap[obj] = index;
            }
            else if (!is_valuetype)
            {
                reverseMap[obj] = index;
            }
			
			return index;
		}
		
		internal object GetObject(RealStatePtr L,int index)
		{
            return (objectCasters.GetCaster(typeof(object))(L, index, null));
        }

        public Type GetTypeOf(RealStatePtr L, int idx)
        {
            Type type = null;
            int type_id = LuaAPI.xlua_gettypeid(L, idx);
            if (type_id != -1)
            {
                typeMap.TryGetValue(type_id, out type);
            }
            return type;
        }

        public bool Assignable<T>(RealStatePtr L, int index)
		{
            return Assignable(L, index, typeof(T));
        }

        public bool Assignable(RealStatePtr L, int index, Type type)
        {
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA) // 快路径
            {
                int udata = LuaAPI.xlua_tocsobj_safe(L, index);
                object obj;
                if (udata != -1 && objects.TryGetValue(udata, out obj))
                {
                    return type.IsAssignableFrom(obj.GetType());
                }

                int type_id = LuaAPI.xlua_gettypeid(L, index);
                Type type_of_struct;
                if (type_id != -1 && typeMap.TryGetValue(type_id, out type_of_struct)) // is struct
                {
                    return type.IsAssignableFrom(type_of_struct);
                }
            }

            return objectCheckers.GetChecker(type)(L, index);
        }

        public object GetObject(RealStatePtr L, int index, Type type)
        {
            int udata = LuaAPI.xlua_tocsobj_safe(L, index);

            if (udata != -1)
            {
                return objects.Get(udata);
            }
            else
            {
                if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
                {
                    GetCSObject get;
                    int type_id = LuaAPI.xlua_gettypeid(L, index);
                    if (type_id != -1 && type_id == decimal_type_id)
                    {
                        decimal d;
                        Get(L, index, out d);
                        return d;
                    }
                    Type type_of_struct;
                    if (type_id != -1 && typeMap.TryGetValue(type_id, out type_of_struct) && type.IsAssignableFrom(type_of_struct) && custom_get_funcs.TryGetValue(type, out get))
                    {
                        return get(L, index);
                    }
                }
                return (objectCasters.GetCaster(type)(L, index, null));
            }
        }

        public void Get<T>(RealStatePtr L, int index, out T v)
        {
            Func<RealStatePtr, int, T> get_func;
            if (tryGetGetFuncByType(typeof(T), out get_func))
            {
                v = get_func(L, index);
            }
            else
            {
                v = (T)GetObject(L, index, typeof(T));
            }
        }

        public void PushByType<T>(RealStatePtr L,  T v)
        {
            Action<RealStatePtr, T> push_func;
            if (tryGetPushFuncByType(typeof(T), out push_func))
            {
                push_func(L, v);
            }
            else
            {
                PushAny(L, v);
            }
        }

        public T[] GetParams<T>(RealStatePtr L, int index)
        {
            T[] ret = new T[Math.Max(LuaAPI.lua_gettop(L) - index + 1, 0)];
            for(int i = 0; i < ret.Length; i++)
            {
                Get(L, index + i, out ret[i]);
            }
            return ret;
        }

        public Array GetParams(RealStatePtr L, int index, Type type) //反射版本
        {
            Array ret = Array.CreateInstance(type, Math.Max(LuaAPI.lua_gettop(L) - index + 1, 0)); //这个函数，长度为0的话，返回null
            for (int i = 0; i < ret.Length; i++)
            {
                ret.SetValue(GetObject(L, index + i, type), i); 
            }
            return ret;
        }

        public T GetDelegate<T>(RealStatePtr L, int index) where T :class
        {
            
            if (LuaAPI.lua_isfunction(L, index))
            {
                return CreateDelegateBridge(L, typeof(T), index) as T;
            }
            else if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
                return (T)SafeGetCSObj(L, index);
            }
            else
            {
                return null;
            }
        }

        Dictionary<Type, int> typeIdMap = new Dictionary<Type, int>();

        //only store the type id to type map for struct
        Dictionary<int, Type> typeMap = new Dictionary<int, Type>();

        int getTypeId(RealStatePtr L, Type type, out bool is_first, LOGLEVEL log_level = LOGLEVEL.WARN)
        {
            int type_id;
            is_first = false;
            if (!typeIdMap.TryGetValue(type, out type_id)) // no reference
            {
                if (type.IsArray)
                {
                    if (common_array_meta == -1) throw new Exception("Fatal Exception! Array Metatable not inited!");
                    return common_array_meta;
                }
                if (typeof(MulticastDelegate).IsAssignableFrom(type))
                {
                    if (common_delegate_meta == -1) throw new Exception("Fatal Exception! Delegate Metatable not inited!");
                    return common_delegate_meta;
                }

                is_first = true;
                Type alias_type = null;
                aliasCfg.TryGetValue(type, out alias_type);
                LuaAPI.luaL_getmetatable(L, alias_type == null ? type.FullName : alias_type.FullName);

                if (LuaAPI.lua_isnil(L, -1)) //no meta yet, try to use reflection meta
                {
                    LuaAPI.lua_pop(L, 1);

                    if (TryDelayWrapLoader(L, alias_type == null ? type : alias_type))
                    {
                        LuaAPI.luaL_getmetatable(L, alias_type == null ? type.FullName : alias_type.FullName);
                    }
                    else
                    {
                        throw new Exception("Fatal: can not load metatable of type:" + type);
                    }
                }

                //循环依赖，自身依赖自己的class，比如有个自身类型的静态readonly对象。
                if (typeIdMap.TryGetValue(type, out type_id))
                {
                    typeIdMap.Remove(type);
                    LuaAPI.lua_unref(L, type_id);
                    if (type.IsValueType && typeMap.ContainsKey(type_id))
                    {
                        typeMap.Remove(type_id);
                    }
                }
                LuaAPI.lua_pushvalue(L, -1);
                type_id = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
                LuaAPI.lua_pushnumber(L, type_id);
                LuaAPI.xlua_rawseti(L, -2, 1);
                LuaAPI.lua_pop(L, 1);

                if (type.IsValueType)
                {
                    typeMap.Add(type_id, type);
                }

                typeIdMap.Add(type, type_id);
            }
            return type_id;
        }

        void pushPrimitive(RealStatePtr L, object o)
        {
            if (o is sbyte || o is byte || o is short || o is ushort ||
                    o is int)
            {
                int i = Convert.ToInt32(o);
                LuaAPI.xlua_pushinteger(L, i);
            }
            else if (o is uint)
            {
                LuaAPI.xlua_pushuint(L, (uint)o);
            }
            else if (o is float || o is double)
            {
                double d = Convert.ToDouble(o);
                LuaAPI.lua_pushnumber(L, d);
            }
            else if (o is IntPtr)
            {
                LuaAPI.lua_pushlightuserdata(L, (IntPtr)o);
            }
            else if (o is char)
            {
                LuaAPI.xlua_pushinteger(L, (char)o);
            }
            else if (o is long)
            {
                LuaAPI.lua_pushint64(L, Convert.ToInt64(o));
            }
            else if (o is ulong)
            {
                LuaAPI.lua_pushuint64(L, Convert.ToUInt64(o));
            }
            else if (o is bool)
            {
                bool b = (bool)o;
                LuaAPI.lua_pushboolean(L, b);
            }
            else
            {
                throw new Exception("No support type " + o.GetType());
            }
        }

        public void PushAny(RealStatePtr L, object o)
        {
            if (o == null)
            {
                LuaAPI.lua_pushnil(L);
                return;
            }

            Type type = o.GetType();
            if (type.IsPrimitive)
            {
                pushPrimitive(L, o);
            }
            else if (o is string)
            {
                LuaAPI.lua_pushstring(L, o as string);
            }
            else if (o is byte[])
            {
                LuaAPI.lua_pushstring(L, o as byte[]);
            }
            else if (o is decimal)
            {
                PushDecimal(L, (decimal)o);
            }
            else if (o is LuaBase)
            {
                ((LuaBase)o).push(L);
            }
            else if (o is LuaCSFunction)
            {
                Push(L, o as LuaCSFunction);
            }
            else if (o is ValueType)
            {
                PushCSObject push;
                if (custom_push_funcs.TryGetValue(o.GetType(), out push))
                {
                    push(L, o);
                }
                else
                {
                    Push(L, o);
                }
            }
            else
            {
                Push(L, o);
            }
        }

        Dictionary<object, int> enumMap = new Dictionary<object, int>();

        public int TranslateToEnumToTop(RealStatePtr L, Type type, int idx)
        {
            object res = null;
            LuaTypes lt = (LuaTypes)LuaAPI.lua_type(L, idx);
            if (lt == LuaTypes.LUA_TNUMBER)
            {
                int ival = (int)LuaAPI.lua_tonumber(L, idx);
                res = Enum.ToObject(type, ival);
            }
            else
            if (lt == LuaTypes.LUA_TSTRING)
            {
                string sflags = LuaAPI.lua_tostring(L, idx);
                string err = null;
                try
                {
                    res = Enum.Parse(type, sflags);
                }
                catch (ArgumentException e)
                {
                    err = e.Message;
                }
                if (err != null)
                {
                    return LuaAPI.luaL_error(L, err);
                }
            }
            else {
                return LuaAPI.luaL_error(L, "#1 argument must be a integer or a string");
            }
            PushAny(L, res);
            return 1;
        }

        public void Push(RealStatePtr L, LuaCSFunction o)
        {
            if (o.Method.IsStatic && Attribute.IsDefined(o.Method, typeof(MonoPInvokeCallbackAttribute)))
            {
                LuaAPI.lua_pushstdcallcfunction(L, o);
            }
            else
            {
                Push(L, (object)o);
                LuaAPI.lua_pushstdcallcfunction(L, metaFunctions.StaticCSFunctionWraper, 1);
            }
        }

        public void Push(RealStatePtr L, LuaBase o)
        {
            if (o == null)
            {
                LuaAPI.lua_pushnil(L);
            }
            else
            {
                o.push(L);
            }
        }

        public void Push(RealStatePtr L, object o)
        {
            if (o == null)
            {
                LuaAPI.lua_pushnil(L);
                return;
            }

            int index = -1;
            Type type = o.GetType();
            bool is_enum = type.IsEnum;
            bool is_valuetype = type.IsValueType;
            bool needcache = !is_valuetype || is_enum;
            if (needcache && (is_enum ? enumMap.TryGetValue(o, out index) : reverseMap.TryGetValue(o, out index)))
            {
                if (LuaAPI.xlua_tryget_cachedud(L, index, cacheRef) == 1)
                {
                    return;
                }
                //这里实在太经典了，weaktable先删除，然后GC会延迟调用，当index会循环利用的时候，不注释这行将会导致重复释放
                //collectObject(index);
            }

            bool is_first;
            int type_id = getTypeId(L, type, out is_first);

            //如果一个type的定义含本身静态readonly实例时，getTypeId会push一个实例，这时候应该用这个实例
            if (is_first && needcache && (is_enum ? enumMap.TryGetValue(o, out index) : reverseMap.TryGetValue(o, out index))) 
            {
                if (LuaAPI.xlua_tryget_cachedud(L, index, cacheRef) == 1)
                {
                    return;
                }
            }

            index = addObject(o, is_valuetype, is_enum);
            LuaAPI.xlua_pushcsobj(L, index, type_id, needcache, cacheRef);
        }

        public void PushObject(RealStatePtr L, object o, int type_id)
        {
            if (o == null)
            {
                LuaAPI.lua_pushnil(L);
                return;
            }

            int index = -1;
            if (reverseMap.TryGetValue(o, out index))
            {
                if (LuaAPI.xlua_tryget_cachedud(L, index, cacheRef) == 1)
                {
                    return;
                }
            }

            index = addObject(o, false, false);

            LuaAPI.xlua_pushcsobj(L, index, type_id, true, cacheRef);
        }

        public void Update(RealStatePtr L, int index, object obj)
        {
            int udata = LuaAPI.xlua_tocsobj_fast(L, index);

            if (udata != -1)
            {
                objects.Replace(udata, obj);
            }
            else
            {
                UpdateCSObject update;
                if (custom_update_funcs.TryGetValue(obj.GetType(), out update))
                {
                    update(L, index, obj);
                }
                else
                {
                    throw new Exception("can not update [" + obj + "]");
                }
            }
        }

        private object getCsObj(RealStatePtr L, int index, int udata)
        {
            object obj;
            if (udata == -1)
            {
                if (LuaAPI.lua_type(L, index) != LuaTypes.LUA_TUSERDATA) return null;

                Type type = GetTypeOf(L, index);
                if (type == typeof(decimal))
                {
                    decimal v;
                    Get(L, index, out v);
                    return v;
                }
                GetCSObject get;
                if (type != null && custom_get_funcs.TryGetValue(type, out get))
                {
                    return get(L, index);
                }
                else
                {
                    return null;
                }
            }
            else if (objects.TryGetValue(udata, out obj))
            {
                return obj;
            }
            return null;
        }

        internal object SafeGetCSObj(RealStatePtr L, int index)
        {
            return getCsObj(L, index, LuaAPI.xlua_tocsobj_safe(L, index));
        }

		internal object FastGetCSObj(RealStatePtr L,int index)
		{
            return getCsObj(L, index, LuaAPI.xlua_tocsobj_fast(L,index));
		}

        List<LuaCSFunction> fix_cs_functions = new List<LuaCSFunction>();

        internal LuaCSFunction GetFixCSFunction(int index)
        {
            return fix_cs_functions[index];
        }

        internal void PushFixCSFunction(RealStatePtr L, LuaCSFunction func)
        {
            if (func == null)
            {
                LuaAPI.lua_pushnil(L);
            }
            else
            {
                LuaAPI.xlua_pushinteger(L, fix_cs_functions.Count);
                fix_cs_functions.Add(func);
                LuaAPI.lua_pushstdcallcfunction(L, metaFunctions.FixCSFunctionWraper, 1);
            }
        }

        internal object[] popValues(RealStatePtr L,int oldTop)
		{
			int newTop=LuaAPI.lua_gettop(L);
			if(oldTop==newTop)
			{
				return null;
			}
			else
			{
				ArrayList returnValues=new ArrayList();
				for(int i=oldTop+1;i<=newTop;i++)
				{
					returnValues.Add(GetObject(L,i));
				}
				LuaAPI.lua_settop(L,oldTop);
				return returnValues.ToArray();
			}
		}

		internal object[] popValues(RealStatePtr L,int oldTop,Type[] popTypes)
		{
			int newTop=LuaAPI.lua_gettop(L);
			if(oldTop==newTop)
			{
				return null;
			}
			else
			{
				int iTypes;
				ArrayList returnValues=new ArrayList();
				if(popTypes[0] == typeof(void))
					iTypes=1;
				else
					iTypes=0;
				for(int i=oldTop+1;i<=newTop;i++)
				{
					returnValues.Add(GetObject(L,i,popTypes[iTypes]));
					iTypes++;
				}
				LuaAPI.lua_settop(L,oldTop);
				return returnValues.ToArray();
			}
		}

        public delegate void PushCSObject(RealStatePtr L, object obj);
        public delegate object GetCSObject(RealStatePtr L, int idx);
        public delegate void UpdateCSObject(RealStatePtr L, int idx, object obj);

        private Dictionary<Type, PushCSObject> custom_push_funcs = new Dictionary<Type, PushCSObject>();
        private Dictionary<Type, GetCSObject> custom_get_funcs = new Dictionary<Type, GetCSObject>();
        private Dictionary<Type, UpdateCSObject> custom_update_funcs = new Dictionary<Type, UpdateCSObject>();

        void registerCustomOp(Type type, PushCSObject push, GetCSObject get, UpdateCSObject update)
        {
            if (push != null) custom_push_funcs.Add(type, push);
            if (get != null) custom_get_funcs.Add(type, get);
            if (update != null) custom_update_funcs.Add(type, update);
        }

        public bool HasCustomOp(Type type)
        {
            return custom_push_funcs.ContainsKey(type);
        }

        private Dictionary<Type, Delegate> push_func_with_type = null;
        
        bool tryGetPushFuncByType<T>(Type type, out T func) where T : class
        {
            if (push_func_with_type == null)
            {
                push_func_with_type = new Dictionary<Type, Delegate>()
                {
                    {typeof(int),  new Action<RealStatePtr, int>(LuaAPI.xlua_pushinteger) },
                    {typeof(double), new Action<RealStatePtr, double>(LuaAPI.lua_pushnumber) },
                    {typeof(string), new Action<RealStatePtr, string>(LuaAPI.lua_pushstring) },
                    {typeof(byte[]), new Action<RealStatePtr, byte[]>(LuaAPI.lua_pushstring) },
                    {typeof(bool), new Action<RealStatePtr, bool>(LuaAPI.lua_pushboolean) },
                    {typeof(long), new Action<RealStatePtr, long>(LuaAPI.lua_pushint64) },
                    {typeof(ulong), new Action<RealStatePtr, ulong>(LuaAPI.lua_pushuint64) },
                    {typeof(IntPtr), new Action<RealStatePtr, IntPtr>(LuaAPI.lua_pushlightuserdata) },
                    {typeof(decimal), new Action<RealStatePtr, decimal>(PushDecimal) },
                    {typeof(byte),  new Action<RealStatePtr, byte>((L, v) => LuaAPI.xlua_pushinteger(L, v)) },
                    {typeof(sbyte),  new Action<RealStatePtr, sbyte>((L, v) => LuaAPI.xlua_pushinteger(L, v)) },
                    {typeof(char),  new Action<RealStatePtr, char>((L, v) => LuaAPI.xlua_pushinteger(L, v)) },
                    {typeof(short),  new Action<RealStatePtr, short>((L, v) => LuaAPI.xlua_pushinteger(L, v)) },
                    {typeof(ushort),  new Action<RealStatePtr, ushort>((L, v) => LuaAPI.xlua_pushinteger(L, v)) },
                    {typeof(uint),  new Action<RealStatePtr, uint>(LuaAPI.xlua_pushuint) },
                    {typeof(float),  new Action<RealStatePtr, float>((L, v) => LuaAPI.lua_pushnumber(L, v)) },
                };
            }

            Delegate obj;
            if (push_func_with_type.TryGetValue(type, out obj))
            {
                func = obj as T;
                return true;
            }
            else
            {
                func = null;
                return false;
            }
        }

        private Dictionary<Type, Delegate> get_func_with_type = null;

        bool tryGetGetFuncByType<T>(Type type, out T func) where T : class
        {
            if (get_func_with_type == null)
            {
                get_func_with_type = new Dictionary<Type, Delegate>()
                {
                    {typeof(int), new Func<RealStatePtr, int, int>(LuaAPI.xlua_tointeger) },
                    {typeof(double), new Func<RealStatePtr, int, double>(LuaAPI.lua_tonumber) },
                    {typeof(string), new Func<RealStatePtr, int, string>(LuaAPI.lua_tostring) },
                    {typeof(byte[]), new Func<RealStatePtr, int, byte[]>(LuaAPI.lua_tobytes) },
                    {typeof(bool), new Func<RealStatePtr, int, bool>(LuaAPI.lua_toboolean) },
                    {typeof(long), new Func<RealStatePtr, int, long>(LuaAPI.lua_toint64) },
                    {typeof(ulong), new Func<RealStatePtr, int, ulong>(LuaAPI.lua_touint64) },
                    {typeof(IntPtr), new Func<RealStatePtr, int, IntPtr>(LuaAPI.lua_touserdata) },
                    {typeof(decimal), new Func<RealStatePtr, int, decimal>((L, idx) => {
                        decimal ret;
                        Get(L, idx, out ret);
                        return ret;
                    }) },
                    {typeof(byte), new Func<RealStatePtr, int, byte>((L, idx) => (byte)LuaAPI.xlua_tointeger(L, idx) ) },
                    {typeof(sbyte), new Func<RealStatePtr, int, sbyte>((L, idx) => (sbyte)LuaAPI.xlua_tointeger(L, idx) ) },
                    {typeof(char), new Func<RealStatePtr, int, char>((L, idx) => (char)LuaAPI.xlua_tointeger(L, idx) ) },
                    {typeof(short), new Func<RealStatePtr, int, short>((L, idx) => (short)LuaAPI.xlua_tointeger(L, idx) ) },
                    {typeof(ushort), new Func<RealStatePtr, int, ushort>((L, idx) => (ushort)LuaAPI.xlua_tointeger(L, idx) ) },
                    {typeof(uint), new Func<RealStatePtr, int, uint>(LuaAPI.xlua_touint) },
                    {typeof(float), new Func<RealStatePtr, int, float>((L, idx) => (float)LuaAPI.lua_tonumber(L, idx) ) },
                };
            }

            Delegate obj;
            if (get_func_with_type.TryGetValue(type, out obj))
            {
                func = obj as T;
                return true;
            }
            else
            {
                func = null;
                return false;
            }
        }


        public delegate void GetFunc<T>(RealStatePtr L, int idx,  out T val);

        public void RegisterPushAndGetAndUpdate<T>(Action<RealStatePtr, T> push, GetFunc<T> get, Action<RealStatePtr, int, T> update)
        {
            Type type = typeof(T);
            Action<RealStatePtr, T> org_push;
            Func<RealStatePtr, int, T> org_get;
            if (tryGetPushFuncByType(type, out org_push) || tryGetGetFuncByType(type, out org_get))
            {
                throw new InvalidOperationException("push or get of " + type + " has register!");
            }
            push_func_with_type.Add(type, push);
            get_func_with_type.Add(type, new Func<RealStatePtr, int, T>((L, idx) => {
                T ret;
                get(L, idx, out ret);
                return ret;
            }));

            registerCustomOp(type, 
                (RealStatePtr L, object obj) => {
                    push(L, (T)obj);
                },
                (RealStatePtr L, int idx) => {
                    T val;
                    get(L, idx, out val);
                    return val;
                },
                (RealStatePtr L, int idx, object obj) => {
                    update(L, idx, (T)obj);
                }
            );
        }

        public void RegisterCaster<T>(GetFunc<T> get)
        {
            objectCasters.AddCaster(typeof(T), (L, idx, o) =>
            {
                T obj;
                get(L, idx, out obj);
                return obj;
            });
        }

        int decimal_type_id = -1;

        public void PushDecimal(RealStatePtr L, decimal val)
        {
            if (decimal_type_id == -1)
            {
                bool is_first;
                decimal_type_id = getTypeId(L, typeof(decimal), out is_first);
            }
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 16, decimal_type_id);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail for decimal ,value=" + val);
            }
            
        }

        public bool IsDecimal(RealStatePtr L, int index)
        {
            if (decimal_type_id == -1) return false;
            return LuaAPI.xlua_gettypeid(L, index) == decimal_type_id;
        }

        public void Get(RealStatePtr L, int index, out decimal val)
        {
            LuaTypes lua_type = LuaAPI.lua_type(L, index);
            if (lua_type == LuaTypes.LUA_TUSERDATA)
            {
                if (LuaAPI.xlua_gettypeid(L, index) != decimal_type_id)
                {
                    throw new Exception("invalid userdata for decimal!");
                }

                IntPtr buff = LuaAPI.lua_touserdata(L, index);

                if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack decimal fail!");
                }
            }
            else if(lua_type == LuaTypes.LUA_TNUMBER)
            {
                if (LuaAPI.lua_isint64(L, index))
                {
                    val = (decimal)LuaAPI.lua_toint64(L, index);
                }
                else
                {
                    val = (decimal)LuaAPI.lua_tonumber(L, index); // has gc
                }
            }
            else
            {
                throw new Exception("invalid lua value for decimal, LuaType=" + lua_type);
            }
        }

#if OBJECT_POOL_STAT
        public void Stat()
        {
            UnityEngine.Debug.Log("---------------------------STAT----------------------------------");
            objects.Stat();
            UnityEngine.Debug.Log("reverse_map.count = " + reverseMap.Count);
            Hashtable ht = new Hashtable();
            foreach (var obj in reverseMap.Keys)
            {
                Type type = obj.GetType();
                ht[type] = ht[type] == null ? 1 : (int)ht[type] + 1;
            }
            foreach (var key in ht.Keys)
            {
                UnityEngine.Debug.Log("reverse type:" + key + ", num:" + ht[key]);
            }
        }
#endif
    }
}