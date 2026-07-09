namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct registry {
    internal map<@string, ж<nint>> handles;
}

[GoRecv] internal static nint at(this ref registry r, @string key) {
    return r.handles[key].Value;
}

[GoRecv] internal static void set(this ref registry r, @string key, nint val) {
    r.handles[key].Value = val;
}

[GoRecv] internal static registry clone(this ref registry r) {
    return r;
}

internal static void Main() {
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    var r = Ꮡ(new registry(handles: new map<@string, ж<nint>>{["a"u8] = Ꮡa, ["b"u8] = Ꮡb}));
    fmt.Println(r.at("a"u8), r.at("b"u8));
    r.set("a"u8, 10);
    fmt.Println(r.at("a"u8), a);
    var c = r.clone();
    fmt.Println(len(c.handles));
}

} // end main_package
