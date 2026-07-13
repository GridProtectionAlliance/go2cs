namespace go;

using fmt = fmt_package;
using time = time_package;

partial class main_package {

internal static nint run(nint n) {
    nint total = 0;
    var buf = keyword();
    for (nint i = 0; i < n; i++) {
        if (((sstring)buf) == "null"u8) {
            total++;
        }
        if (((sstring)buf) == "true"u8) {
            total += 2;
        }
        if (((sstring)buf) == "false"u8) {
            total += 4;
        }
    }
    return total;
}

internal static slice<byte> keyword() {
    @string src = "null"u8;
    var b = new slice<byte>(len(src));
    for (nint i = 0; i < len(src); i++) {
        b[i] = src[i];
    }
    return b;
}

internal static void Main() {
    var start = time.Now().UnixNano();
    nint total = run(20000000);
    var elapsed = time.Now().UnixNano() - start;
    fmt.Println("checksum:", total);
    fmt.Println("elapsed_ns:", elapsed);
}

} // end main_package
