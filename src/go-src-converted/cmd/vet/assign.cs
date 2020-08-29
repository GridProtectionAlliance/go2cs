// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
This file contains the code to check for useless assignments.
*/

// package main -- go2cs converted at 2020 August 29 10:08:46 UTC
// Original source: C:\Go\src\cmd\vet\assign.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using reflect = go.reflect_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("assign", "check for useless assignments", checkAssignStmt, assignStmt);
        }

        // TODO: should also check for assignments to struct fields inside methods
        // that are on T instead of *T.

        // checkAssignStmt checks for assignments of the form "<expr> = <expr>".
        // These are almost always useless, and even when they aren't they are usually a mistake.
        private static void checkAssignStmt(ref File f, ast.Node node)
        {
            ref ast.AssignStmt stmt = node._<ref ast.AssignStmt>();
            if (stmt.Tok != token.ASSIGN)
            {
                return; // ignore :=
            }
            if (len(stmt.Lhs) != len(stmt.Rhs))
            { 
                // If LHS and RHS have different cardinality, they can't be the same.
                return;
            }
            foreach (var (i, lhs) in stmt.Lhs)
            {
                var rhs = stmt.Rhs[i];
                if (hasSideEffects(lhs) || hasSideEffects(rhs))
                {
                    continue; // expressions may not be equal
                }
                if (reflect.TypeOf(lhs) != reflect.TypeOf(rhs))
                {
                    continue; // short-circuit the heavy-weight gofmt check
                }
                var le = f.gofmt(lhs);
                var re = f.gofmt(rhs);
                if (le == re)
                {
                    f.Badf(stmt.Pos(), "self-assignment of %s to %s", re, le);
                }
            }
        }
    }
}
