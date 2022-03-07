// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sync -- go2cs converted at 2022 March 06 22:26:23 UTC
// import "sync" ==> using sync = go.sync_package
// Original source: C:\Program Files\Go\src\sync\runtime.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class sync_package {

    // defined in package runtime

    // Semacquire waits until *s > 0 and then atomically decrements it.
    // It is intended as a simple sleep primitive for use by the synchronization
    // library and should not be used directly.
private static void runtime_Semacquire(ptr<uint> s);

// SemacquireMutex is like Semacquire, but for profiling contended Mutexes.
// If lifo is true, queue waiter at the head of wait queue.
// skipframes is the number of frames to omit during tracing, counting from
// runtime_SemacquireMutex's caller.
private static void runtime_SemacquireMutex(ptr<uint> s, bool lifo, nint skipframes);

// Semrelease atomically increments *s and notifies a waiting goroutine
// if one is blocked in Semacquire.
// It is intended as a simple wakeup primitive for use by the synchronization
// library and should not be used directly.
// If handoff is true, pass count directly to the first waiter.
// skipframes is the number of frames to omit during tracing, counting from
// runtime_Semrelease's caller.
private static void runtime_Semrelease(ptr<uint> s, bool handoff, nint skipframes);

// See runtime/sema.go for documentation.
private static uint runtime_notifyListAdd(ptr<notifyList> l);

// See runtime/sema.go for documentation.
private static void runtime_notifyListWait(ptr<notifyList> l, uint t);

// See runtime/sema.go for documentation.
private static void runtime_notifyListNotifyAll(ptr<notifyList> l);

// See runtime/sema.go for documentation.
private static void runtime_notifyListNotifyOne(ptr<notifyList> l);

// Ensure that sync and runtime agree on size of notifyList.
private static void runtime_notifyListCheck(System.UIntPtr size);
private static void init() {
    notifyList n = default;
    runtime_notifyListCheck(@unsafe.Sizeof(n));
}

// Active spinning runtime support.
// runtime_canSpin reports whether spinning makes sense at the moment.
private static bool runtime_canSpin(nint i);

// runtime_doSpin does active spinning.
private static void runtime_doSpin();

private static long runtime_nanotime();

} // end sync_package
