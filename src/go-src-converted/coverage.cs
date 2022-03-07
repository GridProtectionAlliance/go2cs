// This interpreter test is designed to run very quickly yet provide
// some coverage of a broad selection of constructs.
//
// Validate this file with 'go run' after editing.
// TODO(adonovan): break this into small files organized by theme.

// package main -- go2cs converted at 2022 March 06 23:33:43 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\coverage.go
using fmt = go.fmt_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using System.Threading;
using System;


namespace go;

public static partial class main_package {

private static void init() => func((_, panic, _) => { 
    // Call of variadic function with (implicit) empty slice.
    {
        var x = fmt.Sprint();

        if (x != "") {
            panic(x);
        }
    }

});

private partial interface empty {
}

public partial interface I {
    nint f();
}

public partial struct T {
    public nint z;
}

public static nint f(this T t) {
    return t.z;
}

private static void use(object _p0) {
}

private static nint counter = 2;

// Test initialization, including init blocks containing 'return'.
// Assertion is in main.
private static void init() => func((_, panic, _) => {
    counter *= 3;
    return ;
    counter *= 3;
});

private static void init() => func((_, panic, _) => {
    counter *= 5;
    return ;
    counter *= 5;
});

// Recursion.
private static nint fib(nint x) {
    if (x < 2) {
        return x;
    }
    return fib(x - 1) + fib(x - 2);

}

private static void fibgen(channel<nint> ch) {
    for (nint x = 0; x < 10; x++) {
        ch.Send(fib(x));
    }
    close(ch);
}

// Goroutines and channels.
private static void init() => func((_, panic, _) => {
    var ch = make_channel<nint>();
    go_(() => fibgen(ch));
    slice<nint> fibs = default;
    foreach (var (v) in ch) {
        fibs = append(fibs, v);
        if (len(fibs) == 10) {
            break;
        }
    }    {
        var x = fmt.Sprint(fibs);

        if (x != "[0 1 1 2 3 5 8 13 21 34]") {
            panic(x);
        }
    }

});

// Test of aliasing.
private static void init() => func((_, panic, _) => {
    public partial struct S {
        public @string a;
        public @string b;
    }

    @string s1 = new slice<@string>(new @string[] { "foo", "bar" });
    var s2 = s1; // creates an alias
    s2[0] = "wiz";
    {
        var x = fmt.Sprint(s1, s2);

        if (x != "[wiz bar] [wiz bar]") {
            panic(x);
        }
    }


    ptr<@string> pa1 = addr(new array<@string>(new @string[] { "foo", "bar" }));
    var pa2 = pa1; // creates an alias
    pa2[0] = "wiz";
    {
        x = fmt.Sprint(pa1.val, pa2.val);

        if (x != "[wiz bar] [wiz bar]") {
            panic(x);
        }
    }


    array<@string> a1 = new array<@string>(new @string[] { "foo", "bar" });
    var a2 = a1; // creates a copy
    a2[0] = "wiz";
    {
        x = fmt.Sprint(a1, a2);

        if (x != "[foo bar] [wiz bar]") {
            panic(x);
        }
    }


    S t1 = new S("foo","bar");
    var t2 = t1; // copy
    t2.a = "wiz";
    {
        x = fmt.Sprint(t1, t2);

        if (x != "{foo bar} {wiz bar}") {
            panic(x);
        }
    }

});

private static void Main() => func((_, panic, _) => {
    print(); // legal

    if (counter != 2 * 3 * 5) {
        panic(counter);
    }
    public partial struct N { // : System.Numerics.Complex128
    }
    N n = default;
    n = complex(1.0F, 2.0F);
    if (n != complex(1.0F, 2.0F)) {
        panic(n);
    }
    {
        var x__prev1 = x;

        var x = reflect.TypeOf(n).String();

        if (x != "main.N") {
            panic(x);
        }
        x = x__prev1;

    }

    if (real(n) != 1.0F || imag(n) != 2.0F) {
        panic(n);
    }
    var ch = make_channel<nint>(1);
    panic("couldn't send");
    if (ch != 1.Receive()) {
        panic("couldn't receive");
    }
    nint anint = 0;
    var ok = false;
    _ = anint;
    _ = ok; 

    // Anon structs with methods.
    struct{T} anon = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{T}{T:T{z:1}};
    {
        var x__prev1 = x;

        x = anon.f();

        if (x != 1) {
            panic(x);
        }
        x = x__prev1;

    }

    I i = I.As(anon)!;
    {
        var x__prev1 = x;

        x = i.f();

        if (x != 1) {
            panic(x);
        }
        x = x__prev1;

    } 
    // NB. precise output of reflect.Type.String is undefined.
    {
        var x__prev1 = x;

        x = reflect.TypeOf(i).String();

        if (x != "struct { main.T }" && x != "struct{main.T}") {
            panic(x);
        }
        x = x__prev1;

    } 

    // fmt.
    const @string message = "Hello, World!";

    if (fmt.Sprint("Hello", ", ", "World", "!") != message) {
        panic("oops");
    }
    public partial struct S {
        public @string a;
        public @string b;
    }
    empty e = empty.As(new S(f:42))!;
    switch (e.type()) {
        case S v:
            if (v.f != 42) {
                panic(v.f);
            }
            break;
        default:
        {
            var v = e.type();
            panic(reflect.TypeOf(v));
            break;
        }
    }
    {
        I i__prev1 = i;

        I (i, ok) = I.As(e._<I>())!;

        if (ok) {
            panic(i);
        }
        i = i__prev1;

    } 

    // Switch.
    x = default;

    if (x == 1)
    {
        panic(x);
        fallthrough = true;
    }
    if (fallthrough || x == 2 || x == 3)
    {
        panic(x);
        goto __switch_break0;
    }
    // default: 
    __switch_break0:; 
    // empty switch

    // empty switch

    else     // empty switch

    if (false)     else 
    // string -> []rune conversion.
    use((slice<int>)"foo"); 

    // Calls of form x.f().
    public partial struct S2 {
        public Func<nint> f;
    }
    new S2(f:func()int{return1}).f(); // field is a func value
    new T().f(); // method call
    i.f()().f(); // anon interface method invocation

    // Map lookup.
    {
        var v__prev1 = v;

        map (v, ok) = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{}["foo5"];

        if (v != "" || ok) {
            panic("oops");
        }
        v = v__prev1;

    } 

    // Regression test: implicit address-taken struct literal
    // inside literal map element.
});

private partial struct mybool { // : bool
}

private static void f(this mybool _p0) {
}

private static void init() => func((_, panic, _) => {
    private partial struct mybool { // : bool
    }
    mybool b = default;
    var i = b || b; // result preserves types of operands
    _ = i._<mybool>();

    i = false && b; // result preserves type of "typed" operand
    _ = i._<mybool>();

    i = b || true; // result preserves type of "typed" operand
    _ = i._<mybool>();

});

private static void init() => func((_, panic, _) => {
    nint x = default;    nint y = default;

    mybool b = x == y; // x==y is an untyped bool
    b.f();

});

// Simple closures.
private static void init() => func((_, panic, _) => {
    nint b = 3;
    Func<nint, nint> f = a => {
        return a + b;
    };
    b++;
    {
        var x = f(1);

        if (x != 5) { // 1+4 == 5
            panic(x);

        }
    }

    b++;
    {
        x = f(2);

        if (x != 7) { // 2+5 == 7
            panic(x);

        }
    }

    {
        b = f(1) < 16 || f(2) < 17;

        if (!b) {
            panic("oops");
        }
    }

});

// Shifts.
private static void init() => func((_, panic, _) => {
    long i = 1;
    ulong u = 1 << 32;
    {
        var x = i << (int)(uint32(u));

        if (x != 1) {
            panic(x);
        }
    }

    {
        x = i << (int)(uint64(u));

        if (x != 0) {
            panic(x);
        }
    }

});

// Implicit conversion of delete() key operand.
private static void init() => func((_, panic, _) => {
    public partial interface I {
        nint f();
    }
    var m = make_map<I, bool>();
    m[1] = true;
    m[I(2)] = true;
    if (len(m) != 2) {
        panic(m);
    }
    delete(m, I(1));
    delete(m, 2);
    if (len(m) != 0) {
        panic(m);
    }
});

// An I->I conversion always succeeds.
private static void init() => func((_, panic, _) => {
    I x = default!;
    if (I(x) != I(null)) {
        panic("I->I conversion failed");
    }
});

// An I->I type-assert fails iff the value is nil.
private static void init() => func((_, panic, _) => {
    defer(() => {
        var r = fmt.Sprint(recover()); 
        // Exact error varies by toolchain.
        if (r != "runtime error: interface conversion: interface is nil, not main.I" && r != "interface conversion: interface is nil, not main.I") {
            panic("I->I type assertion succeeded for nil value");
        }
    }());
    I x = default!;
    _ = x._<I>();

});

//////////////////////////////////////////////////////////////////////
// Variadic bridge methods and interface thunks.

public partial struct VT { // : nint
}

private static nint vcount = 0;

public static void f(this VT _p0, nint x, params @string[] y) => func((_, panic, _) => {
    y = y.Clone();

    vcount++;
    if (x != 1) {
        panic(x);
    }
    if (len(y) != 2 || y[0] != "foo" || y[1] != "bar") {
        panic(y);
    }
});

public partial struct VS {
    public ref VT VT => ref VT_val;
}

public partial interface VI {
    void f(nint x, params @string[] y);
}

private static void init() => func((_, panic, _) => {
    @string foobar = new slice<@string>(new @string[] { "foo", "bar" });
    VS s = default;
    s.f(1, "foo", "bar");
    s.f(1, foobar);
    if (vcount != 2) {
        panic("s.f not called twice");
    }
    var fn = VI.f;
    fn(s, 1, "foo", "bar");
    fn(s, 1, foobar);
    if (vcount != 4) {
        panic("I.f not called twice");
    }
});

// Multiple labels on same statement.
private static void multipleLabels() => func((_, panic, _) => {
    slice<nint> trace = default;
    nint i = 0;
one:
two:
    while (i < 3) {
        trace = append(trace, i);
        switch (i) {
            case 0: 
                _continuetwo = true;
                break;
                break;
            case 1: 
                i++;
                goto one;
                break;
            case 2: 
                _breaktwo = true;
                break;
                break;
        }
        i++;
    }
    {
        var x = fmt.Sprint(trace);

        if (x != "[0 1 2]") {
            panic(x);
        }
    }

});

private static void init() => func((_, panic, _) => {
    multipleLabels();
});

private static void init() => func((_, panic, _) => { 
    // Struct equivalence ignores blank fields.
    private partial struct s {
        public nint x;
        public nint _;
        public nint z;
    }
    s s1 = new s(x:1,z:3);
    s s2 = new s(x:1,z:3);
    if (s1 != s2) {
        panic("not equal");
    }
});

private static void init() => func((_, panic, _) => { 
    // A slice var can be compared to const []T nil.
    @string i = new slice<@string>(new @string[] { "foo" });
    slice<@string> j = (slice<@string>)null;
    if (i._<slice<@string>>() == null) {
        panic("expected i non-nil");
    }
    if (j._<slice<@string>>() != null) {
        panic("expected j nil");
    }
    defer(() => {
        var r = fmt.Sprint(recover());
        if (!(strings.Contains(r, "compar") && strings.Contains(r, "[]string"))) {
            panic("want panic from slice comparison, got " + r);
        }
    }());
    _ = i == j; // interface comparison recurses on types
});

private static void init() => func((_, panic, _) => { 
    // Regression test for SSA renaming bug.
    slice<nint> ints = default;
    foreach (>>MARKER:FORRANGEEXPRESSIONS_LEVEL_1<< in "foo") {>>MARKER:FORRANGEMUTABLEEXPRESSIONS_LEVEL_1<<
        nint x = default;
        x++;
        ints = append(ints, x);
    }    if (fmt.Sprint(ints) != "[1 1 1]") {
        panic(ints);
    }
});

// Regression test for issue 6949:
// []byte("foo") is not a constant since it allocates memory.
private static void init() => func((_, panic, _) => {
    @string r = default;
    foreach (var (i, b) in "ABC") {
        slice<byte> x = (slice<byte>)"abc";
        x[i] = byte(b);
        r += string(x);
    }    if (r != "AbcaBcabC") {
        panic(r);
    }
});

// Test of 3-operand x[lo:hi:max] slice.
private static void init() => func((_, panic, _) => {
    nint s = new slice<nint>(new nint[] { 0, 1, 2, 3 });
    Func<slice<nint>, array<nint>> lenCapLoHi = x => new array<nint>(new nint[] { len(x), cap(x), x[0], x[len(x)-1] });
    {
        var got = lenCapLoHi(s[(int)1..(int)3]);

        if (got != new array<nint>(new nint[] { 2, 3, 1, 2 })) {
            panic(got);
        }
    }

    {
        got = lenCapLoHi(s.slice(1, 3, 3));

        if (got != new array<nint>(new nint[] { 2, 2, 1, 2 })) {
            panic(got);
        }
    }

    nint max = 3;
    if ("a"[0] == 'a') {
        max = 2; // max is non-constant, even in SSA form
    }
    {
        got = lenCapLoHi(s.slice(1, 2, max));

        if (got != new array<nint>(new nint[] { 1, 1, 1, 1 })) {
            panic(got);
        }
    }

});

private static nint one = 1; // not a constant

// Test makeslice.
private static void init() => func((_, panic, _) => {
    Action<slice<@string>, nint, nint> check = (s, wantLen, wantCap) => {
        if (len(s) != wantLen) {
            panic(len(s));
        }
        if (cap(s) != wantCap) {
            panic(cap(s));
        }
    }; 
    //                                       SSA form:
    check(make_slice<@string>(10), 10, 10); // new([10]string)[:10]
    check(make_slice<@string>(one), 1, 1); // make([]string, one, one)
    check(make_slice<@string>(0, 10), 0, 10); // new([10]string)[:0]
    check(make_slice<@string>(0, one), 0, 1); // make([]string, 0, one)
    check(make_slice<@string>(one, 10), 1, 10); // new([10]string)[:one]
    check(make_slice<@string>(one, one), 1, 1); // make([]string, one, one)
});

// Test that a nice error is issued by indirection wrappers.
private static void init() => func((_, panic, _) => {
    ptr<T> ptr;
    I i = I.As(ptr)!;

    defer(() => {
        var r = fmt.Sprint(recover()); 
        // Exact error varies by toolchain:
        if (r != "runtime error: value method (main.T).f called using nil *main.T pointer" && r != "value method (main.T).f called using nil *main.T pointer") {
            panic("want panic from call with nil receiver, got " + r);
        }
    }());
    i.f();
    panic("unreachable");

});

// Regression test for a subtle bug in which copying values would causes
// subcomponents of aggregate variables to change address, breaking
// aliases.
private static void init() => func((_, panic, _) => {
    public partial struct T {
        public nint z;
    }
    T x = default;
    var p = _addr_x.f;
    x = new T();
    p.val = 1;
    if (x.f != 1) {
        panic("lost store");
    }
    if (p != _addr_x.f) {
        panic("unstable address");
    }
});

} // end main_package
