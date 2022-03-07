// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !aix && !darwin && !js && (!linux || !amd64) && (!linux || !arm64) && !openbsd && !plan9 && !solaris && !windows
// +build !aix
// +build !darwin
// +build !js
// +build !linux !amd64
// +build !linux !arm64
// +build !openbsd
// +build !plan9
// +build !solaris
// +build !windows

// package runtime -- go2cs converted at 2022 March 06 22:09:56 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mmap.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // mmap calls the mmap system call. It is implemented in assembly.
    // We only pass the lower 32 bits of file offset to the
    // assembly routine; the higher bits (if required), should be provided
    // by the assembly routine as 0.
    // The err result is an OS error code such as ENOMEM.
private static (unsafe.Pointer, nint) mmap(unsafe.Pointer addr, System.UIntPtr n, int prot, int flags, int fd, uint off);

// munmap calls the munmap system call. It is implemented in assembly.
private static void munmap(unsafe.Pointer addr, System.UIntPtr n);

} // end runtime_package
