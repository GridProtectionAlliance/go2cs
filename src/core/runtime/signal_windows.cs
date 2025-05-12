// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using runtime.@internal;

partial class runtime_package {

internal static readonly UntypedInt _SEM_FAILCRITICALERRORS = /* 0x0001 */ 1;
internal static readonly UntypedInt _SEM_NOGPFAULTERRORBOX = /* 0x0002 */ 2;
internal static readonly UntypedInt _SEM_NOOPENFILEERRORBOX = /* 0x8000 */ 32768;
internal static readonly UntypedInt _WER_FAULT_REPORTING_NO_UI = /* 0x0020 */ 32;

internal static void preventErrorDialogs() {
    var errormode = stdcall0(_GetErrorMode);
    stdcall1(_SetErrorMode, (uintptr)((uintptr)((uintptr)(errormode | _SEM_FAILCRITICALERRORS) | _SEM_NOGPFAULTERRORBOX) | _SEM_NOOPENFILEERRORBOX));
    // Disable WER fault reporting UI.
    // Do this even if WER is disabled as a whole,
    // as WER might be enabled later with setTraceback("wer")
    // and we still want the fault reporting UI to be disabled if this happens.
    ref var werflags = ref heap(new uintptr(), out var Ꮡwerflags);
    stdcall2(_WerGetFlags, currentProcess, ((uintptr)((@unsafe.Pointer)(Ꮡwerflags))));
    stdcall1(_WerSetFlags, (uintptr)(werflags | _WER_FAULT_REPORTING_NO_UI));
}

// enableWER re-enables Windows error reporting without fault reporting UI.
internal static void enableWER() {
    // re-enable Windows Error Reporting
    var errormode = stdcall0(_GetErrorMode);
    if ((uintptr)(errormode & _SEM_NOGPFAULTERRORBOX) != 0) {
        stdcall1(_SetErrorMode, (uintptr)(errormode ^ _SEM_NOGPFAULTERRORBOX));
    }
}

// in sys_windows_386.s, sys_windows_amd64.s, sys_windows_arm.s, and sys_windows_arm64.s
internal static partial void exceptiontramp();

internal static partial void firstcontinuetramp();

internal static partial void lastcontinuetramp();

internal static partial void sehtramp();

internal static partial void sigresume();

internal static void initExceptionHandler() {
    stdcall2(_AddVectoredExceptionHandler, 1, abi.FuncPCABI0(exceptiontramp));
    if (GOARCH == "386"u8){
        // use SetUnhandledExceptionFilter for windows-386.
        // note: SetUnhandledExceptionFilter handler won't be called, if debugging.
        stdcall1(_SetUnhandledExceptionFilter, abi.FuncPCABI0(lastcontinuetramp));
    } else {
        stdcall2(_AddVectoredContinueHandler, 1, abi.FuncPCABI0(firstcontinuetramp));
        stdcall2(_AddVectoredContinueHandler, 0, abi.FuncPCABI0(lastcontinuetramp));
    }
}

// isAbort returns true, if context r describes exception raised
// by calling runtime.abort function.
//
//go:nosplit
internal static bool isAbort(ж<context> Ꮡr) {
    ref var r = ref Ꮡr.val;

    var pc = r.ip();
    if (GOARCH == "386"u8 || GOARCH == "amd64"u8 || GOARCH == "arm"u8) {
        // In the case of an abort, the exception IP is one byte after
        // the INT3 (this differs from UNIX OSes). Note that on ARM,
        // this means that the exception IP is no longer aligned.
        pc--;
    }
    return isAbortPC(pc);
}

// isgoexception reports whether this exception should be translated
// into a Go panic or throw.
//
// It is nosplit to avoid growing the stack in case we're aborting
// because of a stack overflow.
//
//go:nosplit
internal static bool isgoexception(ж<exceptionrecord> Ꮡinfo, ж<context> Ꮡr) {
    ref var info = ref Ꮡinfo.val;
    ref var r = ref Ꮡr.val;

    // Only handle exception if executing instructions in Go binary
    // (not Windows library code).
    // TODO(mwhudson): needs to loop to support shared libs
    if (r.ip() < firstmoduledata.text || firstmoduledata.etext < r.ip()) {
        return false;
    }
    // Go will only handle some exceptions.
    var exprᴛ1 = info.exceptioncode;
    { /* default: */
        return false;
    }
    if (exprᴛ1 == _EXCEPTION_ACCESS_VIOLATION) {
    }
    if (exprᴛ1 == _EXCEPTION_IN_PAGE_ERROR) {
    }
    if (exprᴛ1 == _EXCEPTION_INT_DIVIDE_BY_ZERO) {
    }
    if (exprᴛ1 == _EXCEPTION_INT_OVERFLOW) {
    }
    if (exprᴛ1 == _EXCEPTION_FLT_DENORMAL_OPERAND) {
    }
    if (exprᴛ1 == _EXCEPTION_FLT_DIVIDE_BY_ZERO) {
    }
    if (exprᴛ1 == _EXCEPTION_FLT_INEXACT_RESULT) {
    }
    if (exprᴛ1 == _EXCEPTION_FLT_OVERFLOW) {
    }
    if (exprᴛ1 == _EXCEPTION_FLT_UNDERFLOW) {
    }
    if (exprᴛ1 == _EXCEPTION_BREAKPOINT) {
    }
    if (exprᴛ1 == _EXCEPTION_ILLEGAL_INSTRUCTION) {
    }

    // breakpoint arrives this way on arm64
    return true;
}

internal static readonly UntypedInt callbackVEH = iota;
internal static readonly UntypedInt callbackFirstVCH = 1;
internal static readonly UntypedInt callbackLastVCH = 2;

// sigFetchGSafe is like getg() but without panicking
// when TLS is not set.
// Only implemented on windows/386, which is the only
// arch that loads TLS when calling getg(). Others
// use a dedicated register.
internal static partial ж<g> sigFetchGSafe();

internal static ж<g> sigFetchG() {
    if (GOARCH == "386"u8) {
        return sigFetchGSafe();
    }
    return getg();
}

// sigtrampgo is called from the exception handler function, sigtramp,
// written in assembly code.
// Return EXCEPTION_CONTINUE_EXECUTION if the exception is handled,
// else return EXCEPTION_CONTINUE_SEARCH.
//
// It is nosplit for the same reason as exceptionhandler.
//
//go:nosplit
internal static int32 sigtrampgo(ж<exceptionpointers> Ꮡep, nint kind) {
    ref var ep = ref Ꮡep.val;

    var gp = sigFetchG();
    if (gp == nil) {
        return _EXCEPTION_CONTINUE_SEARCH;
    }
    Func<ж<exceptionrecord>, ж<runtime.context>, ж<runtime.g>, int32> fn = default!;
    var exprᴛ1 = kind;
    if (exprᴛ1 == callbackVEH) {
        fn = exceptionhandler;
    }
    else if (exprᴛ1 == callbackFirstVCH) {
        fn = firstcontinuehandler;
    }
    else if (exprᴛ1 == callbackLastVCH) {
        fn = lastcontinuehandler;
    }
    else { /* default: */
        @throw("unknown sigtramp callback"u8);
    }

    // Check if we are running on g0 stack, and if we are,
    // call fn directly instead of creating the closure.
    // for the systemstack argument.
    //
    // A closure can't be marked as nosplit, so it might
    // call morestack if we are at the g0 stack limit.
    // If that happens, the runtime will call abort
    // and end up in sigtrampgo again.
    // TODO: revisit this workaround if/when closures
    // can be compiled as nosplit.
    //
    // Note that this scenario should only occur on
    // TestG0StackOverflow. Any other occurrence should
    // be treated as a bug.
    int32 ret = default!;
    if (gp != (~(~gp).m).g0){
        systemstack(
        var fnʗ2 = fn;
        var gpʗ2 = gp;
        () => {
            ret = fnʗ2(ep.record, ep.context, gpʗ2);
        });
    } else {
        ret = fn(ep.record, ep.context, gp);
    }
    if (ret == _EXCEPTION_CONTINUE_SEARCH) {
        return ret;
    }
    // Check if we need to set up the control flow guard workaround.
    // On Windows, the stack pointer in the context must lie within
    // system stack limits when we resume from exception.
    // Store the resume SP and PC in alternate registers
    // and return to sigresume on the g0 stack.
    // sigresume makes no use of the stack at all,
    // loading SP from RX and jumping to RY, being RX and RY two scratch registers.
    // Note that blindly smashing RX and RY is only safe because we know sigpanic
    // will not actually return to the original frame, so the registers
    // are effectively dead. But this does mean we can't use the
    // same mechanism for async preemption.
    if (ep.context.ip() == abi.FuncPCABI0(sigresume)) {
        // sigresume has already been set up by a previous exception.
        return ret;
    }
    prepareContextForSigResume(ep.context);
    ep.context.set_sp((~(~(~gp).m).g0).sched.sp);
    ep.context.set_ip(abi.FuncPCABI0(sigresume));
    return ret;
}

// Called by sigtramp from Windows VEH handler.
// Return value signals whether the exception has been handled (EXCEPTION_CONTINUE_EXECUTION)
// or should be made available to other handlers in the chain (EXCEPTION_CONTINUE_SEARCH).
//
// This is nosplit to avoid growing the stack until we've checked for
// _EXCEPTION_BREAKPOINT, which is raised by abort() if we overflow the g0 stack.
//
//go:nosplit
internal static int32 exceptionhandler(ж<exceptionrecord> Ꮡinfo, ж<context> Ꮡr, ж<g> Ꮡgp) {
    ref var info = ref Ꮡinfo.val;
    ref var r = ref Ꮡr.val;
    ref var gp = ref Ꮡgp.val;

    if (!isgoexception(Ꮡinfo, Ꮡr)) {
        return _EXCEPTION_CONTINUE_SEARCH;
    }
    if (gp.throwsplit || isAbort(Ꮡr)) {
        // We can't safely sigpanic because it may grow the stack.
        // Or this is a call to abort.
        // Don't go through any more of the Windows handler chain.
        // Crash now.
        winthrow(Ꮡinfo, Ꮡr, Ꮡgp);
    }
    // After this point, it is safe to grow the stack.
    // Make it look like a call to the signal func.
    // Have to pass arguments out of band since
    // augmenting the stack frame would break
    // the unwinding code.
    gp.sig = info.exceptioncode;
    gp.sigcode0 = info.exceptioninformation[0];
    gp.sigcode1 = info.exceptioninformation[1];
    gp.sigpc = r.ip();
    // Only push runtime·sigpanic if r.ip() != 0.
    // If r.ip() == 0, probably panicked because of a
    // call to a nil func. Not pushing that onto sp will
    // make the trace look like a call to runtime·sigpanic instead.
    // (Otherwise the trace will end at runtime·sigpanic and we
    // won't get to see who faulted.)
    // Also don't push a sigpanic frame if the faulting PC
    // is the entry of asyncPreempt. In this case, we suspended
    // the thread right between the fault and the exception handler
    // starting to run, and we have pushed an asyncPreempt call.
    // The exception is not from asyncPreempt, so not to push a
    // sigpanic call to make it look like that. Instead, just
    // overwrite the PC. (See issue #35773)
    if (r.ip() != 0 && r.ip() != abi.FuncPCABI0(asyncPreempt)) {
        @unsafe.Pointer sp = ((@unsafe.Pointer)r.sp());
        var delta = ((uintptr)sys.StackAlign);
        sp = (uintptr)add(sp, -delta);
        r.set_sp(((uintptr)sp));
        if (usesLR){
            (((ж<uintptr>)sp)).val = r.lr();
            r.set_lr(r.ip());
        } else {
            (((ж<uintptr>)sp)).val = r.ip();
        }
    }
    r.set_ip(abi.FuncPCABI0(sigpanic0));
    return _EXCEPTION_CONTINUE_EXECUTION;
}

// sehhandler is reached as part of the SEH chain.
//
// It is nosplit for the same reason as exceptionhandler.
//
//go:nosplit
internal static int32 sehhandler(ж<exceptionrecord> Ꮡ_, uint64 _, ж<context> Ꮡ_, ж<_DISPATCHER_CONTEXT> Ꮡdctxt) {
    ref var _ = ref Ꮡ_.val;
    ref var _ = ref Ꮡ_.val;
    ref var dctxt = ref Ꮡdctxt.val;

    var g0 = getg();
    if (g0 == nil || (~(~g0).m).curg == nil) {
        // No g available, nothing to do here.
        return _EXCEPTION_CONTINUE_SEARCH_SEH;
    }
    // The Windows SEH machinery will unwind the stack until it finds
    // a frame with a handler for the exception or until the frame is
    // outside the stack boundaries, in which case it will call the
    // UnhandledExceptionFilter. Unfortunately, it doesn't know about
    // the goroutine stack, so it will stop unwinding when it reaches the
    // first frame not running in g0. As a result, neither non-Go exceptions
    // handlers higher up the stack nor UnhandledExceptionFilter will be called.
    //
    // To work around this, manually unwind the stack until the top of the goroutine
    // stack is reached, and then pass the control back to Windows.
    var gp = (~g0).m.val.curg;
    var ctxt = dctxt.ctx();
    ref var base = ref heap(new uintptr(), out var Ꮡbase);
    ref var sp = ref heap(new uintptr(), out var Ꮡsp);
    while (ᐧ) {
        var entry = stdcall3(_RtlLookupFunctionEntry, ctxt.ip(), ((uintptr)((@unsafe.Pointer)(Ꮡ@base))), 0);
        if (entry == 0) {
            break;
        }
        stdcall8(_RtlVirtualUnwind, 0, @base, ctxt.ip(), entry, ((uintptr)new @unsafe.Pointer(ctxt)), 0, ((uintptr)((@unsafe.Pointer)(Ꮡsp))), 0);
        if (sp < (~gp).stack.lo || (~gp).stack.hi <= sp) {
            break;
        }
    }
    return _EXCEPTION_CONTINUE_SEARCH_SEH;
}

// It seems Windows searches ContinueHandler's list even
// if ExceptionHandler returns EXCEPTION_CONTINUE_EXECUTION.
// firstcontinuehandler will stop that search,
// if exceptionhandler did the same earlier.
//
// It is nosplit for the same reason as exceptionhandler.
//
//go:nosplit
internal static int32 firstcontinuehandler(ж<exceptionrecord> Ꮡinfo, ж<context> Ꮡr, ж<g> Ꮡgp) {
    ref var info = ref Ꮡinfo.val;
    ref var r = ref Ꮡr.val;
    ref var gp = ref Ꮡgp.val;

    if (!isgoexception(Ꮡinfo, Ꮡr)) {
        return _EXCEPTION_CONTINUE_SEARCH;
    }
    return _EXCEPTION_CONTINUE_EXECUTION;
}

// lastcontinuehandler is reached, because runtime cannot handle
// current exception. lastcontinuehandler will print crash info and exit.
//
// It is nosplit for the same reason as exceptionhandler.
//
//go:nosplit
internal static int32 lastcontinuehandler(ж<exceptionrecord> Ꮡinfo, ж<context> Ꮡr, ж<g> Ꮡgp) {
    ref var info = ref Ꮡinfo.val;
    ref var r = ref Ꮡr.val;
    ref var gp = ref Ꮡgp.val;

    if (islibrary || isarchive) {
        // Go DLL/archive has been loaded in a non-go program.
        // If the exception does not originate from go, the go runtime
        // should not take responsibility of crashing the process.
        return _EXCEPTION_CONTINUE_SEARCH;
    }
    // VEH is called before SEH, but arm64 MSVC DLLs use SEH to trap
    // illegal instructions during runtime initialization to determine
    // CPU features, so if we make it to the last handler and we're
    // arm64 and it's an illegal instruction and this is coming from
    // non-Go code, then assume it's this runtime probing happen, and
    // pass that onward to SEH.
    if (GOARCH == "arm64"u8 && info.exceptioncode == _EXCEPTION_ILLEGAL_INSTRUCTION && (r.ip() < firstmoduledata.text || firstmoduledata.etext < r.ip())) {
        return _EXCEPTION_CONTINUE_SEARCH;
    }
    winthrow(Ꮡinfo, Ꮡr, Ꮡgp);
    return 0;
}

// not reached

// Always called on g0. gp is the G where the exception occurred.
//
//go:nosplit
internal static void winthrow(ж<exceptionrecord> Ꮡinfo, ж<context> Ꮡr, ж<g> Ꮡgp) {
    ref var info = ref Ꮡinfo.val;
    ref var r = ref Ꮡr.val;
    ref var gp = ref Ꮡgp.val;

    var g0 = getg();
    if (panicking.Load() != 0) {
        // traceback already printed
        exit(2);
    }
    panicking.Store(1);
    // In case we're handling a g0 stack overflow, blow away the
    // g0 stack bounds so we have room to print the traceback. If
    // this somehow overflows the stack, the OS will trap it.
    (~g0).stack.lo = 0;
    g0.val.stackguard0 = (~g0).stack.lo + stackGuard;
    g0.val.stackguard1 = g0.val.stackguard0;
    print("Exception ", ((Δhex)info.exceptioncode), " ", ((Δhex)info.exceptioninformation[0]), " ", ((Δhex)info.exceptioninformation[1]), " ", ((Δhex)r.ip()), "\n");
    print("PC=", ((Δhex)r.ip()), "\n");
    if ((~(~g0).m).incgo && Ꮡgp == (~(~g0).m).g0 && (~(~g0).m).curg != nil) {
        if (iscgo) {
            print("signal arrived during external code execution\n");
        }
        gp = (~g0).m.val.curg;
    }
    print("\n");
    (~g0).m.val.throwing = throwTypeRuntime;
    (~(~g0).m).caughtsig.set(Ꮡgp);
    var (level, _, docrash) = gotraceback();
    if (level > 0) {
        tracebacktrap(r.ip(), r.sp(), r.lr(), Ꮡgp);
        tracebackothers(Ꮡgp);
        dumpregs(Ꮡr);
    }
    if (docrash) {
        dieFromException(Ꮡinfo, Ꮡr);
    }
    exit(2);
}

internal static void sigpanic() {
    var gp = getg();
    if (!canpanic()) {
        @throw("unexpected signal during runtime execution"u8);
    }
    var exprᴛ1 = (~gp).sig;
    if (exprᴛ1 == _EXCEPTION_ACCESS_VIOLATION || exprᴛ1 == _EXCEPTION_IN_PAGE_ERROR) {
        if ((~gp).sigcode1 < 4096) {
            panicmem();
        }
        if ((~gp).paniconfault) {
            panicmemAddr((~gp).sigcode1);
        }
        if (inUserArenaChunk((~gp).sigcode1)){
            // We could check that the arena chunk is explicitly set to fault,
            // but the fact that we faulted on accessing it is enough to prove
            // that it is.
            print("accessed data from freed user arena ", ((Δhex)(~gp).sigcode1), "\n");
        } else {
            print("unexpected fault address ", ((Δhex)(~gp).sigcode1), "\n");
        }
        @throw("fault"u8);
    }
    else if (exprᴛ1 == _EXCEPTION_INT_DIVIDE_BY_ZERO) {
        panicdivide();
    }
    else if (exprᴛ1 == _EXCEPTION_INT_OVERFLOW) {
        panicoverflow();
    }
    else if (exprᴛ1 == _EXCEPTION_FLT_DENORMAL_OPERAND || exprᴛ1 == _EXCEPTION_FLT_DIVIDE_BY_ZERO || exprᴛ1 == _EXCEPTION_FLT_INEXACT_RESULT || exprᴛ1 == _EXCEPTION_FLT_OVERFLOW || exprᴛ1 == _EXCEPTION_FLT_UNDERFLOW) {
        panicfloat();
    }

    @throw("fault"u8);
}

// Following are not implemented.
internal static void initsig(bool preinit) {
}

internal static void sigenable(uint32 sig) {
}

internal static void sigdisable(uint32 sig) {
}

internal static void sigignore(uint32 sig) {
}

internal static @string signame(uint32 sig) {
    return ""u8;
}

//go:nosplit
internal static void crash() {
    dieFromException(nil, nil);
}

// dieFromException raises an exception that bypasses all exception handlers.
// This provides the expected exit status for the shell.
//
//go:nosplit
internal static void dieFromException(ж<exceptionrecord> Ꮡinfo, ж<context> Ꮡr) {
    ref var info = ref Ꮡinfo.val;
    ref var r = ref Ꮡr.val;

    if (info == nil) {
        var gp = getg();
        if ((~gp).sig != 0){
            // Try to reconstruct an exception record from
            // the exception information stored in gp.
            Ꮡinfo = Ꮡ(new exceptionrecord(
                exceptionaddress: (~gp).sigpc,
                exceptioncode: (~gp).sig,
                numberparameters: 2
            )); info = ref Ꮡinfo.val;
            info.exceptioninformation[0] = gp.val.sigcode0;
            info.exceptioninformation[1] = gp.val.sigcode1;
        } else {
            // By default, a failing Go application exits with exit code 2.
            // Use this value when gp does not contain exception info.
            Ꮡinfo = Ꮡ(new exceptionrecord(
                exceptioncode: 2
            )); info = ref Ꮡinfo.val;
        }
    }
    static readonly UntypedInt FAIL_FAST_GENERATE_EXCEPTION_ADDRESS = /* 0x1 */ 1;
    stdcall3(_RaiseFailFastException, ((uintptr)new @unsafe.Pointer(Ꮡinfo)), ((uintptr)new @unsafe.Pointer(Ꮡr)), FAIL_FAST_GENERATE_EXCEPTION_ADDRESS);
}

// gsignalStack is unused on Windows.
[GoType] partial struct gsignalStack {
}

} // end runtime_package
