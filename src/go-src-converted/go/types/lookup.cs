// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements various field and method lookup functions.

// package types -- go2cs converted at 2020 August 29 08:47:42 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\lookup.go

using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // LookupFieldOrMethod looks up a field or method with given package and name
        // in T and returns the corresponding *Var or *Func, an index sequence, and a
        // bool indicating if there were any pointer indirections on the path to the
        // field or method. If addressable is set, T is the type of an addressable
        // variable (only matters for method lookups).
        //
        // The last index entry is the field or method index in the (possibly embedded)
        // type where the entry was found, either:
        //
        //    1) the list of declared methods of a named type; or
        //    2) the list of all methods (method set) of an interface type; or
        //    3) the list of fields of a struct type.
        //
        // The earlier index entries are the indices of the anonymous struct fields
        // traversed to get to the found entry, starting at depth 0.
        //
        // If no entry is found, a nil object is returned. In this case, the returned
        // index and indirect values have the following meaning:
        //
        //    - If index != nil, the index sequence points to an ambiguous entry
        //    (the same name appeared more than once at the same embedding level).
        //
        //    - If indirect is set, a method with a pointer receiver type was found
        //      but there was no pointer on the path from the actual receiver type to
        //    the method's formal receiver base type, nor was the receiver addressable.
        //
        public static (Object, slice<long>, bool) LookupFieldOrMethod(Type T, bool addressable, ref Package pkg, @string name)
        { 
            // Methods cannot be associated to a named pointer type
            // (spec: "The type denoted by T is called the receiver base type;
            // it must not be a pointer or interface type and it must be declared
            // in the same package as the method.").
            // Thus, if we have a named pointer type, proceed with the underlying
            // pointer type but discard the result if it is a method since we would
            // not have found it for T (see also issue 8590).
            {
                ref Named (t, _) = T._<ref Named>();

                if (t != null)
                {
                    {
                        ref Pointer (p, _) = t.underlying._<ref Pointer>();

                        if (p != null)
                        {
                            obj, index, indirect = lookupFieldOrMethod(p, false, pkg, name);
                            {
                                ref Func (_, ok) = obj._<ref Func>();

                                if (ok)
                                {
                                    return (null, null, false);
                                }
                            }
                            return;
                        }
                    }
                }
            }

            return lookupFieldOrMethod(T, addressable, pkg, name);
        }

        // TODO(gri) The named type consolidation and seen maps below must be
        //           indexed by unique keys for a given type. Verify that named
        //           types always have only one representation (even when imported
        //           indirectly via different packages.)

        private static (Object, slice<long>, bool) lookupFieldOrMethod(Type T, bool addressable, ref Package pkg, @string name)
        { 
            // WARNING: The code in this function is extremely subtle - do not modify casually!
            //          This function and NewMethodSet should be kept in sync.

            if (name == "_")
            {
                return; // blank fields/methods are never found
            }
            var (typ, isPtr) = deref(T); 

            // *typ where typ is an interface has no methods.
            if (isPtr && IsInterface(typ))
            {
                return;
            } 

            // Start with typ as single entry at shallowest depth.
            embeddedType current = new slice<embeddedType>(new embeddedType[] { {typ,nil,isPtr,false} }); 

            // Named types that we have seen already, allocated lazily.
            // Used to avoid endless searches in case of recursive types.
            // Since only Named types can be used for recursive types, we
            // only need to track those.
            // (If we ever allow type aliases to construct recursive types,
            // we must use type identity rather than pointer equality for
            // the map key comparison, as we do in consolidateMultiples.)
            map<ref Named, bool> seen = default; 

            // search current depth
            while (len(current) > 0L)
            {
                slice<embeddedType> next = default; // embedded types found at current depth

                // look for (pkg, name) in all types at current depth
                foreach (var (_, e) in current)
                {
                    var typ = e.typ; 

                    // If we have a named type, we may have associated methods.
                    // Look for those first.
                    {
                        ref Named (named, _) = typ._<ref Named>();

                        if (named != null)
                        {
                            if (seen[named])
                            { 
                                // We have seen this type before, at a more shallow depth
                                // (note that multiples of this type at the current depth
                                // were consolidated before). The type at that depth shadows
                                // this same type at the current depth, so we can ignore
                                // this one.
                                continue;
                            }
                            if (seen == null)
                            {
                                seen = make_map<ref Named, bool>();
                            }
                            seen[named] = true; 

                            // look for a matching attached method
                            {
                                var i__prev2 = i;

                                var (i, m) = lookupMethod(named.methods, pkg, name);

                                if (m != null)
                                { 
                                    // potential match
                                    assert(m.typ != null);
                                    index = concat(e.index, i);
                                    if (obj != null || e.multiples)
                                    {
                                        return (null, index, false); // collision
                                    }
                                    obj = m;
                                    indirect = e.indirect;
                                    continue; // we can't have a matching field or interface method
                                } 

                                // continue with underlying type

                                i = i__prev2;

                            } 

                            // continue with underlying type
                            typ = named.underlying;
                        }

                    }

                    switch (typ.type())
                    {
                        case ref Struct t:
                            {
                                var i__prev3 = i;
                                var f__prev3 = f;

                                foreach (var (__i, __f) in t.fields)
                                {
                                    i = __i;
                                    f = __f;
                                    if (f.sameId(pkg, name))
                                    {
                                        assert(f.typ != null);
                                        index = concat(e.index, i);
                                        if (obj != null || e.multiples)
                                        {
                                            return (null, index, false); // collision
                                        }
                                        obj = f;
                                        indirect = e.indirect;
                                        continue; // we can't have a matching interface method
                                    } 
                                    // Collect embedded struct fields for searching the next
                                    // lower depth, but only if we have not seen a match yet
                                    // (if we have a match it is either the desired field or
                                    // we have a name collision on the same depth; in either
                                    // case we don't need to look further).
                                    // Embedded fields are always of the form T or *T where
                                    // T is a type name. If e.typ appeared multiple times at
                                    // this depth, f.typ appears multiple times at the next
                                    // depth.
                                    if (obj == null && f.anonymous)
                                    {
                                        (typ, isPtr) = deref(f.typ); 
                                        // TODO(gri) optimization: ignore types that can't
                                        // have fields or methods (only Named, Struct, and
                                        // Interface types need to be considered).
                                        next = append(next, new embeddedType(typ,concat(e.index,i),e.indirect||isPtr,e.multiples));
                                    }
                                }

                                i = i__prev3;
                                f = f__prev3;
                            }
                            break;
                        case ref Interface t:
                            {
                                var i__prev1 = i;

                                (i, m) = lookupMethod(t.allMethods, pkg, name);

                                if (m != null)
                                {
                                    assert(m.typ != null);
                                    index = concat(e.index, i);
                                    if (obj != null || e.multiples)
                                    {
                                        return (null, index, false); // collision
                                    }
                                    obj = m;
                                    indirect = e.indirect;
                                }

                                i = i__prev1;

                            }
                            break;
                    }
                }
                if (obj != null)
                { 
                    // found a potential match
                    // spec: "A method call x.m() is valid if the method set of (the type of) x
                    //        contains m and the argument list can be assigned to the parameter
                    //        list of m. If x is addressable and &x's method set contains m, x.m()
                    //        is shorthand for (&x).m()".
                    {
                        var f__prev2 = f;

                        ref Func (f, _) = obj._<ref Func>();

                        if (f != null && ptrRecv(f) && !indirect && !addressable)
                        {
                            return (null, null, true); // pointer/addressable receiver required
                        }

                        f = f__prev2;

                    }
                    return;
                }
                current = consolidateMultiples(next);
            }


            return (null, null, false); // not found
        }

        // embeddedType represents an embedded type
        private partial struct embeddedType
        {
            public Type typ;
            public slice<long> index; // embedded field indices, starting with index at depth 0
            public bool indirect; // if set, there was a pointer indirection on the path to this field
            public bool multiples; // if set, typ appears multiple times at this depth
        }

        // consolidateMultiples collects multiple list entries with the same type
        // into a single entry marked as containing multiples. The result is the
        // consolidated list.
        private static slice<embeddedType> consolidateMultiples(slice<embeddedType> list)
        {
            if (len(list) <= 1L)
            {
                return list; // at most one entry - nothing to do
            }
            long n = 0L; // number of entries w/ unique type
            var prev = make_map<Type, long>(); // index at which type was previously seen
            foreach (var (_, e) in list)
            {
                {
                    var (i, found) = lookupType(prev, e.typ);

                    if (found)
                    {
                        list[i].multiples = true; 
                        // ignore this entry
                    }
                    else
                    {
                        prev[e.typ] = n;
                        list[n] = e;
                        n++;
                    }

                }
            }
            return list[..n];
        }

        private static (long, bool) lookupType(map<Type, long> m, Type typ)
        { 
            // fast path: maybe the types are equal
            {
                var i__prev1 = i;

                var (i, found) = m[typ];

                if (found)
                {
                    return (i, true);
                }

                i = i__prev1;

            }

            {
                var i__prev1 = i;

                foreach (var (__t, __i) in m)
                {
                    t = __t;
                    i = __i;
                    if (Identical(t, typ))
                    {
                        return (i, true);
                    }
                }

                i = i__prev1;
            }

            return (0L, false);
        }

        // MissingMethod returns (nil, false) if V implements T, otherwise it
        // returns a missing method required by T and whether it is missing or
        // just has the wrong type.
        //
        // For non-interface types V, or if static is set, V implements T if all
        // methods of T are present in V. Otherwise (V is an interface and static
        // is not set), MissingMethod only checks that methods of T which are also
        // present in V have matching types (e.g., for a type assertion x.(T) where
        // x is of interface type V).
        //
        public static (ref Func, bool) MissingMethod(Type V, ref Interface T, bool @static)
        { 
            // fast path for common case
            if (T.Empty())
            {
                return;
            } 

            // TODO(gri) Consider using method sets here. Might be more efficient.
            {
                ref Interface (ityp, _) = V.Underlying()._<ref Interface>();

                if (ityp != null)
                { 
                    // TODO(gri) allMethods is sorted - can do this more efficiently
                    {
                        var m__prev1 = m;

                        foreach (var (_, __m) in T.allMethods)
                        {
                            m = __m;
                            var (_, obj) = lookupMethod(ityp.allMethods, m.pkg, m.name);

                            if (obj == null) 
                                if (static)
                                {
                                    return (m, false);
                                }
                            else if (!Identical(obj.Type(), m.typ)) 
                                return (m, true);
                                                    }

                        m = m__prev1;
                    }

                    return;
                } 

                // A concrete type implements T if it implements all methods of T.

            } 

            // A concrete type implements T if it implements all methods of T.
            {
                var m__prev1 = m;

                foreach (var (_, __m) in T.allMethods)
                {
                    m = __m;
                    var (obj, _, _) = lookupFieldOrMethod(V, false, m.pkg, m.name);

                    ref Func (f, _) = obj._<ref Func>();
                    if (f == null)
                    {
                        return (m, false);
                    }
                    if (!Identical(f.typ, m.typ))
                    {
                        return (m, true);
                    }
                }

                m = m__prev1;
            }

            return;
        }

        // assertableTo reports whether a value of type V can be asserted to have type T.
        // It returns (nil, false) as affirmative answer. Otherwise it returns a missing
        // method required by V and whether it is missing or just has the wrong type.
        private static (ref Func, bool) assertableTo(ref Interface V, Type T)
        { 
            // no static check is required if T is an interface
            // spec: "If T is an interface type, x.(T) asserts that the
            //        dynamic type of x implements the interface T."
            {
                ref Interface (_, ok) = T.Underlying()._<ref Interface>();

                if (ok && !strict)
                {
                    return;
                }

            }
            return MissingMethod(T, V, false);
        }

        // deref dereferences typ if it is a *Pointer and returns its base and true.
        // Otherwise it returns (typ, false).
        private static (Type, bool) deref(Type typ)
        {
            {
                ref Pointer (p, _) = typ._<ref Pointer>();

                if (p != null)
                {
                    return (p.@base, true);
                }

            }
            return (typ, false);
        }

        // derefStructPtr dereferences typ if it is a (named or unnamed) pointer to a
        // (named or unnamed) struct and returns its base. Otherwise it returns typ.
        private static Type derefStructPtr(Type typ)
        {
            {
                ref Pointer (p, _) = typ.Underlying()._<ref Pointer>();

                if (p != null)
                {
                    {
                        ref Struct (_, ok) = p.@base.Underlying()._<ref Struct>();

                        if (ok)
                        {
                            return p.@base;
                        }

                    }
                }

            }
            return typ;
        }

        // concat returns the result of concatenating list and i.
        // The result does not share its underlying array with list.
        private static slice<long> concat(slice<long> list, long i)
        {
            slice<long> t = default;
            t = append(t, list);
            return append(t, i);
        }

        // fieldIndex returns the index for the field with matching package and name, or a value < 0.
        private static long fieldIndex(slice<ref Var> fields, ref Package pkg, @string name)
        {
            if (name != "_")
            {
                foreach (var (i, f) in fields)
                {
                    if (f.sameId(pkg, name))
                    {
                        return i;
                    }
                }
            }
            return -1L;
        }

        // lookupMethod returns the index of and method with matching package and name, or (-1, nil).
        private static (long, ref Func) lookupMethod(slice<ref Func> methods, ref Package pkg, @string name)
        {
            if (name != "_")
            {
                foreach (var (i, m) in methods)
                {
                    if (m.sameId(pkg, name))
                    {
                        return (i, m);
                    }
                }
            }
            return (-1L, null);
        }
    }
}}
