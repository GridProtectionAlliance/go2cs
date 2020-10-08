// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 October 08 04:09:51 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Go\src\cmd\compile\internal\types\type.go
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class types_package
    {
        // Dummy Node so we can refer to *Node without actually
        // having a gc.Node. Necessary to break import cycles.
        // TODO(gri) try to eliminate soon
        public partial struct Node
        {
            public long _;
        }

        //go:generate stringer -type EType -trimprefix T

        // EType describes a kind of type.
        public partial struct EType // : byte
        {
        }

        public static readonly EType Txxx = (EType)iota;

        public static readonly var TINT8 = (var)0;
        public static readonly var TUINT8 = (var)1;
        public static readonly var TINT16 = (var)2;
        public static readonly var TUINT16 = (var)3;
        public static readonly var TINT32 = (var)4;
        public static readonly var TUINT32 = (var)5;
        public static readonly var TINT64 = (var)6;
        public static readonly var TUINT64 = (var)7;
        public static readonly var TINT = (var)8;
        public static readonly var TUINT = (var)9;
        public static readonly var TUINTPTR = (var)10;

        public static readonly var TCOMPLEX64 = (var)11;
        public static readonly var TCOMPLEX128 = (var)12;

        public static readonly var TFLOAT32 = (var)13;
        public static readonly var TFLOAT64 = (var)14;

        public static readonly var TBOOL = (var)15;

        public static readonly var TPTR = (var)16;
        public static readonly var TFUNC = (var)17;
        public static readonly var TSLICE = (var)18;
        public static readonly var TARRAY = (var)19;
        public static readonly var TSTRUCT = (var)20;
        public static readonly var TCHAN = (var)21;
        public static readonly var TMAP = (var)22;
        public static readonly var TINTER = (var)23;
        public static readonly var TFORW = (var)24;
        public static readonly var TANY = (var)25;
        public static readonly var TSTRING = (var)26;
        public static readonly var TUNSAFEPTR = (var)27; 

        // pseudo-types for literals
        public static readonly var TIDEAL = (var)28; // untyped numeric constants
        public static readonly var TNIL = (var)29;
        public static readonly var TBLANK = (var)30; 

        // pseudo-types for frame layout
        public static readonly var TFUNCARGS = (var)31;
        public static readonly var TCHANARGS = (var)32; 

        // SSA backend types
        public static readonly var TSSA = (var)33; // internal types used by SSA backend (flags, memory, etc.)
        public static readonly var TTUPLE = (var)34; // a pair of types, used by SSA backend

        public static readonly var NTYPE = (var)35;


        // ChanDir is whether a channel can send, receive, or both.
        public partial struct ChanDir // : byte
        {
        }

        public static bool CanRecv(this ChanDir c)
        {
            return c & Crecv != 0L;
        }
        public static bool CanSend(this ChanDir c)
        {
            return c & Csend != 0L;
        }

 
        // types of channel
        // must match ../../../../reflect/type.go:/ChanDir
        public static readonly ChanDir Crecv = (ChanDir)1L << (int)(0L);
        public static readonly ChanDir Csend = (ChanDir)1L << (int)(1L);
        public static readonly ChanDir Cboth = (ChanDir)Crecv | Csend;


        // Types stores pointers to predeclared named types.
        //
        // It also stores pointers to several special types:
        //   - Types[TANY] is the placeholder "any" type recognized by substArgTypes.
        //   - Types[TBLANK] represents the blank variable's type.
        //   - Types[TNIL] represents the predeclared "nil" value's type.
        //   - Types[TUNSAFEPTR] is package unsafe's Pointer type.
        public static array<ptr<Type>> Types = new array<ptr<Type>>(NTYPE);

 
        // Predeclared alias types. Kept separate for better error messages.
        public static ptr<Type> Bytetype;        public static ptr<Type> Runetype;        public static ptr<Type> Errortype;        public static ptr<Type> Idealstring;        public static ptr<Type> Idealbool;        public static var Idealint = New(TIDEAL);        public static var Idealrune = New(TIDEAL);        public static var Idealfloat = New(TIDEAL);        public static var Idealcomplex = New(TIDEAL);

        // A Type represents a Go type.
        public partial struct Type
        {
            public long Width; // valid if Align > 0

            public Fields methods;
            public Fields allMethods;
            public ptr<Node> Nod; // canonical OTYPE node
            public ptr<Type> Orig; // original type (type literal or predefined type)

// Cache of composite types, with this type being the element type.
            public ptr<Sym> Sym; // symbol containing name, for named types
            public int Vargen; // unique name for OTYPE/ONAME

            public EType Etype; // kind of type
            public byte Align; // the required alignment of this type, in bytes (0 means Width and Align have not yet been computed)

            public bitset8 flags;
        }

        private static readonly long typeNotInHeap = (long)1L << (int)(iota); // type cannot be heap allocated
        private static readonly var typeBroke = (var)0; // broken type definition
        private static readonly var typeNoalg = (var)1; // suppress hash and eq algorithm generation
        private static readonly var typeDeferwidth = (var)2; // width computation has been deferred and type is on deferredTypeStack
        private static readonly var typeRecur = (var)3;


        private static bool NotInHeap(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.flags & typeNotInHeap != 0L;
        }
        private static bool Broke(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.flags & typeBroke != 0L;
        }
        private static bool Noalg(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.flags & typeNoalg != 0L;
        }
        private static bool Deferwidth(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.flags & typeDeferwidth != 0L;
        }
        private static bool Recur(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.flags & typeRecur != 0L;
        }

        private static void SetNotInHeap(this ptr<Type> _addr_t, bool b)
        {
            ref Type t = ref _addr_t.val;

            t.flags.set(typeNotInHeap, b);
        }
        private static void SetBroke(this ptr<Type> _addr_t, bool b)
        {
            ref Type t = ref _addr_t.val;

            t.flags.set(typeBroke, b);
        }
        private static void SetNoalg(this ptr<Type> _addr_t, bool b)
        {
            ref Type t = ref _addr_t.val;

            t.flags.set(typeNoalg, b);
        }
        private static void SetDeferwidth(this ptr<Type> _addr_t, bool b)
        {
            ref Type t = ref _addr_t.val;

            t.flags.set(typeDeferwidth, b);
        }
        private static void SetRecur(this ptr<Type> _addr_t, bool b)
        {
            ref Type t = ref _addr_t.val;

            t.flags.set(typeRecur, b);
        }

        // Pkg returns the package that t appeared in.
        //
        // Pkg is only defined for function, struct, and interface types
        // (i.e., types with named elements). This information isn't used by
        // cmd/compile itself, but we need to track it because it's exposed by
        // the go/types API.
        private static ptr<Pkg> Pkg(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;


            if (t.Etype == TFUNC) 
                return t.Extra._<ptr<Func>>().pkg;
            else if (t.Etype == TSTRUCT) 
                return t.Extra._<ptr<Struct>>().pkg;
            else if (t.Etype == TINTER) 
                return t.Extra._<ptr<Interface>>().pkg;
            else 
                Fatalf("Pkg: unexpected kind: %v", t);
                return _addr_null!;
            
        }

        // SetPkg sets the package that t appeared in.
        private static void SetPkg(this ptr<Type> _addr_t, ptr<Pkg> _addr_pkg)
        {
            ref Type t = ref _addr_t.val;
            ref Pkg pkg = ref _addr_pkg.val;


            if (t.Etype == TFUNC) 
                t.Extra._<ptr<Func>>().pkg = pkg;
            else if (t.Etype == TSTRUCT) 
                t.Extra._<ptr<Struct>>().pkg = pkg;
            else if (t.Etype == TINTER) 
                t.Extra._<ptr<Interface>>().pkg = pkg;
            else 
                Fatalf("Pkg: unexpected kind: %v", t);
            
        }

        // Map contains Type fields specific to maps.
        public partial struct Map
        {
            public ptr<Type> Key; // Key type
            public ptr<Type> Elem; // Val (elem) type

            public ptr<Type> Bucket; // internal struct type representing a hash bucket
            public ptr<Type> Hmap; // internal struct type representing the Hmap (map header object)
            public ptr<Type> Hiter; // internal struct type representing hash iterator state
        }

        // MapType returns t's extra map-specific fields.
        private static ptr<Map> MapType(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TMAP);
            return t.Extra._<ptr<Map>>();
        }

        // Forward contains Type fields specific to forward types.
        public partial struct Forward
        {
            public slice<ptr<Type>> Copyto; // where to copy the eventual value to
            public src.XPos Embedlineno; // first use of this type as an embedded type
        }

        // ForwardType returns t's extra forward-type-specific fields.
        private static ptr<Forward> ForwardType(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TFORW);
            return t.Extra._<ptr<Forward>>();
        }

        // Func contains Type fields specific to func types.
        public partial struct Func
        {
            public ptr<Type> Receiver; // function receiver
            public ptr<Type> Results; // function results
            public ptr<Type> Params; // function params

            public ptr<Node> Nname;
            public ptr<Pkg> pkg; // Argwid is the total width of the function receiver, params, and results.
// It gets calculated via a temporary TFUNCARGS type.
// Note that TFUNC's Width is Widthptr.
            public long Argwid;
            public bool Outnamed;
        }

        // FuncType returns t's extra func-specific fields.
        private static ptr<Func> FuncType(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TFUNC);
            return t.Extra._<ptr<Func>>();
        }

        // StructType contains Type fields specific to struct types.
        public partial struct Struct
        {
            public Fields fields;
            public ptr<Pkg> pkg; // Maps have three associated internal structs (see struct MapType).
// Map links such structs back to their map type.
            public ptr<Type> Map;
            public Funarg Funarg; // type of function arguments for arg struct
        }

        // Fnstruct records the kind of function argument
        public partial struct Funarg // : byte
        {
        }

        public static readonly Funarg FunargNone = (Funarg)iota;
        public static readonly var FunargRcvr = (var)0; // receiver
        public static readonly var FunargParams = (var)1; // input parameters
        public static readonly var FunargResults = (var)2; // output results

        // StructType returns t's extra struct-specific fields.
        private static ptr<Struct> StructType(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TSTRUCT);
            return t.Extra._<ptr<Struct>>();
        }

        // Interface contains Type fields specific to interface types.
        public partial struct Interface
        {
            public Fields Fields;
            public ptr<Pkg> pkg;
        }

        // Ptr contains Type fields specific to pointer types.
        public partial struct Ptr
        {
            public ptr<Type> Elem; // element type
        }

        // ChanArgs contains Type fields specific to TCHANARGS types.
        public partial struct ChanArgs
        {
            public ptr<Type> T; // reference to a chan type whose elements need a width check
        }

        // // FuncArgs contains Type fields specific to TFUNCARGS types.
        public partial struct FuncArgs
        {
            public ptr<Type> T; // reference to a func type whose elements need a width check
        }

        // Chan contains Type fields specific to channel types.
        public partial struct Chan
        {
            public ptr<Type> Elem; // element type
            public ChanDir Dir; // channel direction
        }

        // ChanType returns t's extra channel-specific fields.
        private static ptr<Chan> ChanType(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TCHAN);
            return t.Extra._<ptr<Chan>>();
        }

        public partial struct Tuple
        {
            public ptr<Type> first;
            public ptr<Type> second; // Any tuple with a memory type must put that memory type second.
        }

        // Array contains Type fields specific to array types.
        public partial struct Array
        {
            public ptr<Type> Elem; // element type
            public long Bound; // number of elements; <0 if unknown yet
        }

        // Slice contains Type fields specific to slice types.
        public partial struct Slice
        {
            public ptr<Type> Elem; // element type
        }

        // A Field represents a field in a struct or a method in an interface or
        // associated with a named type.
        public partial struct Field
        {
            public bitset8 flags;
            public byte Embedded; // embedded field

            public src.XPos Pos;
            public ptr<Sym> Sym;
            public ptr<Type> Type; // field type
            public @string Note; // literal string annotation

// For fields that represent function parameters, Nname points
// to the associated ONAME Node.
            public ptr<Node> Nname; // Offset in bytes of this field or method within its enclosing struct
// or interface Type.
            public long Offset;
        }

        private static readonly long fieldIsDDD = (long)1L << (int)(iota); // field is ... argument
        private static readonly var fieldBroke = (var)0; // broken field definition
        private static readonly var fieldNointerface = (var)1;


        private static bool IsDDD(this ptr<Field> _addr_f)
        {
            ref Field f = ref _addr_f.val;

            return f.flags & fieldIsDDD != 0L;
        }
        private static bool Broke(this ptr<Field> _addr_f)
        {
            ref Field f = ref _addr_f.val;

            return f.flags & fieldBroke != 0L;
        }
        private static bool Nointerface(this ptr<Field> _addr_f)
        {
            ref Field f = ref _addr_f.val;

            return f.flags & fieldNointerface != 0L;
        }

        private static void SetIsDDD(this ptr<Field> _addr_f, bool b)
        {
            ref Field f = ref _addr_f.val;

            f.flags.set(fieldIsDDD, b);
        }
        private static void SetBroke(this ptr<Field> _addr_f, bool b)
        {
            ref Field f = ref _addr_f.val;

            f.flags.set(fieldBroke, b);
        }
        private static void SetNointerface(this ptr<Field> _addr_f, bool b)
        {
            ref Field f = ref _addr_f.val;

            f.flags.set(fieldNointerface, b);
        }

        // End returns the offset of the first byte immediately after this field.
        private static long End(this ptr<Field> _addr_f)
        {
            ref Field f = ref _addr_f.val;

            return f.Offset + f.Type.Width;
        }

        // IsMethod reports whether f represents a method rather than a struct field.
        private static bool IsMethod(this ptr<Field> _addr_f)
        {
            ref Field f = ref _addr_f.val;

            return f.Type.Etype == TFUNC && f.Type.Recv() != null;
        }

        // Fields is a pointer to a slice of *Field.
        // This saves space in Types that do not have fields or methods
        // compared to a simple slice of *Field.
        public partial struct Fields
        {
            public ptr<slice<ptr<Field>>> s;
        }

        // Len returns the number of entries in f.
        private static long Len(this ptr<Fields> _addr_f)
        {
            ref Fields f = ref _addr_f.val;

            if (f.s == null)
            {
                return 0L;
            }

            return len(f.s.val);

        }

        // Slice returns the entries in f as a slice.
        // Changes to the slice entries will be reflected in f.
        private static slice<ptr<Field>> Slice(this ptr<Fields> _addr_f)
        {
            ref Fields f = ref _addr_f.val;

            if (f.s == null)
            {
                return null;
            }

            return f.s.val;

        }

        // Index returns the i'th element of Fields.
        // It panics if f does not have at least i+1 elements.
        private static ptr<Field> Index(this ptr<Fields> _addr_f, long i)
        {
            ref Fields f = ref _addr_f.val;

            return _addr_(f.s.val)[i]!;
        }

        // Set sets f to a slice.
        // This takes ownership of the slice.
        private static void Set(this ptr<Fields> _addr_f, slice<ptr<Field>> s)
        {
            ref Fields f = ref _addr_f.val;

            if (len(s) == 0L)
            {
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
        private static void Append(this ptr<Fields> _addr_f, params ptr<ptr<Field>>[] _addr_s)
        {
            s = s.Clone();
            ref Fields f = ref _addr_f.val;
            ref Field s = ref _addr_s.val;

            if (f.s == null)
            {
                f.s = @new<*Field>();
            }

            f.s.val = append(f.s.val, s);

        }

        // New returns a new Type of the specified kind.
        public static ptr<Type> New(EType et)
        {
            ptr<Type> t = addr(new Type(Etype:et,Width:BADWIDTH,));
            t.Orig = t; 
            // TODO(josharian): lazily initialize some of these?

            if (t.Etype == TMAP) 
                t.Extra = @new<Map>();
            else if (t.Etype == TFORW) 
                t.Extra = @new<Forward>();
            else if (t.Etype == TFUNC) 
                t.Extra = @new<Func>();
            else if (t.Etype == TSTRUCT) 
                t.Extra = @new<Struct>();
            else if (t.Etype == TINTER) 
                t.Extra = @new<Interface>();
            else if (t.Etype == TPTR) 
                t.Extra = new Ptr();
            else if (t.Etype == TCHANARGS) 
                t.Extra = new ChanArgs();
            else if (t.Etype == TFUNCARGS) 
                t.Extra = new FuncArgs();
            else if (t.Etype == TCHAN) 
                t.Extra = @new<Chan>();
            else if (t.Etype == TTUPLE) 
                t.Extra = @new<Tuple>();
                        return _addr_t!;

        }

        // NewArray returns a new fixed-length array Type.
        public static ptr<Type> NewArray(ptr<Type> _addr_elem, long bound)
        {
            ref Type elem = ref _addr_elem.val;

            if (bound < 0L)
            {
                Fatalf("NewArray: invalid bound %v", bound);
            }

            var t = New(TARRAY);
            t.Extra = addr(new Array(Elem:elem,Bound:bound));
            t.SetNotInHeap(elem.NotInHeap());
            return _addr_t!;

        }

        // NewSlice returns the slice Type with element type elem.
        public static ptr<Type> NewSlice(ptr<Type> _addr_elem)
        {
            ref Type elem = ref _addr_elem.val;

            {
                var t__prev1 = t;

                var t = elem.Cache.slice;

                if (t != null)
                {
                    if (t.Elem() != elem)
                    {
                        Fatalf("elem mismatch");
                    }

                    return _addr_t!;

                }

                t = t__prev1;

            }


            t = New(TSLICE);
            t.Extra = new Slice(Elem:elem);
            elem.Cache.slice = t;
            return _addr_t!;

        }

        // NewChan returns a new chan Type with direction dir.
        public static ptr<Type> NewChan(ptr<Type> _addr_elem, ChanDir dir)
        {
            ref Type elem = ref _addr_elem.val;

            var t = New(TCHAN);
            var ct = t.ChanType();
            ct.Elem = elem;
            ct.Dir = dir;
            return _addr_t!;
        }

        public static ptr<Type> NewTuple(ptr<Type> _addr_t1, ptr<Type> _addr_t2)
        {
            ref Type t1 = ref _addr_t1.val;
            ref Type t2 = ref _addr_t2.val;

            var t = New(TTUPLE);
            t.Extra._<ptr<Tuple>>().first = t1;
            t.Extra._<ptr<Tuple>>().second = t2;
            return _addr_t!;
        }

        private static ptr<Type> newSSA(@string name)
        {
            var t = New(TSSA);
            t.Extra = name;
            return _addr_t!;
        }

        // NewMap returns a new map Type with key type k and element (aka value) type v.
        public static ptr<Type> NewMap(ptr<Type> _addr_k, ptr<Type> _addr_v)
        {
            ref Type k = ref _addr_k.val;
            ref Type v = ref _addr_v.val;

            var t = New(TMAP);
            var mt = t.MapType();
            mt.Key = k;
            mt.Elem = v;
            return _addr_t!;
        }

        // NewPtrCacheEnabled controls whether *T Types are cached in T.
        // Caching is disabled just before starting the backend.
        // This allows the backend to run concurrently.
        public static var NewPtrCacheEnabled = true;

        // NewPtr returns the pointer type pointing to t.
        public static ptr<Type> NewPtr(ptr<Type> _addr_elem)
        {
            ref Type elem = ref _addr_elem.val;

            if (elem == null)
            {
                Fatalf("NewPtr: pointer to elem Type is nil");
            }

            {
                var t__prev1 = t;

                var t = elem.Cache.ptr;

                if (t != null)
                {
                    if (t.Elem() != elem)
                    {
                        Fatalf("NewPtr: elem mismatch");
                    }

                    return _addr_t!;

                }

                t = t__prev1;

            }


            t = New(TPTR);
            t.Extra = new Ptr(Elem:elem);
            t.Width = int64(Widthptr);
            t.Align = uint8(Widthptr);
            if (NewPtrCacheEnabled)
            {
                elem.Cache.ptr = t;
            }

            return _addr_t!;

        }

        // NewChanArgs returns a new TCHANARGS type for channel type c.
        public static ptr<Type> NewChanArgs(ptr<Type> _addr_c)
        {
            ref Type c = ref _addr_c.val;

            var t = New(TCHANARGS);
            t.Extra = new ChanArgs(T:c);
            return _addr_t!;
        }

        // NewFuncArgs returns a new TFUNCARGS type for func type f.
        public static ptr<Type> NewFuncArgs(ptr<Type> _addr_f)
        {
            ref Type f = ref _addr_f.val;

            var t = New(TFUNCARGS);
            t.Extra = new FuncArgs(T:f);
            return _addr_t!;
        }

        public static ptr<Field> NewField()
        {
            return addr(new Field(Offset:BADWIDTH,));
        }

        // SubstAny walks t, replacing instances of "any" with successive
        // elements removed from types.  It returns the substituted type.
        public static ptr<Type> SubstAny(ptr<Type> _addr_t, ptr<slice<ptr<Type>>> _addr_types)
        {
            ref Type t = ref _addr_t.val;
            ref slice<ptr<Type>> types = ref _addr_types.val;

            if (t == null)
            {
                return _addr_null!;
            }


            if (t.Etype == TANY) 
                if (len(types.val) == 0L)
                {
                    Fatalf("substArgTypes: not enough argument types");
                }

                t = (types.val)[0L];
                types.val = (types.val)[1L..];
            else if (t.Etype == TPTR) 
                var elem = SubstAny(_addr_t.Elem(), _addr_types);
                if (elem != t.Elem())
                {
                    t = t.copy();
                    t.Extra = new Ptr(Elem:elem);
                }

            else if (t.Etype == TARRAY) 
                elem = SubstAny(_addr_t.Elem(), _addr_types);
                if (elem != t.Elem())
                {
                    t = t.copy();
                    t.Extra._<ptr<Array>>().Elem = elem;
                }

            else if (t.Etype == TSLICE) 
                elem = SubstAny(_addr_t.Elem(), _addr_types);
                if (elem != t.Elem())
                {
                    t = t.copy();
                    t.Extra = new Slice(Elem:elem);
                }

            else if (t.Etype == TCHAN) 
                elem = SubstAny(_addr_t.Elem(), _addr_types);
                if (elem != t.Elem())
                {
                    t = t.copy();
                    t.Extra._<ptr<Chan>>().Elem = elem;
                }

            else if (t.Etype == TMAP) 
                var key = SubstAny(_addr_t.Key(), _addr_types);
                elem = SubstAny(_addr_t.Elem(), _addr_types);
                if (key != t.Key() || elem != t.Elem())
                {
                    t = t.copy();
                    t.Extra._<ptr<Map>>().Key = key;
                    t.Extra._<ptr<Map>>().Elem = elem;
                }

            else if (t.Etype == TFUNC) 
                var recvs = SubstAny(_addr_t.Recvs(), _addr_types);
                var @params = SubstAny(_addr_t.Params(), _addr_types);
                var results = SubstAny(_addr_t.Results(), _addr_types);
                if (recvs != t.Recvs() || params != t.Params() || results != t.Results())
                {
                    t = t.copy();
                    t.FuncType().Receiver = recvs;
                    t.FuncType().Results = results;
                    t.FuncType().Params = params;
                }

            else if (t.Etype == TSTRUCT) 
                // Make a copy of all fields, including ones whose type does not change.
                // This prevents aliasing across functions, which can lead to later
                // fields getting their Offset incorrectly overwritten.
                var fields = t.FieldSlice();
                var nfs = make_slice<ptr<Field>>(len(fields));
                foreach (var (i, f) in fields)
                {
                    var nft = SubstAny(_addr_f.Type, _addr_types);
                    nfs[i] = f.Copy();
                    nfs[i].Type = nft;
                }
                t = t.copy();
                t.SetFields(nfs);
            else                         return _addr_t!;

        }

        // copy returns a shallow copy of the Type.
        private static ptr<Type> copy(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            if (t == null)
            {
                return _addr_null!;
            }

            ref var nt = ref heap(t.val, out ptr<var> _addr_nt); 
            // copy any *T Extra fields, to avoid aliasing

            if (t.Etype == TMAP) 
                ptr<Map> x = t.Extra._<ptr<Map>>().val;
                nt.Extra = _addr_x;
            else if (t.Etype == TFORW) 
                x = t.Extra._<ptr<Forward>>().val;
                nt.Extra = _addr_x;
            else if (t.Etype == TFUNC) 
                x = t.Extra._<ptr<Func>>().val;
                nt.Extra = _addr_x;
            else if (t.Etype == TSTRUCT) 
                x = t.Extra._<ptr<Struct>>().val;
                nt.Extra = _addr_x;
            else if (t.Etype == TINTER) 
                x = t.Extra._<ptr<Interface>>().val;
                nt.Extra = _addr_x;
            else if (t.Etype == TCHAN) 
                x = t.Extra._<ptr<Chan>>().val;
                nt.Extra = _addr_x;
            else if (t.Etype == TARRAY) 
                x = t.Extra._<ptr<Array>>().val;
                nt.Extra = _addr_x;
            else if (t.Etype == TTUPLE || t.Etype == TSSA) 
                Fatalf("ssa types cannot be copied");
            // TODO(mdempsky): Find out why this is necessary and explain.
            if (t.Orig == t)
            {
                _addr_nt.Orig = _addr_nt;
                nt.Orig = ref _addr_nt.Orig.val;

            }

            return _addr__addr_nt!;

        }

        private static ptr<Field> Copy(this ptr<Field> _addr_f)
        {
            ref Field f = ref _addr_f.val;

            ref var nf = ref heap(f.val, out ptr<var> _addr_nf);
            return _addr__addr_nf!;
        }

        private static void wantEtype(this ptr<Type> _addr_t, EType et)
        {
            ref Type t = ref _addr_t.val;

            if (t.Etype != et)
            {
                Fatalf("want %v, but have %v", et, t);
            }

        }

        private static ptr<Type> Recvs(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return _addr_t.FuncType().Receiver!;
        }
        private static ptr<Type> Params(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return _addr_t.FuncType().Params!;
        }
        private static ptr<Type> Results(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return _addr_t.FuncType().Results!;
        }

        private static long NumRecvs(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.FuncType().Receiver.NumFields();
        }
        private static long NumParams(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.FuncType().Params.NumFields();
        }
        private static long NumResults(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.FuncType().Results.NumFields();
        }

        // IsVariadic reports whether function type t is variadic.
        private static bool IsVariadic(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            var n = t.NumParams();
            return n > 0L && t.Params().Field(n - 1L).IsDDD();
        }

        // Recv returns the receiver of function type t, if any.
        private static ptr<Field> Recv(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            var s = t.Recvs();
            if (s.NumFields() == 0L)
            {
                return _addr_null!;
            }

            return _addr_s.Field(0L)!;

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
        private static ptr<Type> Key(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TMAP);
            return t.Extra._<ptr<Map>>().Key;
        }

        // Elem returns the type of elements of t.
        // Usable with pointers, channels, arrays, slices, and maps.
        private static ptr<Type> Elem(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;


            if (t.Etype == TPTR) 
                return _addr_t.Extra._<Ptr>().Elem!;
            else if (t.Etype == TARRAY) 
                return t.Extra._<ptr<Array>>().Elem;
            else if (t.Etype == TSLICE) 
                return _addr_t.Extra._<Slice>().Elem!;
            else if (t.Etype == TCHAN) 
                return t.Extra._<ptr<Chan>>().Elem;
            else if (t.Etype == TMAP) 
                return t.Extra._<ptr<Map>>().Elem;
                        Fatalf("Type.Elem %s", t.Etype);
            return _addr_null!;

        }

        // ChanArgs returns the channel type for TCHANARGS type t.
        private static ptr<Type> ChanArgs(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TCHANARGS);
            return _addr_t.Extra._<ChanArgs>().T!;
        }

        // FuncArgs returns the func type for TFUNCARGS type t.
        private static ptr<Type> FuncArgs(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TFUNCARGS);
            return _addr_t.Extra._<FuncArgs>().T!;
        }

        // Nname returns the associated function's nname.
        private static ptr<Node> Nname(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;


            if (t.Etype == TFUNC) 
                return t.Extra._<ptr<Func>>().Nname;
                        Fatalf("Type.Nname %v %v", t.Etype, t);
            return _addr_null!;

        }

        // Nname sets the associated function's nname.
        private static void SetNname(this ptr<Type> _addr_t, ptr<Node> _addr_n)
        {
            ref Type t = ref _addr_t.val;
            ref Node n = ref _addr_n.val;


            if (t.Etype == TFUNC) 
                t.Extra._<ptr<Func>>().Nname = n;
            else 
                Fatalf("Type.SetNname %v %v", t.Etype, t);
            
        }

        // IsFuncArgStruct reports whether t is a struct representing function parameters.
        private static bool IsFuncArgStruct(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TSTRUCT && t.Extra._<ptr<Struct>>().Funarg != FunargNone;
        }

        private static ptr<Fields> Methods(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;
 
            // TODO(mdempsky): Validate t?
            return _addr__addr_t.methods!;

        }

        private static ptr<Fields> AllMethods(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;
 
            // TODO(mdempsky): Validate t?
            return _addr__addr_t.allMethods!;

        }

        private static ptr<Fields> Fields(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;


            if (t.Etype == TSTRUCT) 
                return _addr_t.Extra._<ptr<Struct>>().fields;
            else if (t.Etype == TINTER) 
                Dowidth(t);
                return _addr_t.Extra._<ptr<Interface>>().Fields;
                        Fatalf("Fields: type %v does not have fields", t);
            return _addr_null!;

        }

        // Field returns the i'th field/method of struct/interface type t.
        private static ptr<Field> Field(this ptr<Type> _addr_t, long i)
        {
            ref Type t = ref _addr_t.val;

            return _addr_t.Fields().Slice()[i]!;
        }

        // FieldSlice returns a slice of containing all fields/methods of
        // struct/interface type t.
        private static slice<ptr<Field>> FieldSlice(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Fields().Slice();
        }

        // SetFields sets struct/interface type t's fields/methods to fields.
        private static void SetFields(this ptr<Type> _addr_t, slice<ptr<Field>> fields)
        {
            ref Type t = ref _addr_t.val;
 
            // If we've calculated the width of t before,
            // then some other type such as a function signature
            // might now have the wrong type.
            // Rather than try to track and invalidate those,
            // enforce that SetFields cannot be called once
            // t's width has been calculated.
            if (t.WidthCalculated())
            {
                Fatalf("SetFields of %v: width previously calculated", t);
            }

            t.wantEtype(TSTRUCT);
            foreach (var (_, f) in fields)
            { 
                // If type T contains a field F with a go:notinheap
                // type, then T must also be go:notinheap. Otherwise,
                // you could heap allocate T and then get a pointer F,
                // which would be a heap pointer to a go:notinheap
                // type.
                if (f.Type != null && f.Type.NotInHeap())
                {
                    t.SetNotInHeap(true);
                    break;
                }

            }
            t.Fields().Set(fields);

        }

        private static void SetInterface(this ptr<Type> _addr_t, slice<ptr<Field>> methods)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TINTER);
            t.Methods().Set(methods);
        }

        private static bool WidthCalculated(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Align > 0L;
        }

        // ArgWidth returns the total aligned argument size for a function.
        // It includes the receiver, parameters, and results.
        private static long ArgWidth(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TFUNC);
            return t.Extra._<ptr<Func>>().Argwid;
        }

        private static long Size(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            if (t.Etype == TSSA)
            {
                if (t == TypeInt128)
                {
                    return 16L;
                }

                return 0L;

            }

            Dowidth(t);
            return t.Width;

        }

        private static long Alignment(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            Dowidth(t);
            return int64(t.Align);
        }

        private static @string SimpleString(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype.String();
        }

        // Cmp is a comparison between values a and b.
        // -1 if a < b
        //  0 if a == b
        //  1 if a > b
        public partial struct Cmp // : sbyte
        {
        }

        public static readonly var CMPlt = (var)Cmp(-1L);
        public static readonly var CMPeq = (var)Cmp(0L);
        public static readonly var CMPgt = (var)Cmp(1L);


        // Compare compares types for purposes of the SSA back
        // end, returning a Cmp (one of CMPlt, CMPeq, CMPgt).
        // The answers are correct for an optimizer
        // or code generator, but not necessarily typechecking.
        // The order chosen is arbitrary, only consistency and division
        // into equivalence classes (Types that compare CMPeq) matters.
        private static Cmp Compare(this ptr<Type> _addr_t, ptr<Type> _addr_x)
        {
            ref Type t = ref _addr_t.val;
            ref Type x = ref _addr_x.val;

            if (x == t)
            {
                return CMPeq;
            }

            return t.cmp(x);

        }

        private static Cmp cmpForNe(bool x)
        {
            if (x)
            {
                return CMPlt;
            }

            return CMPgt;

        }

        private static Cmp cmpsym(this ptr<Sym> _addr_r, ptr<Sym> _addr_s)
        {
            ref Sym r = ref _addr_r.val;
            ref Sym s = ref _addr_s.val;

            if (r == s)
            {
                return CMPeq;
            }

            if (r == null)
            {
                return CMPlt;
            }

            if (s == null)
            {
                return CMPgt;
            } 
            // Fast sort, not pretty sort
            if (len(r.Name) != len(s.Name))
            {
                return cmpForNe(len(r.Name) < len(s.Name));
            }

            if (r.Pkg != s.Pkg)
            {
                if (len(r.Pkg.Prefix) != len(s.Pkg.Prefix))
                {
                    return cmpForNe(len(r.Pkg.Prefix) < len(s.Pkg.Prefix));
                }

                if (r.Pkg.Prefix != s.Pkg.Prefix)
                {
                    return cmpForNe(r.Pkg.Prefix < s.Pkg.Prefix);
                }

            }

            if (r.Name != s.Name)
            {
                return cmpForNe(r.Name < s.Name);
            }

            return CMPeq;

        }

        // cmp compares two *Types t and x, returning CMPlt,
        // CMPeq, CMPgt as t<x, t==x, t>x, for an arbitrary
        // and optimizer-centric notion of comparison.
        // TODO(josharian): make this safe for recursive interface types
        // and use in signatlist sorting. See issue 19869.
        private static Cmp cmp(this ptr<Type> _addr_t, ptr<Type> _addr_x) => func((_, panic, __) =>
        {
            ref Type t = ref _addr_t.val;
            ref Type x = ref _addr_x.val;
 
            // This follows the structure of function identical in identity.go
            // with two exceptions.
            // 1. Symbols are compared more carefully because a <,=,> result is desired.
            // 2. Maps are treated specially to avoid endless recursion -- maps
            //    contain an internal data type not expressible in Go source code.
            if (t == x)
            {
                return CMPeq;
            }

            if (t == null)
            {
                return CMPlt;
            }

            if (x == null)
            {
                return CMPgt;
            }

            if (t.Etype != x.Etype)
            {
                return cmpForNe(t.Etype < x.Etype);
            }

            if (t.Sym != null || x.Sym != null)
            { 
                // Special case: we keep byte and uint8 separate
                // for error messages. Treat them as equal.

                if (t.Etype == TUINT8) 
                    if ((t == Types[TUINT8] || t == Bytetype) && (x == Types[TUINT8] || x == Bytetype))
                    {
                        return CMPeq;
                    }

                else if (t.Etype == TINT32) 
                    if ((t == Types[Runetype.Etype] || t == Runetype) && (x == Types[Runetype.Etype] || x == Runetype))
                    {
                        return CMPeq;
                    }

                            }

            {
                var c__prev1 = c;

                var c = t.Sym.cmpsym(x.Sym);

                if (c != CMPeq)
                {
                    return c;
                }

                c = c__prev1;

            }


            if (x.Sym != null)
            { 
                // Syms non-nil, if vargens match then equal.
                if (t.Vargen != x.Vargen)
                {
                    return cmpForNe(t.Vargen < x.Vargen);
                }

                return CMPeq;

            } 
            // both syms nil, look at structure below.

            if (t.Etype == TBOOL || t.Etype == TFLOAT32 || t.Etype == TFLOAT64 || t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128 || t.Etype == TUNSAFEPTR || t.Etype == TUINTPTR || t.Etype == TINT8 || t.Etype == TINT16 || t.Etype == TINT32 || t.Etype == TINT64 || t.Etype == TINT || t.Etype == TUINT8 || t.Etype == TUINT16 || t.Etype == TUINT32 || t.Etype == TUINT64 || t.Etype == TUINT) 
                return CMPeq;
            else if (t.Etype == TSSA) 
                @string tname = t.Extra._<@string>();
                @string xname = t.Extra._<@string>(); 
                // desire fast sorting, not pretty sorting.
                if (len(tname) == len(xname))
                {
                    if (tname == xname)
                    {
                        return CMPeq;
                    }

                    if (tname < xname)
                    {
                        return CMPlt;
                    }

                    return CMPgt;

                }

                if (len(tname) > len(xname))
                {
                    return CMPgt;
                }

                return CMPlt;
            else if (t.Etype == TTUPLE) 
                ptr<Tuple> xtup = x.Extra._<ptr<Tuple>>();
                ptr<Tuple> ttup = t.Extra._<ptr<Tuple>>();
                {
                    var c__prev1 = c;

                    c = ttup.first.Compare(xtup.first);

                    if (c != CMPeq)
                    {
                        return c;
                    }

                    c = c__prev1;

                }

                return ttup.second.Compare(xtup.second);
            else if (t.Etype == TMAP) 
                {
                    var c__prev1 = c;

                    c = t.Key().cmp(x.Key());

                    if (c != CMPeq)
                    {
                        return c;
                    }

                    c = c__prev1;

                }

                return t.Elem().cmp(x.Elem());
            else if (t.Etype == TPTR || t.Etype == TSLICE)             else if (t.Etype == TSTRUCT) 
                if (t.StructType().Map == null)
                {
                    if (x.StructType().Map != null)
                    {
                        return CMPlt; // nil < non-nil
                    } 
                    // to the fallthrough
                }
                else if (x.StructType().Map == null)
                {
                    return CMPgt; // nil > non-nil
                }
                else if (t.StructType().Map.MapType().Bucket == t)
                { 
                    // Both have non-nil Map
                    // Special case for Maps which include a recursive type where the recursion is not broken with a named type
                    if (x.StructType().Map.MapType().Bucket != x)
                    {
                        return CMPlt; // bucket maps are least
                    }

                    return t.StructType().Map.cmp(x.StructType().Map);

                }
                else if (x.StructType().Map.MapType().Bucket == x)
                {
                    return CMPgt; // bucket maps are least
                } // If t != t.Map.Bucket, fall through to general case
                var tfs = t.FieldSlice();
                var xfs = x.FieldSlice();
                {
                    long i__prev1 = i;

                    for (long i = 0L; i < len(tfs) && i < len(xfs); i++)
                    {
                        var t1 = tfs[i];
                        var x1 = xfs[i];
                        if (t1.Embedded != x1.Embedded)
                        {
                            return cmpForNe(t1.Embedded < x1.Embedded);
                        }

                        if (t1.Note != x1.Note)
                        {
                            return cmpForNe(t1.Note < x1.Note);
                        }

                        {
                            var c__prev1 = c;

                            c = t1.Sym.cmpsym(x1.Sym);

                            if (c != CMPeq)
                            {
                                return c;
                            }

                            c = c__prev1;

                        }

                        {
                            var c__prev1 = c;

                            c = t1.Type.cmp(x1.Type);

                            if (c != CMPeq)
                            {
                                return c;
                            }

                            c = c__prev1;

                        }

                    }


                    i = i__prev1;
                }
                if (len(tfs) != len(xfs))
                {
                    return cmpForNe(len(tfs) < len(xfs));
                }

                return CMPeq;
            else if (t.Etype == TINTER) 
                tfs = t.FieldSlice();
                xfs = x.FieldSlice();
                {
                    long i__prev1 = i;

                    for (i = 0L; i < len(tfs) && i < len(xfs); i++)
                    {
                        t1 = tfs[i];
                        x1 = xfs[i];
                        {
                            var c__prev1 = c;

                            c = t1.Sym.cmpsym(x1.Sym);

                            if (c != CMPeq)
                            {
                                return c;
                            }

                            c = c__prev1;

                        }

                        {
                            var c__prev1 = c;

                            c = t1.Type.cmp(x1.Type);

                            if (c != CMPeq)
                            {
                                return c;
                            }

                            c = c__prev1;

                        }

                    }


                    i = i__prev1;
                }
                if (len(tfs) != len(xfs))
                {
                    return cmpForNe(len(tfs) < len(xfs));
                }

                return CMPeq;
            else if (t.Etype == TFUNC) 
                foreach (var (_, f) in RecvsParamsResults)
                { 
                    // Loop over fields in structs, ignoring argument names.
                    tfs = f(t).FieldSlice();
                    xfs = f(x).FieldSlice();
                    {
                        long i__prev2 = i;

                        for (i = 0L; i < len(tfs) && i < len(xfs); i++)
                        {
                            var ta = tfs[i];
                            var tb = xfs[i];
                            if (ta.IsDDD() != tb.IsDDD())
                            {
                                return cmpForNe(!ta.IsDDD());
                            }

                            {
                                var c__prev1 = c;

                                c = ta.Type.cmp(tb.Type);

                                if (c != CMPeq)
                                {
                                    return c;
                                }

                                c = c__prev1;

                            }

                        }


                        i = i__prev2;
                    }
                    if (len(tfs) != len(xfs))
                    {
                        return cmpForNe(len(tfs) < len(xfs));
                    }

                }
                return CMPeq;
            else if (t.Etype == TARRAY) 
                if (t.NumElem() != x.NumElem())
                {
                    return cmpForNe(t.NumElem() < x.NumElem());
                }

            else if (t.Etype == TCHAN) 
                if (t.ChanDir() != x.ChanDir())
                {
                    return cmpForNe(t.ChanDir() < x.ChanDir());
                }

            else 
                var e = fmt.Sprintf("Do not know how to compare %v with %v", t, x);
                panic(e);
            // Common element type comparison for TARRAY, TCHAN, TPTR, and TSLICE.
            return t.Elem().cmp(x.Elem());

        });

        // IsKind reports whether t is a Type of the specified kind.
        private static bool IsKind(this ptr<Type> _addr_t, EType et)
        {
            ref Type t = ref _addr_t.val;

            return t != null && t.Etype == et;
        }

        private static bool IsBoolean(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TBOOL;
        }

        private static array<EType> unsignedEType = new array<EType>(InitKeyedValues<EType>((TINT8, TUINT8), (TUINT8, TUINT8), (TINT16, TUINT16), (TUINT16, TUINT16), (TINT32, TUINT32), (TUINT32, TUINT32), (TINT64, TUINT64), (TUINT64, TUINT64), (TINT, TUINT), (TUINT, TUINT), (TUINTPTR, TUINTPTR)));

        // ToUnsigned returns the unsigned equivalent of integer type t.
        private static ptr<Type> ToUnsigned(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            if (!t.IsInteger())
            {
                Fatalf("unsignedType(%v)", t);
            }

            return _addr_Types[unsignedEType[t.Etype]]!;

        }

        private static bool IsInteger(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;


            if (t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TINT || t.Etype == TUINT || t.Etype == TUINTPTR) 
                return true;
                        return false;

        }

        private static bool IsSigned(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;


            if (t.Etype == TINT8 || t.Etype == TINT16 || t.Etype == TINT32 || t.Etype == TINT64 || t.Etype == TINT) 
                return true;
                        return false;

        }

        private static bool IsFloat(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TFLOAT32 || t.Etype == TFLOAT64;
        }

        private static bool IsComplex(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128;
        }

        // IsPtr reports whether t is a regular Go pointer type.
        // This does not include unsafe.Pointer.
        private static bool IsPtr(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TPTR;
        }

        // IsPtrElem reports whether t is the element of a pointer (to t).
        private static bool IsPtrElem(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Cache.ptr != null;
        }

        // IsUnsafePtr reports whether t is an unsafe pointer.
        private static bool IsUnsafePtr(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TUNSAFEPTR;
        }

        // IsPtrShaped reports whether t is represented by a single machine pointer.
        // In addition to regular Go pointer types, this includes map, channel, and
        // function types and unsafe.Pointer. It does not include array or struct types
        // that consist of a single pointer shaped type.
        // TODO(mdempsky): Should it? See golang.org/issue/15028.
        private static bool IsPtrShaped(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TPTR || t.Etype == TUNSAFEPTR || t.Etype == TMAP || t.Etype == TCHAN || t.Etype == TFUNC;
        }

        // HasNil reports whether the set of values determined by t includes nil.
        private static bool HasNil(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;


            if (t.Etype == TCHAN || t.Etype == TFUNC || t.Etype == TINTER || t.Etype == TMAP || t.Etype == TPTR || t.Etype == TSLICE || t.Etype == TUNSAFEPTR) 
                return true;
                        return false;

        }

        private static bool IsString(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TSTRING;
        }

        private static bool IsMap(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TMAP;
        }

        private static bool IsChan(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TCHAN;
        }

        private static bool IsSlice(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TSLICE;
        }

        private static bool IsArray(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TARRAY;
        }

        private static bool IsStruct(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TSTRUCT;
        }

        private static bool IsInterface(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TINTER;
        }

        // IsEmptyInterface reports whether t is an empty interface type.
        private static bool IsEmptyInterface(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.IsInterface() && t.NumFields() == 0L;
        }

        private static ptr<Type> PtrTo(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return _addr_NewPtr(_addr_t)!;
        }

        private static long NumFields(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Fields().Len();
        }
        private static ptr<Type> FieldType(this ptr<Type> _addr_t, long i) => func((_, panic, __) =>
        {
            ref Type t = ref _addr_t.val;

            if (t.Etype == TTUPLE)
            {
                switch (i)
                {
                    case 0L: 
                        return t.Extra._<ptr<Tuple>>().first;
                        break;
                    case 1L: 
                        return t.Extra._<ptr<Tuple>>().second;
                        break;
                    default: 
                        panic("bad tuple index");
                        break;
                }

            }

            return _addr_t.Field(i).Type!;

        });
        private static long FieldOff(this ptr<Type> _addr_t, long i)
        {
            ref Type t = ref _addr_t.val;

            return t.Field(i).Offset;
        }
        private static @string FieldName(this ptr<Type> _addr_t, long i)
        {
            ref Type t = ref _addr_t.val;

            return t.Field(i).Sym.Name;
        }

        private static long NumElem(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TARRAY);
            return t.Extra._<ptr<Array>>().Bound;
        }

        private partial struct componentsIncludeBlankFields // : bool
        {
        }

        public static readonly componentsIncludeBlankFields IgnoreBlankFields = (componentsIncludeBlankFields)false;
        public static readonly componentsIncludeBlankFields CountBlankFields = (componentsIncludeBlankFields)true;


        // NumComponents returns the number of primitive elements that compose t.
        // Struct and array types are flattened for the purpose of counting.
        // All other types (including string, slice, and interface types) count as one element.
        // If countBlank is IgnoreBlankFields, then blank struct fields
        // (and their comprised elements) are excluded from the count.
        // struct { x, y [3]int } has six components; [10]struct{ x, y string } has twenty.
        private static long NumComponents(this ptr<Type> _addr_t, componentsIncludeBlankFields countBlank)
        {
            ref Type t = ref _addr_t.val;


            if (t.Etype == TSTRUCT) 
                if (t.IsFuncArgStruct())
                {
                    Fatalf("NumComponents func arg struct");
                }

                long n = default;
                foreach (var (_, f) in t.FieldSlice())
                {
                    if (countBlank == IgnoreBlankFields && f.Sym.IsBlank())
                    {
                        continue;
                    }

                    n += f.Type.NumComponents(countBlank);

                }
                return n;
            else if (t.Etype == TARRAY) 
                return t.NumElem() * t.Elem().NumComponents(countBlank);
                        return 1L;

        }

        // SoleComponent returns the only primitive component in t,
        // if there is exactly one. Otherwise, it returns nil.
        // Components are counted as in NumComponents, including blank fields.
        private static ptr<Type> SoleComponent(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;


            if (t.Etype == TSTRUCT) 
                if (t.IsFuncArgStruct())
                {
                    Fatalf("SoleComponent func arg struct");
                }

                if (t.NumFields() != 1L)
                {
                    return _addr_null!;
                }

                return _addr_t.Field(0L).Type.SoleComponent()!;
            else if (t.Etype == TARRAY) 
                if (t.NumElem() != 1L)
                {
                    return _addr_null!;
                }

                return _addr_t.Elem().SoleComponent()!;
                        return _addr_t!;

        }

        // ChanDir returns the direction of a channel type t.
        // The direction will be one of Crecv, Csend, or Cboth.
        private static ChanDir ChanDir(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            t.wantEtype(TCHAN);
            return t.Extra._<ptr<Chan>>().Dir;
        }

        private static bool IsMemory(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t == TypeMem || t.Etype == TTUPLE && t.Extra._<ptr<Tuple>>().second == TypeMem;
        }
        private static bool IsFlags(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t == TypeFlags;
        }
        private static bool IsVoid(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t == TypeVoid;
        }
        private static bool IsTuple(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return t.Etype == TTUPLE;
        }

        // IsUntyped reports whether t is an untyped type.
        private static bool IsUntyped(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            if (t == null)
            {
                return false;
            }

            if (t == Idealstring || t == Idealbool)
            {
                return true;
            }


            if (t.Etype == TNIL || t.Etype == TIDEAL) 
                return true;
                        return false;

        }

        // TODO(austin): We probably only need HasHeapPointer. See
        // golang.org/cl/73412 for discussion.

        public static bool Haspointers(ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return Haspointers1(_addr_t, false);
        }

        public static bool Haspointers1(ptr<Type> _addr_t, bool ignoreNotInHeap)
        {
            ref Type t = ref _addr_t.val;


            if (t.Etype == TINT || t.Etype == TUINT || t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TUINTPTR || t.Etype == TFLOAT32 || t.Etype == TFLOAT64 || t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128 || t.Etype == TBOOL || t.Etype == TSSA) 
                return false;
            else if (t.Etype == TARRAY) 
                if (t.NumElem() == 0L)
                { // empty array has no pointers
                    return false;

                }

                return Haspointers1(_addr_t.Elem(), ignoreNotInHeap);
            else if (t.Etype == TSTRUCT) 
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    if (Haspointers1(_addr_t1.Type, ignoreNotInHeap))
                    {
                        return true;
                    }

                }
                return false;
            else if (t.Etype == TPTR || t.Etype == TSLICE) 
                return !(ignoreNotInHeap && t.Elem().NotInHeap());
            else if (t.Etype == TTUPLE) 
                ptr<Tuple> ttup = t.Extra._<ptr<Tuple>>();
                return Haspointers1(_addr_ttup.first, ignoreNotInHeap) || Haspointers1(_addr_ttup.second, ignoreNotInHeap);
                        return true;

        }

        // HasHeapPointer reports whether t contains a heap pointer.
        // This is used for write barrier insertion, so it ignores
        // pointers to go:notinheap types.
        private static bool HasHeapPointer(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return Haspointers1(_addr_t, true);
        }

        private static ptr<obj.LSym> Symbol(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            return _addr_TypeLinkSym(t)!;
        }

        // Tie returns 'T' if t is a concrete type,
        // 'I' if t is an interface type, and 'E' if t is an empty interface type.
        // It is used to build calls to the conv* and assert* runtime routines.
        private static byte Tie(this ptr<Type> _addr_t)
        {
            ref Type t = ref _addr_t.val;

            if (t.IsEmptyInterface())
            {
                return 'E';
            }

            if (t.IsInterface())
            {
                return 'I';
            }

            return 'T';

        }

        private static ptr<Type> recvType;

        // FakeRecvType returns the singleton type used for interface method receivers.
        public static ptr<Type> FakeRecvType()
        {
            if (recvType == null)
            {
                recvType = NewPtr(_addr_New(TSTRUCT));
            }

            return _addr_recvType!;

        }

 
        // TSSA types. Haspointers assumes these are pointer-free.
        public static var TypeInvalid = newSSA("invalid");        public static var TypeMem = newSSA("mem");        public static var TypeFlags = newSSA("flags");        public static var TypeVoid = newSSA("void");        public static var TypeInt128 = newSSA("int128");
    }
}}}}
