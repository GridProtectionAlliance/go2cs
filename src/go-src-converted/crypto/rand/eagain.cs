// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package rand -- go2cs converted at 2022 March 13 05:30:38 UTC
// import "crypto/rand" ==> using rand = go.crypto.rand_package
// Original source: C:\Program Files\Go\src\crypto\rand\eagain.go
namespace go.crypto;

using fs = io.fs_package;
using syscall = syscall_package;

public static partial class rand_package {

private static void init() {
    isEAGAIN = unixIsEAGAIN;
}

// unixIsEAGAIN reports whether err is a syscall.EAGAIN wrapped in a PathError.
// See golang.org/issue/9205
private static bool unixIsEAGAIN(error err) {
    {
        ptr<fs.PathError> (pe, ok) = err._<ptr<fs.PathError>>();

        if (ok) {
            {
                syscall.Errno (errno, ok) = pe.Err._<syscall.Errno>();

                if (ok && errno == syscall.EAGAIN) {
                    return true;
                }

            }
        }
    }
    return false;
}

} // end rand_package
