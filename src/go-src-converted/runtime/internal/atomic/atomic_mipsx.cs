// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build mips mipsle

// package atomic -- go2cs converted at 2020 August 29 08:16:30 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Go\src\runtime\internal\atomic\atomic_mipsx.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class atomic_package
    {
        // TODO implement lock striping
        private static var @lock = default;

        //go:noescape
        private static void spinLock(ref uint state)
;

        //go:noescape
        private static void spinUnlock(ref uint state)
;

        //go:nosplit
        private static void lockAndCheck(ref ulong addr)
        { 
            // ensure 8-byte alignement
            if (uintptr(@unsafe.Pointer(addr)) & 7L != 0L)
            {>>MARKER:FUNCTION_spinUnlock_BLOCK_PREFIX<<
                addr = null;
            } 
            // force dereference before taking lock
            _ = addr.Value;

            spinLock(ref @lock.state);
        }

        //go:nosplit
        private static void unlock()
        {
            spinUnlock(ref @lock.state);
        }

        //go:nosplit
        private static void unlockNoFence()
        {
            @lock.state = 0L;
        }

        //go:nosplit
        public static ulong Xadd64(ref ulong addr, long delta)
        {
            lockAndCheck(addr);

            new = addr + uint64(delta).Value;
            addr.Value = new;

            unlock();
            return;
        }

        //go:nosplit
        public static ulong Xchg64(ref ulong addr, ulong @new)
        {
            lockAndCheck(addr);

            old = addr.Value;
            addr.Value = new;

            unlock();
            return;
        }

        //go:nosplit
        public static bool Cas64(ref ulong addr, ulong old, ulong @new)
        {
            lockAndCheck(addr);

            if ((addr.Value) == old)
            {>>MARKER:FUNCTION_spinLock_BLOCK_PREFIX<<
                addr.Value = new;
                unlock();
                return true;
            }
            unlockNoFence();
            return false;
        }

        //go:nosplit
        public static ulong Load64(ref ulong addr)
        {
            lockAndCheck(addr);

            val = addr.Value;

            unlock();
            return;
        }

        //go:nosplit
        public static void Store64(ref ulong addr, ulong val)
        {
            lockAndCheck(addr);

            addr.Value = val;

            unlock();
            return;
        }

        //go:noescape
        public static uint Xadd(ref uint ptr, int delta)
;

        //go:noescape
        public static System.UIntPtr Xadduintptr(ref System.UIntPtr ptr, System.UIntPtr delta)
;

        //go:noescape
        public static uint Xchg(ref uint ptr, uint @new)
;

        //go:noescape
        public static System.UIntPtr Xchguintptr(ref System.UIntPtr ptr, System.UIntPtr @new)
;

        //go:noescape
        public static uint Load(ref uint ptr)
;

        //go:noescape
        public static unsafe.Pointer Loadp(unsafe.Pointer ptr)
;

        //go:noescape
        public static void And8(ref byte ptr, byte val)
;

        //go:noescape
        public static void Or8(ref byte ptr, byte val)
;

        //go:noescape
        public static void Store(ref uint ptr, uint val)
;

        // NO go:noescape annotation; see atomic_pointer.go.
        public static void StorepNoWB(unsafe.Pointer ptr, unsafe.Pointer val)
;
    }
}}}
