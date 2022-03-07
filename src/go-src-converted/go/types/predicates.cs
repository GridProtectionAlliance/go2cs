// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements commonly used type predicates.

// package types -- go2cs converted at 2022 March 06 22:42:08 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\predicates.go
using token = go.go.token_package;
using System;


namespace go.go;

public static partial class types_package {

    // isNamed reports whether typ has a name.
    // isNamed may be called with types that are not fully set up.
private static bool isNamed(Type typ) {
    switch (typ.type()) {
        case ptr<Basic> _:
            return true;
            break;
        case ptr<Named> _:
            return true;
            break;
        case ptr<_TypeParam> _:
            return true;
            break;
        case ptr<instance> _:
            return true;
            break;
    }
    return false;

}

// isGeneric reports whether a type is a generic, uninstantiated type (generic
// signatures are not included).
private static bool isGeneric(Type typ) { 
    // A parameterized type is only instantiated if it doesn't have an instantiation already.
    ptr<Named> (named, _) = typ._<ptr<Named>>();
    return named != null && named.obj != null && named.tparams != null && named.targs == null;

}

private static bool @is(Type typ, BasicInfo what) {
    switch (optype(typ).type()) {
        case ptr<Basic> t:
            return t.info & what != 0;
            break;
        case ptr<_Sum> t:
            return t.@is(typ => is(typ, what));
            break;
    }
    return false;

}

private static bool isBoolean(Type typ) {
    return is(typ, IsBoolean);
}
private static bool isInteger(Type typ) {
    return is(typ, IsInteger);
}
private static bool isUnsigned(Type typ) {
    return is(typ, IsUnsigned);
}
private static bool isFloat(Type typ) {
    return is(typ, IsFloat);
}
private static bool isComplex(Type typ) {
    return is(typ, IsComplex);
}
private static bool isNumeric(Type typ) {
    return is(typ, IsNumeric);
}
private static bool isString(Type typ) {
    return is(typ, IsString);
}

// Note that if typ is a type parameter, isInteger(typ) || isFloat(typ) does not
// produce the expected result because a type list that contains both an integer
// and a floating-point type is neither (all) integers, nor (all) floats.
// Use isIntegerOrFloat instead.
private static bool isIntegerOrFloat(Type typ) {
    return is(typ, IsInteger | IsFloat);
}

// isNumericOrString is the equivalent of isIntegerOrFloat for isNumeric(typ) || isString(typ).
private static bool isNumericOrString(Type typ) {
    return is(typ, IsNumeric | IsString);
}

// isTyped reports whether typ is typed; i.e., not an untyped
// constant or boolean. isTyped may be called with types that
// are not fully set up.
private static bool isTyped(Type typ) { 
    // isTyped is called with types that are not fully
    // set up. Must not call asBasic()!
    // A *Named or *instance type is always typed, so
    // we only need to check if we have a true *Basic
    // type.
    ptr<Basic> (t, _) = typ._<ptr<Basic>>();
    return t == null || t.info & IsUntyped == 0;

}

// isUntyped(typ) is the same as !isTyped(typ).
private static bool isUntyped(Type typ) {
    return !isTyped(typ);
}

private static bool isOrdered(Type typ) {
    return is(typ, IsOrdered);
}

private static bool isConstType(Type typ) { 
    // Type parameters are never const types.
    ptr<Basic> (t, _) = under(typ)._<ptr<Basic>>();
    return t != null && t.info & IsConstType != 0;

}

// IsInterface reports whether typ is an interface type.
public static bool IsInterface(Type typ) {
    return asInterface(typ) != null;
}

// Comparable reports whether values of type T are comparable.
public static bool Comparable(Type T) {
    return comparable(T, null);
}

private static bool comparable(Type T, map<Type, bool> seen) {
    if (seen[T]) {
        return true;
    }
    if (seen == null) {
        seen = make_map<Type, bool>();
    }
    seen[T] = true; 

    // If T is a type parameter not constrained by any type
    // list (i.e., it's underlying type is the top type),
    // T is comparable if it has the == method. Otherwise,
    // the underlying type "wins". For instance
    //
    //     interface{ comparable; type []byte }
    //
    // is not comparable because []byte is not comparable.
    {
        var t__prev1 = t;

        var t = asTypeParam(T);

        if (t != null && optype(t) == theTop) {
            return t.Bound()._IsComparable();
        }
        t = t__prev1;

    }


    switch (optype(T).type()) {
        case ptr<Basic> t:
            return t.kind != UntypedNil;
            break;
        case ptr<Pointer> t:
            return true;
            break;
        case ptr<Interface> t:
            return true;
            break;
        case ptr<Chan> t:
            return true;
            break;
        case ptr<Struct> t:
            foreach (var (_, f) in t.fields) {
                if (!comparable(f.typ, seen)) {
                    return false;
                }
            }
            return true;
            break;
        case ptr<Array> t:
            return comparable(t.elem, seen);
            break;
        case ptr<_Sum> t:
            Func<Type, bool> pred = t => {
                return comparable(t, seen);
            }
;
            return t.@is(pred);
            break;
        case ptr<_TypeParam> t:
            return t.Bound()._IsComparable();
            break;
    }
    return false;

}

// hasNil reports whether a type includes the nil value.
private static bool hasNil(Type typ) {
    switch (optype(typ).type()) {
        case ptr<Basic> t:
            return t.kind == UnsafePointer;
            break;
        case ptr<Slice> t:
            return true;
            break;
        case ptr<Pointer> t:
            return true;
            break;
        case ptr<Signature> t:
            return true;
            break;
        case ptr<Interface> t:
            return true;
            break;
        case ptr<Map> t:
            return true;
            break;
        case ptr<Chan> t:
            return true;
            break;
        case ptr<_Sum> t:
            return t.@is(hasNil);
            break;
    }
    return false;

}

// identical reports whether x and y are identical types.
// Receivers of Signature types are ignored.
private static bool identical(this ptr<Checker> _addr_check, Type x, Type y) {
    ref Checker check = ref _addr_check.val;

    return check.identical0(x, y, true, null);
}

// identicalIgnoreTags reports whether x and y are identical types if tags are ignored.
// Receivers of Signature types are ignored.
private static bool identicalIgnoreTags(this ptr<Checker> _addr_check, Type x, Type y) {
    ref Checker check = ref _addr_check.val;

    return check.identical0(x, y, false, null);
}

// An ifacePair is a node in a stack of interface type pairs compared for identity.
private partial struct ifacePair {
    public ptr<Interface> x;
    public ptr<Interface> y;
    public ptr<ifacePair> prev;
}

private static bool identical(this ptr<ifacePair> _addr_p, ptr<ifacePair> _addr_q) {
    ref ifacePair p = ref _addr_p.val;
    ref ifacePair q = ref _addr_q.val;

    return p.x == q.x && p.y == q.y || p.x == q.y && p.y == q.x;
}

// For changes to this code the corresponding changes should be made to unifier.nify.
private static bool identical0(this ptr<Checker> _addr_check, Type x, Type y, bool cmpTags, ptr<ifacePair> _addr_p) {
    ref Checker check = ref _addr_check.val;
    ref ifacePair p = ref _addr_p.val;
 
    // types must be expanded for comparison
    x = expandf(x);
    y = expandf(y);

    if (x == y) {
        return true;
    }
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
                    return (x.len < 0 || y.len < 0 || x.len == y.len) && check.identical0(x.elem, y.elem, cmpTags, p);

                }

                y = y__prev1;

            }


            break;
        case ptr<Slice> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Slice>>();

                if (ok) {
                    return check.identical0(x.elem, y.elem, cmpTags, p);
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
                                if (f.embedded != g.embedded || cmpTags && x.Tag(i) != y.Tag(i) || !f.sameId(g.pkg, g.name) || !check.identical0(f.typ, g.typ, cmpTags, p)) {
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
                    return check.identical0(x.@base, y.@base, cmpTags, p);
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
                                    if (!check.identical0(v.typ, w.typ, cmpTags, p)) {
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
                    return x.variadic == y.variadic && check.identicalTParams(x.tparams, y.tparams, cmpTags, p) && check.identical0(x.@params, y.@params, cmpTags, p) && check.identical0(x.results, y.results, cmpTags, p);
                }

                y = y__prev1;

            }


            break;
        case ptr<_Sum> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<_Sum>>();

                if (ok && len(x.types) == len(y.types)) { 
                    // Every type in x.types must be in y.types.
                    // Quadratic algorithm, but probably good enough for now.
                    // TODO(gri) we need a fast quick type ID/hash for all types.
L:
                    {
                        var x__prev1 = x;

                        foreach (var (_, __x) in x.types) {
                            x = __x;
                            {
                                ptr<Basic> y__prev2 = y;

                                foreach (var (_, __y) in y.types) {
                                    y = __y;
                                    if (Identical(x, y)) {
                                        _continueL = true; // x is in y.types
                                        break;
                                    }

                                }

                                y = y__prev2;
                            }

                            return false; // x is not in y.types
                        }

                        x = x__prev1;
                    }
                    return true;

                }

                y = y__prev1;

            }


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
                    if (check != null) {
                        check.completeInterface(token.NoPos, x);
                        check.completeInterface(token.NoPos, y);
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
                            assertSortedMethods(a);
                            assertSortedMethods(b);
                        }

                        {
                            var i__prev1 = i;
                            var f__prev1 = f;

                            foreach (var (__i, __f) in a) {
                                i = __i;
                                f = __f;
                                g = b[i];
                                if (f.Id() != g.Id() || !check.identical0(f.typ, g.typ, cmpTags, q)) {
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
                    return check.identical0(x.key, y.key, cmpTags, p) && check.identical0(x.elem, y.elem, cmpTags, p);
                }

                y = y__prev1;

            }


            break;
        case ptr<Chan> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Chan>>();

                if (ok) {
                    return x.dir == y.dir && check.identical0(x.elem, y.elem, cmpTags, p);
                }

                y = y__prev1;

            }


            break;
        case ptr<Named> x:
            {
                ptr<Basic> y__prev1 = y;

                (y, ok) = y._<ptr<Named>>();

                if (ok) { 
                    // TODO(gri) Why is x == y not sufficient? And if it is,
                    //           we can just return false here because x == y
                    //           is caught in the very beginning of this function.
                    return x.obj == y.obj;

                }

                y = y__prev1;

            }


            break;
        case ptr<_TypeParam> x:
            break;
        case ptr<bottom> x:
            break;
        case ptr<top> x:
            break;
        case 
            break;
        default:
        {
            var x = x.type();
            unreachable();
            break;
        }

    }

    return false;

}

private static bool identicalTParams(this ptr<Checker> _addr_check, slice<ptr<TypeName>> x, slice<ptr<TypeName>> y, bool cmpTags, ptr<ifacePair> _addr_p) {
    ref Checker check = ref _addr_check.val;
    ref ifacePair p = ref _addr_p.val;

    if (len(x) != len(y)) {
        return false;
    }
    foreach (var (i, x) in x) {
        var y = y[i];
        if (!check.identical0(x.typ._<ptr<_TypeParam>>().bound, y.typ._<ptr<_TypeParam>>().bound, cmpTags, p)) {
            return false;
        }
    }    return true;

}

// Default returns the default "typed" type for an "untyped" type;
// it returns the incoming type for all other types. The default type
// for untyped nil is untyped nil.
//
public static Type Default(Type typ) {
    {
        ptr<Basic> (t, ok) = typ._<ptr<Basic>>();

        if (ok) {

            if (t.kind == UntypedBool) 
                return Typ[Bool];
            else if (t.kind == UntypedInt) 
                return Typ[Int];
            else if (t.kind == UntypedRune) 
                return universeRune; // use 'rune' name
            else if (t.kind == UntypedFloat) 
                return Typ[Float64];
            else if (t.kind == UntypedComplex) 
                return Typ[Complex128];
            else if (t.kind == UntypedString) 
                return Typ[String];
            
        }
    }

    return typ;

}

} // end types_package
