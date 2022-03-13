// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 || 386
// +build amd64 386

// package runtime -- go2cs converted at 2022 March 13 05:27:15 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sys_x86.go
namespace go;

using sys = runtime.@internal.sys_package;
using @unsafe = @unsafe_package;


// adjust Gobuf as if it executed a call to fn with context ctxt
// and then stopped before the first instruction in fn.

public static partial class runtime_package {

private static void gostartcall(ptr<gobuf> _addr_buf, unsafe.Pointer fn, unsafe.Pointer ctxt) {
    ref gobuf buf = ref _addr_buf.val;

    var sp = buf.sp;
    sp -= sys.PtrSize * (uintptr.val);

    (@unsafe.Pointer(sp)) = buf.pc;
    buf.sp = sp;
    buf.pc = uintptr(fn);
    buf.ctxt = ctxt;
}

} // end runtime_package
