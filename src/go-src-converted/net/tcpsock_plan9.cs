// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:52:30 UTC
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
        private static (long, error) readFrom(this ptr<TCPConn> _addr_c, io.Reader r)
        {
            long _p0 = default;
            error _p0 = default!;
            ref TCPConn c = ref _addr_c.val;

            return genericReadFrom(c, r);
        }

        private static (ptr<TCPConn>, error) dialTCP(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<TCPAddr> _addr_laddr, ptr<TCPAddr> _addr_raddr)
        {
            ptr<TCPConn> _p0 = default!;
            error _p0 = default!;
            ref sysDialer sd = ref _addr_sd.val;
            ref TCPAddr laddr = ref _addr_laddr.val;
            ref TCPAddr raddr = ref _addr_raddr.val;

            if (testHookDialTCP != null)
            {
                return _addr_testHookDialTCP(ctx, sd.network, laddr, raddr)!;
            }

            return _addr_sd.doDialTCP(ctx, laddr, raddr)!;

        }

        private static (ptr<TCPConn>, error) doDialTCP(this ptr<sysDialer> _addr_sd, context.Context ctx, ptr<TCPAddr> _addr_laddr, ptr<TCPAddr> _addr_raddr)
        {
            ptr<TCPConn> _p0 = default!;
            error _p0 = default!;
            ref sysDialer sd = ref _addr_sd.val;
            ref TCPAddr laddr = ref _addr_laddr.val;
            ref TCPAddr raddr = ref _addr_raddr.val;

            switch (sd.network)
            {
                case "tcp4": 
                    // Plan 9 doesn't complain about [::]:0->127.0.0.1, so it's up to us.
                    if (laddr != null && len(laddr.IP) != 0L && laddr.IP.To4() == null)
                    {
                        return (_addr_null!, error.As(addr(new AddrError(Err:"non-IPv4 local address",Addr:laddr.String()))!)!);
                    }

                    break;
                case "tcp": 

                case "tcp6": 
                    break;
                default: 
                    return (_addr_null!, error.As(UnknownNetworkError(sd.network))!);
                    break;
            }
            if (raddr == null)
            {
                return (_addr_null!, error.As(errMissingAddress)!);
            }

            var (fd, err) = dialPlan9(ctx, sd.network, laddr, raddr);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_newTCPConn(fd)!, error.As(null!)!);

        }

        private static bool ok(this ptr<TCPListener> _addr_ln)
        {
            ref TCPListener ln = ref _addr_ln.val;

            return ln != null && ln.fd != null && ln.fd.ctl != null;
        }

        private static (ptr<TCPConn>, error) accept(this ptr<TCPListener> _addr_ln)
        {
            ptr<TCPConn> _p0 = default!;
            error _p0 = default!;
            ref TCPListener ln = ref _addr_ln.val;

            var (fd, err) = ln.fd.acceptPlan9();
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            var tc = newTCPConn(fd);
            if (ln.lc.KeepAlive >= 0L)
            {
                setKeepAlive(fd, true);
                var ka = ln.lc.KeepAlive;
                if (ln.lc.KeepAlive == 0L)
                {
                    ka = defaultTCPKeepAlive;
                }

                setKeepAlivePeriod(fd, ka);

            }

            return (_addr_tc!, error.As(null!)!);

        }

        private static error close(this ptr<TCPListener> _addr_ln)
        {
            ref TCPListener ln = ref _addr_ln.val;

            {
                var err__prev1 = err;

                var err = ln.fd.pfd.Close();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                var (_, err) = ln.fd.ctl.WriteString("hangup");

                if (err != null)
                {
                    ln.fd.ctl.Close();
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            {
                var err__prev1 = err;

                err = ln.fd.ctl.Close();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            return error.As(null!)!;

        }

        private static (ptr<os.File>, error) file(this ptr<TCPListener> _addr_ln)
        {
            ptr<os.File> _p0 = default!;
            error _p0 = default!;
            ref TCPListener ln = ref _addr_ln.val;

            var (f, err) = ln.dup();
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (_addr_f!, error.As(null!)!);

        }

        private static (ptr<TCPListener>, error) listenTCP(this ptr<sysListener> _addr_sl, context.Context ctx, ptr<TCPAddr> _addr_laddr)
        {
            ptr<TCPListener> _p0 = default!;
            error _p0 = default!;
            ref sysListener sl = ref _addr_sl.val;
            ref TCPAddr laddr = ref _addr_laddr.val;

            var (fd, err) = listenPlan9(ctx, sl.network, laddr);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (addr(new TCPListener(fd:fd,lc:sl.ListenConfig)), error.As(null!)!);

        }
    }
}
