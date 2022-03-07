// package main -- go2cs converted at 2022 March 06 23:33:41 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\complit.go
// Tests of composite literals.

using fmt = go.fmt_package;

namespace go;

public static partial class main_package {

    // Map literals.
    // TODO(adonovan): we can no longer print maps
    // until the interpreter supports (reflect.Value).MapRange.
private static void _() => func((_, panic, _) => {
    public partial struct M { // : map<nint, nint>
    }
    ptr<M> m1 = new slice<ptr<M>>(new ptr<M>[] { {1:1}, &M{2:2} });
    @string want = "map[1:1] map[2:2]";
    {
        var got__prev1 = got;

        var got = fmt.Sprint(m1[0].val, m1[1].val);

        if (got != want) {
            panic(got);
        }
        got = got__prev1;

    }

    M m2 = new slice<M>(new M[] { {1:1}, M{2:2} });
    {
        var got__prev1 = got;

        got = fmt.Sprint(m2[0], m2[1]);

        if (got != want) {
            panic(got);
        }
        got = got__prev1;

    }

});

// Nonliteral keys in composite literal.
private static void init() => func((_, panic, _) => {
    const nint zero = 1;

    nint v = new slice<nint>(InitKeyedValues<nint>((1+zero, 42)));
    {
        var x = fmt.Sprint(v);

        if (x != "[0 0 42]") {
            panic(x);
        }
    }

});

// Test for in-place initialization.
private static void init() => func((_, panic, _) => { 
    // struct
    public partial struct S {
        public nint a;
        public nint b;
    }
    S s = new S(1,2);
    s = new S(b:3);
    if (s.a != 0) {
        panic("s.a != 0");
    }
    if (s.b != 3) {
        panic("s.b != 3");
    }
    s = new S();
    if (s.a != 0) {
        panic("s.a != 0");
    }
    if (s.b != 0) {
        panic("s.b != 0");
    }
    public partial struct A { // : array<nint>
    }
    A a = new A(2,4,6,8);
    a = new A(1:6,2:4);
    if (a[0] != 0) {
        panic("a[0] != 0");
    }
    if (a[1] != 6) {
        panic("a[1] != 6");
    }
    if (a[2] != 4) {
        panic("a[2] != 4");
    }
    if (a[3] != 0) {
        panic("a[3] != 0");
    }
    a = new A();
    if (a[0] != 0) {
        panic("a[0] != 0");
    }
    if (a[1] != 0) {
        panic("a[1] != 0");
    }
    if (a[2] != 0) {
        panic("a[2] != 0");
    }
    if (a[3] != 0) {
        panic("a[3] != 0");
    }
});

// Regression test for https://golang.org/issue/10127:
// composite literal clobbers destination before reading from it.
private static void init() => func((_, panic, _) => { 
    // map
 {
        public partial struct M { // : map<@string, nint>
        }
        M m = new M("x":1,"y":2);
        m = new M("x":m["y"],"y":m["x"]);
        if (m["x"] != 2 || m["y"] != 1) {
            panic(fmt.Sprint(m));
        }
        M n = new M("x":3);
        (m, n) = (new M("x":n["x"]), new M("x":m["x"]));        {
            var got = fmt.Sprint(m["x"], n["x"]);

            if (got != "3 2") {
                panic(got);
            }

        }

    } {
        public partial struct T {
            public nint x;
            public nint y;
            public nint z;
        }
        T t = new T(x:1,y:2,z:3);

        t = new T(x:t.y,y:t.z,z:t.x); // all fields
        {
            got = fmt.Sprint(t);

            if (got != "{2 3 1}") {
                panic(got);
            }

        }


        t = new T(x:t.y,y:t.z+3); // not all fields
        {
            got = fmt.Sprint(t);

            if (got != "{3 4 0}") {
                panic(got);
            }

        }


        T u = new T(x:5,y:6,z:7);
        (t, u) = (new T(x:u.x), new T(x:t.x));        {
            got = fmt.Sprint(t, u);

            if (got != "{5 0 0} {3 0 0}") {
                panic(got);
            }

        }

    } {
        array<nint> a = new array<nint>(InitKeyedValues<nint>(3, (0, 1), (1, 2), (2, 3)));

        a = new array<nint>(InitKeyedValues<nint>(3, (0, a[1]), (1, a[2]), (2, a[0]))); //  all elements
        {
            got = fmt.Sprint(a);

            if (got != "[2 3 1]") {
                panic(got);
            }

        }


        a = new array<nint>(InitKeyedValues<nint>(3, (0, a[1]), (1, a[2]+3))); //  not all elements
        {
            got = fmt.Sprint(a);

            if (got != "[3 4 0]") {
                panic(got);
            }

        }


        array<nint> b = new array<nint>(InitKeyedValues<nint>(3, (0, 5), (1, 6), (2, 7)));
        (a, b) = (new array<nint>(InitKeyedValues<nint>(3, (0, b[0]))), new array<nint>(InitKeyedValues<nint>(3, (0, a[0]))));        {
            got = fmt.Sprint(a, b);

            if (got != "[5 0 0] [3 0 0]") {
                panic(got);
            }

        }

    } {
        nint s = new slice<nint>(InitKeyedValues<nint>((0, 1), (1, 2), (2, 3)));

        s = new slice<nint>(InitKeyedValues<nint>((0, s[1]), (1, s[2]), (2, s[0]))); //  all elements
        {
            got = fmt.Sprint(s);

            if (got != "[2 3 1]") {
                panic(got);
            }

        }


        s = new slice<nint>(InitKeyedValues<nint>((0, s[1]), (1, s[2]+3))); //  not all elements
        {
            got = fmt.Sprint(s);

            if (got != "[3 4]") {
                panic(got);
            }

        }


        t = new slice<nint>(InitKeyedValues<nint>((0, 5), (1, 6), (2, 7)));
        (s, t) = (new slice<nint>(InitKeyedValues<nint>((0, t[0]))), new slice<nint>(InitKeyedValues<nint>((0, s[0]))));        {
            got = fmt.Sprint(s, t);

            if (got != "[5] [3]") {
                panic(got);
            }

        }

    }
});

// Regression test for https://golang.org/issue/13341:
// within a map literal, if a key expression is a composite literal,
// Go 1.5 allows its type to be omitted.  An & operation may be implied.
private static void init() => func((_, panic, _) => {
    public partial struct S {
        public nint a;
        public nint b;
    } 
    // same as map[*S]bool{&S{x: 1}: true}
    map m = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<S>, bool>{{x:1}:true};
    foreach (var (s) in m) {
        if (s.x != 1) {
            panic(s); // wrong key
        }
        return ;

    }    panic("map is empty");

});

private static void Main() {
}

} // end main_package
