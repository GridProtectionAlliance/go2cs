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
// package reflect -- go2cs converted at 2020 August 29 08:43:13 UTC
// import "reflect" ==> using reflect = go.reflect_package
// Original source: C:\Go\src\reflect\type.go
using runtime = go.runtime_package;
using strconv = go.strconv_package;
using sync = go.sync_package;
using unicode = go.unicode_package;
using utf8 = go.unicode.utf8_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;
using System.ComponentModel;

namespace go
{
    public static unsafe partial class reflect_package
    {
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
        public partial interface Type
        {
            ref uncommonType Align(); // FieldAlign returns the alignment in bytes of a value of
// this type when used as a field in a struct.
            ref uncommonType FieldAlign(); // Method returns the i'th method in the type's method set.
// It panics if i is not in the range [0, NumMethod()).
//
// For a non-interface type T or *T, the returned Method's Type and Func
// fields describe a function whose first argument is the receiver.
//
// For an interface type, the returned Method's Type field gives the
// method signature, without a receiver, and the Func field is nil.
            ref uncommonType Method(long _p0); // MethodByName returns the method with that name in the type's
// method set and a boolean indicating if the method was found.
//
// For a non-interface type T or *T, the returned Method's Type and Func
// fields describe a function whose first argument is the receiver.
//
// For an interface type, the returned Method's Type field gives the
// method signature, without a receiver, and the Func field is nil.
            ref uncommonType MethodByName(@string _p0); // NumMethod returns the number of exported methods in the type's method set.
            ref uncommonType NumMethod(); // Name returns the type's name within its package.
// It returns an empty string for unnamed types.
            ref uncommonType Name(); // PkgPath returns a named type's package path, that is, the import path
// that uniquely identifies the package, such as "encoding/base64".
// If the type was predeclared (string, error) or unnamed (*T, struct{}, []int),
// the package path will be the empty string.
            ref uncommonType PkgPath(); // Size returns the number of bytes needed to store
// a value of the given type; it is analogous to unsafe.Sizeof.
            ref uncommonType Size(); // String returns a string representation of the type.
// The string representation may use shortened package names
// (e.g., base64 instead of "encoding/base64") and is not
// guaranteed to be unique among types. To test for type identity,
// compare the Types directly.
            ref uncommonType String(); // Kind returns the specific kind of this type.
            ref uncommonType Kind(); // Implements reports whether the type implements the interface type u.
            ref uncommonType Implements(Type u); // AssignableTo reports whether a value of the type is assignable to type u.
            ref uncommonType AssignableTo(Type u); // ConvertibleTo reports whether a value of the type is convertible to type u.
            ref uncommonType ConvertibleTo(Type u); // Comparable reports whether values of this type are comparable.
            ref uncommonType Comparable(); // Methods applicable only to some types, depending on Kind.
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
            ref uncommonType Bits(); // ChanDir returns a channel type's direction.
// It panics if the type's Kind is not Chan.
            ref uncommonType ChanDir(); // IsVariadic reports whether a function type's final input parameter
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
            ref uncommonType IsVariadic(); // Elem returns a type's element type.
// It panics if the type's Kind is not Array, Chan, Map, Ptr, or Slice.
            ref uncommonType Elem(); // Field returns a struct type's i'th field.
// It panics if the type's Kind is not Struct.
// It panics if i is not in the range [0, NumField()).
            ref uncommonType Field(long i); // FieldByIndex returns the nested field corresponding
// to the index sequence. It is equivalent to calling Field
// successively for each index i.
// It panics if the type's Kind is not Struct.
            ref uncommonType FieldByIndex(slice<long> index); // FieldByName returns the struct field with the given name
// and a boolean indicating if the field was found.
            ref uncommonType FieldByName(@string name); // FieldByNameFunc returns the struct field with a name
// that satisfies the match function and a boolean indicating if
// the field was found.
//
// FieldByNameFunc considers the fields in the struct itself
// and then the fields in any anonymous structs, in breadth first order,
// stopping at the shallowest nesting depth containing one or more
// fields satisfying the match function. If multiple fields at that depth
// satisfy the match function, they cancel each other
// and FieldByNameFunc returns no match.
// This behavior mirrors Go's handling of name lookup in
// structs containing anonymous fields.
            ref uncommonType FieldByNameFunc(Func<@string, bool> match); // In returns the type of a function type's i'th input parameter.
// It panics if the type's Kind is not Func.
// It panics if i is not in the range [0, NumIn()).
            ref uncommonType In(long i); // Key returns a map type's key type.
// It panics if the type's Kind is not Map.
            ref uncommonType Key(); // Len returns an array type's length.
// It panics if the type's Kind is not Array.
            ref uncommonType Len(); // NumField returns a struct type's field count.
// It panics if the type's Kind is not Struct.
            ref uncommonType NumField(); // NumIn returns a function type's input parameter count.
// It panics if the type's Kind is not Func.
            ref uncommonType NumIn(); // NumOut returns a function type's output parameter count.
// It panics if the type's Kind is not Func.
            ref uncommonType NumOut(); // Out returns the type of a function type's i'th output parameter.
// It panics if the type's Kind is not Func.
// It panics if i is not in the range [0, NumOut()).
            ref uncommonType Out(long i);
            ref uncommonType common();
            ref uncommonType uncommon();
        }

        // BUG(rsc): FieldByName and related functions consider struct field names to be equal
        // if the names are equal, even if they are unexported names originating
        // in different packages. The practical effect of this is that the result of
        // t.FieldByName("x") is not well defined if the struct type t contains
        // multiple fields named x (embedded from different packages).
        // FieldByName may return one of the fields named x or may report that there are none.
        // See https://golang.org/issue/4876 for more details.

        /*
         * These data structures are known to the compiler (../../cmd/internal/gc/reflect.go).
         * A few are known to ../runtime/type.go to convey to debuggers.
         * They are also known to ../runtime/type.go.
         */

        // A Kind represents the specific kind of type that a Type represents.
        // The zero Kind is not a valid kind.
        public partial struct Kind // : ulong
        {
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
        //    cmd/compile/internal/gc/reflect.go
        //    cmd/link/internal/ld/decodesym.go
        //    runtime/type.go
        private partial struct tflag // : byte
        {
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
        private static readonly tflag tflagUncommon = 1L << (int)(0L); 

        // tflagExtraStar means the name in the str field has an
        // extraneous '*' prefix. This is because for most types T in
        // a program, the type *T also exists and reusing the str data
        // saves binary size.
        private static readonly tflag tflagExtraStar = 1L << (int)(1L); 

        // tflagNamed means the type has a name.
        private static readonly tflag tflagNamed = 1L << (int)(2L);

        // rtype is the common implementation of most values.
        // It is embedded in other, public struct types, but always
        // with a unique tag like `reflect:"array"` or `reflect:"ptr"`
        // so that code cannot convert from, say, *arrayType to *ptrType.
        //
        // rtype must be kept in sync with ../runtime/type.go:/^type._type.
        private partial struct rtype
        {
            public System.UIntPtr size;
            public System.UIntPtr ptrdata; // number of bytes in the type that can contain pointers
            public uint hash; // hash of type; avoids computation in hash tables
            public tflag tflag; // extra type information flags
            public byte align; // alignment of variable with this type
            public byte fieldAlign; // alignment of struct field with this type
            public byte kind; // enumeration for C
            public ptr<typeAlg> alg; // algorithm table
            public ptr<byte> gcdata; // garbage collection data
            public nameOff str; // string form
            public typeOff ptrToThis; // type for pointer to this type, may be zero
        }

        // a copy of runtime.typeAlg
        private partial struct typeAlg
        {
            public Func<unsafe.Pointer, System.UIntPtr, System.UIntPtr> hash; // function for comparing objects of this type
// (ptr to object A, ptr to object B) -> ==?
            public Func<unsafe.Pointer, unsafe.Pointer, bool> equal;
        }

        // Method on non-interface type
        private partial struct method
        {
            public nameOff name; // name of method
            public typeOff mtyp; // method type (without receiver)
            public textOff ifn; // fn used in interface call (one-word receiver)
            public textOff tfn; // fn used for normal method call
        }

        // uncommonType is present only for types with names or methods
        // (if T is a named type, the uncommonTypes for T and *T have methods).
        // Using a pointer to this struct reduces the overall size required
        // to describe an unnamed type with no methods.
        private partial struct uncommonType
        {
            public nameOff pkgPath; // import path; empty for built-in types like int, string
            public ushort mcount; // number of methods
            public ushort _; // unused
            public uint moff; // offset from this uncommontype to [mcount]method
            public uint _; // unused
        }

        // ChanDir represents a channel type's direction.
        public partial struct ChanDir // : long
        {
        }

        public static readonly ChanDir RecvDir = 1L << (int)(iota); // <-chan
        public static readonly BothDir SendDir = RecvDir | SendDir; // chan

        // arrayType represents a fixed array type.
        private partial struct arrayType
        {
            [Description("reflect:\"array\"")]
            public ref rtype rtype => ref rtype_val;
            public ptr<rtype> elem; // array element type
            public ptr<rtype> slice; // slice type
            public System.UIntPtr len;
        }

        // chanType represents a channel type.
        private partial struct chanType
        {
            [Description("reflect:\"chan\"")]
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
        private partial struct funcType
        {
            [Description("reflect:\"func\"")]
            public ref rtype rtype => ref rtype_val;
            public ushort inCount;
            public ushort outCount; // top bit is set if last input parameter is ...
        }

        // imethod represents a method on an interface type
        private partial struct imethod
        {
            public nameOff name; // name of method
            public typeOff typ; // .(*FuncType) underneath
        }

        // interfaceType represents an interface type.
        private partial struct interfaceType
        {
            [Description("reflect:\"interface\"")]
            public ref rtype rtype => ref rtype_val;
            public name pkgPath; // import path
            public slice<imethod> methods; // sorted by hash
        }

        // mapType represents a map type.
        private partial struct mapType
        {
            [Description("reflect:\"map\"")]
            public ref rtype rtype => ref rtype_val;
            public ptr<rtype> key; // map key type
            public ptr<rtype> elem; // map element (value) type
            public ptr<rtype> bucket; // internal bucket structure
            public ptr<rtype> hmap; // internal map header
            public byte keysize; // size of key slot
            public byte indirectkey; // store ptr to key instead of key itself
            public byte valuesize; // size of value slot
            public byte indirectvalue; // store ptr to value instead of value itself
            public ushort bucketsize; // size of bucket
            public bool reflexivekey; // true if k==k for all keys
            public bool needkeyupdate; // true if we need to update key on an overwrite
        }

        // ptrType represents a pointer type.
        private partial struct ptrType
        {
            [Description("reflect:\"ptr\"")]
            public ref rtype rtype => ref rtype_val;
            public ptr<rtype> elem; // pointer element (pointed at) type
        }

        // sliceType represents a slice type.
        private partial struct sliceType
        {
            [Description("reflect:\"slice\"")]
            public ref rtype rtype => ref rtype_val;
            public ptr<rtype> elem; // slice element type
        }

        // Struct field
        private partial struct structField
        {
            public name name; // name is always non-empty
            public ptr<rtype> typ; // type of field
            public System.UIntPtr offsetAnon; // byte offset of field<<1 | isAnonymous
        }

        private static System.UIntPtr offset(this ref structField f)
        {
            return f.offsetAnon >> (int)(1L);
        }

        private static bool anon(this ref structField f)
        {
            return f.offsetAnon & 1L != 0L;
        }

        // structType represents a struct type.
        private partial struct structType
        {
            [Description("reflect:\"struct\"")]
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
        private partial struct name
        {
            public ptr<byte> bytes;
        }

        private static ref byte data(this name n, long off, @string whySafe)
        {
            return (byte.Value)(add(@unsafe.Pointer(n.bytes), uintptr(off), whySafe));
        }

        private static bool isExported(this name n)
        {
            return (n.bytes.Value) & (1L << (int)(0L)) != 0L;
        }

        private static long nameLen(this name n)
        {
            return int(uint16(n.data(1L, "name len field").Value) << (int)(8L) | uint16(n.data(2L, "name len field").Value));
        }

        private static long tagLen(this name n)
        {
            if (n.data(0L, "name flag field") & (1L << (int)(1L)) == 0L.Value)
            {
                return 0L;
            }
            long off = 3L + n.nameLen();
            return int(uint16(n.data(off, "name taglen field").Value) << (int)(8L) | uint16(n.data(off + 1L, "name taglen field").Value));
        }

        private static @string name(this name n)
        {
            if (n.bytes == null)
            {
                return;
            }
            ref array<byte> b = new ptr<ref array<byte>>(@unsafe.Pointer(n.bytes));

            var hdr = (stringHeader.Value)(@unsafe.Pointer(ref s));
            hdr.Data = @unsafe.Pointer(ref b[3L]);
            hdr.Len = int(b[1L]) << (int)(8L) | int(b[2L]);
            return s;
        }

        private static @string tag(this name n)
        {
            var tl = n.tagLen();
            if (tl == 0L)
            {
                return "";
            }
            var nl = n.nameLen();
            var hdr = (stringHeader.Value)(@unsafe.Pointer(ref s));
            hdr.Data = @unsafe.Pointer(n.data(3L + nl + 2L, "non-empty string"));
            hdr.Len = tl;
            return s;
        }

        private static @string pkgPath(this name n)
        {
            if (n.bytes == null || n.data(0L, "name flag field") & (1L << (int)(2L)) == 0L.Value)
            {
                return "";
            }
            long off = 3L + n.nameLen();
            {
                var tl = n.tagLen();

                if (tl > 0L)
                {
                    off += 2L + tl;
                }

            }
            int nameOff = default; 
            // Note that this field may not be aligned in memory,
            // so we cannot use a direct int32 assignment here.
            copy(new ptr<ref array<byte>>(@unsafe.Pointer(ref nameOff))[..], new ptr<ref array<byte>>(@unsafe.Pointer(n.data(off, "name offset field")))[..]);
            name pkgPathName = new name((*byte)(resolveTypeOff(unsafe.Pointer(n.bytes),nameOff)));
            return pkgPathName.name();
        }

        // round n up to a multiple of a.  a must be a power of 2.
        private static System.UIntPtr round(System.UIntPtr n, System.UIntPtr a)
        {
            return (n + a - 1L) & ~(a - 1L);
        }

        private static name newName(@string n, @string tag, bool exported) => func((_, panic, __) =>
        {
            if (len(n) > 1L << (int)(16L) - 1L)
            {
                panic("reflect.nameFrom: name too long: " + n);
            }
            if (len(tag) > 1L << (int)(16L) - 1L)
            {
                panic("reflect.nameFrom: tag too long: " + tag);
            }
            byte bits = default;
            long l = 1L + 2L + len(n);
            if (exported)
            {
                bits |= 1L << (int)(0L);
            }
            if (len(tag) > 0L)
            {
                l += 2L + len(tag);
                bits |= 1L << (int)(1L);
            }
            var b = make_slice<byte>(l);
            b[0L] = bits;
            b[1L] = uint8(len(n) >> (int)(8L));
            b[2L] = uint8(len(n));
            copy(b[3L..], n);
            if (len(tag) > 0L)
            {
                var tb = b[3L + len(n)..];
                tb[0L] = uint8(len(tag) >> (int)(8L));
                tb[1L] = uint8(len(tag));
                copy(tb[2L..], tag);
            }
            return new name(bytes:&b[0]);
        });

        /*
         * The compiler knows the exact layout of all the data structures above.
         * The compiler does not know about the data structures and methods below.
         */

        // Method represents a single method.
        public partial struct Method
        {
            public @string Name;
            public @string PkgPath;
            public Type Type; // method type
            public Value Func; // func with receiver as first argument
            public long Index; // index for Type.Method
        }

        private static readonly long kindDirectIface = 1L << (int)(5L);
        private static readonly long kindGCProg = 1L << (int)(6L); // Type.gc points to GC program
        private static readonly long kindNoPointers = 1L << (int)(7L);
        private static readonly long kindMask = (1L << (int)(5L)) - 1L;

        public static @string String(this Kind k)
        {
            if (int(k) < len(kindNames))
            {
                return kindNames[k];
            }
            return "kind" + strconv.Itoa(int(k));
        }

        private static @string kindNames = new slice<@string>(InitKeyedValues<@string>((Invalid, "invalid"), (Bool, "bool"), (Int, "int"), (Int8, "int8"), (Int16, "int16"), (Int32, "int32"), (Int64, "int64"), (Uint, "uint"), (Uint8, "uint8"), (Uint16, "uint16"), (Uint32, "uint32"), (Uint64, "uint64"), (Uintptr, "uintptr"), (Float32, "float32"), (Float64, "float64"), (Complex64, "complex64"), (Complex128, "complex128"), (Array, "array"), (Chan, "chan"), (Func, "func"), (Interface, "interface"), (Map, "map"), (Ptr, "ptr"), (Slice, "slice"), (String, "string"), (Struct, "struct"), (UnsafePointer, "unsafe.Pointer")));

        private static slice<method> methods(this ref uncommonType t)
        {
            if (t.mcount == 0L)
            {
                return null;
            }
            return new ptr<ref array<method>>(add(@unsafe.Pointer(t), uintptr(t.moff), "t.mcount > 0")).slice(-1, t.mcount, t.mcount);
        }

        // resolveNameOff resolves a name offset from a base pointer.
        // The (*rtype).nameOff method is a convenience wrapper for this function.
        // Implemented in the runtime package.
        private static unsafe.Pointer resolveNameOff(unsafe.Pointer ptrInModule, int off)
;

        // resolveTypeOff resolves an *rtype offset from a base type.
        // The (*rtype).typeOff method is a convenience wrapper for this function.
        // Implemented in the runtime package.
        private static unsafe.Pointer resolveTypeOff(unsafe.Pointer rtype, int off)
;

        // resolveTextOff resolves an function pointer offset from a base type.
        // The (*rtype).textOff method is a convenience wrapper for this function.
        // Implemented in the runtime package.
        private static unsafe.Pointer resolveTextOff(unsafe.Pointer rtype, int off)
;

        // addReflectOff adds a pointer to the reflection lookup map in the runtime.
        // It returns a new ID that can be used as a typeOff or textOff, and will
        // be resolved correctly. Implemented in the runtime package.
        private static int addReflectOff(unsafe.Pointer ptr)
;

        // resolveReflectType adds a name to the reflection lookup map in the runtime.
        // It returns a new nameOff that can be used to refer to the pointer.
        private static nameOff resolveReflectName(name n)
        {
            return nameOff(addReflectOff(@unsafe.Pointer(n.bytes)));
        }

        // resolveReflectType adds a *rtype to the reflection lookup map in the runtime.
        // It returns a new typeOff that can be used to refer to the pointer.
        private static typeOff resolveReflectType(ref rtype t)
        {
            return typeOff(addReflectOff(@unsafe.Pointer(t)));
        }

        // resolveReflectText adds a function pointer to the reflection lookup map in
        // the runtime. It returns a new textOff that can be used to refer to the
        // pointer.
        private static textOff resolveReflectText(unsafe.Pointer ptr)
        {
            return textOff(addReflectOff(ptr));
        }

        private partial struct nameOff // : int
        {
        } // offset to a name
        private partial struct typeOff // : int
        {
        } // offset to an *rtype
        private partial struct textOff // : int
        {
        } // offset from top of text section

        private static name nameOff(this ref rtype t, nameOff off)
        {>>MARKER:FUNCTION_addReflectOff_BLOCK_PREFIX<<
            return new name((*byte)(resolveNameOff(unsafe.Pointer(t),int32(off))));
        }

        private static ref rtype typeOff(this ref rtype t, typeOff off)
        {>>MARKER:FUNCTION_resolveTextOff_BLOCK_PREFIX<<
            return (rtype.Value)(resolveTypeOff(@unsafe.Pointer(t), int32(off)));
        }

        private static unsafe.Pointer textOff(this ref rtype t, textOff off)
        {>>MARKER:FUNCTION_resolveTypeOff_BLOCK_PREFIX<<
            return resolveTextOff(@unsafe.Pointer(t), int32(off));
        }

        private static ref uncommonType uncommon(this ref rtype t)
        {>>MARKER:FUNCTION_resolveNameOff_BLOCK_PREFIX<<
            if (t.tflag & tflagUncommon == 0L)
            {
                return null;
            }

            if (t.Kind() == Struct) 
                return ref (structTypeUncommon.Value)(@unsafe.Pointer(t)).u;
            else if (t.Kind() == Ptr) 
                private partial struct u
                {
                    public ref ptrType ptrType => ref ptrType_val;
                    public uncommonType u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.Kind() == Func) 
                private partial struct u
                {
                    public ref ptrType ptrType => ref ptrType_val;
                    public uncommonType u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.Kind() == Slice) 
                private partial struct u
                {
                    public ref ptrType ptrType => ref ptrType_val;
                    public uncommonType u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.Kind() == Array) 
                private partial struct u
                {
                    public ref ptrType ptrType => ref ptrType_val;
                    public uncommonType u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.Kind() == Chan) 
                private partial struct u
                {
                    public ref ptrType ptrType => ref ptrType_val;
                    public uncommonType u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.Kind() == Map) 
                private partial struct u
                {
                    public ref ptrType ptrType => ref ptrType_val;
                    public uncommonType u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else if (t.Kind() == Interface) 
                private partial struct u
                {
                    public ref ptrType ptrType => ref ptrType_val;
                    public uncommonType u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
            else 
                private partial struct u
                {
                    public ref ptrType ptrType => ref ptrType_val;
                    public uncommonType u;
                }
                return ref (u.Value)(@unsafe.Pointer(t)).u;
                    }

        private static @string String(this ref rtype t)
        {
            var s = t.nameOff(t.str).name();
            if (t.tflag & tflagExtraStar != 0L)
            {
                return s[1L..];
            }
            return s;
        }

        private static System.UIntPtr Size(this ref rtype t)
        {
            return t.size;
        }

        private static long Bits(this ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t == null)
            {
                panic("reflect: Bits of nil Type");
            }
            var k = t.Kind();
            if (k < Int || k > Complex128)
            {
                panic("reflect: Bits of non-arithmetic Type " + t.String());
            }
            return int(t.size) * 8L;
        });

        private static long Align(this ref rtype t)
        {
            return int(t.align);
        }

        private static long FieldAlign(this ref rtype t)
        {
            return int(t.fieldAlign);
        }

        private static Kind Kind(this ref rtype t)
        {
            return Kind(t.kind & kindMask);
        }

        private static bool pointers(this ref rtype t)
        {
            return t.kind & kindNoPointers == 0L;
        }

        private static ref rtype common(this ref rtype t)
        {
            return t;
        }

        private static sync.Map methodCache = default; // map[*rtype][]method

        private static slice<method> exportedMethods(this ref rtype t)
        {
            var (methodsi, found) = methodCache.Load(t);
            if (found)
            {
                return methodsi._<slice<method>>();
            }
            var ut = t.uncommon();
            if (ut == null)
            {
                return null;
            }
            var allm = ut.methods();
            var allExported = true;
            {
                var m__prev1 = m;

                foreach (var (_, __m) in allm)
                {
                    m = __m;
                    var name = t.nameOff(m.name);
                    if (!name.isExported())
                    {
                        allExported = false;
                        break;
                    }
                }

                m = m__prev1;
            }

            slice<method> methods = default;
            if (allExported)
            {
                methods = allm;
            }
            else
            {
                methods = make_slice<method>(0L, len(allm));
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in allm)
                    {
                        m = __m;
                        name = t.nameOff(m.name);
                        if (name.isExported())
                        {
                            methods = append(methods, m);
                        }
                    }

                    m = m__prev1;
                }

                methods = methods.slice(-1, len(methods), len(methods));
            }
            methodsi, _ = methodCache.LoadOrStore(t, methods);
            return methodsi._<slice<method>>();
        }

        private static long NumMethod(this ref rtype t)
        {
            if (t.Kind() == Interface)
            {
                var tt = (interfaceType.Value)(@unsafe.Pointer(t));
                return tt.NumMethod();
            }
            if (t.tflag & tflagUncommon == 0L)
            {
                return 0L; // avoid methodCache synchronization
            }
            return len(t.exportedMethods());
        }

        private static Method Method(this ref rtype _t, long i) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() == Interface)
            {
                var tt = (interfaceType.Value)(@unsafe.Pointer(t));
                return tt.Method(i);
            }
            var methods = t.exportedMethods();
            if (i < 0L || i >= len(methods))
            {
                panic("reflect: Method index out of range");
            }
            var p = methods[i];
            var pname = t.nameOff(p.name);
            m.Name = pname.name();
            var fl = flag(Func);
            var mtyp = t.typeOff(p.mtyp);
            var ft = (funcType.Value)(@unsafe.Pointer(mtyp));
            var @in = make_slice<Type>(0L, 1L + len(ft.@in()));
            in = append(in, t);
            foreach (var (_, arg) in ft.@in())
            {
                in = append(in, arg);
            }
            var @out = make_slice<Type>(0L, len(ft.@out()));
            foreach (var (_, ret) in ft.@out())
            {
                out = append(out, ret);
            }
            var mt = FuncOf(in, out, ft.IsVariadic());
            m.Type = mt;
            var tfn = t.textOff(p.tfn);
            var fn = @unsafe.Pointer(ref tfn);
            m.Func = new Value(mt.(*rtype),fn,fl);

            m.Index = i;
            return m;
        });

        private static (Method, bool) MethodByName(this ref rtype t, @string name)
        {
            if (t.Kind() == Interface)
            {
                var tt = (interfaceType.Value)(@unsafe.Pointer(t));
                return tt.MethodByName(name);
            }
            var ut = t.uncommon();
            if (ut == null)
            {
                return (new Method(), false);
            }
            var utmethods = ut.methods();
            long eidx = default;
            for (long i = 0L; i < int(ut.mcount); i++)
            {
                var p = utmethods[i];
                var pname = t.nameOff(p.name);
                if (pname.isExported())
                {
                    if (pname.name() == name)
                    {
                        return (t.Method(eidx), true);
                    }
                    eidx++;
                }
            }

            return (new Method(), false);
        }

        private static @string PkgPath(this ref rtype t)
        {
            if (t.tflag & tflagNamed == 0L)
            {
                return "";
            }
            var ut = t.uncommon();
            if (ut == null)
            {
                return "";
            }
            return t.nameOff(ut.pkgPath).name();
        }

        private static bool hasPrefix(@string s, @string prefix)
        {
            return len(s) >= len(prefix) && s[..len(prefix)] == prefix;
        }

        private static @string Name(this ref rtype t)
        {
            if (t.tflag & tflagNamed == 0L)
            {
                return "";
            }
            var s = t.String();
            var i = len(s) - 1L;
            while (i >= 0L)
            {
                if (s[i] == '.')
                {
                    break;
                }
                i--;
            }

            return s[i + 1L..];
        }

        private static ChanDir ChanDir(this ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Chan)
            {
                panic("reflect: ChanDir of non-chan type");
            }
            var tt = (chanType.Value)(@unsafe.Pointer(t));
            return ChanDir(tt.dir);
        });

        private static bool IsVariadic(this ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Func)
            {
                panic("reflect: IsVariadic of non-func type");
            }
            var tt = (funcType.Value)(@unsafe.Pointer(t));
            return tt.outCount & (1L << (int)(15L)) != 0L;
        });

        private static Type Elem(this ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {

            if (t.Kind() == Array) 
                var tt = (arrayType.Value)(@unsafe.Pointer(t));
                return toType(tt.elem);
            else if (t.Kind() == Chan) 
                tt = (chanType.Value)(@unsafe.Pointer(t));
                return toType(tt.elem);
            else if (t.Kind() == Map) 
                tt = (mapType.Value)(@unsafe.Pointer(t));
                return toType(tt.elem);
            else if (t.Kind() == Ptr) 
                tt = (ptrType.Value)(@unsafe.Pointer(t));
                return toType(tt.elem);
            else if (t.Kind() == Slice) 
                tt = (sliceType.Value)(@unsafe.Pointer(t));
                return toType(tt.elem);
                        panic("reflect: Elem of invalid type");
        });

        private static StructField Field(this ref rtype _t, long i) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Struct)
            {
                panic("reflect: Field of non-struct type");
            }
            var tt = (structType.Value)(@unsafe.Pointer(t));
            return tt.Field(i);
        });

        private static StructField FieldByIndex(this ref rtype _t, slice<long> index) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Struct)
            {
                panic("reflect: FieldByIndex of non-struct type");
            }
            var tt = (structType.Value)(@unsafe.Pointer(t));
            return tt.FieldByIndex(index);
        });

        private static (StructField, bool) FieldByName(this ref rtype _t, @string name) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Struct)
            {
                panic("reflect: FieldByName of non-struct type");
            }
            var tt = (structType.Value)(@unsafe.Pointer(t));
            return tt.FieldByName(name);
        });

        private static (StructField, bool) FieldByNameFunc(this ref rtype _t, Func<@string, bool> match) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Struct)
            {
                panic("reflect: FieldByNameFunc of non-struct type");
            }
            var tt = (structType.Value)(@unsafe.Pointer(t));
            return tt.FieldByNameFunc(match);
        });

        private static Type In(this ref rtype _t, long i) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Func)
            {
                panic("reflect: In of non-func type");
            }
            var tt = (funcType.Value)(@unsafe.Pointer(t));
            return toType(tt.@in()[i]);
        });

        private static Type Key(this ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Map)
            {
                panic("reflect: Key of non-map type");
            }
            var tt = (mapType.Value)(@unsafe.Pointer(t));
            return toType(tt.key);
        });

        private static long Len(this ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Array)
            {
                panic("reflect: Len of non-array type");
            }
            var tt = (arrayType.Value)(@unsafe.Pointer(t));
            return int(tt.len);
        });

        private static long NumField(this ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Struct)
            {
                panic("reflect: NumField of non-struct type");
            }
            var tt = (structType.Value)(@unsafe.Pointer(t));
            return len(tt.fields);
        });

        private static long NumIn(this ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Func)
            {
                panic("reflect: NumIn of non-func type");
            }
            var tt = (funcType.Value)(@unsafe.Pointer(t));
            return int(tt.inCount);
        });

        private static long NumOut(this ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Func)
            {
                panic("reflect: NumOut of non-func type");
            }
            var tt = (funcType.Value)(@unsafe.Pointer(t));
            return len(tt.@out());
        });

        private static Type Out(this ref rtype _t, long i) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Func)
            {
                panic("reflect: Out of non-func type");
            }
            var tt = (funcType.Value)(@unsafe.Pointer(t));
            return toType(tt.@out()[i]);
        });

        private static slice<ref rtype> @in(this ref funcType t)
        {
            var uadd = @unsafe.Sizeof(t.Value);
            if (t.tflag & tflagUncommon != 0L)
            {
                uadd += @unsafe.Sizeof(new uncommonType());
            }
            if (t.inCount == 0L)
            {
                return null;
            }
            return new ptr<ref array<ref rtype>>(add(@unsafe.Pointer(t), uadd, "t.inCount > 0"))[..t.inCount];
        }

        private static slice<ref rtype> @out(this ref funcType t)
        {
            var uadd = @unsafe.Sizeof(t.Value);
            if (t.tflag & tflagUncommon != 0L)
            {
                uadd += @unsafe.Sizeof(new uncommonType());
            }
            var outCount = t.outCount & (1L << (int)(15L) - 1L);
            if (outCount == 0L)
            {
                return null;
            }
            return new ptr<ref array<ref rtype>>(add(@unsafe.Pointer(t), uadd, "outCount > 0"))[t.inCount..t.inCount + outCount];
        }

        // add returns p+x.
        //
        // The whySafe string is ignored, so that the function still inlines
        // as efficiently as p+x, but all call sites should use the string to
        // record why the addition is safe, which is to say why the addition
        // does not cause x to advance to the very end of p's allocation
        // and therefore point incorrectly at the next block in memory.
        private static unsafe.Pointer add(unsafe.Pointer p, System.UIntPtr x, @string whySafe)
        {
            return @unsafe.Pointer(uintptr(p) + x);
        }

        public static @string String(this ChanDir d)
        {

            if (d == SendDir) 
                return "chan<-";
            else if (d == RecvDir) 
                return "<-chan";
            else if (d == BothDir) 
                return "chan";
                        return "ChanDir" + strconv.Itoa(int(d));
        }

        // Method returns the i'th method in the type's method set.
        private static Method Method(this ref interfaceType t, long i)
        {
            if (i < 0L || i >= len(t.methods))
            {
                return;
            }
            var p = ref t.methods[i];
            var pname = t.nameOff(p.name);
            m.Name = pname.name();
            if (!pname.isExported())
            {
                m.PkgPath = pname.pkgPath();
                if (m.PkgPath == "")
                {
                    m.PkgPath = t.pkgPath.name();
                }
            }
            m.Type = toType(t.typeOff(p.typ));
            m.Index = i;
            return;
        }

        // NumMethod returns the number of interface methods in the type's method set.
        private static long NumMethod(this ref interfaceType t)
        {
            return len(t.methods);
        }

        // MethodByName method with the given name in the type's method set.
        private static (Method, bool) MethodByName(this ref interfaceType t, @string name)
        {
            if (t == null)
            {
                return;
            }
            ref imethod p = default;
            foreach (var (i) in t.methods)
            {
                p = ref t.methods[i];
                if (t.nameOff(p.name).name() == name)
                {
                    return (t.Method(i), true);
                }
            }
            return;
        }

        // A StructField describes a single field in a struct.
        public partial struct StructField
        {
            public @string Name; // PkgPath is the package path that qualifies a lower case (unexported)
// field name. It is empty for upper case (exported) field names.
// See https://golang.org/ref/spec#Uniqueness_of_identifiers
            public @string PkgPath;
            public Type Type; // field type
            public StructTag Tag; // field tag string
            public System.UIntPtr Offset; // offset within struct, in bytes
            public slice<long> Index; // index sequence for Type.FieldByIndex
            public bool Anonymous; // is an embedded field
        }

        // A StructTag is the tag string in a struct field.
        //
        // By convention, tag strings are a concatenation of
        // optionally space-separated key:"value" pairs.
        // Each key is a non-empty string consisting of non-control
        // characters other than space (U+0020 ' '), quote (U+0022 '"'),
        // and colon (U+003A ':').  Each value is quoted using U+0022 '"'
        // characters and Go string literal syntax.
        public partial struct StructTag // : @string
        {
        }

        // Get returns the value associated with key in the tag string.
        // If there is no such key in the tag, Get returns the empty string.
        // If the tag does not have the conventional format, the value
        // returned by Get is unspecified. To determine whether a tag is
        // explicitly set to the empty string, use Lookup.
        public static @string Get(this StructTag tag, @string key)
        {
            var (v, _) = tag.Lookup(key);
            return v;
        }

        // Lookup returns the value associated with key in the tag string.
        // If the key is present in the tag the value (which may be empty)
        // is returned. Otherwise the returned value will be the empty string.
        // The ok return value reports whether the value was explicitly set in
        // the tag string. If the tag does not have the conventional format,
        // the value returned by Lookup is unspecified.
        public static (@string, bool) Lookup(this StructTag tag, @string key)
        { 
            // When modifying this code, also update the validateStructTag code
            // in cmd/vet/structtag.go.

            while (tag != "")
            { 
                // Skip leading space.
                long i = 0L;
                while (i < len(tag) && tag[i] == ' ')
                {
                    i++;
                }

                tag = tag[i..];
                if (tag == "")
                {
                    break;
                } 

                // Scan to colon. A space, a quote or a control character is a syntax error.
                // Strictly speaking, control chars include the range [0x7f, 0x9f], not just
                // [0x00, 0x1f], but in practice, we ignore the multi-byte control characters
                // as it is simpler to inspect the tag's bytes than the tag's runes.
                i = 0L;
                while (i < len(tag) && tag[i] > ' ' && tag[i] != ':' && tag[i] != '"' && tag[i] != 0x7fUL)
                {
                    i++;
                }

                if (i == 0L || i + 1L >= len(tag) || tag[i] != ':' || tag[i + 1L] != '"')
                {
                    break;
                }
                var name = string(tag[..i]);
                tag = tag[i + 1L..]; 

                // Scan quoted string to find value.
                i = 1L;
                while (i < len(tag) && tag[i] != '"')
                {
                    if (tag[i] == '\\')
                    {
                        i++;
                    }
                    i++;
                }

                if (i >= len(tag))
                {
                    break;
                }
                var qvalue = string(tag[..i + 1L]);
                tag = tag[i + 1L..];

                if (key == name)
                {
                    var (value, err) = strconv.Unquote(qvalue);
                    if (err != null)
                    {
                        break;
                    }
                    return (value, true);
                }
            }

            return ("", false);
        }

        // Field returns the i'th struct field.
        private static StructField Field(this ref structType _t, long i) => func(_t, (ref structType t, Defer _, Panic panic, Recover __) =>
        {
            if (i < 0L || i >= len(t.fields))
            {
                panic("reflect: Field index out of bounds");
            }
            var p = ref t.fields[i];
            f.Type = toType(p.typ);
            f.Name = p.name.name();
            f.Anonymous = p.anon();
            if (!p.name.isExported())
            {
                f.PkgPath = t.pkgPath.name();
            }
            {
                var tag = p.name.tag();

                if (tag != "")
                {
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
            f.Index = new slice<long>(new long[] { i });
            return;
        });

        // TODO(gri): Should there be an error/bool indicator if the index
        //            is wrong for FieldByIndex?

        // FieldByIndex returns the nested field corresponding to index.
        private static StructField FieldByIndex(this ref structType t, slice<long> index)
        {
            f.Type = toType(ref t.rtype);
            foreach (var (i, x) in index)
            {
                if (i > 0L)
                {
                    var ft = f.Type;
                    if (ft.Kind() == Ptr && ft.Elem().Kind() == Struct)
                    {
                        ft = ft.Elem();
                    }
                    f.Type = ft;
                }
                f = f.Type.Field(x);
            }
            return;
        }

        // A fieldScan represents an item on the fieldByNameFunc scan work list.
        private partial struct fieldScan
        {
            public ptr<structType> typ;
            public slice<long> index;
        }

        // FieldByNameFunc returns the struct field with a name that satisfies the
        // match function and a boolean to indicate if the field was found.
        private static (StructField, bool) FieldByNameFunc(this ref structType t, Func<@string, bool> match)
        { 
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
            map<ref structType, long> nextCount = default; 

            // visited records the structs that have been considered already.
            // Embedded pointer fields can create cycles in the graph of
            // reachable embedded types; visited avoids following those cycles.
            // It also avoids duplicated effort: if we didn't find the field in an
            // embedded type T at level 2, we won't find it in one at level 4 either.
            map visited = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref structType, bool>{};

            while (len(next) > 0L)
            {
                current = next;
                next = current[..0L];
                var count = nextCount;
                nextCount = null; 

                // Process all the fields at this depth, now listed in 'current'.
                // The loop queues embedded fields found in 'next', for processing during the next
                // iteration. The multiplicity of the 'current' field counts is recorded
                // in 'count'; the multiplicity of the 'next' field counts is recorded in 'nextCount'.
                foreach (var (_, scan) in current)
                {
                    var t = scan.typ;
                    if (visited[t])
                    { 
                        // We've looked through this type before, at a higher level.
                        // That higher level would shadow the lower level we're now at,
                        // so this one can't be useful to us. Ignore it.
                        continue;
                    }
                    visited[t] = true;
                    foreach (var (i) in t.fields)
                    {
                        var f = ref t.fields[i]; 
                        // Find name and (for anonymous field) type for field f.
                        var fname = f.name.name();
                        ref rtype ntyp = default;
                        if (f.anon())
                        { 
                            // Anonymous field of type T or *T.
                            ntyp = f.typ;
                            if (ntyp.Kind() == Ptr)
                            {
                                ntyp = ntyp.Elem().common();
                            }
                        } 

                        // Does it match?
                        if (match(fname))
                        { 
                            // Potential match
                            if (count[t] > 1L || ok)
                            { 
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
                        if (ok || ntyp == null || ntyp.Kind() != Struct)
                        {
                            continue;
                        }
                        var styp = (structType.Value)(@unsafe.Pointer(ntyp));
                        if (nextCount[styp] > 0L)
                        {
                            nextCount[styp] = 2L; // exact multiple doesn't matter
                            continue;
                        }
                        if (nextCount == null)
                        {
                            nextCount = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ref structType, long>{};
                        }
                        nextCount[styp] = 1L;
                        if (count[t] > 1L)
                        {
                            nextCount[styp] = 2L; // exact multiple doesn't matter
                        }
                        slice<long> index = default;
                        index = append(index, scan.index);
                        index = append(index, i);
                        next = append(next, new fieldScan(styp,index));
                    }
                }
                if (ok)
                {
                    break;
                }
            }

            return;
        }

        // FieldByName returns the struct field with the given name
        // and a boolean to indicate if the field was found.
        private static (StructField, bool) FieldByName(this ref structType t, @string name)
        { 
            // Quick check for top-level name, or struct without anonymous fields.
            var hasAnon = false;
            if (name != "")
            {
                foreach (var (i) in t.fields)
                {
                    var tf = ref t.fields[i];
                    if (tf.name.name() == name)
                    {
                        return (t.Field(i), true);
                    }
                    if (tf.anon())
                    {
                        hasAnon = true;
                    }
                }
            }
            if (!hasAnon)
            {
                return;
            }
            return t.FieldByNameFunc(s => s == name);
        }

        // TypeOf returns the reflection Type that represents the dynamic type of i.
        // If i is a nil interface value, TypeOf returns nil.
        public static Type TypeOf(object i)
        {
            *(*emptyInterface) eface = @unsafe.Pointer(ref i).Value;
            return toType(eface.typ);
        }

        // ptrMap is the cache for PtrTo.
        private static sync.Map ptrMap = default; // map[*rtype]*ptrType

        // PtrTo returns the pointer type with element t.
        // For example, if t represents type Foo, PtrTo(t) represents *Foo.
        public static Type PtrTo(Type t)
        {
            return t._<ref rtype>().ptrTo();
        }

        private static ref rtype ptrTo(this ref rtype t)
        {
            if (t.ptrToThis != 0L)
            {
                return t.typeOff(t.ptrToThis);
            } 

            // Check the cache.
            {
                var pi__prev1 = pi;

                var (pi, ok) = ptrMap.Load(t);

                if (ok)
                {
                    return ref pi._<ref ptrType>().rtype;
                } 

                // Look in known types.

                pi = pi__prev1;

            } 

            // Look in known types.
            @string s = "*" + t.String();
            foreach (var (_, tt) in typesByString(s))
            {
                var p = (ptrType.Value)(@unsafe.Pointer(tt));
                if (p.elem != t)
                {
                    continue;
                }
                var (pi, _) = ptrMap.LoadOrStore(t, p);
                return ref pi._<ref ptrType>().rtype;
            } 

            // Create a new ptrType starting with the description
            // of an *unsafe.Pointer.
            var iptr = (@unsafe.Pointer.Value)(null);
            *(ptr<ptr<ptrType>>) prototype = new ptr<*(ptr<ptr<ptrType>>)>(@unsafe.Pointer(ref iptr));
            var pp = prototype.Value;

            pp.str = resolveReflectName(newName(s, "", false));
            pp.ptrToThis = 0L; 

            // For the type structures linked into the binary, the
            // compiler provides a good hash of the string.
            // Create a good hash for the new string by using
            // the FNV-1 hash's mixing function to combine the
            // old hash and the new "*".
            pp.hash = fnv1(t.hash, '*');

            pp.elem = t;

            (pi, _) = ptrMap.LoadOrStore(t, ref pp);
            return ref pi._<ref ptrType>().rtype;
        }

        // fnv1 incorporates the list of bytes into the hash x using the FNV-1 hash function.
        private static uint fnv1(uint x, params byte[] list)
        {
            list = list.Clone();

            foreach (var (_, b) in list)
            {
                x = x * 16777619L ^ uint32(b);
            }
            return x;
        }

        private static bool Implements(this ref rtype _t, Type u) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (u == null)
            {
                panic("reflect: nil type passed to Type.Implements");
            }
            if (u.Kind() != Interface)
            {
                panic("reflect: non-interface type passed to Type.Implements");
            }
            return implements(u._<ref rtype>(), t);
        });

        private static bool AssignableTo(this ref rtype _t, Type u) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (u == null)
            {
                panic("reflect: nil type passed to Type.AssignableTo");
            }
            ref rtype uu = u._<ref rtype>();
            return directlyAssignable(uu, t) || implements(uu, t);
        });

        private static bool ConvertibleTo(this ref rtype _t, Type u) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (u == null)
            {
                panic("reflect: nil type passed to Type.ConvertibleTo");
            }
            ref rtype uu = u._<ref rtype>();
            return convertOp(uu, t) != null;
        });

        private static bool Comparable(this ref rtype t)
        {
            return t.alg != null && t.alg.equal != null;
        }

        // implements reports whether the type V implements the interface type T.
        private static bool implements(ref rtype T, ref rtype V)
        {
            if (T.Kind() != Interface)
            {
                return false;
            }
            var t = (interfaceType.Value)(@unsafe.Pointer(T));
            if (len(t.methods) == 0L)
            {
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
            if (V.Kind() == Interface)
            {
                var v = (interfaceType.Value)(@unsafe.Pointer(V));
                long i = 0L;
                {
                    long j__prev1 = j;

                    for (long j = 0L; j < len(v.methods); j++)
                    {
                        var tm = ref t.methods[i];
                        var tmName = t.nameOff(tm.name);
                        var vm = ref v.methods[j];
                        var vmName = V.nameOff(vm.name);
                        if (vmName.name() == tmName.name() && V.typeOff(vm.typ) == t.typeOff(tm.typ))
                        {
                            if (!tmName.isExported())
                            {
                                var tmPkgPath = tmName.pkgPath();
                                if (tmPkgPath == "")
                                {
                                    tmPkgPath = t.pkgPath.name();
                                }
                                var vmPkgPath = vmName.pkgPath();
                                if (vmPkgPath == "")
                                {
                                    vmPkgPath = v.pkgPath.name();
                                }
                                if (tmPkgPath != vmPkgPath)
                                {
                                    continue;
                                }
                            }
                            i++;

                            if (i >= len(t.methods))
                            {
                                return true;
                            }
                        }
                    }


                    j = j__prev1;
                }
                return false;
            }
            v = V.uncommon();
            if (v == null)
            {
                return false;
            }
            i = 0L;
            var vmethods = v.methods();
            {
                long j__prev1 = j;

                for (j = 0L; j < int(v.mcount); j++)
                {
                    tm = ref t.methods[i];
                    tmName = t.nameOff(tm.name);
                    vm = vmethods[j];
                    vmName = V.nameOff(vm.name);
                    if (vmName.name() == tmName.name() && V.typeOff(vm.mtyp) == t.typeOff(tm.typ))
                    {
                        if (!tmName.isExported())
                        {
                            tmPkgPath = tmName.pkgPath();
                            if (tmPkgPath == "")
                            {
                                tmPkgPath = t.pkgPath.name();
                            }
                            vmPkgPath = vmName.pkgPath();
                            if (vmPkgPath == "")
                            {
                                vmPkgPath = V.nameOff(v.pkgPath).name();
                            }
                            if (tmPkgPath != vmPkgPath)
                            {
                                continue;
                            }
                        }
                        i++;

                        if (i >= len(t.methods))
                        {
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
        private static bool directlyAssignable(ref rtype T, ref rtype V)
        { 
            // x's type V is identical to T?
            if (T == V)
            {
                return true;
            } 

            // Otherwise at least one of T and V must be unnamed
            // and they must have the same kind.
            if (T.Name() != "" && V.Name() != "" || T.Kind() != V.Kind())
            {
                return false;
            } 

            // x's type T and V must  have identical underlying types.
            return haveIdenticalUnderlyingType(T, V, true);
        }

        private static bool haveIdenticalType(Type T, Type V, bool cmpTags)
        {
            if (cmpTags)
            {
                return T == V;
            }
            if (T.Name() != V.Name() || T.Kind() != V.Kind())
            {
                return false;
            }
            return haveIdenticalUnderlyingType(T.common(), V.common(), false);
        }

        private static bool haveIdenticalUnderlyingType(ref rtype T, ref rtype V, bool cmpTags)
        {
            if (T == V)
            {
                return true;
            }
            var kind = T.Kind();
            if (kind != V.Kind())
            {
                return false;
            } 

            // Non-composite types of equal kind have same underlying type
            // (the predefined instance of the type).
            if (Bool <= kind && kind <= Complex128 || kind == String || kind == UnsafePointer)
            {
                return true;
            } 

            // Composite types.

            if (kind == Array) 
                return T.Len() == V.Len() && haveIdenticalType(T.Elem(), V.Elem(), cmpTags);
            else if (kind == Chan) 
                // Special case:
                // x is a bidirectional channel value, T is a channel type,
                // and x's type V and T have identical element types.
                if (V.ChanDir() == BothDir && haveIdenticalType(T.Elem(), V.Elem(), cmpTags))
                {
                    return true;
                } 

                // Otherwise continue test for identical underlying type.
                return V.ChanDir() == T.ChanDir() && haveIdenticalType(T.Elem(), V.Elem(), cmpTags);
            else if (kind == Func) 
                var t = (funcType.Value)(@unsafe.Pointer(T));
                var v = (funcType.Value)(@unsafe.Pointer(V));
                if (t.outCount != v.outCount || t.inCount != v.inCount)
                {
                    return false;
                }
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < t.NumIn(); i++)
                    {
                        if (!haveIdenticalType(t.In(i), v.In(i), cmpTags))
                        {
                            return false;
                        }
                    }


                    i = i__prev1;
                }
                {
                    long i__prev1 = i;

                    for (i = 0L; i < t.NumOut(); i++)
                    {
                        if (!haveIdenticalType(t.Out(i), v.Out(i), cmpTags))
                        {
                            return false;
                        }
                    }


                    i = i__prev1;
                }
                return true;
            else if (kind == Interface) 
                t = (interfaceType.Value)(@unsafe.Pointer(T));
                v = (interfaceType.Value)(@unsafe.Pointer(V));
                if (len(t.methods) == 0L && len(v.methods) == 0L)
                {
                    return true;
                } 
                // Might have the same methods but still
                // need a run time conversion.
                return false;
            else if (kind == Map) 
                return haveIdenticalType(T.Key(), V.Key(), cmpTags) && haveIdenticalType(T.Elem(), V.Elem(), cmpTags);
            else if (kind == Ptr || kind == Slice) 
                return haveIdenticalType(T.Elem(), V.Elem(), cmpTags);
            else if (kind == Struct) 
                t = (structType.Value)(@unsafe.Pointer(T));
                v = (structType.Value)(@unsafe.Pointer(V));
                if (len(t.fields) != len(v.fields))
                {
                    return false;
                }
                if (t.pkgPath.name() != v.pkgPath.name())
                {
                    return false;
                }
                {
                    long i__prev1 = i;

                    foreach (var (__i) in t.fields)
                    {
                        i = __i;
                        var tf = ref t.fields[i];
                        var vf = ref v.fields[i];
                        if (tf.name.name() != vf.name.name())
                        {
                            return false;
                        }
                        if (!haveIdenticalType(tf.typ, vf.typ, cmpTags))
                        {
                            return false;
                        }
                        if (cmpTags && tf.name.tag() != vf.name.tag())
                        {
                            return false;
                        }
                        if (tf.offsetAnon != vf.offsetAnon)
                        {
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
        private static (slice<unsafe.Pointer>, slice<slice<int>>) typelinks()
;

        private static ref rtype rtypeOff(unsafe.Pointer section, int off)
        {
            return (rtype.Value)(add(section, uintptr(off), "sizeof(rtype) > 0"));
        }

        // typesByString returns the subslice of typelinks() whose elements have
        // the given string representation.
        // It may be empty (no known types with that string) or may have
        // multiple elements (multiple types with that string).
        private static slice<ref rtype> typesByString(@string s)
        {
            var (sections, offset) = typelinks();
            slice<ref rtype> ret = default;

            foreach (var (offsI, offs) in offset)
            {
                var section = sections[offsI]; 

                // We are looking for the first index i where the string becomes >= s.
                // This is a copy of sort.Search, with f(h) replaced by (*typ[h].String() >= s).
                long i = 0L;
                var j = len(offs);
                while (i < j)
                {>>MARKER:FUNCTION_typelinks_BLOCK_PREFIX<<
                    var h = i + (j - i) / 2L; // avoid overflow when computing h
                    // i ≤ h < j
                    if (!(rtypeOff(section, offs[h]).String() >= s))
                    {
                        i = h + 1L; // preserves f(i-1) == false
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
 
                // i == j, f(i-1) == false, and f(j) (= f(i)) == true  =>  answer is i.

                // Having found the first, linear scan forward to find the last.
                // We could do a second binary search, but the caller is going
                // to do a linear scan anyway.
                {
                    var j__prev2 = j;

                    for (j = i; j < len(offs); j++)
                    {
                        var typ = rtypeOff(section, offs[j]);
                        if (typ.String() != s)
                        {
                            break;
                        }
                        ret = append(ret, typ);
                    }


                    j = j__prev2;
                }
            }
            return ret;
        }

        // The lookupCache caches ArrayOf, ChanOf, MapOf and SliceOf lookups.
        private static sync.Map lookupCache = default; // map[cacheKey]*rtype

        // A cacheKey is the key for use in the lookupCache.
        // Four values describe any of the types we are looking for:
        // type kind, one or two subtypes, and an extra integer.
        private partial struct cacheKey
        {
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
        public static Type ChanOf(ChanDir dir, Type t) => func((_, panic, __) =>
        {
            ref rtype typ = t._<ref rtype>(); 

            // Look in cache.
            cacheKey ckey = new cacheKey(Chan,typ,nil,uintptr(dir));
            {
                var ch__prev1 = ch;

                var (ch, ok) = lookupCache.Load(ckey);

                if (ok)
                {
                    return ch._<ref rtype>();
                } 

                // This restriction is imposed by the gc compiler and the runtime.

                ch = ch__prev1;

            } 

            // This restriction is imposed by the gc compiler and the runtime.
            if (typ.size >= 1L << (int)(16L))
            {
                panic("reflect.ChanOf: element size too large");
            } 

            // Look in known types.
            // TODO: Precedence when constructing string.
            @string s = default;

            if (dir == SendDir) 
                s = "chan<- " + typ.String();
            else if (dir == RecvDir) 
                s = "<-chan " + typ.String();
            else if (dir == BothDir) 
                s = "chan " + typ.String();
            else 
                panic("reflect.ChanOf: invalid dir");
                        foreach (var (_, tt) in typesByString(s))
            {
                var ch = (chanType.Value)(@unsafe.Pointer(tt));
                if (ch.elem == typ && ch.dir == uintptr(dir))
                {
                    var (ti, _) = lookupCache.LoadOrStore(ckey, tt);
                    return ti._<Type>();
                }
            } 

            // Make a channel type.
            channel<unsafe.Pointer> ichan = (channel<unsafe.Pointer>)null;
            *(ptr<ptr<chanType>>) prototype = new ptr<*(ptr<ptr<chanType>>)>(@unsafe.Pointer(ref ichan));
            ch = prototype.Value;
            ch.tflag = 0L;
            ch.dir = uintptr(dir);
            ch.str = resolveReflectName(newName(s, "", false));
            ch.hash = fnv1(typ.hash, 'c', byte(dir));
            ch.elem = typ;

            (ti, _) = lookupCache.LoadOrStore(ckey, ref ch.rtype);
            return ti._<Type>();
        });

        private static bool ismapkey(ref rtype _p0)
; // implemented in runtime

        // MapOf returns the map type with the given key and element types.
        // For example, if k represents int and e represents string,
        // MapOf(k, e) represents map[int]string.
        //
        // If the key type is not a valid map key type (that is, if it does
        // not implement Go's == operator), MapOf panics.
        public static Type MapOf(Type key, Type elem) => func((_, panic, __) =>
        {
            ref rtype ktyp = key._<ref rtype>();
            ref rtype etyp = elem._<ref rtype>();

            if (!ismapkey(ktyp))
            {>>MARKER:FUNCTION_ismapkey_BLOCK_PREFIX<<
                panic("reflect.MapOf: invalid key type " + ktyp.String());
            } 

            // Look in cache.
            cacheKey ckey = new cacheKey(Map,ktyp,etyp,0);
            {
                var mt__prev1 = mt;

                var (mt, ok) = lookupCache.Load(ckey);

                if (ok)
                {
                    return mt._<Type>();
                } 

                // Look in known types.

                mt = mt__prev1;

            } 

            // Look in known types.
            @string s = "map[" + ktyp.String() + "]" + etyp.String();
            foreach (var (_, tt) in typesByString(s))
            {
                var mt = (mapType.Value)(@unsafe.Pointer(tt));
                if (mt.key == ktyp && mt.elem == etyp)
                {
                    var (ti, _) = lookupCache.LoadOrStore(ckey, tt);
                    return ti._<Type>();
                }
            } 

            // Make a map type.
            map<unsafe.Pointer, unsafe.Pointer> imap = (map<unsafe.Pointer, unsafe.Pointer>)null;
            mt = new ptr<ptr<ptr<*(ptr<ptr<mapType>>)>>>(@unsafe.Pointer(ref imap));
            mt.str = resolveReflectName(newName(s, "", false));
            mt.tflag = 0L;
            mt.hash = fnv1(etyp.hash, 'm', byte(ktyp.hash >> (int)(24L)), byte(ktyp.hash >> (int)(16L)), byte(ktyp.hash >> (int)(8L)), byte(ktyp.hash));
            mt.key = ktyp;
            mt.elem = etyp;
            mt.bucket = bucketOf(ktyp, etyp);
            if (ktyp.size > maxKeySize)
            {
                mt.keysize = uint8(ptrSize);
                mt.indirectkey = 1L;
            }
            else
            {
                mt.keysize = uint8(ktyp.size);
                mt.indirectkey = 0L;
            }
            if (etyp.size > maxValSize)
            {
                mt.valuesize = uint8(ptrSize);
                mt.indirectvalue = 1L;
            }
            else
            {
                mt.valuesize = uint8(etyp.size);
                mt.indirectvalue = 0L;
            }
            mt.bucketsize = uint16(mt.bucket.size);
            mt.reflexivekey = isReflexive(ktyp);
            mt.needkeyupdate = needKeyUpdate(ktyp);
            mt.ptrToThis = 0L;

            (ti, _) = lookupCache.LoadOrStore(ckey, ref mt.rtype);
            return ti._<Type>();
        });

        private partial struct funcTypeFixed4
        {
            public ref funcType funcType => ref funcType_val;
            public array<ref rtype> args;
        }
        private partial struct funcTypeFixed8
        {
            public ref funcType funcType => ref funcType_val;
            public array<ref rtype> args;
        }
        private partial struct funcTypeFixed16
        {
            public ref funcType funcType => ref funcType_val;
            public array<ref rtype> args;
        }
        private partial struct funcTypeFixed32
        {
            public ref funcType funcType => ref funcType_val;
            public array<ref rtype> args;
        }
        private partial struct funcTypeFixed64
        {
            public ref funcType funcType => ref funcType_val;
            public array<ref rtype> args;
        }
        private partial struct funcTypeFixed128
        {
            public ref funcType funcType => ref funcType_val;
            public array<ref rtype> args;
        }

        // FuncOf returns the function type with the given argument and result types.
        // For example if k represents int and e represents string,
        // FuncOf([]Type{k}, []Type{e}, false) represents func(int) string.
        //
        // The variadic argument controls whether the function is variadic. FuncOf
        // panics if the in[len(in)-1] does not represent a slice and variadic is
        // true.
        public static Type FuncOf(slice<Type> @in, slice<Type> @out, bool variadic) => func((defer, panic, _) =>
        {
            if (variadic && (len(in) == 0L || in[len(in) - 1L].Kind() != Slice))
            {
                panic("reflect.FuncOf: last arg of variadic func must be slice");
            } 

            // Make a func type.
            Action ifunc = (Action)null;
            *(ptr<ptr<funcType>>) prototype = new ptr<*(ptr<ptr<funcType>>)>(@unsafe.Pointer(ref ifunc));
            var n = len(in) + len(out);

            ref funcType ft = default;
            slice<ref rtype> args = default;

            if (n <= 4L) 
                ptr<object> @fixed = @new<funcTypeFixed4>();
                args = @fixed.args.slice(-1, 0L, len(@fixed.args));
                ft = ref @fixed.funcType;
            else if (n <= 8L) 
                @fixed = @new<funcTypeFixed8>();
                args = @fixed.args.slice(-1, 0L, len(@fixed.args));
                ft = ref @fixed.funcType;
            else if (n <= 16L) 
                @fixed = @new<funcTypeFixed16>();
                args = @fixed.args.slice(-1, 0L, len(@fixed.args));
                ft = ref @fixed.funcType;
            else if (n <= 32L) 
                @fixed = @new<funcTypeFixed32>();
                args = @fixed.args.slice(-1, 0L, len(@fixed.args));
                ft = ref @fixed.funcType;
            else if (n <= 64L) 
                @fixed = @new<funcTypeFixed64>();
                args = @fixed.args.slice(-1, 0L, len(@fixed.args));
                ft = ref @fixed.funcType;
            else if (n <= 128L) 
                @fixed = @new<funcTypeFixed128>();
                args = @fixed.args.slice(-1, 0L, len(@fixed.args));
                ft = ref @fixed.funcType;
            else 
                panic("reflect.FuncOf: too many arguments");
                        ft.Value = prototype.Value; 

            // Build a hash and minimally populate ft.
            uint hash = default;
            foreach (var (_, in) in in)
            {
                ref rtype t = in._<ref rtype>();
                args = append(args, t);
                hash = fnv1(hash, byte(t.hash >> (int)(24L)), byte(t.hash >> (int)(16L)), byte(t.hash >> (int)(8L)), byte(t.hash));
            }
            if (variadic)
            {
                hash = fnv1(hash, 'v');
            }
            hash = fnv1(hash, '.');
            foreach (var (_, out) in out)
            {
                t = out._<ref rtype>();
                args = append(args, t);
                hash = fnv1(hash, byte(t.hash >> (int)(24L)), byte(t.hash >> (int)(16L)), byte(t.hash >> (int)(8L)), byte(t.hash));
            }
            if (len(args) > 50L)
            {
                panic("reflect.FuncOf does not support more than 50 arguments");
            }
            ft.tflag = 0L;
            ft.hash = hash;
            ft.inCount = uint16(len(in));
            ft.outCount = uint16(len(out));
            if (variadic)
            {
                ft.outCount |= 1L << (int)(15L);
            } 

            // Look in cache.
            {
                var ts__prev1 = ts;

                var (ts, ok) = funcLookupCache.m.Load(hash);

                if (ok)
                {
                    {
                        ref rtype t__prev1 = t;

                        foreach (var (_, __t) in ts._<slice<ref rtype>>())
                        {
                            t = __t;
                            if (haveIdenticalUnderlyingType(ref ft.rtype, t, true))
                            {
                                return t;
                            }
                        }

                        t = t__prev1;
                    }

                } 

                // Not in cache, lock and retry.

                ts = ts__prev1;

            } 

            // Not in cache, lock and retry.
            funcLookupCache.Lock();
            defer(funcLookupCache.Unlock());
            {
                var ts__prev1 = ts;

                (ts, ok) = funcLookupCache.m.Load(hash);

                if (ok)
                {
                    {
                        ref rtype t__prev1 = t;

                        foreach (var (_, __t) in ts._<slice<ref rtype>>())
                        {
                            t = __t;
                            if (haveIdenticalUnderlyingType(ref ft.rtype, t, true))
                            {
                                return t;
                            }
                        }

                        t = t__prev1;
                    }

                }

                ts = ts__prev1;

            }

            Func<ref rtype, Type> addToCache = tt =>
            {
                slice<ref rtype> rts = default;
                {
                    var (rti, ok) = funcLookupCache.m.Load(hash);

                    if (ok)
                    {
                        rts = rti._<slice<ref rtype>>();
                    }

                }
                funcLookupCache.m.Store(hash, append(rts, tt));
                return tt;
            } 

            // Look in known types for the same string representation.
; 

            // Look in known types for the same string representation.
            var str = funcStr(ft);
            foreach (var (_, tt) in typesByString(str))
            {
                if (haveIdenticalUnderlyingType(ref ft.rtype, tt, true))
                {
                    return addToCache(tt);
                }
            } 

            // Populate the remaining fields of ft and store in cache.
            ft.str = resolveReflectName(newName(str, "", false));
            ft.ptrToThis = 0L;
            return addToCache(ref ft.rtype);
        });

        // funcStr builds a string representation of a funcType.
        private static @string funcStr(ref funcType ft)
        {
            var repr = make_slice<byte>(0L, 64L);
            repr = append(repr, "func(");
            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in ft.@in())
                {
                    i = __i;
                    t = __t;
                    if (i > 0L)
                    {
                        repr = append(repr, ", ");
                    }
                    if (ft.IsVariadic() && i == int(ft.inCount) - 1L)
                    {
                        repr = append(repr, "...");
                        repr = append(repr, (sliceType.Value)(@unsafe.Pointer(t)).elem.String());
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
            if (len(out) == 1L)
            {
                repr = append(repr, ' ');
            }
            else if (len(out) > 1L)
            {
                repr = append(repr, " (");
            }
            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in out)
                {
                    i = __i;
                    t = __t;
                    if (i > 0L)
                    {
                        repr = append(repr, ", ");
                    }
                    repr = append(repr, t.String());
                }

                i = i__prev1;
                t = t__prev1;
            }

            if (len(out) > 1L)
            {
                repr = append(repr, ')');
            }
            return string(repr);
        }

        // isReflexive reports whether the == operation on the type is reflexive.
        // That is, x == x for all values x of type t.
        private static bool isReflexive(ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {

            if (t.Kind() == Bool || t.Kind() == Int || t.Kind() == Int8 || t.Kind() == Int16 || t.Kind() == Int32 || t.Kind() == Int64 || t.Kind() == Uint || t.Kind() == Uint8 || t.Kind() == Uint16 || t.Kind() == Uint32 || t.Kind() == Uint64 || t.Kind() == Uintptr || t.Kind() == Chan || t.Kind() == Ptr || t.Kind() == String || t.Kind() == UnsafePointer) 
                return true;
            else if (t.Kind() == Float32 || t.Kind() == Float64 || t.Kind() == Complex64 || t.Kind() == Complex128 || t.Kind() == Interface) 
                return false;
            else if (t.Kind() == Array) 
                var tt = (arrayType.Value)(@unsafe.Pointer(t));
                return isReflexive(tt.elem);
            else if (t.Kind() == Struct) 
                tt = (structType.Value)(@unsafe.Pointer(t));
                foreach (var (_, f) in tt.fields)
                {
                    if (!isReflexive(f.typ))
                    {
                        return false;
                    }
                }
                return true;
            else 
                // Func, Map, Slice, Invalid
                panic("isReflexive called on non-key type " + t.String());
                    });

        // needKeyUpdate reports whether map overwrites require the key to be copied.
        private static bool needKeyUpdate(ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {

            if (t.Kind() == Bool || t.Kind() == Int || t.Kind() == Int8 || t.Kind() == Int16 || t.Kind() == Int32 || t.Kind() == Int64 || t.Kind() == Uint || t.Kind() == Uint8 || t.Kind() == Uint16 || t.Kind() == Uint32 || t.Kind() == Uint64 || t.Kind() == Uintptr || t.Kind() == Chan || t.Kind() == Ptr || t.Kind() == UnsafePointer) 
                return false;
            else if (t.Kind() == Float32 || t.Kind() == Float64 || t.Kind() == Complex64 || t.Kind() == Complex128 || t.Kind() == Interface || t.Kind() == String) 
                // Float keys can be updated from +0 to -0.
                // String keys can be updated to use a smaller backing store.
                // Interfaces might have floats of strings in them.
                return true;
            else if (t.Kind() == Array) 
                var tt = (arrayType.Value)(@unsafe.Pointer(t));
                return needKeyUpdate(tt.elem);
            else if (t.Kind() == Struct) 
                tt = (structType.Value)(@unsafe.Pointer(t));
                foreach (var (_, f) in tt.fields)
                {
                    if (needKeyUpdate(f.typ))
                    {
                        return true;
                    }
                }
                return false;
            else 
                // Func, Map, Slice, Invalid
                panic("needKeyUpdate called on non-key type " + t.String());
                    });

        // Make sure these routines stay in sync with ../../runtime/hashmap.go!
        // These types exist only for GC, so we only fill out GC relevant info.
        // Currently, that's just size and the GC program. We also fill in string
        // for possible debugging use.
        private static readonly System.UIntPtr bucketSize = 8L;
        private static readonly System.UIntPtr maxKeySize = 128L;
        private static readonly System.UIntPtr maxValSize = 128L;

        private static ref rtype bucketOf(ref rtype _ktyp, ref rtype _etyp) => func(_ktyp, _etyp, (ref rtype ktyp, ref rtype etyp, Defer _, Panic panic, Recover __) =>
        { 
            // See comment on hmap.overflow in ../runtime/hashmap.go.
            byte kind = default;
            if (ktyp.kind & kindNoPointers != 0L && etyp.kind & kindNoPointers != 0L && ktyp.size <= maxKeySize && etyp.size <= maxValSize)
            {
                kind = kindNoPointers;
            }
            if (ktyp.size > maxKeySize)
            {
                ktyp = PtrTo(ktyp)._<ref rtype>();
            }
            if (etyp.size > maxValSize)
            {
                etyp = PtrTo(etyp)._<ref rtype>();
            } 

            // Prepare GC data if any.
            // A bucket is at most bucketSize*(1+maxKeySize+maxValSize)+2*ptrSize bytes,
            // or 2072 bytes, or 259 pointer-size words, or 33 bytes of pointer bitmap.
            // Note that since the key and value are known to be <= 128 bytes,
            // they're guaranteed to have bitmaps instead of GC programs.
            ref byte gcdata = default;
            System.UIntPtr ptrdata = default;
            System.UIntPtr overflowPad = default; 

            // On NaCl, pad if needed to make overflow end at the proper struct alignment.
            // On other systems, align > ptrSize is not possible.
            if (runtime.GOARCH == "amd64p32" && (ktyp.align > ptrSize || etyp.align > ptrSize))
            {
                overflowPad = ptrSize;
            }
            var size = bucketSize * (1L + ktyp.size + etyp.size) + overflowPad + ptrSize;
            if (size & uintptr(ktyp.align - 1L) != 0L || size & uintptr(etyp.align - 1L) != 0L)
            {
                panic("reflect: bad size computation in MapOf");
            }
            if (kind != kindNoPointers)
            {
                var nptr = (bucketSize * (1L + ktyp.size + etyp.size) + ptrSize) / ptrSize;
                var mask = make_slice<byte>((nptr + 7L) / 8L);
                var @base = bucketSize / ptrSize;

                if (ktyp.kind & kindNoPointers == 0L)
                {
                    if (ktyp.kind & kindGCProg != 0L)
                    {
                        panic("reflect: unexpected GC program in MapOf");
                    }
                    ref array<byte> kmask = new ptr<ref array<byte>>(@unsafe.Pointer(ktyp.gcdata));
                    {
                        var i__prev1 = i;

                        for (var i = uintptr(0L); i < ktyp.ptrdata / ptrSize; i++)
                        {
                            if ((kmask[i / 8L] >> (int)((i % 8L))) & 1L != 0L)
                            {
                                {
                                    var j__prev2 = j;

                                    for (var j = uintptr(0L); j < bucketSize; j++)
                                    {
                                        var word = base + j * ktyp.size / ptrSize + i;
                                        mask[word / 8L] |= 1L << (int)((word % 8L));
                                    }


                                    j = j__prev2;
                                }
                            }
                        }


                        i = i__prev1;
                    }
                }
                base += bucketSize * ktyp.size / ptrSize;

                if (etyp.kind & kindNoPointers == 0L)
                {
                    if (etyp.kind & kindGCProg != 0L)
                    {
                        panic("reflect: unexpected GC program in MapOf");
                    }
                    ref array<byte> emask = new ptr<ref array<byte>>(@unsafe.Pointer(etyp.gcdata));
                    {
                        var i__prev1 = i;

                        for (i = uintptr(0L); i < etyp.ptrdata / ptrSize; i++)
                        {
                            if ((emask[i / 8L] >> (int)((i % 8L))) & 1L != 0L)
                            {
                                {
                                    var j__prev2 = j;

                                    for (j = uintptr(0L); j < bucketSize; j++)
                                    {
                                        word = base + j * etyp.size / ptrSize + i;
                                        mask[word / 8L] |= 1L << (int)((word % 8L));
                                    }


                                    j = j__prev2;
                                }
                            }
                        }


                        i = i__prev1;
                    }
                }
                base += bucketSize * etyp.size / ptrSize;
                base += overflowPad / ptrSize;

                word = base;
                mask[word / 8L] |= 1L << (int)((word % 8L));
                gcdata = ref mask[0L];
                ptrdata = (word + 1L) * ptrSize; 

                // overflow word must be last
                if (ptrdata != size)
                {
                    panic("reflect: bad layout computation in MapOf");
                }
            }
            rtype b = ref new rtype(align:ptrSize,size:size,kind:kind,ptrdata:ptrdata,gcdata:gcdata,);
            if (overflowPad > 0L)
            {
                b.align = 8L;
            }
            @string s = "bucket(" + ktyp.String() + "," + etyp.String() + ")";
            b.str = resolveReflectName(newName(s, "", false));
            return b;
        });

        // SliceOf returns the slice type with element type t.
        // For example, if t represents int, SliceOf(t) represents []int.
        public static Type SliceOf(Type t)
        {
            ref rtype typ = t._<ref rtype>(); 

            // Look in cache.
            cacheKey ckey = new cacheKey(Slice,typ,nil,0);
            {
                var slice__prev1 = slice;

                var (slice, ok) = lookupCache.Load(ckey);

                if (ok)
                {
                    return slice._<Type>();
                } 

                // Look in known types.

                slice = slice__prev1;

            } 

            // Look in known types.
            @string s = "[]" + typ.String();
            foreach (var (_, tt) in typesByString(s))
            {
                var slice = (sliceType.Value)(@unsafe.Pointer(tt));
                if (slice.elem == typ)
                {
                    var (ti, _) = lookupCache.LoadOrStore(ckey, tt);
                    return ti._<Type>();
                }
            } 

            // Make a slice type.
            slice<unsafe.Pointer> islice = (slice<unsafe.Pointer>)null;
            *(ptr<ptr<sliceType>>) prototype = new ptr<*(ptr<ptr<sliceType>>)>(@unsafe.Pointer(ref islice));
            slice = prototype.Value;
            slice.tflag = 0L;
            slice.str = resolveReflectName(newName(s, "", false));
            slice.hash = fnv1(typ.hash, '[');
            slice.elem = typ;
            slice.ptrToThis = 0L;

            (ti, _) = lookupCache.LoadOrStore(ckey, ref slice.rtype);
            return ti._<Type>();
        }

        // The structLookupCache caches StructOf lookups.
        // StructOf does not share the common lookupCache since we need to pin
        // the memory associated with *structTypeFixedN.
        private static var structLookupCache = default;

        private partial struct structTypeUncommon
        {
            public ref structType structType => ref structType_val;
            public uncommonType u;
        }

        // A *rtype representing a struct is followed directly in memory by an
        // array of method objects representing the methods attached to the
        // struct. To get the same layout for a run time generated type, we
        // need an array directly following the uncommonType memory. The types
        // structTypeFixed4, ...structTypeFixedN are used to do this.
        //
        // A similar strategy is used for funcTypeFixed4, ...funcTypeFixedN.

        // TODO(crawshaw): as these structTypeFixedN and funcTypeFixedN structs
        // have no methods, they could be defined at runtime using the StructOf
        // function.

        private partial struct structTypeFixed4
        {
            public ref structType structType => ref structType_val;
            public uncommonType u;
            public array<method> m;
        }

        private partial struct structTypeFixed8
        {
            public ref structType structType => ref structType_val;
            public uncommonType u;
            public array<method> m;
        }

        private partial struct structTypeFixed16
        {
            public ref structType structType => ref structType_val;
            public uncommonType u;
            public array<method> m;
        }

        private partial struct structTypeFixed32
        {
            public ref structType structType => ref structType_val;
            public uncommonType u;
            public array<method> m;
        }

        // isLetter returns true if a given 'rune' is classified as a Letter.
        private static bool isLetter(int ch)
        {
            return 'a' <= ch && ch <= 'z' || 'A' <= ch && ch <= 'Z' || ch == '_' || ch >= utf8.RuneSelf && unicode.IsLetter(ch);
        }

        // isValidFieldName checks if a string is a valid (struct) field name or not.
        //
        // According to the language spec, a field name should be an identifier.
        //
        // identifier = letter { letter | unicode_digit } .
        // letter = unicode_letter | "_" .
        private static bool isValidFieldName(@string fieldName)
        {
            foreach (var (i, c) in fieldName)
            {
                if (i == 0L && !isLetter(c))
                {
                    return false;
                }
                if (!(isLetter(c) || unicode.IsDigit(c)))
                {
                    return false;
                }
            }
            return len(fieldName) > 0L;
        }

        // StructOf returns the struct type containing fields.
        // The Offset and Index fields are ignored and computed as they would be
        // by the compiler.
        //
        // StructOf currently does not generate wrapper methods for embedded fields.
        // This limitation may be lifted in a future version.
        public static Type StructOf(slice<StructField> fields) => func((defer, panic, _) =>
        {
            var hash = fnv1(0L, (slice<byte>)"struct {");            System.UIntPtr size = default;            byte typalign = default;            var comparable = true;            var hashable = true;            slice<method> methods = default;            var fs = make_slice<structField>(len(fields));            var repr = make_slice<byte>(0L, 64L);            var hasPtr = false;            var hasGCProg = false;

            var lastzero = uintptr(0L);
            repr = append(repr, "struct {");
            {
                var i__prev1 = i;

                foreach (var (__i, __field) in fields)
                {
                    i = __i;
                    field = __field;
                    if (field.Name == "")
                    {
                        panic("reflect.StructOf: field " + strconv.Itoa(i) + " has no name");
                    }
                    if (!isValidFieldName(field.Name))
                    {
                        panic("reflect.StructOf: field " + strconv.Itoa(i) + " has invalid name");
                    }
                    if (field.Type == null)
                    {
                        panic("reflect.StructOf: field " + strconv.Itoa(i) + " has no type");
                    }
                    var f = runtimeStructField(field);
                    var ft = f.typ;
                    if (ft.kind & kindGCProg != 0L)
                    {
                        hasGCProg = true;
                    }
                    if (ft.pointers())
                    {
                        hasPtr = true;
                    } 

                    // Update string and hash
                    var name = f.name.name();
                    hash = fnv1(hash, (slice<byte>)name);
                    repr = append(repr, (" " + name));
                    if (f.anon())
                    { 
                        // Embedded field
                        if (f.typ.Kind() == Ptr)
                        { 
                            // Embedded ** and *interface{} are illegal
                            var elem = ft.Elem();
                            {
                                var k = elem.Kind();

                                if (k == Ptr || k == Interface)
                                {
                                    panic("reflect.StructOf: illegal anonymous field type " + ft.String());
                                }

                            }
                        }

                        if (f.typ.Kind() == Interface) 
                            var ift = (interfaceType.Value)(@unsafe.Pointer(ft));
                            {
                                var m__prev2 = m;

                                foreach (var (__im, __m) in ift.methods)
                                {
                                    im = __im;
                                    m = __m;
                                    if (ift.nameOff(m.name).pkgPath() != "")
                                    { 
                                        // TODO(sbinet).  Issue 15924.
                                        panic("reflect: embedded interface with unexported method(s) not implemented");
                                    }
                                    var mtyp = ift.typeOff(m.typ);                                    var ifield = i;                                    var imethod = im;                                    Value ifn = default;                                    Value tfn = default;

                                    if (ft.kind & kindDirectIface != 0L)
                                    {
                                        tfn = MakeFunc(mtyp, @in =>
                                        {
                                            slice<Value> args = default;
                                            var recv = in[0L];
                                            if (len(in) > 1L)
                                            {
                                                args = in[1L..];
                                            }
                                            return recv.Field(ifield).Method(imethod).Call(args);
                                        }
                                    else
);
                                        ifn = MakeFunc(mtyp, @in =>
                                        {
                                            args = default;
                                            recv = in[0L];
                                            if (len(in) > 1L)
                                            {
                                                args = in[1L..];
                                            }
                                            return recv.Field(ifield).Method(imethod).Call(args);
                                        });
                                    }                                    {
                                        tfn = MakeFunc(mtyp, @in =>
                                        {
                                            args = default;
                                            recv = in[0L];
                                            if (len(in) > 1L)
                                            {
                                                args = in[1L..];
                                            }
                                            return recv.Field(ifield).Method(imethod).Call(args);
                                        });
                                        ifn = MakeFunc(mtyp, @in =>
                                        {
                                            args = default;
                                            recv = Indirect(in[0L]);
                                            if (len(in) > 1L)
                                            {
                                                args = in[1L..];
                                            }
                                            return recv.Field(ifield).Method(imethod).Call(args);
                                        });
                                    }
                                    methods = append(methods, new method(name:resolveReflectName(ift.nameOff(m.name)),mtyp:resolveReflectType(mtyp),ifn:resolveReflectText(unsafe.Pointer(&ifn)),tfn:resolveReflectText(unsafe.Pointer(&tfn)),));
                                }

                                m = m__prev2;
                            }
                        else if (f.typ.Kind() == Ptr) 
                            var ptr = (ptrType.Value)(@unsafe.Pointer(ft));
                            {
                                var unt__prev2 = unt;

                                var unt = ptr.uncommon();

                                if (unt != null)
                                {
                                    if (i > 0L && unt.mcount > 0L)
                                    { 
                                        // Issue 15924.
                                        panic("reflect: embedded type with methods not implemented if type is not first field");
                                    }
                                    {
                                        var m__prev2 = m;

                                        foreach (var (_, __m) in unt.methods())
                                        {
                                            m = __m;
                                            var mname = ptr.nameOff(m.name);
                                            if (mname.pkgPath() != "")
                                            { 
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

                                if (unt != null)
                                {
                                    {
                                        var m__prev2 = m;

                                        foreach (var (_, __m) in unt.methods())
                                        {
                                            m = __m;
                                            mname = ptr.nameOff(m.name);
                                            if (mname.pkgPath() != "")
                                            { 
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

                                if (unt != null)
                                {
                                    if (i > 0L && unt.mcount > 0L)
                                    { 
                                        // Issue 15924.
                                        panic("reflect: embedded type with methods not implemented if type is not first field");
                                    }
                                    {
                                        var m__prev2 = m;

                                        foreach (var (_, __m) in unt.methods())
                                        {
                                            m = __m;
                                            mname = ft.nameOff(m.name);
                                            if (mname.pkgPath() != "")
                                            { 
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

                        if (dup)
                        {
                            panic("reflect.StructOf: duplicate field " + name);
                        }

                    }
                    fset[name] = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};

                    hash = fnv1(hash, byte(ft.hash >> (int)(24L)), byte(ft.hash >> (int)(16L)), byte(ft.hash >> (int)(8L)), byte(ft.hash));

                    repr = append(repr, (" " + ft.String()));
                    if (f.name.tagLen() > 0L)
                    {
                        hash = fnv1(hash, (slice<byte>)f.name.tag());
                        repr = append(repr, (" " + strconv.Quote(f.name.tag())));
                    }
                    if (i < len(fields) - 1L)
                    {
                        repr = append(repr, ';');
                    }
                    comparable = comparable && (ft.alg.equal != null);
                    hashable = hashable && (ft.alg.hash != null);

                    var offset = align(size, uintptr(ft.align));
                    if (ft.align > typalign)
                    {
                        typalign = ft.align;
                    }
                    size = offset + ft.size;
                    f.offsetAnon |= offset << (int)(1L);

                    if (ft.size == 0L)
                    {
                        lastzero = size;
                    }
                    fs[i] = f;
                }

                i = i__prev1;
            }

            if (size > 0L && lastzero == size)
            { 
                // This is a non-zero sized struct that ends in a
                // zero-sized field. We add an extra byte of padding,
                // to ensure that taking the address of the final
                // zero-sized field can't manufacture a pointer to the
                // next object in the heap. See issue 9401.
                size++;
            }
            ref structType typ = default;
            ref uncommonType ut = default;


            if (len(methods) == 0L) 
                ptr<object> t = @new<structTypeUncommon>();
                typ = ref t.structType;
                ut = ref t.u;
            else if (len(methods) <= 4L) 
                t = @new<structTypeFixed4>();
                typ = ref t.structType;
                ut = ref t.u;
                copy(t.m[..], methods);
            else if (len(methods) <= 8L) 
                t = @new<structTypeFixed8>();
                typ = ref t.structType;
                ut = ref t.u;
                copy(t.m[..], methods);
            else if (len(methods) <= 16L) 
                t = @new<structTypeFixed16>();
                typ = ref t.structType;
                ut = ref t.u;
                copy(t.m[..], methods);
            else if (len(methods) <= 32L) 
                t = @new<structTypeFixed32>();
                typ = ref t.structType;
                ut = ref t.u;
                copy(t.m[..], methods);
            else 
                panic("reflect.StructOf: too many methods");
                        ut.mcount = uint16(len(methods));
            ut.moff = uint32(@unsafe.Sizeof(new uncommonType()));

            if (len(fs) > 0L)
            {
                repr = append(repr, ' ');
            }
            repr = append(repr, '}');
            hash = fnv1(hash, '}');
            var str = string(repr); 

            // Round the size up to be a multiple of the alignment.
            size = align(size, uintptr(typalign)); 

            // Make the struct type.
            struct{} istruct = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{};
            *(ptr<ptr<structType>>) prototype = new ptr<*(ptr<ptr<structType>>)>(@unsafe.Pointer(ref istruct));
            typ.Value = prototype.Value;
            typ.fields = fs; 

            // Look in cache.
            {
                var ts__prev1 = ts;

                var (ts, ok) = structLookupCache.m.Load(hash);

                if (ok)
                {
                    {
                        slice<Type> st__prev1 = st;

                        foreach (var (_, __st) in ts._<slice<Type>>())
                        {
                            st = __st;
                            t = st.common();
                            if (haveIdenticalUnderlyingType(ref typ.rtype, t, true))
                            {
                                return t;
                            }
                        }

                        st = st__prev1;
                    }

                } 

                // Not in cache, lock and retry.

                ts = ts__prev1;

            } 

            // Not in cache, lock and retry.
            structLookupCache.Lock();
            defer(structLookupCache.Unlock());
            {
                var ts__prev1 = ts;

                (ts, ok) = structLookupCache.m.Load(hash);

                if (ok)
                {
                    {
                        slice<Type> st__prev1 = st;

                        foreach (var (_, __st) in ts._<slice<Type>>())
                        {
                            st = __st;
                            t = st.common();
                            if (haveIdenticalUnderlyingType(ref typ.rtype, t, true))
                            {
                                return t;
                            }
                        }

                        st = st__prev1;
                    }

                }

                ts = ts__prev1;

            }

            Func<Type, Type> addToCache = t =>
            {
                slice<Type> ts = default;
                {
                    var (ti, ok) = structLookupCache.m.Load(hash);

                    if (ok)
                    {
                        ts = ti._<slice<Type>>();
                    }

                }
                structLookupCache.m.Store(hash, append(ts, t));
                return t;
            } 

            // Look in known types.
; 

            // Look in known types.
            {
                ptr<object> t__prev1 = t;

                foreach (var (_, __t) in typesByString(str))
                {
                    t = __t;
                    if (haveIdenticalUnderlyingType(ref typ.rtype, t, true))
                    { 
                        // even if 't' wasn't a structType with methods, we should be ok
                        // as the 'u uncommonType' field won't be accessed except when
                        // tflag&tflagUncommon is set.
                        return addToCache(t);
                    }
                }

                t = t__prev1;
            }

            typ.str = resolveReflectName(newName(str, "", false));
            typ.tflag = 0L;
            typ.hash = hash;
            typ.size = size;
            typ.align = typalign;
            typ.fieldAlign = typalign;
            typ.ptrToThis = 0L;
            if (len(methods) > 0L)
            {
                typ.tflag |= tflagUncommon;
            }
            if (!hasPtr)
            {
                typ.kind |= kindNoPointers;
            }
            else
            {
                typ.kind &= kindNoPointers;
            }
            if (hasGCProg)
            {
                long lastPtrField = 0L;
                {
                    var i__prev1 = i;
                    var ft__prev1 = ft;

                    foreach (var (__i, __ft) in fs)
                    {
                        i = __i;
                        ft = __ft;
                        if (ft.typ.pointers())
                        {
                            lastPtrField = i;
                        }
                    }
            else

                    i = i__prev1;
                    ft = ft__prev1;
                }

                byte prog = new slice<byte>(new byte[] { 0, 0, 0, 0 }); // will be length of prog
                {
                    var i__prev1 = i;
                    var ft__prev1 = ft;

                    foreach (var (__i, __ft) in fs)
                    {
                        i = __i;
                        ft = __ft;
                        if (i > lastPtrField)
                        { 
                            // gcprog should not include anything for any field after
                            // the last field that contains pointer data
                            break;
                        } 
                        // FIXME(sbinet) handle padding, fields smaller than a word
                        ref array<byte> elemGC = new ptr<ref array<byte>>(@unsafe.Pointer(ft.typ.gcdata))[..];
                        var elemPtrs = ft.typ.ptrdata / ptrSize;

                        if (ft.typ.kind & kindGCProg == 0L && ft.typ.ptrdata != 0L) 
                            // Element is small with pointer mask; use as literal bits.
                            var mask = elemGC; 
                            // Emit 120-bit chunks of full bytes (max is 127 but we avoid using partial bytes).
                            System.UIntPtr n = default;
                            {
                                System.UIntPtr n__prev2 = n;

                                n = elemPtrs;

                                while (n > 120L)
                                {
                                    prog = append(prog, 120L);
                                    prog = append(prog, mask[..15L]);
                                    mask = mask[15L..];
                                    n -= 120L;
                                }


                                n = n__prev2;
                            }
                            prog = append(prog, byte(n));
                            prog = append(prog, mask[..(n + 7L) / 8L]);
                        else if (ft.typ.kind & kindGCProg != 0L) 
                            // Element has GC program; emit one element.
                            var elemProg = elemGC[4L..4L + @unsafe.Pointer(ref elemGC[0L]).Value - 1L];
                            prog = append(prog, elemProg);
                        // Pad from ptrdata to size.
                        var elemWords = ft.typ.size / ptrSize;
                        if (elemPtrs < elemWords)
                        { 
                            // Emit literal 0 bit, then repeat as needed.
                            prog = append(prog, 0x01UL, 0x00UL);
                            if (elemPtrs + 1L < elemWords)
                            {
                                prog = append(prog, 0x81UL);
                                prog = appendVarint(prog, elemWords - elemPtrs - 1L);
                            }
                        }
                    }

                    i = i__prev1;
                    ft = ft__prev1;
                }

                (uint32.Value)(@unsafe.Pointer(ref prog[0L])).Value;

                uint32(len(prog) - 4L);
                typ.kind |= kindGCProg;
                typ.gcdata = ref prog[0L];
            }            {
                typ.kind &= kindGCProg;
                ptr<object> bv = @new<bitVector>();
                addTypeBits(bv, 0L, typ.common());
                if (len(bv.data) > 0L)
                {
                    typ.gcdata = ref bv.data[0L];
                }
            }
            typ.ptrdata = typeptrdata(typ.common());
            typ.alg = @new<typeAlg>();
            if (hashable)
            {
                typ.alg.hash = (p, seed) =>
                {
                    var o = seed;
                    {
                        var ft__prev1 = ft;

                        foreach (var (_, __ft) in typ.fields)
                        {
                            ft = __ft;
                            var pi = add(p, ft.offset(), "&x.field safe");
                            o = ft.typ.alg.hash(pi, o);
                        }

                        ft = ft__prev1;
                    }

                    return o;
                }
;
            }
            if (comparable)
            {
                typ.alg.equal = (p, q) =>
                {
                    {
                        var ft__prev1 = ft;

                        foreach (var (_, __ft) in typ.fields)
                        {
                            ft = __ft;
                            pi = add(p, ft.offset(), "&x.field safe");
                            var qi = add(q, ft.offset(), "&x.field safe");
                            if (!ft.typ.alg.equal(pi, qi))
                            {
                                return false;
                            }
                        }

                        ft = ft__prev1;
                    }

                    return true;
                }
;
            }

            if (len(fs) == 1L && !ifaceIndir(fs[0L].typ)) 
                // structs of 1 direct iface type can be direct
                typ.kind |= kindDirectIface;
            else 
                typ.kind &= kindDirectIface;
                        return addToCache(ref typ.rtype);
        });

        private static structField runtimeStructField(StructField field) => func((_, panic, __) =>
        {
            if (field.PkgPath != "")
            {
                panic("reflect.StructOf: StructOf does not allow unexported fields");
            } 

            // Best-effort check for misuse.
            // Since PkgPath is empty, not much harm done if Unicode lowercase slips through.
            var c = field.Name[0L];
            if ('a' <= c && c <= 'z' || c == '_')
            {
                panic("reflect.StructOf: field \"" + field.Name + "\" is unexported but missing PkgPath");
            }
            var offsetAnon = uintptr(0L);
            if (field.Anonymous)
            {
                offsetAnon |= 1L;
            }
            resolveReflectType(field.Type.common()); // install in runtime
            return new structField(name:newName(field.Name,string(field.Tag),true),typ:field.Type.common(),offsetAnon:offsetAnon,);
        });

        // typeptrdata returns the length in bytes of the prefix of t
        // containing pointer data. Anything after this offset is scalar data.
        // keep in sync with ../cmd/compile/internal/gc/reflect.go
        private static System.UIntPtr typeptrdata(ref rtype _t) => func(_t, (ref rtype t, Defer _, Panic panic, Recover __) =>
        {
            if (!t.pointers())
            {
                return 0L;
            }

            if (t.Kind() == Struct) 
                var st = (structType.Value)(@unsafe.Pointer(t)); 
                // find the last field that has pointers.
                long field = 0L;
                foreach (var (i) in st.fields)
                {
                    var ft = st.fields[i].typ;
                    if (ft.pointers())
                    {
                        field = i;
                    }
                }
                var f = st.fields[field];
                return f.offset() + f.typ.ptrdata;
            else 
                panic("reflect.typeptrdata: unexpected type, " + t.String());
                    });

        // See cmd/compile/internal/gc/reflect.go for derivation of constant.
        private static readonly long maxPtrmaskBytes = 2048L;

        // ArrayOf returns the array type with the given count and element type.
        // For example, if t represents int, ArrayOf(5, t) represents [5]int.
        //
        // If the resulting type would be larger than the available address space,
        // ArrayOf panics.


        // ArrayOf returns the array type with the given count and element type.
        // For example, if t represents int, ArrayOf(5, t) represents [5]int.
        //
        // If the resulting type would be larger than the available address space,
        // ArrayOf panics.
        public static Type ArrayOf(long count, Type elem) => func((_, panic, __) =>
        {
            ref rtype typ = elem._<ref rtype>(); 

            // Look in cache.
            cacheKey ckey = new cacheKey(Array,typ,nil,uintptr(count));
            {
                var array__prev1 = array;

                var (array, ok) = lookupCache.Load(ckey);

                if (ok)
                {
                    return array._<Type>();
                } 

                // Look in known types.

                array = array__prev1;

            } 

            // Look in known types.
            @string s = "[" + strconv.Itoa(count) + "]" + typ.String();
            foreach (var (_, tt) in typesByString(s))
            {
                var array = (arrayType.Value)(@unsafe.Pointer(tt));
                if (array.elem == typ)
                {
                    var (ti, _) = lookupCache.LoadOrStore(ckey, tt);
                    return ti._<Type>();
                }
            } 

            // Make an array type.
            array<unsafe.Pointer> iarray = new array<unsafe.Pointer>(new unsafe.Pointer[] {  });
            *(ptr<ptr<arrayType>>) prototype = new ptr<*(ptr<ptr<arrayType>>)>(@unsafe.Pointer(ref iarray));
            array = prototype.Value;
            array.tflag = 0L;
            array.str = resolveReflectName(newName(s, "", false));
            array.hash = fnv1(typ.hash, '[');
            {
                var n__prev1 = n;

                var n = uint32(count);

                while (n > 0L)
                {
                    array.hash = fnv1(array.hash, byte(n));
                    n >>= 8L;
                }


                n = n__prev1;
            }
            array.hash = fnv1(array.hash, ']');
            array.elem = typ;
            array.ptrToThis = 0L;
            if (typ.size > 0L)
            {
                var max = ~uintptr(0L) / typ.size;
                if (uintptr(count) > max)
                {
                    panic("reflect.ArrayOf: array size would exceed virtual address space");
                }
            }
            array.size = typ.size * uintptr(count);
            if (count > 0L && typ.ptrdata != 0L)
            {
                array.ptrdata = typ.size * uintptr(count - 1L) + typ.ptrdata;
            }
            array.align = typ.align;
            array.fieldAlign = typ.fieldAlign;
            array.len = uintptr(count);
            array.slice = SliceOf(elem)._<ref rtype>();

            array.kind &= kindNoPointers;

            if (typ.kind & kindNoPointers != 0L || array.size == 0L) 
                // No pointers.
                array.kind |= kindNoPointers;
                array.gcdata = null;
                array.ptrdata = 0L;
            else if (count == 1L) 
                // In memory, 1-element array looks just like the element.
                array.kind |= typ.kind & kindGCProg;
                array.gcdata = typ.gcdata;
                array.ptrdata = typ.ptrdata;
            else if (typ.kind & kindGCProg == 0L && array.size <= maxPtrmaskBytes * 8L * ptrSize) 
                // Element is small with pointer mask; array is still small.
                // Create direct pointer mask by turning each 1 bit in elem
                // into count 1 bits in larger mask.
                var mask = make_slice<byte>((array.ptrdata / ptrSize + 7L) / 8L);
                ref array<byte> elemMask = new ptr<ref array<byte>>(@unsafe.Pointer(typ.gcdata))[..];
                var elemWords = typ.size / ptrSize;
                for (var j = uintptr(0L); j < typ.ptrdata / ptrSize; j++)
                {
                    if ((elemMask[j / 8L] >> (int)((j % 8L))) & 1L != 0L)
                    {
                        {
                            var i__prev2 = i;

                            for (var i = uintptr(0L); i < array.len; i++)
                            {
                                var k = i * elemWords + j;
                                mask[k / 8L] |= 1L << (int)((k % 8L));
                            }


                            i = i__prev2;
                        }
                    }
                }

                array.gcdata = ref mask[0L];
            else 
                // Create program that emits one element
                // and then repeats to make the array.
                byte prog = new slice<byte>(new byte[] { 0, 0, 0, 0 }); // will be length of prog
                ref array<byte> elemGC = new ptr<ref array<byte>>(@unsafe.Pointer(typ.gcdata))[..];
                var elemPtrs = typ.ptrdata / ptrSize;
                if (typ.kind & kindGCProg == 0L)
                { 
                    // Element is small with pointer mask; use as literal bits.
                    mask = elemGC; 
                    // Emit 120-bit chunks of full bytes (max is 127 but we avoid using partial bytes).
                    n = default;
                    n = elemPtrs;

                    while (n > 120L)
                    {
                        prog = append(prog, 120L);
                        prog = append(prog, mask[..15L]);
                        mask = mask[15L..];
                        n -= 120L;
                    }
                else

                    prog = append(prog, byte(n));
                    prog = append(prog, mask[..(n + 7L) / 8L]);
                }                { 
                    // Element has GC program; emit one element.
                    var elemProg = elemGC[4L..4L + @unsafe.Pointer(ref elemGC[0L]).Value - 1L];
                    prog = append(prog, elemProg);
                } 
                // Pad from ptrdata to size.
                elemWords = typ.size / ptrSize;
                if (elemPtrs < elemWords)
                { 
                    // Emit literal 0 bit, then repeat as needed.
                    prog = append(prog, 0x01UL, 0x00UL);
                    if (elemPtrs + 1L < elemWords)
                    {
                        prog = append(prog, 0x81UL);
                        prog = appendVarint(prog, elemWords - elemPtrs - 1L);
                    }
                } 
                // Repeat count-1 times.
                if (elemWords < 0x80UL)
                {
                    prog = append(prog, byte(elemWords | 0x80UL));
                }
                else
                {
                    prog = append(prog, 0x80UL);
                    prog = appendVarint(prog, elemWords);
                }
                prog = appendVarint(prog, uintptr(count) - 1L);
                prog = append(prog, 0L) * (uint32.Value)(@unsafe.Pointer(ref prog[0L]));

                uint32(len(prog) - 4L);
                array.kind |= kindGCProg;
                array.gcdata = ref prog[0L];
                array.ptrdata = array.size; // overestimate but ok; must match program
                        var etyp = typ.common();
            var esize = etyp.Size();
            var ealg = etyp.alg;

            array.alg = @new<typeAlg>();
            if (ealg.equal != null)
            {
                var eequal = ealg.equal;
                array.alg.equal = (p, q) =>
                {
                    {
                        var i__prev1 = i;

                        for (i = 0L; i < count; i++)
                        {
                            var pi = arrayAt(p, i, esize, "i < count");
                            var qi = arrayAt(q, i, esize, "i < count");
                            if (!eequal(pi, qi))
                            {
                                return false;
                            }
                        }


                        i = i__prev1;
                    }
                    return true;
                }
;
            }
            if (ealg.hash != null)
            {
                var ehash = ealg.hash;
                array.alg.hash = (ptr, seed) =>
                {
                    var o = seed;
                    {
                        var i__prev1 = i;

                        for (i = 0L; i < count; i++)
                        {
                            o = ehash(arrayAt(ptr, i, esize, "i < count"), o);
                        }


                        i = i__prev1;
                    }
                    return o;
                }
;
            }

            if (count == 1L && !ifaceIndir(typ)) 
                // array of 1 direct iface type can be direct
                array.kind |= kindDirectIface;
            else 
                array.kind &= kindDirectIface;
                        (ti, _) = lookupCache.LoadOrStore(ckey, ref array.rtype);
            return ti._<Type>();
        });

        private static slice<byte> appendVarint(slice<byte> x, System.UIntPtr v)
        {
            while (v >= 0x80UL)
            {
                x = append(x, byte(v | 0x80UL));
                v >>= 7L;
            }

            x = append(x, byte(v));
            return x;
        }

        // toType converts from a *rtype to a Type that can be returned
        // to the client of package reflect. In gc, the only concern is that
        // a nil *rtype must be replaced by a nil Type, but in gccgo this
        // function takes care of ensuring that multiple *rtype for the same
        // type are coalesced into a single Type.
        private static Type toType(ref rtype t)
        {
            if (t == null)
            {
                return null;
            }
            return t;
        }

        private partial struct layoutKey
        {
            public ptr<rtype> t; // function signature
            public ptr<rtype> rcvr; // receiver type, or nil if none
        }

        private partial struct layoutType
        {
            public ptr<rtype> t;
            public System.UIntPtr argSize; // size of arguments
            public System.UIntPtr retOffset; // offset of return values.
            public ptr<bitVector> stack;
            public ptr<sync.Pool> framePool;
        }

        private static sync.Map layoutCache = default; // map[layoutKey]layoutType

        // funcLayout computes a struct type representing the layout of the
        // function arguments and return values for the function type t.
        // If rcvr != nil, rcvr specifies the type of the receiver.
        // The returned type exists only for GC, so we only fill out GC relevant info.
        // Currently, that's just size and the GC program. We also fill in
        // the name for possible debugging use.
        private static (ref rtype, System.UIntPtr, System.UIntPtr, ref bitVector, ref sync.Pool) funcLayout(ref rtype _t, ref rtype _rcvr) => func(_t, _rcvr, (ref rtype t, ref rtype rcvr, Defer _, Panic panic, Recover __) =>
        {
            if (t.Kind() != Func)
            {
                panic("reflect: funcLayout of non-func type");
            }
            if (rcvr != null && rcvr.Kind() == Interface)
            {
                panic("reflect: funcLayout with interface receiver " + rcvr.String());
            }
            layoutKey k = new layoutKey(t,rcvr);
            {
                var lti__prev1 = lti;

                var (lti, ok) = layoutCache.Load(k);

                if (ok)
                {
                    layoutType lt = lti._<layoutType>();
                    return (lt.t, lt.argSize, lt.retOffset, lt.stack, lt.framePool);
                }

                lti = lti__prev1;

            }

            var tt = (funcType.Value)(@unsafe.Pointer(t)); 

            // compute gc program & stack bitmap for arguments
            ptr<bitVector> ptrmap = @new<bitVector>();
            System.UIntPtr offset = default;
            if (rcvr != null)
            { 
                // Reflect uses the "interface" calling convention for
                // methods, where receivers take one word of argument
                // space no matter how big they actually are.
                if (ifaceIndir(rcvr) || rcvr.pointers())
                {
                    ptrmap.append(1L);
                }
                offset += ptrSize;
            }
            foreach (var (_, arg) in tt.@in())
            {
                offset += -offset & uintptr(arg.align - 1L);
                addTypeBits(ptrmap, offset, arg);
                offset += arg.size;
            }
            var argN = ptrmap.n;
            argSize = offset;
            if (runtime.GOARCH == "amd64p32")
            {
                offset += -offset & (8L - 1L);
            }
            offset += -offset & (ptrSize - 1L);
            retOffset = offset;
            foreach (var (_, res) in tt.@out())
            {
                offset += -offset & uintptr(res.align - 1L);
                addTypeBits(ptrmap, offset, res);
                offset += res.size;
            }
            offset += -offset & (ptrSize - 1L); 

            // build dummy rtype holding gc program
            rtype x = ref new rtype(align:ptrSize,size:offset,ptrdata:uintptr(ptrmap.n)*ptrSize,);
            if (runtime.GOARCH == "amd64p32")
            {
                x.align = 8L;
            }
            if (ptrmap.n > 0L)
            {
                x.gcdata = ref ptrmap.data[0L];
            }
            else
            {
                x.kind |= kindNoPointers;
            }
            ptrmap.n = argN;

            @string s = default;
            if (rcvr != null)
            {
                s = "methodargs(" + rcvr.String() + ")(" + t.String() + ")";
            }
            else
            {
                s = "funcargs(" + t.String() + ")";
            }
            x.str = resolveReflectName(newName(s, "", false)); 

            // cache result for future callers
            framePool = ref new sync.Pool(New:func()interface{}{returnunsafe_New(x)});
            var (lti, _) = layoutCache.LoadOrStore(k, new layoutType(t:x,argSize:argSize,retOffset:retOffset,stack:ptrmap,framePool:framePool,));
            lt = lti._<layoutType>();
            return (lt.t, lt.argSize, lt.retOffset, lt.stack, lt.framePool);
        });

        // ifaceIndir reports whether t is stored indirectly in an interface value.
        private static bool ifaceIndir(ref rtype t)
        {
            return t.kind & kindDirectIface == 0L;
        }

        // Layout matches runtime.gobitvector (well enough).
        private partial struct bitVector
        {
            public uint n; // number of bits
            public slice<byte> data;
        }

        // append a bit to the bitmap.
        private static void append(this ref bitVector bv, byte bit)
        {
            if (bv.n % 8L == 0L)
            {
                bv.data = append(bv.data, 0L);
            }
            bv.data[bv.n / 8L] |= bit << (int)((bv.n % 8L));
            bv.n++;
        }

        private static void addTypeBits(ref bitVector bv, System.UIntPtr offset, ref rtype t)
        {
            if (t.kind & kindNoPointers != 0L)
            {
                return;
            }

            if (Kind(t.kind & kindMask) == Chan || Kind(t.kind & kindMask) == Func || Kind(t.kind & kindMask) == Map || Kind(t.kind & kindMask) == Ptr || Kind(t.kind & kindMask) == Slice || Kind(t.kind & kindMask) == String || Kind(t.kind & kindMask) == UnsafePointer) 
                // 1 pointer at start of representation
                while (bv.n < uint32(offset / uintptr(ptrSize)))
                {
                    bv.append(0L);
                }

                bv.append(1L);
            else if (Kind(t.kind & kindMask) == Interface) 
                // 2 pointers
                while (bv.n < uint32(offset / uintptr(ptrSize)))
                {
                    bv.append(0L);
                }

                bv.append(1L);
                bv.append(1L);
            else if (Kind(t.kind & kindMask) == Array) 
                // repeat inner type
                var tt = (arrayType.Value)(@unsafe.Pointer(t));
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < int(tt.len); i++)
                    {
                        addTypeBits(bv, offset + uintptr(i) * tt.elem.size, tt.elem);
                    }


                    i = i__prev1;
                }
            else if (Kind(t.kind & kindMask) == Struct) 
                // apply fields
                tt = (structType.Value)(@unsafe.Pointer(t));
                {
                    long i__prev1 = i;

                    foreach (var (__i) in tt.fields)
                    {
                        i = __i;
                        var f = ref tt.fields[i];
                        addTypeBits(bv, offset + f.offset(), f.typ);
                    }

                    i = i__prev1;
                }
                    }
    }
}
