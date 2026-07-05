namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[3]rune")] public partial struct d;

[GoType] public partial struct inner {
    internal nint v;
}

[GoType("num:nint")] public partial struct level;

[GoType] partial struct CaseRange {
    public uint32 Lo;
    public d Delta;
    public inner Item;
    public level Lvl;
}

[GoType] public partial struct coder {
    internal @string tag;
    internal nint seq;
}

[GoType("coder")] partial struct EncBuffer;

[GoType("num:nint")] public partial struct tally;

[GoType("num:nint")] public partial struct weight;

public static tally Tally(this CaseRange cr) {
    return ((tally)(nint)cr.Lo);
}

public static nint Weigh(this CaseRange cr, weight w) {
    return (nint)w * 2;
}

internal static void Main() {
    CaseRange cr = default!;
    cr.Lo = 65;
    cr.Item = new inner(v: 9);
    cr.Lvl = ((level)3);
    fmt.Println(cr.Lo);
    fmt.Println(cr.Delta[0]);
    fmt.Println(cr.Item.v);
    fmt.Println(cr.Lvl);
    var b = new EncBuffer(new coder(tag: "png"u8, seq: 7));
    fmt.Println(b.tag, b.seq);
    fmt.Println(cr.Tally(), cr.Weigh(3));
}

} // end main_package
