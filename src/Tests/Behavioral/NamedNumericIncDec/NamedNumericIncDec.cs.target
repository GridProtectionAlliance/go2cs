namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:nuint")] partial struct idx;

[GoType("num:nint")] partial struct sidx;

internal static void Main() {
    nint n = 0;
    for (idx c = ((idx)0); c < ((idx)5); c++) {
        n++;
    }
    fmt.Println(n);
    nint m = 0;
    for (idx c = ((idx)4); c > ((idx)0); c--) {
        m++;
    }
    fmt.Println(m);
    sidx d = 3;
    d++;
    d++;
    d--;
    fmt.Println(d == ((sidx)4));
}

} // end main_package
