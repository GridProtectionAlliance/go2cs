// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!s390x

// package strings -- go2cs converted at 2020 August 29 08:42:46 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Go\src\strings\strings_generic.go

using static go.builtin;

namespace go
{
    public static partial class strings_package
    {
        // TODO: implements short string optimization on non amd64 platforms
        // and get rid of strings_amd64.go

        // Index returns the index of the first instance of substr in s, or -1 if substr is not present in s.
        public static long Index(@string s, @string substr)
        {
            var n = len(substr);

            if (n == 0L) 
                return 0L;
            else if (n == 1L) 
                return IndexByte(s, substr[0L]);
            else if (n == len(s)) 
                if (substr == s)
                {
                    return 0L;
                }
                return -1L;
            else if (n > len(s)) 
                return -1L;
                        var c = substr[0L];
            long i = 0L;
            var t = s[..len(s) - n + 1L];
            long fails = 0L;
            while (i < len(t))
            {
                if (t[i] != c)
                {
                    var o = IndexByte(t[i..], c);
                    if (o < 0L)
                    {
                        return -1L;
                    }
                    i += o;
                }
                if (s[i..i + n] == substr)
                {
                    return i;
                }
                i++;
                fails++;
                if (fails >= 4L + i >> (int)(4L) && i < len(t))
                { 
                    // See comment in ../bytes/bytes_generic.go.
                    var j = indexRabinKarp(s[i..], substr);
                    if (j < 0L)
                    {
                        return -1L;
                    }
                    return i + j;
                }
            }
            return -1L;
        }

        // Count counts the number of non-overlapping instances of substr in s.
        // If substr is an empty string, Count returns 1 + the number of Unicode code points in s.
        public static long Count(@string s, @string substr)
        {
            return countGeneric(s, substr);
        }
    }
}
