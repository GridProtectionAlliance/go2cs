namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:float32")] partial struct main_meters;

internal static void Main() {
    complex64 c64 = complex(2.5F, -3.5F);
    complex64 c64b = 2.5F - builtin.i(3.5F);
    float32 a = 2.5F;
    float32 b = -3.5F;
    float32 nested = -(1.5F + 2.0F);
    float32 quo = 7.0F / 2.0F;
    complex128 c128 = 2.5D - builtin.i(3.5D);
    complex128 prec = 0.1D + builtin.i(0.1D);
    float32 minf = min(1.5F, -2.5F);
    float32 maxf = max(1.5F, -2.5F);
    float32 re = real(2.5F - builtin.i(3.5F));
    float32 im = imag(complex(2.5F, -3.5F));
    main_meters dist = -1.5F;
    fmt.Println(real(c64), imag(c64));
    fmt.Println(real(c64b), imag(c64b));
    fmt.Println(a, b);
    fmt.Println(nested);
    fmt.Println(quo);
    fmt.Println(real(c128), imag(c128));
    fmt.Println(real(prec), imag(prec));
    fmt.Println(minf, maxf);
    fmt.Println(re, im);
    fmt.Println(dist);
}

} // end main_package
