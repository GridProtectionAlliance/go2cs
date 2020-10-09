// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 09 05:00:46 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\lockosthread.go
using os = go.os_package;
using runtime = go.runtime_package;
using sync = go.sync_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static long mainTID = default;

        private static void init()
        {
            registerInit("LockOSThreadMain", () =>
            { 
                // init is guaranteed to run on the main thread.
                mainTID = gettid();

            });
            register("LockOSThreadMain", LockOSThreadMain);

            registerInit("LockOSThreadAlt", () =>
            { 
                // Lock the OS thread now so main runs on the main thread.
                runtime.LockOSThread();

            });
            register("LockOSThreadAlt", LockOSThreadAlt);

            registerInit("LockOSThreadAvoidsStatePropagation", () =>
            { 
                // Lock the OS thread now so main runs on the main thread.
                runtime.LockOSThread();

            });
            register("LockOSThreadAvoidsStatePropagation", LockOSThreadAvoidsStatePropagation);
            register("LockOSThreadTemplateThreadRace", LockOSThreadTemplateThreadRace);

        }

        public static void LockOSThreadMain()
        { 
            // gettid only works on Linux, so on other platforms this just
            // checks that the runtime doesn't do anything terrible.

            // This requires GOMAXPROCS=1 from the beginning to reliably
            // start a goroutine on the main thread.
            if (runtime.GOMAXPROCS(-1L) != 1L)
            {
                println("requires GOMAXPROCS=1");
                os.Exit(1L);
            }

            var ready = make_channel<bool>(1L);
            go_(() => () =>
            { 
                // Because GOMAXPROCS=1, this *should* be on the main
                // thread. Stay there.
                runtime.LockOSThread();
                if (mainTID != 0L && gettid() != mainTID)
                {
                    println("failed to start goroutine on main thread");
                    os.Exit(1L);
                } 
                // Exit with the thread locked, which should exit the
                // main thread.
                ready.Send(true);

            }());
            ready.Receive();
            time.Sleep(1L * time.Millisecond); 
            // Check that this goroutine is still running on a different
            // thread.
            if (mainTID != 0L && gettid() == mainTID)
            {
                println("goroutine migrated to locked thread");
                os.Exit(1L);
            }

            println("OK");

        }

        public static void LockOSThreadAlt()
        { 
            // This is running locked to the main OS thread.

            long subTID = default;
            var ready = make_channel<bool>(1L);
            go_(() => () =>
            { 
                // This goroutine must be running on a new thread.
                runtime.LockOSThread();
                subTID = gettid();
                ready.Send(true); 
                // Exit with the thread locked.
            }());
            ready.Receive();
            runtime.UnlockOSThread();
            for (long i = 0L; i < 100L; i++)
            {
                time.Sleep(1L * time.Millisecond); 
                // Check that this goroutine is running on a different thread.
                if (subTID != 0L && gettid() == subTID)
                {
                    println("locked thread reused");
                    os.Exit(1L);
                }

                var (exists, supported) = tidExists(subTID);
                if (!supported || !exists)
                {
                    goto ok;
                }

            }

            println("sub thread", subTID, "still running");
            return ;
ok:
            println("OK");

        }

        public static void LockOSThreadAvoidsStatePropagation()
        { 
            // This test is similar to LockOSThreadAlt in that it will detect if a thread
            // which should have died is still running. However, rather than do this with
            // thread IDs, it does this by unsharing state on that thread. This way, it
            // also detects whether new threads were cloned from the dead thread, and not
            // from a clean thread. Cloning from a locked thread is undesirable since
            // cloned threads will inherit potentially unwanted OS state.
            //
            // unshareFs, getcwd, and chdir("/tmp") are only guaranteed to work on
            // Linux, so on other platforms this just checks that the runtime doesn't
            // do anything terrible.
            //
            // This is running locked to the main OS thread.

            // GOMAXPROCS=1 makes this fail much more reliably if a tainted thread is
            // cloned from.
            if (runtime.GOMAXPROCS(-1L) != 1L)
            {
                println("requires GOMAXPROCS=1");
                os.Exit(1L);
            }

            {
                var err__prev1 = err;

                var err = chdir("/");

                if (err != null)
                {
                    println("failed to chdir:", err.Error());
                    os.Exit(1L);
                } 
                // On systems other than Linux, cwd == "".

                err = err__prev1;

            } 
            // On systems other than Linux, cwd == "".
            var (cwd, err) = getcwd();
            if (err != null)
            {
                println("failed to get cwd:", err.Error());
                os.Exit(1L);
            }

            if (cwd != "" && cwd != "/")
            {
                println("unexpected cwd", cwd, " wanted /");
                os.Exit(1L);
            }

            var ready = make_channel<bool>(1L);
            go_(() => () =>
            { 
                // This goroutine must be running on a new thread.
                runtime.LockOSThread(); 

                // Unshare details about the FS, like the CWD, with
                // the rest of the process on this thread.
                // On systems other than Linux, this is a no-op.
                {
                    var err__prev1 = err;

                    err = unshareFs();

                    if (err != null)
                    {
                        if (err == errNotPermitted)
                        {
                            println("unshare not permitted");
                            os.Exit(0L);
                        }

                        println("failed to unshare fs:", err.Error());
                        os.Exit(1L);

                    } 
                    // Chdir to somewhere else on this thread.
                    // On systems other than Linux, this is a no-op.

                    err = err__prev1;

                } 
                // Chdir to somewhere else on this thread.
                // On systems other than Linux, this is a no-op.
                {
                    var err__prev1 = err;

                    err = chdir("/tmp");

                    if (err != null)
                    {
                        println("failed to chdir:", err.Error());
                        os.Exit(1L);
                    } 

                    // The state on this thread is now considered "tainted", but it
                    // should no longer be observable in any other context.

                    err = err__prev1;

                } 

                // The state on this thread is now considered "tainted", but it
                // should no longer be observable in any other context.

                ready.Send(true); 
                // Exit with the thread locked.
            }());
            ready.Receive(); 

            // Spawn yet another goroutine and lock it. Since GOMAXPROCS=1, if
            // for some reason state from the (hopefully dead) locked thread above
            // propagated into a newly created thread (via clone), or that thread
            // is actually being re-used, then we should get scheduled on such a
            // thread with high likelihood.
            var done = make_channel<bool>();
            go_(() => () =>
            {
                runtime.LockOSThread(); 

                // Get the CWD and check if this is the same as the main thread's
                // CWD. Every thread should share the same CWD.
                // On systems other than Linux, wd == "".
                var (wd, err) = getcwd();
                if (err != null)
                {
                    println("failed to get cwd:", err.Error());
                    os.Exit(1L);
                }

                if (wd != cwd)
                {
                    println("bad state from old thread propagated after it should have died");
                    os.Exit(1L);
                }

                done.Receive();

                runtime.UnlockOSThread();

            }());
            done.Send(true);
            runtime.UnlockOSThread();
            println("OK");

        }

        public static void LockOSThreadTemplateThreadRace()
        { 
            // This test attempts to reproduce the race described in
            // golang.org/issue/38931. To do so, we must have a stop-the-world
            // (achieved via ReadMemStats) racing with two LockOSThread calls.
            //
            // While this test attempts to line up the timing, it is only expected
            // to fail (and thus hang) around 2% of the time if the race is
            // present.

            // Ensure enough Ps to actually run everything in parallel. Though on
            // <4 core machines, we are still at the whim of the kernel scheduler.
            runtime.GOMAXPROCS(4L);

            go_(() => () =>
            { 
                // Stop the world; race with LockOSThread below.
                ref runtime.MemStats m = ref heap(out ptr<runtime.MemStats> _addr_m);
                while (true)
                {
                    runtime.ReadMemStats(_addr_m);
                }


            }()); 

            // Try to synchronize both LockOSThreads.
            var start = time.Now().Add(10L * time.Millisecond);

            sync.WaitGroup wg = default;
            wg.Add(2L);

            for (long i = 0L; i < 2L; i++)
            {
                go_(() => () =>
                {
                    while (time.Now().Before(start))
                    {
                    } 

                    // Add work to the local runq to trigger early startm
                    // in handoffp.
 

                    // Add work to the local runq to trigger early startm
                    // in handoffp.
                    go_(() => () =>
                    {
                    }());

                    runtime.LockOSThread();
                    runtime.Gosched(); // add a preemption point.
                    wg.Done();

                }());

            }


            wg.Wait(); 
            // If both LockOSThreads completed then we did not hit the race.
            println("OK");

        }
    }
}
