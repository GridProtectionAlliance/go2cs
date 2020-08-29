// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:21:27 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\traceback.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
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
        // As an exception, amd64p32 has ptrSize == 4 but the CALL instruction still
        // stores an 8-byte return PC onto the stack. To accommodate this, we use regSize
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
        private static bool tracebackdefers(ref g gp, Func<ref stkframe, unsafe.Pointer, bool> callback, unsafe.Pointer v)
        {
            stkframe frame = default;
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
                        frame.arglen, frame.argmap = getArgInfo(ref frame, f, true, fn);
                    }
                    frame.continpc = frame.pc;
                    if (!callback((stkframe.Value)(noescape(@unsafe.Pointer(ref frame))), v))
                    {
                        return;
                    }
                }

            }
        }

        private static readonly long sizeofSkipFunction = 256L;

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
        private static long gentraceback(System.UIntPtr pc0, System.UIntPtr sp0, System.UIntPtr lr0, ref g gp, long skip, ref System.UIntPtr pcbuf, long max, Func<ref stkframe, unsafe.Pointer, bool> callback, unsafe.Pointer v, ulong flags)
        {
            if (skip > 0L && callback != null)
            {>>MARKER:FUNCTION_skipPleaseUseCallersFrames_BLOCK_PREFIX<<
                throw("gentraceback callback cannot be used with non-zero skip");
            }
            var g = getg();
            if (g == gp && g == g.m.curg)
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
            var (level, _, _) = gotraceback();

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
                }
            }
            long nprint = 0L;
            stkframe frame = default;
            frame.pc = pc0;
            frame.sp = sp0;
            if (usesLR)
            {
                frame.lr = lr0;
            }
            var waspanic = false;
            var cgoCtxt = gp.cgoCtxt;
            var printing = pcbuf == null && callback == null;
            var _defer = gp._defer;
            var elideWrapper = false;

            while (_defer != null && _defer.sp == _NoArgs)
            {
                _defer = _defer.link;
            } 

            // If the PC is zero, it's likely a nil function call.
            // Start in the caller's frame.
 

            // If the PC is zero, it's likely a nil function call.
            // Start in the caller's frame.
            if (frame.pc == 0L)
            {
                if (usesLR)
                {
                    frame.pc = @unsafe.Pointer(frame.sp).Value;
                    frame.lr = 0L;
                }
                else
                {
                    frame.pc = uintptr(@unsafe.Pointer(frame.sp).Value);
                    frame.sp += sys.RegSize;
                }
            }
            var f = findfunc(frame.pc);
            if (!f.valid())
            {
                if (callback != null || printing)
                {
                    print("runtime: unknown pc ", hex(frame.pc), "\n");
                    tracebackHexdump(gp.stack, ref frame, 0L);
                }
                if (callback != null)
                {
                    throw("unknown pc");
                }
                return 0L;
            }
            frame.fn = f;

            pcvalueCache cache = default;

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
                    // We want to jump over the systemstack switch. If we're running on the
                    // g0, this systemstack is at the top of the stack.
                    // if we're not on g0 or there's a no curg, then this is a regular call.
                    var sp = frame.sp;
                    if (flags & _TraceJumpStack != 0L && f.funcID == funcID_systemstack && gp == g.m.g0 && gp.m.curg != null)
                    {
                        sp = gp.m.curg.sched.sp;
                        frame.sp = sp;
                        cgoCtxt = gp.m.curg.cgoCtxt;
                    }
                    frame.fp = sp + uintptr(funcspdelta(f, frame.pc, ref cache));
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
                            frame.lr = @unsafe.Pointer(lrPtr).Value;
                        }
                    }
                    else
                    {
                        if (frame.lr == 0L)
                        {
                            lrPtr = frame.fp - sys.RegSize;
                            frame.lr = uintptr(@unsafe.Pointer(lrPtr).Value);
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
                            tracebackHexdump(gp.stack, ref frame, lrPtr);
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
                if (framepointer_enabled && GOARCH == "amd64" && frame.varp > frame.sp)
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
                    frame.arglen, frame.argmap = getArgInfo(ref frame, f, callback != null, null);
                } 

                // Determine frame's 'continuation PC', where it can continue.
                // Normally this is the return address on the stack, but if sigpanic
                // is immediately below this function on the stack, then the frame
                // stopped executing due to a trap, and frame.pc is probably not
                // a safe point for looking up liveness information. In this panicking case,
                // the function either doesn't return at all (if it has no defers or if the
                // defers do not recover) or it returns from one of the calls to
                // deferproc a second time (if the corresponding deferred func recovers).
                // It suffices to assume that the most recent deferproc is the one that
                // returns; everything live at earlier deferprocs is still live at that one.
                frame.continpc = frame.pc;
                if (waspanic)
                {
                    if (_defer != null && _defer.sp == frame.sp)
                    {
                        frame.continpc = _defer.pc;
                    }
                    else
                    {
                        frame.continpc = 0L;
                    }
                } 

                // Unwind our local defer stack past this frame.
                while (_defer != null && (_defer.sp == frame.sp || _defer.sp == _NoArgs))
                {
                    _defer = _defer.link;
                }


                if (callback != null)
                {
                    if (!callback((stkframe.Value)(noescape(@unsafe.Pointer(ref frame))), v))
                    {
                        return n;
                    }
                }
                if (pcbuf != null)
                {
                    if (skip == 0L)
                    {
                        new ptr<ref array<System.UIntPtr>>(@unsafe.Pointer(pcbuf))[n] = frame.pc;
                    }
                    else
                    { 
                        // backup to CALL instruction to read inlining info (same logic as below)
                        var tracepc = frame.pc;
                        if ((n > 0L || flags & _TraceTrap == 0L) && frame.pc > f.entry && !waspanic)
                        {
                            tracepc--;
                        }
                        var inldata = funcdata(f, _FUNCDATA_InlTree); 

                        // no inlining info, skip the physical frame
                        if (inldata == null)
                        {
                            skip--;
                            goto skipped;
                        }
                        var ix = pcdatavalue(f, _PCDATA_InlTreeIndex, tracepc, ref cache);
                        ref array<inlinedCall> inltree = new ptr<ref array<inlinedCall>>(inldata); 
                        // skip the logical (inlined) frames
                        long logicalSkipped = 0L;
                        while (ix >= 0L && skip > 0L)
                        {
                            skip--;
                            logicalSkipped++;
                            ix = inltree[ix].parent;
                        } 

                        // skip the physical frame if there's more to skip
 

                        // skip the physical frame if there's more to skip
                        if (skip > 0L)
                        {
                            skip--;
                            goto skipped;
                        } 

                        // now we have a partially skipped frame
                        new ptr<ref array<System.UIntPtr>>(@unsafe.Pointer(pcbuf))[n] = frame.pc; 

                        // if there's room, pcbuf[1] is a skip PC that encodes the number of skipped frames in pcbuf[0]
                        if (n + 1L < max)
                        {
                            n++;
                            var pc = skipPC + uintptr(logicalSkipped)(typeof(ref array<System.UIntPtr>))(@unsafe.Pointer(pcbuf))[n];

                            pc;
                        }
                    }
                }
                if (printing)
                { 
                    // assume skip=0 for printing.
                    //
                    // Never elide wrappers if we haven't printed
                    // any frames. And don't elide wrappers that
                    // called panic rather than the wrapped
                    // function. Otherwise, leave them out.
                    var name = funcname(f);
                    var nextElideWrapper = elideWrapperCalling(name);
                    if ((flags & _TraceRuntimeFrames) != 0L || showframe(f, gp, nprint == 0L, elideWrapper && nprint != 0L))
                    { 
                        // Print during crash.
                        //    main(0x1, 0x2, 0x3)
                        //        /home/rsc/go/src/runtime/x.go:23 +0xf
                        //
                        tracepc = frame.pc; // back up to CALL instruction for funcline.
                        if ((n > 0L || flags & _TraceTrap == 0L) && frame.pc > f.entry && !waspanic)
                        {
                            tracepc--;
                        }
                        var (file, line) = funcline(f, tracepc);
                        inldata = funcdata(f, _FUNCDATA_InlTree);
                        if (inldata != null)
                        {
                            inltree = new ptr<ref array<inlinedCall>>(inldata);
                            ix = pcdatavalue(f, _PCDATA_InlTreeIndex, tracepc, null);
                            while (ix != -1L)
                            {
                                name = funcnameFromNameoff(f, inltree[ix].func_);
                                print(name, "(...)\n");
                                print("\t", file, ":", line, "\n");

                                file = funcfile(f, inltree[ix].file);
                                line = inltree[ix].line;
                                ix = inltree[ix].parent;
                            }

                        }
                        if (name == "runtime.gopanic")
                        {
                            name = "panic";
                        }
                        print(name, "(");
                        ref array<System.UIntPtr> argp = new ptr<ref array<System.UIntPtr>>(@unsafe.Pointer(frame.argp));
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
                        if (g.m.throwing > 0L && gp == g.m.curg || level >= 2L)
                        {
                            print(" fp=", hex(frame.fp), " sp=", hex(frame.sp), " pc=", hex(frame.pc));
                        }
                        print("\n");
                        nprint++;
                    }
                    elideWrapper = nextElideWrapper;
                }
                n++;

skipped:

                if (f.funcID == funcID_cgocallback_gofunc && len(cgoCtxt) > 0L)
                {
                    var ctxt = cgoCtxt[len(cgoCtxt) - 1L];
                    cgoCtxt = cgoCtxt[..len(cgoCtxt) - 1L]; 

                    // skip only applies to Go frames.
                    // callback != nil only used when we only care
                    // about Go frames.
                    if (skip == 0L && callback == null)
                    {
                        n = tracebackCgoContext(pcbuf, printing, ctxt, n, max);
                    }
                }
                waspanic = f.funcID == funcID_sigpanic; 

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
                // before faking a call to sigpanic.
                if (usesLR && waspanic)
                {
                    *(*System.UIntPtr) x = @unsafe.Pointer(frame.sp).Value;
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
                    else if (funcspdelta(f, frame.pc, ref cache) == 0L)
                    {
                        frame.lr = x;
                    }
                }
            }


            if (printing)
            {
                n = nprint;
            } 

            // If callback != nil, we're being called to gather stack information during
            // garbage collection or stack growth. In that context, require that we used
            // up the entire defer stack. If not, then there is a bug somewhere and the
            // garbage collection or stack growth may not have seen the correct picture
            // of the stack. Crash now instead of silently executing the garbage collection
            // or stack copy incorrectly and setting up for a mysterious crash later.
            //
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
            // It's okay in those situations not to use up the entire defer stack:
            // incomplete information then is still better than nothing.
            if (callback != null && n < max && _defer != null)
            {
                if (_defer != null)
                {
                    print("runtime: g", gp.goid, ": leftover defer sp=", hex(_defer.sp), " pc=", hex(_defer.pc), "\n");
                }
                _defer = gp._defer;

                while (_defer != null)
                {
                    print("\tdefer ", _defer, " sp=", hex(_defer.sp), " pc=", hex(_defer.pc), "\n");
                    _defer = _defer.link;
                }

                throw("traceback has leftover defers");
            }
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
            public ptr<bitvector> stack; // args bitmap
        }

        // getArgInfo returns the argument frame information for a call to f
        // with call frame frame.
        //
        // This is used for both actual calls with active stack frames and for
        // deferred calls that are not yet executing. If this is an actual
        // call, ctxt must be nil (getArgInfo will retrieve what it needs from
        // the active stack frame). If this is a deferred call, ctxt must be
        // the function object that was deferred.
        private static (System.UIntPtr, ref bitvector) getArgInfo(ref stkframe frame, funcInfo f, bool needArgMap, ref funcval ctxt)
        {
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
                        ref reflectMethodValue mv = default;
                        if (ctxt != null)
                        { 
                            // This is not an actual call, but a
                            // deferred call. The function value
                            // is itself the *reflect.methodValue.
                            mv = (reflectMethodValue.Value)(@unsafe.Pointer(ctxt));
                        }
                        else
                        { 
                            // This is a real call that took the
                            // *reflect.methodValue as its context
                            // register and immediately saved it
                            // to 0(SP). Get the methodValue from
                            // 0(SP).
                            var arg0 = frame.sp + sys.MinFrameSize;
                            mv = new ptr<*(ptr<ptr<reflectMethodValue>>)>(@unsafe.Pointer(arg0));
                        }
                        if (mv.fn != f.entry)
                        {
                            print("runtime: confused by ", funcname(f), "\n");
                            throw("reflect mismatch");
                        }
                        var bv = mv.stack;
                        arglen = uintptr(bv.n * sys.PtrSize);
                        argmap = bv;
                        break;
                }
            }
            return;
        }

        // tracebackCgoContext handles tracing back a cgo context value, from
        // the context argument to setCgoTraceback, for the gentraceback
        // function. It returns the new value of n.
        private static long tracebackCgoContext(ref System.UIntPtr pcbuf, bool printing, System.UIntPtr ctxt, long n, long max)
        {
            array<System.UIntPtr> cgoPCs = new array<System.UIntPtr>(32L);
            cgoContextPCs(ctxt, cgoPCs[..]);
            cgoSymbolizerArg arg = default;
            var anySymbolized = false;
            foreach (var (_, pc) in cgoPCs)
            {
                if (pc == 0L || n >= max)
                {
                    break;
                }
                if (pcbuf != null)
                {
                    new ptr<ref array<System.UIntPtr>>(@unsafe.Pointer(pcbuf))[n] = pc;
                }
                if (printing)
                {
                    if (cgoSymbolizer == null)
                    {
                        print("non-Go function at pc=", hex(pc), "\n");
                    }
                    else
                    {
                        var c = printOneCgoTraceback(pc, max - n, ref arg);
                        n += c - 1L; // +1 a few lines down
                        anySymbolized = true;
                    }
                }
                n++;
            }
            if (anySymbolized)
            {
                arg.pc = 0L;
                callCgoSymbolizer(ref arg);
            }
            return n;
        }

        private static void printcreatedby(ref g gp)
        { 
            // Show what created goroutine, except main goroutine (goid 1).
            var pc = gp.gopc;
            var f = findfunc(pc);
            if (f.valid() && showframe(f, gp, false, false) && gp.goid != 1L)
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
        }

        private static void traceback(System.UIntPtr pc, System.UIntPtr sp, System.UIntPtr lr, ref g gp)
        {
            traceback1(pc, sp, lr, gp, 0L);
        }

        // tracebacktrap is like traceback but expects that the PC and SP were obtained
        // from a trap, not from gp->sched or gp->syscallpc/gp->syscallsp or getcallerpc/getcallersp.
        // Because they are from a trap instead of from a saved pair,
        // the initial PC must not be rewound to the previous instruction.
        // (All the saved pairs record a PC that is a return address, so we
        // rewind it into the CALL instruction.)
        private static void tracebacktrap(System.UIntPtr pc, System.UIntPtr sp, System.UIntPtr lr, ref g gp)
        {
            traceback1(pc, sp, lr, gp, _TraceTrap);
        }

        private static void traceback1(System.UIntPtr pc, System.UIntPtr sp, System.UIntPtr lr, ref g gp, ulong flags)
        { 
            // If the goroutine is in cgo, and we have a cgo traceback, print that.
            if (iscgo && gp.m != null && gp.m.ncgo > 0L && gp.syscallsp != 0L && gp.m.cgoCallers != null && gp.m.cgoCallers[0L] != 0L)
            { 
                // Lock cgoCallers so that a signal handler won't
                // change it, copy the array, reset it, unlock it.
                // We are locked to the thread and are not running
                // concurrently with a signal handler.
                // We just have to stop a signal handler from interrupting
                // in the middle of our copy.
                atomic.Store(ref gp.m.cgoCallersUse, 1L);
                var cgoCallers = gp.m.cgoCallers.Value;
                gp.m.cgoCallers[0L] = 0L;
                atomic.Store(ref gp.m.cgoCallersUse, 0L);

                printCgoTraceback(ref cgoCallers);
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
            n = gentraceback(pc, sp, lr, gp, 0L, null, _TracebackMaxFrames, null, null, flags);
            if (n == 0L && (flags & _TraceRuntimeFrames) == 0L)
            {
                n = gentraceback(pc, sp, lr, gp, 0L, null, _TracebackMaxFrames, null, null, flags | _TraceRuntimeFrames);
            }
            if (n == _TracebackMaxFrames)
            {
                print("...additional frames elided...\n");
            }
            printcreatedby(gp);
        }

        private static long callers(long skip, slice<System.UIntPtr> pcbuf)
        {
            var sp = getcallersp(@unsafe.Pointer(ref skip));
            var pc = getcallerpc();
            var gp = getg();
            long n = default;
            systemstack(() =>
            {
                n = gentraceback(pc, sp, 0L, gp, skip, ref pcbuf[0L], len(pcbuf), null, null, 0L);
            });
            return n;
        }

        private static long gcallers(ref g gp, long skip, slice<System.UIntPtr> pcbuf)
        {
            return gentraceback(~uintptr(0L), ~uintptr(0L), 0L, gp, skip, ref pcbuf[0L], len(pcbuf), null, null, 0L);
        }

        private static bool showframe(funcInfo f, ref g gp, bool firstFrame, bool elideWrapper)
        {
            var g = getg();
            if (g.m.throwing > 0L && gp != null && (gp == g.m.curg || gp == g.m.caughtsig.ptr()))
            {
                return true;
            }
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
            if (elideWrapper)
            {
                var (file, _) = funcline(f, f.entry);
                if (file == "<autogenerated>")
                {
                    return false;
                }
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
            return contains(name, ".") && (!hasprefix(name, "runtime.") || isExportedRuntime(name));
        }

        // isExportedRuntime reports whether name is an exported runtime function.
        // It is only for runtime functions, so ASCII A-Z is fine.
        private static bool isExportedRuntime(@string name)
        {
            const var n = len("runtime.");

            return len(name) > n && name[..n] == "runtime." && 'A' <= name[n] && name[n] <= 'Z';
        }

        // elideWrapperCalling returns whether a wrapper function that called
        // function "name" should be elided from stack traces.
        private static bool elideWrapperCalling(@string name)
        { 
            // If the wrapper called a panic function instead of the
            // wrapped function, we want to include it in stacks.
            return !(name == "runtime.gopanic" || name == "runtime.sigpanic" || name == "runtime.panicwrap");
        }

        private static array<@string> gStatusStrings = new array<@string>(InitKeyedValues<@string>((_Gidle, "idle"), (_Grunnable, "runnable"), (_Grunning, "running"), (_Gsyscall, "syscall"), (_Gwaiting, "waiting"), (_Gdead, "dead"), (_Gcopystack, "copystack")));

        private static void goroutineheader(ref g gp)
        {
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
            if (gpstatus == _Gwaiting && gp.waitreason != "")
            {
                status = gp.waitreason;
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

        private static void tracebackothers(ref g me)
        {
            var (level, _, _) = gotraceback(); 

            // Show the current goroutine first, if we haven't already.
            var g = getg();
            var gp = g.m.curg;
            if (gp != null && gp != me)
            {
                print("\n");
                goroutineheader(gp);
                traceback(~uintptr(0L), ~uintptr(0L), 0L, gp);
            }
            lock(ref allglock);
            {
                var gp__prev1 = gp;

                foreach (var (_, __gp) in allgs)
                {
                    gp = __gp;
                    if (gp == me || gp == g.m.curg || readgstatus(gp) == _Gdead || isSystemGoroutine(gp) && level < 2L)
                    {
                        continue;
                    }
                    print("\n");
                    goroutineheader(gp); 
                    // Note: gp.m == g.m occurs when tracebackothers is
                    // called from a signal handler initiated during a
                    // systemstack call. The original G is still in the
                    // running state, and we want to print its stack.
                    if (gp.m != g.m && readgstatus(gp) & ~_Gscan == _Grunning)
                    {
                        print("\tgoroutine running on other thread; stack unavailable\n");
                        printcreatedby(gp);
                    }
                    else
                    {
                        traceback(~uintptr(0L), ~uintptr(0L), 0L, gp);
                    }
                }

                gp = gp__prev1;
            }

            unlock(ref allglock);
        }

        // tracebackHexdump hexdumps part of stk around frame.sp and frame.fp
        // for debugging purposes. If the address bad is included in the
        // hexdumped range, it will mark it as well.
        private static void tracebackHexdump(stack stk, ref stkframe frame, System.UIntPtr bad)
        {
            const long expand = 32L * sys.PtrSize;

            const long maxExpand = 256L * sys.PtrSize; 
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

        // isSystemGoroutine reports whether the goroutine g must be omitted in
        // stack dumps and deadlock detector.
        private static bool isSystemGoroutine(ref g gp)
        {
            var f = findfunc(gp.startpc);
            if (!f.valid())
            {
                return false;
            }
            return f.funcID == funcID_runfinq && !fingRunning || f.funcID == funcID_bgsweep || f.funcID == funcID_forcegchelper || f.funcID == funcID_timerproc || f.funcID == funcID_gcBgMarkWorker;
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
        private static void printCgoTraceback(ref cgoCallers callers)
        {
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

                return;
            }
            cgoSymbolizerArg arg = default;
            {
                var c__prev1 = c;

                foreach (var (_, __c) in callers)
                {
                    c = __c;
                    if (c == 0L)
                    {
                        break;
                    }
                    printOneCgoTraceback(c, 0x7fffffffUL, ref arg);
                }

                c = c__prev1;
            }

            arg.pc = 0L;
            callCgoSymbolizer(ref arg);
        }

        // printOneCgoTraceback prints the traceback of a single cgo caller.
        // This can print more than one line because of inlining.
        // Returns the number of frames printed.
        private static long printOneCgoTraceback(System.UIntPtr pc, long max, ref cgoSymbolizerArg arg)
        {
            long c = 0L;
            arg.pc = pc;
            while (true)
            {
                if (c > max)
                {
                    break;
                }
                callCgoSymbolizer(arg);
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
        private static void callCgoSymbolizer(ref cgoSymbolizerArg arg)
        {
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
                return;
            }
            var call = cgocall;
            if (panicking > 0L || getg().m.curg != getg())
            { 
                // We do not want to call into the scheduler when panicking
                // or when on the system stack.
                call = asmcgocall;
            }
            cgoTracebackArg arg = new cgoTracebackArg(context:ctxt,buf:(*uintptr)(noescape(unsafe.Pointer(&buf[0]))),max:uintptr(len(buf)),);
            if (msanenabled)
            {
                msanwrite(@unsafe.Pointer(ref arg), @unsafe.Sizeof(arg));
            }
            call(cgoTraceback, noescape(@unsafe.Pointer(ref arg)));
        }
    }
}
