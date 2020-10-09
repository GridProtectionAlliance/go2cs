// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ast -- go2cs converted at 2020 October 09 05:20:07 UTC
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

        private static void walkIdentList(Visitor v, slice<ptr<Ident>> list)
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
                return ;
            } 

            // walk children
            // (the order of the cases matches the order
            // of the corresponding node types in ast.go)
            switch (node.type())
            {
                case ptr<Comment> n:
                    break;
                case ptr<CommentGroup> n:
                    foreach (var (_, c) in n.List)
                    {
                        Walk(v, c);
                    }
                    break;
                case ptr<Field> n:
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
                case ptr<FieldList> n:
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
                case ptr<BadExpr> n:
                    break;
                case ptr<Ident> n:
                    break;
                case ptr<BasicLit> n:
                    break;
                case ptr<Ellipsis> n:
                    if (n.Elt != null)
                    {
                        Walk(v, n.Elt);
                    }

                    break;
                case ptr<FuncLit> n:
                    Walk(v, n.Type);
                    Walk(v, n.Body);
                    break;
                case ptr<CompositeLit> n:
                    if (n.Type != null)
                    {
                        Walk(v, n.Type);
                    }

                    walkExprList(v, n.Elts);
                    break;
                case ptr<ParenExpr> n:
                    Walk(v, n.X);
                    break;
                case ptr<SelectorExpr> n:
                    Walk(v, n.X);
                    Walk(v, n.Sel);
                    break;
                case ptr<IndexExpr> n:
                    Walk(v, n.X);
                    Walk(v, n.Index);
                    break;
                case ptr<SliceExpr> n:
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
                case ptr<TypeAssertExpr> n:
                    Walk(v, n.X);
                    if (n.Type != null)
                    {
                        Walk(v, n.Type);
                    }

                    break;
                case ptr<CallExpr> n:
                    Walk(v, n.Fun);
                    walkExprList(v, n.Args);
                    break;
                case ptr<StarExpr> n:
                    Walk(v, n.X);
                    break;
                case ptr<UnaryExpr> n:
                    Walk(v, n.X);
                    break;
                case ptr<BinaryExpr> n:
                    Walk(v, n.X);
                    Walk(v, n.Y);
                    break;
                case ptr<KeyValueExpr> n:
                    Walk(v, n.Key);
                    Walk(v, n.Value); 

                    // Types
                    break;
                case ptr<ArrayType> n:
                    if (n.Len != null)
                    {
                        Walk(v, n.Len);
                    }

                    Walk(v, n.Elt);
                    break;
                case ptr<StructType> n:
                    Walk(v, n.Fields);
                    break;
                case ptr<FuncType> n:
                    if (n.Params != null)
                    {
                        Walk(v, n.Params);
                    }

                    if (n.Results != null)
                    {
                        Walk(v, n.Results);
                    }

                    break;
                case ptr<InterfaceType> n:
                    Walk(v, n.Methods);
                    break;
                case ptr<MapType> n:
                    Walk(v, n.Key);
                    Walk(v, n.Value);
                    break;
                case ptr<ChanType> n:
                    Walk(v, n.Value); 

                    // Statements
                    break;
                case ptr<BadStmt> n:
                    break;
                case ptr<DeclStmt> n:
                    Walk(v, n.Decl);
                    break;
                case ptr<EmptyStmt> n:
                    break;
                case ptr<LabeledStmt> n:
                    Walk(v, n.Label);
                    Walk(v, n.Stmt);
                    break;
                case ptr<ExprStmt> n:
                    Walk(v, n.X);
                    break;
                case ptr<SendStmt> n:
                    Walk(v, n.Chan);
                    Walk(v, n.Value);
                    break;
                case ptr<IncDecStmt> n:
                    Walk(v, n.X);
                    break;
                case ptr<AssignStmt> n:
                    walkExprList(v, n.Lhs);
                    walkExprList(v, n.Rhs);
                    break;
                case ptr<GoStmt> n:
                    Walk(v, n.Call);
                    break;
                case ptr<DeferStmt> n:
                    Walk(v, n.Call);
                    break;
                case ptr<ReturnStmt> n:
                    walkExprList(v, n.Results);
                    break;
                case ptr<BranchStmt> n:
                    if (n.Label != null)
                    {
                        Walk(v, n.Label);
                    }

                    break;
                case ptr<BlockStmt> n:
                    walkStmtList(v, n.List);
                    break;
                case ptr<IfStmt> n:
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
                case ptr<CaseClause> n:
                    walkExprList(v, n.List);
                    walkStmtList(v, n.Body);
                    break;
                case ptr<SwitchStmt> n:
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
                case ptr<TypeSwitchStmt> n:
                    if (n.Init != null)
                    {
                        Walk(v, n.Init);
                    }

                    Walk(v, n.Assign);
                    Walk(v, n.Body);
                    break;
                case ptr<CommClause> n:
                    if (n.Comm != null)
                    {
                        Walk(v, n.Comm);
                    }

                    walkStmtList(v, n.Body);
                    break;
                case ptr<SelectStmt> n:
                    Walk(v, n.Body);
                    break;
                case ptr<ForStmt> n:
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
                case ptr<RangeStmt> n:
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
                case ptr<ImportSpec> n:
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
                case ptr<ValueSpec> n:
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
                case ptr<TypeSpec> n:
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
                case ptr<BadDecl> n:
                    break;
                case ptr<GenDecl> n:
                    if (n.Doc != null)
                    {
                        Walk(v, n.Doc);
                    }

                    foreach (var (_, s) in n.Specs)
                    {
                        Walk(v, s);
                    }
                    break;
                case ptr<FuncDecl> n:
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
                case ptr<File> n:
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
                case ptr<Package> n:
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
