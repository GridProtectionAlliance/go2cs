// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This test case contains two static temps (the array literals)
// with same contents but different sizes. The linker should not
// report a hash collision. The linker can (and actually does)
// dedup the two symbols, by keeping the larger symbol. The dedup
// is not a requirement for correctness and not checked in this test.
// We do check the emitted symbol contents are correct, though.

// package main -- go2cs converted at 2022 March 06 23:22:35 UTC
// Original source: C:\Program Files\Go\src\cmd\link\testdata\testHashedSyms\p.go


namespace go;

public static partial class main_package {

private static void Main() {
    F(new array<nint>(new nint[] { 1, 2, 3, 4, 5, 6 }), new array<nint>(new nint[] { 1, 2, 3, 4, 5, 6 }));
}

//go:noinline
public static void F(object x, object y) => func((_, panic, _) => {
    array<nint> x1 = x._<array<nint>>();
    array<nint> y1 = y._<array<nint>>();
    foreach (var (i) in y1) {
        if (i < 6) {
            if (x1[i] != i + 1 || y1[i] != i + 1) {
                panic("FAIL");
            }
        }
        else
 {
            if ((i < len(x1) && x1[i] != 0) || y1[i] != 0) {
                panic("FAIL");
            }
        }
    }
});

} // end main_package
