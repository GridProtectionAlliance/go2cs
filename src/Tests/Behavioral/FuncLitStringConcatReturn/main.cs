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
}

} // end main_package
