// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:22:52 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\runtime2.go
using cpu = go.@internal.cpu_package;
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // defined constants
 
        // G status
        //
        // Beyond indicating the general state of a G, the G status
        // acts like a lock on the goroutine's stack (and hence its
        // ability to execute user code).
        //
        // If you add to this list, add to the list
        // of "okay during garbage collection" status
        // in mgcmark.go too.
        //
        // TODO(austin): The _Gscan bit could be much lighter-weight.
        // For example, we could choose not to run _Gscanrunnable
        // goroutines found in the run queue, rather than CAS-looping
        // until they become _Grunnable. And transitions like
        // _Gscanwaiting -> _Gscanrunnable are actually okay because
        // they don't affect stack ownership.

        // _Gidle means this goroutine was just allocated and has not
        // yet been initialized.
        private static readonly var _Gidle = (var)iota; // 0

        // _Grunnable means this goroutine is on a run queue. It is
        // not currently executing user code. The stack is not owned.
        private static readonly var _Grunnable = (var)0; // 1

        // _Grunning means this goroutine may execute user code. The
        // stack is owned by this goroutine. It is not on a run queue.
        // It is assigned an M and a P (g.m and g.m.p are valid).
        private static readonly var _Grunning = (var)1; // 2

        // _Gsyscall means this goroutine is executing a system call.
        // It is not executing user code. The stack is owned by this
        // goroutine. It is not on a run queue. It is assigned an M.
        private static readonly var _Gsyscall = (var)2; // 3

        // _Gwaiting means this goroutine is blocked in the runtime.
        // It is not executing user code. It is not on a run queue,
        // but should be recorded somewhere (e.g., a channel wait
        // queue) so it can be ready()d when necessary. The stack is
        // not owned *except* that a channel operation may read or
        // write parts of the stack under the appropriate channel
        // lock. Otherwise, it is not safe to access the stack after a
        // goroutine enters _Gwaiting (e.g., it may get moved).
        private static readonly var _Gwaiting = (var)3; // 4

        // _Gmoribund_unused is currently unused, but hardcoded in gdb
        // scripts.
        private static readonly var _Gmoribund_unused = (var)4; // 5

        // _Gdead means this goroutine is currently unused. It may be
        // just exited, on a free list, or just being initialized. It
        // is not executing user code. It may or may not have a stack
        // allocated. The G and its stack (if any) are owned by the M
        // that is exiting the G or that obtained the G from the free
        // list.
        private static readonly var _Gdead = (var)5; // 6

        // _Genqueue_unused is currently unused.
        private static readonly var _Genqueue_unused = (var)6; // 7

        // _Gcopystack means this goroutine's stack is being moved. It
        // is not executing user code and is not on a run queue. The
        // stack is owned by the goroutine that put it in _Gcopystack.
        private static readonly var _Gcopystack = (var)7; // 8

        // _Gpreempted means this goroutine stopped itself for a
        // suspendG preemption. It is like _Gwaiting, but nothing is
        // yet responsible for ready()ing it. Some suspendG must CAS
        // the status to _Gwaiting to take responsibility for
        // ready()ing this G.
        private static readonly _Gscan _Gpreempted = (_Gscan)0x1000UL;
        private static readonly var _Gscanrunnable = (var)_Gscan + _Grunnable; // 0x1001
        private static readonly var _Gscanrunning = (var)_Gscan + _Grunning; // 0x1002
        private static readonly var _Gscansyscall = (var)_Gscan + _Gsyscall; // 0x1003
        private static readonly var _Gscanwaiting = (var)_Gscan + _Gwaiting; // 0x1004
        private static readonly var _Gscanpreempted = (var)_Gscan + _Gpreempted; // 0x1009

 
        // P status

        // _Pidle means a P is not being used to run user code or the
        // scheduler. Typically, it's on the idle P list and available
        // to the scheduler, but it may just be transitioning between
        // other states.
        //
        // The P is owned by the idle list or by whatever is
        // transitioning its state. Its run queue is empty.
        private static readonly var _Pidle = (var)iota; 

        // _Prunning means a P is owned by an M and is being used to
        // run user code or the scheduler. Only the M that owns this P
        // is allowed to change the P's status from _Prunning. The M
        // may transition the P to _Pidle (if it has no more work to
        // do), _Psyscall (when entering a syscall), or _Pgcstop (to
        // halt for the GC). The M may also hand ownership of the P
        // off directly to another M (e.g., to schedule a locked G).
        private static readonly var _Prunning = (var)0; 

        // _Psyscall means a P is not running user code. It has
        // affinity to an M in a syscall but is not owned by it and
        // may be stolen by another M. This is similar to _Pidle but
        // uses lightweight transitions and maintains M affinity.
        //
        // Leaving _Psyscall must be done with a CAS, either to steal
        // or retake the P. Note that there's an ABA hazard: even if
        // an M successfully CASes its original P back to _Prunning
        // after a syscall, it must understand the P may have been
        // used by another M in the interim.
        private static readonly var _Psyscall = (var)1; 

        // _Pgcstop means a P is halted for STW and owned by the M
        // that stopped the world. The M that stopped the world
        // continues to use its P, even in _Pgcstop. Transitioning
        // from _Prunning to _Pgcstop causes an M to release its P and
        // park.
        //
        // The P retains its run queue and startTheWorld will restart
        // the scheduler on Ps with non-empty run queues.
        private static readonly var _Pgcstop = (var)2; 

        // _Pdead means a P is no longer used (GOMAXPROCS shrank). We
        // reuse Ps if GOMAXPROCS increases. A dead P is mostly
        // stripped of its resources, though a few things remain
        // (e.g., trace buffers).
        private static readonly var _Pdead = (var)3;


        // Mutual exclusion locks.  In the uncontended case,
        // as fast as spin locks (just a few user-level instructions),
        // but on the contention path they sleep in the kernel.
        // A zeroed Mutex is unlocked (no need to initialize each lock).
        // Initialization is helpful for static lock ranking, but not required.
        private partial struct mutex
        {
            public ref lockRankStruct lockRankStruct => ref lockRankStruct_val; // Futex-based impl treats it as uint32 key,
// while sema-based impl as M* waitm.
// Used to be a union, but unions break precise GC.
            public System.UIntPtr key;
        }

        // sleep and wakeup on one-time events.
        // before any calls to notesleep or notewakeup,
        // must call noteclear to initialize the Note.
        // then, exactly one thread can call notesleep
        // and exactly one thread can call notewakeup (once).
        // once notewakeup has been called, the notesleep
        // will return.  future notesleep will return immediately.
        // subsequent noteclear must be called only after
        // previous notesleep has returned, e.g. it's disallowed
        // to call noteclear straight after notewakeup.
        //
        // notetsleep is like notesleep but wakes up after
        // a given number of nanoseconds even if the event
        // has not yet happened.  if a goroutine uses notetsleep to
        // wake up early, it must wait to call noteclear until it
        // can be sure that no other goroutine is calling
        // notewakeup.
        //
        // notesleep/notetsleep are generally called on g0,
        // notetsleepg is similar to notetsleep but is called on user g.
        private partial struct note
        {
            public System.UIntPtr key;
        }

        private partial struct funcval
        {
            public System.UIntPtr fn; // variable-size, fn-specific data here
        }

        private partial struct iface
        {
            public ptr<itab> tab;
            public unsafe.Pointer data;
        }

        private partial struct eface
        {
            public ptr<_type> _type;
            public unsafe.Pointer data;
        }

        private static ptr<eface> efaceOf(object ep)
        {
            return _addr_(eface.val)(@unsafe.Pointer(ep))!;
        }

        // The guintptr, muintptr, and puintptr are all used to bypass write barriers.
        // It is particularly important to avoid write barriers when the current P has
        // been released, because the GC thinks the world is stopped, and an
        // unexpected write barrier would not be synchronized with the GC,
        // which can lead to a half-executed write barrier that has marked the object
        // but not queued it. If the GC skips the object and completes before the
        // queuing can occur, it will incorrectly free the object.
        //
        // We tried using special assignment functions invoked only when not
        // holding a running P, but then some updates to a particular memory
        // word went through write barriers and some did not. This breaks the
        // write barrier shadow checking mode, and it is also scary: better to have
        // a word that is completely ignored by the GC than to have one for which
        // only a few updates are ignored.
        //
        // Gs and Ps are always reachable via true pointers in the
        // allgs and allp lists or (during allocation before they reach those lists)
        // from stack variables.
        //
        // Ms are always reachable via true pointers either from allm or
        // freem. Unlike Gs and Ps we do free Ms, so it's important that
        // nothing ever hold an muintptr across a safe point.

        // A guintptr holds a goroutine pointer, but typed as a uintptr
        // to bypass write barriers. It is used in the Gobuf goroutine state
        // and in scheduling lists that are manipulated without a P.
        //
        // The Gobuf.g goroutine pointer is almost always updated by assembly code.
        // In one of the few places it is updated by Go code - func save - it must be
        // treated as a uintptr to avoid a write barrier being emitted at a bad time.
        // Instead of figuring out how to emit the write barriers missing in the
        // assembly manipulation, we change the type of the field to uintptr,
        // so that it does not require write barriers at all.
        //
        // Goroutine structs are published in the allg list and never freed.
        // That will keep the goroutine structs from being collected.
        // There is never a time that Gobuf.g's contain the only references
        // to a goroutine: the publishing of the goroutine in allg comes first.
        // Goroutine pointers are also kept in non-GC-visible places like TLS,
        // so I can't see them ever moving. If we did want to start moving data
        // in the GC, we'd need to allocate the goroutine structs from an
        // alternate arena. Using guintptr doesn't make that problem any worse.
        private partial struct guintptr // : System.UIntPtr
        {
        }

        //go:nosplit
        private static ptr<g> ptr(this guintptr gp)
        {
            return _addr_(g.val)(@unsafe.Pointer(gp))!;
        }

        //go:nosplit
        private static void set(this ptr<guintptr> _addr_gp, ptr<g> _addr_g)
        {
            ref guintptr gp = ref _addr_gp.val;
            ref g g = ref _addr_g.val;

            gp.val = guintptr(@unsafe.Pointer(g));
        }

        //go:nosplit
        private static bool cas(this ptr<guintptr> _addr_gp, guintptr old, guintptr @new)
        {
            ref guintptr gp = ref _addr_gp.val;

            return atomic.Casuintptr((uintptr.val)(@unsafe.Pointer(gp)), uintptr(old), uintptr(new));
        }

        // setGNoWB performs *gp = new without a write barrier.
        // For times when it's impractical to use a guintptr.
        //go:nosplit
        //go:nowritebarrier
        private static void setGNoWB(ptr<ptr<g>> _addr_gp, ptr<g> _addr_@new)
        {
            ref ptr<g> gp = ref _addr_gp.val;
            ref g @new = ref _addr_@new.val;

            (guintptr.val)(@unsafe.Pointer(gp)).set(new);
        }

        private partial struct puintptr // : System.UIntPtr
        {
        }

        //go:nosplit
        private static ptr<p> ptr(this puintptr pp)
        {
            return _addr_(p.val)(@unsafe.Pointer(pp))!;
        }

        //go:nosplit
        private static void set(this ptr<puintptr> _addr_pp, ptr<p> _addr_p)
        {
            ref puintptr pp = ref _addr_pp.val;
            ref p p = ref _addr_p.val;

            pp.val = puintptr(@unsafe.Pointer(p));
        }

        // muintptr is a *m that is not tracked by the garbage collector.
        //
        // Because we do free Ms, there are some additional constrains on
        // muintptrs:
        //
        // 1. Never hold an muintptr locally across a safe point.
        //
        // 2. Any muintptr in the heap must be owned by the M itself so it can
        //    ensure it is not in use when the last true *m is released.
        private partial struct muintptr // : System.UIntPtr
        {
        }

        //go:nosplit
        private static ptr<m> ptr(this muintptr mp)
        {
            return _addr_(m.val)(@unsafe.Pointer(mp))!;
        }

        //go:nosplit
        private static void set(this ptr<muintptr> _addr_mp, ptr<m> _addr_m)
        {
            ref muintptr mp = ref _addr_mp.val;
            ref m m = ref _addr_m.val;

            mp.val = muintptr(@unsafe.Pointer(m));
        }

        // setMNoWB performs *mp = new without a write barrier.
        // For times when it's impractical to use an muintptr.
        //go:nosplit
        //go:nowritebarrier
        private static void setMNoWB(ptr<ptr<m>> _addr_mp, ptr<m> _addr_@new)
        {
            ref ptr<m> mp = ref _addr_mp.val;
            ref m @new = ref _addr_@new.val;

            (muintptr.val)(@unsafe.Pointer(mp)).set(new);
        }

        private partial struct gobuf
        {
            public System.UIntPtr sp;
            public System.UIntPtr pc;
            public guintptr g;
            public unsafe.Pointer ctxt;
            public sys.Uintreg ret;
            public System.UIntPtr lr;
            public System.UIntPtr bp; // for GOEXPERIMENT=framepointer
        }

        // sudog represents a g in a wait list, such as for sending/receiving
        // on a channel.
        //
        // sudog is necessary because the g ↔ synchronization object relation
        // is many-to-many. A g can be on many wait lists, so there may be
        // many sudogs for one g; and many gs may be waiting on the same
        // synchronization object, so there may be many sudogs for one object.
        //
        // sudogs are allocated from a special pool. Use acquireSudog and
        // releaseSudog to allocate and free them.
        private partial struct sudog
        {
            public ptr<g> g;
            public ptr<sudog> next;
            public ptr<sudog> prev;
            public unsafe.Pointer elem; // data element (may point to stack)

// The following fields are never accessed concurrently.
// For channels, waitlink is only accessed by g.
// For semaphores, all fields (including the ones above)
// are only accessed when holding a semaRoot lock.

            public long acquiretime;
            public long releasetime;
            public uint ticket; // isSelect indicates g is participating in a select, so
// g.selectDone must be CAS'd to win the wake-up race.
            public bool isSelect;
            public ptr<sudog> parent; // semaRoot binary tree
            public ptr<sudog> waitlink; // g.waiting list or semaRoot
            public ptr<sudog> waittail; // semaRoot
            public ptr<hchan> c; // channel
        }

        private partial struct libcall
        {
            public System.UIntPtr fn;
            public System.UIntPtr n; // number of parameters
            public System.UIntPtr args; // parameters
            public System.UIntPtr r1; // return values
            public System.UIntPtr r2;
            public System.UIntPtr err; // error number
        }

        // describes how to handle callback
        private partial struct wincallbackcontext
        {
            public unsafe.Pointer gobody; // go function to call
            public System.UIntPtr argsize; // callback arguments size (in bytes)
            public System.UIntPtr restorestack; // adjust stack on return by (in bytes) (386 only)
            public bool cleanstack;
        }

        // Stack describes a Go execution stack.
        // The bounds of the stack are exactly [lo, hi),
        // with no implicit data structures on either side.
        private partial struct stack
        {
            public System.UIntPtr lo;
            public System.UIntPtr hi;
        }

        // heldLockInfo gives info on a held lock and the rank of that lock
        private partial struct heldLockInfo
        {
            public System.UIntPtr lockAddr;
            public lockRank rank;
        }

        private partial struct g
        {
            public stack stack; // offset known to runtime/cgo
            public System.UIntPtr stackguard0; // offset known to liblink
            public System.UIntPtr stackguard1; // offset known to liblink

            public ptr<_panic> _panic; // innermost panic - offset known to liblink
            public ptr<_defer> _defer; // innermost defer
            public ptr<m> m; // current m; offset known to arm liblink
            public gobuf sched;
            public System.UIntPtr syscallsp; // if status==Gsyscall, syscallsp = sched.sp to use during gc
            public System.UIntPtr syscallpc; // if status==Gsyscall, syscallpc = sched.pc to use during gc
            public System.UIntPtr stktopsp; // expected sp at top of stack, to check in traceback
            public unsafe.Pointer param; // passed parameter on wakeup
            public uint atomicstatus;
            public uint stackLock; // sigprof/scang lock; TODO: fold in to atomicstatus
            public long goid;
            public guintptr schedlink;
            public long waitsince; // approx time when the g become blocked
            public waitReason waitreason; // if status==Gwaiting

            public bool preempt; // preemption signal, duplicates stackguard0 = stackpreempt
            public bool preemptStop; // transition to _Gpreempted on preemption; otherwise, just deschedule
            public bool preemptShrink; // shrink stack at synchronous safe point

// asyncSafePoint is set if g is stopped at an asynchronous
// safe point. This means there are frames on the stack
// without precise pointer information.
            public bool asyncSafePoint;
            public bool paniconfault; // panic (instead of crash) on unexpected fault address
            public bool gcscandone; // g has scanned stack; protected by _Gscan bit in status
            public bool throwsplit; // must not split stack
// activeStackChans indicates that there are unlocked channels
// pointing into this goroutine's stack. If true, stack
// copying needs to acquire channel locks to protect these
// areas of the stack.
            public bool activeStackChans;
            public sbyte raceignore; // ignore race detection events
            public bool sysblocktraced; // StartTrace has emitted EvGoInSyscall about this goroutine
            public long sysexitticks; // cputicks when syscall has returned (for tracing)
            public ulong traceseq; // trace event sequencer
            public puintptr tracelastp; // last P emitted an event for this goroutine
            public muintptr lockedm;
            public uint sig;
            public slice<byte> writebuf;
            public System.UIntPtr sigcode0;
            public System.UIntPtr sigcode1;
            public System.UIntPtr sigpc;
            public System.UIntPtr gopc; // pc of go statement that created this goroutine
            public ptr<slice<ancestorInfo>> ancestors; // ancestor information goroutine(s) that created this goroutine (only used if debug.tracebackancestors)
            public System.UIntPtr startpc; // pc of goroutine function
            public System.UIntPtr racectx;
            public ptr<sudog> waiting; // sudog structures this g is waiting on (that have a valid elem ptr); in lock order
            public slice<System.UIntPtr> cgoCtxt; // cgo traceback context
            public unsafe.Pointer labels; // profiler labels
            public ptr<timer> timer; // cached timer for time.Sleep
            public uint selectDone; // are we participating in a select and did someone win the race?

// Per-G GC state

// gcAssistBytes is this G's GC assist credit in terms of
// bytes allocated. If this is positive, then the G has credit
// to allocate gcAssistBytes bytes without assisting. If this
// is negative, then the G must correct this by performing
// scan work. We track this in bytes to make it fast to update
// and check for debt in the malloc hot path. The assist ratio
// determines how this corresponds to scan work debt.
            public long gcAssistBytes;
        }

        private partial struct m
        {
            public ptr<g> g0; // goroutine with scheduling stack
            public gobuf morebuf; // gobuf arg to morestack
            public uint divmod; // div/mod denominator for arm - known to liblink

// Fields not known to debuggers.
            public ulong procid; // for debuggers, but offset not hard-coded
            public ptr<g> gsignal; // signal-handling g
            public gsignalStack goSigStack; // Go-allocated signal handling stack
            public sigset sigmask; // storage for saved signal mask
            public array<System.UIntPtr> tls; // thread-local storage (for x86 extern register)
            public Action mstartfn;
            public ptr<g> curg; // current running goroutine
            public guintptr caughtsig; // goroutine running during fatal signal
            public puintptr p; // attached p for executing go code (nil if not executing go code)
            public puintptr nextp;
            public puintptr oldp; // the p that was attached before executing a syscall
            public long id;
            public int mallocing;
            public int throwing;
            public @string preemptoff; // if != "", keep curg running on this m
            public int locks;
            public int dying;
            public int profilehz;
            public bool spinning; // m is out of work and is actively looking for work
            public bool blocked; // m is blocked on a note
            public bool newSigstack; // minit on C thread called sigaltstack
            public sbyte printlock;
            public bool incgo; // m is executing a cgo call
            public uint freeWait; // if == 0, safe to free g0 and delete m (atomic)
            public array<uint> fastrand;
            public bool needextram;
            public byte traceback;
            public ulong ncgocall; // number of cgo calls in total
            public int ncgo; // number of cgo calls currently in progress
            public uint cgoCallersUse; // if non-zero, cgoCallers in use temporarily
            public ptr<cgoCallers> cgoCallers; // cgo traceback if crashing in cgo call
            public note park;
            public ptr<m> alllink; // on allm
            public muintptr schedlink;
            public guintptr lockedg;
            public array<System.UIntPtr> createstack; // stack that created this thread.
            public uint lockedExt; // tracking for external LockOSThread
            public uint lockedInt; // tracking for internal lockOSThread
            public muintptr nextwaitm; // next m waiting for lock
            public Func<ptr<g>, unsafe.Pointer, bool> waitunlockf;
            public unsafe.Pointer waitlock;
            public byte waittraceev;
            public long waittraceskip;
            public bool startingtrace;
            public uint syscalltick;
            public ptr<m> freelink; // on sched.freem

// these are here because they are too large to be on the stack
// of low-level NOSPLIT functions.
            public libcall libcall;
            public System.UIntPtr libcallpc; // for cpu profiler
            public System.UIntPtr libcallsp;
            public guintptr libcallg;
            public libcall syscall; // stores syscall parameters on windows

            public System.UIntPtr vdsoSP; // SP for traceback while in VDSO call (0 if not in call)
            public System.UIntPtr vdsoPC; // PC for traceback while in VDSO call

// preemptGen counts the number of completed preemption
// signals. This is used to detect when a preemption is
// requested, but fails. Accessed atomically.
            public uint preemptGen; // Whether this is a pending preemption signal on this M.
// Accessed atomically.
            public uint signalPending;
            public ref dlogPerM dlogPerM => ref dlogPerM_val;
            public ref mOS mOS => ref mOS_val; // Up to 10 locks held by this m, maintained by the lock ranking code.
            public long locksHeldLen;
            public array<heldLockInfo> locksHeld;
        }

        private partial struct p
        {
            public int id;
            public uint status; // one of pidle/prunning/...
            public puintptr link;
            public uint schedtick; // incremented on every scheduler call
            public uint syscalltick; // incremented on every system call
            public sysmontick sysmontick; // last tick observed by sysmon
            public muintptr m; // back-link to associated m (nil if idle)
            public ptr<mcache> mcache;
            public pageCache pcache;
            public System.UIntPtr raceprocctx;
            public array<slice<ptr<_defer>>> deferpool; // pool of available defer structs of different sizes (see panic.go)
            public array<array<ptr<_defer>>> deferpoolbuf; // Cache of goroutine ids, amortizes accesses to runtime·sched.goidgen.
            public ulong goidcache;
            public ulong goidcacheend; // Queue of runnable goroutines. Accessed without lock.
            public uint runqhead;
            public uint runqtail;
            public array<guintptr> runq; // runnext, if non-nil, is a runnable G that was ready'd by
// the current G and should be run next instead of what's in
// runq if there's time remaining in the running G's time
// slice. It will inherit the time left in the current time
// slice. If a set of goroutines is locked in a
// communicate-and-wait pattern, this schedules that set as a
// unit and eliminates the (potentially large) scheduling
// latency that otherwise arises from adding the ready'd
// goroutines to the end of the run queue.
            public guintptr runnext; // Available G's (status == Gdead)
            public slice<ptr<sudog>> sudogcache;
            public array<ptr<sudog>> sudogbuf; // Cache of mspan objects from the heap.
            public traceBufPtr tracebuf; // traceSweep indicates the sweep events should be traced.
// This is used to defer the sweep start event until a span
// has actually been swept.
            public bool traceSweep; // traceSwept and traceReclaimed track the number of bytes
// swept and reclaimed by sweeping in the current sweep loop.
            public System.UIntPtr traceSwept;
            public System.UIntPtr traceReclaimed;
            public persistentAlloc palloc; // per-P to avoid mutex

            public uint _; // Alignment for atomic fields below

// The when field of the first entry on the timer heap.
// This is updated using atomic functions.
// This is 0 if the timer heap is empty.
            public ulong timer0When; // Per-P GC state
            public long gcAssistTime; // Nanoseconds in assistAlloc
            public long gcFractionalMarkTime; // Nanoseconds in fractional mark worker (atomic)
            public guintptr gcBgMarkWorker; // (atomic)
            public gcMarkWorkerMode gcMarkWorkerMode; // gcMarkWorkerStartTime is the nanotime() at which this mark
// worker started.
            public long gcMarkWorkerStartTime; // gcw is this P's GC work buffer cache. The work buffer is
// filled by write barriers, drained by mutator assists, and
// disposed on certain GC state transitions.
            public gcWork gcw; // wbBuf is this P's GC write barrier buffer.
//
// TODO: Consider caching this in the running G.
            public wbBuf wbBuf;
            public uint runSafePointFn; // if 1, run sched.safePointFn at next safe point

// Lock for timers. We normally access the timers while running
// on this P, but the scheduler can also do it from a different P.
            public mutex timersLock; // Actions to take at some time. This is used to implement the
// standard library's time package.
// Must hold timersLock to access.
            public slice<ptr<timer>> timers; // Number of timers in P's heap.
// Modified using atomic instructions.
            public uint numTimers; // Number of timerModifiedEarlier timers on P's heap.
// This should only be modified while holding timersLock,
// or while the timer status is in a transient state
// such as timerModifying.
            public uint adjustTimers; // Number of timerDeleted timers in P's heap.
// Modified using atomic instructions.
            public uint deletedTimers; // Race context used while executing timer functions.
            public System.UIntPtr timerRaceCtx; // preempt is set to indicate that this P should be enter the
// scheduler ASAP (regardless of what G is running on it).
            public bool preempt;
            public cpu.CacheLinePad pad;
        }

        private partial struct schedt
        {
            public ulong goidgen;
            public ulong lastpoll; // time of last network poll, 0 if currently polling
            public ulong pollUntil; // time to which current poll is sleeping

            public mutex @lock; // When increasing nmidle, nmidlelocked, nmsys, or nmfreed, be
// sure to call checkdead().

            public muintptr midle; // idle m's waiting for work
            public int nmidle; // number of idle m's waiting for work
            public int nmidlelocked; // number of locked m's waiting for work
            public long mnext; // number of m's that have been created and next M ID
            public int maxmcount; // maximum number of m's allowed (or die)
            public int nmsys; // number of system m's not counted for deadlock
            public long nmfreed; // cumulative number of freed m's

            public uint ngsys; // number of system goroutines; updated atomically

            public puintptr pidle; // idle p's
            public uint npidle;
            public uint nmspinning; // See "Worker thread parking/unparking" comment in proc.go.

// Global runnable queue.
            public gQueue runq;
            public int runqsize; // disable controls selective disabling of the scheduler.
//
// Use schedEnableUser to control this.
//
// disable is protected by sched.lock.
            public mutex sudoglock;
            public ptr<sudog> sudogcache; // Central pool of available defer structs of different sizes.
            public mutex deferlock;
            public array<ptr<_defer>> deferpool; // freem is the list of m's waiting to be freed when their
// m.exited is set. Linked through m.freelink.
            public ptr<m> freem;
            public uint gcwaiting; // gc is waiting to run
            public int stopwait;
            public note stopnote;
            public uint sysmonwait;
            public note sysmonnote; // safepointFn should be called on each P at the next GC
// safepoint if p.runSafePointFn is set.
            public Action<ptr<p>> safePointFn;
            public int safePointWait;
            public note safePointNote;
            public int profilehz; // cpu profiling rate

            public long procresizetime; // nanotime() of last change to gomaxprocs
            public long totaltime; // ∫gomaxprocs dt up to procresizetime

// sysmonlock protects sysmon's actions on the runtime.
//
// Acquire and hold this mutex to block sysmon from interacting
// with the rest of the runtime.
            public mutex sysmonlock;
        }

        // Values for the flags field of a sigTabT.
        private static readonly long _SigNotify = (long)1L << (int)(iota); // let signal.Notify have signal, even if from kernel
        private static readonly var _SigKill = (var)0; // if signal.Notify doesn't take it, exit quietly
        private static readonly var _SigThrow = (var)1; // if signal.Notify doesn't take it, exit loudly
        private static readonly var _SigPanic = (var)2; // if the signal is from the kernel, panic
        private static readonly var _SigDefault = (var)3; // if the signal isn't explicitly requested, don't monitor it
        private static readonly var _SigGoExit = (var)4; // cause all runtime procs to exit (only used on Plan 9).
        private static readonly var _SigSetStack = (var)5; // add SA_ONSTACK to libc handler
        private static readonly var _SigUnblock = (var)6; // always unblock; see blockableSig
        private static readonly var _SigIgn = (var)7; // _SIG_DFL action is to ignore the signal

        // Layout of in-memory per-function information prepared by linker
        // See https://golang.org/s/go12symtab.
        // Keep in sync with linker (../cmd/link/internal/ld/pcln.go:/pclntab)
        // and with package debug/gosym and with symtab.go in package runtime.
        private partial struct _func
        {
            public System.UIntPtr entry; // start pc
            public int nameoff; // function name

            public int args; // in/out args size
            public uint deferreturn; // offset of start of a deferreturn call instruction from entry, if any.

            public int pcsp;
            public int pcfile;
            public int pcln;
            public int npcdata;
            public funcID funcID; // set for certain special runtime functions
            public array<sbyte> _; // unused
            public byte nfuncdata; // must be last
        }

        // Pseudo-Func that is returned for PCs that occur in inlined code.
        // A *Func can be either a *_func or a *funcinl, and they are distinguished
        // by the first uintptr.
        private partial struct funcinl
        {
            public System.UIntPtr zero; // set to 0 to distinguish from _func
            public System.UIntPtr entry; // entry of the real (the "outermost") frame.
            public @string name;
            public @string file;
            public long line;
        }

        // layout of Itab known to compilers
        // allocated in non-garbage-collected memory
        // Needs to be in sync with
        // ../cmd/compile/internal/gc/reflect.go:/^func.dumptabs.
        private partial struct itab
        {
            public ptr<interfacetype> inter;
            public ptr<_type> _type;
            public uint hash; // copy of _type.hash. Used for type switches.
            public array<byte> _;
            public array<System.UIntPtr> fun; // variable sized. fun[0]==0 means _type does not implement inter.
        }

        // Lock-free stack node.
        // Also known to export_test.go.
        private partial struct lfnode
        {
            public ulong next;
            public System.UIntPtr pushcnt;
        }

        private partial struct forcegcstate
        {
            public mutex @lock;
            public ptr<g> g;
            public uint idle;
        }

        // startup_random_data holds random bytes initialized at startup. These come from
        // the ELF AT_RANDOM auxiliary vector (vdso_linux_amd64.go or os_linux_386.go).
        private static slice<byte> startupRandomData = default;

        // extendRandom extends the random numbers in r[:n] to the whole slice r.
        // Treats n<0 as n==0.
        private static void extendRandom(slice<byte> r, long n)
        {
            if (n < 0L)
            {
                n = 0L;
            }

            while (n < len(r))
            { 
                // Extend random bits using hash function & time seed
                var w = n;
                if (w > 16L)
                {
                    w = 16L;
                }

                var h = memhash(@unsafe.Pointer(_addr_r[n - w]), uintptr(nanotime()), uintptr(w));
                for (long i = 0L; i < sys.PtrSize && n < len(r); i++)
                {
                    r[n] = byte(h);
                    n++;
                    h >>= 8L;
                }


            }


        }

        // A _defer holds an entry on the list of deferred calls.
        // If you add a field here, add code to clear it in freedefer and deferProcStack
        // This struct must match the code in cmd/compile/internal/gc/reflect.go:deferstruct
        // and cmd/compile/internal/gc/ssa.go:(*state).call.
        // Some defers will be allocated on the stack and some on the heap.
        // All defers are logically part of the stack, so write barriers to
        // initialize them are not required. All defers must be manually scanned,
        // and for heap defers, marked.
        private partial struct _defer
        {
            public int siz; // includes both arguments and results
            public bool started;
            public bool heap; // openDefer indicates that this _defer is for a frame with open-coded
// defers. We have only one defer record for the entire frame (which may
// currently have 0, 1, or more defers active).
            public bool openDefer;
            public System.UIntPtr sp; // sp at time of defer
            public System.UIntPtr pc; // pc at time of defer
            public ptr<funcval> fn; // can be nil for open-coded defers
            public ptr<_panic> _panic; // panic that is running defer
            public ptr<_defer> link; // If openDefer is true, the fields below record values about the stack
// frame and associated function that has the open-coded defer(s). sp
// above will be the sp for the frame, and pc will be address of the
// deferreturn call in the function.
            public unsafe.Pointer fd; // funcdata for the function associated with the frame
            public System.UIntPtr varp; // value of varp for the stack frame
// framepc is the current pc associated with the stack frame. Together,
// with sp above (which is the sp associated with the stack frame),
// framepc/sp can be used as pc/sp pair to continue a stack trace via
// gentraceback().
            public System.UIntPtr framepc;
        }

        // A _panic holds information about an active panic.
        //
        // This is marked go:notinheap because _panic values must only ever
        // live on the stack.
        //
        // The argp and link fields are stack pointers, but don't need special
        // handling during stack growth: because they are pointer-typed and
        // _panic values only live on the stack, regular stack pointer
        // adjustment takes care of them.
        //
        //go:notinheap
        private partial struct _panic
        {
            public unsafe.Pointer argp; // pointer to arguments of deferred call run during panic; cannot move - known to liblink
            public ptr<_panic> link; // link to earlier panic
            public System.UIntPtr pc; // where to return to in runtime if this panic is bypassed
            public unsafe.Pointer sp; // where to return to in runtime if this panic is bypassed
            public bool recovered; // whether this panic is over
            public bool aborted; // the panic was aborted
            public bool goexit;
        }

        // stack traces
        private partial struct stkframe
        {
            public funcInfo fn; // function being run
            public System.UIntPtr pc; // program counter within fn
            public System.UIntPtr continpc; // program counter where execution can continue, or 0 if not
            public System.UIntPtr lr; // program counter at caller aka link register
            public System.UIntPtr sp; // stack pointer at pc
            public System.UIntPtr fp; // stack pointer at caller aka frame pointer
            public System.UIntPtr varp; // top of local variables
            public System.UIntPtr argp; // pointer to function arguments
            public System.UIntPtr arglen; // number of bytes at argp
            public ptr<bitvector> argmap; // force use of this argmap
        }

        // ancestorInfo records details of where a goroutine was started.
        private partial struct ancestorInfo
        {
            public slice<System.UIntPtr> pcs; // pcs from the stack of this goroutine
            public long goid; // goroutine id of this goroutine; original goroutine possibly dead
            public System.UIntPtr gopc; // pc of go statement that created this goroutine
        }

        private static readonly long _TraceRuntimeFrames = (long)1L << (int)(iota); // include frames for internal runtime functions.
        private static readonly var _TraceTrap = (var)0; // the initial PC, SP are from a trap, not a return PC from a call
        private static readonly var _TraceJumpStack = (var)1; // if traceback is on a systemstack, resume trace at g that called into it

        // The maximum number of frames we print for a traceback
        private static readonly long _TracebackMaxFrames = (long)100L;

        // A waitReason explains why a goroutine has been stopped.
        // See gopark. Do not re-use waitReasons, add new ones.


        // A waitReason explains why a goroutine has been stopped.
        // See gopark. Do not re-use waitReasons, add new ones.
        private partial struct waitReason // : byte
        {
        }

        private static readonly waitReason waitReasonZero = (waitReason)iota; // ""
        private static readonly var waitReasonGCAssistMarking = (var)0; // "GC assist marking"
        private static readonly var waitReasonIOWait = (var)1; // "IO wait"
        private static readonly var waitReasonChanReceiveNilChan = (var)2; // "chan receive (nil chan)"
        private static readonly var waitReasonChanSendNilChan = (var)3; // "chan send (nil chan)"
        private static readonly var waitReasonDumpingHeap = (var)4; // "dumping heap"
        private static readonly var waitReasonGarbageCollection = (var)5; // "garbage collection"
        private static readonly var waitReasonGarbageCollectionScan = (var)6; // "garbage collection scan"
        private static readonly var waitReasonPanicWait = (var)7; // "panicwait"
        private static readonly var waitReasonSelect = (var)8; // "select"
        private static readonly var waitReasonSelectNoCases = (var)9; // "select (no cases)"
        private static readonly var waitReasonGCAssistWait = (var)10; // "GC assist wait"
        private static readonly var waitReasonGCSweepWait = (var)11; // "GC sweep wait"
        private static readonly var waitReasonGCScavengeWait = (var)12; // "GC scavenge wait"
        private static readonly var waitReasonChanReceive = (var)13; // "chan receive"
        private static readonly var waitReasonChanSend = (var)14; // "chan send"
        private static readonly var waitReasonFinalizerWait = (var)15; // "finalizer wait"
        private static readonly var waitReasonForceGCIdle = (var)16; // "force gc (idle)"
        private static readonly var waitReasonSemacquire = (var)17; // "semacquire"
        private static readonly var waitReasonSleep = (var)18; // "sleep"
        private static readonly var waitReasonSyncCondWait = (var)19; // "sync.Cond.Wait"
        private static readonly var waitReasonTimerGoroutineIdle = (var)20; // "timer goroutine (idle)"
        private static readonly var waitReasonTraceReaderBlocked = (var)21; // "trace reader (blocked)"
        private static readonly var waitReasonWaitForGCCycle = (var)22; // "wait for GC cycle"
        private static readonly var waitReasonGCWorkerIdle = (var)23; // "GC worker (idle)"
        private static readonly var waitReasonPreempted = (var)24; // "preempted"
        private static readonly var waitReasonDebugCall = (var)25; // "debug call"

        private static array<@string> waitReasonStrings = new array<@string>(InitKeyedValues<@string>((waitReasonZero, ""), (waitReasonGCAssistMarking, "GC assist marking"), (waitReasonIOWait, "IO wait"), (waitReasonChanReceiveNilChan, "chan receive (nil chan)"), (waitReasonChanSendNilChan, "chan send (nil chan)"), (waitReasonDumpingHeap, "dumping heap"), (waitReasonGarbageCollection, "garbage collection"), (waitReasonGarbageCollectionScan, "garbage collection scan"), (waitReasonPanicWait, "panicwait"), (waitReasonSelect, "select"), (waitReasonSelectNoCases, "select (no cases)"), (waitReasonGCAssistWait, "GC assist wait"), (waitReasonGCSweepWait, "GC sweep wait"), (waitReasonGCScavengeWait, "GC scavenge wait"), (waitReasonChanReceive, "chan receive"), (waitReasonChanSend, "chan send"), (waitReasonFinalizerWait, "finalizer wait"), (waitReasonForceGCIdle, "force gc (idle)"), (waitReasonSemacquire, "semacquire"), (waitReasonSleep, "sleep"), (waitReasonSyncCondWait, "sync.Cond.Wait"), (waitReasonTimerGoroutineIdle, "timer goroutine (idle)"), (waitReasonTraceReaderBlocked, "trace reader (blocked)"), (waitReasonWaitForGCCycle, "wait for GC cycle"), (waitReasonGCWorkerIdle, "GC worker (idle)"), (waitReasonPreempted, "preempted"), (waitReasonDebugCall, "debug call")));

        private static @string String(this waitReason w)
        {
            if (w < 0L || w >= waitReason(len(waitReasonStrings)))
            {
                return "unknown wait reason";
            }

            return waitReasonStrings[w];

        }

        private static System.UIntPtr allglen = default;        private static ptr<m> allm;        private static slice<ptr<p>> allp = default;        private static mutex allpLock = default;        private static int gomaxprocs = default;        private static int ncpu = default;        private static forcegcstate forcegc = default;        private static schedt sched = default;        private static int newprocs = default;        private static uint processorVersionInfo = default;        private static bool isIntel = default;        private static bool lfenceBeforeRdtsc = default;        private static byte goarm = default;        private static bool framepointer_enabled = default;

        // Set by the linker so the runtime can determine the buildmode.
        private static bool islibrary = default;        private static bool isarchive = default;
    }
}
