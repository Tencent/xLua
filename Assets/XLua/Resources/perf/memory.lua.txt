-- Tencent is pleased to support the open source community by making xLua available.
-- Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
-- Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
-- http://opensource.org/licenses/MIT
-- Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

local TYPE_NAME = {'GLOBAL', 'REGISTRY', 'UPVALUE', 'LOCAL'}

local function report_output_line(rp)
    local size = string.format("%7i", rp.size)
    return string.format("%-40.40s: %-12s: %-12s: %-12s: %s\n", rp.name, size, TYPE_NAME[rp.type], rp.pointer, rp.used_in)
end

local function snapshot()
    local ss = perf.snapshot()
    
    local FORMAT_HEADER_LINE       = "%-40s: %-12s: %-12s: %-12s: %s\n"
    local header = string.format( FORMAT_HEADER_LINE, "NAME", "SIZE", "TYPE", "ID", "INFO")
    table.sort(ss, function(a, b) return a.size > b.size end)
    
    local output = header
    
    for i, rp in ipairs(ss) do
        output = output .. report_output_line(rp)
    end
    
    return output
end

--returns the total memory in use by Lua (in Kbytes).
local function total()
    return collectgarbage('count')
end


return {
    snapshot = snapshot,
    total = total
}
