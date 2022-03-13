// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build typeparams
// +build typeparams

// package ast -- go2cs converted at 2022 March 13 05:54:10 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Program Files\Go\src\go\ast\walk_typeparams.go
namespace go.go;

using fmt = fmt_package;

public static partial class ast_package {

private static void walkFuncTypeParams(Visitor v, ptr<FuncType> _addr_n) {
    ref FuncType n = ref _addr_n.val;

    if (n.TParams != null) {
        Walk(v, n.TParams);
    }
}

private static void walkTypeSpecParams(Visitor v, ptr<TypeSpec> _addr_n) {
    ref TypeSpec n = ref _addr_n.val;

    if (n.TParams != null) {
        Walk(v, n.TParams);
    }
}

private static void walkOtherNodes(Visitor v, Node n) => func((_, panic, _) => {
    {
        ptr<ListExpr> (e, ok) = n._<ptr<ListExpr>>();

        if (ok) {
            if (e != null) {
                foreach (var (_, elem) in e.ElemList) {
                    Walk(v, elem);
                }
            }
        else
        } {
            panic(fmt.Sprintf("ast.Walk: unexpected node type %T", n));
        }
    }
});

} // end ast_package
