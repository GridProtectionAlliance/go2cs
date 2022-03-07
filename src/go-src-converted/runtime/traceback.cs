// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:24 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\traceback.go
using bytealg = go.@internal.bytealg_package;
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

    // The code in this file implements stack trace walking for all architectures.
    // The most important fact about a given architecture is whether it uses a link register.
    // On systems with link registers, the prologue for a non-leaf function stores the
    // incoming value of LR at the bottom of the newly allocated stack frame.
    // On systems without link registers (x86), the architecture pushes a return PC during
    // the call instruction, so the return PC ends up above the stack frame.
    // In this file, the return PC is always called LR, no matter how it was found.
private static readonly var usesLR = sys.MinFrameSize > 0;

// Traceback over the deferred function calls.
// Report them like calls that have been invoked but not started executing yet.


// Traceback over the deferred function calls.
// Report them like calls that have been invoked but not started executing yet.
private static bool tracebackdefers(ptr<g> _addr_gp, Func<ptr<stkframe>, unsafe.Pointer, bool> callback, unsafe.Pointer v) {
    ref g gp = ref _addr_gp.val;

    ref stkframe frame = ref heap(out ptr<stkframe> _addr_frame);
    {
        var d = gp._defer;

        while (d != null) {
            var fn = d.fn;
            if (fn == null) { 
                // Defer of nil function. Args don't matter.
                frame.pc = 0;
                frame.fn = new funcInfo();
                frame.argp = 0;
                frame.arglen = 0;
                frame.argmap = null;
            d = d.link;
            }
            else
 {
                frame.pc = fn.fn;
                var f = findfunc(frame.pc);
                if (!f.valid()) {
                    print("runtime: unknown pc in defer ", hex(frame.pc), "\n");
                    throw("unknown pc");
                }
                frame.fn = f;
                frame.argp = uintptr(deferArgs(d));
                bool ok = default;
                frame.arglen, frame.argmap, ok = getArgInfoFast(f, true);
                if (!ok) {
                    frame.arglen, frame.argmap = getArgInfo(_addr_frame, f, true, _addr_fn);
                }
            }

            frame.continpc = frame.pc;
            if (!callback((stkframe.val)(noescape(@unsafe.Pointer(_addr_frame))), v)) {
                return ;
            }

        }
    }

}

// Generic traceback. Handles runtime stack prints (pcbuf == nil),
// the runtime.Callers function (pcbuf != nil), as well as the garbage
// collector (callback != nil).  A little clunky to merge these, but avoids
// duplicating the code and all its subtlety.
//
// The skip argument is only valid with pcbuf != nil and counts the number
// of logical frames to skip rather than physical frames (with inlining, a
// PC in pcbuf can represent multiple calls).
private static nint gentraceback(System.UIntPtr pc0, System.UIntPtr sp0, System.UIntPtr lr0, ptr<g> _addr_gp, nint skip, ptr<System.UIntPtr> _addr_pcbuf, nint max, Func<ptr<stkframe>, unsafe.Pointer, bool> callback, unsafe.Pointer v, nuint flags) {
    ref g gp = ref _addr_gp.val;
    ref System.UIntPtr pcbuf = ref _addr_pcbuf.val;

    if (skip > 0 && callback != null) {
        throw("gentraceback callback cannot be used with non-zero skip");
    }
    {
        var ourg = getg();

        if (ourg == gp && ourg == ourg.m.curg) { 
            // The starting sp has been passed in as a uintptr, and the caller may
            // have other uintptr-typed stack references as well.
            // If during one of the calls that got us here or during one of the
            // callbacks below the stack must be grown, all these uintptr references
            // to the stack will not be updated, and gentraceback will continue
            // to inspect the old stack memory, which may no longer be valid.
            // Even if all the variables were updated correctly, it is not clear that
            // we want to expose a traceback that begins on one stack and ends
            // on another stack. That could confuse callers quite a bit.
            // Instead, we require that gentraceback and any other function that
            // accepts an sp for the current goroutine (typically obtained by
            // calling getcallersp) must not run on that goroutine's stack but
            // instead on the g0 stack.
            throw("gentraceback cannot trace user goroutine on its own stack");

        }
    }

    var (level, _, _) = gotraceback();

    ptr<funcval> ctxt; // Context pointer for unstarted goroutines. See issue #25897.

    if (pc0 == ~uintptr(0) && sp0 == ~uintptr(0)) { // Signal to fetch saved values from gp.
        if (gp.syscallsp != 0) {
            pc0 = gp.syscallpc;
            sp0 = gp.syscallsp;
            if (usesLR) {
                lr0 = 0;
            }
        }
        else
 {
            pc0 = gp.sched.pc;
            sp0 = gp.sched.sp;
            if (usesLR) {
                lr0 = gp.sched.lr;
            }
            ctxt = (funcval.val)(gp.sched.ctxt);
        }
    }
    nint nprint = 0;
    ref stkframe frame = ref heap(out ptr<stkframe> _addr_frame);
    frame.pc = pc0;
    frame.sp = sp0;
    if (usesLR) {
        frame.lr = lr0;
    }
    var waspanic = false;
    var cgoCtxt = gp.cgoCtxt;
    var printing = pcbuf == null && callback == null; 

    // If the PC is zero, it's likely a nil function call.
    // Start in the caller's frame.
    if (frame.pc == 0) {
        if (usesLR) {
            frame.pc = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(frame.sp));
            frame.lr = 0;
        }
        else
 {
            frame.pc = uintptr(new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(frame.sp)));
            frame.sp += sys.PtrSize;
        }
    }
    var f = findfunc(frame.pc);
    if (!f.valid()) {
        if (callback != null || printing) {
            print("runtime: unknown pc ", hex(frame.pc), "\n");
            tracebackHexdump(gp.stack, _addr_frame, 0);
        }
        if (callback != null) {
            throw("unknown pc");
        }
        return 0;

    }
    frame.fn = f;

    ref pcvalueCache cache = ref heap(out ptr<pcvalueCache> _addr_cache);

    var lastFuncID = funcID_normal;
    nint n = 0;
    while (n < max) { 
        // Typically:
        //    pc is the PC of the running function.
        //    sp is the stack pointer at that program counter.
        //    fp is the frame pointer (caller's stack pointer) at that program counter, or nil if unknown.
        //    stk is the stack containing sp.
        //    The caller's program counter is lr, unless lr is zero, in which case it is *(uintptr*)sp.
        f = frame.fn;
        if (f.pcsp == 0) { 
            // No frame information, must be external function, like race support.
            // See golang.org/issue/13568.
            break;

        }
        var flag = f.flag;
        if (f.funcID == funcID_cgocallback) { 
            // cgocallback does write SP to switch from the g0 to the curg stack,
            // but it carefully arranges that during the transition BOTH stacks
            // have cgocallback frame valid for unwinding through.
            // So we don't need to exclude it with the other SP-writing functions.
            flag &= funcFlag_SPWRITE;

        }
        if (frame.pc == pc0 && frame.sp == sp0 && pc0 == gp.syscallpc && sp0 == gp.syscallsp) { 
            // Some Syscall functions write to SP, but they do so only after
            // saving the entry PC/SP using entersyscall.
            // Since we are using the entry PC/SP, the later SP write doesn't matter.
            flag &= funcFlag_SPWRITE;

        }
        if (frame.fp == 0) { 
            // Jump over system stack transitions. If we're on g0 and there's a user
            // goroutine, try to jump. Otherwise this is a regular call.
            if (flags & _TraceJumpStack != 0 && gp == gp.m.g0 && gp.m.curg != null) {

                if (f.funcID == funcID_morestack) 
                    // morestack does not return normally -- newstack()
                    // gogo's to curg.sched. Match that.
                    // This keeps morestack() from showing up in the backtrace,
                    // but that makes some sense since it'll never be returned
                    // to.
                    frame.pc = gp.m.curg.sched.pc;
                    frame.fn = findfunc(frame.pc);
                    f = frame.fn;
                    flag = f.flag;
                    frame.sp = gp.m.curg.sched.sp;
                    cgoCtxt = gp.m.curg.cgoCtxt;
                else if (f.funcID == funcID_systemstack) 
                    // systemstack returns normally, so just follow the
                    // stack transition.
                    frame.sp = gp.m.curg.sched.sp;
                    cgoCtxt = gp.m.curg.cgoCtxt;
                    flag &= funcFlag_SPWRITE;
                
            }

            frame.fp = frame.sp + uintptr(funcspdelta(f, frame.pc, _addr_cache));
            if (!usesLR) { 
                // On x86, call instruction pushes return PC before entering new function.
                frame.fp += sys.PtrSize;

            }

        }
        funcInfo flr = default;
        if (flag & funcFlag_TOPFRAME != 0) { 
            // This function marks the top of the stack. Stop the traceback.
            frame.lr = 0;
            flr = new funcInfo();

        }
        else if (flag & funcFlag_SPWRITE != 0 && (callback == null || n > 0)) { 
            // The function we are in does a write to SP that we don't know
            // how to encode in the spdelta table. Examples include context
            // switch routines like runtime.gogo but also any code that switches
            // to the g0 stack to run host C code. Since we can't reliably unwind
            // the SP (we might not even be on the stack we think we are),
            // we stop the traceback here.
            // This only applies for profiling signals (callback == nil).
            //
            // For a GC stack traversal (callback != nil), we should only see
            // a function when it has voluntarily preempted itself on entry
            // during the stack growth check. In that case, the function has
            // not yet had a chance to do any writes to SP and is safe to unwind.
            // isAsyncSafePoint does not allow assembly functions to be async preempted,
            // and preemptPark double-checks that SPWRITE functions are not async preempted.
            // So for GC stack traversal we leave things alone (this if body does not execute for n == 0)
            // at the bottom frame of the stack. But farther up the stack we'd better not
            // find any.
            if (callback != null) {
                println("traceback: unexpected SPWRITE function", funcname(f));
                throw("traceback");
            }

            frame.lr = 0;
            flr = new funcInfo();

        }
        else
 {
            System.UIntPtr lrPtr = default;
            if (usesLR) {
                if (n == 0 && frame.sp < frame.fp || frame.lr == 0) {
                    lrPtr = frame.sp;
                    frame.lr = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(lrPtr));
                }
            }
            else
 {
                if (frame.lr == 0) {
                    lrPtr = frame.fp - sys.PtrSize;
                    frame.lr = uintptr(new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(lrPtr)));
                }
            }

            flr = findfunc(frame.lr);
            if (!flr.valid()) { 
                // This happens if you get a profiling interrupt at just the wrong time.
                // In that context it is okay to stop early.
                // But if callback is set, we're doing a garbage collection and must
                // get everything, so crash loudly.
                var doPrint = printing;
                if (doPrint && gp.m.incgo && f.funcID == funcID_sigpanic) { 
                    // We can inject sigpanic
                    // calls directly into C code,
                    // in which case we'll see a C
                    // return PC. Don't complain.
                    doPrint = false;

                }

                if (callback != null || doPrint) {
                    print("runtime: unexpected return pc for ", funcname(f), " called from ", hex(frame.lr), "\n");
                    tracebackHexdump(gp.stack, _addr_frame, lrPtr);
                }

                if (callback != null) {
                    throw("unknown caller pc");
                }

            }

        }
        frame.varp = frame.fp;
        if (!usesLR) { 
            // On x86, call instruction pushes return PC before entering new function.
            frame.varp -= sys.PtrSize;

        }
        if (frame.varp > frame.sp && framepointer_enabled) {
            frame.varp -= sys.PtrSize;
        }
        if (callback != null || printing) {
            frame.argp = frame.fp + sys.MinFrameSize;
            bool ok = default;
            frame.arglen, frame.argmap, ok = getArgInfoFast(f, callback != null);
            if (!ok) {
                frame.arglen, frame.argmap = getArgInfo(_addr_frame, f, callback != null, ctxt);
            }
        }
        ctxt = null; // ctxt is only needed to get arg maps for the topmost frame

        // Determine frame's 'continuation PC', where it can continue.
        // Normally this is the return address on the stack, but if sigpanic
        // is immediately below this function on the stack, then the frame
        // stopped executing due to a trap, and frame.pc is probably not
        // a safe point for looking up liveness information. In this panicking case,
        // the function either doesn't return at all (if it has no defers or if the
        // defers do not recover) or it returns from one of the calls to
        // deferproc a second time (if the corresponding deferred func recovers).
        // In the latter case, use a deferreturn call site as the continuation pc.
        frame.continpc = frame.pc;
        if (waspanic) {
            if (frame.fn.deferreturn != 0) {
                frame.continpc = frame.fn.entry + uintptr(frame.fn.deferreturn) + 1; 
                // Note: this may perhaps keep return variables alive longer than
                // strictly necessary, as we are using "function has a defer statement"
                // as a proxy for "function actually deferred something". It seems
                // to be a minor drawback. (We used to actually look through the
                // gp._defer for a defer corresponding to this function, but that
                // is hard to do with defer records on the stack during a stack copy.)
                // Note: the +1 is to offset the -1 that
                // stack.go:getStackMap does to back up a return
                // address make sure the pc is in the CALL instruction.
            }
            else
 {
                frame.continpc = 0;
            }

        }
        if (callback != null) {
            if (!callback((stkframe.val)(noescape(@unsafe.Pointer(_addr_frame))), v)) {
                return n;
            }
        }
        if (pcbuf != null) {
            var pc = frame.pc; 
            // backup to CALL instruction to read inlining info (same logic as below)
            var tracepc = pc; 
            // Normally, pc is a return address. In that case, we want to look up
            // file/line information using pc-1, because that is the pc of the
            // call instruction (more precisely, the last byte of the call instruction).
            // Callers expect the pc buffer to contain return addresses and do the
            // same -1 themselves, so we keep pc unchanged.
            // When the pc is from a signal (e.g. profiler or segv) then we want
            // to look up file/line information using pc, and we store pc+1 in the
            // pc buffer so callers can unconditionally subtract 1 before looking up.
            // See issue 34123.
            // The pc can be at function entry when the frame is initialized without
            // actually running code, like runtime.mstart.
            if ((n == 0 && flags & _TraceTrap != 0) || waspanic || pc == f.entry) {
                pc++;
            }
            else
 {
                tracepc--;
            } 

            // If there is inlining info, record the inner frames.
            {
                var inldata__prev2 = inldata;

                var inldata = funcdata(f, _FUNCDATA_InlTree);

                if (inldata != null) {
                    ptr<array<inlinedCall>> inltree = new ptr<ptr<array<inlinedCall>>>(inldata);
                    while (true) {
                        var ix = pcdatavalue(f, _PCDATA_InlTreeIndex, tracepc, _addr_cache);
                        if (ix < 0) {
                            break;
                        }
                        if (inltree[ix].funcID == funcID_wrapper && elideWrapperCalling(lastFuncID)) { 
                            // ignore wrappers
                        }
                        else if (skip > 0) {
                            skip--;
                        }
                        else if (n < max) {
                            new ptr<ptr<array<System.UIntPtr>>>(@unsafe.Pointer(pcbuf))[n] = pc;
                            n++;
                        }

                        lastFuncID = inltree[ix].funcID; 
                        // Back up to an instruction in the "caller".
                        tracepc = frame.fn.entry + uintptr(inltree[ix].parentPc);
                        pc = tracepc + 1;

                    }


                } 
                // Record the main frame.

                inldata = inldata__prev2;

            } 
            // Record the main frame.
            if (f.funcID == funcID_wrapper && elideWrapperCalling(lastFuncID)) { 
                // Ignore wrapper functions (except when they trigger panics).
            }
            else if (skip > 0) {
                skip--;
            }
            else if (n < max) {
                new ptr<ptr<array<System.UIntPtr>>>(@unsafe.Pointer(pcbuf))[n] = pc;
                n++;
            }

            lastFuncID = f.funcID;
            n--; // offset n++ below
        }
        if (printing) { 
            // assume skip=0 for printing.
            //
            // Never elide wrappers if we haven't printed
            // any frames. And don't elide wrappers that
            // called panic rather than the wrapped
            // function. Otherwise, leave them out.

            // backup to CALL instruction to read inlining info (same logic as below)
            tracepc = frame.pc;
            if ((n > 0 || flags & _TraceTrap == 0) && frame.pc > f.entry && !waspanic) {
                tracepc--;
            } 
            // If there is inlining info, print the inner frames.
            {
                var inldata__prev2 = inldata;

                inldata = funcdata(f, _FUNCDATA_InlTree);

                if (inldata != null) {
                    inltree = new ptr<ptr<array<inlinedCall>>>(inldata);
                    ref _func inlFunc = ref heap(out ptr<_func> _addr_inlFunc);
                    funcInfo inlFuncInfo = new funcInfo(&inlFunc,f.datap);
                    while (true) {
                        ix = pcdatavalue(f, _PCDATA_InlTreeIndex, tracepc, null);
                        if (ix < 0) {
                            break;
                        } 

                        // Create a fake _func for the
                        // inlined function.
                        inlFunc.nameoff = inltree[ix].func_;
                        inlFunc.funcID = inltree[ix].funcID;

                        if ((flags & _TraceRuntimeFrames) != 0 || showframe(inlFuncInfo, _addr_gp, nprint == 0, inlFuncInfo.funcID, lastFuncID)) {
                            var name = funcname(inlFuncInfo);
                            var (file, line) = funcline(f, tracepc);
                            print(name, "(...)\n");
                            print("\t", file, ":", line, "\n");
                            nprint++;
                        }

                        lastFuncID = inltree[ix].funcID; 
                        // Back up to an instruction in the "caller".
                        tracepc = frame.fn.entry + uintptr(inltree[ix].parentPc);

                    }


                }

                inldata = inldata__prev2;

            }

            if ((flags & _TraceRuntimeFrames) != 0 || showframe(f, _addr_gp, nprint == 0, f.funcID, lastFuncID)) { 
                // Print during crash.
                //    main(0x1, 0x2, 0x3)
                //        /home/rsc/go/src/runtime/x.go:23 +0xf
                //
                name = funcname(f);
                (file, line) = funcline(f, tracepc);
                if (name == "runtime.gopanic") {
                    name = "panic";
                }

                print(name, "(");
                var argp = @unsafe.Pointer(frame.argp);
                printArgs(f, argp);
                print(")\n");
                print("\t", file, ":", line);
                if (frame.pc > f.entry) {
                    print(" +", hex(frame.pc - f.entry));
                }

                if (gp.m != null && gp.m.throwing > 0 && gp == gp.m.curg || level >= 2) {
                    print(" fp=", hex(frame.fp), " sp=", hex(frame.sp), " pc=", hex(frame.pc));
                }

                print("\n");
                nprint++;

            }

            lastFuncID = f.funcID;

        }
        n++;

        if (f.funcID == funcID_cgocallback && len(cgoCtxt) > 0) {
            ctxt = cgoCtxt[len(cgoCtxt) - 1];
            cgoCtxt = cgoCtxt[..(int)len(cgoCtxt) - 1]; 

            // skip only applies to Go frames.
            // callback != nil only used when we only care
            // about Go frames.
            if (skip == 0 && callback == null) {
                n = tracebackCgoContext(_addr_pcbuf, printing, ctxt, n, max);
            }

        }
        waspanic = f.funcID == funcID_sigpanic;
        var injectedCall = waspanic || f.funcID == funcID_asyncPreempt; 

        // Do not unwind past the bottom of the stack.
        if (!flr.valid()) {
            break;
        }
        frame.fn = flr;
        frame.pc = frame.lr;
        frame.lr = 0;
        frame.sp = frame.fp;
        frame.fp = 0;
        frame.argmap = null; 

        // On link register architectures, sighandler saves the LR on stack
        // before faking a call.
        if (usesLR && injectedCall) {
            ptr<ptr<System.UIntPtr>> x = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(frame.sp));
            frame.sp += alignUp(sys.MinFrameSize, sys.StackAlign);
            f = findfunc(frame.pc);
            frame.fn = f;
            if (!f.valid()) {
                frame.pc = x;
            }
            else if (funcspdelta(f, frame.pc, _addr_cache) == 0) {
                frame.lr = x;
            }

        }
    }

    if (printing) {
        n = nprint;
    }
    if (callback != null && n < max && frame.sp != gp.stktopsp) {
        print("runtime: g", gp.goid, ": frame.sp=", hex(frame.sp), " top=", hex(gp.stktopsp), "\n");
        print("\tstack=[", hex(gp.stack.lo), "-", hex(gp.stack.hi), "] n=", n, " max=", max, "\n");
        throw("traceback did not unwind completely");
    }
    return n;

}

// printArgs prints function arguments in traceback.
private static void printArgs(funcInfo f, unsafe.Pointer argp) { 
    // The "instruction" of argument printing is encoded in _FUNCDATA_ArgInfo.
    // See cmd/compile/internal/ssagen.emitArgInfo for the description of the
    // encoding.
    // These constants need to be in sync with the compiler.
    const nuint _endSeq = 0xff;
    const nuint _startAgg = 0xfe;
    const nuint _endAgg = 0xfd;
    const nuint _dotdotdot = 0xfc;
    const nuint _offsetTooLarge = 0xfb;


    const nint limit = 10; // print no more than 10 args/components
    const nint maxDepth = 5; // no more than 5 layers of nesting
    const var maxLen = (maxDepth * 3 + 2) * limit + 1; // max length of _FUNCDATA_ArgInfo (see the compiler side for reasoning)

    ptr<array<byte>> p = new ptr<ptr<array<byte>>>(funcdata(f, _FUNCDATA_ArgInfo));
    if (p == null) {
        return ;
    }
    Action<byte, byte> print1 = (off, sz) => {
        var x = readUnaligned64(add(argp, uintptr(off))); 
        // mask out irrelavant bits
        if (sz < 8) {
            nint shift = 64 - sz * 8;
            if (sys.BigEndian) {
                x = x >> (int)(shift);
            }
            else
 {
                x = x << (int)(shift) >> (int)(shift);
            }

        }
        print(hex(x));

    };

    var start = true;
    Action printcomma = () => {
        if (!start) {
            print(", ");
        }
    };
    nint pi = 0;
printloop:
    while (true) {
        var o = p[pi];
        pi++;

        if (o == _endSeq) 
            _breakprintloop = true;
            break;
        else if (o == _startAgg) 
            printcomma();
            print("{");
            start = true;
            continue;
        else if (o == _endAgg) 
            print("}");
        else if (o == _dotdotdot) 
            printcomma();
            print("...");
        else if (o == _offsetTooLarge) 
            printcomma();
            print("_");
        else 
            printcomma();
            var sz = p[pi];
            pi++;
            print1(o, sz);
                start = false;

    }

}

// reflectMethodValue is a partial duplicate of reflect.makeFuncImpl
// and reflect.methodValue.
private partial struct reflectMethodValue {
    public System.UIntPtr fn;
    public ptr<bitvector> stack; // ptrmap for both args and results
    public System.UIntPtr argLen; // just args
}

// getArgInfoFast returns the argument frame information for a call to f.
// It is short and inlineable. However, it does not handle all functions.
// If ok reports false, you must call getArgInfo instead.
// TODO(josharian): once we do mid-stack inlining,
// call getArgInfo directly from getArgInfoFast and stop returning an ok bool.
private static (System.UIntPtr, ptr<bitvector>, bool) getArgInfoFast(funcInfo f, bool needArgMap) {
    System.UIntPtr arglen = default;
    ptr<bitvector> argmap = default!;
    bool ok = default;

    return (uintptr(f.args), _addr_null!, !(needArgMap && f.args == _ArgsSizeUnknown));
}

// getArgInfo returns the argument frame information for a call to f
// with call frame frame.
//
// This is used for both actual calls with active stack frames and for
// deferred calls or goroutines that are not yet executing. If this is an actual
// call, ctxt must be nil (getArgInfo will retrieve what it needs from
// the active stack frame). If this is a deferred call or unstarted goroutine,
// ctxt must be the function object that was deferred or go'd.
private static (System.UIntPtr, ptr<bitvector>) getArgInfo(ptr<stkframe> _addr_frame, funcInfo f, bool needArgMap, ptr<funcval> _addr_ctxt) {
    System.UIntPtr arglen = default;
    ptr<bitvector> argmap = default!;
    ref stkframe frame = ref _addr_frame.val;
    ref funcval ctxt = ref _addr_ctxt.val;

    arglen = uintptr(f.args);
    if (needArgMap && f.args == _ArgsSizeUnknown) { 
        // Extract argument bitmaps for reflect stubs from the calls they made to reflect.
        switch (funcname(f)) {
            case "reflect.makeFuncStub": 
                // These take a *reflect.methodValue as their
                // context register.

            case "reflect.methodValueCall": 
                // These take a *reflect.methodValue as their
                // context register.
                           ptr<reflectMethodValue> mv;
                           bool retValid = default;
                           if (ctxt != null) { 
                               // This is not an actual call, but a
                               // deferred call or an unstarted goroutine.
                               // The function value is itself the *reflect.methodValue.
                               mv = (reflectMethodValue.val)(@unsafe.Pointer(ctxt));

                           }
                           else
                { 
                               // This is a real call that took the
                               // *reflect.methodValue as its context
                               // register and immediately saved it
                               // to 0(SP). Get the methodValue from
                               // 0(SP).
                               var arg0 = frame.sp + sys.MinFrameSize;
                               mv = new ptr<ptr<ptr<ptr<reflectMethodValue>>>>(@unsafe.Pointer(arg0)); 
                               // Figure out whether the return values are valid.
                               // Reflect will update this value after it copies
                               // in the return values.
                               retValid = new ptr<ptr<ptr<bool>>>(@unsafe.Pointer(arg0 + 4 * sys.PtrSize));

                           }

                           if (mv.fn != f.entry) {
                               print("runtime: confused by ", funcname(f), "\n");
                               throw("reflect mismatch");
                           }

                           var bv = mv.stack;
                           arglen = uintptr(bv.n * sys.PtrSize);
                           if (!retValid) {
                               arglen = uintptr(mv.argLen) & ~(sys.PtrSize - 1);
                           }

                           argmap = bv;

                break;
        }

    }
    return ;

}

// tracebackCgoContext handles tracing back a cgo context value, from
// the context argument to setCgoTraceback, for the gentraceback
// function. It returns the new value of n.
private static nint tracebackCgoContext(ptr<System.UIntPtr> _addr_pcbuf, bool printing, System.UIntPtr ctxt, nint n, nint max) {
    ref System.UIntPtr pcbuf = ref _addr_pcbuf.val;

    array<System.UIntPtr> cgoPCs = new array<System.UIntPtr>(32);
    cgoContextPCs(ctxt, cgoPCs[..]);
    ref cgoSymbolizerArg arg = ref heap(out ptr<cgoSymbolizerArg> _addr_arg);
    var anySymbolized = false;
    foreach (var (_, pc) in cgoPCs) {
        if (pc == 0 || n >= max) {
            break;
        }
        if (pcbuf != null) {
            new ptr<ptr<array<System.UIntPtr>>>(@unsafe.Pointer(pcbuf))[n] = pc;
        }
        if (printing) {
            if (cgoSymbolizer == null) {
                print("non-Go function at pc=", hex(pc), "\n");
            }
            else
 {
                var c = printOneCgoTraceback(pc, max - n, _addr_arg);
                n += c - 1; // +1 a few lines down
                anySymbolized = true;

            }

        }
        n++;

    }    if (anySymbolized) {
        arg.pc = 0;
        callCgoSymbolizer(_addr_arg);
    }
    return n;

}

private static void printcreatedby(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;
 
    // Show what created goroutine, except main goroutine (goid 1).
    var pc = gp.gopc;
    var f = findfunc(pc);
    if (f.valid() && showframe(f, _addr_gp, false, funcID_normal, funcID_normal) && gp.goid != 1) {
        printcreatedby1(f, pc);
    }
}

private static void printcreatedby1(funcInfo f, System.UIntPtr pc) {
    print("created by ", funcname(f), "\n");
    var tracepc = pc; // back up to CALL instruction for funcline.
    if (pc > f.entry) {
        tracepc -= sys.PCQuantum;
    }
    var (file, line) = funcline(f, tracepc);
    print("\t", file, ":", line);
    if (pc > f.entry) {
        print(" +", hex(pc - f.entry));
    }
    print("\n");

}

private static void traceback(System.UIntPtr pc, System.UIntPtr sp, System.UIntPtr lr, ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    traceback1(pc, sp, lr, _addr_gp, 0);
}

// tracebacktrap is like traceback but expects that the PC and SP were obtained
// from a trap, not from gp->sched or gp->syscallpc/gp->syscallsp or getcallerpc/getcallersp.
// Because they are from a trap instead of from a saved pair,
// the initial PC must not be rewound to the previous instruction.
// (All the saved pairs record a PC that is a return address, so we
// rewind it into the CALL instruction.)
// If gp.m.libcall{g,pc,sp} information is available, it uses that information in preference to
// the pc/sp/lr passed in.
private static void tracebacktrap(System.UIntPtr pc, System.UIntPtr sp, System.UIntPtr lr, ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    if (gp.m.libcallsp != 0) { 
        // We're in C code somewhere, traceback from the saved position.
        traceback1(gp.m.libcallpc, gp.m.libcallsp, 0, _addr_gp.m.libcallg.ptr(), 0);
        return ;

    }
    traceback1(pc, sp, lr, _addr_gp, _TraceTrap);

}

private static void traceback1(System.UIntPtr pc, System.UIntPtr sp, System.UIntPtr lr, ptr<g> _addr_gp, nuint flags) {
    ref g gp = ref _addr_gp.val;
 
    // If the goroutine is in cgo, and we have a cgo traceback, print that.
    if (iscgo && gp.m != null && gp.m.ncgo > 0 && gp.syscallsp != 0 && gp.m.cgoCallers != null && gp.m.cgoCallers[0] != 0) { 
        // Lock cgoCallers so that a signal handler won't
        // change it, copy the array, reset it, unlock it.
        // We are locked to the thread and are not running
        // concurrently with a signal handler.
        // We just have to stop a signal handler from interrupting
        // in the middle of our copy.
        atomic.Store(_addr_gp.m.cgoCallersUse, 1);
        ref var cgoCallers = ref heap(gp.m.cgoCallers.val, out ptr<var> _addr_cgoCallers);
        gp.m.cgoCallers[0] = 0;
        atomic.Store(_addr_gp.m.cgoCallersUse, 0);

        printCgoTraceback(_addr_cgoCallers);

    }
    nint n = default;
    if (readgstatus(gp) & ~_Gscan == _Gsyscall) { 
        // Override registers if blocked in system call.
        pc = gp.syscallpc;
        sp = gp.syscallsp;
        flags &= _TraceTrap;

    }
    n = gentraceback(pc, sp, lr, _addr_gp, 0, _addr_null, _TracebackMaxFrames, null, null, flags);
    if (n == 0 && (flags & _TraceRuntimeFrames) == 0) {
        n = gentraceback(pc, sp, lr, _addr_gp, 0, _addr_null, _TracebackMaxFrames, null, null, flags | _TraceRuntimeFrames);
    }
    if (n == _TracebackMaxFrames) {
        print("...additional frames elided...\n");
    }
    printcreatedby(_addr_gp);

    if (gp.ancestors == null) {
        return ;
    }
    foreach (var (_, ancestor) in gp.ancestors.val) {
        printAncestorTraceback(ancestor);
    }
}

// printAncestorTraceback prints the traceback of the given ancestor.
// TODO: Unify this with gentraceback and CallersFrames.
private static void printAncestorTraceback(ancestorInfo ancestor) {
    print("[originating from goroutine ", ancestor.goid, "]:\n");
    foreach (var (fidx, pc) in ancestor.pcs) {
        var f = findfunc(pc); // f previously validated
        if (showfuncinfo(f, fidx == 0, funcID_normal, funcID_normal)) {
            printAncestorTracebackFuncInfo(f, pc);
        }
    }    if (len(ancestor.pcs) == _TracebackMaxFrames) {
        print("...additional frames elided...\n");
    }
    f = findfunc(ancestor.gopc);
    if (f.valid() && showfuncinfo(f, false, funcID_normal, funcID_normal) && ancestor.goid != 1) {
        printcreatedby1(f, ancestor.gopc);
    }
}

// printAncestorTraceback prints the given function info at a given pc
// within an ancestor traceback. The precision of this info is reduced
// due to only have access to the pcs at the time of the caller
// goroutine being created.
private static void printAncestorTracebackFuncInfo(funcInfo f, System.UIntPtr pc) {
    var name = funcname(f);
    {
        var inldata = funcdata(f, _FUNCDATA_InlTree);

        if (inldata != null) {
            ptr<array<inlinedCall>> inltree = new ptr<ptr<array<inlinedCall>>>(inldata);
            var ix = pcdatavalue(f, _PCDATA_InlTreeIndex, pc, null);
            if (ix >= 0) {
                name = funcnameFromNameoff(f, inltree[ix].func_);
            }
        }
    }

    var (file, line) = funcline(f, pc);
    if (name == "runtime.gopanic") {
        name = "panic";
    }
    print(name, "(...)\n");
    print("\t", file, ":", line);
    if (pc > f.entry) {
        print(" +", hex(pc - f.entry));
    }
    print("\n");

}

private static nint callers(nint skip, slice<System.UIntPtr> pcbuf) {
    var sp = getcallersp();
    var pc = getcallerpc();
    var gp = getg();
    nint n = default;
    systemstack(() => {
        n = gentraceback(pc, sp, 0, _addr_gp, skip, _addr_pcbuf[0], len(pcbuf), null, null, 0);
    });
    return n;
}

private static nint gcallers(ptr<g> _addr_gp, nint skip, slice<System.UIntPtr> pcbuf) {
    ref g gp = ref _addr_gp.val;

    return gentraceback(~uintptr(0), ~uintptr(0), 0, _addr_gp, skip, _addr_pcbuf[0], len(pcbuf), null, null, 0);
}

// showframe reports whether the frame with the given characteristics should
// be printed during a traceback.
private static bool showframe(funcInfo f, ptr<g> _addr_gp, bool firstFrame, funcID funcID, funcID childID) {
    ref g gp = ref _addr_gp.val;

    var g = getg();
    if (g.m.throwing > 0 && gp != null && (gp == g.m.curg || gp == g.m.caughtsig.ptr())) {
        return true;
    }
    return showfuncinfo(f, firstFrame, funcID, childID);

}

// showfuncinfo reports whether a function with the given characteristics should
// be printed during a traceback.
private static bool showfuncinfo(funcInfo f, bool firstFrame, funcID funcID, funcID childID) { 
    // Note that f may be a synthesized funcInfo for an inlined
    // function, in which case only nameoff and funcID are set.

    var (level, _, _) = gotraceback();
    if (level > 1) { 
        // Show all frames.
        return true;

    }
    if (!f.valid()) {
        return false;
    }
    if (funcID == funcID_wrapper && elideWrapperCalling(childID)) {
        return false;
    }
    var name = funcname(f); 

    // Special case: always show runtime.gopanic frame
    // in the middle of a stack trace, so that we can
    // see the boundary between ordinary code and
    // panic-induced deferred code.
    // See golang.org/issue/5832.
    if (name == "runtime.gopanic" && !firstFrame) {
        return true;
    }
    return bytealg.IndexByteString(name, '.') >= 0 && (!hasPrefix(name, "runtime.") || isExportedRuntime(name));

}

// isExportedRuntime reports whether name is an exported runtime function.
// It is only for runtime functions, so ASCII A-Z is fine.
private static bool isExportedRuntime(@string name) {
    const var n = len("runtime.");

    return len(name) > n && name[..(int)n] == "runtime." && 'A' <= name[n] && name[n] <= 'Z';
}

// elideWrapperCalling reports whether a wrapper function that called
// function id should be elided from stack traces.
private static bool elideWrapperCalling(funcID id) { 
    // If the wrapper called a panic function instead of the
    // wrapped function, we want to include it in stacks.
    return !(id == funcID_gopanic || id == funcID_sigpanic || id == funcID_panicwrap);

}

private static array<@string> gStatusStrings = new array<@string>(InitKeyedValues<@string>((_Gidle, "idle"), (_Grunnable, "runnable"), (_Grunning, "running"), (_Gsyscall, "syscall"), (_Gwaiting, "waiting"), (_Gdead, "dead"), (_Gcopystack, "copystack"), (_Gpreempted, "preempted")));

private static void goroutineheader(ptr<g> _addr_gp) {
    ref g gp = ref _addr_gp.val;

    var gpstatus = readgstatus(gp);

    var isScan = gpstatus & _Gscan != 0;
    gpstatus &= _Gscan; // drop the scan bit

    // Basic string status
    @string status = default;
    if (0 <= gpstatus && gpstatus < uint32(len(gStatusStrings))) {
        status = gStatusStrings[gpstatus];
    }
    else
 {
        status = "???";
    }
    if (gpstatus == _Gwaiting && gp.waitreason != waitReasonZero) {
        status = gp.waitreason.String();
    }
    long waitfor = default;
    if ((gpstatus == _Gwaiting || gpstatus == _Gsyscall) && gp.waitsince != 0) {
        waitfor = (nanotime() - gp.waitsince) / 60e9F;
    }
    print("goroutine ", gp.goid, " [", status);
    if (isScan) {
        print(" (scan)");
    }
    if (waitfor >= 1) {
        print(", ", waitfor, " minutes");
    }
    if (gp.lockedm != 0) {
        print(", locked to thread");
    }
    print("]:\n");

}

private static void tracebackothers(ptr<g> _addr_me) {
    ref g me = ref _addr_me.val;

    var (level, _, _) = gotraceback(); 

    // Show the current goroutine first, if we haven't already.
    var curgp = getg().m.curg;
    if (curgp != null && curgp != me) {
        print("\n");
        goroutineheader(_addr_curgp);
        traceback(~uintptr(0), ~uintptr(0), 0, _addr_curgp);
    }
    forEachGRace(gp => {
        if (gp == me || gp == curgp || readgstatus(gp) == _Gdead || isSystemGoroutine(_addr_gp, false) && level < 2) {
            return ;
        }
        print("\n");
        goroutineheader(_addr_gp); 
        // Note: gp.m == g.m occurs when tracebackothers is
        // called from a signal handler initiated during a
        // systemstack call. The original G is still in the
        // running state, and we want to print its stack.
        if (gp.m != getg().m && readgstatus(gp) & ~_Gscan == _Grunning) {
            print("\tgoroutine running on other thread; stack unavailable\n");
            printcreatedby(_addr_gp);
        }
        else
 {
            traceback(~uintptr(0), ~uintptr(0), 0, _addr_gp);
        }
    });

}

// tracebackHexdump hexdumps part of stk around frame.sp and frame.fp
// for debugging purposes. If the address bad is included in the
// hexdumped range, it will mark it as well.
private static void tracebackHexdump(stack stk, ptr<stkframe> _addr_frame, System.UIntPtr bad) {
    ref stkframe frame = ref _addr_frame.val;

    const nint expand = 32 * sys.PtrSize;

    const nint maxExpand = 256 * sys.PtrSize; 
    // Start around frame.sp.
 
    // Start around frame.sp.
    var lo = frame.sp;
    var hi = frame.sp; 
    // Expand to include frame.fp.
    if (frame.fp != 0 && frame.fp < lo) {
        lo = frame.fp;
    }
    if (frame.fp != 0 && frame.fp > hi) {
        hi = frame.fp;
    }
    (lo, hi) = (lo - expand, hi + expand);    if (lo < frame.sp - maxExpand) {
        lo = frame.sp - maxExpand;
    }
    if (hi > frame.sp + maxExpand) {
        hi = frame.sp + maxExpand;
    }
    if (lo < stk.lo) {
        lo = stk.lo;
    }
    if (hi > stk.hi) {
        hi = stk.hi;
    }
    print("stack: frame={sp:", hex(frame.sp), ", fp:", hex(frame.fp), "} stack=[", hex(stk.lo), ",", hex(stk.hi), ")\n");
    hexdumpWords(lo, hi, p => {

        if (p == frame.fp) 
            return '>';
        else if (p == frame.sp) 
            return '<';
        else if (p == bad) 
            return '!';
                return 0;

    });

}

// isSystemGoroutine reports whether the goroutine g must be omitted
// in stack dumps and deadlock detector. This is any goroutine that
// starts at a runtime.* entry point, except for runtime.main,
// runtime.handleAsyncEvent (wasm only) and sometimes runtime.runfinq.
//
// If fixed is true, any goroutine that can vary between user and
// system (that is, the finalizer goroutine) is considered a user
// goroutine.
private static bool isSystemGoroutine(ptr<g> _addr_gp, bool @fixed) {
    ref g gp = ref _addr_gp.val;
 
    // Keep this in sync with cmd/trace/trace.go:isSystemGoroutine.
    var f = findfunc(gp.startpc);
    if (!f.valid()) {
        return false;
    }
    if (f.funcID == funcID_runtime_main || f.funcID == funcID_handleAsyncEvent) {
        return false;
    }
    if (f.funcID == funcID_runfinq) { 
        // We include the finalizer goroutine if it's calling
        // back into user code.
        if (fixed) { 
            // This goroutine can vary. In fixed mode,
            // always consider it a user goroutine.
            return false;

        }
        return !fingRunning;

    }
    return hasPrefix(funcname(f), "runtime.");

}

// SetCgoTraceback records three C functions to use to gather
// traceback information from C code and to convert that traceback
// information into symbolic information. These are used when printing
// stack traces for a program that uses cgo.
//
// The traceback and context functions may be called from a signal
// handler, and must therefore use only async-signal safe functions.
// The symbolizer function may be called while the program is
// crashing, and so must be cautious about using memory.  None of the
// functions may call back into Go.
//
// The context function will be called with a single argument, a
// pointer to a struct:
//
//    struct {
//        Context uintptr
//    }
//
// In C syntax, this struct will be
//
//    struct {
//        uintptr_t Context;
//    };
//
// If the Context field is 0, the context function is being called to
// record the current traceback context. It should record in the
// Context field whatever information is needed about the current
// point of execution to later produce a stack trace, probably the
// stack pointer and PC. In this case the context function will be
// called from C code.
//
// If the Context field is not 0, then it is a value returned by a
// previous call to the context function. This case is called when the
// context is no longer needed; that is, when the Go code is returning
// to its C code caller. This permits the context function to release
// any associated resources.
//
// While it would be correct for the context function to record a
// complete a stack trace whenever it is called, and simply copy that
// out in the traceback function, in a typical program the context
// function will be called many times without ever recording a
// traceback for that context. Recording a complete stack trace in a
// call to the context function is likely to be inefficient.
//
// The traceback function will be called with a single argument, a
// pointer to a struct:
//
//    struct {
//        Context    uintptr
//        SigContext uintptr
//        Buf        *uintptr
//        Max        uintptr
//    }
//
// In C syntax, this struct will be
//
//    struct {
//        uintptr_t  Context;
//        uintptr_t  SigContext;
//        uintptr_t* Buf;
//        uintptr_t  Max;
//    };
//
// The Context field will be zero to gather a traceback from the
// current program execution point. In this case, the traceback
// function will be called from C code.
//
// Otherwise Context will be a value previously returned by a call to
// the context function. The traceback function should gather a stack
// trace from that saved point in the program execution. The traceback
// function may be called from an execution thread other than the one
// that recorded the context, but only when the context is known to be
// valid and unchanging. The traceback function may also be called
// deeper in the call stack on the same thread that recorded the
// context. The traceback function may be called multiple times with
// the same Context value; it will usually be appropriate to cache the
// result, if possible, the first time this is called for a specific
// context value.
//
// If the traceback function is called from a signal handler on a Unix
// system, SigContext will be the signal context argument passed to
// the signal handler (a C ucontext_t* cast to uintptr_t). This may be
// used to start tracing at the point where the signal occurred. If
// the traceback function is not called from a signal handler,
// SigContext will be zero.
//
// Buf is where the traceback information should be stored. It should
// be PC values, such that Buf[0] is the PC of the caller, Buf[1] is
// the PC of that function's caller, and so on.  Max is the maximum
// number of entries to store.  The function should store a zero to
// indicate the top of the stack, or that the caller is on a different
// stack, presumably a Go stack.
//
// Unlike runtime.Callers, the PC values returned should, when passed
// to the symbolizer function, return the file/line of the call
// instruction.  No additional subtraction is required or appropriate.
//
// On all platforms, the traceback function is invoked when a call from
// Go to C to Go requests a stack trace. On linux/amd64, linux/ppc64le,
// and freebsd/amd64, the traceback function is also invoked when a
// signal is received by a thread that is executing a cgo call. The
// traceback function should not make assumptions about when it is
// called, as future versions of Go may make additional calls.
//
// The symbolizer function will be called with a single argument, a
// pointer to a struct:
//
//    struct {
//        PC      uintptr // program counter to fetch information for
//        File    *byte   // file name (NUL terminated)
//        Lineno  uintptr // line number
//        Func    *byte   // function name (NUL terminated)
//        Entry   uintptr // function entry point
//        More    uintptr // set non-zero if more info for this PC
//        Data    uintptr // unused by runtime, available for function
//    }
//
// In C syntax, this struct will be
//
//    struct {
//        uintptr_t PC;
//        char*     File;
//        uintptr_t Lineno;
//        char*     Func;
//        uintptr_t Entry;
//        uintptr_t More;
//        uintptr_t Data;
//    };
//
// The PC field will be a value returned by a call to the traceback
// function.
//
// The first time the function is called for a particular traceback,
// all the fields except PC will be 0. The function should fill in the
// other fields if possible, setting them to 0/nil if the information
// is not available. The Data field may be used to store any useful
// information across calls. The More field should be set to non-zero
// if there is more information for this PC, zero otherwise. If More
// is set non-zero, the function will be called again with the same
// PC, and may return different information (this is intended for use
// with inlined functions). If More is zero, the function will be
// called with the next PC value in the traceback. When the traceback
// is complete, the function will be called once more with PC set to
// zero; this may be used to free any information. Each call will
// leave the fields of the struct set to the same values they had upon
// return, except for the PC field when the More field is zero. The
// function must not keep a copy of the struct pointer between calls.
//
// When calling SetCgoTraceback, the version argument is the version
// number of the structs that the functions expect to receive.
// Currently this must be zero.
//
// The symbolizer function may be nil, in which case the results of
// the traceback function will be displayed as numbers. If the
// traceback function is nil, the symbolizer function will never be
// called. The context function may be nil, in which case the
// traceback function will only be called with the context field set
// to zero.  If the context function is nil, then calls from Go to C
// to Go will not show a traceback for the C portion of the call stack.
//
// SetCgoTraceback should be called only once, ideally from an init function.
public static void SetCgoTraceback(nint version, unsafe.Pointer traceback, unsafe.Pointer context, unsafe.Pointer symbolizer) => func((_, panic, _) => {
    if (version != 0) {
        panic("unsupported version");
    }
    if (cgoTraceback != null && cgoTraceback != traceback || cgoContext != null && cgoContext != context || cgoSymbolizer != null && cgoSymbolizer != symbolizer) {
        panic("call SetCgoTraceback only once");
    }
    cgoTraceback = traceback;
    cgoContext = context;
    cgoSymbolizer = symbolizer; 

    // The context function is called when a C function calls a Go
    // function. As such it is only called by C code in runtime/cgo.
    if (_cgo_set_context_function != null) {
        cgocall(_cgo_set_context_function, context);
    }
});

private static unsafe.Pointer cgoTraceback = default;
private static unsafe.Pointer cgoContext = default;
private static unsafe.Pointer cgoSymbolizer = default;

// cgoTracebackArg is the type passed to cgoTraceback.
private partial struct cgoTracebackArg {
    public System.UIntPtr context;
    public System.UIntPtr sigContext;
    public ptr<System.UIntPtr> buf;
    public System.UIntPtr max;
}

// cgoContextArg is the type passed to the context function.
private partial struct cgoContextArg {
    public System.UIntPtr context;
}

// cgoSymbolizerArg is the type passed to cgoSymbolizer.
private partial struct cgoSymbolizerArg {
    public System.UIntPtr pc;
    public ptr<byte> file;
    public System.UIntPtr lineno;
    public ptr<byte> funcName;
    public System.UIntPtr entry;
    public System.UIntPtr more;
    public System.UIntPtr data;
}

// cgoTraceback prints a traceback of callers.
private static void printCgoTraceback(ptr<cgoCallers> _addr_callers) {
    ref cgoCallers callers = ref _addr_callers.val;

    if (cgoSymbolizer == null) {
        {
            var c__prev1 = c;

            foreach (var (_, __c) in callers) {
                c = __c;
                if (c == 0) {
                    break;
                }
                print("non-Go function at pc=", hex(c), "\n");
            }

            c = c__prev1;
        }

        return ;

    }
    ref cgoSymbolizerArg arg = ref heap(out ptr<cgoSymbolizerArg> _addr_arg);
    {
        var c__prev1 = c;

        foreach (var (_, __c) in callers) {
            c = __c;
            if (c == 0) {
                break;
            }
            printOneCgoTraceback(c, 0x7fffffff, _addr_arg);
        }
        c = c__prev1;
    }

    arg.pc = 0;
    callCgoSymbolizer(_addr_arg);

}

// printOneCgoTraceback prints the traceback of a single cgo caller.
// This can print more than one line because of inlining.
// Returns the number of frames printed.
private static nint printOneCgoTraceback(System.UIntPtr pc, nint max, ptr<cgoSymbolizerArg> _addr_arg) {
    ref cgoSymbolizerArg arg = ref _addr_arg.val;

    nint c = 0;
    arg.pc = pc;
    while (c <= max) {
        callCgoSymbolizer(_addr_arg);
        if (arg.funcName != null) { 
            // Note that we don't print any argument
            // information here, not even parentheses.
            // The symbolizer must add that if appropriate.
            println(gostringnocopy(arg.funcName));

        }
        else
 {
            println("non-Go function");
        }
        print("\t");
        if (arg.file != null) {
            print(gostringnocopy(arg.file), ":", arg.lineno, " ");
        }
        print("pc=", hex(pc), "\n");
        c++;
        if (arg.more == 0) {
            break;
        }
    }
    return c;

}

// callCgoSymbolizer calls the cgoSymbolizer function.
private static void callCgoSymbolizer(ptr<cgoSymbolizerArg> _addr_arg) {
    ref cgoSymbolizerArg arg = ref _addr_arg.val;

    var call = cgocall;
    if (panicking > 0 || getg().m.curg != getg()) { 
        // We do not want to call into the scheduler when panicking
        // or when on the system stack.
        call = asmcgocall;

    }
    if (msanenabled) {
        msanwrite(@unsafe.Pointer(arg), @unsafe.Sizeof(new cgoSymbolizerArg()));
    }
    call(cgoSymbolizer, noescape(@unsafe.Pointer(arg)));

}

// cgoContextPCs gets the PC values from a cgo traceback.
private static void cgoContextPCs(System.UIntPtr ctxt, slice<System.UIntPtr> buf) {
    if (cgoTraceback == null) {
        return ;
    }
    var call = cgocall;
    if (panicking > 0 || getg().m.curg != getg()) { 
        // We do not want to call into the scheduler when panicking
        // or when on the system stack.
        call = asmcgocall;

    }
    ref cgoTracebackArg arg = ref heap(new cgoTracebackArg(context:ctxt,buf:(*uintptr)(noescape(unsafe.Pointer(&buf[0]))),max:uintptr(len(buf)),), out ptr<cgoTracebackArg> _addr_arg);
    if (msanenabled) {
        msanwrite(@unsafe.Pointer(_addr_arg), @unsafe.Sizeof(arg));
    }
    call(cgoTraceback, noescape(@unsafe.Pointer(_addr_arg)));

}

} // end runtime_package
