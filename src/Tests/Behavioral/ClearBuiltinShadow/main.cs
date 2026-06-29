namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct box {
    internal slice<nint> data;
}

[GoRecv] internal static void clear(this ref box b) {
    b.data = default!;
}

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
}

} // end main_package
