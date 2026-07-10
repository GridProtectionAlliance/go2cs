// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = sync.atomic_package;
using sync;

partial class sync_package {

// Map is like a Go map[any]any but is safe for concurrent use
// by multiple goroutines without additional locking or coordination.
// Loads, stores, and deletes run in amortized constant time.
//
// The Map type is specialized. Most code should use a plain Go map instead,
// with separate locking or coordination, for better type safety and to make it
// easier to maintain other invariants along with the map content.
//
// The Map type is optimized for two common use cases: (1) when the entry for a given
// key is only ever written once but read many times, as in caches that only grow,
// or (2) when multiple goroutines read, write, and overwrite entries for disjoint
// sets of keys. In these two cases, use of a Map may significantly reduce lock
// contention compared to a Go map paired with a separate [Mutex] or [RWMutex].
//
// The zero Map is empty and ready for use. A Map must not be copied after first use.
//
// In the terminology of [the Go memory model], Map arranges that a write operation
// “synchronizes before” any read operation that observes the effect of the write, where
// read and write operations are defined as follows.
// [Map.Load], [Map.LoadAndDelete], [Map.LoadOrStore], [Map.Swap], [Map.CompareAndSwap],
// and [Map.CompareAndDelete] are read operations;
// [Map.Delete], [Map.LoadAndDelete], [Map.Store], and [Map.Swap] are write operations;
// [Map.LoadOrStore] is a write operation when it returns loaded set to false;
// [Map.CompareAndSwap] is a write operation when it returns swapped set to true;
// and [Map.CompareAndDelete] is a write operation when it returns deleted set to true.
//
// [the Go memory model]: https://go.dev/ref/mem
[GoType] partial struct Map {
    internal Mutex mu;
    // read contains the portion of the map's contents that are safe for
    // concurrent access (with or without mu held).
    //
    // The read field itself is always safe to load, but must only be stored with
    // mu held.
    //
    // Entries stored in read may be updated concurrently without mu, but updating
    // a previously-expunged entry requires that the entry be copied to the dirty
    // map and unexpunged with mu held.
    internal atomic.Pointer<readOnly> read;
    // dirty contains the portion of the map's contents that require mu to be
    // held. To ensure that the dirty map can be promoted to the read map quickly,
    // it also includes all of the non-expunged entries in the read map.
    //
    // Expunged entries are not stored in the dirty map. An expunged entry in the
    // clean map must be unexpunged and added to the dirty map before a new value
    // can be stored to it.
    //
    // If the dirty map is nil, the next write to the map will initialize it by
    // making a shallow copy of the clean map, omitting stale entries.
    internal map<any, ж<entry>> dirty;
    // misses counts the number of loads since the read map was last updated that
    // needed to lock mu to determine whether the key was present.
    //
    // Once enough misses have occurred to cover the cost of copying the dirty
    // map, the dirty map will be promoted to the read map (in the unamended
    // state) and the next store to the map will make a new dirty copy.
    internal nint misses;
}

// readOnly is an immutable struct stored atomically in the Map.read field.
[GoType] partial struct readOnly {
    internal map<any, ж<entry>> m;
    internal bool amended; // true if the dirty map contains some key not in m.
}

// expunged is an arbitrary pointer that marks entries which have been deleted
// from the dirty map.
internal static ж<any> expunged = @new<any>();

// An entry is a slot in the map corresponding to a particular key.
[GoType] partial struct entry {
    // p points to the interface{} value stored for the entry.
    //
    // If p == nil, the entry has been deleted, and either m.dirty == nil or
    // m.dirty[key] is e.
    //
    // If p == expunged, the entry has been deleted, m.dirty != nil, and the entry
    // is missing from m.dirty.
    //
    // Otherwise, the entry is valid and recorded in m.read.m[key] and, if m.dirty
    // != nil, in m.dirty[key].
    //
    // An entry can be deleted by atomic replacement with nil: when m.dirty is
    // next created, it will atomically replace nil with expunged and leave
    // m.dirty[key] unset.
    //
    // An entry's associated value can be updated by atomic replacement, provided
    // p != expunged. If p == expunged, an entry's associated value can be updated
    // only after first setting m.dirty[key] = e so that lookups using the dirty
    // map find the entry.
    internal atomic.Pointer<any> p;
}

internal static ж<entry> newEntry(any i) {
    var e = Ꮡ(new entry(nil));
    e.of(entry.Ꮡp).Store(Ꮡ(i));
    return e;
}

internal static readOnly loadReadOnly(this ж<Map> Ꮡm) {
    ref var m = ref Ꮡm.Value;

    {
        var p = Ꮡm.of(Map.Ꮡread).Load(); if (p != nil) {
            return p.Value;
        }
    }
    return new readOnly(nil);
}

// Load returns the value stored in the map for a key, or nil if no
// value is present.
// The ok result indicates whether value was found in the map.
public static (any value, bool ok) Load(this ж<Map> Ꮡm, any key) {
    any value = default!;
    bool ok = default!;

    ref var m = ref Ꮡm.Value;
    var read = Ꮡm.loadReadOnly();
    (var e, ok) = read.m[key, ꟷ];
    if (!ok && read.amended) {
        Ꮡm.of(Map.Ꮡmu).Lock();
        // Avoid reporting a spurious miss if m.dirty got promoted while we were
        // blocked on m.mu. (If further loads of the same key will not miss, it's
        // not worth copying the dirty map for this key.)
        read = Ꮡm.loadReadOnly();
        (e, ok) = read.m[key, ꟷ];
        if (!ok && read.amended) {
            (e, ok) = m.dirty[key, ꟷ];
            // Regardless of whether the entry was present, record a miss: this key
            // will take the slow path until the dirty map is promoted to the read
            // map.
            Ꮡm.missLocked();
        }
        Ꮡm.of(Map.Ꮡmu).Unlock();
    }
    if (!ok) {
        return (default!, false);
    }
    return e.load();
}

internal static (any value, bool ok) load(this ж<entry> Ꮡe) {
    any value = default!;
    bool ok = default!;

    ref var e = ref Ꮡe.Value;
    var p = Ꮡe.of(entry.Ꮡp).Load();
    if (p == nil || p == expunged) {
        return (default!, false);
    }
    return (p.ValueSlot, true);
}

// Store sets the value for a key.
public static void Store(this ж<Map> Ꮡm, any key, any value) {
    ref var m = ref Ꮡm.Value;

    (_, _) = Ꮡm.Swap(key, value);
}

// Clear deletes all the entries, resulting in an empty Map.
public static void Clear(this ж<Map> Ꮡm) => func((defer, recover) => {
    ref var m = ref Ꮡm.Value;

    var read = Ꮡm.loadReadOnly();
    if (len(read.m) == 0 && !read.amended) {
        // Avoid allocating a new readOnly when the map is already clear.
        return;
    }
    Ꮡm.of(Map.Ꮡmu).Lock();
    defer(Ꮡm.of(Map.Ꮡmu).Unlock);
    read = Ꮡm.loadReadOnly();
    if (len(read.m) > 0 || read.amended) {
        Ꮡm.of(Map.Ꮡread).Store(Ꮡ(new readOnly(nil)));
    }
    clear(m.dirty);
    // Don't immediately promote the newly-cleared dirty map on the next operation.
    m.misses = 0;
});

// tryCompareAndSwap compare the entry with the given old value and swaps
// it with a new value if the entry is equal to the old value, and the entry
// has not been expunged.
//
// If the entry is expunged, tryCompareAndSwap returns false and leaves
// the entry unchanged.
internal static bool tryCompareAndSwap(this ж<entry> Ꮡe, any old, any @new) {
    ref var e = ref Ꮡe.Value;

    var p = Ꮡe.of(entry.Ꮡp).Load();
    if (p == nil || p == expunged || !AreEqual(p.ValueSlot, old)) {
        return false;
    }
    // Copy the interface after the first load to make this method more amenable
    // to escape analysis: if the comparison fails from the start, we shouldn't
    // bother heap-allocating an interface value to store.
    ref var nc = ref heap<any>(out var Ꮡnc);
    nc = @new;
    while (ᐧ) {
        if (Ꮡe.of(entry.Ꮡp).CompareAndSwap(p, Ꮡnc)) {
            return true;
        }
        p = Ꮡe.of(entry.Ꮡp).Load();
        if (p == nil || p == expunged || !AreEqual(p.ValueSlot, old)) {
            return false;
        }
    }
}

// unexpungeLocked ensures that the entry is not marked as expunged.
//
// If the entry was previously expunged, it must be added to the dirty map
// before m.mu is unlocked.
internal static bool /*wasExpunged*/ unexpungeLocked(this ж<entry> Ꮡe) {
    bool wasExpunged = default!;

    ref var e = ref Ꮡe.Value;
    return Ꮡe.of(entry.Ꮡp).CompareAndSwap(expunged, nil);
}

// swapLocked unconditionally swaps a value into the entry.
//
// The entry must be known not to be expunged.
internal static ж<any> swapLocked(this ж<entry> Ꮡe, ж<any> Ꮡi) {
    ref var e = ref Ꮡe.Value;
    ref var i = ref Ꮡi.Value;

    return Ꮡe.of(entry.Ꮡp).Swap(Ꮡi);
}

// LoadOrStore returns the existing value for the key if present.
// Otherwise, it stores and returns the given value.
// The loaded result is true if the value was loaded, false if stored.
public static (any actual, bool loaded) LoadOrStore(this ж<Map> Ꮡm, any key, any value) {
    any actual = default!;
    bool loaded = default!;

    ref var m = ref Ꮡm.Value;
    // Avoid locking if it's a clean hit.
    var read = Ꮡm.loadReadOnly();
    {
        var (e, ok) = read.m[key, ꟷ]; if (ok) {
            var (actualΔ1, loadedΔ1, okΔ1) = e.tryLoadOrStore(value);
            if (okΔ1) {
                return (actualΔ1, loadedΔ1);
            }
        }
    }
    Ꮡm.of(Map.Ꮡmu).Lock();
    read = Ꮡm.loadReadOnly();
    {
        var (e, ok) = read.m[key, ꟷ]; if (ok){
            if (e.unexpungeLocked()) {
                m.dirty[key] = e;
            }
            (actual, loaded, _) = e.tryLoadOrStore(value);
        } else 
        {
            var (eΔ1, okΔ1) = m.dirty[key, ꟷ]; if (okΔ1){
                (actual, loaded, _) = eΔ1.tryLoadOrStore(value);
                Ꮡm.missLocked();
            } else {
                if (!read.amended) {
                    // We're adding the first new key to the dirty map.
                    // Make sure it is allocated and mark the read-only map as incomplete.
                    Ꮡm.dirtyLocked();
                    Ꮡm.of(Map.Ꮡread).Store(Ꮡ(new readOnly(m: read.m, amended: true)));
                }
                m.dirty[key] = newEntry(value);
                (actual, loaded) = (value, false);
            }
        }
    }
    Ꮡm.of(Map.Ꮡmu).Unlock();
    return (actual, loaded);
}

// tryLoadOrStore atomically loads or stores a value if the entry is not
// expunged.
//
// If the entry is expunged, tryLoadOrStore leaves the entry unchanged and
// returns with ok==false.
internal static (any actual, bool loaded, bool ok) tryLoadOrStore(this ж<entry> Ꮡe, any i) {
    any actual = default!;
    bool loaded = default!;
    bool ok = default!;

    ref var e = ref Ꮡe.Value;
    var p = Ꮡe.of(entry.Ꮡp).Load();
    if (p == expunged) {
        return (default!, false, false);
    }
    if (p != nil) {
        return (p.ValueSlot, true, true);
    }
    // Copy the interface after the first load to make this method more amenable
    // to escape analysis: if we hit the "load" path or the entry is expunged, we
    // shouldn't bother heap-allocating.
    ref var ic = ref heap<any>(out var Ꮡic);
    ic = i;
    while (ᐧ) {
        if (Ꮡe.of(entry.Ꮡp).CompareAndSwap(nil, Ꮡic)) {
            return (i, false, true);
        }
        p = Ꮡe.of(entry.Ꮡp).Load();
        if (p == expunged) {
            return (default!, false, false);
        }
        if (p != nil) {
            return (p.ValueSlot, true, true);
        }
    }
}

// LoadAndDelete deletes the value for a key, returning the previous value if any.
// The loaded result reports whether the key was present.
public static (any value, bool loaded) LoadAndDelete(this ж<Map> Ꮡm, any key) {
    any value = default!;
    bool loaded = default!;

    ref var m = ref Ꮡm.Value;
    var read = Ꮡm.loadReadOnly();
    var (e, ok) = read.m[key, ꟷ];
    if (!ok && read.amended) {
        Ꮡm.of(Map.Ꮡmu).Lock();
        read = Ꮡm.loadReadOnly();
        (e, ok) = read.m[key, ꟷ];
        if (!ok && read.amended) {
            (e, ok) = m.dirty[key, ꟷ];
            builtin.delete(m.dirty, key);
            // Regardless of whether the entry was present, record a miss: this key
            // will take the slow path until the dirty map is promoted to the read
            // map.
            Ꮡm.missLocked();
        }
        Ꮡm.of(Map.Ꮡmu).Unlock();
    }
    if (ok) {
        return e.delete();
    }
    return (default!, false);
}

// Delete deletes the value for a key.
public static void Delete(this ж<Map> Ꮡm, any key) {
    ref var m = ref Ꮡm.Value;

    Ꮡm.LoadAndDelete(key);
}

internal static (any value, bool ok) delete(this ж<entry> Ꮡe) {
    any value = default!;
    bool ok = default!;

    ref var e = ref Ꮡe.Value;
    while (ᐧ) {
        var p = Ꮡe.of(entry.Ꮡp).Load();
        if (p == nil || p == expunged) {
            return (default!, false);
        }
        if (Ꮡe.of(entry.Ꮡp).CompareAndSwap(p, nil)) {
            return (p.ValueSlot, true);
        }
    }
}

// trySwap swaps a value if the entry has not been expunged.
//
// If the entry is expunged, trySwap returns false and leaves the entry
// unchanged.
internal static (ж<any>, bool) trySwap(this ж<entry> Ꮡe, ж<any> Ꮡi) {
    ref var e = ref Ꮡe.Value;
    ref var i = ref Ꮡi.Value;

    while (ᐧ) {
        var p = Ꮡe.of(entry.Ꮡp).Load();
        if (p == expunged) {
            return (default!, false);
        }
        if (Ꮡe.of(entry.Ꮡp).CompareAndSwap(p, Ꮡi)) {
            return (p, true);
        }
    }
}

// Swap swaps the value for a key and returns the previous value if any.
// The loaded result reports whether the key was present.
public static (any previous, bool loaded) Swap(this ж<Map> Ꮡm, any key, any value) {
    any previous = default!;
    bool loaded = default!;

    ref var m = ref Ꮡm.Value;
    var read = Ꮡm.loadReadOnly();
    {
        var (e, ok) = read.m[key, ꟷ]; if (ok) {
            {
                var (v, okΔ1) = e.trySwap(Ꮡ(value)); if (okΔ1) {
                    if (v == nil) {
                        return (default!, false);
                    }
                    return (v.ValueSlot, true);
                }
            }
        }
    }
    Ꮡm.of(Map.Ꮡmu).Lock();
    read = Ꮡm.loadReadOnly();
    {
        var (e, ok) = read.m[key, ꟷ]; if (ok){
            if (e.unexpungeLocked()) {
                // The entry was previously expunged, which implies that there is a
                // non-nil dirty map and this entry is not in it.
                m.dirty[key] = e;
            }
            {
                var v = e.swapLocked(Ꮡ(value)); if (v != nil) {
                    loaded = true;
                    previous = v.ValueSlot;
                }
            }
        } else 
        {
            var (eΔ1, okΔ1) = m.dirty[key, ꟷ]; if (okΔ1){
                {
                    var v = eΔ1.swapLocked(Ꮡ(value)); if (v != nil) {
                        loaded = true;
                        previous = v.ValueSlot;
                    }
                }
            } else {
                if (!read.amended) {
                    // We're adding the first new key to the dirty map.
                    // Make sure it is allocated and mark the read-only map as incomplete.
                    Ꮡm.dirtyLocked();
                    Ꮡm.of(Map.Ꮡread).Store(Ꮡ(new readOnly(m: read.m, amended: true)));
                }
                m.dirty[key] = newEntry(value);
            }
        }
    }
    Ꮡm.of(Map.Ꮡmu).Unlock();
    return (previous, loaded);
}

// CompareAndSwap swaps the old and new values for key
// if the value stored in the map is equal to old.
// The old value must be of a comparable type.
public static bool /*swapped*/ CompareAndSwap(this ж<Map> Ꮡm, any key, any old, any @new) {
    bool swapped = default!;
    func((defer, recover) => {
    ref var m = ref Ꮡm.Value;

        var read = Ꮡm.loadReadOnly();
        {
            var (e, ok) = read.m[key, ꟷ]; if (ok){
                swapped = e.tryCompareAndSwap(old, @new); return;
            } else 
            if (!read.amended) {
                swapped = false; return;
            }
        }
        // No existing value for key.
        Ꮡm.of(Map.Ꮡmu).Lock();
        defer(Ꮡm.of(Map.Ꮡmu).Unlock);
        read = Ꮡm.loadReadOnly();
        swapped = false;
        {
            var (e, ok) = read.m[key, ꟷ]; if (ok){
                swapped = e.tryCompareAndSwap(old, @new);
            } else 
            {
                var (eΔ1, okΔ1) = m.dirty[key, ꟷ]; if (okΔ1) {
                    swapped = eΔ1.tryCompareAndSwap(old, @new);
                    // We needed to lock mu in order to load the entry for key,
                    // and the operation didn't change the set of keys in the map
                    // (so it would be made more efficient by promoting the dirty
                    // map to read-only).
                    // Count it as a miss so that we will eventually switch to the
                    // more efficient steady state.
                    Ꮡm.missLocked();
                }
            }
        }
    });
    return swapped;
}

// CompareAndDelete deletes the entry for key if its value is equal to old.
// The old value must be of a comparable type.
//
// If there is no current value for key in the map, CompareAndDelete
// returns false (even if the old value is the nil interface value).
public static bool /*deleted*/ CompareAndDelete(this ж<Map> Ꮡm, any key, any old) {
    bool deleted = default!;

    ref var m = ref Ꮡm.Value;
    var read = Ꮡm.loadReadOnly();
    var (e, ok) = read.m[key, ꟷ];
    if (!ok && read.amended) {
        Ꮡm.of(Map.Ꮡmu).Lock();
        read = Ꮡm.loadReadOnly();
        (e, ok) = read.m[key, ꟷ];
        if (!ok && read.amended) {
            (e, ok) = m.dirty[key, ꟷ];
            // Don't delete key from m.dirty: we still need to do the “compare” part
            // of the operation. The entry will eventually be expunged when the
            // dirty map is promoted to the read map.
            //
            // Regardless of whether the entry was present, record a miss: this key
            // will take the slow path until the dirty map is promoted to the read
            // map.
            Ꮡm.missLocked();
        }
        Ꮡm.of(Map.Ꮡmu).Unlock();
    }
    while (ok) {
        var p = e.of(entry.Ꮡp).Load();
        if (p == nil || p == expunged || !AreEqual(p.ValueSlot, old)) {
            return false;
        }
        if (e.of(entry.Ꮡp).CompareAndSwap(p, nil)) {
            return true;
        }
    }
    return false;
}

// Range calls f sequentially for each key and value present in the map.
// If f returns false, range stops the iteration.
//
// Range does not necessarily correspond to any consistent snapshot of the Map's
// contents: no key will be visited more than once, but if the value for any key
// is stored or deleted concurrently (including by f), Range may reflect any
// mapping for that key from any point during the Range call. Range does not
// block other methods on the receiver; even f itself may call any method on m.
//
// Range may be O(N) with the number of elements in the map even if f returns
// false after a constant number of calls.
public static void Range(this ж<Map> Ꮡm, Func<any, any, bool> f) {
    ref var m = ref Ꮡm.Value;

    // We need to be able to iterate over all of the keys that were already
    // present at the start of the call to Range.
    // If read.amended is false, then read.m satisfies that property without
    // requiring us to hold m.mu for a long time.
    var read = Ꮡm.loadReadOnly();
    if (read.amended) {
        // m.dirty contains keys not in read.m. Fortunately, Range is already O(N)
        // (assuming the caller does not break out early), so a call to Range
        // amortizes an entire copy of the map: we can promote the dirty copy
        // immediately!
        Ꮡm.of(Map.Ꮡmu).Lock();
        read = Ꮡm.loadReadOnly();
        if (read.amended) {
            read = new readOnly(m: m.dirty);
            ref var copyRead = ref heap<readOnly>(out var ᏑcopyRead);
            copyRead = read;
            Ꮡm.of(Map.Ꮡread).Store(ᏑcopyRead);
            m.dirty = default!;
            m.misses = 0;
        }
        Ꮡm.of(Map.Ꮡmu).Unlock();
    }
    foreach (var (k, e) in read.m) {
        var (v, ok) = e.load();
        if (!ok) {
            continue;
        }
        if (!f(k, v)) {
            break;
        }
    }
}

internal static void missLocked(this ж<Map> Ꮡm) {
    ref var m = ref Ꮡm.Value;

    m.misses++;
    if (m.misses < len(m.dirty)) {
        return;
    }
    Ꮡm.of(Map.Ꮡread).Store(Ꮡ(new readOnly(m: m.dirty)));
    m.dirty = default!;
    m.misses = 0;
}

internal static void dirtyLocked(this ж<Map> Ꮡm) {
    ref var m = ref Ꮡm.Value;

    if (m.dirty != default!) {
        return;
    }
    var read = Ꮡm.loadReadOnly();
    m.dirty = new map<any, ж<entry>>(len(read.m));
    foreach (var (k, e) in read.m) {
        if (!e.tryExpungeLocked()) {
            m.dirty[k] = e;
        }
    }
}

internal static bool /*isExpunged*/ tryExpungeLocked(this ж<entry> Ꮡe) {
    bool isExpunged = default!;

    ref var e = ref Ꮡe.Value;
    var p = Ꮡe.of(entry.Ꮡp).Load();
    while (p == nil) {
        if (Ꮡe.of(entry.Ꮡp).CompareAndSwap(nil, expunged)) {
            return true;
        }
        p = Ꮡe.of(entry.Ꮡp).Load();
    }
    return p == expunged;
}

} // end sync_package
