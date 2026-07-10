namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("map[@string, slice<@string>]")] partial struct header;

[GoType] partial struct box {
    internal slice<nint> data;
}

[GoRecv] internal static void clear(this ref box b) {
    b.data = default!;
}

[GoType] partial struct guard {
    internal error err;
}

internal static void recover(this ж<guard> Ꮡg) => func((defer, recover) => {
    ref var g = ref Ꮡg.Value;

    {
        var e = recover(); if (e != default!) {
            g.err = fmt.Errorf("recovered: %v"u8, e);
        }
    }
});

internal static void run(this ж<guard> Ꮡg) => func((defer, recover) => {
    ref var g = ref Ꮡg.Value;

    defer(Ꮡg.recover);
    throw panic("boom");
});

internal static void Main() {
    var s = new nint[]{1, 2, 3}.slice();
    builtin.clear(s);
    fmt.Println(s[0], s[1], s[2]);
    var b = Ꮡ(new box(data: new nint[]{4, 5}.slice()));
    b.clear();
    fmt.Println(len((~b).data));
    var m = new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2};
    builtin.clear(m);
    fmt.Println(len(m));
    var g = Ꮡ(new guard(nil));
    g.run();
    fmt.Println((~g).err);
    var h = new header(new map<@string, slice<@string>>{["a"u8] = new @string[]{"1"u8}.slice(), ["b"u8] = new @string[]{"2"u8}.slice()});
    var alias = h;
    builtin.clear(h);
    fmt.Println(len(h), len(alias));
    header hn = default!;
    builtin.clear(hn);
    fmt.Println(len(hn));
}

} // end main_package
