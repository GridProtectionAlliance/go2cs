namespace go;

using fmt = fmt_package;
using io = io_package;

partial class main_package {

[GoType("runtime")] partial interface testTypeSwitch_type {
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

[GoType("runtime")] partial interface testTypeAssertion_type {
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
}

} // end main_package
