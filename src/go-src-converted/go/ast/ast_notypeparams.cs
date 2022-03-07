// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !typeparams
// +build !typeparams

// package ast -- go2cs converted at 2022 March 06 22:42:55 UTC
// import "go/ast" ==> using ast = go.go.ast_package
// Original source: C:\Program Files\Go\src\go\ast\ast_notypeparams.go
using token = go.go.token_package;

namespace go.go;

public static partial class ast_package {

 
// A FuncType node represents a function type.
public partial struct FuncType {
    public token.Pos Func; // position of "func" keyword (token.NoPos if there is no "func")
    public ptr<FieldList> Params; // (incoming) parameters; non-nil
    public ptr<FieldList> Results; // (outgoing) results; or nil
} 

// A TypeSpec node represents a type declaration (TypeSpec production).
public partial struct TypeSpec {
    public ptr<CommentGroup> Doc; // associated documentation; or nil
    public ptr<Ident> Name; // type name
    public token.Pos Assign; // position of '=', if any
    public Expr Type; // *Ident, *ParenExpr, *SelectorExpr, *StarExpr, or any of the *XxxTypes
    public ptr<CommentGroup> Comment; // line comments; or nil
}

} // end ast_package
