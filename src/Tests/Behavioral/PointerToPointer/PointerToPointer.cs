namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Buffer {
    public slice<byte> buf;
    public nint off;
    public int8 lastRead;
}

private const int8 opRead = -1;
private const int8 opInvalid = 0;
private static void Main() {
    ref var a = ref heap(new nint(), out var Ꮡa);
    ж<nint> ptr = default!;
    ж<ж<nint>> pptr = default!;
    ж<ж<ж<nint>>> ppptr = default!;
    a = 3000;
    ptr = Ꮡa;
    pptr = Ꮡ(ptr);
    ppptr = Ꮡ(pptr);
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
    b = new Buffer(nil);
    PrintValPtr(Ꮡb.of(Buffer.Ꮡoff));
    PrintValPtr(Ꮡb.of(Buffer.Ꮡoff));
}

[GoRecv] public static (nint n, error err) Read(this ref Buffer b, slice<byte> p) {
    nint n = default!;
    error err = default!;

    b.lastRead = opInvalid;
    b.off += n;
    if (n > 0) {
        b.lastRead = opRead;
    }
    (Ꮡ(new Buffer(buf: p))).Read(p);
    return (n, default!);
}

public static ж<Buffer> /*b1*/ NewBuffer(slice<byte> buf) {
    ж<Buffer> b1 = default!;

    return Ꮡ(new Buffer(buf: buf));
}

public static void PrintValPtr(ж<nint> Ꮡptr) {
    ref var ptr = ref Ꮡptr.val;

    fmt.Printf("Value available at *ptr = %d\n"u8, ptr);
    ptr++;
}

public static ж<nint> EscapePrintValPtr(ж<nint> Ꮡout) {
    ref var @out = ref Ꮡout.val;

    fmt.Printf("Value available at *ptr = %d\n"u8, @out);
    ref var i = ref heap<nint>(out var Ꮡi);
    i = 99;
    Ꮡout = Ꮡi; @out = ref Ꮡout.val;
    fmt.Printf("Intra-function updated value available at *ptr = %d\n"u8, @out);
    PrintValPtr(Ꮡout);
    return Ꮡout;
}

public static void PrintValPtr2Ptr(ж<ж<nint>> Ꮡpptr) {
    ref var pptr = ref Ꮡpptr.val;

    fmt.Printf("Value available at **pptr = %d\n"u8, pptr.val);
}

public static void PrintValPtr2Ptr2Ptr(ж<ж<ж<nint>>> Ꮡppptr) {
    ref var ppptr = ref Ꮡppptr.val;

    fmt.Printf("Value available at ***pptr = %d\n"u8, ppptr.val.val);
}

} // end main_package
