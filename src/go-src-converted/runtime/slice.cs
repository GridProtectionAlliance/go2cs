// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:11:51 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\slice.go
using math = go.runtime.@internal.math_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private partial struct slice {
    public unsafe.Pointer array;
    public nint len;
    public nint cap;
}

// A notInHeapSlice is a slice backed by go:notinheap memory.
private partial struct notInHeapSlice {
    public ptr<notInHeap> array;
    public nint len;
    public nint cap;
}

private static void panicmakeslicelen() => func((_, panic, _) => {
    panic(errorString("makeslice: len out of range"));
});

private static void panicmakeslicecap() => func((_, panic, _) => {
    panic(errorString("makeslice: cap out of range"));
});

// makeslicecopy allocates a slice of "tolen" elements of type "et",
// then copies "fromlen" elements of type "et" into that new allocation from "from".
private static unsafe.Pointer makeslicecopy(ptr<_type> _addr_et, nint tolen, nint fromlen, unsafe.Pointer from) {
    ref _type et = ref _addr_et.val;

    System.UIntPtr tomem = default;    System.UIntPtr copymem = default;

    if (uintptr(tolen) > uintptr(fromlen)) {
        bool overflow = default;
        tomem, overflow = math.MulUintptr(et.size, uintptr(tolen));
        if (overflow || tomem > maxAlloc || tolen < 0) {
            panicmakeslicelen();
        }
        copymem = et.size * uintptr(fromlen);

    }
    else
 { 
        // fromlen is a known good length providing and equal or greater than tolen,
        // thereby making tolen a good slice length too as from and to slices have the
        // same element width.
        tomem = et.size * uintptr(tolen);
        copymem = tomem;

    }
    unsafe.Pointer to = default;
    if (et.ptrdata == 0) {
        to = mallocgc(tomem, null, false);
        if (copymem < tomem) {
            memclrNoHeapPointers(add(to, copymem), tomem - copymem);
        }
    }
    else
 { 
        // Note: can't use rawmem (which avoids zeroing of memory), because then GC can scan uninitialized memory.
        to = mallocgc(tomem, et, true);
        if (copymem > 0 && writeBarrier.enabled) { 
            // Only shade the pointers in old.array since we know the destination slice to
            // only contains nil pointers because it has been cleared during alloc.
            bulkBarrierPreWriteSrcOnly(uintptr(to), uintptr(from), copymem);

        }
    }
    if (raceenabled) {
        var callerpc = getcallerpc();
        var pc = funcPC(makeslicecopy);
        racereadrangepc(from, copymem, callerpc, pc);
    }
    if (msanenabled) {
        msanread(from, copymem);
    }
    memmove(to, from, copymem);

    return to;

}

private static unsafe.Pointer makeslice(ptr<_type> _addr_et, nint len, nint cap) {
    ref _type et = ref _addr_et.val;

    var (mem, overflow) = math.MulUintptr(et.size, uintptr(cap));
    if (overflow || mem > maxAlloc || len < 0 || len > cap) { 
        // NOTE: Produce a 'len out of range' error instead of a
        // 'cap out of range' error when someone does make([]T, bignumber).
        // 'cap out of range' is true too, but since the cap is only being
        // supplied implicitly, saying len is clearer.
        // See golang.org/issue/4085.
        (mem, overflow) = math.MulUintptr(et.size, uintptr(len));
        if (overflow || mem > maxAlloc || len < 0) {
            panicmakeslicelen();
        }
        panicmakeslicecap();

    }
    return mallocgc(mem, et, true);

}

private static unsafe.Pointer makeslice64(ptr<_type> _addr_et, long len64, long cap64) {
    ref _type et = ref _addr_et.val;

    var len = int(len64);
    if (int64(len) != len64) {
        panicmakeslicelen();
    }
    var cap = int(cap64);
    if (int64(cap) != cap64) {
        panicmakeslicecap();
    }
    return makeslice(_addr_et, len, cap);

}

private static void unsafeslice(ptr<_type> _addr_et, unsafe.Pointer ptr, nint len) => func((_, panic, _) => {
    ref _type et = ref _addr_et.val;

    if (len == 0) {
        return ;
    }
    if (ptr == null) {
        panic(errorString("unsafe.Slice: ptr is nil and len is not zero"));
    }
    var (mem, overflow) = math.MulUintptr(et.size, uintptr(len));
    if (overflow || mem > maxAlloc || len < 0) {
        panicunsafeslicelen();
    }
});

private static void unsafeslice64(ptr<_type> _addr_et, unsafe.Pointer ptr, long len64) {
    ref _type et = ref _addr_et.val;

    var len = int(len64);
    if (int64(len) != len64) {
        panicunsafeslicelen();
    }
    unsafeslice(_addr_et, ptr, len);

}

private static void unsafeslicecheckptr(ptr<_type> _addr_et, unsafe.Pointer ptr, long len64) {
    ref _type et = ref _addr_et.val;

    unsafeslice64(_addr_et, ptr, len64); 

    // Check that underlying array doesn't straddle multiple heap objects.
    // unsafeslice64 has already checked for overflow.
    if (checkptrStraddles(ptr, uintptr(len64) * et.size)) {
        throw("checkptr: unsafe.Slice result straddles multiple allocations");
    }
}

private static void panicunsafeslicelen() => func((_, panic, _) => {
    panic(errorString("unsafe.Slice: len out of range"));
});

// growslice handles slice growth during append.
// It is passed the slice element type, the old slice, and the desired new minimum capacity,
// and it returns a new slice with at least that capacity, with the old data
// copied into it.
// The new slice's length is set to the old slice's length,
// NOT to the new requested capacity.
// This is for codegen convenience. The old slice's length is used immediately
// to calculate where to write new values during an append.
// TODO: When the old backend is gone, reconsider this decision.
// The SSA backend might prefer the new length or to return only ptr/cap and save stack space.
private static slice growslice(ptr<_type> _addr_et, slice old, nint cap) => func((_, panic, _) => {
    ref _type et = ref _addr_et.val;

    if (raceenabled) {
        var callerpc = getcallerpc();
        racereadrangepc(old.array, uintptr(old.len * int(et.size)), callerpc, funcPC(growslice));
    }
    if (msanenabled) {
        msanread(old.array, uintptr(old.len * int(et.size)));
    }
    if (cap < old.cap) {
        panic(errorString("growslice: cap out of range"));
    }
    if (et.size == 0) { 
        // append should not create a slice with nil pointer but non-zero len.
        // We assume that append doesn't need to preserve old.array in this case.
        return new slice(unsafe.Pointer(&zerobase),old.len,cap);

    }
    var newcap = old.cap;
    var doublecap = newcap + newcap;
    if (cap > doublecap) {
        newcap = cap;
    }
    else
 {
        if (old.cap < 1024) {
            newcap = doublecap;
        }
        else
 { 
            // Check 0 < newcap to detect overflow
            // and prevent an infinite loop.
            while (0 < newcap && newcap < cap) {
                newcap += newcap / 4;
            } 
            // Set newcap to the requested cap when
            // the newcap calculation overflowed.
 
            // Set newcap to the requested cap when
            // the newcap calculation overflowed.
            if (newcap <= 0) {
                newcap = cap;
            }

        }
    }
    bool overflow = default;
    System.UIntPtr lenmem = default;    System.UIntPtr newlenmem = default;    System.UIntPtr capmem = default; 
    // Specialize for common values of et.size.
    // For 1 we don't need any division/multiplication.
    // For sys.PtrSize, compiler will optimize division/multiplication into a shift by a constant.
    // For powers of 2, use a variable shift.
 
    // Specialize for common values of et.size.
    // For 1 we don't need any division/multiplication.
    // For sys.PtrSize, compiler will optimize division/multiplication into a shift by a constant.
    // For powers of 2, use a variable shift.

    if (et.size == 1) 
        lenmem = uintptr(old.len);
        newlenmem = uintptr(cap);
        capmem = roundupsize(uintptr(newcap));
        overflow = uintptr(newcap) > maxAlloc;
        newcap = int(capmem);
    else if (et.size == sys.PtrSize) 
        lenmem = uintptr(old.len) * sys.PtrSize;
        newlenmem = uintptr(cap) * sys.PtrSize;
        capmem = roundupsize(uintptr(newcap) * sys.PtrSize);
        overflow = uintptr(newcap) > maxAlloc / sys.PtrSize;
        newcap = int(capmem / sys.PtrSize);
    else if (isPowerOfTwo(et.size)) 
        System.UIntPtr shift = default;
        if (sys.PtrSize == 8) { 
            // Mask shift for better code generation.
            shift = uintptr(sys.Ctz64(uint64(et.size))) & 63;

        }
        else
 {
            shift = uintptr(sys.Ctz32(uint32(et.size))) & 31;
        }
        lenmem = uintptr(old.len) << (int)(shift);
        newlenmem = uintptr(cap) << (int)(shift);
        capmem = roundupsize(uintptr(newcap) << (int)(shift));
        overflow = uintptr(newcap) > (maxAlloc >> (int)(shift));
        newcap = int(capmem >> (int)(shift));
    else 
        lenmem = uintptr(old.len) * et.size;
        newlenmem = uintptr(cap) * et.size;
        capmem, overflow = math.MulUintptr(et.size, uintptr(newcap));
        capmem = roundupsize(capmem);
        newcap = int(capmem / et.size);
    // The check of overflow in addition to capmem > maxAlloc is needed
    // to prevent an overflow which can be used to trigger a segfault
    // on 32bit architectures with this example program:
    //
    // type T [1<<27 + 1]int64
    //
    // var d T
    // var s []T
    //
    // func main() {
    //   s = append(s, d, d, d, d)
    //   print(len(s), "\n")
    // }
    if (overflow || capmem > maxAlloc) {
        panic(errorString("growslice: cap out of range"));
    }
    unsafe.Pointer p = default;
    if (et.ptrdata == 0) {
        p = mallocgc(capmem, null, false); 
        // The append() that calls growslice is going to overwrite from old.len to cap (which will be the new length).
        // Only clear the part that will not be overwritten.
        memclrNoHeapPointers(add(p, newlenmem), capmem - newlenmem);

    }
    else
 { 
        // Note: can't use rawmem (which avoids zeroing of memory), because then GC can scan uninitialized memory.
        p = mallocgc(capmem, et, true);
        if (lenmem > 0 && writeBarrier.enabled) { 
            // Only shade the pointers in old.array since we know the destination slice p
            // only contains nil pointers because it has been cleared during alloc.
            bulkBarrierPreWriteSrcOnly(uintptr(p), uintptr(old.array), lenmem - et.size + et.ptrdata);

        }
    }
    memmove(p, old.array, lenmem);

    return new slice(p,old.len,newcap);

});

private static bool isPowerOfTwo(System.UIntPtr x) {
    return x & (x - 1) == 0;
}

// slicecopy is used to copy from a string or slice of pointerless elements into a slice.
private static nint slicecopy(unsafe.Pointer toPtr, nint toLen, unsafe.Pointer fromPtr, nint fromLen, System.UIntPtr width) {
    if (fromLen == 0 || toLen == 0) {
        return 0;
    }
    var n = fromLen;
    if (toLen < n) {
        n = toLen;
    }
    if (width == 0) {
        return n;
    }
    var size = uintptr(n) * width;
    if (raceenabled) {
        var callerpc = getcallerpc();
        var pc = funcPC(slicecopy);
        racereadrangepc(fromPtr, size, callerpc, pc);
        racewriterangepc(toPtr, size, callerpc, pc);
    }
    if (msanenabled) {
        msanread(fromPtr, size);
        msanwrite(toPtr, size);
    }
    if (size == 1) { // common case worth about 2x to do here
        // TODO: is this still worth it with new memmove impl?
        (byte.val)(toPtr).val;

        new ptr<ptr<ptr<byte>>>(fromPtr); // known to be a byte pointer
    }
    else
 {
        memmove(toPtr, fromPtr, size);
    }
    return n;

}

} // end runtime_package
