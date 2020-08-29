// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains boolean condition tests.

// package main -- go2cs converted at 2020 August 29 10:08:47 UTC
// Original source: C:\Go\src\cmd\vet\bool.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("bool", "check for mistakes involving boolean operators", checkBool, binaryExpr);
        }

        private static void checkBool(ref File f, ast.Node n)
        {
            ref ast.BinaryExpr e = n._<ref ast.BinaryExpr>();

            boolOp op = default;

            if (e.Op == token.LOR) 
                op = or;
            else if (e.Op == token.LAND) 
                op = and;
            else 
                return;
                        var comm = op.commutativeSets(e);
            foreach (var (_, exprs) in comm)
            {
                op.checkRedundant(f, exprs);
                op.checkSuspect(f, exprs);
            }
        }

        private partial struct boolOp
        {
            public @string name;
            public token.Token tok; // token corresponding to this operator
            public token.Token badEq; // token corresponding to the equality test that should not be used with this operator
        }

        private static boolOp or = new boolOp("or",token.LOR,token.NEQ);        private static boolOp and = new boolOp("and",token.LAND,token.EQL);

        // commutativeSets returns all side effect free sets of
        // expressions in e that are connected by op.
        // For example, given 'a || b || f() || c || d' with the or op,
        // commutativeSets returns {{b, a}, {d, c}}.
        private static slice<slice<ast.Expr>> commutativeSets(this boolOp op, ref ast.BinaryExpr e)
        {
            var exprs = op.split(e); 

            // Partition the slice of expressions into commutative sets.
            long i = 0L;
            slice<slice<ast.Expr>> sets = default;
            for (long j = 0L; j <= len(exprs); j++)
            {
                if (j == len(exprs) || hasSideEffects(exprs[j]))
                {
                    if (i < j)
                    {
                        sets = append(sets, exprs[i..j]);
                    }
                    i = j + 1L;
                }
            }


            return sets;
        }

        // checkRedundant checks for expressions of the form
        //   e && e
        //   e || e
        // Exprs must contain only side effect free expressions.
        private static void checkRedundant(this boolOp op, ref File f, slice<ast.Expr> exprs)
        {
            var seen = make_map<@string, bool>();
            foreach (var (_, e) in exprs)
            {
                var efmt = f.gofmt(e);
                if (seen[efmt])
                {
                    f.Badf(e.Pos(), "redundant %s: %s %s %s", op.name, efmt, op.tok, efmt);
                }
                else
                {
                    seen[efmt] = true;
                }
            }
        }

        // checkSuspect checks for expressions of the form
        //   x != c1 || x != c2
        //   x == c1 && x == c2
        // where c1 and c2 are constant expressions.
        // If c1 and c2 are the same then it's redundant;
        // if c1 and c2 are different then it's always true or always false.
        // Exprs must contain only side effect free expressions.
        private static void checkSuspect(this boolOp op, ref File f, slice<ast.Expr> exprs)
        { 
            // seen maps from expressions 'x' to equality expressions 'x != c'.
            var seen = make_map<@string, @string>();

            foreach (var (_, e) in exprs)
            {
                ref ast.BinaryExpr (bin, ok) = e._<ref ast.BinaryExpr>();
                if (!ok || bin.Op != op.badEq)
                {
                    continue;
                } 

                // In order to avoid false positives, restrict to cases
                // in which one of the operands is constant. We're then
                // interested in the other operand.
                // In the rare case in which both operands are constant
                // (e.g. runtime.GOOS and "windows"), we'll only catch
                // mistakes if the LHS is repeated, which is how most
                // code is written.
                ast.Expr x = default;

                if (f.pkg.types[bin.Y].Value != null) 
                    x = bin.X;
                else if (f.pkg.types[bin.X].Value != null) 
                    x = bin.Y;
                else 
                    continue;
                // e is of the form 'x != c' or 'x == c'.
                var xfmt = f.gofmt(x);
                var efmt = f.gofmt(e);
                {
                    var (prev, found) = seen[xfmt];

                    if (found)
                    { 
                        // checkRedundant handles the case in which efmt == prev.
                        if (efmt != prev)
                        {
                            f.Badf(e.Pos(), "suspect %s: %s %s %s", op.name, efmt, op.tok, prev);
                        }
                    }
                    else
                    {
                        seen[xfmt] = efmt;
                    }

                }
            }
        }

        // hasSideEffects reports whether evaluation of e has side effects.
        private static bool hasSideEffects(ast.Expr e)
        {
            var safe = true;
            ast.Inspect(e, node =>
            {
                switch (node.type())
                {
                    case ref ast.CallExpr n:
                        safe = false;
                        return false;
                        break;
                    case ref ast.UnaryExpr n:
                        if (n.Op == token.ARROW)
                        {
                            safe = false;
                            return false;
                        }
                        break;
                }
                return true;
            });
            return !safe;
        }

        // split returns a slice of all subexpressions in e that are connected by op.
        // For example, given 'a || (b || c) || d' with the or op,
        // split returns []{d, c, b, a}.
        private static slice<ast.Expr> split(this boolOp op, ast.Expr e)
        {
            while (true)
            {
                e = unparen(e);
                {
                    ref ast.BinaryExpr (b, ok) = e._<ref ast.BinaryExpr>();

                    if (ok && b.Op == op.tok)
                    {
                        exprs = append(exprs, op.split(b.Y));
                        e = b.X;
                    }
                    else
                    {
                        exprs = append(exprs, e);
                        break;
                    }

                }
            }

            return;
        }

        // unparen returns e with any enclosing parentheses stripped.
        private static ast.Expr unparen(ast.Expr e)
        {
            while (true)
            {
                ref ast.ParenExpr (p, ok) = e._<ref ast.ParenExpr>();
                if (!ok)
                {
                    return e;
                }
                e = p.X;
            }

        }
    }
}
