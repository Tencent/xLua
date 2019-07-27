using System;

namespace XLua.LuaDLL
{
    using System.Runtime.InteropServices;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || XLUA_GENERAL || (UNITY_WSA && !UNITY_EDITOR)
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void TableSizeReport(IntPtr p, int size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ObjectRelationshipReport(IntPtr parent, IntPtr child, RelationshipType type, string key, double d, string key2);
#else
    public delegate void TableSizeReport(IntPtr p, int size);

    public delegate void ObjectRelationshipReport(IntPtr parent, IntPtr child, RelationshipType type, string key, double d, string key2);
#endif

    public enum RelationshipType
    {
        TableValue = 1,
        NumberKeyTableValue = 2,
        KeyOfTable = 3,
        Metatable = 4,
        Upvalue = 5,
    }

    public partial class Lua
    {
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_report_table_size(IntPtr L, TableSizeReport cb, int fast);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void xlua_report_object_relationship(IntPtr L, ObjectRelationshipReport cb);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xlua_registry_pointer(IntPtr L);

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr xlua_global_pointer(IntPtr L);
    }
}

namespace XLua
{
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    public static class LuaMemoryLeakChecker
    {
        const string UNKNOW_KEY = "???";
        const string METATABLE_KEY = "__metatable";
        const string KEY_OF_TABLE = "!KEY!";

        public class Data
        {
            internal int Memroy = 0;
            internal Dictionary<IntPtr, int> TableSizes = new Dictionary<IntPtr, int>();

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("memroy:{0}, table count:{1}", Memroy, TableSizes.Count);
                sb.AppendLine();

                if (TableSizes.Count < 10)
                {
                    foreach (var kv in TableSizes)
                    {
                        sb.AppendLine(string.Format("table({0}) : {1}", kv.Key, kv.Value));
                    }
                }
                else
                {
                    sb.AppendLine("too much table...");
                }

                return sb.ToString();
            }

            public int PotentialLeakCount { get { return TableSizes.Count; } }
        }

        static Data getSizeReport(LuaEnv env)
        {
            Data data = new Data();
            data.Memroy = env.Memroy;

            LuaDLL.Lua.xlua_report_table_size(env.L, (IntPtr p, int size) => {
                data.TableSizes.Add(p, size);
            }, 0);

            return data;
        }

        struct RefInfo
        {
            public string Key;

            public bool HasNext;

            public IntPtr Parent;

            public bool IsNumberKey;
        }

        static string makeKey(LuaDLL.RelationshipType type, string key, double d, string key2)
        {
            switch(type)
            {
                case LuaDLL.RelationshipType.TableValue:
                    return key== null ? ((LuaTypes)(int)d).ToString() : key;
                case LuaDLL.RelationshipType.NumberKeyTableValue:
                    return string.Format("[{0}]", d);
                case LuaDLL.RelationshipType.KeyOfTable:
                    return KEY_OF_TABLE;
                case LuaDLL.RelationshipType.Metatable:
                    return METATABLE_KEY;
                case LuaDLL.RelationshipType.Upvalue:
                    return string.Format("{0}:local {1}", key, key2);
            }
            return UNKNOW_KEY;
        }

        static Dictionary<IntPtr, List<RefInfo>> getRelationship(LuaEnv env)
        {
            Dictionary<IntPtr, List<RefInfo>> result = new Dictionary<IntPtr, List<RefInfo>>();
            int top = LuaDLL.Lua.lua_gettop(env.L);
            IntPtr registryPointer = LuaDLL.Lua.xlua_registry_pointer(env.L);
            IntPtr globalPointer = LuaDLL.Lua.xlua_global_pointer(env.L);

            LuaDLL.Lua.xlua_report_object_relationship(env.L, (IntPtr parent, IntPtr child, LuaDLL.RelationshipType type, string key, double d, string key2) => {
                List<RefInfo> infos;
                try
                {
                    if (!result.TryGetValue(child, out infos))
                    {
                        infos = new List<RefInfo>();
                        result.Add(child, infos);
                    }
                    string keyOfRef = makeKey(type, key, d, key2);

                    bool hasNext = type != LuaDLL.RelationshipType.Upvalue;

                    if (hasNext)
                    {
                        if (parent == registryPointer)
                        {
                            keyOfRef = "_R." + keyOfRef;
                            hasNext = false;
                        }
                        else if (parent == globalPointer)
                        {
                            keyOfRef = "_G." + keyOfRef;
                            hasNext = false;
                        }
                    }

                    infos.Add(new RefInfo()
                    {
                        Key = keyOfRef,
                        HasNext = hasNext,
                        Parent = parent,
                        IsNumberKey = type == LuaDLL.RelationshipType.NumberKeyTableValue,
                    });
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e.Message);
                }
            });
            LuaDLL.Lua.lua_settop(env.L, top);
            return result;
        }

        public static Data StartMemoryLeakCheck(this LuaEnv env)
        {
            env.FullGc();
            return getSizeReport(env);
        }

        static Data findGrowing(Data from, Data to)
        {
            Data result = new Data();
            result.Memroy = to.Memroy;
            bool keepEqual = to.Memroy <= from.Memroy;
            foreach (var kv in to.TableSizes)
            {
                int oldSize;
                if (from.TableSizes.TryGetValue(kv.Key, out oldSize) && (oldSize < kv.Value || (keepEqual && oldSize == kv.Value))) // exist table
                {
                    result.TableSizes.Add(kv.Key, kv.Value);
                }
            }
            return result;
        }

        public static Data MemoryLeakCheck(this LuaEnv env, Data last)
        {
            env.FullGc();
            return findGrowing(last, getSizeReport(env));
        }

        public static string MemoryLeakReport(this LuaEnv env, Data data, int maxLevel = 10)
        {
            env.FullGc();
            var relationshipInfo = getRelationship(env);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("total memroy: " + data.Memroy);
            foreach(var kv in data.TableSizes)
            {
                List<RefInfo> infos;
                if (!relationshipInfo.TryGetValue(kv.Key, out infos))
                {
                    continue;
                }
                List<string> paths = new List<string>();
                for(int i = 0; i < maxLevel; i++)
                {
                    int pathCount = paths.Count;
                    paths.AddRange(infos.Where(info => !info.HasNext).Select(info => info.Key));
                    if ((paths.Count - pathCount) != infos.Count)
                    {
                        infos = infos.Where(info => info.HasNext)
                            .SelectMany((info) =>
                            {
                                List<RefInfo> infosOfParent;
                                if (!relationshipInfo.TryGetValue(info.Parent, out infosOfParent))
                                {
                                    return new List<RefInfo>();
                                }

                                return infosOfParent.Select(pinfo =>
                                {
                                    var parentkey = pinfo.Key;
                                    return new RefInfo()
                                    {
                                        HasNext = pinfo.HasNext,
                                        Key = string.Format(info.IsNumberKey ? "{0}{1}" : "{0}.{1}", pinfo.Key, info.Key),
                                        Parent = pinfo.Parent,
                                        IsNumberKey = pinfo.IsNumberKey,
                                    };
                                });
                            }).ToList();
                    }
                    else
                    {
                        break;
                    }
                }

                infos = infos.Where(info => info.HasNext).ToList();
                if (infos.Count != 0)
                {
                    paths.AddRange(infos.Select(info => "..." + info.Key));
                }
                sb.AppendLine(string.Format("potential leak({0}) in {{{1}}}", kv.Value, string.Join(",", paths.ToArray())));
            }
            return sb.ToString();
        }
    }
}
