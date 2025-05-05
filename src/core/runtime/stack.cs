// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using cpu = @internal.cpu_package;
using goarch = @internal.goarch_package;
using goos = @internal.goos_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

/*
Stack layout parameters.
Included both by runtime (compiled via 6c) and linkers (compiled via gcc).

The per-goroutine g->stackguard is set to point StackGuard bytes
above the bottom of the stack.  Each function compares its stack
pointer against g->stackguard to check for overflow.  To cut one
instruction from the check sequence for functions with tiny frames,
the stack is allowed to protrude StackSmall bytes below the stack
guard.  Functions with large frames don't bother with the check and
always call morestack.  The sequences are (for amd64, others are
similar):

	guard = g->stackguard
	frame = function's stack frame size
	argsize = size of function arguments (call + return)

	stack frame size <= StackSmall:
		CMPQ guard, SP
		JHI 3(PC)
		MOVQ m->morearg, $(argsize << 32)
		CALL morestack(SB)

	stack frame size > StackSmall but < StackBig
		LEAQ (frame-StackSmall)(SP), R0
		CMPQ guard, R0
		JHI 3(PC)
		MOVQ m->morearg, $(argsize << 32)
		CALL morestack(SB)

	stack frame size >= StackBig:
		MOVQ m->morearg, $((argsize << 32) | frame)
		CALL morestack(SB)

The bottom StackGuard - StackSmall bytes are important: there has
to be enough room to execute functions that refuse to check for
stack overflow, either because they need to be adjacent to the
actual caller's frame (deferproc) or because they handle the imminent
stack overflow (morestack).

For example, deferproc might call malloc, which does one of the
above checks (without allocating a full frame), which might trigger
a call to morestack.  This sequence needs to fit in the bottom
section of the stack.  On amd64, morestack's frame is 40 bytes, and
deferproc's frame is 56 bytes.  That fits well within the
StackGuard - StackSmall bytes at the bottom.
The linkers explore all possible call traces involving non-splitting
functions to make sure that this limit cannot be violated.
*/
internal static readonly UntypedInt stackSystem = /* goos.IsWindows*512*goarch.PtrSize + goos.IsPlan9*512 + goos.IsIos*goarch.IsArm64*1024 */ 4096;
internal static readonly UntypedInt stackMin = 2048;
internal static readonly UntypedInt fixedStack0 = /* stackMin + stackSystem */ 6144;
internal static readonly UntypedInt fixedStack1 = /* fixedStack0 - 1 */ 6143;
internal static readonly UntypedInt fixedStack2 = /* fixedStack1 | (fixedStack1 >> 1) */ 8191;
internal static readonly UntypedInt fixedStack3 = /* fixedStack2 | (fixedStack2 >> 2) */ 8191;
internal static readonly UntypedInt fixedStack4 = /* fixedStack3 | (fixedStack3 >> 4) */ 8191;
internal static readonly UntypedInt fixedStack5 = /* fixedStack4 | (fixedStack4 >> 8) */ 8191;
internal static readonly UntypedInt fixedStack6 = /* fixedStack5 | (fixedStack5 >> 16) */ 8191;
internal static readonly UntypedInt fixedStack = /* fixedStack6 + 1 */ 8192;
internal static readonly UntypedInt stackNosplit = /* abi.StackNosplitBase * sys.StackGuardMultiplier */ 800;
internal static readonly UntypedInt stackGuard = /* stackNosplit + stackSystem + abi.StackSmall */ 5024;

internal static readonly UntypedInt stackDebug = 0;
internal static readonly UntypedInt stackFromSystem = 0; // allocate stacks from system memory instead of the heap
internal static readonly UntypedInt stackFaultOnFree = 0; // old stacks are mapped noaccess to detect use after free
internal static readonly UntypedInt stackNoCache = 0; // disable per-P small stack caches
internal const bool debugCheckBP = false;

internal static nint stackPoisonCopy = 0; // fill stack that should not be accessed with garbage, to detect bad dereferences during copy

internal static readonly GoUntyped uintptrMask = /* 1<<(8*goarch.PtrSize) - 1 */
    GoUntyped.Parse("18446744073709551615");
// The values below can be stored to g.stackguard0 to force
// the next stack check to fail.
// These are all larger than any real SP.
internal static readonly GoUntyped stackPreempt = /* uintptrMask & -1314 */
    GoUntyped.Parse("18446744073709550302");
internal static readonly GoUntyped stackFork = /* uintptrMask & -1234 */
    GoUntyped.Parse("18446744073709550382");
internal static readonly GoUntyped stackForceMove = /* uintptrMask & -275 */
    GoUntyped.Parse("18446744073709551341");
internal static readonly GoUntyped stackPoisonMin = /* uintptrMask & -4096 */
    GoUntyped.Parse("18446744073709547520");

// Global pool of spans that have free stacks.
// Stacks are assigned an order according to size.
//
//	order = log_2(size/FixedStack)
//
// There is a free list for each order.

[GoType("dyn")] partial struct stackpoolᴛ1 {
    internal stackpoolItem item;
    internal array<byte> _ = new((cpu.CacheLinePadSize - @unsafe.Sizeof(new stackpoolItem(nil)) % cpu.CacheLinePadSize) % cpu.CacheLinePadSize);
}
internal static array<struct{item stackpoolItem; _ <40>byte}> stackpool;

[GoType] partial struct stackpoolItem {
    internal runtime.@internal.sys_package.NotInHeap _;
    internal mutex mu;
    internal mSpanList span;
}

// Global pool of large stack spans.

[GoType("dyn")] partial struct stackLargeᴛ1 {
    internal mutex @lock;
    internal array<mSpanList> free = new(heapAddrBits - pageShift); // free lists by log_2(s.npages)
}
internal static stackLargeᴛ1 stackLarge;

internal static void stackinit() {
    if ((UntypedInt)(_StackCacheSize & _PageMask) != 0) {
        @throw("cache size must be a multiple of page size"u8);
    }
    ref var i = ref heap(new nint(), out var Ꮡi);

    foreach (var (i, _) in stackpool) {
        stackpool[i].item.span.init();
        lockInit(Ꮡstackpool[i].item.of(stackpoolItem.Ꮡmu), lockRankStackpool);
    }
    foreach (var (i, _) in stackLarge.free) {
        stackLarge.free[i].init();
        lockInit(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock), lockRankStackLarge);
    }
}

// stacklog2 returns ⌊log_2(n)⌋.
internal static nint stacklog2(uintptr n) {
    nint log2 = 0;
    while (n > 1) {
        n >>= (UntypedInt)(1);
        log2++;
    }
    return log2;
}

// Allocates a stack from the free pool. Must be called with
// stackpool[order].item.mu held.
internal static gclinkptr stackpoolalloc(uint8 order) {
    var list = Ꮡstackpool[order].item.of(stackpoolItem.Ꮡspan);
    var s = list.val.first;
    lockWithRankMayAcquire(Ꮡmheap_.of(mheap.Ꮡlock), lockRankMheap);
    if (s == nil) {
        // no free stacks. Allocate another span worth.
        s = mheap_.allocManual(_StackCacheSize >> (int)(_PageShift), spanAllocStack);
        if (s == nil) {
            @throw("out of memory"u8);
        }
        if ((~s).allocCount != 0) {
            @throw("bad allocCount"u8);
        }
        if ((~s).manualFreeList.ptr() != nil) {
            @throw("bad manualFreeList"u8);
        }
        osStackAlloc(s);
        s.val.elemsize = fixedStack << (int)(order);
        for (var i = ((uintptr)0); i < _StackCacheSize; i += s.val.elemsize) {
            var xΔ1 = ((gclinkptr)(s.@base() + i));
            x.ptr().val.next = s.val.manualFreeList;
            s.val.manualFreeList = xΔ1;
        }
        list.insert(s);
    }
    var x = s.val.manualFreeList;
    if (x.ptr() == nil) {
        @throw("span has no free stacks"u8);
    }
    s.val.manualFreeList = x.ptr().val.next;
    (~s).allocCount++;
    if ((~s).manualFreeList.ptr() == nil) {
        // all stacks in s are allocated.
        list.remove(s);
    }
    return x;
}

// Adds stack x to the free pool. Must be called with stackpool[order].item.mu held.
internal static void stackpoolfree(gclinkptr x, uint8 order) {
    var s = spanOfUnchecked(((uintptr)x));
    if ((~s).state.get() != mSpanManual) {
        @throw("freeing stack not in a stack span"u8);
    }
    if ((~s).manualFreeList.ptr() == nil) {
        // s will now have a free stack
        stackpool[order].item.span.insert(s);
    }
    x.ptr().val.next = s.val.manualFreeList;
    s.val.manualFreeList = x;
    (~s).allocCount--;
    if (gcphase == _GCoff && (~s).allocCount == 0) {
        // Span is completely free. Return it to the heap
        // immediately if we're sweeping.
        //
        // If GC is active, we delay the free until the end of
        // GC to avoid the following type of situation:
        //
        // 1) GC starts, scans a SudoG but does not yet mark the SudoG.elem pointer
        // 2) The stack that pointer points to is copied
        // 3) The old stack is freed
        // 4) The containing span is marked free
        // 5) GC attempts to mark the SudoG.elem pointer. The
        //    marking fails because the pointer looks like a
        //    pointer into a free span.
        //
        // By not freeing, we prevent step #4 until GC is done.
        stackpool[order].item.span.remove(s);
        s.val.manualFreeList = 0;
        osStackFree(s);
        mheap_.freeManual(s, spanAllocStack);
    }
}

// stackcacherefill/stackcacherelease implement a global pool of stack segments.
// The pool is required to prevent unlimited growth of per-thread caches.
//
//go:systemstack
internal static void stackcacherefill(ж<mcache> Ꮡc, uint8 order) {
    ref var c = ref Ꮡc.val;

    if (stackDebug >= 1) {
        print("stackcacherefill order=", order, "\n");
    }
    // Grab some stacks from the global cache.
    // Grab half of the allowed capacity (to prevent thrashing).
    gclinkptr list = default!;
    uintptr size = default!;
    @lock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
    while (size < _StackCacheSize / 2) {
        var x = stackpoolalloc(order);
        x.ptr().val.next = list;
        list = x;
        size += fixedStack << (int)(order);
    }
    unlock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
    c.stackcache[order].list = list;
    c.stackcache[order].size = size;
}

//go:systemstack
internal static void stackcacherelease(ж<mcache> Ꮡc, uint8 order) {
    ref var c = ref Ꮡc.val;

    if (stackDebug >= 1) {
        print("stackcacherelease order=", order, "\n");
    }
    var x = c.stackcache[order].list;
    var size = c.stackcache[order].size;
    @lock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
    while (size > _StackCacheSize / 2) {
        var y = x.ptr().val.next;
        stackpoolfree(x, order);
        x = y;
        size -= fixedStack << (int)(order);
    }
    unlock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
    c.stackcache[order].list = x;
    c.stackcache[order].size = size;
}

//go:systemstack
internal static void stackcache_clear(ж<mcache> Ꮡc) {
    ref var c = ref Ꮡc.val;

    if (stackDebug >= 1) {
        print("stackcache clear\n");
    }
    ref var order = ref heap<uint8>(out var Ꮡorder);
    for (order = ((uint8)0); order < _NumStackOrders; order++) {
        @lock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
        var x = c.stackcache[order].list;
        while (x.ptr() != nil) {
            var y = x.ptr().val.next;
            stackpoolfree(x, order);
            x = y;
        }
        c.stackcache[order].list = 0;
        c.stackcache[order].size = 0;
        unlock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
    }
}

// stackalloc allocates an n byte stack.
//
// stackalloc must run on the system stack because it uses per-P
// resources and must not split the stack.
//
//go:systemstack
internal static Δstack @stackalloc(uint32 n) {
    // Stackalloc must be called on scheduler stack, so that we
    // never try to grow the stack during the code that stackalloc runs.
    // Doing so would cause a deadlock (issue 1547).
    var thisg = getg();
    if (thisg != (~(~thisg).m).g0) {
        @throw("stackalloc not on scheduler stack"u8);
    }
    if ((uint32)(n & (n - 1)) != 0) {
        @throw("stack size not a power of 2"u8);
    }
    if (stackDebug >= 1) {
        print("stackalloc ", n, "\n");
    }
    if (debug.efence != 0 || stackFromSystem != 0) {
        n = ((uint32)alignUp(((uintptr)n), physPageSize));
        @unsafe.Pointer vΔ1 = (uintptr)sysAlloc(((uintptr)n), Ꮡmemstats.of(mstats.Ꮡstacks_sys));
        if (vΔ1 == nil) {
            @throw("out of memory (stackalloc)"u8);
        }
        return new Δstack(((uintptr)vΔ1), ((uintptr)vΔ1) + ((uintptr)n));
    }
    // Small stacks are allocated with a fixed-size free-list allocator.
    // If we need a stack of a bigger size, we fall back on allocating
    // a dedicated span.
    @unsafe.Pointer v = default!;
    if (n < fixedStack << (int)(_NumStackOrders) && n < _StackCacheSize){
        ref var order = ref heap<uint8>(out var Ꮡorder);
        order = ((uint8)0);
        var n2 = n;
        while (n2 > fixedStack) {
            order++;
            n2 >>= (UntypedInt)(1);
        }
        gclinkptr x = default!;
        if (stackNoCache != 0 || (~(~thisg).m).p == 0 || (~(~thisg).m).preemptoff != ""u8){
            // thisg.m.p == 0 can happen in the guts of exitsyscall
            // or procresize. Just get a stack from the global pool.
            // Also don't touch stackcache during gc
            // as it's flushed concurrently.
            @lock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
            x = stackpoolalloc(order);
            unlock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
        } else {
            var c = (~(~thisg).m).p.ptr().val.mcache;
            x = (~c).stackcache[order].list;
            if (x.ptr() == nil) {
                stackcacherefill(c, order);
                x = (~c).stackcache[order].list;
            }
            (~c).stackcache[order].list = x.ptr().val.next;
            (~c).stackcache[order].size -= ((uintptr)n);
        }
        v = ((@unsafe.Pointer)x);
    } else {
        ж<mspan> s = default!;
        var npage = ((uintptr)n) >> (int)(_PageShift);
        nint log2npage = stacklog2(npage);
        // Try to get a stack from the large stack cache.
        @lock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
        if (!stackLarge.free[log2npage].isEmpty()) {
            s = stackLarge.free[log2npage].first;
            stackLarge.free[log2npage].remove(s);
        }
        unlock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
        lockWithRankMayAcquire(Ꮡmheap_.of(mheap.Ꮡlock), lockRankMheap);
        if (s == nil) {
            // Allocate a new stack from the heap.
            s = mheap_.allocManual(npage, spanAllocStack);
            if (s == nil) {
                @throw("out of memory"u8);
            }
            osStackAlloc(s);
            s.val.elemsize = ((uintptr)n);
        }
        v = ((@unsafe.Pointer)s.@base());
    }
    if (traceAllocFreeEnabled()) {
        var Δtrace = traceTryAcquire();
        if (Δtrace.ok()) {
            Δtrace.GoroutineStackAlloc(((uintptr)v), ((uintptr)n));
            traceRelease(Δtrace);
        }
    }
    if (raceenabled) {
        racemalloc(v, ((uintptr)n));
    }
    if (msanenabled) {
        msanmalloc(v, ((uintptr)n));
    }
    if (asanenabled) {
        asanunpoison(v, ((uintptr)n));
    }
    if (stackDebug >= 1) {
        print("  allocated ", v, "\n");
    }
    return new Δstack(((uintptr)v), ((uintptr)v) + ((uintptr)n));
}

// stackfree frees an n byte stack allocation at stk.
//
// stackfree must run on the system stack because it uses per-P
// resources and must not split the stack.
//
//go:systemstack
internal static void stackfree(Δstack stk) {
    var gp = getg();
    @unsafe.Pointer v = ((@unsafe.Pointer)stk.lo);
    var n = stk.hi - stk.lo;
    if ((uintptr)(n & (n - 1)) != 0) {
        @throw("stack not a power of 2"u8);
    }
    if (stk.lo + n < stk.hi) {
        @throw("bad stack size"u8);
    }
    if (stackDebug >= 1) {
        println("stackfree", v, n);
        memclrNoHeapPointers(v, n);
    }
    // for testing, clobber stack data
    if (debug.efence != 0 || stackFromSystem != 0) {
        if (debug.efence != 0 || stackFaultOnFree != 0){
            sysFault(v, n);
        } else {
            sysFree(v, n, Ꮡmemstats.of(mstats.Ꮡstacks_sys));
        }
        return;
    }
    if (traceAllocFreeEnabled()) {
        var Δtrace = traceTryAcquire();
        if (Δtrace.ok()) {
            Δtrace.GoroutineStackFree(((uintptr)v));
            traceRelease(Δtrace);
        }
    }
    if (msanenabled) {
        msanfree(v, n);
    }
    if (asanenabled) {
        asanpoison(v, n);
    }
    if (n < fixedStack << (int)(_NumStackOrders) && n < _StackCacheSize){
        ref var order = ref heap<uint8>(out var Ꮡorder);
        order = ((uint8)0);
        var n2 = n;
        while (n2 > fixedStack) {
            order++;
            n2 >>= (UntypedInt)(1);
        }
        var x = ((gclinkptr)v);
        if (stackNoCache != 0 || (~(~gp).m).p == 0 || (~(~gp).m).preemptoff != ""u8){
            @lock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
            stackpoolfree(x, order);
            unlock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
        } else {
            var c = (~(~gp).m).p.ptr().val.mcache;
            if ((~c).stackcache[order].size >= _StackCacheSize) {
                stackcacherelease(c, order);
            }
            x.ptr().val.next = (~c).stackcache[order].list;
            (~c).stackcache[order].list = x;
            (~c).stackcache[order].size += n;
        }
    } else {
        var s = spanOfUnchecked(((uintptr)v));
        if ((~s).state.get() != mSpanManual) {
            println(((Δhex)s.@base()), v);
            @throw("bad span state"u8);
        }
        if (gcphase == _GCoff){
            // Free the stack immediately if we're
            // sweeping.
            osStackFree(s);
            mheap_.freeManual(s, spanAllocStack);
        } else {
            // If the GC is running, we can't return a
            // stack span to the heap because it could be
            // reused as a heap span, and this state
            // change would race with GC. Add it to the
            // large stack cache instead.
            nint log2npage = stacklog2((~s).npages);
            @lock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
            stackLarge.free[log2npage].insert(s);
            unlock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
        }
    }
}

internal static uintptr maxstacksize = 1 << (int)(20); // enough until runtime.main sets it for real

internal static uintptr maxstackceiling = maxstacksize;

internal static slice<@string> ptrnames = new slice<@string>(2){
    [0] = "scalar"u8,
    [1] = "ptr"u8
};

// Stack frame layout
//
// (x86)
// +------------------+
// | args from caller |
// +------------------+ <- frame->argp
// |  return address  |
// +------------------+
// |  caller's BP (*) | (*) if framepointer_enabled && varp > sp
// +------------------+ <- frame->varp
// |     locals       |
// +------------------+
// |  args to callee  |
// +------------------+ <- frame->sp
//
// (arm)
// +------------------+
// | args from caller |
// +------------------+ <- frame->argp
// | caller's retaddr |
// +------------------+
// |  caller's FP (*) | (*) on ARM64, if framepointer_enabled && varp > sp
// +------------------+ <- frame->varp
// |     locals       |
// +------------------+
// |  args to callee  |
// +------------------+
// |  return address  |
// +------------------+ <- frame->sp
//
// varp > sp means that the function has a frame;
// varp == sp means frameless function.
[GoType] partial struct adjustinfo {
    internal Δstack old;
    internal uintptr delta; // ptr distance from old to new stack (newbase - oldbase)
    // sghi is the highest sudog.elem on the stack.
    internal uintptr sghi;
}

// adjustpointer checks whether *vpp is in the old stack described by adjinfo.
// If so, it rewrites *vpp to point into the new stack.
internal static void adjustpointer(ж<adjustinfo> Ꮡadjinfo, @unsafe.Pointer vpp) {
    ref var adjinfo = ref Ꮡadjinfo.val;

    var pp = ((ж<uintptr>)vpp);
    var Δp = pp.val;
    if (stackDebug >= 4) {
        print("        ", pp, ":", ((Δhex)Δp), "\n");
    }
    if (adjinfo.old.lo <= Δp && Δp < adjinfo.old.hi) {
        pp.val = Δp + adjinfo.delta;
        if (stackDebug >= 3) {
            print("        adjust ptr ", pp, ":", ((Δhex)Δp), " -> ", ((Δhex)(pp.val)), "\n");
        }
    }
}

// Information from the compiler about the layout of stack frames.
// Note: this type must agree with reflect.bitVector.
[GoType] partial struct bitvector {
    internal int32 n; // # of bits
    internal ж<uint8> bytedata;
}

// ptrbit returns the i'th bit in bv.
// ptrbit is less efficient than iterating directly over bitvector bits,
// and should only be used in non-performance-critical code.
// See adjustpointers for an example of a high-efficiency walk of a bitvector.
[GoRecv] internal static uint8 ptrbit(this ref bitvector bv, uintptr i) {
    var b = (addb(bv.bytedata, i / 8)).val;
    return (byte)((b >> (int)((i % 8))) & 1);
}

// bv describes the memory starting at address scanp.
// Adjust any pointers contained therein.
internal static void adjustpointers(@unsafe.Pointer scanp, ж<bitvector> Ꮡbv, ж<adjustinfo> Ꮡadjinfo, ΔfuncInfo f) {
    ref var bv = ref Ꮡbv.val;
    ref var adjinfo = ref Ꮡadjinfo.val;

    var minp = adjinfo.old.lo;
    var maxp = adjinfo.old.hi;
    var delta = adjinfo.delta;
    var num = ((uintptr)bv.n);
    // If this frame might contain channel receive slots, use CAS
    // to adjust pointers. If the slot hasn't been received into
    // yet, it may contain stack pointers and a concurrent send
    // could race with adjusting those pointers. (The sent value
    // itself can never contain stack pointers.)
    var useCAS = ((uintptr)scanp) < adjinfo.sghi;
    for (var i = ((uintptr)0); i < num; i += 8) {
        if (stackDebug >= 4) {
            for (var j = ((uintptr)0); j < 8; j++) {
                print("        ", (uintptr)add(scanp.val, (i + j) * goarch.PtrSize), ":", ptrnames[bv.ptrbit(i + j)], ":", ((Δhex)(~(ж<uintptr>)(uintptr)((uintptr)add(scanp.val, (i + j) * goarch.PtrSize)))), " # ", i, " ", addb(bv.bytedata, i / 8).val, "\n");
            }
        }
        var b = (addb(bv.bytedata, i / 8)).val;
        while (b != 0) {
            var j = ((uintptr)sys.TrailingZeros8(b));
            b &= (byte)(b - 1);
            var pp = ((ж<uintptr>)(uintptr)add(scanp.val, (i + j) * goarch.PtrSize));
retry:
            var Δp = pp.val;
            if (f.valid() && 0 < Δp && Δp < minLegalPointer && debug.invalidptr != 0) {
                // Looks like a junk value in a pointer slot.
                // Live analysis wrong?
                (~getg()).m.val.traceback = 2;
                print("runtime: bad pointer in frame ", funcname(f), " at ", pp, ": ", ((Δhex)Δp), "\n");
                @throw("invalid pointer found on stack"u8);
            }
            if (minp <= Δp && Δp < maxp) {
                if (stackDebug >= 3) {
                    print("adjust ptr ", ((Δhex)Δp), " ", funcname(f), "\n");
                }
                if (useCAS){
                    var ppu = (ж<@unsafe.Pointer>)(uintptr)(((@unsafe.Pointer)pp));
                    if (!atomic.Casp1(ppu, ((@unsafe.Pointer)Δp), ((@unsafe.Pointer)(Δp + delta)))) {
                        goto retry;
                    }
                } else {
                    pp.val = Δp + delta;
                }
            }
        }
    }
}

// Note: the argument/return area is adjusted by the callee.
internal static void adjustframe(ж<stkframe> Ꮡframe, ж<adjustinfo> Ꮡadjinfo) {
    ref var frame = ref Ꮡframe.val;
    ref var adjinfo = ref Ꮡadjinfo.val;

    if (frame.continpc == 0) {
        // Frame is dead.
        return;
    }
    var f = frame.fn;
    if (stackDebug >= 2) {
        print("    adjusting ", funcname(f), " frame=[", ((Δhex)frame.sp), ",", ((Δhex)frame.fp), "] pc=", ((Δhex)frame.pc), " continpc=", ((Δhex)frame.continpc), "\n");
    }
    // Adjust saved frame pointer if there is one.
    if ((goarch.ArchFamily == goarch.AMD64 || goarch.ArchFamily == goarch.ARM64) && frame.argp - frame.varp == 2 * goarch.PtrSize) {
        if (stackDebug >= 3) {
            print("      saved bp\n");
        }
        if (debugCheckBP) {
            // Frame pointers should always point to the next higher frame on
            // the Go stack (or be nil, for the top frame on the stack).
            var bp = ~(ж<uintptr>)(uintptr)(((@unsafe.Pointer)frame.varp));
            if (bp != 0 && (bp < adjinfo.old.lo || bp >= adjinfo.old.hi)) {
                println("runtime: found invalid frame pointer");
                print("bp=", ((Δhex)bp), " min=", ((Δhex)adjinfo.old.lo), " max=", ((Δhex)adjinfo.old.hi), "\n");
                @throw("bad frame pointer"u8);
            }
        }
        // On AMD64, this is the caller's frame pointer saved in the current
        // frame.
        // On ARM64, this is the frame pointer of the caller's caller saved
        // by the caller in its frame (one word below its SP).
        adjustpointer(Ꮡadjinfo, ((@unsafe.Pointer)frame.varp));
    }
    (locals, args, objs) = frame.getStackMap(true);
    // Adjust local variables if stack frame has been allocated.
    if (locals.n > 0) {
        var size = ((uintptr)locals.n) * goarch.PtrSize;
        adjustpointers(((@unsafe.Pointer)(frame.varp - size)), Ꮡlocals, Ꮡadjinfo, f);
    }
    // Adjust arguments.
    if (args.n > 0) {
        if (stackDebug >= 3) {
            print("      args\n");
        }
        adjustpointers(((@unsafe.Pointer)frame.argp), Ꮡargs, Ꮡadjinfo, new ΔfuncInfo(nil));
    }
    // Adjust pointers in all stack objects (whether they are live or not).
    // See comments in mgcmark.go:scanframeworker.
    if (frame.varp != 0) {
        foreach (var (i, _) in objs) {
            var obj = Ꮡ(objs, i);
            var off = obj.val.off;
            var @base = frame.varp;
            // locals base pointer
            if (off >= 0) {
                @base = frame.argp;
            }
            // arguments and return values base pointer
            var Δp = @base + ((uintptr)off);
            if (Δp < frame.sp) {
                // Object hasn't been allocated in the frame yet.
                // (Happens when the stack bounds check fails and
                // we call into morestack.)
                continue;
            }
            var ptrdata = obj.ptrdata();
            var gcdata = obj.gcdata();
            ж<mspan> s = default!;
            if (obj.useGCProg()) {
                // See comments in mgcmark.go:scanstack
                s = materializeGCProg(ptrdata, gcdata);
                gcdata = (ж<byte>)(uintptr)(((@unsafe.Pointer)(~s).startAddr));
            }
            for (var iΔ1 = ((uintptr)0); iΔ1 < ptrdata; iΔ1 += goarch.PtrSize) {
                if ((byte)(addb(gcdata, iΔ1 / (8 * goarch.PtrSize)).val >> (int)(((uintptr)(iΔ1 / goarch.PtrSize & 7))) & 1) != 0) {
                    adjustpointer(Ꮡadjinfo, ((@unsafe.Pointer)(Δp + iΔ1)));
                }
            }
            if (s != nil) {
                dematerializeGCProg(s);
            }
        }
    }
}

internal static void adjustctxt(ж<g> Ꮡgp, ж<adjustinfo> Ꮡadjinfo) {
    ref var gp = ref Ꮡgp.val;
    ref var adjinfo = ref Ꮡadjinfo.val;

    adjustpointer(Ꮡadjinfo, ((@unsafe.Pointer)(Ꮡgp.sched.of(gobuf.Ꮡctxt))));
    if (!framepointer_enabled) {
        return;
    }
    if (debugCheckBP) {
        var bp = gp.sched.bp;
        if (bp != 0 && (bp < adjinfo.old.lo || bp >= adjinfo.old.hi)) {
            println("runtime: found invalid top frame pointer");
            print("bp=", ((Δhex)bp), " min=", ((Δhex)adjinfo.old.lo), " max=", ((Δhex)adjinfo.old.hi), "\n");
            @throw("bad top frame pointer"u8);
        }
    }
    var oldfp = gp.sched.bp;
    adjustpointer(Ꮡadjinfo, ((@unsafe.Pointer)(Ꮡgp.sched.of(gobuf.Ꮡbp))));
    if (GOARCH == "arm64"u8) {
        // On ARM64, the frame pointer is saved one word *below* the SP,
        // which is not copied or adjusted in any frame. Do it explicitly
        // here.
        if (oldfp == gp.sched.sp - goarch.PtrSize) {
            memmove(((@unsafe.Pointer)gp.sched.bp), ((@unsafe.Pointer)oldfp), goarch.PtrSize);
            adjustpointer(Ꮡadjinfo, ((@unsafe.Pointer)gp.sched.bp));
        }
    }
}

internal static void adjustdefers(ж<g> Ꮡgp, ж<adjustinfo> Ꮡadjinfo) {
    ref var gp = ref Ꮡgp.val;
    ref var adjinfo = ref Ꮡadjinfo.val;

    // Adjust pointers in the Defer structs.
    // We need to do this first because we need to adjust the
    // defer.link fields so we always work on the new stack.
    adjustpointer(Ꮡadjinfo, ((@unsafe.Pointer)(Ꮡ(gp._defer))));
    for (var d = gp._defer; d != nil; d = d.val.link) {
        adjustpointer(Ꮡadjinfo, new @unsafe.Pointer(Ꮡ((~d).fn)));
        adjustpointer(Ꮡadjinfo, ((@unsafe.Pointer)(Ꮡ((~d).sp))));
        adjustpointer(Ꮡadjinfo, ((@unsafe.Pointer)(Ꮡ((~d).link))));
    }
}

internal static void adjustpanics(ж<g> Ꮡgp, ж<adjustinfo> Ꮡadjinfo) {
    ref var gp = ref Ꮡgp.val;
    ref var adjinfo = ref Ꮡadjinfo.val;

    // Panics are on stack and already adjusted.
    // Update pointer to head of list in G.
    adjustpointer(Ꮡadjinfo, ((@unsafe.Pointer)(Ꮡ(gp._panic))));
}

internal static void adjustsudogs(ж<g> Ꮡgp, ж<adjustinfo> Ꮡadjinfo) {
    ref var gp = ref Ꮡgp.val;
    ref var adjinfo = ref Ꮡadjinfo.val;

    // the data elements pointed to by a SudoG structure
    // might be in the stack.
    for (var s = gp.waiting; s != nil; s = s.val.waitlink) {
        adjustpointer(Ꮡadjinfo, ((@unsafe.Pointer)(Ꮡ((~s).elem))));
    }
}

internal static void fillstack(Δstack stk, byte b) {
    for (var Δp = stk.lo; Δp < stk.hi; Δp++) {
        ((ж<byte>)(uintptr)(((@unsafe.Pointer)Δp))).val = b;
    }
}

internal static uintptr findsghi(ж<g> Ꮡgp, Δstack stk) {
    ref var gp = ref Ꮡgp.val;

    uintptr sghi = default!;
    for (var sg = gp.waiting; sg != nil; sg = sg.val.waitlink) {
        var Δp = ((uintptr)(~sg).elem) + ((uintptr)(~(~sg).c).elemsize);
        if (stk.lo <= Δp && Δp < stk.hi && Δp > sghi) {
            sghi = Δp;
        }
    }
    return sghi;
}

// syncadjustsudogs adjusts gp's sudogs and copies the part of gp's
// stack they refer to while synchronizing with concurrent channel
// operations. It returns the number of bytes of stack copied.
internal static uintptr syncadjustsudogs(ж<g> Ꮡgp, uintptr used, ж<adjustinfo> Ꮡadjinfo) {
    ref var gp = ref Ꮡgp.val;
    ref var adjinfo = ref Ꮡadjinfo.val;

    if (gp.waiting == nil) {
        return 0;
    }
    // Lock channels to prevent concurrent send/receive.
    ж<Δhchan> lastc = default!;
    for (var sg = gp.waiting; sg != nil; sg = sg.val.waitlink) {
        if ((~sg).c != lastc) {
            // There is a ranking cycle here between gscan bit and
            // hchan locks. Normally, we only allow acquiring hchan
            // locks and then getting a gscan bit. In this case, we
            // already have the gscan bit. We allow acquiring hchan
            // locks here as a special case, since a deadlock can't
            // happen because the G involved must already be
            // suspended. So, we get a special hchan lock rank here
            // that is lower than gscan, but doesn't allow acquiring
            // any other locks other than hchan.
            lockWithRank(Ꮡ((~(~sg).c).@lock), lockRankHchanLeaf);
        }
        lastc = sg.val.c;
    }
    // Adjust sudogs.
    adjustsudogs(Ꮡgp, Ꮡadjinfo);
    // Copy the part of the stack the sudogs point in to
    // while holding the lock to prevent races on
    // send/receive slots.
    uintptr sgsize = default!;
    if (adjinfo.sghi != 0) {
        var oldBot = adjinfo.old.hi - used;
        var newBot = oldBot + adjinfo.delta;
        sgsize = adjinfo.sghi - oldBot;
        memmove(((@unsafe.Pointer)newBot), ((@unsafe.Pointer)oldBot), sgsize);
    }
    // Unlock channels.
    lastc = default!;
    for (var sg = gp.waiting; sg != nil; sg = sg.val.waitlink) {
        if ((~sg).c != lastc) {
            unlock(Ꮡ((~(~sg).c).@lock));
        }
        lastc = sg.val.c;
    }
    return sgsize;
}

// Copies gp's stack to a new stack of a different size.
// Caller must have changed gp status to Gcopystack.
internal static void copystack(ж<g> Ꮡgp, uintptr newsize) {
    ref var gp = ref Ꮡgp.val;

    if (gp.syscallsp != 0) {
        @throw("stack growth not allowed in system call"u8);
    }
    var old = gp.stack;
    if (old.lo == 0) {
        @throw("nil stackbase"u8);
    }
    var used = old.hi - gp.sched.sp;
    // Add just the difference to gcController.addScannableStack.
    // g0 stacks never move, so this will never account for them.
    // It's also fine if we have no P, addScannableStack can deal with
    // that case.
    gcController.addScannableStack((~(~getg()).m).p.ptr(), ((int64)newsize) - ((int64)(old.hi - old.lo)));
    // allocate new stack
    var @new = @stackalloc(((uint32)newsize));
    if (stackPoisonCopy != 0) {
        fillstack(@new, 253);
    }
    if (stackDebug >= 1) {
        print("copystack gp=", gp, " [", ((Δhex)old.lo), " ", ((Δhex)(old.hi - used)), " ", ((Δhex)old.hi), "]", " -> [", ((Δhex)@new.lo), " ", ((Δhex)(@new.hi - used)), " ", ((Δhex)@new.hi), "]/", newsize, "\n");
    }
    // Compute adjustment.
    ref var adjinfo = ref heap(new adjustinfo(), out var Ꮡadjinfo);
    adjinfo.old = old;
    adjinfo.delta = @new.hi - old.hi;
    // Adjust sudogs, synchronizing with channel ops if necessary.
    var ncopy = used;
    if (!gp.activeStackChans){
        if (newsize < old.hi - old.lo && gp.parkingOnChan.Load()) {
            // It's not safe for someone to shrink this stack while we're actively
            // parking on a channel, but it is safe to grow since we do that
            // ourselves and explicitly don't want to synchronize with channels
            // since we could self-deadlock.
            @throw("racy sudog adjustment due to parking on channel"u8);
        }
        adjustsudogs(Ꮡgp, Ꮡadjinfo);
    } else {
        // sudogs may be pointing in to the stack and gp has
        // released channel locks, so other goroutines could
        // be writing to gp's stack. Find the highest such
        // pointer so we can handle everything there and below
        // carefully. (This shouldn't be far from the bottom
        // of the stack, so there's little cost in handling
        // everything below it carefully.)
        adjinfo.sghi = findsghi(Ꮡgp, old);
        // Synchronize with channel ops and copy the part of
        // the stack they may interact with.
        ncopy -= syncadjustsudogs(Ꮡgp, used, Ꮡadjinfo);
    }
    // Copy the stack (or the rest of it) to the new location
    memmove(((@unsafe.Pointer)(@new.hi - ncopy)), ((@unsafe.Pointer)(old.hi - ncopy)), ncopy);
    // Adjust remaining structures that have pointers into stacks.
    // We have to do most of these before we traceback the new
    // stack because gentraceback uses them.
    adjustctxt(Ꮡgp, Ꮡadjinfo);
    adjustdefers(Ꮡgp, Ꮡadjinfo);
    adjustpanics(Ꮡgp, Ꮡadjinfo);
    if (adjinfo.sghi != 0) {
        adjinfo.sghi += adjinfo.delta;
    }
    // Swap out old stack for new one
    gp.stack = @new;
    gp.stackguard0 = @new.lo + stackGuard;
    // NOTE: might clobber a preempt request
    gp.sched.sp = @new.hi - used;
    gp.stktopsp += adjinfo.delta;
    // Adjust pointers in the new stack.
    ref var u = ref heap(new unwinder(), out var Ꮡu);
    for (
    u.init(Ꮡgp, 0);; u.valid(); 
    u.next();) {
        adjustframe(Ꮡu.of(unwinder.Ꮡframe), Ꮡadjinfo);
    }
    // free old stack
    if (stackPoisonCopy != 0) {
        fillstack(old, 252);
    }
    stackfree(old);
}

// round x up to a power of 2.
internal static int32 round2(int32 x) {
    nuint s = ((nuint)0);
    while (1 << (int)(s) < x) {
        s++;
    }
    return 1 << (int)(s);
}

// Called from runtime·morestack when more stack is needed.
// Allocate larger stack and relocate to new stack.
// Stack growth is multiplicative, for constant amortized cost.
//
// g->atomicstatus will be Grunning or Gscanrunning upon entry.
// If the scheduler is trying to stop this g, then it will set preemptStop.
//
// This must be nowritebarrierrec because it can be called as part of
// stack growth from other nowritebarrierrec functions, but the
// compiler doesn't check this.
//
//go:nowritebarrierrec
internal static void newstack() {
    var thisg = getg();
    // TODO: double check all gp. shouldn't be getg().
    if ((~(~(~thisg).m).morebuf.g.ptr()).stackguard0 == stackFork) {
        @throw("stack growth after fork"u8);
    }
    if ((~(~thisg).m).morebuf.g.ptr() != (~(~thisg).m).curg) {
        print("runtime: newstack called from g=", ((Δhex)(~(~thisg).m).morebuf.g), "\n"u8 + "\tm="u8, (~thisg).m, " m->curg=", (~(~thisg).m).curg, " m->g0=", (~(~thisg).m).g0, " m->gsignal=", (~(~thisg).m).gsignal, "\n");
        var morebufΔ1 = (~thisg).m.val.morebuf;
        traceback(morebufΔ1.pc, morebufΔ1.sp, morebufΔ1.lr, morebufΔ1.g.ptr());
        @throw("runtime: wrong goroutine in newstack"u8);
    }
    var gp = (~thisg).m.val.curg;
    if ((~(~(~thisg).m).curg).throwsplit) {
        // Update syscallsp, syscallpc in case traceback uses them.
        var morebufΔ2 = (~thisg).m.val.morebuf;
        gp.val.syscallsp = morebufΔ2.sp;
        gp.val.syscallpc = morebufΔ2.pc;
        @string pcname = "(unknown)"u8;
        var pcoff = ((uintptr)0);
        var f = findfunc((~gp).sched.pc);
        if (f.valid()) {
            pcname = funcname(f);
            pcoff = (~gp).sched.pc - f.entry();
        }
        print("runtime: newstack at ", pcname, "+", ((Δhex)pcoff),
            " sp=", ((Δhex)(~gp).sched.sp), " stack=[", ((Δhex)(~gp).stack.lo), ", ", ((Δhex)(~gp).stack.hi), "]\n",
            "\tmorebuf={pc:", ((Δhex)morebufΔ2.pc), " sp:", ((Δhex)morebufΔ2.sp), " lr:", ((Δhex)morebufΔ2.lr), "}\n",
            "\tsched={pc:", ((Δhex)(~gp).sched.pc), " sp:", ((Δhex)(~gp).sched.sp), " lr:", ((Δhex)(~gp).sched.lr), " ctxt:", (~gp).sched.ctxt, "}\n");
        (~thisg).m.val.traceback = 2;
        // Include runtime frames
        traceback(morebufΔ2.pc, morebufΔ2.sp, morebufΔ2.lr, gp);
        @throw("runtime: stack split at bad time"u8);
    }
    var morebuf = (~thisg).m.val.morebuf;
    (~(~thisg).m).morebuf.pc = 0;
    (~(~thisg).m).morebuf.lr = 0;
    (~(~thisg).m).morebuf.sp = 0;
    (~(~thisg).m).morebuf.g = 0;
    // NOTE: stackguard0 may change underfoot, if another thread
    // is about to try to preempt gp. Read it just once and use that same
    // value now and below.
    var stackguard0 = atomic.Loaduintptr(Ꮡ((~gp).stackguard0));
    // Be conservative about where we preempt.
    // We are interested in preempting user Go code, not runtime code.
    // If we're holding locks, mallocing, or preemption is disabled, don't
    // preempt.
    // This check is very early in newstack so that even the status change
    // from Grunning to Gwaiting and back doesn't happen in this case.
    // That status change by itself can be viewed as a small preemption,
    // because the GC might change Gwaiting to Gscanwaiting, and then
    // this goroutine has to wait for the GC to finish before continuing.
    // If the GC is in some way dependent on this goroutine (for example,
    // it needs a lock held by the goroutine), that small preemption turns
    // into a real deadlock.
    var preempt = stackguard0 == stackPreempt;
    if (preempt) {
        if (!canPreemptM((~thisg).m)) {
            // Let the goroutine keep running for now.
            // gp->preempt is set, so it will be preempted next time.
            gp.val.stackguard0 = (~gp).stack.lo + stackGuard;
            gogo(Ꮡ((~gp).sched));
        }
    }
    // never return
    if ((~gp).stack.lo == 0) {
        @throw("missing stack in newstack"u8);
    }
    var sp = (~gp).sched.sp;
    if (goarch.ArchFamily == goarch.AMD64 || goarch.ArchFamily == goarch.I386 || goarch.ArchFamily == goarch.WASM) {
        // The call to morestack cost a word.
        sp -= goarch.PtrSize;
    }
    if (stackDebug >= 1 || sp < (~gp).stack.lo) {
        print("runtime: newstack sp=", ((Δhex)sp), " stack=[", ((Δhex)(~gp).stack.lo), ", ", ((Δhex)(~gp).stack.hi), "]\n",
            "\tmorebuf={pc:", ((Δhex)morebuf.pc), " sp:", ((Δhex)morebuf.sp), " lr:", ((Δhex)morebuf.lr), "}\n",
            "\tsched={pc:", ((Δhex)(~gp).sched.pc), " sp:", ((Δhex)(~gp).sched.sp), " lr:", ((Δhex)(~gp).sched.lr), " ctxt:", (~gp).sched.ctxt, "}\n");
    }
    if (sp < (~gp).stack.lo) {
        print("runtime: gp=", gp, ", goid=", (~gp).goid, ", gp->status=", ((Δhex)readgstatus(gp)), "\n ");
        print("runtime: split stack overflow: ", ((Δhex)sp), " < ", ((Δhex)(~gp).stack.lo), "\n");
        @throw("runtime: split stack overflow"u8);
    }
    if (preempt) {
        if (gp == (~(~thisg).m).g0) {
            @throw("runtime: preempt g0"u8);
        }
        if ((~(~thisg).m).p == 0 && (~(~thisg).m).locks == 0) {
            @throw("runtime: g is running but p is not"u8);
        }
        if ((~gp).preemptShrink) {
            // We're at a synchronous safe point now, so
            // do the pending stack shrink.
            gp.val.preemptShrink = false;
            shrinkstack(gp);
        }
        if ((~gp).preemptStop) {
            preemptPark(gp);
        }
        // never returns
        // Act like goroutine called runtime.Gosched.
        gopreempt_m(gp);
    }
    // never return
    // Allocate a bigger segment and move the stack.
    var oldsize = (~gp).stack.hi - (~gp).stack.lo;
    var newsize = oldsize * 2;
    // Make sure we grow at least as much as needed to fit the new frame.
    // (This is just an optimization - the caller of morestack will
    // recheck the bounds on return.)
    {
        var f = findfunc((~gp).sched.pc); if (f.valid()) {
            var max = ((uintptr)funcMaxSPDelta(f));
            var needed = max + stackGuard;
            var used = (~gp).stack.hi - (~gp).sched.sp;
            while (newsize - used < needed) {
                newsize *= 2;
            }
        }
    }
    if (stackguard0 == stackForceMove) {
        // Forced stack movement used for debugging.
        // Don't double the stack (or we may quickly run out
        // if this is done repeatedly).
        newsize = oldsize;
    }
    if (newsize > maxstacksize || newsize > maxstackceiling) {
        if (maxstacksize < maxstackceiling){
            print("runtime: goroutine stack exceeds ", maxstacksize, "-byte limit\n");
        } else {
            print("runtime: goroutine stack exceeds ", maxstackceiling, "-byte limit\n");
        }
        print("runtime: sp=", ((Δhex)sp), " stack=[", ((Δhex)(~gp).stack.lo), ", ", ((Δhex)(~gp).stack.hi), "]\n");
        @throw("stack overflow"u8);
    }
    // The goroutine must be executing in order to call newstack,
    // so it must be Grunning (or Gscanrunning).
    casgstatus(gp, _Grunning, _Gcopystack);
    // The concurrent GC will not scan the stack while we are doing the copy since
    // the gp is in a Gcopystack status.
    copystack(gp, newsize);
    if (stackDebug >= 1) {
        print("stack grow done\n");
    }
    casgstatus(gp, _Gcopystack, _Grunning);
    gogo(Ꮡ((~gp).sched));
}

//go:nosplit
internal static void nilfunc() {
    ((ж<uint8>)(uintptr)(default!)).val = 0;
}

// adjust Gobuf as if it executed a call to fn
// and then stopped before the first instruction in fn.
internal static void gostartcallfn(ж<gobuf> Ꮡgobuf, ж<funcval> Ꮡfv) {
    ref var gobuf = ref Ꮡgobuf.val;
    ref var fv = ref Ꮡfv.val;

    @unsafe.Pointer fn = default!;
    if (fv != nil){
        fn = ((@unsafe.Pointer)fv.fn);
    } else {
        fn = ((@unsafe.Pointer)abi.FuncPCABIInternal(nilfunc));
    }
    gostartcall(Ꮡgobuf, fn, new @unsafe.Pointer(Ꮡfv));
}

// isShrinkStackSafe returns whether it's safe to attempt to shrink
// gp's stack. Shrinking the stack is only safe when we have precise
// pointer maps for all frames on the stack. The caller must hold the
// _Gscan bit for gp or must be running gp itself.
internal static bool isShrinkStackSafe(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.val;

    // We can't copy the stack if we're in a syscall.
    // The syscall might have pointers into the stack and
    // often we don't have precise pointer maps for the innermost
    // frames.
    if (gp.syscallsp != 0) {
        return false;
    }
    // We also can't copy the stack if we're at an asynchronous
    // safe-point because we don't have precise pointer maps for
    // all frames.
    if (gp.asyncSafePoint) {
        return false;
    }
    // We also can't *shrink* the stack in the window between the
    // goroutine calling gopark to park on a channel and
    // gp.activeStackChans being set.
    if (gp.parkingOnChan.Load()) {
        return false;
    }
    // We also can't copy the stack while tracing is enabled, and
    // gp is in _Gwaiting solely to make itself available to the GC.
    // In these cases, the G is actually executing on the system
    // stack, and the execution tracer may want to take a stack trace
    // of the G's stack. Note: it's safe to access gp.waitreason here.
    // We're only checking if this is true if we took ownership of the
    // G with the _Gscan bit. This prevents the goroutine from transitioning,
    // which prevents gp.waitreason from changing.
    if (traceEnabled() && (uint32)(readgstatus(Ꮡgp) & ~_Gscan) == _Gwaiting && gp.waitreason.isWaitingForGC()) {
        return false;
    }
    return true;
}

// Maybe shrink the stack being used by gp.
//
// gp must be stopped and we must own its stack. It may be in
// _Grunning, but only if this is our own user G.
internal static void shrinkstack(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.val;

    if (gp.stack.lo == 0) {
        @throw("missing stack in shrinkstack"u8);
    }
    {
        var s = readgstatus(Ꮡgp); if ((uint32)(s & _Gscan) == 0) {
            // We don't own the stack via _Gscan. We could still
            // own it if this is our own user G and we're on the
            // system stack.
            if (!(Ꮡgp == (~(~getg()).m).curg && getg() != (~(~getg()).m).curg && s == _Grunning)) {
                // We don't own the stack.
                @throw("bad status in shrinkstack"u8);
            }
        }
    }
    if (!isShrinkStackSafe(Ꮡgp)) {
        @throw("shrinkstack at bad time"u8);
    }
    // Check for self-shrinks while in a libcall. These may have
    // pointers into the stack disguised as uintptrs, but these
    // code paths should all be nosplit.
    if (Ꮡgp == (~(~getg()).m).curg && gp.m.libcallsp != 0) {
        @throw("shrinking stack in libcall"u8);
    }
    if (debug.gcshrinkstackoff > 0) {
        return;
    }
    var f = findfunc(gp.startpc);
    if (f.valid() && f.funcID == abi.FuncID_gcBgMarkWorker) {
        // We're not allowed to shrink the gcBgMarkWorker
        // stack (see gcBgMarkWorker for explanation).
        return;
    }
    var oldsize = gp.stack.hi - gp.stack.lo;
    var newsize = oldsize / 2;
    // Don't shrink the allocation below the minimum-sized stack
    // allocation.
    if (newsize < fixedStack) {
        return;
    }
    // Compute how much of the stack is currently in use and only
    // shrink the stack if gp is using less than a quarter of its
    // current stack. The currently used stack includes everything
    // down to the SP plus the stack guard space that ensures
    // there's room for nosplit functions.
    var avail = gp.stack.hi - gp.stack.lo;
    {
        var used = gp.stack.hi - gp.sched.sp + stackNosplit; if (used >= avail / 4) {
            return;
        }
    }
    if (stackDebug > 0) {
        print("shrinking stack ", oldsize, "->", newsize, "\n");
    }
    copystack(Ꮡgp, newsize);
}

// freeStackSpans frees unused stack spans at the end of GC.
internal static void freeStackSpans() {
    // Scan stack pools for empty stack spans.
    ref var order = ref heap(new nint(), out var Ꮡorder);

    foreach (var (order, _) in stackpool) {
        @lock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
        var list = Ꮡstackpool[order].item.of(stackpoolItem.Ꮡspan);
        for (var s = list.val.first; s != nil; ) {
            var next = s.val.next;
            if ((~s).allocCount == 0) {
                list.remove(s);
                s.val.manualFreeList = 0;
                osStackFree(s);
                mheap_.freeManual(s, spanAllocStack);
            }
            s = next;
        }
        unlock(Ꮡstackpool[order].item.of(stackpoolItem.Ꮡmu));
    }
    // Free large stack spans.
    @lock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
    foreach (var (i, _) in stackLarge.free) {
        for (var s = stackLarge.free[i].first; s != nil; ) {
            var next = s.val.next;
            stackLarge.free[i].remove(s);
            osStackFree(s);
            mheap_.freeManual(s, spanAllocStack);
            s = next;
        }
    }
    unlock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
}

// A stackObjectRecord is generated by the compiler for each stack object in a stack frame.
// This record must match the generator code in cmd/compile/internal/liveness/plive.go:emitStackObjects.
[GoType] partial struct stackObjectRecord {
    // offset in frame
    // if negative, offset from varp
    // if non-negative, offset from argp
    internal int32 off;
    internal int32 size;
    internal int32 _ptrdata;  // ptrdata, or -ptrdata is GC prog is used
    internal uint32 gcdataoff; // offset to gcdata from moduledata.rodata
}

[GoRecv] internal static bool useGCProg(this ref stackObjectRecord r) {
    return r._ptrdata < 0;
}

[GoRecv] internal static uintptr ptrdata(this ref stackObjectRecord r) {
    var x = r._ptrdata;
    if (x < 0) {
        return ((uintptr)(-x));
    }
    return ((uintptr)x);
}

// gcdata returns pointer map or GC prog of the type.
[GoRecv] internal static ж<byte> gcdata(this ref stackObjectRecord r) {
    var ptr = ((uintptr)(uintptr)@unsafe.Pointer.FromRef(ref r));
    ж<moduledata> mod = default!;
    for (var datap = Ꮡ(firstmoduledata); datap != nil; datap = datap.val.next) {
        if ((~datap).gofunc <= ptr && ptr < (~datap).end) {
            mod = datap;
            break;
        }
    }
    // If you get a panic here due to a nil mod,
    // you may have made a copy of a stackObjectRecord.
    // You must use the original pointer.
    var res = (~mod).rodata + ((uintptr)r.gcdataoff);
    return (ж<byte>)(uintptr)(((@unsafe.Pointer)res));
}

// This is exported as ABI0 via linkname so obj can call it.
//
//go:nosplit
//go:linkname morestackc
internal static void morestackc() {
    @throw("attempt to execute system stack code on user stack"u8);
}

// startingStackSize is the amount of stack that new goroutines start with.
// It is a power of 2, and between _FixedStack and maxstacksize, inclusive.
// startingStackSize is updated every GC by tracking the average size of
// stacks scanned during the GC.
internal static uint32 startingStackSize = fixedStack;

internal static void gcComputeStartingStackSize() {
    if (debug.adaptivestackstart == 0) {
        return;
    }
    // For details, see the design doc at
    // https://docs.google.com/document/d/1YDlGIdVTPnmUiTAavlZxBI1d9pwGQgZT7IKFKlIXohQ/edit?usp=sharing
    // The basic algorithm is to track the average size of stacks
    // and start goroutines with stack equal to that average size.
    // Starting at the average size uses at most 2x the space that
    // an ideal algorithm would have used.
    // This is just a heuristic to avoid excessive stack growth work
    // early in a goroutine's lifetime. See issue 18138. Stacks that
    // are allocated too small can still grow, and stacks allocated
    // too large can still shrink.
    uint64 scannedStackSize = default!;
    uint64 scannedStacks = default!;
    foreach (var (_, Δp) in allp) {
        scannedStackSize += Δp.val.scannedStackSize;
        scannedStacks += Δp.val.scannedStacks;
        // Reset for next time
        Δp.val.scannedStackSize = 0;
        Δp.val.scannedStacks = 0;
    }
    if (scannedStacks == 0) {
        startingStackSize = fixedStack;
        return;
    }
    var avg = scannedStackSize / scannedStacks + stackGuard;
    // Note: we add stackGuard to ensure that a goroutine that
    // uses the average space will not trigger a growth.
    if (avg > ((uint64)maxstacksize)) {
        avg = ((uint64)maxstacksize);
    }
    if (avg < fixedStack) {
        avg = fixedStack;
    }
    // Note: maxstacksize fits in 30 bits, so avg also does.
    startingStackSize = ((uint32)round2(((int32)avg)));
}

} // end runtime_package
