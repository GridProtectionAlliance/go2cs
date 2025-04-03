// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using @unsafe = unsafe_package;

partial class abi_package {

// Type is the runtime representation of a Go type.
//
// Be careful about accessing this type at build time, as the version
// of this type in the compiler/linker may not have the same layout
// as the version in the target binary, due to pointer width
// differences and any experiments. Use cmd/compile/internal/rttype
// or the functions in compiletype.go to access this type instead.
// (TODO: this admonition applies to every type in this package.
// Put it in some shared location?)
[GoType] partial struct Type {
    public uintptr Size_;
    public uintptr PtrBytes; // number of (prefix) bytes in the type that can contain pointers
    public uint32 Hash;  // hash of type; avoids computation in hash tables
    public TFlag TFlag;   // extra type information flags
    public uint8 Align_;   // alignment of variable with this type
    public uint8 FieldAlign_;   // alignment of struct field with this type
    public ΔKind Kind_;  // enumeration for C
    // function for comparing objects of this type
// (ptr to object A, ptr to object B) -> ==?
    public Func<@unsafe.Pointer, @unsafe.Pointer, bool> Equal;
    // GCData stores the GC type data for the garbage collector.
// If the KindGCProg bit is set in kind, GCData is a GC program.
// Otherwise it is a ptrmask bitmap. See mbitmap.go for details.
    public ж<byte> GCData;
    public NameOff Str; // string form
    public TypeOff PtrToThis; // type for pointer to this type, may be zero
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
public static readonly ΔKind KindGCProg = /* 1 << 6 */ 64;       // Type.gc points to GC program
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

// String returns the name of k.
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

// TypeOf returns the abi.Type of some value.
public static ж<Type> TypeOf(any a) {
    var eface = ~(ж<EmptyInterface>)(uintptr)(new @unsafe.Pointer(Ꮡ(a)));
    // Types are either static (for compiler-created types) or
    // heap-allocated but always reachable (for reflection-created
    // types, held in the central map). So there is no need to
    // escape types. noescape here help avoid unnecessary escape
    // of v.
    return (ж<Type>)((uintptr)NoEscape(new @unsafe.Pointer(eface.Type)));
}

// TypeFor returns the abi.Type for a type parameter.
public static ж<Type> TypeFor<T>()
    where T : new()
{
    T v = default!;
    {
        var t = TypeOf(v); if (t != nil) {
            return t;
        }
    }
    // optimize for T being a non-interface kind
    return TypeOf((ж<T>)(default!)).Elem();
}

// only for an interface kind
[GoRecv] internal static ΔKind Kind(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindMask);
}

[GoRecv] internal static bool HasName(this ref Type t) {
    return (TFlag)(t.TFlag & TFlagNamed) != 0;
}

// Pointers reports whether t contains pointers.
[GoRecv] internal static bool Pointers(this ref Type t) {
    return t.PtrBytes != 0;
}

// IfaceIndir reports whether t is stored indirectly in an interface value.
[GoRecv] internal static bool IfaceIndir(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindDirectIface) == 0;
}

// isDirectIface reports whether t is stored directly in an interface value.
[GoRecv] internal static bool IsDirectIface(this ref Type t) {
    return (ΔKind)(t.Kind_ & KindDirectIface) != 0;
}

[GoRecv] internal static slice<byte> GcSlice(this ref Type t, uintptr begin, uintptr end) {
    return @unsafe.Slice(t.GCData, ((nint)end))[(int)(begin)..];
}

// Method on non-interface type
[GoType] partial struct Method {
    public NameOff ΔName; // name of method
    public TypeOff Mtyp; // method type (without receiver)
    public TextOff Ifn; // fn used in interface call (one-word receiver)
    public TextOff Tfn; // fn used for normal method call
}

// UncommonType is present only for defined types or types with methods
// (if T is a defined type, the uncommonTypes for T and *T have methods).
// Using a pointer to this struct reduces the overall size required
// to describe a non-defined type with no methods.
[GoType] partial struct UncommonType {
    public NameOff PkgPath; // import path; empty for built-in types like int, string
    public uint16 Mcount;  // number of methods
    public uint16 Xcount;  // number of exported methods
    public uint32 Moff;  // offset from this uncommontype to [mcount]Method
    public uint32 _;  // unused
}

[GoRecv] internal static slice<Method> Methods(this ref UncommonType t) {
    if (t.Mcount == 0) {
        return default!;
    }
    return (ж<Method>)((uintptr)addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), ((uintptr)t.Moff), "t.mcount > 0"u8)).slice(-1, t.Mcount, t.Mcount);
}

[GoRecv] internal static slice<Method> ExportedMethods(this ref UncommonType t) {
    if (t.Xcount == 0) {
        return default!;
    }
    return (ж<Method>)((uintptr)addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), ((uintptr)t.Moff), "t.xcount > 0"u8)).slice(-1, t.Xcount, t.Xcount);
}

// addChecked returns p+x.
//
// The whySafe string is ignored, so that the function still inlines
// as efficiently as p+x, but all call sites should use the string to
// record why the addition is safe, which is to say why the addition
// does not cause x to advance to the very end of p's allocation
// and therefore point incorrectly at the next block in memory.
internal static @unsafe.Pointer addChecked(@unsafe.Pointer p, uintptr x, @string whySafe) {
    return ((@unsafe.Pointer)(((uintptr)p) + x));
}

// Imethod represents a method on an interface type
[GoType] partial struct Imethod {
    public NameOff ΔName; // name of method
    public TypeOff Typ; // .(*FuncType) underneath
}

// ArrayType represents a fixed array type.
[GoType] partial struct ΔArrayType {
    public partial ref Type Type { get; }
    public ж<Type> Elem; // array element type
    public ж<Type> Slice; // slice type
    public uintptr Len;
}

// Len returns the length of t if t is an array type, otherwise 0
[GoRecv] internal static nint Len(this ref Type t) {
    if (t.Kind() == Array) {
        return ((nint)(ж<ΔArrayType>)((uintptr)@unsafe.Pointer.FromRef(ref t)).Len);
    }
    return 0;
}

[GoRecv] internal static ж<Type> Common(this ref Type t) {
    return t;
}

[GoType("num:nint")] partial struct ΔChanDir;

public static readonly ΔChanDir RecvDir = /* 1 << iota */ 1;                // <-chan
public static readonly ΔChanDir SendDir = 2;                // chan<-
public static readonly ΔChanDir BothDir = /* RecvDir | SendDir */ 3; // chan
public static readonly ΔChanDir InvalidDir = 0;

// ChanType represents a channel type
[GoType] partial struct ChanType {
    public partial ref Type Type { get; }
    public ж<Type> Elem;
    public ΔChanDir Dir;
}

[GoType] partial struct structTypeUncommon {
    public partial ref ΔStructType ΔStructType { get; }
    public UncommonType u;
}

// ChanDir returns the direction of t if t is a channel type, otherwise InvalidDir (0).
[GoRecv] internal static ΔChanDir ChanDir(this ref Type t) {
    if (t.Kind() == Chan) {
        var ch = (ж<ChanType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
        return (~ch).Dir;
    }
    return InvalidDir;
}

[GoType] partial struct Uncommon_u {
    public partial ref PtrType PtrType { get; }
    public UncommonType u;
}

[GoType] partial struct Uncommon_uᴛ1 {
    public partial ref ΔFuncType ΔFuncType { get; }
    public UncommonType u;
}

[GoType] partial struct Uncommon_uᴛ2 {
    public partial ref SliceType SliceType { get; }
    public UncommonType u;
}

[GoType] partial struct Uncommon_uᴛ3 {
    public partial ref ΔArrayType ΔArrayType { get; }
    public UncommonType u;
}

[GoType] partial struct Uncommon_uᴛ4 {
    public partial ref ChanType ChanType { get; }
    public UncommonType u;
}

[GoType] partial struct Uncommon_uᴛ5 {
    public partial ref ΔMapType ΔMapType { get; }
    public UncommonType u;
}

[GoType] partial struct Uncommon_uᴛ6 {
    public partial ref ΔInterfaceType ΔInterfaceType { get; }
    public UncommonType u;
}

[GoType] partial struct Uncommon_uᴛ7 {
    public partial ref Type Type { get; }
    public UncommonType u;
}

// Uncommon returns a pointer to T's "uncommon" data if there is any, otherwise nil
[GoRecv] internal static ж<UncommonType> Uncommon(this ref Type t) {
    if ((TFlag)(t.TFlag & TFlagUncommon) == 0) {
        return default!;
    }
    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Struct) {
        return Ꮡ((ж<structTypeUncommon>)((uintptr)@unsafe.Pointer.FromRef(ref t)).u);
    }
    if (exprᴛ1 == Pointer) {
        return Ꮡ((ж<Uncommon_u>)((uintptr)@unsafe.Pointer.FromRef(ref t)).u);
    }
    if (exprᴛ1 == Func) {
        return Ꮡ((ж<Uncommon_uᴛ1>)((uintptr)@unsafe.Pointer.FromRef(ref t)).u);
    }
    if (exprᴛ1 == Slice) {
        return Ꮡ((ж<Uncommon_uᴛ2>)((uintptr)@unsafe.Pointer.FromRef(ref t)).u);
    }
    if (exprᴛ1 == Array) {
        return Ꮡ((ж<Uncommon_uᴛ3>)((uintptr)@unsafe.Pointer.FromRef(ref t)).u);
    }
    if (exprᴛ1 == Chan) {
        return Ꮡ((ж<Uncommon_uᴛ4>)((uintptr)@unsafe.Pointer.FromRef(ref t)).u);
    }
    if (exprᴛ1 == Map) {
        return Ꮡ((ж<Uncommon_uᴛ5>)((uintptr)@unsafe.Pointer.FromRef(ref t)).u);
    }
    if (exprᴛ1 == Interface) {
        return Ꮡ((ж<Uncommon_uᴛ6>)((uintptr)@unsafe.Pointer.FromRef(ref t)).u);
    }
    { /* default: */
        return Ꮡ((ж<Uncommon_uᴛ7>)((uintptr)@unsafe.Pointer.FromRef(ref t)).u);
    }

}

// Elem returns the element type for t if t is an array, channel, map, pointer, or slice, otherwise nil.
[GoRecv] internal static ж<Type> Elem(this ref Type t) {
    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Array) {
        var tt = (ж<ΔArrayType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }
    if (exprᴛ1 == Chan) {
        var tt = (ж<ChanType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }
    if (exprᴛ1 == Map) {
        var tt = (ж<ΔMapType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }
    if (exprᴛ1 == Pointer) {
        var tt = (ж<PtrType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }
    if (exprᴛ1 == Slice) {
        var tt = (ж<SliceType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
        return (~tt).Elem;
    }

    return default!;
}

// StructType returns t cast to a *StructType, or nil if its tag does not match.
[GoRecv] internal static ж<ΔStructType> StructType(this ref Type t) {
    if (t.Kind() != Struct) {
        return default!;
    }
    return (ж<ΔStructType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
}

// MapType returns t cast to a *MapType, or nil if its tag does not match.
[GoRecv] internal static ж<ΔMapType> MapType(this ref Type t) {
    if (t.Kind() != Map) {
        return default!;
    }
    return (ж<ΔMapType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
}

// ArrayType returns t cast to a *ArrayType, or nil if its tag does not match.
[GoRecv] internal static ж<ΔArrayType> ArrayType(this ref Type t) {
    if (t.Kind() != Array) {
        return default!;
    }
    return (ж<ΔArrayType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
}

// FuncType returns t cast to a *FuncType, or nil if its tag does not match.
[GoRecv] internal static ж<ΔFuncType> FuncType(this ref Type t) {
    if (t.Kind() != Func) {
        return default!;
    }
    return (ж<ΔFuncType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
}

// InterfaceType returns t cast to a *InterfaceType, or nil if its tag does not match.
[GoRecv] internal static ж<ΔInterfaceType> InterfaceType(this ref Type t) {
    if (t.Kind() != Interface) {
        return default!;
    }
    return (ж<ΔInterfaceType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
}

// Size returns the size of data with type t.
[GoRecv] internal static uintptr Size(this ref Type t) {
    return t.Size_;
}

// Align returns the alignment of data with type t.
[GoRecv] internal static nint Align(this ref Type t) {
    return ((nint)t.Align_);
}

[GoRecv] internal static nint FieldAlign(this ref Type t) {
    return ((nint)t.FieldAlign_);
}

[GoType] partial struct ΔInterfaceType {
    public partial ref Type Type { get; }
    public ΔName PkgPath;    // import path
    public slice<Imethod> Methods; // sorted by hash
}

[GoRecv] internal static slice<Method> ExportedMethods(this ref Type t) {
    var ut = t.Uncommon();
    if (ut == nil) {
        return default!;
    }
    return ut.ExportedMethods();
}

[GoRecv] internal static nint NumMethod(this ref Type t) {
    if (t.Kind() == Interface) {
        var tt = (ж<ΔInterfaceType>)((uintptr)@unsafe.Pointer.FromRef(ref t));
        return tt.NumMethod();
    }
    return len(t.ExportedMethods());
}

// NumMethod returns the number of interface methods in the type's method set.
[GoRecv] internal static nint NumMethod(this ref ΔInterfaceType t) {
    return len(t.Methods);
}

[GoType] partial struct ΔMapType {
    public partial ref Type Type { get; }
    public ж<Type> Key;
    public ж<Type> Elem;
    public ж<Type> Bucket; // internal type representing a hash bucket
    // function for hashing keys (ptr to key, seed) -> hash
    public Func<@unsafe.Pointer, uintptr, uintptr> Hasher;
    public uint8 KeySize;  // size of key slot
    public uint8 ValueSize;  // size of elem slot
    public uint16 BucketSize; // size of bucket
    public uint32 Flags;
}

// Note: flag values must match those used in the TMAP case
// in ../cmd/compile/internal/reflectdata/reflect.go:writeType.
[GoRecv] internal static bool IndirectKey(this ref ΔMapType mt) {
    // store ptr to key instead of key itself
    return (uint32)(mt.Flags & 1) != 0;
}

[GoRecv] internal static bool IndirectElem(this ref ΔMapType mt) {
    // store ptr to elem instead of elem itself
    return (uint32)(mt.Flags & 2) != 0;
}

[GoRecv] internal static bool ReflexiveKey(this ref ΔMapType mt) {
    // true if k==k for all keys
    return (uint32)(mt.Flags & 4) != 0;
}

[GoRecv] internal static bool NeedKeyUpdate(this ref ΔMapType mt) {
    // true if we need to update key on an overwrite
    return (uint32)(mt.Flags & 8) != 0;
}

[GoRecv] internal static bool HashMightPanic(this ref ΔMapType mt) {
    // true if hash function might panic
    return (uint32)(mt.Flags & 16) != 0;
}

[GoRecv] internal static ж<Type> Key(this ref Type t) {
    if (t.Kind() == Map) {
        return (ж<ΔMapType>)((uintptr)@unsafe.Pointer.FromRef(ref t)).Key;
    }
    return default!;
}

[GoType] partial struct SliceType {
    public partial ref Type Type { get; }
    public ж<Type> Elem; // slice element type
}

// funcType represents a function type.
//
// A *Type for each in and out parameter is stored in an array that
// directly follows the funcType (and possibly its uncommonType). So
// a function type with one method, one input, and one output is:
//
//	struct {
//		funcType
//		uncommonType
//		[2]*rtype    // [0] is in, [1] is out
//	}
[GoType] partial struct ΔFuncType {
    public partial ref Type Type { get; }
    public uint16 InCount;
    public uint16 OutCount; // top bit is set if last input parameter is ...
}

[GoRecv] internal static ж<Type> In(this ref ΔFuncType t, nint i) {
    return t.InSlice()[i];
}

[GoRecv] internal static nint NumIn(this ref ΔFuncType t) {
    return ((nint)t.InCount);
}

[GoRecv] internal static nint NumOut(this ref ΔFuncType t) {
    return ((nint)((uint16)(t.OutCount & (1 << (int)(15) - 1))));
}

[GoRecv] internal static ж<Type> Out(this ref ΔFuncType t, nint i) {
    return (t.OutSlice()[i]);
}

[GoRecv] internal static slice<ж<Type>> InSlice(this ref ΔFuncType t) {
    var uadd = @unsafe.Sizeof(t.val);
    if ((TFlag)(t.TFlag & TFlagUncommon) != 0) {
        uadd += @unsafe.Sizeof(new UncommonType(nil));
    }
    if (t.InCount == 0) {
        return default!;
    }
    return (ж<Type>)((uintptr)addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), uadd, "t.inCount > 0"u8)).slice(-1, t.InCount, t.InCount);
}

[GoRecv] internal static slice<ж<Type>> OutSlice(this ref ΔFuncType t) {
    var outCount = ((uint16)t.NumOut());
    if (outCount == 0) {
        return default!;
    }
    var uadd = @unsafe.Sizeof(t.val);
    if ((TFlag)(t.TFlag & TFlagUncommon) != 0) {
        uadd += @unsafe.Sizeof(new UncommonType(nil));
    }
    return (ж<Type>)((uintptr)addChecked((uintptr)@unsafe.Pointer.FromRef(ref t), uadd, "outCount > 0"u8)).slice(t.InCount, t.InCount + outCount, t.InCount + outCount);
}

[GoRecv] internal static bool IsVariadic(this ref ΔFuncType t) {
    return (uint16)(t.OutCount & (1 << (int)(15))) != 0;
}

[GoType] partial struct PtrType {
    public partial ref Type Type { get; }
    public ж<Type> Elem; // pointer element (pointed at) type
}

[GoType] partial struct StructField {
    public ΔName ΔName;  // name is always non-empty
    public ж<Type> Typ; // type of field
    public uintptr Offset; // byte offset of field
}

[GoRecv] internal static bool Embedded(this ref StructField f) {
    return f.Name.IsEmbedded();
}

[GoType] partial struct ΔStructType {
    public partial ref Type Type { get; }
    public ΔName PkgPath;
    public slice<StructField> Fields;
}

// Name is an encoded type Name with optional extra data.
//
// The first byte is a bit field containing:
//
//	1<<0 the name is exported
//	1<<1 tag data follows the name
//	1<<2 pkgPath nameOff follows the name and tag
//	1<<3 the name is of an embedded (a.k.a. anonymous) field
//
// Following that, there is a varint-encoded length of the name,
// followed by the name itself.
//
// If tag data is present, it also has a varint-encoded length
// followed by the tag itself.
//
// If the import path follows, then 4 bytes at the end of
// the data form a nameOff. The import path is only set for concrete
// methods that are defined in a different package than their type.
//
// If a name starts with "*", then the exported bit represents
// whether the pointed to type is exported.
//
// Note: this encoding must match here and in:
//   cmd/compile/internal/reflectdata/reflect.go
//   cmd/link/internal/ld/decodesym.go
[GoType] partial struct ΔName {
    public ж<byte> Bytes;
}

// DataChecked does pointer arithmetic on n's Bytes, and that arithmetic is asserted to
// be safe for the reason in whySafe (which can appear in a backtrace, etc.)
public static ж<byte> DataChecked(this ΔName n, nint off, @string whySafe) {
    return (ж<byte>)((uintptr)addChecked(new @unsafe.Pointer(n.Bytes), ((uintptr)off), whySafe));
}

// Data does pointer arithmetic on n's Bytes, and that arithmetic is asserted to
// be safe because the runtime made the call (other packages use DataChecked)
public static ж<byte> Data(this ΔName n, nint off) {
    return (ж<byte>)((uintptr)addChecked(new @unsafe.Pointer(n.Bytes), ((uintptr)off), "the runtime doesn't need to give you a reason"u8));
}

// IsExported returns "is n exported?"
public static bool IsExported(this ΔName n) {
    return (byte)((n.Bytes.val) & (1 << (int)(0))) != 0;
}

// HasTag returns true iff there is tag data following this name
public static bool HasTag(this ΔName n) {
    return (byte)((n.Bytes.val) & (1 << (int)(1))) != 0;
}

// IsEmbedded returns true iff n is embedded (an anonymous field).
public static bool IsEmbedded(this ΔName n) {
    return (byte)((n.Bytes.val) & (1 << (int)(3))) != 0;
}

// ReadVarint parses a varint as encoded by encoding/binary.
// It returns the number of encoded bytes and the encoded value.
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

// IsBlank indicates whether n is "_".
public static bool IsBlank(this ΔName n) {
    if (n.Bytes == nil) {
        return false;
    }
    var (_, l) = n.ReadVarint(1);
    return l == 1 && n.Data(2).val == (rune)'_';
}

// writeVarint writes n to buf in varint form. Returns the
// number of bytes written. n must be nonnegative.
// Writes at most 10 bytes.
internal static nint writeVarint(slice<byte> buf, nint n) {
    for (nint i = 0; ᐧ ; i++) {
        var b = ((@byte)((nint)(n & 127)));
        n >>= 7;
        if (n == 0) {
            buf[i] = b;
            return i + 1;
        }
        buf[i] = (byte)(b | 128);
    }
}

// Name returns the tag string for n, or empty if there is none.
public static @string Name(this ΔName n) {
    if (n.Bytes == nil) {
        return ""u8;
    }
    var (i, l) = n.ReadVarint(1);
    return @unsafe.String(n.DataChecked(1 + i, "non-empty string"u8), l);
}

// Tag returns the tag string for n, or empty if there is none.
public static @string Tag(this ΔName n) {
    if (!n.HasTag()) {
        return ""u8;
    }
    (i, l) = n.ReadVarint(1);
    var (i2, l2) = n.ReadVarint(1 + i + l);
    return @unsafe.String(n.DataChecked(1 + i + l + i2, "non-empty string"u8), l2);
}

public static ΔName NewName(@string n, @string tag, bool exported, bool embedded) {
    if (len(n) >= 1 << (int)(29)) {
        panic("abi.NewName: name too long: "u8 + n[..1024] + "..."u8);
    }
    if (len(tag) >= 1 << (int)(29)) {
        panic("abi.NewName: tag too long: "u8 + tag[..1024] + "..."u8);
    }
    array<byte> nameLen = default!;
    array<byte> tagLen = default!;
    nint nameLenLen = writeVarint(nameLen[..], len(n));
    nint tagLenLen = writeVarint(tagLen[..], len(tag));
    byte bits = default!;
    nint l = 1 + nameLenLen + len(n);
    if (exported) {
        bits |= 1 << (int)(0);
    }
    if (len(tag) > 0) {
        l += tagLenLen + len(tag);
        bits |= 1 << (int)(1);
    }
    if (embedded) {
        bits |= 1 << (int)(3);
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

public const nint TraceArgsLimit = 10; // print no more than 10 args/components
public const nint TraceArgsMaxDepth = 5; // no more than 5 layers of nesting
public const nint TraceArgsMaxLen = /* (TraceArgsMaxDepth*3+2)*TraceArgsLimit + 1 */ 171;

// Populate the data.
// The data is a stream of bytes, which contains the offsets and sizes of the
// non-aggregate arguments or non-aggregate fields/elements of aggregate-typed
// arguments, along with special "operators". Specifically,
//   - for each non-aggregate arg/field/element, its offset from FP (1 byte) and
//     size (1 byte)
//   - special operators:
//   - 0xff - end of sequence
//   - 0xfe - print { (at the start of an aggregate-typed argument)
//   - 0xfd - print } (at the end of an aggregate-typed argument)
//   - 0xfc - print ... (more args/fields/elements)
//   - 0xfb - print _ (offset too large)
public const nint TraceArgsEndSeq = /* 0xff */ 255;

public const nint TraceArgsStartAgg = /* 0xfe */ 254;

public const nint TraceArgsEndAgg = /* 0xfd */ 253;

public const nint TraceArgsDotdotdot = /* 0xfc */ 252;

public const nint TraceArgsOffsetTooLarge = /* 0xfb */ 251;

public const nint TraceArgsSpecial = /* 0xf0 */ 240; // above this are operators, below this are ordinary offsets

// MaxPtrmaskBytes is the maximum length of a GC ptrmask bitmap,
// which holds 1-bit entries describing where pointers are in a given type.
// Above this length, the GC information is recorded as a GC program,
// which can express repetition compactly. In either form, the
// information is used by the runtime to initialize the heap bitmap,
// and for large types (like 128 or more words), they are roughly the
// same speed. GC programs are never much larger and often more
// compact. (If large arrays are involved, they can be arbitrarily
// more compact.)
//
// The cutoff must be large enough that any allocation large enough to
// use a GC program is large enough that it does not share heap bitmap
// bytes with any other objects, allowing the GC program execution to
// assume an aligned start and not use atomic operations. In the current
// runtime, this means all malloc size classes larger than the cutoff must
// be multiples of four words. On 32-bit systems that's 16 bytes, and
// all size classes >= 16 bytes are 16-byte aligned, so no real constraint.
// On 64-bit systems, that's 32 bytes, and 32-byte alignment is guaranteed
// for size classes >= 256 bytes. On a 64-bit system, 256 bytes allocated
// is 32 pointers, the bits for which fit in 4 bytes. So MaxPtrmaskBytes
// must be >= 4.
//
// We used to use 16 because the GC programs do have some constant overhead
// to get started, and processing 128 pointers seems to be enough to
// amortize that overhead well.
//
// To make sure that the runtime's chansend can call typeBitsBulkBarrier,
// we raised the limit to 2048, so that even 32-bit systems are guaranteed to
// use bitmaps for objects up to 64 kB in size.
public const nint MaxPtrmaskBytes = 2048;

} // end abi_package
