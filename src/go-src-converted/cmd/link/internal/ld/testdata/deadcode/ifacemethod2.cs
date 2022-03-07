// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test that a method *is* live if it matches an interface
// method and the type is "indirectly" converted to an
// interface through reflection.

// package main -- go2cs converted at 2022 March 06 23:22:34 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\deadcode\ifacemethod2.go
using reflect = go.reflect_package;

namespace go;

public static partial class main_package {

public partial interface I {
    void M();
}

public partial struct T { // : nint
}

public static void M(this T _p0) {
    println("XXX");
}

private static void Main() {
    var e = reflect.ValueOf(new slice<T>(new T[] { 1 })).Index(0).Interface();
    e._<I>().M();
}

} // end main_package
