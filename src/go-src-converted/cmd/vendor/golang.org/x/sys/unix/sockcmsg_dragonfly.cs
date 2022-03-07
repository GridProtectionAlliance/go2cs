// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2022 March 06 23:26:39 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Program Files\Go\src\cmd\vendor\golang.org\x\sys\unix\sockcmsg_dragonfly.go


namespace go.cmd.vendor.golang.org.x.sys;

public static partial class unix_package {

    // Round the length of a raw sockaddr up to align it properly.
private static nint cmsgAlignOf(nint salen) {
    var salign = SizeofPtr;
    if (SizeofPtr == 8 && !supportsABI(_dragonflyABIChangeVersion)) { 
        // 64-bit Dragonfly before the September 2019 ABI changes still requires
        // 32-bit aligned access to network subsystem.
        salign = 4;

    }
    return (salen + salign - 1) & ~(salign - 1);

}

} // end unix_package
