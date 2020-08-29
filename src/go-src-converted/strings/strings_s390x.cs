// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strings -- go2cs converted at 2020 August 29 08:42:46 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Go\src\strings\strings_s390x.go

using static go.builtin;

namespace go
{
    public static partial class strings_package
    {
        //go:noescape

        // indexShortStr returns the index of the first instance of sep in s,
        // or -1 if sep is not present in s.
        // indexShortStr requires 2 <= len(sep) <= shortStringLen
        private static long indexShortStr(@string s, @string sep)
; // ../runtime/asm_$GOARCH.s

        // supportsVX reports whether the vector facility is available.
        // indexShortStr must not be called if the vector facility is not
        // available.
        private static bool supportsVX()
; // ../runtime/asm_s390x.s

        private static long shortStringLen = -1L;

        private static void init()
        {
            if (supportsVX())
            {>>MARKER:FUNCTION_supportsVX_BLOCK_PREFIX<<
                shortStringLen = 64L;
            }
        }

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
                {>>MARKER:FUNCTION_indexShortStr_BLOCK_PREFIX<<
                    return 0L;
                }
                return -1L;
            else if (n > len(s)) 
                return -1L;
            else if (n <= shortStringLen) 
                // Use brute force when s and substr both are small
                if (len(s) <= 64L)
                {
                    return indexShortStr(s, substr);
                }
                var c = substr[0L];
                long i = 0L;
                var t = s[..len(s) - n + 1L];
                long fails = 0L;
                while (i < len(t))
                {
                    if (t[i] != c)
                    { 
                        // IndexByte skips 16/32 bytes per iteration,
                        // so it's faster than indexShortStr.
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
                    fails++;
                    i++; 
                    // Switch to indexShortStr when IndexByte produces too many false positives.
                    // Too many means more that 1 error per 8 characters.
                    // Allow some errors in the beginning.
                    if (fails > (i + 16L) / 8L)
                    {
                        var r = indexShortStr(s[i..], substr);
                        if (r >= 0L)
                        {
                            return r + i;
                        }
                        return -1L;
                    }
                }

                return -1L;
                        return indexRabinKarp(s, substr);
        }

        // Count counts the number of non-overlapping instances of substr in s.
        // If substr is an empty string, Count returns 1 + the number of Unicode code points in s.
        public static long Count(@string s, @string substr)
        {
            return countGeneric(s, substr);
        }
    }
}
