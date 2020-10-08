// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 October 08 03:33:02 UTC
// import "net" ==> using net = go.net_package
// Original source: C:\Go\src\net\file_plan9.go
using errors = go.errors_package;
using io = go.io_package;
using os = go.os_package;
using syscall = go.syscall_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class net_package
    {
        private static (@string, error) status(this ptr<netFD> _addr_fd, long ln) => func((defer, _, __) =>
        {
            @string _p0 = default;
            error _p0 = default!;
            ref netFD fd = ref _addr_fd.val;

            if (!fd.ok())
            {
                return ("", error.As(syscall.EINVAL)!);
            }
            var (status, err) = os.Open(fd.dir + "/status");
            if (err != null)
            {
                return ("", error.As(err)!);
            }
            defer(status.Close());
            var buf = make_slice<byte>(ln);
            var (n, err) = io.ReadFull(status, buf[..]);
            if (err != null)
            {
                return ("", error.As(err)!);
            }
            return (string(buf[..n]), error.As(null!)!);

        });

        private static (ptr<netFD>, error) newFileFD(ptr<os.File> _addr_f) => func((defer, _, __) =>
        {
            ptr<netFD> net = default!;
            error err = default!;
            ref os.File f = ref _addr_f.val;

            ptr<os.File> ctl;
            Action<long> close = fd =>
            {
                if (err != null)
                {
                    syscall.Close(fd);
                }

            }
;

            var (path, err) = syscall.Fd2path(int(f.Fd()));
            if (err != null)
            {
                return (_addr_null!, error.As(os.NewSyscallError("fd2path", err))!);
            }

            var comp = splitAtBytes(path, "/");
            var n = len(comp);
            if (n < 3L || comp[0L][0L..3L] != "net")
            {
                return (_addr_null!, error.As(syscall.EPLAN9)!);
            }

            var name = comp[2L];
            {
                var file = comp[n - 1L];

                switch (file)
                {
                    case "ctl": 

                    case "clone": 
                        var (fd, err) = syscall.Dup(int(f.Fd()), -1L);
                        if (err != null)
                        {
                            return (_addr_null!, error.As(os.NewSyscallError("dup", err))!);
                        }

                        defer(close(fd));

                        var dir = netdir + "/" + comp[n - 2L];
                        ctl = os.NewFile(uintptr(fd), dir + "/" + file);
                        ctl.Seek(0L, io.SeekStart);
                        array<byte> buf = new array<byte>(16L);
                        var (n, err) = ctl.Read(buf[..]);
                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        name = string(buf[..n]);
                        break;
                    default: 
                        if (len(comp) < 4L)
                        {
                            return (_addr_null!, error.As(errors.New("could not find control file for connection"))!);
                        }

                        dir = netdir + "/" + comp[1L] + "/" + name;
                        ctl, err = os.OpenFile(dir + "/ctl", os.O_RDWR, 0L);
                        if (err != null)
                        {
                            return (_addr_null!, error.As(err)!);
                        }

                        defer(close(int(ctl.Fd())));
                        break;
                }
            }
            dir = netdir + "/" + comp[1L] + "/" + name;
            var (laddr, err) = readPlan9Addr(comp[1L], dir + "/local");
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            return _addr_newFD(comp[1L], name, null, ctl, null, laddr, null)!;

        });

        private static (Conn, error) fileConn(ptr<os.File> _addr_f)
        {
            Conn _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            var (fd, err) = newFileFD(_addr_f);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (!fd.ok())
            {
                return (null, error.As(syscall.EINVAL)!);
            }

            fd.data, err = os.OpenFile(fd.dir + "/data", os.O_RDWR, 0L);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            switch (fd.laddr.type())
            {
                case ptr<TCPAddr> _:
                    return (newTCPConn(fd), error.As(null!)!);
                    break;
                case ptr<UDPAddr> _:
                    return (newUDPConn(fd), error.As(null!)!);
                    break;
            }
            return (null, error.As(syscall.EPLAN9)!);

        }

        private static (Listener, error) fileListener(ptr<os.File> _addr_f)
        {
            Listener _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            var (fd, err) = newFileFD(_addr_f);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            switch (fd.laddr.type())
            {
                case ptr<TCPAddr> _:
                    break;
                default:
                {
                    return (null, error.As(syscall.EPLAN9)!);
                    break;
                } 

                // check that file corresponds to a listener
            } 

            // check that file corresponds to a listener
            var (s, err) = fd.status(len("Listen"));
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (s != "Listen")
            {
                return (null, error.As(errors.New("file does not represent a listener"))!);
            }

            return (addr(new TCPListener(fd:fd)), error.As(null!)!);

        }

        private static (PacketConn, error) filePacketConn(ptr<os.File> _addr_f)
        {
            PacketConn _p0 = default;
            error _p0 = default!;
            ref os.File f = ref _addr_f.val;

            return (null, error.As(syscall.EPLAN9)!);
        }
    }
}
