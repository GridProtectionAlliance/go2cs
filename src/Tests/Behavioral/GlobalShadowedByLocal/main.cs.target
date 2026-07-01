namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct snapshot {
    internal nint addr;
}

internal static snapshot Δtrace = new snapshot(addr: 42);

[GoType] partial struct acquirer {
    internal nint n;
}

internal static nint trace(this acquirer _) {
    return 1;
}

internal static nint collisionGlobalShadow() {
    nint a = main_package.Δtrace.addr;
    nint Δtrace = 7;
    return a + Δtrace;
}

internal static nint plainCounter = 100;

internal static nint plainGlobalShadow() {
    nint x = main_package.plainCounter * 2;
    nint plainCounter = 5;
    return x + plainCounter;
}

internal static void Main() {
    fmt.Println(collisionGlobalShadow());
    fmt.Println(plainGlobalShadow());
    fmt.Println(Δtrace.addr, plainCounter);
}

} // end main_package
