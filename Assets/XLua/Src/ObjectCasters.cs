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

using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace XLua
{
    public delegate bool ObjectCheck(RealStatePtr L, int idx);

    public delegate object ObjectCast(RealStatePtr L, int idx, object target); // if target is null, will new one

    public class ObjectCheckers
    {
        Dictionary<Type, ObjectCheck> checkersMap = new Dictionary<Type, ObjectCheck>();
        ObjectTranslator translator;

        public ObjectCheckers(ObjectTranslator translator)
        {
            this.translator = translator;
            checkersMap[typeof(sbyte)] = numberCheck;
            checkersMap[typeof(byte)] = numberCheck;
            checkersMap[typeof(short)] = numberCheck;
            checkersMap[typeof(ushort)] = numberCheck;
            checkersMap[typeof(int)] = numberCheck;
            checkersMap[typeof(uint)] = numberCheck;
            checkersMap[typeof(long)] = int64Check;
            checkersMap[typeof(ulong)] = uint64Check;
            checkersMap[typeof(double)] = numberCheck;
            checkersMap[typeof(char)] = numberCheck;
            checkersMap[typeof(float)] = numberCheck;
            checkersMap[typeof(decimal)] = decimalCheck;
            checkersMap[typeof(bool)] = boolCheck;
            checkersMap[typeof(string)] = strCheck;
            checkersMap[typeof(object)] = objectCheck;
            checkersMap[typeof(byte[])] = bytesCheck;
            checkersMap[typeof(IntPtr)] = intptrCheck;

            checkersMap[typeof(LuaTable)] = luaTableCheck;
            checkersMap[typeof(LuaFunction)] = luaFunctionCheck;
        }

        private static bool objectCheck(RealStatePtr L, int idx)
        {
            return true;
        }

        private bool luaTableCheck(RealStatePtr L, int idx)
        {
            return LuaAPI.lua_isnil(L, idx) || LuaAPI.lua_istable(L, idx) || (LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TUSERDATA && translator.SafeGetCSObj(L, idx) is LuaTable);
        }

        private bool numberCheck(RealStatePtr L, int idx)
        {
            return LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TNUMBER;
        }

        private bool decimalCheck(RealStatePtr L, int idx)
        {
            return LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TNUMBER || translator.IsDecimal(L, idx);
        }

        private bool strCheck(RealStatePtr L, int idx)
        {
            return LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TSTRING || LuaAPI.lua_isnil(L, idx);
        }

        private bool bytesCheck(RealStatePtr L, int idx)
        {
            return LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TSTRING || LuaAPI.lua_isnil(L, idx) || (LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TUSERDATA && translator.SafeGetCSObj(L, idx) is byte[]);
        }

        private bool boolCheck(RealStatePtr L, int idx)
        {
            return LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TBOOLEAN;
        }

        private bool int64Check(RealStatePtr L, int idx)
        {
            return LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TNUMBER || LuaAPI.lua_isint64(L, idx);
        }

        private bool uint64Check(RealStatePtr L, int idx)
        {
            return LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TNUMBER || LuaAPI.lua_isuint64(L, idx);
        }

        private bool luaFunctionCheck(RealStatePtr L, int idx)
        {
            return LuaAPI.lua_isnil(L, idx) || LuaAPI.lua_isfunction(L, idx) || (LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TUSERDATA && translator.SafeGetCSObj(L, idx) is LuaFunction);
        }

        private bool intptrCheck(RealStatePtr L, int idx)
        {
            return LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TLIGHTUSERDATA;
        }

        private ObjectCheck genChecker(Type type)
        {
            ObjectCheck fixTypeCheck = (RealStatePtr L, int idx) =>
            {
                if (LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TUSERDATA)
                {
                    object obj = translator.SafeGetCSObj(L, idx);
                    if (obj != null)
                    {
                        return type.IsAssignableFrom(obj.GetType());
                    }
                    else
                    {
                        Type type_of_obj = translator.GetTypeOf(L, idx);
                        if (type_of_obj != null) return type.IsAssignableFrom(type_of_obj);
                    }
                }
                return false;
            };

            if (!type.IsAbstract() && typeof(Delegate).IsAssignableFrom(type))
            {
                return (RealStatePtr L, int idx) =>
                {
                    return LuaAPI.lua_isnil(L, idx) || LuaAPI.lua_isfunction(L, idx) || fixTypeCheck(L, idx);
                };
            }
            else if (type.IsEnum())
            {
                return fixTypeCheck;
            }
            else if (type.IsInterface())
            {
                return (RealStatePtr L, int idx) =>
                {
                    return LuaAPI.lua_isnil(L, idx) || LuaAPI.lua_istable(L, idx) || fixTypeCheck(L, idx);
                };
            }
            else
            {
                if ((type.IsClass() && type.GetConstructor(System.Type.EmptyTypes) != null)) //class has default construtor
                {
                    return (RealStatePtr L, int idx) =>
                    {
                        return LuaAPI.lua_isnil(L, idx) || LuaAPI.lua_istable(L, idx) || fixTypeCheck(L, idx);
                    };
                }
                else if (type.IsValueType())
                {
                    return (RealStatePtr L, int idx) =>
                    {
                        return LuaAPI.lua_istable(L, idx) || fixTypeCheck(L, idx);
                    };
                }
                else if (type.IsArray)
                {
                    return (RealStatePtr L, int idx) =>
                    {
                        return LuaAPI.lua_isnil(L, idx) || LuaAPI.lua_istable(L, idx) || fixTypeCheck(L, idx);
                    };
                }
                else
                {
                    return (RealStatePtr L, int idx) =>
                    {
                        return LuaAPI.lua_isnil(L, idx) || fixTypeCheck(L, idx);
                    };
                }
            }
        }

        public ObjectCheck genNullableChecker(ObjectCheck oc)
        {
            return (RealStatePtr L, int idx) =>
            {
                return LuaAPI.lua_isnil(L, idx) || oc(L, idx);
            };
        }

        public ObjectCheck GetChecker(Type type)
        {
            if (type.IsByRef) type = type.GetElementType();

            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return genNullableChecker(GetChecker(underlyingType));
            }
            ObjectCheck oc;
            if (!checkersMap.TryGetValue(type, out oc))
            {
                oc = genChecker(type);
                checkersMap.Add(type, oc);
            }
            return oc;
        }
    }

    public class ObjectCasters
    {
        Dictionary<Type, ObjectCast> castersMap = new Dictionary<Type, ObjectCast>();
        ObjectTranslator translator;

        public ObjectCasters(ObjectTranslator translator)
        {
            this.translator = translator;
            castersMap[typeof(char)] = charCaster;
            castersMap[typeof(sbyte)] = sbyteCaster;
            castersMap[typeof(byte)] = byteCaster;
            castersMap[typeof(short)] = shortCaster;
            castersMap[typeof(ushort)] = ushortCaster;
            castersMap[typeof(int)] = intCaster;
            castersMap[typeof(uint)] = uintCaster;
            castersMap[typeof(long)] = longCaster;
            castersMap[typeof(ulong)] = ulongCaster;
            castersMap[typeof(double)] = getDouble;
            castersMap[typeof(float)] = floatCaster;
            castersMap[typeof(decimal)] = decimalCaster;
            castersMap[typeof(bool)] = getBoolean;
            castersMap[typeof(string)] =  getString;
            castersMap[typeof(object)] = getObject;
            castersMap[typeof(byte[])] = getBytes;
            castersMap[typeof(IntPtr)] = getIntptr;
            //special type
            castersMap[typeof(LuaTable)] = getLuaTable;
            castersMap[typeof(LuaFunction)] = getLuaFunction;
        }

        private static object charCaster(RealStatePtr L, int idx, object target)
        {
            return (char)LuaAPI.xlua_tointeger(L, idx);
        }

        private static object sbyteCaster(RealStatePtr L, int idx, object target)
        {
            return (sbyte)LuaAPI.xlua_tointeger(L, idx);
        }

        private static object byteCaster(RealStatePtr L, int idx, object target)
        {
            return (byte)LuaAPI.xlua_tointeger(L, idx);
        }

        private static object shortCaster(RealStatePtr L, int idx, object target)
        {
            return (short)LuaAPI.xlua_tointeger(L, idx);
        }

        private static object ushortCaster(RealStatePtr L, int idx, object target)
        {
            return (ushort)LuaAPI.xlua_tointeger(L, idx);
        }

        private static object intCaster(RealStatePtr L, int idx, object target)
        {
            return LuaAPI.xlua_tointeger(L, idx);
        }

        private static object uintCaster(RealStatePtr L, int idx, object target)
        {
            return LuaAPI.xlua_touint(L, idx);
        }

        private static object longCaster(RealStatePtr L, int idx, object target)
        {
            return LuaAPI.lua_toint64(L, idx);
        }

        private static object ulongCaster(RealStatePtr L, int idx, object target)
        {
            return LuaAPI.lua_touint64(L, idx);
        }

        private static object getDouble(RealStatePtr L, int idx, object target)
        {
            return LuaAPI.lua_tonumber(L, idx);
        }

        private static object floatCaster(RealStatePtr L, int idx, object target)
        {
            return (float)LuaAPI.lua_tonumber(L, idx);
        }

        private object decimalCaster(RealStatePtr L, int idx, object target)
        {
            decimal ret;
            translator.Get(L, idx, out ret);
            return ret;
        }

        private static object getBoolean(RealStatePtr L, int idx, object target)
        {
            return LuaAPI.lua_toboolean(L, idx);
        }

        private static object getString(RealStatePtr L, int idx, object target)
        {
            return LuaAPI.lua_tostring(L, idx);
        }

        private object getBytes(RealStatePtr L, int idx, object target)
        {
            return LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TSTRING ? LuaAPI.lua_tobytes(L, idx) : translator.SafeGetCSObj(L, idx) as byte[];
        }

        private object getIntptr(RealStatePtr L, int idx, object target)
        {
            return LuaAPI.lua_touserdata(L, idx);
        }

        private object getObject(RealStatePtr L, int idx, object target)
        {
            //return translator.getObject(L, idx); //TODO: translator.getObject move to here??
            LuaTypes type = (LuaTypes)LuaAPI.lua_type(L, idx);
            switch (type)
            {
                case LuaTypes.LUA_TNUMBER:
                    {
                        if (LuaAPI.lua_isint64(L, idx))
                        {
                            return LuaAPI.lua_toint64(L, idx);
                        }
                        else if(LuaAPI.lua_isinteger(L, idx))
                        {
                            return LuaAPI.xlua_tointeger(L, idx);
                        }
                        else
                        {
                            return LuaAPI.lua_tonumber(L, idx);
                        }
                    }
                case LuaTypes.LUA_TSTRING:
                    {
                        return LuaAPI.lua_tostring(L, idx);
                    }
                case LuaTypes.LUA_TBOOLEAN:
                    {
                        return LuaAPI.lua_toboolean(L, idx);
                    }
                case LuaTypes.LUA_TTABLE:
                    {
                        return getLuaTable(L, idx, null);
                    }
                case LuaTypes.LUA_TFUNCTION:
                    {
                        return getLuaFunction(L, idx, null);
                    }
                case LuaTypes.LUA_TUSERDATA:
                    {
                        if (LuaAPI.lua_isint64(L, idx))
                        {
                            return LuaAPI.lua_toint64(L, idx);
                        }
                        else if(LuaAPI.lua_isuint64(L, idx))
                        {
                            return LuaAPI.lua_touint64(L, idx);
                        }
                        else
                        {
                            object obj = translator.SafeGetCSObj(L, idx);
                            return (obj is RawObject) ? (obj as RawObject).Target : obj;
                        }
                    }
                default:
                    return null;
            }
        }

        private object getLuaTable(RealStatePtr L, int idx, object target)
        {
            if (LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TUSERDATA)
            {
                object obj = translator.SafeGetCSObj(L, idx);
                return (obj != null && obj is LuaTable) ? obj : null;
            }
            if (!LuaAPI.lua_istable(L, idx))
            {
                return null;
            }
            LuaAPI.lua_pushvalue(L, idx);
            return new LuaTable(LuaAPI.luaL_ref(L), translator.luaEnv);
        }

        private object getLuaFunction(RealStatePtr L, int idx, object target)
        {
            if (LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TUSERDATA)
            {
                object obj = translator.SafeGetCSObj(L, idx);
                return (obj != null && obj is LuaFunction) ? obj : null;
            }
            if (!LuaAPI.lua_isfunction(L, idx))
            {
                return null;
            }
            LuaAPI.lua_pushvalue(L, idx);
            return new LuaFunction(LuaAPI.luaL_ref(L), translator.luaEnv);
        }

        public void AddCaster(Type type, ObjectCast oc)
        {
            castersMap[type] = oc;
        }

        private ObjectCast genCaster(Type type)
        {
            ObjectCast fixTypeGetter = (RealStatePtr L, int idx, object target) =>
            {
                if (LuaAPI.lua_type(L, idx) == LuaTypes.LUA_TUSERDATA)
                {
                    object obj = translator.SafeGetCSObj(L, idx);
                    return (obj != null && type.IsAssignableFrom(obj.GetType())) ? obj : null;
                }
                return null;
            }; 

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                return (RealStatePtr L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null) return obj;

                    if (!LuaAPI.lua_isfunction(L, idx))
                    {
                        return null;
                    }

                    return translator.CreateDelegateBridge(L, type, idx);
                };
            }
            else if (typeof(DelegateBridgeBase).IsAssignableFrom(type))
            {
                return (RealStatePtr L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null) return obj;

                    if (!LuaAPI.lua_isfunction(L, idx))
                    {
                        return null;
                    }

                    return translator.CreateDelegateBridge(L, null, idx);
                };
            }
            else if (type.IsInterface())
            {
                return (RealStatePtr L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null) return obj;

                    if (!LuaAPI.lua_istable(L, idx))
                    {
                        return null;
                    }
                    return translator.CreateInterfaceBridge(L, type, idx);
                };
            }
            else if (type.IsEnum())
            {
                return (RealStatePtr L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null) return obj;

                    LuaTypes lua_type = LuaAPI.lua_type(L, idx);
                    if (lua_type == LuaTypes.LUA_TSTRING)
                    {
                        return Enum.Parse(type, LuaAPI.lua_tostring(L, idx));
                    }
                    else if (lua_type == LuaTypes.LUA_TNUMBER)
                    {
                        return Enum.ToObject(type, LuaAPI.xlua_tointeger(L, idx));
                    }
                    throw new InvalidCastException("invalid value for enum " + type);
                };
            }
            else if (type.IsArray)
            {
                return (RealStatePtr L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null) return obj;

                    if (!LuaAPI.lua_istable(L, idx))
                    {
                        return null;
                    }

                    uint len = LuaAPI.xlua_objlen(L, idx);
                    int n = LuaAPI.lua_gettop(L);
                    idx = idx > 0 ? idx : LuaAPI.lua_gettop(L) + idx + 1;// abs of index
                    Type et = type.GetElementType();
                    ObjectCast elementCaster = GetCaster(et);
                    Array ary = target == null ? Array.CreateInstance(et, (int)len) : target as Array;
                    if (!LuaAPI.lua_checkstack(L, 1))
                    {
                        throw new Exception("stack overflow while cast to Array");
                    }
                    for (int i = 0; i < len; ++i)
                    {
                        LuaAPI.lua_pushnumber(L, i + 1);
                        LuaAPI.lua_rawget(L, idx);
                        if (et.IsPrimitive())
                        {
                            if (!StaticLuaCallbacks.TryPrimitiveArraySet(type, L, ary, i, n + 1))
                            {
                                ary.SetValue(elementCaster(L, n + 1, null), i);
                            }
                        }
                        else
                        {
                            if (InternalGlobals.genTryArraySetPtr == null
                                || !InternalGlobals.genTryArraySetPtr(type, L, translator, ary, i, n + 1))
                            {
                                ary.SetValue(elementCaster(L, n + 1, null), i);
                            }
                        }
                        LuaAPI.lua_pop(L, 1);
                    }
                    return ary;
                };
            }
            else if (typeof(IList).IsAssignableFrom(type) && type.IsGenericType())
            {
                Type elementType = type.GetGenericArguments()[0];
                ObjectCast elementCaster = GetCaster(elementType);

                return (RealStatePtr L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null) return obj;

                    if (!LuaAPI.lua_istable(L, idx))
                    {
                        return null;
                    }

                    obj = target == null ? Activator.CreateInstance(type) : target;
                    int n = LuaAPI.lua_gettop(L);
                    idx = idx > 0 ? idx : LuaAPI.lua_gettop(L) + idx + 1;// abs of index
                    IList list = obj as IList;


                    uint len = LuaAPI.xlua_objlen(L, idx);
                    if (!LuaAPI.lua_checkstack(L, 1))
                    {
                        throw new Exception("stack overflow while cast to IList");
                    }
                    for (int i = 0; i < len; ++i)
                    {
                        LuaAPI.lua_pushnumber(L, i + 1);
                        LuaAPI.lua_rawget(L, idx);
                        if (i < list.Count && target != null)
                        {
                            if (translator.Assignable(L, n + 1, elementType))
                            {
                                list[i] = elementCaster(L, n + 1, list[i]); ;
                            }
                        }
                        else
                        {
                            if (translator.Assignable(L, n + 1, elementType))
                            {
                                list.Add(elementCaster(L, n + 1, null));
                            }
                        }
                        LuaAPI.lua_pop(L, 1);
                    }
                    return obj;
                };
            }
            else if (typeof(IDictionary).IsAssignableFrom(type) && type.IsGenericType())
            {
                Type keyType = type.GetGenericArguments()[0];
                ObjectCast keyCaster = GetCaster(keyType);
                Type valueType = type.GetGenericArguments()[1];
                ObjectCast valueCaster = GetCaster(valueType);

                return (RealStatePtr L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null) return obj;

                    if (!LuaAPI.lua_istable(L, idx))
                    {
                        return null;
                    }

                    IDictionary dic = (target == null ? Activator.CreateInstance(type) : target) as IDictionary;
                    int n = LuaAPI.lua_gettop(L);
                    idx = idx > 0 ? idx : LuaAPI.lua_gettop(L) + idx + 1;// abs of index

                    LuaAPI.lua_pushnil(L);
                    if (!LuaAPI.lua_checkstack(L, 1))
                    {
                        throw new Exception("stack overflow while cast to IDictionary");
                    }
                    while (LuaAPI.lua_next(L, idx) != 0)
                    {
                        if (translator.Assignable(L, n + 1, keyType) && translator.Assignable(L, n + 2, valueType))
                        {
                            object k = keyCaster(L, n + 1, null);
                            dic[k] = valueCaster(L, n + 2, !dic.Contains(k) ? null : dic[k]);
                        }
                        LuaAPI.lua_pop(L, 1); // removes value, keeps key for next iteration
                    }
                    return dic;
                };
            }
            else if ((type.IsClass() && type.GetConstructor(System.Type.EmptyTypes) != null) || (type.IsValueType() && !type.IsEnum())) //class has default construtor
            {
                return (RealStatePtr L, int idx, object target) =>
                {
                    object obj = fixTypeGetter(L, idx, target);
                    if (obj != null) return obj;

                    if (!LuaAPI.lua_istable(L, idx))
                    {
                        return null;
                    }

                    obj = target == null ? Activator.CreateInstance(type) : target;

                    int n = LuaAPI.lua_gettop(L);
                    idx = idx > 0 ? idx : LuaAPI.lua_gettop(L) + idx + 1;// abs of index
                    if (!LuaAPI.lua_checkstack(L, 1))
                    {
                        throw new Exception("stack overflow while cast to " + type);
                    }
                    /*foreach (PropertyInfo prop in type.GetProperties())
                    {
                        var _setMethod = prop.GetSetMethod();

                        if (_setMethod == null ||
                            _setMethod.IsPrivate)
                        {
                            continue;
                        }

                        LuaAPI.xlua_pushasciistring(L, prop.Name);
                        LuaAPI.lua_rawget(L, idx);
                        if (!LuaAPI.lua_isnil(L, -1))
                        {
                            try
                            {
                                prop.SetValue(obj, GetCaster(prop.PropertyType)(L, n + 1,
                                    target == null || prop.PropertyType.IsPrimitive() || prop.PropertyType == typeof(string) ? null : prop.GetValue(obj, null)), null);
                            }
                            catch (Exception e)
                            {
                                throw new Exception("exception in tran " + prop.Name + ", msg=" + e.Message);
                            }
                        }
                        LuaAPI.lua_pop(L, 1);
                    }*/
                    foreach (FieldInfo field in type.GetFields())
                    {
                        LuaAPI.xlua_pushasciistring(L, field.Name);
                        LuaAPI.lua_rawget(L, idx);
                        if (!LuaAPI.lua_isnil(L, -1))
                        {
                            try
                            {
                                field.SetValue(obj, GetCaster(field.FieldType)(L, n + 1,
                                        target == null || field.FieldType.IsPrimitive() || field.FieldType == typeof(string) ? null : field.GetValue(obj)));
                            }
                            catch (Exception e)
                            {
                                throw new Exception("exception in tran " + field.Name + ", msg=" + e.Message);
                            }
                        }
                        LuaAPI.lua_pop(L, 1);
                    }

                    return obj;
                };
            }
            else
            {
                return fixTypeGetter;
            }
        }

        ObjectCast genNullableCaster(ObjectCast oc)
        {
            return (RealStatePtr L, int idx, object target) =>
            {
                if (LuaAPI.lua_isnil(L, -1))
                {
                    return null;
                }
                else
                {
                    return oc(L, idx, target);
                }
            };
        }

        public ObjectCast GetCaster(Type type)
        {
            if (type.IsByRef) type = type.GetElementType();

            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                return genNullableCaster(GetCaster(underlyingType)); 
            }
            ObjectCast oc;
            if (!castersMap.TryGetValue(type, out oc))
            {
                oc = genCaster(type);
                castersMap.Add(type, oc);
            }
            return oc;
        }
    }
}
