// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytes -- go2cs converted at 2020 August 29 08:22:21 UTC
// import "bytes" ==> using bytes = go.bytes_package
// Original source: C:\Go\src\bytes\bytes_s390x.go

using static go.builtin;

namespace go
{
    public static partial class bytes_package
    {
        //go:noescape

        // indexShortStr returns the index of the first instance of sep in s,
        // or -1 if sep is not present in s.
        // indexShortStr requires 2 <= len(sep) <= shortStringLen
        private static long indexShortStr(slice<byte> s, slice<byte> c)
; // ../runtime/asm_s390x.s

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
                {>>MARKER:FUNCTION_indexShortStr_BLOCK_PREFIX<<
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
            return countGeneric(s, sep);
        }
    }
}
