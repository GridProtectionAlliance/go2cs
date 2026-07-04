namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

[GoType] partial interface Abser {
    float64 Abs();
}

[GoType] partial struct MyError {
    public time.Time When;
    public @string What;
}

[GoType] partial struct MyCustomError {
    public @string Message;
    public Abser Abser;
    public partial ref ж<MyError> MyError { get; }
}

[GoRecv] public static float64 Time(this ref MyCustomError myErr) {
    return 0.0D;
}

public static float64 Time(this MyError myErr) {
    return (float64)myErr.When.Unix();
}

[GoType] partial struct Device {
    internal @string name;
    internal nint hits;
}

public static ж<nint> Tag(this ж<Device> Ꮡd) {
    ref var d = ref Ꮡd.Value;

    return Ꮡd.of(Device.Ꮡhits);
}

[GoRecv] public static @string Describe(this ref Device d) {
    return d.name;
}

[GoType] partial interface Describer {
    @string Describe();
    ж<nint> Tag();
}

[GoType] partial struct deviceHandle {
    public partial ref ж<Device> Device { get; }
}

[GoType] partial struct Inner {
    public @string Value;
}

[GoType] partial struct Middle {
    public partial ref ж<Inner> Inner { get; }
}

[GoType] partial struct Outer {
    internal ж<ж<Inner>> ptr;
}

internal static void Main() {
    ref var e = ref heap<MyError>(out var Ꮡe);
    e = new MyError(time.Now(), "Hello");
    var a = new MyCustomError("New One", default!, Ꮡe);
    a.Message = "New"u8;
    a.What = "World"u8;
    fmt.Println("MyError What =", e.What);
    fmt.Println("MyCustomError What =", a.What);
    fmt.Println("MyCustomError method =", a.Time());
    var inner = Ꮡ(new Inner(Value: "hello"u8));
    var innerPtr = Ꮡ(inner);
    var middle = new Middle(Inner: inner);
    fmt.Println(middle.Value);
    var outer = new Outer(ptr: innerPtr);
    fmt.Println((~(outer.ptr.ValueSlot)).Value);
    var dev = Ꮡ(new Device(name: "sensor"u8, hits: 3));
    Describer dsc = new deviceHandle(Device: dev);
    fmt.Println(dsc.Describe());
    var p = dsc.Tag();
    p.Value = 7;
    fmt.Println((~dev).hits);
}

} // end main_package
