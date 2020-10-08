// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Linux system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and
// wrap it in our own nicer implementation.

// package syscall -- go2cs converted at 2020 October 08 03:27:27 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_linux.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static (System.UIntPtr, System.UIntPtr) rawSyscallNoError(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3)
;

        /*
         * Wrapped
         */

        public static error Access(@string path, uint mode)
        {
            error err = default!;

            return error.As(Faccessat(_AT_FDCWD, path, mode, 0L))!;
        }

        public static error Chmod(@string path, uint mode)
        {
            error err = default!;

            return error.As(Fchmodat(_AT_FDCWD, path, mode, 0L))!;
        }

        public static error Chown(@string path, long uid, long gid)
        {
            error err = default!;

            return error.As(Fchownat(_AT_FDCWD, path, uid, gid, 0L))!;
        }

        public static (long, error) Creat(@string path, uint mode)
        {
            long fd = default;
            error err = default!;

            return Open(path, O_CREAT | O_WRONLY | O_TRUNC, mode);
        }

        private static bool isGroupMember(long gid)
        {
            var (groups, err) = Getgroups();
            if (err != null)
            {>>MARKER:FUNCTION_rawSyscallNoError_BLOCK_PREFIX<<
                return false;
            }

            foreach (var (_, g) in groups)
            {
                if (g == gid)
                {
                    return true;
                }

            }
            return false;

        }

        //sys    faccessat(dirfd int, path string, mode uint32) (err error)

        public static error Faccessat(long dirfd, @string path, uint mode, long flags)
        {
            error err = default!;

            if (flags & ~(_AT_SYMLINK_NOFOLLOW | _AT_EACCESS) != 0L)
            {
                return error.As(EINVAL)!;
            } 

            // The Linux kernel faccessat system call does not take any flags.
            // The glibc faccessat implements the flags itself; see
            // https://sourceware.org/git/?p=glibc.git;a=blob;f=sysdeps/unix/sysv/linux/faccessat.c;hb=HEAD
            // Because people naturally expect syscall.Faccessat to act
            // like C faccessat, we do the same.
            if (flags == 0L)
            {
                return error.As(faccessat(dirfd, path, mode))!;
            }

            ref Stat_t st = ref heap(out ptr<Stat_t> _addr_st);
            {
                var err = fstatat(dirfd, path, _addr_st, flags & _AT_SYMLINK_NOFOLLOW);

                if (err != null)
                {
                    return error.As(err)!;
                }

            }


            mode &= 7L;
            if (mode == 0L)
            {
                return error.As(null!)!;
            }

            long uid = default;
            if (flags & _AT_EACCESS != 0L)
            {
                uid = Geteuid();
            }
            else
            {
                uid = Getuid();
            }

            if (uid == 0L)
            {
                if (mode & 1L == 0L)
                { 
                    // Root can read and write any file.
                    return error.As(null!)!;

                }

                if (st.Mode & 0111L != 0L)
                { 
                    // Root can execute any file that anybody can execute.
                    return error.As(null!)!;

                }

                return error.As(EACCES)!;

            }

            uint fmode = default;
            if (uint32(uid) == st.Uid)
            {
                fmode = (st.Mode >> (int)(6L)) & 7L;
            }
            else
            {
                long gid = default;
                if (flags & _AT_EACCESS != 0L)
                {
                    gid = Getegid();
                }
                else
                {
                    gid = Getgid();
                }

                if (uint32(gid) == st.Gid || isGroupMember(gid))
                {
                    fmode = (st.Mode >> (int)(3L)) & 7L;
                }
                else
                {
                    fmode = st.Mode & 7L;
                }

            }

            if (fmode & mode == mode)
            {
                return error.As(null!)!;
            }

            return error.As(EACCES)!;

        }

        //sys    fchmodat(dirfd int, path string, mode uint32) (err error)

        public static error Fchmodat(long dirfd, @string path, uint mode, long flags)
        {
            error err = default!;
 
            // Linux fchmodat doesn't support the flags parameter. Mimick glibc's behavior
            // and check the flags. Otherwise the mode would be applied to the symlink
            // destination which is not what the user expects.
            if (flags & ~_AT_SYMLINK_NOFOLLOW != 0L)
            {
                return error.As(EINVAL)!;
            }
            else if (flags & _AT_SYMLINK_NOFOLLOW != 0L)
            {
                return error.As(EOPNOTSUPP)!;
            }

            return error.As(fchmodat(dirfd, path, mode))!;

        }

        //sys    linkat(olddirfd int, oldpath string, newdirfd int, newpath string, flags int) (err error)

        public static error Link(@string oldpath, @string newpath)
        {
            error err = default!;

            return error.As(linkat(_AT_FDCWD, oldpath, _AT_FDCWD, newpath, 0L))!;
        }

        public static error Mkdir(@string path, uint mode)
        {
            error err = default!;

            return error.As(Mkdirat(_AT_FDCWD, path, mode))!;
        }

        public static error Mknod(@string path, uint mode, long dev)
        {
            error err = default!;

            return error.As(Mknodat(_AT_FDCWD, path, mode, dev))!;
        }

        public static (long, error) Open(@string path, long mode, uint perm)
        {
            long fd = default;
            error err = default!;

            return openat(_AT_FDCWD, path, mode | O_LARGEFILE, perm);
        }

        //sys    openat(dirfd int, path string, flags int, mode uint32) (fd int, err error)

        public static (long, error) Openat(long dirfd, @string path, long flags, uint mode)
        {
            long fd = default;
            error err = default!;

            return openat(dirfd, path, flags | O_LARGEFILE, mode);
        }

        //sys    readlinkat(dirfd int, path string, buf []byte) (n int, err error)

        public static (long, error) Readlink(@string path, slice<byte> buf)
        {
            long n = default;
            error err = default!;

            return readlinkat(_AT_FDCWD, path, buf);
        }

        public static error Rename(@string oldpath, @string newpath)
        {
            error err = default!;

            return error.As(Renameat(_AT_FDCWD, oldpath, _AT_FDCWD, newpath))!;
        }

        public static error Rmdir(@string path)
        {
            return error.As(unlinkat(_AT_FDCWD, path, _AT_REMOVEDIR))!;
        }

        //sys    symlinkat(oldpath string, newdirfd int, newpath string) (err error)

        public static error Symlink(@string oldpath, @string newpath)
        {
            error err = default!;

            return error.As(symlinkat(oldpath, _AT_FDCWD, newpath))!;
        }

        public static error Unlink(@string path)
        {
            return error.As(unlinkat(_AT_FDCWD, path, 0L))!;
        }

        //sys    unlinkat(dirfd int, path string, flags int) (err error)

        public static error Unlinkat(long dirfd, @string path)
        {
            return error.As(unlinkat(dirfd, path, 0L))!;
        }

        public static error Utimes(@string path, slice<Timeval> tv)
        {
            error err = default!;

            if (len(tv) != 2L)
            {
                return error.As(EINVAL)!;
            }

            return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0L]))))!;

        }

        //sys    utimensat(dirfd int, path string, times *[2]Timespec, flag int) (err error)

        public static error UtimesNano(@string path, slice<Timespec> ts)
        {
            error err = default!;

            if (len(ts) != 2L)
            {
                return error.As(EINVAL)!;
            }

            err = utimensat(_AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), 0L);
            if (err != ENOSYS)
            {
                return error.As(err)!;
            } 
            // If the utimensat syscall isn't available (utimensat was added to Linux
            // in 2.6.22, Released, 8 July 2007) then fall back to utimes
            array<Timeval> tv = new array<Timeval>(2L);
            for (long i = 0L; i < 2L; i++)
            {
                tv[i].Sec = ts[i].Sec;
                tv[i].Usec = ts[i].Nsec / 1000L;
            }

            return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0L]))))!;

        }

        public static error Futimesat(long dirfd, @string path, slice<Timeval> tv)
        {
            error err = default!;

            if (len(tv) != 2L)
            {
                return error.As(EINVAL)!;
            }

            return error.As(futimesat(dirfd, path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0L]))))!;

        }

        public static error Futimes(long fd, slice<Timeval> tv)
        {
            error err = default!;
 
            // Believe it or not, this is the best we can do on Linux
            // (and is what glibc does).
            return error.As(Utimes("/proc/self/fd/" + itoa(fd), tv))!;

        }

        public static readonly var ImplementsGetwd = (var)true;

        //sys    Getcwd(buf []byte) (n int, err error)



        //sys    Getcwd(buf []byte) (n int, err error)

        public static (@string, error) Getwd()
        {
            @string wd = default;
            error err = default!;

            array<byte> buf = new array<byte>(PathMax);
            var (n, err) = Getcwd(buf[0L..]);
            if (err != null)
            {
                return ("", error.As(err)!);
            } 
            // Getcwd returns the number of bytes written to buf, including the NUL.
            if (n < 1L || n > len(buf) || buf[n - 1L] != 0L)
            {
                return ("", error.As(EINVAL)!);
            }

            return (string(buf[0L..n - 1L]), error.As(null!)!);

        }

        public static (slice<long>, error) Getgroups()
        {
            slice<long> gids = default;
            error err = default!;

            var (n, err) = getgroups(0L, null);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            if (n == 0L)
            {
                return (null, error.As(null!)!);
            } 

            // Sanity check group count. Max is 1<<16 on Linux.
            if (n < 0L || n > 1L << (int)(20L))
            {
                return (null, error.As(EINVAL)!);
            }

            var a = make_slice<_Gid_t>(n);
            n, err = getgroups(n, _addr_a[0L]);
            if (err != null)
            {
                return (null, error.As(err)!);
            }

            gids = make_slice<long>(n);
            foreach (var (i, v) in a[0L..n])
            {
                gids[i] = int(v);
            }
            return ;

        }

        public static error Setgroups(slice<long> gids)
        {
            error err = default!;

            if (len(gids) == 0L)
            {
                return error.As(setgroups(0L, null))!;
            }

            var a = make_slice<_Gid_t>(len(gids));
            foreach (var (i, v) in gids)
            {
                a[i] = _Gid_t(v);
            }
            return error.As(setgroups(len(a), _addr_a[0L]))!;

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

        private static readonly ulong mask = (ulong)0x7FUL;
        private static readonly ulong core = (ulong)0x80UL;
        private static readonly ulong exited = (ulong)0x00UL;
        private static readonly ulong stopped = (ulong)0x7FUL;
        private static readonly long shift = (long)8L;


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

        public static (long, error) Wait4(long pid, ptr<WaitStatus> _addr_wstatus, long options, ptr<Rusage> _addr_rusage)
        {
            long wpid = default;
            error err = default!;
            ref WaitStatus wstatus = ref _addr_wstatus.val;
            ref Rusage rusage = ref _addr_rusage.val;

            ref _C_int status = ref heap(out ptr<_C_int> _addr_status);
            wpid, err = wait4(pid, _addr_status, options, rusage);
            if (wstatus != null)
            {
                wstatus = WaitStatus(status);
            }

            return ;

        }

        public static error Mkfifo(@string path, uint mode)
        {
            error err = default!;

            return error.As(Mknod(path, mode | S_IFIFO, 0L))!;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrInet4> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrInet4 sa = ref _addr_sa.val;

            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            sa.raw.Family = AF_INET;
            ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrInet4, error.As(null!)!);

        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrInet6> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrInet6 sa = ref _addr_sa.val;

            if (sa.Port < 0L || sa.Port > 0xFFFFUL)
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            sa.raw.Family = AF_INET6;
            ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Port));
            p[0L] = byte(sa.Port >> (int)(8L));
            p[1L] = byte(sa.Port);
            sa.raw.Scope_id = sa.ZoneId;
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrInet6, error.As(null!)!);

        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrUnix> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrUnix sa = ref _addr_sa.val;

            var name = sa.Name;
            var n = len(name);
            if (n > len(sa.raw.Path))
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            if (n == len(sa.raw.Path) && name[0L] != '@')
            {
                return (null, 0L, error.As(EINVAL)!);
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

            return (@unsafe.Pointer(_addr_sa.raw), sl, error.As(null!)!);

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

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrLinklayer> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrLinklayer sa = ref _addr_sa.val;

            if (sa.Ifindex < 0L || sa.Ifindex > 0x7fffffffUL)
            {
                return (null, 0L, error.As(EINVAL)!);
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

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrLinklayer, error.As(null!)!);

        }

        public partial struct SockaddrNetlink
        {
            public ushort Family;
            public ushort Pad;
            public uint Pid;
            public uint Groups;
            public RawSockaddrNetlink raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrNetlink> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrNetlink sa = ref _addr_sa.val;

            sa.raw.Family = AF_NETLINK;
            sa.raw.Pad = sa.Pad;
            sa.raw.Pid = sa.Pid;
            sa.raw.Groups = sa.Groups;
            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrNetlink, error.As(null!)!);
        }

        private static (Sockaddr, error) anyToSockaddr(ptr<RawSockaddrAny> _addr_rsa)
        {
            Sockaddr _p0 = default;
            error _p0 = default!;
            ref RawSockaddrAny rsa = ref _addr_rsa.val;


            if (rsa.Addr.Family == AF_NETLINK) 
                var pp = (RawSockaddrNetlink.val)(@unsafe.Pointer(rsa));
                ptr<SockaddrNetlink> sa = @new<SockaddrNetlink>();
                sa.Family = pp.Family;
                sa.Pad = pp.Pad;
                sa.Pid = pp.Pid;
                sa.Groups = pp.Groups;
                return (sa, error.As(null!)!);
            else if (rsa.Addr.Family == AF_PACKET) 
                pp = (RawSockaddrLinklayer.val)(@unsafe.Pointer(rsa));
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
                return (sa, error.As(null!)!);
            else if (rsa.Addr.Family == AF_UNIX) 
                pp = (RawSockaddrUnix.val)(@unsafe.Pointer(rsa));
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

                ptr<array<byte>> bytes = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Path[0L]))[0L..n];
                sa.Name = string(bytes);
                return (sa, error.As(null!)!);
            else if (rsa.Addr.Family == AF_INET) 
                pp = (RawSockaddrInet4.val)(@unsafe.Pointer(rsa));
                sa = @new<SockaddrInet4>();
                ptr<array<byte>> p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Port));
                sa.Port = int(p[0L]) << (int)(8L) + int(p[1L]);
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(sa.Addr); i++)
                    {
                        sa.Addr[i] = pp.Addr[i];
                    }


                    i = i__prev1;
                }
                return (sa, error.As(null!)!);
            else if (rsa.Addr.Family == AF_INET6) 
                pp = (RawSockaddrInet6.val)(@unsafe.Pointer(rsa));
                sa = @new<SockaddrInet6>();
                p = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_pp.Port));
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
                return (sa, error.As(null!)!);
                        return (null, error.As(EAFNOSUPPORT)!);

        }

        public static (long, Sockaddr, error) Accept(long fd)
        {
            long nfd = default;
            Sockaddr sa = default;
            error err = default!;

            ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
            ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
            nfd, err = accept(fd, _addr_rsa, _addr_len);
            if (err != null)
            {
                return ;
            }

            sa, err = anyToSockaddr(_addr_rsa);
            if (err != null)
            {
                Close(nfd);
                nfd = 0L;
            }

            return ;

        }

        public static (long, Sockaddr, error) Accept4(long fd, long flags) => func((_, panic, __) =>
        {
            long nfd = default;
            Sockaddr sa = default;
            error err = default!;

            ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
            ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
            nfd, err = accept4(fd, _addr_rsa, _addr_len, flags);
            if (err != null)
            {
                return ;
            }

            if (len > SizeofSockaddrAny)
            {
                panic("RawSockaddrAny too small");
            }

            sa, err = anyToSockaddr(_addr_rsa);
            if (err != null)
            {
                Close(nfd);
                nfd = 0L;
            }

            return ;

        });

        public static (Sockaddr, error) Getsockname(long fd)
        {
            Sockaddr sa = default;
            error err = default!;

            ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
            ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
            err = getsockname(fd, _addr_rsa, _addr_len);

            if (err != null)
            {
                return ;
            }

            return anyToSockaddr(_addr_rsa);

        }

        public static (array<byte>, error) GetsockoptInet4Addr(long fd, long level, long opt)
        {
            array<byte> value = default;
            error err = default!;

            ref var vallen = ref heap(_Socklen(4L), out ptr<var> _addr_vallen);
            err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value[0L]), _addr_vallen);
            return (value, error.As(err)!);
        }

        public static (ptr<IPMreq>, error) GetsockoptIPMreq(long fd, long level, long opt)
        {
            ptr<IPMreq> _p0 = default!;
            error _p0 = default!;

            ref IPMreq value = ref heap(out ptr<IPMreq> _addr_value);
            ref var vallen = ref heap(_Socklen(SizeofIPMreq), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
            return (_addr__addr_value!, error.As(err)!);
        }

        public static (ptr<IPMreqn>, error) GetsockoptIPMreqn(long fd, long level, long opt)
        {
            ptr<IPMreqn> _p0 = default!;
            error _p0 = default!;

            ref IPMreqn value = ref heap(out ptr<IPMreqn> _addr_value);
            ref var vallen = ref heap(_Socklen(SizeofIPMreqn), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
            return (_addr__addr_value!, error.As(err)!);
        }

        public static (ptr<IPv6Mreq>, error) GetsockoptIPv6Mreq(long fd, long level, long opt)
        {
            ptr<IPv6Mreq> _p0 = default!;
            error _p0 = default!;

            ref IPv6Mreq value = ref heap(out ptr<IPv6Mreq> _addr_value);
            ref var vallen = ref heap(_Socklen(SizeofIPv6Mreq), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
            return (_addr__addr_value!, error.As(err)!);
        }

        public static (ptr<IPv6MTUInfo>, error) GetsockoptIPv6MTUInfo(long fd, long level, long opt)
        {
            ptr<IPv6MTUInfo> _p0 = default!;
            error _p0 = default!;

            ref IPv6MTUInfo value = ref heap(out ptr<IPv6MTUInfo> _addr_value);
            ref var vallen = ref heap(_Socklen(SizeofIPv6MTUInfo), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
            return (_addr__addr_value!, error.As(err)!);
        }

        public static (ptr<ICMPv6Filter>, error) GetsockoptICMPv6Filter(long fd, long level, long opt)
        {
            ptr<ICMPv6Filter> _p0 = default!;
            error _p0 = default!;

            ref ICMPv6Filter value = ref heap(out ptr<ICMPv6Filter> _addr_value);
            ref var vallen = ref heap(_Socklen(SizeofICMPv6Filter), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
            return (_addr__addr_value!, error.As(err)!);
        }

        public static (ptr<Ucred>, error) GetsockoptUcred(long fd, long level, long opt)
        {
            ptr<Ucred> _p0 = default!;
            error _p0 = default!;

            ref Ucred value = ref heap(out ptr<Ucred> _addr_value);
            ref var vallen = ref heap(_Socklen(SizeofUcred), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
            return (_addr__addr_value!, error.As(err)!);
        }

        public static error SetsockoptIPMreqn(long fd, long level, long opt, ptr<IPMreqn> _addr_mreq)
        {
            error err = default!;
            ref IPMreqn mreq = ref _addr_mreq.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), @unsafe.Sizeof(mreq)))!;
        }

        public static (long, long, long, Sockaddr, error) Recvmsg(long fd, slice<byte> p, slice<byte> oob, long flags)
        {
            long n = default;
            long oobn = default;
            long recvflags = default;
            Sockaddr from = default;
            error err = default!;

            ref Msghdr msg = ref heap(out ptr<Msghdr> _addr_msg);
            ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
            msg.Name = (byte.val)(@unsafe.Pointer(_addr_rsa));
            msg.Namelen = uint32(SizeofSockaddrAny);
            ref Iovec iov = ref heap(out ptr<Iovec> _addr_iov);
            if (len(p) > 0L)
            {
                iov.Base = _addr_p[0L];
                iov.SetLen(len(p));
            }

            ref byte dummy = ref heap(out ptr<byte> _addr_dummy);
            if (len(oob) > 0L)
            {
                if (len(p) == 0L)
                {
                    long sockType = default;
                    sockType, err = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);
                    if (err != null)
                    {
                        return ;
                    } 
                    // receive at least one normal byte
                    if (sockType != SOCK_DGRAM)
                    {
                        _addr_iov.Base = _addr_dummy;
                        iov.Base = ref _addr_iov.Base.val;
                        iov.SetLen(1L);

                    }

                }

                msg.Control = _addr_oob[0L];
                msg.SetControllen(len(oob));

            }

            _addr_msg.Iov = _addr_iov;
            msg.Iov = ref _addr_msg.Iov.val;
            msg.Iovlen = 1L;
            n, err = recvmsg(fd, _addr_msg, flags);

            if (err != null)
            {
                return ;
            }

            oobn = int(msg.Controllen);
            recvflags = int(msg.Flags); 
            // source address is only specified if the socket is unconnected
            if (rsa.Addr.Family != AF_UNSPEC)
            {
                from, err = anyToSockaddr(_addr_rsa);
            }

            return ;

        }

        public static error Sendmsg(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            error err = default!;

            _, err = SendmsgN(fd, p, oob, to, flags);
            return ;
        }

        public static (long, error) SendmsgN(long fd, slice<byte> p, slice<byte> oob, Sockaddr to, long flags)
        {
            long n = default;
            error err = default!;

            unsafe.Pointer ptr = default;
            _Socklen salen = default;
            if (to != null)
            {
                error err = default!;
                ptr, salen, err = to.sockaddr();
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

            }

            ref Msghdr msg = ref heap(out ptr<Msghdr> _addr_msg);
            msg.Name = (byte.val)(ptr);
            msg.Namelen = uint32(salen);
            ref Iovec iov = ref heap(out ptr<Iovec> _addr_iov);
            if (len(p) > 0L)
            {
                iov.Base = _addr_p[0L];
                iov.SetLen(len(p));
            }

            ref byte dummy = ref heap(out ptr<byte> _addr_dummy);
            if (len(oob) > 0L)
            {
                if (len(p) == 0L)
                {
                    long sockType = default;
                    sockType, err = GetsockoptInt(fd, SOL_SOCKET, SO_TYPE);
                    if (err != null)
                    {
                        return (0L, error.As(err)!);
                    } 
                    // send at least one normal byte
                    if (sockType != SOCK_DGRAM)
                    {
                        _addr_iov.Base = _addr_dummy;
                        iov.Base = ref _addr_iov.Base.val;
                        iov.SetLen(1L);

                    }

                }

                msg.Control = _addr_oob[0L];
                msg.SetControllen(len(oob));

            }

            _addr_msg.Iov = _addr_iov;
            msg.Iov = ref _addr_msg.Iov.val;
            msg.Iovlen = 1L;
            n, err = sendmsg(fd, _addr_msg, flags);

            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            if (len(oob) > 0L && len(p) == 0L)
            {
                n = 0L;
            }

            return (n, error.As(null!)!);

        }

        // BindToDevice binds the socket associated with fd to device.
        public static error BindToDevice(long fd, @string device)
        {
            error err = default!;

            return error.As(SetsockoptString(fd, SOL_SOCKET, SO_BINDTODEVICE, device))!;
        }

        //sys    ptrace(request int, pid int, addr uintptr, data uintptr) (err error)

        private static (long, error) ptracePeek(long req, long pid, System.UIntPtr addr, slice<byte> @out)
        {
            long count = default;
            error err = default!;
 
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
                err = ptrace(req, pid, addr - addr % sizeofPtr, uintptr(@unsafe.Pointer(_addr_buf[0L])));
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                n += copy(out, buf[addr % sizeofPtr..]);
                out = out[n..];

            } 

            // Remainder.
            while (len(out) > 0L)
            { 
                // We use an internal buffer to guarantee alignment.
                // It's not documented if this is necessary, but we're paranoid.
                err = ptrace(req, pid, addr + uintptr(n), uintptr(@unsafe.Pointer(_addr_buf[0L])));
                if (err != null)
                {
                    return (n, error.As(err)!);
                }

                var copied = copy(out, buf[0L..]);
                n += copied;
                out = out[copied..];

            }


            return (n, error.As(null!)!);

        }

        public static (long, error) PtracePeekText(long pid, System.UIntPtr addr, slice<byte> @out)
        {
            long count = default;
            error err = default!;

            return ptracePeek(PTRACE_PEEKTEXT, pid, addr, out);
        }

        public static (long, error) PtracePeekData(long pid, System.UIntPtr addr, slice<byte> @out)
        {
            long count = default;
            error err = default!;

            return ptracePeek(PTRACE_PEEKDATA, pid, addr, out);
        }

        private static (long, error) ptracePoke(long pokeReq, long peekReq, long pid, System.UIntPtr addr, slice<byte> data)
        {
            long count = default;
            error err = default!;
 
            // As for ptracePeek, we need to align our accesses to deal
            // with the possibility of straddling an invalid page.

            // Leading edge.
            long n = 0L;
            if (addr % sizeofPtr != 0L)
            {
                array<byte> buf = new array<byte>(sizeofPtr);
                err = ptrace(peekReq, pid, addr - addr % sizeofPtr, uintptr(@unsafe.Pointer(_addr_buf[0L])));
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                n += copy(buf[addr % sizeofPtr..], data);
                var word = ((uintptr.val)(@unsafe.Pointer(_addr_buf[0L]))).val;
                err = ptrace(pokeReq, pid, addr - addr % sizeofPtr, word);
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                data = data[n..];

            } 

            // Interior.
            while (len(data) > sizeofPtr)
            {
                word = ((uintptr.val)(@unsafe.Pointer(_addr_data[0L]))).val;
                err = ptrace(pokeReq, pid, addr + uintptr(n), word);
                if (err != null)
                {
                    return (n, error.As(err)!);
                }

                n += sizeofPtr;
                data = data[sizeofPtr..];

            } 

            // Trailing edge.
 

            // Trailing edge.
            if (len(data) > 0L)
            {
                buf = new array<byte>(sizeofPtr);
                err = ptrace(peekReq, pid, addr + uintptr(n), uintptr(@unsafe.Pointer(_addr_buf[0L])));
                if (err != null)
                {
                    return (n, error.As(err)!);
                }

                copy(buf[0L..], data);
                word = ((uintptr.val)(@unsafe.Pointer(_addr_buf[0L]))).val;
                err = ptrace(pokeReq, pid, addr + uintptr(n), word);
                if (err != null)
                {
                    return (n, error.As(err)!);
                }

                n += len(data);

            }

            return (n, error.As(null!)!);

        }

        public static (long, error) PtracePokeText(long pid, System.UIntPtr addr, slice<byte> data)
        {
            long count = default;
            error err = default!;

            return ptracePoke(PTRACE_POKETEXT, PTRACE_PEEKTEXT, pid, addr, data);
        }

        public static (long, error) PtracePokeData(long pid, System.UIntPtr addr, slice<byte> data)
        {
            long count = default;
            error err = default!;

            return ptracePoke(PTRACE_POKEDATA, PTRACE_PEEKDATA, pid, addr, data);
        }

        public static error PtraceGetRegs(long pid, ptr<PtraceRegs> _addr_regsout)
        {
            error err = default!;
            ref PtraceRegs regsout = ref _addr_regsout.val;

            return error.As(ptrace(PTRACE_GETREGS, pid, 0L, uintptr(@unsafe.Pointer(regsout))))!;
        }

        public static error PtraceSetRegs(long pid, ptr<PtraceRegs> _addr_regs)
        {
            error err = default!;
            ref PtraceRegs regs = ref _addr_regs.val;

            return error.As(ptrace(PTRACE_SETREGS, pid, 0L, uintptr(@unsafe.Pointer(regs))))!;
        }

        public static error PtraceSetOptions(long pid, long options)
        {
            error err = default!;

            return error.As(ptrace(PTRACE_SETOPTIONS, pid, 0L, uintptr(options)))!;
        }

        public static (ulong, error) PtraceGetEventMsg(long pid)
        {
            ulong msg = default;
            error err = default!;

            ref _C_long data = ref heap(out ptr<_C_long> _addr_data);
            err = ptrace(PTRACE_GETEVENTMSG, pid, 0L, uintptr(@unsafe.Pointer(_addr_data)));
            msg = uint(data);
            return ;
        }

        public static error PtraceCont(long pid, long signal)
        {
            error err = default!;

            return error.As(ptrace(PTRACE_CONT, pid, 0L, uintptr(signal)))!;
        }

        public static error PtraceSyscall(long pid, long signal)
        {
            error err = default!;

            return error.As(ptrace(PTRACE_SYSCALL, pid, 0L, uintptr(signal)))!;
        }

        public static error PtraceSingleStep(long pid)
        {
            error err = default!;

            return error.As(ptrace(PTRACE_SINGLESTEP, pid, 0L, 0L))!;
        }

        public static error PtraceAttach(long pid)
        {
            error err = default!;

            return error.As(ptrace(PTRACE_ATTACH, pid, 0L, 0L))!;
        }

        public static error PtraceDetach(long pid)
        {
            error err = default!;

            return error.As(ptrace(PTRACE_DETACH, pid, 0L, 0L))!;
        }

        //sys    reboot(magic1 uint, magic2 uint, cmd int, arg string) (err error)

        public static error Reboot(long cmd)
        {
            error err = default!;

            return error.As(reboot(LINUX_REBOOT_MAGIC1, LINUX_REBOOT_MAGIC2, cmd, ""))!;
        }

        public static (long, error) ReadDirent(long fd, slice<byte> buf)
        {
            long n = default;
            error err = default!;

            return Getdents(fd, buf);
        }

        private static (ulong, bool) direntIno(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

            return readInt(buf, @unsafe.Offsetof(new Dirent().Ino), @unsafe.Sizeof(new Dirent().Ino));
        }

        private static (ulong, bool) direntReclen(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

            return readInt(buf, @unsafe.Offsetof(new Dirent().Reclen), @unsafe.Sizeof(new Dirent().Reclen));
        }

        private static (ulong, bool) direntNamlen(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

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
            error err = default!;
 
            // Certain file systems get rather angry and EINVAL if you give
            // them an empty string of data, rather than NULL.
            if (data == "")
            {
                return error.As(mount(source, target, fstype, flags, null))!;
            }

            var (datap, err) = BytePtrFromString(data);
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(mount(source, target, fstype, flags, datap))!;

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
        //sysnb    EpollCreate1(flag int) (fd int, err error)
        //sysnb    EpollCtl(epfd int, op int, fd int, event *EpollEvent) (err error)
        //sys    Fallocate(fd int, mode uint32, off int64, len int64) (err error)
        //sys    Fchdir(fd int) (err error)
        //sys    Fchmod(fd int, mode uint32) (err error)
        //sys    Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
        //sys    fcntl(fd int, cmd int, arg int) (val int, err error)
        //sys    Fdatasync(fd int) (err error)
        //sys    Flock(fd int, how int) (err error)
        //sys    Fsync(fd int) (err error)
        //sys    Getdents(fd int, buf []byte) (n int, err error) = SYS_GETDENTS64
        //sysnb    Getpgid(pid int) (pgid int, err error)

        public static long Getpgrp()
        {
            long pid = default;

            pid, _ = Getpgid(0L);
            return ;
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
        //sys    PivotRoot(newroot string, putold string) (err error) = SYS_PIVOT_ROOT
        //sysnb prlimit(pid int, resource int, newlimit *Rlimit, old *Rlimit) (err error) = SYS_PRLIMIT64
        //sys    read(fd int, p []byte) (n int, err error)
        //sys    Removexattr(path string, attr string) (err error)
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
            error err = default!;

            return error.As(EOPNOTSUPP)!;
        }

        public static error Setgid(long gid)
        {
            error err = default!;

            return error.As(EOPNOTSUPP)!;
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
        //sys    write(fd int, p []byte) (n int, err error)
        //sys    exitThread(code int) (err error) = SYS_EXIT
        //sys    readlen(fd int, p *byte, np int) (n int, err error) = SYS_READ
        //sys    writelen(fd int, p *byte, np int) (n int, err error) = SYS_WRITE

        // mmap varies by architecture; see syscall_linux_*.go.
        //sys    munmap(addr uintptr, length uintptr) (err error)

        private static ptr<mmapper> mapper = addr(new mmapper(active:make(map[*byte][]byte),mmap:mmap,munmap:munmap,));

        public static (slice<byte>, error) Mmap(long fd, long offset, long length, long prot, long flags)
        {
            slice<byte> data = default;
            error err = default!;

            return mapper.Mmap(fd, offset, length, prot, flags);
        }

        public static error Munmap(slice<byte> b)
        {
            error err = default!;

            return error.As(mapper.Munmap(b))!;
        }

        //sys    Madvise(b []byte, advice int) (err error)
        //sys    Mprotect(b []byte, prot int) (err error)
        //sys    Mlock(b []byte) (err error)
        //sys    Munlock(b []byte) (err error)
        //sys    Mlockall(flags int) (err error)
        //sys    Munlockall() (err error)
    }
}
