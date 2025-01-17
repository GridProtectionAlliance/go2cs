// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements various field and method lookup functions.

// package types2 -- go2cs converted at 2022 March 13 06:26:07 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\lookup.go
namespace go.cmd.compile.@internal;

public static partial class types2_package {

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
// The earlier index entries are the indices of the embedded struct fields
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
public static (Object, slice<nint>, bool) LookupFieldOrMethod(Type T, bool addressable, ptr<Package> _addr_pkg, @string name) {
    Object obj = default;
    slice<nint> index = default;
    bool indirect = default;
    ref Package pkg = ref _addr_pkg.val;

    return (Checker.val)(null).lookupFieldOrMethod(T, addressable, pkg, name);
}

// Internal use of Checker.lookupFieldOrMethod: If the obj result is a method
// associated with a concrete (non-interface) type, the method's signature
// may not be fully set up. Call Checker.objDecl(obj, nil) before accessing
// the method's type.
// TODO(gri) Now that we provide the *Checker, we can probably remove this
// caveat by calling Checker.objDecl from lookupFieldOrMethod. Investigate.

// lookupFieldOrMethod is like the external version but completes interfaces
// as necessary.
private static (Object, slice<nint>, bool) lookupFieldOrMethod(this ptr<Checker> _addr_check, Type T, bool addressable, ptr<Package> _addr_pkg, @string name) {
    Object obj = default;
    slice<nint> index = default;
    bool indirect = default;
    ref Checker check = ref _addr_check.val;
    ref Package pkg = ref _addr_pkg.val;
 
    // Methods cannot be associated to a named pointer type
    // (spec: "The type denoted by T is called the receiver base type;
    // it must not be a pointer or interface type and it must be declared
    // in the same package as the method.").
    // Thus, if we have a named pointer type, proceed with the underlying
    // pointer type but discard the result if it is a method since we would
    // not have found it for T (see also issue 8590).
    {
        var t = asNamed(T);

        if (t != null) {
            {
                ptr<Pointer> (p, _) = t.underlying._<ptr<Pointer>>();

                if (p != null) {
                    obj, index, indirect = check.rawLookupFieldOrMethod(p, false, pkg, name);
                    {
                        ptr<Func> (_, ok) = obj._<ptr<Func>>();

                        if (ok) {
                            return (null, null, false);
                        }

                    }
                    return ;
                }

            }
        }
    }

    return check.rawLookupFieldOrMethod(T, addressable, pkg, name);
}

// TODO(gri) The named type consolidation and seen maps below must be
//           indexed by unique keys for a given type. Verify that named
//           types always have only one representation (even when imported
//           indirectly via different packages.)

// rawLookupFieldOrMethod should only be called by lookupFieldOrMethod and missingMethod.
private static (Object, slice<nint>, bool) rawLookupFieldOrMethod(this ptr<Checker> _addr_check, Type T, bool addressable, ptr<Package> _addr_pkg, @string name) {
    Object obj = default;
    slice<nint> index = default;
    bool indirect = default;
    ref Checker check = ref _addr_check.val;
    ref Package pkg = ref _addr_pkg.val;
 
    // WARNING: The code in this function is extremely subtle - do not modify casually!
    //          This function and NewMethodSet should be kept in sync.

    if (name == "_") {
        return ; // blank fields/methods are never found
    }
    var (typ, isPtr) = deref(T); 

    // *typ where typ is an interface has no methods.
    // Be cautious: typ may be nil (issue 39634, crash #3).
    if (typ == null || isPtr && IsInterface(typ)) {
        return ;
    }
    embeddedType current = new slice<embeddedType>(new embeddedType[] { {typ,nil,isPtr,false} }); 

    // Named types that we have seen already, allocated lazily.
    // Used to avoid endless searches in case of recursive types.
    // Since only Named types can be used for recursive types, we
    // only need to track those.
    // (If we ever allow type aliases to construct recursive types,
    // we must use type identity rather than pointer equality for
    // the map key comparison, as we do in consolidateMultiples.)
    map<ptr<Named>, bool> seen = default; 

    // search current depth
    while (len(current) > 0) {
        slice<embeddedType> next = default; // embedded types found at current depth

        // look for (pkg, name) in all types at current depth
        ptr<TypeParam> tpar; // set if obj receiver is a type parameter
        foreach (var (_, e) in current) {
            var typ = e.typ; 

            // If we have a named type, we may have associated methods.
            // Look for those first.
            {
                var named = asNamed(typ);

                if (named != null) {
                    if (seen[named]) { 
                        // We have seen this type before, at a more shallow depth
                        // (note that multiples of this type at the current depth
                        // were consolidated before). The type at that depth shadows
                        // this same type at the current depth, so we can ignore
                        // this one.
                        continue;
                    }
                    if (seen == null) {
                        seen = make_map<ptr<Named>, bool>();
                    }
                    seen[named] = true; 

                    // look for a matching attached method
                    {
                        var i__prev2 = i;

                        var (i, m) = lookupMethod(named.methods, _addr_pkg, name);

                        if (m != null) { 
                            // potential match
                            // caution: method may not have a proper signature yet
                            index = concat(e.index, i);
                            if (obj != null || e.multiples) {
                                return (null, index, false); // collision
                            }
                            obj = m;
                            indirect = e.indirect;
                            continue; // we can't have a matching field or interface method
                        } 

                        // continue with underlying type, but only if it's not a type parameter
                        // TODO(gri) is this what we want to do for type parameters? (spec question)

                        i = i__prev2;

                    } 

                    // continue with underlying type, but only if it's not a type parameter
                    // TODO(gri) is this what we want to do for type parameters? (spec question)
                    typ = named.under();
                    if (asTypeParam(typ) != null) {
                        continue;
                    }
                }

            }

            tpar = null;
            switch (typ.type()) {
                case ptr<Struct> t:
                    {
                        var i__prev3 = i;
                        var f__prev3 = f;

                        foreach (var (__i, __f) in t.fields) {
                            i = __i;
                            f = __f;
                            if (f.sameId(pkg, name)) {
                                assert(f.typ != null);
                                index = concat(e.index, i);
                                if (obj != null || e.multiples) {
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
                            if (obj == null && f.embedded) {
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
                case ptr<Interface> t:
                    check.completeInterface(nopos, t);
                    {
                        var i__prev1 = i;

                        (i, m) = lookupMethod(t.allMethods, _addr_pkg, name);

                        if (m != null) {
                            assert(m.typ != null);
                            index = concat(e.index, i);
                            if (obj != null || e.multiples) {
                                return (null, index, false); // collision
                            }
                            obj = m;
                            indirect = e.indirect;
                        }

                        i = i__prev1;

                    }
                    break;
                case ptr<TypeParam> t:
                    {
                        var i__prev1 = i;

                        (i, m) = lookupMethod(t.Bound().allMethods, _addr_pkg, name);

                        if (m != null) {
                            assert(m.typ != null);
                            index = concat(e.index, i);
                            if (obj != null || e.multiples) {
                                return (null, index, false); // collision
                            }
                            tpar = t;
                            obj = m;
                            indirect = e.indirect;
                        }

                        i = i__prev1;

                    }
                    if (obj == null) { 
                        // At this point we're not (yet) looking into methods
                        // that any underlying type of the types in the type list
                        // might have.
                        // TODO(gri) Do we want to specify the language that way?
                    }
                    break;
            }
        }        if (obj != null) { 
            // found a potential match
            // spec: "A method call x.m() is valid if the method set of (the type of) x
            //        contains m and the argument list can be assigned to the parameter
            //        list of m. If x is addressable and &x's method set contains m, x.m()
            //        is shorthand for (&x).m()".
            {
                var f__prev2 = f;

                ptr<Func> (f, _) = obj._<ptr<Func>>();

                if (f != null) { 
                    // determine if method has a pointer receiver
                    var hasPtrRecv = tpar == null && ptrRecv(_addr_f);
                    if (hasPtrRecv && !indirect && !addressable) {
                        return (null, null, true); // pointer/addressable receiver required
                    }
                }

                f = f__prev2;

            }
            return ;
        }
        current = check.consolidateMultiples(next);
    }

    return (null, null, false); // not found
}

// embeddedType represents an embedded type
private partial struct embeddedType {
    public Type typ;
    public slice<nint> index; // embedded field indices, starting with index at depth 0
    public bool indirect; // if set, there was a pointer indirection on the path to this field
    public bool multiples; // if set, typ appears multiple times at this depth
}

// consolidateMultiples collects multiple list entries with the same type
// into a single entry marked as containing multiples. The result is the
// consolidated list.
private static slice<embeddedType> consolidateMultiples(this ptr<Checker> _addr_check, slice<embeddedType> list) {
    ref Checker check = ref _addr_check.val;

    if (len(list) <= 1) {
        return list; // at most one entry - nothing to do
    }
    nint n = 0; // number of entries w/ unique type
    var prev = make_map<Type, nint>(); // index at which type was previously seen
    foreach (var (_, e) in list) {
        {
            var (i, found) = check.lookupType(prev, e.typ);

            if (found) {
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
    }    return list[..(int)n];
}

private static (nint, bool) lookupType(this ptr<Checker> _addr_check, map<Type, nint> m, Type typ) {
    nint _p0 = default;
    bool _p0 = default;
    ref Checker check = ref _addr_check.val;
 
    // fast path: maybe the types are equal
    {
        var i__prev1 = i;

        var (i, found) = m[typ];

        if (found) {
            return (i, true);
        }
        i = i__prev1;

    }

    {
        var i__prev1 = i;

        foreach (var (__t, __i) in m) {
            t = __t;
            i = __i;
            if (check.identical(t, typ)) {
                return (i, true);
            }
        }
        i = i__prev1;
    }

    return (0, false);
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
public static (ptr<Func>, bool) MissingMethod(Type V, ptr<Interface> _addr_T, bool @static) {
    ptr<Func> method = default!;
    bool wrongType = default;
    ref Interface T = ref _addr_T.val;

    var (m, typ) = (Checker.val)(null).missingMethod(V, T, static);
    return (_addr_m!, typ != null);
}

// missingMethod is like MissingMethod but accepts a *Checker as
// receiver and an addressable flag.
// The receiver may be nil if missingMethod is invoked through
// an exported API call (such as MissingMethod), i.e., when all
// methods have been type-checked.
// If the type has the correctly named method, but with the wrong
// signature, the existing method is returned as well.
// To improve error messages, also report the wrong signature
// when the method exists on *V instead of V.
private static (ptr<Func>, ptr<Func>) missingMethod(this ptr<Checker> _addr_check, Type V, ptr<Interface> _addr_T, bool @static) {
    ptr<Func> method = default!;
    ptr<Func> wrongType = default!;
    ref Checker check = ref _addr_check.val;
    ref Interface T = ref _addr_T.val;

    check.completeInterface(nopos, T); 

    // fast path for common case
    if (T.Empty()) {
        return ;
    }
    {
        var ityp = asInterface(V);

        if (ityp != null) {
            check.completeInterface(nopos, ityp); 
            // TODO(gri) allMethods is sorted - can do this more efficiently
            {
                var m__prev1 = m;

                foreach (var (_, __m) in T.allMethods) {
                    m = __m;
                    var (_, f) = lookupMethod(ityp.allMethods, _addr_m.pkg, m.name);

                    if (f == null) { 
                        // if m is the magic method == we're ok (interfaces are comparable)
                        if (m.name == "==" || !static) {
                            continue;
                        }
                        return (_addr_m!, _addr_f!);
                    } 

                    // both methods must have the same number of type parameters
                    ptr<Signature> ftyp = f.typ._<ptr<Signature>>();
                    ptr<Signature> mtyp = m.typ._<ptr<Signature>>();
                    if (len(ftyp.tparams) != len(mtyp.tparams)) {
                        return (_addr_m!, _addr_f!);
                    } 

                    // If the methods have type parameters we don't care whether they
                    // are the same or not, as long as they match up. Use unification
                    // to see if they can be made to match.
                    // TODO(gri) is this always correct? what about type bounds?
                    // (Alternative is to rename/subst type parameters and compare.)
                    var u = newUnifier(check, true);
                    u.x.init(ftyp.tparams);
                    if (!u.unify(ftyp, mtyp)) {
                        return (_addr_m!, _addr_f!);
                    }
                }

                m = m__prev1;
            }

            return ;
        }
    } 

    // A concrete type implements T if it implements all methods of T.
    var (Vd, _) = deref(V);
    var Vn = asNamed(Vd);
    {
        var m__prev1 = m;

        foreach (var (_, __m) in T.allMethods) {
            m = __m; 
            // TODO(gri) should this be calling lookupFieldOrMethod instead (and why not)?
            var (obj, _, _) = check.rawLookupFieldOrMethod(V, false, m.pkg, m.name); 

            // Check if *V implements this method of T.
            if (obj == null) {
                var ptr = NewPointer(V);
                obj, _, _ = check.rawLookupFieldOrMethod(ptr, false, m.pkg, m.name);
                if (obj != null) {
                    return (_addr_m!, obj._<ptr<Func>>());
                }
            } 

            // we must have a method (not a field of matching function type)
            ptr<Func> (f, _) = obj._<ptr<Func>>();
            if (f == null) { 
                // if m is the magic method == and V is comparable, we're ok
                if (m.name == "==" && Comparable(V)) {
                    continue;
                }
                return (_addr_m!, _addr_null!);
            } 

            // methods may not have a fully set up signature yet
            if (check != null) {
                check.objDecl(f, null);
            } 

            // both methods must have the same number of type parameters
            ftyp = f.typ._<ptr<Signature>>();
            mtyp = m.typ._<ptr<Signature>>();
            if (len(ftyp.tparams) != len(mtyp.tparams)) {
                return (_addr_m!, _addr_f!);
            } 

            // If V is a (instantiated) generic type, its methods are still
            // parameterized using the original (declaration) receiver type
            // parameters (subst simply copies the existing method list, it
            // does not instantiate the methods).
            // In order to compare the signatures, substitute the receiver
            // type parameters of ftyp with V's instantiation type arguments.
            // This lazily instantiates the signature of method f.
            if (Vn != null && len(Vn.tparams) > 0) { 
                // Be careful: The number of type arguments may not match
                // the number of receiver parameters. If so, an error was
                // reported earlier but the length discrepancy is still
                // here. Exit early in this case to prevent an assertion
                // failure in makeSubstMap.
                // TODO(gri) Can we avoid this check by fixing the lengths?
                if (len(ftyp.rparams) != len(Vn.targs)) {
                    return ;
                }
                ftyp = check.subst(nopos, ftyp, makeSubstMap(ftyp.rparams, Vn.targs))._<ptr<Signature>>();
            } 

            // If the methods have type parameters we don't care whether they
            // are the same or not, as long as they match up. Use unification
            // to see if they can be made to match.
            // TODO(gri) is this always correct? what about type bounds?
            // (Alternative is to rename/subst type parameters and compare.)
            u = newUnifier(check, true);
            u.x.init(ftyp.tparams);
            if (!u.unify(ftyp, mtyp)) {
                return (_addr_m!, _addr_f!);
            }
        }
        m = m__prev1;
    }

    return ;
}

// assertableTo reports whether a value of type V can be asserted to have type T.
// It returns (nil, false) as affirmative answer. Otherwise it returns a missing
// method required by V and whether it is missing or just has the wrong type.
// The receiver may be nil if assertableTo is invoked through an exported API call
// (such as AssertableTo), i.e., when all methods have been type-checked.
// If the global constant forceStrict is set, assertions that are known to fail
// are not permitted.
private static (ptr<Func>, ptr<Func>) assertableTo(this ptr<Checker> _addr_check, ptr<Interface> _addr_V, Type T) {
    ptr<Func> method = default!;
    ptr<Func> wrongType = default!;
    ref Checker check = ref _addr_check.val;
    ref Interface V = ref _addr_V.val;
 
    // no static check is required if T is an interface
    // spec: "If T is an interface type, x.(T) asserts that the
    //        dynamic type of x implements the interface T."
    if (asInterface(T) != null && !forceStrict) {
        return ;
    }
    return _addr_check.missingMethod(T, V, false)!;
}

// deref dereferences typ if it is a *Pointer and returns its base and true.
// Otherwise it returns (typ, false).
private static (Type, bool) deref(Type typ) {
    Type _p0 = default;
    bool _p0 = default;

    {
        ptr<Pointer> (p, _) = typ._<ptr<Pointer>>();

        if (p != null) {
            return (p.@base, true);
        }
    }
    return (typ, false);
}

// derefStructPtr dereferences typ if it is a (named or unnamed) pointer to a
// (named or unnamed) struct and returns its base. Otherwise it returns typ.
private static Type derefStructPtr(Type typ) {
    {
        var p = asPointer(typ);

        if (p != null) {
            if (asStruct(p.@base) != null) {
                return p.@base;
            }
        }
    }
    return typ;
}

// concat returns the result of concatenating list and i.
// The result does not share its underlying array with list.
private static slice<nint> concat(slice<nint> list, nint i) {
    slice<nint> t = default;
    t = append(t, list);
    return append(t, i);
}

// fieldIndex returns the index for the field with matching package and name, or a value < 0.
private static nint fieldIndex(slice<ptr<Var>> fields, ptr<Package> _addr_pkg, @string name) {
    ref Package pkg = ref _addr_pkg.val;

    if (name != "_") {
        foreach (var (i, f) in fields) {
            if (f.sameId(pkg, name)) {
                return i;
            }
        }
    }
    return -1;
}

// lookupMethod returns the index of and method with matching package and name, or (-1, nil).
private static (nint, ptr<Func>) lookupMethod(slice<ptr<Func>> methods, ptr<Package> _addr_pkg, @string name) {
    nint _p0 = default;
    ptr<Func> _p0 = default!;
    ref Package pkg = ref _addr_pkg.val;

    if (name != "_") {
        foreach (var (i, m) in methods) {
            if (m.sameId(pkg, name)) {
                return (i, _addr_m!);
            }
        }
    }
    return (-1, _addr_null!);
}

// ptrRecv reports whether the receiver is of the form *T.
private static bool ptrRecv(ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;
 
    // If a method's receiver type is set, use that as the source of truth for the receiver.
    // Caution: Checker.funcDecl (decl.go) marks a function by setting its type to an empty
    // signature. We may reach here before the signature is fully set up: we must explicitly
    // check if the receiver is set (we cannot just look for non-nil f.typ).
    {
        ptr<Signature> (sig, _) = f.typ._<ptr<Signature>>();

        if (sig != null && sig.recv != null) {
            var (_, isPtr) = deref(sig.recv.typ);
            return isPtr;
        }
    } 

    // If a method's type is not set it may be a method/function that is:
    // 1) client-supplied (via NewFunc with no signature), or
    // 2) internally created but not yet type-checked.
    // For case 1) we can't do anything; the client must know what they are doing.
    // For case 2) we can use the information gathered by the resolver.
    return f.hasPtrRecv;
}

} // end types2_package
