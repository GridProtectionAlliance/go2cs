// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:20:37 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mem_linux.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _EACCES = (long)13L;
        private static readonly long _EINVAL = (long)22L;


        // Don't split the stack as this method may be invoked without a valid G, which
        // prevents us from allocating more stack.
        //go:nosplit
        private static unsafe.Pointer sysAlloc(System.UIntPtr n, ptr<ulong> _addr_sysStat)
        {
            ref ulong sysStat = ref _addr_sysStat.val;

            var (p, err) = mmap(null, n, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_PRIVATE, -1L, 0L);
            if (err != 0L)
            {
                if (err == _EACCES)
                {
                    print("runtime: mmap: access denied\n");
                    exit(2L);
                }

                if (err == _EAGAIN)
                {
                    print("runtime: mmap: too much locked memory (check 'ulimit -l').\n");
                    exit(2L);
                }

                return null;

            }

            mSysStatInc(sysStat, n);
            return p;

        }

        private static var adviseUnused = uint32(_MADV_FREE);

        private static void sysUnused(unsafe.Pointer v, System.UIntPtr n)
        { 
            // By default, Linux's "transparent huge page" support will
            // merge pages into a huge page if there's even a single
            // present regular page, undoing the effects of madvise(adviseUnused)
            // below. On amd64, that means khugepaged can turn a single
            // 4KB page to 2MB, bloating the process's RSS by as much as
            // 512X. (See issue #8832 and Linux kernel bug
            // https://bugzilla.kernel.org/show_bug.cgi?id=93111)
            //
            // To work around this, we explicitly disable transparent huge
            // pages when we release pages of the heap. However, we have
            // to do this carefully because changing this flag tends to
            // split the VMA (memory mapping) containing v in to three
            // VMAs in order to track the different values of the
            // MADV_NOHUGEPAGE flag in the different regions. There's a
            // default limit of 65530 VMAs per address space (sysctl
            // vm.max_map_count), so we must be careful not to create too
            // many VMAs (see issue #12233).
            //
            // Since huge pages are huge, there's little use in adjusting
            // the MADV_NOHUGEPAGE flag on a fine granularity, so we avoid
            // exploding the number of VMAs by only adjusting the
            // MADV_NOHUGEPAGE flag on a large granularity. This still
            // gets most of the benefit of huge pages while keeping the
            // number of VMAs under control. With hugePageSize = 2MB, even
            // a pessimal heap can reach 128GB before running out of VMAs.
            if (physHugePageSize != 0L)
            { 
                // If it's a large allocation, we want to leave huge
                // pages enabled. Hence, we only adjust the huge page
                // flag on the huge pages containing v and v+n-1, and
                // only if those aren't aligned.
                System.UIntPtr head = default;                System.UIntPtr tail = default;

                if (uintptr(v) & (physHugePageSize - 1L) != 0L)
                { 
                    // Compute huge page containing v.
                    head = alignDown(uintptr(v), physHugePageSize);

                }

                if ((uintptr(v) + n) & (physHugePageSize - 1L) != 0L)
                { 
                    // Compute huge page containing v+n-1.
                    tail = alignDown(uintptr(v) + n - 1L, physHugePageSize);

                } 

                // Note that madvise will return EINVAL if the flag is
                // already set, which is quite likely. We ignore
                // errors.
                if (head != 0L && head + physHugePageSize == tail)
                { 
                    // head and tail are different but adjacent,
                    // so do this in one call.
                    madvise(@unsafe.Pointer(head), 2L * physHugePageSize, _MADV_NOHUGEPAGE);

                }
                else
                { 
                    // Advise the huge pages containing v and v+n-1.
                    if (head != 0L)
                    {
                        madvise(@unsafe.Pointer(head), physHugePageSize, _MADV_NOHUGEPAGE);
                    }

                    if (tail != 0L && tail != head)
                    {
                        madvise(@unsafe.Pointer(tail), physHugePageSize, _MADV_NOHUGEPAGE);
                    }

                }

            }

            if (uintptr(v) & (physPageSize - 1L) != 0L || n & (physPageSize - 1L) != 0L)
            { 
                // madvise will round this to any physical page
                // *covered* by this range, so an unaligned madvise
                // will release more memory than intended.
                throw("unaligned sysUnused");

            }

            uint advise = default;
            if (debug.madvdontneed != 0L)
            {
                advise = _MADV_DONTNEED;
            }
            else
            {
                advise = atomic.Load(_addr_adviseUnused);
            }

            {
                var errno = madvise(v, n, int32(advise));

                if (advise == _MADV_FREE && errno != 0L)
                { 
                    // MADV_FREE was added in Linux 4.5. Fall back to MADV_DONTNEED if it is
                    // not supported.
                    atomic.Store(_addr_adviseUnused, _MADV_DONTNEED);
                    madvise(v, n, _MADV_DONTNEED);

                }

            }

        }

        private static void sysUsed(unsafe.Pointer v, System.UIntPtr n)
        { 
            // Partially undo the NOHUGEPAGE marks from sysUnused
            // for whole huge pages between v and v+n. This may
            // leave huge pages off at the end points v and v+n
            // even though allocations may cover these entire huge
            // pages. We could detect this and undo NOHUGEPAGE on
            // the end points as well, but it's probably not worth
            // the cost because when neighboring allocations are
            // freed sysUnused will just set NOHUGEPAGE again.
            sysHugePage(v, n);

        }

        private static void sysHugePage(unsafe.Pointer v, System.UIntPtr n)
        {
            if (physHugePageSize != 0L)
            { 
                // Round v up to a huge page boundary.
                var beg = alignUp(uintptr(v), physHugePageSize); 
                // Round v+n down to a huge page boundary.
                var end = alignDown(uintptr(v) + n, physHugePageSize);

                if (beg < end)
                {
                    madvise(@unsafe.Pointer(beg), end - beg, _MADV_HUGEPAGE);
                }

            }

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

        private static unsafe.Pointer sysReserve(unsafe.Pointer v, System.UIntPtr n)
        {
            var (p, err) = mmap(v, n, _PROT_NONE, _MAP_ANON | _MAP_PRIVATE, -1L, 0L);
            if (err != 0L)
            {
                return null;
            }

            return p;

        }

        private static void sysMap(unsafe.Pointer v, System.UIntPtr n, ptr<ulong> _addr_sysStat)
        {
            ref ulong sysStat = ref _addr_sysStat.val;

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
