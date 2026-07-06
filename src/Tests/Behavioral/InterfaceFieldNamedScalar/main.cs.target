namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:nint")] partial struct myErr;

internal static @string Error(this myErr e) {
    return fmt.Sprintf("myErr(%d)"u8, (nint)e);
}

[GoType("@string")] partial struct tag;

internal static @string Name(this tag t) {
    return "tag:"u8 + ((@string)t);
}

[GoType] partial interface named {
    @string Name();
}

[GoType] partial struct box {
    internal error err;
}

[GoType] partial struct holder {
    internal named n;
}

internal static void Main() {
    var b = new box(((myErr)7));
    fmt.Println(b.err.Error());
    var b2 = new box(err: ((myErr)42));
    fmt.Println(b2.err.Error());
    var h = new holder(((tag)(@string)"x"u8));
    fmt.Println(h.n.Name());
}

} // end main_package
