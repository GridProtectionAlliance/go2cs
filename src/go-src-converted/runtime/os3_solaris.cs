// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:22 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os3_solaris.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    //go:cgo_export_dynamic runtime.end _end
    //go:cgo_export_dynamic runtime.etext _etext
    //go:cgo_export_dynamic runtime.edata _edata

    //go:cgo_import_dynamic libc____errno ___errno "libc.so"
    //go:cgo_import_dynamic libc_clock_gettime clock_gettime "libc.so"
    //go:cgo_import_dynamic libc_exit exit "libc.so"
    //go:cgo_import_dynamic libc_getcontext getcontext "libc.so"
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
    //go:cgo_import_dynamic libc_pthread_self pthread_self "libc.so"
    //go:cgo_import_dynamic libc_pthread_kill pthread_kill "libc.so"
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
    //go:cgo_import_dynamic libc_pipe pipe "libc.so"
    //go:cgo_import_dynamic libc_pipe2 pipe2 "libc.so"

    //go:linkname libc____errno libc____errno
    //go:linkname libc_clock_gettime libc_clock_gettime
    //go:linkname libc_exit libc_exit
    //go:linkname libc_getcontext libc_getcontext
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
    //go:linkname libc_pthread_self libc_pthread_self
    //go:linkname libc_pthread_kill libc_pthread_kill
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
    //go:linkname libc_pipe libc_pipe
    //go:linkname libc_pipe2 libc_pipe2
private static libcFunc libc____errno = default;private static libcFunc libc_clock_gettime = default;private static libcFunc libc_exit = default;private static libcFunc libc_getcontext = default;private static libcFunc libc_kill = default;private static libcFunc libc_madvise = default;private static libcFunc libc_malloc = default;private static libcFunc libc_mmap = default;private static libcFunc libc_munmap = default;private static libcFunc libc_open = default;private static libcFunc libc_pthread_attr_destroy = default;private static libcFunc libc_pthread_attr_getstack = default;private static libcFunc libc_pthread_attr_init = default;private static libcFunc libc_pthread_attr_setdetachstate = default;private static libcFunc libc_pthread_attr_setstack = default;private static libcFunc libc_pthread_create = default;private static libcFunc libc_pthread_self = default;private static libcFunc libc_pthread_kill = default;private static libcFunc libc_raise = default;private static libcFunc libc_read = default;private static libcFunc libc_sched_yield = default;private static libcFunc libc_select = default;private static libcFunc libc_sem_init = default;private static libcFunc libc_sem_post = default;private static libcFunc libc_sem_reltimedwait_np = default;private static libcFunc libc_sem_wait = default;private static libcFunc libc_setitimer = default;private static libcFunc libc_sigaction = default;private static libcFunc libc_sigaltstack = default;private static libcFunc libc_sigprocmask = default;private static libcFunc libc_sysconf = default;private static libcFunc libc_usleep = default;private static libcFunc libc_write = default;private static libcFunc libc_pipe = default;private static libcFunc libc_pipe2 = default;


private static sigset sigset_all = new sigset([4]uint32{^uint32(0),^uint32(0),^uint32(0),^uint32(0)});

private static System.UIntPtr getPageSize() {
    var n = int32(sysconf(__SC_PAGESIZE));
    if (n <= 0) {
        return 0;
    }
    return uintptr(n);

}

private static void osinit() {
    ncpu = getncpu();
    if (physPageSize == 0) {
        physPageSize = getPageSize();
    }
}

private static uint tstart_sysvicall(ptr<m> newm);

// May run with m.p==nil, so write barriers are not allowed.
//go:nowritebarrier
private static void newosproc(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    ref pthreadattr attr = ref heap(out ptr<pthreadattr> _addr_attr);    ref sigset oset = ref heap(out ptr<sigset> _addr_oset);    ref pthread tid = ref heap(out ptr<pthread> _addr_tid);    int ret = default;    ref ulong size = ref heap(out ptr<ulong> _addr_size);

    if (pthread_attr_init(_addr_attr) != 0) {>>MARKER:FUNCTION_tstart_sysvicall_BLOCK_PREFIX<<
        throw("pthread_attr_init");
    }
    if (pthread_attr_setstack(_addr_attr, 0, 0x200000) != 0) {
        throw("pthread_attr_setstack");
    }
    if (pthread_attr_getstack(_addr_attr, @unsafe.Pointer(_addr_mp.g0.stack.hi), _addr_size) != 0) {
        throw("pthread_attr_getstack");
    }
    mp.g0.stack.lo = mp.g0.stack.hi - uintptr(size);
    if (pthread_attr_setdetachstate(_addr_attr, _PTHREAD_CREATE_DETACHED) != 0) {
        throw("pthread_attr_setdetachstate");
    }
    sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
    ret = pthread_create(_addr_tid, _addr_attr, funcPC(tstart_sysvicall), @unsafe.Pointer(mp));
    sigprocmask(_SIG_SETMASK, _addr_oset, _addr_null);
    if (ret != 0) {
        print("runtime: failed to create new OS thread (have ", mcount(), " already; errno=", ret, ")\n");
        if (ret == -_EAGAIN) {
            println("runtime: may need to increase max user processes (ulimit -u)");
        }
        throw("newosproc");

    }
}

private static void exitThread(ptr<uint> _addr_wait) {
    ref uint wait = ref _addr_wait.val;
 
    // We should never reach exitThread on Solaris because we let
    // libc clean up threads.
    throw("exitThread");

}

private static slice<byte> urandom_dev = (slice<byte>)"/dev/urandom\x00";

//go:nosplit
private static void getRandomData(slice<byte> r) {
    var fd = open(_addr_urandom_dev[0], 0, 0);
    var n = read(fd, @unsafe.Pointer(_addr_r[0]), int32(len(r)));
    closefd(fd);
    extendRandom(r, int(n));
}

private static void goenvs() {
    goenvs_unix();
}

// Called to initialize a new m (including the bootstrap m).
// Called on the parent thread (main thread in case of bootstrap), can allocate memory.
private static void mpreinit(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    mp.gsignal = malg(32 * 1024);
    mp.gsignal.m = mp;
}

private static void miniterrno();

// Called to initialize a new m (including the bootstrap m).
// Called on the new thread, cannot allocate memory.
private static void minit() {
    asmcgocall(@unsafe.Pointer(funcPC(miniterrno)), @unsafe.Pointer(_addr_libc____errno));

    minitSignals();

    getg().m.procid = uint64(pthread_self());
}

// Called from dropm to undo the effect of an minit.
private static void unminit() {
    unminitSignals();
}

// Called from exitm, but not from drop, to undo the effect of thread-owned
// resources in minit, semacreate, or elsewhere. Do not take locks after calling this.
private static void mdestroy(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

}

private static void sigtramp();

//go:nosplit
//go:nowritebarrierrec
private static void setsig(uint i, System.UIntPtr fn) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);

    sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
    sa.sa_mask = sigset_all;
    if (fn == funcPC(sighandler)) {>>MARKER:FUNCTION_sigtramp_BLOCK_PREFIX<<
        fn = funcPC(sigtramp);
    }
    ((uintptr.val)(@unsafe.Pointer(_addr_sa._funcptr))).val = fn;
    sigaction(i, _addr_sa, _addr_null);

}

//go:nosplit
//go:nowritebarrierrec
private static void setsigstack(uint i) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
    sigaction(i, _addr_null, _addr_sa);
    if (sa.sa_flags & _SA_ONSTACK != 0) {>>MARKER:FUNCTION_miniterrno_BLOCK_PREFIX<<
        return ;
    }
    sa.sa_flags |= _SA_ONSTACK;
    sigaction(i, _addr_sa, _addr_null);

}

//go:nosplit
//go:nowritebarrierrec
private static System.UIntPtr getsig(uint i) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
    sigaction(i, _addr_null, _addr_sa);
    return ((uintptr.val)(@unsafe.Pointer(_addr_sa._funcptr))).val;
}

// setSignaltstackSP sets the ss_sp field of a stackt.
//go:nosplit
private static void setSignalstackSP(ptr<stackt> _addr_s, System.UIntPtr sp) {
    ref stackt s = ref _addr_s.val;

    (uintptr.val)(@unsafe.Pointer(_addr_s.ss_sp)).val;

    sp;

}

//go:nosplit
//go:nowritebarrierrec
private static void sigaddset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    mask.__sigbits[(i - 1) / 32] |= 1 << (int)(((uint32(i) - 1) & 31));
}

private static void sigdelset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    mask.__sigbits[(i - 1) / 32] &= 1 << (int)(((uint32(i) - 1) & 31));
}

//go:nosplit
private static void fixsigcode(this ptr<sigctxt> _addr_c, uint sig) {
    ref sigctxt c = ref _addr_c.val;

}

//go:nosplit
private static void semacreate(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (mp.waitsema != 0) {
        return ;
    }
    ptr<semt> sem;
    var _g_ = getg(); 

    // Call libc's malloc rather than malloc. This will
    // allocate space on the C heap. We can't call malloc
    // here because it could cause a deadlock.
    _g_.m.libcall.fn = uintptr(@unsafe.Pointer(_addr_libc_malloc));
    _g_.m.libcall.n = 1;
    _g_.m.scratch = new mscratch();
    _g_.m.scratch.v[0] = @unsafe.Sizeof(sem.val);
    _g_.m.libcall.args = uintptr(@unsafe.Pointer(_addr__g_.m.scratch));
    asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr__g_.m.libcall));
    sem = (semt.val)(@unsafe.Pointer(_g_.m.libcall.r1));
    if (sem_init(sem, 0, 0) != 0) {
        throw("sem_init");
    }
    mp.waitsema = uintptr(@unsafe.Pointer(sem));

}

//go:nosplit
private static int semasleep(long ns) {
    var _m_ = getg().m;
    if (ns >= 0) {
        _m_.ts.tv_sec = ns / 1000000000;
        _m_.ts.tv_nsec = ns % 1000000000;

        _m_.libcall.fn = uintptr(@unsafe.Pointer(_addr_libc_sem_reltimedwait_np));
        _m_.libcall.n = 2;
        _m_.scratch = new mscratch();
        _m_.scratch.v[0] = _m_.waitsema;
        _m_.scratch.v[1] = uintptr(@unsafe.Pointer(_addr__m_.ts));
        _m_.libcall.args = uintptr(@unsafe.Pointer(_addr__m_.scratch));
        asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr__m_.libcall));
        if (_m_.perrno != 0.val) {
            if (_m_.perrno == _ETIMEDOUT || _m_.perrno == _EAGAIN || _m_.perrno == _EINTR.val) {
                return -1;
            }
            throw("sem_reltimedwait_np");
        }
        return 0;

    }
    while (true) {
        _m_.libcall.fn = uintptr(@unsafe.Pointer(_addr_libc_sem_wait));
        _m_.libcall.n = 1;
        _m_.scratch = new mscratch();
        _m_.scratch.v[0] = _m_.waitsema;
        _m_.libcall.args = uintptr(@unsafe.Pointer(_addr__m_.scratch));
        asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr__m_.libcall));
        if (_m_.libcall.r1 == 0) {
            break;
        }
        if (_m_.perrno == _EINTR.val) {
            continue;
        }
        throw("sem_wait");

    }
    return 0;

}

//go:nosplit
private static void semawakeup(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (sem_post(_addr_(semt.val)(@unsafe.Pointer(mp.waitsema))) != 0) {
        throw("sem_post");
    }
}

//go:nosplit
private static int closefd(int fd) {
    return int32(sysvicall1(_addr_libc_close, uintptr(fd)));
}

//go:nosplit
private static void exit(int r) {
    sysvicall1(_addr_libc_exit, uintptr(r));
}

//go:nosplit
private static void getcontext(ptr<ucontext> _addr_context) {
    ref ucontext context = ref _addr_context.val;

    sysvicall1(_addr_libc_getcontext, uintptr(@unsafe.Pointer(context)));
}

//go:nosplit
private static void madvise(unsafe.Pointer addr, System.UIntPtr n, int flags) {
    sysvicall3(_addr_libc_madvise, uintptr(addr), uintptr(n), uintptr(flags));
}

//go:nosplit
private static (unsafe.Pointer, nint) mmap(unsafe.Pointer addr, System.UIntPtr n, int prot, int flags, int fd, uint off) {
    unsafe.Pointer _p0 = default;
    nint _p0 = default;

    var (p, err) = doMmap(uintptr(addr), n, uintptr(prot), uintptr(flags), uintptr(fd), uintptr(off));
    if (p == ~uintptr(0)) {
        return (null, int(err));
    }
    return (@unsafe.Pointer(p), 0);

}

//go:nosplit
private static (System.UIntPtr, System.UIntPtr) doMmap(System.UIntPtr addr, System.UIntPtr n, System.UIntPtr prot, System.UIntPtr flags, System.UIntPtr fd, System.UIntPtr off) {
    System.UIntPtr _p0 = default;
    System.UIntPtr _p0 = default;

    ref libcall libcall = ref heap(out ptr<libcall> _addr_libcall);
    libcall.fn = uintptr(@unsafe.Pointer(_addr_libc_mmap));
    libcall.n = 6;
    libcall.args = uintptr(noescape(@unsafe.Pointer(_addr_addr)));
    asmcgocall(@unsafe.Pointer(_addr_asmsysvicall6x), @unsafe.Pointer(_addr_libcall));
    return (libcall.r1, libcall.err);
}

//go:nosplit
private static void munmap(unsafe.Pointer addr, System.UIntPtr n) {
    sysvicall2(_addr_libc_munmap, uintptr(addr), uintptr(n));
}

private static readonly nint _CLOCK_REALTIME = 3;
private static readonly nint _CLOCK_MONOTONIC = 4;


//go:nosplit
private static long nanotime1() {
    ref mts ts = ref heap(out ptr<mts> _addr_ts);
    sysvicall2(_addr_libc_clock_gettime, _CLOCK_MONOTONIC, uintptr(@unsafe.Pointer(_addr_ts)));
    return ts.tv_sec * 1e9F + ts.tv_nsec;
}

//go:nosplit
private static int open(ptr<byte> _addr_path, int mode, int perm) {
    ref byte path = ref _addr_path.val;

    return int32(sysvicall3(_addr_libc_open, uintptr(@unsafe.Pointer(path)), uintptr(mode), uintptr(perm)));
}

private static int pthread_attr_destroy(ptr<pthreadattr> _addr_attr) {
    ref pthreadattr attr = ref _addr_attr.val;

    return int32(sysvicall1(_addr_libc_pthread_attr_destroy, uintptr(@unsafe.Pointer(attr))));
}

private static int pthread_attr_getstack(ptr<pthreadattr> _addr_attr, unsafe.Pointer addr, ptr<ulong> _addr_size) {
    ref pthreadattr attr = ref _addr_attr.val;
    ref ulong size = ref _addr_size.val;

    return int32(sysvicall3(_addr_libc_pthread_attr_getstack, uintptr(@unsafe.Pointer(attr)), uintptr(addr), uintptr(@unsafe.Pointer(size))));
}

private static int pthread_attr_init(ptr<pthreadattr> _addr_attr) {
    ref pthreadattr attr = ref _addr_attr.val;

    return int32(sysvicall1(_addr_libc_pthread_attr_init, uintptr(@unsafe.Pointer(attr))));
}

private static int pthread_attr_setdetachstate(ptr<pthreadattr> _addr_attr, int state) {
    ref pthreadattr attr = ref _addr_attr.val;

    return int32(sysvicall2(_addr_libc_pthread_attr_setdetachstate, uintptr(@unsafe.Pointer(attr)), uintptr(state)));
}

private static int pthread_attr_setstack(ptr<pthreadattr> _addr_attr, System.UIntPtr addr, ulong size) {
    ref pthreadattr attr = ref _addr_attr.val;

    return int32(sysvicall3(_addr_libc_pthread_attr_setstack, uintptr(@unsafe.Pointer(attr)), uintptr(addr), uintptr(size)));
}

private static int pthread_create(ptr<pthread> _addr_thread, ptr<pthreadattr> _addr_attr, System.UIntPtr fn, unsafe.Pointer arg) {
    ref pthread thread = ref _addr_thread.val;
    ref pthreadattr attr = ref _addr_attr.val;

    return int32(sysvicall4(_addr_libc_pthread_create, uintptr(@unsafe.Pointer(thread)), uintptr(@unsafe.Pointer(attr)), uintptr(fn), uintptr(arg)));
}

private static pthread pthread_self() {
    return pthread(sysvicall0(_addr_libc_pthread_self));
}

private static void signalM(ptr<m> _addr_mp, nint sig) {
    ref m mp = ref _addr_mp.val;

    sysvicall2(_addr_libc_pthread_kill, uintptr(pthread(mp.procid)), uintptr(sig));
}

//go:nosplit
//go:nowritebarrierrec
private static void raise(uint sig) {
    sysvicall1(_addr_libc_raise, uintptr(sig));
}

private static void raiseproc(uint sig) {
    var pid = sysvicall0(_addr_libc_getpid);
    sysvicall2(_addr_libc_kill, pid, uintptr(sig));
}

//go:nosplit
private static int read(int fd, unsafe.Pointer buf, int nbyte) {
    var (r1, err) = sysvicall3Err(_addr_libc_read, uintptr(fd), uintptr(buf), uintptr(nbyte));
    {
        var c = int32(r1);

        if (c >= 0) {
            return c;
        }
    }

    return -int32(err);

}

//go:nosplit
private static int sem_init(ptr<semt> _addr_sem, int pshared, uint value) {
    ref semt sem = ref _addr_sem.val;

    return int32(sysvicall3(_addr_libc_sem_init, uintptr(@unsafe.Pointer(sem)), uintptr(pshared), uintptr(value)));
}

//go:nosplit
private static int sem_post(ptr<semt> _addr_sem) {
    ref semt sem = ref _addr_sem.val;

    return int32(sysvicall1(_addr_libc_sem_post, uintptr(@unsafe.Pointer(sem))));
}

//go:nosplit
private static int sem_reltimedwait_np(ptr<semt> _addr_sem, ptr<timespec> _addr_timeout) {
    ref semt sem = ref _addr_sem.val;
    ref timespec timeout = ref _addr_timeout.val;

    return int32(sysvicall2(_addr_libc_sem_reltimedwait_np, uintptr(@unsafe.Pointer(sem)), uintptr(@unsafe.Pointer(timeout))));
}

//go:nosplit
private static int sem_wait(ptr<semt> _addr_sem) {
    ref semt sem = ref _addr_sem.val;

    return int32(sysvicall1(_addr_libc_sem_wait, uintptr(@unsafe.Pointer(sem))));
}

private static void setitimer(int which, ptr<itimerval> _addr_value, ptr<itimerval> _addr_ovalue) {
    ref itimerval value = ref _addr_value.val;
    ref itimerval ovalue = ref _addr_ovalue.val;

    sysvicall3(_addr_libc_setitimer, uintptr(which), uintptr(@unsafe.Pointer(value)), uintptr(@unsafe.Pointer(ovalue)));
}

//go:nosplit
//go:nowritebarrierrec
private static void sigaction(uint sig, ptr<sigactiont> _addr_act, ptr<sigactiont> _addr_oact) {
    ref sigactiont act = ref _addr_act.val;
    ref sigactiont oact = ref _addr_oact.val;

    sysvicall3(_addr_libc_sigaction, uintptr(sig), uintptr(@unsafe.Pointer(act)), uintptr(@unsafe.Pointer(oact)));
}

//go:nosplit
//go:nowritebarrierrec
private static void sigaltstack(ptr<stackt> _addr_ss, ptr<stackt> _addr_oss) {
    ref stackt ss = ref _addr_ss.val;
    ref stackt oss = ref _addr_oss.val;

    sysvicall2(_addr_libc_sigaltstack, uintptr(@unsafe.Pointer(ss)), uintptr(@unsafe.Pointer(oss)));
}

//go:nosplit
//go:nowritebarrierrec
private static void sigprocmask(int how, ptr<sigset> _addr_set, ptr<sigset> _addr_oset) {
    ref sigset set = ref _addr_set.val;
    ref sigset oset = ref _addr_oset.val;

    sysvicall3(_addr_libc_sigprocmask, uintptr(how), uintptr(@unsafe.Pointer(set)), uintptr(@unsafe.Pointer(oset)));
}

private static long sysconf(int name) {
    return int64(sysvicall1(_addr_libc_sysconf, uintptr(name)));
}

private static void usleep1(uint usec);

//go:nosplit
private static void usleep_no_g(uint µs) {
    usleep1(µs);
}

//go:nosplit
private static void usleep(uint µs) {
    usleep1(µs);
}

private static (long, int) walltime() {
    long sec = default;
    int nsec = default;

    ref mts ts = ref heap(out ptr<mts> _addr_ts);
    sysvicall2(_addr_libc_clock_gettime, _CLOCK_REALTIME, uintptr(@unsafe.Pointer(_addr_ts)));
    return (ts.tv_sec, int32(ts.tv_nsec));
}

//go:nosplit
private static int write1(System.UIntPtr fd, unsafe.Pointer buf, int nbyte) {
    var (r1, err) = sysvicall3Err(_addr_libc_write, fd, uintptr(buf), uintptr(nbyte));
    {
        var c = int32(r1);

        if (c >= 0) {>>MARKER:FUNCTION_usleep1_BLOCK_PREFIX<<
            return c;
        }
    }

    return -int32(err);

}

//go:nosplit
private static (int, int, int) pipe() {
    int r = default;
    int w = default;
    int errno = default;

    ref array<int> p = ref heap(new array<int>(2), out ptr<array<int>> _addr_p);
    var (_, e) = sysvicall1Err(_addr_libc_pipe, uintptr(noescape(@unsafe.Pointer(_addr_p))));
    return (p[0], p[1], int32(e));
}

//go:nosplit
private static (int, int, int) pipe2(int flags) {
    int r = default;
    int w = default;
    int errno = default;

    ref array<int> p = ref heap(new array<int>(2), out ptr<array<int>> _addr_p);
    var (_, e) = sysvicall2Err(_addr_libc_pipe2, uintptr(noescape(@unsafe.Pointer(_addr_p))), uintptr(flags));
    return (p[0], p[1], int32(e));
}

//go:nosplit
private static void closeonexec(int fd) {
    fcntl(fd, _F_SETFD, _FD_CLOEXEC);
}

//go:nosplit
private static void setNonblock(int fd) {
    var flags = fcntl(fd, _F_GETFL, 0);
    fcntl(fd, _F_SETFL, flags | _O_NONBLOCK);
}

private static void osyield1();

//go:nosplit
private static void osyield_no_g() {
    osyield1();
}

//go:nosplit
private static void osyield() {
    sysvicall0(_addr_libc_sched_yield);
}

//go:linkname executablePath os.executablePath
private static @string executablePath = default;

private static void sysargs(int argc, ptr<ptr<byte>> _addr_argv) {
    ref ptr<byte> argv = ref _addr_argv.val;

    var n = argc + 1; 

    // skip over argv, envp to get to auxv
    while (argv_index(argv, n) != null) {>>MARKER:FUNCTION_osyield1_BLOCK_PREFIX<<
        n++;
    } 

    // skip NULL separator
    n++; 

    // now argv+n is auxv
    ptr<array<System.UIntPtr>> auxv = new ptr<ptr<array<System.UIntPtr>>>(add(@unsafe.Pointer(argv), uintptr(n) * sys.PtrSize));
    sysauxv(auxv[..]);

}

private static readonly nint _AT_NULL = 0; // Terminates the vector
private static readonly nint _AT_PAGESZ = 6; // Page size in bytes
private static readonly nint _AT_SUN_EXECNAME = 2014; // exec() path name

private static void sysauxv(slice<System.UIntPtr> auxv) {
    {
        nint i = 0;

        while (auxv[i] != _AT_NULL) {
            var tag = auxv[i];
            var val = auxv[i + 1];

            if (tag == _AT_PAGESZ) 
                physPageSize = val;
            else if (tag == _AT_SUN_EXECNAME) 
                executablePath = gostringnocopy((byte.val)(@unsafe.Pointer(val)));
                        i += 2;
        }
    }

}

} // end runtime_package
