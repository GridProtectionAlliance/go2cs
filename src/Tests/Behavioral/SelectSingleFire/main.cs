namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var a = new channel<nint>(1);
    var b = new channel<nint>(1);
    switch (select(a.ᐸꟷ(9, ꓸꓸꓸ), b.ᐸꟷ(9, ꓸꓸꓸ))) {
    case 0: {
        break;
    }
    case 1: {
        break;
    }}
    fmt.Println("delivered:", len(a) + len(b));
    nint got = default!;
    var selᴛ1 = a;
    var selᴛ2 = b;
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out got): {
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out got): {
        break;
    }}
    fmt.Println("got:", got, "remaining:", len(a) + len(b));
    var c = new channel<nint>(100);
    var d = new channel<nint>(100);
    for (nint i = 0; i < 100; i++) {
        switch (select(c.ᐸꟷ(i, ꓸꓸꓸ), d.ᐸꟷ(i, ꓸꓸꓸ))) {
        case 0: {
            break;
        }
        case 1: {
            break;
        }}
    }
    fmt.Println("after 100 selects:", len(c) + len(d));
    nint sum = 0;
    while (len(c) > 0) {
        sum += ᐸꟷ(c);
    }
    while (len(d) > 0) {
        sum += ᐸꟷ(d);
    }
    fmt.Println("sum:", sum);
}

} // end main_package
