// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build windows
namespace go.@internal;

using syscall = syscall_package;

partial class poll_package {

// Do the interface allocations only once for common
// Errno values.
internal static syscall.Errno errERROR_IO_PENDING = ((syscall.Errno)syscall.ERROR_IO_PENDING);

// errnoErr returns common boxed Errno values, to prevent
// allocations at runtime.
internal static error errnoErr(syscall.Errno e) {
    var exprᴛ1 = e;
    if (exprᴛ1 == 0) {
        return default!;
    }
    if (exprᴛ1 == syscall.ERROR_IO_PENDING) {
        return errERROR_IO_PENDING;
    }

    // TODO: add more here, after collecting data on the common
    // error values see on Windows. (perhaps when running
    // all.bat?)
    return e;
}

} // end poll_package
