// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build darwin dragonfly freebsd linux nacl netbsd openbsd solaris

// package runtime -- go2cs converted at 2020 August 29 08:20:35 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_sighandler.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        // crashing is the number of m's we have waited for when implementing
        // GOTRACEBACK=crash when a signal is received.
        private static int crashing = default;

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
        private static void sighandler(uint sig, ref siginfo info, unsafe.Pointer ctxt, ref g gp)
        {
            var _g_ = getg();
            sigctxt c = ref new sigctxt(info,ctxt);

            if (sig == _SIGPROF)
            {
                sigprof(c.sigpc(), c.sigsp(), c.siglr(), gp, _g_.m);
                return;
            }
            var flags = int32(_SigThrow);
            if (sig < uint32(len(sigtable)))
            {
                flags = sigtable[sig].flags;
            }
            if (flags & _SigPanic != 0L && gp.throwsplit)
            { 
                // We can't safely sigpanic because it may grow the
                // stack. Abort in the signal handler instead.
                flags = (flags & ~_SigPanic) | _SigThrow;
            }
            if (c.sigcode() != _SI_USER && flags & _SigPanic != 0L)
            { 
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
                return;
            }
            if (c.sigcode() == _SI_USER || flags & _SigNotify != 0L)
            {
                if (sigsend(sig))
                {
                    return;
                }
            }
            if (c.sigcode() == _SI_USER && signal_ignored(sig))
            {
                return;
            }
            if (flags & _SigKill != 0L)
            {
                dieFromSignal(sig);
            }
            if (flags & _SigThrow == 0L)
            {
                return;
            }
            _g_.m.throwing = 1L;
            _g_.m.caughtsig.set(gp);

            if (crashing == 0L)
            {
                startpanic();
            }
            if (sig < uint32(len(sigtable)))
            {
                print(sigtable[sig].name, "\n");
            }
            else
            {
                print("Signal ", sig, "\n");
            }
            print("PC=", hex(c.sigpc()), " m=", _g_.m.id, " sigcode=", c.sigcode(), "\n");
            if (_g_.m.lockedg != 0L && _g_.m.ncgo > 0L && gp == _g_.m.g0)
            {
                print("signal arrived during cgo execution\n");
                gp = _g_.m.lockedg.ptr();
            }
            print("\n");

            var (level, _, docrash) = gotraceback();
            if (level > 0L)
            {
                goroutineheader(gp);
                tracebacktrap(c.sigpc(), c.sigsp(), c.siglr(), gp);
                if (crashing > 0L && gp != _g_.m.curg && _g_.m.curg != null && readgstatus(_g_.m.curg) & ~_Gscan == _Grunning)
                { 
                    // tracebackothers on original m skipped this one; trace it now.
                    goroutineheader(_g_.m.curg);
                    traceback(~uintptr(0L), ~uintptr(0L), 0L, _g_.m.curg);
                }
                else if (crashing == 0L)
                {
                    tracebackothers(gp);
                    print("\n");
                }
                dumpregs(c);
            }
            if (docrash)
            {
                crashing++;
                if (crashing < mcount() - int32(extraMCount))
                { 
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
                    usleep(5L * 1000L * 1000L);
                }
                crash();
            }
            exit(2L);
        }
    }
}
