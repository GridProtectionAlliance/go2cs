namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void probe(@string name, bool isNil, nint length, nint capacity) {
    fmt.Println(name, isNil, length, capacity);
}

internal static void Main() {
    slice<byte> zero = default!;
    probe("zeroValue"u8, zero == default!, len(zero), cap(zero));
    var emptyLit = new byte[]{}.slice();
    probe("emptyLiteral"u8, emptyLit == default!, len(emptyLit), cap(emptyLit));
    var emptyStr = slice<byte>(""u8);
    probe("emptyStringConv"u8, emptyStr == default!, len(emptyStr), cap(emptyStr));
    var nilConv = slice<byte>(default!);
    probe("nilConversion"u8, nilConv == default!, len(nilConv), cap(nilConv));
    var nilReslice = zero[0..0];
    probe("nilReslice"u8, nilReslice == default!, len(nilReslice), cap(nilReslice));
    var made = new slice<byte>(0);
    probe("makeZero"u8, made == default!, len(made), cap(made));
    var x = new byte[]{1, 2}.slice();
    probe("resliceToEmpty"u8, x[..0] == default!, len(x[..0]), cap(x[..0]));
    var tail = x[2..2];
    probe("resliceTailCapZero"u8, tail == default!, len(tail), cap(tail));
    var appendNilNothing = append(slice<byte>(default!));
    probe("appendNilNothing"u8, appendNilNothing == default!, len(appendNilNothing), cap(appendNilNothing));
    var appendEmptyNothing = append(new byte[]{}.slice());
    probe("appendEmptyNothing"u8, appendEmptyNothing == default!, len(appendEmptyNothing), cap(appendEmptyNothing));
    var cloneShape = append(new byte[]{}.slice(), x[..0].ꓸꓸꓸ);
    probe("cloneShape"u8, cloneShape == default!, len(cloneShape), cap(cloneShape));
    var trim = x;
    while (len(trim) > 0) {
        trim = trim[..(int)(len(trim) - 1)];
    }
    probe("trimShape"u8, trim == default!, len(trim), cap(trim));
    slice<@string> zs = default!;
    probe("stringSliceZero"u8, zs == default!, len(zs), cap(zs));
    probe("stringSliceEmpty"u8, new @string[]{}.slice() == default!, len(new @string[]{}.slice()), cap(new @string[]{}.slice()));
}

} // end main_package
