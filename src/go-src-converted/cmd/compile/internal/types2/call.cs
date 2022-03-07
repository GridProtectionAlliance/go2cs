// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements typechecking of call and selector expressions.

// package types2 -- go2cs converted at 2022 March 06 23:12:25 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\call.go
using syntax = go.cmd.compile.@internal.syntax_package;
using strings = go.strings_package;
using unicode = go.unicode_package;

namespace go.cmd.compile.@internal;

public static partial class types2_package {

    // funcInst type-checks a function instantiation inst and returns the result in x.
    // The operand x must be the evaluation of inst.X and its type must be a signature.
private static void funcInst(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<syntax.IndexExpr> _addr_inst) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref syntax.IndexExpr inst = ref _addr_inst.val;

    var xlist = unpackExpr(inst.Index);
    var targs = check.typeList(xlist);
    if (targs == null) {
        x.mode = invalid;
        x.expr = inst;
        return ;
    }
    assert(len(targs) == len(xlist)); 

    // check number of type arguments (got) vs number of type parameters (want)
    ptr<Signature> sig = x.typ._<ptr<Signature>>();
    var got = len(targs);
    var want = len(sig.tparams);
    if (!useConstraintTypeInference && got != want || got > want) {
        check.errorf(xlist[got - 1], "got %d type arguments but want %d", got, want);
        x.mode = invalid;
        x.expr = inst;
        return ;
    }
    var inferred = false;
    if (got < want) {
        targs = check.infer(inst.Pos(), sig.tparams, targs, null, null, true);
        if (targs == null) { 
            // error was already reported
            x.mode = invalid;
            x.expr = inst;
            return ;

        }
        got = len(targs);
        inferred = true;

    }
    assert(got == want); 

    // determine argument positions (for error reporting)
    var poslist = make_slice<syntax.Pos>(len(xlist));
    foreach (var (i, x) in xlist) {
        poslist[i] = syntax.StartPos(x);
    }    ptr<Signature> res = check.instantiate(x.Pos(), sig, targs, poslist)._<ptr<Signature>>();
    assert(res.tparams == null); // signature is not generic anymore
    if (inferred) {
        check.recordInferred(inst, targs, res);
    }
    x.typ = res;
    x.mode = value;
    x.expr = inst;

}

private static exprKind callExpr(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<syntax.CallExpr> _addr_call) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref syntax.CallExpr call = ref _addr_call.val;

    ptr<syntax.IndexExpr> inst; // function instantiation, if any
    {
        ptr<syntax.IndexExpr> (iexpr, _) = call.Fun._<ptr<syntax.IndexExpr>>();

        if (iexpr != null) {
            if (check.indexExpr(x, iexpr)) { 
                // Delay function instantiation to argument checking,
                // where we combine type and value arguments for type
                // inference.
                assert(x.mode == value);
                inst = iexpr;

            }

            x.expr = iexpr;
            check.record(x);

        }
        else
 {
            check.exprOrType(x, call.Fun);
        }
    }



    if (x.mode == invalid) 
        check.use(call.ArgList);
        x.expr = call;
        return statement;
    else if (x.mode == typexpr) 
        // conversion
        var T = x.typ;
        x.mode = invalid;
        {
            var n = len(call.ArgList);

            switch (n) {
                case 0: 
                    check.errorf(call, "missing argument in conversion to %s", T);
                    break;
                case 1: 
                    check.expr(x, call.ArgList[0]);
                    if (x.mode != invalid) {
                        {
                            var t = asInterface(T);

                            if (t != null) {
                                check.completeInterface(nopos, t);
                                if (t.IsConstraint()) {
                                    check.errorf(call, "cannot use interface %s in conversion (contains type list or is comparable)", T);
                                    break;
                                }
                            }

                        }

                        if (call.HasDots) {
                            check.errorf(call.ArgList[0], "invalid use of ... in type conversion to %s", T);
                            break;
                        }

                        check.conversion(x, T);

                    }

                    break;
                default: 
                    check.use(call.ArgList);
                    check.errorf(call.ArgList[n - 1], "too many arguments in conversion to %s", T);
                    break;
            }
        }
        x.expr = call;
        return conversion;
    else if (x.mode == builtin) 
        var id = x.id;
        if (!check.builtin(x, call, id)) {
            x.mode = invalid;
        }
        x.expr = call; 
        // a non-constant result implies a function call
        if (x.mode != invalid && x.mode != constant_) {
            check.hasCallOrRecv = true;
        }
        return predeclaredFuncs[id].kind;
    // ordinary function/method call
    var cgocall = x.mode == cgofunc;

    var sig = asSignature(x.typ);
    if (sig == null) {
        check.errorf(x, invalidOp + "cannot call non-function %s", x);
        x.mode = invalid;
        x.expr = call;
        return statement;
    }
    slice<Type> targs = default;
    if (inst != null) {
        var xlist = unpackExpr(inst.Index);
        targs = check.typeList(xlist);
        if (targs == null) {
            check.use(call.ArgList);
            x.mode = invalid;
            x.expr = call;
            return statement;
        }
        assert(len(targs) == len(xlist)); 

        // check number of type arguments (got) vs number of type parameters (want)
        var got = len(targs);
        var want = len(sig.tparams);
        if (got > want) {
            check.errorf(xlist[want], "got %d type arguments but want %d", got, want);
            check.use(call.ArgList);
            x.mode = invalid;
            x.expr = call;
            return statement;
        }
    }
    var (args, _) = check.exprList(call.ArgList, false);
    sig = check.arguments(call, sig, targs, args); 

    // determine result
    switch (sig.results.Len()) {
        case 0: 
            x.mode = novalue;
            break;
        case 1: 
                   if (cgocall) {
                       x.mode = commaerr;
                   }
                   else
            {
                       x.mode = value;
                   }
                   x.typ = sig.results.vars[0].typ; // unpack tuple
            break;
        default: 
            x.mode = value;
            x.typ = sig.results;
            break;
    }
    x.expr = call;
    check.hasCallOrRecv = true; 

    // if type inference failed, a parametrized result must be invalidated
    // (operands cannot have a parametrized type)
    if (x.mode == value && len(sig.tparams) > 0 && isParameterized(sig.tparams, x.typ)) {
        x.mode = invalid;
    }
    return statement;

}

private static (slice<ptr<operand>>, bool) exprList(this ptr<Checker> _addr_check, slice<syntax.Expr> elist, bool allowCommaOk) {
    slice<ptr<operand>> xlist = default;
    bool commaOk = default;
    ref Checker check = ref _addr_check.val;

    switch (len(elist)) {
        case 0: 

            break;
        case 1: 
            // single (possibly comma-ok) value, or function returning multiple values
            var e = elist[0];
            ref operand x = ref heap(out ptr<operand> _addr_x);
            check.multiExpr(_addr_x, e);
            {
                ptr<Tuple> (t, ok) = x.typ._<ptr<Tuple>>();

                if (ok && x.mode != invalid) { 
                    // multiple values
                    xlist = make_slice<ptr<operand>>(t.Len());
                    {
                        var i__prev1 = i;

                        foreach (var (__i, __v) in t.vars) {
                            i = __i;
                            v = __v;
                            xlist[i] = addr(new operand(mode:value,expr:e,typ:v.typ));
                        }

                        i = i__prev1;
                    }

                    break;

                } 

                // exactly one (possibly invalid or comma-ok) value

            } 

            // exactly one (possibly invalid or comma-ok) value
            xlist = new slice<ptr<operand>>(new ptr<operand>[] { &x });
            if (allowCommaOk && (x.mode == mapindex || x.mode == commaok || x.mode == commaerr)) {
                x.mode = value;
                xlist = append(xlist, addr(new operand(mode:value,expr:e,typ:Typ[UntypedBool])));
                commaOk = true;
            }
            break;
        default: 
            // multiple (possibly invalid) values
            xlist = make_slice<ptr<operand>>(len(elist));
            {
                var i__prev1 = i;
                var e__prev1 = e;

                foreach (var (__i, __e) in elist) {
                    i = __i;
                    e = __e;
                    x = default;
                    check.expr(_addr_x, e);
                    _addr_xlist[i] = _addr_x;
                    xlist[i] = ref _addr_xlist[i].val;

                }

                i = i__prev1;
                e = e__prev1;
            }
            break;
    }

    return ;

}

private static ptr<Signature> arguments(this ptr<Checker> _addr_check, ptr<syntax.CallExpr> _addr_call, ptr<Signature> _addr_sig, slice<Type> targs, slice<ptr<operand>> args) {
    ptr<Signature> rsig = default!;
    ref Checker check = ref _addr_check.val;
    ref syntax.CallExpr call = ref _addr_call.val;
    ref Signature sig = ref _addr_sig.val;

    rsig = sig; 

    // TODO(gri) try to eliminate this extra verification loop
    {
        var a__prev1 = a;

        foreach (var (_, __a) in args) {
            a = __a;

            if (a.mode == typexpr) 
                check.errorf(a, "%s used as value", a);
                return ;
            else if (a.mode == invalid) 
                return ;
            
        }
        a = a__prev1;
    }

    var nargs = len(args);
    var npars = sig.@params.Len();
    var ddd = call.HasDots; 

    // set up parameters
    var sigParams = sig.@params; // adjusted for variadic functions (may be nil for empty parameter lists!)
    var adjusted = false; // indicates if sigParams is different from t.params
    if (sig.variadic) {
        if (ddd) { 
            // variadic_func(a, b, c...)
            if (len(call.ArgList) == 1 && nargs > 1) { 
                // f()... is not permitted if f() is multi-valued
                //check.errorf(call.Ellipsis, "cannot use ... with %d-valued %s", nargs, call.ArgList[0])
                check.errorf(call, "cannot use ... with %d-valued %s", nargs, call.ArgList[0]);
                return ;

            }

        }
        else
 { 
            // variadic_func(a, b, c)
            if (nargs >= npars - 1) { 
                // Create custom parameters for arguments: keep
                // the first npars-1 parameters and add one for
                // each argument mapping to the ... parameter.
                var vars = make_slice<ptr<Var>>(npars - 1); // npars > 0 for variadic functions
                copy(vars, sig.@params.vars);
                var last = sig.@params.vars[npars - 1];
                ptr<Slice> typ = last.typ._<ptr<Slice>>().elem;
                while (len(vars) < nargs) {
                    vars = append(vars, NewParam(last.pos, last.pkg, last.name, typ));
                }
            else

                sigParams = NewTuple(vars); // possibly nil!
                adjusted = true;
                npars = nargs;

            } { 
                // nargs < npars-1
                npars--; // for correct error message below
            }

        }
    else
    } {
        if (ddd) { 
            // standard_func(a, b, c...)
            //check.errorf(call.Ellipsis, "cannot use ... in call to non-variadic %s", call.Fun)
            check.errorf(call, "cannot use ... in call to non-variadic %s", call.Fun);
            return ;

        }
    }

    if (nargs < npars) 
        check.errorf(call, "not enough arguments in call to %s", call.Fun);
        return ;
    else if (nargs > npars) 
        check.errorf(args[npars], "too many arguments in call to %s", call.Fun); // report at first extra argument
        return ;
    // infer type arguments and instantiate signature if necessary
    if (len(sig.tparams) > 0) { 
        // TODO(gri) provide position information for targs so we can feed
        //           it to the instantiate call for better error reporting
        targs = check.infer(call.Pos(), sig.tparams, targs, sigParams, args, true);
        if (targs == null) {
            return ; // error already reported
        }
        rsig = check.instantiate(call.Pos(), sig, targs, null)._<ptr<Signature>>();
        assert(rsig.tparams == null); // signature is not generic anymore
        check.recordInferred(call, targs, rsig); 

        // Optimization: Only if the parameter list was adjusted do we
        // need to compute it from the adjusted list; otherwise we can
        // simply use the result signature's parameter list.
        if (adjusted) {
            sigParams = check.subst(call.Pos(), sigParams, makeSubstMap(sig.tparams, targs))._<ptr<Tuple>>();
        }
        else
 {
            sigParams = rsig.@params;
        }
    }
    {
        var a__prev1 = a;

        foreach (var (__i, __a) in args) {
            i = __i;
            a = __a;
            check.assignment(a, sigParams.vars[i].typ, check.sprintf("argument to %s", call.Fun));
        }
        a = a__prev1;
    }

    return ;

}

private static array<@string> cgoPrefixes = new array<@string>(new @string[] { "_Ciconst_", "_Cfconst_", "_Csconst_", "_Ctype_", "_Cvar_", "_Cfpvar_fp_", "_Cfunc_", "_Cmacro_" });

private static void selector(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<syntax.SelectorExpr> _addr_e) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref syntax.SelectorExpr e = ref _addr_e.val;
 
    // these must be declared before the "goto Error" statements
    Object obj = default;    slice<nint> index = default;    bool indirect = default;

    var sel = e.Sel.Value; 
    // If the identifier refers to a package, handle everything here
    // so we don't need a "package" mode for operands: package names
    // can only appear in qualified identifiers which are mapped to
    // selector expressions.
    {
        ptr<syntax.Name> (ident, ok) = e.X._<ptr<syntax.Name>>();

        if (ok) {
            obj = check.lookup(ident.Value);
            {
                ptr<PkgName> (pname, _) = obj._<ptr<PkgName>>();

                if (pname != null) {
                    assert(pname.pkg == check.pkg);
                    check.recordUse(ident, pname);
                    pname.used = true;
                    var pkg = pname.imported;

                    Object exp = default;
                    var funcMode = value;
                    if (pkg.cgo) { 
                        // cgo special cases C.malloc: it's
                        // rewritten to _CMalloc and does not
                        // support two-result calls.
                        if (sel == "malloc") {
                            sel = "_CMalloc";
                        }
                        else
 {
                            funcMode = cgofunc;
                        }

                        foreach (var (_, prefix) in cgoPrefixes) { 
                            // cgo objects are part of the current package (in file
                            // _cgo_gotypes.go). Use regular lookup.
                            _, exp = check.scope.LookupParent(prefix + sel, check.pos);
                            if (exp != null) {
                                break;
                            }

                        }
                    else
                        if (exp == null) {
                            check.errorf(e.Sel, "%s not declared by package C", sel);
                            goto Error;
                        }

                        check.objDecl(exp, null);

                    } {
                        exp = pkg.scope.Lookup(sel);
                        if (exp == null) {
                            if (!pkg.fake) {
                                if (check.conf.CompilerErrorMessages) {
                                    check.errorf(e.Sel, "undefined: %s.%s", pkg.name, sel);
                                }
                                else
 {
                                    check.errorf(e.Sel, "%s not declared by package %s", sel, pkg.name);
                                }

                            }

                            goto Error;

                        }

                        if (!exp.Exported()) {
                            check.errorf(e.Sel, "%s not exported by package %s", sel, pkg.name); 
                            // ok to continue
                        }

                    }

                    check.recordUse(e.Sel, exp); 

                    // Simplified version of the code for *syntax.Names:
                    // - imported objects are always fully initialized
                    switch (exp.type()) {
                        case ptr<Const> exp:
                            assert(exp.Val() != null);
                            x.mode = constant_;
                            x.typ = exp.typ;
                            x.val = exp.val;
                            break;
                        case ptr<TypeName> exp:
                            x.mode = typexpr;
                            x.typ = exp.typ;
                            break;
                        case ptr<Var> exp:
                            x.mode = variable;
                            x.typ = exp.typ;
                            if (pkg.cgo && strings.HasPrefix(exp.name, "_Cvar_")) {
                                x.typ = x.typ._<ptr<Pointer>>().@base;
                            }
                            break;
                        case ptr<Func> exp:
                            x.mode = funcMode;
                            x.typ = exp.typ;
                            if (pkg.cgo && strings.HasPrefix(exp.name, "_Cmacro_")) {
                                x.mode = value;
                                x.typ = x.typ._<ptr<Signature>>().results.vars[0].typ;
                            }
                            break;
                        case ptr<Builtin> exp:
                            x.mode = builtin;
                            x.typ = exp.typ;
                            x.id = exp.id;
                            break;
                        default:
                        {
                            var exp = exp.type();
                            check.dump("%v: unexpected object %v", posFor(e.Sel), exp);
                            unreachable();
                            break;
                        }
                    }
                    x.expr = e;
                    return ;

                }

            }

        }
    }


    check.exprOrType(x, e.X);
    if (x.mode == invalid) {
        goto Error;
    }
    check.instantiatedOperand(x);

    obj, index, indirect = check.lookupFieldOrMethod(x.typ, x.mode == variable, check.pkg, sel);
    if (obj == null) {

        if (index != null) 
            // TODO(gri) should provide actual type where the conflict happens
            check.errorf(e.Sel, "ambiguous selector %s.%s", x.expr, sel);
        else if (indirect) 
            check.errorf(e.Sel, "cannot call pointer method %s on %s", sel, x.typ);
        else 
            @string why = default;
            {
                var tpar = asTypeParam(x.typ);

                if (tpar != null) { 
                    // Type parameter bounds don't specify fields, so don't mention "field".
                    switch (tpar.Bound().obj.type()) {
                        case 
                            why = check.sprintf("type bound for %s has no method %s", x.typ, sel);
                            break;
                        case ptr<TypeName> obj:
                            why = check.sprintf("interface %s has no method %s", obj.name, sel);
                            break;
                    }

                }
                else
 {
                    why = check.sprintf("type %s has no field or method %s", x.typ, sel);
                } 

                // Check if capitalization of sel matters and provide better error message in that case.

            } 

            // Check if capitalization of sel matters and provide better error message in that case.
            if (len(sel) > 0) {
                @string changeCase = default;
                {
                    var r = rune(sel[0]);

                    if (unicode.IsUpper(r)) {
                        changeCase = string(unicode.ToLower(r)) + sel[(int)1..];
                    }
                    else
 {
                        changeCase = string(unicode.ToUpper(r)) + sel[(int)1..];
                    }

                }

                obj, _, _ = check.lookupFieldOrMethod(x.typ, x.mode == variable, check.pkg, changeCase);

                if (obj != null) {
                    why += ", but does have " + changeCase;
                }

            }

            check.errorf(e.Sel, "%s.%s undefined (%s)", x.expr, sel, why);
                goto Error;

    }
    {
        ptr<Func> m__prev1 = m;

        ptr<Func> (m, _) = obj._<ptr<Func>>();

        if (m != null) { 
            // check.dump("### found method %s", m)
            check.objDecl(m, null); 
            // If m has a parameterized receiver type, infer the type arguments from
            // the actual receiver provided and then substitute the type parameters in
            // the signature accordingly.
            // TODO(gri) factor this code out
            ptr<Signature> sig = m.typ._<ptr<Signature>>();
            if (len(sig.rparams) > 0) { 
                // For inference to work, we must use the receiver type
                // matching the receiver in the actual method declaration.
                // If the method is embedded, the matching receiver is the
                // embedded struct or interface that declared the method.
                // Traverse the embedding to find that type (issue #44688).
                var recv = x.typ;
                for (nint i = 0; i < len(index) - 1; i++) { 
                    // The embedded type is either a struct or a pointer to
                    // a struct except for the last one (which we don't need).
                    recv = asStruct(derefStructPtr(recv)).Field(index[i]).typ;

                } 
                //check.dump("### recv = %s", recv)
                //check.dump("### method = %s rparams = %s tparams = %s", m, sig.rparams, sig.tparams)
                // The method may have a pointer receiver, but the actually provided receiver
                // may be a (hopefully addressable) non-pointer value, or vice versa. Here we
                // only care about inferring receiver type parameters; to make the inference
                // work, match up pointer-ness of receiver and argument.
 
                //check.dump("### recv = %s", recv)
                //check.dump("### method = %s rparams = %s tparams = %s", m, sig.rparams, sig.tparams)
                // The method may have a pointer receiver, but the actually provided receiver
                // may be a (hopefully addressable) non-pointer value, or vice versa. Here we
                // only care about inferring receiver type parameters; to make the inference
                // work, match up pointer-ness of receiver and argument.
                {
                    var ptrRecv = isPointer(sig.recv.typ);

                    if (ptrRecv != isPointer(recv)) {
                        if (ptrRecv) {
                            recv = NewPointer(recv);
                        }
                        else
 {
                            recv = recv._<ptr<Pointer>>().@base;
                        }

                    } 
                    // Disable reporting of errors during inference below. If we're unable to infer
                    // the receiver type arguments here, the receiver must be be otherwise invalid
                    // and an error has been reported elsewhere.

                } 
                // Disable reporting of errors during inference below. If we're unable to infer
                // the receiver type arguments here, the receiver must be be otherwise invalid
                // and an error has been reported elsewhere.
                ref operand arg = ref heap(new operand(mode:variable,expr:x.expr,typ:recv), out ptr<operand> _addr_arg);
                var targs = check.infer(m.pos, sig.rparams, null, NewTuple(sig.recv), new slice<ptr<operand>>(new ptr<operand>[] { &arg }), false); 
                //check.dump("### inferred targs = %s", targs)
                if (targs == null) { 
                    // We may reach here if there were other errors (see issue #40056).
                    goto Error;

                } 
                // Don't modify m. Instead - for now - make a copy of m and use that instead.
                // (If we modify m, some tests will fail; possibly because the m is in use.)
                // TODO(gri) investigate and provide a correct explanation here
                ref var copy = ref heap(m.val, out ptr<var> _addr_copy);
                copy.typ = check.subst(e.Pos(), m.typ, makeSubstMap(sig.rparams, targs));
                _addr_obj = _addr_copy;
                obj = ref _addr_obj.val;

            } 
            // TODO(gri) we also need to do substitution for parameterized interface methods
            //           (this breaks code in testdata/linalg.go2 at the moment)
            //           12/20/2019: Is this TODO still correct?
        }
        m = m__prev1;

    }


    if (x.mode == typexpr) { 
        // method expression
        (m, _) = obj._<ptr<Func>>();
        if (m == null) { 
            // TODO(gri) should check if capitalization of sel matters and provide better error message in that case
            check.errorf(e.Sel, "%s.%s undefined (type %s has no method %s)", x.expr, sel, x.typ, sel);
            goto Error;

        }
        check.recordSelection(e, MethodExpr, x.typ, m, index, indirect); 

        // the receiver type becomes the type of the first function
        // argument of the method expression's function type
        slice<ptr<Var>> @params = default;
        sig = m.typ._<ptr<Signature>>();
        if (sig.@params != null) {
            params = sig.@params.vars;
        }
        x.mode = value;
        x.typ = addr(new Signature(tparams:sig.tparams,params:NewTuple(append([]*Var{NewVar(nopos,check.pkg,"_",x.typ)},params...)...),results:sig.results,variadic:sig.variadic,));

        check.addDeclDep(m);


    }
    else
 { 
        // regular selector
        switch (obj.type()) {
            case ptr<Var> obj:
                check.recordSelection(e, FieldVal, x.typ, obj, index, indirect);
                if (x.mode == variable || indirect) {
                    x.mode = variable;
                }
                else
 {
                    x.mode = value;
                }

                x.typ = obj.typ;
                break;
            case ptr<Func> obj:
                check.recordSelection(e, MethodVal, x.typ, obj, index, indirect);

                x.mode = value; 

                // remove receiver
                sig = obj.typ._<ptr<Signature>>().val;
                sig.recv = null;
                x.typ = _addr_sig;

                check.addDeclDep(obj);
                break;
            default:
            {
                var obj = obj.type();
                unreachable();
                break;
            }
        }

    }
    x.expr = e;
    return ;

Error:
    x.mode = invalid;
    x.expr = e;

}

// use type-checks each argument.
// Useful to make sure expressions are evaluated
// (and variables are "used") in the presence of other errors.
// The arguments may be nil.
// TODO(gri) make this accept a []syntax.Expr and use an unpack function when we have a ListExpr?
private static void use(this ptr<Checker> _addr_check, params syntax.Expr[] arg) {
    arg = arg.Clone();
    ref Checker check = ref _addr_check.val;

    ref operand x = ref heap(out ptr<operand> _addr_x);
    foreach (var (_, e) in arg) { 
        // Certain AST fields may legally be nil (e.g., the ast.SliceExpr.High field).
        if (e == null) {
            continue;
        }
        {
            ptr<syntax.ListExpr> (l, _) = e._<ptr<syntax.ListExpr>>();

            if (l != null) {
                check.use(l.ElemList);
                continue;
            }

        }

        check.rawExpr(_addr_x, e, null);

    }
}

// useLHS is like use, but doesn't "use" top-level identifiers.
// It should be called instead of use if the arguments are
// expressions on the lhs of an assignment.
// The arguments must not be nil.
private static void useLHS(this ptr<Checker> _addr_check, params syntax.Expr[] arg) {
    arg = arg.Clone();
    ref Checker check = ref _addr_check.val;

    ref operand x = ref heap(out ptr<operand> _addr_x);
    foreach (var (_, e) in arg) { 
        // If the lhs is an identifier denoting a variable v, this assignment
        // is not a 'use' of v. Remember current value of v.used and restore
        // after evaluating the lhs via check.rawExpr.
        ptr<Var> v;
        bool v_used = default;
        {
            ptr<syntax.Name> (ident, _) = unparen(e)._<ptr<syntax.Name>>();

            if (ident != null) { 
                // never type-check the blank name on the lhs
                if (ident.Value == "_") {
                    continue;
                }

                {
                    var (_, obj) = check.scope.LookupParent(ident.Value, nopos);

                    if (obj != null) { 
                        // It's ok to mark non-local variables, but ignore variables
                        // from other packages to avoid potential race conditions with
                        // dot-imported variables.
                        {
                            ptr<Var> (w, _) = obj._<ptr<Var>>();

                            if (w != null && w.pkg == check.pkg) {
                                v = w;
                                v_used = v.used;
                            }

                        }

                    }

                }

            }

        }

        check.rawExpr(_addr_x, e, null);
        if (v != null) {
            v.used = v_used; // restore v.used
        }
    }
}

// instantiatedOperand reports an error of x is an uninstantiated (generic) type and sets x.typ to Typ[Invalid].
private static void instantiatedOperand(this ptr<Checker> _addr_check, ptr<operand> _addr_x) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

    if (x.mode == typexpr && isGeneric(x.typ)) {
        check.errorf(x, "cannot use generic type %s without instantiation", x.typ);
        x.typ = Typ[Invalid];
    }
}

} // end types2_package
