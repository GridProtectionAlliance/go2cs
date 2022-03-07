// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin freebsd linux netbsd openbsd solaris

// package syscall -- go2cs converted at 2022 March 06 22:26:47 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\sockcmsg_unix_other.go
using runtime = go.runtime_package;

namespace go;

public static partial class syscall_package {

    // Round the length of a raw sockaddr up to align it properly.
private static nint cmsgAlignOf(nint salen) {
    var salign = sizeofPtr; 

    // dragonfly needs to check ABI version at runtime, see cmsgAlignOf in
    // sockcmsg_dragonfly.go
    switch (runtime.GOOS) {
        case "aix": 
            // There is no alignment on AIX.
            salign = 1;
            break;
        case "darwin": 
            // NOTE: It seems like 64-bit Darwin, Illumos and Solaris
            // kernels still require 32-bit aligned access to network
            // subsystem.

        case "ios": 
            // NOTE: It seems like 64-bit Darwin, Illumos and Solaris
            // kernels still require 32-bit aligned access to network
            // subsystem.

        case "illumos": 
            // NOTE: It seems like 64-bit Darwin, Illumos and Solaris
            // kernels still require 32-bit aligned access to network
            // subsystem.

        case "solaris": 
            // NOTE: It seems like 64-bit Darwin, Illumos and Solaris
            // kernels still require 32-bit aligned access to network
            // subsystem.
            if (sizeofPtr == 8) {
                salign = 4;
            }
            break;
        case "netbsd": 
            // NetBSD and OpenBSD armv7 require 64-bit alignment.

        case "openbsd": 
            // NetBSD and OpenBSD armv7 require 64-bit alignment.
            if (runtime.GOARCH == "arm") {
                salign = 8;
            }
            if (runtime.GOOS == "netbsd" && runtime.GOARCH == "arm64") {
                salign = 16;
            }
            break;
    }

    return (salen + salign - 1) & ~(salign - 1);

}

} // end syscall_package
