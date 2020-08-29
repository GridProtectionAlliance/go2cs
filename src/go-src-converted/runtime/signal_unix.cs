// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux netbsd openbsd solaris

// package runtime -- go2cs converted at 2020 August 29 08:20:39 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_unix.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;
using System.Threading;

namespace go
{
    public static partial class runtime_package
    {
        // sigTabT is the type of an entry in the global sigtable array.
        // sigtable is inherently system dependent, and appears in OS-specific files,
        // but sigTabT is the same for all Unixy systems.
        // The sigtable array is indexed by a system signal number to get the flags
        // and printable name of each signal.
        private partial struct sigTabT
        {
            public int flags;
            public @string name;
        }

        //go:linkname os_sigpipe os.sigpipe
        private static void os_sigpipe()
        {
            systemstack(sigpipe);
        }

        private static @string signame(uint sig)
        {
            if (sig >= uint32(len(sigtable)))
            {
                return "";
            }
            return sigtable[sig].name;
        }

        private static readonly System.UIntPtr _SIG_DFL = 0L;
        private static readonly System.UIntPtr _SIG_IGN = 1L;

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
        private static channel<uint> disableSigChan = default;        private static channel<uint> enableSigChan = default;        private static channel<object> maskUpdatedChan = default;

        private static void init()
        { 
            // _NSIG is the number of signals on this operating system.
            // sigtable should describe what to do for all the possible signals.
            if (len(sigtable) != _NSIG)
            {
                print("runtime: len(sigtable)=", len(sigtable), " _NSIG=", _NSIG, "\n");
                throw("bad sigtable len");
            }
        }

        private static bool signalsOK = default;

        // Initialize signals.
        // Called by libpreinit so runtime may not be initialized.
        //go:nosplit
        //go:nowritebarrierrec
        private static void initsig(bool preinit)
        {
            if (!preinit)
            { 
                // It's now OK for signal handlers to run.
                signalsOK = true;
            } 

            // For c-archive/c-shared this is called by libpreinit with
            // preinit == true.
            if ((isarchive || islibrary) && !preinit)
            {
                return;
            }
            for (var i = uint32(0L); i < _NSIG; i++)
            {
                var t = ref sigtable[i];
                if (t.flags == 0L || t.flags & _SigDefault != 0L)
                {
                    continue;
                } 

                // We don't need to use atomic operations here because
                // there shouldn't be any other goroutines running yet.
                fwdSig[i] = getsig(i);

                if (!sigInstallGoHandler(i))
                { 
                    // Even if we are not installing a signal handler,
                    // set SA_ONSTACK if necessary.
                    if (fwdSig[i] != _SIG_DFL && fwdSig[i] != _SIG_IGN)
                    {
                        setsigstack(i);
                    }
                    continue;
                }
                handlingSig[i] = 1L;
                setsig(i, funcPC(sighandler));
            }

        }

        //go:nosplit
        //go:nowritebarrierrec
        private static bool sigInstallGoHandler(uint sig)
        { 
            // For some signals, we respect an inherited SIG_IGN handler
            // rather than insist on installing our own default handler.
            // Even these signals can be fetched using the os/signal package.

            if (sig == _SIGHUP || sig == _SIGINT) 
                if (atomic.Loaduintptr(ref fwdSig[sig]) == _SIG_IGN)
                {
                    return false;
                }
                        var t = ref sigtable[sig];
            if (t.flags & _SigSetStack != 0L)
            {
                return false;
            } 

            // When built using c-archive or c-shared, only install signal
            // handlers for synchronous signals and SIGPIPE.
            if ((isarchive || islibrary) && t.flags & _SigPanic == 0L && sig != _SIGPIPE)
            {
                return false;
            }
            return true;
        }

        // sigenable enables the Go signal handler to catch the signal sig.
        // It is only called while holding the os/signal.handlers lock,
        // via os/signal.enableSignal and signal_enable.
        private static void sigenable(uint sig)
        {
            if (sig >= uint32(len(sigtable)))
            {
                return;
            } 

            // SIGPROF is handled specially for profiling.
            if (sig == _SIGPROF)
            {
                return;
            }
            var t = ref sigtable[sig];
            if (t.flags & _SigNotify != 0L)
            {
                ensureSigM();
                enableSigChan.Send(sig);
                maskUpdatedChan.Receive();
                if (atomic.Cas(ref handlingSig[sig], 0L, 1L))
                {
                    atomic.Storeuintptr(ref fwdSig[sig], getsig(sig));
                    setsig(sig, funcPC(sighandler));
                }
            }
        }

        // sigdisable disables the Go signal handler for the signal sig.
        // It is only called while holding the os/signal.handlers lock,
        // via os/signal.disableSignal and signal_disable.
        private static void sigdisable(uint sig)
        {
            if (sig >= uint32(len(sigtable)))
            {
                return;
            } 

            // SIGPROF is handled specially for profiling.
            if (sig == _SIGPROF)
            {
                return;
            }
            var t = ref sigtable[sig];
            if (t.flags & _SigNotify != 0L)
            {
                ensureSigM();
                disableSigChan.Send(sig);
                maskUpdatedChan.Receive(); 

                // If initsig does not install a signal handler for a
                // signal, then to go back to the state before Notify
                // we should remove the one we installed.
                if (!sigInstallGoHandler(sig))
                {
                    atomic.Store(ref handlingSig[sig], 0L);
                    setsig(sig, atomic.Loaduintptr(ref fwdSig[sig]));
                }
            }
        }

        // sigignore ignores the signal sig.
        // It is only called while holding the os/signal.handlers lock,
        // via os/signal.ignoreSignal and signal_ignore.
        private static void sigignore(uint sig)
        {
            if (sig >= uint32(len(sigtable)))
            {
                return;
            } 

            // SIGPROF is handled specially for profiling.
            if (sig == _SIGPROF)
            {
                return;
            }
            var t = ref sigtable[sig];
            if (t.flags & _SigNotify != 0L)
            {
                atomic.Store(ref handlingSig[sig], 0L);
                setsig(sig, _SIG_IGN);
            }
        }

        // clearSignalHandlers clears all signal handlers that are not ignored
        // back to the default. This is called by the child after a fork, so that
        // we can enable the signal mask for the exec without worrying about
        // running a signal handler in the child.
        //go:nosplit
        //go:nowritebarrierrec
        private static void clearSignalHandlers()
        {
            for (var i = uint32(0L); i < _NSIG; i++)
            {
                if (atomic.Load(ref handlingSig[i]) != 0L)
                {
                    setsig(i, _SIG_DFL);
                }
            }

        }

        // setProcessCPUProfiler is called when the profiling timer changes.
        // It is called with prof.lock held. hz is the new timer, and is 0 if
        // profiling is being disabled. Enable or disable the signal as
        // required for -buildmode=c-archive.
        private static void setProcessCPUProfiler(int hz)
        {
            if (hz != 0L)
            { 
                // Enable the Go signal handler if not enabled.
                if (atomic.Cas(ref handlingSig[_SIGPROF], 0L, 1L))
                {
                    atomic.Storeuintptr(ref fwdSig[_SIGPROF], getsig(_SIGPROF));
                    setsig(_SIGPROF, funcPC(sighandler));
                }
            }
            else
            { 
                // If the Go signal handler should be disabled by default,
                // disable it if it is enabled.
                if (!sigInstallGoHandler(_SIGPROF))
                {
                    if (atomic.Cas(ref handlingSig[_SIGPROF], 1L, 0L))
                    {
                        setsig(_SIGPROF, atomic.Loaduintptr(ref fwdSig[_SIGPROF]));
                    }
                }
            }
        }

        // setThreadCPUProfiler makes any thread-specific changes required to
        // implement profiling at a rate of hz.
        private static void setThreadCPUProfiler(int hz)
        {
            itimerval it = default;
            if (hz == 0L)
            {
                setitimer(_ITIMER_PROF, ref it, null);
            }
            else
            {
                it.it_interval.tv_sec = 0L;
                it.it_interval.set_usec(1000000L / hz);
                it.it_value = it.it_interval;
                setitimer(_ITIMER_PROF, ref it, null);
            }
            var _g_ = getg();
            _g_.m.profilehz = hz;
        }

        private static void sigpipe()
        {
            if (sigsend(_SIGPIPE))
            {
                return;
            }
            dieFromSignal(_SIGPIPE);
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
        private static void sigtrampgo(uint sig, ref siginfo info, unsafe.Pointer ctx)
        {
            if (sigfwdgo(sig, info, ctx))
            {
                return;
            }
            var g = getg();
            if (g == null)
            {
                sigctxt c = ref new sigctxt(info,ctx);
                if (sig == _SIGPROF)
                {
                    sigprofNonGoPC(c.sigpc());
                    return;
                }
                badsignal(uintptr(sig), c);
                return;
            } 

            // If some non-Go code called sigaltstack, adjust.
            var setStack = false;
            gsignalStack gsignalStack = default;
            var sp = uintptr(@unsafe.Pointer(ref sig));
            if (sp < g.m.gsignal.stack.lo || sp >= g.m.gsignal.stack.hi)
            {
                if (sp >= g.m.g0.stack.lo && sp < g.m.g0.stack.hi)
                { 
                    // The signal was delivered on the g0 stack.
                    // This can happen when linked with C code
                    // using the thread sanitizer, which collects
                    // signals then delivers them itself by calling
                    // the signal handler directly when C code,
                    // including C code called via cgo, calls a
                    // TSAN-intercepted function such as malloc.
                    stackt st = new stackt(ss_size:g.m.g0.stack.hi-g.m.g0.stack.lo);
                    setSignalstackSP(ref st, g.m.g0.stack.lo);
                    setGsignalStack(ref st, ref gsignalStack);
                    g.m.gsignal.stktopsp = getcallersp(@unsafe.Pointer(ref sig));
                    setStack = true;
                }
                else
                {
                    st = default;
                    sigaltstack(null, ref st);
                    if (st.ss_flags & _SS_DISABLE != 0L)
                    {
                        setg(null);
                        needm(0L);
                        noSignalStack(sig);
                        dropm();
                    }
                    var stsp = uintptr(@unsafe.Pointer(st.ss_sp));
                    if (sp < stsp || sp >= stsp + st.ss_size)
                    {
                        setg(null);
                        needm(0L);
                        sigNotOnStack(sig);
                        dropm();
                    }
                    setGsignalStack(ref st, ref gsignalStack);
                    g.m.gsignal.stktopsp = getcallersp(@unsafe.Pointer(ref sig));
                    setStack = true;
                }
            }
            setg(g.m.gsignal);

            if (g.stackguard0 == stackFork)
            {
                signalDuringFork(sig);
            }
            c = ref new sigctxt(info,ctx);
            c.fixsigcode(sig);
            sighandler(sig, info, ctx, g);
            setg(g);
            if (setStack)
            {
                restoreGsignalStack(ref gsignalStack);
            }
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
        private static void sigpanic() => func((_, panic, __) =>
        {
            var g = getg();
            if (!canpanic(g))
            {
                throw("unexpected signal during runtime execution");
            }

            if (g.sig == _SIGBUS) 
                if (g.sigcode0 == _BUS_ADRERR && g.sigcode1 < 0x1000UL)
                {
                    panicmem();
                } 
                // Support runtime/debug.SetPanicOnFault.
                if (g.paniconfault)
                {
                    panicmem();
                }
                print("unexpected fault address ", hex(g.sigcode1), "\n");
                throw("fault");
            else if (g.sig == _SIGSEGV) 
                if ((g.sigcode0 == 0L || g.sigcode0 == _SEGV_MAPERR || g.sigcode0 == _SEGV_ACCERR) && g.sigcode1 < 0x1000UL)
                {
                    panicmem();
                } 
                // Support runtime/debug.SetPanicOnFault.
                if (g.paniconfault)
                {
                    panicmem();
                }
                print("unexpected fault address ", hex(g.sigcode1), "\n");
                throw("fault");
            else if (g.sig == _SIGFPE) 

                if (g.sigcode0 == _FPE_INTDIV) 
                    panicdivide();
                else if (g.sigcode0 == _FPE_INTOVF) 
                    panicoverflow();
                                panicfloat();
                        if (g.sig >= uint32(len(sigtable)))
            { 
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
        private static void dieFromSignal(uint sig)
        {
            unblocksig(sig); 
            // Mark the signal as unhandled to ensure it is forwarded.
            atomic.Store(ref handlingSig[sig], 0L);
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

            // On Darwin we may still fail to die, because raise sends the
            // signal to the whole process rather than just the current thread,
            // and osyield just sleeps briefly rather than letting all other
            // threads run. See issue 20315. Sleep longer.
            if (GOOS == "darwin")
            {
                usleep(100L);
            } 

            // If we are still somehow running, just exit with the wrong status.
            exit(2L);
        }

        // raisebadsignal is called when a signal is received on a non-Go
        // thread, and the Go program does not want to handle it (that is, the
        // program has not called os/signal.Notify for the signal).
        private static void raisebadsignal(uint sig, ref sigctxt c)
        {
            if (sig == _SIGPROF)
            { 
                // Ignore profiling signals that arrive on non-Go threads.
                return;
            }
            System.UIntPtr handler = default;
            if (sig >= _NSIG)
            {
                handler = _SIG_DFL;
            }
            else
            {
                handler = atomic.Loaduintptr(ref fwdSig[sig]);
            } 

            // Reset the signal handler and raise the signal.
            // We are currently running inside a signal handler, so the
            // signal is blocked. We need to unblock it before raising the
            // signal, or the signal we raise will be ignored until we return
            // from the signal handler. We know that the signal was unblocked
            // before entering the handler, or else we would not have received
            // it. That means that we don't have to worry about blocking it
            // again.
            unblocksig(sig);
            setsig(sig, handler); 

            // If we're linked into a non-Go program we want to try to
            // avoid modifying the original context in which the signal
            // was raised. If the handler is the default, we know it
            // is non-recoverable, so we don't have to worry about
            // re-installing sighandler. At this point we can just
            // return and the signal will be re-raised and caught by
            // the default handler with the correct context.
            if ((isarchive || islibrary) && handler == _SIG_DFL && c.sigcode() != _SI_USER)
            {
                return;
            }
            raise(sig); 

            // Give the signal a chance to be delivered.
            // In almost all real cases the program is about to crash,
            // so sleeping here is not a waste of time.
            usleep(1000L); 

            // If the signal didn't cause the program to exit, restore the
            // Go signal handler and carry on.
            //
            // We may receive another instance of the signal before we
            // restore the Go handler, but that is not so bad: we know
            // that the Go program has been ignoring the signal.
            setsig(sig, funcPC(sighandler));
        }

        private static void crash()
        {
            if (GOOS == "darwin")
            { 
                // OS X core dumps are linear dumps of the mapped memory,
                // from the first virtual byte to the last, with zeros in the gaps.
                // Because of the way we arrange the address space on 64-bit systems,
                // this means the OS X core file will be >128 GB and even on a zippy
                // workstation can take OS X well over an hour to write (uninterruptible).
                // Save users from making that mistake.
                if (GOARCH == "amd64")
                {
                    return;
                }
            }
            dieFromSignal(_SIGABRT);
        }

        // ensureSigM starts one global, sleeping thread to make sure at least one thread
        // is available to catch signals enabled for os/signal.
        private static void ensureSigM() => func((defer, _, __) =>
        {
            if (maskUpdatedChan != null)
            {
                return;
            }
            maskUpdatedChan = make_channel<object>();
            disableSigChan = make_channel<uint>();
            enableSigChan = make_channel<uint>();
            go_(() => () =>
            { 
                // Signal masks are per-thread, so make sure this goroutine stays on one
                // thread.
                LockOSThread();
                defer(UnlockOSThread()); 
                // The sigBlocked mask contains the signals not active for os/signal,
                // initially all signals except the essential. When signal.Notify()/Stop is called,
                // sigenable/sigdisable in turn notify this thread to update its signal
                // mask accordingly.
                var sigBlocked = sigset_all;
                foreach (var (i) in sigtable)
                {
                    if (!blockableSig(uint32(i)))
                    {
                        sigdelset(ref sigBlocked, i);
                    }
                }
                sigprocmask(_SIG_SETMASK, ref sigBlocked, null);
                while (true)
                {
                    if (sig > 0L)
                    {
                        sigdelset(ref sigBlocked, int(sig));
                    }
                    if (sig > 0L && blockableSig(sig))
                    {
                        sigaddset(ref sigBlocked, int(sig));
                    }
                    sigprocmask(_SIG_SETMASK, ref sigBlocked, null);
                    maskUpdatedChan.Send(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{}{});
                }

            }());
        });

        // This is called when we receive a signal when there is no signal stack.
        // This can only happen if non-Go code calls sigaltstack to disable the
        // signal stack.
        private static void noSignalStack(uint sig)
        {
            println("signal", sig, "received on thread with no signal stack");
            throw("non-Go code disabled sigaltstack");
        }

        // This is called if we receive a signal when there is a signal stack
        // but we are not on it. This can only happen if non-Go code called
        // sigaction without setting the SS_ONSTACK flag.
        private static void sigNotOnStack(uint sig)
        {
            println("signal", sig, "received but handler not on signal stack");
            throw("non-Go code set up signal handler without SA_ONSTACK flag");
        }

        // signalDuringFork is called if we receive a signal while doing a fork.
        // We do not want signals at that time, as a signal sent to the process
        // group may be delivered to the child process, causing confusion.
        // This should never be called, because we block signals across the fork;
        // this function is just a safety check. See issue 18600 for background.
        private static void signalDuringFork(uint sig)
        {
            println("signal", sig, "received during fork");
            throw("signal received during fork");
        }

        // This runs on a foreign stack, without an m or a g. No stack split.
        //go:nosplit
        //go:norace
        //go:nowritebarrierrec
        private static void badsignal(System.UIntPtr sig, ref sigctxt c)
        {
            needm(0L);
            if (!sigsend(uint32(sig)))
            { 
                // A foreign thread received the signal sig, and the
                // Go code does not want to handle it.
                raisebadsignal(uint32(sig), c);
            }
            dropm();
        }

        //go:noescape
        private static void sigfwd(System.UIntPtr fn, uint sig, ref siginfo info, unsafe.Pointer ctx)
;

        // Determines if the signal should be handled by Go and if not, forwards the
        // signal to the handler that was installed before Go's. Returns whether the
        // signal was forwarded.
        // This is called by the signal handler, and the world may be stopped.
        //go:nosplit
        //go:nowritebarrierrec
        private static bool sigfwdgo(uint sig, ref siginfo info, unsafe.Pointer ctx)
        {
            if (sig >= uint32(len(sigtable)))
            {>>MARKER:FUNCTION_sigfwd_BLOCK_PREFIX<<
                return false;
            }
            var fwdFn = atomic.Loaduintptr(ref fwdSig[sig]);
            var flags = sigtable[sig].flags; 

            // If we aren't handling the signal, forward it.
            if (atomic.Load(ref handlingSig[sig]) == 0L || !signalsOK)
            { 
                // If the signal is ignored, doing nothing is the same as forwarding.
                if (fwdFn == _SIG_IGN || (fwdFn == _SIG_DFL && flags & _SigIgn != 0L))
                {
                    return true;
                } 
                // We are not handling the signal and there is no other handler to forward to.
                // Crash with the default behavior.
                if (fwdFn == _SIG_DFL)
                {
                    setsig(sig, _SIG_DFL);
                    dieFromSignal(sig);
                    return false;
                }
                sigfwd(fwdFn, sig, info, ctx);
                return true;
            } 

            // If there is no handler to forward to, no need to forward.
            if (fwdFn == _SIG_DFL)
            {
                return false;
            }
            sigctxt c = ref new sigctxt(info,ctx); 
            // Only forward synchronous signals and SIGPIPE.
            // Unfortunately, user generated SIGPIPEs will also be forwarded, because si_code
            // is set to _SI_USER even for a SIGPIPE raised from a write to a closed socket
            // or pipe.
            if ((c.sigcode() == _SI_USER || flags & _SigPanic == 0L) && sig != _SIGPIPE)
            {
                return false;
            } 
            // Determine if the signal occurred inside Go code. We test that:
            //   (1) we were in a goroutine (i.e., m.curg != nil), and
            //   (2) we weren't in CGO.
            var g = getg();
            if (g != null && g.m != null && g.m.curg != null && !g.m.incgo)
            {
                return false;
            } 

            // Signal not handled by Go, forward it.
            if (fwdFn != _SIG_IGN)
            {
                sigfwd(fwdFn, sig, info, ctx);
            }
            return true;
        }

        // msigsave saves the current thread's signal mask into mp.sigmask.
        // This is used to preserve the non-Go signal mask when a non-Go
        // thread calls a Go function.
        // This is nosplit and nowritebarrierrec because it is called by needm
        // which may be called on a non-Go thread with no g available.
        //go:nosplit
        //go:nowritebarrierrec
        private static void msigsave(ref m mp)
        {
            sigprocmask(_SIG_SETMASK, null, ref mp.sigmask);
        }

        // msigrestore sets the current thread's signal mask to sigmask.
        // This is used to restore the non-Go signal mask when a non-Go thread
        // calls a Go function.
        // This is nosplit and nowritebarrierrec because it is called by dropm
        // after g has been cleared.
        //go:nosplit
        //go:nowritebarrierrec
        private static void msigrestore(sigset sigmask)
        {
            sigprocmask(_SIG_SETMASK, ref sigmask, null);
        }

        // sigblock blocks all signals in the current thread's signal mask.
        // This is used to block signals while setting up and tearing down g
        // when a non-Go thread calls a Go function.
        // The OS-specific code is expected to define sigset_all.
        // This is nosplit and nowritebarrierrec because it is called by needm
        // which may be called on a non-Go thread with no g available.
        //go:nosplit
        //go:nowritebarrierrec
        private static void sigblock()
        {
            sigprocmask(_SIG_SETMASK, ref sigset_all, null);
        }

        // unblocksig removes sig from the current thread's signal mask.
        // This is nosplit and nowritebarrierrec because it is called from
        // dieFromSignal, which can be called by sigfwdgo while running in the
        // signal handler, on the signal stack, with no g available.
        //go:nosplit
        //go:nowritebarrierrec
        private static void unblocksig(uint sig)
        {
            sigset set = default;
            sigaddset(ref set, int(sig));
            sigprocmask(_SIG_UNBLOCK, ref set, null);
        }

        // minitSignals is called when initializing a new m to set the
        // thread's alternate signal stack and signal mask.
        private static void minitSignals()
        {
            minitSignalStack();
            minitSignalMask();
        }

        // minitSignalStack is called when initializing a new m to set the
        // alternate signal stack. If the alternate signal stack is not set
        // for the thread (the normal case) then set the alternate signal
        // stack to the gsignal stack. If the alternate signal stack is set
        // for the thread (the case when a non-Go thread sets the alternate
        // signal stack and then calls a Go function) then set the gsignal
        // stack to the alternate signal stack. Record which choice was made
        // in newSigstack, so that it can be undone in unminit.
        private static void minitSignalStack()
        {
            var _g_ = getg();
            stackt st = default;
            sigaltstack(null, ref st);
            if (st.ss_flags & _SS_DISABLE != 0L)
            {
                signalstack(ref _g_.m.gsignal.stack);
                _g_.m.newSigstack = true;
            }
            else
            {
                setGsignalStack(ref st, ref _g_.m.goSigStack);
                _g_.m.newSigstack = false;
            }
        }

        // minitSignalMask is called when initializing a new m to set the
        // thread's signal mask. When this is called all signals have been
        // blocked for the thread.  This starts with m.sigmask, which was set
        // either from initSigmask for a newly created thread or by calling
        // msigsave if this is a non-Go thread calling a Go function. It
        // removes all essential signals from the mask, thus causing those
        // signals to not be blocked. Then it sets the thread's signal mask.
        // After this is called the thread can receive signals.
        private static void minitSignalMask()
        {
            var nmask = getg().m.sigmask;
            foreach (var (i) in sigtable)
            {
                if (!blockableSig(uint32(i)))
                {
                    sigdelset(ref nmask, i);
                }
            }
            sigprocmask(_SIG_SETMASK, ref nmask, null);
        }

        // unminitSignals is called from dropm, via unminit, to undo the
        // effect of calling minit on a non-Go thread.
        //go:nosplit
        private static void unminitSignals()
        {
            if (getg().m.newSigstack)
            {
                stackt st = new stackt(ss_flags:_SS_DISABLE);
                sigaltstack(ref st, null);
            }
            else
            { 
                // We got the signal stack from someone else. Restore
                // the Go-allocated stack in case this M gets reused
                // for another thread (e.g., it's an extram). Also, on
                // Android, libc allocates a signal stack for all
                // threads, so it's important to restore the Go stack
                // even on Go-created threads so we can free it.
                restoreGsignalStack(ref getg().m.goSigStack);
            }
        }

        // blockableSig returns whether sig may be blocked by the signal mask.
        // We never want to block the signals marked _SigUnblock;
        // these are the synchronous signals that turn into a Go panic.
        // In a Go program--not a c-archive/c-shared--we never want to block
        // the signals marked _SigKill or _SigThrow, as otherwise it's possible
        // for all running threads to block them and delay their delivery until
        // we start a new thread. When linked into a C program we let the C code
        // decide on the disposition of those signals.
        private static bool blockableSig(uint sig)
        {
            var flags = sigtable[sig].flags;
            if (flags & _SigUnblock != 0L)
            {
                return false;
            }
            if (isarchive || islibrary)
            {
                return true;
            }
            return flags & (_SigKill | _SigThrow) == 0L;
        }

        // gsignalStack saves the fields of the gsignal stack changed by
        // setGsignalStack.
        private partial struct gsignalStack
        {
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
        private static void setGsignalStack(ref stackt st, ref gsignalStack old)
        {
            var g = getg();
            if (old != null)
            {
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
        private static void restoreGsignalStack(ref gsignalStack st)
        {
            var gp = getg().m.gsignal;
            gp.stack = st.stack;
            gp.stackguard0 = st.stackguard0;
            gp.stackguard1 = st.stackguard1;
            gp.stktopsp = st.stktopsp;
        }

        // signalstack sets the current thread's alternate signal stack to s.
        //go:nosplit
        private static void signalstack(ref stack s)
        {
            stackt st = new stackt(ss_size:s.hi-s.lo);
            setSignalstackSP(ref st, s.lo);
            sigaltstack(ref st, null);
        }

        // setsigsegv is used on darwin/arm{,64} to fake a segmentation fault.
        //go:nosplit
        private static void setsigsegv(System.UIntPtr pc)
        {
            var g = getg();
            g.sig = _SIGSEGV;
            g.sigpc = pc;
            g.sigcode0 = _SEGV_MAPERR;
            g.sigcode1 = 0L; // TODO: emulate si_addr
        }
    }
}
