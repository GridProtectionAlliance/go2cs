// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Simple conversions to avoid depending on strconv.

// package os -- go2cs converted at 2022 March 06 22:13:52 UTC
// import "os" ==> using os = go.os_package
// Original source: C:\Program Files\Go\src\os\str.go


namespace go;

public static partial class os_package {

    // itox converts val (an int) to a hexdecimal string.
private static @string itox(nint val) {
    if (val < 0) {
        return "-" + uitox(uint(-val));
    }
    return uitox(uint(val));

}

private static readonly @string hex = "0123456789abcdef";

// uitox converts val (a uint) to a hexdecimal string.


// uitox converts val (a uint) to a hexdecimal string.
private static @string uitox(nuint val) {
    if (val == 0) { // avoid string allocation
        return "0x0";

    }
    array<byte> buf = new array<byte>(20); // big enough for 64bit value base 16 + 0x
    var i = len(buf) - 1;
    while (val >= 16) {
        var q = val / 16;
        buf[i] = hex[val % 16];
        i--;
        val = q;
    } 
    // val < 16
    buf[i] = hex[val % 16];
    i--;
    buf[i] = 'x';
    i--;
    buf[i] = '0';
    return string(buf[(int)i..]);

}

} // end os_package
