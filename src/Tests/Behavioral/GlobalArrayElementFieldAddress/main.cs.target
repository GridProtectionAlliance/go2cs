namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct sub {
    internal nint mu, n;
}

[GoType] partial struct item {
    internal sub inner;
}

internal static ж<array<item>> Ꮡpool = new(new array<item>(3));
internal static ref array<item> pool => ref Ꮡpool.val;


[GoType("dyn")] partial struct gridᴛ1 {
    internal sub cell;
    internal array<byte> pad = new(4);
}
internal static ж<array<gridᴛ1>> Ꮡgrid = new(new array<gridᴛ1>(3));
internal static ref array<gridᴛ1> grid => ref Ꮡgrid.val;

internal static void setInt(ж<nint> Ꮡp) {
    ref var p = ref Ꮡp.val;

    p = 7;
}

internal static void Main() {
    pool[1].inner.n = 5;
    setInt(Ꮡpool.at<item>(1).of(item.Ꮡinner).of(sub.Ꮡmu));
    setInt(Ꮡpool.at<item>(2).of(item.Ꮡinner).of(sub.Ꮡmu));
    grid[0].cell.n = 9;
    setInt(Ꮡgrid.at<gridᴛ1>(0).of(gridᴛ1.Ꮡcell).of(sub.Ꮡmu));
    fmt.Println(pool[1].inner.mu, pool[1].inner.n, pool[2].inner.mu);
    fmt.Println(grid[0].cell.mu, grid[0].cell.n);
}

} // end main_package
