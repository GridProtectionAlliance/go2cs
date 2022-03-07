// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:26:47 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\sock_cloexec_linux.go


namespace go;

public static partial class syscall_package {

    // This is a stripped down version of sysSocket from net/sock_cloexec.go.
private static (nint, error) cloexecSocket(nint family, nint sotype, nint proto) {
    nint _p0 = default;
    error _p0 = default!;

    var (s, err) = Socket(family, sotype | SOCK_CLOEXEC, proto);

    if (err == null) 
        return (s, error.As(null!)!);
    else if (err == EINVAL)     else 
        return (-1, error.As(err)!);
        ForkLock.RLock();
    s, err = Socket(family, sotype, proto);
    if (err == null) {
        CloseOnExec(s);
    }
    ForkLock.RUnlock();
    if (err != null) {
        Close(s);
        return (-1, error.As(err)!);
    }
    return (s, error.As(null!)!);

}

} // end syscall_package
