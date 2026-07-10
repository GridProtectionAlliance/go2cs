namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct boxedResult {
    internal nint value;
    internal @string tag;
}

internal static void selectEscape() {
    var ch = new channel<boxedResult>(1);
    ch.ᐸꟷ(new boxedResult(value: 7, tag: "orig"u8));
    ж<boxedResult> saved = default!;
    switch (select(ᐸꟷ(ch, ꓸꓸꓸ))) {
    case 0 when ch.ꟷᐳ(out var resᴛ1): {
        ref var res = ref heap(resᴛ1, out var Ꮡres);
        saved = Ꮡres;
        saved.Value.value = 42;
        fmt.Println("escape:", res.value, res.tag);
        res.tag = "mutated"u8;
        break;
    }}
    fmt.Println("after:", (~saved).value, (~saved).tag);
}

internal static void selectEscapeCommaOk() {
    var ch = new channel<boxedResult>(1);
    ch.ᐸꟷ(new boxedResult(value: 3, tag: "ok-form"u8));
    ж<boxedResult> whole = default!;
    ж<nint> field = default!;
    switch (select(ᐸꟷ(ch, ꓸꓸꓸ))) {
    case 0 when ch.ꟷᐳ(out var resᴛ2, out var ok): {
        ref var res = ref heap(resᴛ2, out var Ꮡres);
        whole = Ꮡres;
        field = Ꮡres.of(boxedResult.Ꮡvalue);
        field.Value += 10;
        fmt.Println("comma-ok:", res.value, res.tag, ok);
        break;
    }}
    fmt.Println("field:", field.Value, (~whole).value);
}

internal static void selectEscapeMixed() {
    var a = new channel<boxedResult>(1);
    var b = new channel<nint>(1);
    a.ᐸꟷ(new boxedResult(value: 1, tag: "x"u8));
    ж<boxedResult> keep = default!;
    switch (select(ᐸꟷ(a, ꓸꓸꓸ), ᐸꟷ(b, ꓸꓸꓸ))) {
    case 0 when a.ꟷᐳ(out var rᴛ1): {
        ref var r = ref heap(rᴛ1, out var Ꮡr);
        keep = Ꮡr;
        keep.Value.tag = "escaped"u8;
        fmt.Println("mixed:", r.tag);
        break;
    }
    case 1 when b.ꟷᐳ(out var n): {
        fmt.Println("plain:", n);
        break;
    }}
}

internal static void Main() {
    selectEscape();
    selectEscapeCommaOk();
    selectEscapeMixed();
}

} // end main_package
