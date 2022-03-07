// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytealg -- go2cs converted at 2022 March 06 22:30:04 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\index_arm64.go


namespace go.@internal;

public static partial class bytealg_package {

    // Empirical data shows that using Index can get better
    // performance when len(s) <= 16.
public static readonly nint MaxBruteForce = 16;



private static void init() { 
    // Optimize cases where the length of the substring is less than 32 bytes
    MaxLen = 32;

}

// Cutover reports the number of failures of IndexByte we should tolerate
// before switching over to Index.
// n is the number of bytes processed so far.
// See the bytes.Index implementation for details.
public static nint Cutover(nint n) { 
    // 1 error per 16 characters, plus a few slop to start.
    return 4 + n >> 4;

}

} // end bytealg_package
