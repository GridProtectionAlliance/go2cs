// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Though the debug call function feature is not enabled on
// ppc64, inserted ppc64 to avoid missing Go declaration error
// for debugCallPanicked while building runtime.test
//go:build amd64 || arm64 || ppc64le || ppc64
namespace go;

using abi = @internal.abi_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

internal static readonly @string debugCallSystemStack = "executing on Go runtime stack"u8;
internal static readonly @string debugCallUnknownFunc = "call from unknown function"u8;
internal static readonly @string debugCallRuntime = "call from within the Go runtime"u8;
internal static readonly @string debugCallUnsafePoint = "call not at safe point"u8;

internal static partial void debugCallV2();

internal static partial void debugCallPanicked(any val);

// debugCallCheck checks whether it is safe to inject a debugger
// function call with return PC pc. If not, it returns a string
// explaining why.
//
//go:nosplit
internal static @string debugCallCheck(uintptr pc) {
    // No user calls from the system stack.
    if (getg() != (~(~getg()).m).curg) {
        return debugCallSystemStack;
    }
    {
        var sp = getcallersp(); if (!((~getg()).stack.lo < sp && sp <= (~getg()).stack.hi)) {
            // Fast syscalls (nanotime) and racecall switch to the
            // g0 stack without switching g. We can't safely make
            // a call in this state. (We can't even safely
            // systemstack.)
            return debugCallSystemStack;
        }
    }
    // Switch to the system stack to avoid overflowing the user
    // stack.
    @string ret = default!;
    systemstack(() => {
        ref var f = ref heap<ΔfuncInfo>(out var Ꮡf);
        f = findfunc(pc);
        if (!f.valid()) {
            ret = debugCallUnknownFunc;
            return;
        }
        @string name = funcname(f);
        var exprᴛ2 = name;
        if (exprᴛ2 == "debugCall32"u8 || exprᴛ2 == "debugCall64"u8 || exprᴛ2 == "debugCall128"u8 || exprᴛ2 == "debugCall256"u8 || exprᴛ2 == "debugCall512"u8 || exprᴛ2 == "debugCall1024"u8 || exprᴛ2 == "debugCall2048"u8 || exprᴛ2 == "debugCall4096"u8 || exprᴛ2 == "debugCall8192"u8 || exprᴛ2 == "debugCall16384"u8 || exprᴛ2 == "debugCall32768"u8 || exprᴛ2 == "debugCall65536"u8) {
            return;
        }

        {
            @string pfx = "runtime."u8; if (len(name) > len(pfx) && name[..(int)(len(pfx))] == pfx) {
                ret = debugCallRuntime;
                return;
            }
        }
        if (pc != f.entry()) {
            pc--;
        }
        var up = pcdatavalue(f, abi.PCDATA_UnsafePoint, pc);
        if (up != abi.UnsafePointSafe) {
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
internal static void debugCallWrap(uintptr dispatch) {
    uint32 lockedExt = default!;
    var callerpc = getcallerpc();
    var gp = getg();
    // Lock ourselves to the OS thread.
    //
    // Debuggers rely on us running on the same thread until we get to
    // dispatch the function they asked as to.
    //
    // We're going to transfer this to the new G we just created.
    lockOSThread();
    // Create a new goroutine to execute the call on. Run this on
    // the system stack to avoid growing our stack.
    systemstack(
    var gpʗ2 = gp;
    () => {
        var fn = debugCallWrap1;
        var newg = newproc1(~(ж<ж<funcval>>)(uintptr)(new @unsafe.Pointer(Ꮡ(fn))), gpʗ2, callerpc, false, waitReasonZero);
        var args = Ꮡ(new debugCallWrapArgs(
            dispatch: dispatch,
            callingG: gpʗ2
        ));
        newg.val.param = new @unsafe.Pointer(args);
        var mpΔ1 = gpʗ2.val.m;
        if (mpΔ1 != (~gpʗ2).lockedm.ptr()) {
            @throw("inconsistent lockedm"u8);
        }
        lockedExt = mpΔ1.val.lockedExt;
        .val.lockedExt = 0;
        (~mpΔ1).lockedg.set(newg);
        (~newg).lockedm.set(mpΔ1);
        gpʗ2.val.lockedm = 0;
        gpʗ2.val.asyncSafePoint = true;
        (~gpʗ2).schedlink.set(newg);
    });
    // Switch to the new goroutine.
    mcall(
    (ж<g> gp) => {
        var newg = (~gpΔ1).schedlink.ptr();
        gp.val.schedlink = 0;
        ref var trace = ref heap<traceLocker>(out var Ꮡtrace);
        Δtrace = traceAcquire();
        if (Δtrace.ok()) {
            Δtrace.GoPark(traceBlockDebugCall, 1);
        }
        casGToWaiting(gpΔ1, _Grunning, waitReasonDebugCall);
        if (Δtrace.ok()) {
            traceRelease(Δtrace);
        }
        dropg();
        execute(newg, true);
    });
    // We'll resume here when the call returns.
    // Restore locked state.
    var mp = gp.val.m;
    mp.val.lockedExt = lockedExt;
    (~mp).lockedg.set(gp);
    (~gp).lockedm.set(mp);
    // Undo the lockOSThread we did earlier.
    unlockOSThread();
    gp.val.asyncSafePoint = false;
}

[GoType] partial struct debugCallWrapArgs {
    internal uintptr dispatch;
    internal ж<g> callingG;
}

// debugCallWrap1 is the continuation of debugCallWrap on the callee
// goroutine.
internal static void debugCallWrap1() {
    var gp = getg();
    var args = (ж<debugCallWrapArgs>)(uintptr)((~gp).param);
    var dispatch = args.val.dispatch;
    var callingG = args.val.callingG;
    gp.val.param = default!;
    // Dispatch call and trap panics.
    debugCallWrap2(dispatch);
    // Resume the caller goroutine.
    (~getg()).schedlink.set(callingG);
    mcall(
    var schedʗ2 = sched;
    (ж<g> gp) => {
        var callingGΔ1 = (~gpΔ1).schedlink.ptr();
        gp.val.schedlink = 0;
        if ((~gpΔ1).lockedm != 0) {
            gp.val.lockedm = 0;
            (~gp).m.val.lockedg = 0;
        }
        ref var trace = ref heap<traceLocker>(out var Ꮡtrace);
        Δtrace = traceAcquire();
        if (Δtrace.ok()) {
            Δtrace.GoSched();
        }
        casgstatus(gpΔ1, _Grunning, _Grunnable);
        if (Δtrace.ok()) {
            traceRelease(Δtrace);
        }
        dropg();
        @lock(Ꮡschedʗ2.of(schedt.Ꮡlock));
        globrunqput(gpΔ1);
        unlock(Ꮡschedʗ2.of(schedt.Ꮡlock));
        Δtrace = traceAcquire();
        casgstatus(callingGΔ1, _Gwaiting, _Grunnable);
        if (Δtrace.ok()) {
            Δtrace.GoUnpark(callingGΔ1, 0);
            traceRelease(Δtrace);
        }
        execute(callingGΔ1, true);
    });
}

internal static void debugCallWrap2(uintptr dispatch) => func((defer, recover) => {
    // Call the dispatch function and trap panics.
    Action dispatchF = default!;
    ref var dispatchFV = ref heap<funcval>(out var ᏑdispatchFV);
    dispatchFV = new funcval(dispatch);
    ((ж<@unsafe.Pointer>)(uintptr)(new @unsafe.Pointer(Ꮡ(dispatchF)))).val = (uintptr)noescape(new @unsafe.Pointer(ᏑdispatchFV));
    bool ok = default!;
    defer(() => {
        if (!ok) {
            var err = recover();
            debugCallPanicked(err);
        }
    });
    dispatchF();
    ok = true;
});

} // end runtime_package
