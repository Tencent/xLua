local ffi = require "ffi"

ffi.cdef[[
typedef struct { int a; char b; } __attribute__((packed)) myty1;
typedef struct { int a; char b; } __attribute__((__packed__)) myty1_a;

typedef struct { int a; char b; } __attribute__((aligned(16))) myty2_a;
typedef struct { int a; char b; } __attribute__((__aligned__(16))) myty2;

typedef int __attribute__ ((vector_size (32))) myty3;
typedef int __attribute__ ((__vector_size__ (32))) myty3_a;

typedef int __attribute__ ((mode(DI))) myty4;
]]

assert(ffi.sizeof("myty1") == 5 and
       ffi.sizeof("myty1_a") == 5 and
       ffi.alignof("myty2") == 16 and
       ffi.alignof("myty2_a") == 16 and
       ffi.sizeof("myty3") == 32 and
       ffi.sizeof("myty3_a") == 32 and
       ffi.sizeof("myty4") == 8)
