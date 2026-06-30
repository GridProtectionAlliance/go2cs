namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("num:uint64")] partial struct traceArg;

[GoType("num:nuint")] partial struct arenaIdx;

[GoType("num:int32")] partial struct nameOff;

[GoType("num:nuint")] partial struct idx;

internal static void Main() {
    int32 procs = 5;
    var a = ((traceArg)(uint64)procs);
    fmt.Println((uint64)a);
    nint x = (1 << (int)(4));
    arenaIdx b = ((arenaIdx)(nuint)x);
    fmt.Println((nuint)b);
    traceGoStatus g = 3;
    var c = ((traceArg)(uint64)(uint8)g);
    fmt.Println((uint64)c);
    uint64 u = 9;
    var d = ((traceArg)u);
    fmt.Println((uint64)d);
    nameOff s = 7;
    var e = (uint64)(int32)s;
    fmt.Println(e);
    idx i = 9;
    nint f = (nint)(nuint)i;
    fmt.Println(f);
    nameOff s2 = 13;
    var h = (int32)s2;
    fmt.Println(h);
}

[GoType("num:uint8")] partial struct traceGoStatus;

} // end main_package
