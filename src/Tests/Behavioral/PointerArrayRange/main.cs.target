namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void swap(ж<array<uint64>> Ꮡbuf) {
    ref var buf = ref Ꮡbuf.Value;

    foreach (var (i, x) in buf) {
        buf[i] = (uint64)((x >> (int)(32)) | (x << (int)(32)));
    }
}

internal static void Main() {
    ref var b = ref heap<array<uint64>>(out var Ꮡb);
    b = new uint64[]{0x100000002UL, 0x300000004UL, 5, 0}.array();
    swap(Ꮡb);
    fmt.Println(b[0], b[1], b[2], b[3]);
}

} // end main_package
