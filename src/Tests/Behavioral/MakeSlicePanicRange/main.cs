namespace go;

using fmt = fmt_package;

partial class main_package {

internal static any /*r*/ tryMake(nint length, nint capacity) {
    any r = default!;
    func((defer, recover) => {
        defer(() => {
            r = recover();
        });
        slice<byte> b = default!;
        if (capacity < 0){
            b = new slice<byte>(length);
        } else {
            b = new slice<byte>(length, capacity);
        }
        _ = b;
        r = default!;
    });
    return r;
}

internal static void Main() {
    fmt.Println(tryMake(4, -1));
    fmt.Println(tryMake(0, -1));
    fmt.Println(tryMake(-1, -1));
    fmt.Println(tryMake((nint)(4611686018427387904L), -1));
    fmt.Println(tryMake(1, (nint)(4611686018427387904L)));
}

} // end main_package
