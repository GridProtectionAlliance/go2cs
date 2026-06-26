namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var options = new option[]{
        new("fast"u8, Ꮡ(Features).of(Featuresᴛ1.ᏑHasFast)),
        new("wide"u8, Ꮡ(Features).of(Featuresᴛ1.ᏑHasWide))
    }.slice();
    foreach (var (_, o) in options) {
        o.flag.val = true;
    }
    foreach (var (_, o) in options) {
        fmt.Printf("%s=%t\n"u8, o.name, o.flag.val);
    }
    var level = Ꮡ(Features).of(Featuresᴛ1.ᏑLevel);
    level.val = 3;
    fmt.Printf("level=%d\n"u8, level.val);
}

} // end main_package
