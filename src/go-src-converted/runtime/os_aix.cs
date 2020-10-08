// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// +build aix

// package runtime -- go2cs converted at 2020 October 08 03:21:48 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_aix.go
using cpu = go.@internal.cpu_package;
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        private static readonly ulong threadStackSize = (ulong)0x100000UL; // size of a thread stack allocated by OS

        // funcDescriptor is a structure representing a function descriptor
        // A variable with this type is always created in assembler
        private partial struct funcDescriptor
        {
            public System.UIntPtr fn;
            public System.UIntPtr toc;
            public System.UIntPtr envPointer; // unused in Golang
        }

        private partial struct mOS
        {
            public System.UIntPtr waitsema; // semaphore for parking on locks
            public System.UIntPtr perrno; // pointer to tls errno
        }

        //go:nosplit
        private static void semacreate(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            if (mp.waitsema != 0L)
            {
                return ;
            }

            ptr<semt> sem; 

            // Call libc's malloc rather than malloc. This will
            // allocate space on the C heap. We can't call mallocgc
            // here because it could cause a deadlock.
            sem = (semt.val)(malloc(@unsafe.Sizeof(sem.val)));
            if (sem_init(sem, 0L, 0L) != 0L)
            {
                throw("sem_init");
            }

            mp.waitsema = uintptr(@unsafe.Pointer(sem));

        }

        //go:nosplit
        private static int semasleep(long ns)
        {
            var _m_ = getg().m;
            if (ns >= 0L)
            {
                ref timespec ts = ref heap(out ptr<timespec> _addr_ts);

                if (clock_gettime(_CLOCK_REALTIME, _addr_ts) != 0L)
                {
                    throw("clock_gettime");
                }

                ts.tv_sec += ns / 1e9F;
                ts.tv_nsec += ns % 1e9F;
                if (ts.tv_nsec >= 1e9F)
                {
                    ts.tv_sec++;
                    ts.tv_nsec -= 1e9F;
                }

                {
                    var (r, err) = sem_timedwait((semt.val)(@unsafe.Pointer(_m_.waitsema)), _addr_ts);

                    if (r != 0L)
                    {
                        if (err == _ETIMEDOUT || err == _EAGAIN || err == _EINTR)
                        {
                            return -1L;
                        }

                        println("sem_timedwait err ", err, " ts.tv_sec ", ts.tv_sec, " ts.tv_nsec ", ts.tv_nsec, " ns ", ns, " id ", _m_.id);
                        throw("sem_timedwait");

                    }

                }

                return 0L;

            }

            while (true)
            {
                var (r1, err) = sem_wait((semt.val)(@unsafe.Pointer(_m_.waitsema)));
                if (r1 == 0L)
                {
                    break;
                }

                if (err == _EINTR)
                {
                    continue;
                }

                throw("sem_wait");

            }

            return 0L;

        }

        //go:nosplit
        private static void semawakeup(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            if (sem_post((semt.val)(@unsafe.Pointer(mp.waitsema))) != 0L)
            {
                throw("sem_post");
            }

        }

        private static void osinit()
        {
            ncpu = int32(sysconf(__SC_NPROCESSORS_ONLN));
            physPageSize = sysconf(__SC_PAGE_SIZE);
            setupSystemConf();
        }

        // newosproc0 is a version of newosproc that can be called before the runtime
        // is initialized.
        //
        // This function is not safe to use after initialization as it does not pass an M as fnarg.
        //
        //go:nosplit
        private static void newosproc0(System.UIntPtr stacksize, ptr<funcDescriptor> _addr_fn)
        {
            ref funcDescriptor fn = ref _addr_fn.val;

            ref pthread_attr attr = ref heap(out ptr<pthread_attr> _addr_attr);            ref sigset oset = ref heap(out ptr<sigset> _addr_oset);            ref pthread tid = ref heap(out ptr<pthread> _addr_tid);

            if (pthread_attr_init(_addr_attr) != 0L)
            {
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            }

            if (pthread_attr_setstacksize(_addr_attr, threadStackSize) != 0L)
            {
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            }

            if (pthread_attr_setdetachstate(_addr_attr, _PTHREAD_CREATE_DETACHED) != 0L)
            {
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            } 

            // Disable signals during create, so that the new thread starts
            // with signals disabled. It will enable them in minit.
            sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
            int ret = default;
            for (long tries = 0L; tries < 20L; tries++)
            { 
                // pthread_create can fail with EAGAIN for no reasons
                // but it will be ok if it retries.
                ret = pthread_create(_addr_tid, _addr_attr, fn, null);
                if (ret != _EAGAIN)
                {
                    break;
                }

                usleep(uint32(tries + 1L) * 1000L); // Milliseconds.
            }

            sigprocmask(_SIG_SETMASK, _addr_oset, null);
            if (ret != 0L)
            {
                write(2L, @unsafe.Pointer(_addr_failthreadcreate[0L]), int32(len(failthreadcreate)));
                exit(1L);
            }

        }

        private static slice<byte> failthreadcreate = (slice<byte>)"runtime: failed to create new OS thread\n";

        // Called to do synchronous initialization of Go code built with
        // -buildmode=c-archive or -buildmode=c-shared.
        // None of the Go runtime is initialized.
        //go:nosplit
        //go:nowritebarrierrec
        private static void libpreinit()
        {
            initsig(true);
        }

        // Ms related functions
        private static void mpreinit(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            mp.gsignal = malg(32L * 1024L); // AIX wants >= 8K
            mp.gsignal.m = mp;

        }

        // errno address must be retrieved by calling _Errno libc function.
        // This will return a pointer to errno
        private static void miniterrno()
        {
            var mp = getg().m;
            var (r, _) = syscall0(_addr_libc__Errno);
            mp.perrno = r;
        }

        private static void minit()
        {
            miniterrno();
            minitSignals();
            getg().m.procid = uint64(pthread_self());
        }

        private static void unminit()
        {
            unminitSignals();
        }

        // tstart is a function descriptor to _tstart defined in assembly.
        private static funcDescriptor tstart = default;

        private static void newosproc(ptr<m> _addr_mp)
        {
            ref m mp = ref _addr_mp.val;

            ref pthread_attr attr = ref heap(out ptr<pthread_attr> _addr_attr);            ref sigset oset = ref heap(out ptr<sigset> _addr_oset);            ref pthread tid = ref heap(out ptr<pthread> _addr_tid);

            if (pthread_attr_init(_addr_attr) != 0L)
            {
                throw("pthread_attr_init");
            }

            if (pthread_attr_setstacksize(_addr_attr, threadStackSize) != 0L)
            {
                throw("pthread_attr_getstacksize");
            }

            if (pthread_attr_setdetachstate(_addr_attr, _PTHREAD_CREATE_DETACHED) != 0L)
            {
                throw("pthread_attr_setdetachstate");
            } 

            // Disable signals during create, so that the new thread starts
            // with signals disabled. It will enable them in minit.
            sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
            int ret = default;
            for (long tries = 0L; tries < 20L; tries++)
            { 
                // pthread_create can fail with EAGAIN for no reasons
                // but it will be ok if it retries.
                ret = pthread_create(_addr_tid, _addr_attr, _addr_tstart, @unsafe.Pointer(mp));
                if (ret != _EAGAIN)
                {
                    break;
                }

                usleep(uint32(tries + 1L) * 1000L); // Milliseconds.
            }

            sigprocmask(_SIG_SETMASK, _addr_oset, null);
            if (ret != 0L)
            {
                print("runtime: failed to create new OS thread (have ", mcount(), " already; errno=", ret, ")\n");
                if (ret == _EAGAIN)
                {
                    println("runtime: may need to increase max user processes (ulimit -u)");
                }

                throw("newosproc");

            }

        }

        private static void exitThread(ptr<uint> _addr_wait)
        {
            ref uint wait = ref _addr_wait.val;
 
            // We should never reach exitThread on AIX because we let
            // libc clean up threads.
            throw("exitThread");

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

        /* SIGNAL */

        private static readonly long _NSIG = (long)256L;


        // sigtramp is a function descriptor to _sigtramp defined in assembly
        private static funcDescriptor sigtramp = default;

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsig(uint i, System.UIntPtr fn)
        {
            ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
            sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
            sa.sa_mask = sigset_all;
            if (fn == funcPC(sighandler))
            {
                fn = uintptr(@unsafe.Pointer(_addr_sigtramp));
            }

            sa.sa_handler = fn;
            sigaction(uintptr(i), _addr_sa, null);


        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsigstack(uint i)
        {
            ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
            sigaction(uintptr(i), null, _addr_sa);
            if (sa.sa_flags & _SA_ONSTACK != 0L)
            {
                return ;
            }

            sa.sa_flags |= _SA_ONSTACK;
            sigaction(uintptr(i), _addr_sa, null);

        }

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr getsig(uint i)
        {
            ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
            sigaction(uintptr(i), null, _addr_sa);
            return sa.sa_handler;
        }

        // setSignaltstackSP sets the ss_sp field of a stackt.
        //go:nosplit
        private static void setSignalstackSP(ptr<stackt> _addr_s, System.UIntPtr sp)
        {
            ref stackt s = ref _addr_s.val;

            (uintptr.val)(@unsafe.Pointer(_addr_s.ss_sp)).val;

            sp;

        }

        //go:nosplit
        private static void fixsigcode(this ptr<sigctxt> _addr_c, uint sig)
        {
            ref sigctxt c = ref _addr_c.val;


            if (sig == _SIGPIPE) 
                // For SIGPIPE, c.sigcode() isn't set to _SI_USER as on Linux.
                // Therefore, raisebadsignal won't raise SIGPIPE again if
                // it was deliver in a non-Go thread.
                c.set_sigcode(_SI_USER);
            
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaddset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            (mask)[(i - 1L) / 64L] |= 1L << (int)(((uint32(i) - 1L) & 63L));
        }

        private static void sigdelset(ptr<sigset> _addr_mask, long i)
        {
            ref sigset mask = ref _addr_mask.val;

            (mask)[(i - 1L) / 64L] &= 1L << (int)(((uint32(i) - 1L) & 63L));
        }

        private static readonly long _CLOCK_REALTIME = (long)9L;
        private static readonly long _CLOCK_MONOTONIC = (long)10L;


        //go:nosplit
        private static long nanotime1()
        {
            ptr<timespec> tp = addr(new timespec());
            if (clock_gettime(_CLOCK_REALTIME, tp) != 0L)
            {
                throw("syscall clock_gettime failed");
            }

            return tp.tv_sec * 1000000000L + tp.tv_nsec;

        }

        private static (long, int) walltime1()
        {
            long sec = default;
            int nsec = default;

            ptr<timespec> ts = addr(new timespec());
            if (clock_gettime(_CLOCK_REALTIME, ts) != 0L)
            {
                throw("syscall clock_gettime failed");
            }

            return (ts.tv_sec, int32(ts.tv_nsec));

        }

 
        // getsystemcfg constants
        private static readonly long _SC_IMPL = (long)2L;
        private static readonly ulong _IMPL_POWER8 = (ulong)0x10000UL;
        private static readonly ulong _IMPL_POWER9 = (ulong)0x20000UL;


        // setupSystemConf retrieves information about the CPU and updates
        // cpu.HWCap variables.
        private static void setupSystemConf()
        {
            var impl = getsystemcfg(_SC_IMPL);
            if (impl & _IMPL_POWER8 != 0L)
            {
                cpu.HWCap2 |= cpu.PPC_FEATURE2_ARCH_2_07;
            }

            if (impl & _IMPL_POWER9 != 0L)
            {
                cpu.HWCap2 |= cpu.PPC_FEATURE2_ARCH_3_00;
            }

        }

        //go:nosplit
        private static int fcntl(int fd, int cmd, int arg)
        {
            var (r, _) = syscall3(_addr_libc_fcntl, uintptr(fd), uintptr(cmd), uintptr(arg));
            return int32(r);
        }

        //go:nosplit
        private static void closeonexec(int fd)
        {
            fcntl(fd, _F_SETFD, _FD_CLOEXEC);
        }

        //go:nosplit
        private static void setNonblock(int fd)
        {
            var flags = fcntl(fd, _F_GETFL, 0L);
            fcntl(fd, _F_SETFL, flags | _O_NONBLOCK);
        }
    }
}
