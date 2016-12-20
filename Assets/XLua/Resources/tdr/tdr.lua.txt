-- Tencent is pleased to support the open source community by making xLua available.
-- Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
-- Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
-- http://opensource.org/licenses/MIT
-- Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

require "libtdrlua"
local m = {}
for k, v in pairs(libtdrlua) do m[k] = v end
local load_metalib, load_metalib_buf, free_metalib, get_meta, table2buf, buf2table, str2table, metamaxbufsize, bufalloc, buffree, buf2str
load_metalib, m.load_metalib = m.load_metalib, nil
load_metalib_buf, m.load_metalib_buf = m.load_metalib_buf, nil
free_metalib, m.free_metalib = m.free_metalib, nil
get_meta, m.get_meta = m.get_meta, nil
table2buf, m.table2buf = m.table2buf, nil
buf2table, m.buf2table = m.buf2table, nil
str2table, m.str2table = m.str2table, nil
buf2str, m.buf2str = m.buf2str, nil

metamaxbufsize, m.metamaxbufsize = m.metamaxbufsize, nil
bufalloc, m.bufalloc = m.bufalloc, nil
buffree, m.buffree = m.buffree, nil

local function create_msg_pk(meta, buf, buf_size)
    return {
        buff = buf,
        pack = function(obj)
            local ret_code, used_size = table2buf(meta, obj, buf, buf_size, 0)
            if ret_code ~= 0 then
                return ret_code, used_size
            end
            return buf2str(buf, used_size)
        end,
        unpack = function(str)
            return libtdrlua.str2table(meta, str, 0)
        end
    }
end

local function create_lib(metalib)
    return setmetatable({}, {
        __index = function(obj, k)
            local ret_code, meta = libtdrlua.get_meta(metalib, k)
            if ret_code ~= 0 then
                error("libtdrlua.get_meta() failed: errno=".. ret_code .. ",msg=" .. meta)
            end
            local ret_code, buf_size = libtdrlua.metamaxbufsize(metalib, k)
            if ret_code ~= 0 then
                error("libtdrlua.metamaxbufsize() failed: errno=".. ret_code .. ",msg=" .. buf_size)
            end
            
            local ret_code, buf = libtdrlua.bufalloc(buf_size)
            if ret_code ~= 0 then
                error("libtdrlua.bufalloc() failed: errno=".. ret_code .. ",msg=" .. buf)
            end
    
            local pk = create_msg_pk(meta, buf, buf_size)
            rawset(obj, k, pk)
            return pk
        end
    })
end

function m.from_file(file)
    local ret_code, metalib = libtdrlua.load_metalib(file)
    if ret_code ~= 0 then
        error("libtdrlua.load_metalib() failed: " .. metalib)
    end
    return create_lib(metalib)
end

function m.from_memory(str)
    local ret_code, metalib = libtdrlua.load_metalib_buf(str)
    if ret_code ~= 0 then
        error("libtdrlua.load_metalib_buf() failed: errno=".. ret_code .. ",msg=" .. metalib)
    end
    return create_lib(metalib)
end

return m