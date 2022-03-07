// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 06 22:42:22 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\type.go
using fmt = go.fmt_package;
using token = go.go.token_package;
using atomic = go.sync.atomic_package;
using System;


namespace go.go;

public static partial class types_package {

    // A Type represents a type of Go.
    // All types implement the Type interface.
public partial interface Type {
    @string Underlying(); // String returns a string representation of a type.
    @string String();
}

// BasicKind describes the kind of basic type.
public partial struct BasicKind { // : nint
}

public static readonly BasicKind Invalid = iota; // type is invalid

// predeclared types
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
public static readonly var String = 16;
public static readonly var UnsafePointer = 17; 

// types for untyped values
public static readonly var UntypedBool = 18;
public static readonly var UntypedInt = 19;
public static readonly var UntypedRune = 20;
public static readonly var UntypedFloat = 21;
public static readonly var UntypedComplex = 22;
public static readonly var UntypedString = 23;
public static readonly Byte UntypedNil = Uint8;
public static readonly var Rune = Int32;


// BasicInfo is a set of flags describing properties of a basic type.
public partial struct BasicInfo { // : nint
}

// Properties of basic types.
public static readonly BasicInfo IsBoolean = 1 << (int)(iota);
public static readonly var IsInteger = 0;
public static readonly var IsUnsigned = 1;
public static readonly var IsFloat = 2;
public static readonly var IsComplex = 3;
public static readonly var IsString = 4;
public static readonly IsOrdered IsUntyped = IsInteger | IsFloat | IsString;
public static readonly var IsNumeric = IsInteger | IsFloat | IsComplex;
public static readonly var IsConstType = IsBoolean | IsNumeric | IsString;


// A Basic represents a basic type.
public partial struct Basic {
    public BasicKind kind;
    public BasicInfo info;
    public @string name;
}

// Kind returns the kind of basic type b.
private static BasicKind Kind(this ptr<Basic> _addr_b) {
    ref Basic b = ref _addr_b.val;

    return b.kind;
}

// Info returns information about properties of basic type b.
private static BasicInfo Info(this ptr<Basic> _addr_b) {
    ref Basic b = ref _addr_b.val;

    return b.info;
}

// Name returns the name of basic type b.
private static @string Name(this ptr<Basic> _addr_b) {
    ref Basic b = ref _addr_b.val;

    return b.name;
}

// An Array represents an array type.
public partial struct Array {
    public long len;
    public Type elem;
}

// NewArray returns a new array type for the given element type and length.
// A negative length indicates an unknown length.
public static ptr<Array> NewArray(Type elem, long len) {
    return addr(new Array(len:len,elem:elem));
}

// Len returns the length of array a.
// A negative result indicates an unknown length.
private static long Len(this ptr<Array> _addr_a) {
    ref Array a = ref _addr_a.val;

    return a.len;
}

// Elem returns element type of array a.
private static Type Elem(this ptr<Array> _addr_a) {
    ref Array a = ref _addr_a.val;

    return a.elem;
}

// A Slice represents a slice type.
public partial struct Slice {
    public Type elem;
}

// NewSlice returns a new slice type for the given element type.
public static ptr<Slice> NewSlice(Type elem) {
    return addr(new Slice(elem:elem));
}

// Elem returns the element type of slice s.
private static Type Elem(this ptr<Slice> _addr_s) {
    ref Slice s = ref _addr_s.val;

    return s.elem;
}

// A Struct represents a struct type.
public partial struct Struct {
    public slice<ptr<Var>> fields;
    public slice<@string> tags; // field tags; nil if there are no tags
}

// NewStruct returns a new struct with the given fields and corresponding field tags.
// If a field with index i has a tag, tags[i] must be that tag, but len(tags) may be
// only as long as required to hold the tag with the largest index i. Consequently,
// if no field has a tag, tags may be nil.
public static ptr<Struct> NewStruct(slice<ptr<Var>> fields, slice<@string> tags) => func((_, panic, _) => {
    objset fset = default;
    foreach (var (_, f) in fields) {
        if (f.name != "_" && fset.insert(f) != null) {
            panic("multiple fields with the same name");
        }
    }    if (len(tags) > len(fields)) {
        panic("more tags than fields");
    }
    return addr(new Struct(fields:fields,tags:tags));

});

// NumFields returns the number of fields in the struct (including blank and embedded fields).
private static nint NumFields(this ptr<Struct> _addr_s) {
    ref Struct s = ref _addr_s.val;

    return len(s.fields);
}

// Field returns the i'th field for 0 <= i < NumFields().
private static ptr<Var> Field(this ptr<Struct> _addr_s, nint i) {
    ref Struct s = ref _addr_s.val;

    return _addr_s.fields[i]!;
}

// Tag returns the i'th field tag for 0 <= i < NumFields().
private static @string Tag(this ptr<Struct> _addr_s, nint i) {
    ref Struct s = ref _addr_s.val;

    if (i < len(s.tags)) {
        return s.tags[i];
    }
    return "";

}

// A Pointer represents a pointer type.
public partial struct Pointer {
    public Type @base; // element type
}

// NewPointer returns a new pointer type for the given element (base) type.
public static ptr<Pointer> NewPointer(Type elem) {
    return addr(new Pointer(base:elem));
}

// Elem returns the element type for the given pointer p.
private static Type Elem(this ptr<Pointer> _addr_p) {
    ref Pointer p = ref _addr_p.val;

    return p.@base;
}

// A Tuple represents an ordered list of variables; a nil *Tuple is a valid (empty) tuple.
// Tuples are used as components of signatures and to represent the type of multiple
// assignments; they are not first class types of Go.
public partial struct Tuple {
    public slice<ptr<Var>> vars;
}

// NewTuple returns a new tuple for the given variables.
public static ptr<Tuple> NewTuple(params ptr<ptr<Var>>[] _addr_x) {
    x = x.Clone();
    ref Var x = ref _addr_x.val;

    if (len(x) > 0) {
        return addr(new Tuple(vars:x));
    }
    return _addr_null!;

}

// Len returns the number variables of tuple t.
private static nint Len(this ptr<Tuple> _addr_t) {
    ref Tuple t = ref _addr_t.val;

    if (t != null) {
        return len(t.vars);
    }
    return 0;

}

// At returns the i'th variable of tuple t.
private static ptr<Var> At(this ptr<Tuple> _addr_t, nint i) {
    ref Tuple t = ref _addr_t.val;

    return _addr_t.vars[i]!;
}

// A Signature represents a (non-builtin) function or method type.
// The receiver is ignored when comparing signatures for identity.
public partial struct Signature {
    public slice<ptr<TypeName>> rparams; // receiver type parameters from left to right, or nil
    public slice<ptr<TypeName>> tparams; // type parameters from left to right, or nil
    public ptr<Scope> scope; // function scope, present for package-local signatures
    public ptr<Var> recv; // nil if not a method
    public ptr<Tuple> @params; // (incoming) parameters from left to right; or nil
    public ptr<Tuple> results; // (outgoing) results from left to right; or nil
    public bool variadic; // true if the last parameter's type is of the form ...T (or string, for append built-in only)
}

// NewSignature returns a new function type for the given receiver, parameters,
// and results, either of which may be nil. If variadic is set, the function
// is variadic, it must have at least one parameter, and the last parameter
// must be of unnamed slice type.
public static ptr<Signature> NewSignature(ptr<Var> _addr_recv, ptr<Tuple> _addr_@params, ptr<Tuple> _addr_results, bool variadic) => func((_, panic, _) => {
    ref Var recv = ref _addr_recv.val;
    ref Tuple @params = ref _addr_@params.val;
    ref Tuple results = ref _addr_results.val;

    if (variadic) {
        var n = @params.Len();
        if (n == 0) {
            panic("types.NewSignature: variadic function must have at least one parameter");
        }
        {
            ptr<Slice> (_, ok) = @params.At(n - 1).typ._<ptr<Slice>>();

            if (!ok) {
                panic("types.NewSignature: variadic parameter must be of unnamed slice type");
            }

        }

    }
    return addr(new Signature(recv:recv,params:params,results:results,variadic:variadic));

});

// Recv returns the receiver of signature s (if a method), or nil if a
// function. It is ignored when comparing signatures for identity.
//
// For an abstract method, Recv returns the enclosing interface either
// as a *Named or an *Interface. Due to embedding, an interface may
// contain methods whose receiver type is a different interface.
private static ptr<Var> Recv(this ptr<Signature> _addr_s) {
    ref Signature s = ref _addr_s.val;

    return _addr_s.recv!;
}

// _TParams returns the type parameters of signature s, or nil.
private static slice<ptr<TypeName>> _TParams(this ptr<Signature> _addr_s) {
    ref Signature s = ref _addr_s.val;

    return s.tparams;
}

// _SetTParams sets the type parameters of signature s.
private static void _SetTParams(this ptr<Signature> _addr_s, slice<ptr<TypeName>> tparams) {
    ref Signature s = ref _addr_s.val;

    s.tparams = tparams;
}

// Params returns the parameters of signature s, or nil.
private static ptr<Tuple> Params(this ptr<Signature> _addr_s) {
    ref Signature s = ref _addr_s.val;

    return _addr_s.@params!;
}

// Results returns the results of signature s, or nil.
private static ptr<Tuple> Results(this ptr<Signature> _addr_s) {
    ref Signature s = ref _addr_s.val;

    return _addr_s.results!;
}

// Variadic reports whether the signature s is variadic.
private static bool Variadic(this ptr<Signature> _addr_s) {
    ref Signature s = ref _addr_s.val;

    return s.variadic;
}

// A _Sum represents a set of possible types.
// Sums are currently used to represent type lists of interfaces
// and thus the underlying types of type parameters; they are not
// first class types of Go.
private partial struct _Sum {
    public slice<Type> types; // types are unique
}

// _NewSum returns a new Sum type consisting of the provided
// types if there are more than one. If there is exactly one
// type, it returns that type. If the list of types is empty
// the result is nil.
private static Type _NewSum(slice<Type> types) => func((_, panic, _) => {
    if (len(types) == 0) {
        return null;
    }
    foreach (var (_, t) in types) {
        {
            ptr<_Sum> (_, ok) = t._<ptr<_Sum>>();

            if (ok) {
                panic("sum type contains sum type - unimplemented");
            }

        }

    }    if (len(types) == 1) {
        return types[0];
    }
    return addr(new _Sum(types:types));

});

// is reports whether all types in t satisfy pred.
private static bool @is(this ptr<_Sum> _addr_s, Func<Type, bool> pred) {
    ref _Sum s = ref _addr_s.val;

    if (s == null) {
        return false;
    }
    foreach (var (_, t) in s.types) {
        if (!pred(t)) {
            return false;
        }
    }    return true;

}

// An Interface represents an interface type.
public partial struct Interface {
    public slice<ptr<Func>> methods; // ordered list of explicitly declared methods
    public Type types; // (possibly a Sum) type declared with a type list (TODO(gri) need better field name)
    public slice<Type> embeddeds; // ordered list of explicitly embedded types

    public slice<ptr<Func>> allMethods; // ordered list of methods declared with or embedded in this interface (TODO(gri): replace with mset)
    public Type allTypes; // intersection of all embedded and locally declared types  (TODO(gri) need better field name)

    public Object obj; // type declaration defining this interface; or nil (for better error messages)
}

// unpack unpacks a type into a list of types.
// TODO(gri) Try to eliminate the need for this function.
private static slice<Type> unpackType(Type typ) {
    if (typ == null) {
        return null;
    }
    {
        var sum = asSum(typ);

        if (sum != null) {
            return sum.types;
        }
    }

    return new slice<Type>(new Type[] { Type.As(typ)! });

}

// is reports whether interface t represents types that all satisfy pred.
private static bool @is(this ptr<Interface> _addr_t, Func<Type, bool> pred) {
    ref Interface t = ref _addr_t.val;

    if (t.allTypes == null) {
        return false; // we must have at least one type! (was bug)
    }
    foreach (var (_, t) in unpackType(t.allTypes)) {
        if (!pred(t)) {
            return false;
        }
    }    return true;

}

// emptyInterface represents the empty (completed) interface
private static Interface emptyInterface = new Interface(allMethods:markComplete);

// markComplete is used to mark an empty interface as completely
// set up by setting the allMethods field to a non-nil empty slice.
private static var markComplete = make_slice<ptr<Func>>(0);

// NewInterface returns a new (incomplete) interface for the given methods and embedded types.
// Each embedded type must have an underlying type of interface type.
// NewInterface takes ownership of the provided methods and may modify their types by setting
// missing receivers. To compute the method set of the interface, Complete must be called.
//
// Deprecated: Use NewInterfaceType instead which allows any (even non-defined) interface types
// to be embedded. This is necessary for interfaces that embed alias type names referring to
// non-defined (literal) interface types.
public static ptr<Interface> NewInterface(slice<ptr<Func>> methods, slice<ptr<Named>> embeddeds) {
    var tnames = make_slice<Type>(len(embeddeds));
    foreach (var (i, t) in embeddeds) {
        tnames[i] = t;
    }    return _addr_NewInterfaceType(methods, tnames)!;
}

// NewInterfaceType returns a new (incomplete) interface for the given methods and embedded types.
// Each embedded type must have an underlying type of interface type (this property is not
// verified for defined types, which may be in the process of being set up and which don't
// have a valid underlying type yet).
// NewInterfaceType takes ownership of the provided methods and may modify their types by setting
// missing receivers. To compute the method set of the interface, Complete must be called.
public static ptr<Interface> NewInterfaceType(slice<ptr<Func>> methods, slice<Type> embeddeds) => func((_, panic, _) => {
    if (len(methods) == 0 && len(embeddeds) == 0) {
        return _addr__addr_emptyInterface!;
    }
    ptr<Interface> typ = @new<Interface>();
    foreach (var (_, m) in methods) {
        {
            ptr<Signature> sig = m.typ._<ptr<Signature>>();

            if (sig.recv == null) {
                sig.recv = NewVar(m.pos, m.pkg, "", typ);
            }

        }

    }    foreach (var (_, t) in embeddeds) {
        {
            ptr<Named> (_, ok) = t._<ptr<Named>>();

            if (!ok && !IsInterface(t)) {
                panic("embedded type is not an interface");
            }

        }

    }    sortMethods(methods);
    sortTypes(embeddeds);

    typ.methods = methods;
    typ.embeddeds = embeddeds;
    return _addr_typ!;

});

// NumExplicitMethods returns the number of explicitly declared methods of interface t.
private static nint NumExplicitMethods(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    return len(t.methods);
}

// ExplicitMethod returns the i'th explicitly declared method of interface t for 0 <= i < t.NumExplicitMethods().
// The methods are ordered by their unique Id.
private static ptr<Func> ExplicitMethod(this ptr<Interface> _addr_t, nint i) {
    ref Interface t = ref _addr_t.val;

    return _addr_t.methods[i]!;
}

// NumEmbeddeds returns the number of embedded types in interface t.
private static nint NumEmbeddeds(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    return len(t.embeddeds);
}

// Embedded returns the i'th embedded defined (*Named) type of interface t for 0 <= i < t.NumEmbeddeds().
// The result is nil if the i'th embedded type is not a defined type.
//
// Deprecated: Use EmbeddedType which is not restricted to defined (*Named) types.
private static ptr<Named> Embedded(this ptr<Interface> _addr_t, nint i) {
    ref Interface t = ref _addr_t.val;

    ptr<Named> (tname, _) = t.embeddeds[i]._<ptr<Named>>();

    return _addr_tname!;
}

// EmbeddedType returns the i'th embedded type of interface t for 0 <= i < t.NumEmbeddeds().
private static Type EmbeddedType(this ptr<Interface> _addr_t, nint i) {
    ref Interface t = ref _addr_t.val;

    return t.embeddeds[i];
}

// NumMethods returns the total number of methods of interface t.
// The interface must have been completed.
private static nint NumMethods(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    t.assertCompleteness();

    return len(t.allMethods);
}

private static void assertCompleteness(this ptr<Interface> _addr_t) => func((_, panic, _) => {
    ref Interface t = ref _addr_t.val;

    if (t.allMethods == null) {
        panic("interface is incomplete");
    }
});

// Method returns the i'th method of interface t for 0 <= i < t.NumMethods().
// The methods are ordered by their unique Id.
// The interface must have been completed.
private static ptr<Func> Method(this ptr<Interface> _addr_t, nint i) {
    ref Interface t = ref _addr_t.val;

    t.assertCompleteness();

    return _addr_t.allMethods[i]!;
}

// Empty reports whether t is the empty interface.
private static bool Empty(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    if (t.allMethods != null) { 
        // interface is complete - quick test
        // A non-nil allTypes may still be empty and represents the bottom type.
        return len(t.allMethods) == 0 && t.allTypes == null;

    }
    return !t.iterate(t => {
        return len(t.methods) > 0 || t.types != null;
    }, null);

}

// _HasTypeList reports whether interface t has a type list, possibly from an embedded type.
private static bool _HasTypeList(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    if (t.allMethods != null) { 
        // interface is complete - quick test
        return t.allTypes != null;

    }
    return t.iterate(t => {
        return t.types != null;
    }, null);

}

// _IsComparable reports whether interface t is or embeds the predeclared interface "comparable".
private static bool _IsComparable(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    if (t.allMethods != null) { 
        // interface is complete - quick test
        var (_, m) = lookupMethod(t.allMethods, null, "==");
        return m != null;

    }
    return t.iterate(t => {
        (_, m) = lookupMethod(t.methods, null, "==");
        return m != null;
    }, null);

}

// _IsConstraint reports t.HasTypeList() || t.IsComparable().
private static bool _IsConstraint(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    if (t.allMethods != null) { 
        // interface is complete - quick test
        if (t.allTypes != null) {
            return true;
        }
        var (_, m) = lookupMethod(t.allMethods, null, "==");
        return m != null;

    }
    return t.iterate(t => {
        if (t.types != null) {
            return true;
        }
        (_, m) = lookupMethod(t.methods, null, "==");
        return m != null;

    }, null);

}

// iterate calls f with t and then with any embedded interface of t, recursively, until f returns true.
// iterate reports whether any call to f returned true.
private static bool iterate(this ptr<Interface> _addr_t, Func<ptr<Interface>, bool> f, map<ptr<Interface>, bool> seen) {
    ref Interface t = ref _addr_t.val;

    if (f(t)) {
        return true;
    }
    {
        var e__prev1 = e;

        foreach (var (_, __e) in t.embeddeds) {
            e = __e; 
            // e should be an interface but be careful (it may be invalid)
            {
                var e__prev1 = e;

                var e = asInterface(e);

                if (e != null) { 
                    // Cyclic interfaces such as "type E interface { E }" are not permitted
                    // but they are still constructed and we need to detect such cycles.
                    if (seen[e]) {
                        continue;
                    }

                    if (seen == null) {
                        seen = make_map<ptr<Interface>, bool>();
                    }

                    seen[e] = true;
                    if (e.iterate(f, seen)) {
                        return true;
                    }

                }

                e = e__prev1;

            }

        }
        e = e__prev1;
    }

    return false;

}

// isSatisfiedBy reports whether interface t's type list is satisfied by the type typ.
// If the type list is empty (absent), typ trivially satisfies the interface.
// TODO(gri) This is not a great name. Eventually, we should have a more comprehensive
//           "implements" predicate.
private static bool isSatisfiedBy(this ptr<Interface> _addr_t, Type typ) {
    ref Interface t = ref _addr_t.val;

    t.Complete();
    if (t.allTypes == null) {
        return true;
    }
    var types = unpackType(t.allTypes);
    return includes(types, typ) || includes(types, under(typ));

}

// Complete computes the interface's method set. It must be called by users of
// NewInterfaceType and NewInterface after the interface's embedded types are
// fully defined and before using the interface type in any way other than to
// form other types. The interface must not contain duplicate methods or a
// panic occurs. Complete returns the receiver.
private static ptr<Interface> Complete(this ptr<Interface> _addr_t) => func((_, panic, _) => {
    ref Interface t = ref _addr_t.val;
 
    // TODO(gri) consolidate this method with Checker.completeInterface
    if (t.allMethods != null) {
        return _addr_t!;
    }
    t.allMethods = markComplete; // avoid infinite recursion

    slice<ptr<Func>> todo = default;
    slice<ptr<Func>> methods = default;
    objset seen = default;
    Action<ptr<Func>, bool> addMethod = (m, @explicit) => {
        {
            var other__prev1 = other;

            var other = seen.insert(m);


            if (other == null) 
                methods = append(methods, m);
            else if (explicit) 
                panic("duplicate method " + m.name);
            else 
                // check method signatures after all locally embedded interfaces are computed
                todo = append(todo, m, other._<ptr<Func>>());


            other = other__prev1;
        }

    };

    {
        var m__prev1 = m;

        foreach (var (_, __m) in t.methods) {
            m = __m;
            addMethod(m, true);
        }
        m = m__prev1;
    }

    var allTypes = t.types;

    foreach (var (_, typ) in t.embeddeds) {
        var utyp = under(typ);
        var etyp = asInterface(utyp);
        if (etyp == null) {
            if (utyp != Typ[Invalid]) {
                panic(fmt.Sprintf("%s is not an interface", typ));
            }
            continue;
        }
        etyp.Complete();
        {
            var m__prev2 = m;

            foreach (var (_, __m) in etyp.allMethods) {
                m = __m;
                addMethod(m, false);
            }

            m = m__prev2;
        }

        allTypes = intersect(allTypes, etyp.allTypes);

    }    {
        nint i = 0;

        while (i < len(todo)) {
            var m = todo[i];
            other = todo[i + 1];
            if (!Identical(m.typ, other.typ)) {
                panic("duplicate method " + m.name);
            i += 2;
            }

        }
    }

    if (methods != null) {
        sortMethods(methods);
        t.allMethods = methods;
    }
    t.allTypes = allTypes;

    return _addr_t!;

});

// A Map represents a map type.
public partial struct Map {
    public Type key;
    public Type elem;
}

// NewMap returns a new map for the given key and element types.
public static ptr<Map> NewMap(Type key, Type elem) {
    return addr(new Map(key:key,elem:elem));
}

// Key returns the key type of map m.
private static Type Key(this ptr<Map> _addr_m) {
    ref Map m = ref _addr_m.val;

    return m.key;
}

// Elem returns the element type of map m.
private static Type Elem(this ptr<Map> _addr_m) {
    ref Map m = ref _addr_m.val;

    return m.elem;
}

// A Chan represents a channel type.
public partial struct Chan {
    public ChanDir dir;
    public Type elem;
}

// A ChanDir value indicates a channel direction.
public partial struct ChanDir { // : nint
}

// The direction of a channel is indicated by one of these constants.
public static readonly ChanDir SendRecv = iota;
public static readonly var SendOnly = 0;
public static readonly var RecvOnly = 1;


// NewChan returns a new channel type for the given direction and element type.
public static ptr<Chan> NewChan(ChanDir dir, Type elem) {
    return addr(new Chan(dir:dir,elem:elem));
}

// Dir returns the direction of channel c.
private static ChanDir Dir(this ptr<Chan> _addr_c) {
    ref Chan c = ref _addr_c.val;

    return c.dir;
}

// Elem returns the element type of channel c.
private static Type Elem(this ptr<Chan> _addr_c) {
    ref Chan c = ref _addr_c.val;

    return c.elem;
}

// A Named represents a named (defined) type.
public partial struct Named {
    public ptr<Checker> check; // for Named.under implementation; nilled once under has been called
    public typeInfo info; // for cycle detection
    public ptr<TypeName> obj; // corresponding declared object
    public Type orig; // type (on RHS of declaration) this *Named type is derived of (for cycle reporting)
    public Type underlying; // possibly a *Named during setup; never a *Named once set up completely
    public slice<ptr<TypeName>> tparams; // type parameters, or nil
    public slice<Type> targs; // type arguments (after instantiation), or nil
    public slice<ptr<Func>> methods; // methods declared for this type (not the method set of this type); signatures are type-checked lazily
}

// NewNamed returns a new named type for the given type name, underlying type, and associated methods.
// If the given type name obj doesn't have a type yet, its type is set to the returned named type.
// The underlying type must not be a *Named.
public static ptr<Named> NewNamed(ptr<TypeName> _addr_obj, Type underlying, slice<ptr<Func>> methods) => func((_, panic, _) => {
    ref TypeName obj = ref _addr_obj.val;

    {
        ptr<Named> (_, ok) = underlying._<ptr<Named>>();

        if (ok) {
            panic("types.NewNamed: underlying type must not be *Named");
        }
    }

    return _addr_(Checker.val)(null).newNamed(obj, underlying, methods)!;

});

private static ptr<Named> newNamed(this ptr<Checker> _addr_check, ptr<TypeName> _addr_obj, Type underlying, slice<ptr<Func>> methods) => func((_, panic, _) => {
    ref Checker check = ref _addr_check.val;
    ref TypeName obj = ref _addr_obj.val;

    ptr<Named> typ = addr(new Named(check:check,obj:obj,orig:underlying,underlying:underlying,methods:methods));
    if (obj.typ == null) {
        obj.typ = typ;
    }
    if (check != null) {
        check.later(() => {
            switch (typ.under().type()) {
                case ptr<Named> _:
                    panic("internal error: unexpanded underlying type");
                    break;
                case ptr<instance> _:
                    panic("internal error: unexpanded underlying type");
                    break;
            }
            typ.check = null;

        });

    }
    return _addr_typ!;

});

// Obj returns the type name for the named type t.
private static ptr<TypeName> Obj(this ptr<Named> _addr_t) {
    ref Named t = ref _addr_t.val;

    return _addr_t.obj!;
}

// TODO(gri) Come up with a better representation and API to distinguish
//           between parameterized instantiated and non-instantiated types.

// _TParams returns the type parameters of the named type t, or nil.
// The result is non-nil for an (originally) parameterized type even if it is instantiated.
private static slice<ptr<TypeName>> _TParams(this ptr<Named> _addr_t) {
    ref Named t = ref _addr_t.val;

    return t.tparams;
}

// _TArgs returns the type arguments after instantiation of the named type t, or nil if not instantiated.
private static slice<Type> _TArgs(this ptr<Named> _addr_t) {
    ref Named t = ref _addr_t.val;

    return t.targs;
}

// _SetTArgs sets the type arguments of Named.
private static void _SetTArgs(this ptr<Named> _addr_t, slice<Type> args) {
    ref Named t = ref _addr_t.val;

    t.targs = args;
}

// NumMethods returns the number of explicit methods whose receiver is named type t.
private static nint NumMethods(this ptr<Named> _addr_t) {
    ref Named t = ref _addr_t.val;

    return len(t.methods);
}

// Method returns the i'th method of named type t for 0 <= i < t.NumMethods().
private static ptr<Func> Method(this ptr<Named> _addr_t, nint i) {
    ref Named t = ref _addr_t.val;

    return _addr_t.methods[i]!;
}

// SetUnderlying sets the underlying type and marks t as complete.
private static void SetUnderlying(this ptr<Named> _addr_t, Type underlying) => func((_, panic, _) => {
    ref Named t = ref _addr_t.val;

    if (underlying == null) {
        panic("types.Named.SetUnderlying: underlying type must not be nil");
    }
    {
        ptr<Named> (_, ok) = underlying._<ptr<Named>>();

        if (ok) {
            panic("types.Named.SetUnderlying: underlying type must not be *Named");
        }
    }

    t.underlying = underlying;

});

// AddMethod adds method m unless it is already in the method list.
private static void AddMethod(this ptr<Named> _addr_t, ptr<Func> _addr_m) {
    ref Named t = ref _addr_t.val;
    ref Func m = ref _addr_m.val;

    {
        var (i, _) = lookupMethod(t.methods, m.pkg, m.name);

        if (i < 0) {
            t.methods = append(t.methods, m);
        }
    }

}

// Note: This is a uint32 rather than a uint64 because the
// respective 64 bit atomic instructions are not available
// on all platforms.
private static uint lastId = default;

// nextId returns a value increasing monotonically by 1 with
// each call, starting with 1. It may be called concurrently.
private static ulong nextId() {
    return uint64(atomic.AddUint32(_addr_lastId, 1));
}

// A _TypeParam represents a type parameter type.
private partial struct _TypeParam {
    public ptr<Checker> check; // for lazy type bound completion
    public ulong id; // unique id
    public ptr<TypeName> obj; // corresponding type name
    public nint index; // parameter index
    public Type bound; // *Named or *Interface; underlying type is always *Interface
}

// newTypeParam returns a new TypeParam.
private static ptr<_TypeParam> newTypeParam(this ptr<Checker> _addr_check, ptr<TypeName> _addr_obj, nint index, Type bound) {
    ref Checker check = ref _addr_check.val;
    ref TypeName obj = ref _addr_obj.val;

    assert(bound != null);
    ptr<_TypeParam> typ = addr(new _TypeParam(check:check,id:nextId(),obj:obj,index:index,bound:bound));
    if (obj.typ == null) {
        obj.typ = typ;
    }
    return _addr_typ!;

}

private static ptr<Interface> Bound(this ptr<_TypeParam> _addr_t) {
    ref _TypeParam t = ref _addr_t.val;

    var iface = asInterface(t.bound); 
    // use the type bound position if we have one
    var pos = token.NoPos;
    {
        ptr<Named> (n, _) = t.bound._<ptr<Named>>();

        if (n != null) {
            pos = n.obj.pos;
        }
    } 
    // TODO(rFindley) switch this to an unexported method on Checker.
    t.check.completeInterface(pos, iface);
    return _addr_iface!;

}

// optype returns a type's operational type. Except for
// type parameters, the operational type is the same
// as the underlying type (as returned by under). For
// Type parameters, the operational type is determined
// by the corresponding type bound's type list. The
// result may be the bottom or top type, but it is never
// the incoming type parameter.
private static Type optype(Type typ) {
    {
        var t = asTypeParam(typ);

        if (t != null) { 
            // If the optype is typ, return the top type as we have
            // no information. It also prevents infinite recursion
            // via the asTypeParam converter function. This can happen
            // for a type parameter list of the form:
            // (type T interface { type T }).
            // See also issue #39680.
            {
                var u = t.Bound().allTypes;

                if (u != null && u != typ) { 
                    // u != typ and u is a type parameter => under(u) != typ, so this is ok
                    return under(u);

                }

            }

            return theTop;

        }
    }

    return under(typ);

}

// An instance represents an instantiated generic type syntactically
// (without expanding the instantiation). Type instances appear only
// during type-checking and are replaced by their fully instantiated
// (expanded) types before the end of type-checking.
private partial struct instance {
    public ptr<Checker> check; // for lazy instantiation
    public token.Pos pos; // position of type instantiation; for error reporting only
    public ptr<Named> @base; // parameterized type to be instantiated
    public slice<Type> targs; // type arguments
    public slice<token.Pos> poslist; // position of each targ; for error reporting only
    public Type value; // base(targs...) after instantiation or Typ[Invalid]; nil if not yet set
}

// expand returns the instantiated (= expanded) type of t.
// The result is either an instantiated *Named type, or
// Typ[Invalid] if there was an error.
private static Type expand(this ptr<instance> _addr_t) {
    ref instance t = ref _addr_t.val;

    var v = t.value;
    if (v == null) {
        v = t.check.instantiate(t.pos, t.@base, t.targs, t.poslist);
        if (v == null) {
            v = Typ[Invalid];
        }
        t.value = v;

    }
    if (debug && v != Typ[Invalid]) {
        _ = v._<ptr<Named>>();
    }
    return v;

}

// expand expands a type instance into its instantiated
// type and leaves all other types alone. expand does
// not recurse.
private static Type expand(Type typ) {
    {
        ptr<instance> (t, _) = typ._<ptr<instance>>();

        if (t != null) {
            return t.expand();
        }
    }

    return typ;

}

// expandf is set to expand.
// Call expandf when calling expand causes compile-time cycle error.
private static Func<Type, Type> expandf = default;

private static void init() {
    expandf = expand;
}

// bottom represents the bottom of the type lattice.
// It is the underlying type of a type parameter that
// cannot be satisfied by any type, usually because
// the intersection of type constraints left nothing).
private partial struct bottom {
}

// theBottom is the singleton bottom type.
private static ptr<bottom> theBottom = addr(new bottom());

// top represents the top of the type lattice.
// It is the underlying type of a type parameter that
// can be satisfied by any type (ignoring methods),
// usually because the type constraint has no type
// list.
private partial struct top {
}

// theTop is the singleton top type.
private static ptr<top> theTop = addr(new top());

// Type-specific implementations of Underlying.
private static Type Underlying(this ptr<Basic> _addr_t) {
    ref Basic t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<Array> _addr_t) {
    ref Array t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<Slice> _addr_t) {
    ref Slice t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<Struct> _addr_t) {
    ref Struct t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<Pointer> _addr_t) {
    ref Pointer t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<Tuple> _addr_t) {
    ref Tuple t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<Signature> _addr_t) {
    ref Signature t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<_Sum> _addr_t) {
    ref _Sum t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<Map> _addr_t) {
    ref Map t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<Chan> _addr_t) {
    ref Chan t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<Named> _addr_t) {
    ref Named t = ref _addr_t.val;

    return t.underlying;
}
private static Type Underlying(this ptr<_TypeParam> _addr_t) {
    ref _TypeParam t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<instance> _addr_t) {
    ref instance t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<bottom> _addr_t) {
    ref bottom t = ref _addr_t.val;

    return t;
}
private static Type Underlying(this ptr<top> _addr_t) {
    ref top t = ref _addr_t.val;

    return t;
}

// Type-specific implementations of String.
private static @string String(this ptr<Basic> _addr_t) {
    ref Basic t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<Array> _addr_t) {
    ref Array t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<Slice> _addr_t) {
    ref Slice t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<Struct> _addr_t) {
    ref Struct t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<Pointer> _addr_t) {
    ref Pointer t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<Tuple> _addr_t) {
    ref Tuple t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<Signature> _addr_t) {
    ref Signature t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<_Sum> _addr_t) {
    ref _Sum t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<Map> _addr_t) {
    ref Map t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<Chan> _addr_t) {
    ref Chan t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<Named> _addr_t) {
    ref Named t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<_TypeParam> _addr_t) {
    ref _TypeParam t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<instance> _addr_t) {
    ref instance t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<bottom> _addr_t) {
    ref bottom t = ref _addr_t.val;

    return TypeString(t, null);
}
private static @string String(this ptr<top> _addr_t) {
    ref top t = ref _addr_t.val;

    return TypeString(t, null);
}

// under returns the true expanded underlying type.
// If it doesn't exist, the result is Typ[Invalid].
// under must only be called when a type is known
// to be fully set up.
private static Type under(Type t) { 
    // TODO(gri) is this correct for *Sum?
    {
        var n = asNamed(t);

        if (n != null) {
            return n.under();
        }
    }

    return t;

}

// Converters
//
// A converter must only be called when a type is
// known to be fully set up. A converter returns
// a type's operational type (see comment for optype)
// or nil if the type argument is not of the
// respective type.

private static ptr<Basic> asBasic(Type t) {
    ptr<Basic> (op, _) = optype(t)._<ptr<Basic>>();
    return _addr_op!;
}

private static ptr<Array> asArray(Type t) {
    ptr<Array> (op, _) = optype(t)._<ptr<Array>>();
    return _addr_op!;
}

private static ptr<Slice> asSlice(Type t) {
    ptr<Slice> (op, _) = optype(t)._<ptr<Slice>>();
    return _addr_op!;
}

private static ptr<Struct> asStruct(Type t) {
    ptr<Struct> (op, _) = optype(t)._<ptr<Struct>>();
    return _addr_op!;
}

private static ptr<Pointer> asPointer(Type t) {
    ptr<Pointer> (op, _) = optype(t)._<ptr<Pointer>>();
    return _addr_op!;
}

private static ptr<Tuple> asTuple(Type t) {
    ptr<Tuple> (op, _) = optype(t)._<ptr<Tuple>>();
    return _addr_op!;
}

private static ptr<Signature> asSignature(Type t) {
    ptr<Signature> (op, _) = optype(t)._<ptr<Signature>>();
    return _addr_op!;
}

private static ptr<_Sum> asSum(Type t) {
    ptr<_Sum> (op, _) = optype(t)._<ptr<_Sum>>();
    return _addr_op!;
}

private static ptr<Interface> asInterface(Type t) {
    ptr<Interface> (op, _) = optype(t)._<ptr<Interface>>();
    return _addr_op!;
}

private static ptr<Map> asMap(Type t) {
    ptr<Map> (op, _) = optype(t)._<ptr<Map>>();
    return _addr_op!;
}

private static ptr<Chan> asChan(Type t) {
    ptr<Chan> (op, _) = optype(t)._<ptr<Chan>>();
    return _addr_op!;
}

// If the argument to asNamed and asTypeParam is of the respective types
// (possibly after expanding an instance type), these methods return that type.
// Otherwise the result is nil.

private static ptr<Named> asNamed(Type t) {
    ptr<Named> (e, _) = expand(t)._<ptr<Named>>();
    return _addr_e!;
}

private static ptr<_TypeParam> asTypeParam(Type t) {
    ptr<_TypeParam> (u, _) = under(t)._<ptr<_TypeParam>>();
    return _addr_u!;
}

} // end types_package
