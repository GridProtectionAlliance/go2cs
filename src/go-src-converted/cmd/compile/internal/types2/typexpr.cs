// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements type-checking of identifiers and type expressions.

// package types2 -- go2cs converted at 2022 March 06 23:13:07 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\typexpr.go
using syntax = go.cmd.compile.@internal.syntax_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class types2_package {

    // Disabled by default, but enabled when running tests (via types_test.go).
private static bool acceptMethodTypeParams = default;

// ident type-checks identifier e and initializes x with the value or type of e.
// If an error occurred, x.mode is set to invalid.
// For the meaning of def, see Checker.definedType, below.
// If wantType is set, the identifier e is expected to denote a type.
//
private static void ident(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<syntax.Name> _addr_e, ptr<Named> _addr_def, bool wantType) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref syntax.Name e = ref _addr_e.val;
    ref Named def = ref _addr_def.val;

    x.mode = invalid;
    x.expr = e; 

    // Note that we cannot use check.lookup here because the returned scope
    // may be different from obj.Parent(). See also Scope.LookupParent doc.
    var (scope, obj) = check.scope.LookupParent(e.Value, check.pos);
    if (obj == null) {
        if (e.Value == "_") {
            check.error(e, "cannot use _ as value or type");
        }
        else
 {
            if (check.conf.CompilerErrorMessages) {
                check.errorf(e, "undefined: %s", e.Value);
            }
            else
 {
                check.errorf(e, "undeclared name: %s", e.Value);
            }

        }
        return ;

    }
    check.recordUse(e, obj); 

    // Type-check the object.
    // Only call Checker.objDecl if the object doesn't have a type yet
    // (in which case we must actually determine it) or the object is a
    // TypeName and we also want a type (in which case we might detect
    // a cycle which needs to be reported). Otherwise we can skip the
    // call and avoid a possible cycle error in favor of the more
    // informative "not a type/value" error that this function's caller
    // will issue (see issue #25790).
    var typ = obj.Type();
    {
        ptr<TypeName> (_, gotType) = obj._<ptr<TypeName>>();

        if (typ == null || gotType && wantType) {
            check.objDecl(obj, def);
            typ = obj.Type(); // type must have been assigned by Checker.objDecl
        }
    }

    assert(typ != null); 

    // The object may have been dot-imported.
    // If so, mark the respective package as used.
    // (This code is only needed for dot-imports. Without them,
    // we only have to mark variables, see *Var case below).
    {
        var pkgName = check.dotImportMap[new dotImportKey(scope,obj)];

        if (pkgName != null) {
            pkgName.used = true;
        }
    }


    switch (obj.type()) {
        case ptr<PkgName> obj:
            check.errorf(e, "use of package %s not in selector", obj.name);
            return ;
            break;
        case ptr<Const> obj:
            check.addDeclDep(obj);
            if (typ == Typ[Invalid]) {
                return ;
            }
            if (obj == universeIota) {
                if (check.iota == null) {
                    check.error(e, "cannot use iota outside constant declaration");
                    return ;
                }
                x.val = check.iota;
            }
            else
 {
                x.val = obj.val;
            }

            assert(x.val != null);
            x.mode = constant_;
            break;
        case ptr<TypeName> obj:
            x.mode = typexpr;
            break;
        case ptr<Var> obj:
            if (obj.pkg == check.pkg) {
                obj.used = true;
            }
            check.addDeclDep(obj);
            if (typ == Typ[Invalid]) {
                return ;
            }
            x.mode = variable;
            break;
        case ptr<Func> obj:
            check.addDeclDep(obj);
            x.mode = value;
            break;
        case ptr<Builtin> obj:
            x.id = obj.id;
            x.mode = builtin;
            break;
        case ptr<Nil> obj:
            x.mode = nilvalue;
            break;
        default:
        {
            var obj = obj.type();
            unreachable();
            break;
        }

    }

    x.typ = typ;

}

// typ type-checks the type expression e and returns its type, or Typ[Invalid].
// The type must not be an (uninstantiated) generic type.
private static Type typ(this ptr<Checker> _addr_check, syntax.Expr e) {
    ref Checker check = ref _addr_check.val;

    return check.definedType(e, null);
}

// varType type-checks the type expression e and returns its type, or Typ[Invalid].
// The type must not be an (uninstantiated) generic type and it must be ordinary
// (see ordinaryType).
private static Type varType(this ptr<Checker> _addr_check, syntax.Expr e) {
    ref Checker check = ref _addr_check.val;

    var typ = check.definedType(e, null);
    check.ordinaryType(syntax.StartPos(e), typ);
    return typ;
}

// ordinaryType reports an error if typ is an interface type containing
// type lists or is (or embeds) the predeclared type comparable.
private static void ordinaryType(this ptr<Checker> _addr_check, syntax.Pos pos, Type typ) {
    ref Checker check = ref _addr_check.val;
 
    // We don't want to call under() (via Interface) or complete interfaces while we
    // are in the middle of type-checking parameter declarations that might belong to
    // interface methods. Delay this check to the end of type-checking.
    check.later(() => {
        {
            var t = asInterface(typ);

            if (t != null) {
                check.completeInterface(pos, t); // TODO(gri) is this the correct position?
                if (t.allTypes != null) {
                    check.softErrorf(pos, "interface contains type constraints (%s)", t.allTypes);
                    return ;
                }

                if (t.IsComparable()) {
                    check.softErrorf(pos, "interface is (or embeds) comparable");
                }

            }

        }

    });

}

// anyType type-checks the type expression e and returns its type, or Typ[Invalid].
// The type may be generic or instantiated.
private static Type anyType(this ptr<Checker> _addr_check, syntax.Expr e) {
    ref Checker check = ref _addr_check.val;

    var typ = check.typInternal(e, null);
    assert(isTyped(typ));
    check.recordTypeAndValue(e, typexpr, typ, null);
    return typ;
}

// definedType is like typ but also accepts a type name def.
// If def != nil, e is the type specification for the defined type def, declared
// in a type declaration, and def.underlying will be set to the type of e before
// any components of e are type-checked.
//
private static Type definedType(this ptr<Checker> _addr_check, syntax.Expr e, ptr<Named> _addr_def) {
    ref Checker check = ref _addr_check.val;
    ref Named def = ref _addr_def.val;

    var typ = check.typInternal(e, def);
    assert(isTyped(typ));
    if (isGeneric(typ)) {
        check.errorf(e, "cannot use generic type %s without instantiation", typ);
        typ = Typ[Invalid];
    }
    check.recordTypeAndValue(e, typexpr, typ, null);
    return typ;

}

// genericType is like typ but the type must be an (uninstantiated) generic type.
private static Type genericType(this ptr<Checker> _addr_check, syntax.Expr e, bool reportErr) {
    ref Checker check = ref _addr_check.val;

    var typ = check.typInternal(e, null);
    assert(isTyped(typ));
    if (typ != Typ[Invalid] && !isGeneric(typ)) {
        if (reportErr) {
            check.errorf(e, "%s is not a generic type", typ);
        }
        typ = Typ[Invalid];

    }
    check.recordTypeAndValue(e, typexpr, typ, null);
    return typ;

}

// isubst returns an x with identifiers substituted per the substitution map smap.
// isubst only handles the case of (valid) method receiver type expressions correctly.
private static syntax.Expr isubst(syntax.Expr x, map<ptr<syntax.Name>, ptr<syntax.Name>> smap) {
    switch (x.type()) {
        case ptr<syntax.Name> n:
            {
                var alt = smap[n];

                if (alt != null) {
                    return alt;
                } 
                // case *syntax.StarExpr:
                //     X := isubst(n.X, smap)
                //     if X != n.X {
                //         new := *n
                //         new.X = X
                //         return &new
                //     }

            } 
            // case *syntax.StarExpr:
            //     X := isubst(n.X, smap)
            //     if X != n.X {
            //         new := *n
            //         new.X = X
            //         return &new
            //     }
            break;
        case ptr<syntax.Operation> n:
            if (n.Op == syntax.Mul && n.Y == null) {
                var X = isubst(n.X, smap);
                if (X != n.X) {
                    ref var @new = ref heap(n.val, out ptr<var> _addr_@new);
                    @new.X = X;
                    return _addr_new;
                }
            }
            break;
        case ptr<syntax.IndexExpr> n:
            var Index = isubst(n.Index, smap);
            if (Index != n.Index) {
                @new = n.val;
                @new.Index = Index;
                return _addr_new;
            }
            break;
        case ptr<syntax.ListExpr> n:
            slice<syntax.Expr> elems = default;
            foreach (var (i, elem) in n.ElemList) {
                @new = isubst(elem, smap);
                if (new != elem) {
                    if (elems == null) {
                        elems = make_slice<syntax.Expr>(len(n.ElemList));
                        copy(elems, n.ElemList);
                    }
                    elems[i] = new;
                }
            }
            if (elems != null) {
                @new = n.val;
                @new.ElemList = elems;
                return _addr_new;
            }
            break;
        case ptr<syntax.ParenExpr> n:
            return isubst(n.X, smap); // no need to keep parentheses
            break;
        default:
        {
            var n = x.type();
            break;
        }
    }
    return x;

}

// funcType type-checks a function or method type.
private static void funcType(this ptr<Checker> _addr_check, ptr<Signature> _addr_sig, ptr<syntax.Field> _addr_recvPar, slice<ptr<syntax.Field>> tparams, ptr<syntax.FuncType> _addr_ftyp) => func((defer, _, _) => {
    ref Checker check = ref _addr_check.val;
    ref Signature sig = ref _addr_sig.val;
    ref syntax.Field recvPar = ref _addr_recvPar.val;
    ref syntax.FuncType ftyp = ref _addr_ftyp.val;

    check.openScope(ftyp, "function");
    check.scope.isFunc = true;
    check.recordScope(ftyp, check.scope);
    sig.scope = check.scope;
    defer(check.closeScope());

    syntax.Expr recvTyp = default; // rewritten receiver type; valid if != nil
    if (recvPar != null) { 
        // collect generic receiver type parameters, if any
        // - a receiver type parameter is like any other type parameter, except that it is declared implicitly
        // - the receiver specification acts as local declaration for its type parameters, which may be blank
        var (_, rname, rparams) = check.unpackRecv(recvPar.Type, true);
        if (len(rparams) > 0) { 
            // Blank identifiers don't get declared and regular type-checking of the instantiated
            // parameterized receiver type expression fails in Checker.collectParams of receiver.
            // Identify blank type parameters and substitute each with a unique new identifier named
            // "n_" (where n is the parameter index) and which cannot conflict with any user-defined
            // name.
            map<ptr<syntax.Name>, ptr<syntax.Name>> smap = default; // substitution map from "_" to "!n" identifiers
            {
                var i__prev1 = i;

                foreach (var (__i, __p) in rparams) {
                    i = __i;
                    p = __p;
                    if (p.Value == "_") {
                        ref var @new = ref heap(p.val, out ptr<var> _addr_@new);
                        @new.Value = fmt.Sprintf("%d_", i);
                        _addr_rparams[i] = _addr_new;
                        rparams[i] = ref _addr_rparams[i].val; // use n_ identifier instead of _ so it can be looked up
                        if (smap == null) {
                            smap = make_map<ptr<syntax.Name>, ptr<syntax.Name>>();
                        }

                        _addr_smap[p] = _addr_new;
                        smap[p] = ref _addr_smap[p].val;

                    }

                }

                i = i__prev1;
            }

            if (smap != null) { 
                // blank identifiers were found => use rewritten receiver type
                recvTyp = isubst(recvPar.Type, smap);

            } 
            // TODO(gri) rework declareTypeParams
            sig.rparams = null;
            foreach (var (_, rparam) in rparams) {
                sig.rparams = check.declareTypeParam(sig.rparams, rparam);
            } 
            // determine receiver type to get its type parameters
            // and the respective type parameter bounds
            slice<ptr<TypeName>> recvTParams = default;
            if (rname != null) { 
                // recv should be a Named type (otherwise an error is reported elsewhere)
                // Also: Don't report an error via genericType since it will be reported
                //       again when we type-check the signature.
                // TODO(gri) maybe the receiver should be marked as invalid instead?
                {
                    var recv__prev4 = recv;

                    var recv = asNamed(check.genericType(rname, false));

                    if (recv != null) {
                        recvTParams = recv.tparams;
                    }

                    recv = recv__prev4;

                }

            } 
            // provide type parameter bounds
            // - only do this if we have the right number (otherwise an error is reported elsewhere)
            if (len(sig.rparams) == len(recvTParams)) { 
                // We have a list of *TypeNames but we need a list of Types.
                var list = make_slice<Type>(len(sig.rparams));
                {
                    var i__prev1 = i;
                    var t__prev1 = t;

                    foreach (var (__i, __t) in sig.rparams) {
                        i = __i;
                        t = __t;
                        list[i] = t.typ;
                    }

                    i = i__prev1;
                    t = t__prev1;
                }

                smap = makeSubstMap(recvTParams, list);
                {
                    var i__prev1 = i;

                    foreach (var (__i, __tname) in sig.rparams) {
                        i = __i;
                        tname = __tname;
                        ptr<TypeParam> bound = recvTParams[i].typ._<ptr<TypeParam>>().bound; 
                        // bound is (possibly) parameterized in the context of the
                        // receiver type declaration. Substitute parameters for the
                        // current context.
                        // TODO(gri) should we assume now that bounds always exist?
                        //           (no bound == empty interface)
                        if (bound != null) {
                            bound = check.subst(tname.pos, bound, smap);
                            tname.typ._<ptr<TypeParam>>().bound = addr(bound);
                        }

                    }

                    i = i__prev1;
                }
            }

        }
    }
    if (tparams != null) {
        sig.tparams = check.collectTypeParams(tparams); 
        // Always type-check method type parameters but complain if they are not enabled.
        // (A separate check is needed when type-checking interface method signatures because
        // they don't have a receiver specification.)
        if (recvPar != null && !acceptMethodTypeParams) {
            check.error(ftyp, "methods cannot have type parameters");
        }
    }
    var scope = NewScope(check.scope, nopos, nopos, "function body (temp. scope)");
    slice<ptr<Var>> recvList = default; // TODO(gri) remove the need for making a list here
    if (recvPar != null) {
        recvList, _ = check.collectParams(scope, new slice<ptr<syntax.Field>>(new ptr<syntax.Field>[] { recvPar }), recvTyp, false); // use rewritten receiver type, if any
    }
    var (params, variadic) = check.collectParams(scope, ftyp.ParamList, null, true);
    var (results, _) = check.collectParams(scope, ftyp.ResultList, null, false);
    scope.Squash((obj, alt) => {
        ref error_ err = ref heap(out ptr<error_> _addr_err);
        err.errorf(obj, "%s redeclared in this block", obj.Name());
        err.recordAltDecl(alt);
        check.report(_addr_err);
    });

    if (recvPar != null) { 
        // recv parameter list present (may be empty)
        // spec: "The receiver is specified via an extra parameter section preceding the
        // method name. That parameter section must declare a single parameter, the receiver."
        recv = ;
        switch (len(recvList)) {
            case 0: 
                // error reported by resolver
                recv = NewParam(nopos, null, "", Typ[Invalid]); // ignore recv below
                break;
            case 1: 
                recv = recvList[0];
                break;
            default: 
                // more than one receiver
                check.error(recvList[len(recvList) - 1].Pos(), "method must have exactly one receiver");
                break;
        } 

        // TODO(gri) We should delay rtyp expansion to when we actually need the
        //           receiver; thus all checks here should be delayed to later.
        var (rtyp, _) = deref(recv.typ);
        rtyp = expand(rtyp); 

        // spec: "The receiver type must be of the form T or *T where T is a type name."
        // (ignore invalid types - error was reported before)
        {
            var t__prev2 = t;

            var t = rtyp;

            if (t != Typ[Invalid]) {
                err = default;
                {
                    var T__prev3 = T;

                    var T = asNamed(t);

                    if (T != null) { 
                        // spec: "The type denoted by T is called the receiver base type; it must not
                        // be a pointer or interface type and it must be declared in the same package
                        // as the method."
                        if (T.obj.pkg != check.pkg) {
                            err = "type not defined in this package";
                            if (check.conf.CompilerErrorMessages) {
                                check.errorf(recv.pos, "cannot define new methods on non-local type %s", recv.typ);
                                err = "";
                            }
                        }
                        else
 {
                            switch (optype(T).type()) {
                                case ptr<Basic> u:
                                    if (u.kind == UnsafePointer) {
                                        err = "unsafe.Pointer";
                                    }
                                    break;
                                case ptr<Pointer> u:
                                    err = "pointer or interface type";
                                    break;
                                case ptr<Interface> u:
                                    err = "pointer or interface type";
                                    break;
                            }

                        }

                    }                    {
                        var T__prev4 = T;

                        T = asBasic(t);


                        else if (T != null) {
                            err = "basic or unnamed type";
                            if (check.conf.CompilerErrorMessages) {
                                check.errorf(recv.pos, "cannot define new methods on non-local type %s", recv.typ);
                                err = "";
                            }
                        }
                        else
 {
                            check.errorf(recv.pos, "invalid receiver type %s", recv.typ);
                        }

                        T = T__prev4;

                    }


                    T = T__prev3;

                }

                if (err != "") {
                    check.errorf(recv.pos, "invalid receiver type %s (%s)", recv.typ, err); 
                    // ok to continue
                }

            }

            t = t__prev2;

        }

        sig.recv = recv;

    }
    sig.@params = NewTuple(params);
    sig.results = NewTuple(results);
    sig.variadic = variadic;

});

// goTypeName returns the Go type name for typ and
// removes any occurrences of "types2." from that name.
private static @string goTypeName(Type typ) {
    return strings.Replace(fmt.Sprintf("%T", typ), "types2.", "", -1); // strings.ReplaceAll is not available in Go 1.4
}

// typInternal drives type checking of types.
// Must only be called by definedType or genericType.
//
private static Type typInternal(this ptr<Checker> _addr_check, syntax.Expr e0, ptr<Named> _addr_def) => func((defer, _, _) => {
    Type T = default;
    ref Checker check = ref _addr_check.val;
    ref Named def = ref _addr_def.val;

    if (check.conf.Trace) {
        check.trace(e0.Pos(), "type %s", e0);
        check.indent++;
        defer(() => {
            check.indent--;
            Type under = default;
            if (T != null) { 
                // Calling under() here may lead to endless instantiations.
                // Test case: type T[P any] *T[P]
                // TODO(gri) investigate if that's a bug or to be expected
                // (see also analogous comment in Checker.instantiate).
                under = T.Underlying();

            }

            if (T == under) {
                check.trace(e0.Pos(), "=> %s // %s", T, goTypeName(T));
            }
            else
 {
                check.trace(e0.Pos(), "=> %s (under = %s) // %s", T, under, goTypeName(T));
            }

        }());

    }
    switch (e0.type()) {
        case ptr<syntax.BadExpr> e:
            break;
        case ptr<syntax.Name> e:
            ref operand x = ref heap(out ptr<operand> _addr_x);
            check.ident(_addr_x, e, def, true);


            if (x.mode == typexpr) 
                var typ = x.typ;
                def.setUnderlying(typ);
                return typ;
            else if (x.mode == invalid)             else if (x.mode == novalue) 
                check.errorf(_addr_x, "%s used as type", _addr_x);
            else 
                check.errorf(_addr_x, "%s is not a type", _addr_x);
                        break;
        case ptr<syntax.SelectorExpr> e:
            x = default;
            check.selector(_addr_x, e);


            if (x.mode == typexpr) 
                typ = x.typ;
                def.setUnderlying(typ);
                return typ;
            else if (x.mode == invalid)             else if (x.mode == novalue) 
                check.errorf(_addr_x, "%s used as type", _addr_x);
            else 
                check.errorf(_addr_x, "%s is not a type", _addr_x);
                        break;
        case ptr<syntax.IndexExpr> e:
            return check.instantiatedType(e.X, unpackExpr(e.Index), def);
            break;
        case ptr<syntax.ParenExpr> e:
            return check.definedType(e.X, def);
            break;
        case ptr<syntax.ArrayType> e:
            typ = @new<Array>();
            def.setUnderlying(typ);
            if (e.Len != null) {
                typ.len = check.arrayLength(e.Len);
            }
            else
 { 
                // [...]array
                check.error(e, "invalid use of [...] array (outside a composite literal)");
                typ.len = -1;

            }

            typ.elem = check.varType(e.Elem);
            if (typ.len >= 0) {
                return typ;
            } 
            // report error if we encountered [...]
            break;
        case ptr<syntax.SliceType> e:
            typ = @new<Slice>();
            def.setUnderlying(typ);
            typ.elem = check.varType(e.Elem);
            return typ;
            break;
        case ptr<syntax.DotsType> e:
            check.error(e, "invalid use of '...'");
            check.use(e.Elem);
            break;
        case ptr<syntax.StructType> e:
            typ = @new<Struct>();
            def.setUnderlying(typ);
            check.structType(typ, e);
            return typ;
            break;
        case ptr<syntax.Operation> e:
            if (e.Op == syntax.Mul && e.Y == null) {
                typ = @new<Pointer>();
                def.setUnderlying(typ);
                typ.@base = check.varType(e.X);
                return typ;
            }
            check.errorf(e0, "%s is not a type", e0);
            check.use(e0);
            break;
        case ptr<syntax.FuncType> e:
            typ = @new<Signature>();
            def.setUnderlying(typ);
            check.funcType(typ, null, null, e);
            return typ;
            break;
        case ptr<syntax.InterfaceType> e:
            typ = @new<Interface>();
            def.setUnderlying(typ);
            if (def != null) {
                typ.obj = def.obj;
            }
            check.interfaceType(typ, e, def);
            return typ;
            break;
        case ptr<syntax.MapType> e:
            typ = @new<Map>();
            def.setUnderlying(typ);

            typ.key = check.varType(e.Key);
            typ.elem = check.varType(e.Value); 

            // spec: "The comparison operators == and != must be fully defined
            // for operands of the key type; thus the key type must not be a
            // function, map, or slice."
            //
            // Delay this check because it requires fully setup types;
            // it is safe to continue in any case (was issue 6667).
            check.later(() => {
                if (!Comparable(typ.key)) {
                    @string why = default;
                    if (asTypeParam(typ.key) != null) {
                        why = " (missing comparable constraint)";
                    }
                    check.errorf(e.Key, "invalid map key type %s%s", typ.key, why);
                }
            });

            return typ;
            break;
        case ptr<syntax.ChanType> e:
            typ = @new<Chan>();
            def.setUnderlying(typ);

            var dir = SendRecv;

            if (e.Dir == 0)             else if (e.Dir == syntax.SendOnly) 
                dir = SendOnly;
            else if (e.Dir == syntax.RecvOnly) 
                dir = RecvOnly;
            else 
                check.errorf(e, invalidAST + "unknown channel direction %d", e.Dir); 
                // ok to continue
                        typ.dir = dir;
            typ.elem = check.varType(e.Elem);
            return typ;
            break;
        default:
        {
            var e = e0.type();
            check.errorf(e0, "%s is not a type", e0);
            check.use(e0);
            break;
        }

    }

    typ = Typ[Invalid];
    def.setUnderlying(typ);
    return typ;

});

// typeOrNil type-checks the type expression (or nil value) e
// and returns the type of e, or nil. If e is a type, it must
// not be an (uninstantiated) generic type.
// If e is neither a type nor nil, typeOrNil returns Typ[Invalid].
// TODO(gri) should we also disallow non-var types?
private static Type typOrNil(this ptr<Checker> _addr_check, syntax.Expr e) {
    ref Checker check = ref _addr_check.val;

    ref operand x = ref heap(out ptr<operand> _addr_x);
    check.rawExpr(_addr_x, e, null);

    if (x.mode == invalid)     else if (x.mode == novalue) 
        check.errorf(_addr_x, "%s used as type", _addr_x);
    else if (x.mode == typexpr) 
        check.instantiatedOperand(_addr_x);
        return x.typ;
    else if (x.mode == nilvalue) 
        return null;
    else 
        check.errorf(_addr_x, "%s is not a type", _addr_x);
        return Typ[Invalid];

}

private static Type instantiatedType(this ptr<Checker> _addr_check, syntax.Expr x, slice<syntax.Expr> targs, ptr<Named> _addr_def) {
    ref Checker check = ref _addr_check.val;
    ref Named def = ref _addr_def.val;

    var b = check.genericType(x, true); // TODO(gri) what about cycles?
    if (b == Typ[Invalid]) {
        return b; // error already reported
    }
    var @base = asNamed(b);
    if (base == null) {
        unreachable(); // should have been caught by genericType
    }
    ptr<object> typ = @new<instance>();
    def.setUnderlying(typ);

    typ.check = check;
    typ.pos = x.Pos();
    typ.@base = base; 

    // evaluate arguments (always)
    typ.targs = check.typeList(targs);
    if (typ.targs == null) {
        def.setUnderlying(Typ[Invalid]); // avoid later errors due to lazy instantiation
        return Typ[Invalid];

    }
    typ.poslist = make_slice<syntax.Pos>(len(targs));
    foreach (var (i, arg) in targs) {
        typ.poslist[i] = syntax.StartPos(arg);
    }    check.later(() => {
        var t = typ.expand();
        check.validType(t, null);
    });

    return typ;

}

// arrayLength type-checks the array length expression e
// and returns the constant length >= 0, or a value < 0
// to indicate an error (and thus an unknown length).
private static long arrayLength(this ptr<Checker> _addr_check, syntax.Expr e) {
    ref Checker check = ref _addr_check.val;

    ref operand x = ref heap(out ptr<operand> _addr_x);
    check.expr(_addr_x, e);
    if (x.mode != constant_) {
        if (x.mode != invalid) {
            check.errorf(_addr_x, "array length %s must be constant", _addr_x);
        }
        return -1;

    }
    if (isUntyped(x.typ) || isInteger(x.typ)) {
        {
            var val = constant.ToInt(x.val);

            if (val.Kind() == constant.Int) {
                if (representableConst(val, check, Typ[Int], null)) {
                    {
                        var (n, ok) = constant.Int64Val(val);

                        if (ok && n >= 0) {
                            return n;
                        }

                    }

                    check.errorf(_addr_x, "invalid array length %s", _addr_x);
                    return -1;

                }

            }

        }

    }
    check.errorf(_addr_x, "array length %s must be integer", _addr_x);
    return -1;

}

// typeList provides the list of types corresponding to the incoming expression list.
// If an error occurred, the result is nil, but all list elements were type-checked.
private static slice<Type> typeList(this ptr<Checker> _addr_check, slice<syntax.Expr> list) {
    ref Checker check = ref _addr_check.val;

    var res = make_slice<Type>(len(list)); // res != nil even if len(list) == 0
    foreach (var (i, x) in list) {
        var t = check.varType(x);
        if (t == Typ[Invalid]) {
            res = null;
        }
        if (res != null) {
            res[i] = t;
        }
    }    return res;

}

// collectParams declares the parameters of list in scope and returns the corresponding
// variable list. If type0 != nil, it is used instead of the first type in list.
private static (slice<ptr<Var>>, bool) collectParams(this ptr<Checker> _addr_check, ptr<Scope> _addr_scope, slice<ptr<syntax.Field>> list, syntax.Expr type0, bool variadicOk) {
    slice<ptr<Var>> @params = default;
    bool variadic = default;
    ref Checker check = ref _addr_check.val;
    ref Scope scope = ref _addr_scope.val;

    if (list == null) {
        return ;
    }
    bool named = default;    bool anonymous = default;



    Type typ = default;
    syntax.Expr prev = default;
    foreach (var (i, field) in list) {
        var ftype = field.Type; 
        // type-check type of grouped fields only once
        if (ftype != prev) {
            prev = ftype;
            if (i == 0 && type0 != null) {
                ftype = type0;
            }
            {
                ptr<syntax.DotsType> (t, _) = ftype._<ptr<syntax.DotsType>>();

                if (t != null) {
                    ftype = t.Elem;
                    if (variadicOk && i == len(list) - 1) {
                        variadic = true;
                    }
                    else
 {
                        check.softErrorf(t, "can only use ... with final parameter in list"); 
                        // ignore ... and continue
                    }

                }

            }

            typ = check.varType(ftype);

        }
        if (field.Name != null) { 
            // named parameter
            var name = field.Name.Value;
            if (name == "") {
                check.error(field.Name, invalidAST + "anonymous parameter"); 
                // ok to continue
            }

            var par = NewParam(field.Name.Pos(), check.pkg, name, typ);
            check.declare(scope, field.Name, par, scope.pos);
            params = append(params, par);
            named = true;

        }
        else
 { 
            // anonymous parameter
            par = NewParam(ftype.Pos(), check.pkg, "", typ);
            check.recordImplicit(field, par);
            params = append(params, par);
            anonymous = true;

        }
    }    if (named && anonymous) {
        check.error(list[0], invalidAST + "list contains both named and anonymous parameters"); 
        // ok to continue
    }
    if (variadic) {
        var last = params[len(params) - 1];
        last.typ = addr(new Slice(elem:last.typ));
        check.recordTypeAndValue(list[len(list) - 1].Type, typexpr, last.typ, null);
    }
    return ;

}

private static bool declareInSet(this ptr<Checker> _addr_check, ptr<objset> _addr_oset, syntax.Pos pos, Object obj) {
    ref Checker check = ref _addr_check.val;
    ref objset oset = ref _addr_oset.val;

    {
        var alt = oset.insert(obj);

        if (alt != null) {
            ref error_ err = ref heap(out ptr<error_> _addr_err);
            err.errorf(pos, "%s redeclared", obj.Name());
            err.recordAltDecl(alt);
            check.report(_addr_err);
            return false;
        }
    }

    return true;

}

private static void interfaceType(this ptr<Checker> _addr_check, ptr<Interface> _addr_ityp, ptr<syntax.InterfaceType> _addr_iface, ptr<Named> _addr_def) {
    ref Checker check = ref _addr_check.val;
    ref Interface ityp = ref _addr_ityp.val;
    ref syntax.InterfaceType iface = ref _addr_iface.val;
    ref Named def = ref _addr_def.val;

    ptr<syntax.Name> tname; // most recent "type" name
    slice<syntax.Expr> types = default;
    foreach (var (_, f) in iface.MethodList) {
        if (f.Name != null) { 
            // We have a method with name f.Name, or a type
            // of a type list (f.Name.Value == "type").
            var name = f.Name.Value;
            if (name == "_") {
                if (check.conf.CompilerErrorMessages) {
                    check.error(f.Name, "methods must have a unique non-blank name");
                }
                else
 {
                    check.error(f.Name, "invalid method name _");
                }

                continue; // ignore
            }

            if (name == "type") { 
                // Always collect all type list entries, even from
                // different type lists, under the assumption that
                // the author intended to include all types.
                types = append(types, f.Type);
                if (tname != null && tname != f.Name) {
                    check.error(f.Name, "cannot have multiple type lists in an interface");
                }

                tname = f.Name;
                continue;

            }

            var typ = check.typ(f.Type);
            ptr<Signature> (sig, _) = typ._<ptr<Signature>>();
            if (sig == null) {
                if (typ != Typ[Invalid]) {
                    check.errorf(f.Type, invalidAST + "%s is not a method signature", typ);
                }
                continue; // ignore
            } 

            // Always type-check method type parameters but complain if they are not enabled.
            // (This extra check is needed here because interface method signatures don't have
            // a receiver specification.)
            if (sig.tparams != null && !acceptMethodTypeParams) {
                check.error(f.Type, "methods cannot have type parameters");
            } 

            // use named receiver type if available (for better error messages)
            Type recvTyp = ityp;
            if (def != null) {
                recvTyp = def;
            }

            sig.recv = NewVar(f.Name.Pos(), check.pkg, "", recvTyp);

            var m = NewFunc(f.Name.Pos(), check.pkg, name, sig);
            check.recordDef(f.Name, m);
            ityp.methods = append(ityp.methods, m);

        }
        else
 { 
            // We have an embedded type. completeInterface will
            // eventually verify that we have an interface.
            ityp.embeddeds = append(ityp.embeddeds, check.typ(f.Type));
            check.posMap[ityp] = append(check.posMap[ityp], f.Type.Pos());

        }
    }    ityp.types = NewSum(check.collectTypeConstraints(iface.Pos(), types));

    if (len(ityp.methods) == 0 && ityp.types == null && len(ityp.embeddeds) == 0) { 
        // empty interface
        ityp.allMethods = markComplete;
        return ;

    }
    sortMethods(ityp.methods);
    sortTypes(ityp.embeddeds);

    check.later(() => {
        check.completeInterface(iface.Pos(), ityp);
    });

}

private static void completeInterface(this ptr<Checker> _addr_check, syntax.Pos pos, ptr<Interface> _addr_ityp) => func((defer, panic, _) => {
    ref Checker check = ref _addr_check.val;
    ref Interface ityp = ref _addr_ityp.val;

    if (ityp.allMethods != null) {
        return ;
    }
    if (check == null) {
        panic("internal error: incomplete interface");
    }
    if (check.conf.Trace) { 
        // Types don't generally have position information.
        // If we don't have a valid pos provided, try to use
        // one close enough.
        if (!pos.IsKnown() && len(ityp.methods) > 0) {
            pos = ityp.methods[0].pos;
        }
        check.trace(pos, "complete %s", ityp);
        check.indent++;
        defer(() => {
            check.indent--;
            check.trace(pos, "=> %s (methods = %v, types = %v)", ityp, ityp.allMethods, ityp.allTypes);
        }());

    }
    ityp.allMethods = markComplete; 

    // Methods of embedded interfaces are collected unchanged; i.e., the identity
    // of a method I.m's Func Object of an interface I is the same as that of
    // the method m in an interface that embeds interface I. On the other hand,
    // if a method is embedded via multiple overlapping embedded interfaces, we
    // don't provide a guarantee which "original m" got chosen for the embedding
    // interface. See also issue #34421.
    //
    // If we don't care to provide this identity guarantee anymore, instead of
    // reusing the original method in embeddings, we can clone the method's Func
    // Object and give it the position of a corresponding embedded interface. Then
    // we can get rid of the mpos map below and simply use the cloned method's
    // position.

    objset seen = default;
    slice<ptr<Func>> methods = default;
    var mpos = make_map<ptr<Func>, syntax.Pos>(); // method specification or method embedding position, for good error messages
    Action<syntax.Pos, ptr<Func>, bool> addMethod = (pos, m, @explicit) => {
        {
            var other = seen.insert(m);


            if (other == null) 
                methods = append(methods, m);
                mpos[m] = pos;
            else if (explicit) 
                ref error_ err = ref heap(out ptr<error_> _addr_err);
                err.errorf(pos, "duplicate method %s", m.name);
                err.errorf(mpos[other._<ptr<Func>>()], "other declaration of %s", m.name);
                check.report(_addr_err);
            else 
                // We have a duplicate method name in an embedded (not explicitly declared) method.
                // Check method signatures after all types are computed (issue #33656).
                // If we're pre-go1.14 (overlapping embeddings are not permitted), report that
                // error here as well (even though we could do it eagerly) because it's the same
                // error message.
                check.later(() => {
                    if (!check.allowVersion(m.pkg, 1, 14) || !check.identical(m.typ, other.Type())) {
                        err = default;
                        err.errorf(pos, "duplicate method %s", m.name);
                        err.errorf(mpos[other._<ptr<Func>>()], "other declaration of %s", m.name);
                        check.report(_addr_err);
                    }
                });

        }

    };

    {
        var m__prev1 = m;

        foreach (var (_, __m) in ityp.methods) {
            m = __m;
            addMethod(m.pos, m, true);
        }
        m = m__prev1;
    }

    var allTypes = ityp.types;

    var posList = check.posMap[ityp];
    foreach (var (i, typ) in ityp.embeddeds) {
        var pos = posList[i]; // embedding position
        var utyp = under(typ);
        var etyp = asInterface(utyp);
        if (etyp == null) {
            if (utyp != Typ[Invalid]) {
                @string format = default;
                {
                    ptr<TypeParam> (_, ok) = utyp._<ptr<TypeParam>>();

                    if (ok) {
                        format = "%s is a type parameter, not an interface";
                    }
                    else
 {
                        format = "%s is not an interface";
                    }

                }

                check.errorf(pos, format, typ);

            }

            continue;

        }
        check.completeInterface(pos, etyp);
        {
            var m__prev2 = m;

            foreach (var (_, __m) in etyp.allMethods) {
                m = __m;
                addMethod(pos, m, false); // use embedding position pos rather than m.pos
            }

            m = m__prev2;
        }

        allTypes = intersect(allTypes, etyp.allTypes);

    }    if (methods != null) {
        sortMethods(methods);
        ityp.allMethods = methods;
    }
    ityp.allTypes = allTypes;

});

// intersect computes the intersection of the types x and y.
// Note: A incomming nil type stands for the top type. A top
// type result is returned as nil.
private static Type intersect(Type x, Type y) => func((defer, _, _) => {
    Type r = default;

    defer(() => {
        if (r == theTop) {
            r = null;
        }
    }());


    if (x == theBottom || y == theBottom) 
        return theBottom;
    else if (x == null || x == theTop) 
        return y;
    else if (y == null || x == theTop) 
        return x;
        var xtypes = unpack(x);
    var ytypes = unpack(y); 
    // Compute the list rtypes which includes only
    // types that are in both xtypes and ytypes.
    // Quadratic algorithm, but good enough for now.
    // TODO(gri) fix this
    slice<Type> rtypes = default;
    foreach (var (_, x) in xtypes) {
        if (includes(ytypes, x)) {
            rtypes = append(rtypes, x);
        }
    }    if (rtypes == null) {
        return theBottom;
    }
    return NewSum(rtypes);

});

private static void sortTypes(slice<Type> list) {
    sort.Stable(byUniqueTypeName(list));
}

// byUniqueTypeName named type lists can be sorted by their unique type names.
private partial struct byUniqueTypeName { // : slice<Type>
}

private static nint Len(this byUniqueTypeName a) {
    return len(a);
}
private static bool Less(this byUniqueTypeName a, nint i, nint j) {
    return sortName(a[i]) < sortName(a[j]);
}
private static void Swap(this byUniqueTypeName a, nint i, nint j) {
    (a[i], a[j]) = (a[j], a[i]);
}

private static @string sortName(Type t) {
    {
        var named = asNamed(t);

        if (named != null) {
            return named.obj.Id();
        }
    }

    return "";

}

private static void sortMethods(slice<ptr<Func>> list) {
    sort.Sort(byUniqueMethodName(list));
}

private static void assertSortedMethods(slice<ptr<Func>> list) => func((_, panic, _) => {
    if (!debug) {
        panic("internal error: assertSortedMethods called outside debug mode");
    }
    if (!sort.IsSorted(byUniqueMethodName(list))) {
        panic("internal error: methods not sorted");
    }
});

// byUniqueMethodName method lists can be sorted by their unique method names.
private partial struct byUniqueMethodName { // : slice<ptr<Func>>
}

private static nint Len(this byUniqueMethodName a) {
    return len(a);
}
private static bool Less(this byUniqueMethodName a, nint i, nint j) {
    return a[i].less(a[j]);
}
private static void Swap(this byUniqueMethodName a, nint i, nint j) {
    (a[i], a[j]) = (a[j], a[i]);
}

private static @string tag(this ptr<Checker> _addr_check, ptr<syntax.BasicLit> _addr_t) {
    ref Checker check = ref _addr_check.val;
    ref syntax.BasicLit t = ref _addr_t.val;
 
    // If t.Bad, an error was reported during parsing.
    if (t != null && !t.Bad) {
        if (t.Kind == syntax.StringLit) {
            {
                var (val, err) = strconv.Unquote(t.Value);

                if (err == null) {
                    return val;
                }

            }

        }
        check.errorf(t, invalidAST + "incorrect tag syntax: %q", t.Value);

    }
    return "";

}

private static void structType(this ptr<Checker> _addr_check, ptr<Struct> _addr_styp, ptr<syntax.StructType> _addr_e) {
    ref Checker check = ref _addr_check.val;
    ref Struct styp = ref _addr_styp.val;
    ref syntax.StructType e = ref _addr_e.val;

    if (e.FieldList == null) {
        return ;
    }
    slice<ptr<Var>> fields = default;
    slice<@string> tags = default; 

    // for double-declaration checks
    ref objset fset = ref heap(out ptr<objset> _addr_fset); 

    // current field typ and tag
    Type typ = default;
    @string tag = default;
    Action<ptr<syntax.Name>, bool, syntax.Pos> add = (ident, embedded, pos) => {
        if (tag != "" && tags == null) {
            tags = make_slice<@string>(len(fields));
        }
        if (tags != null) {
            tags = append(tags, tag);
        }
        var name = ident.Value;
        var fld = NewField(pos, check.pkg, name, typ, embedded); 
        // spec: "Within a struct, non-blank field names must be unique."
        if (name == "_" || check.declareInSet(_addr_fset, pos, fld)) {
            fields = append(fields, fld);
            check.recordDef(ident, fld);
        }
    }; 

    // addInvalid adds an embedded field of invalid type to the struct for
    // fields with errors; this keeps the number of struct fields in sync
    // with the source as long as the fields are _ or have different names
    // (issue #25627).
    Action<ptr<syntax.Name>, syntax.Pos> addInvalid = (ident, pos) => {
        typ = Typ[Invalid];
        tag = "";
        add(ident, true, pos);
    };

    syntax.Expr prev = default;
    foreach (var (i, f) in e.FieldList) { 
        // Fields declared syntactically with the same type (e.g.: a, b, c T)
        // share the same type expression. Only check type if it's a new type.
        if (i == 0 || f.Type != prev) {
            typ = check.varType(f.Type);
            prev = f.Type;
        }
        tag = "";
        if (i < len(e.TagList)) {
            tag = check.tag(e.TagList[i]);
        }
        if (f.Name != null) { 
            // named field
            add(f.Name, false, f.Name.Pos());

        }
        else
 { 
            // embedded field
            // spec: "An embedded type must be specified as a type name T or as a
            // pointer to a non-interface type name *T, and T itself may not be a
            // pointer type."
            var pos = syntax.StartPos(f.Type);
            name = embeddedFieldIdent(f.Type);
            if (name == null) {
                check.errorf(pos, "invalid embedded field type %s", f.Type);
                name = addr(new syntax.Name(Value:"_")); // TODO(gri) need to set position to pos
                addInvalid(name, pos);
                continue;

            }

            add(name, true, pos); 

            // Because we have a name, typ must be of the form T or *T, where T is the name
            // of a (named or alias) type, and t (= deref(typ)) must be the type of T.
            // We must delay this check to the end because we don't want to instantiate
            // (via under(t)) a possibly incomplete type.
            var embeddedTyp = typ; // for closure below
            var embeddedPos = pos;
            check.later(() => {
                var (t, isPtr) = deref(embeddedTyp);
                switch (optype(t).type()) {
                    case ptr<Basic> t:
                        if (t == Typ[Invalid]) { 
                            // error was reported before
                            return ;

                        } 
                        // unsafe.Pointer is treated like a regular pointer
                        if (t.kind == UnsafePointer) {
                            check.error(embeddedPos, "embedded field type cannot be unsafe.Pointer");
                        }

                        break;
                    case ptr<Pointer> t:
                        check.error(embeddedPos, "embedded field type cannot be a pointer");
                        break;
                    case ptr<Interface> t:
                        if (isPtr) {
                            check.error(embeddedPos, "embedded field type cannot be a pointer to an interface");
                        }
                        break;
                }

            });

        }
    }    styp.fields = fields;
    styp.tags = tags;

}

private static ptr<syntax.Name> embeddedFieldIdent(syntax.Expr e) {
    switch (e.type()) {
        case ptr<syntax.Name> e:
            return _addr_e!;
            break;
        case ptr<syntax.Operation> e:
            {
                var @base = ptrBase(_addr_e);

                if (base != null) { 
                    // *T is valid, but **T is not
                    {
                        ptr<syntax.Operation> (op, _) = base._<ptr<syntax.Operation>>();

                        if (op == null || ptrBase(op) == null) {
                            return _addr_embeddedFieldIdent(e.X)!;
                        }

                    }

                }

            }

            break;
        case ptr<syntax.SelectorExpr> e:
            return _addr_e.Sel!;
            break;
        case ptr<syntax.IndexExpr> e:
            return _addr_embeddedFieldIdent(e.X)!;
            break;
    }
    return _addr_null!; // invalid embedded field
}

private static slice<Type> collectTypeConstraints(this ptr<Checker> _addr_check, syntax.Pos pos, slice<syntax.Expr> types) {
    ref Checker check = ref _addr_check.val;

    var list = make_slice<Type>(0, len(types)); // assume all types are correct
    foreach (var (_, texpr) in types) {
        if (texpr == null) {
            check.error(pos, invalidAST + "missing type constraint");
            continue;
        }
        list = append(list, check.varType(texpr));

    }    check.later(() => {
        {
            var t__prev1 = t;

            foreach (var (__i, __t) in list) {
                i = __i;
                t = __t;
                {
                    var t__prev1 = t;

                    var t = asInterface(t);

                    if (t != null) {
                        check.completeInterface(types[i].Pos(), t);
                    }

                    t = t__prev1;

                }

                if (includes(list[..(int)i], t)) {
                    check.softErrorf(types[i], "duplicate type %s in type list", t);
                }

            }

            t = t__prev1;
        }
    });

    return list;

}

// includes reports whether typ is in list
private static bool includes(slice<Type> list, Type typ) {
    foreach (var (_, e) in list) {
        if (Identical(typ, e)) {
            return true;
        }
    }    return false;

}

private static syntax.Expr ptrBase(ptr<syntax.Operation> _addr_x) {
    ref syntax.Operation x = ref _addr_x.val;

    if (x.Op == syntax.Mul && x.Y == null) {
        return x.X;
    }
    return null;

}

} // end types2_package
