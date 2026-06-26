namespace go;

using fmt = fmt_package;

partial class main_package {

public static T First<T>(params Span<T> valsʗp)
    where T : new()
{
    var vals = valsʗp.slice();

    return vals[0];
}

public static nint Count<T>(params Span<T> valsʗp)
    where T : new()
{
    var vals = valsʗp.slice();

    return len(vals);
}

public static T Or<T>(params Span<T> valsʗp)
    where T : /* comparable */ comparable<T>, new()
{
    var vals = valsʗp.slice();

    T zero = default!;
    foreach (var (_, v) in vals) {
        if (!AreEqual(v, zero)) {
            return v;
        }
    }
    return zero;
}

internal static void Main() {
    fmt.Println(First(10, 20, 30));
    fmt.Println(First(1.5F, 2.5F));
    fmt.Println(Count(1, 2, 3, 4));
    @string s1 = "go"u8;
    @string s2 = "2cs"u8;
    fmt.Println(First(s1, s2));
}

} // end main_package
