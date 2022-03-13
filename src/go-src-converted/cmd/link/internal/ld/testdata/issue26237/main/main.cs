// package main -- go2cs converted at 2022 March 13 06:35:38 UTC
// Original source: C:\Program Files\Go\src\cmd\link\internal\ld\testdata\issue26237\main\main.go
namespace go;

using fmt = fmt_package;

using b = cmd.link.@internal.ld.testdata.issue26237.b.dir_package;

public static partial class main_package {

private static nint skyx = default;

private static void Main() {
    skyx += b.OOO(skyx);
    if (b.Top(1) == 99) {
        fmt.Printf("Beware the Jabberwock, my son!\n");
    }
}

} // end main_package
