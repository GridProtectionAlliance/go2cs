namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface Sizer {
    nint Size();
}

[GoType] partial interface Namer {
    @string Name();
}

[GoType("dyn")] partial interface describe_thing :
    Sizer,
    Namer
{
}

internal static @string describe(describe_thing thing) {
    return fmt.Sprintf("%s(%d)"u8, thing.Name(), thing.Size());
}

} // end main_package
