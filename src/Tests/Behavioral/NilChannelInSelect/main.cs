namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    channel<nint> nilCh = default!;
    fmt.Println("nil len:", len(nilCh), "cap:", cap(nilCh));
    var live = new channel<nint>(1);
    live.ᐸꟷ(42);
    switch (select(ᐸꟷ(nilCh, ꓸꓸꓸ), nilCh.ᐸꟷ(1, ꓸꓸꓸ), ᐸꟷ(live, ꓸꓸꓸ))) {
    case 0 when nilCh.ꟷᐳ(out var v): {
        fmt.Println("nil recv (wrong):", v);
        break;
    }
    case 1: {
        fmt.Println("nil send (wrong)");
        break;
    }
    case 2 when live.ꟷᐳ(out var v): {
        fmt.Println("live recv:", v);
        break;
    }}
    var u = new channel<nint>(0);
    var uʗ1 = u;
    goǃ(() => {
        uʗ1.ᐸꟷ(7);
    });
    switch (select(ᐸꟷ(nilCh, ꓸꓸꓸ), ᐸꟷ(u, ꓸꓸꓸ))) {
    case 0 when nilCh.ꟷᐳ(out var v): {
        fmt.Println("nil recv (wrong):", v);
        break;
    }
    case 1 when u.ꟷᐳ(out var v): {
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
