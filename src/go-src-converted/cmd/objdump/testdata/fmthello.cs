// package main -- go2cs converted at 2022 March 13 06:35:41 UTC
// Original source: C:\Program Files\Go\src\cmd\objdump\testdata\fmthello.go
namespace go;

using fmt = fmt_package;

public static partial class main_package {

private static void Main() {
    Println("hello, world");
    if (flag) {
        //line fmthello.go:999999
        Println("bad line");
        while (true) {
        }
    }
}

//go:noinline
public static void Println(@string s) {
    fmt.Println(s);
}

private static bool flag = default;

} // end main_package
