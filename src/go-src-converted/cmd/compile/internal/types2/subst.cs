// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements instantiation of generic types
// through substitution of type parameters by actual
// types.

// package types2 -- go2cs converted at 2022 March 06 23:12:57 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\subst.go
using bytes = go.bytes_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using fmt = go.fmt_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class types2_package {

private partial struct substMap {
    public slice<Type> targs;
    public map<ptr<TypeParam>, Type> proj;
}

// makeSubstMap creates a new substitution map mapping tpars[i] to targs[i].
// If targs[i] is nil, tpars[i] is not substituted.
private static ptr<substMap> makeSubstMap(slice<ptr<TypeName>> tpars, slice<Type> targs) {
    assert(len(tpars) == len(targs));
    var proj = make_map<ptr<TypeParam>, Type>(len(tpars));
    foreach (var (i, tpar) in tpars) { 
        // We must expand type arguments otherwise *instance
        // types end up as components in composite types.
        // TODO(gri) explain why this causes problems, if it does
        var targ = expand(targs[i]); // possibly nil
        targs[i] = targ;
        proj[tpar.typ._<ptr<TypeParam>>()] = targ;

    }    return addr(new substMap(targs,proj));

}

private static @string String(this ptr<substMap> _addr_m) {
    ref substMap m = ref _addr_m.val;

    return fmt.Sprintf("%s", m.proj);
}

private static bool empty(this ptr<substMap> _addr_m) {
    ref substMap m = ref _addr_m.val;

    return len(m.proj) == 0;
}

private static Type lookup(this ptr<substMap> _addr_m, ptr<TypeParam> _addr_tpar) {
    ref substMap m = ref _addr_m.val;
    ref TypeParam tpar = ref _addr_tpar.val;

    {
        var t = m.proj[tpar];

        if (t != null) {
            return t;
        }
    }

    return tpar;

}

private static Type instantiate(this ptr<Checker> _addr_check, syntax.Pos pos, Type typ, slice<Type> targs, slice<syntax.Pos> poslist) => func((defer, _, _) => {
    Type res = default;
    ref Checker check = ref _addr_check.val;

    if (check.conf.Trace) {
        check.trace(pos, "-- instantiating %s with %s", typ, typeListString(targs));
        check.indent++;
        defer(() => {
            check.indent--;
            Type under = default;
            if (res != null) { 
                // Calling under() here may lead to endless instantiations.
                // Test case: type T[P any] T[P]
                // TODO(gri) investigate if that's a bug or to be expected.
                under = res.Underlying();

            }

            check.trace(pos, "=> %s (under = %s)", res, under);

        }());

    }
    assert(len(poslist) <= len(targs)); 

    // TODO(gri) What is better here: work with TypeParams, or work with TypeNames?
    slice<ptr<TypeName>> tparams = default;
    switch (typ.type()) {
        case ptr<Named> t:
            tparams = t.tparams;
            break;
        case ptr<Signature> t:
            tparams = t.tparams;
            defer(() => { 
                // If we had an unexpected failure somewhere don't panic below when
                // asserting res.(*Signature). Check for *Signature in case Typ[Invalid]
                // is returned.
                {
                    ptr<Signature> (_, ok) = res._<ptr<Signature>>();

                    if (!ok) {
                        return ;
                    } 
                    // If the signature doesn't use its type parameters, subst
                    // will not make a copy. In that case, make a copy now (so
                    // we can set tparams to nil w/o causing side-effects).

                } 
                // If the signature doesn't use its type parameters, subst
                // will not make a copy. In that case, make a copy now (so
                // we can set tparams to nil w/o causing side-effects).
                if (t == res) {
                    ref var copy = ref heap(t.val, out ptr<var> _addr_copy);
                    _addr_res = _addr_copy;
                    res = ref _addr_res.val;

                } 
                // After instantiating a generic signature, it is not generic
                // anymore; we need to set tparams to nil.
                res._<ptr<Signature>>().tparams = null;

            }());
            break;
        default:
        {
            var t = typ.type();
            check.dump("%v: cannot instantiate %v", pos, typ);
            unreachable(); // only defined types and (defined) functions can be generic
            break;
        } 

        // the number of supplied types must match the number of type parameters
    } 

    // the number of supplied types must match the number of type parameters
    if (len(targs) != len(tparams)) { 
        // TODO(gri) provide better error message
        check.errorf(pos, "got %d arguments but %d type parameters", len(targs), len(tparams));
        return Typ[Invalid];

    }
    if (len(tparams) == 0) {
        return typ; // nothing to do (minor optimization)
    }
    var smap = makeSubstMap(tparams, targs); 

    // check bounds
    foreach (var (i, tname) in tparams) {
        ptr<TypeParam> tpar = tname.typ._<ptr<TypeParam>>();
        var iface = tpar.Bound();
        if (iface.Empty()) {
            continue; // no type bound
        }
        var targ = targs[i]; 

        // best position for error reporting
        var pos = pos;
        if (i < len(poslist)) {
            pos = poslist[i];
        }
        iface = check.subst(pos, iface, smap)._<ptr<Interface>>(); 

        // targ must implement iface (methods)
        // - check only if we have methods
        check.completeInterface(nopos, iface);
        if (len(iface.allMethods) > 0) { 
            // If the type argument is a pointer to a type parameter, the type argument's
            // method set is empty.
            // TODO(gri) is this what we want? (spec question)
            {
                var (base, isPtr) = deref(targ);

                if (isPtr && asTypeParam(base) != null) {
                    check.errorf(pos, "%s has no methods", targ);
                    break;
                }

            }

            {
                var (m, wrong) = check.missingMethod(targ, iface, true);

                if (m != null) { 
                    // TODO(gri) needs to print updated name to avoid major confusion in error message!
                    //           (print warning for now)
                    // Old warning:
                    // check.softErrorf(pos, "%s does not satisfy %s (warning: name not updated) = %s (missing method %s)", targ, tpar.bound, iface, m)
                    if (m.name == "==") { 
                        // We don't want to report "missing method ==".
                        check.softErrorf(pos, "%s does not satisfy comparable", targ);

                    }
                    else if (wrong != null) { 
                        // TODO(gri) This can still report uninstantiated types which makes the error message
                        //           more difficult to read then necessary.
                        check.softErrorf(pos, "%s does not satisfy %s: wrong method signature\n\tgot  %s\n\twant %s", targ, tpar.bound, wrong, m);

                    }
                    else
 {
                        check.softErrorf(pos, "%s does not satisfy %s (missing method %s)", targ, tpar.bound, m.name);
                    }

                    break;

                }

            }

        }
        if (iface.allTypes == null) {
            continue; // nothing to do
        }
        {
            var targ__prev1 = targ;

            targ = asTypeParam(targ);

            if (targ != null) {
                var targBound = targ.Bound();
                if (targBound.allTypes == null) {
                    check.softErrorf(pos, "%s does not satisfy %s (%s has no type constraints)", targ, tpar.bound, targ);
                    break;
                }
                {
                    var t__prev2 = t;

                    foreach (var (_, __t) in unpack(targBound.allTypes)) {
                        t = __t;
                        if (!iface.isSatisfiedBy(t)) { 
                            // TODO(gri) match this error message with the one below (or vice versa)
                            check.softErrorf(pos, "%s does not satisfy %s (%s type constraint %s not found in %s)", targ, tpar.bound, targ, t, iface.allTypes);
                            break;

                        }

                    }

                    t = t__prev2;
                }

                break;

            } 

            // Otherwise, targ's type or underlying type must also be one of the interface types listed, if any.

            targ = targ__prev1;

        } 

        // Otherwise, targ's type or underlying type must also be one of the interface types listed, if any.
        if (!iface.isSatisfiedBy(targ)) {
            check.softErrorf(pos, "%s does not satisfy %s (%s not found in %s)", targ, tpar.bound, under(targ), iface.allTypes);
            break;
        }
    }    return check.subst(pos, typ, smap);

});

// subst returns the type typ with its type parameters tpars replaced by
// the corresponding type arguments targs, recursively.
// subst is functional in the sense that it doesn't modify the incoming
// type. If a substitution took place, the result type is different from
// from the incoming type.
private static Type subst(this ptr<Checker> _addr_check, syntax.Pos pos, Type typ, ptr<substMap> _addr_smap) {
    ref Checker check = ref _addr_check.val;
    ref substMap smap = ref _addr_smap.val;

    if (smap.empty()) {
        return typ;
    }
    switch (typ.type()) {
        case ptr<Basic> t:
            return typ; // nothing to do
            break;
        case ptr<TypeParam> t:
            return smap.lookup(t);
            break; 

        // general case
    } 

    // general case
    subster subst = new subster(check,pos,make(map[Type]Type),smap);
    return subst.typ(typ);

}

private partial struct subster {
    public ptr<Checker> check;
    public syntax.Pos pos;
    public map<Type, Type> cache;
    public ptr<substMap> smap;
}

private static Type typ(this ptr<subster> _addr_subst, Type typ) => func((defer, panic, _) => {
    ref subster subst = ref _addr_subst.val;

    switch (typ.type()) {
        case 
            panic("nil typ");
            break;
        case ptr<Basic> t:
            break;
        case ptr<bottom> t:
            break;
        case ptr<top> t:
            break;
        case ptr<Array> t:
            var elem = subst.typOrNil(t.elem);
            if (elem != t.elem) {
                return addr(new Array(len:t.len,elem:elem));
            }
            break;
        case ptr<Slice> t:
            elem = subst.typOrNil(t.elem);
            if (elem != t.elem) {
                return addr(new Slice(elem:elem));
            }
            break;
        case ptr<Struct> t:
            {
                var (fields, copied) = subst.varList(t.fields);

                if (copied) {
                    return addr(new Struct(fields:fields,tags:t.tags));
                }

            }


            break;
        case ptr<Pointer> t:
            var @base = subst.typ(t.@base);
            if (base != t.@base) {
                return addr(new Pointer(base:base));
            }
            break;
        case ptr<Tuple> t:
            return subst.tuple(t);
            break;
        case ptr<Signature> t:
            var recv = t.recv;
            var @params = subst.tuple(t.@params);
            var results = subst.tuple(t.results);
            if (recv != t.recv || params != t.@params || results != t.results) {
                return addr(new Signature(rparams:t.rparams,tparams:t.tparams,scope:t.scope,recv:recv,params:params,results:results,variadic:t.variadic,));
            }
            break;
        case ptr<Sum> t:
            var (types, copied) = subst.typeList(t.types);
            if (copied) { 
                // Don't do it manually, with a Sum literal: the new
                // types list may not be unique and NewSum may remove
                // duplicates.
                return NewSum(types);

            }

            break;
        case ptr<Interface> t:
            var (methods, mcopied) = subst.funcList(t.methods);
            var types = t.types;
            if (t.types != null) {
                types = subst.typ(t.types);
            }
            var (embeddeds, ecopied) = subst.typeList(t.embeddeds);
            if (mcopied || types != t.types || ecopied) {
                ptr<Interface> iface = addr(new Interface(methods:methods,types:types,embeddeds:embeddeds));
                if (subst.check == null) {
                    panic("internal error: cannot instantiate interfaces yet");
                }
                subst.check.posMap[iface] = subst.check.posMap[t]; // satisfy completeInterface requirement
                subst.check.completeInterface(nopos, iface);
                return iface;

            }

            break;
        case ptr<Map> t:
            var key = subst.typ(t.key);
            elem = subst.typ(t.elem);
            if (key != t.key || elem != t.elem) {
                return addr(new Map(key:key,elem:elem));
            }
            break;
        case ptr<Chan> t:
            elem = subst.typ(t.elem);
            if (elem != t.elem) {
                return addr(new Chan(dir:t.dir,elem:elem));
            }
            break;
        case ptr<Named> t:
            Action<@string, object> dump = (_p0, _p0) => {
            }
;
            if (subst.check != null && subst.check.conf.Trace) {
                subst.check.indent++;
                defer(() => {
                    subst.check.indent--;
                }());
                dump = (format, args) => {
                    subst.check.trace(subst.pos, format, args);
                }
;
            }

            if (t.tparams == null) {
                dump(">>> %s is not parameterized", t);
                return t; // type is not parameterized
            }

            slice<Type> new_targs = default;

            if (len(t.targs) > 0) { 
                // already instantiated
                dump(">>> %s already instantiated", t);
                assert(len(t.targs) == len(t.tparams)); 
                // For each (existing) type argument targ, determine if it needs
                // to be substituted; i.e., if it is or contains a type parameter
                // that has a type argument for it.
                foreach (var (i, targ) in t.targs) {
                    dump(">>> %d targ = %s", i, targ);
                    var new_targ = subst.typ(targ);
                    if (new_targ != targ) {
                        dump(">>> substituted %d targ %s => %s", i, targ, new_targ);
                        if (new_targs == null) {
                            new_targs = make_slice<Type>(len(t.tparams));
                            copy(new_targs, t.targs);
                        }
                        new_targs[i] = new_targ;
                    }
                }
            else

                if (new_targs == null) {
                    dump(">>> nothing to substitute in %s", t);
                    return t; // nothing to substitute
                }

            } { 
                // not yet instantiated
                dump(">>> first instantiation of %s", t);
                new_targs = subst.smap.targs;

            } 

            // before creating a new named type, check if we have this one already
            var h = instantiatedHash(_addr_t, new_targs);
            dump(">>> new type hash: %s", h);
            if (subst.check != null) {
                {
                    var named__prev2 = named;

                    var (named, found) = subst.check.typMap[h];

                    if (found) {
                        dump(">>> found %s", named);
                        subst.cache[t] = named;
                        return named;
                    }

                    named = named__prev2;

                }

            } 

            // create a new named type and populate caches to avoid endless recursion
            var tname = NewTypeName(subst.pos, t.obj.pkg, t.obj.name, null);
            var named = subst.check.newNamed(tname, t, t.underlying, t.tparams, t.methods); // method signatures are updated lazily
            named.targs = new_targs;
            if (subst.check != null) {
                subst.check.typMap[h] = named;
            }

            subst.cache[t] = named; 

            // do the substitution
            dump(">>> subst %s with %s (new: %s)", t.underlying, subst.smap, new_targs);
            named.underlying = subst.typOrNil(t.underlying);
            named.fromRHS = named.underlying; // for cycle detection (Checker.validType)

            return named;
            break;
        case ptr<TypeParam> t:
            return subst.smap.lookup(t);
            break;
        case ptr<instance> t:
            return subst.typ(t.expand());
            break;
        default:
        {
            var t = typ.type();
            unimplemented();
            break;
        }

    }

    return typ;

});

// TODO(gri) Eventually, this should be more sophisticated.
//           It won't work correctly for locally declared types.
private static @string instantiatedHash(ptr<Named> _addr_typ, slice<Type> targs) {
    ref Named typ = ref _addr_typ.val;

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    writeTypeName(_addr_buf, typ.obj, null);
    buf.WriteByte('[');
    writeTypeList(_addr_buf, targs, null, null);
    buf.WriteByte(']'); 

    // With respect to the represented type, whether a
    // type is fully expanded or stored as instance
    // does not matter - they are the same types.
    // Remove the instanceMarkers printed for instances.
    var res = buf.Bytes();
    nint i = 0;
    foreach (var (_, b) in res) {
        if (b != instanceMarker) {
            res[i] = b;
            i++;
        }
    }    return string(res[..(int)i]);

}

private static @string typeListString(slice<Type> list) {
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    writeTypeList(_addr_buf, list, null, null);
    return buf.String();
}

// typOrNil is like typ but if the argument is nil it is replaced with Typ[Invalid].
// A nil type may appear in pathological cases such as type T[P any] []func(_ T([]_))
// where an array/slice element is accessed before it is set up.
private static Type typOrNil(this ptr<subster> _addr_subst, Type typ) {
    ref subster subst = ref _addr_subst.val;

    if (typ == null) {
        return Typ[Invalid];
    }
    return subst.typ(typ);

}

private static ptr<Var> var_(this ptr<subster> _addr_subst, ptr<Var> _addr_v) {
    ref subster subst = ref _addr_subst.val;
    ref Var v = ref _addr_v.val;

    if (v != null) {
        {
            var typ = subst.typ(v.typ);

            if (typ != v.typ) {
                ref Var copy = ref heap(v, out ptr<Var> _addr_copy);
                copy.typ = typ;
                return _addr__addr_copy!;
            }

        }

    }
    return _addr_v!;

}

private static ptr<Tuple> tuple(this ptr<subster> _addr_subst, ptr<Tuple> _addr_t) {
    ref subster subst = ref _addr_subst.val;
    ref Tuple t = ref _addr_t.val;

    if (t != null) {
        {
            var (vars, copied) = subst.varList(t.vars);

            if (copied) {
                return addr(new Tuple(vars:vars));
            }

        }

    }
    return _addr_t!;

}

private static (slice<ptr<Var>>, bool) varList(this ptr<subster> _addr_subst, slice<ptr<Var>> @in) {
    slice<ptr<Var>> @out = default;
    bool copied = default;
    ref subster subst = ref _addr_subst.val;

    out = in;
    foreach (var (i, v) in in) {
        {
            var w = subst.var_(v);

            if (w != v) {
                if (!copied) { 
                    // first variable that got substituted => allocate new out slice
                    // and copy all variables
                    var @new = make_slice<ptr<Var>>(len(in));
                    copy(new, out);
                    out = new;
                    copied = true;

                }

                out[i] = w;

            }

        }

    }    return ;

}

private static ptr<Func> func_(this ptr<subster> _addr_subst, ptr<Func> _addr_f) {
    ref subster subst = ref _addr_subst.val;
    ref Func f = ref _addr_f.val;

    if (f != null) {
        {
            var typ = subst.typ(f.typ);

            if (typ != f.typ) {
                ref Func copy = ref heap(f, out ptr<Func> _addr_copy);
                copy.typ = typ;
                return _addr__addr_copy!;
            }

        }

    }
    return _addr_f!;

}

private static (slice<ptr<Func>>, bool) funcList(this ptr<subster> _addr_subst, slice<ptr<Func>> @in) {
    slice<ptr<Func>> @out = default;
    bool copied = default;
    ref subster subst = ref _addr_subst.val;

    out = in;
    foreach (var (i, f) in in) {
        {
            var g = subst.func_(f);

            if (g != f) {
                if (!copied) { 
                    // first function that got substituted => allocate new out slice
                    // and copy all functions
                    var @new = make_slice<ptr<Func>>(len(in));
                    copy(new, out);
                    out = new;
                    copied = true;

                }

                out[i] = g;

            }

        }

    }    return ;

}

private static (slice<Type>, bool) typeList(this ptr<subster> _addr_subst, slice<Type> @in) {
    slice<Type> @out = default;
    bool copied = default;
    ref subster subst = ref _addr_subst.val;

    out = in;
    foreach (var (i, t) in in) {
        {
            var u = subst.typ(t);

            if (u != t) {
                if (!copied) { 
                    // first function that got substituted => allocate new out slice
                    // and copy all functions
                    var @new = make_slice<Type>(len(in));
                    copy(new, out);
                    out = new;
                    copied = true;

                }

                out[i] = u;

            }

        }

    }    return ;

}

} // end types2_package
