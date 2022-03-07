// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bytealg -- go2cs converted at 2022 March 06 22:30:04 UTC
// import "internal/bytealg" ==> using bytealg = go.@internal.bytealg_package
// Original source: C:\Program Files\Go\src\internal\bytealg\equal_generic.go


namespace go.@internal;

public static partial class bytealg_package {

    // Equal reports whether a and b
    // are the same length and contain the same bytes.
    // A nil argument is equivalent to an empty slice.
    //
    // Equal is equivalent to bytes.Equal.
    // It is provided here for convenience,
    // because some packages cannot depend on bytes.
public static bool Equal(slice<byte> a, slice<byte> b) { 
    // Neither cmd/compile nor gccgo allocates for these string conversions.
    // There is a test for this in package bytes.
    return string(a) == string(b);

}

} // end bytealg_package
