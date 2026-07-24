namespace go;

using fmt = fmt_package;
using @unsafe = unsafe_package;

partial class main_package {

[GoType] partial struct Type {
    public uintptr Size_;
    public uintptr PtrBytes;
    public uint32 Hash;
    public TFlag TFlag;
    public uint8 Align_;
    public uint8 FieldAlign_;
    public ΔKind Kind_;
    public Func<@unsafe.Pointer, @unsafe.Pointer, bool> Equal;
    public ж<byte> GCData;
    public NameOff Str;
    public TypeOff PtrToThis;
}

[GoType("num:uint8")] partial struct ΔKind;

public static readonly ΔKind Invalid = /* iota */ 0;
public static readonly ΔKind Bool = 1;
public static readonly ΔKind Int = 2;
public static readonly ΔKind Int8 = 3;
public static readonly ΔKind Int16 = 4;
public static readonly ΔKind Int32 = 5;
public static readonly ΔKind Int64 = 6;
public static readonly ΔKind Uint = 7;
public static readonly ΔKind Uint8 = 8;
public static readonly ΔKind Uint16 = 9;
public static readonly ΔKind Uint32 = 10;
public static readonly ΔKind Uint64 = 11;
public static readonly ΔKind Uintptr = 12;
public static readonly ΔKind Float32 = 13;
public static readonly ΔKind Float64 = 14;
public static readonly ΔKind Complex64 = 15;
public static readonly ΔKind Complex128 = 16;
public static readonly ΔKind Array = 17;
public static readonly ΔKind Chan = 18;
public static readonly ΔKind Func = 19;
public static readonly ΔKind Interface = 20;
public static readonly ΔKind Map = 21;
public static readonly ΔKind Pointer = 22;
public static readonly ΔKind Slice = 23;
public static readonly ΔKind ΔString = 24;
public static readonly ΔKind Struct = 25;
public static readonly ΔKind UnsafePointer = 26;

public static @string String(this ΔKind k) {
    if ((nint)(uint8)k < len(kindNames)) {
        return kindNames[k];
    }
    return kindNames[0];
}

internal static slice<@string> kindNames = new golib.SparseArray<@string>{
    [Invalid] = "invalid"u8,
    [Bool] = "bool"u8,
    [Int] = "int"u8,
    [Int8] = "int8"u8,
    [Int16] = "int16"u8,
    [Int32] = "int32"u8,
    [Int64] = "int64"u8,
    [Uint] = "uint"u8,
    [Uint8] = "uint8"u8,
    [Uint16] = "uint16"u8,
    [Uint32] = "uint32"u8,
    [Uint64] = "uint64"u8,
    [Uintptr] = "uintptr"u8,
    [Float32] = "float32"u8,
    [Float64] = "float64"u8,
    [Complex64] = "complex64"u8,
    [Complex128] = "complex128"u8,
    [Array] = "array"u8,
    [Chan] = "chan"u8,
    [Func] = "func"u8,
    [Interface] = "interface"u8,
    [Map] = "map"u8,
    [Pointer] = "ptr"u8,
    [Slice] = "slice"u8,
    [ΔString] = "string"u8,
    [Struct] = "struct"u8,
    [UnsafePointer] = "unsafe.Pointer"u8
}.slice();

[GoType("num:int32")] partial struct NameOff;

[GoType("num:int32")] partial struct TypeOff;

[GoType("num:int32")] partial struct TextOff;

public static readonly ΔKind KindDirectIface = /* 1 << 5 */ 32;
public static readonly ΔKind KindGCProg = /* 1 << 6 */ 64;
public static readonly ΔKind KindMask = /* (1 << 5) - 1 */ 31;

[GoType("num:uint8")] partial struct TFlag;

public static readonly TFlag TFlagUncommon = /* 1 << 0 */ 1;
public static readonly TFlag TFlagExtraStar = /* 1 << 1 */ 2;
public static readonly TFlag TFlagNamed = /* 1 << 2 */ 4;
public static readonly TFlag TFlagRegularMemory = /* 1 << 3 */ 8;
public static readonly TFlag TFlagUnrolledBitmap = /* 1 << 4 */ 16;

public static @unsafe.Pointer NoEscape(@unsafe.Pointer p) {
    var x = (uintptr)p;
    return (@unsafe.Pointer)((uintptr)(x ^ 0));
}

[GoType] partial struct EmptyInterface {
    public ж<Type> Type;
    public @unsafe.Pointer Data;
}

public static ж<Type> TypeOf(any a) {
    var eface = ~(ж<EmptyInterface>)(uintptr)(new @unsafe.Pointer(Ꮡ(a)));
    return eface.Type;
}

public static ж<Type> TypeFor<T>() {
    T v = default!;
    {
        var t = TypeOf(v); if (t != nil) {
            return t;
        }
    }
    return TypeOf(((ж<T>)nil)).Elem();
}

[GoRecv] public static ΔKind Kind(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindMask);
}

[GoRecv] public static bool HasName(this ref Type t) {
    return (TFlag)(t.TFlag & TFlagNamed) != 0;
}

public static ж<Type> Elem(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Array) {
        var tt = (ж<ΔArrayType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }
    if (exprᴛ1 == Map) {
        var tt = (ж<ΔMapType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }

    return default!;
}

[GoType] partial struct ΔMapType {
    public partial ref Type Type { get; }
    public ж<Type> Key;
    public ж<Type> Elem;
    public ж<Type> Bucket;
    public Func<@unsafe.Pointer, uintptr, uintptr> Hasher;
    public uint8 KeySize;
    public uint8 ValueSize;
    public uint16 BucketSize;
    public uint32 Flags;
}

public static ж<ΔMapType> MapType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Map) {
        return default!;
    }
    return (ж<ΔMapType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
}

[GoType] partial struct ΔArrayType {
    public partial ref Type Type { get; }
    public ж<Type> Elem;
    public ж<Type> Slice;
    public uintptr Len;
}

public static ж<ΔArrayType> ArrayType(this ж<Type> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Array) {
        return default!;
    }
    return (ж<ΔArrayType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
}

internal const nint seqFirst = iota;
internal const nint seqSecond = 1;
internal const nint seqAgain = /* iota */ 2;

internal const int64 wideFirst = /* iota */ 0;
internal const int64 wideNext = 1;

internal static readonly UntypedInt rawZero = iota;
internal static readonly UntypedInt rawOne = /* iota */ 1;

internal static @string stateMachine(nint step) {
    const nint stateInit = iota;
    const nint stateDict = 1;
    const int64 wideLocal = /* iota */ 0;
    int64 offset = wideLocal;
    var exprᴛ1 = step;
    if (exprᴛ1 == stateInit) {
        return fmt.Sprintf("init[%d]"u8, offset);
    }
    if (exprᴛ1 == stateDict) {
        return fmt.Sprintf("dict[%d]"u8, offset);
    }

    return "unknown"u8;
}

internal static void Main() {
    fmt.Printf("Slice Kind Value: %s [%d]\n"u8, Slice.String(), (nint)(uint8)Slice);
    fmt.Println(seqFirst, seqSecond, seqAgain);
    fmt.Println(wideFirst, wideNext);
    fmt.Println(rawZero, rawOne);
    fmt.Println(stateMachine(0));
    fmt.Println(stateMachine(1));
    fmt.Println(stateMachine(2));
}

} // end main_package
