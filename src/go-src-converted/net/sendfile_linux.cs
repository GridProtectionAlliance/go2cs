// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:17 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sendfile_linux.go
using poll = go.@internal.poll_package;
using io = go.io_package;
using os = go.os_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // sendFile copies the contents of r to c using the sendfile
        // system call to minimize copies.
        //
        // if handled == true, sendFile returns the number of bytes copied and any
        // non-EOF error.
        //
        // if handled == false, sendFile performed no work.
        private static (long, error, bool) sendFile(ref netFD c, io.Reader r)
        {
            long remain = 1L << (int)(62L); // by default, copy until EOF

            ref io.LimitedReader (lr, ok) = r._<ref io.LimitedReader>();
            if (ok)
            {
                remain = lr.N;
                r = lr.R;
                if (remain <= 0L)
                {
                    return (0L, null, true);
                }
            }
            ref os.File (f, ok) = r._<ref os.File>();
            if (!ok)
            {
                return (0L, null, false);
            }
            written, err = poll.SendFile(ref c.pfd, int(f.Fd()), remain);

            if (lr != null)
            {
                lr.N = remain - written;
            }
            return (written, wrapSyscallError("sendfile", err), written > 0L);
        }
    }
}
