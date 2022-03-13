// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build 386 || amd64 || arm64 || ppc64 || ppc64le || s390x || wasm
// +build 386 amd64 arm64 ppc64 ppc64le s390x wasm

// package math -- go2cs converted at 2022 March 13 05:41:57 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\floor_asm.go
namespace go;

public static partial class math_package {

private static readonly var haveArchFloor = true;



private static double archFloor(double x);

private static readonly var haveArchCeil = true;



private static double archCeil(double x);

private static readonly var haveArchTrunc = true;



private static double archTrunc(double x);

} // end math_package
