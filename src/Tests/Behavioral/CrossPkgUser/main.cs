namespace go;

using fmt = fmt_package;
using CrossPkgLib = CrossPkgLib_package;

partial class main_package {

internal static void Main() {
    var b = CrossPkgLib.Boiling();
    var r = b.Add(10);
    fmt.Println((float64)b);
    fmt.Println((float64)r);
    CrossPkgLibꓸTemperature t = CrossPkgLib.Freezing();
    t = t.Add(32);
    fmt.Println((float64)t);
    var s = new CrossPkgLib.Sensor(Name: "kitchen"u8, Temp: CrossPkgLib.Boiling());
    fmt.Println(s.Name, (float64)s.Temp, s.Hot());
    CrossPkgLib.Labeled l = s;
    fmt.Println(l.Label());
    fmt.Println(CrossPkgLib.Describe(s));
}

} // end main_package
