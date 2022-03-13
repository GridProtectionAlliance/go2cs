// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build typeparams
// +build typeparams

// package ast -- go2cs converted at 2022 March 13 05:54:04 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Program Files\Go\src\go\ast\ast_typeparams.go
namespace go.go;

using token = go.token_package;

public static partial class ast_package {

 
// A FuncType node represents a function type.
public partial struct FuncType {
    public token.Pos Func; // position of "func" keyword (token.NoPos if there is no "func")
    public ptr<FieldList> TParams; // type parameters; or nil
    public ptr<FieldList> Params; // (incoming) parameters; non-nil
    public ptr<FieldList> Results; // (outgoing) results; or nil
} 

// A TypeSpec node represents a type declaration (TypeSpec production).
public partial struct TypeSpec {
    public ptr<CommentGroup> Doc; // associated documentation; or nil
    public ptr<Ident> Name; // type name
    public ptr<FieldList> TParams; // type parameters; or nil
    public token.Pos Assign; // position of '=', if any
    public Expr Type; // *Ident, *ParenExpr, *SelectorExpr, *StarExpr, or any of the *XxxTypes
    public ptr<CommentGroup> Comment; // line comments; or nil
} 

// A ListExpr node represents a list of expressions separated by commas.
// ListExpr nodes are used as index in IndexExpr nodes representing type
// or function instantiations with more than one type argument.
public partial struct ListExpr {
    public slice<Expr> ElemList;
}private static void exprNode(this ptr<ListExpr> _addr__p0) {
    ref ListExpr _p0 = ref _addr__p0.val;

}
private static token.Pos Pos(this ptr<ListExpr> _addr_x) {
    ref ListExpr x = ref _addr_x.val;

    if (len(x.ElemList) > 0) {
        return x.ElemList[0].Pos();
    }
    return token.NoPos;
}
private static token.Pos End(this ptr<ListExpr> _addr_x) {
    ref ListExpr x = ref _addr_x.val;

    if (len(x.ElemList) > 0) {
        return x.ElemList[len(x.ElemList) - 1].End();
    }
    return token.NoPos;
}

} // end ast_package
