// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 || arm64 || s390x || ppc64le || ppc64
// +build amd64 arm64 s390x ppc64le ppc64

// package bytealg -- go2cs converted at 2022 March 13 05:40:52 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\index_native.go
namespace go.@internal;

public static partial class bytealg_package {

//go:noescape

// Index returns the index of the first instance of b in a, or -1 if b is not present in a.
// Requires 2 <= len(b) <= MaxLen.
public static nint Index(slice<byte> a, slice<byte> b);

//go:noescape

// IndexString returns the index of the first instance of b in a, or -1 if b is not present in a.
// Requires 2 <= len(b) <= MaxLen.
public static nint IndexString(@string a, @string b);

} // end bytealg_package
