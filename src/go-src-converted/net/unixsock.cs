// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:28:09 UTC
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
        // UnixAddr represents the address of a Unix domain socket end point.
        public partial struct UnixAddr
        {
            public @string Name;
            public @string Net;
        }

        // Network returns the address's network name, "unix", "unixgram" or
        // "unixpacket".
        private static @string Network(this ref UnixAddr a)
        {
            return a.Net;
        }

        private static @string String(this ref UnixAddr a)
        {
            if (a == null)
            {
                return "<nil>";
            }
            return a.Name;
        }

        private static bool isWildcard(this ref UnixAddr a)
        {
            return a == null || a.Name == "";
        }

        private static Addr opAddr(this ref UnixAddr a)
        {
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
        public static (ref UnixAddr, error) ResolveUnixAddr(@string network, @string address)
        {
            switch (network)
            {
                case "unix": 

                case "unixgram": 

                case "unixpacket": 
                    return (ref new UnixAddr(Name:address,Net:network), null);
                    break;
                default: 
                    return (null, UnknownNetworkError(network));
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
        private static (syscall.RawConn, error) SyscallConn(this ref UnixConn c)
        {
            if (!c.ok())
            {
                return (null, syscall.EINVAL);
            }
            return newRawConn(c.fd);
        }

        // CloseRead shuts down the reading side of the Unix domain connection.
        // Most callers should just use Close.
        private static error CloseRead(this ref UnixConn c)
        {
            if (!c.ok())
            {
                return error.As(syscall.EINVAL);
            }
            {
                var err = c.fd.closeRead();

                if (err != null)
                {
                    return error.As(ref new OpError(Op:"close",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
                }

            }
            return error.As(null);
        }

        // CloseWrite shuts down the writing side of the Unix domain connection.
        // Most callers should just use Close.
        private static error CloseWrite(this ref UnixConn c)
        {
            if (!c.ok())
            {
                return error.As(syscall.EINVAL);
            }
            {
                var err = c.fd.closeWrite();

                if (err != null)
                {
                    return error.As(ref new OpError(Op:"close",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
                }

            }
            return error.As(null);
        }

        // ReadFromUnix acts like ReadFrom but returns a UnixAddr.
        private static (long, ref UnixAddr, error) ReadFromUnix(this ref UnixConn c, slice<byte> b)
        {
            if (!c.ok())
            {
                return (0L, null, syscall.EINVAL);
            }
            var (n, addr, err) = c.readFrom(b);
            if (err != null)
            {
                err = ref new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            return (n, addr, err);
        }

        // ReadFrom implements the PacketConn ReadFrom method.
        private static (long, Addr, error) ReadFrom(this ref UnixConn c, slice<byte> b)
        {
            if (!c.ok())
            {
                return (0L, null, syscall.EINVAL);
            }
            var (n, addr, err) = c.readFrom(b);
            if (err != null)
            {
                err = ref new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            if (addr == null)
            {
                return (n, null, err);
            }
            return (n, addr, err);
        }

        // ReadMsgUnix reads a message from c, copying the payload into b and
        // the associated out-of-band data into oob. It returns the number of
        // bytes copied into b, the number of bytes copied into oob, the flags
        // that were set on the message and the source address of the message.
        //
        // Note that if len(b) == 0 and len(oob) > 0, this function will still
        // read (and discard) 1 byte from the connection.
        private static (long, long, long, ref UnixAddr, error) ReadMsgUnix(this ref UnixConn c, slice<byte> b, slice<byte> oob)
        {
            if (!c.ok())
            {
                return (0L, 0L, 0L, null, syscall.EINVAL);
            }
            n, oobn, flags, addr, err = c.readMsg(b, oob);
            if (err != null)
            {
                err = ref new OpError(Op:"read",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err);
            }
            return;
        }

        // WriteToUnix acts like WriteTo but takes a UnixAddr.
        private static (long, error) WriteToUnix(this ref UnixConn c, slice<byte> b, ref UnixAddr addr)
        {
            if (!c.ok())
            {
                return (0L, syscall.EINVAL);
            }
            var (n, err) = c.writeTo(b, addr);
            if (err != null)
            {
                err = ref new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr.opAddr(),Err:err);
            }
            return (n, err);
        }

        // WriteTo implements the PacketConn WriteTo method.
        private static (long, error) WriteTo(this ref UnixConn c, slice<byte> b, Addr addr)
        {
            if (!c.ok())
            {
                return (0L, syscall.EINVAL);
            }
            ref UnixAddr (a, ok) = addr._<ref UnixAddr>();
            if (!ok)
            {
                return (0L, ref new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr,Err:syscall.EINVAL));
            }
            var (n, err) = c.writeTo(b, a);
            if (err != null)
            {
                err = ref new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:a.opAddr(),Err:err);
            }
            return (n, err);
        }

        // WriteMsgUnix writes a message to addr via c, copying the payload
        // from b and the associated out-of-band data from oob. It returns the
        // number of payload and out-of-band bytes written.
        //
        // Note that if len(b) == 0 and len(oob) > 0, this function will still
        // write 1 byte to the connection.
        private static (long, long, error) WriteMsgUnix(this ref UnixConn c, slice<byte> b, slice<byte> oob, ref UnixAddr addr)
        {
            if (!c.ok())
            {
                return (0L, 0L, syscall.EINVAL);
            }
            n, oobn, err = c.writeMsg(b, oob, addr);
            if (err != null)
            {
                err = ref new OpError(Op:"write",Net:c.fd.net,Source:c.fd.laddr,Addr:addr.opAddr(),Err:err);
            }
            return;
        }

        private static ref UnixConn newUnixConn(ref netFD fd)
        {
            return ref new UnixConn(conn{fd});
        }

        // DialUnix acts like Dial for Unix networks.
        //
        // The network must be a Unix network name; see func Dial for details.
        //
        // If laddr is non-nil, it is used as the local address for the
        // connection.
        public static (ref UnixConn, error) DialUnix(@string network, ref UnixAddr laddr, ref UnixAddr raddr)
        {
            switch (network)
            {
                case "unix": 

                case "unixgram": 

                case "unixpacket": 
                    break;
                default: 
                    return (null, ref new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:UnknownNetworkError(network)));
                    break;
            }
            var (c, err) = dialUnix(context.Background(), network, laddr, raddr);
            if (err != null)
            {
                return (null, ref new OpError(Op:"dial",Net:network,Source:laddr.opAddr(),Addr:raddr.opAddr(),Err:err));
            }
            return (c, null);
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

        private static bool ok(this ref UnixListener ln)
        {
            return ln != null && ln.fd != null;
        }

        // SyscallConn returns a raw network connection.
        // This implements the syscall.Conn interface.
        //
        // The returned RawConn only supports calling Control. Read and
        // Write return an error.
        private static (syscall.RawConn, error) SyscallConn(this ref UnixListener l)
        {
            if (!l.ok())
            {
                return (null, syscall.EINVAL);
            }
            return newRawListener(l.fd);
        }

        // AcceptUnix accepts the next incoming call and returns the new
        // connection.
        private static (ref UnixConn, error) AcceptUnix(this ref UnixListener l)
        {
            if (!l.ok())
            {
                return (null, syscall.EINVAL);
            }
            var (c, err) = l.accept();
            if (err != null)
            {
                return (null, ref new OpError(Op:"accept",Net:l.fd.net,Source:nil,Addr:l.fd.laddr,Err:err));
            }
            return (c, null);
        }

        // Accept implements the Accept method in the Listener interface.
        // Returned connections will be of type *UnixConn.
        private static (Conn, error) Accept(this ref UnixListener l)
        {
            if (!l.ok())
            {
                return (null, syscall.EINVAL);
            }
            var (c, err) = l.accept();
            if (err != null)
            {
                return (null, ref new OpError(Op:"accept",Net:l.fd.net,Source:nil,Addr:l.fd.laddr,Err:err));
            }
            return (c, null);
        }

        // Close stops listening on the Unix address. Already accepted
        // connections are not closed.
        private static error Close(this ref UnixListener l)
        {
            if (!l.ok())
            {
                return error.As(syscall.EINVAL);
            }
            {
                var err = l.close();

                if (err != null)
                {
                    return error.As(ref new OpError(Op:"close",Net:l.fd.net,Source:nil,Addr:l.fd.laddr,Err:err));
                }

            }
            return error.As(null);
        }

        // Addr returns the listener's network address.
        // The Addr returned is shared by all invocations of Addr, so
        // do not modify it.
        private static Addr Addr(this ref UnixListener l)
        {
            return l.fd.laddr;
        }

        // SetDeadline sets the deadline associated with the listener.
        // A zero time value disables the deadline.
        private static error SetDeadline(this ref UnixListener l, time.Time t)
        {
            if (!l.ok())
            {
                return error.As(syscall.EINVAL);
            }
            {
                var err = l.fd.pfd.SetDeadline(t);

                if (err != null)
                {
                    return error.As(ref new OpError(Op:"set",Net:l.fd.net,Source:nil,Addr:l.fd.laddr,Err:err));
                }

            }
            return error.As(null);
        }

        // File returns a copy of the underlying os.File, set to blocking
        // mode. It is the caller's responsibility to close f when finished.
        // Closing l does not affect f, and closing f does not affect l.
        //
        // The returned os.File's file descriptor is different from the
        // connection's. Attempting to change properties of the original
        // using this duplicate may or may not have the desired effect.
        private static (ref os.File, error) File(this ref UnixListener l)
        {
            if (!l.ok())
            {
                return (null, syscall.EINVAL);
            }
            f, err = l.file();
            if (err != null)
            {
                err = ref new OpError(Op:"file",Net:l.fd.net,Source:nil,Addr:l.fd.laddr,Err:err);
            }
            return;
        }

        // ListenUnix acts like Listen for Unix networks.
        //
        // The network must be "unix" or "unixpacket".
        public static (ref UnixListener, error) ListenUnix(@string network, ref UnixAddr laddr)
        {
            switch (network)
            {
                case "unix": 

                case "unixpacket": 
                    break;
                default: 
                    return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:UnknownNetworkError(network)));
                    break;
            }
            if (laddr == null)
            {
                return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:errMissingAddress));
            }
            var (ln, err) = listenUnix(context.Background(), network, laddr);
            if (err != null)
            {
                return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:err));
            }
            return (ln, null);
        }

        // ListenUnixgram acts like ListenPacket for Unix networks.
        //
        // The network must be "unixgram".
        public static (ref UnixConn, error) ListenUnixgram(@string network, ref UnixAddr laddr)
        {
            switch (network)
            {
                case "unixgram": 
                    break;
                default: 
                    return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:UnknownNetworkError(network)));
                    break;
            }
            if (laddr == null)
            {
                return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:nil,Err:errMissingAddress));
            }
            var (c, err) = listenUnixgram(context.Background(), network, laddr);
            if (err != null)
            {
                return (null, ref new OpError(Op:"listen",Net:network,Source:nil,Addr:laddr.opAddr(),Err:err));
            }
            return (c, null);
        }
    }
}
