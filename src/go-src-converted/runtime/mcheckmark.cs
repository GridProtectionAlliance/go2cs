// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// GC checkmarks
//
// In a concurrent garbage collector, one worries about failing to mark
// a live object due to mutations without write barriers or bugs in the
// collector implementation. As a sanity check, the GC has a 'checkmark'
// mode that retraverses the object graph with the world stopped, to make
// sure that everything that should be marked is marked.
namespace go;

using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// A checkmarksMap stores the GC marks in "checkmarks" mode. It is a
// per-arena bitmap with a bit for every word in the arena. The mark
// is stored on the bit corresponding to the first word of the marked
// allocation.
[GoType] partial struct checkmarksMap {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal array<uint8> b = new(heapArenaBytes / goarch.PtrSize / 8);
}

// If useCheckmark is true, marking of an object uses the checkmark
// bits instead of the standard mark bits.
internal static bool useCheckmark = false;

// startCheckmarks prepares for the checkmarks phase.
//
// The world must be stopped.
internal static void startCheckmarks() {
    assertWorldStopped();
    // Clear all checkmarks.
    foreach (var (_, ai) in mheap_.allArenas) {
        var arena = mheap_.arenas[ai.l1()].val[ai.l2()];
        var bitmap = arena.val.checkmarks;
        if (bitmap == nil){
            // Allocate bitmap on first use.
            bitmap = (ж<checkmarksMap>)(uintptr)(persistentalloc(@unsafe.Sizeof(bitmap.val), 0, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
            if (bitmap == nil) {
                @throw("out of memory allocating checkmarks bitmap"u8);
            }
            arena.val.checkmarks = bitmap;
        } else {
            // Otherwise clear the existing bitmap.
            clear((~bitmap).b[..]);
        }
    }
    // Enable checkmarking.
    useCheckmark = true;
}

// endCheckmarks ends the checkmarks phase.
internal static void endCheckmarks() {
    if (gcMarkWorkAvailable(nil)) {
        @throw("GC work not flushed"u8);
    }
    useCheckmark = false;
}

// setCheckmark throws if marking object is a checkmarks violation,
// and otherwise sets obj's checkmark. It returns true if obj was
// already checkmarked.
internal static bool setCheckmark(uintptr obj, uintptr @base, uintptr off, markBits mbits) {
    if (!mbits.isMarked()) {
        printlock();
        print("runtime: checkmarks found unexpected unmarked object obj=", ((Δhex)obj), "\n");
        print("runtime: found obj at *(", ((Δhex)@base), "+", ((Δhex)off), ")\n");
        // Dump the source (base) object
        gcDumpObject("base"u8, @base, off);
        // Dump the object
        gcDumpObject("obj"u8, obj, ~((uintptr)0));
        (~getg()).m.val.traceback = 2;
        @throw("checkmark found unmarked object"u8);
    }
    arenaIdx ai = arenaIndex(obj);
    var arena = mheap_.arenas[ai.l1()].val[ai.l2()];
    var arenaWord = (obj / heapArenaBytes / 8) % ((uintptr)len((~(~arena).checkmarks).b));
    var mask = ((byte)(1 << (int)(((obj / heapArenaBytes) % 8))));
    var bytep = Ꮡ(~(~arena).checkmarks).b.at<uint8>(arenaWord);
    if ((uint8)(atomic.Load8(bytep) & mask) != 0) {
        // Already checkmarked.
        return true;
    }
    atomic.Or8(bytep, mask);
    return false;
}

} // end runtime_package
