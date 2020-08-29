// Copyright 2009,2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Darwin system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and wrap
// it in our own nicer implementation, either here or in
// syscall_bsd.go or syscall_unix.go.

// package syscall -- go2cs converted at 2020 August 29 08:37:46 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\syscall_darwin.go
using errorspkg = go.errors_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static unsafe partial class syscall_package
    {
        public static readonly var ImplementsGetwd = true;



        public static (@string, error) Getwd()
        {
            var buf = make_slice<byte>(2048L);
            var (attrs, err) = getAttrList(".", new attrList(CommonAttr:attrCmnFullpath), buf, 0L);
            if (err == null && len(attrs) == 1L && len(attrs[0L]) >= 2L)
            {
                var wd = string(attrs[0L]); 
                // Sanity check that it's an absolute path and ends
                // in a null byte, which we then strip.
                if (wd[0L] == '/' && wd[len(wd) - 1L] == 0L)
                {
                    return (wd[..len(wd) - 1L], null);
                }
            } 
            // If pkg/os/getwd.go gets ENOTSUP, it will fall back to the
            // slow algorithm.
            return ("", ENOTSUP);
        }

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
            return readInt(buf, @unsafe.Offsetof(new Dirent().Ino), @unsafe.Sizeof(new Dirent().Ino));
        }

        private static (ulong, bool) direntReclen(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Reclen), @unsafe.Sizeof(new Dirent().Reclen));
        }

        private static (ulong, bool) direntNamlen(slice<byte> buf)
        {
            return readInt(buf, @unsafe.Offsetof(new Dirent().Namlen), @unsafe.Sizeof(new Dirent().Namlen));
        }

        //sys   ptrace(request int, pid int, addr uintptr, data uintptr) (err error)
        public static error PtraceAttach(long pid)
        {
            return error.As(ptrace(PT_ATTACH, pid, 0L, 0L));
        }
        public static error PtraceDetach(long pid)
        {
            return error.As(ptrace(PT_DETACH, pid, 0L, 0L));
        }

        private static readonly long attrBitMapCount = 5L;
        private static readonly ulong attrCmnModtime = 0x00000400UL;
        private static readonly ulong attrCmnAcctime = 0x00001000UL;
        private static readonly ulong attrCmnFullpath = 0x08000000UL;

        private partial struct attrList
        {
            public ushort bitmapCount;
            public ushort _;
            public uint CommonAttr;
            public uint VolAttr;
            public uint DirAttr;
            public uint FileAttr;
            public uint Forkattr;
        }

        private static (slice<slice<byte>>, error) getAttrList(@string path, attrList attrList, slice<byte> attrBuf, ulong options)
        {
            if (len(attrBuf) < 4L)
            {
                return (null, errorspkg.New("attrBuf too small"));
            }
            attrList.bitmapCount = attrBitMapCount;

            ref byte _p0 = default;
            _p0, err = BytePtrFromString(path);
            if (err != null)
            {
                return (null, err);
            }
            var (_, _, e1) = Syscall6(SYS_GETATTRLIST, uintptr(@unsafe.Pointer(_p0)), uintptr(@unsafe.Pointer(ref attrList)), uintptr(@unsafe.Pointer(ref attrBuf[0L])), uintptr(len(attrBuf)), uintptr(options), 0L);
            if (e1 != 0L)
            {
                return (null, e1);
            }
            *(*uint) size = @unsafe.Pointer(ref attrBuf[0L]).Value; 

            // dat is the section of attrBuf that contains valid data,
            // without the 4 byte length header. All attribute offsets
            // are relative to dat.
            var dat = attrBuf;
            if (int(size) < len(attrBuf))
            {
                dat = dat[..size];
            }
            dat = dat[4L..]; // remove length prefix

            {
                var i = uint32(0L);

                while (int(i) < len(dat))
                {
                    var header = dat[i..];
                    if (len(header) < 8L)
                    {
                        return (attrs, errorspkg.New("truncated attribute header"));
                    }
                    *(*int) datOff = @unsafe.Pointer(ref header[0L]).Value;
                    *(*uint) attrLen = @unsafe.Pointer(ref header[4L]).Value;
                    if (datOff < 0L || uint32(datOff) + attrLen > uint32(len(dat)))
                    {
                        return (attrs, errorspkg.New("truncated results; attrBuf too small"));
                    }
                    var end = uint32(datOff) + attrLen;
                    attrs = append(attrs, dat[datOff..end]);
                    i = end;
                    {
                        var r = i % 4L;

                        if (r != 0L)
                        {
                            i += (4L - r);
                        }

                    }
                }

            }
            return;
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

        public static (long, error) Getfsstat(slice<Statfs_t> buf, long flags)
        {
            unsafe.Pointer _p0 = default;
            System.UIntPtr bufsize = default;
            if (len(buf) > 0L)
            {
                _p0 = @unsafe.Pointer(ref buf[0L]);
                bufsize = @unsafe.Sizeof(new Statfs_t()) * uintptr(len(buf));
            }
            var (r0, _, e1) = Syscall(SYS_GETFSSTAT64, uintptr(_p0), bufsize, uintptr(flags));
            n = int(r0);
            if (e1 != 0L)
            {
                err = e1;
            }
            return;
        }

        private static error setattrlistTimes(@string path, slice<Timespec> times)
        {
            var (_p0, err) = BytePtrFromString(path);
            if (err != null)
            {
                return error.As(err);
            }
            attrList attrList = default;
            attrList.bitmapCount = attrBitMapCount;
            attrList.CommonAttr = attrCmnModtime | attrCmnAcctime; 

            // order is mtime, atime: the opposite of Chtimes
            array<Timespec> attributes = new array<Timespec>(new Timespec[] { times[1], times[0] });
            const long options = 0L;

            var (_, _, e1) = Syscall6(SYS_SETATTRLIST, uintptr(@unsafe.Pointer(_p0)), uintptr(@unsafe.Pointer(ref attrList)), uintptr(@unsafe.Pointer(ref attributes)), uintptr(@unsafe.Sizeof(attributes)), uintptr(options), 0L);
            if (e1 != 0L)
            {
                return error.As(e1);
            }
            return error.As(null);
        }

        private static error utimensat(long dirfd, @string path, ref array<Timespec> times, long flag)
        { 
            // Darwin doesn't support SYS_UTIMENSAT
            return error.As(ENOSYS);
        }

        /*
         * Wrapped
         */

        //sys    kill(pid int, signum int, posix int) (err error)

        public static error Kill(long pid, Signal signum)
        {
            return error.As(kill(pid, int(signum), 1L));
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
        //sys    Exchangedata(path1 string, path2 string, options int) (err error)
        //sys    Fchdir(fd int) (err error)
        //sys    Fchflags(fd int, flags int) (err error)
        //sys    Fchmod(fd int, mode uint32) (err error)
        //sys    Fchown(fd int, uid int, gid int) (err error)
        //sys    Flock(fd int, how int) (err error)
        //sys    Fpathconf(fd int, name int) (val int, err error)
        //sys    Fstat(fd int, stat *Stat_t) (err error) = SYS_FSTAT64
        //sys    Fstatfs(fd int, stat *Statfs_t) (err error) = SYS_FSTATFS64
        //sys    Fsync(fd int) (err error)
        //sys    Ftruncate(fd int, length int64) (err error)
        //sys    Getdirentries(fd int, buf []byte, basep *uintptr) (n int, err error) = SYS_GETDIRENTRIES64
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
        //sysnb    Getuid() (uid int)
        //sysnb    Issetugid() (tainted bool)
        //sys    Kqueue() (fd int, err error)
        //sys    Lchown(path string, uid int, gid int) (err error)
        //sys    Link(path string, link string) (err error)
        //sys    Listen(s int, backlog int) (err error)
        //sys    Lstat(path string, stat *Stat_t) (err error) = SYS_LSTAT64
        //sys    Mkdir(path string, mode uint32) (err error)
        //sys    Mkfifo(path string, mode uint32) (err error)
        //sys    Mknod(path string, mode uint32, dev int) (err error)
        //sys    Mlock(b []byte) (err error)
        //sys    Mlockall(flags int) (err error)
        //sys    Mprotect(b []byte, prot int) (err error)
        //sys    Munlock(b []byte) (err error)
        //sys    Munlockall() (err error)
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
        //sys    Setegid(egid int) (err error)
        //sysnb    Seteuid(euid int) (err error)
        //sysnb    Setgid(gid int) (err error)
        //sys    Setlogin(name string) (err error)
        //sysnb    Setpgid(pid int, pgid int) (err error)
        //sys    Setpriority(which int, who int, prio int) (err error)
        //sys    Setprivexec(flag int) (err error)
        //sysnb    Setregid(rgid int, egid int) (err error)
        //sysnb    Setreuid(ruid int, euid int) (err error)
        //sysnb    Setrlimit(which int, lim *Rlimit) (err error)
        //sysnb    Setsid() (pid int, err error)
        //sysnb    Settimeofday(tp *Timeval) (err error)
        //sysnb    Setuid(uid int) (err error)
        //sys    Stat(path string, stat *Stat_t) (err error) = SYS_STAT64
        //sys    Statfs(path string, stat *Statfs_t) (err error) = SYS_STATFS64
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

        /*
         * Unimplemented
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
        // sendfile
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
