// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:49 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\runtime2.go
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

        // _Gidle means this goroutine was just allocated and has not
        // yet been initialized.
        private static readonly var _Gidle = iota; // 0

        // _Grunnable means this goroutine is on a run queue. It is
        // not currently executing user code. The stack is not owned.
        private static readonly var _Grunnable = 0; // 1

        // _Grunning means this goroutine may execute user code. The
        // stack is owned by this goroutine. It is not on a run queue.
        // It is assigned an M and a P.
        private static readonly var _Grunning = 1; // 2

        // _Gsyscall means this goroutine is executing a system call.
        // It is not executing user code. The stack is owned by this
        // goroutine. It is not on a run queue. It is assigned an M.
        private static readonly var _Gsyscall = 2; // 3

        // _Gwaiting means this goroutine is blocked in the runtime.
        // It is not executing user code. It is not on a run queue,
        // but should be recorded somewhere (e.g., a channel wait
        // queue) so it can be ready()d when necessary. The stack is
        // not owned *except* that a channel operation may read or
        // write parts of the stack under the appropriate channel
        // lock. Otherwise, it is not safe to access the stack after a
        // goroutine enters _Gwaiting (e.g., it may get moved).
        private static readonly var _Gwaiting = 3; // 4

        // _Gmoribund_unused is currently unused, but hardcoded in gdb
        // scripts.
        private static readonly var _Gmoribund_unused = 4; // 5

        // _Gdead means this goroutine is currently unused. It may be
        // just exited, on a free list, or just being initialized. It
        // is not executing user code. It may or may not have a stack
        // allocated. The G and its stack (if any) are owned by the M
        // that is exiting the G or that obtained the G from the free
        // list.
        private static readonly var _Gdead = 5; // 6

        // _Genqueue_unused is currently unused.
        private static readonly var _Genqueue_unused = 6; // 7

        // _Gcopystack means this goroutine's stack is being moved. It
        // is not executing user code and is not on a run queue. The
        // stack is owned by the goroutine that put it in _Gcopystack.
        private static readonly _Gscan _Gcopystack = 0x1000UL;
        private static readonly var _Gscanrunnable = _Gscan + _Grunnable; // 0x1001
        private static readonly var _Gscanrunning = _Gscan + _Grunning; // 0x1002
        private static readonly var _Gscansyscall = _Gscan + _Gsyscall; // 0x1003
        private static readonly var _Gscanwaiting = _Gscan + _Gwaiting; // 0x1004

 
        // P status
        private static readonly var _Pidle = iota;
        private static readonly var _Prunning = 0; // Only this P is allowed to change from _Prunning.
        private static readonly var _Psyscall = 1;
        private static readonly var _Pgcstop = 2;
        private static readonly var _Pdead = 3;

        // Mutual exclusion locks.  In the uncontended case,
        // as fast as spin locks (just a few user-level instructions),
        // but on the contention path they sleep in the kernel.
        // A zeroed Mutex is unlocked (no need to initialize each lock).
        private partial struct mutex
        {
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

        private static ref eface efaceOf(object ep)
        {
            return (eface.Value)(@unsafe.Pointer(ep));
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
        private static ref g ptr(this guintptr gp)
        {
            return (g.Value)(@unsafe.Pointer(gp));
        }

        //go:nosplit
        private static void set(this ref guintptr gp, ref g g)
        {
            gp.Value = guintptr(@unsafe.Pointer(g));

        }

        //go:nosplit
        private static bool cas(this ref guintptr gp, guintptr old, guintptr @new)
        {
            return atomic.Casuintptr((uintptr.Value)(@unsafe.Pointer(gp)), uintptr(old), uintptr(new));
        }

        // setGNoWB performs *gp = new without a write barrier.
        // For times when it's impractical to use a guintptr.
        //go:nosplit
        //go:nowritebarrier
        private static void setGNoWB(ptr<ptr<g>> gp, ref g @new)
        {
            (guintptr.Value)(@unsafe.Pointer(gp)).set(new);
        }

        private partial struct puintptr // : System.UIntPtr
        {
        }

        //go:nosplit
        private static ref p ptr(this puintptr pp)
        {
            return (p.Value)(@unsafe.Pointer(pp));
        }

        //go:nosplit
        private static void set(this ref puintptr pp, ref p p)
        {
            pp.Value = puintptr(@unsafe.Pointer(p));

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
        private static ref m ptr(this muintptr mp)
        {
            return (m.Value)(@unsafe.Pointer(mp));
        }

        //go:nosplit
        private static void set(this ref muintptr mp, ref m m)
        {
            mp.Value = muintptr(@unsafe.Pointer(m));

        }

        // setMNoWB performs *mp = new without a write barrier.
        // For times when it's impractical to use an muintptr.
        //go:nosplit
        //go:nowritebarrier
        private static void setMNoWB(ptr<ptr<m>> mp, ref m @new)
        {
            (muintptr.Value)(@unsafe.Pointer(mp)).set(new);
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
            public ptr<g> g; // isSelect indicates g is participating in a select, so
// g.selectDone must be CAS'd to win the wake-up race.
            public bool isSelect;
            public ptr<sudog> next;
            public ptr<sudog> prev;
            public unsafe.Pointer elem; // data element (may point to stack)

// The following fields are never accessed concurrently.
// For channels, waitlink is only accessed by g.
// For semaphores, all fields (including the ones above)
// are only accessed when holding a semaRoot lock.

            public long acquiretime;
            public long releasetime;
            public uint ticket;
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
            public long waitsince; // approx time when the g become blocked
            public @string waitreason; // if status==Gwaiting
            public guintptr schedlink;
            public bool preempt; // preemption signal, duplicates stackguard0 = stackpreempt
            public bool paniconfault; // panic (instead of crash) on unexpected fault address
            public bool preemptscan; // preempted g does scan for gc
            public bool gcscandone; // g has scanned stack; protected by _Gscan bit in status
            public bool gcscanvalid; // false at start of gc cycle, true if G has not run since last scan; TODO: remove?
            public bool throwsplit; // must not split stack
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
            public long id;
            public int mallocing;
            public int throwing;
            public @string preemptoff; // if != "", keep curg running on this m
            public int locks;
            public int softfloat;
            public int dying;
            public int profilehz;
            public int helpgc;
            public bool spinning; // m is out of work and is actively looking for work
            public bool blocked; // m is blocked on a note
            public bool inwb; // m is executing a write barrier
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
            public ptr<mcache> mcache;
            public guintptr lockedg;
            public array<System.UIntPtr> createstack; // stack that created this thread.
            public array<uint> freglo; // d[i] lsb and f[i]
            public array<uint> freghi; // d[i] msb and f[i+16]
            public uint fflag; // floating point compare flags
            public uint lockedExt; // tracking for external LockOSThread
            public uint lockedInt; // tracking for internal lockOSThread
            public muintptr nextwaitm; // next m waiting for lock
            public unsafe.Pointer waitunlockf; // todo go func(*g, unsafe.pointer) bool
            public unsafe.Pointer waitlock;
            public byte waittraceev;
            public long waittraceskip;
            public bool startingtrace;
            public uint syscalltick;
            public System.UIntPtr thread; // thread handle
            public ptr<m> freelink; // on sched.freem

// these are here because they are too large to be on the stack
// of low-level NOSPLIT functions.
            public libcall libcall;
            public System.UIntPtr libcallpc; // for cpu profiler
            public System.UIntPtr libcallsp;
            public guintptr libcallg;
            public libcall syscall; // stores syscall parameters on windows

            public ref mOS mOS => ref mOS_val;
        }

        private partial struct p
        {
            public mutex @lock;
            public int id;
            public uint status; // one of pidle/prunning/...
            public puintptr link;
            public uint schedtick; // incremented on every scheduler call
            public uint syscalltick; // incremented on every system call
            public sysmontick sysmontick; // last tick observed by sysmon
            public muintptr m; // back-link to associated m (nil if idle)
            public ptr<mcache> mcache;
            public System.UIntPtr racectx;
            public array<slice<ref _defer>> deferpool; // pool of available defer structs of different sizes (see panic.go)
            public array<array<ref _defer>> deferpoolbuf; // Cache of goroutine ids, amortizes accesses to runtime·sched.goidgen.
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
            public ptr<g> gfree;
            public int gfreecnt;
            public slice<ref sudog> sudogcache;
            public array<ref sudog> sudogbuf;
            public traceBufPtr tracebuf; // traceSweep indicates the sweep events should be traced.
// This is used to defer the sweep start event until a span
// has actually been swept.
            public bool traceSweep; // traceSwept and traceReclaimed track the number of bytes
// swept and reclaimed by sweeping in the current sweep loop.
            public System.UIntPtr traceSwept;
            public System.UIntPtr traceReclaimed;
            public persistentAlloc palloc; // per-P to avoid mutex

// Per-P GC state
            public long gcAssistTime; // Nanoseconds in assistAlloc
            public long gcFractionalMarkTime; // Nanoseconds in fractional mark worker
            public guintptr gcBgMarkWorker;
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

            public array<byte> pad;
        }

        private partial struct schedt
        {
            public ulong goidgen;
            public ulong lastpoll;
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
            public guintptr runqhead;
            public guintptr runqtail;
            public int runqsize; // Global cache of dead G's.
            public mutex gflock;
            public ptr<g> gfreeStack;
            public ptr<g> gfreeNoStack;
            public int ngfree; // Central cache of sudog structs.
            public mutex sudoglock;
            public ptr<sudog> sudogcache; // Central pool of available defer structs of different sizes.
            public mutex deferlock;
            public array<ref _defer> deferpool; // freem is the list of m's waiting to be freed when their
// m.exited is set. Linked through m.freelink.
            public ptr<m> freem;
            public uint gcwaiting; // gc is waiting to run
            public int stopwait;
            public note stopnote;
            public uint sysmonwait;
            public note sysmonnote; // safepointFn should be called on each P at the next GC
// safepoint if p.runSafePointFn is set.
            public Action<ref p> safePointFn;
            public int safePointWait;
            public note safePointNote;
            public int profilehz; // cpu profiling rate

            public long procresizetime; // nanotime() of last change to gomaxprocs
            public long totaltime; // ∫gomaxprocs dt up to procresizetime
        }

        // Values for the flags field of a sigTabT.
        private static readonly long _SigNotify = 1L << (int)(iota); // let signal.Notify have signal, even if from kernel
        private static readonly var _SigKill = 0; // if signal.Notify doesn't take it, exit quietly
        private static readonly var _SigThrow = 1; // if signal.Notify doesn't take it, exit loudly
        private static readonly var _SigPanic = 2; // if the signal is from the kernel, panic
        private static readonly var _SigDefault = 3; // if the signal isn't explicitly requested, don't monitor it
        private static readonly var _SigGoExit = 4; // cause all runtime procs to exit (only used on Plan 9).
        private static readonly var _SigSetStack = 5; // add SA_ONSTACK to libc handler
        private static readonly var _SigUnblock = 6; // always unblock; see blockableSig
        private static readonly var _SigIgn = 7; // _SIG_DFL action is to ignore the signal

        // Layout of in-memory per-function information prepared by linker
        // See https://golang.org/s/go12symtab.
        // Keep in sync with linker (../cmd/link/internal/ld/pcln.go:/pclntab)
        // and with package debug/gosym and with symtab.go in package runtime.
        private partial struct _func
        {
            public System.UIntPtr entry; // start pc
            public int nameoff; // function name

            public int args; // in/out args size
            public funcID funcID; // set for certain special runtime functions

            public int pcsp;
            public int pcfile;
            public int pcln;
            public int npcdata;
            public int nfuncdata;
        }

        // layout of Itab known to compilers
        // allocated in non-garbage-collected memory
        // Needs to be in sync with
        // ../cmd/compile/internal/gc/reflect.go:/^func.dumptypestructs.
        private partial struct itab
        {
            public ptr<interfacetype> inter;
            public ptr<_type> _type;
            public uint hash; // copy of _type.hash. Used for type switches.
            public array<byte> _;
            public array<System.UIntPtr> fun; // variable sized. fun[0]==0 means _type does not implement inter.
        }

        // Lock-free stack node.
        // // Also known to export_test.go.
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
                var h = memhash(@unsafe.Pointer(ref r[n - w]), uintptr(nanotime()), uintptr(w));
                for (long i = 0L; i < sys.PtrSize && n < len(r); i++)
                {
                    r[n] = byte(h);
                    n++;
                    h >>= 8L;
                }

            }

        }

        // A _defer holds an entry on the list of deferred calls.
        // If you add a field here, add code to clear it in freedefer.
        private partial struct _defer
        {
            public int siz;
            public bool started;
            public System.UIntPtr sp; // sp at time of defer
            public System.UIntPtr pc;
            public ptr<funcval> fn;
            public ptr<_panic> _panic; // panic that is running defer
            public ptr<_defer> link;
        }

        // panics
        private partial struct _panic
        {
            public unsafe.Pointer argp; // pointer to arguments of deferred call run during panic; cannot move - known to liblink
            public ptr<_panic> link; // link to earlier panic
            public bool recovered; // whether this panic is over
            public bool aborted; // the panic was aborted
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

        private static readonly long _TraceRuntimeFrames = 1L << (int)(iota); // include frames for internal runtime functions.
        private static readonly var _TraceTrap = 0; // the initial PC, SP are from a trap, not a return PC from a call
        private static readonly var _TraceJumpStack = 1; // if traceback is on a systemstack, resume trace at g that called into it

        // The maximum number of frames we print for a traceback
        private static readonly long _TracebackMaxFrames = 100L;



        private static System.UIntPtr allglen = default;        private static ref m allm = default;        private static slice<ref p> allp = default;        private static mutex allpLock = default;        private static int gomaxprocs = default;        private static int ncpu = default;        private static forcegcstate forcegc = default;        private static schedt sched = default;        private static int newprocs = default;        private static uint processorVersionInfo = default;        private static bool isIntel = default;        private static bool lfenceBeforeRdtsc = default;        private static bool support_aes = default;        private static bool support_avx = default;        private static bool support_avx2 = default;        private static bool support_bmi1 = default;        private static bool support_bmi2 = default;        private static bool support_erms = default;        private static bool support_osxsave = default;        private static bool support_popcnt = default;        private static bool support_sse2 = default;        private static bool support_sse41 = default;        private static bool support_sse42 = default;        private static bool support_ssse3 = default;        private static byte goarm = default;        private static bool framepointer_enabled = default;

        // Set by the linker so the runtime can determine the buildmode.
        private static bool islibrary = default;        private static bool isarchive = default;
    }
}
