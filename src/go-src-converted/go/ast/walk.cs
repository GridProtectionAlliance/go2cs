// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ast -- go2cs converted at 2020 August 29 08:48:37 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Go\src\go\ast\walk.go
using fmt = go.fmt_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class ast_package
    {
        // A Visitor's Visit method is invoked for each node encountered by Walk.
        // If the result visitor w is not nil, Walk visits each of the children
        // of node with the visitor w, followed by a call of w.Visit(nil).
        public partial interface Visitor
        {
            Visitor Visit(Node node);
        }

        // Helper functions for common node lists. They may be empty.

        private static void walkIdentList(Visitor v, slice<ref Ident> list)
        {
            foreach (var (_, x) in list)
            {
                Walk(v, x);
            }
        }

        private static void walkExprList(Visitor v, slice<Expr> list)
        {
            foreach (var (_, x) in list)
            {
                Walk(v, x);
            }
        }

        private static void walkStmtList(Visitor v, slice<Stmt> list)
        {
            foreach (var (_, x) in list)
            {
                Walk(v, x);
            }
        }

        private static void walkDeclList(Visitor v, slice<Decl> list)
        {
            foreach (var (_, x) in list)
            {
                Walk(v, x);
            }
        }

        // TODO(gri): Investigate if providing a closure to Walk leads to
        //            simpler use (and may help eliminate Inspect in turn).

        // Walk traverses an AST in depth-first order: It starts by calling
        // v.Visit(node); node must not be nil. If the visitor w returned by
        // v.Visit(node) is not nil, Walk is invoked recursively with visitor
        // w for each of the non-nil children of node, followed by a call of
        // w.Visit(nil).
        //
        public static void Walk(Visitor v, Node node) => func((_, panic, __) =>
        {
            v = v.Visit(node);

            if (v == null)
            {
                return;
            } 

            // walk children
            // (the order of the cases matches the order
            // of the corresponding node types in ast.go)
            switch (node.type())
            {
                case ref Comment n:
                    break;
                case ref CommentGroup n:
                    foreach (var (_, c) in n.List)
                    {
                        Walk(v, c);
                    }
                    break;
                case ref Field n:
                    if (n.Doc != null)
                    {
                        Walk(v, n.Doc);
                    }
                    walkIdentList(v, n.Names);
                    Walk(v, n.Type);
                    if (n.Tag != null)
                    {
                        Walk(v, n.Tag);
                    }
                    if (n.Comment != null)
                    {
                        Walk(v, n.Comment);
                    }
                    break;
                case ref FieldList n:
                    {
                        var f__prev1 = f;

                        foreach (var (_, __f) in n.List)
                        {
                            f = __f;
                            Walk(v, f);
                        } 

                        // Expressions

                        f = f__prev1;
                    }
                    break;
                case ref BadExpr n:
                    break;
                case ref Ident n:
                    break;
                case ref BasicLit n:
                    break;
                case ref Ellipsis n:
                    if (n.Elt != null)
                    {
                        Walk(v, n.Elt);
                    }
                    break;
                case ref FuncLit n:
                    Walk(v, n.Type);
                    Walk(v, n.Body);
                    break;
                case ref CompositeLit n:
                    if (n.Type != null)
                    {
                        Walk(v, n.Type);
                    }
                    walkExprList(v, n.Elts);
                    break;
                case ref ParenExpr n:
                    Walk(v, n.X);
                    break;
                case ref SelectorExpr n:
                    Walk(v, n.X);
                    Walk(v, n.Sel);
                    break;
                case ref IndexExpr n:
                    Walk(v, n.X);
                    Walk(v, n.Index);
                    break;
                case ref SliceExpr n:
                    Walk(v, n.X);
                    if (n.Low != null)
                    {
                        Walk(v, n.Low);
                    }
                    if (n.High != null)
                    {
                        Walk(v, n.High);
                    }
                    if (n.Max != null)
                    {
                        Walk(v, n.Max);
                    }
                    break;
                case ref TypeAssertExpr n:
                    Walk(v, n.X);
                    if (n.Type != null)
                    {
                        Walk(v, n.Type);
                    }
                    break;
                case ref CallExpr n:
                    Walk(v, n.Fun);
                    walkExprList(v, n.Args);
                    break;
                case ref StarExpr n:
                    Walk(v, n.X);
                    break;
                case ref UnaryExpr n:
                    Walk(v, n.X);
                    break;
                case ref BinaryExpr n:
                    Walk(v, n.X);
                    Walk(v, n.Y);
                    break;
                case ref KeyValueExpr n:
                    Walk(v, n.Key);
                    Walk(v, n.Value); 

                    // Types
                    break;
                case ref ArrayType n:
                    if (n.Len != null)
                    {
                        Walk(v, n.Len);
                    }
                    Walk(v, n.Elt);
                    break;
                case ref StructType n:
                    Walk(v, n.Fields);
                    break;
                case ref FuncType n:
                    if (n.Params != null)
                    {
                        Walk(v, n.Params);
                    }
                    if (n.Results != null)
                    {
                        Walk(v, n.Results);
                    }
                    break;
                case ref InterfaceType n:
                    Walk(v, n.Methods);
                    break;
                case ref MapType n:
                    Walk(v, n.Key);
                    Walk(v, n.Value);
                    break;
                case ref ChanType n:
                    Walk(v, n.Value); 

                    // Statements
                    break;
                case ref BadStmt n:
                    break;
                case ref DeclStmt n:
                    Walk(v, n.Decl);
                    break;
                case ref EmptyStmt n:
                    break;
                case ref LabeledStmt n:
                    Walk(v, n.Label);
                    Walk(v, n.Stmt);
                    break;
                case ref ExprStmt n:
                    Walk(v, n.X);
                    break;
                case ref SendStmt n:
                    Walk(v, n.Chan);
                    Walk(v, n.Value);
                    break;
                case ref IncDecStmt n:
                    Walk(v, n.X);
                    break;
                case ref AssignStmt n:
                    walkExprList(v, n.Lhs);
                    walkExprList(v, n.Rhs);
                    break;
                case ref GoStmt n:
                    Walk(v, n.Call);
                    break;
                case ref DeferStmt n:
                    Walk(v, n.Call);
                    break;
                case ref ReturnStmt n:
                    walkExprList(v, n.Results);
                    break;
                case ref BranchStmt n:
                    if (n.Label != null)
                    {
                        Walk(v, n.Label);
                    }
                    break;
                case ref BlockStmt n:
                    walkStmtList(v, n.List);
                    break;
                case ref IfStmt n:
                    if (n.Init != null)
                    {
                        Walk(v, n.Init);
                    }
                    Walk(v, n.Cond);
                    Walk(v, n.Body);
                    if (n.Else != null)
                    {
                        Walk(v, n.Else);
                    }
                    break;
                case ref CaseClause n:
                    walkExprList(v, n.List);
                    walkStmtList(v, n.Body);
                    break;
                case ref SwitchStmt n:
                    if (n.Init != null)
                    {
                        Walk(v, n.Init);
                    }
                    if (n.Tag != null)
                    {
                        Walk(v, n.Tag);
                    }
                    Walk(v, n.Body);
                    break;
                case ref TypeSwitchStmt n:
                    if (n.Init != null)
                    {
                        Walk(v, n.Init);
                    }
                    Walk(v, n.Assign);
                    Walk(v, n.Body);
                    break;
                case ref CommClause n:
                    if (n.Comm != null)
                    {
                        Walk(v, n.Comm);
                    }
                    walkStmtList(v, n.Body);
                    break;
                case ref SelectStmt n:
                    Walk(v, n.Body);
                    break;
                case ref ForStmt n:
                    if (n.Init != null)
                    {
                        Walk(v, n.Init);
                    }
                    if (n.Cond != null)
                    {
                        Walk(v, n.Cond);
                    }
                    if (n.Post != null)
                    {
                        Walk(v, n.Post);
                    }
                    Walk(v, n.Body);
                    break;
                case ref RangeStmt n:
                    if (n.Key != null)
                    {
                        Walk(v, n.Key);
                    }
                    if (n.Value != null)
                    {
                        Walk(v, n.Value);
                    }
                    Walk(v, n.X);
                    Walk(v, n.Body); 

                    // Declarations
                    break;
                case ref ImportSpec n:
                    if (n.Doc != null)
                    {
                        Walk(v, n.Doc);
                    }
                    if (n.Name != null)
                    {
                        Walk(v, n.Name);
                    }
                    Walk(v, n.Path);
                    if (n.Comment != null)
                    {
                        Walk(v, n.Comment);
                    }
                    break;
                case ref ValueSpec n:
                    if (n.Doc != null)
                    {
                        Walk(v, n.Doc);
                    }
                    walkIdentList(v, n.Names);
                    if (n.Type != null)
                    {
                        Walk(v, n.Type);
                    }
                    walkExprList(v, n.Values);
                    if (n.Comment != null)
                    {
                        Walk(v, n.Comment);
                    }
                    break;
                case ref TypeSpec n:
                    if (n.Doc != null)
                    {
                        Walk(v, n.Doc);
                    }
                    Walk(v, n.Name);
                    Walk(v, n.Type);
                    if (n.Comment != null)
                    {
                        Walk(v, n.Comment);
                    }
                    break;
                case ref BadDecl n:
                    break;
                case ref GenDecl n:
                    if (n.Doc != null)
                    {
                        Walk(v, n.Doc);
                    }
                    foreach (var (_, s) in n.Specs)
                    {
                        Walk(v, s);
                    }
                    break;
                case ref FuncDecl n:
                    if (n.Doc != null)
                    {
                        Walk(v, n.Doc);
                    }
                    if (n.Recv != null)
                    {
                        Walk(v, n.Recv);
                    }
                    Walk(v, n.Name);
                    Walk(v, n.Type);
                    if (n.Body != null)
                    {
                        Walk(v, n.Body);
                    } 

                    // Files and packages
                    break;
                case ref File n:
                    if (n.Doc != null)
                    {
                        Walk(v, n.Doc);
                    }
                    Walk(v, n.Name);
                    walkDeclList(v, n.Decls); 
                    // don't walk n.Comments - they have been
                    // visited already through the individual
                    // nodes
                    break;
                case ref Package n:
                    {
                        var f__prev1 = f;

                        foreach (var (_, __f) in n.Files)
                        {
                            f = __f;
                            Walk(v, f);
                        }

                        f = f__prev1;
                    }
                    break;
                default:
                {
                    var n = node.type();
                    panic(fmt.Sprintf("ast.Walk: unexpected node type %T", n));
                    break;
                }

            }

            v.Visit(null);
        });

        public delegate  bool inspector(Node);

        private static Visitor Visit(this inspector f, Node node)
        {
            if (f(node))
            {
                return f;
            }
            return null;
        }

        // Inspect traverses an AST in depth-first order: It starts by calling
        // f(node); node must not be nil. If f returns true, Inspect invokes f
        // recursively for each of the non-nil children of node, followed by a
        // call of f(nil).
        //
        public static bool Inspect(Node node, Func<Node, bool> f)
        {
            Walk(inspector(f), node);
        }
    }
}}
