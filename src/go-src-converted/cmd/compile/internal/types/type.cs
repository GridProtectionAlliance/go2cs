// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 August 29 08:53:18 UTC
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

        public static readonly EType Txxx = iota;

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

        public static readonly var TPTR32 = 16;
        public static readonly var TPTR64 = 17;

        public static readonly var TFUNC = 18;
        public static readonly var TSLICE = 19;
        public static readonly var TARRAY = 20;
        public static readonly var TSTRUCT = 21;
        public static readonly var TCHAN = 22;
        public static readonly var TMAP = 23;
        public static readonly var TINTER = 24;
        public static readonly var TFORW = 25;
        public static readonly var TANY = 26;
        public static readonly var TSTRING = 27;
        public static readonly var TUNSAFEPTR = 28; 

        // pseudo-types for literals
        public static readonly var TIDEAL = 29;
        public static readonly var TNIL = 30;
        public static readonly var TBLANK = 31; 

        // pseudo-types for frame layout
        public static readonly var TFUNCARGS = 32;
        public static readonly var TCHANARGS = 33; 

        // pseudo-types for import/export
        public static readonly var TDDDFIELD = 34; // wrapper: contained type is a ... field

        // SSA backend types
        public static readonly var TSSA = 35; // internal types used by SSA backend (flags, memory, etc.)
        public static readonly var TTUPLE = 36; // a pair of types, used by SSA backend

        public static readonly var NTYPE = 37;

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
        public static readonly ChanDir Crecv = 1L << (int)(0L);
        public static readonly ChanDir Csend = 1L << (int)(1L);
        public static readonly ChanDir Cboth = Crecv | Csend;

        // Types stores pointers to predeclared named types.
        //
        // It also stores pointers to several special types:
        //   - Types[TANY] is the placeholder "any" type recognized by substArgTypes.
        //   - Types[TBLANK] represents the blank variable's type.
        //   - Types[TIDEAL] represents untyped numeric constants.
        //   - Types[TNIL] represents the predeclared "nil" value's type.
        //   - Types[TUNSAFEPTR] is package unsafe's Pointer type.
        public static array<ref Type> Types = new array<ref Type>(NTYPE);

 
        // Predeclared alias types. Kept separate for better error messages.
        public static ref Type Bytetype = default;        public static ref Type Runetype = default;        public static ref Type Errortype = default;        public static ref Type Idealstring = default;        public static ref Type Idealbool = default;        public static var Idealint = New(TIDEAL);        public static var Idealrune = New(TIDEAL);        public static var Idealfloat = New(TIDEAL);        public static var Idealcomplex = New(TIDEAL);

        // A Type represents a Go type.
        public partial struct Type
        {
            public long Width;
            public Fields methods;
            public Fields allMethods;
            public ptr<Node> Nod; // canonical OTYPE node
            public ptr<Type> Orig; // original type (type literal or predefined type)

            public ptr<Type> SliceOf;
            public ptr<Type> PtrBase;
            public ptr<Sym> Sym; // symbol containing name, for named types
            public int Vargen; // unique name for OTYPE/ONAME

            public EType Etype; // kind of type
            public byte Align; // the required alignment of this type, in bytes

            public bitset8 flags;
        }

        private static readonly long typeNotInHeap = 1L << (int)(iota); // type cannot be heap allocated
        private static readonly var typeBroke = 0; // broken type definition
        private static readonly var typeNoalg = 1; // suppress hash and eq algorithm generation
        private static readonly var typeDeferwidth = 2;
        private static readonly var typeRecur = 3;

        private static bool NotInHeap(this ref Type t)
        {
            return t.flags & typeNotInHeap != 0L;
        }
        private static bool Broke(this ref Type t)
        {
            return t.flags & typeBroke != 0L;
        }
        private static bool Noalg(this ref Type t)
        {
            return t.flags & typeNoalg != 0L;
        }
        private static bool Deferwidth(this ref Type t)
        {
            return t.flags & typeDeferwidth != 0L;
        }
        private static bool Recur(this ref Type t)
        {
            return t.flags & typeRecur != 0L;
        }

        private static void SetNotInHeap(this ref Type t, bool b)
        {
            t.flags.set(typeNotInHeap, b);

        }
        private static void SetBroke(this ref Type t, bool b)
        {
            t.flags.set(typeBroke, b);

        }
        private static void SetNoalg(this ref Type t, bool b)
        {
            t.flags.set(typeNoalg, b);

        }
        private static void SetDeferwidth(this ref Type t, bool b)
        {
            t.flags.set(typeDeferwidth, b);

        }
        private static void SetRecur(this ref Type t, bool b)
        {
            t.flags.set(typeRecur, b);

        }

        // Map contains Type fields specific to maps.
        public partial struct Map
        {
            public ptr<Type> Key; // Key type
            public ptr<Type> Val; // Val (elem) type

            public ptr<Type> Bucket; // internal struct type representing a hash bucket
            public ptr<Type> Hmap; // internal struct type representing the Hmap (map header object)
            public ptr<Type> Hiter; // internal struct type representing hash iterator state
        }

        // MapType returns t's extra map-specific fields.
        private static ref Map MapType(this ref Type t)
        {
            t.wantEtype(TMAP);
            return t.Extra._<ref Map>();
        }

        // Forward contains Type fields specific to forward types.
        public partial struct Forward
        {
            public slice<ref Node> Copyto; // where to copy the eventual value to
            public src.XPos Embedlineno; // first use of this type as an embedded type
        }

        // ForwardType returns t's extra forward-type-specific fields.
        private static ref Forward ForwardType(this ref Type t)
        {
            t.wantEtype(TFORW);
            return t.Extra._<ref Forward>();
        }

        // Func contains Type fields specific to func types.
        public partial struct Func
        {
            public ptr<Type> Receiver; // function receiver
            public ptr<Type> Results; // function results
            public ptr<Type> Params; // function params

            public ptr<Node> Nname; // Argwid is the total width of the function receiver, params, and results.
// It gets calculated via a temporary TFUNCARGS type.
// Note that TFUNC's Width is Widthptr.
            public long Argwid;
            public bool Outnamed;
        }

        // FuncType returns t's extra func-specific fields.
        private static ref Func FuncType(this ref Type t)
        {
            t.wantEtype(TFUNC);
            return t.Extra._<ref Func>();
        }

        // StructType contains Type fields specific to struct types.
        public partial struct Struct
        {
            public Fields fields; // Maps have three associated internal structs (see struct MapType).
// Map links such structs back to their map type.
            public ptr<Type> Map;
            public Funarg Funarg; // type of function arguments for arg struct
        }

        // Fnstruct records the kind of function argument
        public partial struct Funarg // : byte
        {
        }

        public static readonly Funarg FunargNone = iota;
        public static readonly var FunargRcvr = 0; // receiver
        public static readonly var FunargParams = 1; // input parameters
        public static readonly var FunargResults = 2; // output results

        // StructType returns t's extra struct-specific fields.
        private static ref Struct StructType(this ref Type t)
        {
            t.wantEtype(TSTRUCT);
            return t.Extra._<ref Struct>();
        }

        // Interface contains Type fields specific to interface types.
        public partial struct Interface
        {
            public Fields Fields;
        }

        // Ptr contains Type fields specific to pointer types.
        public partial struct Ptr
        {
            public ptr<Type> Elem; // element type
        }

        // DDDField contains Type fields specific to TDDDFIELD types.
        public partial struct DDDField
        {
            public ptr<Type> T; // reference to a slice type for ... args
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
        private static ref Chan ChanType(this ref Type t)
        {
            t.wantEtype(TCHAN);
            return t.Extra._<ref Chan>();
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
            public Funarg Funarg;
            public ptr<Sym> Sym;
            public ptr<Node> Nname;
            public ptr<Type> Type; // field type

// Offset in bytes of this field or method within its enclosing struct
// or interface Type.
            public long Offset;
            public @string Note; // literal string annotation
        }

        private static readonly long fieldIsddd = 1L << (int)(iota); // field is ... argument
        private static readonly var fieldBroke = 0; // broken field definition
        private static readonly var fieldNointerface = 1;

        private static bool Isddd(this ref Field f)
        {
            return f.flags & fieldIsddd != 0L;
        }
        private static bool Broke(this ref Field f)
        {
            return f.flags & fieldBroke != 0L;
        }
        private static bool Nointerface(this ref Field f)
        {
            return f.flags & fieldNointerface != 0L;
        }

        private static void SetIsddd(this ref Field f, bool b)
        {
            f.flags.set(fieldIsddd, b);

        }
        private static void SetBroke(this ref Field f, bool b)
        {
            f.flags.set(fieldBroke, b);

        }
        private static void SetNointerface(this ref Field f, bool b)
        {
            f.flags.set(fieldNointerface, b);

        }

        // End returns the offset of the first byte immediately after this field.
        private static long End(this ref Field f)
        {
            return f.Offset + f.Type.Width;
        }

        // Fields is a pointer to a slice of *Field.
        // This saves space in Types that do not have fields or methods
        // compared to a simple slice of *Field.
        public partial struct Fields
        {
            public ref slice<ref Field> s;
        }

        // Len returns the number of entries in f.
        private static long Len(this ref Fields f)
        {
            if (f.s == null)
            {
                return 0L;
            }
            return len(f.s.Value);
        }

        // Slice returns the entries in f as a slice.
        // Changes to the slice entries will be reflected in f.
        private static slice<ref Field> Slice(this ref Fields f)
        {
            if (f.s == null)
            {
                return null;
            }
            return f.s.Value;
        }

        // Index returns the i'th element of Fields.
        // It panics if f does not have at least i+1 elements.
        private static ref Field Index(this ref Fields f, long i)
        {
            return (f.s.Value)[i];
        }

        // Set sets f to a slice.
        // This takes ownership of the slice.
        private static void Set(this ref Fields f, slice<ref Field> s)
        {
            if (len(s) == 0L)
            {
                f.s = null;
            }
            else
            { 
                // Copy s and take address of t rather than s to avoid
                // allocation in the case where len(s) == 0.
                var t = s;
                f.s = ref t;
            }
        }

        // Append appends entries to f.
        private static void Append(this ref Fields f, params ptr<Field>[] s)
        {
            if (f.s == null)
            {
                f.s = @new<slice<ref Field>>();
            }
            f.s.Value = append(f.s.Value, s);
        }

        // New returns a new Type of the specified kind.
        public static ref Type New(EType et)
        {
            Type t = ref new Type(Etype:et,Width:BADWIDTH,);
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
            else if (t.Etype == TPTR32 || t.Etype == TPTR64) 
                t.Extra = new Ptr();
            else if (t.Etype == TCHANARGS) 
                t.Extra = new ChanArgs();
            else if (t.Etype == TFUNCARGS) 
                t.Extra = new FuncArgs();
            else if (t.Etype == TDDDFIELD) 
                t.Extra = new DDDField();
            else if (t.Etype == TCHAN) 
                t.Extra = @new<Chan>();
            else if (t.Etype == TTUPLE) 
                t.Extra = @new<Tuple>();
                        return t;
        }

        // NewArray returns a new fixed-length array Type.
        public static ref Type NewArray(ref Type elem, long bound)
        {
            if (bound < 0L)
            {
                Fatalf("NewArray: invalid bound %v", bound);
            }
            var t = New(TARRAY);
            t.Extra = ref new Array(Elem:elem,Bound:bound);
            t.SetNotInHeap(elem.NotInHeap());
            return t;
        }

        // NewSlice returns the slice Type with element type elem.
        public static ref Type NewSlice(ref Type elem)
        {
            {
                var t__prev1 = t;

                var t = elem.SliceOf;

                if (t != null)
                {
                    if (t.Elem() != elem)
                    {
                        Fatalf("elem mismatch");
                    }
                    return t;
                }

                t = t__prev1;

            }

            t = New(TSLICE);
            t.Extra = new Slice(Elem:elem);
            elem.SliceOf = t;
            return t;
        }

        // NewDDDArray returns a new [...]T array Type.
        public static ref Type NewDDDArray(ref Type elem)
        {
            var t = New(TARRAY);
            t.Extra = ref new Array(Elem:elem,Bound:-1);
            t.SetNotInHeap(elem.NotInHeap());
            return t;
        }

        // NewChan returns a new chan Type with direction dir.
        public static ref Type NewChan(ref Type elem, ChanDir dir)
        {
            var t = New(TCHAN);
            var ct = t.ChanType();
            ct.Elem = elem;
            ct.Dir = dir;
            return t;
        }

        public static ref Type NewTuple(ref Type t1, ref Type t2)
        {
            var t = New(TTUPLE);
            t.Extra._<ref Tuple>().first = t1;
            t.Extra._<ref Tuple>().second = t2;
            return t;
        }

        private static ref Type newSSA(@string name)
        {
            var t = New(TSSA);
            t.Extra = name;
            return t;
        }

        // NewMap returns a new map Type with key type k and element (aka value) type v.
        public static ref Type NewMap(ref Type k, ref Type v)
        {
            var t = New(TMAP);
            var mt = t.MapType();
            mt.Key = k;
            mt.Val = v;
            return t;
        }

        // NewPtrCacheEnabled controls whether *T Types are cached in T.
        // Caching is disabled just before starting the backend.
        // This allows the backend to run concurrently.
        public static var NewPtrCacheEnabled = true;

        // NewPtr returns the pointer type pointing to t.
        public static ref Type NewPtr(ref Type elem)
        {
            if (elem == null)
            {
                Fatalf("NewPtr: pointer to elem Type is nil");
            }
            {
                var t__prev1 = t;

                var t = elem.PtrBase;

                if (t != null)
                {
                    if (t.Elem() != elem)
                    {
                        Fatalf("NewPtr: elem mismatch");
                    }
                    return t;
                }

                t = t__prev1;

            }

            if (Tptr == 0L)
            {
                Fatalf("NewPtr: Tptr not initialized");
            }
            t = New(Tptr);
            t.Extra = new Ptr(Elem:elem);
            t.Width = int64(Widthptr);
            t.Align = uint8(Widthptr);
            if (NewPtrCacheEnabled)
            {
                elem.PtrBase = t;
            }
            return t;
        }

        // NewDDDField returns a new TDDDFIELD type for slice type s.
        public static ref Type NewDDDField(ref Type s)
        {
            var t = New(TDDDFIELD);
            t.Extra = new DDDField(T:s);
            return t;
        }

        // NewChanArgs returns a new TCHANARGS type for channel type c.
        public static ref Type NewChanArgs(ref Type c)
        {
            var t = New(TCHANARGS);
            t.Extra = new ChanArgs(T:c);
            return t;
        }

        // NewFuncArgs returns a new TFUNCARGS type for func type f.
        public static ref Type NewFuncArgs(ref Type f)
        {
            var t = New(TFUNCARGS);
            t.Extra = new FuncArgs(T:f);
            return t;
        }

        public static ref Field NewField()
        {
            return ref new Field(Offset:BADWIDTH,);
        }

        // SubstAny walks t, replacing instances of "any" with successive
        // elements removed from types.  It returns the substituted type.
        public static ref Type SubstAny(ref Type t, ref slice<ref Type> types)
        {
            if (t == null)
            {
                return null;
            }

            if (t.Etype == TANY) 
                if (len(types.Value) == 0L)
                {
                    Fatalf("substArgTypes: not enough argument types");
                }
                t = (types.Value)[0L];
                types.Value = (types.Value)[1L..];
            else if (t.Etype == TPTR32 || t.Etype == TPTR64) 
                var elem = SubstAny(t.Elem(), types);
                if (elem != t.Elem())
                {
                    t = t.copy();
                    t.Extra = new Ptr(Elem:elem);
                }
            else if (t.Etype == TARRAY) 
                elem = SubstAny(t.Elem(), types);
                if (elem != t.Elem())
                {
                    t = t.copy();
                    t.Extra._<ref Array>().Elem = elem;
                }
            else if (t.Etype == TSLICE) 
                elem = SubstAny(t.Elem(), types);
                if (elem != t.Elem())
                {
                    t = t.copy();
                    t.Extra = new Slice(Elem:elem);
                }
            else if (t.Etype == TCHAN) 
                elem = SubstAny(t.Elem(), types);
                if (elem != t.Elem())
                {
                    t = t.copy();
                    t.Extra._<ref Chan>().Elem = elem;
                }
            else if (t.Etype == TMAP) 
                var key = SubstAny(t.Key(), types);
                var val = SubstAny(t.Val(), types);
                if (key != t.Key() || val != t.Val())
                {
                    t = t.copy();
                    t.Extra._<ref Map>().Key = key;
                    t.Extra._<ref Map>().Val = val;
                }
            else if (t.Etype == TFUNC) 
                var recvs = SubstAny(t.Recvs(), types);
                var @params = SubstAny(t.Params(), types);
                var results = SubstAny(t.Results(), types);
                if (recvs != t.Recvs() || params != t.Params() || results != t.Results())
                {
                    t = t.copy();
                    t.FuncType().Receiver = recvs;
                    t.FuncType().Results = results;
                    t.FuncType().Params = params;
                }
            else if (t.Etype == TSTRUCT) 
                var fields = t.FieldSlice();
                slice<ref Field> nfs = default;
                foreach (var (i, f) in fields)
                {
                    var nft = SubstAny(f.Type, types);
                    if (nft == f.Type)
                    {
                        continue;
                    }
                    if (nfs == null)
                    {
                        nfs = append((slice<ref Field>)null, fields);
                    }
                    nfs[i] = f.Copy();
                    nfs[i].Type = nft;
                }
                if (nfs != null)
                {
                    t = t.copy();
                    t.SetFields(nfs);
                }
            else                         return t;
        }

        // copy returns a shallow copy of the Type.
        private static ref Type copy(this ref Type t)
        {
            if (t == null)
            {
                return null;
            }
            var nt = t.Value; 
            // copy any *T Extra fields, to avoid aliasing

            if (t.Etype == TMAP) 
                ref Map x = t.Extra._<ref Map>().Value;
                nt.Extra = ref x;
            else if (t.Etype == TFORW) 
                x = t.Extra._<ref Forward>().Value;
                nt.Extra = ref x;
            else if (t.Etype == TFUNC) 
                x = t.Extra._<ref Func>().Value;
                nt.Extra = ref x;
            else if (t.Etype == TSTRUCT) 
                x = t.Extra._<ref Struct>().Value;
                nt.Extra = ref x;
            else if (t.Etype == TINTER) 
                x = t.Extra._<ref Interface>().Value;
                nt.Extra = ref x;
            else if (t.Etype == TCHAN) 
                x = t.Extra._<ref Chan>().Value;
                nt.Extra = ref x;
            else if (t.Etype == TARRAY) 
                x = t.Extra._<ref Array>().Value;
                nt.Extra = ref x;
            else if (t.Etype == TTUPLE || t.Etype == TSSA) 
                Fatalf("ssa types cannot be copied");
            // TODO(mdempsky): Find out why this is necessary and explain.
            if (t.Orig == t)
            {
                nt.Orig = ref nt;
            }
            return ref nt;
        }

        private static ref Field Copy(this ref Field f)
        {
            var nf = f.Value;
            return ref nf;
        }

        private static void wantEtype(this ref Type t, EType et)
        {
            if (t.Etype != et)
            {
                Fatalf("want %v, but have %v", et, t);
            }
        }

        private static ref Type Recvs(this ref Type t)
        {
            return t.FuncType().Receiver;
        }
        private static ref Type Params(this ref Type t)
        {
            return t.FuncType().Params;
        }
        private static ref Type Results(this ref Type t)
        {
            return t.FuncType().Results;
        }

        private static long NumRecvs(this ref Type t)
        {
            return t.FuncType().Receiver.NumFields();
        }
        private static long NumParams(this ref Type t)
        {
            return t.FuncType().Params.NumFields();
        }
        private static long NumResults(this ref Type t)
        {
            return t.FuncType().Results.NumFields();
        }

        // Recv returns the receiver of function type t, if any.
        private static ref Field Recv(this ref Type t)
        {
            var s = t.Recvs();
            if (s.NumFields() == 0L)
            {
                return null;
            }
            return s.Field(0L);
        }

        // RecvsParamsResults stores the accessor functions for a function Type's
        // receiver, parameters, and result parameters, in that order.
        // It can be used to iterate over all of a function's parameter lists.
        public static array<Func<ref Type, ref Type>> RecvsParamsResults = new array<Func<ref Type, ref Type>>(new Func<ref Type, ref Type>[] { (*Type).Recvs, (*Type).Params, (*Type).Results });

        // ParamsResults is like RecvsParamsResults, but omits receiver parameters.
        public static array<Func<ref Type, ref Type>> ParamsResults = new array<Func<ref Type, ref Type>>(new Func<ref Type, ref Type>[] { (*Type).Params, (*Type).Results });

        // Key returns the key type of map type t.
        private static ref Type Key(this ref Type t)
        {
            t.wantEtype(TMAP);
            return t.Extra._<ref Map>().Key;
        }

        // Val returns the value type of map type t.
        private static ref Type Val(this ref Type t)
        {
            t.wantEtype(TMAP);
            return t.Extra._<ref Map>().Val;
        }

        // Elem returns the type of elements of t.
        // Usable with pointers, channels, arrays, and slices.
        private static ref Type Elem(this ref Type t)
        {

            if (t.Etype == TPTR32 || t.Etype == TPTR64) 
                return t.Extra._<Ptr>().Elem;
            else if (t.Etype == TARRAY) 
                return t.Extra._<ref Array>().Elem;
            else if (t.Etype == TSLICE) 
                return t.Extra._<Slice>().Elem;
            else if (t.Etype == TCHAN) 
                return t.Extra._<ref Chan>().Elem;
                        Fatalf("Type.Elem %s", t.Etype);
            return null;
        }

        // DDDField returns the slice ... type for TDDDFIELD type t.
        private static ref Type DDDField(this ref Type t)
        {
            t.wantEtype(TDDDFIELD);
            return t.Extra._<DDDField>().T;
        }

        // ChanArgs returns the channel type for TCHANARGS type t.
        private static ref Type ChanArgs(this ref Type t)
        {
            t.wantEtype(TCHANARGS);
            return t.Extra._<ChanArgs>().T;
        }

        // FuncArgs returns the channel type for TFUNCARGS type t.
        private static ref Type FuncArgs(this ref Type t)
        {
            t.wantEtype(TFUNCARGS);
            return t.Extra._<FuncArgs>().T;
        }

        // Nname returns the associated function's nname.
        private static ref Node Nname(this ref Type t)
        {

            if (t.Etype == TFUNC) 
                return t.Extra._<ref Func>().Nname;
                        Fatalf("Type.Nname %v %v", t.Etype, t);
            return null;
        }

        // Nname sets the associated function's nname.
        private static void SetNname(this ref Type t, ref Node n)
        {

            if (t.Etype == TFUNC) 
                t.Extra._<ref Func>().Nname = n;
            else 
                Fatalf("Type.SetNname %v %v", t.Etype, t);
                    }

        // IsFuncArgStruct reports whether t is a struct representing function parameters.
        private static bool IsFuncArgStruct(this ref Type t)
        {
            return t.Etype == TSTRUCT && t.Extra._<ref Struct>().Funarg != FunargNone;
        }

        private static ref Fields Methods(this ref Type t)
        { 
            // TODO(mdempsky): Validate t?
            return ref t.methods;
        }

        private static ref Fields AllMethods(this ref Type t)
        { 
            // TODO(mdempsky): Validate t?
            return ref t.allMethods;
        }

        private static ref Fields Fields(this ref Type t)
        {

            if (t.Etype == TSTRUCT) 
                return ref t.Extra._<ref Struct>().fields;
            else if (t.Etype == TINTER) 
                Dowidth(t);
                return ref t.Extra._<ref Interface>().Fields;
                        Fatalf("Fields: type %v does not have fields", t);
            return null;
        }

        // Field returns the i'th field/method of struct/interface type t.
        private static ref Field Field(this ref Type t, long i)
        {
            return t.Fields().Slice()[i];
        }

        // FieldSlice returns a slice of containing all fields/methods of
        // struct/interface type t.
        private static slice<ref Field> FieldSlice(this ref Type t)
        {
            return t.Fields().Slice();
        }

        // SetFields sets struct/interface type t's fields/methods to fields.
        private static void SetFields(this ref Type t, slice<ref Field> fields)
        { 
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

        private static void SetInterface(this ref Type t, slice<ref Field> methods)
        {
            t.wantEtype(TINTER);
            t.Methods().Set(methods);
        }

        private static bool IsDDDArray(this ref Type t)
        {
            if (t.Etype != TARRAY)
            {
                return false;
            }
            return t.Extra._<ref Array>().Bound < 0L;
        }

        private static bool WidthCalculated(this ref Type t)
        {
            return t.Align > 0L;
        }

        // ArgWidth returns the total aligned argument size for a function.
        // It includes the receiver, parameters, and results.
        private static long ArgWidth(this ref Type t)
        {
            t.wantEtype(TFUNC);
            return t.Extra._<ref Func>().Argwid;
        }

        private static long Size(this ref Type t)
        {
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

        private static long Alignment(this ref Type t)
        {
            Dowidth(t);
            return int64(t.Align);
        }

        private static @string SimpleString(this ref Type t)
        {
            return t.Etype.String();
        }

        // Cmp is a comparison between values a and b.
        // -1 if a < b
        //  0 if a == b
        //  1 if a > b
        public partial struct Cmp // : sbyte
        {
        }

        public static readonly var CMPlt = Cmp(-1L);
        public static readonly var CMPeq = Cmp(0L);
        public static readonly var CMPgt = Cmp(1L);

        // Compare compares types for purposes of the SSA back
        // end, returning a Cmp (one of CMPlt, CMPeq, CMPgt).
        // The answers are correct for an optimizer
        // or code generator, but not necessarily typechecking.
        // The order chosen is arbitrary, only consistency and division
        // into equivalence classes (Types that compare CMPeq) matters.
        private static Cmp Compare(this ref Type t, ref Type x)
        {
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

        private static Cmp cmpsym(this ref Sym r, ref Sym s)
        {
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
        private static Cmp cmp(this ref Type _t, ref Type _x) => func(_t, _x, (ref Type t, ref Type x, Defer _, Panic panic, Recover __) =>
        { 
            // This follows the structure of eqtype in subr.go
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
                ref Tuple xtup = x.Extra._<ref Tuple>();
                ref Tuple ttup = t.Extra._<ref Tuple>();
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
                return t.Val().cmp(x.Val());
            else if (t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TSLICE)             else if (t.Etype == TSTRUCT) 
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
                            if (ta.Isddd() != tb.Isddd())
                            {
                                return cmpForNe(!ta.Isddd());
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
            // Common element type comparison for TARRAY, TCHAN, TPTR32, TPTR64, and TSLICE.
            return t.Elem().cmp(x.Elem());
        });

        // IsKind reports whether t is a Type of the specified kind.
        private static bool IsKind(this ref Type t, EType et)
        {
            return t != null && t.Etype == et;
        }

        private static bool IsBoolean(this ref Type t)
        {
            return t.Etype == TBOOL;
        }

        private static array<EType> unsignedEType = new array<EType>(InitKeyedValues<EType>((TINT8, TUINT8), (TUINT8, TUINT8), (TINT16, TUINT16), (TUINT16, TUINT16), (TINT32, TUINT32), (TUINT32, TUINT32), (TINT64, TUINT64), (TUINT64, TUINT64), (TINT, TUINT), (TUINT, TUINT), (TUINTPTR, TUINTPTR)));

        // ToUnsigned returns the unsigned equivalent of integer type t.
        private static ref Type ToUnsigned(this ref Type t)
        {
            if (!t.IsInteger())
            {
                Fatalf("unsignedType(%v)", t);
            }
            return Types[unsignedEType[t.Etype]];
        }

        private static bool IsInteger(this ref Type t)
        {

            if (t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TINT || t.Etype == TUINT || t.Etype == TUINTPTR) 
                return true;
                        return false;
        }

        private static bool IsSigned(this ref Type t)
        {

            if (t.Etype == TINT8 || t.Etype == TINT16 || t.Etype == TINT32 || t.Etype == TINT64 || t.Etype == TINT) 
                return true;
                        return false;
        }

        private static bool IsFloat(this ref Type t)
        {
            return t.Etype == TFLOAT32 || t.Etype == TFLOAT64;
        }

        private static bool IsComplex(this ref Type t)
        {
            return t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128;
        }

        // IsPtr reports whether t is a regular Go pointer type.
        // This does not include unsafe.Pointer.
        private static bool IsPtr(this ref Type t)
        {
            return t.Etype == TPTR32 || t.Etype == TPTR64;
        }

        // IsUnsafePtr reports whether t is an unsafe pointer.
        private static bool IsUnsafePtr(this ref Type t)
        {
            return t.Etype == TUNSAFEPTR;
        }

        // IsPtrShaped reports whether t is represented by a single machine pointer.
        // In addition to regular Go pointer types, this includes map, channel, and
        // function types and unsafe.Pointer. It does not include array or struct types
        // that consist of a single pointer shaped type.
        // TODO(mdempsky): Should it? See golang.org/issue/15028.
        private static bool IsPtrShaped(this ref Type t)
        {
            return t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TUNSAFEPTR || t.Etype == TMAP || t.Etype == TCHAN || t.Etype == TFUNC;
        }

        private static bool IsString(this ref Type t)
        {
            return t.Etype == TSTRING;
        }

        private static bool IsMap(this ref Type t)
        {
            return t.Etype == TMAP;
        }

        private static bool IsChan(this ref Type t)
        {
            return t.Etype == TCHAN;
        }

        private static bool IsSlice(this ref Type t)
        {
            return t.Etype == TSLICE;
        }

        private static bool IsArray(this ref Type t)
        {
            return t.Etype == TARRAY;
        }

        private static bool IsStruct(this ref Type t)
        {
            return t.Etype == TSTRUCT;
        }

        private static bool IsInterface(this ref Type t)
        {
            return t.Etype == TINTER;
        }

        // IsEmptyInterface reports whether t is an empty interface type.
        private static bool IsEmptyInterface(this ref Type t)
        {
            return t.IsInterface() && t.NumFields() == 0L;
        }

        private static ref Type ElemType(this ref Type t)
        { 
            // TODO(josharian): If Type ever moves to a shared
            // internal package, remove this silly wrapper.
            return t.Elem();
        }
        private static ref Type PtrTo(this ref Type t)
        {
            return NewPtr(t);
        }

        private static long NumFields(this ref Type t)
        {
            return t.Fields().Len();
        }
        private static ref Type FieldType(this ref Type _t, long i) => func(_t, (ref Type t, Defer _, Panic panic, Recover __) =>
        {
            if (t.Etype == TTUPLE)
            {
                switch (i)
                {
                    case 0L: 
                        return t.Extra._<ref Tuple>().first;
                        break;
                    case 1L: 
                        return t.Extra._<ref Tuple>().second;
                        break;
                    default: 
                        panic("bad tuple index");
                        break;
                }
            }
            return t.Field(i).Type;
        });
        private static long FieldOff(this ref Type t, long i)
        {
            return t.Field(i).Offset;
        }
        private static @string FieldName(this ref Type t, long i)
        {
            return t.Field(i).Sym.Name;
        }

        private static long NumElem(this ref Type t)
        {
            t.wantEtype(TARRAY);
            ref Array at = t.Extra._<ref Array>();
            if (at.Bound < 0L)
            {
                Fatalf("NumElem array %v does not have bound yet", t);
            }
            return at.Bound;
        }

        // SetNumElem sets the number of elements in an array type.
        // The only allowed use is on array types created with NewDDDArray.
        // For other uses, create a new array with NewArray instead.
        private static void SetNumElem(this ref Type t, long n)
        {
            t.wantEtype(TARRAY);
            ref Array at = t.Extra._<ref Array>();
            if (at.Bound >= 0L)
            {
                Fatalf("SetNumElem array %v already has bound %d", t, at.Bound);
            }
            at.Bound = n;
        }

        private static long NumComponents(this ref Type t)
        {

            if (t.Etype == TSTRUCT) 
                if (t.IsFuncArgStruct())
                {
                    Fatalf("NumComponents func arg struct");
                }
                long n = default;
                foreach (var (_, f) in t.FieldSlice())
                {
                    n += f.Type.NumComponents();
                }
                return n;
            else if (t.Etype == TARRAY) 
                return t.NumElem() * t.Elem().NumComponents();
                        return 1L;
        }

        // ChanDir returns the direction of a channel type t.
        // The direction will be one of Crecv, Csend, or Cboth.
        private static ChanDir ChanDir(this ref Type t)
        {
            t.wantEtype(TCHAN);
            return t.Extra._<ref Chan>().Dir;
        }

        private static bool IsMemory(this ref Type t)
        {
            return t == TypeMem || t.Etype == TTUPLE && t.Extra._<ref Tuple>().second == TypeMem;
        }
        private static bool IsFlags(this ref Type t)
        {
            return t == TypeFlags;
        }
        private static bool IsVoid(this ref Type t)
        {
            return t == TypeVoid;
        }
        private static bool IsTuple(this ref Type t)
        {
            return t.Etype == TTUPLE;
        }

        // IsUntyped reports whether t is an untyped type.
        private static bool IsUntyped(this ref Type t)
        {
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

        public static bool Haspointers(ref Type t)
        {
            return Haspointers1(t, false);
        }

        public static bool Haspointers1(ref Type t, bool ignoreNotInHeap)
        {

            if (t.Etype == TINT || t.Etype == TUINT || t.Etype == TINT8 || t.Etype == TUINT8 || t.Etype == TINT16 || t.Etype == TUINT16 || t.Etype == TINT32 || t.Etype == TUINT32 || t.Etype == TINT64 || t.Etype == TUINT64 || t.Etype == TUINTPTR || t.Etype == TFLOAT32 || t.Etype == TFLOAT64 || t.Etype == TCOMPLEX64 || t.Etype == TCOMPLEX128 || t.Etype == TBOOL) 
                return false;
            else if (t.Etype == TARRAY) 
                if (t.NumElem() == 0L)
                { // empty array has no pointers
                    return false;
                }
                return Haspointers1(t.Elem(), ignoreNotInHeap);
            else if (t.Etype == TSTRUCT) 
                foreach (var (_, t1) in t.Fields().Slice())
                {
                    if (Haspointers1(t1.Type, ignoreNotInHeap))
                    {
                        return true;
                    }
                }
                return false;
            else if (t.Etype == TPTR32 || t.Etype == TPTR64 || t.Etype == TSLICE) 
                return !(ignoreNotInHeap && t.Elem().NotInHeap());
                        return true;
        }

        // HasHeapPointer returns whether t contains a heap pointer.
        // This is used for write barrier insertion, so it ignores
        // pointers to go:notinheap types.
        private static bool HasHeapPointer(this ref Type t)
        {
            return Haspointers1(t, true);
        }

        private static ref obj.LSym Symbol(this ref Type t)
        {
            return TypeLinkSym(t);
        }

        // Tie returns 'T' if t is a concrete type,
        // 'I' if t is an interface type, and 'E' if t is an empty interface type.
        // It is used to build calls to the conv* and assert* runtime routines.
        private static byte Tie(this ref Type t)
        {
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

        private static ref Type recvType = default;

        // FakeRecvType returns the singleton type used for interface method receivers.
        public static ref Type FakeRecvType()
        {
            if (recvType == null)
            {
                recvType = NewPtr(New(TSTRUCT));
            }
            return recvType;
        }

        public static var TypeInvalid = newSSA("invalid");        public static var TypeMem = newSSA("mem");        public static var TypeFlags = newSSA("flags");        public static var TypeVoid = newSSA("void");        public static var TypeInt128 = newSSA("int128");
    }
}}}}
