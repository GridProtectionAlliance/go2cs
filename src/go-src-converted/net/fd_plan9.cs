// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:07 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\fd_plan9.go
using poll = go.@internal.poll_package;
using io = go.io_package;
using os = go.os_package;
using syscall = go.syscall_package;
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

        private static (ref netFD, error) newFD(@string net, @string name, ref os.File listen, ref os.File ctl, ref os.File data, Addr laddr, Addr raddr)
        {
            netFD ret = ref new netFD(net:net,n:name,dir:netdir+"/"+net+"/"+name,listen:listen,ctl:ctl,data:data,laddr:laddr,raddr:raddr,);
            ret.pfd.Destroy = ret.destroy;
            return (ret, null);
        }

        private static error init(this ref netFD fd)
        { 
            // stub for future fd.pd.Init(fd)
            return error.As(null);
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

        private static bool ok(this ref netFD fd)
        {
            return fd != null && fd.ctl != null;
        }

        private static void destroy(this ref netFD fd)
        {
            if (!fd.ok())
            {
                return;
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

        private static (long, error) Read(this ref netFD fd, slice<byte> b)
        {
            if (!fd.ok() || fd.data == null)
            {
                return (0L, syscall.EINVAL);
            }
            n, err = fd.pfd.Read(fd.data.Read, b);
            if (fd.net == "udp" && err == io.EOF)
            {
                n = 0L;
                err = null;
            }
            return;
        }

        private static (long, error) Write(this ref netFD fd, slice<byte> b)
        {
            if (!fd.ok() || fd.data == null)
            {
                return (0L, syscall.EINVAL);
            }
            return fd.pfd.Write(fd.data.Write, b);
        }

        private static error closeRead(this ref netFD fd)
        {
            if (!fd.ok())
            {
                return error.As(syscall.EINVAL);
            }
            return error.As(syscall.EPLAN9);
        }

        private static error closeWrite(this ref netFD fd)
        {
            if (!fd.ok())
            {
                return error.As(syscall.EINVAL);
            }
            return error.As(syscall.EPLAN9);
        }

        private static error Close(this ref netFD fd)
        {
            {
                var err__prev1 = err;

                var err = fd.pfd.Close();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }
            if (!fd.ok())
            {
                return error.As(syscall.EINVAL);
            }
            if (fd.net == "tcp")
            { 
                // The following line is required to unblock Reads.
                var (_, err) = fd.ctl.WriteString("close");
                if (err != null)
                {
                    return error.As(err);
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
            return error.As(err);
        }

        // This method is only called via Conn.
        private static (ref os.File, error) dup(this ref netFD fd)
        {
            if (!fd.ok() || fd.data == null)
            {
                return (null, syscall.EINVAL);
            }
            return fd.file(fd.data, fd.dir + "/data");
        }

        private static (ref os.File, error) dup(this ref TCPListener l)
        {
            if (!l.fd.ok())
            {
                return (null, syscall.EINVAL);
            }
            return l.fd.file(l.fd.ctl, l.fd.dir + "/ctl");
        }

        private static (ref os.File, error) file(this ref netFD fd, ref os.File f, @string s)
        {
            var (dfd, err) = syscall.Dup(int(f.Fd()), -1L);
            if (err != null)
            {
                return (null, os.NewSyscallError("dup", err));
            }
            return (os.NewFile(uintptr(dfd), s), null);
        }

        private static error setReadBuffer(ref netFD fd, long bytes)
        {
            return error.As(syscall.EPLAN9);
        }

        private static error setWriteBuffer(ref netFD fd, long bytes)
        {
            return error.As(syscall.EPLAN9);
        }
    }
}
