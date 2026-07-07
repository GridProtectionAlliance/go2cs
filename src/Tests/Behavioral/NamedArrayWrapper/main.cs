global using feAlias = go.array<ulong>;

namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[4]uint64")] partial struct pageBits;

[GoType("pageBits")] partial struct pallocBits;

[GoType("[]uint32")] partial struct pm;

[GoType("[4]byte")] partial struct tb;

internal static void zeroTB(ж<tb> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.Value;

    buf = new tb(new byte[4].array());
}

[GoRecv] internal static void set(this ref pageBits b, nuint i, uint64 v) {
    b.Value[i] = v;
}

[GoRecv] internal static uint64 get(this ref pageBits b, nuint i) {
    return b.Value[i];
}

internal static void fill(this ж<pallocBits> Ꮡb) {
    ref var b = ref Ꮡb.Value;

    for (nint i = 0; i < len(b.Value); i++) {
        b.Value[i] = (uint64)(i * 10 + 1);
    }
}

internal static void Main() {
    ref var e = ref heap(new pallocBits(), out var Ꮡe);
    Ꮡe.fill();
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
    ref var t = ref heap<tb>(out var Ꮡt);
    t = new tb(new byte[4].array());
    fmt.Println(len(t), t[0], t[3]);
    t[2] = 9;
    fmt.Println(t[2]);
    var w = new pm(new uint32[]{}.slice());
    fmt.Println(len(w), cap(w));
    w = append(w, (uint32)(42));
    fmt.Println(len(w), w[0]);
    zeroTB(Ꮡt);
    fmt.Println(t[2], len(t));
    ref var c = ref heap(new callers(), out var Ꮡc);
    var h = new holder(trace: Ꮡc);
    h.trace.Value[0] = 0x10;
    h.trace.Value[1] = h.trace.Value[0] + 2;
    fmt.Println(len(h.trace.Value), h.trace.Value[0], h.trace.Value[1], c[0]);
    var dst = new slice<uintptr>(2);
    nint nc = copy(dst, (~h.trace).Value[..2]);
    fmt.Println(nc, dst[1]);
    ref var cs = ref heap(new counters(), out var Ꮡcs);
    var pcs = Ꮡcs;
    fmt.Println(pcs.at<counter2>(0).bump(), pcs.at<counter2>(0).bump(), cs[0].n);
    ref var sm = ref heap(new scal(), out var Ꮡsm);
    fromBytes(Ꮡ((Ꮡsm.of(scal.Ꮡs)).Value.Value), 7);
    @double(Ꮡsm.of(scal.Ꮡs), Ꮡ((nonMont)((Ꮡsm.of(scal.Ꮡs)).Value.Value)));
    fmt.Println(sm.s[0], sm.s[3]);
    ref var dm = ref heap(new nonMont(), out var Ꮡdm);
    fromBytes(Ꮡ((Ꮡdm).Value.Value), 3);
    fmt.Println(dm[1], dm[2]);
    fromBytes(Ꮡ((Ꮡsm.of(scal.Ꮡs)).Value.Value), 20);
    fmt.Println(sm.s[0], sm.s[3]);
    var grid = new Grid(new unit[]{2, 3, 4}.array());
    fmt.Println(grid.Total(), (nint)grid[0], len(grid));
}

[GoType("num:nint")] public partial struct unit;

[GoType("[3]unit")] partial struct Grid;

public static nint Total(this Grid g) {
    return (nint)g[0] + (nint)g[1] + (nint)g[2];
}

[GoType("[4]uint64")] partial struct mont;

[GoType("[4]uint64")] partial struct nonMont;

[GoType] partial struct scal {
    internal mont s;
}

internal static void fromBytes(ж<array<uint64>> Ꮡout, uint64 seed) {
    ref var @out = ref Ꮡout.Value;

    foreach (var (i, _) in @out) {
        @out[i] = seed + (uint64)i;
    }
}

internal static void @double(ж<mont> Ꮡout, ж<nonMont> Ꮡarg) {
    ref var @out = ref Ꮡout.Value;
    ref var arg = ref Ꮡarg.Value;

    foreach (var (i, _) in @out) {
        @out[i] = arg[i] * 2;
    }
}

[GoType("[4]uintptr")] partial struct callers;

[GoType] partial struct holder {
    internal ж<callers> trace;
}

[GoType] partial struct counter2 {
    internal int32 n;
}

[GoRecv] internal static int32 bump(this ref counter2 c) {
    c.n++;
    return c.n;
}

[GoType("[3]counter2")] partial struct counters;

} // end main_package
