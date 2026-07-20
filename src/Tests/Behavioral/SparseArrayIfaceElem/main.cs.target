namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface shape {
    @string name();
}

[GoType] partial struct circle {
}

internal static @string name(this circle _) {
    return "circle"u8;
}

[GoType] partial struct square {
}

internal static @string name(this square _) {
    return "square"u8;
}

internal static readonly UntypedInt kCircle = /* iota + 2 */ 2;
internal static readonly UntypedInt kSquare = 3;
internal static readonly UntypedInt kLast = 4;

internal static shape lookup(nint i) {
    return new golib.SparseArray<shape>{[kCircle] = new circle(nil), [kSquare] = new square(nil)
    }.array(4)[i];
}

internal static array<shape> registry = new golib.SparseArray<shape>{[kCircle] = new circle(nil), [kSquare] = new square(nil)
}.array(4);

[GoType("num:nuint")] partial struct hashKind;

internal static readonly hashKind hCircle = 5;
internal static readonly hashKind hSquare = 6;

internal static array<shape> byKind = new golib.SparseArray<shape>{[(int)((nuint)hCircle)] = new circle(nil), [(int)((nuint)hSquare)] = new square(nil)
}.array(7);

internal static void Main() {
    fmt.Println(lookup(kCircle).name());
    fmt.Println(lookup(kSquare).name());
    fmt.Println(lookup(0) == default!);
    fmt.Println("registry:", registry[kCircle].name(), registry[kSquare].name(), len(registry));
    fmt.Println("byKind:", byKind[hCircle].name(), byKind[hSquare].name(), len(byKind));
}

} // end main_package
