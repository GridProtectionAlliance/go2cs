// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements typechecking of statements.
global using Expr = go.go.ast_package.Expr;
global using identType = go.go.ast_package.Ident;

namespace go.go;

using ast = go.ast_package;
using constant = go.constant_package;
using token = go.token_package;
using buildcfg = @internal.buildcfg_package;
using static @internal.types.errors_package;
using sort = sort_package;
using @internal;

partial class types_package {

[GoRecv] public static void funcBody(this ref Checker check, ж<declInfo> Ꮡdecl, @string name, ж<ΔSignature> Ꮡsig, ж<ast.BlockStmt> Ꮡbody, constant.Value iota) => func((defer, _) => {
    ref var decl = ref Ꮡdecl.val;
    ref var sig = ref Ꮡsig.val;
    ref var body = ref Ꮡbody.val;

    if (check.conf.IgnoreFuncBodies) {
        throw panic("function body not ignored");
    }
    if (check.conf._Trace) {
        check.trace(body.Pos(), "-- %s: %s"u8, name, sig);
    }
    // save/restore current environment and set up function environment
    // (and use 0 indentation at function start)
    deferǃ((environment env, nint indent) => {
        check.environment = env;
        check.indent = indent;
    }, check.environment, check.indent, defer);
    check.environment = new environment(
        decl: decl,
        scope: sig.scope,
        iota: iota,
        sig: sig
    );
    check.indent = 0;
    check.stmtList(0, body.List);
    if (check.hasLabel) {
        check.labels(Ꮡbody);
    }
    if (sig.results.Len() > 0 && !check.isTerminating(~body, ""u8)) {
        check.error(((atPos)body.Rbrace), MissingReturn, "missing return"u8);
    }
    // spec: "Implementation restriction: A compiler may make it illegal to
    // declare a variable inside a function body if the variable is never used."
    check.usage(sig.scope);
});

[GoRecv] public static void usage(this ref Checker check, ж<ΔScope> Ꮡscope) {
    ref var scope = ref Ꮡscope.val;

    slice<ж<Var>> unused = default!;
    foreach (var (name, elem) in scope.elems) {
        elem = resolve(name, elem);
        {
            var (v, _) = elem._<Var.val>(ᐧ); if (v != nil && !(~v).used) {
                unused = append(unused, v);
            }
        }
    }
    sort.Slice(unused, 
    var unusedʗ1 = unused;
    (nint i, nint j) => cmpPos(unusedʗ1[i].pos, unusedʗ1[j].pos) < 0);
    foreach (var (_, v) in unused) {
        check.softErrorf(~v, UnusedVar, "declared and not used: %s"u8, v.name);
    }
    foreach (var (_, scopeΔ1) in scope.children) {
        // Don't go inside function literal scopes a second time;
        // they are handled explicitly by funcBody.
        if (!(~scopeΔ1).isFunc) {
            check.usage(ᏑscopeΔ1);
        }
    }
}

[GoType("num:nuint")] partial struct stmtContext;

internal static readonly stmtContext breakOk = /* 1 << iota */ 1;
internal static readonly stmtContext continueOk = 2;
internal static readonly stmtContext fallthroughOk = 4;
internal static readonly stmtContext finalSwitchCase = 8;
internal static readonly stmtContext inTypeSwitch = 16;

[GoRecv] internal static void simpleStmt(this ref Checker check, ast.Stmt s) {
    if (s != default!) {
        check.stmt(0, s);
    }
}

internal static slice<ast.Stmt> trimTrailingEmptyStmts(slice<ast.Stmt> list) {
    for (nint i = len(list); i > 0; i--) {
        {
            var (_, ok) = list[i - 1]._<ж<ast.EmptyStmt>>(ᐧ); if (!ok) {
                return list[..(int)(i)];
            }
        }
    }
    return default!;
}

[GoRecv] internal static void stmtList(this ref Checker check, stmtContext ctxt, slice<ast.Stmt> list) {
    var ok = (stmtContext)(ctxt & fallthroughOk) != 0;
    stmtContext inner = (stmtContext)(ctxt & ~fallthroughOk);
    list = trimTrailingEmptyStmts(list);
    // trailing empty statements are "invisible" to fallthrough analysis
    foreach (var (i, s) in list) {
        stmtContext innerΔ1 = inner;
        if (ok && i + 1 == len(list)) {
            inner |= (stmtContext)(fallthroughOk);
        }
        check.stmt(innerΔ1, s);
    }
}

[GoRecv] internal static void multipleDefaults(this ref Checker check, slice<ast.Stmt> list) {
    ast.Stmt first = default!;
    foreach (var (_, s) in list) {
        ast.Stmt d = default!;
        switch (s.type()) {
        case ж<ast.CaseClause> c: {
            if (len((~c).List) == 0) {
                d = s;
            }
            break;
        }
        case ж<ast.CommClause> c: {
            if ((~c).Comm == default!) {
                d = s;
            }
            break;
        }
        default: {
            var c = s.type();
            check.error(s, InvalidSyntaxTree, "case/communication clause expected"u8);
            break;
        }}
        if (d != default!) {
            if (first != default!){
                check.errorf(d, DuplicateDefault, "multiple defaults (first at %s)"u8, check.fset.Position(first.Pos()));
            } else {
                first = d;
            }
        }
    }
}

[GoRecv] internal static void openScope(this ref Checker check, ast.Node node, @string comment) {
    var scope = NewScope(check.scope, node.Pos(), node.End(), comment);
    check.recordScope(node, scope);
    check.scope = scope;
}

[GoRecv] internal static void closeScope(this ref Checker check) {
    check.scope = check.scope.Parent();
}

internal static token.Token assignOp(token.Token op) {
    // token_test.go verifies the token ordering this function relies on
    if (token.ADD_ASSIGN <= op && op <= token.AND_NOT_ASSIGN) {
        return op + (token.ADD - token.ADD_ASSIGN);
    }
    return token.ILLEGAL;
}

[GoRecv] public static void suspendedCall(this ref Checker check, @string keyword, ж<ast.CallExpr> Ꮡcall) {
    ref var call = ref Ꮡcall.val;

    ref var x = ref heap(new operand(), out var Ꮡx);
    @string msg = default!;
    errors.Code code = default!;
    var exprᴛ1 = check.rawExpr(nil, Ꮡx, ~call, default!, false);
    if (exprᴛ1 == Δconversion) {
        msg = "requires function call, not conversion"u8;
        code = InvalidDefer;
        if (keyword == "go"u8) {
            code = InvalidGo;
        }
    }
    else if (exprᴛ1 == expression) {
        msg = "discards result of"u8;
        code = UnusedResults;
    }
    else if (exprᴛ1 == statement) {
        return;
    }
    { /* default: */
        throw panic("unreachable");
    }

    check.errorf(~Ꮡx, code, "%s %s %s"u8, keyword, msg, Ꮡx);
}

// goVal returns the Go value for val, or nil.
internal static any goVal(constant.Value val) {
    // val should exist, but be conservative and check
    if (val == default!) {
        return default!;
    }
    // Match implementation restriction of other compilers.
    // gc only checks duplicates for integer, floating-point
    // and string values, so only create Go values for these
    // types.
    var exprᴛ1 = val.Kind();
    if (exprᴛ1 == constant.Int) {
        {
            var (x, ok) = constant.Int64Val(val); if (ok) {
                return x;
            }
        }
        {
            var (x, ok) = constant.Uint64Val(val); if (ok) {
                return x;
            }
        }
    }
    if (exprᴛ1 == constant.Float) {
        {
            var (x, ok) = constant.Float64Val(val); if (ok) {
                return x;
            }
        }
    }
    if (exprᴛ1 == constant.ΔString) {
        return constant.StringVal(val);
    }

    return default!;
}
/* visitMapType: map[any][]valueType */

// A valueMap maps a case value (of a basic Go type) to a list of positions
// where the same case value appeared, together with the corresponding case
// types.
// Since two case values may have the same "underlying" value but different
// types we need to also check the value's types (e.g., byte(1) vs myByte(1))
// when the switch expression is of interface type.
[GoType] partial struct valueType {
    internal go.token_package.ΔPos pos;
    internal ΔType typ;
}

[GoRecv] public static void caseValues(this ref Checker check, ж<operand> Ꮡx, slice<ast.Expr> values, valueMap seen) {
    ref var x = ref Ꮡx.val;

L:
    foreach (var (_, e) in values) {
        ref var v = ref heap(new operand(), out var Ꮡv);
        check.expr(nil, Ꮡv, e);
        if (x.mode == invalid || v.mode == invalid) {
            goto continue_L;
        }
        check.convertUntyped(Ꮡv, x.typ);
        if (v.mode == invalid) {
            goto continue_L;
        }
        // Order matters: By comparing v against x, error positions are at the case values.
        ref var res = ref heap<operand>(out var Ꮡres);
        res = v;
        // keep original v unchanged
        check.comparison(Ꮡres, Ꮡx, token.EQL, true);
        if (res.mode == invalid) {
            goto continue_L;
        }
        if (v.mode != constant_) {
            goto continue_L;
        }
        // we're done
        // look for duplicate values
        {
            var val = goVal(v.val); if (val != default!) {
                // look for duplicate types for a given value
                // (quadratic algorithm, but these lists tend to be very short)
                foreach (var (_, vt) in seen[val]) {
                    if (Identical(v.typ, vt.typ)) {
                        var err = check.newError(DuplicateCase);
                        err.addf(~Ꮡv, "duplicate case %s in expression switch"u8, Ꮡv);
                        err.addf(((atPos)vt.pos), "previous case"u8);
                        err.report();
                        goto continue_L;
                    }
                }
                seen[val] = append(seen[val], new valueType(v.Pos(), v.typ));
            }
        }
    }
}

// isNil reports whether the expression e denotes the predeclared value nil.
[GoRecv] internal static bool isNil(this ref Checker check, ast.Expr e) {
    // The only way to express the nil value is by literally writing nil (possibly in parentheses).
    {
        var (name, _) = ast.Unparen(e)._<ж<ast.Ident>>(ᐧ); if (name != nil) {
            var (_, ok) = check.lookup((~name).Name)._<Nil.val>(ᐧ);
            return ok;
        }
    }
    return false;
}

// caseTypes typechecks the type expressions of a type case, checks for duplicate types
// using the seen map, and verifies that each type is valid with respect to the type of
// the operand x in the type switch clause. If the type switch expression is invalid, x
// must be nil. The result is the type of the last type expression; it is nil if the
// expression denotes the predeclared nil.
[GoRecv] public static ΔType /*T*/ caseTypes(this ref Checker check, ж<operand> Ꮡx, slice<ast.Expr> types, ast.Expr seen) {
    ΔType T = default!;

    ref var x = ref Ꮡx.val;
    ref var dummy = ref heap(new operand(), out var Ꮡdummy);
L:
    foreach (var (_, e) in types) {
        // The spec allows the value nil instead of a type.
        if (check.isNil(e)){
            T = default!;
            check.expr(nil, Ꮡdummy, e);
        } else {
            // run e through expr so we get the usual Info recordings
            T = check.varType(e);
            if (!isValid(T)) {
                goto continue_L;
            }
        }
        // look for duplicate types
        // (quadratic algorithm, but type switches tend to be reasonably small)
        foreach (var (t, other) in seen) {
            if (T == default! && t == default! || T != default! && t != default! && Identical(T, t)) {
                // talk about "case" rather than "type" because of nil case
                @string Ts = "nil"u8;
                if (T != default!) {
                    Ts = TypeString(T, check.qualifier);
                }
                var err = check.newError(DuplicateCase);
                err.addf(e, "duplicate case %s in type switch"u8, Ts);
                err.addf(other, "previous case"u8);
                err.report();
                goto continue_L;
            }
        }
        seen[T] = e;
        if (x != nil && T != default!) {
            check.typeAssertion(e, Ꮡx, T, true);
        }
    }
    return T;
}

// TODO(gri) Once we are certain that typeHash is correct in all situations, use this version of caseTypes instead.
// (Currently it may be possible that different types have identical names and import paths due to ImporterFrom.)
//
// func (check *Checker) caseTypes(x *operand, xtyp *Interface, types []ast.Expr, seen map[string]ast.Expr) (T Type) {
// 	var dummy operand
// L:
// 	for _, e := range types {
// 		// The spec allows the value nil instead of a type.
// 		var hash string
// 		if check.isNil(e) {
// 			check.expr(nil, &dummy, e) // run e through expr so we get the usual Info recordings
// 			T = nil
// 			hash = "<nil>" // avoid collision with a type named nil
// 		} else {
// 			T = check.varType(e)
// 			if !isValid(T) {
// 				continue L
// 			}
// 			hash = typeHash(T, nil)
// 		}
// 		// look for duplicate types
// 		if other := seen[hash]; other != nil {
// 			// talk about "case" rather than "type" because of nil case
// 			Ts := "nil"
// 			if T != nil {
// 				Ts = TypeString(T, check.qualifier)
// 			}
// 			err := check.newError(_DuplicateCase)
// 			err.addf(e, "duplicate case %s in type switch", Ts)
// 			err.addf(other, "previous case")
// 			err.report()
// 			continue L
// 		}
// 		seen[hash] = e
// 		if T != nil {
// 			check.typeAssertion(e.Pos(), x, xtyp, T)
// 		}
// 	}
// 	return
// }

// stmt typechecks statement s.
[GoRecv] internal static void stmt(this ref Checker check, stmtContext ctxt, ast.Stmt s) => func((defer, recover) => {
    // statements must end with the same top scope as they started with
    if (debug) {
        deferǃ((ж<ΔScope> scope) => {
            {
                var p = recover(); if (p != default!) {
                    throw panic(p);
                }
            }
            assert(scope == check.scope);
        }, check.scope, defer);
    }
    // process collected function literals before scope changes
    deferǃ(check.processDelayed, len(check.delayed), defer);
    // reset context for statements of inner blocks
    stmtContext inner = (stmtContext)(ctxt & ~((stmtContext)((stmtContext)(fallthroughOk | finalSwitchCase) | inTypeSwitch)));
    switch (s.type()) {
    case ж<ast.BadStmt> s: {
        break;
    }
    case ж<ast.EmptyStmt> s: {
        break;
    }
    case ж<ast.DeclStmt> s: {
        check.declStmt((~s).Decl);
        break;
    }
    case ж<ast.LabeledStmt> s: {
        check.hasLabel = true;
        check.stmt(ctxt, // ignore
 (~s).Stmt);
        break;
    }
    case ж<ast.ExprStmt> s: {
        // spec: "With the exception of specific built-in functions,
        // function and method calls and receive operations can appear
        // in statement context. Such statements may be parenthesized."
        ref var xΔ1 = ref heap(new operand(), out var ᏑxΔ1);
        exprKind kind = check.rawExpr(nil, ᏑxΔ1, (~s).X, default!, false);
        @string msgΔ1 = default!;
        errors.Code code = default!;
        var exprᴛ1 = xΔ1.mode;
        { /* default: */
            if (kind == statement) {
                return;
            }
             = "is not used"u8;
            code = UnusedExpr;
        }
        else if (exprᴛ1 == Δbuiltin) {
             = "must be called"u8;
            code = UncalledBuiltin;
        }
        else if (exprᴛ1 == typexpr) {
             = "is not an expression"u8;
            code = NotAnExpr;
        }

        check.errorf(~ᏑxΔ1, code, "%s %s"u8, ᏑxΔ1, msgΔ1);
        break;
    }
    case ж<ast.SendStmt> s: {
        ref var ch = ref heap(new operand(), out var Ꮡch);
        ref var val = ref heap(new operand(), out var Ꮡval);
        check.expr(nil, Ꮡch, (~s).Chan);
        check.expr(nil, Ꮡval, (~s).Value);
        if (ch.mode == invalid || val.mode == invalid) {
            return;
        }
        var u = coreType(ch.typ);
        if (u == default!) {
            check.errorf(inNode(~s, (~s).Arrow), InvalidSend, invalidOp + "cannot send to %s: no core type", Ꮡch);
            return;
        }
        var (uch, _) = u._<Chan.val>(ᐧ);
        if (uch == nil) {
            check.errorf(inNode(~s, (~s).Arrow), InvalidSend, invalidOp + "cannot send to non-channel %s", Ꮡch);
            return;
        }
        if ((~uch).dir == RecvOnly) {
            check.errorf(inNode(~s, (~s).Arrow), InvalidSend, invalidOp + "cannot send to receive-only channel %s", Ꮡch);
            return;
        }
        check.assignment(Ꮡval, (~uch).elem, "send"u8);
        break;
    }
    case ж<ast.IncDecStmt> s: {
        token.Token op = default!;
        var exprᴛ2 = (~s).Tok;
        if (exprᴛ2 == token.INC) {
            op = token.ADD;
        }
        else if (exprᴛ2 == token.DEC) {
            op = token.SUB;
        }
        else { /* default: */
            check.errorf(inNode(~s, (~s).TokPos), InvalidSyntaxTree, "unknown inc/dec operation %s"u8, (~s).Tok);
            return;
        }

        ref var xΔ2 = ref heap(new operand(), out var ᏑxΔ2);
        check.expr(nil, ᏑxΔ2, (~s).X);
        if (xΔ2.mode == invalid) {
            return;
        }
        if (!allNumeric(xΔ2.typ)) {
            check.errorf((~s).X, NonNumericIncDec, invalidOp + "%s%s (non-numeric type %s)", (~s).X, (~s).Tok, xΔ2.typ);
            return;
        }
        var Y = Ꮡ(new ast.BasicLit(ValuePos: (~s).X.Pos(), Kind: token.INT, Value: "1"u8));
        check.binary(ᏑxΔ2, // use x's position
 default!, (~s).X, ~Y, op, (~s).TokPos);
        if (xΔ2.mode == invalid) {
            return;
        }
        check.assignVar((~s).X, default!, ᏑxΔ2, "assignment"u8);
        break;
    }
    case ж<ast.AssignStmt> s: {
        var exprᴛ3 = (~s).Tok;
        if (exprᴛ3 == token.ASSIGN || exprᴛ3 == token.DEFINE) {
            if (len((~s).Lhs) == 0) {
                check.error(~s, InvalidSyntaxTree, "missing lhs in assignment"u8);
                return;
            }
            if ((~s).Tok == token.DEFINE){
                check.shortVarDecl(inNode(~s, (~s).TokPos), (~s).Lhs, (~s).Rhs);
            } else {
                // regular assignment
                check.assignVars((~s).Lhs, (~s).Rhs);
            }
        }
        else { /* default: */
            if (len((~s).Lhs) != 1 || len((~s).Rhs) != 1) {
                // assignment operations
                check.errorf(inNode(~s, (~s).TokPos), MultiValAssignOp, "assignment operation %s requires single-valued expressions"u8, (~s).Tok);
                return;
            }
            token.Token op = assignOp((~s).Tok);
            if (op == token.ILLEGAL) {
                check.errorf(((atPos)(~s).TokPos), InvalidSyntaxTree, "unknown assignment operation %s"u8, (~s).Tok);
                return;
            }
            ref var xΔ4 = ref heap(new operand(), out var ᏑxΔ4);
            check.binary(ᏑxΔ4, default!, (~s).Lhs[0], (~s).Rhs[0], op, (~s).TokPos);
            if (xΔ4.mode == invalid) {
                return;
            }
            check.assignVar((~s).Lhs[0], default!, ᏑxΔ4, "assignment"u8);
        }

        break;
    }
    case ж<ast.GoStmt> s: {
        check.suspendedCall("go"u8, (~s).Call);
        break;
    }
    case ж<ast.DeferStmt> s: {
        check.suspendedCall("defer"u8, (~s).Call);
        break;
    }
    case ж<ast.ReturnStmt> s: {
        var res = check.sig.results;
        if (len((~s).Results) == 0 && res.Len() > 0 && (~res).vars[0].name != ""u8){
            // Return with implicit results allowed for function with named results.
            // (If one is named, all are named.)
            // spec: "Implementation restriction: A compiler may disallow an empty expression
            // list in a "return" statement if a different entity (constant, type, or variable)
            // with the same name as a result parameter is in scope at the place of the return."
            foreach (var (_, obj) in (~res).vars) {
                {
                    var alt = check.lookup(obj.name); if (alt != default! && Ꮡalt != ~obj) {
                        var err = check.newError(OutOfScopeResult);
                        err.addf(~s, "result parameter %s not in scope at return"u8, obj.name);
                        err.addf(alt, "inner declaration of %s"u8, obj);
                        err.report();
                    }
                }
            }
        } else {
            // ok to continue
            slice<ж<Var>> lhsΔ1 = default!;
            if (res.Len() > 0) {
                 = res.val.vars;
            }
            check.initVars(lhsΔ1, (~s).Results, ~s);
        }
        break;
    }
    case ж<ast.BranchStmt> s: {
        if ((~s).Label != nil) {
            check.hasLabel = true;
            return;
        }
        var exprᴛ4 = (~s).Tok;
        if (exprᴛ4 == token.BREAK) {
            if ((stmtContext)(ctxt & breakOk) == 0) {
                // checked in 2nd pass (check.labels)
                check.error(~s, MisplacedBreak, "break not in for, switch, or select statement"u8);
            }
        }
        else if (exprᴛ4 == token.CONTINUE) {
            if ((stmtContext)(ctxt & continueOk) == 0) {
                check.error(~s, MisplacedContinue, "continue not in for statement"u8);
            }
        }
        else if (exprᴛ4 == token.FALLTHROUGH) {
            if ((stmtContext)(ctxt & fallthroughOk) == 0) {
                @string msgΔ3 = default!;
                switch (ᐧ) {
                case {} when (stmtContext)(ctxt & finalSwitchCase) != 0: {
                    msgΔ3 = "cannot fallthrough final case in switch"u8;
                    break;
                }
                case {} when (stmtContext)(ctxt & inTypeSwitch) != 0: {
                    msgΔ3 = "cannot fallthrough in type switch"u8;
                    break;
                }
                default: {
                    msgΔ3 = "fallthrough statement out of place"u8;
                    break;
                }}

                check.error(~s, MisplacedFallthrough, msgΔ3);
            }
        }
        else { /* default: */
            check.errorf(~s, InvalidSyntaxTree, "branch statement: %s"u8, (~s).Tok);
        }

        break;
    }
    case ж<ast.BlockStmt> s: {
        check.openScope(~s, "block"u8);
        defer(check.closeScope);
        check.stmtList(inner, (~s).List);
        break;
    }
    case ж<ast.IfStmt> s: {
        check.openScope(~s, "if"u8);
        defer(check.closeScope);
        check.simpleStmt((~s).Init);
        ref var xΔ5 = ref heap(new operand(), out var ᏑxΔ5);
        check.expr(nil, ᏑxΔ5, (~s).Cond);
        if (xΔ5.mode != invalid && !allBoolean(xΔ5.typ)) {
            check.error((~s).Cond, InvalidCond, "non-boolean condition in if statement"u8);
        }
        check.stmt(inner, ~(~s).Body);
        switch ((~s).Else.type()) {
        case default! : {
            break;
        }
        case ж<ast.BadStmt> : {
            break;
        }
        case ж<ast.IfStmt> : {
            check.stmt(inner, // The parser produces a correct AST but if it was modified
 // elsewhere the else branch may be invalid. Check again.
 // valid or error already reported
 (~s).Else);
            break;
        }
        case ж<ast.BlockStmt> : {
            check.stmt(inner, (~s).Else);
            break;
        }
        default: {

            check.error((~s).Else, InvalidSyntaxTree, "invalid else branch in if statement"u8);
            break;
        }}

        break;
    }
    case ж<ast.SwitchStmt> s: {
        inner |= (stmtContext)(breakOk);
        check.openScope(~s, "switch"u8);
        defer(check.closeScope);
        check.simpleStmt((~s).Init);
        ref var xΔ6 = ref heap(new operand(), out var ᏑxΔ6);
        if ((~s).Tag != default!){
            check.expr(nil, ᏑxΔ6, (~s).Tag);
            // By checking assignment of x to an invisible temporary
            // (as a compiler would), we get all the relevant checks.
            check.assignment(ᏑxΔ6, default!, "switch expression"u8);
            if (xΔ6.mode != invalid && !Comparable(xΔ6.typ) && !hasNil(xΔ6.typ)) {
                check.errorf(~ᏑxΔ6, InvalidExprSwitch, "cannot switch on %s (%s is not comparable)"u8, ᏑxΔ6, xΔ6.typ);
                .mode = invalid;
            }
        } else {
            // spec: "A missing switch expression is
            // equivalent to the boolean value true."
            .mode = constant_;
            .typ = Typ[Bool];
            .val = constant.MakeBool(true);
            .expr = Ꮡ(new ast.Ident(NamePos: (~(~s).Body).Lbrace, Name: "true"u8));
        }
        check.multipleDefaults((~(~s).Body).List);
        var seen = new valueMap();
        foreach (var (i, c) in (~(~s).Body).List) {
            // map of seen case values to positions and types
            var (clause, _) = c._<ж<ast.CaseClause>>(ᐧ);
            if (clause == nil) {
                check.error(c, InvalidSyntaxTree, "incorrect expression switch case"u8);
                continue;
            }
            check.caseValues(ᏑxΔ6, (~clause).List, seen);
            check.openScope(~clause, "case"u8);
            stmtContext inner = inner;
            if (i + 1 < len((~(~s).Body).List)){
                inner |= (stmtContext)(fallthroughOk);
            } else {
                inner |= (stmtContext)(finalSwitchCase);
            }
            check.stmtList(inner, (~clause).Body);
            check.closeScope();
        }
        break;
    }
    case ж<ast.TypeSwitchStmt> s: {
        inner |= (stmtContext)((stmtContext)(breakOk | inTypeSwitch));
        check.openScope(~s, "type switch"u8);
        defer(check.closeScope);
        check.simpleStmt((~s).Init);
        // A type switch guard must be of the form:
        //
        //     TypeSwitchGuard = [ identifier ":=" ] PrimaryExpr "." "(" "type" ")" .
        //
        // The parser is checking syntactic correctness;
        // remaining syntactic errors are considered AST errors here.
        // TODO(gri) better factoring of error handling (invalid ASTs)
        //
        ж<ast.Ident> lhs = default!;                   // lhs identifier or nil
        ast.Expr rhsΔ1 = default!;
        switch ((~s).Assign.type()) {
        case ж<ast.ExprStmt> guard: {
             = guard.val.X;
            break;
        }
        case ж<ast.AssignStmt> guard: {
            if (len((~guard).Lhs) != 1 || (~guard).Tok != token.DEFINE || len((~guard).Rhs) != 1) {
                check.error(~s, InvalidSyntaxTree, "incorrect form of type switch guard"u8);
                return;
            }
            (lhs, _) = (~guard).Lhs[0]._<ж<ast.Ident>>(ᐧ);
            if (lhs == nil) {
                check.error(~s, InvalidSyntaxTree, "incorrect form of type switch guard"u8);
                return;
            }
            if ((~lhs).Name == "_"u8){
                // _ := x.(type) is an invalid short variable declaration
                check.softErrorf(~lhs, NoNewVar, "no new variable on left side of :="u8);
                lhs = default!;
            } else {
                // avoid declared and not used error below
                check.recordDef(lhs, default!);
            }
             = (~guard).Rhs[0];
            break;
        }
        default: {
            var guard = (~s).Assign.type();
            check.error(~s, // lhs variable is implicitly declared in each cause clause
 InvalidSyntaxTree, "incorrect form of type switch guard"u8);
            return;
        }}
        var (expr, _) = rhsΔ1._<ж<ast.TypeAssertExpr>>(ᐧ);
        if (expr == nil || (~expr).Type != default!) {
            // rhs must be of the form: expr.(type) and expr must be an ordinary interface
            check.error(~s, InvalidSyntaxTree, "incorrect form of type switch guard"u8);
            return;
        }
        ж<operand> sx = default!;                // switch expression against which cases are compared against; nil if invalid
        {
            ref var xΔ7 = ref heap(new operand(), out var ᏑxΔ7);
            check.expr(nil, ᏑxΔ7, (~expr).X);
            if (xΔ7.mode != invalid) {
                if (isTypeParam(xΔ7.typ)){
                    check.errorf(~ᏑxΔ7, InvalidTypeSwitch, "cannot use type switch on type parameter value %s"u8, ᏑxΔ7);
                } else 
                if (IsInterface(xΔ7.typ)){
                    sx = ᏑxΔ7;
                } else {
                    check.errorf(~ᏑxΔ7, InvalidTypeSwitch, "%s is not an interface"u8, ᏑxΔ7);
                }
            }
        }
        check.multipleDefaults((~(~s).Body).List);
        slice<ж<Var>> lhsVars = default!;                    // list of implicitly declared lhs variables
        seen = new ast.Expr();
        foreach (var (_, sΔ1) in (~(~s).Body).List) {
            // map of seen types to positions
            var (clause, _) = sΔ1._<ж<ast.CaseClause>>(ᐧ);
            if (clause == nil) {
                check.error(sΔ1, InvalidSyntaxTree, "incorrect type switch case"u8);
                continue;
            }
            // Check each type in this type switch case.
            var T = check.caseTypes(sx, (~clause).List, seen);
            check.openScope(~clause, "case"u8);
            // If lhs exists, declare a corresponding variable in the case-local scope.
            if (lhs != nil) {
                // spec: "The TypeSwitchGuard may include a short variable declaration.
                // When that form is used, the variable is declared at the beginning of
                // the implicit block in each clause. In clauses with a case listing
                // exactly one type, the variable has that type; otherwise, the variable
                // has the type of the expression in the TypeSwitchGuard."
                if (len((~clause).List) != 1 || T == default!) {
                    T = ~Typ[Invalid];
                    if (sx != nil) {
                        T = sx.val.typ;
                    }
                }
                var obj = NewVar(lhs.Pos(), check.pkg, (~lhs).Name, T);
                tokenꓸPos scopePos = clause.Pos() + ((tokenꓸPos)len("default"));
                // for default clause (len(List) == 0)
                {
                    nint n = len((~clause).List); if (n > 0) {
                        scopePos = (~clause).List[n - 1].End();
                    }
                }
                check.declare(check.scope, nil, ~obj, scopePos);
                check.recordImplicit(~clause, ~obj);
                // For the "declared and not used" error, all lhs variables act as
                // one; i.e., if any one of them is 'used', all of them are 'used'.
                // Collect them for later analysis.
                lhsVars = append(lhsVars, obj);
            }
            check.stmtList(inner, (~clause).Body);
            check.closeScope();
        }
        if (lhs != nil) {
            // If lhs exists, we must have at least one lhs variable that was used.
            bool used = default!;
            foreach (var (_, v) in lhsVars) {
                if ((~v).used) {
                    used = true;
                }
                v.val.used = true;
            }
            // avoid usage error when checking entire function
            if (!used) {
                check.softErrorf(~lhs, UnusedVar, "%s declared and not used"u8, (~lhs).Name);
            }
        }
        break;
    }
    case ж<ast.SelectStmt> s: {
        inner |= (stmtContext)(breakOk);
        check.multipleDefaults((~(~s).Body).List);
        foreach (var (_, sΔ2) in (~(~s).Body).List) {
            var (clause, _) = sΔ2._<ж<ast.CommClause>>(ᐧ);
            if (clause == nil) {
                continue;
            }
            // error reported before
            // clause.Comm must be a SendStmt, RecvStmt, or default case
            var valid = false;
            ast.Expr rhs = default!;               // rhs of RecvStmt, or nil
            switch ((~clause).Comm.type()) {
            case default! s: {
                valid = true;
                break;
            }
            case ж<ast.SendStmt> s: {
                valid = true;
                break;
            }
            case ж<ast.AssignStmt> s: {
                if (len((~sΔ2).Rhs) == 1) {
                    rhs = (~sΔ2).Rhs[0];
                }
                break;
            }
            case ж<ast.ExprStmt> s: {
                rhs = sΔ2.val.X;
                break;
            }}
            // if present, rhs must be a receive operation
            if (rhs != default!) {
                {
                    var (xΔ8, _) = ast.Unparen(rhs)._<ж<ast.UnaryExpr>>(ᐧ); if (xΔ8 != nil && (~xΔ8).Op == token.ARROW) {
                        valid = true;
                    }
                }
            }
            if (!valid) {
                check.error((~clause).Comm, InvalidSelectCase, "select case must be send or receive (possibly with assignment)"u8);
                continue;
            }
            check.openScope(sΔ2, "case"u8);
            if ((~clause).Comm != default!) {
                check.stmt(inner, (~clause).Comm);
            }
            check.stmtList(inner, (~clause).Body);
            check.closeScope();
        }
        break;
    }
    case ж<ast.ForStmt> s: {
        inner |= (stmtContext)((stmtContext)(breakOk | continueOk));
        check.openScope(~s, "for"u8);
        defer(check.closeScope);
        check.simpleStmt((~s).Init);
        if ((~s).Cond != default!) {
            ref var xΔ9 = ref heap(new operand(), out var ᏑxΔ9);
            check.expr(nil, ᏑxΔ9, (~s).Cond);
            if (xΔ9.mode != invalid && !allBoolean(xΔ9.typ)) {
                check.error((~s).Cond, InvalidCond, "non-boolean condition in for statement"u8);
            }
        }
        check.simpleStmt((~s).Post);
        {
            var (sΔ3, _) = (~s).Post._<ж<ast.AssignStmt>>(ᐧ); if (sΔ3 != nil && (~sΔ3).Tok == token.DEFINE) {
                // spec: "The init statement may be a short variable
                // declaration, but the post statement must not."
                check.softErrorf(~sΔ3, InvalidPostDecl, "cannot declare in post statement"u8);
                // Don't call useLHS here because we want to use the lhs in
                // this erroneous statement so that we don't get errors about
                // these lhs variables being declared and not used.
                check.use((~sΔ3).Lhs.ꓸꓸꓸ);
            }
        }
        check.stmt(inner, // avoid follow-up errors
 ~(~s).Body);
        break;
    }
    case ж<ast.RangeStmt> s: {
        inner |= (stmtContext)((stmtContext)(breakOk | continueOk));
        check.rangeStmt(inner, Ꮡs);
        break;
    }
    default: {
        var s = s.type();
        check.error(s, InvalidSyntaxTree, "invalid statement"u8);
        break;
    }}
});

[GoRecv] public static void rangeStmt(this ref Checker check, stmtContext inner, ж<ast.RangeStmt> Ꮡs) => func((defer, _) => {
    ref var s = ref Ꮡs.val;

    var identName = (ж<identType> n) => (~n).Name;
    var sKey = s.Key;
    var sValue = s.Value;
    ast.Expr sExtra = default!;         // (used only in types2 fork)
    var isDef = s.Tok == token.DEFINE;
    var rangeVar = s.X;
    var noNewVarPos = inNode(~s, s.TokPos);
    // Everything from here on is shared between cmd/compile/internal/types2 and go/types.
    // check expression to iterate over
    ref var x = ref heap(new operand(), out var Ꮡx);
    check.expr(nil, Ꮡx, rangeVar);
    // determine key/value types
    ΔType key = default!;
    ΔType val = default!;
    if (x.mode != invalid) {
        // Ranging over a type parameter is permitted if it has a core type.
        var (k, v, cause, ok) = rangeKeyVal(x.typ, 
        var xʗ1 = x;
        (goVersion v) => check.allowVersion(xʗ1.expr, vΔ1));
        switch (ᐧ) {
        case {} when !ok && cause != ""u8: {
            check.softErrorf(~Ꮡx, InvalidRangeExpr, "cannot range over %s: %s"u8, Ꮡx, cause);
            break;
        }
        case {} when !ok: {
            check.softErrorf(~Ꮡx, InvalidRangeExpr, "cannot range over %s"u8, Ꮡx);
            break;
        }
        case {} when k == default! && sKey != default!: {
            check.softErrorf(sKey, InvalidIterVar, "range over %s permits no iteration variables"u8, Ꮡx);
            break;
        }
        case {} when v == default! && sValue != default!: {
            check.softErrorf(sValue, InvalidIterVar, "range over %s permits only one iteration variable"u8, Ꮡx);
            break;
        }
        case {} when sExtra != default!: {
            check.softErrorf(sExtra, InvalidIterVar, "range clause permits at most two iteration variables"u8);
            break;
        }}

        (key, val) = (k, v);
    }
    // Open the for-statement block scope now, after the range clause.
    // Iteration variables declared with := need to go in this scope (was go.dev/issue/51437).
    check.openScope(~s, "range"u8);
    defer(check.closeScope);
    // check assignment to/declaration of iteration variables
    // (irregular assignment, cannot easily map to existing assignment checks)
    // lhs expressions and initialization value (rhs) types
    var lhs = new Expr[]{sKey, sValue}.array();
    // sKey, sValue may be nil
    var rhs = new ΔType[]{key, val}.array();
    // key, val may be nil
    var rangeOverInt = isInteger(x.typ);
    if (isDef){
        // short variable declaration
        slice<ж<Var>> vars = default!;
        foreach (var (i, lhsΔ1) in lhs) {
            if (lhsΔ1 == default!) {
                continue;
            }
            // determine lhs variable
            ж<Var> obj = default!;
            {
                var (ident, _) = lhsΔ1._<identType.val>(ᐧ); if (ident != nil){
                    // declare new variable
                    @string name = identName(ident);
                    obj = NewVar(ident.Pos(), check.pkg, name, default!);
                    check.recordDef(ident, ~obj);
                    // _ variables don't count as new variables
                    if (name != "_"u8) {
                        vars = append(vars, obj);
                    }
                } else {
                    check.errorf(lhsΔ1, InvalidSyntaxTree, "cannot declare %s"u8, lhsΔ1);
                    obj = NewVar(lhsΔ1.Pos(), check.pkg, "_"u8, default!);
                }
            }
            // dummy variable
            assert(obj.typ == default!);
            // initialize lhs iteration variable, if any
            var typ = rhs[i];
            if (typ == default! || Ꮡtyp == ~Typ[Invalid]) {
                // typ == Typ[Invalid] can happen if allowVersion fails.
                obj.typ = Typ[Invalid];
                obj.val.used = true;
                // don't complain about unused variable
                continue;
            }
            if (rangeOverInt){
                assert(i == 0);
                // at most one iteration variable (rhs[1] == nil or Typ[Invalid] for rangeOverInt)
                check.initVar(obj, Ꮡx, "range clause"u8);
            } else {
                ref var yΔ1 = ref heap(new operand(), out var ᏑyΔ1);
                .mode = value;
                .expr = lhsΔ1;
                // we don't have a better rhs expression to use here
                .typ = typ;
                check.initVar(obj, ᏑyΔ1, "assignment"u8);
            }
            // error is on variable, use "assignment" not "range clause"
            assert(obj.typ != default!);
        }
        // declare variables
        if (len(vars) > 0){
            tokenꓸPos scopePos = s.Body.Pos();
            foreach (var (_, obj) in vars) {
                check.declare(check.scope, nil, /* recordDef already called */
 ~obj, scopePos);
            }
        } else {
            check.error(noNewVarPos, NoNewVar, "no new variables on left side of :="u8);
        }
    } else 
    if (sKey != default!){
        /* lhs[0] != nil */
        // ordinary assignment
        foreach (var (i, lhsΔ2) in lhs) {
            if (lhsΔ2 == default!) {
                continue;
            }
            // assign to lhs iteration variable, if any
            var typ = rhs[i];
            if (typ == default! || Ꮡtyp == ~Typ[Invalid]) {
                continue;
            }
            if (rangeOverInt){
                assert(i == 0);
                // at most one iteration variable (rhs[1] == nil or Typ[Invalid] for rangeOverInt)
                check.assignVar(lhsΔ2, default!, Ꮡx, "range clause"u8);
                // If the assignment succeeded, if x was untyped before, it now
                // has a type inferred via the assignment. It must be an integer.
                // (go.dev/issues/67027)
                if (x.mode != invalid && !isInteger(x.typ)) {
                    check.softErrorf(lhsΔ2, InvalidRangeExpr, "cannot use iteration variable of type %s"u8, x.typ);
                }
            } else {
                ref var y = ref heap(new operand(), out var Ꮡy);
                y.mode = value;
                y.expr = lhsΔ2;
                // we don't have a better rhs expression to use here
                y.typ = typ;
                check.assignVar(lhsΔ2, default!, Ꮡy, "assignment"u8);
            }
        }
    } else 
    if (rangeOverInt) {
        // error is on variable, use "assignment" not "range clause"
        // If we don't have any iteration variables, we still need to
        // check that a (possibly untyped) integer range expression x
        // is valid.
        // We do this by checking the assignment _ = x. This ensures
        // that an untyped x can be converted to a value of its default
        // type (rune or int).
        check.assignment(Ꮡx, default!, "range clause"u8);
    }
    check.stmt(inner, ~s.Body);
});

// rangeKeyVal returns the key and value type produced by a range clause
// over an expression of type typ.
// If allowVersion != nil, it is used to check the required language version.
// If the range clause is not permitted, rangeKeyVal returns ok = false.
// When ok = false, rangeKeyVal may also return a reason in cause.
internal static (ΔType key, ΔType val, @string cause, bool ok) rangeKeyVal(ΔType typ, Func<goVersion, bool> allowVersion) {
    ΔType key = default!;
    ΔType val = default!;
    @string cause = default!;
    bool ok = default!;

    var bad = 
    var Typʗ1 = Typ;
    (@string cause) => (~Typʗ1[Invalid], ~Typʗ1[Invalid], causeΔ1, false);
    var toSig = 
    (ΔType t) => {
        var (sig, _) = coreType(t)._<ΔSignature.val>(ᐧ);
        return ~sig;
    };
    var orig = typ;
    switch (arrayPtrDeref(coreType(typ)).type()) {
    case default! typ: {
        return bad("no core type"u8);
    }
    case Basic.val typ: {
        if (isString(~typ)) {
            return (~Typ[Int], universeRune, "", true);
        }
        if (isInteger(~typ)) {
            // use 'rune' name
            if (allowVersion != default! && !allowVersion(go1_22)) {
                return bad("requires go1.22 or later"u8);
            }
            return (orig, default!, "", true);
        }
        break;
    }
    case Array.val typ: {
        return (~Typ[Int], (~typ).elem, "", true);
    }
    case Slice.val typ: {
        return (~Typ[Int], (~typ).elem, "", true);
    }
    case Map.val typ: {
        return ((~typ).key, (~typ).elem, "", true);
    }
    case Chan.val typ: {
        if ((~typ).dir == SendOnly) {
            return bad("receive from send-only channel"u8);
        }
        return ((~typ).elem, default!, "", true);
    }
    case ΔSignature.val typ: {
        if (!buildcfg.Experiment.RangeFunc && allowVersion != default! && !allowVersion(go1_23)) {
            return bad("requires go1.23 or later"u8);
        }
        assert(typ.Recv() == nil);
        switch (ᐧ) {
        case {} when typ.Params().Len() is != 1: {
            return bad("func must be func(yield func(...) bool): wrong argument count"u8);
        }
        case {} when toSig(typ.Params().At(0).Type()) == nil: {
            return bad("func must be func(yield func(...) bool): argument is not func"u8);
        }
        case {} when typ.Results().Len() is != 0: {
            return bad("func must be func(yield func(...) bool): unexpected results"u8);
        }}

        var cb = toSig(typ.Params().At(0).Type());
        assert(cb.Recv() == nil);
        switch (ᐧ) {
        case {} when cb.Params().Len() is > 2: {
            return bad("func must be func(yield func(...) bool): yield func has too many parameters"u8);
        }
        case {} when cb.Results().Len() != 1 || !isBoolean(cb.Results().At(0).Type()): {
            return bad("func must be func(yield func(...) bool): yield func does not return bool"u8);
        }}

        if (cb.Params().Len() >= 1) {
            key = cb.Params().At(0).Type();
        }
        if (cb.Params().Len() >= 2) {
            val = cb.Params().At(1).Type();
        }
        return (key, val, "", true);
    }}
    return (key, val, cause, ok);
}

} // end types_package
