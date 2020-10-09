// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:49:06 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\traceback.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        // The code in this file implements stack trace walking for all architectures.
        // The most important fact about a given architecture is whether it uses a link register.
        // On systems with link registers, the prologue for a non-leaf function stores the
        // incoming value of LR at the bottom of the newly allocated stack frame.
        // On systems without link registers, the architecture pushes a return PC during
        // the call instruction, so the return PC ends up above the stack frame.
        // In this file, the return PC is always called LR, no matter how it was found.
        //
        // To date, the opposite of a link register architecture is an x86 architecture.
        // This code may need to change if some other kind of non-link-register
        // architecture comes along.
        //
        // The other important fact is the size of a pointer: on 32-bit systems the LR
        // takes up only 4 bytes on the stack, while on 64-bit systems it takes up 8 bytes.
        // Typically this is ptrSize.
        //
        // As an exception, amd64p32 had ptrSize == 4 but the CALL instruction still
        // stored an 8-byte return PC onto the stack. To accommodate this, we used regSize
        // as the size of the architecture-pushed return PC.
        //
        // usesLR is defined below in terms of minFrameSize, which is defined in
        // arch_$GOARCH.go. ptrSize and regSize are defined in stubs.go.
        private static readonly var usesLR = sys.MinFrameSize > 0L;



        private static System.UIntPtr skipPC = default;

        private static void tracebackinit()
        { 
            // Go variable initialization happens late during runtime startup.
            // Instead of initializing the variables above in the declarations,
            // schedinit calls this function so that the variables are
            // initialized and available earlier in the startup sequence.
            skipPC = funcPC(skipPleaseUseCallersFrames);

        }

        // Traceback over the deferred function calls.
        // Report them like calls that have been invoked but not started executing yet.
        private static bool tracebackdefers(ptr<g> _addr_gp, Func<ptr<stkframe>, unsafe.Pointer, bool> callback, unsafe.Pointer v)
        {
            ref g gp = ref _addr_gp.val;

            ref stkframe frame = ref heap(out ptr<stkframe> _addr_frame);
            {
                var d = gp._defer;

                while (d != null)
                {
                    var fn = d.fn;
                    if (fn == null)
                    { 
                        // Defer of nil function. Args don't matter.
                        frame.pc = 0L;
                        frame.fn = new funcInfo();
                        frame.argp = 0L;
                        frame.arglen = 0L;
                        frame.argmap = null;
                    d = d.link;
                    }
                    else
                    {
                        frame.pc = fn.fn;
                        var f = findfunc(frame.pc);
                        if (!f.valid())
                        {
                            print("runtime: unknown pc in defer ", hex(frame.pc), "\n");
                            throw("unknown pc");
                        }

                        frame.fn = f;
                        frame.argp = uintptr(deferArgs(d));
                        bool ok = default;
                        frame.arglen, frame.argmap, ok = getArgInfoFast(f, true);
                        if (!ok)
                        {
                            frame.arglen, frame.argmap = getArgInfo(_addr_frame, f, true, _addr_fn);
                        }

                    }

                    frame.continpc = frame.pc;
                    if (!callback((stkframe.val)(noescape(@unsafe.Pointer(_addr_frame))), v))
                    {
                        return ;
                    }

                }

            }

        }

        private static readonly long sizeofSkipFunction = (long)256L;

        // This function is defined in asm.s to be sizeofSkipFunction bytes long.


        // This function is defined in asm.s to be sizeofSkipFunction bytes long.
        private static void skipPleaseUseCallersFrames()
;

        // Generic traceback. Handles runtime stack prints (pcbuf == nil),
        // the runtime.Callers function (pcbuf != nil), as well as the garbage
        // collector (callback != nil).  A little clunky to merge these, but avoids
        // duplicating the code and all its subtlety.
        //
        // The skip argument is only valid with pcbuf != nil and counts the number
        // of logical frames to skip rather than physical frames (with inlining, a
        // PC in pcbuf can represent multiple calls). If a PC is partially skipped
        // and max > 1, pcbuf[1] will be runtime.skipPleaseUseCallersFrames+N where
        // N indicates the number of logical frames to skip in pcbuf[0].
        private static long gentraceback(System.UIntPtr pc0, System.UIntPtr sp0, System.UIntPtr lr0, ptr<g> _addr_gp, long skip, ptr<System.UIntPtr> _addr_pcbuf, long max, Func<ptr<stkframe>, unsafe.Pointer, bool> callback, unsafe.Pointer v, ulong flags)
        {
            ref g gp = ref _addr_gp.val;
            ref System.UIntPtr pcbuf = ref _addr_pcbuf.val;

            if (skip > 0L && callback != null)
            {>>MARKER:FUNCTION_skipPleaseUseCallersFrames_BLOCK_PREFIX<<
                throw("gentraceback callback cannot be used with non-zero skip");
            } 

            // Don't call this "g"; it's too easy get "g" and "gp" confused.
            {
                var ourg = getg();

                if (ourg == gp && ourg == ourg.m.curg)
                { 
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

            if (pc0 == ~uintptr(0L) && sp0 == ~uintptr(0L))
            { // Signal to fetch saved values from gp.
                if (gp.syscallsp != 0L)
                {
                    pc0 = gp.syscallpc;
                    sp0 = gp.syscallsp;
                    if (usesLR)
                    {
                        lr0 = 0L;
                    }

                }
                else
                {
                    pc0 = gp.sched.pc;
                    sp0 = gp.sched.sp;
                    if (usesLR)
                    {
                        lr0 = gp.sched.lr;
                    }

                    ctxt = (funcval.val)(gp.sched.ctxt);

                }

            }

            long nprint = 0L;
            ref stkframe frame = ref heap(out ptr<stkframe> _addr_frame);
            frame.pc = pc0;
            frame.sp = sp0;
            if (usesLR)
            {
                frame.lr = lr0;
            }

            var waspanic = false;
            var cgoCtxt = gp.cgoCtxt;
            var printing = pcbuf == null && callback == null; 

            // If the PC is zero, it's likely a nil function call.
            // Start in the caller's frame.
            if (frame.pc == 0L)
            {
                if (usesLR)
                {
                    frame.pc = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(frame.sp));
                    frame.lr = 0L;
                }
                else
                {
                    frame.pc = uintptr(new ptr<ptr<ptr<sys.Uintreg>>>(@unsafe.Pointer(frame.sp)));
                    frame.sp += sys.RegSize;
                }

            }

            var f = findfunc(frame.pc);
            if (!f.valid())
            {
                if (callback != null || printing)
                {
                    print("runtime: unknown pc ", hex(frame.pc), "\n");
                    tracebackHexdump(gp.stack, _addr_frame, 0L);
                }

                if (callback != null)
                {
                    throw("unknown pc");
                }

                return 0L;

            }

            frame.fn = f;

            ref pcvalueCache cache = ref heap(out ptr<pcvalueCache> _addr_cache);

            var lastFuncID = funcID_normal;
            long n = 0L;
            while (n < max)
            { 
                // Typically:
                //    pc is the PC of the running function.
                //    sp is the stack pointer at that program counter.
                //    fp is the frame pointer (caller's stack pointer) at that program counter, or nil if unknown.
                //    stk is the stack containing sp.
                //    The caller's program counter is lr, unless lr is zero, in which case it is *(uintptr*)sp.
                f = frame.fn;
                if (f.pcsp == 0L)
                { 
                    // No frame information, must be external function, like race support.
                    // See golang.org/issue/13568.
                    break;

                } 

                // Found an actual function.
                // Derive frame pointer and link register.
                if (frame.fp == 0L)
                { 
                    // Jump over system stack transitions. If we're on g0 and there's a user
                    // goroutine, try to jump. Otherwise this is a regular call.
                    if (flags & _TraceJumpStack != 0L && gp == gp.m.g0 && gp.m.curg != null)
                    {

                        if (f.funcID == funcID_morestack) 
                            // morestack does not return normally -- newstack()
                            // gogo's to curg.sched. Match that.
                            // This keeps morestack() from showing up in the backtrace,
                            // but that makes some sense since it'll never be returned
                            // to.
                            frame.pc = gp.m.curg.sched.pc;
                            frame.fn = findfunc(frame.pc);
                            f = frame.fn;
                            frame.sp = gp.m.curg.sched.sp;
                            cgoCtxt = gp.m.curg.cgoCtxt;
                        else if (f.funcID == funcID_systemstack) 
                            // systemstack returns normally, so just follow the
                            // stack transition.
                            frame.sp = gp.m.curg.sched.sp;
                            cgoCtxt = gp.m.curg.cgoCtxt;
                        
                    }

                    frame.fp = frame.sp + uintptr(funcspdelta(f, frame.pc, _addr_cache));
                    if (!usesLR)
                    { 
                        // On x86, call instruction pushes return PC before entering new function.
                        frame.fp += sys.RegSize;

                    }

                }

                funcInfo flr = default;
                if (topofstack(f, gp.m != null && gp == gp.m.g0))
                {
                    frame.lr = 0L;
                    flr = new funcInfo();
                }
                else if (usesLR && f.funcID == funcID_jmpdefer)
                { 
                    // jmpdefer modifies SP/LR/PC non-atomically.
                    // If a profiling interrupt arrives during jmpdefer,
                    // the stack unwind may see a mismatched register set
                    // and get confused. Stop if we see PC within jmpdefer
                    // to avoid that confusion.
                    // See golang.org/issue/8153.
                    if (callback != null)
                    {
                        throw("traceback_arm: found jmpdefer when tracing with callback");
                    }

                    frame.lr = 0L;

                }
                else
                {
                    System.UIntPtr lrPtr = default;
                    if (usesLR)
                    {
                        if (n == 0L && frame.sp < frame.fp || frame.lr == 0L)
                        {
                            lrPtr = frame.sp;
                            frame.lr = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(lrPtr));
                        }

                    }
                    else
                    {
                        if (frame.lr == 0L)
                        {
                            lrPtr = frame.fp - sys.RegSize;
                            frame.lr = uintptr(new ptr<ptr<ptr<sys.Uintreg>>>(@unsafe.Pointer(lrPtr)));
                        }

                    }

                    flr = findfunc(frame.lr);
                    if (!flr.valid())
                    { 
                        // This happens if you get a profiling interrupt at just the wrong time.
                        // In that context it is okay to stop early.
                        // But if callback is set, we're doing a garbage collection and must
                        // get everything, so crash loudly.
                        var doPrint = printing;
                        if (doPrint && gp.m.incgo && f.funcID == funcID_sigpanic)
                        { 
                            // We can inject sigpanic
                            // calls directly into C code,
                            // in which case we'll see a C
                            // return PC. Don't complain.
                            doPrint = false;

                        }

                        if (callback != null || doPrint)
                        {
                            print("runtime: unexpected return pc for ", funcname(f), " called from ", hex(frame.lr), "\n");
                            tracebackHexdump(gp.stack, _addr_frame, lrPtr);
                        }

                        if (callback != null)
                        {
                            throw("unknown caller pc");
                        }

                    }

                }

                frame.varp = frame.fp;
                if (!usesLR)
                { 
                    // On x86, call instruction pushes return PC before entering new function.
                    frame.varp -= sys.RegSize;

                } 

                // If framepointer_enabled and there's a frame, then
                // there's a saved bp here.
                if (frame.varp > frame.sp && (framepointer_enabled && GOARCH == "amd64" || GOARCH == "arm64"))
                {
                    frame.varp -= sys.RegSize;
                } 

                // Derive size of arguments.
                // Most functions have a fixed-size argument block,
                // so we can use metadata about the function f.
                // Not all, though: there are some variadic functions
                // in package runtime and reflect, and for those we use call-specific
                // metadata recorded by f's caller.
                if (callback != null || printing)
                {
                    frame.argp = frame.fp + sys.MinFrameSize;
                    bool ok = default;
                    frame.arglen, frame.argmap, ok = getArgInfoFast(f, callback != null);
                    if (!ok)
                    {
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
                if (waspanic)
                {
                    if (frame.fn.deferreturn != 0L)
                    {
                        frame.continpc = frame.fn.entry + uintptr(frame.fn.deferreturn) + 1L; 
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
                        frame.continpc = 0L;
                    }

                }

                if (callback != null)
                {
                    if (!callback((stkframe.val)(noescape(@unsafe.Pointer(_addr_frame))), v))
                    {
                        return n;
                    }

                }

                if (pcbuf != null)
                {
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
                    if ((n == 0L && flags & _TraceTrap != 0L) || waspanic || pc == f.entry)
                    {
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

                        if (inldata != null)
                        {
                            ptr<array<inlinedCall>> inltree = new ptr<ptr<array<inlinedCall>>>(inldata);
                            while (true)
                            {
                                var ix = pcdatavalue(f, _PCDATA_InlTreeIndex, tracepc, _addr_cache);
                                if (ix < 0L)
                                {
                                    break;
                                }

                                if (inltree[ix].funcID == funcID_wrapper && elideWrapperCalling(lastFuncID))
                                { 
                                    // ignore wrappers
                                }
                                else if (skip > 0L)
                                {
                                    skip--;
                                }
                                else if (n < max)
                                {
                                    new ptr<ptr<array<System.UIntPtr>>>(@unsafe.Pointer(pcbuf))[n] = pc;
                                    n++;
                                }

                                lastFuncID = inltree[ix].funcID; 
                                // Back up to an instruction in the "caller".
                                tracepc = frame.fn.entry + uintptr(inltree[ix].parentPc);
                                pc = tracepc + 1L;

                            }


                        } 
                        // Record the main frame.

                        inldata = inldata__prev2;

                    } 
                    // Record the main frame.
                    if (f.funcID == funcID_wrapper && elideWrapperCalling(lastFuncID))
                    { 
                        // Ignore wrapper functions (except when they trigger panics).
                    }
                    else if (skip > 0L)
                    {
                        skip--;
                    }
                    else if (n < max)
                    {
                        new ptr<ptr<array<System.UIntPtr>>>(@unsafe.Pointer(pcbuf))[n] = pc;
                        n++;
                    }

                    lastFuncID = f.funcID;
                    n--; // offset n++ below
                }

                if (printing)
                { 
                    // assume skip=0 for printing.
                    //
                    // Never elide wrappers if we haven't printed
                    // any frames. And don't elide wrappers that
                    // called panic rather than the wrapped
                    // function. Otherwise, leave them out.

                    // backup to CALL instruction to read inlining info (same logic as below)
                    tracepc = frame.pc;
                    if ((n > 0L || flags & _TraceTrap == 0L) && frame.pc > f.entry && !waspanic)
                    {
                        tracepc--;
                    } 
                    // If there is inlining info, print the inner frames.
                    {
                        var inldata__prev2 = inldata;

                        inldata = funcdata(f, _FUNCDATA_InlTree);

                        if (inldata != null)
                        {
                            inltree = new ptr<ptr<array<inlinedCall>>>(inldata);
                            while (true)
                            {
                                ix = pcdatavalue(f, _PCDATA_InlTreeIndex, tracepc, null);
                                if (ix < 0L)
                                {
                                    break;
                                }

                                if ((flags & _TraceRuntimeFrames) != 0L || showframe(f, _addr_gp, nprint == 0L, inltree[ix].funcID, lastFuncID))
                                {
                                    var name = funcnameFromNameoff(f, inltree[ix].func_);
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

                    if ((flags & _TraceRuntimeFrames) != 0L || showframe(f, _addr_gp, nprint == 0L, f.funcID, lastFuncID))
                    { 
                        // Print during crash.
                        //    main(0x1, 0x2, 0x3)
                        //        /home/rsc/go/src/runtime/x.go:23 +0xf
                        //
                        name = funcname(f);
                        (file, line) = funcline(f, tracepc);
                        if (name == "runtime.gopanic")
                        {
                            name = "panic";
                        }

                        print(name, "(");
                        ptr<array<System.UIntPtr>> argp = new ptr<ptr<array<System.UIntPtr>>>(@unsafe.Pointer(frame.argp));
                        for (var i = uintptr(0L); i < frame.arglen / sys.PtrSize; i++)
                        {
                            if (i >= 10L)
                            {
                                print(", ...");
                                break;
                            }

                            if (i != 0L)
                            {
                                print(", ");
                            }

                            print(hex(argp[i]));

                        }

                        print(")\n");
                        print("\t", file, ":", line);
                        if (frame.pc > f.entry)
                        {
                            print(" +", hex(frame.pc - f.entry));
                        }

                        if (gp.m != null && gp.m.throwing > 0L && gp == gp.m.curg || level >= 2L)
                        {
                            print(" fp=", hex(frame.fp), " sp=", hex(frame.sp), " pc=", hex(frame.pc));
                        }

                        print("\n");
                        nprint++;

                    }

                    lastFuncID = f.funcID;

                }

                n++;

                if (f.funcID == funcID_cgocallback_gofunc && len(cgoCtxt) > 0L)
                {
                    ctxt = cgoCtxt[len(cgoCtxt) - 1L];
                    cgoCtxt = cgoCtxt[..len(cgoCtxt) - 1L]; 

                    // skip only applies to Go frames.
                    // callback != nil only used when we only care
                    // about Go frames.
                    if (skip == 0L && callback == null)
                    {
                        n = tracebackCgoContext(_addr_pcbuf, printing, ctxt, n, max);
                    }

                }

                waspanic = f.funcID == funcID_sigpanic;
                var injectedCall = waspanic || f.funcID == funcID_asyncPreempt; 

                // Do not unwind past the bottom of the stack.
                if (!flr.valid())
                {
                    break;
                } 

                // Unwind to next frame.
                frame.fn = flr;
                frame.pc = frame.lr;
                frame.lr = 0L;
                frame.sp = frame.fp;
                frame.fp = 0L;
                frame.argmap = null; 

                // On link register architectures, sighandler saves the LR on stack
                // before faking a call.
                if (usesLR && injectedCall)
                {
                    ptr<ptr<System.UIntPtr>> x = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(frame.sp));
                    frame.sp += sys.MinFrameSize;
                    if (GOARCH == "arm64")
                    { 
                        // arm64 needs 16-byte aligned SP, always
                        frame.sp += sys.PtrSize;

                    }

                    f = findfunc(frame.pc);
                    frame.fn = f;
                    if (!f.valid())
                    {
                        frame.pc = x;
                    }
                    else if (funcspdelta(f, frame.pc, _addr_cache) == 0L)
                    {
                        frame.lr = x;
                    }

                }

            }


            if (printing)
            {
                n = nprint;
            } 

            // Note that panic != nil is okay here: there can be leftover panics,
            // because the defers on the panic stack do not nest in frame order as
            // they do on the defer stack. If you have:
            //
            //    frame 1 defers d1
            //    frame 2 defers d2
            //    frame 3 defers d3
            //    frame 4 panics
            //    frame 4's panic starts running defers
            //    frame 5, running d3, defers d4
            //    frame 5 panics
            //    frame 5's panic starts running defers
            //    frame 6, running d4, garbage collects
            //    frame 6, running d2, garbage collects
            //
            // During the execution of d4, the panic stack is d4 -> d3, which
            // is nested properly, and we'll treat frame 3 as resumable, because we
            // can find d3. (And in fact frame 3 is resumable. If d4 recovers
            // and frame 5 continues running, d3, d3 can recover and we'll
            // resume execution in (returning from) frame 3.)
            //
            // During the execution of d2, however, the panic stack is d2 -> d3,
            // which is inverted. The scan will match d2 to frame 2 but having
            // d2 on the stack until then means it will not match d3 to frame 3.
            // This is okay: if we're running d2, then all the defers after d2 have
            // completed and their corresponding frames are dead. Not finding d3
            // for frame 3 means we'll set frame 3's continpc == 0, which is correct
            // (frame 3 is dead). At the end of the walk the panic stack can thus
            // contain defers (d3 in this case) for dead frames. The inversion here
            // always indicates a dead frame, and the effect of the inversion on the
            // scan is to hide those dead frames, so the scan is still okay:
            // what's left on the panic stack are exactly (and only) the dead frames.
            //
            // We require callback != nil here because only when callback != nil
            // do we know that gentraceback is being called in a "must be correct"
            // context as opposed to a "best effort" context. The tracebacks with
            // callbacks only happen when everything is stopped nicely.
            // At other times, such as when gathering a stack for a profiling signal
            // or when printing a traceback during a crash, everything may not be
            // stopped nicely, and the stack walk may not be able to complete.
            if (callback != null && n < max && frame.sp != gp.stktopsp)
            {
                print("runtime: g", gp.goid, ": frame.sp=", hex(frame.sp), " top=", hex(gp.stktopsp), "\n");
                print("\tstack=[", hex(gp.stack.lo), "-", hex(gp.stack.hi), "] n=", n, " max=", max, "\n");
                throw("traceback did not unwind completely");
            }

            return n;

        }

        // reflectMethodValue is a partial duplicate of reflect.makeFuncImpl
        // and reflect.methodValue.
        private partial struct reflectMethodValue
        {
            public System.UIntPtr fn;
            public ptr<bitvector> stack; // ptrmap for both args and results
            public System.UIntPtr argLen; // just args
        }

        // getArgInfoFast returns the argument frame information for a call to f.
        // It is short and inlineable. However, it does not handle all functions.
        // If ok reports false, you must call getArgInfo instead.
        // TODO(josharian): once we do mid-stack inlining,
        // call getArgInfo directly from getArgInfoFast and stop returning an ok bool.
        private static (System.UIntPtr, ptr<bitvector>, bool) getArgInfoFast(funcInfo f, bool needArgMap)
        {
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
        private static (System.UIntPtr, ptr<bitvector>) getArgInfo(ptr<stkframe> _addr_frame, funcInfo f, bool needArgMap, ptr<funcval> _addr_ctxt)
        {
            System.UIntPtr arglen = default;
            ptr<bitvector> argmap = default!;
            ref stkframe frame = ref _addr_frame.val;
            ref funcval ctxt = ref _addr_ctxt.val;

            arglen = uintptr(f.args);
            if (needArgMap && f.args == _ArgsSizeUnknown)
            { 
                // Extract argument bitmaps for reflect stubs from the calls they made to reflect.
                switch (funcname(f))
                {
                    case "reflect.makeFuncStub": 
                        // These take a *reflect.methodValue as their
                        // context register.

                    case "reflect.methodValueCall": 
                        // These take a *reflect.methodValue as their
                        // context register.
                        ptr<reflectMethodValue> mv;
                        bool retValid = default;
                        if (ctxt != null)
                        { 
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
                            retValid = new ptr<ptr<ptr<bool>>>(@unsafe.Pointer(arg0 + 3L * sys.PtrSize));

                        }

                        if (mv.fn != f.entry)
                        {
                            print("runtime: confused by ", funcname(f), "\n");
                            throw("reflect mismatch");
                        }

                        var bv = mv.stack;
                        arglen = uintptr(bv.n * sys.PtrSize);
                        if (!retValid)
                        {
                            arglen = uintptr(mv.argLen) & ~(sys.PtrSize - 1L);
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
        private static long tracebackCgoContext(ptr<System.UIntPtr> _addr_pcbuf, bool printing, System.UIntPtr ctxt, long n, long max)
        {
            ref System.UIntPtr pcbuf = ref _addr_pcbuf.val;

            array<System.UIntPtr> cgoPCs = new array<System.UIntPtr>(32L);
            cgoContextPCs(ctxt, cgoPCs[..]);
            ref cgoSymbolizerArg arg = ref heap(out ptr<cgoSymbolizerArg> _addr_arg);
            var anySymbolized = false;
            foreach (var (_, pc) in cgoPCs)
            {
                if (pc == 0L || n >= max)
                {
                    break;
                }

                if (pcbuf != null)
                {
                    new ptr<ptr<array<System.UIntPtr>>>(@unsafe.Pointer(pcbuf))[n] = pc;
                }

                if (printing)
                {
                    if (cgoSymbolizer == null)
                    {
                        print("non-Go function at pc=", hex(pc), "\n");
                    }
                    else
                    {
                        var c = printOneCgoTraceback(pc, max - n, _addr_arg);
                        n += c - 1L; // +1 a few lines down
                        anySymbolized = true;

                    }

                }

                n++;

            }
            if (anySymbolized)
            {
                arg.pc = 0L;
                callCgoSymbolizer(_addr_arg);
            }

            return n;

        }

        private static void printcreatedby(ptr<g> _addr_gp)
        {
            ref g gp = ref _addr_gp.val;
 
            // Show what created goroutine, except main goroutine (goid 1).
            var pc = gp.gopc;
            var f = findfunc(pc);
            if (f.valid() && showframe(f, _addr_gp, false, funcID_normal, funcID_normal) && gp.goid != 1L)
            {
                printcreatedby1(f, pc);
            }

        }

        private static void printcreatedby1(funcInfo f, System.UIntPtr pc)
        {
            print("created by ", funcname(f), "\n");
            var tracepc = pc; // back up to CALL instruction for funcline.
            if (pc > f.entry)
            {
                tracepc -= sys.PCQuantum;
            }

            var (file, line) = funcline(f, tracepc);
            print("\t", file, ":", line);
            if (pc > f.entry)
            {
                print(" +", hex(pc - f.entry));
            }

            print("\n");

        }

        private static void traceback(System.UIntPtr pc, System.UIntPtr sp, System.UIntPtr lr, ptr<g> _addr_gp)
        {
            ref g gp = ref _addr_gp.val;

            traceback1(pc, sp, lr, _addr_gp, 0L);
        }

        // tracebacktrap is like traceback but expects that the PC and SP were obtained
        // from a trap, not from gp->sched or gp->syscallpc/gp->syscallsp or getcallerpc/getcallersp.
        // Because they are from a trap instead of from a saved pair,
        // the initial PC must not be rewound to the previous instruction.
        // (All the saved pairs record a PC that is a return address, so we
        // rewind it into the CALL instruction.)
        // If gp.m.libcall{g,pc,sp} information is available, it uses that information in preference to
        // the pc/sp/lr passed in.
        private static void tracebacktrap(System.UIntPtr pc, System.UIntPtr sp, System.UIntPtr lr, ptr<g> _addr_gp)
        {
            ref g gp = ref _addr_gp.val;

            if (gp.m.libcallsp != 0L)
            { 
                // We're in C code somewhere, traceback from the saved position.
                traceback1(gp.m.libcallpc, gp.m.libcallsp, 0L, _addr_gp.m.libcallg.ptr(), 0L);
                return ;

            }

            traceback1(pc, sp, lr, _addr_gp, _TraceTrap);

        }

        private static void traceback1(System.UIntPtr pc, System.UIntPtr sp, System.UIntPtr lr, ptr<g> _addr_gp, ulong flags)
        {
            ref g gp = ref _addr_gp.val;
 
            // If the goroutine is in cgo, and we have a cgo traceback, print that.
            if (iscgo && gp.m != null && gp.m.ncgo > 0L && gp.syscallsp != 0L && gp.m.cgoCallers != null && gp.m.cgoCallers[0L] != 0L)
            { 
                // Lock cgoCallers so that a signal handler won't
                // change it, copy the array, reset it, unlock it.
                // We are locked to the thread and are not running
                // concurrently with a signal handler.
                // We just have to stop a signal handler from interrupting
                // in the middle of our copy.
                atomic.Store(_addr_gp.m.cgoCallersUse, 1L);
                ref var cgoCallers = ref heap(gp.m.cgoCallers.val, out ptr<var> _addr_cgoCallers);
                gp.m.cgoCallers[0L] = 0L;
                atomic.Store(_addr_gp.m.cgoCallersUse, 0L);

                printCgoTraceback(_addr_cgoCallers);

            }

            long n = default;
            if (readgstatus(gp) & ~_Gscan == _Gsyscall)
            { 
                // Override registers if blocked in system call.
                pc = gp.syscallpc;
                sp = gp.syscallsp;
                flags &= _TraceTrap;

            } 
            // Print traceback. By default, omits runtime frames.
            // If that means we print nothing at all, repeat forcing all frames printed.
            n = gentraceback(pc, sp, lr, _addr_gp, 0L, _addr_null, _TracebackMaxFrames, null, null, flags);
            if (n == 0L && (flags & _TraceRuntimeFrames) == 0L)
            {
                n = gentraceback(pc, sp, lr, _addr_gp, 0L, _addr_null, _TracebackMaxFrames, null, null, flags | _TraceRuntimeFrames);
            }

            if (n == _TracebackMaxFrames)
            {
                print("...additional frames elided...\n");
            }

            printcreatedby(_addr_gp);

            if (gp.ancestors == null)
            {
                return ;
            }

            foreach (var (_, ancestor) in gp.ancestors.val)
            {
                printAncestorTraceback(ancestor);
            }

        }

        // printAncestorTraceback prints the traceback of the given ancestor.
        // TODO: Unify this with gentraceback and CallersFrames.
        private static void printAncestorTraceback(ancestorInfo ancestor)
        {
            print("[originating from goroutine ", ancestor.goid, "]:\n");
            foreach (var (fidx, pc) in ancestor.pcs)
            {
                var f = findfunc(pc); // f previously validated
                if (showfuncinfo(f, fidx == 0L, funcID_normal, funcID_normal))
                {
                    printAncestorTracebackFuncInfo(f, pc);
                }

            }
            if (len(ancestor.pcs) == _TracebackMaxFrames)
            {
                print("...additional frames elided...\n");
            } 
            // Show what created goroutine, except main goroutine (goid 1).
            f = findfunc(ancestor.gopc);
            if (f.valid() && showfuncinfo(f, false, funcID_normal, funcID_normal) && ancestor.goid != 1L)
            {
                printcreatedby1(f, ancestor.gopc);
            }

        }

        // printAncestorTraceback prints the given function info at a given pc
        // within an ancestor traceback. The precision of this info is reduced
        // due to only have access to the pcs at the time of the caller
        // goroutine being created.
        private static void printAncestorTracebackFuncInfo(funcInfo f, System.UIntPtr pc)
        {
            var name = funcname(f);
            {
                var inldata = funcdata(f, _FUNCDATA_InlTree);

                if (inldata != null)
                {
                    ptr<array<inlinedCall>> inltree = new ptr<ptr<array<inlinedCall>>>(inldata);
                    var ix = pcdatavalue(f, _PCDATA_InlTreeIndex, pc, null);
                    if (ix >= 0L)
                    {
                        name = funcnameFromNameoff(f, inltree[ix].func_);
                    }

                }

            }

            var (file, line) = funcline(f, pc);
            if (name == "runtime.gopanic")
            {
                name = "panic";
            }

            print(name, "(...)\n");
            print("\t", file, ":", line);
            if (pc > f.entry)
            {
                print(" +", hex(pc - f.entry));
            }

            print("\n");

        }

        private static long callers(long skip, slice<System.UIntPtr> pcbuf)
        {
            var sp = getcallersp();
            var pc = getcallerpc();
            var gp = getg();
            long n = default;
            systemstack(() =>
            {
                n = gentraceback(pc, sp, 0L, _addr_gp, skip, _addr_pcbuf[0L], len(pcbuf), null, null, 0L);
            });
            return n;

        }

        private static long gcallers(ptr<g> _addr_gp, long skip, slice<System.UIntPtr> pcbuf)
        {
            ref g gp = ref _addr_gp.val;

            return gentraceback(~uintptr(0L), ~uintptr(0L), 0L, _addr_gp, skip, _addr_pcbuf[0L], len(pcbuf), null, null, 0L);
        }

        // showframe reports whether the frame with the given characteristics should
        // be printed during a traceback.
        private static bool showframe(funcInfo f, ptr<g> _addr_gp, bool firstFrame, funcID funcID, funcID childID)
        {
            ref g gp = ref _addr_gp.val;

            var g = getg();
            if (g.m.throwing > 0L && gp != null && (gp == g.m.curg || gp == g.m.caughtsig.ptr()))
            {
                return true;
            }

            return showfuncinfo(f, firstFrame, funcID, childID);

        }

        // showfuncinfo reports whether a function with the given characteristics should
        // be printed during a traceback.
        private static bool showfuncinfo(funcInfo f, bool firstFrame, funcID funcID, funcID childID)
        {
            var (level, _, _) = gotraceback();
            if (level > 1L)
            { 
                // Show all frames.
                return true;

            }

            if (!f.valid())
            {
                return false;
            }

            if (funcID == funcID_wrapper && elideWrapperCalling(childID))
            {
                return false;
            }

            var name = funcname(f); 

            // Special case: always show runtime.gopanic frame
            // in the middle of a stack trace, so that we can
            // see the boundary between ordinary code and
            // panic-induced deferred code.
            // See golang.org/issue/5832.
            if (name == "runtime.gopanic" && !firstFrame)
            {
                return true;
            }

            return contains(name, ".") && (!hasPrefix(name, "runtime.") || isExportedRuntime(name));

        }

        // isExportedRuntime reports whether name is an exported runtime function.
        // It is only for runtime functions, so ASCII A-Z is fine.
        private static bool isExportedRuntime(@string name)
        {
            const var n = len("runtime.");

            return len(name) > n && name[..n] == "runtime." && 'A' <= name[n] && name[n] <= 'Z';
        }

        // elideWrapperCalling reports whether a wrapper function that called
        // function id should be elided from stack traces.
        private static bool elideWrapperCalling(funcID id)
        { 
            // If the wrapper called a panic function instead of the
            // wrapped function, we want to include it in stacks.
            return !(id == funcID_gopanic || id == funcID_sigpanic || id == funcID_panicwrap);

        }

        private static array<@string> gStatusStrings = new array<@string>(InitKeyedValues<@string>((_Gidle, "idle"), (_Grunnable, "runnable"), (_Grunning, "running"), (_Gsyscall, "syscall"), (_Gwaiting, "waiting"), (_Gdead, "dead"), (_Gcopystack, "copystack"), (_Gpreempted, "preempted")));

        private static void goroutineheader(ptr<g> _addr_gp)
        {
            ref g gp = ref _addr_gp.val;

            var gpstatus = readgstatus(gp);

            var isScan = gpstatus & _Gscan != 0L;
            gpstatus &= _Gscan; // drop the scan bit

            // Basic string status
            @string status = default;
            if (0L <= gpstatus && gpstatus < uint32(len(gStatusStrings)))
            {
                status = gStatusStrings[gpstatus];
            }
            else
            {
                status = "???";
            } 

            // Override.
            if (gpstatus == _Gwaiting && gp.waitreason != waitReasonZero)
            {
                status = gp.waitreason.String();
            } 

            // approx time the G is blocked, in minutes
            long waitfor = default;
            if ((gpstatus == _Gwaiting || gpstatus == _Gsyscall) && gp.waitsince != 0L)
            {
                waitfor = (nanotime() - gp.waitsince) / 60e9F;
            }

            print("goroutine ", gp.goid, " [", status);
            if (isScan)
            {
                print(" (scan)");
            }

            if (waitfor >= 1L)
            {
                print(", ", waitfor, " minutes");
            }

            if (gp.lockedm != 0L)
            {
                print(", locked to thread");
            }

            print("]:\n");

        }

        private static void tracebackothers(ptr<g> _addr_me)
        {
            ref g me = ref _addr_me.val;

            var (level, _, _) = gotraceback(); 

            // Show the current goroutine first, if we haven't already.
            var g = getg();
            var gp = g.m.curg;
            if (gp != null && gp != me)
            {
                print("\n");
                goroutineheader(_addr_gp);
                traceback(~uintptr(0L), ~uintptr(0L), 0L, _addr_gp);
            }

            lock(_addr_allglock);
            {
                var gp__prev1 = gp;

                foreach (var (_, __gp) in allgs)
                {
                    gp = __gp;
                    if (gp == me || gp == g.m.curg || readgstatus(gp) == _Gdead || isSystemGoroutine(_addr_gp, false) && level < 2L)
                    {
                        continue;
                    }

                    print("\n");
                    goroutineheader(_addr_gp); 
                    // Note: gp.m == g.m occurs when tracebackothers is
                    // called from a signal handler initiated during a
                    // systemstack call. The original G is still in the
                    // running state, and we want to print its stack.
                    if (gp.m != g.m && readgstatus(gp) & ~_Gscan == _Grunning)
                    {
                        print("\tgoroutine running on other thread; stack unavailable\n");
                        printcreatedby(_addr_gp);
                    }
                    else
                    {
                        traceback(~uintptr(0L), ~uintptr(0L), 0L, _addr_gp);
                    }

                }

                gp = gp__prev1;
            }

            unlock(_addr_allglock);

        }

        // tracebackHexdump hexdumps part of stk around frame.sp and frame.fp
        // for debugging purposes. If the address bad is included in the
        // hexdumped range, it will mark it as well.
        private static void tracebackHexdump(stack stk, ptr<stkframe> _addr_frame, System.UIntPtr bad)
        {
            ref stkframe frame = ref _addr_frame.val;

            const long expand = (long)32L * sys.PtrSize;

            const long maxExpand = (long)256L * sys.PtrSize; 
            // Start around frame.sp.
 
            // Start around frame.sp.
            var lo = frame.sp;
            var hi = frame.sp; 
            // Expand to include frame.fp.
            if (frame.fp != 0L && frame.fp < lo)
            {
                lo = frame.fp;
            }

            if (frame.fp != 0L && frame.fp > hi)
            {
                hi = frame.fp;
            } 
            // Expand a bit more.
            lo = lo - expand;
            hi = hi + expand; 
            // But don't go too far from frame.sp.
            if (lo < frame.sp - maxExpand)
            {
                lo = frame.sp - maxExpand;
            }

            if (hi > frame.sp + maxExpand)
            {
                hi = frame.sp + maxExpand;
            } 
            // And don't go outside the stack bounds.
            if (lo < stk.lo)
            {
                lo = stk.lo;
            }

            if (hi > stk.hi)
            {
                hi = stk.hi;
            } 

            // Print the hex dump.
            print("stack: frame={sp:", hex(frame.sp), ", fp:", hex(frame.fp), "} stack=[", hex(stk.lo), ",", hex(stk.hi), ")\n");
            hexdumpWords(lo, hi, p =>
            {

                if (p == frame.fp) 
                    return '>';
                else if (p == frame.sp) 
                    return '<';
                else if (p == bad) 
                    return '!';
                                return 0L;

            });

        }

        // Does f mark the top of a goroutine stack?
        private static bool topofstack(funcInfo f, bool g0)
        {
            return f.funcID == funcID_goexit || f.funcID == funcID_mstart || f.funcID == funcID_mcall || f.funcID == funcID_morestack || f.funcID == funcID_rt0_go || f.funcID == funcID_externalthreadhandler || (g0 && f.funcID == funcID_asmcgocall);
        }

        // isSystemGoroutine reports whether the goroutine g must be omitted
        // in stack dumps and deadlock detector. This is any goroutine that
        // starts at a runtime.* entry point, except for runtime.main,
        // runtime.handleAsyncEvent (wasm only) and sometimes runtime.runfinq.
        //
        // If fixed is true, any goroutine that can vary between user and
        // system (that is, the finalizer goroutine) is considered a user
        // goroutine.
        private static bool isSystemGoroutine(ptr<g> _addr_gp, bool @fixed)
        {
            ref g gp = ref _addr_gp.val;
 
            // Keep this in sync with cmd/trace/trace.go:isSystemGoroutine.
            var f = findfunc(gp.startpc);
            if (!f.valid())
            {
                return false;
            }

            if (f.funcID == funcID_runtime_main || f.funcID == funcID_handleAsyncEvent)
            {
                return false;
            }

            if (f.funcID == funcID_runfinq)
            { 
                // We include the finalizer goroutine if it's calling
                // back into user code.
                if (fixed)
                { 
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
        public static void SetCgoTraceback(long version, unsafe.Pointer traceback, unsafe.Pointer context, unsafe.Pointer symbolizer) => func((_, panic, __) =>
        {
            if (version != 0L)
            {
                panic("unsupported version");
            }

            if (cgoTraceback != null && cgoTraceback != traceback || cgoContext != null && cgoContext != context || cgoSymbolizer != null && cgoSymbolizer != symbolizer)
            {
                panic("call SetCgoTraceback only once");
            }

            cgoTraceback = traceback;
            cgoContext = context;
            cgoSymbolizer = symbolizer; 

            // The context function is called when a C function calls a Go
            // function. As such it is only called by C code in runtime/cgo.
            if (_cgo_set_context_function != null)
            {
                cgocall(_cgo_set_context_function, context);
            }

        });

        private static unsafe.Pointer cgoTraceback = default;
        private static unsafe.Pointer cgoContext = default;
        private static unsafe.Pointer cgoSymbolizer = default;

        // cgoTracebackArg is the type passed to cgoTraceback.
        private partial struct cgoTracebackArg
        {
            public System.UIntPtr context;
            public System.UIntPtr sigContext;
            public ptr<System.UIntPtr> buf;
            public System.UIntPtr max;
        }

        // cgoContextArg is the type passed to the context function.
        private partial struct cgoContextArg
        {
            public System.UIntPtr context;
        }

        // cgoSymbolizerArg is the type passed to cgoSymbolizer.
        private partial struct cgoSymbolizerArg
        {
            public System.UIntPtr pc;
            public ptr<byte> file;
            public System.UIntPtr lineno;
            public ptr<byte> funcName;
            public System.UIntPtr entry;
            public System.UIntPtr more;
            public System.UIntPtr data;
        }

        // cgoTraceback prints a traceback of callers.
        private static void printCgoTraceback(ptr<cgoCallers> _addr_callers)
        {
            ref cgoCallers callers = ref _addr_callers.val;

            if (cgoSymbolizer == null)
            {
                {
                    var c__prev1 = c;

                    foreach (var (_, __c) in callers)
                    {
                        c = __c;
                        if (c == 0L)
                        {
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

                foreach (var (_, __c) in callers)
                {
                    c = __c;
                    if (c == 0L)
                    {
                        break;
                    }

                    printOneCgoTraceback(c, 0x7fffffffUL, _addr_arg);

                }

                c = c__prev1;
            }

            arg.pc = 0L;
            callCgoSymbolizer(_addr_arg);

        }

        // printOneCgoTraceback prints the traceback of a single cgo caller.
        // This can print more than one line because of inlining.
        // Returns the number of frames printed.
        private static long printOneCgoTraceback(System.UIntPtr pc, long max, ptr<cgoSymbolizerArg> _addr_arg)
        {
            ref cgoSymbolizerArg arg = ref _addr_arg.val;

            long c = 0L;
            arg.pc = pc;
            while (c <= max)
            {
                callCgoSymbolizer(_addr_arg);
                if (arg.funcName != null)
                { 
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
                if (arg.file != null)
                {
                    print(gostringnocopy(arg.file), ":", arg.lineno, " ");
                }

                print("pc=", hex(pc), "\n");
                c++;
                if (arg.more == 0L)
                {
                    break;
                }

            }

            return c;

        }

        // callCgoSymbolizer calls the cgoSymbolizer function.
        private static void callCgoSymbolizer(ptr<cgoSymbolizerArg> _addr_arg)
        {
            ref cgoSymbolizerArg arg = ref _addr_arg.val;

            var call = cgocall;
            if (panicking > 0L || getg().m.curg != getg())
            { 
                // We do not want to call into the scheduler when panicking
                // or when on the system stack.
                call = asmcgocall;

            }

            if (msanenabled)
            {
                msanwrite(@unsafe.Pointer(arg), @unsafe.Sizeof(new cgoSymbolizerArg()));
            }

            call(cgoSymbolizer, noescape(@unsafe.Pointer(arg)));

        }

        // cgoContextPCs gets the PC values from a cgo traceback.
        private static void cgoContextPCs(System.UIntPtr ctxt, slice<System.UIntPtr> buf)
        {
            if (cgoTraceback == null)
            {
                return ;
            }

            var call = cgocall;
            if (panicking > 0L || getg().m.curg != getg())
            { 
                // We do not want to call into the scheduler when panicking
                // or when on the system stack.
                call = asmcgocall;

            }

            ref cgoTracebackArg arg = ref heap(new cgoTracebackArg(context:ctxt,buf:(*uintptr)(noescape(unsafe.Pointer(&buf[0]))),max:uintptr(len(buf)),), out ptr<cgoTracebackArg> _addr_arg);
            if (msanenabled)
            {
                msanwrite(@unsafe.Pointer(_addr_arg), @unsafe.Sizeof(arg));
            }

            call(cgoTraceback, noescape(@unsafe.Pointer(_addr_arg)));

        }
    }
}
