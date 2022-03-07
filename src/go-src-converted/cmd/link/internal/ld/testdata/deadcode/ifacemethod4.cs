// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Test that a live type's method is not live even if
// it matches an interface method, as long as the interface
// method is not used.

// package main -- go2cs converted at 2022 March 06 23:22:34 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\deadcode\ifacemethod4.go


namespace go;

public static partial class main_package {

public partial struct T { // : nint
}

//go:noinline
public static void M(this T _p0) {
}

public partial interface I {
    void M();
}

private static ptr<T> p;
private static ptr<I> pp;

private static void Main() {
    p = @new<T>(); // use type T
    pp = @new<I>(); // use type I
    pp.val = p.val; // convert T to I, build itab
}

} // end main_package
