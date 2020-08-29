// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd

// package net -- go2cs converted at 2020 August 29 08:27:16 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\sendfile_bsd.go
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
            // FreeBSD and DragonFly use 0 as the "until EOF" value.
            // If you pass in more bytes than the file contains, it will
            // loop back to the beginning ad nauseam until it's sent
            // exactly the number of bytes told to. As such, we need to
            // know exactly how many bytes to send.
            long remain = 0L;

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
            if (remain == 0L)
            {
                var (fi, err) = f.Stat();
                if (err != null)
                {
                    return (0L, err, false);
                }
                remain = fi.Size();
            }
            var (pos, err) = f.Seek(0L, io.SeekCurrent);
            if (err != null)
            {
                return (0L, err, false);
            }
            written, err = poll.SendFile(ref c.pfd, int(f.Fd()), pos, remain);

            if (lr != null)
            {
                lr.N = remain - written;
            }
            return (written, wrapSyscallError("sendfile", err), written > 0L);
        }
    }
}
