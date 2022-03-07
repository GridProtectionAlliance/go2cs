// package main -- go2cs converted at 2022 March 06 23:33:45 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\initorder.go
using fmt = go.fmt_package;

namespace go;

public static partial class main_package {

    // Test of initialization order of package-level vars.
private static nint counter = default;

private static nint next() {
    var c = counter;
    counter++;
    return c;
}

private static (nint, nint) next2() {
    nint x = default;
    nint y = default;

    x = next();
    y = next();
    return ;
}

private static nint makeOrder() {
    (_, _, _, _) = (f, b, d, e);    return 0;
}

private static void Main() => func((_, panic, _) => { 
    // Initialization constraints:
    // - {f,b,c/d,e} < order  (ref graph traversal)
    // - order < {a}          (lexical order)
    // - b < c/d < e < f      (lexical order)
    // Solution: a b c/d e f
    array<nint> abcdef = new array<nint>(new nint[] { a, b, c, d, e, f });
    if (abcdef != new array<nint>(new nint[] { 0, 1, 2, 3, 4, 5 })) {
        panic(abcdef);
    }
});

private static var order = makeOrder();

private static var a = next();private static var b = next();


private static var e = next();private static var f = next();

// ------------------------------------------------------------------------



// ------------------------------------------------------------------------

private static slice<@string> order2 = default;

private static nint create(nint x, @string name) {
    order2 = append(order2, name);
    return x;
}

public static var C = create(B + 1, "C");
public static var A = create(1, "A");public static var B = create(2, "B");

// Initialization order of package-level value specs.


// Initialization order of package-level value specs.
private static void init() => func((_, panic, _) => {
    var x = fmt.Sprint(order2); 
    // Result varies by toolchain.  This is a spec bug.
    if (x != "[B C A]" && x != "[A B C]") { // go/types
        panic(x);

    }
    if (C != 3) {
        panic(c);
    }
});

} // end main_package
