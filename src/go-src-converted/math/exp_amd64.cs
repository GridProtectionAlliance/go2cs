// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64
// +build amd64

// package math -- go2cs converted at 2022 March 06 22:31:04 UTC
// import "math" ==> using math = go.math_package
// Original source: C:\Program Files\Go\src\math\exp_amd64.go
using cpu = go.@internal.cpu_package;

namespace go;

public static partial class math_package {

private static var useFMA = cpu.X86.HasAVX && cpu.X86.HasFMA;

} // end math_package
