// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !math_big_pure_go
// +build !math_big_pure_go

// package big -- go2cs converted at 2022 March 06 22:17:37 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\arith_decl_s390x.go
using cpu = go.@internal.cpu_package;

namespace go.math;

public static partial class big_package {

private static Word addVV_check(slice<Word> z, slice<Word> x, slice<Word> y);
private static Word addVV_vec(slice<Word> z, slice<Word> x, slice<Word> y);
private static Word addVV_novec(slice<Word> z, slice<Word> x, slice<Word> y);
private static Word subVV_check(slice<Word> z, slice<Word> x, slice<Word> y);
private static Word subVV_vec(slice<Word> z, slice<Word> x, slice<Word> y);
private static Word subVV_novec(slice<Word> z, slice<Word> x, slice<Word> y);

private static var hasVX = cpu.S390X.HasVX;

} // end big_package
