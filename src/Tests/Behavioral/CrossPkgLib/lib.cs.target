global using Temperature = go.CrossPkgLib_package.Celsius;

namespace go;

partial class CrossPkgLib_package {

public static readonly UntypedInt Precision = 2;

public static readonly UntypedInt Sep = /* ':' */ 58;

[GoType("num:float64")] partial struct Celsius;

public static Celsius Boiling() {
    return 100;
}

public static Temperature Freezing() {
    return 0;
}

public static Celsius Add(this Celsius c, Celsius d) {
    return c + d;
}

[GoType] partial struct Sensor {
    public @string Name;
    public Celsius Temp;
}

public static bool Hot(this Sensor s) {
    return s.Temp > 50;
}

public static @string Label(this Sensor s) {
    return s.Name;
}

[GoRecv] public static void Calibrate(this ref Sensor s, Celsius d) {
    s.Temp += d;
}

[GoType] partial interface Labeled {
    @string Label();
}

internal static Labeled _ᴛ1ʗ = new Sensor(nil);

public static @string Describe(Labeled l) {
    return l.Label();
}

[GoType] partial struct Meter {
    internal nint count;
}

[GoRecv] public static nint Bump(this ref Meter m) {
    m.count++;
    return m.count;
}

public static ж<Meter> NewMeter() {
    return Ꮡ(new Meter(nil));
}

[GoType] partial struct ΔStatus {
    public nint Code;
}

public static nint Status(this Sensor s) {
    return (nint)(float64)s.Temp;
}

[GoType("num:nint")] partial struct ΔGrade;

public static nint Grade(this Sensor s) {
    return 1;
}

[GoType] public partial struct snapshot {
    public nint At;
}

public static snapshot Latest = new snapshot(At: 42);

public static snapshot Peek() {
    return Latest;
}

[GoType("num:uintptr")] partial struct Ticks;

[GoType] partial struct Device {
    public partial ref Sensor Sensor { get; }
    public nint Serial;
}

} // end CrossPkgLib_package
