// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build dragonfly freebsd nacl netbsd openbsd solaris

// package runtime -- go2cs converted at 2020 August 29 08:17:45 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\mem_bsd.go
using sys = go.runtime.@internal.sys_package;
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
            // On 64-bit, people with ulimit -v set complain if we reserve too
            // much address space. Instead, assume that the reservation is okay
            // and check the assumption in SysMap.
            if (sys.PtrSize == 8L && uint64(n) > 1L << (int)(32L) || sys.GoosNacl != 0L)
            {
                reserved.Value = false;
                return v;
            }
            var (p, err) = mmap(v, n, _PROT_NONE, _MAP_ANON | _MAP_PRIVATE, -1L, 0L);
            if (err != 0L)
            {
                return null;
            }
            reserved.Value = true;
            return p;
        }

        private static readonly long _sunosEAGAIN = 11L;

        private static readonly long _ENOMEM = 12L;



        private static void sysMap(unsafe.Pointer v, System.UIntPtr n, bool reserved, ref ulong sysStat)
        {
            mSysStatInc(sysStat, n); 

            // On 64-bit, we don't actually have v reserved, so tread carefully.
            if (!reserved)
            {
                var flags = int32(_MAP_ANON | _MAP_PRIVATE);
                if (GOOS == "dragonfly")
                { 
                    // TODO(jsing): For some reason DragonFly seems to return
                    // memory at a different address than we requested, even when
                    // there should be no reason for it to do so. This can be
                    // avoided by using MAP_FIXED, but I'm not sure we should need
                    // to do this - we do not on other platforms.
                    flags |= _MAP_FIXED;
                }
                var (p, err) = mmap(v, n, _PROT_READ | _PROT_WRITE, flags, -1L, 0L);
                if (err == _ENOMEM || (GOOS == "solaris" && err == _sunosEAGAIN))
                {
                    throw("runtime: out of memory");
                }
                if (p != v || err != 0L)
                {
                    print("runtime: address space conflict: map(", v, ") = ", p, "(err ", err, ")\n");
                    throw("runtime: address space conflict");
                }
                return;
            }
            (p, err) = mmap(v, n, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_FIXED | _MAP_PRIVATE, -1L, 0L);
            if (err == _ENOMEM || (GOOS == "solaris" && err == _sunosEAGAIN))
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
