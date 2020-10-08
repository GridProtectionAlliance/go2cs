// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd
// +build 386 amd64

// package runtime -- go2cs converted at 2020 October 08 03:24:22 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\vdso_freebsd_x86.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _VDSO_TH_ALGO_X86_TSC = (long)1L;
        private static readonly long _VDSO_TH_ALGO_X86_HPET = (long)2L;


        private static readonly long _HPET_DEV_MAP_MAX = (long)10L;
        private static readonly ulong _HPET_MAIN_COUNTER = (ulong)0xf0UL;        /* Main counter register */

        private static readonly @string hpetDevPath = (@string)"/dev/hpetX\x00";


        private static array<System.UIntPtr> hpetDevMap = new array<System.UIntPtr>(_HPET_DEV_MAP_MAX);

        //go:nosplit
        private static uint getTSCTimecounter(this ptr<vdsoTimehands> _addr_th)
        {
            ref vdsoTimehands th = ref _addr_th.val;

            var tsc = cputicks();
            if (th.x86_shift > 0L)
            {
                tsc >>= th.x86_shift;
            }

            return uint32(tsc);

        }

        //go:systemstack
        private static (uint, bool) getHPETTimecounter(this ptr<vdsoTimehands> _addr_th)
        {
            uint _p0 = default;
            bool _p0 = default;
            ref vdsoTimehands th = ref _addr_th.val;

            const @string digits = (@string)"0123456789";



            var idx = int(th.x86_hpet_idx);
            if (idx >= len(hpetDevMap))
            {
                return (0L, false);
            }

            var p = atomic.Loaduintptr(_addr_hpetDevMap[idx]);
            if (p == 0L)
            {
                array<byte> devPath = new array<byte>(len(hpetDevPath));
                copy(devPath[..], hpetDevPath);
                devPath[9L] = digits[idx];

                var fd = open(_addr_devPath[0L], 0L, 0L);
                if (fd < 0L)
                {
                    atomic.Casuintptr(_addr_hpetDevMap[idx], 0L, ~uintptr(0L));
                    return (0L, false);
                }

                var (addr, mmapErr) = mmap(null, physPageSize, _PROT_READ, _MAP_SHARED, fd, 0L);
                closefd(fd);
                var newP = uintptr(addr);
                if (mmapErr != 0L)
                {
                    newP = ~uintptr(0L);
                }

                if (!atomic.Casuintptr(_addr_hpetDevMap[idx], 0L, newP) && mmapErr == 0L)
                {
                    munmap(addr, physPageSize);
                }

                p = atomic.Loaduintptr(_addr_hpetDevMap[idx]);

            }

            if (p == ~uintptr(0L))
            {
                return (0L, false);
            }

            return (new ptr<ptr<ptr<uint>>>(@unsafe.Pointer(p + _HPET_MAIN_COUNTER)), true);

        }

        //go:nosplit
        private static (uint, bool) getTimecounter(this ptr<vdsoTimehands> _addr_th)
        {
            uint _p0 = default;
            bool _p0 = default;
            ref vdsoTimehands th = ref _addr_th.val;


            if (th.algo == _VDSO_TH_ALGO_X86_TSC) 
                return (th.getTSCTimecounter(), true);
            else if (th.algo == _VDSO_TH_ALGO_X86_HPET) 
                uint tc = default;                bool ok = default;
                systemstack(() =>
                {
                    tc, ok = th.getHPETTimecounter();
                });
                return (tc, ok);
            else 
                return (0L, false);
            
        }
    }
}
