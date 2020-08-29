// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9
// +build !solaris
// +build !windows
// +build !nacl

// package runtime -- go2cs converted at 2020 August 29 08:21:01 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\stubs2.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static int read(int fd, unsafe.Pointer p, int n)
;
        private static int closefd(int fd)
;

        private static void exit(int code)
;
        private static long nanotime()
;
        private static void usleep(uint usec)
;

        //go:noescape
        private static int write(System.UIntPtr fd, unsafe.Pointer p, int n)
;

        //go:noescape
        private static int open(ref byte name, int mode, int perm)
;

        private static void madvise(unsafe.Pointer addr, System.UIntPtr n, int flags)
;

        // exitThread terminates the current thread, writing *wait = 0 when
        // the stack is safe to reclaim.
        //
        //go:noescape
        private static void exitThread(ref uint wait)
;
    }
}
