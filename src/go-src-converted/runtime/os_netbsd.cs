// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:59 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_netbsd.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _SS_DISABLE = 4L;
        private static readonly long _SIG_BLOCK = 1L;
        private static readonly long _SIG_UNBLOCK = 2L;
        private static readonly long _SIG_SETMASK = 3L;
        private static readonly long _NSIG = 33L;
        private static readonly long _SI_USER = 0L; 

        // From NetBSD's <sys/ucontext.h>
        private static readonly ulong _UC_SIGMASK = 0x01UL;
        private static readonly ulong _UC_CPU = 0x04UL; 

        // From <sys/lwp.h>
        private static readonly ulong _LWP_DETACHED = 0x00000040UL;

        private static readonly long _EAGAIN = 35L;

        private partial struct mOS
        {
            public uint waitsemacount;
        }

        //go:noescape
        private static void setitimer(int mode, ref itimerval @new, ref itimerval old)
;

        //go:noescape
        private static void sigaction(uint sig, ref sigactiont @new, ref sigactiont old)
;

        //go:noescape
        private static void sigaltstack(ref stackt @new, ref stackt old)
;

        //go:noescape
        private static void sigprocmask(int how, ref sigset @new, ref sigset old)
;

        //go:noescape
        private static int sysctl(ref uint mib, uint miblen, ref byte @out, ref System.UIntPtr size, ref byte dst, System.UIntPtr ndst)
;

        private static void lwp_tramp()
;

        private static void raise(uint sig)
;
        private static void raiseproc(uint sig)
;

        //go:noescape
        private static void getcontext(unsafe.Pointer ctxt)
;

        //go:noescape
        private static int lwp_create(unsafe.Pointer ctxt, System.UIntPtr flags, unsafe.Pointer lwpid)
;

        //go:noescape
        private static int lwp_park(int clockid, int flags, ref timespec ts, int unpark, unsafe.Pointer hint, unsafe.Pointer unparkhint)
;

        //go:noescape
        private static int lwp_unpark(int lwp, unsafe.Pointer hint)
;

        private static int lwp_self()
;

        private static void osyield()
;

        private static readonly long _ESRCH = 3L;
        private static readonly long _ETIMEDOUT = 60L; 

        // From NetBSD's <sys/time.h>
        private static readonly long _CLOCK_REALTIME = 0L;
        private static readonly long _CLOCK_VIRTUAL = 1L;
        private static readonly long _CLOCK_PROF = 2L;
        private static readonly long _CLOCK_MONOTONIC = 3L;

        private static readonly long _TIMER_RELTIME = 0L;
        private static readonly long _TIMER_ABSTIME = 1L;

        private static sigset sigset_all = new sigset([4]uint32{^uint32(0),^uint32(0),^uint32(0),^uint32(0)});

        // From NetBSD's <sys/sysctl.h>
        private static readonly long _CTL_HW = 6L;
        private static readonly long _HW_NCPU = 3L;
        private static readonly long _HW_PAGESIZE = 7L;

        private static int getncpu()
        {
            array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_NCPU });
            var @out = uint32(0L);
            var nout = @unsafe.Sizeof(out);
            var ret = sysctl(ref mib[0L], 2L, (byte.Value)(@unsafe.Pointer(ref out)), ref nout, null, 0L);
            if (ret >= 0L)
            {>>MARKER:FUNCTION_osyield_BLOCK_PREFIX<<
                return int32(out);
            }
            return 1L;
        }

        private static System.UIntPtr getPageSize()
        {
            array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_PAGESIZE });
            var @out = uint32(0L);
            var nout = @unsafe.Sizeof(out);
            var ret = sysctl(ref mib[0L], 2L, (byte.Value)(@unsafe.Pointer(ref out)), ref nout, null, 0L);
            if (ret >= 0L)
            {>>MARKER:FUNCTION_lwp_self_BLOCK_PREFIX<<
                return uintptr(out);
            }
            return 0L;
        }

        //go:nosplit
        private static void semacreate(ref m mp)
        {
        }

        //go:nosplit
        private static int semasleep(long ns)
        {
            var _g_ = getg(); 

            // Compute sleep deadline.
            ref timespec tsp = default;
            timespec ts = default;
            if (ns >= 0L)
            {>>MARKER:FUNCTION_lwp_unpark_BLOCK_PREFIX<<
                int nsec = default;
                ts.set_sec(timediv(ns, 1000000000L, ref nsec));
                ts.set_nsec(nsec);
                tsp = ref ts;
            }
            while (true)
            {>>MARKER:FUNCTION_lwp_park_BLOCK_PREFIX<<
                var v = atomic.Load(ref _g_.m.waitsemacount);
                if (v > 0L)
                {>>MARKER:FUNCTION_lwp_create_BLOCK_PREFIX<<
                    if (atomic.Cas(ref _g_.m.waitsemacount, v, v - 1L))
                    {>>MARKER:FUNCTION_getcontext_BLOCK_PREFIX<<
                        return 0L; // semaphore acquired
                    }
                    continue;
                } 

                // Sleep until unparked by semawakeup or timeout.
                var ret = lwp_park(_CLOCK_MONOTONIC, _TIMER_RELTIME, tsp, 0L, @unsafe.Pointer(ref _g_.m.waitsemacount), null);
                if (ret == _ETIMEDOUT)
                {>>MARKER:FUNCTION_raiseproc_BLOCK_PREFIX<<
                    return -1L;
                }
                else if (ret == _EINTR && ns >= 0L)
                {>>MARKER:FUNCTION_raise_BLOCK_PREFIX<< 
                    // Avoid sleeping forever if we keep getting
                    // interrupted (for example by the profiling
                    // timer). It would be if tsp upon return had the
                    // remaining time to sleep, but this is good enough.
                    nsec = default;
                    ns /= 2L;
                    ts.set_sec(timediv(ns, 1000000000L, ref nsec));
                    ts.set_nsec(nsec);
                }
            }

        }

        //go:nosplit
        private static void semawakeup(ref m mp)
        {
            atomic.Xadd(ref mp.waitsemacount, 1L); 
            // From NetBSD's _lwp_unpark(2) manual:
            // "If the target LWP is not currently waiting, it will return
            // immediately upon the next call to _lwp_park()."
            var ret = lwp_unpark(int32(mp.procid), @unsafe.Pointer(ref mp.waitsemacount));
            if (ret != 0L && ret != _ESRCH)
            {>>MARKER:FUNCTION_lwp_tramp_BLOCK_PREFIX<< 
                // semawakeup can be called on signal stack.
                systemstack(() =>
                {>>MARKER:FUNCTION_sysctl_BLOCK_PREFIX<<
                    print("thrwakeup addr=", ref mp.waitsemacount, " sem=", mp.waitsemacount, " ret=", ret, "\n");
                });
            }
        }

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrier
        private static void newosproc(ref m mp, unsafe.Pointer stk)
        {
            if (false)
            {>>MARKER:FUNCTION_sigprocmask_BLOCK_PREFIX<<
                print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " id=", mp.id, " ostk=", ref mp, "\n");
            }
            ucontextt uc = default;
            getcontext(@unsafe.Pointer(ref uc)); 

            // _UC_SIGMASK does not seem to work here.
            // It would be nice if _UC_SIGMASK and _UC_STACK
            // worked so that we could do all the work setting
            // the sigmask and the stack here, instead of setting
            // the mask here and the stack in netbsdMstart.
            // For now do the blocking manually.
            uc.uc_flags = _UC_SIGMASK | _UC_CPU;
            uc.uc_link = null;
            uc.uc_sigmask = sigset_all;

            sigset oset = default;
            sigprocmask(_SIG_SETMASK, ref sigset_all, ref oset);

            lwp_mcontext_init(ref uc.uc_mcontext, stk, mp, mp.g0, funcPC(netbsdMstart));

            var ret = lwp_create(@unsafe.Pointer(ref uc), _LWP_DETACHED, @unsafe.Pointer(ref mp.procid));
            sigprocmask(_SIG_SETMASK, ref oset, null);
            if (ret < 0L)
            {>>MARKER:FUNCTION_sigaltstack_BLOCK_PREFIX<<
                print("runtime: failed to create new OS thread (have ", mcount() - 1L, " already; errno=", -ret, ")\n");
                if (ret == -_EAGAIN)
                {>>MARKER:FUNCTION_sigaction_BLOCK_PREFIX<<
                    println("runtime: may need to increase max user processes (ulimit -p)");
                }
                throw("runtime.newosproc");
            }
        }

        // netbsdMStart is the function call that starts executing a newly
        // created thread. On NetBSD, a new thread inherits the signal stack
        // of the creating thread. That confuses minit, so we remove that
        // signal stack here before calling the regular mstart. It's a bit
        // baroque to remove a signal stack here only to add one in minit, but
        // it's a simple change that keeps NetBSD working like other OS's.
        // At this point all signals are blocked, so there is no race.
        //go:nosplit
        private static void netbsdMstart()
        {
            stackt st = new stackt(ss_flags:_SS_DISABLE);
            sigaltstack(ref st, null);
            mstart();
        }

        private static void osinit()
        {
            ncpu = getncpu();
            physPageSize = getPageSize();
        }

        private static slice<byte> urandom_dev = (slice<byte>)"/dev/urandom\x00";

        //go:nosplit
        private static void getRandomData(slice<byte> r)
        {
            var fd = open(ref urandom_dev[0L], 0L, 0L);
            var n = read(fd, @unsafe.Pointer(ref r[0L]), int32(len(r)));
            closefd(fd);
            extendRandom(r, int(n));
        }

        private static void goenvs()
        {
            goenvs_unix();
        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the parent thread (main thread in case of bootstrap), can allocate memory.
        private static void mpreinit(ref m mp)
        {
            mp.gsignal = malg(32L * 1024L);
            mp.gsignal.m = mp;
        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the new thread, cannot allocate memory.
        private static void minit()
        {
            var _g_ = getg();
            _g_.m.procid = uint64(lwp_self()); 

            // On NetBSD a thread created by pthread_create inherits the
            // signal stack of the creating thread. We always create a
            // new signal stack here, to avoid having two Go threads using
            // the same signal stack. This breaks the case of a thread
            // created in C that calls sigaltstack and then calls a Go
            // function, because we will lose track of the C code's
            // sigaltstack, but it's the best we can do.
            signalstack(ref _g_.m.gsignal.stack);
            _g_.m.newSigstack = true;

            minitSignalMask();
        }

        // Called from dropm to undo the effect of an minit.
        //go:nosplit
        private static void unminit()
        {
            unminitSignals();
        }

        private static System.UIntPtr memlimit()
        {
            return 0L;
        }

        private static void sigtramp()
;

        private partial struct sigactiont
        {
            public System.UIntPtr sa_sigaction;
            public sigset sa_mask;
            public int sa_flags;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsig(uint i, System.UIntPtr fn)
        {
            sigactiont sa = default;
            sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
            sa.sa_mask = sigset_all;
            if (fn == funcPC(sighandler))
            {>>MARKER:FUNCTION_sigtramp_BLOCK_PREFIX<<
                fn = funcPC(sigtramp);
            }
            sa.sa_sigaction = fn;
            sigaction(i, ref sa, null);
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsigstack(uint i)
        {
            throw("setsigstack");
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr getsig(uint i)
        {
            sigactiont sa = default;
            sigaction(i, null, ref sa);
            return sa.sa_sigaction;
        }

        // setSignaltstackSP sets the ss_sp field of a stackt.
        //go:nosplit
        private static void setSignalstackSP(ref stackt s, System.UIntPtr sp)
        {
            s.ss_sp = sp;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaddset(ref sigset mask, long i)
        {
            mask.__bits[(i - 1L) / 32L] |= 1L << (int)(((uint32(i) - 1L) & 31L));
        }

        private static void sigdelset(ref sigset mask, long i)
        {
            mask.__bits[(i - 1L) / 32L] &= 1L << (int)(((uint32(i) - 1L) & 31L));
        }

        private static void fixsigcode(this ref sigctxt c, uint sig)
        {>>MARKER:FUNCTION_setitimer_BLOCK_PREFIX<<
        }
    }
}
