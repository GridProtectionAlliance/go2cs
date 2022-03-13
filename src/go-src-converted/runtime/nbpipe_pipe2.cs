// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build dragonfly freebsd linux netbsd openbsd solaris

// package runtime -- go2cs converted at 2022 March 13 05:26:01 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\nbpipe_pipe2.go
namespace go;

public static partial class runtime_package {

private static (int, int, int) nonblockingPipe() {
    int r = default;
    int w = default;
    int errno = default;

    r, w, errno = pipe2(_O_NONBLOCK | _O_CLOEXEC);
    if (errno == -_ENOSYS) {
        r, w, errno = pipe();
        if (errno != 0) {
            return (-1, -1, errno);
        }
        closeonexec(r);
        setNonblock(r);
        closeonexec(w);
        setNonblock(w);
    }
    return (r, w, errno);
}

} // end runtime_package
