namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct ColorList {
    public nint Total;
    public @string Color;
    public ptr<ColorList> Next;
    public ptr<ptr<ColorList>> NextNext;
}

private static void Main() {
    var red = new ColorList(2, "red", default!, default!);
    ref var blue = ref heap<ColorList>(out var Ꮡblue);
    blue = new ColorList(2, "blue", default!, default!);
    red.Next = Ꮡblue;
    fmt.Printf("Value of red = %v\n"u8, red);
    fmt.Printf("Value of blue = %v\n"u8, blue);
}

} // end main_package
