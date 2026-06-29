namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct box {
    internal nint n;
}

internal static void Main() {
    safeAssertions();
    assertionsWithPanic();
    pointerAssertion();
    fmt.Println("Program completed after panic recovery");
}

internal static void pointerAssertion() {
    any i = Ꮡ(new box(n: 7));
    {
        var (bΔ1, ok) = i._<ж<box>>(ᐧ); if (ok) {
            fmt.Println("Value is a *box:", (~bΔ1).n);
        }
    }
    var b = i._<ж<box>>();
    fmt.Println("Pointer value:", (~b).n);
}

internal static void safeAssertions() {
    any i = "hello";
    {
        var (s, ok) = i._<@string>(ᐧ); if (ok){
            fmt.Println("Value is a string:", s);
        } else {
            fmt.Println("Value is not a string");
        }
    }
    {
        var (n, ok) = i._<nint>(ᐧ); if (ok){
            fmt.Println("Value is an int:", n);
        } else {
            fmt.Println("Value is not an int");
        }
    }
}

internal static void assertionsWithPanic() => func((defer, recover) => {
    defer(() => {
        {
            var r = recover(); if (r != default!) {
                fmt.Println("Recovered from panic:", r);
            }
        }
    });
    any i = "hello";
    @string s = i._<@string>();
    fmt.Println("String value:", s);
    nint n = i._<nint>();
    fmt.Println("Integer value:", n);
});

} // end main_package
