// package main -- go2cs converted at 2022 March 06 23:33:43 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\defer.go
// Tests of defer.  (Deferred recover() belongs is recover.go.)

using fmt = go.fmt_package;
using System;


namespace go;

public static partial class main_package {

private static (nint, nint) deferMutatesResults(bool noArgReturn) => func((defer, panic, _) => {
    nint a = default;
    nint b = default;

    defer(() => {
        if (a != 1 || b != 2) {
            panic(fmt.Sprint(a, b));
        }
        (a, b) = (3, 4);
    }());
    if (noArgReturn) {
        (a, b) = (1, 2);        return ;
    }
    return (1, 2);

});

private static void init() => func((_, panic, _) => {
    var (a, b) = deferMutatesResults(true);
    if (a != 3 || b != 4) {
        panic(fmt.Sprint(a, b));
    }
    a, b = deferMutatesResults(false);
    if (a != 3 || b != 4) {
        panic(fmt.Sprint(a, b));
    }
});

// We concatenate init blocks to make a single function, but we must
// run defers at the end of each block, not the combined function.
private static nint deferCount = 0;

private static void init() => func((_, panic, _) => {
    deferCount = 1;
    defer(() => {
        deferCount++;
    }()); 
    // defer runs HERE
});

private static void init() => func((_, panic, _) => { 
    // Strictly speaking the spec says deferCount may be 0 or 2
    // since the relative order of init blocks is unspecified.
    if (deferCount != 2) {
        panic(deferCount); // defer call has not run!
    }
});

private static void Main() {
}

} // end main_package
