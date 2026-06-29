namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{10, 20, 30, 40, 50}.array();
    var p = Ꮡarr;
    var full = (~p)[..];
    var low = (~p)[2..];
    var high = (~p)[..3];
    var mid = (~p)[1..4];
    var three = (~p).slice(1, 3, 4);
    fmt.Println(full);
    fmt.Println(low);
    fmt.Println(high);
    fmt.Println(mid);
    fmt.Println(three, len(three), cap(three));
    full[0] = 99;
    fmt.Println(arr[0]);
}

} // end main_package
