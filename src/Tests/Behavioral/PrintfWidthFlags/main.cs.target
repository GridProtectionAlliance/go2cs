namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    foreach (var (_, v) in new nint[]{5, 99, 999, 1234, -5, -999}.slice()) {
        fmt.Printf("k =% 4d;\n"u8, v);
    }
    fmt.Printf("[% d] [% d] [%+d] [%+d]\n"u8, 7, -7, 7, -7);
    fmt.Printf("[%4d] [%-4d] [%04d] [%04d] [%+4d]\n"u8, 42, 42, 42, -42, 7);
    fmt.Printf("[% 04d] [%+04d]\n"u8, 5, 5);
    fmt.Printf("[%6s] [%-6s] [%6v]\n"u8, "ab", "ab", 12);
    fmt.Printf("[%.1f] [%0.1f] [%6.2f] [%-7.2f] [%.0f]\n"u8, 45.678D, 45.678D, 3.14159D, 3.14159D, 2.71D);
    fmt.Printf("[%2d] [%2s]\n"u8, 12345, "hello");
    fmt.Printf("[% d] [% 6s]\n"u8, -3, "ab");
    var zero = 0.0D;
    var posInf = 1.0D / zero;
    var negInf = -1.0D / zero;
    var nan = zero / zero;
    var posInf32 = (float32)posInf;
    var negInf32 = (float32)negInf;
    fmt.Println(posInf, negInf, nan);
    fmt.Println(posInf32, negInf32);
    fmt.Printf("[%v] [%v] [%g] [%g]\n"u8, posInf, negInf, posInf, negInf);
    fmt.Printf("[%e] [%e] [%f] [%f]\n"u8, posInf, negInf, posInf, negInf);
    fmt.Printf("[%.2f] [%.2f] [%8.2f] [%-8.2f]\n"u8, posInf, negInf, posInf, negInf);
    fmt.Printf("[%+v] [%+f] [% f] [% f]\n"u8, posInf, posInf, posInf, negInf);
    fmt.Printf("[%08f] [%08f] [%08f]\n"u8, posInf, negInf, nan);
    fmt.Printf("[%v] [%g] [%e] [%f]\n"u8, posInf32, negInf32, posInf32, negInf32);
    fmt.Println(fmt.Sprint(posInf), fmt.Sprint(negInf));
    fmt.Printf("[%v] [%f] [%+f] [% f]\n"u8, nan, nan, nan, nan);
    var bits = (uint64)9218868437227405311UL;
    fmt.Printf("[%x] [%X] [%#x] [%#X]\n"u8, bits, bits, bits, bits);
    fmt.Printf("[%x] [%X] [%#x] [%#X]\n"u8, 255, 255, 255, 255);
    fmt.Printf("[%x] [%X] [%#x] [%#X]\n"u8, -255, -255, -255, -255);
    fmt.Printf("[%x] [%#x] [%x] [%#x]\n"u8, 0, 0, 15, 15);
    fmt.Printf("[%8x] [%-8x] [%08x] [%20x]\n"u8, 255, 255, 255, bits);
    fmt.Printf("[%#8x] [%#-8x] [%#08x] [%#016x]\n"u8, 255, 255, 255, bits);
    fmt.Printf("[%08x] [%#08x] [%#08X] [%04x]\n"u8, -255, -255, -255, 255);
    fmt.Printf("[%+x] [% x] [%+#x] [%+08x] [% 08x]\n"u8, 255, 255, 255, 255, 255);
    fmt.Printf("[%.4x] [%.4x] [%#.4x] [%8.4x] [%-8.4x]\n"u8, 255, -255, 255, 255, 255);
    fmt.Printf("[%.0x] [%.0x] [%5.2x] [%05.2x]\n"u8, 0, 255, 255, 255);
    fmt.Printf("[%x] [%#x] [%x] [%x]\n"u8, (int8)(-128), (int8)(-128), (uint8)255, (nuint)255);
    fmt.Printf("[%x] [%#x]\n"u8, (uint64)(nuint)18446744073709551615UL, (int64)(-1));
    fmt.Printf("[%x] [%X] [%#x] [%#X]\n"u8, "abc", "abc", "abc", "abc");
    fmt.Printf("[% x] [% X] [% #x]\n"u8, "abc", "abc", "abc");
    fmt.Printf("[%x] [%#x] [% x]\n"u8, slice<byte>("abc"u8), slice<byte>("abc"u8), new byte[]{1, 2}.slice());
    fmt.Printf("[%x] [%.2x] [%.2x] [%#.2x]\n"u8, "äb", "äb", "abcd", "abcd");
    fmt.Printf("[%6x] [%-6x] [%06x] [%#08x] [%-08x]\n"u8, "ab", "ab", "ab", "ab", "ab");
    fmt.Printf("[% 08x] [%12x] [%10.3x]\n"u8, "ab", "abc", "abcdef");
    fmt.Printf("[%x] [%#x] [%8x] [%08x] [%#x]\n"u8, "", "", "", "", new byte[]{}.slice());
}

} // end main_package
