// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

/*
This file contains the code to check for suspicious shifts.
*/

// package main -- go2cs converted at 2020 August 29 10:09:29 UTC
// Original source: C:\Go\src\cmd\vet\shift.go
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("shift", "check for useless shifts", checkShift, binaryExpr, assignStmt);
        }

        private static void checkShift(ref File f, ast.Node node)
        {
            if (f.dead[node])
            { 
                // Skip shift checks on unreachable nodes.
                return;
            }
            switch (node.type())
            {
                case ref ast.BinaryExpr node:
                    if (node.Op == token.SHL || node.Op == token.SHR)
                    {
                        checkLongShift(f, node, node.X, node.Y);
                    }
                    break;
                case ref ast.AssignStmt node:
                    if (len(node.Lhs) != 1L || len(node.Rhs) != 1L)
                    {
                        return;
                    }
                    if (node.Tok == token.SHL_ASSIGN || node.Tok == token.SHR_ASSIGN)
                    {
                        checkLongShift(f, node, node.Lhs[0L], node.Rhs[0L]);
                    }
                    break;
            }
        }

        // checkLongShift checks if shift or shift-assign operations shift by more than
        // the length of the underlying variable.
        private static void checkLongShift(ref File f, ast.Node node, ast.Expr x, ast.Expr y)
        {
            if (f.pkg.types[x].Value != null)
            { 
                // Ignore shifts of constants.
                // These are frequently used for bit-twiddling tricks
                // like ^uint(0) >> 63 for 32/64 bit detection and compatibility.
                return;
            }
            var v = f.pkg.types[y].Value;
            if (v == null)
            {
                return;
            }
            var (amt, ok) = constant.Int64Val(v);
            if (!ok)
            {
                return;
            }
            var t = f.pkg.types[x].Type;
            if (t == null)
            {
                return;
            }
            ref types.Basic (b, ok) = t.Underlying()._<ref types.Basic>();
            if (!ok)
            {
                return;
            }
            long size = default;

            if (b.Kind() == types.Uint8 || b.Kind() == types.Int8) 
                size = 8L;
            else if (b.Kind() == types.Uint16 || b.Kind() == types.Int16) 
                size = 16L;
            else if (b.Kind() == types.Uint32 || b.Kind() == types.Int32) 
                size = 32L;
            else if (b.Kind() == types.Uint64 || b.Kind() == types.Int64) 
                size = 64L;
            else if (b.Kind() == types.Int || b.Kind() == types.Uint) 
                size = uintBitSize;
            else if (b.Kind() == types.Uintptr) 
                size = uintptrBitSize;
            else 
                return;
                        if (amt >= size)
            {
                var ident = f.gofmt(x);
                f.Badf(node.Pos(), "%s (%d bits) too small for shift of %d", ident, size, amt);
            }
        }

        private static long uintBitSize = 8L * archSizes.Sizeof(types.Typ[types.Uint]);        private static long uintptrBitSize = 8L * archSizes.Sizeof(types.Typ[types.Uintptr]);
    }
}
