// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using race = @internal.race_package;
using runtime = runtime_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using sync;

partial class sync_package {

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
//
// In the terminology of [the Go memory model], a call to Put(x) “synchronizes before”
// a call to [Pool.Get] returning that same value x.
// Similarly, a call to New returning x “synchronizes before”
// a call to Get returning that same value x.
//
// [the Go memory model]: https://go.dev/ref/mem
[GoType] partial struct Pool {
    internal noCopy noCopy;
    internal @unsafe.Pointer local; // local fixed-size per-P pool, actual type is [P]poolLocal
    internal uintptr localSize;        // size of the local array
    internal @unsafe.Pointer victim; // local from previous cycle
    internal uintptr victimSize;        // size of victims array
    // New optionally specifies a function to generate
    // a value when Get would otherwise return nil.
    // It may not be changed concurrently with calls to Get.
    public Func<any> New;
}

// Local per-P Pool appendix.
[GoType] partial struct poolLocalInternal {
    internal any @private;       // Can be used only by the respective P.
    internal poolChain shared; // Local P can pushHead/popHead; any P can popTail.
}

[GoType] partial struct poolLocal {
    internal partial ref poolLocalInternal poolLocalInternal { get; }
    // Prevents false sharing on widespread platforms with
    // 128 mod (cache line size) = 0 .
    internal array<byte> pad = new(128 - @unsafe.Sizeof(new poolLocalInternal(nil)) % 128);
}

// from runtime
//
//go:linkname runtime_randn runtime.randn
internal static partial uint32 runtime_randn(uint32 n);

internal static array<uint64> poolRaceHash;

// poolRaceAddr returns an address to use as the synchronization point
// for race detector logic. We don't use the actual pointer stored in x
// directly, for fear of conflicting with other synchronization on that address.
// Instead, we hash the pointer to get an index into poolRaceHash.
// See discussion on golang.org/cl/31589.
internal static @unsafe.Pointer poolRaceAddr(any x) {
    var ptr = ((uintptr)(ж<array<@unsafe.Pointer>>)(uintptr)(new @unsafe.Pointer(Ꮡ(x)))[1]);
    var h = ((uint32)((((uint64)((uint32)ptr)) * (nint)2246822507L) >> (int)(16)));
    return new @unsafe.Pointer(ᏑpoolRaceHash.at<uint64>(h % ((uint32)len(poolRaceHash))));
}

// Put adds x to the pool.
[GoRecv] public static void Put(this ref Pool p, any x) {
    if (x == default!) {
        return;
    }
    if (race.Enabled) {
        if (runtime_randn(4) == 0) {
            // Randomly drop x on floor.
            return;
        }
        race.ReleaseMerge((uintptr)poolRaceAddr(x));
        race.Disable();
    }
    var (l, _) = p.pin();
    if (l.@private == default!){
        l.@private = x;
    } else {
        l.shared.pushHead(x);
    }
    runtime_procUnpin();
    if (race.Enabled) {
        race.Enable();
    }
}

// Get selects an arbitrary item from the [Pool], removes it from the
// Pool, and returns it to the caller.
// Get may choose to ignore the pool and treat it as empty.
// Callers should not assume any relation between values passed to [Pool.Put] and
// the values returned by Get.
//
// If Get would otherwise return nil and p.New is non-nil, Get returns
// the result of calling p.New.
[GoRecv] public static any Get(this ref Pool p) {
    if (race.Enabled) {
        race.Disable();
    }
    var (l, pid) = p.pin();
    var x = l.@private;
    l.@private = default!;
    if (x == default!) {
        // Try to pop the head of the local shard. We prefer
        // the head over the tail for temporal locality of
        // reuse.
        (x, _) = l.shared.popHead();
        if (x == default!) {
            x = p.getSlow(pid);
        }
    }
    runtime_procUnpin();
    if (race.Enabled) {
        race.Enable();
        if (x != default!) {
            race.Acquire((uintptr)poolRaceAddr(x));
        }
    }
    if (x == default! && p.New != default!) {
        x = p.New();
    }
    return x;
}

[GoRecv] internal static any getSlow(this ref Pool p, nint pid) {
    // See the comment in pin regarding ordering of the loads.
    var size = runtime_LoadAcquintptr(Ꮡ(p.localSize));
    // load-acquire
    @unsafe.Pointer locals = p.local;
    // load-consume
    // Try to steal one element from other procs.
    for (nint i = 0; i < ((nint)size); i++) {
        var lΔ1 = indexLocal(locals, (pid + i + 1) % ((nint)size));
        {
            var (x, _) = lΔ1.shared.popTail(); if (x != default!) {
                return x;
            }
        }
    }
    // Try the victim cache. We do this after attempting to steal
    // from all primary caches because we want objects in the
    // victim cache to age out if at all possible.
    size = atomic.LoadUintptr(Ꮡ(p.victimSize));
    if (((uintptr)pid) >= size) {
        return default!;
    }
    locals = p.victim;
    var l = indexLocal(locals, pid);
    {
        var x = l.@private; if (x != default!) {
            l.@private = default!;
            return x;
        }
    }
    for (nint i = 0; i < ((nint)size); i++) {
        var lΔ2 = indexLocal(locals, (pid + i) % ((nint)size));
        {
            var (x, _) = lΔ2.shared.popTail(); if (x != default!) {
                return x;
            }
        }
    }
    // Mark the victim cache as empty for future gets don't bother
    // with it.
    atomic.StoreUintptr(Ꮡ(p.victimSize), 0);
    return default!;
}

// pin pins the current goroutine to P, disables preemption and
// returns poolLocal pool for the P and the P's id.
// Caller must call runtime_procUnpin() when done with the pool.
[GoRecv] internal static (ж<poolLocal>, nint) pin(this ref Pool p) {
    // Check whether p is nil to get a panic.
    // Otherwise the nil dereference happens while the m is pinned,
    // causing a fatal error rather than a panic.
    if (p == nil) {
        throw panic("nil Pool");
    }
    nint pid = runtime_procPin();
    // In pinSlow we store to local and then to localSize, here we load in opposite order.
    // Since we've disabled preemption, GC cannot happen in between.
    // Thus here we must observe local at least as large localSize.
    // We can observe a newer/larger local, it is fine (we must observe its zero-initialized-ness).
    var s = runtime_LoadAcquintptr(Ꮡ(p.localSize));
    // load-acquire
    @unsafe.Pointer l = p.local;
    // load-consume
    if (((uintptr)pid) < s) {
        return (indexLocal(l, pid), pid);
    }
    return p.pinSlow();
}

[GoRecv] internal static (ж<poolLocal>, nint) pinSlow(this ref Pool p) => func((defer, _) => {
    // Retry under the mutex.
    // Can not lock the mutex while pinned.
    runtime_procUnpin();
    allPoolsMu.Lock();
    var allPoolsMuʗ1 = allPoolsMu;
    defer(allPoolsMuʗ1.Unlock);
    nint pid = runtime_procPin();
    // poolCleanup won't be called while we are pinned.
    var s = p.localSize;
    @unsafe.Pointer l = p.local;
    if (((uintptr)pid) < s) {
        return (indexLocal(l, pid), pid);
    }
    if (p.local == nil) {
        allPools = append(allPools, p);
    }
    // If GOMAXPROCS changes between GCs, we re-allocate the array and lose the old one.
    nint size = runtime.GOMAXPROCS(0);
    var local = new slice<poolLocal>(size);
    atomic.StorePointer(Ꮡ(p.local), new @unsafe.Pointer(Ꮡ(local, 0)));
    // store-release
    runtime_StoreReluintptr(Ꮡ(p.localSize), ((uintptr)size));
    // store-release
    return (Ꮡ(local, pid), pid);
});

// poolCleanup should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/gopkg
//   - github.com/songzhibin97/gkit
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname poolCleanup
internal static void poolCleanup() {
    // This function is called with the world stopped, at the beginning of a garbage collection.
    // It must not allocate and probably should not call any runtime functions.
    // Because the world is stopped, no pool user can be in a
    // pinned section (in effect, this has all Ps pinned).
    // Drop victim caches from all pools.
    foreach (var (_, p) in oldPools) {
        p.val.victim = default!;
        p.val.victimSize = 0;
    }
    // Move primary cache to victim cache.
    foreach (var (_, p) in allPools) {
        p.val.victim = p.val.local;
        p.val.victimSize = p.val.localSize;
        p.val.local = default!;
        p.val.localSize = 0;
    }
    // The pools with non-empty primary caches now have non-empty
    // victim caches and no pools have primary caches.
    (oldPools, allPools) = (allPools, default!);
}

internal static Mutex allPoolsMu;
internal static slice<ж<Pool>> allPools;
internal static slice<ж<Pool>> oldPools;

[GoInit] internal static void initΔ1() {
    runtime_registerPoolCleanup(poolCleanup);
}

internal static ж<poolLocal> indexLocal(@unsafe.Pointer l, nint i) {
    @unsafe.Pointer lp = ((@unsafe.Pointer)(((uintptr)l) + ((uintptr)i) * @unsafe.Sizeof(new poolLocal(nil))));
    return (ж<poolLocal>)(uintptr)(lp);
}

// Implemented in runtime.
internal static partial void runtime_registerPoolCleanup(Action cleanup);

internal static partial nint runtime_procPin();

internal static partial void runtime_procUnpin();

// The below are implemented in internal/runtime/atomic and the
// compiler also knows to intrinsify the symbol we linkname into this
// package.

//go:linkname runtime_LoadAcquintptr internal/runtime/atomic.LoadAcquintptr
internal static partial uintptr runtime_LoadAcquintptr(ж<uintptr> ptr);

//go:linkname runtime_StoreReluintptr internal/runtime/atomic.StoreReluintptr
internal static partial uintptr runtime_StoreReluintptr(ж<uintptr> ptr, uintptr val);

} // end sync_package
