// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:45:39 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\checkptr.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void checkptrAlignment(unsafe.Pointer p, ptr<_type> _addr_elem, System.UIntPtr n)
        {
            ref _type elem = ref _addr_elem.val;
 
            // Check that (*[n]elem)(p) is appropriately aligned.
            // Note that we allow unaligned pointers if the types they point to contain
            // no pointers themselves. See issue 37298.
            // TODO(mdempsky): What about fieldAlign?
            if (elem.ptrdata != 0L && uintptr(p) & (uintptr(elem.align) - 1L) != 0L)
            {
                throw("checkptr: misaligned pointer conversion");
            }
            {
                var size = n * elem.size;

                if (size > 1L && checkptrBase(p) != checkptrBase(add(p, size - 1L)))
                {
                    throw("checkptr: converted pointer straddles multiple allocations");
                }
            }

        }

        private static void checkptrArithmetic(unsafe.Pointer p, slice<unsafe.Pointer> originals)
        {
            if (0L < uintptr(p) && uintptr(p) < minLegalPointer)
            {
                throw("checkptr: pointer arithmetic computed bad pointer value");
            } 

            // Check that if the computed pointer p points into a heap
            // object, then one of the original pointers must have pointed
            // into the same object.
            var @base = checkptrBase(p);
            if (base == 0L)
            {
                return ;
            }

            foreach (var (_, original) in originals)
            {
                if (base == checkptrBase(original))
                {
                    return ;
                }

            }
            throw("checkptr: pointer arithmetic result points to invalid allocation");

        }

        // checkptrBase returns the base address for the allocation containing
        // the address p.
        //
        // Importantly, if p1 and p2 point into the same variable, then
        // checkptrBase(p1) == checkptrBase(p2). However, the converse/inverse
        // is not necessarily true as allocations can have trailing padding,
        // and multiple variables may be packed into a single allocation.
        private static System.UIntPtr checkptrBase(unsafe.Pointer p)
        { 
            // stack
            {
                var gp = getg();

                if (gp.stack.lo <= uintptr(p) && uintptr(p) < gp.stack.hi)
                { 
                    // TODO(mdempsky): Walk the stack to identify the
                    // specific stack frame or even stack object that p
                    // points into.
                    //
                    // In the mean time, use "1" as a pseudo-address to
                    // represent the stack. This is an invalid address on
                    // all platforms, so it's guaranteed to be distinct
                    // from any of the addresses we might return below.
                    return 1L;

                } 

                // heap (must check after stack because of #35068)

            } 

            // heap (must check after stack because of #35068)
            {
                var (base, _, _) = findObject(uintptr(p), 0L, 0L);

                if (base != 0L)
                {
                    return base;
                } 

                // data or bss

            } 

            // data or bss
            foreach (var (_, datap) in activeModules())
            {
                if (datap.data <= uintptr(p) && uintptr(p) < datap.edata)
                {
                    return datap.data;
                }

                if (datap.bss <= uintptr(p) && uintptr(p) < datap.ebss)
                {
                    return datap.bss;
                }

            }
            return 0L;

        }
    }
}
