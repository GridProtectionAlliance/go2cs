// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:18 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sendfile_windows.go
using poll = go.@internal.poll_package;
using io = go.io_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // sendFile copies the contents of r to c using the TransmitFile
        // system call to minimize copies.
        //
        // if handled == true, sendFile returns the number of bytes copied and any
        // non-EOF error.
        //
        // if handled == false, sendFile performed no work.
        //
        // Note that sendfile for windows does not support >2GB file.
        private static (long, error, bool) sendFile(ref netFD fd, io.Reader r)
        {
            long n = 0L; // by default, copy until EOF

            ref io.LimitedReader (lr, ok) = r._<ref io.LimitedReader>();
            if (ok)
            {
                n = lr.N;
                r = lr.R;
                if (n <= 0L)
                {
                    return (0L, null, true);
                }
            }
            ref os.File (f, ok) = r._<ref os.File>();
            if (!ok)
            {
                return (0L, null, false);
            }
            var (done, err) = poll.SendFile(ref fd.pfd, syscall.Handle(f.Fd()), n);

            if (err != null)
            {
                return (0L, wrapSyscallError("transmitfile", err), false);
            }
            if (lr != null)
            {
                lr.N -= int64(done);
            }
            return (int64(done), null, true);
        }
    }
}
