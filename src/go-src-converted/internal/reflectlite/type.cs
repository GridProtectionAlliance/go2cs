// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package reflectlite implements lightweight version of reflect, not using
// any package except for "runtime" and "unsafe".
// package reflectlite -- go2cs converted at 2022 March 06 22:12:34 UTC
// import "internal/reflectlite" ==> using reflectlite = go.@internal.reflectlite_package
// Original source: C:\Program Files\Go\src\internal\reflectlite\type.go
using unsafeheader = go.@internal.unsafeheader_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go.@internal;

public static partial class reflectlite_package {

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
public partial interface Type {
    ptr<uncommonType> Name(); // PkgPath returns a defined type's package path, that is, the import path
// that uniquely identifies the package, such as "encoding/base64".
// If the type was predeclared (string, error) or not defined (*T, struct{},
// []int, or A where A is an alias for a non-defined type), the package path
// will be the empty string.
    ptr<uncommonType> PkgPath(); // Size returns the number of bytes needed to store
// a value of the given type; it is analogous to unsafe.Sizeof.
    ptr<uncommonType> Size(); // Kind returns the specific kind of this type.
    ptr<uncommonType> Kind(); // Implements reports whether the type implements the interface type u.
    ptr<uncommonType> Implements(Type u); // AssignableTo reports whether a value of the type is assignable to type u.
    ptr<uncommonType> AssignableTo(Type u); // Comparable reports whether values of this type are comparable.
    ptr<uncommonType> Comparable(); // String returns a string representation of the type.
// The string representation may use shortened package names
// (e.g., base64 instead of "encoding/base64") and is not
// guaranteed to be unique among types. To test for type identity,
// compare the Types directly.
    ptr<uncommonType> String(); // Elem returns a type's element type.
// It panics if the type's Kind is not Ptr.
    ptr<uncommonType> Elem();
    ptr<uncommonType> common();
    ptr<uncommonType> uncommon();
}

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

// chanDir represents a channel type's direction.
private partial struct chanDir { // : nint
}

private static readonly chanDir recvDir = 1 << (int)(iota); // <-chan
private static readonly bothDir sendDir = recvDir | sendDir; // chan

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
    public System.UIntPtr dir; // channel direction (chanDir)
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
// The next two bytes are the data length:
//
//     l := uint16(data[1])<<8 | uint16(data[2])
//
// Bytes [3:3+l] are the string data.
//
// If tag data follows then bytes 3+l and 3+l+1 are the tag length,
// with the data following.
//
// If the import path follows, then 4 bytes at the end of
// the data form a nameOff. The import path is only set for concrete
// methods that are defined in a different package than their type.
//
// If a name starts with "*", then the exported bit represents
// whether the pointed to type is exported.
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
    for (nint i = 0; >>MARKER:FOREXPRESSION_LEVEL_1<<; i++) {
        var x = n.data(off + i, "read varint").val;
        v += int(x & 0x7f) << (int)((7 * i));
        if (x & 0x80 == 0) {
            return (i + 1, v);
        }
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

/*
 * The compiler knows the exact layout of all the data structures above.
 * The compiler does not know about the data structures and methods below.
 */

private static readonly nint kindDirectIface = 1 << 5;
private static readonly nint kindGCProg = 1 << 6; // Type.gc points to GC program
private static readonly nint kindMask = (1 << 5) - 1;


// String returns the name of k.
public static @string String(this Kind k) {
    if (int(k) < len(kindNames)) {
        return kindNames[k];
    }
    return kindNames[0];

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

private static ptr<uncommonType> uncommon(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    if (t.tflag & tflagUncommon == 0) {>>MARKER:FUNCTION_resolveTypeOff_BLOCK_PREFIX<<
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
    if (t.tflag & tflagExtraStar != 0) {>>MARKER:FUNCTION_resolveNameOff_BLOCK_PREFIX<<
        return s[(int)1..];
    }
    return s;

}

private static System.UIntPtr Size(this ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return t.size;
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

private static chanDir chanDir(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Chan) {
        panic("reflect: chanDir of non-chan type");
    }
    var tt = (chanType.val)(@unsafe.Pointer(t));
    return chanDir(tt.dir);

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
        panic("reflect: Elem of invalid type");

});

private static Type In(this ptr<rtype> _addr_t, nint i) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Func) {
        panic("reflect: In of non-func type");
    }
    var tt = (funcType.val)(@unsafe.Pointer(t));
    return toType(_addr_tt.@in()[i]);

});

private static Type Key(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Map) {
        panic("reflect: Key of non-map type");
    }
    var tt = (mapType.val)(@unsafe.Pointer(t));
    return toType(_addr_tt.key);

});

private static nint Len(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Array) {
        panic("reflect: Len of non-array type");
    }
    var tt = (arrayType.val)(@unsafe.Pointer(t));
    return int(tt.len);

});

private static nint NumField(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Struct) {
        panic("reflect: NumField of non-struct type");
    }
    var tt = (structType.val)(@unsafe.Pointer(t));
    return len(tt.fields);

});

private static nint NumIn(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Func) {
        panic("reflect: NumIn of non-func type");
    }
    var tt = (funcType.val)(@unsafe.Pointer(t));
    return int(tt.inCount);

});

private static nint NumOut(this ptr<rtype> _addr_t) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Func) {
        panic("reflect: NumOut of non-func type");
    }
    var tt = (funcType.val)(@unsafe.Pointer(t));
    return len(tt.@out());

});

private static Type Out(this ptr<rtype> _addr_t, nint i) => func((_, panic, _) => {
    ref rtype t = ref _addr_t.val;

    if (t.Kind() != Func) {
        panic("reflect: Out of non-func type");
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

// NumMethod returns the number of interface methods in the type's method set.
private static nint NumMethod(this ptr<interfaceType> _addr_t) {
    ref interfaceType t = ref _addr_t.val;

    return len(t.methods);
}

// TypeOf returns the reflection Type that represents the dynamic type of i.
// If i is a nil interface value, TypeOf returns nil.
public static Type TypeOf(object i) {
    ptr<ptr<emptyInterface>> eface = new ptr<ptr<ptr<emptyInterface>>>(@unsafe.Pointer(_addr_i));
    return toType(_addr_eface.typ);
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
    return haveIdenticalUnderlyingType(_addr_T, _addr_V, true);

}

private static bool haveIdenticalType(Type T, Type V, bool cmpTags) {
    if (cmpTags) {
        return T == V;
    }
    if (T.Name() != V.Name() || T.Kind() != V.Kind()) {
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
        // Special case:
        // x is a bidirectional channel value, T is a channel type,
        // and x's type V and T have identical element types.
        if (V.chanDir() == bothDir && haveIdenticalType(T.Elem(), V.Elem(), cmpTags)) {
            return true;
        }
        return V.chanDir() == T.chanDir() && haveIdenticalType(T.Elem(), V.Elem(), cmpTags);
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

private partial struct structTypeUncommon {
    public ref structType structType => ref structType_val;
    public uncommonType u;
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

// ifaceIndir reports whether t is stored indirectly in an interface value.
private static bool ifaceIndir(ptr<rtype> _addr_t) {
    ref rtype t = ref _addr_t.val;

    return t.kind & kindDirectIface == 0;
}

} // end reflectlite_package
