// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Check for syntactically unreachable code.

// package main -- go2cs converted at 2020 August 29 10:08:53 UTC
// Original source: C:\Go\src\cmd\vet\deadcode.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("unreachable", "check for unreachable code", checkUnreachable, funcDecl, funcLit);
        }

        private partial struct deadState
        {
            public ptr<File> f;
            public map<ast.Stmt, bool> hasBreak;
            public map<@string, bool> hasGoto;
            public map<@string, ast.Stmt> labels;
            public ast.Stmt breakTarget;
            public bool reachable;
        }

        // checkUnreachable checks a function body for dead code.
        //
        // TODO(adonovan): use the new cfg package, which is more precise.
        private static void checkUnreachable(ref File f, ast.Node node)
        {
            ref ast.BlockStmt body = default;
            switch (node.type())
            {
                case ref ast.FuncDecl n:
                    body = n.Body;
                    break;
                case ref ast.FuncLit n:
                    body = n.Body;
                    break;
            }
            if (body == null)
            {
                return;
            }
            deadState d = ref new deadState(f:f,hasBreak:make(map[ast.Stmt]bool),hasGoto:make(map[string]bool),labels:make(map[string]ast.Stmt),);

            d.findLabels(body);

            d.reachable = true;
            d.findDead(body);
        }

        // findLabels gathers information about the labels defined and used by stmt
        // and about which statements break, whether a label is involved or not.
        private static void findLabels(this ref deadState d, ast.Stmt stmt)
        {
            switch (stmt.type())
            {
                case ref ast.AssignStmt x:
                    break;
                case ref ast.BadStmt x:
                    break;
                case ref ast.DeclStmt x:
                    break;
                case ref ast.DeferStmt x:
                    break;
                case ref ast.EmptyStmt x:
                    break;
                case ref ast.ExprStmt x:
                    break;
                case ref ast.GoStmt x:
                    break;
                case ref ast.IncDecStmt x:
                    break;
                case ref ast.ReturnStmt x:
                    break;
                case ref ast.SendStmt x:
                    break;
                case ref ast.BlockStmt x:
                    {
                        var stmt__prev1 = stmt;

                        foreach (var (_, __stmt) in x.List)
                        {
                            stmt = __stmt;
                            d.findLabels(stmt);
                        }

                        stmt = stmt__prev1;
                    }
                    break;
                case ref ast.BranchStmt x:

                    if (x.Tok == token.GOTO) 
                        if (x.Label != null)
                        {
                            d.hasGoto[x.Label.Name] = true;
                        }
                    else if (x.Tok == token.BREAK) 
                        var stmt = d.breakTarget;
                        if (x.Label != null)
                        {
                            stmt = d.labels[x.Label.Name];
                        }
                        if (stmt != null)
                        {
                            d.hasBreak[stmt] = true;
                        }
                                        break;
                case ref ast.IfStmt x:
                    d.findLabels(x.Body);
                    if (x.Else != null)
                    {
                        d.findLabels(x.Else);
                    }
                    break;
                case ref ast.LabeledStmt x:
                    d.labels[x.Label.Name] = x.Stmt;
                    d.findLabels(x.Stmt); 

                    // These cases are all the same, but the x.Body only works
                    // when the specific type of x is known, so the cases cannot
                    // be merged.
                    break;
                case ref ast.ForStmt x:
                    var outer = d.breakTarget;
                    d.breakTarget = x;
                    d.findLabels(x.Body);
                    d.breakTarget = outer;
                    break;
                case ref ast.RangeStmt x:
                    outer = d.breakTarget;
                    d.breakTarget = x;
                    d.findLabels(x.Body);
                    d.breakTarget = outer;
                    break;
                case ref ast.SelectStmt x:
                    outer = d.breakTarget;
                    d.breakTarget = x;
                    d.findLabels(x.Body);
                    d.breakTarget = outer;
                    break;
                case ref ast.SwitchStmt x:
                    outer = d.breakTarget;
                    d.breakTarget = x;
                    d.findLabels(x.Body);
                    d.breakTarget = outer;
                    break;
                case ref ast.TypeSwitchStmt x:
                    outer = d.breakTarget;
                    d.breakTarget = x;
                    d.findLabels(x.Body);
                    d.breakTarget = outer;
                    break;
                case ref ast.CommClause x:
                    {
                        var stmt__prev1 = stmt;

                        foreach (var (_, __stmt) in x.Body)
                        {
                            stmt = __stmt;
                            d.findLabels(stmt);
                        }

                        stmt = stmt__prev1;
                    }
                    break;
                case ref ast.CaseClause x:
                    {
                        var stmt__prev1 = stmt;

                        foreach (var (_, __stmt) in x.Body)
                        {
                            stmt = __stmt;
                            d.findLabels(stmt);
                        }

                        stmt = stmt__prev1;
                    }
                    break;
                default:
                {
                    var x = stmt.type();
                    d.f.Warnf(x.Pos(), "internal error in findLabels: unexpected statement %T", x);
                    break;
                }
            }
        }

        // findDead walks the statement looking for dead code.
        // If d.reachable is false on entry, stmt itself is dead.
        // When findDead returns, d.reachable tells whether the
        // statement following stmt is reachable.
        private static void findDead(this ref deadState d, ast.Stmt stmt)
        { 
            // Is this a labeled goto target?
            // If so, assume it is reachable due to the goto.
            // This is slightly conservative, in that we don't
            // check that the goto is reachable, so
            //    L: goto L
            // will not provoke a warning.
            // But it's good enough.
            {
                ref ast.LabeledStmt x__prev1 = x;

                ref ast.LabeledStmt (x, isLabel) = stmt._<ref ast.LabeledStmt>();

                if (isLabel && d.hasGoto[x.Label.Name])
                {
                    d.reachable = true;
                }

                x = x__prev1;

            }

            if (!d.reachable)
            {
                switch (stmt.type())
                {
                    case ref ast.EmptyStmt _:
                        break;
                    default:
                    {
                        d.f.Bad(stmt.Pos(), "unreachable code");
                        d.reachable = true; // silence error about next statement
                        break;
                    }
                }
            }
            switch (stmt.type())
            {
                case ref ast.AssignStmt x:
                    break;
                case ref ast.BadStmt x:
                    break;
                case ref ast.DeclStmt x:
                    break;
                case ref ast.DeferStmt x:
                    break;
                case ref ast.EmptyStmt x:
                    break;
                case ref ast.GoStmt x:
                    break;
                case ref ast.IncDecStmt x:
                    break;
                case ref ast.SendStmt x:
                    break;
                case ref ast.BlockStmt x:
                    {
                        var stmt__prev1 = stmt;

                        foreach (var (_, __stmt) in x.List)
                        {
                            stmt = __stmt;
                            d.findDead(stmt);
                        }

                        stmt = stmt__prev1;
                    }
                    break;
                case ref ast.BranchStmt x:

                    if (x.Tok == token.BREAK || x.Tok == token.GOTO || x.Tok == token.FALLTHROUGH) 
                        d.reachable = false;
                    else if (x.Tok == token.CONTINUE) 
                        // NOTE: We accept "continue" statements as terminating.
                        // They are not necessary in the spec definition of terminating,
                        // because a continue statement cannot be the final statement
                        // before a return. But for the more general problem of syntactically
                        // identifying dead code, continue redirects control flow just
                        // like the other terminating statements.
                        d.reachable = false;
                                        break;
                case ref ast.ExprStmt x:
                    ref ast.CallExpr (call, ok) = x.X._<ref ast.CallExpr>();
                    if (ok)
                    {
                        ref ast.Ident (name, ok) = call.Fun._<ref ast.Ident>();
                        if (ok && name.Name == "panic" && name.Obj == null)
                        {
                            d.reachable = false;
                        }
                    }
                    break;
                case ref ast.ForStmt x:
                    d.findDead(x.Body);
                    d.reachable = x.Cond != null || d.hasBreak[x];
                    break;
                case ref ast.IfStmt x:
                    d.findDead(x.Body);
                    if (x.Else != null)
                    {
                        var r = d.reachable;
                        d.reachable = true;
                        d.findDead(x.Else);
                        d.reachable = d.reachable || r;
                    }
                    else
                    { 
                        // might not have executed if statement
                        d.reachable = true;
                    }
                    break;
                case ref ast.LabeledStmt x:
                    d.findDead(x.Stmt);
                    break;
                case ref ast.RangeStmt x:
                    d.findDead(x.Body);
                    d.reachable = true;
                    break;
                case ref ast.ReturnStmt x:
                    d.reachable = false;
                    break;
                case ref ast.SelectStmt x:
                    var anyReachable = false;
                    foreach (var (_, comm) in x.Body.List)
                    {
                        d.reachable = true;
                        {
                            var stmt__prev2 = stmt;

                            foreach (var (_, __stmt) in comm._<ref ast.CommClause>().Body)
                            {
                                stmt = __stmt;
                                d.findDead(stmt);
                            }

                            stmt = stmt__prev2;
                        }

                        anyReachable = anyReachable || d.reachable;
                    }
                    d.reachable = anyReachable || d.hasBreak[x];
                    break;
                case ref ast.SwitchStmt x:
                    anyReachable = false;
                    var hasDefault = false;
                    {
                        var cas__prev1 = cas;

                        foreach (var (_, __cas) in x.Body.List)
                        {
                            cas = __cas;
                            ref ast.CaseClause cc = cas._<ref ast.CaseClause>();
                            if (cc.List == null)
                            {
                                hasDefault = true;
                            }
                            d.reachable = true;
                            {
                                var stmt__prev2 = stmt;

                                foreach (var (_, __stmt) in cc.Body)
                                {
                                    stmt = __stmt;
                                    d.findDead(stmt);
                                }

                                stmt = stmt__prev2;
                            }

                            anyReachable = anyReachable || d.reachable;
                        }

                        cas = cas__prev1;
                    }

                    d.reachable = anyReachable || d.hasBreak[x] || !hasDefault;
                    break;
                case ref ast.TypeSwitchStmt x:
                    anyReachable = false;
                    hasDefault = false;
                    {
                        var cas__prev1 = cas;

                        foreach (var (_, __cas) in x.Body.List)
                        {
                            cas = __cas;
                            cc = cas._<ref ast.CaseClause>();
                            if (cc.List == null)
                            {
                                hasDefault = true;
                            }
                            d.reachable = true;
                            {
                                var stmt__prev2 = stmt;

                                foreach (var (_, __stmt) in cc.Body)
                                {
                                    stmt = __stmt;
                                    d.findDead(stmt);
                                }

                                stmt = stmt__prev2;
                            }

                            anyReachable = anyReachable || d.reachable;
                        }

                        cas = cas__prev1;
                    }

                    d.reachable = anyReachable || d.hasBreak[x] || !hasDefault;
                    break;
                default:
                {
                    var x = stmt.type();
                    d.f.Warnf(x.Pos(), "internal error in findDead: unexpected statement %T", x);
                    break;
                }
            }
        }
    }
}
