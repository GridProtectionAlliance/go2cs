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

// package runtime -- go2cs converted at 2022 March 13 05:25:06 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\mcheckmark.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = @unsafe_package;


// A checkmarksMap stores the GC marks in "checkmarks" mode. It is a
// per-arena bitmap with a bit for every word in the arena. The mark
// is stored on the bit corresponding to the first word of the marked
// allocation.
//
//go:notinheap

public static partial class runtime_package {

private partial struct checkmarksMap { // : array<byte>
}

// If useCheckmark is true, marking of an object uses the checkmark
// bits instead of the standard mark bits.
private static var useCheckmark = false;

// startCheckmarks prepares for the checkmarks phase.
//
// The world must be stopped.
private static void startCheckmarks() {
    assertWorldStopped(); 

    // Clear all checkmarks.
    foreach (var (_, ai) in mheap_.allArenas) {
        var arena = mheap_.arenas[ai.l1()][ai.l2()];
        var bitmap = arena.checkmarks;

        if (bitmap == null) { 
            // Allocate bitmap on first use.
            bitmap = (checkmarksMap.val)(persistentalloc(@unsafe.Sizeof(bitmap.val), 0, _addr_memstats.gcMiscSys));
            if (bitmap == null) {
                throw("out of memory allocating checkmarks bitmap");
            }
            arena.checkmarks = bitmap;
        }
        else
 { 
            // Otherwise clear the existing bitmap.
            foreach (var (i) in bitmap) {
                bitmap[i] = 0;
            }
        }
    }    useCheckmark = true;
}

// endCheckmarks ends the checkmarks phase.
private static void endCheckmarks() {
    if (gcMarkWorkAvailable(null)) {
        throw("GC work not flushed");
    }
    useCheckmark = false;
}

// setCheckmark throws if marking object is a checkmarks violation,
// and otherwise sets obj's checkmark. It returns true if obj was
// already checkmarked.
private static bool setCheckmark(System.UIntPtr obj, System.UIntPtr @base, System.UIntPtr off, markBits mbits) {
    if (!mbits.isMarked()) {
        printlock();
        print("runtime: checkmarks found unexpected unmarked object obj=", hex(obj), "\n");
        print("runtime: found obj at *(", hex(base), "+", hex(off), ")\n"); 

        // Dump the source (base) object
        gcDumpObject("base", base, off); 

        // Dump the object
        gcDumpObject("obj", obj, ~uintptr(0));

        getg().m.traceback = 2;
        throw("checkmark found unmarked object");
    }
    var ai = arenaIndex(obj);
    var arena = mheap_.arenas[ai.l1()][ai.l2()];
    var arenaWord = (obj / heapArenaBytes / 8) % uintptr(len(arena.checkmarks));
    var mask = byte(1 << (int)(((obj / heapArenaBytes) % 8)));
    var bytep = _addr_arena.checkmarks[arenaWord];

    if (atomic.Load8(bytep) & mask != 0) { 
        // Already checkmarked.
        return true;
    }
    atomic.Or8(bytep, mask);
    return false;
}

} // end runtime_package
