namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var name = slice<byte>((@string)"go2cs");
    sstring s = ((sstring)name);
    if (s == "go2cs"u8) {
        fmt.Println("match");
    }
    var digits = slice<byte>((@string)"2468");
    sstring d = ((sstring)digits);
    fmt.Println((nint)d[0] + (nint)d[3] + len(d));
    var scratch = slice<byte>((@string)"AB");
    @string t = ((@string)scratch);
    scratch[0] = (rune)'X';
    if (t == "AB"u8) {
        fmt.Println("copy-safe");
    }
    @string u = ((@string)slice<byte>((@string)"printed"));
    fmt.Println(u);
    fmt.Println(returnedString());
    var tag = slice<byte>((@string)"v2");
    sstring tagᴛ1 = ((sstring)tag);
    if (tagᴛ1 == "v2"u8) {
        fmt.Println("tagged");
    }
    @string want = "v2"u8;
    if (tagᴛ1 == want) {
        fmt.Println("wanted");
    }
    fmt.Println("classify:", classify(slice<byte>((@string)"rust")));
    fmt.Println("pick:", pick(slice<byte>((@string)"b")));
    fmt.Println("prefix:", prefix(slice<byte>((@string)"GET /x")));
    fmt.Println("matchVar:", matchVar(slice<byte>((@string)"go2cs"), "go2cs"u8));
    fmt.Println("matchField:", matchField(slice<byte>((@string)"prod"), new config(name: "prod"u8)));
    fmt.Println("twoConv:", matchTwoConversions(slice<byte>((@string)"abc"), slice<byte>((@string)"abc")));
    fmt.Println("callOperand:", staysHeapCallOperand(slice<byte>((@string)"y"), () => "y"u8));
}

[GoType] partial struct config {
    internal @string name;
}

internal static bool matchVar(slice<byte> b, @string want) {
    sstring s = ((sstring)b);
    return s == want;
}

internal static bool matchField(slice<byte> b, config cfg) {
    sstring s = ((sstring)b);
    return s == cfg.name;
}

internal static bool matchTwoConversions(slice<byte> a, slice<byte> b) {
    return ((sstring)a) == ((sstring)b);
}

internal static bool staysHeapCallOperand(slice<byte> a, Func<@string> next) {
    return ((@string)a) == next();
}

internal static @string returnedString() {
    var b = slice<byte>((@string)"returned");
    @string r = ((@string)b);
    return r;
}

internal static nint classify(slice<byte> word) {
    nint total = 0;
    sstring wordᴛ1 = ((sstring)word);
    for (nint i = 0; i < 2; i++) {
        if (wordᴛ1 == "go"u8) {
            total++;
        }
        if (wordᴛ1 == "rust"u8) {
            total += 10;
        }
        if (wordᴛ1 == "c"u8) {
            total += 100;
        }
    }
    return total;
}

internal static @string pick(slice<byte> tag) {
    sstring tagᴛ1 = ((sstring)tag);
    if (tagᴛ1 == "a"u8) {
        return "alpha"u8;
    }
    if (tagᴛ1 == "b"u8) {
        return "bravo"u8;
    }
    return "other"u8;
}

internal static bool prefix(slice<byte> buf) {
    if (len(buf) < 6) {
        return false;
    }
    if (((sstring)(buf[..3])) != "GET"u8) {
        return false;
    }
    return ((sstring)(buf[3..6])) == " /x"u8;
}

} // end main_package
