// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build linux
// +build ppc64 ppc64le

// package runtime -- go2cs converted at 2020 October 08 03:24:23 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\vdso_linux_ppc64x.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
 
        // vdsoArrayMax is the byte-size of a maximally sized array on this architecture.
        // See cmd/compile/internal/ppc64/galign.go arch.MAXWIDTH initialization.
        private static readonly long vdsoArrayMax = (long)1L << (int)(50L) - 1L;


        private static vdsoVersionKey vdsoLinuxVersion = new vdsoVersionKey("LINUX_2.6.15",0x75fcba5);

        private static vdsoSymbolKey vdsoSymbolKeys = new slice<vdsoSymbolKey>(new vdsoSymbolKey[] { {"__kernel_clock_gettime",0xb0cd725,0xdfa941fd,&vdsoClockgettimeSym} });

        // initialize with vsyscall fallbacks
        private static System.UIntPtr vdsoClockgettimeSym = 0L;
    }
}
