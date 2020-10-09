// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Parallel cache.
// This file is copied from cmd/go/internal/par.

// package sumdb -- go2cs converted at 2020 October 09 05:55:59 UTC
// import "cmd/vendor/golang.org/x/mod/sumdb" ==> using sumdb = go.cmd.vendor.golang.org.x.mod.sumdb_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\mod\sumdb\cache.go
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace mod
{
    public static partial class sumdb_package
    {
        // parCache runs an action once per key and caches the result.
        private partial struct parCache
        {
            public sync.Map m;
        }

        private partial struct cacheEntry
        {
            public uint done;
            public sync.Mutex mu;
        }

        // Do calls the function f if and only if Do is being called for the first time with this key.
        // No call to Do with a given key returns until the one call to f returns.
        // Do returns the value returned by the one call to f.
        private static void Do(this ptr<parCache> _addr_c, object key, Action f)
        {
            ref parCache c = ref _addr_c.val;

            var (entryIface, ok) = c.m.Load(key);
            if (!ok)
            {
                entryIface, _ = c.m.LoadOrStore(key, @new<cacheEntry>());
            }

            ptr<cacheEntry> e = entryIface._<ptr<cacheEntry>>();
            if (atomic.LoadUint32(_addr_e.done) == 0L)
            {
                e.mu.Lock();
                if (atomic.LoadUint32(_addr_e.done) == 0L)
                {
                    e.result = f();
                    atomic.StoreUint32(_addr_e.done, 1L);
                }

                e.mu.Unlock();

            }

            return e.result;

        }

        // Get returns the cached result associated with key.
        // It returns nil if there is no such result.
        // If the result for key is being computed, Get does not wait for the computation to finish.
        private static void Get(this ptr<parCache> _addr_c, object key)
        {
            ref parCache c = ref _addr_c.val;

            var (entryIface, ok) = c.m.Load(key);
            if (!ok)
            {
                return null;
            }

            ptr<cacheEntry> e = entryIface._<ptr<cacheEntry>>();
            if (atomic.LoadUint32(_addr_e.done) == 0L)
            {
                return null;
            }

            return e.result;

        }
    }
}}}}}}
