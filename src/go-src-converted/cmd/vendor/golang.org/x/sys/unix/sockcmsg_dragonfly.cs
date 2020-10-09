// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package unix -- go2cs converted at 2020 October 09 05:56:19 UTC
// import "cmd/vendor/golang.org/x/sys/unix" ==> using unix = go.cmd.vendor.golang.org.x.sys.unix_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\sys\unix\sockcmsg_dragonfly.go

using static go.builtin;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace sys
{
    public static partial class unix_package
    {
        // Round the length of a raw sockaddr up to align it properly.
        private static long cmsgAlignOf(long salen)
        {
            var salign = SizeofPtr;
            if (SizeofPtr == 8L && !supportsABI(_dragonflyABIChangeVersion))
            { 
                // 64-bit Dragonfly before the September 2019 ABI changes still requires
                // 32-bit aligned access to network subsystem.
                salign = 4L;

            }
            return (salen + salign - 1L) & ~(salign - 1L);

        }
    }
}}}}}}
