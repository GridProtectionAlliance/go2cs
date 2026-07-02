namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    @string s = "AB\U0001F600\U0001F601C"u8;
    slice<uint16> @out = default!;
    foreach (var (_, rᴛ1) in s) {
        var r = rᴛ1;

        if (r < 0x10000){
            @out = append(@out, (uint16)r);
        } else {
            r -= 0x10000;
            @out = append(@out, (uint16)(0xD800 + ((r >> (int)(10)))));
            @out = append(@out, (uint16)(0xDC00 + ((rune)(r & 0x3FF))));
        }
    }
    foreach (var (_, u) in @out) {
        fmt.Printf("%d "u8, u);
    }
    fmt.Println();
    fmt.Println("len", len(@out));
}

} // end main_package
