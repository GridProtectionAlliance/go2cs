// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:22:01 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_netbsd.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _SS_DISABLE = (long)4L;
        private static readonly long _SIG_BLOCK = (long)1L;
        private static readonly long _SIG_UNBLOCK = (long)2L;
        private static readonly long _SIG_SETMASK = (long)3L;
        private static readonly long _NSIG = (long)33L;
        private static readonly long _SI_USER = (long)0L; 

        // From NetBSD's <sys/ucontext.h>
        private static readonly ulong _UC_SIGMASK = (ulong)0x01UL;
        private static readonly ulong _UC_CPU = (ulong)0x04UL; 

        // From <sys/lwp.h>
        private static readonly ulong _LWP_DETACHED = (ulong)0x00000040UL;


        private partial struct mOS
        {
            public uint waitsemacount;
        }

        //go:noescape
        private static void setitimer(int mode, ptr<itimerval> @new, ptr<itimerval> old)
;

        //go:noescape
        private static void sigaction(uint sig, ptr<sigactiont> @new, ptr<sigactiont> old)
;

        //go:noescape
        private static void sigaltstack(ptr<stackt> @new, ptr<stackt> old)
;

        //go:noescape
        private static void sigprocmask(int how, ptr<sigset> @new, ptr<sigset> old)
;

        //go:noescape
        private static int sysctl(ptr<uint> mib, uint miblen, ptr<byte> @out, ptr<System.UIntPtr> size, ptr<byte> dst, System.UIntPtr ndst)
;

        private static void lwp_tramp()
;

        private static void raiseproc(uint sig)
;

        private static void lwp_kill(int tid, long sig)
;

        //go:noescape
        private static void getcontext(unsafe.Pointer ctxt)
;

        //go:noescape
        private static int lwp_create(unsafe.Pointer ctxt, System.UIntPtr flags, unsafe.Pointer lwpid)
;

        //go:noescape
        private static int lwp_park(int clockid, int flags, ptr<timespec> ts, int unpark, unsafe.Pointer hint, unsafe.Pointer unparkhint)
;

        //go:noescape
        private static int lwp_unpark(int lwp, unsafe.Pointer hint)
;

        private static int lwp_self()
;

        private static void osyield()
;

        private static int kqueue()
;

        //go:noescape
        private static int kevent(int kq, ptr<keventt> ch, int nch, ptr<keventt> ev, int nev, ptr<timespec> ts)
;

        private static (int, int, int) pipe()
;
        private static (int, int, int) pipe2(int flags)
;
        private static void closeonexec(int fd)
;
        private static void setNonblock(int fd)
;

        private static readonly long _ESRCH = (long)3L;
        private static readonly long _ETIMEDOUT = (long)60L; 

        // From NetBSD's <sys/time.h>
        private static readonly long _CLOCK_REALTIME = (long)0L;
        private static readonly long _CLOCK_VIRTUAL = (long)1L;
        private static readonly long _CLOCK_PROF = (long)2L;
        private static readonly long _CLOCK_MONOTONIC = (long)3L;

        private static readonly long _TIMER_RELTIME = (long)0L;
        private static readonly long _TIMER_ABSTIME = (long)1L;


        private static sigset sigset_all = new sigset([4]uint32{^uint32(0),^uint32(0),^uint32(0),^uint32(0)});

        // From NetBSD's <sys/sysctl.h>
        private static readonly long _CTL_HW = (long)6L;
        private static readonly long _HW_NCPU = (long)3L;
        private static readonly long _HW_PAGESIZE = (long)7L;


        private static int getncpu()
        {
            array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_NCPU });
            ref var @out = ref heap(uint32(0L), out ptr<var> _addr_@out);
            ref var nout = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_nout);
            var ret = sysctl(_addr_mib[0L], 2L, _addr_(byte.val)(@unsafe.Pointer(_addr_out)), _addr_nout, _addr_null, 0L);
            if (ret >= 0L)
            {>>MARKER:FUNCTION_setNonblock_BLOCK_PREFIX<<
                return int32(out);
            }

            return 1L;

        }

        private static System.UIntPtr getPageSize()
        {
            array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_PAGESIZE });
            ref var @out = ref heap(uint32(0L), out ptr<var> _addr_@out);
            ref var nout = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_nout);
            var ret = sysctl(_addr_mib[0L], 2L, _addr_(byte.val)(@unsafe.Pointer(_addr_out)), _addr_nout, _addr_null, 0L);
            if (ret >= 0L)
            {>>MARKER:FUNCTION_closeonexec_BLOCK_PREFIX<<
                return uintptr(out);
            }

            return 0L;

        }

        //go:nosplit
        private static void semacreate(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

        }

        //go:nosplit
        private static int semasleep(long ns)
        {
            var _g_ = getg();
            long deadline = default;
            if (ns >= 0L)
            {>>MARKER:FUNCTION_pipe2_BLOCK_PREFIX<<
                deadline = nanotime() + ns;
            }

            while (true)
            {>>MARKER:FUNCTION_pipe_BLOCK_PREFIX<<
                var v = atomic.Load(_addr__g_.m.waitsemacount);
                if (v > 0L)
                {>>MARKER:FUNCTION_kevent_BLOCK_PREFIX<<
                    if (atomic.Cas(_addr__g_.m.waitsemacount, v, v - 1L))
                    {>>MARKER:FUNCTION_kqueue_BLOCK_PREFIX<<
                        return 0L; // semaphore acquired
                    }

                    continue;

                } 

                // Sleep until unparked by semawakeup or timeout.
                ptr<timespec> tsp;
                ref timespec ts = ref heap(out ptr<timespec> _addr_ts);
                if (ns >= 0L)
                {>>MARKER:FUNCTION_osyield_BLOCK_PREFIX<<
                    var wait = deadline - nanotime();
                    if (wait <= 0L)
                    {>>MARKER:FUNCTION_lwp_self_BLOCK_PREFIX<<
                        return -1L;
                    }

                    ts.setNsec(wait);
                    tsp = _addr_ts;

                }

                var ret = lwp_park(_CLOCK_MONOTONIC, _TIMER_RELTIME, tsp, 0L, @unsafe.Pointer(_addr__g_.m.waitsemacount), null);
                if (ret == _ETIMEDOUT)
                {>>MARKER:FUNCTION_lwp_unpark_BLOCK_PREFIX<<
                    return -1L;
                }

            }


        }

        //go:nosplit
        private static void semawakeup(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            atomic.Xadd(_addr_mp.waitsemacount, 1L); 
            // From NetBSD's _lwp_unpark(2) manual:
            // "If the target LWP is not currently waiting, it will return
            // immediately upon the next call to _lwp_park()."
            var ret = lwp_unpark(int32(mp.procid), @unsafe.Pointer(_addr_mp.waitsemacount));
            if (ret != 0L && ret != _ESRCH)
            {>>MARKER:FUNCTION_lwp_park_BLOCK_PREFIX<< 
                // semawakeup can be called on signal stack.
                systemstack(() =>
                {>>MARKER:FUNCTION_lwp_create_BLOCK_PREFIX<<
                    print("thrwakeup addr=", _addr_mp.waitsemacount, " sem=", mp.waitsemacount, " ret=", ret, "\n");
                });

            }

        }

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrier
        private static void newosproc(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            var stk = @unsafe.Pointer(mp.g0.stack.hi);
            if (false)
            {>>MARKER:FUNCTION_getcontext_BLOCK_PREFIX<<
                print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " id=", mp.id, " ostk=", _addr_mp, "\n");
            }

            ref ucontextt uc = ref heap(out ptr<ucontextt> _addr_uc);
            getcontext(@unsafe.Pointer(_addr_uc)); 

            // _UC_SIGMASK does not seem to work here.
            // It would be nice if _UC_SIGMASK and _UC_STACK
            // worked so that we could do all the work setting
            // the sigmask and the stack here, instead of setting
            // the mask here and the stack in netbsdMstart.
            // For now do the blocking manually.
            uc.uc_flags = _UC_SIGMASK | _UC_CPU;
            uc.uc_link = null;
            uc.uc_sigmask = sigset_all;

            ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
            sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);

            lwp_mcontext_init(_addr_uc.uc_mcontext, stk, mp, mp.g0, funcPC(netbsdMstart));

            var ret = lwp_create(@unsafe.Pointer(_addr_uc), _LWP_DETACHED, @unsafe.Pointer(_addr_mp.procid));
            sigprocmask(_SIG_SETMASK, _addr_oset, _addr_null);
            if (ret < 0L)
            {>>MARKER:FUNCTION_lwp_kill_BLOCK_PREFIX<<
                print("runtime: failed to create new OS thread (have ", mcount() - 1L, " already; errno=", -ret, ")\n");
                if (ret == -_EAGAIN)
                {>>MARKER:FUNCTION_raiseproc_BLOCK_PREFIX<<
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
            ref stackt st = ref heap(new stackt(ss_flags:_SS_DISABLE), out ptr<stackt> _addr_st);
            sigaltstack(_addr_st, _addr_null);
            mstart();
        }

        private static void osinit()
        {
            ncpu = getncpu();
            if (physPageSize == 0L)
            {>>MARKER:FUNCTION_lwp_tramp_BLOCK_PREFIX<<
                physPageSize = getPageSize();
            }

        }

        private static slice<byte> urandom_dev = (slice<byte>)"/dev/urandom\x00";

        //go:nosplit
        private static void getRandomData(slice<byte> r)
        {
            var fd = open(_addr_urandom_dev[0L], 0L, 0L);
            var n = read(fd, @unsafe.Pointer(_addr_r[0L]), int32(len(r)));
            closefd(fd);
            extendRandom(r, int(n));
        }

        private static void goenvs()
        {
            goenvs_unix();
        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the parent thread (main thread in case of bootstrap), can allocate memory.
        private static void mpreinit(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

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
            signalstack(_addr__g_.m.gsignal.stack);
            _g_.m.newSigstack = true;

            minitSignalMask();

        }

        // Called from dropm to undo the effect of an minit.
        //go:nosplit
        private static void unminit()
        {
            unminitSignals();
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
            ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
            sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
            sa.sa_mask = sigset_all;
            if (fn == funcPC(sighandler))
            {>>MARKER:FUNCTION_sigtramp_BLOCK_PREFIX<<
                fn = funcPC(sigtramp);
            }

            sa.sa_sigaction = fn;
            sigaction(i, _addr_sa, _addr_null);

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
            ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
            sigaction(i, _addr_null, _addr_sa);
            return sa.sa_sigaction;
        }

        // setSignaltstackSP sets the ss_sp field of a stackt.
        //go:nosplit
        private static void setSignalstackSP(ptr<stackt> _addr_s, System.UIntPtr sp)
        {
            ref stackt s = ref _addr_s.val;

            s.ss_sp = sp;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaddset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            mask.__bits[(i - 1L) / 32L] |= 1L << (int)(((uint32(i) - 1L) & 31L));
        }

        private static void sigdelset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            mask.__bits[(i - 1L) / 32L] &= 1L << (int)(((uint32(i) - 1L) & 31L));
        }

        //go:nosplit
        private static void fixsigcode(this ptr<sigctxt> _addr_c, uint sig)
        {
            ref sigctxt c = ref _addr_c.val;

        }

        private static void sysargs(int argc, ptr<ptr<byte>> _addr_argv)
        {
            ref ptr<byte> argv = ref _addr_argv.val;

            var n = argc + 1L; 

            // skip over argv, envp to get to auxv
            while (argv_index(argv, n) != null)
            {>>MARKER:FUNCTION_sysctl_BLOCK_PREFIX<<
                n++;
            } 

            // skip NULL separator
 

            // skip NULL separator
            n++; 

            // now argv+n is auxv
            ptr<array<System.UIntPtr>> auxv = new ptr<ptr<array<System.UIntPtr>>>(add(@unsafe.Pointer(argv), uintptr(n) * sys.PtrSize));
            sysauxv(auxv[..]);

        }

        private static readonly long _AT_NULL = (long)0L; // Terminates the vector
        private static readonly long _AT_PAGESZ = (long)6L; // Page size in bytes

        private static void sysauxv(slice<System.UIntPtr> auxv)
        {
            {
                long i = 0L;

                while (auxv[i] != _AT_NULL)
                {>>MARKER:FUNCTION_sigprocmask_BLOCK_PREFIX<<
                    var tag = auxv[i];
                    var val = auxv[i + 1L];

                    if (tag == _AT_PAGESZ) 
                        physPageSize = val;
                                        i += 2L;
                }

            }

        }

        // raise sends signal to the calling thread.
        //
        // It must be nosplit because it is used by the signal handler before
        // it definitely has a Go stack.
        //
        //go:nosplit
        private static void raise(uint sig)
        {
            lwp_kill(lwp_self(), int(sig));
        }

        private static void signalM(ptr<m> _addr_mp, long sig)
        {
            ref m mp = ref _addr_mp.val;

            lwp_kill(int32(mp.procid), sig);
        }
    }
}
