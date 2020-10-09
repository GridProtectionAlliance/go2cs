// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build 386 amd64 amd64p32
// +build gccgo

// package cpu -- go2cs converted at 2020 October 09 06:07:54 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_gccgo_x86.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        //extern gccgoGetCpuidCount
        private static void gccgoGetCpuidCount(uint eaxArg, uint ecxArg, ptr<uint> eax, ptr<uint> ebx, ptr<uint> ecx, ptr<uint> edx)
;

        private static (uint, uint, uint, uint) cpuid(uint eaxArg, uint ecxArg)
        {
            uint eax = default;
            uint ebx = default;
            uint ecx = default;
            uint edx = default;

            ref uint a = ref heap(out ptr<uint> _addr_a);            ref uint b = ref heap(out ptr<uint> _addr_b);            ref uint c = ref heap(out ptr<uint> _addr_c);            ref uint d = ref heap(out ptr<uint> _addr_d);

            gccgoGetCpuidCount(eaxArg, ecxArg, _addr_a, _addr_b, _addr_c, _addr_d);
            return (a, b, c, d);
        }

        //extern gccgoXgetbv
        private static void gccgoXgetbv(ptr<uint> eax, ptr<uint> edx)
;

        private static (uint, uint) xgetbv()
        {
            uint eax = default;
            uint edx = default;

            ref uint a = ref heap(out ptr<uint> _addr_a);            ref uint d = ref heap(out ptr<uint> _addr_d);

            gccgoXgetbv(_addr_a, _addr_d);
            return (a, d);
        }
    }
}}}}}
