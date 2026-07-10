// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build 386 || amd64 || arm64 || ppc64 || ppc64le || s390x || wasm
namespace go;

partial class math_package {

internal const bool haveArchFloor = true;

internal static partial float64 archFloor(float64 x);

internal const bool haveArchCeil = true;

internal static partial float64 archCeil(float64 x);

internal const bool haveArchTrunc = true;

internal static partial float64 archTrunc(float64 x);

} // end math_package
