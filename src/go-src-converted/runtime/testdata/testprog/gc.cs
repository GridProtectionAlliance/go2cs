// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 08:24:25 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\gc.go
using fmt = go.fmt_package;
using os = go.os_package;
using runtime = go.runtime_package;
using debug = go.runtime.debug_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register("GCFairness", GCFairness);
            register("GCFairness2", GCFairness2);
            register("GCSys", GCSys);
        }

        public static void GCSys()
        {
            runtime.GOMAXPROCS(1L);
            ptr<object> memstats = @new<runtime.MemStats>();
            runtime.GC();
            runtime.ReadMemStats(memstats);
            var sys = memstats.Sys;

            runtime.MemProfileRate = 0L; // disable profiler

            long itercount = 100000L;
            for (long i = 0L; i < itercount; i++)
            {
                workthegc();
            } 

            // Should only be using a few MB.
            // We allocated 100 MB or (if not short) 1 GB.
 

            // Should only be using a few MB.
            // We allocated 100 MB or (if not short) 1 GB.
            runtime.ReadMemStats(memstats);
            if (sys > memstats.Sys)
            {
                sys = 0L;
            }
            else
            {
                sys = memstats.Sys - sys;
            }
            if (sys > 16L << (int)(20L))
            {
                fmt.Printf("using too much memory: %d bytes\n", sys);
                return;
            }
            fmt.Printf("OK\n");
        }

        private static slice<byte> workthegc()
        {
            return make_slice<byte>(1029L);
        }

        public static void GCFairness()
        {
            runtime.GOMAXPROCS(1L);
            var (f, err) = os.Open("/dev/null");
            if (os.IsNotExist(err))
            { 
                // This test tests what it is intended to test only if writes are fast.
                // If there is no /dev/null, we just don't execute the test.
                fmt.Println("OK");
                return;
            }
            if (err != null)
            {
                fmt.Println(err);
                os.Exit(1L);
            }
            for (long i = 0L; i < 2L; i++)
            {
                go_(() => () =>
                {
                    while (true)
                    {
                        f.Write((slice<byte>)".");
                    }

                }());
            }

            time.Sleep(10L * time.Millisecond);
            fmt.Println("OK");
        }

        public static void GCFairness2()
        { 
            // Make sure user code can't exploit the GC's high priority
            // scheduling to make scheduling of user code unfair. See
            // issue #15706.
            runtime.GOMAXPROCS(1L);
            debug.SetGCPercent(1L);
            array<long> count = new array<long>(3L);
            var sink = default;
            {
                var i__prev1 = i;

                foreach (var (__i) in count)
                {
                    i = __i;
                    go_(() => i =>
                    {
                        while (true)
                        {
                            sink[i] = make_slice<byte>(1024L);
                            atomic.AddInt64(ref count[i], 1L);
                        }

                    }(i));
                } 
                // Note: If the unfairness is really bad, it may not even get
                // past the sleep.
                //
                // If the scheduling rules change, this may not be enough time
                // to let all goroutines run, but for now we cycle through
                // them rapidly.
                //
                // OpenBSD's scheduler makes every usleep() take at least
                // 20ms, so we need a long time to ensure all goroutines have
                // run. If they haven't run after 30ms, give it another 1000ms
                // and check again.

                i = i__prev1;
            }

            time.Sleep(30L * time.Millisecond);
            bool fail = default;
            {
                var i__prev1 = i;

                foreach (var (__i) in count)
                {
                    i = __i;
                    if (atomic.LoadInt64(ref count[i]) == 0L)
                    {
                        fail = true;
                    }
                }

                i = i__prev1;
            }

            if (fail)
            {
                time.Sleep(1L * time.Second);
                {
                    var i__prev1 = i;

                    foreach (var (__i) in count)
                    {
                        i = __i;
                        if (atomic.LoadInt64(ref count[i]) == 0L)
                        {
                            fmt.Printf("goroutine %d did not run\n", i);
                            return;
                        }
                    }

                    i = i__prev1;
                }

            }
            fmt.Println("OK");
        }
    }
}
