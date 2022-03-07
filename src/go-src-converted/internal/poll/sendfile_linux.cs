// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package poll -- go2cs converted at 2022 March 06 22:13:18 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Program Files\Go\src\internal\poll\sendfile_linux.go
using syscall = go.syscall_package;

namespace go.@internal;

public static partial class poll_package {

    // maxSendfileSize is the largest chunk size we ask the kernel to copy
    // at a time.
private static readonly nint maxSendfileSize = 4 << 20;

// SendFile wraps the sendfile system call.


// SendFile wraps the sendfile system call.
public static (long, error) SendFile(ptr<FD> _addr_dstFD, nint src, long remain) => func((defer, _, _) => {
    long _p0 = default;
    error _p0 = default!;
    ref FD dstFD = ref _addr_dstFD.val;

    {
        var err__prev1 = err;

        var err = dstFD.writeLock();

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }

    defer(dstFD.writeUnlock());
    {
        var err__prev1 = err;

        err = dstFD.pd.prepareWrite(dstFD.isFile);

        if (err != null) {
            return (0, error.As(err)!);
        }
        err = err__prev1;

    }


    var dst = dstFD.Sysfd;
    long written = default;
    err = default!;
    while (remain > 0) {
        var n = maxSendfileSize;
        if (int64(n) > remain) {
            n = int(remain);
        }
        var (n, err1) = syscall.Sendfile(dst, src, null, n);
        if (n > 0) {
            written += int64(n);
            remain -= int64(n);
        }
        else if (n == 0 && err1 == null) {
            break;
        }
        if (err1 == syscall.EINTR) {
            continue;
        }
        if (err1 == syscall.EAGAIN) {
            err1 = dstFD.pd.waitWrite(dstFD.isFile);

            if (err1 == null) {
                continue;
            }

        }
        if (err1 != null) { 
            // This includes syscall.ENOSYS (no kernel
            // support) and syscall.EINVAL (fd types which
            // don't implement sendfile)
            err = err1;
            break;

        }
    }
    return (written, error.As(err)!);

});

} // end poll_package
