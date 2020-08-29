// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytes -- go2cs converted at 2020 August 29 08:22:20 UTC
// import "bytes" ==> using bytes = go.bytes_package
// Original source: C:\Go\src\bytes\bytes_amd64.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go
{
    public static partial class bytes_package
    {
        //go:noescape

        // indexShortStr returns the index of the first instance of c in s, or -1 if c is not present in s.
        // indexShortStr requires 2 <= len(c) <= shortStringLen
        private static long indexShortStr(slice<byte> s, slice<byte> c)
; // ../runtime/asm_amd64.s
        private static long countByte(slice<byte> s, byte c)
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
            else if (n <= shortStringLen) 
                // Use brute force when s and sep both are small
                if (len(s) <= 64L)
                {
                    return indexShortStr(s, sep);
                }
                var c = sep[0L];
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
                    if (Equal(s[i..i + n], sep))
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
                        var r = indexShortStr(s[i..], sep);
                        if (r >= 0L)
                        {
                            return r + i;
                        }
                        return -1L;
                    }
                }

                return -1L;
                        return indexRabinKarp(s, sep);
        }

        // Count counts the number of non-overlapping instances of sep in s.
        // If sep is an empty slice, Count returns 1 + the number of UTF-8-encoded code points in s.
        public static long Count(slice<byte> s, slice<byte> sep)
        {
            if (len(sep) == 1L && cpu.X86.HasPOPCNT)
            {
                return countByte(s, sep[0L]);
            }
            return countGeneric(s, sep);
        }
    }
}
