// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytealg -- go2cs converted at 2020 October 08 03:19:43 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Go\src\internal\bytealg\index_s390x.go
using cpu = go.@internal.cpu_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class bytealg_package
    {
        public static readonly long MaxBruteForce = (long)64L;



        private static void init()
        { 
            // Note: we're kind of lucky that this flag is available at this point.
            // The runtime sets HasVX when processing auxv records, and that happens
            // to happen *before* running the init functions of packages that
            // the runtime depends on.
            // TODO: it would really be nicer for internal/cpu to figure out this
            // flag by itself. Then we wouldn't need to depend on quirks of
            // early startup initialization order.
            if (cpu.S390X.HasVX)
            {
                MaxLen = 64L;
            }

        }

        // Cutover reports the number of failures of IndexByte we should tolerate
        // before switching over to Index.
        // n is the number of bytes processed so far.
        // See the bytes.Index implementation for details.
        public static long Cutover(long n)
        { 
            // 1 error per 8 characters, plus a few slop to start.
            return (n + 16L) / 8L;

        }
    }
}}
