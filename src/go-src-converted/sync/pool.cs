// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sync -- go2cs converted at 2022 March 13 05:24:06 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Program Files\Go\src\sync\pool.go
namespace go;

using race = @internal.race_package;
using runtime = runtime_package;
using atomic = sync.atomic_package;
using @unsafe = @unsafe_package;


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

using System;
public static partial class sync_package {

public partial struct Pool {
    public noCopy noCopy;
    public unsafe.Pointer local; // local fixed-size per-P pool, actual type is [P]poolLocal
    public System.UIntPtr localSize; // size of the local array

    public unsafe.Pointer victim; // local from previous cycle
    public System.UIntPtr victimSize; // size of victims array

// New optionally specifies a function to generate
// a value when Get would otherwise return nil.
// It may not be changed concurrently with calls to Get.
    public Action New;
}

// Local per-P Pool appendix.
private partial struct poolLocalInternal {
    public poolChain shared; // Local P can pushHead/popHead; any P can popTail.
}

private partial struct poolLocal {
    public ref poolLocalInternal poolLocalInternal => ref poolLocalInternal_val; // Prevents false sharing on widespread platforms with
// 128 mod (cache line size) = 0 .
    public array<byte> pad;
}

// from runtime
private static uint fastrand();

private static array<ulong> poolRaceHash = new array<ulong>(128);

// poolRaceAddr returns an address to use as the synchronization point
// for race detector logic. We don't use the actual pointer stored in x
// directly, for fear of conflicting with other synchronization on that address.
// Instead, we hash the pointer to get an index into poolRaceHash.
// See discussion on golang.org/cl/31589.
private static unsafe.Pointer poolRaceAddr(object x) {
    var ptr = uintptr(new ptr<ptr<array<unsafe.Pointer>>>(@unsafe.Pointer(_addr_x))[1]);
    var h = uint32((uint64(uint32(ptr)) * 0x85ebca6b) >> 16);
    return @unsafe.Pointer(_addr_poolRaceHash[h % uint32(len(poolRaceHash))]);
}

// Put adds x to the pool.
private static void Put(this ptr<Pool> _addr_p, object x) {
    ref Pool p = ref _addr_p.val;

    if (x == null) {>>MARKER:FUNCTION_fastrand_BLOCK_PREFIX<<
        return ;
    }
    if (race.Enabled) {
        if (fastrand() % 4 == 0) { 
            // Randomly drop x on floor.
            return ;
        }
        race.ReleaseMerge(poolRaceAddr(x));
        race.Disable();
    }
    var (l, _) = p.pin();
    if (l.@private == null) {
        l.@private = x;
        x = null;
    }
    if (x != null) {
        l.shared.pushHead(x);
    }
    runtime_procUnpin();
    if (race.Enabled) {
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
private static void Get(this ptr<Pool> _addr_p) {
    ref Pool p = ref _addr_p.val;

    if (race.Enabled) {
        race.Disable();
    }
    var (l, pid) = p.pin();
    var x = l.@private;
    l.@private = null;
    if (x == null) { 
        // Try to pop the head of the local shard. We prefer
        // the head over the tail for temporal locality of
        // reuse.
        x, _ = l.shared.popHead();
        if (x == null) {
            x = p.getSlow(pid);
        }
    }
    runtime_procUnpin();
    if (race.Enabled) {
        race.Enable();
        if (x != null) {
            race.Acquire(poolRaceAddr(x));
        }
    }
    if (x == null && p.New != null) {
        x = p.New();
    }
    return x;
}

private static void getSlow(this ptr<Pool> _addr_p, nint pid) {
    ref Pool p = ref _addr_p.val;
 
    // See the comment in pin regarding ordering of the loads.
    var size = runtime_LoadAcquintptr(_addr_p.localSize); // load-acquire
    var locals = p.local; // load-consume
    // Try to steal one element from other procs.
    {
        nint i__prev1 = i;

        for (nint i = 0; i < int(size); i++) {
            var l = indexLocal(locals, (pid + i + 1) % int(size));
            {
                var x__prev1 = x;

                var (x, _) = l.shared.popTail();

                if (x != null) {
                    return x;
                }

                x = x__prev1;

            }
        }

        i = i__prev1;
    } 

    // Try the victim cache. We do this after attempting to steal
    // from all primary caches because we want objects in the
    // victim cache to age out if at all possible.
    size = atomic.LoadUintptr(_addr_p.victimSize);
    if (uintptr(pid) >= size) {
        return null;
    }
    locals = p.victim;
    l = indexLocal(locals, pid);
    {
        var x__prev1 = x;

        var x = l.@private;

        if (x != null) {
            l.@private = null;
            return x;
        }
        x = x__prev1;

    }
    {
        nint i__prev1 = i;

        for (i = 0; i < int(size); i++) {
            l = indexLocal(locals, (pid + i) % int(size));
            {
                var x__prev1 = x;

                (x, _) = l.shared.popTail();

                if (x != null) {
                    return x;
                }

                x = x__prev1;

            }
        }

        i = i__prev1;
    } 

    // Mark the victim cache as empty for future gets don't bother
    // with it.
    atomic.StoreUintptr(_addr_p.victimSize, 0);

    return null;
}

// pin pins the current goroutine to P, disables preemption and
// returns poolLocal pool for the P and the P's id.
// Caller must call runtime_procUnpin() when done with the pool.
private static (ptr<poolLocal>, nint) pin(this ptr<Pool> _addr_p) {
    ptr<poolLocal> _p0 = default!;
    nint _p0 = default;
    ref Pool p = ref _addr_p.val;

    var pid = runtime_procPin(); 
    // In pinSlow we store to local and then to localSize, here we load in opposite order.
    // Since we've disabled preemption, GC cannot happen in between.
    // Thus here we must observe local at least as large localSize.
    // We can observe a newer/larger local, it is fine (we must observe its zero-initialized-ness).
    var s = runtime_LoadAcquintptr(_addr_p.localSize); // load-acquire
    var l = p.local; // load-consume
    if (uintptr(pid) < s) {
        return (_addr_indexLocal(l, pid)!, pid);
    }
    return _addr_p.pinSlow()!;
}

private static (ptr<poolLocal>, nint) pinSlow(this ptr<Pool> _addr_p) => func((defer, _, _) => {
    ptr<poolLocal> _p0 = default!;
    nint _p0 = default;
    ref Pool p = ref _addr_p.val;
 
    // Retry under the mutex.
    // Can not lock the mutex while pinned.
    runtime_procUnpin();
    allPoolsMu.Lock();
    defer(allPoolsMu.Unlock());
    var pid = runtime_procPin(); 
    // poolCleanup won't be called while we are pinned.
    var s = p.localSize;
    var l = p.local;
    if (uintptr(pid) < s) {
        return (_addr_indexLocal(l, pid)!, pid);
    }
    if (p.local == null) {
        allPools = append(allPools, p);
    }
    var size = runtime.GOMAXPROCS(0);
    var local = make_slice<poolLocal>(size);
    atomic.StorePointer(_addr_p.local, @unsafe.Pointer(_addr_local[0])); // store-release
    runtime_StoreReluintptr(_addr_p.localSize, uintptr(size)); // store-release
    return (_addr__addr_local[pid]!, pid);
});

private static void poolCleanup() { 
    // This function is called with the world stopped, at the beginning of a garbage collection.
    // It must not allocate and probably should not call any runtime functions.

    // Because the world is stopped, no pool user can be in a
    // pinned section (in effect, this has all Ps pinned).

    // Drop victim caches from all pools.
    {
        var p__prev1 = p;

        foreach (var (_, __p) in oldPools) {
            p = __p;
            p.victim = null;
            p.victimSize = 0;
        }
        p = p__prev1;
    }

    {
        var p__prev1 = p;

        foreach (var (_, __p) in allPools) {
            p = __p;
            p.victim = p.local;
            p.victimSize = p.localSize;
            p.local = null;
            p.localSize = 0;
        }
        p = p__prev1;
    }

    (oldPools, allPools) = (allPools, null);
}

private static Mutex allPoolsMu = default;private static slice<ptr<Pool>> allPools = default;private static slice<ptr<Pool>> oldPools = default;

private static void init() {
    runtime_registerPoolCleanup(poolCleanup);
}

private static ptr<poolLocal> indexLocal(unsafe.Pointer l, nint i) {
    var lp = @unsafe.Pointer(uintptr(l) + uintptr(i) * @unsafe.Sizeof(new poolLocal()));
    return _addr_(poolLocal.val)(lp)!;
}

// Implemented in runtime.
private static void runtime_registerPoolCleanup(Action cleanup);
private static nint runtime_procPin();
private static void runtime_procUnpin();

// The below are implemented in runtime/internal/atomic and the
// compiler also knows to intrinsify the symbol we linkname into this
// package.

//go:linkname runtime_LoadAcquintptr runtime/internal/atomic.LoadAcquintptr
private static System.UIntPtr runtime_LoadAcquintptr(ptr<System.UIntPtr> ptr);

//go:linkname runtime_StoreReluintptr runtime/internal/atomic.StoreReluintptr
private static System.UIntPtr runtime_StoreReluintptr(ptr<System.UIntPtr> ptr, System.UIntPtr val);

} // end sync_package
