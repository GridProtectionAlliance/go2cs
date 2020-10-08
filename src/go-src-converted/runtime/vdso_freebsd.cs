// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build freebsd

// package runtime -- go2cs converted at 2020 October 08 03:24:21 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\vdso_freebsd.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _VDSO_TH_NUM = (long)4L; // defined in <sys/vdso.h> #ifdef _KERNEL

 // defined in <sys/vdso.h> #ifdef _KERNEL

        private static ptr<vdsoTimekeep> timekeepSharedPage;

        //go:nosplit
        private static void Add(this ptr<bintime> _addr_bt, ptr<bintime> _addr_bt2)
        {
            ref bintime bt = ref _addr_bt.val;
            ref bintime bt2 = ref _addr_bt2.val;

            var u = bt.frac;
            bt.frac += bt2.frac;
            if (u > bt.frac)
            {
                bt.sec++;
            }

            bt.sec += bt2.sec;

        }

        //go:nosplit
        private static void AddX(this ptr<bintime> _addr_bt, ulong x)
        {
            ref bintime bt = ref _addr_bt.val;

            var u = bt.frac;
            bt.frac += x;
            if (u > bt.frac)
            {
                bt.sec++;
            }

        }

 
        // binuptimeDummy is used in binuptime as the address of an atomic.Load, to simulate
        // an atomic_thread_fence_acq() call which behaves as an instruction reordering and
        // memory barrier.
        private static uint binuptimeDummy = default;        private static bintime zeroBintime = default;

        // based on /usr/src/lib/libc/sys/__vdso_gettimeofday.c
        //
        //go:nosplit
        private static bintime binuptime(bool abs)
        {
            bintime bt = default;

            ptr<array<vdsoTimehands>> timehands = new ptr<ptr<array<vdsoTimehands>>>(add(@unsafe.Pointer(timekeepSharedPage), vdsoTimekeepSize));
            while (true)
            {
                if (timekeepSharedPage.enabled == 0L)
                {
                    return zeroBintime;
                }

                var curr = atomic.Load(_addr_timekeepSharedPage.current); // atomic_load_acq_32
                var th = _addr_timehands[curr];
                var gen = atomic.Load(_addr_th.gen); // atomic_load_acq_32
                bt = th.offset;

                {
                    var (tc, ok) = th.getTimecounter();

                    if (!ok)
                    {
                        return zeroBintime;
                    }
                    else
                    {
                        var delta = (tc - th.offset_count) & th.counter_mask;
                        bt.AddX(th.scale * uint64(delta));
                    }

                }

                if (abs)
                {
                    bt.Add(_addr_th.boottime);
                }

                atomic.Load(_addr_binuptimeDummy); // atomic_thread_fence_acq()
                if (curr == timekeepSharedPage.current && gen != 0L && gen == th.gen)
                {
                    break;
                }

            }

            return bt;

        }

        //go:nosplit
        private static bintime vdsoClockGettime(int clockID)
        {
            if (timekeepSharedPage == null || timekeepSharedPage.ver != _VDSO_TK_VER_CURR)
            {
                return zeroBintime;
            }

            var abs = false;

            if (clockID == _CLOCK_MONOTONIC)             else if (clockID == _CLOCK_REALTIME) 
                abs = true;
            else 
                return zeroBintime;
                        return binuptime(abs);

        }

        private static long fallback_nanotime()
;
        private static (long, int) fallback_walltime()
;

        //go:nosplit
        private static long nanotime1()
        {
            var bt = vdsoClockGettime(_CLOCK_MONOTONIC);
            if (bt == zeroBintime)
            {>>MARKER:FUNCTION_fallback_walltime_BLOCK_PREFIX<<
                return fallback_nanotime();
            }

            return int64((1e9F * uint64(bt.sec)) + ((1e9F * uint64(bt.frac >> (int)(32L))) >> (int)(32L)));

        }

        private static (long, int) walltime1()
        {
            long sec = default;
            int nsec = default;

            var bt = vdsoClockGettime(_CLOCK_REALTIME);
            if (bt == zeroBintime)
            {>>MARKER:FUNCTION_fallback_nanotime_BLOCK_PREFIX<<
                return fallback_walltime();
            }

            return (int64(bt.sec), int32((1e9F * uint64(bt.frac >> (int)(32L))) >> (int)(32L)));

        }
    }
}
