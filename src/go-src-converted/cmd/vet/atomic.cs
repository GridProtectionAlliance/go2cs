// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 10:08:47 UTC
// Original source: C:\Go\src\cmd\vet\atomic.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("atomic", "check for common mistaken usages of the sync/atomic package", checkAtomicAssignment, assignStmt);
        }

        // checkAtomicAssignment walks the assignment statement checking for common
        // mistaken usage of atomic package, such as: x = atomic.AddUint64(&x, 1)
        private static void checkAtomicAssignment(ref File f, ast.Node node)
        {
            ref ast.AssignStmt n = node._<ref ast.AssignStmt>();
            if (len(n.Lhs) != len(n.Rhs))
            {
                return;
            }
            if (len(n.Lhs) == 1L && n.Tok == token.DEFINE)
            {
                return;
            }
            foreach (var (i, right) in n.Rhs)
            {
                ref ast.CallExpr (call, ok) = right._<ref ast.CallExpr>();
                if (!ok)
                {
                    continue;
                }
                ref ast.SelectorExpr (sel, ok) = call.Fun._<ref ast.SelectorExpr>();
                if (!ok)
                {
                    continue;
                }
                ref ast.Ident (pkg, ok) = sel.X._<ref ast.Ident>();
                if (!ok || pkg.Name != "atomic")
                {
                    continue;
                }
                switch (sel.Sel.Name)
                {
                    case "AddInt32": 

                    case "AddInt64": 

                    case "AddUint32": 

                    case "AddUint64": 

                    case "AddUintptr": 
                        f.checkAtomicAddAssignment(n.Lhs[i], call);
                        break;
                }
            }
        }

        // checkAtomicAddAssignment walks the atomic.Add* method calls checking for assigning the return value
        // to the same variable being used in the operation
        private static void checkAtomicAddAssignment(this ref File f, ast.Expr left, ref ast.CallExpr call)
        {
            if (len(call.Args) != 2L)
            {
                return;
            }
            var arg = call.Args[0L];
            var broken = false;

            {
                ref ast.UnaryExpr (uarg, ok) = arg._<ref ast.UnaryExpr>();

                if (ok && uarg.Op == token.AND)
                {
                    broken = f.gofmt(left) == f.gofmt(uarg.X);
                }                {
                    ref ast.StarExpr (star, ok) = left._<ref ast.StarExpr>();


                    else if (ok)
                    {
                        broken = f.gofmt(star.X) == f.gofmt(arg);
                    }

                }


            }

            if (broken)
            {
                f.Bad(left.Pos(), "direct assignment to atomic value");
            }
        }
    }
}
