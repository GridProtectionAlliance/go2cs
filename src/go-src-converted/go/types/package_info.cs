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
global using constantꓸKind = go.go.constant_package.ΔKind;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
using ast = go.go.ast_package;
using bytes = go.bytes_package;
using token = go.go.token_package;
using typeparams = go.go.@internal.typeparams_package;
// </ImportedTypeAliases>

using go;
using static go.go.types_package;

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
[assembly: GoTypeAlias("Error", "ΔError")]
[assembly: GoTypeAlias("Expr", "go.go.ast_package.Expr")]
[assembly: GoTypeAlias("Info", "ΔInfo")]
[assembly: GoTypeAlias("Scope", "ΔScope")]
[assembly: GoTypeAlias("Signature", "ΔSignature")]
[assembly: GoTypeAlias("String", "const:ΔString")]
[assembly: GoTypeAlias("Term", "ΔTerm")]
[assembly: GoTypeAlias("Type", "ΔType")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<Alias, cleaner>(Pointer = true)]
[assembly: GoImplement<Alias, ΔType>(Pointer = true)]
[assembly: GoImplement<Alias, ΔgenericType>(Pointer = true)]
[assembly: GoImplement<ArgumentError, error>(Pointer = true)]
[assembly: GoImplement<Basic, ΔType>(Pointer = true)]
[assembly: GoImplement<Builtin, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Chan, ΔType>(Pointer = true)]
[assembly: GoImplement<Const, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Expr, positioner>]
[assembly: GoImplement<Interface, cleaner>(Pointer = true)]
[assembly: GoImplement<Interface, ΔType>(Pointer = true)]
[assembly: GoImplement<Label, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Label, positioner>(Pointer = true)]
[assembly: GoImplement<Map, ΔType>(Pointer = true)]
[assembly: GoImplement<Named, cleaner>(Pointer = true)]
[assembly: GoImplement<Named, ΔType>(Pointer = true)]
[assembly: GoImplement<Named, ΔgenericType>(Pointer = true)]
[assembly: GoImplement<Nil, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<PkgName, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<PkgName, positioner>(Pointer = true)]
[assembly: GoImplement<Pointer, ΔType>(Pointer = true)]
[assembly: GoImplement<Slice, ΔType>(Pointer = true)]
[assembly: GoImplement<StdSizes, Sizes>(Pointer = true)]
[assembly: GoImplement<Struct, ΔType>(Pointer = true)]
[assembly: GoImplement<TypeName, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<TypeName, positioner>(Pointer = true)]
[assembly: GoImplement<TypeParam, cleaner>(Pointer = true)]
[assembly: GoImplement<TypeParam, ΔType>(Pointer = true)]
[assembly: GoImplement<Union, ΔType>(Pointer = true)]
[assembly: GoImplement<Var, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<Var, positioner>(Pointer = true)]
[assembly: GoImplement<atPos, positioner>]
[assembly: GoImplement<byUniqueMethodName, sort_package.Interface>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<gcSizes, Sizes>(Pointer = true)]
[assembly: GoImplement<go.go.@internal.typeparams_package.IndexExpr, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.AssignStmt, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.AssignStmt, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.BasicLit, go.go.ast_package.Expr>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.BasicLit, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.BinaryExpr, go.go.ast_package.Expr>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.BlockStmt, go.go.ast_package.Stmt>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.BranchStmt, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.CallExpr, go.go.ast_package.Expr>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.CallExpr, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.CaseClause, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.ChanType, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.CompositeLit, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.CompositeLit, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.Decl, positioner>]
[assembly: GoImplement<go.go.ast_package.Ellipsis, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.Expr, positioner>]
[assembly: GoImplement<go.go.ast_package.Field, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.FieldList, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.File, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.ForStmt, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.FuncDecl, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.FuncLit, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.FuncType, go.go.ast_package.Expr>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.Ident, go.go.ast_package.Expr>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.Ident, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.IfStmt, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.ImportSpec, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.ImportSpec, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.IncDecStmt, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.InterfaceType, go.go.ast_package.Expr>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.InterfaceType, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.KeyValueExpr, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.Node, positioner>]
[assembly: GoImplement<go.go.ast_package.ParenExpr, go.go.ast_package.Expr>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.RangeStmt, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.ReturnStmt, go.go.ast_package.Stmt>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.ReturnStmt, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.SelectorExpr, go.go.ast_package.Expr>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.SelectorExpr, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.SendStmt, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.SliceExpr, go.go.ast_package.Expr>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.SliceExpr, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.Spec, positioner>]
[assembly: GoImplement<go.go.ast_package.Stmt, positioner>]
[assembly: GoImplement<go.go.ast_package.SwitchStmt, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.TypeAssertExpr, go.go.ast_package.Expr>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.TypeAssertExpr, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.TypeSpec, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.TypeSpec, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.TypeSwitchStmt, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.TypeSwitchStmt, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.UnaryExpr, go.go.ast_package.Expr>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.UnaryExpr, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.ValueSpec, go.go.ast_package.Node>(Pointer = true)]
[assembly: GoImplement<go.go.ast_package.ValueSpec, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.types_package.Array, ΔType>(Pointer = true)]
[assembly: GoImplement<go.go.types_package.Func, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<go.go.types_package.Func, positioner>(Pointer = true)]
[assembly: GoImplement<go.go.types_package.Object, positioner>]
[assembly: GoImplement<go.go.types_package.Tuple, ΔType>(Pointer = true)]
[assembly: GoImplement<importDecl, decl>]
[assembly: GoImplement<inSourceOrder, sort_package.Interface>]
[assembly: GoImplement<lazyObject, go.go.types_package.Object>(Pointer = true)]
[assembly: GoImplement<nodeQueue, go.container.heap_package.Interface>(Pointer = true)]
[assembly: GoImplement<operand, positioner>(Pointer = true)]
[assembly: GoImplement<posSpan, positioner>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>(Pointer = true)]
[assembly: GoImplement<typeParamsById, sort_package.Interface>]
[assembly: GoImplement<ΔError, error>]
[assembly: GoImplement<ΔSignature, ΔType>(Pointer = true)]
[assembly: GoImplement<ΔSignature, ΔgenericType>(Pointer = true)]
[assembly: GoImplement<ΔconstDecl, decl>]
[assembly: GoImplement<ΔfuncDecl, decl>]
[assembly: GoImplement<ΔtypeDecl, decl>]
[assembly: GoImplement<ΔvarDecl, decl>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Alias, ж<Alias>>(Indirect = true)]
[assembly: GoImplicitConv<Basic, ж<Basic>>(Indirect = true)]
[assembly: GoImplicitConv<Chan, ж<Chan>>(Indirect = true)]
[assembly: GoImplicitConv<Const, ж<Const>>(Indirect = true)]
[assembly: GoImplicitConv<Context, ж<Context>>(Indirect = true)]
[assembly: GoImplicitConv<Interface, ж<Interface>>(Indirect = true)]
[assembly: GoImplicitConv<Label, ж<Label>>(Indirect = true)]
[assembly: GoImplicitConv<Map, ж<Map>>(Indirect = true)]
[assembly: GoImplicitConv<Named, ж<Named>>(Indirect = true)]
[assembly: GoImplicitConv<Package, ж<Package>>(Indirect = true)]
[assembly: GoImplicitConv<PkgName, ж<PkgName>>(Indirect = true)]
[assembly: GoImplicitConv<Pointer, ж<Pointer>>(Indirect = true)]
[assembly: GoImplicitConv<Slice, ж<Slice>>(Indirect = true)]
[assembly: GoImplicitConv<Struct, ж<Struct>>(Indirect = true)]
[assembly: GoImplicitConv<TypeName, ж<TypeName>>(Indirect = true)]
[assembly: GoImplicitConv<TypeParam, ж<TypeParam>>(Indirect = true)]
[assembly: GoImplicitConv<Union, ж<Union>>(Indirect = true)]
[assembly: GoImplicitConv<Var, ж<Var>>(Indirect = true)]
[assembly: GoImplicitConv<ast.BasicLit, ж<ast.BasicLit>>(Indirect = true)]
[assembly: GoImplicitConv<ast.BinaryExpr, ж<ast.BinaryExpr>>(Indirect = true)]
[assembly: GoImplicitConv<ast.BlockStmt, ж<ast.BlockStmt>>(Indirect = true)]
[assembly: GoImplicitConv<ast.CallExpr, ж<ast.CallExpr>>(Indirect = true)]
[assembly: GoImplicitConv<ast.FieldList, ж<ast.FieldList>>(Indirect = true)]
[assembly: GoImplicitConv<ast.FuncType, ж<ast.FuncType>>(Indirect = true)]
[assembly: GoImplicitConv<ast.Ident, ж<ast.Ident>>(Indirect = true)]
[assembly: GoImplicitConv<ast.InterfaceType, ж<ast.InterfaceType>>(Indirect = true)]
[assembly: GoImplicitConv<ast.LabeledStmt, ж<ast.LabeledStmt>>(Indirect = true)]
[assembly: GoImplicitConv<ast.SelectorExpr, ж<ast.SelectorExpr>>(Indirect = true)]
[assembly: GoImplicitConv<ast.SliceExpr, ж<ast.SliceExpr>>(Indirect = true)]
[assembly: GoImplicitConv<ast.StructType, ж<ast.StructType>>(Indirect = true)]
[assembly: GoImplicitConv<ast.TypeSpec, ж<ast.TypeSpec>>(Indirect = true)]
[assembly: GoImplicitConv<ast.UnaryExpr, ж<ast.UnaryExpr>>(Indirect = true)]
[assembly: GoImplicitConv<block, ж<block>>(Indirect = true)]
[assembly: GoImplicitConv<declInfo, ж<declInfo>>(Indirect = true)]
[assembly: GoImplicitConv<go.go.types_package.Array, ж<go.go.types_package.Array>>(Indirect = true)]
[assembly: GoImplicitConv<go.go.types_package.Func, ж<go.go.types_package.Func>>(Indirect = true)]
[assembly: GoImplicitConv<go.go.types_package.Tuple, ж<go.go.types_package.Tuple>>(Indirect = true)]
[assembly: GoImplicitConv<operand, ж<operand>>(Indirect = true)]
[assembly: GoImplicitConv<token.FileSet, ж<token.FileSet>>(Indirect = true)]
[assembly: GoImplicitConv<typeparams.IndexExpr, ж<typeparams.IndexExpr>>(Indirect = true)]
[assembly: GoImplicitConv<ΔInfo, ж<ΔInfo>>(Indirect = true)]
[assembly: GoImplicitConv<ΔSignature, ж<ΔSignature>>(Indirect = true)]
// </ImplicitConversions>

namespace go.go;

[GoPackage("types")]
public static partial class types_package
{
}
