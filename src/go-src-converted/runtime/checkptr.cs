// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:08:25 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\checkptr.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static void checkptrAlignment(unsafe.Pointer p, ptr<_type> _addr_elem, System.UIntPtr n) {
    ref _type elem = ref _addr_elem.val;
 
    // nil pointer is always suitably aligned (#47430).
    if (p == null) {
        return ;
    }
    if (elem.ptrdata != 0 && uintptr(p) & (uintptr(elem.align) - 1) != 0) {
        throw("checkptr: misaligned pointer conversion");
    }
    if (checkptrStraddles(p, n * elem.size)) {
        throw("checkptr: converted pointer straddles multiple allocations");
    }
}

// checkptrStraddles reports whether the first size-bytes of memory
// addressed by ptr is known to straddle more than one Go allocation.
private static bool checkptrStraddles(unsafe.Pointer ptr, System.UIntPtr size) {
    if (size <= 1) {
        return false;
    }
    if (uintptr(ptr) >= -(size - 1)) {
        return true;
    }
    var end = add(ptr, size - 1); 

    // TODO(mdempsky): Detect when [ptr, end] contains Go allocations,
    // but neither ptr nor end point into one themselves.

    return checkptrBase(ptr) != checkptrBase(end);

}

private static void checkptrArithmetic(unsafe.Pointer p, slice<unsafe.Pointer> originals) {
    if (0 < uintptr(p) && uintptr(p) < minLegalPointer) {
        throw("checkptr: pointer arithmetic computed bad pointer value");
    }
    var @base = checkptrBase(p);
    if (base == 0) {
        return ;
    }
    foreach (var (_, original) in originals) {
        if (base == checkptrBase(original)) {
            return ;
        }
    }    throw("checkptr: pointer arithmetic result points to invalid allocation");

}

// checkptrBase returns the base address for the allocation containing
// the address p.
//
// Importantly, if p1 and p2 point into the same variable, then
// checkptrBase(p1) == checkptrBase(p2). However, the converse/inverse
// is not necessarily true as allocations can have trailing padding,
// and multiple variables may be packed into a single allocation.
private static System.UIntPtr checkptrBase(unsafe.Pointer p) { 
    // stack
    {
        var gp = getg();

        if (gp.stack.lo <= uintptr(p) && uintptr(p) < gp.stack.hi) { 
            // TODO(mdempsky): Walk the stack to identify the
            // specific stack frame or even stack object that p
            // points into.
            //
            // In the mean time, use "1" as a pseudo-address to
            // represent the stack. This is an invalid address on
            // all platforms, so it's guaranteed to be distinct
            // from any of the addresses we might return below.
            return 1;

        }
    } 

    // heap (must check after stack because of #35068)
    {
        var (base, _, _) = findObject(uintptr(p), 0, 0);

        if (base != 0) {
            return base;
        }
    } 

    // data or bss
    foreach (var (_, datap) in activeModules()) {
        if (datap.data <= uintptr(p) && uintptr(p) < datap.edata) {
            return datap.data;
        }
        if (datap.bss <= uintptr(p) && uintptr(p) < datap.ebss) {
            return datap.bss;
        }
    }    return 0;

}

} // end runtime_package
