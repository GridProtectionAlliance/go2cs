// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 October 08 03:21:51 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_dragonfly.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly long _NSIG = (long)33L;
        private static readonly long _SI_USER = (long)0L;
        private static readonly long _SS_DISABLE = (long)4L;
        private static readonly long _SIG_BLOCK = (long)1L;
        private static readonly long _SIG_UNBLOCK = (long)2L;
        private static readonly long _SIG_SETMASK = (long)3L;


        private partial struct mOS
        {
        }

        //go:noescape
        private static int lwp_create(ptr<lwpparams> param)
;

        //go:noescape
        private static void sigaltstack(ptr<stackt> @new, ptr<stackt> old)
;

        //go:noescape
        private static void sigaction(uint sig, ptr<sigactiont> @new, ptr<sigactiont> old)
;

        //go:noescape
        private static void sigprocmask(int how, ptr<sigset> @new, ptr<sigset> old)
;

        //go:noescape
        private static void setitimer(int mode, ptr<itimerval> @new, ptr<itimerval> old)
;

        //go:noescape
        private static int sysctl(ptr<uint> mib, uint miblen, ptr<byte> @out, ptr<System.UIntPtr> size, ptr<byte> dst, System.UIntPtr ndst)
;

        private static void raiseproc(uint sig)
;

        private static int lwp_gettid()
;
        private static void lwp_kill(int pid, int tid, long sig)
;

        //go:noescape
        private static int sys_umtx_sleep(ptr<uint> addr, int val, int timeout)
;

        //go:noescape
        private static int sys_umtx_wakeup(ptr<uint> addr, int val)
;

        private static void osyield()
;

        private static int kqueue()
;

        //go:noescape
        private static int kevent(int kq, ptr<keventt> ch, int nch, ptr<keventt> ev, int nev, ptr<timespec> ts)
;
        private static void closeonexec(int fd)
;
        private static void setNonblock(int fd)
;

        private static (int, int, int) pipe()
;

        private static readonly long stackSystem = (long)0L;

        // From DragonFly's <sys/sysctl.h>


        // From DragonFly's <sys/sysctl.h>
        private static readonly long _CTL_HW = (long)6L;
        private static readonly long _HW_NCPU = (long)3L;
        private static readonly long _HW_PAGESIZE = (long)7L;


        private static sigset sigset_all = new sigset([4]uint32{^uint32(0),^uint32(0),^uint32(0),^uint32(0)});

        private static int getncpu()
        {
            array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_NCPU });
            ref var @out = ref heap(uint32(0L), out ptr<var> _addr_@out);
            ref var nout = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_nout);
            var ret = sysctl(_addr_mib[0L], 2L, _addr_(byte.val)(@unsafe.Pointer(_addr_out)), _addr_nout, _addr_null, 0L);
            if (ret >= 0L)
            {>>MARKER:FUNCTION_pipe_BLOCK_PREFIX<<
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
            {>>MARKER:FUNCTION_setNonblock_BLOCK_PREFIX<<
                return uintptr(out);
            }

            return 0L;

        }

        //go:nosplit
        private static void futexsleep(ptr<uint> _addr_addr, uint val, long ns)
        {
            ref uint addr = ref _addr_addr.val;

            systemstack(() =>
            {>>MARKER:FUNCTION_closeonexec_BLOCK_PREFIX<<
                futexsleep1(_addr_addr, val, ns);
            });

        }

        private static void futexsleep1(ptr<uint> _addr_addr, uint val, long ns)
        {
            ref uint addr = ref _addr_addr.val;

            int timeout = default;
            if (ns >= 0L)
            {>>MARKER:FUNCTION_kevent_BLOCK_PREFIX<< 
                // The timeout is specified in microseconds - ensure that we
                // do not end up dividing to zero, which would put us to sleep
                // indefinitely...
                timeout = timediv(ns, 1000L, null);
                if (timeout == 0L)
                {>>MARKER:FUNCTION_kqueue_BLOCK_PREFIX<<
                    timeout = 1L;
                }

            } 

            // sys_umtx_sleep will return EWOULDBLOCK (EAGAIN) when the timeout
            // expires or EBUSY if the mutex value does not match.
            var ret = sys_umtx_sleep(_addr_addr, int32(val), timeout);
            if (ret >= 0L || ret == -_EINTR || ret == -_EAGAIN || ret == -_EBUSY)
            {>>MARKER:FUNCTION_osyield_BLOCK_PREFIX<<
                return ;
            }

            print("umtx_sleep addr=", addr, " val=", val, " ret=", ret, "\n") * (int32.val)(@unsafe.Pointer(uintptr(0x1005UL)));

            0x1005UL;

        }

        //go:nosplit
        private static void futexwakeup(ptr<uint> _addr_addr, uint cnt)
        {
            ref uint addr = ref _addr_addr.val;

            var ret = sys_umtx_wakeup(_addr_addr, int32(cnt));
            if (ret >= 0L)
            {>>MARKER:FUNCTION_sys_umtx_wakeup_BLOCK_PREFIX<<
                return ;
            }

            systemstack(() =>
            {>>MARKER:FUNCTION_sys_umtx_sleep_BLOCK_PREFIX<<
                print("umtx_wake_addr=", addr, " ret=", ret, "\n") * (int32.val)(@unsafe.Pointer(uintptr(0x1006UL)));

                0x1006UL;

            });

        }

        private static void lwp_start(System.UIntPtr _p0)
;

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrier
        private static void newosproc(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            var stk = @unsafe.Pointer(mp.g0.stack.hi);
            if (false)
            {>>MARKER:FUNCTION_lwp_start_BLOCK_PREFIX<<
                print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " lwp_start=", funcPC(lwp_start), " id=", mp.id, " ostk=", _addr_mp, "\n");
            }

            ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
            sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);

            ref lwpparams @params = ref heap(new lwpparams(start_func:funcPC(lwp_start),arg:unsafe.Pointer(mp),stack:uintptr(stk),tid1:nil,tid2:nil,), out ptr<lwpparams> _addr_@params); 

            // TODO: Check for error.
            lwp_create(_addr_params);
            sigprocmask(_SIG_SETMASK, _addr_oset, _addr_null);

        }

        private static void osinit()
        {
            ncpu = getncpu();
            if (physPageSize == 0L)
            {>>MARKER:FUNCTION_lwp_kill_BLOCK_PREFIX<<
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
            getg().m.procid = uint64(lwp_gettid());
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
            public int sa_flags;
            public sigset sa_mask;
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
            {>>MARKER:FUNCTION_lwp_gettid_BLOCK_PREFIX<<
                n++;
            } 

            // skip NULL separator
 

            // skip NULL separator
            n++;

            ptr<array<System.UIntPtr>> auxv = new ptr<ptr<array<System.UIntPtr>>>(add(@unsafe.Pointer(argv), uintptr(n) * sys.PtrSize));
            sysauxv(auxv[..]);

        }

        private static readonly long _AT_NULL = (long)0L;
        private static readonly long _AT_PAGESZ = (long)6L;


        private static void sysauxv(slice<System.UIntPtr> auxv)
        {
            {
                long i = 0L;

                while (auxv[i] != _AT_NULL)
                {>>MARKER:FUNCTION_raiseproc_BLOCK_PREFIX<<
                    var tag = auxv[i];
                    var val = auxv[i + 1L];

                    if (tag == _AT_PAGESZ) 
                        physPageSize = val;
                                        i += 2L;
                }

            }

        }

        // raise sends a signal to the calling thread.
        //
        // It must be nosplit because it is used by the signal handler before
        // it definitely has a Go stack.
        //
        //go:nosplit
        private static void raise(uint sig)
        {
            lwp_kill(-1L, lwp_gettid(), int(sig));
        }

        private static void signalM(ptr<m> _addr_mp, long sig)
        {
            ref m mp = ref _addr_mp.val;

            lwp_kill(-1L, int32(mp.procid), sig);
        }
    }
}
