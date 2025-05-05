// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// A Pinner is a set of Go objects each pinned to a fixed location in memory. The
// [Pinner.Pin] method pins one object, while [Pinner.Unpin] unpins all pinned
// objects. See their comments for more information.
[GoType] partial struct Pinner {
    public partial ref ж<pinner> pinner { get; }
}

// Pin pins a Go object, preventing it from being moved or freed by the garbage
// collector until the [Pinner.Unpin] method has been called.
//
// A pointer to a pinned object can be directly stored in C memory or can be
// contained in Go memory passed to C functions. If the pinned object itself
// contains pointers to Go objects, these objects must be pinned separately if they
// are going to be accessed from C code.
//
// The argument must be a pointer of any type or an [unsafe.Pointer].
// It's safe to call Pin on non-Go pointers, in which case Pin will do nothing.
[GoRecv] public static void Pin(this ref Pinner Δp, any pointer) {
    if (Δp.pinner == nil) {
        // Check the pinner cache first.
        var mp = acquirem();
        {
            var pp = (~mp).p.ptr(); if (pp != nil) {
                Δp.pinner = pp.val.pinnerCache;
                pp.val.pinnerCache = default!;
            }
        }
        releasem(mp);
        if (Δp.pinner == nil) {
            // Didn't get anything from the pinner cache.
            Δp.pinner = @new<pinner>();
            Δp.refs = Δp.refStore[..0];
            // We set this finalizer once and never clear it. Thus, if the
            // pinner gets cached, we'll reuse it, along with its finalizer.
            // This lets us avoid the relatively expensive SetFinalizer call
            // when reusing from the cache. The finalizer however has to be
            // resilient to an empty pinner being finalized, which is done
            // by checking p.refs' length.
            SetFinalizer(Δp.pinner, (ж<pinner> i) => {
                if (len((~i).refs) != 0) {
                    i.unpin();
                    // only required to make the test idempotent
                    pinnerLeakPanic();
                }
            });
        }
    }
    @unsafe.Pointer ptr = (uintptr)pinnerGetPtr(Ꮡ(pointer));
    if (setPinned(ptr, true)) {
        Δp.refs = append(Δp.refs, ptr);
    }
}

// Unpin unpins all pinned objects of the [Pinner].
[GoRecv] public static void Unpin(this ref Pinner Δp) {
    Δp.pinner.unpin();
    var mp = acquirem();
    {
        var pp = (~mp).p.ptr(); if (pp != nil && (~pp).pinnerCache == nil) {
            // Put the pinner back in the cache, but only if the
            // cache is empty. If application code is reusing Pinners
            // on its own, we want to leave the backing store in place
            // so reuse is more efficient.
            pp.val.pinnerCache = Δp.pinner;
            Δp.pinner = default!;
        }
    }
    releasem(mp);
}

internal static readonly UntypedInt pinnerSize = 64;
internal const uintptr pinnerRefStoreSize = /* (pinnerSize - unsafe.Sizeof([]unsafe.Pointer{})) / unsafe.Sizeof(unsafe.Pointer(nil)) */ 5;

[GoType] partial struct pinner {
    internal slice<@unsafe.Pointer> refs;
    internal array<@unsafe.Pointer> refStore = new(pinnerRefStoreSize);
}

[GoRecv] internal static void unpin(this ref pinner Δp) {
    if (Δp == nil || Δp.refs == default!) {
        return;
    }
    foreach (var (i, _) in Δp.refs) {
        setPinned(Δp.refs[i], false);
    }
    // The following two lines make all pointers to references
    // in p.refs unreachable, either by deleting them or dropping
    // p.refs' backing store (if it was not backed by refStore).
    Δp.refStore = new @unsafe.Pointer[]{}.array();
    Δp.refs = Δp.refStore[..0];
}

internal static @unsafe.Pointer pinnerGetPtr(ж<any> Ꮡi) {
    ref var i = ref Ꮡi.val;

    var e = efaceOf(Ꮡi);
    var etyp = e.val._type;
    if (etyp == nil) {
        throw panic(((errorString)"runtime.Pinner: argument is nil"u8));
    }
    {
        var kind = (abiꓸKind)((~etyp).Kind_ & abi.KindMask); if (kind != abi.Pointer && kind != abi.UnsafePointer) {
            throw panic(((errorString)("runtime.Pinner: argument is not a pointer: "u8 + toRType(etyp).@string())));
        }
    }
    if (inUserArenaChunk(((uintptr)(~e).data))) {
        // Arena-allocated objects are not eligible for pinning.
        throw panic(((errorString)"runtime.Pinner: object was allocated into an arena"u8));
    }
    return (~e).data;
}

// isPinned checks if a Go pointer is pinned.
// nosplit, because it's called from nosplit code in cgocheck.
//
//go:nosplit
internal static bool isPinned(@unsafe.Pointer ptr) {
    var span = spanOfHeap(((uintptr)ptr));
    if (span == nil) {
        // this code is only called for Go pointer, so this must be a
        // linker-allocated global object.
        return true;
    }
    var pinnerBits = span.getPinnerBits();
    // these pinnerBits might get unlinked by a concurrently running sweep, but
    // that's OK because gcBits don't get cleared until the following GC cycle
    // (nextMarkBitArenaEpoch)
    if (pinnerBits == nil) {
        return false;
    }
    var objIndex = span.objIndex(((uintptr)ptr));
    var pinState = pinnerBits.ofObject(objIndex);
    KeepAlive(ptr);
    // make sure ptr is alive until we are done so the span can't be freed
    return pinState.isPinned();
}

// setPinned marks or unmarks a Go pointer as pinned, when the ptr is a Go pointer.
// It will be ignored while try to pin a non-Go pointer,
// and it will be panic while try to unpin a non-Go pointer,
// which should not happen in normal usage.
internal static bool setPinned(@unsafe.Pointer ptr, bool pin) {
    var span = spanOfHeap(((uintptr)ptr));
    if (span == nil) {
        if (!pin) {
            throw panic(((errorString)"tried to unpin non-Go pointer"u8));
        }
        // This is a linker-allocated, zero size object or other object,
        // nothing to do, silently ignore it.
        return false;
    }
    // ensure that the span is swept, b/c sweeping accesses the specials list
    // w/o locks.
    var mp = acquirem();
    span.ensureSwept();
    KeepAlive(ptr);
    // make sure ptr is still alive after span is swept
    var objIndex = span.objIndex(((uintptr)ptr));
    @lock(Ꮡ((~span).speciallock));
    // guard against concurrent calls of setPinned on same span
    var pinnerBits = span.getPinnerBits();
    if (pinnerBits == nil) {
        pinnerBits = span.newPinnerBits();
        span.setPinnerBits(pinnerBits);
    }
    var pinState = pinnerBits.ofObject(objIndex);
    if (pin){
        if (pinState.isPinned()){
            // multiple pins on same object, set multipin bit
            pinState.setMultiPinned(true);
            // and increase the pin counter
            // TODO(mknyszek): investigate if systemstack is necessary here
            systemstack(
            var spanʗ2 = span;
            () => {
                var offset = objIndex * (~spanʗ2).elemsize;
                spanʗ2.incPinCounter(offset);
            });
        } else {
            // set pin bit
            pinState.setPinned(true);
        }
    } else {
        // unpin
        if (pinState.isPinned()){
            if (pinState.isMultiPinned()){
                bool exists = default!;
                // TODO(mknyszek): investigate if systemstack is necessary here
                systemstack(
                var spanʗ5 = span;
                () => {
                    var offset = objIndex * (~spanʗ5).elemsize;
                    exists = spanʗ5.decPinCounter(offset);
                });
                if (!exists) {
                    // counter is 0, clear multipin bit
                    pinState.setMultiPinned(false);
                }
            } else {
                // no multipins recorded. unpin object.
                pinState.setPinned(false);
            }
        } else {
            // unpinning unpinned object, bail out
            @throw("runtime.Pinner: object already unpinned"u8);
        }
    }
    unlock(Ꮡ((~span).speciallock));
    releasem(mp);
    return true;
}

[GoType] partial struct pinState {
    internal ж<uint8> bytep;
    internal uint8 byteVal;
    internal uint8 mask;
}

// nosplit, because it's called by isPinned, which is nosplit
//
//go:nosplit
[GoRecv] internal static bool isPinned(this ref pinState v) {
    return ((uint8)(v.byteVal & v.mask)) != 0;
}

[GoRecv] internal static bool isMultiPinned(this ref pinState v) {
    return ((uint8)(v.byteVal & (v.mask << (int)(1)))) != 0;
}

[GoRecv] internal static void setPinned(this ref pinState v, bool val) {
    v.set(val, false);
}

[GoRecv] internal static void setMultiPinned(this ref pinState v, bool val) {
    v.set(val, true);
}

// set sets the pin bit of the pinState to val. If multipin is true, it
// sets/unsets the multipin bit instead.
[GoRecv] internal static void set(this ref pinState v, bool val, bool multipin) {
    var mask = v.mask;
    if (multipin) {
        mask <<= (UntypedInt)(1);
    }
    if (val){
        atomic.Or8(v.bytep, mask);
    } else {
        atomic.And8(v.bytep, ^mask);
    }
}

[GoType("struct{_ runtime.@internal.sys.NotInHeap; x uint8}")] partial struct pinnerBits;

// ofObject returns the pinState of the n'th object.
// nosplit, because it's called by isPinned, which is nosplit
//
//go:nosplit
[GoRecv] internal static pinState ofObject(this ref pinnerBits Δp, uintptr n) {
    var (bytep, mask) = (((ж<gcBits>)(Δp?.val ?? default!))).val.bitp(n * 2);
    var byteVal = atomic.Load8(bytep);
    return new pinState(bytep, byteVal, mask);
}

[GoRecv] internal static uintptr pinnerBitSize(this ref mspan s) {
    return divRoundUp(((uintptr)s.nelems) * 2, 8);
}

// newPinnerBits returns a pointer to 8 byte aligned bytes to be used for this
// span's pinner bits. newPinnerBits is used to mark objects that are pinned.
// They are copied when the span is swept.
[GoRecv] internal static ж<pinnerBits> newPinnerBits(this ref mspan s) {
    return ((ж<pinnerBits>)(newMarkBits(((uintptr)s.nelems) * 2)?.val ?? default!));
}

// nosplit, because it's called by isPinned, which is nosplit
//
//go:nosplit
[GoRecv] internal static ж<pinnerBits> getPinnerBits(this ref mspan s) {
    return (ж<pinnerBits>)(uintptr)(atomic.Loadp(((@unsafe.Pointer)(Ꮡ(s.pinnerBits)))));
}

[GoRecv] internal static void setPinnerBits(this ref mspan s, ж<pinnerBits> Ꮡp) {
    ref var Δp = ref Ꮡp.val;

    atomicstorep(((@unsafe.Pointer)(Ꮡ(s.pinnerBits))), new @unsafe.Pointer(Ꮡp));
}

// refreshPinnerBits replaces pinnerBits with a fresh copy in the arenas for the
// next GC cycle. If it does not contain any pinned objects, pinnerBits of the
// span is set to nil.
[GoRecv] internal static void refreshPinnerBits(this ref mspan s) {
    var Δp = s.getPinnerBits();
    if (Δp == nil) {
        return;
    }
    var hasPins = false;
    var bytes = alignUp(s.pinnerBitSize(), 8);
    // Iterate over each 8-byte chunk and check for pins. Note that
    // newPinnerBits guarantees that pinnerBits will be 8-byte aligned, so we
    // don't have to worry about edge cases, irrelevant bits will simply be
    // zero.
    foreach (var (_, x) in @unsafe.Slice((ж<uint64>)(uintptr)(new @unsafe.Pointer(Ꮡ((~Δp).x))), bytes / 8)) {
        if (x != 0) {
            hasPins = true;
            break;
        }
    }
    if (hasPins){
        var newPinnerBits = s.newPinnerBits();
        memmove(new @unsafe.Pointer(Ꮡ((~newPinnerBits).x)), new @unsafe.Pointer(Ꮡ((~Δp).x)), bytes);
        s.setPinnerBits(newPinnerBits);
    } else {
        s.setPinnerBits(nil);
    }
}

// incPinCounter is only called for multiple pins of the same object and records
// the _additional_ pins.
[GoRecv] internal static void incPinCounter(this ref mspan span, uintptr offset) {
    ж<specialPinCounter> rec = default!;
    var (@ref, exists) = span.specialFindSplicePoint(offset, _KindSpecialPinCounter);
    if (!exists){
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        rec = (ж<specialPinCounter>)(uintptr)(mheap_.specialPinCounterAlloc.alloc());
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        // splice in record, fill in offset.
        (~rec).special.offset = ((uint16)offset);
        (~rec).special.kind = _KindSpecialPinCounter;
        (~rec).special.next = @ref.val;
        @ref.val = (ж<special>)(uintptr)(new @unsafe.Pointer(rec));
        spanHasSpecials(span);
    } else {
        rec = (ж<specialPinCounter>)(uintptr)(new @unsafe.Pointer(@ref.val));
    }
    (~rec).counter++;
}

// decPinCounter decreases the counter. If the counter reaches 0, the counter
// special is deleted and false is returned. Otherwise true is returned.
[GoRecv] internal static bool decPinCounter(this ref mspan span, uintptr offset) {
    var (@ref, exists) = span.specialFindSplicePoint(offset, _KindSpecialPinCounter);
    if (!exists) {
        @throw("runtime.Pinner: decreased non-existing pin counter"u8);
    }
    var counter = (ж<specialPinCounter>)(uintptr)(new @unsafe.Pointer(@ref.val));
    (~counter).counter--;
    if ((~counter).counter == 0) {
        @ref.val = (~counter).special.next;
        if (span.specials == nil) {
            spanHasNoSpecials(span);
        }
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        mheap_.specialPinCounterAlloc.free(new @unsafe.Pointer(counter));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        return false;
    }
    return true;
}

// only for tests
internal static ж<uintptr> pinnerGetPinCounter(@unsafe.Pointer addr) {
    var (_, span, objIndex) = findObject(((uintptr)addr), 0, 0);
    var offset = objIndex * (~span).elemsize;
    var (t, exists) = span.specialFindSplicePoint(offset, _KindSpecialPinCounter);
    if (!exists) {
        return default!;
    }
    var counter = (ж<specialPinCounter>)(uintptr)(new @unsafe.Pointer(t.val));
    return Ꮡ((~counter).counter);
}

// to be able to test that the GC panics when a pinned pointer is leaking, this
// panic function is a variable, that can be overwritten by a test.
internal static Action pinnerLeakPanic = () => {
    throw panic(((errorString)"runtime.Pinner: found leaking pinned pointer; forgot to call Unpin()?"u8));
};

} // end runtime_package
