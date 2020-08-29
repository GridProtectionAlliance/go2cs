// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2020 August 29 08:18:44 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Go\src\runtime\os3_solaris.go
using @unsafe = go.@unsafe_package;
using static go.builtin;

namespace go
{
    public static partial class runtime_package
    {
        //go:cgo_export_dynamic runtime.end _end
        //go:cgo_export_dynamic runtime.etext _etext
        //go:cgo_export_dynamic runtime.edata _edata

        //go:cgo_import_dynamic libc____errno ___errno "libc.so"
        //go:cgo_import_dynamic libc_clock_gettime clock_gettime "libc.so"
        //go:cgo_import_dynamic libc_close close "libc.so"
        //go:cgo_import_dynamic libc_exit exit "libc.so"
        //go:cgo_import_dynamic libc_fstat fstat "libc.so"
        //go:cgo_import_dynamic libc_getcontext getcontext "libc.so"
        //go:cgo_import_dynamic libc_getrlimit getrlimit "libc.so"
        //go:cgo_import_dynamic libc_kill kill "libc.so"
        //go:cgo_import_dynamic libc_madvise madvise "libc.so"
        //go:cgo_import_dynamic libc_malloc malloc "libc.so"
        //go:cgo_import_dynamic libc_mmap mmap "libc.so"
        //go:cgo_import_dynamic libc_munmap munmap "libc.so"
        //go:cgo_import_dynamic libc_open open "libc.so"
        //go:cgo_import_dynamic libc_pthread_attr_destroy pthread_attr_destroy "libc.so"
        //go:cgo_import_dynamic libc_pthread_attr_getstack pthread_attr_getstack "libc.so"
        //go:cgo_import_dynamic libc_pthread_attr_init pthread_attr_init "libc.so"
        //go:cgo_import_dynamic libc_pthread_attr_setdetachstate pthread_attr_setdetachstate "libc.so"
        //go:cgo_import_dynamic libc_pthread_attr_setstack pthread_attr_setstack "libc.so"
        //go:cgo_import_dynamic libc_pthread_create pthread_create "libc.so"
        //go:cgo_import_dynamic libc_raise raise "libc.so"
        //go:cgo_import_dynamic libc_read read "libc.so"
        //go:cgo_import_dynamic libc_select select "libc.so"
        //go:cgo_import_dynamic libc_sched_yield sched_yield "libc.so"
        //go:cgo_import_dynamic libc_sem_init sem_init "libc.so"
        //go:cgo_import_dynamic libc_sem_post sem_post "libc.so"
        //go:cgo_import_dynamic libc_sem_reltimedwait_np sem_reltimedwait_np "libc.so"
        //go:cgo_import_dynamic libc_sem_wait sem_wait "libc.so"
        //go:cgo_import_dynamic libc_setitimer setitimer "libc.so"
        //go:cgo_import_dynamic libc_sigaction sigaction "libc.so"
        //go:cgo_import_dynamic libc_sigaltstack sigaltstack "libc.so"
        //go:cgo_import_dynamic libc_sigprocmask sigprocmask "libc.so"
        //go:cgo_import_dynamic libc_sysconf sysconf "libc.so"
        //go:cgo_import_dynamic libc_usleep usleep "libc.so"
        //go:cgo_import_dynamic libc_write write "libc.so"

        //go:linkname libc____errno libc____errno
        //go:linkname libc_clock_gettime libc_clock_gettime
        //go:linkname libc_close libc_close
        //go:linkname libc_exit libc_exit
        //go:linkname libc_fstat libc_fstat
        //go:linkname libc_getcontext libc_getcontext
        //go:linkname libc_getrlimit libc_getrlimit
        //go:linkname libc_kill libc_kill
        //go:linkname libc_madvise libc_madvise
        //go:linkname libc_malloc libc_malloc
        //go:linkname libc_mmap libc_mmap
        //go:linkname libc_munmap libc_munmap
        //go:linkname libc_open libc_open
        //go:linkname libc_pthread_attr_destroy libc_pthread_attr_destroy
        //go:linkname libc_pthread_attr_getstack libc_pthread_attr_getstack
        //go:linkname libc_pthread_attr_init libc_pthread_attr_init
        //go:linkname libc_pthread_attr_setdetachstate libc_pthread_attr_setdetachstate
        //go:linkname libc_pthread_attr_setstack libc_pthread_attr_setstack
        //go:linkname libc_pthread_create libc_pthread_create
        //go:linkname libc_raise libc_raise
        //go:linkname libc_read libc_read
        //go:linkname libc_select libc_select
        //go:linkname libc_sched_yield libc_sched_yield
        //go:linkname libc_sem_init libc_sem_init
        //go:linkname libc_sem_post libc_sem_post
        //go:linkname libc_sem_reltimedwait_np libc_sem_reltimedwait_np
        //go:linkname libc_sem_wait libc_sem_wait
        //go:linkname libc_setitimer libc_setitimer
        //go:linkname libc_sigaction libc_sigaction
        //go:linkname libc_sigaltstack libc_sigaltstack
        //go:linkname libc_sigprocmask libc_sigprocmask
        //go:linkname libc_sysconf libc_sysconf
        //go:linkname libc_usleep libc_usleep
        //go:linkname libc_write libc_write
        private static libcFunc libc____errno = default;        private static libcFunc libc_clock_gettime = default;        private static libcFunc libc_close = default;        private static libcFunc libc_exit = default;        private static libcFunc libc_fstat = default;        private static libcFunc libc_getcontext = default;        private static libcFunc libc_getrlimit = default;        private static libcFunc libc_kill = default;        private static libcFunc libc_madvise = default;        private static libcFunc libc_malloc = default;        private static libcFunc libc_mmap = default;        private static libcFunc libc_munmap = default;        private static libcFunc libc_open = default;        private static libcFunc libc_pthread_attr_destroy = default;        private static libcFunc libc_pthread_attr_getstack = default;        private static libcFunc libc_pthread_attr_init = default;        private static libcFunc libc_pthread_attr_setdetachstate = default;        private static libcFunc libc_pthread_attr_setstack = default;        private static libcFunc libc_pthread_create = default;        private static libcFunc libc_raise = default;        private static libcFunc libc_read = default;        private static libcFunc libc_sched_yield = default;        private static libcFunc libc_select = default;        private static libcFunc libc_sem_init = default;        private static libcFunc libc_sem_post = default;        private static libcFunc libc_sem_reltimedwait_np = default;        private static libcFunc libc_sem_wait = default;        private static libcFunc libc_setitimer = default;        private static libcFunc libc_sigaction = default;        private static libcFunc libc_sigaltstack = default;        private static libcFunc libc_sigprocmask = default;        private static libcFunc libc_sysconf = default;        private static libcFunc libc_usleep = default;        private static libcFunc libc_write = default;

        private static sigset sigset_all = new sigset([4]uint32{^uint32(0),^uint32(0),^uint32(0),^uint32(0)});

        private static int getncpu()
        {
            var n = int32(sysconf(__SC_NPROCESSORS_ONLN));
            if (n < 1L)
            {
                return 1L;
            }
            return n;
        }

        private static System.UIntPtr getPageSize()
        {
            var n = int32(sysconf(__SC_PAGESIZE));
            if (n <= 0L)
            {
                return 0L;
            }
            return uintptr(n);
        }

        private static void osinit()
        {
            ncpu = getncpu();
            physPageSize = getPageSize();
        }

        private static uint tstart_sysvicall(ref m newm)
;

        // May run with m.p==nil, so write barriers are not allowed.
        //go:nowritebarrier
        private static void newosproc(ref m mp, unsafe.Pointer _)
        {
            pthreadattr attr = default;            sigset oset = default;            pthread tid = default;            int ret = default;            ulong size = default;

            if (pthread_attr_init(ref attr) != 0L)
            {>>MARKER:FUNCTION_tstart_sysvicall_BLOCK_PREFIX<<
                throw("pthread_attr_init");
            }
            if (pthread_attr_setstack(ref attr, 0L, 0x200000UL) != 0L)
            {
                throw("pthread_attr_setstack");
            }
            if (pthread_attr_getstack(ref attr, @unsafe.Pointer(ref mp.g0.stack.hi), ref size) != 0L)
            {
                throw("pthread_attr_getstack");
            }
            mp.g0.stack.lo = mp.g0.stack.hi - uintptr(size);
            if (pthread_attr_setdetachstate(ref attr, _PTHREAD_CREATE_DETACHED) != 0L)
            {
                throw("pthread_attr_setdetachstate");
            } 

            // Disable signals during create, so that the new thread starts
            // with signals disabled. It will enable them in minit.
            sigprocmask(_SIG_SETMASK, ref sigset_all, ref oset);
            ret = pthread_create(ref tid, ref attr, funcPC(tstart_sysvicall), @unsafe.Pointer(mp));
            sigprocmask(_SIG_SETMASK, ref oset, null);
            if (ret != 0L)
            {
                print("runtime: failed to create new OS thread (have ", mcount(), " already; errno=", ret, ")\n");
                if (ret == -_EAGAIN)
                {
                    println("runtime: may need to increase max user processes (ulimit -u)");
                }
                throw("newosproc");
            }
        }

        private static void exitThread(ref uint wait)
        { 
            // We should never reach exitThread on Solaris because we let
            // libc clean up threads.
            throw("exitThread");
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

        private static void miniterrno()
;

        // Called to initialize a new m (including the bootstrap m).
        // Called on the new thread, cannot allocate memory.
        private static void minit()
        {
            asmcgocall(@unsafe.Pointer(funcPC(miniterrno)), @unsafe.Pointer(ref libc____errno));

            minitSignals();
        }

        // Called from dropm to undo the effect of an minit.
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
            ((uintptr.Value)(@unsafe.Pointer(ref sa._funcptr))).Value = fn;
            sigaction(i, ref sa, null);
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void setsigstack(uint i)
        {
            sigactiont sa = default;
            sigaction(i, null, ref sa);
            if (sa.sa_flags & _SA_ONSTACK != 0L)
            {>>MARKER:FUNCTION_miniterrno_BLOCK_PREFIX<<
                return;
            }
            sa.sa_flags |= _SA_ONSTACK;
            sigaction(i, ref sa, null);
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static System.UIntPtr getsig(uint i)
        {
            sigactiont sa = default;
            sigaction(i, null, ref sa);
            return ((uintptr.Value)(@unsafe.Pointer(ref sa._funcptr))).Value;
        }

        // setSignaltstackSP sets the ss_sp field of a stackt.
        //go:nosplit
        private static void setSignalstackSP(ref stackt s, System.UIntPtr sp)
        {
            (uintptr.Value)(@unsafe.Pointer(ref s.ss_sp)).Value;

            sp;
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaddset(ref sigset mask, long i)
        {
            mask.__sigbits[(i - 1L) / 32L] |= 1L << (int)(((uint32(i) - 1L) & 31L));
        }

        private static void sigdelset(ref sigset mask, long i)
        {
            mask.__sigbits[(i - 1L) / 32L] &= 1L << (int)(((uint32(i) - 1L) & 31L));
        }

        private static void fixsigcode(this ref sigctxt c, uint sig)
        {
        }

        //go:nosplit
        private static void semacreate(ref m mp)
        {
            if (mp.waitsema != 0L)
            {
                return;
            }
            ref semt sem = default;
            var _g_ = getg(); 

            // Call libc's malloc rather than malloc. This will
            // allocate space on the C heap. We can't call malloc
            // here because it could cause a deadlock.
            _g_.m.libcall.fn = uintptr(@unsafe.Pointer(ref libc_malloc));
            _g_.m.libcall.n = 1L;
            _g_.m.scratch = new mscratch();
            _g_.m.scratch.v[0L] = @unsafe.Sizeof(sem.Value);
            _g_.m.libcall.args = uintptr(@unsafe.Pointer(ref _g_.m.scratch));
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref _g_.m.libcall));
            sem = (semt.Value)(@unsafe.Pointer(_g_.m.libcall.r1));
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
                _m_.ts.tv_sec = ns / 1000000000L;
                _m_.ts.tv_nsec = ns % 1000000000L;

                _m_.libcall.fn = uintptr(@unsafe.Pointer(ref libc_sem_reltimedwait_np));
                _m_.libcall.n = 2L;
                _m_.scratch = new mscratch();
                _m_.scratch.v[0L] = _m_.waitsema;
                _m_.scratch.v[1L] = uintptr(@unsafe.Pointer(ref _m_.ts));
                _m_.libcall.args = uintptr(@unsafe.Pointer(ref _m_.scratch));
                asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref _m_.libcall));
                if (_m_.perrno != 0L.Value)
                {
                    if (_m_.perrno == _ETIMEDOUT || _m_.perrno == _EAGAIN || _m_.perrno == _EINTR.Value.Value.Value)
                    {
                        return -1L;
                    }
                    throw("sem_reltimedwait_np");
                }
                return 0L;
            }
            while (true)
            {
                _m_.libcall.fn = uintptr(@unsafe.Pointer(ref libc_sem_wait));
                _m_.libcall.n = 1L;
                _m_.scratch = new mscratch();
                _m_.scratch.v[0L] = _m_.waitsema;
                _m_.libcall.args = uintptr(@unsafe.Pointer(ref _m_.scratch));
                asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref _m_.libcall));
                if (_m_.libcall.r1 == 0L)
                {
                    break;
                }
                if (_m_.perrno == _EINTR.Value)
                {
                    continue;
                }
                throw("sem_wait");
            }

            return 0L;
        }

        //go:nosplit
        private static void semawakeup(ref m mp)
        {
            if (sem_post((semt.Value)(@unsafe.Pointer(mp.waitsema))) != 0L)
            {
                throw("sem_post");
            }
        }

        //go:nosplit
        private static int closefd(int fd)
        {
            return int32(sysvicall1(ref libc_close, uintptr(fd)));
        }

        //go:nosplit
        private static void exit(int r)
        {
            sysvicall1(ref libc_exit, uintptr(r));
        }

        //go:nosplit
        private static void getcontext(ref ucontext context)
        {
            sysvicall1(ref libc_getcontext, uintptr(@unsafe.Pointer(context)));
        }

        //go:nosplit
        private static void madvise(unsafe.Pointer addr, System.UIntPtr n, int flags)
        {
            sysvicall3(ref libc_madvise, uintptr(addr), uintptr(n), uintptr(flags));
        }

        //go:nosplit
        private static (unsafe.Pointer, long) mmap(unsafe.Pointer addr, System.UIntPtr n, int prot, int flags, int fd, uint off)
        {
            var (p, err) = doMmap(uintptr(addr), n, uintptr(prot), uintptr(flags), uintptr(fd), uintptr(off));
            if (p == ~uintptr(0L))
            {
                return (null, int(err));
            }
            return (@unsafe.Pointer(p), 0L);
        }

        //go:nosplit
        private static (System.UIntPtr, System.UIntPtr) doMmap(System.UIntPtr addr, System.UIntPtr n, System.UIntPtr prot, System.UIntPtr flags, System.UIntPtr fd, System.UIntPtr off)
        {
            libcall libcall = default;
            libcall.fn = uintptr(@unsafe.Pointer(ref libc_mmap));
            libcall.n = 6L;
            libcall.args = uintptr(noescape(@unsafe.Pointer(ref addr)));
            asmcgocall(@unsafe.Pointer(ref asmsysvicall6), @unsafe.Pointer(ref libcall));
            return (libcall.r1, libcall.err);
        }

        //go:nosplit
        private static void munmap(unsafe.Pointer addr, System.UIntPtr n)
        {
            sysvicall2(ref libc_munmap, uintptr(addr), uintptr(n));
        }

        private static void nanotime1()
;

        //go:nosplit
        private static long nanotime()
        {
            return int64(sysvicall0((libcFunc.Value)(@unsafe.Pointer(funcPC(nanotime1)))));
        }

        //go:nosplit
        private static int open(ref byte path, int mode, int perm)
        {
            return int32(sysvicall3(ref libc_open, uintptr(@unsafe.Pointer(path)), uintptr(mode), uintptr(perm)));
        }

        private static int pthread_attr_destroy(ref pthreadattr attr)
        {
            return int32(sysvicall1(ref libc_pthread_attr_destroy, uintptr(@unsafe.Pointer(attr))));
        }

        private static int pthread_attr_getstack(ref pthreadattr attr, unsafe.Pointer addr, ref ulong size)
        {
            return int32(sysvicall3(ref libc_pthread_attr_getstack, uintptr(@unsafe.Pointer(attr)), uintptr(addr), uintptr(@unsafe.Pointer(size))));
        }

        private static int pthread_attr_init(ref pthreadattr attr)
        {
            return int32(sysvicall1(ref libc_pthread_attr_init, uintptr(@unsafe.Pointer(attr))));
        }

        private static int pthread_attr_setdetachstate(ref pthreadattr attr, int state)
        {
            return int32(sysvicall2(ref libc_pthread_attr_setdetachstate, uintptr(@unsafe.Pointer(attr)), uintptr(state)));
        }

        private static int pthread_attr_setstack(ref pthreadattr attr, System.UIntPtr addr, ulong size)
        {
            return int32(sysvicall3(ref libc_pthread_attr_setstack, uintptr(@unsafe.Pointer(attr)), uintptr(addr), uintptr(size)));
        }

        private static int pthread_create(ref pthread thread, ref pthreadattr attr, System.UIntPtr fn, unsafe.Pointer arg)
        {
            return int32(sysvicall4(ref libc_pthread_create, uintptr(@unsafe.Pointer(thread)), uintptr(@unsafe.Pointer(attr)), uintptr(fn), uintptr(arg)));
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void raise(uint sig)
        {
            sysvicall1(ref libc_raise, uintptr(sig));
        }

        private static void raiseproc(uint sig)
        {
            var pid = sysvicall0(ref libc_getpid);
            sysvicall2(ref libc_kill, pid, uintptr(sig));
        }

        //go:nosplit
        private static int read(int fd, unsafe.Pointer buf, int nbyte)
        {
            return int32(sysvicall3(ref libc_read, uintptr(fd), uintptr(buf), uintptr(nbyte)));
        }

        //go:nosplit
        private static int sem_init(ref semt sem, int pshared, uint value)
        {
            return int32(sysvicall3(ref libc_sem_init, uintptr(@unsafe.Pointer(sem)), uintptr(pshared), uintptr(value)));
        }

        //go:nosplit
        private static int sem_post(ref semt sem)
        {
            return int32(sysvicall1(ref libc_sem_post, uintptr(@unsafe.Pointer(sem))));
        }

        //go:nosplit
        private static int sem_reltimedwait_np(ref semt sem, ref timespec timeout)
        {
            return int32(sysvicall2(ref libc_sem_reltimedwait_np, uintptr(@unsafe.Pointer(sem)), uintptr(@unsafe.Pointer(timeout))));
        }

        //go:nosplit
        private static int sem_wait(ref semt sem)
        {
            return int32(sysvicall1(ref libc_sem_wait, uintptr(@unsafe.Pointer(sem))));
        }

        private static void setitimer(int which, ref itimerval value, ref itimerval ovalue)
        {
            sysvicall3(ref libc_setitimer, uintptr(which), uintptr(@unsafe.Pointer(value)), uintptr(@unsafe.Pointer(ovalue)));
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaction(uint sig, ref sigactiont act, ref sigactiont oact)
        {
            sysvicall3(ref libc_sigaction, uintptr(sig), uintptr(@unsafe.Pointer(act)), uintptr(@unsafe.Pointer(oact)));
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigaltstack(ref stackt ss, ref stackt oss)
        {
            sysvicall2(ref libc_sigaltstack, uintptr(@unsafe.Pointer(ss)), uintptr(@unsafe.Pointer(oss)));
        }

        //go:nosplit
        //go:nowritebarrierrec
        private static void sigprocmask(int how, ref sigset set, ref sigset oset)
        {
            sysvicall3(ref libc_sigprocmask, uintptr(how), uintptr(@unsafe.Pointer(set)), uintptr(@unsafe.Pointer(oset)));
        }

        private static long sysconf(int name)
        {
            return int64(sysvicall1(ref libc_sysconf, uintptr(name)));
        }

        private static void usleep1(uint usec)
;

        //go:nosplit
        private static void usleep(uint µs)
        {
            usleep1(µs);
        }

        //go:nosplit
        private static int write(System.UIntPtr fd, unsafe.Pointer buf, int nbyte)
        {
            return int32(sysvicall3(ref libc_write, uintptr(fd), uintptr(buf), uintptr(nbyte)));
        }

        private static void osyield1()
;

        //go:nosplit
        private static void osyield()
        {
            var _g_ = getg(); 

            // Check the validity of m because we might be called in cgo callback
            // path early enough where there isn't a m available yet.
            if (_g_ != null && _g_.m != null)
            {>>MARKER:FUNCTION_osyield1_BLOCK_PREFIX<<
                sysvicall0(ref libc_sched_yield);
                return;
            }
            osyield1();
        }
    }
}
