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

internal static void Main() {
    CaseRange cr = default!;
    cr.Lo = 65;
    cr.Item = new inner(v: 9);
    cr.Lvl = 3;
    fmt.Println(cr.Lo);
    fmt.Println(cr.Delta[0]);
    fmt.Println(cr.Item.v);
    fmt.Println(cr.Lvl);
}

} // end main_package
