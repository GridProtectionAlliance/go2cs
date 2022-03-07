// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2022 March 06 22:16:43 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Program Files\Go\src\net\splice_linux.go
using poll = go.@internal.poll_package;
using io = go.io_package;

namespace go;

public static partial class net_package {

    // splice transfers data from r to c using the splice system call to minimize
    // copies from and to userspace. c must be a TCP connection. Currently, splice
    // is only enabled if r is a TCP or a stream-oriented Unix connection.
    //
    // If splice returns handled == false, it has performed no work.
private static (long, error, bool) splice(ptr<netFD> _addr_c, io.Reader r) {
    long written = default;
    error err = default!;
    bool handled = default;
    ref netFD c = ref _addr_c.val;

    long remain = 1 << 62; // by default, copy until EOF
    ptr<io.LimitedReader> (lr, ok) = r._<ptr<io.LimitedReader>>();
    if (ok) {
        (remain, r) = (lr.N, lr.R);        if (remain <= 0) {
            return (0, error.As(null!)!, true);
        }
    }
    ptr<netFD> s;
    {
        ptr<TCPConn> (tc, ok) = r._<ptr<TCPConn>>();

        if (ok) {
            s = tc.fd;
        }        {
            ptr<UnixConn> (uc, ok) = r._<ptr<UnixConn>>();


            else if (ok) {
                if (uc.fd.net != "unix") {
                    return (0, error.As(null!)!, false);
                }
                s = uc.fd;

            }
            else
 {
                return (0, error.As(null!)!, false);
            }
        }



    }


    var (written, handled, sc, err) = poll.Splice(_addr_c.pfd, _addr_s.pfd, remain);
    if (lr != null) {
        lr.N -= written;
    }
    return (written, error.As(wrapSyscallError(sc, err))!, handled);

}

} // end net_package
