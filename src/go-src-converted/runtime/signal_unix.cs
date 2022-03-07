// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || solaris
// +build aix darwin dragonfly freebsd linux netbsd openbsd solaris

// package runtime -- go2cs converted at 2022 March 06 22:11:47 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\signal_unix.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using System;
using System.Threading;


namespace go;

public static partial class runtime_package {

    // sigTabT is the type of an entry in the global sigtable array.
    // sigtable is inherently system dependent, and appears in OS-specific files,
    // but sigTabT is the same for all Unixy systems.
    // The sigtable array is indexed by a system signal number to get the flags
    // and printable name of each signal.
private partial struct sigTabT {
    public int flags;
    public @string name;
}

//go:linkname os_sigpipe os.sigpipe
private static void os_sigpipe() {
    systemstack(sigpipe);
}

private static @string signame(uint sig) {
    if (sig >= uint32(len(sigtable))) {
        return "";
    }
    return sigtable[sig].name;

}

private static readonly System.UIntPtr _SIG_DFL = 0;
private static readonly System.UIntPtr _SIG_IGN = 1;


// sigPreempt is the signal used for non-cooperative preemption.
//
// There's no good way to choose this signal, but there are some
// heuristics:
//
// 1. It should be a signal that's passed-through by debuggers by
// default. On Linux, this is SIGALRM, SIGURG, SIGCHLD, SIGIO,
// SIGVTALRM, SIGPROF, and SIGWINCH, plus some glibc-internal signals.
//
// 2. It shouldn't be used internally by libc in mixed Go/C binaries
// because libc may assume it's the only thing that can handle these
// signals. For example SIGCANCEL or SIGSETXID.
//
// 3. It should be a signal that can happen spuriously without
// consequences. For example, SIGALRM is a bad choice because the
// signal handler can't tell if it was caused by the real process
// alarm or not (arguably this means the signal is broken, but I
// digress). SIGUSR1 and SIGUSR2 are also bad because those are often
// used in meaningful ways by applications.
//
// 4. We need to deal with platforms without real-time signals (like
// macOS), so those are out.
//
// We use SIGURG because it meets all of these criteria, is extremely
// unlikely to be used by an application for its "real" meaning (both
// because out-of-band data is basically unused and because SIGURG
// doesn't report which socket has the condition, making it pretty
// useless), and even if it is, the application has to be ready for
// spurious SIGURG. SIGIO wouldn't be a bad choice either, but is more
// likely to be used for real.
private static readonly var sigPreempt = _SIGURG;

// Stores the signal handlers registered before Go installed its own.
// These signal handlers will be invoked in cases where Go doesn't want to
// handle a particular signal (e.g., signal occurred on a non-Go thread).
// See sigfwdgo for more information on when the signals are forwarded.
//
// This is read by the signal handler; accesses should use
// atomic.Loaduintptr and atomic.Storeuintptr.


// Stores the signal handlers registered before Go installed its own.
// These signal handlers will be invoked in cases where Go doesn't want to
// handle a particular signal (e.g., signal occurred on a non-Go thread).
// See sigfwdgo for more information on when the signals are forwarded.
//
// This is read by the signal handler; accesses should use
// atomic.Loaduintptr and atomic.Storeuintptr.
private static array<System.UIntPtr> fwdSig = new array<System.UIntPtr>(_NSIG);

// handlingSig is indexed by signal number and is non-zero if we are
// currently handling the signal. Or, to put it another way, whether
// the signal handler is currently set to the Go signal handler or not.
// This is uint32 rather than bool so that we can use atomic instructions.
private static array<uint> handlingSig = new array<uint>(_NSIG);

// channels for synchronizing signal mask updates with the signal mask
// thread
private static channel<uint> disableSigChan = default;private static channel<uint> enableSigChan = default;private static channel<object> maskUpdatedChan = default;

private static void init() { 
    // _NSIG is the number of signals on this operating system.
    // sigtable should describe what to do for all the possible signals.
    if (len(sigtable) != _NSIG) {
        print("runtime: len(sigtable)=", len(sigtable), " _NSIG=", _NSIG, "\n");
        throw("bad sigtable len");
    }
}

private static bool signalsOK = default;

// Initialize signals.
// Called by libpreinit so runtime may not be initialized.
//go:nosplit
//go:nowritebarrierrec
private static void initsig(bool preinit) {
    if (!preinit) { 
        // It's now OK for signal handlers to run.
        signalsOK = true;

    }
    if ((isarchive || islibrary) && !preinit) {
        return ;
    }
    for (var i = uint32(0); i < _NSIG; i++) {
        var t = _addr_sigtable[i];
        if (t.flags == 0 || t.flags & _SigDefault != 0) {
            continue;
        }
        fwdSig[i] = getsig(i);

        if (!sigInstallGoHandler(i)) { 
            // Even if we are not installing a signal handler,
            // set SA_ONSTACK if necessary.
            if (fwdSig[i] != _SIG_DFL && fwdSig[i] != _SIG_IGN) {
                setsigstack(i);
            }
            else if (fwdSig[i] == _SIG_IGN) {
                sigInitIgnored(i);
            }

            continue;

        }
        handlingSig[i] = 1;
        setsig(i, funcPC(sighandler));

    }

}

//go:nosplit
//go:nowritebarrierrec
private static bool sigInstallGoHandler(uint sig) { 
    // For some signals, we respect an inherited SIG_IGN handler
    // rather than insist on installing our own default handler.
    // Even these signals can be fetched using the os/signal package.

    if (sig == _SIGHUP || sig == _SIGINT) 
        if (atomic.Loaduintptr(_addr_fwdSig[sig]) == _SIG_IGN) {
            return false;
        }
        var t = _addr_sigtable[sig];
    if (t.flags & _SigSetStack != 0) {
        return false;
    }
    if ((isarchive || islibrary) && t.flags & _SigPanic == 0 && sig != _SIGPIPE) {
        return false;
    }
    return true;

}

// sigenable enables the Go signal handler to catch the signal sig.
// It is only called while holding the os/signal.handlers lock,
// via os/signal.enableSignal and signal_enable.
private static void sigenable(uint sig) {
    if (sig >= uint32(len(sigtable))) {
        return ;
    }
    if (sig == _SIGPROF) {
        return ;
    }
    var t = _addr_sigtable[sig];
    if (t.flags & _SigNotify != 0) {
        ensureSigM();
        enableSigChan.Send(sig);
        maskUpdatedChan.Receive();
        if (atomic.Cas(_addr_handlingSig[sig], 0, 1)) {
            atomic.Storeuintptr(_addr_fwdSig[sig], getsig(sig));
            setsig(sig, funcPC(sighandler));
        }
    }
}

// sigdisable disables the Go signal handler for the signal sig.
// It is only called while holding the os/signal.handlers lock,
// via os/signal.disableSignal and signal_disable.
private static void sigdisable(uint sig) {
    if (sig >= uint32(len(sigtable))) {
        return ;
    }
    if (sig == _SIGPROF) {
        return ;
    }
    var t = _addr_sigtable[sig];
    if (t.flags & _SigNotify != 0) {
        ensureSigM();
        disableSigChan.Send(sig);
        maskUpdatedChan.Receive(); 

        // If initsig does not install a signal handler for a
        // signal, then to go back to the state before Notify
        // we should remove the one we installed.
        if (!sigInstallGoHandler(sig)) {
            atomic.Store(_addr_handlingSig[sig], 0);
            setsig(sig, atomic.Loaduintptr(_addr_fwdSig[sig]));
        }
    }
}

// sigignore ignores the signal sig.
// It is only called while holding the os/signal.handlers lock,
// via os/signal.ignoreSignal and signal_ignore.
private static void sigignore(uint sig) {
    if (sig >= uint32(len(sigtable))) {
        return ;
    }
    if (sig == _SIGPROF) {
        return ;
    }
    var t = _addr_sigtable[sig];
    if (t.flags & _SigNotify != 0) {
        atomic.Store(_addr_handlingSig[sig], 0);
        setsig(sig, _SIG_IGN);
    }
}

// clearSignalHandlers clears all signal handlers that are not ignored
// back to the default. This is called by the child after a fork, so that
// we can enable the signal mask for the exec without worrying about
// running a signal handler in the child.
//go:nosplit
//go:nowritebarrierrec
private static void clearSignalHandlers() {
    for (var i = uint32(0); i < _NSIG; i++) {
        if (atomic.Load(_addr_handlingSig[i]) != 0) {
            setsig(i, _SIG_DFL);
        }
    }

}

// setProcessCPUProfiler is called when the profiling timer changes.
// It is called with prof.lock held. hz is the new timer, and is 0 if
// profiling is being disabled. Enable or disable the signal as
// required for -buildmode=c-archive.
private static void setProcessCPUProfiler(int hz) {
    if (hz != 0) { 
        // Enable the Go signal handler if not enabled.
        if (atomic.Cas(_addr_handlingSig[_SIGPROF], 0, 1)) {
            atomic.Storeuintptr(_addr_fwdSig[_SIGPROF], getsig(_SIGPROF));
            setsig(_SIGPROF, funcPC(sighandler));
        }
        ref itimerval it = ref heap(out ptr<itimerval> _addr_it);
        it.it_interval.tv_sec = 0;
        it.it_interval.set_usec(1000000 / hz);
        it.it_value = it.it_interval;
        setitimer(_ITIMER_PROF, _addr_it, null);

    }
    else
 {
        setitimer(_ITIMER_PROF, addr(new itimerval()), null); 

        // If the Go signal handler should be disabled by default,
        // switch back to the signal handler that was installed
        // when we enabled profiling. We don't try to handle the case
        // of a program that changes the SIGPROF handler while Go
        // profiling is enabled.
        //
        // If no signal handler was installed before, then start
        // ignoring SIGPROF signals. We do this, rather than change
        // to SIG_DFL, because there may be a pending SIGPROF
        // signal that has not yet been delivered to some other thread.
        // If we change to SIG_DFL here, the program will crash
        // when that SIGPROF is delivered. We assume that programs
        // that use profiling don't want to crash on a stray SIGPROF.
        // See issue 19320.
        if (!sigInstallGoHandler(_SIGPROF)) {
            if (atomic.Cas(_addr_handlingSig[_SIGPROF], 1, 0)) {
                var h = atomic.Loaduintptr(_addr_fwdSig[_SIGPROF]);
                if (h == _SIG_DFL) {
                    h = _SIG_IGN;
                }
                setsig(_SIGPROF, h);
            }
        }
    }
}

// setThreadCPUProfiler makes any thread-specific changes required to
// implement profiling at a rate of hz.
// No changes required on Unix systems.
private static void setThreadCPUProfiler(int hz) {
    getg().m.profilehz = hz;
}

private static void sigpipe() {
    if (signal_ignored(_SIGPIPE) || sigsend(_SIGPIPE)) {
        return ;
    }
    dieFromSignal(_SIGPIPE);

}

// doSigPreempt handles a preemption signal on gp.
private static void doSigPreempt(ptr<g> _addr_gp, ptr<sigctxt> _addr_ctxt) {
    ref g gp = ref _addr_gp.val;
    ref sigctxt ctxt = ref _addr_ctxt.val;
 
    // Check if this G wants to be preempted and is safe to
    // preempt.
    if (wantAsyncPreempt(gp)) {
        {
            var (ok, newpc) = isAsyncSafePoint(gp, ctxt.sigpc(), ctxt.sigsp(), ctxt.siglr());

            if (ok) { 
                // Adjust the PC and inject a call to asyncPreempt.
                ctxt.pushCall(funcPC(asyncPreempt), newpc);

            }

        }

    }
    atomic.Xadd(_addr_gp.m.preemptGen, 1);
    atomic.Store(_addr_gp.m.signalPending, 0);

    if (GOOS == "darwin" || GOOS == "ios") {
        atomic.Xadd(_addr_pendingPreemptSignals, -1);
    }
}

private static readonly var preemptMSupported = true;

// preemptM sends a preemption request to mp. This request may be
// handled asynchronously and may be coalesced with other requests to
// the M. When the request is received, if the running G or P are
// marked for preemption and the goroutine is at an asynchronous
// safe-point, it will preempt the goroutine. It always atomically
// increments mp.preemptGen after handling a preemption request.


// preemptM sends a preemption request to mp. This request may be
// handled asynchronously and may be coalesced with other requests to
// the M. When the request is received, if the running G or P are
// marked for preemption and the goroutine is at an asynchronous
// safe-point, it will preempt the goroutine. It always atomically
// increments mp.preemptGen after handling a preemption request.
private static void preemptM(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;
 
    // On Darwin, don't try to preempt threads during exec.
    // Issue #41702.
    if (GOOS == "darwin" || GOOS == "ios") {
        execLock.rlock();
    }
    if (atomic.Cas(_addr_mp.signalPending, 0, 1)) {
        if (GOOS == "darwin" || GOOS == "ios") {
            atomic.Xadd(_addr_pendingPreemptSignals, 1);
        }
        signalM(mp, sigPreempt);

    }
    if (GOOS == "darwin" || GOOS == "ios") {
        execLock.runlock();
    }
}

// sigFetchG fetches the value of G safely when running in a signal handler.
// On some architectures, the g value may be clobbered when running in a VDSO.
// See issue #32912.
//
//go:nosplit
private static ptr<g> sigFetchG(ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    switch (GOARCH) {
        case "arm": 

        case "arm64": 

        case "ppc64": 

        case "ppc64le": 
            if (!iscgo && inVDSOPage(c.sigpc())) { 
                // When using cgo, we save the g on TLS and load it from there
                // in sigtramp. Just use that.
                // Otherwise, before making a VDSO call we save the g to the
                // bottom of the signal stack. Fetch from there.
                // TODO: in efence mode, stack is sysAlloc'd, so this wouldn't
                // work.
                var sp = getcallersp();
                var s = spanOf(sp);
                if (s != null && s.state.get() == mSpanManual && s.@base() < sp && sp < s.limit) {
                    ptr<ptr<ptr<g>>> gp = new ptr<ptr<ptr<ptr<g>>>>(@unsafe.Pointer(s.@base()));
                    return _addr_gp!;
                }

                return _addr_null!;

            }
            break;
    }
    return _addr_getg()!;

}

// sigtrampgo is called from the signal handler function, sigtramp,
// written in assembly code.
// This is called by the signal handler, and the world may be stopped.
//
// It must be nosplit because getg() is still the G that was running
// (if any) when the signal was delivered, but it's (usually) called
// on the gsignal stack. Until this switches the G to gsignal, the
// stack bounds check won't work.
//
//go:nosplit
//go:nowritebarrierrec
private static void sigtrampgo(uint sig, ptr<siginfo> _addr_info, unsafe.Pointer ctx) {
    ref siginfo info = ref _addr_info.val;

    if (sigfwdgo(sig, _addr_info, ctx)) {
        return ;
    }
    ptr<sigctxt> c = addr(new sigctxt(info,ctx));
    var g = sigFetchG(c);
    setg(g);
    if (g == null) {
        if (sig == _SIGPROF) {
            sigprofNonGoPC(c.sigpc());
            return ;
        }
        if (sig == sigPreempt && preemptMSupported && debug.asyncpreemptoff == 0) { 
            // This is probably a signal from preemptM sent
            // while executing Go code but received while
            // executing non-Go code.
            // We got past sigfwdgo, so we know that there is
            // no non-Go signal handler for sigPreempt.
            // The default behavior for sigPreempt is to ignore
            // the signal, so badsignal will be a no-op anyway.
            if (GOOS == "darwin" || GOOS == "ios") {
                atomic.Xadd(_addr_pendingPreemptSignals, -1);
            }

            return ;

        }
        c.fixsigcode(sig);
        badsignal(uintptr(sig), c);
        return ;

    }
    setg(g.m.gsignal); 

    // If some non-Go code called sigaltstack, adjust.
    ref gsignalStack gsignalStack = ref heap(out ptr<gsignalStack> _addr_gsignalStack);
    var setStack = adjustSignalStack(sig, _addr_g.m, _addr_gsignalStack);
    if (setStack) {
        g.m.gsignal.stktopsp = getcallersp();
    }
    if (g.stackguard0 == stackFork) {
        signalDuringFork(sig);
    }
    c.fixsigcode(sig);
    sighandler(sig, _addr_info, ctx, _addr_g);
    setg(g);
    if (setStack) {
        restoreGsignalStack(_addr_gsignalStack);
    }
}

// adjustSignalStack adjusts the current stack guard based on the
// stack pointer that is actually in use while handling a signal.
// We do this in case some non-Go code called sigaltstack.
// This reports whether the stack was adjusted, and if so stores the old
// signal stack in *gsigstack.
//go:nosplit
private static bool adjustSignalStack(uint sig, ptr<m> _addr_mp, ptr<gsignalStack> _addr_gsigStack) {
    ref m mp = ref _addr_mp.val;
    ref gsignalStack gsigStack = ref _addr_gsigStack.val;

    var sp = uintptr(@unsafe.Pointer(_addr_sig));
    if (sp >= mp.gsignal.stack.lo && sp < mp.gsignal.stack.hi) {
        return false;
    }
    ref stackt st = ref heap(out ptr<stackt> _addr_st);
    sigaltstack(null, _addr_st);
    var stsp = uintptr(@unsafe.Pointer(st.ss_sp));
    if (st.ss_flags & _SS_DISABLE == 0 && sp >= stsp && sp < stsp + st.ss_size) {
        setGsignalStack(_addr_st, _addr_gsigStack);
        return true;
    }
    if (sp >= mp.g0.stack.lo && sp < mp.g0.stack.hi) { 
        // The signal was delivered on the g0 stack.
        // This can happen when linked with C code
        // using the thread sanitizer, which collects
        // signals then delivers them itself by calling
        // the signal handler directly when C code,
        // including C code called via cgo, calls a
        // TSAN-intercepted function such as malloc.
        //
        // We check this condition last as g0.stack.lo
        // may be not very accurate (see mstart).
        st = new stackt(ss_size:mp.g0.stack.hi-mp.g0.stack.lo);
        setSignalstackSP(_addr_st, mp.g0.stack.lo);
        setGsignalStack(_addr_st, _addr_gsigStack);
        return true;

    }
    setg(null);
    needm();
    if (st.ss_flags & _SS_DISABLE != 0) {
        noSignalStack(sig);
    }
    else
 {
        sigNotOnStack(sig);
    }
    dropm();
    return false;

}

// crashing is the number of m's we have waited for when implementing
// GOTRACEBACK=crash when a signal is received.
private static int crashing = default;

// testSigtrap and testSigusr1 are used by the runtime tests. If
// non-nil, it is called on SIGTRAP/SIGUSR1. If it returns true, the
// normal behavior on this signal is suppressed.
private static Func<ptr<siginfo>, ptr<sigctxt>, ptr<g>, bool> testSigtrap = default;
private static Func<ptr<g>, bool> testSigusr1 = default;

// sighandler is invoked when a signal occurs. The global g will be
// set to a gsignal goroutine and we will be running on the alternate
// signal stack. The parameter g will be the value of the global g
// when the signal occurred. The sig, info, and ctxt parameters are
// from the system signal handler: they are the parameters passed when
// the SA is passed to the sigaction system call.
//
// The garbage collector may have stopped the world, so write barriers
// are not allowed.
//
//go:nowritebarrierrec
private static void sighandler(uint sig, ptr<siginfo> _addr_info, unsafe.Pointer ctxt, ptr<g> _addr_gp) {
    ref siginfo info = ref _addr_info.val;
    ref g gp = ref _addr_gp.val;

    var _g_ = getg();
    ptr<sigctxt> c = addr(new sigctxt(info,ctxt));

    if (sig == _SIGPROF) {
        sigprof(c.sigpc(), c.sigsp(), c.siglr(), gp, _g_.m);
        return ;
    }
    if (sig == _SIGTRAP && testSigtrap != null && testSigtrap(info, (sigctxt.val)(noescape(@unsafe.Pointer(c))), gp)) {
        return ;
    }
    if (sig == _SIGUSR1 && testSigusr1 != null && testSigusr1(gp)) {
        return ;
    }
    if (sig == sigPreempt && debug.asyncpreemptoff == 0) { 
        // Might be a preemption signal.
        doSigPreempt(_addr_gp, c); 
        // Even if this was definitely a preemption signal, it
        // may have been coalesced with another signal, so we
        // still let it through to the application.
    }
    var flags = int32(_SigThrow);
    if (sig < uint32(len(sigtable))) {
        flags = sigtable[sig].flags;
    }
    if (c.sigcode() != _SI_USER && flags & _SigPanic != 0 && gp.throwsplit) { 
        // We can't safely sigpanic because it may grow the
        // stack. Abort in the signal handler instead.
        flags = _SigThrow;

    }
    if (isAbortPC(c.sigpc())) { 
        // On many architectures, the abort function just
        // causes a memory fault. Don't turn that into a panic.
        flags = _SigThrow;

    }
    if (c.sigcode() != _SI_USER && flags & _SigPanic != 0) { 
        // The signal is going to cause a panic.
        // Arrange the stack so that it looks like the point
        // where the signal occurred made a call to the
        // function sigpanic. Then set the PC to sigpanic.

        // Have to pass arguments out of band since
        // augmenting the stack frame would break
        // the unwinding code.
        gp.sig = sig;
        gp.sigcode0 = uintptr(c.sigcode());
        gp.sigcode1 = uintptr(c.fault());
        gp.sigpc = c.sigpc();

        c.preparePanic(sig, gp);
        return ;

    }
    if (c.sigcode() == _SI_USER || flags & _SigNotify != 0) {
        if (sigsend(sig)) {
            return ;
        }
    }
    if (c.sigcode() == _SI_USER && signal_ignored(sig)) {
        return ;
    }
    if (flags & _SigKill != 0) {
        dieFromSignal(sig);
    }
    if (flags & (_SigThrow | _SigPanic) == 0) {
        return ;
    }
    _g_.m.throwing = 1;
    _g_.m.caughtsig.set(gp);

    if (crashing == 0) {
        startpanic_m();
    }
    if (sig < uint32(len(sigtable))) {
        print(sigtable[sig].name, "\n");
    }
    else
 {
        print("Signal ", sig, "\n");
    }
    print("PC=", hex(c.sigpc()), " m=", _g_.m.id, " sigcode=", c.sigcode(), "\n");
    if (_g_.m.lockedg != 0 && _g_.m.ncgo > 0 && gp == _g_.m.g0) {
        print("signal arrived during cgo execution\n");
        gp = _g_.m.lockedg.ptr();
    }
    if (sig == _SIGILL || sig == _SIGFPE) { 
        // It would be nice to know how long the instruction is.
        // Unfortunately, that's complicated to do in general (mostly for x86
        // and s930x, but other archs have non-standard instruction lengths also).
        // Opt to print 16 bytes, which covers most instructions.
        const nint maxN = 16;

        var n = uintptr(maxN); 
        // We have to be careful, though. If we're near the end of
        // a page and the following page isn't mapped, we could
        // segfault. So make sure we don't straddle a page (even though
        // that could lead to printing an incomplete instruction).
        // We're assuming here we can read at least the page containing the PC.
        // I suppose it is possible that the page is mapped executable but not readable?
        var pc = c.sigpc();
        if (n > physPageSize - pc % physPageSize) {
            n = physPageSize - pc % physPageSize;
        }
        print("instruction bytes:");
        ptr<array<byte>> b = new ptr<ptr<array<byte>>>(@unsafe.Pointer(pc));
        for (var i = uintptr(0); i < n; i++) {
            print(" ", hex(b[i]));
        }
        println();

    }
    print("\n");

    var (level, _, docrash) = gotraceback();
    if (level > 0) {
        goroutineheader(gp);
        tracebacktrap(c.sigpc(), c.sigsp(), c.siglr(), gp);
        if (crashing > 0 && gp != _g_.m.curg && _g_.m.curg != null && readgstatus(_g_.m.curg) & ~_Gscan == _Grunning) { 
            // tracebackothers on original m skipped this one; trace it now.
            goroutineheader(_g_.m.curg);
            traceback(~uintptr(0), ~uintptr(0), 0, _g_.m.curg);

        }
        else if (crashing == 0) {
            tracebackothers(gp);
            print("\n");
        }
        dumpregs(c);

    }
    if (docrash) {
        crashing++;
        if (crashing < mcount() - int32(extraMCount)) { 
            // There are other m's that need to dump their stacks.
            // Relay SIGQUIT to the next m by sending it to the current process.
            // All m's that have already received SIGQUIT have signal masks blocking
            // receipt of any signals, so the SIGQUIT will go to an m that hasn't seen it yet.
            // When the last m receives the SIGQUIT, it will fall through to the call to
            // crash below. Just in case the relaying gets botched, each m involved in
            // the relay sleeps for 5 seconds and then does the crash/exit itself.
            // In expected operation, the last m has received the SIGQUIT and run
            // crash/exit and the process is gone, all long before any of the
            // 5-second sleeps have finished.
            print("\n-----\n\n");
            raiseproc(_SIGQUIT);
            usleep(5 * 1000 * 1000);

        }
        crash();

    }
    printDebugLog();

    exit(2);

}

// sigpanic turns a synchronous signal into a run-time panic.
// If the signal handler sees a synchronous panic, it arranges the
// stack to look like the function where the signal occurred called
// sigpanic, sets the signal's PC value to sigpanic, and returns from
// the signal handler. The effect is that the program will act as
// though the function that got the signal simply called sigpanic
// instead.
//
// This must NOT be nosplit because the linker doesn't know where
// sigpanic calls can be injected.
//
// The signal handler must not inject a call to sigpanic if
// getg().throwsplit, since sigpanic may need to grow the stack.
//
// This is exported via linkname to assembly in runtime/cgo.
//go:linkname sigpanic
private static void sigpanic() => func((_, panic, _) => {
    var g = getg();
    if (!canpanic(g)) {
        throw("unexpected signal during runtime execution");
    }

    if (g.sig == _SIGBUS) 
        if (g.sigcode0 == _BUS_ADRERR && g.sigcode1 < 0x1000) {
            panicmem();
        }
        if (g.paniconfault) {
            panicmemAddr(g.sigcode1);
        }
        print("unexpected fault address ", hex(g.sigcode1), "\n");
        throw("fault");
    else if (g.sig == _SIGSEGV) 
        if ((g.sigcode0 == 0 || g.sigcode0 == _SEGV_MAPERR || g.sigcode0 == _SEGV_ACCERR) && g.sigcode1 < 0x1000) {
            panicmem();
        }
        if (g.paniconfault) {
            panicmemAddr(g.sigcode1);
        }
        print("unexpected fault address ", hex(g.sigcode1), "\n");
        throw("fault");
    else if (g.sig == _SIGFPE) 

        if (g.sigcode0 == _FPE_INTDIV) 
            panicdivide();
        else if (g.sigcode0 == _FPE_INTOVF) 
            panicoverflow();
                panicfloat();
        if (g.sig >= uint32(len(sigtable))) { 
        // can't happen: we looked up g.sig in sigtable to decide to call sigpanic
        throw("unexpected signal value");

    }
    panic(errorString(sigtable[g.sig].name));

});

// dieFromSignal kills the program with a signal.
// This provides the expected exit status for the shell.
// This is only called with fatal signals expected to kill the process.
//go:nosplit
//go:nowritebarrierrec
private static void dieFromSignal(uint sig) {
    unblocksig(sig); 
    // Mark the signal as unhandled to ensure it is forwarded.
    atomic.Store(_addr_handlingSig[sig], 0);
    raise(sig); 

    // That should have killed us. On some systems, though, raise
    // sends the signal to the whole process rather than to just
    // the current thread, which means that the signal may not yet
    // have been delivered. Give other threads a chance to run and
    // pick up the signal.
    osyield();
    osyield();
    osyield(); 

    // If that didn't work, try _SIG_DFL.
    setsig(sig, _SIG_DFL);
    raise(sig);

    osyield();
    osyield();
    osyield(); 

    // If we are still somehow running, just exit with the wrong status.
    exit(2);

}

// raisebadsignal is called when a signal is received on a non-Go
// thread, and the Go program does not want to handle it (that is, the
// program has not called os/signal.Notify for the signal).
private static void raisebadsignal(uint sig, ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    if (sig == _SIGPROF) { 
        // Ignore profiling signals that arrive on non-Go threads.
        return ;

    }
    System.UIntPtr handler = default;
    if (sig >= _NSIG) {
        handler = _SIG_DFL;
    }
    else
 {
        handler = atomic.Loaduintptr(_addr_fwdSig[sig]);
    }
    unblocksig(sig);
    setsig(sig, handler); 

    // If we're linked into a non-Go program we want to try to
    // avoid modifying the original context in which the signal
    // was raised. If the handler is the default, we know it
    // is non-recoverable, so we don't have to worry about
    // re-installing sighandler. At this point we can just
    // return and the signal will be re-raised and caught by
    // the default handler with the correct context.
    //
    // On FreeBSD, the libthr sigaction code prevents
    // this from working so we fall through to raise.
    if (GOOS != "freebsd" && (isarchive || islibrary) && handler == _SIG_DFL && c.sigcode() != _SI_USER) {
        return ;
    }
    raise(sig); 

    // Give the signal a chance to be delivered.
    // In almost all real cases the program is about to crash,
    // so sleeping here is not a waste of time.
    usleep(1000); 

    // If the signal didn't cause the program to exit, restore the
    // Go signal handler and carry on.
    //
    // We may receive another instance of the signal before we
    // restore the Go handler, but that is not so bad: we know
    // that the Go program has been ignoring the signal.
    setsig(sig, funcPC(sighandler));

}

//go:nosplit
private static void crash() { 
    // OS X core dumps are linear dumps of the mapped memory,
    // from the first virtual byte to the last, with zeros in the gaps.
    // Because of the way we arrange the address space on 64-bit systems,
    // this means the OS X core file will be >128 GB and even on a zippy
    // workstation can take OS X well over an hour to write (uninterruptible).
    // Save users from making that mistake.
    if (GOOS == "darwin" && GOARCH == "amd64") {
        return ;
    }
    dieFromSignal(_SIGABRT);

}

// ensureSigM starts one global, sleeping thread to make sure at least one thread
// is available to catch signals enabled for os/signal.
private static void ensureSigM() => func((defer, _, _) => {
    if (maskUpdatedChan != null) {
        return ;
    }
    maskUpdatedChan = make_channel<object>();
    disableSigChan = make_channel<uint>();
    enableSigChan = make_channel<uint>();
    go_(() => () => { 
        // Signal masks are per-thread, so make sure this goroutine stays on one
        // thread.
        LockOSThread();
        defer(UnlockOSThread()); 
        // The sigBlocked mask contains the signals not active for os/signal,
        // initially all signals except the essential. When signal.Notify()/Stop is called,
        // sigenable/sigdisable in turn notify this thread to update its signal
        // mask accordingly.
        ref var sigBlocked = ref heap(sigset_all, out ptr<var> _addr_sigBlocked);
        foreach (var (i) in sigtable) {
            if (!blockableSig(uint32(i))) {
                sigdelset(_addr_sigBlocked, i);
            }
        }        sigprocmask(_SIG_SETMASK, _addr_sigBlocked, null);
        while (true) {
            if (sig > 0) {
                sigdelset(_addr_sigBlocked, int(sig));
            }
            if (sig > 0 && blockableSig(sig)) {
                sigaddset(_addr_sigBlocked, int(sig));
            }
            sigprocmask(_SIG_SETMASK, _addr_sigBlocked, null);
            maskUpdatedChan.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
        }

    }());

});

// This is called when we receive a signal when there is no signal stack.
// This can only happen if non-Go code calls sigaltstack to disable the
// signal stack.
private static void noSignalStack(uint sig) {
    println("signal", sig, "received on thread with no signal stack");
    throw("non-Go code disabled sigaltstack");
}

// This is called if we receive a signal when there is a signal stack
// but we are not on it. This can only happen if non-Go code called
// sigaction without setting the SS_ONSTACK flag.
private static void sigNotOnStack(uint sig) {
    println("signal", sig, "received but handler not on signal stack");
    throw("non-Go code set up signal handler without SA_ONSTACK flag");
}

// signalDuringFork is called if we receive a signal while doing a fork.
// We do not want signals at that time, as a signal sent to the process
// group may be delivered to the child process, causing confusion.
// This should never be called, because we block signals across the fork;
// this function is just a safety check. See issue 18600 for background.
private static void signalDuringFork(uint sig) {
    println("signal", sig, "received during fork");
    throw("signal received during fork");
}

private static @string badginsignalMsg = "fatal: bad g in signal handler\n";

// This runs on a foreign stack, without an m or a g. No stack split.
//go:nosplit
//go:norace
//go:nowritebarrierrec
private static void badsignal(System.UIntPtr sig, ptr<sigctxt> _addr_c) {
    ref sigctxt c = ref _addr_c.val;

    if (!iscgo && !cgoHasExtraM) { 
        // There is no extra M. needm will not be able to grab
        // an M. Instead of hanging, just crash.
        // Cannot call split-stack function as there is no G.
        var s = stringStructOf(_addr_badginsignalMsg);
        write(2, s.str, int32(s.len));
        exit(2) * (uintptr.val)(@unsafe.Pointer(uintptr(123)));

        2;

    }
    needm();
    if (!sigsend(uint32(sig))) { 
        // A foreign thread received the signal sig, and the
        // Go code does not want to handle it.
        raisebadsignal(uint32(sig), _addr_c);

    }
    dropm();

}

//go:noescape
private static void sigfwd(System.UIntPtr fn, uint sig, ptr<siginfo> info, unsafe.Pointer ctx);

// Determines if the signal should be handled by Go and if not, forwards the
// signal to the handler that was installed before Go's. Returns whether the
// signal was forwarded.
// This is called by the signal handler, and the world may be stopped.
//go:nosplit
//go:nowritebarrierrec
private static bool sigfwdgo(uint sig, ptr<siginfo> _addr_info, unsafe.Pointer ctx) {
    ref siginfo info = ref _addr_info.val;

    if (sig >= uint32(len(sigtable))) {>>MARKER:FUNCTION_sigfwd_BLOCK_PREFIX<<
        return false;
    }
    var fwdFn = atomic.Loaduintptr(_addr_fwdSig[sig]);
    var flags = sigtable[sig].flags; 

    // If we aren't handling the signal, forward it.
    if (atomic.Load(_addr_handlingSig[sig]) == 0 || !signalsOK) { 
        // If the signal is ignored, doing nothing is the same as forwarding.
        if (fwdFn == _SIG_IGN || (fwdFn == _SIG_DFL && flags & _SigIgn != 0)) {
            return true;
        }
        if (fwdFn == _SIG_DFL) {
            setsig(sig, _SIG_DFL);
            dieFromSignal(sig);
            return false;
        }
        sigfwd(fwdFn, sig, _addr_info, ctx);
        return true;

    }
    if ((GOOS == "darwin" || GOOS == "ios") && sig == _SIGPIPE) {
        return true;
    }
    if (fwdFn == _SIG_DFL) {
        return false;
    }
    ptr<sigctxt> c = addr(new sigctxt(info,ctx)); 
    // Only forward synchronous signals and SIGPIPE.
    // Unfortunately, user generated SIGPIPEs will also be forwarded, because si_code
    // is set to _SI_USER even for a SIGPIPE raised from a write to a closed socket
    // or pipe.
    if ((c.sigcode() == _SI_USER || flags & _SigPanic == 0) && sig != _SIGPIPE) {
        return false;
    }
    var g = sigFetchG(c);
    if (g != null && g.m != null && g.m.curg != null && !g.m.incgo) {
        return false;
    }
    if (fwdFn != _SIG_IGN) {
        sigfwd(fwdFn, sig, _addr_info, ctx);
    }
    return true;

}

// sigsave saves the current thread's signal mask into *p.
// This is used to preserve the non-Go signal mask when a non-Go
// thread calls a Go function.
// This is nosplit and nowritebarrierrec because it is called by needm
// which may be called on a non-Go thread with no g available.
//go:nosplit
//go:nowritebarrierrec
private static void sigsave(ptr<sigset> _addr_p) {
    ref sigset p = ref _addr_p.val;

    sigprocmask(_SIG_SETMASK, null, p);
}

// msigrestore sets the current thread's signal mask to sigmask.
// This is used to restore the non-Go signal mask when a non-Go thread
// calls a Go function.
// This is nosplit and nowritebarrierrec because it is called by dropm
// after g has been cleared.
//go:nosplit
//go:nowritebarrierrec
private static void msigrestore(sigset sigmask) {
    sigprocmask(_SIG_SETMASK, _addr_sigmask, null);
}

// sigsetAllExiting is used by sigblock(true) when a thread is
// exiting. sigset_all is defined in OS specific code, and per GOOS
// behavior may override this default for sigsetAllExiting: see
// osinit().
private static var sigsetAllExiting = sigset_all;

// sigblock blocks signals in the current thread's signal mask.
// This is used to block signals while setting up and tearing down g
// when a non-Go thread calls a Go function. When a thread is exiting
// we use the sigsetAllExiting value, otherwise the OS specific
// definition of sigset_all is used.
// This is nosplit and nowritebarrierrec because it is called by needm
// which may be called on a non-Go thread with no g available.
//go:nosplit
//go:nowritebarrierrec
private static void sigblock(bool exiting) {
    if (exiting) {
        sigprocmask(_SIG_SETMASK, _addr_sigsetAllExiting, null);
        return ;
    }
    sigprocmask(_SIG_SETMASK, _addr_sigset_all, null);

}

// unblocksig removes sig from the current thread's signal mask.
// This is nosplit and nowritebarrierrec because it is called from
// dieFromSignal, which can be called by sigfwdgo while running in the
// signal handler, on the signal stack, with no g available.
//go:nosplit
//go:nowritebarrierrec
private static void unblocksig(uint sig) {
    ref sigset set = ref heap(out ptr<sigset> _addr_set);
    sigaddset(_addr_set, int(sig));
    sigprocmask(_SIG_UNBLOCK, _addr_set, null);
}

// minitSignals is called when initializing a new m to set the
// thread's alternate signal stack and signal mask.
private static void minitSignals() {
    minitSignalStack();
    minitSignalMask();
}

// minitSignalStack is called when initializing a new m to set the
// alternate signal stack. If the alternate signal stack is not set
// for the thread (the normal case) then set the alternate signal
// stack to the gsignal stack. If the alternate signal stack is set
// for the thread (the case when a non-Go thread sets the alternate
// signal stack and then calls a Go function) then set the gsignal
// stack to the alternate signal stack. We also set the alternate
// signal stack to the gsignal stack if cgo is not used (regardless
// of whether it is already set). Record which choice was made in
// newSigstack, so that it can be undone in unminit.
private static void minitSignalStack() {
    var _g_ = getg();
    ref stackt st = ref heap(out ptr<stackt> _addr_st);
    sigaltstack(null, _addr_st);
    if (st.ss_flags & _SS_DISABLE != 0 || !iscgo) {
        signalstack(_addr__g_.m.gsignal.stack);
        _g_.m.newSigstack = true;
    }
    else
 {
        setGsignalStack(_addr_st, _addr__g_.m.goSigStack);
        _g_.m.newSigstack = false;
    }
}

// minitSignalMask is called when initializing a new m to set the
// thread's signal mask. When this is called all signals have been
// blocked for the thread.  This starts with m.sigmask, which was set
// either from initSigmask for a newly created thread or by calling
// sigsave if this is a non-Go thread calling a Go function. It
// removes all essential signals from the mask, thus causing those
// signals to not be blocked. Then it sets the thread's signal mask.
// After this is called the thread can receive signals.
private static void minitSignalMask() {
    ref var nmask = ref heap(getg().m.sigmask, out ptr<var> _addr_nmask);
    foreach (var (i) in sigtable) {
        if (!blockableSig(uint32(i))) {
            sigdelset(_addr_nmask, i);
        }
    }    sigprocmask(_SIG_SETMASK, _addr_nmask, null);

}

// unminitSignals is called from dropm, via unminit, to undo the
// effect of calling minit on a non-Go thread.
//go:nosplit
private static void unminitSignals() {
    if (getg().m.newSigstack) {
        ref stackt st = ref heap(new stackt(ss_flags:_SS_DISABLE), out ptr<stackt> _addr_st);
        sigaltstack(_addr_st, null);
    }
    else
 { 
        // We got the signal stack from someone else. Restore
        // the Go-allocated stack in case this M gets reused
        // for another thread (e.g., it's an extram). Also, on
        // Android, libc allocates a signal stack for all
        // threads, so it's important to restore the Go stack
        // even on Go-created threads so we can free it.
        restoreGsignalStack(_addr_getg().m.goSigStack);

    }
}

// blockableSig reports whether sig may be blocked by the signal mask.
// We never want to block the signals marked _SigUnblock;
// these are the synchronous signals that turn into a Go panic.
// In a Go program--not a c-archive/c-shared--we never want to block
// the signals marked _SigKill or _SigThrow, as otherwise it's possible
// for all running threads to block them and delay their delivery until
// we start a new thread. When linked into a C program we let the C code
// decide on the disposition of those signals.
private static bool blockableSig(uint sig) {
    var flags = sigtable[sig].flags;
    if (flags & _SigUnblock != 0) {
        return false;
    }
    if (isarchive || islibrary) {
        return true;
    }
    return flags & (_SigKill | _SigThrow) == 0;

}

// gsignalStack saves the fields of the gsignal stack changed by
// setGsignalStack.
private partial struct gsignalStack {
    public stack stack;
    public System.UIntPtr stackguard0;
    public System.UIntPtr stackguard1;
    public System.UIntPtr stktopsp;
}

// setGsignalStack sets the gsignal stack of the current m to an
// alternate signal stack returned from the sigaltstack system call.
// It saves the old values in *old for use by restoreGsignalStack.
// This is used when handling a signal if non-Go code has set the
// alternate signal stack.
//go:nosplit
//go:nowritebarrierrec
private static void setGsignalStack(ptr<stackt> _addr_st, ptr<gsignalStack> _addr_old) {
    ref stackt st = ref _addr_st.val;
    ref gsignalStack old = ref _addr_old.val;

    var g = getg();
    if (old != null) {
        old.stack = g.m.gsignal.stack;
        old.stackguard0 = g.m.gsignal.stackguard0;
        old.stackguard1 = g.m.gsignal.stackguard1;
        old.stktopsp = g.m.gsignal.stktopsp;
    }
    var stsp = uintptr(@unsafe.Pointer(st.ss_sp));
    g.m.gsignal.stack.lo = stsp;
    g.m.gsignal.stack.hi = stsp + st.ss_size;
    g.m.gsignal.stackguard0 = stsp + _StackGuard;
    g.m.gsignal.stackguard1 = stsp + _StackGuard;

}

// restoreGsignalStack restores the gsignal stack to the value it had
// before entering the signal handler.
//go:nosplit
//go:nowritebarrierrec
private static void restoreGsignalStack(ptr<gsignalStack> _addr_st) {
    ref gsignalStack st = ref _addr_st.val;

    var gp = getg().m.gsignal;
    gp.stack = st.stack;
    gp.stackguard0 = st.stackguard0;
    gp.stackguard1 = st.stackguard1;
    gp.stktopsp = st.stktopsp;
}

// signalstack sets the current thread's alternate signal stack to s.
//go:nosplit
private static void signalstack(ptr<stack> _addr_s) {
    ref stack s = ref _addr_s.val;

    ref stackt st = ref heap(new stackt(ss_size:s.hi-s.lo), out ptr<stackt> _addr_st);
    setSignalstackSP(_addr_st, s.lo);
    sigaltstack(_addr_st, null);
}

// setsigsegv is used on darwin/arm64 to fake a segmentation fault.
//
// This is exported via linkname to assembly in runtime/cgo.
//
//go:nosplit
//go:linkname setsigsegv
private static void setsigsegv(System.UIntPtr pc) {
    var g = getg();
    g.sig = _SIGSEGV;
    g.sigpc = pc;
    g.sigcode0 = _SEGV_MAPERR;
    g.sigcode1 = 0; // TODO: emulate si_addr
}

} // end runtime_package
