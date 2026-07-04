namespace go;

using fmt = fmt_package;
using strings = strings_package;

partial class main_package {

internal static nint f(nint x) {
    if (x >= 2) {
        nint zΔ1 = default!;
        if (x > 5){
            zΔ1 = 100;
        } else {
            zΔ1 = 200;
        }
        return zΔ1;
    }
    nint z = x * x;
    return z;
}

[GoType] partial struct tagErr {
    internal @string tag;
}

[GoRecv] internal static @string Error(this ref tagErr e) {
    return "tag:"u8 + e.tag;
}

internal static (nint, error) check(@string s) {
    if (s == ""u8) {
        return (0, new tagErrжerror(Ꮡ(new tagErr(tag: "empty"u8))));
    }
    return (len(s), default!);
}

internal static @string g(@string s) {
    {
        nint n = len(s); if (n >= 0) {
            var (vΔ1, errΔ1) = check(""u8);
            if (errΔ1 != default!) {
                {
                    var (e, ok) = errΔ1._<ж<tagErr>>(ᐧ); if (ok) {
                        e.Value.tag = "inner"u8;
                    }
                }
                return errΔ1.Error();
            }
            _ = vΔ1;
        }
    }
    var (v, err) = check(s);
    if (err != default!) {
        return "outer"u8;
    }
    return fmt.Sprint(v);
}

internal static @string pkgShadow(@string s) {
    @string doubled = strings.Join(new @string[]{s, s}.slice(), "-"u8);
    nint stringsΔ1 = len(doubled);
    return fmt.Sprint(stringsΔ1);
}

internal static void Main() {
    fmt.Println(f(10));
    fmt.Println(f(3));
    fmt.Println(f(1));
    fmt.Println(g("ab"u8));
    fmt.Println(pkgShadow("xy"u8));
}

} // end main_package
