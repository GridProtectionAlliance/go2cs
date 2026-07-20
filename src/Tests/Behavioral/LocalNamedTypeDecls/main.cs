namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("dyn")] partial struct main_Point {
    public nint X, Y;
}

[GoType("[]main_Point")] partial struct main_Points;

[GoType("map[@string, nint]")] partial struct main_Tally;

[GoType("chan nint")] partial struct main_Stream;

[GoType("[3]nint")] partial struct main_Triple;

internal static void Main() {
    var pts = new main_Points(new main_Point[]{new(1, 2), new(3, 4), new(5, 6)}.slice());
    foreach (var (_, p) in pts) {
        fmt.Println(p.X, p.Y);
    }
    var tally = new main_Tally(new map<@string, nint>{["a"u8] = 1, ["b"u8] = 2});
    fmt.Println(tally["a"u8] + tally["b"u8]);
    var stream = new main_Stream(1);
    stream.ᐸꟷ(7);
    fmt.Println(ᐸꟷ<nint>(stream));
    var triple = new main_Triple(new nint[]{10, 20, 30}.array());
    nint sum = 0;
    foreach (var (_, n) in triple) {
        sum += n;
    }
    fmt.Println(sum);
}

} // end main_package
