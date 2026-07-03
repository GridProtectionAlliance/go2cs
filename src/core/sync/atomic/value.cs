// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.sync;

using @unsafe = unsafe_package;

partial class atomic_package {

// A Value provides an atomic load and store of a consistently typed value.
// The zero value for a Value returns nil from [Value.Load].
// Once [Value.Store] has been called, a Value must not be copied.
//
// A Value must not be copied after first use.
[GoType] partial struct Value {
    internal any v;
}

// efaceWords is interface{} internal representation.
[GoType] partial struct efaceWords {
    internal @unsafe.Pointer typ;
    internal @unsafe.Pointer data;
}

// Load returns the value set by the most recent Store.
// It returns nil if there has been no call to Store for this Value.
[GoRecv] public static any /*val*/ Load(this ref Value v) {
    any val = default!;

    var vp = (ж<efaceWords>)(uintptr)(@unsafe.Pointer.FromRef(ref v));
    @unsafe.Pointer typ = (uintptr)LoadPointer(Ꮡ((~vp).typ));
    if (typ == nil || typ.Value == new @unsafe.Pointer(ᏑfirstStoreInProgress)) {
        // First store not yet completed.
        return default!;
    }
    @unsafe.Pointer data = (uintptr)LoadPointer(Ꮡ((~vp).data));
    var vlp = (ж<efaceWords>)(uintptr)(new @unsafe.Pointer(Ꮡ(val)));
    vlp.Value.typ = typ;
    vlp.Value.data = data;
    return val;
}

internal static ж<byte> ᏑfirstStoreInProgress = new(default(byte));
internal static ref byte firstStoreInProgress => ref ᏑfirstStoreInProgress.Value;

// Store sets the value of the [Value] v to val.
// All calls to Store for a given Value must use values of the same concrete type.
// Store of an inconsistent type panics, as does Store(nil).
[GoRecv] public static void Store(this ref Value v, any val) {
    if (val == default!) {
        throw panic("sync/atomic: store of nil value into Value");
    }
    var vp = (ж<efaceWords>)(uintptr)(@unsafe.Pointer.FromRef(ref v));
    var vlp = (ж<efaceWords>)(uintptr)(new @unsafe.Pointer(Ꮡ(val)));
    while (ᐧ) {
        @unsafe.Pointer typ = (uintptr)LoadPointer(Ꮡ((~vp).typ));
        if (typ == nil) {
            // Attempt to start first store.
            // Disable preemption so that other goroutines can use
            // active spin wait to wait for completion.
            runtime_procPin();
            if (!CompareAndSwapPointer(Ꮡ((~vp).typ), nil, new @unsafe.Pointer(ᏑfirstStoreInProgress))) {
                runtime_procUnpin();
                continue;
            }
            // Complete first store.
            StorePointer(Ꮡ((~vp).data), (~vlp).data);
            StorePointer(Ꮡ((~vp).typ), (~vlp).typ);
            runtime_procUnpin();
            return;
        }
        if (typ.Value == new @unsafe.Pointer(ᏑfirstStoreInProgress)) {
            // First store in progress. Wait.
            // Since we disable preemption around the first store,
            // we can wait with active spinning.
            continue;
        }
        // First store completed. Check type and overwrite data.
        if (typ.Value != (~vlp).typ) {
            throw panic("sync/atomic: store of inconsistently typed value into Value");
        }
        StorePointer(Ꮡ((~vp).data), (~vlp).data);
        return;
    }
}

// Swap stores new into Value and returns the previous value. It returns nil if
// the Value is empty.
//
// All calls to Swap for a given Value must use values of the same concrete
// type. Swap of an inconsistent type panics, as does Swap(nil).
[GoRecv] public static any /*old*/ Swap(this ref Value v, any @new) {
    any old = default!;

    if (@new == default!) {
        throw panic("sync/atomic: swap of nil value into Value");
    }
    var vp = (ж<efaceWords>)(uintptr)(@unsafe.Pointer.FromRef(ref v));
    var np = (ж<efaceWords>)(uintptr)(new @unsafe.Pointer(Ꮡ(@new)));
    while (ᐧ) {
        @unsafe.Pointer typ = (uintptr)LoadPointer(Ꮡ((~vp).typ));
        if (typ == nil) {
            // Attempt to start first store.
            // Disable preemption so that other goroutines can use
            // active spin wait to wait for completion; and so that
            // GC does not see the fake type accidentally.
            runtime_procPin();
            if (!CompareAndSwapPointer(Ꮡ((~vp).typ), nil, new @unsafe.Pointer(ᏑfirstStoreInProgress))) {
                runtime_procUnpin();
                continue;
            }
            // Complete first store.
            StorePointer(Ꮡ((~vp).data), (~np).data);
            StorePointer(Ꮡ((~vp).typ), (~np).typ);
            runtime_procUnpin();
            return default!;
        }
        if (typ.Value == new @unsafe.Pointer(ᏑfirstStoreInProgress)) {
            // First store in progress. Wait.
            // Since we disable preemption around the first store,
            // we can wait with active spinning.
            continue;
        }
        // First store completed. Check type and overwrite data.
        if (typ.Value != (~np).typ) {
            throw panic("sync/atomic: swap of inconsistently typed value into Value");
        }
        var op = (ж<efaceWords>)(uintptr)(new @unsafe.Pointer(Ꮡ(old)));
        (op.Value.typ, op.Value.data) = (np.Value.typ, (uintptr)SwapPointer(Ꮡ((~vp).data), (~np).data));
        return old;
    }
}

// CompareAndSwap executes the compare-and-swap operation for the [Value].
//
// All calls to CompareAndSwap for a given Value must use values of the same
// concrete type. CompareAndSwap of an inconsistent type panics, as does
// CompareAndSwap(old, nil).
[GoRecv] public static bool /*swapped*/ CompareAndSwap(this ref Value v, any old, any @new) {
    bool swapped = default!;

    if (@new == default!) {
        throw panic("sync/atomic: compare and swap of nil value into Value");
    }
    var vp = (ж<efaceWords>)(uintptr)(@unsafe.Pointer.FromRef(ref v));
    var np = (ж<efaceWords>)(uintptr)(new @unsafe.Pointer(Ꮡ(@new)));
    var op = (ж<efaceWords>)(uintptr)(new @unsafe.Pointer(Ꮡ(old)));
    if ((~op).typ != nil && (~np).typ != (~op).typ) {
        throw panic("sync/atomic: compare and swap of inconsistently typed values");
    }
    while (ᐧ) {
        @unsafe.Pointer typ = (uintptr)LoadPointer(Ꮡ((~vp).typ));
        if (typ == nil) {
            if (old != default!) {
                return false;
            }
            // Attempt to start first store.
            // Disable preemption so that other goroutines can use
            // active spin wait to wait for completion; and so that
            // GC does not see the fake type accidentally.
            runtime_procPin();
            if (!CompareAndSwapPointer(Ꮡ((~vp).typ), nil, new @unsafe.Pointer(ᏑfirstStoreInProgress))) {
                runtime_procUnpin();
                continue;
            }
            // Complete first store.
            StorePointer(Ꮡ((~vp).data), (~np).data);
            StorePointer(Ꮡ((~vp).typ), (~np).typ);
            runtime_procUnpin();
            return true;
        }
        if (typ.Value == new @unsafe.Pointer(ᏑfirstStoreInProgress)) {
            // First store in progress. Wait.
            // Since we disable preemption around the first store,
            // we can wait with active spinning.
            continue;
        }
        // First store completed. Check type and overwrite data.
        if (typ.Value != (~np).typ) {
            throw panic("sync/atomic: compare and swap of inconsistently typed value into Value");
        }
        // Compare old and current via runtime equality check.
        // This allows value types to be compared, something
        // not offered by the package functions.
        // CompareAndSwapPointer below only ensures vp.data
        // has not changed since LoadPointer.
        @unsafe.Pointer data = (uintptr)LoadPointer(Ꮡ((~vp).data));
        any i = default!;
        ((ж<efaceWords>)(uintptr)(new @unsafe.Pointer(Ꮡ(i)))).Value.typ = typ;
        ((ж<efaceWords>)(uintptr)(new @unsafe.Pointer(Ꮡ(i)))).Value.data = data;
        if (!AreEqual(i, old)) {
            return false;
        }
        return CompareAndSwapPointer(Ꮡ((~vp).data), data, (~np).data);
    }
}

// Disable/enable preemption, implemented in runtime.
internal static partial nint runtime_procPin();

internal static partial void runtime_procUnpin();

} // end atomic_package
