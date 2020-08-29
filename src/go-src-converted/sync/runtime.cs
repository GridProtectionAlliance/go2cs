// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sync -- go2cs converted at 2020 August 29 08:36:44 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Go\src\sync\runtime.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class sync_package
    {
        // defined in package runtime

        // Semacquire waits until *s > 0 and then atomically decrements it.
        // It is intended as a simple sleep primitive for use by the synchronization
        // library and should not be used directly.
        private static void runtime_Semacquire(ref uint s)
;

        // SemacquireMutex is like Semacquire, but for profiling contended Mutexes.
        // If lifo is true, queue waiter at the head of wait queue.
        private static void runtime_SemacquireMutex(ref uint s, bool lifo)
;

        // Semrelease atomically increments *s and notifies a waiting goroutine
        // if one is blocked in Semacquire.
        // It is intended as a simple wakeup primitive for use by the synchronization
        // library and should not be used directly.
        // If handoff is true, pass count directly to the first waiter.
        private static void runtime_Semrelease(ref uint s, bool handoff)
;

        // Approximation of notifyList in runtime/sema.go. Size and alignment must
        // agree.
        private partial struct notifyList
        {
            public uint wait;
            public uint notify;
            public System.UIntPtr @lock;
            public unsafe.Pointer head;
            public unsafe.Pointer tail;
        }

        // See runtime/sema.go for documentation.
        private static uint runtime_notifyListAdd(ref notifyList l)
;

        // See runtime/sema.go for documentation.
        private static void runtime_notifyListWait(ref notifyList l, uint t)
;

        // See runtime/sema.go for documentation.
        private static void runtime_notifyListNotifyAll(ref notifyList l)
;

        // See runtime/sema.go for documentation.
        private static void runtime_notifyListNotifyOne(ref notifyList l)
;

        // Ensure that sync and runtime agree on size of notifyList.
        private static void runtime_notifyListCheck(System.UIntPtr size)
;
        private static void init()
        {
            notifyList n = default;
            runtime_notifyListCheck(@unsafe.Sizeof(n));
        }

        // Active spinning runtime support.
        // runtime_canSpin returns true is spinning makes sense at the moment.
        private static bool runtime_canSpin(long i)
;

        // runtime_doSpin does active spinning.
        private static void runtime_doSpin()
;

        private static long runtime_nanotime()
;
    }
}
