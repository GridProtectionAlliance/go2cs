// channel.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable StaticMemberInGenericType

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using go.golib;

namespace go;

public interface IChannel : IEnumerable
{
    nint Capacity { get; }

    nint Length { get; }

    void Send(object value);

    object Receive();

    bool Sent(object value);

    bool Received(out object value);

    void Close();
}

public interface IChannel<T> : IChannel
{
    void Send(in T value);

    bool Sent(in T value);

    bool Received(out T value);
}

/// <summary>
/// Describes one registered case of a Go <c>select</c> statement — a pending send or receive on a
/// specific channel. Returned by the select-registration methods (<see cref="channel{T}.Receiving"/>,
/// <see cref="channel{T}.Sending"/> and their operator forms) and consumed by
/// <c>builtin.select(params SelectOp[])</c>.
/// </summary>
/// <remarks>
/// The descriptor is type-erased: the select algorithm operates on the channel's untyped core, and a
/// send case carries its (boxed) value captured at registration time — matching Go, which evaluates
/// every case's channel and send operands exactly once, before any case is chosen. A NIL channel
/// registers a descriptor with no core; the select algorithm never registers it as a waiter (a nil
/// channel's case is never ready — Go semantics).
/// </remarks>
public sealed class SelectOp
{
    internal readonly ChanCore? Core;
    internal readonly bool IsSend;
    internal readonly object? SendValue;

    internal SelectOp(ChanCore? core, bool isSend, object? sendValue)
    {
        Core = core;
        IsSend = isSend;
        SendValue = sendValue;
    }
}

/// <summary>
/// The single-fire authority for one blocked <c>select</c>: every waker (a plain send, a plain
/// receive, another select's commit, or a close) must claim the select by winning the
/// <see cref="Winner"/> CAS before touching any of its waiters; exactly one claim can succeed.
/// </summary>
internal sealed class SelectState
{
    /// <summary>Case ordinal of the winning op; -1 until claimed.</summary>
    internal int Winner = -1;

    /// <summary>The park shared by all of the select's waiters — released once, by the claimant.</summary>
    internal readonly SemaphoreSlim Park = new(0, 1);

    internal bool TryClaim(int opIndex)
    {
        return Interlocked.CompareExchange(ref Winner, opIndex, -1) == -1;
    }
}

/// <summary>
/// A parked channel operation (Go's <c>sudog</c> analog): either a plain blocked send/receive, or
/// one case of a blocked <c>select</c> (then <see cref="Sel"/> is non-null and all of the select's
/// waiters share its park).
/// </summary>
internal sealed class Waiter(bool isSend, SelectState? sel = null, int opIndex = -1)
{
    /// <summary>Send: the (boxed) value to deliver. Receive: the delivered value (null = zero value).</summary>
    internal object? Elem;

    /// <summary>
    /// Receive: whether the value came from a send (false = channel closed). Send: whether the value
    /// was taken by a receiver (false = channel closed — the woken sender panics).
    /// </summary>
    internal bool Ok;

    /// <summary>Park for a PLAIN waiter; a select waiter parks on <see cref="SelectState.Park"/> instead.</summary>
    internal readonly SemaphoreSlim Park = new(0, 1);

    internal readonly SelectState? Sel = sel;
    internal readonly int OpIndex = opIndex;
    internal readonly bool IsSend = isSend;

    // Intrusive queue links — owned by the channel lock of the queue the waiter is on.
    internal Waiter? Next;
    internal Waiter? Prev;
    internal WaiterQueue? Queue;

    /// <summary>
    /// Publishes the waiter's result fields and then signals its park — the publish MUST precede the
    /// release (the woken thread reads <see cref="Elem"/>/<see cref="Ok"/> immediately after its
    /// wait returns; the semaphore release/wait pair provides the happens-before edge).
    /// </summary>
    internal void Wake()
    {
        (Sel?.Park ?? Park).Release();
    }
}

/// <summary>
/// Intrusive FIFO of parked channel operations (Go's <c>waitq</c>). All access is guarded by the
/// owning channel core's lock.
/// </summary>
internal sealed class WaiterQueue
{
    private Waiter? m_head;
    private Waiter? m_tail;

    internal bool IsEmpty => m_head is null;

    internal void Enqueue(Waiter waiter)
    {
        waiter.Queue = this;
        waiter.Prev = m_tail;
        waiter.Next = null;

        if (m_tail is null)
            m_head = waiter;
        else
            m_tail.Next = waiter;

        m_tail = waiter;
    }

    /// <summary>
    /// Removes a waiter if it is still on this queue; a no-op when a waker already unlinked it
    /// (used by select's loser unregistration).
    /// </summary>
    internal void Remove(Waiter waiter)
    {
        if (waiter.Queue != this)
            return;

        if (waiter.Prev is null)
            m_head = waiter.Next;
        else
            waiter.Prev.Next = waiter.Next;

        if (waiter.Next is null)
            m_tail = waiter.Prev;
        else
            waiter.Next.Prev = waiter.Prev;

        waiter.Prev = waiter.Next = null;
        waiter.Queue = null;
    }

    /// <summary>
    /// Pops the first waiter this waker is entitled to wake. A plain waiter is always usable; a
    /// select waiter is usable only if the waker wins the claim CAS on its select — a lost claim
    /// means another case already fired, so the dead waiter is discarded (unlinked, never touched)
    /// and the scan continues. Every waker — plain send, plain receive, a select commit, AND close —
    /// routes through this claim discipline.
    /// </summary>
    internal Waiter? DequeueForWake()
    {
        while (true)
        {
            Waiter? waiter = m_head;

            if (waiter is null)
                return null;

            Remove(waiter);

            if (waiter.Sel is not null && !waiter.Sel.TryClaim(waiter.OpIndex))
                continue;

            return waiter;
        }
    }
}

/// <summary>
/// Type-erased channel core (Go's <c>hchan</c> analog) — the part of the channel state the
/// <c>select</c> algorithm operates on without knowing the element type.
/// </summary>
internal abstract class ChanCore
{
    private static long s_nextId;

    /// <summary>Monotonic creation id — the total order used to lock multiple channels in select.</summary>
    internal readonly long Id = Interlocked.Increment(ref s_nextId);

    /// <summary>The channel lock (hchan.lock). Never held across a park.</summary>
    internal readonly object SyncRoot = new();

    /// <summary>Buffer capacity; 0 = unbuffered (rendezvous).</summary>
    internal readonly int Dataqsiz;

    /// <summary>Number of buffered elements.</summary>
    internal int Qcount;

    internal bool Closed;

    internal readonly WaiterQueue Recvq = new();
    internal readonly WaiterQueue Sendq = new();

    protected ChanCore(nint size)
    {
        Dataqsiz = (int)size;
    }

    /// <summary>
    /// Attempts to commit a send under the channel lock (held by the caller): direct handoff to a
    /// parked receiver, or a buffer enqueue. Returns false if the send would block. The caller has
    /// already checked <see cref="Closed"/>.
    /// </summary>
    internal abstract bool TryCommitSendLocked(object? value);

    /// <summary>
    /// Attempts to commit a receive under the channel lock (held by the caller): a closed-and-drained
    /// channel commits the (zero, false) result, a parked sender hands off (with the buffered
    /// head-take/tail-enqueue rotation), and a non-empty buffer dequeues. Returns false if the
    /// receive would block.
    /// </summary>
    internal abstract bool TryCommitRecvLocked(out object? value, out bool ok);
}

/// <summary>
/// Typed channel core implementing Go's chansend/chanrecv/closechan over a Monitor lock, a circular
/// buffer (null when unbuffered) and intrusive parked-waiter queues.
/// </summary>
internal sealed class ChanCore<T> : ChanCore
{
    private readonly T[]? m_buf;
    private int m_sendx;
    private int m_recvx;

    internal ChanCore(nint size) : base(size)
    {
        m_buf = size > 0 ? new T[size] : null;
    }

    /// <summary>
    /// Go's chansend. Blocking mode parks until the value is delivered (panicking on wake if the
    /// channel closed while parked); non-blocking mode returns false when the send would block.
    /// Sends on a closed channel always panic, even when the channel is full.
    /// </summary>
    internal bool Send(in T value, bool block)
    {
        Monitor.Enter(SyncRoot);

        if (Closed)
        {
            Monitor.Exit(SyncRoot);
            throw new PanicException("send on closed channel");
        }

        Waiter? receiver = Recvq.DequeueForWake();

        if (receiver is not null)
        {
            // Direct handoff to a parked receiver — the rendezvous. Unlock first (Go's send()
            // unlockf), then publish value+ok and signal.
            Monitor.Exit(SyncRoot);
            receiver.Elem = value;
            receiver.Ok = true;
            receiver.Wake();
            return true;
        }

        if (Qcount < Dataqsiz)
        {
            m_buf![m_sendx] = value;

            if (++m_sendx == Dataqsiz)
                m_sendx = 0;

            Qcount++;
            Monitor.Exit(SyncRoot);
            return true;
        }

        if (!block)
        {
            Monitor.Exit(SyncRoot);
            return false;
        }

        // Park: enqueue as a sender, release the channel lock, THEN wait — the lock is never held
        // across a park. A receiver (or close) publishes into the waiter before signaling.
        Waiter parked = new(isSend: true) { Elem = value };
        Sendq.Enqueue(parked);
        Monitor.Exit(SyncRoot);
        parked.Park.Wait();

        if (!parked.Ok)
            throw new PanicException("send on closed channel");

        return true;
    }

    /// <summary>
    /// Go's chanrecv. Returns true when the receive committed (with <paramref name="ok"/> false for
    /// the closed-and-drained zero value); non-blocking mode returns false when the receive would
    /// block. A closed channel drains its buffered values first — only then yields (zero, false).
    /// </summary>
    internal bool Recv(out T value, out bool ok, bool block)
    {
        Monitor.Enter(SyncRoot);

        if (Closed && Qcount == 0)
        {
            Monitor.Exit(SyncRoot);
            value = default!;
            ok = false;
            return true;
        }

        Waiter? sender = Sendq.DequeueForWake();

        if (sender is not null)
        {
            if (Dataqsiz == 0)
            {
                // Rendezvous: take the parked sender's value directly.
                Monitor.Exit(SyncRoot);
                value = sender.Elem is null ? default! : (T)sender.Elem;
            }
            else
            {
                // Buffer full with parked senders: take the buffer HEAD, slot the parked sender's
                // value at the TAIL (Go's chanrecv rotation — FIFO order is preserved).
                value = m_buf![m_recvx];
                m_buf[m_recvx] = sender.Elem is null ? default! : (T)sender.Elem;

                if (++m_recvx == Dataqsiz)
                    m_recvx = 0;

                m_sendx = m_recvx; // buffer stays full
                Monitor.Exit(SyncRoot);
            }

            sender.Elem = null;
            sender.Ok = true;
            sender.Wake();
            ok = true;
            return true;
        }

        if (Qcount > 0)
        {
            value = m_buf![m_recvx];
            m_buf[m_recvx] = default!;

            if (++m_recvx == Dataqsiz)
                m_recvx = 0;

            Qcount--;
            Monitor.Exit(SyncRoot);
            ok = true;
            return true;
        }

        if (!block)
        {
            Monitor.Exit(SyncRoot);
            value = default!;
            ok = false;
            return false;
        }

        Waiter parked = new(isSend: false);
        Recvq.Enqueue(parked);
        Monitor.Exit(SyncRoot);
        parked.Park.Wait();
        value = parked.Elem is null ? default! : (T)parked.Elem;
        ok = parked.Ok;
        return true;
    }

    /// <summary>
    /// Go's closechan: marks the channel closed and wakes every parked waiter — receivers with
    /// (zero, false), senders with a failed delivery (they panic on their own thread). Select
    /// waiters are woken only when the claim CAS is won; a waiter whose claim is lost is never
    /// touched (its select already fired another case).
    /// </summary>
    internal void Close()
    {
        Monitor.Enter(SyncRoot);

        if (Closed)
        {
            Monitor.Exit(SyncRoot);
            throw new PanicException("close of closed channel");
        }

        Closed = true;

        List<Waiter>? wake = null;
        Waiter? waiter;

        while ((waiter = Recvq.DequeueForWake()) is not null)
            (wake ??= []).Add(waiter);

        while ((waiter = Sendq.DequeueForWake()) is not null)
            (wake ??= []).Add(waiter);

        Monitor.Exit(SyncRoot);

        if (wake is null)
            return;

        foreach (Waiter claimed in wake)
        {
            claimed.Elem = null;
            claimed.Ok = false;
            claimed.Wake();
        }
    }

    internal override bool TryCommitSendLocked(object? value)
    {
        Waiter? receiver = Recvq.DequeueForWake();

        if (receiver is not null)
        {
            receiver.Elem = value;
            receiver.Ok = true;
            receiver.Wake();
            return true;
        }

        if (Qcount < Dataqsiz)
        {
            m_buf![m_sendx] = value is null ? default! : (T)value;

            if (++m_sendx == Dataqsiz)
                m_sendx = 0;

            Qcount++;
            return true;
        }

        return false;
    }

    internal override bool TryCommitRecvLocked(out object? value, out bool ok)
    {
        if (Closed && Qcount == 0)
        {
            value = null;
            ok = false;
            return true;
        }

        Waiter? sender = Sendq.DequeueForWake();

        if (sender is not null)
        {
            if (Dataqsiz == 0)
            {
                value = sender.Elem;
            }
            else
            {
                value = m_buf![m_recvx];
                m_buf[m_recvx] = sender.Elem is null ? default! : (T)sender.Elem;

                if (++m_recvx == Dataqsiz)
                    m_recvx = 0;

                m_sendx = m_recvx;
            }

            sender.Elem = null;
            sender.Ok = true;
            sender.Wake();
            ok = true;
            return true;
        }

        if (Qcount > 0)
        {
            value = m_buf![m_recvx];
            m_buf[m_recvx] = default!;

            if (++m_recvx == Dataqsiz)
                m_recvx = 0;

            Qcount--;
            ok = true;
            return true;
        }

        value = null;
        ok = false;
        return false;
    }
}

/// <summary>
/// Per-thread STACK of pending receive commits carrying each select's committed RECEIVE result
/// from <c>builtin.select</c>/<c>trySelect</c> to the winning case's guard
/// (<c>case N when ch.ꟷᐳ(out v):</c>), which pops its own frame. A stack — not a single slot —
/// because the guard's out-argument TARGET expression is evaluated BEFORE the guard call, and
/// legal Go can run another select there (<c>case a[f()] = &lt;-ch:</c> where <c>f()</c> selects):
/// the inner select pushes and pops its own frame, so the outer select's committed value survives.
/// ONLY receive commits push frames (a send-case win pushes nothing — and must not touch the
/// stack, or it would destroy an outer frame mid-nest); the winning guard pops exactly the frame
/// whose channel core matches its own, so frames cannot be delivered to the wrong channel.
/// </summary>
/// <remarks>
/// Known bounded residual (accepted, per the channels-redesign verification round): a panic or
/// exception unwinding between a select's commit (frame pushed) and the winning guard's consume
/// (frame popped) strands the frame on this thread's stack. A stranded frame is inert — later
/// guards match by channel core against the TOP frame only, and every well-formed select pushes
/// its winner's frame above any strand — so the failure is a bounded per-thread leak, not value
/// corruption; the same unwind loses the committed value in Go terms (the panic abandoned the
/// communication's result). Debug builds emit a depth-growth warning (never a process-killing
/// assert) when the stack keeps growing, which is the observable signature of stranding.
/// </remarks>
internal static class SelectPending
{
    private struct Frame
    {
        internal ChanCore Core;
        internal object? Value;
        internal bool Ok;
    }

    private const int DepthWarnThreshold = 8;

    [ThreadStatic] private static Stack<Frame>? t_frames;

    /// <summary>Pushes a committed receive (called by the select runtime, exactly once per recv win).</summary>
    internal static void Push(ChanCore core, object? value, bool ok)
    {
        Stack<Frame> frames = t_frames ??= new Stack<Frame>();

        frames.Push(new Frame { Core = core, Value = value, Ok = ok });

        // Debug-only growth warning (power-of-two gate to avoid spam): legitimate nesting depth is
        // the number of selects currently mid-guard on this thread — reaching 8+ almost certainly
        // means frames are being stranded by exceptions between commit and consume (see remarks).
        if (frames.Count >= DepthWarnThreshold && (frames.Count & (frames.Count - 1)) == 0)
            Debug.WriteLine($"go2cs WARNING: select pending-frame depth reached {frames.Count} on thread {Environment.CurrentManagedThreadId} — frames may have been stranded by exceptions unwinding between a select commit and its winning guard");
    }

    /// <summary>
    /// Pops the pending receive for the winning guard's channel: consumes the TOP frame iff its
    /// core matches <paramref name="core"/> — an inner select's frames are balanced (pushed and
    /// popped within the guard's target-expression evaluation), so the matching frame is always on
    /// top when the winning guard runs. A mismatched top is a select-discipline violation
    /// (debug-asserted); in release the guard falls back to its non-blocking probe.
    /// </summary>
    internal static bool TryConsume(ChanCore? core, out object? value, out bool ok)
    {
        Stack<Frame>? frames = t_frames;

        if (core is null || frames is null || frames.Count == 0)
        {
            value = null;
            ok = false;
            return false;
        }

        Frame top = frames.Peek();

        if (!ReferenceEquals(top.Core, core))
        {
            Debug.Assert(false, "select pending frame does not match the winning guard's channel");
            value = null;
            ok = false;
            return false;
        }

        frames.Pop();
        value = top.Value;
        ok = top.Ok;
        return true;
    }
}

/// <summary>
/// The Go selectgo algorithm over <see cref="SelectOp"/> descriptors: nil channels are partitioned
/// out, all distinct cores are locked in Id order, a uniformly-random poll pass commits exactly one
/// ready op under the held locks, and otherwise one SelectState-linked waiter per case is parked
/// until a waker claims exactly one of them.
/// </summary>
internal static class SelectRuntime
{
    /// <summary>Blocking select: returns the ordinal of the single case that fired.</summary>
    internal static int Run(SelectOp[] ops)
    {
        int liveCount = CountLive(ops);

        if (liveCount == 0)
        {
            // select{} or every case on a nil channel: blocks forever — the standing
            // nil-channel deadlock-grace path (matches plain send/receive on nil).
            if (!channel.Wait(CancellationToken.None))
                fatal(FatalError.DeadLock());

            return -1; // unreachable
        }

        ChanCore[] lockOrder = BuildLockOrder(ops);
        int[] pollOrder = BuildPollOrder(ops, liveCount);

        LockAll(lockOrder);

        int committed = PollPassLocked(ops, pollOrder, lockOrder);

        if (committed >= 0)
            return committed;

        // Nothing ready — park one waiter per live case, all sharing one SelectState.
        SelectState sel = new();
        Waiter?[] waiters = new Waiter?[ops.Length];

        for (int i = 0; i < ops.Length; i++)
        {
            SelectOp op = ops[i];

            if (op.Core is null)
                continue;

            Waiter waiter = new(op.IsSend, sel, i);

            if (op.IsSend)
            {
                waiter.Elem = op.SendValue;
                op.Core.Sendq.Enqueue(waiter);
            }
            else
            {
                op.Core.Recvq.Enqueue(waiter);
            }

            waiters[i] = waiter;
        }

        UnlockAll(lockOrder);

        // Park = unlock THEN wait. A waker that claimed us between the unlock and this wait has
        // already released the semaphore, so the wait returns immediately — no lost wakeup.
        sel.Park.Wait();

        int winner = Volatile.Read(ref sel.Winner);

        // Re-lock and unregister the losing waiters. A waiter a waker already unlinked (the winner,
        // or a loser discarded on a failed claim) is a Remove no-op.
        LockAll(lockOrder);

        for (int i = 0; i < waiters.Length; i++)
        {
            Waiter? waiter = waiters[i];

            if (waiter is not null && i != winner)
                waiter.Queue?.Remove(waiter);
        }

        UnlockAll(lockOrder);

        Waiter won = waiters[winner]!;

        if (won.IsSend)
        {
            // A send-case win pushes no pending frame — and must not touch the stack: an OUTER
            // select's committed frame may be in flight when this select runs nested inside the
            // outer guard's target expression.
            if (!won.Ok)
                throw new PanicException("send on closed channel");
        }
        else
        {
            SelectPending.Push(ops[winner].Core!, won.Elem, won.Ok);
        }

        return winner;
    }

    /// <summary>
    /// Non-blocking select (the <c>default:</c> form): the same poll pass as <see cref="Run"/> —
    /// uniformly-random single commit under the held locks — but never parks; returns -1 (the
    /// default sentinel) when no case is ready.
    /// </summary>
    internal static int TryRun(SelectOp[] ops)
    {
        int liveCount = CountLive(ops);

        if (liveCount == 0)
            return -1;

        ChanCore[] lockOrder = BuildLockOrder(ops);
        int[] pollOrder = BuildPollOrder(ops, liveCount);

        LockAll(lockOrder);

        int committed = PollPassLocked(ops, pollOrder, lockOrder);

        if (committed >= 0)
            return committed;

        UnlockAll(lockOrder);
        return -1;
    }

    // Commits exactly one ready op scanning in pollOrder; returns its ordinal (having released all
    // locks) or -1 with the locks still held. A send case on a closed channel panics — Go panics
    // there even when the select has a default clause.
    private static int PollPassLocked(SelectOp[] ops, int[] pollOrder, ChanCore[] lockOrder)
    {
        foreach (int i in pollOrder)
        {
            SelectOp op = ops[i];
            ChanCore core = op.Core!;

            if (op.IsSend)
            {
                if (core.Closed)
                {
                    UnlockAll(lockOrder);
                    throw new PanicException("send on closed channel");
                }

                if (core.TryCommitSendLocked(op.SendValue))
                {
                    UnlockAll(lockOrder);
                    return i; // a send-case win pushes no pending frame (and must not touch the stack)
                }
            }
            else
            {
                if (core.TryCommitRecvLocked(out object? value, out bool ok))
                {
                    SelectPending.Push(core, value, ok);
                    UnlockAll(lockOrder);
                    return i;
                }
            }
        }

        return -1;
    }

    private static int CountLive(SelectOp[] ops)
    {
        int liveCount = 0;

        foreach (SelectOp op in ops)
        {
            if (op.Core is not null)
                liveCount++;
        }

        return liveCount;
    }

    // Distinct cores in Id order — the total lock order. A select may list the same channel more
    // than once (send and receive cases on one channel), so cores are deduplicated.
    private static ChanCore[] BuildLockOrder(SelectOp[] ops)
    {
        List<ChanCore> cores = new(ops.Length);

        foreach (SelectOp op in ops)
        {
            if (op.Core is not null && !cores.Contains(op.Core))
                cores.Add(op.Core);
        }

        ChanCore[] lockOrder = cores.ToArray();
        Array.Sort(lockOrder, static (x, y) => x.Id.CompareTo(y.Id));
        return lockOrder;
    }

    // Fisher-Yates shuffle of the live case ordinals (Random.Shared is backed by a per-thread
    // generator) — Go picks uniformly at random among ready cases.
    private static int[] BuildPollOrder(SelectOp[] ops, int liveCount)
    {
        int[] pollOrder = new int[liveCount];
        int next = 0;

        for (int i = 0; i < ops.Length; i++)
        {
            if (ops[i].Core is not null)
                pollOrder[next++] = i;
        }

        for (int i = liveCount - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);
            (pollOrder[i], pollOrder[j]) = (pollOrder[j], pollOrder[i]);
        }

        return pollOrder;
    }

    private static void LockAll(ChanCore[] lockOrder)
    {
        foreach (ChanCore core in lockOrder)
            Monitor.Enter(core.SyncRoot);
    }

    private static void UnlockAll(ChanCore[] lockOrder)
    {
        for (int i = lockOrder.Length - 1; i >= 0; i--)
            Monitor.Exit(lockOrder[i].SyncRoot);
    }
}

/// <summary>
/// Represents a concurrency primitive that operates like a Go channel.
/// </summary>
/// <typeparam name="T">Target type for channel.</typeparam>
public struct channel<T> : IChannel<T>, IEnumerable<T>, ISupportMake<channel<T>>
{
    // The entire channel state lives in the heap core so struct copies share one channel (Go
    // channel values are references) and the zero value (all-null) is the NIL channel.
    private readonly ChanCore<T>? m_core;

    /// <summary>
    /// Creates a new channel.
    /// </summary>
    /// <param name="size">
    /// Buffer capacity: 0 creates an unbuffered (rendezvous) channel; a positive value creates a
    /// buffered channel of that capacity — matching Go's <c>make(chan T[, size])</c>.
    /// </param>
    public channel(nint size)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(size);
        m_core = new ChanCore<T>(size);
    }

    /// <summary>
    /// Gets the capacity of the channel (0 for an unbuffered or nil channel) — Go's <c>cap()</c>.
    /// </summary>
    public nint Capacity => m_core?.Dataqsiz ?? 0;

    /// <summary>
    /// Gets the count of buffered items in the channel — Go's <c>len()</c> (0 for an unbuffered or
    /// nil channel).
    /// </summary>
    public nint Length => m_core?.Qcount ?? 0;

    /// <summary>
    /// Gets a flag that determines if the channel is unbuffered.
    /// </summary>
    public bool IsUnbuffered => m_core is null || m_core.Dataqsiz == 0;

    /// <summary>
    /// Gets a flag that determines if the channel is closed.
    /// </summary>
    /// <remarks>
    /// A NIL channel is the struct's ZERO value, so the core is null. Go treats a nil channel as
    /// never closed and never ready — it is not an error to ASK about one (os/exec's Start polls
    /// context.Background().Done(), which IS a nil channel).
    /// </remarks>
    public bool IsClosed => m_core is not null && m_core.Closed;

    /// <summary>
    /// Gets the select-case descriptor registering a RECEIVE on this channel with
    /// <c>builtin.select</c>. A nil channel registers a never-ready descriptor.
    /// </summary>
    public SelectOp Receiving => new(m_core, isSend: false, sendValue: null);

    /// <summary>
    /// Closes the channel.
    /// </summary>
    public void Close()
    {
        if (m_core is null)
            throw new PanicException("close of nil channel");

        m_core.Close();
    }

    /// <summary>
    /// Attempt to send an item to channel.
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <returns>
    /// <c>true</c> if value was sent; otherwise,
    /// <c>false</c> as send would have blocked.
    /// </returns>
    /// <remarks>
    /// This method will not block. A send on a CLOSED channel panics (Go semantics — even when the
    /// channel is full); a NIL channel is never ready.
    /// </remarks>
    public bool TrySend(in T value)
    {
        return m_core is not null && m_core.Send(value, block: false);
    }

    /// <summary>
    /// Sends an item to channel.
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <remarks>
    /// Blocks the current thread until the value is delivered: an unbuffered channel waits for a
    /// receiver (rendezvous), a full buffered channel waits for space.
    /// </remarks>
    public void Send(in T value)
    {
        // Sending to a nil channel blocks forever
        if (m_core is null)
        {
            if (channel.Wait(CancellationToken.None))
                return;

            fatal(FatalError.DeadLock());
            return;
        }

        m_core.Send(value, block: true);
    }

    /// <summary>
    /// Sends an item to channel.
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <remarks>
    /// <para>
    /// Blocks the current thread until the value is delivered (see <see cref="Send(in T)"/>).
    /// </para>
    /// <para>
    /// Defines a Go style channel Send operation.
    /// </para>
    /// </remarks>
    public void ᐸꟷ(in T value)
    {
        Send(value);
    }

    /// <summary>
    /// Gets the select-case descriptor registering a SEND of <paramref name="value"/> on this
    /// channel with <c>builtin.select</c>.
    /// </summary>
    /// <param name="value">Value to send (captured now — Go evaluates send operands before choosing a case).</param>
    /// <returns>Send-case descriptor.</returns>
    public SelectOp Sending(in T value)
    {
        return new SelectOp(m_core, isSend: true, sendValue: value);
    }

    /// <summary>
    /// Gets the select-case descriptor registering a SEND of <paramref name="value"/> on this
    /// channel with <c>builtin.select</c>.
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <param name="_">Overload discriminator for different return type, <see cref="ꓸꓸꓸ"/>.</param>
    /// <returns>Send-case descriptor.</returns>
    /// <remarks>
    /// Defines a Go style channel <see cref="Sending"/> select registration.
    /// </remarks>
    public SelectOp ᐸꟷ(in T value, NilType _)
    {
        return Sending(value);
    }

    /// <summary>
    /// Attempts to send a value to the channel without blocking.
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <param name="_">Overload discriminator for different return type, <see cref="ꟷ"/>.</param>
    /// <returns><c>true</c> if a value was sent; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Defines a Go style channel non-blocking <see cref="Sent"/> operation — the guard emitted for
    /// a <c>select</c> send case that has a <c>default:</c> clause. Mirrors the receive side's
    /// <see cref="ꟷᐳ(out T)"/>.
    /// </remarks>
    public bool ᐸꟷ(in T value, bool _)
    {
        return Sent(value);
    }

    void IChannel.Send(object value)
    {
        Send((T)value);
    }

    /// <summary>
    /// Attempts to send a value to the channel without blocking.
    /// </summary>
    /// <param name="value">Value to send.</param>
    /// <returns><c>true</c> if a value was sent; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// <para>
    /// The send-side mirror of <see cref="Received(out T)"/>: this both PROBES readiness and, when
    /// ready, performs the send. A <c>select</c> send case that has a <c>default:</c> clause lowers
    /// to a guarded <c>case ᐧ when ch.Sent(value):</c> label, and the guard is the only place the
    /// operation can happen — a guard that merely asked whether the channel was ready would take
    /// the case and never deliver the value.
    /// </para>
    /// <para>
    /// Deferring to <see cref="TrySend(in T)"/> keeps ONE non-blocking send implementation, so Go's
    /// rules fall out by construction: a CLOSED channel panics (a closed FULL channel is not
    /// send-ready, yet Go still panics rather than taking the <c>default:</c>), and a NIL channel
    /// is never ready, so its case is never chosen.
    /// </para>
    /// </remarks>
    public bool Sent(in T value)
    {
        return TrySend(value);
    }

    /// <summary>
    /// Attempts to remove an item from channel.
    /// </summary>
    /// <param name="value">Returned value.</param>
    /// <returns>
    /// <c>true</c> if value was received; otherwise,
    /// <c>false</c> as receive would have blocked.
    /// </returns>
    /// <remarks>
    /// This method will not block. A closed channel drains its buffered values first, then reports
    /// <c>false</c> once empty (use <see cref="Received(out T)"/> for the guard form that reports
    /// the closed zero-value receive as taken).
    /// </remarks>
    public bool TryReceive(out T value)
    {
        if (m_core is null)
        {
            value = default!;
            return false;
        }

        return m_core.Recv(out value, out bool ok, block: false) && ok;
    }

    /// <summary>
    /// Removes an item from channel.
    /// </summary>
    /// <remarks>
    /// If the channel is empty, method will block the current thread until a value is sent to the
    /// channel; a receive on a closed empty channel returns the zero value.
    /// </remarks>
    /// <returns>Value received.</returns>
    public T Receive()
    {
        // Receiving from a nil channel blocks forever
        if (m_core is null)
        {
            if (channel.Wait(CancellationToken.None))
                return default!;

            fatal(FatalError.DeadLock());
            return default!;
        }

        m_core.Recv(out T value, out _, block: true);
        return value;
    }

    /// <summary>
    /// Removes an item from channel.
    /// </summary>
    /// <param name="_">Overload discriminator for different return type, <see cref="ꟷ"/>.</param>
    /// <returns>
    /// Received value and boolean result reporting whether the communication succeeded which is
    /// <c>true</c> if the value received was delivered by a successful send operation; otherwise,
    /// <c>false</c> if a zero value was generated because the channel is closed AND drained — a
    /// closed channel yields its remaining buffered values with <c>ok == true</c> first (Go's
    /// drain-before-zero comma-ok semantics).
    /// </returns>
    /// <remarks>
    /// <para>
    /// If the channel is empty, method will block the current thread until a value is sent to the channel.
    /// </para>
    /// <para>
    /// Defines a Go style channel <see cref="channel{T}.Receive()"/> operation.
    /// </para>
    /// </remarks>
    public (T val, bool ok) Receive(bool _)
    {
        // Receiving from a nil channel blocks forever
        if (m_core is null)
        {
            if (channel.Wait(CancellationToken.None))
                return (default!, false);

            fatal(FatalError.DeadLock());
            return (default!, false);
        }

        m_core.Recv(out T value, out bool ok, block: true);
        return (value, ok);
    }

    /// <summary>
    /// Attempts to receive a value from the channel — the guard emitted for a <c>select</c>
    /// receive case.
    /// </summary>
    /// <param name="value">Output parameter for received value.</param>
    /// <returns><c>true</c> if a value was received; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// When a <c>select</c>/<c>trySelect</c> committed a receive on this channel on this thread,
    /// the pending frame is popped here (matched by channel core — an inner select nested in the
    /// guard's target expression pushes and pops its own frames, so this channel's frame is on top
    /// when the winning guard runs). Otherwise this is a non-blocking probe: an OPEN empty channel
    /// is NOT ready, while a CLOSED drained channel IS ready with the zero value (Go semantics).
    /// </remarks>
    public bool Received(out T value)
    {
        if (SelectPending.TryConsume(m_core, out object? pending, out _))
        {
            value = pending is null ? default! : (T)pending;
            return true;
        }

        if (m_core is null)
        {
            value = default!;
            return false;
        }

        return m_core.Recv(out value, out _, block: false);
    }

    /// <summary>
    /// Attempts to receive a value from the channel and boolean flag indicating if the value was
    /// delivered by a send — the guard emitted for a comma-ok <c>select</c> receive case.
    /// </summary>
    /// <param name="value">Output parameter for received value.</param>
    /// <param name="ok">
    /// <c>true</c> if the value was delivered by a send; <c>false</c> for the zero value of a
    /// closed AND drained channel (buffered values drain with <c>ok == true</c> first).
    /// </param>
    /// <returns><c>true</c> if a receive committed; otherwise, <c>false</c>.</returns>
    public bool Received(out T value, out bool ok)
    {
        if (SelectPending.TryConsume(m_core, out object? pending, out ok))
        {
            value = pending is null ? default! : (T)pending;
            return true;
        }

        if (m_core is null)
        {
            value = default!;
            ok = false;
            return false;
        }

        return m_core.Recv(out value, out ok, block: false);
    }

    /// <summary>
    /// Attempts to receive a value from the channel.
    /// </summary>
    /// <param name="value">Output parameter for received value.</param>
    /// <returns><c>true</c> if a value was received; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Defines a Go style channel Receive operation.
    /// </remarks>
    /// <seealso cref="ᐸꟷ{T}(channel{T})"/>.
    public bool ꟷᐳ(out T value)
    {
        return Received(out value);
    }

    /// <summary>
    /// Attempts to receive a value from the channel and boolean flag indicating if
    /// the value was delivered by a send.
    /// </summary>
    /// <param name="value">Output parameter for received value.</param>
    /// <param name="ok">Boolean flag indicating if the value was delivered by a send.</param>
    /// <returns><c>true</c> if a receive committed; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Defines a Go style channel Receive operation.
    /// </remarks>
    /// <seealso cref="ᐸꟷ{T}(channel{T})"/>.
    public bool ꟷᐳ(out T value, out bool ok)
    {
        return Received(out value, out ok);
    }

    object IChannel.Receive()
    {
        return Receive()!;
    }

    bool IChannel.Sent(object value)
    {
        return Sent((T)value);
    }

    bool IChannel.Received(out object value)
    {
        if (Received(out T receivedValue))
        {
            value = receivedValue!;
            return true;
        }

        value = null!;
        return false;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection — Go's <c>for range ch</c>:
    /// blocks for each value and terminates when the channel is closed and drained.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        // Ranging over a nil channel blocks forever
        if (m_core is null)
        {
            if (channel.Wait(CancellationToken.None))
                yield break;

            fatal(FatalError.DeadLock());
            yield break;
        }

        while (m_core.Recv(out T value, out bool ok, block: true) && ok)
            yield return value;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // Stylistic support for channel send operation: "ch <- 12" becomes "ch <<= 12"
    // This option is not used by default since it incurs an unnecessary assignment
    // since "ch <<= 12" is equivalent to "ch = ch << 12"
    public static channel<T> operator <<(channel<T> source, T value)
    {
        source.Send(value);
        return source;
    }

    // From Go spec: Two channel values are equal if they were created by the same call to make or if both have value nil.
    public static bool operator ==(channel<T> left, channel<T> right)
    {
        return ReferenceEquals(left.m_core, right.m_core);
    }

    public static bool operator !=(channel<T> left, channel<T> right)
    {
        return !(left == right);
    }

    // Enable comparisons between nil and channel struct
    public static bool operator ==(channel<T> value, NilType nil)
    {
        return value == default(channel<T>);
    }

    public static bool operator !=(channel<T> value, NilType nil)
    {
        return !(value == nil);
    }

    public static bool operator ==(NilType nil, channel<T> value)
    {
        return value == nil;
    }

    public static bool operator !=(NilType nil, channel<T> value)
    {
        return value != nil;
    }

    public static implicit operator channel<T>(NilType nil)
    {
        return default;
    }

    /// <inheritdoc />
    public static channel<T> Make(nint p1 = 0, nint p2 = -1)
    {
        return new channel<T>(p1);
    }
}

public static class channel
{
    public const int DeadLockDetectionTimeout = 200;

    /// <summary>
    /// Sleeps for the deadlock-grace <paramref name="timeout"/> used by operations on a nil
    /// channel, returning <c>true</c> only if <paramref name="token"/> was canceled first —
    /// <c>false</c> means the timeout elapsed (the caller reports the deadlock).
    /// </summary>
    public static bool Wait(CancellationToken token, int timeout = DeadLockDetectionTimeout)
    {
        // Plain timed wait — every nil-channel op funnels here, so no per-call throwaway
        // SemaphoreSlim. An uncancelable token (the in-tree callers all pass None) is a sleep.
        if (!token.CanBeCanceled)
        {
            Thread.Sleep(timeout);
            return false;
        }

        return token.WaitHandle.WaitOne(timeout);
    }
}
