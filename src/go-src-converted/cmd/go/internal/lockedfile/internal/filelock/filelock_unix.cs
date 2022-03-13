// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build darwin || dragonfly || freebsd || illumos || linux || netbsd || openbsd
// +build darwin dragonfly freebsd illumos linux netbsd openbsd

// package filelock -- go2cs converted at 2022 March 13 06:30:19 UTC
// import "cmd/go/internal/lockedfile/internal/filelock" ==> using filelock = go.cmd.go.@internal.lockedfile.@internal.filelock_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\lockedfile\internal\filelock\filelock_unix.go
namespace go.cmd.go.@internal.lockedfile.@internal;

using fs = io.fs_package;
using syscall = syscall_package;

public static partial class filelock_package {

private partial struct lockType { // : short
}

private static readonly lockType readLock = syscall.LOCK_SH;
private static readonly lockType writeLock = syscall.LOCK_EX;

private static error @lock(File f, lockType lt) {
    error err = default!;

    while (true) {
        err = syscall.Flock(int(f.Fd()), int(lt));
        if (err != syscall.EINTR) {
            break;
        }
    }
    if (err != null) {
        return error.As(addr(new fs.PathError(Op:lt.String(),Path:f.Name(),Err:err,))!)!;
    }
    return error.As(null!)!;
}

private static error unlock(File f) {
    return error.As(lock(f, syscall.LOCK_UN))!;
}

private static bool isNotSupported(error err) {
    return err == syscall.ENOSYS || err == syscall.ENOTSUP || err == syscall.EOPNOTSUPP || err == ErrNotSupported;
}

} // end filelock_package
