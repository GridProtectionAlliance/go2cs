// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build openbsd && !mips64
// +build openbsd,!mips64

// package runtime -- go2cs converted at 2022 March 06 22:12:09 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sys_openbsd.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // The *_trampoline functions convert from the Go calling convention to the C calling convention
    // and then call the underlying libc function. These are defined in sys_openbsd_$ARCH.s.

    //go:nosplit
    //go:cgo_unsafe_args
private static int pthread_attr_init(ptr<pthreadattr> _addr_attr) {
    ref pthreadattr attr = ref _addr_attr.val;

    return libcCall(@unsafe.Pointer(funcPC(pthread_attr_init_trampoline)), @unsafe.Pointer(_addr_attr));
}
private static void pthread_attr_init_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_attr_destroy(ptr<pthreadattr> _addr_attr) {
    ref pthreadattr attr = ref _addr_attr.val;

    return libcCall(@unsafe.Pointer(funcPC(pthread_attr_destroy_trampoline)), @unsafe.Pointer(_addr_attr));
}
private static void pthread_attr_destroy_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_attr_getstacksize(ptr<pthreadattr> _addr_attr, ptr<System.UIntPtr> _addr_size) {
    ref pthreadattr attr = ref _addr_attr.val;
    ref System.UIntPtr size = ref _addr_size.val;

    return libcCall(@unsafe.Pointer(funcPC(pthread_attr_getstacksize_trampoline)), @unsafe.Pointer(_addr_attr));
}
private static void pthread_attr_getstacksize_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_attr_setdetachstate(ptr<pthreadattr> _addr_attr, nint state) {
    ref pthreadattr attr = ref _addr_attr.val;

    return libcCall(@unsafe.Pointer(funcPC(pthread_attr_setdetachstate_trampoline)), @unsafe.Pointer(_addr_attr));
}
private static void pthread_attr_setdetachstate_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_create(ptr<pthreadattr> _addr_attr, System.UIntPtr start, unsafe.Pointer arg) {
    ref pthreadattr attr = ref _addr_attr.val;

    return libcCall(@unsafe.Pointer(funcPC(pthread_create_trampoline)), @unsafe.Pointer(_addr_attr));
}
private static void pthread_create_trampoline();

// Tell the linker that the libc_* functions are to be found
// in a system library, with the libc_ prefix missing.

//go:cgo_import_dynamic libc_pthread_attr_init pthread_attr_init "libpthread.so"
//go:cgo_import_dynamic libc_pthread_attr_destroy pthread_attr_destroy "libpthread.so"
//go:cgo_import_dynamic libc_pthread_attr_getstacksize pthread_attr_getstacksize "libpthread.so"
//go:cgo_import_dynamic libc_pthread_attr_setdetachstate pthread_attr_setdetachstate "libpthread.so"
//go:cgo_import_dynamic libc_pthread_create pthread_create "libpthread.so"
//go:cgo_import_dynamic libc_pthread_sigmask pthread_sigmask "libpthread.so"

//go:cgo_import_dynamic _ _ "libpthread.so"
//go:cgo_import_dynamic _ _ "libc.so"

} // end runtime_package
