// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 August 29 08:48:01 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\type.go
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // A Type represents a type of Go.
        // All types implement the Type interface.
        public partial interface Type
        {
            @string Underlying(); // String returns a string representation of a type.
            @string String();
        }

        // BasicKind describes the kind of basic type.
        public partial struct BasicKind // : long
        {
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
        public partial struct BasicInfo // : long
        {
        }

        // Properties of basic types.
        public static readonly BasicInfo IsBoolean = 1L << (int)(iota);
        public static readonly var IsInteger = 0;
        public static readonly var IsUnsigned = 1;
        public static readonly var IsFloat = 2;
        public static readonly var IsComplex = 3;
        public static readonly var IsString = 4;
        public static readonly IsOrdered IsUntyped = IsInteger | IsFloat | IsString;
        public static readonly var IsNumeric = IsInteger | IsFloat | IsComplex;
        public static readonly var IsConstType = IsBoolean | IsNumeric | IsString;

        // A Basic represents a basic type.
        public partial struct Basic
        {
            public BasicKind kind;
            public BasicInfo info;
            public @string name;
        }

        // Kind returns the kind of basic type b.
        private static BasicKind Kind(this ref Basic b)
        {
            return b.kind;
        }

        // Info returns information about properties of basic type b.
        private static BasicInfo Info(this ref Basic b)
        {
            return b.info;
        }

        // Name returns the name of basic type b.
        private static @string Name(this ref Basic b)
        {
            return b.name;
        }

        // An Array represents an array type.
        public partial struct Array
        {
            public long len;
            public Type elem;
        }

        // NewArray returns a new array type for the given element type and length.
        public static ref Array NewArray(Type elem, long len)
        {
            return ref new Array(len,elem);
        }

        // Len returns the length of array a.
        private static long Len(this ref Array a)
        {
            return a.len;
        }

        // Elem returns element type of array a.
        private static Type Elem(this ref Array a)
        {
            return a.elem;
        }

        // A Slice represents a slice type.
        public partial struct Slice
        {
            public Type elem;
        }

        // NewSlice returns a new slice type for the given element type.
        public static ref Slice NewSlice(Type elem)
        {
            return ref new Slice(elem);
        }

        // Elem returns the element type of slice s.
        private static Type Elem(this ref Slice s)
        {
            return s.elem;
        }

        // A Struct represents a struct type.
        public partial struct Struct
        {
            public slice<ref Var> fields;
            public slice<@string> tags; // field tags; nil if there are no tags
        }

        // NewStruct returns a new struct with the given fields and corresponding field tags.
        // If a field with index i has a tag, tags[i] must be that tag, but len(tags) may be
        // only as long as required to hold the tag with the largest index i. Consequently,
        // if no field has a tag, tags may be nil.
        public static ref Struct NewStruct(slice<ref Var> fields, slice<@string> tags) => func((_, panic, __) =>
        {
            objset fset = default;
            foreach (var (_, f) in fields)
            {
                if (f.name != "_" && fset.insert(f) != null)
                {
                    panic("multiple fields with the same name");
                }
            }
            if (len(tags) > len(fields))
            {
                panic("more tags than fields");
            }
            return ref new Struct(fields:fields,tags:tags);
        });

        // NumFields returns the number of fields in the struct (including blank and anonymous fields).
        private static long NumFields(this ref Struct s)
        {
            return len(s.fields);
        }

        // Field returns the i'th field for 0 <= i < NumFields().
        private static ref Var Field(this ref Struct s, long i)
        {
            return s.fields[i];
        }

        // Tag returns the i'th field tag for 0 <= i < NumFields().
        private static @string Tag(this ref Struct s, long i)
        {
            if (i < len(s.tags))
            {
                return s.tags[i];
            }
            return "";
        }

        // A Pointer represents a pointer type.
        public partial struct Pointer
        {
            public Type @base; // element type
        }

        // NewPointer returns a new pointer type for the given element (base) type.
        public static ref Pointer NewPointer(Type elem)
        {
            return ref new Pointer(base:elem);
        }

        // Elem returns the element type for the given pointer p.
        private static Type Elem(this ref Pointer p)
        {
            return p.@base;
        }

        // A Tuple represents an ordered list of variables; a nil *Tuple is a valid (empty) tuple.
        // Tuples are used as components of signatures and to represent the type of multiple
        // assignments; they are not first class types of Go.
        public partial struct Tuple
        {
            public slice<ref Var> vars;
        }

        // NewTuple returns a new tuple for the given variables.
        public static ref Tuple NewTuple(params ptr<Var>[] x)
        {
            x = x.Clone();

            if (len(x) > 0L)
            {
                return ref new Tuple(x);
            }
            return null;
        }

        // Len returns the number variables of tuple t.
        private static long Len(this ref Tuple t)
        {
            if (t != null)
            {
                return len(t.vars);
            }
            return 0L;
        }

        // At returns the i'th variable of tuple t.
        private static ref Var At(this ref Tuple t, long i)
        {
            return t.vars[i];
        }

        // A Signature represents a (non-builtin) function or method type.
        // The receiver is ignored when comparing signatures for identity.
        public partial struct Signature
        {
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
        public static ref Signature NewSignature(ref Var _recv, ref Tuple _@params, ref Tuple _results, bool variadic) => func(_recv, _@params, _results, (ref Var recv, ref Tuple @params, ref Tuple results, Defer _, Panic panic, Recover __) =>
        {
            if (variadic)
            {
                var n = @params.Len();
                if (n == 0L)
                {
                    panic("types.NewSignature: variadic function must have at least one parameter");
                }
                {
                    ref Slice (_, ok) = @params.At(n - 1L).typ._<ref Slice>();

                    if (!ok)
                    {
                        panic("types.NewSignature: variadic parameter must be of unnamed slice type");
                    }

                }
            }
            return ref new Signature(nil,recv,params,results,variadic);
        });

        // Recv returns the receiver of signature s (if a method), or nil if a
        // function. It is ignored when comparing signatures for identity.
        //
        // For an abstract method, Recv returns the enclosing interface either
        // as a *Named or an *Interface. Due to embedding, an interface may
        // contain methods whose receiver type is a different interface.
        private static ref Var Recv(this ref Signature s)
        {
            return s.recv;
        }

        // Params returns the parameters of signature s, or nil.
        private static ref Tuple Params(this ref Signature s)
        {
            return s.@params;
        }

        // Results returns the results of signature s, or nil.
        private static ref Tuple Results(this ref Signature s)
        {
            return s.results;
        }

        // Variadic reports whether the signature s is variadic.
        private static bool Variadic(this ref Signature s)
        {
            return s.variadic;
        }

        // An Interface represents an interface type.
        public partial struct Interface
        {
            public slice<ref Func> methods; // ordered list of explicitly declared methods
            public slice<ref Named> embeddeds; // ordered list of explicitly embedded types

            public slice<ref Func> allMethods; // ordered list of methods declared with or embedded in this interface (TODO(gri): replace with mset)
        }

        // emptyInterface represents the empty (completed) interface
        private static Interface emptyInterface = new Interface(allMethods:markComplete);

        // markComplete is used to mark an empty interface as completely
        // set up by setting the allMethods field to a non-nil empty slice.
        private static var markComplete = make_slice<ref Func>(0L);

        // NewInterface returns a new (incomplete) interface for the given methods and embedded types.
        // To compute the method set of the interface, Complete must be called.
        public static ref Interface NewInterface(slice<ref Func> methods, slice<ref Named> embeddeds) => func((_, panic, __) =>
        {
            ptr<Interface> typ = @new<Interface>();

            if (len(methods) == 0L && len(embeddeds) == 0L)
            {
                return typ;
            }
            objset mset = default;
            foreach (var (_, m) in methods)
            {
                if (mset.insert(m) != null)
                {
                    panic("multiple methods with the same name");
                } 
                // set receiver
                // TODO(gri) Ideally, we should use a named type here instead of
                // typ, for less verbose printing of interface method signatures.
                m.typ._<ref Signature>().recv = NewVar(m.pos, m.pkg, "", typ);
            }
            sort.Sort(byUniqueMethodName(methods));

            if (embeddeds != null)
            {
                sort.Sort(byUniqueTypeName(embeddeds));
            }
            typ.methods = methods;
            typ.embeddeds = embeddeds;
            return typ;
        });

        // NumExplicitMethods returns the number of explicitly declared methods of interface t.
        private static long NumExplicitMethods(this ref Interface t)
        {
            return len(t.methods);
        }

        // ExplicitMethod returns the i'th explicitly declared method of interface t for 0 <= i < t.NumExplicitMethods().
        // The methods are ordered by their unique Id.
        private static ref Func ExplicitMethod(this ref Interface t, long i)
        {
            return t.methods[i];
        }

        // NumEmbeddeds returns the number of embedded types in interface t.
        private static long NumEmbeddeds(this ref Interface t)
        {
            return len(t.embeddeds);
        }

        // Embedded returns the i'th embedded type of interface t for 0 <= i < t.NumEmbeddeds().
        // The types are ordered by the corresponding TypeName's unique Id.
        private static ref Named Embedded(this ref Interface t, long i)
        {
            return t.embeddeds[i];
        }

        // NumMethods returns the total number of methods of interface t.
        private static long NumMethods(this ref Interface t)
        {
            return len(t.allMethods);
        }

        // Method returns the i'th method of interface t for 0 <= i < t.NumMethods().
        // The methods are ordered by their unique Id.
        private static ref Func Method(this ref Interface t, long i)
        {
            return t.allMethods[i];
        }

        // Empty returns true if t is the empty interface.
        private static bool Empty(this ref Interface t)
        {
            return len(t.allMethods) == 0L;
        }

        // Complete computes the interface's method set. It must be called by users of
        // NewInterface after the interface's embedded types are fully defined and
        // before using the interface type in any way other than to form other types.
        // Complete returns the receiver.
        private static ref Interface Complete(this ref Interface t)
        {
            if (t.allMethods != null)
            {
                return t;
            }
            slice<ref Func> allMethods = default;
            if (t.embeddeds == null)
            {
                if (t.methods == null)
                {
                    allMethods = make_slice<ref Func>(0L, 1L);
                }
                else
                {
                    allMethods = t.methods;
                }
            }
            else
            {
                allMethods = append(allMethods, t.methods);
                foreach (var (_, et) in t.embeddeds)
                {
                    ref Interface it = et.Underlying()._<ref Interface>();
                    it.Complete();
                    foreach (var (_, tm) in it.allMethods)
                    { 
                        // Make a copy of the method and adjust its receiver type.
                        var newm = tm.Value;
                        ref Signature newmtyp = tm.typ._<ref Signature>().Value;
                        newm.typ = ref newmtyp;
                        newmtyp.recv = NewVar(newm.pos, newm.pkg, "", t);
                        allMethods = append(allMethods, ref newm);
                    }
                }
                sort.Sort(byUniqueMethodName(allMethods));
            }
            t.allMethods = allMethods;

            return t;
        }

        // A Map represents a map type.
        public partial struct Map
        {
            public Type key;
            public Type elem;
        }

        // NewMap returns a new map for the given key and element types.
        public static ref Map NewMap(Type key, Type elem)
        {
            return ref new Map(key,elem);
        }

        // Key returns the key type of map m.
        private static Type Key(this ref Map m)
        {
            return m.key;
        }

        // Elem returns the element type of map m.
        private static Type Elem(this ref Map m)
        {
            return m.elem;
        }

        // A Chan represents a channel type.
        public partial struct Chan
        {
            public ChanDir dir;
            public Type elem;
        }

        // A ChanDir value indicates a channel direction.
        public partial struct ChanDir // : long
        {
        }

        // The direction of a channel is indicated by one of these constants.
        public static readonly ChanDir SendRecv = iota;
        public static readonly var SendOnly = 0;
        public static readonly var RecvOnly = 1;

        // NewChan returns a new channel type for the given direction and element type.
        public static ref Chan NewChan(ChanDir dir, Type elem)
        {
            return ref new Chan(dir,elem);
        }

        // Dir returns the direction of channel c.
        private static ChanDir Dir(this ref Chan c)
        {
            return c.dir;
        }

        // Elem returns the element type of channel c.
        private static Type Elem(this ref Chan c)
        {
            return c.elem;
        }

        // A Named represents a named type.
        public partial struct Named
        {
            public ptr<TypeName> obj; // corresponding declared object
            public Type underlying; // possibly a *Named during setup; never a *Named once set up completely
            public slice<ref Func> methods; // methods declared for this type (not the method set of this type)
        }

        // NewNamed returns a new named type for the given type name, underlying type, and associated methods.
        // If the given type name obj doesn't have a type yet, its type is set to the returned named type.
        // The underlying type must not be a *Named.
        public static ref Named NewNamed(ref TypeName _obj, Type underlying, slice<ref Func> methods) => func(_obj, (ref TypeName obj, Defer _, Panic panic, Recover __) =>
        {
            {
                ref Named (_, ok) = underlying._<ref Named>();

                if (ok)
                {
                    panic("types.NewNamed: underlying type must not be *Named");
                }

            }
            Named typ = ref new Named(obj:obj,underlying:underlying,methods:methods);
            if (obj.typ == null)
            {
                obj.typ = typ;
            }
            return typ;
        });

        // Obj returns the type name for the named type t.
        private static ref TypeName Obj(this ref Named t)
        {
            return t.obj;
        }

        // NumMethods returns the number of explicit methods whose receiver is named type t.
        private static long NumMethods(this ref Named t)
        {
            return len(t.methods);
        }

        // Method returns the i'th method of named type t for 0 <= i < t.NumMethods().
        private static ref Func Method(this ref Named t, long i)
        {
            return t.methods[i];
        }

        // SetUnderlying sets the underlying type and marks t as complete.
        private static void SetUnderlying(this ref Named _t, Type underlying) => func(_t, (ref Named t, Defer _, Panic panic, Recover __) =>
        {
            if (underlying == null)
            {
                panic("types.Named.SetUnderlying: underlying type must not be nil");
            }
            {
                ref Named (_, ok) = underlying._<ref Named>();

                if (ok)
                {
                    panic("types.Named.SetUnderlying: underlying type must not be *Named");
                }

            }
            t.underlying = underlying;
        });

        // AddMethod adds method m unless it is already in the method list.
        private static void AddMethod(this ref Named t, ref Func m)
        {
            {
                var (i, _) = lookupMethod(t.methods, m.pkg, m.name);

                if (i < 0L)
                {
                    t.methods = append(t.methods, m);
                }

            }
        }

        // Implementations for Type methods.

        private static Type Underlying(this ref Basic t)
        {
            return t;
        }
        private static Type Underlying(this ref Array t)
        {
            return t;
        }
        private static Type Underlying(this ref Slice t)
        {
            return t;
        }
        private static Type Underlying(this ref Struct t)
        {
            return t;
        }
        private static Type Underlying(this ref Pointer t)
        {
            return t;
        }
        private static Type Underlying(this ref Tuple t)
        {
            return t;
        }
        private static Type Underlying(this ref Signature t)
        {
            return t;
        }
        private static Type Underlying(this ref Interface t)
        {
            return t;
        }
        private static Type Underlying(this ref Map t)
        {
            return t;
        }
        private static Type Underlying(this ref Chan t)
        {
            return t;
        }
        private static Type Underlying(this ref Named t)
        {
            return t.underlying;
        }

        private static @string String(this ref Basic t)
        {
            return TypeString(t, null);
        }
        private static @string String(this ref Array t)
        {
            return TypeString(t, null);
        }
        private static @string String(this ref Slice t)
        {
            return TypeString(t, null);
        }
        private static @string String(this ref Struct t)
        {
            return TypeString(t, null);
        }
        private static @string String(this ref Pointer t)
        {
            return TypeString(t, null);
        }
        private static @string String(this ref Tuple t)
        {
            return TypeString(t, null);
        }
        private static @string String(this ref Signature t)
        {
            return TypeString(t, null);
        }
        private static @string String(this ref Interface t)
        {
            return TypeString(t, null);
        }
        private static @string String(this ref Map t)
        {
            return TypeString(t, null);
        }
        private static @string String(this ref Chan t)
        {
            return TypeString(t, null);
        }
        private static @string String(this ref Named t)
        {
            return TypeString(t, null);
        }
    }
}}
