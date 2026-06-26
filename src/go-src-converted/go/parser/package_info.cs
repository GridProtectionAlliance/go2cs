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
global using osꓸDirEntry = go.io.fs_package.DirEntry;
global using osꓸFileInfo = go.io.fs_package.FileInfo;
global using osꓸFileMode = go.io.fs_package.FileMode;
global using osꓸPathError = go.io.fs_package.PathError;
global using osꓸSignal = go.os_package.ΔSignal;
global using scannerꓸError = go.go.scanner_package.ΔError;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
// </ImportedTypeAliases>

using go;
using static go.go.parser_package;

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
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<ast.Expr), go.ast_package.Stmt>]
[assembly: GoImplement<ast.Expr, err error), go.ast_package.Expr>]
[assembly: GoImplement<ast.Stmt, bool), go.ast_package.Stmt>]
[assembly: GoImplement<constraint.Expr, error), go.build.constraint_package.Expr>]
[assembly: GoImplement<fs.FileInfo, error), io.fs_package.FileInfo>]
[assembly: GoImplement<go.ast_package.ArrayType, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.AssignStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.BadDecl, go.ast_package.Decl>]
[assembly: GoImplement<go.ast_package.BadExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.BadStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.BasicLit, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.BinaryExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.BlockStmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.BlockStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.BranchStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.CallExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.CaseClause, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.ChanType, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.CommClause, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.CompositeLit, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.Decl, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.DeclStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.DeferStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.Ellipsis, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.EmptyStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.Expr, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.ExprStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.ForStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.FuncDecl, go.ast_package.Decl>]
[assembly: GoImplement<go.ast_package.FuncLit, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.FuncType, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.GenDecl, go.ast_package.Decl>]
[assembly: GoImplement<go.ast_package.GoStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.Ident, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.IfStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.ImportSpec, go.ast_package.Spec>]
[assembly: GoImplement<go.ast_package.IncDecStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.IndexExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.InterfaceType, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.KeyValueExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.LabeledStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.MapType, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.ParenExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.RangeStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.ReturnStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.SelectStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.SelectorExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.SendStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.SliceExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.StarExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.Stmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.StructType, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.SwitchStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.TypeAssertExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.TypeSpec, go.ast_package.Spec>]
[assembly: GoImplement<go.ast_package.TypeSwitchStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.UnaryExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.ValueSpec, go.ast_package.Spec>]
[assembly: GoImplement<resolver, go.ast_package.Visitor>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<ast.Ident, ж<ast.Ident>>(Indirect = true)]
[assembly: GoImplicitConv<tokenꓸFile, ж<tokenꓸFile>>(Indirect = true)]
// </ImplicitConversions>

namespace go.go;

[GoPackage("parser")]
public static partial class parser_package
{
}
