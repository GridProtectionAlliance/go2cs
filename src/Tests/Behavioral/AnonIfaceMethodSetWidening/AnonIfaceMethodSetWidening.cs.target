namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct gadget {
    internal nint n;
}

internal static @string Foo(this gadget g) {
    return fmt.Sprintf("foo %d"u8, g.n);
}

[GoRecv] internal static @string Bar(this ref gadget g) {
    return "bar"u8;
}

[GoType] partial interface fooer {
    @string Foo();
}

[GoType("dyn")] partial interface main_type {
    @string Foo();
    @string Bar();
}

[GoType("dyn")] partial interface main_typeᴛ1 {
    @string Foo();
    @string Bar();
}

internal static void Main() {
    fooer v = new gadget(1);
    {
        var (_, ok) = v._<main_type>(ᐧ); if (ok){
            fmt.Println("value-widened-wrong");
        } else {
            fmt.Println("value-not-widened-ok");
        }
    }
    fooer p = new gadgetжfooer(Ꮡ(new gadget(2)));
    {
        var (b, ok) = p._<main_typeᴛ1>(ᐧ); if (ok){
            fmt.Println("pointer-widened-ok", b.Foo(), b.Bar());
        } else {
            fmt.Println("pointer-not-widened-wrong");
        }
    }
}

} // end main_package
