// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !arm64
// +build !arm64

// package math -- go2cs converted at 2022 March 13 05:41:56 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\exp2_noasm.go
namespace go;

public static partial class math_package {

private static readonly var haveArchExp2 = false;



private static double archExp2(double x) => func((_, panic, _) => {
    panic("not implemented");
});

} // end math_package
