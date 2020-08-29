// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
This file contains the code to check for useless function comparisons.
A useless comparison is one like f == nil as opposed to f() == nil.
*/

// package main -- go2cs converted at 2020 August 29 10:09:24 UTC
// Original source: C:\Go\src\cmd\vet\nilfunc.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("nilfunc", "check for comparisons between functions and nil", checkNilFuncComparison, binaryExpr);
        }

        private static void checkNilFuncComparison(ref File f, ast.Node node)
        {
            ref ast.BinaryExpr e = node._<ref ast.BinaryExpr>(); 

            // Only want == or != comparisons.
            if (e.Op != token.EQL && e.Op != token.NEQ)
            {
                return;
            } 

            // Only want comparisons with a nil identifier on one side.
            ast.Expr e2 = default;

            if (f.isNil(e.X)) 
                e2 = e.Y;
            else if (f.isNil(e.Y)) 
                e2 = e.X;
            else 
                return;
            // Only want identifiers or selector expressions.
            types.Object obj = default;
            switch (e2.type())
            {
                case ref ast.Ident v:
                    obj = f.pkg.uses[v];
                    break;
                case ref ast.SelectorExpr v:
                    obj = f.pkg.uses[v.Sel];
                    break;
                default:
                {
                    var v = e2.type();
                    return;
                    break;
                } 

                // Only want functions.
            } 

            // Only want functions.
            {
                ref types.Func (_, ok) = obj._<ref types.Func>();

                if (!ok)
                {
                    return;
                }

            }

            f.Badf(e.Pos(), "comparison of function %v %v nil is always %v", obj.Name(), e.Op, e.Op == token.NEQ);
        }

        // isNil reports whether the provided expression is the built-in nil
        // identifier.
        private static bool isNil(this ref File f, ast.Expr e)
        {
            return f.pkg.types[e].Type == types.Typ[types.UntypedNil];
        }
    }
}
