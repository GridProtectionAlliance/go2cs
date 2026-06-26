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
global using reflectꓸChanDir = go.reflect_package.ΔChanDir;
global using reflectꓸKind = go.reflect_package.ΔKind;
global using reflectꓸMethod = go.reflect_package.ΔMethod;
global using reflectꓸType = go.reflect_package.ΔType;
global using reflectꓸValue = go.reflect_package.ΔValue;
global using runtimeꓸError = go.runtime_package.ΔError;
global using timeꓸLocation = go.time_package.ΔLocation;
global using timeꓸMonth = go.time_package.ΔMonth;
global using timeꓸWeekday = go.time_package.ΔWeekday;
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
[assembly: GoImplement<(Result, error), Result>]
[assembly: GoImplement<(Scanner, bool), Scanner>]
[assembly: GoImplement<(ctx context.Context, cancel context.CancelFunc), context_package.Context>]
[assembly: GoImplement<(io.Closer, bool), io_package.Closer>]
[assembly: GoImplement<(res Result, err error), Result>]
[assembly: GoImplement<IsolationLevel, fmt_package.Stringer>]
[assembly: GoImplement<Tx, stmtConnGrabber>]
[assembly: GoImplement<database.sql.driver_package.Value, database.sql.driver_package.Valuer>]
[assembly: GoImplement<driver.ColumnConverter, bool), database.sql.driver_package.ColumnConverter>]
[assembly: GoImplement<driver.Conn, error), database.sql.driver_package.Conn>]
[assembly: GoImplement<driver.ConnBeginTx, bool), database.sql.driver_package.ConnBeginTx>]
[assembly: GoImplement<driver.ConnPrepareContext, bool), database.sql.driver_package.ConnPrepareContext>]
[assembly: GoImplement<driver.Connector, error), database.sql.driver_package.Connector>]
[assembly: GoImplement<driver.Driver, bool), database.sql.driver_package.Driver>]
[assembly: GoImplement<driver.DriverContext, bool), database.sql.driver_package.DriverContext>]
[assembly: GoImplement<driver.Execer, bool), database.sql.driver_package.Execer>]
[assembly: GoImplement<driver.ExecerContext, bool), database.sql.driver_package.ExecerContext>]
[assembly: GoImplement<driver.NamedValueChecker, bool), database.sql.driver_package.NamedValueChecker>]
[assembly: GoImplement<driver.Pinger, bool), database.sql.driver_package.Pinger>]
[assembly: GoImplement<driver.Queryer, bool), database.sql.driver_package.Queryer>]
[assembly: GoImplement<driver.QueryerContext, bool), database.sql.driver_package.QueryerContext>]
[assembly: GoImplement<driver.Result, error), database.sql.driver_package.Result>]
[assembly: GoImplement<driver.Rows, error), database.sql.driver_package.Rows>]
[assembly: GoImplement<driver.RowsColumnTypeDatabaseTypeName, bool), database.sql.driver_package.RowsColumnTypeDatabaseTypeName>]
[assembly: GoImplement<driver.RowsColumnTypeLength, bool), database.sql.driver_package.RowsColumnTypeLength>]
[assembly: GoImplement<driver.RowsColumnTypeNullable, bool), database.sql.driver_package.RowsColumnTypeNullable>]
[assembly: GoImplement<driver.RowsColumnTypePrecisionScale, bool), database.sql.driver_package.RowsColumnTypePrecisionScale>]
[assembly: GoImplement<driver.RowsColumnTypeScanType, bool), database.sql.driver_package.RowsColumnTypeScanType>]
[assembly: GoImplement<driver.RowsNextResultSet, bool), database.sql.driver_package.RowsNextResultSet>]
[assembly: GoImplement<driver.SessionResetter, bool), database.sql.driver_package.SessionResetter>]
[assembly: GoImplement<driver.Stmt, error), database.sql.driver_package.Stmt>]
[assembly: GoImplement<driver.StmtExecContext, bool), database.sql.driver_package.StmtExecContext>]
[assembly: GoImplement<driver.StmtQueryContext, bool), database.sql.driver_package.StmtQueryContext>]
[assembly: GoImplement<driver.Tx, error), database.sql.driver_package.Tx>]
[assembly: GoImplement<driver.Validator, bool), database.sql.driver_package.Validator>]
[assembly: GoImplement<driver.Valuer, bool), database.sql.driver_package.Valuer>]
[assembly: GoImplement<driverConn, finalCloser>]
[assembly: GoImplement<driverConn, sync_package.Locker>]
[assembly: GoImplement<driverResult, Result>]
[assembly: GoImplement<dsnConnector, database.sql.driver_package.Connector>]
[assembly: GoImplement<error, Result>]
[assembly: GoImplement<ΔConn, stmtConnGrabber>]
[assembly: GoImplement<ΔStmt, finalCloser>]
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
