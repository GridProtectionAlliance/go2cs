// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:25:58 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mspanset.go
namespace go;

using cpu = @internal.cpu_package;
using atomic = runtime.@internal.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = @unsafe_package;


// A spanSet is a set of *mspans.
//
// spanSet is safe for concurrent push and pop operations.

public static partial class runtime_package {

private partial struct spanSet {
    public mutex spineLock;
    public unsafe.Pointer spine; // *[N]*spanSetBlock, accessed atomically
    public System.UIntPtr spineLen; // Spine array length, accessed atomically
    public System.UIntPtr spineCap; // Spine array cap, accessed under lock

// index is the head and tail of the spanSet in a single field.
// The head and the tail both represent an index into the logical
// concatenation of all blocks, with the head always behind or
// equal to the tail (indicating an empty set). This field is
// always accessed atomically.
//
// The head and the tail are only 32 bits wide, which means we
// can only support up to 2^32 pushes before a reset. If every
// span in the heap were stored in this set, and each span were
// the minimum size (1 runtime page, 8 KiB), then roughly the
// smallest heap which would be unrepresentable is 32 TiB in size.
    public headTailIndex index;
}

private static readonly nint spanSetBlockEntries = 512; // 4KB on 64-bit
private static readonly nint spanSetInitSpineCap = 256; // Enough for 1GB heap on 64-bit

private partial struct spanSetBlock {
    public ref lfnode lfnode => ref lfnode_val; // popped is the number of pop operations that have occurred on
// this block. This number is used to help determine when a block
// may be safely recycled.
    public uint popped; // spans is the set of spans in this block.
    public array<ptr<mspan>> spans;
}

// push adds span s to buffer b. push is safe to call concurrently
// with other push and pop operations.
private static void push(this ptr<spanSet> _addr_b, ptr<mspan> _addr_s) {
    ref spanSet b = ref _addr_b.val;
    ref mspan s = ref _addr_s.val;
 
    // Obtain our slot.
    var cursor = uintptr(b.index.incTail().tail() - 1);
    var top = cursor / spanSetBlockEntries;
    var bottom = cursor % spanSetBlockEntries; 

    // Do we need to add a block?
    var spineLen = atomic.Loaduintptr(_addr_b.spineLen);
    ptr<spanSetBlock> block;
retry: 

    // We have a block. Insert the span atomically, since there may be
    // concurrent readers via the block API.
    if (top < spineLen) {
        var spine = atomic.Loadp(@unsafe.Pointer(_addr_b.spine));
        var blockp = add(spine, sys.PtrSize * top);
        block = (spanSetBlock.val)(atomic.Loadp(blockp));
    }
    else
 { 
        // Add a new block to the spine, potentially growing
        // the spine.
        lock(_addr_b.spineLock); 
        // spineLen cannot change until we release the lock,
        // but may have changed while we were waiting.
        spineLen = atomic.Loaduintptr(_addr_b.spineLen);
        if (top < spineLen) {
            unlock(_addr_b.spineLock);
            goto retry;
        }
        if (spineLen == b.spineCap) { 
            // Grow the spine.
            var newCap = b.spineCap * 2;
            if (newCap == 0) {
                newCap = spanSetInitSpineCap;
            }
            var newSpine = persistentalloc(newCap * sys.PtrSize, cpu.CacheLineSize, _addr_memstats.gcMiscSys);
            if (b.spineCap != 0) { 
                // Blocks are allocated off-heap, so
                // no write barriers.
                memmove(newSpine, b.spine, b.spineCap * sys.PtrSize);
            } 
            // Spine is allocated off-heap, so no write barrier.
            atomic.StorepNoWB(@unsafe.Pointer(_addr_b.spine), newSpine);
            b.spineCap = newCap; 
            // We can't immediately free the old spine
            // since a concurrent push with a lower index
            // could still be reading from it. We let it
            // leak because even a 1TB heap would waste
            // less than 2MB of memory on old spines. If
            // this is a problem, we could free old spines
            // during STW.
        }
        block = spanSetBlockPool.alloc(); 

        // Add it to the spine.
        blockp = add(b.spine, sys.PtrSize * top); 
        // Blocks are allocated off-heap, so no write barrier.
        atomic.StorepNoWB(blockp, @unsafe.Pointer(block));
        atomic.Storeuintptr(_addr_b.spineLen, spineLen + 1);
        unlock(_addr_b.spineLock);
    }
    atomic.StorepNoWB(@unsafe.Pointer(_addr_block.spans[bottom]), @unsafe.Pointer(s));
}

// pop removes and returns a span from buffer b, or nil if b is empty.
// pop is safe to call concurrently with other pop and push operations.
private static ptr<mspan> pop(this ptr<spanSet> _addr_b) {
    ref spanSet b = ref _addr_b.val;

    uint head = default;    uint tail = default;

claimLoop:
    while (true) {
        var headtail = b.index.load();
        head, tail = headtail.split();
        if (head >= tail) { 
            // The buf is empty, as far as we can tell.
            return _addr_null!;
        }
        var spineLen = atomic.Loaduintptr(_addr_b.spineLen);
        if (spineLen <= uintptr(head) / spanSetBlockEntries) { 
            // We're racing with a spine growth and the allocation of
            // a new block (and maybe a new spine!), and trying to grab
            // the span at the index which is currently being pushed.
            // Instead of spinning, let's just notify the caller that
            // there's nothing currently here. Spinning on this is
            // almost definitely not worth it.
            return _addr_null!;
        }
        var want = head;
        while (want == head) {
            if (b.index.cas(headtail, makeHeadTailIndex(want + 1, tail))) {
                _breakclaimLoop = true;
                break;
            }
            headtail = b.index.load();
            head, tail = headtail.split();
        } 
        // We failed to claim the spot we were after and the head changed,
        // meaning a popper got ahead of us. Try again from the top because
        // the buf may not be empty.
    }
    var top = head / spanSetBlockEntries;
    var bottom = head % spanSetBlockEntries; 

    // We may be reading a stale spine pointer, but because the length
    // grows monotonically and we've already verified it, we'll definitely
    // be reading from a valid block.
    var spine = atomic.Loadp(@unsafe.Pointer(_addr_b.spine));
    var blockp = add(spine, sys.PtrSize * uintptr(top)); 

    // Given that the spine length is correct, we know we will never
    // see a nil block here, since the length is always updated after
    // the block is set.
    var block = (spanSetBlock.val)(atomic.Loadp(blockp));
    var s = (mspan.val)(atomic.Loadp(@unsafe.Pointer(_addr_block.spans[bottom])));
    while (s == null) { 
        // We raced with the span actually being set, but given that we
        // know a block for this span exists, the race window here is
        // extremely small. Try again.
        s = (mspan.val)(atomic.Loadp(@unsafe.Pointer(_addr_block.spans[bottom])));
    } 
    // Clear the pointer. This isn't strictly necessary, but defensively
    // avoids accidentally re-using blocks which could lead to memory
    // corruption. This way, we'll get a nil pointer access instead.
    atomic.StorepNoWB(@unsafe.Pointer(_addr_block.spans[bottom]), null); 

    // Increase the popped count. If we are the last possible popper
    // in the block (note that bottom need not equal spanSetBlockEntries-1
    // due to races) then it's our resposibility to free the block.
    //
    // If we increment popped to spanSetBlockEntries, we can be sure that
    // we're the last popper for this block, and it's thus safe to free it.
    // Every other popper must have crossed this barrier (and thus finished
    // popping its corresponding mspan) by the time we get here. Because
    // we're the last popper, we also don't have to worry about concurrent
    // pushers (there can't be any). Note that we may not be the popper
    // which claimed the last slot in the block, we're just the last one
    // to finish popping.
    if (atomic.Xadd(_addr_block.popped, 1) == spanSetBlockEntries) { 
        // Clear the block's pointer.
        atomic.StorepNoWB(blockp, null); 

        // Return the block to the block pool.
        spanSetBlockPool.free(block);
    }
    return _addr_s!;
}

// reset resets a spanSet which is empty. It will also clean up
// any left over blocks.
//
// Throws if the buf is not empty.
//
// reset may not be called concurrently with any other operations
// on the span set.
private static void reset(this ptr<spanSet> _addr_b) {
    ref spanSet b = ref _addr_b.val;

    var (head, tail) = b.index.load().split();
    if (head < tail) {
        print("head = ", head, ", tail = ", tail, "\n");
        throw("attempt to clear non-empty span set");
    }
    var top = head / spanSetBlockEntries;
    if (uintptr(top) < b.spineLen) { 
        // If the head catches up to the tail and the set is empty,
        // we may not clean up the block containing the head and tail
        // since it may be pushed into again. In order to avoid leaking
        // memory since we're going to reset the head and tail, clean
        // up such a block now, if it exists.
        var blockp = (spanSetBlock.val)(add(b.spine, sys.PtrSize * uintptr(top)));
        var block = blockp.val;
        if (block != null) { 
            // Sanity check the popped value.
            if (block.popped == 0) { 
                // popped should never be zero because that means we have
                // pushed at least one value but not yet popped if this
                // block pointer is not nil.
                throw("span set block with unpopped elements found in reset");
            }
            if (block.popped == spanSetBlockEntries) { 
                // popped should also never be equal to spanSetBlockEntries
                // because the last popper should have made the block pointer
                // in this slot nil.
                throw("fully empty unfreed span set block found in reset");
            } 

            // Clear the pointer to the block.
            atomic.StorepNoWB(@unsafe.Pointer(blockp), null); 

            // Return the block to the block pool.
            spanSetBlockPool.free(block);
        }
    }
    b.index.reset();
    atomic.Storeuintptr(_addr_b.spineLen, 0);
}

// spanSetBlockPool is a global pool of spanSetBlocks.
private static spanSetBlockAlloc spanSetBlockPool = default;

// spanSetBlockAlloc represents a concurrent pool of spanSetBlocks.
private partial struct spanSetBlockAlloc {
    public lfstack stack;
}

// alloc tries to grab a spanSetBlock out of the pool, and if it fails
// persistentallocs a new one and returns it.
private static ptr<spanSetBlock> alloc(this ptr<spanSetBlockAlloc> _addr_p) {
    ref spanSetBlockAlloc p = ref _addr_p.val;

    {
        var s = (spanSetBlock.val)(p.stack.pop());

        if (s != null) {
            return _addr_s!;
        }
    }
    return _addr_(spanSetBlock.val)(persistentalloc(@unsafe.Sizeof(new spanSetBlock()), cpu.CacheLineSize, _addr_memstats.gcMiscSys))!;
}

// free returns a spanSetBlock back to the pool.
private static void free(this ptr<spanSetBlockAlloc> _addr_p, ptr<spanSetBlock> _addr_block) {
    ref spanSetBlockAlloc p = ref _addr_p.val;
    ref spanSetBlock block = ref _addr_block.val;

    atomic.Store(_addr_block.popped, 0);
    p.stack.push(_addr_block.lfnode);
}

// haidTailIndex represents a combined 32-bit head and 32-bit tail
// of a queue into a single 64-bit value.
private partial struct headTailIndex { // : ulong
}

// makeHeadTailIndex creates a headTailIndex value from a separate
// head and tail.
private static headTailIndex makeHeadTailIndex(uint head, uint tail) {
    return headTailIndex(uint64(head) << 32 | uint64(tail));
}

// head returns the head of a headTailIndex value.
private static uint head(this headTailIndex h) {
    return uint32(h >> 32);
}

// tail returns the tail of a headTailIndex value.
private static uint tail(this headTailIndex h) {
    return uint32(h);
}

// split splits the headTailIndex value into its parts.
private static (uint, uint) split(this headTailIndex h) {
    uint head = default;
    uint tail = default;

    return (h.head(), h.tail());
}

// load atomically reads a headTailIndex value.
private static headTailIndex load(this ptr<headTailIndex> _addr_h) {
    ref headTailIndex h = ref _addr_h.val;

    return headTailIndex(atomic.Load64((uint64.val)(h)));
}

// cas atomically compares-and-swaps a headTailIndex value.
private static bool cas(this ptr<headTailIndex> _addr_h, headTailIndex old, headTailIndex @new) {
    ref headTailIndex h = ref _addr_h.val;

    return atomic.Cas64((uint64.val)(h), uint64(old), uint64(new));
}

// incHead atomically increments the head of a headTailIndex.
private static headTailIndex incHead(this ptr<headTailIndex> _addr_h) {
    ref headTailIndex h = ref _addr_h.val;

    return headTailIndex(atomic.Xadd64((uint64.val)(h), (1 << 32)));
}

// decHead atomically decrements the head of a headTailIndex.
private static headTailIndex decHead(this ptr<headTailIndex> _addr_h) {
    ref headTailIndex h = ref _addr_h.val;

    return headTailIndex(atomic.Xadd64((uint64.val)(h), -(1 << 32)));
}

// incTail atomically increments the tail of a headTailIndex.
private static headTailIndex incTail(this ptr<headTailIndex> _addr_h) {
    ref headTailIndex h = ref _addr_h.val;

    var ht = headTailIndex(atomic.Xadd64((uint64.val)(h), +1)); 
    // Check for overflow.
    if (ht.tail() == 0) {
        print("runtime: head = ", ht.head(), ", tail = ", ht.tail(), "\n");
        throw("headTailIndex overflow");
    }
    return ht;
}

// reset clears the headTailIndex to (0, 0).
private static void reset(this ptr<headTailIndex> _addr_h) {
    ref headTailIndex h = ref _addr_h.val;

    atomic.Store64((uint64.val)(h), 0);
}

} // end runtime_package
