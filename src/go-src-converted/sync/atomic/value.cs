// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package atomic -- go2cs converted at 2022 March 13 05:24:02 UTC
// import "sync/atomic" ==> using atomic = go.sync.atomic_package
// Original source: C:\Program Files\Go\src\sync\atomic\value.go
namespace go.sync;

using @unsafe = @unsafe_package;


// A Value provides an atomic load and store of a consistently typed value.
// The zero value for a Value returns nil from Load.
// Once Store has been called, a Value must not be copied.
//
// A Value must not be copied after first use.

public static partial class atomic_package {

public partial struct Value {
}

// ifaceWords is interface{} internal representation.
private partial struct ifaceWords {
    public unsafe.Pointer typ;
    public unsafe.Pointer data;
}

// Load returns the value set by the most recent Store.
// It returns nil if there has been no call to Store for this Value.
private static object Load(this ptr<Value> _addr_v) {
    object val = default;
    ref Value v = ref _addr_v.val;

    var vp = (ifaceWords.val)(@unsafe.Pointer(v));
    var typ = LoadPointer(_addr_vp.typ);
    if (typ == null || uintptr(typ) == ~uintptr(0)) { 
        // First store not yet completed.
        return null;
    }
    var data = LoadPointer(_addr_vp.data);
    var vlp = (ifaceWords.val)(@unsafe.Pointer(_addr_val));
    vlp.typ = typ;
    vlp.data = data;
    return ;
}

// Store sets the value of the Value to x.
// All calls to Store for a given Value must use values of the same concrete type.
// Store of an inconsistent type panics, as does Store(nil).
private static void Store(this ptr<Value> _addr_v, object val) => func((_, panic, _) => {
    ref Value v = ref _addr_v.val;

    if (val == null) {
        panic("sync/atomic: store of nil value into Value");
    }
    var vp = (ifaceWords.val)(@unsafe.Pointer(v));
    var vlp = (ifaceWords.val)(@unsafe.Pointer(_addr_val));
    while (true) {
        var typ = LoadPointer(_addr_vp.typ);
        if (typ == null) { 
            // Attempt to start first store.
            // Disable preemption so that other goroutines can use
            // active spin wait to wait for completion; and so that
            // GC does not see the fake type accidentally.
            runtime_procPin();
            if (!CompareAndSwapPointer(_addr_vp.typ, null, @unsafe.Pointer(~uintptr(0)))) {
                runtime_procUnpin();
                continue;
            } 
            // Complete first store.
            StorePointer(_addr_vp.data, vlp.data);
            StorePointer(_addr_vp.typ, vlp.typ);
            runtime_procUnpin();
            return ;
        }
        if (uintptr(typ) == ~uintptr(0)) { 
            // First store in progress. Wait.
            // Since we disable preemption around the first store,
            // we can wait with active spinning.
            continue;
        }
        if (typ != vlp.typ) {
            panic("sync/atomic: store of inconsistently typed value into Value");
        }
        StorePointer(_addr_vp.data, vlp.data);
        return ;
    }
});

// Swap stores new into Value and returns the previous value. It returns nil if
// the Value is empty.
//
// All calls to Swap for a given Value must use values of the same concrete
// type. Swap of an inconsistent type panics, as does Swap(nil).
private static object Swap(this ptr<Value> _addr_v, object @new) => func((_, panic, _) => {
    object old = default;
    ref Value v = ref _addr_v.val;

    if (new == null) {
        panic("sync/atomic: swap of nil value into Value");
    }
    var vp = (ifaceWords.val)(@unsafe.Pointer(v));
    var np = (ifaceWords.val)(@unsafe.Pointer(_addr_new));
    while (true) {
        var typ = LoadPointer(_addr_vp.typ);
        if (typ == null) { 
            // Attempt to start first store.
            // Disable preemption so that other goroutines can use
            // active spin wait to wait for completion; and so that
            // GC does not see the fake type accidentally.
            runtime_procPin();
            if (!CompareAndSwapPointer(_addr_vp.typ, null, @unsafe.Pointer(~uintptr(0)))) {
                runtime_procUnpin();
                continue;
            } 
            // Complete first store.
            StorePointer(_addr_vp.data, np.data);
            StorePointer(_addr_vp.typ, np.typ);
            runtime_procUnpin();
            return null;
        }
        if (uintptr(typ) == ~uintptr(0)) { 
            // First store in progress. Wait.
            // Since we disable preemption around the first store,
            // we can wait with active spinning.
            continue;
        }
        if (typ != np.typ) {
            panic("sync/atomic: swap of inconsistently typed value into Value");
        }
        var op = (ifaceWords.val)(@unsafe.Pointer(_addr_old));
        (op.typ, op.data) = (np.typ, SwapPointer(_addr_vp.data, np.data));        return old;
    }
});

// CompareAndSwap executes the compare-and-swap operation for the Value.
//
// All calls to CompareAndSwap for a given Value must use values of the same
// concrete type. CompareAndSwap of an inconsistent type panics, as does
// CompareAndSwap(old, nil).
private static bool CompareAndSwap(this ptr<Value> _addr_v, object old, object @new) => func((_, panic, _) => {
    bool swapped = default;
    ref Value v = ref _addr_v.val;

    if (new == null) {
        panic("sync/atomic: compare and swap of nil value into Value");
    }
    var vp = (ifaceWords.val)(@unsafe.Pointer(v));
    var np = (ifaceWords.val)(@unsafe.Pointer(_addr_new));
    var op = (ifaceWords.val)(@unsafe.Pointer(_addr_old));
    if (op.typ != null && np.typ != op.typ) {
        panic("sync/atomic: compare and swap of inconsistently typed values");
    }
    while (true) {
        var typ = LoadPointer(_addr_vp.typ);
        if (typ == null) {
            if (old != null) {
                return false;
            } 
            // Attempt to start first store.
            // Disable preemption so that other goroutines can use
            // active spin wait to wait for completion; and so that
            // GC does not see the fake type accidentally.
            runtime_procPin();
            if (!CompareAndSwapPointer(_addr_vp.typ, null, @unsafe.Pointer(~uintptr(0)))) {
                runtime_procUnpin();
                continue;
            } 
            // Complete first store.
            StorePointer(_addr_vp.data, np.data);
            StorePointer(_addr_vp.typ, np.typ);
            runtime_procUnpin();
            return true;
        }
        if (uintptr(typ) == ~uintptr(0)) { 
            // First store in progress. Wait.
            // Since we disable preemption around the first store,
            // we can wait with active spinning.
            continue;
        }
        if (typ != np.typ) {
            panic("sync/atomic: compare and swap of inconsistently typed value into Value");
        }
        var data = LoadPointer(_addr_vp.data);
        ref var i = ref heap(out ptr<var> _addr_i);
        (ifaceWords.val);

        (@unsafe.Pointer(_addr_i)).typ = typ(ifaceWords.val);

        (@unsafe.Pointer(_addr_i)).data = data;
        if (i != old) {
            return false;
        }
        return CompareAndSwapPointer(_addr_vp.data, data, np.data);
    }
});

// Disable/enable preemption, implemented in runtime.
private static void runtime_procPin();
private static void runtime_procUnpin();

} // end atomic_package
