// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build amd64
// +build amd64

// package runtime -- go2cs converted at 2022 March 06 22:08:29 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\debugcall.go
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

private static readonly @string debugCallSystemStack = "executing on Go runtime stack";
private static readonly @string debugCallUnknownFunc = "call from unknown function";
private static readonly @string debugCallRuntime = "call from within the Go runtime";
private static readonly @string debugCallUnsafePoint = "call not at safe point";


private static void debugCallV2();
private static void debugCallPanicked(object val);

// debugCallCheck checks whether it is safe to inject a debugger
// function call with return PC pc. If not, it returns a string
// explaining why.
//
//go:nosplit
private static @string debugCallCheck(System.UIntPtr pc) { 
    // No user calls from the system stack.
    if (getg() != getg().m.curg) {>>MARKER:FUNCTION_debugCallPanicked_BLOCK_PREFIX<<
        return debugCallSystemStack;
    }
    {
        var sp = getcallersp();

        if (!(getg().stack.lo < sp && sp <= getg().stack.hi)) {>>MARKER:FUNCTION_debugCallV2_BLOCK_PREFIX<< 
            // Fast syscalls (nanotime) and racecall switch to the
            // g0 stack without switching g. We can't safely make
            // a call in this state. (We can't even safely
            // systemstack.)
            return debugCallSystemStack;

        }
    } 

    // Switch to the system stack to avoid overflowing the user
    // stack.
    @string ret = default;
    systemstack(() => {
        var f = findfunc(pc);
        if (!f.valid()) {
            ret = debugCallUnknownFunc;
            return ;
        }
        var name = funcname(f);

        switch (name) {
            case "debugCall32": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/

            case "debugCall64": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/

            case "debugCall128": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/

            case "debugCall256": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/

            case "debugCall512": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/

            case "debugCall1024": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/

            case "debugCall2048": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/

            case "debugCall4096": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/

            case "debugCall8192": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/

            case "debugCall16384": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/

            case "debugCall32768": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/

            case "debugCall65536": 
                // These functions are allowed so that the debugger can initiate multiple function calls.
                // See: https://golang.org/cl/161137/
                return ;
                break;
        } 

        // Disallow calls from the runtime. We could
        // potentially make this condition tighter (e.g., not
        // when locks are held), but there are enough tightly
        // coded sequences (e.g., defer handling) that it's
        // better to play it safe.
        {
            @string pfx = "runtime.";

            if (len(name) > len(pfx) && name[..(int)len(pfx)] == pfx) {
                ret = debugCallRuntime;
                return ;
            } 

            // Check that this isn't an unsafe-point.

        } 

        // Check that this isn't an unsafe-point.
        if (pc != f.entry) {
            pc--;
        }
        var up = pcdatavalue(f, _PCDATA_UnsafePoint, pc, null);
        if (up != _PCDATA_UnsafePointSafe) { 
            // Not at a safe point.
            ret = debugCallUnsafePoint;

        }
    });
    return ret;

}

// debugCallWrap starts a new goroutine to run a debug call and blocks
// the calling goroutine. On the goroutine, it prepares to recover
// panics from the debug call, and then calls the call dispatching
// function at PC dispatch.
//
// This must be deeply nosplit because there are untyped values on the
// stack from debugCallV2.
//
//go:nosplit
private static void debugCallWrap(System.UIntPtr dispatch) {
    bool lockedm = default;
    uint lockedExt = default;
    var callerpc = getcallerpc();
    var gp = getg(); 

    // Create a new goroutine to execute the call on. Run this on
    // the system stack to avoid growing our stack.
    systemstack(() => { 
        // TODO(mknyszek): It would be nice to wrap these arguments in an allocated
        // closure and start the goroutine with that closure, but the compiler disallows
        // implicit closure allocation in the runtime.
        ref var fn = ref heap(debugCallWrap1, out ptr<var> _addr_fn);
        var newg = newproc1(new ptr<ptr<ptr<ptr<funcval>>>>(@unsafe.Pointer(_addr_fn)), null, 0, gp, callerpc);
        ptr<debugCallWrapArgs> args = addr(new debugCallWrapArgs(dispatch:dispatch,callingG:gp,));
        newg.param = @unsafe.Pointer(args); 

        // If the current G is locked, then transfer that
        // locked-ness to the new goroutine.
        if (gp.lockedm != 0) { 
            // Save lock state to restore later.
            var mp = gp.m;
            if (mp != gp.lockedm.ptr()) {
                throw("inconsistent lockedm");
            }

            lockedm = true;
            lockedExt = mp.lockedExt; 

            // Transfer external lock count to internal so
            // it can't be unlocked from the debug call.
            mp.lockedInt++;
            mp.lockedExt = 0;

            mp.lockedg.set(newg);
            newg.lockedm.set(mp);
            gp.lockedm = 0;

        }
        gp.asyncSafePoint = true; 

        // Stash newg away so we can execute it below (mcall's
        // closure can't capture anything).
        gp.schedlink.set(newg);

    }); 

    // Switch to the new goroutine.
    mcall(gp => { 
        // Get newg.
        newg = gp.schedlink.ptr();
        gp.schedlink = 0; 

        // Park the calling goroutine.
        gp.waitreason = waitReasonDebugCall;
        if (trace.enabled) {
            traceGoPark(traceEvGoBlock, 1);
        }
        casgstatus(gp, _Grunning, _Gwaiting);
        dropg(); 

        // Directly execute the new goroutine. The debug
        // protocol will continue on the new goroutine, so
        // it's important we not just let the scheduler do
        // this or it may resume a different goroutine.
        execute(newg, true);

    }); 

    // We'll resume here when the call returns.

    // Restore locked state.
    if (lockedm) {
        mp = gp.m;
        mp.lockedExt = lockedExt;
        mp.lockedInt--;
        mp.lockedg.set(gp);
        gp.lockedm.set(mp);
    }
    gp.asyncSafePoint = false;

}

private partial struct debugCallWrapArgs {
    public System.UIntPtr dispatch;
    public ptr<g> callingG;
}

// debugCallWrap1 is the continuation of debugCallWrap on the callee
// goroutine.
private static void debugCallWrap1() {
    var gp = getg();
    var args = (debugCallWrapArgs.val)(gp.param);
    var dispatch = args.dispatch;
    var callingG = args.callingG;
    gp.param = null; 

    // Dispatch call and trap panics.
    debugCallWrap2(dispatch); 

    // Resume the caller goroutine.
    getg().schedlink.set(callingG);
    mcall(gp => {
        callingG = gp.schedlink.ptr();
        gp.schedlink = 0; 

        // Unlock this goroutine from the M if necessary. The
        // calling G will relock.
        if (gp.lockedm != 0) {
            gp.lockedm = 0;
            gp.m.lockedg = 0;
        }
        if (trace.enabled) {
            traceGoSched();
        }
        casgstatus(gp, _Grunning, _Grunnable);
        dropg();
        lock(_addr_sched.@lock);
        globrunqput(gp);
        unlock(_addr_sched.@lock);

        if (trace.enabled) {
            traceGoUnpark(callingG, 0);
        }
        casgstatus(callingG, _Gwaiting, _Grunnable);
        execute(callingG, true);

    });

}

private static void debugCallWrap2(System.UIntPtr dispatch) => func((defer, _, _) => { 
    // Call the dispatch function and trap panics.
    ref Action dispatchF = ref heap(out ptr<Action> _addr_dispatchF);
    ref funcval dispatchFV = ref heap(new funcval(dispatch) * (@unsafe.Pointer.val)(@unsafe.Pointer(_addr_dispatchF)), out ptr<funcval> _addr_dispatchFV);

    noescape(@unsafe.Pointer(_addr_dispatchFV));

    bool ok = default;
    defer(() => {
        if (!ok) {
            var err = recover();
            debugCallPanicked(err);
        }
    }());
    dispatchF();
    ok = true;

});

} // end runtime_package
