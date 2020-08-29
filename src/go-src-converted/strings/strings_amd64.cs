// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package strings -- go2cs converted at 2020 August 29 08:42:46 UTC
// import "strings" ==> using strings = go.strings_package
// Original source: C:\Go\src\strings\strings_amd64.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go
{
    public static partial class strings_package
    {
        //go:noescape

        // indexShortStr returns the index of the first instance of c in s, or -1 if c is not present in s.
        // indexShortStr requires 2 <= len(c) <= shortStringLen
        private static long indexShortStr(@string s, @string c)
; // ../runtime/asm_amd64.s
        private static long countByte(@string s, byte c)
; // ../runtime/asm_amd64.s

        private static long shortStringLen = default;

        private static void init()
        {
            if (cpu.X86.HasAVX2)
            {>>MARKER:FUNCTION_countByte_BLOCK_PREFIX<<
                shortStringLen = 63L;
            }
            else
            {>>MARKER:FUNCTION_indexShortStr_BLOCK_PREFIX<<
                shortStringLen = 31L;
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
                {
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
            if (len(substr) == 1L && cpu.X86.HasPOPCNT)
            {
                return countByte(s, byte(substr[0L]));
            }
            return countGeneric(s, substr);
        }
    }
}
