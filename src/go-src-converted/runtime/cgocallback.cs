// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:21 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\cgocallback.go


namespace go;

public static partial class runtime_package {

    // These functions are called from C code via cgo/callbacks.go.

    // Panic.
private static void _cgo_panic_internal(ptr<byte> _addr_p) => func((_, panic, _) => {
    ref byte p = ref _addr_p.val;

    panic(gostringnocopy(p));
});

} // end runtime_package
