// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:26:08 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\syscalls_linux.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using os = go.os_package;
using syscall = go.syscall_package;

namespace go;

public static partial class main_package {

private static nint gettid() {
    return syscall.Gettid();
}

private static (bool, bool) tidExists(nint tid) {
    bool exists = default;
    bool supported = default;

    var (stat, err) = os.ReadFile(fmt.Sprintf("/proc/self/task/%d/stat", tid));
    if (os.IsNotExist(err)) {
        return (false, true);
    }
    var state = bytes.Fields(stat)[2];
    return (!(len(state) == 1 && state[0] == 'Z'), true);

}

private static (@string, error) getcwd() {
    @string _p0 = default;
    error _p0 = default!;

    if (!syscall.ImplementsGetwd) {
        return ("", error.As(null!)!);
    }
    array<byte> buf = new array<byte>(4096);
    var (n, err) = syscall.Getcwd(buf[..]);
    if (err != null) {
        return ("", error.As(err)!);
    }
    return (string(buf[..(int)n - 1]), error.As(null!)!);

}

private static error unshareFs() {
    var err = syscall.Unshare(syscall.CLONE_FS);
    if (err != null) {
        syscall.Errno (errno, ok) = err._<syscall.Errno>();
        if (ok && errno == syscall.EPERM) {
            return error.As(errNotPermitted)!;
        }
    }
    return error.As(err)!;

}

private static error chdir(@string path) {
    return error.As(syscall.Chdir(path))!;
}

} // end main_package
