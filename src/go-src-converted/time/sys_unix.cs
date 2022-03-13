// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || (js && wasm) || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd js,wasm linux netbsd openbsd solaris

// package time -- go2cs converted at 2022 March 13 05:40:58 UTC
// import "time" ==> using time = go.time_package
// Original source: C:\Program Files\Go\src\time\sys_unix.go
namespace go;

using errors = errors_package;
using syscall = syscall_package;


// for testing: whatever interrupts a sleep

public static partial class time_package {

private static void interrupt() {
    syscall.Kill(syscall.Getpid(), syscall.SIGCHLD);
}

private static (System.UIntPtr, error) open(@string name) {
    System.UIntPtr _p0 = default;
    error _p0 = default!;

    var (fd, err) = syscall.Open(name, syscall.O_RDONLY, 0);
    if (err != null) {
        return (0, error.As(err)!);
    }
    return (uintptr(fd), error.As(null!)!);
}

private static (nint, error) read(System.UIntPtr fd, slice<byte> buf) {
    nint _p0 = default;
    error _p0 = default!;

    return syscall.Read(int(fd), buf);
}

private static void closefd(System.UIntPtr fd) {
    syscall.Close(int(fd));
}

private static error preadn(System.UIntPtr fd, slice<byte> buf, nint off) {
    var whence = seekStart;
    if (off < 0) {
        whence = seekEnd;
    }
    {
        var (_, err) = syscall.Seek(int(fd), int64(off), whence);

        if (err != null) {
            return error.As(err)!;
        }
    }
    while (len(buf) > 0) {
        var (m, err) = syscall.Read(int(fd), buf);
        if (m <= 0) {
            if (err == null) {
                return error.As(errors.New("short read"))!;
            }
            return error.As(err)!;
        }
        buf = buf[(int)m..];
    }
    return error.As(null!)!;
}

} // end time_package
