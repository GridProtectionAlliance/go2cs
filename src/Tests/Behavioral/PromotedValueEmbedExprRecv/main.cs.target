namespace go;

using fmt = fmt_package;
using PromotedValueEmbedLib = PromotedValueEmbedLib_package;

partial class main_package {

internal static map<@string, any> registry() {
    return new map<@string, any>{["w"u8] = PromotedValueEmbedLib.New("boxed"u8)};
}

internal static void Main() {
    var m = registry();
    fmt.Println(m["w"u8]._<ж<PromotedValueEmbedLib.Widget>>().Name());
    fmt.Println(PromotedValueEmbedLib.New("direct"u8).Name());
}

} // end main_package
