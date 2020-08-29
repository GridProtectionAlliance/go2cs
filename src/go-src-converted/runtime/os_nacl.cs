// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:58 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os_nacl.go
using @unsafe = go.@unsafe_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class runtime_package
    {
        private partial struct mOS
        {
            public int waitsema; // semaphore for parking on locks
            public int waitsemacount;
            public int waitsemalock;
        }

        private static int nacl_exception_stack(System.UIntPtr p, int size)
;
        private static int nacl_exception_handler(System.UIntPtr fn, unsafe.Pointer arg)
;
        private static int nacl_sem_create(int flag)
;
        private static int nacl_sem_wait(int sem)
;
        private static int nacl_sem_post(int sem)
;
        private static int nacl_mutex_create(int flag)
;
        private static int nacl_mutex_lock(int mutex)
;
        private static int nacl_mutex_trylock(int mutex)
;
        private static int nacl_mutex_unlock(int mutex)
;
        private static int nacl_cond_create(int flag)
;
        private static int nacl_cond_wait(int cond, int n)
;
        private static int nacl_cond_signal(int cond)
;
        private static int nacl_cond_broadcast(int cond)
;

        //go:noescape
        private static int nacl_cond_timed_wait_abs(int cond, int @lock, ref timespec ts)
;
        private static int nacl_thread_create(System.UIntPtr fn, unsafe.Pointer stk, unsafe.Pointer tls, unsafe.Pointer xx)
;

        //go:noescape
        private static int nacl_nanosleep(ref timespec ts, ref timespec extra)
;
        private static long nanotime()
;
        private static (unsafe.Pointer, long) mmap(unsafe.Pointer addr, System.UIntPtr n, int prot, int flags, int fd, uint off)
;
        private static void exit(int code)
;
        private static void osyield()
;

        //go:noescape
        private static int write(System.UIntPtr fd, unsafe.Pointer p, int n)
;

        //go:linkname os_sigpipe os.sigpipe
        private static void os_sigpipe()
        {
            throw("too many writes on closed pipe");
        }

        private static void dieFromSignal(uint sig)
        {
            exit(2L);
        }

        private static void sigpanic()
        {
            var g = getg();
            if (!canpanic(g))
            {>>MARKER:FUNCTION_write_BLOCK_PREFIX<<
                throw("unexpected signal during runtime execution");
            } 

            // Native Client only invokes the exception handler for memory faults.
            g.sig = _SIGSEGV;
            panicmem();
        }

        private static void raiseproc(uint sig)
        {
        }

        // Stubs so tests can link correctly. These should never be called.
        private static int open(ref byte name, int mode, int perm)
;
        private static int closefd(int fd)
;
        private static int read(int fd, unsafe.Pointer p, int n)
;

        private partial struct sigset
        {
        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the parent thread (main thread in case of bootstrap), can allocate memory.
        private static void mpreinit(ref m mp)
        {
            mp.gsignal = malg(32L * 1024L);
            mp.gsignal.m = mp;
        }

        private static void sigtramp()
;

        //go:nosplit
        private static void msigsave(ref m mp)
        {
        }

        //go:nosplit
        private static void msigrestore(sigset sigmask)
        {
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void clearSignalHandlers()
        {
        }

        //go:nosplit
        private static void sigblock()
        {
        }

        // Called to initialize a new m (including the bootstrap m).
        // Called on the new thread, cannot allocate memory.
        private static void minit()
        {
            var _g_ = getg(); 

            // Initialize signal handling
            var ret = nacl_exception_stack(_g_.m.gsignal.stack.lo, 32L * 1024L);
            if (ret < 0L)
            {>>MARKER:FUNCTION_sigtramp_BLOCK_PREFIX<<
                print("runtime: nacl_exception_stack: error ", -ret, "\n");
            }
            ret = nacl_exception_handler(funcPC(sigtramp), null);
            if (ret < 0L)
            {>>MARKER:FUNCTION_read_BLOCK_PREFIX<<
                print("runtime: nacl_exception_handler: error ", -ret, "\n");
            }
        }

        // Called from dropm to undo the effect of an minit.
        private static void unminit()
        {
        }

        private static void osinit()
        {
            ncpu = 1L;
            getg().m.procid = 2L; 
            //nacl_exception_handler(funcPC(sigtramp), nil);
            physPageSize = 65536L;
        }

        private static @string signame(uint sig)
        {
            if (sig >= uint32(len(sigtable)))
            {>>MARKER:FUNCTION_closefd_BLOCK_PREFIX<<
                return "";
            }
            return sigtable[sig].name;
        }

        private static void crash()
        {
            (int32.Value)(null).Value;

            0L;
        }

        //go:noescape
        private static void getRandomData(slice<byte> _p0)
;

        private static void goenvs()
        {
            goenvs_unix();
        }

        private static void initsig(bool preinit)
        {
        }

        //go:nosplit
        private static void usleep(uint us)
        {
            timespec ts = default;

            ts.tv_sec = int64(us / 1e6F);
            ts.tv_nsec = int32(us % 1e6F) * 1e3F;
            nacl_nanosleep(ref ts, null);
        }

        private static void mstart_nacl()
;

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrier
        private static void newosproc(ref m mp, unsafe.Pointer stk)
        {
            mp.tls[0L] = uintptr(@unsafe.Pointer(mp.g0));
            mp.tls[1L] = uintptr(@unsafe.Pointer(mp));
            var ret = nacl_thread_create(funcPC(mstart_nacl), stk, @unsafe.Pointer(ref mp.tls[2L]), null);
            if (ret < 0L)
            {>>MARKER:FUNCTION_mstart_nacl_BLOCK_PREFIX<<
                print("nacl_thread_create: error ", -ret, "\n");
                throw("newosproc");
            }
        }

        //go:noescape
        private static void exitThread(ref uint wait)
;

        //go:nosplit
        private static void semacreate(ref m mp)
        {
            if (mp.waitsema != 0L)
            {>>MARKER:FUNCTION_exitThread_BLOCK_PREFIX<<
                return;
            }
            systemstack(() =>
            {>>MARKER:FUNCTION_getRandomData_BLOCK_PREFIX<<
                var mu = nacl_mutex_create(0L);
                if (mu < 0L)
                {>>MARKER:FUNCTION_open_BLOCK_PREFIX<<
                    print("nacl_mutex_create: error ", -mu, "\n");
                    throw("semacreate");
                }
                var c = nacl_cond_create(0L);
                if (c < 0L)
                {>>MARKER:FUNCTION_osyield_BLOCK_PREFIX<<
                    print("nacl_cond_create: error ", -c, "\n");
                    throw("semacreate");
                }
                mp.waitsema = c;
                mp.waitsemalock = mu;
            });
        }

        //go:nosplit
        private static int semasleep(long ns)
        {
            int ret = default;

            systemstack(() =>
            {>>MARKER:FUNCTION_exit_BLOCK_PREFIX<<
                var _g_ = getg();
                if (nacl_mutex_lock(_g_.m.waitsemalock) < 0L)
                {>>MARKER:FUNCTION_mmap_BLOCK_PREFIX<<
                    throw("semasleep");
                }
                while (_g_.m.waitsemacount == 0L)
                {>>MARKER:FUNCTION_nanotime_BLOCK_PREFIX<<
                    if (ns < 0L)
                    {>>MARKER:FUNCTION_nacl_nanosleep_BLOCK_PREFIX<<
                        if (nacl_cond_wait(_g_.m.waitsema, _g_.m.waitsemalock) < 0L)
                        {>>MARKER:FUNCTION_nacl_thread_create_BLOCK_PREFIX<<
                            throw("semasleep");
                        }
                    }
                    else
                    {>>MARKER:FUNCTION_nacl_cond_timed_wait_abs_BLOCK_PREFIX<<
                        timespec ts = default;
                        var end = ns + nanotime();
                        ts.tv_sec = end / 1e9F;
                        ts.tv_nsec = int32(end % 1e9F);
                        var r = nacl_cond_timed_wait_abs(_g_.m.waitsema, _g_.m.waitsemalock, ref ts);
                        if (r == -_ETIMEDOUT)
                        {>>MARKER:FUNCTION_nacl_cond_broadcast_BLOCK_PREFIX<<
                            nacl_mutex_unlock(_g_.m.waitsemalock);
                            ret = -1L;
                            return;
                        }
                        if (r < 0L)
                        {>>MARKER:FUNCTION_nacl_cond_signal_BLOCK_PREFIX<<
                            throw("semasleep");
                        }
                    }
                }


                _g_.m.waitsemacount = 0L;
                nacl_mutex_unlock(_g_.m.waitsemalock);
                ret = 0L;
            });
            return ret;
        }

        //go:nosplit
        private static void semawakeup(ref m mp)
        {
            systemstack(() =>
            {>>MARKER:FUNCTION_nacl_cond_wait_BLOCK_PREFIX<<
                if (nacl_mutex_lock(mp.waitsemalock) < 0L)
                {>>MARKER:FUNCTION_nacl_cond_create_BLOCK_PREFIX<<
                    throw("semawakeup");
                }
                if (mp.waitsemacount != 0L)
                {>>MARKER:FUNCTION_nacl_mutex_unlock_BLOCK_PREFIX<<
                    throw("semawakeup");
                }
                mp.waitsemacount = 1L;
                nacl_cond_signal(mp.waitsema);
                nacl_mutex_unlock(mp.waitsemalock);
            });
        }

        private static System.UIntPtr memlimit()
        {
            return 0L;
        }

        // This runs on a foreign stack, without an m or a g. No stack split.
        //go:nosplit
        //go:norace
        //go:nowritebarrierrec
        private static void badsignal(System.UIntPtr sig)
        {
            cgocallback(@unsafe.Pointer(funcPC(badsignalgo)), noescape(@unsafe.Pointer(ref sig)), @unsafe.Sizeof(sig), 0L);
        }

        private static void badsignalgo(System.UIntPtr sig)
        {
            if (!sigsend(uint32(sig)))
            {>>MARKER:FUNCTION_nacl_mutex_trylock_BLOCK_PREFIX<< 
                // A foreign thread received the signal sig, and the
                // Go code does not want to handle it.
                raisebadsignal(uint32(sig));
            }
        }

        // This runs on a foreign stack, without an m or a g. No stack split.
        //go:nosplit
        private static void badsignal2()
        {
            write(2L, @unsafe.Pointer(ref badsignal1[0L]), int32(len(badsignal1)));
            exit(2L);
        }

        private static slice<byte> badsignal1 = (slice<byte>)"runtime: signal received on thread not created by Go.\n";

        private static void raisebadsignal(uint sig)
        {
            badsignal2();
        }

        private static void madvise(unsafe.Pointer addr, System.UIntPtr n, int flags)
        {
        }
        private static void munmap(unsafe.Pointer addr, System.UIntPtr n)
        {
        }
        private static void setProcessCPUProfiler(int hz)
        {
        }
        private static void setThreadCPUProfiler(int hz)
        {
        }
        private static void sigdisable(uint _p0)
        {
        }
        private static void sigenable(uint _p0)
        {
        }
        private static void sigignore(uint _p0)
        {
        }
        private static void closeonexec(int _p0)
        {
        }

        // gsignalStack is unused on nacl.
        private partial struct gsignalStack
        {
        }

        private static uint writelock = default; // test-and-set spin lock for write

        /*
        An attempt at IRT. Doesn't work. See end of sys_nacl_amd64.s.

        void (*nacl_irt_query)(void);

        int8 nacl_irt_basic_v0_1_str[] = "nacl-irt-basic-0.1";
        void *nacl_irt_basic_v0_1[6]; // exit, gettod, clock, nanosleep, sched_yield, sysconf
        int32 nacl_irt_basic_v0_1_size = sizeof(nacl_irt_basic_v0_1);

        int8 nacl_irt_memory_v0_3_str[] = "nacl-irt-memory-0.3";
        void *nacl_irt_memory_v0_3[3]; // mmap, munmap, mprotect
        int32 nacl_irt_memory_v0_3_size = sizeof(nacl_irt_memory_v0_3);

        int8 nacl_irt_thread_v0_1_str[] = "nacl-irt-thread-0.1";
        void *nacl_irt_thread_v0_1[3]; // thread_create, thread_exit, thread_nice
        int32 nacl_irt_thread_v0_1_size = sizeof(nacl_irt_thread_v0_1);
        */
    }
}
