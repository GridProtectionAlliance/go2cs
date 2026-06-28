namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint32 byteSum<T>(T s)
    where T : /* string | []byte */ IByteSeq<byte>, new()
{
    uint32 h = default!;
    for (nint i = 0; i < len(s); i++) {
        h = h * 31 + ((uint32)s[i]);
    }
    return h;
}

internal static bool prefixMatch<T>(T s, T sep)
    where T : /* string | []byte */ IByteSeq<byte>, new()
{
    nint n = len(sep);
    if (len(s) < n) {
        return false;
    }
    return new @string(s[..(int)(n)]) == new @string(sep);
}

internal static void Main() {
    @string str = "hello"u8;
    var bs = slice<byte>(str);
    @string hel = "hel"u8;
    var helBytes = slice<byte>(hel);
    fmt.Println("sum(string):", byteSum(str));
    fmt.Println("sum([]byte):", byteSum(bs));
    fmt.Println("prefix string:", prefixMatch(str, hel));
    fmt.Println("prefix []byte:", prefixMatch(bs, helBytes));
    fmt.Println("prefix miss:", prefixMatch(str, (@string)"xyz"));
    fmt.Println("prefix too long:", prefixMatch(str, (@string)"hello world"));
}

} // end main_package
