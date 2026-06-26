// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Code to check that pointer writes follow the cgo rules.
// These functions are invoked when GOEXPERIMENT=cgocheck2 is enabled.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

internal static readonly @string cgoWriteBarrierFail = "unpinned Go pointer stored into non-Go memory"u8;

// cgoCheckPtrWrite is called whenever a pointer is stored into memory.
// It throws if the program is storing an unpinned Go pointer into non-Go
// memory.
//
// This is called from generated code when GOEXPERIMENT=cgocheck2 is enabled.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckPtrWrite(ж<@unsafe.Pointer> Ꮡdst, @unsafe.Pointer src) {
    ref var dst = ref Ꮡdst.val;

    if (!mainStarted) {
        // Something early in startup hates this function.
        // Don't start doing any actual checking until the
        // runtime has set itself up.
        return;
    }
    if (!cgoIsGoPointer(src.val)) {
        return;
    }
    if (cgoIsGoPointer(((@unsafe.Pointer)dst))) {
        return;
    }
    // If we are running on the system stack then dst might be an
    // address on the stack, which is OK.
    var gp = getg();
    if (gp == (~(~gp).m).g0 || gp == (~(~gp).m).gsignal) {
        return;
    }
    // Allocating memory can write to various mfixalloc structs
    // that look like they are non-Go memory.
    if ((~(~gp).m).mallocing != 0) {
        return;
    }
    // If the object is pinned, it's safe to store it in C memory. The GC
    // ensures it will not be moved or freed.
    if (isPinned(src.val)) {
        return;
    }
    // It's OK if writing to memory allocated by persistentalloc.
    // Do this check last because it is more expensive and rarely true.
    // If it is false the expense doesn't matter since we are crashing.
    if (inPersistentAlloc(((uintptr)((@unsafe.Pointer)dst)))) {
        return;
    }
    systemstack(() => {
        println("write of unpinned Go pointer", ((Δhex)((uintptr)src)), "to non-Go memory", ((Δhex)((uintptr)((@unsafe.Pointer)dst))));
        @throw(cgoWriteBarrierFail);
    });
}

// cgoCheckMemmove is called when moving a block of memory.
// It throws if the program is copying a block that contains an unpinned Go
// pointer into non-Go memory.
//
// This is called from generated code when GOEXPERIMENT=cgocheck2 is enabled.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckMemmove(ж<_type> Ꮡtyp, @unsafe.Pointer dst, @unsafe.Pointer src) {
    ref var typ = ref Ꮡtyp.val;

    cgoCheckMemmove2(Ꮡtyp, dst.val, src.val, 0, typ.Size_);
}

// cgoCheckMemmove2 is called when moving a block of memory.
// dst and src point off bytes into the value to copy.
// size is the number of bytes to copy.
// It throws if the program is copying a block that contains an unpinned Go
// pointer into non-Go memory.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckMemmove2(ж<_type> Ꮡtyp, @unsafe.Pointer dst, @unsafe.Pointer src, uintptr off, uintptr size) {
    ref var typ = ref Ꮡtyp.val;

    if (!typ.Pointers()) {
        return;
    }
    if (!cgoIsGoPointer(src.val)) {
        return;
    }
    if (cgoIsGoPointer(dst.val)) {
        return;
    }
    cgoCheckTypedBlock(Ꮡtyp, src.val, off, size);
}

// cgoCheckSliceCopy is called when copying n elements of a slice.
// src and dst are pointers to the first element of the slice.
// typ is the element type of the slice.
// It throws if the program is copying slice elements that contain unpinned Go
// pointers into non-Go memory.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckSliceCopy(ж<_type> Ꮡtyp, @unsafe.Pointer dst, @unsafe.Pointer src, nint n) {
    ref var typ = ref Ꮡtyp.val;

    if (!typ.Pointers()) {
        return;
    }
    if (!cgoIsGoPointer(src.val)) {
        return;
    }
    if (cgoIsGoPointer(dst.val)) {
        return;
    }
    @unsafe.Pointer Δp = src;
    for (nint i = 0; i < n; i++) {
        cgoCheckTypedBlock(Ꮡtyp, Δp, 0, typ.Size_);
        Δp = (uintptr)add(Δp, typ.Size_);
    }
}

// cgoCheckTypedBlock checks the block of memory at src, for up to size bytes,
// and throws if it finds an unpinned Go pointer. The type of the memory is typ,
// and src is off bytes into that type.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckTypedBlock(ж<_type> Ꮡtyp, @unsafe.Pointer src, uintptr off, uintptr size) {
    ref var typ = ref Ꮡtyp.val;

    // Anything past typ.PtrBytes is not a pointer.
    if (typ.PtrBytes <= off) {
        return;
    }
    {
        var ptrdataSize = typ.PtrBytes - off; if (size > ptrdataSize) {
            size = ptrdataSize;
        }
    }
    if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) == 0) {
        cgoCheckBits(src.val, typ.GCData, off, size);
        return;
    }
    // The type has a GC program. Try to find GC bits somewhere else.
    foreach (var (_, datap) in activeModules()) {
        if (cgoInRange(src.val, (~datap).data, (~datap).edata)) {
            var doff = ((uintptr)src) - (~datap).data;
            cgoCheckBits((uintptr)add(src.val, -doff), (~datap).gcdatamask.bytedata, off + doff, size);
            return;
        }
        if (cgoInRange(src.val, (~datap).bss, (~datap).ebss)) {
            var boff = ((uintptr)src) - (~datap).bss;
            cgoCheckBits((uintptr)add(src.val, -boff), (~datap).gcbssmask.bytedata, off + boff, size);
            return;
        }
    }
    var s = spanOfUnchecked(((uintptr)src));
    if ((~s).state.get() == mSpanManual) {
        // There are no heap bits for value stored on the stack.
        // For a channel receive src might be on the stack of some
        // other goroutine, so we can't unwind the stack even if
        // we wanted to.
        // We can't expand the GC program without extra storage
        // space we can't easily get.
        // Fortunately we have the type information.
        systemstack(() => {
            cgoCheckUsingType(Ꮡtyp, src.val, off, size);
        });
        return;
    }
    // src must be in the regular heap.
    var tp = s.typePointersOf(((uintptr)src), size);
    while (ᐧ) {
        uintptr addr = default!;
        {
            (tp, addr) = tp.next(((uintptr)src) + size); if (addr == 0) {
                break;
            }
        }
        @unsafe.Pointer v = ~(ж<@unsafe.Pointer>)(uintptr)(((@unsafe.Pointer)addr));
        if (cgoIsGoPointer(v) && !isPinned(v)) {
            @throw(cgoWriteBarrierFail);
        }
    }
}

// cgoCheckBits checks the block of memory at src, for up to size
// bytes, and throws if it finds an unpinned Go pointer. The gcbits mark each
// pointer value. The src pointer is off bytes into the gcbits.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckBits(@unsafe.Pointer src, ж<byte> Ꮡgcbits, uintptr off, uintptr size) {
    ref var gcbits = ref Ꮡgcbits.val;

    var skipMask = off / goarch.PtrSize / 8;
    var skipBytes = skipMask * goarch.PtrSize * 8;
    var ptrmask = addb(Ꮡgcbits, skipMask);
    src = (uintptr)add(src.val, skipBytes);
    off -= skipBytes;
    size += off;
    uint32 bits = default!;
    for (var i = ((uintptr)0); i < size; i += goarch.PtrSize) {
        if ((uintptr)(i & (goarch.PtrSize * 8 - 1)) == 0){
            bits = ((uint32)(ptrmask.val));
            ptrmask = addb(ptrmask, 1);
        } else {
            bits >>= (UntypedInt)(1);
        }
        if (off > 0){
            off -= goarch.PtrSize;
        } else {
            if ((uint32)(bits & 1) != 0) {
                @unsafe.Pointer v = ~(ж<@unsafe.Pointer>)(uintptr)(add(src.val, i));
                if (cgoIsGoPointer(v) && !isPinned(v)) {
                    @throw(cgoWriteBarrierFail);
                }
            }
        }
    }
}

// cgoCheckUsingType is like cgoCheckTypedBlock, but is a last ditch
// fall back to look for pointers in src using the type information.
// We only use this when looking at a value on the stack when the type
// uses a GC program, because otherwise it's more efficient to use the
// GC bits. This is called on the system stack.
//
//go:nowritebarrier
//go:systemstack
internal static void cgoCheckUsingType(ж<_type> Ꮡtyp, @unsafe.Pointer src, uintptr off, uintptr size) {
    ref var typ = ref Ꮡtyp.val;

    if (!typ.Pointers()) {
        return;
    }
    // Anything past typ.PtrBytes is not a pointer.
    if (typ.PtrBytes <= off) {
        return;
    }
    {
        var ptrdataSize = typ.PtrBytes - off; if (size > ptrdataSize) {
            size = ptrdataSize;
        }
    }
    if ((abiꓸKind)(typ.Kind_ & abi.KindGCProg) == 0) {
        cgoCheckBits(src.val, typ.GCData, off, size);
        return;
    }
    var exprᴛ1 = (abiꓸKind)(typ.Kind_ & abi.KindMask);
    { /* default: */
        @throw("can't happen"u8);
    }
    else if (exprᴛ1 == abi.Array) {
        var at = (ж<arraytype>)(uintptr)(new @unsafe.Pointer(Ꮡtyp));
        for (var i = ((uintptr)0); i < (~at).Len; i++) {
            if (off < (~(~at).Elem).Size_) {
                cgoCheckUsingType((~at).Elem, src.val, off, size);
            }
            src = (uintptr)add(src.val, (~(~at).Elem).Size_);
            var skipped = off;
            if (skipped > (~(~at).Elem).Size_) {
                skipped = (~at).Elem.val.Size_;
            }
            var @checked = (~(~at).Elem).Size_ - skipped;
            off -= skipped;
            if (size <= @checked) {
                return;
            }
            size -= @checked;
        }
    }
    else if (exprᴛ1 == abi.Struct) {
        var st = (ж<structtype>)(uintptr)(new @unsafe.Pointer(Ꮡtyp));
        ref var f = ref heap(new @internal.abi_package.StructField(), out var Ꮡf);

        foreach (var (_, f) in (~st).Fields) {
            if (off < (~f.Typ).Size_) {
                cgoCheckUsingType(f.Typ, src.val, off, size);
            }
            src = (uintptr)add(src.val, (~f.Typ).Size_);
            var skipped = off;
            if (skipped > (~f.Typ).Size_) {
                skipped = f.Typ.val.Size_;
            }
            var @checked = (~f.Typ).Size_ - skipped;
            off -= skipped;
            if (size <= @checked) {
                return;
            }
            size -= @checked;
        }
    }

}

} // end runtime_package
