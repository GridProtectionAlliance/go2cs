// package main -- go2cs converted at 2022 March 06 23:22:34 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\issue26237\main\main.go
using fmt = go.fmt_package;

using b = go.cmd.link.@internal.ld.testdata.issue26237.b.dir_package;

namespace go;

public static partial class main_package {

private static nint skyx = default;

private static void Main() {
    skyx += b.OOO(skyx);
    if (b.Top(1) == 99) {
        fmt.Printf("Beware the Jabberwock, my son!\n");
    }
}

} // end main_package
