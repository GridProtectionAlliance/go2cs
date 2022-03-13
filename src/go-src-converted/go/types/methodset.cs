// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements method sets.

// package types -- go2cs converted at 2022 March 13 05:53:12 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\methodset.go
namespace go.go;

using fmt = fmt_package;
using sort = sort_package;
using strings = strings_package;


// A MethodSet is an ordered set of concrete or abstract (interface) methods;
// a method is a MethodVal selection, and they are ordered by ascending m.Obj().Id().
// The zero value for a MethodSet is a ready-to-use empty method set.

using System;
public static partial class types_package {

public partial struct MethodSet {
    public slice<ptr<Selection>> list;
}

private static @string String(this ptr<MethodSet> _addr_s) {
    ref MethodSet s = ref _addr_s.val;

    if (s.Len() == 0) {
        return "MethodSet {}";
    }
    ref strings.Builder buf = ref heap(out ptr<strings.Builder> _addr_buf);
    fmt.Fprintln(_addr_buf, "MethodSet {");
    foreach (var (_, f) in s.list) {
        fmt.Fprintf(_addr_buf, "\t%s\n", f);
    }    fmt.Fprintln(_addr_buf, "}");
    return buf.String();
}

// Len returns the number of methods in s.
private static nint Len(this ptr<MethodSet> _addr_s) {
    ref MethodSet s = ref _addr_s.val;

    return len(s.list);
}

// At returns the i'th method in s for 0 <= i < s.Len().
private static ptr<Selection> At(this ptr<MethodSet> _addr_s, nint i) {
    ref MethodSet s = ref _addr_s.val;

    return _addr_s.list[i]!;
}

// Lookup returns the method with matching package and name, or nil if not found.
private static ptr<Selection> Lookup(this ptr<MethodSet> _addr_s, ptr<Package> _addr_pkg, @string name) {
    ref MethodSet s = ref _addr_s.val;
    ref Package pkg = ref _addr_pkg.val;

    if (s.Len() == 0) {
        return _addr_null!;
    }
    var key = Id(pkg, name);
    var i = sort.Search(len(s.list), i => {
        var m = s.list[i];
        return _addr_m.obj.Id() >= key!;
    });
    if (i < len(s.list)) {
        m = s.list[i];
        if (m.obj.Id() == key) {
            return _addr_m!;
        }
    }
    return _addr_null!;
}

// Shared empty method set.
private static MethodSet emptyMethodSet = default;

// Note: NewMethodSet is intended for external use only as it
//       requires interfaces to be complete. It may be used
//       internally if LookupFieldOrMethod completed the same
//       interfaces beforehand.

// NewMethodSet returns the method set for the given type T.
// It always returns a non-nil method set, even if it is empty.
public static ptr<MethodSet> NewMethodSet(Type T) { 
    // WARNING: The code in this function is extremely subtle - do not modify casually!
    //          This function and lookupFieldOrMethod should be kept in sync.

    // TODO(rfindley) confirm that this code is in sync with lookupFieldOrMethod
    //                with respect to type params.

    // method set up to the current depth, allocated lazily
    methodSet @base = default;

    var (typ, isPtr) = deref(T); 

    // *typ where typ is an interface has no methods.
    if (isPtr && IsInterface(typ)) {
        return _addr__addr_emptyMethodSet!;
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

    // collect methods at current depth
    while (len(current) > 0) {
        slice<embeddedType> next = default; // embedded types found at current depth

        // field and method sets at current depth, indexed by names (Id's), and allocated lazily
        map<@string, bool> fset = default; // we only care about the field names
        methodSet mset = default;

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

                    mset = mset.add(named.methods, e.index, e.indirect, e.multiples); 

                    // continue with underlying type, but only if it's not a type parameter
                    // TODO(rFindley): should this use named.under()? Can there be a difference?
                    typ = named.underlying;
                    {
                        ptr<_TypeParam> (_, ok) = typ._<ptr<_TypeParam>>();

                        if (ok) {
                            continue;
                        }

                    }
                }

            }

            switch (typ.type()) {
                case ptr<Struct> t:
                    foreach (var (i, f) in t.fields) {
                        if (fset == null) {
                            fset = make_map<@string, bool>();
                        }
                        fset[f.Id()] = true; 

                        // Embedded fields are always of the form T or *T where
                        // T is a type name. If typ appeared multiple times at
                        // this depth, f.Type appears multiple times at the next
                        // depth.
                        if (f.embedded) {
                            (typ, isPtr) = deref(f.typ); 
                            // TODO(gri) optimization: ignore types that can't
                            // have fields or methods (only Named, Struct, and
                            // Interface types need to be considered).
                            next = append(next, new embeddedType(typ,concat(e.index,i),e.indirect||isPtr,e.multiples));
                        }
                    }
                    break;
                case ptr<Interface> t:
                    mset = mset.add(t.allMethods, e.index, true, e.multiples);
                    break;
                case ptr<_TypeParam> t:
                    mset = mset.add(t.Bound().allMethods, e.index, true, e.multiples);
                    break;
            }
        }        {
            var k__prev2 = k;
            var m__prev2 = m;

            foreach (var (__k, __m) in mset) {
                k = __k;
                m = __m;
                {
                    var (_, found) = base[k];

                    if (!found) { 
                        // Fields collide with methods of the same name at this depth.
                        if (fset[k]) {
                            m = null; // collision
                        }
                        if (base == null) {
                            base = make(methodSet);
                        }
                        base[k] = m;
                    }

                }
            } 

            // Add all (remaining) fields at this depth as collisions (since they will
            // hide any method further down) if no entries with matching names exist already.

            k = k__prev2;
            m = m__prev2;
        }

        {
            var k__prev2 = k;

            foreach (var (__k) in fset) {
                k = __k;
                {
                    (_, found) = base[k];

                    if (!found) {
                        if (base == null) {
                            base = make(methodSet);
                        }
                        base[k] = null; // collision
                    }

                }
            } 

            // It's ok to call consolidateMultiples with a nil *Checker because
            // MethodSets are not used internally (outside debug mode). When used
            // externally, interfaces are expected to be completed and then we do
            // not need a *Checker to complete them when (indirectly) calling
            // Checker.identical via consolidateMultiples.

            k = k__prev2;
        }

        current = (Checker.val)(null).consolidateMultiples(next);
    }

    if (len(base) == 0) {
        return _addr__addr_emptyMethodSet!;
    }
    slice<ptr<Selection>> list = default;
    {
        var m__prev1 = m;

        foreach (var (_, __m) in base) {
            m = __m;
            if (m != null) {
                m.recv = T;
                list = append(list, m);
            }
        }
        m = m__prev1;
    }

    sort.Slice(list, (i, j) => _addr_list[i].obj.Id() < list[j].obj.Id()!);
    return addr(new MethodSet(list));
}

// A methodSet is a set of methods and name collisions.
// A collision indicates that multiple methods with the
// same unique id, or a field with that id appeared.
private partial struct methodSet { // : map<@string, ptr<Selection>>
} // a nil entry indicates a name collision

// Add adds all functions in list to the method set s.
// If multiples is set, every function in list appears multiple times
// and is treated as a collision.
private static methodSet add(this methodSet s, slice<ptr<Func>> list, slice<nint> index, bool indirect, bool multiples) {
    if (len(list) == 0) {
        return s;
    }
    if (s == null) {
        s = make(methodSet);
    }
    foreach (var (i, f) in list) {
        var key = f.Id(); 
        // if f is not in the set, add it
        if (!multiples) { 
            // TODO(gri) A found method may not be added because it's not in the method set
            // (!indirect && ptrRecv(f)). A 2nd method on the same level may be in the method
            // set and may not collide with the first one, thus leading to a false positive.
            // Is that possible? Investigate.
            {
                var (_, found) = s[key];

                if (!found && (indirect || !ptrRecv(_addr_f))) {
                    s[key] = addr(new Selection(MethodVal,nil,f,concat(index,i),indirect));
                    continue;
                }

            }
        }
        s[key] = null; // collision
    }    return s;
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

} // end types_package
