namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var data = new channel<nint>(1);
    var @out = new channel<nint>(1);
    data.ᐸꟷ(5);
    nint recvGot = 0;
    nint sentTo = 0;
    nint rounds = 0;
    while (len(data) > 0 || len(@out) == 0) {
        rounds++;
        var selᴛ1 = data;
        switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), @out.ᐸꟷ(7, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out var v): {
            recvGot = v;
            break;
        }
        case 1: {
            sentTo++;
            break;
        }}
    }
    fmt.Println("rounds:", rounds, "recvGot:", recvGot, "sentTo:", sentTo, "out:", ᐸꟷ(@out));
    var ch = new channel<nint>(1);
    ch.ᐸꟷ(3);
    nint took = 0;
    var selᴛ2 = ch;
    switch (select(ch.ᐸꟷ(8, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0: {
        fmt.Println("send fired on full channel (wrong)");
        break;
    }
    case 1 when selᴛ2.ꟷᐳ(out took): {
        break;
    }}
    fmt.Println("took:", took, "len:", len(ch));
    var selᴛ3 = ch;
    switch (select(ch.ᐸꟷ(8, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
    case 0: {
        break;
    }
    case 1 when selᴛ3.ꟷᐳ(out took): {
        fmt.Println("recv fired on empty channel (wrong)");
        break;
    }}
    fmt.Println("len:", len(ch), "drained:", ᐸꟷ(ch));
}

} // end main_package
