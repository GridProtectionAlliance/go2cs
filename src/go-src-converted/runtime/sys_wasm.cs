// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:10 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sys_wasm.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private partial struct m0Stack {
    public array<byte> _;
}

private static m0Stack wasmStack = default;

private static void wasmMove();

private static void wasmZero();

private static void wasmDiv();

private static void wasmTruncS();
private static void wasmTruncU();

private static void wasmExit(int code);

// adjust Gobuf as it if executed a call to fn with context ctxt
// and then stopped before the first instruction in fn.
private static void gostartcall(ptr<gobuf> _addr_buf, unsafe.Pointer fn, unsafe.Pointer ctxt) {
    ref gobuf buf = ref _addr_buf.val;

    var sp = buf.sp;
    sp -= sys.PtrSize * (uintptr.val)(@unsafe.Pointer(sp));

    buf.pc;
    buf.sp = sp;
    buf.pc = uintptr(fn);
    buf.ctxt = ctxt;

}

} // end runtime_package
