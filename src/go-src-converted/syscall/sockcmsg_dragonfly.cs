// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2022 March 06 22:26:46 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Program Files\Go\src\syscall\sockcmsg_dragonfly.go


namespace go;

public static partial class syscall_package {

    // Round the length of a raw sockaddr up to align it properly.
private static nint cmsgAlignOf(nint salen) {
    var salign = sizeofPtr;
    if (sizeofPtr == 8 && !supportsABI(_dragonflyABIChangeVersion)) { 
        // 64-bit Dragonfly before the September 2019 ABI changes still requires
        // 32-bit aligned access to network subsystem.
        salign = 4;

    }
    return (salen + salign - 1) & ~(salign - 1);

}

} // end syscall_package
