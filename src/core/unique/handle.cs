// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using concurrent = @internal.concurrent_package;
using weak = @internal.weak_package;
using runtime = runtime_package;
using sync = sync_package;
using _ = unsafe_package;
using @internal;

partial class unique_package {

// Handle is a globally unique identity for some value of type T.
//
// Two handles compare equal exactly if the two values used to create the handles
// would have also compared equal. The comparison of two handles is trivial and
// typically much more efficient than comparing the values used to create them.
[GoType] partial struct Handle<T>
    where T : /* comparable */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    internal ж<T> value;
}

// Value returns a shallow copy of the T value that produced the Handle.
public static T Value<T>(this Handle<T> h)
    where T : /* comparable */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    return h.value.val;
}

// Make returns a globally unique handle for a value of type T. Handles
// are equal if and only if the values used to produce them are equal.
public static Handle<T> Make<T>(T value)
    where T : /* comparable */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    // Find the map for type T.
    var typ = abi.TypeFor<T>();
    var (ma, ok) = uniqueMaps.Load(typ);
    if (!ok) {
        // This is a good time to initialize cleanup, since we must go through
        // this path on the first use of Make, and it's not on the hot path.
        setupMake.Do(registerCleanup);
        ma = addUniqueMap<T>(typ);
    }
    var m = ma._<uniqueMap<T>.val>();
    // Keep around any values we allocate for insertion. There
    // are a few different ways we can race with other threads
    // and create values that we might discard. By keeping
    // the first one we make around, we can avoid generating
    // more than one per racing thread.
    ж<T> toInsert = default!;  // Keep this around to keep it alive.
    
    ref var toInsertWeak = ref heap(new @internal.weak_package.Pointer(), out var ᏑtoInsertWeak);
    var newValue = 
    var mʗ1 = m;
    var toInsertʗ1 = toInsert;
    var toInsertWeakʗ1 = toInsertWeak;
    () => {
        if (toInsertʗ1 == nil) {
            toInsertʗ1 = @new<T>();
            toInsertʗ1.val = clone(value, Ꮡ((~mʗ1).cloneSeq));
            toInsertWeakʗ1 = weak.Make<T>(toInsertʗ1);
        }
        return (toInsertʗ1.val, toInsertWeakʗ1);
    };
    ж<T> ptr = default!;
    while (ᐧ) {
        // Check the map.
        var (wp, okΔ1) = m.Load(value);
        if (!okΔ1) {
            // Try to insert a new value into the map.
            var (k, v) = newValue();
            (wp, _) = m.LoadOrStore(k, v);
        }
        // Now that we're sure there's a value in the map, let's
        // try to get the pointer we need out of it.
        ptr = wp.Strong();
        if (ptr != nil) {
            break;
        }
        // The weak pointer is nil, so the old value is truly dead.
        // Try to remove it and start over.
        m.CompareAndDelete(value, wp);
    }
    runtime.KeepAlive(toInsert);
    return new Handle<T>(ptr);
}

internal static ж<concurrent.abi.Type, any>> uniqueMaps = concurrent.NewHashTrieMap<ж<abi.Type>, any>();             // any is always a *uniqueMap[T].
internal static sync.Mutex cleanupMu;
internal static sync.Mutex cleanupFuncsMu;
internal static slice<Action> cleanupFuncs;
internal static slice<Action> cleanupNotify; // One-time notifications when cleanups finish.

[GoType] partial struct uniqueMap<T>
    where T : /* comparable */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    internal partial ref cloneSeq cloneSeq { get; }
}

internal static ж<uniqueMap<T>> addUniqueMap<T>(ж<abi.Type> Ꮡtyp)
    where T : /* comparable */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    ref var typ = ref Ꮡtyp.val;

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
        cleanupFuncsMu.Lock();
        cleanupFuncs = append(cleanupFuncs, 
        var mʗ1 = m;
        () => {
            // Delete all the entries whose weak references are nil and clean up
            // deleted entries.
            mʗ1.All()(
            var mʗ3 = m;
            (T key, weak.Pointer<T> wp) => {
                if (wp.Strong() == nil) {
                    mʗ3.CompareAndDelete(key, wp);
                }
                return true;
            });
        });
        cleanupFuncsMu.Unlock();
    }
    return a._<uniqueMap<T>.val>();
}

// setupMake is used to perform initial setup for unique.Make.
internal static sync.Once setupMake;

// startBackgroundCleanup sets up a background goroutine to occasionally call cleanupFuncs.
internal static void registerCleanup() {
    runtime_registerUniqueMapCleanup(
    var cleanupFuncsʗ2 = cleanupFuncs;
    var cleanupFuncsMuʗ2 = cleanupFuncsMu;
    var cleanupMuʗ2 = cleanupMu;
    var cleanupNotifyʗ2 = cleanupNotify;
    () => {
        cleanupMuʗ2.Lock();
        cleanupFuncsMuʗ2.Lock();
        var cf = cleanupFuncsʗ2;
        cleanupFuncsMuʗ2.Unlock();
        foreach (var (_, f) in cf) {
            f();
        }
        foreach (var (_, f) in cleanupNotifyʗ2) {
            f();
        }
        cleanupNotifyʗ2 = default!;
        cleanupMuʗ2.Unlock();
    });
}

// Implemented in runtime.

//go:linkname runtime_registerUniqueMapCleanup
internal static partial void runtime_registerUniqueMapCleanup(Action cleanup);

} // end unique_package
