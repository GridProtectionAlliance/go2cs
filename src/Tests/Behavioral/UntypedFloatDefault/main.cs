namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var z = 1.0D;
    float64 x = 3;
    while (x >= 1) {
        z = z * x;
        x = x - 1;
    }
    fmt.Println(z);
    float32 f = 2.5F;
    f = f * 2;
    fmt.Println(f);
}

} // end main_package
