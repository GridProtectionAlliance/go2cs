// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytealg -- go2cs converted at 2022 March 06 22:30:04 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\index_amd64.go
using cpu = go.@internal.cpu_package;

namespace go.@internal;

public static partial class bytealg_package {

public static readonly nint MaxBruteForce = 64;



private static void init() {
    if (cpu.X86.HasAVX2) {
        MaxLen = 63;
    }
    else
 {
        MaxLen = 31;
    }
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
