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

// package reflect -- go2cs converted at 2022 March 13 05:41:39 UTC
// import "reflect" ==> using reflect = go.reflect_package
// Original source: C:\Program Files\Go\src\reflect\type.go
namespace go;

using unsafeheader = @internal.unsafeheader_package;
using strconv = strconv_package;
using sync = sync_package;
using unicode = unicode_package;
using utf8 = unicode.utf8_package;
using @unsafe = @unsafe_package;


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

using System;
public static partial class reflect_package {

public partial interface Type {
    ptr<uncommonType> Align(); // FieldAlign returns the alignment in bytes of a value of
// this type when used as a field in a struct.
    ptr<uncommonType> FieldAlign(); // Method returns the i'th method in the type's method set.
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
    ptr<uncommonType> Method(nint _p0); // MethodByName returns the method with that name in the type's
// method set and a boolean indicating if the method was found.
//
// For a non-interface type T or *T, the returned Method's Type and Func
// fields describe a function whose first argument is the receiver.
//
// For an interface type, the returned Method's Type field gives the
// method signature, without a receiver, and the Func field is nil.
    ptr<uncommonType> MethodByName(@string _p0); // NumMethod returns the number of methods accessible using Method.
//
// Note that NumMethod counts unexported methods only for interface types.
    ptr<uncommonType> NumMethod(); // Name returns the type's name within its package for a defined type.
// For other (non-defined) types it returns the empty string.
    ptr<uncommonType> Name(); // PkgPath returns a defined type's package path, that is, the import path
// that uniquely identifies the package, such as "encoding/base64".
// If the type was predeclared (string, error) or not defined (*T, struct{},
// []int, or A where A is an alias for a non-defined type), the package path
// will be the empty string.
    ptr<uncommonType> PkgPath(); // Size returns the number of bytes needed to store
// a value of the given type; it is analogous to unsafe.Sizeof.
    ptr<uncommonType> Size(); // String returns a string representation of the type.
// The string representation may use shortened package names
// (e.g., base64 instead of "encoding/base64") and is not
// guaranteed to be unique among types. To test for type identity,
// compare the Types directly.
    ptr<uncommonType> String(); // Kind returns the specific kind of this type.
    ptr<uncommonType> Kind(); // Implements reports whether the type implements the interface type u.
    ptr<uncommonType> Implements(Type u); // AssignableTo reports whether a value of the type is assignable to type u.
    ptr<uncommonType> AssignableTo(Type u); // ConvertibleTo reports whether a value of the type is convertible to type u.
// Even if ConvertibleTo returns true, the conversion may still panic.
// For example, a slice of type []T is convertible to *[N]T,
// but the conversion will panic if its length is less than N.
    ptr<uncommonType> ConvertibleTo(Type u); // Comparable reports whether values of this type are comparable.
// Even if Comparable returns true, the comparison may still panic.
// For example, values of interface type are comparable,
// but the comparison will panic if their dynamic type is not comparable.
    ptr<uncommonType> Comparable(); // Methods applicable only to some types, depending on Kind.
// The methods allowed for each kind are:
//
//    Int*, Uint*, Float*, Complex*: Bits
//    Array: Elem, Len
//    Chan: ChanDir, Elem
//    Func: In, NumIn, Out, NumOut, IsVariadic.
//    Map: Key, Elem
//    Ptr: Elem
//    Slice: Elem
//    Struct: Field, FieldByIndex, FieldByName, FieldByNameFunc, NumField

// Bits returns the size of the type in bits.
// It panics if the type's Kind is not one of the
// sized or unsized Int, Uint, Float, or Complex kinds.
    ptr<uncommonType> Bits(); // ChanDir returns a channel type's direction.
// It panics if the type's Kind is not Chan.
    ptr<uncommonType> ChanDir(); // IsVariadic reports whether a function type's final input parameter
// is a "..." parameter. If so, t.In(t.NumIn() - 1) returns the parameter's
// implicit actual type []T.
//
// For concreteness, if t represents func(x int, y ... float64), then
//
//    t.NumIn() == 2
//    t.In(0) is the reflect.Type for "int"
//    t.In(1) is the reflect.Type for "[]float64"
//    t.IsVariadic() == true
//
// IsVariadic panics if the type's Kind is not Func.
    ptr<uncommonType> IsVariadic(); // Elem returns a type's element type.
// It panics if the type's Kind is not Array, Chan, Map, Ptr, or Slice.
    ptr<uncommonType> Elem(); // Field returns a struct type's i'th field.
// It panics if the type's Kind is not Struct.
// It panics if i is not in the range [0, NumField()).
    ptr<uncommonType> Field(nint i); // FieldByIndex returns the nested field corresponding
// to the index sequence. It is equivalent to calling Field
// successively for each index i.
// It panics if the type's Kind is not Struct.
    ptr<uncommonType> FieldByIndex(slice<nint> index); // FieldByName returns the struct field with the given name
// and a boolean indicating if the field was found.
    ptr<uncommonType> FieldByName(@string name); // FieldByNameFunc returns the struct field with a name
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
    ptr<uncommonType> FieldByNameFunc(Func<@string, bool> match); // In returns the type of a function type's i'th input parameter.
// It panics if the type's Kind is not Func.
// It panics if i is not in the range [0, NumIn()).
    ptr<uncommonType> In(nint i); // Key returns a map type's key type.
// It panics if the type's Kind is not Map.
    ptr<uncommonType> Key(); // Len returns an array type's length.
// It panics if the type's Kind is not Array.
    ptr<uncommonType> Len(); // NumField returns a struct type's field count.
// It panics if the type's Kind is not Struct.
    ptr<uncommonType> NumField(); // NumIn returns a function type's input parameter count.
// It panics if the type's Kind is not Func.
    ptr<uncommonType> NumIn(); // NumOut returns a function type's output parameter count.
// It panics if the type's Kind is not Func.
    ptr<uncommonType> NumOut(); // Out returns the type of a function type's i'th output parameter.
// It panics if the type's Kind is not Func.
// It panics if i is not in the range [0, NumOut()).
    ptr<uncommonType> Out(nint i);
    ptr<uncommonType> common();
    ptr<uncommonType> uncommon();
}

// BUG(rsc): FieldByName and related functions consider struct field names to be equal
// if the names are equal, even if they are unexported names originating
// in different packages. The practical effect of this is that the result of
// t.FieldByName("x") is not well defined if the struct type t contains
// multiple fields named x (embedded from different packages).
// FieldByName may return one of the fields named x or may report that there are none.
// See https://golang.org/issue/4876 for more details.

/*
 * These data structures are known to the compiler (../../cmd/internal/reflectdata/reflect.go).
 * A few are known to ../runtime/type.go to convey to debuggers.
 * They are also known to ../runtime/type.go.
 */

// A Kind represents the specific kind of type that a Type represents.
// The zero Kind is not a valid kind.
public partial struct Kind { // : nuint
}

public static readonly Kind Invalid = iota;
public static readonly var Bool = 0;
public static readonly var Int = 1;
public static readonly var Int8 = 2;
public static readonly var Int16 = 3;
public static readonly var Int32 = 4;
public static readonly var Int64 = 5;
public static readonly var Uint = 6;
public static readonly var Uint8 = 7;
public static readonly var Uint16 = 8;
public static readonly var Uint32 = 9;
public static readonly var Uint64 = 10;
public static readonly var Uintptr = 11;
public static readonly var Float32 = 12;
public static readonly var Float64 = 13;
public static readonly var Complex64 = 14;
public static readonly var Complex128 = 15;
public static readonly var Array = 16;
public static readonly var Chan = 17;
public static readonly var Func = 18;
public static readonly var Interface = 19;
public static readonly var Map = 20;
public static readonly var Ptr = 21;
public static readonly var Slice = 22;
public static readonly var String = 23;
public static readonly var Struct = 24;
public static readonly var UnsafePointer = 25;

// tflag is used by an rtype to signal what extra type information is
// available in the memory directly following the rtype value.
//
// tflag values must be kept in sync with copies in:
//    cmd/compile/internal/reflectdata/reflect.go
//    cmd/link/internal/ld/decodesym.go
//    runtime/type.go
private partial struct tflag { // : byte
}

 
// tflagUncommon means that there is a pointer, *uncommonType,
// just beyond the outer type structure.
//
// For example, if t.Kind() == Struct and t.tflag&tflagUncommon != 0,
// then t has uncommonType data and it can be accessed as:
//
//    type tUncommon struct {
//        structType
//        u uncommonType
//    }
//    u := &(*tUncommon)(unsafe.Pointer(t)).u
private static readonly tflag tflagUncommon = 1 << 0; 

// tflagExtraStar means the name in the str field has an
// extraneous '*' prefix. This is because for most types T in
// a program, the type *T also exists and reusing the str data
// saves binary size.
private static readonly tflag tflagExtraStar = 1 << 1; 

// tflagNamed means the type has a name.
private static readonly tflag tflagNamed = 1 << 2; 

// tflagRegularMemory means that equal and hash functions can treat
// this type as a single region of t.size bytes.
private static readonly tflag tflagRegularMemory = 1 << 3;

// rtype is the common implementation of most values.
// It is embedded in other struct types.
//
// rtype must be kept in sync with ../runtime/type.go:/^type._type.
private partial struct rtype {
    public System.UIntPtr size;
    public System.UIntPtr ptrdata; // number of bytes in the type that can contain pointers
    public uint hash; // hash of type; avoids computation in hash tables
    public tflag tflag; // extra type information flags
    public byte align; // alignment of variable with this type
    public byte fieldAlign; // alignment of struct field with this type
    public byte kind; // enumeration for C
// function for comparing objects of this type
// (ptr to object A, ptr to object B) -> ==?
    public Func<unsafe.Pointer, unsafe.Pointer, bool> equal;
    public ptr<byte> gcdata; // garbage collection data
    public nameOff str; // string form
    public typeOff ptrToThis; // type for pointer to this type, may be zero
}

// Method on non-interface type
private partial struct method {
    public nameOff name; // name of method
    public typeOff mtyp; // method type (without receiver)
    public textOff ifn; // fn used in interface call (one-word receiver)
    public textOff tfn; // fn used for normal method call
}

// uncommonType is present only for defined types or types with methods
// (if T is a defined type, the uncommonTypes for T and *T have methods).
// Using a pointer to this struct reduces the overall size required
// to describe a non-defined type with no methods.
private partial struct uncommonType {
    public nameOff pkgPath; // import path; empty for built-in types like int, string
    public ushort mcount; // number of methods
    public ushort xcount; // number of exported methods
    public uint moff; // offset from this uncommontype to [mcount]method
    public uint _; // unused
}

// ChanDir represents a channel type's direction.
public partial struct ChanDir { // : nint
}

public static readonly ChanDir RecvDir = 1 << (int)(iota); // <-chan
public static readonly BothDir SendDir = RecvDir | SendDir; // chan

// arrayType represents a fixed array type.
private partial struct arrayType {
    public ref rtype rtype => ref rtype_val;
    public ptr<rtype> elem; // array element type
    public ptr<rtype> slice; // slice type
    public System.UIntPtr len;
}

// chanType represents a channel type.
private partial struct chanType {
    public ref rtype rtype => ref rtype_val;
    public ptr<rtype> elem; // channel element type
    public System.UIntPtr dir; // channel direction (ChanDir)
}

// funcType represents a function type.
//
// A *rtype for each in and out parameter is stored in an array that
// directly follows the funcType (and possibly its uncommonType). So
// a function type with one method, one input, and one output is:
//
//    struct {
//        funcType
//        uncommonType
//        [2]*rtype    // [0] is in, [1] is out
//    }
private partial struct funcType {
    public ref rtype rtype => ref rtype_val;
    public ushort inCount;
    public ushort outCount; // top bit is set if last input parameter is ...
}

// imethod represents a method on an interface type
private partial struct imethod {
    public nameOff name; // name of method
    public typeOff typ; // .(*FuncType) underneath
}

// interfaceType represents an interface type.
private partial struct interfaceType {
    public ref rtype rtype => ref rtype_val;
    public name pkgPath; // import path
    public slice<imethod> methods; // sorted by hash
}

// mapType represents a map type.
private partial struct mapType {
    public ref rtype rtype => ref rtype_val;
    public ptr<rtype> key; // map key type
    public ptr<rtype> elem; // map element (value) type
    public ptr<rtype> bucket; // internal bucket structure
// function for hashing keys (ptr to key, seed) -> hash
    public Func<unsafe.Pointer, System.UIntPtr, System.UIntPtr> hasher;
    public byte keysize; // size of key slot
    public byte valuesize; // size of value slot
    public ushort bucketsize; // size of bucket
    public uint flags;
}

// ptrType represents a pointer type.
private partial struct ptrType {
    public ref rtype rtype => ref rtype_val;
    public ptr<rtype> elem; // pointer element (pointed at) type
}

// sliceType represents a slice type.
private partial struct sliceType {
    public ref rtype rtype => ref rtype_val;
    public ptr<rtype> elem; // slice element type
}

// Struct field
private partial struct structField {
    public name name; // name is always non-empty
    public ptr<rtype> typ; // type of field
    public System.UIntPtr offsetEmbed; // byte offset of field<<1 | isEmbedded
}

private static System.UIntPtr offset(this ptr<structField> _addr_f) {
    ref structField f = ref _addr_f.val;

    return f.offsetEmbed >> 1;
}

private static bool embedded(this ptr<structField> _addr_f) {
    ref structField f = ref _addr_f.val;

    return f.offsetEmbed & 1 != 0;
}

// structType represents a struct type.
private partial struct structType {
    public ref rtype rtype => ref rtype_val;
    public name pkgPath;
    public slice<structField> fields; // sorted by offset
}

// name is an encoded type name with optional extra data.
//
// The first byte is a bit field containing:
//
//    1<<0 the name is exported
//    1<<1 tag data follows the name
//    1<<2 pkgPath nameOff follows the name and tag
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
//   runtime/type.go
//   internal/reflectlite/type.go
//   cmd/link/internal/ld/decodesym.go

private partial struct name {
    public ptr<byte> bytes;
}

private static ptr<byte> data(this name n, nint off, @string whySafe) {
    return _addr_(byte.val)(add(@unsafe.Pointer(n.bytes), uintptr(off), whySafe))!;
}

private static bool isExported(this name n) {
    return (n.bytes.val) & (1 << 0) != 0;
}

private static bool hasTag(this name n) {
    return (n.bytes.val) & (1 << 1) != 0;
}

// readVarint parses a varint as encoded by encoding/binary.
// It returns the number of encoded bytes and the encoded value.
private static (nint, nint) readVarint(this name n, nint off) {
    nint _p0 = default;
    nint _p0 = default;

    nint v = 0;
    for (nint i = 0; ; i++) {
        var x = n.data(off + i, "read varint").val;
        v += int(x & 0x7f) << (int)((7 * i));
        if (x & 0x80 == 0) {
            return (i + 1, v);
        }
    }
}

// writeVarint writes n to buf in varint form. Returns the
// number of bytes written. n must be nonnegative.
// Writes at most 10 bytes.
private static nint writeVarint(slice<byte> buf, nint n) {
    for (nint i = 0; ; i++) {
        var b = byte(n & 0x7f);
        n>>=7;
        if (n == 0) {
            buf[i] = b;
            return i + 1;
        }
        buf[i] = b | 0x80;
    }
}

private static @string name(this name n) {
    @string s = default;

    if (n.bytes == null) {
        return ;
    }
    var (i, l) = n.readVarint(1);
    var hdr = (unsafeheader.String.val)(@unsafe.Pointer(_addr_s));
    hdr.Data = @unsafe.Pointer(n.data(1 + i, "non-empty string"));
    hdr.Len = l;
    return ;
}

private static @string tag(this name n) {
    @string s = default;

    if (!n.hasTag()) {
        return "";
    }
    var (i, l) = n.readVarint(1);
    var (i2, l2) = n.readVarint(1 + i + l);
    var hdr = (unsafeheader.String.val)(@unsafe.Pointer(_addr_s));
    hdr.Data = @unsafe.Pointer(n.data(1 + i + l + i2, "non-empty string"));
    hdr.Len = l2;
    return ;
}

private static @string pkgPath(this name n) {
    if (n.bytes == null || n.data(0, "name flag field") & (1 << 2) == 0.val) {
        return "";
    }
    var (i, l) = n.readVarint(1);
    nint off = 1 + i + l;
    if (n.hasTag()) {
        var (i2, l2) = n.readVarint(off);
        off += i2 + l2;
    }
    ref int nameOff = ref heap(out ptr<int> _addr_nameOff); 
    // Note that this field may not be aligned in memory,
    // so we cannot use a direct int32 assignment here.
    copy(new ptr<ptr<array<byte>>>(@unsafe.Pointer(_addr_nameOff))[..], new ptr<ptr<array<byte>>>(@unsafe.Pointer(n.data(off, "name offset field")))[..]);
    name pkgPathName = new name((*byte)(resolveTypeOff(unsafe.Pointer(n.bytes),nameOff)));
    return pkgPathName.name();
}

private static name newName(@string n, @string tag, bool exported) => func((_, panic, _) => {
    if (len(n) >= 1 << 29) {
        panic("reflect.nameFrom: name too long: " + n[..(int)1024] + "...");
    }
    if (len(tag) >= 1 << 29) {
        panic("reflect.nameFrom: tag too long: " + tag[..(int)1024] + "...");
    }
    array<byte> nameLen = new array<byte>(10);
    array<byte> tagLen = new array<byte>(10);
    var nameLenLen = writeVarint(nameLen[..], len(n));
    var tagLenLen = writeVarint(tagLen[..], len(tag));

    byte bits = default;
    nint l = 1 + nameLenLen + len(n);
    if (exported) {
        bits |= 1 << 0;
    }
    if (len(tag) > 0) {
        l += tagLenLen + len(tag);
        bits |= 1 << 1;
    }
    var b = make_slice<byte>(l);
    b[0] = bits;
    copy(b[(int)1..], nameLen[..(int)nameLenLen]);
    copy(b[(int)1 + nameLenLen..], n);
    if (len(tag) > 0) {
        var tb = b[(int)1 + nameLenLen + len(n)..];
        copy(tb, tagLen[..(int)tagLenLen]);
        copy(tb[(int)tagLenLen..], tag);
    }
    return new name(bytes:&b[0]);
});

/*
 * The compiler knows the exact layout of all the data structures above.
 * The compiler does not know about the data structures and methods below.
 */

// Method represents a single method.
public partial struct Method {
    public @string Name; // PkgPath is the package path that qualifies a lower case (unexported)
// method name. It is empty for upper case (exported) method names.
// The combination of PkgPath and Name uniquely identifies a method
// in a method set.
// See https://golang.org/ref/spec#Uniqueness_of_identifiers
    public @string PkgPath;
    public Type Type; // method type
    public Value Func; // func with receiver as first argument
    public nint Index; // index for Type.Method
}

// IsExported reports whether the method is exported.
public static bool IsExported(this Method m) {
    return m.PkgPath == "";
}

private static readonly nint kindDirectIface = 1 << 5;
private static readonly nint kindGCProg = 1 << 6; // Type.gc points to GC program
private static readonly nint kindMask = (1 << 5) - 1;

// String returns the name of k.
public static @string String(this Kind k) {
    if (int(k) < len(kindNames)) {
        return kindNames[k];
    }
    return "kind" + strconv.Itoa(int(k));
}

private static @string kindNames = new slice<@string>(InitKeyedValues<@string>((Invalid, "invalid"), (Bool, "bool"), (Int, "int"), (Int8, "int8"), (Int16, "int16"), (Int32, "int32"), (Int64, "int64"), (Uint, "uint"), (Uint8, "uint8"), (Uint16, "uint16"), (Uint32, "uint32"), (Uint64, "uint64"), (Uintptr, "uintptr"), (Float32, "float32"), (Float64, "float64"), (Complex64, "complex64"), (Complex128, "complex128"), (Array, "array"), (Chan, "chan"), (Func, "func"), (Interface, "interface"), (Map, "map"), (Ptr, "ptr"), (Slice, "slice"), (String, "string"), (Struct, "struct"), (UnsafePointer, "unsafe.Pointer")));

private static slice<method> methods(this ptr<uncommonType> _addr_t) {
    ref uncommonType t = ref _addr_t.val;

    if (t.mcount == 0) {
        return null;
    }
    return new ptr<ptr<array<method>>>(add(@unsafe.Pointer(t), uintptr(t.moff), "t.mcount > 0")).slice(-1, t.mcount, t.mcount);
}

private static slice<method> exportedMethods(this ptr<uncommonType> _addr_t) {
    ref uncommonType t = ref _addr_t.val;

    if (t.xcount == 0) {
        return null;
    }
    return new ptr<ptr<array<method>>>(add(@unsafe.Pointer(t), uintptr(t.moff), "t.xcount > 0")).slice(-1, t.xcount, t.xcount);
}

// resolveNameOff resolves a name offset from a base pointer.
// The (*rtype).nameOff method is a convenience wrapper for this function.
// Implemented in the runtime package.
private static unsafe.Pointer resolveNameOff(unsafe.Pointer ptrInModule, int off);

// resolveTypeOff resolves an *rtype offset from a base type.
// The (*rtype).typeOff method is a convenience wrapper for this function.
// Implemented in the runtime package.
private static unsafe.Pointer resolveTypeOff(unsafe.Pointer rtype, int off);

// resolveTextOff resolves a function pointer offset from a base type.
// The (*rtype).textOff method is a convenience wrapper for this function.
// Implemented in the runtime package.
private static unsafe.Pointer resolveTextOff(unsafe.Pointer rtype, int off);

// addReflectOff adds a pointer to the reflection lookup map in the runtime.
// It returns a new ID that can be used as a typeOff or textOff, and will
// be resolved correctly. Implemented in the runtime package.
private static int addReflectOff(unsafe.Pointer ptr);

// resolveReflectName adds a name to the reflection lookup map in the runtime.
// It returns a new nameOff that can be used to refer to the pointer.
private static nameOff resolveReflectName(name n) {
    return nameOff(addReflectOff(@unsafe.Pointer(n.bytes)));
}

// resolveReflectType adds a *rtype to the reflection lookup map in the runtime.
// It returns a new typeOff that can be used to refer to the pointer.
private static typeOff resolveReflectType(ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return typeOff(addReflectOff(@unsafe.Pointer(t)));
}

// resolveReflectText adds a function pointer to the reflection lookup map in
// the runtime. It returns a new textOff that can be used to refer to the
// pointer.
private static textOff resolveReflectText(unsafe.Pointer ptr) {
    return textOff(addReflectOff(ptr));
}

private partial struct nameOff { // : int
} // offset to a name
private partial struct typeOff { // : int
} // offset to an *rtype
private partial struct textOff { // : int
} // offset from top of text section

private static name nameOff(this ptr<rtype> _addr_t, nameOff off) {
    ref rtype t = ref _addr_t.val;

    return new name((*byte)(resolveNameOff(unsafe.Pointer(t),int32(off))));
}

private static ptr<rtype> typeOff(this ptr<rtype> _addr_t, typeOff off) {
    ref rtype t = ref _addr_t.val;

    return _addr_(rtype.val)(resolveTypeOff(@unsafe.Pointer(t), int32(off)))!;
}

private static unsafe.Pointer textOff(this ptr<rtype> _addr_t, textOff off) {
    ref rtype t = ref _addr_t.val;

    return resolveTextOff(@unsafe.Pointer(t), int32(off));
}

private static ptr<uncommonType> uncommon(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    if (t.tflag & tflagUncommon == 0) {>>MARKER:FUNCTION_addReflectOff_BLOCK_PREFIX<<
        return _addr_null!;
    }

    if (t.Kind() == Struct) 
        return _addr__addr_(structTypeUncommon.val)(@unsafe.Pointer(t)).u!;
    else if (t.Kind() == Ptr) 
        private partial struct u {
            public ref ptrType ptrType => ref ptrType_val;
            public uncommonType u;
        }
        return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
    else if (t.Kind() == Func) 
        private partial struct u {
            public ref ptrType ptrType => ref ptrType_val;
            public uncommonType u;
        }
        return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
    else if (t.Kind() == Slice) 
        private partial struct u {
            public ref ptrType ptrType => ref ptrType_val;
            public uncommonType u;
        }
        return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
    else if (t.Kind() == Array) 
        private partial struct u {
            public ref ptrType ptrType => ref ptrType_val;
            public uncommonType u;
        }
        return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
    else if (t.Kind() == Chan) 
        private partial struct u {
            public ref ptrType ptrType => ref ptrType_val;
            public uncommonType u;
        }
        return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
    else if (t.Kind() == Map) 
        private partial struct u {
            public ref ptrType ptrType => ref ptrType_val;
            public uncommonType u;
        }
        return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
    else if (t.Kind() == Interface) 
        private partial struct u {
            public ref ptrType ptrType => ref ptrType_val;
            public uncommonType u;
        }
        return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
    else 
        private partial struct u {
            public ref ptrType ptrType => ref ptrType_val;
            public uncommonType u;
        }
        return _addr__addr_(u.val)(@unsafe.Pointer(t)).u!;
    }

private static @string String(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    var s = t.nameOff(t.str).name();
    if (t.tflag & tflagExtraStar != 0) {>>MARKER:FUNCTION_resolveTextOff_BLOCK_PREFIX<<
        return s[(int)1..];
    }
    return s;
}

private static System.UIntPtr Size(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return t.size;
}

private static nint Bits(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t == null) {>>MARKER:FUNCTION_resolveTypeOff_BLOCK_PREFIX<<
        panic("reflect: Bits of nil Type");
    }
    var k = t.Kind();
    if (k < Int || k > Complex128) {>>MARKER:FUNCTION_resolveNameOff_BLOCK_PREFIX<<
        panic("reflect: Bits of non-arithmetic Type " + t.String());
    }
    return int(t.size) * 8;
});

private static nint Align(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return int(t.align);
}

private static nint FieldAlign(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return int(t.fieldAlign);
}

private static Kind Kind(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return Kind(t.kind & kindMask);
}

private static bool pointers(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return t.ptrdata != 0;
}

private static ptr<rtype> common(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return _addr_t!;
}

private static slice<method> exportedMethods(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    var ut = t.uncommon();
    if (ut == null) {
        return null;
    }
    return ut.exportedMethods();
}

private static nint NumMethod(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() == Interface) {
        var tt = (interfaceType.val)(@unsafe.Pointer(t));
        return tt.NumMethod();
    }
    return len(t.exportedMethods());
}

private static Method Method(this ptr<rtype> _addr_t, nint i) => func((_, panic, _) => {
    Method m = default;
    ref rtype t = ref _addr_t.val;

    if (t.Kind() == Interface) {
        var tt = (interfaceType.val)(@unsafe.Pointer(t));
        return tt.Method(i);
    }
    var methods = t.exportedMethods();
    if (i < 0 || i >= len(methods)) {
        panic("reflect: Method index out of range");
    }
    var p = methods[i];
    var pname = t.nameOff(p.name);
    m.Name = pname.name();
    var fl = flag(Func);
    var mtyp = t.typeOff(p.mtyp);
    var ft = (funcType.val)(@unsafe.Pointer(mtyp));
    var @in = make_slice<Type>(0, 1 + len(ft.@in()));
    in = append(in, t);
    foreach (var (_, arg) in ft.@in()) {
        in = append(in, arg);
    }    var @out = make_slice<Type>(0, len(ft.@out()));
    foreach (var (_, ret) in ft.@out()) {
        out = append(out, ret);
    }    var mt = FuncOf(in, out, ft.IsVariadic());
    m.Type = mt;
    ref var tfn = ref heap(t.textOff(p.tfn), out ptr<var> _addr_tfn);
    var fn = @unsafe.Pointer(_addr_tfn);
    m.Func = new Value(mt.(*rtype),fn,fl);

    m.Index = i;
    return m;
});

private static (Method, bool) MethodByName(this ptr<rtype> _addr_t, @string name) {
    Method m = default;
    bool ok = default;
    ref rtype t = ref _addr_t.val;

    if (t.Kind() == Interface) {
        var tt = (interfaceType.val)(@unsafe.Pointer(t));
        return tt.MethodByName(name);
    }
    var ut = t.uncommon();
    if (ut == null) {
        return (new Method(), false);
    }
    foreach (var (i, p) in ut.exportedMethods()) {
        if (t.nameOff(p.name).name() == name) {
            return (t.Method(i), true);
        }
    }    return (new Method(), false);
}

private static @string PkgPath(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    if (t.tflag & tflagNamed == 0) {
        return "";
    }
    var ut = t.uncommon();
    if (ut == null) {
        return "";
    }
    return t.nameOff(ut.pkgPath).name();
}

private static bool hasName(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return t.tflag & tflagNamed != 0;
}

private static @string Name(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    if (!t.hasName()) {
        return "";
    }
    var s = t.String();
    var i = len(s) - 1;
    while (i >= 0 && s[i] != '.') {
        i--;
    }
    return s[(int)i + 1..];
}

private static ChanDir ChanDir(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Chan) {
        panic("reflect: ChanDir of non-chan type " + t.String());
    }
    var tt = (chanType.val)(@unsafe.Pointer(t));
    return ChanDir(tt.dir);
});

private static bool IsVariadic(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Func) {
        panic("reflect: IsVariadic of non-func type " + t.String());
    }
    var tt = (funcType.val)(@unsafe.Pointer(t));
    return tt.outCount & (1 << 15) != 0;
});

private static Type Elem(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;


    if (t.Kind() == Array) 
        var tt = (arrayType.val)(@unsafe.Pointer(t));
        return toType(_addr_tt.elem);
    else if (t.Kind() == Chan) 
        tt = (chanType.val)(@unsafe.Pointer(t));
        return toType(_addr_tt.elem);
    else if (t.Kind() == Map) 
        tt = (mapType.val)(@unsafe.Pointer(t));
        return toType(_addr_tt.elem);
    else if (t.Kind() == Ptr) 
        tt = (ptrType.val)(@unsafe.Pointer(t));
        return toType(_addr_tt.elem);
    else if (t.Kind() == Slice) 
        tt = (sliceType.val)(@unsafe.Pointer(t));
        return toType(_addr_tt.elem);
        panic("reflect: Elem of invalid type " + t.String());
});

private static StructField Field(this ptr<rtype> _addr_t, nint i) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Struct) {
        panic("reflect: Field of non-struct type " + t.String());
    }
    var tt = (structType.val)(@unsafe.Pointer(t));
    return tt.Field(i);
});

private static StructField FieldByIndex(this ptr<rtype> _addr_t, slice<nint> index) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Struct) {
        panic("reflect: FieldByIndex of non-struct type " + t.String());
    }
    var tt = (structType.val)(@unsafe.Pointer(t));
    return tt.FieldByIndex(index);
});

private static (StructField, bool) FieldByName(this ptr<rtype> _addr_t, @string name) => func((_, panic, _) => {
    StructField _p0 = default;
    bool _p0 = default;
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Struct) {
        panic("reflect: FieldByName of non-struct type " + t.String());
    }
    var tt = (structType.val)(@unsafe.Pointer(t));
    return tt.FieldByName(name);
});

private static (StructField, bool) FieldByNameFunc(this ptr<rtype> _addr_t, Func<@string, bool> match) => func((_, panic, _) => {
    StructField _p0 = default;
    bool _p0 = default;
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Struct) {
        panic("reflect: FieldByNameFunc of non-struct type " + t.String());
    }
    var tt = (structType.val)(@unsafe.Pointer(t));
    return tt.FieldByNameFunc(match);
});

private static Type In(this ptr<rtype> _addr_t, nint i) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Func) {
        panic("reflect: In of non-func type " + t.String());
    }
    var tt = (funcType.val)(@unsafe.Pointer(t));
    return toType(_addr_tt.@in()[i]);
});

private static Type Key(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Map) {
        panic("reflect: Key of non-map type " + t.String());
    }
    var tt = (mapType.val)(@unsafe.Pointer(t));
    return toType(_addr_tt.key);
});

private static nint Len(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Array) {
        panic("reflect: Len of non-array type " + t.String());
    }
    var tt = (arrayType.val)(@unsafe.Pointer(t));
    return int(tt.len);
});

private static nint NumField(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Struct) {
        panic("reflect: NumField of non-struct type " + t.String());
    }
    var tt = (structType.val)(@unsafe.Pointer(t));
    return len(tt.fields);
});

private static nint NumIn(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Func) {
        panic("reflect: NumIn of non-func type " + t.String());
    }
    var tt = (funcType.val)(@unsafe.Pointer(t));
    return int(tt.inCount);
});

private static nint NumOut(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Func) {
        panic("reflect: NumOut of non-func type " + t.String());
    }
    var tt = (funcType.val)(@unsafe.Pointer(t));
    return len(tt.@out());
});

private static Type Out(this ptr<rtype> _addr_t, nint i) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Func) {
        panic("reflect: Out of non-func type " + t.String());
    }
    var tt = (funcType.val)(@unsafe.Pointer(t));
    return toType(_addr_tt.@out()[i]);
});

private static slice<ptr<rtype>> @in(this ptr<funcType> _addr_t) {
    ref funcType t = ref _addr_t.val;

    var uadd = @unsafe.Sizeof(t.val);
    if (t.tflag & tflagUncommon != 0) {
        uadd += @unsafe.Sizeof(new uncommonType());
    }
    if (t.inCount == 0) {
        return null;
    }
    return new ptr<ptr<array<ptr<rtype>>>>(add(@unsafe.Pointer(t), uadd, "t.inCount > 0")).slice(-1, t.inCount, t.inCount);
}

private static slice<ptr<rtype>> @out(this ptr<funcType> _addr_t) {
    ref funcType t = ref _addr_t.val;

    var uadd = @unsafe.Sizeof(t.val);
    if (t.tflag & tflagUncommon != 0) {
        uadd += @unsafe.Sizeof(new uncommonType());
    }
    var outCount = t.outCount & (1 << 15 - 1);
    if (outCount == 0) {
        return null;
    }
    return new ptr<ptr<array<ptr<rtype>>>>(add(@unsafe.Pointer(t), uadd, "outCount > 0")).slice(t.inCount, t.inCount + outCount, t.inCount + outCount);
}

// add returns p+x.
//
// The whySafe string is ignored, so that the function still inlines
// as efficiently as p+x, but all call sites should use the string to
// record why the addition is safe, which is to say why the addition
// does not cause x to advance to the very end of p's allocation
// and therefore point incorrectly at the next block in memory.
private static unsafe.Pointer add(unsafe.Pointer p, System.UIntPtr x, @string whySafe) {
    return @unsafe.Pointer(uintptr(p) + x);
}

public static @string String(this ChanDir d) {

    if (d == SendDir) 
        return "chan<-";
    else if (d == RecvDir) 
        return "<-chan";
    else if (d == BothDir) 
        return "chan";
        return "ChanDir" + strconv.Itoa(int(d));
}

// Method returns the i'th method in the type's method set.
private static Method Method(this ptr<interfaceType> _addr_t, nint i) {
    Method m = default;
    ref interfaceType t = ref _addr_t.val;

    if (i < 0 || i >= len(t.methods)) {
        return ;
    }
    var p = _addr_t.methods[i];
    var pname = t.nameOff(p.name);
    m.Name = pname.name();
    if (!pname.isExported()) {
        m.PkgPath = pname.pkgPath();
        if (m.PkgPath == "") {
            m.PkgPath = t.pkgPath.name();
        }
    }
    m.Type = toType(_addr_t.typeOff(p.typ));
    m.Index = i;
    return ;
}

// NumMethod returns the number of interface methods in the type's method set.
private static nint NumMethod(this ptr<interfaceType> _addr_t) {
    ref interfaceType t = ref _addr_t.val;

    return len(t.methods);
}

// MethodByName method with the given name in the type's method set.
private static (Method, bool) MethodByName(this ptr<interfaceType> _addr_t, @string name) {
    Method m = default;
    bool ok = default;
    ref interfaceType t = ref _addr_t.val;

    if (t == null) {
        return ;
    }
    ptr<imethod> p;
    foreach (var (i) in t.methods) {
        p = _addr_t.methods[i];
        if (t.nameOff(p.name).name() == name) {
            return (t.Method(i), true);
        }
    }    return ;
}

// A StructField describes a single field in a struct.
public partial struct StructField {
    public @string Name; // PkgPath is the package path that qualifies a lower case (unexported)
// field name. It is empty for upper case (exported) field names.
// See https://golang.org/ref/spec#Uniqueness_of_identifiers
    public @string PkgPath;
    public Type Type; // field type
    public StructTag Tag; // field tag string
    public System.UIntPtr Offset; // offset within struct, in bytes
    public slice<nint> Index; // index sequence for Type.FieldByIndex
    public bool Anonymous; // is an embedded field
}

// IsExported reports whether the field is exported.
public static bool IsExported(this StructField f) {
    return f.PkgPath == "";
}

// A StructTag is the tag string in a struct field.
//
// By convention, tag strings are a concatenation of
// optionally space-separated key:"value" pairs.
// Each key is a non-empty string consisting of non-control
// characters other than space (U+0020 ' '), quote (U+0022 '"'),
// and colon (U+003A ':').  Each value is quoted using U+0022 '"'
// characters and Go string literal syntax.
public partial struct StructTag { // : @string
}

// Get returns the value associated with key in the tag string.
// If there is no such key in the tag, Get returns the empty string.
// If the tag does not have the conventional format, the value
// returned by Get is unspecified. To determine whether a tag is
// explicitly set to the empty string, use Lookup.
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
public static (@string, bool) Lookup(this StructTag tag, @string key) {
    @string value = default;
    bool ok = default;
 
    // When modifying this code, also update the validateStructTag code
    // in cmd/vet/structtag.go.

    while (tag != "") { 
        // Skip leading space.
        nint i = 0;
        while (i < len(tag) && tag[i] == ' ') {
            i++;
        }
        tag = tag[(int)i..];
        if (tag == "") {
            break;
        }
        i = 0;
        while (i < len(tag) && tag[i] > ' ' && tag[i] != ':' && tag[i] != '"' && tag[i] != 0x7f) {
            i++;
        }
        if (i == 0 || i + 1 >= len(tag) || tag[i] != ':' || tag[i + 1] != '"') {
            break;
        }
        var name = string(tag[..(int)i]);
        tag = tag[(int)i + 1..]; 

        // Scan quoted string to find value.
        i = 1;
        while (i < len(tag) && tag[i] != '"') {
            if (tag[i] == '\\') {
                i++;
            }
            i++;
        }
        if (i >= len(tag)) {
            break;
        }
        var qvalue = string(tag[..(int)i + 1]);
        tag = tag[(int)i + 1..];

        if (key == name) {
            var (value, err) = strconv.Unquote(qvalue);
            if (err != null) {
                break;
            }
            return (value, true);
        }
    }
    return ("", false);
}

// Field returns the i'th struct field.
private static StructField Field(this ptr<structType> _addr_t, nint i) => func((_, panic, _) => {
    StructField f = default;
    ref structType t = ref _addr_t.val;

    if (i < 0 || i >= len(t.fields)) {
        panic("reflect: Field index out of bounds");
    }
    var p = _addr_t.fields[i];
    f.Type = toType(_addr_p.typ);
    f.Name = p.name.name();
    f.Anonymous = p.embedded();
    if (!p.name.isExported()) {
        f.PkgPath = t.pkgPath.name();
    }
    {
        var tag = p.name.tag();

        if (tag != "") {
            f.Tag = StructTag(tag);
        }
    }
    f.Offset = p.offset(); 

    // NOTE(rsc): This is the only allocation in the interface
    // presented by a reflect.Type. It would be nice to avoid,
    // at least in the common cases, but we need to make sure
    // that misbehaving clients of reflect cannot affect other
    // uses of reflect. One possibility is CL 5371098, but we
    // postponed that ugliness until there is a demonstrated
    // need for the performance. This is issue 2320.
    f.Index = new slice<nint>(new nint[] { i });
    return ;
});

// TODO(gri): Should there be an error/bool indicator if the index
//            is wrong for FieldByIndex?

// FieldByIndex returns the nested field corresponding to index.
private static StructField FieldByIndex(this ptr<structType> _addr_t, slice<nint> index) {
    StructField f = default;
    ref structType t = ref _addr_t.val;

    f.Type = toType(_addr_t.rtype);
    foreach (var (i, x) in index) {
        if (i > 0) {
            var ft = f.Type;
            if (ft.Kind() == Ptr && ft.Elem().Kind() == Struct) {
                ft = ft.Elem();
            }
            f.Type = ft;
        }
        f = f.Type.Field(x);
    }    return ;
}

// A fieldScan represents an item on the fieldByNameFunc scan work list.
private partial struct fieldScan {
    public ptr<structType> typ;
    public slice<nint> index;
}

// FieldByNameFunc returns the struct field with a name that satisfies the
// match function and a boolean to indicate if the field was found.
private static (StructField, bool) FieldByNameFunc(this ptr<structType> _addr_t, Func<@string, bool> match) {
    StructField result = default;
    bool ok = default;
    ref structType t = ref _addr_t.val;
 
    // This uses the same condition that the Go language does: there must be a unique instance
    // of the match at a given depth level. If there are multiple instances of a match at the
    // same depth, they annihilate each other and inhibit any possible match at a lower level.
    // The algorithm is breadth first search, one depth level at a time.

    // The current and next slices are work queues:
    // current lists the fields to visit on this depth level,
    // and next lists the fields on the next lower level.
    fieldScan current = new slice<fieldScan>(new fieldScan[] {  });
    fieldScan next = new slice<fieldScan>(new fieldScan[] { {typ:t} }); 

    // nextCount records the number of times an embedded type has been
    // encountered and considered for queueing in the 'next' slice.
    // We only queue the first one, but we increment the count on each.
    // If a struct type T can be reached more than once at a given depth level,
    // then it annihilates itself and need not be considered at all when we
    // process that next depth level.
    map<ptr<structType>, nint> nextCount = default; 

    // visited records the structs that have been considered already.
    // Embedded pointer fields can create cycles in the graph of
    // reachable embedded types; visited avoids following those cycles.
    // It also avoids duplicated effort: if we didn't find the field in an
    // embedded type T at level 2, we won't find it in one at level 4 either.
    map visited = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<structType>, bool>{};

    while (len(next) > 0) {
        (current, next) = (next, current[..(int)0]);        var count = nextCount;
        nextCount = null; 

        // Process all the fields at this depth, now listed in 'current'.
        // The loop queues embedded fields found in 'next', for processing during the next
        // iteration. The multiplicity of the 'current' field counts is recorded
        // in 'count'; the multiplicity of the 'next' field counts is recorded in 'nextCount'.
        foreach (var (_, scan) in current) {
            var t = scan.typ;
            if (visited[t]) { 
                // We've looked through this type before, at a higher level.
                // That higher level would shadow the lower level we're now at,
                // so this one can't be useful to us. Ignore it.
                continue;
            }
            visited[t] = true;
            foreach (var (i) in t.fields) {
                var f = _addr_t.fields[i]; 
                // Find name and (for embedded field) type for field f.
                var fname = f.name.name();
                ptr<rtype> ntyp;
                if (f.embedded()) { 
                    // Embedded field of type T or *T.
                    ntyp = f.typ;
                    if (ntyp.Kind() == Ptr) {
                        ntyp = ntyp.Elem().common();
                    }
                } 

                // Does it match?
                if (match(fname)) { 
                    // Potential match
                    if (count[t] > 1 || ok) { 
                        // Name appeared multiple times at this level: annihilate.
                        return (new StructField(), false);
                    }
                    result = t.Field(i);
                    result.Index = null;
                    result.Index = append(result.Index, scan.index);
                    result.Index = append(result.Index, i);
                    ok = true;
                    continue;
                } 

                // Queue embedded struct fields for processing with next level,
                // but only if we haven't seen a match yet at this level and only
                // if the embedded types haven't already been queued.
                if (ok || ntyp == null || ntyp.Kind() != Struct) {
                    continue;
                }
                var styp = (structType.val)(@unsafe.Pointer(ntyp));
                if (nextCount[styp] > 0) {
                    nextCount[styp] = 2; // exact multiple doesn't matter
                    continue;
                }
                if (nextCount == null) {
                    nextCount = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<structType>, nint>{};
                }
                nextCount[styp] = 1;
                if (count[t] > 1) {
                    nextCount[styp] = 2; // exact multiple doesn't matter
                }
                slice<nint> index = default;
                index = append(index, scan.index);
                index = append(index, i);
                next = append(next, new fieldScan(styp,index));
            }
        }        if (ok) {
            break;
        }
    }
    return ;
}

// FieldByName returns the struct field with the given name
// and a boolean to indicate if the field was found.
private static (StructField, bool) FieldByName(this ptr<structType> _addr_t, @string name) {
    StructField f = default;
    bool present = default;
    ref structType t = ref _addr_t.val;
 
    // Quick check for top-level name, or struct without embedded fields.
    var hasEmbeds = false;
    if (name != "") {
        foreach (var (i) in t.fields) {
            var tf = _addr_t.fields[i];
            if (tf.name.name() == name) {
                return (t.Field(i), true);
            }
            if (tf.embedded()) {
                hasEmbeds = true;
            }
        }
    }
    if (!hasEmbeds) {
        return ;
    }
    return t.FieldByNameFunc(s => s == name);
}

// TypeOf returns the reflection Type that represents the dynamic type of i.
// If i is a nil interface value, TypeOf returns nil.
public static Type TypeOf(object i) {
    ptr<ptr<emptyInterface>> eface = new ptr<ptr<ptr<emptyInterface>>>(@unsafe.Pointer(_addr_i));
    return toType(_addr_eface.typ);
}

// ptrMap is the cache for PtrTo.
private static sync.Map ptrMap = default; // map[*rtype]*ptrType

// PtrTo returns the pointer type with element t.
// For example, if t represents type Foo, PtrTo(t) represents *Foo.
public static Type PtrTo(Type t) {
    return t._<ptr<rtype>>().ptrTo();
}

private static ptr<rtype> ptrTo(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    if (t.ptrToThis != 0) {
        return _addr_t.typeOff(t.ptrToThis)!;
    }
    {
        var pi__prev1 = pi;

        var (pi, ok) = ptrMap.Load(t);

        if (ok) {
            return _addr_pi._<ptr<ptrType>>().rtype;
        }
        pi = pi__prev1;

    } 

    // Look in known types.
    @string s = "*" + t.String();
    foreach (var (_, tt) in typesByString(s)) {
        var p = (ptrType.val)(@unsafe.Pointer(tt));
        if (p.elem != t) {
            continue;
        }
        var (pi, _) = ptrMap.LoadOrStore(t, p);
        return _addr_pi._<ptr<ptrType>>().rtype;
    }    ref var iptr = ref heap((@unsafe.Pointer.val)(null), out ptr<var> _addr_iptr);
    ptr<ptr<ptr<ptrType>>> prototype = new ptr<ptr<ptr<ptr<ptrType>>>>(@unsafe.Pointer(_addr_iptr));
    ref var pp = ref heap(prototype.val, out ptr<var> _addr_pp);

    pp.str = resolveReflectName(newName(s, "", false));
    pp.ptrToThis = 0; 

    // For the type structures linked into the binary, the
    // compiler provides a good hash of the string.
    // Create a good hash for the new string by using
    // the FNV-1 hash's mixing function to combine the
    // old hash and the new "*".
    pp.hash = fnv1(t.hash, '*');

    pp.elem = t;

    (pi, _) = ptrMap.LoadOrStore(t, _addr_pp);
    return _addr_pi._<ptr<ptrType>>().rtype;
}

// fnv1 incorporates the list of bytes into the hash x using the FNV-1 hash function.
private static uint fnv1(uint x, params byte[] list) {
    list = list.Clone();

    foreach (var (_, b) in list) {
        x = x * 16777619 ^ uint32(b);
    }    return x;
}

private static bool Implements(this ptr<rtype> _addr_t, Type u) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (u == null) {
        panic("reflect: nil type passed to Type.Implements");
    }
    if (u.Kind() != Interface) {
        panic("reflect: non-interface type passed to Type.Implements");
    }
    return implements(u._<ptr<rtype>>(), _addr_t);
});

private static bool AssignableTo(this ptr<rtype> _addr_t, Type u) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (u == null) {
        panic("reflect: nil type passed to Type.AssignableTo");
    }
    ptr<rtype> uu = u._<ptr<rtype>>();
    return directlyAssignable(uu, _addr_t) || implements(uu, _addr_t);
});

private static bool ConvertibleTo(this ptr<rtype> _addr_t, Type u) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (u == null) {
        panic("reflect: nil type passed to Type.ConvertibleTo");
    }
    ptr<rtype> uu = u._<ptr<rtype>>();
    return convertOp(uu, t) != null;
});

private static bool Comparable(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return t.equal != null;
}

// implements reports whether the type V implements the interface type T.
private static bool implements(ptr<rtype> _addr_T, ptr<rtype> _addr_V) {
    ref rtype T = ref _addr_T.val;
    ref rtype V = ref _addr_V.val;

    if (T.Kind() != Interface) {
        return false;
    }
    var t = (interfaceType.val)(@unsafe.Pointer(T));
    if (len(t.methods) == 0) {
        return true;
    }
    if (V.Kind() == Interface) {
        var v = (interfaceType.val)(@unsafe.Pointer(V));
        nint i = 0;
        {
            nint j__prev1 = j;

            for (nint j = 0; j < len(v.methods); j++) {
                var tm = _addr_t.methods[i];
                var tmName = t.nameOff(tm.name);
                var vm = _addr_v.methods[j];
                var vmName = V.nameOff(vm.name);
                if (vmName.name() == tmName.name() && V.typeOff(vm.typ) == t.typeOff(tm.typ)) {
                    if (!tmName.isExported()) {
                        var tmPkgPath = tmName.pkgPath();
                        if (tmPkgPath == "") {
                            tmPkgPath = t.pkgPath.name();
                        }
                        var vmPkgPath = vmName.pkgPath();
                        if (vmPkgPath == "") {
                            vmPkgPath = v.pkgPath.name();
                        }
                        if (tmPkgPath != vmPkgPath) {
                            continue;
                        }
                    }
                    i++;

                    if (i >= len(t.methods)) {
                        return true;
                    }
                }
            }


            j = j__prev1;
        }
        return false;
    }
    v = V.uncommon();
    if (v == null) {
        return false;
    }
    i = 0;
    var vmethods = v.methods();
    {
        nint j__prev1 = j;

        for (j = 0; j < int(v.mcount); j++) {
            tm = _addr_t.methods[i];
            tmName = t.nameOff(tm.name);
            vm = vmethods[j];
            vmName = V.nameOff(vm.name);
            if (vmName.name() == tmName.name() && V.typeOff(vm.mtyp) == t.typeOff(tm.typ)) {
                if (!tmName.isExported()) {
                    tmPkgPath = tmName.pkgPath();
                    if (tmPkgPath == "") {
                        tmPkgPath = t.pkgPath.name();
                    }
                    vmPkgPath = vmName.pkgPath();
                    if (vmPkgPath == "") {
                        vmPkgPath = V.nameOff(v.pkgPath).name();
                    }
                    if (tmPkgPath != vmPkgPath) {
                        continue;
                    }
                }
                i++;

                if (i >= len(t.methods)) {
                    return true;
                }
            }
        }

        j = j__prev1;
    }
    return false;
}

// specialChannelAssignability reports whether a value x of channel type V
// can be directly assigned (using memmove) to another channel type T.
// https://golang.org/doc/go_spec.html#Assignability
// T and V must be both of Chan kind.
private static bool specialChannelAssignability(ptr<rtype> _addr_T, ptr<rtype> _addr_V) {
    ref rtype T = ref _addr_T.val;
    ref rtype V = ref _addr_V.val;
 
    // Special case:
    // x is a bidirectional channel value, T is a channel type,
    // x's type V and T have identical element types,
    // and at least one of V or T is not a defined type.
    return V.ChanDir() == BothDir && (T.Name() == "" || V.Name() == "") && haveIdenticalType(T.Elem(), V.Elem(), true);
}

// directlyAssignable reports whether a value x of type V can be directly
// assigned (using memmove) to a value of type T.
// https://golang.org/doc/go_spec.html#Assignability
// Ignoring the interface rules (implemented elsewhere)
// and the ideal constant rules (no ideal constants at run time).
private static bool directlyAssignable(ptr<rtype> _addr_T, ptr<rtype> _addr_V) {
    ref rtype T = ref _addr_T.val;
    ref rtype V = ref _addr_V.val;
 
    // x's type V is identical to T?
    if (T == V) {
        return true;
    }
    if (T.hasName() && V.hasName() || T.Kind() != V.Kind()) {
        return false;
    }
    if (T.Kind() == Chan && specialChannelAssignability(_addr_T, _addr_V)) {
        return true;
    }
    return haveIdenticalUnderlyingType(_addr_T, _addr_V, true);
}

private static bool haveIdenticalType(Type T, Type V, bool cmpTags) {
    if (cmpTags) {
        return T == V;
    }
    if (T.Name() != V.Name() || T.Kind() != V.Kind() || T.PkgPath() != V.PkgPath()) {
        return false;
    }
    return haveIdenticalUnderlyingType(_addr_T.common(), _addr_V.common(), false);
}

private static bool haveIdenticalUnderlyingType(ptr<rtype> _addr_T, ptr<rtype> _addr_V, bool cmpTags) {
    ref rtype T = ref _addr_T.val;
    ref rtype V = ref _addr_V.val;

    if (T == V) {
        return true;
    }
    var kind = T.Kind();
    if (kind != V.Kind()) {
        return false;
    }
    if (Bool <= kind && kind <= Complex128 || kind == String || kind == UnsafePointer) {
        return true;
    }

    if (kind == Array) 
        return T.Len() == V.Len() && haveIdenticalType(T.Elem(), V.Elem(), cmpTags);
    else if (kind == Chan) 
        return V.ChanDir() == T.ChanDir() && haveIdenticalType(T.Elem(), V.Elem(), cmpTags);
    else if (kind == Func) 
        var t = (funcType.val)(@unsafe.Pointer(T));
        var v = (funcType.val)(@unsafe.Pointer(V));
        if (t.outCount != v.outCount || t.inCount != v.inCount) {
            return false;
        }
        {
            nint i__prev1 = i;

            for (nint i = 0; i < t.NumIn(); i++) {
                if (!haveIdenticalType(t.In(i), v.In(i), cmpTags)) {
                    return false;
                }
            }


            i = i__prev1;
        }
        {
            nint i__prev1 = i;

            for (i = 0; i < t.NumOut(); i++) {
                if (!haveIdenticalType(t.Out(i), v.Out(i), cmpTags)) {
                    return false;
                }
            }


            i = i__prev1;
        }
        return true;
    else if (kind == Interface) 
        t = (interfaceType.val)(@unsafe.Pointer(T));
        v = (interfaceType.val)(@unsafe.Pointer(V));
        if (len(t.methods) == 0 && len(v.methods) == 0) {
            return true;
        }
        return false;
    else if (kind == Map) 
        return haveIdenticalType(T.Key(), V.Key(), cmpTags) && haveIdenticalType(T.Elem(), V.Elem(), cmpTags);
    else if (kind == Ptr || kind == Slice) 
        return haveIdenticalType(T.Elem(), V.Elem(), cmpTags);
    else if (kind == Struct) 
        t = (structType.val)(@unsafe.Pointer(T));
        v = (structType.val)(@unsafe.Pointer(V));
        if (len(t.fields) != len(v.fields)) {
            return false;
        }
        if (t.pkgPath.name() != v.pkgPath.name()) {
            return false;
        }
        {
            nint i__prev1 = i;

            foreach (var (__i) in t.fields) {
                i = __i;
                var tf = _addr_t.fields[i];
                var vf = _addr_v.fields[i];
                if (tf.name.name() != vf.name.name()) {
                    return false;
                }
                if (!haveIdenticalType(tf.typ, vf.typ, cmpTags)) {
                    return false;
                }
                if (cmpTags && tf.name.tag() != vf.name.tag()) {
                    return false;
                }
                if (tf.offsetEmbed != vf.offsetEmbed) {
                    return false;
                }
            }

            i = i__prev1;
        }

        return true;
        return false;
}

// typelinks is implemented in package runtime.
// It returns a slice of the sections in each module,
// and a slice of *rtype offsets in each module.
//
// The types in each module are sorted by string. That is, the first
// two linked types of the first module are:
//
//    d0 := sections[0]
//    t1 := (*rtype)(add(d0, offset[0][0]))
//    t2 := (*rtype)(add(d0, offset[0][1]))
//
// and
//
//    t1.String() < t2.String()
//
// Note that strings are not unique identifiers for types:
// there can be more than one with a given string.
// Only types we might want to look up are included:
// pointers, channels, maps, slices, and arrays.
private static (slice<unsafe.Pointer>, slice<slice<int>>) typelinks();

private static ptr<rtype> rtypeOff(unsafe.Pointer section, int off) {
    return _addr_(rtype.val)(add(section, uintptr(off), "sizeof(rtype) > 0"))!;
}

// typesByString returns the subslice of typelinks() whose elements have
// the given string representation.
// It may be empty (no known types with that string) or may have
// multiple elements (multiple types with that string).
private static slice<ptr<rtype>> typesByString(@string s) {
    var (sections, offset) = typelinks();
    slice<ptr<rtype>> ret = default;

    foreach (var (offsI, offs) in offset) {
        var section = sections[offsI]; 

        // We are looking for the first index i where the string becomes >= s.
        // This is a copy of sort.Search, with f(h) replaced by (*typ[h].String() >= s).
        nint i = 0;
        var j = len(offs);
        while (i < j) {>>MARKER:FUNCTION_typelinks_BLOCK_PREFIX<<
            var h = i + (j - i) >> 1; // avoid overflow when computing h
            // i ≤ h < j
            if (!(rtypeOff(section, offs[h]).String() >= s)) {
                i = h + 1; // preserves f(i-1) == false
            }
            else
 {
                j = h; // preserves f(j) == true
            }
        } 
        // i == j, f(i-1) == false, and f(j) (= f(i)) == true  =>  answer is i.

        // Having found the first, linear scan forward to find the last.
        // We could do a second binary search, but the caller is going
        // to do a linear scan anyway.
        {
            var j__prev2 = j;

            for (j = i; j < len(offs); j++) {
                var typ = rtypeOff(section, offs[j]);
                if (typ.String() != s) {
                    break;
                }
                ret = append(ret, typ);
            }


            j = j__prev2;
        }
    }    return ret;
}

// The lookupCache caches ArrayOf, ChanOf, MapOf and SliceOf lookups.
private static sync.Map lookupCache = default; // map[cacheKey]*rtype

// A cacheKey is the key for use in the lookupCache.
// Four values describe any of the types we are looking for:
// type kind, one or two subtypes, and an extra integer.
private partial struct cacheKey {
    public Kind kind;
    public ptr<rtype> t1;
    public ptr<rtype> t2;
    public System.UIntPtr extra;
}

// The funcLookupCache caches FuncOf lookups.
// FuncOf does not share the common lookupCache since cacheKey is not
// sufficient to represent functions unambiguously.
private static var funcLookupCache = default;

// ChanOf returns the channel type with the given direction and element type.
// For example, if t represents int, ChanOf(RecvDir, t) represents <-chan int.
//
// The gc runtime imposes a limit of 64 kB on channel element types.
// If t's size is equal to or exceeds this limit, ChanOf panics.
public static Type ChanOf(ChanDir dir, Type t) => func((_, panic, _) => {
    ptr<rtype> typ = t._<ptr<rtype>>(); 

    // Look in cache.
    cacheKey ckey = new cacheKey(Chan,typ,nil,uintptr(dir));
    {
        var ch__prev1 = ch;

        var (ch, ok) = lookupCache.Load(ckey);

        if (ok) {
            return ch._<ptr<rtype>>();
        }
        ch = ch__prev1;

    } 

    // This restriction is imposed by the gc compiler and the runtime.
    if (typ.size >= 1 << 16) {
        panic("reflect.ChanOf: element size too large");
    }
    @string s = default;

    if (dir == SendDir) 
        s = "chan<- " + typ.String();
    else if (dir == RecvDir) 
        s = "<-chan " + typ.String();
    else if (dir == BothDir) 
        var typeStr = typ.String();
        if (typeStr[0] == '<') { 
            // typ is recv chan, need parentheses as "<-" associates with leftmost
            // chan possible, see:
            // * https://golang.org/ref/spec#Channel_types
            // * https://github.com/golang/go/issues/39897
            s = "chan (" + typeStr + ")";
        }
        else
 {
            s = "chan " + typeStr;
        }
    else 
        panic("reflect.ChanOf: invalid dir");
        foreach (var (_, tt) in typesByString(s)) {
        var ch = (chanType.val)(@unsafe.Pointer(tt));
        if (ch.elem == typ && ch.dir == uintptr(dir)) {
            var (ti, _) = lookupCache.LoadOrStore(ckey, tt);
            return ti._<Type>();
        }
    }    ref channel<unsafe.Pointer> ichan = ref heap((channel<unsafe.Pointer>)null, out ptr<channel<unsafe.Pointer>> _addr_ichan);
    ptr<ptr<ptr<chanType>>> prototype = new ptr<ptr<ptr<ptr<chanType>>>>(@unsafe.Pointer(_addr_ichan));
    ch = prototype.val;
    ch.tflag = tflagRegularMemory;
    ch.dir = uintptr(dir);
    ch.str = resolveReflectName(newName(s, "", false));
    ch.hash = fnv1(typ.hash, 'c', byte(dir));
    ch.elem = typ;

    (ti, _) = lookupCache.LoadOrStore(ckey, _addr_ch.rtype);
    return ti._<Type>();
});

// MapOf returns the map type with the given key and element types.
// For example, if k represents int and e represents string,
// MapOf(k, e) represents map[int]string.
//
// If the key type is not a valid map key type (that is, if it does
// not implement Go's == operator), MapOf panics.
public static Type MapOf(Type key, Type elem) => func((_, panic, _) => {
    ptr<rtype> ktyp = key._<ptr<rtype>>();
    ptr<rtype> etyp = elem._<ptr<rtype>>();

    if (ktyp.equal == null) {
        panic("reflect.MapOf: invalid key type " + ktyp.String());
    }
    cacheKey ckey = new cacheKey(Map,ktyp,etyp,0);
    {
        var mt__prev1 = mt;

        var (mt, ok) = lookupCache.Load(ckey);

        if (ok) {
            return mt._<Type>();
        }
        mt = mt__prev1;

    } 

    // Look in known types.
    @string s = "map[" + ktyp.String() + "]" + etyp.String();
    foreach (var (_, tt) in typesByString(s)) {
        var mt = (mapType.val)(@unsafe.Pointer(tt));
        if (mt.key == ktyp && mt.elem == etyp) {
            var (ti, _) = lookupCache.LoadOrStore(ckey, tt);
            return ti._<Type>();
        }
    }    ref map<unsafe.Pointer, unsafe.Pointer> imap = ref heap((map<unsafe.Pointer, unsafe.Pointer>)null, out ptr<map<unsafe.Pointer, unsafe.Pointer>> _addr_imap);
    mt = new ptr<ptr<ptr<ptr<ptr<mapType>>>>>(@unsafe.Pointer(_addr_imap));
    mt.str = resolveReflectName(newName(s, "", false));
    mt.tflag = 0;
    mt.hash = fnv1(etyp.hash, 'm', byte(ktyp.hash >> 24), byte(ktyp.hash >> 16), byte(ktyp.hash >> 8), byte(ktyp.hash));
    mt.key = ktyp;
    mt.elem = etyp;
    mt.bucket = bucketOf(ktyp, etyp);
    mt.hasher = (p, seed) => typehash(ktyp, p, seed);
    mt.flags = 0;
    if (ktyp.size > maxKeySize) {
        mt.keysize = uint8(ptrSize);
        mt.flags |= 1; // indirect key
    }
    else
 {
        mt.keysize = uint8(ktyp.size);
    }
    if (etyp.size > maxValSize) {
        mt.valuesize = uint8(ptrSize);
        mt.flags |= 2; // indirect value
    }
    else
 {
        mt.valuesize = uint8(etyp.size);
    }
    mt.bucketsize = uint16(mt.bucket.size);
    if (isReflexive(ktyp)) {
        mt.flags |= 4;
    }
    if (needKeyUpdate(ktyp)) {
        mt.flags |= 8;
    }
    if (hashMightPanic(ktyp)) {
        mt.flags |= 16;
    }
    mt.ptrToThis = 0;

    (ti, _) = lookupCache.LoadOrStore(ckey, _addr_mt.rtype);
    return ti._<Type>();
});

// TODO(crawshaw): as these funcTypeFixedN structs have no methods,
// they could be defined at runtime using the StructOf function.
private partial struct funcTypeFixed4 {
    public ref funcType funcType => ref funcType_val;
    public array<ptr<rtype>> args;
}
private partial struct funcTypeFixed8 {
    public ref funcType funcType => ref funcType_val;
    public array<ptr<rtype>> args;
}
private partial struct funcTypeFixed16 {
    public ref funcType funcType => ref funcType_val;
    public array<ptr<rtype>> args;
}
private partial struct funcTypeFixed32 {
    public ref funcType funcType => ref funcType_val;
    public array<ptr<rtype>> args;
}
private partial struct funcTypeFixed64 {
    public ref funcType funcType => ref funcType_val;
    public array<ptr<rtype>> args;
}
private partial struct funcTypeFixed128 {
    public ref funcType funcType => ref funcType_val;
    public array<ptr<rtype>> args;
}

// FuncOf returns the function type with the given argument and result types.
// For example if k represents int and e represents string,
// FuncOf([]Type{k}, []Type{e}, false) represents func(int) string.
//
// The variadic argument controls whether the function is variadic. FuncOf
// panics if the in[len(in)-1] does not represent a slice and variadic is
// true.
public static Type FuncOf(slice<Type> @in, slice<Type> @out, bool variadic) => func((defer, panic, _) => {
    if (variadic && (len(in) == 0 || in[len(in) - 1].Kind() != Slice)) {
        panic("reflect.FuncOf: last arg of variadic func must be slice");
    }
    ref Action ifunc = ref heap((Action)null, out ptr<Action> _addr_ifunc);
    ptr<ptr<ptr<funcType>>> prototype = new ptr<ptr<ptr<ptr<funcType>>>>(@unsafe.Pointer(_addr_ifunc));
    var n = len(in) + len(out);

    ptr<funcType> ft;
    slice<ptr<rtype>> args = default;

    if (n <= 4) 
        ptr<funcTypeFixed4> @fixed = @new<funcTypeFixed4>();
        args = @fixed.args.slice(-1, 0, len(@fixed.args));
        ft = _addr_@fixed.funcType;
    else if (n <= 8) 
        @fixed = @new<funcTypeFixed8>();
        args = @fixed.args.slice(-1, 0, len(@fixed.args));
        ft = _addr_@fixed.funcType;
    else if (n <= 16) 
        @fixed = @new<funcTypeFixed16>();
        args = @fixed.args.slice(-1, 0, len(@fixed.args));
        ft = _addr_@fixed.funcType;
    else if (n <= 32) 
        @fixed = @new<funcTypeFixed32>();
        args = @fixed.args.slice(-1, 0, len(@fixed.args));
        ft = _addr_@fixed.funcType;
    else if (n <= 64) 
        @fixed = @new<funcTypeFixed64>();
        args = @fixed.args.slice(-1, 0, len(@fixed.args));
        ft = _addr_@fixed.funcType;
    else if (n <= 128) 
        @fixed = @new<funcTypeFixed128>();
        args = @fixed.args.slice(-1, 0, len(@fixed.args));
        ft = _addr_@fixed.funcType;
    else 
        panic("reflect.FuncOf: too many arguments");
        ft.val = prototype.val; 

    // Build a hash and minimally populate ft.
    uint hash = default;
    foreach (var (_, in) in in) {
        ptr<rtype> t = in._<ptr<rtype>>();
        args = append(args, t);
        hash = fnv1(hash, byte(t.hash >> 24), byte(t.hash >> 16), byte(t.hash >> 8), byte(t.hash));
    }    if (variadic) {
        hash = fnv1(hash, 'v');
    }
    hash = fnv1(hash, '.');
    foreach (var (_, out) in out) {
        t = out._<ptr<rtype>>();
        args = append(args, t);
        hash = fnv1(hash, byte(t.hash >> 24), byte(t.hash >> 16), byte(t.hash >> 8), byte(t.hash));
    }    if (len(args) > 50) {
        panic("reflect.FuncOf does not support more than 50 arguments");
    }
    ft.tflag = 0;
    ft.hash = hash;
    ft.inCount = uint16(len(in));
    ft.outCount = uint16(len(out));
    if (variadic) {
        ft.outCount |= 1 << 15;
    }
    {
        var ts__prev1 = ts;

        var (ts, ok) = funcLookupCache.m.Load(hash);

        if (ok) {
            {
                ptr<rtype> t__prev1 = t;

                foreach (var (_, __t) in ts._<slice<ptr<rtype>>>()) {
                    t = __t;
                    if (haveIdenticalUnderlyingType(_addr_ft.rtype, t, true)) {
                        return t;
                    }
                }

                t = t__prev1;
            }
        }
        ts = ts__prev1;

    } 

    // Not in cache, lock and retry.
    funcLookupCache.Lock();
    defer(funcLookupCache.Unlock());
    {
        var ts__prev1 = ts;

        (ts, ok) = funcLookupCache.m.Load(hash);

        if (ok) {
            {
                ptr<rtype> t__prev1 = t;

                foreach (var (_, __t) in ts._<slice<ptr<rtype>>>()) {
                    t = __t;
                    if (haveIdenticalUnderlyingType(_addr_ft.rtype, t, true)) {
                        return t;
                    }
                }

                t = t__prev1;
            }
        }
        ts = ts__prev1;

    }

    Func<ptr<rtype>, Type> addToCache = tt => {
        slice<ptr<rtype>> rts = default;
        {
            var (rti, ok) = funcLookupCache.m.Load(hash);

            if (ok) {
                rts = rti._<slice<ptr<rtype>>>();
            }

        }
        funcLookupCache.m.Store(hash, append(rts, tt));
        return tt;
    }; 

    // Look in known types for the same string representation.
    var str = funcStr(ft);
    foreach (var (_, tt) in typesByString(str)) {
        if (haveIdenticalUnderlyingType(_addr_ft.rtype, _addr_tt, true)) {
            return addToCache(tt);
        }
    }    ft.str = resolveReflectName(newName(str, "", false));
    ft.ptrToThis = 0;
    return addToCache(_addr_ft.rtype);
});

// funcStr builds a string representation of a funcType.
private static @string funcStr(ptr<funcType> _addr_ft) {
    ref funcType ft = ref _addr_ft.val;

    var repr = make_slice<byte>(0, 64);
    repr = append(repr, "func(");
    {
        var i__prev1 = i;
        var t__prev1 = t;

        foreach (var (__i, __t) in ft.@in()) {
            i = __i;
            t = __t;
            if (i > 0) {
                repr = append(repr, ", ");
            }
            if (ft.IsVariadic() && i == int(ft.inCount) - 1) {
                repr = append(repr, "...");
                repr = append(repr, (sliceType.val)(@unsafe.Pointer(t)).elem.String());
            }
            else
 {
                repr = append(repr, t.String());
            }
        }
        i = i__prev1;
        t = t__prev1;
    }

    repr = append(repr, ')');
    var @out = ft.@out();
    if (len(out) == 1) {
        repr = append(repr, ' ');
    }
    else if (len(out) > 1) {
        repr = append(repr, " (");
    }
    {
        var i__prev1 = i;
        var t__prev1 = t;

        foreach (var (__i, __t) in out) {
            i = __i;
            t = __t;
            if (i > 0) {
                repr = append(repr, ", ");
            }
            repr = append(repr, t.String());
        }
        i = i__prev1;
        t = t__prev1;
    }

    if (len(out) > 1) {
        repr = append(repr, ')');
    }
    return string(repr);
}

// isReflexive reports whether the == operation on the type is reflexive.
// That is, x == x for all values x of type t.
private static bool isReflexive(ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;


    if (t.Kind() == Bool || t.Kind() == Int || t.Kind() == Int8 || t.Kind() == Int16 || t.Kind() == Int32 || t.Kind() == Int64 || t.Kind() == Uint || t.Kind() == Uint8 || t.Kind() == Uint16 || t.Kind() == Uint32 || t.Kind() == Uint64 || t.Kind() == Uintptr || t.Kind() == Chan || t.Kind() == Ptr || t.Kind() == String || t.Kind() == UnsafePointer) 
        return true;
    else if (t.Kind() == Float32 || t.Kind() == Float64 || t.Kind() == Complex64 || t.Kind() == Complex128 || t.Kind() == Interface) 
        return false;
    else if (t.Kind() == Array) 
        var tt = (arrayType.val)(@unsafe.Pointer(t));
        return isReflexive(_addr_tt.elem);
    else if (t.Kind() == Struct) 
        tt = (structType.val)(@unsafe.Pointer(t));
        foreach (var (_, f) in tt.fields) {
            if (!isReflexive(_addr_f.typ)) {
                return false;
            }
        }        return true;
    else 
        // Func, Map, Slice, Invalid
        panic("isReflexive called on non-key type " + t.String());
    });

// needKeyUpdate reports whether map overwrites require the key to be copied.
private static bool needKeyUpdate(ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;


    if (t.Kind() == Bool || t.Kind() == Int || t.Kind() == Int8 || t.Kind() == Int16 || t.Kind() == Int32 || t.Kind() == Int64 || t.Kind() == Uint || t.Kind() == Uint8 || t.Kind() == Uint16 || t.Kind() == Uint32 || t.Kind() == Uint64 || t.Kind() == Uintptr || t.Kind() == Chan || t.Kind() == Ptr || t.Kind() == UnsafePointer) 
        return false;
    else if (t.Kind() == Float32 || t.Kind() == Float64 || t.Kind() == Complex64 || t.Kind() == Complex128 || t.Kind() == Interface || t.Kind() == String) 
        // Float keys can be updated from +0 to -0.
        // String keys can be updated to use a smaller backing store.
        // Interfaces might have floats of strings in them.
        return true;
    else if (t.Kind() == Array) 
        var tt = (arrayType.val)(@unsafe.Pointer(t));
        return needKeyUpdate(_addr_tt.elem);
    else if (t.Kind() == Struct) 
        tt = (structType.val)(@unsafe.Pointer(t));
        foreach (var (_, f) in tt.fields) {
            if (needKeyUpdate(_addr_f.typ)) {
                return true;
            }
        }        return false;
    else 
        // Func, Map, Slice, Invalid
        panic("needKeyUpdate called on non-key type " + t.String());
    });

// hashMightPanic reports whether the hash of a map key of type t might panic.
private static bool hashMightPanic(ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;


    if (t.Kind() == Interface) 
        return true;
    else if (t.Kind() == Array) 
        var tt = (arrayType.val)(@unsafe.Pointer(t));
        return hashMightPanic(_addr_tt.elem);
    else if (t.Kind() == Struct) 
        tt = (structType.val)(@unsafe.Pointer(t));
        foreach (var (_, f) in tt.fields) {
            if (hashMightPanic(_addr_f.typ)) {
                return true;
            }
        }        return false;
    else 
        return false;
    }

// Make sure these routines stay in sync with ../../runtime/map.go!
// These types exist only for GC, so we only fill out GC relevant info.
// Currently, that's just size and the GC program. We also fill in string
// for possible debugging use.
private static readonly System.UIntPtr bucketSize = 8;
private static readonly System.UIntPtr maxKeySize = 128;
private static readonly System.UIntPtr maxValSize = 128;

private static ptr<rtype> bucketOf(ptr<rtype> _addr_ktyp, ptr<rtype> _addr_etyp) => func((_, panic, _) => {
    ref rtype ktyp = ref _addr_ktyp.val;
    ref rtype etyp = ref _addr_etyp.val;

    if (ktyp.size > maxKeySize) {
        ktyp = PtrTo(ktyp)._<ptr<rtype>>();
    }
    if (etyp.size > maxValSize) {
        etyp = PtrTo(etyp)._<ptr<rtype>>();
    }
    ptr<byte> gcdata;
    System.UIntPtr ptrdata = default;
    System.UIntPtr overflowPad = default;

    var size = bucketSize * (1 + ktyp.size + etyp.size) + overflowPad + ptrSize;
    if (size & uintptr(ktyp.align - 1) != 0 || size & uintptr(etyp.align - 1) != 0) {
        panic("reflect: bad size computation in MapOf");
    }
    if (ktyp.ptrdata != 0 || etyp.ptrdata != 0) {
        var nptr = (bucketSize * (1 + ktyp.size + etyp.size) + ptrSize) / ptrSize;
        var mask = make_slice<byte>((nptr + 7) / 8);
        var @base = bucketSize / ptrSize;

        if (ktyp.ptrdata != 0) {
            emitGCMask(mask, base, _addr_ktyp, bucketSize);
        }
        base += bucketSize * ktyp.size / ptrSize;

        if (etyp.ptrdata != 0) {
            emitGCMask(mask, base, _addr_etyp, bucketSize);
        }
        base += bucketSize * etyp.size / ptrSize;
        base += overflowPad / ptrSize;

        var word = base;
        mask[word / 8] |= 1 << (int)((word % 8));
        gcdata = _addr_mask[0];
        ptrdata = (word + 1) * ptrSize; 

        // overflow word must be last
        if (ptrdata != size) {
            panic("reflect: bad layout computation in MapOf");
        }
    }
    ptr<rtype> b = addr(new rtype(align:ptrSize,size:size,kind:uint8(Struct),ptrdata:ptrdata,gcdata:gcdata,));
    if (overflowPad > 0) {
        b.align = 8;
    }
    @string s = "bucket(" + ktyp.String() + "," + etyp.String() + ")";
    b.str = resolveReflectName(newName(s, "", false));
    return _addr_b!;
});

private static slice<byte> gcSlice(this ptr<rtype> _addr_t, System.UIntPtr begin, System.UIntPtr end) {
    ref rtype t = ref _addr_t.val;

    return new ptr<ptr<array<byte>>>(@unsafe.Pointer(t.gcdata)).slice(begin, end, end);
}

// emitGCMask writes the GC mask for [n]typ into out, starting at bit
// offset base.
private static void emitGCMask(slice<byte> @out, System.UIntPtr @base, ptr<rtype> _addr_typ, System.UIntPtr n) => func((_, panic, _) => {
    ref rtype typ = ref _addr_typ.val;

    if (typ.kind & kindGCProg != 0) {
        panic("reflect: unexpected GC program");
    }
    var ptrs = typ.ptrdata / ptrSize;
    var words = typ.size / ptrSize;
    var mask = typ.gcSlice(0, (ptrs + 7) / 8);
    for (var j = uintptr(0); j < ptrs; j++) {
        if ((mask[j / 8] >> (int)((j % 8))) & 1 != 0) {
            for (var i = uintptr(0); i < n; i++) {
                var k = base + i * words + j;
                out[k / 8] |= 1 << (int)((k % 8));
            }
        }
    }
});

// appendGCProg appends the GC program for the first ptrdata bytes of
// typ to dst and returns the extended slice.
private static slice<byte> appendGCProg(slice<byte> dst, ptr<rtype> _addr_typ) {
    ref rtype typ = ref _addr_typ.val;

    if (typ.kind & kindGCProg != 0) { 
        // Element has GC program; emit one element.
        var n = uintptr(new ptr<ptr<ptr<uint>>>(@unsafe.Pointer(typ.gcdata)));
        var prog = typ.gcSlice(4, 4 + n - 1);
        return append(dst, prog);
    }
    var ptrs = typ.ptrdata / ptrSize;
    var mask = typ.gcSlice(0, (ptrs + 7) / 8); 

    // Emit 120-bit chunks of full bytes (max is 127 but we avoid using partial bytes).
    while (ptrs > 120) {
        dst = append(dst, 120);
        dst = append(dst, mask[..(int)15]);
        mask = mask[(int)15..];
        ptrs -= 120;
    }

    dst = append(dst, byte(ptrs));
    dst = append(dst, mask);
    return dst;
}

// SliceOf returns the slice type with element type t.
// For example, if t represents int, SliceOf(t) represents []int.
public static Type SliceOf(Type t) {
    ptr<rtype> typ = t._<ptr<rtype>>(); 

    // Look in cache.
    cacheKey ckey = new cacheKey(Slice,typ,nil,0);
    {
        var slice__prev1 = slice;

        var (slice, ok) = lookupCache.Load(ckey);

        if (ok) {
            return slice._<Type>();
        }
        slice = slice__prev1;

    } 

    // Look in known types.
    @string s = "[]" + typ.String();
    foreach (var (_, tt) in typesByString(s)) {
        var slice = (sliceType.val)(@unsafe.Pointer(tt));
        if (slice.elem == typ) {
            var (ti, _) = lookupCache.LoadOrStore(ckey, tt);
            return ti._<Type>();
        }
    }    ref slice<unsafe.Pointer> islice = ref heap((slice<unsafe.Pointer>)null, out ptr<slice<unsafe.Pointer>> _addr_islice);
    ptr<ptr<ptr<sliceType>>> prototype = new ptr<ptr<ptr<ptr<sliceType>>>>(@unsafe.Pointer(_addr_islice));
    slice = prototype.val;
    slice.tflag = 0;
    slice.str = resolveReflectName(newName(s, "", false));
    slice.hash = fnv1(typ.hash, '[');
    slice.elem = typ;
    slice.ptrToThis = 0;

    (ti, _) = lookupCache.LoadOrStore(ckey, _addr_slice.rtype);
    return ti._<Type>();
}

// The structLookupCache caches StructOf lookups.
// StructOf does not share the common lookupCache since we need to pin
// the memory associated with *structTypeFixedN.
private static var structLookupCache = default;

private partial struct structTypeUncommon {
    public ref structType structType => ref structType_val;
    public uncommonType u;
}

// isLetter reports whether a given 'rune' is classified as a Letter.
private static bool isLetter(int ch) {
    return 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_' || ch >= utf8.RuneSelf && unicode.IsLetter(ch);
}

// isValidFieldName checks if a string is a valid (struct) field name or not.
//
// According to the language spec, a field name should be an identifier.
//
// identifier = letter { letter | unicode_digit } .
// letter = unicode_letter | "_" .
private static bool isValidFieldName(@string fieldName) {
    foreach (var (i, c) in fieldName) {
        if (i == 0 && !isLetter(c)) {
            return false;
        }
        if (!(isLetter(c) || unicode.IsDigit(c))) {
            return false;
        }
    }    return len(fieldName) > 0;
}

// StructOf returns the struct type containing fields.
// The Offset and Index fields are ignored and computed as they would be
// by the compiler.
//
// StructOf currently does not generate wrapper methods for embedded
// fields and panics if passed unexported StructFields.
// These limitations may be lifted in a future version.
public static Type StructOf(slice<StructField> fields) => func((defer, panic, _) => {
    var hash = fnv1(0, (slice<byte>)"struct {");    System.UIntPtr size = default;    byte typalign = default;    var comparable = true;    slice<method> methods = default;    var fs = make_slice<structField>(len(fields));    var repr = make_slice<byte>(0, 64);    var hasGCProg = false;

    var lastzero = uintptr(0);
    repr = append(repr, "struct {");
    @string pkgpath = "";
    {
        var i__prev1 = i;

        foreach (var (__i, __field) in fields) {
            i = __i;
            field = __field;
            if (field.Name == "") {
                panic("reflect.StructOf: field " + strconv.Itoa(i) + " has no name");
            }
            if (!isValidFieldName(field.Name)) {
                panic("reflect.StructOf: field " + strconv.Itoa(i) + " has invalid name");
            }
            if (field.Type == null) {
                panic("reflect.StructOf: field " + strconv.Itoa(i) + " has no type");
            }
            var (f, fpkgpath) = runtimeStructField(field);
            var ft = f.typ;
            if (ft.kind & kindGCProg != 0) {
                hasGCProg = true;
            }
            if (fpkgpath != "") {
                if (pkgpath == "") {
                    pkgpath = fpkgpath;
                }
                else if (pkgpath != fpkgpath) {
                    panic("reflect.Struct: fields with different PkgPath " + pkgpath + " and " + fpkgpath);
                }
            } 

            // Update string and hash
            var name = f.name.name();
            hash = fnv1(hash, (slice<byte>)name);
            repr = append(repr, (" " + name));
            if (f.embedded()) { 
                // Embedded field
                if (f.typ.Kind() == Ptr) { 
                    // Embedded ** and *interface{} are illegal
                    var elem = ft.Elem();
                    {
                        var k = elem.Kind();

                        if (k == Ptr || k == Interface) {
                            panic("reflect.StructOf: illegal embedded field type " + ft.String());
                        }

                    }
                }

                if (f.typ.Kind() == Interface) 
                    var ift = (interfaceType.val)(@unsafe.Pointer(ft));
                    {
                        var m__prev2 = m;

                        foreach (var (__im, __m) in ift.methods) {
                            im = __im;
                            m = __m;
                            if (ift.nameOff(m.name).pkgPath() != "") { 
                                // TODO(sbinet).  Issue 15924.
                                panic("reflect: embedded interface with unexported method(s) not implemented");
                            }
                            var mtyp = ift.typeOff(m.typ);                            var ifield = i;                            var imethod = im;                            ref Value ifn = ref heap(out ptr<Value> _addr_ifn);                            ref Value tfn = ref heap(out ptr<Value> _addr_tfn);

                            if (ft.kind & kindDirectIface != 0) {
                                tfn = MakeFunc(mtyp, @in => {
                                    slice<Value> args = default;
                                    var recv = in[0];
                                    if (len(in) > 1) {
                                        args = in[(int)1..];
                                    }
                                    return recv.Field(ifield).Method(imethod).Call(args);
                                }
                            else
);
                                ifn = MakeFunc(mtyp, @in => {
                                    args = default;
                                    recv = in[0];
                                    if (len(in) > 1) {
                                        args = in[(int)1..];
                                    }
                                    return recv.Field(ifield).Method(imethod).Call(args);
                                });
                            } {
                                tfn = MakeFunc(mtyp, @in => {
                                    args = default;
                                    recv = in[0];
                                    if (len(in) > 1) {
                                        args = in[(int)1..];
                                    }
                                    return recv.Field(ifield).Method(imethod).Call(args);
                                });
                                ifn = MakeFunc(mtyp, @in => {
                                    args = default;
                                    recv = Indirect(in[0]);
                                    if (len(in) > 1) {
                                        args = in[(int)1..];
                                    }
                                    return recv.Field(ifield).Method(imethod).Call(args);
                                });
                            }
                            methods = append(methods, new method(name:resolveReflectName(ift.nameOff(m.name)),mtyp:resolveReflectType(mtyp),ifn:resolveReflectText(unsafe.Pointer(&ifn)),tfn:resolveReflectText(unsafe.Pointer(&tfn)),));
                        }

                        m = m__prev2;
                    }
                else if (f.typ.Kind() == Ptr) 
                    var ptr = (ptrType.val)(@unsafe.Pointer(ft));
                    {
                        var unt__prev2 = unt;

                        var unt = ptr.uncommon();

                        if (unt != null) {
                            if (i > 0 && unt.mcount > 0) { 
                                // Issue 15924.
                                panic("reflect: embedded type with methods not implemented if type is not first field");
                            }
                            if (len(fields) > 1) {
                                panic("reflect: embedded type with methods not implemented if there is more than one field");
                            }
                            {
                                var m__prev2 = m;

                                foreach (var (_, __m) in unt.methods()) {
                                    m = __m;
                                    var mname = ptr.nameOff(m.name);
                                    if (mname.pkgPath() != "") { 
                                        // TODO(sbinet).
                                        // Issue 15924.
                                        panic("reflect: embedded interface with unexported method(s) not implemented");
                                    }
                                    methods = append(methods, new method(name:resolveReflectName(mname),mtyp:resolveReflectType(ptr.typeOff(m.mtyp)),ifn:resolveReflectText(ptr.textOff(m.ifn)),tfn:resolveReflectText(ptr.textOff(m.tfn)),));
                                }

                                m = m__prev2;
                            }
                        }

                        unt = unt__prev2;

                    }
                    {
                        var unt__prev2 = unt;

                        unt = ptr.elem.uncommon();

                        if (unt != null) {
                            {
                                var m__prev2 = m;

                                foreach (var (_, __m) in unt.methods()) {
                                    m = __m;
                                    mname = ptr.nameOff(m.name);
                                    if (mname.pkgPath() != "") { 
                                        // TODO(sbinet)
                                        // Issue 15924.
                                        panic("reflect: embedded interface with unexported method(s) not implemented");
                                    }
                                    methods = append(methods, new method(name:resolveReflectName(mname),mtyp:resolveReflectType(ptr.elem.typeOff(m.mtyp)),ifn:resolveReflectText(ptr.elem.textOff(m.ifn)),tfn:resolveReflectText(ptr.elem.textOff(m.tfn)),));
                                }

                                m = m__prev2;
                            }
                        }

                        unt = unt__prev2;

                    }
                else 
                    {
                        var unt__prev2 = unt;

                        unt = ft.uncommon();

                        if (unt != null) {
                            if (i > 0 && unt.mcount > 0) { 
                                // Issue 15924.
                                panic("reflect: embedded type with methods not implemented if type is not first field");
                            }
                            if (len(fields) > 1 && ft.kind & kindDirectIface != 0) {
                                panic("reflect: embedded type with methods not implemented for non-pointer type");
                            }
                            {
                                var m__prev2 = m;

                                foreach (var (_, __m) in unt.methods()) {
                                    m = __m;
                                    mname = ft.nameOff(m.name);
                                    if (mname.pkgPath() != "") { 
                                        // TODO(sbinet)
                                        // Issue 15924.
                                        panic("reflect: embedded interface with unexported method(s) not implemented");
                                    }
                                    methods = append(methods, new method(name:resolveReflectName(mname),mtyp:resolveReflectType(ft.typeOff(m.mtyp)),ifn:resolveReflectText(ft.textOff(m.ifn)),tfn:resolveReflectText(ft.textOff(m.tfn)),));
                                }

                                m = m__prev2;
                            }
                        }

                        unt = unt__prev2;

                    }
                            }
            {
                var (_, dup) = fset[name];

                if (dup) {
                    panic("reflect.StructOf: duplicate field " + name);
                }

            }
            fset[name] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

            hash = fnv1(hash, byte(ft.hash >> 24), byte(ft.hash >> 16), byte(ft.hash >> 8), byte(ft.hash));

            repr = append(repr, (" " + ft.String()));
            if (f.name.hasTag()) {
                hash = fnv1(hash, (slice<byte>)f.name.tag());
                repr = append(repr, (" " + strconv.Quote(f.name.tag())));
            }
            if (i < len(fields) - 1) {
                repr = append(repr, ';');
            }
            comparable = comparable && (ft.equal != null);

            var offset = align(size, uintptr(ft.align));
            if (ft.align > typalign) {
                typalign = ft.align;
            }
            size = offset + ft.size;
            f.offsetEmbed |= offset << 1;

            if (ft.size == 0) {
                lastzero = size;
            }
            fs[i] = f;
        }
        i = i__prev1;
    }

    if (size > 0 && lastzero == size) { 
        // This is a non-zero sized struct that ends in a
        // zero-sized field. We add an extra byte of padding,
        // to ensure that taking the address of the final
        // zero-sized field can't manufacture a pointer to the
        // next object in the heap. See issue 9401.
        size++;
    }
    ptr<structType> typ;
    ptr<uncommonType> ut;

    if (len(methods) == 0) {
        ptr<structTypeUncommon> t = @new<structTypeUncommon>();
        typ = _addr_t.structType;
        ut = _addr_t.u;
    }
    else
 { 
        // A *rtype representing a struct is followed directly in memory by an
        // array of method objects representing the methods attached to the
        // struct. To get the same layout for a run time generated type, we
        // need an array directly following the uncommonType memory.
        // A similar strategy is used for funcTypeFixed4, ...funcTypeFixedN.
        var tt = New(StructOf(new slice<StructField>(new StructField[] { {Name:"S",Type:TypeOf(structType{})}, {Name:"U",Type:TypeOf(uncommonType{})}, {Name:"M",Type:ArrayOf(len(methods),TypeOf(methods[0]))} })));

        typ = (structType.val)(@unsafe.Pointer(tt.Elem().Field(0).UnsafeAddr()));
        ut = (uncommonType.val)(@unsafe.Pointer(tt.Elem().Field(1).UnsafeAddr()));

        copy(tt.Elem().Field(2).Slice(0, len(methods)).Interface()._<slice<method>>(), methods);
    }
    ut.mcount = uint16(len(methods));
    ut.xcount = ut.mcount;
    ut.moff = uint32(@unsafe.Sizeof(new uncommonType()));

    if (len(fs) > 0) {
        repr = append(repr, ' ');
    }
    repr = append(repr, '}');
    hash = fnv1(hash, '}');
    var str = string(repr); 

    // Round the size up to be a multiple of the alignment.
    size = align(size, uintptr(typalign)); 

    // Make the struct type.
    ref struct{} istruct = ref heap(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{}, out ptr<struct{}> _addr_istruct);
    ptr<ptr<ptr<structType>>> prototype = new ptr<ptr<ptr<ptr<structType>>>>(@unsafe.Pointer(_addr_istruct));
    typ.val = prototype.val;
    typ.fields = fs;
    if (pkgpath != "") {
        typ.pkgPath = newName(pkgpath, "", false);
    }
    {
        var ts__prev1 = ts;

        var (ts, ok) = structLookupCache.m.Load(hash);

        if (ok) {
            {
                slice<Type> st__prev1 = st;

                foreach (var (_, __st) in ts._<slice<Type>>()) {
                    st = __st;
                    t = st.common();
                    if (haveIdenticalUnderlyingType(_addr_typ.rtype, t, true)) {
                        return t;
                    }
                }

                st = st__prev1;
            }
        }
        ts = ts__prev1;

    } 

    // Not in cache, lock and retry.
    structLookupCache.Lock();
    defer(structLookupCache.Unlock());
    {
        var ts__prev1 = ts;

        (ts, ok) = structLookupCache.m.Load(hash);

        if (ok) {
            {
                slice<Type> st__prev1 = st;

                foreach (var (_, __st) in ts._<slice<Type>>()) {
                    st = __st;
                    t = st.common();
                    if (haveIdenticalUnderlyingType(_addr_typ.rtype, t, true)) {
                        return t;
                    }
                }

                st = st__prev1;
            }
        }
        ts = ts__prev1;

    }

    Func<Type, Type> addToCache = t => {
        slice<Type> ts = default;
        {
            var (ti, ok) = structLookupCache.m.Load(hash);

            if (ok) {
                ts = ti._<slice<Type>>();
            }

        }
        structLookupCache.m.Store(hash, append(ts, t));
        return t;
    }; 

    // Look in known types.
    {
        ptr<structTypeUncommon> t__prev1 = t;

        foreach (var (_, __t) in typesByString(str)) {
            t = __t;
            if (haveIdenticalUnderlyingType(_addr_typ.rtype, t, true)) { 
                // even if 't' wasn't a structType with methods, we should be ok
                // as the 'u uncommonType' field won't be accessed except when
                // tflag&tflagUncommon is set.
                return addToCache(t);
            }
        }
        t = t__prev1;
    }

    typ.str = resolveReflectName(newName(str, "", false));
    typ.tflag = 0; // TODO: set tflagRegularMemory
    typ.hash = hash;
    typ.size = size;
    typ.ptrdata = typeptrdata(_addr_typ.common());
    typ.align = typalign;
    typ.fieldAlign = typalign;
    typ.ptrToThis = 0;
    if (len(methods) > 0) {
        typ.tflag |= tflagUncommon;
    }
    if (hasGCProg) {
        nint lastPtrField = 0;
        {
            var i__prev1 = i;
            var ft__prev1 = ft;

            foreach (var (__i, __ft) in fs) {
                i = __i;
                ft = __ft;
                if (ft.typ.pointers()) {
                    lastPtrField = i;
                }
            }
    else

            i = i__prev1;
            ft = ft__prev1;
        }

        byte prog = new slice<byte>(new byte[] { 0, 0, 0, 0 }); // will be length of prog
        System.UIntPtr off = default;
        {
            var i__prev1 = i;
            var ft__prev1 = ft;

            foreach (var (__i, __ft) in fs) {
                i = __i;
                ft = __ft;
                if (i > lastPtrField) { 
                    // gcprog should not include anything for any field after
                    // the last field that contains pointer data
                    break;
                }
                if (!ft.typ.pointers()) { 
                    // Ignore pointerless fields.
                    continue;
                } 
                // Pad to start of this field with zeros.
                if (ft.offset() > off) {
                    var n = (ft.offset() - off) / ptrSize;
                    prog = append(prog, 0x01, 0x00); // emit a 0 bit
                    if (n > 1) {
                        prog = append(prog, 0x81); // repeat previous bit
                        prog = appendVarint(prog, n - 1); // n-1 times
                    }
                    off = ft.offset();
                }
                prog = appendGCProg(prog, _addr_ft.typ);
                off += ft.typ.ptrdata;
            }

            i = i__prev1;
            ft = ft__prev1;
        }

        prog = append(prog, 0) * (uint32.val);

        (@unsafe.Pointer(_addr_prog[0])) = uint32(len(prog) - 4);
        typ.kind |= kindGCProg;
        typ.gcdata = _addr_prog[0];
    } {
        typ.kind &= kindGCProg;
        ptr<object> bv = @new<bitVector>();
        addTypeBits(bv, 0, _addr_typ.common());
        if (len(bv.data) > 0) {
            typ.gcdata = _addr_bv.data[0];
        }
    }
    typ.equal = null;
    if (comparable) {
        typ.equal = (p, q) => {
            {
                var ft__prev1 = ft;

                foreach (var (_, __ft) in typ.fields) {
                    ft = __ft;
                    var pi = add(p, ft.offset(), "&x.field safe");
                    var qi = add(q, ft.offset(), "&x.field safe");
                    if (!ft.typ.equal(pi, qi)) {
                        return false;
                    }
                }

                ft = ft__prev1;
            }

            return true;
        };
    }

    if (len(fs) == 1 && !ifaceIndir(_addr_fs[0].typ)) 
        // structs of 1 direct iface type can be direct
        typ.kind |= kindDirectIface;
    else 
        typ.kind &= kindDirectIface;
        return addToCache(_addr_typ.rtype);
});

// runtimeStructField takes a StructField value passed to StructOf and
// returns both the corresponding internal representation, of type
// structField, and the pkgpath value to use for this field.
private static (structField, @string) runtimeStructField(StructField field) => func((_, panic, _) => {
    structField _p0 = default;
    @string _p0 = default;

    if (field.Anonymous && field.PkgPath != "") {
        panic("reflect.StructOf: field \"" + field.Name + "\" is anonymous but has PkgPath set");
    }
    if (field.IsExported()) { 
        // Best-effort check for misuse.
        // Since this field will be treated as exported, not much harm done if Unicode lowercase slips through.
        var c = field.Name[0];
        if ('a' <= c && c <= 'z' || c == '_') {
            panic("reflect.StructOf: field \"" + field.Name + "\" is unexported but missing PkgPath");
        }
    }
    var offsetEmbed = uintptr(0);
    if (field.Anonymous) {
        offsetEmbed |= 1;
    }
    resolveReflectType(_addr_field.Type.common()); // install in runtime
    structField f = new structField(name:newName(field.Name,string(field.Tag),field.IsExported()),typ:field.Type.common(),offsetEmbed:offsetEmbed,);
    return (f, field.PkgPath);
});

// typeptrdata returns the length in bytes of the prefix of t
// containing pointer data. Anything after this offset is scalar data.
// keep in sync with ../cmd/compile/internal/reflectdata/reflect.go
private static System.UIntPtr typeptrdata(ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;


    if (t.Kind() == Struct) 
        var st = (structType.val)(@unsafe.Pointer(t)); 
        // find the last field that has pointers.
        nint field = -1;
        foreach (var (i) in st.fields) {
            var ft = st.fields[i].typ;
            if (ft.pointers()) {
                field = i;
            }
        }        if (field == -1) {
            return 0;
        }
        var f = st.fields[field];
        return f.offset() + f.typ.ptrdata;
    else 
        panic("reflect.typeptrdata: unexpected type, " + t.String());
    });

// See cmd/compile/internal/reflectdata/reflect.go for derivation of constant.
private static readonly nint maxPtrmaskBytes = 2048;

// ArrayOf returns the array type with the given length and element type.
// For example, if t represents int, ArrayOf(5, t) represents [5]int.
//
// If the resulting type would be larger than the available address space,
// ArrayOf panics.


// ArrayOf returns the array type with the given length and element type.
// For example, if t represents int, ArrayOf(5, t) represents [5]int.
//
// If the resulting type would be larger than the available address space,
// ArrayOf panics.
public static Type ArrayOf(nint length, Type elem) => func((_, panic, _) => {
    if (length < 0) {
        panic("reflect: negative length passed to ArrayOf");
    }
    ptr<rtype> typ = elem._<ptr<rtype>>(); 

    // Look in cache.
    cacheKey ckey = new cacheKey(Array,typ,nil,uintptr(length));
    {
        var array__prev1 = array;

        var (array, ok) = lookupCache.Load(ckey);

        if (ok) {
            return array._<Type>();
        }
        array = array__prev1;

    } 

    // Look in known types.
    @string s = "[" + strconv.Itoa(length) + "]" + typ.String();
    foreach (var (_, tt) in typesByString(s)) {
        var array = (arrayType.val)(@unsafe.Pointer(tt));
        if (array.elem == typ) {
            var (ti, _) = lookupCache.LoadOrStore(ckey, tt);
            return ti._<Type>();
        }
    }    ref array<unsafe.Pointer> iarray = ref heap(new array<unsafe.Pointer>(new unsafe.Pointer[] {  }), out ptr<array<unsafe.Pointer>> _addr_iarray);
    ptr<ptr<ptr<arrayType>>> prototype = new ptr<ptr<ptr<ptr<arrayType>>>>(@unsafe.Pointer(_addr_iarray));
    array = prototype.val;
    array.tflag = typ.tflag & tflagRegularMemory;
    array.str = resolveReflectName(newName(s, "", false));
    array.hash = fnv1(typ.hash, '[');
    {
        var n = uint32(length);

        while (n > 0) {
            array.hash = fnv1(array.hash, byte(n));
            n>>=8;
        }
    }
    array.hash = fnv1(array.hash, ']');
    array.elem = typ;
    array.ptrToThis = 0;
    if (typ.size > 0) {
        var max = ~uintptr(0) / typ.size;
        if (uintptr(length) > max) {
            panic("reflect.ArrayOf: array size would exceed virtual address space");
        }
    }
    array.size = typ.size * uintptr(length);
    if (length > 0 && typ.ptrdata != 0) {
        array.ptrdata = typ.size * uintptr(length - 1) + typ.ptrdata;
    }
    array.align = typ.align;
    array.fieldAlign = typ.fieldAlign;
    array.len = uintptr(length);
    array.slice = SliceOf(elem)._<ptr<rtype>>();


    if (typ.ptrdata == 0 || array.size == 0) 
        // No pointers.
        array.gcdata = null;
        array.ptrdata = 0;
    else if (length == 1) 
        // In memory, 1-element array looks just like the element.
        array.kind |= typ.kind & kindGCProg;
        array.gcdata = typ.gcdata;
        array.ptrdata = typ.ptrdata;
    else if (typ.kind & kindGCProg == 0 && array.size <= maxPtrmaskBytes * 8 * ptrSize) 
        // Element is small with pointer mask; array is still small.
        // Create direct pointer mask by turning each 1 bit in elem
        // into length 1 bits in larger mask.
        var mask = make_slice<byte>((array.ptrdata / ptrSize + 7) / 8);
        emitGCMask(mask, 0, typ, array.len);
        array.gcdata = _addr_mask[0];
    else 
        // Create program that emits one element
        // and then repeats to make the array.
        byte prog = new slice<byte>(new byte[] { 0, 0, 0, 0 }); // will be length of prog
        prog = appendGCProg(prog, typ); 
        // Pad from ptrdata to size.
        var elemPtrs = typ.ptrdata / ptrSize;
        var elemWords = typ.size / ptrSize;
        if (elemPtrs < elemWords) { 
            // Emit literal 0 bit, then repeat as needed.
            prog = append(prog, 0x01, 0x00);
            if (elemPtrs + 1 < elemWords) {
                prog = append(prog, 0x81);
                prog = appendVarint(prog, elemWords - elemPtrs - 1);
            }
        }
        if (elemWords < 0x80) {
            prog = append(prog, byte(elemWords | 0x80));
        }
        else
 {
            prog = append(prog, 0x80);
            prog = appendVarint(prog, elemWords);
        }
        prog = appendVarint(prog, uintptr(length) - 1);
        prog = append(prog, 0) * (uint32.val);

        (@unsafe.Pointer(_addr_prog[0])) = uint32(len(prog) - 4);
        array.kind |= kindGCProg;
        array.gcdata = _addr_prog[0];
        array.ptrdata = array.size; // overestimate but ok; must match program
        var etyp = typ.common();
    var esize = etyp.Size();

    array.equal = null;
    {
        var eequal = etyp.equal;

        if (eequal != null) {
            array.equal = (p, q) => {
                for (nint i = 0; i < length; i++) {
                    var pi = arrayAt(p, i, esize, "i < length");
                    var qi = arrayAt(q, i, esize, "i < length");
                    if (!eequal(pi, qi)) {
                        return false;
                    }
                }

                return true;
            }
;
        }
    }


    if (length == 1 && !ifaceIndir(typ)) 
        // array of 1 direct iface type can be direct
        array.kind |= kindDirectIface;
    else 
        array.kind &= kindDirectIface;
        (ti, _) = lookupCache.LoadOrStore(ckey, _addr_array.rtype);
    return ti._<Type>();
});

private static slice<byte> appendVarint(slice<byte> x, System.UIntPtr v) {
    while (v >= 0x80) {
        x = append(x, byte(v | 0x80));
        v>>=7;
    }
    x = append(x, byte(v));
    return x;
}

// toType converts from a *rtype to a Type that can be returned
// to the client of package reflect. In gc, the only concern is that
// a nil *rtype must be replaced by a nil Type, but in gccgo this
// function takes care of ensuring that multiple *rtype for the same
// type are coalesced into a single Type.
private static Type toType(ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    if (t == null) {
        return null;
    }
    return t;
}

private partial struct layoutKey {
    public ptr<funcType> ftyp; // function signature
    public ptr<rtype> rcvr; // receiver type, or nil if none
}

private partial struct layoutType {
    public ptr<rtype> t;
    public ptr<sync.Pool> framePool;
    public abiDesc abi;
}

private static sync.Map layoutCache = default; // map[layoutKey]layoutType

// funcLayout computes a struct type representing the layout of the
// stack-assigned function arguments and return values for the function
// type t.
// If rcvr != nil, rcvr specifies the type of the receiver.
// The returned type exists only for GC, so we only fill out GC relevant info.
// Currently, that's just size and the GC program. We also fill in
// the name for possible debugging use.
private static (ptr<rtype>, ptr<sync.Pool>, abiDesc) funcLayout(ptr<funcType> _addr_t, ptr<rtype> _addr_rcvr) => func((_, panic, _) => {
    ptr<rtype> frametype = default!;
    ptr<sync.Pool> framePool = default!;
    abiDesc abi = default;
    ref funcType t = ref _addr_t.val;
    ref rtype rcvr = ref _addr_rcvr.val;

    if (t.Kind() != Func) {
        panic("reflect: funcLayout of non-func type " + t.String());
    }
    if (rcvr != null && rcvr.Kind() == Interface) {
        panic("reflect: funcLayout with interface receiver " + rcvr.String());
    }
    layoutKey k = new layoutKey(t,rcvr);
    {
        var lti__prev1 = lti;

        var (lti, ok) = layoutCache.Load(k);

        if (ok) {
            layoutType lt = lti._<layoutType>();
            return (_addr_lt.t!, _addr_lt.framePool!, lt.abi);
        }
        lti = lti__prev1;

    } 

    // Compute the ABI layout.
    abi = newAbiDesc(t, rcvr); 

    // build dummy rtype holding gc program
    ptr<rtype> x = addr(new rtype(align:ptrSize,size:align(abi.retOffset+abi.ret.stackBytes,ptrSize),ptrdata:uintptr(abi.stackPtrs.n)*ptrSize,));
    if (abi.stackPtrs.n > 0) {
        x.gcdata = _addr_abi.stackPtrs.data[0];
    }
    @string s = default;
    if (rcvr != null) {
        s = "methodargs(" + rcvr.String() + ")(" + t.String() + ")";
    }
    else
 {
        s = "funcargs(" + t.String() + ")";
    }
    x.str = resolveReflectName(newName(s, "", false)); 

    // cache result for future callers
    framePool = addr(new sync.Pool(New:func()interface{}{returnunsafe_New(x)}));
    var (lti, _) = layoutCache.LoadOrStore(k, new layoutType(t:x,framePool:framePool,abi:abi,));
    lt = lti._<layoutType>();
    return (_addr_lt.t!, _addr_lt.framePool!, lt.abi);
});

// ifaceIndir reports whether t is stored indirectly in an interface value.
private static bool ifaceIndir(ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return t.kind & kindDirectIface == 0;
}

// Note: this type must agree with runtime.bitvector.
private partial struct bitVector {
    public uint n; // number of bits
    public slice<byte> data;
}

// append a bit to the bitmap.
private static void append(this ptr<bitVector> _addr_bv, byte bit) {
    ref bitVector bv = ref _addr_bv.val;

    if (bv.n % 8 == 0) {
        bv.data = append(bv.data, 0);
    }
    bv.data[bv.n / 8] |= bit << (int)((bv.n % 8));
    bv.n++;
}

private static void addTypeBits(ptr<bitVector> _addr_bv, System.UIntPtr offset, ptr<rtype> _addr_t) {
    ref bitVector bv = ref _addr_bv.val;
    ref rtype t = ref _addr_t.val;

    if (t.ptrdata == 0) {
        return ;
    }

    if (Kind(t.kind & kindMask) == Chan || Kind(t.kind & kindMask) == Func || Kind(t.kind & kindMask) == Map || Kind(t.kind & kindMask) == Ptr || Kind(t.kind & kindMask) == Slice || Kind(t.kind & kindMask) == String || Kind(t.kind & kindMask) == UnsafePointer) 
        // 1 pointer at start of representation
        while (bv.n < uint32(offset / uintptr(ptrSize))) {
            bv.append(0);
        }
        bv.append(1);
    else if (Kind(t.kind & kindMask) == Interface) 
        // 2 pointers
        while (bv.n < uint32(offset / uintptr(ptrSize))) {
            bv.append(0);
        }
        bv.append(1);
        bv.append(1);
    else if (Kind(t.kind & kindMask) == Array) 
        // repeat inner type
        var tt = (arrayType.val)(@unsafe.Pointer(t));
        {
            nint i__prev1 = i;

            for (nint i = 0; i < int(tt.len); i++) {
                addTypeBits(_addr_bv, offset + uintptr(i) * tt.elem.size, _addr_tt.elem);
            }


            i = i__prev1;
        }
    else if (Kind(t.kind & kindMask) == Struct) 
        // apply fields
        tt = (structType.val)(@unsafe.Pointer(t));
        {
            nint i__prev1 = i;

            foreach (var (__i) in tt.fields) {
                i = __i;
                var f = _addr_tt.fields[i];
                addTypeBits(_addr_bv, offset + f.offset(), _addr_f.typ);
            }

            i = i__prev1;
        }
    }

} // end reflect_package
