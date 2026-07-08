namespace go;

using fmt = fmt_package;
using PromotedValueEmbedLib = PromotedValueEmbedLib_package;

partial class main_package {

internal static @string describe(ж<PromotedValueEmbedLib.Widget> Ꮡw) {
    ref var w = ref Ꮡw.Value;

    if (Ꮡw.Name() == "widget"u8) {
        return "match"u8;
    }
    return "nomatch"u8;
}

internal static void Main() {
    var w = PromotedValueEmbedLib.New("widget"u8);
    fmt.Println(w.Name(), w.Name() == "widget"u8, describe(w));
    var g = PromotedValueEmbedLib.NewGadget("gadget"u8);
    fmt.Println(g.Name());
}

} // end main_package
