// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!s390x,!arm64

// package bytes -- go2cs converted at 2020 August 29 08:22:21 UTC
// import "bytes" ==> using bytes = go.bytes_package
// Original source: C:\Go\src\bytes\bytes_generic.go

using static go.builtin;

namespace go
{
    public static partial class bytes_package
    {
        // Index returns the index of the first instance of sep in s, or -1 if sep is not present in s.
        public static long Index(slice<byte> s, slice<byte> sep)
        {
            var n = len(sep);

            if (n == 0L) 
                return 0L;
            else if (n == 1L) 
                return IndexByte(s, sep[0L]);
            else if (n == len(s)) 
                if (Equal(sep, s))
                {
                    return 0L;
                }
                return -1L;
            else if (n > len(s)) 
                return -1L;
                        var c = sep[0L];
            long i = 0L;
            long fails = 0L;
            var t = s[..len(s) - n + 1L];
            while (i < len(t))
            {
                if (t[i] != c)
                {
                    var o = IndexByte(t[i..], c);
                    if (o < 0L)
                    {
                        break;
                    }
                    i += o;
                }
                if (Equal(s[i..i + n], sep))
                {
                    return i;
                }
                i++;
                fails++;
                if (fails >= 4L + i >> (int)(4L) && i < len(t))
                { 
                    // Give up on IndexByte, it isn't skipping ahead
                    // far enough to be better than Rabin-Karp.
                    // Experiments (using IndexPeriodic) suggest
                    // the cutover is about 16 byte skips.
                    // TODO: if large prefixes of sep are matching
                    // we should cutover at even larger average skips,
                    // because Equal becomes that much more expensive.
                    // This code does not take that effect into account.
                    var j = indexRabinKarp(s[i..], sep);
                    if (j < 0L)
                    {
                        return -1L;
                    }
                    return i + j;
                }
            }
            return -1L;
        }

        // Count counts the number of non-overlapping instances of sep in s.
        // If sep is an empty slice, Count returns 1 + the number of UTF-8-encoded code points in s.
        public static long Count(slice<byte> s, slice<byte> sep)
        {
            return countGeneric(s, sep);
        }
    }
}
