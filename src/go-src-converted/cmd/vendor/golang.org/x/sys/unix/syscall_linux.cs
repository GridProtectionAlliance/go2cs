// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Linux system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and
// wrap it in our own nicer implementation.

// package unix -- go2cs converted at 2020 October 08 04:47:21 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_linux.go
using binary = go.encoding.binary_package;
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        /*
         * Wrapped
         */
        public static error Access(@string path, uint mode)
        {
            error err = default!;

            return error.As(Faccessat(AT_FDCWD, path, mode, 0L))!;
        }

        public static error Chmod(@string path, uint mode)
        {
            error err = default!;

            return error.As(Fchmodat(AT_FDCWD, path, mode, 0L))!;
        }

        public static error Chown(@string path, long uid, long gid)
        {
            error err = default!;

            return error.As(Fchownat(AT_FDCWD, path, uid, gid, 0L))!;
        }

        public static (long, error) Creat(@string path, uint mode)
        {
            long fd = default;
            error err = default!;

            return Open(path, O_CREAT | O_WRONLY | O_TRUNC, mode);
        }

        //sys    FanotifyInit(flags uint, event_f_flags uint) (fd int, err error)
        //sys    fanotifyMark(fd int, flags uint, mask uint64, dirFd int, pathname *byte) (err error)

        public static error FanotifyMark(long fd, ulong flags, ulong mask, long dirFd, @string pathname)
        {
            error err = default!;

            if (pathname == "")
            {
                return error.As(fanotifyMark(fd, flags, mask, dirFd, null))!;
            }

            var (p, err) = BytePtrFromString(pathname);
            if (err != null)
            {
                return error.As(err)!;
            }

            return error.As(fanotifyMark(fd, flags, mask, dirFd, p))!;

        }

        //sys    fchmodat(dirfd int, path string, mode uint32) (err error)

        public static error Fchmodat(long dirfd, @string path, uint mode, long flags)
        {
            error err = default!;
 
            // Linux fchmodat doesn't support the flags parameter. Mimick glibc's behavior
            // and check the flags. Otherwise the mode would be applied to the symlink
            // destination which is not what the user expects.
            if (flags & ~AT_SYMLINK_NOFOLLOW != 0L)
            {
                return error.As(EINVAL)!;
            }
            else if (flags & AT_SYMLINK_NOFOLLOW != 0L)
            {
                return error.As(EOPNOTSUPP)!;
            }

            return error.As(fchmodat(dirfd, path, mode))!;

        }

        //sys    ioctl(fd int, req uint, arg uintptr) (err error)

        // ioctl itself should not be exposed directly, but additional get/set
        // functions for specific types are permissible.

        // IoctlRetInt performs an ioctl operation specified by req on a device
        // associated with opened file descriptor fd, and returns a non-negative
        // integer that is returned by the ioctl syscall.
        public static (long, error) IoctlRetInt(long fd, ulong req)
        {
            long _p0 = default;
            error _p0 = default!;

            var (ret, _, err) = Syscall(SYS_IOCTL, uintptr(fd), uintptr(req), 0L);
            if (err != 0L)
            {
                return (0L, error.As(err)!);
            }

            return (int(ret), error.As(null!)!);

        }

        // IoctlSetPointerInt performs an ioctl operation which sets an
        // integer value on fd, using the specified request number. The ioctl
        // argument is called with a pointer to the integer value, rather than
        // passing the integer value directly.
        public static error IoctlSetPointerInt(long fd, ulong req, long value)
        {
            ref var v = ref heap(int32(value), out ptr<var> _addr_v);
            return error.As(ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_v))))!;
        }

        public static error IoctlSetRTCTime(long fd, ptr<RTCTime> _addr_value)
        {
            ref RTCTime value = ref _addr_value.val;

            var err = ioctl(fd, RTC_SET_TIME, uintptr(@unsafe.Pointer(value)));
            runtime.KeepAlive(value);
            return error.As(err)!;
        }

        public static (uint, error) IoctlGetUint32(long fd, ulong req)
        {
            uint _p0 = default;
            error _p0 = default!;

            ref uint value = ref heap(out ptr<uint> _addr_value);
            var err = ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_value)));
            return (value, error.As(err)!);
        }

        public static (ptr<RTCTime>, error) IoctlGetRTCTime(long fd)
        {
            ptr<RTCTime> _p0 = default!;
            error _p0 = default!;

            ref RTCTime value = ref heap(out ptr<RTCTime> _addr_value);
            var err = ioctl(fd, RTC_RD_TIME, uintptr(@unsafe.Pointer(_addr_value)));
            return (_addr__addr_value!, error.As(err)!);
        }

        //sys    Linkat(olddirfd int, oldpath string, newdirfd int, newpath string, flags int) (err error)

        public static error Link(@string oldpath, @string newpath)
        {
            error err = default!;

            return error.As(Linkat(AT_FDCWD, oldpath, AT_FDCWD, newpath, 0L))!;
        }

        public static error Mkdir(@string path, uint mode)
        {
            error err = default!;

            return error.As(Mkdirat(AT_FDCWD, path, mode))!;
        }

        public static error Mknod(@string path, uint mode, long dev)
        {
            error err = default!;

            return error.As(Mknodat(AT_FDCWD, path, mode, dev))!;
        }

        public static (long, error) Open(@string path, long mode, uint perm)
        {
            long fd = default;
            error err = default!;

            return openat(AT_FDCWD, path, mode | O_LARGEFILE, perm);
        }

        //sys    openat(dirfd int, path string, flags int, mode uint32) (fd int, err error)

        public static (long, error) Openat(long dirfd, @string path, long flags, uint mode)
        {
            long fd = default;
            error err = default!;

            return openat(dirfd, path, flags | O_LARGEFILE, mode);
        }

        //sys    ppoll(fds *PollFd, nfds int, timeout *Timespec, sigmask *Sigset_t) (n int, err error)

        public static (long, error) Ppoll(slice<PollFd> fds, ptr<Timespec> _addr_timeout, ptr<Sigset_t> _addr_sigmask)
        {
            long n = default;
            error err = default!;
            ref Timespec timeout = ref _addr_timeout.val;
            ref Sigset_t sigmask = ref _addr_sigmask.val;

            if (len(fds) == 0L)
            {
                return ppoll(null, 0L, timeout, sigmask);
            }

            return ppoll(_addr_fds[0L], len(fds), timeout, sigmask);

        }

        //sys    Readlinkat(dirfd int, path string, buf []byte) (n int, err error)

        public static (long, error) Readlink(@string path, slice<byte> buf)
        {
            long n = default;
            error err = default!;

            return Readlinkat(AT_FDCWD, path, buf);
        }

        public static error Rename(@string oldpath, @string newpath)
        {
            error err = default!;

            return error.As(Renameat(AT_FDCWD, oldpath, AT_FDCWD, newpath))!;
        }

        public static error Rmdir(@string path)
        {
            return error.As(Unlinkat(AT_FDCWD, path, AT_REMOVEDIR))!;
        }

        //sys    Symlinkat(oldpath string, newdirfd int, newpath string) (err error)

        public static error Symlink(@string oldpath, @string newpath)
        {
            error err = default!;

            return error.As(Symlinkat(oldpath, AT_FDCWD, newpath))!;
        }

        public static error Unlink(@string path)
        {
            return error.As(Unlinkat(AT_FDCWD, path, 0L))!;
        }

        //sys    Unlinkat(dirfd int, path string, flags int) (err error)

        public static error Utimes(@string path, slice<Timeval> tv)
        {
            if (tv == null)
            {
                var err = utimensat(AT_FDCWD, path, null, 0L);
                if (err != ENOSYS)
                {
                    return error.As(err)!;
                }

                return error.As(utimes(path, null))!;

            }

            if (len(tv) != 2L)
            {
                return error.As(EINVAL)!;
            }

            array<Timespec> ts = new array<Timespec>(2L);
            ts[0L] = NsecToTimespec(TimevalToNsec(tv[0L]));
            ts[1L] = NsecToTimespec(TimevalToNsec(tv[1L]));
            err = utimensat(AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), 0L);
            if (err != ENOSYS)
            {
                return error.As(err)!;
            }

            return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0L]))))!;

        }

        //sys    utimensat(dirfd int, path string, times *[2]Timespec, flags int) (err error)

        public static error UtimesNano(@string path, slice<Timespec> ts)
        {
            if (ts == null)
            {
                var err = utimensat(AT_FDCWD, path, null, 0L);
                if (err != ENOSYS)
                {
                    return error.As(err)!;
                }

                return error.As(utimes(path, null))!;

            }

            if (len(ts) != 2L)
            {
                return error.As(EINVAL)!;
            }

            err = utimensat(AT_FDCWD, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), 0L);
            if (err != ENOSYS)
            {
                return error.As(err)!;
            } 
            // If the utimensat syscall isn't available (utimensat was added to Linux
            // in 2.6.22, Released, 8 July 2007) then fall back to utimes
            array<Timeval> tv = new array<Timeval>(2L);
            for (long i = 0L; i < 2L; i++)
            {
                tv[i] = NsecToTimeval(TimespecToNsec(ts[i]));
            }

            return error.As(utimes(path, new ptr<ptr<array<Timeval>>>(@unsafe.Pointer(_addr_tv[0L]))))!;

        }

        public static error UtimesNanoAt(long dirfd, @string path, slice<Timespec> ts, long flags)
        {
            if (ts == null)
            {
                return error.As(utimensat(dirfd, path, null, flags))!;
            }

            if (len(ts) != 2L)
            {
                return error.As(EINVAL)!;
            }

            return error.As(utimensat(dirfd, path, new ptr<ptr<array<Timespec>>>(@unsafe.Pointer(_addr_ts[0L])), flags))!;

        }

        public static error Futimesat(long dirfd, @string path, slice<Timeval> tv)
        {
            if (tv == null)
            {
                return error.As(futimesat(dirfd, path, null))!;
            }

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

        public static syscall.Signal Signal(this WaitStatus w)
        {
            if (!w.Signaled())
            {
                return -1L;
            }

            return syscall.Signal(w & mask);

        }

        public static syscall.Signal StopSignal(this WaitStatus w)
        {
            if (!w.Stopped())
            {
                return -1L;
            }

            return syscall.Signal(w >> (int)(shift)) & 0xFFUL;

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
            return error.As(Mknod(path, mode | S_IFIFO, 0L))!;
        }

        public static error Mkfifoat(long dirfd, @string path, uint mode)
        {
            return error.As(Mknodat(dirfd, path, mode | S_IFIFO, 0L))!;
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
            if (n >= len(sa.raw.Path))
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

        // SockaddrLinklayer implements the Sockaddr interface for AF_PACKET type sockets.
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

        // SockaddrNetlink implements the Sockaddr interface for AF_NETLINK type sockets.
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

        // SockaddrHCI implements the Sockaddr interface for AF_BLUETOOTH type sockets
        // using the HCI protocol.
        public partial struct SockaddrHCI
        {
            public ushort Dev;
            public ushort Channel;
            public RawSockaddrHCI raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrHCI> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrHCI sa = ref _addr_sa.val;

            sa.raw.Family = AF_BLUETOOTH;
            sa.raw.Dev = sa.Dev;
            sa.raw.Channel = sa.Channel;
            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrHCI, error.As(null!)!);
        }

        // SockaddrL2 implements the Sockaddr interface for AF_BLUETOOTH type sockets
        // using the L2CAP protocol.
        public partial struct SockaddrL2
        {
            public ushort PSM;
            public ushort CID;
            public array<byte> Addr;
            public byte AddrType;
            public RawSockaddrL2 raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrL2> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrL2 sa = ref _addr_sa.val;

            sa.raw.Family = AF_BLUETOOTH;
            ptr<array<byte>> psm = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Psm));
            psm[0L] = byte(sa.PSM);
            psm[1L] = byte(sa.PSM >> (int)(8L));
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Bdaddr[i] = sa.Addr[len(sa.Addr) - 1L - i];
            }

            ptr<array<byte>> cid = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.raw.Cid));
            cid[0L] = byte(sa.CID);
            cid[1L] = byte(sa.CID >> (int)(8L));
            sa.raw.Bdaddr_type = sa.AddrType;
            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrL2, error.As(null!)!);

        }

        // SockaddrRFCOMM implements the Sockaddr interface for AF_BLUETOOTH type sockets
        // using the RFCOMM protocol.
        //
        // Server example:
        //
        //      fd, _ := Socket(AF_BLUETOOTH, SOCK_STREAM, BTPROTO_RFCOMM)
        //      _ = unix.Bind(fd, &unix.SockaddrRFCOMM{
        //          Channel: 1,
        //          Addr:    [6]uint8{0, 0, 0, 0, 0, 0}, // BDADDR_ANY or 00:00:00:00:00:00
        //      })
        //      _ = Listen(fd, 1)
        //      nfd, sa, _ := Accept(fd)
        //      fmt.Printf("conn addr=%v fd=%d", sa.(*unix.SockaddrRFCOMM).Addr, nfd)
        //      Read(nfd, buf)
        //
        // Client example:
        //
        //      fd, _ := Socket(AF_BLUETOOTH, SOCK_STREAM, BTPROTO_RFCOMM)
        //      _ = Connect(fd, &SockaddrRFCOMM{
        //          Channel: 1,
        //          Addr:    [6]byte{0x11, 0x22, 0x33, 0xaa, 0xbb, 0xcc}, // CC:BB:AA:33:22:11
        //      })
        //      Write(fd, []byte(`hello`))
        public partial struct SockaddrRFCOMM
        {
            public array<byte> Addr; // Channel is a designated bluetooth channel, only 1-30 are available for use.
// Since Linux 2.6.7 and further zero value is the first available channel.
            public byte Channel;
            public RawSockaddrRFCOMM raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrRFCOMM> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrRFCOMM sa = ref _addr_sa.val;

            sa.raw.Family = AF_BLUETOOTH;
            sa.raw.Channel = sa.Channel;
            sa.raw.Bdaddr = sa.Addr;
            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrRFCOMM, error.As(null!)!);
        }

        // SockaddrCAN implements the Sockaddr interface for AF_CAN type sockets.
        // The RxID and TxID fields are used for transport protocol addressing in
        // (CAN_TP16, CAN_TP20, CAN_MCNET, and CAN_ISOTP), they can be left with
        // zero values for CAN_RAW and CAN_BCM sockets as they have no meaning.
        //
        // The SockaddrCAN struct must be bound to the socket file descriptor
        // using Bind before the CAN socket can be used.
        //
        //      // Read one raw CAN frame
        //      fd, _ := Socket(AF_CAN, SOCK_RAW, CAN_RAW)
        //      addr := &SockaddrCAN{Ifindex: index}
        //      Bind(fd, addr)
        //      frame := make([]byte, 16)
        //      Read(fd, frame)
        //
        // The full SocketCAN documentation can be found in the linux kernel
        // archives at: https://www.kernel.org/doc/Documentation/networking/can.txt
        public partial struct SockaddrCAN
        {
            public long Ifindex;
            public uint RxID;
            public uint TxID;
            public RawSockaddrCAN raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrCAN> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrCAN sa = ref _addr_sa.val;

            if (sa.Ifindex < 0L || sa.Ifindex > 0x7fffffffUL)
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            sa.raw.Family = AF_CAN;
            sa.raw.Ifindex = int32(sa.Ifindex);
            ptr<array<byte>> rx = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.RxID));
            {
                long i__prev1 = i;

                for (long i = 0L; i < 4L; i++)
                {
                    sa.raw.Addr[i] = rx[i];
                }


                i = i__prev1;
            }
            ptr<array<byte>> tx = new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_sa.TxID));
            {
                long i__prev1 = i;

                for (i = 0L; i < 4L; i++)
                {
                    sa.raw.Addr[i + 4L] = tx[i];
                }


                i = i__prev1;
            }
            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrCAN, error.As(null!)!);

        }

        // SockaddrALG implements the Sockaddr interface for AF_ALG type sockets.
        // SockaddrALG enables userspace access to the Linux kernel's cryptography
        // subsystem. The Type and Name fields specify which type of hash or cipher
        // should be used with a given socket.
        //
        // To create a file descriptor that provides access to a hash or cipher, both
        // Bind and Accept must be used. Once the setup process is complete, input
        // data can be written to the socket, processed by the kernel, and then read
        // back as hash output or ciphertext.
        //
        // Here is an example of using an AF_ALG socket with SHA1 hashing.
        // The initial socket setup process is as follows:
        //
        //      // Open a socket to perform SHA1 hashing.
        //      fd, _ := unix.Socket(unix.AF_ALG, unix.SOCK_SEQPACKET, 0)
        //      addr := &unix.SockaddrALG{Type: "hash", Name: "sha1"}
        //      unix.Bind(fd, addr)
        //      // Note: unix.Accept does not work at this time; must invoke accept()
        //      // manually using unix.Syscall.
        //      hashfd, _, _ := unix.Syscall(unix.SYS_ACCEPT, uintptr(fd), 0, 0)
        //
        // Once a file descriptor has been returned from Accept, it may be used to
        // perform SHA1 hashing. The descriptor is not safe for concurrent use, but
        // may be re-used repeatedly with subsequent Write and Read operations.
        //
        // When hashing a small byte slice or string, a single Write and Read may
        // be used:
        //
        //      // Assume hashfd is already configured using the setup process.
        //      hash := os.NewFile(hashfd, "sha1")
        //      // Hash an input string and read the results. Each Write discards
        //      // previous hash state. Read always reads the current state.
        //      b := make([]byte, 20)
        //      for i := 0; i < 2; i++ {
        //          io.WriteString(hash, "Hello, world.")
        //          hash.Read(b)
        //          fmt.Println(hex.EncodeToString(b))
        //      }
        //      // Output:
        //      // 2ae01472317d1935a84797ec1983ae243fc6aa28
        //      // 2ae01472317d1935a84797ec1983ae243fc6aa28
        //
        // For hashing larger byte slices, or byte streams such as those read from
        // a file or socket, use Sendto with MSG_MORE to instruct the kernel to update
        // the hash digest instead of creating a new one for a given chunk and finalizing it.
        //
        //      // Assume hashfd and addr are already configured using the setup process.
        //      hash := os.NewFile(hashfd, "sha1")
        //      // Hash the contents of a file.
        //      f, _ := os.Open("/tmp/linux-4.10-rc7.tar.xz")
        //      b := make([]byte, 4096)
        //      for {
        //          n, err := f.Read(b)
        //          if err == io.EOF {
        //              break
        //          }
        //          unix.Sendto(hashfd, b[:n], unix.MSG_MORE, addr)
        //      }
        //      hash.Read(b)
        //      fmt.Println(hex.EncodeToString(b))
        //      // Output: 85cdcad0c06eef66f805ecce353bec9accbeecc5
        //
        // For more information, see: http://www.chronox.de/crypto-API/crypto/userspace-if.html.
        public partial struct SockaddrALG
        {
            public @string Type;
            public @string Name;
            public uint Feature;
            public uint Mask;
            public RawSockaddrALG raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrALG> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrALG sa = ref _addr_sa.val;
 
            // Leave room for NUL byte terminator.
            if (len(sa.Type) > 13L)
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            if (len(sa.Name) > 63L)
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            sa.raw.Family = AF_ALG;
            sa.raw.Feat = sa.Feature;
            sa.raw.Mask = sa.Mask;

            var (typ, err) = ByteSliceFromString(sa.Type);
            if (err != null)
            {
                return (null, 0L, error.As(err)!);
            }

            var (name, err) = ByteSliceFromString(sa.Name);
            if (err != null)
            {
                return (null, 0L, error.As(err)!);
            }

            copy(sa.raw.Type[..], typ);
            copy(sa.raw.Name[..], name);

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrALG, error.As(null!)!);

        }

        // SockaddrVM implements the Sockaddr interface for AF_VSOCK type sockets.
        // SockaddrVM provides access to Linux VM sockets: a mechanism that enables
        // bidirectional communication between a hypervisor and its guest virtual
        // machines.
        public partial struct SockaddrVM
        {
            public uint CID;
            public uint Port;
            public RawSockaddrVM raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrVM> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrVM sa = ref _addr_sa.val;

            sa.raw.Family = AF_VSOCK;
            sa.raw.Port = sa.Port;
            sa.raw.Cid = sa.CID;

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrVM, error.As(null!)!);
        }

        public partial struct SockaddrXDP
        {
            public ushort Flags;
            public uint Ifindex;
            public uint QueueID;
            public uint SharedUmemFD;
            public RawSockaddrXDP raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrXDP> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrXDP sa = ref _addr_sa.val;

            sa.raw.Family = AF_XDP;
            sa.raw.Flags = sa.Flags;
            sa.raw.Ifindex = sa.Ifindex;
            sa.raw.Queue_id = sa.QueueID;
            sa.raw.Shared_umem_fd = sa.SharedUmemFD;

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrXDP, error.As(null!)!);
        }

        // This constant mirrors the #define of PX_PROTO_OE in
        // linux/if_pppox.h. We're defining this by hand here instead of
        // autogenerating through mkerrors.sh because including
        // linux/if_pppox.h causes some declaration conflicts with other
        // includes (linux/if_pppox.h includes linux/in.h, which conflicts
        // with netinet/in.h). Given that we only need a single zero constant
        // out of that file, it's cleaner to just define it by hand here.
        private static readonly long px_proto_oe = (long)0L;



        public partial struct SockaddrPPPoE
        {
            public ushort SID;
            public slice<byte> Remote;
            public @string Dev;
            public RawSockaddrPPPoX raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrPPPoE> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrPPPoE sa = ref _addr_sa.val;

            if (len(sa.Remote) != 6L)
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            if (len(sa.Dev) > IFNAMSIZ - 1L)
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            (uint16.val)(@unsafe.Pointer(_addr_sa.raw[0L])).val;

            AF_PPPOX; 
            // This next field is in host-endian byte order. We can't use the
            // same unsafe pointer cast as above, because this value is not
            // 32-bit aligned and some architectures don't allow unaligned
            // access.
            //
            // However, the value of px_proto_oe is 0, so we can use
            // encoding/binary helpers to write the bytes without worrying
            // about the ordering.
            binary.BigEndian.PutUint32(sa.raw[2L..6L], px_proto_oe); 
            // This field is deliberately big-endian, unlike the previous
            // one. The kernel expects SID to be in network byte order.
            binary.BigEndian.PutUint16(sa.raw[6L..8L], sa.SID);
            copy(sa.raw[8L..14L], sa.Remote);
            for (long i = 14L; i < 14L + IFNAMSIZ; i++)
            {
                sa.raw[i] = 0L;
            }

            copy(sa.raw[14L..], sa.Dev);
            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrPPPoX, error.As(null!)!);

        }

        // SockaddrTIPC implements the Sockaddr interface for AF_TIPC type sockets.
        // For more information on TIPC, see: http://tipc.sourceforge.net/.
        public partial struct SockaddrTIPC
        {
            public long Scope; // Addr is the type of address used to manipulate a socket. Addr must be
// one of:
//  - *TIPCSocketAddr: "id" variant in the C addr union
//  - *TIPCServiceRange: "nameseq" variant in the C addr union
//  - *TIPCServiceName: "name" variant in the C addr union
//
// If nil, EINVAL will be returned when the structure is used.
            public TIPCAddr Addr;
            public RawSockaddrTIPC raw;
        }

        // TIPCAddr is implemented by types that can be used as an address for
        // SockaddrTIPC. It is only implemented by *TIPCSocketAddr, *TIPCServiceRange,
        // and *TIPCServiceName.
        public partial interface TIPCAddr
        {
            array<byte> tipcAddrtype();
            array<byte> tipcAddr();
        }

        private static array<byte> tipcAddr(this ptr<TIPCSocketAddr> _addr_sa)
        {
            ref TIPCSocketAddr sa = ref _addr_sa.val;

            array<byte> @out = new array<byte>(12L);
            copy(out[..], (new ptr<ptr<ptr<array<byte>>>>(@unsafe.Pointer(sa)))[..]);
            return out;
        }

        private static byte tipcAddrtype(this ptr<TIPCSocketAddr> _addr_sa)
        {
            ref TIPCSocketAddr sa = ref _addr_sa.val;

            return TIPC_SOCKET_ADDR;
        }

        private static array<byte> tipcAddr(this ptr<TIPCServiceRange> _addr_sa)
        {
            ref TIPCServiceRange sa = ref _addr_sa.val;

            array<byte> @out = new array<byte>(12L);
            copy(out[..], (new ptr<ptr<ptr<array<byte>>>>(@unsafe.Pointer(sa)))[..]);
            return out;
        }

        private static byte tipcAddrtype(this ptr<TIPCServiceRange> _addr_sa)
        {
            ref TIPCServiceRange sa = ref _addr_sa.val;

            return TIPC_SERVICE_RANGE;
        }

        private static array<byte> tipcAddr(this ptr<TIPCServiceName> _addr_sa)
        {
            ref TIPCServiceName sa = ref _addr_sa.val;

            array<byte> @out = new array<byte>(12L);
            copy(out[..], (new ptr<ptr<ptr<array<byte>>>>(@unsafe.Pointer(sa)))[..]);
            return out;
        }

        private static byte tipcAddrtype(this ptr<TIPCServiceName> _addr_sa)
        {
            ref TIPCServiceName sa = ref _addr_sa.val;

            return TIPC_SERVICE_ADDR;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrTIPC> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrTIPC sa = ref _addr_sa.val;

            if (sa.Addr == null)
            {
                return (null, 0L, error.As(EINVAL)!);
            }

            sa.raw.Family = AF_TIPC;
            sa.raw.Scope = int8(sa.Scope);
            sa.raw.Addrtype = sa.Addr.tipcAddrtype();
            sa.raw.Addr = sa.Addr.tipcAddr();

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrTIPC, error.As(null!)!);

        }

        // SockaddrL2TPIP implements the Sockaddr interface for IPPROTO_L2TP/AF_INET sockets.
        public partial struct SockaddrL2TPIP
        {
            public array<byte> Addr;
            public uint ConnId;
            public RawSockaddrL2TPIP raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrL2TPIP> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrL2TPIP sa = ref _addr_sa.val;

            sa.raw.Family = AF_INET;
            sa.raw.Conn_id = sa.ConnId;
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrL2TPIP, error.As(null!)!);

        }

        // SockaddrL2TPIP6 implements the Sockaddr interface for IPPROTO_L2TP/AF_INET6 sockets.
        public partial struct SockaddrL2TPIP6
        {
            public array<byte> Addr;
            public uint ZoneId;
            public uint ConnId;
            public RawSockaddrL2TPIP6 raw;
        }

        private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrL2TPIP6> _addr_sa)
        {
            unsafe.Pointer _p0 = default;
            _Socklen _p0 = default;
            error _p0 = default!;
            ref SockaddrL2TPIP6 sa = ref _addr_sa.val;

            sa.raw.Family = AF_INET6;
            sa.raw.Conn_id = sa.ConnId;
            sa.raw.Scope_id = sa.ZoneId;
            for (long i = 0L; i < len(sa.Addr); i++)
            {
                sa.raw.Addr[i] = sa.Addr[i];
            }

            return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrL2TPIP6, error.As(null!)!);

        }

        private static (Sockaddr, error) anyToSockaddr(long fd, ptr<RawSockaddrAny> _addr_rsa)
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
                var (proto, err) = GetsockoptInt(fd, SOL_SOCKET, SO_PROTOCOL);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }


                if (proto == IPPROTO_L2TP) 
                    pp = (RawSockaddrL2TPIP.val)(@unsafe.Pointer(rsa));
                    sa = @new<SockaddrL2TPIP>();
                    sa.ConnId = pp.Conn_id;
                    {
                        long i__prev1 = i;

                        for (i = 0L; i < len(sa.Addr); i++)
                        {
                            sa.Addr[i] = pp.Addr[i];
                        }


                        i = i__prev1;
                    }
                    return (sa, error.As(null!)!);
                else 
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
                (proto, err) = GetsockoptInt(fd, SOL_SOCKET, SO_PROTOCOL);
                if (err != null)
                {
                    return (null, error.As(err)!);
                }


                if (proto == IPPROTO_L2TP) 
                    pp = (RawSockaddrL2TPIP6.val)(@unsafe.Pointer(rsa));
                    sa = @new<SockaddrL2TPIP6>();
                    sa.ConnId = pp.Conn_id;
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
                else 
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
                            else if (rsa.Addr.Family == AF_VSOCK) 
                pp = (RawSockaddrVM.val)(@unsafe.Pointer(rsa));
                sa = addr(new SockaddrVM(CID:pp.Cid,Port:pp.Port,));
                return (sa, error.As(null!)!);
            else if (rsa.Addr.Family == AF_BLUETOOTH) 
                (proto, err) = GetsockoptInt(fd, SOL_SOCKET, SO_PROTOCOL);
                if (err != null)
                {
                    return (null, error.As(err)!);
                } 
                // only BTPROTO_L2CAP and BTPROTO_RFCOMM can accept connections

                if (proto == BTPROTO_L2CAP) 
                    pp = (RawSockaddrL2.val)(@unsafe.Pointer(rsa));
                    sa = addr(new SockaddrL2(PSM:pp.Psm,CID:pp.Cid,Addr:pp.Bdaddr,AddrType:pp.Bdaddr_type,));
                    return (sa, error.As(null!)!);
                else if (proto == BTPROTO_RFCOMM) 
                    pp = (RawSockaddrRFCOMM.val)(@unsafe.Pointer(rsa));
                    sa = addr(new SockaddrRFCOMM(Channel:pp.Channel,Addr:pp.Bdaddr,));
                    return (sa, error.As(null!)!);
                            else if (rsa.Addr.Family == AF_XDP) 
                pp = (RawSockaddrXDP.val)(@unsafe.Pointer(rsa));
                sa = addr(new SockaddrXDP(Flags:pp.Flags,Ifindex:pp.Ifindex,QueueID:pp.Queue_id,SharedUmemFD:pp.Shared_umem_fd,));
                return (sa, error.As(null!)!);
            else if (rsa.Addr.Family == AF_PPPOX) 
                pp = (RawSockaddrPPPoX.val)(@unsafe.Pointer(rsa));
                if (binary.BigEndian.Uint32(pp[2L..6L]) != px_proto_oe)
                {
                    return (null, error.As(EINVAL)!);
                }

                sa = addr(new SockaddrPPPoE(SID:binary.BigEndian.Uint16(pp[6:8]),Remote:pp[8:14],));
                {
                    long i__prev1 = i;

                    for (i = 14L; i < 14L + IFNAMSIZ; i++)
                    {
                        if (pp[i] == 0L)
                        {
                            sa.Dev = string(pp[14L..i]);
                            break;
                        }

                    }


                    i = i__prev1;
                }
                return (sa, error.As(null!)!);
            else if (rsa.Addr.Family == AF_TIPC) 
                pp = (RawSockaddrTIPC.val)(@unsafe.Pointer(rsa));

                sa = addr(new SockaddrTIPC(Scope:int(pp.Scope),)); 

                // Determine which union variant is present in pp.Addr by checking
                // pp.Addrtype.

                if (pp.Addrtype == TIPC_SERVICE_RANGE) 
                    sa.Addr = (TIPCServiceRange.val)(@unsafe.Pointer(_addr_pp.Addr));
                else if (pp.Addrtype == TIPC_SERVICE_ADDR) 
                    sa.Addr = (TIPCServiceName.val)(@unsafe.Pointer(_addr_pp.Addr));
                else if (pp.Addrtype == TIPC_SOCKET_ADDR) 
                    sa.Addr = (TIPCSocketAddr.val)(@unsafe.Pointer(_addr_pp.Addr));
                else 
                    return (null, error.As(EINVAL)!);
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

            sa, err = anyToSockaddr(fd, _addr_rsa);
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

            sa, err = anyToSockaddr(fd, _addr_rsa);
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

            return anyToSockaddr(fd, _addr_rsa);

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

        public static (ptr<Ucred>, error) GetsockoptUcred(long fd, long level, long opt)
        {
            ptr<Ucred> _p0 = default!;
            error _p0 = default!;

            ref Ucred value = ref heap(out ptr<Ucred> _addr_value);
            ref var vallen = ref heap(_Socklen(SizeofUcred), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
            return (_addr__addr_value!, error.As(err)!);
        }

        public static (ptr<TCPInfo>, error) GetsockoptTCPInfo(long fd, long level, long opt)
        {
            ptr<TCPInfo> _p0 = default!;
            error _p0 = default!;

            ref TCPInfo value = ref heap(out ptr<TCPInfo> _addr_value);
            ref var vallen = ref heap(_Socklen(SizeofTCPInfo), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
            return (_addr__addr_value!, error.As(err)!);
        }

        // GetsockoptString returns the string value of the socket option opt for the
        // socket associated with fd at the given socket level.
        public static (@string, error) GetsockoptString(long fd, long level, long opt)
        {
            @string _p0 = default;
            error _p0 = default!;

            var buf = make_slice<byte>(256L);
            ref var vallen = ref heap(_Socklen(len(buf)), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_buf[0L]), _addr_vallen);
            if (err != null)
            {
                if (err == ERANGE)
                {
                    buf = make_slice<byte>(vallen);
                    err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_buf[0L]), _addr_vallen);
                }

                if (err != null)
                {
                    return ("", error.As(err)!);
                }

            }

            return (string(buf[..vallen - 1L]), error.As(null!)!);

        }

        public static (ptr<TpacketStats>, error) GetsockoptTpacketStats(long fd, long level, long opt)
        {
            ptr<TpacketStats> _p0 = default!;
            error _p0 = default!;

            ref TpacketStats value = ref heap(out ptr<TpacketStats> _addr_value);
            ref var vallen = ref heap(_Socklen(SizeofTpacketStats), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
            return (_addr__addr_value!, error.As(err)!);
        }

        public static (ptr<TpacketStatsV3>, error) GetsockoptTpacketStatsV3(long fd, long level, long opt)
        {
            ptr<TpacketStatsV3> _p0 = default!;
            error _p0 = default!;

            ref TpacketStatsV3 value = ref heap(out ptr<TpacketStatsV3> _addr_value);
            ref var vallen = ref heap(_Socklen(SizeofTpacketStatsV3), out ptr<var> _addr_vallen);
            var err = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
            return (_addr__addr_value!, error.As(err)!);
        }

        public static error SetsockoptIPMreqn(long fd, long level, long opt, ptr<IPMreqn> _addr_mreq)
        {
            error err = default!;
            ref IPMreqn mreq = ref _addr_mreq.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), @unsafe.Sizeof(mreq)))!;
        }

        public static error SetsockoptPacketMreq(long fd, long level, long opt, ptr<PacketMreq> _addr_mreq)
        {
            ref PacketMreq mreq = ref _addr_mreq.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), @unsafe.Sizeof(mreq)))!;
        }

        // SetsockoptSockFprog attaches a classic BPF or an extended BPF program to a
        // socket to filter incoming packets.  See 'man 7 socket' for usage information.
        public static error SetsockoptSockFprog(long fd, long level, long opt, ptr<SockFprog> _addr_fprog)
        {
            ref SockFprog fprog = ref _addr_fprog.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(fprog), @unsafe.Sizeof(fprog)))!;
        }

        public static error SetsockoptCanRawFilter(long fd, long level, long opt, slice<CanFilter> filter)
        {
            unsafe.Pointer p = default;
            if (len(filter) > 0L)
            {
                p = @unsafe.Pointer(_addr_filter[0L]);
            }

            return error.As(setsockopt(fd, level, opt, p, uintptr(len(filter) * SizeofCanFilter)))!;

        }

        public static error SetsockoptTpacketReq(long fd, long level, long opt, ptr<TpacketReq> _addr_tp)
        {
            ref TpacketReq tp = ref _addr_tp.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(tp), @unsafe.Sizeof(tp)))!;
        }

        public static error SetsockoptTpacketReq3(long fd, long level, long opt, ptr<TpacketReq3> _addr_tp)
        {
            ref TpacketReq3 tp = ref _addr_tp.val;

            return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(tp), @unsafe.Sizeof(tp)))!;
        }

        // Keyctl Commands (http://man7.org/linux/man-pages/man2/keyctl.2.html)

        // KeyctlInt calls keyctl commands in which each argument is an int.
        // These commands are KEYCTL_REVOKE, KEYCTL_CHOWN, KEYCTL_CLEAR, KEYCTL_LINK,
        // KEYCTL_UNLINK, KEYCTL_NEGATE, KEYCTL_SET_REQKEY_KEYRING, KEYCTL_SET_TIMEOUT,
        // KEYCTL_ASSUME_AUTHORITY, KEYCTL_SESSION_TO_PARENT, KEYCTL_REJECT,
        // KEYCTL_INVALIDATE, and KEYCTL_GET_PERSISTENT.
        //sys    KeyctlInt(cmd int, arg2 int, arg3 int, arg4 int, arg5 int) (ret int, err error) = SYS_KEYCTL

        // KeyctlBuffer calls keyctl commands in which the third and fourth
        // arguments are a buffer and its length, respectively.
        // These commands are KEYCTL_UPDATE, KEYCTL_READ, and KEYCTL_INSTANTIATE.
        //sys    KeyctlBuffer(cmd int, arg2 int, buf []byte, arg5 int) (ret int, err error) = SYS_KEYCTL

        // KeyctlString calls keyctl commands which return a string.
        // These commands are KEYCTL_DESCRIBE and KEYCTL_GET_SECURITY.
        public static (@string, error) KeyctlString(long cmd, long id)
        {
            @string _p0 = default;
            error _p0 = default!;
 
            // We must loop as the string data may change in between the syscalls.
            // We could allocate a large buffer here to reduce the chance that the
            // syscall needs to be called twice; however, this is unnecessary as
            // the performance loss is negligible.
            slice<byte> buffer = default;
            while (true)
            { 
                // Try to fill the buffer with data
                var (length, err) = KeyctlBuffer(cmd, id, buffer, 0L);
                if (err != null)
                {
                    return ("", error.As(err)!);
                } 

                // Check if the data was written
                if (length <= len(buffer))
                { 
                    // Exclude the null terminator
                    return (string(buffer[..length - 1L]), error.As(null!)!);

                } 

                // Make a bigger buffer if needed
                buffer = make_slice<byte>(length);

            }


        }

        // Keyctl commands with special signatures.

        // KeyctlGetKeyringID implements the KEYCTL_GET_KEYRING_ID command.
        // See the full documentation at:
        // http://man7.org/linux/man-pages/man3/keyctl_get_keyring_ID.3.html
        public static (long, error) KeyctlGetKeyringID(long id, bool create)
        {
            long ringid = default;
            error err = default!;

            long createInt = 0L;
            if (create)
            {
                createInt = 1L;
            }

            return KeyctlInt(KEYCTL_GET_KEYRING_ID, id, createInt, 0L, 0L);

        }

        // KeyctlSetperm implements the KEYCTL_SETPERM command. The perm value is the
        // key handle permission mask as described in the "keyctl setperm" section of
        // http://man7.org/linux/man-pages/man1/keyctl.1.html.
        // See the full documentation at:
        // http://man7.org/linux/man-pages/man3/keyctl_setperm.3.html
        public static error KeyctlSetperm(long id, uint perm)
        {
            var (_, err) = KeyctlInt(KEYCTL_SETPERM, id, int(perm), 0L, 0L);
            return error.As(err)!;
        }

        //sys    keyctlJoin(cmd int, arg2 string) (ret int, err error) = SYS_KEYCTL

        // KeyctlJoinSessionKeyring implements the KEYCTL_JOIN_SESSION_KEYRING command.
        // See the full documentation at:
        // http://man7.org/linux/man-pages/man3/keyctl_join_session_keyring.3.html
        public static (long, error) KeyctlJoinSessionKeyring(@string name)
        {
            long ringid = default;
            error err = default!;

            return keyctlJoin(KEYCTL_JOIN_SESSION_KEYRING, name);
        }

        //sys    keyctlSearch(cmd int, arg2 int, arg3 string, arg4 string, arg5 int) (ret int, err error) = SYS_KEYCTL

        // KeyctlSearch implements the KEYCTL_SEARCH command.
        // See the full documentation at:
        // http://man7.org/linux/man-pages/man3/keyctl_search.3.html
        public static (long, error) KeyctlSearch(long ringid, @string keyType, @string description, long destRingid)
        {
            long id = default;
            error err = default!;

            return keyctlSearch(KEYCTL_SEARCH, ringid, keyType, description, destRingid);
        }

        //sys    keyctlIOV(cmd int, arg2 int, payload []Iovec, arg5 int) (err error) = SYS_KEYCTL

        // KeyctlInstantiateIOV implements the KEYCTL_INSTANTIATE_IOV command. This
        // command is similar to KEYCTL_INSTANTIATE, except that the payload is a slice
        // of Iovec (each of which represents a buffer) instead of a single buffer.
        // See the full documentation at:
        // http://man7.org/linux/man-pages/man3/keyctl_instantiate_iov.3.html
        public static error KeyctlInstantiateIOV(long id, slice<Iovec> payload, long ringid)
        {
            return error.As(keyctlIOV(KEYCTL_INSTANTIATE_IOV, id, payload, ringid))!;
        }

        //sys    keyctlDH(cmd int, arg2 *KeyctlDHParams, buf []byte) (ret int, err error) = SYS_KEYCTL

        // KeyctlDHCompute implements the KEYCTL_DH_COMPUTE command. This command
        // computes a Diffie-Hellman shared secret based on the provide params. The
        // secret is written to the provided buffer and the returned size is the number
        // of bytes written (returning an error if there is insufficient space in the
        // buffer). If a nil buffer is passed in, this function returns the minimum
        // buffer length needed to store the appropriate data. Note that this differs
        // from KEYCTL_READ's behavior which always returns the requested payload size.
        // See the full documentation at:
        // http://man7.org/linux/man-pages/man3/keyctl_dh_compute.3.html
        public static (long, error) KeyctlDHCompute(ptr<KeyctlDHParams> _addr_@params, slice<byte> buffer)
        {
            long size = default;
            error err = default!;
            ref KeyctlDHParams @params = ref _addr_@params.val;

            return keyctlDH(KEYCTL_DH_COMPUTE, params, buffer);
        }

        // KeyctlRestrictKeyring implements the KEYCTL_RESTRICT_KEYRING command. This
        // command limits the set of keys that can be linked to the keyring, regardless
        // of keyring permissions. The command requires the "setattr" permission.
        //
        // When called with an empty keyType the command locks the keyring, preventing
        // any further keys from being linked to the keyring.
        //
        // The "asymmetric" keyType defines restrictions requiring key payloads to be
        // DER encoded X.509 certificates signed by keys in another keyring. Restrictions
        // for "asymmetric" include "builtin_trusted", "builtin_and_secondary_trusted",
        // "key_or_keyring:<key>", and "key_or_keyring:<key>:chain".
        //
        // As of Linux 4.12, only the "asymmetric" keyType defines type-specific
        // restrictions.
        //
        // See the full documentation at:
        // http://man7.org/linux/man-pages/man3/keyctl_restrict_keyring.3.html
        // http://man7.org/linux/man-pages/man2/keyctl.2.html
        public static error KeyctlRestrictKeyring(long ringid, @string keyType, @string restriction)
        {
            if (keyType == "")
            {
                return error.As(keyctlRestrictKeyring(KEYCTL_RESTRICT_KEYRING, ringid))!;
            }

            return error.As(keyctlRestrictKeyringByType(KEYCTL_RESTRICT_KEYRING, ringid, keyType, restriction))!;

        }

        //sys keyctlRestrictKeyringByType(cmd int, arg2 int, keyType string, restriction string) (err error) = SYS_KEYCTL
        //sys keyctlRestrictKeyring(cmd int, arg2 int) (err error) = SYS_KEYCTL

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
                from, err = anyToSockaddr(fd, _addr_rsa);
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

            array<byte> buf = new array<byte>(SizeofPtr); 

            // Leading edge. PEEKTEXT/PEEKDATA don't require aligned
            // access (PEEKUSER warns that it might), but if we don't
            // align our reads, we might straddle an unmapped page
            // boundary and not get the bytes leading up to the page
            // boundary.
            long n = 0L;
            if (addr % SizeofPtr != 0L)
            {
                err = ptrace(req, pid, addr - addr % SizeofPtr, uintptr(@unsafe.Pointer(_addr_buf[0L])));
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                n += copy(out, buf[addr % SizeofPtr..]);
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

        public static (long, error) PtracePeekUser(long pid, System.UIntPtr addr, slice<byte> @out)
        {
            long count = default;
            error err = default!;

            return ptracePeek(PTRACE_PEEKUSR, pid, addr, out);
        }

        private static (long, error) ptracePoke(long pokeReq, long peekReq, long pid, System.UIntPtr addr, slice<byte> data)
        {
            long count = default;
            error err = default!;
 
            // As for ptracePeek, we need to align our accesses to deal
            // with the possibility of straddling an invalid page.

            // Leading edge.
            long n = 0L;
            if (addr % SizeofPtr != 0L)
            {
                array<byte> buf = new array<byte>(SizeofPtr);
                err = ptrace(peekReq, pid, addr - addr % SizeofPtr, uintptr(@unsafe.Pointer(_addr_buf[0L])));
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                n += copy(buf[addr % SizeofPtr..], data);
                var word = ((uintptr.val)(@unsafe.Pointer(_addr_buf[0L]))).val;
                err = ptrace(pokeReq, pid, addr - addr % SizeofPtr, word);
                if (err != null)
                {
                    return (0L, error.As(err)!);
                }

                data = data[n..];

            } 

            // Interior.
            while (len(data) > SizeofPtr)
            {
                word = ((uintptr.val)(@unsafe.Pointer(_addr_data[0L]))).val;
                err = ptrace(pokeReq, pid, addr + uintptr(n), word);
                if (err != null)
                {
                    return (n, error.As(err)!);
                }

                n += SizeofPtr;
                data = data[SizeofPtr..];

            } 

            // Trailing edge.
 

            // Trailing edge.
            if (len(data) > 0L)
            {
                buf = new array<byte>(SizeofPtr);
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

        public static (long, error) PtracePokeUser(long pid, System.UIntPtr addr, slice<byte> data)
        {
            long count = default;
            error err = default!;

            return ptracePoke(PTRACE_POKEUSR, PTRACE_PEEKUSR, pid, addr, data);
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

        public static error PtraceInterrupt(long pid)
        {
            error err = default!;

            return error.As(ptrace(PTRACE_INTERRUPT, pid, 0L, 0L))!;
        }

        public static error PtraceAttach(long pid)
        {
            error err = default!;

            return error.As(ptrace(PTRACE_ATTACH, pid, 0L, 0L))!;
        }

        public static error PtraceSeize(long pid)
        {
            error err = default!;

            return error.As(ptrace(PTRACE_SEIZE, pid, 0L, 0L))!;
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

        public static (long, error) Sendfile(long outfd, long infd, ptr<long> _addr_offset, long count)
        {
            long written = default;
            error err = default!;
            ref long offset = ref _addr_offset.val;

            if (raceenabled)
            {
                raceReleaseMerge(@unsafe.Pointer(_addr_ioSync));
            }

            return sendfile(outfd, infd, offset, count);

        }

        // Sendto
        // Recvfrom
        // Socketpair

        /*
         * Direct access
         */
        //sys    Acct(path string) (err error)
        //sys    AddKey(keyType string, description string, payload []byte, ringid int) (id int, err error)
        //sys    Adjtimex(buf *Timex) (state int, err error)
        //sysnb    Capget(hdr *CapUserHeader, data *CapUserData) (err error)
        //sysnb    Capset(hdr *CapUserHeader, data *CapUserData) (err error)
        //sys    Chdir(path string) (err error)
        //sys    Chroot(path string) (err error)
        //sys    ClockGetres(clockid int32, res *Timespec) (err error)
        //sys    ClockGettime(clockid int32, time *Timespec) (err error)
        //sys    ClockNanosleep(clockid int32, flags int, request *Timespec, remain *Timespec) (err error)
        //sys    Close(fd int) (err error)
        //sys    CopyFileRange(rfd int, roff *int64, wfd int, woff *int64, len int, flags int) (n int, err error)
        //sys    DeleteModule(name string, flags int) (err error)
        //sys    Dup(oldfd int) (fd int, err error)

        public static error Dup2(long oldfd, long newfd)
        { 
            // Android O and newer blocks dup2; riscv and arm64 don't implement dup2.
            if (runtime.GOOS == "android" || runtime.GOARCH == "riscv64" || runtime.GOARCH == "arm64")
            {
                return error.As(Dup3(oldfd, newfd, 0L))!;
            }

            return error.As(dup2(oldfd, newfd))!;

        }

        //sys    Dup3(oldfd int, newfd int, flags int) (err error)
        //sysnb    EpollCreate1(flag int) (fd int, err error)
        //sysnb    EpollCtl(epfd int, op int, fd int, event *EpollEvent) (err error)
        //sys    Eventfd(initval uint, flags int) (fd int, err error) = SYS_EVENTFD2
        //sys    Exit(code int) = SYS_EXIT_GROUP
        //sys    Fallocate(fd int, mode uint32, off int64, len int64) (err error)
        //sys    Fchdir(fd int) (err error)
        //sys    Fchmod(fd int, mode uint32) (err error)
        //sys    Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
        //sys    Fdatasync(fd int) (err error)
        //sys    Fgetxattr(fd int, attr string, dest []byte) (sz int, err error)
        //sys    FinitModule(fd int, params string, flags int) (err error)
        //sys    Flistxattr(fd int, dest []byte) (sz int, err error)
        //sys    Flock(fd int, how int) (err error)
        //sys    Fremovexattr(fd int, attr string) (err error)
        //sys    Fsetxattr(fd int, attr string, dest []byte, flags int) (err error)
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
        //sys    Getrandom(buf []byte, flags int) (n int, err error)
        //sysnb    Getrusage(who int, rusage *Rusage) (err error)
        //sysnb    Getsid(pid int) (sid int, err error)
        //sysnb    Gettid() (tid int)
        //sys    Getxattr(path string, attr string, dest []byte) (sz int, err error)
        //sys    InitModule(moduleImage []byte, params string) (err error)
        //sys    InotifyAddWatch(fd int, pathname string, mask uint32) (watchdesc int, err error)
        //sysnb    InotifyInit1(flags int) (fd int, err error)
        //sysnb    InotifyRmWatch(fd int, watchdesc uint32) (success int, err error)
        //sysnb    Kill(pid int, sig syscall.Signal) (err error)
        //sys    Klogctl(typ int, buf []byte) (n int, err error) = SYS_SYSLOG
        //sys    Lgetxattr(path string, attr string, dest []byte) (sz int, err error)
        //sys    Listxattr(path string, dest []byte) (sz int, err error)
        //sys    Llistxattr(path string, dest []byte) (sz int, err error)
        //sys    Lremovexattr(path string, attr string) (err error)
        //sys    Lsetxattr(path string, attr string, data []byte, flags int) (err error)
        //sys    MemfdCreate(name string, flags int) (fd int, err error)
        //sys    Mkdirat(dirfd int, path string, mode uint32) (err error)
        //sys    Mknodat(dirfd int, path string, mode uint32, dev int) (err error)
        //sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
        //sys    PerfEventOpen(attr *PerfEventAttr, pid int, cpu int, groupFd int, flags int) (fd int, err error)
        //sys    PivotRoot(newroot string, putold string) (err error) = SYS_PIVOT_ROOT
        //sysnb prlimit(pid int, resource int, newlimit *Rlimit, old *Rlimit) (err error) = SYS_PRLIMIT64
        //sys   Prctl(option int, arg2 uintptr, arg3 uintptr, arg4 uintptr, arg5 uintptr) (err error)
        //sys    Pselect(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timespec, sigmask *Sigset_t) (n int, err error) = SYS_PSELECT6
        //sys    read(fd int, p []byte) (n int, err error)
        //sys    Removexattr(path string, attr string) (err error)
        //sys    Renameat2(olddirfd int, oldpath string, newdirfd int, newpath string, flags uint) (err error)
        //sys    RequestKey(keyType string, description string, callback string, destRingid int) (id int, err error)
        //sys    Setdomainname(p []byte) (err error)
        //sys    Sethostname(p []byte) (err error)
        //sysnb    Setpgid(pid int, pgid int) (err error)
        //sysnb    Setsid() (pid int, err error)
        //sysnb    Settimeofday(tv *Timeval) (err error)
        //sys    Setns(fd int, nstype int) (err error)

        // PrctlRetInt performs a prctl operation specified by option and further
        // optional arguments arg2 through arg5 depending on option. It returns a
        // non-negative integer that is returned by the prctl syscall.
        public static (long, error) PrctlRetInt(long option, System.UIntPtr arg2, System.UIntPtr arg3, System.UIntPtr arg4, System.UIntPtr arg5)
        {
            long _p0 = default;
            error _p0 = default!;

            var (ret, _, err) = Syscall6(SYS_PRCTL, uintptr(option), uintptr(arg2), uintptr(arg3), uintptr(arg4), uintptr(arg5), 0L);
            if (err != 0L)
            {
                return (0L, error.As(err)!);
            }

            return (int(ret), error.As(null!)!);

        }

        // issue 1435.
        // On linux Setuid and Setgid only affects the current thread, not the process.
        // This does not match what most callers expect so we must return an error
        // here rather than letting the caller think that the call succeeded.

        public static error Setuid(long uid)
        {
            error err = default!;

            return error.As(EOPNOTSUPP)!;
        }

        public static error Setgid(long uid)
        {
            error err = default!;

            return error.As(EOPNOTSUPP)!;
        }

        // SetfsgidRetGid sets fsgid for current thread and returns previous fsgid set.
        // setfsgid(2) will return a non-nil error only if its caller lacks CAP_SETUID capability.
        // If the call fails due to other reasons, current fsgid will be returned.
        public static (long, error) SetfsgidRetGid(long gid)
        {
            long _p0 = default;
            error _p0 = default!;

            return setfsgid(gid);
        }

        // SetfsuidRetUid sets fsuid for current thread and returns previous fsuid set.
        // setfsgid(2) will return a non-nil error only if its caller lacks CAP_SETUID capability
        // If the call fails due to other reasons, current fsuid will be returned.
        public static (long, error) SetfsuidRetUid(long uid)
        {
            long _p0 = default;
            error _p0 = default!;

            return setfsuid(uid);
        }

        public static error Setfsgid(long gid)
        {
            var (_, err) = setfsgid(gid);
            return error.As(err)!;
        }

        public static error Setfsuid(long uid)
        {
            var (_, err) = setfsuid(uid);
            return error.As(err)!;
        }

        public static (long, error) Signalfd(long fd, ptr<Sigset_t> _addr_sigmask, long flags)
        {
            long newfd = default;
            error err = default!;
            ref Sigset_t sigmask = ref _addr_sigmask.val;

            return signalfd(fd, sigmask, _C__NSIG / 8L, flags);
        }

        //sys    Setpriority(which int, who int, prio int) (err error)
        //sys    Setxattr(path string, attr string, data []byte, flags int) (err error)
        //sys    signalfd(fd int, sigmask *Sigset_t, maskSize uintptr, flags int) (newfd int, err error) = SYS_SIGNALFD4
        //sys    Statx(dirfd int, path string, flags int, mask int, stat *Statx_t) (err error)
        //sys    Sync()
        //sys    Syncfs(fd int) (err error)
        //sysnb    Sysinfo(info *Sysinfo_t) (err error)
        //sys    Tee(rfd int, wfd int, len int, flags int) (n int64, err error)
        //sysnb TimerfdCreate(clockid int, flags int) (fd int, err error)
        //sysnb TimerfdGettime(fd int, currValue *ItimerSpec) (err error)
        //sysnb TimerfdSettime(fd int, flags int, newValue *ItimerSpec, oldValue *ItimerSpec) (err error)
        //sysnb    Tgkill(tgid int, tid int, sig syscall.Signal) (err error)
        //sysnb    Times(tms *Tms) (ticks uintptr, err error)
        //sysnb    Umask(mask int) (oldmask int)
        //sysnb    Uname(buf *Utsname) (err error)
        //sys    Unmount(target string, flags int) (err error) = SYS_UMOUNT2
        //sys    Unshare(flags int) (err error)
        //sys    write(fd int, p []byte) (n int, err error)
        //sys    exitThread(code int) (err error) = SYS_EXIT
        //sys    readlen(fd int, p *byte, np int) (n int, err error) = SYS_READ
        //sys    writelen(fd int, p *byte, np int) (n int, err error) = SYS_WRITE
        //sys    readv(fd int, iovs []Iovec) (n int, err error) = SYS_READV
        //sys    writev(fd int, iovs []Iovec) (n int, err error) = SYS_WRITEV
        //sys    preadv(fd int, iovs []Iovec, offs_l uintptr, offs_h uintptr) (n int, err error) = SYS_PREADV
        //sys    pwritev(fd int, iovs []Iovec, offs_l uintptr, offs_h uintptr) (n int, err error) = SYS_PWRITEV
        //sys    preadv2(fd int, iovs []Iovec, offs_l uintptr, offs_h uintptr, flags int) (n int, err error) = SYS_PREADV2
        //sys    pwritev2(fd int, iovs []Iovec, offs_l uintptr, offs_h uintptr, flags int) (n int, err error) = SYS_PWRITEV2

        private static slice<Iovec> bytes2iovec(slice<slice<byte>> bs)
        {
            var iovecs = make_slice<Iovec>(len(bs));
            foreach (var (i, b) in bs)
            {
                iovecs[i].SetLen(len(b));
                if (len(b) > 0L)
                {
                    iovecs[i].Base = _addr_b[0L];
                }
                else
                {
                    iovecs[i].Base = (byte.val)(@unsafe.Pointer(_addr__zero));
                }

            }
            return iovecs;

        }

        // offs2lohi splits offs into its lower and upper unsigned long. On 64-bit
        // systems, hi will always be 0. On 32-bit systems, offs will be split in half.
        // preadv/pwritev chose this calling convention so they don't need to add a
        // padding-register for alignment on ARM.
        private static (System.UIntPtr, System.UIntPtr) offs2lohi(long offs)
        {
            System.UIntPtr lo = default;
            System.UIntPtr hi = default;

            return (uintptr(offs), uintptr(uint64(offs) >> (int)(SizeofLong)));
        }

        public static (long, error) Readv(long fd, slice<slice<byte>> iovs)
        {
            long n = default;
            error err = default!;

            var iovecs = bytes2iovec(iovs);
            n, err = readv(fd, iovecs);
            readvRacedetect(iovecs, n, err);
            return (n, error.As(err)!);
        }

        public static (long, error) Preadv(long fd, slice<slice<byte>> iovs, long offset)
        {
            long n = default;
            error err = default!;

            var iovecs = bytes2iovec(iovs);
            var (lo, hi) = offs2lohi(offset);
            n, err = preadv(fd, iovecs, lo, hi);
            readvRacedetect(iovecs, n, err);
            return (n, error.As(err)!);
        }

        public static (long, error) Preadv2(long fd, slice<slice<byte>> iovs, long offset, long flags)
        {
            long n = default;
            error err = default!;

            var iovecs = bytes2iovec(iovs);
            var (lo, hi) = offs2lohi(offset);
            n, err = preadv2(fd, iovecs, lo, hi, flags);
            readvRacedetect(iovecs, n, err);
            return (n, error.As(err)!);
        }

        private static void readvRacedetect(slice<Iovec> iovecs, long n, error err)
        {
            if (!raceenabled)
            {
                return ;
            }

            for (long i = 0L; n > 0L && i < len(iovecs); i++)
            {
                var m = int(iovecs[i].Len);
                if (m > n)
                {
                    m = n;
                }

                n -= m;
                if (m > 0L)
                {
                    raceWriteRange(@unsafe.Pointer(iovecs[i].Base), m);
                }

            }

            if (err == null)
            {
                raceAcquire(@unsafe.Pointer(_addr_ioSync));
            }

        }

        public static (long, error) Writev(long fd, slice<slice<byte>> iovs)
        {
            long n = default;
            error err = default!;

            var iovecs = bytes2iovec(iovs);
            if (raceenabled)
            {
                raceReleaseMerge(@unsafe.Pointer(_addr_ioSync));
            }

            n, err = writev(fd, iovecs);
            writevRacedetect(iovecs, n);
            return (n, error.As(err)!);

        }

        public static (long, error) Pwritev(long fd, slice<slice<byte>> iovs, long offset)
        {
            long n = default;
            error err = default!;

            var iovecs = bytes2iovec(iovs);
            if (raceenabled)
            {
                raceReleaseMerge(@unsafe.Pointer(_addr_ioSync));
            }

            var (lo, hi) = offs2lohi(offset);
            n, err = pwritev(fd, iovecs, lo, hi);
            writevRacedetect(iovecs, n);
            return (n, error.As(err)!);

        }

        public static (long, error) Pwritev2(long fd, slice<slice<byte>> iovs, long offset, long flags)
        {
            long n = default;
            error err = default!;

            var iovecs = bytes2iovec(iovs);
            if (raceenabled)
            {
                raceReleaseMerge(@unsafe.Pointer(_addr_ioSync));
            }

            var (lo, hi) = offs2lohi(offset);
            n, err = pwritev2(fd, iovecs, lo, hi, flags);
            writevRacedetect(iovecs, n);
            return (n, error.As(err)!);

        }

        private static void writevRacedetect(slice<Iovec> iovecs, long n)
        {
            if (!raceenabled)
            {
                return ;
            }

            for (long i = 0L; n > 0L && i < len(iovecs); i++)
            {
                var m = int(iovecs[i].Len);
                if (m > n)
                {
                    m = n;
                }

                n -= m;
                if (m > 0L)
                {
                    raceReadRange(@unsafe.Pointer(iovecs[i].Base), m);
                }

            }


        }

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
        //sys    Mlockall(flags int) (err error)
        //sys    Msync(b []byte, flags int) (err error)
        //sys    Munlock(b []byte) (err error)
        //sys    Munlockall() (err error)

        // Vmsplice splices user pages from a slice of Iovecs into a pipe specified by fd,
        // using the specified flags.
        public static (long, error) Vmsplice(long fd, slice<Iovec> iovs, long flags)
        {
            long _p0 = default;
            error _p0 = default!;

            unsafe.Pointer p = default;
            if (len(iovs) > 0L)
            {
                p = @unsafe.Pointer(_addr_iovs[0L]);
            }

            var (n, _, errno) = Syscall6(SYS_VMSPLICE, uintptr(fd), uintptr(p), uintptr(len(iovs)), uintptr(flags), 0L, 0L);
            if (errno != 0L)
            {
                return (0L, error.As(syscall.Errno(errno))!);
            }

            return (int(n), error.As(null!)!);

        }

        //sys    faccessat(dirfd int, path string, mode uint32) (err error)

        public static error Faccessat(long dirfd, @string path, uint mode, long flags)
        {
            error err = default!;

            if (flags & ~(AT_SYMLINK_NOFOLLOW | AT_EACCESS) != 0L)
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
                var err = Fstatat(dirfd, path, _addr_st, flags & AT_SYMLINK_NOFOLLOW);

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
            if (flags & AT_EACCESS != 0L)
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
                if (flags & AT_EACCESS != 0L)
                {
                    gid = Getegid();
                }
                else
                {
                    gid = Getgid();
                }

                if (uint32(gid) == st.Gid)
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

        //sys nameToHandleAt(dirFD int, pathname string, fh *fileHandle, mountID *_C_int, flags int) (err error) = SYS_NAME_TO_HANDLE_AT
        //sys openByHandleAt(mountFD int, fh *fileHandle, flags int) (fd int, err error) = SYS_OPEN_BY_HANDLE_AT

        // fileHandle is the argument to nameToHandleAt and openByHandleAt. We
        // originally tried to generate it via unix/linux/types.go with "type
        // fileHandle C.struct_file_handle" but that generated empty structs
        // for mips64 and mips64le. Instead, hard code it for now (it's the
        // same everywhere else) until the mips64 generator issue is fixed.
        private partial struct fileHandle
        {
            public uint Bytes;
            public int Type;
        }

        // FileHandle represents the C struct file_handle used by
        // name_to_handle_at (see NameToHandleAt) and open_by_handle_at (see
        // OpenByHandleAt).
        public partial struct FileHandle
        {
            public ref ptr<fileHandle> ptr<fileHandle> => ref ptr<fileHandle>_ptr;
        }

        // NewFileHandle constructs a FileHandle.
        public static FileHandle NewFileHandle(int handleType, slice<byte> handle)
        {
            const var hdrSize = (var)@unsafe.Sizeof(new fileHandle());

            var buf = make_slice<byte>(hdrSize + uintptr(len(handle)));
            copy(buf[hdrSize..], handle);
            var fh = (fileHandle.val)(@unsafe.Pointer(_addr_buf[0L]));
            fh.Type = handleType;
            fh.Bytes = uint32(len(handle));
            return new FileHandle(fh);
        }

        private static long Size(this ptr<FileHandle> _addr_fh)
        {
            ref FileHandle fh = ref _addr_fh.val;

            return int(fh.fileHandle.Bytes);
        }
        private static int Type(this ptr<FileHandle> _addr_fh)
        {
            ref FileHandle fh = ref _addr_fh.val;

            return fh.fileHandle.Type;
        }
        private static slice<byte> Bytes(this ptr<FileHandle> _addr_fh)
        {
            ref FileHandle fh = ref _addr_fh.val;

            var n = fh.Size();
            if (n == 0L)
            {
                return null;
            }

            return new ptr<ptr<array<byte>>>(@unsafe.Pointer(uintptr(@unsafe.Pointer(_addr_fh.fileHandle.Type)) + 4L)).slice(-1, n, n);

        }

        // NameToHandleAt wraps the name_to_handle_at system call; it obtains
        // a handle for a path name.
        public static (FileHandle, long, error) NameToHandleAt(long dirfd, @string path, long flags)
        {
            FileHandle handle = default;
            long mountID = default;
            error err = default!;

            ref _C_int mid = ref heap(out ptr<_C_int> _addr_mid); 
            // Try first with a small buffer, assuming the handle will
            // only be 32 bytes.
            var size = uint32(32L + @unsafe.Sizeof(new fileHandle()));
            var didResize = false;
            while (true)
            {
                var buf = make_slice<byte>(size);
                var fh = (fileHandle.val)(@unsafe.Pointer(_addr_buf[0L]));
                fh.Bytes = size - uint32(@unsafe.Sizeof(new fileHandle()));
                err = nameToHandleAt(dirfd, path, fh, _addr_mid, flags);
                if (err == EOVERFLOW)
                {
                    if (didResize)
                    { 
                        // We shouldn't need to resize more than once
                        return ;

                    }

                    didResize = true;
                    size = fh.Bytes + uint32(@unsafe.Sizeof(new fileHandle()));
                    continue;

                }

                if (err != null)
                {
                    return ;
                }

                return (new FileHandle(fh), int(mid), error.As(null!)!);

            }


        }

        // OpenByHandleAt wraps the open_by_handle_at system call; it opens a
        // file via a handle as previously returned by NameToHandleAt.
        public static (long, error) OpenByHandleAt(long mountFD, FileHandle handle, long flags)
        {
            long fd = default;
            error err = default!;

            return openByHandleAt(mountFD, handle.fileHandle, flags);
        }

        // Klogset wraps the sys_syslog system call; it sets console_loglevel to
        // the value specified by arg and passes a dummy pointer to bufp.
        public static error Klogset(long typ, long arg)
        {
            error err = default!;

            unsafe.Pointer p = default;
            var (_, _, errno) = Syscall(SYS_SYSLOG, uintptr(typ), uintptr(p), uintptr(arg));
            if (errno != 0L)
            {
                return error.As(errnoErr(errno))!;
            }

            return error.As(null!)!;

        }

        /*
         * Unimplemented
         */
        // AfsSyscall
        // Alarm
        // ArchPrctl
        // Brk
        // ClockNanosleep
        // ClockSettime
        // Clone
        // EpollCtlOld
        // EpollPwait
        // EpollWaitOld
        // Execve
        // Fork
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
        // IoprioGet
        // IoprioSet
        // KexecLoad
        // LookupDcookie
        // Mbind
        // MigratePages
        // Mincore
        // ModifyLdt
        // Mount
        // MovePages
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
        // Nfsservctl
        // Personality
        // Pselect6
        // Ptrace
        // Putpmsg
        // Quotactl
        // Readahead
        // Readv
        // RemapFilePages
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
        // SchedGetparam
        // SchedGetscheduler
        // SchedRrGetInterval
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
        // Swapoff
        // Swapon
        // Sysfs
        // TimerCreate
        // TimerDelete
        // TimerGetoverrun
        // TimerGettime
        // TimerSettime
        // Tkill (obsolete)
        // Tuxcall
        // Umount2
        // Uselib
        // Utimensat
        // Vfork
        // Vhangup
        // Vserver
        // Waitid
        // _Sysctl
    }
}}}}}}
