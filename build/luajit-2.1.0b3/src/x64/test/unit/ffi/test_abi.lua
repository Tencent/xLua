local ffi = require "ffi"

-- TODO: test "gc64" and "win" parameters
assert((ffi.abi("32bit") or ffi.abi("64bit"))
        and ffi.abi("le")
        and not ffi.abi("be")
        and ffi.abi("fpu")
        and not ffi.abi("softfp")
        and ffi.abi("hardfp")
        and not ffi.abi("eabi"))
