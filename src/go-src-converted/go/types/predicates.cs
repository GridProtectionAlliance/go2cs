// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements commonly used type predicates.

// package types -- go2cs converted at 2020 August 29 08:47:49 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\predicates.go
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        private static bool isNamed(Type typ)
        {
            {
                ref Basic (_, ok) = typ._<ref Basic>();

                if (ok)
                {
                    return ok;
                }
            }
            (_, ok) = typ._<ref Named>();
            return ok;
        }

        private static bool isBoolean(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.info & IsBoolean != 0L;
        }

        private static bool isInteger(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.info & IsInteger != 0L;
        }

        private static bool isUnsigned(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.info & IsUnsigned != 0L;
        }

        private static bool isFloat(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.info & IsFloat != 0L;
        }

        private static bool isComplex(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.info & IsComplex != 0L;
        }

        private static bool isNumeric(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.info & IsNumeric != 0L;
        }

        private static bool isString(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.info & IsString != 0L;
        }

        private static bool isTyped(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return !ok || t.info & IsUntyped == 0L;
        }

        private static bool isUntyped(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.info & IsUntyped != 0L;
        }

        private static bool isOrdered(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.info & IsOrdered != 0L;
        }

        private static bool isConstType(Type typ)
        {
            ref Basic (t, ok) = typ.Underlying()._<ref Basic>();
            return ok && t.info & IsConstType != 0L;
        }

        // IsInterface reports whether typ is an interface type.
        public static bool IsInterface(Type typ)
        {
            ref Interface (_, ok) = typ.Underlying()._<ref Interface>();
            return ok;
        }

        // Comparable reports whether values of type T are comparable.
        public static bool Comparable(Type T)
        {
            switch (T.Underlying().type())
            {
                case ref Basic t:
                    return t.kind != UntypedNil;
                    break;
                case ref Pointer t:
                    return true;
                    break;
                case ref Interface t:
                    return true;
                    break;
                case ref Chan t:
                    return true;
                    break;
                case ref Struct t:
                    foreach (var (_, f) in t.fields)
                    {
                        if (!Comparable(f.typ))
                        {
                            return false;
                        }
                    }
                    return true;
                    break;
                case ref Array t:
                    return Comparable(t.elem);
                    break;
            }
            return false;
        }

        // hasNil reports whether a type includes the nil value.
        private static bool hasNil(Type typ)
        {
            switch (typ.Underlying().type())
            {
                case ref Basic t:
                    return t.kind == UnsafePointer;
                    break;
                case ref Slice t:
                    return true;
                    break;
                case ref Pointer t:
                    return true;
                    break;
                case ref Signature t:
                    return true;
                    break;
                case ref Interface t:
                    return true;
                    break;
                case ref Map t:
                    return true;
                    break;
                case ref Chan t:
                    return true;
                    break;
            }
            return false;
        }

        // Identical reports whether x and y are identical types.
        // Receivers of Signature types are ignored.
        public static bool Identical(Type x, Type y)
        {
            return identical(x, y, true, null);
        }

        // IdenticalIgnoreTags reports whether x and y are identical types if tags are ignored.
        // Receivers of Signature types are ignored.
        public static bool IdenticalIgnoreTags(Type x, Type y)
        {
            return identical(x, y, false, null);
        }

        // An ifacePair is a node in a stack of interface type pairs compared for identity.
        private partial struct ifacePair
        {
            public ptr<Interface> x;
            public ptr<Interface> y;
            public ptr<ifacePair> prev;
        }

        private static bool identical(this ref ifacePair p, ref ifacePair q)
        {
            return p.x == q.x && p.y == q.y || p.x == q.y && p.y == q.x;
        }

        private static bool identical(Type x, Type y, bool cmpTags, ref ifacePair p)
        {
            if (x == y)
            {
                return true;
            }
            switch (x.type())
            {
                case ref Basic x:
                    {
                        ref Basic y__prev1 = y;

                        ref Basic (y, ok) = y._<ref Basic>();

                        if (ok)
                        {
                            return x.kind == y.kind;
                        }

                        y = y__prev1;

                    }
                    break;
                case ref Array x:
                    {
                        ref Basic y__prev1 = y;

                        (y, ok) = y._<ref Array>();

                        if (ok)
                        {
                            return x.len == y.len && identical(x.elem, y.elem, cmpTags, p);
                        }

                        y = y__prev1;

                    }
                    break;
                case ref Slice x:
                    {
                        ref Basic y__prev1 = y;

                        (y, ok) = y._<ref Slice>();

                        if (ok)
                        {
                            return identical(x.elem, y.elem, cmpTags, p);
                        }

                        y = y__prev1;

                    }
                    break;
                case ref Struct x:
                    {
                        ref Basic y__prev1 = y;

                        (y, ok) = y._<ref Struct>();

                        if (ok)
                        {
                            if (x.NumFields() == y.NumFields())
                            {
                                {
                                    var i__prev1 = i;
                                    var f__prev1 = f;

                                    foreach (var (__i, __f) in x.fields)
                                    {
                                        i = __i;
                                        f = __f;
                                        var g = y.fields[i];
                                        if (f.anonymous != g.anonymous || cmpTags && x.Tag(i) != y.Tag(i) || !f.sameId(g.pkg, g.name) || !identical(f.typ, g.typ, cmpTags, p))
                                        {
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
                case ref Pointer x:
                    {
                        ref Basic y__prev1 = y;

                        (y, ok) = y._<ref Pointer>();

                        if (ok)
                        {
                            return identical(x.@base, y.@base, cmpTags, p);
                        }

                        y = y__prev1;

                    }
                    break;
                case ref Tuple x:
                    {
                        ref Basic y__prev1 = y;

                        (y, ok) = y._<ref Tuple>();

                        if (ok)
                        {
                            if (x.Len() == y.Len())
                            {
                                if (x != null)
                                {
                                    {
                                        var i__prev1 = i;

                                        foreach (var (__i, __v) in x.vars)
                                        {
                                            i = __i;
                                            v = __v;
                                            var w = y.vars[i];
                                            if (!identical(v.typ, w.typ, cmpTags, p))
                                            {
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
                case ref Signature x:
                    {
                        ref Basic y__prev1 = y;

                        (y, ok) = y._<ref Signature>();

                        if (ok)
                        {
                            return x.variadic == y.variadic && identical(x.@params, y.@params, cmpTags, p) && identical(x.results, y.results, cmpTags, p);
                        }

                        y = y__prev1;

                    }
                    break;
                case ref Interface x:
                    {
                        ref Basic y__prev1 = y;

                        (y, ok) = y._<ref Interface>();

                        if (ok)
                        {
                            var a = x.allMethods;
                            var b = y.allMethods;
                            if (len(a) == len(b))
                            { 
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
                                ifacePair q = ref new ifacePair(x,y,p);
                                while (p != null)
                                {
                                    if (p.identical(q))
                                    {
                                        return true; // same pair was compared before
                                    }
                                    p = p.prev;
                                }

                                if (debug)
                                {
                                    assert(sort.IsSorted(byUniqueMethodName(a)));
                                    assert(sort.IsSorted(byUniqueMethodName(b)));
                                }
                                {
                                    var i__prev1 = i;
                                    var f__prev1 = f;

                                    foreach (var (__i, __f) in a)
                                    {
                                        i = __i;
                                        f = __f;
                                        g = b[i];
                                        if (f.Id() != g.Id() || !identical(f.typ, g.typ, cmpTags, q))
                                        {
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
                case ref Map x:
                    {
                        ref Basic y__prev1 = y;

                        (y, ok) = y._<ref Map>();

                        if (ok)
                        {
                            return identical(x.key, y.key, cmpTags, p) && identical(x.elem, y.elem, cmpTags, p);
                        }

                        y = y__prev1;

                    }
                    break;
                case ref Chan x:
                    {
                        ref Basic y__prev1 = y;

                        (y, ok) = y._<ref Chan>();

                        if (ok)
                        {
                            return x.dir == y.dir && identical(x.elem, y.elem, cmpTags, p);
                        }

                        y = y__prev1;

                    }
                    break;
                case ref Named x:
                    {
                        ref Basic y__prev1 = y;

                        (y, ok) = y._<ref Named>();

                        if (ok)
                        {
                            return x.obj == y.obj;
                        }

                        y = y__prev1;

                    }
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

        // Default returns the default "typed" type for an "untyped" type;
        // it returns the incoming type for all other types. The default type
        // for untyped nil is untyped nil.
        //
        public static Type Default(Type typ)
        {
            {
                ref Basic (t, ok) = typ._<ref Basic>();

                if (ok)
                {

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
    }
}}
