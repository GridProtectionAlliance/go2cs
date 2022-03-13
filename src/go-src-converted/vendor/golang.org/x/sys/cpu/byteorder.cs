// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2022 March 13 06:46:34 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Program Files\Go\src\vendor\golang.org\x\sys\cpu\byteorder.go
namespace go.vendor.golang.org.x.sys;

using runtime = runtime_package;


// byteOrder is a subset of encoding/binary.ByteOrder.

public static partial class cpu_package {

private partial interface byteOrder {
    ulong Uint32(slice<byte> _p0);
    ulong Uint64(slice<byte> _p0);
}

private partial struct littleEndian {
}
private partial struct bigEndian {
}

private static uint Uint32(this littleEndian _p0, slice<byte> b) {
    _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
    return uint32(b[0]) | uint32(b[1]) << 8 | uint32(b[2]) << 16 | uint32(b[3]) << 24;
}

private static ulong Uint64(this littleEndian _p0, slice<byte> b) {
    _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
    return uint64(b[0]) | uint64(b[1]) << 8 | uint64(b[2]) << 16 | uint64(b[3]) << 24 | uint64(b[4]) << 32 | uint64(b[5]) << 40 | uint64(b[6]) << 48 | uint64(b[7]) << 56;
}

private static uint Uint32(this bigEndian _p0, slice<byte> b) {
    _ = b[3]; // bounds check hint to compiler; see golang.org/issue/14808
    return uint32(b[3]) | uint32(b[2]) << 8 | uint32(b[1]) << 16 | uint32(b[0]) << 24;
}

private static ulong Uint64(this bigEndian _p0, slice<byte> b) {
    _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
    return uint64(b[7]) | uint64(b[6]) << 8 | uint64(b[5]) << 16 | uint64(b[4]) << 24 | uint64(b[3]) << 32 | uint64(b[2]) << 40 | uint64(b[1]) << 48 | uint64(b[0]) << 56;
}

// hostByteOrder returns littleEndian on little-endian machines and
// bigEndian on big-endian machines.
private static byteOrder hostByteOrder() => func((_, panic, _) => {
    switch (runtime.GOARCH) {
        case "386": 

        case "amd64": 

        case "amd64p32": 

        case "alpha": 

        case "arm": 

        case "arm64": 

        case "mipsle": 

        case "mips64le": 

        case "mips64p32le": 

        case "nios2": 

        case "ppc64le": 

        case "riscv": 

        case "riscv64": 

        case "sh": 
            return new littleEndian();
            break;
        case "armbe": 

        case "arm64be": 

        case "m68k": 

        case "mips": 

        case "mips64": 

        case "mips64p32": 

        case "ppc": 

        case "ppc64": 

        case "s390": 

        case "s390x": 

        case "shbe": 

        case "sparc": 

        case "sparc64": 
            return new bigEndian();
            break;
    }
    panic("unknown architecture");
});

} // end cpu_package
