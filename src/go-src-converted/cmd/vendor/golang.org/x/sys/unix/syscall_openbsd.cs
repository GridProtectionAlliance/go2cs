// Copyright 2009,2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// OpenBSD system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and wrap
// it in our own nicer implementation, either here or in
// syscall_bsd.go or syscall_unix.go.

// package unix -- go2cs converted at 2020 October 09 05:56:56 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_openbsd.go
using sort = go.sort_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        // SockaddrDatalink implements the Sockaddr interface for AF_LINK type sockets.
        public partial struct SockaddrDatalink
        {
            public byte Len;
            public byte Family;
            public ushort Index;
            public byte Type;
            public byte Nlen;
            public byte Alen;
            public byte Slen;
            public array<sbyte> Data;
            public RawSockaddrDatalink raw;
        }

        public static (System.UIntPtr, System.UIntPtr, syscall.Errno) Syscall9(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9)
;

        private static (slice<_C_int>, error) nametomib(@string name)
        {
            slice<_C_int> mib = default;
            error err = default!;

            var i = sort.Search(len(sysctlMib), i =>
            {>>MARKER:FUNCTION_Syscall9_BLOCK_PREFIX<<
                return sysctlMib[i].ctlname >= name;
            });
            if (i < len(sysctlMib) && sysctlMib[i].ctlname == name)
            {
                return (sysctlMib[i].ctloid, error.As(null!)!);
            }

            return (null, error.As(EINVAL)!);

        }

        private static (ulong, bool) direntIno(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

            return readInt(buf, @unsafe.Offsetof(new Dirent().Fileno), @unsafe.Sizeof(new Dirent().Fileno));
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

            return readInt(buf, @unsafe.Offsetof(new Dirent().Namlen), @unsafe.Sizeof(new Dirent().Namlen));
        }

        public static (ptr<Uvmexp>, error) SysctlUvmexp(@string name)
        {
            ptr<Uvmexp> _p0 = default!;
            error _p0 = default!;

            var (mib, err) = sysctlmib(name);
            if (err != null)
            {
                return (_addr_null!, error.As(err)!);
            }

            ref var n = ref heap(uintptr(SizeofUvmexp), out ptr<var> _addr_n);
            ref Uvmexp u = ref heap(out ptr<Uvmexp> _addr_u);
            {
                var err = sysctl(mib, (byte.val)(@unsafe.Pointer(_addr_u)), _addr_n, null, 0L);

                if (err != null)
                {
                    return (_addr_null!, error.As(err)!);
                }

            }

            if (n != SizeofUvmexp)
            {
                return (_addr_null!, error.As(EIO)!);
            }

            return (_addr__addr_u!, error.As(null!)!);

        }

        public static error Pipe(slice<long> p)
        {
            error err = default!;

            return error.As(Pipe2(p, 0L))!;
        }

        //sysnb    pipe2(p *[2]_C_int, flags int) (err error)
        public static error Pipe2(slice<long> p, long flags)
        {
            if (len(p) != 2L)
            {
                return error.As(EINVAL)!;
            }

            ref array<_C_int> pp = ref heap(new array<_C_int>(2L), out ptr<array<_C_int>> _addr_pp);
            var err = pipe2(_addr_pp, flags);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return error.As(err)!;

        }

        //sys Getdents(fd int, buf []byte) (n int, err error)
        public static (long, error) Getdirentries(long fd, slice<byte> buf, ptr<System.UIntPtr> _addr_basep)
        {
            long n = default;
            error err = default!;
            ref System.UIntPtr basep = ref _addr_basep.val;

            n, err = Getdents(fd, buf);
            if (err != null || basep == null)
            {
                return ;
            }

            long off = default;
            off, err = Seek(fd, 0L, 1L);
            if (err != null)
            {
                basep = ~uintptr(0L);
                return ;
            }

            basep = uintptr(off);
            if (@unsafe.Sizeof(basep) == 8L)
            {
                return ;
            }

            if (off >> (int)(32L) != 0L)
            { 
                // We can't stuff the offset back into a uintptr, so any
                // future calls would be suspect. Generate an error.
                // EIO was allowed by getdirentries.
                err = EIO;

            }

            return ;

        }

        public static readonly var ImplementsGetwd = true;

        //sys    Getcwd(buf []byte) (n int, err error) = SYS___GETCWD



        //sys    Getcwd(buf []byte) (n int, err error) = SYS___GETCWD

        public static (@string, error) Getwd()
        {
            @string _p0 = default;
            error _p0 = default!;

            array<byte> buf = new array<byte>(PathMax);
            var (_, err) = Getcwd(buf[0L..]);
            if (err != null)
            {
                return ("", error.As(err)!);
            }

            var n = clen(buf[..]);
            if (n < 1L)
            {
                return ("", error.As(EINVAL)!);
            }

            return (string(buf[..n]), error.As(null!)!);

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

            return sendfile(outfd, infd, _addr_offset, count);

        }

        // TODO
        private static (long, error) sendfile(long outfd, long infd, ptr<long> _addr_offset, long count)
        {
            long written = default;
            error err = default!;
            ref long offset = ref _addr_offset.val;

            return (-1L, error.As(ENOSYS)!);
        }

        public static (long, error) Getfsstat(slice<Statfs_t> buf, long flags)
        {
            long n = default;
            error err = default!;

            unsafe.Pointer _p0 = default;
            System.UIntPtr bufsize = default;
            if (len(buf) > 0L)
            {
                _p0 = @unsafe.Pointer(_addr_buf[0L]);
                bufsize = @unsafe.Sizeof(new Statfs_t()) * uintptr(len(buf));
            }

            var (r0, _, e1) = Syscall(SYS_GETFSSTAT, uintptr(_p0), bufsize, uintptr(flags));
            n = int(r0);
            if (e1 != 0L)
            {
                err = e1;
            }

            return ;

        }

        private static error setattrlistTimes(@string path, slice<Timespec> times, long flags)
        { 
            // used on Darwin for UtimesNano
            return error.As(ENOSYS)!;

        }

        //sys    ioctl(fd int, req uint, arg uintptr) (err error)

        //sys   sysctl(mib []_C_int, old *byte, oldlen *uintptr, new *byte, newlen uintptr) (err error) = SYS___SYSCTL

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

        public static error Uname(ptr<Utsname> _addr_uname)
        {
            ref Utsname uname = ref _addr_uname.val;

            _C_int mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_OSTYPE });
            ref var n = ref heap(@unsafe.Sizeof(uname.Sysname), out ptr<var> _addr_n);
            {
                var err__prev1 = err;

                var err = sysctl(mib, _addr_uname.Sysname[0L], _addr_n, null, 0L);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_HOSTNAME });
            n = @unsafe.Sizeof(uname.Nodename);
            {
                var err__prev1 = err;

                err = sysctl(mib, _addr_uname.Nodename[0L], _addr_n, null, 0L);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_OSRELEASE });
            n = @unsafe.Sizeof(uname.Release);
            {
                var err__prev1 = err;

                err = sysctl(mib, _addr_uname.Release[0L], _addr_n, null, 0L);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_VERSION });
            n = @unsafe.Sizeof(uname.Version);
            {
                var err__prev1 = err;

                err = sysctl(mib, _addr_uname.Version[0L], _addr_n, null, 0L);

                if (err != null)
                {
                    return error.As(err)!;
                } 

                // The version might have newlines or tabs in it, convert them to
                // spaces.

                err = err__prev1;

            } 

            // The version might have newlines or tabs in it, convert them to
            // spaces.
            foreach (var (i, b) in uname.Version)
            {
                if (b == '\n' || b == '\t')
                {
                    if (i == len(uname.Version) - 1L)
                    {
                        uname.Version[i] = 0L;
                    }
                    else
                    {
                        uname.Version[i] = ' ';
                    }

                }

            }
            mib = new slice<_C_int>(new _C_int[] { CTL_HW, HW_MACHINE });
            n = @unsafe.Sizeof(uname.Machine);
            {
                var err__prev1 = err;

                err = sysctl(mib, _addr_uname.Machine[0L], _addr_n, null, 0L);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }


            return error.As(null!)!;

        }

        /*
         * Exposed directly
         */
        //sys    Access(path string, mode uint32) (err error)
        //sys    Adjtime(delta *Timeval, olddelta *Timeval) (err error)
        //sys    Chdir(path string) (err error)
        //sys    Chflags(path string, flags int) (err error)
        //sys    Chmod(path string, mode uint32) (err error)
        //sys    Chown(path string, uid int, gid int) (err error)
        //sys    Chroot(path string) (err error)
        //sys    Close(fd int) (err error)
        //sys    Dup(fd int) (nfd int, err error)
        //sys    Dup2(from int, to int) (err error)
        //sys    Dup3(from int, to int, flags int) (err error)
        //sys    Exit(code int)
        //sys    Faccessat(dirfd int, path string, mode uint32, flags int) (err error)
        //sys    Fchdir(fd int) (err error)
        //sys    Fchflags(fd int, flags int) (err error)
        //sys    Fchmod(fd int, mode uint32) (err error)
        //sys    Fchmodat(dirfd int, path string, mode uint32, flags int) (err error)
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
        //sys    Flock(fd int, how int) (err error)
        //sys    Fpathconf(fd int, name int) (val int, err error)
        //sys    Fstat(fd int, stat *Stat_t) (err error)
        //sys    Fstatat(fd int, path string, stat *Stat_t, flags int) (err error)
        //sys    Fstatfs(fd int, stat *Statfs_t) (err error)
        //sys    Fsync(fd int) (err error)
        //sys    Ftruncate(fd int, length int64) (err error)
        //sysnb    Getegid() (egid int)
        //sysnb    Geteuid() (uid int)
        //sysnb    Getgid() (gid int)
        //sysnb    Getpgid(pid int) (pgid int, err error)
        //sysnb    Getpgrp() (pgrp int)
        //sysnb    Getpid() (pid int)
        //sysnb    Getppid() (ppid int)
        //sys    Getpriority(which int, who int) (prio int, err error)
        //sysnb    Getrlimit(which int, lim *Rlimit) (err error)
        //sysnb    Getrtable() (rtable int, err error)
        //sysnb    Getrusage(who int, rusage *Rusage) (err error)
        //sysnb    Getsid(pid int) (sid int, err error)
        //sysnb    Gettimeofday(tv *Timeval) (err error)
        //sysnb    Getuid() (uid int)
        //sys    Issetugid() (tainted bool)
        //sys    Kill(pid int, signum syscall.Signal) (err error)
        //sys    Kqueue() (fd int, err error)
        //sys    Lchown(path string, uid int, gid int) (err error)
        //sys    Link(path string, link string) (err error)
        //sys    Linkat(pathfd int, path string, linkfd int, link string, flags int) (err error)
        //sys    Listen(s int, backlog int) (err error)
        //sys    Lstat(path string, stat *Stat_t) (err error)
        //sys    Mkdir(path string, mode uint32) (err error)
        //sys    Mkdirat(dirfd int, path string, mode uint32) (err error)
        //sys    Mkfifo(path string, mode uint32) (err error)
        //sys    Mkfifoat(dirfd int, path string, mode uint32) (err error)
        //sys    Mknod(path string, mode uint32, dev int) (err error)
        //sys    Mknodat(dirfd int, path string, mode uint32, dev int) (err error)
        //sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
        //sys    Open(path string, mode int, perm uint32) (fd int, err error)
        //sys    Openat(dirfd int, path string, mode int, perm uint32) (fd int, err error)
        //sys    Pathconf(path string, name int) (val int, err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error)
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error)
        //sys    read(fd int, p []byte) (n int, err error)
        //sys    Readlink(path string, buf []byte) (n int, err error)
        //sys    Readlinkat(dirfd int, path string, buf []byte) (n int, err error)
        //sys    Rename(from string, to string) (err error)
        //sys    Renameat(fromfd int, from string, tofd int, to string) (err error)
        //sys    Revoke(path string) (err error)
        //sys    Rmdir(path string) (err error)
        //sys    Seek(fd int, offset int64, whence int) (newoffset int64, err error) = SYS_LSEEK
        //sys    Select(nfd int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (n int, err error)
        //sysnb    Setegid(egid int) (err error)
        //sysnb    Seteuid(euid int) (err error)
        //sysnb    Setgid(gid int) (err error)
        //sys    Setlogin(name string) (err error)
        //sysnb    Setpgid(pid int, pgid int) (err error)
        //sys    Setpriority(which int, who int, prio int) (err error)
        //sysnb    Setregid(rgid int, egid int) (err error)
        //sysnb    Setreuid(ruid int, euid int) (err error)
        //sysnb    Setresgid(rgid int, egid int, sgid int) (err error)
        //sysnb    Setresuid(ruid int, euid int, suid int) (err error)
        //sysnb    Setrlimit(which int, lim *Rlimit) (err error)
        //sysnb    Setrtable(rtable int) (err error)
        //sysnb    Setsid() (pid int, err error)
        //sysnb    Settimeofday(tp *Timeval) (err error)
        //sysnb    Setuid(uid int) (err error)
        //sys    Stat(path string, stat *Stat_t) (err error)
        //sys    Statfs(path string, stat *Statfs_t) (err error)
        //sys    Symlink(path string, link string) (err error)
        //sys    Symlinkat(oldpath string, newdirfd int, newpath string) (err error)
        //sys    Sync() (err error)
        //sys    Truncate(path string, length int64) (err error)
        //sys    Umask(newmask int) (oldmask int)
        //sys    Unlink(path string) (err error)
        //sys    Unlinkat(dirfd int, path string, flags int) (err error)
        //sys    Unmount(path string, flags int) (err error)
        //sys    write(fd int, p []byte) (n int, err error)
        //sys    mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error)
        //sys    munmap(addr uintptr, length uintptr) (err error)
        //sys    readlen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_READ
        //sys    writelen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_WRITE
        //sys    utimensat(dirfd int, path string, times *[2]Timespec, flags int) (err error)

        /*
         * Unimplemented
         */
        // __getcwd
        // __semctl
        // __syscall
        // __sysctl
        // adjfreq
        // break
        // clock_getres
        // clock_gettime
        // clock_settime
        // closefrom
        // execve
        // fhopen
        // fhstat
        // fhstatfs
        // fork
        // futimens
        // getfh
        // getgid
        // getitimer
        // getlogin
        // getresgid
        // getresuid
        // getthrid
        // ktrace
        // lfs_bmapv
        // lfs_markv
        // lfs_segclean
        // lfs_segwait
        // mincore
        // minherit
        // mount
        // mquery
        // msgctl
        // msgget
        // msgrcv
        // msgsnd
        // nfssvc
        // nnpfspioctl
        // preadv
        // profil
        // pwritev
        // quotactl
        // readv
        // reboot
        // renameat
        // rfork
        // sched_yield
        // semget
        // semop
        // setgroups
        // setitimer
        // setsockopt
        // shmat
        // shmctl
        // shmdt
        // shmget
        // sigaction
        // sigaltstack
        // sigpending
        // sigprocmask
        // sigreturn
        // sigsuspend
        // sysarch
        // syscall
        // threxit
        // thrsigdivert
        // thrsleep
        // thrwakeup
        // vfork
        // writev
    }
}}}}}}
