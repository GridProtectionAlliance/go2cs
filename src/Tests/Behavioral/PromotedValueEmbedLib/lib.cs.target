namespace go;

partial class PromotedValueEmbedLib_package {

[GoType] partial struct common {
    internal @string label;
}

[GoRecv] internal static @string Name(this ref common c) {
    return c.label;
}

[GoType] partial struct Widget {
    internal partial ref common common { get; }
}

public static ж<Widget> New(@string label) {
    return Ꮡ(new Widget(common: new common(label: label)));
}

[GoType] partial struct Gadget {
    internal @string tag;
}

[GoRecv] public static @string Name(this ref Gadget g) {
    return g.tag;
}

public static ж<Gadget> NewGadget(@string tag) {
    return Ꮡ(new Gadget(tag: tag));
}

} // end PromotedValueEmbedLib_package
