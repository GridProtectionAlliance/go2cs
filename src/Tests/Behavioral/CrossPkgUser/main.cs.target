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
    var c = new counter(Meter: CrossPkgLib.NewMeter());
    fmt.Println(c.Meter.Value.Bump());
    ΔMeter m = c;
    fmt.Println(m.Bump());
    fmt.Println(c.Meter.Value.Bump());
    fmt.Println(g.Meter());
    var rd = ((reading)CrossPkgLib.Boiling());
    var cback = ((CrossPkgLib.Celsius)rd);
    fmt.Println((float64)cback, cback == CrossPkgLib.Boiling());
    var (st1, err1) = stampOrErr(false);
    var (st2, err2) = stampOrErr(true);
    fmt.Println(st1 == st2, err1 != default!, st2 == bigStamp, err2 == default!);
    var rg = new rig(Device: new CrossPkgLib.Device(Sensor: new CrossPkgLib.Sensor(Name: "deep"u8, Temp: 55), Serial: 9), id: 1);
    fmt.Println((float64)rg.Temp, rg.Serial, rg.id);
    rg.Temp = 66;
    fmt.Println((float64)rg.Device.Sensor.Temp);
    fmt.Println(rg.Device.Sensor.Hot());
    Ꮡ(rg).of(rig.ᏑDevice).of(CrossPkgLib.Device.ᏑSensor).Calibrate(3);
    fmt.Println((float64)rg.Device.Sensor.Temp);
    var exprᴛ1 = CrossPkgLib.Precision;
    if (exprᴛ1 == 1) {
        fmt.Println("coarse");
    }
    else if (exprᴛ1 == 2) {
        fmt.Println("fine");
    }
    else { /* default: */
        fmt.Println("unknown");
    }

    fmt.Println("a" + ((@string)(rune)CrossPkgLib.Sep) + "b");
}

[GoType("CrossPkgLib_package.Celsius")] partial struct reading;

[GoType("CrossPkgLib_package.Ticks")] partial struct stamp;

internal static readonly stamp bigStamp = unchecked((stamp)(CrossPkgLib.Ticks)0x80000001);

internal static (stamp, error) stampOrErr(bool ok) {
    if (!ok) {
        return ((stamp)(CrossPkgLib.Ticks)(0), fmt.Errorf("no stamp"u8));
    }
    return ((stamp)(CrossPkgLib.Ticks)(bigStamp), default!);
}

[GoType] partial struct probe {
    public partial ref ж<CrossPkgLib_package.Sensor> Sensor { get; }
    internal nint id;
}

[GoType] partial struct tagged {
    public partial ref CrossPkgLib_package.Sensor Sensor { get; }
    internal nint n;
}

[GoType] partial interface ΔMeter {
    nint Bump();
}

internal static @string Meter(this tagged t) {
    return "tagged-meter"u8;
}

[GoType] partial struct counter {
    public partial ref ж<CrossPkgLib_package.Meter> Meter { get; }
}

[GoType] partial struct rig {
    public partial ref CrossPkgLib_package.Device Device { get; }
    internal nint id;
}

} // end main_package
