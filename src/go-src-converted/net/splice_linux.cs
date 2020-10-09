// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:52:24 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\splice_linux.go
using poll = go.@internal.poll_package;
using io = go.io_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // splice transfers data from r to c using the splice system call to minimize
        // copies from and to userspace. c must be a TCP connection. Currently, splice
        // is only enabled if r is a TCP or a stream-oriented Unix connection.
        //
        // If splice returns handled == false, it has performed no work.
        private static (long, error, bool) splice(ptr<netFD> _addr_c, io.Reader r)
        {
            long written = default;
            error err = default!;
            bool handled = default;
            ref netFD c = ref _addr_c.val;

            long remain = 1L << (int)(62L); // by default, copy until EOF
            ptr<io.LimitedReader> (lr, ok) = r._<ptr<io.LimitedReader>>();
            if (ok)
            {
                remain = lr.N;
                r = lr.R;
                if (remain <= 0L)
                {
                    return (0L, error.As(null!)!, true);
                }
            }
            ptr<netFD> s;
            {
                ptr<TCPConn> (tc, ok) = r._<ptr<TCPConn>>();

                if (ok)
                {
                    s = tc.fd;
                }                {
                    ptr<UnixConn> (uc, ok) = r._<ptr<UnixConn>>();


                    else if (ok)
                    {
                        if (uc.fd.net != "unix")
                        {
                            return (0L, error.As(null!)!, false);
                        }
                        s = uc.fd;

                    }
                    else
                    {
                        return (0L, error.As(null!)!, false);
                    }
                }



            }


            var (written, handled, sc, err) = poll.Splice(_addr_c.pfd, _addr_s.pfd, remain);
            if (lr != null)
            {
                lr.N -= written;
            }
            return (written, error.As(wrapSyscallError(sc, err))!, handled);

        }
    }
}
