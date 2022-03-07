// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test that read-only data is indeed read-only. This
// program attempts to modify read-only data, and it
// should fail.

// package main -- go2cs converted at 2022 March 06 23:22:35 UTC
// Original source: C:\Program Files\Go\src\cmd\link\testdata\testRO\x.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class main_package {

private static @string s = "hello";

private static void Main() {
    println(s);
    .p = 'H';
    println(s);
}

} // end main_package
