// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package cpu -- go2cs converted at 2020 October 09 06:07:53 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\byteorder.go
using runtime = go.runtime_package;
using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        // byteOrder is a subset of encoding/binary.ByteOrder.
        private partial interface byteOrder
        {
            ulong Uint32(slice<byte> _p0);
            ulong Uint64(slice<byte> _p0);
        }

        private partial struct littleEndian
        {
        }
        private partial struct bigEndian
        {
        }

        private static uint Uint32(this littleEndian _p0, slice<byte> b)
        {
            _ = b[3L]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint32(b[0L]) | uint32(b[1L]) << (int)(8L) | uint32(b[2L]) << (int)(16L) | uint32(b[3L]) << (int)(24L);

        }

        private static ulong Uint64(this littleEndian _p0, slice<byte> b)
        {
            _ = b[7L]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[0L]) | uint64(b[1L]) << (int)(8L) | uint64(b[2L]) << (int)(16L) | uint64(b[3L]) << (int)(24L) | uint64(b[4L]) << (int)(32L) | uint64(b[5L]) << (int)(40L) | uint64(b[6L]) << (int)(48L) | uint64(b[7L]) << (int)(56L);

        }

        private static uint Uint32(this bigEndian _p0, slice<byte> b)
        {
            _ = b[3L]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint32(b[3L]) | uint32(b[2L]) << (int)(8L) | uint32(b[1L]) << (int)(16L) | uint32(b[0L]) << (int)(24L);

        }

        private static ulong Uint64(this bigEndian _p0, slice<byte> b)
        {
            _ = b[7L]; // bounds check hint to compiler; see golang.org/issue/14808
            return uint64(b[7L]) | uint64(b[6L]) << (int)(8L) | uint64(b[5L]) << (int)(16L) | uint64(b[4L]) << (int)(24L) | uint64(b[3L]) << (int)(32L) | uint64(b[2L]) << (int)(40L) | uint64(b[1L]) << (int)(48L) | uint64(b[0L]) << (int)(56L);

        }

        // hostByteOrder returns binary.LittleEndian on little-endian machines and
        // binary.BigEndian on big-endian machines.
        private static byteOrder hostByteOrder() => func((_, panic, __) =>
        {
            switch (runtime.GOARCH)
            {
                case "386": 

                case "amd64": 

                case "amd64p32": 

                case "arm": 

                case "arm64": 

                case "mipsle": 

                case "mips64le": 

                case "mips64p32le": 

                case "ppc64le": 

                case "riscv": 

                case "riscv64": 
                    return new littleEndian();
                    break;
                case "armbe": 

                case "arm64be": 

                case "mips": 

                case "mips64": 

                case "mips64p32": 

                case "ppc": 

                case "ppc64": 

                case "s390": 

                case "s390x": 

                case "sparc": 

                case "sparc64": 
                    return new bigEndian();
                    break;
            }
            panic("unknown architecture");

        });
    }
}}}}}
