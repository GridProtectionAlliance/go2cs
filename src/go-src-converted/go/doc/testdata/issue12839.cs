// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package issue12839 is a go/doc test to test association of a function
// that returns multiple types.
// See golang.org/issue/12839.
// (See also golang.org/issue/27928.)

// package issue12839 -- go2cs converted at 2022 March 13 05:52:40 UTC
// import "go/doc.issue12839" ==> using issue12839 = go.go.doc.issue12839_package
// Original source: C:\Program Files\Go\src\go\doc\testdata\issue12839.go
namespace go.go;

using p = p_package;

public static partial class issue12839_package {

public partial struct T1 {
}

public partial struct T2 {
}

public static @string hello(this T1 t) {
    return "hello";
}

// F1 should not be associated with T1
public static (ptr<T1>, ptr<T2>) F1() {
    ptr<T1> _p0 = default!;
    ptr<T2> _p0 = default!;

    return (addr(new T1()), addr(new T2()));
}

// F2 should be associated with T1
public static (T1, T1, T1) F2() {
    T1 a = default;
    T1 b = default;
    T1 c = default;

    return (new T1(), new T1(), new T1());
}

// F3 should be associated with T1 because b.T3 is from a different package
public static (T1, p.T3) F3() {
    T1 a = default;
    p.T3 b = default;

    return (new T1(), new p.T3());
}

// F4 should not be associated with a type (same as F1)
public static (T1, T2) F4() {
    T1 a = default;
    T2 b = default;

    return (new T1(), new T2());
}

// F5 should be associated with T1.
public static (T1, error) F5() {
    T1 _p0 = default;
    error _p0 = default!;

    return (new T1(), error.As(null!)!);
}

// F6 should be associated with T1.
public static (ptr<T1>, error) F6() {
    ptr<T1> _p0 = default!;
    error _p0 = default!;

    return (addr(new T1()), error.As(null!)!);
}

// F7 should be associated with T1.
public static (T1, @string) F7() {
    T1 _p0 = default;
    @string _p0 = default;

    return (new T1(), null);
}

// F8 should be associated with T1.
public static (nint, T1, @string) F8() {
    nint _p0 = default;
    T1 _p0 = default;
    @string _p0 = default;

    return (0, new T1(), null);
}

// F9 should not be associated with T1.
public static (nint, T1, T2) F9() {
    nint _p0 = default;
    T1 _p0 = default;
    T2 _p0 = default;

    return (0, new T1(), new T2());
}

// F10 should not be associated with T1.
public static (T1, T2, error) F10() {
    T1 _p0 = default;
    T2 _p0 = default;
    error _p0 = default!;

    return (new T1(), new T2(), error.As(null!)!);
}

} // end issue12839_package
