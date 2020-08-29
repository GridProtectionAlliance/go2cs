// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package atomic -- go2cs converted at 2020 August 29 08:16:21 UTC
// import "sync/atomic" ==> using atomic = go.sync.atomic_package
// Original source: C:\Go\src\sync\atomic\value.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace sync
{
    public static partial class atomic_package
    {
        // A Value provides an atomic load and store of a consistently typed value.
        // The zero value for a Value returns nil from Load.
        // Once Store has been called, a Value must not be copied.
        //
        // A Value must not be copied after first use.
        public partial struct Value
        {
        }

        // ifaceWords is interface{} internal representation.
        private partial struct ifaceWords
        {
            public unsafe.Pointer typ;
            public unsafe.Pointer data;
        }

        // Load returns the value set by the most recent Store.
        // It returns nil if there has been no call to Store for this Value.
        private static object Load(this ref Value v)
        {
            var vp = (ifaceWords.Value)(@unsafe.Pointer(v));
            var typ = LoadPointer(ref vp.typ);
            if (typ == null || uintptr(typ) == ~uintptr(0L))
            { 
                // First store not yet completed.
                return null;
            }
            var data = LoadPointer(ref vp.data);
            var xp = (ifaceWords.Value)(@unsafe.Pointer(ref x));
            xp.typ = typ;
            xp.data = data;
            return;
        }

        // Store sets the value of the Value to x.
        // All calls to Store for a given Value must use values of the same concrete type.
        // Store of an inconsistent type panics, as does Store(nil).
        private static void Store(this ref Value _v, object x) => func(_v, (ref Value v, Defer _, Panic panic, Recover __) =>
        {
            if (x == null)
            {
                panic("sync/atomic: store of nil value into Value");
            }
            var vp = (ifaceWords.Value)(@unsafe.Pointer(v));
            var xp = (ifaceWords.Value)(@unsafe.Pointer(ref x));
            while (true)
            {
                var typ = LoadPointer(ref vp.typ);
                if (typ == null)
                { 
                    // Attempt to start first store.
                    // Disable preemption so that other goroutines can use
                    // active spin wait to wait for completion; and so that
                    // GC does not see the fake type accidentally.
                    runtime_procPin();
                    if (!CompareAndSwapPointer(ref vp.typ, null, @unsafe.Pointer(~uintptr(0L))))
                    {
                        runtime_procUnpin();
                        continue;
                    } 
                    // Complete first store.
                    StorePointer(ref vp.data, xp.data);
                    StorePointer(ref vp.typ, xp.typ);
                    runtime_procUnpin();
                    return;
                }
                if (uintptr(typ) == ~uintptr(0L))
                { 
                    // First store in progress. Wait.
                    // Since we disable preemption around the first store,
                    // we can wait with active spinning.
                    continue;
                } 
                // First store completed. Check type and overwrite data.
                if (typ != xp.typ)
                {
                    panic("sync/atomic: store of inconsistently typed value into Value");
                }
                StorePointer(ref vp.data, xp.data);
                return;
            }

        });

        // Disable/enable preemption, implemented in runtime.
        private static void runtime_procPin()
;
        private static void runtime_procUnpin()
;
    }
}}
