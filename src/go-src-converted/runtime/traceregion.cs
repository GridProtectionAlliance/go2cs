// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Simple not-in-heap bump-pointer traceRegion allocator.
namespace go;

using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// traceRegionAlloc is a thread-safe region allocator.
// It holds a linked list of traceRegionAllocBlock.
[GoType] partial struct traceRegionAlloc {
    internal mutex @lock;
    internal atomic.Bool dropping;          // For checking invariants.
    internal atomic.UnsafePointer current; // *traceRegionAllocBlock
    internal ж<traceRegionAllocBlock> full;
}

// traceRegionAllocBlock is a block in traceRegionAlloc.
//
// traceRegionAllocBlock is allocated from non-GC'd memory, so it must not
// contain heap pointers. Writes to pointers to traceRegionAllocBlocks do
// not need write barriers.
[GoType] partial struct traceRegionAllocBlock {
    internal sys.NotInHeap _;
    internal partial ref traceRegionAllocBlockHeader traceRegionAllocBlockHeader { get; }
    internal array<byte> data = new(traceRegionAllocBlockData);
}

[GoType] partial struct traceRegionAllocBlockHeader {
    internal ж<traceRegionAllocBlock> next;
    internal atomic.Uintptr off;
}

internal static readonly uintptr traceRegionAllocBlockData = /* 64<<10 - unsafe.Sizeof(traceRegionAllocBlockHeader{}) */ 65520;

// alloc allocates n-byte block. The block is always aligned to 8 bytes, regardless of platform.
internal static ж<notInHeap> alloc(this ж<traceRegionAlloc> Ꮡa, uintptr n) {
    ref var a = ref Ꮡa.Value;

    n = alignUp(n, 8);
    if (n > traceRegionAllocBlockData) {
        @throw("traceRegion: alloc too large"u8);
    }
    if (Ꮡa.of(traceRegionAlloc.Ꮡdropping).Load()) {
        @throw("traceRegion: alloc with concurrent drop"u8);
    }
    // Try to bump-pointer allocate into the current block.
    var block = (ж<traceRegionAllocBlock>)(uintptr)(Ꮡa.of(traceRegionAlloc.Ꮡcurrent).Load());
    if (block != nil) {
        var r = block.of(traceRegionAllocBlock.Ꮡoff).Add(n);
        if (r <= (uintptr)len((~block).data)) {
            return (ж<notInHeap>)(uintptr)(new @unsafe.Pointer(block.at(traceRegionAllocBlock.Ꮡdata, (nint)(r - n))));
        }
    }
    // Try to install a new block.
    @lock(Ꮡa.of(traceRegionAlloc.Ꮡlock));
    // Check block again under the lock. Someone may
    // have gotten here first.
    block = (ж<traceRegionAllocBlock>)(uintptr)(Ꮡa.of(traceRegionAlloc.Ꮡcurrent).Load());
    if (block != nil) {
        var r = block.of(traceRegionAllocBlock.Ꮡoff).Add(n);
        if (r <= (uintptr)len((~block).data)) {
            unlock(Ꮡa.of(traceRegionAlloc.Ꮡlock));
            return (ж<notInHeap>)(uintptr)(new @unsafe.Pointer(block.at(traceRegionAllocBlock.Ꮡdata, (nint)(r - n))));
        }
        // Add the existing block to the full list.
        block.Value.next = a.full;
        a.full = block;
    }
    // Allocate a new block.
    block = (ж<traceRegionAllocBlock>)(uintptr)(sysAlloc(@unsafe.Sizeof(new traceRegionAllocBlock(nil)), Ꮡmemstats.of(mstats.Ꮡother_sys)));
    if (block == nil) {
        @throw("traceRegion: out of memory"u8);
    }
    // Allocate space for our current request, so we always make
    // progress.
    block.of(traceRegionAllocBlock.Ꮡoff).Store(n);
    var x = (ж<notInHeap>)(uintptr)(new @unsafe.Pointer(block.at(traceRegionAllocBlock.Ꮡdata, 0)));
    // Publish the new block.
    Ꮡa.of(traceRegionAlloc.Ꮡcurrent).Store(new @unsafe.Pointer(block));
    unlock(Ꮡa.of(traceRegionAlloc.Ꮡlock));
    return x;
}

// drop frees all previously allocated memory and resets the allocator.
//
// drop is not safe to call concurrently with other calls to drop or with calls to alloc. The caller
// must ensure that it is not possible for anything else to be using the same structure.
internal static void drop(this ж<traceRegionAlloc> Ꮡa) {
    ref var a = ref Ꮡa.Value;

    Ꮡa.of(traceRegionAlloc.Ꮡdropping).Store(true);
    while (a.full != nil) {
        var block = a.full;
        a.full = block.Value.next;
        sysFree(new @unsafe.Pointer(block), @unsafe.Sizeof(new traceRegionAllocBlock(nil)), Ꮡmemstats.of(mstats.Ꮡother_sys));
    }
    {
        @unsafe.Pointer current = (uintptr)Ꮡa.of(traceRegionAlloc.Ꮡcurrent).Load(); if (current != nil) {
            sysFree(current, @unsafe.Sizeof(new traceRegionAllocBlock(nil)), Ꮡmemstats.of(mstats.Ꮡother_sys));
            Ꮡa.of(traceRegionAlloc.Ꮡcurrent).Store(nil);
        }
    }
    Ꮡa.of(traceRegionAlloc.Ꮡdropping).Store(false);
}

} // end runtime_package
