namespace go;

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

public static readonly ΔKind KindDirectIface = /* 1 << 5 */ 32;
public static readonly ΔKind KindGCProg = /* 1 << 6 */ 64;
public static readonly ΔKind KindMask = /* (1 << 5) - 1 */ 31;

[GoType("num:uint8")] partial struct TFlag;

public static readonly TFlag TFlagUncommon = /* 1 << 0 */ 1;
public static readonly TFlag TFlagExtraStar = /* 1 << 1 */ 2;
public static readonly TFlag TFlagNamed = /* 1 << 2 */ 4;
public static readonly TFlag TFlagRegularMemory = /* 1 << 3 */ 8;
public static readonly TFlag TFlagUnrolledBitmap = /* 1 << 4 */ 16;

[GoType("num:int32")] partial struct NameOff;

[GoType("num:int32")] partial struct TypeOff;

[GoType("num:int32")] partial struct TextOff;

public static @string String(this ΔKind k) {
    if (((nint)k) < len(kindNames)) {
        return kindNames[k];
    }
    return kindNames[0];
}

internal static slice<@string> kindNames = new runtime.SparseArray<@string>{
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

public static ж<Type> TypeOf(any a) {
    var eface = ~(ж<EmptyInterface>)(uintptr)(new @unsafe.Pointer(Ꮡ(a)));
    return (ж<Type>)(uintptr)(NoEscape(new @unsafe.Pointer(eface.Type)));
}

public static ж<Type> TypeFor<T>()
    where T : new()
{
    T v = default!;
    {
        var t = TypeOf(v); if (t != nil) {
            return t;
        }
    }
    return TypeOf((ж<T>)(default!)).Elem();
}

[GoRecv] public static ΔKind Kind(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindMask);
}

[GoRecv] public static bool HasName(this ref Type t) {
    return (TFlag)(t.TFlag & TFlagNamed) != 0;
}

[GoRecv] public static bool Pointers(this ref Type t) {
    return t.PtrBytes != 0;
}

[GoRecv] public static bool IfaceIndir(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindDirectIface) == 0;
}

[GoRecv] public static bool IsDirectIface(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindDirectIface) != 0;
}

[GoRecv] public static slice<byte> GcSlice(this ref Type t, uintptr begin, uintptr end) {
    return @unsafe.Slice(t.GCData, ((nint)end))[(int)(begin)..];
}

[GoType] partial struct Method {
    public NameOff Name;
    public TypeOff Mtyp;
    public TextOff Ifn;
    public TextOff Tfn;
}

[GoType] partial struct UncommonType {
    public NameOff PkgPath;
    public uint16 Mcount;
    public uint16 Xcount;
    public uint32 Moff;
    public uint32 _;
}

[GoRecv] public static unsafe slice<Method> Methods(this ref UncommonType t) {
    if (t.Mcount == 0) {
        return default!;
    }
    return new Span<Method>((Method*)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), ((uintptr)t.Moff), "t.mcount > 0"u8)), t.Mcount);
}

[GoRecv] public static unsafe slice<Method> ExportedMethods(this ref UncommonType t) {
    if (t.Xcount == 0) {
        return default!;
    }
    return new Span<Method>((Method*)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), ((uintptr)t.Moff), "t.xcount > 0"u8)), t.Xcount);
}

internal static @unsafe.Pointer addChecked(@unsafe.Pointer p, uintptr x, @string whySafe) {
    return ((@unsafe.Pointer)(((uintptr)p) + x));
}

[GoType] partial struct Imethod {
    public NameOff Name;
    public TypeOff Typ;
}

[GoType] partial struct ΔArrayType {
    public partial ref Type Type { get; }
    public ж<Type> Elem;
    public ж<Type> Slice;
    public uintptr Len;
}

[GoRecv] public static nint Len(this ref Type t) {
    if (t.Kind() == Array) {
        return ((nint)((ж<ΔArrayType>)(uintptr)(@unsafe.Pointer.FromRef(ref t))).val.Len);
    }
    return 0;
}

[GoRecv("capture")] public static ж<Type> Common(this ref Type t) {
    return CommonꓸᏑt;
}

[GoType("num:nint")] partial struct ΔChanDir;

public static readonly ΔChanDir RecvDir = /* 1 << iota */ 1;
public static readonly ΔChanDir SendDir = 2;
public static readonly ΔChanDir BothDir = /* RecvDir | SendDir */ 3;
public static readonly ΔChanDir InvalidDir = 0;

[GoType] partial struct ChanType {
    public partial ref Type Type { get; }
    public ж<Type> Elem;
    public ΔChanDir Dir;
}

[GoType] partial struct structTypeUncommon {
    public partial ref ΔStructType StructType { get; }
    internal UncommonType u;
}

[GoRecv] public static ΔChanDir ChanDir(this ref Type t) {
    if (t.Kind() == Chan) {
        var ch = (ж<ChanType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return (~ch).Dir;
    }
    return InvalidDir;
}

[GoType("dyn")] partial struct Uncommon_u {
    public partial ref PtrType PtrType { get; }
    internal UncommonType u;
}

[GoType("dyn")] partial struct Uncommon_uᴛ1 {
    public partial ref ΔFuncType FuncType { get; }
    internal UncommonType u;
}

[GoType("dyn")] partial struct Uncommon_uᴛ2 {
    public partial ref SliceType SliceType { get; }
    internal UncommonType u;
}

[GoType("dyn")] partial struct Uncommon_uᴛ3 {
    public partial ref ΔArrayType ArrayType { get; }
    internal UncommonType u;
}

[GoType("dyn")] partial struct Uncommon_uᴛ4 {
    public partial ref ChanType ChanType { get; }
    internal UncommonType u;
}

[GoType("dyn")] partial struct Uncommon_uᴛ5 {
    public partial ref ΔMapType MapType { get; }
    internal UncommonType u;
}

[GoType("dyn")] partial struct Uncommon_uᴛ6 {
    public partial ref ΔInterfaceType InterfaceType { get; }
    internal UncommonType u;
}

[GoType("dyn")] partial struct Uncommon_uᴛ7 {
    public partial ref Type Type { get; }
    internal UncommonType u;
}

[GoRecv] public static ж<UncommonType> Uncommon(this ref Type t) {
    if ((TFlag)(t.TFlag & TFlagUncommon) == 0) {
        return default!;
    }
    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Struct) {
        return Ꮡ(((ж<structTypeUncommon>)(uintptr)(@unsafe.Pointer.FromRef(ref t))).val.u);
    }
    if (exprᴛ1 == Pointer) {
        return Ꮡ(((ж<Uncommon_u>)(uintptr)(@unsafe.Pointer.FromRef(ref t))).val.u);
    }
    if (exprᴛ1 == Func) {
        return Ꮡ(((ж<Uncommon_uᴛ1>)(uintptr)(@unsafe.Pointer.FromRef(ref t))).val.u);
    }
    if (exprᴛ1 == Slice) {
        return Ꮡ(((ж<Uncommon_uᴛ2>)(uintptr)(@unsafe.Pointer.FromRef(ref t))).val.u);
    }
    if (exprᴛ1 == Array) {
        return Ꮡ(((ж<Uncommon_uᴛ3>)(uintptr)(@unsafe.Pointer.FromRef(ref t))).val.u);
    }
    if (exprᴛ1 == Chan) {
        return Ꮡ(((ж<Uncommon_uᴛ4>)(uintptr)(@unsafe.Pointer.FromRef(ref t))).val.u);
    }
    if (exprᴛ1 == Map) {
        return Ꮡ(((ж<Uncommon_uᴛ5>)(uintptr)(@unsafe.Pointer.FromRef(ref t))).val.u);
    }
    if (exprᴛ1 == Interface) {
        return Ꮡ(((ж<Uncommon_uᴛ6>)(uintptr)(@unsafe.Pointer.FromRef(ref t))).val.u);
    }
    { /* default: */
        return Ꮡ(((ж<Uncommon_uᴛ7>)(uintptr)(@unsafe.Pointer.FromRef(ref t))).val.u);
    }

}

[GoRecv] public static ж<Type> Elem(this ref Type t) {
    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Array) {
        var tt = (ж<ΔArrayType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }
    if (exprᴛ1 == Chan) {
        var tt = (ж<ChanType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }
    if (exprᴛ1 == Map) {
        var tt = (ж<ΔMapType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }
    if (exprᴛ1 == Pointer) {
        var tt = (ж<PtrType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }
    if (exprᴛ1 == Slice) {
        var tt = (ж<SliceType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }

    return default!;
}

[GoRecv] public static ж<ΔStructType> StructType(this ref Type t) {
    if (t.Kind() != Struct) {
        return default!;
    }
    return (ж<ΔStructType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
}

[GoRecv] public static ж<ΔMapType> MapType(this ref Type t) {
    if (t.Kind() != Map) {
        return default!;
    }
    return (ж<ΔMapType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
}

[GoRecv] public static ж<ΔArrayType> ArrayType(this ref Type t) {
    if (t.Kind() != Array) {
        return default!;
    }
    return (ж<ΔArrayType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
}

[GoRecv] public static ж<ΔFuncType> FuncType(this ref Type t) {
    if (t.Kind() != Func) {
        return default!;
    }
    return (ж<ΔFuncType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
}

[GoRecv] public static ж<ΔInterfaceType> InterfaceType(this ref Type t) {
    if (t.Kind() != Interface) {
        return default!;
    }
    return (ж<ΔInterfaceType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
}

[GoRecv] public static uintptr Size(this ref Type t) {
    return t.Size_;
}

[GoRecv] public static nint Align(this ref Type t) {
    return ((nint)t.Align_);
}

[GoRecv] public static nint FieldAlign(this ref Type t) {
    return ((nint)t.FieldAlign_);
}

[GoType] partial struct ΔInterfaceType {
    public partial ref Type Type { get; }
    public ΔName PkgPath;
    public slice<Imethod> Methods;
}

[GoRecv] public static slice<Method> ExportedMethods(this ref Type t) {
    var ut = t.Uncommon();
    if (ut == nil) {
        return default!;
    }
    return ut.ExportedMethods();
}

[GoRecv] public static nint NumMethod(this ref Type t) {
    if (t.Kind() == Interface) {
        var tt = (ж<ΔInterfaceType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return tt.NumMethod();
    }
    return len(t.ExportedMethods());
}

[GoRecv] public static nint NumMethod(this ref ΔInterfaceType t) {
    return len(t.Methods);
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

[GoRecv] public static bool IndirectKey(this ref ΔMapType mt) {
    return (uint32)(mt.Flags & 1) != 0;
}

[GoRecv] public static bool IndirectElem(this ref ΔMapType mt) {
    return (uint32)(mt.Flags & 2) != 0;
}

[GoRecv] public static bool ReflexiveKey(this ref ΔMapType mt) {
    return (uint32)(mt.Flags & 4) != 0;
}

[GoRecv] public static bool NeedKeyUpdate(this ref ΔMapType mt) {
    return (uint32)(mt.Flags & 8) != 0;
}

[GoRecv] public static bool HashMightPanic(this ref ΔMapType mt) {
    return (uint32)(mt.Flags & 16) != 0;
}

[GoRecv] public static ж<Type> Key(this ref Type t) {
    if (t.Kind() == Map) {
        return ((ж<ΔMapType>)(uintptr)(@unsafe.Pointer.FromRef(ref t))).val.Key;
    }
    return default!;
}

[GoType] partial struct SliceType {
    public partial ref Type Type { get; }
    public ж<Type> Elem;
}

[GoType] partial struct ΔFuncType {
    public partial ref Type Type { get; }
    public uint16 InCount;
    public uint16 OutCount;
}

[GoRecv] public static ж<Type> In(this ref ΔFuncType t, nint i) {
    return t.InSlice()[i];
}

[GoRecv] public static nint NumIn(this ref ΔFuncType t) {
    return ((nint)t.InCount);
}

[GoRecv] public static nint NumOut(this ref ΔFuncType t) {
    return ((nint)((uint16)(t.OutCount & (1 << (int)(15) - 1))));
}

[GoRecv] public static ж<Type> Out(this ref ΔFuncType t, nint i) {
    return (t.OutSlice()[i]);
}

[GoRecv] public static unsafe slice<ж<Type>> InSlice(this ref ΔFuncType t) {
    var uadd = @unsafe.Sizeof(t);
    if ((TFlag)(t.TFlag & TFlagUncommon) != 0) {
        uadd += @unsafe.Sizeof(new UncommonType(nil));
    }
    if (t.InCount == 0) {
        return default!;
    }
    return new Span<ж<Type>>((Type**)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), uadd, "t.inCount > 0"u8)), t.InCount);
}

[GoRecv] public static unsafe slice<ж<Type>> OutSlice(this ref ΔFuncType t) {
    var outCount = ((uint16)t.NumOut());
    if (outCount == 0) {
        return default!;
    }
    var uadd = @unsafe.Sizeof(t);
    if ((TFlag)(t.TFlag & TFlagUncommon) != 0) {
        uadd += @unsafe.Sizeof(new UncommonType(nil));
    }
    return new Span<ж<Type>>((Type**)(uintptr)(addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), uadd, "outCount > 0"u8)), t.InCount + outCount);
}

[GoRecv] public static bool IsVariadic(this ref ΔFuncType t) {
    return (uint16)(t.OutCount & (1 << (int)(15))) != 0;
}

[GoType] partial struct PtrType {
    public partial ref Type Type { get; }
    public ж<Type> Elem;
}

[GoType] partial struct StructField {
    public ΔName Name;
    public ж<Type> Typ;
    public uintptr Offset;
}

[GoRecv] public static bool Embedded(this ref StructField f) {
    return f.Name.IsEmbedded();
}

[GoType] partial struct ΔStructType {
    public partial ref Type Type { get; }
    public ΔName PkgPath;
    public slice<StructField> Fields;
}

[GoType] partial struct ΔName {
    public ж<byte> Bytes;
}

public static ж<byte> DataChecked(this ΔName n, nint off, @string whySafe) {
    return (ж<byte>)(uintptr)(addChecked(new @unsafe.Pointer(n.Bytes), ((uintptr)off), whySafe));
}

public static ж<byte> Data(this ΔName n, nint off) {
    return (ж<byte>)(uintptr)(addChecked(new @unsafe.Pointer(n.Bytes), ((uintptr)off), "the runtime doesn't need to give you a reason"u8));
}

public static bool IsExported(this ΔName n) {
    return (byte)((n.Bytes.val) & (1 << (int)(0))) != 0;
}

public static bool HasTag(this ΔName n) {
    return (byte)((n.Bytes.val) & (1 << (int)(1))) != 0;
}

public static bool IsEmbedded(this ΔName n) {
    return (byte)((n.Bytes.val) & (1 << (int)(3))) != 0;
}

public static (nint, nint) ReadVarint(this ΔName n, nint off) {
    nint v = 0;
    for (nint i = 0; ᐧ ; i++) {
        var x = n.DataChecked(off + i, "read varint"u8).val;
        v += ((nint)((byte)(x & 127))) << (int)((7 * i));
        if ((byte)(x & 128) == 0) {
            return (i + 1, v);
        }
    }
}

public static bool IsBlank(this ΔName n) {
    if (n.Bytes == nil) {
        return false;
    }
    var (_, l) = n.ReadVarint(1);
    return l == 1 && n.Data(2).val == (rune)'_';
}

internal static nint writeVarint(slice<byte> buf, nint n) {
    for (nint i = 0; ᐧ ; i++) {
        var b = ((byte)((nint)(n & 127)));
        n >>= (UntypedInt)(7);
        if (n == 0) {
            buf[i] = b;
            return i + 1;
        }
        buf[i] = (byte)(b | 128);
    }
}

public static @string Name(this ΔName n) {
    if (n.Bytes == nil) {
        return ""u8;
    }
    var (i, l) = n.ReadVarint(1);
    return @unsafe.String(n.DataChecked(1 + i, "non-empty string"u8), l);
}

public static @string Tag(this ΔName n) {
    if (!n.HasTag()) {
        return ""u8;
    }
    var (i, l) = n.ReadVarint(1);
    var (i2, l2) = n.ReadVarint(1 + i + l);
    return @unsafe.String(n.DataChecked(1 + i + l + i2, "non-empty string"u8), l2);
}

public static ΔName NewName(@string n, @string tag, bool exported, bool embedded) {
    if (len(n) >= 1 << (int)(29)) {
        panic("abi.NewName: name too long: " + n[..1024] + "...");
    }
    if (len(tag) >= 1 << (int)(29)) {
        panic("abi.NewName: tag too long: " + tag[..1024] + "...");
    }
    array<byte> nameLen = new(10);
    array<byte> tagLen = new(10);
    nint nameLenLen = writeVarint(nameLen[..], len(n));
    nint tagLenLen = writeVarint(tagLen[..], len(tag));
    byte bits = default!;
    nint l = 1 + nameLenLen + len(n);
    if (exported) {
        bits |= (byte)(1 << (int)(0));
    }
    if (len(tag) > 0) {
        l += tagLenLen + len(tag);
        bits |= (byte)(1 << (int)(1));
    }
    if (embedded) {
        bits |= (byte)(1 << (int)(3));
    }
    var b = new slice<byte>(l);
    b[0] = bits;
    copy(b[1..], nameLen[..(int)(nameLenLen)]);
    copy(b[(int)(1 + nameLenLen)..], n);
    if (len(tag) > 0) {
        var tb = b[(int)(1 + nameLenLen + len(n))..];
        copy(tb, tagLen[..(int)(tagLenLen)]);
        copy(tb[(int)(tagLenLen)..], tag);
    }
    return new ΔName(Bytes: Ꮡ(b, 0));
}

public static readonly UntypedInt TraceArgsLimit = 10;
public static readonly UntypedInt TraceArgsMaxDepth = 5;
public static readonly UntypedInt TraceArgsMaxLen = /* (TraceArgsMaxDepth*3+2)*TraceArgsLimit + 1 */ 171;

public static readonly UntypedInt TraceArgsEndSeq = /* 0xff */ 255;
public static readonly UntypedInt TraceArgsStartAgg = /* 0xfe */ 254;
public static readonly UntypedInt TraceArgsEndAgg = /* 0xfd */ 253;
public static readonly UntypedInt TraceArgsDotdotdot = /* 0xfc */ 252;
public static readonly UntypedInt TraceArgsOffsetTooLarge = /* 0xfb */ 251;
public static readonly UntypedInt TraceArgsSpecial = /* 0xf0 */ 240;

public static readonly UntypedInt MaxPtrmaskBytes = 2048;

} // end main_package
