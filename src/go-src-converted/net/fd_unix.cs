// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package net -- go2cs converted at 2020 August 29 08:26:10 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\fd_unix.go
using context = go.context_package;
using poll = go.@internal.poll_package;
using os = go.os_package;
using runtime = go.runtime_package;
using atomic = go.sync.atomic_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class net_package
    {
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

        private static (ref netFD, error) newFD(long sysfd, long family, long sotype, @string net)
        {
            netFD ret = ref new netFD(pfd:poll.FD{Sysfd:sysfd,IsStream:sotype==syscall.SOCK_STREAM,ZeroReadIsEOF:sotype!=syscall.SOCK_DGRAM&&sotype!=syscall.SOCK_RAW,},family:family,sotype:sotype,net:net,);
            return (ret, null);
        }

        private static error init(this ref netFD fd)
        {
            return error.As(fd.pfd.Init(fd.net, true));
        }

        private static void setAddr(this ref netFD fd, Addr laddr, Addr raddr)
        {
            fd.laddr = laddr;
            fd.raddr = raddr;
            runtime.SetFinalizer(fd, ref netFD);
        }

        private static @string name(this ref netFD fd)
        {
            @string ls = default;            @string rs = default;

            if (fd.laddr != null)
            {
                ls = fd.laddr.String();
            }
            if (fd.raddr != null)
            {
                rs = fd.raddr.String();
            }
            return fd.net + ":" + ls + "->" + rs;
        }

        private static (syscall.Sockaddr, error) connect(this ref netFD _fd, context.Context ctx, syscall.Sockaddr la, syscall.Sockaddr ra) => func(_fd, (ref netFD fd, Defer defer, Panic _, Recover __) =>
        { 
            // Do not need to call fd.writeLock here,
            // because fd is not yet accessible to user,
            // so no concurrent operations are possible.
            {
                var err__prev1 = err;

                var err = connectFunc(fd.pfd.Sysfd, ra);


                if (err == syscall.EINPROGRESS || err == syscall.EALREADY || err == syscall.EINTR)
                {
                    goto __switch_break0;
                }
                if (err == null || err == syscall.EISCONN)
                {
                    return (null, mapErr(ctx.Err()));
                    {
                        var err__prev1 = err;

                        err = fd.pfd.Init(fd.net, true);

                        if (err != null)
                        {
                            return (null, err);
                        }

                        err = err__prev1;

                    }
                    runtime.KeepAlive(fd);
                    return (null, null);
                    goto __switch_break0;
                }
                if (err == syscall.EINVAL) 
                {
                    // On Solaris we can see EINVAL if the socket has
                    // already been accepted and closed by the server.
                    // Treat this as a successful connection--writes to
                    // the socket will see EOF.  For details and a test
                    // case in C see https://golang.org/issue/6828.
                    if (runtime.GOOS == "solaris")
                    {
                        return (null, null);
                    }
                }
                // default: 
                    return (null, os.NewSyscallError("connect", err));

                __switch_break0:;

                err = err__prev1;
            }
            {
                var err__prev1 = err;

                err = fd.pfd.Init(fd.net, true);

                if (err != null)
                {
                    return (null, err);
                }

                err = err__prev1;

            }
            {
                var (deadline, _) = ctx.Deadline();

                if (!deadline.IsZero())
                {
                    fd.pfd.SetWriteDeadline(deadline);
                    defer(fd.pfd.SetWriteDeadline(noDeadline));
                } 

                // Start the "interrupter" goroutine, if this context might be canceled.
                // (The background context cannot)
                //
                // The interrupter goroutine waits for the context to be done and
                // interrupts the dial (by altering the fd's write deadline, which
                // wakes up waitWrite).

            } 

            // Start the "interrupter" goroutine, if this context might be canceled.
            // (The background context cannot)
            //
            // The interrupter goroutine waits for the context to be done and
            // interrupts the dial (by altering the fd's write deadline, which
            // wakes up waitWrite).
            if (ctx != context.Background())
            { 
                // Wait for the interrupter goroutine to exit before returning
                // from connect.
                var done = make_channel<object>();
                var interruptRes = make_channel<error>();
                defer(() =>
                {
                    close(done);
                    {
                        var ctxErr = interruptRes.Receive();

                        if (ctxErr != null && ret == null)
                        { 
                            // The interrupter goroutine called SetWriteDeadline,
                            // but the connect code below had returned from
                            // waitWrite already and did a successful connect (ret
                            // == nil). Because we've now poisoned the connection
                            // by making it unwritable, don't return a successful
                            // dial. This was issue 16523.
                            ret = ctxErr;
                            fd.Close(); // prevent a leak
                        }

                    }
                }());
                go_(() => () =>
                {
                    fd.pfd.SetWriteDeadline(aLongTimeAgo);
                    testHookCanceledDial();
                    interruptRes.Send(ctx.Err());
                    interruptRes.Send(null);
                }());
            }
            while (true)
            { 
                // Performing multiple connect system calls on a
                // non-blocking socket under Unix variants does not
                // necessarily result in earlier errors being
                // returned. Instead, once runtime-integrated network
                // poller tells us that the socket is ready, get the
                // SO_ERROR socket option to see if the connection
                // succeeded or failed. See issue 7474 for further
                // details.
                {
                    var err__prev1 = err;

                    err = fd.pfd.WaitWrite();

                    if (err != null)
                    {
                        return (null, mapErr(ctx.Err()));
                        return (null, err);
                    }

                    err = err__prev1;

                }
                var (nerr, err) = getsockoptIntFunc(fd.pfd.Sysfd, syscall.SOL_SOCKET, syscall.SO_ERROR);
                if (err != null)
                {
                    return (null, os.NewSyscallError("getsockopt", err));
                }
                {
                    var err__prev1 = err;

                    err = syscall.Errno(nerr);


                    if (err == syscall.EINPROGRESS || err == syscall.EALREADY || err == syscall.EINTR)                     else if (err == syscall.EISCONN) 
                        return (null, null);
                    else if (err == syscall.Errno(0L)) 
                        // The runtime poller can wake us up spuriously;
                        // see issues 14548 and 19289. Check that we are
                        // really connected; if not, wait again.
                        {
                            var err__prev1 = err;

                            var (rsa, err) = syscall.Getpeername(fd.pfd.Sysfd);

                            if (err == null)
                            {
                                return (rsa, null);
                            }

                            err = err__prev1;

                        }
                    else 
                        return (null, os.NewSyscallError("connect", err));


                    err = err__prev1;
                }
                runtime.KeepAlive(fd);
            }

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
            return error.As(wrapSyscallError("shutdown", err));
        }

        private static error closeRead(this ref netFD fd)
        {
            return error.As(fd.shutdown(syscall.SHUT_RD));
        }

        private static error closeWrite(this ref netFD fd)
        {
            return error.As(fd.shutdown(syscall.SHUT_WR));
        }

        private static (long, error) Read(this ref netFD fd, slice<byte> p)
        {
            n, err = fd.pfd.Read(p);
            runtime.KeepAlive(fd);
            return (n, wrapSyscallError("read", err));
        }

        private static (long, syscall.Sockaddr, error) readFrom(this ref netFD fd, slice<byte> p)
        {
            n, sa, err = fd.pfd.ReadFrom(p);
            runtime.KeepAlive(fd);
            return (n, sa, wrapSyscallError("recvfrom", err));
        }

        private static (long, long, long, syscall.Sockaddr, error) readMsg(this ref netFD fd, slice<byte> p, slice<byte> oob)
        {
            n, oobn, flags, sa, err = fd.pfd.ReadMsg(p, oob);
            runtime.KeepAlive(fd);
            return (n, oobn, flags, sa, wrapSyscallError("recvmsg", err));
        }

        private static (long, error) Write(this ref netFD fd, slice<byte> p)
        {
            nn, err = fd.pfd.Write(p);
            runtime.KeepAlive(fd);
            return (nn, wrapSyscallError("write", err));
        }

        private static (long, error) writeTo(this ref netFD fd, slice<byte> p, syscall.Sockaddr sa)
        {
            n, err = fd.pfd.WriteTo(p, sa);
            runtime.KeepAlive(fd);
            return (n, wrapSyscallError("sendto", err));
        }

        private static (long, long, error) writeMsg(this ref netFD fd, slice<byte> p, slice<byte> oob, syscall.Sockaddr sa)
        {
            n, oobn, err = fd.pfd.WriteMsg(p, oob, sa);
            runtime.KeepAlive(fd);
            return (n, oobn, wrapSyscallError("sendmsg", err));
        }

        private static (ref netFD, error) accept(this ref netFD fd)
        {
            var (d, rsa, errcall, err) = fd.pfd.Accept();
            if (err != null)
            {
                if (errcall != "")
                {
                    err = wrapSyscallError(errcall, err);
                }
                return (null, err);
            }
            netfd, err = newFD(d, fd.family, fd.sotype, fd.net);

            if (err != null)
            {
                poll.CloseFunc(d);
                return (null, err);
            }
            err = netfd.init();

            if (err != null)
            {
                fd.Close();
                return (null, err);
            }
            var (lsa, _) = syscall.Getsockname(netfd.pfd.Sysfd);
            netfd.setAddr(netfd.addrFunc()(lsa), netfd.addrFunc()(rsa));
            return (netfd, null);
        }

        // tryDupCloexec indicates whether F_DUPFD_CLOEXEC should be used.
        // If the kernel doesn't support it, this is set to 0.
        private static var tryDupCloexec = int32(1L);

        private static (long, error) dupCloseOnExec(long fd)
        {
            if (atomic.LoadInt32(ref tryDupCloexec) == 1L)
            {
                var (r0, _, e1) = syscall.Syscall(syscall.SYS_FCNTL, uintptr(fd), syscall.F_DUPFD_CLOEXEC, 0L);
                if (runtime.GOOS == "darwin" && e1 == syscall.EBADF)
                { 
                    // On OS X 10.6 and below (but we only support
                    // >= 10.6), F_DUPFD_CLOEXEC is unsupported
                    // and fcntl there falls back (undocumented)
                    // to doing an ioctl instead, returning EBADF
                    // in this case because fd is not of the
                    // expected device fd type. Treat it as
                    // EINVAL instead, so we fall back to the
                    // normal dup path.
                    // TODO: only do this on 10.6 if we can detect 10.6
                    // cheaply.
                    e1 = syscall.EINVAL;
                }

                if (e1 == 0L) 
                    return (int(r0), null);
                else if (e1 == syscall.EINVAL) 
                    // Old kernel. Fall back to the portable way
                    // from now on.
                    atomic.StoreInt32(ref tryDupCloexec, 0L);
                else 
                    return (-1L, os.NewSyscallError("fcntl", e1));
                            }
            return dupCloseOnExecOld(fd);
        }

        // dupCloseOnExecUnixOld is the traditional way to dup an fd and
        // set its O_CLOEXEC bit, using two system calls.
        private static (long, error) dupCloseOnExecOld(long fd) => func((defer, _, __) =>
        {
            syscall.ForkLock.RLock();
            defer(syscall.ForkLock.RUnlock());
            newfd, err = syscall.Dup(fd);
            if (err != null)
            {
                return (-1L, os.NewSyscallError("dup", err));
            }
            syscall.CloseOnExec(newfd);
            return;
        });

        private static (ref os.File, error) dup(this ref netFD fd)
        {
            var (ns, err) = dupCloseOnExec(fd.pfd.Sysfd);
            if (err != null)
            {
                return (null, err);
            } 

            // We want blocking mode for the new fd, hence the double negative.
            // This also puts the old fd into blocking mode, meaning that
            // I/O will block the thread instead of letting us use the epoll server.
            // Everything will still work, just with more threads.
            err = fd.pfd.SetBlocking();

            if (err != null)
            {
                return (null, os.NewSyscallError("setnonblock", err));
            }
            return (os.NewFile(uintptr(ns), fd.name()), null);
        }
    }
}
