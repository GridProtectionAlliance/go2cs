namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var pick = @string (any v) => {
        switch (v.type()) {
        case @string t: {
            return "string:"u8 + t;
        }
        case nint t: {
            return fmt.Sprintf("int:%d"u8, t);
        }
        case int32 t: {
            return fmt.Sprintf("int:%d"u8, t);
        }
        default: {
            var t = v;
            return fmt.Sprintf("other:%v"u8, t);
        }}
    };
    fmt.Println(pick("abc"));
    fmt.Println(pick(42));
    fmt.Println(pick(true));
    var wrap = @string (@string s, bool quote) => {
        if (quote) {
            return "q:"u8 + s;
        }
        return fmt.Sprintf("p:%s"u8, s);
    };
    fmt.Println(wrap("x"u8, true));
    fmt.Println(wrap("y"u8, false));
    var tag = @string (@string s) => {
        if (len(s) == 0) {
            return "tag:"u8 + "empty"u8;
        }
        return "tag:"u8 + s;
    };
    fmt.Println(tag("z"u8));
    fmt.Println(tag(""u8));
    var bang = @string (@string s) => {
        if (len(s) > 1) {
            return s + "!"u8;
        }
        return fmt.Sprintf("%s?"u8, s);
    };
    fmt.Println(bang("hi"u8));
    fmt.Println(bang("h"u8));
    Func<@string, bool, @string> pad = (@string s, bool wide) => {
        if (wide) {
            return "  "u8 + s;
        }
        return fmt.Sprintf("[%s]"u8, s);
    };
    fmt.Println(pad("v"u8, true));
    fmt.Println(pad("v"u8, false));
    var fuzzish = (nint dur, slice<byte> cov, @string errMsg) (nint entry) => {
        nint dur = default!;
        slice<byte> cov = default!;
        @string errMsg = default!;
        if (entry < 0) {
            @string msg = fmt.Sprintf("bad entry %d"u8, entry);
            return (entry, default!, msg);
        }
        if (entry == 0) {
            return (entry, default!, "");
        }
        return (entry, new byte[]{(byte)entry}.slice(), "");
    };
    var (d1, c1, e1) = fuzzish(-1);
    fmt.Println(d1, c1, e1 != ""u8, e1);
    var (d2, c2, e2) = fuzzish(0);
    fmt.Println(d2, c2, e2 != ""u8, e2);
    var (d3, c3, e3) = fuzzish(2);
    fmt.Println(d3, c3, e3 != ""u8, e3);
    var sget = (@string, error) (bool ok) => {
        if (!ok) {
            return ("", fmt.Errorf("no string"u8));
        }
        return ("found", default!);
    };
    var (s1, err1) = sget(false);
    fmt.Println(s1 == ""u8, err1 != default!);
    var (s2, err2) = sget(true);
    fmt.Println(s2, err2 == default!);
}

} // end main_package
