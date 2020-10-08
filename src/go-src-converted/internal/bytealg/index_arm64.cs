// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytealg -- go2cs converted at 2020 October 08 03:19:43 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Go\src\internal\bytealg\index_arm64.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class bytealg_package
    {
        // Empirical data shows that using Index can get better
        // performance when len(s) <= 16.
        public static readonly long MaxBruteForce = (long)16L;



        private static void init()
        { 
            // Optimize cases where the length of the substring is less than 32 bytes
            MaxLen = 32L;

        }

        // Cutover reports the number of failures of IndexByte we should tolerate
        // before switching over to Index.
        // n is the number of bytes processed so far.
        // See the bytes.Index implementation for details.
        public static long Cutover(long n)
        { 
            // 1 error per 16 characters, plus a few slop to start.
            return 4L + n >> (int)(4L);

        }
    }
}}
