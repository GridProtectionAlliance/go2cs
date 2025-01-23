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
}

[GoRecv] public static float64 Abs(this ref MyCustomError myErr) {
    return 0.0F;
}

private static void Main() {
    var a = new MyCustomError("New One", default!, new MyError(time.Now(), "Hello"), default!);
    a.Abs();
    a.Message = "New"u8;
    fmt.Println("MyCustomError method =", a.Abs());
}

} // end main_package
