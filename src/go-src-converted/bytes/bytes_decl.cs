// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytes -- go2cs converted at 2020 August 29 08:22:21 UTC
// import "bytes" ==> using bytes = go.bytes_package
// Original source: C:\Go\src\bytes\bytes_decl.go

using static go.builtin;

namespace go
{
    public static partial class bytes_package
    {
        //go:noescape

        // IndexByte returns the index of the first instance of c in s, or -1 if c is not present in s.
        public static long IndexByte(slice<byte> s, byte c)
; // ../runtime/asm_$GOARCH.s

        //go:noescape

        // Equal returns a boolean reporting whether a and b
        // are the same length and contain the same bytes.
        // A nil argument is equivalent to an empty slice.
        public static bool Equal(slice<byte> a, slice<byte> b)
; // ../runtime/asm_$GOARCH.s

        //go:noescape

        // Compare returns an integer comparing two byte slices lexicographically.
        // The result will be 0 if a==b, -1 if a < b, and +1 if a > b.
        // A nil argument is equivalent to an empty slice.
        public static long Compare(slice<byte> a, slice<byte> b)
; // ../runtime/noasm.go or ../runtime/asm_{386,amd64}.s
    }
}
