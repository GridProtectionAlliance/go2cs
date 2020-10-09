// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 09 04:47:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_openbsd.go
using atomic = go.runtime.@internal.atomic_package;
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
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
        private static sigset obsdsigprocmask(int how, sigset @new)
;

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigprocmask(int how, ptr<sigset> _addr_@new, ptr<sigset> _addr_old)
        {
            ref sigset @new = ref _addr_@new.val;
            ref sigset old = ref _addr_old.val;

            var n = sigset(0L);
            if (new != null)
            {>>MARKER:FUNCTION_obsdsigprocmask_BLOCK_PREFIX<<
                n = new.val;
            }

            var r = obsdsigprocmask(how, n);
            if (old != null)
            {>>MARKER:FUNCTION_sigaltstack_BLOCK_PREFIX<<
                old = r;
            }

        }

        //go:noescape
        private static int sysctl(ptr<uint> mib, uint miblen, ptr<byte> @out, ptr<System.UIntPtr> size, ptr<byte> dst, System.UIntPtr ndst)
;

        private static void raiseproc(uint sig)
;

        private static int getthrid()
;
        private static void thrkill(int tid, long sig)
;

        //go:noescape
        private static int tfork(ptr<tforkt> param, System.UIntPtr psize, ptr<m> mm, ptr<g> gg, System.UIntPtr fn)
;

        //go:noescape
        private static int thrsleep(System.UIntPtr ident, int clock_id, ptr<timespec> tsp, System.UIntPtr @lock, ptr<uint> abort)
;

        //go:noescape
        private static int thrwakeup(System.UIntPtr ident, int n)
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
        private static readonly var _EWOULDBLOCK = _EAGAIN;
        private static readonly long _ENOTSUP = (long)91L; 

        // From OpenBSD's sys/time.h
        private static readonly long _CLOCK_REALTIME = (long)0L;
        private static readonly long _CLOCK_VIRTUAL = (long)1L;
        private static readonly long _CLOCK_PROF = (long)2L;
        private static readonly long _CLOCK_MONOTONIC = (long)3L;


        private partial struct sigset // : uint
        {
        }

        private static var sigset_all = ~sigset(0L);

        // From OpenBSD's <sys/sysctl.h>
        private static readonly long _CTL_KERN = (long)1L;
        private static readonly long _KERN_OSREV = (long)3L;

        private static readonly long _CTL_HW = (long)6L;
        private static readonly long _HW_NCPU = (long)3L;
        private static readonly long _HW_PAGESIZE = (long)7L;
        private static readonly long _HW_NCPUONLINE = (long)25L;


        private static (int, bool) sysctlInt(slice<uint> mib)
        {
            int _p0 = default;
            bool _p0 = default;

            ref int @out = ref heap(out ptr<int> _addr_@out);
            ref var nout = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_nout);
            var ret = sysctl(_addr_mib[0L], uint32(len(mib)), _addr_(byte.val)(@unsafe.Pointer(_addr_out)), _addr_nout, _addr_null, 0L);
            if (ret < 0L)
            {>>MARKER:FUNCTION_setNonblock_BLOCK_PREFIX<<
                return (0L, false);
            }

            return (out, true);

        }

        private static int getncpu()
        { 
            // Try hw.ncpuonline first because hw.ncpu would report a number twice as
            // high as the actual CPUs running on OpenBSD 6.4 with hyperthreading
            // disabled (hw.smt=0). See https://golang.org/issue/30127
            {
                var n__prev1 = n;

                var (n, ok) = sysctlInt(new slice<uint>(new uint[] { _CTL_HW, _HW_NCPUONLINE }));

                if (ok)
                {>>MARKER:FUNCTION_closeonexec_BLOCK_PREFIX<<
                    return int32(n);
                }

                n = n__prev1;

            }

            {
                var n__prev1 = n;

                (n, ok) = sysctlInt(new slice<uint>(new uint[] { _CTL_HW, _HW_NCPU }));

                if (ok)
                {>>MARKER:FUNCTION_pipe2_BLOCK_PREFIX<<
                    return int32(n);
                }

                n = n__prev1;

            }

            return 1L;

        }

        private static System.UIntPtr getPageSize()
        {
            {
                var (ps, ok) = sysctlInt(new slice<uint>(new uint[] { _CTL_HW, _HW_PAGESIZE }));

                if (ok)
                {>>MARKER:FUNCTION_pipe_BLOCK_PREFIX<<
                    return uintptr(ps);
                }

            }

            return 0L;

        }

        private static long getOSRev()
        {
            {
                var (osrev, ok) = sysctlInt(new slice<uint>(new uint[] { _CTL_KERN, _KERN_OSREV }));

                if (ok)
                {>>MARKER:FUNCTION_kevent_BLOCK_PREFIX<<
                    return int(osrev);
                }

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

            // Compute sleep deadline.
            ptr<timespec> tsp;
            if (ns >= 0L)
            {>>MARKER:FUNCTION_kqueue_BLOCK_PREFIX<<
                ref timespec ts = ref heap(out ptr<timespec> _addr_ts);
                ts.setNsec(ns + nanotime());
                tsp = _addr_ts;
            }

            while (true)
            {>>MARKER:FUNCTION_osyield_BLOCK_PREFIX<<
                var v = atomic.Load(_addr__g_.m.waitsemacount);
                if (v > 0L)
                {>>MARKER:FUNCTION_thrwakeup_BLOCK_PREFIX<<
                    if (atomic.Cas(_addr__g_.m.waitsemacount, v, v - 1L))
                    {>>MARKER:FUNCTION_thrsleep_BLOCK_PREFIX<<
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
                var ret = thrsleep(uintptr(@unsafe.Pointer(_addr__g_.m.waitsemacount)), _CLOCK_MONOTONIC, tsp, 0L, _addr__g_.m.waitsemacount);
                if (ret == _EWOULDBLOCK)
                {>>MARKER:FUNCTION_tfork_BLOCK_PREFIX<<
                    return -1L;
                }

            }


        }

        //go:nosplit
        private static void semawakeup(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            atomic.Xadd(_addr_mp.waitsemacount, 1L);
            var ret = thrwakeup(uintptr(@unsafe.Pointer(_addr_mp.waitsemacount)), 1L);
            if (ret != 0L && ret != _ESRCH)
            {>>MARKER:FUNCTION_thrkill_BLOCK_PREFIX<< 
                // semawakeup can be called on signal stack.
                systemstack(() =>
                {>>MARKER:FUNCTION_getthrid_BLOCK_PREFIX<<
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
            {>>MARKER:FUNCTION_raiseproc_BLOCK_PREFIX<<
                print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " id=", mp.id, " ostk=", _addr_mp, "\n");
            } 

            // Stack pointer must point inside stack area (as marked with MAP_STACK),
            // rather than at the top of it.
            ref tforkt param = ref heap(new tforkt(tf_tcb:unsafe.Pointer(&mp.tls[0]),tf_tid:nil,tf_stack:uintptr(stk)-sys.PtrSize,), out ptr<tforkt> _addr_param);

            ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
            sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
            var ret = tfork(_addr_param, @unsafe.Sizeof(param), _addr_mp, _addr_mp.g0, funcPC(mstart));
            sigprocmask(_SIG_SETMASK, _addr_oset, _addr_null);

            if (ret < 0L)
            {>>MARKER:FUNCTION_sysctl_BLOCK_PREFIX<<
                print("runtime: failed to create new OS thread (have ", mcount() - 1L, " already; errno=", -ret, ")\n");
                if (ret == -_EAGAIN)
                {>>MARKER:FUNCTION_sigaction_BLOCK_PREFIX<<
                    println("runtime: may need to increase max user processes (ulimit -p)");
                }

                throw("runtime.newosproc");

            }

        }

        private static void osinit()
        {
            ncpu = getncpu();
            physPageSize = getPageSize();
            haveMapStack = getOSRev() >= 201805L; // OpenBSD 6.3
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
        // Called on the new thread, can not allocate memory.
        private static void minit()
        {
            getg().m.procid = uint64(getthrid());
            minitSignals();
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
            public uint sa_mask;
            public int sa_flags;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsig(uint i, System.UIntPtr fn)
        {
            ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
            sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
            sa.sa_mask = uint32(sigset_all);
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

            mask |= 1L << (int)((uint32(i) - 1L));
        }

        private static void sigdelset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            mask &= 1L << (int)((uint32(i) - 1L));
        }

        //go:nosplit
        private static void fixsigcode(this ptr<sigctxt> _addr_c, uint sig)
        {
            ref sigctxt c = ref _addr_c.val;

        }

        private static var haveMapStack = false;

        private static void osStackAlloc(ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;
 
            // OpenBSD 6.4+ requires that stacks be mapped with MAP_STACK.
            // It will check this on entry to system calls, traps, and
            // when switching to the alternate system stack.
            //
            // This function is called before s is used for any data, so
            // it's safe to simply re-map it.
            osStackRemap(_addr_s, _MAP_STACK);

        }

        private static void osStackFree(ptr<mspan> _addr_s)
        {
            ref mspan s = ref _addr_s.val;
 
            // Undo MAP_STACK.
            osStackRemap(_addr_s, 0L);

        }

        private static void osStackRemap(ptr<mspan> _addr_s, int flags)
        {
            ref mspan s = ref _addr_s.val;

            if (!haveMapStack)
            {>>MARKER:FUNCTION_setitimer_BLOCK_PREFIX<< 
                // OpenBSD prior to 6.3 did not have MAP_STACK and so
                // the following mmap will fail. But it also didn't
                // require MAP_STACK (obviously), so there's no need
                // to do the mmap.
                return ;

            }

            var (a, err) = mmap(@unsafe.Pointer(s.@base()), s.npages * pageSize, _PROT_READ | _PROT_WRITE, _MAP_PRIVATE | _MAP_ANON | _MAP_FIXED | flags, -1L, 0L);
            if (err != 0L || uintptr(a) != s.@base())
            {
                print("runtime: remapping stack memory ", hex(s.@base()), " ", s.npages * pageSize, " a=", a, " err=", err, "\n");
                throw("remapping stack memory failed");
            }

        }

        //go:nosplit
        private static void raise(uint sig)
        {
            thrkill(getthrid(), int(sig));
        }

        private static void signalM(ptr<m> _addr_mp, long sig)
        {
            ref m mp = ref _addr_mp.val;

            thrkill(int32(mp.procid), sig);
        }
    }
}
