// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build plan9

// Simple conversions to avoid depending on strconv.

// package poll -- go2cs converted at 2020 October 09 04:51:25 UTC
// import "internal/poll" ==> using poll = go.@internal.poll_package
// Original source: C:\Go\src\internal\poll\strconv.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class poll_package
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

        // stringsHasSuffix is strings.HasSuffix. It reports whether s ends in
        // suffix.
        private static bool stringsHasSuffix(@string s, @string suffix)
        {
            return len(s) >= len(suffix) && s[len(s) - len(suffix)..] == suffix;
        }
    }
}}
