// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build !plan9,!windows

// package main -- go2cs converted at 2020 October 09 05:01:01 UTC
// Original source: C:\Go\src\runtime\testdata\testprogcgo\lockosthread.go
using os = go.os_package;
using runtime = go.runtime_package;
using atomic = go.sync.atomic_package;
using time = go.time_package;
using @unsafe = go.@unsafe_package;
using C = go.C_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class main_package
    {
        private static C.pthread_t mainThread = default;

        private static void init()
        {
            registerInit("LockOSThreadMain", () =>
            { 
                // init is guaranteed to run on the main thread.
                mainThread = C.pthread_self();

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
                var self = C.pthread_self();
                if (C.pthread_equal(mainThread, self) == 0L)
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
            self = C.pthread_self();
            if (C.pthread_equal(mainThread, self) != 0L)
            {
                println("goroutine migrated to locked thread");
                os.Exit(1L);
            }

            println("OK");

        }

        public static void LockOSThreadAlt()
        { 
            // This is running locked to the main OS thread.

            C.pthread_t subThread = default;
            var ready = make_channel<bool>(1L);
            C.threadExited = 0L;
            go_(() => () =>
            { 
                // This goroutine must be running on a new thread.
                runtime.LockOSThread();
                subThread = C.pthread_self(); 
                // Register a pthread destructor so we can tell this
                // thread has exited.
                ref C.pthread_key_t key = ref heap(out ptr<C.pthread_key_t> _addr_key);
                C.pthread_key_create(_addr_key, new ptr<ptr<array<byte>>>(@unsafe.Pointer(C.setExited)));
                C.pthread_setspecific(key, @unsafe.Pointer(@new<int>()));
                ready.Send(true); 
                // Exit with the thread locked.
            }());
            ready.Receive();
            for (long i = 0L; i < 100L; i++)
            {
                time.Sleep(1L * time.Millisecond); 
                // Check that this goroutine is running on a different thread.
                var self = C.pthread_self();
                if (C.pthread_equal(subThread, self) != 0L)
                {
                    println("locked thread reused");
                    os.Exit(1L);
                }

                if (atomic.LoadUint32((uint32.val)(_addr_C.threadExited)) != 0L)
                {
                    println("OK");
                    return ;
                }

            }

            println("sub thread still running");
            os.Exit(1L);

        }
    }
}
