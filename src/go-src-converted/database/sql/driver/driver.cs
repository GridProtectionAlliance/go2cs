// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package driver defines interfaces to be implemented by database
// drivers as used by package sql.
//
// Most code should use package sql.
//
// The driver interface has evolved over time. Drivers should implement
// Connector and DriverContext interfaces.
// The Connector.Connect and Driver.Open methods should never return ErrBadConn.
// ErrBadConn should only be returned from Validator, SessionResetter, or
// a query method if the connection is already in an invalid (e.g. closed) state.
//
// All Conn implementations should implement the following interfaces:
// Pinger, SessionResetter, and Validator.
//
// If named parameters or context are supported, the driver's Conn should implement:
// ExecerContext, QueryerContext, ConnPrepareContext, and ConnBeginTx.
//
// To support custom data types, implement NamedValueChecker. NamedValueChecker
// also allows queries to accept per-query options as a parameter by returning
// ErrRemoveArgument from CheckNamedValue.
//
// If multiple result sets are supported, Rows should implement RowsNextResultSet.
// If the driver knows how to describe the types present in the returned result
// it should implement the following interfaces: RowsColumnTypeScanType,
// RowsColumnTypeDatabaseTypeName, RowsColumnTypeLength, RowsColumnTypeNullable,
// and RowsColumnTypePrecisionScale. A given row value may also return a Rows
// type, which may represent a database cursor value.
//
// Before a connection is returned to the connection pool after use, IsValid is
// called if implemented. Before a connection is reused for another query,
// ResetSession is called if implemented. If a connection is never returned to the
// connection pool but immediately reused, then ResetSession is called prior to
// reuse but IsValid is not called.
// package driver -- go2cs converted at 2020 October 08 04:58:46 UTC
// import "database/sql/driver" ==> using driver = go.database.sql.driver_package
// Original source: C:\Go\src\database\sql\driver\driver.go
using context = go.context_package;
using errors = go.errors_package;
using reflect = go.reflect_package;
using static go.builtin;

namespace go {
namespace database {
namespace sql
{
    public static partial class driver_package
    {
        // Value is a value that drivers must be able to handle.
        // It is either nil, a type handled by a database driver's NamedValueChecker
        // interface, or an instance of one of these types:
        //
        //   int64
        //   float64
        //   bool
        //   []byte
        //   string
        //   time.Time
        //
        // If the driver supports cursors, a returned Value may also implement the Rows interface
        // in this package. This is used, for example, when a user selects a cursor
        // such as "select cursor(select * from my_table) from dual". If the Rows
        // from the select is closed, the cursor Rows will also be closed.
        public partial interface Value
        {
        }

        // NamedValue holds both the value name and value.
        public partial struct NamedValue
        {
            public @string Name; // Ordinal position of the parameter starting from one and is always set.
            public long Ordinal; // Value is the parameter value.
            public Value Value;
        }

        // Driver is the interface that must be implemented by a database
        // driver.
        //
        // Database drivers may implement DriverContext for access
        // to contexts and to parse the name only once for a pool of connections,
        // instead of once per connection.
        public partial interface Driver
        {
            (Conn, error) Open(@string name);
        }

        // If a Driver implements DriverContext, then sql.DB will call
        // OpenConnector to obtain a Connector and then invoke
        // that Connector's Connect method to obtain each needed connection,
        // instead of invoking the Driver's Open method for each connection.
        // The two-step sequence allows drivers to parse the name just once
        // and also provides access to per-Conn contexts.
        public partial interface DriverContext
        {
            (Connector, error) OpenConnector(@string name);
        }

        // A Connector represents a driver in a fixed configuration
        // and can create any number of equivalent Conns for use
        // by multiple goroutines.
        //
        // A Connector can be passed to sql.OpenDB, to allow drivers
        // to implement their own sql.DB constructors, or returned by
        // DriverContext's OpenConnector method, to allow drivers
        // access to context and to avoid repeated parsing of driver
        // configuration.
        public partial interface Connector
        {
            Driver Connect(context.Context _p0); // Driver returns the underlying Driver of the Connector,
// mainly to maintain compatibility with the Driver method
// on sql.DB.
            Driver Driver();
        }

        // ErrSkip may be returned by some optional interfaces' methods to
        // indicate at runtime that the fast path is unavailable and the sql
        // package should continue as if the optional interface was not
        // implemented. ErrSkip is only supported where explicitly
        // documented.
        public static var ErrSkip = errors.New("driver: skip fast-path; continue as if unimplemented");

        // ErrBadConn should be returned by a driver to signal to the sql
        // package that a driver.Conn is in a bad state (such as the server
        // having earlier closed the connection) and the sql package should
        // retry on a new connection.
        //
        // To prevent duplicate operations, ErrBadConn should NOT be returned
        // if there's a possibility that the database server might have
        // performed the operation. Even if the server sends back an error,
        // you shouldn't return ErrBadConn.
        public static var ErrBadConn = errors.New("driver: bad connection");

        // Pinger is an optional interface that may be implemented by a Conn.
        //
        // If a Conn does not implement Pinger, the sql package's DB.Ping and
        // DB.PingContext will check if there is at least one Conn available.
        //
        // If Conn.Ping returns ErrBadConn, DB.Ping and DB.PingContext will remove
        // the Conn from pool.
        public partial interface Pinger
        {
            error Ping(context.Context ctx);
        }

        // Execer is an optional interface that may be implemented by a Conn.
        //
        // If a Conn implements neither ExecerContext nor Execer,
        // the sql package's DB.Exec will first prepare a query, execute the statement,
        // and then close the statement.
        //
        // Exec may return ErrSkip.
        //
        // Deprecated: Drivers should implement ExecerContext instead.
        public partial interface Execer
        {
            (Result, error) Exec(@string query, slice<Value> args);
        }

        // ExecerContext is an optional interface that may be implemented by a Conn.
        //
        // If a Conn does not implement ExecerContext, the sql package's DB.Exec
        // will fall back to Execer; if the Conn does not implement Execer either,
        // DB.Exec will first prepare a query, execute the statement, and then
        // close the statement.
        //
        // ExecerContext may return ErrSkip.
        //
        // ExecerContext must honor the context timeout and return when the context is canceled.
        public partial interface ExecerContext
        {
            (Result, error) ExecContext(context.Context ctx, @string query, slice<NamedValue> args);
        }

        // Queryer is an optional interface that may be implemented by a Conn.
        //
        // If a Conn implements neither QueryerContext nor Queryer,
        // the sql package's DB.Query will first prepare a query, execute the statement,
        // and then close the statement.
        //
        // Query may return ErrSkip.
        //
        // Deprecated: Drivers should implement QueryerContext instead.
        public partial interface Queryer
        {
            (Rows, error) Query(@string query, slice<Value> args);
        }

        // QueryerContext is an optional interface that may be implemented by a Conn.
        //
        // If a Conn does not implement QueryerContext, the sql package's DB.Query
        // will fall back to Queryer; if the Conn does not implement Queryer either,
        // DB.Query will first prepare a query, execute the statement, and then
        // close the statement.
        //
        // QueryerContext may return ErrSkip.
        //
        // QueryerContext must honor the context timeout and return when the context is canceled.
        public partial interface QueryerContext
        {
            (Rows, error) QueryContext(context.Context ctx, @string query, slice<NamedValue> args);
        }

        // Conn is a connection to a database. It is not used concurrently
        // by multiple goroutines.
        //
        // Conn is assumed to be stateful.
        public partial interface Conn
        {
            (Tx, error) Prepare(@string query); // Close invalidates and potentially stops any current
// prepared statements and transactions, marking this
// connection as no longer in use.
//
// Because the sql package maintains a free pool of
// connections and only calls Close when there's a surplus of
// idle connections, it shouldn't be necessary for drivers to
// do their own connection caching.
//
// Drivers must ensure all network calls made by Close
// do not block indefinitely (e.g. apply a timeout).
            (Tx, error) Close(); // Begin starts and returns a new transaction.
//
// Deprecated: Drivers should implement ConnBeginTx instead (or additionally).
            (Tx, error) Begin();
        }

        // ConnPrepareContext enhances the Conn interface with context.
        public partial interface ConnPrepareContext
        {
            (Stmt, error) PrepareContext(context.Context ctx, @string query);
        }

        // IsolationLevel is the transaction isolation level stored in TxOptions.
        //
        // This type should be considered identical to sql.IsolationLevel along
        // with any values defined on it.
        public partial struct IsolationLevel // : long
        {
        }

        // TxOptions holds the transaction options.
        //
        // This type should be considered identical to sql.TxOptions.
        public partial struct TxOptions
        {
            public IsolationLevel Isolation;
            public bool ReadOnly;
        }

        // ConnBeginTx enhances the Conn interface with context and TxOptions.
        public partial interface ConnBeginTx
        {
            (Tx, error) BeginTx(context.Context ctx, TxOptions opts);
        }

        // SessionResetter may be implemented by Conn to allow drivers to reset the
        // session state associated with the connection and to signal a bad connection.
        public partial interface SessionResetter
        {
            error ResetSession(context.Context ctx);
        }

        // Validator may be implemented by Conn to allow drivers to
        // signal if a connection is valid or if it should be discarded.
        //
        // If implemented, drivers may return the underlying error from queries,
        // even if the connection should be discarded by the connection pool.
        public partial interface Validator
        {
            bool IsValid();
        }

        // Result is the result of a query execution.
        public partial interface Result
        {
            (long, error) LastInsertId(); // RowsAffected returns the number of rows affected by the
// query.
            (long, error) RowsAffected();
        }

        // Stmt is a prepared statement. It is bound to a Conn and not
        // used by multiple goroutines concurrently.
        public partial interface Stmt
        {
            (Rows, error) Close(); // NumInput returns the number of placeholder parameters.
//
// If NumInput returns >= 0, the sql package will sanity check
// argument counts from callers and return errors to the caller
// before the statement's Exec or Query methods are called.
//
// NumInput may also return -1, if the driver doesn't know
// its number of placeholders. In that case, the sql package
// will not sanity check Exec or Query argument counts.
            (Rows, error) NumInput(); // Exec executes a query that doesn't return rows, such
// as an INSERT or UPDATE.
//
// Deprecated: Drivers should implement StmtExecContext instead (or additionally).
            (Rows, error) Exec(slice<Value> args); // Query executes a query that may return rows, such as a
// SELECT.
//
// Deprecated: Drivers should implement StmtQueryContext instead (or additionally).
            (Rows, error) Query(slice<Value> args);
        }

        // StmtExecContext enhances the Stmt interface by providing Exec with context.
        public partial interface StmtExecContext
        {
            (Result, error) ExecContext(context.Context ctx, slice<NamedValue> args);
        }

        // StmtQueryContext enhances the Stmt interface by providing Query with context.
        public partial interface StmtQueryContext
        {
            (Rows, error) QueryContext(context.Context ctx, slice<NamedValue> args);
        }

        // ErrRemoveArgument may be returned from NamedValueChecker to instruct the
        // sql package to not pass the argument to the driver query interface.
        // Return when accepting query specific options or structures that aren't
        // SQL query arguments.
        public static var ErrRemoveArgument = errors.New("driver: remove argument from query");

        // NamedValueChecker may be optionally implemented by Conn or Stmt. It provides
        // the driver more control to handle Go and database types beyond the default
        // Values types allowed.
        //
        // The sql package checks for value checkers in the following order,
        // stopping at the first found match: Stmt.NamedValueChecker, Conn.NamedValueChecker,
        // Stmt.ColumnConverter, DefaultParameterConverter.
        //
        // If CheckNamedValue returns ErrRemoveArgument, the NamedValue will not be included in
        // the final query arguments. This may be used to pass special options to
        // the query itself.
        //
        // If ErrSkip is returned the column converter error checking
        // path is used for the argument. Drivers may wish to return ErrSkip after
        // they have exhausted their own special cases.
        public partial interface NamedValueChecker
        {
            error CheckNamedValue(ptr<NamedValue> _p0);
        }

        // ColumnConverter may be optionally implemented by Stmt if the
        // statement is aware of its own columns' types and can convert from
        // any type to a driver Value.
        //
        // Deprecated: Drivers should implement NamedValueChecker.
        public partial interface ColumnConverter
        {
            ValueConverter ColumnConverter(long idx);
        }

        // Rows is an iterator over an executed query's results.
        public partial interface Rows
        {
            error Columns(); // Close closes the rows iterator.
            error Close(); // Next is called to populate the next row of data into
// the provided slice. The provided slice will be the same
// size as the Columns() are wide.
//
// Next should return io.EOF when there are no more rows.
//
// The dest should not be written to outside of Next. Care
// should be taken when closing Rows not to modify
// a buffer held in dest.
            error Next(slice<Value> dest);
        }

        // RowsNextResultSet extends the Rows interface by providing a way to signal
        // the driver to advance to the next result set.
        public partial interface RowsNextResultSet : Rows
        {
            error HasNextResultSet(); // NextResultSet advances the driver to the next result set even
// if there are remaining rows in the current result set.
//
// NextResultSet should return io.EOF when there are no more result sets.
            error NextResultSet();
        }

        // RowsColumnTypeScanType may be implemented by Rows. It should return
        // the value type that can be used to scan types into. For example, the database
        // column type "bigint" this should return "reflect.TypeOf(int64(0))".
        public partial interface RowsColumnTypeScanType : Rows
        {
            reflect.Type ColumnTypeScanType(long index);
        }

        // RowsColumnTypeDatabaseTypeName may be implemented by Rows. It should return the
        // database system type name without the length. Type names should be uppercase.
        // Examples of returned types: "VARCHAR", "NVARCHAR", "VARCHAR2", "CHAR", "TEXT",
        // "DECIMAL", "SMALLINT", "INT", "BIGINT", "BOOL", "[]BIGINT", "JSONB", "XML",
        // "TIMESTAMP".
        public partial interface RowsColumnTypeDatabaseTypeName : Rows
        {
            @string ColumnTypeDatabaseTypeName(long index);
        }

        // RowsColumnTypeLength may be implemented by Rows. It should return the length
        // of the column type if the column is a variable length type. If the column is
        // not a variable length type ok should return false.
        // If length is not limited other than system limits, it should return math.MaxInt64.
        // The following are examples of returned values for various types:
        //   TEXT          (math.MaxInt64, true)
        //   varchar(10)   (10, true)
        //   nvarchar(10)  (10, true)
        //   decimal       (0, false)
        //   int           (0, false)
        //   bytea(30)     (30, true)
        public partial interface RowsColumnTypeLength : Rows
        {
            (long, bool) ColumnTypeLength(long index);
        }

        // RowsColumnTypeNullable may be implemented by Rows. The nullable value should
        // be true if it is known the column may be null, or false if the column is known
        // to be not nullable.
        // If the column nullability is unknown, ok should be false.
        public partial interface RowsColumnTypeNullable : Rows
        {
            (bool, bool) ColumnTypeNullable(long index);
        }

        // RowsColumnTypePrecisionScale may be implemented by Rows. It should return
        // the precision and scale for decimal types. If not applicable, ok should be false.
        // The following are examples of returned values for various types:
        //   decimal(38, 4)    (38, 4, true)
        //   int               (0, 0, false)
        //   decimal           (math.MaxInt64, math.MaxInt64, true)
        public partial interface RowsColumnTypePrecisionScale : Rows
        {
            (long, long, bool) ColumnTypePrecisionScale(long index);
        }

        // Tx is a transaction.
        public partial interface Tx
        {
            error Commit();
            error Rollback();
        }

        // RowsAffected implements Result for an INSERT or UPDATE operation
        // which mutates a number of rows.
        public partial struct RowsAffected // : long
        {
        }

        private static Result _ = Result.As(RowsAffected(0L))!;

        public static (long, error) LastInsertId(this RowsAffected _p0)
        {
            long _p0 = default;
            error _p0 = default!;

            return (0L, error.As(errors.New("LastInsertId is not supported by this driver"))!);
        }

        public static (long, error) RowsAffected(this RowsAffected v)
        {
            long _p0 = default;
            error _p0 = default!;

            return (int64(v), error.As(null!)!);
        }

        // ResultNoRows is a pre-defined Result for drivers to return when a DDL
        // command (such as a CREATE TABLE) succeeds. It returns an error for both
        // LastInsertId and RowsAffected.
        public static noRows ResultNoRows = default;

        private partial struct noRows
        {
        }

        private static Result _ = Result.As(new noRows())!;

        private static (long, error) LastInsertId(this noRows _p0)
        {
            long _p0 = default;
            error _p0 = default!;

            return (0L, error.As(errors.New("no LastInsertId available after DDL statement"))!);
        }

        private static (long, error) RowsAffected(this noRows _p0)
        {
            long _p0 = default;
            error _p0 = default!;

            return (0L, error.As(errors.New("no RowsAffected available after DDL statement"))!);
        }
    }
}}}
