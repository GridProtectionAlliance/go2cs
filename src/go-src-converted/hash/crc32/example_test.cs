// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.hash;

using fmt = fmt_package;
using crc32 = go.hash.crc32_package;
using go.hash;

partial class crc32_test_package {

public static void ExampleMakeTable() {
    // In this package, the CRC polynomial is represented in reversed notation,
    // or LSB-first representation.
    //
    // LSB-first representation is a hexadecimal number with n bits, in which the
    // most significant bit represents the coefficient of x⁰ and the least significant
    // bit represents the coefficient of xⁿ⁻¹ (the coefficient for xⁿ is implicit).
    //
    // For example, CRC32-Q, as defined by the following polynomial,
    //	x³²+ x³¹+ x²⁴+ x²²+ x¹⁶+ x¹⁴+ x⁸+ x⁷+ x⁵+ x³+ x¹+ x⁰
    // has the reversed notation 0b11010101100000101000001010000001, so the value
    // that should be passed to MakeTable is 0xD5828281.
    var crc32q = crc32.MakeTable(0xD5828281U);
    fmt.Printf("%08x\n"u8, crc32.Checksum(slice<byte>("Hello world"u8), crc32q));
}

// Output:
// 2964d064

} // end crc32_test_package
