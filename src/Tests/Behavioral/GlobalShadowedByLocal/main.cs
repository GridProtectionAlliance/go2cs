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
    nint traceΔ1 = 7;
    return a + traceΔ1;
}

internal static nint plainCounter = 100;

internal static nint plainGlobalShadow() {
    nint x = main_package.plainCounter * 2;
    nint plainCounterΔ1 = 5;
    return x + plainCounterΔ1;
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

internal static map<@string, slice<@string>> hosts = new map<@string, slice<@string>>{["a"u8] = new @string[]{"x"u8, "y"u8}.slice()};

internal static nint tupleInitShadow(@string key) {
    {
        var (hostsΔ1, ok) = hosts[key, ꟷ]; if (ok) {
            return len(hostsΔ1);
        }
    }
    return -1;
}

internal static void Main() {
    fmt.Println(collisionGlobalShadow());
    fmt.Println(plainGlobalShadow());
    fmt.Println(Δtrace.addr, plainCounter);
    fmt.Println(nestedBlockShadow(2), nestedBlockShadow(1));
    fmt.Println(tupleInitShadow("a"u8), tupleInitShadow("z"u8));
}

} // end main_package
