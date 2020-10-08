// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 08 03:31:45 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\fd_plan9.go
using poll = go.@internal.poll_package;
using io = go.io_package;
using os = go.os_package;
using syscall = go.syscall_package;
using time = go.time_package;
using static go.builtin;

namespace go
{
    public static partial class net_package
    {
        // Network file descriptor.
        private partial struct netFD
        {
            public poll.FD pfd; // immutable until Close
            public @string net;
            public @string n;
            public @string dir;
            public ptr<os.File> listen;
            public ptr<os.File> ctl;
            public ptr<os.File> data;
            public Addr laddr;
            public Addr raddr;
            public bool isStream;
        }

        private static @string netdir = "/net"; // default network

        private static (ptr<netFD>, error) newFD(@string net, @string name, ptr<os.File> _addr_listen, ptr<os.File> _addr_ctl, ptr<os.File> _addr_data, Addr laddr, Addr raddr)
        {
            ptr<netFD> _p0 = default!;
            error _p0 = default!;
            ref os.File listen = ref _addr_listen.val;
            ref os.File ctl = ref _addr_ctl.val;
            ref os.File data = ref _addr_data.val;

            ptr<netFD> ret = addr(new netFD(net:net,n:name,dir:netdir+"/"+net+"/"+name,listen:listen,ctl:ctl,data:data,laddr:laddr,raddr:raddr,));
            ret.pfd.Destroy = ret.destroy;
            return (_addr_ret!, error.As(null!)!);
        }

        private static error init(this ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;
 
            // stub for future fd.pd.Init(fd)
            return error.As(null!)!;

        }

        private static @string name(this ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

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

        private static bool ok(this ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

            return fd != null && fd.ctl != null;
        }

        private static void destroy(this ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

            if (!fd.ok())
            {
                return ;
            }

            var err = fd.ctl.Close();
            if (fd.data != null)
            {
                {
                    var err1__prev2 = err1;

                    var err1 = fd.data.Close();

                    if (err1 != null && err == null)
                    {
                        err = err1;
                    }

                    err1 = err1__prev2;

                }

            }

            if (fd.listen != null)
            {
                {
                    var err1__prev2 = err1;

                    err1 = fd.listen.Close();

                    if (err1 != null && err == null)
                    {
                        err = err1;
                    }

                    err1 = err1__prev2;

                }

            }

            fd.ctl = null;
            fd.data = null;
            fd.listen = null;

        }

        private static (long, error) Read(this ptr<netFD> _addr_fd, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref netFD fd = ref _addr_fd.val;

            if (!fd.ok() || fd.data == null)
            {
                return (0L, error.As(syscall.EINVAL)!);
            }

            n, err = fd.pfd.Read(fd.data.Read, b);
            if (fd.net == "udp" && err == io.EOF)
            {
                n = 0L;
                err = null;
            }

            return ;

        }

        private static (long, error) Write(this ptr<netFD> _addr_fd, slice<byte> b)
        {
            long n = default;
            error err = default!;
            ref netFD fd = ref _addr_fd.val;

            if (!fd.ok() || fd.data == null)
            {
                return (0L, error.As(syscall.EINVAL)!);
            }

            return fd.pfd.Write(fd.data.Write, b);

        }

        private static error closeRead(this ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

            if (!fd.ok())
            {
                return error.As(syscall.EINVAL)!;
            }

            return error.As(syscall.EPLAN9)!;

        }

        private static error closeWrite(this ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

            if (!fd.ok())
            {
                return error.As(syscall.EINVAL)!;
            }

            return error.As(syscall.EPLAN9)!;

        }

        private static error Close(this ptr<netFD> _addr_fd)
        {
            ref netFD fd = ref _addr_fd.val;

            {
                var err__prev1 = err;

                var err = fd.pfd.Close();

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            if (!fd.ok())
            {
                return error.As(syscall.EINVAL)!;
            }

            if (fd.net == "tcp")
            { 
                // The following line is required to unblock Reads.
                var (_, err) = fd.ctl.WriteString("close");
                if (err != null)
                {
                    return error.As(err)!;
                }

            }

            err = fd.ctl.Close();
            if (fd.data != null)
            {
                {
                    var err1__prev2 = err1;

                    var err1 = fd.data.Close();

                    if (err1 != null && err == null)
                    {
                        err = err1;
                    }

                    err1 = err1__prev2;

                }

            }

            if (fd.listen != null)
            {
                {
                    var err1__prev2 = err1;

                    err1 = fd.listen.Close();

                    if (err1 != null && err == null)
                    {
                        err = err1;
                    }

                    err1 = err1__prev2;

                }

            }

            fd.ctl = null;
            fd.data = null;
            fd.listen = null;
            return error.As(err)!;

        }

        // This method is only called via Conn.
        private static (ptr<os.File>, error) dup(this ptr<netFD> _addr_fd)
        {
            ptr<os.File> _p0 = default!;
            error _p0 = default!;
            ref netFD fd = ref _addr_fd.val;

            if (!fd.ok() || fd.data == null)
            {
                return (_addr_null!, error.As(syscall.EINVAL)!);
            }

            return _addr_fd.file(fd.data, fd.dir + "/data")!;

        }

        private static (ptr<os.File>, error) dup(this ptr<TCPListener> _addr_l)
        {
            ptr<os.File> _p0 = default!;
            error _p0 = default!;
            ref TCPListener l = ref _addr_l.val;

            if (!l.fd.ok())
            {
                return (_addr_null!, error.As(syscall.EINVAL)!);
            }

            return _addr_l.fd.file(l.fd.ctl, l.fd.dir + "/ctl")!;

        }

        private static (ptr<os.File>, error) file(this ptr<netFD> _addr_fd, ptr<os.File> _addr_f, @string s)
        {
            ptr<os.File> _p0 = default!;
            error _p0 = default!;
            ref netFD fd = ref _addr_fd.val;
            ref os.File f = ref _addr_f.val;

            var (dfd, err) = syscall.Dup(int(f.Fd()), -1L);
            if (err != null)
            {
                return (_addr_null!, error.As(os.NewSyscallError("dup", err))!);
            }

            return (_addr_os.NewFile(uintptr(dfd), s)!, error.As(null!)!);

        }

        private static error setReadBuffer(ptr<netFD> _addr_fd, long bytes)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(syscall.EPLAN9)!;
        }

        private static error setWriteBuffer(ptr<netFD> _addr_fd, long bytes)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(syscall.EPLAN9)!;
        }

        private static error SetDeadline(this ptr<netFD> _addr_fd, time.Time t)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(fd.pfd.SetDeadline(t))!;
        }

        private static error SetReadDeadline(this ptr<netFD> _addr_fd, time.Time t)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(fd.pfd.SetReadDeadline(t))!;
        }

        private static error SetWriteDeadline(this ptr<netFD> _addr_fd, time.Time t)
        {
            ref netFD fd = ref _addr_fd.val;

            return error.As(fd.pfd.SetWriteDeadline(t))!;
        }
    }
}
