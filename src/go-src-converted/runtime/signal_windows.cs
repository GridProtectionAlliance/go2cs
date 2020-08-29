// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:20:41 UTC
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
            const ulong SEM_FAILCRITICALERRORS = 0x0001UL;
            const ulong SEM_NOGPFAULTERRORBOX = 0x0002UL;
            const ulong SEM_NOALIGNMENTFAULTEXCEPT = 0x0004UL;
            const ulong SEM_NOOPENFILEERRORBOX = 0x8000UL;
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
            if (_AddVectoredContinueHandler == null || @unsafe.Sizeof(ref _AddVectoredContinueHandler) == 4L)
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

        private static bool isgoexception(ref exceptionrecord info, ref context r)
        { 
            // Only handle exception if executing instructions in Go binary
            // (not Windows library code).
            // TODO(mwhudson): needs to loop to support shared libs
            if (r.ip() < firstmoduledata.text || firstmoduledata.etext < r.ip())
            {>>MARKER:FUNCTION_exceptiontramp_BLOCK_PREFIX<<
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
        private static int exceptionhandler(ref exceptionrecord info, ref context r, ref g gp)
        {
            if (!isgoexception(info, r))
            {
                return _EXCEPTION_CONTINUE_SEARCH;
            }
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
            if (r.ip() != 0L)
            {
                var sp = @unsafe.Pointer(r.sp());
                sp = add(sp, ~(@unsafe.Sizeof(uintptr(0L)) - 1L)); // sp--
                ((uintptr.Value)(sp)).Value = r.ip();
                r.setsp(uintptr(sp));
            }
            r.setip(funcPC(sigpanic));
            return _EXCEPTION_CONTINUE_EXECUTION;
        }

        // It seems Windows searches ContinueHandler's list even
        // if ExceptionHandler returns EXCEPTION_CONTINUE_EXECUTION.
        // firstcontinuehandler will stop that search,
        // if exceptionhandler did the same earlier.
        private static int firstcontinuehandler(ref exceptionrecord info, ref context r, ref g gp)
        {
            if (!isgoexception(info, r))
            {
                return _EXCEPTION_CONTINUE_SEARCH;
            }
            return _EXCEPTION_CONTINUE_EXECUTION;
        }

        private static bool testingWER = default;

        // lastcontinuehandler is reached, because runtime cannot handle
        // current exception. lastcontinuehandler will print crash info and exit.
        private static int lastcontinuehandler(ref exceptionrecord info, ref context r, ref g gp)
        {
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

            var (level, _, docrash) = gotraceback();
            if (level > 0L)
            {
                tracebacktrap(r.ip(), r.sp(), 0L, gp);
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
            const @string msg = "runtime: signal received on thread not created by Go.\n";

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
