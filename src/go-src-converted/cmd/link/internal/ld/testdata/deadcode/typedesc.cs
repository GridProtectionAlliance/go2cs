// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test that a live variable doesn't bring its type
// descriptor live.

// package main -- go2cs converted at 2022 March 13 06:35:38 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\deadcode\typedesc.go
namespace go;

public static partial class main_package {

public partial struct T { // : array<@string>
}

private static T t = default;

private static void Main() {
    println(t[8]);
}

} // end main_package
