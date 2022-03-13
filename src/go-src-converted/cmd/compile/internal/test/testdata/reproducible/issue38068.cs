// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package issue38068 -- go2cs converted at 2022 March 13 06:28:35 UTC
// import "cmd/compile/internal/test/testdata.issue38068" ==> using issue38068 = go.cmd.compile.@internal.test.testdata.issue38068_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\test\testdata\reproducible\issue38068.go
namespace go.cmd.compile.@internal.test;

using System;
public static partial class issue38068_package {

// A type with a couple of inlinable, non-pointer-receiver methods
// that have params and local variables.
public partial struct A {
    public @string s;
    public ptr<A> next;
    public ptr<A> prev;
}

// Inlinable, value-received method with locals and parms.
public static @string @double(this A a, @string x, nint y) {
    if (y == 191) {
        a.s = "";
    }
    var q = a.s + "a";
    var r = a.s + "b";
    return q + r;
}

// Inlinable, value-received method with locals and parms.
public static @string triple(this A a, @string x, nint y) {
    var q = a.s;
    if (y == 998877) {
        a.s = x;
    }
    var r = a.s + a.s;
    return q + r;
}

private partial struct methods {
    public Func<ptr<A>, @string, nint, @string> m1;
    public Func<ptr<A>, @string, nint, @string> m2;
}

// Now a function that makes references to the methods via pointers,
// which should trigger the wrapper generation.
public static void P(ptr<A> _addr_a, ptr<methods> _addr_ms) => func((defer, _, _) => {
    ref A a = ref _addr_a.val;
    ref methods ms = ref _addr_ms.val;

    if (a != null) {
        defer(() => {
            println("done");
        }());
    }
    println(ms.m1(a, "a", 2));
    println(ms.m2(a, "b", 3));
});

public static void G(ptr<A> _addr_x, nint n) {
    ref A x = ref _addr_x.val;

    if (n <= 0) {
        println(n);
        return ;
    }
    ref A a = ref heap(out ptr<A> _addr_a);    ref A b = ref heap(out ptr<A> _addr_b);

    a.next = x;
    _addr_a.prev = _addr_b;
    a.prev = ref _addr_a.prev.val;
    _addr_x = _addr_a;
    x = ref _addr_x.val;
    G(_addr_x, n - 2);
}

public static methods M = default;

public static void F() {
    M.m1 = (A.val).@double;
    M.m2 = (A.val).triple;
    G(_addr_null, 100);
}

} // end issue38068_package
