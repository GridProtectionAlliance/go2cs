// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 03:44:04 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\stack_windows.go
using C = go.C_package;
using windows = go.@internal.syscall.windows_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using syscall = go.syscall_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("StackMemory", StackMemory);
        }

        private static (System.UIntPtr, error) getPagefileUsage()
        {
            System.UIntPtr _p0 = default;
            error _p0 = default!;

            var (p, err) = syscall.GetCurrentProcess();
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            ref windows.PROCESS_MEMORY_COUNTERS m = ref heap(out ptr<windows.PROCESS_MEMORY_COUNTERS> _addr_m);
            err = windows.GetProcessMemoryInfo(p, _addr_m, uint32(@unsafe.Sizeof(m)));
            if (err != null)
            {
                return (0L, error.As(err)!);
            }

            return (m.PagefileUsage, error.As(null!)!);

        }

        public static void StackMemory() => func((_, panic, __) =>
        {
            var (mem1, err) = getPagefileUsage();
            if (err != null)
            {
                panic(err);
            }

            const long threadCount = (long)100L;

            sync.WaitGroup wg = default;
            for (long i = 0L; i < threadCount; i++)
            {
                wg.Add(1L);
                go_(() => () =>
                {
                    runtime.LockOSThread();
                    wg.Done();
                }());

            }

            wg.Wait();
            var (mem2, err) = getPagefileUsage();
            if (err != null)
            {
                panic(err);
            }

            print((mem2 - mem1) / threadCount);

        });
    }
}
