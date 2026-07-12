// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package reflect implements run-time reflection, allowing a program to
// manipulate objects with arbitrary types. The typical use is to take a value
// with static type interface{} and extract its dynamic type information by
// calling TypeOf, which returns a Type.
//
// A call to ValueOf returns a Value representing the run-time data.
// Zero takes a Type and returns a Value representing a zero value
// for that type.
//
// See "The Laws of Reflection" for an introduction to reflection in Go:
// https://golang.org/doc/articles/laws_of_reflection.html
global using uncommonType = go.@internal.abi_package.UncommonType;
global using aNameOff = go.@internal.abi_package.NameOff;
global using aTypeOff = go.@internal.abi_package.TypeOff;
global using aTextOff = go.@internal.abi_package.TextOff;
global using arrayType = go.@internal.abi_package.ΔArrayType;
global using chanType = go.@internal.abi_package.ChanType;
global using funcType = go.@internal.abi_package.ΔFuncType;
global using structField = go.@internal.abi_package.StructField;

namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using strconv = strconv_package;
using Δsync = sync_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = unsafe_package;
using @internal;
using go.unicode;
using ꓸꓸꓸbyte = Span<byte>;

partial class reflect_package {

// Type is the representation of a Go type.
//
// Not all methods apply to all kinds of types. Restrictions,
// if any, are noted in the documentation for each method.
// Use the Kind method to find out the kind of type before
// calling kind-specific methods. Calling a method
// inappropriate to the kind of type causes a run-time panic.
//
// Type values are comparable, such as with the == operator,
// so they can be used as map keys.
// Two Type values are equal if they represent identical types.
[GoType] partial interface ΔType {
// Methods applicable to all types.

    // Align returns the alignment in bytes of a value of
    // this type when allocated in memory.
    nint Align();
    // FieldAlign returns the alignment in bytes of a value of
    // this type when used as a field in a struct.
    nint FieldAlign();
    // Method returns the i'th method in the type's method set.
    // It panics if i is not in the range [0, NumMethod()).
    //
    // For a non-interface type T or *T, the returned Method's Type and Func
    // fields describe a function whose first argument is the receiver,
    // and only exported methods are accessible.
    //
    // For an interface type, the returned Method's Type field gives the
    // method signature, without a receiver, and the Func field is nil.
    //
    // Methods are sorted in lexicographic order.
    ΔMethod Method(nint _);
    // MethodByName returns the method with that name in the type's
    // method set and a boolean indicating if the method was found.
    //
    // For a non-interface type T or *T, the returned Method's Type and Func
    // fields describe a function whose first argument is the receiver.
    //
    // For an interface type, the returned Method's Type field gives the
    // method signature, without a receiver, and the Func field is nil.
    (ΔMethod, bool) MethodByName(@string _);
    // NumMethod returns the number of methods accessible using Method.
    //
    // For a non-interface type, it returns the number of exported methods.
    //
    // For an interface type, it returns the number of exported and unexported methods.
    nint NumMethod();
    // Name returns the type's name within its package for a defined type.
    // For other (non-defined) types it returns the empty string.
    @string Name();
    // PkgPath returns a defined type's package path, that is, the import path
    // that uniquely identifies the package, such as "encoding/base64".
    // If the type was predeclared (string, error) or not defined (*T, struct{},
    // []int, or A where A is an alias for a non-defined type), the package path
    // will be the empty string.
    @string PkgPath();
    // Size returns the number of bytes needed to store
    // a value of the given type; it is analogous to unsafe.Sizeof.
    uintptr Size();
    // String returns a string representation of the type.
    // The string representation may use shortened package names
    // (e.g., base64 instead of "encoding/base64") and is not
    // guaranteed to be unique among types. To test for type identity,
    // compare the Types directly.
    @string String();
    // Kind returns the specific kind of this type.
    ΔKind Kind();
    // Implements reports whether the type implements the interface type u.
    bool Implements(ΔType u);
    // AssignableTo reports whether a value of the type is assignable to type u.
    bool AssignableTo(ΔType u);
    // ConvertibleTo reports whether a value of the type is convertible to type u.
    // Even if ConvertibleTo returns true, the conversion may still panic.
    // For example, a slice of type []T is convertible to *[N]T,
    // but the conversion will panic if its length is less than N.
    bool ConvertibleTo(ΔType u);
    // Comparable reports whether values of this type are comparable.
    // Even if Comparable returns true, the comparison may still panic.
    // For example, values of interface type are comparable,
    // but the comparison will panic if their dynamic type is not comparable.
    bool Comparable();
// Methods applicable only to some types, depending on Kind.
// The methods allowed for each kind are:
//
//	Int*, Uint*, Float*, Complex*: Bits
//	Array: Elem, Len
//	Chan: ChanDir, Elem
//	Func: In, NumIn, Out, NumOut, IsVariadic.
//	Map: Key, Elem
//	Pointer: Elem
//	Slice: Elem
//	Struct: Field, FieldByIndex, FieldByName, FieldByNameFunc, NumField

    // Bits returns the size of the type in bits.
    // It panics if the type's Kind is not one of the
    // sized or unsized Int, Uint, Float, or Complex kinds.
    nint Bits();
    // ChanDir returns a channel type's direction.
    // It panics if the type's Kind is not Chan.
    ΔChanDir ChanDir();
    // IsVariadic reports whether a function type's final input parameter
    // is a "..." parameter. If so, t.In(t.NumIn() - 1) returns the parameter's
    // implicit actual type []T.
    //
    // For concreteness, if t represents func(x int, y ... float64), then
    //
    //	t.NumIn() == 2
    //	t.In(0) is the reflect.Type for "int"
    //	t.In(1) is the reflect.Type for "[]float64"
    //	t.IsVariadic() == true
    //
    // IsVariadic panics if the type's Kind is not Func.
    bool IsVariadic();
    // Elem returns a type's element type.
    // It panics if the type's Kind is not Array, Chan, Map, Pointer, or Slice.
    ΔType Elem();
    // Field returns a struct type's i'th field.
    // It panics if the type's Kind is not Struct.
    // It panics if i is not in the range [0, NumField()).
    StructField Field(nint i);
    // FieldByIndex returns the nested field corresponding
    // to the index sequence. It is equivalent to calling Field
    // successively for each index i.
    // It panics if the type's Kind is not Struct.
    StructField FieldByIndex(slice<nint> index);
    // FieldByName returns the struct field with the given name
    // and a boolean indicating if the field was found.
    // If the returned field is promoted from an embedded struct,
    // then Offset in the returned StructField is the offset in
    // the embedded struct.
    (StructField, bool) FieldByName(@string name);
    // FieldByNameFunc returns the struct field with a name
    // that satisfies the match function and a boolean indicating if
    // the field was found.
    //
    // FieldByNameFunc considers the fields in the struct itself
    // and then the fields in any embedded structs, in breadth first order,
    // stopping at the shallowest nesting depth containing one or more
    // fields satisfying the match function. If multiple fields at that depth
    // satisfy the match function, they cancel each other
    // and FieldByNameFunc returns no match.
    // This behavior mirrors Go's handling of name lookup in
    // structs containing embedded fields.
    //
    // If the returned field is promoted from an embedded struct,
    // then Offset in the returned StructField is the offset in
    // the embedded struct.
    (StructField, bool) FieldByNameFunc(Func<@string, bool> match);
    // In returns the type of a function type's i'th input parameter.
    // It panics if the type's Kind is not Func.
    // It panics if i is not in the range [0, NumIn()).
    ΔType In(nint i);
    // Key returns a map type's key type.
    // It panics if the type's Kind is not Map.
    ΔType Key();
    // Len returns an array type's length.
    // It panics if the type's Kind is not Array.
    nint Len();
    // NumField returns a struct type's field count.
    // It panics if the type's Kind is not Struct.
    nint NumField();
    // NumIn returns a function type's input parameter count.
    // It panics if the type's Kind is not Func.
    nint NumIn();
    // NumOut returns a function type's output parameter count.
    // It panics if the type's Kind is not Func.
    nint NumOut();
    // Out returns the type of a function type's i'th output parameter.
    // It panics if the type's Kind is not Func.
    // It panics if i is not in the range [0, NumOut()).
    ΔType Out(nint i);
    // OverflowComplex reports whether the complex128 x cannot be represented by type t.
    // It panics if t's Kind is not Complex64 or Complex128.
    bool OverflowComplex(complex128 x);
    // OverflowFloat reports whether the float64 x cannot be represented by type t.
    // It panics if t's Kind is not Float32 or Float64.
    bool OverflowFloat(float64 x);
    // OverflowInt reports whether the int64 x cannot be represented by type t.
    // It panics if t's Kind is not Int, Int8, Int16, Int32, or Int64.
    bool OverflowInt(int64 x);
    // OverflowUint reports whether the uint64 x cannot be represented by type t.
    // It panics if t's Kind is not Uint, Uintptr, Uint8, Uint16, Uint32, or Uint64.
    bool OverflowUint(uint64 x);
    // CanSeq reports whether a [Value] with this type can be iterated over using [Value.Seq].
    bool CanSeq();
    // CanSeq2 reports whether a [Value] with this type can be iterated over using [Value.Seq2].
    bool CanSeq2();
    ж<abi.Type> common();
    ж<uncommonType> uncommon();
}

[GoType("num:nuint")] partial struct ΔKind;

// BUG(rsc): FieldByName and related functions consider struct field names to be equal
// if the names are equal, even if they are unexported names originating
// in different packages. The practical effect of this is that the result of
// t.FieldByName("x") is not well defined if the struct type t contains
// multiple fields named x (embedded from different packages).
// FieldByName may return one of the fields named x or may report that there are none.
// See https://golang.org/issue/4876 for more details.
/*
 * These data structures are known to the compiler (../cmd/compile/internal/reflectdata/reflect.go).
 * A few are known to ../runtime/type.go to convey to debuggers.
 * They are also known to ../runtime/type.go.
 */
public static readonly ΔKind Invalid = /* iota */ 0;
public static readonly ΔKind ΔBool = 1;
public static readonly ΔKind ΔInt = 2;
public static readonly ΔKind Int8 = 3;
public static readonly ΔKind Int16 = 4;
public static readonly ΔKind Int32 = 5;
public static readonly ΔKind Int64 = 6;
public static readonly ΔKind ΔUint = 7;
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
public static readonly ΔKind ΔInterface = 20;
public static readonly ΔKind Map = 21;
public static readonly ΔKind ΔPointer = 22;
public static readonly ΔKind ΔSlice = 23;
public static readonly ΔKind ΔString = 24;
public static readonly ΔKind Struct = 25;
public static readonly ΔKind ΔUnsafePointer = 26;

// Ptr is the old name for the [Pointer] kind.
public static readonly ΔKind Ptr = /* Pointer */ 22;

// Embed this type to get common/uncommon
[GoType] partial struct Δcommon {
    public partial ref @internal.abi_package.Type Type { get; }
}

// rtype is the common implementation of most values.
// It is embedded in other struct types.
[GoType] partial struct rtype {
    internal abi.Type t;
}

internal static ж<abi.Type> common(this ж<rtype> Ꮡt) {
    return Ꮡt.of(rtype.Ꮡt);
}

internal static ж<abi.UncommonType> uncommon(this ж<rtype> Ꮡt) {
    return Ꮡt.of(rtype.Ꮡt).Uncommon();
}

[GoType("num:nint")] partial struct ΔChanDir;

public static readonly ΔChanDir RecvDir = /* 1 << iota */ 1;                 // <-chan
public static readonly ΔChanDir SendDir = 2;                 // chan<-
public static readonly ΔChanDir BothDir = /* RecvDir | SendDir */ 3; // chan

// interfaceType represents an interface type.
[GoType] partial struct interfaceType {
    public partial ref @internal.abi_package.ΔInterfaceType InterfaceType { get; } // can embed directly because not a public type.
}

internal static abiꓸName nameOff(this ж<interfaceType> Ꮡt, aNameOff off) {
    return toRType(Ꮡt.of(interfaceType.ᏑType)).nameOff(off);
}

internal static abiꓸName nameOffFor(ж<abi.Type> Ꮡt, aNameOff off) {
    return toRType(Ꮡt).nameOff(off);
}

internal static ж<abi.Type> typeOffFor(ж<abi.Type> Ꮡt, aTypeOff off) {
    return toRType(Ꮡt).typeOff(off);
}

internal static ж<abi.Type> typeOff(this ж<interfaceType> Ꮡt, aTypeOff off) {
    return toRType(Ꮡt.of(interfaceType.ᏑType)).typeOff(off);
}

internal static ж<abi.Type> common(this ж<interfaceType> Ꮡt) {
    return Ꮡt.of(interfaceType.ᏑType);
}

internal static ж<abi.UncommonType> uncommon(this ж<interfaceType> Ꮡt) {
    return Ꮡt.of(interfaceType.ᏑInterfaceType).of(abiꓸInterfaceType.ᏑType).Uncommon();
}

// mapType represents a map type.
[GoType] partial struct mapType {
    public partial ref @internal.abi_package.ΔMapType MapType { get; }
}

// ptrType represents a pointer type.
[GoType] partial struct ptrType {
    public partial ref @internal.abi_package.PtrType PtrType { get; }
}

// sliceType represents a slice type.
[GoType] partial struct sliceType {
    public partial ref @internal.abi_package.SliceType SliceType { get; }
}

// structType represents a struct type.
[GoType] partial struct structType {
    public partial ref @internal.abi_package.ΔStructType StructType { get; }
}

internal static @string pkgPath(abiꓸName n) {
    if (n.Bytes == nil || (byte)(n.DataChecked(0, "name flag field"u8).Value & ((byte)(1 << (int)(2)))) == 0) {
        return ""u8;
    }
    var (i, l) = n.ReadVarint(1);
    nint off = 1 + i + l;
    if (n.HasTag()) {
        var (i2, l2) = n.ReadVarint(off);
        off += i2 + l2;
    }
    ref var nameOff = ref heap(new int32(), out var ᏑnameOff);
    // Note that this field may not be aligned in memory,
    // so we cannot use a direct int32 assignment here.
    copy((~(ж<array<byte>>)(uintptr)(new @unsafe.Pointer(ᏑnameOff)))[..], (~(ж<array<byte>>)(uintptr)(new @unsafe.Pointer(n.DataChecked(off, "name offset field"u8))))[..]);
    var pkgPathName = new abiꓸName(Bytes: (ж<byte>)(uintptr)(resolveTypeOff(new @unsafe.Pointer(n.Bytes), nameOff)));
    return pkgPathName.Name();
}

internal static abiꓸName newName(@string n, @string tag, bool exported, bool embedded) {
    return abi.NewName(n, tag, exported, embedded);
}

/*
 * The compiler knows the exact layout of all the data structures above.
 * The compiler does not know about the data structures and methods below.
 */

// Method represents a single method.
[GoType] partial struct ΔMethod {
    // Name is the method name.
    public @string Name;
    // PkgPath is the package path that qualifies a lower case (unexported)
    // method name. It is empty for upper case (exported) method names.
    // The combination of PkgPath and Name uniquely identifies a method
    // in a method set.
    // See https://golang.org/ref/spec#Uniqueness_of_identifiers
    public @string PkgPath;
    public ΔType Type; // method type
    public ΔValue Func; // func with receiver as first argument
    public nint Index;  // index for Type.Method
}

// IsExported reports whether the method is exported.
public static bool IsExported(this ΔMethod m) {
    return m.PkgPath == ""u8;
}

// String returns the name of k.
public static @string String(this ΔKind k) {
    if ((nuint)k < (nuint)len(kindNames)) {
        return kindNames[(nint)((nuint)k)];
    }
    return "kind"u8 + strconv.Itoa((nint)(nuint)k);
}

internal static slice<@string> kindNames = new golib.SparseArray<@string>{
    [(int)((nuint)Invalid)] = "invalid"u8,
    [(int)((nuint)ΔBool)] = "bool"u8,
    [(int)((nuint)ΔInt)] = "int"u8,
    [(int)((nuint)Int8)] = "int8"u8,
    [(int)((nuint)Int16)] = "int16"u8,
    [(int)((nuint)Int32)] = "int32"u8,
    [(int)((nuint)Int64)] = "int64"u8,
    [(int)((nuint)ΔUint)] = "uint"u8,
    [(int)((nuint)Uint8)] = "uint8"u8,
    [(int)((nuint)Uint16)] = "uint16"u8,
    [(int)((nuint)Uint32)] = "uint32"u8,
    [(int)((nuint)Uint64)] = "uint64"u8,
    [(int)((nuint)Uintptr)] = "uintptr"u8,
    [(int)((nuint)Float32)] = "float32"u8,
    [(int)((nuint)Float64)] = "float64"u8,
    [(int)((nuint)Complex64)] = "complex64"u8,
    [(int)((nuint)Complex128)] = "complex128"u8,
    [(int)((nuint)Array)] = "array"u8,
    [(int)((nuint)Chan)] = "chan"u8,
    [(int)((nuint)Func)] = "func"u8,
    [(int)((nuint)ΔInterface)] = "interface"u8,
    [(int)((nuint)Map)] = "map"u8,
    [(int)((nuint)ΔPointer)] = "ptr"u8,
    [(int)((nuint)ΔSlice)] = "slice"u8,
    [(int)((nuint)ΔString)] = "string"u8,
    [(int)((nuint)Struct)] = "struct"u8,
    [(int)((nuint)ΔUnsafePointer)] = "unsafe.Pointer"u8
}.slice();

// resolveNameOff resolves a name offset from a base pointer.
// The (*rtype).nameOff method is a convenience wrapper for this function.
// Implemented in the runtime package.
//
//go:noescape
internal static partial @unsafe.Pointer resolveNameOff(@unsafe.Pointer ptrInModule, int32 off);

// resolveTypeOff resolves an *rtype offset from a base type.
// The (*rtype).typeOff method is a convenience wrapper for this function.
// Implemented in the runtime package.
//
//go:noescape
internal static partial @unsafe.Pointer resolveTypeOff(@unsafe.Pointer rtype, int32 off);

// resolveTextOff resolves a function pointer offset from a base type.
// The (*rtype).textOff method is a convenience wrapper for this function.
// Implemented in the runtime package.
//
//go:noescape
internal static partial @unsafe.Pointer resolveTextOff(@unsafe.Pointer rtype, int32 off);

// addReflectOff adds a pointer to the reflection lookup map in the runtime.
// It returns a new ID that can be used as a typeOff or textOff, and will
// be resolved correctly. Implemented in the runtime package.
//
// addReflectOff should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/goplus/reflectx
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname addReflectOff
//go:noescape
internal static partial int32 addReflectOff(@unsafe.Pointer ptr);

// resolveReflectName adds a name to the reflection lookup map in the runtime.
// It returns a new nameOff that can be used to refer to the pointer.
internal static aNameOff resolveReflectName(abiꓸName n) {
    return ((aNameOff)addReflectOff(new @unsafe.Pointer(n.Bytes)));
}

// resolveReflectType adds a *rtype to the reflection lookup map in the runtime.
// It returns a new typeOff that can be used to refer to the pointer.
internal static aTypeOff resolveReflectType(ж<abi.Type> Ꮡt) {
    return ((aTypeOff)addReflectOff(new @unsafe.Pointer(Ꮡt)));
}

// resolveReflectText adds a function pointer to the reflection lookup map in
// the runtime. It returns a new textOff that can be used to refer to the
// pointer.
internal static aTextOff resolveReflectText(@unsafe.Pointer ptr) {
    return ((aTextOff)addReflectOff(ptr));
}

internal static abiꓸName nameOff(this ж<rtype> Ꮡt, aNameOff off) {
    ref var t = ref Ꮡt.Value;

    return new abiꓸName(Bytes: (ж<byte>)(uintptr)(resolveNameOff((uintptr)@unsafe.Pointer.FromRef(ref t), (int32)off)));
}

internal static ж<abi.Type> typeOff(this ж<rtype> Ꮡt, aTypeOff off) {
    ref var t = ref Ꮡt.Value;

    return (ж<abi.Type>)(uintptr)(resolveTypeOff((uintptr)@unsafe.Pointer.FromRef(ref t), (int32)off));
}

internal static @unsafe.Pointer textOff(this ж<rtype> Ꮡt, aTextOff off) {
    ref var t = ref Ꮡt.Value;

    return (uintptr)resolveTextOff((uintptr)@unsafe.Pointer.FromRef(ref t), (int32)off);
}

internal static @unsafe.Pointer textOffFor(ж<abi.Type> Ꮡt, aTextOff off) {
    return (uintptr)toRType(Ꮡt).textOff(off);
}

// func String is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

[GoRecv] internal static uintptr Size(this ref rtype t) {
    return t.t.Size();
}

internal static nint Bits(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (Ꮡt == nil) {
        throw panic("reflect: Bits of nil Type");
    }
    ΔKind k = t.Kind();
    if (k < ΔInt || k > Complex128) {
        throw panic("reflect: Bits of non-arithmetic Type " + Ꮡt.String());
    }
    return (nint)t.t.Size_ * 8;
}

[GoRecv] internal static nint Align(this ref rtype t) {
    return t.t.Align();
}

[GoRecv] internal static nint FieldAlign(this ref rtype t) {
    return t.t.FieldAlign();
}

[GoRecv] internal static ΔKind Kind(this ref rtype t) {
    return ((ΔKind)(nuint)(uint8)t.t.Kind());
}

internal static slice<abi.Method> exportedMethods(this ж<rtype> Ꮡt) {
    var ut = Ꮡt.uncommon();
    if (ut == nil) {
        return default!;
    }
    return ut.ExportedMethods();
}

internal static nint NumMethod(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() == ΔInterface) {
        var tt = (ж<interfaceType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return tt.NumMethod();
    }
    return len(Ꮡt.exportedMethods());
}

internal static ΔMethod /*m*/ Method(this ж<rtype> Ꮡt, nint i) {
    ΔMethod m = default!;

    ref var t = ref Ꮡt.Value;
    if (t.Kind() == ΔInterface) {
        var tt = (ж<interfaceType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return tt.Method(i);
    }
    var methods = Ꮡt.exportedMethods();
    if (i < 0 || i >= len(methods)) {
        throw panic("reflect: Method index out of range");
    }
    var p = methods[i];
    var pname = Ꮡt.nameOff(p.Name);
    m.Name = pname.Name();
    var fl = ((flag)(uintptr)(nuint)Func);
    var mtyp = Ꮡt.typeOff(p.Mtyp);
    var ft = (ж<funcType>)(uintptr)(new @unsafe.Pointer(mtyp));
    var @in = new slice<ΔType>(0, 1 + ft.NumIn());
    @in = builtin.append(@in, (ΔType)(new rtypeжΔType(Ꮡt)));
    foreach (var (_, arg) in ft.InSlice()) {
        @in = builtin.append(@in, (ΔType)(new rtypeжΔType(toRType(arg))));
    }
    var @out = new slice<ΔType>(0, ft.NumOut());
    foreach (var (_, ret) in ft.OutSlice()) {
        @out = builtin.append(@out, (ΔType)(new rtypeжΔType(toRType(ret))));
    }
    var mt = FuncOf(@in, @out, ft.IsVariadic());
    m.Type = mt;
    ref var tfn = ref heap<@unsafe.Pointer>(out var Ꮡtfn);
    tfn = (uintptr)Ꮡt.textOff(p.Tfn);
    @unsafe.Pointer fn = @unsafe.Pointer.FromRef(ref (Ꮡtfn).Value);
    m.Func = new ΔValue(Ꮡ((~mt._<ж<rtype>>()).t), fn.Value, fl);
    m.Index = i;
    return m;
}

internal static (ΔMethod m, bool ok) MethodByName(this ж<rtype> Ꮡt, @string name) {
    ΔMethod m = default!;
    bool ok = default!;

    ref var t = ref Ꮡt.Value;
    if (t.Kind() == ΔInterface) {
        var tt = (ж<interfaceType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
        return tt.MethodByName(name);
    }
    var ut = Ꮡt.uncommon();
    if (ut == nil) {
        return (new ΔMethod(nil), false);
    }
    var methods = ut.ExportedMethods();
    // We are looking for the first index i where the string becomes >= s.
    // This is a copy of sort.Search, with f(h) replaced by (t.nameOff(methods[h].name).name() >= name).
    nint i = 0;
    nint j = len(methods);
    while (i < j) {
        nint h = (nint)(((nuint)(i + j) >> (int)(1)));
        // avoid overflow when computing h
        // i ≤ h < j
        if (!(Ꮡt.nameOff(methods[h].Name).Name() >= name)){
            i = h + 1;
        } else {
            // preserves f(i-1) == false
            j = h;
        }
    }
    // preserves f(j) == true
    // i == j, f(i-1) == false, and f(j) (= f(i)) == true  =>  answer is i.
    if (i < len(methods) && name == Ꮡt.nameOff(methods[i].Name).Name()) {
        return (Ꮡt.Method(i), true);
    }
    return (new ΔMethod(nil), false);
}

internal static @string PkgPath(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if ((abi.TFlag)(t.t.TFlag & abi.TFlagNamed) == 0) {
        return ""u8;
    }
    var ut = Ꮡt.uncommon();
    if (ut == nil) {
        return ""u8;
    }
    return Ꮡt.nameOff((~ut).PkgPath).Name();
}

internal static @string pkgPathFor(ж<abi.Type> Ꮡt) {
    return toRType(Ꮡt).PkgPath();
}

// func Name is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

internal static @string nameFor(ж<abi.Type> Ꮡt) {
    return toRType(Ꮡt).Name();
}

internal static ΔChanDir ChanDir(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Chan) {
        throw panic("reflect: ChanDir of non-chan type " + Ꮡt.String());
    }
    var tt = (ж<abi.ChanType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
    return ((ΔChanDir)(nint)(~tt).Dir);
}

internal static ж<rtype> toRType(ж<abi.Type> Ꮡt) {
    return (ж<rtype>)(uintptr)(new @unsafe.Pointer(Ꮡt));
}

internal static ж<abi.Type> elem(ж<abi.Type> Ꮡt) {
    var et = Ꮡt.Elem();
    if (et != nil) {
        return et;
    }
    throw panic("reflect: Elem of invalid type " + stringFor(Ꮡt));
}

// func Elem is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

// func Field is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

internal static StructField FieldByIndex(this ж<rtype> Ꮡt, slice<nint> index) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Struct) {
        throw panic("reflect: FieldByIndex of non-struct type " + Ꮡt.String());
    }
    var tt = (ж<structType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
    return tt.FieldByIndex(index);
}

internal static (StructField, bool) FieldByName(this ж<rtype> Ꮡt, @string name) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Struct) {
        throw panic("reflect: FieldByName of non-struct type " + Ꮡt.String());
    }
    var tt = (ж<structType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
    return tt.FieldByName(name);
}

internal static (StructField, bool) FieldByNameFunc(this ж<rtype> Ꮡt, Func<@string, bool> match) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Struct) {
        throw panic("reflect: FieldByNameFunc of non-struct type " + Ꮡt.String());
    }
    var tt = (ж<structType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
    return tt.FieldByNameFunc(match);
}

internal static ΔType Key(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Map) {
        throw panic("reflect: Key of non-map type " + Ꮡt.String());
    }
    var tt = (ж<mapType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
    return toType((~tt).Key);
}

internal static nint Len(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Array) {
        throw panic("reflect: Len of non-array type " + Ꮡt.String());
    }
    var tt = (ж<arrayType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
    return (nint)(~tt).Len;
}

// func NumField is hand-converted with managed semantics — see the package's *_impl.cs ([module: GoManualConversion])

internal static ΔType In(this ж<rtype> Ꮡt, nint i) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Func) {
        throw panic("reflect: In of non-func type " + Ꮡt.String());
    }
    var tt = (ж<abiꓸFuncType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
    return toType(tt.InSlice()[i]);
}

internal static nint NumIn(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Func) {
        throw panic("reflect: NumIn of non-func type " + Ꮡt.String());
    }
    var tt = (ж<abiꓸFuncType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
    return tt.NumIn();
}

internal static nint NumOut(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Func) {
        throw panic("reflect: NumOut of non-func type " + Ꮡt.String());
    }
    var tt = (ж<abiꓸFuncType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
    return tt.NumOut();
}

internal static ΔType Out(this ж<rtype> Ꮡt, nint i) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Func) {
        throw panic("reflect: Out of non-func type " + Ꮡt.String());
    }
    var tt = (ж<abiꓸFuncType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
    return toType(tt.OutSlice()[i]);
}

internal static bool IsVariadic(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != Func) {
        throw panic("reflect: IsVariadic of non-func type " + Ꮡt.String());
    }
    var tt = (ж<abiꓸFuncType>)(uintptr)(@unsafe.Pointer.FromRef(ref t));
    return tt.IsVariadic();
}

internal static bool OverflowComplex(this ж<rtype> Ꮡt, complex128 x) {
    ref var t = ref Ꮡt.Value;

    ΔKind k = t.Kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Complex64) {
        return overflowFloat32(real(x)) || overflowFloat32(imag(x));
    }
    if (exprᴛ1 == Complex128) {
        return false;
    }

    throw panic("reflect: OverflowComplex of non-complex type " + Ꮡt.String());
}

internal static bool OverflowFloat(this ж<rtype> Ꮡt, float64 x) {
    ref var t = ref Ꮡt.Value;

    ΔKind k = t.Kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == Float32) {
        return overflowFloat32(x);
    }
    if (exprᴛ1 == Float64) {
        return false;
    }

    throw panic("reflect: OverflowFloat of non-float type " + Ꮡt.String());
}

internal static bool OverflowInt(this ж<rtype> Ꮡt, int64 x) {
    ref var t = ref Ꮡt.Value;

    ΔKind k = t.Kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64) {
        var bitSize = t.Size() * 8;
        var trunc = (((x << (int)((64 - bitSize)))) >> (int)((64 - bitSize)));
        return x != trunc;
    }

    throw panic("reflect: OverflowInt of non-int type " + Ꮡt.String());
}

internal static bool OverflowUint(this ж<rtype> Ꮡt, uint64 x) {
    ref var t = ref Ꮡt.Value;

    ΔKind k = t.Kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == ΔUint || exprᴛ1 == Uintptr || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64) {
        var bitSize = t.Size() * 8;
        var trunc = (((x << (int)((64 - bitSize)))) >> (int)((64 - bitSize)));
        return x != trunc;
    }

    throw panic("reflect: OverflowUint of non-uint type " + Ꮡt.String());
}

internal static bool CanSeq(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64 || exprᴛ1 == ΔInt || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == ΔUint || exprᴛ1 == Uintptr || exprᴛ1 == Array || exprᴛ1 == ΔSlice || exprᴛ1 == Chan || exprᴛ1 == ΔString || exprᴛ1 == Map) {
        return true;
    }
    if (exprᴛ1 == Func) {
        return canRangeFunc(Ꮡt.of(rtype.Ꮡt));
    }
    if (exprᴛ1 == ΔPointer) {
        return Ꮡt.Elem().Kind() == Array;
    }

    return false;
}

internal static bool canRangeFunc(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != abi.Func) {
        return false;
    }
    var f = Ꮡt.FuncType();
    if ((~f).InCount != 1 || (~f).OutCount != 0) {
        return false;
    }
    var y = f.In(0);
    if (y.Kind() != abi.Func) {
        return false;
    }
    var yield = y.FuncType();
    return (~yield).InCount == 1 && (~yield).OutCount == 1 && yield.Out(0).Kind() == abi.Bool;
}

internal static bool CanSeq2(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Array || exprᴛ1 == ΔSlice || exprᴛ1 == ΔString || exprᴛ1 == Map) {
        return true;
    }
    if (exprᴛ1 == Func) {
        return canRangeFunc2(Ꮡt.of(rtype.Ꮡt));
    }
    if (exprᴛ1 == ΔPointer) {
        return Ꮡt.Elem().Kind() == Array;
    }

    return false;
}

internal static bool canRangeFunc2(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    if (t.Kind() != abi.Func) {
        return false;
    }
    var f = Ꮡt.FuncType();
    if ((~f).InCount != 1 || (~f).OutCount != 0) {
        return false;
    }
    var y = f.In(0);
    if (y.Kind() != abi.Func) {
        return false;
    }
    var yield = y.FuncType();
    return (~yield).InCount == 2 && (~yield).OutCount == 1 && yield.Out(0).Kind() == abi.Bool;
}

// add returns p+x.
//
// The whySafe string is ignored, so that the function still inlines
// as efficiently as p+x, but all call sites should use the string to
// record why the addition is safe, which is to say why the addition
// does not cause x to advance to the very end of p's allocation
// and therefore point incorrectly at the next block in memory.
//
// add should be an internal detail (and is trivially copyable),
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/pinpoint-apm/pinpoint-go-agent
//   - github.com/vmware/govmomi
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname add
internal static @unsafe.Pointer add(@unsafe.Pointer p, uintptr x, @string whySafe) {
    return (@unsafe.Pointer)((uintptr)p + x);
}

public static @string String(this ΔChanDir d) {
    var exprᴛ1 = d;
    if (exprᴛ1 == SendDir) {
        return "chan<-"u8;
    }
    if (exprᴛ1 == RecvDir) {
        return "<-chan"u8;
    }
    if (exprᴛ1 == BothDir) {
        return "chan"u8;
    }

    return "ChanDir"u8 + strconv.Itoa((nint)d);
}

// Method returns the i'th method in the type's method set.
internal static ΔMethod /*m*/ Method(this ж<interfaceType> Ꮡt, nint i) {
    ΔMethod m = default!;

    ref var t = ref Ꮡt.Value;
    if (i < 0 || i >= len(t.Methods)) {
        return m;
    }
    var p = Ꮡ(t.Methods[i]);
    var pname = Ꮡt.nameOff((~p).Name);
    m.Name = pname.Name();
    if (!pname.IsExported()) {
        m.PkgPath = pkgPath(pname);
        if (m.PkgPath == ""u8) {
            m.PkgPath = t.PkgPath.Name();
        }
    }
    m.Type = toType(Ꮡt.typeOff((~p).Typ));
    m.Index = i;
    return m;
}

// NumMethod returns the number of interface methods in the type's method set.
[GoRecv] internal static nint NumMethod(this ref interfaceType t) {
    return len(t.Methods);
}

// MethodByName method with the given name in the type's method set.
internal static (ΔMethod m, bool ok) MethodByName(this ж<interfaceType> Ꮡt, @string name) {
    ΔMethod m = default!;
    bool ok = default!;

    ref var t = ref Ꮡt.Value;
    if (Ꮡt == nil) {
        return (m, ok);
    }
    ж<abi.Imethod> p = default!;
    foreach (var (i, _) in t.Methods) {
        p = Ꮡ(t.Methods[i]);
        if (Ꮡt.nameOff((~p).Name).Name() == name) {
            return (Ꮡt.Method(i), true);
        }
    }
    return (m, ok);
}

// A StructField describes a single field in a struct.
[GoType] partial struct StructField {
    // Name is the field name.
    public @string Name;
    // PkgPath is the package path that qualifies a lower case (unexported)
    // field name. It is empty for upper case (exported) field names.
    // See https://golang.org/ref/spec#Uniqueness_of_identifiers
    public @string PkgPath;
    public ΔType Type;    // field type
    public StructTag Tag; // field tag string
    public uintptr Offset;   // offset within struct, in bytes
    public slice<nint> Index; // index sequence for Type.FieldByIndex
    public bool Anonymous;      // is an embedded field
}

// IsExported reports whether the field is exported.
public static bool IsExported(this StructField f) {
    return f.PkgPath == ""u8;
}

[GoType("@string")] partial struct StructTag;

// Get returns the value associated with key in the tag string.
// If there is no such key in the tag, Get returns the empty string.
// If the tag does not have the conventional format, the value
// returned by Get is unspecified. To determine whether a tag is
// explicitly set to the empty string, use [StructTag.Lookup].
public static @string Get(this StructTag tag, @string key) {
    var (v, _) = tag.Lookup(key);
    return v;
}

// Lookup returns the value associated with key in the tag string.
// If the key is present in the tag the value (which may be empty)
// is returned. Otherwise the returned value will be the empty string.
// The ok return value reports whether the value was explicitly set in
// the tag string. If the tag does not have the conventional format,
// the value returned by Lookup is unspecified.
public static (@string value, bool ok) Lookup(this StructTag tag, @string key) {
    @string value = default!;
    bool ok = default!;

    // When modifying this code, also update the validateStructTag code
    // in cmd/vet/structtag.go.
    while (tag != ""u8) {
        // Skip leading space.
        nint i = 0;
        while (i < len(tag) && tag[i] == (rune)' ') {
            i++;
        }
        tag = tag[(int)(i)..];
        if (tag == ""u8) {
            break;
        }
        // Scan to colon. A space, a quote or a control character is a syntax error.
        // Strictly speaking, control chars include the range [0x7f, 0x9f], not just
        // [0x00, 0x1f], but in practice, we ignore the multi-byte control characters
        // as it is simpler to inspect the tag's bytes than the tag's runes.
        i = 0;
        while (i < len(tag) && tag[i] > (rune)' ' && tag[i] != (rune)':' && tag[i] != (rune)'"' && tag[i] != 0x7f) {
            i++;
        }
        if (i == 0 || i + 1 >= len(tag) || tag[i] != (rune)':' || tag[i + 1] != (rune)'"') {
            break;
        }
        @string name = ((@string)(tag[..(int)(i)]));
        tag = tag[(int)(i + 1)..];
        // Scan quoted string to find value.
        i = 1;
        while (i < len(tag) && tag[i] != (rune)'"') {
            if (tag[i] == (rune)'\\') {
                i++;
            }
            i++;
        }
        if (i >= len(tag)) {
            break;
        }
        @string qvalue = ((@string)(tag[..(int)(i + 1)]));
        tag = tag[(int)(i + 1)..];
        if (key == name) {
            var (valueΔ1, err) = strconv.Unquote(qvalue);
            if (err != default!) {
                break;
            }
            return (valueΔ1, true);
        }
    }
    return ("", false);
}

// Field returns the i'th struct field.
[GoRecv] internal static StructField /*f*/ Field(this ref structType t, nint i) {
    StructField f = default!;

    if (i < 0 || i >= len(t.Fields)) {
        throw panic("reflect: Field index out of bounds");
    }
    var p = Ꮡ(t.Fields[i]);
    f.Type = toType((~p).Typ);
    f.Name = (~p).Name.Name();
    f.Anonymous = p.Embedded();
    if (!(~p).Name.IsExported()) {
        f.PkgPath = t.PkgPath.Name();
    }
    {
        @string tag = (~p).Name.Tag(); if (tag != ""u8) {
            f.Tag = ((StructTag)tag);
        }
    }
    f.Offset = p.Value.Offset;
    // NOTE(rsc): This is the only allocation in the interface
    // presented by a reflect.Type. It would be nice to avoid,
    // at least in the common cases, but we need to make sure
    // that misbehaving clients of reflect cannot affect other
    // uses of reflect. One possibility is CL 5371098, but we
    // postponed that ugliness until there is a demonstrated
    // need for the performance. This is issue 2320.
    f.Index = new nint[]{i}.slice();
    return f;
}

// TODO(gri): Should there be an error/bool indicator if the index
// is wrong for FieldByIndex?

// FieldByIndex returns the nested field corresponding to index.
internal static StructField /*f*/ FieldByIndex(this ж<structType> Ꮡt, slice<nint> index) {
    StructField f = default!;

    f.Type = toType(Ꮡt.of(structType.ᏑType));
    foreach (var (i, x) in index) {
        if (i > 0) {
            var ft = f.Type;
            if (ft.Kind() == ΔPointer && ft.Elem().Kind() == Struct) {
                ft = ft.Elem();
            }
            f.Type = ft;
        }
        f = f.Type.Field(x);
    }
    return f;
}

// A fieldScan represents an item on the fieldByNameFunc scan work list.
[GoType] partial struct fieldScan {
    internal ж<structType> typ;
    internal slice<nint> index;
}

// FieldByNameFunc returns the struct field with a name that satisfies the
// match function and a boolean to indicate if the field was found.
internal static (StructField result, bool ok) FieldByNameFunc(this ж<structType> Ꮡt, Func<@string, bool> match) {
    StructField result = default!;
    bool ok = default!;

    ref var t = ref Ꮡt.Value;
    // This uses the same condition that the Go language does: there must be a unique instance
    // of the match at a given depth level. If there are multiple instances of a match at the
    // same depth, they annihilate each other and inhibit any possible match at a lower level.
    // The algorithm is breadth first search, one depth level at a time.
    // The current and next slices are work queues:
    // current lists the fields to visit on this depth level,
    // and next lists the fields on the next lower level.
    var current = new fieldScan[]{}.slice();
    var next = new fieldScan[]{new(typ: Ꮡt)}.slice();
    // nextCount records the number of times an embedded type has been
    // encountered and considered for queueing in the 'next' slice.
    // We only queue the first one, but we increment the count on each.
    // If a struct type T can be reached more than once at a given depth level,
    // then it annihilates itself and need not be considered at all when we
    // process that next depth level.
    map<ж<structType>, nint> nextCount = default!;
    // visited records the structs that have been considered already.
    // Embedded pointer fields can create cycles in the graph of
    // reachable embedded types; visited avoids following those cycles.
    // It also avoids duplicated effort: if we didn't find the field in an
    // embedded type T at level 2, we won't find it in one at level 4 either.
    var visited = new map<ж<structType>, bool>{};
    while (len(next) > 0) {
        (current, next) = (next, current[..0]);
        var count = nextCount;
        nextCount = default!;
        // Process all the fields at this depth, now listed in 'current'.
        // The loop queues embedded fields found in 'next', for processing during the next
        // iteration. The multiplicity of the 'current' field counts is recorded
        // in 'count'; the multiplicity of the 'next' field counts is recorded in 'nextCount'.
        foreach (var (_, scan) in current) {
            var tΔ1 = scan.typ;
            if (visited[tΔ1]) {
                // We've looked through this type before, at a higher level.
                // That higher level would shadow the lower level we're now at,
                // so this one can't be useful to us. Ignore it.
                continue;
            }
            visited[tΔ1] = true;
            foreach (var (i, _) in (~tΔ1).Fields) {
                var f = Ꮡ((~tΔ1).Fields, i);
                // Find name and (for embedded field) type for field f.
                @string fname = (~f).Name.Name();
                ж<abi.Type> ntyp = default!;
                if (f.Embedded()) {
                    // Embedded field of type T or *T.
                    ntyp = f.Value.Typ;
                    if (ntyp.Kind() == abi.Pointer) {
                        ntyp = ntyp.Elem();
                    }
                }
                // Does it match?
                if (match(fname)) {
                    // Potential match
                    if (count[tΔ1] > 1 || ok) {
                        // Name appeared multiple times at this level: annihilate.
                        return (new StructField(nil), false);
                    }
                    result = tΔ1.Field(i);
                    result.Index = default!;
                    result.Index = builtin.append(result.Index, scan.index.ꓸꓸꓸ);
                    result.Index = builtin.append(result.Index, i);
                    ok = true;
                    continue;
                }
                // Queue embedded struct fields for processing with next level,
                // but only if we haven't seen a match yet at this level and only
                // if the embedded types haven't already been queued.
                if (ok || ntyp == nil || ntyp.Kind() != abi.Struct) {
                    continue;
                }
                var styp = (ж<structType>)(uintptr)(new @unsafe.Pointer(ntyp));
                if (nextCount[styp] > 0) {
                    nextCount[styp] = 2;
                    // exact multiple doesn't matter
                    continue;
                }
                if (nextCount == default!) {
                    nextCount = new map<ж<structType>, nint>{};
                }
                nextCount[styp] = 1;
                if (count[tΔ1] > 1) {
                    nextCount[styp] = 2;
                }
                // exact multiple doesn't matter
                slice<nint> index = default!;
                index = builtin.append(index, scan.index.ꓸꓸꓸ);
                index = builtin.append(index, i);
                next = builtin.append(next, new fieldScan(styp, index));
            }
        }
        if (ok) {
            break;
        }
    }
    return (result, ok);
}

// FieldByName returns the struct field with the given name
// and a boolean to indicate if the field was found.
internal static (StructField f, bool present) FieldByName(this ж<structType> Ꮡt, @string name) {
    StructField f = default!;
    bool present = default!;

    ref var t = ref Ꮡt.Value;
    // Quick check for top-level name, or struct without embedded fields.
    var hasEmbeds = false;
    if (name != ""u8) {
        foreach (var (i, _) in t.Fields) {
            var tf = Ꮡ(t.Fields[i]);
            if ((~tf).Name.Name() == name) {
                return (t.Field(i), true);
            }
            if (tf.Embedded()) {
                hasEmbeds = true;
            }
        }
    }
    if (!hasEmbeds) {
        return (f, present);
    }
    return Ꮡt.FieldByNameFunc((@string s) => s == name);
}

// TypeOf returns the reflection [Type] that represents the dynamic type of i.
// If i is a nil interface value, TypeOf returns nil.
public static ΔType TypeOf(any i) {
    return toType(abi.TypeOf(i));
}

// rtypeOf directly extracts the *rtype of the provided value.
internal static ж<abi.Type> rtypeOf(any i) {
    return abi.TypeOf(i);
}

// ptrMap is the cache for PointerTo.
internal static ж<Δsync.Map> ᏑptrMap = new(default(Δsync.Map));
internal static ref Δsync.Map ptrMap => ref ᏑptrMap.Value; // map[*rtype]*ptrType

// PtrTo returns the pointer type with element t.
// For example, if t represents type Foo, PtrTo(t) represents *Foo.
//
// PtrTo is the old spelling of [PointerTo].
// The two functions behave identically.
//
// Deprecated: Superseded by [PointerTo].
public static ΔType PtrTo(ΔType t) {
    return PointerTo(t);
}

// PointerTo returns the pointer type with element t.
// For example, if t represents type Foo, PointerTo(t) represents *Foo.
public static ΔType PointerTo(ΔType t) {
    return new rtypeжΔType(toRType(t._<ж<rtype>>().ptrTo()));
}

internal static ж<abi.Type> ptrTo(this ж<rtype> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var at = Ꮡt.of(rtype.Ꮡt);
    if ((~at).PtrToThis != 0) {
        return Ꮡt.typeOff((~at).PtrToThis);
    }
    // Check the cache.
    {
        var (piΔ1, ok) = ᏑptrMap.Load(t); if (ok) {
            return Ꮡ((~piΔ1._<ж<ptrType>>()).Type);
        }
    }
    // Look in known types.
    @string s = "*"u8 + Ꮡt.String();
    foreach (var (_, tt) in typesByString(s)) {
        var p = (ж<ptrType>)(uintptr)(new @unsafe.Pointer(tt));
        if ((~p).Elem != Ꮡt.of(rtype.Ꮡt)) {
            continue;
        }
        var (piΔ2, _) = ᏑptrMap.LoadOrStore(t, p);
        return Ꮡ((~piΔ2._<ж<ptrType>>()).Type);
    }
    // Create a new ptrType starting with the description
    // of an *unsafe.Pointer.
    ref var iptr = ref heap<any>(out var Ꮡiptr);

    iptr = ((ж<@unsafe.Pointer>)default!);
    var prototype = ~(ж<ж<ptrType>>)(uintptr)(new @unsafe.Pointer(Ꮡiptr));
    ref var pp = ref heap<ptrType>(out var Ꮡpp);
    pp = prototype.Value;
    pp.Str = resolveReflectName(newName(s, ""u8, false, false));
    pp.PtrToThis = 0;
    // For the type structures linked into the binary, the
    // compiler provides a good hash of the string.
    // Create a good hash for the new string by using
    // the FNV-1 hash's mixing function to combine the
    // old hash and the new "*".
    pp.Hash = fnv1(t.t.Hash, (rune)'*');
    pp.Elem = at;
    var (pi, _) = ᏑptrMap.LoadOrStore(t, Ꮡpp);
    return Ꮡ((~pi._<ж<ptrType>>()).Type);
}

internal static ж<abi.Type> ptrTo(ж<abi.Type> Ꮡt) {
    return toRType(Ꮡt).ptrTo();
}

// fnv1 incorporates the list of bytes into the hash x using the FNV-1 hash function.
internal static uint32 fnv1(uint32 x, params ꓸꓸꓸbyte listʗp) {
    var list = listʗp.slice();

    foreach (var (_, b) in list) {
        x = (uint32)(x * 16777619 ^ (uint32)b);
    }
    return x;
}

internal static bool Implements(this ж<rtype> Ꮡt, ΔType u) {
    if (u == default!) {
        throw panic("reflect: nil type passed to Type.Implements");
    }
    if (u.Kind() != ΔInterface) {
        throw panic("reflect: non-interface type passed to Type.Implements");
    }
    return implements(u.common(), Ꮡt.common());
}

internal static bool AssignableTo(this ж<rtype> Ꮡt, ΔType u) {
    if (u == default!) {
        throw panic("reflect: nil type passed to Type.AssignableTo");
    }
    var uu = u.common();
    return directlyAssignable(uu, Ꮡt.common()) || implements(uu, Ꮡt.common());
}

internal static bool ConvertibleTo(this ж<rtype> Ꮡt, ΔType u) {
    if (u == default!) {
        throw panic("reflect: nil type passed to Type.ConvertibleTo");
    }
    return convertOp(u.common(), Ꮡt.common()) != default!;
}

[GoRecv] internal static bool Comparable(this ref rtype t) {
    return t.t.Equal != default!;
}

// implements reports whether the type V implements the interface type T.
internal static bool implements(ж<abi.Type> ᏑT, ж<abi.Type> ᏑV) {
    ref var T = ref ᏑT.Value;
    ref var V = ref ᏑV.Value;

    if (T.Kind() != abi.Interface) {
        return false;
    }
    var t = (ж<interfaceType>)(uintptr)(new @unsafe.Pointer(ᏑT));
    if (len((~t).Methods) == 0) {
        return true;
    }
    // The same algorithm applies in both cases, but the
    // method tables for an interface type and a concrete type
    // are different, so the code is duplicated.
    // In both cases the algorithm is a linear scan over the two
    // lists - T's methods and V's methods - simultaneously.
    // Since method tables are stored in a unique sorted order
    // (alphabetical, with no duplicate method names), the scan
    // through V's methods must hit a match for each of T's
    // methods along the way, or else V does not implement T.
    // This lets us run the scan in overall linear time instead of
    // the quadratic time  a naive search would require.
    // See also ../runtime/iface.go.
    if (V.Kind() == abi.Interface) {
        var vΔ1 = (ж<interfaceType>)(uintptr)(new @unsafe.Pointer(ᏑV));
        nint iΔ1 = 0;
        for (nint j = 0; j < len((~vΔ1).Methods); j++) {
            var tm = Ꮡ((~t).Methods, iΔ1);
            var tmName = t.nameOff((~tm).Name);
            var vm = Ꮡ((~vΔ1).Methods, j);
            var vmName = nameOffFor(ᏑV, (~vm).Name);
            if (vmName.Name() == tmName.Name() && typeOffFor(ᏑV, (~vm).Typ) == t.typeOff((~tm).Typ)) {
                if (!tmName.IsExported()) {
                    @string tmPkgPath = pkgPath(tmName);
                    if (tmPkgPath == ""u8) {
                        tmPkgPath = (~t).PkgPath.Name();
                    }
                    @string vmPkgPath = pkgPath(vmName);
                    if (vmPkgPath == ""u8) {
                        vmPkgPath = (~vΔ1).PkgPath.Name();
                    }
                    if (tmPkgPath != vmPkgPath) {
                        continue;
                    }
                }
                {
                    iΔ1++; if (iΔ1 >= len((~t).Methods)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    var v = ᏑV.Uncommon();
    if (v == nil) {
        return false;
    }
    nint i = 0;
    var vmethods = v.Methods();
    for (nint j = 0; j < (nint)(~v).Mcount; j++) {
        var tm = Ꮡ((~t).Methods, i);
        var tmName = t.nameOff((~tm).Name);
        var vm = vmethods[j];
        var vmName = nameOffFor(ᏑV, vm.Name);
        if (vmName.Name() == tmName.Name() && typeOffFor(ᏑV, vm.Mtyp) == t.typeOff((~tm).Typ)) {
            if (!tmName.IsExported()) {
                @string tmPkgPath = pkgPath(tmName);
                if (tmPkgPath == ""u8) {
                    tmPkgPath = (~t).PkgPath.Name();
                }
                @string vmPkgPath = pkgPath(vmName);
                if (vmPkgPath == ""u8) {
                    vmPkgPath = nameOffFor(ᏑV, (~v).PkgPath).Name();
                }
                if (tmPkgPath != vmPkgPath) {
                    continue;
                }
            }
            {
                i++; if (i >= len((~t).Methods)) {
                    return true;
                }
            }
        }
    }
    return false;
}

// specialChannelAssignability reports whether a value x of channel type V
// can be directly assigned (using memmove) to another channel type T.
// https://golang.org/doc/go_spec.html#Assignability
// T and V must be both of Chan kind.
internal static bool specialChannelAssignability(ж<abi.Type> ᏑT, ж<abi.Type> ᏑV) {
    ref var T = ref ᏑT.Value;
    ref var V = ref ᏑV.Value;

    // Special case:
    // x is a bidirectional channel value, T is a channel type,
    // x's type V and T have identical element types,
    // and at least one of V or T is not a defined type.
    return ᏑV.ChanDir() == abi.BothDir && (nameFor(ᏑT) == ""u8 || nameFor(ᏑV) == ""u8) && haveIdenticalType(ᏑT.Elem(), ᏑV.Elem(), true);
}

// directlyAssignable reports whether a value x of type V can be directly
// assigned (using memmove) to a value of type T.
// https://golang.org/doc/go_spec.html#Assignability
// Ignoring the interface rules (implemented elsewhere)
// and the ideal constant rules (no ideal constants at run time).
internal static bool directlyAssignable(ж<abi.Type> ᏑT, ж<abi.Type> ᏑV) {
    ref var T = ref ᏑT.DerefOrNil();
    ref var V = ref ᏑV.DerefOrNil();

    // x's type V is identical to T?
    if (ᏑT == ᏑV) {
        return true;
    }
    // Otherwise at least one of T and V must not be defined
    // and they must have the same kind.
    if (T.HasName() && V.HasName() || T.Kind() != V.Kind()) {
        return false;
    }
    if (T.Kind() == abi.Chan && specialChannelAssignability(ᏑT, ᏑV)) {
        return true;
    }
    // x's type T and V must have identical underlying types.
    return haveIdenticalUnderlyingType(ᏑT, ᏑV, true);
}

internal static bool haveIdenticalType(ж<abi.Type> ᏑT, ж<abi.Type> ᏑV, bool cmpTags) {
    ref var T = ref ᏑT.DerefOrNil();
    ref var V = ref ᏑV.DerefOrNil();

    if (cmpTags) {
        return ᏑT == ᏑV;
    }
    if (nameFor(ᏑT) != nameFor(ᏑV) || T.Kind() != V.Kind() || pkgPathFor(ᏑT) != pkgPathFor(ᏑV)) {
        return false;
    }
    return haveIdenticalUnderlyingType(ᏑT, ᏑV, false);
}

internal static bool haveIdenticalUnderlyingType(ж<abi.Type> ᏑT, ж<abi.Type> ᏑV, bool cmpTags) {
    ref var T = ref ᏑT.DerefOrNil();
    ref var V = ref ᏑV.DerefOrNil();

    if (ᏑT == ᏑV) {
        return true;
    }
    ΔKind kind = ((ΔKind)(nuint)(uint8)T.Kind());
    if (kind != ((ΔKind)(nuint)(uint8)V.Kind())) {
        return false;
    }
    // Non-composite types of equal kind have same underlying type
    // (the predefined instance of the type).
    if (ΔBool <= kind && kind <= Complex128 || kind == ΔString || kind == ΔUnsafePointer) {
        return true;
    }
    // Composite types.
    var exprᴛ1 = kind;
    if (exprᴛ1 == Array) {
        return ᏑT.Len() == ᏑV.Len() && haveIdenticalType(ᏑT.Elem(), ᏑV.Elem(), cmpTags);
    }
    if (exprᴛ1 == Chan) {
        return ᏑV.ChanDir() == ᏑT.ChanDir() && haveIdenticalType(ᏑT.Elem(), ᏑV.Elem(), cmpTags);
    }
    if (exprᴛ1 == Func) {
        var t = (ж<funcType>)(uintptr)(new @unsafe.Pointer(ᏑT));
        var v = (ж<funcType>)(uintptr)(new @unsafe.Pointer(ᏑV));
        if ((~t).OutCount != (~v).OutCount || (~t).InCount != (~v).InCount) {
            return false;
        }
        for (nint i = 0; i < t.NumIn(); i++) {
            if (!haveIdenticalType(t.In(i), v.In(i), cmpTags)) {
                return false;
            }
        }
        for (nint i = 0; i < t.NumOut(); i++) {
            if (!haveIdenticalType(t.Out(i), v.Out(i), cmpTags)) {
                return false;
            }
        }
        return true;
    }
    if (exprᴛ1 == ΔInterface) {
        var t = (ж<interfaceType>)(uintptr)(new @unsafe.Pointer(ᏑT));
        var v = (ж<interfaceType>)(uintptr)(new @unsafe.Pointer(ᏑV));
        if (len((~t).Methods) == 0 && len((~v).Methods) == 0) {
            return true;
        }
        return false;
    }
    if (exprᴛ1 == Map) {
        return haveIdenticalType(ᏑT.Key(), // Might have the same methods but still
 // need a run time conversion.
 ᏑV.Key(), cmpTags) && haveIdenticalType(ᏑT.Elem(), ᏑV.Elem(), cmpTags);
    }
    if (exprᴛ1 == ΔPointer || exprᴛ1 == ΔSlice) {
        return haveIdenticalType(ᏑT.Elem(), ᏑV.Elem(), cmpTags);
    }
    if (exprᴛ1 == Struct) {
        var t = (ж<structType>)(uintptr)(new @unsafe.Pointer(ᏑT));
        var v = (ж<structType>)(uintptr)(new @unsafe.Pointer(ᏑV));
        if (len((~t).Fields) != len((~v).Fields)) {
            return false;
        }
        if ((~t).PkgPath.Name() != (~v).PkgPath.Name()) {
            return false;
        }
        foreach (var (i, _) in (~t).Fields) {
            var tf = Ꮡ((~t).Fields, i);
            var vf = Ꮡ((~v).Fields, i);
            if ((~tf).Name.Name() != (~vf).Name.Name()) {
                return false;
            }
            if (!haveIdenticalType((~tf).Typ, (~vf).Typ, cmpTags)) {
                return false;
            }
            if (cmpTags && (~tf).Name.Tag() != (~vf).Name.Tag()) {
                return false;
            }
            if ((~tf).Offset != (~vf).Offset) {
                return false;
            }
            if (tf.Embedded() != vf.Embedded()) {
                return false;
            }
        }
        return true;
    }

    return false;
}

// typelinks is implemented in package runtime.
// It returns a slice of the sections in each module,
// and a slice of *rtype offsets in each module.
//
// The types in each module are sorted by string. That is, the first
// two linked types of the first module are:
//
//	d0 := sections[0]
//	t1 := (*rtype)(add(d0, offset[0][0]))
//	t2 := (*rtype)(add(d0, offset[0][1]))
//
// and
//
//	t1.String() < t2.String()
//
// Note that strings are not unique identifiers for types:
// there can be more than one with a given string.
// Only types we might want to look up are included:
// pointers, channels, maps, slices, and arrays.
internal static partial (slice<@unsafe.Pointer> sections, slice<slice<int32>> offset) typelinks();

// rtypeOff should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/goccy/go-json
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname rtypeOff
internal static ж<abi.Type> rtypeOff(@unsafe.Pointer section, int32 off) {
    return (ж<abi.Type>)(uintptr)(add(section, (uintptr)off, "sizeof(rtype) > 0"u8));
}

// typesByString returns the subslice of typelinks() whose elements have
// the given string representation.
// It may be empty (no known types with that string) or may have
// multiple elements (multiple types with that string).
//
// typesByString should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/aristanetworks/goarista
//   - fortio.org/log
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname typesByString
internal static slice<ж<abi.Type>> typesByString(@string s) {
    var (sections, offset) = typelinks();
    slice<ж<abi.Type>> ret = default!;
    foreach (var (offsI, offs) in offset) {
        @unsafe.Pointer section = sections[offsI];
        // We are looking for the first index i where the string becomes >= s.
        // This is a copy of sort.Search, with f(h) replaced by (*typ[h].String() >= s).
        nint i = 0;
        nint j = len(offs);
        while (i < j) {
            nint h = (nint)(((nuint)(i + j) >> (int)(1)));
            // avoid overflow when computing h
            // i ≤ h < j
            if (!(stringFor(rtypeOff(section, offs[h])) >= s)){
                i = h + 1;
            } else {
                // preserves f(i-1) == false
                j = h;
            }
        }
        // preserves f(j) == true
        // i == j, f(i-1) == false, and f(j) (= f(i)) == true  =>  answer is i.
        // Having found the first, linear scan forward to find the last.
        // We could do a second binary search, but the caller is going
        // to do a linear scan anyway.
        for (nint jΔ1 = i; jΔ1 < len(offs); jΔ1++) {
            var typ = rtypeOff(section, offs[jΔ1]);
            if (stringFor(typ) != s) {
                break;
            }
            ret = builtin.append(ret, typ);
        }
    }
    return ret;
}

// The lookupCache caches ArrayOf, ChanOf, MapOf and SliceOf lookups.
internal static ж<Δsync.Map> ᏑlookupCache = new(default(Δsync.Map));
internal static ref Δsync.Map lookupCache => ref ᏑlookupCache.Value; // map[cacheKey]*rtype

// A cacheKey is the key for use in the lookupCache.
// Four values describe any of the types we are looking for:
// type kind, one or two subtypes, and an extra integer.
[GoType] partial struct cacheKey {
    internal ΔKind kind;
    internal ж<abi.Type> t1;
    internal ж<abi.Type> t2;
    internal uintptr extra;
}

// The funcLookupCache caches FuncOf lookups.
// FuncOf does not share the common lookupCache since cacheKey is not
// sufficient to represent functions unambiguously.

[GoType("dyn")] partial struct funcLookupCacheᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; } // Guards stores (but not loads) on m.
    // m is a map[uint32][]*rtype keyed by the hash calculated in FuncOf.
    // Elements of m are append-only and thus safe for concurrent reading.
    internal Δsync.Map m;
}
internal static ж<funcLookupCacheᴛ1> ᏑfuncLookupCache = new(new funcLookupCacheᴛ1(nil));
internal static ref funcLookupCacheᴛ1 funcLookupCache => ref ᏑfuncLookupCache.Value;

// ChanOf returns the channel type with the given direction and element type.
// For example, if t represents int, ChanOf(RecvDir, t) represents <-chan int.
//
// The gc runtime imposes a limit of 64 kB on channel element types.
// If t's size is equal to or exceeds this limit, ChanOf panics.
public static ΔType ChanOf(ΔChanDir dir, ΔType t) {
    var typ = t.common();
    // Look in cache.
    var ckey = new cacheKey(Chan, typ, nil, (uintptr)(nint)dir);
    {
        var (chΔ1, ok) = ᏑlookupCache.Load(ckey); if (ok) {
            return new rtypeжΔType(chΔ1._<ж<rtype>>());
        }
    }
    // This restriction is imposed by the gc compiler and the runtime.
    if ((~typ).Size_ >= (uintptr)(1 << (int)(16))) {
        throw panic("reflect.ChanOf: element size too large");
    }
    // Look in known types.
    @string s = default!;
    var exprᴛ1 = dir;
    if (exprᴛ1 == SendDir) {
        s = "chan<- "u8 + stringFor(typ);
    }
    else if (exprᴛ1 == RecvDir) {
        s = "<-chan "u8 + stringFor(typ);
    }
    else if (exprᴛ1 == BothDir) {
        @string typeStr = stringFor(typ);
        if (typeStr[0] == (rune)'<'){
            // typ is recv chan, need parentheses as "<-" associates with leftmost
            // chan possible, see:
            // * https://golang.org/ref/spec#Channel_types
            // * https://github.com/golang/go/issues/39897
            s = "chan ("u8 + typeStr + ")"u8;
        } else {
            s = "chan "u8 + typeStr;
        }
    }
    else { /* default: */
        throw panic("reflect.ChanOf: invalid dir");
    }

    foreach (var (_, tt) in typesByString(s)) {
        var chΔ2 = (ж<chanType>)(uintptr)(new @unsafe.Pointer(tt));
        if ((~chΔ2).Elem == typ && (~chΔ2).Dir == ((abiꓸChanDir)(nint)dir)) {
            var (tiΔ1, _) = ᏑlookupCache.LoadOrStore(ckey, toRType(tt));
            return tiΔ1._<ΔType>();
        }
    }
    // Make a channel type.
    ref var ichan = ref heap<any>(out var Ꮡichan);

    ichan = (channel<@unsafe.Pointer>)(default!);
    var prototype = ~(ж<ж<chanType>>)(uintptr)(new @unsafe.Pointer(Ꮡichan));
    ref var ch = ref heap<chanType>(out var Ꮡch);
    ch = prototype.Value;
    ch.TFlag = abi.TFlagRegularMemory;
    ch.Dir = ((abiꓸChanDir)(nint)dir);
    ch.Str = resolveReflectName(newName(s, ""u8, false, false));
    ch.Hash = fnv1((~typ).Hash, (rune)'c', (byte)(nint)dir);
    ch.Elem = typ;
    var (ti, _) = ᏑlookupCache.LoadOrStore(ckey, toRType(Ꮡch.of(chanType.ᏑType)));
    return ti._<ΔType>();
}

// MapOf returns the map type with the given key and element types.
// For example, if k represents int and e represents string,
// MapOf(k, e) represents map[int]string.
//
// If the key type is not a valid map key type (that is, if it does
// not implement Go's == operator), MapOf panics.
public static ΔType MapOf(ΔType key, ΔType elem) {
    var ktyp = key.common();
    var etyp = elem.common();
    if ((~ktyp).Equal == default!) {
        throw panic("reflect.MapOf: invalid key type " + stringFor(ktyp));
    }
    // Look in cache.
    var ckey = new cacheKey(Map, ktyp, etyp, 0);
    {
        var (mtΔ1, ok) = ᏑlookupCache.Load(ckey); if (ok) {
            return mtΔ1._<ΔType>();
        }
    }
    // Look in known types.
    @string s = "map["u8 + stringFor(ktyp) + "]"u8 + stringFor(etyp);
    foreach (var (_, tt) in typesByString(s)) {
        var mtΔ2 = (ж<mapType>)(uintptr)(new @unsafe.Pointer(tt));
        if ((~mtΔ2).Key == ktyp && (~mtΔ2).Elem == etyp) {
            var (tiΔ1, _) = ᏑlookupCache.LoadOrStore(ckey, toRType(tt));
            return tiΔ1._<ΔType>();
        }
    }
    // Make a map type.
    // Note: flag values must match those used in the TMAP case
    // in ../cmd/compile/internal/reflectdata/reflect.go:writeType.
    ref var imap = ref heap<any>(out var Ꮡimap);

    imap = (map<@unsafe.Pointer, @unsafe.Pointer>)(default!);
    ref var mt = ref heap<mapType>(out var Ꮡmt);
    mt = (~(ж<ж<mapType>>)(uintptr)(new @unsafe.Pointer(Ꮡimap))).Value;
    mt.Str = resolveReflectName(newName(s, ""u8, false, false));
    mt.TFlag = 0;
    mt.Hash = fnv1((~etyp).Hash, (rune)'m', (byte)(((~ktyp).Hash >> (int)(24))), (byte)(((~ktyp).Hash >> (int)(16))), (byte)(((~ktyp).Hash >> (int)(8))), (byte)(~ktyp).Hash);
    mt.Key = ktyp;
    mt.Elem = etyp;
    mt.Bucket = bucketOf(ktyp, etyp);
    var ktypʗ1 = ktyp;
    mt.Hasher = (@unsafe.Pointer p, uintptr seed) => typehash(ktypʗ1, p, seed);
    mt.Flags = 0;
    if ((~ktyp).Size_ > abi.MapMaxKeyBytes){
        mt.KeySize = (uint8)goarch.PtrSize;
        mt.Flags |= 1;
    } else {
        // indirect key
        mt.KeySize = (uint8)(~ktyp).Size_;
    }
    if ((~etyp).Size_ > abi.MapMaxElemBytes){
        mt.ValueSize = (uint8)goarch.PtrSize;
        mt.Flags |= 2;
    } else {
        // indirect value
        mt.MapType.ValueSize = (uint8)(~etyp).Size_;
    }
    mt.MapType.BucketSize = (uint16)(~mt.Bucket).Size_;
    if (isReflexive(ktyp)) {
        mt.Flags |= 4;
    }
    if (needKeyUpdate(ktyp)) {
        mt.Flags |= 8;
    }
    if (hashMightPanic(ktyp)) {
        mt.Flags |= 16;
    }
    mt.PtrToThis = 0;
    var (ti, _) = ᏑlookupCache.LoadOrStore(ckey, toRType(Ꮡmt.of(mapType.ᏑType)));
    return ti._<ΔType>();
}

internal static slice<ΔType> funcTypes;

internal static ж<Δsync.Mutex> ᏑfuncTypesMutex = new(default(Δsync.Mutex));
internal static ref Δsync.Mutex funcTypesMutex => ref ᏑfuncTypesMutex.Value;

internal static ΔType initFuncTypes(nint n) => func((defer, recover) => {
    ᏑfuncTypesMutex.Lock();
    defer(ᏑfuncTypesMutex.Unlock);
    if (n >= len(funcTypes)) {
        var newFuncTypes = new slice<ΔType>(n + 1);
        copy(newFuncTypes, funcTypes);
        funcTypes = newFuncTypes;
    }
    if (funcTypes[n] != default!) {
        return funcTypes[n];
    }
    funcTypes[n] = StructOf(new StructField[]{
        new(
            Name: "FuncType"u8,
            Type: TypeOf(new funcType())
        ),
        new(
            Name: "Args"u8,
            Type: ArrayOf(n, TypeOf(Ꮡ(new rtype(nil))))
        )
    }.slice());
    return funcTypes[n];
});

// FuncOf returns the function type with the given argument and result types.
// For example if k represents int and e represents string,
// FuncOf([]Type{k}, []Type{e}, false) represents func(int) string.
//
// The variadic argument controls whether the function is variadic. FuncOf
// panics if the in[len(in)-1] does not represent a slice and variadic is
// true.
public static ΔType FuncOf(slice<ΔType> @in, slice<ΔType> @out, bool variadic) => func((defer, recover) => {
    if (variadic && (len(@in) == 0 || @in[len(@in) - 1].Kind() != ΔSlice)) {
        throw panic("reflect.FuncOf: last arg of variadic func must be slice");
    }
    // Make a func type.
    ref var ifunc = ref heap<any>(out var Ꮡifunc);

    ifunc = (Action)(default!);
    var prototype = ~(ж<ж<funcType>>)(uintptr)(new @unsafe.Pointer(Ꮡifunc));
    nint n = len(@in) + len(@out);
    if (n > 128) {
        throw panic("reflect.FuncOf: too many arguments");
    }
    var o = New(initFuncTypes(n)).Elem();
    var ft = (ж<funcType>)(uintptr)((@unsafe.Pointer)o.Field(0).Addr().Pointer());
    var args = @unsafe.Slice((ж<ж<rtype>>)(uintptr)((@unsafe.Pointer)o.Field(1).Addr().Pointer()), n).slice(0, 0, n);
    ft.Value = prototype.Value;
    // Build a hash and minimally populate ft.
    uint32 hash = default!;
    foreach (var (_, inΔ1) in @in) {
        var t = inΔ1._<ж<rtype>>();
        args = builtin.append(args, t);
        hash = fnv1(hash, (byte)(((~t).t.Hash >> (int)(24))), (byte)(((~t).t.Hash >> (int)(16))), (byte)(((~t).t.Hash >> (int)(8))), (byte)(~t).t.Hash);
    }
    if (variadic) {
        hash = fnv1(hash, (rune)'v');
    }
    hash = fnv1(hash, (rune)'.');
    foreach (var (_, outΔ1) in @out) {
        var t = outΔ1._<ж<rtype>>();
        args = builtin.append(args, t);
        hash = fnv1(hash, (byte)(((~t).t.Hash >> (int)(24))), (byte)(((~t).t.Hash >> (int)(16))), (byte)(((~t).t.Hash >> (int)(8))), (byte)(~t).t.Hash);
    }
    ft.Value.TFlag = 0;
    ft.Value.Hash = hash;
    ft.Value.InCount = (uint16)len(@in);
    ft.Value.OutCount = (uint16)len(@out);
    if (variadic) {
        ft.Value.OutCount |= (uint16)(1 << (int)(15));
    }
    // Look in cache.
    {
        var (ts, ok) = ᏑfuncLookupCache.of(funcLookupCacheᴛ1.Ꮡm).Load(hash); if (ok) {
            foreach (var (_, t) in ts._<slice<ж<abi.Type>>>()) {
                if (haveIdenticalUnderlyingType(ft.of(funcType.ᏑType), t, true)) {
                    return new rtypeжΔType(toRType(t));
                }
            }
        }
    }
    // Not in cache, lock and retry.
    ᏑfuncLookupCache.of(funcLookupCacheᴛ1.ᏑMutex).Lock();
    defer(ᏑfuncLookupCache.of(funcLookupCacheᴛ1.ᏑMutex).Unlock);
    {
        var (ts, ok) = ᏑfuncLookupCache.of(funcLookupCacheᴛ1.Ꮡm).Load(hash); if (ok) {
            foreach (var (_, t) in ts._<slice<ж<abi.Type>>>()) {
                if (haveIdenticalUnderlyingType(ft.of(funcType.ᏑType), t, true)) {
                    return new rtypeжΔType(toRType(t));
                }
            }
        }
    }
    var addToCache = (ж<abi.Type> tt) => {
        slice<ж<abi.Type>> rts = default!;
        {
            var (rti, ok) = ᏑfuncLookupCache.of(funcLookupCacheᴛ1.Ꮡm).Load(hash); if (ok) {
                rts = rti._<slice<ж<abi.Type>>>();
            }
        }
        ᏑfuncLookupCache.of(funcLookupCacheᴛ1.Ꮡm).Store(hash, builtin.append(rts, tt));
        return toType(tt);
    };
    // Look in known types for the same string representation.
    @string str = funcStr(ft);
    foreach (var (_, tt) in typesByString(str)) {
        if (haveIdenticalUnderlyingType(ft.of(funcType.ᏑType), tt, true)) {
            return addToCache(tt);
        }
    }
    // Populate the remaining fields of ft and store in cache.
    ft.Value.Str = resolveReflectName(newName(str, ""u8, false, false));
    ft.Value.PtrToThis = 0;
    return addToCache(ft.of(funcType.ᏑType));
});

internal static @string stringFor(ж<abi.Type> Ꮡt) {
    return toRType(Ꮡt).String();
}

// funcStr builds a string representation of a funcType.
internal static @string funcStr(ж<funcType> Ꮡft) {
    ref var ft = ref Ꮡft.Value;

    var repr = new slice<byte>(0, 64);
    repr = builtin.append(repr, ((@string)"func("u8).ꓸꓸꓸ);
    foreach (var (i, t) in Ꮡft.InSlice()) {
        if (i > 0) {
            repr = builtin.append(repr, ((@string)", "u8).ꓸꓸꓸ);
        }
        if (ft.IsVariadic() && i == (nint)ft.InCount - 1){
            repr = builtin.append(repr, ((@string)"..."u8).ꓸꓸꓸ);
            repr = builtin.append(repr, stringFor(((ж<sliceType>)(uintptr)(new @unsafe.Pointer(t))).Value.Elem).ꓸꓸꓸ);
        } else {
            repr = builtin.append(repr, stringFor(t).ꓸꓸꓸ);
        }
    }
    repr = builtin.append(repr, (byte)((rune)')'));
    var @out = Ꮡft.OutSlice();
    if (len(@out) == 1){
        repr = builtin.append(repr, (byte)((rune)' '));
    } else 
    if (len(@out) > 1) {
        repr = builtin.append(repr, ((@string)" ("u8).ꓸꓸꓸ);
    }
    foreach (var (i, t) in @out) {
        if (i > 0) {
            repr = builtin.append(repr, ((@string)", "u8).ꓸꓸꓸ);
        }
        repr = builtin.append(repr, stringFor(t).ꓸꓸꓸ);
    }
    if (len(@out) > 1) {
        repr = builtin.append(repr, (byte)((rune)')'));
    }
    return ((@string)repr);
}

// isReflexive reports whether the == operation on the type is reflexive.
// That is, x == x for all values x of type t.
internal static bool isReflexive(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var exprᴛ1 = ((ΔKind)(nuint)(uint8)t.Kind());
    if (exprᴛ1 == ΔBool || exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64 || exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr || exprᴛ1 == Chan || exprᴛ1 == ΔPointer || exprᴛ1 == ΔString || exprᴛ1 == ΔUnsafePointer) {
        return true;
    }
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64 || exprᴛ1 == Complex64 || exprᴛ1 == Complex128 || exprᴛ1 == ΔInterface) {
        return false;
    }
    if (exprᴛ1 == Array) {
        var tt = (ж<arrayType>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        return isReflexive((~tt).Elem);
    }
    if (exprᴛ1 == Struct) {
        var tt = (ж<structType>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        foreach (var (_, f) in (~tt).Fields) {
            if (!isReflexive(f.Typ)) {
                return false;
            }
        }
        return true;
    }
    { /* default: */
        throw panic("isReflexive called on non-key type " + stringFor(Ꮡt));
    }

}

// Func, Map, Slice, Invalid

// needKeyUpdate reports whether map overwrites require the key to be copied.
internal static bool needKeyUpdate(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var exprᴛ1 = ((ΔKind)(nuint)(uint8)t.Kind());
    if (exprᴛ1 == ΔBool || exprᴛ1 == ΔInt || exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64 || exprᴛ1 == ΔUint || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == Uintptr || exprᴛ1 == Chan || exprᴛ1 == ΔPointer || exprᴛ1 == ΔUnsafePointer) {
        return false;
    }
    if (exprᴛ1 == Float32 || exprᴛ1 == Float64 || exprᴛ1 == Complex64 || exprᴛ1 == Complex128 || exprᴛ1 == ΔInterface || exprᴛ1 == ΔString) {
        return true;
    }
    if (exprᴛ1 == Array) {
        var tt = (ж<arrayType>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        return needKeyUpdate((~tt).Elem);
    }
    if (exprᴛ1 == Struct) {
        var tt = (ж<structType>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        foreach (var (_, f) in (~tt).Fields) {
            // Float keys can be updated from +0 to -0.
            // String keys can be updated to use a smaller backing store.
            // Interfaces might have floats or strings in them.
            if (needKeyUpdate(f.Typ)) {
                return true;
            }
        }
        return false;
    }
    { /* default: */
        throw panic("needKeyUpdate called on non-key type " + stringFor(Ꮡt));
    }

}

// Func, Map, Slice, Invalid

// hashMightPanic reports whether the hash of a map key of type t might panic.
internal static bool hashMightPanic(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var exprᴛ1 = ((ΔKind)(nuint)(uint8)t.Kind());
    if (exprᴛ1 == ΔInterface) {
        return true;
    }
    if (exprᴛ1 == Array) {
        var tt = (ж<arrayType>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        return hashMightPanic((~tt).Elem);
    }
    if (exprᴛ1 == Struct) {
        var tt = (ж<structType>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        foreach (var (_, f) in (~tt).Fields) {
            if (hashMightPanic(f.Typ)) {
                return true;
            }
        }
        return false;
    }
    { /* default: */
        return false;
    }

}

internal static ж<abi.Type> bucketOf(ж<abi.Type> Ꮡktyp, ж<abi.Type> Ꮡetyp) {
    ref var ktyp = ref Ꮡktyp.Value;
    ref var etyp = ref Ꮡetyp.Value;

    if (ktyp.Size_ > abi.MapMaxKeyBytes) {
        Ꮡktyp = ptrTo(Ꮡktyp); ktyp = ref Ꮡktyp.Value;
    }
    if (etyp.Size_ > abi.MapMaxElemBytes) {
        Ꮡetyp = ptrTo(Ꮡetyp); etyp = ref Ꮡetyp.Value;
    }
    // Prepare GC data if any.
    // A bucket is at most bucketSize*(1+maxKeySize+maxValSize)+ptrSize bytes,
    // or 2064 bytes, or 258 pointer-size words, or 33 bytes of pointer bitmap.
    // Note that since the key and value are known to be <= 128 bytes,
    // they're guaranteed to have bitmaps instead of GC programs.
    ж<byte> gcdata = default!;
    ref var ptrdata = ref heap(new uintptr(), out var Ꮡptrdata);
    ref var size = ref heap<uintptr>(out var Ꮡsize);
    size = (uintptr)abi.MapBucketCount * (1 + ktyp.Size_ + etyp.Size_) + (uintptr)goarch.PtrSize;
    if ((uintptr)(size & (uintptr)(ktyp.Align_ - 1)) != 0 || (uintptr)(size & (uintptr)(etyp.Align_ - 1)) != 0) {
        throw panic("reflect: bad size computation in MapOf");
    }
    if (ktyp.Pointers() || etyp.Pointers()) {
        var nptr = ((uintptr)abi.MapBucketCount * (1 + ktyp.Size_ + etyp.Size_) + (uintptr)goarch.PtrSize) / (uintptr)goarch.PtrSize;
        var n = (nptr + 7) / 8;
        // Runtime needs pointer masks to be a multiple of uintptr in size.
        n = (uintptr)((n + (uintptr)goarch.PtrSize - 1) & ~(uintptr)(goarch.PtrSize - 1));
        var mask = new slice<byte>((nint)(n));
        var @base = (uintptr)(abi.MapBucketCount / goarch.PtrSize);
        if (ktyp.Pointers()) {
            emitGCMask(mask, @base, Ꮡktyp, abi.MapBucketCount);
        }
        @base += (uintptr)abi.MapBucketCount * ktyp.Size_ / (uintptr)goarch.PtrSize;
        if (etyp.Pointers()) {
            emitGCMask(mask, @base, Ꮡetyp, abi.MapBucketCount);
        }
        @base += (uintptr)abi.MapBucketCount * etyp.Size_ / (uintptr)goarch.PtrSize;
        var word = @base;
        mask[(nint)(word / 8)] |= (byte)((byte)(1 << (int)((word % 8))));
        gcdata = Ꮡ(mask, 0);
        ptrdata = (word + 1) * (uintptr)goarch.PtrSize;
        // overflow word must be last
        if (ptrdata != size) {
            throw panic("reflect: bad layout computation in MapOf");
        }
    }
    var b = Ꮡ(new abi.Type(
        Align_: goarch.PtrSize,
        Size_: size,
        Kind_: abi.Struct,
        PtrBytes: ptrdata,
        GCData: gcdata
    ));
    @string s = "bucket("u8 + stringFor(Ꮡktyp) + ","u8 + stringFor(Ꮡetyp) + ")"u8;
    b.Value.Str = resolveReflectName(newName(s, ""u8, false, false));
    return b;
}

[GoRecv] internal static unsafe slice<byte> gcSlice(this ref rtype t, uintptr begin, uintptr end) {
    return new slice<byte>(new ReadOnlySpan<byte>((byte*)(uintptr)(new @unsafe.Pointer(t.t.GCData)), (int)(end)));
}

// emitGCMask writes the GC mask for [n]typ into out, starting at bit
// offset base.
internal static void emitGCMask(slice<byte> @out, uintptr @base, ж<abi.Type> Ꮡtyp, uintptr n) {
    ref var typ = ref Ꮡtyp.Value;

    if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0) {
        throw panic("reflect: unexpected GC program");
    }
    var ptrs = typ.PtrBytes / (uintptr)goarch.PtrSize;
    var words = typ.Size_ / (uintptr)goarch.PtrSize;
    var mask = typ.GcSlice(0, (ptrs + 7) / 8);
    for (var j = (uintptr)0; j < ptrs; j++) {
        if ((byte)(((mask[(nint)(j / 8)] >> (int)((j % 8)))) & 1) != 0) {
            for (var i = (uintptr)0; i < n; i++) {
                var k = @base + i * words + j;
                @out[(nint)(k / 8)] |= (byte)((byte)(1 << (int)((k % 8))));
            }
        }
    }
}

// appendGCProg appends the GC program for the first ptrdata bytes of
// typ to dst and returns the extended slice.
internal static slice<byte> appendGCProg(slice<byte> dst, ж<abi.Type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.Value;

    if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0) {
        // Element has GC program; emit one element.
        var n = (uintptr)(~(ж<uint32>)(uintptr)(new @unsafe.Pointer(typ.GCData)));
        var prog = typ.GcSlice(4, 4 + n - 1);
        return builtin.append(dst, prog.ꓸꓸꓸ);
    }
    // Element is small with pointer mask; use as literal bits.
    var ptrs = typ.PtrBytes / (uintptr)goarch.PtrSize;
    var mask = typ.GcSlice(0, (ptrs + 7) / 8);
    // Emit 120-bit chunks of full bytes (max is 127 but we avoid using partial bytes).
    for (; ptrs > 120; ptrs -= 120) {
        dst = builtin.append(dst, (byte)(120));
        dst = builtin.append(dst, mask[..15].ꓸꓸꓸ);
        mask = mask[15..];
    }
    dst = builtin.append(dst, (byte)ptrs);
    dst = builtin.append(dst, mask.ꓸꓸꓸ);
    return dst;
}

// SliceOf returns the slice type with element type t.
// For example, if t represents int, SliceOf(t) represents []int.
public static ΔType SliceOf(ΔType t) {
    var typ = t.common();
    // Look in cache.
    var ckey = new cacheKey(ΔSlice, typ, nil, 0);
    {
        var (sliceΔ1, ok) = ᏑlookupCache.Load(ckey); if (ok) {
            return sliceΔ1._<ΔType>();
        }
    }
    // Look in known types.
    @string s = "[]"u8 + stringFor(typ);
    foreach (var (_, tt) in typesByString(s)) {
        var sliceΔ2 = (ж<sliceType>)(uintptr)(new @unsafe.Pointer(tt));
        if ((~sliceΔ2).Elem == typ) {
            var (tiΔ1, _) = ᏑlookupCache.LoadOrStore(ckey, toRType(tt));
            return tiΔ1._<ΔType>();
        }
    }
    // Make a slice type.
    ref var islice = ref heap<any>(out var Ꮡislice);

    islice = (slice<@unsafe.Pointer>)(default!);
    var prototype = ~(ж<ж<sliceType>>)(uintptr)(new @unsafe.Pointer(Ꮡislice));
    ref var Δslice = ref heap<sliceType>(out var Ꮡslice);
    Δslice = prototype.Value;
    Δslice.TFlag = 0;
    Δslice.Str = resolveReflectName(newName(s, ""u8, false, false));
    Δslice.Hash = fnv1((~typ).Hash, (rune)'[');
    Δslice.Elem = typ;
    Δslice.PtrToThis = 0;
    var (ti, _) = ᏑlookupCache.LoadOrStore(ckey, toRType(Ꮡslice.of(sliceType.ᏑType)));
    return ti._<ΔType>();
}

// The structLookupCache caches StructOf lookups.
// StructOf does not share the common lookupCache since we need to pin
// the memory associated with *structTypeFixedN.

[GoType("dyn")] partial struct structLookupCacheᴛ1 {
    public partial ref sync_package.Mutex Mutex { get; } // Guards stores (but not loads) on m.
    // m is a map[uint32][]Type keyed by the hash calculated in StructOf.
    // Elements in m are append-only and thus safe for concurrent reading.
    internal Δsync.Map m;
}
internal static ж<structLookupCacheᴛ1> ᏑstructLookupCache = new(new structLookupCacheᴛ1(nil));
internal static ref structLookupCacheᴛ1 structLookupCache => ref ᏑstructLookupCache.Value;

[GoType] partial struct structTypeUncommon {
    internal partial ref structType structType { get; }
    internal uncommonType u;
}

// isLetter reports whether a given 'rune' is classified as a Letter.
internal static bool isLetter(rune ch) {
    return (rune)'a' <= ch && ch <= (rune)'z' || (rune)'A' <= ch && ch <= (rune)'Z' || ch == (rune)'_' || ch >= utf8.RuneSelf && Δunicode.IsLetter(ch);
}

// isValidFieldName checks if a string is a valid (struct) field name or not.
//
// According to the language spec, a field name should be an identifier.
//
// identifier = letter { letter | unicode_digit } .
// letter = unicode_letter | "_" .
internal static bool isValidFieldName(@string fieldName) {
    foreach (var (i, c) in fieldName) {
        if (i == 0 && !isLetter(c)) {
            return false;
        }
        if (!(isLetter(c) || Δunicode.IsDigit(c))) {
            return false;
        }
    }
    return len(fieldName) > 0;
}

// This must match cmd/compile/internal/compare.IsRegularMemory
internal static bool isRegularMemory(ΔType t) {
    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == Array) {
        var elem = t.Elem();
        if (isRegularMemory(elem)) {
            return true;
        }
        return elem.Comparable() && t.Len() == 0;
    }
    if (exprᴛ1 == Int8 || exprᴛ1 == Int16 || exprᴛ1 == Int32 || exprᴛ1 == Int64 || exprᴛ1 == ΔInt || exprᴛ1 == Uint8 || exprᴛ1 == Uint16 || exprᴛ1 == Uint32 || exprᴛ1 == Uint64 || exprᴛ1 == ΔUint || exprᴛ1 == Uintptr || exprᴛ1 == Chan || exprᴛ1 == ΔPointer || exprᴛ1 == ΔBool || exprᴛ1 == ΔUnsafePointer) {
        return true;
    }
    if (exprᴛ1 == Struct) {
        nint num = t.NumField();
        switch (num) {
        case 0: {
            return true;
        }
        case 1: {
            var field = t.Field(0);
            if (field.Name == "_"u8) {
                return false;
            }
            return isRegularMemory(field.Type);
        }
        default: {
            foreach (var i in range(num)) {
                var field = t.Field(i);
                if (field.Name == "_"u8 || !isRegularMemory(field.Type) || isPaddedField(t, i)) {
                    return false;
                }
            }
            return true;
        }}

    }

    return false;
}

// isPaddedField reports whether the i'th field of struct type t is followed
// by padding.
internal static bool isPaddedField(ΔType t, nint i) {
    var field = t.Field(i);
    if (i + 1 < t.NumField()) {
        return field.Offset + field.Type.Size() != t.Field(i + 1).Offset;
    }
    return field.Offset + field.Type.Size() != t.Size();
}

// StructOf returns the struct type containing fields.
// The Offset and Index fields are ignored and computed as they would be
// by the compiler.
//
// StructOf currently does not support promoted methods of embedded fields
// and panics if passed unexported StructFields.
public static ΔType StructOf(slice<StructField> fields) => func((defer, recover) => {
    uint32 hash = fnv1(0, slice<byte>((@string)"struct {").ꓸꓸꓸ);
    uintptr size = default!;
    uint8 typalign = default!;
    bool comparable = true;
    slice<abi.Method> methods = default!;
    slice<structField> fs = new slice<structField>(len(fields));
    slice<byte> repr = new slice<byte>(0, 64);
    map<@string, EmptyStruct> fset = new map<@string, EmptyStruct>{};                          // fields' names
    bool hasGCProg = false; // records whether a struct-field type has a GCProg
    var lastzero = (uintptr)0;
    repr = builtin.append(repr, ((@string)"struct {"u8).ꓸꓸꓸ);
    @string pkgpath = ""u8;
    foreach (var (i, field) in fields) {
        if (field.Name == ""u8) {
            throw panic("reflect.StructOf: field " + strconv.Itoa(i) + " has no name");
        }
        if (!isValidFieldName(field.Name)) {
            throw panic("reflect.StructOf: field " + strconv.Itoa(i) + " has invalid name");
        }
        if (field.Type == default!) {
            throw panic("reflect.StructOf: field " + strconv.Itoa(i) + " has no type");
        }
        var (f, fpkgpath) = runtimeStructField(field);
        var ft = f.Typ;
        if ((abiꓸKind)((~ft).Kind_ & abi.KindGCProg) != 0) {
            hasGCProg = true;
        }
        if (fpkgpath != ""u8) {
            if (pkgpath == ""u8){
                pkgpath = fpkgpath;
            } else 
            if (pkgpath != fpkgpath) {
                throw panic("reflect.Struct: fields with different PkgPath " + pkgpath + " and " + fpkgpath);
            }
        }
        // Update string and hash
        @string name = f.Name.Name();
        hash = fnv1(hash, slice<byte>(name).ꓸꓸꓸ);
        if (!f.Embedded()){
            repr = builtin.append(repr, ((@string)((" "u8 + name))).ꓸꓸꓸ);
        } else {
            // Embedded field
            if (f.Typ.Kind() == abi.Pointer) {
                // Embedded ** and *interface{} are illegal
                var elem = ft.Elem();
                {
                    var k = elem.Kind(); if (k == abi.Pointer || k == abi.Interface) {
                        throw panic("reflect.StructOf: illegal embedded field type " + stringFor(ft));
                    }
                }
            }
            var exprᴛ1 = ((ΔKind)(nuint)(uint8)f.Typ.Kind());
            if (exprᴛ1 == ΔInterface) {
                var ift = (ж<interfaceType>)(uintptr)(new @unsafe.Pointer(ft));
                foreach (var (_, m) in (~ift).Methods) {
                    if (pkgPath(ift.nameOff(m.Name)) != ""u8) {
                        // TODO(sbinet).  Issue 15924.
                        throw panic("reflect: embedded interface with unexported method(s) not implemented");
                    }
                    var fnStub = resolveReflectText((@unsafe.Pointer)abi.FuncPCABIInternal(embeddedIfaceMethStub));
                    methods = builtin.append(methods, new abi.Method(
                        Name: resolveReflectName(ift.nameOff(m.Name)),
                        Mtyp: resolveReflectType(ift.typeOff(m.Typ)),
                        Ifn: fnStub,
                        Tfn: fnStub
                    ));
                }
            }
            else if (exprᴛ1 == ΔPointer) {
                var ptr = (ж<ptrType>)(uintptr)(new @unsafe.Pointer(ft));
                {
                    var unt = ptr.of(ptrType.ᏑPtrType).of(abi.PtrType.ᏑType).Uncommon(); if (unt != nil) {
                        if (i > 0 && (~unt).Mcount > 0) {
                            // Issue 15924.
                            throw panic("reflect: embedded type with methods not implemented if type is not first field");
                        }
                        if (len(fields) > 1) {
                            throw panic("reflect: embedded type with methods not implemented if there is more than one field");
                        }
                        foreach (var (_, m) in unt.Methods()) {
                            var mname = nameOffFor(ft, m.Name);
                            if (pkgPath(mname) != ""u8) {
                                // TODO(sbinet).
                                // Issue 15924.
                                throw panic("reflect: embedded interface with unexported method(s) not implemented");
                            }
                            methods = builtin.append(methods, new abi.Method(
                                Name: resolveReflectName(mname),
                                Mtyp: resolveReflectType(typeOffFor(ft, m.Mtyp)),
                                Ifn: resolveReflectText((uintptr)textOffFor(ft, m.Ifn)),
                                Tfn: resolveReflectText((uintptr)textOffFor(ft, m.Tfn))
                            ));
                        }
                    }
                }
                {
                    var unt = (~ptr).Elem.Uncommon(); if (unt != nil) {
                        foreach (var (_, m) in unt.Methods()) {
                            var mname = nameOffFor(ft, m.Name);
                            if (pkgPath(mname) != ""u8) {
                                // TODO(sbinet)
                                // Issue 15924.
                                throw panic("reflect: embedded interface with unexported method(s) not implemented");
                            }
                            methods = builtin.append(methods, new abi.Method(
                                Name: resolveReflectName(mname),
                                Mtyp: resolveReflectType(typeOffFor((~ptr).Elem, m.Mtyp)),
                                Ifn: resolveReflectText((uintptr)textOffFor((~ptr).Elem, m.Ifn)),
                                Tfn: resolveReflectText((uintptr)textOffFor((~ptr).Elem, m.Tfn))
                            ));
                        }
                    }
                }
            }
            else { /* default: */
                {
                    var unt = ft.Uncommon(); if (unt != nil) {
                        if (i > 0 && (~unt).Mcount > 0) {
                            // Issue 15924.
                            throw panic("reflect: embedded type with methods not implemented if type is not first field");
                        }
                        if (len(fields) > 1 && (abiꓸKind)((~ft).Kind_ & abi.KindDirectIface) != 0) {
                            throw panic("reflect: embedded type with methods not implemented for non-pointer type");
                        }
                        foreach (var (_, m) in unt.Methods()) {
                            var mname = nameOffFor(ft, m.Name);
                            if (pkgPath(mname) != ""u8) {
                                // TODO(sbinet)
                                // Issue 15924.
                                throw panic("reflect: embedded interface with unexported method(s) not implemented");
                            }
                            methods = builtin.append(methods, new abi.Method(
                                Name: resolveReflectName(mname),
                                Mtyp: resolveReflectType(typeOffFor(ft, m.Mtyp)),
                                Ifn: resolveReflectText((uintptr)textOffFor(ft, m.Ifn)),
                                Tfn: resolveReflectText((uintptr)textOffFor(ft, m.Tfn))
                            ));
                        }
                    }
                }
            }

        }
        {
            var (_, dup) = fset[name, ꟷ]; if (dup && name != "_"u8) {
                throw panic("reflect.StructOf: duplicate field " + name);
            }
        }
        fset[name] = new EmptyStruct();
        hash = fnv1(hash, (byte)(((~ft).Hash >> (int)(24))), (byte)(((~ft).Hash >> (int)(16))), (byte)(((~ft).Hash >> (int)(8))), (byte)(~ft).Hash);
        repr = builtin.append(repr, ((@string)((" "u8 + stringFor(ft)))).ꓸꓸꓸ);
        if (f.Name.HasTag()) {
            hash = fnv1(hash, slice<byte>(f.Name.Tag()).ꓸꓸꓸ);
            repr = builtin.append(repr, ((@string)((" "u8 + strconv.Quote(f.Name.Tag())))).ꓸꓸꓸ);
        }
        if (i < len(fields) - 1) {
            repr = builtin.append(repr, (byte)((rune)';'));
        }
        comparable = comparable && ((~ft).Equal != default!);
        var offset = align(size, (uintptr)(~ft).Align_);
        if (offset < size) {
            throw panic("reflect.StructOf: struct size would exceed virtual address space");
        }
        if ((~ft).Align_ > typalign) {
            typalign = ft.Value.Align_;
        }
        size = offset + (~ft).Size_;
        if (size < offset) {
            throw panic("reflect.StructOf: struct size would exceed virtual address space");
        }
        f.Offset = offset;
        if ((~ft).Size_ == 0) {
            lastzero = size;
        }
        fs[i] = f;
    }
    if (size > 0 && lastzero == size) {
        // This is a non-zero sized struct that ends in a
        // zero-sized field. We add an extra byte of padding,
        // to ensure that taking the address of the final
        // zero-sized field can't manufacture a pointer to the
        // next object in the heap. See issue 9401.
        size++;
        if (size == 0) {
            throw panic("reflect.StructOf: struct size would exceed virtual address space");
        }
    }
    ж<structType> typ = default!;
    ж<uncommonType> ut = default!;
    if (len(methods) == 0){
        var t = @new<structTypeUncommon>();
        typ = t.of(structTypeUncommon.ᏑstructType);
        ut = t.of(structTypeUncommon.Ꮡu);
    } else {
        // A *rtype representing a struct is followed directly in memory by an
        // array of method objects representing the methods attached to the
        // struct. To get the same layout for a run time generated type, we
        // need an array directly following the uncommonType memory.
        // A similar strategy is used for funcTypeFixed4, ...funcTypeFixedN.
        var tt = New(StructOf(new StructField[]{
            new(Name: "S"u8, Type: TypeOf(new structType(nil))),
            new(Name: "U"u8, Type: TypeOf(new uncommonType())),
            new(Name: "M"u8, Type: ArrayOf(len(methods), TypeOf(methods[0])))
        }.slice()));
        typ = (ж<structType>)(uintptr)(tt.Elem().Field(0).Addr().UnsafePointer());
        ut = (ж<uncommonType>)(uintptr)(tt.Elem().Field(1).Addr().UnsafePointer());
        copy(tt.Elem().Field(2).Slice(0, len(methods)).Interface()._<slice<abi.Method>>(), methods);
    }
    // TODO(sbinet): Once we allow embedding multiple types,
    // methods will need to be sorted like the compiler does.
    // TODO(sbinet): Once we allow non-exported methods, we will
    // need to compute xcount as the number of exported methods.
    ut.Value.Mcount = (uint16)len(methods);
    ut.Value.Xcount = ut.Value.Mcount;
    ut.Value.Moff = (uint32)@unsafe.Sizeof(new uncommonType());
    if (len(fs) > 0) {
        repr = builtin.append(repr, (byte)((rune)' '));
    }
    repr = builtin.append(repr, (byte)((rune)'}'));
    hash = fnv1(hash, (rune)'}');
    @string str = ((@string)repr);
    // Round the size up to be a multiple of the alignment.
    var s = align(size, (uintptr)typalign);
    if (s < size) {
        throw panic("reflect.StructOf: struct size would exceed virtual address space");
    }
    size = s;
    // Make the struct type.
    ref var istruct = ref heap<any>(out var Ꮡistruct);

    istruct = new EmptyStruct();
    var prototype = ~(ж<ж<structType>>)(uintptr)(new @unsafe.Pointer(Ꮡistruct));
    typ.Value = prototype.Value;
    typ.Value.Fields = fs;
    if (pkgpath != ""u8) {
        typ.Value.PkgPath = newName(pkgpath, ""u8, false, false);
    }
    // Look in cache.
    {
        var (ts, ok) = ᏑstructLookupCache.of(structLookupCacheᴛ1.Ꮡm).Load(hash); if (ok) {
            foreach (var (_, st) in ts._<slice<ΔType>>()) {
                var t = st.common();
                if (haveIdenticalUnderlyingType(typ.of(structType.ᏑType), t, true)) {
                    return toType(t);
                }
            }
        }
    }
    // Not in cache, lock and retry.
    ᏑstructLookupCache.of(structLookupCacheᴛ1.ᏑMutex).Lock();
    defer(ᏑstructLookupCache.of(structLookupCacheᴛ1.ᏑMutex).Unlock);
    {
        var (ts, ok) = ᏑstructLookupCache.of(structLookupCacheᴛ1.Ꮡm).Load(hash); if (ok) {
            foreach (var (_, st) in ts._<slice<ΔType>>()) {
                var t = st.common();
                if (haveIdenticalUnderlyingType(typ.of(structType.ᏑType), t, true)) {
                    return toType(t);
                }
            }
        }
    }
    var addToCache = (ΔType t) => {
        slice<ΔType> ts = default!;
        {
            var (ti, ok) = ᏑstructLookupCache.of(structLookupCacheᴛ1.Ꮡm).Load(hash); if (ok) {
                ts = ti._<slice<ΔType>>();
            }
        }
        ᏑstructLookupCache.of(structLookupCacheᴛ1.Ꮡm).Store(hash, builtin.append(ts, t));
        return t;
    };
    // Look in known types.
    foreach (var (_, t) in typesByString(str)) {
        if (haveIdenticalUnderlyingType(typ.of(structType.ᏑType), t, true)) {
            // even if 't' wasn't a structType with methods, we should be ok
            // as the 'u uncommonType' field won't be accessed except when
            // tflag&abi.TFlagUncommon is set.
            return addToCache(toType(t));
        }
    }
    typ.Value.Str = resolveReflectName(newName(str, ""u8, false, false));
    if (isRegularMemory(toType(typ.of(structType.ᏑType)))){
        typ.Value.TFlag = abi.TFlagRegularMemory;
    } else {
        typ.Value.TFlag = 0;
    }
    typ.Value.Hash = hash;
    typ.Value.Size_ = size;
    typ.Value.PtrBytes = typeptrdata(typ.of(structType.ᏑType));
    typ.Value.Align_ = typalign;
    typ.Value.FieldAlign_ = typalign;
    typ.Value.PtrToThis = 0;
    if (len(methods) > 0) {
        typ.Value.TFlag |= abi.TFlagUncommon;
    }
    if (hasGCProg){
        nint lastPtrField = 0;
        foreach (var (i, ft) in fs) {
            if (ft.Typ.Pointers()) {
                lastPtrField = i;
            }
        }
        var prog = new byte[]{0, 0, 0, 0}.slice();
        // will be length of prog
        uintptr off = default!;
        foreach (var (i, ft) in fs) {
            if (i > lastPtrField) {
                // gcprog should not include anything for any field after
                // the last field that contains pointer data
                break;
            }
            if (!ft.Typ.Pointers()) {
                // Ignore pointerless fields.
                continue;
            }
            // Pad to start of this field with zeros.
            if (ft.Offset > off) {
                var n = (ft.Offset - off) / (uintptr)goarch.PtrSize;
                prog = builtin.append(prog, (byte)(0x01), (byte)(0x00));
                // emit a 0 bit
                if (n > 1) {
                    prog = builtin.append(prog, (byte)(0x81));
                    // repeat previous bit
                    prog = appendVarint(prog, n - 1);
                }
                // n-1 times
                off = ft.Offset;
            }
            prog = appendGCProg(prog, ft.Typ);
            off += ft.Typ.Value.PtrBytes;
        }
        prog = builtin.append(prog, (byte)(0));
        ((ж<uint32>)(uintptr)(new @unsafe.Pointer(Ꮡ(prog, 0)))).Value = (uint32)(len(prog) - 4);
        typ.Value.Kind_ |= abi.KindGCProg;
        typ.Value.GCData = Ꮡ(prog, 0);
    } else {
        typ.Value.Kind_ &= unchecked((abiꓸKind)~abi.KindGCProg);
        var bv = @new<bitVector>();
        addTypeBits(bv, 0, typ.of(structType.ᏑType));
        if (len((~bv).data) > 0) {
            typ.Value.GCData = Ꮡ((~bv).data, 0);
        }
    }
    typ.Value.Equal = default!;
    if (comparable) {
        var typʗ1 = typ;
        typ.Value.Equal = (@unsafe.Pointer p, @unsafe.Pointer q) => {
            foreach (var (_, vᴛ1) in (~typʗ1).Fields) {
                ref var ft = ref heap(new abi.StructField(), out var Ꮡft);
                ft = vᴛ1;

                @unsafe.Pointer pi = (uintptr)add(p, ft.Offset, "&x.field safe"u8);
                @unsafe.Pointer qi = (uintptr)add(q, ft.Offset, "&x.field safe"u8);
                if (!(~ft.Typ).Equal(pi, qi)) {
                    return false;
                }
            }
            return true;
        };
    }
    switch (ᐧ) {
    case {} when len(fs) == 1 && !fs[0].Typ.IfaceIndir(): {
        typ.Value.Kind_ |= abi.KindDirectIface;
        break;
    }
    default: {
        typ.Value.Kind_ &= unchecked((abiꓸKind)~abi.KindDirectIface);
        break;
    }}

    // structs of 1 direct iface type can be direct
    return addToCache(toType(typ.of(structType.ᏑType)));
});

internal static void embeddedIfaceMethStub() {
    throw panic("reflect: StructOf does not support methods of embedded interfaces");
}

// runtimeStructField takes a StructField value passed to StructOf and
// returns both the corresponding internal representation, of type
// structField, and the pkgpath value to use for this field.
internal static (structField, @string) runtimeStructField(StructField field) {
    if (field.Anonymous && field.PkgPath != ""u8) {
        throw panic("reflect.StructOf: field \"" + field.Name + "\" is anonymous but has PkgPath set");
    }
    if (field.IsExported()) {
        // Best-effort check for misuse.
        // Since this field will be treated as exported, not much harm done if Unicode lowercase slips through.
        var c = field.Name[0];
        if ((rune)'a' <= c && c <= (rune)'z' || c == (rune)'_') {
            throw panic("reflect.StructOf: field \"" + field.Name + "\" is unexported but missing PkgPath");
        }
    }
    resolveReflectType(field.Type.common());
    // install in runtime
    var f = new structField(
        Name: newName(field.Name, ((@string)field.Tag), field.IsExported(), field.Anonymous),
        Typ: field.Type.common(),
        Offset: 0
    );
    return (f, field.PkgPath);
}

// typeptrdata returns the length in bytes of the prefix of t
// containing pointer data. Anything after this offset is scalar data.
// keep in sync with ../cmd/compile/internal/reflectdata/reflect.go
internal static uintptr typeptrdata(ж<abi.Type> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    var exprᴛ1 = t.Kind();
    if (exprᴛ1 == abi.Struct) {
        var st = (ж<structType>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        nint field = -1;
        foreach (var (i, _) in (~st).Fields) {
            // find the last field that has pointers.
            var ft = (~st).Fields[i].Typ;
            if (ft.Pointers()) {
                field = i;
            }
        }
        if (field == -1) {
            return 0;
        }
        var f = (~st).Fields[field];
        return f.Offset + (~f.Typ).PtrBytes;
    }
    { /* default: */
        throw panic("reflect.typeptrdata: unexpected type, " + stringFor(Ꮡt));
    }

}

// ArrayOf returns the array type with the given length and element type.
// For example, if t represents int, ArrayOf(5, t) represents [5]int.
//
// If the resulting type would be larger than the available address space,
// ArrayOf panics.
public static ΔType ArrayOf(nint length, ΔType elem) {
    if (length < 0) {
        throw panic("reflect: negative length passed to ArrayOf");
    }
    var typ = elem.common();
    // Look in cache.
    var ckey = new cacheKey(Array, typ, nil, (uintptr)length);
    {
        var (arrayΔ1, ok) = ᏑlookupCache.Load(ckey); if (ok) {
            return arrayΔ1._<ΔType>();
        }
    }
    // Look in known types.
    @string s = "["u8 + strconv.Itoa(length) + "]"u8 + stringFor(typ);
    foreach (var (_, tt) in typesByString(s)) {
        var arrayΔ2 = (ж<arrayType>)(uintptr)(new @unsafe.Pointer(tt));
        if ((~arrayΔ2).Elem == typ) {
            var (tiΔ1, _) = ᏑlookupCache.LoadOrStore(ckey, toRType(tt));
            return tiΔ1._<ΔType>();
        }
    }
    // Make an array type.
    ref var iarray = ref heap<any>(out var Ꮡiarray);

    iarray = new @unsafe.Pointer[]{}.array();
    var prototype = ~(ж<ж<arrayType>>)(uintptr)(new @unsafe.Pointer(Ꮡiarray));
    ref var Δarray = ref heap<abiꓸArrayType>(out var Ꮡarray);
    Δarray = prototype.Value;
    Δarray.TFlag = (abi.TFlag)((~typ).TFlag & abi.TFlagRegularMemory);
    Δarray.Str = resolveReflectName(newName(s, ""u8, false, false));
    Δarray.Hash = fnv1((~typ).Hash, (rune)'[');
    for (var n = (uint32)length; n > 0; n >>= (int)(8)) {
        Δarray.Hash = fnv1(Δarray.Hash, (byte)n);
    }
    Δarray.Hash = fnv1(Δarray.Hash, (rune)']');
    Δarray.Elem = typ;
    Δarray.PtrToThis = 0;
    if ((~typ).Size_ > 0) {
        var max = ~(uintptr)0 / (~typ).Size_;
        if ((uintptr)length > max) {
            throw panic("reflect.ArrayOf: array size would exceed virtual address space");
        }
    }
    Δarray.Size_ = (~typ).Size_ * (uintptr)length;
    if (length > 0 && typ.Pointers()) {
        Δarray.PtrBytes = (~typ).Size_ * (uintptr)(length - 1) + (~typ).PtrBytes;
    }
    Δarray.Align_ = typ.Value.Align_;
    Δarray.FieldAlign_ = typ.Value.FieldAlign_;
    Δarray.Len = (uintptr)length;
    Δarray.Slice = Ꮡ(((~SliceOf(elem)._<ж<rtype>>()).t));
    switch (ᐧ) {
    case {} when !typ.Pointers() || Δarray.Size_ == 0: {
        Δarray.GCData = default!;
        Δarray.PtrBytes = 0;
        break;
    }
    case {} when length is 1: {
        Δarray.Kind_ |= (abiꓸKind)((~typ).Kind_ & abi.KindGCProg);
        Δarray.GCData = typ.Value.GCData;
        Δarray.PtrBytes = typ.Value.PtrBytes;
        break;
    }
    case {} when (abiꓸKind)((~typ).Kind_ & abi.KindGCProg) == 0 && Δarray.Size_ <= abi.MaxPtrmaskBytes * 8 * goarch.PtrSize: {
        var n = (Δarray.PtrBytes / (uintptr)goarch.PtrSize + 7) / 8;
        n = (uintptr)((n + (uintptr)goarch.PtrSize - 1) & ~(uintptr)(goarch.PtrSize - 1));
        var mask = new slice<byte>((nint)(n));
        emitGCMask(mask, // No pointers.
 // In memory, 1-element array looks just like the element.
 // Element is small with pointer mask; array is still small.
 // Create direct pointer mask by turning each 1 bit in elem
 // into length 1 bits in larger mask.
 // Runtime needs pointer masks to be a multiple of uintptr in size.
 0, typ, Δarray.Len);
        Δarray.GCData = Ꮡ(mask, 0);
        break;
    }
    default: {
        var prog = new byte[]{ // Create program that emits one element
 // and then repeats to make the array.
0, 0, 0, 0}.slice();
        prog = appendGCProg(prog, // will be length of prog
 typ);
        var elemPtrs = (~typ).PtrBytes / (uintptr)goarch.PtrSize;
        var elemWords = (~typ).Size_ / (uintptr)goarch.PtrSize;
        if (elemPtrs < elemWords) {
            // Pad from ptrdata to size.
            // Emit literal 0 bit, then repeat as needed.
            prog = builtin.append(prog, (byte)(0x01), (byte)(0x00));
            if (elemPtrs + 1 < elemWords) {
                prog = builtin.append(prog, (byte)(0x81));
                prog = appendVarint(prog, elemWords - elemPtrs - 1);
            }
        }
        if (elemWords < 0x80){
            // Repeat length-1 times.
            prog = builtin.append(prog, (byte)((uintptr)(elemWords | 0x80)));
        } else {
            prog = builtin.append(prog, (byte)(0x80));
            prog = appendVarint(prog, elemWords);
        }
        prog = appendVarint(prog, (uintptr)length - 1);
        prog = builtin.append(prog, (byte)(0));
        ((ж<uint32>)(uintptr)(new @unsafe.Pointer(Ꮡ(prog, 0)))).Value = (uint32)(len(prog) - 4);
        Δarray.Kind_ |= abi.KindGCProg;
        Δarray.GCData = Ꮡ(prog, 0);
        Δarray.PtrBytes = Δarray.Size_;
        break;
    }}

    // overestimate but ok; must match program
    var etyp = typ;
    var esize = etyp.Size();
    Δarray.Equal = default!;
    {
        var eequal = etyp.Value.Equal; if (eequal != default!) {
            var eequalʗ1 = eequal;
            Δarray.Equal = (@unsafe.Pointer p, @unsafe.Pointer q) => {
                for (nint i = 0; i < length; i++) {
                    @unsafe.Pointer pi = (uintptr)arrayAt(p, i, esize, "i < length"u8);
                    @unsafe.Pointer qi = (uintptr)arrayAt(q, i, esize, "i < length"u8);
                    if (!eequalʗ1(pi, qi)) {
                        return false;
                    }
                }
                return true;
            };
        }
    }
    switch (ᐧ) {
    case {} when length == 1 && !typ.IfaceIndir(): {
        Δarray.Kind_ |= abi.KindDirectIface;
        break;
    }
    default: {
        Δarray.Kind_ &= unchecked((abiꓸKind)~abi.KindDirectIface);
        break;
    }}

    // array of 1 direct iface type can be direct
    var (ti, _) = ᏑlookupCache.LoadOrStore(ckey, toRType(Ꮡarray.of(arrayType.ᏑType)));
    return ti._<ΔType>();
}

internal static slice<byte> appendVarint(slice<byte> x, uintptr v) {
    for (; v >= 0x80; v >>= (int)(7)) {
        x = builtin.append(x, (byte)((uintptr)(v | 0x80)));
    }
    x = builtin.append(x, (byte)v);
    return x;
}

// toType converts from a *rtype to a Type that can be returned
// to the client of package reflect. In gc, the only concern is that
// a nil *rtype must be replaced by a nil Type, but in gccgo this
// function takes care of ensuring that multiple *rtype for the same
// type are coalesced into a single Type.
//
// toType should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - fortio.org/log
//   - github.com/goccy/go-json
//   - github.com/goccy/go-reflect
//   - github.com/sohaha/zlsgo
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname toType
internal static ΔType toType(ж<abi.Type> Ꮡt) {
    if (Ꮡt == nil) {
        return default!;
    }
    return new rtypeжΔType(toRType(Ꮡt));
}

[GoType] partial struct layoutKey {
    internal ж<funcType> ftyp; // function signature
    internal ж<abi.Type> rcvr; // receiver type, or nil if none
}

[GoType] partial struct layoutType {
    internal ж<abi.Type> t;
    internal ж<Δsync.Pool> framePool;
    internal abiDesc abid;
}

internal static ж<Δsync.Map> ᏑlayoutCache = new(default(Δsync.Map));
internal static ref Δsync.Map layoutCache => ref ᏑlayoutCache.Value; // map[layoutKey]layoutType

// funcLayout computes a struct type representing the layout of the
// stack-assigned function arguments and return values for the function
// type t.
// If rcvr != nil, rcvr specifies the type of the receiver.
// The returned type exists only for GC, so we only fill out GC relevant info.
// Currently, that's just size and the GC program. We also fill in
// the name for possible debugging use.
internal static (ж<abi.Type> frametype, ж<Δsync.Pool> framePool, abiDesc abid) funcLayout(ж<funcType> Ꮡt, ж<abi.Type> Ꮡrcvr) {
    ж<abi.Type> frametype = default!;
    ж<Δsync.Pool> framePool = default!;
    abiDesc abid = default!;

    ref var t = ref Ꮡt.Value;
    ref var rcvr = ref Ꮡrcvr.DerefOrNil();
    if (Ꮡt.of(funcType.ᏑType).Kind() != abi.Func) {
        throw panic("reflect: funcLayout of non-func type " + stringFor(Ꮡt.of(funcType.ᏑType)));
    }
    if (Ꮡrcvr != nil && rcvr.Kind() == abi.Interface) {
        throw panic("reflect: funcLayout with interface receiver " + stringFor(Ꮡrcvr));
    }
    var k = new layoutKey(Ꮡt, Ꮡrcvr);
    {
        var (ltiΔ1, ok) = ᏑlayoutCache.Load(k); if (ok) {
            var ltΔ1 = ltiΔ1._<layoutType>();
            return (ltΔ1.t, ltΔ1.framePool, ltΔ1.abid);
        }
    }
    // Compute the ABI layout.
    abid = newAbiDesc(Ꮡt, Ꮡrcvr);
    // build dummy rtype holding gc program
    var x = Ꮡ(new abi.Type(
        Align_: goarch.PtrSize, // Don't add spill space here; it's only necessary in
 // reflectcall's frame, not in the allocated frame.
 // TODO(mknyszek): Remove this comment when register
 // spill space in the frame is no longer required.

        Size_: align(abid.retOffset + abid.ret.stackBytes, goarch.PtrSize),
        PtrBytes: (uintptr)(~abid.stackPtrs).n * (uintptr)goarch.PtrSize
    ));
    if ((~abid.stackPtrs).n > 0) {
        x.Value.GCData = Ꮡ((~abid.stackPtrs).data, 0);
    }
    @string s = default!;
    if (Ꮡrcvr != nil){
        s = "methodargs("u8 + stringFor(Ꮡrcvr) + ")("u8 + stringFor(Ꮡt.of(funcType.ᏑType)) + ")"u8;
    } else {
        s = "funcargs("u8 + stringFor(Ꮡt.of(funcType.ᏑType)) + ")"u8;
    }
    x.Value.Str = resolveReflectName(newName(s, ""u8, false, false));
    // cache result for future callers
    var xʗ1 = x;
    framePool = Ꮡ(new Δsync.Pool(New: () => (uintptr)unsafe_New(xʗ1)
    ));
    var (lti, _) = ᏑlayoutCache.LoadOrStore(k, new layoutType(
        t: x,
        framePool: framePool,
        abid: abid
    ));
    var lt = lti._<layoutType>();
    return (lt.t, lt.framePool, lt.abid);
}

// Note: this type must agree with runtime.bitvector.
[GoType] partial struct bitVector {
    internal uint32 n; // number of bits
    internal slice<byte> data;
}

// append a bit to the bitmap.
[GoRecv] internal static void append(this ref bitVector bv, uint8 bit) {
    if (bv.n % (uint32)(8 * goarch.PtrSize) == 0) {
        // Runtime needs pointer masks to be a multiple of uintptr in size.
        // Since reflect passes bv.data directly to the runtime as a pointer mask,
        // we append a full uintptr of zeros at a time.
        for (nint i = 0; i < goarch.PtrSize; i++) {
            bv.data = builtin.append(bv.data, (byte)(0));
        }
    }
    bv.data[(nint)(bv.n / 8)] |= (uint8)((bit << (int)((bv.n % 8))));
    bv.n++;
}

internal static void addTypeBits(ж<bitVector> Ꮡbv, uintptr offset, ж<abi.Type> Ꮡt) {
    ref var bv = ref Ꮡbv.Value;
    ref var t = ref Ꮡt.Value;

    if (!t.Pointers()) {
        return;
    }
    var exprᴛ1 = ((ΔKind)(nuint)((uint8)((abiꓸKind)(t.Kind_ & abi.KindMask))));
    if (exprᴛ1 == Chan || exprᴛ1 == Func || exprᴛ1 == Map || exprᴛ1 == ΔPointer || exprᴛ1 == ΔSlice || exprᴛ1 == ΔString || exprᴛ1 == ΔUnsafePointer) {
        while (bv.n < (uint32)(offset / (uintptr)goarch.PtrSize)) {
            // 1 pointer at start of representation
            bv.append(0);
        }
        bv.append(1);
    }
    else if (exprᴛ1 == ΔInterface) {
        while (bv.n < (uint32)(offset / (uintptr)goarch.PtrSize)) {
            // 2 pointers
            bv.append(0);
        }
        bv.append(1);
        bv.append(1);
    }
    else if (exprᴛ1 == Array) {
        var tt = (ж<arrayType>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        for (nint i = 0; i < (nint)(~tt).Len; i++) {
            // repeat inner type
            addTypeBits(Ꮡbv, offset + (uintptr)i * (~(~tt).Elem).Size_, (~tt).Elem);
        }
    }
    else if (exprᴛ1 == Struct) {
        var tt = (ж<structType>)(uintptr)(new @unsafe.Pointer(Ꮡt));
        foreach (var (i, _) in (~tt).Fields) {
            // apply fields
            var f = Ꮡ((~tt).Fields, i);
            addTypeBits(Ꮡbv, offset + (~f).Offset, (~f).Typ);
        }
    }

}

// TypeFor returns the [Type] that represents the type argument T.
public static ΔType TypeFor<T>() {
    T v = default!;
    {
        var t = TypeOf(v); if (t != default!) {
            return t;
        }
    }
    // optimize for T being a non-interface kind
    return TypeOf((ж<T>)(default!)).Elem();
}

// only for an interface kind

} // end reflect_package
