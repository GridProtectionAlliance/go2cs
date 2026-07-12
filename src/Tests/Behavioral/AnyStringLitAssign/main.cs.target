namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct holder {
    internal any value;
}

internal static @string sprint(any x) {
    switch (x.type()) {
    case @string v: {
        return "str:"u8 + v;
    }
    case null: {
        return "none"u8;
    }
    default: {
        var v = x;
        return fmt.Sprintf("other:%v"u8, v);
    }}
}

internal static void Main() {
    any arg = default!;
    arg = (@string)"<nil>";
    fmt.Println(sprint(arg));
    var args = new any[]{(nint)(1), default!, (@string)"keep"}.slice();
    foreach (var (i, vᴛ1) in args) {
        var a = vᴛ1;

        if (a == default!) {
            a = (@string)"<missing>";
        }
        fmt.Println(i, sprint(a));
    }
    holder h = default!;
    h.value = (@string)"field";
    fmt.Println(sprint(h.value));
}

} // end main_package
