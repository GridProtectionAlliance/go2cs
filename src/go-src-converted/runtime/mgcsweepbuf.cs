// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:46:53 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mgcsweepbuf.go
using cpu = go.@internal.cpu_package;
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

        private static readonly long gcSweepBlockEntries = (long)512L; // 4KB on 64-bit
        private static readonly long gcSweepBufInitSpineCap = (long)256L; // Enough for 1GB heap on 64-bit

        private partial struct gcSweepBlock
        {
            public array<ptr<mspan>> spans;
        }

        // push adds span s to buffer b. push is safe to call concurrently
        // with other push operations, but NOT to call concurrently with pop.
        private static void push(this ptr<gcSweepBuf> _addr_b, ptr<mspan> _addr_s)
        {
            ref gcSweepBuf b = ref _addr_b.val;
            ref mspan s = ref _addr_s.val;
 
            // Obtain our slot.
            var cursor = uintptr(atomic.Xadd(_addr_b.index, +1L) - 1L);
            var top = cursor / gcSweepBlockEntries;
            var bottom = cursor % gcSweepBlockEntries; 

            // Do we need to add a block?
            var spineLen = atomic.Loaduintptr(_addr_b.spineLen);
            ptr<gcSweepBlock> block;
retry: 

            // We have a block. Insert the span atomically, since there may be
            // concurrent readers via the block API.
            if (top < spineLen)
            {
                var spine = atomic.Loadp(@unsafe.Pointer(_addr_b.spine));
                var blockp = add(spine, sys.PtrSize * top);
                block = (gcSweepBlock.val)(atomic.Loadp(blockp));
            }
            else
            { 
                // Add a new block to the spine, potentially growing
                // the spine.
                lock(_addr_b.spineLock); 
                // spineLen cannot change until we release the lock,
                // but may have changed while we were waiting.
                spineLen = atomic.Loaduintptr(_addr_b.spineLen);
                if (top < spineLen)
                {
                    unlock(_addr_b.spineLock);
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

                    var newSpine = persistentalloc(newCap * sys.PtrSize, cpu.CacheLineSize, _addr_memstats.gc_sys);
                    if (b.spineCap != 0L)
                    { 
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

                // Allocate a new block and add it to the spine.
                block = (gcSweepBlock.val)(persistentalloc(@unsafe.Sizeof(new gcSweepBlock()), cpu.CacheLineSize, _addr_memstats.gc_sys));
                blockp = add(b.spine, sys.PtrSize * top); 
                // Blocks are allocated off-heap, so no write barrier.
                atomic.StorepNoWB(blockp, @unsafe.Pointer(block));
                atomic.Storeuintptr(_addr_b.spineLen, spineLen + 1L);
                unlock(_addr_b.spineLock);

            } 

            // We have a block. Insert the span atomically, since there may be
            // concurrent readers via the block API.
            atomic.StorepNoWB(@unsafe.Pointer(_addr_block.spans[bottom]), @unsafe.Pointer(s));

        }

        // pop removes and returns a span from buffer b, or nil if b is empty.
        // pop is safe to call concurrently with other pop operations, but NOT
        // to call concurrently with push.
        private static ptr<mspan> pop(this ptr<gcSweepBuf> _addr_b)
        {
            ref gcSweepBuf b = ref _addr_b.val;

            var cursor = atomic.Xadd(_addr_b.index, -1L);
            if (int32(cursor) < 0L)
            {
                atomic.Xadd(_addr_b.index, +1L);
                return _addr_null!;
            } 

            // There are no concurrent spine or block modifications during
            // pop, so we can omit the atomics.
            var top = cursor / gcSweepBlockEntries;
            var bottom = cursor % gcSweepBlockEntries;
            var blockp = (gcSweepBlock.val)(add(b.spine, sys.PtrSize * uintptr(top)));
            var block = blockp.val;
            var s = block.spans[bottom]; 
            // Clear the pointer for block(i).
            block.spans[bottom] = null;
            return _addr_s!;

        }

        // numBlocks returns the number of blocks in buffer b. numBlocks is
        // safe to call concurrently with any other operation. Spans that have
        // been pushed prior to the call to numBlocks are guaranteed to appear
        // in some block in the range [0, numBlocks()), assuming there are no
        // intervening pops. Spans that are pushed after the call may also
        // appear in these blocks.
        private static long numBlocks(this ptr<gcSweepBuf> _addr_b)
        {
            ref gcSweepBuf b = ref _addr_b.val;

            return int(divRoundUp(uintptr(atomic.Load(_addr_b.index)), gcSweepBlockEntries));
        }

        // block returns the spans in the i'th block of buffer b. block is
        // safe to call concurrently with push. The block may contain nil
        // pointers that must be ignored, and each entry in the block must be
        // loaded atomically.
        private static slice<ptr<mspan>> block(this ptr<gcSweepBuf> _addr_b, long i)
        {
            ref gcSweepBuf b = ref _addr_b.val;
 
            // Perform bounds check before loading spine address since
            // push ensures the allocated length is at least spineLen.
            if (i < 0L || uintptr(i) >= atomic.Loaduintptr(_addr_b.spineLen))
            {
                throw("block index out of range");
            } 

            // Get block i.
            var spine = atomic.Loadp(@unsafe.Pointer(_addr_b.spine));
            var blockp = add(spine, sys.PtrSize * uintptr(i));
            var block = (gcSweepBlock.val)(atomic.Loadp(blockp)); 

            // Slice the block if necessary.
            var cursor = uintptr(atomic.Load(_addr_b.index));
            var top = cursor / gcSweepBlockEntries;
            var bottom = cursor % gcSweepBlockEntries;
            slice<ptr<mspan>> spans = default;
            if (uintptr(i) < top)
            {
                spans = block.spans[..];
            }
            else
            {
                spans = block.spans[..bottom];
            }

            return spans;

        }
    }
}
