// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:29 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_linux.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

private partial struct mOS {
}

//go:noescape
private static int futex(unsafe.Pointer addr, int op, uint val, unsafe.Pointer ts, unsafe.Pointer addr2, uint val3);

// Linux futex.
//
//    futexsleep(uint32 *addr, uint32 val)
//    futexwakeup(uint32 *addr)
//
// Futexsleep atomically checks if *addr == val and if so, sleeps on addr.
// Futexwakeup wakes up threads sleeping on addr.
// Futexsleep is allowed to wake up spuriously.

private static readonly nint _FUTEX_PRIVATE_FLAG = 128;
private static readonly nint _FUTEX_WAIT_PRIVATE = 0 | _FUTEX_PRIVATE_FLAG;
private static readonly nint _FUTEX_WAKE_PRIVATE = 1 | _FUTEX_PRIVATE_FLAG;


// Atomically,
//    if(*addr == val) sleep
// Might be woken up spuriously; that's allowed.
// Don't sleep longer than ns; ns < 0 means forever.
//go:nosplit
private static void futexsleep(ptr<uint> _addr_addr, uint val, long ns) {
    ref uint addr = ref _addr_addr.val;
 
    // Some Linux kernels have a bug where futex of
    // FUTEX_WAIT returns an internal error code
    // as an errno. Libpthread ignores the return value
    // here, and so can we: as it says a few lines up,
    // spurious wakeups are allowed.
    if (ns < 0) {>>MARKER:FUNCTION_futex_BLOCK_PREFIX<<
        futex(@unsafe.Pointer(addr), _FUTEX_WAIT_PRIVATE, val, null, null, 0);
        return ;
    }
    ref timespec ts = ref heap(out ptr<timespec> _addr_ts);
    ts.setNsec(ns);
    futex(@unsafe.Pointer(addr), _FUTEX_WAIT_PRIVATE, val, @unsafe.Pointer(_addr_ts), null, 0);

}

// If any procs are sleeping on addr, wake up at most cnt.
//go:nosplit
private static void futexwakeup(ptr<uint> _addr_addr, uint cnt) {
    ref uint addr = ref _addr_addr.val;

    var ret = futex(@unsafe.Pointer(addr), _FUTEX_WAKE_PRIVATE, cnt, null, null, 0);
    if (ret >= 0) {
        return ;
    }
    systemstack(() => {
        print("futexwakeup addr=", addr, " returned ", ret, "\n");
    }) * (int32.val)(@unsafe.Pointer(uintptr(0x1006)));

    0x1006;

}

private static int getproccount() { 
    // This buffer is huge (8 kB) but we are on the system stack
    // and there should be plenty of space (64 kB).
    // Also this is a leaf, so we're not holding up the memory for long.
    // See golang.org/issue/11823.
    // The suggested behavior here is to keep trying with ever-larger
    // buffers, but we don't have a dynamic memory allocator at the
    // moment, so that's a bit tricky and seems like overkill.
    const nint maxCPUs = 64 * 1024;

    array<byte> buf = new array<byte>(maxCPUs / 8);
    var r = sched_getaffinity(0, @unsafe.Sizeof(buf), _addr_buf[0]);
    if (r < 0) {
        return 1;
    }
    var n = int32(0);
    foreach (var (_, v) in buf[..(int)r]) {
        while (v != 0) {
            n += int32(v & 1);
            v>>=1;
        }
    }    if (n == 0) {
        n = 1;
    }
    return n;

}

// Clone, the Linux rfork.
private static readonly nuint _CLONE_VM = 0x100;
private static readonly nuint _CLONE_FS = 0x200;
private static readonly nuint _CLONE_FILES = 0x400;
private static readonly nuint _CLONE_SIGHAND = 0x800;
private static readonly nuint _CLONE_PTRACE = 0x2000;
private static readonly nuint _CLONE_VFORK = 0x4000;
private static readonly nuint _CLONE_PARENT = 0x8000;
private static readonly nuint _CLONE_THREAD = 0x10000;
private static readonly nuint _CLONE_NEWNS = 0x20000;
private static readonly nuint _CLONE_SYSVSEM = 0x40000;
private static readonly nuint _CLONE_SETTLS = 0x80000;
private static readonly nuint _CLONE_PARENT_SETTID = 0x100000;
private static readonly nuint _CLONE_CHILD_CLEARTID = 0x200000;
private static readonly nuint _CLONE_UNTRACED = 0x800000;
private static readonly nuint _CLONE_CHILD_SETTID = 0x1000000;
private static readonly nuint _CLONE_STOPPED = 0x2000000;
private static readonly nuint _CLONE_NEWUTS = 0x4000000;
private static readonly nuint _CLONE_NEWIPC = 0x8000000; 

// As of QEMU 2.8.0 (5ea2fc84d), user emulation requires all six of these
// flags to be set when creating a thread; attempts to share the other
// five but leave SYSVSEM unshared will fail with -EINVAL.
//
// In non-QEMU environments CLONE_SYSVSEM is inconsequential as we do not
// use System V semaphores.

private static readonly var cloneFlags = _CLONE_VM | _CLONE_FS | _CLONE_FILES | _CLONE_SIGHAND | _CLONE_SYSVSEM | _CLONE_THREAD; /* revisit - okay for now */

//go:noescape
private static int clone(int flags, unsafe.Pointer stk, unsafe.Pointer mp, unsafe.Pointer gp, unsafe.Pointer fn);

// May run with m.p==nil, so write barriers are not allowed.
//go:nowritebarrier
private static void newosproc(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    var stk = @unsafe.Pointer(mp.g0.stack.hi);
    /*
         * note: strace gets confused if we use CLONE_PTRACE here.
         */
    if (false) {>>MARKER:FUNCTION_clone_BLOCK_PREFIX<<
        print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " clone=", funcPC(clone), " id=", mp.id, " ostk=", _addr_mp, "\n");
    }
    ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
    sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
    var ret = clone(cloneFlags, stk, @unsafe.Pointer(mp), @unsafe.Pointer(mp.g0), @unsafe.Pointer(funcPC(mstart)));
    sigprocmask(_SIG_SETMASK, _addr_oset, _addr_null);

    if (ret < 0) {
        print("runtime: failed to create new OS thread (have ", mcount(), " already; errno=", -ret, ")\n");
        if (ret == -_EAGAIN) {
            println("runtime: may need to increase max user processes (ulimit -u)");
        }
        throw("newosproc");

    }
}

// Version of newosproc that doesn't require a valid G.
//go:nosplit
private static void newosproc0(System.UIntPtr stacksize, unsafe.Pointer fn) {
    var stack = sysAlloc(stacksize, _addr_memstats.stacks_sys);
    if (stack == null) {
        write(2, @unsafe.Pointer(_addr_failallocatestack[0]), int32(len(failallocatestack)));
        exit(1);
    }
    var ret = clone(cloneFlags, @unsafe.Pointer(uintptr(stack) + stacksize), null, null, fn);
    if (ret < 0) {
        write(2, @unsafe.Pointer(_addr_failthreadcreate[0]), int32(len(failthreadcreate)));
        exit(1);
    }
}

private static slice<byte> failallocatestack = (slice<byte>)"runtime: failed to allocate stack for the new OS thread\n";
private static slice<byte> failthreadcreate = (slice<byte>)"runtime: failed to create new OS thread\n";

private static readonly nint _AT_NULL = 0; // End of vector
private static readonly nint _AT_PAGESZ = 6; // System physical page size
private static readonly nint _AT_HWCAP = 16; // hardware capability bit vector
private static readonly nint _AT_RANDOM = 25; // introduced in 2.6.29
private static readonly nint _AT_HWCAP2 = 26; // hardware capability bit vector 2

private static slice<byte> procAuxv = (slice<byte>)"/proc/self/auxv\x00";

private static array<byte> addrspace_vec = new array<byte>(1);

private static int mincore(unsafe.Pointer addr, System.UIntPtr n, ptr<byte> dst);

private static void sysargs(int argc, ptr<ptr<byte>> _addr_argv) {
    ref ptr<byte> argv = ref _addr_argv.val;

    var n = argc + 1; 

    // skip over argv, envp to get to auxv
    while (argv_index(argv, n) != null) {>>MARKER:FUNCTION_mincore_BLOCK_PREFIX<<
        n++;
    } 

    // skip NULL separator
    n++; 

    // now argv+n is auxv
    ptr<array<System.UIntPtr>> auxv = new ptr<ptr<array<System.UIntPtr>>>(add(@unsafe.Pointer(argv), uintptr(n) * sys.PtrSize));
    if (sysauxv(auxv[..]) != 0) {
        return ;
    }
    var fd = open(_addr_procAuxv[0], 0, 0);
    if (fd < 0) { 
        // On Android, /proc/self/auxv might be unreadable (issue 9229), so we fallback to
        // try using mincore to detect the physical page size.
        // mincore should return EINVAL when address is not a multiple of system page size.
        const nint size = 256 << 10; // size of memory region to allocate
 // size of memory region to allocate
        var (p, err) = mmap(null, size, _PROT_READ | _PROT_WRITE, _MAP_ANON | _MAP_PRIVATE, -1, 0);
        if (err != 0) {
            return ;
        }
        n = default;
        n = 4 << 10;

        while (n < size) {
            var err = mincore(@unsafe.Pointer(uintptr(p) + n), 1, _addr_addrspace_vec[0]);
            if (err == 0) {
                physPageSize = n;
                break;
            n<<=1;
            }

        }
        if (physPageSize == 0) {
            physPageSize = size;
        }
        munmap(p, size);
        return ;

    }
    array<System.UIntPtr> buf = new array<System.UIntPtr>(128);
    n = read(fd, noescape(@unsafe.Pointer(_addr_buf[0])), int32(@unsafe.Sizeof(buf)));
    closefd(fd);
    if (n < 0) {
        return ;
    }
    buf[len(buf) - 2] = _AT_NULL;
    sysauxv(buf[..]);

}

// startupRandomData holds random bytes initialized at startup. These come from
// the ELF AT_RANDOM auxiliary vector.
private static slice<byte> startupRandomData = default;

private static nint sysauxv(slice<System.UIntPtr> auxv) {
    nint i = default;
    while (auxv[i] != _AT_NULL) {
        var tag = auxv[i];
        var val = auxv[i + 1];

        if (tag == _AT_RANDOM) 
            // The kernel provides a pointer to 16-bytes
            // worth of random data.
            startupRandomData = new ptr<ptr<array<byte>>>(@unsafe.Pointer(val))[..];
        else if (tag == _AT_PAGESZ) 
            physPageSize = val;
                archauxv(tag, val);
        vdsoauxv(tag, val);
        i += 2;
    }
    return i / 2;

}

private static slice<byte> sysTHPSizePath = (slice<byte>)"/sys/kernel/mm/transparent_hugepage/hpage_pmd_size\x00";

private static System.UIntPtr getHugePageSize() {
    array<byte> numbuf = new array<byte>(20);
    var fd = open(_addr_sysTHPSizePath[0], 0, 0);
    if (fd < 0) {
        return 0;
    }
    var ptr = noescape(@unsafe.Pointer(_addr_numbuf[0]));
    var n = read(fd, ptr, int32(len(numbuf)));
    closefd(fd);
    if (n <= 0) {
        return 0;
    }
    n--; // remove trailing newline
    var (v, ok) = atoi(slicebytetostringtmp((byte.val)(ptr), int(n)));
    if (!ok || v < 0) {
        v = 0;
    }
    if (v & (v - 1) != 0) { 
        // v is not a power of 2
        return 0;

    }
    return uintptr(v);

}

private static void osinit() {
    ncpu = getproccount();
    physHugePageSize = getHugePageSize();
    if (iscgo) { 
        // #42494 glibc and musl reserve some signals for
        // internal use and require they not be blocked by
        // the rest of a normal C runtime. When the go runtime
        // blocks...unblocks signals, temporarily, the blocked
        // interval of time is generally very short. As such,
        // these expectations of *libc code are mostly met by
        // the combined go+cgo system of threads. However,
        // when go causes a thread to exit, via a return from
        // mstart(), the combined runtime can deadlock if
        // these signals are blocked. Thus, don't block these
        // signals when exiting threads.
        // - glibc: SIGCANCEL (32), SIGSETXID (33)
        // - musl: SIGTIMER (32), SIGCANCEL (33), SIGSYNCCALL (34)
        sigdelset(_addr_sigsetAllExiting, 32);
        sigdelset(_addr_sigsetAllExiting, 33);
        sigdelset(_addr_sigsetAllExiting, 34);

    }
    osArchInit();

}

private static slice<byte> urandom_dev = (slice<byte>)"/dev/urandom\x00";

private static void getRandomData(slice<byte> r) {
    if (startupRandomData != null) {
        var n = copy(r, startupRandomData);
        extendRandom(r, n);
        return ;
    }
    var fd = open(_addr_urandom_dev[0], 0, 0);
    n = read(fd, @unsafe.Pointer(_addr_r[0]), int32(len(r)));
    closefd(fd);
    extendRandom(r, int(n));

}

private static void goenvs() {
    goenvs_unix();
}

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

    mp.gsignal = malg(32 * 1024); // Linux wants >= 2K
    mp.gsignal.m = mp;

}

private static uint gettid();

// Called to initialize a new m (including the bootstrap m).
// Called on the new thread, cannot allocate memory.
private static void minit() {
    minitSignals(); 

    // Cgo-created threads and the bootstrap m are missing a
    // procid. We need this for asynchronous preemption and it's
    // useful in debuggers.
    getg().m.procid = uint64(gettid());

}

// Called from dropm to undo the effect of an minit.
//go:nosplit
private static void unminit() {
    unminitSignals();
}

// Called from exitm, but not from drop, to undo the effect of thread-owned
// resources in minit, semacreate, or elsewhere. Do not take locks after calling this.
private static void mdestroy(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

}

//#ifdef GOARCH_386
//#define sa_handler k_sa_handler
//#endif

private static void sigreturn();
private static void sigtramp(); // Called via C ABI
private static void cgoSigtramp();

//go:noescape
private static void sigaltstack(ptr<stackt> @new, ptr<stackt> old);

//go:noescape
private static void setitimer(int mode, ptr<itimerval> @new, ptr<itimerval> old);

//go:noescape
private static void rtsigprocmask(int how, ptr<sigset> @new, ptr<sigset> old, int size);

//go:nosplit
//go:nowritebarrierrec
private static void sigprocmask(int how, ptr<sigset> _addr_@new, ptr<sigset> _addr_old) {
    ref sigset @new = ref _addr_@new.val;
    ref sigset old = ref _addr_old.val;

    rtsigprocmask(how, _addr_new, _addr_old, int32(@unsafe.Sizeof(new.val)));
}

private static void raise(uint sig);
private static void raiseproc(uint sig);

//go:noescape
private static int sched_getaffinity(System.UIntPtr pid, System.UIntPtr len, ptr<byte> buf);
private static void osyield();

//go:nosplit
private static void osyield_no_g() {
    osyield();
}

private static (int, int, int) pipe();
private static (int, int, int) pipe2(int flags);
private static void setNonblock(int fd);

//go:nosplit
//go:nowritebarrierrec
private static void setsig(uint i, System.UIntPtr fn) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
    sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTORER | _SA_RESTART;
    sigfillset(_addr_sa.sa_mask); 
    // Although Linux manpage says "sa_restorer element is obsolete and
    // should not be used". x86_64 kernel requires it. Only use it on
    // x86.
    if (GOARCH == "386" || GOARCH == "amd64") {>>MARKER:FUNCTION_setNonblock_BLOCK_PREFIX<<
        sa.sa_restorer = funcPC(sigreturn);
    }
    if (fn == funcPC(sighandler)) {>>MARKER:FUNCTION_pipe2_BLOCK_PREFIX<<
        if (iscgo) {>>MARKER:FUNCTION_pipe_BLOCK_PREFIX<<
            fn = funcPC(cgoSigtramp);
        }
        else
 {>>MARKER:FUNCTION_osyield_BLOCK_PREFIX<<
            fn = funcPC(sigtramp);
        }
    }
    sa.sa_handler = fn;
    sigaction(i, _addr_sa, null);

}

//go:nosplit
//go:nowritebarrierrec
private static void setsigstack(uint i) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
    sigaction(i, null, _addr_sa);
    if (sa.sa_flags & _SA_ONSTACK != 0) {>>MARKER:FUNCTION_sched_getaffinity_BLOCK_PREFIX<<
        return ;
    }
    sa.sa_flags |= _SA_ONSTACK;
    sigaction(i, _addr_sa, null);

}

//go:nosplit
//go:nowritebarrierrec
private static System.UIntPtr getsig(uint i) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
    sigaction(i, null, _addr_sa);
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

}

// sysSigaction calls the rt_sigaction system call.
//go:nosplit
private static void sysSigaction(uint sig, ptr<sigactiont> _addr_@new, ptr<sigactiont> _addr_old) {
    ref sigactiont @new = ref _addr_@new.val;
    ref sigactiont old = ref _addr_old.val;

    if (rt_sigaction(uintptr(sig), _addr_new, _addr_old, @unsafe.Sizeof(new sigactiont().sa_mask)) != 0) {>>MARKER:FUNCTION_raiseproc_BLOCK_PREFIX<< 
        // Workaround for bugs in QEMU user mode emulation.
        //
        // QEMU turns calls to the sigaction system call into
        // calls to the C library sigaction call; the C
        // library call rejects attempts to call sigaction for
        // SIGCANCEL (32) or SIGSETXID (33).
        //
        // QEMU rejects calling sigaction on SIGRTMAX (64).
        //
        // Just ignore the error in these case. There isn't
        // anything we can do about it anyhow.
        if (sig != 32 && sig != 33 && sig != 64) {>>MARKER:FUNCTION_raise_BLOCK_PREFIX<< 
            // Use system stack to avoid split stack overflow on ppc64/ppc64le.
            systemstack(() => {>>MARKER:FUNCTION_rtsigprocmask_BLOCK_PREFIX<<
                throw("sigaction failed");
            });

        }
    }
}

// rt_sigaction is implemented in assembly.
//go:noescape
private static int rt_sigaction(System.UIntPtr sig, ptr<sigactiont> @new, ptr<sigactiont> old, System.UIntPtr size);

private static nint getpid();
private static void tgkill(nint tgid, nint tid, nint sig);

// signalM sends a signal to mp.
private static void signalM(ptr<m> _addr_mp, nint sig) {
    ref m mp = ref _addr_mp.val;

    tgkill(getpid(), int(mp.procid), sig);
}

} // end runtime_package
