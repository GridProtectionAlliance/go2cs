// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !386,!amd64,!s390x,!arm,!arm64,!ppc64,!ppc64le,!mips,!mipsle,!wasm,!mips64,!mips64le

// package bytealg -- go2cs converted at 2020 October 08 03:19:42 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Go\src\internal\bytealg\compare_generic.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class bytealg_package
    { // for go:linkname
        public static long Compare(slice<byte> a, slice<byte> b)
        {
            var l = len(a);
            if (len(b) < l)
            {
                l = len(b);
            }
            if (l == 0L || _addr_a[0L] == _addr_b[0L])
            {
                goto samebytes;
            }
            for (long i = 0L; i < l; i++)
            {
                var c1 = a[i];
                var c2 = b[i];
                if (c1 < c2)
                {
                    return -1L;
                }
                if (c1 > c2)
                {
                    return +1L;
                }
            }
samebytes:
            if (len(a) < len(b))
            {
                return -1L;
            }
            if (len(a) > len(b))
            {
                return +1L;
            }
            return 0L;

        }

        //go:linkname runtime_cmpstring runtime.cmpstring
        private static long runtime_cmpstring(@string a, @string b)
        {
            var l = len(a);
            if (len(b) < l)
            {
                l = len(b);
            }

            for (long i = 0L; i < l; i++)
            {
                var c1 = a[i];
                var c2 = b[i];
                if (c1 < c2)
                {
                    return -1L;
                }

                if (c1 > c2)
                {
                    return +1L;
                }

            }

            if (len(a) < len(b))
            {
                return -1L;
            }

            if (len(a) > len(b))
            {
                return +1L;
            }

            return 0L;

        }
    }
}}
