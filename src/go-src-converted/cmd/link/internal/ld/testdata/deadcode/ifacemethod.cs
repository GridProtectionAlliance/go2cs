// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test that a method of a reachable type is not necessarily
// live even if it matches an interface method, as long as
// the type is never converted to an interface.

// package main -- go2cs converted at 2022 March 06 23:22:34 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\deadcode\ifacemethod.go


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

private static ptr<T> p;
private static var e = default;

private static void Main() {
    p = @new<T>(); // used T, but never converted to interface in any reachable code
    e._<I>().M(); // used I and I.M
}

public static void Unused() { // convert T to interface, but this function is not reachable
    I i = I.As(T(0))!;
    i.M();

}

public static var Unused2 = T(1); // convert T to interface, in an unreachable global initializer

} // end main_package
