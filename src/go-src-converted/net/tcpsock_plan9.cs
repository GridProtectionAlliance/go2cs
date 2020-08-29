// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:27:59 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\tcpsock_plan9.go
using context = go.context_package;
using io = go.io_package;
using os = go.os_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        private static (long, error) readFrom(this ref TCPConn c, io.Reader r)
        {
            return genericReadFrom(c, r);
        }

        private static (ref TCPConn, error) dialTCP(context.Context ctx, @string net, ref TCPAddr laddr, ref TCPAddr raddr)
        {
            if (testHookDialTCP != null)
            {
                return testHookDialTCP(ctx, net, laddr, raddr);
            }
            return doDialTCP(ctx, net, laddr, raddr);
        }

        private static (ref TCPConn, error) doDialTCP(context.Context ctx, @string net, ref TCPAddr laddr, ref TCPAddr raddr)
        {
            switch (net)
            {
                case "tcp": 

                case "tcp4": 

                case "tcp6": 
                    break;
                default: 
                    return (null, UnknownNetworkError(net));
                    break;
            }
            if (raddr == null)
            {
                return (null, errMissingAddress);
            }
            var (fd, err) = dialPlan9(ctx, net, laddr, raddr);
            if (err != null)
            {
                return (null, err);
            }
            return (newTCPConn(fd), null);
        }

        private static bool ok(this ref TCPListener ln)
        {
            return ln != null && ln.fd != null && ln.fd.ctl != null;
        }

        private static (ref TCPConn, error) accept(this ref TCPListener ln)
        {
            var (fd, err) = ln.fd.acceptPlan9();
            if (err != null)
            {
                return (null, err);
            }
            return (newTCPConn(fd), null);
        }

        private static error close(this ref TCPListener ln)
        {
            {
                var err__prev1 = err;

                var err = ln.fd.pfd.Close();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            {
                var err__prev1 = err;

                var (_, err) = ln.fd.ctl.WriteString("hangup");

                if (err != null)
                {
                    ln.fd.ctl.Close();
                    return error.As(err);
                }

                err = err__prev1;

            }
            {
                var err__prev1 = err;

                err = ln.fd.ctl.Close();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            return error.As(null);
        }

        private static (ref os.File, error) file(this ref TCPListener ln)
        {
            var (f, err) = ln.dup();
            if (err != null)
            {
                return (null, err);
            }
            return (f, null);
        }

        private static (ref TCPListener, error) listenTCP(context.Context ctx, @string network, ref TCPAddr laddr)
        {
            var (fd, err) = listenPlan9(ctx, network, laddr);
            if (err != null)
            {
                return (null, err);
            }
            return (ref new TCPListener(fd), null);
        }
    }
}
