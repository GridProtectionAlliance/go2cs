namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    channel<nint> nilCh = default!;
    fmt.Println("nil len:", len(nilCh), "cap:", cap(nilCh));
    var live = new channel<nint>(1);
    live.ᐸꟷ(42);
    var selᴛ1 = nilCh;
    var selᴛ2 = live;
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ), nilCh.ᐸꟷ(1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out var v): {
        fmt.Println("nil recv (wrong):", v);
        break;
    }
    case 1: {
        fmt.Println("nil send (wrong)");
        break;
    }
    case 2 when selᴛ2.ꟷᐳ(out var v): {
        fmt.Println("live recv:", v);
        break;
    }}
    var u = new channel<nint>(0);
    var uʗ1 = u;
    goǃ(() => {
        uʗ1.ᐸꟷ(7);
    });
    var selᴛ3 = nilCh;
    var selᴛ4 = u;
    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
    case 0 when selᴛ3.ꟷᐳ(out var v): {
        fmt.Println("nil recv (wrong):", v);
        break;
    }
    case 1 when selᴛ4.ꟷᐳ(out var v): {
        fmt.Println("rendezvous recv:", v);
        break;
    }}
    var @out = new channel<nint>(1);
    switch (select(nilCh.ᐸꟷ(9, ꓸꓸꓸ), @out.ᐸꟷ(5, ꓸꓸꓸ))) {
    case 0: {
        fmt.Println("nil send (wrong)");
        break;
    }
    case 1: {
        fmt.Println("live send fired");
        break;
    }}
    fmt.Println("out:", ᐸꟷ(@out));
}

} // end main_package
