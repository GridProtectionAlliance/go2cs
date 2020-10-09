// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build 386 amd64 amd64p32
// +build !gccgo

// package cpu -- go2cs converted at 2020 October 09 06:07:54 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_gc_x86.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        // cpuid is implemented in cpu_x86.s for gc compiler
        // and in cpu_gccgo.c for gccgo.
        private static (uint, uint, uint, uint) cpuid(uint eaxArg, uint ecxArg)
;

        // xgetbv with ecx = 0 is implemented in cpu_x86.s for gc compiler
        // and in cpu_gccgo.c for gccgo.
        private static (uint, uint) xgetbv()
;
    }
}}}}}
