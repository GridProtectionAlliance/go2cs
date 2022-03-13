// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:59:16 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types\type.go
namespace go.cmd.compile.@internal;

using @base = cmd.compile.@internal.@base_package;
using src = cmd.@internal.src_package;
using fmt = fmt_package;
using sync = sync_package;


// Object represents an ir.Node, but without needing to import cmd/compile/internal/ir,
// which would cause an import cycle. The uses in other packages must type assert
// values of type Object to ir.Node or a more specific type.

using System;
public static partial class types_package {

public partial interface Object {
    ptr<Type> Pos();
    ptr<Type> Sym();
    ptr<Type> Type();
}

// A TypeObject is an Object representing a named type.
public partial interface TypeObject {
    ptr<Type> TypeDefn(); // for "type T Defn", returns Defn
}

// A VarObject is an Object representing a function argument, variable, or struct field.
public partial interface VarObject {
    void RecordFrameOffset(long _p0); // save frame offset
}

//go:generate stringer -type Kind -trimprefix T type.go

// Kind describes a kind of type.
public partial struct Kind { // : byte
}

public static readonly Kind Txxx = iota;

public static readonly var TINT8 = 0;
public static readonly var TUINT8 = 1;
public static readonly var TINT16 = 2;
public static readonly var TUINT16 = 3;
public static readonly var TINT32 = 4;
public static readonly var TUINT32 = 5;
public static readonly var TINT64 = 6;
public static readonly var TUINT64 = 7;
public static readonly var TINT = 8;
public static readonly var TUINT = 9;
public static readonly var TUINTPTR = 10;

public static readonly var TCOMPLEX64 = 11;
public static readonly var TCOMPLEX128 = 12;

public static readonly var TFLOAT32 = 13;
public static readonly var TFLOAT64 = 14;

public static readonly var TBOOL = 15;

public static readonly var TPTR = 16;
public static readonly var TFUNC = 17;
public static readonly var TSLICE = 18;
public static readonly var TARRAY = 19;
public static readonly var TSTRUCT = 20;
public static readonly var TCHAN = 21;
public static readonly var TMAP = 22;
public static readonly var TINTER = 23;
public static readonly var TFORW = 24;
public static readonly var TANY = 25;
public static readonly var TSTRING = 26;
public static readonly var TUNSAFEPTR = 27;
public static readonly var TTYPEPARAM = 28; 

// pseudo-types for literals
public static readonly var TIDEAL = 29; // untyped numeric constants
public static readonly var TNIL = 30;
public static readonly var TBLANK = 31; 

// pseudo-types for frame layout
public static readonly var TFUNCARGS = 32;
public static readonly var TCHANARGS = 33; 

// SSA backend types
public static readonly var TSSA = 34; // internal types used by SSA backend (flags, memory, etc.)
public static readonly var TTUPLE = 35; // a pair of types, used by SSA backend
public static readonly var TRESULTS = 36; // multiple types; the result of calling a function or method, with a memory at the end.

public static readonly var NTYPE = 37;

// ChanDir is whether a channel can send, receive, or both.
public partial struct ChanDir { // : byte
}

public static bool CanRecv(this ChanDir c) {
    return c & Crecv != 0;
}
public static bool CanSend(this ChanDir c) {
    return c & Csend != 0;
}

 
// types of channel
// must match ../../../../reflect/type.go:/ChanDir
public static readonly ChanDir Crecv = 1 << 0;
public static readonly ChanDir Csend = 1 << 1;
public static readonly ChanDir Cboth = Crecv | Csend;

// Types stores pointers to predeclared named types.
//
// It also stores pointers to several special types:
//   - Types[TANY] is the placeholder "any" type recognized by SubstArgTypes.
//   - Types[TBLANK] represents the blank variable's type.
//   - Types[TNIL] represents the predeclared "nil" value's type.
//   - Types[TUNSAFEPTR] is package unsafe's Pointer type.
public static array<ptr<Type>> Types = new array<ptr<Type>>(NTYPE);

 
// Predeclared alias types. Kept separate for better error messages.
public static ptr<Type> ByteType;public static ptr<Type> RuneType;public static ptr<Type> ErrorType;public static var UntypedString = New(TSTRING);public static var UntypedBool = New(TBOOL);public static var UntypedInt = New(TIDEAL);public static var UntypedRune = New(TIDEAL);public static var UntypedFloat = New(TIDEAL);public static var UntypedComplex = New(TIDEAL);

// A Type represents a Go type.
public partial struct Type {
    public long Width; // valid if Align > 0

// list of base methods (excluding embedding)
    public Fields methods; // list of all methods (including embedding)
    public Fields allMethods; // canonical OTYPE node for a named type (should be an ir.Name node with same sym)
    public Object nod; // the underlying type (type literal or predeclared type) for a defined type
    public ptr<Type> underlying; // Cache of composite types, with this type being the element type.
    public ptr<Sym> sym; // symbol containing name, for named types
    public int Vargen; // unique name for OTYPE/ONAME

    public Kind kind; // kind of type
    public byte Align; // the required alignment of this type, in bytes (0 means Width and Align have not yet been computed)

    public bitset8 flags; // For defined (named) generic types, a pointer to the list of type params
// (in order) of this type that need to be instantiated. For
// fully-instantiated generic types, this is the targs used to instantiate
// them (which are used when generating the corresponding instantiated
// methods). rparams is only set for named types that are generic or are
// fully-instantiated from a generic type, and is otherwise set to nil.
    public ptr<slice<ptr<Type>>> rparams;
}

private static void CanBeAnSSAAux(this ptr<Type> _addr__p0) {
    ref Type _p0 = ref _addr__p0.val;

}

private static readonly nint typeNotInHeap = 1 << (int)(iota); // type cannot be heap allocated
private static readonly var typeBroke = 0; // broken type definition
private static readonly var typeNoalg = 1; // suppress hash and eq algorithm generation
private static readonly var typeDeferwidth = 2; // width computation has been deferred and type is on deferredTypeStack
private static readonly var typeRecur = 3;
private static readonly var typeHasTParam = 4; // there is a typeparam somewhere in the type (generic function or type)

private static bool NotInHeap(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.flags & typeNotInHeap != 0;
}
private static bool Broke(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.flags & typeBroke != 0;
}
private static bool Noalg(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.flags & typeNoalg != 0;
}
private static bool Deferwidth(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.flags & typeDeferwidth != 0;
}
private static bool Recur(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.flags & typeRecur != 0;
}
private static bool HasTParam(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.flags & typeHasTParam != 0;
}

private static void SetNotInHeap(this ptr<Type> _addr_t, bool b) {
    ref Type t = ref _addr_t.val;

    t.flags.set(typeNotInHeap, b);
}
private static void SetBroke(this ptr<Type> _addr_t, bool b) {
    ref Type t = ref _addr_t.val;

    t.flags.set(typeBroke, b);
}
private static void SetNoalg(this ptr<Type> _addr_t, bool b) {
    ref Type t = ref _addr_t.val;

    t.flags.set(typeNoalg, b);
}
private static void SetDeferwidth(this ptr<Type> _addr_t, bool b) {
    ref Type t = ref _addr_t.val;

    t.flags.set(typeDeferwidth, b);
}
private static void SetRecur(this ptr<Type> _addr_t, bool b) {
    ref Type t = ref _addr_t.val;

    t.flags.set(typeRecur, b);
}
private static void SetHasTParam(this ptr<Type> _addr_t, bool b) {
    ref Type t = ref _addr_t.val;

    t.flags.set(typeHasTParam, b);
}

// Kind returns the kind of type t.
private static Kind Kind(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind;
}

// Sym returns the name of type t.
private static ptr<Sym> Sym(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return _addr_t.sym!;
}
private static void SetSym(this ptr<Type> _addr_t, ptr<Sym> _addr_sym) {
    ref Type t = ref _addr_t.val;
    ref Sym sym = ref _addr_sym.val;

    t.sym = sym;
}

// Underlying returns the underlying type of type t.
private static ptr<Type> Underlying(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return _addr_t.underlying!;
}

// SetNod associates t with syntax node n.
private static void SetNod(this ptr<Type> _addr_t, Object n) {
    ref Type t = ref _addr_t.val;
 
    // t.nod can be non-nil already
    // in the case of shared *Types, like []byte or interface{}.
    if (t.nod == null) {
        t.nod = n;
    }
}

// Pos returns a position associated with t, if any.
// This should only be used for diagnostics.
private static src.XPos Pos(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t.nod != null) {
        return t.nod.Pos();
    }
    return src.NoXPos;
}

private static slice<ptr<Type>> RParams(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t.rparams == null) {
        return null;
    }
    return t.rparams.val;
}

private static void SetRParams(this ptr<Type> _addr_t, slice<ptr<Type>> rparams) {
    ref Type t = ref _addr_t.val;

    if (len(rparams) == 0) {
        @base.Fatalf("Setting nil or zero-length rparams");
    }
    t.rparams = _addr_rparams;
    if (t.HasTParam()) {
        return ;
    }
    foreach (var (_, rparam) in rparams) {
        if (rparam.HasTParam()) {
            t.SetHasTParam(true);
            break;
        }
    }
}

// NoPkg is a nil *Pkg value for clarity.
// It's intended for use when constructing types that aren't exported
// and thus don't need to be associated with any package.
public static ptr<Pkg> NoPkgnull;

// Pkg returns the package that t appeared in.
//
// Pkg is only defined for function, struct, and interface types
// (i.e., types with named elements). This information isn't used by
// cmd/compile itself, but we need to track it because it's exposed by
// the go/types API.
private static ptr<Pkg> Pkg(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.kind == TFUNC) 
        return t.Extra._<ptr<Func>>().pkg;
    else if (t.kind == TSTRUCT) 
        return t.Extra._<ptr<Struct>>().pkg;
    else if (t.kind == TINTER) 
        return t.Extra._<ptr<Interface>>().pkg;
    else 
        @base.Fatalf("Pkg: unexpected kind: %v", t);
        return _addr_null!;
    }

// Map contains Type fields specific to maps.
public partial struct Map {
    public ptr<Type> Key; // Key type
    public ptr<Type> Elem; // Val (elem) type

    public ptr<Type> Bucket; // internal struct type representing a hash bucket
    public ptr<Type> Hmap; // internal struct type representing the Hmap (map header object)
    public ptr<Type> Hiter; // internal struct type representing hash iterator state
}

// MapType returns t's extra map-specific fields.
private static ptr<Map> MapType(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TMAP);
    return t.Extra._<ptr<Map>>();
}

// Forward contains Type fields specific to forward types.
public partial struct Forward {
    public slice<ptr<Type>> Copyto; // where to copy the eventual value to
    public src.XPos Embedlineno; // first use of this type as an embedded type
}

// ForwardType returns t's extra forward-type-specific fields.
private static ptr<Forward> ForwardType(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TFORW);
    return t.Extra._<ptr<Forward>>();
}

// Func contains Type fields specific to func types.
public partial struct Func {
    public ptr<Type> Receiver; // function receiver
    public ptr<Type> Results; // function results
    public ptr<Type> Params; // function params
    public ptr<Type> TParams; // type params of receiver (if method) or function

    public ptr<Pkg> pkg; // Argwid is the total width of the function receiver, params, and results.
// It gets calculated via a temporary TFUNCARGS type.
// Note that TFUNC's Width is Widthptr.
    public long Argwid;
}

// FuncType returns t's extra func-specific fields.
private static ptr<Func> FuncType(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TFUNC);
    return t.Extra._<ptr<Func>>();
}

// StructType contains Type fields specific to struct types.
public partial struct Struct {
    public Fields fields;
    public ptr<Pkg> pkg; // Maps have three associated internal structs (see struct MapType).
// Map links such structs back to their map type.
    public ptr<Type> Map;
    public Funarg Funarg; // type of function arguments for arg struct
}

// Fnstruct records the kind of function argument
public partial struct Funarg { // : byte
}

public static readonly Funarg FunargNone = iota;
public static readonly var FunargRcvr = 0; // receiver
public static readonly var FunargParams = 1; // input parameters
public static readonly var FunargResults = 2; // output results
public static readonly var FunargTparams = 3; // type params

// StructType returns t's extra struct-specific fields.
private static ptr<Struct> StructType(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TSTRUCT);
    return t.Extra._<ptr<Struct>>();
}

// Interface contains Type fields specific to interface types.
public partial struct Interface {
    public ptr<Pkg> pkg;
}

// Ptr contains Type fields specific to pointer types.
public partial struct Ptr {
    public ptr<Type> Elem; // element type
}

// ChanArgs contains Type fields specific to TCHANARGS types.
public partial struct ChanArgs {
    public ptr<Type> T; // reference to a chan type whose elements need a width check
}

// // FuncArgs contains Type fields specific to TFUNCARGS types.
public partial struct FuncArgs {
    public ptr<Type> T; // reference to a func type whose elements need a width check
}

// Chan contains Type fields specific to channel types.
public partial struct Chan {
    public ptr<Type> Elem; // element type
    public ChanDir Dir; // channel direction
}

// ChanType returns t's extra channel-specific fields.
private static ptr<Chan> ChanType(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TCHAN);
    return t.Extra._<ptr<Chan>>();
}

public partial struct Tuple {
    public ptr<Type> first;
    public ptr<Type> second; // Any tuple with a memory type must put that memory type second.
}

// Results are the output from calls that will be late-expanded.
public partial struct Results {
    public slice<ptr<Type>> Types; // Last element is memory output from call.
}

// Array contains Type fields specific to array types.
public partial struct Array {
    public ptr<Type> Elem; // element type
    public long Bound; // number of elements; <0 if unknown yet
}

// Slice contains Type fields specific to slice types.
public partial struct Slice {
    public ptr<Type> Elem; // element type
}

// A Field is a (Sym, Type) pairing along with some other information, and,
// depending on the context, is used to represent:
//  - a field in a struct
//  - a method in an interface or associated with a named type
//  - a function parameter
public partial struct Field {
    public bitset8 flags;
    public byte Embedded; // embedded field

    public src.XPos Pos;
    public ptr<Sym> Sym;
    public ptr<Type> Type; // field type
    public @string Note; // literal string annotation

// For fields that represent function parameters, Nname points
// to the associated ONAME Node.
    public Object Nname; // Offset in bytes of this field or method within its enclosing struct
// or interface Type.  Exception: if field is function receiver, arg or
// result, then this is BOGUS_FUNARG_OFFSET; types does not know the Abi.
    public long Offset;
}

private static readonly nint fieldIsDDD = 1 << (int)(iota); // field is ... argument
private static readonly var fieldBroke = 0; // broken field definition
private static readonly var fieldNointerface = 1;

private static bool IsDDD(this ptr<Field> _addr_f) {
    ref Field f = ref _addr_f.val;

    return f.flags & fieldIsDDD != 0;
}
private static bool Broke(this ptr<Field> _addr_f) {
    ref Field f = ref _addr_f.val;

    return f.flags & fieldBroke != 0;
}
private static bool Nointerface(this ptr<Field> _addr_f) {
    ref Field f = ref _addr_f.val;

    return f.flags & fieldNointerface != 0;
}

private static void SetIsDDD(this ptr<Field> _addr_f, bool b) {
    ref Field f = ref _addr_f.val;

    f.flags.set(fieldIsDDD, b);
}
private static void SetBroke(this ptr<Field> _addr_f, bool b) {
    ref Field f = ref _addr_f.val;

    f.flags.set(fieldBroke, b);
}
private static void SetNointerface(this ptr<Field> _addr_f, bool b) {
    ref Field f = ref _addr_f.val;

    f.flags.set(fieldNointerface, b);
}

// End returns the offset of the first byte immediately after this field.
private static long End(this ptr<Field> _addr_f) {
    ref Field f = ref _addr_f.val;

    return f.Offset + f.Type.Width;
}

// IsMethod reports whether f represents a method rather than a struct field.
private static bool IsMethod(this ptr<Field> _addr_f) {
    ref Field f = ref _addr_f.val;

    return f.Type.kind == TFUNC && f.Type.Recv() != null;
}

// Fields is a pointer to a slice of *Field.
// This saves space in Types that do not have fields or methods
// compared to a simple slice of *Field.
public partial struct Fields {
    public ptr<slice<ptr<Field>>> s;
}

// Len returns the number of entries in f.
private static nint Len(this ptr<Fields> _addr_f) {
    ref Fields f = ref _addr_f.val;

    if (f.s == null) {
        return 0;
    }
    return len(f.s.val);
}

// Slice returns the entries in f as a slice.
// Changes to the slice entries will be reflected in f.
private static slice<ptr<Field>> Slice(this ptr<Fields> _addr_f) {
    ref Fields f = ref _addr_f.val;

    if (f.s == null) {
        return null;
    }
    return f.s.val;
}

// Index returns the i'th element of Fields.
// It panics if f does not have at least i+1 elements.
private static ptr<Field> Index(this ptr<Fields> _addr_f, nint i) {
    ref Fields f = ref _addr_f.val;

    return _addr_(f.s.val)[i]!;
}

// Set sets f to a slice.
// This takes ownership of the slice.
private static void Set(this ptr<Fields> _addr_f, slice<ptr<Field>> s) {
    ref Fields f = ref _addr_f.val;

    if (len(s) == 0) {
        f.s = null;
    }
    else
 { 
        // Copy s and take address of t rather than s to avoid
        // allocation in the case where len(s) == 0.
        ref var t = ref heap(s, out ptr<var> _addr_t);
        _addr_f.s = _addr_t;
        f.s = ref _addr_f.s.val;
    }
}

// Append appends entries to f.
private static void Append(this ptr<Fields> _addr_f, params ptr<ptr<Field>>[] _addr_s) {
    s = s.Clone();
    ref Fields f = ref _addr_f.val;
    ref Field s = ref _addr_s.val;

    if (f.s == null) {
        f.s = @new<*Field>();
    }
    f.s.val = append(f.s.val, s);
}

// New returns a new Type of the specified kind.
public static ptr<Type> New(Kind et) {
    ptr<Type> t = addr(new Type(kind:et,Width:BADWIDTH,));
    t.underlying = t; 
    // TODO(josharian): lazily initialize some of these?

    if (t.kind == TMAP) 
        t.Extra = @new<Map>();
    else if (t.kind == TFORW) 
        t.Extra = @new<Forward>();
    else if (t.kind == TFUNC) 
        t.Extra = @new<Func>();
    else if (t.kind == TSTRUCT) 
        t.Extra = @new<Struct>();
    else if (t.kind == TINTER) 
        t.Extra = @new<Interface>();
    else if (t.kind == TPTR) 
        t.Extra = new Ptr();
    else if (t.kind == TCHANARGS) 
        t.Extra = new ChanArgs();
    else if (t.kind == TFUNCARGS) 
        t.Extra = new FuncArgs();
    else if (t.kind == TCHAN) 
        t.Extra = @new<Chan>();
    else if (t.kind == TTUPLE) 
        t.Extra = @new<Tuple>();
    else if (t.kind == TRESULTS) 
        t.Extra = @new<Results>();
    else if (t.kind == TTYPEPARAM) 
        t.Extra = @new<Interface>();
        return _addr_t!;
}

// NewArray returns a new fixed-length array Type.
public static ptr<Type> NewArray(ptr<Type> _addr_elem, long bound) {
    ref Type elem = ref _addr_elem.val;

    if (bound < 0) {
        @base.Fatalf("NewArray: invalid bound %v", bound);
    }
    var t = New(TARRAY);
    t.Extra = addr(new Array(Elem:elem,Bound:bound));
    t.SetNotInHeap(elem.NotInHeap());
    if (elem.HasTParam()) {
        t.SetHasTParam(true);
    }
    return _addr_t!;
}

// NewSlice returns the slice Type with element type elem.
public static ptr<Type> NewSlice(ptr<Type> _addr_elem) {
    ref Type elem = ref _addr_elem.val;

    {
        var t__prev1 = t;

        var t = elem.cache.slice;

        if (t != null) {
            if (t.Elem() != elem) {
                @base.Fatalf("elem mismatch");
            }
            return _addr_t!;
        }
        t = t__prev1;

    }

    t = New(TSLICE);
    t.Extra = new Slice(Elem:elem);
    elem.cache.slice = t;
    if (elem.HasTParam()) {
        t.SetHasTParam(true);
    }
    return _addr_t!;
}

// NewChan returns a new chan Type with direction dir.
public static ptr<Type> NewChan(ptr<Type> _addr_elem, ChanDir dir) {
    ref Type elem = ref _addr_elem.val;

    var t = New(TCHAN);
    var ct = t.ChanType();
    ct.Elem = elem;
    ct.Dir = dir;
    if (elem.HasTParam()) {
        t.SetHasTParam(true);
    }
    return _addr_t!;
}

public static ptr<Type> NewTuple(ptr<Type> _addr_t1, ptr<Type> _addr_t2) {
    ref Type t1 = ref _addr_t1.val;
    ref Type t2 = ref _addr_t2.val;

    var t = New(TTUPLE);
    t.Extra._<ptr<Tuple>>().first = t1;
    t.Extra._<ptr<Tuple>>().second = t2;
    if (t1.HasTParam() || t2.HasTParam()) {
        t.SetHasTParam(true);
    }
    return _addr_t!;
}

private static ptr<Type> newResults(slice<ptr<Type>> types) {
    var t = New(TRESULTS);
    t.Extra._<ptr<Results>>().Types = types;
    return _addr_t!;
}

public static ptr<Type> NewResults(slice<ptr<Type>> types) {
    if (len(types) == 1 && types[0] == TypeMem) {
        return _addr_TypeResultMem!;
    }
    return _addr_newResults(types)!;
}

private static ptr<Type> newSSA(@string name) {
    var t = New(TSSA);
    t.Extra = name;
    return _addr_t!;
}

// NewMap returns a new map Type with key type k and element (aka value) type v.
public static ptr<Type> NewMap(ptr<Type> _addr_k, ptr<Type> _addr_v) {
    ref Type k = ref _addr_k.val;
    ref Type v = ref _addr_v.val;

    var t = New(TMAP);
    var mt = t.MapType();
    mt.Key = k;
    mt.Elem = v;
    if (k.HasTParam() || v.HasTParam()) {
        t.SetHasTParam(true);
    }
    return _addr_t!;
}

// NewPtrCacheEnabled controls whether *T Types are cached in T.
// Caching is disabled just before starting the backend.
// This allows the backend to run concurrently.
public static var NewPtrCacheEnabled = true;

// NewPtr returns the pointer type pointing to t.
public static ptr<Type> NewPtr(ptr<Type> _addr_elem) {
    ref Type elem = ref _addr_elem.val;

    if (elem == null) {
        @base.Fatalf("NewPtr: pointer to elem Type is nil");
    }
    {
        var t__prev1 = t;

        var t = elem.cache.ptr;

        if (t != null) {
            if (t.Elem() != elem) {
                @base.Fatalf("NewPtr: elem mismatch");
            }
            if (elem.HasTParam()) { 
                // Extra check when reusing the cache, since the elem
                // might have still been undetermined (i.e. a TFORW type)
                // when this entry was cached.
                t.SetHasTParam(true);
            }
            return _addr_t!;
        }
        t = t__prev1;

    }

    t = New(TPTR);
    t.Extra = new Ptr(Elem:elem);
    t.Width = int64(PtrSize);
    t.Align = uint8(PtrSize);
    if (NewPtrCacheEnabled) {
        elem.cache.ptr = t;
    }
    if (elem.HasTParam()) {
        t.SetHasTParam(true);
    }
    return _addr_t!;
}

// NewChanArgs returns a new TCHANARGS type for channel type c.
public static ptr<Type> NewChanArgs(ptr<Type> _addr_c) {
    ref Type c = ref _addr_c.val;

    var t = New(TCHANARGS);
    t.Extra = new ChanArgs(T:c);
    return _addr_t!;
}

// NewFuncArgs returns a new TFUNCARGS type for func type f.
public static ptr<Type> NewFuncArgs(ptr<Type> _addr_f) {
    ref Type f = ref _addr_f.val;

    var t = New(TFUNCARGS);
    t.Extra = new FuncArgs(T:f);
    return _addr_t!;
}

public static ptr<Field> NewField(src.XPos pos, ptr<Sym> _addr_sym, ptr<Type> _addr_typ) {
    ref Sym sym = ref _addr_sym.val;
    ref Type typ = ref _addr_typ.val;

    ptr<Field> f = addr(new Field(Pos:pos,Sym:sym,Type:typ,Offset:BADWIDTH,));
    if (typ == null) {
        f.SetBroke(true);
    }
    return _addr_f!;
}

// SubstAny walks t, replacing instances of "any" with successive
// elements removed from types.  It returns the substituted type.
public static ptr<Type> SubstAny(ptr<Type> _addr_t, ptr<slice<ptr<Type>>> _addr_types) {
    ref Type t = ref _addr_t.val;
    ref slice<ptr<Type>> types = ref _addr_types.val;

    if (t == null) {
        return _addr_null!;
    }

    if (t.kind == TANY) 
        if (len(types.val) == 0) {
            @base.Fatalf("SubstArgTypes: not enough argument types");
        }
        t = (types.val)[0];
        types.val = (types.val)[(int)1..];
    else if (t.kind == TPTR) 
        var elem = SubstAny(_addr_t.Elem(), _addr_types);
        if (elem != t.Elem()) {
            t = t.copy();
            t.Extra = new Ptr(Elem:elem);
        }
    else if (t.kind == TARRAY) 
        elem = SubstAny(_addr_t.Elem(), _addr_types);
        if (elem != t.Elem()) {
            t = t.copy();
            t.Extra._<ptr<Array>>().Elem = elem;
        }
    else if (t.kind == TSLICE) 
        elem = SubstAny(_addr_t.Elem(), _addr_types);
        if (elem != t.Elem()) {
            t = t.copy();
            t.Extra = new Slice(Elem:elem);
        }
    else if (t.kind == TCHAN) 
        elem = SubstAny(_addr_t.Elem(), _addr_types);
        if (elem != t.Elem()) {
            t = t.copy();
            t.Extra._<ptr<Chan>>().Elem = elem;
        }
    else if (t.kind == TMAP) 
        var key = SubstAny(_addr_t.Key(), _addr_types);
        elem = SubstAny(_addr_t.Elem(), _addr_types);
        if (key != t.Key() || elem != t.Elem()) {
            t = t.copy();
            t.Extra._<ptr<Map>>().Key = key;
            t.Extra._<ptr<Map>>().Elem = elem;
        }
    else if (t.kind == TFUNC) 
        var recvs = SubstAny(_addr_t.Recvs(), _addr_types);
        var @params = SubstAny(_addr_t.Params(), _addr_types);
        var results = SubstAny(_addr_t.Results(), _addr_types);
        if (recvs != t.Recvs() || params != t.Params() || results != t.Results()) {
            t = t.copy();
            t.FuncType().Receiver = recvs;
            t.FuncType().Results = results;
            t.FuncType().Params = params;
        }
    else if (t.kind == TSTRUCT) 
        // Make a copy of all fields, including ones whose type does not change.
        // This prevents aliasing across functions, which can lead to later
        // fields getting their Offset incorrectly overwritten.
        var fields = t.FieldSlice();
        var nfs = make_slice<ptr<Field>>(len(fields));
        foreach (var (i, f) in fields) {
            var nft = SubstAny(_addr_f.Type, _addr_types);
            nfs[i] = f.Copy();
            nfs[i].Type = nft;
        }        t = t.copy();
        t.SetFields(nfs);
    else         return _addr_t!;
}

// copy returns a shallow copy of the Type.
private static ptr<Type> copy(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t == null) {
        return _addr_null!;
    }
    ref var nt = ref heap(t.val, out ptr<var> _addr_nt); 
    // copy any *T Extra fields, to avoid aliasing

    if (t.kind == TMAP) 
        ptr<Map> x = t.Extra._<ptr<Map>>().val;
        nt.Extra = _addr_x;
    else if (t.kind == TFORW) 
        x = t.Extra._<ptr<Forward>>().val;
        nt.Extra = _addr_x;
    else if (t.kind == TFUNC) 
        x = t.Extra._<ptr<Func>>().val;
        nt.Extra = _addr_x;
    else if (t.kind == TSTRUCT) 
        x = t.Extra._<ptr<Struct>>().val;
        nt.Extra = _addr_x;
    else if (t.kind == TINTER) 
        x = t.Extra._<ptr<Interface>>().val;
        nt.Extra = _addr_x;
    else if (t.kind == TCHAN) 
        x = t.Extra._<ptr<Chan>>().val;
        nt.Extra = _addr_x;
    else if (t.kind == TARRAY) 
        x = t.Extra._<ptr<Array>>().val;
        nt.Extra = _addr_x;
    else if (t.kind == TTUPLE || t.kind == TSSA || t.kind == TRESULTS) 
        @base.Fatalf("ssa types cannot be copied");
    // TODO(mdempsky): Find out why this is necessary and explain.
    if (t.underlying == t) {
        _addr_nt.underlying = _addr_nt;
        nt.underlying = ref _addr_nt.underlying.val;
    }
    return _addr__addr_nt!;
}

private static ptr<Field> Copy(this ptr<Field> _addr_f) {
    ref Field f = ref _addr_f.val;

    ref var nf = ref heap(f.val, out ptr<var> _addr_nf);
    return _addr__addr_nf!;
}

private static void wantEtype(this ptr<Type> _addr_t, Kind et) {
    ref Type t = ref _addr_t.val;

    if (t.kind != et) {
        @base.Fatalf("want %v, but have %v", et, t);
    }
}

private static ptr<Type> Recvs(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return _addr_t.FuncType().Receiver!;
}
private static ptr<Type> TParams(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return _addr_t.FuncType().TParams!;
}
private static ptr<Type> Params(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return _addr_t.FuncType().Params!;
}
private static ptr<Type> Results(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return _addr_t.FuncType().Results!;
}

private static nint NumRecvs(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.FuncType().Receiver.NumFields();
}
private static nint NumTParams(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.FuncType().TParams.NumFields();
}
private static nint NumParams(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.FuncType().Params.NumFields();
}
private static nint NumResults(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.FuncType().Results.NumFields();
}

// IsVariadic reports whether function type t is variadic.
private static bool IsVariadic(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    var n = t.NumParams();
    return n > 0 && t.Params().Field(n - 1).IsDDD();
}

// Recv returns the receiver of function type t, if any.
private static ptr<Field> Recv(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    var s = t.Recvs();
    if (s.NumFields() == 0) {
        return _addr_null!;
    }
    return _addr_s.Field(0)!;
}

// RecvsParamsResults stores the accessor functions for a function Type's
// receiver, parameters, and result parameters, in that order.
// It can be used to iterate over all of a function's parameter lists.
public static array<Func<ptr<Type>, ptr<Type>>> RecvsParamsResults = new array<Func<ptr<Type>, ptr<Type>>>(new Func<ptr<Type>, ptr<Type>>[] { (*Type).Recvs, (*Type).Params, (*Type).Results });

// RecvsParams is like RecvsParamsResults, but omits result parameters.
public static array<Func<ptr<Type>, ptr<Type>>> RecvsParams = new array<Func<ptr<Type>, ptr<Type>>>(new Func<ptr<Type>, ptr<Type>>[] { (*Type).Recvs, (*Type).Params });

// ParamsResults is like RecvsParamsResults, but omits receiver parameters.
public static array<Func<ptr<Type>, ptr<Type>>> ParamsResults = new array<Func<ptr<Type>, ptr<Type>>>(new Func<ptr<Type>, ptr<Type>>[] { (*Type).Params, (*Type).Results });

// Key returns the key type of map type t.
private static ptr<Type> Key(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TMAP);
    return t.Extra._<ptr<Map>>().Key;
}

// Elem returns the type of elements of t.
// Usable with pointers, channels, arrays, slices, and maps.
private static ptr<Type> Elem(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.kind == TPTR) 
        return _addr_t.Extra._<Ptr>().Elem!;
    else if (t.kind == TARRAY) 
        return t.Extra._<ptr<Array>>().Elem;
    else if (t.kind == TSLICE) 
        return _addr_t.Extra._<Slice>().Elem!;
    else if (t.kind == TCHAN) 
        return t.Extra._<ptr<Chan>>().Elem;
    else if (t.kind == TMAP) 
        return t.Extra._<ptr<Map>>().Elem;
        @base.Fatalf("Type.Elem %s", t.kind);
    return _addr_null!;
}

// ChanArgs returns the channel type for TCHANARGS type t.
private static ptr<Type> ChanArgs(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TCHANARGS);
    return _addr_t.Extra._<ChanArgs>().T!;
}

// FuncArgs returns the func type for TFUNCARGS type t.
private static ptr<Type> FuncArgs(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TFUNCARGS);
    return _addr_t.Extra._<FuncArgs>().T!;
}

// IsFuncArgStruct reports whether t is a struct representing function parameters.
private static bool IsFuncArgStruct(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TSTRUCT && t.Extra._<ptr<Struct>>().Funarg != FunargNone;
}

// Methods returns a pointer to the base methods (excluding embedding) for type t.
// These can either be concrete methods (for non-interface types) or interface
// methods (for interface types).
private static ptr<Fields> Methods(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return _addr__addr_t.methods!;
}

// AllMethods returns a pointer to all the methods (including embedding) for type t.
// For an interface type, this is the set of methods that are typically iterated over.
private static ptr<Fields> AllMethods(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t.kind == TINTER) { 
        // Calculate the full method set of an interface type on the fly
        // now, if not done yet.
        CalcSize(t);
    }
    return _addr__addr_t.allMethods!;
}

// SetAllMethods sets the set of all methods (including embedding) for type t.
// Use this method instead of t.AllMethods().Set(), which might call CalcSize() on
// an uninitialized interface type.
private static void SetAllMethods(this ptr<Type> _addr_t, slice<ptr<Field>> fs) {
    ref Type t = ref _addr_t.val;

    t.allMethods.Set(fs);
}

// Fields returns the fields of struct type t.
private static ptr<Fields> Fields(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TSTRUCT);
    return _addr_t.Extra._<ptr<Struct>>().fields;
}

// Field returns the i'th field of struct type t.
private static ptr<Field> Field(this ptr<Type> _addr_t, nint i) {
    ref Type t = ref _addr_t.val;

    return _addr_t.Fields().Slice()[i]!;
}

// FieldSlice returns a slice of containing all fields of
// a struct type t.
private static slice<ptr<Field>> FieldSlice(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.Fields().Slice();
}

// SetFields sets struct type t's fields to fields.
private static void SetFields(this ptr<Type> _addr_t, slice<ptr<Field>> fields) {
    ref Type t = ref _addr_t.val;
 
    // If we've calculated the width of t before,
    // then some other type such as a function signature
    // might now have the wrong type.
    // Rather than try to track and invalidate those,
    // enforce that SetFields cannot be called once
    // t's width has been calculated.
    if (t.WidthCalculated()) {
        @base.Fatalf("SetFields of %v: width previously calculated", t);
    }
    t.wantEtype(TSTRUCT);
    foreach (var (_, f) in fields) { 
        // If type T contains a field F with a go:notinheap
        // type, then T must also be go:notinheap. Otherwise,
        // you could heap allocate T and then get a pointer F,
        // which would be a heap pointer to a go:notinheap
        // type.
        if (f.Type != null && f.Type.NotInHeap()) {
            t.SetNotInHeap(true);
            break;
        }
    }    t.Fields().Set(fields);
}

// SetInterface sets the base methods of an interface type t.
private static void SetInterface(this ptr<Type> _addr_t, slice<ptr<Field>> methods) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TINTER);
    t.Methods().Set(methods);
}

private static bool WidthCalculated(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.Align > 0;
}

// ArgWidth returns the total aligned argument size for a function.
// It includes the receiver, parameters, and results.
private static long ArgWidth(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TFUNC);
    return t.Extra._<ptr<Func>>().Argwid;
}

private static long Size(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t.kind == TSSA) {
        if (t == TypeInt128) {
            return 16;
        }
        return 0;
    }
    CalcSize(t);
    return t.Width;
}

private static long Alignment(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    CalcSize(t);
    return int64(t.Align);
}

private static @string SimpleString(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind.String();
}

// Cmp is a comparison between values a and b.
// -1 if a < b
//  0 if a == b
//  1 if a > b
public partial struct Cmp { // : sbyte
}

public static readonly var CMPlt = Cmp(-1);
public static readonly var CMPeq = Cmp(0);
public static readonly var CMPgt = Cmp(1);

// Compare compares types for purposes of the SSA back
// end, returning a Cmp (one of CMPlt, CMPeq, CMPgt).
// The answers are correct for an optimizer
// or code generator, but not necessarily typechecking.
// The order chosen is arbitrary, only consistency and division
// into equivalence classes (Types that compare CMPeq) matters.
private static Cmp Compare(this ptr<Type> _addr_t, ptr<Type> _addr_x) {
    ref Type t = ref _addr_t.val;
    ref Type x = ref _addr_x.val;

    if (x == t) {
        return CMPeq;
    }
    return t.cmp(x);
}

private static Cmp cmpForNe(bool x) {
    if (x) {
        return CMPlt;
    }
    return CMPgt;
}

private static Cmp cmpsym(this ptr<Sym> _addr_r, ptr<Sym> _addr_s) {
    ref Sym r = ref _addr_r.val;
    ref Sym s = ref _addr_s.val;

    if (r == s) {
        return CMPeq;
    }
    if (r == null) {
        return CMPlt;
    }
    if (s == null) {
        return CMPgt;
    }
    if (len(r.Name) != len(s.Name)) {
        return cmpForNe(len(r.Name) < len(s.Name));
    }
    if (r.Pkg != s.Pkg) {
        if (len(r.Pkg.Prefix) != len(s.Pkg.Prefix)) {
            return cmpForNe(len(r.Pkg.Prefix) < len(s.Pkg.Prefix));
        }
        if (r.Pkg.Prefix != s.Pkg.Prefix) {
            return cmpForNe(r.Pkg.Prefix < s.Pkg.Prefix);
        }
    }
    if (r.Name != s.Name) {
        return cmpForNe(r.Name < s.Name);
    }
    return CMPeq;
}

// cmp compares two *Types t and x, returning CMPlt,
// CMPeq, CMPgt as t<x, t==x, t>x, for an arbitrary
// and optimizer-centric notion of comparison.
// TODO(josharian): make this safe for recursive interface types
// and use in signatlist sorting. See issue 19869.
private static Cmp cmp(this ptr<Type> _addr_t, ptr<Type> _addr_x) => func((_, panic, _) => {
    ref Type t = ref _addr_t.val;
    ref Type x = ref _addr_x.val;
 
    // This follows the structure of function identical in identity.go
    // with two exceptions.
    // 1. Symbols are compared more carefully because a <,=,> result is desired.
    // 2. Maps are treated specially to avoid endless recursion -- maps
    //    contain an internal data type not expressible in Go source code.
    if (t == x) {
        return CMPeq;
    }
    if (t == null) {
        return CMPlt;
    }
    if (x == null) {
        return CMPgt;
    }
    if (t.kind != x.kind) {
        return cmpForNe(t.kind < x.kind);
    }
    if (t.sym != null || x.sym != null) { 
        // Special case: we keep byte and uint8 separate
        // for error messages. Treat them as equal.

        if (t.kind == TUINT8) 
            if ((t == Types[TUINT8] || t == ByteType) && (x == Types[TUINT8] || x == ByteType)) {
                return CMPeq;
            }
        else if (t.kind == TINT32) 
            if ((t == Types[RuneType.kind] || t == RuneType) && (x == Types[RuneType.kind] || x == RuneType)) {
                return CMPeq;
            }
            }
    {
        var c__prev1 = c;

        var c = t.sym.cmpsym(x.sym);

        if (c != CMPeq) {
            return c;
        }
        c = c__prev1;

    }

    if (x.sym != null) { 
        // Syms non-nil, if vargens match then equal.
        if (t.Vargen != x.Vargen) {
            return cmpForNe(t.Vargen < x.Vargen);
        }
        return CMPeq;
    }

    if (t.kind == TBOOL || t.kind == TFLOAT32 || t.kind == TFLOAT64 || t.kind == TCOMPLEX64 || t.kind == TCOMPLEX128 || t.kind == TUNSAFEPTR || t.kind == TUINTPTR || t.kind == TINT8 || t.kind == TINT16 || t.kind == TINT32 || t.kind == TINT64 || t.kind == TINT || t.kind == TUINT8 || t.kind == TUINT16 || t.kind == TUINT32 || t.kind == TUINT64 || t.kind == TUINT) 
        return CMPeq;
    else if (t.kind == TSSA) 
        @string tname = t.Extra._<@string>();
        @string xname = x.Extra._<@string>(); 
        // desire fast sorting, not pretty sorting.
        if (len(tname) == len(xname)) {
            if (tname == xname) {
                return CMPeq;
            }
            if (tname < xname) {
                return CMPlt;
            }
            return CMPgt;
        }
        if (len(tname) > len(xname)) {
            return CMPgt;
        }
        return CMPlt;
    else if (t.kind == TTUPLE) 
        ptr<Tuple> xtup = x.Extra._<ptr<Tuple>>();
        ptr<Tuple> ttup = t.Extra._<ptr<Tuple>>();
        {
            var c__prev1 = c;

            c = ttup.first.Compare(xtup.first);

            if (c != CMPeq) {
                return c;
            }

            c = c__prev1;

        }
        return ttup.second.Compare(xtup.second);
    else if (t.kind == TRESULTS) 
        ptr<Results> xResults = x.Extra._<ptr<Results>>();
        ptr<Results> tResults = t.Extra._<ptr<Results>>();
        var xl = len(xResults.Types);
        var tl = len(tResults.Types);
        if (tl != xl) {
            if (tl < xl) {
                return CMPlt;
            }
            return CMPgt;
        }
        {
            nint i__prev1 = i;

            for (nint i = 0; i < tl; i++) {
                {
                    var c__prev1 = c;

                    c = tResults.Types[i].Compare(xResults.Types[i]);

                    if (c != CMPeq) {
                        return c;
                    }

                    c = c__prev1;

                }
            }


            i = i__prev1;
        }
        return CMPeq;
    else if (t.kind == TMAP) 
        {
            var c__prev1 = c;

            c = t.Key().cmp(x.Key());

            if (c != CMPeq) {
                return c;
            }

            c = c__prev1;

        }
        return t.Elem().cmp(x.Elem());
    else if (t.kind == TPTR || t.kind == TSLICE)     else if (t.kind == TSTRUCT) 
        if (t.StructType().Map == null) {
            if (x.StructType().Map != null) {
                return CMPlt; // nil < non-nil
            } 
            // to the fallthrough
        }
        else if (x.StructType().Map == null) {
            return CMPgt; // nil > non-nil
        }
        else if (t.StructType().Map.MapType().Bucket == t) { 
            // Both have non-nil Map
            // Special case for Maps which include a recursive type where the recursion is not broken with a named type
            if (x.StructType().Map.MapType().Bucket != x) {
                return CMPlt; // bucket maps are least
            }
            return t.StructType().Map.cmp(x.StructType().Map);
        }
        else if (x.StructType().Map.MapType().Bucket == x) {
            return CMPgt; // bucket maps are least
        }
        var tfs = t.FieldSlice();
        var xfs = x.FieldSlice();
        {
            nint i__prev1 = i;

            for (i = 0; i < len(tfs) && i < len(xfs); i++) {
                var t1 = tfs[i];
                var x1 = xfs[i];
                if (t1.Embedded != x1.Embedded) {
                    return cmpForNe(t1.Embedded < x1.Embedded);
                }
                if (t1.Note != x1.Note) {
                    return cmpForNe(t1.Note < x1.Note);
                }
                {
                    var c__prev1 = c;

                    c = t1.Sym.cmpsym(x1.Sym);

                    if (c != CMPeq) {
                        return c;
                    }

                    c = c__prev1;

                }
                {
                    var c__prev1 = c;

                    c = t1.Type.cmp(x1.Type);

                    if (c != CMPeq) {
                        return c;
                    }

                    c = c__prev1;

                }
            }


            i = i__prev1;
        }
        if (len(tfs) != len(xfs)) {
            return cmpForNe(len(tfs) < len(xfs));
        }
        return CMPeq;
    else if (t.kind == TINTER) 
        tfs = t.AllMethods().Slice();
        xfs = x.AllMethods().Slice();
        {
            nint i__prev1 = i;

            for (i = 0; i < len(tfs) && i < len(xfs); i++) {
                t1 = tfs[i];
                x1 = xfs[i];
                {
                    var c__prev1 = c;

                    c = t1.Sym.cmpsym(x1.Sym);

                    if (c != CMPeq) {
                        return c;
                    }

                    c = c__prev1;

                }
                {
                    var c__prev1 = c;

                    c = t1.Type.cmp(x1.Type);

                    if (c != CMPeq) {
                        return c;
                    }

                    c = c__prev1;

                }
            }


            i = i__prev1;
        }
        if (len(tfs) != len(xfs)) {
            return cmpForNe(len(tfs) < len(xfs));
        }
        return CMPeq;
    else if (t.kind == TFUNC) 
        foreach (var (_, f) in RecvsParamsResults) { 
            // Loop over fields in structs, ignoring argument names.
            tfs = f(t).FieldSlice();
            xfs = f(x).FieldSlice();
            {
                nint i__prev2 = i;

                for (i = 0; i < len(tfs) && i < len(xfs); i++) {
                    var ta = tfs[i];
                    var tb = xfs[i];
                    if (ta.IsDDD() != tb.IsDDD()) {
                        return cmpForNe(!ta.IsDDD());
                    }
                    {
                        var c__prev1 = c;

                        c = ta.Type.cmp(tb.Type);

                        if (c != CMPeq) {
                            return c;
                        }

                        c = c__prev1;

                    }
                }


                i = i__prev2;
            }
            if (len(tfs) != len(xfs)) {
                return cmpForNe(len(tfs) < len(xfs));
            }
        }        return CMPeq;
    else if (t.kind == TARRAY) 
        if (t.NumElem() != x.NumElem()) {
            return cmpForNe(t.NumElem() < x.NumElem());
        }
    else if (t.kind == TCHAN) 
        if (t.ChanDir() != x.ChanDir()) {
            return cmpForNe(t.ChanDir() < x.ChanDir());
        }
    else 
        var e = fmt.Sprintf("Do not know how to compare %v with %v", t, x);
        panic(e);
    // Common element type comparison for TARRAY, TCHAN, TPTR, and TSLICE.
    return t.Elem().cmp(x.Elem());
});

// IsKind reports whether t is a Type of the specified kind.
private static bool IsKind(this ptr<Type> _addr_t, Kind et) {
    ref Type t = ref _addr_t.val;

    return t != null && t.kind == et;
}

private static bool IsBoolean(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TBOOL;
}

private static array<Kind> unsignedEType = new array<Kind>(InitKeyedValues<Kind>((TINT8, TUINT8), (TUINT8, TUINT8), (TINT16, TUINT16), (TUINT16, TUINT16), (TINT32, TUINT32), (TUINT32, TUINT32), (TINT64, TUINT64), (TUINT64, TUINT64), (TINT, TUINT), (TUINT, TUINT), (TUINTPTR, TUINTPTR)));

// ToUnsigned returns the unsigned equivalent of integer type t.
private static ptr<Type> ToUnsigned(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (!t.IsInteger()) {
        @base.Fatalf("unsignedType(%v)", t);
    }
    return _addr_Types[unsignedEType[t.kind]]!;
}

private static bool IsInteger(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.kind == TINT8 || t.kind == TUINT8 || t.kind == TINT16 || t.kind == TUINT16 || t.kind == TINT32 || t.kind == TUINT32 || t.kind == TINT64 || t.kind == TUINT64 || t.kind == TINT || t.kind == TUINT || t.kind == TUINTPTR) 
        return true;
        return t == UntypedInt || t == UntypedRune;
}

private static bool IsSigned(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.kind == TINT8 || t.kind == TINT16 || t.kind == TINT32 || t.kind == TINT64 || t.kind == TINT) 
        return true;
        return false;
}

private static bool IsUnsigned(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.kind == TUINT8 || t.kind == TUINT16 || t.kind == TUINT32 || t.kind == TUINT64 || t.kind == TUINT || t.kind == TUINTPTR) 
        return true;
        return false;
}

private static bool IsFloat(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TFLOAT32 || t.kind == TFLOAT64 || t == UntypedFloat;
}

private static bool IsComplex(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TCOMPLEX64 || t.kind == TCOMPLEX128 || t == UntypedComplex;
}

// IsPtr reports whether t is a regular Go pointer type.
// This does not include unsafe.Pointer.
private static bool IsPtr(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TPTR;
}

// IsPtrElem reports whether t is the element of a pointer (to t).
private static bool IsPtrElem(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.cache.ptr != null;
}

// IsUnsafePtr reports whether t is an unsafe pointer.
private static bool IsUnsafePtr(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TUNSAFEPTR;
}

// IsUintptr reports whether t is an uintptr.
private static bool IsUintptr(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TUINTPTR;
}

// IsPtrShaped reports whether t is represented by a single machine pointer.
// In addition to regular Go pointer types, this includes map, channel, and
// function types and unsafe.Pointer. It does not include array or struct types
// that consist of a single pointer shaped type.
// TODO(mdempsky): Should it? See golang.org/issue/15028.
private static bool IsPtrShaped(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TPTR || t.kind == TUNSAFEPTR || t.kind == TMAP || t.kind == TCHAN || t.kind == TFUNC;
}

// HasNil reports whether the set of values determined by t includes nil.
private static bool HasNil(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.kind == TCHAN || t.kind == TFUNC || t.kind == TINTER || t.kind == TMAP || t.kind == TNIL || t.kind == TPTR || t.kind == TSLICE || t.kind == TUNSAFEPTR) 
        return true;
        return false;
}

private static bool IsString(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TSTRING;
}

private static bool IsMap(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TMAP;
}

private static bool IsChan(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TCHAN;
}

private static bool IsSlice(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TSLICE;
}

private static bool IsArray(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TARRAY;
}

private static bool IsStruct(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TSTRUCT;
}

private static bool IsInterface(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TINTER;
}

// IsEmptyInterface reports whether t is an empty interface type.
private static bool IsEmptyInterface(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.IsInterface() && t.AllMethods().Len() == 0;
}

// IsScalar reports whether 't' is a scalar Go type, e.g.
// bool/int/float/complex. Note that struct and array types consisting
// of a single scalar element are not considered scalar, likewise
// pointer types are also not considered scalar.
private static bool IsScalar(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.kind == TBOOL || t.kind == TINT8 || t.kind == TUINT8 || t.kind == TINT16 || t.kind == TUINT16 || t.kind == TINT32 || t.kind == TUINT32 || t.kind == TINT64 || t.kind == TUINT64 || t.kind == TINT || t.kind == TUINT || t.kind == TUINTPTR || t.kind == TCOMPLEX64 || t.kind == TCOMPLEX128 || t.kind == TFLOAT32 || t.kind == TFLOAT64) 
        return true;
        return false;
}

private static ptr<Type> PtrTo(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return _addr_NewPtr(_addr_t)!;
}

private static nint NumFields(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t.kind == TRESULTS) {
        return len(t.Extra._<ptr<Results>>().Types);
    }
    return t.Fields().Len();
}
private static ptr<Type> FieldType(this ptr<Type> _addr_t, nint i) => func((_, panic, _) => {
    ref Type t = ref _addr_t.val;

    if (t.kind == TTUPLE) {
        switch (i) {
            case 0: 
                return t.Extra._<ptr<Tuple>>().first;
                break;
            case 1: 
                return t.Extra._<ptr<Tuple>>().second;
                break;
            default: 
                panic("bad tuple index");
                break;
        }
    }
    if (t.kind == TRESULTS) {
        return t.Extra._<ptr<Results>>().Types[i];
    }
    return _addr_t.Field(i).Type!;
});
private static long FieldOff(this ptr<Type> _addr_t, nint i) {
    ref Type t = ref _addr_t.val;

    return t.Field(i).Offset;
}
private static @string FieldName(this ptr<Type> _addr_t, nint i) {
    ref Type t = ref _addr_t.val;

    return t.Field(i).Sym.Name;
}

private static long NumElem(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TARRAY);
    return t.Extra._<ptr<Array>>().Bound;
}

private partial struct componentsIncludeBlankFields { // : bool
}

public static readonly componentsIncludeBlankFields IgnoreBlankFields = false;
public static readonly componentsIncludeBlankFields CountBlankFields = true;

// NumComponents returns the number of primitive elements that compose t.
// Struct and array types are flattened for the purpose of counting.
// All other types (including string, slice, and interface types) count as one element.
// If countBlank is IgnoreBlankFields, then blank struct fields
// (and their comprised elements) are excluded from the count.
// struct { x, y [3]int } has six components; [10]struct{ x, y string } has twenty.
private static long NumComponents(this ptr<Type> _addr_t, componentsIncludeBlankFields countBlank) {
    ref Type t = ref _addr_t.val;


    if (t.kind == TSTRUCT) 
        if (t.IsFuncArgStruct()) {
            @base.Fatalf("NumComponents func arg struct");
        }
        long n = default;
        foreach (var (_, f) in t.FieldSlice()) {
            if (countBlank == IgnoreBlankFields && f.Sym.IsBlank()) {
                continue;
            }
            n += f.Type.NumComponents(countBlank);
        }        return n;
    else if (t.kind == TARRAY) 
        return t.NumElem() * t.Elem().NumComponents(countBlank);
        return 1;
}

// SoleComponent returns the only primitive component in t,
// if there is exactly one. Otherwise, it returns nil.
// Components are counted as in NumComponents, including blank fields.
private static ptr<Type> SoleComponent(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.kind == TSTRUCT) 
        if (t.IsFuncArgStruct()) {
            @base.Fatalf("SoleComponent func arg struct");
        }
        if (t.NumFields() != 1) {
            return _addr_null!;
        }
        return _addr_t.Field(0).Type.SoleComponent()!;
    else if (t.kind == TARRAY) 
        if (t.NumElem() != 1) {
            return _addr_null!;
        }
        return _addr_t.Elem().SoleComponent()!;
        return _addr_t!;
}

// ChanDir returns the direction of a channel type t.
// The direction will be one of Crecv, Csend, or Cboth.
private static ChanDir ChanDir(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    t.wantEtype(TCHAN);
    return t.Extra._<ptr<Chan>>().Dir;
}

private static bool IsMemory(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t == TypeMem || t.kind == TTUPLE && t.Extra._<ptr<Tuple>>().second == TypeMem) {
        return true;
    }
    if (t.kind == TRESULTS) {
        {
            ptr<Results> types = t.Extra._<ptr<Results>>().Types;

            if (len(types) > 0 && types[len(types) - 1] == TypeMem) {
                return true;
            }

        }
    }
    return false;
}
private static bool IsFlags(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t == TypeFlags;
}
private static bool IsVoid(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t == TypeVoid;
}
private static bool IsTuple(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TTUPLE;
}
private static bool IsResults(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return t.kind == TRESULTS;
}

// IsUntyped reports whether t is an untyped type.
private static bool IsUntyped(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t == null) {
        return false;
    }
    if (t == UntypedString || t == UntypedBool) {
        return true;
    }

    if (t.kind == TNIL || t.kind == TIDEAL) 
        return true;
        return false;
}

// HasPointers reports whether t contains a heap pointer.
// Note that this function ignores pointers to go:notinheap types.
private static bool HasPointers(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.kind == TINT || t.kind == TUINT || t.kind == TINT8 || t.kind == TUINT8 || t.kind == TINT16 || t.kind == TUINT16 || t.kind == TINT32 || t.kind == TUINT32 || t.kind == TINT64 || t.kind == TUINT64 || t.kind == TUINTPTR || t.kind == TFLOAT32 || t.kind == TFLOAT64 || t.kind == TCOMPLEX64 || t.kind == TCOMPLEX128 || t.kind == TBOOL || t.kind == TSSA) 
        return false;
    else if (t.kind == TARRAY) 
        if (t.NumElem() == 0) { // empty array has no pointers
            return false;
        }
        return t.Elem().HasPointers();
    else if (t.kind == TSTRUCT) 
        foreach (var (_, t1) in t.Fields().Slice()) {
            if (t1.Type.HasPointers()) {
                return true;
            }
        }        return false;
    else if (t.kind == TPTR || t.kind == TSLICE) 
        return !t.Elem().NotInHeap();
    else if (t.kind == TTUPLE) 
        ptr<Tuple> ttup = t.Extra._<ptr<Tuple>>();
        return ttup.first.HasPointers() || ttup.second.HasPointers();
    else if (t.kind == TRESULTS) 
        ptr<Results> types = t.Extra._<ptr<Results>>().Types;
        foreach (var (_, et) in types) {
            if (et.HasPointers()) {
                return true;
            }
        }        return false;
        return true;
}

// Tie returns 'T' if t is a concrete type,
// 'I' if t is an interface type, and 'E' if t is an empty interface type.
// It is used to build calls to the conv* and assert* runtime routines.
private static byte Tie(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t.IsEmptyInterface()) {
        return 'E';
    }
    if (t.IsInterface()) {
        return 'I';
    }
    return 'T';
}

private static ptr<Type> recvType;

// FakeRecvType returns the singleton type used for interface method receivers.
public static ptr<Type> FakeRecvType() {
    if (recvType == null) {
        recvType = NewPtr(_addr_New(TSTRUCT));
    }
    return _addr_recvType!;
}

 
// TSSA types. HasPointers assumes these are pointer-free.
public static var TypeInvalid = newSSA("invalid");public static var TypeMem = newSSA("mem");public static var TypeFlags = newSSA("flags");public static var TypeVoid = newSSA("void");public static var TypeInt128 = newSSA("int128");public static var TypeResultMem = newResults(new slice<ptr<Type>>(new ptr<Type>[] { TypeMem }));

// NewNamed returns a new named type for the given type name. obj should be an
// ir.Name. The new type is incomplete (marked as TFORW kind), and the underlying
// type should be set later via SetUnderlying(). References to the type are
// maintained until the type is filled in, so those references can be updated when
// the type is complete.
public static ptr<Type> NewNamed(Object obj) {
    var t = New(TFORW);
    t.sym = obj.Sym();
    t.nod = obj;
    return _addr_t!;
}

// Obj returns the canonical type name node for a named type t, nil for an unnamed type.
private static Object Obj(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t.sym != null) {
        return t.nod;
    }
    return null;
}

// SetUnderlying sets the underlying type. SetUnderlying automatically updates any
// types that were waiting for this type to be completed.
private static void SetUnderlying(this ptr<Type> _addr_t, ptr<Type> _addr_underlying) {
    ref Type t = ref _addr_t.val;
    ref Type underlying = ref _addr_underlying.val;

    if (underlying.kind == TFORW) { 
        // This type isn't computed yet; when it is, update n.
        underlying.ForwardType().Copyto = append(underlying.ForwardType().Copyto, t);
        return ;
    }
    var ft = t.ForwardType(); 

    // TODO(mdempsky): Fix Type rekinding.
    t.kind = underlying.kind;
    t.Extra = underlying.Extra;
    t.Width = underlying.Width;
    t.Align = underlying.Align;
    t.underlying = underlying.underlying;

    if (underlying.NotInHeap()) {
        t.SetNotInHeap(true);
    }
    if (underlying.Broke()) {
        t.SetBroke(true);
    }
    if (underlying.HasTParam()) {
        t.SetHasTParam(true);
    }
    if (t.IsInterface()) {
        t.methods = underlying.methods;
        t.allMethods = underlying.allMethods;
    }
    foreach (var (_, w) in ft.Copyto) {
        w.SetUnderlying(t);
    }    if (ft.Embedlineno.IsKnown()) {
        if (t.IsPtr() || t.IsUnsafePtr()) {
            @base.ErrorfAt(ft.Embedlineno, "embedded type cannot be a pointer");
        }
    }
}

private static bool fieldsHasTParam(slice<ptr<Field>> fields) {
    foreach (var (_, f) in fields) {
        if (f.Type != null && f.Type.HasTParam()) {
            return true;
        }
    }    return false;
}

// NewBasic returns a new basic type of the given kind.
public static ptr<Type> NewBasic(Kind kind, Object obj) {
    var t = New(kind);
    t.sym = obj.Sym();
    t.nod = obj;
    return _addr_t!;
}

// NewInterface returns a new interface for the given methods and
// embedded types. Embedded types are specified as fields with no Sym.
public static ptr<Type> NewInterface(ptr<Pkg> _addr_pkg, slice<ptr<Field>> methods) {
    ref Pkg pkg = ref _addr_pkg.val;

    var t = New(TINTER);
    t.SetInterface(methods);
    foreach (var (_, f) in methods) { 
        // f.Type could be nil for a broken interface declaration
        if (f.Type != null && f.Type.HasTParam()) {
            t.SetHasTParam(true);
            break;
        }
    }    if (anyBroke(methods)) {
        t.SetBroke(true);
    }
    t.Extra._<ptr<Interface>>().pkg = pkg;
    return _addr_t!;
}

// NewTypeParam returns a new type param.
public static ptr<Type> NewTypeParam(ptr<Pkg> _addr_pkg) {
    ref Pkg pkg = ref _addr_pkg.val;

    var t = New(TTYPEPARAM);
    t.Extra._<ptr<Interface>>().pkg = pkg;
    t.SetHasTParam(true);
    return _addr_t!;
}

public static readonly nint BOGUS_FUNARG_OFFSET = -1000000000;



private static void unzeroFieldOffsets(slice<ptr<Field>> f) {
    foreach (var (i) in f) {
        f[i].Offset = BOGUS_FUNARG_OFFSET; // This will cause an explosion if it is not corrected
    }
}

// NewSignature returns a new function type for the given receiver,
// parameters, results, and type parameters, any of which may be nil.
public static ptr<Type> NewSignature(ptr<Pkg> _addr_pkg, ptr<Field> _addr_recv, slice<ptr<Field>> tparams, slice<ptr<Field>> @params, slice<ptr<Field>> results) {
    ref Pkg pkg = ref _addr_pkg.val;
    ref Field recv = ref _addr_recv.val;

    slice<ptr<Field>> recvs = default;
    if (recv != null) {
        recvs = new slice<ptr<Field>>(new ptr<Field>[] { recv });
    }
    var t = New(TFUNC);
    var ft = t.FuncType();

    Func<slice<ptr<Field>>, Funarg, ptr<Type>> funargs = (fields, funarg) => {
        var s = NewStruct(_addr_NoPkg, fields);
        s.StructType().Funarg = funarg;
        if (s.Broke()) {
            t.SetBroke(true);
        }
        return _addr_s!;
    };

    if (recv != null) {
        recv.Offset = BOGUS_FUNARG_OFFSET;
    }
    unzeroFieldOffsets(params);
    unzeroFieldOffsets(results);
    ft.Receiver = funargs(recvs, FunargRcvr); 
    // TODO(danscales): just use nil here (save memory) if no tparams
    ft.TParams = funargs(tparams, FunargTparams);
    ft.Params = funargs(params, FunargParams);
    ft.Results = funargs(results, FunargResults);
    ft.pkg = pkg;
    if (len(tparams) > 0 || fieldsHasTParam(recvs) || fieldsHasTParam(params) || fieldsHasTParam(results)) {
        t.SetHasTParam(true);
    }
    return _addr_t!;
}

// NewStruct returns a new struct with the given fields.
public static ptr<Type> NewStruct(ptr<Pkg> _addr_pkg, slice<ptr<Field>> fields) {
    ref Pkg pkg = ref _addr_pkg.val;

    var t = New(TSTRUCT);
    t.SetFields(fields);
    if (anyBroke(fields)) {
        t.SetBroke(true);
    }
    t.Extra._<ptr<Struct>>().pkg = pkg;
    if (fieldsHasTParam(fields)) {
        t.SetHasTParam(true);
    }
    return _addr_t!;
}

private static bool anyBroke(slice<ptr<Field>> fields) {
    foreach (var (_, f) in fields) {
        if (f.Broke()) {
            return true;
        }
    }    return false;
}

public static array<bool> IsInt = new array<bool>(NTYPE);public static array<bool> IsFloat = new array<bool>(NTYPE);public static array<bool> IsComplex = new array<bool>(NTYPE);public static array<bool> IsSimple = new array<bool>(NTYPE);

public static array<bool> IsOrdered = new array<bool>(NTYPE);

// IsReflexive reports whether t has a reflexive equality operator.
// That is, if x==x for all x of type t.
public static bool IsReflexive(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.Kind() == TBOOL || t.Kind() == TINT || t.Kind() == TUINT || t.Kind() == TINT8 || t.Kind() == TUINT8 || t.Kind() == TINT16 || t.Kind() == TUINT16 || t.Kind() == TINT32 || t.Kind() == TUINT32 || t.Kind() == TINT64 || t.Kind() == TUINT64 || t.Kind() == TUINTPTR || t.Kind() == TPTR || t.Kind() == TUNSAFEPTR || t.Kind() == TSTRING || t.Kind() == TCHAN) 
        return true;
    else if (t.Kind() == TFLOAT32 || t.Kind() == TFLOAT64 || t.Kind() == TCOMPLEX64 || t.Kind() == TCOMPLEX128 || t.Kind() == TINTER) 
        return false;
    else if (t.Kind() == TARRAY) 
        return IsReflexive(_addr_t.Elem());
    else if (t.Kind() == TSTRUCT) 
        foreach (var (_, t1) in t.Fields().Slice()) {
            if (!IsReflexive(_addr_t1.Type)) {
                return false;
            }
        }        return true;
    else 
        @base.Fatalf("bad type for map key: %v", t);
        return false;
    }

// Can this type be stored directly in an interface word?
// Yes, if the representation is a single pointer.
public static bool IsDirectIface(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t.Broke()) {
        return false;
    }

    if (t.Kind() == TPTR) 
        // Pointers to notinheap types must be stored indirectly. See issue 42076.
        return !t.Elem().NotInHeap();
    else if (t.Kind() == TCHAN || t.Kind() == TMAP || t.Kind() == TFUNC || t.Kind() == TUNSAFEPTR) 
        return true;
    else if (t.Kind() == TARRAY) 
        // Array of 1 direct iface type can be direct.
        return t.NumElem() == 1 && IsDirectIface(_addr_t.Elem());
    else if (t.Kind() == TSTRUCT) 
        // Struct with 1 field of direct iface type can be direct.
        return t.NumFields() == 1 && IsDirectIface(_addr_t.Field(0).Type);
        return false;
}

// IsInterfaceMethod reports whether (field) m is
// an interface method. Such methods have the
// special receiver type types.FakeRecvType().
public static bool IsInterfaceMethod(ptr<Type> _addr_f) {
    ref Type f = ref _addr_f.val;

    return f.Recv().Type == FakeRecvType();
}

// IsMethodApplicable reports whether method m can be called on a
// value of type t. This is necessary because we compute a single
// method set for both T and *T, but some *T methods are not
// applicable to T receivers.
public static bool IsMethodApplicable(ptr<Type> _addr_t, ptr<Field> _addr_m) {
    ref Type t = ref _addr_t.val;
    ref Field m = ref _addr_m.val;

    return t.IsPtr() || !m.Type.Recv().Type.IsPtr() || IsInterfaceMethod(_addr_m.Type) || m.Embedded == 2;
}

// IsRuntimePkg reports whether p is package runtime.
public static bool IsRuntimePkg(ptr<Pkg> _addr_p) {
    ref Pkg p = ref _addr_p.val;

    if (@base.Flag.CompilingRuntime && p == LocalPkg) {
        return true;
    }
    return p.Path == "runtime";
}

// IsReflectPkg reports whether p is package reflect.
public static bool IsReflectPkg(ptr<Pkg> _addr_p) {
    ref Pkg p = ref _addr_p.val;

    if (p == LocalPkg) {
        return @base.Ctxt.Pkgpath == "reflect";
    }
    return p.Path == "reflect";
}

// ReceiverBaseType returns the underlying type, if any,
// that owns methods with receiver parameter t.
// The result is either a named type or an anonymous struct.
public static ptr<Type> ReceiverBaseType(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    if (t == null) {
        return _addr_null!;
    }
    if (t.IsPtr()) {
        if (t.Sym() != null) {
            return _addr_null!;
        }
        t = t.Elem();
        if (t == null) {
            return _addr_null!;
        }
    }
    if (t.Sym() == null && !t.IsStruct()) {
        return _addr_null!;
    }
    if (IsSimple[t.Kind()]) {
        return _addr_t!;
    }

    if (t.Kind() == TARRAY || t.Kind() == TCHAN || t.Kind() == TFUNC || t.Kind() == TMAP || t.Kind() == TSLICE || t.Kind() == TSTRING || t.Kind() == TSTRUCT) 
        return _addr_t!;
        return _addr_null!;
}

public static ptr<Type> FloatForComplex(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.Kind() == TCOMPLEX64) 
        return _addr_Types[TFLOAT32]!;
    else if (t.Kind() == TCOMPLEX128) 
        return _addr_Types[TFLOAT64]!;
        @base.Fatalf("unexpected type: %v", t);
    return _addr_null!;
}

public static ptr<Type> ComplexForFloat(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;


    if (t.Kind() == TFLOAT32) 
        return _addr_Types[TCOMPLEX64]!;
    else if (t.Kind() == TFLOAT64) 
        return _addr_Types[TCOMPLEX128]!;
        @base.Fatalf("unexpected type: %v", t);
    return _addr_null!;
}

public static ptr<Sym> TypeSym(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return _addr_TypeSymLookup(TypeSymName(_addr_t))!;
}

public static ptr<Sym> TypeSymLookup(@string name) {
    typepkgmu.Lock();
    var s = typepkg.Lookup(name);
    typepkgmu.Unlock();
    return _addr_s!;
}

public static @string TypeSymName(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    var name = t.ShortString(); 
    // Use a separate symbol name for Noalg types for #17752.
    if (TypeHasNoAlg(t)) {
        name = "noalg." + name;
    }
    return name;
}

// Fake package for runtime type info (headers)
// Don't access directly, use typeLookup below.
private static sync.Mutex typepkgmu = default;private static var typepkg = NewPkg("type", "type");

public static array<Kind> SimType = new array<Kind>(NTYPE);

} // end types_package
