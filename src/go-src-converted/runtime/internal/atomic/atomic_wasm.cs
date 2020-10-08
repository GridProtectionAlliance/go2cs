// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TODO(neelance): implement with actual atomic operations as soon as threads are available
// See https://github.com/WebAssembly/design/issues/1073

// Export some functions via linkname to assembly in sync/atomic.
//go:linkname Load
//go:linkname Loadp
//go:linkname Load64
//go:linkname Loaduintptr
//go:linkname Xadd
//go:linkname Xadd64
//go:linkname Xadduintptr
//go:linkname Xchg
//go:linkname Xchg64
//go:linkname Xchguintptr
//go:linkname Cas
//go:linkname Cas64
//go:linkname Casuintptr
//go:linkname Store
//go:linkname Store64
//go:linkname Storeuintptr

// package atomic -- go2cs converted at 2020 October 08 03:19:11 UTC
// import "runtime/internal/atomic" ==> using atomic = go.runtime.@internal.atomic_package
// Original source: C:\Go\src\runtime\internal\atomic\atomic_wasm.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go {
namespace runtime {
namespace @internal
{
    public static partial class atomic_package
    {
        //go:nosplit
        //go:noinline
        public static uint Load(ptr<uint> _addr_ptr)
        {
            ref uint ptr = ref _addr_ptr.val;

            return ptr;
        }

        //go:nosplit
        //go:noinline
        public static unsafe.Pointer Loadp(unsafe.Pointer ptr)
        {
            return new ptr<ptr<ptr<unsafe.Pointer>>>(ptr);
        }

        //go:nosplit
        //go:noinline
        public static uint LoadAcq(ptr<uint> _addr_ptr)
        {
            ref uint ptr = ref _addr_ptr.val;

            return ptr;
        }

        //go:nosplit
        //go:noinline
        public static byte Load8(ptr<byte> _addr_ptr)
        {
            ref byte ptr = ref _addr_ptr.val;

            return ptr;
        }

        //go:nosplit
        //go:noinline
        public static ulong Load64(ptr<ulong> _addr_ptr)
        {
            ref ulong ptr = ref _addr_ptr.val;

            return ptr;
        }

        //go:nosplit
        //go:noinline
        public static uint Xadd(ptr<uint> _addr_ptr, int delta)
        {
            ref uint ptr = ref _addr_ptr.val;

            var @new = ptr + uint32(delta).val;
            ptr = new;
            return new;
        }

        //go:nosplit
        //go:noinline
        public static ulong Xadd64(ptr<ulong> _addr_ptr, long delta)
        {
            ref ulong ptr = ref _addr_ptr.val;

            var @new = ptr + uint64(delta).val;
            ptr = new;
            return new;
        }

        //go:nosplit
        //go:noinline
        public static System.UIntPtr Xadduintptr(ptr<System.UIntPtr> _addr_ptr, System.UIntPtr delta)
        {
            ref System.UIntPtr ptr = ref _addr_ptr.val;

            var @new = ptr + delta.val;
            ptr = new;
            return new;
        }

        //go:nosplit
        //go:noinline
        public static uint Xchg(ptr<uint> _addr_ptr, uint @new)
        {
            ref uint ptr = ref _addr_ptr.val;

            uint old = ptr;
            ptr = new;
            return old;
        }

        //go:nosplit
        //go:noinline
        public static ulong Xchg64(ptr<ulong> _addr_ptr, ulong @new)
        {
            ref ulong ptr = ref _addr_ptr.val;

            ulong old = ptr;
            ptr = new;
            return old;
        }

        //go:nosplit
        //go:noinline
        public static System.UIntPtr Xchguintptr(ptr<System.UIntPtr> _addr_ptr, System.UIntPtr @new)
        {
            ref System.UIntPtr ptr = ref _addr_ptr.val;

            System.UIntPtr old = ptr;
            ptr = new;
            return old;
        }

        //go:nosplit
        //go:noinline
        public static void And8(ptr<byte> _addr_ptr, byte val)
        {
            ref byte ptr = ref _addr_ptr.val;

            ptr = ptr & val.val;
        }

        //go:nosplit
        //go:noinline
        public static void Or8(ptr<byte> _addr_ptr, byte val)
        {
            ref byte ptr = ref _addr_ptr.val;

            ptr = ptr | val.val;
        }

        // NOTE: Do not add atomicxor8 (XOR is not idempotent).

        //go:nosplit
        //go:noinline
        public static bool Cas64(ptr<ulong> _addr_ptr, ulong old, ulong @new)
        {
            ref ulong ptr = ref _addr_ptr.val;

            if (ptr == old.val)
            {
                ptr = new;
                return true;
            }

            return false;

        }

        //go:nosplit
        //go:noinline
        public static void Store(ptr<uint> _addr_ptr, uint val)
        {
            ref uint ptr = ref _addr_ptr.val;

            ptr = val;
        }

        //go:nosplit
        //go:noinline
        public static void StoreRel(ptr<uint> _addr_ptr, uint val)
        {
            ref uint ptr = ref _addr_ptr.val;

            ptr = val;
        }

        //go:nosplit
        //go:noinline
        public static void Store8(ptr<byte> _addr_ptr, byte val)
        {
            ref byte ptr = ref _addr_ptr.val;

            ptr = val;
        }

        //go:nosplit
        //go:noinline
        public static void Store64(ptr<ulong> _addr_ptr, ulong val)
        {
            ref ulong ptr = ref _addr_ptr.val;

            ptr = val;
        }

        //go:notinheap
        private partial struct noWB
        {
        }

        //go:noinline
        //go:nosplit
        public static void StorepNoWB(unsafe.Pointer ptr, unsafe.Pointer val)
        {
            (noWB.val)(ptr).val;

            (noWB.val)(val);

        }

        //go:nosplit
        //go:noinline
        public static bool Cas(ptr<uint> _addr_ptr, uint old, uint @new)
        {
            ref uint ptr = ref _addr_ptr.val;

            if (ptr == old.val)
            {
                ptr = new;
                return true;
            }

            return false;

        }

        //go:nosplit
        //go:noinline
        public static bool Casp1(ptr<unsafe.Pointer> _addr_ptr, unsafe.Pointer old, unsafe.Pointer @new)
        {
            ref unsafe.Pointer ptr = ref _addr_ptr.val;

            if (ptr == old.val)
            {
                ptr = new;
                return true;
            }

            return false;

        }

        //go:nosplit
        //go:noinline
        public static bool Casuintptr(ptr<System.UIntPtr> _addr_ptr, System.UIntPtr old, System.UIntPtr @new)
        {
            ref System.UIntPtr ptr = ref _addr_ptr.val;

            if (ptr == old.val)
            {
                ptr = new;
                return true;
            }

            return false;

        }

        //go:nosplit
        //go:noinline
        public static bool CasRel(ptr<uint> _addr_ptr, uint old, uint @new)
        {
            ref uint ptr = ref _addr_ptr.val;

            if (ptr == old.val)
            {
                ptr = new;
                return true;
            }

            return false;

        }

        //go:nosplit
        //go:noinline
        public static void Storeuintptr(ptr<System.UIntPtr> _addr_ptr, System.UIntPtr @new)
        {
            ref System.UIntPtr ptr = ref _addr_ptr.val;

            ptr = new;
        }

        //go:nosplit
        //go:noinline
        public static System.UIntPtr Loaduintptr(ptr<System.UIntPtr> _addr_ptr)
        {
            ref System.UIntPtr ptr = ref _addr_ptr.val;

            return ptr;
        }

        //go:nosplit
        //go:noinline
        public static ulong Loaduint(ptr<ulong> _addr_ptr)
        {
            ref ulong ptr = ref _addr_ptr.val;

            return ptr;
        }

        //go:nosplit
        //go:noinline
        public static long Loadint64(ptr<long> _addr_ptr)
        {
            ref long ptr = ref _addr_ptr.val;

            return ptr;
        }

        //go:nosplit
        //go:noinline
        public static long Xaddint64(ptr<long> _addr_ptr, long delta)
        {
            ref long ptr = ref _addr_ptr.val;

            var @new = ptr + delta.val;
            ptr = new;
            return new;
        }
    }
}}}
