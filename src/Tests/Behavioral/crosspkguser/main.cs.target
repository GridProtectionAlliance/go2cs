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
    var s = new crosspkglib.Sensor(Name: "kitchen"u8, Temp: crosspkglib.Boiling());
    fmt.Println(s.Name, ((float64)s.Temp), s.Hot());
    crosspkglib.Labeled l = s;
    fmt.Println(l.Label());
    fmt.Println(crosspkglib.Describe(s));
}

} // end main_package
