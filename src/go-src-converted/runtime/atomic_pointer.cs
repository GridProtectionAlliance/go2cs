// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:16:28 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\atomic_pointer.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // These functions cannot have go:noescape annotations,
        // because while ptr does not escape, new does.
        // If new is marked as not escaping, the compiler will make incorrect
        // escape analysis decisions about the pointer value being stored.
        // Instead, these are wrappers around the actual atomics (casp1 and so on)
        // that use noescape to convey which arguments do not escape.

        // atomicstorep performs *ptr = new atomically and invokes a write barrier.
        //
        //go:nosplit
        private static void atomicstorep(unsafe.Pointer ptr, unsafe.Pointer @new)
        {
            writebarrierptr_prewrite((uintptr.Value)(ptr), uintptr(new));
            atomic.StorepNoWB(noescape(ptr), new);
        }

        //go:nosplit
        private static bool casp(ref unsafe.Pointer ptr, unsafe.Pointer old, unsafe.Pointer @new)
        { 
            // The write barrier is only necessary if the CAS succeeds,
            // but since it needs to happen before the write becomes
            // public, we have to do it conservatively all the time.
            writebarrierptr_prewrite((uintptr.Value)(@unsafe.Pointer(ptr)), uintptr(new));
            return atomic.Casp1((@unsafe.Pointer.Value)(noescape(@unsafe.Pointer(ptr))), noescape(old), new);
        }

        // Like above, but implement in terms of sync/atomic's uintptr operations.
        // We cannot just call the runtime routines, because the race detector expects
        // to be able to intercept the sync/atomic forms but not the runtime forms.

        //go:linkname sync_atomic_StoreUintptr sync/atomic.StoreUintptr
        private static void sync_atomic_StoreUintptr(ref System.UIntPtr ptr, System.UIntPtr @new)
;

        //go:linkname sync_atomic_StorePointer sync/atomic.StorePointer
        //go:nosplit
        private static void sync_atomic_StorePointer(ref unsafe.Pointer ptr, unsafe.Pointer @new)
        {
            writebarrierptr_prewrite((uintptr.Value)(@unsafe.Pointer(ptr)), uintptr(new));
            sync_atomic_StoreUintptr((uintptr.Value)(@unsafe.Pointer(ptr)), uintptr(new));
        }

        //go:linkname sync_atomic_SwapUintptr sync/atomic.SwapUintptr
        private static System.UIntPtr sync_atomic_SwapUintptr(ref System.UIntPtr ptr, System.UIntPtr @new)
;

        //go:linkname sync_atomic_SwapPointer sync/atomic.SwapPointer
        //go:nosplit
        private static unsafe.Pointer sync_atomic_SwapPointer(ref unsafe.Pointer ptr, unsafe.Pointer @new)
        {
            writebarrierptr_prewrite((uintptr.Value)(@unsafe.Pointer(ptr)), uintptr(new));
            var old = @unsafe.Pointer(sync_atomic_SwapUintptr((uintptr.Value)(noescape(@unsafe.Pointer(ptr))), uintptr(new)));
            return old;
        }

        //go:linkname sync_atomic_CompareAndSwapUintptr sync/atomic.CompareAndSwapUintptr
        private static bool sync_atomic_CompareAndSwapUintptr(ref System.UIntPtr ptr, System.UIntPtr old, System.UIntPtr @new)
;

        //go:linkname sync_atomic_CompareAndSwapPointer sync/atomic.CompareAndSwapPointer
        //go:nosplit
        private static bool sync_atomic_CompareAndSwapPointer(ref unsafe.Pointer ptr, unsafe.Pointer old, unsafe.Pointer @new)
        {
            writebarrierptr_prewrite((uintptr.Value)(@unsafe.Pointer(ptr)), uintptr(new));
            return sync_atomic_CompareAndSwapUintptr((uintptr.Value)(noescape(@unsafe.Pointer(ptr))), uintptr(old), uintptr(new));
        }
    }
}
