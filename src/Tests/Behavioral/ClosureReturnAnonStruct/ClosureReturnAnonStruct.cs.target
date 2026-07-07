namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("dyn")] partial struct closureReturningAnonStruct_func_R0 {
    internal @string name;
    internal nint size;
}

[GoType("dyn")] partial struct closureReturningAnonStruct_type {
    internal @string name;
    internal nint size;
}

internal static void closureReturningAnonStruct() {
    var makeEntry = (@string name, nint size) => new closureReturningAnonStruct_type(name, size);
    var e1 = makeEntry("alpha"u8, 10);
    var e2 = makeEntry("beta"u8, 20);
    fmt.Printf("entries: %s=%d %s=%d total=%d\n"u8, e1.name, e1.size, e2.name, e2.size, e1.size + e2.size);
}

internal static void Main() {
    closureReturningAnonStruct();
}

} // end main_package
