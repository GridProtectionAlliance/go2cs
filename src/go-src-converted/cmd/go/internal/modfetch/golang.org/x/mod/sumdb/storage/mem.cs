// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package storage -- go2cs converted at 2020 October 08 04:36:23 UTC
// import "golang.org/x/mod/sumdb/storage" ==> using storage = go.golang.org.x.mod.sumdb.storage_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\sumdb\storage\mem.go
using context = go.context_package;
using errors = go.errors_package;
using rand = go.math.rand_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace mod {
namespace sumdb
{
    public static partial class storage_package
    {
        // Mem is an in-memory implementation of Storage.
        // It is meant for tests and does not store any data to persistent storage.
        //
        // The zero value is an empty Mem ready for use.
        public partial struct Mem
        {
            public sync.RWMutex mu;
            public map<@string, @string> table;
        }

        // A memTx is a transaction in a Mem.
        private partial struct memTx
        {
            public ptr<Mem> m;
            public slice<Write> writes;
        }

        // errRetry is an internal sentinel indicating that the transaction should be retried.
        // It is never returned to the caller.
        private static var errRetry = errors.New("retry");

        // ReadOnly runs f in a read-only transaction.
        private static error ReadOnly(this ptr<Mem> _addr_m, context.Context ctx, Func<context.Context, Transaction, error> f) => func((defer, _, __) =>
        {
            ref Mem m = ref _addr_m.val;

            ptr<memTx> tx = addr(new memTx(m:m));
            while (true)
            {
                err = () =>
                {
                    m.mu.Lock();
                    defer(m.mu.Unlock());

                    {
                        var err__prev1 = err;

                        var err = f(ctx, tx);

                        if (err != null)
                        {
                            return error.As(err)!;
                        } 
                        // Spurious retry with 10% probability.

                        err = err__prev1;

                    } 
                    // Spurious retry with 10% probability.
                    if (rand.Intn(10L) == 0L)
                    {
                        return error.As(errRetry)!;
                    }
                    return error.As(null!)!;
                }();
                if (err != errRetry)
                {
                    return error.As(err)!;
                }
            }
        });

        // ReadWrite runs f in a read-write transaction.
        private static error ReadWrite(this ptr<Mem> _addr_m, context.Context ctx, Func<context.Context, Transaction, error> f) => func((defer, _, __) =>
        {
            ref Mem m = ref _addr_m.val;

            ptr<memTx> tx = addr(new memTx(m:m));
            while (true)
            {
                err = () =>
                {
                    m.mu.Lock();
                    defer(m.mu.Unlock());

                    tx.writes = new slice<Write>(new Write[] {  });
                    {
                        var err__prev1 = err;

                        var err = f(ctx, tx);

                        if (err != null)
                        {
                            return error.As(err)!;
                        } 
                        // Spurious retry with 10% probability.

                        err = err__prev1;

                    } 
                    // Spurious retry with 10% probability.
                    if (rand.Intn(10L) == 0L)
                    {
                        return error.As(errRetry)!;
                    }
                    if (m.table == null)
                    {
                        m.table = make_map<@string, @string>();
                    }
                    foreach (var (_, w) in tx.writes)
                    {
                        if (w.Value == "")
                        {
                            delete(m.table, w.Key);
                        }
                        else
                        {
                            m.table[w.Key] = w.Value;
                        }
                    }
                    return error.As(null!)!;
                }();
                if (err != errRetry)
                {
                    return error.As(err)!;
                }
            }
        });

        // ReadValues returns the values associated with the given keys.
        private static (slice<@string>, error) ReadValues(this ptr<memTx> _addr_tx, context.Context ctx, slice<@string> keys)
        {
            slice<@string> _p0 = default;
            error _p0 = default!;
            ref memTx tx = ref _addr_tx.val;

            var vals = make_slice<@string>(len(keys));
            foreach (var (i, key) in keys)
            {
                vals[i] = tx.m.table[key];
            }
            return (vals, error.As(null!)!);
        }

        // ReadValue returns the value associated with the single key.
        private static (@string, error) ReadValue(this ptr<memTx> _addr_tx, context.Context ctx, @string key)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref memTx tx = ref _addr_tx.val;

            return (tx.m.table[key], error.As(null!)!);
        }

        // BufferWrites buffers a list of writes to be applied
        // to the table when the transaction commits.
        // The changes are not visible to reads within the transaction.
        // The map argument is not used after the call returns.
        private static error BufferWrites(this ptr<memTx> _addr_tx, slice<Write> list) => func((_, panic, __) =>
        {
            ref memTx tx = ref _addr_tx.val;

            if (tx.writes == null)
            {
                panic("BufferWrite on read-only transaction");
            }
            tx.writes = append(tx.writes, list);
            return error.As(null!)!;
        });
    }
}}}}}
