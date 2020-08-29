// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Linux system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and
// wrap it in our own nicer implementation.

// package syscall -- go2cs converted at 2020 August 29 08:38:01 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_linux.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        /*
         * Wrapped
         */
        public static error Access(@string path, uint mode)
        {
            return error.As(Faccessat(_AT_FDCWD, path, mode, 0L));
        }

        public static error Chmod(@string path, uint mode)
        {
            return error.As(Fchmodat(_AT_FDCWD, path, mode, 0L));
        }

        public static error Chown(@string path, long uid, long gid)
        {
            return error.As(Fchownat(_AT_FDCWD, path, uid, gid, 0L));
        }

        public static (long, error) Creat(@string path, uint mode)
        {
            return Open(path, O_CREAT | O_WRONLY | O_TRUNC, mode);
        }

        //sys    linkat(olddirfd int, oldpath string, newdirfd int, newpath string, flags int) (err error)

        public static error Link(@string oldpath, @string newpath)
        {
            return error.As(linkat(_AT_FDCWD, oldpath, _AT_FDCWD, newpath, 0L));
        }

        public static error Mkdir(@string path, uint mode)
        {
            return error.As(Mkdirat(_AT_FDCWD, path, mode));
        }

        public static error Mknod(@string path, uint mode, long dev)
        {
            return error.As(Mknodat(_AT_FDCWD, path, mode, dev));
        }

        public static (long, error) Open(@string path, long mode, uint perm)
        {
            return openat(_AT_FDCWD, path, mode | O_LARGEFILE, perm);
        }

        //sys    openat(dirfd int, path string, flags int, mode uint32) (fd int, err error)

        public static (long, error) Openat(long dirfd, @string path, long flags, uint mode)
        {
            return openat(dirfd, path, flags | O_LARGEFILE, mode);
        }

        //sys    readlinkat(dirfd int, path string, buf []byte) (n int, err error)

        public static (long, error) Readlink(@string path, slice<byte> buf)
        {
            return readlinkat(_AT_FDCWD, path, buf);
        }

        public static error Rename(@string oldpath, @string newpath)
        {
            return error.As(Renameat(_AT_FDCWD, oldpath, _AT_FDCWD, newpath));
        }

        public static error Rmdir(@string path)
        {
            return error.As(unlinkat(_AT_FDCWD, path, _AT_REMOVEDIR));
        }

        //sys    symlinkat(oldpath string, newdirfd int, newpath string) (err error)

        public static error Symlink(@string oldpath, @string newpath)
        {
            return error.As(symlinkat(oldpath, _AT_FDCWD, newpath));
        }

        public static error Unlink(@string path)
        {
            return error.As(unlinkat(_AT_FDCWD, path, 0L));
        }

        //sys    unlinkat(dirfd int, path string, flags int) (err error)

        public static error Unlinkat(long dirfd, @string path)
        {
            return error.As(unlinkat(dirfd, path, 0L));
        }

        //sys    utimes(path string, times *[2]Timeval) (err error)

        public static error Utimes(@string path, slice<Timeval> tv)
        {
            if (len(tv) != 2L)
            {
                return error.As(EINVAL);
            }
            return error.As(utimes(path, new ptr<ref array<Timeval>>(@unsafe.Pointer(ref tv[0L]))));
        }

        //sys    utimensat(dirfd int, path string, times *[2]Timespec, flag int) (err error)

        public static error UtimesNano(@string path, slice<Timespec> ts)
        {
            if (len(ts) != 2L)
            {
                return error.As(EINVAL);
            }
            err = utimensat(_AT_FDCWD, path, new ptr<ref array<Timespec>>(@unsafe.Pointer(ref ts[0L])), 0L);
            if (err != ENOSYS)
            {
                return error.As(err);
            } 
            // If the utimensat syscall isn't available (utimensat was added to Linux
            // in 2.6.22, Released, 8 July 2007) then fall back to utimes
            array<Timeval> tv = new array<Timeval>(2L);
            for (long i = 0L; i < 2L; i++)
            {
                tv[i].Sec = ts[i].Sec;
                tv[i].Usec = ts[i].Nsec / 1000L;
            }

            return error.As(utimes(path, new ptr<ref array<Timeval>>(@unsafe.Pointer(ref tv[0L]))));
        }

        //sys    futimesat(dirfd int, path *byte, times *[2]Timeval) (err error)

        public static error Futimesat(long dirfd, @string path, slice<Timeval> tv)
        {
            if (len(tv) != 2L)
            {
                return error.As(EINVAL);
            }
            var (pathp, err) = BytePtrFromString(path);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(futimesat(dirfd, pathp, new ptr<ref array<Timeval>>(@unsafe.Pointer(ref tv[0L]))));
        }

        public static error Futimes(long fd, slice<Timeval> tv)
        { 
            // Believe it or not, this is the best we can do on Linux
            // (and is what glibc does).
            return error.As(Utimes("/proc/self/fd/" + itoa(fd), tv));
        }

        public static readonly var ImplementsGetwd = true;

        //sys    Getcwd(buf []byte) (n int, err error)



        //sys    Getcwd(buf []byte) (n int, err error)

        public static (@string, error) Getwd()
        {
            array<byte> buf = new array<byte>(PathMax);
            var (n, err) = Getcwd(buf[0L..]);
            if (err != null)
            {
                return ("", err);
            } 
            // Getcwd returns the number of bytes written to buf, including the NUL.
            if (n < 1L || n > len(buf) || buf[n - 1L] != 0L)
            {
                return ("", EINVAL);
            }
            return (string(buf[0L..n - 1L]), null);
        }

        public static (slice<long>, error) Getgroups()
        {
            var (n, err) = getgroups(0L, null);
            if (err != null)
            {
                return (null, err);
            }
            if (n == 0L)
            {
                return (null, null);
            } 

            // Sanity check group count. Max is 1<<16 on Linux.
            if (n < 0L || n > 1L << (int)(20L))
            {
                return (null, EINVAL);
            }
            var a = make_slice<_Gid_t>(n);
            n, err = getgroups(n, ref a[0L]);
            if (err != null)
            {
                return (null, err);
            }
            gids = make_slice<long>(n);
            foreach (var (i, v) in a[0L..n])
            {
                gids[i] = int(v);
            }
            return;
        }

        public static error Setgroups(slice<long> gids)
        {
            if (len(gids) == 0L)
            {
                return error.As(setgroups(0L, null));
            }
            var a = make_slice<_Gid_t>(len(gids));
            foreach (var (i, v) in gids)
            {
                a[i] = _Gid_t(v);
            }
            return error.As(setgroups(len(a), ref a[0L]));
        }

        public partial struct WaitStatus // : uint
        {
        }

        // Wait status is 7 bits at bottom, either 0 (exited),
        // 0x7F (stopped), or a signal number that caused an exit.
        // The 0x80 bit is whether there was a core dump.
        // An extra number (exit code, signal causing a stop)
        // is in the high bits. At least that's the idea.
        // There are various irregularities. For example, the
        // "continued" status is 0xFFFF, distinguishing itself
        // from stopped via the core dump bit.

        private static readonly ulong mask = 0x7FUL;
        private static readonly ulong core = 0x80UL;
        private static readonly ulong exited = 0x00UL;
        private static readonly ulong stopped = 0x7FUL;
        private static readonly long shift = 8L;

        public static bool Exited(this WaitStatus w)
        {
            return w & mask == exited;
        }

        public static bool Signaled(this WaitStatus w)
        {
            return w & mask != stopped && w & mask != exited;
        }

        public static bool Stopped(this WaitStatus w)
        {
            return w & 0xFFUL == stopped;
        }

        public static bool Continued(this WaitStatus w)
        {
            return w == 0xFFFFUL;
        }

        public static bool CoreDump(this WaitStatus w)
        {
            return w.Signaled() && w & core != 0L;
        }

        public static long ExitStatus(this WaitStatus w)
        {
            if (!w.Exited())
            {
                return -1L;
            }
            return int(w >> (int)(shift)) & 0xFFUL;
        }

        public static Signal Signal(this WaitStatus w)
        {
            if (!w.Signaled())
            {
                return -1L;
            }
            return Signal(w & mask);
        }

        public static Signal StopSignal(this WaitStatus w)
        {
            if (!w.Stopped())
            {
                return -1L;
            }
            return Signal(w >> (int)(shift)) & 0xFFUL;
        }

        public static long TrapCause(this WaitStatus w)
        {
            if (w.StopSignal() != SIGTRAP)
            {
                return -1L;
            }
            return int(w >> (int)(shift)) >> (int)(8L);
        }

        //sys    wait4(pid int, wstatus *_C_int, options int, rusage *Rusage) (wpid int, err error)

        public static (long, error) Wait4(long pid, ref WaitStatus wstatus, long options, ref Rusage rusage)
        {
            _C_int status = default;
            wpid, err = wait4(pid, ref status, options, rusage);
            if (wstatus != null)
            {
                wstatus.Value = WaitStatus(status);
            }
            return;
        }

        public static error Mkfifo(@string path, uint mode)
        {
            return error.As(Mknod(path, mode | S_IFIFO, 0L));
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrInet4 sa)
        {
            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Family = AF_INET;
            ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(ref sa.raw), SizeofSockaddrInet4, null);
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrInet6 sa)
        {
            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Family = AF_INET6;
            ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            sa.raw.Scope_id = sa.ZoneId;
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(ref sa.raw), SizeofSockaddrInet6, null);
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrUnix sa)
        {
            var name = sa.Name;
            var n = len(name);
            if (n > len(sa.raw.Path))
            {
                return (null, 0L, EINVAL);
            }
            if (n == len(sa.raw.Path) && name[0L] != '@')
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Family = AF_UNIX;
            for (long i = 0L; i < n; i++)
            {
                sa.raw.Path[i] = int8(name[i]);
            } 
            // length is family (uint16), name, NUL.
 
            // length is family (uint16), name, NUL.
            var sl = _Socklen(2L);
            if (n > 0L)
            {
                sl += _Socklen(n) + 1L;
            }
            if (sa.raw.Path[0L] == '@')
            {
                sa.raw.Path[0L] = 0L; 
                // Don't count trailing NUL for abstract address.
                sl--;
            }
            return (@unsafe.Pointer(ref sa.raw), sl, null);
        }

        public partial struct SockaddrLinklayer
        {
            public ushort Protocol;
            public long Ifindex;
            public ushort Hatype;
            public byte Pkttype;
            public byte Halen;
            public array<byte> Addr;
            public RawSockaddrLinklayer raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrLinklayer sa)
        {
            if (sa.Ifindex < 0L || sa.Ifindex > 0x7fffffffUL)
            {
                return (null, 0L, EINVAL);
            }
            sa.raw.Family = AF_PACKET;
            sa.raw.Protocol = sa.Protocol;
            sa.raw.Ifindex = int32(sa.Ifindex);
            sa.raw.Hatype = sa.Hatype;
            sa.raw.Pkttype = sa.Pkttype;
            sa.raw.Halen = sa.Halen;
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(ref sa.raw), SizeofSockaddrLinklayer, null);
        }

        public partial struct SockaddrNetlink
        {
            public ushort Family;
            public ushort Pad;
            public uint Pid;
            public uint Groups;
            public RawSockaddrNetlink raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ref SockaddrNetlink sa)
        {
            sa.raw.Family = AF_NETLINK;
            sa.raw.Pad = sa.Pad;
            sa.raw.Pid = sa.Pid;
            sa.raw.Groups = sa.Groups;
            return (@unsafe.Pointer(ref sa.raw), SizeofSockaddrNetlink, null);
        }

        private static (Sockaddr, error) anyToSockaddr(ref RawSockaddrAny rsa)
        {

            if (rsa.Addr.Family == AF_NETLINK) 
                var pp = (RawSockaddrNetlink.Value)(@unsafe.Pointer(rsa));
                ptr<SockaddrNetlink> sa = @new<SockaddrNetlink>();
                sa.Family = pp.Family;
                sa.Pad = pp.Pad;
                sa.Pid = pp.Pid;
                sa.Groups = pp.Groups;
                return (sa, null);
            else if (rsa.Addr.Family == AF_PACKET) 
                pp = (RawSockaddrLinklayer.Value)(@unsafe.Pointer(rsa));
                sa = @new<SockaddrLinklayer>();
                sa.Protocol = pp.Protocol;
                sa.Ifindex = int(pp.Ifindex);
                sa.Hatype = pp.Hatype;
                sa.Pkttype = pp.Pkttype;
                sa.Halen = pp.Halen;
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < len(sa.Addr); i++)
                    {
                        sa.Addr[i] = pp.Addr[i];
                    }


                    i = i__prev1;
                }
                return (sa, null);
            else if (rsa.Addr.Family == AF_UNIX) 
                pp = (RawSockaddrUnix.Value)(@unsafe.Pointer(rsa));
                sa = @new<SockaddrUnix>();
                if (pp.Path[0L] == 0L)
                { 
                    // "Abstract" Unix domain socket.
                    // Rewrite leading NUL as @ for textual display.
                    // (This is the standard convention.)
                    // Not friendly to overwrite in place,
                    // but the callers below don't care.
                    pp.Path[0L] = '@';
                } 

                // Assume path ends at NUL.
                // This is not technically the Linux semantics for
                // abstract Unix domain sockets--they are supposed
                // to be uninterpreted fixed-size binary blobs--but
                // everyone uses this convention.
                long n = 0L;
                while (n < len(pp.Path) && pp.Path[n] != 0L)
                {
                    n++;
                }

                ref array<byte> bytes = new ptr<ref array<byte>>(@unsafe.Pointer(ref pp.Path[0L]))[0L..n];
                sa.Name = string(bytes);
                return (sa, null);
            else if (rsa.Addr.Family == AF_INET) 
                pp = (RawSockaddrInet4.Value)(@unsafe.Pointer(rsa));
                sa = @new<SockaddrInet4>();
                ref array<byte> p = new ptr<ref array<byte>>(@unsafe.Pointer(ref pp.Port));
                sa.Port = int(p[0L]) << (int)(8L) + int(p[1L]);
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(sa.Addr); i++)
                    {
                        sa.Addr[i] = pp.Addr[i];
                    }


                    i = i__prev1;
                }
                return (sa, null);
            else if (rsa.Addr.Family == AF_INET6) 
                pp = (RawSockaddrInet6.Value)(@unsafe.Pointer(rsa));
                sa = @new<SockaddrInet6>();
                p = new ptr<ref array<byte>>(@unsafe.Pointer(ref pp.Port));
                sa.Port = int(p[0L]) << (int)(8L) + int(p[1L]);
                sa.ZoneId = pp.Scope_id;
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(sa.Addr); i++)
                    {
                        sa.Addr[i] = pp.Addr[i];
                    }


                    i = i__prev1;
                }
                return (sa, null);
                        return (null, EAFNOSUPPORT);
        }

        public static (long, Sockaddr, error) Accept(long fd)
        {
            RawSockaddrAny rsa = default;
            _Socklen len = SizeofSockaddrAny;
            nfd, err = accept(fd, ref rsa, ref len);
            if (err != null)
            {
                return;
            }
            sa, err = anyToSockaddr(ref rsa);
            if (err != null)
            {
                Close(nfd);
                nfd = 0L;
            }
            return;
        }

        public static (long, Sockaddr, error) Accept4(long fd, long flags) => func((_, panic, __) =>
        {
            RawSockaddrAny rsa = default;
            _Socklen len = SizeofSockaddrAny;
            nfd, err = accept4(fd, ref rsa, ref len, flags);
            if (err != null)
            {
                return;
            }
            if (len > SizeofSockaddrAny)
            {
                panic("RawSockaddrAny too small");
            }
            sa, err = anyToSockaddr(ref rsa);
            if (err != null)
            {
                Close(nfd);
                nfd = 0L;
            }
            return;
        });

        public static (Sockaddr, error) Getsockname(long fd)
        {
            RawSockaddrAny rsa = default;
            _Socklen len = SizeofSockaddrAny;
            err = getsockname(fd, ref rsa, ref len);

            if (err != null)
            {
                return;
            }
            return anyToSockaddr(ref rsa);
        }

        public static (array<byte>, error) GetsockoptInet4Addr(long fd, long level, long opt)
        {
            var vallen = _Socklen(4L);
            err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value[0L]), ref vallen);
            return (value, err);
        }

        public static (ref IPMreq, error) GetsockoptIPMreq(long fd, long level, long opt)
        {
            IPMreq value = default;
            var vallen = _Socklen(SizeofIPMreq);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value), ref vallen);
            return (ref value, err);
        }

        public static (ref IPMreqn, error) GetsockoptIPMreqn(long fd, long level, long opt)
        {
            IPMreqn value = default;
            var vallen = _Socklen(SizeofIPMreqn);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value), ref vallen);
            return (ref value, err);
        }

        public static (ref IPv6Mreq, error) GetsockoptIPv6Mreq(long fd, long level, long opt)
        {
            IPv6Mreq value = default;
            var vallen = _Socklen(SizeofIPv6Mreq);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value), ref vallen);
            return (ref value, err);
        }

        public static (ref IPv6MTUInfo, error) GetsockoptIPv6MTUInfo(long fd, long level, long opt)
        {
            IPv6MTUInfo value = default;
            var vallen = _Socklen(SizeofIPv6MTUInfo);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value), ref vallen);
            return (ref value, err);
        }

        public static (ref ICMPv6Filter, error) GetsockoptICMPv6Filter(long fd, long level, long opt)
        {
            ICMPv6Filter value = default;
            var vallen = _Socklen(SizeofICMPv6Filter);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value), ref vallen);
            return (ref value, err);
        }

        public static (ref Ucred, error) GetsockoptUcred(long fd, long level, long opt)
        {
            Ucred value = default;
            var vallen = _Socklen(SizeofUcred);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(ref value), ref vallen);
            return (ref value, err);
        }

        public static error SetsockoptIPMreqn(long fd, long level, long opt, ref IPMreqn mreq)
        {
            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), @unsafe.Sizeof(mreq.Value)));
        }

        public static (long, long, long, Sockaddr, error) Recvmsg(long fd, slice<byte> p, slice<byte> oob, long flags)
        {
            Msghdr msg = default;
            RawSockaddrAny rsa = default;
            msg.Name = (byte.Value)(@unsafe.Pointer(ref rsa));
            msg.Namelen = uint32(SizeofSockaddrAny);
            Iovec iov = default;
            if (len(p) > 0L)
            {
                iov.Base = ref p[0L];
                iov.SetLen(len(p));
            }
            byte dummy = default;
            if (len(oob) > 0L)
            {
                long sockType = default;
                sockType, err = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);
                if (err != null)
                {
                    return;
                } 
                // receive at least one normal byte
                if (sockType != SOCK_DGRAM && len(p) == 0L)
                {
                    iov.Base = ref dummy;
                    iov.SetLen(1L);
                }
                msg.Control = ref oob[0L];
                msg.SetControllen(len(oob));
            }
            msg.Iov = ref iov;
            msg.Iovlen = 1L;
            n, err = recvmsg(fd, ref msg, flags);

            if (err != null)
            {
                return;
            }
            oobn = int(msg.Controllen);
            recvflags = int(msg.Flags); 
            // source address is only specified if the socket is unconnected
            if (rsa.Addr.Family != AF_UNSPEC)
            {
                from, err = anyToSockaddr(ref rsa);
            }
            return;
        }

        public static error Sendmsg(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            _, err = SendmsgN(fd, p, oob, to, flags);
            return;
        }

        public static (long, error) SendmsgN(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            unsafe.Pointer ptr = default;
            _Socklen salen = default;
            if (to != null)
            {
                error err = default;
                ptr, salen, err = to.sockaddr();
                if (err != null)
                {
                    return (0L, err);
                }
            }
            Msghdr msg = default;
            msg.Name = (byte.Value)(ptr);
            msg.Namelen = uint32(salen);
            Iovec iov = default;
            if (len(p) > 0L)
            {
                iov.Base = ref p[0L];
                iov.SetLen(len(p));
            }
            byte dummy = default;
            if (len(oob) > 0L)
            {
                long sockType = default;
                sockType, err = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);
                if (err != null)
                {
                    return (0L, err);
                } 
                // send at least one normal byte
                if (sockType != SOCK_DGRAM && len(p) == 0L)
                {
                    iov.Base = ref dummy;
                    iov.SetLen(1L);
                }
                msg.Control = ref oob[0L];
                msg.SetControllen(len(oob));
            }
            msg.Iov = ref iov;
            msg.Iovlen = 1L;
            n, err = sendmsg(fd, ref msg, flags);

            if (err != null)
            {
                return (0L, err);
            }
            if (len(oob) > 0L && len(p) == 0L)
            {
                n = 0L;
            }
            return (n, null);
        }

        // BindToDevice binds the socket associated with fd to device.
        public static error BindToDevice(long fd, @string device)
        {
            return error.As(SetsockoptString(fd, SOL_SOCKET, SO_BINDTODEVICE, device));
        }

        //sys    ptrace(request int, pid int, addr uintptr, data uintptr) (err error)

        private static (long, error) ptracePeek(long req, long pid, System.UIntPtr addr, slice<byte> @out)
        { 
            // The peek requests are machine-size oriented, so we wrap it
            // to retrieve arbitrary-length data.

            // The ptrace syscall differs from glibc's ptrace.
            // Peeks returns the word in *data, not as the return value.

            array<byte> buf = new array<byte>(sizeofPtr); 

            // Leading edge. PEEKTEXT/PEEKDATA don't require aligned
            // access (PEEKUSER warns that it might), but if we don't
            // align our reads, we might straddle an unmapped page
            // boundary and not get the bytes leading up to the page
            // boundary.
            long n = 0L;
            if (addr % sizeofPtr != 0L)
            {
                err = ptrace(req, pid, addr - addr % sizeofPtr, uintptr(@unsafe.Pointer(ref buf[0L])));
                if (err != null)
                {
                    return (0L, err);
                }
                n += copy(out, buf[addr % sizeofPtr..]);
                out = out[n..];
            } 

            // Remainder.
            while (len(out) > 0L)
            { 
                // We use an internal buffer to guarantee alignment.
                // It's not documented if this is necessary, but we're paranoid.
                err = ptrace(req, pid, addr + uintptr(n), uintptr(@unsafe.Pointer(ref buf[0L])));
                if (err != null)
                {
                    return (n, err);
                }
                var copied = copy(out, buf[0L..]);
                n += copied;
                out = out[copied..];
            }


            return (n, null);
        }

        public static (long, error) PtracePeekText(long pid, System.UIntPtr addr, slice<byte> @out)
        {
            return ptracePeek(PTRACE_PEEKTEXT, pid, addr, out);
        }

        public static (long, error) PtracePeekData(long pid, System.UIntPtr addr, slice<byte> @out)
        {
            return ptracePeek(PTRACE_PEEKDATA, pid, addr, out);
        }

        private static (long, error) ptracePoke(long pokeReq, long peekReq, long pid, System.UIntPtr addr, slice<byte> data)
        { 
            // As for ptracePeek, we need to align our accesses to deal
            // with the possibility of straddling an invalid page.

            // Leading edge.
            long n = 0L;
            if (addr % sizeofPtr != 0L)
            {
                array<byte> buf = new array<byte>(sizeofPtr);
                err = ptrace(peekReq, pid, addr - addr % sizeofPtr, uintptr(@unsafe.Pointer(ref buf[0L])));
                if (err != null)
                {
                    return (0L, err);
                }
                n += copy(buf[addr % sizeofPtr..], data);
                var word = ((uintptr.Value)(@unsafe.Pointer(ref buf[0L]))).Value;
                err = ptrace(pokeReq, pid, addr - addr % sizeofPtr, word);
                if (err != null)
                {
                    return (0L, err);
                }
                data = data[n..];
            } 

            // Interior.
            while (len(data) > sizeofPtr)
            {
                word = ((uintptr.Value)(@unsafe.Pointer(ref data[0L]))).Value;
                err = ptrace(pokeReq, pid, addr + uintptr(n), word);
                if (err != null)
                {
                    return (n, err);
                }
                n += sizeofPtr;
                data = data[sizeofPtr..];
            } 

            // Trailing edge.
 

            // Trailing edge.
            if (len(data) > 0L)
            {
                buf = new array<byte>(sizeofPtr);
                err = ptrace(peekReq, pid, addr + uintptr(n), uintptr(@unsafe.Pointer(ref buf[0L])));
                if (err != null)
                {
                    return (n, err);
                }
                copy(buf[0L..], data);
                word = ((uintptr.Value)(@unsafe.Pointer(ref buf[0L]))).Value;
                err = ptrace(pokeReq, pid, addr + uintptr(n), word);
                if (err != null)
                {
                    return (n, err);
                }
                n += len(data);
            }
            return (n, null);
        }

        public static (long, error) PtracePokeText(long pid, System.UIntPtr addr, slice<byte> data)
        {
            return ptracePoke(PTRACE_POKETEXT, PTRACE_PEEKTEXT, pid, addr, data);
        }

        public static (long, error) PtracePokeData(long pid, System.UIntPtr addr, slice<byte> data)
        {
            return ptracePoke(PTRACE_POKEDATA, PTRACE_PEEKDATA, pid, addr, data);
        }

        public static error PtraceGetRegs(long pid, ref PtraceRegs regsout)
        {
            return error.As(ptrace(PTRACE_GETREGS, pid, 0L, uintptr(@unsafe.Pointer(regsout))));
        }

        public static error PtraceSetRegs(long pid, ref PtraceRegs regs)
        {
            return error.As(ptrace(PTRACE_SETREGS, pid, 0L, uintptr(@unsafe.Pointer(regs))));
        }

        public static error PtraceSetOptions(long pid, long options)
        {
            return error.As(ptrace(PTRACE_SETOPTIONS, pid, 0L, uintptr(options)));
        }

        public static (ulong, error) PtraceGetEventMsg(long pid)
        {
            _C_long data = default;
            err = ptrace(PTRACE_GETEVENTMSG, pid, 0L, uintptr(@unsafe.Pointer(ref data)));
            msg = uint(data);
            return;
        }

        public static error PtraceCont(long pid, long signal)
        {
            return error.As(ptrace(PTRACE_CONT, pid, 0L, uintptr(signal)));
        }

        public static error PtraceSyscall(long pid, long signal)
        {
            return error.As(ptrace(PTRACE_SYSCALL, pid, 0L, uintptr(signal)));
        }

        public static error PtraceSingleStep(long pid)
        {
            return error.As(ptrace(PTRACE_SINGLESTEP, pid, 0L, 0L));
        }

        public static error PtraceAttach(long pid)
        {
            return error.As(ptrace(PTRACE_ATTACH, pid, 0L, 0L));
        }

        public static error PtraceDetach(long pid)
        {
            return error.As(ptrace(PTRACE_DETACH, pid, 0L, 0L));
        }

        //sys    reboot(magic1 uint, magic2 uint, cmd int, arg string) (err error)

        public static error Reboot(long cmd)
        {
            return error.As(reboot(LINUX_REBOOT_MAGIC1, LINUX_REBOOT_MAGIC2, cmd, ""));
        }

        public static (long, error) ReadDirent(long fd, slice<byte> buf)
        {
            return Getdents(fd, buf);
        }

        private static (ulong, bool) direntIno(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Ino), @unsafe.Sizeof(new Dirent().Ino));
        }

        private static (ulong, bool) direntReclen(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Reclen), @unsafe.Sizeof(new Dirent().Reclen));
        }

        private static (ulong, bool) direntNamlen(slice<byte> buf)
        {
            var (reclen, ok) = direntReclen(buf);
            if (!ok)
            {
                return (0L, false);
            }
            return (reclen - uint64(@unsafe.Offsetof(new Dirent().Name)), true);
        }

        //sys    mount(source string, target string, fstype string, flags uintptr, data *byte) (err error)

        public static error Mount(@string source, @string target, @string fstype, System.UIntPtr flags, @string data)
        { 
            // Certain file systems get rather angry and EINVAL if you give
            // them an empty string of data, rather than NULL.
            if (data == "")
            {
                return error.As(mount(source, target, fstype, flags, null));
            }
            var (datap, err) = BytePtrFromString(data);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(mount(source, target, fstype, flags, datap));
        }

        // Sendto
        // Recvfrom
        // Socketpair

        /*
         * Direct access
         */
        //sys    Acct(path string) (err error)
        //sys    Adjtimex(buf *Timex) (state int, err error)
        //sys    Chdir(path string) (err error)
        //sys    Chroot(path string) (err error)
        //sys    Close(fd int) (err error)
        //sys    Dup(oldfd int) (fd int, err error)
        //sys    Dup3(oldfd int, newfd int, flags int) (err error)
        //sysnb    EpollCreate(size int) (fd int, err error)
        //sysnb    EpollCreate1(flag int) (fd int, err error)
        //sysnb    EpollCtl(epfd int, op int, fd int, event *EpollEvent) (err error)
        //sys    EpollWait(epfd int, events []EpollEvent, msec int) (n int, err error)
        //sys    Faccessat(dirfd int, path string, mode uint32, flags int) (err error)
        //sys    Fallocate(fd int, mode uint32, off int64, len int64) (err error)
        //sys    Fchdir(fd int) (err error)
        //sys    Fchmod(fd int, mode uint32) (err error)
        //sys    Fchmodat(dirfd int, path string, mode uint32, flags int) (err error)
        //sys    Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
        //sys    fcntl(fd int, cmd int, arg int) (val int, err error)
        //sys    Fdatasync(fd int) (err error)
        //sys    Flock(fd int, how int) (err error)
        //sys    Fsync(fd int) (err error)
        //sys    Getdents(fd int, buf []byte) (n int, err error) = SYS_GETDENTS64
        //sysnb    Getpgid(pid int) (pgid int, err error)

        public static long Getpgrp()
        {
            pid, _ = Getpgid(0L);
            return;
        }

        //sysnb    Getpid() (pid int)
        //sysnb    Getppid() (ppid int)
        //sys    Getpriority(which int, who int) (prio int, err error)
        //sysnb    Getrusage(who int, rusage *Rusage) (err error)
        //sysnb    Gettid() (tid int)
        //sys    Getxattr(path string, attr string, dest []byte) (sz int, err error)
        //sys    InotifyAddWatch(fd int, pathname string, mask uint32) (watchdesc int, err error)
        //sysnb    InotifyInit1(flags int) (fd int, err error)
        //sysnb    InotifyRmWatch(fd int, watchdesc uint32) (success int, err error)
        //sysnb    Kill(pid int, sig Signal) (err error)
        //sys    Klogctl(typ int, buf []byte) (n int, err error) = SYS_SYSLOG
        //sys    Listxattr(path string, dest []byte) (sz int, err error)
        //sys    Mkdirat(dirfd int, path string, mode uint32) (err error)
        //sys    Mknodat(dirfd int, path string, mode uint32, dev int) (err error)
        //sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
        //sys    Pause() (err error)
        //sys    PivotRoot(newroot string, putold string) (err error) = SYS_PIVOT_ROOT
        //sysnb prlimit(pid int, resource int, newlimit *Rlimit, old *Rlimit) (err error) = SYS_PRLIMIT64
        //sys    read(fd int, p []byte) (n int, err error)
        //sys    Removexattr(path string, attr string) (err error)
        //sys    Renameat(olddirfd int, oldpath string, newdirfd int, newpath string) (err error)
        //sys    Setdomainname(p []byte) (err error)
        //sys    Sethostname(p []byte) (err error)
        //sysnb    Setpgid(pid int, pgid int) (err error)
        //sysnb    Setsid() (pid int, err error)
        //sysnb    Settimeofday(tv *Timeval) (err error)

        // issue 1435.
        // On linux Setuid and Setgid only affects the current thread, not the process.
        // This does not match what most callers expect so we must return an error
        // here rather than letting the caller think that the call succeeded.

        public static error Setuid(long uid)
        {
            return error.As(EOPNOTSUPP);
        }

        public static error Setgid(long gid)
        {
            return error.As(EOPNOTSUPP);
        }

        //sys    Setpriority(which int, who int, prio int) (err error)
        //sys    Setxattr(path string, attr string, data []byte, flags int) (err error)
        //sys    Sync()
        //sysnb    Sysinfo(info *Sysinfo_t) (err error)
        //sys    Tee(rfd int, wfd int, len int, flags int) (n int64, err error)
        //sysnb    Tgkill(tgid int, tid int, sig Signal) (err error)
        //sysnb    Times(tms *Tms) (ticks uintptr, err error)
        //sysnb    Umask(mask int) (oldmask int)
        //sysnb    Uname(buf *Utsname) (err error)
        //sys    Unmount(target string, flags int) (err error) = SYS_UMOUNT2
        //sys    Unshare(flags int) (err error)
        //sys    Ustat(dev int, ubuf *Ustat_t) (err error)
        //sys    Utime(path string, buf *Utimbuf) (err error)
        //sys    write(fd int, p []byte) (n int, err error)
        //sys    exitThread(code int) (err error) = SYS_EXIT
        //sys    readlen(fd int, p *byte, np int) (n int, err error) = SYS_READ
        //sys    writelen(fd int, p *byte, np int) (n int, err error) = SYS_WRITE

        // mmap varies by architecture; see syscall_linux_*.go.
        //sys    munmap(addr uintptr, length uintptr) (err error)

        private static mmapper mapper = ref new mmapper(active:make(map[*byte][]byte),mmap:mmap,munmap:munmap,);

        public static (slice<byte>, error) Mmap(long fd, long offset, long length, long prot, long flags)
        {
            return mapper.Mmap(fd, offset, length, prot, flags);
        }

        public static error Munmap(slice<byte> b)
        {
            return error.As(mapper.Munmap(b));
        }

        //sys    Madvise(b []byte, advice int) (err error)
        //sys    Mprotect(b []byte, prot int) (err error)
        //sys    Mlock(b []byte) (err error)
        //sys    Munlock(b []byte) (err error)
        //sys    Mlockall(flags int) (err error)
        //sys    Munlockall() (err error)

        /*
         * Unimplemented
         */
        // AddKey
        // AfsSyscall
        // Alarm
        // ArchPrctl
        // Brk
        // Capget
        // Capset
        // ClockGetres
        // ClockGettime
        // ClockNanosleep
        // ClockSettime
        // Clone
        // CreateModule
        // DeleteModule
        // EpollCtlOld
        // EpollPwait
        // EpollWaitOld
        // Eventfd
        // Execve
        // Fadvise64
        // Fgetxattr
        // Flistxattr
        // Fork
        // Fremovexattr
        // Fsetxattr
        // Futex
        // GetKernelSyms
        // GetMempolicy
        // GetRobustList
        // GetThreadArea
        // Getitimer
        // Getpmsg
        // IoCancel
        // IoDestroy
        // IoGetevents
        // IoSetup
        // IoSubmit
        // Ioctl
        // IoprioGet
        // IoprioSet
        // KexecLoad
        // Keyctl
        // Lgetxattr
        // Llistxattr
        // LookupDcookie
        // Lremovexattr
        // Lsetxattr
        // Mbind
        // MigratePages
        // Mincore
        // ModifyLdt
        // Mount
        // MovePages
        // Mprotect
        // MqGetsetattr
        // MqNotify
        // MqOpen
        // MqTimedreceive
        // MqTimedsend
        // MqUnlink
        // Mremap
        // Msgctl
        // Msgget
        // Msgrcv
        // Msgsnd
        // Msync
        // Newfstatat
        // Nfsservctl
        // Personality
        // Poll
        // Ppoll
        // Prctl
        // Pselect6
        // Ptrace
        // Putpmsg
        // QueryModule
        // Quotactl
        // Readahead
        // Readv
        // RemapFilePages
        // RequestKey
        // RestartSyscall
        // RtSigaction
        // RtSigpending
        // RtSigprocmask
        // RtSigqueueinfo
        // RtSigreturn
        // RtSigsuspend
        // RtSigtimedwait
        // SchedGetPriorityMax
        // SchedGetPriorityMin
        // SchedGetaffinity
        // SchedGetparam
        // SchedGetscheduler
        // SchedRrGetInterval
        // SchedSetaffinity
        // SchedSetparam
        // SchedYield
        // Security
        // Semctl
        // Semget
        // Semop
        // Semtimedop
        // SetMempolicy
        // SetRobustList
        // SetThreadArea
        // SetTidAddress
        // Shmat
        // Shmctl
        // Shmdt
        // Shmget
        // Sigaltstack
        // Signalfd
        // Swapoff
        // Swapon
        // Sysfs
        // TimerCreate
        // TimerDelete
        // TimerGetoverrun
        // TimerGettime
        // TimerSettime
        // Timerfd
        // Tkill (obsolete)
        // Tuxcall
        // Umount2
        // Uselib
        // Utimensat
        // Vfork
        // Vhangup
        // Vmsplice
        // Vserver
        // Waitid
        // _Sysctl
    }
}
