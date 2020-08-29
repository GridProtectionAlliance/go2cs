// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:49 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_dragonfly.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private static readonly long _NSIG = 33L;
        private static readonly long _SI_USER = 0L;
        private static readonly long _SS_DISABLE = 4L;
        private static readonly long _RLIMIT_AS = 10L;
        private static readonly long _SIG_BLOCK = 1L;
        private static readonly long _SIG_UNBLOCK = 2L;
        private static readonly long _SIG_SETMASK = 3L;

        private partial struct mOS
        {
        }

        //go:noescape
        private static int lwp_create(ref lwpparams param)
;

        //go:noescape
        private static void sigaltstack(ref stackt @new, ref stackt old)
;

        //go:noescape
        private static void sigaction(uint sig, ref sigactiont @new, ref sigactiont old)
;

        //go:noescape
        private static void sigprocmask(int how, ref sigset @new, ref sigset old)
;

        //go:noescape
        private static void setitimer(int mode, ref itimerval @new, ref itimerval old)
;

        //go:noescape
        private static int sysctl(ref uint mib, uint miblen, ref byte @out, ref System.UIntPtr size, ref byte dst, System.UIntPtr ndst)
;

        //go:noescape
        private static int getrlimit(int kind, unsafe.Pointer limit)
;

        private static void raise(uint sig)
;
        private static void raiseproc(uint sig)
;

        //go:noescape
        private static int sys_umtx_sleep(ref uint addr, int val, int timeout)
;

        //go:noescape
        private static int sys_umtx_wakeup(ref uint addr, int val)
;

        private static void osyield()
;

        private static readonly long stackSystem = 0L;

        // From DragonFly's <sys/sysctl.h>


        // From DragonFly's <sys/sysctl.h>
        private static readonly long _CTL_HW = 6L;
        private static readonly long _HW_NCPU = 3L;
        private static readonly long _HW_PAGESIZE = 7L;

        private static sigset sigset_all = new sigset([4]uint32{^uint32(0),^uint32(0),^uint32(0),^uint32(0)});

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
            {>>MARKER:FUNCTION_sys_umtx_wakeup_BLOCK_PREFIX<<
                return uintptr(out);
            }
            return 0L;
        }

        //go:nosplit
        private static void futexsleep(ref uint addr, uint val, long ns)
        {
            systemstack(() =>
            {>>MARKER:FUNCTION_sys_umtx_sleep_BLOCK_PREFIX<<
                futexsleep1(addr, val, ns);
            });
        }

        private static void futexsleep1(ref uint addr, uint val, long ns)
        {
            int timeout = default;
            if (ns >= 0L)
            {>>MARKER:FUNCTION_raiseproc_BLOCK_PREFIX<< 
                // The timeout is specified in microseconds - ensure that we
                // do not end up dividing to zero, which would put us to sleep
                // indefinitely...
                timeout = timediv(ns, 1000L, null);
                if (timeout == 0L)
                {>>MARKER:FUNCTION_raise_BLOCK_PREFIX<<
                    timeout = 1L;
                }
            } 

            // sys_umtx_sleep will return EWOULDBLOCK (EAGAIN) when the timeout
            // expires or EBUSY if the mutex value does not match.
            var ret = sys_umtx_sleep(addr, int32(val), timeout);
            if (ret >= 0L || ret == -_EINTR || ret == -_EAGAIN || ret == -_EBUSY)
            {>>MARKER:FUNCTION_getrlimit_BLOCK_PREFIX<<
                return;
            }
            print("umtx_sleep addr=", addr, " val=", val, " ret=", ret, "\n") * (int32.Value)(@unsafe.Pointer(uintptr(0x1005UL)));

            0x1005UL;
        }

        //go:nosplit
        private static void futexwakeup(ref uint addr, uint cnt)
        {
            var ret = sys_umtx_wakeup(addr, int32(cnt));
            if (ret >= 0L)
            {>>MARKER:FUNCTION_sysctl_BLOCK_PREFIX<<
                return;
            }
            systemstack(() =>
            {>>MARKER:FUNCTION_setitimer_BLOCK_PREFIX<<
                print("umtx_wake_addr=", addr, " ret=", ret, "\n") * (int32.Value)(@unsafe.Pointer(uintptr(0x1006UL)));

                0x1006UL;
            });
        }

        private static void lwp_start(System.UIntPtr _p0)
;

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrier
        private static void newosproc(ref m mp, unsafe.Pointer stk)
        {
            if (false)
            {>>MARKER:FUNCTION_lwp_start_BLOCK_PREFIX<<
                print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " lwp_start=", funcPC(lwp_start), " id=", mp.id, " ostk=", ref mp, "\n");
            }
            sigset oset = default;
            sigprocmask(_SIG_SETMASK, ref sigset_all, ref oset);

            lwpparams @params = new lwpparams(start_func:funcPC(lwp_start),arg:unsafe.Pointer(mp),stack:uintptr(stk),tid1:unsafe.Pointer(&mp.procid),tid2:nil,); 

            // TODO: Check for error.
            lwp_create(ref params);
            sigprocmask(_SIG_SETMASK, ref oset, null);
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
            // m.procid is a uint64, but lwp_start writes an int32. Fix it up.
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
            /*
                                    TODO: Convert to Go when something actually uses the result.

                            Rlimit rl;
                            extern byte runtime·text[], runtime·end[];
                            uintptr used;

                            if(runtime·getrlimit(RLIMIT_AS, &rl) != 0)
                                return 0;
                            if(rl.rlim_cur >= 0x7fffffff)
                                return 0;

                            // Estimate our VM footprint excluding the heap.
                            // Not an exact science: use size of binary plus
                            // some room for thread stacks.
                            used = runtime·end - runtime·text + (64<<20);
                            if(used >= rl.rlim_cur)
                                return 0;

                            // If there's not at least 16 MB left, we're probably
                            // not going to be able to do much. Treat as no limit.
                            rl.rlim_cur -= used;
                            if(rl.rlim_cur < (16<<20))
                                return 0;

                            return rl.rlim_cur - used;
                */
            return 0L;
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
        {>>MARKER:FUNCTION_sigprocmask_BLOCK_PREFIX<<
        }
    }
}
