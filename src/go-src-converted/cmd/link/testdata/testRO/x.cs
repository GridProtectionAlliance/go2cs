// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test that read-only data is indeed read-only. This
// program attempts to modify read-only data, and it
// should fail.

// package main -- go2cs converted at 2022 March 13 06:35:39 UTC
// Original source: C:\Program Files\Go\src\cmd\link\testdata\testRO\x.go
namespace go;

using @unsafe = @unsafe_package;

public static partial class main_package {

private static @string s = "hello";

private static void Main() {
    println(s);
    .p = 'H';
    println(s);
}

} // end main_package
