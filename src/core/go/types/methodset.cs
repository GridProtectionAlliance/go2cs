// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements method sets.
namespace go.go;

using fmt = fmt_package;
using sort = sort_package;
using strings = strings_package;

partial class types_package {

// A MethodSet is an ordered set of concrete or abstract (interface) methods;
// a method is a [MethodVal] selection, and they are ordered by ascending m.Obj().Id().
// The zero value for a MethodSet is a ready-to-use empty method set.
[GoType] partial struct MethodSet {
    internal slice<ж<Selection>> list;
}

[GoRecv] public static @string String(this ref MethodSet s) {
    if (s.Len() == 0) {
        return "MethodSet {}"u8;
    }
    ref var buf = ref heap(new strings_package.Builder(), out var Ꮡbuf);
    fmt.Fprintln(~Ꮡbuf, "MethodSet {");
    foreach (var (_, f) in s.list) {
        fmt.Fprintf(~Ꮡbuf, "\t%s\n"u8, f);
    }
    fmt.Fprintln(~Ꮡbuf, "}");
    return buf.String();
}

// Len returns the number of methods in s.
[GoRecv] public static nint Len(this ref MethodSet s) {
    return len(s.list);
}

// At returns the i'th method in s for 0 <= i < s.Len().
[GoRecv] public static ж<Selection> At(this ref MethodSet s, nint i) {
    return s.list[i];
}

// Lookup returns the method with matching package and name, or nil if not found.
[GoRecv] public static ж<Selection> Lookup(this ref MethodSet s, ж<Package> Ꮡpkg, @string name) {
    ref var pkg = ref Ꮡpkg.val;

    if (s.Len() == 0) {
        return default!;
    }
    @string key = Id(Ꮡpkg, name);
    nint i = sort.Search(len(s.list), (nint i) => {
        var m = s.list[iΔ1];
        return (~m).obj.Id() >= key;
    });
    if (i < len(s.list)) {
        var m = s.list[i];
        if ((~m).obj.Id() == key) {
            return m;
        }
    }
    return default!;
}

// Shared empty method set.
internal static MethodSet emptyMethodSet;

// Note: NewMethodSet is intended for external use only as it
//       requires interfaces to be complete. It may be used
//       internally if LookupFieldOrMethod completed the same
//       interfaces beforehand.

// NewMethodSet returns the method set for the given type T.
// It always returns a non-nil method set, even if it is empty.
public static ж<MethodSet> NewMethodSet(ΔType T) {
    // WARNING: The code in this function is extremely subtle - do not modify casually!
    //          This function and lookupFieldOrMethod should be kept in sync.
    // TODO(rfindley) confirm that this code is in sync with lookupFieldOrMethod
    //                with respect to type params.
    // Methods cannot be associated with a named pointer type.
    // (spec: "The type denoted by T is called the receiver base type;
    // it must not be a pointer or interface type and it must be declared
    // in the same package as the method.").
    {
        var t = asNamed(T); if (t != nil && isPointer(~t)) {
            return Ꮡ(emptyMethodSet);
        }
    }
    // method set up to the current depth, allocated lazily
    methodSet @base = default!;
    var (typ, isPtr) = deref(T);
    // *typ where typ is an interface has no methods.
    if (isPtr && IsInterface(typ)) {
        return Ꮡ(emptyMethodSet);
    }
    // Start with typ as single entry at shallowest depth.
    var current = new embeddedType[]{new(typ, default!, isPtr, false)}.slice();
    // seen tracks named types that we have seen already, allocated lazily.
    // Used to avoid endless searches in case of recursive types.
    //
    // We must use a lookup on identity rather than a simple map[*Named]bool as
    // instantiated types may be identical but not equal.
    instanceLookup seen = default!;
    // collect methods at current depth
    while (len(current) > 0) {
        slice<embeddedType> next = default!;                          // embedded types found at current depth
        // field and method sets at current depth, indexed by names (Id's), and allocated lazily
        map<@string, bool> fset = default!;                            // we only care about the field names
        methodSet mset = default!;
        foreach (var (_, e) in current) {
            var typΔ1 = e.typ;
            // If we have a named type, we may have associated methods.
            // Look for those first.
            {
                var named = asNamed(typΔ1); if (named != nil) {
                    {
                        var alt = seen.lookup(named); if (alt != nil) {
                            // We have seen this type before, at a more shallow depth
                            // (note that multiples of this type at the current depth
                            // were consolidated before). The type at that depth shadows
                            // this same type at the current depth, so we can ignore
                            // this one.
                            continue;
                        }
                    }
                    seen.add(named);
                    for (nint i = 0; i < named.NumMethods(); i++) {
                        mset = mset.addOne(named.Method(i), concat(e.index, i), e.indirect, e.multiples);
                    }
                }
            }
            switch (under(typ).type()) {
            case Struct.val t: {
                foreach (var (i, f) in (~t).fields) {
                    if (fset == default!) {
                        fset = new map<@string, bool>();
                    }
                    fset[f.Id()] = true;
                    // Embedded fields are always of the form T or *T where
                    // T is a type name. If typ appeared multiple times at
                    // this depth, f.Type appears multiple times at the next
                    // depth.
                    if ((~f).embedded) {
                        var (typΔ2, isPtrΔ1) = deref(f.typ);
                        // TODO(gri) optimization: ignore types that can't
                        // have fields or methods (only Named, Struct, and
                        // Interface types need to be considered).
                        next = append(next, new embeddedType(typΔ2, concat(e.index, i), e.indirect || isPtrΔ1, e.multiples));
                    }
                }
                break;
            }
            case Interface.val t: {
                mset = mset.add((~t.typeSet()).methods, e.index, true, e.multiples);
                break;
            }}
        }
        // Add methods and collisions at this depth to base if no entries with matching
        // names exist already.
        foreach (var (k, m) in mset) {
            {
                var _ = @base[k];
                var found = @base[k]; if (!found) {
                    // Fields collide with methods of the same name at this depth.
                    if (fset[k]) {
                        m = default!;
                    }
                    // collision
                    if (@base == default!) {
                        @base = new methodSet();
                    }
                    @base[k] = m;
                }
            }
        }
        // Add all (remaining) fields at this depth as collisions (since they will
        // hide any method further down) if no entries with matching names exist already.
        foreach (var (k, _) in fset) {
            {
                var _ = @base[k];
                var found = @base[k]; if (!found) {
                    if (@base == default!) {
                        @base = new methodSet();
                    }
                    @base[k] = default!;
                }
            }
        }
        // collision
        current = consolidateMultiples(next);
    }
    if (len(@base) == 0) {
        return Ꮡ(emptyMethodSet);
    }
    // collect methods
    slice<ж<Selection>> list = default!;
    foreach (var (_, m) in @base) {
        if (m != nil) {
            m.val.recv = T;
            list = append(list, m);
        }
    }
    // sort by unique name
    sort.Slice(list, 
    var listʗ1 = list;
    (nint i, nint j) => (~listʗ1[i]).obj.Id() < (~listʗ1[j]).obj.Id());
    return Ꮡ(new MethodSet(list));
}
/* visitMapType: map[string]*Selection */

// Add adds all functions in list to the method set s.
// If multiples is set, every function in list appears multiple times
// and is treated as a collision.
internal static methodSet add(this methodSet s, slice<ж<Func>> list, slice<nint> index, bool indirect, bool multiples) {
    if (len(list) == 0) {
        return s;
    }
    foreach (var (i, f) in list) {
        s = s.addOne(f, concat(index, i), indirect, multiples);
    }
    return s;
}

internal static methodSet addOne(this methodSet s, ж<Func> Ꮡf, slice<nint> index, bool indirect, bool multiples) {
    ref var f = ref Ꮡf.val;

    if (s == default!) {
        s = new methodSet();
    }
    @string key = f.Id();
    // if f is not in the set, add it
    if (!multiples) {
        // TODO(gri) A found method may not be added because it's not in the method set
        // (!indirect && f.hasPtrRecv()). A 2nd method on the same level may be in the method
        // set and may not collide with the first one, thus leading to a false positive.
        // Is that possible? Investigate.
        {
            var _ = s[key];
            var found = s[key]; if (!found && (indirect || !f.hasPtrRecv())) {
                s[key] = Ꮡ(new Selection(MethodVal, default!, f, index, indirect));
                return s;
            }
        }
    }
    s[key] = default!;
    // collision
    return s;
}

} // end types_package
