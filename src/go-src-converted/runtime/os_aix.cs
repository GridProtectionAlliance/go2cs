// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix
// +build aix

// package runtime -- go2cs converted at 2022 March 06 22:10:23 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_aix.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static readonly nuint threadStackSize = 0x100000; // size of a thread stack allocated by OS

// funcDescriptor is a structure representing a function descriptor
// A variable with this type is always created in assembler
private partial struct funcDescriptor {
    public System.UIntPtr fn;
    public System.UIntPtr toc;
    public System.UIntPtr envPointer; // unused in Golang
}

private partial struct mOS {
    public System.UIntPtr waitsema; // semaphore for parking on locks
    public System.UIntPtr perrno; // pointer to tls errno
}

//go:nosplit
private static void semacreate(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (mp.waitsema != 0) {
        return ;
    }
    ptr<semt> sem; 

    // Call libc's malloc rather than malloc. This will
    // allocate space on the C heap. We can't call mallocgc
    // here because it could cause a deadlock.
    sem = (semt.val)(malloc(@unsafe.Sizeof(sem.val)));
    if (sem_init(sem, 0, 0) != 0) {
        throw("sem_init");
    }
    mp.waitsema = uintptr(@unsafe.Pointer(sem));

}

//go:nosplit
private static int semasleep(long ns) {
    var _m_ = getg().m;
    if (ns >= 0) {
        ref timespec ts = ref heap(out ptr<timespec> _addr_ts);

        if (clock_gettime(_CLOCK_REALTIME, _addr_ts) != 0) {
            throw("clock_gettime");
        }
        ts.tv_sec += ns / 1e9F;
        ts.tv_nsec += ns % 1e9F;
        if (ts.tv_nsec >= 1e9F) {
            ts.tv_sec++;
            ts.tv_nsec -= 1e9F;
        }
        {
            var (r, err) = sem_timedwait((semt.val)(@unsafe.Pointer(_m_.waitsema)), _addr_ts);

            if (r != 0) {
                if (err == _ETIMEDOUT || err == _EAGAIN || err == _EINTR) {
                    return -1;
                }
                println("sem_timedwait err ", err, " ts.tv_sec ", ts.tv_sec, " ts.tv_nsec ", ts.tv_nsec, " ns ", ns, " id ", _m_.id);
                throw("sem_timedwait");
            }

        }

        return 0;

    }
    while (true) {
        var (r1, err) = sem_wait((semt.val)(@unsafe.Pointer(_m_.waitsema)));
        if (r1 == 0) {
            break;
        }
        if (err == _EINTR) {
            continue;
        }
        throw("sem_wait");

    }
    return 0;

}

//go:nosplit
private static void semawakeup(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (sem_post((semt.val)(@unsafe.Pointer(mp.waitsema))) != 0) {
        throw("sem_post");
    }
}

private static void osinit() {
    ncpu = int32(sysconf(__SC_NPROCESSORS_ONLN));
    physPageSize = sysconf(__SC_PAGE_SIZE);
}

// newosproc0 is a version of newosproc that can be called before the runtime
// is initialized.
//
// This function is not safe to use after initialization as it does not pass an M as fnarg.
//
//go:nosplit
private static void newosproc0(System.UIntPtr stacksize, ptr<funcDescriptor> _addr_fn) {
    ref funcDescriptor fn = ref _addr_fn.val;

    ref pthread_attr attr = ref heap(out ptr<pthread_attr> _addr_attr);    ref sigset oset = ref heap(out ptr<sigset> _addr_oset);    ref pthread tid = ref heap(out ptr<pthread> _addr_tid);

    if (pthread_attr_init(_addr_attr) != 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
    if (pthread_attr_setstacksize(_addr_attr, threadStackSize) != 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
    if (pthread_attr_setdetachstate(_addr_attr, _PTHREAD_CREATE_DETACHED) != 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
    sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
    int ret = default;
    for (nint tries = 0; tries < 20; tries++) { 
        // pthread_create can fail with EAGAIN for no reasons
        // but it will be ok if it retries.
        ret = pthread_create(_addr_tid, _addr_attr, fn, null);
        if (ret != _EAGAIN) {
            break;
        }
        usleep(uint32(tries + 1) * 1000); // Milliseconds.
    }
    sigprocmask(_SIG_SETMASK, _addr_oset, null);
    if (ret != 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
}

private static slice<byte> failthreadcreate = (slice<byte>)"runtime: failed to create new OS thread\n";

// Called to do synchronous initialization of Go code built with
// -buildmode=c-archive or -buildmode=c-shared.
// None of the Go runtime is initialized.
//go:nosplit
//go:nowritebarrierrec
private static void libpreinit() {
    initsig(true);
}

// Ms related functions
private static void mpreinit(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    mp.gsignal = malg(32 * 1024); // AIX wants >= 8K
    mp.gsignal.m = mp;

}

// errno address must be retrieved by calling _Errno libc function.
// This will return a pointer to errno
private static void miniterrno() {
    var mp = getg().m;
    var (r, _) = syscall0(_addr_libc__Errno);
    mp.perrno = r;
}

private static void minit() {
    miniterrno();
    minitSignals();
    getg().m.procid = uint64(pthread_self());
}

private static void unminit() {
    unminitSignals();
}

// Called from exitm, but not from drop, to undo the effect of thread-owned
// resources in minit, semacreate, or elsewhere. Do not take locks after calling this.
private static void mdestroy(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

}

// tstart is a function descriptor to _tstart defined in assembly.
private static funcDescriptor tstart = default;

private static void newosproc(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    ref pthread_attr attr = ref heap(out ptr<pthread_attr> _addr_attr);    ref sigset oset = ref heap(out ptr<sigset> _addr_oset);    ref pthread tid = ref heap(out ptr<pthread> _addr_tid);

    if (pthread_attr_init(_addr_attr) != 0) {
        throw("pthread_attr_init");
    }
    if (pthread_attr_setstacksize(_addr_attr, threadStackSize) != 0) {
        throw("pthread_attr_getstacksize");
    }
    if (pthread_attr_setdetachstate(_addr_attr, _PTHREAD_CREATE_DETACHED) != 0) {
        throw("pthread_attr_setdetachstate");
    }
    sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
    int ret = default;
    for (nint tries = 0; tries < 20; tries++) { 
        // pthread_create can fail with EAGAIN for no reasons
        // but it will be ok if it retries.
        ret = pthread_create(_addr_tid, _addr_attr, _addr_tstart, @unsafe.Pointer(mp));
        if (ret != _EAGAIN) {
            break;
        }
        usleep(uint32(tries + 1) * 1000); // Milliseconds.
    }
    sigprocmask(_SIG_SETMASK, _addr_oset, null);
    if (ret != 0) {
        print("runtime: failed to create new OS thread (have ", mcount(), " already; errno=", ret, ")\n");
        if (ret == _EAGAIN) {
            println("runtime: may need to increase max user processes (ulimit -u)");
        }
        throw("newosproc");

    }
}

private static void exitThread(ptr<uint> _addr_wait) {
    ref uint wait = ref _addr_wait.val;
 
    // We should never reach exitThread on AIX because we let
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

/* SIGNAL */

private static readonly nint _NSIG = 256;


// sigtramp is a function descriptor to _sigtramp defined in assembly
private static funcDescriptor sigtramp = default;

//go:nosplit
//go:nowritebarrierrec
private static void setsig(uint i, System.UIntPtr fn) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
    sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
    sa.sa_mask = sigset_all;
    if (fn == funcPC(sighandler)) {
        fn = uintptr(@unsafe.Pointer(_addr_sigtramp));
    }
    sa.sa_handler = fn;
    sigaction(uintptr(i), _addr_sa, null);


}

//go:nosplit
//go:nowritebarrierrec
private static void setsigstack(uint i) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
    sigaction(uintptr(i), null, _addr_sa);
    if (sa.sa_flags & _SA_ONSTACK != 0) {
        return ;
    }
    sa.sa_flags |= _SA_ONSTACK;
    sigaction(uintptr(i), _addr_sa, null);

}

//go:nosplit
//go:nowritebarrierrec
private static System.UIntPtr getsig(uint i) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
    sigaction(uintptr(i), null, _addr_sa);
    return sa.sa_handler;
}

// setSignaltstackSP sets the ss_sp field of a stackt.
//go:nosplit
private static void setSignalstackSP(ptr<stackt> _addr_s, System.UIntPtr sp) {
    ref stackt s = ref _addr_s.val;

    (uintptr.val)(@unsafe.Pointer(_addr_s.ss_sp)).val;

    sp;

}

//go:nosplit
private static void fixsigcode(this ptr<sigctxt> _addr_c, uint sig) {
    ref sigctxt c = ref _addr_c.val;


    if (sig == _SIGPIPE) 
        // For SIGPIPE, c.sigcode() isn't set to _SI_USER as on Linux.
        // Therefore, raisebadsignal won't raise SIGPIPE again if
        // it was deliver in a non-Go thread.
        c.set_sigcode(_SI_USER);
    
}

//go:nosplit
//go:nowritebarrierrec
private static void sigaddset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    (mask)[(i - 1) / 64] |= 1 << (int)(((uint32(i) - 1) & 63));
}

private static void sigdelset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    (mask)[(i - 1) / 64] &= 1 << (int)(((uint32(i) - 1) & 63));
}

private static readonly nint _CLOCK_REALTIME = 9;
private static readonly nint _CLOCK_MONOTONIC = 10;


//go:nosplit
private static long nanotime1() {
    ptr<timespec> tp = addr(new timespec());
    if (clock_gettime(_CLOCK_REALTIME, tp) != 0) {
        throw("syscall clock_gettime failed");
    }
    return tp.tv_sec * 1000000000 + tp.tv_nsec;

}

private static (long, int) walltime() {
    long sec = default;
    int nsec = default;

    ptr<timespec> ts = addr(new timespec());
    if (clock_gettime(_CLOCK_REALTIME, ts) != 0) {
        throw("syscall clock_gettime failed");
    }
    return (ts.tv_sec, int32(ts.tv_nsec));

}

//go:nosplit
private static int fcntl(int fd, int cmd, int arg) {
    var (r, _) = syscall3(_addr_libc_fcntl, uintptr(fd), uintptr(cmd), uintptr(arg));
    return int32(r);
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

} // end runtime_package
