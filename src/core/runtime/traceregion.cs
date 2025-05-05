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
    internal @internal.runtime.atomic_package.Bool dropping;         // For checking invariants.
    internal @internal.runtime.atomic_package.UnsafePointer current; // *traceRegionAllocBlock
    internal ж<traceRegionAllocBlock> full;
}

// traceRegionAllocBlock is a block in traceRegionAlloc.
//
// traceRegionAllocBlock is allocated from non-GC'd memory, so it must not
// contain heap pointers. Writes to pointers to traceRegionAllocBlocks do
// not need write barriers.
[GoType] partial struct traceRegionAllocBlock {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal partial ref traceRegionAllocBlockHeader traceRegionAllocBlockHeader { get; }
    internal array<byte> data = new(traceRegionAllocBlockData);
}

[GoType] partial struct traceRegionAllocBlockHeader {
    internal ж<traceRegionAllocBlock> next;
    internal @internal.runtime.atomic_package.Uintptr off;
}

internal const uintptr traceRegionAllocBlockData = /* 64<<10 - unsafe.Sizeof(traceRegionAllocBlockHeader{}) */ 65520;

// alloc allocates n-byte block. The block is always aligned to 8 bytes, regardless of platform.
[GoRecv] internal static ж<notInHeap> alloc(this ref traceRegionAlloc a, uintptr n) {
    n = alignUp(n, 8);
    if (n > traceRegionAllocBlockData) {
        @throw("traceRegion: alloc too large"u8);
    }
    if (a.dropping.Load()) {
        @throw("traceRegion: alloc with concurrent drop"u8);
    }
    // Try to bump-pointer allocate into the current block.
    var block = (ж<traceRegionAllocBlock>)(uintptr)(a.current.Load());
    if (block != nil) {
        var r = block.off.Add(n);
        if (r <= ((uintptr)len((~block).data))) {
            return (ж<notInHeap>)(uintptr)(new @unsafe.Pointer(Ꮡ(~block).data.at<byte>(r - n)));
        }
    }
    // Try to install a new block.
    @lock(Ꮡ(a.@lock));
    // Check block again under the lock. Someone may
    // have gotten here first.
    block = (ж<traceRegionAllocBlock>)(uintptr)(a.current.Load());
    if (block != nil) {
        var r = block.off.Add(n);
        if (r <= ((uintptr)len((~block).data))) {
            unlock(Ꮡ(a.@lock));
            return (ж<notInHeap>)(uintptr)(new @unsafe.Pointer(Ꮡ(~block).data.at<byte>(r - n)));
        }
        // Add the existing block to the full list.
        block.next = a.full;
        a.full = block;
    }
    // Allocate a new block.
    block = (ж<traceRegionAllocBlock>)(uintptr)(sysAlloc(@unsafe.Sizeof(new traceRegionAllocBlock(nil)), Ꮡmemstats.of(mstats.Ꮡother_sys)));
    if (block == nil) {
        @throw("traceRegion: out of memory"u8);
    }
    // Allocate space for our current request, so we always make
    // progress.
    block.off.Store(n);
    var x = (ж<notInHeap>)(uintptr)(new @unsafe.Pointer(Ꮡ(~block).data.at<byte>(0)));
    // Publish the new block.
    a.current.Store(new @unsafe.Pointer(block));
    unlock(Ꮡ(a.@lock));
    return x;
}

// drop frees all previously allocated memory and resets the allocator.
//
// drop is not safe to call concurrently with other calls to drop or with calls to alloc. The caller
// must ensure that it is not possible for anything else to be using the same structure.
[GoRecv] internal static void drop(this ref traceRegionAlloc a) {
    a.dropping.Store(true);
    while (a.full != nil) {
        var block = a.full;
        a.full = block.next;
        sysFree(new @unsafe.Pointer(block), @unsafe.Sizeof(new traceRegionAllocBlock(nil)), Ꮡmemstats.of(mstats.Ꮡother_sys));
    }
    {
        @unsafe.Pointer current = (uintptr)a.current.Load(); if (current != nil) {
            sysFree(current, @unsafe.Sizeof(new traceRegionAllocBlock(nil)), Ꮡmemstats.of(mstats.Ꮡother_sys));
            a.current.Store(nil);
        }
    }
    a.dropping.Store(false);
}

} // end runtime_package
