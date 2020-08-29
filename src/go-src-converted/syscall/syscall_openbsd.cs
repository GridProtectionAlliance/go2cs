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

// package syscall -- go2cs converted at 2020 August 29 08:38:15 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_openbsd.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
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

        public static (System.UIntPtr, System.UIntPtr, Errno) Syscall9(System.UIntPtr num, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9)
;

        private static (slice<_C_int>, error) nametomib(@string name)
        {
            // Perform lookup via a binary search
            long left = 0L;
            var right = len(sysctlMib) - 1L;
            while (true)
            {>>MARKER:FUNCTION_Syscall9_BLOCK_PREFIX<<
                var idx = left + (right - left) / 2L;

                if (name == sysctlMib[idx].ctlname) 
                    return (sysctlMib[idx].ctloid, null);
                else if (name > sysctlMib[idx].ctlname) 
                    left = idx + 1L;
                else 
                    right = idx - 1L;
                                if (left > right)
                {
                    break;
                }
            }

            return (null, EINVAL);
        }

        private static (ulong, bool) direntIno(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Fileno), @unsafe.Sizeof(new Dirent().Fileno));
        }

        private static (ulong, bool) direntReclen(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Reclen), @unsafe.Sizeof(new Dirent().Reclen));
        }

        private static (ulong, bool) direntNamlen(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Namlen), @unsafe.Sizeof(new Dirent().Namlen));
        }

        //sysnb pipe(p *[2]_C_int) (err error)
        public static error Pipe(slice<long> p)
        {
            if (len(p) != 2L)
            {
                return error.As(EINVAL);
            }
            array<_C_int> pp = new array<_C_int>(2L);
            err = pipe(ref pp);
            p[0L] = int(pp[0L]);
            p[1L] = int(pp[1L]);
            return;
        }

        //sys getdents(fd int, buf []byte) (n int, err error)
        public static (long, error) Getdirentries(long fd, slice<byte> buf, ref System.UIntPtr basep)
        {
            return getdents(fd, buf);
        }

        // TODO
        private static (long, error) sendfile(long outfd, long infd, ref long offset, long count)
        {
            return (-1L, ENOSYS);
        }

        public static (long, error) Getfsstat(slice<Statfs_t> buf, long flags)
        {
            unsafe.Pointer _p0 = default;
            System.UIntPtr bufsize = default;
            if (len(buf) > 0L)
            {
                _p0 = @unsafe.Pointer(ref buf[0L]);
                bufsize = @unsafe.Sizeof(new Statfs_t()) * uintptr(len(buf));
            }
            var (r0, _, e1) = Syscall(SYS_GETFSSTAT, uintptr(_p0), bufsize, uintptr(flags));
            n = int(r0);
            if (e1 != 0L)
            {
                err = e1;
            }
            return;
        }

        private static error setattrlistTimes(@string path, slice<Timespec> times)
        { 
            // used on Darwin for UtimesNano
            return error.As(ENOSYS);
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
        //sys    Fchdir(fd int) (err error)
        //sys    Fchflags(fd int, flags int) (err error)
        //sys    Fchmod(fd int, mode uint32) (err error)
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Flock(fd int, how int) (err error)
        //sys    Fpathconf(fd int, name int) (val int, err error)
        //sys    Fstat(fd int, stat *Stat_t) (err error)
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
        //sysnb    Getrusage(who int, rusage *Rusage) (err error)
        //sysnb    Getsid(pid int) (sid int, err error)
        //sysnb    Gettimeofday(tv *Timeval) (err error)
        //sysnb    Getuid() (uid int)
        //sys    Issetugid() (tainted bool)
        //sys    Kill(pid int, signum Signal) (err error)
        //sys    Kqueue() (fd int, err error)
        //sys    Lchown(path string, uid int, gid int) (err error)
        //sys    Link(path string, link string) (err error)
        //sys    Listen(s int, backlog int) (err error)
        //sys    Lstat(path string, stat *Stat_t) (err error)
        //sys    Mkdir(path string, mode uint32) (err error)
        //sys    Mkfifo(path string, mode uint32) (err error)
        //sys    Mknod(path string, mode uint32, dev int) (err error)
        //sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
        //sys    Open(path string, mode int, perm uint32) (fd int, err error)
        //sys    Pathconf(path string, name int) (val int, err error)
        //sys    Pread(fd int, p []byte, offset int64) (n int, err error)
        //sys    Pwrite(fd int, p []byte, offset int64) (n int, err error)
        //sys    read(fd int, p []byte) (n int, err error)
        //sys    Readlink(path string, buf []byte) (n int, err error)
        //sys    Rename(from string, to string) (err error)
        //sys    Revoke(path string) (err error)
        //sys    Rmdir(path string) (err error)
        //sys    Seek(fd int, offset int64, whence int) (newoffset int64, err error) = SYS_LSEEK
        //sys    Select(n int, r *FdSet, w *FdSet, e *FdSet, timeout *Timeval) (err error)
        //sysnb    Setegid(egid int) (err error)
        //sysnb    Seteuid(euid int) (err error)
        //sysnb    Setgid(gid int) (err error)
        //sys    Setlogin(name string) (err error)
        //sysnb    Setpgid(pid int, pgid int) (err error)
        //sys    Setpriority(which int, who int, prio int) (err error)
        //sysnb    Setregid(rgid int, egid int) (err error)
        //sysnb    Setreuid(ruid int, euid int) (err error)
        //sysnb    Setrlimit(which int, lim *Rlimit) (err error)
        //sysnb    Setsid() (pid int, err error)
        //sysnb    Settimeofday(tp *Timeval) (err error)
        //sysnb    Setuid(uid int) (err error)
        //sys    Stat(path string, stat *Stat_t) (err error)
        //sys    Statfs(path string, stat *Statfs_t) (err error)
        //sys    Symlink(path string, link string) (err error)
        //sys    Sync() (err error)
        //sys    Truncate(path string, length int64) (err error)
        //sys    Umask(newmask int) (oldmask int)
        //sys    Unlink(path string) (err error)
        //sys    Unmount(path string, flags int) (err error)
        //sys    write(fd int, p []byte) (n int, err error)
        //sys    mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error)
        //sys    munmap(addr uintptr, length uintptr) (err error)
        //sys    readlen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_READ
        //sys    writelen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_WRITE
        //sys    utimensat(dirfd int, path string, times *[2]Timespec, flag int) (err error)

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
        // faccessat
        // fchmodat
        // fchownat
        // fcntl
        // fhopen
        // fhstat
        // fhstatfs
        // fork
        // fstatat
        // futimens
        // getfh
        // getgid
        // getitimer
        // getlogin
        // getresgid
        // getresuid
        // getrtable
        // getthrid
        // ioctl
        // ktrace
        // lfs_bmapv
        // lfs_markv
        // lfs_segclean
        // lfs_segwait
        // linkat
        // mincore
        // minherit
        // mkdirat
        // mkfifoat
        // mknodat
        // mlock
        // mlockall
        // mount
        // mquery
        // msgctl
        // msgget
        // msgrcv
        // msgsnd
        // munlock
        // munlockall
        // nfssvc
        // nnpfspioctl
        // openat
        // poll
        // preadv
        // profil
        // pwritev
        // quotactl
        // readlinkat
        // readv
        // reboot
        // renameat
        // rfork
        // sched_yield
        // semget
        // semop
        // setgroups
        // setitimer
        // setresgid
        // setresuid
        // setrtable
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
        // symlinkat
        // sysarch
        // syscall
        // threxit
        // thrsigdivert
        // thrsleep
        // thrwakeup
        // unlinkat
        // vfork
        // writev
    }
}
