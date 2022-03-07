// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types2 -- go2cs converted at 2022 March 06 23:12:50 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\sanitize.go


namespace go.cmd.compile.@internal;

public static partial class types2_package {

    // sanitizeInfo walks the types contained in info to ensure that all instances
    // are expanded.
    //
    // This includes some objects that may be shared across concurrent
    // type-checking passes (such as those in the universe scope), so we are
    // careful here not to write types that are already sanitized. This avoids a
    // data race as any shared types should already be sanitized.
private static void sanitizeInfo(ptr<Info> _addr_info) {
    ref Info info = ref _addr_info.val;

    sanitizer s = make_map<Type, Type>(); 

    // Note: Some map entries are not references.
    // If modified, they must be assigned back.

    {
        var e__prev1 = e;

        foreach (var (__e, __tv) in info.Types) {
            e = __e;
            tv = __tv;
            {
                var typ__prev1 = typ;

                var typ = s.typ(tv.Type);

                if (typ != tv.Type) {
                    tv.Type = typ;
                    info.Types[e] = tv;
                }
                typ = typ__prev1;

            }

        }
        e = e__prev1;
    }

    {
        var e__prev1 = e;

        foreach (var (__e, __inf) in info.Inferred) {
            e = __e;
            inf = __inf;
            var changed = false;
            foreach (var (i, targ) in inf.Targs) {
                {
                    var typ__prev1 = typ;

                    typ = s.typ(targ);

                    if (typ != targ) {
                        inf.Targs[i] = typ;
                        changed = true;
                    }
                    typ = typ__prev1;

                }

            }            {
                var typ__prev1 = typ;

                typ = s.typ(inf.Sig);

                if (typ != inf.Sig) {
                    inf.Sig = typ._<ptr<Signature>>();
                    changed = true;
                }
                typ = typ__prev1;

            }

            if (changed) {
                info.Inferred[e] = inf;
            }
        }
        e = e__prev1;
    }

    {
        var obj__prev1 = obj;

        foreach (var (_, __obj) in info.Defs) {
            obj = __obj;
            if (obj != null) {
                {
                    var typ__prev2 = typ;

                    typ = s.typ(obj.Type());

                    if (typ != obj.Type()) {
                        obj.setType(typ);
                    }
                    typ = typ__prev2;

                }

            }
        }
        obj = obj__prev1;
    }

    {
        var obj__prev1 = obj;

        foreach (var (_, __obj) in info.Uses) {
            obj = __obj;
            if (obj != null) {
                {
                    var typ__prev2 = typ;

                    typ = s.typ(obj.Type());

                    if (typ != obj.Type()) {
                        obj.setType(typ);
                    }
                    typ = typ__prev2;

                }

            }
        }
        obj = obj__prev1;
    }
}

private partial struct sanitizer { // : map<Type, Type>
}

private static Type typ(this sanitizer s, Type typ) => func((_, panic, _) => {
    if (typ == null) {
        return null;
    }
    {
        var t__prev1 = t;

        var (t, found) = s[typ];

        if (found) {
            return t;
        }
        t = t__prev1;

    }

    s[typ] = typ;

    switch (typ.type()) {
        case ptr<Basic> t:
            break;
        case ptr<bottom> t:
            break;
        case ptr<top> t:
            break;
        case ptr<Array> t:
            {
                var elem__prev1 = elem;

                var elem = s.typ(t.elem);

                if (elem != t.elem) {
                    t.elem = elem;
                }

                elem = elem__prev1;

            }


            break;
        case ptr<Slice> t:
            {
                var elem__prev1 = elem;

                elem = s.typ(t.elem);

                if (elem != t.elem) {
                    t.elem = elem;
                }

                elem = elem__prev1;

            }


            break;
        case ptr<Struct> t:
            s.varList(t.fields);
            break;
        case ptr<Pointer> t:
            {
                var @base = s.typ(t.@base);

                if (base != t.@base) {
                    t.@base = base;
                }

            }


            break;
        case ptr<Tuple> t:
            s.tuple(t);
            break;
        case ptr<Signature> t:
            s.var_(t.recv);
            s.tuple(t.@params);
            s.tuple(t.results);
            break;
        case ptr<Sum> t:
            s.typeList(t.types);
            break;
        case ptr<Interface> t:
            s.funcList(t.methods);
            {
                var types = s.typ(t.types);

                if (types != t.types) {
                    t.types = types;
                }

            }

            s.typeList(t.embeddeds);
            s.funcList(t.allMethods);
            {
                var allTypes = s.typ(t.allTypes);

                if (allTypes != t.allTypes) {
                    t.allTypes = allTypes;
                }

            }


            break;
        case ptr<Map> t:
            {
                var key = s.typ(t.key);

                if (key != t.key) {
                    t.key = key;
                }

            }

            {
                var elem__prev1 = elem;

                elem = s.typ(t.elem);

                if (elem != t.elem) {
                    t.elem = elem;
                }

                elem = elem__prev1;

            }


            break;
        case ptr<Chan> t:
            {
                var elem__prev1 = elem;

                elem = s.typ(t.elem);

                if (elem != t.elem) {
                    t.elem = elem;
                }

                elem = elem__prev1;

            }


            break;
        case ptr<Named> t:
            {
                var orig = s.typ(t.fromRHS);

                if (orig != t.fromRHS) {
                    t.fromRHS = orig;
                }

            }

            {
                var under = s.typ(t.underlying);

                if (under != t.underlying) {
                    t.underlying = under;
                }

            }

            s.typeList(t.targs);
            s.funcList(t.methods);
            break;
        case ptr<TypeParam> t:
            {
                var bound = s.typ(t.bound);

                if (bound != t.bound) {
                    t.bound = bound;
                }

            }


            break;
        case ptr<instance> t:
            typ = t.expand();
            s[t] = typ;
            break;
        default:
        {
            var t = typ.type();
            panic("unimplemented");
            break;
        }

    }

    return typ;

});

private static void var_(this sanitizer s, ptr<Var> _addr_v) {
    ref Var v = ref _addr_v.val;

    if (v != null) {
        {
            var typ = s.typ(v.typ);

            if (typ != v.typ) {
                v.typ = typ;
            }

        }

    }
}

private static void varList(this sanitizer s, slice<ptr<Var>> list) {
    foreach (var (_, v) in list) {
        s.var_(v);
    }
}

private static void tuple(this sanitizer s, ptr<Tuple> _addr_t) {
    ref Tuple t = ref _addr_t.val;

    if (t != null) {
        s.varList(t.vars);
    }
}

private static void func_(this sanitizer s, ptr<Func> _addr_f) {
    ref Func f = ref _addr_f.val;

    if (f != null) {
        {
            var typ = s.typ(f.typ);

            if (typ != f.typ) {
                f.typ = typ;
            }

        }

    }
}

private static void funcList(this sanitizer s, slice<ptr<Func>> list) {
    foreach (var (_, f) in list) {
        s.func_(f);
    }
}

private static void typeList(this sanitizer s, slice<Type> list) {
    foreach (var (i, t) in list) {
        {
            var typ = s.typ(t);

            if (typ != t) {
                list[i] = typ;
            }

        }

    }
}

} // end types2_package
