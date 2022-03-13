// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || freebsd || linux || netbsd || openbsd || solaris || zos
// +build aix darwin freebsd linux netbsd openbsd solaris zos

// package unix -- go2cs converted at 2022 March 13 06:41:19 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\sockcmsg_unix_other.go
namespace go.cmd.vendor.golang.org.x.sys;

using runtime = runtime_package;


// Round the length of a raw sockaddr up to align it properly.

public static partial class unix_package {

private static nint cmsgAlignOf(nint salen) {
    var salign = SizeofPtr; 

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
            if (SizeofPtr == 8) {
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
        case "zos": 
            // z/OS socket macros use [32-bit] sizeof(int) alignment,
            // not pointer width.
            salign = SizeofInt;
            break;
    }

    return (salen + salign - 1) & ~(salign - 1);
}

} // end unix_package
