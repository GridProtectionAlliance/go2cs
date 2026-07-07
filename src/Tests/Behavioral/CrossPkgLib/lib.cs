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

public static Labeled LabeledOf(ж<Sensor> Ꮡs) {
    ref var s = ref Ꮡs.Value;

    return new SensorжLabeled(Ꮡs);
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

[GoType] partial interface Reporter {
    @string Report();
}

[GoRecv] public static @string Report(this ref Meter m) {
    return "count"u8;
}

[GoType] partial struct Alarm {
    public @string Msg;
}

[GoRecv] public static @string Error(this ref Alarm a) {
    return a.Msg;
}

public static error AsErr(ж<Alarm> Ꮡa) {
    ref var a = ref Ꮡa.Value;

    return new Alarmжerror(Ꮡa);
}

public static Reporter AsReporter(ж<Meter> Ꮡm) {
    ref var m = ref Ꮡm.Value;

    return new MeterжReporter(Ꮡm);
}

[GoType] partial struct Cache<T> {
    public nint Hits;
}

[GoRecv] public static nint Bump<T>(this ref Cache<T> c) {
    c.Hits++;
    return c.Hits;
}

public static slice<T> Wrap<T>(T v) {
    return new T[]{v}.slice();
}

public static V Pair<K, V>(K k, V v)
    where K : /* comparable */ new()
{
    return v;
}

// type ΔSift is a methodless func type — rendered inline as its base delegate

public static bool Sift(this Sensor s, Func<nint, bool> f) {
    return f((nint)(float64)s.Temp);
}

[GoType] partial struct Probe {
    public nint Hits;
}

[GoRecv] public static nint Sample(this ref Probe p) {
    p.Hits++;
    return p.Hits;
}

[GoType] partial interface Sampler {
    nint Sample();
}

[GoType] partial interface Sealed {
    @string Label();
    @string Seal();
}

[GoType] partial interface Rated {
    @string Label();
    nint Rating();
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

[GoType] partial struct ΔMarker {
    public @string ΔΔMarker;
}

public static @string Marker(this Sensor s) {
    return s.Name;
}

public static ΔMarker MakeMarker(@string s) {
    return new ΔMarker(ΔΔMarker: s);
}

} // end CrossPkgLib_package
