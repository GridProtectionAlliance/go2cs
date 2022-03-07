// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:26 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_freebsd.go
using sys = go.runtime.@internal.sys_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

private partial struct mOS {
}

//go:noescape
private static int thr_new(ptr<thrparam> param, int size);

//go:noescape
private static void sigaltstack(ptr<stackt> @new, ptr<stackt> old);

//go:noescape
private static void sigprocmask(int how, ptr<sigset> @new, ptr<sigset> old);

//go:noescape
private static void setitimer(int mode, ptr<itimerval> @new, ptr<itimerval> old);

//go:noescape
private static int sysctl(ptr<uint> mib, uint miblen, ptr<byte> @out, ptr<System.UIntPtr> size, ptr<byte> dst, System.UIntPtr ndst);

private static void raiseproc(uint sig);

private static thread thr_self();
private static void thr_kill(thread tid, nint sig);

//go:noescape
private static int sys_umtx_op(ptr<uint> addr, int mode, uint val, System.UIntPtr uaddr1, ptr<umtx_time> ut);

private static void osyield();

//go:nosplit
private static void osyield_no_g() {
    osyield();
}

private static int kqueue();

//go:noescape
private static int kevent(int kq, ptr<keventt> ch, int nch, ptr<keventt> ev, int nev, ptr<timespec> ts);

private static (int, int, int) pipe();
private static (int, int, int) pipe2(int flags);
private static void closeonexec(int fd);
private static void setNonblock(int fd);

// From FreeBSD's <sys/sysctl.h>
private static readonly nint _CTL_HW = 6;
private static readonly nint _HW_PAGESIZE = 7;


private static sigset sigset_all = new sigset([4]uint32{^uint32(0),^uint32(0),^uint32(0),^uint32(0)});

// Undocumented numbers from FreeBSD's lib/libc/gen/sysctlnametomib.c.
private static readonly nint _CTL_QUERY = 0;
private static readonly nint _CTL_QUERY_MIB = 3;


// sysctlnametomib fill mib with dynamically assigned sysctl entries of name,
// return count of effected mib slots, return 0 on error.
private static uint sysctlnametomib(slice<byte> name, ptr<array<uint>> _addr_mib) {
    ref array<uint> mib = ref _addr_mib.val;

    array<uint> oid = new array<uint>(new uint[] { _CTL_QUERY, _CTL_QUERY_MIB });
    ref var miblen = ref heap(uintptr(_CTL_MAXNAME), out ptr<var> _addr_miblen);
    if (sysctl(_addr_oid[0], 2, _addr_(byte.val)(@unsafe.Pointer(mib)), _addr_miblen, _addr_(byte.val)(@unsafe.Pointer(_addr_name[0])), (uintptr)(len(name))) < 0) {>>MARKER:FUNCTION_setNonblock_BLOCK_PREFIX<<
        return 0;
    }
    miblen /= @unsafe.Sizeof(uint32(0));
    if (miblen <= 0) {>>MARKER:FUNCTION_closeonexec_BLOCK_PREFIX<<
        return 0;
    }
    return uint32(miblen);

}

private static readonly nint _CPU_CURRENT_PID = -1; // Current process ID.

//go:noescape
private static int cpuset_getaffinity(nint level, nint which, long id, nint size, ptr<byte> mask);

//go:systemstack
private static int getncpu() { 
    // Use a large buffer for the CPU mask. We're on the system
    // stack, so this is fine, and we can't allocate memory for a
    // dynamically-sized buffer at this point.
    const nint maxCPUs = 64 * 1024;

    array<byte> mask = new array<byte>(maxCPUs / 8);
    ref array<uint> mib = ref heap(new array<uint>(_CTL_MAXNAME), out ptr<array<uint>> _addr_mib); 

    // According to FreeBSD's /usr/src/sys/kern/kern_cpuset.c,
    // cpuset_getaffinity return ERANGE when provided buffer size exceed the limits in kernel.
    // Querying kern.smp.maxcpus to calculate maximum buffer size.
    // See https://bugs.freebsd.org/bugzilla/show_bug.cgi?id=200802

    // Variable kern.smp.maxcpus introduced at Dec 23 2003, revision 123766,
    // with dynamically assigned sysctl entries.
    var miblen = sysctlnametomib((slice<byte>)"kern.smp.maxcpus", _addr_mib);
    if (miblen == 0) {>>MARKER:FUNCTION_cpuset_getaffinity_BLOCK_PREFIX<<
        return 1;
    }
    ref var dstsize = ref heap(uintptr(4), out ptr<var> _addr_dstsize);
    ref var maxcpus = ref heap(uint32(0), out ptr<var> _addr_maxcpus);
    if (sysctl(_addr_mib[0], miblen, _addr_(byte.val)(@unsafe.Pointer(_addr_maxcpus)), _addr_dstsize, _addr_null, 0) != 0) {>>MARKER:FUNCTION_pipe2_BLOCK_PREFIX<<
        return 1;
    }
    var maskSize = int(maxcpus + 7) / 8;
    if (maskSize < sys.PtrSize) {>>MARKER:FUNCTION_pipe_BLOCK_PREFIX<<
        maskSize = sys.PtrSize;
    }
    if (maskSize > len(mask)) {>>MARKER:FUNCTION_kevent_BLOCK_PREFIX<<
        maskSize = len(mask);
    }
    if (cpuset_getaffinity(_CPU_LEVEL_WHICH, _CPU_WHICH_PID, _CPU_CURRENT_PID, maskSize, _addr_(byte.val)(@unsafe.Pointer(_addr_mask[0]))) != 0) {>>MARKER:FUNCTION_kqueue_BLOCK_PREFIX<<
        return 1;
    }
    var n = int32(0);
    foreach (var (_, v) in mask[..(int)maskSize]) {
        while (v != 0) {>>MARKER:FUNCTION_osyield_BLOCK_PREFIX<<
            n += int32(v & 1);
            v>>=1;
        }
    }    if (n == 0) {>>MARKER:FUNCTION_sys_umtx_op_BLOCK_PREFIX<<
        return 1;
    }
    return n;

}

private static System.UIntPtr getPageSize() {
    array<uint> mib = new array<uint>(new uint[] { _CTL_HW, _HW_PAGESIZE });
    ref var @out = ref heap(uint32(0), out ptr<var> _addr_@out);
    ref var nout = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_nout);
    var ret = sysctl(_addr_mib[0], 2, _addr_(byte.val)(@unsafe.Pointer(_addr_out)), _addr_nout, _addr_null, 0);
    if (ret >= 0) {>>MARKER:FUNCTION_thr_kill_BLOCK_PREFIX<<
        return uintptr(out);
    }
    return 0;

}

// FreeBSD's umtx_op syscall is effectively the same as Linux's futex, and
// thus the code is largely similar. See Linux implementation
// and lock_futex.go for comments.

//go:nosplit
private static void futexsleep(ptr<uint> _addr_addr, uint val, long ns) {
    ref uint addr = ref _addr_addr.val;

    systemstack(() => {>>MARKER:FUNCTION_thr_self_BLOCK_PREFIX<<
        futexsleep1(_addr_addr, val, ns);
    });
}

private static void futexsleep1(ptr<uint> _addr_addr, uint val, long ns) {
    ref uint addr = ref _addr_addr.val;

    ptr<umtx_time> utp;
    if (ns >= 0) {>>MARKER:FUNCTION_raiseproc_BLOCK_PREFIX<<
        ref umtx_time ut = ref heap(out ptr<umtx_time> _addr_ut);
        ut._clockid = _CLOCK_MONOTONIC;
        ut._timeout.setNsec(ns);
        utp = _addr_ut;
    }
    var ret = sys_umtx_op(_addr_addr, _UMTX_OP_WAIT_UINT_PRIVATE, val, @unsafe.Sizeof(utp.val), utp);
    if (ret >= 0 || ret == -_EINTR || ret == -_ETIMEDOUT) {>>MARKER:FUNCTION_sysctl_BLOCK_PREFIX<<
        return ;
    }
    print("umtx_wait addr=", addr, " val=", val, " ret=", ret, "\n") * (int32.val)(@unsafe.Pointer(uintptr(0x1005)));

    0x1005;

}

//go:nosplit
private static void futexwakeup(ptr<uint> _addr_addr, uint cnt) {
    ref uint addr = ref _addr_addr.val;

    var ret = sys_umtx_op(_addr_addr, _UMTX_OP_WAKE_PRIVATE, cnt, 0, _addr_null);
    if (ret >= 0) {>>MARKER:FUNCTION_setitimer_BLOCK_PREFIX<<
        return ;
    }
    systemstack(() => {>>MARKER:FUNCTION_sigprocmask_BLOCK_PREFIX<<
        print("umtx_wake_addr=", addr, " ret=", ret, "\n");
    });

}

private static void thr_start();

// May run with m.p==nil, so write barriers are not allowed.
//go:nowritebarrier
private static void newosproc(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    var stk = @unsafe.Pointer(mp.g0.stack.hi);
    if (false) {>>MARKER:FUNCTION_thr_start_BLOCK_PREFIX<<
        print("newosproc stk=", stk, " m=", mp, " g=", mp.g0, " thr_start=", funcPC(thr_start), " id=", mp.id, " ostk=", _addr_mp, "\n");
    }
    ref thrparam param = ref heap(new thrparam(start_func:funcPC(thr_start),arg:unsafe.Pointer(mp),stack_base:mp.g0.stack.lo,stack_size:uintptr(stk)-mp.g0.stack.lo,child_tid:nil,parent_tid:nil,tls_base:unsafe.Pointer(&mp.tls[0]),tls_size:unsafe.Sizeof(mp.tls),), out ptr<thrparam> _addr_param);

    ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
    sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
    var ret = thr_new(_addr_param, int32(@unsafe.Sizeof(param)));
    sigprocmask(_SIG_SETMASK, _addr_oset, _addr_null);
    if (ret < 0) {>>MARKER:FUNCTION_sigaltstack_BLOCK_PREFIX<<
        print("runtime: failed to create new OS thread (have ", mcount(), " already; errno=", -ret, ")\n");
        throw("newosproc");
    }
}

// Version of newosproc that doesn't require a valid G.
//go:nosplit
private static void newosproc0(System.UIntPtr stacksize, unsafe.Pointer fn) {
    var stack = sysAlloc(stacksize, _addr_memstats.stacks_sys);
    if (stack == null) {>>MARKER:FUNCTION_thr_new_BLOCK_PREFIX<<
        write(2, @unsafe.Pointer(_addr_failallocatestack[0]), int32(len(failallocatestack)));
        exit(1);
    }
    ref thrparam param = ref heap(new thrparam(start_func:funcPC(fn),arg:nil,stack_base:uintptr(stack),stack_size:stacksize,child_tid:nil,parent_tid:nil,tls_base:unsafe.Pointer(&m0.tls[0]),tls_size:unsafe.Sizeof(m0.tls),), out ptr<thrparam> _addr_param);

    ref sigset oset = ref heap(out ptr<sigset> _addr_oset);
    sigprocmask(_SIG_SETMASK, _addr_sigset_all, _addr_oset);
    var ret = thr_new(_addr_param, int32(@unsafe.Sizeof(param)));
    sigprocmask(_SIG_SETMASK, _addr_oset, _addr_null);
    if (ret < 0) {
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

private static void osinit() {
    ncpu = getncpu();
    if (physPageSize == 0) {
        physPageSize = getPageSize();
    }
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

// Called to initialize a new m (including the bootstrap m).
// Called on the new thread, cannot allocate memory.
private static void minit() {
    getg().m.procid = uint64(thr_self()); 

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

        if (!isarchive && !islibrary && gp.m == _addr_m0 && gp == gp.m.g0) {
            ref stackt st = ref heap(new stackt(ss_flags:_SS_DISABLE), out ptr<stackt> _addr_st);
            sigaltstack(_addr_st, _addr_null);
        }
    }


    minitSignals();

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

private static void sigtramp();

private partial struct sigactiont {
    public System.UIntPtr sa_handler;
    public int sa_flags;
    public sigset sa_mask;
}

// See os_freebsd2.go, os_freebsd_amd64.go for setsig function

//go:nosplit
//go:nowritebarrierrec
private static void setsigstack(uint i) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
    sigaction(i, null, _addr_sa);
    if (sa.sa_flags & _SA_ONSTACK != 0) {>>MARKER:FUNCTION_sigtramp_BLOCK_PREFIX<<
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

    s.ss_sp = sp;
}

//go:nosplit
//go:nowritebarrierrec
private static void sigaddset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    mask.__bits[(i - 1) / 32] |= 1 << (int)(((uint32(i) - 1) & 31));
}

private static void sigdelset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    mask.__bits[(i - 1) / 32] &= 1 << (int)(((uint32(i) - 1) & 31));
}

//go:nosplit
private static void fixsigcode(this ptr<sigctxt> _addr_c, uint sig) {
    ref sigctxt c = ref _addr_c.val;

}

private static void sysargs(int argc, ptr<ptr<byte>> _addr_argv) {
    ref ptr<byte> argv = ref _addr_argv.val;

    var n = argc + 1; 

    // skip over argv, envp to get to auxv
    while (argv_index(argv, n) != null) {
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
private static readonly nint _AT_TIMEKEEP = 22; // Pointer to timehands.
private static readonly nint _AT_HWCAP = 25; // CPU feature flags
private static readonly nint _AT_HWCAP2 = 26; // CPU feature flags 2

private static void sysauxv(slice<System.UIntPtr> auxv) {
    {
        nint i = 0;

        while (auxv[i] != _AT_NULL) {
            var tag = auxv[i];
            var val = auxv[i + 1];

            // _AT_NCPUS from auxv shouldn't be used due to golang.org/issue/15206
            if (tag == _AT_PAGESZ) 
                physPageSize = val;
            else if (tag == _AT_TIMEKEEP) 
                timekeepSharedPage = (vdsoTimekeep.val)(@unsafe.Pointer(val));
                        archauxv(tag, val);
            i += 2;
        }
    }

}

// sysSigaction calls the sigaction system call.
//go:nosplit
private static void sysSigaction(uint sig, ptr<sigactiont> _addr_@new, ptr<sigactiont> _addr_old) {
    ref sigactiont @new = ref _addr_@new.val;
    ref sigactiont old = ref _addr_old.val;
 
    // Use system stack to avoid split stack overflow on amd64
    if (asmSigaction(uintptr(sig), _addr_new, _addr_old) != 0) {
        systemstack(() => {
            throw("sigaction failed");
        });
    }
}

// asmSigaction is implemented in assembly.
//go:noescape
private static int asmSigaction(System.UIntPtr sig, ptr<sigactiont> @new, ptr<sigactiont> old);

// raise sends a signal to the calling thread.
//
// It must be nosplit because it is used by the signal handler before
// it definitely has a Go stack.
//
//go:nosplit
private static void raise(uint sig) {
    thr_kill(thr_self(), int(sig));
}

private static void signalM(ptr<m> _addr_mp, nint sig) {
    ref m mp = ref _addr_mp.val;

    thr_kill(thread(mp.procid), sig);
}

} // end runtime_package
