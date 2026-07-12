namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct node {
    internal any inner;
}

internal static @string inspect(any n) {
    switch (n.type()) {
    case ж<node> nΔ1: {
        switch ((~nΔ1).inner.type()) {
        case nint: {
            return "node:int"u8;
        }
        case int32: {
            return "node:int"u8;
        }
        case @string: {
            return "node:string"u8;
        }
        default: {
            return "node:other"u8;
        }}

        break;
    }
    default: {
        var nΔ1 = n;
        _ = nΔ1;
        return "plain"u8;
    }}
}

internal static (@string, @string) classifyVia(any v) {
    var unwrap = (any vΔ1) => {
        {
            var (p, ok) = vΔ1._<ж<node>>(ᐧ); if (ok) {
                return (~p).inner;
            }
        }
        return vΔ1;
    };
    var unwrapʗ1 = unwrap;
    var pick = @string (any vΔ2) => {
        var switchᴛ1 = unwrapʗ1(vΔ2);
        switch (switchᴛ1.type()) {
        case nint vΔ3: {
            return fmt.Sprintf("int:%d"u8, vΔ3);
        }
        case int32 vΔ3: {
            return fmt.Sprintf("int:%d"u8, vΔ3);
        }
        case @string vΔ3: {
            return fmt.Sprintf("string:%s"u8, vΔ3);
        }
        default: {
            var vΔ3 = switchᴛ1;
            return fmt.Sprintf("other:%v"u8, vΔ3);
        }}
    };
    @string label = "shadow"u8;
    var other = Ꮡ(new node(nil));
    other.Value.inner = label;
    return (pick(v), pick(other));
}

internal static void Main() {
    @string hi = "hi"u8;
    var sn = Ꮡ(new node(nil));
    sn.Value.inner = hi;
    fmt.Println(inspect(Ꮡ(new node(inner: (nint)(5)))));
    fmt.Println(inspect(sn));
    fmt.Println(inspect(Ꮡ(new node(inner: 1.5D))));
    fmt.Println(inspect((nint)(42)));
    var (a, b) = classifyVia(Ꮡ(new node(inner: (nint)(7))));
    fmt.Println(a, b);
    var (c, d) = classifyVia("direct");
    fmt.Println(c, d);
}

} // end main_package
