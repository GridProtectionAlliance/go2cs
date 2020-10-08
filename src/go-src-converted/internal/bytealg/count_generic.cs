// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !amd64,!arm,!arm64,!ppc64le,!ppc64,!s390x

// package bytealg -- go2cs converted at 2020 October 08 03:19:42 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Go\src\internal\bytealg\count_generic.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class bytealg_package
    {
        public static long Count(slice<byte> b, byte c)
        {
            long n = 0L;
            foreach (var (_, x) in b)
            {
                if (x == c)
                {
                    n++;
                }
            }            return n;

        }

        public static long CountString(@string s, byte c)
        {
            long n = 0L;
            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] == c)
                {
                    n++;
                }

            }

            return n;

        }
    }
}}
