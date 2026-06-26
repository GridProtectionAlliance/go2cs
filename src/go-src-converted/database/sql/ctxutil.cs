// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.database;

using context = context_package;
using driver = database.sql.driver_package;
using errors = errors_package;
using database.sql;

partial class sql_package {

internal static (driver.Stmt, error) ctxDriverPrepare(context.Context ctx, driver.Conn ci, @string query) {
    {
        var (ciCtx, @is) = ci._<driver.ConnPrepareContext>(ᐧ); if (@is) {
            return ciCtx.PrepareContext(ctx, query);
        }
    }
    (si, err) = ci.Prepare(query);
    if (err == default!) {
        switch (ᐧ) {
        case ᐧ when ctx.Done().ꟷᐳ(out _): {
            si.Close();
            return (default!, ctx.Err());
        }
        default: {
        }}
    }
    return (si, err);
}

internal static (driver.Result, error) ctxDriverExec(context.Context ctx, driver.ExecerContext execerCtx, driver.Execer execer, @string query, slice<driver.NamedValue> nvdargs) {
    if (execerCtx != default!) {
        return execerCtx.ExecContext(ctx, query, nvdargs);
    }
    (dargs, err) = namedValueToValue(nvdargs);
    if (err != default!) {
        return (default!, err);
    }
    switch (ᐧ) {
    case ᐧ when ctx.Done().ꟷᐳ(out _): {
        return (default!, ctx.Err());
    }
    default: {
    }}
    return execer.Exec(query, dargs);
}

internal static (driver.Rows, error) ctxDriverQuery(context.Context ctx, driver.QueryerContext queryerCtx, driver.Queryer queryer, @string query, slice<driver.NamedValue> nvdargs) {
    if (queryerCtx != default!) {
        return queryerCtx.QueryContext(ctx, query, nvdargs);
    }
    (dargs, err) = namedValueToValue(nvdargs);
    if (err != default!) {
        return (default!, err);
    }
    switch (ᐧ) {
    case ᐧ when ctx.Done().ꟷᐳ(out _): {
        return (default!, ctx.Err());
    }
    default: {
    }}
    return queryer.Query(query, dargs);
}

internal static (driver.Result, error) ctxDriverStmtExec(context.Context ctx, driver.Stmt si, slice<driver.NamedValue> nvdargs) {
    {
        var (siCtx, @is) = si._<driver.StmtExecContext>(ᐧ); if (@is) {
            return siCtx.ExecContext(ctx, nvdargs);
        }
    }
    (dargs, err) = namedValueToValue(nvdargs);
    if (err != default!) {
        return (default!, err);
    }
    switch (ᐧ) {
    case ᐧ when ctx.Done().ꟷᐳ(out _): {
        return (default!, ctx.Err());
    }
    default: {
    }}
    return si.Exec(dargs);
}

internal static (driver.Rows, error) ctxDriverStmtQuery(context.Context ctx, driver.Stmt si, slice<driver.NamedValue> nvdargs) {
    {
        var (siCtx, @is) = si._<driver.StmtQueryContext>(ᐧ); if (@is) {
            return siCtx.QueryContext(ctx, nvdargs);
        }
    }
    (dargs, err) = namedValueToValue(nvdargs);
    if (err != default!) {
        return (default!, err);
    }
    switch (ᐧ) {
    case ᐧ when ctx.Done().ꟷᐳ(out _): {
        return (default!, ctx.Err());
    }
    default: {
    }}
    return si.Query(dargs);
}

internal static (driver.Tx, error) ctxDriverBegin(context.Context ctx, ж<TxOptions> Ꮡopts, driver.Conn ci) {
    ref var opts = ref Ꮡopts.val;

    {
        var (ciCtx, @is) = ci._<driver.ConnBeginTx>(ᐧ); if (@is) {
            var dopts = new driver.TxOptions(nil);
            if (opts != nil) {
                dopts.Isolation = ((driver.IsolationLevel)opts.Isolation);
                dopts.ReadOnly = opts.ReadOnly;
            }
            return ciCtx.BeginTx(ctx, dopts);
        }
    }
    if (opts != nil) {
        // Check the transaction level. If the transaction level is non-default
        // then return an error here as the BeginTx driver value is not supported.
        if (opts.Isolation != LevelDefault) {
            return (default!, errors.New("sql: driver does not support non-default isolation level"u8));
        }
        // If a read-only transaction is requested return an error as the
        // BeginTx driver value is not supported.
        if (opts.ReadOnly) {
            return (default!, errors.New("sql: driver does not support read-only transactions"u8));
        }
    }
    if (ctx.Done() == default!) {
        return ci.Begin();
    }
    (txi, err) = ci.Begin();
    if (err == default!) {
        switch (ᐧ) {
        case ᐧ when ctx.Done().ꟷᐳ(out _): {
            txi.Rollback();
            return (default!, ctx.Err());
        }
        default: {
        }}
    }
    return (txi, err);
}

internal static (slice<driver.Value>, error) namedValueToValue(slice<driver.NamedValue> named) {
    var dargs = new slice<driver.Value>(len(named));
    foreach (var (n, param) in named) {
        if (len(param.Name) > 0) {
            return (default!, errors.New("sql: driver does not support the use of Named Parameters"u8));
        }
        dargs[n] = param.Value;
    }
    return (dargs, default!);
}

} // end sql_package
