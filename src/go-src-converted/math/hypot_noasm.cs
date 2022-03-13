// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !386 && !amd64
// +build !386,!amd64

// package math -- go2cs converted at 2022 March 13 05:41:58 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\hypot_noasm.go
namespace go;

public static partial class math_package {

private static readonly var haveArchHypot = false;



private static double archHypot(double p, double q) => func((_, panic, _) => {
    panic("not implemented");
});

} // end math_package
