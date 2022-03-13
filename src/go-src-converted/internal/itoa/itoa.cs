// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Simple conversions to avoid depending on strconv.

// package itoa -- go2cs converted at 2022 March 13 05:27:55 UTC
// import "internal/itoa" ==> using itoa = go.@internal.itoa_package
// Original source: C:\Program Files\Go\src\internal\itoa\itoa.go
namespace go.@internal;

public static partial class itoa_package {

// Itoa converts val to a decimal string.
public static @string Itoa(nint val) {
    if (val < 0) {
        return "-" + Uitoa(uint(-val));
    }
    return Uitoa(uint(val));
}

// Uitoa converts val to a decimal string.
public static @string Uitoa(nuint val) {
    if (val == 0) { // avoid string allocation
        return "0";
    }
    array<byte> buf = new array<byte>(20); // big enough for 64bit value base 10
    var i = len(buf) - 1;
    while (val >= 10) {
        var q = val / 10;
        buf[i] = byte('0' + val - q * 10);
        i--;
        val = q;
    } 
    // val < 10
    buf[i] = byte('0' + val);
    return string(buf[(int)i..]);
}

} // end itoa_package
