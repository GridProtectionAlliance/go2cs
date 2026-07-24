namespace go;

using fmt = fmt_package;

partial class main_package {

internal static array<nint> a = new(4);

internal static channel<nint> innerCh = new channel<nint>(1);

internal static nint idxDefault(nint i) {
    switch (trySelect(ᐸꟷ(innerCh, ꓸꓸꓸ))) {
    case 0 when innerCh.ꟷᐳ(out var v): {
        fmt.Println("  inner recv:", v);
        break;
    }
    default: {
        fmt.Println("  inner default");
        break;
    }}
    return i;
}

internal static nint idxBlocking(nint i) {
    var ready = new channel<nint>(0);
    var readyʗ1 = ready;
    goǃ(() => {
        readyʗ1.ᐸꟷ(100 + i);
    });
    switch (select(ᐸꟷ(ready, ꓸꓸꓸ))) {
    case 0 when ready.ꟷᐳ(out var v): {
        fmt.Println("  inner blocking recv:", v);
        break;
    }}
    return i;
}

internal static void Main() {
    var ch = new channel<nint>(2);
    ch.ᐸꟷ(42);
    switch (select(ᐸꟷ(ch, ꓸꓸꓸ))) {
    case 0 when ch.ꟷᐳ(out a[idxDefault(0)]): {
        fmt.Println("outer fired: a[0] =", a[0], "len(ch) =", len(ch));
        break;
    }}
    ch.ᐸꟷ(43);
    innerCh.ᐸꟷ(7);
    switch (select(ᐸꟷ(ch, ꓸꓸꓸ))) {
    case 0 when ch.ꟷᐳ(out a[idxDefault(1)]): {
        fmt.Println("outer fired: a[1] =", a[1], "len(ch) =", len(ch));
        break;
    }}
    ch.ᐸꟷ(44);
    ch.ᐸꟷ(45);
    switch (select(ᐸꟷ(ch, ꓸꓸꓸ))) {
    case 0 when ch.ꟷᐳ(out a[idxDefault(2)]): {
        fmt.Println("outer fired: a[2] =", a[2], "len(ch) =", len(ch), "next =", ᐸꟷ(ch));
        break;
    }}
    ch.ᐸꟷ(46);
    switch (select(ᐸꟷ(ch, ꓸꓸꓸ))) {
    case 0 when ch.ꟷᐳ(out a[idxBlocking(3)]): {
        fmt.Println("outer fired: a[3] =", a[3], "len(ch) =", len(ch));
        break;
    }}
    ch.ᐸꟷ(47);
    switch (trySelect(ᐸꟷ(ch, ꓸꓸꓸ))) {
    case 0 when ch.ꟷᐳ(out a[idxDefault(0)]): {
        fmt.Println("outer default-form fired: a[0] =", a[0], "len(ch) =", len(ch));
        break;
    }
    default: {
        fmt.Println("outer default (wrong)");
        break;
    }}
}

} // end main_package
