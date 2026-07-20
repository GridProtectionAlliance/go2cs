namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    var i32 = new int32[]{0, 1, (int32)(1 << (int)(20)), (1 << (int)(20)) + 1, (int32)(2147483648L - 2), (int32)(2147483648L - 1)}.slice();
    var i64 = new int64[]{1152921504606846976L, 4611686018427387905L, 9223372036854775807L}.slice();
    var i16 = new int16[]{(int16)((1 << (int)(15)) - 1)}.slice();
    var u32 = new uint32[]{(uint32)(4294967296L - 1)}.slice();
    var u64 = new uint64[]{(uint64)(1099511627776L + 1), 18446744073709551615UL}.slice();
    var ptr = new uintptr[]{(uintptr)(1099511627776L + 1)}.slice();
    var ints = new nint[]{(nint)(2147483648L - 1)}.slice();
    int32 n32 = (int32)(2147483648L - 1);
    var c32 = (int32)(2147483648L - 1);
    uintptr nptr = (uintptr)(1099511627776L + 1);
    foreach (var (_, v) in i32) {
        fmt.Println(v);
    }
    foreach (var (_, v) in i64) {
        fmt.Println(v);
    }
    foreach (var (_, v) in i16) {
        fmt.Println(v);
    }
    foreach (var (_, v) in u32) {
        fmt.Println(v);
    }
    foreach (var (_, v) in u64) {
        fmt.Println(v);
    }
    foreach (var (_, v) in ptr) {
        fmt.Println(v);
    }
    foreach (var (_, v) in ints) {
        fmt.Println(v);
    }
    fmt.Println(n32, c32, nptr);
    showInt32((int32)(2147483648L - 2));
    showUint32((uint32)(4294967296L - 1));
    var words = new Word[]{(nuint)(9223372036854775809UL), (nuint)(9223372036854775807UL)}.slice();
    var wide = new uintptr[]{(nuint)(9223372036854775809UL), (nuint)(9223372036854775807UL)}.slice();
    var uns = new nuint[]{(nuint)(9223372036854775809UL)}.slice();
    var u64wide = new uint64[]{9223372036854775809UL}.slice();
    foreach (var (_, v) in words) {
        fmt.Println(v);
    }
    foreach (var (_, v) in wide) {
        fmt.Println(v);
    }
    foreach (var (_, v) in uns) {
        fmt.Println(v);
    }
    foreach (var (_, v) in u64wide) {
        fmt.Println(v);
    }
}

[GoType("num:uintptr")] partial struct Word;

internal static readonly UntypedInt _W = 64;
internal static readonly GoUntyped _B = /* 1 << _W */
    GoUntyped.Parse("18446744073709551616");
internal static readonly UntypedInt _M = /* _B - 1 */ 18446744073709551615;

internal static void showInt32(int32 v) {
    fmt.Println(v);
}

internal static void showUint32(uint32 v) {
    fmt.Println(v);
}

} // end main_package
