// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!arm64,!s390x

// package bytealg -- go2cs converted at 2020 October 08 03:19:43 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Go\src\internal\bytealg\index_generic.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class bytealg_package
    {
        public static readonly long MaxBruteForce = (long)0L;

        // Index returns the index of the first instance of b in a, or -1 if b is not present in a.
        // Requires 2 <= len(b) <= MaxLen.


        // Index returns the index of the first instance of b in a, or -1 if b is not present in a.
        // Requires 2 <= len(b) <= MaxLen.
        public static long Index(slice<byte> a, slice<byte> b) => func((_, panic, __) =>
        {
            panic("unimplemented");
        });

        // IndexString returns the index of the first instance of b in a, or -1 if b is not present in a.
        // Requires 2 <= len(b) <= MaxLen.
        public static long IndexString(@string s, @string substr)
        { 
            // This is a partial copy of strings.Index, here because bytes.IndexAny and bytes.LastIndexAny
            // call bytealg.IndexString. Some platforms have an optimized assembly version of this function.
            // This implementation is used for those that do not. Although the pure Go implementation here
            // works for the case of len(b) > MaxLen, we do not require that its assembly implementation also
            // supports the case of len(b) > MaxLen. And we do not guarantee that this function supports the
            // case of len(b) > MaxLen.
            var n = len(substr);
            var c0 = substr[0L];
            var c1 = substr[1L];
            long i = 0L;
            var t = len(s) - n + 1L;
            long fails = 0L;
            while (i < t)
            {
                if (s[i] != c0)
                {
                    var o = IndexByteString(s[i..t], c0);
                    if (o < 0L)
                    {
                        return -1L;
                    }

                    i += o;

                }

                if (s[i + 1L] == c1 && s[i..i + n] == substr)
                {
                    return i;
                }

                i++;
                fails++;
                if (fails >= 4L + i >> (int)(4L) && i < t)
                { 
                    // See comment in src/bytes/bytes.go.
                    var j = IndexRabinKarp(s[i..], substr);
                    if (j < 0L)
                    {
                        return -1L;
                    }

                    return i + j;

                }

            }

            return -1L;

        }

        // Cutover reports the number of failures of IndexByte we should tolerate
        // before switching over to Index.
        // n is the number of bytes processed so far.
        // See the bytes.Index implementation for details.
        public static long Cutover(long n) => func((_, panic, __) =>
        {
            panic("unimplemented");
        });
    }
}}
