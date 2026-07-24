namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var a = new channel<nint>(1);
    var b = new channel<nint>(1);
    nint aCount = 0;
    nint bCount = 0;
    for (nint i = 0; i < 200; i++) {
        a.ᐸꟷ(1);
        b.ᐸꟷ(1);
        switch (select(ᐸꟷ(a, ꓸꓸꓸ), ᐸꟷ(b, ꓸꓸꓸ))) {
        case 0 when a.ꟷᐳ(out _): {
            aCount++;
            break;
        }
        case 1 when b.ꟷᐳ(out _): {
            bCount++;
            break;
        }}
        switch (select(ᐸꟷ(a, ꓸꓸꓸ), ᐸꟷ(b, ꓸꓸꓸ))) {
        case 0 when a.ꟷᐳ(out _): {
            break;
        }
        case 1 when b.ꟷᐳ(out _): {
            break;
        }}
    }
    fmt.Println("total:", aCount + bCount);
    fmt.Println("both branches taken:", aCount > 0 && bCount > 0);
}

} // end main_package
