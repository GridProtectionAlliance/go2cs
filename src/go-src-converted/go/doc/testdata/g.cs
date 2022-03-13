// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The package g is a go/doc test for mixed exported/unexported values.

// package g -- go2cs converted at 2022 March 13 05:52:40 UTC
// import "go/doc.g" ==> using g = go.go.doc.g_package
// Original source: C:\Program Files\Go\src\go\doc\testdata\g.go
namespace go.go;

public static partial class g_package {

public static readonly var A = iota;
private static readonly var b = iota;
private static readonly var c = 0;
public static readonly var D = 1;
public static readonly var E = 2;
private static readonly var f = 3;
public static readonly var G = 4;
public static readonly var H = 5;

private static nint c1 = 1;public static nint C2 = 2;private static nint c3 = 3;
public static nint C4 = 4;private static nint c5 = 5;public static nint C6 = 6;
private static nint c7 = 7;public static nint C8 = 8;private static nint c9 = 9;
private static nint xx = 0;private static nint yy = 0;private static nint zz = 0; // all unexported and hidden


} // end g_package
