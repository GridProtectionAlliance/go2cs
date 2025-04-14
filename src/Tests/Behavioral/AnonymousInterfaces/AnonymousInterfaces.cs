namespace go;

using fmt = fmt_package;
using io = io_package;

partial class main_package {

[GoType("runtime")] partial interface testTypeSwitch_type {
    error Unwrap();
}

// 1. Type Switch using inline interface
internal static void testTypeSwitch(error err) {
    switch (err.type()) {
    case {} x when Implements<testTypeSwitch_type>(x):
        var x1 = testTypeSwitch_type.As(x);
        fmt.Println("TypeSwitch: Unwrap =", x1.Unwrap());
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
func takesReader(r interface{ Read([]byte) (int, error) }) {
	buf := make([]byte, 4)
	n, _ := r.Read(buf)
	fmt.Println("FuncParam: Read =", string(buf[:n]))
}

// 4. Composite literal with inline interface
func testCompositeLiteral() {
	readers := []interface{ Read([]byte) (int, error) }{fakeReader{}}
	buf := make([]byte, 4)
	n, _ := readers[0].Read(buf)
	fmt.Println("CompositeLiteral: Read =", string(buf[:n]))
}

// 5. Struct field with inline interface type
type WithInlineField struct {
	R interface{ Read([]byte) (int, error) }
}

func testInlineField() {
	s := WithInlineField{R: fakeReader{}}
	buf := make([]byte, 4)
	n, _ := s.R.Read(buf)
	fmt.Println("InlineField: Read =", string(buf[:n]))
}

// 6. Interface embedding inline interface
type InlineEmbed interface {
	interface{ Close() error }
	Flush() error
}

type embeddedImpl struct{}

func (embeddedImpl) Close() error { return nil }
func (embeddedImpl) Flush() error { return nil }

func testInterfaceEmbedding(x InlineEmbed) {
	_ = x.Close()
	_ = x.Flush()
	fmt.Println("InterfaceEmbed: Close and Flush OK")
}
*/
// Supporting types
[GoType] partial struct fakeReader {
}

internal static (nint, error) Read(this fakeReader Δ_, slice<byte> b) {
    copy(b, "DATA"u8);
    return (4, default!);
}

[GoType] partial struct fakeError {
}

internal static @string Error(this fakeError Δ_) {
    return "fake error"u8;
}

internal static error Unwrap(this fakeError Δ_) {
    return io.EOF;
}

internal static bool Is(this fakeError Δ_, error err) {
    return AreEqual(err, io.EOF);
}

internal static void Main() {
    testTypeSwitch(new fakeError(nil));
    testTypeAssertion(new fakeError(nil));
}

//takesReader(fakeReader{})
//testCompositeLiteral()
//testInlineField()
//testInterfaceEmbedding(embeddedImpl{})

} // end main_package
