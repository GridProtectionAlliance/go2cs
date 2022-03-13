// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !aix && !darwin && !js && !openbsd && !plan9 && !solaris && !windows
// +build !aix,!darwin,!js,!openbsd,!plan9,!solaris,!windows

// package runtime -- go2cs converted at 2022 March 13 05:27:11 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\stubs2.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class runtime_package {

// read calls the read system call.
// It returns a non-negative number of bytes written or a negative errno value.
private static int read(int fd, unsafe.Pointer p, int n);

private static int closefd(int fd);

private static void exit(int code);
private static void usleep(uint usec);

//go:nosplit
private static void usleep_no_g(uint usec) {
    usleep(usec);
}

// write calls the write system call.
// It returns a non-negative number of bytes written or a negative errno value.
//go:noescape
private static int write1(System.UIntPtr fd, unsafe.Pointer p, int n);

//go:noescape
private static int open(ptr<byte> name, int mode, int perm);

// return value is only set on linux to be used in osinit()
private static int madvise(unsafe.Pointer addr, System.UIntPtr n, int flags);

// exitThread terminates the current thread, writing *wait = 0 when
// the stack is safe to reclaim.
//
//go:noescape
private static void exitThread(ptr<uint> wait);

} // end runtime_package
