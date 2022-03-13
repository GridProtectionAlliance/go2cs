// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:24:08 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\atomic_pointer.go
namespace go;

using atomic = runtime.@internal.atomic_package;
using @unsafe = @unsafe_package;


// These functions cannot have go:noescape annotations,
// because while ptr does not escape, new does.
// If new is marked as not escaping, the compiler will make incorrect
// escape analysis decisions about the pointer value being stored.

// atomicwb performs a write barrier before an atomic pointer write.
// The caller should guard the call with "if writeBarrier.enabled".
//
//go:nosplit

public static partial class runtime_package {

private static void atomicwb(ptr<unsafe.Pointer> _addr_ptr, unsafe.Pointer @new) {
    ref unsafe.Pointer ptr = ref _addr_ptr.val;

    var slot = (uintptr.val)(@unsafe.Pointer(ptr));
    if (!getg().m.p.ptr().wbBuf.putFast(slot.val, uintptr(new))) {
        wbBufFlush(slot, uintptr(new));
    }
}

// atomicstorep performs *ptr = new atomically and invokes a write barrier.
//
//go:nosplit
private static void atomicstorep(unsafe.Pointer ptr, unsafe.Pointer @new) {
    if (writeBarrier.enabled) {
        atomicwb(_addr_(@unsafe.Pointer.val)(ptr), new);
    }
    atomic.StorepNoWB(noescape(ptr), new);
}

// Like above, but implement in terms of sync/atomic's uintptr operations.
// We cannot just call the runtime routines, because the race detector expects
// to be able to intercept the sync/atomic forms but not the runtime forms.

//go:linkname sync_atomic_StoreUintptr sync/atomic.StoreUintptr
private static void sync_atomic_StoreUintptr(ptr<System.UIntPtr> ptr, System.UIntPtr @new);

//go:linkname sync_atomic_StorePointer sync/atomic.StorePointer
//go:nosplit
private static void sync_atomic_StorePointer(ptr<unsafe.Pointer> _addr_ptr, unsafe.Pointer @new) {
    ref unsafe.Pointer ptr = ref _addr_ptr.val;

    if (writeBarrier.enabled) {>>MARKER:FUNCTION_sync_atomic_StoreUintptr_BLOCK_PREFIX<<
        atomicwb(_addr_ptr, new);
    }
    sync_atomic_StoreUintptr(_addr_(uintptr.val)(@unsafe.Pointer(ptr)), uintptr(new));
}

//go:linkname sync_atomic_SwapUintptr sync/atomic.SwapUintptr
private static System.UIntPtr sync_atomic_SwapUintptr(ptr<System.UIntPtr> ptr, System.UIntPtr @new);

//go:linkname sync_atomic_SwapPointer sync/atomic.SwapPointer
//go:nosplit
private static unsafe.Pointer sync_atomic_SwapPointer(ptr<unsafe.Pointer> _addr_ptr, unsafe.Pointer @new) {
    ref unsafe.Pointer ptr = ref _addr_ptr.val;

    if (writeBarrier.enabled) {>>MARKER:FUNCTION_sync_atomic_SwapUintptr_BLOCK_PREFIX<<
        atomicwb(_addr_ptr, new);
    }
    var old = @unsafe.Pointer(sync_atomic_SwapUintptr(_addr_(uintptr.val)(noescape(@unsafe.Pointer(ptr))), uintptr(new)));
    return old;
}

//go:linkname sync_atomic_CompareAndSwapUintptr sync/atomic.CompareAndSwapUintptr
private static bool sync_atomic_CompareAndSwapUintptr(ptr<System.UIntPtr> ptr, System.UIntPtr old, System.UIntPtr @new);

//go:linkname sync_atomic_CompareAndSwapPointer sync/atomic.CompareAndSwapPointer
//go:nosplit
private static bool sync_atomic_CompareAndSwapPointer(ptr<unsafe.Pointer> _addr_ptr, unsafe.Pointer old, unsafe.Pointer @new) {
    ref unsafe.Pointer ptr = ref _addr_ptr.val;

    if (writeBarrier.enabled) {>>MARKER:FUNCTION_sync_atomic_CompareAndSwapUintptr_BLOCK_PREFIX<<
        atomicwb(_addr_ptr, new);
    }
    return sync_atomic_CompareAndSwapUintptr(_addr_(uintptr.val)(noescape(@unsafe.Pointer(ptr))), uintptr(old), uintptr(new));
}

} // end runtime_package
