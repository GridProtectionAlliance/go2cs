// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Simple append-only thread-safe hash map for tracing.
// Provides a mapping between variable-length data and a
// unique ID. Subsequent puts of the same data will return
// the same ID. The zero value is ready to use.
//
// Uses a region-based allocation scheme internally, and
// reset clears the whole map.
//
// It avoids doing any high-level Go operations so it's safe
// to use even in sensitive contexts.
namespace go;

using cpu = @internal.cpu_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

[GoType] partial struct traceMap {
    internal @internal.runtime.atomic_package.UnsafePointer root; // *traceMapNode (can't use generics because it's notinheap)
    internal @internal.cpu_package.CacheLinePad _;
    internal @internal.runtime.atomic_package.Uint64 seq;
    internal @internal.cpu_package.CacheLinePad __;
    internal traceRegionAlloc mem;
}

// traceMapNode is an implementation of a lock-free append-only hash-trie
// (a trie of the hash bits).
//
// Key features:
//   - 4-ary trie. Child nodes are indexed by the upper 2 (remaining) bits of the hash.
//     For example, top level uses bits [63:62], next level uses [61:60] and so on.
//   - New nodes are placed at the first empty level encountered.
//   - When the first child is added to a node, the existing value is not moved into a child.
//     This means that you must check the key at each level, not just at the leaf.
//   - No deletion or rebalancing.
//   - Intentionally devolves into a linked list on hash collisions (the hash bits will all
//     get shifted out during iteration, and new nodes will just be appended to the 0th child).
[GoType] partial struct traceMapNode {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal atomic.UnsafePointer children = new(4); // *traceMapNode (can't use generics because it's notinheap)
    internal uintptr hash;
    internal uint64 id;
    internal slice<byte> data;
}

// stealID steals an ID from the table, ensuring that it will not
// appear in the table anymore.
[GoRecv] internal static uint64 stealID(this ref traceMap tab) {
    return tab.seq.Add(1);
}

// put inserts the data into the table.
//
// It's always safe for callers to noescape data because put copies its bytes.
//
// Returns a unique ID for the data and whether this is the first time
// the data has been added to the map.
[GoRecv] internal static (uint64, bool) put(this ref traceMap tab, @unsafe.Pointer data, uintptr size) {
    if (size == 0) {
        return (0, false);
    }
    var hash = memhash(data.val, 0, size);
    ж<traceMapNode> newNode = default!;
    var m = Ꮡ(tab.root);
    var hashIter = hash;
    while (ᐧ) {
        var n = (ж<traceMapNode>)(uintptr)(m.Load());
        if (n == nil) {
            // Try to insert a new map node. We may end up discarding
            // this node if we fail to insert because it turns out the
            // value is already in the map.
            //
            // The discard will only happen if two threads race on inserting
            // the same value. Both might create nodes, but only one will
            // succeed on insertion. If two threads race to insert two
            // different values, then both nodes will *always* get inserted,
            // because the equality checking below will always fail.
            //
            // Performance note: contention on insertion is likely to be
            // higher for small maps, but since this data structure is
            // append-only, either the map stays small because there isn't
            // much activity, or the map gets big and races to insert on
            // the same node are much less likely.
            if (newNode == nil) {
                newNode = tab.newTraceMapNode(data.val, size, hash, tab.seq.Add(1));
            }
            if (m.CompareAndSwapNoWB(nil, new @unsafe.Pointer(newNode))) {
                return ((~newNode).id, true);
            }
            // Reload n. Because pointers are only stored once,
            // we must have lost the race, and therefore n is not nil
            // anymore.
            n = (ж<traceMapNode>)(uintptr)(m.Load());
        }
        if ((~n).hash == hash && ((uintptr)len((~n).data)) == size) {
            if (memequal(new @unsafe.Pointer(Ꮡ((~n).data, 0)), data.val, size)) {
                return ((~n).id, false);
            }
        }
        m = Ꮡ(~n).children.at<atomic.UnsafePointer>(hashIter >> (int)((8 * goarch.PtrSize - 2)));
        hashIter <<= (UntypedInt)(2);
    }
}

[GoRecv] internal static ж<traceMapNode> newTraceMapNode(this ref traceMap tab, @unsafe.Pointer data, uintptr size, uintptr hash, uint64 id) {
    // Create data array.
    var sl = new notInHeapSlice(
        Δarray: tab.mem.alloc(size),
        len: ((nint)size),
        cap: ((nint)size)
    );
    memmove(new @unsafe.Pointer(sl.Δarray), data.val, size);
    // Create metadata structure.
    var meta = (ж<traceMapNode>)(uintptr)(new @unsafe.Pointer(tab.mem.alloc(@unsafe.Sizeof(new traceMapNode(nil)))));
    ((ж<notInHeapSlice>)(uintptr)(new @unsafe.Pointer(Ꮡ((~meta).data)))).val = sl;
    meta.val.id = id;
    meta.val.hash = hash;
    return meta;
}

// reset drops all allocated memory from the table and resets it.
//
// The caller must ensure that there are no put operations executing concurrently
// with this function.
[GoRecv] internal static void reset(this ref traceMap tab) {
    tab.root.Store(nil);
    tab.seq.Store(0);
    tab.mem.drop();
}

} // end runtime_package
