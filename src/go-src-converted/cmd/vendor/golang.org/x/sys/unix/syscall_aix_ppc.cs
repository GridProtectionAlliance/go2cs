// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix && ppc
// +build aix,ppc

// package unix -- go2cs converted at 2022 March 06 23:26:42 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_aix_ppc.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    //sysnb    Getrlimit(resource int, rlim *Rlimit) (err error) = getrlimit64
    //sysnb    Setrlimit(resource int, rlim *Rlimit) (err error) = setrlimit64
    //sys    Seek(fd int, offset int64, whence int) (off int64, err error) = lseek64

    //sys    mmap(addr uintptr, length uintptr, prot int, flags int, fd int, offset int64) (xaddr uintptr, err error)
private static Timespec setTimespec(long sec, long nsec) {
    return new Timespec(Sec:int32(sec),Nsec:int32(nsec));
}

private static Timeval setTimeval(long sec, long usec) {
    return new Timeval(Sec:int32(sec),Usec:int32(usec));
}

private static void SetLen(this ptr<Iovec> _addr_iov, nint length) {
    ref Iovec iov = ref _addr_iov.val;

    iov.Len = uint32(length);
}

private static void SetControllen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Controllen = uint32(length);
}

private static void SetIovlen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Iovlen = int32(length);
}

private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, nint length) {
    ref Cmsghdr cmsg = ref _addr_cmsg.val;

    cmsg.Len = uint32(length);
}

public static error Fstat(nint fd, ptr<Stat_t> _addr_stat) {
    ref Stat_t stat = ref _addr_stat.val;

    return error.As(fstat(fd, stat))!;
}

public static error Fstatat(nint dirfd, @string path, ptr<Stat_t> _addr_stat, nint flags) {
    ref Stat_t stat = ref _addr_stat.val;

    return error.As(fstatat(dirfd, path, stat, flags))!;
}

public static error Lstat(@string path, ptr<Stat_t> _addr_stat) {
    ref Stat_t stat = ref _addr_stat.val;

    return error.As(lstat(path, stat))!;
}

public static error Stat(@string path, ptr<Stat_t> _addr_statptr) {
    ref Stat_t statptr = ref _addr_statptr.val;

    return error.As(stat(path, statptr))!;
}

} // end unix_package
