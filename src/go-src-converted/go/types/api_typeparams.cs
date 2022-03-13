// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build typeparams
// +build typeparams

// package types -- go2cs converted at 2022 March 13 05:52:42 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\api_typeparams.go
namespace go.go;

using ast = go.ast_package;

public static partial class types_package {

public partial struct Inferred { // : _Inferred
}
public partial struct Sum { // : _Sum
}
public partial struct TypeParam { // : _TypeParam
}public static Type NewSum(slice<Type> types) {
    return _NewSum(types);
}

private static slice<ptr<TypeName>> TParams(this ptr<Signature> _addr_s) {
    ref Signature s = ref _addr_s.val;

    return s._TParams();
}
private static void SetTParams(this ptr<Signature> _addr_s, slice<ptr<TypeName>> tparams) {
    ref Signature s = ref _addr_s.val;

    s._SetTParams(tparams);
}

private static bool HasTypeList(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    return t._HasTypeList();
}
private static bool IsComparable(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    return t._IsComparable();
}
private static bool IsConstraint(this ptr<Interface> _addr_t) {
    ref Interface t = ref _addr_t.val;

    return t._IsConstraint();
}

private static slice<ptr<TypeName>> TParams(this ptr<Named> _addr_t) {
    ref Named t = ref _addr_t.val;

    return t._TParams();
}
private static slice<Type> TArgs(this ptr<Named> _addr_t) {
    ref Named t = ref _addr_t.val;

    return t._TArgs();
}
private static void SetTArgs(this ptr<Named> _addr_t, slice<Type> args) {
    ref Named t = ref _addr_t.val;

    t._SetTArgs(args);
}

// Info is documented in api_notypeparams.go.
public partial struct Info {
    public map<ast.Expr, TypeAndValue> Types; // Inferred maps calls of parameterized functions that use type inference to
// the Inferred type arguments and signature of the function called. The
// recorded "call" expression may be an *ast.CallExpr (as in f(x)), or an
// *ast.IndexExpr (s in f[T]).
    public map<ast.Expr, _Inferred> Inferred;
    public map<ptr<ast.Ident>, Object> Defs;
    public map<ptr<ast.Ident>, Object> Uses;
    public map<ast.Node, Object> Implicits;
    public map<ptr<ast.SelectorExpr>, ptr<Selection>> Selections;
    public map<ast.Node, ptr<Scope>> Scopes;
    public slice<ptr<Initializer>> InitOrder;
}

private static map<ast.Expr, _Inferred> getInferred(ptr<Info> _addr_info) {
    ref Info info = ref _addr_info.val;

    return info.Inferred;
}

} // end types_package
