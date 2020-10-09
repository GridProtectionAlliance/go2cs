// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syscall -- go2cs converted at 2020 October 09 05:01:33 UTC
// import "syscall" ==> using syscall = go.syscall_package
// Original source: C:\Go\src\syscall\str.go

using static go.builtin;

namespace go
{
    public static partial class syscall_package
    {
        private static @string itoa(long val)
        { // do it here rather than with fmt to avoid dependency
            if (val < 0L)
            {
                return "-" + uitoa(uint(-val));
            }
            return uitoa(uint(val));

        }

        private static @string uitoa(ulong val)
        {
            array<byte> buf = new array<byte>(32L); // big enough for int64
            var i = len(buf) - 1L;
            while (val >= 10L)
            {
                buf[i] = byte(val % 10L + '0');
                i--;
                val /= 10L;
            }

            buf[i] = byte(val + '0');
            return string(buf[i..]);

        }
    }
}
