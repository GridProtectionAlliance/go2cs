// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 || arm64 || riscv64 || s390x
namespace go;

partial class math_package {

internal const bool haveArchMax = true;

internal static partial float64 archMax(float64 x, float64 y);

internal const bool haveArchMin = true;

internal static partial float64 archMin(float64 x, float64 y);

} // end math_package
