// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:06 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_solaris.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private partial struct mts
        {
            public long tv_sec;
            public long tv_nsec;
        }

        private partial struct mscratch
        {
            public array<System.UIntPtr> v;
        }

        private partial struct mOS
        {
            public System.UIntPtr waitsema; // semaphore for parking on locks
            public ptr<int> perrno; // pointer to tls errno
// these are here because they are too large to be on the stack
// of low-level NOSPLIT functions.
//LibCall       libcall;
            public mts ts;
            public mscratch scratch;
        }

        private partial struct libcFunc // : System.UIntPtr
        {
        }

        private static libcFunc asmsysvicall6 = default;

        //go:nosplit
        private static System.UIntPtr sysvicall0(ref libcFunc fn)
        {
            libcall libcall = default;
            libcall.fn = uintptr(@unsafe.Pointer(fn));
            libcall.n = 0L;
            libcall.args = uintptr(@unsafe.Pointer(fn)); // it's unused but must be non-nil, otherwise crashes
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref libcall));
            return libcall.r1;
        }

        //go:nosplit
        private static System.UIntPtr sysvicall1(ref libcFunc fn, System.UIntPtr a1)
        {
            libcall libcall = default;
            libcall.fn = uintptr(@unsafe.Pointer(fn));
            libcall.n = 1L; 
            // TODO(rsc): Why is noescape necessary here and below?
            libcall.args = uintptr(noescape(@unsafe.Pointer(ref a1)));
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref libcall));
            return libcall.r1;
        }

        //go:nosplit
        private static System.UIntPtr sysvicall2(ref libcFunc fn, System.UIntPtr a1, System.UIntPtr a2)
        {
            libcall libcall = default;
            libcall.fn = uintptr(@unsafe.Pointer(fn));
            libcall.n = 2L;
            libcall.args = uintptr(noescape(@unsafe.Pointer(ref a1)));
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref libcall));
            return libcall.r1;
        }

        //go:nosplit
        private static System.UIntPtr sysvicall3(ref libcFunc fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
        {
            libcall libcall = default;
            libcall.fn = uintptr(@unsafe.Pointer(fn));
            libcall.n = 3L;
            libcall.args = uintptr(noescape(@unsafe.Pointer(ref a1)));
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref libcall));
            return libcall.r1;
        }

        //go:nosplit
        private static System.UIntPtr sysvicall4(ref libcFunc fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4)
        {
            libcall libcall = default;
            libcall.fn = uintptr(@unsafe.Pointer(fn));
            libcall.n = 4L;
            libcall.args = uintptr(noescape(@unsafe.Pointer(ref a1)));
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref libcall));
            return libcall.r1;
        }

        //go:nosplit
        private static System.UIntPtr sysvicall5(ref libcFunc fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5)
        {
            libcall libcall = default;
            libcall.fn = uintptr(@unsafe.Pointer(fn));
            libcall.n = 5L;
            libcall.args = uintptr(noescape(@unsafe.Pointer(ref a1)));
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref libcall));
            return libcall.r1;
        }

        //go:nosplit
        private static System.UIntPtr sysvicall6(ref libcFunc fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6)
        {
            libcall libcall = default;
            libcall.fn = uintptr(@unsafe.Pointer(fn));
            libcall.n = 6L;
            libcall.args = uintptr(noescape(@unsafe.Pointer(ref a1)));
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref libcall));
            return libcall.r1;
        }
    }
}
