// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build msan

// package runtime -- go2cs converted at 2020 October 08 03:21:28 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\msan.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Public memory sanitizer API.
        public static void MSanRead(unsafe.Pointer addr, long len)
        {
            msanread(addr, uintptr(len));
        }

        public static void MSanWrite(unsafe.Pointer addr, long len)
        {
            msanwrite(addr, uintptr(len));
        }

        // Private interface for the runtime.
        private static readonly var msanenabled = (var)true;

        // If we are running on the system stack, the C program may have
        // marked part of that stack as uninitialized. We don't instrument
        // the runtime, but operations like a slice copy can call msanread
        // anyhow for values on the stack. Just ignore msanread when running
        // on the system stack. The other msan functions are fine.
        //
        //go:nosplit


        // If we are running on the system stack, the C program may have
        // marked part of that stack as uninitialized. We don't instrument
        // the runtime, but operations like a slice copy can call msanread
        // anyhow for values on the stack. Just ignore msanread when running
        // on the system stack. The other msan functions are fine.
        //
        //go:nosplit
        private static void msanread(unsafe.Pointer addr, System.UIntPtr sz)
        {
            var g = getg();
            if (g == null || g.m == null || g == g.m.g0 || g == g.m.gsignal)
            {
                return ;
            }

            domsanread(addr, sz);

        }

        //go:noescape
        private static void domsanread(unsafe.Pointer addr, System.UIntPtr sz)
;

        //go:noescape
        private static void msanwrite(unsafe.Pointer addr, System.UIntPtr sz)
;

        //go:noescape
        private static void msanmalloc(unsafe.Pointer addr, System.UIntPtr sz)
;

        //go:noescape
        private static void msanfree(unsafe.Pointer addr, System.UIntPtr sz)
;

        // These are called from msan_amd64.s
        //go:cgo_import_static __msan_read_go
        //go:cgo_import_static __msan_write_go
        //go:cgo_import_static __msan_malloc_go
        //go:cgo_import_static __msan_free_go
    }
}
