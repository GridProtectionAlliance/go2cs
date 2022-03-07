// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64 || arm64 || riscv64 || s390x
// +build amd64 arm64 riscv64 s390x

// package math -- go2cs converted at 2022 March 06 22:31:03 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\dim_asm.go


namespace go;

public static partial class math_package {

private static readonly var haveArchMax = true;



private static double archMax(double x, double y);

private static readonly var haveArchMin = true;



private static double archMin(double x, double y);

} // end math_package
