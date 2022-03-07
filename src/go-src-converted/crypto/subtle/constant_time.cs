// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package subtle implements functions that are often useful in cryptographic
// code but require careful thought to use correctly.
// package subtle -- go2cs converted at 2022 March 06 22:17:16 UTC
// import "crypto/subtle" ==> using subtle = go.crypto.subtle_package
// Original source: C:\Program Files\Go\src\crypto\subtle\constant_time.go


namespace go.crypto;

public static partial class subtle_package {

    // ConstantTimeCompare returns 1 if the two slices, x and y, have equal contents
    // and 0 otherwise. The time taken is a function of the length of the slices and
    // is independent of the contents.
public static nint ConstantTimeCompare(slice<byte> x, slice<byte> y) {
    if (len(x) != len(y)) {
        return 0;
    }
    byte v = default;

    for (nint i = 0; i < len(x); i++) {
        v |= x[i] ^ y[i];
    }

    return ConstantTimeByteEq(v, 0);

}

// ConstantTimeSelect returns x if v == 1 and y if v == 0.
// Its behavior is undefined if v takes any other value.
public static nint ConstantTimeSelect(nint v, nint x, nint y) {
    return ~(v - 1) & x | (v - 1) & y;
}

// ConstantTimeByteEq returns 1 if x == y and 0 otherwise.
public static nint ConstantTimeByteEq(byte x, byte y) {
    return int((uint32(x ^ y) - 1) >> 31);
}

// ConstantTimeEq returns 1 if x == y and 0 otherwise.
public static nint ConstantTimeEq(int x, int y) {
    return int((uint64(uint32(x ^ y)) - 1) >> 63);
}

// ConstantTimeCopy copies the contents of y into x (a slice of equal length)
// if v == 1. If v == 0, x is left unchanged. Its behavior is undefined if v
// takes any other value.
public static void ConstantTimeCopy(nint v, slice<byte> x, slice<byte> y) => func((_, panic, _) => {
    if (len(x) != len(y)) {
        panic("subtle: slices have different lengths");
    }
    var xmask = byte(v - 1);
    var ymask = byte(~(v - 1));
    for (nint i = 0; i < len(x); i++) {
        x[i] = x[i] & xmask | y[i] & ymask;
    }

});

// ConstantTimeLessOrEq returns 1 if x <= y and 0 otherwise.
// Its behavior is undefined if x or y are negative or > 2**31 - 1.
public static nint ConstantTimeLessOrEq(nint x, nint y) {
    var x32 = int32(x);
    var y32 = int32(y);
    return int(((x32 - y32 - 1) >> 31) & 1);
}

} // end subtle_package
