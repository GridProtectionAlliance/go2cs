namespace go;

using fmt = fmt_package;

public static partial class main_package {

[GoType("struct")]
public partial struct Buffer {
    public slice<byte> buf;
    public nint off;
    public int8 lastRead;
}

private const int8 opRead = -1;
private const int8 opInvalid = 0;
private static void Main() {
    ref var a = ref heap(new nint(), out var Ꮡa);
    ptr<nint> ptr = default;
    ptr<ptr<nint>> pptr = default;
    ptr<ptr<ptr<nint>>> ppptr = default;
    a = 3000;
    ptr = Ꮡa;
    pptr = addr(ptr);
    ppptr = addr(pptr);
    fmt.Printf("Value of a = %d\n"u8, a);
    PrintValPtr(ptr);
    fmt.Printf("Main-function return value available at *ptr = %d\n"u8, EscapePrintValPtr(ptr).val);
    fmt.Printf("Main-function updated value available at *ptr = %d\n"u8, ptr.val);
    PrintValPtr2Ptr(pptr);
    PrintValPtr2Ptr2Ptr(ppptr);
    a = 1900;
    fmt.Printf("Value of a = %d\n"u8, a);
    PrintValPtr(ptr);
    fmt.Printf("Main-function return value available at *ptr = %d\n"u8, EscapePrintValPtr(ptr).val);
    fmt.Printf("Main-function updated value available at *ptr = %d\n"u8, ptr.val);
    PrintValPtr2Ptr(pptr);
    PrintValPtr2Ptr2Ptr(ppptr);
    ref var b = ref heap<Buffer>(out var Ꮡb);
    b = new Buffer();
    PrintValPtr(Ꮡb.of(Buffer.Ꮡoff));
    PrintValPtr(Ꮡb.of(Buffer.Ꮡoff));
}

public static (nint n, error err) Read(this ptr<Buffer> Ꮡb, slice<byte> p) {
    nint n = default;
    error err = default;

    ref var b = ref Ꮡb.val;
    b.lastRead = opInvalid;
    b.off += n;
    if (n > 0) {
        b.lastRead = opRead;
    }
    (addr(new Buffer(buf: p))).Read(p);
    return (n, error.As(null));
}

public static ptr<Buffer> /*b1*/ NewBuffer(slice<byte> buf) {
    ptr<Buffer> b1 = default;

    return addr(new Buffer(buf: buf));
}

public static void PrintValPtr(ptr<nint> Ꮡptr) {
    ref var ptr = ref Ꮡptr.val;

    fmt.Printf("Value available at *ptr = %d\n"u8, ptr);
    ptr++;
}

public static ptr<nint> EscapePrintValPtr(ptr<nint> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    fmt.Printf("Value available at *ptr = %d\n"u8, @out);
    ref var i = ref heap<nint>(out var Ꮡi);
    i = 99;
    Ꮡout = Ꮡi;
    fmt.Printf("Intra-function updated value available at *ptr = %d\n"u8, @out);
    PrintValPtr(Ꮡout);
    return Ꮡout;
}

public static void PrintValPtr2Ptr(ptr<ptr<nint>> Ꮡpptr) {
    ref var pptr = ref Ꮡpptr.val;

    fmt.Printf("Value available at **pptr = %d\n"u8, pptr.val);
}

public static void PrintValPtr2Ptr2Ptr(ptr<ptr<ptr<nint>>> Ꮡppptr) {
    ref var ppptr = ref Ꮡppptr.val;

    fmt.Printf("Value available at ***pptr = %d\n"u8, ppptr.val.val);
}

} // end main_package
