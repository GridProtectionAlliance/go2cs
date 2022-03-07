// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build math_big_pure_go
// +build math_big_pure_go

// package big -- go2cs converted at 2022 March 06 22:17:37 UTC
// import "math/big" ==> using big = go.math.big_package
// Original source: C:\Program Files\Go\src\math\big\arith_decl_pure.go


namespace go.math;

public static partial class big_package {

private static (Word, Word) mulWW(Word x, Word y) {
    Word z1 = default;
    Word z0 = default;

    return mulWW_g(x, y);
}

private static Word addVV(slice<Word> z, slice<Word> x, slice<Word> y) {
    Word c = default;

    return addVV_g(z, x, y);
}

private static Word subVV(slice<Word> z, slice<Word> x, slice<Word> y) {
    Word c = default;

    return subVV_g(z, x, y);
}

private static Word addVW(slice<Word> z, slice<Word> x, Word y) {
    Word c = default;
 
    // TODO: remove indirect function call when golang.org/issue/30548 is fixed
    var fn = addVW_g;
    if (len(z) > 32) {
        fn = addVWlarge;
    }
    return fn(z, x, y);

}

private static Word subVW(slice<Word> z, slice<Word> x, Word y) {
    Word c = default;
 
    // TODO: remove indirect function call when golang.org/issue/30548 is fixed
    var fn = subVW_g;
    if (len(z) > 32) {
        fn = subVWlarge;
    }
    return fn(z, x, y);

}

private static Word shlVU(slice<Word> z, slice<Word> x, nuint s) {
    Word c = default;

    return shlVU_g(z, x, s);
}

private static Word shrVU(slice<Word> z, slice<Word> x, nuint s) {
    Word c = default;

    return shrVU_g(z, x, s);
}

private static Word mulAddVWW(slice<Word> z, slice<Word> x, Word y, Word r) {
    Word c = default;

    return mulAddVWW_g(z, x, y, r);
}

private static Word addMulVVW(slice<Word> z, slice<Word> x, Word y) {
    Word c = default;

    return addMulVVW_g(z, x, y);
}

} // end big_package
