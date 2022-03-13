// package main -- go2cs converted at 2022 March 13 06:22:53 UTC
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\testdata\infloop.go
namespace go;

using System;
using System.Threading;
public static partial class main_package {

private static nint sink = default;

//go:noinline
private static void test() { 
    // This is for #30167, incorrect line numbers in an infinite loop
    go_(() => () => {
    }());

    while (true) {
    }
}

private static void Main() {
    test();
}

} // end main_package
