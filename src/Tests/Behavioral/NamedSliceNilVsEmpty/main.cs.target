namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[]nint")] partial struct IntSlice;

[GoType("map[@string, nint]")] partial struct StrIntMap;

[GoType("chan nint")] partial struct IntChan;

internal static void probe(@string name, bool isNil) {
    fmt.Println(name, isNil);
}

internal static void Main() {
    IntSlice zeroSlice = default!;
    probe("sliceZero"u8, zeroSlice == default!);
    var emptySlice = new IntSlice(new nint[]{}.slice());
    probe("sliceEmptyLiteral"u8, emptySlice == default!);
    var madeSlice = new IntSlice(0);
    probe("sliceMakeZero"u8, madeSlice == default!);
    var filledSlice = new IntSlice(new nint[]{1, 2}.slice());
    probe("sliceFilled"u8, filledSlice == default!);
    var tail = filledSlice[2..2];
    probe("sliceResliceTailCapZero"u8, tail == default!);
    var nilReslice = zeroSlice[0..0];
    probe("sliceNilReslice"u8, nilReslice == default!);
    StrIntMap zeroMap = default!;
    probe("mapZero"u8, zeroMap == default!);
    var emptyMap = new StrIntMap(new map<@string, nint>{});
    probe("mapEmptyLiteral"u8, emptyMap == default!);
    IntChan zeroChan = default!;
    probe("chanZero"u8, zeroChan == default!);
    var madeChan = new IntChan(1);
    probe("chanMade"u8, madeChan == default!);
}

} // end main_package
