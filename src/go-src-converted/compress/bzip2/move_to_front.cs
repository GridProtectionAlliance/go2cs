// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package bzip2 -- go2cs converted at 2022 March 13 06:43:20 UTC
// import "compress/bzip2" ==> using bzip2 = go.compress.bzip2_package
// Original source: C:\Program Files\Go\src\compress\bzip2\move_to_front.go
namespace go.compress;

public static partial class bzip2_package {

// moveToFrontDecoder implements a move-to-front list. Such a list is an
// efficient way to transform a string with repeating elements into one with
// many small valued numbers, which is suitable for entropy encoding. It works
// by starting with an initial list of symbols and references symbols by their
// index into that list. When a symbol is referenced, it's moved to the front
// of the list. Thus, a repeated symbol ends up being encoded with many zeros,
// as the symbol will be at the front of the list after the first access.
private partial struct moveToFrontDecoder { // : slice<byte>
}

// newMTFDecoder creates a move-to-front decoder with an explicit initial list
// of symbols.
private static moveToFrontDecoder newMTFDecoder(slice<byte> symbols) => func((_, panic, _) => {
    if (len(symbols) > 256) {
        panic("too many symbols");
    }
    return moveToFrontDecoder(symbols);
});

// newMTFDecoderWithRange creates a move-to-front decoder with an initial
// symbol list of 0...n-1.
private static moveToFrontDecoder newMTFDecoderWithRange(nint n) => func((_, panic, _) => {
    if (n > 256) {
        panic("newMTFDecoderWithRange: cannot have > 256 symbols");
    }
    var m = make_slice<byte>(n);
    for (nint i = 0; i < n; i++) {
        m[i] = byte(i);
    }
    return moveToFrontDecoder(m);
});

private static byte Decode(this moveToFrontDecoder m, nint n) {
    byte b = default;
 
    // Implement move-to-front with a simple copy. This approach
    // beats more sophisticated approaches in benchmarking, probably
    // because it has high locality of reference inside of a
    // single cache line (most move-to-front operations have n < 64).
    b = m[n];
    copy(m[(int)1..], m[..(int)n]);
    m[0] = b;
    return ;
}

// First returns the symbol at the front of the list.
private static byte First(this moveToFrontDecoder m) {
    return m[0];
}

} // end bzip2_package
