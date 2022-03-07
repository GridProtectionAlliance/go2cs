// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package robustio -- go2cs converted at 2022 March 06 23:18:38 UTC
// import "cmd/go/internal/robustio" ==> using robustio = go.cmd.go.@internal.robustio_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\robustio\robustio_darwin.go
using errors = go.errors_package;
using syscall = go.syscall_package;

namespace go.cmd.go.@internal;

public static partial class robustio_package {

private static readonly var errFileNotFound = syscall.ENOENT;

// isEphemeralError returns true if err may be resolved by waiting.


// isEphemeralError returns true if err may be resolved by waiting.
private static bool isEphemeralError(error err) {
    ref syscall.Errno errno = ref heap(out ptr<syscall.Errno> _addr_errno);
    if (errors.As(err, _addr_errno)) {
        return errno == errFileNotFound;
    }
    return false;

}

} // end robustio_package
