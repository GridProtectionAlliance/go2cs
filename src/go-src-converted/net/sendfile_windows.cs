// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:32 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\sendfile_windows.go
using poll = go.@internal.poll_package;
using io = go.io_package;
using os = go.os_package;
using syscall = go.syscall_package;

namespace go;

public static partial class net_package {

    // sendFile copies the contents of r to c using the TransmitFile
    // system call to minimize copies.
    //
    // if handled == true, sendFile returns the number of bytes copied and any
    // non-EOF error.
    //
    // if handled == false, sendFile performed no work.
private static (long, error, bool) sendFile(ptr<netFD> _addr_fd, io.Reader r) {
    long written = default;
    error err = default!;
    bool handled = default;
    ref netFD fd = ref _addr_fd.val;

    long n = 0; // by default, copy until EOF.

    ptr<io.LimitedReader> (lr, ok) = r._<ptr<io.LimitedReader>>();
    if (ok) {
        (n, r) = (lr.N, lr.R);        if (n <= 0) {
            return (0, error.As(null!)!, true);
        }
    }
    ptr<os.File> (f, ok) = r._<ptr<os.File>>();
    if (!ok) {
        return (0, error.As(null!)!, false);
    }
    written, err = poll.SendFile(_addr_fd.pfd, syscall.Handle(f.Fd()), n);
    if (err != null) {
        err = wrapSyscallError("transmitfile", err);
    }
    handled = written > 0;

    return ;

}

} // end net_package
