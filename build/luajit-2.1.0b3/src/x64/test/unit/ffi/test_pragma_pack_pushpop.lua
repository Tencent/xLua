local ffi = require "ffi"

ffi.cdef[[
#pragma pack(push, 1)
typedef struct {
    char x;
    double y;
} foo;
#pragma pack(pop)
]]

assert(ffi.sizeof("foo") == 9)
