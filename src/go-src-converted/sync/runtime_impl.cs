//******************************************************************************************************
//  runtime_impl.cs - Gbtc
//
//  Copyright © 2026, J. Ritchie Carroll.  All Rights Reserved.
//
//  Licensed under the MIT License (MIT), the "License"; you may not use this file except in compliance
//  with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/11/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// Hand-written implementations of sync's //go:linkname runtime primitives. In Go these are provided by
// the runtime (sema.go / proc.go); go2cs emits them as bodyless `partial` methods, and without a body
// here the PartialStubGenerator would fill them with a throwing stub — so sync.init() (which registers a
// Pool cleanup) and every Mutex/RWMutex/WaitGroup/Once/Cond operation would crash at first use. The
// converted Mutex/RWMutex/WaitGroup/Cond state-machine logic is faithful to Go; only this runtime layer
// beneath it needs a real body.
//
// The sleeping semaphore is a faithful port of Go's runtime semaphore (sema.go), keyed by the address of
// the *uint32 — here the ж<uint32> pointer, whose Equals/GetHashCode are IDENTITY-based (same &m.sema
// slot ⇒ same bucket; distinct fields ⇒ distinct buckets). Each bucket is a counter plus a FIFO waiter
// queue; crucially, Semrelease with handoff=true hands ownership DIRECTLY to the dequeued waiter (it
// returns already-acquired, without re-competing) — the exact contract Go's Mutex/RWMutex starvation
// mode relies on, and the reason a plain SemaphoreSlim (which cannot hand off to a specific waiter)
// trips "sync: inconsistent mutex state" / "unlock of unlocked mutex" under contention. The uint32 the
// pointer addresses is never read outside these calls, so the count lives entirely in the bucket.
//
// Known Phase-4 limitations: bucket/notify-list entries persist for the process lifetime (a bounded leak
// for programs that churn many short-lived locks), and sync.Pool sharding (procPin/registerPoolCleanup)
// is a best-effort no-op (correct single-threaded; not yet a faithful per-P concurrent pool).

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Stopwatch = System.Diagnostics.Stopwatch;

// Hand-owned (no runtime_impl.go exists, so a reconvert never regenerates it); marked for consistency
// with the other hand-owned sync files.
[module: go.GoManualConversion]

namespace go;

partial class sync_package
{
    // ---- Sleeping semaphore (Mutex, RWMutex, WaitGroup) -------------------------------------------

    private sealed class SemaWaiter
    {
        internal readonly ManualResetEventSlim Signal = new(false);
        internal bool HandedOff;
    }

    private sealed class SemaBucket
    {
        internal uint Count;
        internal readonly Queue<SemaWaiter> Waiters = new();
    }

    private static readonly ConcurrentDictionary<ж<uint32>, SemaBucket> semaTable = new();

    private static SemaBucket bucketFor(ж<uint32> s) => semaTable.GetOrAdd(s, static _ => new SemaBucket());

    private static void semacquire(ж<uint32> s)
    {
        SemaBucket b = bucketFor(s);

        while (true)
        {
            SemaWaiter w;

            lock (b)
            {
                if (b.Count > 0)
                {
                    b.Count--; // acquired without parking
                    return;
                }

                w = new SemaWaiter();
                b.Waiters.Enqueue(w);
            }

            w.Signal.Wait();

            if (w.HandedOff)
                return; // ownership was handed to us directly (starvation mode)

            // Normal wake: we were merely readied — re-compete for the count.
        }
    }

    private static void semrelease(ж<uint32> s, bool handoff)
    {
        SemaBucket b = bucketFor(s);
        SemaWaiter? w = null;

        lock (b)
        {
            b.Count++;

            if (b.Waiters.Count > 0)
            {
                w = b.Waiters.Dequeue();

                if (handoff)
                {
                    b.Count--;        // hand the just-added permit directly to w
                    w.HandedOff = true;
                }
            }
        }

        w?.Signal.Set();
    }

    internal static partial void runtime_Semacquire(ж<uint32> s) => semacquire(s);

    internal static partial void runtime_SemacquireMutex(ж<uint32> s, bool lifo, nint skipframes) => semacquire(s);

    internal static partial void runtime_SemacquireRWMutex(ж<uint32> s, bool lifo, nint skipframes) => semacquire(s);

    internal static partial void runtime_SemacquireRWMutexR(ж<uint32> s, bool lifo, nint skipframes) => semacquire(s);

    internal static partial void runtime_Semrelease(ж<uint32> s, bool handoff, nint skipframes) => semrelease(s, handoff);

    // ---- Cond notify-list -------------------------------------------------------------------------
    //
    // A ticket is handed out per waiter (the old `wait`); (wait, notify) count outstanding vs. released
    // tickets so NotifyOne/NotifyAll release exactly as many permits as there are waiters. Permit
    // banking in SemaphoreSlim covers the signal-before-wait race, and Cond need not wake any particular
    // waiter, so a plain counting semaphore is faithful here (no handoff subtlety, unlike the mutex sema).

    private sealed class NotifyState
    {
        internal readonly SemaphoreSlim Sem = new(0);
        internal uint32 Wait;
        internal uint32 Notify;
    }

    private static readonly ConcurrentDictionary<ж<notifyList>, NotifyState> notifyTable = new();

    private static NotifyState notifyFor(ж<notifyList> l) => notifyTable.GetOrAdd(l, static _ => new NotifyState());

    internal static partial uint32 runtime_notifyListAdd(ж<notifyList> l)
    {
        NotifyState n = notifyFor(l);

        lock (n)
            return unchecked(n.Wait++);
    }

    internal static partial void runtime_notifyListWait(ж<notifyList> l, uint32 t) => notifyFor(l).Sem.Wait();

    internal static partial void runtime_notifyListNotifyOne(ж<notifyList> l)
    {
        NotifyState n = notifyFor(l);

        lock (n)
        {
            if (n.Wait == n.Notify)
                return; // no outstanding waiters

            n.Notify = unchecked(n.Notify + 1);
        }

        n.Sem.Release();
    }

    internal static partial void runtime_notifyListNotifyAll(ж<notifyList> l)
    {
        NotifyState n = notifyFor(l);
        int count;

        lock (n)
        {
            count = unchecked((int)(n.Wait - n.Notify));
            n.Notify = n.Wait;
        }

        if (count > 0)
            n.Sem.Release(count);
    }

    // Size-agreement sanity check between sync.notifyList and runtime's — irrelevant here.
    internal static partial void runtime_notifyListCheck(uintptr size) { }

    // (runtime.throw / runtime.fatal are defined natively in mutex.cs — used by the still-converted
    // rwmutex/cond as well as the native types.)

    // ---- Spin / timing ----------------------------------------------------------------------------

    internal static partial bool runtime_canSpin(nint i) => false;

    internal static partial void runtime_doSpin() => Thread.SpinWait(30);

    private static readonly long nanotimeBase = Stopwatch.GetTimestamp();

    internal static partial int64 runtime_nanotime() =>
        unchecked((long)((Stopwatch.GetTimestamp() - nanotimeBase) * (1_000_000_000.0 / Stopwatch.Frequency)));

    internal static partial uint32 runtime_randn(uint32 n) =>
        n == 0 ? 0u : unchecked((uint32)((ulong)System.Random.Shared.NextInt64() % n));

    // ---- Pool sharding (best-effort) --------------------------------------------------------------

    internal static partial void runtime_registerPoolCleanup(Action cleanup) { }

    internal static partial nint runtime_procPin() => 0;

    internal static partial void runtime_procUnpin() { }

    internal static partial uintptr runtime_LoadAcquintptr(ж<uintptr> ptr) => ptr.Value;

    internal static partial uintptr runtime_StoreReluintptr(ж<uintptr> ptr, uintptr val)
    {
        ptr.Value = val;
        return val;
    }
}
