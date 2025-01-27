namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

[GoType] partial interface Abser {
    float64 Abs();
}

[GoType] partial struct MyError {
    public time_package.Time When;
    public @string What;
}

[GoType] partial struct MyCustomError {
    public @string Message;
    public Abser Abser;
    public partial ref ж<MyError> *MyError { get; }
}

private static void Main() {
    ref var e = ref heap<MyError>(out var Ꮡe);
    e = new MyError(time.Now(), "Hello");
    var a = new MyCustomError("New One", default!, Ꮡe);
    a.Message = "New"u8;
    a.What = "World"u8;
    fmt.Println("MyError What =", e.What);
    fmt.Println("MyCustomError What =", a.What);
}

} // end main_package
