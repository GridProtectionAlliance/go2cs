// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 08 03:35:00 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\unixsock.go
using context = go.context_package;
using os = go.os_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // BUG(mikio): On JS and Plan 9, methods and functions related
        // to UnixConn and UnixListener are not implemented.

        // BUG(mikio): On Windows, methods and functions related to UnixConn
        // and UnixListener don't work for "unixgram" and "unixpacket".

        // UnixAddr represents the address of a Unix domain socket end point.
        public partial struct UnixAddr
        {
            public @string Name;
            public @string Net;
        }

        // Network returns the address's network name, "unix", "unixgram" or
        // "unixpacket".
        private static @string Network(this ptr<UnixAddr> _addr_a)
        {
            ref UnixAddr a = ref _addr_a.val;

            return a.Net;
        }

        private static @string String(this ptr<UnixAddr> _addr_a)
        {
            ref UnixAddr a = ref _addr_a.val;

            if (a == null)
            {
                return "<nil>";
            }

            return a.Name;

        }

        private static bool isWildcard(this ptr<UnixAddr> _addr_a)
        {
            ref UnixAddr a = ref _addr_a.val;

            return a == null || a.Name == "";
        }

        private static Addr opAddr(this ptr<UnixAddr> _addr_a)
        {
            ref UnixAddr a = ref _addr_a.val;

            if (a == null)
            {
                return null;
            }

            return a;

        }

        // ResolveUnixAddr returns an address of Unix domain socket end point.
        //
        // The network must be a Unix network name.
        //
        // See func Dial for a description of the network and address
        // parameters.
        public static (ptr<UnixAddr>, error) ResolveUnixAddr(@string network, @string address)
        {
            ptr<UnixAddr> _p0 = default!;
            error _p0 = default!;

            switch (network)
            {
                case "unix": 

                case "unixgram": 

                case "unixpacket": 
                    return (addr(new UnixAddr(Name:address,Net:network)), error.As(null!)!);
                    break;
                default: 
                    return (_addr_null!, error.As(UnknownNetworkError(network))!);
                    break;
            }

        }

        // UnixConn is an implementation of the Conn interface for connections
        // to Unix domain sockets.
        public partial struct UnixConn
        {
            public ref conn conn => ref conn_val;
        }

        // SyscallConn returns a raw network connection.
        // This implements the syscall.Conn interface.
        private static (syscall.RawConn, error) SyscallConn(this ptr<UnixConn> _addr_c)
        {
            syscall.RawConn _p0 = default;
            error _p0 = default!;
            ref UnixConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (null, error.As(syscall.EINVAL)!);
            }

            return newRawConn(c.fd);

        }

        // CloseRead shuts down the reading side of the Unix domain connection.
        // Most callers should just use Close.
        private static error CloseRead(this ptr<UnixConn> _addr_c)
        {
            ref UnixConn c = ref _addr_c.val;

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

        // CloseWrite shuts down the writing side of the Unix domain connection.
        // Most callers should just use Close.
        private static error CloseWrite(this ptr<UnixConn> _addr_c)
        {
            ref UnixConn c = ref _addr_c.val;

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

        // ReadFromUnix acts like ReadFrom but returns a UnixAddr.
        private static (long, ptr<UnixAddr>, error) ReadFromUnix(this ptr<UnixConn> _addr_c, slice<byte> b)
        {
            long _p0 = default;
            ptr<UnixAddr> _p0 = default!;
            error _p0 = default!;
            ref UnixConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (0L, _addr_null!, error.As(syscall.EINVAL)!);
            }

            var (n, addr, err) = c.readFrom(b);
            if (err != null)
            {
                err = addr(new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
            }

            return (n, _addr_addr!, error.As(err)!);

        }

        // ReadFrom implements the PacketConn ReadFrom method.
        private static (long, Addr, error) ReadFrom(this ptr<UnixConn> _addr_c, slice<byte> b)
        {
            long _p0 = default;
            Addr _p0 = default;
            error _p0 = default!;
            ref UnixConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (0L, null, error.As(syscall.EINVAL)!);
            }

            var (n, addr, err) = c.readFrom(b);
            if (err != null)
            {
                err = addr(new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
            }

            if (addr == null)
            {
                return (n, null, error.As(err)!);
            }

            return (n, addr, error.As(err)!);

        }

        // ReadMsgUnix reads a message from c, copying the payload into b and
        // the associated out-of-band data into oob. It returns the number of
        // bytes copied into b, the number of bytes copied into oob, the flags
        // that were set on the message and the source address of the message.
        //
        // Note that if len(b) == 0 and len(oob) > 0, this function will still
        // read (and discard) 1 byte from the connection.
        private static (long, long, long, ptr<UnixAddr>, error) ReadMsgUnix(this ptr<UnixConn> _addr_c, slice<byte> b, slice<byte> oob)
        {
            long n = default;
            long oobn = default;
            long flags = default;
            ptr<UnixAddr> addr = default!;
            error err = default!;
            ref UnixConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (0L, 0L, 0L, _addr_null!, error.As(syscall.EINVAL)!);
            }

            n, oobn, flags, addr, err = c.readMsg(b, oob);
            if (err != null)
            {
                err = addr(new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
            }

            return ;

        }

        // WriteToUnix acts like WriteTo but takes a UnixAddr.
        private static (long, error) WriteToUnix(this ptr<UnixConn> _addr_c, slice<byte> b, ptr<UnixAddr> _addr_addr)
        {
            long _p0 = default;
            error _p0 = default!;
            ref UnixConn c = ref _addr_c.val;
            ref UnixAddr addr = ref _addr_addr.val;

            if (!c.ok())
            {
                return (0L, error.As(syscall.EINVAL)!);
            }

            var (n, err) = c.writeTo(b, addr);
            if (err != null)
            {
                err = addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr.opAddr(),Err:err));
            }

            return (n, error.As(err)!);

        }

        // WriteTo implements the PacketConn WriteTo method.
        private static (long, error) WriteTo(this ptr<UnixConn> _addr_c, slice<byte> b, Addr addr)
        {
            long _p0 = default;
            error _p0 = default!;
            ref UnixConn c = ref _addr_c.val;

            if (!c.ok())
            {
                return (0L, error.As(syscall.EINVAL)!);
            }

            ptr<UnixAddr> (a, ok) = addr._<ptr<UnixAddr>>();
            if (!ok)
            {
                return (0L, error.As(addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr,Err:syscall.EINVAL))!)!);
            }

            var (n, err) = c.writeTo(b, a);
            if (err != null)
            {
                err = addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:a.opAddr(),Err:err));
            }

            return (n, error.As(err)!);

        }

        // WriteMsgUnix writes a message to addr via c, copying the payload
        // from b and the associated out-of-band data from oob. It returns the
        // number of payload and out-of-band bytes written.
        //
        // Note that if len(b) == 0 and len(oob) > 0, this function will still
        // write 1 byte to the connection.
        private static (long, long, error) WriteMsgUnix(this ptr<UnixConn> _addr_c, slice<byte> b, slice<byte> oob, ptr<UnixAddr> _addr_addr)
        {
            long n = default;
            long oobn = default;
            error err = default!;
            ref UnixConn c = ref _addr_c.val;
            ref UnixAddr addr = ref _addr_addr.val;

            if (!c.ok())
            {
                return (0L, 0L, error.As(syscall.EINVAL)!);
            }

            n, oobn, err = c.writeMsg(b, oob, addr);
            if (err != null)
            {
                err = addr(new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr.opAddr(),Err:err));
            }

            return ;

        }

        private static ptr<UnixConn> newUnixConn(ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

            return addr(new UnixConn(conn{fd}));
        }

        // DialUnix acts like Dial for Unix networks.
        //
        // The network must be a Unix network name; see func Dial for details.
        //
        // If laddr is non-nil, it is used as the local address for the
        // connection.
        public static (ptr<UnixConn>, error) DialUnix(@string network, ptr<UnixAddr> _addr_laddr, ptr<UnixAddr> _addr_raddr)
        {
            ptr<UnixConn> _p0 = default!;
            error _p0 = default!;
            ref UnixAddr laddr = ref _addr_laddr.val;
            ref UnixAddr raddr = ref _addr_raddr.val;

            switch (network)
            {
                case "unix": 

                case "unixgram": 

                case "unixpacket": 
                    break;
                default: 
                    return (_addr_null!, error.As(addr(new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:UnknownNetworkError(network)))!)!);
                    break;
            }
            ptr<sysDialer> sd = addr(new sysDialer(network:network,address:raddr.String()));
            var (c, err) = sd.dialUnix(context.Background(), laddr, raddr);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:err))!)!);
            }

            return (_addr_c!, error.As(null!)!);

        }

        // UnixListener is a Unix domain socket listener. Clients should
        // typically use variables of type Listener instead of assuming Unix
        // domain sockets.
        public partial struct UnixListener
        {
            public ptr<netFD> fd;
            public @string path;
            public bool unlink;
            public sync.Once unlinkOnce;
        }

        private static bool ok(this ptr<UnixListener> _addr_ln)
        {
            ref UnixListener ln = ref _addr_ln.val;

            return ln != null && ln.fd != null;
        }

        // SyscallConn returns a raw network connection.
        // This implements the syscall.Conn interface.
        //
        // The returned RawConn only supports calling Control. Read and
        // Write return an error.
        private static (syscall.RawConn, error) SyscallConn(this ptr<UnixListener> _addr_l)
        {
            syscall.RawConn _p0 = default;
            error _p0 = default!;
            ref UnixListener l = ref _addr_l.val;

            if (!l.ok())
            {
                return (null, error.As(syscall.EINVAL)!);
            }

            return newRawListener(l.fd);

        }

        // AcceptUnix accepts the next incoming call and returns the new
        // connection.
        private static (ptr<UnixConn>, error) AcceptUnix(this ptr<UnixListener> _addr_l)
        {
            ptr<UnixConn> _p0 = default!;
            error _p0 = default!;
            ref UnixListener l = ref _addr_l.val;

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

        // Accept implements the Accept method in the Listener interface.
        // Returned connections will be of type *UnixConn.
        private static (Conn, error) Accept(this ptr<UnixListener> _addr_l)
        {
            Conn _p0 = default;
            error _p0 = default!;
            ref UnixListener l = ref _addr_l.val;

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

        // Close stops listening on the Unix address. Already accepted
        // connections are not closed.
        private static error Close(this ptr<UnixListener> _addr_l)
        {
            ref UnixListener l = ref _addr_l.val;

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

        // Addr returns the listener's network address.
        // The Addr returned is shared by all invocations of Addr, so
        // do not modify it.
        private static Addr Addr(this ptr<UnixListener> _addr_l)
        {
            ref UnixListener l = ref _addr_l.val;

            return l.fd.laddr;
        }

        // SetDeadline sets the deadline associated with the listener.
        // A zero time value disables the deadline.
        private static error SetDeadline(this ptr<UnixListener> _addr_l, time.Time t)
        {
            ref UnixListener l = ref _addr_l.val;

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
        private static (ptr<os.File>, error) File(this ptr<UnixListener> _addr_l)
        {
            ptr<os.File> f = default!;
            error err = default!;
            ref UnixListener l = ref _addr_l.val;

            if (!l.ok())
            {
                return (_addr_null!, error.As(syscall.EINVAL)!);
            }

            f, err = l.file();
            if (err != null)
            {
                err = addr(new OpError(Op:"file",Net:l.fd.net,Source:nil,Addr:l.fd.laddr,Err:err));
            }

            return ;

        }

        // ListenUnix acts like Listen for Unix networks.
        //
        // The network must be "unix" or "unixpacket".
        public static (ptr<UnixListener>, error) ListenUnix(@string network, ptr<UnixAddr> _addr_laddr)
        {
            ptr<UnixListener> _p0 = default!;
            error _p0 = default!;
            ref UnixAddr laddr = ref _addr_laddr.val;

            switch (network)
            {
                case "unix": 

                case "unixpacket": 
                    break;
                default: 
                    return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:UnknownNetworkError(network)))!)!);
                    break;
            }
            if (laddr == null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:errMissingAddress))!)!);
            }

            ptr<sysListener> sl = addr(new sysListener(network:network,address:laddr.String()));
            var (ln, err) = sl.listenUnix(context.Background(), laddr);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:err))!)!);
            }

            return (_addr_ln!, error.As(null!)!);

        }

        // ListenUnixgram acts like ListenPacket for Unix networks.
        //
        // The network must be "unixgram".
        public static (ptr<UnixConn>, error) ListenUnixgram(@string network, ptr<UnixAddr> _addr_laddr)
        {
            ptr<UnixConn> _p0 = default!;
            error _p0 = default!;
            ref UnixAddr laddr = ref _addr_laddr.val;

            switch (network)
            {
                case "unixgram": 
                    break;
                default: 
                    return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:UnknownNetworkError(network)))!)!);
                    break;
            }
            if (laddr == null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:nil,Err:errMissingAddress))!)!);
            }

            ptr<sysListener> sl = addr(new sysListener(network:network,address:laddr.String()));
            var (c, err) = sl.listenUnixgram(context.Background(), laddr);
            if (err != null)
            {
                return (_addr_null!, error.As(addr(new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:err))!)!);
            }

            return (_addr_c!, error.As(null!)!);

        }
    }
}
