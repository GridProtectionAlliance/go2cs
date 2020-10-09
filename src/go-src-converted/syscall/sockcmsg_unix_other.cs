// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix darwin freebsd linux netbsd openbsd solaris

// package syscall -- go2cs converted at 2020 October 09 05:01:32 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\sockcmsg_unix_other.go
using runtime = go.runtime_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        // Round the length of a raw sockaddr up to align it properly.
        private static long cmsgAlignOf(long salen)
        {
            var salign = sizeofPtr; 

            // dragonfly needs to check ABI version at runtime, see cmsgAlignOf in
            // sockcmsg_dragonfly.go
            switch (runtime.GOOS)
            {
                case "aix": 
                    // There is no alignment on AIX.
                    salign = 1L;
                    break;
                case "darwin": 
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
                    if (sizeofPtr == 8L)
                    {
                        salign = 4L;
                    }
                    break;
                case "netbsd": 
                    // NetBSD and OpenBSD armv7 require 64-bit alignment.

                case "openbsd": 
                    // NetBSD and OpenBSD armv7 require 64-bit alignment.
                    if (runtime.GOARCH == "arm")
                    {
                        salign = 8L;
                    }
                    break;
            }

            return (salen + salign - 1L) & ~(salign - 1L);

        }
    }
}
