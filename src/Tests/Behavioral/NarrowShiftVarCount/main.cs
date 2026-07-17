namespace go;

using fmt = fmt_package;

partial class main_package {

internal const byte tcb = 200;

internal static readonly UntypedInt ucb = 200;

internal const uint16 tcw = 40000;

[GoType("num:byte")] partial struct nb;

internal static void Main() {
    nuint k = 1;
    nint ki = 9;
    byte b = 1;
    byte cb = 200;
    uint16 w = 40000;
    int8 s8 = -100;
    int16 s16 = -30000;
    nb n = 200;
    fmt.Println(b + (byte)(cb << (int)(k)));
    fmt.Println((byte)(cb << (int)(k)));
    fmt.Println((byte)(tcb << (int)(k)));
    fmt.Println(b + (byte)(ucb << (int)(k)));
    fmt.Println((byte)(cb << (int)(3)));
    fmt.Println((byte)(cb << (int)(ki)));
    fmt.Println((uint16)(w << (int)(k)));
    fmt.Println((uint16)(tcw << (int)(k)));
    fmt.Println((int8)(s8 << (int)(k)));
    fmt.Println((int16)(s16 << (int)(k)));
    fmt.Println((nb)(byte)(n << (int)(k)));
    fmt.Println((cb >> (int)(k)));
    fmt.Println((s8 >> (int)(k)));
    var x = (byte)(cb << (int)(k));
    fmt.Println(x + b);
}

} // end main_package
