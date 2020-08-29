// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 08:24:42 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\syscall_windows.go
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
            register("RaiseException", RaiseException);
            register("ZeroDivisionException", ZeroDivisionException);
            register("StackMemory", StackMemory);
        }

        public static void RaiseException()
        {
            const long EXCEPTION_NONCONTINUABLE = 1L;

            var mod = syscall.MustLoadDLL("kernel32.dll");
            var proc = mod.MustFindProc("RaiseException");
            proc.Call(0xbadUL, EXCEPTION_NONCONTINUABLE, 0L, 0L);
            println("RaiseException should not return");
        }

        public static void ZeroDivisionException()
        {
            long x = 1L;
            long y = 0L;
            var z = x / y;
            println(z);
        }

        private static (System.UIntPtr, error) getPagefileUsage()
        {
            var (p, err) = syscall.GetCurrentProcess();
            if (err != null)
            {
                return (0L, err);
            }
            windows.PROCESS_MEMORY_COUNTERS m = default;
            err = windows.GetProcessMemoryInfo(p, ref m, uint32(@unsafe.Sizeof(m)));
            if (err != null)
            {
                return (0L, err);
            }
            return (m.PagefileUsage, null);
        }

        public static void StackMemory() => func((_, panic, __) =>
        {
            var (mem1, err) = getPagefileUsage();
            if (err != null)
            {
                panic(err);
            }
            const long threadCount = 100L;

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
