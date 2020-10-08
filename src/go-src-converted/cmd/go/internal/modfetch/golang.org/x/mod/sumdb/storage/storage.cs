// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package storage defines storage interfaces for and a basic implementation of a checksum database.
// package storage -- go2cs converted at 2020 October 08 04:36:23 UTC
// import "golang.org/x/mod/sumdb/storage" ==> using storage = go.golang.org.x.mod.sumdb.storage_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\mod\sumdb\storage\storage.go
using context = go.context_package;
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
        // A Storage is a transaction key-value storage system.
        public partial interface Storage
        {
            error ReadOnly(context.Context ctx, Func<context.Context, Transaction, error> f); // ReadWrite runs f in a read-write transaction.
// If f returns an error, the transaction aborts and returns that error.
// If f returns nil, the transaction attempts to commit and then then return nil.
// Otherwise it tries again. Note that f may be called multiple times and that
// the result only describes the effect of the final call to f.
// The caller must take care not to use any state computed during
// earlier calls to f, or even the last call to f when an error is returned.
            error ReadWrite(context.Context ctx, Func<context.Context, Transaction, error> f);
        }

        // A Transaction provides read and write operations within a transaction,
        // as executed by Storage's ReadOnly or ReadWrite methods.
        public partial interface Transaction
        {
            error ReadValue(context.Context ctx, @string key); // ReadValues reads the values associated with the given keys.
// If there is no value stored for a given key, ReadValues returns an empty value for that key.
// An error is only returned for problems accessing the storage.
            error ReadValues(context.Context ctx, slice<@string> keys); // BufferWrites buffers the given writes,
// to be applied at the end of the transaction.
// BufferWrites panics if this is a ReadOnly transaction.
// It returns an error if it detects any other problems.
// The behavior of multiple writes buffered using the same key
// is undefined: it may return an error or not.
            error BufferWrites(slice<Write> writes);
        }

        // A Write is a single change to be applied at the end of a read-write transaction.
        // A Write with an empty value deletes the value associated with the given key.
        public partial struct Write
        {
            public @string Key;
            public @string Value;
        }
    }
}}}}}
