// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build arm

// package atomic -- go2cs converted at 2020 October 08 03:19:09 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Go\src\runtime\internal\atomic\atomic_arm.go
using cpu = go.@internal.cpu_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class atomic_package
    {
        // Export some functions via linkname to assembly in sync/atomic.
        //go:linkname Xchg
        //go:linkname Xchguintptr
        private partial struct spinlock
        {
            public uint v;
        }

        //go:nosplit
        private static void @lock(this ptr<spinlock> _addr_l)
        {
            ref spinlock l = ref _addr_l.val;

            while (true)
            {
                if (Cas(_addr_l.v, 0L, 1L))
                {
                    return ;
                }

            }


        }

        //go:nosplit
        private static void unlock(this ptr<spinlock> _addr_l)
        {
            ref spinlock l = ref _addr_l.val;

            Store(_addr_l.v, 0L);
        }

        private static var locktab = default;

        private static ptr<spinlock> addrLock(ptr<ulong> _addr_addr)
        {
            ref ulong addr = ref _addr_addr.val;

            return _addr__addr_locktab[(uintptr(@unsafe.Pointer(addr)) >> (int)(3L)) % uintptr(len(locktab))].l!;
        }

        // Atomic add and return new value.
        //go:nosplit
        public static uint Xadd(ptr<uint> _addr_val, int delta)
        {
            ref uint val = ref _addr_val.val;

            while (true)
            {
                uint oval = val;
                var nval = oval + uint32(delta);
                if (Cas(val, oval, nval))
                {
                    return nval;
                }

            }


        }

        //go:noescape
        public static System.UIntPtr Xadduintptr(ptr<System.UIntPtr> ptr, System.UIntPtr delta)
;

        //go:nosplit
        public static uint Xchg(ptr<uint> _addr_addr, uint v)
        {
            ref uint addr = ref _addr_addr.val;

            while (true)
            {>>MARKER:FUNCTION_Xadduintptr_BLOCK_PREFIX<<
                uint old = addr;
                if (Cas(addr, old, v))
                {
                    return old;
                }

            }


        }

        //go:nosplit
        public static System.UIntPtr Xchguintptr(ptr<System.UIntPtr> _addr_addr, System.UIntPtr v)
        {
            ref System.UIntPtr addr = ref _addr_addr.val;

            return uintptr(Xchg(_addr_(uint32.val)(@unsafe.Pointer(addr)), uint32(v)));
        }

        // Not noescape -- it installs a pointer to addr.
        public static void StorepNoWB(unsafe.Pointer addr, unsafe.Pointer v)
;

        //go:noescape
        public static void Store(ptr<uint> addr, uint v)
;

        //go:noescape
        public static void StoreRel(ptr<uint> addr, uint v)
;

        //go:nosplit
        private static bool goCas64(ptr<ulong> _addr_addr, ulong old, ulong @new)
        {
            ref ulong addr = ref _addr_addr.val;

            if (uintptr(@unsafe.Pointer(addr)) & 7L != 0L)
            {>>MARKER:FUNCTION_StoreRel_BLOCK_PREFIX<<
                (int.val)(null).val;

                0L; // crash on unaligned uint64
            }

            _ = addr; // if nil, fault before taking the lock
            bool ok = default;
            addrLock(_addr_addr).@lock();
            if (addr == old.val)
            {>>MARKER:FUNCTION_Store_BLOCK_PREFIX<<
                addr = new;
                ok = true;
            }

            addrLock(_addr_addr).unlock();
            return ok;

        }

        //go:nosplit
        private static ulong goXadd64(ptr<ulong> _addr_addr, long delta)
        {
            ref ulong addr = ref _addr_addr.val;

            if (uintptr(@unsafe.Pointer(addr)) & 7L != 0L)
            {>>MARKER:FUNCTION_StorepNoWB_BLOCK_PREFIX<<
                (int.val)(null).val;

                0L; // crash on unaligned uint64
            }

            _ = addr; // if nil, fault before taking the lock
            ulong r = default;
            addrLock(_addr_addr).@lock();
            r = addr + uint64(delta).val;
            addr = r;
            addrLock(_addr_addr).unlock();
            return r;

        }

        //go:nosplit
        private static ulong goXchg64(ptr<ulong> _addr_addr, ulong v)
        {
            ref ulong addr = ref _addr_addr.val;

            if (uintptr(@unsafe.Pointer(addr)) & 7L != 0L)
            {
                (int.val)(null).val;

                0L; // crash on unaligned uint64
            }

            _ = addr; // if nil, fault before taking the lock
            ulong r = default;
            addrLock(_addr_addr).@lock();
            r = addr;
            addr = v;
            addrLock(_addr_addr).unlock();
            return r;

        }

        //go:nosplit
        private static ulong goLoad64(ptr<ulong> _addr_addr)
        {
            ref ulong addr = ref _addr_addr.val;

            if (uintptr(@unsafe.Pointer(addr)) & 7L != 0L)
            {
                (int.val)(null).val;

                0L; // crash on unaligned uint64
            }

            _ = addr; // if nil, fault before taking the lock
            ulong r = default;
            addrLock(_addr_addr).@lock();
            r = addr;
            addrLock(_addr_addr).unlock();
            return r;

        }

        //go:nosplit
        private static void goStore64(ptr<ulong> _addr_addr, ulong v)
        {
            ref ulong addr = ref _addr_addr.val;

            if (uintptr(@unsafe.Pointer(addr)) & 7L != 0L)
            {
                (int.val)(null).val;

                0L; // crash on unaligned uint64
            }

            _ = addr; // if nil, fault before taking the lock
            addrLock(_addr_addr).@lock();
            addr = v;
            addrLock(_addr_addr).unlock();

        }

        //go:nosplit
        public static void Or8(ptr<byte> _addr_addr, byte v)
        {
            ref byte addr = ref _addr_addr.val;
 
            // Align down to 4 bytes and use 32-bit CAS.
            var uaddr = uintptr(@unsafe.Pointer(addr));
            var addr32 = (uint32.val)(@unsafe.Pointer(uaddr & ~3L));
            var word = uint32(v) << (int)(((uaddr & 3L) * 8L)); // little endian
            while (true)
            {
                var old = addr32.val;
                if (Cas(addr32, old, old | word))
                {
                    return ;
                }

            }


        }

        //go:nosplit
        public static void And8(ptr<byte> _addr_addr, byte v)
        {
            ref byte addr = ref _addr_addr.val;
 
            // Align down to 4 bytes and use 32-bit CAS.
            var uaddr = uintptr(@unsafe.Pointer(addr));
            var addr32 = (uint32.val)(@unsafe.Pointer(uaddr & ~3L));
            var word = uint32(v) << (int)(((uaddr & 3L) * 8L)); // little endian
            var mask = uint32(0xFFUL) << (int)(((uaddr & 3L) * 8L)); // little endian
            word |= ~mask;
            while (true)
            {
                var old = addr32.val;
                if (Cas(addr32, old, old & word))
                {
                    return ;
                }

            }


        }

        //go:nosplit
        private static bool armcas(ptr<uint> ptr, uint old, uint @new)
;

        //go:noescape
        public static uint Load(ptr<uint> addr)
;

        // NO go:noescape annotation; *addr escapes if result escapes (#31525)
        public static unsafe.Pointer Loadp(unsafe.Pointer addr)
;

        //go:noescape
        public static byte Load8(ptr<byte> addr)
;

        //go:noescape
        public static uint LoadAcq(ptr<uint> addr)
;

        //go:noescape
        public static bool Cas64(ptr<ulong> addr, ulong old, ulong @new)
;

        //go:noescape
        public static bool CasRel(ptr<uint> addr, uint old, uint @new)
;

        //go:noescape
        public static ulong Xadd64(ptr<ulong> addr, long delta)
;

        //go:noescape
        public static ulong Xchg64(ptr<ulong> addr, ulong v)
;

        //go:noescape
        public static ulong Load64(ptr<ulong> addr)
;

        //go:noescape
        public static void Store8(ptr<byte> addr, byte v)
;

        //go:noescape
        public static void Store64(ptr<ulong> addr, ulong v)
;
    }
}}}
