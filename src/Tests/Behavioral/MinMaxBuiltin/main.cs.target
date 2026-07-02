namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly UntypedInt limit = /* 128 << 10 */ 131072;

internal static readonly UntypedInt floor = 16;

internal static uintptr clampU(uintptr n) {
    return min(n, (uintptr)(limit));
}

internal static int32 clampI(int32 d) {
    return max(d, (int32)(floor));
}

internal static void Main() {
    fmt.Println(min(3, 7));
    fmt.Println(max(3, 7));
    fmt.Println(min(5, 2, 9, 1, 4));
    fmt.Println(max(5, 2, 9, 1, 4));
    fmt.Println(min(42));
    fmt.Println(min(2.5D, 1.5D));
    fmt.Println(max(2.5D, 1.5D));
    fmt.Println(min("banana", "apple", "cherry"));
    fmt.Println(max("banana", "apple", "cherry"));
    var x = new byte[]{1, 2, 3}.slice();
    var y = new byte[]{1, 2, 3, 4, 5}.slice();
    nint n = min(len(x), len(y));
    fmt.Println(n);
    fmt.Println(clampU(999999), clampU(7));
    fmt.Println(clampI(3), clampI(100));
    uintptr big = 200000;
    fmt.Println(min(big, (uintptr)(limit), (uintptr)(500)));
}

} // end main_package
