// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:10:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_openbsd.go
using atomic = go.runtime.@internal.atomic_package;
using @unsafe = go.@unsafe_package;
using System;


namespace go;

public static partial class runtime_package {

private partial struct mOS {
    public uint waitsemacount;
}

private static readonly nint _ESRCH = 3;
private static readonly var _EWOULDBLOCK = _EAGAIN;
private static readonly nint _ENOTSUP = 91; 

// From OpenBSD's sys/time.h
private static readonly nint _CLOCK_REALTIME = 0;
private static readonly nint _CLOCK_VIRTUAL = 1;
private static readonly nint _CLOCK_PROF = 2;
private static readonly nint _CLOCK_MONOTONIC = 3;


private partial struct sigset { // : uint
}

private static var sigset_all = ~sigset(0);

// From OpenBSD's <sys/sysctl.h>
private static readonly nint _CTL_KERN = 1;
private static readonly nint _KERN_OSREV = 3;

private static readonly nint _CTL_HW = 6;
private static readonly nint _HW_NCPU = 3;
private static readonly nint _HW_PAGESIZE = 7;
private static readonly nint _HW_NCPUONLINE = 25;


private static (int, bool) sysctlInt(slice<uint> mib) {
    int _p0 = default;
    bool _p0 = default;

    ref int @out = ref heap(out ptr<int> _addr_@out);
    ref var nout = ref heap(@unsafe.Sizeof(out), out ptr<var> _addr_nout);
    var ret = sysctl(_addr_mib[0], uint32(len(mib)), (byte.val)(@unsafe.Pointer(_addr_out)), _addr_nout, null, 0);
    if (ret < 0) {
        return (0, false);
    }
    return (out, true);

}

private static int getncpu() { 
    // Try hw.ncpuonline first because hw.ncpu would report a number twice as
    // high as the actual CPUs running on OpenBSD 6.4 with hyperthreading
    // disabled (hw.smt=0). See https://golang.org/issue/30127
    {
        var n__prev1 = n;

        var (n, ok) = sysctlInt(new slice<uint>(new uint[] { _CTL_HW, _HW_NCPUONLINE }));

        if (ok) {
            return int32(n);
        }
        n = n__prev1;

    }

    {
        var n__prev1 = n;

        (n, ok) = sysctlInt(new slice<uint>(new uint[] { _CTL_HW, _HW_NCPU }));

        if (ok) {
            return int32(n);
        }
        n = n__prev1;

    }

    return 1;

}

private static System.UIntPtr getPageSize() {
    {
        var (ps, ok) = sysctlInt(new slice<uint>(new uint[] { _CTL_HW, _HW_PAGESIZE }));

        if (ok) {
            return uintptr(ps);
        }
    }

    return 0;

}

private static nint getOSRev() {
    {
        var (osrev, ok) = sysctlInt(new slice<uint>(new uint[] { _CTL_KERN, _KERN_OSREV }));

        if (ok) {
            return int(osrev);
        }
    }

    return 0;

}

//go:nosplit
private static void semacreate(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

}

//go:nosplit
private static int semasleep(long ns) {
    var _g_ = getg(); 

    // Compute sleep deadline.
    ptr<timespec> tsp;
    if (ns >= 0) {
        ref timespec ts = ref heap(out ptr<timespec> _addr_ts);
        ts.setNsec(ns + nanotime());
        tsp = _addr_ts;
    }
    while (true) {
        var v = atomic.Load(_addr__g_.m.waitsemacount);
        if (v > 0) {
            if (atomic.Cas(_addr__g_.m.waitsemacount, v, v - 1)) {
                return 0; // semaphore acquired
            }

            continue;

        }
        var ret = thrsleep(uintptr(@unsafe.Pointer(_addr__g_.m.waitsemacount)), _CLOCK_MONOTONIC, tsp, 0, _addr__g_.m.waitsemacount);
        if (ret == _EWOULDBLOCK) {
            return -1;
        }
    }

}

//go:nosplit
private static void semawakeup(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    atomic.Xadd(_addr_mp.waitsemacount, 1);
    var ret = thrwakeup(uintptr(@unsafe.Pointer(_addr_mp.waitsemacount)), 1);
    if (ret != 0 && ret != _ESRCH) { 
        // semawakeup can be called on signal stack.
        systemstack(() => {
            print("thrwakeup addr=", _addr_mp.waitsemacount, " sem=", mp.waitsemacount, " ret=", ret, "\n");
        });

    }
}

private static void osinit() {
    ncpu = getncpu();
    physPageSize = getPageSize();
    haveMapStack = getOSRev() >= 201805; // OpenBSD 6.3
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

    var gsignalSize = int32(32 * 1024);
    if (GOARCH == "mips64") {
        gsignalSize = int32(64 * 1024);
    }
    mp.gsignal = malg(gsignalSize);
    mp.gsignal.m = mp;

}

// Called to initialize a new m (including the bootstrap m).
// Called on the new thread, can not allocate memory.
private static void minit() {
    getg().m.procid = uint64(getthrid());
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
    public System.UIntPtr sa_sigaction;
    public uint sa_mask;
    public int sa_flags;
}

//go:nosplit
//go:nowritebarrierrec
private static void setsig(uint i, System.UIntPtr fn) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
    sa.sa_flags = _SA_SIGINFO | _SA_ONSTACK | _SA_RESTART;
    sa.sa_mask = uint32(sigset_all);
    if (fn == funcPC(sighandler)) {>>MARKER:FUNCTION_sigtramp_BLOCK_PREFIX<<
        fn = funcPC(sigtramp);
    }
    sa.sa_sigaction = fn;
    sigaction(i, _addr_sa, null);

}

//go:nosplit
//go:nowritebarrierrec
private static void setsigstack(uint i) {
    throw("setsigstack");
}

//go:nosplit
//go:nowritebarrierrec
private static System.UIntPtr getsig(uint i) {
    ref sigactiont sa = ref heap(out ptr<sigactiont> _addr_sa);
    sigaction(i, null, _addr_sa);
    return sa.sa_sigaction;
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

    mask |= 1 << (int)((uint32(i) - 1));
}

private static void sigdelset(ptr<sigset> _addr_mask, nint i) {
    ref sigset mask = ref _addr_mask.val;

    mask &= 1 << (int)((uint32(i) - 1));
}

//go:nosplit
private static void fixsigcode(this ptr<sigctxt> _addr_c, uint sig) {
    ref sigctxt c = ref _addr_c.val;

}

private static var haveMapStack = false;

private static void osStackAlloc(ptr<mspan> _addr_s) {
    ref mspan s = ref _addr_s.val;
 
    // OpenBSD 6.4+ requires that stacks be mapped with MAP_STACK.
    // It will check this on entry to system calls, traps, and
    // when switching to the alternate system stack.
    //
    // This function is called before s is used for any data, so
    // it's safe to simply re-map it.
    osStackRemap(_addr_s, _MAP_STACK);

}

private static void osStackFree(ptr<mspan> _addr_s) {
    ref mspan s = ref _addr_s.val;
 
    // Undo MAP_STACK.
    osStackRemap(_addr_s, 0);

}

private static void osStackRemap(ptr<mspan> _addr_s, int flags) {
    ref mspan s = ref _addr_s.val;

    if (!haveMapStack) { 
        // OpenBSD prior to 6.3 did not have MAP_STACK and so
        // the following mmap will fail. But it also didn't
        // require MAP_STACK (obviously), so there's no need
        // to do the mmap.
        return ;

    }
    var (a, err) = mmap(@unsafe.Pointer(s.@base()), s.npages * pageSize, _PROT_READ | _PROT_WRITE, _MAP_PRIVATE | _MAP_ANON | _MAP_FIXED | flags, -1, 0);
    if (err != 0 || uintptr(a) != s.@base()) {
        print("runtime: remapping stack memory ", hex(s.@base()), " ", s.npages * pageSize, " a=", a, " err=", err, "\n");
        throw("remapping stack memory failed");
    }
}

//go:nosplit
private static void raise(uint sig) {
    thrkill(getthrid(), int(sig));
}

private static void signalM(ptr<m> _addr_mp, nint sig) {
    ref m mp = ref _addr_mp.val;

    thrkill(int32(mp.procid), sig);
}

} // end runtime_package
