// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build faketime

// package syscall -- go2cs converted at 2020 October 09 05:02:03 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\time_fake.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static readonly var faketime = true;

        // When faketime is enabled, we redirect writes to FDs 1 and 2 through
        // the runtime's write function, since that adds the framing that
        // reports the emulated time.

        //go:linkname runtimeWrite runtime.write


        // When faketime is enabled, we redirect writes to FDs 1 and 2 through
        // the runtime's write function, since that adds the framing that
        // reports the emulated time.

        //go:linkname runtimeWrite runtime.write
        private static int runtimeWrite(System.UIntPtr fd, unsafe.Pointer p, int n)
;

        private static long faketimeWrite(long fd, slice<byte> p)
        {
            ptr<byte> pp;
            if (len(p) > 0L)
            {>>MARKER:FUNCTION_runtimeWrite_BLOCK_PREFIX<<
                pp = _addr_p[0L];
            }

            return int(runtimeWrite(uintptr(fd), @unsafe.Pointer(pp), int32(len(p))));

        }
    }
}
