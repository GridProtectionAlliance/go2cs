// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements typechecking of statements.

// package types -- go2cs converted at 2022 March 13 05:53:27 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\stmt.go
namespace go.go;

using ast = go.ast_package;
using constant = go.constant_package;
using token = go.token_package;
using sort = sort_package;
using System;

public static partial class types_package {

private static void funcBody(this ptr<Checker> _addr_check, ptr<declInfo> _addr_decl, @string name, ptr<Signature> _addr_sig, ptr<ast.BlockStmt> _addr_body, constant.Value iota) => func((defer, panic, _) => {
    ref Checker check = ref _addr_check.val;
    ref declInfo decl = ref _addr_decl.val;
    ref Signature sig = ref _addr_sig.val;
    ref ast.BlockStmt body = ref _addr_body.val;

    if (check.conf.IgnoreFuncBodies) {
        panic("internal error: function body not ignored");
    }
    if (trace) {
        check.trace(body.Pos(), "--- %s: %s", name, sig);
        defer(() => {
            check.trace(body.End(), "--- <end>");
        }());
    }
    sig.scope.pos = body.Pos();
    sig.scope.end = body.End(); 

    // save/restore current context and setup function context
    // (and use 0 indentation at function start)
    defer((ctxt, indent) => {
        check.context = ctxt;
        check.indent = indent;
    }(check.context, check.indent));
    check.context = new context(decl:decl,scope:sig.scope,iota:iota,sig:sig,);
    check.indent = 0;

    check.stmtList(0, body.List);

    if (check.hasLabel) {
        check.labels(body);
    }
    if (sig.results.Len() > 0 && !check.isTerminating(body, "")) {
        check.error(atPos(body.Rbrace), _MissingReturn, "missing return");
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
    }    sort.Slice(unused, (i, j) => unused[i].pos < unused[j].pos);
    {
        ptr<Var> v__prev1 = v;

        foreach (var (_, __v) in unused) {
            v = __v;
            check.softErrorf(v, _UnusedVar, "%s declared but not used", v.name);
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

private static void simpleStmt(this ptr<Checker> _addr_check, ast.Stmt s) {
    ref Checker check = ref _addr_check.val;

    if (s != null) {
        check.stmt(0, s);
    }
}

private static slice<ast.Stmt> trimTrailingEmptyStmts(slice<ast.Stmt> list) {
    for (var i = len(list); i > 0; i--) {
        {
            ptr<ast.EmptyStmt> (_, ok) = list[i - 1]._<ptr<ast.EmptyStmt>>();

            if (!ok) {
                return list[..(int)i];
            }

        }
    }
    return null;
}

private static void stmtList(this ptr<Checker> _addr_check, stmtContext ctxt, slice<ast.Stmt> list) {
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

private static void multipleDefaults(this ptr<Checker> _addr_check, slice<ast.Stmt> list) {
    ref Checker check = ref _addr_check.val;

    ast.Stmt first = default;
    foreach (var (_, s) in list) {
        ast.Stmt d = default;
        switch (s.type()) {
            case ptr<ast.CaseClause> c:
                if (len(c.List) == 0) {
                    d = s;
                }
                break;
            case ptr<ast.CommClause> c:
                if (c.Comm == null) {
                    d = s;
                }
                break;
            default:
            {
                var c = s.type();
                check.invalidAST(s, "case/communication clause expected");
                break;
            }
        }
        if (d != null) {
            if (first != null) {
                check.errorf(d, _DuplicateDefault, "multiple defaults (first at %s)", check.fset.Position(first.Pos()));
            }
            else
 {
                first = d;
            }
        }
    }
}

private static void openScope(this ptr<Checker> _addr_check, ast.Node node, @string comment) {
    ref Checker check = ref _addr_check.val;

    var scope = NewScope(check.scope, node.Pos(), node.End(), comment);
    check.recordScope(node, scope);
    check.scope = scope;
}

private static void closeScope(this ptr<Checker> _addr_check) {
    ref Checker check = ref _addr_check.val;

    check.scope = check.scope.Parent();
}

private static token.Token assignOp(token.Token op) { 
    // token_test.go verifies the token ordering this function relies on
    if (token.ADD_ASSIGN <= op && op <= token.AND_NOT_ASSIGN) {
        return op + (token.ADD - token.ADD_ASSIGN);
    }
    return token.ILLEGAL;
}

private static void suspendedCall(this ptr<Checker> _addr_check, @string keyword, ptr<ast.CallExpr> _addr_call) {
    ref Checker check = ref _addr_check.val;
    ref ast.CallExpr call = ref _addr_call.val;

    ref operand x = ref heap(out ptr<operand> _addr_x);
    @string msg = default;
    errorCode code = default;

    if (check.rawExpr(_addr_x, call, null) == conversion) 
        msg = "requires function call, not conversion";
        code = _InvalidDefer;
        if (keyword == "go") {
            code = _InvalidGo;
        }
    else if (check.rawExpr(_addr_x, call, null) == expression) 
        msg = "discards result of";
        code = _UnusedResults;
    else if (check.rawExpr(_addr_x, call, null) == statement) 
        return ;
    else 
        unreachable();
        check.errorf(_addr_x, code, "%s %s %s", keyword, msg, _addr_x);
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
    public token.Pos pos;
    public Type typ;
}private static void caseValues(this ptr<Checker> _addr_check, ptr<operand> _addr_x, slice<ast.Expr> values, valueMap seen) {
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
        check.comparison(_addr_res, x, token.EQL);
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
                        check.errorf(_addr_v, _DuplicateCase, "duplicate case %s in expression switch", _addr_v);
                        check.error(atPos(vt.pos), _DuplicateCase, "\tprevious case"); // secondary error, \t indented
                        _continueL = true;
                        break;
                    }
                }
                seen[val] = append(seen[val], new valueType(v.Pos(),v.typ));
            }

        }
    }
}

private static Type caseTypes(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<Interface> _addr_xtyp, slice<ast.Expr> types, map<Type, ast.Expr> seen) {
    Type T = default;
    ref Checker check = ref _addr_check.val;
    ref operand x = ref _addr_x.val;
    ref Interface xtyp = ref _addr_xtyp.val;

L:
    foreach (var (_, e) in types) {
        T = check.typeOrNil(e);
        if (T == Typ[Invalid]) {
            _continueL = true;
            break;
        }
        if (T != null) {
            check.ordinaryType(e, T);
        }
        foreach (var (t, other) in seen) {
            if (T == null && t == null || T != null && t != null && check.identical(T, t)) { 
                // talk about "case" rather than "type" because of nil case
                @string Ts = "nil";
                if (T != null) {
                    Ts = T.String();
                }
                check.errorf(e, _DuplicateCase, "duplicate case %s in type switch", Ts);
                check.error(other, _DuplicateCase, "\tprevious case"); // secondary error, \t indented
                _continueL = true;
                break;
            }
        }        seen[T] = e;
        if (T != null) {
            check.typeAssertion(e, x, xtyp, T);
        }
    }    return ;
}

// stmt typechecks statement s.
private static void stmt(this ptr<Checker> _addr_check, stmtContext ctxt, ast.Stmt s) => func((defer, panic, _) => {
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
        case ptr<ast.BadStmt> s:
            break;
        case ptr<ast.EmptyStmt> s:
            break;
        case ptr<ast.DeclStmt> s:
            check.declStmt(s.Decl);
            break;
        case ptr<ast.LabeledStmt> s:
            check.hasLabel = true;
            check.stmt(ctxt, s.Stmt);
            break;
        case ptr<ast.ExprStmt> s:
            ref operand x = ref heap(out ptr<operand> _addr_x);
            var kind = check.rawExpr(_addr_x, s.X, null);
            @string msg = default;
            errorCode code = default;

            if (x.mode == builtin) 
                msg = "must be called";
                code = _UncalledBuiltin;
            else if (x.mode == typexpr) 
                msg = "is not an expression";
                code = _NotAnExpr;
            else 
                if (kind == statement) {
                    return ;
                }
                msg = "is not used";
                code = _UnusedExpr;
                        check.errorf(_addr_x, code, "%s %s", _addr_x, msg);
            break;
        case ptr<ast.SendStmt> s:
            ref operand ch = ref heap(out ptr<operand> _addr_ch);            x = default;

            check.expr(_addr_ch, s.Chan);
            check.expr(_addr_x, s.Value);
            if (ch.mode == invalid || x.mode == invalid) {
                return ;
            }
            var tch = asChan(ch.typ);
            if (tch == null) {
                check.invalidOp(inNode(s, s.Arrow), _InvalidSend, "cannot send to non-chan type %s", ch.typ);
                return ;
            }
            if (tch.dir == RecvOnly) {
                check.invalidOp(inNode(s, s.Arrow), _InvalidSend, "cannot send to receive-only type %s", tch);
                return ;
            }
            check.assignment(_addr_x, tch.elem, "send");
            break;
        case ptr<ast.IncDecStmt> s:
            token.Token op = default;

            if (s.Tok == token.INC) 
                op = token.ADD;
            else if (s.Tok == token.DEC) 
                op = token.SUB;
            else 
                check.invalidAST(inNode(s, s.TokPos), "unknown inc/dec operation %s", s.Tok);
                return ;
                        x = default;
            check.expr(_addr_x, s.X);
            if (x.mode == invalid) {
                return ;
            }
            if (!isNumeric(x.typ)) {
                check.invalidOp(s.X, _NonNumericIncDec, "%s%s (non-numeric type %s)", s.X, s.Tok, x.typ);
                return ;
            }
            ptr<ast.BasicLit> Y = addr(new ast.BasicLit(ValuePos:s.X.Pos(),Kind:token.INT,Value:"1")); // use x's position
            check.binary(_addr_x, null, s.X, Y, op, s.TokPos);
            if (x.mode == invalid) {
                return ;
            }
            check.assignVar(s.X, _addr_x);
            break;
        case ptr<ast.AssignStmt> s:

            if (s.Tok == token.ASSIGN || s.Tok == token.DEFINE) 
                if (len(s.Lhs) == 0) {
                    check.invalidAST(s, "missing lhs in assignment");
                    return ;
                }
                if (s.Tok == token.DEFINE) {
                    check.shortVarDecl(inNode(s, s.TokPos), s.Lhs, s.Rhs);
                }
                else
 { 
                    // regular assignment
                    check.assignVars(s.Lhs, s.Rhs);
                }
            else 
                // assignment operations
                if (len(s.Lhs) != 1 || len(s.Rhs) != 1) {
                    check.errorf(inNode(s, s.TokPos), _MultiValAssignOp, "assignment operation %s requires single-valued expressions", s.Tok);
                    return ;
                }
                op = assignOp(s.Tok);
                if (op == token.ILLEGAL) {
                    check.invalidAST(atPos(s.TokPos), "unknown assignment operation %s", s.Tok);
                    return ;
                }
                x = default;
                check.binary(_addr_x, null, s.Lhs[0], s.Rhs[0], op, s.TokPos);
                if (x.mode == invalid) {
                    return ;
                }
                check.assignVar(s.Lhs[0], _addr_x);
                        break;
        case ptr<ast.GoStmt> s:
            check.suspendedCall("go", s.Call);
            break;
        case ptr<ast.DeferStmt> s:
            check.suspendedCall("defer", s.Call);
            break;
        case ptr<ast.ReturnStmt> s:
            var res = check.sig.results;
            if (res.Len() > 0) { 
                // function returns results
                // (if one, say the first, result parameter is named, all of them are named)
                if (len(s.Results) == 0 && res.vars[0].name != "") { 
                    // spec: "Implementation restriction: A compiler may disallow an empty expression
                    // list in a "return" statement if a different entity (constant, type, or variable)
                    // with the same name as a result parameter is in scope at the place of the return."
                    {
                        var obj__prev1 = obj;

                        foreach (var (_, __obj) in res.vars) {
                            obj = __obj;
                            {
                                var alt = check.lookup(obj.name);

                                if (alt != null && alt != obj) {
                                    check.errorf(s, _OutOfScopeResult, "result parameter %s not in scope at return", obj.name);
                                    check.errorf(alt, _OutOfScopeResult, "\tinner declaration of %s", obj); 
                                    // ok to continue
                                }

                            }
                        }
                else

                        obj = obj__prev1;
                    }
                } { 
                    // return has results or result parameters are unnamed
                    check.initVars(res.vars, s.Results, s.Return);
                }
            }
            else if (len(s.Results) > 0) {
                check.error(s.Results[0], _WrongResultCount, "no result values expected");
                check.use(s.Results);
            }
            break;
        case ptr<ast.BranchStmt> s:
            if (s.Label != null) {
                check.hasLabel = true;
                return ; // checked in 2nd pass (check.labels)
            }

            if (s.Tok == token.BREAK) 
                if (ctxt & breakOk == 0) {
                    check.error(s, _MisplacedBreak, "break not in for, switch, or select statement");
                }
            else if (s.Tok == token.CONTINUE) 
                if (ctxt & continueOk == 0) {
                    check.error(s, _MisplacedContinue, "continue not in for statement");
                }
            else if (s.Tok == token.FALLTHROUGH) 
                if (ctxt & fallthroughOk == 0) {
                    msg = "fallthrough statement out of place";
                    code = _MisplacedFallthrough;
                    if (ctxt & finalSwitchCase != 0) {
                        msg = "cannot fallthrough final case in switch";
                    }
                    check.error(s, code, msg);
                }
            else 
                check.invalidAST(s, "branch statement: %s", s.Tok);
                        break;
        case ptr<ast.BlockStmt> s:
            check.openScope(s, "block");
            defer(check.closeScope());

            check.stmtList(inner, s.List);
            break;
        case ptr<ast.IfStmt> s:
            check.openScope(s, "if");
            defer(check.closeScope());

            check.simpleStmt(s.Init);
            x = default;
            check.expr(_addr_x, s.Cond);
            if (x.mode != invalid && !isBoolean(x.typ)) {
                check.error(s.Cond, _InvalidCond, "non-boolean condition in if statement");
            }
            check.stmt(inner, s.Body); 
            // The parser produces a correct AST but if it was modified
            // elsewhere the else branch may be invalid. Check again.
            switch (s.Else.type()) {
                case ptr<ast.BadStmt> _:
                    break;
                case ptr<ast.IfStmt> _:
                    check.stmt(inner, s.Else);
                    break;
                case ptr<ast.BlockStmt> _:
                    check.stmt(inner, s.Else);
                    break;
                default:
                {
                    check.invalidAST(s.Else, "invalid else branch in if statement");
                    break;
                }

            }
            break;
        case ptr<ast.SwitchStmt> s:
            inner |= breakOk;
            check.openScope(s, "switch");
            defer(check.closeScope());

            check.simpleStmt(s.Init);
            x = default;
            if (s.Tag != null) {
                check.expr(_addr_x, s.Tag); 
                // By checking assignment of x to an invisible temporary
                // (as a compiler would), we get all the relevant checks.
                check.assignment(_addr_x, null, "switch expression");
                if (x.mode != invalid && !Comparable(x.typ) && !hasNil(x.typ)) {
                    check.errorf(_addr_x, _InvalidExprSwitch, "cannot switch on %s (%s is not comparable)", _addr_x, x.typ);
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
                x.expr = addr(new ast.Ident(NamePos:s.Body.Lbrace,Name:"true"));
            }
            check.multipleDefaults(s.Body.List);

            var seen = make(valueMap); // map of seen case values to positions and types
            {
                var i__prev1 = i;

                foreach (var (__i, __c) in s.Body.List) {
                    i = __i;
                    c = __c;
                    ptr<ast.CaseClause> (clause, _) = c._<ptr<ast.CaseClause>>();
                    if (clause == null) {
                        check.invalidAST(c, "incorrect expression switch case");
                        continue;
                    }
                    check.caseValues(_addr_x, clause.List, seen);
                    check.openScope(clause, "case");
                    inner = inner;
                    if (i + 1 < len(s.Body.List)) {
                        inner |= fallthroughOk;
                    }
                    else
 {
                        inner |= finalSwitchCase;
                    }
                    check.stmtList(inner, clause.Body);
                    check.closeScope();
                }

                i = i__prev1;
            }
            break;
        case ptr<ast.TypeSwitchStmt> s:
            inner |= breakOk;
            check.openScope(s, "type switch");
            defer(check.closeScope());

            check.simpleStmt(s.Init); 

            // A type switch guard must be of the form:
            //
            //     TypeSwitchGuard = [ identifier ":=" ] PrimaryExpr "." "(" "type" ")" .
            //
            // The parser is checking syntactic correctness;
            // remaining syntactic errors are considered AST errors here.
            // TODO(gri) better factoring of error handling (invalid ASTs)
            //
            ptr<ast.Ident> lhs; // lhs identifier or nil
            ast.Expr rhs = default;
            switch (s.Assign.type()) {
                case ptr<ast.ExprStmt> guard:
                    rhs = guard.X;
                    break;
                case ptr<ast.AssignStmt> guard:
                    if (len(guard.Lhs) != 1 || guard.Tok != token.DEFINE || len(guard.Rhs) != 1) {
                        check.invalidAST(s, "incorrect form of type switch guard");
                        return ;
                    }
                    lhs, _ = guard.Lhs[0]._<ptr<ast.Ident>>();
                    if (lhs == null) {
                        check.invalidAST(s, "incorrect form of type switch guard");
                        return ;
                    }
                    if (lhs.Name == "_") { 
                        // _ := x.(type) is an invalid short variable declaration
                        check.softErrorf(lhs, _NoNewVar, "no new variable on left side of :=");
                        lhs = null; // avoid declared but not used error below
                    }
                    else
 {
                        check.recordDef(lhs, null); // lhs variable is implicitly declared in each cause clause
                    }
                    rhs = guard.Rhs[0];
                    break;
                default:
                {
                    var guard = s.Assign.type();
                    check.invalidAST(s, "incorrect form of type switch guard");
                    return ;
                    break;
                } 

                // rhs must be of the form: expr.(type) and expr must be an ordinary interface
            } 

            // rhs must be of the form: expr.(type) and expr must be an ordinary interface
            ptr<ast.TypeAssertExpr> (expr, _) = rhs._<ptr<ast.TypeAssertExpr>>();
            if (expr == null || expr.Type != null) {
                check.invalidAST(s, "incorrect form of type switch guard");
                return ;
            }
            x = default;
            check.expr(_addr_x, expr.X);
            if (x.mode == invalid) {
                return ;
            }
            ptr<Interface> (xtyp, _) = under(x.typ)._<ptr<Interface>>();
            if (xtyp == null) {
                check.errorf(_addr_x, _InvalidTypeSwitch, "%s is not an interface", _addr_x);
                return ;
            }
            check.ordinaryType(_addr_x, xtyp);

            check.multipleDefaults(s.Body.List);

            slice<ptr<Var>> lhsVars = default; // list of implicitly declared lhs variables
            seen = make_map<Type, ast.Expr>(); // map of seen types to positions
            {
                var s__prev1 = s;

                foreach (var (_, __s) in s.Body.List) {
                    s = __s;
                    (clause, _) = s._<ptr<ast.CaseClause>>();
                    if (clause == null) {
                        check.invalidAST(s, "incorrect type switch case");
                        continue;
                    } 
                    // Check each type in this type switch case.
                    var T = check.caseTypes(_addr_x, xtyp, clause.List, seen);
                    check.openScope(clause, "case"); 
                    // If lhs exists, declare a corresponding variable in the case-local scope.
                    if (lhs != null) { 
                        // spec: "The TypeSwitchGuard may include a short variable declaration.
                        // When that form is used, the variable is declared at the beginning of
                        // the implicit block in each clause. In clauses with a case listing
                        // exactly one type, the variable has that type; otherwise, the variable
                        // has the type of the expression in the TypeSwitchGuard."
                        if (len(clause.List) != 1 || T == null) {
                            T = x.typ;
                        }
                        var obj = NewVar(lhs.Pos(), check.pkg, lhs.Name, T);
                        var scopePos = clause.Pos() + token.Pos(len("default")); // for default clause (len(List) == 0)
                        {
                            var n = len(clause.List);

                            if (n > 0) {
                                scopePos = clause.List[n - 1].End();
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
                } 

                // If lhs exists, we must have at least one lhs variable that was used.

                s = s__prev1;
            }

            if (lhs != null) {
                bool used = default;
                foreach (var (_, v) in lhsVars) {
                    if (v.used) {
                        used = true;
                    }
                    v.used = true; // avoid usage error when checking entire function
                }
                if (!used) {
                    check.softErrorf(lhs, _UnusedVar, "%s declared but not used", lhs.Name);
                }
            }
            break;
        case ptr<ast.SelectStmt> s:
            inner |= breakOk;

            check.multipleDefaults(s.Body.List);

            {
                var s__prev1 = s;

                foreach (var (_, __s) in s.Body.List) {
                    s = __s;
                    (clause, _) = s._<ptr<ast.CommClause>>();
                    if (clause == null) {
                        continue; // error reported before
                    } 

                    // clause.Comm must be a SendStmt, RecvStmt, or default case
                    var valid = false;
                    rhs = default; // rhs of RecvStmt, or nil
                    switch (clause.Comm.type()) {
                        case ptr<ast.SendStmt> s:
                            valid = true;
                            break;
                        case ptr<ast.AssignStmt> s:
                            if (len(s.Rhs) == 1) {
                                rhs = s.Rhs[0];
                            }
                            break;
                        case ptr<ast.ExprStmt> s:
                            rhs = s.X;
                            break; 

                        // if present, rhs must be a receive operation
                    } 

                    // if present, rhs must be a receive operation
                    if (rhs != null) {
                        {
                            operand x__prev2 = x;

                            ptr<ast.UnaryExpr> (x, _) = unparen(rhs)._<ptr<ast.UnaryExpr>>();

                            if (x != null && x.Op == token.ARROW) {
                                valid = true;
                            }

                            x = x__prev2;

                        }
                    }
                    if (!valid) {
                        check.error(clause.Comm, _InvalidSelectCase, "select case must be send or receive (possibly with assignment)");
                        continue;
                    }
                    check.openScope(s, "case");
                    if (clause.Comm != null) {
                        check.stmt(inner, clause.Comm);
                    }
                    check.stmtList(inner, clause.Body);
                    check.closeScope();
                }

                s = s__prev1;
            }
            break;
        case ptr<ast.ForStmt> s:
            inner |= breakOk | continueOk;
            check.openScope(s, "for");
            defer(check.closeScope());

            check.simpleStmt(s.Init);
            if (s.Cond != null) {
                x = default;
                check.expr(_addr_x, s.Cond);
                if (x.mode != invalid && !isBoolean(x.typ)) {
                    check.error(s.Cond, _InvalidCond, "non-boolean condition in for statement");
                }
            }
            check.simpleStmt(s.Post); 
            // spec: "The init statement may be a short variable
            // declaration, but the post statement must not."
            {
                var s__prev1 = s;

                ptr<ast.AssignStmt> (s, _) = s.Post._<ptr<ast.AssignStmt>>();

                if (s != null && s.Tok == token.DEFINE) {
                    check.softErrorf(s, _InvalidPostDecl, "cannot declare in post statement"); 
                    // Don't call useLHS here because we want to use the lhs in
                    // this erroneous statement so that we don't get errors about
                    // these lhs variables being declared but not used.
                    check.use(s.Lhs); // avoid follow-up errors
                }

                s = s__prev1;

            }
            check.stmt(inner, s.Body);
            break;
        case ptr<ast.RangeStmt> s:
            inner |= breakOk | continueOk;
            check.openScope(s, "for");
            defer(check.closeScope()); 

            // check expression to iterate over
            x = default;
            check.expr(_addr_x, s.X); 

            // determine key/value types
            Type key = default;            Type val = default;

            if (x.mode != invalid) {
                var typ = optype(x.typ);
                {
                    ptr<Chan> (_, ok) = typ._<ptr<Chan>>();

                    if (ok && s.Value != null) { 
                        // TODO(gri) this also needs to happen for channels in generic variables
                        check.softErrorf(atPos(s.Value.Pos()), _InvalidIterVar, "range over %s permits only one iteration variable", _addr_x); 
                        // ok to continue
                    }

                }
                msg = default;
                key, val, msg = rangeKeyVal(typ, isVarName(s.Key), isVarName(s.Value));
                if (key == null || msg != "") {
                    if (msg != "") { 
                        // TODO(rFindley) should this be parenthesized, to be consistent with other qualifiers?
                        msg = ": " + msg;
                    }
                    check.softErrorf(_addr_x, _InvalidRangeExpr, "cannot range over %s%s", _addr_x, msg); 
                    // ok to continue
                }
            } 

            // check assignment to/declaration of iteration variables
            // (irregular assignment, cannot easily map to existing assignment checks)

            // lhs expressions and initialization value (rhs) types
            lhs = new array<ast.Expr>(new ast.Expr[] { s.Key, s.Value });
            rhs = new array<Type>(new Type[] { key, val }); // key, val may be nil

            if (s.Tok == token.DEFINE) { 
                // short variable declaration; variable scope starts after the range clause
                // (the for loop opens a new scope, so variables on the lhs never redeclare
                // previously declared variables)
                slice<ptr<Var>> vars = default;
                {
                    var i__prev1 = i;
                    ptr<ast.Ident> lhs__prev1 = lhs;

                    foreach (var (__i, __lhs) in lhs) {
                        i = __i;
                        lhs = __lhs;
                        if (lhs == null) {
                            continue;
                        } 

                        // determine lhs variable
                        obj = ;
                        {
                            ptr<ast.Ident> (ident, _) = lhs._<ptr<ast.Ident>>();

                            if (ident != null) { 
                                // declare new variable
                                var name = ident.Name;
                                obj = NewVar(ident.Pos(), check.pkg, name, null);
                                check.recordDef(ident, obj); 
                                // _ variables don't count as new variables
                                if (name != "_") {
                                    vars = append(vars, obj);
                                }
                            }
                            else
 {
                                check.invalidAST(lhs, "cannot declare %s", lhs);
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
                    scopePos = s.X.End();
                    {
                        var obj__prev1 = obj;

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
                    check.error(inNode(s, s.TokPos), _NoNewVar, "no new variables on left side of :=");
                }
            } { 
                // ordinary assignment
                {
                    var i__prev1 = i;
                    ptr<ast.Ident> lhs__prev1 = lhs;

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
            break;
        default:
        {
            var s = s.type();
            check.invalidAST(s, "invalid statement");
            break;
        }
    }
});

// isVarName reports whether x is a non-nil, non-blank (_) expression.
private static bool isVarName(ast.Expr x) {
    if (x == null) {
        return false;
    }
    ptr<ast.Ident> (ident, _) = unparen(x)._<ptr<ast.Ident>>();
    return ident == null || ident.Name != "_";
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
                msg = "send-only channel";
            }
            return (typ.elem, Typ[Invalid], msg);
            break;
        case ptr<_Sum> typ:
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

} // end types_package
