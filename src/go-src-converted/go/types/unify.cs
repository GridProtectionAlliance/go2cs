// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements type unification.

// package types -- go2cs converted at 2022 March 13 05:53:40 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\unify.go
namespace go.go;

using bytes = bytes_package;
using token = go.token_package;
using sort = sort_package;


// The unifier maintains two separate sets of type parameters x and y
// which are used to resolve type parameters in the x and y arguments
// provided to the unify call. For unidirectional unification, only
// one of these sets (say x) is provided, and then type parameters are
// only resolved for the x argument passed to unify, not the y argument
// (even if that also contains possibly the same type parameters). This
// is crucial to infer the type parameters of self-recursive calls:
//
//    func f[P any](a P) { f(a) }
//
// For the call f(a) we want to infer that the type argument for P is P.
// During unification, the parameter type P must be resolved to the type
// parameter P ("x" side), but the argument type P must be left alone so
// that unification resolves the type parameter P to P.
//
// For bidirection unification, both sets are provided. This enables
// unification to go from argument to parameter type and vice versa.
// For constraint type inference, we use bidirectional unification
// where both the x and y type parameters are identical. This is done
// by setting up one of them (using init) and then assigning its value
// to the other.

// A unifier maintains the current type parameters for x and y
// and the respective types inferred for each type parameter.
// A unifier is created by calling newUnifier.

public static partial class types_package {

private partial struct unifier {
    public ptr<Checker> check;
    public bool exact;
    public tparamsList x; // x and y must initialized via tparamsList.init
    public tparamsList y; // x and y must initialized via tparamsList.init
    public slice<Type> types; // inferred types, shared by x and y
}

// newUnifier returns a new unifier.
// If exact is set, unification requires unified types to match
// exactly. If exact is not set, a named type's underlying type
// is considered if unification would fail otherwise, and the
// direction of channels is ignored.
private static ptr<unifier> newUnifier(ptr<Checker> _addr_check, bool exact) {
    ref Checker check = ref _addr_check.val;

    ptr<unifier> u = addr(new unifier(check:check,exact:exact));
    u.x.unifier = u;
    u.y.unifier = u;
    return _addr_u!;
}

// unify attempts to unify x and y and reports whether it succeeded.
private static bool unify(this ptr<unifier> _addr_u, Type x, Type y) {
    ref unifier u = ref _addr_u.val;

    return u.nify(x, y, null);
}

// A tparamsList describes a list of type parameters and the types inferred for them.
private partial struct tparamsList {
    public ptr<unifier> unifier;
    public slice<ptr<TypeName>> tparams; // For each tparams element, there is a corresponding type slot index in indices.
// index  < 0: unifier.types[-index-1] == nil
// index == 0: no type slot allocated yet
// index  > 0: unifier.types[index-1] == typ
// Joined tparams elements share the same type slot and thus have the same index.
// By using a negative index for nil types we don't need to check unifier.types
// to see if we have a type or not.
    public slice<nint> indices; // len(d.indices) == len(d.tparams)
}

// String returns a string representation for a tparamsList. For debugging.
private static @string String(this ptr<tparamsList> _addr_d) {
    ref tparamsList d = ref _addr_d.val;

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    buf.WriteByte('[');
    foreach (var (i, tname) in d.tparams) {
        if (i > 0) {
            buf.WriteString(", ");
        }
        writeType(_addr_buf, tname.typ, null, null);
        buf.WriteString(": ");
        writeType(_addr_buf, d.at(i), null, null);
    }    buf.WriteByte(']');
    return buf.String();
}

// init initializes d with the given type parameters.
// The type parameters must be in the order in which they appear in their declaration
// (this ensures that the tparams indices match the respective type parameter index).
private static void init(this ptr<tparamsList> _addr_d, slice<ptr<TypeName>> tparams) {
    ref tparamsList d = ref _addr_d.val;

    if (len(tparams) == 0) {
        return ;
    }
    if (debug) {
        foreach (var (i, tpar) in tparams) {
            assert(i == tpar.typ._<ptr<_TypeParam>>().index);
        }
    }
    d.tparams = tparams;
    d.indices = make_slice<nint>(len(tparams));
}

// join unifies the i'th type parameter of x with the j'th type parameter of y.
// If both type parameters already have a type associated with them and they are
// not joined, join fails and return false.
private static bool join(this ptr<unifier> _addr_u, nint i, nint j) {
    ref unifier u = ref _addr_u.val;

    var ti = u.x.indices[i];
    var tj = u.y.indices[j];

    if (ti == 0 && tj == 0) 
        // Neither type parameter has a type slot associated with them.
        // Allocate a new joined nil type slot (negative index).
        u.types = append(u.types, null);
        u.x.indices[i] = -len(u.types);
        u.y.indices[j] = -len(u.types);
    else if (ti == 0) 
        // The type parameter for x has no type slot yet. Use slot of y.
        u.x.indices[i] = tj;
    else if (tj == 0) 
        // The type parameter for y has no type slot yet. Use slot of x.
        u.y.indices[j] = ti; 

        // Both type parameters have a slot: ti != 0 && tj != 0.
    else if (ti == tj) 
        // Both type parameters already share the same slot. Nothing to do.
        break;
    else if (ti > 0 && tj > 0) 
        // Both type parameters have (possibly different) inferred types. Cannot join.
        return false;
    else if (ti > 0) 
        // Only the type parameter for x has an inferred type. Use x slot for y.
        u.y.setIndex(j, ti);
    else 
        // Either the type parameter for y has an inferred type, or neither type
        // parameter has an inferred type. In either case, use y slot for x.
        u.x.setIndex(i, tj);
        return true;
}

// If typ is a type parameter of d, index returns the type parameter index.
// Otherwise, the result is < 0.
private static nint index(this ptr<tparamsList> _addr_d, Type typ) {
    ref tparamsList d = ref _addr_d.val;

    {
        ptr<_TypeParam> (t, ok) = typ._<ptr<_TypeParam>>();

        if (ok) {
            {
                var i = t.index;

                if (i < len(d.tparams) && d.tparams[i].typ == t) {
                    return i;
                }

            }
        }
    }
    return -1;
}

// setIndex sets the type slot index for the i'th type parameter
// (and all its joined parameters) to tj. The type parameter
// must have a (possibly nil) type slot associated with it.
private static void setIndex(this ptr<tparamsList> _addr_d, nint i, nint tj) {
    ref tparamsList d = ref _addr_d.val;

    var ti = d.indices[i];
    assert(ti != 0 && tj != 0);
    foreach (var (k, tk) in d.indices) {
        if (tk == ti) {
            d.indices[k] = tj;
        }
    }
}

// at returns the type set for the i'th type parameter; or nil.
private static Type at(this ptr<tparamsList> _addr_d, nint i) {
    ref tparamsList d = ref _addr_d.val;

    {
        var ti = d.indices[i];

        if (ti > 0) {
            return d.unifier.types[ti - 1];
        }
    }
    return null;
}

// set sets the type typ for the i'th type parameter;
// typ must not be nil and it must not have been set before.
private static void set(this ptr<tparamsList> _addr_d, nint i, Type typ) => func((_, panic, _) => {
    ref tparamsList d = ref _addr_d.val;

    assert(typ != null);
    var u = d.unifier;
    {
        var ti = d.indices[i];


        if (ti < 0) 
            u.types[-ti - 1] = typ;
            d.setIndex(i, -ti);
        else if (ti == 0) 
            u.types = append(u.types, typ);
            d.indices[i] = len(u.types);
        else 
            panic("type already set");

    }
});

// types returns the list of inferred types (via unification) for the type parameters
// described by d, and an index. If all types were inferred, the returned index is < 0.
// Otherwise, it is the index of the first type parameter which couldn't be inferred;
// i.e., for which list[index] is nil.
private static (slice<Type>, nint) types(this ptr<tparamsList> _addr_d) {
    slice<Type> list = default;
    nint index = default;
    ref tparamsList d = ref _addr_d.val;

    list = make_slice<Type>(len(d.tparams));
    index = -1;
    foreach (var (i) in d.tparams) {
        var t = d.at(i);
        list[i] = t;
        if (index < 0 && t == null) {
            index = i;
        }
    }    return ;
}

private static bool nifyEq(this ptr<unifier> _addr_u, Type x, Type y, ptr<ifacePair> _addr_p) {
    ref unifier u = ref _addr_u.val;
    ref ifacePair p = ref _addr_p.val;

    return x == y || u.nify(x, y, p);
}

// nify implements the core unification algorithm which is an
// adapted version of Checker.identical0. For changes to that
// code the corresponding changes should be made here.
// Must not be called directly from outside the unifier.
private static bool nify(this ptr<unifier> _addr_u, Type x, Type y, ptr<ifacePair> _addr_p) => func((_, panic, _) => {
    ref unifier u = ref _addr_u.val;
    ref ifacePair p = ref _addr_p.val;
 
    // types must be expanded for comparison
    x = expand(x);
    y = expand(y);

    if (!u.exact) { 
        // If exact unification is known to fail because we attempt to
        // match a type name against an unnamed type literal, consider
        // the underlying type of the named type.
        // (Subtle: We use isNamed to include any type with a name (incl.
        // basic types and type parameters. We use asNamed() because we only
        // want *Named types.)

        if (!isNamed(x) && y != null && asNamed(y) != null) 
            return u.nify(x, under(y), p);
        else if (x != null && asNamed(x) != null && !isNamed(y)) 
            return u.nify(under(x), y, p);
            }
    {
        var i__prev1 = i;

        var i = u.x.index(x);
        var j = u.y.index(y);


        if (i >= 0 && j >= 0) 
            // both x and y are type parameters
            if (u.join(i, j)) {
                return true;
            } 
            // both x and y have an inferred type - they must match
            return u.nifyEq(u.x.at(i), u.y.at(j), p);
        else if (i >= 0) 
            // x is a type parameter, y is not
            {
                var tx = u.x.at(i);

                if (tx != null) {
                    return u.nifyEq(tx, y, p);
                } 
                // otherwise, infer type from y

            } 
            // otherwise, infer type from y
            u.x.set(i, y);
            return true;
        else if (j >= 0) 
            // y is a type parameter, x is not
            {
                var ty = u.y.at(j);

                if (ty != null) {
                    return u.nifyEq(x, ty, p);
                } 
                // otherwise, infer type from x

            } 
            // otherwise, infer type from x
            u.y.set(j, x);
            return true;


        i = i__prev1;
    } 

    // For type unification, do not shortcut (x == y) for identical
    // types. Instead keep comparing them element-wise to unify the
    // matching (and equal type parameter types). A simple test case
    // where this matters is: func f[P any](a P) { f(a) } .

    switch (x.type()) {
        case ptr<Basic> x:
            {
                ptr<Basic> y__prev1 = y;

                ptr<Basic> (y, ok) = y._<ptr<Basic>>();

                if (ok) {
                    return x.kind == y.kind;
                }

                y = y__prev1;

            }
            break;
        case ptr<Array> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Array>>();

                if (ok) { 
                    // If one or both array lengths are unknown (< 0) due to some error,
                    // assume they are the same to avoid spurious follow-on errors.
                    return (x.len < 0 || y.len < 0 || x.len == y.len) && u.nify(x.elem, y.elem, p);
                }

                y = y__prev1;

            }
            break;
        case ptr<Slice> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Slice>>();

                if (ok) {
                    return u.nify(x.elem, y.elem, p);
                }

                y = y__prev1;

            }
            break;
        case ptr<Struct> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Struct>>();

                if (ok) {
                    if (x.NumFields() == y.NumFields()) {
                        {
                            var i__prev1 = i;
                            var f__prev1 = f;

                            foreach (var (__i, __f) in x.fields) {
                                i = __i;
                                f = __f;
                                var g = y.fields[i];
                                if (f.embedded != g.embedded || x.Tag(i) != y.Tag(i) || !f.sameId(g.pkg, g.name) || !u.nify(f.typ, g.typ, p)) {
                                    return false;
                                }
                            }

                            i = i__prev1;
                            f = f__prev1;
                        }

                        return true;
                    }
                }

                y = y__prev1;

            }
            break;
        case ptr<Pointer> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Pointer>>();

                if (ok) {
                    return u.nify(x.@base, y.@base, p);
                }

                y = y__prev1;

            }
            break;
        case ptr<Tuple> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Tuple>>();

                if (ok) {
                    if (x.Len() == y.Len()) {
                        if (x != null) {
                            {
                                var i__prev1 = i;

                                foreach (var (__i, __v) in x.vars) {
                                    i = __i;
                                    v = __v;
                                    var w = y.vars[i];
                                    if (!u.nify(v.typ, w.typ, p)) {
                                        return false;
                                    }
                                }

                                i = i__prev1;
                            }
                        }
                        return true;
                    }
                }

                y = y__prev1;

            }
            break;
        case ptr<Signature> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Signature>>();

                if (ok) {
                    return x.variadic == y.variadic && u.nify(x.@params, y.@params, p) && u.nify(x.results, y.results, p);
                }

                y = y__prev1;

            }
            break;
        case ptr<_Sum> x:
            panic("type inference across sum types not implemented");
            break;
        case ptr<Interface> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Interface>>();

                if (ok) { 
                    // If identical0 is called (indirectly) via an external API entry point
                    // (such as Identical, IdenticalIgnoreTags, etc.), check is nil. But in
                    // that case, interfaces are expected to be complete and lazy completion
                    // here is not needed.
                    if (u.check != null) {
                        u.check.completeInterface(token.NoPos, x);
                        u.check.completeInterface(token.NoPos, y);
                    }
                    var a = x.allMethods;
                    var b = y.allMethods;
                    if (len(a) == len(b)) { 
                        // Interface types are the only types where cycles can occur
                        // that are not "terminated" via named types; and such cycles
                        // can only be created via method parameter types that are
                        // anonymous interfaces (directly or indirectly) embedding
                        // the current interface. Example:
                        //
                        //    type T interface {
                        //        m() interface{T}
                        //    }
                        //
                        // If two such (differently named) interfaces are compared,
                        // endless recursion occurs if the cycle is not detected.
                        //
                        // If x and y were compared before, they must be equal
                        // (if they were not, the recursion would have stopped);
                        // search the ifacePair stack for the same pair.
                        //
                        // This is a quadratic algorithm, but in practice these stacks
                        // are extremely short (bounded by the nesting depth of interface
                        // type declarations that recur via parameter types, an extremely
                        // rare occurrence). An alternative implementation might use a
                        // "visited" map, but that is probably less efficient overall.
                        ptr<ifacePair> q = addr(new ifacePair(x,y,p));
                        while (p != null) {
                            if (p.identical(q)) {
                                return true; // same pair was compared before
                            }
                            p = p.prev;
                        }

                        if (debug) {
                            assert(sort.IsSorted(byUniqueMethodName(a)));
                            assert(sort.IsSorted(byUniqueMethodName(b)));
                        }
                        {
                            var i__prev1 = i;
                            var f__prev1 = f;

                            foreach (var (__i, __f) in a) {
                                i = __i;
                                f = __f;
                                g = b[i];
                                if (f.Id() != g.Id() || !u.nify(f.typ, g.typ, q)) {
                                    return false;
                                }
                            }

                            i = i__prev1;
                            f = f__prev1;
                        }

                        return true;
                    }
                }

                y = y__prev1;

            }
            break;
        case ptr<Map> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Map>>();

                if (ok) {
                    return u.nify(x.key, y.key, p) && u.nify(x.elem, y.elem, p);
                }

                y = y__prev1;

            }
            break;
        case ptr<Chan> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Chan>>();

                if (ok) {
                    return (!u.exact || x.dir == y.dir) && u.nify(x.elem, y.elem, p);
                }

                y = y__prev1;

            }
            break;
        case ptr<Named> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Named>>();

                if (ok) { 
                    // TODO(gri) This is not always correct: two types may have the same names
                    //           in the same package if one of them is nested in a function.
                    //           Extremely unlikely but we need an always correct solution.
                    if (x.obj.pkg == y.obj.pkg && x.obj.name == y.obj.name) {
                        assert(len(x.targs) == len(y.targs));
                        {
                            var i__prev1 = i;
                            var x__prev1 = x;

                            foreach (var (__i, __x) in x.targs) {
                                i = __i;
                                x = __x;
                                if (!u.nify(x, y.targs[i], p)) {
                                    return false;
                                }
                            }

                            i = i__prev1;
                            x = x__prev1;
                        }

                        return true;
                    }
                }

                y = y__prev1;

            }
            break;
        case ptr<_TypeParam> x:
            return x == y; 

            // case *instance:
            //    unreachable since types are expanded
            break;
        case 
            break;
        default:
        {
            var x = x.type();
            u.check.dump("### u.nify(%s, %s), u.x.tparams = %s", x, y, u.x.tparams);
            unreachable();
            break;
        }

    }

    return false;
});

} // end types_package
