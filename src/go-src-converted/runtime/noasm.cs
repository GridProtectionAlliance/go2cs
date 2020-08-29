// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Routines that are implemented in assembly in asm_{amd64,386,arm,arm64,ppc64x,s390x}.s
// These routines have corresponding stubs in stubs_asm.go.

// +build mips64 mips64le

// package runtime -- go2cs converted at 2020 August 29 08:18:40 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\noasm.go
using _@unsafe_ = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    { // for go:linkname
        private static long cmpstring(@string s1, @string s2)
        {
            var l = len(s1);
            if (len(s2) < l)
            {
                l = len(s2);
            }
            for (long i = 0L; i < l; i++)
            {
                var c1 = s1[i];
                var c2 = s2[i];
                if (c1 < c2)
                {
                    return -1L;
                }
                if (c1 > c2)
                {
                    return +1L;
                }
            }
            if (len(s1) < len(s2))
            {
                return -1L;
            }
            if (len(s1) > len(s2))
            {
                return +1L;
            }
            return 0L;
        }

        //go:linkname bytes_Compare bytes.Compare
        private static long bytes_Compare(slice<byte> s1, slice<byte> s2)
        {
            var l = len(s1);
            if (len(s2) < l)
            {
                l = len(s2);
            }
            if (l == 0L || ref s1[0L] == ref s2[0L])
            {
                goto samebytes;
            }
            for (long i = 0L; i < l; i++)
            {
                var c1 = s1[i];
                var c2 = s2[i];
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
            if (len(s1) < len(s2))
            {
                return -1L;
            }
            if (len(s1) > len(s2))
            {
                return +1L;
            }
            return 0L;
        }
    }
}
