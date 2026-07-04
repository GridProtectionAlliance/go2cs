namespace go;

using fmt = fmt_package;
using Δio = io_package;

partial class main_package {

[GoType("dyn")] partial interface testTypeSwitch_type {
    error Unwrap();
}

internal static void testTypeSwitch(error err) {
    switch (err.type()) {
    case {} Δx when Δx._<testTypeSwitch_type>(out var x): {
        fmt.Println("TypeSwitch: Unwrap =", x.Unwrap());
        break;
    }
    default: {
        var x = err;
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

[GoType("dyn")] partial interface testCompositeLiteral_readers {
    (nint, error) Read(slice<byte> _);
}

internal static void testCompositeLiteral() {
    var readers = new testCompositeLiteral_readers[]{new fakeReader(nil)}.slice();
    var buf = new slice<byte>(4);
    var (n, _) = readers[0].Read(buf);
    fmt.Println("CompositeLiteral: Read =", ((@string)(buf[..(int)(n)])));
}

[GoType("dyn")] partial interface WithInlineField_R {
    (nint, error) Read(slice<byte> _);
}

[GoType] partial struct WithInlineField {
    public WithInlineField_R R;
}

internal static void testInlineField() {
    var s = new WithInlineField(R: new fakeReader(nil));
    var buf = new slice<byte>(4);
    var (n, _) = s.R.Read(buf);
    fmt.Println("InlineField: Read =", ((@string)(buf[..(int)(n)])));
}

[GoType("dyn")] partial interface Δtype {
    error Close();
}

[GoType] partial interface InlineEmbed :
    Δtype
{    error Flush();
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
    _ = x.Close();
    _ = x.Flush();
    fmt.Println("InterfaceEmbed: Close and Flush OK");
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
    return Δio.EOF;
}

internal static bool Is(this fakeError _, error err) {
    return AreEqual(err, Δio.EOF);
}

[GoType] partial struct tally {
    internal nint total;
}

[GoRecv] internal static (nint, error) Write(this ref tally t, slice<byte> p) {
    t.total += len(p);
    return (len(p), default!);
}

[GoType("dyn")] partial struct fill_dst {
    public io_package.Writer Writer;
}

internal static (int64, error) fill(this ж<tally> Ꮡt, Δio.Reader r) {
    ref var t = ref Ꮡt.Value;

    return Δio.Copy(new fill_dst(new tallyжWriter(Ꮡt)), r);
}

[GoType] partial struct byteRepeat {
    internal nint left;
}

[GoRecv] internal static (nint, error) Read(this ref byteRepeat b, slice<byte> p) {
    if (b.left == 0) {
        return (0, Δio.EOF);
    }
    nint n = 0;
    foreach (var (i, _) in p) {
        if (b.left == 0) {
            break;
        }
        p[i] = (rune)'x';
        b.left--;
        n++;
    }
    return (n, default!);
}

[GoType("[4]byte")] partial struct quad;

[GoType] partial struct frame {
    internal quad data;
}

internal static nint checksum(ж<frame> Ꮡf) {
    ref var f = ref Ꮡf.Value;

    nint s = 0;
    foreach (var (_, v) in f.data[..]) {
        s += (nint)v;
    }
    return s;
}

internal static void Main() {
    testTypeSwitch(new fakeError(nil));
    testTypeAssertion(new fakeError(nil));
    takesReader(new fakeReader(nil));
    testCompositeLiteral();
    testInlineField();
    testInterfaceEmbedding(new embeddedImpl(nil));
    var tl = Ꮡ(new tally(nil));
    var (n, err) = tl.fill(new byteRepeatжReader(Ꮡ(new byteRepeat(left: 10))));
    fmt.Println(n, err == default!, (~tl).total);
    ref var fr = ref heap<frame>(out var Ꮡfr);
    fr = new frame(data: new quad(new byte[]{1, 2, 3, 4}.array()));
    fmt.Println(checksum(Ꮡfr));
}

} // end main_package
