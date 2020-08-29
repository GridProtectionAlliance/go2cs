// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements typechecking of statements.

// package types -- go2cs converted at 2020 August 29 08:48:00 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\stmt.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class types_package
    {
        private static void funcBody(this ref Checker _check, ref declInfo _decl, @string name, ref Signature _sig, ref ast.BlockStmt _body) => func(_check, _decl, _sig, _body, (ref Checker check, ref declInfo decl, ref Signature sig, ref ast.BlockStmt body, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                if (name == "")
                {
                    name = "<function literal>";
                }
                fmt.Printf("--- %s: %s {\n", name, sig);
                defer(fmt.Println("--- <end>"));
            }
            sig.scope.pos = body.Pos();
            sig.scope.end = body.End(); 

            // save/restore current context and setup function context
            // (and use 0 indentation at function start)
            defer((ctxt, indent) =>
            {
                check.context = ctxt;
                check.indent = indent;
            }(check.context, check.indent));
            check.context = new context(decl:decl,scope:sig.scope,sig:sig,);
            check.indent = 0L;

            check.stmtList(0L, body.List);

            if (check.hasLabel)
            {
                check.labels(body);
            }
            if (sig.results.Len() > 0L && !check.isTerminating(body, ""))
            {
                check.error(body.Rbrace, "missing return");
            }
            check.usage(sig.scope);
        });

        private static void usage(this ref Checker check, ref Scope scope)
        {
            slice<ref Var> unused = default;
            foreach (var (_, elem) in scope.elems)
            {
                {
                    ref Var v__prev1 = v;

                    ref Var (v, _) = elem._<ref Var>();

                    if (v != null && !v.used)
                    {
                        unused = append(unused, v);
                    }

                    v = v__prev1;

                }
            }
            sort.Slice(unused, (i, j) =>
            {
                return unused[i].pos < unused[j].pos;
            });
            {
                ref Var v__prev1 = v;

                foreach (var (_, __v) in unused)
                {
                    v = __v;
                    check.softErrorf(v.pos, "%s declared but not used", v.name);
                }

                v = v__prev1;
            }

            foreach (var (_, scope) in scope.children)
            { 
                // Don't go inside closure scopes a second time;
                // they are handled explicitly by funcBody.
                if (!scope.isFunc)
                {
                    check.usage(scope);
                }
            }
        }

        // stmtContext is a bitset describing which
        // control-flow statements are permissible,
        // and provides additional context information
        // for better error messages.
        private partial struct stmtContext // : ulong
        {
        }

 
        // permissible control-flow statements
        private static readonly stmtContext breakOk = 1L << (int)(iota);
        private static readonly var continueOk = 0;
        private static readonly var fallthroughOk = 1; 

        // additional context information
        private static readonly var finalSwitchCase = 2;

        private static void simpleStmt(this ref Checker check, ast.Stmt s)
        {
            if (s != null)
            {
                check.stmt(0L, s);
            }
        }

        private static slice<ast.Stmt> trimTrailingEmptyStmts(slice<ast.Stmt> list)
        {
            for (var i = len(list); i > 0L; i--)
            {
                {
                    ref ast.EmptyStmt (_, ok) = list[i - 1L]._<ref ast.EmptyStmt>();

                    if (!ok)
                    {
                        return list[..i];
                    }

                }
            }

            return null;
        }

        private static void stmtList(this ref Checker check, stmtContext ctxt, slice<ast.Stmt> list)
        {
            var ok = ctxt & fallthroughOk != 0L;
            var inner = ctxt & ~fallthroughOk;
            list = trimTrailingEmptyStmts(list); // trailing empty statements are "invisible" to fallthrough analysis
            foreach (var (i, s) in list)
            {
                inner = inner;
                if (ok && i + 1L == len(list))
                {
                    inner |= fallthroughOk;
                }
                check.stmt(inner, s);
            }
        }

        private static void multipleDefaults(this ref Checker check, slice<ast.Stmt> list)
        {
            ast.Stmt first = default;
            foreach (var (_, s) in list)
            {
                ast.Stmt d = default;
                switch (s.type())
                {
                    case ref ast.CaseClause c:
                        if (len(c.List) == 0L)
                        {
                            d = s;
                        }
                        break;
                    case ref ast.CommClause c:
                        if (c.Comm == null)
                        {
                            d = s;
                        }
                        break;
                    default:
                    {
                        var c = s.type();
                        check.invalidAST(s.Pos(), "case/communication clause expected");
                        break;
                    }
                }
                if (d != null)
                {
                    if (first != null)
                    {
                        check.errorf(d.Pos(), "multiple defaults (first at %s)", check.fset.Position(first.Pos()));
                    }
                    else
                    {
                        first = d;
                    }
                }
            }
        }

        private static void openScope(this ref Checker check, ast.Stmt s, @string comment)
        {
            var scope = NewScope(check.scope, s.Pos(), s.End(), comment);
            check.recordScope(s, scope);
            check.scope = scope;
        }

        private static void closeScope(this ref Checker check)
        {
            check.scope = check.scope.Parent();
        }

        private static token.Token assignOp(token.Token op)
        { 
            // token_test.go verifies the token ordering this function relies on
            if (token.ADD_ASSIGN <= op && op <= token.AND_NOT_ASSIGN)
            {
                return op + (token.ADD - token.ADD_ASSIGN);
            }
            return token.ILLEGAL;
        }

        private static void suspendedCall(this ref Checker check, @string keyword, ref ast.CallExpr call)
        {
            operand x = default;
            @string msg = default;

            if (check.rawExpr(ref x, call, null) == conversion) 
                msg = "requires function call, not conversion";
            else if (check.rawExpr(ref x, call, null) == expression) 
                msg = "discards result of";
            else if (check.rawExpr(ref x, call, null) == statement) 
                return;
            else 
                unreachable();
                        check.errorf(x.pos(), "%s %s %s", keyword, msg, ref x);
        }

        // goVal returns the Go value for val, or nil.
        private static void goVal(constant.Value val)
        { 
            // val should exist, but be conservative and check
            if (val == null)
            {
                return null;
            } 
            // Match implementation restriction of other compilers.
            // gc only checks duplicates for integer, floating-point
            // and string values, so only create Go values for these
            // types.

            if (val.Kind() == constant.Int) 
                {
                    var x__prev1 = x;

                    var (x, ok) = constant.Int64Val(val);

                    if (ok)
                    {
                        return x;
                    }

                    x = x__prev1;

                }
                {
                    var x__prev1 = x;

                    (x, ok) = constant.Uint64Val(val);

                    if (ok)
                    {
                        return x;
                    }

                    x = x__prev1;

                }
            else if (val.Kind() == constant.Float) 
                {
                    var x__prev1 = x;

                    (x, ok) = constant.Float64Val(val);

                    if (ok)
                    {
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
        private partial struct valueType
        {
            public token.Pos pos;
            public Type typ;
        }        private static void caseValues(this ref Checker check, ref operand x, slice<ast.Expr> values, valueMap seen)
        {
L:
            foreach (var (_, e) in values)
            {
                operand v = default;
                check.expr(ref v, e);
                if (x.mode == invalid || v.mode == invalid)
                {
                    _continueL = true;
                    break;
                }
                check.convertUntyped(ref v, x.typ);
                if (v.mode == invalid)
                {
                    _continueL = true;
                    break;
                } 
                // Order matters: By comparing v against x, error positions are at the case values.
                var res = v; // keep original v unchanged
                check.comparison(ref res, x, token.EQL);
                if (res.mode == invalid)
                {
                    _continueL = true;
                    break;
                }
                if (v.mode != constant_)
                {
                    _continueL = true; // we're done
                    break;
                } 
                // look for duplicate values
                {
                    var val = goVal(v.val);

                    if (val != null)
                    { 
                        // look for duplicate types for a given value
                        // (quadratic algorithm, but these lists tend to be very short)
                        foreach (var (_, vt) in seen[val])
                        {
                            if (Identical(v.typ, vt.typ))
                            {
                                check.errorf(v.pos(), "duplicate case %s in expression switch", ref v);
                                check.error(vt.pos, "\tprevious case"); // secondary error, \t indented
                                _continueL = true;
                                break;
                            }
                        }
                        seen[val] = append(seen[val], new valueType(v.pos(),v.typ));
                    }

                }
            }
        }

        private static Type caseTypes(this ref Checker check, ref operand x, ref Interface xtyp, slice<ast.Expr> types, map<Type, token.Pos> seen)
        {
L:
            foreach (var (_, e) in types)
            {
                T = check.typOrNil(e);
                if (T == Typ[Invalid])
                {
                    _continueL = true;
                    break;
                } 
                // look for duplicate types
                // (quadratic algorithm, but type switches tend to be reasonably small)
                foreach (var (t, pos) in seen)
                {
                    if (T == null && t == null || T != null && t != null && Identical(T, t))
                    { 
                        // talk about "case" rather than "type" because of nil case
                        @string Ts = "nil";
                        if (T != null)
                        {
                            Ts = T.String();
                        }
                        check.errorf(e.Pos(), "duplicate case %s in type switch", Ts);
                        check.error(pos, "\tprevious case"); // secondary error, \t indented
                        _continueL = true;
                        break;
                    }
                }
                seen[T] = e.Pos();
                if (T != null)
                {
                    check.typeAssertion(e.Pos(), x, xtyp, T);
                }
            }
            return;
        }

        // stmt typechecks statement s.
        private static void stmt(this ref Checker _check, stmtContext ctxt, ast.Stmt s) => func(_check, (ref Checker check, Defer defer, Panic panic, Recover _) =>
        { 
            // statements cannot use iota in general
            // (constant declarations set it explicitly)
            assert(check.iota == null); 

            // statements must end with the same top scope as they started with
            if (debug)
            {
                defer(scope =>
                { 
                    // don't check if code is panicking
                    {
                        var p = recover();

                        if (p != null)
                        {
                            panic(p);
                        }

                    }
                    assert(scope == check.scope);
                }(check.scope));
            }
            var inner = ctxt & ~(fallthroughOk | finalSwitchCase);
            switch (s.type())
            {
                case ref ast.BadStmt s:
                    break;
                case ref ast.EmptyStmt s:
                    break;
                case ref ast.DeclStmt s:
                    check.declStmt(s.Decl);
                    break;
                case ref ast.LabeledStmt s:
                    check.hasLabel = true;
                    check.stmt(ctxt, s.Stmt);
                    break;
                case ref ast.ExprStmt s:
                    operand x = default;
                    var kind = check.rawExpr(ref x, s.X, null);
                    @string msg = default;

                    if (x.mode == builtin) 
                        msg = "must be called";
                    else if (x.mode == typexpr) 
                        msg = "is not an expression";
                    else 
                        if (kind == statement)
                        {
                            return;
                        }
                        msg = "is not used";
                                        check.errorf(x.pos(), "%s %s", ref x, msg);
                    break;
                case ref ast.SendStmt s:
                    operand ch = default;                    x = default;

                    check.expr(ref ch, s.Chan);
                    check.expr(ref x, s.Value);
                    if (ch.mode == invalid || x.mode == invalid)
                    {
                        return;
                    }
                    ref Chan (tch, ok) = ch.typ.Underlying()._<ref Chan>();
                    if (!ok)
                    {
                        check.invalidOp(s.Arrow, "cannot send to non-chan type %s", ch.typ);
                        return;
                    }
                    if (tch.dir == RecvOnly)
                    {
                        check.invalidOp(s.Arrow, "cannot send to receive-only type %s", tch);
                        return;
                    }
                    check.assignment(ref x, tch.elem, "send");
                    break;
                case ref ast.IncDecStmt s:
                    token.Token op = default;

                    if (s.Tok == token.INC) 
                        op = token.ADD;
                    else if (s.Tok == token.DEC) 
                        op = token.SUB;
                    else 
                        check.invalidAST(s.TokPos, "unknown inc/dec operation %s", s.Tok);
                        return;
                                        x = default;
                    check.expr(ref x, s.X);
                    if (x.mode == invalid)
                    {
                        return;
                    }
                    if (!isNumeric(x.typ))
                    {
                        check.invalidOp(s.X.Pos(), "%s%s (non-numeric type %s)", s.X, s.Tok, x.typ);
                        return;
                    }
                    ast.BasicLit Y = ref new ast.BasicLit(ValuePos:s.X.Pos(),Kind:token.INT,Value:"1"); // use x's position
                    check.binary(ref x, null, s.X, Y, op);
                    if (x.mode == invalid)
                    {
                        return;
                    }
                    check.assignVar(s.X, ref x);
                    break;
                case ref ast.AssignStmt s:

                    if (s.Tok == token.ASSIGN || s.Tok == token.DEFINE) 
                        if (len(s.Lhs) == 0L)
                        {
                            check.invalidAST(s.Pos(), "missing lhs in assignment");
                            return;
                        }
                        if (s.Tok == token.DEFINE)
                        {
                            check.shortVarDecl(s.TokPos, s.Lhs, s.Rhs);
                        }
                        else
                        { 
                            // regular assignment
                            check.assignVars(s.Lhs, s.Rhs);
                        }
                    else 
                        // assignment operations
                        if (len(s.Lhs) != 1L || len(s.Rhs) != 1L)
                        {
                            check.errorf(s.TokPos, "assignment operation %s requires single-valued expressions", s.Tok);
                            return;
                        }
                        op = assignOp(s.Tok);
                        if (op == token.ILLEGAL)
                        {
                            check.invalidAST(s.TokPos, "unknown assignment operation %s", s.Tok);
                            return;
                        }
                        x = default;
                        check.binary(ref x, null, s.Lhs[0L], s.Rhs[0L], op);
                        if (x.mode == invalid)
                        {
                            return;
                        }
                        check.assignVar(s.Lhs[0L], ref x);
                                        break;
                case ref ast.GoStmt s:
                    check.suspendedCall("go", s.Call);
                    break;
                case ref ast.DeferStmt s:
                    check.suspendedCall("defer", s.Call);
                    break;
                case ref ast.ReturnStmt s:
                    var res = check.sig.results;
                    if (res.Len() > 0L)
                    { 
                        // function returns results
                        // (if one, say the first, result parameter is named, all of them are named)
                        if (len(s.Results) == 0L && res.vars[0L].name != "")
                        { 
                            // spec: "Implementation restriction: A compiler may disallow an empty expression
                            // list in a "return" statement if a different entity (constant, type, or variable)
                            // with the same name as a result parameter is in scope at the place of the return."
                            {
                                var obj__prev1 = obj;

                                foreach (var (_, __obj) in res.vars)
                                {
                                    obj = __obj;
                                    {
                                        var (_, alt) = check.scope.LookupParent(obj.name, check.pos);

                                        if (alt != null && alt != obj)
                                        {
                                            check.errorf(s.Pos(), "result parameter %s not in scope at return", obj.name);
                                            check.errorf(alt.Pos(), "\tinner declaration of %s", obj); 
                                            // ok to continue
                                        }

                                    }
                                }
                        else

                                obj = obj__prev1;
                            }

                        }                        { 
                            // return has results or result parameters are unnamed
                            check.initVars(res.vars, s.Results, s.Return);
                        }
                    }
                    else if (len(s.Results) > 0L)
                    {
                        check.error(s.Results[0L].Pos(), "no result values expected");
                        check.use(s.Results);
                    }
                    break;
                case ref ast.BranchStmt s:
                    if (s.Label != null)
                    {
                        check.hasLabel = true;
                        return; // checked in 2nd pass (check.labels)
                    }

                    if (s.Tok == token.BREAK) 
                        if (ctxt & breakOk == 0L)
                        {
                            check.error(s.Pos(), "break not in for, switch, or select statement");
                        }
                    else if (s.Tok == token.CONTINUE) 
                        if (ctxt & continueOk == 0L)
                        {
                            check.error(s.Pos(), "continue not in for statement");
                        }
                    else if (s.Tok == token.FALLTHROUGH) 
                        if (ctxt & fallthroughOk == 0L)
                        {
                            msg = "fallthrough statement out of place";
                            if (ctxt & finalSwitchCase != 0L)
                            {
                                msg = "cannot fallthrough final case in switch";
                            }
                            check.error(s.Pos(), msg);
                        }
                    else 
                        check.invalidAST(s.Pos(), "branch statement: %s", s.Tok);
                                        break;
                case ref ast.BlockStmt s:
                    check.openScope(s, "block");
                    defer(check.closeScope());

                    check.stmtList(inner, s.List);
                    break;
                case ref ast.IfStmt s:
                    check.openScope(s, "if");
                    defer(check.closeScope());

                    check.simpleStmt(s.Init);
                    x = default;
                    check.expr(ref x, s.Cond);
                    if (x.mode != invalid && !isBoolean(x.typ))
                    {
                        check.error(s.Cond.Pos(), "non-boolean condition in if statement");
                    }
                    check.stmt(inner, s.Body); 
                    // The parser produces a correct AST but if it was modified
                    // elsewhere the else branch may be invalid. Check again.
                    switch (s.Else.type())
                    {
                        case ref ast.BadStmt _:
                            break;
                        case ref ast.IfStmt _:
                            check.stmt(inner, s.Else);
                            break;
                        case ref ast.BlockStmt _:
                            check.stmt(inner, s.Else);
                            break;
                        default:
                        {
                            check.error(s.Else.Pos(), "invalid else branch in if statement");
                            break;
                        }

                    }
                    break;
                case ref ast.SwitchStmt s:
                    inner |= breakOk;
                    check.openScope(s, "switch");
                    defer(check.closeScope());

                    check.simpleStmt(s.Init);
                    x = default;
                    if (s.Tag != null)
                    {
                        check.expr(ref x, s.Tag); 
                        // By checking assignment of x to an invisible temporary
                        // (as a compiler would), we get all the relevant checks.
                        check.assignment(ref x, null, "switch expression");
                    }
                    else
                    { 
                        // spec: "A missing switch expression is
                        // equivalent to the boolean value true."
                        x.mode = constant_;
                        x.typ = Typ[Bool];
                        x.val = constant.MakeBool(true);
                        x.expr = ref new ast.Ident(NamePos:s.Body.Lbrace,Name:"true");
                    }
                    check.multipleDefaults(s.Body.List);

                    var seen = make(valueMap); // map of seen case values to positions and types
                    {
                        var i__prev1 = i;

                        foreach (var (__i, __c) in s.Body.List)
                        {
                            i = __i;
                            c = __c;
                            ref ast.CaseClause (clause, _) = c._<ref ast.CaseClause>();
                            if (clause == null)
                            {
                                check.invalidAST(c.Pos(), "incorrect expression switch case");
                                continue;
                            }
                            check.caseValues(ref x, clause.List, seen);
                            check.openScope(clause, "case");
                            inner = inner;
                            if (i + 1L < len(s.Body.List))
                            {
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
                case ref ast.TypeSwitchStmt s:
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
                    ref ast.Ident lhs = default; // lhs identifier or nil
                    ast.Expr rhs = default;
                    switch (s.Assign.type())
                    {
                        case ref ast.ExprStmt guard:
                            rhs = guard.X;
                            break;
                        case ref ast.AssignStmt guard:
                            if (len(guard.Lhs) != 1L || guard.Tok != token.DEFINE || len(guard.Rhs) != 1L)
                            {
                                check.invalidAST(s.Pos(), "incorrect form of type switch guard");
                                return;
                            }
                            lhs, _ = guard.Lhs[0L]._<ref ast.Ident>();
                            if (lhs == null)
                            {
                                check.invalidAST(s.Pos(), "incorrect form of type switch guard");
                                return;
                            }
                            if (lhs.Name == "_")
                            { 
                                // _ := x.(type) is an invalid short variable declaration
                                check.softErrorf(lhs.Pos(), "no new variable on left side of :=");
                                lhs = null; // avoid declared but not used error below
                            }
                            else
                            {
                                check.recordDef(lhs, null); // lhs variable is implicitly declared in each cause clause
                            }
                            rhs = guard.Rhs[0L];
                            break;
                        default:
                        {
                            var guard = s.Assign.type();
                            check.invalidAST(s.Pos(), "incorrect form of type switch guard");
                            return;
                            break;
                        } 

                        // rhs must be of the form: expr.(type) and expr must be an interface
                    } 

                    // rhs must be of the form: expr.(type) and expr must be an interface
                    ref ast.TypeAssertExpr (expr, _) = rhs._<ref ast.TypeAssertExpr>();
                    if (expr == null || expr.Type != null)
                    {
                        check.invalidAST(s.Pos(), "incorrect form of type switch guard");
                        return;
                    }
                    x = default;
                    check.expr(ref x, expr.X);
                    if (x.mode == invalid)
                    {
                        return;
                    }
                    ref Interface (xtyp, _) = x.typ.Underlying()._<ref Interface>();
                    if (xtyp == null)
                    {
                        check.errorf(x.pos(), "%s is not an interface", ref x);
                        return;
                    }
                    check.multipleDefaults(s.Body.List);

                    slice<ref Var> lhsVars = default; // list of implicitly declared lhs variables
                    seen = make_map<Type, token.Pos>(); // map of seen types to positions
                    {
                        var s__prev1 = s;

                        foreach (var (_, __s) in s.Body.List)
                        {
                            s = __s;
                            (clause, _) = s._<ref ast.CaseClause>();
                            if (clause == null)
                            {
                                check.invalidAST(s.Pos(), "incorrect type switch case");
                                continue;
                            } 
                            // Check each type in this type switch case.
                            var T = check.caseTypes(ref x, xtyp, clause.List, seen);
                            check.openScope(clause, "case"); 
                            // If lhs exists, declare a corresponding variable in the case-local scope.
                            if (lhs != null)
                            { 
                                // spec: "The TypeSwitchGuard may include a short variable declaration.
                                // When that form is used, the variable is declared at the beginning of
                                // the implicit block in each clause. In clauses with a case listing
                                // exactly one type, the variable has that type; otherwise, the variable
                                // has the type of the expression in the TypeSwitchGuard."
                                if (len(clause.List) != 1L || T == null)
                                {
                                    T = x.typ;
                                }
                                var obj = NewVar(lhs.Pos(), check.pkg, lhs.Name, T);
                                var scopePos = clause.Pos() + token.Pos(len("default")); // for default clause (len(List) == 0)
                                {
                                    var n = len(clause.List);

                                    if (n > 0L)
                                    {
                                        scopePos = clause.List[n - 1L].End();
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

                    if (lhs != null)
                    {
                        bool used = default;
                        foreach (var (_, v) in lhsVars)
                        {
                            if (v.used)
                            {
                                used = true;
                            }
                            v.used = true; // avoid usage error when checking entire function
                        }
                        if (!used)
                        {
                            check.softErrorf(lhs.Pos(), "%s declared but not used", lhs.Name);
                        }
                    }
                    break;
                case ref ast.SelectStmt s:
                    inner |= breakOk;

                    check.multipleDefaults(s.Body.List);

                    {
                        var s__prev1 = s;

                        foreach (var (_, __s) in s.Body.List)
                        {
                            s = __s;
                            (clause, _) = s._<ref ast.CommClause>();
                            if (clause == null)
                            {
                                continue; // error reported before
                            } 

                            // clause.Comm must be a SendStmt, RecvStmt, or default case
                            var valid = false;
                            rhs = default; // rhs of RecvStmt, or nil
                            switch (clause.Comm.type())
                            {
                                case ref ast.SendStmt s:
                                    valid = true;
                                    break;
                                case ref ast.AssignStmt s:
                                    if (len(s.Rhs) == 1L)
                                    {
                                        rhs = s.Rhs[0L];
                                    }
                                    break;
                                case ref ast.ExprStmt s:
                                    rhs = s.X;
                                    break; 

                                // if present, rhs must be a receive operation
                            } 

                            // if present, rhs must be a receive operation
                            if (rhs != null)
                            {
                                {
                                    operand x__prev2 = x;

                                    ref ast.UnaryExpr (x, _) = unparen(rhs)._<ref ast.UnaryExpr>();

                                    if (x != null && x.Op == token.ARROW)
                                    {
                                        valid = true;
                                    }

                                    x = x__prev2;

                                }
                            }
                            if (!valid)
                            {
                                check.error(clause.Comm.Pos(), "select case must be send or receive (possibly with assignment)");
                                continue;
                            }
                            check.openScope(s, "case");
                            if (clause.Comm != null)
                            {
                                check.stmt(inner, clause.Comm);
                            }
                            check.stmtList(inner, clause.Body);
                            check.closeScope();
                        }

                        s = s__prev1;
                    }
                    break;
                case ref ast.ForStmt s:
                    inner |= breakOk | continueOk;
                    check.openScope(s, "for");
                    defer(check.closeScope());

                    check.simpleStmt(s.Init);
                    if (s.Cond != null)
                    {
                        x = default;
                        check.expr(ref x, s.Cond);
                        if (x.mode != invalid && !isBoolean(x.typ))
                        {
                            check.error(s.Cond.Pos(), "non-boolean condition in for statement");
                        }
                    }
                    check.simpleStmt(s.Post); 
                    // spec: "The init statement may be a short variable
                    // declaration, but the post statement must not."
                    {
                        var s__prev1 = s;

                        ref ast.AssignStmt (s, _) = s.Post._<ref ast.AssignStmt>();

                        if (s != null && s.Tok == token.DEFINE)
                        {
                            check.softErrorf(s.Pos(), "cannot declare in post statement"); 
                            // Don't call useLHS here because we want to use the lhs in
                            // this erroneous statement so that we don't get errors about
                            // these lhs variables being declared but not used.
                            check.use(s.Lhs); // avoid follow-up errors
                        }

                        s = s__prev1;

                    }
                    check.stmt(inner, s.Body);
                    break;
                case ref ast.RangeStmt s:
                    inner |= breakOk | continueOk;
                    check.openScope(s, "for");
                    defer(check.closeScope()); 

                    // check expression to iterate over
                    x = default;
                    check.expr(ref x, s.X); 

                    // determine key/value types
                    Type key = default;                    Type val = default;

                    if (x.mode != invalid)
                    {
                        switch (x.typ.Underlying().type())
                        {
                            case ref Basic typ:
                                if (isString(typ))
                                {
                                    key = Typ[Int];
                                    val = universeRune; // use 'rune' name
                                }
                                break;
                            case ref Array typ:
                                key = Typ[Int];
                                val = typ.elem;
                                break;
                            case ref Slice typ:
                                key = Typ[Int];
                                val = typ.elem;
                                break;
                            case ref Pointer typ:
                                {
                                    var typ__prev2 = typ;

                                    ref Array (typ, _) = typ.@base.Underlying()._<ref Array>();

                                    if (typ != null)
                                    {
                                        key = Typ[Int];
                                        val = typ.elem;
                                    }

                                    typ = typ__prev2;

                                }
                                break;
                            case ref Map typ:
                                key = typ.key;
                                val = typ.elem;
                                break;
                            case ref Chan typ:
                                key = typ.elem;
                                val = Typ[Invalid];
                                if (typ.dir == SendOnly)
                                {
                                    check.errorf(x.pos(), "cannot range over send-only channel %s", ref x); 
                                    // ok to continue
                                }
                                if (s.Value != null)
                                {
                                    check.errorf(s.Value.Pos(), "iteration over %s permits only one iteration variable", ref x); 
                                    // ok to continue
                                }
                                break;
                        }
                    }
                    if (key == null)
                    {
                        check.errorf(x.pos(), "cannot range over %s", ref x); 
                        // ok to continue
                    } 

                    // check assignment to/declaration of iteration variables
                    // (irregular assignment, cannot easily map to existing assignment checks)

                    // lhs expressions and initialization value (rhs) types
                    lhs = new array<ast.Expr>(new ast.Expr[] { s.Key, s.Value });
                    rhs = new array<Type>(new Type[] { key, val }); // key, val may be nil

                    if (s.Tok == token.DEFINE)
                    { 
                        // short variable declaration; variable scope starts after the range clause
                        // (the for loop opens a new scope, so variables on the lhs never redeclare
                        // previously declared variables)
                        slice<ref Var> vars = default;
                        {
                            var i__prev1 = i;
                            ref ast.Ident lhs__prev1 = lhs;

                            foreach (var (__i, __lhs) in lhs)
                            {
                                i = __i;
                                lhs = __lhs;
                                if (lhs == null)
                                {
                                    continue;
                                } 

                                // determine lhs variable
                                obj = default;
                                {
                                    ref ast.Ident (ident, _) = lhs._<ref ast.Ident>();

                                    if (ident != null)
                                    { 
                                        // declare new variable
                                        var name = ident.Name;
                                        obj = NewVar(ident.Pos(), check.pkg, name, null);
                                        check.recordDef(ident, obj); 
                                        // _ variables don't count as new variables
                                        if (name != "_")
                                        {
                                            vars = append(vars, obj);
                                        }
                                    }
                                    else
                                    {
                                        check.errorf(lhs.Pos(), "cannot declare %s", lhs);
                                        obj = NewVar(lhs.Pos(), check.pkg, "_", null); // dummy variable
                                    } 

                                    // initialize lhs variable

                                } 

                                // initialize lhs variable
                                {
                                    var typ__prev2 = typ;

                                    var typ = rhs[i];

                                    if (typ != null)
                                    {
                                        x.mode = value;
                                        x.expr = lhs; // we don't have a better rhs expression to use here
                                        x.typ = typ;
                                        check.initVar(obj, ref x, "range clause");
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

                        if (len(vars) > 0L)
                        {
                            scopePos = s.X.End();
                            {
                                var obj__prev1 = obj;

                                foreach (var (_, __obj) in vars)
                                {
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

                        }                        {
                            check.error(s.TokPos, "no new variables on left side of :=");
                        }
                    }                    { 
                        // ordinary assignment
                        {
                            var i__prev1 = i;
                            ref ast.Ident lhs__prev1 = lhs;

                            foreach (var (__i, __lhs) in lhs)
                            {
                                i = __i;
                                lhs = __lhs;
                                if (lhs == null)
                                {
                                    continue;
                                }
                                {
                                    var typ__prev2 = typ;

                                    typ = rhs[i];

                                    if (typ != null)
                                    {
                                        x.mode = value;
                                        x.expr = lhs; // we don't have a better rhs expression to use here
                                        x.typ = typ;
                                        check.assignVar(lhs, ref x);
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
                    check.error(s.Pos(), "invalid statement");
                    break;
                }
            }
        });
    }
}}
