using fmt = go.fmt_package;

namespace go;

public static partial class main_package {

public partial struct MyError {
    public @string description;
}

public static @string Error(this MyError err) {
    return fmt.Sprintf("error: %s", err.description);
}

// error is an interface - MyError is cast to error interface upon return
private static error f() {
    return error.As(new MyError("foo"))!;
}

private static void Main() {
    error err = default!;

    err = error.As(new MyError("bar"))!;

    fmt.Printf("%v %v\n", f(), err); // error: foo
}

} // end main_package
