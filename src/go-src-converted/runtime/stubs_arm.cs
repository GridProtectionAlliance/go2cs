// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:48:47 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\stubs_arm.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Called from compiler-generated code; declared for go vet.
        private static void udiv()
;
        private static void _div()
;
        private static void _divu()
;
        private static void _mod()
;
        private static void _modu()
;

        // Called from assembly only; declared for go vet.
        private static void usplitR0()
;
        private static void load_g()
;
        private static void save_g()
;
        private static void emptyfunc()
;
        private static void _initcgo()
;
        private static void read_tls_fallback()
;
    }
}
