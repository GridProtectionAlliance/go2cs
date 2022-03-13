// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package flate -- go2cs converted at 2022 March 13 05:29:15 UTC
// import "compress/flate" ==> using flate = go.compress.flate_package
// Original source: C:\Program Files\Go\src\compress\flate\token.go
namespace go.compress;

public static partial class flate_package {

 
// 2 bits:   type   0 = literal  1=EOF  2=Match   3=Unused
// 8 bits:   xlength = length - MIN_MATCH_LENGTH
// 22 bits   xoffset = offset - MIN_OFFSET_SIZE, or literal
private static readonly nint lengthShift = 22;
private static readonly nint offsetMask = 1 << (int)(lengthShift) - 1;
private static readonly nint typeMask = 3 << 30;
private static readonly nint literalType = 0 << 30;
private static readonly nint matchType = 1 << 30;

// The length code for length X (MIN_MATCH_LENGTH <= X <= MAX_MATCH_LENGTH)
// is lengthCodes[length - MIN_MATCH_LENGTH]
private static array<uint> lengthCodes = new array<uint>(new uint[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 8, 9, 9, 10, 10, 11, 11, 12, 12, 12, 12, 13, 13, 13, 13, 14, 14, 14, 14, 15, 15, 15, 15, 16, 16, 16, 16, 16, 16, 16, 16, 17, 17, 17, 17, 17, 17, 17, 17, 18, 18, 18, 18, 18, 18, 18, 18, 19, 19, 19, 19, 19, 19, 19, 19, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28 });

private static array<uint> offsetCodes = new array<uint>(new uint[] { 0, 1, 2, 3, 4, 4, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 11, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15 });

private partial struct token { // : uint
}

// Convert a literal into a literal token.
private static token literalToken(uint literal) {
    return token(literalType + literal);
}

// Convert a < xlength, xoffset > pair into a match token.
private static token matchToken(uint xlength, uint xoffset) {
    return token(matchType + xlength << (int)(lengthShift) + xoffset);
}

// Returns the literal of a literal token
private static uint literal(this token t) {
    return uint32(t - literalType);
}

// Returns the extra offset of a match token
private static uint offset(this token t) {
    return uint32(t) & offsetMask;
}

private static uint length(this token t) {
    return uint32((t - matchType) >> (int)(lengthShift));
}

private static uint lengthCode(uint len) {
    return lengthCodes[len];
}

// Returns the offset code corresponding to a specific offset
private static uint offsetCode(uint off) {
    if (off < uint32(len(offsetCodes))) {
        return offsetCodes[off];
    }
    if (off >> 7 < uint32(len(offsetCodes))) {
        return offsetCodes[off >> 7] + 14;
    }
    return offsetCodes[off >> 14] + 28;
}

} // end flate_package
