// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2022 March 06 22:13:22 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\sockopt_windows.go
using syscall = go.syscall_package;

namespace go.@internal;

public static partial class poll_package {

    // Setsockopt wraps the setsockopt network call.
private static error Setsockopt(this ptr<FD> _addr_fd, int level, int optname, ptr<byte> _addr_optval, int optlen) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;
    ref byte optval = ref _addr_optval.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }

    defer(fd.decref());
    return error.As(syscall.Setsockopt(fd.Sysfd, level, optname, optval, optlen))!;

});

// WSAIoctl wraps the WSAIoctl network call.
private static error WSAIoctl(this ptr<FD> _addr_fd, uint iocc, ptr<byte> _addr_inbuf, uint cbif, ptr<byte> _addr_outbuf, uint cbob, ptr<uint> _addr_cbbr, ptr<syscall.Overlapped> _addr_overlapped, System.UIntPtr completionRoutine) => func((defer, _, _) => {
    ref FD fd = ref _addr_fd.val;
    ref byte inbuf = ref _addr_inbuf.val;
    ref byte outbuf = ref _addr_outbuf.val;
    ref uint cbbr = ref _addr_cbbr.val;
    ref syscall.Overlapped overlapped = ref _addr_overlapped.val;

    {
        var err = fd.incref();

        if (err != null) {
            return error.As(err)!;
        }
    }

    defer(fd.decref());
    return error.As(syscall.WSAIoctl(fd.Sysfd, iocc, inbuf, cbif, outbuf, cbob, cbbr, overlapped, completionRoutine))!;

});

} // end poll_package
