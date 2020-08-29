// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:16:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\cgocallback.go

using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // These functions are called from C code via cgo/callbacks.go.

        // Panic.
        private static void _cgo_panic_internal(ref byte _p) => func(_p, (ref byte p, Defer _, Panic panic, Recover __) =>
        {
            panic(gostringnocopy(p));
        });
    }
}
