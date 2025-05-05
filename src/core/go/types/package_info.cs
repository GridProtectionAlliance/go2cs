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
global using constantꓸKind = go.go.constant_package.ΔKind;
global using runtimeꓸError = go.runtime_package.ΔError;
global using tokenꓸFile = go.go.token_package.ΔFile;
global using tokenꓸPos = go.go.token_package.ΔPos;
global using tokenꓸPosition = go.go.token_package.ΔPosition;
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
[assembly: GoImplement<(ImporterFrom, bool), ImporterFrom>]
[assembly: GoImplement<(Type, bool), ΔType>]
[assembly: GoImplement<(obj Object, index <>int, indirect bool), Object>]
[assembly: GoImplement<(typ Type, val int64), ΔType>]
[assembly: GoImplement<Alias, cleaner>]
[assembly: GoImplement<Alias, ΔType>]
[assembly: GoImplement<Alias, ΔgenericType>]
[assembly: GoImplement<ArgumentError, error>]
[assembly: GoImplement<Array, ΔType>]
[assembly: GoImplement<Basic, ΔType>]
[assembly: GoImplement<Builtin, Object>]
[assembly: GoImplement<Chan, ΔType>]
[assembly: GoImplement<Const, Object>]
[assembly: GoImplement<Expr, go.ast_package.Expr>]
[assembly: GoImplement<Expr, positioner>]
[assembly: GoImplement<Func, Object>]
[assembly: GoImplement<Func, positioner>]
[assembly: GoImplement<Interface, cleaner>]
[assembly: GoImplement<Interface, ΔType>]
[assembly: GoImplement<Label, Object>]
[assembly: GoImplement<Label, positioner>]
[assembly: GoImplement<Map, ΔType>]
[assembly: GoImplement<Named, cleaner>]
[assembly: GoImplement<Named, ΔType>]
[assembly: GoImplement<Named, ΔgenericType>]
[assembly: GoImplement<Nil, Object>]
[assembly: GoImplement<Object, positioner>]
[assembly: GoImplement<PkgName, Object>]
[assembly: GoImplement<PkgName, positioner>]
[assembly: GoImplement<Pointer, ΔType>]
[assembly: GoImplement<Slice, ΔType>]
[assembly: GoImplement<Struct, ΔType>]
[assembly: GoImplement<Tuple, ΔType>]
[assembly: GoImplement<TypeName, Object>]
[assembly: GoImplement<TypeName, positioner>]
[assembly: GoImplement<TypeParam, cleaner>]
[assembly: GoImplement<TypeParam, ΔType>]
[assembly: GoImplement<Union, ΔType>]
[assembly: GoImplement<Var, Object>]
[assembly: GoImplement<Var, positioner>]
[assembly: GoImplement<ast.Expr, err error), go.ast_package.Expr>]
[assembly: GoImplement<ast.Expr, go.ast_package.Expr>]
[assembly: GoImplement<atPos, positioner>]
[assembly: GoImplement<bool, ΔType>]
[assembly: GoImplement<byUniqueMethodName, sort_package.Interface>]
[assembly: GoImplement<bytes_package.Buffer, io_package.Writer>]
[assembly: GoImplement<dependency, Object>]
[assembly: GoImplement<errors.Code), go.constant_package.Value>]
[assembly: GoImplement<errors.Code), ΔType>]
[assembly: GoImplement<go.@internal.typeparams_package.IndexExpr, positioner>]
[assembly: GoImplement<go.ast_package.AssignStmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.AssignStmt, positioner>]
[assembly: GoImplement<go.ast_package.BasicLit, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.BasicLit, positioner>]
[assembly: GoImplement<go.ast_package.BinaryExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.BlockStmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.BlockStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.BranchStmt, positioner>]
[assembly: GoImplement<go.ast_package.CallExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.CallExpr, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.CallExpr, positioner>]
[assembly: GoImplement<go.ast_package.CaseClause, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.ChanType, positioner>]
[assembly: GoImplement<go.ast_package.CompositeLit, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.CompositeLit, positioner>]
[assembly: GoImplement<go.ast_package.Decl, positioner>]
[assembly: GoImplement<go.ast_package.Ellipsis, positioner>]
[assembly: GoImplement<go.ast_package.Expr, Expr>]
[assembly: GoImplement<go.ast_package.Expr, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.Expr, positioner>]
[assembly: GoImplement<go.ast_package.Field, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.Field, positioner>]
[assembly: GoImplement<go.ast_package.FieldList, positioner>]
[assembly: GoImplement<go.ast_package.File, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.File, positioner>]
[assembly: GoImplement<go.ast_package.ForStmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.FuncDecl, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.FuncLit, positioner>]
[assembly: GoImplement<go.ast_package.FuncType, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.FuncType, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.Ident, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.Ident, positioner>]
[assembly: GoImplement<go.ast_package.IfStmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.ImportSpec, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.ImportSpec, positioner>]
[assembly: GoImplement<go.ast_package.IncDecStmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.InterfaceType, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.InterfaceType, positioner>]
[assembly: GoImplement<go.ast_package.KeyValueExpr, positioner>]
[assembly: GoImplement<go.ast_package.Node, positioner>]
[assembly: GoImplement<go.ast_package.ParenExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.RangeStmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.ReturnStmt, go.ast_package.Stmt>]
[assembly: GoImplement<go.ast_package.ReturnStmt, positioner>]
[assembly: GoImplement<go.ast_package.SelectorExpr, positioner>]
[assembly: GoImplement<go.ast_package.SendStmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.SliceExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.SliceExpr, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.Spec, positioner>]
[assembly: GoImplement<go.ast_package.Stmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.Stmt, positioner>]
[assembly: GoImplement<go.ast_package.SwitchStmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.TypeAssertExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.TypeAssertExpr, positioner>]
[assembly: GoImplement<go.ast_package.TypeSpec, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.TypeSpec, positioner>]
[assembly: GoImplement<go.ast_package.TypeSwitchStmt, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.TypeSwitchStmt, positioner>]
[assembly: GoImplement<go.ast_package.UnaryExpr, go.ast_package.Expr>]
[assembly: GoImplement<go.ast_package.UnaryExpr, positioner>]
[assembly: GoImplement<go.ast_package.ValueSpec, go.ast_package.Node>]
[assembly: GoImplement<go.ast_package.ValueSpec, positioner>]
[assembly: GoImplement<importDecl, decl>]
[assembly: GoImplement<inSourceOrder, sort_package.Interface>]
[assembly: GoImplement<lazyObject, Object>]
[assembly: GoImplement<nodeQueue, container.heap_package.Interface>]
[assembly: GoImplement<operand, positioner>]
[assembly: GoImplement<posSpan, positioner>]
[assembly: GoImplement<slice<ΔType>, ΔType>]
[assembly: GoImplement<strings_package.Builder, io_package.Writer>]
[assembly: GoImplement<typeParamsById, sort_package.Interface>]
[assembly: GoImplement<types.Type, cause string, ok bool), ΔType>]
[assembly: GoImplement<types.Type, string, bool), ΔType>]
[assembly: GoImplement<ΔError, error>]
[assembly: GoImplement<ΔSignature, ΔType>]
[assembly: GoImplement<ΔSignature, ΔgenericType>]
[assembly: GoImplement<ΔconstDecl, decl>]
[assembly: GoImplement<ΔfuncDecl, decl>]
[assembly: GoImplement<ΔgenericType, ΔType>]
[assembly: GoImplement<ΔtypeDecl, decl>]
[assembly: GoImplement<ΔvarDecl, decl>]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<Alias, ж<Alias>>(Indirect = true)]
[assembly: GoImplicitConv<Array, ж<Array>>(Indirect = true)]
[assembly: GoImplicitConv<Basic, ж<Basic>>(Indirect = true)]
[assembly: GoImplicitConv<Chan, ж<Chan>>(Indirect = true)]
[assembly: GoImplicitConv<Const, ж<Const>>(Indirect = true)]
[assembly: GoImplicitConv<Context, ж<Context>>(Indirect = true)]
[assembly: GoImplicitConv<Func, ж<Func>>(Indirect = true)]
[assembly: GoImplicitConv<Interface, ж<Interface>>(Indirect = true)]
[assembly: GoImplicitConv<Label, ж<Label>>(Indirect = true)]
[assembly: GoImplicitConv<Map, ж<Map>>(Indirect = true)]
[assembly: GoImplicitConv<Named, ж<Named>>(Indirect = true)]
[assembly: GoImplicitConv<Package, ж<Package>>(Indirect = true)]
[assembly: GoImplicitConv<PkgName, ж<PkgName>>(Indirect = true)]
[assembly: GoImplicitConv<Pointer, ж<Pointer>>(Indirect = true)]
[assembly: GoImplicitConv<Slice, ж<Slice>>(Indirect = true)]
[assembly: GoImplicitConv<Struct, ж<Struct>>(Indirect = true)]
[assembly: GoImplicitConv<Tuple, ж<Tuple>>(Indirect = true)]
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
[assembly: GoImplicitConv<atPos, token.Pos>(Inverted = false, ValueType = "token.Pos")]
[assembly: GoImplicitConv<atPos, tokenꓸPos>(Inverted = false, ValueType = "tokenꓸPos")]
[assembly: GoImplicitConv<block, ж<block>>(Indirect = true)]
[assembly: GoImplicitConv<declInfo, ж<declInfo>>(Indirect = true)]
[assembly: GoImplicitConv<operand, ж<operand>>(Indirect = true)]
[assembly: GoImplicitConv<token.FileSet, ж<token.FileSet>>(Indirect = true)]
[assembly: GoImplicitConv<token.Pos; comment string; isFunc bool}, token.Pos; comment string; isFunc bool}>(Inverted = true)]
[assembly: GoImplicitConv<typeparams.IndexExpr, ж<typeparams.IndexExpr>>(Indirect = true)]
[assembly: GoImplicitConv<types.Package; complete bool; fake bool; cgo bool; goVersion string}, types.Package; complete bool; fake bool; cgo bool; goVersion string}>(Inverted = true)]
[assembly: GoImplicitConv<ΔInfo, ж<ΔInfo>>(Indirect = true)]
[assembly: GoImplicitConv<ΔSignature, ж<ΔSignature>>(Indirect = true)]
[assembly: GoImplicitConv<ΔTerm, ж<term>>(Indirect = true)]
// </ImplicitConversions>

namespace go.go;

[GoPackage("types")]
public static partial class types_package
{
}
