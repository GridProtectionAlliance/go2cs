// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:13 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\fd_windows.go
using context = go.context_package;
using poll = go.@internal.poll_package;
using os = go.os_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class net_package
    {
        // canUseConnectEx reports whether we can use the ConnectEx Windows API call
        // for the given network type.
        private static bool canUseConnectEx(@string net)
        {
            switch (net)
            {
                case "tcp": 

                case "tcp4": 

                case "tcp6": 
                    return true;
                    break;
            } 
            // ConnectEx windows API does not support connectionless sockets.
            return false;
        }

        // Network file descriptor.
        private partial struct netFD
        {
            public poll.FD pfd; // immutable until Close
            public long family;
            public long sotype;
            public bool isConnected;
            public @string net;
            public Addr laddr;
            public Addr raddr;
        }

        private static (ref netFD, error) newFD(syscall.Handle sysfd, long family, long sotype, @string net)
        {
            netFD ret = ref new netFD(pfd:poll.FD{Sysfd:sysfd,IsStream:sotype==syscall.SOCK_STREAM,ZeroReadIsEOF:sotype!=syscall.SOCK_DGRAM&&sotype!=syscall.SOCK_RAW,},family:family,sotype:sotype,net:net,);
            return (ret, null);
        }

        private static error init(this ref netFD fd)
        {
            var (errcall, err) = fd.pfd.Init(fd.net, true);
            if (errcall != "")
            {
                err = wrapSyscallError(errcall, err);
            }
            return error.As(err);
        }

        private static void setAddr(this ref netFD fd, Addr laddr, Addr raddr)
        {
            fd.laddr = laddr;
            fd.raddr = raddr;
            runtime.SetFinalizer(fd, ref netFD);
        }

        // Always returns nil for connected peer address result.
        private static (syscall.Sockaddr, error) connect(this ref netFD _fd, context.Context ctx, syscall.Sockaddr la, syscall.Sockaddr ra) => func(_fd, (ref netFD fd, Defer defer, Panic panic, Recover _) =>
        { 
            // Do not need to call fd.writeLock here,
            // because fd is not yet accessible to user,
            // so no concurrent operations are possible.
            {
                var err__prev1 = err;

                var err = fd.init();

                if (err != null)
                {
                    return (null, err);
                }

                err = err__prev1;

            }
            {
                var (deadline, ok) = ctx.Deadline();

                if (ok && !deadline.IsZero())
                {
                    fd.pfd.SetWriteDeadline(deadline);
                    defer(fd.pfd.SetWriteDeadline(noDeadline));
                }

            }
            if (!canUseConnectEx(fd.net))
            {
                err = connectFunc(fd.pfd.Sysfd, ra);
                return (null, os.NewSyscallError("connect", err));
            } 
            // ConnectEx windows API requires an unconnected, previously bound socket.
            if (la == null)
            {
                switch (ra.type())
                {
                    case ref syscall.SockaddrInet4 _:
                        la = ref new syscall.SockaddrInet4();
                        break;
                    case ref syscall.SockaddrInet6 _:
                        la = ref new syscall.SockaddrInet6();
                        break;
                    default:
                    {
                        panic("unexpected type in connect");
                        break;
                    }
                }
                {
                    var err__prev2 = err;

                    err = syscall.Bind(fd.pfd.Sysfd, la);

                    if (err != null)
                    {
                        return (null, os.NewSyscallError("bind", err));
                    }

                    err = err__prev2;

                }
            } 

            // Wait for the goroutine converting context.Done into a write timeout
            // to exist, otherwise our caller might cancel the context and
            // cause fd.setWriteDeadline(aLongTimeAgo) to cancel a successful dial.
            var done = make_channel<bool>(); // must be unbuffered
            defer(() =>
            {
                done.Send(true);

            }());
            go_(() => () =>
            {
                fd.pfd.SetWriteDeadline(aLongTimeAgo).Send(done);
            }()); 

            // Call ConnectEx API.
            {
                var err__prev1 = err;

                err = fd.pfd.ConnectEx(ra);

                if (err != null)
                {
                    return (null, mapErr(ctx.Err()));
                    {
                        syscall.Errno (_, ok) = err._<syscall.Errno>();

                        if (ok)
                        {
                            err = os.NewSyscallError("connectex", err);
                        }

                    }
                    return (null, err);
                } 
                // Refresh socket properties.

                err = err__prev1;

            } 
            // Refresh socket properties.
            return (null, os.NewSyscallError("setsockopt", syscall.Setsockopt(fd.pfd.Sysfd, syscall.SOL_SOCKET, syscall.SO_UPDATE_CONNECT_CONTEXT, (byte.Value)(@unsafe.Pointer(ref fd.pfd.Sysfd)), int32(@unsafe.Sizeof(fd.pfd.Sysfd)))));
        });

        private static error Close(this ref netFD fd)
        {
            runtime.SetFinalizer(fd, null);
            return error.As(fd.pfd.Close());
        }

        private static error shutdown(this ref netFD fd, long how)
        {
            var err = fd.pfd.Shutdown(how);
            runtime.KeepAlive(fd);
            return error.As(err);
        }

        private static error closeRead(this ref netFD fd)
        {
            return error.As(fd.shutdown(syscall.SHUT_RD));
        }

        private static error closeWrite(this ref netFD fd)
        {
            return error.As(fd.shutdown(syscall.SHUT_WR));
        }

        private static (long, error) Read(this ref netFD fd, slice<byte> buf)
        {
            var (n, err) = fd.pfd.Read(buf);
            runtime.KeepAlive(fd);
            return (n, wrapSyscallError("wsarecv", err));
        }

        private static (long, syscall.Sockaddr, error) readFrom(this ref netFD fd, slice<byte> buf)
        {
            var (n, sa, err) = fd.pfd.ReadFrom(buf);
            runtime.KeepAlive(fd);
            return (n, sa, wrapSyscallError("wsarecvfrom", err));
        }

        private static (long, error) Write(this ref netFD fd, slice<byte> buf)
        {
            var (n, err) = fd.pfd.Write(buf);
            runtime.KeepAlive(fd);
            return (n, wrapSyscallError("wsasend", err));
        }

        private static (long, error) writeBuffers(this ref conn c, ref Buffers v)
        {
            if (!c.ok())
            {
                return (0L, syscall.EINVAL);
            }
            var (n, err) = c.fd.writeBuffers(v);
            if (err != null)
            {
                return (n, ref new OpError(Op:"wsasend",Net:c.fd.net,Source:c.fd.laddr,Addr:c.fd.raddr,Err:err));
            }
            return (n, null);
        }

        private static (long, error) writeBuffers(this ref netFD fd, ref Buffers buf)
        {
            var (n, err) = fd.pfd.Writev(new ptr<ref slice<slice<byte>>>(buf));
            runtime.KeepAlive(fd);
            return (n, wrapSyscallError("wsasend", err));
        }

        private static (long, error) writeTo(this ref netFD fd, slice<byte> buf, syscall.Sockaddr sa)
        {
            var (n, err) = fd.pfd.WriteTo(buf, sa);
            runtime.KeepAlive(fd);
            return (n, wrapSyscallError("wsasendto", err));
        }

        private static (ref netFD, error) accept(this ref netFD fd)
        {
            var (s, rawsa, rsan, errcall, err) = fd.pfd.Accept(() =>
            {
                return sysSocket(fd.family, fd.sotype, 0L);
            });

            if (err != null)
            {
                if (errcall != "")
                {
                    err = wrapSyscallError(errcall, err);
                }
                return (null, err);
            } 

            // Associate our new socket with IOCP.
            var (netfd, err) = newFD(s, fd.family, fd.sotype, fd.net);
            if (err != null)
            {
                poll.CloseFunc(s);
                return (null, err);
            }
            {
                var err = netfd.init();

                if (err != null)
                {
                    fd.Close();
                    return (null, err);
                } 

                // Get local and peer addr out of AcceptEx buffer.

            } 

            // Get local and peer addr out of AcceptEx buffer.
            ref syscall.RawSockaddrAny lrsa = default;            ref syscall.RawSockaddrAny rrsa = default;

            int llen = default;            int rlen = default;

            syscall.GetAcceptExSockaddrs((byte.Value)(@unsafe.Pointer(ref rawsa[0L])), 0L, rsan, rsan, ref lrsa, ref llen, ref rrsa, ref rlen);
            var (lsa, _) = lrsa.Sockaddr();
            var (rsa, _) = rrsa.Sockaddr();

            netfd.setAddr(netfd.addrFunc()(lsa), netfd.addrFunc()(rsa));
            return (netfd, null);
        }

        private static (long, long, long, syscall.Sockaddr, error) readMsg(this ref netFD fd, slice<byte> p, slice<byte> oob)
        {
            n, oobn, flags, sa, err = fd.pfd.ReadMsg(p, oob);
            runtime.KeepAlive(fd);
            return (n, oobn, flags, sa, wrapSyscallError("wsarecvmsg", err));
        }

        private static (long, long, error) writeMsg(this ref netFD fd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa)
        {
            n, oobn, err = fd.pfd.WriteMsg(p, oob, sa);
            runtime.KeepAlive(fd);
            return (n, oobn, wrapSyscallError("wsasendmsg", err));
        }

        // Unimplemented functions.

        private static (ref os.File, error) dup(this ref netFD fd)
        { 
            // TODO: Implement this
            return (null, syscall.EWINDOWS);
        }
    }
}
