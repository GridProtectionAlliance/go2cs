namespace go;

using fmt = fmt_package;
using crosspkglib = crosspkglib_package;

partial class main_package {

internal static void Main() {
    var b = crosspkglib.Boiling();
    var r = b.Add(10);
    fmt.Println(((float64)b));
    fmt.Println(((float64)r));
    crosspkglibꓸTemperature t = crosspkglib.Freezing();
    t = t.Add(32);
    fmt.Println(((float64)t));
}

} // end main_package
