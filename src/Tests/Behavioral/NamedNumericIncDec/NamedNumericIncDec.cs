namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:nuint")] partial struct idx;

[GoType("num:nint")] partial struct sidx;

internal static void Main() {
    nint n = 0;
    for (idx c = 0; c < 5; c++) {
        n++;
    }
    fmt.Println(n);
    nint m = 0;
    for (idx c = 4; c > 0; c--) {
        m++;
    }
    fmt.Println(m);
    sidx d = 3;
    d++;
    d++;
    d--;
    fmt.Println(d == 4);
}

} // end main_package
