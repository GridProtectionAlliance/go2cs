// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sync -- go2cs converted at 2020 October 08 00:34:03 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Go\src\sync\map.go
using atomic = go.sync.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class sync_package
    {
        // Map is like a Go map[interface{}]interface{} but is safe for concurrent use
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
        // contention compared to a Go map paired with a separate Mutex or RWMutex.
        //
        // The zero Map is empty and ready for use. A Map must not be copied after first use.
        public partial struct Map
        {
            public Mutex mu; // read contains the portion of the map's contents that are safe for
// concurrent access (with or without mu held).
//
// The read field itself is always safe to load, but must only be stored with
// mu held.
//
// Entries stored in read may be updated concurrently without mu, but updating
// a previously-expunged entry requires that the entry be copied to the dirty
// map and unexpunged with mu held.
            public atomic.Value read; // readOnly

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
            public long misses;
        }

        // readOnly is an immutable struct stored atomically in the Map.read field.
        private partial struct readOnly
        {
            public bool amended; // true if the dirty map contains some key not in m.
        }

        // expunged is an arbitrary pointer that marks entries which have been deleted
        // from the dirty map.
        private static var expunged = @unsafe.Pointer(@new<>());

        // An entry is a slot in the map corresponding to a particular key.
        private partial struct entry
        {
            public unsafe.Pointer p; // *interface{}
        }

        private static ptr<entry> newEntry(object i)
        {
            return addr(new entry(p:unsafe.Pointer(&i)));
        }

        // Load returns the value stored in the map for a key, or nil if no
        // value is present.
        // The ok result indicates whether value was found in the map.
        private static (object, bool) Load(this ptr<Map> _addr_m, object key)
        {
            object value = default;
            bool ok = default;
            ref Map m = ref _addr_m.val;

            readOnly (read, _) = m.read.Load()._<readOnly>();
            var (e, ok) = read.m[key];
            if (!ok && read.amended)
            {
                m.mu.Lock(); 
                // Avoid reporting a spurious miss if m.dirty got promoted while we were
                // blocked on m.mu. (If further loads of the same key will not miss, it's
                // not worth copying the dirty map for this key.)
                read, _ = m.read.Load()._<readOnly>();
                e, ok = read.m[key];
                if (!ok && read.amended)
                {
                    e, ok = m.dirty[key]; 
                    // Regardless of whether the entry was present, record a miss: this key
                    // will take the slow path until the dirty map is promoted to the read
                    // map.
                    m.missLocked();

                }

                m.mu.Unlock();

            }

            if (!ok)
            {
                return (null, false);
            }

            return e.load();

        }

        private static (object, bool) load(this ptr<entry> _addr_e)
        {
            object value = default;
            bool ok = default;
            ref entry e = ref _addr_e.val;

            var p = atomic.LoadPointer(_addr_e.p);
            if (p == null || p == expunged)
            {
                return (null, false);
            }

            return true;

        }

        // Store sets the value for a key.
        private static void Store(this ptr<Map> _addr_m, object key, object value)
        {
            ref Map m = ref _addr_m.val;

            readOnly (read, _) = m.read.Load()._<readOnly>();
            {
                var e__prev1 = e;

                var (e, ok) = read.m[key];

                if (ok && e.tryStore(_addr_value))
                {
                    return ;
                }

                e = e__prev1;

            }


            m.mu.Lock();
            read, _ = m.read.Load()._<readOnly>();
            {
                var e__prev1 = e;

                (e, ok) = read.m[key];

                if (ok)
                {
                    if (e.unexpungeLocked())
                    { 
                        // The entry was previously expunged, which implies that there is a
                        // non-nil dirty map and this entry is not in it.
                        m.dirty[key] = e;

                    }

                    e.storeLocked(_addr_value);

                }                {
                    var e__prev2 = e;

                    (e, ok) = m.dirty[key];


                    else if (ok)
                    {
                        e.storeLocked(_addr_value);
                    }
                    else
                    {
                        if (!read.amended)
                        { 
                            // We're adding the first new key to the dirty map.
                            // Make sure it is allocated and mark the read-only map as incomplete.
                            m.dirtyLocked();
                            m.read.Store(new readOnly(m:read.m,amended:true));

                        }

                        m.dirty[key] = newEntry(value);

                    }

                    e = e__prev2;

                }


                e = e__prev1;

            }

            m.mu.Unlock();

        }

        // tryStore stores a value if the entry has not been expunged.
        //
        // If the entry is expunged, tryStore returns false and leaves the entry
        // unchanged.
        private static bool tryStore(this ptr<entry> _addr_e, object i)
        {
            ref entry e = ref _addr_e.val;

            while (true)
            {
                var p = atomic.LoadPointer(_addr_e.p);
                if (p == expunged)
                {
                    return false;
                }

                if (atomic.CompareAndSwapPointer(_addr_e.p, p, @unsafe.Pointer(i)))
                {
                    return true;
                }

            }


        }

        // unexpungeLocked ensures that the entry is not marked as expunged.
        //
        // If the entry was previously expunged, it must be added to the dirty map
        // before m.mu is unlocked.
        private static bool unexpungeLocked(this ptr<entry> _addr_e)
        {
            bool wasExpunged = default;
            ref entry e = ref _addr_e.val;

            return atomic.CompareAndSwapPointer(_addr_e.p, expunged, null);
        }

        // storeLocked unconditionally stores a value to the entry.
        //
        // The entry must be known not to be expunged.
        private static void storeLocked(this ptr<entry> _addr_e, object i)
        {
            ref entry e = ref _addr_e.val;

            atomic.StorePointer(_addr_e.p, @unsafe.Pointer(i));
        }

        // LoadOrStore returns the existing value for the key if present.
        // Otherwise, it stores and returns the given value.
        // The loaded result is true if the value was loaded, false if stored.
        private static (object, bool) LoadOrStore(this ptr<Map> _addr_m, object key, object value)
        {
            object actual = default;
            bool loaded = default;
            ref Map m = ref _addr_m.val;
 
            // Avoid locking if it's a clean hit.
            readOnly (read, _) = m.read.Load()._<readOnly>();
            {
                var e__prev1 = e;

                var (e, ok) = read.m[key];

                if (ok)
                {
                    var (actual, loaded, ok) = e.tryLoadOrStore(value);
                    if (ok)
                    {
                        return (actual, loaded);
                    }

                }

                e = e__prev1;

            }


            m.mu.Lock();
            read, _ = m.read.Load()._<readOnly>();
            {
                var e__prev1 = e;

                (e, ok) = read.m[key];

                if (ok)
                {
                    if (e.unexpungeLocked())
                    {
                        m.dirty[key] = e;
                    }

                    actual, loaded, _ = e.tryLoadOrStore(value);

                }                {
                    var e__prev2 = e;

                    (e, ok) = m.dirty[key];


                    else if (ok)
                    {
                        actual, loaded, _ = e.tryLoadOrStore(value);
                        m.missLocked();
                    }
                    else
                    {
                        if (!read.amended)
                        { 
                            // We're adding the first new key to the dirty map.
                            // Make sure it is allocated and mark the read-only map as incomplete.
                            m.dirtyLocked();
                            m.read.Store(new readOnly(m:read.m,amended:true));

                        }

                        m.dirty[key] = newEntry(value);
                        actual = value;
                        loaded = false;

                    }

                    e = e__prev2;

                }


                e = e__prev1;

            }

            m.mu.Unlock();

            return (actual, loaded);

        }

        // tryLoadOrStore atomically loads or stores a value if the entry is not
        // expunged.
        //
        // If the entry is expunged, tryLoadOrStore leaves the entry unchanged and
        // returns with ok==false.
        private static (object, bool, bool) tryLoadOrStore(this ptr<entry> _addr_e, object i)
        {
            object actual = default;
            bool loaded = default;
            bool ok = default;
            ref entry e = ref _addr_e.val;

            var p = atomic.LoadPointer(_addr_e.p);
            if (p == expunged)
            {
                return (null, false, false);
            }

            if (p != null)
            {
                return (true, true);
            } 

            // Copy the interface after the first load to make this method more amenable
            // to escape analysis: if we hit the "load" path or the entry is expunged, we
            // shouldn't bother heap-allocating.
            ref var ic = ref heap(i, out ptr<var> _addr_ic);
            while (true)
            {
                if (atomic.CompareAndSwapPointer(_addr_e.p, null, @unsafe.Pointer(_addr_ic)))
                {
                    return (i, false, true);
                }

                p = atomic.LoadPointer(_addr_e.p);
                if (p == expunged)
                {
                    return (null, false, false);
                }

                if (p != null)
                {
                    return (true, true);
                }

            }


        }

        // LoadAndDelete deletes the value for a key, returning the previous value if any.
        // The loaded result reports whether the key was present.
        private static (object, bool) LoadAndDelete(this ptr<Map> _addr_m, object key)
        {
            object value = default;
            bool loaded = default;
            ref Map m = ref _addr_m.val;

            readOnly (read, _) = m.read.Load()._<readOnly>();
            var (e, ok) = read.m[key];
            if (!ok && read.amended)
            {
                m.mu.Lock();
                read, _ = m.read.Load()._<readOnly>();
                e, ok = read.m[key];
                if (!ok && read.amended)
                {
                    e, ok = m.dirty[key];
                    delete(m.dirty, key); 
                    // Regardless of whether the entry was present, record a miss: this key
                    // will take the slow path until the dirty map is promoted to the read
                    // map.
                    m.missLocked();

                }

                m.mu.Unlock();

            }

            if (ok)
            {
                return e.delete();
            }

            return (null, false);

        }

        // Delete deletes the value for a key.
        private static void Delete(this ptr<Map> _addr_m, object key)
        {
            ref Map m = ref _addr_m.val;

            m.LoadAndDelete(key);
        }

        private static (object, bool) delete(this ptr<entry> _addr_e)
        {
            object value = default;
            bool ok = default;
            ref entry e = ref _addr_e.val;

            while (true)
            {
                var p = atomic.LoadPointer(_addr_e.p);
                if (p == null || p == expunged)
                {
                    return (null, false);
                }

                if (atomic.CompareAndSwapPointer(_addr_e.p, p, null))
                {
                    return true;
                }

            }


        }

        // Range calls f sequentially for each key and value present in the map.
        // If f returns false, range stops the iteration.
        //
        // Range does not necessarily correspond to any consistent snapshot of the Map's
        // contents: no key will be visited more than once, but if the value for any key
        // is stored or deleted concurrently, Range may reflect any mapping for that key
        // from any point during the Range call.
        //
        // Range may be O(N) with the number of elements in the map even if f returns
        // false after a constant number of calls.
        private static bool Range(this ptr<Map> _addr_m, Func<object, object, bool> f)
        {
            ref Map m = ref _addr_m.val;
 
            // We need to be able to iterate over all of the keys that were already
            // present at the start of the call to Range.
            // If read.amended is false, then read.m satisfies that property without
            // requiring us to hold m.mu for a long time.
            readOnly (read, _) = m.read.Load()._<readOnly>();
            if (read.amended)
            { 
                // m.dirty contains keys not in read.m. Fortunately, Range is already O(N)
                // (assuming the caller does not break out early), so a call to Range
                // amortizes an entire copy of the map: we can promote the dirty copy
                // immediately!
                m.mu.Lock();
                read, _ = m.read.Load()._<readOnly>();
                if (read.amended)
                {
                    read = new readOnly(m:m.dirty);
                    m.read.Store(read);
                    m.dirty = null;
                    m.misses = 0L;
                }

                m.mu.Unlock();

            }

            foreach (var (k, e) in read.m)
            {
                var (v, ok) = e.load();
                if (!ok)
                {
                    continue;
                }

                if (!f(k, v))
                {
                    break;
                }

            }

        }

        private static void missLocked(this ptr<Map> _addr_m)
        {
            ref Map m = ref _addr_m.val;

            m.misses++;
            if (m.misses < len(m.dirty))
            {
                return ;
            }

            m.read.Store(new readOnly(m:m.dirty));
            m.dirty = null;
            m.misses = 0L;

        }

        private static void dirtyLocked(this ptr<Map> _addr_m)
        {
            ref Map m = ref _addr_m.val;

            if (m.dirty != null)
            {
                return ;
            }

            readOnly (read, _) = m.read.Load()._<readOnly>();
            m.dirty = make(len(read.m));
            foreach (var (k, e) in read.m)
            {
                if (!e.tryExpungeLocked())
                {
                    m.dirty[k] = e;
                }

            }

        }

        private static bool tryExpungeLocked(this ptr<entry> _addr_e)
        {
            bool isExpunged = default;
            ref entry e = ref _addr_e.val;

            var p = atomic.LoadPointer(_addr_e.p);
            while (p == null)
            {
                if (atomic.CompareAndSwapPointer(_addr_e.p, null, expunged))
                {
                    return true;
                }

                p = atomic.LoadPointer(_addr_e.p);

            }

            return p == expunged;

        }
    }
}
