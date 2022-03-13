// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || illumos || linux || netbsd || openbsd
// +build darwin dragonfly freebsd illumos linux netbsd openbsd

// package poll -- go2cs converted at 2022 March 13 05:27:54 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\writev.go
namespace go.@internal;

using io = io_package;
using syscall = syscall_package;


// Writev wraps the writev system call.

public static partial class poll_package {

private static (long, error) Writev(this ptr<FD> _addr_fd, ptr<slice<slice<byte>>> _addr_v) => func((defer, _, _) => {
    long _p0 = default;
    error _p0 = default!;
    ref FD fd = ref _addr_fd.val;
    ref slice<slice<byte>> v = ref _addr_v.val;

    {
        var err__prev1 = err;

        var err = fd.writeLock();

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }
    defer(fd.writeUnlock());
    {
        var err__prev1 = err;

        err = fd.pd.prepareWrite(fd.isFile);

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }

    slice<syscall.Iovec> iovecs = default;
    if (fd.iovecs != null) {
        iovecs = fd.iovecs.val;
    }
    nint maxVec = 1024;

    long n = default;
    err = default!;
    while (len(v) > 0) {
        iovecs = iovecs[..(int)0];
        foreach (var (_, chunk) in v) {
            if (len(chunk) == 0) {
                continue;
            }
            iovecs = append(iovecs, newIovecWithBase(_addr_chunk[0]));
            if (fd.IsStream && len(chunk) > 1 << 30) {
                iovecs[len(iovecs) - 1].SetLen(1 << 30);
                break; // continue chunk on next writev
            }
            iovecs[len(iovecs) - 1].SetLen(len(chunk));
            if (len(iovecs) == maxVec) {
                break;
            }
        }        if (len(iovecs) == 0) {
            break;
        }
        if (fd.iovecs == null) {
            fd.iovecs = @new<syscall.Iovec>();
        }
        fd.iovecs.val = iovecs; // cache

        System.UIntPtr wrote = default;
        wrote, err = writev(fd.Sysfd, iovecs);
        if (wrote == ~uintptr(0)) {
            wrote = 0;
        }
        TestHookDidWritev(int(wrote));
        n += int64(wrote);
        consume(v, int64(wrote));
        foreach (var (i) in iovecs) {
            iovecs[i] = new syscall.Iovec();
        }        if (err != null) {
            if (err == syscall.EINTR) {
                continue;
            }
            if (err == syscall.EAGAIN) {
                err = fd.pd.waitWrite(fd.isFile);

                if (err == null) {
                    continue;
                }
            }
            break;
        }
        if (n == 0) {
            err = io.ErrUnexpectedEOF;
            break;
        }
    }
    return (n, error.As(err)!);
});

} // end poll_package
