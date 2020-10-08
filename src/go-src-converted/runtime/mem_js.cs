// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build js,wasm

// package runtime -- go2cs converted at 2020 October 08 03:20:36 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mem_js.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Don't split the stack as this function may be invoked without a valid G,
        // which prevents us from allocating more stack.
        //go:nosplit
        private static unsafe.Pointer sysAlloc(System.UIntPtr n, ptr<ulong> _addr_sysStat)
        {
            ref ulong sysStat = ref _addr_sysStat.val;

            var p = sysReserve(null, n);
            sysMap(p, n, _addr_sysStat);
            return p;
        }

        private static void sysUnused(unsafe.Pointer v, System.UIntPtr n)
        {
        }

        private static void sysUsed(unsafe.Pointer v, System.UIntPtr n)
        {
        }

        private static void sysHugePage(unsafe.Pointer v, System.UIntPtr n)
        {
        }

        // Don't split the stack as this function may be invoked without a valid G,
        // which prevents us from allocating more stack.
        //go:nosplit
        private static void sysFree(unsafe.Pointer v, System.UIntPtr n, ptr<ulong> _addr_sysStat)
        {
            ref ulong sysStat = ref _addr_sysStat.val;

            mSysStatDec(sysStat, n);
        }

        private static void sysFault(unsafe.Pointer v, System.UIntPtr n)
        {
        }

        private static System.UIntPtr reserveEnd = default;

        private static unsafe.Pointer sysReserve(unsafe.Pointer v, System.UIntPtr n)
        { 
            // TODO(neelance): maybe unify with mem_plan9.go, depending on how https://github.com/WebAssembly/design/blob/master/FutureFeatures.md#finer-grained-control-over-memory turns out

            if (v != null)
            { 
                // The address space of WebAssembly's linear memory is contiguous,
                // so requesting specific addresses is not supported. We could use
                // a different address, but then mheap.sysAlloc discards the result
                // right away and we don't reuse chunks passed to sysFree.
                return null;

            } 

            // Round up the initial reserveEnd to 64 KiB so that
            // reservations are always aligned to the page size.
            var initReserveEnd = alignUp(lastmoduledatap.end, physPageSize);
            if (reserveEnd < initReserveEnd)
            {
                reserveEnd = initReserveEnd;
            }

            v = @unsafe.Pointer(reserveEnd);
            reserveEnd += alignUp(n, physPageSize);

            var current = currentMemory(); 
            // reserveEnd is always at a page boundary.
            var needed = int32(reserveEnd / physPageSize);
            if (current < needed)
            {
                if (growMemory(needed - current) == -1L)
                {
                    return null;
                }

                resetMemoryDataView();

            }

            return v;

        }

        private static int currentMemory()
;
        private static int growMemory(int pages)
;

        // resetMemoryDataView signals the JS front-end that WebAssembly's memory.grow instruction has been used.
        // This allows the front-end to replace the old DataView object with a new one.
        private static void resetMemoryDataView()
;

        private static void sysMap(unsafe.Pointer v, System.UIntPtr n, ptr<ulong> _addr_sysStat)
        {
            ref ulong sysStat = ref _addr_sysStat.val;

            mSysStatInc(sysStat, n);
        }
    }
}
