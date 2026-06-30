namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    slice<byte> b = default!;
    for (var ch = (rune)(rune)'A'; ch <= (rune)'E'; ch++) {
        b = append(b, ((@string)ch).ꓸꓸꓸ);
    }
    fmt.Println(((@string)b));
    var bs = new byte[]{(rune)'H', (rune)'i'}.slice();
    fmt.Println(((@string)bs)[0]);
    fmt.Println(len(((@string)bs)));
    slice<byte> m = default!;
    m = append(m, ((@string)(rune)(rune)'世').ꓸꓸꓸ);
    fmt.Println(len(m));
}

} // end main_package
