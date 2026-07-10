// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !arm64 && !ppc64 && !ppc64le
namespace go;

partial class math_package {

internal const bool haveArchModf = false;

internal static (float64 @int, float64 frac) archModf(float64 f) {
    float64 @int = default!;
    float64 frac = default!;

    throw panic("not implemented");
}

} // end math_package
