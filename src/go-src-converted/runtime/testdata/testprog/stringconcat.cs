// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 06 22:26:07 UTC
// Original source: C:\Program Files\Go\src\runtime\testdata\testprog\stringconcat.go
using strings = go.strings_package;

namespace go;

public static partial class main_package {

private static void init() {
    register("stringconcat", stringconcat);
}

private static void stringconcat() => func((_, panic, _) => {
    var s0 = strings.Repeat("0", 1 << 10);
    var s1 = strings.Repeat("1", 1 << 10);
    var s2 = strings.Repeat("2", 1 << 10);
    var s3 = strings.Repeat("3", 1 << 10);
    var s = s0 + s1 + s2 + s3;
    panic(s);
});

} // end main_package
