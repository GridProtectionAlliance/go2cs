namespace go;

using fmt = fmt_package;
using Δmath = math_package;

partial class main_package {

internal static void Main() {
    complex128 c128 = 2 + 3D.i();
    complex128 neg = 2 - 3D.i();
    complex128 frac = complex(2.5D, -1.25D);
    complex128 realOnly = complex(5, 0);
    complex128 imagOnly = 3D.i();
    complex64 c64 = complex(2.5F, -3.5F);
    complex64 c64pos = 1 + 2F.i();
    var inf = complex(Δmath.Inf(1), Δmath.Inf(-1));
    var nan = complex(Δmath.NaN(), Δmath.NaN());
    fmt.Println(c128);
    fmt.Println(neg);
    fmt.Println(frac);
    fmt.Println(realOnly);
    fmt.Println(imagOnly);
    fmt.Println(c64);
    fmt.Println(c64pos);
    fmt.Println(inf);
    fmt.Println(nan);
    fmt.Println(2 + 3D.i());
    fmt.Printf("%v;%v\n"u8, c128, c64);
    fmt.Println("sprint:", fmt.Sprint(neg));
}

} // end main_package
