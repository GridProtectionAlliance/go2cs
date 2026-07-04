namespace go;

using fmt = fmt_package;

partial class main_package {

internal static uint32 byteSum<T>(T s)
    where T : /* string | []byte */ IByteSeq<byte>, new()
{
    uint32 h = default!;
    for (nint i = 0; i < len(s); i++) {
        h = h * 31 + (uint32)s[i];
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
    return new @string(((T)(s[..(int)(n)]))) == new @string(sep);
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
    fmt.Println("last(string):", lastByte(str));
    fmt.Println("last([]byte):", lastByte(bs));
    fmt.Println("sum:", digitSum((@string)"12:34"), digitSum(slice<byte>((@string)"56:78")));
    fmt.Println("head:", headSum((@string)"x98:76"), headSum(slice<byte>((@string)"y10:23")));
}

internal static nint digitSum<T>(T s)
    where T : /* []byte | string */ IByteSeq<byte>, new()
{
    var parse = (T part) => {
        nint n = 0;
        foreach (var (_, c) in new slice<byte>(part)) {
            n = n * 10 + (nint)(c - (rune)'0');
        }
        return n;
    };
    return parse(((T)(s[0..2]))) + parse(((T)(s[3..5])));
}

internal static T trimHead<T>(T s, nint n)
    where T : /* []byte | string */ IByteSeq<byte>, new()
{
    for (nint i = 0; i < n; i++) {
        if (len(s) > 1) {
            s = ((T)(s[1..]));
        }
    }
    return ((T)(s[0..]));
}

internal static nint headSum<T>(T s)
    where T : /* []byte | string */ IByteSeq<byte>, new()
{
    return digitSum(trimHead(s, 1));
}

internal static byte lastByte<T>(T s)
    where T : /* []byte | string */ IByteSeq<byte>, new()
{
    return s[len(s) - 1];
}

} // end main_package
