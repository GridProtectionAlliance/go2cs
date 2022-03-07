// package main -- go2cs converted at 2022 March 06 23:33:46 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\interp\testdata\recover.go
// Tests of panic/recover.

using fmt = go.fmt_package;
using System;


namespace go;

public static partial class main_package {

private static nint fortyTwo() => func((defer, panic, recover) => {
    nint r = default;

    r = 42; 
    // The next two statements simulate a 'return' statement.
    defer(() => {
        recover();
    }());
    panic(null);

});

private static nint zero() => func((defer, panic, recover) => {
    defer(() => {
        recover();
    }());
    panic(1);

});

private static (nint, @string) zeroEmpty() => func((defer, panic, recover) => {
    nint _p0 = default;
    @string _p0 = default;

    defer(() => {
        recover();
    }());
    panic(1);

});

private static void Main() => func((_, panic, _) => {
    {
        var r__prev1 = r;

        var r = fortyTwo();

        if (r != 42) {
            panic(r);
        }
        r = r__prev1;

    }

    {
        var r__prev1 = r;

        r = zero();

        if (r != 0) {
            panic(r);
        }
        r = r__prev1;

    }

    {
        var r__prev1 = r;

        var (r, s) = zeroEmpty();

        if (r != 0 || s != "") {
            panic(fmt.Sprint(r, s));
        }
        r = r__prev1;

    }

});

} // end main_package
