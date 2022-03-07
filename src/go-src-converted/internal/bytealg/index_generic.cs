// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !amd64 && !arm64 && !s390x && !ppc64le && !ppc64
// +build !amd64,!arm64,!s390x,!ppc64le,!ppc64

// package bytealg -- go2cs converted at 2022 March 06 22:30:04 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\index_generic.go


namespace go.@internal;

public static partial class bytealg_package {

public static readonly nint MaxBruteForce = 0;

// Index returns the index of the first instance of b in a, or -1 if b is not present in a.
// Requires 2 <= len(b) <= MaxLen.


// Index returns the index of the first instance of b in a, or -1 if b is not present in a.
// Requires 2 <= len(b) <= MaxLen.
public static nint Index(slice<byte> a, slice<byte> b) => func((_, panic, _) => {
    panic("unimplemented");
});

// IndexString returns the index of the first instance of b in a, or -1 if b is not present in a.
// Requires 2 <= len(b) <= MaxLen.
public static nint IndexString(@string a, @string b) => func((_, panic, _) => {
    panic("unimplemented");
});

// Cutover reports the number of failures of IndexByte we should tolerate
// before switching over to Index.
// n is the number of bytes processed so far.
// See the bytes.Index implementation for details.
public static nint Cutover(nint n) => func((_, panic, _) => {
    panic("unimplemented");
});

} // end bytealg_package
