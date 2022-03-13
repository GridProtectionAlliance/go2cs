// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 13 05:27:09 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\stack.go
namespace go;

using abi = @internal.abi_package;
using cpu = @internal.cpu_package;
using atomic = runtime.@internal.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = @unsafe_package;


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

public static partial class runtime_package {

 
// StackSystem is a number of additional bytes to add
// to each stack below the usual guard area for OS-specific
// purposes like signal handling. Used on Windows, Plan 9,
// and iOS because they do not use a separate stack.
private static readonly var _StackSystem = sys.GoosWindows * 512 * sys.PtrSize + sys.GoosPlan9 * 512 + sys.GoosIos * sys.GoarchArm64 * 1024; 

// The minimum size of stack used by Go code
private static readonly nint _StackMin = 2048; 

// The minimum stack size to allocate.
// The hackery here rounds FixedStack0 up to a power of 2.
private static readonly var _FixedStack0 = _StackMin + _StackSystem;
private static readonly var _FixedStack1 = _FixedStack0 - 1;
private static readonly var _FixedStack2 = _FixedStack1 | (_FixedStack1 >> 1);
private static readonly var _FixedStack3 = _FixedStack2 | (_FixedStack2 >> 2);
private static readonly var _FixedStack4 = _FixedStack3 | (_FixedStack3 >> 4);
private static readonly var _FixedStack5 = _FixedStack4 | (_FixedStack4 >> 8);
private static readonly var _FixedStack6 = _FixedStack5 | (_FixedStack5 >> 16);
private static readonly var _FixedStack = _FixedStack6 + 1; 

// Functions that need frames bigger than this use an extra
// instruction to do the stack split check, to avoid overflow
// in case SP - framesize wraps below zero.
// This value can be no bigger than the size of the unmapped
// space at zero.
private static readonly nint _StackBig = 4096; 

// The stack guard is a pointer this many bytes above the
// bottom of the stack.
//
// The guard leaves enough room for one _StackSmall frame plus
// a _StackLimit chain of NOSPLIT calls plus _StackSystem
// bytes for the OS.
private static readonly nint _StackGuard = 928 * sys.StackGuardMultiplier + _StackSystem; 

// After a stack split check the SP is allowed to be this
// many bytes below the stack guard. This saves an instruction
// in the checking sequence for tiny frames.
private static readonly nint _StackSmall = 128; 

// The maximum number of bytes that a chain of NOSPLIT
// functions can use.
private static readonly var _StackLimit = _StackGuard - _StackSystem - _StackSmall;

 
// stackDebug == 0: no logging
//            == 1: logging of per-stack operations
//            == 2: logging of per-frame operations
//            == 3: logging of per-word updates
//            == 4: logging of per-word reads
private static readonly nint stackDebug = 0;
private static readonly nint stackFromSystem = 0; // allocate stacks from system memory instead of the heap
private static readonly nint stackFaultOnFree = 0; // old stacks are mapped noaccess to detect use after free
private static readonly nint stackPoisonCopy = 0; // fill stack that should not be accessed with garbage, to detect bad dereferences during copy
private static readonly nint stackNoCache = 0; // disable per-P small stack caches

// check the BP links during traceback.
private static readonly var debugCheckBP = false;

private static readonly nint uintptrMask = 1 << (int)((8 * sys.PtrSize)) - 1; 

// The values below can be stored to g.stackguard0 to force
// the next stack check to fail.
// These are all larger than any real SP.

// Goroutine preemption request.
// 0xfffffade in hex.
private static readonly var stackPreempt = uintptrMask & -1314; 

// Thread is forking. Causes a split stack check failure.
// 0xfffffb2e in hex.
private static readonly var stackFork = uintptrMask & -1234; 

// Force a stack movement. Used for debugging.
// 0xfffffeed in hex.
private static readonly var stackForceMove = uintptrMask & -275;

// Global pool of spans that have free stacks.
// Stacks are assigned an order according to size.
//     order = log_2(size/FixedStack)
// There is a free list for each order.
private static var stackpool = default;

//go:notinheap
private partial struct stackpoolItem {
    public mutex mu;
    public mSpanList span;
}

// Global pool of large stack spans.
private static var stackLarge = default;

private static void stackinit() {
    if (_StackCacheSize & _PageMask != 0) {
        throw("cache size must be a multiple of page size");
    }
    {
        var i__prev1 = i;

        foreach (var (__i) in stackpool) {
            i = __i;
            stackpool[i].item.span.init();
            lockInit(_addr_stackpool[i].item.mu, lockRankStackpool);
        }
        i = i__prev1;
    }

    {
        var i__prev1 = i;

        foreach (var (__i) in stackLarge.free) {
            i = __i;
            stackLarge.free[i].init();
            lockInit(_addr_stackLarge.@lock, lockRankStackLarge);
        }
        i = i__prev1;
    }
}

// stacklog2 returns ⌊log_2(n)⌋.
private static nint stacklog2(System.UIntPtr n) {
    nint log2 = 0;
    while (n > 1) {
        n>>=1;
        log2++;
    }
    return log2;
}

// Allocates a stack from the free pool. Must be called with
// stackpool[order].item.mu held.
private static gclinkptr stackpoolalloc(byte order) {
    var list = _addr_stackpool[order].item.span;
    var s = list.first;
    lockWithRankMayAcquire(_addr_mheap_.@lock, lockRankMheap);
    if (s == null) { 
        // no free stacks. Allocate another span worth.
        s = mheap_.allocManual(_StackCacheSize >> (int)(_PageShift), spanAllocStack);
        if (s == null) {
            throw("out of memory");
        }
        if (s.allocCount != 0) {
            throw("bad allocCount");
        }
        if (s.manualFreeList.ptr() != null) {
            throw("bad manualFreeList");
        }
        osStackAlloc(s);
        s.elemsize = _FixedStack << (int)(order);
        {
            var i = uintptr(0);

            while (i < _StackCacheSize) {
                var x = gclinkptr(s.@base() + i);
                x.ptr().next = s.manualFreeList;
                s.manualFreeList = x;
                i += s.elemsize;
            }

        }
        list.insert(s);
    }
    x = s.manualFreeList;
    if (x.ptr() == null) {
        throw("span has no free stacks");
    }
    s.manualFreeList = x.ptr().next;
    s.allocCount++;
    if (s.manualFreeList.ptr() == null) { 
        // all stacks in s are allocated.
        list.remove(s);
    }
    return x;
}

// Adds stack x to the free pool. Must be called with stackpool[order].item.mu held.
private static void stackpoolfree(gclinkptr x, byte order) {
    var s = spanOfUnchecked(uintptr(x));
    if (s.state.get() != mSpanManual) {
        throw("freeing stack not in a stack span");
    }
    if (s.manualFreeList.ptr() == null) { 
        // s will now have a free stack
        stackpool[order].item.span.insert(s);
    }
    x.ptr().next = s.manualFreeList;
    s.manualFreeList = x;
    s.allocCount--;
    if (gcphase == _GCoff && s.allocCount == 0) { 
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
        s.manualFreeList = 0;
        osStackFree(s);
        mheap_.freeManual(s, spanAllocStack);
    }
}

// stackcacherefill/stackcacherelease implement a global pool of stack segments.
// The pool is required to prevent unlimited growth of per-thread caches.
//
//go:systemstack
private static void stackcacherefill(ptr<mcache> _addr_c, byte order) {
    ref mcache c = ref _addr_c.val;

    if (stackDebug >= 1) {
        print("stackcacherefill order=", order, "\n");
    }
    gclinkptr list = default;
    System.UIntPtr size = default;
    lock(_addr_stackpool[order].item.mu);
    while (size < _StackCacheSize / 2) {
        var x = stackpoolalloc(order);
        x.ptr().next = list;
        list = x;
        size += _FixedStack << (int)(order);
    }
    unlock(_addr_stackpool[order].item.mu);
    c.stackcache[order].list = list;
    c.stackcache[order].size = size;
}

//go:systemstack
private static void stackcacherelease(ptr<mcache> _addr_c, byte order) {
    ref mcache c = ref _addr_c.val;

    if (stackDebug >= 1) {
        print("stackcacherelease order=", order, "\n");
    }
    var x = c.stackcache[order].list;
    var size = c.stackcache[order].size;
    lock(_addr_stackpool[order].item.mu);
    while (size > _StackCacheSize / 2) {
        var y = x.ptr().next;
        stackpoolfree(x, order);
        x = y;
        size -= _FixedStack << (int)(order);
    }
    unlock(_addr_stackpool[order].item.mu);
    c.stackcache[order].list = x;
    c.stackcache[order].size = size;
}

//go:systemstack
private static void stackcache_clear(ptr<mcache> _addr_c) {
    ref mcache c = ref _addr_c.val;

    if (stackDebug >= 1) {
        print("stackcache clear\n");
    }
    for (var order = uint8(0); order < _NumStackOrders; order++) {
        lock(_addr_stackpool[order].item.mu);
        var x = c.stackcache[order].list;
        while (x.ptr() != null) {
            var y = x.ptr().next;
            stackpoolfree(x, order);
            x = y;
        }
        c.stackcache[order].list = 0;
        c.stackcache[order].size = 0;
        unlock(_addr_stackpool[order].item.mu);
    }
}

// stackalloc allocates an n byte stack.
//
// stackalloc must run on the system stack because it uses per-P
// resources and must not split the stack.
//
//go:systemstack
private static stack @stackalloc(uint n) { 
    // Stackalloc must be called on scheduler stack, so that we
    // never try to grow the stack during the code that stackalloc runs.
    // Doing so would cause a deadlock (issue 1547).
    var thisg = getg();
    if (thisg != thisg.m.g0) {
        throw("stackalloc not on scheduler stack");
    }
    if (n & (n - 1) != 0) {
        throw("stack size not a power of 2");
    }
    if (stackDebug >= 1) {
        print("stackalloc ", n, "\n");
    }
    if (debug.efence != 0 || stackFromSystem != 0) {
        n = uint32(alignUp(uintptr(n), physPageSize));
        var v = sysAlloc(uintptr(n), _addr_memstats.stacks_sys);
        if (v == null) {
            throw("out of memory (stackalloc)");
        }
        return new stack(uintptr(v),uintptr(v)+uintptr(n));
    }
    v = default;
    if (n < _FixedStack << (int)(_NumStackOrders) && n < _StackCacheSize) {
        var order = uint8(0);
        var n2 = n;
        while (n2 > _FixedStack) {
            order++;
            n2>>=1;
        }
    else

        gclinkptr x = default;
        if (stackNoCache != 0 || thisg.m.p == 0 || thisg.m.preemptoff != "") { 
            // thisg.m.p == 0 can happen in the guts of exitsyscall
            // or procresize. Just get a stack from the global pool.
            // Also don't touch stackcache during gc
            // as it's flushed concurrently.
            lock(_addr_stackpool[order].item.mu);
            x = stackpoolalloc(order);
            unlock(_addr_stackpool[order].item.mu);
        }
        else
 {
            var c = thisg.m.p.ptr().mcache;
            x = c.stackcache[order].list;
            if (x.ptr() == null) {
                stackcacherefill(_addr_c, order);
                x = c.stackcache[order].list;
            }
            c.stackcache[order].list = x.ptr().next;
            c.stackcache[order].size -= uintptr(n);
        }
        v = @unsafe.Pointer(x);
    } {
        ptr<mspan> s;
        var npage = uintptr(n) >> (int)(_PageShift);
        var log2npage = stacklog2(npage); 

        // Try to get a stack from the large stack cache.
        lock(_addr_stackLarge.@lock);
        if (!stackLarge.free[log2npage].isEmpty()) {
            s = stackLarge.free[log2npage].first;
            stackLarge.free[log2npage].remove(s);
        }
        unlock(_addr_stackLarge.@lock);

        lockWithRankMayAcquire(_addr_mheap_.@lock, lockRankMheap);

        if (s == null) { 
            // Allocate a new stack from the heap.
            s = mheap_.allocManual(npage, spanAllocStack);
            if (s == null) {
                throw("out of memory");
            }
            osStackAlloc(s);
            s.elemsize = uintptr(n);
        }
        v = @unsafe.Pointer(s.@base());
    }
    if (raceenabled) {
        racemalloc(v, uintptr(n));
    }
    if (msanenabled) {
        msanmalloc(v, uintptr(n));
    }
    if (stackDebug >= 1) {
        print("  allocated ", v, "\n");
    }
    return new stack(uintptr(v),uintptr(v)+uintptr(n));
}

// stackfree frees an n byte stack allocation at stk.
//
// stackfree must run on the system stack because it uses per-P
// resources and must not split the stack.
//
//go:systemstack
private static void stackfree(stack stk) {
    var gp = getg();
    var v = @unsafe.Pointer(stk.lo);
    var n = stk.hi - stk.lo;
    if (n & (n - 1) != 0) {
        throw("stack not a power of 2");
    }
    if (stk.lo + n < stk.hi) {
        throw("bad stack size");
    }
    if (stackDebug >= 1) {
        println("stackfree", v, n);
        memclrNoHeapPointers(v, n); // for testing, clobber stack data
    }
    if (debug.efence != 0 || stackFromSystem != 0) {
        if (debug.efence != 0 || stackFaultOnFree != 0) {
            sysFault(v, n);
        }
        else
 {
            sysFree(v, n, _addr_memstats.stacks_sys);
        }
        return ;
    }
    if (msanenabled) {
        msanfree(v, n);
    }
    if (n < _FixedStack << (int)(_NumStackOrders) && n < _StackCacheSize) {
        var order = uint8(0);
        var n2 = n;
        while (n2 > _FixedStack) {
            order++;
            n2>>=1;
        }
    else

        var x = gclinkptr(v);
        if (stackNoCache != 0 || gp.m.p == 0 || gp.m.preemptoff != "") {
            lock(_addr_stackpool[order].item.mu);
            stackpoolfree(x, order);
            unlock(_addr_stackpool[order].item.mu);
        }
        else
 {
            var c = gp.m.p.ptr().mcache;
            if (c.stackcache[order].size >= _StackCacheSize) {
                stackcacherelease(_addr_c, order);
            }
            x.ptr().next = c.stackcache[order].list;
            c.stackcache[order].list = x;
            c.stackcache[order].size += n;
        }
    } {
        var s = spanOfUnchecked(uintptr(v));
        if (s.state.get() != mSpanManual) {
            println(hex(s.@base()), v);
            throw("bad span state");
        }
        if (gcphase == _GCoff) { 
            // Free the stack immediately if we're
            // sweeping.
            osStackFree(s);
            mheap_.freeManual(s, spanAllocStack);
        }
        else
 { 
            // If the GC is running, we can't return a
            // stack span to the heap because it could be
            // reused as a heap span, and this state
            // change would race with GC. Add it to the
            // large stack cache instead.
            var log2npage = stacklog2(s.npages);
            lock(_addr_stackLarge.@lock);
            stackLarge.free[log2npage].insert(s);
            unlock(_addr_stackLarge.@lock);
        }
    }
}

private static System.UIntPtr maxstacksize = 1 << 20; // enough until runtime.main sets it for real

private static var maxstackceiling = maxstacksize;

private static @string ptrnames = new slice<@string>(InitKeyedValues<@string>((0, "scalar"), (1, "ptr")));

// Stack frame layout
//
// (x86)
// +------------------+
// | args from caller |
// +------------------+ <- frame->argp
// |  return address  |
// +------------------+
// |  caller's BP (*) | (*) if framepointer_enabled && varp < sp
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
// +------------------+ <- frame->varp
// |     locals       |
// +------------------+
// |  args to callee  |
// +------------------+
// |  return address  |
// +------------------+ <- frame->sp

private partial struct adjustinfo {
    public stack old;
    public System.UIntPtr delta; // ptr distance from old to new stack (newbase - oldbase)
    public pcvalueCache cache; // sghi is the highest sudog.elem on the stack.
    public System.UIntPtr sghi;
}

// Adjustpointer checks whether *vpp is in the old stack described by adjinfo.
// If so, it rewrites *vpp to point into the new stack.
private static void adjustpointer(ptr<adjustinfo> _addr_adjinfo, unsafe.Pointer vpp) {
    ref adjustinfo adjinfo = ref _addr_adjinfo.val;

    var pp = (uintptr.val)(vpp);
    var p = pp.val;
    if (stackDebug >= 4) {
        print("        ", pp, ":", hex(p), "\n");
    }
    if (adjinfo.old.lo <= p && p < adjinfo.old.hi) {
        pp.val = p + adjinfo.delta;
        if (stackDebug >= 3) {
            print("        adjust ptr ", pp, ":", hex(p), " -> ", hex(pp.val), "\n");
        }
    }
}

// Information from the compiler about the layout of stack frames.
// Note: this type must agree with reflect.bitVector.
private partial struct bitvector {
    public int n; // # of bits
    public ptr<byte> bytedata;
}

// ptrbit returns the i'th bit in bv.
// ptrbit is less efficient than iterating directly over bitvector bits,
// and should only be used in non-performance-critical code.
// See adjustpointers for an example of a high-efficiency walk of a bitvector.
private static byte ptrbit(this ptr<bitvector> _addr_bv, System.UIntPtr i) {
    ref bitvector bv = ref _addr_bv.val;

    var b = (addb(bv.bytedata, i / 8)).val;
    return (b >> (int)((i % 8))) & 1;
}

// bv describes the memory starting at address scanp.
// Adjust any pointers contained therein.
private static void adjustpointers(unsafe.Pointer scanp, ptr<bitvector> _addr_bv, ptr<adjustinfo> _addr_adjinfo, funcInfo f) {
    ref bitvector bv = ref _addr_bv.val;
    ref adjustinfo adjinfo = ref _addr_adjinfo.val;

    var minp = adjinfo.old.lo;
    var maxp = adjinfo.old.hi;
    var delta = adjinfo.delta;
    var num = uintptr(bv.n); 
    // If this frame might contain channel receive slots, use CAS
    // to adjust pointers. If the slot hasn't been received into
    // yet, it may contain stack pointers and a concurrent send
    // could race with adjusting those pointers. (The sent value
    // itself can never contain stack pointers.)
    var useCAS = uintptr(scanp) < adjinfo.sghi;
    {
        var i = uintptr(0);

        while (i < num) {
            if (stackDebug >= 4) {
                {
                    var j__prev2 = j;

                    for (var j = uintptr(0); j < 8; j++) {
                        print("        ", add(scanp, (i + j) * sys.PtrSize), ":", ptrnames[bv.ptrbit(i + j)], ":", hex(new ptr<ptr<ptr<System.UIntPtr>>>(add(scanp, (i + j) * sys.PtrSize))), " # ", i, " ", addb(bv.bytedata, i / 8).val, "\n");
                    }


                    j = j__prev2;
                }
            i += 8;
            }
            var b = (addb(bv.bytedata, i / 8)).val;
            while (b != 0) {
                j = uintptr(sys.Ctz8(b));
                b &= b - 1;
                var pp = (uintptr.val)(add(scanp, (i + j) * sys.PtrSize));
retry:
                var p = pp.val;
                if (f.valid() && 0 < p && p < minLegalPointer && debug.invalidptr != 0) { 
                    // Looks like a junk value in a pointer slot.
                    // Live analysis wrong?
                    getg().m.traceback = 2;
                    print("runtime: bad pointer in frame ", funcname(f), " at ", pp, ": ", hex(p), "\n");
                    throw("invalid pointer found on stack");
                }
                if (minp <= p && p < maxp) {
                    if (stackDebug >= 3) {
                        print("adjust ptr ", hex(p), " ", funcname(f), "\n");
                    }
                    if (useCAS) {
                        var ppu = (@unsafe.Pointer.val)(@unsafe.Pointer(pp));
                        if (!atomic.Casp1(ppu, @unsafe.Pointer(p), @unsafe.Pointer(p + delta))) {
                            goto retry;
                        }
                    }
                    else
 {
                        pp.val = p + delta;
                    }
                }
            }
        }
    }
}

// Note: the argument/return area is adjusted by the callee.
private static bool adjustframe(ptr<stkframe> _addr_frame, unsafe.Pointer arg) {
    ref stkframe frame = ref _addr_frame.val;

    var adjinfo = (adjustinfo.val)(arg);
    if (frame.continpc == 0) { 
        // Frame is dead.
        return true;
    }
    var f = frame.fn;
    if (stackDebug >= 2) {
        print("    adjusting ", funcname(f), " frame=[", hex(frame.sp), ",", hex(frame.fp), "] pc=", hex(frame.pc), " continpc=", hex(frame.continpc), "\n");
    }
    if (f.funcID == funcID_systemstack_switch) { 
        // A special routine at the bottom of stack of a goroutine that does a systemstack call.
        // We will allow it to be copied even though we don't
        // have full GC info for it (because it is written in asm).
        return true;
    }
    var (locals, args, objs) = getStackMap(_addr_frame, _addr_adjinfo.cache, true); 

    // Adjust local variables if stack frame has been allocated.
    if (locals.n > 0) {
        var size = uintptr(locals.n) * sys.PtrSize;
        adjustpointers(@unsafe.Pointer(frame.varp - size), _addr_locals, _addr_adjinfo, f);
    }
    if (sys.ArchFamily == sys.AMD64 && frame.argp - frame.varp == 2 * sys.PtrSize) {
        if (stackDebug >= 3) {
            print("      saved bp\n");
        }
        if (debugCheckBP) { 
            // Frame pointers should always point to the next higher frame on
            // the Go stack (or be nil, for the top frame on the stack).
            ptr<ptr<System.UIntPtr>> bp = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(frame.varp));
            if (bp != 0 && (bp < adjinfo.old.lo || bp >= adjinfo.old.hi)) {
                println("runtime: found invalid frame pointer");
                print("bp=", hex(bp), " min=", hex(adjinfo.old.lo), " max=", hex(adjinfo.old.hi), "\n");
                throw("bad frame pointer");
            }
        }
        adjustpointer(_addr_adjinfo, @unsafe.Pointer(frame.varp));
    }
    if (args.n > 0) {
        if (stackDebug >= 3) {
            print("      args\n");
        }
        adjustpointers(@unsafe.Pointer(frame.argp), _addr_args, _addr_adjinfo, new funcInfo());
    }
    if (frame.varp != 0) {
        foreach (var (_, obj) in objs) {
            var off = obj.off;
            var @base = frame.varp; // locals base pointer
            if (off >= 0) {
                base = frame.argp; // arguments and return values base pointer
            }
            var p = base + uintptr(off);
            if (p < frame.sp) { 
                // Object hasn't been allocated in the frame yet.
                // (Happens when the stack bounds check fails and
                // we call into morestack.)
                continue;
            }
            var ptrdata = obj.ptrdata();
            var gcdata = obj.gcdata;
            ptr<mspan> s;
            if (obj.useGCProg()) { 
                // See comments in mgcmark.go:scanstack
                s = materializeGCProg(ptrdata, gcdata);
                gcdata = (byte.val)(@unsafe.Pointer(s.startAddr));
            }
            {
                var i = uintptr(0);

                while (i < ptrdata) {
                    if (addb(gcdata, i / (8 * sys.PtrSize)) >> (int)((i / sys.PtrSize & 7)) & 1 != 0.val) {
                        adjustpointer(_addr_adjinfo, @unsafe.Pointer(p + i));
                    i += sys.PtrSize;
                    }
                }

            }
            if (s != null) {
                dematerializeGCProg(s);
            }
        }
    }
    return true;
}

private static void adjustctxt(ptr<g> _addr_gp, ptr<adjustinfo> _addr_adjinfo) {
    ref g gp = ref _addr_gp.val;
    ref adjustinfo adjinfo = ref _addr_adjinfo.val;

    adjustpointer(_addr_adjinfo, @unsafe.Pointer(_addr_gp.sched.ctxt));
    if (!framepointer_enabled) {
        return ;
    }
    if (debugCheckBP) {
        var bp = gp.sched.bp;
        if (bp != 0 && (bp < adjinfo.old.lo || bp >= adjinfo.old.hi)) {
            println("runtime: found invalid top frame pointer");
            print("bp=", hex(bp), " min=", hex(adjinfo.old.lo), " max=", hex(adjinfo.old.hi), "\n");
            throw("bad top frame pointer");
        }
    }
    adjustpointer(_addr_adjinfo, @unsafe.Pointer(_addr_gp.sched.bp));
}

private static void adjustdefers(ptr<g> _addr_gp, ptr<adjustinfo> _addr_adjinfo) {
    ref g gp = ref _addr_gp.val;
    ref adjustinfo adjinfo = ref _addr_adjinfo.val;
 
    // Adjust pointers in the Defer structs.
    // We need to do this first because we need to adjust the
    // defer.link fields so we always work on the new stack.
    adjustpointer(_addr_adjinfo, @unsafe.Pointer(_addr_gp._defer));
    {
        var d = gp._defer;

        while (d != null) {
            adjustpointer(_addr_adjinfo, @unsafe.Pointer(_addr_d.fn));
            adjustpointer(_addr_adjinfo, @unsafe.Pointer(_addr_d.sp));
            adjustpointer(_addr_adjinfo, @unsafe.Pointer(_addr_d._panic));
            adjustpointer(_addr_adjinfo, @unsafe.Pointer(_addr_d.link));
            adjustpointer(_addr_adjinfo, @unsafe.Pointer(_addr_d.varp));
            adjustpointer(_addr_adjinfo, @unsafe.Pointer(_addr_d.fd));
            d = d.link;
        }
    } 

    // Adjust defer argument blocks the same way we adjust active stack frames.
    // Note: this code is after the loop above, so that if a defer record is
    // stack allocated, we work on the copy in the new stack.
    tracebackdefers(gp, adjustframe, noescape(@unsafe.Pointer(adjinfo)));
}

private static void adjustpanics(ptr<g> _addr_gp, ptr<adjustinfo> _addr_adjinfo) {
    ref g gp = ref _addr_gp.val;
    ref adjustinfo adjinfo = ref _addr_adjinfo.val;
 
    // Panics are on stack and already adjusted.
    // Update pointer to head of list in G.
    adjustpointer(_addr_adjinfo, @unsafe.Pointer(_addr_gp._panic));
}

private static void adjustsudogs(ptr<g> _addr_gp, ptr<adjustinfo> _addr_adjinfo) {
    ref g gp = ref _addr_gp.val;
    ref adjustinfo adjinfo = ref _addr_adjinfo.val;
 
    // the data elements pointed to by a SudoG structure
    // might be in the stack.
    {
        var s = gp.waiting;

        while (s != null) {
            adjustpointer(_addr_adjinfo, @unsafe.Pointer(_addr_s.elem));
            s = s.waitlink;
        }
    }
}

private static void fillstack(stack stk, byte b) {
    for (var p = stk.lo; p < stk.hi; p++) {
        (byte.val).val;

        (@unsafe.Pointer(p)) = b;
    }
}

private static System.UIntPtr findsghi(ptr<g> _addr_gp, stack stk) {
    ref g gp = ref _addr_gp.val;

    System.UIntPtr sghi = default;
    {
        var sg = gp.waiting;

        while (sg != null) {
            var p = uintptr(sg.elem) + uintptr(sg.c.elemsize);
            if (stk.lo <= p && p < stk.hi && p > sghi) {
                sghi = p;
            sg = sg.waitlink;
            }
        }
    }
    return sghi;
}

// syncadjustsudogs adjusts gp's sudogs and copies the part of gp's
// stack they refer to while synchronizing with concurrent channel
// operations. It returns the number of bytes of stack copied.
private static System.UIntPtr syncadjustsudogs(ptr<g> _addr_gp, System.UIntPtr used, ptr<adjustinfo> _addr_adjinfo) {
    ref g gp = ref _addr_gp.val;
    ref adjustinfo adjinfo = ref _addr_adjinfo.val;

    if (gp.waiting == null) {
        return 0;
    }
    ptr<hchan> lastc;
    {
        var sg__prev1 = sg;

        var sg = gp.waiting;

        while (sg != null) {
            if (sg.c != lastc) { 
                // There is a ranking cycle here between gscan bit and
                // hchan locks. Normally, we only allow acquiring hchan
                // locks and then getting a gscan bit. In this case, we
                // already have the gscan bit. We allow acquiring hchan
                // locks here as a special case, since a deadlock can't
                // happen because the G involved must already be
                // suspended. So, we get a special hchan lock rank here
                // that is lower than gscan, but doesn't allow acquiring
                // any other locks other than hchan.
                lockWithRank(_addr_sg.c.@lock, lockRankHchanLeaf);
            sg = sg.waitlink;
            }
            lastc = sg.c;
        }

        sg = sg__prev1;
    } 

    // Adjust sudogs.
    adjustsudogs(_addr_gp, _addr_adjinfo); 

    // Copy the part of the stack the sudogs point in to
    // while holding the lock to prevent races on
    // send/receive slots.
    System.UIntPtr sgsize = default;
    if (adjinfo.sghi != 0) {
        var oldBot = adjinfo.old.hi - used;
        var newBot = oldBot + adjinfo.delta;
        sgsize = adjinfo.sghi - oldBot;
        memmove(@unsafe.Pointer(newBot), @unsafe.Pointer(oldBot), sgsize);
    }
    lastc = null;
    {
        var sg__prev1 = sg;

        sg = gp.waiting;

        while (sg != null) {
            if (sg.c != lastc) {
                unlock(_addr_sg.c.@lock);
            sg = sg.waitlink;
            }
            lastc = sg.c;
        }

        sg = sg__prev1;
    }

    return sgsize;
}

// Copies gp's stack to a new stack of a different size.
// Caller must have changed gp status to Gcopystack.
private static void copystack(ptr<g> _addr_gp, System.UIntPtr newsize) {
    ref g gp = ref _addr_gp.val;

    if (gp.syscallsp != 0) {
        throw("stack growth not allowed in system call");
    }
    var old = gp.stack;
    if (old.lo == 0) {
        throw("nil stackbase");
    }
    var used = old.hi - gp.sched.sp; 

    // allocate new stack
    var @new = stackalloc(uint32(newsize));
    if (stackPoisonCopy != 0) {
        fillstack(new, 0xfd);
    }
    if (stackDebug >= 1) {
        print("copystack gp=", gp, " [", hex(old.lo), " ", hex(old.hi - used), " ", hex(old.hi), "]", " -> [", hex(@new.lo), " ", hex(@new.hi - used), " ", hex(@new.hi), "]/", newsize, "\n");
    }
    ref adjustinfo adjinfo = ref heap(out ptr<adjustinfo> _addr_adjinfo);
    adjinfo.old = old;
    adjinfo.delta = @new.hi - old.hi; 

    // Adjust sudogs, synchronizing with channel ops if necessary.
    var ncopy = used;
    if (!gp.activeStackChans) {
        if (newsize < old.hi - old.lo && atomic.Load8(_addr_gp.parkingOnChan) != 0) { 
            // It's not safe for someone to shrink this stack while we're actively
            // parking on a channel, but it is safe to grow since we do that
            // ourselves and explicitly don't want to synchronize with channels
            // since we could self-deadlock.
            throw("racy sudog adjustment due to parking on channel");
        }
        adjustsudogs(_addr_gp, _addr_adjinfo);
    }
    else
 { 
        // sudogs may be pointing in to the stack and gp has
        // released channel locks, so other goroutines could
        // be writing to gp's stack. Find the highest such
        // pointer so we can handle everything there and below
        // carefully. (This shouldn't be far from the bottom
        // of the stack, so there's little cost in handling
        // everything below it carefully.)
        adjinfo.sghi = findsghi(_addr_gp, old); 

        // Synchronize with channel ops and copy the part of
        // the stack they may interact with.
        ncopy -= syncadjustsudogs(_addr_gp, used, _addr_adjinfo);
    }
    memmove(@unsafe.Pointer(@new.hi - ncopy), @unsafe.Pointer(old.hi - ncopy), ncopy); 

    // Adjust remaining structures that have pointers into stacks.
    // We have to do most of these before we traceback the new
    // stack because gentraceback uses them.
    adjustctxt(_addr_gp, _addr_adjinfo);
    adjustdefers(_addr_gp, _addr_adjinfo);
    adjustpanics(_addr_gp, _addr_adjinfo);
    if (adjinfo.sghi != 0) {
        adjinfo.sghi += adjinfo.delta;
    }
    gp.stack = new;
    gp.stackguard0 = @new.lo + _StackGuard; // NOTE: might clobber a preempt request
    gp.sched.sp = @new.hi - used;
    gp.stktopsp += adjinfo.delta; 

    // Adjust pointers in the new stack.
    gentraceback(~uintptr(0), ~uintptr(0), 0, gp, 0, null, 0x7fffffff, adjustframe, noescape(@unsafe.Pointer(_addr_adjinfo)), 0); 

    // free old stack
    if (stackPoisonCopy != 0) {
        fillstack(old, 0xfc);
    }
    stackfree(old);
}

// round x up to a power of 2.
private static int round2(int x) {
    var s = uint(0);
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
private static void newstack() {
    var thisg = getg(); 
    // TODO: double check all gp. shouldn't be getg().
    if (thisg.m.morebuf.g.ptr().stackguard0 == stackFork) {
        throw("stack growth after fork");
    }
    if (thisg.m.morebuf.g.ptr() != thisg.m.curg) {
        print("runtime: newstack called from g=", hex(thisg.m.morebuf.g), "\n" + "\tm=", thisg.m, " m->curg=", thisg.m.curg, " m->g0=", thisg.m.g0, " m->gsignal=", thisg.m.gsignal, "\n");
        var morebuf = thisg.m.morebuf;
        traceback(morebuf.pc, morebuf.sp, morebuf.lr, morebuf.g.ptr());
        throw("runtime: wrong goroutine in newstack");
    }
    var gp = thisg.m.curg;

    if (thisg.m.curg.throwsplit) { 
        // Update syscallsp, syscallpc in case traceback uses them.
        morebuf = thisg.m.morebuf;
        gp.syscallsp = morebuf.sp;
        gp.syscallpc = morebuf.pc;
        @string pcname = "(unknown)";
        var pcoff = uintptr(0);
        var f = findfunc(gp.sched.pc);
        if (f.valid()) {
            pcname = funcname(f);
            pcoff = gp.sched.pc - f.entry;
        }
        print("runtime: newstack at ", pcname, "+", hex(pcoff), " sp=", hex(gp.sched.sp), " stack=[", hex(gp.stack.lo), ", ", hex(gp.stack.hi), "]\n", "\tmorebuf={pc:", hex(morebuf.pc), " sp:", hex(morebuf.sp), " lr:", hex(morebuf.lr), "}\n", "\tsched={pc:", hex(gp.sched.pc), " sp:", hex(gp.sched.sp), " lr:", hex(gp.sched.lr), " ctxt:", gp.sched.ctxt, "}\n");

        thisg.m.traceback = 2; // Include runtime frames
        traceback(morebuf.pc, morebuf.sp, morebuf.lr, gp);
        throw("runtime: stack split at bad time");
    }
    morebuf = thisg.m.morebuf;
    thisg.m.morebuf.pc = 0;
    thisg.m.morebuf.lr = 0;
    thisg.m.morebuf.sp = 0;
    thisg.m.morebuf.g = 0; 

    // NOTE: stackguard0 may change underfoot, if another thread
    // is about to try to preempt gp. Read it just once and use that same
    // value now and below.
    var preempt = atomic.Loaduintptr(_addr_gp.stackguard0) == stackPreempt; 

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
    if (preempt) {
        if (!canPreemptM(thisg.m)) { 
            // Let the goroutine keep running for now.
            // gp->preempt is set, so it will be preempted next time.
            gp.stackguard0 = gp.stack.lo + _StackGuard;
            gogo(_addr_gp.sched); // never return
        }
    }
    if (gp.stack.lo == 0) {
        throw("missing stack in newstack");
    }
    var sp = gp.sched.sp;
    if (sys.ArchFamily == sys.AMD64 || sys.ArchFamily == sys.I386 || sys.ArchFamily == sys.WASM) { 
        // The call to morestack cost a word.
        sp -= sys.PtrSize;
    }
    if (stackDebug >= 1 || sp < gp.stack.lo) {
        print("runtime: newstack sp=", hex(sp), " stack=[", hex(gp.stack.lo), ", ", hex(gp.stack.hi), "]\n", "\tmorebuf={pc:", hex(morebuf.pc), " sp:", hex(morebuf.sp), " lr:", hex(morebuf.lr), "}\n", "\tsched={pc:", hex(gp.sched.pc), " sp:", hex(gp.sched.sp), " lr:", hex(gp.sched.lr), " ctxt:", gp.sched.ctxt, "}\n");
    }
    if (sp < gp.stack.lo) {
        print("runtime: gp=", gp, ", goid=", gp.goid, ", gp->status=", hex(readgstatus(gp)), "\n ");
        print("runtime: split stack overflow: ", hex(sp), " < ", hex(gp.stack.lo), "\n");
        throw("runtime: split stack overflow");
    }
    if (preempt) {
        if (gp == thisg.m.g0) {
            throw("runtime: preempt g0");
        }
        if (thisg.m.p == 0 && thisg.m.locks == 0) {
            throw("runtime: g is running but p is not");
        }
        if (gp.preemptShrink) { 
            // We're at a synchronous safe point now, so
            // do the pending stack shrink.
            gp.preemptShrink = false;
            shrinkstack(_addr_gp);
        }
        if (gp.preemptStop) {
            preemptPark(gp); // never returns
        }
        gopreempt_m(gp); // never return
    }
    var oldsize = gp.stack.hi - gp.stack.lo;
    var newsize = oldsize * 2; 

    // Make sure we grow at least as much as needed to fit the new frame.
    // (This is just an optimization - the caller of morestack will
    // recheck the bounds on return.)
    {
        var f__prev1 = f;

        f = findfunc(gp.sched.pc);

        if (f.valid()) {
            var max = uintptr(funcMaxSPDelta(f));
            var needed = max + _StackGuard;
            var used = gp.stack.hi - gp.sched.sp;
            while (newsize - used < needed) {
                newsize *= 2;
            }
        }
        f = f__prev1;

    }

    if (gp.stackguard0 == stackForceMove) { 
        // Forced stack movement used for debugging.
        // Don't double the stack (or we may quickly run out
        // if this is done repeatedly).
        newsize = oldsize;
    }
    if (newsize > maxstacksize || newsize > maxstackceiling) {
        if (maxstacksize < maxstackceiling) {
            print("runtime: goroutine stack exceeds ", maxstacksize, "-byte limit\n");
        }
        else
 {
            print("runtime: goroutine stack exceeds ", maxstackceiling, "-byte limit\n");
        }
        print("runtime: sp=", hex(sp), " stack=[", hex(gp.stack.lo), ", ", hex(gp.stack.hi), "]\n");
        throw("stack overflow");
    }
    casgstatus(gp, _Grunning, _Gcopystack); 

    // The concurrent GC will not scan the stack while we are doing the copy since
    // the gp is in a Gcopystack status.
    copystack(_addr_gp, newsize);
    if (stackDebug >= 1) {
        print("stack grow done\n");
    }
    casgstatus(gp, _Gcopystack, _Grunning);
    gogo(_addr_gp.sched);
}

//go:nosplit
private static void nilfunc() {
    (uint8.val).val;

    (null) = 0;
}

// adjust Gobuf as if it executed a call to fn
// and then stopped before the first instruction in fn.
private static void gostartcallfn(ptr<gobuf> _addr_gobuf, ptr<funcval> _addr_fv) {
    ref gobuf gobuf = ref _addr_gobuf.val;
    ref funcval fv = ref _addr_fv.val;

    unsafe.Pointer fn = default;
    if (fv != null) {
        fn = @unsafe.Pointer(fv.fn);
    }
    else
 {
        fn = @unsafe.Pointer(funcPC(nilfunc));
    }
    gostartcall(gobuf, fn, @unsafe.Pointer(fv));
}

// isShrinkStackSafe returns whether it's safe to attempt to shrink
// gp's stack. Shrinking the stack is only safe when we have precise
// pointer maps for all frames on the stack.
private static bool isShrinkStackSafe(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;
 
    // We can't copy the stack if we're in a syscall.
    // The syscall might have pointers into the stack and
    // often we don't have precise pointer maps for the innermost
    // frames.
    //
    // We also can't copy the stack if we're at an asynchronous
    // safe-point because we don't have precise pointer maps for
    // all frames.
    //
    // We also can't *shrink* the stack in the window between the
    // goroutine calling gopark to park on a channel and
    // gp.activeStackChans being set.
    return gp.syscallsp == 0 && !gp.asyncSafePoint && atomic.Load8(_addr_gp.parkingOnChan) == 0;
}

// Maybe shrink the stack being used by gp.
//
// gp must be stopped and we must own its stack. It may be in
// _Grunning, but only if this is our own user G.
private static void shrinkstack(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    if (gp.stack.lo == 0) {
        throw("missing stack in shrinkstack");
    }
    {
        var s = readgstatus(gp);

        if (s & _Gscan == 0) { 
            // We don't own the stack via _Gscan. We could still
            // own it if this is our own user G and we're on the
            // system stack.
            if (!(gp == getg().m.curg && getg() != getg().m.curg && s == _Grunning)) { 
                // We don't own the stack.
                throw("bad status in shrinkstack");
            }
        }
    }
    if (!isShrinkStackSafe(_addr_gp)) {
        throw("shrinkstack at bad time");
    }
    if (gp == getg().m.curg && gp.m.libcallsp != 0) {
        throw("shrinking stack in libcall");
    }
    if (debug.gcshrinkstackoff > 0) {
        return ;
    }
    var f = findfunc(gp.startpc);
    if (f.valid() && f.funcID == funcID_gcBgMarkWorker) { 
        // We're not allowed to shrink the gcBgMarkWorker
        // stack (see gcBgMarkWorker for explanation).
        return ;
    }
    var oldsize = gp.stack.hi - gp.stack.lo;
    var newsize = oldsize / 2; 
    // Don't shrink the allocation below the minimum-sized stack
    // allocation.
    if (newsize < _FixedStack) {
        return ;
    }
    var avail = gp.stack.hi - gp.stack.lo;
    {
        var used = gp.stack.hi - gp.sched.sp + _StackLimit;

        if (used >= avail / 4) {
            return ;
        }
    }

    if (stackDebug > 0) {
        print("shrinking stack ", oldsize, "->", newsize, "\n");
    }
    copystack(_addr_gp, newsize);
}

// freeStackSpans frees unused stack spans at the end of GC.
private static void freeStackSpans() {
    // Scan stack pools for empty stack spans.
    foreach (var (order) in stackpool) {
        lock(_addr_stackpool[order].item.mu);
        var list = _addr_stackpool[order].item.span;
        {
            var s__prev2 = s;

            var s = list.first;

            while (s != null) {
                var next = s.next;
                if (s.allocCount == 0) {
                    list.remove(s);
                    s.manualFreeList = 0;
                    osStackFree(s);
                    mheap_.freeManual(s, spanAllocStack);
                }
                s = next;
            }


            s = s__prev2;
        }
        unlock(_addr_stackpool[order].item.mu);
    }    lock(_addr_stackLarge.@lock);
    foreach (var (i) in stackLarge.free) {
        {
            var s__prev2 = s;

            s = stackLarge.free[i].first;

            while (s != null) {
                next = s.next;
                stackLarge.free[i].remove(s);
                osStackFree(s);
                mheap_.freeManual(s, spanAllocStack);
                s = next;
            }


            s = s__prev2;
        }
    }    unlock(_addr_stackLarge.@lock);
}

// getStackMap returns the locals and arguments live pointer maps, and
// stack object list for frame.
private static (bitvector, bitvector, slice<stackObjectRecord>) getStackMap(ptr<stkframe> _addr_frame, ptr<pcvalueCache> _addr_cache, bool debug) {
    bitvector locals = default;
    bitvector args = default;
    slice<stackObjectRecord> objs = default;
    ref stkframe frame = ref _addr_frame.val;
    ref pcvalueCache cache = ref _addr_cache.val;

    var targetpc = frame.continpc;
    if (targetpc == 0) { 
        // Frame is dead. Return empty bitvectors.
        return ;
    }
    var f = frame.fn;
    var pcdata = int32(-1);
    if (targetpc != f.entry) { 
        // Back up to the CALL. If we're at the function entry
        // point, we want to use the entry map (-1), even if
        // the first instruction of the function changes the
        // stack map.
        targetpc--;
        pcdata = pcdatavalue(f, _PCDATA_StackMapIndex, targetpc, cache);
    }
    if (pcdata == -1) { 
        // We do not have a valid pcdata value but there might be a
        // stackmap for this function. It is likely that we are looking
        // at the function prologue, assume so and hope for the best.
        pcdata = 0;
    }
    var size = frame.varp - frame.sp;
    System.UIntPtr minsize = default;

    if (sys.ArchFamily == sys.ARM64) 
        minsize = sys.StackAlign;
    else 
        minsize = sys.MinFrameSize;
        if (size > minsize) {
        var stackid = pcdata;
        var stkmap = (stackmap.val)(funcdata(f, _FUNCDATA_LocalsPointerMaps));
        if (stkmap == null || stkmap.n <= 0) {
            print("runtime: frame ", funcname(f), " untyped locals ", hex(frame.varp - size), "+", hex(size), "\n");
            throw("missing stackmap");
        }
        if (stkmap.nbit > 0) {
            if (stackid < 0 || stackid >= stkmap.n) { 
                // don't know where we are
                print("runtime: pcdata is ", stackid, " and ", stkmap.n, " locals stack map entries for ", funcname(f), " (targetpc=", hex(targetpc), ")\n");
                throw("bad symbol table");
            }
            locals = stackmapdata(stkmap, stackid);
            if (stackDebug >= 3 && debug) {
                print("      locals ", stackid, "/", stkmap.n, " ", locals.n, " words ", locals.bytedata, "\n");
            }
        }
        else if (stackDebug >= 3 && debug) {
            print("      no locals to adjust\n");
        }
    }
    if (frame.arglen > 0) {
        if (frame.argmap != null) { 
            // argmap is set when the function is reflect.makeFuncStub or reflect.methodValueCall.
            // In this case, arglen specifies how much of the args section is actually live.
            // (It could be either all the args + results, or just the args.)
            args = frame.argmap.val;
            var n = int32(frame.arglen / sys.PtrSize);
            if (n < args.n) {
                args.n = n; // Don't use more of the arguments than arglen.
            }
        }
        else
 {
            var stackmap = (stackmap.val)(funcdata(f, _FUNCDATA_ArgsPointerMaps));
            if (stackmap == null || stackmap.n <= 0) {
                print("runtime: frame ", funcname(f), " untyped args ", hex(frame.argp), "+", hex(frame.arglen), "\n");
                throw("missing stackmap");
            }
            if (pcdata < 0 || pcdata >= stackmap.n) { 
                // don't know where we are
                print("runtime: pcdata is ", pcdata, " and ", stackmap.n, " args stack map entries for ", funcname(f), " (targetpc=", hex(targetpc), ")\n");
                throw("bad symbol table");
            }
            if (stackmap.nbit > 0) {
                args = stackmapdata(stackmap, pcdata);
            }
        }
    }
    if (GOARCH == "amd64" && @unsafe.Sizeof(new abi.RegArgs()) > 0 && frame.argmap != null) { 
        // argmap is set when the function is reflect.makeFuncStub or reflect.methodValueCall.
        // We don't actually use argmap in this case, but we need to fake the stack object
        // record for these frames which contain an internal/abi.RegArgs at a hard-coded offset
        // on amd64.
        objs = methodValueCallFrameObjs;
    }
    else
 {
        var p = funcdata(f, _FUNCDATA_StackObjects);
        if (p != null) {
            n = new ptr<ptr<ptr<System.UIntPtr>>>(p);
            p = add(p, sys.PtrSize) * (slice.val);

            (@unsafe.Pointer(_addr_objs)) = new slice(array:noescape(p),len:int(n),cap:int(n)); 
            // Note: the noescape above is needed to keep
            // getStackMap from "leaking param content:
            // frame".  That leak propagates up to getgcmask, then
            // GCMask, then verifyGCInfo, which converts the stack
            // gcinfo tests into heap gcinfo tests :(
        }
    }
    return ;
}

private static abi.RegArgs abiRegArgsEface = new abi.RegArgs();private static ptr<_type> abiRegArgsTypeefaceOf(_addr_abiRegArgsEface)._type;private static stackObjectRecord methodValueCallFrameObjs = new slice<stackObjectRecord>(new stackObjectRecord[] { {off:-int32(alignUp(abiRegArgsType.size,8)),size:int32(abiRegArgsType.size),_ptrdata:int32(abiRegArgsType.ptrdata),gcdata:abiRegArgsType.gcdata,} });

private static void init() {
    if (abiRegArgsType.kind & kindGCProg != 0) {
        throw("abiRegArgsType needs GC Prog, update methodValueCallFrameObjs");
    }
}

// A stackObjectRecord is generated by the compiler for each stack object in a stack frame.
// This record must match the generator code in cmd/compile/internal/liveness/plive.go:emitStackObjects.
private partial struct stackObjectRecord {
    public int off;
    public int size;
    public int _ptrdata; // ptrdata, or -ptrdata is GC prog is used
    public ptr<byte> gcdata; // pointer map or GC prog of the type
}

private static bool useGCProg(this ptr<stackObjectRecord> _addr_r) {
    ref stackObjectRecord r = ref _addr_r.val;

    return r._ptrdata < 0;
}

private static System.UIntPtr ptrdata(this ptr<stackObjectRecord> _addr_r) {
    ref stackObjectRecord r = ref _addr_r.val;

    var x = r._ptrdata;
    if (x < 0) {
        return uintptr(-x);
    }
    return uintptr(x);
}

// This is exported as ABI0 via linkname so obj can call it.
//
//go:nosplit
//go:linkname morestackc
private static void morestackc() {
    throw("attempt to execute system stack code on user stack");
}

} // end runtime_package
