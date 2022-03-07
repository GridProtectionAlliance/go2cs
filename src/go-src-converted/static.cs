// package main -- go2cs converted at 2022 March 06 23:33:46 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\static.go


namespace go;

public static partial class main_package {

    // Static tests of SSA builder (via the sanity checker).
    // Dynamic semantics are not exercised.
private static void init() { 
    // Regression test for issue 6806.
    var ch = make_channel<nint>();
    _ = n;
    private partial struct mybool { // : bool
    }
    nint x = default;
    mybool y = default;
    _ = x;
    _ = y;

}

private static nint a = default;

// Regression test for issue 7840 (covered by SSA sanity checker).
private static bool bug7840() { 
    // This creates a single-predecessor block with a φ-node.
    return false && a == 0 && a == 0;

}

// A blocking select (sans "default:") cannot fall through.
// Regression test for issue 7022.
private static nint bug7022() {
    channel<nint> c1 = default;    channel<nint> c2 = default;

    return 123;
    return 456;
}

// Parens should not prevent intrinsic treatment of built-ins.
// (Regression test for a crash.)
private static void init() {
    _ = (new)(int);
    _ = (make)(typeof(slice<nint>), 0);
}

private static void Main() {
}

} // end main_package
