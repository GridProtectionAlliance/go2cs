namespace go;

using fmt = fmt_package;
using io = io_package;

partial class main_package {

[GoType] partial interface testTypeSwitch_type {
    error Unwrap();
}

// 1. Type Switch using inline interface
internal static void testTypeSwitch(error err) {
    switch (err.type()) {
    case {} x when Implements<testTypeSwitch_type>(x):
        testTypeSwitch_type x1 = testTypeSwitch_type.As(x)!;
        fmt.Println("TypeSwitch: Unwrap =", x1.Unwrap());
        break;
    default: {
        var x = err.type();
        fmt.Println("TypeSwitch: No match");
        break;
    }}
}

[GoType] partial interface testTypeAssertion_type {
    bool Is(error _);
}

// 2. Type Assertion using inline interface
internal static void testTypeAssertion(error err) {
    {
        var (x, ok) = err._<testTypeAssertion_type>(ᐧ); if (ok){
            fmt.Println("TypeAssertion: Is(nil) =", x.Is(default!));
        } else {
            fmt.Println("TypeAssertion: No match");
        }
    }
}

    /*
    // 3. Function parameter using inline interface
    internal static void takesReader(interface{Read(<>byte) (int, error)} r) {
        var buf = new slice<byte>(4);
        var (n, _) = r.Read(buf);
        fmt.Println("FuncParam: Read =", ((@string)(buf[..(int)(n)])));
    }

    // 4. Composite literal with inline interface
    internal static void testCompositeLiteral() {
        var readers = new interface{Read(<>byte) (int, error)}[]{new fakeReader(nil)}.slice();
        var buf = new slice<byte>(4);
        var (n, _) = readers[0].Read(buf);
        fmt.Println("CompositeLiteral: Read =", ((@string)(buf[..(int)(n)])));
    }

    // 5. Struct field with inline interface type
    [GoType] partial struct WithInlineField {
        public interface{Read(<>byte) (int, error)} R;
    }

    internal static void testInlineField() {
        var s = new WithInlineField(R: new fakeReader(nil));
        var buf = new slice<byte>(4);
        var (n, _) = s.R.Read(buf);
        fmt.Println("InlineField: Read =", ((@string)(buf[..(int)(n)])));
    }
    // 6. Interface embedding inline interface

        [GoType] partial interface type {
            error Close();
        }
    [GoType] partial interface InlineEmbed :
        type
    {
        error Flush();
    }

    [GoType] partial struct embeddedImpl {
    }

    internal static error Close(this embeddedImpl _) {
        return default!;
    }

    internal static error Flush(this embeddedImpl _) {
        return default!;
    }

    internal static void testInterfaceEmbedding(InlineEmbed x) {
        ref var _ = ref heap<>(out var Ꮡ_);
        _ = x.Close();
        ref var _ = ref heap<>(out var Ꮡ_);
        _ = x.Flush();
        fmt.Println("InterfaceEmbed: Close and Flush OK");
    }

    // Supporting types
    [GoType] partial struct fakeReader {
}

internal static (nint, error) Read(this fakeReader _, slice<byte> b) {
    copy(b, "DATA"u8);
    return (4, default!);
}
    */
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
    //takesReader(new fakeReader(nil));
    //testCompositeLiteral();
    //testInlineField();
    //testInterfaceEmbedding(new embeddedImpl(nil));
}

} // end main_package
