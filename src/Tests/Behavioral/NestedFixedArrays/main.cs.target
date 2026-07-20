namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[4]byte")] partial struct row;

[GoType] partial struct inner {
    internal array<byte> b = new(3);
}

[GoType] partial struct holder {
    internal array<array<byte>> grid = new(2, () => new(3));
}

internal static array<array<byte>> gNested = new(2, () => new(4));

internal static array<array<array<byte>>> gDeep = new(2, () => new(3, () => new(4)));

internal static array<inner> gStructElem = new(2, () => new());

internal static void Main() {
    array<array<byte>> x = new(2, () => new(4));
    fmt.Println(len(x), len(x[0]), len(x[1]));
    x[1][2] = 7;
    x[0][3] = 9;
    fmt.Println(x[1][2], x[0][3], x[0][2]);
    array<array<array<byte>>> deep = new(2, () => new(3, () => new(4)));
    fmt.Println(len(deep), len(deep[1]), len(deep[1][2]));
    deep[1][2][3] = 5;
    fmt.Println(deep[1][2][3], deep[0][0][0]);
    holder h = new();
    fmt.Println(len(h.grid), len(h.grid[1]));
    h.grid[1][2] = 4;
    fmt.Println(h.grid[1][2], h.grid[0][2]);
    array<inner> se = new(2, () => new());
    fmt.Println(len(se), len(se[1].b));
    se[1].b[2] = 3;
    fmt.Println(se[1].b[2], se[0].b[2]);
    array<row> nr = new(2);
    fmt.Println(len(nr), len(nr[1]));
    nr[1][3] = 6;
    fmt.Println(nr[1][3], nr[0][3]);
    fmt.Println(len(gNested), len(gNested[1]));
    gNested[1][2] = 8;
    fmt.Println(gNested[1][2], gNested[0][2]);
    fmt.Println(len(gDeep), len(gDeep[1]), len(gDeep[1][2]));
    fmt.Println(len(gStructElem), len(gStructElem[1].b));
    array<array<byte>> share = new(3, () => new(2));
    share[0][0] = 1;
    share[1][0] = 2;
    share[2][0] = 3;
    fmt.Println(share[0][0], share[1][0], share[2][0]);
}

} // end main_package
