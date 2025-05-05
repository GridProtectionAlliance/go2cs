// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Fixed-size object allocator. Returned memory is not zeroed.
//
// See malloc.go for overview.
namespace go;

using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using runtime.@internal;

partial class runtime_package {

// fixalloc is a simple free-list allocator for fixed size objects.
// Malloc uses a FixAlloc wrapped around sysAlloc to manage its
// mcache and mspan objects.
//
// Memory returned by fixalloc.alloc is zeroed by default, but the
// caller may take responsibility for zeroing allocations by setting
// the zero flag to false. This is only safe if the memory never
// contains heap pointers.
//
// The caller is responsible for locking around FixAlloc calls.
// Callers can keep state in the object but the first word is
// smashed by freeing and reallocating.
//
// Consider marking fixalloc'd types not in heap by embedding
// runtime/internal/sys.NotInHeap.
[GoType] partial struct fixalloc {
    internal uintptr size;
    internal Action<@unsafe.Pointer, @unsafe.Pointer> first;   // called first time p is returned
    internal @unsafe.Pointer arg;
    internal ж<mlink> list;
    internal uintptr chunk; // use uintptr instead of unsafe.Pointer to avoid write barriers
    internal uint32 nchunk;  // bytes remaining in current chunk
    internal uint32 nalloc;  // size of new chunks in bytes
    internal uintptr inuse; // in-use bytes now
    internal ж<sysMemStat> stat;
    internal bool zero; // zero allocations
}

// A generic linked list of blocks.  (Typically the block is bigger than sizeof(MLink).)
// Since assignments to mlink.next will result in a write barrier being performed
// this cannot be used by some of the internal GC structures. For example when
// the sweeper is placing an unmarked object on the free list it does not want the
// write barrier to be called since that could result in the object being reachable.
[GoType] partial struct mlink {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal ж<mlink> next;
}

// Initialize f to allocate objects of the given size,
// using the allocator to obtain chunks of memory.
[GoRecv] internal static void init(this ref fixalloc f, uintptr size, Action<@unsafe.Pointer, @unsafe.Pointer> first, @unsafe.Pointer arg, ж<sysMemStat> Ꮡstat) {
    ref var stat = ref Ꮡstat.val;

    if (size > _FixAllocChunk) {
        @throw("runtime: fixalloc size too large"u8);
    }
    size = max(size, @unsafe.Sizeof(new mlink(nil)));
    f.size = size;
    f.first = first;
    f.arg = arg;
    f.list = default!;
    f.chunk = 0;
    f.nchunk = 0;
    f.nalloc = ((uint32)(_FixAllocChunk / size * size));
    // Round _FixAllocChunk down to an exact multiple of size to eliminate tail waste
    f.inuse = 0;
    f.stat = stat;
    f.zero = true;
}

[GoRecv] internal static @unsafe.Pointer alloc(this ref fixalloc f) {
    if (f.size == 0) {
        print("runtime: use of FixAlloc_Alloc before FixAlloc_Init\n");
        @throw("runtime: internal error"u8);
    }
    if (f.list != nil) {
        @unsafe.Pointer vΔ1 = new @unsafe.Pointer(f.list);
        f.list = f.list.next;
        f.inuse += f.size;
        if (f.zero) {
            memclrNoHeapPointers(vΔ1, f.size);
        }
        return vΔ1;
    }
    if (((uintptr)f.nchunk) < f.size) {
        f.chunk = ((uintptr)(uintptr)persistentalloc(((uintptr)f.nalloc), 0, f.stat));
        f.nchunk = f.nalloc;
    }
    @unsafe.Pointer v = ((@unsafe.Pointer)f.chunk);
    if (f.first != default!) {
        f.first(f.arg, v);
    }
    f.chunk = f.chunk + f.size;
    f.nchunk -= ((uint32)f.size);
    f.inuse += f.size;
    return v;
}

[GoRecv] internal static void free(this ref fixalloc f, @unsafe.Pointer Δp) {
    f.inuse -= f.size;
    var v = (ж<mlink>)(uintptr)(Δp);
    v.val.next = f.list;
    f.list = v;
}

} // end runtime_package
