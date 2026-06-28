namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct Box<T>
    where T : new()
{
    internal T v;
}

[GoType] partial struct Pair<A, B>
    where A : new()
    where B : new()
{
    internal A a;
    internal B b;
}

internal static void Main() {
    Pair<nint, @string> p = default!;
    p.a = 5;
    p.b = "hi"u8;
    fmt.Println(p.a, p.b);
    Box<@string> b = default!;
    b.v = "boxed"u8;
    fmt.Println(b.v);
    Pair<@string, @string> sp = default!;
    sp.a = "x"u8;
    sp.b = "y"u8;
    fmt.Println(sp.a, sp.b);
    Pair<nint, Box<@string>> nested = default!;
    nested.a = 9;
    nested.b.v = "deep"u8;
    fmt.Println(nested.a, nested.b.v);
}

} // end main_package
