// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !linux

// package main -- go2cs converted at 2022 March 13 05:29:27 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\syscalls_none.go
namespace go;

public static partial class main_package {

private static nint gettid() {
    return 0;
}

private static (bool, bool) tidExists(nint tid) {
    bool exists = default;
    bool supported = default;

    return (false, false);
}

private static (@string, error) getcwd() {
    @string _p0 = default;
    error _p0 = default!;

    return ("", error.As(null!)!);
}

private static error unshareFs() {
    return error.As(null!)!;
}

private static error chdir(@string path) {
    return error.As(null!)!;
}

} // end main_package
