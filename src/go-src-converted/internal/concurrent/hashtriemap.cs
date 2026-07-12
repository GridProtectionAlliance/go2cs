// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using abi = go.@internal.abi_package;
using goarch = go.@internal.goarch_package;
using rand = math.rand.rand_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.sync;
using math.rand;

partial class concurrent_package {

// HashTrieMap is an implementation of a concurrent hash-trie. The implementation
// is designed around frequent loads, but offers decent performance for stores
// and deletes as well, especially if the map is larger. It's primary use-case is
// the unique package, but can be used elsewhere as well.
[GoType] partial struct HashTrieMap<K, V>
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    internal ж<Δindirect<K, V>> root;
    internal Func<@unsafe.Pointer, uintptr, uintptr> keyHash;
    internal Func<@unsafe.Pointer, @unsafe.Pointer, bool> keyEqual;
    internal Func<@unsafe.Pointer, @unsafe.Pointer, bool> valEqual;
    internal uintptr seed;
}

// NewHashTrieMap creates a new HashTrieMap for the provided key and value.
public static ж<HashTrieMap<K, V>> NewHashTrieMap<K, V>()
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    map<K, V> m = default!;
    var mapType = abi.TypeOf(m).MapType();
    var ht = Ꮡ(new HashTrieMap<K, V>(
        root: newIndirectNode<K, V>(nil),
        keyHash: new Func<@unsafe.Pointer, uintptr, uintptr>((~mapType).Hasher),
        keyEqual: new Func<@unsafe.Pointer, @unsafe.Pointer, bool>((~(~mapType).Key).Equal),
        valEqual: new Func<@unsafe.Pointer, @unsafe.Pointer, bool>((~(~mapType).Elem).Equal),
        seed: (uintptr)rand.Uint64()
    ));
    return ht;
}

// type hashFunc is a methodless func type — rendered inline as its base delegate

// type equalFunc is a methodless func type — rendered inline as its base delegate

// Load returns the value stored in the map for a key, or nil if no
// value is present.
// The ok result indicates whether value was found in the map.
[GoRecv] public static (V value, bool ok) Load<K, V>(this ref HashTrieMap<K, V> ht, K key)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    V value = default!;
    bool ok = default!;

    var hash = ht.keyHash((uintptr)abi.NoEscape(new @unsafe.Pointer(Ꮡ(key))), ht.seed);
    var i = ht.root;
    nint hashShift = 8 * goarch.PtrSize;
    while (hashShift != 0) {
        hashShift -= nChildrenLog2;
        var n = i.at(concurrent_package.Δindirect<K, V>.Ꮡchildren, (nint)((uintptr)(((hash >> (int)(hashShift))) & (uintptr)nChildrenMask))).Load();
        if (n == nil) {
            return (@new<V>().ValueSlot, false);
        }
        if ((~n).isEntry) {
            return n.entry().lookup(key, ht.keyEqual);
        }
        i = n.indirect();
    }
    throw panic("internal/concurrent.HashMapTrie: ran out of hash bits while iterating");
}

// LoadOrStore returns the existing value for the key if present.
// Otherwise, it stores and returns the given value.
// The loaded result is true if the value was loaded, false if stored.
public static (V result, bool loaded) LoadOrStore<K, V>(this ж<HashTrieMap<K, V>> Ꮡht, K key, V value)
    where K : /* comparable */ new()
    where V : /* comparable */ new() {
    V result = default!;
    bool loaded = default!;
    func((defer, recover) =>
    {
    ref var ht = ref Ꮡht.Value;

        var hash = ht.keyHash((uintptr)abi.NoEscape(new @unsafe.Pointer(Ꮡ(key))), ht.seed);
        ж<Δindirect<K, V>> i = default!;
        nuint hashShift = default!;
        ж<atomic.Pointer<node<K, V>>> slot = default!;
        ж<node<K, V>> n = default!;
        while (ᐧ) {
            // Find the key or a candidate location for insertion.
            i = ht.root;
            hashShift = 8 * goarch.PtrSize;
            var haveInsertPoint = false;
            while (hashShift != 0) {
                hashShift -= nChildrenLog2;
                slot = i.at(concurrent_package.Δindirect<K, V>.Ꮡchildren, (nint)((uintptr)(((hash >> (int)(hashShift))) & (uintptr)nChildrenMask)));
                n = slot.Load();
                if (n == nil) {
                    // We found a nil slot which is a candidate for insertion.
                    haveInsertPoint = true;
                    break;
                }
                if ((~n).isEntry) {
                    // We found an existing entry, which is as far as we can go.
                    // If it stays this way, we'll have to replace it with an
                    // indirect node.
                    {
                        var (v, ok) = n.entry().lookup(key, ht.keyEqual); if (ok) {
                            (result, loaded) = (v, true); return;
                        }
                    }
                    haveInsertPoint = true;
                    break;
                }
                i = n.indirect();
            }
            if (!haveInsertPoint) {
                throw panic("internal/concurrent.HashMapTrie: ran out of hash bits while iterating");
            }
            // Grab the lock and double-check what we saw.
            i.of(concurrent_package.Δindirect<K, V>.Ꮡmu).Lock();
            n = slot.Load();
            if ((n == nil || (~n).isEntry) && !i.of(concurrent_package.Δindirect<K, V>.Ꮡdead).Load()) {
                // What we saw is still true, so we can continue with the insert.
                break;
            }
            // We have to start over.
            i.of(concurrent_package.Δindirect<K, V>.Ꮡmu).Unlock();
        }
        // N.B. This lock is held from when we broke out of the outer loop above.
        // We specifically break this out so that we can use defer here safely.
        // One option is to break this out into a new function instead, but
        // there's so much local iteration state used below that this turns out
        // to be cleaner.
        var iʗ1 = i;
        defer(iʗ1.of(concurrent_package.Δindirect<K, V>.Ꮡmu).Unlock);
        ж<Δentry<K, V>> oldEntry = default!;
        if (n != nil) {
            oldEntry = n.entry();
            {
                var (v, ok) = oldEntry.lookup(key, ht.keyEqual); if (ok) {
                    // Easy case: by loading again, it turns out exactly what we wanted is here!
                    (result, loaded) = (v, true); return;
                }
            }
        }
        var newEntry = newEntryNode(key, value);
        if (oldEntry == nil){
            // Easy case: create a new entry and store it.
            slot.Store(newEntry.of(concurrent_package.Δentry<K, V>.Ꮡnode));
        } else {
            // We possibly need to expand the entry already there into one or more new nodes.
            //
            // Publish the node last, which will make both oldEntry and newEntry visible. We
            // don't want readers to be able to observe that oldEntry isn't in the tree.
            slot.Store(ht.expand(oldEntry, newEntry, hash, hashShift, i));
        }
        (result, loaded) = (value, false);
    });
    return (result, loaded);
}

// expand takes oldEntry and newEntry whose hashes conflict from bit 64 down to hashShift and
// produces a subtree of indirect nodes to hold the two new entries.
[GoRecv] internal static ж<node<K, V>> expand<K, V>(this ref HashTrieMap<K, V> ht, ж<Δentry<K, V>> ᏑoldEntry, ж<Δentry<K, V>> ᏑnewEntry, uintptr newHash, nuint hashShift, ж<Δindirect<K, V>> Ꮡparent)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    ref var oldEntry = ref ᏑoldEntry.Value;
    ref var newEntry = ref ᏑnewEntry.Value;
    ref var parent = ref Ꮡparent.Value;

    // Check for a hash collision.
    var oldHash = ht.keyHash(new @unsafe.Pointer(ᏑoldEntry.of(concurrent_package.Δentry<K, V>.Ꮡkey)), ht.seed);
    if (oldHash == newHash) {
        // Store the old entry in the new entry's overflow list, then store
        // the new entry.
        ᏑnewEntry.of(concurrent_package.Δentry<K, V>.Ꮡoverflow).Store(ᏑoldEntry);
        return ᏑnewEntry.of(concurrent_package.Δentry<K, V>.Ꮡnode);
    }
    // We have to add an indirect node. Worse still, we may need to add more than one.
    var newIndirect = newIndirectNode(Ꮡparent);
    var top = newIndirect;
    while (ᐧ) {
        if (hashShift == 0) {
            throw panic("internal/concurrent.HashMapTrie: ran out of hash bits while inserting");
        }
        hashShift -= nChildrenLog2;
        // hashShift is for the level parent is at. We need to go deeper.
        var oi = (uintptr)(((oldHash >> (int)(hashShift))) & (uintptr)nChildrenMask);
        var ni = (uintptr)(((newHash >> (int)(hashShift))) & (uintptr)nChildrenMask);
        if (oi != ni) {
            newIndirect.at(concurrent_package.Δindirect<K, V>.Ꮡchildren, (nint)(oi)).Store(ᏑoldEntry.of(concurrent_package.Δentry<K, V>.Ꮡnode));
            newIndirect.at(concurrent_package.Δindirect<K, V>.Ꮡchildren, (nint)(ni)).Store(ᏑnewEntry.of(concurrent_package.Δentry<K, V>.Ꮡnode));
            break;
        }
        var nextIndirect = newIndirectNode(newIndirect);
        newIndirect.at(concurrent_package.Δindirect<K, V>.Ꮡchildren, (nint)(oi)).Store(nextIndirect.of(concurrent_package.Δindirect<K, V>.Ꮡnode));
        newIndirect = nextIndirect;
    }
    return top.of(concurrent_package.Δindirect<K, V>.Ꮡnode);
}

// CompareAndDelete deletes the entry for key if its value is equal to old.
//
// If there is no current value for key in the map, CompareAndDelete returns false
// (even if the old value is the nil interface value).
[GoRecv] public static bool /*deleted*/ CompareAndDelete<K, V>(this ref HashTrieMap<K, V> ht, K key, V old)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    bool deleted = default!;

    var hash = ht.keyHash((uintptr)abi.NoEscape(new @unsafe.Pointer(Ꮡ(key))), ht.seed);
    ж<Δindirect<K, V>> i = default!;
    nuint hashShift = default!;
    ж<atomic.Pointer<node<K, V>>> slot = default!;
    ж<node<K, V>> n = default!;
    while (ᐧ) {
        // Find the key or return when there's nothing to delete.
        i = ht.root;
        hashShift = 8 * goarch.PtrSize;
        var found = false;
        while (hashShift != 0) {
            hashShift -= nChildrenLog2;
            slot = i.at(concurrent_package.Δindirect<K, V>.Ꮡchildren, (nint)((uintptr)(((hash >> (int)(hashShift))) & (uintptr)nChildrenMask)));
            n = slot.Load();
            if (n == nil) {
                // Nothing to delete. Give up.
                return deleted;
            }
            if ((~n).isEntry) {
                // We found an entry. Check if it matches.
                {
                    var (_, ok) = n.entry().lookup(key, ht.keyEqual); if (!ok) {
                        // No match, nothing to delete.
                        return deleted;
                    }
                }
                // We've got something to delete.
                found = true;
                break;
            }
            i = n.indirect();
        }
        if (!found) {
            throw panic("internal/concurrent.HashMapTrie: ran out of hash bits while iterating");
        }
        // Grab the lock and double-check what we saw.
        i.of(concurrent_package.Δindirect<K, V>.Ꮡmu).Lock();
        n = slot.Load();
        if (!i.of(concurrent_package.Δindirect<K, V>.Ꮡdead).Load()) {
            if (n == nil) {
                // Valid node that doesn't contain what we need. Nothing to delete.
                i.of(concurrent_package.Δindirect<K, V>.Ꮡmu).Unlock();
                return deleted;
            }
            if ((~n).isEntry) {
                // What we saw is still true, so we can continue with the delete.
                break;
            }
        }
        // We have to start over.
        i.of(concurrent_package.Δindirect<K, V>.Ꮡmu).Unlock();
    }
    // Try to delete the entry.
    (var e, deleted) = n.entry().compareAndDelete(key, old, ht.keyEqual, ht.valEqual);
    if (!deleted) {
        // Nothing was actually deleted, which means the node is no longer there.
        i.of(concurrent_package.Δindirect<K, V>.Ꮡmu).Unlock();
        return false;
    }
    if (e != nil) {
        // We didn't actually delete the whole entry, just one entry in the chain.
        // Nothing else to do, since the parent is definitely not empty.
        slot.Store(e.of(concurrent_package.Δentry<K, V>.Ꮡnode));
        i.of(concurrent_package.Δindirect<K, V>.Ꮡmu).Unlock();
        return true;
    }
    // Delete the entry.
    slot.Store(nil);
    // Check if the node is now empty (and isn't the root), and delete it if able.
    while ((~i).parent != nil && i.empty()) {
        if (hashShift == 8 * goarch.PtrSize) {
            throw panic("internal/concurrent.HashMapTrie: ran out of hash bits while iterating");
        }
        hashShift += nChildrenLog2;
        // Delete the current node in the parent.
        var parent = i.Value.parent;
        parent.of(concurrent_package.Δindirect<K, V>.Ꮡmu).Lock();
        i.of(concurrent_package.Δindirect<K, V>.Ꮡdead).Store(true);
        parent.at(concurrent_package.Δindirect<K, V>.Ꮡchildren, (nint)((uintptr)(((hash >> (int)(hashShift))) & (uintptr)nChildrenMask))).Store(nil);
        i.of(concurrent_package.Δindirect<K, V>.Ꮡmu).Unlock();
        i = parent;
    }
    i.of(concurrent_package.Δindirect<K, V>.Ꮡmu).Unlock();
    return true;
}

// All returns an iter.Seq2 that produces all key-value pairs in the map.
// The enumeration does not represent any consistent snapshot of the map,
// but is guaranteed to visit each unique key-value pair only once. It is
// safe to operate on the tree during iteration. No particular enumeration
// order is guaranteed.
public static Action<Func<K, V, bool>> All<K, V>(this ж<HashTrieMap<K, V>> Ꮡht)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    ref var ht = ref Ꮡht.Value;

    return (Func<K, V, bool> yield) => {
        Ꮡht.Value.iter(Ꮡht.Value.root, yield);
    };
}

[GoRecv] internal static bool iter<K, V>(this ref HashTrieMap<K, V> ht, ж<Δindirect<K, V>> Ꮡi, Func<K, V, bool> yield)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    ref var i = ref Ꮡi.Value;

    foreach (var (j, _) in i.children) {
        var n = Ꮡi.at(concurrent_package.Δindirect<K, V>.Ꮡchildren, j).Load();
        if (n == nil) {
            continue;
        }
        if (!(~n).isEntry) {
            if (!ht.iter(n.indirect(), yield)) {
                return false;
            }
            continue;
        }
        var e = n.entry();
        while (e != nil) {
            if (!yield((~e).key, (~e).value)) {
                return false;
            }
            e = e.of(concurrent_package.Δentry<K, V>.Ꮡoverflow).Load();
        }
    }
    return true;
}

internal static readonly UntypedInt nChildrenLog2 = 4;
internal static readonly UntypedInt nChildren = /* 1 << nChildrenLog2 */ 16;
internal static readonly UntypedInt nChildrenMask = /* nChildren - 1 */ 15;

// indirect is an internal node in the hash-trie.
[GoType] partial struct Δindirect<K, V>
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    internal partial ref node<K, V> node { get; }
    internal atomic.Bool dead;
    internal sync.Mutex mu; // Protects mutation to children and any children that are entry nodes.
    internal ж<Δindirect<K, V>> parent;
    internal array<atomic.Pointer<node<K, V>>> children = new(nChildren);
}

internal static ж<Δindirect<K, V>> newIndirectNode<K, V>(ж<Δindirect<K, V>> Ꮡparent)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    ref var parent = ref Ꮡparent.Value;

    return Ꮡ(new Δindirect<K, V>(node: new node<K, V>(isEntry: false), parent: Ꮡparent));
}

[GoRecv] internal static bool empty<K, V>(this ref Δindirect<K, V> i)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    nint nc = 0;
    foreach (var (j, _) in i.children) {
        if (Ꮡ(i.children[j]).Load() != nil) {
            nc++;
        }
    }
    return nc == 0;
}

// entry is a leaf node in the hash-trie.
[GoType] partial struct Δentry<K, V>
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    internal partial ref node<K, V> node { get; }
    internal atomic.Pointer<Δentry<K, V>> overflow; // Overflow for hash collisions.
    internal K key;
    internal V value;
}

internal static ж<Δentry<K, V>> newEntryNode<K, V>(K key, V value)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    return Ꮡ(new Δentry<K, V>(
        node: new node<K, V>(isEntry: true),
        key: key,
        value: value
    ));
}

internal static (V, bool) lookup<K, V>(this ж<Δentry<K, V>> Ꮡe, K key, Func<@unsafe.Pointer, @unsafe.Pointer, bool> equal)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    ref var e = ref Ꮡe.Value;

    while (Ꮡe != nil) {
        if (equal(new @unsafe.Pointer(Ꮡe.of(concurrent_package.Δentry<K, V>.Ꮡkey)), (uintptr)abi.NoEscape(new @unsafe.Pointer(Ꮡ(key))))) {
            return (e.value, true);
        }
        Ꮡe = Ꮡe.of(concurrent_package.Δentry<K, V>.Ꮡoverflow).Load(); e = ref Ꮡe.Value;
    }
    return (@new<V>().ValueSlot, false);
}

// compareAndDelete deletes an entry in the overflow chain if both the key and value compare
// equal. Returns the new entry chain and whether or not anything was deleted.
//
// compareAndDelete must be called under the mutex of the indirect node which e is a child of.
internal static (ж<Δentry<K, V>>, bool) compareAndDelete<K, V>(this ж<Δentry<K, V>> Ꮡhead, K key, V value, Func<@unsafe.Pointer, @unsafe.Pointer, bool> keyEqual, Func<@unsafe.Pointer, @unsafe.Pointer, bool> valEqual)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    ref var head = ref Ꮡhead.Value;

    if (keyEqual(new @unsafe.Pointer(Ꮡhead.of(concurrent_package.Δentry<K, V>.Ꮡkey)), (uintptr)abi.NoEscape(new @unsafe.Pointer(Ꮡ(key)))) && valEqual(new @unsafe.Pointer(Ꮡhead.of(concurrent_package.Δentry<K, V>.Ꮡvalue)), (uintptr)abi.NoEscape(new @unsafe.Pointer(Ꮡ(value))))) {
        // Drop the head of the list.
        return (Ꮡhead.of(concurrent_package.Δentry<K, V>.Ꮡoverflow).Load(), true);
    }
    var i = Ꮡhead.of(concurrent_package.Δentry<K, V>.Ꮡoverflow);
    var e = i.Load();
    while (e != nil) {
        if (keyEqual(new @unsafe.Pointer(e.of(concurrent_package.Δentry<K, V>.Ꮡkey)), (uintptr)abi.NoEscape(new @unsafe.Pointer(Ꮡ(key)))) && valEqual(new @unsafe.Pointer(e.of(concurrent_package.Δentry<K, V>.Ꮡvalue)), (uintptr)abi.NoEscape(new @unsafe.Pointer(Ꮡ(value))))) {
            i.Store(e.of(concurrent_package.Δentry<K, V>.Ꮡoverflow).Load());
            return (Ꮡhead, true);
        }
        i = e.of(concurrent_package.Δentry<K, V>.Ꮡoverflow);
        e = e.of(concurrent_package.Δentry<K, V>.Ꮡoverflow).Load();
    }
    return (Ꮡhead, false);
}

// node is the header for a node. It's polymorphic and
// is actually either an entry or an indirect.
[GoType] partial struct node<K, V>
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    internal bool isEntry;
}

internal static ж<Δentry<K, V>> entry<K, V>(this ж<node<K, V>> Ꮡn)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    ref var n = ref Ꮡn.Value;

    if (!n.isEntry) {
        throw panic("called entry on non-entry node");
    }
    return (ж<Δentry<K, V>>)(uintptr)(@unsafe.Pointer.FromRef(ref n));
}

internal static ж<Δindirect<K, V>> indirect<K, V>(this ж<node<K, V>> Ꮡn)
    where K : /* comparable */ new()
    where V : /* comparable */ new()
{
    ref var n = ref Ꮡn.Value;

    if (n.isEntry) {
        throw panic("called indirect on entry node");
    }
    return (ж<Δindirect<K, V>>)(uintptr)(@unsafe.Pointer.FromRef(ref n));
}

} // end concurrent_package
