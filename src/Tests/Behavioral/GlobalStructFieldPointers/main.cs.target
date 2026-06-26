namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var options = new option[]{
        new("fast"u8, ᏑFeatures.of(Featuresᴛ1.ᏑHasFast)),
        new("wide"u8, ᏑFeatures.of(Featuresᴛ1.ᏑHasWide))
    }.slice();
    foreach (var (_, o) in options) {
        o.flag.val = true;
    }
    foreach (var (_, o) in options) {
        fmt.Printf("%s=%t\n"u8, o.name, o.flag.val);
    }
    var level = ᏑFeatures.of(Featuresᴛ1.ᏑLevel);
    level.val = 3;
    fmt.Printf("level=%d\n"u8, level.val);
    fmt.Printf("global: HasFast=%t HasWide=%t Level=%d\n"u8,
        Features.HasFast, Features.HasWide, Features.Level);
}

} // end main_package
