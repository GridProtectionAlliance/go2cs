// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !math_big_pure_go
// +build !math_big_pure_go

// package big -- go2cs converted at 2022 March 06 22:17:37 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\arith_amd64.go
using cpu = go.@internal.cpu_package;

namespace go.math;

public static partial class big_package {

private static var support_adx = cpu.X86.HasADX && cpu.X86.HasBMI2;

} // end big_package
