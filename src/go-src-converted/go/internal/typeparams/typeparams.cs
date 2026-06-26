// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go.@internal;

using ast = go.ast_package;
using token = go.token_package;
using go;

partial class typeparams_package {

public static ast.Expr PackIndexExpr(ast.Expr x, tokenꓸPos lbrack, slice<ast.Expr> exprs, tokenꓸPos rbrack) {
    switch (len(exprs)) {
    case 0: {
        throw panic("internal error: PackIndexExpr with empty expr slice");
        break;
    }
    case 1: {
        return new ast.IndexExpr(
            X: x,
            Lbrack: lbrack,
            Index: exprs[0],
            Rbrack: rbrack
        );
    }
    default: {
        return new ast.IndexListExpr(
            X: x,
            Lbrack: lbrack,
            Indices: exprs,
            Rbrack: rbrack
        );
    }}

}

// IndexExpr wraps an ast.IndexExpr or ast.IndexListExpr.
//
// Orig holds the original ast.Expr from which this IndexExpr was derived.
//
// Note: IndexExpr (intentionally) does not wrap ast.Expr, as that leads to
// accidental misuse such as encountered in golang/go#63933.
//
// TODO(rfindley): remove this helper, in favor of just having a helper
// function that returns indices.
[GoType] partial struct IndexExpr {
    public go.ast_package.Expr Orig;   // the wrapped expr, which may be distinct from the IndexListExpr below.
    public go.ast_package.Expr X;   // expression
    public go.token_package.ΔPos Lbrack; // position of "["
    public ast.Expr Indices; // index expressions
    public go.token_package.ΔPos Rbrack; // position of "]"
}

[GoRecv] public static tokenꓸPos Pos(this ref IndexExpr x) {
    return x.Orig.Pos();
}

public static ж<IndexExpr> UnpackIndexExpr(ast.Node n) {
    switch (n.type()) {
    case ж<ast.IndexExpr> e: {
        return Ꮡ(new IndexExpr(
            Orig: e,
            X: (~e).X,
            Lbrack: (~e).Lbrack,
            Indices: new ast.Expr[]{(~e).Index}.slice(),
            Rbrack: (~e).Rbrack
        ));
    }
    case ж<ast.IndexListExpr> e: {
        return Ꮡ(new IndexExpr(
            Orig: e,
            X: (~e).X,
            Lbrack: (~e).Lbrack,
            Indices: (~e).Indices,
            Rbrack: (~e).Rbrack
        ));
    }}
    return default!;
}

} // end typeparams_package
