local x = [=[
local ffi = require "ffi"

ffi.cdef [[
    #line 100
    typedef Int xxx
]]
]=]

local function foo()
    loadstring(x)()
end

local r, e = pcall(foo)
assert(string.find(e, "declaration specifier expected near 'Int' at line 100") ~= nil)
