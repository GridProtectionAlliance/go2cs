// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Garbage collector: finalizers and block profiling.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// finblock is an array of finalizers to be executed. finblocks are
// arranged in a linked list for the finalizer queue.
//
// finblock is allocated from non-GC'd memory, so any heap pointers
// must be specially handled. GC currently assumes that the finalizer
// queue does not grow during marking (but it can shrink).
[GoType] partial struct finblock {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal ж<finblock> alllink;
    internal ж<finblock> next;
    internal uint32 cnt;
    internal int32 __;
    internal array<finalizer> fin = new((_FinBlockSize - 2 * goarch.PtrSize - 2 * 4) / @unsafe.Sizeof(new finalizer(nil)));
}

internal static atomic.Uint32 fingStatus;

// finalizer goroutine status.
internal const uint32 fingUninitialized = /* iota */ 0;

internal const uint32 fingCreated = /* 1 << (iota - 1) */ 1;

internal const uint32 fingRunningFinalizer = 2;

internal const uint32 fingWait = 4;

internal const uint32 fingWake = 8;

internal static mutex finlock; // protects the following variables

internal static ж<g> fing; // goroutine that runs finalizers

internal static ж<finblock> finq;  // list of finalizers that are to be executed

internal static ж<finblock> finc;  // cache of free blocks

internal static array<byte> finptrmask;

internal static ж<finblock> allfin; // list of all blocks

// NOTE: Layout known to queuefinalizer.
[GoType] partial struct finalizer {
    internal ж<funcval> fn;    // function to call (may be a heap pointer)
    internal @unsafe.Pointer arg; // ptr to object (may be a heap pointer)
    internal uintptr nret;        // bytes of return values from fn
    internal ж<_type> fint;      // type of first argument of fn
    internal ж<ptrtype> ot;    // type of ptr to object (may be a heap pointer)
}

// Each Finalizer is 5 words, ptr ptr INT ptr ptr (INT = uintptr here)
// Each byte describes 8 words.
// Need 8 Finalizers described by 5 bytes before pattern repeats:
//	ptr ptr INT ptr ptr
//	ptr ptr INT ptr ptr
//	ptr ptr INT ptr ptr
//	ptr ptr INT ptr ptr
//	ptr ptr INT ptr ptr
//	ptr ptr INT ptr ptr
//	ptr ptr INT ptr ptr
//	ptr ptr INT ptr ptr
// aka
//
//	ptr ptr INT ptr ptr ptr ptr INT
//	ptr ptr ptr ptr INT ptr ptr ptr
//	ptr INT ptr ptr ptr ptr INT ptr
//	ptr ptr ptr INT ptr ptr ptr ptr
//	INT ptr ptr ptr ptr INT ptr ptr
//
// Assumptions about Finalizer layout checked below.
internal static array<byte> finalizer1 = new byte[]{
    (byte)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)(1 << (int)(0) | 1 << (int)(1)) | 0 << (int)(2)) | 1 << (int)(3)) | 1 << (int)(4)) | 1 << (int)(5)) | 1 << (int)(6)) | 0 << (int)(7)),
    (byte)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)(1 << (int)(0) | 1 << (int)(1)) | 1 << (int)(2)) | 1 << (int)(3)) | 0 << (int)(4)) | 1 << (int)(5)) | 1 << (int)(6)) | 1 << (int)(7)),
    (byte)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)(1 << (int)(0) | 0 << (int)(1)) | 1 << (int)(2)) | 1 << (int)(3)) | 1 << (int)(4)) | 1 << (int)(5)) | 0 << (int)(6)) | 1 << (int)(7)),
    (byte)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)(1 << (int)(0) | 1 << (int)(1)) | 1 << (int)(2)) | 0 << (int)(3)) | 1 << (int)(4)) | 1 << (int)(5)) | 1 << (int)(6)) | 1 << (int)(7)),
    (byte)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)((UntypedInt)(0 << (int)(0) | 1 << (int)(1)) | 1 << (int)(2)) | 1 << (int)(3)) | 1 << (int)(4)) | 0 << (int)(5)) | 1 << (int)(6)) | 1 << (int)(7))
}.array();

// lockRankMayQueueFinalizer records the lock ranking effects of a
// function that may call queuefinalizer.
internal static void lockRankMayQueueFinalizer() {
    lockWithRankMayAcquire(Ꮡ(finlock), getLockRank(Ꮡ(finlock)));
}

internal static void queuefinalizer(@unsafe.Pointer Δp, ж<funcval> Ꮡfn, uintptr nret, ж<_type> Ꮡfint, ж<ptrtype> Ꮡot) {
    ref var fn = ref Ꮡfn.val;
    ref var fint = ref Ꮡfint.val;
    ref var ot = ref Ꮡot.val;

    if (gcphase != _GCoff) {
        // Currently we assume that the finalizer queue won't
        // grow during marking so we don't have to rescan it
        // during mark termination. If we ever need to lift
        // this assumption, we can do it by adding the
        // necessary barriers to queuefinalizer (which it may
        // have automatically).
        @throw("queuefinalizer during GC"u8);
    }
    @lock(Ꮡ(finlock));
    if (finq == nil || (~finq).cnt == ((uint32)len((~finq).fin))) {
        if (finc == nil) {
            finc = (ж<finblock>)(uintptr)(persistentalloc(_FinBlockSize, 0, Ꮡmemstats.of(mstats.ᏑgcMiscSys)));
            finc.val.alllink = allfin;
            allfin = finc;
            if (finptrmask[0] == 0) {
                // Build pointer mask for Finalizer array in block.
                // Check assumptions made in finalizer1 array above.
                if ((@unsafe.Sizeof(new finalizer(nil)) != 5 * goarch.PtrSize || @unsafe.Offsetof(new finalizer(nil).GetType(), "fn") != 0 || @unsafe.Offsetof(new finalizer(nil).GetType(), "arg") != goarch.PtrSize || @unsafe.Offsetof(new finalizer(nil).GetType(), "nret") != 2 * goarch.PtrSize || @unsafe.Offsetof(new finalizer(nil).GetType(), "fint") != 3 * goarch.PtrSize || @unsafe.Offsetof(new finalizer(nil).GetType(), "ot") != 4 * goarch.PtrSize)) {
                    @throw("finalizer out of sync"u8);
                }
                foreach (var (i, _) in finptrmask) {
                    finptrmask[i] = finalizer1[i % len(finalizer1)];
                }
            }
        }
        var block = finc;
        finc = block.val.next;
        block.val.next = finq;
        finq = block;
    }
    var f = Ꮡ(~finq).fin.at<finalizer>((~finq).cnt);
    atomic.Xadd(Ꮡ((~finq).cnt), +1);
    // Sync with markroots
    f.val.fn = fn;
    f.val.nret = nret;
    f.val.fint = fint;
    f.val.ot = ot;
    f.val.arg = Δp;
    unlock(Ꮡ(finlock));
    fingStatus.Or(fingWake);
}

//go:nowritebarrier
internal static void iterate_finq(Action<ж<funcval>, @unsafe.Pointer, uintptr, ж<runtime._type>, ж<runtime.ptrtype>> callback) {
    for (var fb = allfin; fb != nil; fb = fb.val.alllink) {
        for (var i = ((uint32)0); i < (~fb).cnt; i++) {
            var f = Ꮡ(~fb).fin.at<finalizer>(i);
            callback((~f).fn, (~f).arg, (~f).nret, (~f).fint, (~f).ot);
        }
    }
}

internal static ж<g> wakefing() {
    {
        var ok = fingStatus.CompareAndSwap((uint32)((uint32)(fingCreated | fingWait) | fingWake), fingCreated); if (ok) {
            return fing;
        }
    }
    return default!;
}

internal static void createfing() {
    // start the finalizer goroutine exactly once
    if (fingStatus.Load() == fingUninitialized && fingStatus.CompareAndSwap(fingUninitialized, fingCreated)) {
        goǃ(runfinq);
    }
}

internal static bool finalizercommit(ж<g> Ꮡgp, @unsafe.Pointer @lock) {
    ref var gp = ref Ꮡgp.val;

    unlock((ж<mutex>)(uintptr)(@lock));
    // fingStatus should be modified after fing is put into a waiting state
    // to avoid waking fing in running state, even if it is about to be parked.
    fingStatus.Or(fingWait);
    return true;
}

// This is the goroutine that runs all of the finalizers.
internal static void runfinq() {
    @unsafe.Pointer frame = default!;
    uintptr framecap = default!;
    nint argRegs = default!;
    var gp = getg();
    @lock(Ꮡ(finlock));
    fing = gp;
    unlock(Ꮡ(finlock));
    while (ᐧ) {
        @lock(Ꮡ(finlock));
        var fb = finq;
        finq = default!;
        if (fb == nil) {
            gopark(finalizercommit, new @unsafe.Pointer(Ꮡ(finlock)), waitReasonFinalizerWait, traceBlockSystemGoroutine, 1);
            continue;
        }
        argRegs = intArgRegs;
        unlock(Ꮡ(finlock));
        if (raceenabled) {
            racefingo();
        }
        while (fb != nil) {
            for (var i = fb.val.cnt; i > 0; i--) {
                var f = Ꮡ(~fb).fin.at<finalizer>(i - 1);
                ref var regs = ref heap(new @internal.abi_package.RegArgs(), out var Ꮡregs);
                // The args may be passed in registers or on stack. Even for
                // the register case, we still need the spill slots.
                // TODO: revisit if we remove spill slots.
                //
                // Unfortunately because we can have an arbitrary
                // amount of returns and it would be complex to try and
                // figure out how many of those can get passed in registers,
                // just conservatively assume none of them do.
                var framesz = @unsafe.Sizeof(((any)default!)) + (~f).nret;
                if (framecap < framesz) {
                    // The frame does not contain pointers interesting for GC,
                    // all not yet finalized objects are stored in finq.
                    // If we do not mark it as FlagNoScan,
                    // the last finalized object is not collected.
                    frame = (uintptr)mallocgc(framesz, nil, true);
                    framecap = framesz;
                }
                if ((~f).fint == nil) {
                    @throw("missing type in runfinq"u8);
                }
                ref var r = ref heap<@unsafe.Pointer>(out var Ꮡr);
                r = frame;
                if (argRegs > 0){
                    r = new @unsafe.Pointer(Ꮡregs.of(abi.RegArgs.ᏑInts));
                } else {
                    // frame is effectively uninitialized
                    // memory. That means we have to clear
                    // it before writing to it to avoid
                    // confusing the write barrier.
                    ((ж<array<uintptr>>)(uintptr)(frame)).val = new uintptr[]{}.array();
                }
                var exprᴛ1 = (abiꓸKind)((~(~f).fint).Kind_ & abi.KindMask);
                if (exprᴛ1 == abi.Pointer) {
                    ((ж<@unsafe.Pointer>)(uintptr)(r)).val = f.val.arg;
                }
                else if (exprᴛ1 == abi.Interface) {
                    var ityp = (ж<interfacetype>)(uintptr)(new @unsafe.Pointer((~f).fint));
                    ((ж<eface>)(uintptr)(r)).val._type = Ꮡ((~(~f).ot).Type);
                    ((ж<eface>)(uintptr)(r)).val.data = f.val.arg;
                    if (len((~ityp).Methods) != 0) {
                        // direct use of pointer
                        // set up with empty interface
                        // convert to interface with methods
                        // this conversion is guaranteed to succeed - we checked in SetFinalizer
                        ((ж<iface>)(uintptr)(r)).val.tab = assertE2I(ityp, ((ж<eface>)(uintptr)(r)).val._type);
                    }
                }
                else { /* default: */
                    @throw("bad kind in runfinq"u8);
                }

                fingStatus.Or(fingRunningFinalizer);
                reflectcall(nil, new @unsafe.Pointer((~f).fn), frame, ((uint32)framesz), ((uint32)framesz), ((uint32)framesz), Ꮡregs);
                fingStatus.And(~fingRunningFinalizer);
                // Drop finalizer queue heap references
                // before hiding them from markroot.
                // This also ensures these will be
                // clear if we reuse the finalizer.
                f.val.fn = default!;
                f.val.arg = default!;
                f.val.ot = default!;
                atomic.Store(Ꮡ((~fb).cnt), i - 1);
            }
            var next = fb.val.next;
            @lock(Ꮡ(finlock));
            fb.val.next = finc;
            finc = fb;
            unlock(Ꮡ(finlock));
            fb = next;
        }
    }
}

internal static bool isGoPointerWithoutSpan(@unsafe.Pointer Δp) {
    // 0-length objects are okay.
    if (p.val == ((@unsafe.Pointer)(Ꮡ(zerobase)))) {
        return true;
    }
    // Global initializers might be linker-allocated.
    //	var Foo = &Object{}
    //	func main() {
    //		runtime.SetFinalizer(Foo, nil)
    //	}
    // The relevant segments are: noptrdata, data, bss, noptrbss.
    // We cannot assume they are in any order or even contiguous,
    // due to external linking.
    for (var datap = Ꮡ(firstmoduledata); datap != nil; datap = datap.val.next) {
        if ((~datap).noptrdata <= ((uintptr)Δp) && ((uintptr)Δp) < (~datap).enoptrdata || (~datap).data <= ((uintptr)Δp) && ((uintptr)Δp) < (~datap).edata || (~datap).bss <= ((uintptr)Δp) && ((uintptr)Δp) < (~datap).ebss || (~datap).noptrbss <= ((uintptr)Δp) && ((uintptr)Δp) < (~datap).enoptrbss) {
            return true;
        }
    }
    return false;
}

// blockUntilEmptyFinalizerQueue blocks until either the finalizer
// queue is emptied (and the finalizers have executed) or the timeout
// is reached. Returns true if the finalizer queue was emptied.
// This is used by the runtime and sync tests.
internal static bool blockUntilEmptyFinalizerQueue(int64 timeout) {
    var start = nanotime();
    while (nanotime() - start < timeout) {
        @lock(Ꮡ(finlock));
        // We know the queue has been drained when both finq is nil
        // and the finalizer g has stopped executing.
        var empty = finq == nil;
        empty = empty && readgstatus(fing) == _Gwaiting && (~fing).waitreason == waitReasonFinalizerWait;
        unlock(Ꮡ(finlock));
        if (empty) {
            return true;
        }
        Gosched();
    }
    return false;
}

// SetFinalizer sets the finalizer associated with obj to the provided
// finalizer function. When the garbage collector finds an unreachable block
// with an associated finalizer, it clears the association and runs
// finalizer(obj) in a separate goroutine. This makes obj reachable again,
// but now without an associated finalizer. Assuming that SetFinalizer
// is not called again, the next time the garbage collector sees
// that obj is unreachable, it will free obj.
//
// SetFinalizer(obj, nil) clears any finalizer associated with obj.
//
// The argument obj must be a pointer to an object allocated by calling
// new, by taking the address of a composite literal, or by taking the
// address of a local variable.
// The argument finalizer must be a function that takes a single argument
// to which obj's type can be assigned, and can have arbitrary ignored return
// values. If either of these is not true, SetFinalizer may abort the
// program.
//
// Finalizers are run in dependency order: if A points at B, both have
// finalizers, and they are otherwise unreachable, only the finalizer
// for A runs; once A is freed, the finalizer for B can run.
// If a cyclic structure includes a block with a finalizer, that
// cycle is not guaranteed to be garbage collected and the finalizer
// is not guaranteed to run, because there is no ordering that
// respects the dependencies.
//
// The finalizer is scheduled to run at some arbitrary time after the
// program can no longer reach the object to which obj points.
// There is no guarantee that finalizers will run before a program exits,
// so typically they are useful only for releasing non-memory resources
// associated with an object during a long-running program.
// For example, an [os.File] object could use a finalizer to close the
// associated operating system file descriptor when a program discards
// an os.File without calling Close, but it would be a mistake
// to depend on a finalizer to flush an in-memory I/O buffer such as a
// [bufio.Writer], because the buffer would not be flushed at program exit.
//
// It is not guaranteed that a finalizer will run if the size of *obj is
// zero bytes, because it may share same address with other zero-size
// objects in memory. See https://go.dev/ref/spec#Size_and_alignment_guarantees.
//
// It is not guaranteed that a finalizer will run for objects allocated
// in initializers for package-level variables. Such objects may be
// linker-allocated, not heap-allocated.
//
// Note that because finalizers may execute arbitrarily far into the future
// after an object is no longer referenced, the runtime is allowed to perform
// a space-saving optimization that batches objects together in a single
// allocation slot. The finalizer for an unreferenced object in such an
// allocation may never run if it always exists in the same batch as a
// referenced object. Typically, this batching only happens for tiny
// (on the order of 16 bytes or less) and pointer-free objects.
//
// A finalizer may run as soon as an object becomes unreachable.
// In order to use finalizers correctly, the program must ensure that
// the object is reachable until it is no longer required.
// Objects stored in global variables, or that can be found by tracing
// pointers from a global variable, are reachable. A function argument or
// receiver may become unreachable at the last point where the function
// mentions it. To make an unreachable object reachable, pass the object
// to a call of the [KeepAlive] function to mark the last point in the
// function where the object must be reachable.
//
// For example, if p points to a struct, such as os.File, that contains
// a file descriptor d, and p has a finalizer that closes that file
// descriptor, and if the last use of p in a function is a call to
// syscall.Write(p.d, buf, size), then p may be unreachable as soon as
// the program enters [syscall.Write]. The finalizer may run at that moment,
// closing p.d, causing syscall.Write to fail because it is writing to
// a closed file descriptor (or, worse, to an entirely different
// file descriptor opened by a different goroutine). To avoid this problem,
// call KeepAlive(p) after the call to syscall.Write.
//
// A single goroutine runs all finalizers for a program, sequentially.
// If a finalizer must run for a long time, it should do so by starting
// a new goroutine.
//
// In the terminology of the Go memory model, a call
// SetFinalizer(x, f) “synchronizes before” the finalization call f(x).
// However, there is no guarantee that KeepAlive(x) or any other use of x
// “synchronizes before” f(x), so in general a finalizer should use a mutex
// or other synchronization mechanism if it needs to access mutable state in x.
// For example, consider a finalizer that inspects a mutable field in x
// that is modified from time to time in the main program before x
// becomes unreachable and the finalizer is invoked.
// The modifications in the main program and the inspection in the finalizer
// need to use appropriate synchronization, such as mutexes or atomic updates,
// to avoid read-write races.
public static void SetFinalizer(any obj, any finalizer) {
    if (debug.sbrk != 0) {
        // debug.sbrk never frees memory, so no finalizers run
        // (and we don't have the data structures to record them).
        return;
    }
    var e = efaceOf(Ꮡ(obj));
    var etyp = e.val._type;
    if (etyp == nil) {
        @throw("runtime.SetFinalizer: first argument is nil"u8);
    }
    if ((abiꓸKind)((~etyp).Kind_ & abi.KindMask) != abi.Pointer) {
        @throw("runtime.SetFinalizer: first argument is "u8 + toRType(etyp).@string() + ", not pointer"u8);
    }
    var ot = (ж<ptrtype>)(uintptr)(new @unsafe.Pointer(etyp));
    if ((~ot).Elem == nil) {
        @throw("nil elem type!"u8);
    }
    if (inUserArenaChunk(((uintptr)(~e).data))) {
        // Arena-allocated objects are not eligible for finalizers.
        @throw("runtime.SetFinalizer: first argument was allocated into an arena"u8);
    }
    // find the containing object
    var (@base, span, _) = findObject(((uintptr)(~e).data), 0, 0);
    if (@base == 0) {
        if (isGoPointerWithoutSpan((~e).data)) {
            return;
        }
        @throw("runtime.SetFinalizer: pointer not in allocated block"u8);
    }
    // Move base forward if we've got an allocation header.
    if (!(~span).spanclass.noscan() && !heapBitsInSpan((~span).elemsize) && (~span).spanclass.sizeclass() != 0) {
        @base += mallocHeaderSize;
    }
    if (((uintptr)(~e).data) != @base) {
        // As an implementation detail we allow to set finalizers for an inner byte
        // of an object if it could come from tiny alloc (see mallocgc for details).
        if ((~ot).Elem == nil || (~ot).Elem.Pointers() || (~(~ot).Elem).Size_ >= maxTinySize) {
            @throw("runtime.SetFinalizer: pointer not at beginning of allocated block"u8);
        }
    }
    var f = efaceOf(Ꮡ(finalizer));
    var ftyp = f.val._type;
    if (ftyp == nil) {
        // switch to system stack and remove finalizer
        systemstack(
        var eʗ2 = e;
        () => {
            removefinalizer((~eʗ2).data);
        });
        return;
    }
    if ((abiꓸKind)((~ftyp).Kind_ & abi.KindMask) != abi.Func) {
        @throw("runtime.SetFinalizer: second argument is "u8 + toRType(ftyp).@string() + ", not a function"u8);
    }
    var ft = (ж<functype>)(uintptr)(new @unsafe.Pointer(ftyp));
    if (ft.IsVariadic()) {
        @throw("runtime.SetFinalizer: cannot pass "u8 + toRType(etyp).@string() + " to finalizer "u8 + toRType(ftyp).@string() + " because dotdotdot"u8);
    }
    if ((~ft).InCount != 1) {
        @throw("runtime.SetFinalizer: cannot pass "u8 + toRType(etyp).@string() + " to finalizer "u8 + toRType(ftyp).@string());
    }
    var fint = ft.InSlice()[0];
    switch (ᐧ) {
    case {} when fint is etyp: {
        goto okarg;
        break;
    }
    case {} when (abiꓸKind)((~fint).Kind_ & abi.KindMask) == abi.Pointer: {
        if ((fint.Uncommon() == nil || etyp.Uncommon() == nil) && ((ж<ptrtype>)(uintptr)(new @unsafe.Pointer(fint))).val.Elem == (~ot).Elem) {
            // ok - same type
            // ok - not same type, but both pointers,
            // one or the other is unnamed, and same element type, so assignable.
            goto okarg;
        }
        break;
    }
    case {} when (abiꓸKind)((~fint).Kind_ & abi.KindMask) == abi.Interface: {
        var ityp = (ж<interfacetype>)(uintptr)(new @unsafe.Pointer(fint));
        if (len((~ityp).Methods) == 0) {
            // ok - satisfies empty interface
            goto okarg;
        }
        {
            var itab = assertE2I2(ityp, (~efaceOf(Ꮡ(obj)))._type); if (itab != nil) {
                goto okarg;
            }
        }
        break;
    }}

    @throw("runtime.SetFinalizer: cannot pass "u8 + toRType(etyp).@string() + " to finalizer "u8 + toRType(ftyp).@string());
okarg:
    var nret = ((uintptr)0);
    // compute size needed for return parameters
    foreach (var (_, t) in ft.OutSlice()) {
        nret = alignUp(nret, ((uintptr)(~t).Align_)) + (~t).Size_;
    }
    nret = alignUp(nret, goarch.PtrSize);
    // make sure we have a finalizer goroutine
    createfing();
    systemstack(
    var eʗ5 = e;
    var fʗ2 = f;
    var fintʗ2 = fint;
    var otʗ2 = ot;
    () => {
        if (!addfinalizer((~eʗ5).data, (ж<funcval>)(uintptr)((~fʗ2).data), nret, fintʗ2, otʗ2)) {
            @throw("runtime.SetFinalizer: finalizer already set"u8);
        }
    });
}

// Mark KeepAlive as noinline so that it is easily detectable as an intrinsic.
//
//go:noinline

// KeepAlive marks its argument as currently reachable.
// This ensures that the object is not freed, and its finalizer is not run,
// before the point in the program where KeepAlive is called.
//
// A very simplified example showing where KeepAlive is required:
//
//	type File struct { d int }
//	d, err := syscall.Open("/file/path", syscall.O_RDONLY, 0)
//	// ... do something if err != nil ...
//	p := &File{d}
//	runtime.SetFinalizer(p, func(p *File) { syscall.Close(p.d) })
//	var buf [10]byte
//	n, err := syscall.Read(p.d, buf[:])
//	// Ensure p is not finalized until Read returns.
//	runtime.KeepAlive(p)
//	// No more uses of p after this point.
//
// Without the KeepAlive call, the finalizer could run at the start of
// [syscall.Read], closing the file descriptor before syscall.Read makes
// the actual system call.
//
// Note: KeepAlive should only be used to prevent finalizers from
// running prematurely. In particular, when used with [unsafe.Pointer],
// the rules for valid uses of unsafe.Pointer still apply.
public static void KeepAlive(any x) {
    // Introduce a use of x that the compiler can't eliminate.
    // This makes sure x is alive on entry. We need x to be alive
    // on entry for "defer runtime.KeepAlive(x)"; see issue 21402.
    if (cgoAlwaysFalse) {
        println(x);
    }
}

} // end runtime_package
