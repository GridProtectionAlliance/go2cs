// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !aix && !darwin && !dragonfly && !freebsd && !linux && !netbsd && !openbsd && !plan9 && !solaris && !windows
// +build !aix,!darwin,!dragonfly,!freebsd,!linux,!netbsd,!openbsd,!plan9,!solaris,!windows

// package filelock -- go2cs converted at 2022 March 06 23:17:00 UTC
// import "cmd/go/internal/lockedfile/internal/filelock" ==> using filelock = go.cmd.go.@internal.lockedfile.@internal.filelock_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\lockedfile\internal\filelock\filelock_other.go
using fs = go.io.fs_package;

namespace go.cmd.go.@internal.lockedfile.@internal;

public static partial class filelock_package {

private partial struct lockType { // : sbyte
}

private static readonly var readLock = iota + 1;
private static readonly var writeLock = 0;


private static error @lock(File f, lockType lt) {
    return error.As(addr(new fs.PathError(Op:lt.String(),Path:f.Name(),Err:ErrNotSupported,))!)!;
}

private static error unlock(File f) {
    return error.As(addr(new fs.PathError(Op:"Unlock",Path:f.Name(),Err:ErrNotSupported,))!)!;
}

private static bool isNotSupported(error err) {
    return err == ErrNotSupported;
}

} // end filelock_package
