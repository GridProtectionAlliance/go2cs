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

// package syscall -- go2cs converted at 2020 August 29 08:37:52 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_dragonfly.go
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
            public ushort Rcf;
            public array<ushort> Route;
            public RawSockaddrDatalink raw;
        }

        // Translate "kern.hostname" to []_C_int{0,1,2,3}.
        private static (slice<_C_int>, error) nametomib(@string name)
        {
            const var siz = @unsafe.Sizeof(mib[0L]); 

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
            var n = uintptr(CTL_MAXNAME) * siz;

            var p = (byte.Value)(@unsafe.Pointer(ref buf[0L]));
            var (bytes, err) = ByteSliceFromString(name);
            if (err != null)
            {
                return (null, err);
            } 

            // Magic sysctl: "setting" 0.3 to a string name
            // lets you read back the array of integers form.
            err = sysctl(new slice<_C_int>(new _C_int[] { 0, 3 }), p, ref n, ref bytes[0L], uintptr(len(name)));

            if (err != null)
            {
                return (null, err);
            }
            return (buf[0L..n / siz], null);
        }

        private static (ulong, bool) direntIno(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Fileno), @unsafe.Sizeof(new Dirent().Fileno));
        }

        private static (ulong, bool) direntReclen(slice<byte> buf)
        {
            var (namlen, ok) = direntNamlen(buf);
            if (!ok)
            {
                return (0L, false);
            }
            return ((16L + namlen + 1L + 7L) & ~7L, true);
        }

        private static (ulong, bool) direntNamlen(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Namlen), @unsafe.Sizeof(new Dirent().Namlen));
        }

        //sysnb pipe() (r int, w int, err error)

        public static error Pipe(slice<long> p)
        {
            if (len(p) != 2L)
            {
                return error.As(EINVAL);
            }
            p[0L], p[1L], err = pipe();
            return;
        }

        //sys    extpread(fd int, p []byte, flags int, offset int64) (n int, err error)
        public static (long, error) Pread(long fd, slice<byte> p, long offset)
        {
            return extpread(fd, p, 0L, offset);
        }

        //sys    extpwrite(fd int, p []byte, flags int, offset int64) (n int, err error)
        public static (long, error) Pwrite(long fd, slice<byte> p, long offset)
        {
            return extpwrite(fd, p, 0L, offset);
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
        //sys    Undelete(path string) (err error)
        //sys    Unlink(path string) (err error)
        //sys    Unmount(path string, flags int) (err error)
        //sys    write(fd int, p []byte) (n int, err error)
        //sys   mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error)
        //sys   munmap(addr uintptr, length uintptr) (err error)
        //sys    readlen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_READ
        //sys    writelen(fd int, buf *byte, nbuf int) (n int, err error) = SYS_WRITE
        //sys    accept4(fd int, rsa *RawSockaddrAny, addrlen *_Socklen, flags int) (nfd int, err error)
        //sys    utimensat(dirfd int, path string, times *[2]Timespec, flag int) (err error)

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
        // Ioctl
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
        // Mmap
        // Mlock
        // Munlock
        // Atsocket
        // Kqueue_from_portset_np
        // Kqueue_portset
        // Getattrlist
        // Setattrlist
        // Getdirentriesattr
        // Searchfs
        // Delete
        // Copyfile
        // Poll
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
        // Mlockall
        // Munlockall
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
        // Msync_nocancel
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
        // Poll_nocancel
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
}
