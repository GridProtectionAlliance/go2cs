// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package net -- go2cs converted at 2020 August 29 08:26:16 UTC
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
        private static (@string, error) status(this ref netFD _fd, long ln) => func(_fd, (ref netFD fd, Defer defer, Panic _, Recover __) =>
        {
            if (!fd.ok())
            {
                return ("", syscall.EINVAL);
            }
            var (status, err) = os.Open(fd.dir + "/status");
            if (err != null)
            {
                return ("", err);
            }
            defer(status.Close());
            var buf = make_slice<byte>(ln);
            var (n, err) = io.ReadFull(status, buf[..]);
            if (err != null)
            {
                return ("", err);
            }
            return (string(buf[..n]), null);
        });

        private static (ref netFD, error) newFileFD(ref os.File _f) => func(_f, (ref os.File f, Defer defer, Panic _, Recover __) =>
        {
            ref os.File ctl = default;
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
                return (null, os.NewSyscallError("fd2path", err));
            }
            var comp = splitAtBytes(path, "/");
            var n = len(comp);
            if (n < 3L || comp[0L][0L..3L] != "net")
            {
                return (null, syscall.EPLAN9);
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
                            return (null, os.NewSyscallError("dup", err));
                        }
                        defer(close(fd));

                        var dir = netdir + "/" + comp[n - 2L];
                        ctl = os.NewFile(uintptr(fd), dir + "/" + file);
                        ctl.Seek(0L, io.SeekStart);
                        array<byte> buf = new array<byte>(16L);
                        var (n, err) = ctl.Read(buf[..]);
                        if (err != null)
                        {
                            return (null, err);
                        }
                        name = string(buf[..n]);
                        break;
                    default: 
                        if (len(comp) < 4L)
                        {
                            return (null, errors.New("could not find control file for connection"));
                        }
                        dir = netdir + "/" + comp[1L] + "/" + name;
                        ctl, err = os.OpenFile(dir + "/ctl", os.O_RDWR, 0L);
                        if (err != null)
                        {
                            return (null, err);
                        }
                        defer(close(int(ctl.Fd())));
                        break;
                }
            }
            dir = netdir + "/" + comp[1L] + "/" + name;
            var (laddr, err) = readPlan9Addr(comp[1L], dir + "/local");
            if (err != null)
            {
                return (null, err);
            }
            return newFD(comp[1L], name, null, ctl, null, laddr, null);
        });

        private static (Conn, error) fileConn(ref os.File f)
        {
            var (fd, err) = newFileFD(f);
            if (err != null)
            {
                return (null, err);
            }
            if (!fd.ok())
            {
                return (null, syscall.EINVAL);
            }
            fd.data, err = os.OpenFile(fd.dir + "/data", os.O_RDWR, 0L);
            if (err != null)
            {
                return (null, err);
            }
            switch (fd.laddr.type())
            {
                case ref TCPAddr _:
                    return (newTCPConn(fd), null);
                    break;
                case ref UDPAddr _:
                    return (newUDPConn(fd), null);
                    break;
            }
            return (null, syscall.EPLAN9);
        }

        private static (Listener, error) fileListener(ref os.File f)
        {
            var (fd, err) = newFileFD(f);
            if (err != null)
            {
                return (null, err);
            }
            switch (fd.laddr.type())
            {
                case ref TCPAddr _:
                    break;
                default:
                {
                    return (null, syscall.EPLAN9);
                    break;
                } 

                // check that file corresponds to a listener
            } 

            // check that file corresponds to a listener
            var (s, err) = fd.status(len("Listen"));
            if (err != null)
            {
                return (null, err);
            }
            if (s != "Listen")
            {
                return (null, errors.New("file does not represent a listener"));
            }
            return (ref new TCPListener(fd), null);
        }

        private static (PacketConn, error) filePacketConn(ref os.File f)
        {
            return (null, syscall.EPLAN9);
        }
    }
}
