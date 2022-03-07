// Copyright 2009,2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// FreeBSD system calls.
// This file is compiled as ordinary Go code,
// but it is also input to mksyscall,
// which parses the //sys lines and generates system call stubs.
// Note that sometimes we use a lowercase //sys name and wrap
// it in our own nicer implementation, either here or in
// syscall_bsd.go or syscall_unix.go.

// package unix -- go2cs converted at 2022 March 06 23:26:52 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_freebsd.go
using sync = go.sync_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

public static readonly nint SYS_FSTAT_FREEBSD12 = 551; // { int fstat(int fd, _Out_ struct stat *sb); }
public static readonly nint SYS_FSTATAT_FREEBSD12 = 552; // { int fstatat(int fd, _In_z_ char *path, \
public static readonly nint SYS_GETDIRENTRIES_FREEBSD12 = 554; // { ssize_t getdirentries(int fd, \
public static readonly nint SYS_STATFS_FREEBSD12 = 555; // { int statfs(_In_z_ char *path, \
public static readonly nint SYS_FSTATFS_FREEBSD12 = 556; // { int fstatfs(int fd, \
public static readonly nint SYS_GETFSSTAT_FREEBSD12 = 557; // { int getfsstat( \
public static readonly nint SYS_MKNODAT_FREEBSD12 = 559; // { int mknodat(int fd, _In_z_ char *path, \

// See https://www.freebsd.org/doc/en_US.ISO8859-1/books/porters-handbook/versions.html.
private static sync.Once osreldateOnce = default;private static uint osreldate = default;

// INO64_FIRST from /usr/src/lib/libc/sys/compat-ino64.h
private static readonly nint _ino64First = 1200031;



private static bool supportsABI(uint ver) {
    osreldateOnce.Do(() => {
        osreldate, _ = SysctlUint32("kern.osreldate");
    });
    return osreldate >= ver;

}

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

public static error Pipe(slice<nint> p) {
    error err = default!;

    return error.As(Pipe2(p, 0))!;
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

public static (nint, Sockaddr, error) Accept4(nint fd, nint flags) => func((_, panic, _) => {
    nint nfd = default;
    Sockaddr sa = default;
    error err = default!;

    ref RawSockaddrAny rsa = ref heap(out ptr<RawSockaddrAny> _addr_rsa);
    ref _Socklen len = ref heap(SizeofSockaddrAny, out ptr<_Socklen> _addr_len);
    nfd, err = accept4(fd, _addr_rsa, _addr_len, flags);
    if (err != null) {
        return ;
    }
    if (len > SizeofSockaddrAny) {
        panic("RawSockaddrAny too small");
    }
    sa, err = anyToSockaddr(fd, _addr_rsa);
    if (err != null) {
        Close(nfd);
        nfd = 0;
    }
    return ;

});

//sys    Getcwd(buf []byte) (n int, err error) = SYS___GETCWD

public static (nint, error) Getfsstat(slice<Statfs_t> buf, nint flags) {
    nint n = default;
    error err = default!;

    unsafe.Pointer _p0 = default;    System.UIntPtr bufsize = default;    slice<statfs_freebsd11_t> oldBuf = default;    bool needsConvert = default;

    if (len(buf) > 0) {
        if (supportsABI(_ino64First)) {
            _p0 = @unsafe.Pointer(_addr_buf[0]);
            bufsize = @unsafe.Sizeof(new Statfs_t()) * uintptr(len(buf));
        }
        else
 {
            var n = len(buf);
            oldBuf = make_slice<statfs_freebsd11_t>(n);
            _p0 = @unsafe.Pointer(_addr_oldBuf[0]);
            bufsize = @unsafe.Sizeof(new statfs_freebsd11_t()) * uintptr(n);
            needsConvert = true;
        }
    }
    System.UIntPtr sysno = SYS_GETFSSTAT;
    if (supportsABI(_ino64First)) {
        sysno = SYS_GETFSSTAT_FREEBSD12;
    }
    var (r0, _, e1) = Syscall(sysno, uintptr(_p0), bufsize, uintptr(flags));
    n = int(r0);
    if (e1 != 0) {
        err = e1;
    }
    if (e1 == 0 && needsConvert) {
        foreach (var (i) in oldBuf) {
            buf[i].convertFrom(_addr_oldBuf[i]);
        }
    }
    return ;

}

private static error setattrlistTimes(@string path, slice<Timespec> times, nint flags) { 
    // used on Darwin for UtimesNano
    return error.As(ENOSYS)!;

}

//sys    ioctl(fd int, req uint, arg uintptr) (err error)

//sys    sysctl(mib []_C_int, old *byte, oldlen *uintptr, new *byte, newlen uintptr) (err error) = SYS___SYSCTL

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

public static error Stat(@string path, ptr<Stat_t> _addr_st) {
    error err = default!;
    ref Stat_t st = ref _addr_st.val;

    ref stat_freebsd11_t oldStat = ref heap(out ptr<stat_freebsd11_t> _addr_oldStat);
    if (supportsABI(_ino64First)) {
        return error.As(fstatat_freebsd12(AT_FDCWD, path, st, 0))!;
    }
    err = stat(path, _addr_oldStat);
    if (err != null) {
        return error.As(err)!;
    }
    st.convertFrom(_addr_oldStat);
    return error.As(null!)!;

}

public static error Lstat(@string path, ptr<Stat_t> _addr_st) {
    error err = default!;
    ref Stat_t st = ref _addr_st.val;

    ref stat_freebsd11_t oldStat = ref heap(out ptr<stat_freebsd11_t> _addr_oldStat);
    if (supportsABI(_ino64First)) {
        return error.As(fstatat_freebsd12(AT_FDCWD, path, st, AT_SYMLINK_NOFOLLOW))!;
    }
    err = lstat(path, _addr_oldStat);
    if (err != null) {
        return error.As(err)!;
    }
    st.convertFrom(_addr_oldStat);
    return error.As(null!)!;

}

public static error Fstat(nint fd, ptr<Stat_t> _addr_st) {
    error err = default!;
    ref Stat_t st = ref _addr_st.val;

    ref stat_freebsd11_t oldStat = ref heap(out ptr<stat_freebsd11_t> _addr_oldStat);
    if (supportsABI(_ino64First)) {
        return error.As(fstat_freebsd12(fd, st))!;
    }
    err = fstat(fd, _addr_oldStat);
    if (err != null) {
        return error.As(err)!;
    }
    st.convertFrom(_addr_oldStat);
    return error.As(null!)!;

}

public static error Fstatat(nint fd, @string path, ptr<Stat_t> _addr_st, nint flags) {
    error err = default!;
    ref Stat_t st = ref _addr_st.val;

    ref stat_freebsd11_t oldStat = ref heap(out ptr<stat_freebsd11_t> _addr_oldStat);
    if (supportsABI(_ino64First)) {
        return error.As(fstatat_freebsd12(fd, path, st, flags))!;
    }
    err = fstatat(fd, path, _addr_oldStat, flags);
    if (err != null) {
        return error.As(err)!;
    }
    st.convertFrom(_addr_oldStat);
    return error.As(null!)!;

}

public static error Statfs(@string path, ptr<Statfs_t> _addr_st) {
    error err = default!;
    ref Statfs_t st = ref _addr_st.val;

    ref statfs_freebsd11_t oldStatfs = ref heap(out ptr<statfs_freebsd11_t> _addr_oldStatfs);
    if (supportsABI(_ino64First)) {
        return error.As(statfs_freebsd12(path, st))!;
    }
    err = statfs(path, _addr_oldStatfs);
    if (err != null) {
        return error.As(err)!;
    }
    st.convertFrom(_addr_oldStatfs);
    return error.As(null!)!;

}

public static error Fstatfs(nint fd, ptr<Statfs_t> _addr_st) {
    error err = default!;
    ref Statfs_t st = ref _addr_st.val;

    ref statfs_freebsd11_t oldStatfs = ref heap(out ptr<statfs_freebsd11_t> _addr_oldStatfs);
    if (supportsABI(_ino64First)) {
        return error.As(fstatfs_freebsd12(fd, st))!;
    }
    err = fstatfs(fd, _addr_oldStatfs);
    if (err != null) {
        return error.As(err)!;
    }
    st.convertFrom(_addr_oldStatfs);
    return error.As(null!)!;

}

public static (nint, error) Getdents(nint fd, slice<byte> buf) {
    nint n = default;
    error err = default!;

    return Getdirentries(fd, buf, _addr_null);
}

public static (nint, error) Getdirentries(nint fd, slice<byte> buf, ptr<System.UIntPtr> _addr_basep) {
    nint n = default;
    error err = default!;
    ref System.UIntPtr basep = ref _addr_basep.val;

    if (supportsABI(_ino64First)) {
        if (basep == null || @unsafe.Sizeof(basep) == 8) {
            return getdirentries_freebsd12(fd, buf, (uint64.val)(@unsafe.Pointer(basep)));
        }
        ref ulong @base = ref heap(uint64(basep), out ptr<ulong> _addr_@base);
        n, err = getdirentries_freebsd12(fd, buf, _addr_base);
        basep = uintptr(base);
        if (base >> 32 != 0) { 
            // We can't stuff the base back into a uintptr, so any
            // future calls would be suspect. Generate an error.
            // EIO is allowed by getdirentries.
            err = EIO;

        }
        return ;

    }
    var oldBufLen = roundup(len(buf) / 4, _dirblksiz);
    var oldBuf = make_slice<byte>(oldBufLen);
    n, err = getdirentries(fd, oldBuf, basep);
    if (err == null && n > 0) {
        n = convertFromDirents11(buf, oldBuf[..(int)n]);
    }
    return ;

}

public static error Mknod(@string path, uint mode, ulong dev) {
    error err = default!;

    nint oldDev = default;
    if (supportsABI(_ino64First)) {
        return error.As(mknodat_freebsd12(AT_FDCWD, path, mode, dev))!;
    }
    oldDev = int(dev);
    return error.As(mknod(path, mode, oldDev))!;

}

public static error Mknodat(nint fd, @string path, uint mode, ulong dev) {
    error err = default!;

    nint oldDev = default;
    if (supportsABI(_ino64First)) {
        return error.As(mknodat_freebsd12(fd, path, mode, dev))!;
    }
    oldDev = int(dev);
    return error.As(mknodat(fd, path, mode, oldDev))!;

}

// round x to the nearest multiple of y, larger or equal to x.
//
// from /usr/include/sys/param.h Macros for counting and rounding.
// #define roundup(x, y)   ((((x)+((y)-1))/(y))*(y))
private static nint roundup(nint x, nint y) {
    return ((x + y - 1) / y) * y;
}

private static void convertFrom(this ptr<Stat_t> _addr_s, ptr<stat_freebsd11_t> _addr_old) {
    ref Stat_t s = ref _addr_s.val;
    ref stat_freebsd11_t old = ref _addr_old.val;

    s.val = new Stat_t(Dev:uint64(old.Dev),Ino:uint64(old.Ino),Nlink:uint64(old.Nlink),Mode:old.Mode,Uid:old.Uid,Gid:old.Gid,Rdev:uint64(old.Rdev),Atim:old.Atim,Mtim:old.Mtim,Ctim:old.Ctim,Btim:old.Btim,Size:old.Size,Blocks:old.Blocks,Blksize:old.Blksize,Flags:old.Flags,Gen:uint64(old.Gen),);
}

private static void convertFrom(this ptr<Statfs_t> _addr_s, ptr<statfs_freebsd11_t> _addr_old) {
    ref Statfs_t s = ref _addr_s.val;
    ref statfs_freebsd11_t old = ref _addr_old.val;

    s.val = new Statfs_t(Version:_statfsVersion,Type:old.Type,Flags:old.Flags,Bsize:old.Bsize,Iosize:old.Iosize,Blocks:old.Blocks,Bfree:old.Bfree,Bavail:old.Bavail,Files:old.Files,Ffree:old.Ffree,Syncwrites:old.Syncwrites,Asyncwrites:old.Asyncwrites,Syncreads:old.Syncreads,Asyncreads:old.Asyncreads,Namemax:old.Namemax,Owner:old.Owner,Fsid:old.Fsid,);

    ref var sl = ref heap(old.Fstypename[..], out ptr<var> _addr_sl);
    var n = clen(new ptr<ptr<ptr<slice<byte>>>>(@unsafe.Pointer(_addr_sl)));
    copy(s.Fstypename[..], old.Fstypename[..(int)n]);

    sl = old.Mntfromname[..];
    n = clen(new ptr<ptr<ptr<slice<byte>>>>(@unsafe.Pointer(_addr_sl)));
    copy(s.Mntfromname[..], old.Mntfromname[..(int)n]);

    sl = old.Mntonname[..];
    n = clen(new ptr<ptr<ptr<slice<byte>>>>(@unsafe.Pointer(_addr_sl)));
    copy(s.Mntonname[..], old.Mntonname[..(int)n]);
}

private static nint convertFromDirents11(slice<byte> buf, slice<byte> old) {
    const var fixedSize = int(@unsafe.Offsetof(new Dirent().Name));
    const var oldFixedSize = int(@unsafe.Offsetof(new dirent_freebsd11().Name));

    nint dstPos = 0;
    nint srcPos = 0;
    while (dstPos + fixedSize < len(buf) && srcPos + oldFixedSize < len(old)) {
        ref Dirent dstDirent = ref heap(out ptr<Dirent> _addr_dstDirent);
        ref dirent_freebsd11 srcDirent = ref heap(out ptr<dirent_freebsd11> _addr_srcDirent); 

        // If multiple direntries are written, sometimes when we reach the final one,
        // we may have cap of old less than size of dirent_freebsd11.
        copy(new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_srcDirent))[..], old[(int)srcPos..]);

        var reclen = roundup(fixedSize + int(srcDirent.Namlen) + 1, 8);
        if (dstPos + reclen > len(buf)) {
            break;
        }
        dstDirent.Fileno = uint64(srcDirent.Fileno);
        dstDirent.Off = 0;
        dstDirent.Reclen = uint16(reclen);
        dstDirent.Type = srcDirent.Type;
        dstDirent.Pad0 = 0;
        dstDirent.Namlen = uint16(srcDirent.Namlen);
        dstDirent.Pad1 = 0;

        copy(dstDirent.Name[..], srcDirent.Name[..(int)srcDirent.Namlen]);
        copy(buf[(int)dstPos..], new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_dstDirent))[..]);
        var padding = buf[(int)dstPos + fixedSize + int(dstDirent.Namlen)..(int)dstPos + reclen];
        foreach (var (i) in padding) {
            padding[i] = 0;
        }        dstPos += int(dstDirent.Reclen);
        srcPos += int(srcDirent.Reclen);

    }

    return dstPos;

}

public static (nint, error) Sendfile(nint outfd, nint infd, ptr<long> _addr_offset, nint count) {
    nint written = default;
    error err = default!;
    ref long offset = ref _addr_offset.val;

    if (raceenabled) {
        raceReleaseMerge(@unsafe.Pointer(_addr_ioSync));
    }
    return sendfile(outfd, infd, offset, count);

}

//sys    ptrace(request int, pid int, addr uintptr, data int) (err error)

public static error PtraceAttach(nint pid) {
    error err = default!;

    return error.As(ptrace(PTRACE_ATTACH, pid, 0, 0))!;
}

public static error PtraceCont(nint pid, nint signal) {
    error err = default!;

    return error.As(ptrace(PTRACE_CONT, pid, 1, signal))!;
}

public static error PtraceDetach(nint pid) {
    error err = default!;

    return error.As(ptrace(PTRACE_DETACH, pid, 1, 0))!;
}

public static error PtraceGetFpRegs(nint pid, ptr<FpReg> _addr_fpregsout) {
    error err = default!;
    ref FpReg fpregsout = ref _addr_fpregsout.val;

    return error.As(ptrace(PTRACE_GETFPREGS, pid, uintptr(@unsafe.Pointer(fpregsout)), 0))!;
}

public static error PtraceGetRegs(nint pid, ptr<Reg> _addr_regsout) {
    error err = default!;
    ref Reg regsout = ref _addr_regsout.val;

    return error.As(ptrace(PTRACE_GETREGS, pid, uintptr(@unsafe.Pointer(regsout)), 0))!;
}

public static error PtraceLwpEvents(nint pid, nint enable) {
    error err = default!;

    return error.As(ptrace(PTRACE_LWPEVENTS, pid, 0, enable))!;
}

public static error PtraceLwpInfo(nint pid, System.UIntPtr info) {
    error err = default!;

    return error.As(ptrace(PTRACE_LWPINFO, pid, info, int(@unsafe.Sizeof(new PtraceLwpInfoStruct()))))!;
}

public static (nint, error) PtracePeekData(nint pid, System.UIntPtr addr, slice<byte> @out) {
    nint count = default;
    error err = default!;

    return PtraceIO(PIOD_READ_D, pid, addr, out, SizeofLong);
}

public static (nint, error) PtracePeekText(nint pid, System.UIntPtr addr, slice<byte> @out) {
    nint count = default;
    error err = default!;

    return PtraceIO(PIOD_READ_I, pid, addr, out, SizeofLong);
}

public static (nint, error) PtracePokeData(nint pid, System.UIntPtr addr, slice<byte> data) {
    nint count = default;
    error err = default!;

    return PtraceIO(PIOD_WRITE_D, pid, addr, data, SizeofLong);
}

public static (nint, error) PtracePokeText(nint pid, System.UIntPtr addr, slice<byte> data) {
    nint count = default;
    error err = default!;

    return PtraceIO(PIOD_WRITE_I, pid, addr, data, SizeofLong);
}

public static error PtraceSetRegs(nint pid, ptr<Reg> _addr_regs) {
    error err = default!;
    ref Reg regs = ref _addr_regs.val;

    return error.As(ptrace(PTRACE_SETREGS, pid, uintptr(@unsafe.Pointer(regs)), 0))!;
}

public static error PtraceSingleStep(nint pid) {
    error err = default!;

    return error.As(ptrace(PTRACE_SINGLESTEP, pid, 1, 0))!;
}

/*
 * Exposed directly
 */
//sys    Access(path string, mode uint32) (err error)
//sys    Adjtime(delta *Timeval, olddelta *Timeval) (err error)
//sys    CapEnter() (err error)
//sys    capRightsGet(version int, fd int, rightsp *CapRights) (err error) = SYS___CAP_RIGHTS_GET
//sys    capRightsLimit(fd int, rightsp *CapRights) (err error)
//sys    Chdir(path string) (err error)
//sys    Chflags(path string, flags int) (err error)
//sys    Chmod(path string, mode uint32) (err error)
//sys    Chown(path string, uid int, gid int) (err error)
//sys    Chroot(path string) (err error)
//sys    Close(fd int) (err error)
//sys    Dup(fd int) (nfd int, err error)
//sys    Dup2(from int, to int) (err error)
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
//sys    Fadvise(fd int, offset int64, length int64, advice int) (err error) = SYS_POSIX_FADVISE
//sys    Faccessat(dirfd int, path string, mode uint32, flags int) (err error)
//sys    Fchdir(fd int) (err error)
//sys    Fchflags(fd int, flags int) (err error)
//sys    Fchmod(fd int, mode uint32) (err error)
//sys    Fchmodat(dirfd int, path string, mode uint32, flags int) (err error)
//sys    Fchown(fd int, uid int, gid int) (err error)
//sys    Fchownat(dirfd int, path string, uid int, gid int, flags int) (err error)
//sys    Flock(fd int, how int) (err error)
//sys    Fpathconf(fd int, name int) (val int, err error)
//sys    fstat(fd int, stat *stat_freebsd11_t) (err error)
//sys    fstat_freebsd12(fd int, stat *Stat_t) (err error)
//sys    fstatat(fd int, path string, stat *stat_freebsd11_t, flags int) (err error)
//sys    fstatat_freebsd12(fd int, path string, stat *Stat_t, flags int) (err error)
//sys    fstatfs(fd int, stat *statfs_freebsd11_t) (err error)
//sys    fstatfs_freebsd12(fd int, stat *Statfs_t) (err error)
//sys    Fsync(fd int) (err error)
//sys    Ftruncate(fd int, length int64) (err error)
//sys    getdirentries(fd int, buf []byte, basep *uintptr) (n int, err error)
//sys    getdirentries_freebsd12(fd int, buf []byte, basep *uint64) (n int, err error)
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
//sys    lstat(path string, stat *stat_freebsd11_t) (err error)
//sys    Mkdir(path string, mode uint32) (err error)
//sys    Mkdirat(dirfd int, path string, mode uint32) (err error)
//sys    Mkfifo(path string, mode uint32) (err error)
//sys    mknod(path string, mode uint32, dev int) (err error)
//sys    mknodat(fd int, path string, mode uint32, dev int) (err error)
//sys    mknodat_freebsd12(fd int, path string, mode uint32, dev uint64) (err error)
//sys    Nanosleep(time *Timespec, leftover *Timespec) (err error)
//sys    Open(path string, mode int, perm uint32) (fd int, err error)
//sys    Openat(fdat int, path string, mode int, perm uint32) (fd int, err error)
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
//sysnb    Setsid() (pid int, err error)
//sysnb    Settimeofday(tp *Timeval) (err error)
//sysnb    Setuid(uid int) (err error)
//sys    stat(path string, stat *stat_freebsd11_t) (err error)
//sys    statfs(path string, stat *statfs_freebsd11_t) (err error)
//sys    statfs_freebsd12(path string, stat *Statfs_t) (err error)
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
//sys    accept4(fd int, rsa *RawSockaddrAny, addrlen *_Socklen, flags int) (nfd int, err error)
//sys    utimensat(dirfd int, path string, times *[2]Timespec, flags int) (err error)

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
// Getdents
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
