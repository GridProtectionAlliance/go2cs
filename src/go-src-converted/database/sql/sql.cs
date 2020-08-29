// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sql provides a generic interface around SQL (or SQL-like)
// databases.
//
// The sql package must be used in conjunction with a database driver.
// See https://golang.org/s/sqldrivers for a list of drivers.
//
// Drivers that do not support context cancelation will not return until
// after the query is completed.
//
// For usage examples, see the wiki page at
// https://golang.org/s/sqlwiki.
// package sql -- go2cs converted at 2020 August 29 10:11:00 UTC
// import "database/sql" ==> using sql = go.database.sql_package
// Original source: C:\Go\src\database\sql\sql.go
using context = go.context_package;
using driver = go.database.sql.driver_package;
using errors = go.errors_package;
using fmt = go.fmt_package;
using io = go.io_package;
using reflect = go.reflect_package;
using runtime = go.runtime_package;
using sort = go.sort_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace database
{
    public static partial class sql_package
    {
        private static sync.RWMutex driversMu = default;        private static var drivers = make_map<@string, driver.Driver>();

        // nowFunc returns the current time; it's overridden in tests.
        private static var nowFunc = time.Now;

        // Register makes a database driver available by the provided name.
        // If Register is called twice with the same name or if driver is nil,
        // it panics.
        public static void Register(@string name, driver.Driver driver) => func((defer, panic, _) =>
        {
            driversMu.Lock();
            defer(driversMu.Unlock());
            if (driver == null)
            {
                panic("sql: Register driver is nil");
            }
            {
                var (_, dup) = drivers[name];

                if (dup)
                {
                    panic("sql: Register called twice for driver " + name);
                }

            }
            drivers[name] = driver;
        });

        private static void unregisterAllDrivers() => func((defer, _, __) =>
        {
            driversMu.Lock();
            defer(driversMu.Unlock()); 
            // For tests.
            drivers = make_map<@string, driver.Driver>();
        });

        // Drivers returns a sorted list of the names of the registered drivers.
        public static slice<@string> Drivers() => func((defer, _, __) =>
        {
            driversMu.RLock();
            defer(driversMu.RUnlock());
            slice<@string> list = default;
            foreach (var (name) in drivers)
            {
                list = append(list, name);
            }
            sort.Strings(list);
            return list;
        });

        // A NamedArg is a named argument. NamedArg values may be used as
        // arguments to Query or Exec and bind to the corresponding named
        // parameter in the SQL statement.
        //
        // For a more concise way to create NamedArg values, see
        // the Named function.
        public partial struct NamedArg
        {
            public @string Name; // Value is the value of the parameter.
// It may be assigned the same value types as the query
// arguments.
        }

        // Named provides a more concise way to create NamedArg values.
        //
        // Example usage:
        //
        //     db.ExecContext(ctx, `
        //         delete from Invoice
        //         where
        //             TimeCreated < @end
        //             and TimeCreated >= @start;`,
        //         sql.Named("start", startTime),
        //         sql.Named("end", endTime),
        //     )
        public static NamedArg Named(@string name, object value)
        { 
            // This method exists because the go1compat promise
            // doesn't guarantee that structs don't grow more fields,
            // so unkeyed struct literals are a vet error. Thus, we don't
            // want to allow sql.NamedArg{name, value}.
            return new NamedArg(Name:name,Value:value);
        }

        // IsolationLevel is the transaction isolation level used in TxOptions.
        public partial struct IsolationLevel // : long
        {
        }

        // Various isolation levels that drivers may support in BeginTx.
        // If a driver does not support a given isolation level an error may be returned.
        //
        // See https://en.wikipedia.org/wiki/Isolation_(database_systems)#Isolation_levels.
        public static readonly IsolationLevel LevelDefault = iota;
        public static readonly var LevelReadUncommitted = 0;
        public static readonly var LevelReadCommitted = 1;
        public static readonly var LevelWriteCommitted = 2;
        public static readonly var LevelRepeatableRead = 3;
        public static readonly var LevelSnapshot = 4;
        public static readonly var LevelSerializable = 5;
        public static readonly var LevelLinearizable = 6;

        // TxOptions holds the transaction options to be used in DB.BeginTx.
        public partial struct TxOptions
        {
            public IsolationLevel Isolation;
            public bool ReadOnly;
        }

        // RawBytes is a byte slice that holds a reference to memory owned by
        // the database itself. After a Scan into a RawBytes, the slice is only
        // valid until the next call to Next, Scan, or Close.
        public partial struct RawBytes // : slice<byte>
        {
        }

        // NullString represents a string that may be null.
        // NullString implements the Scanner interface so
        // it can be used as a scan destination:
        //
        //  var s NullString
        //  err := db.QueryRow("SELECT name FROM foo WHERE id=?", id).Scan(&s)
        //  ...
        //  if s.Valid {
        //     // use s.String
        //  } else {
        //     // NULL value
        //  }
        //
        public partial struct NullString
        {
            public @string String;
            public bool Valid; // Valid is true if String is not NULL
        }

        // Scan implements the Scanner interface.
        private static error Scan(this ref NullString ns, object value)
        {
            if (value == null)
            {
                ns.String = "";
                ns.Valid = false;
                return error.As(null);
            }
            ns.Valid = true;
            return error.As(convertAssign(ref ns.String, value));
        }

        // Value implements the driver Valuer interface.
        public static (driver.Value, error) Value(this NullString ns)
        {
            if (!ns.Valid)
            {
                return (null, null);
            }
            return (ns.String, null);
        }

        // NullInt64 represents an int64 that may be null.
        // NullInt64 implements the Scanner interface so
        // it can be used as a scan destination, similar to NullString.
        public partial struct NullInt64
        {
            public long Int64;
            public bool Valid; // Valid is true if Int64 is not NULL
        }

        // Scan implements the Scanner interface.
        private static error Scan(this ref NullInt64 n, object value)
        {
            if (value == null)
            {
                n.Int64 = 0L;
                n.Valid = false;
                return error.As(null);
            }
            n.Valid = true;
            return error.As(convertAssign(ref n.Int64, value));
        }

        // Value implements the driver Valuer interface.
        public static (driver.Value, error) Value(this NullInt64 n)
        {
            if (!n.Valid)
            {
                return (null, null);
            }
            return (n.Int64, null);
        }

        // NullFloat64 represents a float64 that may be null.
        // NullFloat64 implements the Scanner interface so
        // it can be used as a scan destination, similar to NullString.
        public partial struct NullFloat64
        {
            public double Float64;
            public bool Valid; // Valid is true if Float64 is not NULL
        }

        // Scan implements the Scanner interface.
        private static error Scan(this ref NullFloat64 n, object value)
        {
            if (value == null)
            {
                n.Float64 = 0L;
                n.Valid = false;
                return error.As(null);
            }
            n.Valid = true;
            return error.As(convertAssign(ref n.Float64, value));
        }

        // Value implements the driver Valuer interface.
        public static (driver.Value, error) Value(this NullFloat64 n)
        {
            if (!n.Valid)
            {
                return (null, null);
            }
            return (n.Float64, null);
        }

        // NullBool represents a bool that may be null.
        // NullBool implements the Scanner interface so
        // it can be used as a scan destination, similar to NullString.
        public partial struct NullBool
        {
            public bool Bool;
            public bool Valid; // Valid is true if Bool is not NULL
        }

        // Scan implements the Scanner interface.
        private static error Scan(this ref NullBool n, object value)
        {
            if (value == null)
            {
                n.Bool = false;
                n.Valid = false;
                return error.As(null);
            }
            n.Valid = true;
            return error.As(convertAssign(ref n.Bool, value));
        }

        // Value implements the driver Valuer interface.
        public static (driver.Value, error) Value(this NullBool n)
        {
            if (!n.Valid)
            {
                return (null, null);
            }
            return (n.Bool, null);
        }

        // Scanner is an interface used by Scan.
        public partial interface Scanner
        {
            error Scan(object src);
        }

        // Out may be used to retrieve OUTPUT value parameters from stored procedures.
        //
        // Not all drivers and databases support OUTPUT value parameters.
        //
        // Example usage:
        //
        //   var outArg string
        //   _, err := db.ExecContext(ctx, "ProcName", sql.Named("Arg1", sql.Out{Dest: &outArg}))
        public partial struct Out
        {
            public bool In;
        }

        // ErrNoRows is returned by Scan when QueryRow doesn't return a
        // row. In such a case, QueryRow returns a placeholder *Row value that
        // defers this error until a Scan.
        public static var ErrNoRows = errors.New("sql: no rows in result set");

        // DB is a database handle representing a pool of zero or more
        // underlying connections. It's safe for concurrent use by multiple
        // goroutines.
        //
        // The sql package creates and frees connections automatically; it
        // also maintains a free pool of idle connections. If the database has
        // a concept of per-connection state, such state can only be reliably
        // observed within a transaction. Once DB.Begin is called, the
        // returned Tx is bound to a single connection. Once Commit or
        // Rollback is called on the transaction, that transaction's
        // connection is returned to DB's idle connection pool. The pool size
        // can be controlled with SetMaxIdleConns.
        public partial struct DB
        {
            public driver.Connector connector; // numClosed is an atomic counter which represents a total number of
// closed connections. Stmt.openStmt checks it before cleaning closed
// connections in Stmt.css.
            public ulong numClosed;
            public sync.Mutex mu; // protects following fields
            public slice<ref driverConn> freeConn;
            public map<ulong, channel<connRequest>> connRequests;
            public ulong nextRequest; // Next key to use in connRequests.
            public long numOpen; // number of opened and pending open connections
// Used to signal the need for new connections
// a goroutine running connectionOpener() reads on this chan and
// maybeOpenNewConnections sends on the chan (one send per needed connection)
// It is closed during db.Close(). The close tells the connectionOpener
// goroutine to exit.
            public channel<object> openerCh;
            public channel<ref driverConn> resetterCh;
            public bool closed;
            public map<finalCloser, depSet> dep;
            public map<ref driverConn, @string> lastPut; // stacktrace of last conn's put; debug only
            public long maxIdle; // zero means defaultMaxIdleConns; negative means 0
            public long maxOpen; // <= 0 means unlimited
            public time.Duration maxLifetime; // maximum amount of time a connection may be reused
            public channel<object> cleanerCh;
            public Action stop; // stop cancels the connection opener and the session resetter.
        }

        // connReuseStrategy determines how (*DB).conn returns database connections.
        private partial struct connReuseStrategy // : byte
        {
        }

 
        // alwaysNewConn forces a new connection to the database.
        private static readonly connReuseStrategy alwaysNewConn = iota; 
        // cachedOrNewConn returns a cached connection, if available, else waits
        // for one to become available (if MaxOpenConns has been reached) or
        // creates a new database connection.
        private static readonly var cachedOrNewConn = 0;

        // driverConn wraps a driver.Conn with a mutex, to
        // be held during all calls into the Conn. (including any calls onto
        // interfaces returned via that Conn, such as calls on Tx, Stmt,
        // Result, Rows)
        private partial struct driverConn
        {
            public ptr<DB> db;
            public time.Time createdAt;
            public ref sync.Mutex Mutex => ref Mutex_val; // guards following
            public driver.Conn ci;
            public bool closed;
            public bool finalClosed; // ci.Close has been called
            public map<ref driverStmt, bool> openStmt;
            public error lastErr; // lastError captures the result of the session resetter.

// guarded by db.mu
            public bool inUse;
            public slice<Action> onPut; // code (with db.mu held) run when conn is next returned
            public bool dbmuClosed; // same as closed, but guarded by db.mu, for removeClosedStmtLocked
        }

        private static void releaseConn(this ref driverConn dc, error err)
        {
            dc.db.putConn(dc, err, true);
        }

        private static void removeOpenStmt(this ref driverConn _dc, ref driverStmt _ds) => func(_dc, _ds, (ref driverConn dc, ref driverStmt ds, Defer defer, Panic _, Recover __) =>
        {
            dc.Lock();
            defer(dc.Unlock());
            delete(dc.openStmt, ds);
        });

        private static bool expired(this ref driverConn dc, time.Duration timeout)
        {
            if (timeout <= 0L)
            {
                return false;
            }
            return dc.createdAt.Add(timeout).Before(nowFunc());
        }

        // prepareLocked prepares the query on dc. When cg == nil the dc must keep track of
        // the prepared statements in a pool.
        private static (ref driverStmt, error) prepareLocked(this ref driverConn dc, context.Context ctx, stmtConnGrabber cg, @string query)
        {
            var (si, err) = ctxDriverPrepare(ctx, dc.ci, query);
            if (err != null)
            {
                return (null, err);
            }
            driverStmt ds = ref new driverStmt(Locker:dc,si:si); 

            // No need to manage open statements if there is a single connection grabber.
            if (cg != null)
            {
                return (ds, null);
            } 

            // Track each driverConn's open statements, so we can close them
            // before closing the conn.
            //
            // Wrap all driver.Stmt is *driverStmt to ensure they are only closed once.
            if (dc.openStmt == null)
            {
                dc.openStmt = make_map<ref driverStmt, bool>();
            }
            dc.openStmt[ds] = true;
            return (ds, null);
        }

        // resetSession resets the connection session and sets the lastErr
        // that is checked before returning the connection to another query.
        //
        // resetSession assumes that the embedded mutex is locked when the connection
        // was returned to the pool. This unlocks the mutex.
        private static void resetSession(this ref driverConn _dc, context.Context ctx) => func(_dc, (ref driverConn dc, Defer defer, Panic _, Recover __) =>
        {
            defer(dc.Unlock()); // In case of panic.
            if (dc.closed)
            { // Check if the database has been closed.
                return;
            }
            dc.lastErr = dc.ci._<driver.SessionResetter>().ResetSession(ctx);
        });

        // the dc.db's Mutex is held.
        private static Func<error> closeDBLocked(this ref driverConn _dc) => func(_dc, (ref driverConn dc, Defer defer, Panic _, Recover __) =>
        {
            dc.Lock();
            defer(dc.Unlock());
            if (dc.closed)
            {
                return () => errors.New("sql: duplicate driverConn close");
            }
            dc.closed = true;
            return dc.db.removeDepLocked(dc, dc);
        });

        private static error Close(this ref driverConn dc)
        {
            dc.Lock();
            if (dc.closed)
            {
                dc.Unlock();
                return error.As(errors.New("sql: duplicate driverConn close"));
            }
            dc.closed = true;
            dc.Unlock(); // not defer; removeDep finalClose calls may need to lock

            // And now updates that require holding dc.mu.Lock.
            dc.db.mu.Lock();
            dc.dbmuClosed = true;
            var fn = dc.db.removeDepLocked(dc, dc);
            dc.db.mu.Unlock();
            return error.As(fn());
        }

        private static error finalClose(this ref driverConn dc)
        {
            error err = default; 

            // Each *driverStmt has a lock to the dc. Copy the list out of the dc
            // before calling close on each stmt.
            slice<ref driverStmt> openStmt = default;
            withLock(dc, () =>
            {
                openStmt = make_slice<ref driverStmt>(0L, len(dc.openStmt));
                {
                    var ds__prev1 = ds;

                    foreach (var (__ds) in dc.openStmt)
                    {
                        ds = __ds;
                        openStmt = append(openStmt, ds);
                    }

                    ds = ds__prev1;
                }

                dc.openStmt = null;
            });
            {
                var ds__prev1 = ds;

                foreach (var (_, __ds) in openStmt)
                {
                    ds = __ds;
                    ds.Close();
                }

                ds = ds__prev1;
            }

            withLock(dc, () =>
            {
                dc.finalClosed = true;
                err = error.As(dc.ci.Close());
                dc.ci = null;
            });

            dc.db.mu.Lock();
            dc.db.numOpen--;
            dc.db.maybeOpenNewConnections();
            dc.db.mu.Unlock();

            atomic.AddUint64(ref dc.db.numClosed, 1L);
            return error.As(err);
        }

        // driverStmt associates a driver.Stmt with the
        // *driverConn from which it came, so the driverConn's lock can be
        // held during calls.
        private partial struct driverStmt : sync.Locker
        {
            public ref sync.Locker Locker => ref Locker_val; // the *driverConn
            public driver.Stmt si;
            public bool closed;
            public error closeErr; // return value of previous Close call
        }

        // Close ensures dirver.Stmt is only closed once any always returns the same
        // result.
        private static error Close(this ref driverStmt _ds) => func(_ds, (ref driverStmt ds, Defer defer, Panic _, Recover __) =>
        {
            ds.Lock();
            defer(ds.Unlock());
            if (ds.closed)
            {
                return error.As(ds.closeErr);
            }
            ds.closed = true;
            ds.closeErr = ds.si.Close();
            return error.As(ds.closeErr);
        });

        // depSet is a finalCloser's outstanding dependencies
        private partial interface finalCloser
        {
            error finalClose();
        }

        // addDep notes that x now depends on dep, and x's finalClose won't be
        // called until all of x's dependencies are removed with removeDep.
        private static void addDep(this ref DB _db, finalCloser x, object dep) => func(_db, (ref DB db, Defer defer, Panic _, Recover __) =>
        { 
            //println(fmt.Sprintf("addDep(%T %p, %T %p)", x, x, dep, dep))
            db.mu.Lock();
            defer(db.mu.Unlock());
            db.addDepLocked(x, dep);
        });

        private static void addDepLocked(this ref DB db, finalCloser x, object dep)
        {
            if (db.dep == null)
            {
                db.dep = make_map<finalCloser, depSet>();
            }
            var xdep = db.dep[x];
            if (xdep == null)
            {
                xdep = make(depSet);
                db.dep[x] = xdep;
            }
            xdep[dep] = true;
        }

        // removeDep notes that x no longer depends on dep.
        // If x still has dependencies, nil is returned.
        // If x no longer has any dependencies, its finalClose method will be
        // called and its error value will be returned.
        private static error removeDep(this ref DB db, finalCloser x, object dep)
        {
            db.mu.Lock();
            var fn = db.removeDepLocked(x, dep);
            db.mu.Unlock();
            return error.As(fn());
        }

        private static Func<error> removeDepLocked(this ref DB _db, finalCloser x, object dep) => func(_db, (ref DB db, Defer _, Panic panic, Recover __) =>
        { 
            //println(fmt.Sprintf("removeDep(%T %p, %T %p)", x, x, dep, dep))

            var (xdep, ok) = db.dep[x];
            if (!ok)
            {
                panic(fmt.Sprintf("unpaired removeDep: no deps for %T", x));
            }
            var l0 = len(xdep);
            delete(xdep, dep);


            if (len(xdep) == l0) 
                // Nothing removed. Shouldn't happen.
                panic(fmt.Sprintf("unpaired removeDep: no %T dep on %T", dep, x));
            else if (len(xdep) == 0L) 
                // No more dependencies.
                delete(db.dep, x);
                return x.finalClose;
            else 
                // Dependencies remain.
                return () => null;
                    });

        // This is the size of the connectionOpener request chan (DB.openerCh).
        // This value should be larger than the maximum typical value
        // used for db.maxOpen. If maxOpen is significantly larger than
        // connectionRequestQueueSize then it is possible for ALL calls into the *DB
        // to block until the connectionOpener can satisfy the backlog of requests.
        private static long connectionRequestQueueSize = 1000000L;

        private partial struct dsnConnector
        {
            public @string dsn;
            public driver.Driver driver;
        }

        private static (driver.Conn, error) Connect(this dsnConnector t, context.Context _)
        {
            return t.driver.Open(t.dsn);
        }

        private static driver.Driver Driver(this dsnConnector t)
        {
            return t.driver;
        }

        // OpenDB opens a database using a Connector, allowing drivers to
        // bypass a string based data source name.
        //
        // Most users will open a database via a driver-specific connection
        // helper function that returns a *DB. No database drivers are included
        // in the Go standard library. See https://golang.org/s/sqldrivers for
        // a list of third-party drivers.
        //
        // OpenDB may just validate its arguments without creating a connection
        // to the database. To verify that the data source name is valid, call
        // Ping.
        //
        // The returned DB is safe for concurrent use by multiple goroutines
        // and maintains its own pool of idle connections. Thus, the OpenDB
        // function should be called just once. It is rarely necessary to
        // close a DB.
        public static ref DB OpenDB(driver.Connector c)
        {
            var (ctx, cancel) = context.WithCancel(context.Background());
            DB db = ref new DB(connector:c,openerCh:make(chanstruct{},connectionRequestQueueSize),resetterCh:make(chan*driverConn,50),lastPut:make(map[*driverConn]string),connRequests:make(map[uint64]chanconnRequest),stop:cancel,);

            go_(() => db.connectionOpener(ctx));
            go_(() => db.connectionResetter(ctx));

            return db;
        }

        // Open opens a database specified by its database driver name and a
        // driver-specific data source name, usually consisting of at least a
        // database name and connection information.
        //
        // Most users will open a database via a driver-specific connection
        // helper function that returns a *DB. No database drivers are included
        // in the Go standard library. See https://golang.org/s/sqldrivers for
        // a list of third-party drivers.
        //
        // Open may just validate its arguments without creating a connection
        // to the database. To verify that the data source name is valid, call
        // Ping.
        //
        // The returned DB is safe for concurrent use by multiple goroutines
        // and maintains its own pool of idle connections. Thus, the Open
        // function should be called just once. It is rarely necessary to
        // close a DB.
        public static (ref DB, error) Open(@string driverName, @string dataSourceName)
        {
            driversMu.RLock();
            var (driveri, ok) = drivers[driverName];
            driversMu.RUnlock();
            if (!ok)
            {
                return (null, fmt.Errorf("sql: unknown driver %q (forgotten import?)", driverName));
            }
            {
                driver.DriverContext (driverCtx, ok) = driveri._<driver.DriverContext>();

                if (ok)
                {
                    var (connector, err) = driverCtx.OpenConnector(dataSourceName);
                    if (err != null)
                    {
                        return (null, err);
                    }
                    return (OpenDB(connector), null);
                }

            }

            return (OpenDB(new dsnConnector(dsn:dataSourceName,driver:driveri)), null);
        }

        private static error pingDC(this ref DB db, context.Context ctx, ref driverConn dc, Action<error> release)
        {
            error err = default;
            {
                driver.Pinger (pinger, ok) = dc.ci._<driver.Pinger>();

                if (ok)
                {
                    withLock(dc, () =>
                    {
                        err = error.As(pinger.Ping(ctx));
                    });
                }

            }
            release(err);
            return error.As(err);
        }

        // PingContext verifies a connection to the database is still alive,
        // establishing a connection if necessary.
        private static error PingContext(this ref DB db, context.Context ctx)
        {
            ref driverConn dc = default;
            error err = default;

            for (long i = 0L; i < maxBadConnRetries; i++)
            {
                dc, err = db.conn(ctx, cachedOrNewConn);
                if (err != driver.ErrBadConn)
                {
                    break;
                }
            }

            if (err == driver.ErrBadConn)
            {
                dc, err = db.conn(ctx, alwaysNewConn);
            }
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(db.pingDC(ctx, dc, dc.releaseConn));
        }

        // Ping verifies a connection to the database is still alive,
        // establishing a connection if necessary.
        private static error Ping(this ref DB db)
        {
            return error.As(db.PingContext(context.Background()));
        }

        // Close closes the database, releasing any open resources.
        //
        // It is rare to Close a DB, as the DB handle is meant to be
        // long-lived and shared between many goroutines.
        private static error Close(this ref DB db)
        {
            db.mu.Lock();
            if (db.closed)
            { // Make DB.Close idempotent
                db.mu.Unlock();
                return error.As(null);
            }
            if (db.cleanerCh != null)
            {
                close(db.cleanerCh);
            }
            error err = default;
            var fns = make_slice<Func<error>>(0L, len(db.freeConn));
            foreach (var (_, dc) in db.freeConn)
            {
                fns = append(fns, dc.closeDBLocked());
            }
            db.freeConn = null;
            db.closed = true;
            foreach (var (_, req) in db.connRequests)
            {
                close(req);
            }
            db.mu.Unlock();
            foreach (var (_, fn) in fns)
            {
                var err1 = fn();
                if (err1 != null)
                {
                    err = error.As(err1);
                }
            }
            db.stop();
            return error.As(err);
        }

        private static readonly long defaultMaxIdleConns = 2L;



        private static long maxIdleConnsLocked(this ref DB db)
        {
            var n = db.maxIdle;

            if (n == 0L) 
                // TODO(bradfitz): ask driver, if supported, for its default preference
                return defaultMaxIdleConns;
            else if (n < 0L) 
                return 0L;
            else 
                return n;
                    }

        // SetMaxIdleConns sets the maximum number of connections in the idle
        // connection pool.
        //
        // If MaxOpenConns is greater than 0 but less than the new MaxIdleConns
        // then the new MaxIdleConns will be reduced to match the MaxOpenConns limit
        //
        // If n <= 0, no idle connections are retained.
        private static void SetMaxIdleConns(this ref DB db, long n)
        {
            db.mu.Lock();
            if (n > 0L)
            {
                db.maxIdle = n;
            }
            else
            { 
                // No idle connections.
                db.maxIdle = -1L;
            } 
            // Make sure maxIdle doesn't exceed maxOpen
            if (db.maxOpen > 0L && db.maxIdleConnsLocked() > db.maxOpen)
            {
                db.maxIdle = db.maxOpen;
            }
            slice<ref driverConn> closing = default;
            var idleCount = len(db.freeConn);
            var maxIdle = db.maxIdleConnsLocked();
            if (idleCount > maxIdle)
            {
                closing = db.freeConn[maxIdle..];
                db.freeConn = db.freeConn[..maxIdle];
            }
            db.mu.Unlock();
            foreach (var (_, c) in closing)
            {
                c.Close();
            }
        }

        // SetMaxOpenConns sets the maximum number of open connections to the database.
        //
        // If MaxIdleConns is greater than 0 and the new MaxOpenConns is less than
        // MaxIdleConns, then MaxIdleConns will be reduced to match the new
        // MaxOpenConns limit
        //
        // If n <= 0, then there is no limit on the number of open connections.
        // The default is 0 (unlimited).
        private static void SetMaxOpenConns(this ref DB db, long n)
        {
            db.mu.Lock();
            db.maxOpen = n;
            if (n < 0L)
            {
                db.maxOpen = 0L;
            }
            var syncMaxIdle = db.maxOpen > 0L && db.maxIdleConnsLocked() > db.maxOpen;
            db.mu.Unlock();
            if (syncMaxIdle)
            {
                db.SetMaxIdleConns(n);
            }
        }

        // SetConnMaxLifetime sets the maximum amount of time a connection may be reused.
        //
        // Expired connections may be closed lazily before reuse.
        //
        // If d <= 0, connections are reused forever.
        private static void SetConnMaxLifetime(this ref DB db, time.Duration d)
        {
            if (d < 0L)
            {
                d = 0L;
            }
            db.mu.Lock(); 
            // wake cleaner up when lifetime is shortened.
            if (d > 0L && d < db.maxLifetime && db.cleanerCh != null)
            {
            }
            db.maxLifetime = d;
            db.startCleanerLocked();
            db.mu.Unlock();
        }

        // startCleanerLocked starts connectionCleaner if needed.
        private static void startCleanerLocked(this ref DB db)
        {
            if (db.maxLifetime > 0L && db.numOpen > 0L && db.cleanerCh == null)
            {
                db.cleanerCh = make_channel<object>(1L);
                go_(() => db.connectionCleaner(db.maxLifetime));
            }
        }

        private static void connectionCleaner(this ref DB db, time.Duration d)
        {
            const var minInterval = time.Second;



            if (d < minInterval)
            {
                d = minInterval;
            }
            var t = time.NewTimer(d);

            while (true)
            {
                db.mu.Lock();
                d = db.maxLifetime;
                if (db.closed || db.numOpen == 0L || d <= 0L)
                {
                    db.cleanerCh = null;
                    db.mu.Unlock();
                    return;
                }
                var expiredSince = nowFunc().Add(-d);
                slice<ref driverConn> closing = default;
                for (long i = 0L; i < len(db.freeConn); i++)
                {
                    var c = db.freeConn[i];
                    if (c.createdAt.Before(expiredSince))
                    {
                        closing = append(closing, c);
                        var last = len(db.freeConn) - 1L;
                        db.freeConn[i] = db.freeConn[last];
                        db.freeConn[last] = null;
                        db.freeConn = db.freeConn[..last];
                        i--;
                    }
                }

                db.mu.Unlock();

                {
                    var c__prev2 = c;

                    foreach (var (_, __c) in closing)
                    {
                        c = __c;
                        c.Close();
                    }

                    c = c__prev2;
                }

                if (d < minInterval)
                {
                    d = minInterval;
                }
                t.Reset(d);
            }

        }

        // DBStats contains database statistics.
        public partial struct DBStats
        {
            public long OpenConnections;
        }

        // Stats returns database statistics.
        private static DBStats Stats(this ref DB db)
        {
            db.mu.Lock();
            DBStats stats = new DBStats(OpenConnections:db.numOpen,);
            db.mu.Unlock();
            return stats;
        }

        // Assumes db.mu is locked.
        // If there are connRequests and the connection limit hasn't been reached,
        // then tell the connectionOpener to open new connections.
        private static void maybeOpenNewConnections(this ref DB db)
        {
            var numRequests = len(db.connRequests);
            if (db.maxOpen > 0L)
            {
                var numCanOpen = db.maxOpen - db.numOpen;
                if (numRequests > numCanOpen)
                {
                    numRequests = numCanOpen;
                }
            }
            while (numRequests > 0L)
            {
                db.numOpen++; // optimistically
                numRequests--;
                if (db.closed)
                {
                    return;
                }
                db.openerCh.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
            }

        }

        // Runs in a separate goroutine, opens new connections when requested.
        private static void connectionOpener(this ref DB db, context.Context ctx)
        {
            while (true)
            {
                return;
                db.openNewConnection(ctx);
            }

        }

        // connectionResetter runs in a separate goroutine to reset connections async
        // to exported API.
        private static void connectionResetter(this ref DB db, context.Context ctx)
        {
            while (true)
            {
                close(db.resetterCh);
                {
                    var dc__prev2 = dc;

                    foreach (var (__dc) in db.resetterCh)
                    {
                        dc = __dc;
                        dc.Unlock();
                    }

                    dc = dc__prev2;
                }

                return;
                dc.resetSession(ctx);
            }

        }

        // Open one new connection
        private static void openNewConnection(this ref DB _db, context.Context ctx) => func(_db, (ref DB db, Defer defer, Panic _, Recover __) =>
        { 
            // maybeOpenNewConnctions has already executed db.numOpen++ before it sent
            // on db.openerCh. This function must execute db.numOpen-- if the
            // connection fails or is closed before returning.
            var (ci, err) = db.connector.Connect(ctx);
            db.mu.Lock();
            defer(db.mu.Unlock());
            if (db.closed)
            {
                if (err == null)
                {
                    ci.Close();
                }
                db.numOpen--;
                return;
            }
            if (err != null)
            {
                db.numOpen--;
                db.putConnDBLocked(null, err);
                db.maybeOpenNewConnections();
                return;
            }
            driverConn dc = ref new driverConn(db:db,createdAt:nowFunc(),ci:ci,);
            if (db.putConnDBLocked(dc, err))
            {
                db.addDepLocked(dc, dc);
            }
            else
            {
                db.numOpen--;
                ci.Close();
            }
        });

        // connRequest represents one request for a new connection
        // When there are no idle connections available, DB.conn will create
        // a new connRequest and put it on the db.connRequests list.
        private partial struct connRequest
        {
            public ptr<driverConn> conn;
            public error err;
        }

        private static var errDBClosed = errors.New("sql: database is closed");

        // nextRequestKeyLocked returns the next connection request key.
        // It is assumed that nextRequest will not overflow.
        private static ulong nextRequestKeyLocked(this ref DB db)
        {
            var next = db.nextRequest;
            db.nextRequest++;
            return next;
        }

        // conn returns a newly-opened or cached *driverConn.
        private static (ref driverConn, error) conn(this ref DB db, context.Context ctx, connReuseStrategy strategy)
        {
            db.mu.Lock();
            if (db.closed)
            {
                db.mu.Unlock();
                return (null, errDBClosed);
            } 
            // Check if the context is expired.
            db.mu.Unlock();
            return (null, ctx.Err());
            var lifetime = db.maxLifetime; 

            // Prefer a free connection, if possible.
            var numFree = len(db.freeConn);
            if (strategy == cachedOrNewConn && numFree > 0L)
            {
                var conn = db.freeConn[0L];
                copy(db.freeConn, db.freeConn[1L..]);
                db.freeConn = db.freeConn[..numFree - 1L];
                conn.inUse = true;
                db.mu.Unlock();
                if (conn.expired(lifetime))
                {
                    conn.Close();
                    return (null, driver.ErrBadConn);
                } 
                // Lock around reading lastErr to ensure the session resetter finished.
                conn.Lock();
                var err = conn.lastErr;
                conn.Unlock();
                if (err == driver.ErrBadConn)
                {
                    conn.Close();
                    return (null, driver.ErrBadConn);
                }
                return (conn, null);
            } 

            // Out of free connections or we were asked not to use one. If we're not
            // allowed to open any more connections, make a request and wait.
            if (db.maxOpen > 0L && db.numOpen >= db.maxOpen)
            { 
                // Make the connRequest channel. It's buffered so that the
                // connectionOpener doesn't block while waiting for the req to be read.
                var req = make_channel<connRequest>(1L);
                var reqKey = db.nextRequestKeyLocked();
                db.connRequests[reqKey] = req;
                db.mu.Unlock(); 

                // Timeout the connection request with the context.
                db.mu.Lock();
                delete(db.connRequests, reqKey);
                db.mu.Unlock();
                if (ok)
                {
                    db.putConn(ret.conn, ret.err, false);
                }
                return (null, ctx.Err());
                if (!ok)
                {
                    return (null, errDBClosed);
                }
                if (ret.err == null && ret.conn.expired(lifetime))
                {
                    ret.conn.Close();
                    return (null, driver.ErrBadConn);
                }
                if (ret.conn == null)
                {
                    return (null, ret.err);
                } 
                // Lock around reading lastErr to ensure the session resetter finished.
                ret.conn.Lock();
                err = ret.conn.lastErr;
                ret.conn.Unlock();
                if (err == driver.ErrBadConn)
                {
                    ret.conn.Close();
                    return (null, driver.ErrBadConn);
                }
                return (ret.conn, ret.err);
            }
            db.numOpen++; // optimistically
            db.mu.Unlock();
            var (ci, err) = db.connector.Connect(ctx);
            if (err != null)
            {
                db.mu.Lock();
                db.numOpen--; // correct for earlier optimism
                db.maybeOpenNewConnections();
                db.mu.Unlock();
                return (null, err);
            }
            db.mu.Lock();
            driverConn dc = ref new driverConn(db:db,createdAt:nowFunc(),ci:ci,inUse:true,);
            db.addDepLocked(dc, dc);
            db.mu.Unlock();
            return (dc, null);
        }

        // putConnHook is a hook for testing.
        private static Action<ref DB, ref driverConn> putConnHook = default;

        // noteUnusedDriverStatement notes that ds is no longer used and should
        // be closed whenever possible (when c is next not in use), unless c is
        // already closed.
        private static void noteUnusedDriverStatement(this ref DB _db, ref driverConn _c, ref driverStmt _ds) => func(_db, _c, _ds, (ref DB db, ref driverConn c, ref driverStmt ds, Defer defer, Panic _, Recover __) =>
        {
            db.mu.Lock();
            defer(db.mu.Unlock());
            if (c.inUse)
            {
                c.onPut = append(c.onPut, () =>
                {
                    ds.Close();
                }
            else
);
            }            {
                c.Lock();
                var fc = c.finalClosed;
                c.Unlock();
                if (!fc)
                {
                    ds.Close();
                }
            }
        });

        // debugGetPut determines whether getConn & putConn calls' stack traces
        // are returned for more verbose crashes.
        private static readonly var debugGetPut = false;

        // putConn adds a connection to the db's free pool.
        // err is optionally the last error that occurred on this connection.


        // putConn adds a connection to the db's free pool.
        // err is optionally the last error that occurred on this connection.
        private static void putConn(this ref DB _db, ref driverConn _dc, error err, bool resetSession) => func(_db, _dc, (ref DB db, ref driverConn dc, Defer _, Panic panic, Recover __) =>
        {
            db.mu.Lock();
            if (!dc.inUse)
            {
                if (debugGetPut)
                {
                    fmt.Printf("putConn(%v) DUPLICATE was: %s\n\nPREVIOUS was: %s", dc, stack(), db.lastPut[dc]);
                }
                panic("sql: connection returned that was never out");
            }
            if (debugGetPut)
            {
                db.lastPut[dc] = stack();
            }
            dc.inUse = false;

            foreach (var (_, fn) in dc.onPut)
            {
                fn();
            }
            dc.onPut = null;

            if (err == driver.ErrBadConn)
            { 
                // Don't reuse bad connections.
                // Since the conn is considered bad and is being discarded, treat it
                // as closed. Don't decrement the open count here, finalClose will
                // take care of that.
                db.maybeOpenNewConnections();
                db.mu.Unlock();
                dc.Close();
                return;
            }
            if (putConnHook != null)
            {
                putConnHook(db, dc);
            }
            if (db.closed)
            { 
                // Connections do not need to be reset if they will be closed.
                // Prevents writing to resetterCh after the DB has closed.
                resetSession = false;
            }
            if (resetSession)
            {
                _, resetSession = dc.ci._<driver.SessionResetter>();

                if (resetSession)
                { 
                    // Lock the driverConn here so it isn't released until
                    // the connection is reset.
                    // The lock must be taken before the connection is put into
                    // the pool to prevent it from being taken out before it is reset.
                    dc.Lock();
                }
            }
            var added = db.putConnDBLocked(dc, null);
            db.mu.Unlock();

            if (!added)
            {
                if (resetSession)
                {
                    dc.Unlock();
                }
                dc.Close();
                return;
            }
            if (!resetSession)
            {
                return;
            }
            dc.lastErr = driver.ErrBadConn;
            dc.Unlock();
        });

        // Satisfy a connRequest or put the driverConn in the idle pool and return true
        // or return false.
        // putConnDBLocked will satisfy a connRequest if there is one, or it will
        // return the *driverConn to the freeConn list if err == nil and the idle
        // connection limit will not be exceeded.
        // If err != nil, the value of dc is ignored.
        // If err == nil, then dc must not equal nil.
        // If a connRequest was fulfilled or the *driverConn was placed in the
        // freeConn list, then true is returned, otherwise false is returned.
        private static bool putConnDBLocked(this ref DB db, ref driverConn dc, error err)
        {
            if (db.closed)
            {
                return false;
            }
            if (db.maxOpen > 0L && db.numOpen > db.maxOpen)
            {
                return false;
            }
            {
                var c = len(db.connRequests);

                if (c > 0L)
                {
                    channel<connRequest> req = default;
                    ulong reqKey = default;
                    foreach (var (__reqKey, __req) in db.connRequests)
                    {
                        reqKey = __reqKey;
                        req = __req;
                        break;
                    }

                    delete(db.connRequests, reqKey); // Remove from pending requests.
                    if (err == null)
                    {
                        dc.inUse = true;
                    }
                    req.Send(new connRequest(conn:dc,err:err,));
                    return true;
                }
                else if (err == null && !db.closed && db.maxIdleConnsLocked() > len(db.freeConn))
                {
                    db.freeConn = append(db.freeConn, dc);
                    db.startCleanerLocked();
                    return true;
                }

            }
            return false;
        }

        // maxBadConnRetries is the number of maximum retries if the driver returns
        // driver.ErrBadConn to signal a broken connection before forcing a new
        // connection to be opened.
        private static readonly long maxBadConnRetries = 2L;

        // PrepareContext creates a prepared statement for later queries or executions.
        // Multiple queries or executions may be run concurrently from the
        // returned statement.
        // The caller must call the statement's Close method
        // when the statement is no longer needed.
        //
        // The provided context is used for the preparation of the statement, not for the
        // execution of the statement.


        // PrepareContext creates a prepared statement for later queries or executions.
        // Multiple queries or executions may be run concurrently from the
        // returned statement.
        // The caller must call the statement's Close method
        // when the statement is no longer needed.
        //
        // The provided context is used for the preparation of the statement, not for the
        // execution of the statement.
        private static (ref Stmt, error) PrepareContext(this ref DB db, context.Context ctx, @string query)
        {
            ref Stmt stmt = default;
            error err = default;
            for (long i = 0L; i < maxBadConnRetries; i++)
            {
                stmt, err = db.prepare(ctx, query, cachedOrNewConn);
                if (err != driver.ErrBadConn)
                {
                    break;
                }
            }

            if (err == driver.ErrBadConn)
            {
                return db.prepare(ctx, query, alwaysNewConn);
            }
            return (stmt, err);
        }

        // Prepare creates a prepared statement for later queries or executions.
        // Multiple queries or executions may be run concurrently from the
        // returned statement.
        // The caller must call the statement's Close method
        // when the statement is no longer needed.
        private static (ref Stmt, error) Prepare(this ref DB db, @string query)
        {
            return db.PrepareContext(context.Background(), query);
        }

        private static (ref Stmt, error) prepare(this ref DB db, context.Context ctx, @string query, connReuseStrategy strategy)
        { 
            // TODO: check if db.driver supports an optional
            // driver.Preparer interface and call that instead, if so,
            // otherwise we make a prepared statement that's bound
            // to a connection, and to execute this prepared statement
            // we either need to use this connection (if it's free), else
            // get a new connection + re-prepare + execute on that one.
            var (dc, err) = db.conn(ctx, strategy);
            if (err != null)
            {
                return (null, err);
            }
            return db.prepareDC(ctx, dc, dc.releaseConn, null, query);
        }

        // prepareDC prepares a query on the driverConn and calls release before
        // returning. When cg == nil it implies that a connection pool is used, and
        // when cg != nil only a single driver connection is used.
        private static (ref Stmt, error) prepareDC(this ref DB _db, context.Context ctx, ref driverConn _dc, Action<error> release, stmtConnGrabber cg, @string query) => func(_db, _dc, (ref DB db, ref driverConn dc, Defer defer, Panic _, Recover __) =>
        {
            ref driverStmt ds = default;
            error err = default;
            defer(() =>
            {
                release(err);
            }());
            withLock(dc, () =>
            {
                ds, err = dc.prepareLocked(ctx, cg, query);
            });
            if (err != null)
            {
                return (null, err);
            }
            Stmt stmt = ref new Stmt(db:db,query:query,cg:cg,cgds:ds,); 

            // When cg == nil this statement will need to keep track of various
            // connections they are prepared on and record the stmt dependency on
            // the DB.
            if (cg == null)
            {
                stmt.css = new slice<connStmt>(new connStmt[] { {dc,ds} });
                stmt.lastNumClosed = atomic.LoadUint64(ref db.numClosed);
                db.addDep(stmt, stmt);
            }
            return (stmt, null);
        });

        // ExecContext executes a query without returning any rows.
        // The args are for any placeholder parameters in the query.
        private static (Result, error) ExecContext(this ref DB db, context.Context ctx, @string query, params object[] args)
        {
            Result res = default;
            error err = default;
            for (long i = 0L; i < maxBadConnRetries; i++)
            {
                res, err = db.exec(ctx, query, args, cachedOrNewConn);
                if (err != driver.ErrBadConn)
                {
                    break;
                }
            }

            if (err == driver.ErrBadConn)
            {
                return db.exec(ctx, query, args, alwaysNewConn);
            }
            return (res, err);
        }

        // Exec executes a query without returning any rows.
        // The args are for any placeholder parameters in the query.
        private static (Result, error) Exec(this ref DB db, @string query, params object[] args)
        {
            return db.ExecContext(context.Background(), query, args);
        }

        private static (Result, error) exec(this ref DB db, context.Context ctx, @string query, slice<object> args, connReuseStrategy strategy)
        {
            var (dc, err) = db.conn(ctx, strategy);
            if (err != null)
            {
                return (null, err);
            }
            return db.execDC(ctx, dc, dc.releaseConn, query, args);
        }

        private static (Result, error) execDC(this ref DB _db, context.Context ctx, ref driverConn _dc, Action<error> release, @string query, slice<object> args) => func(_db, _dc, (ref DB db, ref driverConn dc, Defer defer, Panic _, Recover __) =>
        {
            defer(() =>
            {
                release(err);
            }());
            driver.ExecerContext (execerCtx, ok) = dc.ci._<driver.ExecerContext>();
            driver.Execer execer = default;
            if (!ok)
            {
                execer, ok = dc.ci._<driver.Execer>();
            }
            if (ok)
            {
                slice<driver.NamedValue> nvdargs = default;
                driver.Result resi = default;
                withLock(dc, () =>
                {
                    nvdargs, err = driverArgsConnLocked(dc.ci, null, args);
                    if (err != null)
                    {
                        return;
                    }
                    resi, err = ctxDriverExec(ctx, execerCtx, execer, query, nvdargs);
                });
                if (err != driver.ErrSkip)
                {
                    if (err != null)
                    {
                        return (null, err);
                    }
                    return (new driverResult(dc,resi), null);
                }
            }
            driver.Stmt si = default;
            withLock(dc, () =>
            {
                si, err = ctxDriverPrepare(ctx, dc.ci, query);
            });
            if (err != null)
            {
                return (null, err);
            }
            driverStmt ds = ref new driverStmt(Locker:dc,si:si);
            defer(ds.Close());
            return resultFromStatement(ctx, dc.ci, ds, args);
        });

        // QueryContext executes a query that returns rows, typically a SELECT.
        // The args are for any placeholder parameters in the query.
        private static (ref Rows, error) QueryContext(this ref DB db, context.Context ctx, @string query, params object[] args)
        {
            ref Rows rows = default;
            error err = default;
            for (long i = 0L; i < maxBadConnRetries; i++)
            {
                rows, err = db.query(ctx, query, args, cachedOrNewConn);
                if (err != driver.ErrBadConn)
                {
                    break;
                }
            }

            if (err == driver.ErrBadConn)
            {
                return db.query(ctx, query, args, alwaysNewConn);
            }
            return (rows, err);
        }

        // Query executes a query that returns rows, typically a SELECT.
        // The args are for any placeholder parameters in the query.
        private static (ref Rows, error) Query(this ref DB db, @string query, params object[] args)
        {
            return db.QueryContext(context.Background(), query, args);
        }

        private static (ref Rows, error) query(this ref DB db, context.Context ctx, @string query, slice<object> args, connReuseStrategy strategy)
        {
            var (dc, err) = db.conn(ctx, strategy);
            if (err != null)
            {
                return (null, err);
            }
            return db.queryDC(ctx, null, dc, dc.releaseConn, query, args);
        }

        // queryDC executes a query on the given connection.
        // The connection gets released by the releaseConn function.
        // The ctx context is from a query method and the txctx context is from an
        // optional transaction context.
        private static (ref Rows, error) queryDC(this ref DB db, context.Context ctx, context.Context txctx, ref driverConn dc, Action<error> releaseConn, @string query, slice<object> args)
        {
            driver.QueryerContext (queryerCtx, ok) = dc.ci._<driver.QueryerContext>();
            driver.Queryer queryer = default;
            if (!ok)
            {
                queryer, ok = dc.ci._<driver.Queryer>();
            }
            if (ok)
            {
                slice<driver.NamedValue> nvdargs = default;
                driver.Rows rowsi = default;
                error err = default;
                withLock(dc, () =>
                {
                    nvdargs, err = driverArgsConnLocked(dc.ci, null, args);
                    if (err != null)
                    {
                        return;
                    }
                    rowsi, err = ctxDriverQuery(ctx, queryerCtx, queryer, query, nvdargs);
                });
                if (err != driver.ErrSkip)
                {
                    if (err != null)
                    {
                        releaseConn(err);
                        return (null, err);
                    } 
                    // Note: ownership of dc passes to the *Rows, to be freed
                    // with releaseConn.
                    Rows rows = ref new Rows(dc:dc,releaseConn:releaseConn,rowsi:rowsi,);
                    rows.initContextClose(ctx, txctx);
                    return (rows, null);
                }
            }
            driver.Stmt si = default;
            err = default;
            withLock(dc, () =>
            {
                si, err = ctxDriverPrepare(ctx, dc.ci, query);
            });
            if (err != null)
            {
                releaseConn(err);
                return (null, err);
            }
            driverStmt ds = ref new driverStmt(Locker:dc,si:si);
            var (rowsi, err) = rowsiFromStatement(ctx, dc.ci, ds, args);
            if (err != null)
            {
                ds.Close();
                releaseConn(err);
                return (null, err);
            } 

            // Note: ownership of ci passes to the *Rows, to be freed
            // with releaseConn.
            rows = ref new Rows(dc:dc,releaseConn:releaseConn,rowsi:rowsi,closeStmt:ds,);
            rows.initContextClose(ctx, txctx);
            return (rows, null);
        }

        // QueryRowContext executes a query that is expected to return at most one row.
        // QueryRowContext always returns a non-nil value. Errors are deferred until
        // Row's Scan method is called.
        // If the query selects no rows, the *Row's Scan will return ErrNoRows.
        // Otherwise, the *Row's Scan scans the first selected row and discards
        // the rest.
        private static ref Row QueryRowContext(this ref DB db, context.Context ctx, @string query, params object[] args)
        {
            var (rows, err) = db.QueryContext(ctx, query, args);
            return ref new Row(rows:rows,err:err);
        }

        // QueryRow executes a query that is expected to return at most one row.
        // QueryRow always returns a non-nil value. Errors are deferred until
        // Row's Scan method is called.
        // If the query selects no rows, the *Row's Scan will return ErrNoRows.
        // Otherwise, the *Row's Scan scans the first selected row and discards
        // the rest.
        private static ref Row QueryRow(this ref DB db, @string query, params object[] args)
        {
            return db.QueryRowContext(context.Background(), query, args);
        }

        // BeginTx starts a transaction.
        //
        // The provided context is used until the transaction is committed or rolled back.
        // If the context is canceled, the sql package will roll back
        // the transaction. Tx.Commit will return an error if the context provided to
        // BeginTx is canceled.
        //
        // The provided TxOptions is optional and may be nil if defaults should be used.
        // If a non-default isolation level is used that the driver doesn't support,
        // an error will be returned.
        private static (ref Tx, error) BeginTx(this ref DB db, context.Context ctx, ref TxOptions opts)
        {
            ref Tx tx = default;
            error err = default;
            for (long i = 0L; i < maxBadConnRetries; i++)
            {
                tx, err = db.begin(ctx, opts, cachedOrNewConn);
                if (err != driver.ErrBadConn)
                {
                    break;
                }
            }

            if (err == driver.ErrBadConn)
            {
                return db.begin(ctx, opts, alwaysNewConn);
            }
            return (tx, err);
        }

        // Begin starts a transaction. The default isolation level is dependent on
        // the driver.
        private static (ref Tx, error) Begin(this ref DB db)
        {
            return db.BeginTx(context.Background(), null);
        }

        private static (ref Tx, error) begin(this ref DB db, context.Context ctx, ref TxOptions opts, connReuseStrategy strategy)
        {
            var (dc, err) = db.conn(ctx, strategy);
            if (err != null)
            {
                return (null, err);
            }
            return db.beginDC(ctx, dc, dc.releaseConn, opts);
        }

        // beginDC starts a transaction. The provided dc must be valid and ready to use.
        private static (ref Tx, error) beginDC(this ref DB db, context.Context ctx, ref driverConn dc, Action<error> release, ref TxOptions opts)
        {
            driver.Tx txi = default;
            withLock(dc, () =>
            {
                txi, err = ctxDriverBegin(ctx, opts, dc.ci);
            });
            if (err != null)
            {
                release(err);
                return (null, err);
            } 

            // Schedule the transaction to rollback when the context is cancelled.
            // The cancel function in Tx will be called after done is set to true.
            var (ctx, cancel) = context.WithCancel(ctx);
            tx = ref new Tx(db:db,dc:dc,releaseConn:release,txi:txi,cancel:cancel,ctx:ctx,);
            go_(() => tx.awaitDone());
            return (tx, null);
        }

        // Driver returns the database's underlying driver.
        private static driver.Driver Driver(this ref DB db)
        {
            return db.connector.Driver();
        }

        // ErrConnDone is returned by any operation that is performed on a connection
        // that has already been returned to the connection pool.
        public static var ErrConnDone = errors.New("database/sql: connection is already closed");

        // Conn returns a single connection by either opening a new connection
        // or returning an existing connection from the connection pool. Conn will
        // block until either a connection is returned or ctx is canceled.
        // Queries run on the same Conn will be run in the same database session.
        //
        // Every Conn must be returned to the database pool after use by
        // calling Conn.Close.
        private static (ref Conn, error) Conn(this ref DB db, context.Context ctx)
        {
            ref driverConn dc = default;
            error err = default;
            for (long i = 0L; i < maxBadConnRetries; i++)
            {
                dc, err = db.conn(ctx, cachedOrNewConn);
                if (err != driver.ErrBadConn)
                {
                    break;
                }
            }

            if (err == driver.ErrBadConn)
            {
                dc, err = db.conn(ctx, cachedOrNewConn);
            }
            if (err != null)
            {
                return (null, err);
            }
            Conn conn = ref new Conn(db:db,dc:dc,);
            return (conn, null);
        }

        public delegate void releaseConn(error);

        // Conn represents a single database connection rather than a pool of database
        // connections. Prefer running queries from DB unless there is a specific
        // need for a continuous single database connection.
        //
        // A Conn must call Close to return the connection to the database pool
        // and may do so concurrently with a running query.
        //
        // After a call to Close, all operations on the
        // connection fail with ErrConnDone.
        public partial struct Conn
        {
            public ptr<DB> db; // closemu prevents the connection from closing while there
// is an active query. It is held for read during queries
// and exclusively during close.
            public sync.RWMutex closemu; // dc is owned until close, at which point
// it's returned to the connection pool.
            public ptr<driverConn> dc; // done transitions from 0 to 1 exactly once, on close.
// Once done, all operations fail with ErrConnDone.
// Use atomic operations on value when checking value.
            public int done;
        }

        private static (ref driverConn, releaseConn, error) grabConn(this ref Conn c, context.Context _p0)
        {
            if (atomic.LoadInt32(ref c.done) != 0L)
            {
                return (null, null, ErrConnDone);
            }
            c.closemu.RLock();
            return (c.dc, c.closemuRUnlockCondReleaseConn, null);
        }

        // PingContext verifies the connection to the database is still alive.
        private static error PingContext(this ref Conn c, context.Context ctx)
        {
            var (dc, release, err) = c.grabConn(ctx);
            if (err != null)
            {
                return error.As(err);
            }
            return error.As(c.db.pingDC(ctx, dc, release));
        }

        // ExecContext executes a query without returning any rows.
        // The args are for any placeholder parameters in the query.
        private static (Result, error) ExecContext(this ref Conn c, context.Context ctx, @string query, params object[] args)
        {
            var (dc, release, err) = c.grabConn(ctx);
            if (err != null)
            {
                return (null, err);
            }
            return c.db.execDC(ctx, dc, release, query, args);
        }

        // QueryContext executes a query that returns rows, typically a SELECT.
        // The args are for any placeholder parameters in the query.
        private static (ref Rows, error) QueryContext(this ref Conn c, context.Context ctx, @string query, params object[] args)
        {
            var (dc, release, err) = c.grabConn(ctx);
            if (err != null)
            {
                return (null, err);
            }
            return c.db.queryDC(ctx, null, dc, release, query, args);
        }

        // QueryRowContext executes a query that is expected to return at most one row.
        // QueryRowContext always returns a non-nil value. Errors are deferred until
        // Row's Scan method is called.
        // If the query selects no rows, the *Row's Scan will return ErrNoRows.
        // Otherwise, the *Row's Scan scans the first selected row and discards
        // the rest.
        private static ref Row QueryRowContext(this ref Conn c, context.Context ctx, @string query, params object[] args)
        {
            var (rows, err) = c.QueryContext(ctx, query, args);
            return ref new Row(rows:rows,err:err);
        }

        // PrepareContext creates a prepared statement for later queries or executions.
        // Multiple queries or executions may be run concurrently from the
        // returned statement.
        // The caller must call the statement's Close method
        // when the statement is no longer needed.
        //
        // The provided context is used for the preparation of the statement, not for the
        // execution of the statement.
        private static (ref Stmt, error) PrepareContext(this ref Conn c, context.Context ctx, @string query)
        {
            var (dc, release, err) = c.grabConn(ctx);
            if (err != null)
            {
                return (null, err);
            }
            return c.db.prepareDC(ctx, dc, release, c, query);
        }

        // BeginTx starts a transaction.
        //
        // The provided context is used until the transaction is committed or rolled back.
        // If the context is canceled, the sql package will roll back
        // the transaction. Tx.Commit will return an error if the context provided to
        // BeginTx is canceled.
        //
        // The provided TxOptions is optional and may be nil if defaults should be used.
        // If a non-default isolation level is used that the driver doesn't support,
        // an error will be returned.
        private static (ref Tx, error) BeginTx(this ref Conn c, context.Context ctx, ref TxOptions opts)
        {
            var (dc, release, err) = c.grabConn(ctx);
            if (err != null)
            {
                return (null, err);
            }
            return c.db.beginDC(ctx, dc, release, opts);
        }

        // closemuRUnlockCondReleaseConn read unlocks closemu
        // as the sql operation is done with the dc.
        private static void closemuRUnlockCondReleaseConn(this ref Conn c, error err)
        {
            c.closemu.RUnlock();
            if (err == driver.ErrBadConn)
            {
                c.close(err);
            }
        }

        private static context.Context txCtx(this ref Conn c)
        {
            return null;
        }

        private static error close(this ref Conn _c, error err) => func(_c, (ref Conn c, Defer defer, Panic _, Recover __) =>
        {
            if (!atomic.CompareAndSwapInt32(ref c.done, 0L, 1L))
            {
                return error.As(ErrConnDone);
            } 

            // Lock around releasing the driver connection
            // to ensure all queries have been stopped before doing so.
            c.closemu.Lock();
            defer(c.closemu.Unlock());

            c.dc.releaseConn(err);
            c.dc = null;
            c.db = null;
            return error.As(err);
        });

        // Close returns the connection to the connection pool.
        // All operations after a Close will return with ErrConnDone.
        // Close is safe to call concurrently with other operations and will
        // block until all other operations finish. It may be useful to first
        // cancel any used context and then call close directly after.
        private static error Close(this ref Conn c)
        {
            return error.As(c.close(null));
        }

        // Tx is an in-progress database transaction.
        //
        // A transaction must end with a call to Commit or Rollback.
        //
        // After a call to Commit or Rollback, all operations on the
        // transaction fail with ErrTxDone.
        //
        // The statements prepared for a transaction by calling
        // the transaction's Prepare or Stmt methods are closed
        // by the call to Commit or Rollback.
        public partial struct Tx
        {
            public ptr<DB> db; // closemu prevents the transaction from closing while there
// is an active query. It is held for read during queries
// and exclusively during close.
            public sync.RWMutex closemu; // dc is owned exclusively until Commit or Rollback, at which point
// it's returned with putConn.
            public ptr<driverConn> dc;
            public driver.Tx txi; // releaseConn is called once the Tx is closed to release
// any held driverConn back to the pool.
            public Action<error> releaseConn; // done transitions from 0 to 1 exactly once, on Commit
// or Rollback. once done, all operations fail with
// ErrTxDone.
// Use atomic operations on value when checking value.
            public int done; // All Stmts prepared for this transaction. These will be closed after the
// transaction has been committed or rolled back.
            public Action cancel; // ctx lives for the life of the transaction.
            public context.Context ctx;
        }

        // awaitDone blocks until the context in Tx is canceled and rolls back
        // the transaction if it's not already done.
        private static void awaitDone(this ref Tx tx)
        { 
            // Wait for either the transaction to be committed or rolled
            // back, or for the associated context to be closed.
            tx.ctx.Done().Receive(); 

            // Discard and close the connection used to ensure the
            // transaction is closed and the resources are released.  This
            // rollback does nothing if the transaction has already been
            // committed or rolled back.
            tx.rollback(true);
        }

        private static bool isDone(this ref Tx tx)
        {
            return atomic.LoadInt32(ref tx.done) != 0L;
        }

        // ErrTxDone is returned by any operation that is performed on a transaction
        // that has already been committed or rolled back.
        public static var ErrTxDone = errors.New("sql: Transaction has already been committed or rolled back");

        // close returns the connection to the pool and
        // must only be called by Tx.rollback or Tx.Commit.
        private static void close(this ref Tx _tx, error err) => func(_tx, (ref Tx tx, Defer defer, Panic _, Recover __) =>
        {
            tx.cancel();

            tx.closemu.Lock();
            defer(tx.closemu.Unlock());

            tx.releaseConn(err);
            tx.dc = null;
            tx.txi = null;
        });

        // hookTxGrabConn specifies an optional hook to be called on
        // a successful call to (*Tx).grabConn. For tests.
        private static Action hookTxGrabConn = default;

        private static (ref driverConn, releaseConn, error) grabConn(this ref Tx tx, context.Context ctx)
        {
            return (null, null, ctx.Err());
            tx.closemu.RLock();
            if (tx.isDone())
            {
                tx.closemu.RUnlock();
                return (null, null, ErrTxDone);
            }
            if (hookTxGrabConn != null)
            { // test hook
                hookTxGrabConn();
            }
            return (tx.dc, tx.closemuRUnlockRelease, null);
        }

        private static context.Context txCtx(this ref Tx tx)
        {
            return tx.ctx;
        }

        // closemuRUnlockRelease is used as a func(error) method value in
        // ExecContext and QueryContext. Unlocking in the releaseConn keeps
        // the driver conn from being returned to the connection pool until
        // the Rows has been closed.
        private static void closemuRUnlockRelease(this ref Tx tx, error _p0)
        {
            tx.closemu.RUnlock();
        }

        // Closes all Stmts prepared for this transaction.
        private static void closePrepared(this ref Tx _tx) => func(_tx, (ref Tx tx, Defer defer, Panic _, Recover __) =>
        {
            tx.stmts.Lock();
            defer(tx.stmts.Unlock());
            foreach (var (_, stmt) in tx.stmts.v)
            {
                stmt.Close();
            }
        });

        // Commit commits the transaction.
        private static error Commit(this ref Tx tx)
        { 
            // Check context first to avoid transaction leak.
            // If put it behind tx.done CompareAndSwap statement, we cant't ensure
            // the consistency between tx.done and the real COMMIT operation.
            if (atomic.LoadInt32(ref tx.done) == 1L)
            {
                return error.As(ErrTxDone);
            }
            return error.As(tx.ctx.Err());
            if (!atomic.CompareAndSwapInt32(ref tx.done, 0L, 1L))
            {
                return error.As(ErrTxDone);
            }
            error err = default;
            withLock(tx.dc, () =>
            {
                err = error.As(tx.txi.Commit());
            });
            if (err != driver.ErrBadConn)
            {
                tx.closePrepared();
            }
            tx.close(err);
            return error.As(err);
        }

        // rollback aborts the transaction and optionally forces the pool to discard
        // the connection.
        private static error rollback(this ref Tx tx, bool discardConn)
        {
            if (!atomic.CompareAndSwapInt32(ref tx.done, 0L, 1L))
            {
                return error.As(ErrTxDone);
            }
            error err = default;
            withLock(tx.dc, () =>
            {
                err = error.As(tx.txi.Rollback());
            });
            if (err != driver.ErrBadConn)
            {
                tx.closePrepared();
            }
            if (discardConn)
            {
                err = error.As(driver.ErrBadConn);
            }
            tx.close(err);
            return error.As(err);
        }

        // Rollback aborts the transaction.
        private static error Rollback(this ref Tx tx)
        {
            return error.As(tx.rollback(false));
        }

        // PrepareContext creates a prepared statement for use within a transaction.
        //
        // The returned statement operates within the transaction and will be closed
        // when the transaction has been committed or rolled back.
        //
        // To use an existing prepared statement on this transaction, see Tx.Stmt.
        //
        // The provided context will be used for the preparation of the context, not
        // for the execution of the returned statement. The returned statement
        // will run in the transaction context.
        private static (ref Stmt, error) PrepareContext(this ref Tx tx, context.Context ctx, @string query)
        {
            var (dc, release, err) = tx.grabConn(ctx);
            if (err != null)
            {
                return (null, err);
            }
            var (stmt, err) = tx.db.prepareDC(ctx, dc, release, tx, query);
            if (err != null)
            {
                return (null, err);
            }
            tx.stmts.Lock();
            tx.stmts.v = append(tx.stmts.v, stmt);
            tx.stmts.Unlock();
            return (stmt, null);
        }

        // Prepare creates a prepared statement for use within a transaction.
        //
        // The returned statement operates within the transaction and can no longer
        // be used once the transaction has been committed or rolled back.
        //
        // To use an existing prepared statement on this transaction, see Tx.Stmt.
        private static (ref Stmt, error) Prepare(this ref Tx tx, @string query)
        {
            return tx.PrepareContext(context.Background(), query);
        }

        // StmtContext returns a transaction-specific prepared statement from
        // an existing statement.
        //
        // Example:
        //  updateMoney, err := db.Prepare("UPDATE balance SET money=money+? WHERE id=?")
        //  ...
        //  tx, err := db.Begin()
        //  ...
        //  res, err := tx.StmtContext(ctx, updateMoney).Exec(123.45, 98293203)
        //
        // The provided context is used for the preparation of the statement, not for the
        // execution of the statement.
        //
        // The returned statement operates within the transaction and will be closed
        // when the transaction has been committed or rolled back.
        private static ref Stmt StmtContext(this ref Tx _tx, context.Context ctx, ref Stmt _stmt) => func(_tx, _stmt, (ref Tx tx, ref Stmt stmt, Defer defer, Panic _, Recover __) =>
        {
            var (dc, release, err) = tx.grabConn(ctx);
            if (err != null)
            {
                return ref new Stmt(stickyErr:err);
            }
            defer(release(null));

            if (tx.db != stmt.db)
            {
                return ref new Stmt(stickyErr:errors.New("sql: Tx.Stmt: statement from different database used"));
            }
            driver.Stmt si = default;
            ref Stmt parentStmt = default;
            stmt.mu.Lock();
            if (stmt.closed || stmt.cg != null)
            { 
                // If the statement has been closed or already belongs to a
                // transaction, we can't reuse it in this connection.
                // Since tx.StmtContext should never need to be called with a
                // Stmt already belonging to tx, we ignore this edge case and
                // re-prepare the statement in this case. No need to add
                // code-complexity for this.
                stmt.mu.Unlock();
                withLock(dc, () =>
                {
                    si, err = ctxDriverPrepare(ctx, dc.ci, stmt.query);
                }
            else
);
                if (err != null)
                {
                    return ref new Stmt(stickyErr:err);
                }
            }            {
                stmt.removeClosedStmtLocked(); 
                // See if the statement has already been prepared on this connection,
                // and reuse it if possible.
                foreach (var (_, v) in stmt.css)
                {
                    if (v.dc == dc)
                    {
                        si = v.ds.si;
                        break;
                    }
                }
                stmt.mu.Unlock();

                if (si == null)
                {
                    ref driverStmt ds = default;
                    withLock(dc, () =>
                    {
                        ds, err = stmt.prepareOnConnLocked(ctx, dc);
                    });
                    if (err != null)
                    {
                        return ref new Stmt(stickyErr:err);
                    }
                    si = ds.si;
                }
                parentStmt = stmt;
            }
            Stmt txs = ref new Stmt(db:tx.db,cg:tx,cgds:&driverStmt{Locker:dc,si:si,},parentStmt:parentStmt,query:stmt.query,);
            if (parentStmt != null)
            {
                tx.db.addDep(parentStmt, txs);
            }
            tx.stmts.Lock();
            tx.stmts.v = append(tx.stmts.v, txs);
            tx.stmts.Unlock();
            return txs;
        });

        // Stmt returns a transaction-specific prepared statement from
        // an existing statement.
        //
        // Example:
        //  updateMoney, err := db.Prepare("UPDATE balance SET money=money+? WHERE id=?")
        //  ...
        //  tx, err := db.Begin()
        //  ...
        //  res, err := tx.Stmt(updateMoney).Exec(123.45, 98293203)
        //
        // The returned statement operates within the transaction and will be closed
        // when the transaction has been committed or rolled back.
        private static ref Stmt Stmt(this ref Tx tx, ref Stmt stmt)
        {
            return tx.StmtContext(context.Background(), stmt);
        }

        // ExecContext executes a query that doesn't return rows.
        // For example: an INSERT and UPDATE.
        private static (Result, error) ExecContext(this ref Tx tx, context.Context ctx, @string query, params object[] args)
        {
            var (dc, release, err) = tx.grabConn(ctx);
            if (err != null)
            {
                return (null, err);
            }
            return tx.db.execDC(ctx, dc, release, query, args);
        }

        // Exec executes a query that doesn't return rows.
        // For example: an INSERT and UPDATE.
        private static (Result, error) Exec(this ref Tx tx, @string query, params object[] args)
        {
            return tx.ExecContext(context.Background(), query, args);
        }

        // QueryContext executes a query that returns rows, typically a SELECT.
        private static (ref Rows, error) QueryContext(this ref Tx tx, context.Context ctx, @string query, params object[] args)
        {
            var (dc, release, err) = tx.grabConn(ctx);
            if (err != null)
            {
                return (null, err);
            }
            return tx.db.queryDC(ctx, tx.ctx, dc, release, query, args);
        }

        // Query executes a query that returns rows, typically a SELECT.
        private static (ref Rows, error) Query(this ref Tx tx, @string query, params object[] args)
        {
            return tx.QueryContext(context.Background(), query, args);
        }

        // QueryRowContext executes a query that is expected to return at most one row.
        // QueryRowContext always returns a non-nil value. Errors are deferred until
        // Row's Scan method is called.
        // If the query selects no rows, the *Row's Scan will return ErrNoRows.
        // Otherwise, the *Row's Scan scans the first selected row and discards
        // the rest.
        private static ref Row QueryRowContext(this ref Tx tx, context.Context ctx, @string query, params object[] args)
        {
            var (rows, err) = tx.QueryContext(ctx, query, args);
            return ref new Row(rows:rows,err:err);
        }

        // QueryRow executes a query that is expected to return at most one row.
        // QueryRow always returns a non-nil value. Errors are deferred until
        // Row's Scan method is called.
        // If the query selects no rows, the *Row's Scan will return ErrNoRows.
        // Otherwise, the *Row's Scan scans the first selected row and discards
        // the rest.
        private static ref Row QueryRow(this ref Tx tx, @string query, params object[] args)
        {
            return tx.QueryRowContext(context.Background(), query, args);
        }

        // connStmt is a prepared statement on a particular connection.
        private partial struct connStmt
        {
            public ptr<driverConn> dc;
            public ptr<driverStmt> ds;
        }

        // stmtConnGrabber represents a Tx or Conn that will return the underlying
        // driverConn and release function.
        private partial interface stmtConnGrabber
        {
            context.Context grabConn(context.Context _p0); // txCtx returns the transaction context if available.
// The returned context should be selected on along with
// any query context when awaiting a cancel.
            context.Context txCtx();
        }

        private static stmtConnGrabber _ = stmtConnGrabber.As(ref new Tx());        private static stmtConnGrabber _ = stmtConnGrabber.As(ref new Conn());

        // Stmt is a prepared statement.
        // A Stmt is safe for concurrent use by multiple goroutines.
        public partial struct Stmt
        {
            public ptr<DB> db; // where we came from
            public @string query; // that created the Stmt
            public error stickyErr; // if non-nil, this error is returned for all operations

            public sync.RWMutex closemu; // held exclusively during close, for read otherwise.

// If Stmt is prepared on a Tx or Conn then cg is present and will
// only ever grab a connection from cg.
// If cg is nil then the Stmt must grab an arbitrary connection
// from db and determine if it must prepare the stmt again by
// inspecting css.
            public stmtConnGrabber cg;
            public ptr<driverStmt> cgds; // parentStmt is set when a transaction-specific statement
// is requested from an identical statement prepared on the same
// conn. parentStmt is used to track the dependency of this statement
// on its originating ("parent") statement so that parentStmt may
// be closed by the user without them having to know whether or not
// any transactions are still using it.
            public ptr<Stmt> parentStmt;
            public sync.Mutex mu; // protects the rest of the fields
            public bool closed; // css is a list of underlying driver statement interfaces
// that are valid on particular connections. This is only
// used if cg == nil and one is found that has idle
// connections. If cg != nil, cgds is always used.
            public slice<connStmt> css; // lastNumClosed is copied from db.numClosed when Stmt is created
// without tx and closed connections in css are removed.
            public ulong lastNumClosed;
        }

        // ExecContext executes a prepared statement with the given arguments and
        // returns a Result summarizing the effect of the statement.
        private static (Result, error) ExecContext(this ref Stmt _s, context.Context ctx, params object[] args) => func(_s, (ref Stmt s, Defer defer, Panic _, Recover __) =>
        {
            s.closemu.RLock();
            defer(s.closemu.RUnlock());

            Result res = default;
            var strategy = cachedOrNewConn;
            for (long i = 0L; i < maxBadConnRetries + 1L; i++)
            {
                if (i == maxBadConnRetries)
                {
                    strategy = alwaysNewConn;
                }
                var (dc, releaseConn, ds, err) = s.connStmt(ctx, strategy);
                if (err != null)
                {
                    if (err == driver.ErrBadConn)
                    {
                        continue;
                    }
                    return (null, err);
                }
                res, err = resultFromStatement(ctx, dc.ci, ds, args);
                releaseConn(err);
                if (err != driver.ErrBadConn)
                {
                    return (res, err);
                }
            }

            return (null, driver.ErrBadConn);
        });

        // Exec executes a prepared statement with the given arguments and
        // returns a Result summarizing the effect of the statement.
        private static (Result, error) Exec(this ref Stmt s, params object[] args)
        {
            return s.ExecContext(context.Background(), args);
        }

        private static (Result, error) resultFromStatement(context.Context ctx, driver.Conn ci, ref driverStmt _ds, params object[] args) => func(_ds, (ref driverStmt ds, Defer defer, Panic _, Recover __) =>
        {
            args = args.Clone();

            ds.Lock();
            defer(ds.Unlock());

            var (dargs, err) = driverArgsConnLocked(ci, ds, args);
            if (err != null)
            {
                return (null, err);
            } 

            // -1 means the driver doesn't know how to count the number of
            // placeholders, so we won't sanity check input here and instead let the
            // driver deal with errors.
            {
                var want = ds.si.NumInput();

                if (want >= 0L && want != len(dargs))
                {
                    return (null, fmt.Errorf("sql: statement expects %d inputs; got %d", want, len(dargs)));
                }

            }

            var (resi, err) = ctxDriverStmtExec(ctx, ds.si, dargs);
            if (err != null)
            {
                return (null, err);
            }
            return (new driverResult(ds.Locker,resi), null);
        });

        // removeClosedStmtLocked removes closed conns in s.css.
        //
        // To avoid lock contention on DB.mu, we do it only when
        // s.db.numClosed - s.lastNum is large enough.
        private static void removeClosedStmtLocked(this ref Stmt s)
        {
            var t = len(s.css) / 2L + 1L;
            if (t > 10L)
            {
                t = 10L;
            }
            var dbClosed = atomic.LoadUint64(ref s.db.numClosed);
            if (dbClosed - s.lastNumClosed < uint64(t))
            {
                return;
            }
            s.db.mu.Lock();
            for (long i = 0L; i < len(s.css); i++)
            {
                if (s.css[i].dc.dbmuClosed)
                {
                    s.css[i] = s.css[len(s.css) - 1L];
                    s.css = s.css[..len(s.css) - 1L];
                    i--;
                }
            }

            s.db.mu.Unlock();
            s.lastNumClosed = dbClosed;
        }

        // connStmt returns a free driver connection on which to execute the
        // statement, a function to call to release the connection, and a
        // statement bound to that connection.
        private static (ref driverConn, Action<error>, ref driverStmt, error) connStmt(this ref Stmt s, context.Context ctx, connReuseStrategy strategy)
        {
            err = s.stickyErr;

            if (err != null)
            {
                return;
            }
            s.mu.Lock();
            if (s.closed)
            {
                s.mu.Unlock();
                err = errors.New("sql: statement is closed");
                return;
            } 

            // In a transaction or connection, we always use the connection that the
            // the stmt was created on.
            if (s.cg != null)
            {
                s.mu.Unlock();
                dc, releaseConn, err = s.cg.grabConn(ctx); // blocks, waiting for the connection.
                if (err != null)
                {
                    return;
                }
                return (dc, releaseConn, s.cgds, null);
            }
            s.removeClosedStmtLocked();
            s.mu.Unlock();

            dc, err = s.db.conn(ctx, strategy);
            if (err != null)
            {
                return (null, null, null, err);
            }
            s.mu.Lock();
            foreach (var (_, v) in s.css)
            {
                if (v.dc == dc)
                {
                    s.mu.Unlock();
                    return (dc, dc.releaseConn, v.ds, null);
                }
            }
            s.mu.Unlock(); 

            // No luck; we need to prepare the statement on this connection
            withLock(dc, () =>
            {
                ds, err = s.prepareOnConnLocked(ctx, dc);
            });
            if (err != null)
            {
                dc.releaseConn(err);
                return (null, null, null, err);
            }
            return (dc, dc.releaseConn, ds, null);
        }

        // prepareOnConnLocked prepares the query in Stmt s on dc and adds it to the list of
        // open connStmt on the statement. It assumes the caller is holding the lock on dc.
        private static (ref driverStmt, error) prepareOnConnLocked(this ref Stmt s, context.Context ctx, ref driverConn dc)
        {
            var (si, err) = dc.prepareLocked(ctx, s.cg, s.query);
            if (err != null)
            {
                return (null, err);
            }
            connStmt cs = new connStmt(dc,si);
            s.mu.Lock();
            s.css = append(s.css, cs);
            s.mu.Unlock();
            return (cs.ds, null);
        }

        // QueryContext executes a prepared query statement with the given arguments
        // and returns the query results as a *Rows.
        private static (ref Rows, error) QueryContext(this ref Stmt _s, context.Context ctx, params object[] args) => func(_s, (ref Stmt s, Defer defer, Panic _, Recover __) =>
        {
            s.closemu.RLock();
            defer(s.closemu.RUnlock());

            driver.Rows rowsi = default;
            var strategy = cachedOrNewConn;
            for (long i = 0L; i < maxBadConnRetries + 1L; i++)
            {
                if (i == maxBadConnRetries)
                {
                    strategy = alwaysNewConn;
                }
                var (dc, releaseConn, ds, err) = s.connStmt(ctx, strategy);
                if (err != null)
                {
                    if (err == driver.ErrBadConn)
                    {
                        continue;
                    }
                    return (null, err);
                }
                rowsi, err = rowsiFromStatement(ctx, dc.ci, ds, args);
                if (err == null)
                { 
                    // Note: ownership of ci passes to the *Rows, to be freed
                    // with releaseConn.
                    Rows rows = ref new Rows(dc:dc,rowsi:rowsi,); 
                    // addDep must be added before initContextClose or it could attempt
                    // to removeDep before it has been added.
                    s.db.addDep(s, rows); 

                    // releaseConn must be set before initContextClose or it could
                    // release the connection before it is set.
                    rows.releaseConn = err =>
                    {
                        releaseConn(err);
                        s.db.removeDep(s, rows);
                    }
;
                    context.Context txctx = default;
                    if (s.cg != null)
                    {
                        txctx = s.cg.txCtx();
                    }
                    rows.initContextClose(ctx, txctx);
                    return (rows, null);
                }
                releaseConn(err);
                if (err != driver.ErrBadConn)
                {
                    return (null, err);
                }
            }

            return (null, driver.ErrBadConn);
        });

        // Query executes a prepared query statement with the given arguments
        // and returns the query results as a *Rows.
        private static (ref Rows, error) Query(this ref Stmt s, params object[] args)
        {
            return s.QueryContext(context.Background(), args);
        }

        private static (driver.Rows, error) rowsiFromStatement(context.Context ctx, driver.Conn ci, ref driverStmt _ds, params object[] args) => func(_ds, (ref driverStmt ds, Defer defer, Panic _, Recover __) =>
        {
            args = args.Clone();

            ds.Lock();
            defer(ds.Unlock());

            var (dargs, err) = driverArgsConnLocked(ci, ds, args);
            if (err != null)
            {
                return (null, err);
            } 

            // -1 means the driver doesn't know how to count the number of
            // placeholders, so we won't sanity check input here and instead let the
            // driver deal with errors.
            {
                var want = ds.si.NumInput();

                if (want >= 0L && want != len(dargs))
                {
                    return (null, fmt.Errorf("sql: statement expects %d inputs; got %d", want, len(dargs)));
                }

            }

            var (rowsi, err) = ctxDriverStmtQuery(ctx, ds.si, dargs);
            if (err != null)
            {
                return (null, err);
            }
            return (rowsi, null);
        });

        // QueryRowContext executes a prepared query statement with the given arguments.
        // If an error occurs during the execution of the statement, that error will
        // be returned by a call to Scan on the returned *Row, which is always non-nil.
        // If the query selects no rows, the *Row's Scan will return ErrNoRows.
        // Otherwise, the *Row's Scan scans the first selected row and discards
        // the rest.
        //
        // Example usage:
        //
        //  var name string
        //  err := nameByUseridStmt.QueryRowContext(ctx, id).Scan(&name)
        private static ref Row QueryRowContext(this ref Stmt s, context.Context ctx, params object[] args)
        {
            var (rows, err) = s.QueryContext(ctx, args);
            if (err != null)
            {
                return ref new Row(err:err);
            }
            return ref new Row(rows:rows);
        }

        // QueryRow executes a prepared query statement with the given arguments.
        // If an error occurs during the execution of the statement, that error will
        // be returned by a call to Scan on the returned *Row, which is always non-nil.
        // If the query selects no rows, the *Row's Scan will return ErrNoRows.
        // Otherwise, the *Row's Scan scans the first selected row and discards
        // the rest.
        //
        // Example usage:
        //
        //  var name string
        //  err := nameByUseridStmt.QueryRow(id).Scan(&name)
        private static ref Row QueryRow(this ref Stmt s, params object[] args)
        {
            return s.QueryRowContext(context.Background(), args);
        }

        // Close closes the statement.
        private static error Close(this ref Stmt _s) => func(_s, (ref Stmt s, Defer defer, Panic _, Recover __) =>
        {
            s.closemu.Lock();
            defer(s.closemu.Unlock());

            if (s.stickyErr != null)
            {
                return error.As(s.stickyErr);
            }
            s.mu.Lock();
            if (s.closed)
            {
                s.mu.Unlock();
                return error.As(null);
            }
            s.closed = true;
            var txds = s.cgds;
            s.cgds = null;

            s.mu.Unlock();

            if (s.cg == null)
            {
                return error.As(s.db.removeDep(s, s));
            }
            if (s.parentStmt != null)
            { 
                // If parentStmt is set, we must not close s.txds since it's stored
                // in the css array of the parentStmt.
                return error.As(s.db.removeDep(s.parentStmt, s));
            }
            return error.As(txds.Close());
        });

        private static error finalClose(this ref Stmt _s) => func(_s, (ref Stmt s, Defer defer, Panic _, Recover __) =>
        {
            s.mu.Lock();
            defer(s.mu.Unlock());
            if (s.css != null)
            {
                foreach (var (_, v) in s.css)
                {
                    s.db.noteUnusedDriverStatement(v.dc, v.ds);
                    v.dc.removeOpenStmt(v.ds);
                }
                s.css = null;
            }
            return error.As(null);
        });

        // Rows is the result of a query. Its cursor starts before the first row
        // of the result set. Use Next to advance through the rows:
        //
        //     rows, err := db.Query("SELECT ...")
        //     ...
        //     defer rows.Close()
        //     for rows.Next() {
        //         var id int
        //         var name string
        //         err = rows.Scan(&id, &name)
        //         ...
        //     }
        //     err = rows.Err() // get any error encountered during iteration
        //     ...
        public partial struct Rows
        {
            public ptr<driverConn> dc; // owned; must call releaseConn when closed to release
            public Action<error> releaseConn;
            public driver.Rows rowsi;
            public Action cancel; // called when Rows is closed, may be nil.
            public ptr<driverStmt> closeStmt; // if non-nil, statement to Close on close

// closemu prevents Rows from closing while there
// is an active streaming result. It is held for read during non-close operations
// and exclusively during close.
//
// closemu guards lasterr and closed.
            public sync.RWMutex closemu;
            public bool closed;
            public error lasterr; // non-nil only if closed is true

// lastcols is only used in Scan, Next, and NextResultSet which are expected
// not to be called concurrently.
            public slice<driver.Value> lastcols;
        }

        private static void initContextClose(this ref Rows rs, context.Context ctx, context.Context txctx)
        {
            ctx, rs.cancel = context.WithCancel(ctx);
            go_(() => rs.awaitDone(ctx, txctx));
        }

        // awaitDone blocks until either ctx or txctx is canceled. The ctx is provided
        // from the query context and is canceled when the query Rows is closed.
        // If the query was issued in a transaction, the transaction's context
        // is also provided in txctx to ensure Rows is closed if the Tx is closed.
        private static void awaitDone(this ref Rows rs, context.Context ctx, context.Context txctx)
        {
            channel<object> txctxDone = default;
            if (txctx != null)
            {
                txctxDone = txctx.Done();
            }
            rs.close(ctx.Err());
        }

        // Next prepares the next result row for reading with the Scan method. It
        // returns true on success, or false if there is no next result row or an error
        // happened while preparing it. Err should be consulted to distinguish between
        // the two cases.
        //
        // Every call to Scan, even the first one, must be preceded by a call to Next.
        private static bool Next(this ref Rows rs)
        {
            bool doClose = default;            bool ok = default;

            withLock(rs.closemu.RLocker(), () =>
            {
                doClose, ok = rs.nextLocked();
            });
            if (doClose)
            {
                rs.Close();
            }
            return ok;
        }

        private static (bool, bool) nextLocked(this ref Rows _rs) => func(_rs, (ref Rows rs, Defer defer, Panic _, Recover __) =>
        {
            if (rs.closed)
            {
                return (false, false);
            } 

            // Lock the driver connection before calling the driver interface
            // rowsi to prevent a Tx from rolling back the connection at the same time.
            rs.dc.Lock();
            defer(rs.dc.Unlock());

            if (rs.lastcols == null)
            {
                rs.lastcols = make_slice<driver.Value>(len(rs.rowsi.Columns()));
            }
            rs.lasterr = rs.rowsi.Next(rs.lastcols);
            if (rs.lasterr != null)
            { 
                // Close the connection if there is a driver error.
                if (rs.lasterr != io.EOF)
                {
                    return (true, false);
                }
                driver.RowsNextResultSet (nextResultSet, ok) = rs.rowsi._<driver.RowsNextResultSet>();
                if (!ok)
                {
                    return (true, false);
                } 
                // The driver is at the end of the current result set.
                // Test to see if there is another result set after the current one.
                // Only close Rows if there is no further result sets to read.
                if (!nextResultSet.HasNextResultSet())
                {
                    doClose = true;
                }
                return (doClose, false);
            }
            return (false, true);
        });

        // NextResultSet prepares the next result set for reading. It returns true if
        // there is further result sets, or false if there is no further result set
        // or if there is an error advancing to it. The Err method should be consulted
        // to distinguish between the two cases.
        //
        // After calling NextResultSet, the Next method should always be called before
        // scanning. If there are further result sets they may not have rows in the result
        // set.
        private static bool NextResultSet(this ref Rows _rs) => func(_rs, (ref Rows rs, Defer defer, Panic _, Recover __) =>
        {
            bool doClose = default;
            defer(() =>
            {
                if (doClose)
                {
                    rs.Close();
                }
            }());
            rs.closemu.RLock();
            defer(rs.closemu.RUnlock());

            if (rs.closed)
            {
                return false;
            }
            rs.lastcols = null;
            driver.RowsNextResultSet (nextResultSet, ok) = rs.rowsi._<driver.RowsNextResultSet>();
            if (!ok)
            {
                doClose = true;
                return false;
            } 

            // Lock the driver connection before calling the driver interface
            // rowsi to prevent a Tx from rolling back the connection at the same time.
            rs.dc.Lock();
            defer(rs.dc.Unlock());

            rs.lasterr = nextResultSet.NextResultSet();
            if (rs.lasterr != null)
            {
                doClose = true;
                return false;
            }
            return true;
        });

        // Err returns the error, if any, that was encountered during iteration.
        // Err may be called after an explicit or implicit Close.
        private static error Err(this ref Rows _rs) => func(_rs, (ref Rows rs, Defer defer, Panic _, Recover __) =>
        {
            rs.closemu.RLock();
            defer(rs.closemu.RUnlock());
            if (rs.lasterr == io.EOF)
            {
                return error.As(null);
            }
            return error.As(rs.lasterr);
        });

        // Columns returns the column names.
        // Columns returns an error if the rows are closed, or if the rows
        // are from QueryRow and there was a deferred error.
        private static (slice<@string>, error) Columns(this ref Rows _rs) => func(_rs, (ref Rows rs, Defer defer, Panic _, Recover __) =>
        {
            rs.closemu.RLock();
            defer(rs.closemu.RUnlock());
            if (rs.closed)
            {
                return (null, errors.New("sql: Rows are closed"));
            }
            if (rs.rowsi == null)
            {
                return (null, errors.New("sql: no Rows available"));
            }
            rs.dc.Lock();
            defer(rs.dc.Unlock());

            return (rs.rowsi.Columns(), null);
        });

        // ColumnTypes returns column information such as column type, length,
        // and nullable. Some information may not be available from some drivers.
        private static (slice<ref ColumnType>, error) ColumnTypes(this ref Rows _rs) => func(_rs, (ref Rows rs, Defer defer, Panic _, Recover __) =>
        {
            rs.closemu.RLock();
            defer(rs.closemu.RUnlock());
            if (rs.closed)
            {
                return (null, errors.New("sql: Rows are closed"));
            }
            if (rs.rowsi == null)
            {
                return (null, errors.New("sql: no Rows available"));
            }
            rs.dc.Lock();
            defer(rs.dc.Unlock());

            return (rowsColumnInfoSetupConnLocked(rs.rowsi), null);
        });

        // ColumnType contains the name and type of a column.
        public partial struct ColumnType
        {
            public @string name;
            public bool hasNullable;
            public bool hasLength;
            public bool hasPrecisionScale;
            public bool nullable;
            public long length;
            public @string databaseType;
            public long precision;
            public long scale;
            public reflect.Type scanType;
        }

        // Name returns the name or alias of the column.
        private static @string Name(this ref ColumnType ci)
        {
            return ci.name;
        }

        // Length returns the column type length for variable length column types such
        // as text and binary field types. If the type length is unbounded the value will
        // be math.MaxInt64 (any database limits will still apply).
        // If the column type is not variable length, such as an int, or if not supported
        // by the driver ok is false.
        private static (long, bool) Length(this ref ColumnType ci)
        {
            return (ci.length, ci.hasLength);
        }

        // DecimalSize returns the scale and precision of a decimal type.
        // If not applicable or if not supported ok is false.
        private static (long, long, bool) DecimalSize(this ref ColumnType ci)
        {
            return (ci.precision, ci.scale, ci.hasPrecisionScale);
        }

        // ScanType returns a Go type suitable for scanning into using Rows.Scan.
        // If a driver does not support this property ScanType will return
        // the type of an empty interface.
        private static reflect.Type ScanType(this ref ColumnType ci)
        {
            return ci.scanType;
        }

        // Nullable returns whether the column may be null.
        // If a driver does not support this property ok will be false.
        private static (bool, bool) Nullable(this ref ColumnType ci)
        {
            return (ci.nullable, ci.hasNullable);
        }

        // DatabaseTypeName returns the database system name of the column type. If an empty
        // string is returned the driver type name is not supported.
        // Consult your driver documentation for a list of driver data types. Length specifiers
        // are not included.
        // Common type include "VARCHAR", "TEXT", "NVARCHAR", "DECIMAL", "BOOL", "INT", "BIGINT".
        private static @string DatabaseTypeName(this ref ColumnType ci)
        {
            return ci.databaseType;
        }

        private static slice<ref ColumnType> rowsColumnInfoSetupConnLocked(driver.Rows rowsi)
        {
            var names = rowsi.Columns();

            var list = make_slice<ref ColumnType>(len(names));
            foreach (var (i) in list)
            {
                ColumnType ci = ref new ColumnType(name:names[i],);
                list[i] = ci;

                {
                    driver.RowsColumnTypeScanType prop__prev1 = prop;

                    driver.RowsColumnTypeScanType (prop, ok) = rowsi._<driver.RowsColumnTypeScanType>();

                    if (ok)
                    {
                        ci.scanType = prop.ColumnTypeScanType(i);
                    }
                    else
                    {
                        ci.scanType = reflect.TypeOf(@new<>()).Elem();
                    }

                    prop = prop__prev1;

                }
                {
                    driver.RowsColumnTypeScanType prop__prev1 = prop;

                    (prop, ok) = rowsi._<driver.RowsColumnTypeDatabaseTypeName>();

                    if (ok)
                    {
                        ci.databaseType = prop.ColumnTypeDatabaseTypeName(i);
                    }

                    prop = prop__prev1;

                }
                {
                    driver.RowsColumnTypeScanType prop__prev1 = prop;

                    (prop, ok) = rowsi._<driver.RowsColumnTypeLength>();

                    if (ok)
                    {
                        ci.length, ci.hasLength = prop.ColumnTypeLength(i);
                    }

                    prop = prop__prev1;

                }
                {
                    driver.RowsColumnTypeScanType prop__prev1 = prop;

                    (prop, ok) = rowsi._<driver.RowsColumnTypeNullable>();

                    if (ok)
                    {
                        ci.nullable, ci.hasNullable = prop.ColumnTypeNullable(i);
                    }

                    prop = prop__prev1;

                }
                {
                    driver.RowsColumnTypeScanType prop__prev1 = prop;

                    (prop, ok) = rowsi._<driver.RowsColumnTypePrecisionScale>();

                    if (ok)
                    {
                        ci.precision, ci.scale, ci.hasPrecisionScale = prop.ColumnTypePrecisionScale(i);
                    }

                    prop = prop__prev1;

                }
            }
            return list;
        }

        // Scan copies the columns in the current row into the values pointed
        // at by dest. The number of values in dest must be the same as the
        // number of columns in Rows.
        //
        // Scan converts columns read from the database into the following
        // common Go types and special types provided by the sql package:
        //
        //    *string
        //    *[]byte
        //    *int, *int8, *int16, *int32, *int64
        //    *uint, *uint8, *uint16, *uint32, *uint64
        //    *bool
        //    *float32, *float64
        //    *interface{}
        //    *RawBytes
        //    any type implementing Scanner (see Scanner docs)
        //
        // In the most simple case, if the type of the value from the source
        // column is an integer, bool or string type T and dest is of type *T,
        // Scan simply assigns the value through the pointer.
        //
        // Scan also converts between string and numeric types, as long as no
        // information would be lost. While Scan stringifies all numbers
        // scanned from numeric database columns into *string, scans into
        // numeric types are checked for overflow. For example, a float64 with
        // value 300 or a string with value "300" can scan into a uint16, but
        // not into a uint8, though float64(255) or "255" can scan into a
        // uint8. One exception is that scans of some float64 numbers to
        // strings may lose information when stringifying. In general, scan
        // floating point columns into *float64.
        //
        // If a dest argument has type *[]byte, Scan saves in that argument a
        // copy of the corresponding data. The copy is owned by the caller and
        // can be modified and held indefinitely. The copy can be avoided by
        // using an argument of type *RawBytes instead; see the documentation
        // for RawBytes for restrictions on its use.
        //
        // If an argument has type *interface{}, Scan copies the value
        // provided by the underlying driver without conversion. When scanning
        // from a source value of type []byte to *interface{}, a copy of the
        // slice is made and the caller owns the result.
        //
        // Source values of type time.Time may be scanned into values of type
        // *time.Time, *interface{}, *string, or *[]byte. When converting to
        // the latter two, time.Format3339Nano is used.
        //
        // Source values of type bool may be scanned into types *bool,
        // *interface{}, *string, *[]byte, or *RawBytes.
        //
        // For scanning into *bool, the source may be true, false, 1, 0, or
        // string inputs parseable by strconv.ParseBool.
        private static error Scan(this ref Rows rs, params object[] dest)
        {
            rs.closemu.RLock();
            if (rs.closed)
            {
                rs.closemu.RUnlock();
                return error.As(errors.New("sql: Rows are closed"));
            }
            rs.closemu.RUnlock();

            if (rs.lastcols == null)
            {
                return error.As(errors.New("sql: Scan called without calling Next"));
            }
            if (len(dest) != len(rs.lastcols))
            {
                return error.As(fmt.Errorf("sql: expected %d destination arguments in Scan, not %d", len(rs.lastcols), len(dest)));
            }
            foreach (var (i, sv) in rs.lastcols)
            {
                var err = convertAssign(dest[i], sv);
                if (err != null)
                {
                    return error.As(fmt.Errorf("sql: Scan error on column index %d: %v", i, err));
                }
            }
            return error.As(null);
        }

        // rowsCloseHook returns a function so tests may install the
        // hook through a test only mutex.
        private static Func<Action<ref Rows, ref error>> rowsCloseHook = () => null;

        // Close closes the Rows, preventing further enumeration. If Next is called
        // and returns false and there are no further result sets,
        // the Rows are closed automatically and it will suffice to check the
        // result of Err. Close is idempotent and does not affect the result of Err.
        private static error Close(this ref Rows rs)
        {
            return error.As(rs.close(null));
        }

        private static error close(this ref Rows _rs, error err) => func(_rs, (ref Rows rs, Defer defer, Panic _, Recover __) =>
        {
            rs.closemu.Lock();
            defer(rs.closemu.Unlock());

            if (rs.closed)
            {
                return error.As(null);
            }
            rs.closed = true;

            if (rs.lasterr == null)
            {
                rs.lasterr = err;
            }
            withLock(rs.dc, () =>
            {
                err = rs.rowsi.Close();
            });
            {
                var fn = rowsCloseHook();

                if (fn != null)
                {
                    fn(rs, ref err);
                }

            }
            if (rs.cancel != null)
            {
                rs.cancel();
            }
            if (rs.closeStmt != null)
            {
                rs.closeStmt.Close();
            }
            rs.releaseConn(err);
            return error.As(err);
        });

        // Row is the result of calling QueryRow to select a single row.
        public partial struct Row
        {
            public error err; // deferred error for easy chaining
            public ptr<Rows> rows;
        }

        // Scan copies the columns from the matched row into the values
        // pointed at by dest. See the documentation on Rows.Scan for details.
        // If more than one row matches the query,
        // Scan uses the first row and discards the rest. If no row matches
        // the query, Scan returns ErrNoRows.
        private static error Scan(this ref Row _r, params object[] dest) => func(_r, (ref Row r, Defer defer, Panic _, Recover __) =>
        {
            if (r.err != null)
            {
                return error.As(r.err);
            } 

            // TODO(bradfitz): for now we need to defensively clone all
            // []byte that the driver returned (not permitting
            // *RawBytes in Rows.Scan), since we're about to close
            // the Rows in our defer, when we return from this function.
            // the contract with the driver.Next(...) interface is that it
            // can return slices into read-only temporary memory that's
            // only valid until the next Scan/Close. But the TODO is that
            // for a lot of drivers, this copy will be unnecessary. We
            // should provide an optional interface for drivers to
            // implement to say, "don't worry, the []bytes that I return
            // from Next will not be modified again." (for instance, if
            // they were obtained from the network anyway) But for now we
            // don't care.
            defer(r.rows.Close());
            foreach (var (_, dp) in dest)
            {
                {
                    ref RawBytes (_, ok) = dp._<ref RawBytes>();

                    if (ok)
                    {
                        return error.As(errors.New("sql: RawBytes isn't allowed on Row.Scan"));
                    }

                }
            }
            if (!r.rows.Next())
            {
                {
                    var err__prev2 = err;

                    var err = r.rows.Err();

                    if (err != null)
                    {
                        return error.As(err);
                    }

                    err = err__prev2;

                }
                return error.As(ErrNoRows);
            }
            err = r.rows.Scan(dest);
            if (err != null)
            {
                return error.As(err);
            } 
            // Make sure the query can be processed to completion with no errors.
            {
                var err__prev1 = err;

                err = r.rows.Close();

                if (err != null)
                {
                    return error.As(err);
                }

                err = err__prev1;

            }

            return error.As(null);
        });

        // A Result summarizes an executed SQL command.
        public partial interface Result
        {
            (long, error) LastInsertId(); // RowsAffected returns the number of rows affected by an
// update, insert, or delete. Not every database or database
// driver may support this.
            (long, error) RowsAffected();
        }

        private partial struct driverResult : sync.Locker
        {
            public ref sync.Locker Locker => ref Locker_val; // the *driverConn
            public driver.Result resi;
        }

        private static (long, error) LastInsertId(this driverResult dr) => func((defer, _, __) =>
        {
            dr.Lock();
            defer(dr.Unlock());
            return dr.resi.LastInsertId();
        });

        private static (long, error) RowsAffected(this driverResult dr) => func((defer, _, __) =>
        {
            dr.Lock();
            defer(dr.Unlock());
            return dr.resi.RowsAffected();
        });

        private static @string stack()
        {
            array<byte> buf = new array<byte>(2L << (int)(10L));
            return string(buf[..runtime.Stack(buf[..], false)]);
        }

        // withLock runs while holding lk.
        private static void withLock(sync.Locker lk, Action fn) => func((defer, _, __) =>
        {
            lk.Lock();
            defer(lk.Unlock()); // in case fn panics
            fn();
        });
    }
}}
