// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build arm

// package atomic -- go2cs converted at 2020 August 29 08:16:29 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Go\src\runtime\internal\atomic\atomic_arm.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static unsafe partial class atomic_package
    {
        private partial struct spinlock
        {
            public uint v;
        }

        //go:nosplit
        private static void @lock(this ref spinlock l)
        {
            while (true)
            {
                if (Cas(ref l.v, 0L, 1L))
                {
                    return;
                }
            }

        }

        //go:nosplit
        private static void unlock(this ref spinlock l)
        {
            Store(ref l.v, 0L);
        }

        private static var locktab = default;

        private static ref spinlock addrLock(ref ulong addr)
        {
            return ref locktab[(uintptr(@unsafe.Pointer(addr)) >> (int)(3L)) % uintptr(len(locktab))].l;
        }

        // Atomic add and return new value.
        //go:nosplit
        public static uint Xadd(ref uint val, int delta)
        {
            while (true)
            {
                var oval = val.Value;
                var nval = oval + uint32(delta);
                if (Cas(val, oval, nval))
                {
                    return nval;
                }
            }

        }

        //go:noescape
        public static System.UIntPtr Xadduintptr(ref System.UIntPtr ptr, System.UIntPtr delta)
;

        //go:nosplit
        public static uint Xchg(ref uint addr, uint v)
        {
            while (true)
            {>>MARKER:FUNCTION_Xadduintptr_BLOCK_PREFIX<<
                var old = addr.Value;
                if (Cas(addr, old, v))
                {
                    return old;
                }
            }

        }

        //go:nosplit
        public static System.UIntPtr Xchguintptr(ref System.UIntPtr addr, System.UIntPtr v)
        {
            return uintptr(Xchg((uint32.Value)(@unsafe.Pointer(addr)), uint32(v)));
        }

        //go:nosplit
        public static uint Load(ref uint addr)
        {
            return Xadd(addr, 0L);
        }

        // Should be a built-in for unsafe.Pointer?
        //go:nosplit
        private static unsafe.Pointer add(unsafe.Pointer p, System.UIntPtr x)
        {
            return @unsafe.Pointer(uintptr(p) + x);
        }

        //go:nosplit
        public static unsafe.Pointer Loadp(unsafe.Pointer addr)
        {
            return @unsafe.Pointer(uintptr(Xadd((uint32.Value)(addr), 0L)));
        }

        //go:nosplit
        public static void StorepNoWB(unsafe.Pointer addr, unsafe.Pointer v)
        {
            while (true)
            {
                *(*unsafe.Pointer) old = addr.Value;
                if (Casp1((@unsafe.Pointer.Value)(addr), old, v))
                {
                    return;
                }
            }

        }

        //go:nosplit
        public static void Store(ref uint addr, uint v)
        {
            while (true)
            {
                var old = addr.Value;
                if (Cas(addr, old, v))
                {
                    return;
                }
            }

        }

        //go:nosplit
        public static bool Cas64(ref ulong addr, ulong old, ulong @new)
        {
            if (uintptr(@unsafe.Pointer(addr)) & 7L != 0L)
            {
                (int.Value)(null).Value;

                0L; // crash on unaligned uint64
            }
            bool ok = default;
            addrLock(addr).@lock();
            if (addr == old.Value)
            {
                addr.Value = new;
                ok = true;
            }
            addrLock(addr).unlock();
            return ok;
        }

        //go:nosplit
        public static ulong Xadd64(ref ulong addr, long delta)
        {
            if (uintptr(@unsafe.Pointer(addr)) & 7L != 0L)
            {
                (int.Value)(null).Value;

                0L; // crash on unaligned uint64
            }
            ulong r = default;
            addrLock(addr).@lock();
            r = addr + uint64(delta).Value;
            addr.Value = r;
            addrLock(addr).unlock();
            return r;
        }

        //go:nosplit
        public static ulong Xchg64(ref ulong addr, ulong v)
        {
            if (uintptr(@unsafe.Pointer(addr)) & 7L != 0L)
            {
                (int.Value)(null).Value;

                0L; // crash on unaligned uint64
            }
            ulong r = default;
            addrLock(addr).@lock();
            r = addr.Value;
            addr.Value = v;
            addrLock(addr).unlock();
            return r;
        }

        //go:nosplit
        public static ulong Load64(ref ulong addr)
        {
            if (uintptr(@unsafe.Pointer(addr)) & 7L != 0L)
            {
                (int.Value)(null).Value;

                0L; // crash on unaligned uint64
            }
            ulong r = default;
            addrLock(addr).@lock();
            r = addr.Value;
            addrLock(addr).unlock();
            return r;
        }

        //go:nosplit
        public static void Store64(ref ulong addr, ulong v)
        {
            if (uintptr(@unsafe.Pointer(addr)) & 7L != 0L)
            {
                (int.Value)(null).Value;

                0L; // crash on unaligned uint64
            }
            addrLock(addr).@lock();
            addr.Value = v;
            addrLock(addr).unlock();
        }

        //go:nosplit
        public static void Or8(ref byte addr, byte v)
        { 
            // Align down to 4 bytes and use 32-bit CAS.
            var uaddr = uintptr(@unsafe.Pointer(addr));
            var addr32 = (uint32.Value)(@unsafe.Pointer(uaddr & ~3L));
            var word = uint32(v) << (int)(((uaddr & 3L) * 8L)); // little endian
            while (true)
            {
                var old = addr32.Value;
                if (Cas(addr32, old, old | word))
                {
                    return;
                }
            }

        }

        //go:nosplit
        public static void And8(ref byte addr, byte v)
        { 
            // Align down to 4 bytes and use 32-bit CAS.
            var uaddr = uintptr(@unsafe.Pointer(addr));
            var addr32 = (uint32.Value)(@unsafe.Pointer(uaddr & ~3L));
            var word = uint32(v) << (int)(((uaddr & 3L) * 8L)); // little endian
            var mask = uint32(0xFFUL) << (int)(((uaddr & 3L) * 8L)); // little endian
            word |= ~mask;
            while (true)
            {
                var old = addr32.Value;
                if (Cas(addr32, old, old & word))
                {
                    return;
                }
            }

        }

        //go:nosplit
        private static bool armcas(ref uint ptr, uint old, uint @new)
;
    }
}}}
