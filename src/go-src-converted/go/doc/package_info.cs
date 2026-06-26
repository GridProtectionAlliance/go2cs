// go2cs code converter defines `global using` statements here for imported type
// aliases as package references are encountered via `import' statements. Exported
// type aliases that need a `global using` declaration will be loaded from the
// referenced package by parsing its 'package_info.cs' source file and reading its
// defined `GoTypeAlias` attributes.

// Package name separator "dot" used in imported type aliases is extended Unicode
// character '\uA4F8' which is a valid character in a C# identifier name. This is
// used to simulate Go's package level type aliases since C# does not yet support
// importing type aliases at a namespace level.

// <ImportedTypeAliases>
global using astꓸFilter = go.go.ast_package.ΔFilter;
global using commentꓸText = go.go.doc.comment_package.ΔText;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static go.go.doc_package;

// For encountered type alias declarations, e.g., `type Table = map[string]int`,
// go2cs code converter will generate a `global using` statement for the alias in
// the converted source, e.g.: `global using Table = go.map<go.@string, nint>;`.
// Although scope of `global using` is available to all files in the project, all
// converted Go code for the project targets the same package, so `global using`
// statements will effectively have package level scope.

// Additionally, `GoTypeAlias` attributes will be generated here for exported type
// aliases. This allows the type alias to be imported and used from other packages
// when referenced.

// <ExportedTypeAliases>
[assembly: GoTypeAlias("Filter", "ΔFilter")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<ast.Decl, go.ast_package.Decl>]
[assembly: GoImplement<ast.Spec, go.ast_package.Spec>]
[assembly: GoImplement<go.ast_package.BlockStmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.Expr, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.FuncDecl, go.ast_package.Decl>]
[assembly: GoImplement<go.ast_package.GenDecl, go.ast_package.Decl>]
[assembly: GoImplement<go.ast_package.Ident, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.ImportSpec, go.ast_package.Spec>]
[assembly: GoImplement<go.ast_package.SelectorExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.Spec, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.StarExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.TypeSpec, go.ast_package.Spec>]
[assembly: GoImplement<go.ast_package.ValueSpec, go.ast_package.Spec>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<ast.FieldList, ж<ast.FieldList>>(Indirect = true)]
[assembly: GoImplicitConv<ast.FuncDecl, ж<ast.FuncDecl>>(Indirect = true)]
[assembly: GoImplicitConv<ast.InterfaceType, ж<ast.InterfaceType>>(Indirect = true)]
[assembly: GoImplicitConv<ast.TypeSpec, ж<ast.TypeSpec>>(Indirect = true)]
// </ImplicitConversions>

namespace go.go;

[GoPackage("doc")]
public static partial class doc_package
{
}
