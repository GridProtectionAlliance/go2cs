namespace go;

using fmt = fmt_package;

public static partial class main_package {

[GoType("struct")]
public partial struct MyError {
    public @string description;
}

public static @string Error(this MyError err) {
    return fmt.Sprintf("error: %s"u8, err.description);
}

private static error f() {
    return error.As(new MyError("foo"u8));
}

private static void Main() {
    error err;

    err = error.As(new MyError("bar"u8));
    fmt.Printf("%v %v\n"u8, f(), err);
}

} // end main_package
