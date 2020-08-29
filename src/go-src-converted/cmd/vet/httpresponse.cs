// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file contains the check for http.Response values being used before
// checking for errors.

// package main -- go2cs converted at 2020 August 29 10:08:53 UTC
// Original source: C:\Go\src\cmd\vet\httpresponse.go
using ast = go.go.ast_package;
using types = go.go.types_package;
using static go.builtin;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("httpresponse", "check errors are checked before using an http Response", checkHTTPResponse, callExpr);
        }

        private static void checkHTTPResponse(ref File f, ast.Node node)
        {
            ref ast.CallExpr call = node._<ref ast.CallExpr>();
            if (!isHTTPFuncOrMethodOnClient(f, call))
            {
                return; // the function call is not related to this check.
            }
            blockStmtFinder finder = ref new blockStmtFinder(node:call);
            ast.Walk(finder, f.file);
            var stmts = finder.stmts();
            if (len(stmts) < 2L)
            {
                return; // the call to the http function is the last statement of the block.
            }
            ref ast.AssignStmt (asg, ok) = stmts[0L]._<ref ast.AssignStmt>();
            if (!ok)
            {
                return; // the first statement is not assignment.
            }
            var resp = rootIdent(asg.Lhs[0L]);
            if (resp == null)
            {
                return; // could not find the http.Response in the assignment.
            }
            ref ast.DeferStmt (def, ok) = stmts[1L]._<ref ast.DeferStmt>();
            if (!ok)
            {
                return; // the following statement is not a defer.
            }
            var root = rootIdent(def.Call.Fun);
            if (root == null)
            {
                return; // could not find the receiver of the defer call.
            }
            if (resp.Obj == root.Obj)
            {
                f.Badf(root.Pos(), "using %s before checking for errors", resp.Name);
            }
        }

        // isHTTPFuncOrMethodOnClient checks whether the given call expression is on
        // either a function of the net/http package or a method of http.Client that
        // returns (*http.Response, error).
        private static bool isHTTPFuncOrMethodOnClient(ref File f, ref ast.CallExpr expr)
        {
            ref ast.SelectorExpr (fun, _) = expr.Fun._<ref ast.SelectorExpr>();
            ref types.Signature (sig, _) = f.pkg.types[fun].Type._<ref types.Signature>();
            if (sig == null)
            {
                return false; // the call is not on of the form x.f()
            }
            var res = sig.Results();
            if (res.Len() != 2L)
            {
                return false; // the function called does not return two values.
            }
            {
                ref types.Pointer ptr__prev1 = ptr;

                ref types.Pointer (ptr, ok) = res.At(0L).Type()._<ref types.Pointer>();

                if (!ok || !isNamedType(ptr.Elem(), "net/http", "Response"))
                {
                    return false; // the first return type is not *http.Response.
                }

                ptr = ptr__prev1;

            }
            if (!types.Identical(res.At(1L).Type().Underlying(), errorType))
            {
                return false; // the second return type is not error
            }
            var typ = f.pkg.types[fun.X].Type;
            if (typ == null)
            {
                ref ast.Ident (id, ok) = fun.X._<ref ast.Ident>();
                return ok && id.Name == "http"; // function in net/http package.
            }
            if (isNamedType(typ, "net/http", "Client"))
            {
                return true; // method on http.Client.
            }
            (ptr, ok) = typ._<ref types.Pointer>();
            return ok && isNamedType(ptr.Elem(), "net/http", "Client"); // method on *http.Client.
        }

        // blockStmtFinder is an ast.Visitor that given any ast node can find the
        // statement containing it and its succeeding statements in the same block.
        private partial struct blockStmtFinder
        {
            public ast.Node node; // target of search
            public ast.Stmt stmt; // innermost statement enclosing argument to Visit
            public ptr<ast.BlockStmt> block; // innermost block enclosing argument to Visit.
        }

        // Visit finds f.node performing a search down the ast tree.
        // It keeps the last block statement and statement seen for later use.
        private static ast.Visitor Visit(this ref blockStmtFinder f, ast.Node node)
        {
            if (node == null || f.node.Pos() < node.Pos() || f.node.End() > node.End())
            {
                return null; // not here
            }
            switch (node.type())
            {
                case ref ast.BlockStmt n:
                    f.block = n;
                    break;
                case ast.Stmt n:
                    f.stmt = n;
                    break;
            }
            if (f.node.Pos() == node.Pos() && f.node.End() == node.End())
            {
                return null; // found
            }
            return f; // keep looking
        }

        // stmts returns the statements of f.block starting from the one including f.node.
        private static slice<ast.Stmt> stmts(this ref blockStmtFinder f)
        {
            foreach (var (i, v) in f.block.List)
            {
                if (f.stmt == v)
                {
                    return f.block.List[i..];
                }
            }
            return null;
        }

        // rootIdent finds the root identifier x in a chain of selections x.y.z, or nil if not found.
        private static ref ast.Ident rootIdent(ast.Node n)
        {
            switch (n.type())
            {
                case ref ast.SelectorExpr n:
                    return rootIdent(n.X);
                    break;
                case ref ast.Ident n:
                    return n;
                    break;
                default:
                {
                    var n = n.type();
                    return null;
                    break;
                }
            }
        }
    }
}
