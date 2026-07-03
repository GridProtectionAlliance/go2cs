namespace go;

using fmt = fmt_package;

partial class main_package {

internal static readonly UntypedInt two32 = /* 1 << 32 */ 4294967296;

internal static int64 sum(int64 n) {
    return n;
}

internal static void Main() {
    uint64 a = 100;
    uint64 b = 3;
    fmt.Println(a * (uint64)two32 + b);
    fmt.Println((uint64)two32 * a);
    fmt.Println(a >= two32);
    fmt.Println(a - (uint64)two32 % a);
    int64 c = 1099511627783L;
    var d = (int64)(9223372036854775807L);
    fmt.Println(c);
    fmt.Println(d);
    fmt.Println(sum(12345000054321L));
    var n = (int64)7;
    var max = (int64)(9223372036854775807UL - (((uint64)1 << (int)(63))) % (uint64)n);
    fmt.Println(max);
}

} // end main_package
