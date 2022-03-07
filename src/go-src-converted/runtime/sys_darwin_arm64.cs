// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:08 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sys_darwin_arm64.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // libc function wrappers. Must run on system stack.

    //go:nosplit
    //go:cgo_unsafe_args
private static int g0_pthread_key_create(ptr<pthreadkey> _addr_k, System.UIntPtr destructor) {
    ref pthreadkey k = ref _addr_k.val;

    return asmcgocall(@unsafe.Pointer(funcPC(pthread_key_create_trampoline)), @unsafe.Pointer(_addr_k));
}
private static void pthread_key_create_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int g0_pthread_setspecific(pthreadkey k, System.UIntPtr value) {
    return asmcgocall(@unsafe.Pointer(funcPC(pthread_setspecific_trampoline)), @unsafe.Pointer(_addr_k));
}
private static void pthread_setspecific_trampoline();

//go:cgo_import_dynamic libc_pthread_key_create pthread_key_create "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_setspecific pthread_setspecific "/usr/lib/libSystem.B.dylib"

// tlsinit allocates a thread-local storage slot for g.
//
// It finds the first available slot using pthread_key_create and uses
// it as the offset value for runtime.tlsg.
//
// This runs at startup on g0 stack, but before g is set, so it must
// not split stack (transitively). g is expected to be nil, so things
// (e.g. asmcgocall) will skip saving or reading g.
//
//go:nosplit
private static void tlsinit(ptr<System.UIntPtr> _addr_tlsg, ptr<array<System.UIntPtr>> _addr_tlsbase) {
    ref System.UIntPtr tlsg = ref _addr_tlsg.val;
    ref array<System.UIntPtr> tlsbase = ref _addr_tlsbase.val;

    ref pthreadkey k = ref heap(out ptr<pthreadkey> _addr_k);
    var err = g0_pthread_key_create(_addr_k, 0);
    if (err != 0) {>>MARKER:FUNCTION_pthread_setspecific_trampoline_BLOCK_PREFIX<<
        abort();
    }
    const nuint magic = 0xc476c475c47957;

    err = g0_pthread_setspecific(k, magic);
    if (err != 0) {>>MARKER:FUNCTION_pthread_key_create_trampoline_BLOCK_PREFIX<<
        abort();
    }
    foreach (var (i, x) in tlsbase) {
        if (x == magic) {
            tlsg = uintptr(i * sys.PtrSize);
            g0_pthread_setspecific(k, 0);
            return ;
        }
    }    abort();

}

} // end runtime_package
