// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build 386 amd64 s390x arm arm64 ppc64 ppc64le mips mipsle mips64 mips64le riscv64 wasm

// package bytealg -- go2cs converted at 2020 October 09 04:45:56 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Go\src\internal\bytealg\indexbyte_native.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class bytealg_package
    {
        //go:noescape
        public static long IndexByte(slice<byte> b, byte c)
;

        //go:noescape
        public static long IndexByteString(@string s, byte c)
;
    }
}}
