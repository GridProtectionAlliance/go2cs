// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package par implements parallel execution helpers.
// package par -- go2cs converted at 2020 October 09 05:45:32 UTC
// import "cmd/go/internal/par" ==> using par = go.cmd.go.@internal.par_package
// Original source: C:\Go\src\cmd\go\internal\par\work.go
using rand = go.math.rand_package;
using sync = go.sync_package;
using atomic = go.sync.atomic_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class par_package
    {
        // Work manages a set of work items to be executed in parallel, at most once each.
        // The items in the set must all be valid map keys.
        public partial struct Work
        {
            public Action<object> f; // function to run for each item
            public long running; // total number of runners

            public sync.Mutex mu;
            public slice<object> todo; // items yet to be run
            public sync.Cond wait; // wait when todo is empty
            public long waiting; // number of runners waiting for todo
        }

        private static void init(this ptr<Work> _addr_w)
        {
            ref Work w = ref _addr_w.val;

            if (w.added == null)
            {
                w.added = make();
            }

        }

        // Add adds item to the work set, if it hasn't already been added.
        private static void Add(this ptr<Work> _addr_w, object item)
        {
            ref Work w = ref _addr_w.val;

            w.mu.Lock();
            w.init();
            if (!w.added[item])
            {
                w.added[item] = true;
                w.todo = append(w.todo, item);
                if (w.waiting > 0L)
                {
                    w.wait.Signal();
                }

            }

            w.mu.Unlock();

        }

        // Do runs f in parallel on items from the work set,
        // with at most n invocations of f running at a time.
        // It returns when everything added to the work set has been processed.
        // At least one item should have been added to the work set
        // before calling Do (or else Do returns immediately),
        // but it is allowed for f(item) to add new items to the set.
        // Do should only be used once on a given Work.
        private static void Do(this ptr<Work> _addr_w, long n, Action<object> f) => func((_, panic, __) =>
        {
            ref Work w = ref _addr_w.val;

            if (n < 1L)
            {
                panic("par.Work.Do: n < 1");
            }

            if (w.running >= 1L)
            {
                panic("par.Work.Do: already called Do");
            }

            w.running = n;
            w.f = f;
            w.wait.L = _addr_w.mu;

            for (long i = 0L; i < n - 1L; i++)
            {
                go_(() => w.runner());
            }

            w.runner();

        });

        // runner executes work in w until both nothing is left to do
        // and all the runners are waiting for work.
        // (Then all the runners return.)
        private static void runner(this ptr<Work> _addr_w)
        {
            ref Work w = ref _addr_w.val;

            while (true)
            { 
                // Wait for something to do.
                w.mu.Lock();
                while (len(w.todo) == 0L)
                {
                    w.waiting++;
                    if (w.waiting == w.running)
                    { 
                        // All done.
                        w.wait.Broadcast();
                        w.mu.Unlock();
                        return ;

                    }

                    w.wait.Wait();
                    w.waiting--;

                } 

                // Pick something to do at random,
                // to eliminate pathological contention
                // in case items added at about the same time
                // are most likely to contend.
 

                // Pick something to do at random,
                // to eliminate pathological contention
                // in case items added at about the same time
                // are most likely to contend.
                var i = rand.Intn(len(w.todo));
                var item = w.todo[i];
                w.todo[i] = w.todo[len(w.todo) - 1L];
                w.todo = w.todo[..len(w.todo) - 1L];
                w.mu.Unlock();

                w.f(item);

            }


        }

        // Cache runs an action once per key and caches the result.
        public partial struct Cache
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
        private static void Do(this ptr<Cache> _addr_c, object key, Action f)
        {
            ref Cache c = ref _addr_c.val;

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
        private static void Get(this ptr<Cache> _addr_c, object key)
        {
            ref Cache c = ref _addr_c.val;

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

        // Clear removes all entries in the cache.
        //
        // Concurrent calls to Get may return old values. Concurrent calls to Do
        // may return old values or store results in entries that have been deleted.
        //
        // TODO(jayconrod): Delete this after the package cache clearing functions
        // in internal/load have been removed.
        private static void Clear(this ptr<Cache> _addr_c)
        {
            ref Cache c = ref _addr_c.val;

            c.m.Range((key, value) =>
            {
                c.m.Delete(key);
                return true;
            });

        }

        // Delete removes an entry from the map. It is safe to call Delete for an
        // entry that does not exist. Delete will return quickly, even if the result
        // for a key is still being computed; the computation will finish, but the
        // result won't be accessible through the cache.
        //
        // TODO(jayconrod): Delete this after the package cache clearing functions
        // in internal/load have been removed.
        private static void Delete(this ptr<Cache> _addr_c, object key)
        {
            ref Cache c = ref _addr_c.val;

            c.m.Delete(key);
        }

        // DeleteIf calls pred for each key in the map. If pred returns true for a key,
        // DeleteIf removes the corresponding entry. If the result for a key is
        // still being computed, DeleteIf will remove the entry without waiting for
        // the computation to finish. The result won't be accessible through the cache.
        //
        // TODO(jayconrod): Delete this after the package cache clearing functions
        // in internal/load have been removed.
        private static bool DeleteIf(this ptr<Cache> _addr_c, Func<object, bool> pred)
        {
            ref Cache c = ref _addr_c.val;

            c.m.Range((key, _) =>
            {
                if (pred(key))
                {
                    c.Delete(key);
                }

                return true;

            });

        }
    }
}}}}
