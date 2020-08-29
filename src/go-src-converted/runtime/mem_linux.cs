// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:17:47 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mem_linux.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _EACCES = 13L;
        private static readonly long _EINVAL = 22L;

        // NOTE: vec must be just 1 byte long here.
        // Mincore returns ENOMEM if any of the pages are unmapped,
        // but we want to know that all of the pages are unmapped.
        // To make these the same, we can only ask about one page
        // at a time. See golang.org/issue/7476.
        private static array<byte> addrspace_vec = new array<byte>(1L);

        private static bool addrspace_free(unsafe.Pointer v, System.UIntPtr n)
        {
            {
                var off = uintptr(0L);

                while (off < n)
                { 
                    // Use a length of 1 byte, which the kernel will round
                    // up to one physical page regardless of the true
                    // physical page size.
                    var errval = mincore(@unsafe.Pointer(uintptr(v) + off), 1L, ref addrspace_vec[0L]);
                    if (errval == -_EINVAL)
                    { 
                        // Address is not a multiple of the physical
                        // page size. Shouldn't happen, but just ignore it.
                        continue;
                    off += physPageSize;
                    } 
                    // ENOMEM means unmapped, which is what we want.
                    // Anything else we assume means the pages are mapped.
                    if (errval != -_ENOMEM)
                    {
                        return false;
                    }
                }

            }
            return true;
        }

        private static (unsafe.Pointer, long) mmap_fixed(unsafe.Pointer v, System.UIntPtr n, int prot, int flags, int fd, uint offset)
        {
            var (p, err) = mmap(v, n, prot, flags, fd, offset); 
            // On some systems, mmap ignores v without
            // MAP_FIXED, so retry if the address space is free.
            if (p != v && addrspace_free(v, n))
            {
                if (err == 0L)
                {
                    munmap(p, n);
                }
                p, err = mmap(v, n, prot, flags | _MAP_FIXED, fd, offset);
            }
            return (p, err);
        }

        // Don't split the stack as this method may be invoked without a valid G, which
        // prevents us from allocating more stack.
        //go:nosplit
        private static unsafe.Pointer sysAlloc(System.UIntPtr n, ref ulong sysStat)
        {
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

        private static void sysUnused(unsafe.Pointer v, System.UIntPtr n)
        { 
            // By default, Linux's "transparent huge page" support will
            // merge pages into a huge page if there's even a single
            // present regular page, undoing the effects of the DONTNEED
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
            if (sys.HugePageSize != 0L)
            {
                System.UIntPtr s = sys.HugePageSize; // division by constant 0 is a compile-time error :(

                // If it's a large allocation, we want to leave huge
                // pages enabled. Hence, we only adjust the huge page
                // flag on the huge pages containing v and v+n-1, and
                // only if those aren't aligned.
                System.UIntPtr head = default;                System.UIntPtr tail = default;

                if (uintptr(v) % s != 0L)
                { 
                    // Compute huge page containing v.
                    head = uintptr(v) & ~(s - 1L);
                }
                if ((uintptr(v) + n) % s != 0L)
                { 
                    // Compute huge page containing v+n-1.
                    tail = (uintptr(v) + n - 1L) & ~(s - 1L);
                } 

                // Note that madvise will return EINVAL if the flag is
                // already set, which is quite likely. We ignore
                // errors.
                if (head != 0L && head + sys.HugePageSize == tail)
                { 
                    // head and tail are different but adjacent,
                    // so do this in one call.
                    madvise(@unsafe.Pointer(head), 2L * sys.HugePageSize, _MADV_NOHUGEPAGE);
                }
                else
                { 
                    // Advise the huge pages containing v and v+n-1.
                    if (head != 0L)
                    {
                        madvise(@unsafe.Pointer(head), sys.HugePageSize, _MADV_NOHUGEPAGE);
                    }
                    if (tail != 0L && tail != head)
                    {
                        madvise(@unsafe.Pointer(tail), sys.HugePageSize, _MADV_NOHUGEPAGE);
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
            madvise(v, n, _MADV_DONTNEED);
        }

        private static void sysUsed(unsafe.Pointer v, System.UIntPtr n)
        {
            if (sys.HugePageSize != 0L)
            { 
                // Partially undo the NOHUGEPAGE marks from sysUnused
                // for whole huge pages between v and v+n. This may
                // leave huge pages off at the end points v and v+n
                // even though allocations may cover these entire huge
                // pages. We could detect this and undo NOHUGEPAGE on
                // the end points as well, but it's probably not worth
                // the cost because when neighboring allocations are
                // freed sysUnused will just set NOHUGEPAGE again.
                System.UIntPtr s = sys.HugePageSize; 

                // Round v up to a huge page boundary.
                var beg = (uintptr(v) + (s - 1L)) & ~(s - 1L); 
                // Round v+n down to a huge page boundary.
                var end = (uintptr(v) + n) & ~(s - 1L);

                if (beg < end)
                {
                    madvise(@unsafe.Pointer(beg), end - beg, _MADV_HUGEPAGE);
                }
            }
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
            // On 64-bit, people with ulimit -v set complain if we reserve too
            // much address space. Instead, assume that the reservation is okay
            // if we can reserve at least 64K and check the assumption in SysMap.
            // Only user-mode Linux (UML) rejects these requests.
            if (sys.PtrSize == 8L && uint64(n) > 1L << (int)(32L))
            {
                var (p, err) = mmap_fixed(v, 64L << (int)(10L), _PROT_NONE, _MAP_ANON | _MAP_PRIVATE, -1L, 0L);
                if (p != v || err != 0L)
                {
                    if (err == 0L)
                    {
                        munmap(p, 64L << (int)(10L));
                    }
                    return null;
                }
                munmap(p, 64L << (int)(10L));
                reserved.Value = false;
                return v;
            }
            (p, err) = mmap(v, n, _PROT_NONE, _MAP_ANON | _MAP_PRIVATE, -1L, 0L);
            if (err != 0L)
            {
                return null;
            }
            reserved.Value = true;
            return p;
        }

        private static void sysMap(unsafe.Pointer v, System.UIntPtr n, bool reserved, ref ulong sysStat)
        {
            mSysStatInc(sysStat, n); 

            // On 64-bit, we don't actually have v reserved, so tread carefully.
            if (!reserved)
            {
                var (p, err) = mmap_fixed(v, n, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_PRIVATE, -1L, 0L);
                if (err == _ENOMEM)
                {
                    throw("runtime: out of memory");
                }
                if (p != v || err != 0L)
                {
                    print("runtime: address space conflict: map(", v, ") = ", p, " (err ", err, ")\n");
                    throw("runtime: address space conflict");
                }
                return;
            }
            (p, err) = mmap(v, n, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_FIXED | _MAP_PRIVATE, -1L, 0L);
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
