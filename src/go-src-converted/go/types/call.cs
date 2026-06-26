// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements typechecking of call and selector expressions.
namespace go.go;

using ast = go.ast_package;
using typeparams = go.@internal.typeparams_package;
using token = go.token_package;
using static @internal.types.errors_package;
using strings = strings_package;
using go.@internal;
using ꓸꓸꓸast.Expr = Span<ast.Expr>;

partial class types_package {

// funcInst type-checks a function instantiation.
// The incoming x must be a generic function.
// If ix != nil, it provides some or all of the type arguments (ix.Indices).
// If target != nil, it may be used to infer missing type arguments of x, if any.
// At least one of T or ix must be provided.
//
// There are two modes of operation:
//
//  1. If infer == true, funcInst infers missing type arguments as needed and
//     instantiates the function x. The returned results are nil.
//
//  2. If infer == false and inst provides all type arguments, funcInst
//     instantiates the function x. The returned results are nil.
//     If inst doesn't provide enough type arguments, funcInst returns the
//     available arguments and the corresponding expression list; x remains
//     unchanged.
//
// If an error (other than a version error) occurs in any case, it is reported
// and x.mode is set to invalid.
[GoRecv] public static (slice<ΔType>, slice<ast.Expr>) funcInst(this ref Checker check, ж<target> ᏑT, tokenꓸPos pos, ж<operand> Ꮡx, ж<typeparams.IndexExpr> Ꮡix, bool infer) {
    ref var T = ref ᏑT.val;
    ref var x = ref Ꮡx.val;
    ref var ix = ref Ꮡix.val;

    assert(T != nil || ix != nil);
    positioner instErrPos = default!;
    if (ix != nil){
        instErrPos = inNode(ix.Orig, ix.Lbrack);
        x.expr = ix.Orig;
    } else {
        // if we don't have an index expression, keep the existing expression of x
        instErrPos = ((atPos)pos);
    }
    var versionErr = !check.verifyVersionf(instErrPos, go1_18, "function instantiation"u8);
    // targs and xlist are the type arguments and corresponding type expressions, or nil.
    slice<ΔType> targs = default!;
    slice<ast.Expr> xlist = default!;
    if (ix != nil) {
        xlist = ix.Indices;
        targs = check.typeList(xlist);
        if (targs == default!) {
            x.mode = invalid;
            return (default!, default!);
        }
        assert(len(targs) == len(xlist));
    }
    // Check the number of type arguments (got) vs number of type parameters (want).
    // Note that x is a function value, not a type expression, so we don't need to
    // call under below.
    var sig = x.typ._<ΔSignature.val>();
    nint got = len(targs);
    nint want = sig.TypeParams().Len();
    if (got > want) {
        // Providing too many type arguments is always an error.
        check.errorf(ix.Indices[got - 1], WrongTypeArgCount, "got %d type arguments but want %d"u8, got, want);
        x.mode = invalid;
        return (default!, default!);
    }
    if (got < want) {
        if (!infer) {
            return (targs, xlist);
        }
        // If the uninstantiated or partially instantiated function x is used in
        // an assignment (tsig != nil), infer missing type arguments by treating
        // the assignment
        //
        //    var tvar tsig = x
        //
        // like a call g(tvar) of the synthetic generic function g
        //
        //    func g[type_parameters_of_x](func_type_of_x)
        //
        slice<ж<operand>> args = default!;
        slice<ж<Var>> @params = default!;
        bool reverse = default!;
        if (T != nil && (~sig).tparams != nil) {
            if (!versionErr && !check.allowVersion(instErrPos, go1_21)) {
                if (ix != nil){
                    check.versionErrorf(instErrPos, go1_21, "partially instantiated function in assignment"u8);
                } else {
                    check.versionErrorf(instErrPos, go1_21, "implicitly instantiated function in assignment"u8);
                }
            }
            var gsig = NewSignatureType(nil, default!, default!, (~sig).@params, (~sig).results, (~sig).variadic);
            @params = new ж<Var>[]{NewVar(x.Pos(), check.pkg, ""u8, ~gsig)}.slice();
            // The type of the argument operand is tsig, which is the type of the LHS in an assignment
            // or the result type in a return statement. Create a pseudo-expression for that operand
            // that makes sense when reported in error messages from infer, below.
            var expr = ast.NewIdent(T.desc);
            expr.val.NamePos = x.Pos();
            // correct position
            args = new ж<operand>[]{new(mode: value, expr: expr, typ: T.sig)}.slice();
            reverse = true;
        }
        // Rename type parameters to avoid problems with recursive instantiations.
        // Note that NewTuple(params...) below is (*Tuple)(nil) if len(params) == 0, as desired.
        (tparams, params2) = check.renameTParams(pos, sig.TypeParams().list(), ~NewTuple(Ꮡparams.ꓸꓸꓸ));
        var err = check.newError(CannotInferTypeArgs);
        targs = check.infer(((atPos)pos), tparams, targs, params2._<Tuple.val>(), args, reverse, err);
        if (targs == default!) {
            if (!err.empty()) {
                err.report();
            }
            x.mode = invalid;
            return (default!, default!);
        }
        got = len(targs);
    }
    assert(got == want);
    // instantiate function signature
    sig = check.instantiateSignature(x.Pos(), x.expr, sig, targs, xlist);
    x.typ = sig;
    x.mode = value;
    return (default!, default!);
}

[GoRecv] public static ж<ΔSignature> /*res*/ instantiateSignature(this ref Checker check, tokenꓸPos pos, ast.Expr expr, ж<ΔSignature> Ꮡtyp, slice<ΔType> targs, slice<ast.Expr> xlist) => func((defer, _) => {
    ж<ΔSignature> res = default!;

    ref var typ = ref Ꮡtyp.val;
    assert(check != nil);
    assert(len(targs) == typ.TypeParams().Len());
    if (check.conf._Trace) {
        check.trace(pos, "-- instantiating signature %s with %s"u8, typ, targs);
        check.indent++;
        defer(() => {
            check.indent--;
            check.trace(pos, "=> %s (under = %s)"u8, res, res.Underlying());
        });
    }
    var inst = check.instance(pos, ~typ, targs, nil, check.context())._<ΔSignature.val>();
    assert(inst.TypeParams().Len() == 0);
    // signature is not generic anymore
    check.recordInstance(expr, targs, ~inst);
    assert(len(xlist) <= len(targs));
    // verify instantiation lazily (was go.dev/issue/50450)
    check.later(
    var targsʗ11 = targs;
    var xlistʗ11 = xlist;
    () => {
        var tparams = typ.TypeParams().list();
        {
            var (i, err) = check.verify(pos, tparams, targsʗ11, check.context()); if (err != default!){
                tokenꓸPos posΔ1 = pos;
                if (i < len(xlistʗ11)) {
                    posΔ1 = xlistʗ11[i].Pos();
                }
                check.softErrorf(((atPos)posΔ1), InvalidTypeArg, "%s"u8, err);
            } else {
                check.mono.recordInstance(check.pkg, pos, tparams, targsʗ11, xlistʗ11);
            }
        }
    }).describef(((atPos)pos), "verify instantiation"u8);
    return inst;
});

[GoRecv] public static exprKind callExpr(this ref Checker check, ж<operand> Ꮡx, ж<ast.CallExpr> Ꮡcall) {
    ref var x = ref Ꮡx.val;
    ref var call = ref Ꮡcall.val;

    var ix = typeparams.UnpackIndexExpr(call.Fun);
    if (ix != nil){
        if (check.indexExpr(Ꮡx, ix)){
            // Delay function instantiation to argument checking,
            // where we combine type and value arguments for type
            // inference.
            assert(x.mode == value);
        } else {
            ix = default!;
        }
        x.expr = call.Fun;
        check.record(Ꮡx);
    } else {
        check.exprOrType(Ꮡx, call.Fun, true);
    }
    // x.typ may be generic
    var exprᴛ1 = x.mode;
    if (exprᴛ1 == invalid) {
        check.use(call.Args.ꓸꓸꓸ);
        x.expr = call;
        return statement;
    }
    if (exprᴛ1 == typexpr) {
        check.nonGeneric(nil, // conversion
 Ꮡx);
        if (x.mode == invalid) {
            return Δconversion;
        }
        var T = x.typ;
        x.mode = invalid;
        {
            nint n = len(call.Args);
            switch (n) {
            case 0: {
                check.errorf(inNode(~call, call.Rparen), WrongArgCount, "missing argument in conversion to %s"u8, T);
                break;
            }
            case 1: {
                check.expr(nil, Ꮡx, call.Args[0]);
                if (x.mode != invalid) {
                    if (hasDots(Ꮡcall)) {
                        check.errorf(call.Args[0], BadDotDotDotSyntax, "invalid use of ... in conversion to %s"u8, T);
                        break;
                    }
                    {
                        var (t, _) = under(T)._<Interface.val>(ᐧ); if (t != nil && !isTypeParam(T)) {
                            if (!t.IsMethodSet()) {
                                check.errorf(~call, MisplacedConstraintIface, "cannot use interface %s in conversion (contains specific type constraints or is comparable)"u8, T);
                                break;
                            }
                        }
                    }
                    check.conversion(Ꮡx, T);
                }
                break;
            }
            default: {
                check.use(call.Args.ꓸꓸꓸ);
                check.errorf(call.Args[n - 1], WrongArgCount, "too many arguments in conversion to %s"u8, T);
                break;
            }}
        }

        x.expr = call;
        return Δconversion;
    }
    if (exprᴛ1 == Δbuiltin) {
        builtinId id = x.id;
        if (!check.builtin(Ꮡx, // no need to check for non-genericity here
 Ꮡcall, id)) {
            x.mode = invalid;
        }
        x.expr = call;
        if (x.mode != invalid && x.mode != constant_) {
            // a non-constant result implies a function call
            check.hasCallOrRecv = true;
        }
        return predeclaredFuncs[id].kind;
    }

    // ordinary function/method call
    // signature may be generic
    var cgocall = x.mode == cgofunc;
    // a type parameter may be "called" if all types have the same signature
    var (sig, _) = coreType(x.typ)._<ΔSignature.val>(ᐧ);
    if (sig == nil) {
        check.errorf(~x, InvalidCall, invalidOp + "cannot call non-function %s", x);
        x.mode = invalid;
        x.expr = call;
        return statement;
    }
    // Capture wasGeneric before sig is potentially instantiated below.
    var wasGeneric = sig.TypeParams().Len() > 0;
    // evaluate type arguments, if any
    slice<ast.Expr> xlist = default!;
    slice<ΔType> targs = default!;
    if (ix != nil) {
        xlist = ix.val.Indices;
        targs = check.typeList(xlist);
        if (targs == default!) {
            check.use(call.Args.ꓸꓸꓸ);
            x.mode = invalid;
            x.expr = call;
            return statement;
        }
        assert(len(targs) == len(xlist));
        // check number of type arguments (got) vs number of type parameters (want)
        nint got = len(targs);
        nint want = sig.TypeParams().Len();
        if (got > want) {
            check.errorf(xlist[want], WrongTypeArgCount, "got %d type arguments but want %d"u8, got, want);
            check.use(call.Args.ꓸꓸꓸ);
            x.mode = invalid;
            x.expr = call;
            return statement;
        }
        // If sig is generic and all type arguments are provided, preempt function
        // argument type inference by explicitly instantiating the signature. This
        // ensures that we record accurate type information for sig, even if there
        // is an error checking its arguments (for example, if an incorrect number
        // of arguments is supplied).
        if (got == want && want > 0) {
            check.verifyVersionf(((atPos)(~ix).Lbrack), go1_18, "function instantiation"u8);
            sig = check.instantiateSignature(ix.Pos(), (~ix).Orig, sig, targs, xlist);
            // targs have been consumed; proceed with checking arguments of the
            // non-generic signature.
            targs = default!;
            xlist = default!;
        }
    }
    // evaluate arguments
    (args, atargs, atxlist) = check.genericExprList(call.Args);
    sig = check.arguments(Ꮡcall, sig, targs, xlist, args, atargs, atxlist);
    if (wasGeneric && sig.TypeParams().Len() == 0) {
        // Update the recorded type of call.Fun to its instantiated type.
        check.recordTypeAndValue(call.Fun, value, ~sig, default!);
    }
    // determine result
    switch ((~sig).results.Len()) {
    case 0: {
        x.mode = novalue;
        break;
    }
    case 1: {
        if (cgocall){
            x.mode = commaerr;
        } else {
            x.mode = value;
        }
        x.typ = (~(~sig).results).vars[0].typ;
        break;
    }
    default: {
        x.mode = value;
        x.typ = sig.val.results;
        break;
    }}

    // unpack tuple
    x.expr = call;
    check.hasCallOrRecv = true;
    // if type inference failed, a parameterized result must be invalidated
    // (operands cannot have a parameterized type)
    if (x.mode == value && sig.TypeParams().Len() > 0 && isParameterized(sig.TypeParams().list(), x.typ)) {
        x.mode = invalid;
    }
    return statement;
}

// exprList evaluates a list of expressions and returns the corresponding operands.
// A single-element expression list may evaluate to multiple operands.
[GoRecv] internal static slice<ж<operand>> /*xlist*/ exprList(this ref Checker check, slice<ast.Expr> elist) {
    slice<ж<operand>> xlist = default!;

    {
        nint n = len(elist); if (n == 1){
            (xlist, _) = check.multiExpr(elist[0], false);
        } else 
        if (n > 1) {
            // multiple (possibly invalid) values
            xlist = new slice<ж<operand>>(n);
            foreach (var (i, e) in elist) {
                ref var x = ref heap(new operand(), out var Ꮡx);
                check.expr(nil, Ꮡx, e);
                xlist[i] = Ꮡx;
            }
        }
    }
    return xlist;
}

// genericExprList is like exprList but result operands may be uninstantiated or partially
// instantiated generic functions (where constraint information is insufficient to infer
// the missing type arguments) for Go 1.21 and later.
// For each non-generic or uninstantiated generic operand, the corresponding targsList and
// xlistList elements do not exist (targsList and xlistList are nil) or the elements are nil.
// For each partially instantiated generic function operand, the corresponding targsList and
// xlistList elements are the operand's partial type arguments and type expression lists.
[GoRecv] internal static (slice<ж<operand>> resList, slice<slice<ΔType>> targsList, slice<ast.Expr> xlistList) genericExprList(this ref Checker check, slice<ast.Expr> elist) => func((defer, _) => {
    slice<ж<operand>> resList = default!;
    slice<slice<ΔType>> targsList = default!;
    slice<ast.Expr> xlistList = default!;

    if (debug) {
        var resListʗ1 = resList;
        var targsListʗ1 = targsList;
        var xlistListʗ1 = xlistList;
        defer(() => {
            // targsList and xlistList must have matching lengths
            assert(len(targsListʗ1) == len(xlistListʗ1));
            // type arguments must only exist for partially instantiated functions
            foreach (var (i, xΔ1) in resListʗ1) {
                if (i < len(targsListʗ1)) {
                    {
                        nint nΔ1 = len(targsListʗ1[i]); if (nΔ1 > 0) {
                            // x must be a partially instantiated function
                            assert(nΔ1 < (~xΔ1).typ._<ΔSignature.val>().TypeParams().Len());
                        }
                    }
                }
            }
        });
    }
    // Before Go 1.21, uninstantiated or partially instantiated argument functions are
    // nor permitted. Checker.funcInst must infer missing type arguments in that case.
    var infer = true;
    // for -lang < go1.21
    nint n = len(elist);
    if (n > 0 && check.allowVersion(elist[0], go1_21)) {
        infer = false;
    }
    if (n == 1){
        // single value (possibly a partially instantiated function), or a multi-valued expression
        var e = elist[0];
        ref var xΔ2 = ref heap(new operand(), out var ᏑxΔ2);
        {
            var ix = typeparams.UnpackIndexExpr(e); if (ix != nil && check.indexExpr(ᏑxΔ2, ix)){
                // x is a generic function.
                (targs, xlist) = check.funcInst(nil, xΔ2.Pos(), ᏑxΔ2, ix, infer);
                if (targs != default!){
                    // x was not instantiated: collect the (partial) type arguments.
                    targsList = new slice<ΔType>[]{targs}.slice();
                    xlistList = new ast.Expr[]{xlist}.slice();
                    // Update x.expr so that we can record the partially instantiated function.
                    .expr = ix.val.Orig;
                } else {
                    // x was instantiated: we must record it here because we didn't
                    // use the usual expression evaluators.
                    check.record(ᏑxΔ2);
                }
                resList = new ж<operand>[]{ᏑxΔ2}.slice();
            } else {
                // x is not a function instantiation (it may still be a generic function).
                check.rawExpr(nil, ᏑxΔ2, e, default!, true);
                check.exclude(ᏑxΔ2, (nuint)((UntypedInt)(1 << (int)(novalue) | 1 << (int)(Δbuiltin)) | 1 << (int)(typexpr)));
                {
                    var (t, ok) = x.typ._<Tuple.val>(ᐧ); if (ok && xΔ2.mode != invalid){
                        // x is a function call returning multiple values; it cannot be generic.
                        resList = new slice<ж<operand>>(t.Len());
                        foreach (var (i, v) in (~t).vars) {
                            resList[i] = Ꮡ(new operand(mode: value, expr: e, typ: v.typ));
                        }
                    } else {
                        // x is exactly one value (possibly invalid or uninstantiated generic function).
                        resList = new ж<operand>[]{ᏑxΔ2}.slice();
                    }
                }
            }
        }
    } else 
    if (n > 1) {
        // multiple values
        resList = new slice<ж<operand>>(n);
        targsList = new slice<slice<ΔType>>(n);
        xlistList = new slice<ast.Expr>(n);
        foreach (var (i, e) in elist) {
            ref var x = ref heap(new operand(), out var Ꮡx);
            {
                var ix = typeparams.UnpackIndexExpr(e); if (ix != nil && check.indexExpr(Ꮡx, ix)){
                    // x is a generic function.
                    (targs, xlist) = check.funcInst(nil, x.Pos(), Ꮡx, ix, infer);
                    if (targs != default!){
                        // x was not instantiated: collect the (partial) type arguments.
                        targsList[i] = targs;
                        xlistList[i] = xlist;
                        // Update x.expr so that we can record the partially instantiated function.
                        x.expr = ix.val.Orig;
                    } else {
                        // x was instantiated: we must record it here because we didn't
                        // use the usual expression evaluators.
                        check.record(Ꮡx);
                    }
                } else {
                    // x is exactly one value (possibly invalid or uninstantiated generic function).
                    check.genericExpr(Ꮡx, e);
                }
            }
            resList[i] = Ꮡx;
        }
    }
    return (resList, targsList, xlistList);
});

// arguments type-checks arguments passed to a function call with the given signature.
// The function and its arguments may be generic, and possibly partially instantiated.
// targs and xlist are the function's type arguments (and corresponding expressions).
// args are the function arguments. If an argument args[i] is a partially instantiated
// generic function, atargs[i] and atxlist[i] are the corresponding type arguments
// (and corresponding expressions).
// If the callee is variadic, arguments adjusts its signature to match the provided
// arguments. The type parameters and arguments of the callee and all its arguments
// are used together to infer any missing type arguments, and the callee and argument
// functions are instantiated as necessary.
// The result signature is the (possibly adjusted and instantiated) function signature.
// If an error occurred, the result signature is the incoming sig.
[GoRecv] public static ж<ΔSignature> /*rsig*/ arguments(this ref Checker check, ж<ast.CallExpr> Ꮡcall, ж<ΔSignature> Ꮡsig, slice<ΔType> targs, slice<ast.Expr> xlist, slice<ж<operand>> args, slice<slice<ΔType>> atargs, slice<ast.Expr> atxlist) {
    ж<ΔSignature> rsig = default!;

    ref var call = ref Ꮡcall.val;
    ref var sig = ref Ꮡsig.val;
    rsig = sig;
    // Function call argument/parameter count requirements
    //
    //               | standard call    | dotdotdot call |
    // --------------+------------------+----------------+
    // standard func | nargs == npars   | invalid        |
    // --------------+------------------+----------------+
    // variadic func | nargs >= npars-1 | nargs == npars |
    // --------------+------------------+----------------+
    nint nargs = len(args);
    nint npars = sig.@params.Len();
    var ddd = hasDots(Ꮡcall);
    // set up parameters
    var sigParams = sig.@params;
    // adjusted for variadic functions (may be nil for empty parameter lists!)
    var adjusted = false;
    // indicates if sigParams is different from sig.params
    if (sig.variadic){
        if (ddd){
            // variadic_func(a, b, c...)
            if (len(call.Args) == 1 && nargs > 1) {
                // f()... is not permitted if f() is multi-valued
                check.errorf(inNode(~call, call.Ellipsis), InvalidDotDotDot, "cannot use ... with %d-valued %s"u8, nargs, call.Args[0]);
                return rsig;
            }
        } else {
            // variadic_func(a, b, c)
            if (nargs >= npars - 1){
                // Create custom parameters for arguments: keep
                // the first npars-1 parameters and add one for
                // each argument mapping to the ... parameter.
                var vars = new slice<ж<Var>>(npars - 1);
                // npars > 0 for variadic functions
                copy(vars, sig.@params.vars);
                var last = sig.@params.vars[npars - 1];
                var typ = last.typ._<Slice.val>().elem;
                while (len(vars) < nargs) {
                    vars = append(vars, NewParam(last.pos, last.pkg, last.name, typ));
                }
                sigParams = NewTuple(Ꮡvars.ꓸꓸꓸ);
                // possibly nil!
                adjusted = true;
                npars = nargs;
            } else {
                // nargs < npars-1
                npars--;
            }
        }
    } else {
        // for correct error message below
        if (ddd) {
            // standard_func(a, b, c...)
            check.errorf(inNode(~call, call.Ellipsis), NonVariadicDotDotDot, "cannot use ... in call to non-variadic %s"u8, call.Fun);
            return rsig;
        }
    }
    // standard_func(a, b, c)
    // check argument count
    if (nargs != npars) {
        positioner at = call;
        @string qualifier = "not enough"u8;
        if (nargs > npars){
            at = args[npars].expr;
            // report at first extra argument
            qualifier = "too many"u8;
        } else {
            at = ((atPos)call.Rparen);
        }
        // report at closing )
        // take care of empty parameter lists represented by nil tuples
        slice<ж<Var>> @params = default!;
        if (sig.@params != nil) {
            @params = sig.@params.vars;
        }
        var err = check.newError(WrongArgCount);
        err.addf(at, "%s arguments in call to %s"u8, qualifier, call.Fun);
        err.addf(noposn, "have %s"u8, check.typesSummary(operandTypes(args), false));
        err.addf(noposn, "want %s"u8, check.typesSummary(varTypes(@params), sig.variadic));
        err.report();
        return rsig;
    }
    // collect type parameters of callee and generic function arguments
    slice<ж<TypeParam>> tparams = default!;
    // collect type parameters of callee
    nint n = sig.TypeParams().Len();
    if (n > 0) {
        if (!check.allowVersion(~call, go1_18)) {
            switch (call.Fun.type()) {
            case ж<ast.IndexExpr> : {
                var ix = typeparams.UnpackIndexExpr(call.Fun);
                check.versionErrorf(inNode(call.Fun, (~ix).Lbrack), go1_18, "function instantiation"u8);
                break;
            }
            case ж<ast.IndexListExpr> : {
                var ix = typeparams.UnpackIndexExpr(call.Fun);
                check.versionErrorf(inNode(call.Fun, (~ix).Lbrack), go1_18, "function instantiation"u8);
                break;
            }
            default: {

                check.versionErrorf(inNode(~call, call.Lparen), go1_18, "implicit function instantiation"u8);
                break;
            }}

        }
        // rename type parameters to avoid problems with recursive calls
        ΔType tmp = default!;
        (tparams, tmp) = check.renameTParams(call.Pos(), sig.TypeParams().list(), ~sigParams);
        sigParams = tmp._<Tuple.val>();
        // make sure targs and tparams have the same length
        while (len(targs) < len(tparams)) {
            targs = append(targs, default!);
        }
    }
    assert(len(tparams) == len(targs));
    // collect type parameters from generic function arguments
    slice<nint> genericArgs = default!; // indices of generic function arguments
    if (enableReverseTypeInference) {
        foreach (var (i, arg) in args) {
            // generic arguments cannot have a defined (*Named) type - no need for underlying type below
            {
                var (asig, _) = (~arg).typ._<ΔSignature.val>(ᐧ); if (asig != nil && asig.TypeParams().Len() > 0) {
                    // The argument type is a generic function signature. This type is
                    // pointer-identical with (it's copied from) the type of the generic
                    // function argument and thus the function object.
                    // Before we change the type (type parameter renaming, below), make
                    // a clone of it as otherwise we implicitly modify the object's type
                    // (go.dev/issues/63260).
                    asig = clone(asig);
                    // Rename type parameters for cases like f(g, g); this gives each
                    // generic function argument a unique type identity (go.dev/issues/59956).
                    // TODO(gri) Consider only doing this if a function argument appears
                    //           multiple times, which is rare (possible optimization).
                    (atparams, tmp) = check.renameTParams(call.Pos(), asig.TypeParams().list(), ~asig);
                    asig = tmp._<ΔSignature.val>();
                    asig.val.tparams = Ꮡ(new TypeParamList(atparams));
                    // renameTParams doesn't touch associated type parameters
                    arg.val.typ = asig;
                    // new type identity for the function argument
                    tparams = append(tparams, Ꮡatparams.ꓸꓸꓸ);
                    // add partial list of type arguments, if any
                    if (i < len(atargs)) {
                        targs = append(targs, atargs[i].ꓸꓸꓸ);
                    }
                    // make sure targs and tparams have the same length
                    while (len(targs) < len(tparams)) {
                        targs = append(targs, default!);
                    }
                    genericArgs = append(genericArgs, i);
                }
            }
        }
    }
    assert(len(tparams) == len(targs));
    // at the moment we only support implicit instantiations of argument functions
    _ = len(genericArgs) > 0 && check.verifyVersionf(~args[genericArgs[0]], go1_21, "implicitly instantiated function as argument"u8);
    // tparams holds the type parameters of the callee and generic function arguments, if any:
    // the first n type parameters belong to the callee, followed by mi type parameters for each
    // of the generic function arguments, where mi = args[i].typ.(*Signature).TypeParams().Len().
    // infer missing type arguments of callee and function arguments
    if (len(tparams) > 0) {
        var err = check.newError(CannotInferTypeArgs);
        targs = check.infer(~call, tparams, targs, sigParams, args, false, err);
        if (targs == default!) {
            // TODO(gri) If infer inferred the first targs[:n], consider instantiating
            //           the call signature for better error messages/gopls behavior.
            //           Perhaps instantiate as much as we can, also for arguments.
            //           This will require changes to how infer returns its results.
            if (!err.empty()) {
                check.errorf(err.posn(), CannotInferTypeArgs, "in call to %s, %s"u8, call.Fun, err.msg());
            }
            return rsig;
        }
        // update result signature: instantiate if needed
        if (n > 0) {
            rsig = check.instantiateSignature(call.Pos(), call.Fun, Ꮡsig, targs[..(int)(n)], xlist);
            // If the callee's parameter list was adjusted we need to update (instantiate)
            // it separately. Otherwise we can simply use the result signature's parameter
            // list.
            if (adjusted){
                sigParams = check.subst(call.Pos(), ~sigParams, makeSubstMap(tparams[..(int)(n)], targs[..(int)(n)]), nil, check.context())._<Tuple.val>();
            } else {
                sigParams = rsig.val.@params;
            }
        }
        // compute argument signatures: instantiate if needed
        nint j = n;
        foreach (var (_, i) in genericArgs) {
            var arg = args[i];
            var asig = (~arg).typ._<ΔSignature.val>();
            nint k = j + asig.TypeParams().Len();
            // targs[j:k] are the inferred type arguments for asig
            arg.val.typ = check.instantiateSignature(call.Pos(), (~arg).expr, asig, targs[(int)(j)..(int)(k)], default!);
            // TODO(gri) provide xlist if possible (partial instantiations)
            check.record(arg);
            // record here because we didn't use the usual expr evaluators
            j = k;
        }
    }
    // check arguments
    if (len(args) > 0) {
        @string context = check.sprintf("argument to %s"u8, call.Fun);
        foreach (var (i, a) in args) {
            check.assignment(a, (~sigParams).vars[i].typ, context);
        }
    }
    return rsig;
}

// actually a pointer to the var
// function to evaluate the expanded expression
internal static array<@string> cgoPrefixes = new @string[]{
    "_Ciconst_",
    "_Cfconst_",
    "_Csconst_",
    "_Ctype_",
    "_Cvar_",
    "_Cfpvar_fp_",
    "_Cfunc_",
    "_Cmacro_"
}.array();

[GoRecv] public static void selector(this ref Checker check, ж<operand> Ꮡx, ж<ast.SelectorExpr> Ꮡe, ж<TypeName> Ꮡdef, bool wantType) {
    ref var x = ref Ꮡx.val;
    ref var e = ref Ꮡe.val;
    ref var def = ref Ꮡdef.val;

    // these must be declared before the "goto Error" statements
    Object obj = default!;
    
    slice<nint> index = default!;
    
    bool indirect = default!;
    @string sel = e.Sel.Name;
    // If the identifier refers to a package, handle everything here
    // so we don't need a "package" mode for operands: package names
    // can only appear in qualified identifiers which are mapped to
    // selector expressions.
    {
        var (ident, ok) = e.X._<ж<ast.Ident>>(ᐧ); if (ok) {
            var objΔ1 = check.lookup((~ident).Name);
            {
                var (pname, _) = obj._<PkgName.val>(ᐧ); if (pname != nil) {
                    assert(pname.pkg == check.pkg);
                    check.recordUse(ident, ~pname);
                    pname.val.used = true;
                    var pkg = pname.val.imported;
                    Object exp = default!;
                    var funcMode = value;
                    if ((~pkg).cgo){
                        // cgo special cases C.malloc: it's
                        // rewritten to _CMalloc and does not
                        // support two-result calls.
                        if (sel == "malloc"u8){
                            sel = "_CMalloc"u8;
                        } else {
                            funcMode = cgofunc;
                        }
                        foreach (var (_, prefix) in cgoPrefixes) {
                            // cgo objects are part of the current package (in file
                            // _cgo_gotypes.go). Use regular lookup.
                            (_, exp) = check.scope.LookupParent(prefix + sel, check.pos);
                            if (exp != default!) {
                                break;
                            }
                        }
                        if (exp == default!) {
                            check.errorf(~e.Sel, UndeclaredImportedName, "undefined: %s"u8, new ast.Expr(e));
                            // cast to ast.Expr to silence vet
                            goto ΔError;
                        }
                        check.objDecl(exp, nil);
                    } else {
                        exp = (~pkg).scope.Lookup(sel);
                        if (exp == default!) {
                            if (!(~pkg).fake) {
                                check.errorf(~e.Sel, UndeclaredImportedName, "undefined: %s"u8, new ast.Expr(e));
                            }
                            goto ΔError;
                        }
                        if (!exp.Exported()) {
                            check.errorf(~e.Sel, UnexportedName, "name %s not exported by package %s"u8, sel, (~pkg).name);
                        }
                    }
                    // ok to continue
                    check.recordUse(e.Sel, exp);
                    // Simplified version of the code for *ast.Idents:
                    // - imported objects are always fully initialized
                    switch (exp.type()) {
                    case Const.val exp: {
                        assert(exp.Val() != default!);
                        x.mode = constant_;
                        x.typ = exp.typ;
                        x.val = exp.val.val;
                        break;
                    }
                    case TypeName.val exp: {
                        x.mode = typexpr;
                        x.typ = exp.typ;
                        break;
                    }
                    case Var.val exp: {
                        x.mode = variable;
                        x.typ = exp.typ;
                        if ((~pkg).cgo && strings.HasPrefix(exp.name, "_Cvar_"u8)) {
                            x.typ = x.typ._<Pointer.val>().@base;
                        }
                        break;
                    }
                    case Func.val exp: {
                        x.mode = funcMode;
                        x.typ = exp.typ;
                        if ((~pkg).cgo && strings.HasPrefix(exp.name, "_Cmacro_"u8)) {
                            x.mode = value;
                            x.typ = x.typ._<ΔSignature.val>().results.vars[0].typ;
                        }
                        break;
                    }
                    case Builtin.val exp: {
                        x.mode = Δbuiltin;
                        x.typ = exp.typ;
                        x.id = exp.val.id;
                        break;
                    }
                    default: {
                        var exp = exp.type();
                        check.dump("%v: unexpected object %v"u8, e.Sel.Pos(), exp);
                        throw panic("unreachable");
                        break;
                    }}
                    x.expr = e;
                    return;
                }
            }
        }
    }
    check.exprOrType(Ꮡx, e.X, false);
    var exprᴛ1 = x.mode;
    if (exprᴛ1 == typexpr) {
        if (def != nil && AreEqual(def.typ, x.typ)) {
            // don't crash for "type T T.x" (was go.dev/issue/51509)
            check.cycleError(new Object[]{~def}.slice(), 0);
            goto ΔError;
        }
    }
    else if (exprᴛ1 == Δbuiltin) {
        check.errorf(~e.Sel, // types2 uses the position of '.' for the error
 UncalledBuiltin, "cannot select on %s"u8, x);
        goto ΔError;
    }
    else if (exprᴛ1 == invalid) {
        goto ΔError;
    }

    // Avoid crashing when checking an invalid selector in a method declaration
    // (i.e., where def is not set):
    //
    //   type S[T any] struct{}
    //   type V = S[any]
    //   func (fs *S[T]) M(x V.M) {}
    //
    // All codepaths below return a non-type expression. If we get here while
    // expecting a type expression, it is an error.
    //
    // See go.dev/issue/57522 for more details.
    //
    // TODO(rfindley): We should do better by refusing to check selectors in all cases where
    // x.typ is incomplete.
    if (wantType) {
        check.errorf(~e.Sel, NotAType, "%s is not a type"u8, new ast.Expr(e));
        goto ΔError;
    }
    (obj, index, indirect) = lookupFieldOrMethod(x.typ, x.mode == variable, check.pkg, sel, false);
    if (obj == default!) {
        // Don't report another error if the underlying type was invalid (go.dev/issue/49541).
        if (!isValid(under(x.typ))) {
            goto ΔError;
        }
        if (index != default!) {
            // TODO(gri) should provide actual type where the conflict happens
            check.errorf(~e.Sel, AmbiguousSelector, "ambiguous selector %s.%s"u8, x.expr, sel);
            goto ΔError;
        }
        if (indirect) {
            if (x.mode == typexpr){
                check.errorf(~e.Sel, InvalidMethodExpr, "invalid method expression %s.%s (needs pointer receiver (*%s).%s)"u8, x.typ, sel, x.typ, sel);
            } else {
                check.errorf(~e.Sel, InvalidMethodExpr, "cannot call pointer method %s on %s"u8, sel, x.typ);
            }
            goto ΔError;
        }
        @string why = default!;
        if (isInterfacePtr(x.typ)){
            why = check.interfacePtrError(x.typ);
        } else {
            var (alt, _, _) = lookupFieldOrMethod(x.typ, x.mode == variable, check.pkg, sel, true);
            why = check.lookupError(x.typ, sel, alt, false);
        }
        check.errorf(~e.Sel, MissingFieldOrMethod, "%s.%s undefined (%s)"u8, x.expr, sel, why);
        goto ΔError;
    }
    // methods may not have a fully set up signature yet
    {
        var (m, _) = obj._<Func.val>(ᐧ); if (m != nil) {
            check.objDecl(~m, nil);
        }
    }
    if (x.mode == typexpr){
        // method expression
        var (m, _) = obj._<Func.val>(ᐧ);
        if (m == nil) {
            check.errorf(~e.Sel, MissingFieldOrMethod, "%s.%s undefined (type %s has no method %s)"u8, x.expr, sel, x.typ, sel);
            goto ΔError;
        }
        check.recordSelection(Ꮡe, MethodExpr, x.typ, ~m, index, indirect);
        var sig = m.typ._<ΔSignature.val>();
        if ((~sig).recv == nil) {
            check.error(~e, InvalidDeclCycle, "illegal cycle in method declaration"u8);
            goto ΔError;
        }
        // the receiver type becomes the type of the first function
        // argument of the method expression's function type
        slice<ж<Var>> @params = default!;
        if ((~sig).@params != nil) {
            @params = (~sig).@params.val.vars;
        }
        // Be consistent about named/unnamed parameters. This is not needed
        // for type-checking, but the newly constructed signature may appear
        // in an error message and then have mixed named/unnamed parameters.
        // (An alternative would be to not print parameter names in errors,
        // but it's useful to see them; this is cheap and method expressions
        // are rare.)
        @string name = ""u8;
        if (len(@params) > 0 && @params[0].name != ""u8) {
            // name needed
            name = (~sig).recv.name;
            if (name == ""u8) {
                name = "_"u8;
            }
        }
        @params = append(new ж<Var>[]{NewVar((~sig).recv.pos, (~sig).recv.pkg, name, x.typ)}.slice(), Ꮡparams.ꓸꓸꓸ);
        x.mode = value;
        x.typ = Ꮡ(new ΔSignature(
            tparams: (~sig).tparams,
            @params: NewTuple(Ꮡparams.ꓸꓸꓸ),
            results: (~sig).results,
            variadic: (~sig).variadic
        ));
        check.addDeclDep(~m);
    } else {
        // regular selector
        switch (obj.type()) {
        case Var.val obj: {
            check.recordSelection(Ꮡe, FieldVal, x.typ, ~obj, index, indirect);
            if (x.mode == variable || indirect){
                x.mode = variable;
            } else {
                x.mode = value;
            }
            x.typ = obj.typ;
            break;
        }
        case Func.val obj: {
            check.recordSelection(Ꮡe, // TODO(gri) If we needed to take into account the receiver's
 // addressability, should we report the type &(x.typ) instead?
 MethodVal, x.typ, ~obj, index, indirect);
            var disabled = true;
            if (!disabled && debug) {
                // TODO(gri) The verification pass below is disabled for now because
                //           method sets don't match method lookup in some cases.
                //           For instance, if we made a copy above when creating a
                //           custom method for a parameterized received type, the
                //           method set method doesn't match (no copy there). There
                ///          may be other situations.
                // Verify that LookupFieldOrMethod and MethodSet.Lookup agree.
                // TODO(gri) This only works because we call LookupFieldOrMethod
                // _before_ calling NewMethodSet: LookupFieldOrMethod completes
                // any incomplete interfaces so they are available to NewMethodSet
                // (which assumes that interfaces have been completed already).
                var typ = x.typ;
                if (x.mode == variable) {
                    // If typ is not an (unnamed) pointer or an interface,
                    // use *typ instead, because the method set of *typ
                    // includes the methods of typ.
                    // Variables are addressable, so we can always take their
                    // address.
                    {
                        var (_, ok) = typ._<Pointer.val>(ᐧ); if (!ok && !IsInterface(typ)) {
                            Ꮡtyp = new Pointer(@base: typ); typ = ref Ꮡtyp.val;
                        }
                    }
                }
                // If we created a synthetic pointer type above, we will throw
                // away the method set computed here after use.
                // TODO(gri) Method set computation should probably always compute
                // both, the value and the pointer receiver method set and represent
                // them in a single structure.
                // TODO(gri) Consider also using a method set cache for the lifetime
                // of checker once we rely on MethodSet lookup instead of individual
                // lookup.
                var mset = NewMethodSet(typ);
                {
                    var m = mset.Lookup(check.pkg, sel); if (m == nil || (~m).obj != ~obj) {
                        check.dump("%v: (%s).%v -> %s"u8, e.Pos(), typ, obj.name, m);
                        check.dump("%s\n"u8, mset);
                        // Caution: MethodSets are supposed to be used externally
                        // only (after all interface types were completed). It's
                        // now possible that we get here incorrectly. Not urgent
                        // to fix since we only run this code in debug mode.
                        // TODO(gri) fix this eventually.
                        throw panic("method sets and lookup don't agree");
                    }
                }
            }
            x.mode = value;
            ref var sig = ref heap<ΔSignature>(out var Ꮡsig);
            sig = obj.typ._<ΔSignature.val>().val;
            sig.recv = default!;
            x.typ = Ꮡsig;
            check.addDeclDep(~obj);
            break;
        }
        default: {
            var obj = obj.type();
            throw panic("unreachable");
            break;
        }}
    }
    // remove receiver
    // everything went well
    x.expr = e;
    return;
ΔError:
    x.mode = invalid;
    x.expr = e;
}

// use type-checks each argument.
// Useful to make sure expressions are evaluated
// (and variables are "used") in the presence of
// other errors. Arguments may be nil.
// Reports if all arguments evaluated without error.
[GoRecv] internal static bool use(this ref Checker check, params ꓸꓸꓸast.Expr argsʗp) {
    var args = argsʗp.slice();

    return check.useN(args, false);
}

// useLHS is like use, but doesn't "use" top-level identifiers.
// It should be called instead of use if the arguments are
// expressions on the lhs of an assignment.
[GoRecv] internal static bool useLHS(this ref Checker check, params ꓸꓸꓸast.Expr argsʗp) {
    var args = argsʗp.slice();

    return check.useN(args, true);
}

[GoRecv] internal static bool useN(this ref Checker check, slice<ast.Expr> args, bool lhs) {
    var ok = true;
    foreach (var (_, e) in args) {
        if (!check.use1(e, lhs)) {
            ok = false;
        }
    }
    return ok;
}

[GoRecv] internal static bool use1(this ref Checker check, ast.Expr e, bool lhs) {
    ref var x = ref heap(new operand(), out var Ꮡx);
    x.mode = value;
    // anything but invalid
    switch (ast.Unparen(e).type()) {
    case default! n: {
        break;
    }
    case ж<ast.Ident> n: {
        if ((~n).Name == "_"u8) {
            // nothing to do
            // don't report an error evaluating blank
            break;
        }
        // If the lhs is an identifier denoting a variable v, this assignment
        // is not a 'use' of v. Remember current value of v.used and restore
        // after evaluating the lhs via check.rawExpr.
        ж<Var> v = default!;
        bool v_used = default!;
        if (lhs) {
            {
                (_, obj) = check.scope.LookupParent((~n).Name, nopos); if (obj != default!) {
                    // It's ok to mark non-local variables, but ignore variables
                    // from other packages to avoid potential race conditions with
                    // dot-imported variables.
                    {
                        var (w, _) = obj._<Var.val>(ᐧ); if (w != nil && w.pkg == check.pkg) {
                            v = w;
                            v_used = v.val.used;
                        }
                    }
                }
            }
        }
        check.exprOrType(Ꮡx, ~n, true);
        if (v != nil) {
            v.val.used = v_used;
        }
        break;
    }
    default: {
        var n = ast.Unparen(e).type();
        check.rawExpr(nil, // restore v.used
 Ꮡx, e, default!, true);
        break;
    }}
    return x.mode != invalid;
}

} // end types_package
