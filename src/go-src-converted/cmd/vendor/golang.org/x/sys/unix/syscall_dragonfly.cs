// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// DragonFly BSD system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and wrap
// it in our own nicer implementation, either here or in
// syscall_bsd.go or syscall_unix.go.

// package unix -- go2cs converted at 2020 October 08 04:47:01 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_dragonfly.go
using sync = go.sync_package;
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
        // See version list in https://github.com/DragonFlyBSD/DragonFlyBSD/blob/master/sys/sys/param.h
        private static sync.Once osreldateOnce = default;        private static uint osreldate = default;

        // First __DragonFly_version after September 2019 ABI changes
        // http://lists.dragonflybsd.org/pipermail/users/2019-September/358280.html
        private static readonly long _dragonflyABIChangeVersion = (long)500705L;



        private static bool supportsABI(uint ver)
        {
            osreldateOnce.Do(() =>
            {
                osreldate, _ = SysctlUint32("kern.osreldate");
            });
            return osreldate >= ver;

        }

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
            public ushort Rcf;
            public array<ushort> Route;
            public RawSockaddrDatalink raw;
        }

        // Translate "kern.hostname" to []_C_int{0,1,2,3}.
        private static (slice<_C_int>, error) nametomib(@string name)
        {
            slice<_C_int> mib = default;
            error err = default!;

            const var siz = (var)@unsafe.Sizeof(mib[0L]); 

            // NOTE(rsc): It seems strange to set the buffer to have
            // size CTL_MAXNAME+2 but use only CTL_MAXNAME
            // as the size. I don't know why the +2 is here, but the
            // kernel uses +2 for its own implementation of this function.
            // I am scared that if we don't include the +2 here, the kernel
            // will silently write 2 words farther than we specify
            // and we'll get memory corruption.
 

            // NOTE(rsc): It seems strange to set the buffer to have
            // size CTL_MAXNAME+2 but use only CTL_MAXNAME
            // as the size. I don't know why the +2 is here, but the
            // kernel uses +2 for its own implementation of this function.
            // I am scared that if we don't include the +2 here, the kernel
            // will silently write 2 words farther than we specify
            // and we'll get memory corruption.
            array<_C_int> buf = new array<_C_int>(CTL_MAXNAME + 2L);
            ref var n = ref heap(uintptr(CTL_MAXNAME) * siz, out ptr<var> _addr_n);

            var p = (byte.val)(@unsafe.Pointer(_addr_buf[0L]));
            var (bytes, err) = ByteSliceFromString(name);
            if (err != null)
            {
                return (null, error.As(err)!);
            } 

            // Magic sysctl: "setting" 0.3 to a string name
            // lets you read back the array of integers form.
            err = sysctl(new slice<_C_int>(new _C_int[] { 0, 3 }), p, _addr_n, _addr_bytes[0L], uintptr(len(name)));

            if (err != null)
            {
                return (null, error.As(err)!);
            }

            return (buf[0L..n / siz], error.As(null!)!);

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

            var (namlen, ok) = direntNamlen(buf);
            if (!ok)
            {
                return (0L, false);
            }

            return ((16L + namlen + 1L + 7L) & ~7L, true);

        }

        private static (ulong, bool) direntNamlen(slice<byte> buf)
        {
            ulong _p0 = default;
            bool _p0 = default;

            return readInt(buf, @unsafe.Offsetof(new Dirent().Namlen), @unsafe.Sizeof(new Dirent().Namlen));
        }

        //sysnb pipe() (r int, w int, err error)

        public static error Pipe(slice<long> p)
        {
            error err = default!;

            if (len(p) != 2L)
            {
                return error.As(EINVAL)!;
            }

            p[0L], p[1L], err = pipe();
            return ;

        }

        //sys    extpread(fd int, p []byte, flags int, offset int64) (n int, err error)
        public static (long, error) Pread(long fd, slice<byte> p, long offset)
        {
            long n = default;
            error err = default!;

            return extpread(fd, p, 0L, offset);
        }

        //sys    extpwrite(fd int, p []byte, flags int, offset int64) (n int, err error)
        public static (long, error) Pwrite(long fd, slice<byte> p, long offset)
        {
            long n = default;
            error err = default!;

            return extpwrite(fd, p, 0L, offset);
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

        public static readonly var ImplementsGetwd = (var)true;

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

        private static error sysctlUname(slice<_C_int> mib, ptr<byte> _addr_old, ptr<System.UIntPtr> _addr_oldlen)
        {
            ref byte old = ref _addr_old.val;
            ref System.UIntPtr oldlen = ref _addr_oldlen.val;

            var err = sysctl(mib, old, oldlen, null, 0L);
            if (err != null)
            { 
                // Utsname members on Dragonfly are only 32 bytes and
                // the syscall returns ENOMEM in case the actual value
                // is longer.
                if (err == ENOMEM)
                {
                    err = null;
                }

            }

            return error.As(err)!;

        }

        public static error Uname(ptr<Utsname> _addr_uname)
        {
            ref Utsname uname = ref _addr_uname.val;

            _C_int mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_OSTYPE });
            ref var n = ref heap(@unsafe.Sizeof(uname.Sysname), out ptr<var> _addr_n);
            {
                var err__prev1 = err;

                var err = sysctlUname(mib, _addr_uname.Sysname[0L], _addr_n);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            uname.Sysname[@unsafe.Sizeof(uname.Sysname) - 1L] = 0L;

            mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_HOSTNAME });
            n = @unsafe.Sizeof(uname.Nodename);
            {
                var err__prev1 = err;

                err = sysctlUname(mib, _addr_uname.Nodename[0L], _addr_n);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            uname.Nodename[@unsafe.Sizeof(uname.Nodename) - 1L] = 0L;

            mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_OSRELEASE });
            n = @unsafe.Sizeof(uname.Release);
            {
                var err__prev1 = err;

                err = sysctlUname(mib, _addr_uname.Release[0L], _addr_n);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            uname.Release[@unsafe.Sizeof(uname.Release) - 1L] = 0L;

            mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_VERSION });
            n = @unsafe.Sizeof(uname.Version);
            {
                var err__prev1 = err;

                err = sysctlUname(mib, _addr_uname.Version[0L], _addr_n);

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

                err = sysctlUname(mib, _addr_uname.Machine[0L], _addr_n);

                if (err != null)
                {
                    return error.As(err)!;
                }

                err = err__prev1;

            }

            uname.Machine[@unsafe.Sizeof(uname.Machine) - 1L] = 0L;

            return error.As(null!)!;

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
        //sys    Getdents(fd int, buf []byte) (n int, err error)
        //sys    Getdirentries(fd int, buf []byte, basep *uintptr) (n int, err error)
        //sys    Getdtablesize() (size int)
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
        //sys    Mknod(path string, mode uint32, dev int) (err error)
        //sys    Mknodat(fd int, path string, mode uint32, dev int) (err error)
        //sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
        //sys    Open(path string, mode int, perm uint32) (fd int, err error)
        //sys    Openat(dirfd int, path string, mode int, perm uint32) (fd int, err error)
        //sys    Pathconf(path string, name int) (val int, err error)
        //sys    read(fd int, p []byte) (n int, err error)
        //sys    Readlink(path string, buf []byte) (n int, err error)
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
        //sys    Undelete(path string) (err error)
        //sys    Unlink(path string) (err error)
        //sys    Unlinkat(dirfd int, path string, flags int) (err error)
        //sys    Unmount(path string, flags int) (err error)
        //sys    write(fd int, p []byte) (n int, err error)
        //sys   mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error)
        //sys   munmap(addr uintptr, length uintptr) (err error)
        //sys    readlen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_READ
        //sys    writelen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_WRITE
        //sys    accept4(fd int, rsa *RawSockaddrAny, addrlen *_Socklen, flags int) (nfd int, err error)
        //sys    utimensat(dirfd int, path string, times *[2]Timespec, flags int) (err error)

        /*
         * Unimplemented
         * TODO(jsing): Update this list for DragonFly.
         */
        // Profil
        // Sigaction
        // Sigprocmask
        // Getlogin
        // Sigpending
        // Sigaltstack
        // Reboot
        // Execve
        // Vfork
        // Sbrk
        // Sstk
        // Ovadvise
        // Mincore
        // Setitimer
        // Swapon
        // Select
        // Sigsuspend
        // Readv
        // Writev
        // Nfssvc
        // Getfh
        // Quotactl
        // Mount
        // Csops
        // Waitid
        // Add_profil
        // Kdebug_trace
        // Sigreturn
        // Atsocket
        // Kqueue_from_portset_np
        // Kqueue_portset
        // Getattrlist
        // Setattrlist
        // Getdirentriesattr
        // Searchfs
        // Delete
        // Copyfile
        // Watchevent
        // Waitevent
        // Modwatch
        // Getxattr
        // Fgetxattr
        // Setxattr
        // Fsetxattr
        // Removexattr
        // Fremovexattr
        // Listxattr
        // Flistxattr
        // Fsctl
        // Initgroups
        // Posix_spawn
        // Nfsclnt
        // Fhopen
        // Minherit
        // Semsys
        // Msgsys
        // Shmsys
        // Semctl
        // Semget
        // Semop
        // Msgctl
        // Msgget
        // Msgsnd
        // Msgrcv
        // Shmat
        // Shmctl
        // Shmdt
        // Shmget
        // Shm_open
        // Shm_unlink
        // Sem_open
        // Sem_close
        // Sem_unlink
        // Sem_wait
        // Sem_trywait
        // Sem_post
        // Sem_getvalue
        // Sem_init
        // Sem_destroy
        // Open_extended
        // Umask_extended
        // Stat_extended
        // Lstat_extended
        // Fstat_extended
        // Chmod_extended
        // Fchmod_extended
        // Access_extended
        // Settid
        // Gettid
        // Setsgroups
        // Getsgroups
        // Setwgroups
        // Getwgroups
        // Mkfifo_extended
        // Mkdir_extended
        // Identitysvc
        // Shared_region_check_np
        // Shared_region_map_np
        // __pthread_mutex_destroy
        // __pthread_mutex_init
        // __pthread_mutex_lock
        // __pthread_mutex_trylock
        // __pthread_mutex_unlock
        // __pthread_cond_init
        // __pthread_cond_destroy
        // __pthread_cond_broadcast
        // __pthread_cond_signal
        // Setsid_with_pid
        // __pthread_cond_timedwait
        // Aio_fsync
        // Aio_return
        // Aio_suspend
        // Aio_cancel
        // Aio_error
        // Aio_read
        // Aio_write
        // Lio_listio
        // __pthread_cond_wait
        // Iopolicysys
        // __pthread_kill
        // __pthread_sigmask
        // __sigwait
        // __disable_threadsignal
        // __pthread_markcancel
        // __pthread_canceled
        // __semwait_signal
        // Proc_info
        // Stat64_extended
        // Lstat64_extended
        // Fstat64_extended
        // __pthread_chdir
        // __pthread_fchdir
        // Audit
        // Auditon
        // Getauid
        // Setauid
        // Getaudit
        // Setaudit
        // Getaudit_addr
        // Setaudit_addr
        // Auditctl
        // Bsdthread_create
        // Bsdthread_terminate
        // Stack_snapshot
        // Bsdthread_register
        // Workq_open
        // Workq_ops
        // __mac_execve
        // __mac_syscall
        // __mac_get_file
        // __mac_set_file
        // __mac_get_link
        // __mac_set_link
        // __mac_get_proc
        // __mac_set_proc
        // __mac_get_fd
        // __mac_set_fd
        // __mac_get_pid
        // __mac_get_lcid
        // __mac_get_lctx
        // __mac_set_lctx
        // Setlcid
        // Read_nocancel
        // Write_nocancel
        // Open_nocancel
        // Close_nocancel
        // Wait4_nocancel
        // Recvmsg_nocancel
        // Sendmsg_nocancel
        // Recvfrom_nocancel
        // Accept_nocancel
        // Fcntl_nocancel
        // Select_nocancel
        // Fsync_nocancel
        // Connect_nocancel
        // Sigsuspend_nocancel
        // Readv_nocancel
        // Writev_nocancel
        // Sendto_nocancel
        // Pread_nocancel
        // Pwrite_nocancel
        // Waitid_nocancel
        // Msgsnd_nocancel
        // Msgrcv_nocancel
        // Sem_wait_nocancel
        // Aio_suspend_nocancel
        // __sigwait_nocancel
        // __semwait_signal_nocancel
        // __mac_mount
        // __mac_get_mount
        // __mac_getfsstat
    }
}}}}}}
