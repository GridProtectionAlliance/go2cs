// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:35:38 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\issue10978\main.go
namespace go;

public static partial class main_package {

private static void undefined();

private static nint defined1() { 
    // To check multiple errors for a single symbol,
    // reference undefined more than once.
    undefined();
    undefined();
    return 0;
}

private static void defined2() {
    undefined();
    undefined();
}

private static void init() {
    _ = defined1();
    defined2();
}

// The "main" function remains undeclared.

} // end main_package
