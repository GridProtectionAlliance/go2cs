// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !386,!amd64,!s390x,!arm,!arm64,!ppc64,!ppc64le,!mips,!mipsle,!mips64,!mips64le,!riscv64,!wasm

// package bytealg -- go2cs converted at 2020 October 08 03:19:43 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Go\src\internal\bytealg\indexbyte_generic.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class bytealg_package
    {
        public static long IndexByte(slice<byte> b, byte c)
        {
            foreach (var (i, x) in b)
            {
                if (x == c)
                {
                    return i;
                }
            }            return -1L;

        }

        public static long IndexByteString(@string s, byte c)
        {
            for (long i = 0L; i < len(s); i++)
            {
                if (s[i] == c)
                {
                    return i;
                }

            }

            return -1L;

        }
    }
}}
