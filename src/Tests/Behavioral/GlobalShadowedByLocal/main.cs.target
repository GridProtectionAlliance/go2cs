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

internal static nint acquire(nint n) {
    return n * 10;
}

internal static nint nestedBlockShadow(nint kind) {
    nint total = 0;
    nint Δtrace = acquire(1);
    total += Δtrace;
    if (kind == 0){
        total = -1;
    } else {
        if (kind > 1) {
            nint traceΔ1 = acquire(2);
            total += traceΔ1;
        }
        nint traceΔ2 = acquire(3);
        total += traceΔ2;
    }
    total += Δtrace;
    return total;
}

internal static void Main() {
    fmt.Println(collisionGlobalShadow());
    fmt.Println(plainGlobalShadow());
    fmt.Println(Δtrace.addr, plainCounter);
    fmt.Println(nestedBlockShadow(2), nestedBlockShadow(1));
}

} // end main_package
