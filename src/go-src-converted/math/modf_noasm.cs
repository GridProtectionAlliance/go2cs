// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !arm64 && !ppc64 && !ppc64le
// +build !arm64,!ppc64,!ppc64le

// package math -- go2cs converted at 2022 March 13 05:42:03 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\modf_noasm.go
namespace go;

public static partial class math_package {

private static readonly var haveArchModf = false;



private static (double, double) archModf(double f) => func((_, panic, _) => {
    double @int = default;
    double frac = default;

    panic("not implemented");
});

} // end math_package
