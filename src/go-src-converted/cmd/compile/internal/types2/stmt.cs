// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements typechecking of statements.

// package types2 -- go2cs converted at 2022 March 13 06:26:20 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\stmt.go
namespace go.cmd.compile.@internal;

using syntax = cmd.compile.@internal.syntax_package;
using constant = go.constant_package;
using sort = sort_package;
using System;

public static partial class types2_package {

private static void funcBody(this ptr<Checker> _addr_check, ptr<declInfo> _addr_decl, @string name, ptr<Signature> _addr_sig, ptr<syntax.BlockStmt> _addr_body, constant.Value iota) => func((defer, panic, _) => {
    ref Checker check = ref _addr_check.val;
    ref declInfo decl = ref _addr_decl.val;
    ref Signature sig = ref _addr_sig.val;
    ref syntax.BlockStmt body = ref _addr_body.val;

    if (check.conf.IgnoreFuncBodies) {
        panic("internal error: function body not ignored");
    }
    if (check.conf.Trace) {
        check.trace(body.Pos(), "--- %s: %s", name, sig);
        defer(() => {
            check.trace(syntax.EndPos(body), "--- <end>");
        }());
    }
    sig.scope.pos = body.Pos();
    sig.scope.end = syntax.EndPos(body); 

    // save/restore current context and setup function context
    // (and use 0 indentation at function start)
    defer((ctxt, indent) => {
        check.context = ctxt;
        check.indent = indent;
    }(check.context, check.indent));
    check.context = new context(decl:decl,scope:sig.scope,iota:iota,sig:sig,);
    check.indent = 0;

    check.stmtList(0, body.List);

    if (check.hasLabel && !check.conf.IgnoreLabels) {
        check.labels(body);
    }
    if (sig.results.Len() > 0 && !check.isTerminating(body, "")) {
        check.error(body.Rbrace, "missing return");
    }
    check.usage(sig.scope);
});

private static void usage(this ptr<Checker> _addr_check, ptr<Scope> _addr_scope) {
    ref Checker check = ref _addr_check.val;
    ref Scope scope = ref _addr_scope.val;

    slice<ptr<Var>> unused = default;
    foreach (var (_, elem) in scope.elems) {
        {
            ptr<Var> v__prev1 = v;

            ptr<Var> (v, _) = elem._<ptr<Var>>();

            if (v != null && !v.used) {
                unused = append(unused, v);
            }

            v = v__prev1;

        }
    }    sort.Slice(unused, (i, j) => unused[i].pos.Cmp(unused[j].pos) < 0);
    {
        ptr<Var> v__prev1 = v;

        foreach (var (_, __v) in unused) {
            v = __v;
            check.softErrorf(v.pos, "%s declared but not used", v.name);
        }
        v = v__prev1;
    }

    foreach (var (_, scope) in scope.children) { 
        // Don't go inside function literal scopes a second time;
        // they are handled explicitly by funcBody.
        if (!scope.isFunc) {
            check.usage(scope);
        }
    }
}

// stmtContext is a bitset describing which
// control-flow statements are permissible,
// and provides additional context information
// for better error messages.
private partial struct stmtContext { // : nuint
}

 
// permissible control-flow statements
private static readonly stmtContext breakOk = 1 << (int)(iota);
private static readonly var continueOk = 0;
private static readonly var fallthroughOk = 1; 

// additional context information
private static readonly var finalSwitchCase = 2;

private static void simpleStmt(this ptr<Checker> _addr_check, syntax.Stmt s) {
    ref Checker check = ref _addr_check.val;

    if (s != null) {
        check.stmt(0, s);
    }
}

private static slice<syntax.Stmt> trimTrailingEmptyStmts(slice<syntax.Stmt> list) {
    for (var i = len(list); i > 0; i--) {
        {
            ptr<syntax.EmptyStmt> (_, ok) = list[i - 1]._<ptr<syntax.EmptyStmt>>();

            if (!ok) {
                return list[..(int)i];
            }

        }
    }
    return null;
}

private static void stmtList(this ptr<Checker> _addr_check, stmtContext ctxt, slice<syntax.Stmt> list) {
    ref Checker check = ref _addr_check.val;

    var ok = ctxt & fallthroughOk != 0;
    var inner = ctxt & ~fallthroughOk;
    list = trimTrailingEmptyStmts(list); // trailing empty statements are "invisible" to fallthrough analysis
    foreach (var (i, s) in list) {
        inner = inner;
        if (ok && i + 1 == len(list)) {
            inner |= fallthroughOk;
        }
        check.stmt(inner, s);
    }
}

private static void multipleSwitchDefaults(this ptr<Checker> _addr_check, slice<ptr<syntax.CaseClause>> list) {
    ref Checker check = ref _addr_check.val;

    ptr<syntax.CaseClause> first;
    foreach (var (_, c) in list) {
        if (c.Cases == null) {
            if (first != null) {
                check.errorf(c, "multiple defaults (first at %s)", first.Pos()); 
                // TODO(gri) probably ok to bail out after first error (and simplify this code)
            }
            else
 {
                first = c;
            }
        }
    }
}

private static void multipleSelectDefaults(this ptr<Checker> _addr_check, slice<ptr<syntax.CommClause>> list) {
    ref Checker check = ref _addr_check.val;

    ptr<syntax.CommClause> first;
    foreach (var (_, c) in list) {
        if (c.Comm == null) {
            if (first != null) {
                check.errorf(c, "multiple defaults (first at %s)", first.Pos()); 
                // TODO(gri) probably ok to bail out after first error (and simplify this code)
            }
            else
 {
                first = c;
            }
        }
    }
}

private static void openScope(this ptr<Checker> _addr_check, syntax.Node node, @string comment) {
    ref Checker check = ref _addr_check.val;

    check.openScopeUntil(node, syntax.EndPos(node), comment);
}

private static void openScopeUntil(this ptr<Checker> _addr_check, syntax.Node node, syntax.Pos end, @string comment) {
    ref Checker check = ref _addr_check.val;

    var scope = NewScope(check.scope, node.Pos(), end, comment);
    check.recordScope(node, scope);
    check.scope = scope;
}

private static void closeScope(this ptr<Checker> _addr_check) {
    ref Checker check = ref _addr_check.val;

    check.scope = check.scope.Parent();
}

private static void suspendedCall(this ptr<Checker> _addr_check, @string keyword, ptr<syntax.CallExpr> _addr_call) {
    ref Checker check = ref _addr_check.val;
    ref syntax.CallExpr call = ref _addr_call.val;

    ref operand x = ref heap(out ptr<operand> _addr_x);
    @string msg = default;

    if (check.rawExpr(_addr_x, call, null) == conversion) 
        msg = "requires function call, not conversion";
    else if (check.rawExpr(_addr_x, call, null) == expression) 
        msg = "discards result of";
    else if (check.rawExpr(_addr_x, call, null) == statement) 
        return ;
    else 
        unreachable();
        check.errorf(_addr_x, "%s %s %s", keyword, msg, _addr_x);
}

// goVal returns the Go value for val, or nil.
private static void goVal(constant.Value val) { 
    // val should exist, but be conservative and check
    if (val == null) {
        return null;
    }

    if (val.Kind() == constant.Int) 
        {
            var x__prev1 = x;

            var (x, ok) = constant.Int64Val(val);

            if (ok) {
                return x;
            }

            x = x__prev1;

        }
        {
            var x__prev1 = x;

            (x, ok) = constant.Uint64Val(val);

            if (ok) {
                return x;
            }

            x = x__prev1;

        }
    else if (val.Kind() == constant.Float) 
        {
            var x__prev1 = x;

            (x, ok) = constant.Float64Val(val);

            if (ok) {
                return x;
            }

            x = x__prev1;

        }
    else if (val.Kind() == constant.String) 
        return constant.StringVal(val);
        return null;
}

// A valueMap maps a case value (of a basic Go type) to a list of positions
// where the same case value appeared, together with the corresponding case
// types.
// Since two case values may have the same "underlying" value but different
// types we need to also check the value's types (e.g., byte(1) vs myByte(1))
// when the switch expression is of interface type.
private partial struct valueType {
    public syntax.Pos pos;
    public Type typ;
}private static void caseValues(this ptr<Checker> _addr_check, ptr<operand> _addr_x, slice<syntax.Expr> values, valueMap seen) {
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;

L:
    foreach (var (_, e) in values) {
        ref operand v = ref heap(out ptr<operand> _addr_v);
        check.expr(_addr_v, e);
        if (x.mode == invalid || v.mode == invalid) {
            _continueL = true;
            break;
        }
        check.convertUntyped(_addr_v, x.typ);
        if (v.mode == invalid) {
            _continueL = true;
            break;
        }
        ref var res = ref heap(v, out ptr<var> _addr_res); // keep original v unchanged
        check.comparison(_addr_res, x, syntax.Eql);
        if (res.mode == invalid) {
            _continueL = true;
            break;
        }
        if (v.mode != constant_) {
            _continueL = true; // we're done
            break;
        }
        {
            var val = goVal(v.val);

            if (val != null) { 
                // look for duplicate types for a given value
                // (quadratic algorithm, but these lists tend to be very short)
                foreach (var (_, vt) in seen[val]) {
                    if (check.identical(v.typ, vt.typ)) {
                        ref error_ err = ref heap(out ptr<error_> _addr_err);
                        err.errorf(_addr_v, "duplicate case %s in expression switch", _addr_v);
                        err.errorf(vt.pos, "previous case");
                        check.report(_addr_err);
                        _continueL = true;
                        break;
                    }
                }
                seen[val] = append(seen[val], new valueType(v.Pos(),v.typ));
            }

        }
    }
}

private static Type caseTypes(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<Interface> _addr_xtyp, slice<syntax.Expr> types, map<Type, syntax.Expr> seen) {
    Type T = default;
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref Interface xtyp = ref _addr_xtyp.val;

L:
    foreach (var (_, e) in types) {
        T = check.typOrNil(e);
        if (T == Typ[Invalid]) {
            _continueL = true;
            break;
        }
        if (T != null) {
            check.ordinaryType(e.Pos(), T);
        }
        foreach (var (t, other) in seen) {
            if (T == null && t == null || T != null && t != null && check.identical(T, t)) { 
                // talk about "case" rather than "type" because of nil case
                @string Ts = "nil";
                if (T != null) {
                    Ts = T.String();
                }
                ref error_ err = ref heap(out ptr<error_> _addr_err);
                err.errorf(e, "duplicate case %s in type switch", Ts);
                err.errorf(other, "previous case");
                check.report(_addr_err);
                _continueL = true;
                break;
            }
        }        seen[T] = e;
        if (T != null) {
            check.typeAssertion(e.Pos(), x, xtyp, T);
        }
    }    return ;
}

// stmt typechecks statement s.
private static void stmt(this ptr<Checker> _addr_check, stmtContext ctxt, syntax.Stmt s) => func((defer, panic, _) => {
    ref Checker check = ref _addr_check.val;
 
    // statements must end with the same top scope as they started with
    if (debug) {
        defer(scope => { 
            // don't check if code is panicking
            {
                var p = recover();

                if (p != null) {
                    panic(p);
                }

            }
            assert(scope == check.scope);
        }(check.scope));
    }
    defer(check.processDelayed(len(check.delayed)));

    var inner = ctxt & ~(fallthroughOk | finalSwitchCase);
    switch (s.type()) {
        case ptr<syntax.EmptyStmt> s:
            break;
        case ptr<syntax.DeclStmt> s:
            check.declStmt(s.DeclList);
            break;
        case ptr<syntax.LabeledStmt> s:
            check.hasLabel = true;
            check.stmt(ctxt, s.Stmt);
            break;
        case ptr<syntax.ExprStmt> s:
            ref operand x = ref heap(out ptr<operand> _addr_x);
            var kind = check.rawExpr(_addr_x, s.X, null);
            @string msg = default;

            if (x.mode == builtin) 
                msg = "must be called";
            else if (x.mode == typexpr) 
                msg = "is not an expression";
            else 
                if (kind == statement) {
                    return ;
                }
                msg = "is not used";
                        check.errorf(_addr_x, "%s %s", _addr_x, msg);
            break;
        case ptr<syntax.SendStmt> s:
            ref operand ch = ref heap(out ptr<operand> _addr_ch);            x = default;

            check.expr(_addr_ch, s.Chan);
            check.expr(_addr_x, s.Value);
            if (ch.mode == invalid || x.mode == invalid) {
                return ;
            }
            var tch = asChan(ch.typ);
            if (tch == null) {
                check.errorf(s, invalidOp + "cannot send to non-chan type %s", ch.typ);
                return ;
            }
            if (tch.dir == RecvOnly) {
                check.errorf(s, invalidOp + "cannot send to receive-only type %s", tch);
                return ;
            }
            check.assignment(_addr_x, tch.elem, "send");
            break;
        case ptr<syntax.AssignStmt> s:
            var lhs = unpackExpr(s.Lhs);
            if (s.Rhs == null) { 
                // x++ or x--
                if (len(lhs) != 1) {
                    check.errorf(s, invalidAST + "%s%s requires one operand", s.Op, s.Op);
                    return ;
                }
                x = default;
                check.expr(_addr_x, lhs[0]);
                if (x.mode == invalid) {
                    return ;
                }
                if (!isNumeric(x.typ)) {
                    check.errorf(lhs[0], invalidOp + "%s%s%s (non-numeric type %s)", lhs[0], s.Op, s.Op, x.typ);
                    return ;
                }
                check.assignVar(lhs[0], _addr_x);
                return ;
            }
            var rhs = unpackExpr(s.Rhs);

            if (s.Op == 0) 
                check.assignVars(lhs, rhs);
                return ;
            else if (s.Op == syntax.Def) 
                check.shortVarDecl(s.Pos(), lhs, rhs);
                return ;
            // assignment operations
            if (len(lhs) != 1 || len(rhs) != 1) {
                check.errorf(s, "assignment operation %s requires single-valued expressions", s.Op);
                return ;
            }
            x = default;
            check.binary(_addr_x, null, lhs[0], rhs[0], s.Op);
            check.assignVar(lhs[0], _addr_x);
            break;
        case ptr<syntax.CallStmt> s:
            kind = "go";
            if (s.Tok == syntax.Defer) {
                kind = "defer";
            }
            check.suspendedCall(kind, s.Call);
            break;
        case ptr<syntax.ReturnStmt> s:
            var res = check.sig.results;
            var results = unpackExpr(s.Results);
            if (res.Len() > 0) { 
                // function returns results
                // (if one, say the first, result parameter is named, all of them are named)
                if (len(results) == 0 && res.vars[0].name != "") { 
                    // spec: "Implementation restriction: A compiler may disallow an empty expression
                    // list in a "return" statement if a different entity (constant, type, or variable)
                    // with the same name as a result parameter is in scope at the place of the return."
                    foreach (var (_, obj) in res.vars) {
                        {
                            var alt = check.lookup(obj.name);

                            if (alt != null && alt != obj) {
                                ref error_ err = ref heap(out ptr<error_> _addr_err);
                                err.errorf(s, "result parameter %s not in scope at return", obj.name);
                                err.errorf(alt, "inner declaration of %s", obj);
                                check.report(_addr_err); 
                                // ok to continue
                            }

                        }
                    }
                else
                } { 
                    // return has results or result parameters are unnamed
                    check.initVars(res.vars, results, s.Pos());
                }
            }
            else if (len(results) > 0) {
                check.error(results[0], "no result values expected");
                check.use(results);
            }
            break;
        case ptr<syntax.BranchStmt> s:
            if (s.Label != null) {
                check.hasLabel = true;
                break; // checked in 2nd pass (check.labels)
            }

            if (s.Tok == syntax.Break)
            {
                if (ctxt & breakOk == 0) {
                    if (check.conf.CompilerErrorMessages) {
                        check.error(s, "break is not in a loop, switch, or select statement");
                    }
                    else
 {
                        check.error(s, "break not in for, switch, or select statement");
                    }
                }
                goto __switch_break0;
            }
            if (s.Tok == syntax.Continue)
            {
                if (ctxt & continueOk == 0) {
                    if (check.conf.CompilerErrorMessages) {
                        check.error(s, "continue is not in a loop");
                    }
                    else
 {
                        check.error(s, "continue not in for statement");
                    }
                }
                goto __switch_break0;
            }
            if (s.Tok == syntax.Fallthrough)
            {
                if (ctxt & fallthroughOk == 0) {
                    msg = "fallthrough statement out of place";
                    if (ctxt & finalSwitchCase != 0) {
                        msg = "cannot fallthrough final case in switch";
                    }
                    check.error(s, msg);
                }
                goto __switch_break0;
            }
            if (s.Tok == syntax.Goto) 
            {
                // goto's must have labels, should have been caught above
            }
            // default: 
                check.errorf(s, invalidAST + "branch statement: %s", s.Tok);

            __switch_break0:;
            break;
        case ptr<syntax.BlockStmt> s:
            check.openScope(s, "block");
            defer(check.closeScope());

            check.stmtList(inner, s.List);
            break;
        case ptr<syntax.IfStmt> s:
            check.openScope(s, "if");
            defer(check.closeScope());

            check.simpleStmt(s.Init);
            x = default;
            check.expr(_addr_x, s.Cond);
            if (x.mode != invalid && !isBoolean(x.typ)) {
                check.error(s.Cond, "non-boolean condition in if statement");
            }
            check.stmt(inner, s.Then); 
            // The parser produces a correct AST but if it was modified
            // elsewhere the else branch may be invalid. Check again.
            switch (s.Else.type()) {
                case 
                    break;
                case ptr<syntax.IfStmt> _:
                    check.stmt(inner, s.Else);
                    break;
                case ptr<syntax.BlockStmt> _:
                    check.stmt(inner, s.Else);
                    break;
                default:
                {
                    check.error(s.Else, "invalid else branch in if statement");
                    break;
                }

            }
            break;
        case ptr<syntax.SwitchStmt> s:
            inner |= breakOk;
            check.openScope(s, "switch");
            defer(check.closeScope());

            check.simpleStmt(s.Init);

            {
                ptr<syntax.TypeSwitchGuard> (g, _) = s.Tag._<ptr<syntax.TypeSwitchGuard>>();

                if (g != null) {
                    check.typeSwitchStmt(inner, s, g);
                }
                else
 {
                    check.switchStmt(inner, s);
                }

            }
            break;
        case ptr<syntax.SelectStmt> s:
            inner |= breakOk;

            check.multipleSelectDefaults(s.Body);

            foreach (var (i, clause) in s.Body) {
                if (clause == null) {
                    continue; // error reported before
                } 

                // clause.Comm must be a SendStmt, RecvStmt, or default case
                var valid = false;
                rhs = default; // rhs of RecvStmt, or nil
                switch (clause.Comm.type()) {
                    case ptr<syntax.SendStmt> s:
                        valid = true;
                        break;
                    case ptr<syntax.AssignStmt> s:
                        {
                            ptr<syntax.ListExpr> (_, ok) = s.Rhs._<ptr<syntax.ListExpr>>();

                            if (!ok) {
                                rhs = s.Rhs;
                            }

                        }
                        break;
                    case ptr<syntax.ExprStmt> s:
                        rhs = s.X;
                        break; 

                    // if present, rhs must be a receive operation
                } 

                // if present, rhs must be a receive operation
                if (rhs != null) {
                    {
                        operand x__prev2 = x;

                        ptr<syntax.Operation> (x, _) = unparen(rhs)._<ptr<syntax.Operation>>();

                        if (x != null && x.Y == null && x.Op == syntax.Recv) {
                            valid = true;
                        }

                        x = x__prev2;

                    }
                }
                if (!valid) {
                    check.error(clause.Comm, "select case must be send or receive (possibly with assignment)");
                    continue;
                }
                var end = s.Rbrace;
                if (i + 1 < len(s.Body)) {
                    end = s.Body[i + 1].Pos();
                }
                check.openScopeUntil(clause, end, "case");
                if (clause.Comm != null) {
                    check.stmt(inner, clause.Comm);
                }
                check.stmtList(inner, clause.Body);
                check.closeScope();
            }
            break;
        case ptr<syntax.ForStmt> s:
            inner |= breakOk | continueOk;
            check.openScope(s, "for");
            defer(check.closeScope());

            {
                ptr<syntax.RangeClause> (rclause, _) = s.Init._<ptr<syntax.RangeClause>>();

                if (rclause != null) {
                    check.rangeStmt(inner, s, rclause);
                    break;
                }

            }

            check.simpleStmt(s.Init);
            if (s.Cond != null) {
                x = default;
                check.expr(_addr_x, s.Cond);
                if (x.mode != invalid && !isBoolean(x.typ)) {
                    check.error(s.Cond, "non-boolean condition in for statement");
                }
            }
            check.simpleStmt(s.Post); 
            // spec: "The init statement may be a short variable
            // declaration, but the post statement must not."
            {
                var s__prev1 = s;

                ptr<syntax.AssignStmt> (s, _) = s.Post._<ptr<syntax.AssignStmt>>();

                if (s != null && s.Op == syntax.Def) { 
                    // The parser already reported an error.
                    // Don't call useLHS here because we want to use the lhs in
                    // this erroneous statement so that we don't get errors about
                    // these lhs variables being declared but not used.
                    check.use(s.Lhs); // avoid follow-up errors
                }

                s = s__prev1;

            }
            check.stmt(inner, s.Body);
            break;
        default:
        {
            var s = s.type();
            check.error(s, "invalid statement");
            break;
        }
    }
});

private static void switchStmt(this ptr<Checker> _addr_check, stmtContext inner, ptr<syntax.SwitchStmt> _addr_s) {
    ref Checker check = ref _addr_check.val;
    ref syntax.SwitchStmt s = ref _addr_s.val;
 
    // init statement already handled

    ref operand x = ref heap(out ptr<operand> _addr_x);
    if (s.Tag != null) {
        check.expr(_addr_x, s.Tag); 
        // By checking assignment of x to an invisible temporary
        // (as a compiler would), we get all the relevant checks.
        check.assignment(_addr_x, null, "switch expression");
        if (x.mode != invalid && !Comparable(x.typ) && !hasNil(x.typ)) {
            check.errorf(_addr_x, "cannot switch on %s (%s is not comparable)", _addr_x, x.typ);
            x.mode = invalid;
        }
    }
    else
 { 
        // spec: "A missing switch expression is
        // equivalent to the boolean value true."
        x.mode = constant_;
        x.typ = Typ[Bool];
        x.val = constant.MakeBool(true); 
        // TODO(gri) should have a better position here
        var pos = s.Rbrace;
        if (len(s.Body) > 0) {
            pos = s.Body[0].Pos();
        }
        x.expr = syntax.NewName(pos, "true");
    }
    check.multipleSwitchDefaults(s.Body);

    var seen = make(valueMap); // map of seen case values to positions and types
    foreach (var (i, clause) in s.Body) {
        if (clause == null) {
            check.error(clause, invalidAST + "incorrect expression switch case");
            continue;
        }
        var end = s.Rbrace;
        var inner = inner;
        if (i + 1 < len(s.Body)) {
            end = s.Body[i + 1].Pos();
            inner |= fallthroughOk;
        }
        else
 {
            inner |= finalSwitchCase;
        }
        check.caseValues(_addr_x, unpackExpr(clause.Cases), seen);
        check.openScopeUntil(clause, end, "case");
        check.stmtList(inner, clause.Body);
        check.closeScope();
    }
}

private static void typeSwitchStmt(this ptr<Checker> _addr_check, stmtContext inner, ptr<syntax.SwitchStmt> _addr_s, ptr<syntax.TypeSwitchGuard> _addr_guard) {
    ref Checker check = ref _addr_check.val;
    ref syntax.SwitchStmt s = ref _addr_s.val;
    ref syntax.TypeSwitchGuard guard = ref _addr_guard.val;
 
    // init statement already handled

    // A type switch guard must be of the form:
    //
    //     TypeSwitchGuard = [ identifier ":=" ] PrimaryExpr "." "(" "type" ")" .
    //                          \__lhs__/        \___rhs___/

    // check lhs, if any
    var lhs = guard.Lhs;
    if (lhs != null) {
        if (lhs.Value == "_") { 
            // _ := x.(type) is an invalid short variable declaration
            check.softErrorf(lhs, "no new variable on left side of :=");
            lhs = null; // avoid declared but not used error below
        }
        else
 {
            check.recordDef(lhs, null); // lhs variable is implicitly declared in each cause clause
        }
    }
    ref operand x = ref heap(out ptr<operand> _addr_x);
    check.expr(_addr_x, guard.X);
    if (x.mode == invalid) {
        return ;
    }
    ptr<Interface> (xtyp, _) = under(x.typ)._<ptr<Interface>>();
    if (xtyp == null) {
        check.errorf(_addr_x, "%s is not an interface type", _addr_x);
        return ;
    }
    check.ordinaryType(x.Pos(), xtyp);

    check.multipleSwitchDefaults(s.Body);

    slice<ptr<Var>> lhsVars = default; // list of implicitly declared lhs variables
    var seen = make_map<Type, syntax.Expr>(); // map of seen types to positions
    foreach (var (i, clause) in s.Body) {
        if (clause == null) {
            check.error(s, invalidAST + "incorrect type switch case");
            continue;
        }
        var end = s.Rbrace;
        if (i + 1 < len(s.Body)) {
            end = s.Body[i + 1].Pos();
        }
        var cases = unpackExpr(clause.Cases);
        var T = check.caseTypes(_addr_x, xtyp, cases, seen);
        check.openScopeUntil(clause, end, "case"); 
        // If lhs exists, declare a corresponding variable in the case-local scope.
        if (lhs != null) { 
            // spec: "The TypeSwitchGuard may include a short variable declaration.
            // When that form is used, the variable is declared at the beginning of
            // the implicit block in each clause. In clauses with a case listing
            // exactly one type, the variable has that type; otherwise, the variable
            // has the type of the expression in the TypeSwitchGuard."
            if (len(cases) != 1 || T == null) {
                T = x.typ;
            }
            var obj = NewVar(lhs.Pos(), check.pkg, lhs.Value, T); 
            // TODO(mdempsky): Just use clause.Colon? Why did I even suggest
            // "at the end of the TypeSwitchCase" in #16794 instead?
            var scopePos = clause.Pos(); // for default clause (len(List) == 0)
            {
                var n = len(cases);

                if (n > 0) {
                    scopePos = syntax.EndPos(cases[n - 1]);
                }

            }
            check.declare(check.scope, null, obj, scopePos);
            check.recordImplicit(clause, obj); 
            // For the "declared but not used" error, all lhs variables act as
            // one; i.e., if any one of them is 'used', all of them are 'used'.
            // Collect them for later analysis.
            lhsVars = append(lhsVars, obj);
        }
        check.stmtList(inner, clause.Body);
        check.closeScope();
    }    if (lhs != null) {
        bool used = default;
        foreach (var (_, v) in lhsVars) {
            if (v.used) {
                used = true;
            }
            v.used = true; // avoid usage error when checking entire function
        }        if (!used) {
            check.softErrorf(lhs, "%s declared but not used", lhs.Value);
        }
    }
}

private static void rangeStmt(this ptr<Checker> _addr_check, stmtContext inner, ptr<syntax.ForStmt> _addr_s, ptr<syntax.RangeClause> _addr_rclause) {
    ref Checker check = ref _addr_check.val;
    ref syntax.ForStmt s = ref _addr_s.val;
    ref syntax.RangeClause rclause = ref _addr_rclause.val;
 
    // scope already opened

    // check expression to iterate over
    ref operand x = ref heap(out ptr<operand> _addr_x);
    check.expr(_addr_x, rclause.X); 

    // determine lhs, if any
    var sKey = rclause.Lhs; // possibly nil
    syntax.Expr sValue = default;
    {
        ptr<syntax.ListExpr> (p, _) = sKey._<ptr<syntax.ListExpr>>();

        if (p != null) {
            if (len(p.ElemList) != 2) {
                check.error(s, invalidAST + "invalid lhs in range clause");
                return ;
            }
            sKey = p.ElemList[0];
            sValue = p.ElemList[1];
        }
    } 

    // determine key/value types
    Type key = default;    Type val = default;

    if (x.mode != invalid) {
        var typ = optype(x.typ);
        {
            ptr<Chan> (_, ok) = typ._<ptr<Chan>>();

            if (ok && sValue != null) { 
                // TODO(gri) this also needs to happen for channels in generic variables
                check.softErrorf(sValue, "range over %s permits only one iteration variable", _addr_x); 
                // ok to continue
            }

        }
        @string msg = default;
        key, val, msg = rangeKeyVal(typ, isVarName(sKey), isVarName(sValue));
        if (key == null || msg != "") {
            if (msg != "") {
                msg = ": " + msg;
            }
            check.softErrorf(_addr_x, "cannot range over %s%s", _addr_x, msg); 
            // ok to continue
        }
    }
    array<syntax.Expr> lhs = new array<syntax.Expr>(new syntax.Expr[] { sKey, sValue });
    array<Type> rhs = new array<Type>(new Type[] { key, val }); // key, val may be nil

    if (rclause.Def) { 
        // short variable declaration; variable scope starts after the range clause
        // (the for loop opens a new scope, so variables on the lhs never redeclare
        // previously declared variables)
        slice<ptr<Var>> vars = default;
        {
            var i__prev1 = i;
            array<syntax.Expr> lhs__prev1 = lhs;

            foreach (var (__i, __lhs) in lhs) {
                i = __i;
                lhs = __lhs;
                if (lhs == null) {
                    continue;
                } 

                // determine lhs variable
                ptr<Var> obj;
                {
                    ptr<syntax.Name> (ident, _) = lhs._<ptr<syntax.Name>>();

                    if (ident != null) { 
                        // declare new variable
                        var name = ident.Value;
                        obj = NewVar(ident.Pos(), check.pkg, name, null);
                        check.recordDef(ident, obj); 
                        // _ variables don't count as new variables
                        if (name != "_") {
                            vars = append(vars, obj);
                        }
                    }
                    else
 {
                        check.errorf(lhs, "cannot declare %s", lhs);
                        obj = NewVar(lhs.Pos(), check.pkg, "_", null); // dummy variable
                    } 

                    // initialize lhs variable

                } 

                // initialize lhs variable
                {
                    var typ__prev2 = typ;

                    typ = rhs[i];

                    if (typ != null) {
                        x.mode = value;
                        x.expr = lhs; // we don't have a better rhs expression to use here
                        x.typ = typ;
                        check.initVar(obj, _addr_x, "range clause");
                    }
                    else
 {
                        obj.typ = Typ[Invalid];
                        obj.used = true; // don't complain about unused variable
                    }

                    typ = typ__prev2;

                }
            }
    else
 

            // declare variables

            i = i__prev1;
            lhs = lhs__prev1;
        }

        if (len(vars) > 0) {
            var scopePos = syntax.EndPos(rclause.X); // TODO(gri) should this just be s.Body.Pos (spec clarification)?
            {
                ptr<Var> obj__prev1 = obj;

                foreach (var (_, __obj) in vars) {
                    obj = __obj; 
                    // spec: "The scope of a constant or variable identifier declared inside
                    // a function begins at the end of the ConstSpec or VarSpec (ShortVarDecl
                    // for short variable declarations) and ends at the end of the innermost
                    // containing block."
                    check.declare(check.scope, null, obj, scopePos);
                }
        else

                obj = obj__prev1;
            }
        } {
            check.error(s, "no new variables on left side of :=");
        }
    } { 
        // ordinary assignment
        {
            var i__prev1 = i;
            array<syntax.Expr> lhs__prev1 = lhs;

            foreach (var (__i, __lhs) in lhs) {
                i = __i;
                lhs = __lhs;
                if (lhs == null) {
                    continue;
                }
                {
                    var typ__prev2 = typ;

                    typ = rhs[i];

                    if (typ != null) {
                        x.mode = value;
                        x.expr = lhs; // we don't have a better rhs expression to use here
                        x.typ = typ;
                        check.assignVar(lhs, _addr_x);
                    }

                    typ = typ__prev2;

                }
            }

            i = i__prev1;
            lhs = lhs__prev1;
        }
    }
    check.stmt(inner, s.Body);
}

// isVarName reports whether x is a non-nil, non-blank (_) expression.
private static bool isVarName(syntax.Expr x) {
    if (x == null) {
        return false;
    }
    ptr<syntax.Name> (ident, _) = unparen(x)._<ptr<syntax.Name>>();
    return ident == null || ident.Value != "_";
}

// rangeKeyVal returns the key and value type produced by a range clause
// over an expression of type typ, and possibly an error message. If the
// range clause is not permitted the returned key is nil or msg is not
// empty (in that case we still may have a non-nil key type which can be
// used to reduce the chance for follow-on errors).
// The wantKey, wantVal, and hasVal flags indicate which of the iteration
// variables are used or present; this matters if we range over a generic
// type where not all keys or values are of the same type.
private static (Type, Type, @string) rangeKeyVal(Type typ, bool wantKey, bool wantVal) {
    Type _p0 = default;
    Type _p0 = default;
    @string _p0 = default;

    switch (typ.type()) {
        case ptr<Basic> typ:
            if (isString(typ)) {
                return (Typ[Int], universeRune, ""); // use 'rune' name
            }
            break;
        case ptr<Array> typ:
            return (Typ[Int], typ.elem, "");
            break;
        case ptr<Slice> typ:
            return (Typ[Int], typ.elem, "");
            break;
        case ptr<Pointer> typ:
            {
                var typ__prev1 = typ;

                var typ = asArray(typ.@base);

                if (typ != null) {
                    return (Typ[Int], typ.elem, "");
                }

                typ = typ__prev1;

            }
            break;
        case ptr<Map> typ:
            return (typ.key, typ.elem, "");
            break;
        case ptr<Chan> typ:
            @string msg = default;
            if (typ.dir == SendOnly) {
                msg = "receive from send-only channel";
            }
            return (typ.elem, Typ[Invalid], msg);
            break;
        case ptr<Sum> typ:
            var first = true;
            Type key = default;            Type val = default;

            msg = default;
            typ.@is(t => {
                var (k, v, m) = rangeKeyVal(under(t), wantKey, wantVal);
                if (k == null || m != "") {
                    (key, val, msg) = (k, v, m);                    return false;
                }
                if (first) {
                    (key, val, msg) = (k, v, m);                    first = false;
                    return true;
                }
                if (wantKey && !Identical(key, k)) {
                    (key, val, msg) = (null, null, "all possible values must have the same key type");                    return false;
                }
                if (wantVal && !Identical(val, v)) {
                    (key, val, msg) = (null, null, "all possible values must have the same element type");                    return false;
                }
                return true;
            });
            return (key, val, msg);
            break;
    }
    return (null, null, "");
}

} // end types2_package
