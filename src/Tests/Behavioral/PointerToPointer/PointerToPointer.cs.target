namespace go;

using fmt = fmt_package;

public static partial class main_package {

public partial struct Buffer {
    public slice<byte> buf;
    public nint off;
    public int8 lastRead;
}

private static readonly int8 opRead = -1;
private static readonly int8 opInvalid = 0;

private static void Main() {
    ref nint a = ref heap(out ptr<nint> _addr_a);
    ptr<nint> ptr;
    ptr<ptr<nint>> pptr;
    ptr<ptr<ptr<nint>>> ppptr;

    a = 3000; 

    /* take the address of var */
    ptr = _addr_a; 

    /* take the address of ptr using address of operator & */
    pptr = addr(ptr);
    ppptr = addr(pptr); 

    /* take the value using pptr */
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
}

private static (nint, error) Read(this ptr<Buffer> _addr_b, slice<byte> p) {
    nint n = default;
    error err = default!;
    ref Buffer b = ref _addr_b.val;

    b.lastRead = opInvalid;
    b.off += n;
    if (n > 0) {
        b.lastRead = opRead;
    }
    (addr(new Buffer(buf:p))).Read(p);

    return (n, error.As(null!)!);
}

public static ptr<Buffer> NewBuffer(slice<byte> buf) {
    ptr<Buffer> b1 = default!;

    return addr(new Buffer(buf:buf));
}

public static void PrintValPtr(ptr<nint> _addr_ptr) {
    ref nint ptr = ref _addr_ptr.val;

    fmt.Printf("Value available at *ptr = %d\n"u8, ptr);
    ptr++;
}

public static ptr<nint> EscapePrintValPtr(ptr<nint> _addr_ptr) {
    ref nint ptr = ref _addr_ptr.val;

    fmt.Printf("Value available at *ptr = %d\n"u8, ptr);
    ref nint i = ref heap(99, out ptr<nint> _addr_i);
    _addr_ptr = _addr_i;
    ptr = ref _addr_ptr.val;
    fmt.Printf("Intra-function updated value available at *ptr = %d\n"u8, ptr);
    PrintValPtr(_addr_ptr);
    return _addr_ptr!;
}

public static void PrintValPtr2Ptr(ptr<ptr<nint>> _addr_pptr) {
    ref ptr<nint> pptr = ref _addr_pptr.val;

    fmt.Printf("Value available at **pptr = %d\n"u8, pptr.val);
}

public static void PrintValPtr2Ptr2Ptr(ptr<ptr<ptr<nint>>> _addr_ppptr) {
    ref ptr<ptr<nint>> ppptr = ref _addr_ppptr.val;

    fmt.Printf("Value available at ***pptr = %d\n"u8, ppptr.val.val);
}

} // end main_package
