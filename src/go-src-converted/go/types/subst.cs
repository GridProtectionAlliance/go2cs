// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements instantiation of generic types
// through substitution of type parameters by actual
// types.

// package types -- go2cs converted at 2022 March 06 22:42:20 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\subst.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using token = go.go.token_package;
using System;


namespace go.go;

public static partial class types_package {

    // TODO(rFindley) decide error codes for the errors in this file, and check
    //                if error spans can be improved
private partial struct substMap {
    public slice<Type> targs;
    public map<ptr<_TypeParam>, Type> proj;
}

// makeSubstMap creates a new substitution map mapping tpars[i] to targs[i].
// If targs[i] is nil, tpars[i] is not substituted.
private static ptr<substMap> makeSubstMap(slice<ptr<TypeName>> tpars, slice<Type> targs) {
    assert(len(tpars) == len(targs));
    var proj = make_map<ptr<_TypeParam>, Type>(len(tpars));
    foreach (var (i, tpar) in tpars) { 
        // We must expand type arguments otherwise *instance
        // types end up as components in composite types.
        // TODO(gri) explain why this causes problems, if it does
        var targ = expand(targs[i]); // possibly nil
        targs[i] = targ;
        proj[tpar.typ._<ptr<_TypeParam>>()] = targ;

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

private static Type lookup(this ptr<substMap> _addr_m, ptr<_TypeParam> _addr_tpar) {
    ref substMap m = ref _addr_m.val;
    ref _TypeParam tpar = ref _addr_tpar.val;

    {
        var t = m.proj[tpar];

        if (t != null) {
            return t;
        }
    }

    return tpar;

}

private static Type instantiate(this ptr<Checker> _addr_check, token.Pos pos, Type typ, slice<Type> targs, slice<token.Pos> poslist) => func((defer, _, _) => {
    Type res = default;
    ref Checker check = ref _addr_check.val;

    if (trace) {
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
        check.errorf(atPos(pos), _Todo, "got %d arguments but %d type parameters", len(targs), len(tparams));
        return Typ[Invalid];

    }
    if (len(tparams) == 0) {
        return typ; // nothing to do (minor optimization)
    }
    var smap = makeSubstMap(tparams, targs); 

    // check bounds
    foreach (var (i, tname) in tparams) {
        ptr<_TypeParam> tpar = tname.typ._<ptr<_TypeParam>>();
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
        check.completeInterface(token.NoPos, iface);
        if (len(iface.allMethods) > 0) { 
            // If the type argument is a pointer to a type parameter, the type argument's
            // method set is empty.
            // TODO(gri) is this what we want? (spec question)
            {
                var (base, isPtr) = deref(targ);

                if (isPtr && asTypeParam(base) != null) {
                    check.errorf(atPos(pos), 0, "%s has no methods", targ);
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
                        check.softErrorf(atPos(pos), 0, "%s does not satisfy comparable", targ);

                    }
                    else if (wrong != null) { 
                        // TODO(gri) This can still report uninstantiated types which makes the error message
                        //           more difficult to read then necessary.
                        // TODO(rFindley) should this use parentheses rather than ':' for qualification?
                        check.softErrorf(atPos(pos), _Todo, "%s does not satisfy %s: wrong method signature\n\tgot  %s\n\twant %s", targ, tpar.bound, wrong, m);

                    }
                    else
 {
                        check.softErrorf(atPos(pos), 0, "%s does not satisfy %s (missing method %s)", targ, tpar.bound, m.name);
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
                    check.softErrorf(atPos(pos), _Todo, "%s does not satisfy %s (%s has no type constraints)", targ, tpar.bound, targ);
                    break;
                }
                {
                    var t__prev2 = t;

                    foreach (var (_, __t) in unpackType(targBound.allTypes)) {
                        t = __t;
                        if (!iface.isSatisfiedBy(t)) { 
                            // TODO(gri) match this error message with the one below (or vice versa)
                            check.softErrorf(atPos(pos), 0, "%s does not satisfy %s (%s type constraint %s not found in %s)", targ, tpar.bound, targ, t, iface.allTypes);
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
            check.softErrorf(atPos(pos), _Todo, "%s does not satisfy %s (%s or %s not found in %s)", targ, tpar.bound, targ, under(targ), iface.allTypes);
            break;
        }
    }    return check.subst(pos, typ, smap);

});

// subst returns the type typ with its type parameters tpars replaced by
// the corresponding type arguments targs, recursively.
// subst is functional in the sense that it doesn't modify the incoming
// type. If a substitution took place, the result type is different from
// from the incoming type.
private static Type subst(this ptr<Checker> _addr_check, token.Pos pos, Type typ, ptr<substMap> _addr_smap) {
    ref Checker check = ref _addr_check.val;
    ref substMap smap = ref _addr_smap.val;

    if (smap.empty()) {
        return typ;
    }
    switch (typ.type()) {
        case ptr<Basic> t:
            return typ; // nothing to do
            break;
        case ptr<_TypeParam> t:
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
    public token.Pos pos;
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
        case ptr<_Sum> t:
            var (types, copied) = subst.typeList(t.types);
            if (copied) { 
                // Don't do it manually, with a Sum literal: the new
                // types list may not be unique and NewSum may remove
                // duplicates.
                return _NewSum(types);

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
                subst.check.posMap[iface] = subst.check.posMap[t]; // satisfy completeInterface requirement
                subst.check.completeInterface(token.NoPos, iface);
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
            subst.check.indent++;
            defer(() => {
                subst.check.indent--;
            }());
            Action<@string, object[]> dump = (format, args) => {
                if (trace) {
                    subst.check.trace(subst.pos, format, args);
                }
            }
;

            if (t.tparams == null) {
                dump(">>> %s is not parameterized", t);
                return t; // type is not parameterized
            }

            slice<Type> newTargs = default;

            if (len(t.targs) > 0) { 
                // already instantiated
                dump(">>> %s already instantiated", t);
                assert(len(t.targs) == len(t.tparams)); 
                // For each (existing) type argument targ, determine if it needs
                // to be substituted; i.e., if it is or contains a type parameter
                // that has a type argument for it.
                foreach (var (i, targ) in t.targs) {
                    dump(">>> %d targ = %s", i, targ);
                    var newTarg = subst.typ(targ);
                    if (newTarg != targ) {
                        dump(">>> substituted %d targ %s => %s", i, targ, newTarg);
                        if (newTargs == null) {
                            newTargs = make_slice<Type>(len(t.tparams));
                            copy(newTargs, t.targs);
                        }
                        newTargs[i] = newTarg;
                    }
                }
            else

                if (newTargs == null) {
                    dump(">>> nothing to substitute in %s", t);
                    return t; // nothing to substitute
                }

            } { 
                // not yet instantiated
                dump(">>> first instantiation of %s", t); 
                // TODO(rFindley) can we instead subst the tparam types here?
                newTargs = subst.smap.targs;

            } 

            // before creating a new named type, check if we have this one already
            var h = instantiatedHash(_addr_t, newTargs);
            dump(">>> new type hash: %s", h);
            {
                var named__prev1 = named;

                var (named, found) = subst.check.typMap[h];

                if (found) {
                    dump(">>> found %s", named);
                    subst.cache[t] = named;
                    return named;
                } 

                // create a new named type and populate caches to avoid endless recursion

                named = named__prev1;

            } 

            // create a new named type and populate caches to avoid endless recursion
            var tname = NewTypeName(subst.pos, t.obj.pkg, t.obj.name, null);
            var named = subst.check.newNamed(tname, t.underlying, t.methods); // method signatures are updated lazily
            named.tparams = t.tparams; // new type is still parameterized
            named.targs = newTargs;
            subst.check.typMap[h] = named;
            subst.cache[t] = named; 

            // do the substitution
            dump(">>> subst %s with %s (new: %s)", t.underlying, subst.smap, newTargs);
            named.underlying = subst.typOrNil(t.underlying);
            named.orig = named.underlying; // for cycle detection (Checker.validType)

            return named;
            break;
        case ptr<_TypeParam> t:
            return subst.smap.lookup(t);
            break;
        case ptr<instance> t:
            return subst.typ(t.expand());
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

} // end types_package
