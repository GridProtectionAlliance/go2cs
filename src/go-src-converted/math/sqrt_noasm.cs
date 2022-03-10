// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !386 && !amd64 && !arm64 && !arm && !mips && !mipsle && !ppc64 && !ppc64le && !s390x && !riscv64 && !wasm
// +build !386,!amd64,!arm64,!arm,!mips,!mipsle,!ppc64,!ppc64le,!s390x,!riscv64,!wasm

// package math -- go2cs converted at 2022 March 06 22:31:11 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\sqrt_noasm.go


namespace go;

public static partial class math_package {

private static readonly var haveArchSqrt = false;



private static double archSqrt(double x) => func((_, panic, _) => {
    panic("not implemented");
});

} // end math_package