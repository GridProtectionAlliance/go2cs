// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
This file contains the code to check range loop variables bound inside function
literals that are deferred or launched in new goroutines. We only check
instances where the defer or go statement is the last statement in the loop
body, as otherwise we would need whole program analysis.

For example:

    for i, v := range s {
        go func() {
            println(i, v) // not what you might expect
        }()
    }

See: https://golang.org/doc/go_faq.html#closures_and_goroutines
*/

// package main -- go2cs converted at 2020 August 29 10:09:28 UTC
// Original source: C:\Go\src\cmd\vet\rangeloop.go
using ast = go.go.ast_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("rangeloops", "check that loop variables are used correctly", checkLoop, rangeStmt, forStmt);
        }

        // checkLoop walks the body of the provided loop statement, checking whether
        // its index or value variables are used unsafely inside goroutines or deferred
        // function literals.
        private static void checkLoop(ref File f, ast.Node node)
        { 
            // Find the variables updated by the loop statement.
            slice<ref ast.Ident> vars = default;
            Action<ast.Expr> addVar = expr =>
            {
                {
                    ref ast.Ident id__prev1 = id;

                    ref ast.Ident (id, ok) = expr._<ref ast.Ident>();

                    if (ok)
                    {
                        vars = append(vars, id);
                    }

                    id = id__prev1;

                }
            }
;
            ref ast.BlockStmt body = default;
            switch (node.type())
            {
                case ref ast.RangeStmt n:
                    body = n.Body;
                    addVar(n.Key);
                    addVar(n.Value);
                    break;
                case ref ast.ForStmt n:
                    body = n.Body;
                    switch (n.Post.type())
                    {
                        case ref ast.AssignStmt post:
                            foreach (var (_, lhs) in post.Lhs)
                            {
                                addVar(lhs);
                            }
                            break;
                        case ref ast.IncDecStmt post:
                            addVar(post.X);
                            break;
                    }
                    break;
            }
            if (vars == null)
            {
                return;
            } 

            // Inspect a go or defer statement
            // if it's the last one in the loop body.
            // (We give up if there are following statements,
            // because it's hard to prove go isn't followed by wait,
            // or defer by return.)
            if (len(body.List) == 0L)
            {
                return;
            }
            ref ast.CallExpr last = default;
            switch (body.List[len(body.List) - 1L].type())
            {
                case ref ast.GoStmt s:
                    last = s.Call;
                    break;
                case ref ast.DeferStmt s:
                    last = s.Call;
                    break;
                default:
                {
                    var s = body.List[len(body.List) - 1L].type();
                    return;
                    break;
                }
            }
            ref ast.FuncLit (lit, ok) = last.Fun._<ref ast.FuncLit>();
            if (!ok)
            {
                return;
            }
            ast.Inspect(lit.Body, n =>
            {
                (id, ok) = n._<ref ast.Ident>();
                if (!ok || id.Obj == null)
                {
                    return true;
                }
                if (f.pkg.types[id].Type == null)
                { 
                    // Not referring to a variable (e.g. struct field name)
                    return true;
                }
                foreach (var (_, v) in vars)
                {
                    if (v.Obj == id.Obj)
                    {
                        f.Badf(id.Pos(), "loop variable %s captured by func literal", id.Name);
                    }
                }
                return true;
            });
        }
    }
}
