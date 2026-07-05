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
    var pts = new point[]{new(1, 2), new(3, 4)}.slice();
    nint total = 0;
    foreach (var (_, vᴛ1) in pts) {
        var p = vᴛ1;

        total += p.bump();
    }
    fmt.Println("total", total);
    fmt.Println("orig", pts[0].x, pts[1].x);
    var tags = new point[]{new(10, 1), new(20, 2)}.slice();
    nint sum = 0;
    foreach (var (_, vᴛ2) in tags) {
        var t = vᴛ2;

        t.x = t.x + 1;
        t.y = t.y + 1;
        sum += t.x + t.y;
    }
    fmt.Println("fieldwrite", sum, tags[0].x, tags[1].y);
}

[GoType] partial struct point {
    internal nint x, y;
}

[GoRecv] internal static nint bump(this ref point p) {
    p.x++;
    return p.x + p.y;
}

} // end main_package
