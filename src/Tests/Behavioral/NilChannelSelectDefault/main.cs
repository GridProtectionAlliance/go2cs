namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    channel<nint> nilRecv = default!;
    switch (ᐧ) {
    case ᐧ when nilRecv.ꟷᐳ(out var v): {
        fmt.Println("nil recv took case", v);
        break;
    }
    default: {
        fmt.Println("nil recv default");
        break;
    }}
    fmt.Println("nil len:", len(nilRecv), "nil cap:", cap(nilRecv));
    switch (ᐧ) {
    case ᐧ when nilRecv.ꟷᐳ(out var v, out var ok): {
        fmt.Println("nil comma-ok took case", v, ok);
        break;
    }
    default: {
        fmt.Println("nil comma-ok default");
        break;
    }}
    var ready = new channel<nint>(1);
    switch (ᐧ) {
    case ᐧ when ready.ꟷᐳ(out var v): {
        fmt.Println("real recv took case", v);
        break;
    }
    default: {
        fmt.Println("real empty default");
        break;
    }}
    ready.ᐸꟷ(7);
    switch (ᐧ) {
    case ᐧ when ready.ꟷᐳ(out var v): {
        fmt.Println("real recv", v);
        break;
    }
    default: {
        fmt.Println("real default");
        break;
    }}
    ready.ᐸꟷ(9);
    switch (select(ᐸꟷ(nilRecv, ꓸꓸꓸ), ᐸꟷ(ready, ꓸꓸꓸ))) {
    case 0 when nilRecv.ꟷᐳ(out var v): {
        fmt.Println("mixed took nil", v);
        break;
    }
    case 1 when ready.ꟷᐳ(out var v): {
        fmt.Println("mixed took real", v);
        break;
    }}
}

} // end main_package
