// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 October 08 04:03:51 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\type.go
using sort = go.sort_package;
using static go.builtin;
using System;

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

        public static readonly BasicKind Invalid = (BasicKind)iota; // type is invalid

        // predeclared types
        public static readonly var Bool = (var)0;
        public static readonly var Int = (var)1;
        public static readonly var Int8 = (var)2;
        public static readonly var Int16 = (var)3;
        public static readonly var Int32 = (var)4;
        public static readonly var Int64 = (var)5;
        public static readonly var Uint = (var)6;
        public static readonly var Uint8 = (var)7;
        public static readonly var Uint16 = (var)8;
        public static readonly var Uint32 = (var)9;
        public static readonly var Uint64 = (var)10;
        public static readonly var Uintptr = (var)11;
        public static readonly var Float32 = (var)12;
        public static readonly var Float64 = (var)13;
        public static readonly var Complex64 = (var)14;
        public static readonly var Complex128 = (var)15;
        public static readonly var String = (var)16;
        public static readonly var UnsafePointer = (var)17; 

        // types for untyped values
        public static readonly var UntypedBool = (var)18;
        public static readonly var UntypedInt = (var)19;
        public static readonly var UntypedRune = (var)20;
        public static readonly var UntypedFloat = (var)21;
        public static readonly var UntypedComplex = (var)22;
        public static readonly var UntypedString = (var)23;
        public static readonly Byte UntypedNil = (Byte)Uint8;
        public static readonly var Rune = (var)Int32;


        // BasicInfo is a set of flags describing properties of a basic type.
        public partial struct BasicInfo // : long
        {
        }

        // Properties of basic types.
        public static readonly BasicInfo IsBoolean = (BasicInfo)1L << (int)(iota);
        public static readonly var IsInteger = (var)0;
        public static readonly var IsUnsigned = (var)1;
        public static readonly var IsFloat = (var)2;
        public static readonly var IsComplex = (var)3;
        public static readonly var IsString = (var)4;
        public static readonly IsOrdered IsUntyped = (IsOrdered)IsInteger | IsFloat | IsString;
        public static readonly var IsNumeric = (var)IsInteger | IsFloat | IsComplex;
        public static readonly var IsConstType = (var)IsBoolean | IsNumeric | IsString;


        // A Basic represents a basic type.
        public partial struct Basic
        {
            public BasicKind kind;
            public BasicInfo info;
            public @string name;
        }

        // Kind returns the kind of basic type b.
        private static BasicKind Kind(this ptr<Basic> _addr_b)
        {
            ref Basic b = ref _addr_b.val;

            return b.kind;
        }

        // Info returns information about properties of basic type b.
        private static BasicInfo Info(this ptr<Basic> _addr_b)
        {
            ref Basic b = ref _addr_b.val;

            return b.info;
        }

        // Name returns the name of basic type b.
        private static @string Name(this ptr<Basic> _addr_b)
        {
            ref Basic b = ref _addr_b.val;

            return b.name;
        }

        // An Array represents an array type.
        public partial struct Array
        {
            public long len;
            public Type elem;
        }

        // NewArray returns a new array type for the given element type and length.
        // A negative length indicates an unknown length.
        public static ptr<Array> NewArray(Type elem, long len)
        {
            return addr(new Array(len,elem));
        }

        // Len returns the length of array a.
        // A negative result indicates an unknown length.
        private static long Len(this ptr<Array> _addr_a)
        {
            ref Array a = ref _addr_a.val;

            return a.len;
        }

        // Elem returns element type of array a.
        private static Type Elem(this ptr<Array> _addr_a)
        {
            ref Array a = ref _addr_a.val;

            return a.elem;
        }

        // A Slice represents a slice type.
        public partial struct Slice
        {
            public Type elem;
        }

        // NewSlice returns a new slice type for the given element type.
        public static ptr<Slice> NewSlice(Type elem)
        {
            return addr(new Slice(elem));
        }

        // Elem returns the element type of slice s.
        private static Type Elem(this ptr<Slice> _addr_s)
        {
            ref Slice s = ref _addr_s.val;

            return s.elem;
        }

        // A Struct represents a struct type.
        public partial struct Struct
        {
            public slice<ptr<Var>> fields;
            public slice<@string> tags; // field tags; nil if there are no tags
        }

        // NewStruct returns a new struct with the given fields and corresponding field tags.
        // If a field with index i has a tag, tags[i] must be that tag, but len(tags) may be
        // only as long as required to hold the tag with the largest index i. Consequently,
        // if no field has a tag, tags may be nil.
        public static ptr<Struct> NewStruct(slice<ptr<Var>> fields, slice<@string> tags) => func((_, panic, __) =>
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

            return addr(new Struct(fields:fields,tags:tags));

        });

        // NumFields returns the number of fields in the struct (including blank and embedded fields).
        private static long NumFields(this ptr<Struct> _addr_s)
        {
            ref Struct s = ref _addr_s.val;

            return len(s.fields);
        }

        // Field returns the i'th field for 0 <= i < NumFields().
        private static ptr<Var> Field(this ptr<Struct> _addr_s, long i)
        {
            ref Struct s = ref _addr_s.val;

            return _addr_s.fields[i]!;
        }

        // Tag returns the i'th field tag for 0 <= i < NumFields().
        private static @string Tag(this ptr<Struct> _addr_s, long i)
        {
            ref Struct s = ref _addr_s.val;

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
        public static ptr<Pointer> NewPointer(Type elem)
        {
            return addr(new Pointer(base:elem));
        }

        // Elem returns the element type for the given pointer p.
        private static Type Elem(this ptr<Pointer> _addr_p)
        {
            ref Pointer p = ref _addr_p.val;

            return p.@base;
        }

        // A Tuple represents an ordered list of variables; a nil *Tuple is a valid (empty) tuple.
        // Tuples are used as components of signatures and to represent the type of multiple
        // assignments; they are not first class types of Go.
        public partial struct Tuple
        {
            public slice<ptr<Var>> vars;
        }

        // NewTuple returns a new tuple for the given variables.
        public static ptr<Tuple> NewTuple(params ptr<ptr<Var>>[] _addr_x)
        {
            x = x.Clone();
            ref Var x = ref _addr_x.val;

            if (len(x) > 0L)
            {
                return addr(new Tuple(x));
            }

            return _addr_null!;

        }

        // Len returns the number variables of tuple t.
        private static long Len(this ptr<Tuple> _addr_t)
        {
            ref Tuple t = ref _addr_t.val;

            if (t != null)
            {
                return len(t.vars);
            }

            return 0L;

        }

        // At returns the i'th variable of tuple t.
        private static ptr<Var> At(this ptr<Tuple> _addr_t, long i)
        {
            ref Tuple t = ref _addr_t.val;

            return _addr_t.vars[i]!;
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
        public static ptr<Signature> NewSignature(ptr<Var> _addr_recv, ptr<Tuple> _addr_@params, ptr<Tuple> _addr_results, bool variadic) => func((_, panic, __) =>
        {
            ref Var recv = ref _addr_recv.val;
            ref Tuple @params = ref _addr_@params.val;
            ref Tuple results = ref _addr_results.val;

            if (variadic)
            {
                var n = @params.Len();
                if (n == 0L)
                {
                    panic("types.NewSignature: variadic function must have at least one parameter");
                }

                {
                    ptr<Slice> (_, ok) = @params.At(n - 1L).typ._<ptr<Slice>>();

                    if (!ok)
                    {
                        panic("types.NewSignature: variadic parameter must be of unnamed slice type");
                    }

                }

            }

            return addr(new Signature(nil,recv,params,results,variadic));

        });

        // Recv returns the receiver of signature s (if a method), or nil if a
        // function. It is ignored when comparing signatures for identity.
        //
        // For an abstract method, Recv returns the enclosing interface either
        // as a *Named or an *Interface. Due to embedding, an interface may
        // contain methods whose receiver type is a different interface.
        private static ptr<Var> Recv(this ptr<Signature> _addr_s)
        {
            ref Signature s = ref _addr_s.val;

            return _addr_s.recv!;
        }

        // Params returns the parameters of signature s, or nil.
        private static ptr<Tuple> Params(this ptr<Signature> _addr_s)
        {
            ref Signature s = ref _addr_s.val;

            return _addr_s.@params!;
        }

        // Results returns the results of signature s, or nil.
        private static ptr<Tuple> Results(this ptr<Signature> _addr_s)
        {
            ref Signature s = ref _addr_s.val;

            return _addr_s.results!;
        }

        // Variadic reports whether the signature s is variadic.
        private static bool Variadic(this ptr<Signature> _addr_s)
        {
            ref Signature s = ref _addr_s.val;

            return s.variadic;
        }

        // An Interface represents an interface type.
        public partial struct Interface
        {
            public slice<ptr<Func>> methods; // ordered list of explicitly declared methods
            public slice<Type> embeddeds; // ordered list of explicitly embedded types

            public slice<ptr<Func>> allMethods; // ordered list of methods declared with or embedded in this interface (TODO(gri): replace with mset)
        }

        // emptyInterface represents the empty (completed) interface
        private static Interface emptyInterface = new Interface(allMethods:markComplete);

        // markComplete is used to mark an empty interface as completely
        // set up by setting the allMethods field to a non-nil empty slice.
        private static var markComplete = make_slice<ptr<Func>>(0L);

        // NewInterface returns a new (incomplete) interface for the given methods and embedded types.
        // Each embedded type must have an underlying type of interface type.
        // NewInterface takes ownership of the provided methods and may modify their types by setting
        // missing receivers. To compute the method set of the interface, Complete must be called.
        //
        // Deprecated: Use NewInterfaceType instead which allows any (even non-defined) interface types
        // to be embedded. This is necessary for interfaces that embed alias type names referring to
        // non-defined (literal) interface types.
        public static ptr<Interface> NewInterface(slice<ptr<Func>> methods, slice<ptr<Named>> embeddeds)
        {
            var tnames = make_slice<Type>(len(embeddeds));
            foreach (var (i, t) in embeddeds)
            {
                tnames[i] = t;
            }
            return _addr_NewInterfaceType(methods, tnames)!;

        }

        // NewInterfaceType returns a new (incomplete) interface for the given methods and embedded types.
        // Each embedded type must have an underlying type of interface type (this property is not
        // verified for defined types, which may be in the process of being set up and which don't
        // have a valid underlying type yet).
        // NewInterfaceType takes ownership of the provided methods and may modify their types by setting
        // missing receivers. To compute the method set of the interface, Complete must be called.
        public static ptr<Interface> NewInterfaceType(slice<ptr<Func>> methods, slice<Type> embeddeds) => func((_, panic, __) =>
        {
            if (len(methods) == 0L && len(embeddeds) == 0L)
            {
                return _addr__addr_emptyInterface!;
            } 

            // set method receivers if necessary
            ptr<Interface> typ = @new<Interface>();
            foreach (var (_, m) in methods)
            {
                {
                    ptr<Signature> sig = m.typ._<ptr<Signature>>();

                    if (sig.recv == null)
                    {
                        sig.recv = NewVar(m.pos, m.pkg, "", typ);
                    }

                }

            } 

            // All embedded types should be interfaces; however, defined types
            // may not yet be fully resolved. Only verify that non-defined types
            // are interfaces. This matches the behavior of the code before the
            // fix for #25301 (issue #25596).
            foreach (var (_, t) in embeddeds)
            {
                {
                    ptr<Named> (_, ok) = t._<ptr<Named>>();

                    if (!ok && !IsInterface(t))
                    {
                        panic("embedded type is not an interface");
                    }

                }

            } 

            // sort for API stability
            sort.Sort(byUniqueMethodName(methods));
            sort.Stable(byUniqueTypeName(embeddeds));

            typ.methods = methods;
            typ.embeddeds = embeddeds;
            return _addr_typ!;

        });

        // NumExplicitMethods returns the number of explicitly declared methods of interface t.
        private static long NumExplicitMethods(this ptr<Interface> _addr_t)
        {
            ref Interface t = ref _addr_t.val;

            return len(t.methods);
        }

        // ExplicitMethod returns the i'th explicitly declared method of interface t for 0 <= i < t.NumExplicitMethods().
        // The methods are ordered by their unique Id.
        private static ptr<Func> ExplicitMethod(this ptr<Interface> _addr_t, long i)
        {
            ref Interface t = ref _addr_t.val;

            return _addr_t.methods[i]!;
        }

        // NumEmbeddeds returns the number of embedded types in interface t.
        private static long NumEmbeddeds(this ptr<Interface> _addr_t)
        {
            ref Interface t = ref _addr_t.val;

            return len(t.embeddeds);
        }

        // Embedded returns the i'th embedded defined (*Named) type of interface t for 0 <= i < t.NumEmbeddeds().
        // The result is nil if the i'th embedded type is not a defined type.
        //
        // Deprecated: Use EmbeddedType which is not restricted to defined (*Named) types.
        private static ptr<Named> Embedded(this ptr<Interface> _addr_t, long i)
        {
            ref Interface t = ref _addr_t.val;

            ptr<Named> (tname, _) = t.embeddeds[i]._<ptr<Named>>();

            return _addr_tname!;
        }

        // EmbeddedType returns the i'th embedded type of interface t for 0 <= i < t.NumEmbeddeds().
        private static Type EmbeddedType(this ptr<Interface> _addr_t, long i)
        {
            ref Interface t = ref _addr_t.val;

            return t.embeddeds[i];
        }

        // NumMethods returns the total number of methods of interface t.
        // The interface must have been completed.
        private static long NumMethods(this ptr<Interface> _addr_t)
        {
            ref Interface t = ref _addr_t.val;

            t.assertCompleteness();

            return len(t.allMethods);
        }

        private static void assertCompleteness(this ptr<Interface> _addr_t) => func((_, panic, __) =>
        {
            ref Interface t = ref _addr_t.val;

            if (t.allMethods == null)
            {
                panic("interface is incomplete");
            }

        });

        // Method returns the i'th method of interface t for 0 <= i < t.NumMethods().
        // The methods are ordered by their unique Id.
        // The interface must have been completed.
        private static ptr<Func> Method(this ptr<Interface> _addr_t, long i)
        {
            ref Interface t = ref _addr_t.val;

            t.assertCompleteness();

            return _addr_t.allMethods[i]!;
        }

        // Empty reports whether t is the empty interface.
        // The interface must have been completed.
        private static bool Empty(this ptr<Interface> _addr_t)
        {
            ref Interface t = ref _addr_t.val;

            t.assertCompleteness();

            return len(t.allMethods) == 0L;
        }

        // Complete computes the interface's method set. It must be called by users of
        // NewInterfaceType and NewInterface after the interface's embedded types are
        // fully defined and before using the interface type in any way other than to
        // form other types. The interface must not contain duplicate methods or a
        // panic occurs. Complete returns the receiver.
        private static ptr<Interface> Complete(this ptr<Interface> _addr_t) => func((_, panic, __) =>
        {
            ref Interface t = ref _addr_t.val;
 
            // TODO(gri) consolidate this method with Checker.completeInterface
            if (t.allMethods != null)
            {
                return _addr_t!;
            }

            t.allMethods = markComplete; // avoid infinite recursion

            slice<ptr<Func>> todo = default;
            slice<ptr<Func>> methods = default;
            objset seen = default;
            Action<ptr<Func>, bool> addMethod = (m, @explicit) =>
            {
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

            }
;

            {
                var m__prev1 = m;

                foreach (var (_, __m) in t.methods)
                {
                    m = __m;
                    addMethod(m, true);
                }

                m = m__prev1;
            }

            {
                var typ__prev1 = typ;

                foreach (var (_, __typ) in t.embeddeds)
                {
                    typ = __typ;
                    ptr<Interface> typ = typ.Underlying()._<ptr<Interface>>();
                    typ.Complete();
                    {
                        var m__prev2 = m;

                        foreach (var (_, __m) in typ.allMethods)
                        {
                            m = __m;
                            addMethod(m, false);
                        }

                        m = m__prev2;
                    }
                }

                typ = typ__prev1;
            }

            {
                long i = 0L;

                while (i < len(todo))
                {
                    var m = todo[i];
                    other = todo[i + 1L];
                    if (!Identical(m.typ, other.typ))
                    {
                        panic("duplicate method " + m.name);
                    i += 2L;
                    }

                }

            }

            if (methods != null)
            {
                sort.Sort(byUniqueMethodName(methods));
                t.allMethods = methods;
            }

            return _addr_t!;

        });

        // A Map represents a map type.
        public partial struct Map
        {
            public Type key;
            public Type elem;
        }

        // NewMap returns a new map for the given key and element types.
        public static ptr<Map> NewMap(Type key, Type elem)
        {
            return addr(new Map(key,elem));
        }

        // Key returns the key type of map m.
        private static Type Key(this ptr<Map> _addr_m)
        {
            ref Map m = ref _addr_m.val;

            return m.key;
        }

        // Elem returns the element type of map m.
        private static Type Elem(this ptr<Map> _addr_m)
        {
            ref Map m = ref _addr_m.val;

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
        public static readonly ChanDir SendRecv = (ChanDir)iota;
        public static readonly var SendOnly = (var)0;
        public static readonly var RecvOnly = (var)1;


        // NewChan returns a new channel type for the given direction and element type.
        public static ptr<Chan> NewChan(ChanDir dir, Type elem)
        {
            return addr(new Chan(dir,elem));
        }

        // Dir returns the direction of channel c.
        private static ChanDir Dir(this ptr<Chan> _addr_c)
        {
            ref Chan c = ref _addr_c.val;

            return c.dir;
        }

        // Elem returns the element type of channel c.
        private static Type Elem(this ptr<Chan> _addr_c)
        {
            ref Chan c = ref _addr_c.val;

            return c.elem;
        }

        // A Named represents a named type.
        public partial struct Named
        {
            public typeInfo info; // for cycle detection
            public ptr<TypeName> obj; // corresponding declared object
            public Type orig; // type (on RHS of declaration) this *Named type is derived of (for cycle reporting)
            public Type underlying; // possibly a *Named during setup; never a *Named once set up completely
            public slice<ptr<Func>> methods; // methods declared for this type (not the method set of this type); signatures are type-checked lazily
        }

        // NewNamed returns a new named type for the given type name, underlying type, and associated methods.
        // If the given type name obj doesn't have a type yet, its type is set to the returned named type.
        // The underlying type must not be a *Named.
        public static ptr<Named> NewNamed(ptr<TypeName> _addr_obj, Type underlying, slice<ptr<Func>> methods) => func((_, panic, __) =>
        {
            ref TypeName obj = ref _addr_obj.val;

            {
                ptr<Named> (_, ok) = underlying._<ptr<Named>>();

                if (ok)
                {
                    panic("types.NewNamed: underlying type must not be *Named");
                }

            }

            ptr<Named> typ = addr(new Named(obj:obj,orig:underlying,underlying:underlying,methods:methods));
            if (obj.typ == null)
            {
                obj.typ = typ;
            }

            return _addr_typ!;

        });

        // Obj returns the type name for the named type t.
        private static ptr<TypeName> Obj(this ptr<Named> _addr_t)
        {
            ref Named t = ref _addr_t.val;

            return _addr_t.obj!;
        }

        // NumMethods returns the number of explicit methods whose receiver is named type t.
        private static long NumMethods(this ptr<Named> _addr_t)
        {
            ref Named t = ref _addr_t.val;

            return len(t.methods);
        }

        // Method returns the i'th method of named type t for 0 <= i < t.NumMethods().
        private static ptr<Func> Method(this ptr<Named> _addr_t, long i)
        {
            ref Named t = ref _addr_t.val;

            return _addr_t.methods[i]!;
        }

        // SetUnderlying sets the underlying type and marks t as complete.
        private static void SetUnderlying(this ptr<Named> _addr_t, Type underlying) => func((_, panic, __) =>
        {
            ref Named t = ref _addr_t.val;

            if (underlying == null)
            {
                panic("types.Named.SetUnderlying: underlying type must not be nil");
            }

            {
                ptr<Named> (_, ok) = underlying._<ptr<Named>>();

                if (ok)
                {
                    panic("types.Named.SetUnderlying: underlying type must not be *Named");
                }

            }

            t.underlying = underlying;

        });

        // AddMethod adds method m unless it is already in the method list.
        private static void AddMethod(this ptr<Named> _addr_t, ptr<Func> _addr_m)
        {
            ref Named t = ref _addr_t.val;
            ref Func m = ref _addr_m.val;

            {
                var (i, _) = lookupMethod(t.methods, m.pkg, m.name);

                if (i < 0L)
                {
                    t.methods = append(t.methods, m);
                }

            }

        }

        // Implementations for Type methods.

        private static Type Underlying(this ptr<Basic> _addr_b)
        {
            ref Basic b = ref _addr_b.val;

            return b;
        }
        private static Type Underlying(this ptr<Array> _addr_a)
        {
            ref Array a = ref _addr_a.val;

            return a;
        }
        private static Type Underlying(this ptr<Slice> _addr_s)
        {
            ref Slice s = ref _addr_s.val;

            return s;
        }
        private static Type Underlying(this ptr<Struct> _addr_s)
        {
            ref Struct s = ref _addr_s.val;

            return s;
        }
        private static Type Underlying(this ptr<Pointer> _addr_p)
        {
            ref Pointer p = ref _addr_p.val;

            return p;
        }
        private static Type Underlying(this ptr<Tuple> _addr_t)
        {
            ref Tuple t = ref _addr_t.val;

            return t;
        }
        private static Type Underlying(this ptr<Signature> _addr_s)
        {
            ref Signature s = ref _addr_s.val;

            return s;
        }
        private static Type Underlying(this ptr<Interface> _addr_t)
        {
            ref Interface t = ref _addr_t.val;

            return t;
        }
        private static Type Underlying(this ptr<Map> _addr_m)
        {
            ref Map m = ref _addr_m.val;

            return m;
        }
        private static Type Underlying(this ptr<Chan> _addr_c)
        {
            ref Chan c = ref _addr_c.val;

            return c;
        }
        private static Type Underlying(this ptr<Named> _addr_t)
        {
            ref Named t = ref _addr_t.val;

            return t.underlying;
        }

        private static @string String(this ptr<Basic> _addr_b)
        {
            ref Basic b = ref _addr_b.val;

            return TypeString(b, null);
        }
        private static @string String(this ptr<Array> _addr_a)
        {
            ref Array a = ref _addr_a.val;

            return TypeString(a, null);
        }
        private static @string String(this ptr<Slice> _addr_s)
        {
            ref Slice s = ref _addr_s.val;

            return TypeString(s, null);
        }
        private static @string String(this ptr<Struct> _addr_s)
        {
            ref Struct s = ref _addr_s.val;

            return TypeString(s, null);
        }
        private static @string String(this ptr<Pointer> _addr_p)
        {
            ref Pointer p = ref _addr_p.val;

            return TypeString(p, null);
        }
        private static @string String(this ptr<Tuple> _addr_t)
        {
            ref Tuple t = ref _addr_t.val;

            return TypeString(t, null);
        }
        private static @string String(this ptr<Signature> _addr_s)
        {
            ref Signature s = ref _addr_s.val;

            return TypeString(s, null);
        }
        private static @string String(this ptr<Interface> _addr_t)
        {
            ref Interface t = ref _addr_t.val;

            return TypeString(t, null);
        }
        private static @string String(this ptr<Map> _addr_m)
        {
            ref Map m = ref _addr_m.val;

            return TypeString(m, null);
        }
        private static @string String(this ptr<Chan> _addr_c)
        {
            ref Chan c = ref _addr_c.val;

            return TypeString(c, null);
        }
        private static @string String(this ptr<Named> _addr_t)
        {
            ref Named t = ref _addr_t.val;

            return TypeString(t, null);
        }
    }
}}
