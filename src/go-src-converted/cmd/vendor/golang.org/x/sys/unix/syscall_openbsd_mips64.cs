// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 23:27:15 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_openbsd_mips64.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

private static Timespec setTimespec(long sec, long nsec) {
    return new Timespec(Sec:sec,Nsec:nsec);
}

private static Timeval setTimeval(long sec, long usec) {
    return new Timeval(Sec:sec,Usec:usec);
}

public static void SetKevent(ptr<Kevent_t> _addr_k, nint fd, nint mode, nint flags) {
    ref Kevent_t k = ref _addr_k.val;

    k.Ident = uint64(fd);
    k.Filter = int16(mode);
    k.Flags = uint16(flags);
}

private static void SetLen(this ptr<Iovec> _addr_iov, nint length) {
    ref Iovec iov = ref _addr_iov.val;

    iov.Len = uint64(length);
}

private static void SetControllen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Controllen = uint32(length);
}

private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, nint length) {
    ref Cmsghdr cmsg = ref _addr_cmsg.val;

    cmsg.Len = uint32(length);
}

// SYS___SYSCTL is used by syscall_bsd.go for all BSDs, but in modern versions
// of OpenBSD the syscall is called sysctl instead of __sysctl.
public static readonly var SYS___SYSCTL = SYS_SYSCTL;


} // end unix_package
