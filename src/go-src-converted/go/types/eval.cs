// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 06 22:41:50 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\eval.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using parser = go.go.parser_package;
using token = go.go.token_package;

namespace go.go;

public static partial class types_package {

    // Eval returns the type and, if constant, the value for the
    // expression expr, evaluated at position pos of package pkg,
    // which must have been derived from type-checking an AST with
    // complete position information relative to the provided file
    // set.
    //
    // The meaning of the parameters fset, pkg, and pos is the
    // same as in CheckExpr. An error is returned if expr cannot
    // be parsed successfully, or the resulting expr AST cannot be
    // type-checked.
public static (TypeAndValue, error) Eval(ptr<token.FileSet> _addr_fset, ptr<Package> _addr_pkg, token.Pos pos, @string expr) {
    TypeAndValue _ = default;
    error err = default!;
    ref token.FileSet fset = ref _addr_fset.val;
    ref Package pkg = ref _addr_pkg.val;
 
    // parse expressions
    var (node, err) = parser.ParseExprFrom(fset, "eval", expr, 0);
    if (err != null) {
        return (new TypeAndValue(), error.As(err)!);
    }
    ptr<Info> info = addr(new Info(Types:make(map[ast.Expr]TypeAndValue),));
    err = CheckExpr(_addr_fset, _addr_pkg, pos, node, info);
    return (info.Types[node], error.As(err)!);

}

// CheckExpr type checks the expression expr as if it had appeared at
// position pos of package pkg. Type information about the expression
// is recorded in info.
//
// If pkg == nil, the Universe scope is used and the provided
// position pos is ignored. If pkg != nil, and pos is invalid,
// the package scope is used. Otherwise, pos must belong to the
// package.
//
// An error is returned if pos is not within the package or
// if the node cannot be type-checked.
//
// Note: Eval and CheckExpr should not be used instead of running Check
// to compute types and values, but in addition to Check, as these
// functions ignore the context in which an expression is used (e.g., an
// assignment). Thus, top-level untyped constants will return an
// untyped type rather then the respective context-specific type.
//
public static error CheckExpr(ptr<token.FileSet> _addr_fset, ptr<Package> _addr_pkg, token.Pos pos, ast.Expr expr, ptr<Info> _addr_info) => func((defer, _, _) => {
    error err = default!;
    ref token.FileSet fset = ref _addr_fset.val;
    ref Package pkg = ref _addr_pkg.val;
    ref Info info = ref _addr_info.val;
 
    // determine scope
    ptr<Scope> scope;
    if (pkg == null) {
        scope = Universe;
        pos = token.NoPos;
    }
    else if (!pos.IsValid()) {
        scope = pkg.scope;
    }
    else
 { 
        // The package scope extent (position information) may be
        // incorrect (files spread across a wide range of fset
        // positions) - ignore it and just consider its children
        // (file scopes).
        foreach (var (_, fscope) in pkg.scope.children) {
            scope = fscope.Innermost(pos);

            if (scope != null) {
                break;
            }

        }        if (scope == null || debug) {
            var s = scope;
            while (s != null && s != pkg.scope) {
                s = s.parent;
            } 
            // s == nil || s == pkg.scope
 
            // s == nil || s == pkg.scope
            if (s == null) {
                return error.As(fmt.Errorf("no position %s found in package %s", fset.Position(pos), pkg.name))!;
            }

        }
    }
    var check = NewChecker(null, fset, pkg, info);
    check.scope = scope;
    check.pos = pos;
    defer(check.handleBailout(_addr_err)); 

    // evaluate node
    ref operand x = ref heap(out ptr<operand> _addr_x);
    check.rawExpr(_addr_x, expr, null);
    check.processDelayed(0); // incl. all functions
    check.recordUntyped();

    return error.As(null!)!;

});

} // end types_package
