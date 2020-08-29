// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:51 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_freebsd.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static unsafe partial class runtime_package
    {
        private partial struct mOS
        {
        }

        //go:noescape
        private static void thr_new(ref thrparam param, int size)
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
        private static int sys_umtx_op(ref uint addr, int mode, uint val, System.UIntPtr uaddr1, ref umtx_time ut)
;

        private static void osyield()
;

        // From FreeBSD's <sys/sysctl.h>
        private static readonly long _CTL_HW = 6L;
        private static readonly long _HW_PAGESIZE = 7L;

        private static sigset sigset_all = new sigset([4]uint32{^uint32(0),^uint32(0),^uint32(0),^uint32(0)});

        // Undocumented numbers from FreeBSD's lib/libc/gen/sysctlnametomib.c.
        private static readonly long _CTL_QUERY = 0L;
        private static readonly long _CTL_QUERY_MIB = 3L;

        // sysctlnametomib fill mib with dynamically assigned sysctl entries of name,
        // return count of effected mib slots, return 0 on error.
        private static uint sysctlnametomib(slice<byte> name, ref array<uint> mib)
        {
            array<uint> oid = new array<uint>(new uint[] { _CTL_QUERY, _CTL_QUERY_MIB });
            var miblen = uintptr(_CTL_MAXNAME);
            if (sysctl(ref oid[0L], 2L, (byte.Value)(@unsafe.Pointer(mib)), ref miblen, (byte.Value)(@unsafe.Pointer(ref name[0L])), (uintptr)(len(name))) < 0L)
            {>>MARKER:FUNCTION_osyield_BLOCK_PREFIX<<
                return 0L;
            }
            miblen /= @unsafe.Sizeof(uint32(0L));
            if (miblen <= 0L)
            {>>MARKER:FUNCTION_sys_umtx_op_BLOCK_PREFIX<<
                return 0L;
            }
            return uint32(miblen);
        }

        private static readonly long _CPU_CURRENT_PID = -1L; // Current process ID.

        //go:noescape
        private static int cpuset_getaffinity(long level, long which, long id, long size, ref byte mask)
;

        //go:systemstack
        private static int getncpu()
        { 
            // Use a large buffer for the CPU mask. We're on the system
            // stack, so this is fine, and we can't allocate memory for a
            // dynamically-sized buffer at this point.
            const long maxCPUs = 64L * 1024L;

            array<byte> mask = new array<byte>(maxCPUs / 8L);
            array<uint> mib = new array<uint>(_CTL_MAXNAME); 

            // According to FreeBSD's /usr/src/sys/kern/kern_cpuset.c,
            // cpuset_getaffinity return ERANGE when provided buffer size exceed the limits in kernel.
            // Querying kern.smp.maxcpus to calculate maximum buffer size.
            // See https://bugs.freebsd.org/bugzilla/show_bug.cgi?id=200802

            // Variable kern.smp.maxcpus introduced at Dec 23 2003, revision 123766,
            // with dynamically assigned sysctl entries.
            var miblen = sysctlnametomib((slice<byte>)"kern.smp.maxcpus", ref mib);
            if (miblen == 0L)
            {>>MARKER:FUNCTION_cpuset_getaffinity_BLOCK_PREFIX<<
                return 1L;
            } 

            // Query kern.smp.maxcpus.
            var dstsize = uintptr(4L);
            var maxcpus = uint32(0L);
            if (sysctl(ref mib[0L], miblen, (byte.Value)(@unsafe.Pointer(ref maxcpus)), ref dstsize, null, 0L) != 0L)
            {>>MARKER:FUNCTION_raiseproc_BLOCK_PREFIX<<
                return 1L;
            }
            var maskSize = int(maxcpus + 7L) / 8L;
            if (maskSize < sys.PtrSize)
            {>>MARKER:FUNCTION_raise_BLOCK_PREFIX<<
                maskSize = sys.PtrSize;
            }
            if (maskSize > len(mask))
            {>>MARKER:FUNCTION_getrlimit_BLOCK_PREFIX<<
                maskSize = len(mask);
            }
            if (cpuset_getaffinity(_CPU_LEVEL_WHICH, _CPU_WHICH_PID, _CPU_CURRENT_PID, maskSize, (byte.Value)(@unsafe.Pointer(ref mask[0L]))) != 0L)
            {>>MARKER:FUNCTION_sysctl_BLOCK_PREFIX<<
                return 1L;
            }
            var n = int32(0L);
            foreach (var (_, v) in mask[..maskSize])
            {
                while (v != 0L)
                {>>MARKER:FUNCTION_setitimer_BLOCK_PREFIX<<
                    n += int32(v & 1L);
                    v >>= 1L;
                }

            }
            if (n == 0L)
            {>>MARKER:FUNCTION_sigprocmask_BLOCK_PREFIX<<
                return 1L;
            }
            return n;
        }

        private static System.UIntPtr getPageSize()
        {
            array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_PAGESIZE });
            var @out = uint32(0L);
            var nout = @unsafe.Sizeof(out);
            var ret = sysctl(ref mib[0L], 2L, (byte.Value)(@unsafe.Pointer(ref out)), ref nout, null, 0L);
            if (ret >= 0L)
            {>>MARKER:FUNCTION_sigaction_BLOCK_PREFIX<<
                return uintptr(out);
            }
            return 0L;
        }

        // FreeBSD's umtx_op syscall is effectively the same as Linux's futex, and
        // thus the code is largely similar. See Linux implementation
        // and lock_futex.go for comments.

        //go:nosplit
        private static void futexsleep(ref uint addr, uint val, long ns)
        {
            systemstack(() =>
            {>>MARKER:FUNCTION_sigaltstack_BLOCK_PREFIX<<
                futexsleep1(addr, val, ns);
            });
        }

        private static void futexsleep1(ref uint addr, uint val, long ns)
        {
            ref umtx_time utp = default;
            if (ns >= 0L)
            {>>MARKER:FUNCTION_thr_new_BLOCK_PREFIX<<
                umtx_time ut = default;
                ut._clockid = _CLOCK_MONOTONIC;
                ut._timeout.set_sec(int64(timediv(ns, 1000000000L, (int32.Value)(@unsafe.Pointer(ref ut._timeout.tv_nsec)))));
                utp = ref ut;
            }
            var ret = sys_umtx_op(addr, _UMTX_OP_WAIT_UINT_PRIVATE, val, @unsafe.Sizeof(utp.Value), utp);
            if (ret >= 0L || ret == -_EINTR)
            {
                return;
            }
            print("umtx_wait addr=", addr, " val=", val, " ret=", ret, "\n") * (int32.Value)(@unsafe.Pointer(uintptr(0x1005UL)));

            0x1005UL;
        }

        //go:nosplit
        private static void futexwakeup(ref uint addr, uint cnt)
        {
            var ret = sys_umtx_op(addr, _UMTX_OP_WAKE_PRIVATE, cnt, 0L, null);
            if (ret >= 0L)
            {
                return;
            }
            systemstack(() =>
            {
                print("umtx_wake_addr=", addr, " ret=", ret, "\n");
            });
        }

        private static void thr_start()
;

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrier
        private static void newosproc(ref m mp, unsafe.Pointer stk)
        {
            if (false)
            {>>MARKER:FUNCTION_thr_start_BLOCK_PREFIX<<
                print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " thr_start=", funcPC(thr_start), " id=", mp.id, " ostk=", ref mp, "\n");
            } 

            // NOTE(rsc): This code is confused. stackbase is the top of the stack
            // and is equal to stk. However, it's working, so I'm not changing it.
            thrparam param = new thrparam(start_func:funcPC(thr_start),arg:unsafe.Pointer(mp),stack_base:mp.g0.stack.hi,stack_size:uintptr(stk)-mp.g0.stack.hi,child_tid:unsafe.Pointer(&mp.procid),parent_tid:nil,tls_base:unsafe.Pointer(&mp.tls[0]),tls_size:unsafe.Sizeof(mp.tls),);

            sigset oset = default;
            sigprocmask(_SIG_SETMASK, ref sigset_all, ref oset); 
            // TODO: Check for error.
            thr_new(ref param, int32(@unsafe.Sizeof(param)));
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
            // m.procid is a uint64, but thr_new writes a uint32 on 32-bit systems.
            // Fix it up. (Only matters on big-endian, but be clean anyway.)
            if (sys.PtrSize == 4L)
            {
                var _g_ = getg();
                _g_.m.procid = uint64(@unsafe.Pointer(ref _g_.m.procid).Value);
            } 

            // On FreeBSD before about April 2017 there was a bug such
            // that calling execve from a thread other than the main
            // thread did not reset the signal stack. That would confuse
            // minitSignals, which calls minitSignalStack, which checks
            // whether there is currently a signal stack and uses it if
            // present. To avoid this confusion, explicitly disable the
            // signal stack on the main thread when not running in a
            // library. This can be removed when we are confident that all
            // FreeBSD users are running a patched kernel. See issue #15658.
            {
                var gp = getg();

                if (!isarchive && !islibrary && gp.m == ref m0 && gp == gp.m.g0)
                {
                    stackt st = new stackt(ss_flags:_SS_DISABLE);
                    sigaltstack(ref st, null);
                }

            }

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
            public System.UIntPtr sa_handler;
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
            sa.sa_handler = fn;
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
            return sa.sa_handler;
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
        {
        }
    }
}
