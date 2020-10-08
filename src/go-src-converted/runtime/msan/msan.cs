// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build msan,linux
// +build amd64 arm64

// package msan -- go2cs converted at 2020 October 08 03:24:28 UTC
// import "runtime/msan" ==> using msan = go.runtime.msan_package
// Original source: C:\Go\src\runtime\msan\msan.go
/*
#cgo CFLAGS: -fsanitize=memory
#cgo LDFLAGS: -fsanitize=memory

#include <stdint.h>
#include <sanitizer/msan_interface.h>

void __msan_read_go(void *addr, uintptr_t sz) {
    __msan_check_mem_is_initialized(addr, sz);
}

void __msan_write_go(void *addr, uintptr_t sz) {
    __msan_unpoison(addr, sz);
}

void __msan_malloc_go(void *addr, uintptr_t sz) {
    __msan_unpoison(addr, sz);
}

void __msan_free_go(void *addr, uintptr_t sz) {
    __msan_poison(addr, sz);
}
*/
using C = go.C_package;    }

