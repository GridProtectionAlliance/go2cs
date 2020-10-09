// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 October 09 05:01:31 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\sockcmsg_dragonfly.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // Round the length of a raw sockaddr up to align it properly.
        private static long cmsgAlignOf(long salen)
        {
            var salign = sizeofPtr;
            if (sizeofPtr == 8L && !supportsABI(_dragonflyABIChangeVersion))
            { 
                // 64-bit Dragonfly before the September 2019 ABI changes still requires
                // 32-bit aligned access to network subsystem.
                salign = 4L;

            }
            return (salen + salign - 1L) & ~(salign - 1L);

        }
    }
}
