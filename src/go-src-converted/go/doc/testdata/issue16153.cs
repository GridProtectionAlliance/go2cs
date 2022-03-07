// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue16153 -- go2cs converted at 2022 March 06 22:41:35 UTC
// import "go/doc.issue16153" ==> using issue16153 = go.go.doc.issue16153_package
// Original source: C:\Program Files\Go\src\go\doc\testdata\issue16153.go


namespace go.go;

public static partial class issue16153_package {

    // original test case
private static readonly byte x1 = 255;
public static readonly nint Y1 = 256;


// variations
private static readonly byte x2 = 255;
public static readonly var Y2 = 0;


public static readonly long X3 = iota;
public static readonly nint Y3 = 1;


public static readonly long X4 = iota;
public static readonly var Y4 = 0;


} // end issue16153_package
