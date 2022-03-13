// package main -- go2cs converted at 2022 March 13 06:22:53 UTC
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ssa\testdata\i22600.go
namespace go;

using fmt = fmt_package;
using os = os_package;

public static partial class main_package {

private static void test() {
    var (pwd, err) = os.Getwd();
    if (err != null) {
        fmt.Println(err);
        os.Exit(1);
    }
    fmt.Println(pwd);
}

private static void Main() {
    growstack(); // Use stack early to prevent growth during test, which confuses gdb
    test();
}

private static @string snk = default;

//go:noinline
private static void growstack() {
    snk = fmt.Sprintf("%#v,%#v,%#v", 1, true, "cat");
}

} // end main_package
