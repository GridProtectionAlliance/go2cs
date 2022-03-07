// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:24 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_darwin.go
using abi = go.@internal.abi_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private partial struct mOS {
    public bool initialized;
    public pthreadmutex mutex;
    public pthreadcond cond;
    public nint count;
}

private static void unimplemented(@string name) {
    println(name, "not implemented") * (int.val)(@unsafe.Pointer(uintptr(1231)));

    1231;

}

//go:nosplit
private static void semacreate(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    if (mp.initialized) {
        return ;
    }
    mp.initialized = true;
    {
        var err__prev1 = err;

        var err = pthread_mutex_init(_addr_mp.mutex, null);

        if (err != 0) {
            throw("pthread_mutex_init");
        }
        err = err__prev1;

    }

    {
        var err__prev1 = err;

        err = pthread_cond_init(_addr_mp.cond, null);

        if (err != 0) {
            throw("pthread_cond_init");
        }
        err = err__prev1;

    }

}

//go:nosplit
private static int semasleep(long ns) {
    long start = default;
    if (ns >= 0) {
        start = nanotime();
    }
    var mp = getg().m;
    pthread_mutex_lock(_addr_mp.mutex);
    while (true) {
        if (mp.count > 0) {
            mp.count--;
            pthread_mutex_unlock(_addr_mp.mutex);
            return 0;
        }
        if (ns >= 0) {
            var spent = nanotime() - start;
            if (spent >= ns) {
                pthread_mutex_unlock(_addr_mp.mutex);
                return -1;
            }
            ref timespec t = ref heap(out ptr<timespec> _addr_t);
            t.setNsec(ns - spent);
            var err = pthread_cond_timedwait_relative_np(_addr_mp.cond, _addr_mp.mutex, _addr_t);
            if (err == _ETIMEDOUT) {
                pthread_mutex_unlock(_addr_mp.mutex);
                return -1;
            }
        }
        else
 {
            pthread_cond_wait(_addr_mp.cond, _addr_mp.mutex);
        }
    }

}

//go:nosplit
private static void semawakeup(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    pthread_mutex_lock(_addr_mp.mutex);
    mp.count++;
    if (mp.count > 0) {
        pthread_cond_signal(_addr_mp.cond);
    }
    pthread_mutex_unlock(_addr_mp.mutex);

}

// The read and write file descriptors used by the sigNote functions.
private static int sigNoteRead = default;private static int sigNoteWrite = default;

// sigNoteSetup initializes an async-signal-safe note.
//
// The current implementation of notes on Darwin is not async-signal-safe,
// because the functions pthread_mutex_lock, pthread_cond_signal, and
// pthread_mutex_unlock, called by semawakeup, are not async-signal-safe.
// There is only one case where we need to wake up a note from a signal
// handler: the sigsend function. The signal handler code does not require
// all the features of notes: it does not need to do a timed wait.
// This is a separate implementation of notes, based on a pipe, that does
// not support timed waits but is async-signal-safe.


// sigNoteSetup initializes an async-signal-safe note.
//
// The current implementation of notes on Darwin is not async-signal-safe,
// because the functions pthread_mutex_lock, pthread_cond_signal, and
// pthread_mutex_unlock, called by semawakeup, are not async-signal-safe.
// There is only one case where we need to wake up a note from a signal
// handler: the sigsend function. The signal handler code does not require
// all the features of notes: it does not need to do a timed wait.
// This is a separate implementation of notes, based on a pipe, that does
// not support timed waits but is async-signal-safe.
private static void sigNoteSetup(ptr<note> _addr__p0) {
    ref note _p0 = ref _addr__p0.val;

    if (sigNoteRead != 0 || sigNoteWrite != 0) {
        throw("duplicate sigNoteSetup");
    }
    int errno = default;
    sigNoteRead, sigNoteWrite, errno = pipe();
    if (errno != 0) {
        throw("pipe failed");
    }
    closeonexec(sigNoteRead);
    closeonexec(sigNoteWrite); 

    // Make the write end of the pipe non-blocking, so that if the pipe
    // buffer is somehow full we will not block in the signal handler.
    // Leave the read end of the pipe blocking so that we will block
    // in sigNoteSleep.
    setNonblock(sigNoteWrite);

}

// sigNoteWakeup wakes up a thread sleeping on a note created by sigNoteSetup.
private static void sigNoteWakeup(ptr<note> _addr__p0) {
    ref note _p0 = ref _addr__p0.val;

    ref byte b = ref heap(out ptr<byte> _addr_b);
    write(uintptr(sigNoteWrite), @unsafe.Pointer(_addr_b), 1);
}

// sigNoteSleep waits for a note created by sigNoteSetup to be woken.
private static void sigNoteSleep(ptr<note> _addr__p0) {
    ref note _p0 = ref _addr__p0.val;

    while (true) {
        ref byte b = ref heap(out ptr<byte> _addr_b);
        entersyscallblock();
        var n = read(sigNoteRead, @unsafe.Pointer(_addr_b), 1);
        exitsyscall();
        if (n != -_EINTR) {
            return ;
        }
    }

}

// BSD interface for threading.
private static void osinit() { 
    // pthread_create delayed until end of goenvs so that we
    // can look at the environment first.

    ncpu = getncpu();
    physPageSize = getPageSize();

}

private static (int, int) sysctlbynameInt32(slice<byte> name) {
    int _p0 = default;
    int _p0 = default;

    ref var @out = ref heap(int32(0), out ptr<var> _addr_@out);
    ref var nout = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_nout);
    var ret = sysctlbyname(_addr_name[0], (byte.val)(@unsafe.Pointer(_addr_out)), _addr_nout, null, 0);
    return (ret, out);
}

//go:linkname internal_cpu_getsysctlbyname internal/cpu.getsysctlbyname
private static (int, int) internal_cpu_getsysctlbyname(slice<byte> name) {
    int _p0 = default;
    int _p0 = default;

    return sysctlbynameInt32(name);
}

private static readonly nint _CTL_HW = 6;
private static readonly nint _HW_NCPU = 3;
private static readonly nint _HW_PAGESIZE = 7;


private static int getncpu() { 
    // Use sysctl to fetch hw.ncpu.
    array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_NCPU });
    ref var @out = ref heap(uint32(0), out ptr<var> _addr_@out);
    ref var nout = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_nout);
    var ret = sysctl(_addr_mib[0], 2, (byte.val)(@unsafe.Pointer(_addr_out)), _addr_nout, null, 0);
    if (ret >= 0 && int32(out) > 0) {
        return int32(out);
    }
    return 1;

}

private static System.UIntPtr getPageSize() { 
    // Use sysctl to fetch hw.pagesize.
    array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_PAGESIZE });
    ref var @out = ref heap(uint32(0), out ptr<var> _addr_@out);
    ref var nout = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_nout);
    var ret = sysctl(_addr_mib[0], 2, (byte.val)(@unsafe.Pointer(_addr_out)), _addr_nout, null, 0);
    if (ret >= 0 && int32(out) > 0) {
        return uintptr(out);
    }
    return 0;

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

// May run with m.p==nil, so write barriers are not allowed.
//go:nowritebarrierrec
private static void newosproc(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    var stk = @unsafe.Pointer(mp.g0.stack.hi);
    if (false) {
        print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " id=", mp.id, " ostk=", _addr_mp, "\n");
    }
    ref pthreadattr attr = ref heap(out ptr<pthreadattr> _addr_attr);
    int err = default;
    err = pthread_attr_init(_addr_attr);
    if (err != 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
    ref System.UIntPtr stacksize = ref heap(out ptr<System.UIntPtr> _addr_stacksize);
    if (pthread_attr_getstacksize(_addr_attr, _addr_stacksize) != 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
    mp.g0.stack.hi = stacksize; // for mstart

    // Tell the pthread library we won't join with this thread.
    if (pthread_attr_setdetachstate(_addr_attr, _PTHREAD_CREATE_DETACHED) != 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
    ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
    sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
    err = pthread_create(_addr_attr, abi.FuncPCABI0(mstart_stub), @unsafe.Pointer(mp));
    sigprocmask(_SIG_SETMASK, _addr_oset, null);
    if (err != 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
}

// glue code to call mstart from pthread_create.
private static void mstart_stub();

// newosproc0 is a version of newosproc that can be called before the runtime
// is initialized.
//
// This function is not safe to use after initialization as it does not pass an M as fnarg.
//
//go:nosplit
private static void newosproc0(System.UIntPtr stacksize, System.UIntPtr fn) { 
    // Initialize an attribute object.
    ref pthreadattr attr = ref heap(out ptr<pthreadattr> _addr_attr);
    int err = default;
    err = pthread_attr_init(_addr_attr);
    if (err != 0) {>>MARKER:FUNCTION_mstart_stub_BLOCK_PREFIX<<
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
    if (pthread_attr_getstacksize(_addr_attr, _addr_stacksize) != 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
    g0.stack.hi = stacksize; // for mstart
    memstats.stacks_sys.add(int64(stacksize)); 

    // Tell the pthread library we won't join with this thread.
    if (pthread_attr_setdetachstate(_addr_attr, _PTHREAD_CREATE_DETACHED) != 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
    ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
    sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
    err = pthread_create(_addr_attr, fn, null);
    sigprocmask(_SIG_SETMASK, _addr_oset, null);
    if (err != 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
}

private static slice<byte> failallocatestack = (slice<byte>)"runtime: failed to allocate stack for the new OS thread\n";
private static slice<byte> failthreadcreate = (slice<byte>)"runtime: failed to create new OS thread\n";

// Called to do synchronous initialization of Go code built with
// -buildmode=c-archive or -buildmode=c-shared.
// None of the Go runtime is initialized.
//go:nosplit
//go:nowritebarrierrec
private static void libpreinit() {
    initsig(true);
}

// Called to initialize a new m (including the bootstrap m).
// Called on the parent thread (main thread in case of bootstrap), can allocate memory.
private static void mpreinit(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    mp.gsignal = malg(32 * 1024); // OS X wants >= 8K
    mp.gsignal.m = mp;
    if (GOOS == "darwin" && GOARCH == "arm64") { 
        // mlock the signal stack to work around a kernel bug where it may
        // SIGILL when the signal stack is not faulted in while a signal
        // arrives. See issue 42774.
        mlock(@unsafe.Pointer(mp.gsignal.stack.hi - physPageSize), physPageSize);

    }
}

// Called to initialize a new m (including the bootstrap m).
// Called on the new thread, cannot allocate memory.
private static void minit() { 
    // iOS does not support alternate signal stack.
    // The signal handler handles it directly.
    if (!(GOOS == "ios" && GOARCH == "arm64")) {
        minitSignalStack();
    }
    minitSignalMask();
    getg().m.procid = uint64(pthread_self());

}

// Called from dropm to undo the effect of an minit.
//go:nosplit
private static void unminit() { 
    // iOS does not support alternate signal stack.
    // See minit.
    if (!(GOOS == "ios" && GOARCH == "arm64")) {
        unminitSignals();
    }
}

// Called from exitm, but not from drop, to undo the effect of thread-owned
// resources in minit, semacreate, or elsewhere. Do not take locks after calling this.
private static void mdestroy(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

}

//go:nosplit
private static void osyield_no_g() {
    usleep_no_g(1);
}

//go:nosplit
private static void osyield() {
    usleep(1);
}

private static readonly nint _NSIG = 32;
private static readonly nint _SI_USER = 0; /* empirically true, but not what headers say */
private static readonly nint _SIG_BLOCK = 1;
private static readonly nint _SIG_UNBLOCK = 2;
private static readonly nint _SIG_SETMASK = 3;
private static readonly nint _SS_DISABLE = 4;


//extern SigTabTT runtime·sigtab[];

private partial struct sigset { // : uint
}

private static var sigset_all = ~sigset(0);

//go:nosplit
//go:nowritebarrierrec
private static void setsig(uint i, System.UIntPtr fn) {
    ref usigactiont sa = ref heap(out ptr<usigactiont> _addr_sa);
    sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
    sa.sa_mask = ~uint32(0);
    if (fn == funcPC(sighandler)) { // funcPC(sighandler) matches the callers in signal_unix.go
        if (iscgo) {
            fn = abi.FuncPCABI0(cgoSigtramp);
        }
        else
 {
            fn = abi.FuncPCABI0(sigtramp);
        }
    }
    (uintptr.val)(@unsafe.Pointer(_addr_sa.__sigaction_u)).val;

    fn;
    sigaction(i, _addr_sa, null);

}

// sigtramp is the callback from libc when a signal is received.
// It is called with the C calling convention.
private static void sigtramp();
private static void cgoSigtramp();

//go:nosplit
//go:nowritebarrierrec
private static void setsigstack(uint i) {
    ref usigactiont osa = ref heap(out ptr<usigactiont> _addr_osa);
    sigaction(i, null, _addr_osa);
    ptr<ptr<System.UIntPtr>> handler = new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(_addr_osa.__sigaction_u));
    if (osa.sa_flags & _SA_ONSTACK != 0) {>>MARKER:FUNCTION_cgoSigtramp_BLOCK_PREFIX<<
        return ;
    }
    ref usigactiont sa = ref heap(out ptr<usigactiont> _addr_sa);
    (uintptr.val)(@unsafe.Pointer(_addr_sa.__sigaction_u)).val;

    handler;
    sa.sa_mask = osa.sa_mask;
    sa.sa_flags = osa.sa_flags | _SA_ONSTACK;
    sigaction(i, _addr_sa, null);

}

//go:nosplit
//go:nowritebarrierrec
private static System.UIntPtr getsig(uint i) {
    ref usigactiont sa = ref heap(out ptr<usigactiont> _addr_sa);
    sigaction(i, null, _addr_sa);
    return new ptr<ptr<ptr<System.UIntPtr>>>(@unsafe.Pointer(_addr_sa.__sigaction_u));
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

    mask |= 1 << (int)((uint32(i) - 1));
}

private static void sigdelset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    mask &= 1 << (int)((uint32(i) - 1));
}

//go:linkname executablePath os.executablePath
private static @string executablePath = default;

private static void sysargs(int argc, ptr<ptr<byte>> _addr_argv) {
    ref ptr<byte> argv = ref _addr_argv.val;
 
    // skip over argv, envv and the first string will be the path
    var n = argc + 1;
    while (argv_index(argv, n) != null) {>>MARKER:FUNCTION_sigtramp_BLOCK_PREFIX<<
        n++;
    }
    executablePath = gostringnocopy(argv_index(argv, n + 1)); 

    // strip "executable_path=" prefix if available, it's added after OS X 10.11.
    const @string prefix = "executable_path=";

    if (len(executablePath) > len(prefix) && executablePath[..(int)len(prefix)] == prefix) {
        executablePath = executablePath[(int)len(prefix)..];
    }
}

private static void signalM(ptr<m> _addr_mp, nint sig) {
    ref m mp = ref _addr_mp.val;

    pthread_kill(pthread(mp.procid), uint32(sig));
}

} // end runtime_package
