// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:19:02 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_openbsd.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
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
        private static sigset obsdsigprocmask(int how, sigset @new)
;

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigprocmask(int how, ref sigset @new, ref sigset old)
        {
            var n = sigset(0L);
            if (new != null)
            {>>MARKER:FUNCTION_obsdsigprocmask_BLOCK_PREFIX<<
                n = new.Value;
            }
            var r = obsdsigprocmask(how, n);
            if (old != null)
            {>>MARKER:FUNCTION_sigaltstack_BLOCK_PREFIX<<
                old.Value = r;
            }
        }

        //go:noescape
        private static int sysctl(ref uint mib, uint miblen, ref byte @out, ref System.UIntPtr size, ref byte dst, System.UIntPtr ndst)
;

        private static void raise(uint sig)
;
        private static void raiseproc(uint sig)
;

        //go:noescape
        private static int tfork(ref tforkt param, System.UIntPtr psize, ref m mm, ref g gg, System.UIntPtr fn)
;

        //go:noescape
        private static int thrsleep(System.UIntPtr ident, int clock_id, ref timespec tsp, System.UIntPtr @lock, ref uint abort)
;

        //go:noescape
        private static int thrwakeup(System.UIntPtr ident, int n)
;

        private static void osyield()
;

        private static readonly long _ESRCH = 3L;
        private static readonly long _EAGAIN = 35L;
        private static readonly var _EWOULDBLOCK = _EAGAIN;
        private static readonly long _ENOTSUP = 91L; 

        // From OpenBSD's sys/time.h
        private static readonly long _CLOCK_REALTIME = 0L;
        private static readonly long _CLOCK_VIRTUAL = 1L;
        private static readonly long _CLOCK_PROF = 2L;
        private static readonly long _CLOCK_MONOTONIC = 3L;

        private partial struct sigset // : uint
        {
        }

        private static var sigset_all = ~sigset(0L);

        // From OpenBSD's <sys/sysctl.h>
        private static readonly long _CTL_HW = 6L;
        private static readonly long _HW_NCPU = 3L;
        private static readonly long _HW_PAGESIZE = 7L;

        private static int getncpu()
        {
            array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_NCPU });
            var @out = uint32(0L);
            var nout = @unsafe.Sizeof(out); 

            // Fetch hw.ncpu via sysctl.
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
            {>>MARKER:FUNCTION_thrwakeup_BLOCK_PREFIX<<
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
            if (ns >= 0L)
            {>>MARKER:FUNCTION_thrsleep_BLOCK_PREFIX<<
                timespec ts = default;
                int nsec = default;
                ns += nanotime();
                ts.set_sec(int64(timediv(ns, 1000000000L, ref nsec)));
                ts.set_nsec(nsec);
                tsp = ref ts;
            }
            while (true)
            {>>MARKER:FUNCTION_tfork_BLOCK_PREFIX<<
                var v = atomic.Load(ref _g_.m.waitsemacount);
                if (v > 0L)
                {>>MARKER:FUNCTION_raiseproc_BLOCK_PREFIX<<
                    if (atomic.Cas(ref _g_.m.waitsemacount, v, v - 1L))
                    {>>MARKER:FUNCTION_raise_BLOCK_PREFIX<<
                        return 0L; // semaphore acquired
                    }
                    continue;
                } 

                // Sleep until woken by semawakeup or timeout; or abort if waitsemacount != 0.
                //
                // From OpenBSD's __thrsleep(2) manual:
                // "The abort argument, if not NULL, points to an int that will
                // be examined [...] immediately before blocking. If that int
                // is non-zero then __thrsleep() will immediately return EINTR
                // without blocking."
                var ret = thrsleep(uintptr(@unsafe.Pointer(ref _g_.m.waitsemacount)), _CLOCK_MONOTONIC, tsp, 0L, ref _g_.m.waitsemacount);
                if (ret == _EWOULDBLOCK)
                {>>MARKER:FUNCTION_sysctl_BLOCK_PREFIX<<
                    return -1L;
                }
            }

        }

        //go:nosplit
        private static void semawakeup(ref m mp)
        {
            atomic.Xadd(ref mp.waitsemacount, 1L);
            var ret = thrwakeup(uintptr(@unsafe.Pointer(ref mp.waitsemacount)), 1L);
            if (ret != 0L && ret != _ESRCH)
            {>>MARKER:FUNCTION_sigaction_BLOCK_PREFIX<< 
                // semawakeup can be called on signal stack.
                systemstack(() =>
                {>>MARKER:FUNCTION_setitimer_BLOCK_PREFIX<<
                    print("thrwakeup addr=", ref mp.waitsemacount, " sem=", mp.waitsemacount, " ret=", ret, "\n");
                });
            }
        }

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrier
        private static void newosproc(ref m mp, unsafe.Pointer stk)
        {
            if (false)
            {
                print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " id=", mp.id, " ostk=", ref mp, "\n");
            }
            tforkt param = new tforkt(tf_tcb:unsafe.Pointer(&mp.tls[0]),tf_tid:(*int32)(unsafe.Pointer(&mp.procid)),tf_stack:uintptr(stk),);

            sigset oset = default;
            sigprocmask(_SIG_SETMASK, ref sigset_all, ref oset);
            var ret = tfork(ref param, @unsafe.Sizeof(param), mp, mp.g0, funcPC(mstart));
            sigprocmask(_SIG_SETMASK, ref oset, null);

            if (ret < 0L)
            {
                print("runtime: failed to create new OS thread (have ", mcount() - 1L, " already; errno=", -ret, ")\n");
                if (ret == -_EAGAIN)
                {
                    println("runtime: may need to increase max user processes (ulimit -p)");
                }
                throw("runtime.newosproc");
            }
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
        // Called on the new thread, can not allocate memory.
        private static void minit()
        { 
            // m.procid is a uint64, but tfork writes an int32. Fix it up.
            var _g_ = getg();
            _g_.m.procid = uint64(@unsafe.Pointer(ref _g_.m.procid).Value);

            minitSignals();
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
            public uint sa_mask;
            public int sa_flags;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsig(uint i, System.UIntPtr fn)
        {
            sigactiont sa = default;
            sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
            sa.sa_mask = uint32(sigset_all);
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
            mask.Value |= 1L << (int)((uint32(i) - 1L));
        }

        private static void sigdelset(ref sigset mask, long i)
        {
            mask.Value &= 1L << (int)((uint32(i) - 1L));
        }

        private static void fixsigcode(this ref sigctxt c, uint sig)
        {
        }
    }
}
