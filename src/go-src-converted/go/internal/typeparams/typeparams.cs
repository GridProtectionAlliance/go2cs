// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build typeparams
// +build typeparams

// package typeparams -- go2cs converted at 2022 March 13 05:52:50 UTC
// import "go/internal/typeparams" ==> using typeparams = go.go.@internal.typeparams_package
// Original source: C:\Program Files\Go\src\go\internal\typeparams\typeparams.go
namespace go.go.@internal;

using fmt = fmt_package;
using ast = go.ast_package;

public static partial class typeparams_package {

public static readonly var Enabled = true;



public static ast.Expr PackExpr(slice<ast.Expr> list) {
    switch (len(list)) {
        case 0: 
            // Return an empty ListExpr here, rather than nil, as IndexExpr.Index must
            // never be nil.
            // TODO(rFindley) would a BadExpr be more appropriate here?
            return addr(new ast.ListExpr());
            break;
        case 1: 
            return list[0];
            break;
        default: 
            return addr(new ast.ListExpr(ElemList:list));
            break;
    }
}

// TODO(gri) Should find a more efficient solution that doesn't
//           require introduction of a new slice for simple
//           expressions.
public static slice<ast.Expr> UnpackExpr(ast.Expr x) {
    {
        ptr<ast.ListExpr> (x, _) = x._<ptr<ast.ListExpr>>();

        if (x != null) {
            return x.ElemList;
        }
    }
    if (x != null) {
        return new slice<ast.Expr>(new ast.Expr[] { x });
    }
    return null;
}

public static bool IsListExpr(ast.Node n) {
    ptr<ast.ListExpr> (_, ok) = n._<ptr<ast.ListExpr>>();
    return ok;
}

public static ptr<ast.FieldList> Get(ast.Node n) => func((_, panic, _) => {
    switch (n.type()) {
        case ptr<ast.TypeSpec> n:
            return _addr_n.TParams!;
            break;
        case ptr<ast.FuncType> n:
            return _addr_n.TParams!;
            break;
        default:
        {
            var n = n.type();
            panic(fmt.Sprintf("node type %T has no type parameters", n));
            break;
        }
    }
});

public static void Set(ast.Node n, ptr<ast.FieldList> _addr_@params) => func((_, panic, _) => {
    ref ast.FieldList @params = ref _addr_@params.val;

    switch (n.type()) {
        case ptr<ast.TypeSpec> n:
            n.TParams = params;
            break;
        case ptr<ast.FuncType> n:
            n.TParams = params;
            break;
        default:
        {
            var n = n.type();
            panic(fmt.Sprintf("node type %T has no type parameters", n));
            break;
        }
    }
});

} // end typeparams_package
