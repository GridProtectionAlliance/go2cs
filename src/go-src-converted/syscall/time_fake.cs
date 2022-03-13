// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build faketime
// +build faketime

// package syscall -- go2cs converted at 2022 March 13 05:40:38 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\time_fake.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class syscall_package {

private static readonly var faketime = true;

// When faketime is enabled, we redirect writes to FDs 1 and 2 through
// the runtime's write function, since that adds the framing that
// reports the emulated time.

//go:linkname runtimeWrite runtime.write


// When faketime is enabled, we redirect writes to FDs 1 and 2 through
// the runtime's write function, since that adds the framing that
// reports the emulated time.

//go:linkname runtimeWrite runtime.write
private static int runtimeWrite(System.UIntPtr fd, unsafe.Pointer p, int n);

private static nint faketimeWrite(nint fd, slice<byte> p) {
    ptr<byte> pp;
    if (len(p) > 0) {>>MARKER:FUNCTION_runtimeWrite_BLOCK_PREFIX<<
        pp = _addr_p[0];
    }
    return int(runtimeWrite(uintptr(fd), @unsafe.Pointer(pp), int32(len(p))));
}

} // end syscall_package
