// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 August 29 08:24:27 UTC
// Original source: C:\Go\src\runtime\testdata\testprog\lockosthread.go
using os = go.os_package;
using runtime = go.runtime_package;
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
            return;
ok:
            println("OK");
        }
    }
}
