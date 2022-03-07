// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build windows
// +build windows

// package filelock -- go2cs converted at 2022 March 06 23:17:01 UTC
// import "cmd/go/internal/lockedfile/internal/filelock" ==> using filelock = go.cmd.go.@internal.lockedfile.@internal.filelock_package
// Original source: C:\Program Files\Go\src\cmd\go\internal\lockedfile\internal\filelock\filelock_windows.go
using windows = go.@internal.syscall.windows_package;
using fs = go.io.fs_package;
using syscall = go.syscall_package;

namespace go.cmd.go.@internal.lockedfile.@internal;

public static partial class filelock_package {

private partial struct lockType { // : uint
}

private static readonly lockType readLock = 0;
private static readonly lockType writeLock = windows.LOCKFILE_EXCLUSIVE_LOCK;


private static readonly nint reserved = 0;
private static readonly var allBytes = ~uint32(0);


private static error @lock(File f, lockType lt) { 
    // Per https://golang.org/issue/19098, “Programs currently expect the Fd
    // method to return a handle that uses ordinary synchronous I/O.”
    // However, LockFileEx still requires an OVERLAPPED structure,
    // which contains the file offset of the beginning of the lock range.
    // We want to lock the entire file, so we leave the offset as zero.
    ptr<object> ol = @new<syscall.Overlapped>();

    var err = windows.LockFileEx(syscall.Handle(f.Fd()), uint32(lt), reserved, allBytes, allBytes, ol);
    if (err != null) {
        return error.As(addr(new fs.PathError(Op:lt.String(),Path:f.Name(),Err:err,))!)!;
    }
    return error.As(null!)!;

}

private static error unlock(File f) {
    ptr<object> ol = @new<syscall.Overlapped>();
    var err = windows.UnlockFileEx(syscall.Handle(f.Fd()), reserved, allBytes, allBytes, ol);
    if (err != null) {
        return error.As(addr(new fs.PathError(Op:"Unlock",Path:f.Name(),Err:err,))!)!;
    }
    return error.As(null!)!;

}

private static bool isNotSupported(error err) {

    if (err == windows.ERROR_NOT_SUPPORTED || err == windows.ERROR_CALL_NOT_IMPLEMENTED || err == ErrNotSupported) 
        return true;
    else 
        return false;
    
}

} // end filelock_package
