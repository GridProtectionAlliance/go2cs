// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !math_big_pure_go
// +build !math_big_pure_go

// package big -- go2cs converted at 2022 March 13 05:31:44 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\arith_decl.go
namespace go.math;

public static partial class big_package {

// implemented in arith_$GOARCH.s
private static (Word, Word) mulWW(Word x, Word y);
private static Word addVV(slice<Word> z, slice<Word> x, slice<Word> y);
private static Word subVV(slice<Word> z, slice<Word> x, slice<Word> y);
private static Word addVW(slice<Word> z, slice<Word> x, Word y);
private static Word subVW(slice<Word> z, slice<Word> x, Word y);
private static Word shlVU(slice<Word> z, slice<Word> x, nuint s);
private static Word shrVU(slice<Word> z, slice<Word> x, nuint s);
private static Word mulAddVWW(slice<Word> z, slice<Word> x, Word y, Word r);
private static Word addMulVVW(slice<Word> z, slice<Word> x, Word y);

} // end big_package
