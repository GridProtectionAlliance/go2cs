// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2022 March 13 06:35:38 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\issue39757\issue39757main.go
namespace go;

public static partial class main_package {

public static nint G = default;

private static void Main() {
    if (G != 101) {
        println("not 101");
    }
    else
 {
        println("well now that's interesting");
    }
}

} // end main_package
