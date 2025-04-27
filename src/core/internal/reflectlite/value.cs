// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using unsafeheader = @internal.unsafeheader_package;
using runtime = runtime_package;
using @unsafe = unsafe_package;

partial class reflectlite_package {

// Value is the reflection interface to a Go value.
//
// Not all methods apply to all kinds of values. Restrictions,
// if any, are noted in the documentation for each method.
// Use the Kind method to find out the kind of value before
// calling kind-specific methods. Calling a method
// inappropriate to the kind of type causes a run time panic.
//
// The zero Value represents no value.
// Its IsValid method returns false, its Kind method returns Invalid,
// its String method returns "<invalid Value>", and all other methods panic.
// Most functions and methods never return an invalid value.
// If one does, its documentation states the conditions explicitly.
//
// A Value can be used concurrently by multiple goroutines provided that
// the underlying Go value can be used concurrently for the equivalent
// direct operations.
//
// To compare two Values, compare the results of the Interface method.
// Using == on two Values does not compare the underlying values
// they represent.
[GoType] partial struct Value {
    // typ_ holds the type of the value represented by a Value.
    // Access using the typ method to avoid escape of v.
    internal ж<@internal.abi_package.Type> typ_;
    // Pointer-valued data or, if flagIndir is set, pointer to data.
    // Valid when either flagIndir is set or typ.pointers() is true.
    internal @unsafe.Pointer ptr;
    // flag holds metadata about the value.
    // The lowest bits are flag bits:
    //	- flagStickyRO: obtained via unexported not embedded field, so read-only
    //	- flagEmbedRO: obtained via unexported embedded field, so read-only
    //	- flagIndir: val holds a pointer to the data
    //	- flagAddr: v.CanAddr is true (implies flagIndir)
    // Value cannot represent method values.
    // The next five bits give the Kind of the value.
    // This repeats typ.Kind() except for method values.
    // The remaining 23+ bits give a method number for method values.
    // If flag.kind() != Func, code can assume that flagMethod is unset.
    // If ifaceIndir(typ), code can assume that flagIndir is set.
    internal partial ref flag flag { get; }
}

[GoType("num:uintptr")] partial struct flag;

// A method value represents a curried method invocation
// like r.Read for some receiver r. The typ+val+flag bits describe
// the receiver r, but the flag's Kind bits say Func (methods are
// functions), and the top bits of the flag give the method number
// in r's type's method table.
internal static readonly UntypedInt flagKindWidth = 5; // there are 27 kinds
internal static readonly flag flagKindMask = /* 1<<flagKindWidth - 1 */ 31;
internal static readonly flag flagStickyRO = /* 1 << 5 */ 32;
internal static readonly flag flagEmbedRO = /* 1 << 6 */ 64;
internal static readonly flag flagIndir = /* 1 << 7 */ 128;
internal static readonly flag flagAddr = /* 1 << 8 */ 256;
internal static readonly flag flagMethod = /* 1 << 9 */ 512;
internal static readonly UntypedInt flagMethodShift = 10;
internal static readonly flag flagRO = /* flagStickyRO | flagEmbedRO */ 96;

internal static Kind kind(this flag f) {
    return ((Kind)((flag)(f & flagKindMask)));
}

internal static flag ro(this flag f) {
    if ((flag)(f & flagRO) != 0) {
        return flagStickyRO;
    }
    return 0;
}

internal static ж<abi.Type> typ(this Value v) {
    // Types are either static (for compiler-created types) or
    // heap-allocated but always reachable (for reflection-created
    // types, held in the central map). So there is no need to
    // escape types. noescape here help avoid unnecessary escape
    // of v.
    return (ж<abi.Type>)(uintptr)(abi.NoEscape(new @unsafe.Pointer(v.typ_)));
}

// pointer returns the underlying pointer represented by v.
// v.Kind() must be Pointer, Map, Chan, Func, or UnsafePointer
internal static @unsafe.Pointer pointer(this Value v) {
    if (v.typ().Size() != goarch.PtrSize || !v.typ().Pointers()) {
        panic("can't call pointer on a non-pointer Value");
    }
    if ((flag)(v.flag & flagIndir) != 0) {
        return ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr);
    }
    return v.ptr;
}

// packEface converts v to the empty interface.
internal static any packEface(Value v) {
    var t = v.typ();
    any i = default!;
    var e = (ж<abi.EmptyInterface>)(uintptr)(new @unsafe.Pointer(Ꮡi));
    // First, fill in the data portion of the interface.
    switch (ᐧ) {
    case {} when t.IfaceIndir():
        if ((flag)(v.flag & flagIndir) == 0) {
            panic("bad indir");
        }
        var ptr = v.ptr;
        if ((flag)(v.flag & flagAddr) != 0) {
            // Value is indirect, and so is the interface we're making.
            var c = (uintptr)unsafe_New(t);
            typedmemmove(t, c, ptr);
            ptr = c;
        }
        e.val.Data = ptr;
        break;
    case {} when (flag)(v.flag & flagIndir) is != 0:
        e.val.Data = ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr);
        break;
    default:
        e.val.Data = v.ptr;
        break;
    }

    // Value is indirect, but interface is direct. We need
    // to load the data at v.ptr into the interface data word.
    // Value is direct, and so is the interface.
    // Now, fill in the type portion. We're very careful here not
    // to have any operation between the e.word and e.typ assignments
    // that would let the garbage collector observe the partially-built
    // interface value.
    e.val.Type = t;
    return i;
}

// unpackEface converts the empty interface i to a Value.
internal static Value unpackEface(any i) {
    var e = (ж<abi.EmptyInterface>)(uintptr)(new @unsafe.Pointer(Ꮡ(i)));
    // NOTE: don't read e.word until we know whether it is really a pointer or not.
    var t = e.val.Type;
    if (t == nil) {
        return new Value(nil);
    }
    var f = ((flag)t.Kind());
    if (t.IfaceIndir()) {
        f |= (flag)(flagIndir);
    }
    return new Value(Ꮡt, (~e).Data, f);
}

// A ValueError occurs when a Value method is invoked on
// a Value that does not support it. Such cases are documented
// in the description of each method.
[GoType] partial struct ValueError {
    public @string Method;
    public Kind Kind;
}

[GoRecv] public static @string Error(this ref ValueError e) {
    if (e.Kind == 0) {
        return "reflect: call of "u8 + e.Method + " on zero Value"u8;
    }
    return "reflect: call of "u8 + e.Method + " on "u8 + e.Kind.String() + " Value"u8;
}

// methodName returns the name of the calling method,
// assumed to be two stack frames above.
internal static @string methodName() {
    var (pc, _, _, _) = runtime.Caller(2);
    var f = runtime.FuncForPC(pc);
    if (f == nil) {
        return "unknown method"u8;
    }
    return f.Name();
}

// mustBeExported panics if f records that the value was obtained using
// an unexported field.
internal static void mustBeExported(this flag f) {
    if (f == 0) {
        panic(Ꮡ(new ValueError(methodName(), 0)));
    }
    if ((flag)(f & flagRO) != 0) {
        panic("reflect: "u8 + methodName() + " using value obtained using unexported field"u8);
    }
}

// mustBeAssignable panics if f records that the value is not assignable,
// which is to say that either it was obtained using an unexported field
// or it is not addressable.
internal static void mustBeAssignable(this flag f) {
    if (f == 0) {
        panic(Ꮡ(new ValueError(methodName(), abi.Invalid)));
    }
    // Assignable if addressable and not read-only.
    if ((flag)(f & flagRO) != 0) {
        panic("reflect: "u8 + methodName() + " using value obtained using unexported field"u8);
    }
    if ((flag)(f & flagAddr) == 0) {
        panic("reflect: "u8 + methodName() + " using unaddressable value"u8);
    }
}

// CanSet reports whether the value of v can be changed.
// A Value can be changed only if it is addressable and was not
// obtained by the use of unexported struct fields.
// If CanSet returns false, calling Set or any type-specific
// setter (e.g., SetBool, SetInt) will panic.
public static bool CanSet(this Value v) {
    return (flag)(v.flag & ((flag)(flagAddr | flagRO))) == flagAddr;
}

[GoType("dyn")] partial interface Elem_type {
    void M();
}

// Elem returns the value that the interface v contains
// or that the pointer v points to.
// It panics if v's Kind is not Interface or Pointer.
// It returns the zero Value if v is nil.
public static Value Elem(this Value v) {
    var k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == abi.Interface) {
        any eface = default!;
        if (v.typ().NumMethod() == 0){
            eface = ~(ж<any>)(uintptr)(v.ptr);
        } else {
            eface = ((any)((ж<Elem_type>)(uintptr)(v.ptr).val));
        }
        var x = unpackEface(eface);
        if (x.flag != 0) {
            x.flag |= (flag)(v.flag.ro());
        }
        return x;
    }
    if (exprᴛ1 == abi.Pointer) {
        var ptr = v.ptr;
        if ((flag)(v.flag & flagIndir) != 0) {
            ptr = ~(ж<@unsafe.Pointer>)(uintptr)(ptr);
        }
        if (ptr == nil) {
            // The returned value's address is v's value.
            return new Value(nil);
        }
        var tt = (ж<ptrType>)(uintptr)(new @unsafe.Pointer(v.typ()));
        var typ = tt.val.Elem;
        var fl = (flag)((flag)((flag)(v.flag & flagRO) | flagIndir) | flagAddr);
        fl |= (flag)(((flag)typ.Kind()));
        return new Value(Ꮡtyp, ptr.val, fl);
    }

    panic(Ꮡ(new ValueError("reflectlite.Value.Elem", v.kind())));
}

[GoType("dyn")] partial interface valueInterface_type {
    void M();
}

internal static any valueInterface(Value v) {
    if (v.flag == 0) {
        panic(Ꮡ(new ValueError("reflectlite.Value.Interface", 0)));
    }
    if (v.kind() == abi.Interface) {
        // Special case: return the element inside the interface.
        // Empty interface has one layout, all interfaces with
        // methods have a second layout.
        if (v.numMethod() == 0) {
            return ~(ж<any>)(uintptr)(v.ptr);
        }
        return (ж<valueInterface_type>)(uintptr)(v.ptr).val;
    }
    return packEface(v);
}

// IsNil reports whether its argument v is nil. The argument must be
// a chan, func, interface, map, pointer, or slice value; if it is
// not, IsNil panics. Note that IsNil is not always equivalent to a
// regular comparison with nil in Go. For example, if v was created
// by calling ValueOf with an uninitialized interface variable i,
// i==nil will be true but v.IsNil will panic as v will be the zero
// Value.
public static bool IsNil(this Value v) {
    var k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == abi.Chan || exprᴛ1 == abi.Func || exprᴛ1 == abi.Map || exprᴛ1 == abi.Pointer || exprᴛ1 == abi.UnsafePointer) {
        var ptr = v.ptr;
        if ((flag)(v.flag & flagIndir) != 0) {
            // if v.flag&flagMethod != 0 {
            // 	return false
            // }
            ptr = ~(ж<@unsafe.Pointer>)(uintptr)(ptr);
        }
        return ptr == nil;
    }
    if (exprᴛ1 == abi.Interface || exprᴛ1 == abi.Slice) {
        return ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr) == nil;
    }

    // Both interface and slice are nil if first word is 0.
    // Both are always bigger than a word; assume flagIndir.
    panic(Ꮡ(new ValueError("reflectlite.Value.IsNil", v.kind())));
}

// IsValid reports whether v represents a value.
// It returns false if v is the zero Value.
// If IsValid returns false, all other methods except String panic.
// Most functions and methods never return an invalid Value.
// If one does, its documentation states the conditions explicitly.
public static bool IsValid(this Value v) {
    return v.flag != 0;
}

// Kind returns v's Kind.
// If v is the zero Value (IsValid returns false), Kind returns Invalid.
public static Kind Kind(this Value v) {
    return v.kind();
}

// implemented in runtime:

//go:noescape
internal static partial nint chanlen(@unsafe.Pointer _);

//go:noescape
internal static partial nint maplen(@unsafe.Pointer _);

// Len returns v's length.
// It panics if v's Kind is not Array, Chan, Map, Slice, or String.
public static nint Len(this Value v) {
    var k = v.kind();
    var exprᴛ1 = k;
    if (exprᴛ1 == abi.Array) {
        var tt = (ж<arrayType>)(uintptr)(new @unsafe.Pointer(v.typ()));
        return ((nint)(~tt).Len);
    }
    if (exprᴛ1 == abi.Chan) {
        return chanlen((uintptr)v.pointer());
    }
    if (exprᴛ1 == abi.Map) {
        return maplen((uintptr)v.pointer());
    }
    if (exprᴛ1 == abi.Slice) {
        return ((ж<unsafeheader.Slice>)(uintptr)(v.ptr)).val.Len;
    }
    if (exprᴛ1 == abi.ΔString) {
        return ((ж<unsafeheader.String>)(uintptr)(v.ptr)).val.Len;
    }

    // Slice is bigger than a word; assume flagIndir.
    // String is bigger than a word; assume flagIndir.
    panic(Ꮡ(new ValueError("reflect.Value.Len", v.kind())));
}

// NumMethod returns the number of exported methods in the value's method set.
internal static nint numMethod(this Value v) {
    if (v.typ() == nil) {
        panic(Ꮡ(new ValueError("reflectlite.Value.NumMethod", abi.Invalid)));
    }
    return v.typ().NumMethod();
}

// Set assigns x to the value v.
// It panics if CanSet returns false.
// As in Go, x's value must be assignable to v's type.
public static void Set(this Value v, Value x) {
    v.mustBeAssignable();
    x.mustBeExported();
    // do not let unexported x leak
    @unsafe.Pointer target = default!;
    if (v.kind() == abi.Interface) {
        target = v.ptr;
    }
    x = x.assignTo("reflectlite.Set"u8, v.typ(), target);
    if ((flag)(x.flag & flagIndir) != 0){
        typedmemmove(v.typ(), v.ptr, x.ptr);
    } else {
        ~(ж<@unsafe.Pointer>)(uintptr)(v.ptr) = x.ptr;
    }
}

// Type returns v's type.
public static ΔType Type(this Value v) {
    var f = v.flag;
    if (f == 0) {
        panic(Ꮡ(new ValueError("reflectlite.Value.Type", abi.Invalid)));
    }
    // Method values not supported.
    return toRType(v.typ());
}

/*
 * constructors
 */
// implemented in package runtime

//go:noescape
internal static partial @unsafe.Pointer unsafe_New(ж<abi.Type> _);

// ValueOf returns a new Value initialized to the concrete value
// stored in the interface i. ValueOf(nil) returns the zero Value.
public static Value ValueOf(any i) {
    if (i == default!) {
        return new Value(nil);
    }
    return unpackEface(i);
}

// assignTo returns a value v that can be assigned directly to typ.
// It panics if v is not assignable to typ.
// For a conversion to an interface type, target is a suggested scratch space to use.
public static Value assignTo(this Value v, @string context, ж<abi.Type> Ꮡdst, @unsafe.Pointer target) {
    ref var dst = ref Ꮡdst.val;

    // if v.flag&flagMethod != 0 {
    // 	v = makeMethodValue(context, v)
    // }
    switch (ᐧ) {
    case {} when directlyAssignable(Ꮡdst, v.typ()):
        var fl = (flag)((flag)(v.flag & ((flag)(flagAddr | flagIndir))) | v.flag.ro());
        fl |= (flag)(((flag)dst.Kind()));
        return new Value( // Overwrite type so that they match.
 // Same memory layout, so no harm done.
Ꮡdst, v.ptr, fl);
    case {} when implements(Ꮡdst, v.typ()):
        if (target == nil) {
            target = (uintptr)unsafe_New(Ꮡdst);
        }
        if (v.Kind() == abi.Interface && v.IsNil()) {
            // A nil ReadWriter passed to nil Reader is OK,
            // but using ifaceE2I below will panic.
            // Avoid the panic by returning a nil dst (e.g., Reader) explicitly.
            return new Value(Ꮡdst, nil, ((flag)abi.Interface));
        }
        var x = valueInterface(v);
        if (dst.NumMethod() == 0){
            ~(ж<any>)(uintptr)(target) = x;
        } else {
            ifaceE2I(Ꮡdst, x, target.val);
        }
        return new Value(Ꮡdst, target.val, (flag)(flagIndir | ((flag)abi.Interface)));
    }

    // Failed.
    panic(context + ": value of type "u8 + toRType(v.typ()).String() + " is not assignable to type "u8 + toRType(Ꮡdst).String());
}

// arrayAt returns the i-th element of p,
// an array whose elements are eltSize bytes wide.
// The array pointed at by p must have at least i+1 elements:
// it is invalid (but impossible to check here) to pass i >= len,
// because then the result will point outside the array.
// whySafe must explain why i < len. (Passing "i < len" is fine;
// the benefit is to surface this assumption at the call site.)
internal static @unsafe.Pointer arrayAt(@unsafe.Pointer p, nint i, uintptr eltSize, @string whySafe) {
    return (uintptr)add(p.val, ((uintptr)i) * eltSize, "i < len"u8);
}

internal static partial void ifaceE2I(ж<abi.Type> t, any src, @unsafe.Pointer dst);

// typedmemmove copies a value of type t to dst from src.
//
//go:noescape
internal static partial void typedmemmove(ж<abi.Type> t, @unsafe.Pointer dst, @unsafe.Pointer src);

// Dummy annotation marking that the value x escapes,
// for use in cases where the reflect code is so clever that
// the compiler cannot follow.
internal static void escapes(any x) {
    if (dummy.b) {
        dummy.x = x;
    }
}


[GoType("dyn")] partial struct dummyᴛ1 {
    internal bool b;
    internal any x;
}
internal static dummyᴛ1 dummy;

} // end reflectlite_package
