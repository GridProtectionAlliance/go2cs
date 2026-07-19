namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly UntypedFloat gHalfPi = 1.5707963267948966;

internal static void Main() {
    var huge = new complex128[]{34359738368D, 1766847064778384329583297500742918515827483896875618958121606201292619776D, -1329227995784915872903807060280344576D, 1357421751691302484408532992D}.slice();
    foreach (var (_, h) in huge) {
        fmt.Println(real(h));
    }
    var c = complex(0D, gHalfPi);
    fmt.Println(real(c), imag(c));
    float64 q = 7 / 2;
    var e = complex(7 / 2, 0D);
    fmt.Println(q, real(e), imag(e));
    fmt.Println(gHalfPi / 2D);
}

} // end main_package
