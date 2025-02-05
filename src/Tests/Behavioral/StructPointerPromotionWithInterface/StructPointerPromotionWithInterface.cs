namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

[GoType] partial interface abser {
    float64 Abs();
}

[GoType] partial struct MyError {
    public time_package.Time When;
    public @string What;
}

[GoType] partial struct MyCustomError {
    public @string Message;
    public abser Abser;
    public partial ref ж<MyError> MyError { get; }
}

[GoRecv] public static float64 Time(this ref MyCustomError myErr) {
    return 0.0F;
}

public static float64 Time(this MyError myErr) {
    return ((float64)myErr.When.Unix());
}

[GoType] partial struct Inner {
    public @string Value;
}

[GoType] partial struct Middle {
    public partial ref ж<Inner> Inner { get; }
}

[GoType] partial struct Outer {
    public ж<ж<Inner>> ptr;
}

private static void Main() {
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
    fmt.Println((outer.ptr.val.val).Value);
}

} // end main_package
