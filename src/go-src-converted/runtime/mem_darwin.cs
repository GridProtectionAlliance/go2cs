// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:17:45 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mem_darwin.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // Don't split the stack as this function may be invoked without a valid G,
        // which prevents us from allocating more stack.
        //go:nosplit
        private static unsafe.Pointer sysAlloc(System.UIntPtr n, ref ulong sysStat)
        {
            var (v, err) = mmap(null, n, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_PRIVATE, -1L, 0L);
            if (err != 0L)
            {
                return null;
            }
            mSysStatInc(sysStat, n);
            return v;
        }

        private static void sysUnused(unsafe.Pointer v, System.UIntPtr n)
        { 
            // Linux's MADV_DONTNEED is like BSD's MADV_FREE.
            madvise(v, n, _MADV_FREE);
        }

        private static void sysUsed(unsafe.Pointer v, System.UIntPtr n)
        {
        }

        // Don't split the stack as this function may be invoked without a valid G,
        // which prevents us from allocating more stack.
        //go:nosplit
        private static void sysFree(unsafe.Pointer v, System.UIntPtr n, ref ulong sysStat)
        {
            mSysStatDec(sysStat, n);
            munmap(v, n);
        }

        private static void sysFault(unsafe.Pointer v, System.UIntPtr n)
        {
            mmap(v, n, _PROT_NONE, _MAP_ANON | _MAP_PRIVATE | _MAP_FIXED, -1L, 0L);
        }

        private static unsafe.Pointer sysReserve(unsafe.Pointer v, System.UIntPtr n, ref bool reserved)
        {
            reserved.Value = true;
            var (p, err) = mmap(v, n, _PROT_NONE, _MAP_ANON | _MAP_PRIVATE, -1L, 0L);
            if (err != 0L)
            {
                return null;
            }
            return p;
        }

        private static readonly long _ENOMEM = 12L;

        private static void sysMap(unsafe.Pointer v, System.UIntPtr n, bool reserved, ref ulong sysStat)
        {
            mSysStatInc(sysStat, n);
            var (p, err) = mmap(v, n, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_FIXED | _MAP_PRIVATE, -1L, 0L);
            if (err == _ENOMEM)
            {
                throw("runtime: out of memory");
            }
            if (p != v || err != 0L)
            {
                throw("runtime: cannot map pages in arena address space");
            }
        }
    }
}
