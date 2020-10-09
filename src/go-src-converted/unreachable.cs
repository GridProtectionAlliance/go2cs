// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package unreachable defines an Analyzer that checks for unreachable code.
// package unreachable -- go2cs converted at 2020 October 09 06:04:20 UTC
// import "golang.org/x/tools/go/analysis/passes/unreachable" ==> using unreachable = go.golang.org.x.tools.go.analysis.passes.unreachable_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\unreachable\unreachable.go
// TODO(adonovan): use the new cfg package, which is more precise.

using ast = go.go.ast_package;
using token = go.go.token_package;
using log = go.log_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class unreachable_package
    {
        public static readonly @string Doc = (@string)"check for unreachable code\n\nThe unreachable analyzer finds statements that execut" +
    "ion can never reach\nbecause they are preceded by an return statement, a call to " +
    "panic, an\ninfinite loop, or similar constructs.";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"unreachable",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},RunDespiteErrors:true,Run:run,));

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.FuncDecl)(nil), (*ast.FuncLit)(nil) });
            inspect.Preorder(nodeFilter, n =>
            {
                ptr<ast.BlockStmt> body;
                switch (n.type())
                {
                    case ptr<ast.FuncDecl> n:
                        body = n.Body;
                        break;
                    case ptr<ast.FuncLit> n:
                        body = n.Body;
                        break;
                }
                if (body == null)
                {
                    return ;
                }

                ptr<deadState> d = addr(new deadState(pass:pass,hasBreak:make(map[ast.Stmt]bool),hasGoto:make(map[string]bool),labels:make(map[string]ast.Stmt),));
                d.findLabels(body);
                d.reachable = true;
                d.findDead(body);

            });
            return (null, error.As(null!)!);

        }

        private partial struct deadState
        {
            public ptr<analysis.Pass> pass;
            public map<ast.Stmt, bool> hasBreak;
            public map<@string, bool> hasGoto;
            public map<@string, ast.Stmt> labels;
            public ast.Stmt breakTarget;
            public bool reachable;
        }

        // findLabels gathers information about the labels defined and used by stmt
        // and about which statements break, whether a label is involved or not.
        private static void findLabels(this ptr<deadState> _addr_d, ast.Stmt stmt)
        {
            ref deadState d = ref _addr_d.val;

            switch (stmt.type())
            {
                case ptr<ast.AssignStmt> x:
                    break;
                case ptr<ast.BadStmt> x:
                    break;
                case ptr<ast.DeclStmt> x:
                    break;
                case ptr<ast.DeferStmt> x:
                    break;
                case ptr<ast.EmptyStmt> x:
                    break;
                case ptr<ast.ExprStmt> x:
                    break;
                case ptr<ast.GoStmt> x:
                    break;
                case ptr<ast.IncDecStmt> x:
                    break;
                case ptr<ast.ReturnStmt> x:
                    break;
                case ptr<ast.SendStmt> x:
                    break;
                case ptr<ast.BlockStmt> x:
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
                case ptr<ast.BranchStmt> x:

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
                case ptr<ast.IfStmt> x:
                    d.findLabels(x.Body);
                    if (x.Else != null)
                    {
                        d.findLabels(x.Else);
                    }

                    break;
                case ptr<ast.LabeledStmt> x:
                    d.labels[x.Label.Name] = x.Stmt;
                    d.findLabels(x.Stmt); 

                    // These cases are all the same, but the x.Body only works
                    // when the specific type of x is known, so the cases cannot
                    // be merged.
                    break;
                case ptr<ast.ForStmt> x:
                    var outer = d.breakTarget;
                    d.breakTarget = x;
                    d.findLabels(x.Body);
                    d.breakTarget = outer;
                    break;
                case ptr<ast.RangeStmt> x:
                    outer = d.breakTarget;
                    d.breakTarget = x;
                    d.findLabels(x.Body);
                    d.breakTarget = outer;
                    break;
                case ptr<ast.SelectStmt> x:
                    outer = d.breakTarget;
                    d.breakTarget = x;
                    d.findLabels(x.Body);
                    d.breakTarget = outer;
                    break;
                case ptr<ast.SwitchStmt> x:
                    outer = d.breakTarget;
                    d.breakTarget = x;
                    d.findLabels(x.Body);
                    d.breakTarget = outer;
                    break;
                case ptr<ast.TypeSwitchStmt> x:
                    outer = d.breakTarget;
                    d.breakTarget = x;
                    d.findLabels(x.Body);
                    d.breakTarget = outer;
                    break;
                case ptr<ast.CommClause> x:
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
                case ptr<ast.CaseClause> x:
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
                    log.Fatalf("%s: internal error in findLabels: unexpected statement %T", d.pass.Fset.Position(x.Pos()), x);
                    break;
                }
            }

        }

        // findDead walks the statement looking for dead code.
        // If d.reachable is false on entry, stmt itself is dead.
        // When findDead returns, d.reachable tells whether the
        // statement following stmt is reachable.
        private static void findDead(this ptr<deadState> _addr_d, ast.Stmt stmt)
        {
            ref deadState d = ref _addr_d.val;
 
            // Is this a labeled goto target?
            // If so, assume it is reachable due to the goto.
            // This is slightly conservative, in that we don't
            // check that the goto is reachable, so
            //    L: goto L
            // will not provoke a warning.
            // But it's good enough.
            {
                ptr<ast.LabeledStmt> x__prev1 = x;

                ptr<ast.LabeledStmt> (x, isLabel) = stmt._<ptr<ast.LabeledStmt>>();

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
                    case ptr<ast.EmptyStmt> _:
                        break;
                    default:
                    {
                        d.pass.Report(new analysis.Diagnostic(Pos:stmt.Pos(),End:stmt.End(),Message:"unreachable code",SuggestedFixes:[]analysis.SuggestedFix{{Message:"Remove",TextEdits:[]analysis.TextEdit{{Pos:stmt.Pos(),End:stmt.End(),}},}},));
                        d.reachable = true; // silence error about next statement
                        break;
                    }
                }

            }

            switch (stmt.type())
            {
                case ptr<ast.AssignStmt> x:
                    break;
                case ptr<ast.BadStmt> x:
                    break;
                case ptr<ast.DeclStmt> x:
                    break;
                case ptr<ast.DeferStmt> x:
                    break;
                case ptr<ast.EmptyStmt> x:
                    break;
                case ptr<ast.GoStmt> x:
                    break;
                case ptr<ast.IncDecStmt> x:
                    break;
                case ptr<ast.SendStmt> x:
                    break;
                case ptr<ast.BlockStmt> x:
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
                case ptr<ast.BranchStmt> x:

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
                case ptr<ast.ExprStmt> x:
                    ptr<ast.CallExpr> (call, ok) = x.X._<ptr<ast.CallExpr>>();
                    if (ok)
                    {
                        ptr<ast.Ident> (name, ok) = call.Fun._<ptr<ast.Ident>>();
                        if (ok && name.Name == "panic" && name.Obj == null)
                        {
                            d.reachable = false;
                        }

                    }

                    break;
                case ptr<ast.ForStmt> x:
                    d.findDead(x.Body);
                    d.reachable = x.Cond != null || d.hasBreak[x];
                    break;
                case ptr<ast.IfStmt> x:
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
                case ptr<ast.LabeledStmt> x:
                    d.findDead(x.Stmt);
                    break;
                case ptr<ast.RangeStmt> x:
                    d.findDead(x.Body);
                    d.reachable = true;
                    break;
                case ptr<ast.ReturnStmt> x:
                    d.reachable = false;
                    break;
                case ptr<ast.SelectStmt> x:
                    var anyReachable = false;
                    foreach (var (_, comm) in x.Body.List)
                    {
                        d.reachable = true;
                        {
                            var stmt__prev2 = stmt;

                            foreach (var (_, __stmt) in comm._<ptr<ast.CommClause>>().Body)
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
                case ptr<ast.SwitchStmt> x:
                    anyReachable = false;
                    var hasDefault = false;
                    {
                        var cas__prev1 = cas;

                        foreach (var (_, __cas) in x.Body.List)
                        {
                            cas = __cas;
                            ptr<ast.CaseClause> cc = cas._<ptr<ast.CaseClause>>();
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
                case ptr<ast.TypeSwitchStmt> x:
                    anyReachable = false;
                    hasDefault = false;
                    {
                        var cas__prev1 = cas;

                        foreach (var (_, __cas) in x.Body.List)
                        {
                            cas = __cas;
                            cc = cas._<ptr<ast.CaseClause>>();
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
                    log.Fatalf("%s: internal error in findDead: unexpected statement %T", d.pass.Fset.Position(x.Pos()), x);
                    break;
                }
            }

        }
    }
}}}}}}}
