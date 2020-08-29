// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sync -- go2cs converted at 2020 August 29 08:36:43 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Go\src\sync\pool.go
using race = go.@internal.race_package;
using runtime = go.runtime_package;
using atomic = go.sync.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class sync_package
    {
        // A Pool is a set of temporary objects that may be individually saved and
        // retrieved.
        //
        // Any item stored in the Pool may be removed automatically at any time without
        // notification. If the Pool holds the only reference when this happens, the
        // item might be deallocated.
        //
        // A Pool is safe for use by multiple goroutines simultaneously.
        //
        // Pool's purpose is to cache allocated but unused items for later reuse,
        // relieving pressure on the garbage collector. That is, it makes it easy to
        // build efficient, thread-safe free lists. However, it is not suitable for all
        // free lists.
        //
        // An appropriate use of a Pool is to manage a group of temporary items
        // silently shared among and potentially reused by concurrent independent
        // clients of a package. Pool provides a way to amortize allocation overhead
        // across many clients.
        //
        // An example of good use of a Pool is in the fmt package, which maintains a
        // dynamically-sized store of temporary output buffers. The store scales under
        // load (when many goroutines are actively printing) and shrinks when
        // quiescent.
        //
        // On the other hand, a free list maintained as part of a short-lived object is
        // not a suitable use for a Pool, since the overhead does not amortize well in
        // that scenario. It is more efficient to have such objects implement their own
        // free list.
        //
        // A Pool must not be copied after first use.
        public partial struct Pool
        {
            public noCopy noCopy;
            public unsafe.Pointer local; // local fixed-size per-P pool, actual type is [P]poolLocal
            public System.UIntPtr localSize; // size of the local array

// New optionally specifies a function to generate
// a value when Get would otherwise return nil.
// It may not be changed concurrently with calls to Get.
            public Action New;
        }

        // Local per-P Pool appendix.
        private partial struct poolLocalInternal
        {
            public slice<object> shared; // Can be used by any P.
            public ref Mutex Mutex => ref Mutex_val; // Protects shared.
        }

        private partial struct poolLocal
        {
            public ref poolLocalInternal poolLocalInternal => ref poolLocalInternal_val; // Prevents false sharing on widespread platforms with
// 128 mod (cache line size) = 0 .
            public array<byte> pad;
        }

        // from runtime
        private static uint fastrand()
;

        private static array<ulong> poolRaceHash = new array<ulong>(128L);

        // poolRaceAddr returns an address to use as the synchronization point
        // for race detector logic. We don't use the actual pointer stored in x
        // directly, for fear of conflicting with other synchronization on that address.
        // Instead, we hash the pointer to get an index into poolRaceHash.
        // See discussion on golang.org/cl/31589.
        private static unsafe.Pointer poolRaceAddr(object x)
        {
            var ptr = uintptr(new ptr<ref array<unsafe.Pointer>>(@unsafe.Pointer(ref x))[1L]);
            var h = uint32((uint64(uint32(ptr)) * 0x85ebca6bUL) >> (int)(16L));
            return @unsafe.Pointer(ref poolRaceHash[h % uint32(len(poolRaceHash))]);
        }

        // Put adds x to the pool.
        private static void Put(this ref Pool p, object x)
        {>>MARKER:FUNCTION_fastrand_BLOCK_PREFIX<<
            if (x == null)
            {
                return;
            }
            if (race.Enabled)
            {
                if (fastrand() % 4L == 0L)
                { 
                    // Randomly drop x on floor.
                    return;
                }
                race.ReleaseMerge(poolRaceAddr(x));
                race.Disable();
            }
            var l = p.pin();
            if (l.@private == null)
            {
                l.@private = x;
                x = null;
            }
            runtime_procUnpin();
            if (x != null)
            {
                l.Lock();
                l.shared = append(l.shared, x);
                l.Unlock();
            }
            if (race.Enabled)
            {
                race.Enable();
            }
        }

        // Get selects an arbitrary item from the Pool, removes it from the
        // Pool, and returns it to the caller.
        // Get may choose to ignore the pool and treat it as empty.
        // Callers should not assume any relation between values passed to Put and
        // the values returned by Get.
        //
        // If Get would otherwise return nil and p.New is non-nil, Get returns
        // the result of calling p.New.
        private static void Get(this ref Pool p)
        {
            if (race.Enabled)
            {
                race.Disable();
            }
            var l = p.pin();
            var x = l.@private;
            l.@private = null;
            runtime_procUnpin();
            if (x == null)
            {
                l.Lock();
                var last = len(l.shared) - 1L;
                if (last >= 0L)
                {
                    x = l.shared[last];
                    l.shared = l.shared[..last];
                }
                l.Unlock();
                if (x == null)
                {
                    x = p.getSlow();
                }
            }
            if (race.Enabled)
            {
                race.Enable();
                if (x != null)
                {
                    race.Acquire(poolRaceAddr(x));
                }
            }
            if (x == null && p.New != null)
            {
                x = p.New();
            }
            return x;
        }

        private static object getSlow(this ref Pool p)
        { 
            // See the comment in pin regarding ordering of the loads.
            var size = atomic.LoadUintptr(ref p.localSize); // load-acquire
            var local = p.local; // load-consume
            // Try to steal one element from other procs.
            var pid = runtime_procPin();
            runtime_procUnpin();
            for (long i = 0L; i < int(size); i++)
            {
                var l = indexLocal(local, (pid + i + 1L) % int(size));
                l.Lock();
                var last = len(l.shared) - 1L;
                if (last >= 0L)
                {
                    x = l.shared[last];
                    l.shared = l.shared[..last];
                    l.Unlock();
                    break;
                }
                l.Unlock();
            }

            return x;
        }

        // pin pins the current goroutine to P, disables preemption and returns poolLocal pool for the P.
        // Caller must call runtime_procUnpin() when done with the pool.
        private static ref poolLocal pin(this ref Pool p)
        {
            var pid = runtime_procPin(); 
            // In pinSlow we store to localSize and then to local, here we load in opposite order.
            // Since we've disabled preemption, GC cannot happen in between.
            // Thus here we must observe local at least as large localSize.
            // We can observe a newer/larger local, it is fine (we must observe its zero-initialized-ness).
            var s = atomic.LoadUintptr(ref p.localSize); // load-acquire
            var l = p.local; // load-consume
            if (uintptr(pid) < s)
            {
                return indexLocal(l, pid);
            }
            return p.pinSlow();
        }

        private static ref poolLocal pinSlow(this ref Pool _p) => func(_p, (ref Pool p, Defer defer, Panic _, Recover __) =>
        { 
            // Retry under the mutex.
            // Can not lock the mutex while pinned.
            runtime_procUnpin();
            allPoolsMu.Lock();
            defer(allPoolsMu.Unlock());
            var pid = runtime_procPin(); 
            // poolCleanup won't be called while we are pinned.
            var s = p.localSize;
            var l = p.local;
            if (uintptr(pid) < s)
            {
                return indexLocal(l, pid);
            }
            if (p.local == null)
            {
                allPools = append(allPools, p);
            } 
            // If GOMAXPROCS changes between GCs, we re-allocate the array and lose the old one.
            var size = runtime.GOMAXPROCS(0L);
            var local = make_slice<poolLocal>(size);
            atomic.StorePointer(ref p.local, @unsafe.Pointer(ref local[0L])); // store-release
            atomic.StoreUintptr(ref p.localSize, uintptr(size)); // store-release
            return ref local[pid];
        });

        private static void poolCleanup()
        { 
            // This function is called with the world stopped, at the beginning of a garbage collection.
            // It must not allocate and probably should not call any runtime functions.
            // Defensively zero out everything, 2 reasons:
            // 1. To prevent false retention of whole Pools.
            // 2. If GC happens while a goroutine works with l.shared in Put/Get,
            //    it will retain whole Pool. So next cycle memory consumption would be doubled.
            {
                var i__prev1 = i;

                foreach (var (__i, __p) in allPools)
                {
                    i = __i;
                    p = __p;
                    allPools[i] = null;
                    {
                        var i__prev2 = i;

                        for (long i = 0L; i < int(p.localSize); i++)
                        {
                            var l = indexLocal(p.local, i);
                            l.@private = null;
                            foreach (var (j) in l.shared)
                            {
                                l.shared[j] = null;
                            }
                            l.shared = null;
                        }


                        i = i__prev2;
                    }
                    p.local = null;
                    p.localSize = 0L;
                }

                i = i__prev1;
            }

            allPools = new slice<ref Pool>(new ref Pool[] {  });
        }

        private static Mutex allPoolsMu = default;        private static slice<ref Pool> allPools = default;

        private static void init()
        {
            runtime_registerPoolCleanup(poolCleanup);
        }

        private static ref poolLocal indexLocal(unsafe.Pointer l, long i)
        {
            var lp = @unsafe.Pointer(uintptr(l) + uintptr(i) * @unsafe.Sizeof(new poolLocal()));
            return (poolLocal.Value)(lp);
        }

        // Implemented in runtime.
        private static void runtime_registerPoolCleanup(Action cleanup)
;
        private static long runtime_procPin()
;
        private static void runtime_procUnpin()
;
    }
}
