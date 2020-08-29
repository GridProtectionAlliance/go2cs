// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// Simplified dead code detector. Used for skipping certain checks
// on unreachable code (for instance, shift checks on arch-specific code).

// package main -- go2cs converted at 2020 August 29 10:08:51 UTC
// Original source: C:\Go\src\cmd\vet\dead.go
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        // updateDead puts unreachable "if" and "case" nodes into f.dead.
        private static void updateDead(this ref File f, ast.Node node)
        {
            if (f.dead[node])
            { 
                // The node is already marked as dead.
                return;
            }
            switch (node.type())
            {
                case ref ast.IfStmt stmt:
                    var v = f.pkg.types[stmt.Cond].Value;
                    if (v == null)
                    {
                        return;
                    }
                    if (!constant.BoolVal(v))
                    {
                        f.setDead(stmt.Body);
                        return;
                    }
                    f.setDead(stmt.Else);
                    break;
                case ref ast.SwitchStmt stmt:
                    if (stmt.Tag == null)
                    {
BodyLoopBool:
                        {
                            var stmt__prev1 = stmt;

                            foreach (var (_, __stmt) in stmt.Body.List)
                            {
                                stmt = __stmt;
                                ref ast.CaseClause cc = stmt._<ref ast.CaseClause>();
                                if (cc.List == null)
                                { 
                                    // Skip default case.
                                    continue;
                                }
                                {
                                    var expr__prev2 = expr;

                                    foreach (var (_, __expr) in cc.List)
                                    {
                                        expr = __expr;
                                        v = f.pkg.types[expr].Value;
                                        if (v == null || constant.BoolVal(v))
                                        {
                                            _continueBodyLoopBool = true;
                                            break;
                                        }
                                    }
                                    expr = expr__prev2;
                                }

                                f.setDead(cc);
                            }
                            stmt = stmt__prev1;
                        }
                        return;
                    }
                    v = f.pkg.types[stmt.Tag].Value;
                    if (v == null || v.Kind() != constant.Int)
                    {
                        return;
                    }
                    var (tagN, ok) = constant.Uint64Val(v);
                    if (!ok)
                    {
                        return;
                    }
BodyLoopInt:
                    foreach (var (_, x) in stmt.Body.List)
                    {
                        cc = x._<ref ast.CaseClause>();
                        if (cc.List == null)
                        { 
                            // Skip default case.
                            continue;
                        }
                        {
                            var expr__prev2 = expr;

                            foreach (var (_, __expr) in cc.List)
                            {
                                expr = __expr;
                                v = f.pkg.types[expr].Value;
                                if (v == null)
                                {
                                    _continueBodyLoopInt = true;
                                    break;
                                }
                                var (n, ok) = constant.Uint64Val(v);
                                if (!ok || tagN == n)
                                {
                                    _continueBodyLoopInt = true;
                                    break;
                                }
                            }
                            expr = expr__prev2;
                        }

                        f.setDead(cc);
                    }                    break;
            }
        }

        // setDead marks the node and all the children as dead.
        private static void setDead(this ref File f, ast.Node node)
        {
            deadVisitor dv = new deadVisitor(f:f,);
            ast.Walk(dv, node);
        }

        private partial struct deadVisitor
        {
            public ptr<File> f;
        }

        private static ast.Visitor Visit(this deadVisitor dv, ast.Node node)
        {
            if (node == null)
            {
                return null;
            }
            dv.f.dead[node] = true;
            return dv;
        }
    }
}
