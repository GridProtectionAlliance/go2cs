// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Simple conversions to avoid depending on strconv.

// package os -- go2cs converted at 2020 October 08 03:45:15 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Go\src\os\str.go

using static go.builtin;

namespace go
{
    public static partial class os_package
    {
        // Convert integer to decimal string
        private static @string itoa(long val)
        {
            if (val < 0L)
            {
                return "-" + uitoa(uint(-val));
            }
            return uitoa(uint(val));

        }

        // Convert unsigned integer to decimal string
        private static @string uitoa(ulong val)
        {
            if (val == 0L)
            { // avoid string allocation
                return "0";

            }

            array<byte> buf = new array<byte>(20L); // big enough for 64bit value base 10
            var i = len(buf) - 1L;
            while (val >= 10L)
            {
                var q = val / 10L;
                buf[i] = byte('0' + val - q * 10L);
                i--;
                val = q;
            } 
            // val < 10
 
            // val < 10
            buf[i] = byte('0' + val);
            return string(buf[i..]);

        }
    }
}
