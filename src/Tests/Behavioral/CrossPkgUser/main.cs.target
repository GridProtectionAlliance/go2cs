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
    ref var s2 = ref heap<CrossPkgLib.Sensor>(out var Ꮡs2);
    s2 = new CrossPkgLib.Sensor(Name: "attic"u8, Temp: 20);
    var p = new probe(Sensor: Ꮡs2, id: 7);
    fmt.Println(p.Name, (float64)p.Temp, p.id);
    p.Temp = 75;
    fmt.Println((float64)s2.Temp, s2.Hot());
    p.Sensor.Value.Calibrate(5);
    fmt.Println((float64)s2.Temp);
    var g = new tagged(Sensor: new CrossPkgLib.Sensor(Name: "cellar"u8, Temp: 5), n: 3);
    fmt.Println(g.Name, (float64)g.Temp, g.n);
    g.Temp = 60;
    fmt.Println((float64)g.Temp, g.Sensor.Hot());
}

[GoType] partial struct probe {
    public partial ref ж<CrossPkgLib_package.Sensor> Sensor { get; }
    internal nint id;
}

[GoType] partial struct tagged {
    public partial ref CrossPkgLib_package.Sensor Sensor { get; }
    internal nint n;
}

} // end main_package
