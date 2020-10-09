// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 09 04:52:25 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\tcpsock.go
using context = go.context_package;
using io = go.io_package;
using os = go.os_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // BUG(mikio): On JS and Windows, the File method of TCPConn and
        // TCPListener is not implemented.

        // TCPAddr represents the address of a TCP end point.
        public partial struct TCPAddr
        {
            public IP IP;
            public long Port;
            public @string Zone; // IPv6 scoped addressing zone
        }

        // Network returns the address's network name, "tcp".
        private static @string Network(this ptr<TCPAddr> _addr_a)
        {
            ref TCPAddr a = ref _addr_a.val;

            return "tcp";
        }

        private static @string String(this ptr<TCPAddr> _addr_a)
        {
            ref TCPAddr a = ref _addr_a.val;

            if (a == null)
            {
                return "<nil>";
            }

            var ip = ipEmptyString(a.IP);
            if (a.Zone != "")
            {
                return JoinHostPort(ip + "%" + a.Zone, itoa(a.Port));
            }

            return JoinHostPort(ip, itoa(a.Port));

        }

        private static bool isWildcard(this ptr<TCPAddr> _addr_a)
        {
            ref TCPAddr a = ref _addr_a.val;

            if (a == null || a.IP == null)
            {
                return true;
            }

            return a.IP.IsUnspecified();

        }

        private static Addr opAddr(this ptr<TCPAddr> _addr_a)
        {
            ref TCPAddr a = ref _addr_a.val;

            if (a == null)
            {
                return null;
            }

            return a;

        }

        // ResolveTCPAddr returns an address of TCP end point.
        //
        // The network must be a TCP network name.
        //
        // If the host in the address parameter is not a literal IP address or
        // the port is not a literal port number, ResolveTCPAddr resolves the
        // address to an address of TCP end point.
        // Otherwise, it parses the address as a pair of literal IP address
        // and port number.
        // The address parameter can use a host name, but this is not
        // recommended, because it will return at most one of the host name's
        // IP addresses.
        //
        // See func Dial for a description of the network and address
        // parameters.
        public static (ptr<TCPAddr>, error) ResolveTCPAddr(@string network, @string address)
        {
            ptr<TCPAddr> _p0 = default!;
            error _p0 = default!;

            switch (network)
            {
                case "tcp": 

                case "tcp4": 

                case "tcp6": 
                    break;
                case "": // a hint wildcard for Go 1.0 undocumented behavior
                    network = "tcp";
                    break;
                default: 
                    return (_addr_null!, error.As(UnknownNetworkError(network))!);
                    break;
            }
            var (addrs, err) = DefaultResolver.internetAddrList(context.Background(), network, address);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return (addrs.forResolve(network, address)._<ptr<TCPAddr>>(), error.As(null!)!);

        }

        // TCPConn is an implementation of the Conn interface for TCP network
        // connections.
        public partial struct TCPConn
        {
            public ref conn conn => ref conn_val;
        }

        // SyscallConn returns a raw network connection.
        // This implements the syscall.Conn interface.
        private static (syscall.RawConn, error) SyscallConn(this ptr<TCPConn> _addr_c)
        {
            syscall.RawConn _p0 = default;
            error _p0 = default!;
            ref TCPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (null, error.As(syscall.EINVAL)!);
            }

            return newRawConn(c.fd);

        }

        // ReadFrom implements the io.ReaderFrom ReadFrom method.
        private static (long, error) ReadFrom(this ptr<TCPConn> _addr_c, io.Reader r)
        {
            long _p0 = default;
            error _p0 = default!;
            ref TCPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (0L, error.As(syscall.EINVAL)!);
            }

            var (n, err) = c.readFrom(r);
            if (err != null && err != io.EOF)
            {
                err = addr(new OpError(Op:"readfrom",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
            }

            return (n, error.As(err)!);

        }

        // CloseRead shuts down the reading side of the TCP connection.
        // Most callers should just use Close.
        private static error CloseRead(this ptr<TCPConn> _addr_c)
        {
            ref TCPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return error.As(syscall.EINVAL)!;
            }

            {
                var err = c.fd.closeRead();

                if (err != null)
                {
                    return error.As(addr(new OpError(Op:"close",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err))!)!;
                }

            }

            return error.As(null!)!;

        }

        // CloseWrite shuts down the writing side of the TCP connection.
        // Most callers should just use Close.
        private static error CloseWrite(this ptr<TCPConn> _addr_c)
        {
            ref TCPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return error.As(syscall.EINVAL)!;
            }

            {
                var err = c.fd.closeWrite();

                if (err != null)
                {
                    return error.As(addr(new OpError(Op:"close",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err))!)!;
                }

            }

            return error.As(null!)!;

        }

        // SetLinger sets the behavior of Close on a connection which still
        // has data waiting to be sent or to be acknowledged.
        //
        // If sec < 0 (the default), the operating system finishes sending the
        // data in the background.
        //
        // If sec == 0, the operating system discards any unsent or
        // unacknowledged data.
        //
        // If sec > 0, the data is sent in the background as with sec < 0. On
        // some operating systems after sec seconds have elapsed any remaining
        // unsent data may be discarded.
        private static error SetLinger(this ptr<TCPConn> _addr_c, long sec)
        {
            ref TCPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return error.As(syscall.EINVAL)!;
            }

            {
                var err = setLinger(c.fd, sec);

                if (err != null)
                {
                    return error.As(addr(new OpError(Op:"set",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err))!)!;
                }

            }

            return error.As(null!)!;

        }

        // SetKeepAlive sets whether the operating system should send
        // keep-alive messages on the connection.
        private static error SetKeepAlive(this ptr<TCPConn> _addr_c, bool keepalive)
        {
            ref TCPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return error.As(syscall.EINVAL)!;
            }

            {
                var err = setKeepAlive(c.fd, keepalive);

                if (err != null)
                {
                    return error.As(addr(new OpError(Op:"set",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err))!)!;
                }

            }

            return error.As(null!)!;

        }

        // SetKeepAlivePeriod sets period between keep-alives.
        private static error SetKeepAlivePeriod(this ptr<TCPConn> _addr_c, time.Duration d)
        {
            ref TCPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return error.As(syscall.EINVAL)!;
            }

            {
                var err = setKeepAlivePeriod(c.fd, d);

                if (err != null)
                {
                    return error.As(addr(new OpError(Op:"set",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err))!)!;
                }

            }

            return error.As(null!)!;

        }

        // SetNoDelay controls whether the operating system should delay
        // packet transmission in hopes of sending fewer packets (Nagle's
        // algorithm).  The default is true (no delay), meaning that data is
        // sent as soon as possible after a Write.
        private static error SetNoDelay(this ptr<TCPConn> _addr_c, bool noDelay)
        {
            ref TCPConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return error.As(syscall.EINVAL)!;
            }

            {
                var err = setNoDelay(c.fd, noDelay);

                if (err != null)
                {
                    return error.As(addr(new OpError(Op:"set",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err))!)!;
                }

            }

            return error.As(null!)!;

        }

        private static ptr<TCPConn> newTCPConn(ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

            ptr<TCPConn> c = addr(new TCPConn(conn{fd}));
            setNoDelay(c.fd, true);
            return _addr_c!;
        }

        // DialTCP acts like Dial for TCP networks.
        //
        // The network must be a TCP network name; see func Dial for details.
        //
        // If laddr is nil, a local address is automatically chosen.
        // If the IP field of raddr is nil or an unspecified IP address, the
        // local system is assumed.
        public static (ptr<TCPConn>, error) DialTCP(@string network, ptr<TCPAddr> _addr_laddr, ptr<TCPAddr> _addr_raddr)
        {
            ptr<TCPConn> _p0 = default!;
            error _p0 = default!;
            ref TCPAddr laddr = ref _addr_laddr.val;
            ref TCPAddr raddr = ref _addr_raddr.val;

            switch (network)
            {
                case "tcp": 

                case "tcp4": 

                case "tcp6": 
                    break;
                default: 
                    return (_addr_null!, error.As(addr(new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:UnknownNetworkError(network)))!)!);
                    break;
            }
            if (raddr == null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:nil,Err:errMissingAddress))!)!);
            }

            ptr<sysDialer> sd = addr(new sysDialer(network:network,address:raddr.String()));
            var (c, err) = sd.dialTCP(context.Background(), laddr, raddr);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:err))!)!);
            }

            return (_addr_c!, error.As(null!)!);

        }

        // TCPListener is a TCP network listener. Clients should typically
        // use variables of type Listener instead of assuming TCP.
        public partial struct TCPListener
        {
            public ptr<netFD> fd;
            public ListenConfig lc;
        }

        // SyscallConn returns a raw network connection.
        // This implements the syscall.Conn interface.
        //
        // The returned RawConn only supports calling Control. Read and
        // Write return an error.
        private static (syscall.RawConn, error) SyscallConn(this ptr<TCPListener> _addr_l)
        {
            syscall.RawConn _p0 = default;
            error _p0 = default!;
            ref TCPListener l = ref _addr_l.val;

            if (!l.ok())
            {
                return (null, error.As(syscall.EINVAL)!);
            }

            return newRawListener(l.fd);

        }

        // AcceptTCP accepts the next incoming call and returns the new
        // connection.
        private static (ptr<TCPConn>, error) AcceptTCP(this ptr<TCPListener> _addr_l)
        {
            ptr<TCPConn> _p0 = default!;
            error _p0 = default!;
            ref TCPListener l = ref _addr_l.val;

            if (!l.ok())
            {
                return (_addr_null!, error.As(syscall.EINVAL)!);
            }

            var (c, err) = l.accept();
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"accept",Net:l.fd.net,Source:nil,Addr:l.fd.laddr,Err:err))!)!);
            }

            return (_addr_c!, error.As(null!)!);

        }

        // Accept implements the Accept method in the Listener interface; it
        // waits for the next call and returns a generic Conn.
        private static (Conn, error) Accept(this ptr<TCPListener> _addr_l)
        {
            Conn _p0 = default;
            error _p0 = default!;
            ref TCPListener l = ref _addr_l.val;

            if (!l.ok())
            {
                return (null, error.As(syscall.EINVAL)!);
            }

            var (c, err) = l.accept();
            if (err != null)
            {
                return (null, error.As(addr(new OpError(Op:"accept",Net:l.fd.net,Source:nil,Addr:l.fd.laddr,Err:err))!)!);
            }

            return (c, error.As(null!)!);

        }

        // Close stops listening on the TCP address.
        // Already Accepted connections are not closed.
        private static error Close(this ptr<TCPListener> _addr_l)
        {
            ref TCPListener l = ref _addr_l.val;

            if (!l.ok())
            {
                return error.As(syscall.EINVAL)!;
            }

            {
                var err = l.close();

                if (err != null)
                {
                    return error.As(addr(new OpError(Op:"close",Net:l.fd.net,Source:nil,Addr:l.fd.laddr,Err:err))!)!;
                }

            }

            return error.As(null!)!;

        }

        // Addr returns the listener's network address, a *TCPAddr.
        // The Addr returned is shared by all invocations of Addr, so
        // do not modify it.
        private static Addr Addr(this ptr<TCPListener> _addr_l)
        {
            ref TCPListener l = ref _addr_l.val;

            return l.fd.laddr;
        }

        // SetDeadline sets the deadline associated with the listener.
        // A zero time value disables the deadline.
        private static error SetDeadline(this ptr<TCPListener> _addr_l, time.Time t)
        {
            ref TCPListener l = ref _addr_l.val;

            if (!l.ok())
            {
                return error.As(syscall.EINVAL)!;
            }

            {
                var err = l.fd.pfd.SetDeadline(t);

                if (err != null)
                {
                    return error.As(addr(new OpError(Op:"set",Net:l.fd.net,Source:nil,Addr:l.fd.laddr,Err:err))!)!;
                }

            }

            return error.As(null!)!;

        }

        // File returns a copy of the underlying os.File.
        // It is the caller's responsibility to close f when finished.
        // Closing l does not affect f, and closing f does not affect l.
        //
        // The returned os.File's file descriptor is different from the
        // connection's. Attempting to change properties of the original
        // using this duplicate may or may not have the desired effect.
        private static (ptr<os.File>, error) File(this ptr<TCPListener> _addr_l)
        {
            ptr<os.File> f = default!;
            error err = default!;
            ref TCPListener l = ref _addr_l.val;

            if (!l.ok())
            {
                return (_addr_null!, error.As(syscall.EINVAL)!);
            }

            f, err = l.file();
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"file",Net:l.fd.net,Source:nil,Addr:l.fd.laddr,Err:err))!)!);
            }

            return ;

        }

        // ListenTCP acts like Listen for TCP networks.
        //
        // The network must be a TCP network name; see func Dial for details.
        //
        // If the IP field of laddr is nil or an unspecified IP address,
        // ListenTCP listens on all available unicast and anycast IP addresses
        // of the local system.
        // If the Port field of laddr is 0, a port number is automatically
        // chosen.
        public static (ptr<TCPListener>, error) ListenTCP(@string network, ptr<TCPAddr> _addr_laddr)
        {
            ptr<TCPListener> _p0 = default!;
            error _p0 = default!;
            ref TCPAddr laddr = ref _addr_laddr.val;

            switch (network)
            {
                case "tcp": 

                case "tcp4": 

                case "tcp6": 
                    break;
                default: 
                    return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:UnknownNetworkError(network)))!)!);
                    break;
            }
            if (laddr == null)
            {
                laddr = addr(new TCPAddr());
            }

            ptr<sysListener> sl = addr(new sysListener(network:network,address:laddr.String()));
            var (ln, err) = sl.listenTCP(context.Background(), laddr);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:err))!)!);
            }

            return (_addr_ln!, error.As(null!)!);

        }

        // roundDurationUp rounds d to the next multiple of to.
        private static time.Duration roundDurationUp(time.Duration d, time.Duration to)
        {
            return (d + to - 1L) / to;
        }
    }
}
