namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("dyn")] partial struct Stats_BySize {
    public uint32 Size;
    public uint64 Count;
}

[GoType] partial struct Stats {
    public nint Total;
    public array<Stats_BySize> BySize = new(3);
}


[GoType("dyn")] partial struct poolᴛ1 {
    internal nint item;
    internal array<byte> pad = new(4);
}
internal static array<poolᴛ1> pool = new(2);

internal static array<nint> nums = new(3);

internal static ж<array<nint>> Ꮡaddr = new(new array<nint>(2));
internal static ref array<nint> addr => ref Ꮡaddr.val;

internal static nint statsTotal() {
    var s = new Stats(Total: 42);
    return s.Total;
}

internal static void Main() {
    pool[0].item = 5;
    pool[1].item = 6;
    nums[0] = 11;
    nums[2] = 13;
    var p = Ꮡaddr;
    p.val[0] = 21;
    p.val[1] = 22;
    fmt.Println(pool[0].item, pool[1].item);
    fmt.Println(nums[0], nums[2], addr[0], addr[1]);
    fmt.Println(statsTotal());
}

} // end main_package
