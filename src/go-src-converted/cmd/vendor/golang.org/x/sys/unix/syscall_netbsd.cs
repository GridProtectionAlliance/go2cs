// Copyright 2009,2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// NetBSD system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and wrap
// it in our own nicer implementation, either here or in
// syscall_bsd.go or syscall_unix.go.

// package unix -- go2cs converted at 2022 March 06 23:27:13 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_netbsd.go
using runtime = go.runtime_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // SockaddrDatalink implements the Sockaddr interface for AF_LINK type sockets.
public partial struct SockaddrDatalink {
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

private static (Sockaddr, error) anyToSockaddrGOOS(nint fd, ptr<RawSockaddrAny> _addr_rsa) {
    Sockaddr _p0 = default;
    error _p0 = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;

    return (null, error.As(EAFNOSUPPORT)!);
}

public static (System.UIntPtr, System.UIntPtr, syscall.Errno) Syscall9(System.UIntPtr trap, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9);

private static (slice<Sysctlnode>, error) sysctlNodes(slice<_C_int> mib) {
    slice<Sysctlnode> nodes = default;
    error err = default!;

    ref System.UIntPtr olen = ref heap(out ptr<System.UIntPtr> _addr_olen); 

    // Get a list of all sysctl nodes below the given MIB by performing
    // a sysctl for the given MIB with CTL_QUERY appended.
    mib = append(mib, CTL_QUERY);
    ref Sysctlnode qnode = ref heap(new Sysctlnode(Flags:SYSCTL_VERS_1), out ptr<Sysctlnode> _addr_qnode);
    var qp = (byte.val)(@unsafe.Pointer(_addr_qnode));
    var sz = @unsafe.Sizeof(qnode);
    err = sysctl(mib, null, _addr_olen, qp, sz);

    if (err != null) {>>MARKER:FUNCTION_Syscall9_BLOCK_PREFIX<<
        return (null, error.As(err)!);
    }
    nodes = make_slice<Sysctlnode>(olen / sz);
    var np = (byte.val)(@unsafe.Pointer(_addr_nodes[0]));
    err = sysctl(mib, np, _addr_olen, qp, sz);

    if (err != null) {
        return (null, error.As(err)!);
    }
    return (nodes, error.As(null!)!);

}

private static (slice<_C_int>, error) nametomib(@string name) {
    slice<_C_int> mib = default;
    error err = default!;
 
    // Split name into components.
    slice<@string> parts = default;
    nint last = 0;
    {
        nint i__prev1 = i;

        for (nint i = 0; i < len(name); i++) {
            if (name[i] == '.') {
                parts = append(parts, name[(int)last..(int)i]);
                last = i + 1;
            }
        }

        i = i__prev1;
    }
    parts = append(parts, name[(int)last..]); 

    // Discover the nodes and construct the MIB OID.
    foreach (var (partno, part) in parts) {
        var (nodes, err) = sysctlNodes(mib);
        if (err != null) {
            return (null, error.As(err)!);
        }
        foreach (var (_, node) in nodes) {
            var n = make_slice<byte>(0);
            {
                nint i__prev3 = i;

                foreach (var (__i) in node.Name) {
                    i = __i;
                    if (node.Name[i] != 0) {
                        n = append(n, byte(node.Name[i]));
                    }
                }

                i = i__prev3;
            }

            if (string(n) == part) {
                mib = append(mib, _C_int(node.Num));
                break;
            }

        }        if (len(mib) != partno + 1) {
            return (null, error.As(EINVAL)!);
        }
    }    return (mib, error.As(null!)!);

}

private static (ulong, bool) direntIno(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new Dirent().Fileno), @unsafe.Sizeof(new Dirent().Fileno));
}

private static (ulong, bool) direntReclen(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new Dirent().Reclen), @unsafe.Sizeof(new Dirent().Reclen));
}

private static (ulong, bool) direntNamlen(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new Dirent().Namlen), @unsafe.Sizeof(new Dirent().Namlen));
}

//sysnb    pipe() (fd1 int, fd2 int, err error)

public static error Pipe(slice<nint> p) {
    error err = default!;

    if (len(p) != 2) {
        return error.As(EINVAL)!;
    }
    p[0], p[1], err = pipe();
    return ;

}

//sysnb    pipe2(p *[2]_C_int, flags int) (err error)

public static error Pipe2(slice<nint> p, nint flags) {
    if (len(p) != 2) {
        return error.As(EINVAL)!;
    }
    ref array<_C_int> pp = ref heap(new array<_C_int>(2), out ptr<array<_C_int>> _addr_pp);
    var err = pipe2(_addr_pp, flags);
    p[0] = int(pp[0]);
    p[1] = int(pp[1]);
    return error.As(err)!;

}

//sys    Getdents(fd int, buf []byte) (n int, err error)

public static (nint, error) Getdirentries(nint fd, slice<byte> buf, ptr<System.UIntPtr> _addr_basep) {
    nint n = default;
    error err = default!;
    ref System.UIntPtr basep = ref _addr_basep.val;

    n, err = Getdents(fd, buf);
    if (err != null || basep == null) {
        return ;
    }
    long off = default;
    off, err = Seek(fd, 0, 1);
    if (err != null) {
        basep = ~uintptr(0);
        return ;
    }
    basep = uintptr(off);
    if (@unsafe.Sizeof(basep) == 8) {
        return ;
    }
    if (off >> 32 != 0) { 
        // We can't stuff the offset back into a uintptr, so any
        // future calls would be suspect. Generate an error.
        // EIO is allowed by getdirentries.
        err = EIO;

    }
    return ;

}

//sys    Getcwd(buf []byte) (n int, err error) = SYS___GETCWD

// TODO
private static (nint, error) sendfile(nint outfd, nint infd, ptr<long> _addr_offset, nint count) {
    nint written = default;
    error err = default!;
    ref long offset = ref _addr_offset.val;

    return (-1, error.As(ENOSYS)!);
}

private static error setattrlistTimes(@string path, slice<Timespec> times, nint flags) { 
    // used on Darwin for UtimesNano
    return error.As(ENOSYS)!;

}

//sys    ioctl(fd int, req uint, arg uintptr) (err error)

//sys    sysctl(mib []_C_int, old *byte, oldlen *uintptr, new *byte, newlen uintptr) (err error) = SYS___SYSCTL

public static (ptr<Ptmget>, error) IoctlGetPtmget(nint fd, nuint req) {
    ptr<Ptmget> _p0 = default!;
    error _p0 = default!;

    ref Ptmget value = ref heap(out ptr<Ptmget> _addr_value);
    var err = ioctl(fd, req, uintptr(@unsafe.Pointer(_addr_value)));
    runtime.KeepAlive(value);
    return (_addr__addr_value!, error.As(err)!);
}

public static error Uname(ptr<Utsname> _addr_uname) {
    ref Utsname uname = ref _addr_uname.val;

    _C_int mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_OSTYPE });
    ref var n = ref heap(@unsafe.Sizeof(uname.Sysname), out ptr<var> _addr_n);
    {
        var err__prev1 = err;

        var err = sysctl(mib, _addr_uname.Sysname[0], _addr_n, null, 0);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }


    mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_HOSTNAME });
    n = @unsafe.Sizeof(uname.Nodename);
    {
        var err__prev1 = err;

        err = sysctl(mib, _addr_uname.Nodename[0], _addr_n, null, 0);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }


    mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_OSRELEASE });
    n = @unsafe.Sizeof(uname.Release);
    {
        var err__prev1 = err;

        err = sysctl(mib, _addr_uname.Release[0], _addr_n, null, 0);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }


    mib = new slice<_C_int>(new _C_int[] { CTL_KERN, KERN_VERSION });
    n = @unsafe.Sizeof(uname.Version);
    {
        var err__prev1 = err;

        err = sysctl(mib, _addr_uname.Version[0], _addr_n, null, 0);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    } 

    // The version might have newlines or tabs in it, convert them to
    // spaces.
    foreach (var (i, b) in uname.Version) {
        if (b == '\n' || b == '\t') {
            if (i == len(uname.Version) - 1) {
                uname.Version[i] = 0;
            }
            else
 {
                uname.Version[i] = ' ';
            }

        }
    }    mib = new slice<_C_int>(new _C_int[] { CTL_HW, HW_MACHINE });
    n = @unsafe.Sizeof(uname.Machine);
    {
        var err__prev1 = err;

        err = sysctl(mib, _addr_uname.Machine[0], _addr_n, null, 0);

        if (err != null) {
            return error.As(err)!;
        }
        err = err__prev1;

    }


    return error.As(null!)!;

}

public static (nint, error) Sendfile(nint outfd, nint infd, ptr<long> _addr_offset, nint count) {
    nint written = default;
    error err = default!;
    ref long offset = ref _addr_offset.val;

    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer(_addr_ioSync));
    }
    return sendfile(outfd, infd, _addr_offset, count);

}

public static error Fstatvfs(nint fd, ptr<Statvfs_t> _addr_buf) {
    error err = default!;
    ref Statvfs_t buf = ref _addr_buf.val;

    return error.As(Fstatvfs1(fd, buf, ST_WAIT))!;
}

public static error Statvfs(@string path, ptr<Statvfs_t> _addr_buf) {
    error err = default!;
    ref Statvfs_t buf = ref _addr_buf.val;

    return error.As(Statvfs1(path, buf, ST_WAIT))!;
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
//sys    ExtattrGetFd(fd int, attrnamespace int, attrname string, data uintptr, nbytes int) (ret int, err error)
//sys    ExtattrSetFd(fd int, attrnamespace int, attrname string, data uintptr, nbytes int) (ret int, err error)
//sys    ExtattrDeleteFd(fd int, attrnamespace int, attrname string) (err error)
//sys    ExtattrListFd(fd int, attrnamespace int, data uintptr, nbytes int) (ret int, err error)
//sys    ExtattrGetFile(file string, attrnamespace int, attrname string, data uintptr, nbytes int) (ret int, err error)
//sys    ExtattrSetFile(file string, attrnamespace int, attrname string, data uintptr, nbytes int) (ret int, err error)
//sys    ExtattrDeleteFile(file string, attrnamespace int, attrname string) (err error)
//sys    ExtattrListFile(file string, attrnamespace int, data uintptr, nbytes int) (ret int, err error)
//sys    ExtattrGetLink(link string, attrnamespace int, attrname string, data uintptr, nbytes int) (ret int, err error)
//sys    ExtattrSetLink(link string, attrnamespace int, attrname string, data uintptr, nbytes int) (ret int, err error)
//sys    ExtattrDeleteLink(link string, attrnamespace int, attrname string) (err error)
//sys    ExtattrListLink(link string, attrnamespace int, data uintptr, nbytes int) (ret int, err error)
//sys    Faccessat(dirfd int, path string, mode uint32, flags int) (err error)
//sys    Fadvise(fd int, offset int64, length int64, advice int) (err error) = SYS_POSIX_FADVISE
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
//sys    Fstatvfs1(fd int, buf *Statvfs_t, flags int) (err error) = SYS_FSTATVFS1
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
//sysnb    Setpgid(pid int, pgid int) (err error)
//sys    Setpriority(which int, who int, prio int) (err error)
//sysnb    Setregid(rgid int, egid int) (err error)
//sysnb    Setreuid(ruid int, euid int) (err error)
//sysnb    Setrlimit(which int, lim *Rlimit) (err error)
//sysnb    Setsid() (pid int, err error)
//sysnb    Settimeofday(tp *Timeval) (err error)
//sysnb    Setuid(uid int) (err error)
//sys    Stat(path string, stat *Stat_t) (err error)
//sys    Statvfs1(path string, buf *Statvfs_t, flags int) (err error) = SYS_STATVFS1
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
// ____semctl13
// __clone
// __fhopen40
// __fhstat40
// __fhstatvfs140
// __fstat30
// __getcwd
// __getfh30
// __getlogin
// __lstat30
// __mount50
// __msgctl13
// __msync13
// __ntp_gettime30
// __posix_chown
// __posix_fchown
// __posix_lchown
// __posix_rename
// __setlogin
// __shmctl13
// __sigaction_sigtramp
// __sigaltstack14
// __sigpending14
// __sigprocmask14
// __sigsuspend14
// __sigtimedwait
// __stat30
// __syscall
// __vfork14
// _ksem_close
// _ksem_destroy
// _ksem_getvalue
// _ksem_init
// _ksem_open
// _ksem_post
// _ksem_trywait
// _ksem_unlink
// _ksem_wait
// _lwp_continue
// _lwp_create
// _lwp_ctl
// _lwp_detach
// _lwp_exit
// _lwp_getname
// _lwp_getprivate
// _lwp_kill
// _lwp_park
// _lwp_self
// _lwp_setname
// _lwp_setprivate
// _lwp_suspend
// _lwp_unpark
// _lwp_unpark_all
// _lwp_wait
// _lwp_wakeup
// _pset_bind
// _sched_getaffinity
// _sched_getparam
// _sched_setaffinity
// _sched_setparam
// acct
// aio_cancel
// aio_error
// aio_fsync
// aio_read
// aio_return
// aio_suspend
// aio_write
// break
// clock_getres
// clock_gettime
// clock_settime
// compat_09_ogetdomainname
// compat_09_osetdomainname
// compat_09_ouname
// compat_10_omsgsys
// compat_10_osemsys
// compat_10_oshmsys
// compat_12_fstat12
// compat_12_getdirentries
// compat_12_lstat12
// compat_12_msync
// compat_12_oreboot
// compat_12_oswapon
// compat_12_stat12
// compat_13_sigaction13
// compat_13_sigaltstack13
// compat_13_sigpending13
// compat_13_sigprocmask13
// compat_13_sigreturn13
// compat_13_sigsuspend13
// compat_14___semctl
// compat_14_msgctl
// compat_14_shmctl
// compat_16___sigaction14
// compat_16___sigreturn14
// compat_20_fhstatfs
// compat_20_fstatfs
// compat_20_getfsstat
// compat_20_statfs
// compat_30___fhstat30
// compat_30___fstat13
// compat_30___lstat13
// compat_30___stat13
// compat_30_fhopen
// compat_30_fhstat
// compat_30_fhstatvfs1
// compat_30_getdents
// compat_30_getfh
// compat_30_ntp_gettime
// compat_30_socket
// compat_40_mount
// compat_43_fstat43
// compat_43_lstat43
// compat_43_oaccept
// compat_43_ocreat
// compat_43_oftruncate
// compat_43_ogetdirentries
// compat_43_ogetdtablesize
// compat_43_ogethostid
// compat_43_ogethostname
// compat_43_ogetkerninfo
// compat_43_ogetpagesize
// compat_43_ogetpeername
// compat_43_ogetrlimit
// compat_43_ogetsockname
// compat_43_okillpg
// compat_43_olseek
// compat_43_ommap
// compat_43_oquota
// compat_43_orecv
// compat_43_orecvfrom
// compat_43_orecvmsg
// compat_43_osend
// compat_43_osendmsg
// compat_43_osethostid
// compat_43_osethostname
// compat_43_osetrlimit
// compat_43_osigblock
// compat_43_osigsetmask
// compat_43_osigstack
// compat_43_osigvec
// compat_43_otruncate
// compat_43_owait
// compat_43_stat43
// execve
// extattr_delete_fd
// extattr_delete_file
// extattr_delete_link
// extattr_get_fd
// extattr_get_file
// extattr_get_link
// extattr_list_fd
// extattr_list_file
// extattr_list_link
// extattr_set_fd
// extattr_set_file
// extattr_set_link
// extattrctl
// fchroot
// fdatasync
// fgetxattr
// fktrace
// flistxattr
// fork
// fremovexattr
// fsetxattr
// fstatvfs1
// fsync_range
// getcontext
// getitimer
// getvfsstat
// getxattr
// ktrace
// lchflags
// lchmod
// lfs_bmapv
// lfs_markv
// lfs_segclean
// lfs_segwait
// lgetxattr
// lio_listio
// listxattr
// llistxattr
// lremovexattr
// lseek
// lsetxattr
// lutimes
// madvise
// mincore
// minherit
// modctl
// mq_close
// mq_getattr
// mq_notify
// mq_open
// mq_receive
// mq_send
// mq_setattr
// mq_timedreceive
// mq_timedsend
// mq_unlink
// mremap
// msgget
// msgrcv
// msgsnd
// nfssvc
// ntp_adjtime
// pmc_control
// pmc_get_info
// pollts
// preadv
// profil
// pselect
// pset_assign
// pset_create
// pset_destroy
// ptrace
// pwritev
// quotactl
// rasctl
// readv
// reboot
// removexattr
// sa_enable
// sa_preempt
// sa_register
// sa_setconcurrency
// sa_stacks
// sa_yield
// sbrk
// sched_yield
// semconfig
// semget
// semop
// setcontext
// setitimer
// setxattr
// shmat
// shmdt
// shmget
// sstk
// statvfs1
// swapctl
// sysarch
// syscall
// timer_create
// timer_delete
// timer_getoverrun
// timer_gettime
// timer_settime
// undelete
// utrace
// uuidgen
// vadvise
// vfork
// writev

} // end unix_package
