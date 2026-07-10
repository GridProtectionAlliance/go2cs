// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using concurrent = @internal.concurrent_package;
using weak = @internal.weak_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using @internal;

partial class unique_package {

// Handle is a globally unique identity for some value of type T.
//
// Two handles compare equal exactly if the two values used to create the handles
// would have also compared equal. The comparison of two handles is trivial and
// typically much more efficient than comparing the values used to create them.
[GoType] partial struct Handle<T>
    where T : /* comparable */ new()
{
    internal ж<T> value;
}

// Value returns a shallow copy of the T value that produced the Handle.
public static T Value<T>(this Handle<T> h)
    where T : /* comparable */ new()
{
    return h.value.ValueSlot;
}

// Make returns a globally unique handle for a value of type T. Handles
// are equal if and only if the values used to produce them are equal.
public static Handle<T> Make<T>(T value)
    where T : /* comparable */ new()
{
    // Find the map for type T.
    var typ = abi.TypeFor<T>();
    var (ma, ok) = uniqueMaps.Load(typ);
    if (!ok) {
        // This is a good time to initialize cleanup, since we must go through
        // this path on the first use of Make, and it's not on the hot path.
        ᏑsetupMake.Do(registerCleanup);
        ma = addUniqueMap<T>(typ);
    }
    var m = ma._<ж<uniqueMap<T>>>();
    // Keep around any values we allocate for insertion. There
    // are a few different ways we can race with other threads
    // and create values that we might discard. By keeping
    // the first one we make around, we can avoid generating
    // more than one per racing thread.
    ref var toInsert = ref heap<ж<T>>(out var ᏑtoInsert);  // Keep this around to keep it alive.
    
    ref var toInsertWeak = ref heap(new weak.Pointer<T>(), out var ᏑtoInsertWeak);
    var mʗ1 = m;
    var newValue = () => {
        if (ᏑtoInsert.ValueSlot == nil) {
            ᏑtoInsert.ValueSlot = @new<T>();
            ᏑtoInsert.ValueSlot.ValueSlot = clone(value, mʗ1.of(uniqueMap<T>.ᏑcloneSeq));
            ᏑtoInsertWeak.Value = weak.Make<T>(ᏑtoInsert.ValueSlot);
        }
        return ᏑtoInsertWeak.Value;
    };
    ж<T> ptr = default!;
    while (ᐧ) {
        // Check the map.
        var (wp, okΔ1) = m.Value.HashTrieMap.Value.Load(value);
        if (!okΔ1) {
            // Try to insert a new value into the map.
            (wp, _) = m.Value.HashTrieMap.LoadOrStore(value, newValue());
        }
        // Now that we're sure there's a value in the map, let's
        // try to get the pointer we need out of it.
        ptr = wp.Strong();
        if (ptr != nil) {
            break;
        }
        // The weak pointer is nil, so the old value is truly dead.
        // Try to remove it and start over.
        m.Value.HashTrieMap.Value.CompareAndDelete(value, wp);
    }
    Δruntime.KeepAlive(toInsert);
    return new Handle<T>(ptr);
}

internal static ж<concurrent.HashTrieMap<ж<abi.Type>, any>> uniqueMaps = concurrent.NewHashTrieMap<ж<abi.Type>, any>();                             // any is always a *uniqueMap[T].
internal static ж<Δsync.Mutex> ᏑcleanupMu = new(default(Δsync.Mutex));
internal static ref Δsync.Mutex cleanupMu => ref ᏑcleanupMu.Value;
internal static ж<Δsync.Mutex> ᏑcleanupFuncsMu = new(default(Δsync.Mutex));
internal static ref Δsync.Mutex cleanupFuncsMu => ref ᏑcleanupFuncsMu.Value;
internal static slice<Action> cleanupFuncs;
internal static slice<Action> cleanupNotify; // One-time notifications when cleanups finish.

[GoType] partial struct uniqueMap<T>
    where T : /* comparable */ new()
{
    public partial ref ж<@internal.concurrent_package.HashTrieMap<T, @internal.weak_package.Pointer<T>>> HashTrieMap { get; }
    internal partial ref cloneSeq cloneSeq { get; }
}

internal static ж<uniqueMap<T>> addUniqueMap<T>(ж<abi.Type> Ꮡtyp)
    where T : /* comparable */ new()
{
    ref var typ = ref Ꮡtyp.Value;

    // Create a map for T and try to register it. We could
    // race with someone else, but that's fine; it's one
    // small, stray allocation. The number of allocations
    // this can create is bounded by a small constant.
    var m = Ꮡ(new uniqueMap<T>(
        HashTrieMap: concurrent.NewHashTrieMap<T, weak.Pointer<T>>(),
        cloneSeq: makeCloneSeq(Ꮡtyp)
    ));
    var (a, loaded) = uniqueMaps.LoadOrStore(Ꮡtyp, m);
    if (!loaded) {
        // Add a cleanup function for the new map.
        ᏑcleanupFuncsMu.Lock();
        var mʗ1 = m;
        cleanupFuncs = append(cleanupFuncs, () => {
            // Delete all the entries whose weak references are nil and clean up
            // deleted entries.
            var mʗ2 = mʗ1;
            mʗ1.Value.HashTrieMap.All()((T key, weak.Pointer<T> wp) => {
                if (wp.Strong() == nil) {
                    mʗ2.Value.HashTrieMap.Value.CompareAndDelete(key, wp);
                }
                return true;
            });
        });
        ᏑcleanupFuncsMu.Unlock();
    }
    return a._<ж<uniqueMap<T>>>();
}

// setupMake is used to perform initial setup for unique.Make.
internal static ж<Δsync.Once> ᏑsetupMake = new(default(Δsync.Once));
internal static ref Δsync.Once setupMake => ref ᏑsetupMake.Value;

// startBackgroundCleanup sets up a background goroutine to occasionally call cleanupFuncs.
internal static void registerCleanup() {
    runtime_registerUniqueMapCleanup(() => {
        // Lock for cleanup.
        ᏑcleanupMu.Lock();
        // Grab funcs to run.
        ᏑcleanupFuncsMu.Lock();
        var cf = cleanupFuncs;
        ᏑcleanupFuncsMu.Unlock();
        // Run cleanup.
        foreach (var (_, f) in cf) {
            f();
        }
        // Run cleanup notifications.
        foreach (var (_, f) in cleanupNotify) {
            f();
        }
        cleanupNotify = default!;
        // Finished.
        ᏑcleanupMu.Unlock();
    });
}

// Implemented in runtime.

//go:linkname runtime_registerUniqueMapCleanup
internal static partial void runtime_registerUniqueMapCleanup(Action cleanup);

} // end unique_package
