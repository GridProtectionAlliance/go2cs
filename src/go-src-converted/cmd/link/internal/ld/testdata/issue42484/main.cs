// package main -- go2cs converted at 2022 March 13 06:35:38 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\issue42484\main.go
namespace go;

using fmt = fmt_package;

public static partial class main_package {

private static void Main() {
    nint a = 0;
    a++;
    nint b = 0;
    f1(a, b);
}

private static void f1(nint a, nint b) {
    fmt.Printf("%d %d\n", a, b);
}

} // end main_package
