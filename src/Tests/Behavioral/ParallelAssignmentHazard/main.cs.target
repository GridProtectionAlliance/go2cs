namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var x = (uint64)0b1011010;
    (x, var frac) = ((x >> (int)(3)), (uint64)(x & 7));
    fmt.Println(x, frac);
    nint a = 10;
    nint b = 20;
    (a, b, nint c) = (b, a, a + b);
    fmt.Println(a, b, c);
    nint p = 1;
    nint q = 2;
    nint r = 3;
    (p, q, r, nint s) = (q, r, p, p + q + r);
    fmt.Println(p, q, r, s);
    nint m = 5;
    m = m + 1;
    nint n = 100;
    fmt.Println(m, n);
}

} // end main_package
