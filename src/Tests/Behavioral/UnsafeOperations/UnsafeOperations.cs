namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

[GoType] partial struct T1 {
    public int32 a;
}

[GoType] partial struct T2 {
    public int32 a;
}

public static uint64 Float64bits(float64 f) {
    return ~(ж<uint64>)(uintptr)(new @unsafe.Pointer(Ꮡ(f)));
}

public static float64 Float64frombits(uint64 b) {
    return ~(ж<float64>)(uintptr)(new @unsafe.Pointer(Ꮡ(b)));
}

[GoType] partial struct main_x {
    public int64 a;
    public bool b;
    public @string c;
}

internal static void Main() {
    var b = new byte[]{}.slice();
    for (nint ch = 32; ch < 80; ch++) {
        b = append(b, ((@string)((rune)ch)).ꓸꓸꓸ);
    }
    @string str = @unsafe.String(Ꮡ(b, 0), len(b));
    fmt.Println(str);
    var strptr = @unsafe.StringData(str);
    fmt.Println(@unsafe.String(strptr, len(str)));
    ref var arr = ref heap<array<nint>>(out var Ꮡarr);
    arr = new nint[]{1, 2, 3, 4}.array();
    var arrptr = Ꮡarr.at<nint>(0);
    // Move the pointer to the next element in the array
    var nextPtr = ((@unsafe.Pointer)(((uintptr)new @unsafe.Pointer(arrptr)) + @unsafe.Sizeof(arr[0])));
    fmt.Println("Value of the next element:", ~(ж<nint>)(uintptr)(nextPtr));
    ref var t1 = ref heap(new T1(), out var Ꮡt1);
    t1.a = 42;
    // Convert t1 to type T2
    var t2 = ~(ж<T2>)(uintptr)(new @unsafe.Pointer(Ꮡt1));
    fmt.Println("Value of t2.a:", t2.a);
    ref var i = ref heap(new int8(), out var Ꮡi);
    i = -1;
    int16 j = ((int16)i);
    fmt.Println(i, j);
    uint8 k = ~(ж<uint8>)(uintptr)(new @unsafe.Pointer(Ꮡi));
    fmt.Println(k);
    main_x x = default!;
    const uintptr M = /* unsafe.Sizeof(x.c) */ 16;
    const uintptr N = /* unsafe.Sizeof(x) */ 32;
    fmt.Println(M, N);
    fmt.Println(@unsafe.Alignof(x.GetType(), "a"));
    fmt.Println(@unsafe.Alignof(x.GetType(), "b"));
    fmt.Println(@unsafe.Alignof(x.GetType(), "c"));
    fmt.Println(@unsafe.Offsetof(x.GetType(), "a"));
    fmt.Println(@unsafe.Offsetof(x.GetType(), "b"));
    fmt.Println(@unsafe.Offsetof(x.GetType(), "c"));
    var i2 = Float64bits(9.5F);
    var f2 = Float64frombits(i2);
    fmt.Println(i2);
    fmt.Println(f2);
}

} // end main_package
