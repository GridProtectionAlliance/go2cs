// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
using go;

// Hand-finished conversion (lock_sema.go — runtime mutex + one-time note).
//
// Go's mutex.key is a tagged atomic slot: 0 unlocked, `locked` (1) held, or an *m ADDRESS|locked
// heading a waiter chain through m.nextwaitm, parked on OS semaphores (semacreate/semasleep/
// semawakeup — OS-metal stubs in the managed conversion). A managed runtime cannot smuggle an m
// reference through the uintptr slot (the manual muintptr rightly panics on a non-zero integer),
// so the managed model keeps the SAME key protocol restricted to {0, locked} and replaces
// parking with SpinWait escalation (progressive Thread.Yield/Sleep — adaptive spinning). The
// note (one-time notification) never stores the waiting m either: it is a pure signaled/clear
// latch polled with SpinWait.
//
// PRESERVED contracts: mutual exclusion; release visibility (Interlocked full fences ≥ Go's
// atomics); notewakeup's double-wakeup throw; noteclear/mheap's `key = 0` re-init compatibility.
// NOT modeled (deliberately, documented): the waiter QUEUE (fairness/FIFO wakeup), lock
// profiling (lockTimer/mLockProfile), and the m.locks/preempt bookkeeping — getg() is a Go
// compiler intrinsic with no managed realization yet (a [ThreadStatic] g/m model is the future
// root that unlocks it); when getg lands, the bookkeeping lines return here. Known divergence:
// Go's throw() is process-fatal while managed exceptions are catchable, so an exception
// unwinding between lock2 and unlock2 orphans key=locked and later lockers poll forever —
// where Go would have died loudly (adversarial review, latent L2).
[module: GoManualConversion]

namespace go;

using System.Threading;

partial class runtime_package {

internal static bool mutexContended(ж<mutex> Ꮡl) {
    // No waiter chain exists in the managed model, so contention beyond the held bit is not
    // observable (consumed only by lock-profiling paths, which are not modeled).
    return false;
}

internal static void lock2(ж<mutex> Ꮡl) {
    ref var l = ref Ꮡl.val;

    // Speculative grab, then adaptive test-test-and-set spin (the Volatile.Read pre-test keeps
    // contended pollers off exclusive cache-line acquisition; SpinWait escalates spin → yield →
    // sleep, standing in for Go's active_spin/osyield/semasleep ladder).
    if (Interlocked.CompareExchange(ref l.key.Value, locked, 0) == 0) {
        return;
    }

    SpinWait spinner = default;

    while (Volatile.Read(ref l.key.Value) != 0 || Interlocked.CompareExchange(ref l.key.Value, locked, 0) != 0) {
        spinner.SpinOnce();
    }
}

// We might not be holding a p in this code.
internal static void unlock2(ж<mutex> Ꮡl) {
    ref var l = ref Ꮡl.val;

    // No waiter chain to dequeue — release the slot; a spinning lock2 observes it.
    Interlocked.Exchange(ref l.key.Value, 0);
}

// One-time notifications.
internal static void notewakeup(ж<note> Ꮡn) {
    ref var n = ref Ꮡn.val;

    uintptr v = Interlocked.Exchange(ref n.key.Value, locked);

    if (v == locked) {
        // Two notewakeups! Not allowed.
        @throw("notewakeup - double wakeup"u8);
    }
    // v == 0: nothing was waiting — done. A non-zero non-locked value (an m address in the
    // original) cannot occur: the managed notesleep never stores a reference into the slot.
}

internal static void notesleep(ж<note> Ꮡn) {
    ref var n = ref Ꮡn.val;

    SpinWait spinner = default;

    while (ᐧ) {
        uintptr v = Volatile.Read(ref n.key.Value);

        if (v == locked) {
            return;
        }

        if (v != 0) {
            // The slot only ever holds {0, locked} in the managed model — anything else is
            // corruption; keep Go's loud diagnostic rather than spinning silently.
            @throw("notesleep - waitm out of sync"u8);
        }

        spinner.SpinOnce();
    }
}

// Managed timeout latch: poll until signaled or the budget elapses. ns < 0 waits forever
// (as Go's semasleep(-1)); millisecond granularity stands in for Go's nanosecond budget
// (the note is a coarse rendezvous — parking precision is not part of its contract).
internal static bool notetsleep_internal(ж<note> Ꮡn, int64 ns, ж<g> Ꮡgp, int64 deadline) {
    ref var n = ref Ꮡn.val;

    if (ns < 0) {
        notesleep(Ꮡn);
        return true;
    }

    long deadlineMs = System.Environment.TickCount64 + (ns / 1000000) + 1;
    SpinWait spinner = default;

    while (ᐧ) {
        uintptr v = Volatile.Read(ref n.key.Value);

        if (v == locked) {
            return true;
        }

        if (v != 0) {
            @throw("notetsleep - waitm out of sync"u8);
        }

        if (System.Environment.TickCount64 >= deadlineMs) {
            return false;
        }

        spinner.SpinOnce();
    }
}

} // end runtime_package
