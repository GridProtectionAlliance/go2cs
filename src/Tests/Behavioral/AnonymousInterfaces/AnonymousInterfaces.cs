namespace go;

using fmt = fmt_package;
using io = io_package;

partial class main_package {

[GoType("dyn")] partial interface testTypeSwitch_type {
    error Unwrap();
}

internal static void testTypeSwitch(error err) {
    switch (err.type()) {
    case {} Δx when Δx._<testTypeSwitch_type>(out var x):
        fmt.Println("TypeSwitch: Unwrap =", x.Unwrap());
        break;
    default: {
        var x = err.type();
        fmt.Println("TypeSwitch: No match");
        break;
    }}
}

[GoType("dyn")] partial interface testTypeAssertion_type {
    bool Is(error _);
}

internal static void testTypeAssertion(error err) {
    {
        var (x, ok) = err._<testTypeAssertion_type>(ᐧ); if (ok){
            fmt.Println("TypeAssertion: Is(nil) =", x.Is(default!));
        } else {
            fmt.Println("TypeAssertion: No match");
        }
    }
}

[GoType("dyn")] partial interface takesReader_r {
    (nint, error) Read(slice<byte> _);
}

internal static void takesReader(takesReader_r r) {
    var buf = new slice<byte>(4);
    var (n, _) = r.Read(buf);
    fmt.Println("FuncParam: Read =", ((@string)(buf[..(int)(n)])));
}

[GoType] partial struct fakeReader {
}

internal static (nint, error) Read(this fakeReader _, slice<byte> b) {
    copy(b, "DATA"u8);
    return (4, default!);
}

[GoType] partial struct fakeError {
}

internal static @string Error(this fakeError _) {
    return "fake error"u8;
}

internal static error Unwrap(this fakeError _) {
    return io.EOF;
}

internal static bool Is(this fakeError _, error err) {
    return AreEqual(err, io.EOF);
}

internal static void Main() {
    testTypeSwitch(new fakeError(nil));
    testTypeAssertion(new fakeError(nil));
    takesReader(new fakeReader(nil));
}

} // end main_package
