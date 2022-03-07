// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (aix || linux) && (ppc64 || ppc64le)
// +build aix linux
// +build ppc64 ppc64le

// package bytealg -- go2cs converted at 2022 March 06 22:30:04 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\index_ppc64x.go
using cpu = go.@internal.cpu_package;

namespace go.@internal;

public static partial class bytealg_package {

public static readonly nint MaxBruteForce = 16;



public static var SupportsPower9 = cpu.PPC64.IsPOWER9;

private static void init() {
    MaxLen = 32;
}

// Cutover reports the number of failures of IndexByte we should tolerate
// before switching over to Index.
// n is the number of bytes processed so far.
// See the bytes.Index implementation for details.
public static nint Cutover(nint n) { 
    // 1 error per 8 characters, plus a few slop to start.
    return (n + 16) / 8;

}

} // end bytealg_package
