// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:23:32 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\signal_windows.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static void disableWER()
        { 
            // do not display Windows Error Reporting dialogue
            const ulong SEM_FAILCRITICALERRORS = (ulong)0x0001UL;
            const ulong SEM_NOGPFAULTERRORBOX = (ulong)0x0002UL;
            const ulong SEM_NOALIGNMENTFAULTEXCEPT = (ulong)0x0004UL;
            const ulong SEM_NOOPENFILEERRORBOX = (ulong)0x8000UL;

            var errormode = uint32(stdcall1(_SetErrorMode, SEM_NOGPFAULTERRORBOX));
            stdcall1(_SetErrorMode, uintptr(errormode) | SEM_FAILCRITICALERRORS | SEM_NOGPFAULTERRORBOX | SEM_NOOPENFILEERRORBOX);

        }

        // in sys_windows_386.s and sys_windows_amd64.s
        private static void exceptiontramp()
;
        private static void firstcontinuetramp()
;
        private static void lastcontinuetramp()
;

        private static void initExceptionHandler()
        {
            stdcall2(_AddVectoredExceptionHandler, 1L, funcPC(exceptiontramp));
            if (_AddVectoredContinueHandler == null || GOARCH == "386")
            {>>MARKER:FUNCTION_lastcontinuetramp_BLOCK_PREFIX<< 
                // use SetUnhandledExceptionFilter for windows-386 or
                // if VectoredContinueHandler is unavailable.
                // note: SetUnhandledExceptionFilter handler won't be called, if debugging.
                stdcall1(_SetUnhandledExceptionFilter, funcPC(lastcontinuetramp));

            }
            else
            {>>MARKER:FUNCTION_firstcontinuetramp_BLOCK_PREFIX<<
                stdcall2(_AddVectoredContinueHandler, 1L, funcPC(firstcontinuetramp));
                stdcall2(_AddVectoredContinueHandler, 0L, funcPC(lastcontinuetramp));
            }

        }

        // isAbort returns true, if context r describes exception raised
        // by calling runtime.abort function.
        //
        //go:nosplit
        private static bool isAbort(ptr<context> _addr_r)
        {
            ref context r = ref _addr_r.val;

            switch (GOARCH)
            {
                case "386": 
                    // In the case of an abort, the exception IP is one byte after
                    // the INT3 (this differs from UNIX OSes).

                case "amd64": 
                    // In the case of an abort, the exception IP is one byte after
                    // the INT3 (this differs from UNIX OSes).
                    return isAbortPC(r.ip() - 1L);
                    break;
                case "arm": 
                    return isAbortPC(r.ip());
                    break;
                default: 
                    return false;
                    break;
            }

        }

        // isgoexception reports whether this exception should be translated
        // into a Go panic.
        //
        // It is nosplit to avoid growing the stack in case we're aborting
        // because of a stack overflow.
        //
        //go:nosplit
        private static bool isgoexception(ptr<exceptionrecord> _addr_info, ptr<context> _addr_r)
        {
            ref exceptionrecord info = ref _addr_info.val;
            ref context r = ref _addr_r.val;
 
            // Only handle exception if executing instructions in Go binary
            // (not Windows library code).
            // TODO(mwhudson): needs to loop to support shared libs
            if (r.ip() < firstmoduledata.text || firstmoduledata.etext < r.ip())
            {>>MARKER:FUNCTION_exceptiontramp_BLOCK_PREFIX<<
                return false;
            }

            if (isAbort(_addr_r))
            { 
                // Never turn abort into a panic.
                return false;

            } 

            // Go will only handle some exceptions.

            if (info.exceptioncode == _EXCEPTION_ACCESS_VIOLATION)             else if (info.exceptioncode == _EXCEPTION_INT_DIVIDE_BY_ZERO)             else if (info.exceptioncode == _EXCEPTION_INT_OVERFLOW)             else if (info.exceptioncode == _EXCEPTION_FLT_DENORMAL_OPERAND)             else if (info.exceptioncode == _EXCEPTION_FLT_DIVIDE_BY_ZERO)             else if (info.exceptioncode == _EXCEPTION_FLT_INEXACT_RESULT)             else if (info.exceptioncode == _EXCEPTION_FLT_OVERFLOW)             else if (info.exceptioncode == _EXCEPTION_FLT_UNDERFLOW)             else if (info.exceptioncode == _EXCEPTION_BREAKPOINT)             else 
                return false;
                        return true;

        }

        // Called by sigtramp from Windows VEH handler.
        // Return value signals whether the exception has been handled (EXCEPTION_CONTINUE_EXECUTION)
        // or should be made available to other handlers in the chain (EXCEPTION_CONTINUE_SEARCH).
        //
        // This is the first entry into Go code for exception handling. This
        // is nosplit to avoid growing the stack until we've checked for
        // _EXCEPTION_BREAKPOINT, which is raised if we overflow the g0 stack,
        //
        //go:nosplit
        private static int exceptionhandler(ptr<exceptionrecord> _addr_info, ptr<context> _addr_r, ptr<g> _addr_gp) => func((_, panic, __) =>
        {
            ref exceptionrecord info = ref _addr_info.val;
            ref context r = ref _addr_r.val;
            ref g gp = ref _addr_gp.val;

            if (!isgoexception(_addr_info, _addr_r))
            {
                return _EXCEPTION_CONTINUE_SEARCH;
            } 

            // After this point, it is safe to grow the stack.
            if (gp.throwsplit)
            { 
                // We can't safely sigpanic because it may grow the
                // stack. Let it fall through.
                return _EXCEPTION_CONTINUE_SEARCH;

            } 

            // Make it look like a call to the signal func.
            // Have to pass arguments out of band since
            // augmenting the stack frame would break
            // the unwinding code.
            gp.sig = info.exceptioncode;
            gp.sigcode0 = uintptr(info.exceptioninformation[0L]);
            gp.sigcode1 = uintptr(info.exceptioninformation[1L]);
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
            if (r.ip() != 0L && r.ip() != funcPC(asyncPreempt))
            {
                var sp = @unsafe.Pointer(r.sp());
                sp = add(sp, ~(@unsafe.Sizeof(uintptr(0L)) - 1L)); // sp--
                r.set_sp(uintptr(sp));
                switch (GOARCH)
                {
                    case "386": 

                    case "amd64": 
                        ((uintptr.val)(sp)).val = r.ip();
                        break;
                    case "arm": 
                        ((uintptr.val)(sp)).val = r.lr();
                        r.set_lr(r.ip());
                        break;
                    default: 
                        panic("unsupported architecture");
                        break;
                }

            }

            r.set_ip(funcPC(sigpanic));
            return _EXCEPTION_CONTINUE_EXECUTION;

        });

        // It seems Windows searches ContinueHandler's list even
        // if ExceptionHandler returns EXCEPTION_CONTINUE_EXECUTION.
        // firstcontinuehandler will stop that search,
        // if exceptionhandler did the same earlier.
        //
        // It is nosplit for the same reason as exceptionhandler.
        //
        //go:nosplit
        private static int firstcontinuehandler(ptr<exceptionrecord> _addr_info, ptr<context> _addr_r, ptr<g> _addr_gp)
        {
            ref exceptionrecord info = ref _addr_info.val;
            ref context r = ref _addr_r.val;
            ref g gp = ref _addr_gp.val;

            if (!isgoexception(_addr_info, _addr_r))
            {
                return _EXCEPTION_CONTINUE_SEARCH;
            }

            return _EXCEPTION_CONTINUE_EXECUTION;

        }

        private static bool testingWER = default;

        // lastcontinuehandler is reached, because runtime cannot handle
        // current exception. lastcontinuehandler will print crash info and exit.
        //
        // It is nosplit for the same reason as exceptionhandler.
        //
        //go:nosplit
        private static int lastcontinuehandler(ptr<exceptionrecord> _addr_info, ptr<context> _addr_r, ptr<g> _addr_gp)
        {
            ref exceptionrecord info = ref _addr_info.val;
            ref context r = ref _addr_r.val;
            ref g gp = ref _addr_gp.val;

            if (islibrary || isarchive)
            { 
                // Go DLL/archive has been loaded in a non-go program.
                // If the exception does not originate from go, the go runtime
                // should not take responsibility of crashing the process.
                return _EXCEPTION_CONTINUE_SEARCH;

            }

            if (testingWER)
            {
                return _EXCEPTION_CONTINUE_SEARCH;
            }

            var _g_ = getg();

            if (panicking != 0L)
            { // traceback already printed
                exit(2L);

            }

            panicking = 1L; 

            // In case we're handling a g0 stack overflow, blow away the
            // g0 stack bounds so we have room to print the traceback. If
            // this somehow overflows the stack, the OS will trap it.
            _g_.stack.lo = 0L;
            _g_.stackguard0 = _g_.stack.lo + _StackGuard;
            _g_.stackguard1 = _g_.stackguard0;

            print("Exception ", hex(info.exceptioncode), " ", hex(info.exceptioninformation[0L]), " ", hex(info.exceptioninformation[1L]), " ", hex(r.ip()), "\n");

            print("PC=", hex(r.ip()), "\n");
            if (_g_.m.lockedg != 0L && _g_.m.ncgo > 0L && gp == _g_.m.g0)
            {
                if (iscgo)
                {
                    print("signal arrived during external code execution\n");
                }

                gp = _g_.m.lockedg.ptr();

            }

            print("\n"); 

            // TODO(jordanrh1): This may be needed for 386/AMD64 as well.
            if (GOARCH == "arm")
            {
                _g_.m.throwing = 1L;
                _g_.m.caughtsig.set(gp);
            }

            var (level, _, docrash) = gotraceback();
            if (level > 0L)
            {
                tracebacktrap(r.ip(), r.sp(), r.lr(), gp);
                tracebackothers(gp);
                dumpregs(r);
            }

            if (docrash)
            {
                crash();
            }

            exit(2L);
            return 0L; // not reached
        }

        private static void sigpanic()
        {
            var g = getg();
            if (!canpanic(g))
            {
                throw("unexpected signal during runtime execution");
            }


            if (g.sig == _EXCEPTION_ACCESS_VIOLATION) 
                if (g.sigcode1 < 0x1000UL || g.paniconfault)
                {
                    panicmem();
                }

                print("unexpected fault address ", hex(g.sigcode1), "\n");
                throw("fault");
            else if (g.sig == _EXCEPTION_INT_DIVIDE_BY_ZERO) 
                panicdivide();
            else if (g.sig == _EXCEPTION_INT_OVERFLOW) 
                panicoverflow();
            else if (g.sig == _EXCEPTION_FLT_DENORMAL_OPERAND || g.sig == _EXCEPTION_FLT_DIVIDE_BY_ZERO || g.sig == _EXCEPTION_FLT_INEXACT_RESULT || g.sig == _EXCEPTION_FLT_OVERFLOW || g.sig == _EXCEPTION_FLT_UNDERFLOW) 
                panicfloat();
                        throw("fault");

        }

        private static array<byte> badsignalmsg = new array<byte>(100L);        private static int badsignallen = default;

        private static void setBadSignalMsg()
        {
            const @string msg = (@string)"runtime: signal received on thread not created by Go.\n";

            foreach (var (i, c) in msg)
            {
                badsignalmsg[i] = byte(c);
                badsignallen++;
            }

        }

        // Following are not implemented.

        private static void initsig(bool preinit)
        {
        }

        private static void sigenable(uint sig)
        {
        }

        private static void sigdisable(uint sig)
        {
        }

        private static void sigignore(uint sig)
        {
        }

        private static void badsignal2()
;

        private static void raisebadsignal(uint sig)
        {
            badsignal2();
        }

        private static @string signame(uint sig)
        {
            return "";
        }

        //go:nosplit
        private static void crash()
        { 
            // TODO: This routine should do whatever is needed
            // to make the Windows program abort/crash as it
            // would if Go was not intercepting signals.
            // On Unix the routine would remove the custom signal
            // handler and then raise a signal (like SIGABRT).
            // Something like that should happen here.
            // It's okay to leave this empty for now: if crash returns
            // the ordinary exit-after-panic happens.
        }

        // gsignalStack is unused on Windows.
        private partial struct gsignalStack
        {
        }
    }
}
