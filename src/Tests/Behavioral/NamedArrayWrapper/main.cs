namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[4]uint64")] partial struct pageBits;

[GoType("pageBits")] partial struct pallocBits;

[GoType("[]uint32")] partial struct pm;

[GoRecv] internal static void set(this ref pageBits b, nuint i, uint64 v) {
    b.val[i] = v;
}

[GoRecv] internal static uint64 get(this ref pageBits b, nuint i) {
    return b.val[i];
}

[GoRecv] internal static void fill(this ref pallocBits b) {
    for (nint i = 0; i < len(b); i++) {
        b.val[i] = (uint64)(i * 10 + 1);
    }
}

internal static void Main() {
    ref var e = ref heap(new pallocBits(), out var Ꮡe);
    e.fill();
    fmt.Println((Ꮡ((pageBits)(~Ꮡe))).get(0), (Ꮡ((pageBits)(~Ꮡe))).get(3));
    (Ꮡ((pageBits)(~Ꮡe))).set(1, 99);
    fmt.Println(e[1]);
    var arr = new uint32[]{10, 20, 30, 40, 50, 60}.array();
    var p = ((pm)(arr[2..5]));
    var d = new slice<uint32>(3);
    nint n = copy(d, p);
    fmt.Println(n, d[0], d[1], d[2]);
    array<uint32> arr2 = new(6);
    nint n2 = copy(arr2[3..], new pm(new uint32[]{7, 8, 9}.slice()));
    fmt.Println(n2, arr2[3], arr2[4], arr2[5], arr2[0]);
    var arr3 = new uint32[]{1, 2, 3, 4, 5, 6}.array();
    var ov = ((pm)(arr3[0..4]));
    nint n3 = copy(arr3[2..6], ov);
    fmt.Println(n3, arr3[2], arr3[3], arr3[4], arr3[5]);
    ref var b = ref heap(new pallocBits(), out var Ꮡb);
    b[0] = 5;
    (Ꮡ((pageBits)(~Ꮡb))).set(2, 30);
    fmt.Println(len(b), b[0] + b[2]);
    var src = new pm(new uint32[]{1, 2, 3, 4}.slice());
    var dd = new slice<uint32>(3);
    fmt.Println(copy(dd, src), dd[2]);
    src[0] = 100;
    fmt.Println(src[0], dd[0]);
}

} // end main_package
