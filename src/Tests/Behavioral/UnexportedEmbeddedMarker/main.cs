namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct noCopy {
}

[GoRecv] internal static void Lock(this ref noCopy _) {
}

[GoRecv] internal static void Unlock(this ref noCopy _) {
}

[GoType] partial struct Counter {
    internal noCopy _;
    internal int64 v;
}

[GoRecv] public static void Add(this ref Counter c, int64 n) {
    c.v += n;
}

[GoRecv] public static int64 Value(this ref Counter c) {
    return c.v;
}

internal static void Main() {
    Counter c = default!;
    c.Add(5);
    c.Add(3);
    fmt.Println("counter:", c.Value());
    Counter d = default!;
    fmt.Println("zero:", d.Value());
}

} // end main_package
