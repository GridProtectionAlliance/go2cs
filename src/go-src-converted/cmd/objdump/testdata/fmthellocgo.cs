// package main -- go2cs converted at 2022 March 06 23:22:37 UTC
// Original source: C:\Program Files\Go\src\cmd\objdump\testdata\fmthellocgo.go
using fmt = go.fmt_package;
using C = go.C_package;

namespace go;

public static partial class main_package {

private static void Main() {
    Println("hello, world");
    if (flag) {
        //line fmthello.go:999999
        Println("bad line");
        while (true)         }

    }
}

//go:noinline
public static void Println(@string s) {
    fmt.Println(s);
}

private static bool flag = default;

} // end main_package
