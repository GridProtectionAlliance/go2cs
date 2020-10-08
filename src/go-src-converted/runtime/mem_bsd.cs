// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd netbsd openbsd solaris

// package runtime -- go2cs converted at 2020 October 08 03:20:35 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mem_bsd.go
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
            madvise(v, n, _MADV_FREE);
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
            munmap(v, n);
        }

        private static void sysFault(unsafe.Pointer v, System.UIntPtr n)
        {
            mmap(v, n, _PROT_NONE, _MAP_ANON | _MAP_PRIVATE | _MAP_FIXED, -1L, 0L);
        }

        // Indicates not to reserve swap space for the mapping.
        private static readonly ulong _sunosMAP_NORESERVE = (ulong)0x40UL;



        private static unsafe.Pointer sysReserve(unsafe.Pointer v, System.UIntPtr n)
        {
            var flags = int32(_MAP_ANON | _MAP_PRIVATE);
            if (GOOS == "solaris" || GOOS == "illumos")
            { 
                // Be explicit that we don't want to reserve swap space
                // for PROT_NONE anonymous mappings. This avoids an issue
                // wherein large mappings can cause fork to fail.
                flags |= _sunosMAP_NORESERVE;

            }

            var (p, err) = mmap(v, n, _PROT_NONE, flags, -1L, 0L);
            if (err != 0L)
            {
                return null;
            }

            return p;

        }

        private static readonly long _sunosEAGAIN = (long)11L;

        private static readonly long _ENOMEM = (long)12L;



        private static void sysMap(unsafe.Pointer v, System.UIntPtr n, ptr<ulong> _addr_sysStat)
        {
            ref ulong sysStat = ref _addr_sysStat.val;

            mSysStatInc(sysStat, n);

            var (p, err) = mmap(v, n, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_FIXED | _MAP_PRIVATE, -1L, 0L);
            if (err == _ENOMEM || ((GOOS == "solaris" || GOOS == "illumos") && err == _sunosEAGAIN))
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
