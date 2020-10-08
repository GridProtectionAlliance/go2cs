// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build amd64 arm64 s390x

// package bytealg -- go2cs converted at 2020 October 08 03:19:43 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Go\src\internal\bytealg\index_native.go

using static go.builtin;

namespace go {
namespace @internal
{
    public static partial class bytealg_package
    {
        //go:noescape

        // Index returns the index of the first instance of b in a, or -1 if b is not present in a.
        // Requires 2 <= len(b) <= MaxLen.
        public static long Index(slice<byte> a, slice<byte> b)
;

        //go:noescape

        // IndexString returns the index of the first instance of b in a, or -1 if b is not present in a.
        // Requires 2 <= len(b) <= MaxLen.
        public static long IndexString(@string a, @string b)
;
    }
}}
