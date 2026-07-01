namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void run() {
    var arr = new nint[]{10, 11, 12, 13, 14, 15, 16, 17}.array();
    var sl = new nint[]{20, 21, 22, 23, 24, 25}.slice();
    uintptr n = 5;
    var a = arr.slice(-1, (int)(n), (int)(n));
    uintptr lo = 1;
    var b = arr.slice((int)(lo), (int)(n), (int)(n));
    nuint u = 4;
    var c = sl.slice(-1, (int)(u), (int)(u));
    uint64 q = 3;
    var d = sl.slice((int)(q), (int)(q + 2), (int)(q + 3));
    nint k = 6;
    var e = arr.slice(-1, k, k);
    fmt.Println(len(a), cap(a), a[0], a[4]);
    fmt.Println(len(b), cap(b), b[0]);
    fmt.Println(len(c), cap(c), len(d), cap(d));
    fmt.Println(len(e), cap(e));
}

internal static void Main() {
    run();
}

} // end main_package
