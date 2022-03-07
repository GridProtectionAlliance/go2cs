// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements type parameter inference.

// package types2 -- go2cs converted at 2022 March 06 23:12:40 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\infer.go
using bytes = go.bytes_package;
using syntax = go.cmd.compile.@internal.syntax_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class types2_package {

private static readonly var useConstraintTypeInference = true;

// infer attempts to infer the complete set of type arguments for generic function instantiation/call
// based on the given type parameters tparams, type arguments targs, function parameters params, and
// function arguments args, if any. There must be at least one type parameter, no more type arguments
// than type parameters, and params and args must match in number (incl. zero).
// If successful, infer returns the complete list of type arguments, one for each type parameter.
// Otherwise the result is nil and appropriate errors will be reported unless report is set to false.
//
// Inference proceeds in 3 steps:
//
//   1) Start with given type arguments.
//   2) Infer type arguments from typed function arguments.
//   3) Infer type arguments from untyped function arguments.
//
// Constraint type inference is used after each step to expand the set of type arguments.
//


// infer attempts to infer the complete set of type arguments for generic function instantiation/call
// based on the given type parameters tparams, type arguments targs, function parameters params, and
// function arguments args, if any. There must be at least one type parameter, no more type arguments
// than type parameters, and params and args must match in number (incl. zero).
// If successful, infer returns the complete list of type arguments, one for each type parameter.
// Otherwise the result is nil and appropriate errors will be reported unless report is set to false.
//
// Inference proceeds in 3 steps:
//
//   1) Start with given type arguments.
//   2) Infer type arguments from typed function arguments.
//   3) Infer type arguments from untyped function arguments.
//
// Constraint type inference is used after each step to expand the set of type arguments.
//
private static slice<Type> infer(this ptr<Checker> _addr_check, syntax.Pos pos, slice<ptr<TypeName>> tparams, slice<Type> targs, ptr<Tuple> _addr_@params, slice<ptr<operand>> args, bool report) => func((defer, _, _) => {
    slice<Type> result = default;
    ref Checker check = ref _addr_check.val;
    ref Tuple @params = ref _addr_@params.val;

    if (debug) {
        defer(() => {
            assert(result == null || len(result) == len(tparams));
            {
                var targ__prev1 = targ;

                foreach (var (_, __targ) in result) {
                    targ = __targ;
                    assert(targ != null);
                } 
                //check.dump("### inferred targs = %s", result)

                targ = targ__prev1;
            }
        }());

    }
    var n = len(tparams);
    assert(n > 0 && len(targs) <= n); 

    // Function parameters and arguments must match in number.
    assert(@params.Len() == len(args)); 

    // --- 0 ---
    // If we already have all type arguments, we're done.
    if (len(targs) == n) {
        return targs;
    }
    if (len(targs) > 0 && useConstraintTypeInference) {
        nint index = default;
        targs, index = check.inferB(tparams, targs, report);
        if (targs == null || index < 0) {
            return targs;
        }
    }
    if (len(targs) < n) {
        var targs2 = make_slice<Type>(n);
        copy(targs2, targs);
        targs = targs2;
    }
    if (@params.Len() > 0) {
        var smap = makeSubstMap(tparams, targs);
        params = check.subst(nopos, params, smap)._<ptr<Tuple>>();
    }
    var u = newUnifier(check, false);
    u.x.init(tparams); 

    // Set the type arguments which we know already.
    {
        var i__prev1 = i;
        var targ__prev1 = targ;

        foreach (var (__i, __targ) in targs) {
            i = __i;
            targ = __targ;
            if (targ != null) {
                u.x.set(i, targ);
            }
        }
        i = i__prev1;
        targ = targ__prev1;
    }

    Action<@string, Type, Type, ptr<operand>> errorf = (kind, tpar, targ, arg) => {
        if (!report) {
            return ;
        }
        var (targs, index) = u.x.types();
        if (index == 0) { 
            // The first type parameter couldn't be inferred.
            // If none of them could be inferred, don't try
            // to provide the inferred type in the error msg.
            var allFailed = true;
            {
                var targ__prev1 = targ;

                foreach (var (_, __targ) in targs) {
                    targ = __targ;
                    if (targ != null) {
                        allFailed = false;
                        break;
                    }
                }

                targ = targ__prev1;
            }

            if (allFailed) {
                check.errorf(arg, "%s %s of %s does not match %s (cannot infer %s)", kind, targ, arg.expr, tpar, typeNamesString(tparams));
                return ;
            }

        }
        smap = makeSubstMap(tparams, targs);
        var inferred = check.subst(arg.Pos(), tpar, smap);
        if (inferred != tpar) {
            check.errorf(arg, "%s %s of %s does not match inferred type %s for %s", kind, targ, arg.expr, inferred, tpar);
        }
        else
 {
            check.errorf(arg, "%s %s of %s does not match %s", kind, targ, arg.expr, tpar);
        }
    }; 

    // indices of the generic parameters with untyped arguments - save for later
    slice<nint> indices = default;
    {
        var i__prev1 = i;
        var arg__prev1 = arg;

        foreach (var (__i, __arg) in args) {
            i = __i;
            arg = __arg;
            var par = @params.At(i); 
            // If we permit bidirectional unification, this conditional code needs to be
            // executed even if par.typ is not parameterized since the argument may be a
            // generic function (for which we want to infer its type arguments).
            if (isParameterized(tparams, par.typ)) {
                if (arg.mode == invalid) { 
                    // An error was reported earlier. Ignore this targ
                    // and continue, we may still be able to infer all
                    // targs resulting in fewer follon-on errors.
                    continue;

                }

                {
                    var targ__prev2 = targ;

                    var targ = arg.typ;

                    if (isTyped(targ)) { 
                        // If we permit bidirectional unification, and targ is
                        // a generic function, we need to initialize u.y with
                        // the respective type parameters of targ.
                        if (!u.unify(par.typ, targ)) {
                            errorf("type", par.typ, targ, arg);
                            return null;
                        }

                    }
                    else
 {
                        indices = append(indices, i);
                    }

                    targ = targ__prev2;

                }

            }

        }
        i = i__prev1;
        arg = arg__prev1;
    }

    index = default;
    targs, index = u.x.types();
    if (index < 0) {
        return targs;
    }
    if (useConstraintTypeInference) {
        targs, index = check.inferB(tparams, targs, report);
        if (targs == null || index < 0) {
            return targs;
        }
    }
    {
        var i__prev1 = i;

        foreach (var (_, __i) in indices) {
            i = __i;
            par = @params.At(i); 
            // Since untyped types are all basic (i.e., non-composite) types, an
            // untyped argument will never match a composite parameter type; the
            // only parameter type it can possibly match against is a *TypeParam.
            // Thus, only consider untyped arguments for generic parameters that
            // are not of composite types and which don't have a type inferred yet.
            {
                ptr<TypeParam> tpar__prev1 = tpar;

                ptr<TypeParam> (tpar, _) = par.typ._<ptr<TypeParam>>();

                if (tpar != null && targs[tpar.index] == null) {
                    var arg = args[i];
                    targ = Default(arg.typ); 
                    // The default type for an untyped nil is untyped nil. We must not
                    // infer an untyped nil type as type parameter type. Ignore untyped
                    // nil by making sure all default argument types are typed.
                    if (isTyped(targ) && !u.unify(par.typ, targ)) {
                        errorf("default type", par.typ, targ, arg);
                        return null;
                    }

                }

                tpar = tpar__prev1;

            }

        }
        i = i__prev1;
    }

    targs, index = u.x.types();
    if (index < 0) {
        return targs;
    }
    if (useConstraintTypeInference) {
        targs, index = check.inferB(tparams, targs, report);
        if (targs == null || index < 0) {
            return targs;
        }
    }
    assert(targs != null && index >= 0 && targs[index] == null);
    var tpar = tparams[index];
    if (report) {
        check.errorf(pos, "cannot infer %s (%s) (%s)", tpar.name, tpar.pos, targs);
    }
    return null;

});

// typeNamesString produces a string containing all the
// type names in list suitable for human consumption.
private static @string typeNamesString(slice<ptr<TypeName>> list) { 
    // common cases
    var n = len(list);
    switch (n) {
        case 0: 
            return "";
            break;
        case 1: 
            return list[0].name;
            break;
        case 2: 
            return list[0].name + " and " + list[1].name;
            break;
    } 

    // general case (n > 2)
    // Would like to use strings.Builder but it's not available in Go 1.4.
    bytes.Buffer b = default;
    foreach (var (i, tname) in list[..(int)n - 1]) {
        if (i > 0) {
            b.WriteString(", ");
        }
        b.WriteString(tname.name);

    }    b.WriteString(", and ");
    b.WriteString(list[n - 1].name);
    return b.String();

}

// IsParameterized reports whether typ contains any of the type parameters of tparams.
private static bool isParameterized(slice<ptr<TypeName>> tparams, Type typ) {
    tpWalker w = new tpWalker(seen:make(map[Type]bool),tparams:tparams,);
    return w.isParameterized(typ);
}

private partial struct tpWalker {
    public map<Type, bool> seen;
    public slice<ptr<TypeName>> tparams;
}

private static bool isParameterized(this ptr<tpWalker> _addr_w, Type typ) => func((defer, _, _) => {
    bool res = default;
    ref tpWalker w = ref _addr_w.val;
 
    // detect cycles
    {
        var (x, ok) = w.seen[typ];

        if (ok) {
            return x;
        }
    }

    w.seen[typ] = false;
    defer(() => {
        w.seen[typ] = res;
    }());

    switch (typ.type()) {
        case ptr<Basic> t:
            break;
            break;
        case ptr<Array> t:
            return w.isParameterized(t.elem);
            break;
        case ptr<Slice> t:
            return w.isParameterized(t.elem);
            break;
        case ptr<Struct> t:
            foreach (var (_, fld) in t.fields) {
                if (w.isParameterized(fld.typ)) {
                    return true;
                }
            }
            break;
        case ptr<Pointer> t:
            return w.isParameterized(t.@base);
            break;
        case ptr<Tuple> t:
            var n = t.Len();
            for (nint i = 0; i < n; i++) {
                if (w.isParameterized(t.At(i).typ)) {
                    return true;
                }
            }

            break;
        case ptr<Sum> t:
            return w.isParameterizedList(t.types);
            break;
        case ptr<Signature> t:
            return w.isParameterized(t.@params) || w.isParameterized(t.results);
            break;
        case ptr<Interface> t:
            if (t.allMethods != null) { 
                // interface is complete - quick test
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in t.allMethods) {
                        m = __m;
                        if (w.isParameterized(m.typ)) {
                            return true;
                        }
                    }

                    m = m__prev1;
                }

                return w.isParameterizedList(unpack(t.allTypes));

            }

            return t.iterate(t => {
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in t.methods) {
                        m = __m;
                        if (w.isParameterized(m.typ)) {
                            return true;
                        }
                    }

                    m = m__prev1;
                }

                return w.isParameterizedList(unpack(t.types));

            }, null);
            break;
        case ptr<Map> t:
            return w.isParameterized(t.key) || w.isParameterized(t.elem);
            break;
        case ptr<Chan> t:
            return w.isParameterized(t.elem);
            break;
        case ptr<Named> t:
            return w.isParameterizedList(t.targs);
            break;
        case ptr<TypeParam> t:
            return t.index < len(w.tparams) && w.tparams[t.index].typ == t;
            break;
        case ptr<instance> t:
            return w.isParameterizedList(t.targs);
            break;
        default:
        {
            var t = typ.type();
            unreachable();
            break;
        }

    }

    return false;

});

private static bool isParameterizedList(this ptr<tpWalker> _addr_w, slice<Type> list) {
    ref tpWalker w = ref _addr_w.val;

    foreach (var (_, t) in list) {
        if (w.isParameterized(t)) {
            return true;
        }
    }    return false;

}

// inferB returns the list of actual type arguments inferred from the type parameters'
// bounds and an initial set of type arguments. If type inference is impossible because
// unification fails, an error is reported if report is set to true, the resulting types
// list is nil, and index is 0.
// Otherwise, types is the list of inferred type arguments, and index is the index of the
// first type argument in that list that couldn't be inferred (and thus is nil). If all
// type arguments were inferred successfully, index is < 0. The number of type arguments
// provided may be less than the number of type parameters, but there must be at least one.
private static (slice<Type>, nint) inferB(this ptr<Checker> _addr_check, slice<ptr<TypeName>> tparams, slice<Type> targs, bool report) {
    slice<Type> types = default;
    nint index = default;
    ref Checker check = ref _addr_check.val;

    assert(len(tparams) >= len(targs) && len(targs) > 0); 

    // Setup bidirectional unification between those structural bounds
    // and the corresponding type arguments (which may be nil!).
    var u = newUnifier(check, false);
    u.x.init(tparams);
    u.y = u.x; // type parameters between LHS and RHS of unification are identical

    // Set the type arguments which we know already.
    {
        var i__prev1 = i;
        var targ__prev1 = targ;

        foreach (var (__i, __targ) in targs) {
            i = __i;
            targ = __targ;
            if (targ != null) {
                u.x.set(i, targ);
            }
        }
        i = i__prev1;
        targ = targ__prev1;
    }

    foreach (var (_, tpar) in tparams) {
        ptr<TypeParam> typ = tpar.typ._<ptr<TypeParam>>();
        var sbound = check.structuralType(typ.bound);
        if (sbound != null) {
            if (!u.unify(typ, sbound)) {
                if (report) {
                    check.errorf(tpar, "%s does not match %s", tpar, sbound);
                }
                return (null, 0);
            }
        }
    }    types, _ = u.x.types();
    if (debug) {
        {
            var i__prev1 = i;
            var targ__prev1 = targ;

            foreach (var (__i, __targ) in targs) {
                i = __i;
                targ = __targ;
                assert(targ == null || types[i] == targ);
            }

            i = i__prev1;
            targ = targ__prev1;
        }
    }
    slice<nint> dirty = default;
    {
        var i__prev1 = i;
        ptr<TypeParam> typ__prev1 = typ;

        foreach (var (__i, __typ) in types) {
            i = __i;
            typ = __typ;
            if (typ != null && (i >= len(targs) || targs[i] == null)) {
                dirty = append(dirty, i);
            }
        }
        i = i__prev1;
        typ = typ__prev1;
    }

    while (len(dirty) > 0) { 
        // TODO(gri) Instead of creating a new substMap for each iteration,
        // provide an update operation for substMaps and only change when
        // needed. Optimization.
        var smap = makeSubstMap(tparams, types);
        nint n = 0;
        foreach (var (_, index) in dirty) {
            var t0 = types[index];
            {
                var t1 = check.subst(nopos, t0, smap);

                if (t1 != t0) {
                    types[index] = t1;
                    dirty[n] = index;
                    n++;
                }

            }

        }        dirty = dirty[..(int)n];

    } 

    // Once nothing changes anymore, we may still have type parameters left;
    // e.g., a structural constraint *P may match a type parameter Q but we
    // don't have any type arguments to fill in for *P or Q (issue #45548).
    // Don't let such inferences escape, instead nil them out.
    {
        var i__prev1 = i;
        ptr<TypeParam> typ__prev1 = typ;

        foreach (var (__i, __typ) in types) {
            i = __i;
            typ = __typ;
            if (typ != null && isParameterized(tparams, typ)) {
                types[i] = null;
            }
        }
        i = i__prev1;
        typ = typ__prev1;
    }

    index = -1;
    {
        var i__prev1 = i;
        ptr<TypeParam> typ__prev1 = typ;

        foreach (var (__i, __typ) in types) {
            i = __i;
            typ = __typ;
            if (typ == null) {
                index = i;
                break;
            }
        }
        i = i__prev1;
        typ = typ__prev1;
    }

    return ;

}

// structuralType returns the structural type of a constraint, if any.
private static Type structuralType(this ptr<Checker> _addr_check, Type constraint) {
    ref Checker check = ref _addr_check.val;

    {
        ptr<Interface> (iface, _) = under(constraint)._<ptr<Interface>>();

        if (iface != null) {
            check.completeInterface(nopos, iface);
            var types = unpack(iface.allTypes);
            if (len(types) == 1) {
                return types[0];
            }
            return null;
        }
    }

    return constraint;

}

} // end types2_package
