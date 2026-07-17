// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build aix || darwin || netbsd || openbsd || plan9 || solaris || windows
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// This implementation depends on OS-specific implementations of
//
//	func semacreate(mp *m)
//		Create a semaphore for mp, if it does not already have one.
//
//	func semasleep(ns int64) int32
//		If ns < 0, acquire m's semaphore and return 0.
//		If ns >= 0, try to acquire m's semaphore for at most ns nanoseconds.
//		Return 0 if the semaphore was acquired, -1 if interrupted or timed out.
//
//	func semawakeup(mp *m)
//		Wake up mp, which is or will soon be sleeping on its semaphore.
internal static readonly uintptr locked = 1;

internal static readonly UntypedInt active_spin = 4;

internal static readonly UntypedInt active_spin_cnt = 30;

internal static readonly UntypedInt passive_spin = 1;

// go2cs generated this placeholder — func mutexContended is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static void @lock(ж<mutex> Ꮡl) {
    lockWithRank(Ꮡl, getLockRank(Ꮡl));
}

// go2cs generated this placeholder — func lock2 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Speculative grab for lock.
// On uniprocessor's, no point spinning.
// On multiprocessors, spin for ACTIVE_SPIN attempts.
// Unlocked. Try to lock.
// Someone else has it.
// l->waitm points to a linked list of M's waiting
// for this lock, chained through m->nextwaitm.
// Queue this M.
// Queued. Wait.
internal static void unlock(ж<mutex> Ꮡl) {
    unlockWithRank(Ꮡl);
}

// go2cs generated this placeholder — func unlock2 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Other M's are waiting for the lock.
// Dequeue an M.
// Dequeued an M.  Wake it.
// restore the preemption request in case we've cleared it in newstack

// One-time notifications.
internal static void noteclear(ж<note> Ꮡn) {
    ref var n = ref Ꮡn.Value;

    n.key = 0;
}

// go2cs generated this placeholder — func notewakeup is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func notesleep is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func notetsleep_internal is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Successfully set waitm to locked.
// What was it before?
// Nothing was waiting. Done.
// Two notewakeups! Not allowed.
// Must be the waiting m. Wake it up.
// Must be locked (got wakeup).
// Queued. Sleep.
// gp and deadline are logically local variables, but they are written
// as parameters so that the stack space they require is charged
// to the caller.
// This reduces the nosplit footprint of notetsleep_internal.
// Register for wakeup on n->waitm.
// Must be locked (got wakeup).
// Queued. Sleep.
// Registered. Sleep.
// Acquired semaphore, semawakeup unregistered us.
// Done.
// Interrupted or timed out. Still registered. Semaphore not acquired.
// Deadline hasn't arrived. Keep sleeping.
// Deadline arrived. Still registered. Semaphore not acquired.
// Want to give up and return, but have to unregister first,
// so that any notewakeup racing with the return does not
// try to grant us the semaphore when we don't expect it.
// No wakeup yet; unregister if possible.
// Wakeup happened so semaphore is available.
// Grab it to avoid getting out of sync.
internal static bool notetsleep(ж<note> Ꮡn, int64 ns) {
    var gp = getg();
    if (gp != (~(~gp).m).g0) {
        @throw("notetsleep not on g0"u8);
    }
    semacreate((~gp).m);
    return notetsleep_internal(Ꮡn, ns, nil, 0);
}

// same as runtime·notetsleep, but called on user g (not g0)
// calls only nosplit functions between entersyscallblock/exitsyscall.
internal static bool notetsleepg(ж<note> Ꮡn, int64 ns) {
    var gp = getg();
    if (gp == (~(~gp).m).g0) {
        @throw("notetsleepg on g0"u8);
    }
    semacreate((~gp).m);
    entersyscallblock();
    var ok = notetsleep_internal(Ꮡn, ns, nil, 0);
    exitsyscall();
    return ok;
}

internal static (ж<g>, bool) beforeIdle(int64 _Δp0, int64 _Δp1) {
    return (default!, false);
}

internal static void checkTimeouts() {
}

} // end runtime_package
