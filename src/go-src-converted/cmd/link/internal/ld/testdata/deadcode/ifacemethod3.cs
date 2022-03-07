// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Like ifacemethod2.go, this tests that a method *is* live
// if the type is "indirectly" converted to an interface
// using reflection with a method descriptor as intermediate.

// package main -- go2cs converted at 2022 March 06 23:22:34 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\deadcode\ifacemethod3.go
using reflect = go.reflect_package;

namespace go;

public static partial class main_package {

public partial struct S { // : nint
}

public static void M(this S s) {
    println("S.M");
}

public partial interface I {
    void M();
}

public partial struct T { // : double
}

public static void F(this T t, S s) {
}

private static void Main() {
    T t = default;
    var ft = reflect.TypeOf(t).Method(0).Type;
    var at = ft.In(1);
    var v = reflect.New(at).Elem();
    v.Interface()._<I>().M();
}

} // end main_package
