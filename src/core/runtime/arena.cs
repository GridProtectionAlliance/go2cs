// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Implementation of (safe) user arenas.
//
// This file contains the implementation of user arenas wherein Go values can
// be manually allocated and freed in bulk. The act of manually freeing memory,
// potentially before a GC cycle, means that a garbage collection cycle can be
// delayed, improving efficiency by reducing GC cycle frequency. There are other
// potential efficiency benefits, such as improved locality and access to a more
// efficient allocation strategy.
//
// What makes the arenas here safe is that once they are freed, accessing the
// arena's memory will cause an explicit program fault, and the arena's address
// space will not be reused until no more pointers into it are found. There's one
// exception to this: if an arena allocated memory that isn't exhausted, it's placed
// back into a pool for reuse. This means that a crash is not always guaranteed.
//
// While this may seem unsafe, it still prevents memory corruption, and is in fact
// necessary in order to make new(T) a valid implementation of arenas. Such a property
// is desirable to allow for a trivial implementation. (It also avoids complexities
// that arise from synchronization with the GC when trying to set the arena chunks to
// fault while the GC is active.)
//
// The implementation works in layers. At the bottom, arenas are managed in chunks.
// Each chunk must be a multiple of the heap arena size, or the heap arena size must
// be divisible by the arena chunks. The address space for each chunk, and each
// corresponding heapArena for that address space, are eternally reserved for use as
// arena chunks. That is, they can never be used for the general heap. Each chunk
// is also represented by a single mspan, and is modeled as a single large heap
// allocation. It must be, because each chunk contains ordinary Go values that may
// point into the heap, so it must be scanned just like any other object. Any
// pointer into a chunk will therefore always cause the whole chunk to be scanned
// while its corresponding arena is still live.
//
// Chunks may be allocated either from new memory mapped by the OS on our behalf,
// or by reusing old freed chunks. When chunks are freed, their underlying memory
// is returned to the OS, set to fault on access, and may not be reused until the
// program doesn't point into the chunk anymore (the code refers to this state as
// "quarantined"), a property checked by the GC.
//
// The sweeper handles moving chunks out of this quarantine state to be ready for
// reuse. When the chunk is placed into the quarantine state, its corresponding
// span is marked as noscan so that the GC doesn't try to scan memory that would
// cause a fault.
//
// At the next layer are the user arenas themselves. They consist of a single
// active chunk which new Go values are bump-allocated into and a list of chunks
// that were exhausted when allocating into the arena. Once the arena is freed,
// it frees all full chunks it references, and places the active one onto a reuse
// list for a future arena to use. Each arena keeps its list of referenced chunks
// explicitly live until it is freed. Each user arena also maps to an object which
// has a finalizer attached that ensures the arena's chunks are all freed even if
// the arena itself is never explicitly freed.
//
// Pointer-ful memory is bump-allocated from low addresses to high addresses in each
// chunk, while pointer-free memory is bump-allocated from high address to low
// addresses. The reason for this is to take advantage of a GC optimization wherein
// the GC will stop scanning an object when there are no more pointers in it, which
// also allows us to elide clearing the heap bitmap for pointer-free Go values
// allocated into arenas.
//
// Note that arenas are not safe to use concurrently.
//
// In summary, there are 2 resources: arenas, and arena chunks. They exist in the
// following lifecycle:
//
// (1) A new arena is created via newArena.
// (2) Chunks are allocated to hold memory allocated into the arena with new or slice.
//    (a) Chunks are first allocated from the reuse list of partially-used chunks.
//    (b) If there are no such chunks, then chunks on the ready list are taken.
//    (c) Failing all the above, memory for a new chunk is mapped.
// (3) The arena is freed, or all references to it are dropped, triggering its finalizer.
//    (a) If the GC is not active, exhausted chunks are set to fault and placed on a
//        quarantine list.
//    (b) If the GC is active, exhausted chunks are placed on a fault list and will
//        go through step (a) at a later point in time.
//    (c) Any remaining partially-used chunk is placed on a reuse list.
// (4) Once no more pointers are found into quarantined arena chunks, the sweeper
//     takes these chunks out of quarantine and places them on the ready list.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using math = runtime.@internal.math_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// Functions starting with arena_ are meant to be exported to downstream users
// of arenas. They should wrap these functions in a higher-lever API.
//
// The underlying arena and its resources are managed through an opaque unsafe.Pointer.

// arena_newArena is a wrapper around newUserArena.
//
//go:linkname arena_newArena arena.runtime_arena_newArena
internal static @unsafe.Pointer arena_newArena() {
    return new @unsafe.Pointer(newUserArena());
}

// arena_arena_New is a wrapper around (*userArena).new, except that typ
// is an any (must be a *_type, still) and typ must be a type descriptor
// for a pointer to the type to actually be allocated, i.e. pass a *T
// to allocate a T. This is necessary because this function returns a *T.
//
//go:linkname arena_arena_New arena.runtime_arena_arena_New
internal static any arena_arena_New(@unsafe.Pointer arena, any typ) {
    var t = (ж<_type>)(uintptr)((~efaceOf(Ꮡ(typ))).data);
    if ((abiꓸKind)((~t).Kind_ & abi.KindMask) != abi.Pointer) {
        @throw("arena_New: non-pointer type"u8);
    }
    var te = ((ж<ptrtype>)(uintptr)(new @unsafe.Pointer(t))).val.Elem;
    @unsafe.Pointer x = (uintptr)((ж<userArena>)(uintptr)(arena)).@new(te);
    any result = default!;
    var e = efaceOf(Ꮡ(result));
    e.val._type = t;
    e.val.data = x;
    return result;
}

// arena_arena_Slice is a wrapper around (*userArena).slice.
//
//go:linkname arena_arena_Slice arena.runtime_arena_arena_Slice
internal static void arena_arena_Slice(@unsafe.Pointer arena, any Δslice, nint cap) {
    ((ж<userArena>)(uintptr)(arena)).Δslice(Δslice, cap);
}

// arena_arena_Free is a wrapper around (*userArena).free.
//
//go:linkname arena_arena_Free arena.runtime_arena_arena_Free
internal static void arena_arena_Free(@unsafe.Pointer arena) {
    ((ж<userArena>)(uintptr)(arena)).free();
}

// arena_heapify takes a value that lives in an arena and makes a copy
// of it on the heap. Values that don't live in an arena are returned unmodified.
//
//go:linkname arena_heapify arena.runtime_arena_heapify
internal static any arena_heapify(any s) {
    @unsafe.Pointer v = default!;
    var e = efaceOf(Ꮡ(s));
    var t = e.val._type;
    var exprᴛ1 = (abiꓸKind)((~t).Kind_ & abi.KindMask);
    if (exprᴛ1 == abi.ΔString) {
        v = stringStructOf((ж<@string>)(uintptr)((~e).data)).str;
    }
    else if (exprᴛ1 == abi.Slice) {
        v = ((ж<Δslice>)(uintptr)((~e).data)).val.Δarray;
    }
    else if (exprᴛ1 == abi.Pointer) {
        v = e.val.data;
    }
    else { /* default: */
        throw panic("arena: Clone only supports pointers, slices, and strings");
    }

    var span = spanOf(((uintptr)v));
    if (span == nil || !(~span).isUserArenaChunk) {
        // Not stored in a user arena chunk.
        return s;
    }
    // Heap-allocate storage for a copy.
    any x = default!;
    var exprᴛ2 = (abiꓸKind)((~t).Kind_ & abi.KindMask);
    if (exprᴛ2 == abi.ΔString) {
        @string s1 = s._<@string>();
        var (s2, b) = rawstring(len(s1));
        copy(b, s1);
        x = s2;
    }
    else if (exprᴛ2 == abi.Slice) {
        nint len = ((ж<Δslice>)(uintptr)((~e).data)).val.len;
        var et = ((ж<slicetype>)(uintptr)(new @unsafe.Pointer(t))).val.Elem;
        var sl = @new<Δslice>();
        sl.val = new Δslice((uintptr)makeslicecopy(et, len, len, ((ж<Δslice>)(uintptr)((~e).data)).val.Δarray), len, len);
        var xe = efaceOf(Ꮡ(x));
        xe.val._type = t;
        xe.val.data = new @unsafe.Pointer(sl);
    }
    else if (exprᴛ2 == abi.Pointer) {
        var et = ((ж<ptrtype>)(uintptr)(new @unsafe.Pointer(t))).val.Elem;
        @unsafe.Pointer e2 = (uintptr)newobject(et);
        typedmemmove(et, e2, (~e).data);
        var xe = efaceOf(Ꮡ(x));
        xe.val._type = t;
        xe.val.data = e2;
    }

    return x;
}

internal static readonly UntypedInt userArenaChunkBytesMax = /* 8 << 20 */ 8388608;
internal const uintptr userArenaChunkBytes = /* uintptr(int64(userArenaChunkBytesMax-heapArenaBytes)&(int64(userArenaChunkBytesMax-heapArenaBytes)>>63) + heapArenaBytes) */ 4194304; // min(userArenaChunkBytesMax, heapArenaBytes)
internal const uintptr userArenaChunkPages = /* userArenaChunkBytes / pageSize */ 512;
internal const uintptr userArenaChunkMaxAllocBytes = /* userArenaChunkBytes / 4 */ 1048576;

[GoInit] internal static void initΔ1() {
    if (userArenaChunkPages * pageSize != userArenaChunkBytes) {
        @throw("user arena chunk size is not a multiple of the page size"u8);
    }
    if (userArenaChunkBytes % physPageSize != 0) {
        @throw("user arena chunk size is not a multiple of the physical page size"u8);
    }
    if (userArenaChunkBytes < heapArenaBytes){
        if (heapArenaBytes % userArenaChunkBytes != 0) {
            @throw("user arena chunk size is smaller than a heap arena, but doesn't divide it"u8);
        }
    } else {
        if (userArenaChunkBytes % heapArenaBytes != 0) {
            @throw("user arena chunks size is larger than a heap arena, but not a multiple"u8);
        }
    }
    lockInit(ᏑuserArenaState.of(struct{lock mutex; reuse []runtime.liveUserArenaChunk; fault []runtime.liveUserArenaChunk}.Ꮡlock), lockRankUserArenaState);
}

// userArenaChunkReserveBytes returns the amount of additional bytes to reserve for
// heap metadata.
internal static uintptr userArenaChunkReserveBytes() {
    // In the allocation headers experiment, we reserve the end of the chunk for
    // a pointer/scalar bitmap. We also reserve space for a dummy _type that
    // refers to the bitmap. The PtrBytes field of the dummy _type indicates how
    // many of those bits are valid.
    return userArenaChunkBytes / goarch.PtrSize / 8 + @unsafe.Sizeof(new _type{});
}

[GoType] partial struct userArena {
    // fullList is a list of full chunks that have not enough free memory left, and
    // that we'll free once this user arena is freed.
    //
    // Can't use mSpanList here because it's not-in-heap.
    internal ж<mspan> fullList;
    // active is the user arena chunk we're currently allocating into.
    internal ж<mspan> active;
    // refs is a set of references to the arena chunks so that they're kept alive.
    //
    // The last reference in the list always refers to active, while the rest of
    // them correspond to fullList. Specifically, the head of fullList is the
    // second-to-last one, fullList.next is the third-to-last, and so on.
    //
    // In other words, every time a new chunk becomes active, its appended to this
    // list.
    internal slice<@unsafe.Pointer> refs;
    // defunct is true if free has been called on this arena.
    //
    // This is just a best-effort way to discover a concurrent allocation
    // and free. Also used to detect a double-free.
    internal @internal.runtime.atomic_package.Bool defunct;
}

// newUserArena creates a new userArena ready to be used.
internal static ж<userArena> newUserArena() {
    var a = @new<userArena>();
    SetFinalizer(a, (ж<userArena> a) => {
        // If arena handle is dropped without being freed, then call
        // free on the arena, so the arena chunks are never reclaimed
        // by the garbage collector.
        aΔ1.free();
    });
    a.refill();
    return a;
}

// new allocates a new object of the provided type into the arena, and returns
// its pointer.
//
// This operation is not safe to call concurrently with other operations on the
// same arena.
[GoRecv] internal static @unsafe.Pointer @new(this ref userArena a, ж<_type> Ꮡtyp) {
    ref var typ = ref Ꮡtyp.val;

    return (uintptr)a.alloc(Ꮡtyp, -1);
}

// slice allocates a new slice backing store. slice must be a pointer to a slice
// (i.e. *[]T), because userArenaSlice will update the slice directly.
//
// cap determines the capacity of the slice backing store and must be non-negative.
//
// This operation is not safe to call concurrently with other operations on the
// same arena.
[GoRecv] internal static void Δslice(this ref userArena a, any sl, nint cap) {
    if (cap < 0) {
        throw panic("userArena.slice: negative cap");
    }
    var i = efaceOf(Ꮡ(sl));
    var typ = i.val._type;
    if ((abiꓸKind)((~typ).Kind_ & abi.KindMask) != abi.Pointer) {
        throw panic("slice result of non-ptr type");
    }
    typ = ((ж<ptrtype>)(uintptr)(new @unsafe.Pointer(typ))).val.Elem;
    if ((abiꓸKind)((~typ).Kind_ & abi.KindMask) != abi.Slice) {
        throw panic("slice of non-ptr-to-slice type");
    }
    typ = ((ж<slicetype>)(uintptr)(new @unsafe.Pointer(typ))).val.Elem;
    // t is now the element type of the slice we want to allocate.
    ((ж<Δslice>)(uintptr)((~i).data)).val = new Δslice((uintptr)a.alloc(typ, cap), cap, cap);
}

// free returns the userArena's chunks back to mheap and marks it as defunct.
//
// Must be called at most once for any given arena.
//
// This operation is not safe to call concurrently with other operations on the
// same arena.
[GoRecv] internal static void free(this ref userArena a) {
    // Check for a double-free.
    if (a.defunct.Load()) {
        throw panic("arena double free");
    }
    // Mark ourselves as defunct.
    a.defunct.Store(true);
    SetFinalizer(a, default!);
    // Free all the full arenas.
    //
    // The refs on this list are in reverse order from the second-to-last.
    var s = a.fullList;
    nint i = len(a.refs) - 2;
    while (s != nil) {
        a.fullList = s.val.next;
        s.val.next = default!;
        freeUserArenaChunk(s, a.refs[i]);
        s = a.fullList;
        i--;
    }
    if (a.fullList != nil || i >= 0) {
        // There's still something left on the full list, or we
        // failed to actually iterate over the entire refs list.
        @throw("full list doesn't match refs list in length"u8);
    }
    // Put the active chunk onto the reuse list.
    //
    // Note that active's reference is always the last reference in refs.
    s = a.active;
    if (s != nil) {
        if (raceenabled || msanenabled || asanenabled){
            // Don't reuse arenas with sanitizers enabled. We want to catch
            // any use-after-free errors aggressively.
            freeUserArenaChunk(s, a.refs[len(a.refs) - 1]);
        } else {
            @lock(ᏑuserArenaState.of(struct{lock mutex; reuse []runtime.liveUserArenaChunk; fault []runtime.liveUserArenaChunk}.Ꮡlock));
            userArenaState.reuse = append(userArenaState.reuse, new liveUserArenaChunk(s, a.refs[len(a.refs) - 1]));
            unlock(ᏑuserArenaState.of(struct{lock mutex; reuse []runtime.liveUserArenaChunk; fault []runtime.liveUserArenaChunk}.Ꮡlock));
        }
    }
    // nil out a.active so that a race with freeing will more likely cause a crash.
    a.active = default!;
    a.refs = default!;
}

// alloc reserves space in the current chunk or calls refill and reserves space
// in a new chunk. If cap is negative, the type will be taken literally, otherwise
// it will be considered as an element type for a slice backing store with capacity
// cap.
[GoRecv] internal static @unsafe.Pointer alloc(this ref userArena a, ж<_type> Ꮡtyp, nint cap) {
    ref var typ = ref Ꮡtyp.val;

    var s = a.active;
    @unsafe.Pointer x = default!;
    while (ᐧ) {
        x = (uintptr)s.userArenaNextFree(Ꮡtyp, cap);
        if (x != nil) {
            break;
        }
        s = a.refill();
    }
    return x;
}

// refill inserts the current arena chunk onto the full list and obtains a new
// one, either from the partial list or allocating a new one, both from mheap.
[GoRecv] internal static ж<mspan> refill(this ref userArena a) {
    // If there's an active chunk, assume it's full.
    var s = a.active;
    if (s != nil) {
        if ((~s).userArenaChunkFree.size() > userArenaChunkMaxAllocBytes) {
            // It's difficult to tell when we're actually out of memory
            // in a chunk because the allocation that failed may still leave
            // some free space available. However, that amount of free space
            // should never exceed the maximum allocation size.
            @throw("wasted too much memory in an arena chunk"u8);
        }
        s.val.next = a.fullList;
        a.fullList = s;
        a.active = default!;
        s = default!;
    }
    @unsafe.Pointer x = default!;
    // Check the partially-used list.
    @lock(ᏑuserArenaState.of(struct{lock mutex; reuse []runtime.liveUserArenaChunk; fault []runtime.liveUserArenaChunk}.Ꮡlock));
    if (len(userArenaState.reuse) > 0) {
        // Pick off the last arena chunk from the list.
        nint n = len(userArenaState.reuse) - 1;
        x = userArenaState.reuse[n].x;
        s = userArenaState.reuse[n].mspan;
        userArenaState.reuse[n].x = default!;
        userArenaState.reuse[n].mspan = default!;
        userArenaState.reuse = userArenaState.reuse[..(int)(n)];
    }
    unlock(ᏑuserArenaState.of(struct{lock mutex; reuse []runtime.liveUserArenaChunk; fault []runtime.liveUserArenaChunk}.Ꮡlock));
    if (s == nil) {
        // Allocate a new one.
        (x, s) = newUserArenaChunk();
        if (s == nil) {
            @throw("out of memory"u8);
        }
    }
    a.refs = append(a.refs, x);
    a.active = s;
    return s;
}

[GoType] partial struct liveUserArenaChunk {
    public partial ref ж<mspan> mspan { get; } // Must represent a user arena chunk.
    // Reference to mspan.base() to keep the chunk alive.
    internal @unsafe.Pointer x;
}


[GoType("dyn")] partial struct userArenaStateᴛ1 {
    internal mutex @lock;
    // reuse contains a list of partially-used and already-live
    // user arena chunks that can be quickly reused for another
    // arena.
    //
    // Protected by lock.
    internal slice<liveUserArenaChunk> reuse;
    // fault contains full user arena chunks that need to be faulted.
    //
    // Protected by lock.
    internal slice<liveUserArenaChunk> fault;
}
internal static userArenaStateᴛ1 userArenaState;

// userArenaNextFree reserves space in the user arena for an item of the specified
// type. If cap is not -1, this is for an array of cap elements of type t.
[GoRecv] internal static @unsafe.Pointer userArenaNextFree(this ref mspan s, ж<_type> Ꮡtyp, nint cap) {
    ref var typ = ref Ꮡtyp.val;

    var size = typ.Size_;
    if (cap > 0) {
        if (size > ^((uintptr)0) / ((uintptr)cap)) {
            // Overflow.
            @throw("out of memory"u8);
        }
        size *= ((uintptr)cap);
    }
    if (size == 0 || cap == 0) {
        return ((@unsafe.Pointer)(Ꮡ(zerobase)));
    }
    if (size > userArenaChunkMaxAllocBytes) {
        // Redirect allocations that don't fit into a chunk well directly
        // from the heap.
        if (cap >= 0) {
            return (uintptr)newarray(Ꮡtyp, cap);
        }
        return (uintptr)newobject(Ꮡtyp);
    }
    // Prevent preemption as we set up the space for a new object.
    //
    // Act like we're allocating.
    var mp = acquirem();
    if ((~mp).mallocing != 0) {
        @throw("malloc deadlock"u8);
    }
    if ((~mp).gsignal == getg()) {
        @throw("malloc during signal"u8);
    }
    mp.val.mallocing = 1;
    @unsafe.Pointer ptr = default!;
    if (!typ.Pointers()){
        // Allocate pointer-less objects from the tail end of the chunk.
        var (v, ok) = s.userArenaChunkFree.takeFromBack(size, typ.Align_);
        if (ok) {
            ptr = ((@unsafe.Pointer)v);
        }
    } else {
        var (v, ok) = s.userArenaChunkFree.takeFromFront(size, typ.Align_);
        if (ok) {
            ptr = ((@unsafe.Pointer)v);
        }
    }
    if (ptr == nil) {
        // Failed to allocate.
        mp.val.mallocing = 0;
        releasem(mp);
        return default!;
    }
    if (s.needzero != 0) {
        @throw("arena chunk needs zeroing, but should already be zeroed"u8);
    }
    // Set up heap bitmap and do extra accounting.
    if (typ.Pointers()) {
        if (cap >= 0){
            userArenaHeapBitsSetSliceType(Ꮡtyp, cap, ptr, s);
        } else {
            userArenaHeapBitsSetType(Ꮡtyp, ptr, s);
        }
        var c = getMCache(mp);
        if (c == nil) {
            @throw("mallocgc called without a P or outside bootstrapping"u8);
        }
        if (cap > 0){
            c.val.scanAlloc += size - (typ.Size_ - typ.PtrBytes);
        } else {
            c.val.scanAlloc += typ.PtrBytes;
        }
    }
    // Ensure that the stores above that initialize x to
    // type-safe memory and set the heap bits occur before
    // the caller can make ptr observable to the garbage
    // collector. Otherwise, on weakly ordered machines,
    // the garbage collector could follow a pointer to x,
    // but see uninitialized memory or stale heap bits.
    publicationBarrier();
    mp.val.mallocing = 0;
    releasem(mp);
    return ptr;
}

// userArenaHeapBitsSetSliceType is the equivalent of heapBitsSetType but for
// Go slice backing store values allocated in a user arena chunk. It sets up the
// heap bitmap for n consecutive values with type typ allocated at address ptr.
internal static void userArenaHeapBitsSetSliceType(ж<_type> Ꮡtyp, nint n, @unsafe.Pointer ptr, ж<mspan> Ꮡs) {
    ref var typ = ref Ꮡtyp.val;
    ref var s = ref Ꮡs.val;

    var (mem, overflow) = math.MulUintptr(typ.Size_, ((uintptr)n));
    if (overflow || n < 0 || mem > maxAlloc) {
        throw panic(((plainError)"runtime: allocation size out of range"u8));
    }
    for (nint i = 0; i < n; i++) {
        userArenaHeapBitsSetType(Ꮡtyp, (uintptr)add(ptr.val, ((uintptr)i) * typ.Size_), Ꮡs);
    }
}

// userArenaHeapBitsSetType is the equivalent of heapSetType but for
// non-slice-backing-store Go values allocated in a user arena chunk. It
// sets up the type metadata for the value with type typ allocated at address ptr.
// base is the base address of the arena chunk.
internal static void userArenaHeapBitsSetType(ж<_type> Ꮡtyp, @unsafe.Pointer ptr, ж<mspan> Ꮡs) {
    ref var typ = ref Ꮡtyp.val;
    ref var s = ref Ꮡs.val;

    var @base = s.@base();
    var h = s.writeUserArenaHeapBits(((uintptr)ptr));
    var Δp = typ.GCData;
    // start of 1-bit pointer mask (or GC program)
    uintptr gcProgBits = default!;
    if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0) {
        // Expand gc program, using the object itself for storage.
        gcProgBits = runGCProg(addb(Δp, 4), (ж<byte>)(uintptr)(ptr));
        Δp = (ж<byte>)(uintptr)(ptr);
    }
    var nb = typ.PtrBytes / goarch.PtrSize;
    for (var i = ((uintptr)0); i < nb; i += ptrBits) {
        var k = nb - i;
        if (k > ptrBits) {
            k = ptrBits;
        }
        // N.B. On big endian platforms we byte swap the data that we
        // read from GCData, which is always stored in little-endian order
        // by the compiler. writeUserArenaHeapBits handles data in
        // a platform-ordered way for efficiency, but stores back the
        // data in little endian order, since we expose the bitmap through
        // a dummy type.
        h = h.write(Ꮡs, readUintptr(addb(Δp, i / 8)), k);
    }
    // Note: we call pad here to ensure we emit explicit 0 bits
    // for the pointerless tail of the object. This ensures that
    // there's only a single noMorePtrs mark for the next object
    // to clear. We don't need to do this to clear stale noMorePtrs
    // markers from previous uses because arena chunk pointer bitmaps
    // are always fully cleared when reused.
    h = h.pad(Ꮡs, typ.Size_ - typ.PtrBytes);
    h.flush(Ꮡs, ((uintptr)ptr), typ.Size_);
    if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) != 0) {
        // Zero out temporary ptrmask buffer inside object.
        memclrNoHeapPointers(ptr.val, (gcProgBits + 7) / 8);
    }
    // Update the PtrBytes value in the type information. After this
    // point, the GC will observe the new bitmap.
    s.largeType.PtrBytes = ((uintptr)ptr) - @base + typ.PtrBytes;
    // Double-check that the bitmap was written out correctly.
    const bool doubleCheck = false;
    if (doubleCheck) {
        doubleCheckHeapPointersInterior(((uintptr)ptr), ((uintptr)ptr), typ.Size_, typ.Size_, Ꮡtyp, Ꮡ(s.largeType), Ꮡs);
    }
}

[GoType] partial struct ΔwriteUserArenaHeapBits {
    internal uintptr offset; // offset in span that the low bit of mask represents the pointer state of.
    internal uintptr mask; // some pointer bits starting at the address addr.
    internal uintptr valid; // number of bits in buf that are valid (including low)
    internal uintptr low; // number of low-order bits to not overwrite
}

[GoRecv] internal static ΔwriteUserArenaHeapBits /*h*/ writeUserArenaHeapBits(this ref mspan s, uintptr addr) {
    ΔwriteUserArenaHeapBits h = default!;

    var offset = addr - s.@base();
    // We start writing bits maybe in the middle of a heap bitmap word.
    // Remember how many bits into the word we started, so we can be sure
    // not to overwrite the previous bits.
    h.low = offset / goarch.PtrSize % ptrBits;
    // round down to heap word that starts the bitmap word.
    h.offset = offset - h.low * goarch.PtrSize;
    // We don't have any bits yet.
    h.mask = 0;
    h.valid = h.low;
    return h;
}

// write appends the pointerness of the next valid pointer slots
// using the low valid bits of bits. 1=pointer, 0=scalar.
public static ΔwriteUserArenaHeapBits write(this ΔwriteUserArenaHeapBits h, ж<mspan> Ꮡs, uintptr bits, uintptr valid) {
    ref var s = ref Ꮡs.val;

    if (h.valid + valid <= ptrBits) {
        // Fast path - just accumulate the bits.
        h.mask |= (uintptr)(bits << (int)(h.valid));
        h.valid += valid;
        return h;
    }
    // Too many bits to fit in this word. Write the current word
    // out and move on to the next word.
    var data = (uintptr)(h.mask | bits << (int)(h.valid));
    // mask for this word
    h.mask = bits >> (int)((ptrBits - h.valid));
    // leftover for next word
    h.valid += valid - ptrBits;
    // have h.valid+valid bits, writing ptrBits of them
    // Flush mask to the memory bitmap.
    var idx = h.offset / (ptrBits * goarch.PtrSize);
    var m = ((uintptr)1) << (int)(h.low) - 1;
    var bitmap = s.heapBits();
    bitmap[idx] = bswapIfBigEndian((uintptr)((uintptr)(bswapIfBigEndian(bitmap[idx]) & m) | data));
    // Note: no synchronization required for this write because
    // the allocator has exclusive access to the page, and the bitmap
    // entries are all for a single page. Also, visibility of these
    // writes is guaranteed by the publication barrier in mallocgc.
    // Move to next word of bitmap.
    h.offset += ptrBits * goarch.PtrSize;
    h.low = 0;
    return h;
}

// Add padding of size bytes.
public static ΔwriteUserArenaHeapBits pad(this ΔwriteUserArenaHeapBits h, ж<mspan> Ꮡs, uintptr size) {
    ref var s = ref Ꮡs.val;

    if (size == 0) {
        return h;
    }
    var words = size / goarch.PtrSize;
    while (words > ptrBits) {
        h = h.write(Ꮡs, 0, ptrBits);
        words -= ptrBits;
    }
    return h.write(Ꮡs, 0, words);
}

// Flush the bits that have been written, and add zeros as needed
// to cover the full object [addr, addr+size).
public static void flush(this ΔwriteUserArenaHeapBits h, ж<mspan> Ꮡs, uintptr addr, uintptr size) {
    ref var s = ref Ꮡs.val;

    var offset = addr - s.@base();
    // zeros counts the number of bits needed to represent the object minus the
    // number of bits we've already written. This is the number of 0 bits
    // that need to be added.
    var zeros = (offset + size - h.offset) / goarch.PtrSize - h.valid;
    // Add zero bits up to the bitmap word boundary
    if (zeros > 0) {
        var z = ptrBits - h.valid;
        if (z > zeros) {
            z = zeros;
        }
        h.valid += z;
        zeros -= z;
    }
    // Find word in bitmap that we're going to write.
    var bitmap = s.heapBits();
    var idx = h.offset / (ptrBits * goarch.PtrSize);
    // Write remaining bits.
    if (h.valid != h.low) {
        var m = ((uintptr)1) << (int)(h.low) - 1;
        // don't clear existing bits below "low"
        m |= (uintptr)(^(((uintptr)1) << (int)(h.valid) - 1));
        // don't clear existing bits above "valid"
        bitmap[idx] = bswapIfBigEndian((uintptr)((uintptr)(bswapIfBigEndian(bitmap[idx]) & m) | h.mask));
    }
    if (zeros == 0) {
        return;
    }
    // Advance to next bitmap word.
    h.offset += ptrBits * goarch.PtrSize;
    // Continue on writing zeros for the rest of the object.
    // For standard use of the ptr bits this is not required, as
    // the bits are read from the beginning of the object. Some uses,
    // like noscan spans, oblets, bulk write barriers, and cgocheck, might
    // start mid-object, so these writes are still required.
    while (ᐧ) {
        // Write zero bits.
        var idxΔ1 = h.offset / (ptrBits * goarch.PtrSize);
        if (zeros < ptrBits){
            bitmap[idx] = bswapIfBigEndian((uintptr)(bswapIfBigEndian(bitmap[idxΔ1]) & ~(((uintptr)1) << (int)(zeros) - 1)));
            break;
        } else 
        if (zeros == ptrBits){
            bitmap[idx] = 0;
            break;
        } else {
            bitmap[idx] = 0;
            zeros -= ptrBits;
        }
        h.offset += ptrBits * goarch.PtrSize;
    }
}

// bswapIfBigEndian swaps the byte order of the uintptr on goarch.BigEndian platforms,
// and leaves it alone elsewhere.
internal static uintptr bswapIfBigEndian(uintptr x) {
    if (goarch.BigEndian) {
        if (goarch.PtrSize == 8) {
            return ((uintptr)sys.Bswap64(((uint64)x)));
        }
        return ((uintptr)sys.Bswap32(((uint32)x)));
    }
    return x;
}

// newUserArenaChunk allocates a user arena chunk, which maps to a single
// heap arena and single span. Returns a pointer to the base of the chunk
// (this is really important: we need to keep the chunk alive) and the span.
internal static (@unsafe.Pointer, ж<mspan>) newUserArenaChunk() {
    if (gcphase == _GCmarktermination) {
        @throw("newUserArenaChunk called with gcphase == _GCmarktermination"u8);
    }
    // Deduct assist credit. Because user arena chunks are modeled as one
    // giant heap object which counts toward heapLive, we're obligated to
    // assist the GC proportionally (and it's worth noting that the arena
    // does represent additional work for the GC, but we also have no idea
    // what that looks like until we actually allocate things into the
    // arena).
    deductAssistCredit(userArenaChunkBytes);
    // Set mp.mallocing to keep from being preempted by GC.
    var mp = acquirem();
    if ((~mp).mallocing != 0) {
        @throw("malloc deadlock"u8);
    }
    if ((~mp).gsignal == getg()) {
        @throw("malloc during signal"u8);
    }
    mp.val.mallocing = 1;
    // Allocate a new user arena.
    ж<mspan> span = default!;
    systemstack(
    var mheap_ʗ2 = mheap_;
    var spanʗ2 = span;
    () => {
        spanʗ2 = mheap_ʗ2.allocUserArenaChunk();
    });
    if (span == nil) {
        @throw("out of memory"u8);
    }
    @unsafe.Pointer x = ((@unsafe.Pointer)span.@base());
    // Allocate black during GC.
    // All slots hold nil so no scanning is needed.
    // This may be racing with GC so do it atomically if there can be
    // a race marking the bit.
    if (gcphase != _GCoff) {
        gcmarknewobject(span, span.@base());
    }
    if (raceenabled) {
        // TODO(mknyszek): Track individual objects.
        racemalloc(((@unsafe.Pointer)span.@base()), (~span).elemsize);
    }
    if (msanenabled) {
        // TODO(mknyszek): Track individual objects.
        msanmalloc(((@unsafe.Pointer)span.@base()), (~span).elemsize);
    }
    if (asanenabled) {
        // TODO(mknyszek): Track individual objects.
        var rzSize = computeRZlog((~span).elemsize);
        span.val.elemsize -= rzSize;
        (~span).largeType.val.Size_ = span.val.elemsize;
        var rzStart = span.@base() + (~span).elemsize;
        span.val.userArenaChunkFree = makeAddrRange(span.@base(), rzStart);
        asanpoison(((@unsafe.Pointer)rzStart), (~span).limit - rzStart);
        asanunpoison(((@unsafe.Pointer)span.@base()), (~span).elemsize);
    }
    {
        nint rate = MemProfileRate; if (rate > 0) {
            var c = getMCache(mp);
            if (c == nil) {
                @throw("newUserArenaChunk called without a P or outside bootstrapping"u8);
            }
            // Note cache c only valid while m acquired; see #47302
            if (rate != 1 && userArenaChunkBytes < (~c).nextSample){
                c.val.nextSample -= userArenaChunkBytes;
            } else {
                profilealloc(mp, ((@unsafe.Pointer)span.@base()), userArenaChunkBytes);
            }
        }
    }
    mp.val.mallocing = 0;
    releasem(mp);
    // Again, because this chunk counts toward heapLive, potentially trigger a GC.
    {
        var t = (new gcTrigger(kind: gcTriggerHeap)); if (t.test()) {
            gcStart(t);
        }
    }
    if (debug.malloc) {
        if (inittrace.active && inittrace.id == (~getg()).goid) {
            // Init functions are executed sequentially in a single goroutine.
            inittrace.bytes += ((uint64)userArenaChunkBytes);
        }
    }
    // Double-check it's aligned to the physical page size. Based on the current
    // implementation this is trivially true, but it need not be in the future.
    // However, if it's not aligned to the physical page size then we can't properly
    // set it to fault later.
    if (((uintptr)x) % physPageSize != 0) {
        @throw("user arena chunk is not aligned to the physical page size"u8);
    }
    return (x, span);
}

// isUnusedUserArenaChunk indicates that the arena chunk has been set to fault
// and doesn't contain any scannable memory anymore. However, it might still be
// mSpanInUse as it sits on the quarantine list, since it needs to be swept.
//
// This is not safe to execute unless the caller has ownership of the mspan or
// the world is stopped (preemption is prevented while the relevant state changes).
//
// This is really only meant to be used by accounting tests in the runtime to
// distinguish when a span shouldn't be counted (since mSpanInUse might not be
// enough).
[GoRecv] internal static bool isUnusedUserArenaChunk(this ref mspan s) {
    return s.isUserArenaChunk && s.spanclass == makeSpanClass(0, true);
}

// setUserArenaChunkToFault sets the address space for the user arena chunk to fault
// and releases any underlying memory resources.
//
// Must be in a non-preemptible state to ensure the consistency of statistics
// exported to MemStats.
[GoRecv] internal static void setUserArenaChunkToFault(this ref mspan s) {
    if (!s.isUserArenaChunk) {
        @throw("invalid span in heapArena for user arena"u8);
    }
    if (s.npages * pageSize != userArenaChunkBytes) {
        @throw("span on userArena.faultList has invalid size"u8);
    }
    // Update the span class to be noscan. What we want to happen is that
    // any pointer into the span keeps it from getting recycled, so we want
    // the mark bit to get set, but we're about to set the address space to fault,
    // so we have to prevent the GC from scanning this memory.
    //
    // It's OK to set it here because (1) a GC isn't in progress, so the scanning code
    // won't make a bad decision, (2) we're currently non-preemptible and in the runtime,
    // so a GC is blocked from starting. We might race with sweeping, which could
    // put it on the "wrong" sweep list, but really don't care because the chunk is
    // treated as a large object span and there's no meaningful difference between scan
    // and noscan large objects in the sweeper. The STW at the start of the GC acts as a
    // barrier for this update.
    s.spanclass = makeSpanClass(0, true);
    // Actually set the arena chunk to fault, so we'll get dangling pointer errors.
    // sysFault currently uses a method on each OS that forces it to evacuate all
    // memory backing the chunk.
    sysFault(((@unsafe.Pointer)s.@base()), s.npages * pageSize);
    // Everything on the list is counted as in-use, however sysFault transitions to
    // Reserved, not Prepared, so we skip updating heapFree or heapReleased and just
    // remove the memory from the total altogether; it's just address space now.
    gcController.heapInUse.add(-((int64)(s.npages * pageSize)));
    // Count this as a free of an object right now as opposed to when
    // the span gets off the quarantine list. The main reason is so that the
    // amount of bytes allocated doesn't exceed how much is counted as
    // "mapped ready," which could cause a deadlock in the pacer.
    gcController.totalFree.Add(((int64)s.elemsize));
    // Update consistent stats to match.
    //
    // We're non-preemptible, so it's safe to update consistent stats (our P
    // won't change out from under us).
    var stats = memstats.heapStats.acquire();
    atomic.Xaddint64(Ꮡ((~stats).committed), -((int64)(s.npages * pageSize)));
    atomic.Xaddint64(Ꮡ((~stats).inHeap), -((int64)(s.npages * pageSize)));
    atomic.Xadd64(Ꮡ((~stats).largeFreeCount), 1);
    atomic.Xadd64(Ꮡ((~stats).largeFree), ((int64)s.elemsize));
    memstats.heapStats.release();
    // This counts as a free, so update heapLive.
    gcController.update(-((int64)s.elemsize), 0);
    // Mark it as free for the race detector.
    if (raceenabled) {
        racefree(((@unsafe.Pointer)s.@base()), s.elemsize);
    }
    systemstack(
    var mheap_ʗ2 = mheap_;
    () => {
        @lock(Ꮡmheap_ʗ2.of(mheap.Ꮡlock));
        mheap_ʗ2.userArena.quarantineList.insert(s);
        unlock(Ꮡmheap_ʗ2.of(mheap.Ꮡlock));
    });
}

// inUserArenaChunk returns true if p points to a user arena chunk.
internal static bool inUserArenaChunk(uintptr Δp) {
    var s = spanOf(Δp);
    if (s == nil) {
        return false;
    }
    return (~s).isUserArenaChunk;
}

// freeUserArenaChunk releases the user arena represented by s back to the runtime.
//
// x must be a live pointer within s.
//
// The runtime will set the user arena to fault once it's safe (the GC is no longer running)
// and then once the user arena is no longer referenced by the application, will allow it to
// be reused.
internal static void freeUserArenaChunk(ж<mspan> Ꮡs, @unsafe.Pointer x) {
    ref var s = ref Ꮡs.val;

    if (!s.isUserArenaChunk) {
        @throw("span is not for a user arena"u8);
    }
    if (s.npages * pageSize != userArenaChunkBytes) {
        @throw("invalid user arena span size"u8);
    }
    // Mark the region as free to various sanitizers immediately instead
    // of handling them at sweep time.
    if (raceenabled) {
        racefree(((@unsafe.Pointer)s.@base()), s.elemsize);
    }
    if (msanenabled) {
        msanfree(((@unsafe.Pointer)s.@base()), s.elemsize);
    }
    if (asanenabled) {
        asanpoison(((@unsafe.Pointer)s.@base()), s.elemsize);
    }
    // Make ourselves non-preemptible as we manipulate state and statistics.
    //
    // Also required by setUserArenaChunksToFault.
    var mp = acquirem();
    // We can only set user arenas to fault if we're in the _GCoff phase.
    if (gcphase == _GCoff){
        @lock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
        var faultList = userArenaState.fault;
        userArenaState.fault = default!;
        unlock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
        s.setUserArenaChunkToFault();
        foreach (var (_, lc) in faultList) {
            lc.mspan.setUserArenaChunkToFault();
        }
        // Until the chunks are set to fault, keep them alive via the fault list.
        KeepAlive(x);
        KeepAlive(faultList);
    } else {
        // Put the user arena on the fault list.
        @lock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
        userArenaState.fault = append(userArenaState.fault, new liveUserArenaChunk(Ꮡs, x.val));
        unlock(ᏑuserArenaState.of(userArenaStateᴛ1.Ꮡlock));
    }
    releasem(mp);
}

// allocUserArenaChunk attempts to reuse a free user arena chunk represented
// as a span.
//
// Must be in a non-preemptible state to ensure the consistency of statistics
// exported to MemStats.
//
// Acquires the heap lock. Must run on the system stack for that reason.
//
//go:systemstack
[GoRecv] internal static ж<mspan> allocUserArenaChunk(this ref mheap h) {
    ж<mspan> s = default!;
    uintptr @base = default!;
    // First check the free list.
    @lock(Ꮡ(h.@lock));
    if (!h.userArena.readyList.isEmpty()){
        s = h.userArena.readyList.first;
        h.userArena.readyList.remove(s);
        @base = s.@base();
    } else {
        // Free list was empty, so allocate a new arena.
        var hintList = Ꮡh.userArena.of(struct{arenaHints *arenaHint; quarantineList runtime.mSpanList; readyList runtime.mSpanList}.ᏑarenaHints);
        if (raceenabled) {
            // In race mode just use the regular heap hints. We might fragment
            // the address space, but the race detector requires that the heap
            // is mapped contiguously.
            hintList = Ꮡ(h.arenaHints);
        }
        var (v, size) = h.sysAlloc(userArenaChunkBytes, hintList, false);
        if (size % userArenaChunkBytes != 0) {
            @throw("sysAlloc size is not divisible by userArenaChunkBytes"u8);
        }
        if (size > userArenaChunkBytes) {
            // We got more than we asked for. This can happen if
            // heapArenaSize > userArenaChunkSize, or if sysAlloc just returns
            // some extra as a result of trying to find an aligned region.
            //
            // Divide it up and put it on the ready list.
            for (var i = userArenaChunkBytes; i < size; i += userArenaChunkBytes) {
                var sΔ1 = h.allocMSpanLocked();
                sΔ1.init(((uintptr)v) + i, userArenaChunkPages);
                h.userArena.readyList.insertBack(sΔ1);
            }
            size = userArenaChunkBytes;
        }
        @base = ((uintptr)v);
        if (@base == 0) {
            // Out of memory.
            unlock(Ꮡ(h.@lock));
            return default!;
        }
        s = h.allocMSpanLocked();
    }
    unlock(Ꮡ(h.@lock));
    // sysAlloc returns Reserved address space, and any span we're
    // reusing is set to fault (so, also Reserved), so transition
    // it to Prepared and then Ready.
    //
    // Unlike (*mheap).grow, just map in everything that we
    // asked for. We're likely going to use it all.
    sysMap(((@unsafe.Pointer)@base), userArenaChunkBytes, ᏑgcController.of(gcControllerState.ᏑheapReleased));
    sysUsed(((@unsafe.Pointer)@base), userArenaChunkBytes, userArenaChunkBytes);
    // Model the user arena as a heap span for a large object.
    var spc = makeSpanClass(0, false);
    h.initSpan(s, spanAllocHeap, spc, @base, userArenaChunkPages);
    s.val.isUserArenaChunk = true;
    s.val.elemsize -= userArenaChunkReserveBytes();
    s.val.limit = s.@base() + (~s).elemsize;
    s.val.freeindex = 1;
    s.val.allocCount = 1;
    // Account for this new arena chunk memory.
    gcController.heapInUse.add(((int64)userArenaChunkBytes));
    gcController.heapReleased.add(-((int64)userArenaChunkBytes));
    var stats = memstats.heapStats.acquire();
    atomic.Xaddint64(Ꮡ((~stats).inHeap), ((int64)userArenaChunkBytes));
    atomic.Xaddint64(Ꮡ((~stats).committed), ((int64)userArenaChunkBytes));
    // Model the arena as a single large malloc.
    atomic.Xadd64(Ꮡ((~stats).largeAlloc), ((int64)(~s).elemsize));
    atomic.Xadd64(Ꮡ((~stats).largeAllocCount), 1);
    memstats.heapStats.release();
    // Count the alloc in inconsistent, internal stats.
    gcController.totalAlloc.Add(((int64)(~s).elemsize));
    // Update heapLive.
    gcController.update(((int64)(~s).elemsize), 0);
    // This must clear the entire heap bitmap so that it's safe
    // to allocate noscan data without writing anything out.
    s.initHeapBits(true);
    // Clear the span preemptively. It's an arena chunk, so let's assume
    // everything is going to be used.
    //
    // This also seems to make a massive difference as to whether or
    // not Linux decides to back this memory with transparent huge
    // pages. There's latency involved in this zeroing, but the hugepage
    // gains are almost always worth it. Note: it's important that we
    // clear even if it's freshly mapped and we know there's no point
    // to zeroing as *that* is the critical signal to use huge pages.
    memclrNoHeapPointers(((@unsafe.Pointer)s.@base()), (~s).elemsize);
    s.val.needzero = 0;
    s.val.freeIndexForScan = 1;
    // Set up the range for allocation.
    s.val.userArenaChunkFree = makeAddrRange(@base, @base + (~s).elemsize);
    // Put the large span in the mcentral swept list so that it's
    // visible to the background sweeper.
    h.central[spc].mcentral.fullSwept(h.sweepgen).push(s);
    // Set up an allocation header. Avoid write barriers here because this type
    // is not a real type, and it exists in an invalid location.
    ((ж<uintptr>)(uintptr)(((@unsafe.Pointer)(Ꮡ((~s).largeType))))).val = ((uintptr)((@unsafe.Pointer)(~s).limit));
    ((ж<uintptr>)(uintptr)(((@unsafe.Pointer)(Ꮡ((~(~s).largeType).GCData))))).val = (~s).limit + @unsafe.Sizeof(new _type{});
    (~s).largeType.val.PtrBytes = 0;
    (~s).largeType.val.Size_ = s.val.elemsize;
    return s;
}

} // end runtime_package
