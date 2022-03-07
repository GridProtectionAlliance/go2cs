// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements syntax tree walking.

// package syntax -- go2cs converted at 2022 March 06 23:13:44 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\syntax\walk.go
using fmt = go.fmt_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class syntax_package {

    // Walk traverses a syntax in pre-order: It starts by calling f(root);
    // root must not be nil. If f returns false (== "continue"), Walk calls
    // f recursively for each of the non-nil children of that node; if f
    // returns true (== "stop"), Walk does not traverse the respective node's
    // children.
    // Some nodes may be shared among multiple parent nodes (e.g., types in
    // field lists such as type T in "a, b, c T"). Such shared nodes are
    // walked multiple times.
    // TODO(gri) Revisit this design. It may make sense to walk those nodes
    //           only once. A place where this matters is types2.TestResolveIdents.
public static bool Walk(Node root, Func<Node, bool> f) {
    walker w = new walker(f);
    w.node(root);
}

private partial struct walker {
    public Func<Node, bool> f;
}

private static void node(this ptr<walker> _addr_w, Node n) => func((_, panic, _) => {
    ref walker w = ref _addr_w.val;

    if (n == null) {
        panic("invalid syntax tree: nil node");
    }
    if (w.f(n)) {
        return ;
    }
    switch (n.type()) {
        case ptr<File> n:
            w.node(n.PkgName);
            w.declList(n.DeclList); 

            // declarations
            break;
        case ptr<ImportDecl> n:
            if (n.LocalPkgName != null) {
                w.node(n.LocalPkgName);
            }
            w.node(n.Path);
            break;
        case ptr<ConstDecl> n:
            w.nameList(n.NameList);
            if (n.Type != null) {
                w.node(n.Type);
            }
            if (n.Values != null) {
                w.node(n.Values);
            }
            break;
        case ptr<TypeDecl> n:
            w.node(n.Name);
            w.fieldList(n.TParamList);
            w.node(n.Type);
            break;
        case ptr<VarDecl> n:
            w.nameList(n.NameList);
            if (n.Type != null) {
                w.node(n.Type);
            }
            if (n.Values != null) {
                w.node(n.Values);
            }
            break;
        case ptr<FuncDecl> n:
            if (n.Recv != null) {
                w.node(n.Recv);
            }
            w.node(n.Name);
            w.fieldList(n.TParamList);
            w.node(n.Type);
            if (n.Body != null) {
                w.node(n.Body);
            } 

            // expressions
            break;
        case ptr<BadExpr> n:
            break;
        case ptr<Name> n:
            break;
        case ptr<BasicLit> n:
            break;
        case ptr<CompositeLit> n:
            if (n.Type != null) {
                w.node(n.Type);
            }
            w.exprList(n.ElemList);
            break;
        case ptr<KeyValueExpr> n:
            w.node(n.Key);
            w.node(n.Value);
            break;
        case ptr<FuncLit> n:
            w.node(n.Type);
            w.node(n.Body);
            break;
        case ptr<ParenExpr> n:
            w.node(n.X);
            break;
        case ptr<SelectorExpr> n:
            w.node(n.X);
            w.node(n.Sel);
            break;
        case ptr<IndexExpr> n:
            w.node(n.X);
            w.node(n.Index);
            break;
        case ptr<SliceExpr> n:
            w.node(n.X);
            foreach (var (_, x) in n.Index) {
                if (x != null) {
                    w.node(x);
                }
            }
            break;
        case ptr<AssertExpr> n:
            w.node(n.X);
            w.node(n.Type);
            break;
        case ptr<TypeSwitchGuard> n:
            if (n.Lhs != null) {
                w.node(n.Lhs);
            }
            w.node(n.X);
            break;
        case ptr<Operation> n:
            w.node(n.X);
            if (n.Y != null) {
                w.node(n.Y);
            }
            break;
        case ptr<CallExpr> n:
            w.node(n.Fun);
            w.exprList(n.ArgList);
            break;
        case ptr<ListExpr> n:
            w.exprList(n.ElemList); 

            // types
            break;
        case ptr<ArrayType> n:
            if (n.Len != null) {
                w.node(n.Len);
            }
            w.node(n.Elem);
            break;
        case ptr<SliceType> n:
            w.node(n.Elem);
            break;
        case ptr<DotsType> n:
            w.node(n.Elem);
            break;
        case ptr<StructType> n:
            w.fieldList(n.FieldList);
            foreach (var (_, t) in n.TagList) {
                if (t != null) {
                    w.node(t);
                }
            }
            break;
        case ptr<Field> n:
            if (n.Name != null) {
                w.node(n.Name);
            }
            w.node(n.Type);
            break;
        case ptr<InterfaceType> n:
            w.fieldList(n.MethodList);
            break;
        case ptr<FuncType> n:
            w.fieldList(n.ParamList);
            w.fieldList(n.ResultList);
            break;
        case ptr<MapType> n:
            w.node(n.Key);
            w.node(n.Value);
            break;
        case ptr<ChanType> n:
            w.node(n.Elem); 

            // statements
            break;
        case ptr<EmptyStmt> n:
            break;
        case ptr<LabeledStmt> n:
            w.node(n.Label);
            w.node(n.Stmt);
            break;
        case ptr<BlockStmt> n:
            w.stmtList(n.List);
            break;
        case ptr<ExprStmt> n:
            w.node(n.X);
            break;
        case ptr<SendStmt> n:
            w.node(n.Chan);
            w.node(n.Value);
            break;
        case ptr<DeclStmt> n:
            w.declList(n.DeclList);
            break;
        case ptr<AssignStmt> n:
            w.node(n.Lhs);
            if (n.Rhs != null) {
                w.node(n.Rhs);
            }
            break;
        case ptr<BranchStmt> n:
            if (n.Label != null) {
                w.node(n.Label);
            } 
            // Target points to nodes elsewhere in the syntax tree
            break;
        case ptr<CallStmt> n:
            w.node(n.Call);
            break;
        case ptr<ReturnStmt> n:
            if (n.Results != null) {
                w.node(n.Results);
            }
            break;
        case ptr<IfStmt> n:
            if (n.Init != null) {
                w.node(n.Init);
            }
            w.node(n.Cond);
            w.node(n.Then);
            if (n.Else != null) {
                w.node(n.Else);
            }
            break;
        case ptr<ForStmt> n:
            if (n.Init != null) {
                w.node(n.Init);
            }
            if (n.Cond != null) {
                w.node(n.Cond);
            }
            if (n.Post != null) {
                w.node(n.Post);
            }
            w.node(n.Body);
            break;
        case ptr<SwitchStmt> n:
            if (n.Init != null) {
                w.node(n.Init);
            }
            if (n.Tag != null) {
                w.node(n.Tag);
            }
            {
                var s__prev1 = s;

                foreach (var (_, __s) in n.Body) {
                    s = __s;
                    w.node(s);
                }

                s = s__prev1;
            }
            break;
        case ptr<SelectStmt> n:
            {
                var s__prev1 = s;

                foreach (var (_, __s) in n.Body) {
                    s = __s;
                    w.node(s);
                } 

                // helper nodes

                s = s__prev1;
            }
            break;
        case ptr<RangeClause> n:
            if (n.Lhs != null) {
                w.node(n.Lhs);
            }
            w.node(n.X);
            break;
        case ptr<CaseClause> n:
            if (n.Cases != null) {
                w.node(n.Cases);
            }
            w.stmtList(n.Body);
            break;
        case ptr<CommClause> n:
            if (n.Comm != null) {
                w.node(n.Comm);
            }
            w.stmtList(n.Body);
            break;
        default:
        {
            var n = n.type();
            panic(fmt.Sprintf("internal error: unknown node type %T", n));
            break;
        }
    }

});

private static void declList(this ptr<walker> _addr_w, slice<Decl> list) {
    ref walker w = ref _addr_w.val;

    foreach (var (_, n) in list) {
        w.node(n);
    }
}

private static void exprList(this ptr<walker> _addr_w, slice<Expr> list) {
    ref walker w = ref _addr_w.val;

    foreach (var (_, n) in list) {
        w.node(n);
    }
}

private static void stmtList(this ptr<walker> _addr_w, slice<Stmt> list) {
    ref walker w = ref _addr_w.val;

    foreach (var (_, n) in list) {
        w.node(n);
    }
}

private static void nameList(this ptr<walker> _addr_w, slice<ptr<Name>> list) {
    ref walker w = ref _addr_w.val;

    foreach (var (_, n) in list) {
        w.node(n);
    }
}

private static void fieldList(this ptr<walker> _addr_w, slice<ptr<Field>> list) {
    ref walker w = ref _addr_w.val;

    foreach (var (_, n) in list) {
        w.node(n);
    }
}

} // end syntax_package
