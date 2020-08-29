// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:57 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\stack.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
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
 
        // StackSystem is a number of additional bytes to add
        // to each stack below the usual guard area for OS-specific
        // purposes like signal handling. Used on Windows, Plan 9,
        // and Darwin/ARM because they do not use a separate stack.
        private static readonly var _StackSystem = sys.GoosWindows * 512L * sys.PtrSize + sys.GoosPlan9 * 512L + sys.GoosDarwin * sys.GoarchArm * 1024L; 

        // The minimum size of stack used by Go code
        private static readonly long _StackMin = 2048L; 

        // The minimum stack size to allocate.
        // The hackery here rounds FixedStack0 up to a power of 2.
        private static readonly var _FixedStack0 = _StackMin + _StackSystem;
        private static readonly var _FixedStack1 = _FixedStack0 - 1L;
        private static readonly var _FixedStack2 = _FixedStack1 | (_FixedStack1 >> (int)(1L));
        private static readonly var _FixedStack3 = _FixedStack2 | (_FixedStack2 >> (int)(2L));
        private static readonly var _FixedStack4 = _FixedStack3 | (_FixedStack3 >> (int)(4L));
        private static readonly var _FixedStack5 = _FixedStack4 | (_FixedStack4 >> (int)(8L));
        private static readonly var _FixedStack6 = _FixedStack5 | (_FixedStack5 >> (int)(16L));
        private static readonly var _FixedStack = _FixedStack6 + 1L; 

        // Functions that need frames bigger than this use an extra
        // instruction to do the stack split check, to avoid overflow
        // in case SP - framesize wraps below zero.
        // This value can be no bigger than the size of the unmapped
        // space at zero.
        private static readonly long _StackBig = 4096L; 

        // The stack guard is a pointer this many bytes above the
        // bottom of the stack.
        private static readonly long _StackGuard = 880L * sys.StackGuardMultiplier + _StackSystem; 

        // After a stack split check the SP is allowed to be this
        // many bytes below the stack guard. This saves an instruction
        // in the checking sequence for tiny frames.
        private static readonly long _StackSmall = 128L; 

        // The maximum number of bytes that a chain of NOSPLIT
        // functions can use.
        private static readonly var _StackLimit = _StackGuard - _StackSystem - _StackSmall;

 
        // stackDebug == 0: no logging
        //            == 1: logging of per-stack operations
        //            == 2: logging of per-frame operations
        //            == 3: logging of per-word updates
        //            == 4: logging of per-word reads
        private static readonly long stackDebug = 0L;
        private static readonly long stackFromSystem = 0L; // allocate stacks from system memory instead of the heap
        private static readonly long stackFaultOnFree = 0L; // old stacks are mapped noaccess to detect use after free
        private static readonly long stackPoisonCopy = 0L; // fill stack that should not be accessed with garbage, to detect bad dereferences during copy
        private static readonly long stackNoCache = 0L; // disable per-P small stack caches

        // check the BP links during traceback.
        private static readonly var debugCheckBP = false;

        private static readonly long uintptrMask = 1L << (int)((8L * sys.PtrSize)) - 1L; 

        // Goroutine preemption request.
        // Stored into g->stackguard0 to cause split stack check failure.
        // Must be greater than any real sp.
        // 0xfffffade in hex.
        private static readonly var stackPreempt = uintptrMask & -1314L; 

        // Thread is forking.
        // Stored into g->stackguard0 to cause split stack check failure.
        // Must be greater than any real sp.
        private static readonly var stackFork = uintptrMask & -1234L;

        // Global pool of spans that have free stacks.
        // Stacks are assigned an order according to size.
        //     order = log_2(size/FixedStack)
        // There is a free list for each order.
        // TODO: one lock per order?
        private static array<mSpanList> stackpool = new array<mSpanList>(_NumStackOrders);
        private static mutex stackpoolmu = default;

        // Global pool of large stack spans.
        private static var stackLarge = default;

        private static void stackinit()
        {
            if (_StackCacheSize & _PageMask != 0L)
            {
                throw("cache size must be a multiple of page size");
            }
            {
                var i__prev1 = i;

                foreach (var (__i) in stackpool)
                {
                    i = __i;
                    stackpool[i].init();
                }

                i = i__prev1;
            }

            {
                var i__prev1 = i;

                foreach (var (__i) in stackLarge.free)
                {
                    i = __i;
                    stackLarge.free[i].init();
                }

                i = i__prev1;
            }

        }

        // stacklog2 returns ⌊log_2(n)⌋.
        private static long stacklog2(System.UIntPtr n)
        {
            long log2 = 0L;
            while (n > 1L)
            {
                n >>= 1L;
                log2++;
            }

            return log2;
        }

        // Allocates a stack from the free pool. Must be called with
        // stackpoolmu held.
        private static gclinkptr stackpoolalloc(byte order)
        {
            var list = ref stackpool[order];
            var s = list.first;
            if (s == null)
            { 
                // no free stacks. Allocate another span worth.
                s = mheap_.allocManual(_StackCacheSize >> (int)(_PageShift), ref memstats.stacks_inuse);
                if (s == null)
                {
                    throw("out of memory");
                }
                if (s.allocCount != 0L)
                {
                    throw("bad allocCount");
                }
                if (s.manualFreeList.ptr() != null)
                {
                    throw("bad manualFreeList");
                }
                s.elemsize = _FixedStack << (int)(order);
                {
                    var i = uintptr(0L);

                    while (i < _StackCacheSize)
                    {
                        var x = gclinkptr(s.@base() + i);
                        x.ptr().next = s.manualFreeList;
                        s.manualFreeList = x;
                        i += s.elemsize;
                    }

                }
                list.insert(s);
            }
            x = s.manualFreeList;
            if (x.ptr() == null)
            {
                throw("span has no free stacks");
            }
            s.manualFreeList = x.ptr().next;
            s.allocCount++;
            if (s.manualFreeList.ptr() == null)
            { 
                // all stacks in s are allocated.
                list.remove(s);
            }
            return x;
        }

        // Adds stack x to the free pool. Must be called with stackpoolmu held.
        private static void stackpoolfree(gclinkptr x, byte order)
        {
            var s = mheap_.lookup(@unsafe.Pointer(x));
            if (s.state != _MSpanManual)
            {
                throw("freeing stack not in a stack span");
            }
            if (s.manualFreeList.ptr() == null)
            { 
                // s will now have a free stack
                stackpool[order].insert(s);
            }
            x.ptr().next = s.manualFreeList;
            s.manualFreeList = x;
            s.allocCount--;
            if (gcphase == _GCoff && s.allocCount == 0L)
            { 
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
                stackpool[order].remove(s);
                s.manualFreeList = 0L;
                mheap_.freeManual(s, ref memstats.stacks_inuse);
            }
        }

        // stackcacherefill/stackcacherelease implement a global pool of stack segments.
        // The pool is required to prevent unlimited growth of per-thread caches.
        //
        //go:systemstack
        private static void stackcacherefill(ref mcache c, byte order)
        {
            if (stackDebug >= 1L)
            {
                print("stackcacherefill order=", order, "\n");
            } 

            // Grab some stacks from the global cache.
            // Grab half of the allowed capacity (to prevent thrashing).
            gclinkptr list = default;
            System.UIntPtr size = default;
            lock(ref stackpoolmu);
            while (size < _StackCacheSize / 2L)
            {
                var x = stackpoolalloc(order);
                x.ptr().next = list;
                list = x;
                size += _FixedStack << (int)(order);
            }

            unlock(ref stackpoolmu);
            c.stackcache[order].list = list;
            c.stackcache[order].size = size;
        }

        //go:systemstack
        private static void stackcacherelease(ref mcache c, byte order)
        {
            if (stackDebug >= 1L)
            {
                print("stackcacherelease order=", order, "\n");
            }
            var x = c.stackcache[order].list;
            var size = c.stackcache[order].size;
            lock(ref stackpoolmu);
            while (size > _StackCacheSize / 2L)
            {
                var y = x.ptr().next;
                stackpoolfree(x, order);
                x = y;
                size -= _FixedStack << (int)(order);
            }

            unlock(ref stackpoolmu);
            c.stackcache[order].list = x;
            c.stackcache[order].size = size;
        }

        //go:systemstack
        private static void stackcache_clear(ref mcache c)
        {
            if (stackDebug >= 1L)
            {
                print("stackcache clear\n");
            }
            lock(ref stackpoolmu);
            for (var order = uint8(0L); order < _NumStackOrders; order++)
            {
                var x = c.stackcache[order].list;
                while (x.ptr() != null)
                {
                    var y = x.ptr().next;
                    stackpoolfree(x, order);
                    x = y;
                }

                c.stackcache[order].list = 0L;
                c.stackcache[order].size = 0L;
            }

            unlock(ref stackpoolmu);
        }

        // stackalloc allocates an n byte stack.
        //
        // stackalloc must run on the system stack because it uses per-P
        // resources and must not split the stack.
        //
        //go:systemstack
        private static stack @stackalloc(uint n)
        { 
            // Stackalloc must be called on scheduler stack, so that we
            // never try to grow the stack during the code that stackalloc runs.
            // Doing so would cause a deadlock (issue 1547).
            var thisg = getg();
            if (thisg != thisg.m.g0)
            {
                throw("stackalloc not on scheduler stack");
            }
            if (n & (n - 1L) != 0L)
            {
                throw("stack size not a power of 2");
            }
            if (stackDebug >= 1L)
            {
                print("stackalloc ", n, "\n");
            }
            if (debug.efence != 0L || stackFromSystem != 0L)
            {
                n = uint32(round(uintptr(n), physPageSize));
                var v = sysAlloc(uintptr(n), ref memstats.stacks_sys);
                if (v == null)
                {
                    throw("out of memory (stackalloc)");
                }
                return new stack(uintptr(v),uintptr(v)+uintptr(n));
            } 

            // Small stacks are allocated with a fixed-size free-list allocator.
            // If we need a stack of a bigger size, we fall back on allocating
            // a dedicated span.
            v = default;
            if (n < _FixedStack << (int)(_NumStackOrders) && n < _StackCacheSize)
            {
                var order = uint8(0L);
                var n2 = n;
                while (n2 > _FixedStack)
                {
                    order++;
                    n2 >>= 1L;
                }
            else

                gclinkptr x = default;
                var c = thisg.m.mcache;
                if (stackNoCache != 0L || c == null || thisg.m.preemptoff != "" || thisg.m.helpgc != 0L)
                { 
                    // c == nil can happen in the guts of exitsyscall or
                    // procresize. Just get a stack from the global pool.
                    // Also don't touch stackcache during gc
                    // as it's flushed concurrently.
                    lock(ref stackpoolmu);
                    x = stackpoolalloc(order);
                    unlock(ref stackpoolmu);
                }
                else
                {
                    x = c.stackcache[order].list;
                    if (x.ptr() == null)
                    {
                        stackcacherefill(c, order);
                        x = c.stackcache[order].list;
                    }
                    c.stackcache[order].list = x.ptr().next;
                    c.stackcache[order].size -= uintptr(n);
                }
                v = @unsafe.Pointer(x);
            }            {
                ref mspan s = default;
                var npage = uintptr(n) >> (int)(_PageShift);
                var log2npage = stacklog2(npage); 

                // Try to get a stack from the large stack cache.
                lock(ref stackLarge.@lock);
                if (!stackLarge.free[log2npage].isEmpty())
                {
                    s = stackLarge.free[log2npage].first;
                    stackLarge.free[log2npage].remove(s);
                }
                unlock(ref stackLarge.@lock);

                if (s == null)
                { 
                    // Allocate a new stack from the heap.
                    s = mheap_.allocManual(npage, ref memstats.stacks_inuse);
                    if (s == null)
                    {
                        throw("out of memory");
                    }
                    s.elemsize = uintptr(n);
                }
                v = @unsafe.Pointer(s.@base());
            }
            if (raceenabled)
            {
                racemalloc(v, uintptr(n));
            }
            if (msanenabled)
            {
                msanmalloc(v, uintptr(n));
            }
            if (stackDebug >= 1L)
            {
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
        private static void stackfree(stack stk)
        {
            var gp = getg();
            var v = @unsafe.Pointer(stk.lo);
            var n = stk.hi - stk.lo;
            if (n & (n - 1L) != 0L)
            {
                throw("stack not a power of 2");
            }
            if (stk.lo + n < stk.hi)
            {
                throw("bad stack size");
            }
            if (stackDebug >= 1L)
            {
                println("stackfree", v, n);
                memclrNoHeapPointers(v, n); // for testing, clobber stack data
            }
            if (debug.efence != 0L || stackFromSystem != 0L)
            {
                if (debug.efence != 0L || stackFaultOnFree != 0L)
                {
                    sysFault(v, n);
                }
                else
                {
                    sysFree(v, n, ref memstats.stacks_sys);
                }
                return;
            }
            if (msanenabled)
            {
                msanfree(v, n);
            }
            if (n < _FixedStack << (int)(_NumStackOrders) && n < _StackCacheSize)
            {
                var order = uint8(0L);
                var n2 = n;
                while (n2 > _FixedStack)
                {
                    order++;
                    n2 >>= 1L;
                }
            else

                var x = gclinkptr(v);
                var c = gp.m.mcache;
                if (stackNoCache != 0L || c == null || gp.m.preemptoff != "" || gp.m.helpgc != 0L)
                {
                    lock(ref stackpoolmu);
                    stackpoolfree(x, order);
                    unlock(ref stackpoolmu);
                }
                else
                {
                    if (c.stackcache[order].size >= _StackCacheSize)
                    {
                        stackcacherelease(c, order);
                    }
                    x.ptr().next = c.stackcache[order].list;
                    c.stackcache[order].list = x;
                    c.stackcache[order].size += n;
                }
            }            {
                var s = mheap_.lookup(v);
                if (s.state != _MSpanManual)
                {
                    println(hex(s.@base()), v);
                    throw("bad span state");
                }
                if (gcphase == _GCoff)
                { 
                    // Free the stack immediately if we're
                    // sweeping.
                    mheap_.freeManual(s, ref memstats.stacks_inuse);
                }
                else
                { 
                    // If the GC is running, we can't return a
                    // stack span to the heap because it could be
                    // reused as a heap span, and this state
                    // change would race with GC. Add it to the
                    // large stack cache instead.
                    var log2npage = stacklog2(s.npages);
                    lock(ref stackLarge.@lock);
                    stackLarge.free[log2npage].insert(s);
                    unlock(ref stackLarge.@lock);
                }
            }
        }

        private static System.UIntPtr maxstacksize = 1L << (int)(20L); // enough until runtime.main sets it for real

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

        private partial struct adjustinfo
        {
            public stack old;
            public System.UIntPtr delta; // ptr distance from old to new stack (newbase - oldbase)
            public pcvalueCache cache; // sghi is the highest sudog.elem on the stack.
            public System.UIntPtr sghi;
        }

        // Adjustpointer checks whether *vpp is in the old stack described by adjinfo.
        // If so, it rewrites *vpp to point into the new stack.
        private static void adjustpointer(ref adjustinfo adjinfo, unsafe.Pointer vpp)
        {
            var pp = (uintptr.Value)(vpp);
            var p = pp.Value;
            if (stackDebug >= 4L)
            {
                print("        ", pp, ":", hex(p), "\n");
            }
            if (adjinfo.old.lo <= p && p < adjinfo.old.hi)
            {
                pp.Value = p + adjinfo.delta;
                if (stackDebug >= 3L)
                {
                    print("        adjust ptr ", pp, ":", hex(p), " -> ", hex(pp.Value), "\n");
                }
            }
        }

        // Information from the compiler about the layout of stack frames.
        private partial struct bitvector
        {
            public int n; // # of bits
            public ptr<byte> bytedata;
        }

        private partial struct gobitvector
        {
            public System.UIntPtr n;
            public slice<byte> bytedata;
        }

        private static gobitvector gobv(bitvector bv)
        {
            return new gobitvector(uintptr(bv.n),(*[1<<30]byte)(unsafe.Pointer(bv.bytedata))[:(bv.n+7)/8],);
        }

        private static byte ptrbit(ref gobitvector bv, System.UIntPtr i)
        {
            return (bv.bytedata[i / 8L] >> (int)((i % 8L))) & 1L;
        }

        // bv describes the memory starting at address scanp.
        // Adjust any pointers contained therein.
        private static void adjustpointers(unsafe.Pointer scanp, ref bitvector cbv, ref adjustinfo adjinfo, funcInfo f)
        {
            var bv = gobv(cbv.Value);
            var minp = adjinfo.old.lo;
            var maxp = adjinfo.old.hi;
            var delta = adjinfo.delta;
            var num = bv.n; 
            // If this frame might contain channel receive slots, use CAS
            // to adjust pointers. If the slot hasn't been received into
            // yet, it may contain stack pointers and a concurrent send
            // could race with adjusting those pointers. (The sent value
            // itself can never contain stack pointers.)
            var useCAS = uintptr(scanp) < adjinfo.sghi;
            for (var i = uintptr(0L); i < num; i++)
            {
                if (stackDebug >= 4L)
                {
                    print("        ", add(scanp, i * sys.PtrSize), ":", ptrnames[ptrbit(ref bv, i)], ":", hex(add(scanp, i * sys.PtrSize).Value), " # ", i, " ", bv.bytedata[i / 8L], "\n");
                }
                if (ptrbit(ref bv, i) != 1L)
                {
                    continue;
                }
                var pp = (uintptr.Value)(add(scanp, i * sys.PtrSize));
retry:
                var p = pp.Value;
                if (f.valid() && 0L < p && p < minLegalPointer && debug.invalidptr != 0L)
                { 
                    // Looks like a junk value in a pointer slot.
                    // Live analysis wrong?
                    getg().m.traceback = 2L;
                    print("runtime: bad pointer in frame ", funcname(f), " at ", pp, ": ", hex(p), "\n");
                    throw("invalid pointer found on stack");
                }
                if (minp <= p && p < maxp)
                {
                    if (stackDebug >= 3L)
                    {
                        print("adjust ptr ", hex(p), " ", funcname(f), "\n");
                    }
                    if (useCAS)
                    {
                        var ppu = (@unsafe.Pointer.Value)(@unsafe.Pointer(pp));
                        if (!atomic.Casp1(ppu, @unsafe.Pointer(p), @unsafe.Pointer(p + delta)))
                        {
                            goto retry;
                        }
                    }
                    else
                    {
                        pp.Value = p + delta;
                    }
                }
            }

        }

        // Note: the argument/return area is adjusted by the callee.
        private static bool adjustframe(ref stkframe frame, unsafe.Pointer arg)
        {
            var adjinfo = (adjustinfo.Value)(arg);
            var targetpc = frame.continpc;
            if (targetpc == 0L)
            { 
                // Frame is dead.
                return true;
            }
            var f = frame.fn;
            if (stackDebug >= 2L)
            {
                print("    adjusting ", funcname(f), " frame=[", hex(frame.sp), ",", hex(frame.fp), "] pc=", hex(frame.pc), " continpc=", hex(frame.continpc), "\n");
            }
            if (f.funcID == funcID_systemstack_switch)
            { 
                // A special routine at the bottom of stack of a goroutine that does an systemstack call.
                // We will allow it to be copied even though we don't
                // have full GC info for it (because it is written in asm).
                return true;
            }
            if (targetpc != f.entry)
            {
                targetpc--;
            }
            var pcdata = pcdatavalue(f, _PCDATA_StackMapIndex, targetpc, ref adjinfo.cache);
            if (pcdata == -1L)
            {
                pcdata = 0L; // in prologue
            } 

            // Adjust local variables if stack frame has been allocated.
            var size = frame.varp - frame.sp;
            System.UIntPtr minsize = default;

            if (sys.ArchFamily == sys.ARM64) 
                minsize = sys.SpAlign;
            else 
                minsize = sys.MinFrameSize;
                        if (size > minsize)
            {
                bitvector bv = default;
                var stackmap = (stackmap.Value)(funcdata(f, _FUNCDATA_LocalsPointerMaps));
                if (stackmap == null || stackmap.n <= 0L)
                {
                    print("runtime: frame ", funcname(f), " untyped locals ", hex(frame.varp - size), "+", hex(size), "\n");
                    throw("missing stackmap");
                } 
                // Locals bitmap information, scan just the pointers in locals.
                if (pcdata < 0L || pcdata >= stackmap.n)
                { 
                    // don't know where we are
                    print("runtime: pcdata is ", pcdata, " and ", stackmap.n, " locals stack map entries for ", funcname(f), " (targetpc=", targetpc, ")\n");
                    throw("bad symbol table");
                }
                bv = stackmapdata(stackmap, pcdata);
                size = uintptr(bv.n) * sys.PtrSize;
                if (stackDebug >= 3L)
                {
                    print("      locals ", pcdata, "/", stackmap.n, " ", size / sys.PtrSize, " words ", bv.bytedata, "\n");
                }
                adjustpointers(@unsafe.Pointer(frame.varp - size), ref bv, adjinfo, f);
            } 

            // Adjust saved base pointer if there is one.
            if (sys.ArchFamily == sys.AMD64 && frame.argp - frame.varp == 2L * sys.RegSize)
            {
                if (!framepointer_enabled)
                {
                    print("runtime: found space for saved base pointer, but no framepointer experiment\n");
                    print("argp=", hex(frame.argp), " varp=", hex(frame.varp), "\n");
                    throw("bad frame layout");
                }
                if (stackDebug >= 3L)
                {
                    print("      saved bp\n");
                }
                if (debugCheckBP)
                { 
                    // Frame pointers should always point to the next higher frame on
                    // the Go stack (or be nil, for the top frame on the stack).
                    *(*System.UIntPtr) bp = @unsafe.Pointer(frame.varp).Value;
                    if (bp != 0L && (bp < adjinfo.old.lo || bp >= adjinfo.old.hi))
                    {
                        println("runtime: found invalid frame pointer");
                        print("bp=", hex(bp), " min=", hex(adjinfo.old.lo), " max=", hex(adjinfo.old.hi), "\n");
                        throw("bad frame pointer");
                    }
                }
                adjustpointer(adjinfo, @unsafe.Pointer(frame.varp));
            } 

            // Adjust arguments.
            if (frame.arglen > 0L)
            {
                bv = default;
                if (frame.argmap != null)
                {
                    bv = frame.argmap.Value;
                }
                else
                {
                    stackmap = (stackmap.Value)(funcdata(f, _FUNCDATA_ArgsPointerMaps));
                    if (stackmap == null || stackmap.n <= 0L)
                    {
                        print("runtime: frame ", funcname(f), " untyped args ", frame.argp, "+", frame.arglen, "\n");
                        throw("missing stackmap");
                    }
                    if (pcdata < 0L || pcdata >= stackmap.n)
                    { 
                        // don't know where we are
                        print("runtime: pcdata is ", pcdata, " and ", stackmap.n, " args stack map entries for ", funcname(f), " (targetpc=", targetpc, ")\n");
                        throw("bad symbol table");
                    }
                    bv = stackmapdata(stackmap, pcdata);
                }
                if (stackDebug >= 3L)
                {
                    print("      args\n");
                }
                adjustpointers(@unsafe.Pointer(frame.argp), ref bv, adjinfo, new funcInfo());
            }
            return true;
        }

        private static void adjustctxt(ref g gp, ref adjustinfo adjinfo)
        {
            adjustpointer(adjinfo, @unsafe.Pointer(ref gp.sched.ctxt));
            if (!framepointer_enabled)
            {
                return;
            }
            if (debugCheckBP)
            {
                var bp = gp.sched.bp;
                if (bp != 0L && (bp < adjinfo.old.lo || bp >= adjinfo.old.hi))
                {
                    println("runtime: found invalid top frame pointer");
                    print("bp=", hex(bp), " min=", hex(adjinfo.old.lo), " max=", hex(adjinfo.old.hi), "\n");
                    throw("bad top frame pointer");
                }
            }
            adjustpointer(adjinfo, @unsafe.Pointer(ref gp.sched.bp));
        }

        private static void adjustdefers(ref g gp, ref adjustinfo adjinfo)
        { 
            // Adjust defer argument blocks the same way we adjust active stack frames.
            tracebackdefers(gp, adjustframe, noescape(@unsafe.Pointer(adjinfo))); 

            // Adjust pointers in the Defer structs.
            // Defer structs themselves are never on the stack.
            {
                var d = gp._defer;

                while (d != null)
                {
                    adjustpointer(adjinfo, @unsafe.Pointer(ref d.fn));
                    adjustpointer(adjinfo, @unsafe.Pointer(ref d.sp));
                    adjustpointer(adjinfo, @unsafe.Pointer(ref d._panic));
                    d = d.link;
                }

            }
        }

        private static void adjustpanics(ref g gp, ref adjustinfo adjinfo)
        { 
            // Panics are on stack and already adjusted.
            // Update pointer to head of list in G.
            adjustpointer(adjinfo, @unsafe.Pointer(ref gp._panic));
        }

        private static void adjustsudogs(ref g gp, ref adjustinfo adjinfo)
        { 
            // the data elements pointed to by a SudoG structure
            // might be in the stack.
            {
                var s = gp.waiting;

                while (s != null)
                {
                    adjustpointer(adjinfo, @unsafe.Pointer(ref s.elem));
                    s = s.waitlink;
                }

            }
        }

        private static void fillstack(stack stk, byte b)
        {
            for (var p = stk.lo; p < stk.hi; p++)
            {
                (byte.Value)(@unsafe.Pointer(p)).Value;

                b;
            }

        }

        private static System.UIntPtr findsghi(ref g gp, stack stk)
        {
            System.UIntPtr sghi = default;
            {
                var sg = gp.waiting;

                while (sg != null)
                {
                    var p = uintptr(sg.elem) + uintptr(sg.c.elemsize);
                    if (stk.lo <= p && p < stk.hi && p > sghi)
                    {
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
        private static System.UIntPtr syncadjustsudogs(ref g gp, System.UIntPtr used, ref adjustinfo adjinfo)
        {
            if (gp.waiting == null)
            {
                return 0L;
            } 

            // Lock channels to prevent concurrent send/receive.
            // It's important that we *only* do this for async
            // copystack; otherwise, gp may be in the middle of
            // putting itself on wait queues and this would
            // self-deadlock.
            ref hchan lastc = default;
            {
                var sg__prev1 = sg;

                var sg = gp.waiting;

                while (sg != null)
                {
                    if (sg.c != lastc)
                    {
                        lock(ref sg.c.@lock);
                    sg = sg.waitlink;
                    }
                    lastc = sg.c;
                } 

                // Adjust sudogs.


                sg = sg__prev1;
            } 

            // Adjust sudogs.
            adjustsudogs(gp, adjinfo); 

            // Copy the part of the stack the sudogs point in to
            // while holding the lock to prevent races on
            // send/receive slots.
            System.UIntPtr sgsize = default;
            if (adjinfo.sghi != 0L)
            {
                var oldBot = adjinfo.old.hi - used;
                var newBot = oldBot + adjinfo.delta;
                sgsize = adjinfo.sghi - oldBot;
                memmove(@unsafe.Pointer(newBot), @unsafe.Pointer(oldBot), sgsize);
            } 

            // Unlock channels.
            lastc = null;
            {
                var sg__prev1 = sg;

                sg = gp.waiting;

                while (sg != null)
                {
                    if (sg.c != lastc)
                    {
                        unlock(ref sg.c.@lock);
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
        //
        // If sync is true, this is a self-triggered stack growth and, in
        // particular, no other G may be writing to gp's stack (e.g., via a
        // channel operation). If sync is false, copystack protects against
        // concurrent channel operations.
        private static void copystack(ref g gp, System.UIntPtr newsize, bool sync)
        {
            if (gp.syscallsp != 0L)
            {
                throw("stack growth not allowed in system call");
            }
            var old = gp.stack;
            if (old.lo == 0L)
            {
                throw("nil stackbase");
            }
            var used = old.hi - gp.sched.sp; 

            // allocate new stack
            var @new = stackalloc(uint32(newsize));
            if (stackPoisonCopy != 0L)
            {
                fillstack(new, 0xfdUL);
            }
            if (stackDebug >= 1L)
            {
                print("copystack gp=", gp, " [", hex(old.lo), " ", hex(old.hi - used), " ", hex(old.hi), "]", " -> [", hex(@new.lo), " ", hex(@new.hi - used), " ", hex(@new.hi), "]/", newsize, "\n");
            } 

            // Compute adjustment.
            adjustinfo adjinfo = default;
            adjinfo.old = old;
            adjinfo.delta = @new.hi - old.hi; 

            // Adjust sudogs, synchronizing with channel ops if necessary.
            var ncopy = used;
            if (sync)
            {
                adjustsudogs(gp, ref adjinfo);
            }
            else
            { 
                // sudogs can point in to the stack. During concurrent
                // shrinking, these areas may be written to. Find the
                // highest such pointer so we can handle everything
                // there and below carefully. (This shouldn't be far
                // from the bottom of the stack, so there's little
                // cost in handling everything below it carefully.)
                adjinfo.sghi = findsghi(gp, old); 

                // Synchronize with channel ops and copy the part of
                // the stack they may interact with.
                ncopy -= syncadjustsudogs(gp, used, ref adjinfo);
            } 

            // Copy the stack (or the rest of it) to the new location
            memmove(@unsafe.Pointer(@new.hi - ncopy), @unsafe.Pointer(old.hi - ncopy), ncopy); 

            // Adjust remaining structures that have pointers into stacks.
            // We have to do most of these before we traceback the new
            // stack because gentraceback uses them.
            adjustctxt(gp, ref adjinfo);
            adjustdefers(gp, ref adjinfo);
            adjustpanics(gp, ref adjinfo);
            if (adjinfo.sghi != 0L)
            {
                adjinfo.sghi += adjinfo.delta;
            } 

            // Swap out old stack for new one
            gp.stack = new;
            gp.stackguard0 = @new.lo + _StackGuard; // NOTE: might clobber a preempt request
            gp.sched.sp = @new.hi - used;
            gp.stktopsp += adjinfo.delta; 

            // Adjust pointers in the new stack.
            gentraceback(~uintptr(0L), ~uintptr(0L), 0L, gp, 0L, null, 0x7fffffffUL, adjustframe, noescape(@unsafe.Pointer(ref adjinfo)), 0L); 

            // free old stack
            if (stackPoisonCopy != 0L)
            {
                fillstack(old, 0xfcUL);
            }
            stackfree(old);
        }

        // round x up to a power of 2.
        private static int round2(int x)
        {
            var s = uint(0L);
            while (1L << (int)(s) < x)
            {
                s++;
            }

            return 1L << (int)(s);
        }

        // Called from runtime·morestack when more stack is needed.
        // Allocate larger stack and relocate to new stack.
        // Stack growth is multiplicative, for constant amortized cost.
        //
        // g->atomicstatus will be Grunning or Gscanrunning upon entry.
        // If the GC is trying to stop this g then it will set preemptscan to true.
        //
        // This must be nowritebarrierrec because it can be called as part of
        // stack growth from other nowritebarrierrec functions, but the
        // compiler doesn't check this.
        //
        //go:nowritebarrierrec
        private static void newstack()
        {
            var thisg = getg(); 
            // TODO: double check all gp. shouldn't be getg().
            if (thisg.m.morebuf.g.ptr().stackguard0 == stackFork)
            {
                throw("stack growth after fork");
            }
            if (thisg.m.morebuf.g.ptr() != thisg.m.curg)
            {
                print("runtime: newstack called from g=", hex(thisg.m.morebuf.g), "\n" + "\tm=", thisg.m, " m->curg=", thisg.m.curg, " m->g0=", thisg.m.g0, " m->gsignal=", thisg.m.gsignal, "\n");
                var morebuf = thisg.m.morebuf;
                traceback(morebuf.pc, morebuf.sp, morebuf.lr, morebuf.g.ptr());
                throw("runtime: wrong goroutine in newstack");
            }
            var gp = thisg.m.curg;

            if (thisg.m.curg.throwsplit)
            { 
                // Update syscallsp, syscallpc in case traceback uses them.
                morebuf = thisg.m.morebuf;
                gp.syscallsp = morebuf.sp;
                gp.syscallpc = morebuf.pc;
                @string pcname = "(unknown)";
                var pcoff = uintptr(0L);
                var f = findfunc(gp.sched.pc);
                if (f.valid())
                {
                    pcname = funcname(f);
                    pcoff = gp.sched.pc - f.entry;
                }
                print("runtime: newstack at ", pcname, "+", hex(pcoff), " sp=", hex(gp.sched.sp), " stack=[", hex(gp.stack.lo), ", ", hex(gp.stack.hi), "]\n", "\tmorebuf={pc:", hex(morebuf.pc), " sp:", hex(morebuf.sp), " lr:", hex(morebuf.lr), "}\n", "\tsched={pc:", hex(gp.sched.pc), " sp:", hex(gp.sched.sp), " lr:", hex(gp.sched.lr), " ctxt:", gp.sched.ctxt, "}\n");

                thisg.m.traceback = 2L; // Include runtime frames
                traceback(morebuf.pc, morebuf.sp, morebuf.lr, gp);
                throw("runtime: stack split at bad time");
            }
            morebuf = thisg.m.morebuf;
            thisg.m.morebuf.pc = 0L;
            thisg.m.morebuf.lr = 0L;
            thisg.m.morebuf.sp = 0L;
            thisg.m.morebuf.g = 0L; 

            // NOTE: stackguard0 may change underfoot, if another thread
            // is about to try to preempt gp. Read it just once and use that same
            // value now and below.
            var preempt = atomic.Loaduintptr(ref gp.stackguard0) == stackPreempt; 

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
            if (preempt)
            {
                if (thisg.m.locks != 0L || thisg.m.mallocing != 0L || thisg.m.preemptoff != "" || thisg.m.p.ptr().status != _Prunning)
                { 
                    // Let the goroutine keep running for now.
                    // gp->preempt is set, so it will be preempted next time.
                    gp.stackguard0 = gp.stack.lo + _StackGuard;
                    gogo(ref gp.sched); // never return
                }
            }
            if (gp.stack.lo == 0L)
            {
                throw("missing stack in newstack");
            }
            var sp = gp.sched.sp;
            if (sys.ArchFamily == sys.AMD64 || sys.ArchFamily == sys.I386)
            { 
                // The call to morestack cost a word.
                sp -= sys.PtrSize;
            }
            if (stackDebug >= 1L || sp < gp.stack.lo)
            {
                print("runtime: newstack sp=", hex(sp), " stack=[", hex(gp.stack.lo), ", ", hex(gp.stack.hi), "]\n", "\tmorebuf={pc:", hex(morebuf.pc), " sp:", hex(morebuf.sp), " lr:", hex(morebuf.lr), "}\n", "\tsched={pc:", hex(gp.sched.pc), " sp:", hex(gp.sched.sp), " lr:", hex(gp.sched.lr), " ctxt:", gp.sched.ctxt, "}\n");
            }
            if (sp < gp.stack.lo)
            {
                print("runtime: gp=", gp, ", gp->status=", hex(readgstatus(gp)), "\n ");
                print("runtime: split stack overflow: ", hex(sp), " < ", hex(gp.stack.lo), "\n");
                throw("runtime: split stack overflow");
            }
            if (preempt)
            {
                if (gp == thisg.m.g0)
                {
                    throw("runtime: preempt g0");
                }
                if (thisg.m.p == 0L && thisg.m.locks == 0L)
                {
                    throw("runtime: g is running but p is not");
                } 
                // Synchronize with scang.
                casgstatus(gp, _Grunning, _Gwaiting);
                if (gp.preemptscan)
                {
                    while (!castogscanstatus(gp, _Gwaiting, _Gscanwaiting))
                    { 
                        // Likely to be racing with the GC as
                        // it sees a _Gwaiting and does the
                        // stack scan. If so, gcworkdone will
                        // be set and gcphasework will simply
                        // return.
                    }

                    if (!gp.gcscandone)
                    { 
                        // gcw is safe because we're on the
                        // system stack.
                        var gcw = ref gp.m.p.ptr().gcw;
                        scanstack(gp, gcw);
                        if (gcBlackenPromptly)
                        {
                            gcw.dispose();
                        }
                        gp.gcscandone = true;
                    }
                    gp.preemptscan = false;
                    gp.preempt = false;
                    casfrom_Gscanstatus(gp, _Gscanwaiting, _Gwaiting); 
                    // This clears gcscanvalid.
                    casgstatus(gp, _Gwaiting, _Grunning);
                    gp.stackguard0 = gp.stack.lo + _StackGuard;
                    gogo(ref gp.sched); // never return
                } 

                // Act like goroutine called runtime.Gosched.
                casgstatus(gp, _Gwaiting, _Grunning);
                gopreempt_m(gp); // never return
            } 

            // Allocate a bigger segment and move the stack.
            var oldsize = gp.stack.hi - gp.stack.lo;
            var newsize = oldsize * 2L;
            if (newsize > maxstacksize)
            {
                print("runtime: goroutine stack exceeds ", maxstacksize, "-byte limit\n");
                throw("stack overflow");
            } 

            // The goroutine must be executing in order to call newstack,
            // so it must be Grunning (or Gscanrunning).
            casgstatus(gp, _Grunning, _Gcopystack); 

            // The concurrent GC will not scan the stack while we are doing the copy since
            // the gp is in a Gcopystack status.
            copystack(gp, newsize, true);
            if (stackDebug >= 1L)
            {
                print("stack grow done\n");
            }
            casgstatus(gp, _Gcopystack, _Grunning);
            gogo(ref gp.sched);
        }

        //go:nosplit
        private static void nilfunc()
        {
            (uint8.Value)(null).Value;

            0L;
        }

        // adjust Gobuf as if it executed a call to fn
        // and then did an immediate gosave.
        private static void gostartcallfn(ref gobuf gobuf, ref funcval fv)
        {
            unsafe.Pointer fn = default;
            if (fv != null)
            {
                fn = @unsafe.Pointer(fv.fn);
            }
            else
            {
                fn = @unsafe.Pointer(funcPC(nilfunc));
            }
            gostartcall(gobuf, fn, @unsafe.Pointer(fv));
        }

        // Maybe shrink the stack being used by gp.
        // Called at garbage collection time.
        // gp must be stopped, but the world need not be.
        private static void shrinkstack(ref g gp)
        {
            var gstatus = readgstatus(gp);
            if (gstatus & ~_Gscan == _Gdead)
            {
                if (gp.stack.lo != 0L)
                { 
                    // Free whole stack - it will get reallocated
                    // if G is used again.
                    stackfree(gp.stack);
                    gp.stack.lo = 0L;
                    gp.stack.hi = 0L;
                }
                return;
            }
            if (gp.stack.lo == 0L)
            {
                throw("missing stack in shrinkstack");
            }
            if (gstatus & _Gscan == 0L)
            {
                throw("bad status in shrinkstack");
            }
            if (debug.gcshrinkstackoff > 0L)
            {
                return;
            }
            var f = findfunc(gp.startpc);
            if (f.valid() && f.funcID == funcID_gcBgMarkWorker)
            { 
                // We're not allowed to shrink the gcBgMarkWorker
                // stack (see gcBgMarkWorker for explanation).
                return;
            }
            var oldsize = gp.stack.hi - gp.stack.lo;
            var newsize = oldsize / 2L; 
            // Don't shrink the allocation below the minimum-sized stack
            // allocation.
            if (newsize < _FixedStack)
            {
                return;
            } 
            // Compute how much of the stack is currently in use and only
            // shrink the stack if gp is using less than a quarter of its
            // current stack. The currently used stack includes everything
            // down to the SP plus the stack guard space that ensures
            // there's room for nosplit functions.
            var avail = gp.stack.hi - gp.stack.lo;
            {
                var used = gp.stack.hi - gp.sched.sp + _StackLimit;

                if (used >= avail / 4L)
                {
                    return;
                } 

                // We can't copy the stack if we're in a syscall.
                // The syscall might have pointers into the stack.

            } 

            // We can't copy the stack if we're in a syscall.
            // The syscall might have pointers into the stack.
            if (gp.syscallsp != 0L)
            {
                return;
            }
            if (sys.GoosWindows != 0L && gp.m != null && gp.m.libcallsp != 0L)
            {
                return;
            }
            if (stackDebug > 0L)
            {
                print("shrinking stack ", oldsize, "->", newsize, "\n");
            }
            copystack(gp, newsize, false);
        }

        // freeStackSpans frees unused stack spans at the end of GC.
        private static void freeStackSpans()
        {
            lock(ref stackpoolmu); 

            // Scan stack pools for empty stack spans.
            foreach (var (order) in stackpool)
            {
                var list = ref stackpool[order];
                {
                    var s__prev2 = s;

                    var s = list.first;

                    while (s != null)
                    {
                        var next = s.next;
                        if (s.allocCount == 0L)
                        {
                            list.remove(s);
                            s.manualFreeList = 0L;
                            mheap_.freeManual(s, ref memstats.stacks_inuse);
                        }
                        s = next;
                    }


                    s = s__prev2;
                }
            }
            unlock(ref stackpoolmu); 

            // Free large stack spans.
            lock(ref stackLarge.@lock);
            foreach (var (i) in stackLarge.free)
            {
                {
                    var s__prev2 = s;

                    s = stackLarge.free[i].first;

                    while (s != null)
                    {
                        next = s.next;
                        stackLarge.free[i].remove(s);
                        mheap_.freeManual(s, ref memstats.stacks_inuse);
                        s = next;
                    }


                    s = s__prev2;
                }
            }
            unlock(ref stackLarge.@lock);
        }

        //go:nosplit
        private static void morestackc()
        {
            systemstack(() =>
            {
                throw("attempt to execute system stack code on user stack");
            });
        }
    }
}
