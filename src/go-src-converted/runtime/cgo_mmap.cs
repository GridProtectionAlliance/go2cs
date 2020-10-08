// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Support for memory sanitizer. See runtime/cgo/mmap.go.

// +build linux,amd64 linux,arm64

// package runtime -- go2cs converted at 2020 October 08 03:19:13 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\cgo_mmap.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // _cgo_mmap is filled in by runtime/cgo when it is linked into the
        // program, so it is only non-nil when using cgo.
        //go:linkname _cgo_mmap _cgo_mmap
        private static unsafe.Pointer _cgo_mmap = default;

        // _cgo_munmap is filled in by runtime/cgo when it is linked into the
        // program, so it is only non-nil when using cgo.
        //go:linkname _cgo_munmap _cgo_munmap
        private static unsafe.Pointer _cgo_munmap = default;

        // mmap is used to route the mmap system call through C code when using cgo, to
        // support sanitizer interceptors. Don't allow stack splits, since this function
        // (used by sysAlloc) is called in a lot of low-level parts of the runtime and
        // callers often assume it won't acquire any locks.
        //go:nosplit
        private static (unsafe.Pointer, long) mmap(unsafe.Pointer addr, System.UIntPtr n, int prot, int flags, int fd, uint off)
        {
            unsafe.Pointer _p0 = default;
            long _p0 = default;

            if (_cgo_mmap != null)
            { 
                // Make ret a uintptr so that writing to it in the
                // function literal does not trigger a write barrier.
                // A write barrier here could break because of the way
                // that mmap uses the same value both as a pointer and
                // an errno value.
                System.UIntPtr ret = default;
                systemstack(() =>
                {
                    ret = callCgoMmap(addr, n, prot, flags, fd, off);
                });
                if (ret < 4096L)
                {
                    return (null, int(ret));
                }

                return (@unsafe.Pointer(ret), 0L);

            }

            return sysMmap(addr, n, prot, flags, fd, off);

        }

        private static void munmap(unsafe.Pointer addr, System.UIntPtr n)
        {
            if (_cgo_munmap != null)
            {
                systemstack(() =>
                {
                    callCgoMunmap(addr, n);
                });
                return ;

            }

            sysMunmap(addr, n);

        }

        // sysMmap calls the mmap system call. It is implemented in assembly.
        private static (unsafe.Pointer, long) sysMmap(unsafe.Pointer addr, System.UIntPtr n, int prot, int flags, int fd, uint off)
;

        // callCgoMmap calls the mmap function in the runtime/cgo package
        // using the GCC calling convention. It is implemented in assembly.
        private static System.UIntPtr callCgoMmap(unsafe.Pointer addr, System.UIntPtr n, int prot, int flags, int fd, uint off)
;

        // sysMunmap calls the munmap system call. It is implemented in assembly.
        private static void sysMunmap(unsafe.Pointer addr, System.UIntPtr n)
;

        // callCgoMunmap calls the munmap function in the runtime/cgo package
        // using the GCC calling convention. It is implemented in assembly.
        private static void callCgoMunmap(unsafe.Pointer addr, System.UIntPtr n)
;
    }
}
