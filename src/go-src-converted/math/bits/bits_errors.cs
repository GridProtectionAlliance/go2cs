// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !compiler_bootstrap
// +build !compiler_bootstrap

// package bits -- go2cs converted at 2022 March 13 05:29:04 UTC
// import "math/bits" ==> using bits = go.math.bits_package
// Original source: C:\Program Files\Go\src\math\bits\bits_errors.go
namespace go.math;

using _@unsafe_ = @unsafe_package;

public static partial class bits_package {

//go:linkname overflowError runtime.overflowError
private static error overflowError = default!;

//go:linkname divideError runtime.divideError
private static error divideError = default!;

} // end bits_package
