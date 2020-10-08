// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements commonly used type predicates.

// package types -- go2cs converted at 2020 October 08 04:03:37 UTC
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
                ptr<Basic> (_, ok) = typ._<ptr<Basic>>();

                if (ok)
                {
                    return ok;
                }
            }

            (_, ok) = typ._<ptr<Named>>();
            return ok;

        }

        private static bool isBoolean(Type typ)
        {
            ptr<Basic> (t, ok) = typ.Underlying()._<ptr<Basic>>();
            return ok && t.info & IsBoolean != 0L;
        }

        private static bool isInteger(Type typ)
        {
            ptr<Basic> (t, ok) = typ.Underlying()._<ptr<Basic>>();
            return ok && t.info & IsInteger != 0L;
        }

        private static bool isUnsigned(Type typ)
        {
            ptr<Basic> (t, ok) = typ.Underlying()._<ptr<Basic>>();
            return ok && t.info & IsUnsigned != 0L;
        }

        private static bool isFloat(Type typ)
        {
            ptr<Basic> (t, ok) = typ.Underlying()._<ptr<Basic>>();
            return ok && t.info & IsFloat != 0L;
        }

        private static bool isComplex(Type typ)
        {
            ptr<Basic> (t, ok) = typ.Underlying()._<ptr<Basic>>();
            return ok && t.info & IsComplex != 0L;
        }

        private static bool isNumeric(Type typ)
        {
            ptr<Basic> (t, ok) = typ.Underlying()._<ptr<Basic>>();
            return ok && t.info & IsNumeric != 0L;
        }

        private static bool isString(Type typ)
        {
            ptr<Basic> (t, ok) = typ.Underlying()._<ptr<Basic>>();
            return ok && t.info & IsString != 0L;
        }

        private static bool isTyped(Type typ)
        {
            ptr<Basic> (t, ok) = typ.Underlying()._<ptr<Basic>>();
            return !ok || t.info & IsUntyped == 0L;
        }

        private static bool isUntyped(Type typ)
        {
            ptr<Basic> (t, ok) = typ.Underlying()._<ptr<Basic>>();
            return ok && t.info & IsUntyped != 0L;
        }

        private static bool isOrdered(Type typ)
        {
            ptr<Basic> (t, ok) = typ.Underlying()._<ptr<Basic>>();
            return ok && t.info & IsOrdered != 0L;
        }

        private static bool isConstType(Type typ)
        {
            ptr<Basic> (t, ok) = typ.Underlying()._<ptr<Basic>>();
            return ok && t.info & IsConstType != 0L;
        }

        // IsInterface reports whether typ is an interface type.
        public static bool IsInterface(Type typ)
        {
            ptr<Interface> (_, ok) = typ.Underlying()._<ptr<Interface>>();
            return ok;
        }

        // Comparable reports whether values of type T are comparable.
        public static bool Comparable(Type T)
        {
            switch (T.Underlying().type())
            {
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
                    foreach (var (_, f) in t.fields)
                    {
                        if (!Comparable(f.typ))
                        {
                            return false;
                        }

                    }
                    return true;
                    break;
                case ptr<Array> t:
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
            }
            return false;

        }

        // identical reports whether x and y are identical types.
        // Receivers of Signature types are ignored.
        private static bool identical(this ptr<Checker> _addr_check, Type x, Type y)
        {
            ref Checker check = ref _addr_check.val;

            return check.identical0(x, y, true, null);
        }

        // identicalIgnoreTags reports whether x and y are identical types if tags are ignored.
        // Receivers of Signature types are ignored.
        private static bool identicalIgnoreTags(this ptr<Checker> _addr_check, Type x, Type y)
        {
            ref Checker check = ref _addr_check.val;

            return check.identical0(x, y, false, null);
        }

        // An ifacePair is a node in a stack of interface type pairs compared for identity.
        private partial struct ifacePair
        {
            public ptr<Interface> x;
            public ptr<Interface> y;
            public ptr<ifacePair> prev;
        }

        private static bool identical(this ptr<ifacePair> _addr_p, ptr<ifacePair> _addr_q)
        {
            ref ifacePair p = ref _addr_p.val;
            ref ifacePair q = ref _addr_q.val;

            return p.x == q.x && p.y == q.y || p.x == q.y && p.y == q.x;
        }

        private static bool identical0(this ptr<Checker> _addr_check, Type x, Type y, bool cmpTags, ptr<ifacePair> _addr_p)
        {
            ref Checker check = ref _addr_check.val;
            ref ifacePair p = ref _addr_p.val;

            if (x == y)
            {
                return true;
            }

            switch (x.type())
            {
                case ptr<Basic> x:
                    {
                        ptr<Basic> y__prev1 = y;

                        ptr<Basic> (y, ok) = y._<ptr<Basic>>();

                        if (ok)
                        {
                            return x.kind == y.kind;
                        }

                        y = y__prev1;

                    }


                    break;
                case ptr<Array> x:
                    {
                        ptr<Basic> y__prev1 = y;

                        (y, ok) = y._<ptr<Array>>();

                        if (ok)
                        { 
                            // If one or both array lengths are unknown (< 0) due to some error,
                            // assume they are the same to avoid spurious follow-on errors.
                            return (x.len < 0L || y.len < 0L || x.len == y.len) && check.identical0(x.elem, y.elem, cmpTags, p);

                        }

                        y = y__prev1;

                    }


                    break;
                case ptr<Slice> x:
                    {
                        ptr<Basic> y__prev1 = y;

                        (y, ok) = y._<ptr<Slice>>();

                        if (ok)
                        {
                            return check.identical0(x.elem, y.elem, cmpTags, p);
                        }

                        y = y__prev1;

                    }


                    break;
                case ptr<Struct> x:
                    {
                        ptr<Basic> y__prev1 = y;

                        (y, ok) = y._<ptr<Struct>>();

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
                                        if (f.embedded != g.embedded || cmpTags && x.Tag(i) != y.Tag(i) || !f.sameId(g.pkg, g.name) || !check.identical0(f.typ, g.typ, cmpTags, p))
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
                case ptr<Pointer> x:
                    {
                        ptr<Basic> y__prev1 = y;

                        (y, ok) = y._<ptr<Pointer>>();

                        if (ok)
                        {
                            return check.identical0(x.@base, y.@base, cmpTags, p);
                        }

                        y = y__prev1;

                    }


                    break;
                case ptr<Tuple> x:
                    {
                        ptr<Basic> y__prev1 = y;

                        (y, ok) = y._<ptr<Tuple>>();

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
                                            if (!check.identical0(v.typ, w.typ, cmpTags, p))
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
                case ptr<Signature> x:
                    {
                        ptr<Basic> y__prev1 = y;

                        (y, ok) = y._<ptr<Signature>>();

                        if (ok)
                        {
                            return x.variadic == y.variadic && check.identical0(x.@params, y.@params, cmpTags, p) && check.identical0(x.results, y.results, cmpTags, p);
                        }

                        y = y__prev1;

                    }


                    break;
                case ptr<Interface> x:
                    {
                        ptr<Basic> y__prev1 = y;

                        (y, ok) = y._<ptr<Interface>>();

                        if (ok)
                        { 
                            // If identical0 is called (indirectly) via an external API entry point
                            // (such as Identical, IdenticalIgnoreTags, etc.), check is nil. But in
                            // that case, interfaces are expected to be complete and lazy completion
                            // here is not needed.
                            if (check != null)
                            {
                                check.completeInterface(x);
                                check.completeInterface(y);
                            }

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
                                ptr<ifacePair> q = addr(new ifacePair(x,y,p));
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
                                        if (f.Id() != g.Id() || !check.identical0(f.typ, g.typ, cmpTags, q))
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
                case ptr<Map> x:
                    {
                        ptr<Basic> y__prev1 = y;

                        (y, ok) = y._<ptr<Map>>();

                        if (ok)
                        {
                            return check.identical0(x.key, y.key, cmpTags, p) && check.identical0(x.elem, y.elem, cmpTags, p);
                        }

                        y = y__prev1;

                    }


                    break;
                case ptr<Chan> x:
                    {
                        ptr<Basic> y__prev1 = y;

                        (y, ok) = y._<ptr<Chan>>();

                        if (ok)
                        {
                            return x.dir == y.dir && check.identical0(x.elem, y.elem, cmpTags, p);
                        }

                        y = y__prev1;

                    }


                    break;
                case ptr<Named> x:
                    {
                        ptr<Basic> y__prev1 = y;

                        (y, ok) = y._<ptr<Named>>();

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
                ptr<Basic> (t, ok) = typ._<ptr<Basic>>();

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
