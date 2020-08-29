// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:12 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mgcsweepbuf.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // A gcSweepBuf is a set of *mspans.
        //
        // gcSweepBuf is safe for concurrent push operations *or* concurrent
        // pop operations, but not both simultaneously.
        private partial struct gcSweepBuf
        {
            public mutex spineLock;
            public unsafe.Pointer spine; // *[N]*gcSweepBlock, accessed atomically
            public System.UIntPtr spineLen; // Spine array length, accessed atomically
            public System.UIntPtr spineCap; // Spine array cap, accessed under lock

// index is the first unused slot in the logical concatenation
// of all blocks. It is accessed atomically.
            public uint index;
        }

        private static readonly long gcSweepBlockEntries = 512L; // 4KB on 64-bit
        private static readonly long gcSweepBufInitSpineCap = 256L; // Enough for 1GB heap on 64-bit

        private partial struct gcSweepBlock
        {
            public array<ref mspan> spans;
        }

        // push adds span s to buffer b. push is safe to call concurrently
        // with other push operations, but NOT to call concurrently with pop.
        private static void push(this ref gcSweepBuf b, ref mspan s)
        { 
            // Obtain our slot.
            var cursor = uintptr(atomic.Xadd(ref b.index, +1L) - 1L);
            var top = cursor / gcSweepBlockEntries;
            var bottom = cursor % gcSweepBlockEntries; 

            // Do we need to add a block?
            var spineLen = atomic.Loaduintptr(ref b.spineLen);
            ref gcSweepBlock block = default;
retry: 

            // We have a block. Insert the span.
            if (top < spineLen)
            {
                var spine = atomic.Loadp(@unsafe.Pointer(ref b.spine));
                var blockp = add(spine, sys.PtrSize * top);
                block = (gcSweepBlock.Value)(atomic.Loadp(blockp));
            }
            else
            { 
                // Add a new block to the spine, potentially growing
                // the spine.
                lock(ref b.spineLock); 
                // spineLen cannot change until we release the lock,
                // but may have changed while we were waiting.
                spineLen = atomic.Loaduintptr(ref b.spineLen);
                if (top < spineLen)
                {
                    unlock(ref b.spineLock);
                    goto retry;
                }
                if (spineLen == b.spineCap)
                { 
                    // Grow the spine.
                    var newCap = b.spineCap * 2L;
                    if (newCap == 0L)
                    {
                        newCap = gcSweepBufInitSpineCap;
                    }
                    var newSpine = persistentalloc(newCap * sys.PtrSize, sys.CacheLineSize, ref memstats.gc_sys);
                    if (b.spineCap != 0L)
                    { 
                        // Blocks are allocated off-heap, so
                        // no write barriers.
                        memmove(newSpine, b.spine, b.spineCap * sys.PtrSize);
                    } 
                    // Spine is allocated off-heap, so no write barrier.
                    atomic.StorepNoWB(@unsafe.Pointer(ref b.spine), newSpine);
                    b.spineCap = newCap; 
                    // We can't immediately free the old spine
                    // since a concurrent push with a lower index
                    // could still be reading from it. We let it
                    // leak because even a 1TB heap would waste
                    // less than 2MB of memory on old spines. If
                    // this is a problem, we could free old spines
                    // during STW.
                } 

                // Allocate a new block and add it to the spine.
                block = (gcSweepBlock.Value)(persistentalloc(@unsafe.Sizeof(new gcSweepBlock()), sys.CacheLineSize, ref memstats.gc_sys));
                blockp = add(b.spine, sys.PtrSize * top); 
                // Blocks are allocated off-heap, so no write barrier.
                atomic.StorepNoWB(blockp, @unsafe.Pointer(block));
                atomic.Storeuintptr(ref b.spineLen, spineLen + 1L);
                unlock(ref b.spineLock);
            } 

            // We have a block. Insert the span.
            block.spans[bottom] = s;
        }

        // pop removes and returns a span from buffer b, or nil if b is empty.
        // pop is safe to call concurrently with other pop operations, but NOT
        // to call concurrently with push.
        private static ref mspan pop(this ref gcSweepBuf b)
        {
            var cursor = atomic.Xadd(ref b.index, -1L);
            if (int32(cursor) < 0L)
            {
                atomic.Xadd(ref b.index, +1L);
                return null;
            } 

            // There are no concurrent spine or block modifications during
            // pop, so we can omit the atomics.
            var top = cursor / gcSweepBlockEntries;
            var bottom = cursor % gcSweepBlockEntries;
            var blockp = (gcSweepBlock.Value.Value)(add(b.spine, sys.PtrSize * uintptr(top)));
            var block = blockp.Value;
            var s = block.spans[bottom]; 
            // Clear the pointer for block(i).
            block.spans[bottom] = null;
            return s;
        }

        // numBlocks returns the number of blocks in buffer b. numBlocks is
        // safe to call concurrently with any other operation. Spans that have
        // been pushed prior to the call to numBlocks are guaranteed to appear
        // in some block in the range [0, numBlocks()), assuming there are no
        // intervening pops. Spans that are pushed after the call may also
        // appear in these blocks.
        private static long numBlocks(this ref gcSweepBuf b)
        {
            return int((atomic.Load(ref b.index) + gcSweepBlockEntries - 1L) / gcSweepBlockEntries);
        }

        // block returns the spans in the i'th block of buffer b. block is
        // safe to call concurrently with push.
        private static slice<ref mspan> block(this ref gcSweepBuf b, long i)
        { 
            // Perform bounds check before loading spine address since
            // push ensures the allocated length is at least spineLen.
            if (i < 0L || uintptr(i) >= atomic.Loaduintptr(ref b.spineLen))
            {
                throw("block index out of range");
            } 

            // Get block i.
            var spine = atomic.Loadp(@unsafe.Pointer(ref b.spine));
            var blockp = add(spine, sys.PtrSize * uintptr(i));
            var block = (gcSweepBlock.Value)(atomic.Loadp(blockp)); 

            // Slice the block if necessary.
            var cursor = uintptr(atomic.Load(ref b.index));
            var top = cursor / gcSweepBlockEntries;
            var bottom = cursor % gcSweepBlockEntries;
            slice<ref mspan> spans = default;
            if (uintptr(i) < top)
            {
                spans = block.spans[..];
            }
            else
            {
                spans = block.spans[..bottom];
            } 

            // push may have reserved a slot but not filled it yet, so
            // trim away unused entries.
            while (len(spans) > 0L && spans[len(spans) - 1L] == null)
            {
                spans = spans[..len(spans) - 1L];
            }

            return spans;
        }
    }
}
