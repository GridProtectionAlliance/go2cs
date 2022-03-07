// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 && dragonfly
// +build amd64,dragonfly

// package unix -- go2cs converted at 2022 March 06 23:26:50 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\syscall_dragonfly_amd64.go
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;

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

private static void SetIovlen(this ptr<Msghdr> _addr_msghdr, nint length) {
    ref Msghdr msghdr = ref _addr_msghdr.val;

    msghdr.Iovlen = int32(length);
}

private static void SetLen(this ptr<Cmsghdr> _addr_cmsg, nint length) {
    ref Cmsghdr cmsg = ref _addr_cmsg.val;

    cmsg.Len = uint32(length);
}

private static (nint, error) sendfile(nint outfd, nint infd, ptr<long> _addr_offset, nint count) {
    nint written = default;
    error err = default!;
    ref long offset = ref _addr_offset.val;

    ref ulong writtenOut = ref heap(0, out ptr<ulong> _addr_writtenOut);
    var (_, _, e1) = Syscall9(SYS_SENDFILE, uintptr(infd), uintptr(outfd), uintptr(offset), uintptr(count), 0, uintptr(@unsafe.Pointer(_addr_writtenOut)), 0, 0, 0);

    written = int(writtenOut);

    if (e1 != 0) {
        err = e1;
    }
    return ;

}

public static (System.UIntPtr, System.UIntPtr, syscall.Errno) Syscall9(System.UIntPtr num, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6, System.UIntPtr a7, System.UIntPtr a8, System.UIntPtr a9);

} // end unix_package
