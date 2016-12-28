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

using System;

namespace XLua
{
    public static partial class CopyByValue
    {
        // for int 8
        public static bool Pack(IntPtr buff, int offset, byte field)
        {
            return LuaAPI.xlua_pack_int8_t(buff, offset, field);
        }
        public static bool UnPack(IntPtr buff, int offset, out byte field)
        {
            return LuaAPI.xlua_unpack_int8_t(buff, offset, out field);
        }
        public static bool Pack(IntPtr buff, int offset, sbyte field)
        {
            return LuaAPI.xlua_pack_int8_t(buff, offset, (byte)field);
        }
        public static bool UnPack(IntPtr buff, int offset, out sbyte field)
        {
            byte tfield;
            bool ret = LuaAPI.xlua_unpack_int8_t(buff, offset, out tfield);
            field = (sbyte)tfield;
            return ret;
        }
        // for int16
        public static bool Pack(IntPtr buff, int offset, short field)
        {
            return LuaAPI.xlua_pack_int16_t(buff, offset, field);
        }
        public static bool UnPack(IntPtr buff, int offset, out short field)
        {
            return LuaAPI.xlua_unpack_int16_t(buff, offset, out field);
        }
        public static bool Pack(IntPtr buff, int offset, ushort field)
        {
            return LuaAPI.xlua_pack_int16_t(buff, offset, (short)field);
        }
        public static bool UnPack(IntPtr buff, int offset, out ushort field)
        {
            short tfield;
            bool ret = LuaAPI.xlua_unpack_int16_t(buff, offset, out tfield);
            field = (ushort)tfield;
            return ret;
        }
        // for int32
        public static bool Pack(IntPtr buff, int offset, int field)
        {
            return LuaAPI.xlua_pack_int32_t(buff, offset, field);
        }
        public static bool UnPack(IntPtr buff, int offset, out int field)
        {
            return LuaAPI.xlua_unpack_int32_t(buff, offset, out field);
        }
        public static bool Pack(IntPtr buff, int offset, uint field)
        {
            return LuaAPI.xlua_pack_int32_t(buff, offset, (int)field);
        }
        public static bool UnPack(IntPtr buff, int offset, out uint field)
        {
            int tfield;
            bool ret = LuaAPI.xlua_unpack_int32_t(buff, offset, out tfield);
            field = (uint)tfield;
            return ret;
        }
        // for int64
        public static bool Pack(IntPtr buff, int offset, long field)
        {
            return LuaAPI.xlua_pack_int64_t(buff, offset, field);
        }
        public static bool UnPack(IntPtr buff, int offset, out long field)
        {
            return LuaAPI.xlua_unpack_int64_t(buff, offset, out field);
        }
        public static bool Pack(IntPtr buff, int offset, ulong field)
        {
            return LuaAPI.xlua_pack_int64_t(buff, offset, (long)field);
        }
        public static bool UnPack(IntPtr buff, int offset, out ulong field)
        {
            long tfield;
            bool ret = LuaAPI.xlua_unpack_int64_t(buff, offset, out tfield);
            field = (ulong)tfield;
            return ret;
        }
        // for float
        public static bool Pack(IntPtr buff, int offset, float field)
        {
            return LuaAPI.xlua_pack_float(buff, offset, field);
        }
        public static bool UnPack(IntPtr buff, int offset, out float field)
        {
            return LuaAPI.xlua_unpack_float(buff, offset, out field);
        }
        // for double
        public static bool Pack(IntPtr buff, int offset, double field)
        {
            return LuaAPI.xlua_pack_double(buff, offset, field);
        }
        public static bool UnPack(IntPtr buff, int offset, out double field)
        {
            return LuaAPI.xlua_unpack_double(buff, offset, out field);
        }
        // for decimal
        public static bool Pack(IntPtr buff, int offset, decimal field)
        {
            return LuaAPI.xlua_pack_decimal(buff, offset, ref field);
        }
        public static bool UnPack(IntPtr buff, int offset, out decimal field)
        {
            byte scale;
            byte sign;
            int hi32;
            ulong lo64;
            if (!LuaAPI.xlua_unpack_decimal(buff, offset, out scale, out sign, out hi32, out lo64))
            {
                field = default(decimal);
                return false;
            }

            field = new Decimal((int)(lo64 & 0xFFFFFFFF), (int)(lo64 >> 32), hi32, (sign & 0x80) != 0, scale);
            return true;
        }

        public static bool IsStruct(Type type)
        {
            return type.IsValueType && !type.IsEnum && !type.IsPrimitive;
        }

    }
}
