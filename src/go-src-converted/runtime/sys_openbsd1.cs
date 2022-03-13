// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build openbsd && !mips64
// +build openbsd,!mips64

// package runtime -- go2cs converted at 2022 March 13 05:27:14 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sys_openbsd1.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class runtime_package {

//go:nosplit
//go:cgo_unsafe_args
private static int thrsleep(System.UIntPtr ident, int clock_id, ptr<timespec> _addr_tsp, System.UIntPtr @lock, ptr<uint> _addr_abort) {
    ref timespec tsp = ref _addr_tsp.val;
    ref uint abort = ref _addr_abort.val;

    return libcCall(@unsafe.Pointer(funcPC(thrsleep_trampoline)), @unsafe.Pointer(_addr_ident));
}
private static void thrsleep_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int thrwakeup(System.UIntPtr ident, int n) {
    return libcCall(@unsafe.Pointer(funcPC(thrwakeup_trampoline)), @unsafe.Pointer(_addr_ident));
}
private static void thrwakeup_trampoline();

//go:nosplit
private static void osyield() {
    libcCall(@unsafe.Pointer(funcPC(sched_yield_trampoline)), @unsafe.Pointer(null));
}
private static void sched_yield_trampoline();

//go:nosplit
private static void osyield_no_g() {
    asmcgocall_no_g(@unsafe.Pointer(funcPC(sched_yield_trampoline)), @unsafe.Pointer(null));
}

//go:cgo_import_dynamic libc_thrsleep __thrsleep "libc.so"
//go:cgo_import_dynamic libc_thrwakeup __thrwakeup "libc.so"
//go:cgo_import_dynamic libc_sched_yield sched_yield "libc.so"

//go:cgo_import_dynamic _ _ "libc.so"

} // end runtime_package
