// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build !typeparams
// +build !typeparams

// package types -- go2cs converted at 2022 March 06 22:41:37 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\api_notypeparams.go
using ast = go.go.ast_package;

namespace go.go;

public static partial class types_package {

    // Info holds result type information for a type-checked package.
    // Only the information for which a map is provided is collected.
    // If the package has type errors, the collected information may
    // be incomplete.
public partial struct Info {
    public map<ast.Expr, TypeAndValue> Types; // Defs maps identifiers to the objects they define (including
// package names, dots "." of dot-imports, and blank "_" identifiers).
// For identifiers that do not denote objects (e.g., the package name
// in package clauses, or symbolic variables t in t := x.(type) of
// type switch headers), the corresponding objects are nil.
//
// For an embedded field, Defs returns the field *Var it defines.
//
// Invariant: Defs[id] == nil || Defs[id].Pos() == id.Pos()
    public map<ptr<ast.Ident>, Object> Defs; // Uses maps identifiers to the objects they denote.
//
// For an embedded field, Uses returns the *TypeName it denotes.
//
// Invariant: Uses[id].Pos() != id.Pos()
    public map<ptr<ast.Ident>, Object> Uses; // Implicits maps nodes to their implicitly declared objects, if any.
// The following node and object types may appear:
//
//     node               declared object
//
//     *ast.ImportSpec    *PkgName for imports without renames
//     *ast.CaseClause    type-specific *Var for each type switch case clause (incl. default)
//     *ast.Field         anonymous parameter *Var (incl. unnamed results)
//
    public map<ast.Node, Object> Implicits; // Selections maps selector expressions (excluding qualified identifiers)
// to their corresponding selections.
    public map<ptr<ast.SelectorExpr>, ptr<Selection>> Selections; // Scopes maps ast.Nodes to the scopes they define. Package scopes are not
// associated with a specific node but with all files belonging to a package.
// Thus, the package scope can be found in the type-checked Package object.
// Scopes nest, with the Universe scope being the outermost scope, enclosing
// the package scope, which contains (one or more) files scopes, which enclose
// function scopes which in turn enclose statement and function literal scopes.
// Note that even though package-level functions are declared in the package
// scope, the function scopes are embedded in the file scope of the file
// containing the function declaration.
//
// The following node types may appear in Scopes:
//
//     *ast.File
//     *ast.FuncType
//     *ast.BlockStmt
//     *ast.IfStmt
//     *ast.SwitchStmt
//     *ast.TypeSwitchStmt
//     *ast.CaseClause
//     *ast.CommClause
//     *ast.ForStmt
//     *ast.RangeStmt
//
    public map<ast.Node, ptr<Scope>> Scopes; // InitOrder is the list of package-level initializers in the order in which
// they must be executed. Initializers referring to variables related by an
// initialization dependency appear in topological order, the others appear
// in source order. Variables without an initialization expression do not
// appear in this list.
    public slice<ptr<Initializer>> InitOrder;
}

private static map<ast.Expr, _Inferred> getInferred(ptr<Info> _addr_info) {
    ref Info info = ref _addr_info.val;

    return null;
}

} // end types_package
