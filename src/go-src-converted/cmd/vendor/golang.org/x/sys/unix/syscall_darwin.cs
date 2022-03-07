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

// package unix -- go2cs converted at 2022 March 06 23:26:47 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_darwin.go
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

// SockaddrCtl implements the Sockaddr interface for AF_SYSTEM type sockets.
public partial struct SockaddrCtl {
    public uint ID;
    public uint Unit;
    public RawSockaddrCtl raw;
}

private static (unsafe.Pointer, _Socklen, error) sockaddr(this ptr<SockaddrCtl> _addr_sa) {
    unsafe.Pointer _p0 = default;
    _Socklen _p0 = default;
    error _p0 = default!;
    ref SockaddrCtl sa = ref _addr_sa.val;

    sa.raw.Sc_len = SizeofSockaddrCtl;
    sa.raw.Sc_family = AF_SYSTEM;
    sa.raw.Ss_sysaddr = AF_SYS_CONTROL;
    sa.raw.Sc_id = sa.ID;
    sa.raw.Sc_unit = sa.Unit;
    return (@unsafe.Pointer(_addr_sa.raw), SizeofSockaddrCtl, error.As(null!)!);
}

private static (Sockaddr, error) anyToSockaddrGOOS(nint fd, ptr<RawSockaddrAny> _addr_rsa) {
    Sockaddr _p0 = default;
    error _p0 = default!;
    ref RawSockaddrAny rsa = ref _addr_rsa.val;


    if (rsa.Addr.Family == AF_SYSTEM) 
        var pp = (RawSockaddrCtl.val)(@unsafe.Pointer(rsa));
        if (pp.Ss_sysaddr == AF_SYS_CONTROL) {
            ptr<SockaddrCtl> sa = @new<SockaddrCtl>();
            sa.ID = pp.Sc_id;
            sa.Unit = pp.Sc_unit;
            return (sa, error.As(null!)!);
        }
        return (null, error.As(EAFNOSUPPORT)!);

}

// Some external packages rely on SYS___SYSCTL being defined to implement their
// own sysctl wrappers. Provide it here, even though direct syscalls are no
// longer supported on darwin.
public static readonly var SYS___SYSCTL = SYS_SYSCTL;

// Translate "kern.hostname" to []_C_int{0,1,2,3}.


// Translate "kern.hostname" to []_C_int{0,1,2,3}.
private static (slice<_C_int>, error) nametomib(@string name) {
    slice<_C_int> mib = default;
    error err = default!;

    const var siz = @unsafe.Sizeof(mib[0]); 

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
    array<_C_int> buf = new array<_C_int>(CTL_MAXNAME + 2);
    ref var n = ref heap(uintptr(CTL_MAXNAME) * siz, out ptr<var> _addr_n);

    var p = (byte.val)(@unsafe.Pointer(_addr_buf[0]));
    var (bytes, err) = ByteSliceFromString(name);
    if (err != null) {
        return (null, error.As(err)!);
    }
    err = sysctl(new slice<_C_int>(new _C_int[] { 0, 3 }), p, _addr_n, _addr_bytes[0], uintptr(len(name)));

    if (err != null) {
        return (null, error.As(err)!);
    }
    return (buf[(int)0..(int)n / siz], error.As(null!)!);

}

private static (ulong, bool) direntIno(slice<byte> buf) {
    ulong _p0 = default;
    bool _p0 = default;

    return readInt(buf, @unsafe.Offsetof(new Dirent().Ino), @unsafe.Sizeof(new Dirent().Ino));
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

public static error PtraceAttach(nint pid) {
    error err = default!;

    return error.As(ptrace(PT_ATTACH, pid, 0, 0))!;
}
public static error PtraceDetach(nint pid) {
    error err = default!;

    return error.As(ptrace(PT_DETACH, pid, 0, 0))!;
}

private partial struct attrList {
    public ushort bitmapCount;
    public ushort _;
    public uint CommonAttr;
    public uint VolAttr;
    public uint DirAttr;
    public uint FileAttr;
    public uint Forkattr;
}

//sysnb    pipe(p *[2]int32) (err error)

public static error Pipe(slice<nint> p) {
    error err = default!;

    if (len(p) != 2) {
        return error.As(EINVAL)!;
    }
    ref array<int> x = ref heap(new array<int>(2), out ptr<array<int>> _addr_x);
    err = pipe(_addr_x);
    p[0] = int(x[0]);
    p[1] = int(x[1]);
    return ;

}

public static (nint, error) Getfsstat(slice<Statfs_t> buf, nint flags) {
    nint n = default;
    error err = default!;

    unsafe.Pointer _p0 = default;
    System.UIntPtr bufsize = default;
    if (len(buf) > 0) {
        _p0 = @unsafe.Pointer(_addr_buf[0]);
        bufsize = @unsafe.Sizeof(new Statfs_t()) * uintptr(len(buf));
    }
    return getfsstat(_p0, bufsize, flags);

}

private static ptr<byte> xattrPointer(slice<byte> dest) { 
    // It's only when dest is set to NULL that the OS X implementations of
    // getxattr() and listxattr() return the current sizes of the named attributes.
    // An empty byte array is not sufficient. To maintain the same behaviour as the
    // linux implementation, we wrap around the system calls and pass in NULL when
    // dest is empty.
    ptr<byte> destp;
    if (len(dest) > 0) {
        destp = _addr_dest[0];
    }
    return _addr_destp!;

}

//sys    getxattr(path string, attr string, dest *byte, size int, position uint32, options int) (sz int, err error)

public static (nint, error) Getxattr(@string path, @string attr, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    return getxattr(path, attr, xattrPointer(dest), len(dest), 0, 0);
}

public static (nint, error) Lgetxattr(@string link, @string attr, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    return getxattr(link, attr, xattrPointer(dest), len(dest), 0, XATTR_NOFOLLOW);
}

//sys    fgetxattr(fd int, attr string, dest *byte, size int, position uint32, options int) (sz int, err error)

public static (nint, error) Fgetxattr(nint fd, @string attr, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    return fgetxattr(fd, attr, xattrPointer(dest), len(dest), 0, 0);
}

//sys    setxattr(path string, attr string, data *byte, size int, position uint32, options int) (err error)

public static error Setxattr(@string path, @string attr, slice<byte> data, nint flags) {
    error err = default!;
 
    // The parameters for the OS X implementation vary slightly compared to the
    // linux system call, specifically the position parameter:
    //
    //  linux:
    //      int setxattr(
    //          const char *path,
    //          const char *name,
    //          const void *value,
    //          size_t size,
    //          int flags
    //      );
    //
    //  darwin:
    //      int setxattr(
    //          const char *path,
    //          const char *name,
    //          void *value,
    //          size_t size,
    //          u_int32_t position,
    //          int options
    //      );
    //
    // position specifies the offset within the extended attribute. In the
    // current implementation, only the resource fork extended attribute makes
    // use of this argument. For all others, position is reserved. We simply
    // default to setting it to zero.
    return error.As(setxattr(path, attr, xattrPointer(data), len(data), 0, flags))!;

}

public static error Lsetxattr(@string link, @string attr, slice<byte> data, nint flags) {
    error err = default!;

    return error.As(setxattr(link, attr, xattrPointer(data), len(data), 0, flags | XATTR_NOFOLLOW))!;
}

//sys    fsetxattr(fd int, attr string, data *byte, size int, position uint32, options int) (err error)

public static error Fsetxattr(nint fd, @string attr, slice<byte> data, nint flags) {
    error err = default!;

    return error.As(fsetxattr(fd, attr, xattrPointer(data), len(data), 0, 0))!;
}

//sys    removexattr(path string, attr string, options int) (err error)

public static error Removexattr(@string path, @string attr) {
    error err = default!;
 
    // We wrap around and explicitly zero out the options provided to the OS X
    // implementation of removexattr, we do so for interoperability with the
    // linux variant.
    return error.As(removexattr(path, attr, 0))!;

}

public static error Lremovexattr(@string link, @string attr) {
    error err = default!;

    return error.As(removexattr(link, attr, XATTR_NOFOLLOW))!;
}

//sys    fremovexattr(fd int, attr string, options int) (err error)

public static error Fremovexattr(nint fd, @string attr) {
    error err = default!;

    return error.As(fremovexattr(fd, attr, 0))!;
}

//sys    listxattr(path string, dest *byte, size int, options int) (sz int, err error)

public static (nint, error) Listxattr(@string path, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    return listxattr(path, xattrPointer(dest), len(dest), 0);
}

public static (nint, error) Llistxattr(@string link, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    return listxattr(link, xattrPointer(dest), len(dest), XATTR_NOFOLLOW);
}

//sys    flistxattr(fd int, dest *byte, size int, options int) (sz int, err error)

public static (nint, error) Flistxattr(nint fd, slice<byte> dest) {
    nint sz = default;
    error err = default!;

    return flistxattr(fd, xattrPointer(dest), len(dest), 0);
}

private static error setattrlistTimes(@string path, slice<Timespec> times, nint flags) {
    var (_p0, err) = BytePtrFromString(path);
    if (err != null) {
        return error.As(err)!;
    }
    ref attrList attrList = ref heap(out ptr<attrList> _addr_attrList);
    attrList.bitmapCount = ATTR_BIT_MAP_COUNT;
    attrList.CommonAttr = ATTR_CMN_MODTIME | ATTR_CMN_ACCTIME; 

    // order is mtime, atime: the opposite of Chtimes
    ref array<Timespec> attributes = ref heap(new array<Timespec>(new Timespec[] { times[1], times[0] }), out ptr<array<Timespec>> _addr_attributes);
    nint options = 0;
    if (flags & AT_SYMLINK_NOFOLLOW != 0) {
        options |= FSOPT_NOFOLLOW;
    }
    return error.As(setattrlist(_p0, @unsafe.Pointer(_addr_attrList), @unsafe.Pointer(_addr_attributes), @unsafe.Sizeof(attributes), options))!;

}

//sys    setattrlist(path *byte, list unsafe.Pointer, buf unsafe.Pointer, size uintptr, options int) (err error)

private static error utimensat(nint dirfd, @string path, ptr<array<Timespec>> _addr_times, nint flags) {
    ref array<Timespec> times = ref _addr_times.val;
 
    // Darwin doesn't support SYS_UTIMENSAT
    return error.As(ENOSYS)!;

}

/*
 * Wrapped
 */

//sys    fcntl(fd int, cmd int, arg int) (val int, err error)

//sys    kill(pid int, signum int, posix int) (err error)

public static error Kill(nint pid, syscall.Signal signum) {
    error err = default!;

    return error.As(kill(pid, int(signum), 1))!;
}

//sys    ioctl(fd int, req uint, arg uintptr) (err error)

public static error IoctlCtlInfo(nint fd, ptr<CtlInfo> _addr_ctlInfo) {
    ref CtlInfo ctlInfo = ref _addr_ctlInfo.val;

    var err = ioctl(fd, CTLIOCGINFO, uintptr(@unsafe.Pointer(ctlInfo)));
    runtime.KeepAlive(ctlInfo);
    return error.As(err)!;
}

// IfreqMTU is struct ifreq used to get or set a network device's MTU.
public partial struct IfreqMTU {
    public array<byte> Name;
    public int MTU;
}

// IoctlGetIfreqMTU performs the SIOCGIFMTU ioctl operation on fd to get the MTU
// of the network device specified by ifname.
public static (ptr<IfreqMTU>, error) IoctlGetIfreqMTU(nint fd, @string ifname) {
    ptr<IfreqMTU> _p0 = default!;
    error _p0 = default!;

    ref IfreqMTU ifreq = ref heap(out ptr<IfreqMTU> _addr_ifreq);
    copy(ifreq.Name[..], ifname);
    var err = ioctl(fd, SIOCGIFMTU, uintptr(@unsafe.Pointer(_addr_ifreq)));
    return (_addr__addr_ifreq!, error.As(err)!);
}

// IoctlSetIfreqMTU performs the SIOCSIFMTU ioctl operation on fd to set the MTU
// of the network device specified by ifreq.Name.
public static error IoctlSetIfreqMTU(nint fd, ptr<IfreqMTU> _addr_ifreq) {
    ref IfreqMTU ifreq = ref _addr_ifreq.val;

    var err = ioctl(fd, SIOCSIFMTU, uintptr(@unsafe.Pointer(ifreq)));
    runtime.KeepAlive(ifreq);
    return error.As(err)!;
}

//sys    sysctl(mib []_C_int, old *byte, oldlen *uintptr, new *byte, newlen uintptr) (err error) = SYS_SYSCTL

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
    ref var length = ref heap(int64(count), out ptr<var> _addr_length);
    err = sendfile(infd, outfd, offset, _addr_length, null, 0);
    written = int(length);
    return ;

}

public static (ptr<IPMreqn>, error) GetsockoptIPMreqn(nint fd, nint level, nint opt) {
    ptr<IPMreqn> _p0 = default!;
    error _p0 = default!;

    ref IPMreqn value = ref heap(out ptr<IPMreqn> _addr_value);
    ref var vallen = ref heap(_Socklen(SizeofIPMreqn), out ptr<var> _addr_vallen);
    var errno = getsockopt(fd, level, opt, @unsafe.Pointer(_addr_value), _addr_vallen);
    return (_addr__addr_value!, error.As(errno)!);
}

public static error SetsockoptIPMreqn(nint fd, nint level, nint opt, ptr<IPMreqn> _addr_mreq) {
    error err = default!;
    ref IPMreqn mreq = ref _addr_mreq.val;

    return error.As(setsockopt(fd, level, opt, @unsafe.Pointer(mreq), @unsafe.Sizeof(mreq)))!;
}

// GetsockoptXucred is a getsockopt wrapper that returns an Xucred struct.
// The usual level and opt are SOL_LOCAL and LOCAL_PEERCRED, respectively.
public static (ptr<Xucred>, error) GetsockoptXucred(nint fd, nint level, nint opt) {
    ptr<Xucred> _p0 = default!;
    error _p0 = default!;

    ptr<Xucred> x = @new<Xucred>();
    ref var vallen = ref heap(_Socklen(SizeofXucred), out ptr<var> _addr_vallen);
    var err = getsockopt(fd, level, opt, @unsafe.Pointer(x), _addr_vallen);
    return (_addr_x!, error.As(err)!);
}

//sys    sendfile(infd int, outfd int, offset int64, len *int64, hdtr unsafe.Pointer, flags int) (err error)

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
//sys    ClockGettime(clockid int32, time *Timespec) (err error)
//sys    Close(fd int) (err error)
//sys    Clonefile(src string, dst string, flags int) (err error)
//sys    Clonefileat(srcDirfd int, src string, dstDirfd int, dst string, flags int) (err error)
//sys    Dup(fd int) (nfd int, err error)
//sys    Dup2(from int, to int) (err error)
//sys    Exchangedata(path1 string, path2 string, options int) (err error)
//sys    Exit(code int)
//sys    Faccessat(dirfd int, path string, mode uint32, flags int) (err error)
//sys    Fchdir(fd int) (err error)
//sys    Fchflags(fd int, flags int) (err error)
//sys    Fchmod(fd int, mode uint32) (err error)
//sys    Fchmodat(dirfd int, path string, mode uint32, flags int) (err error)
//sys    Fchown(fd int, uid int, gid int) (err error)
//sys    Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
//sys    Fclonefileat(srcDirfd int, dstDirfd int, dst string, flags int) (err error)
//sys    Flock(fd int, how int) (err error)
//sys    Fpathconf(fd int, name int) (val int, err error)
//sys    Fsync(fd int) (err error)
//sys    Ftruncate(fd int, length int64) (err error)
//sys    Getcwd(buf []byte) (n int, err error)
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
//sysnb    Gettimeofday(tp *Timeval) (err error)
//sysnb    Getuid() (uid int)
//sysnb    Issetugid() (tainted bool)
//sys    Kqueue() (fd int, err error)
//sys    Lchown(path string, uid int, gid int) (err error)
//sys    Link(path string, link string) (err error)
//sys    Linkat(pathfd int, path string, linkfd int, link string, flags int) (err error)
//sys    Listen(s int, backlog int) (err error)
//sys    Mkdir(path string, mode uint32) (err error)
//sys    Mkdirat(dirfd int, path string, mode uint32) (err error)
//sys    Mkfifo(path string, mode uint32) (err error)
//sys    Mknod(path string, mode uint32, dev int) (err error)
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
//sys    mmap(addr uintptr, length uintptr, prot int, flag int, fd int, pos int64) (ret uintptr, err error)
//sys    munmap(addr uintptr, length uintptr) (err error)
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

} // end unix_package
