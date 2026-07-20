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
global using driverꓸRowsAffected = go.database.sql.driver_package.ΔRowsAffected;
global using driverꓸValue = object;
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
using driver = go.database.sql.driver_package;
// </ImportedTypeAliases>

using go;
using static go.database.sql_package;

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
[assembly: GoTypeAlias("Conn", "ΔConn")]
[assembly: GoTypeAlias("Stmt", "ΔStmt")]
// </ExportedTypeAliases>

// As types are cast to interfaces in Go source code, the go2cs code converter
// will generate an assembly level `GoImplement` attribute for each unique cast.
// This allows the interface to be implemented in the C# source code using source
// code generation (see go2cs-gen). An alternate interface implementation exists
// that can resolve duck-typed interfaces at run-time, but handling interface
// implementations at compile-time results in faster startup times, avoiding
// reflection-based interface resolution.

// <InterfaceImplementations>
[assembly: GoImplement<IsolationLevel, fmt_package.Stringer>]
[assembly: GoImplement<NullBool, Scanner>(Pointer = true)]
[assembly: GoImplement<NullByte, Scanner>(Pointer = true)]
[assembly: GoImplement<NullFloat64, Scanner>(Pointer = true)]
[assembly: GoImplement<NullInt16, Scanner>(Pointer = true)]
[assembly: GoImplement<NullInt32, Scanner>(Pointer = true)]
[assembly: GoImplement<NullInt64, Scanner>(Pointer = true)]
[assembly: GoImplement<NullString, Scanner>(Pointer = true)]
[assembly: GoImplement<NullTime, Scanner>(Pointer = true)]
[assembly: GoImplement<Tx, stmtConnGrabber>(Pointer = true)]
[assembly: GoImplement<driverConn, finalCloser>(Pointer = true)]
[assembly: GoImplement<driverConn, sync_package.Locker>(Pointer = true)]
[assembly: GoImplement<driverResult, Result>]
[assembly: GoImplement<driverResult, sync_package.Locker>(Promoted = true)]
[assembly: GoImplement<driverStmt, sync_package.Locker>(Promoted = true)]
[assembly: GoImplement<dsnConnector, go.database.sql.driver_package.Connector>]
[assembly: GoImplement<ΔConn, stmtConnGrabber>(Pointer = true)]
[assembly: GoImplement<ΔStmt, finalCloser>(Pointer = true)]
// </InterfaceImplementations>

// <ImplicitConversions>
[assembly: GoImplicitConv<IsolationLevel, driver.IsolationLevel>(Inverted = true, ValueType = "IsolationLevel")]
[assembly: GoImplicitConv<driverConn, ж<driverConn>>(Indirect = true)]
[assembly: GoImplicitConv<driverStmt, ж<driverStmt>>(Indirect = true)]
// </ImplicitConversions>

namespace go.database;

[GoPackage("sql")]
public static partial class sql_package
{
}
