// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build wasm

// package cpu -- go2cs converted at 2020 October 09 06:07:55 UTC
// import "vendor/golang.org/x/sys/cpu" ==> using cpu = go.vendor.golang.org.x.sys.cpu_package
// Original source: C:\Go\src\vendor\golang.org\x\sys\cpu\cpu_wasm.go

using static go.builtin;

namespace go {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class cpu_package
    {
        // We're compiling the cpu package for an unknown (software-abstracted) CPU.
        // Make CacheLinePad an empty struct and hope that the usual struct alignment
        // rules are good enough.
        private static readonly long cacheLineSize = (long)0L;

    }
}}}}}
