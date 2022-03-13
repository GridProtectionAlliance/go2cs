// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !typeparams
// +build !typeparams

// package typeparams -- go2cs converted at 2022 March 13 05:52:49 UTC
// import "go/internal/typeparams" ==> using typeparams = go.go.@internal.typeparams_package
// Original source: C:\Program Files\Go\src\go\internal\typeparams\notypeparams.go
namespace go.go.@internal;

using ast = go.ast_package;

public static partial class typeparams_package {

public static readonly var Enabled = false;



public static ast.Expr PackExpr(slice<ast.Expr> list) => func((_, panic, _) => {
    switch (len(list)) {
        case 1: 
            return list[0];
            break;
        default: 
            // The parser should not attempt to pack multiple expressions into an
            // IndexExpr if type params are disabled.
            panic("multiple index expressions are unsupported without type params");
            break;
    }
});

public static slice<ast.Expr> UnpackExpr(ast.Expr expr) {
    return new slice<ast.Expr>(new ast.Expr[] { expr });
}

public static bool IsListExpr(ast.Node n) {
    return false;
}

public static ptr<ast.FieldList> Get(ast.Node _p0) {
    return _addr_null!;
}

public static void Set(ast.Node node, ptr<ast.FieldList> _addr_@params) {
    ref ast.FieldList @params = ref _addr_@params.val;

}

} // end typeparams_package
