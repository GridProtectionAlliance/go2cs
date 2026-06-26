// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using parser = go.parser_package;
using token = go.token_package;

partial class types_package {

// Eval returns the type and, if constant, the value for the
// expression expr, evaluated at position pos of package pkg,
// which must have been derived from type-checking an AST with
// complete position information relative to the provided file
// set.
//
// The meaning of the parameters fset, pkg, and pos is the
// same as in [CheckExpr]. An error is returned if expr cannot
// be parsed successfully, or the resulting expr AST cannot be
// type-checked.
public static (TypeAndValue _, error err) Eval(ж<token.FileSet> Ꮡfset, ж<Package> Ꮡpkg, tokenꓸPos pos, @string expr) {
    error err = default!;

    ref var fset = ref Ꮡfset.val;
    ref var pkg = ref Ꮡpkg.val;
    // parse expressions
    (node, err) = parser.ParseExprFrom(Ꮡfset, "eval"u8, expr, 0);
    if (err != default!) {
        return (new TypeAndValue(nil), err);
    }
    var info = Ꮡ(new ΔInfo(
        Types: new ast.Expr>TypeAndValue()
    ));
    err = CheckExpr(Ꮡfset, Ꮡpkg, pos, node, info);
    return ((~info).Types[node], err);
}

// CheckExpr type checks the expression expr as if it had appeared at position
// pos of package pkg. [Type] information about the expression is recorded in
// info. The expression may be an identifier denoting an uninstantiated generic
// function or type.
//
// If pkg == nil, the [Universe] scope is used and the provided
// position pos is ignored. If pkg != nil, and pos is invalid,
// the package scope is used. Otherwise, pos must belong to the
// package.
//
// An error is returned if pos is not within the package or
// if the node cannot be type-checked.
//
// Note: [Eval] and CheckExpr should not be used instead of running Check
// to compute types and values, but in addition to Check, as these
// functions ignore the context in which an expression is used (e.g., an
// assignment). Thus, top-level untyped constants will return an
// untyped type rather than the respective context-specific type.
public static error /*err*/ CheckExpr(ж<token.FileSet> Ꮡfset, ж<Package> Ꮡpkg, tokenꓸPos pos, ast.Expr expr, ж<ΔInfo> Ꮡinfo) => func((defer, _) => {
    error err = default!;

    ref var fset = ref Ꮡfset.val;
    ref var pkg = ref Ꮡpkg.val;
    ref var info = ref Ꮡinfo.val;
    // determine scope
    ж<ΔScope> scope = default!;
    if (pkg == nil){
        scope = Universe;
        pos = nopos;
    } else 
    if (!pos.IsValid()){
        scope = pkg.scope;
    } else {
        // The package scope extent (position information) may be
        // incorrect (files spread across a wide range of fset
        // positions) - ignore it and just consider its children
        // (file scopes).
        foreach (var (_, fscope) in pkg.scope.children) {
            {
                scope = fscope.Innermost(pos); if (scope != nil) {
                    break;
                }
            }
        }
        if (scope == nil || debug) {
            var s = scope;
            while (s != nil && s != pkg.scope) {
                s = s.val.parent;
            }
            // s == nil || s == pkg.scope
            if (s == nil) {
                return fmt.Errorf("no position %s found in package %s"u8, fset.Position(pos), pkg.name);
            }
        }
    }
    // initialize checker
    var check = NewChecker(nil, Ꮡfset, Ꮡpkg, Ꮡinfo);
    check.scope = scope;
    check.pos = pos;
    var checkʗ1 = check;
    deferǃ(checkʗ1.handleBailout, Ꮡ(err), defer);
    // evaluate node
    ref var x = ref heap(new operand(), out var Ꮡx);
    check.rawExpr(nil, Ꮡx, expr, default!, true);
    // allow generic expressions
    check.processDelayed(0);
    // incl. all functions
    check.recordUntyped();
    return default!;
});

} // end types_package
