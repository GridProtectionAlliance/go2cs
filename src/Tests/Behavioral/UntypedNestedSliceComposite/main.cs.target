namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:nint")] partial struct rank;

internal static readonly rank rA = /* iota */ 0;
internal static readonly rank rB = 1;
internal static readonly rank rC = 2;

internal static slice<slice<rank>> order = new golib.SparseArray<slice<rank>>{
    [(int)rA] = new rank[]{}.slice(),
    [(int)rB] = new rank[]{rA}.slice(),
    [(int)rC] = new rank[]{rA, rB}.slice()
}.slice();

internal static slice<array<nint>> grid = new array<nint>[]{
    new nint[]{1, 2}.array(),
    new nint[]{3, 4}.array()
}.slice();

[GoType] partial struct dbgVar {
    internal @string name;
    internal ж<int32> value;
}

internal static ж<int32> Ꮡx = new(7);
internal static ref int32 x => ref Ꮡx.Value;

internal static slice<ж<dbgVar>> dbgvars = new ж<dbgVar>[]{
    Ꮡ(new dbgVar(name: "a"u8, value: Ꮡx)),
    Ꮡ(new dbgVar(name: "b"u8, value: Ꮡx))
}.slice();

internal static void Main() {
    fmt.Println(len(order[rA]), len(order[rB]), len(order[rC]));
    fmt.Println(order[rC][0], order[rC][1]);
    fmt.Println(grid[0][1], grid[1][0]);
    fmt.Println((~dbgvars[0]).name, (~dbgvars[1]).value.Value, len(dbgvars));
}

} // end main_package
