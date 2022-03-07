// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build js && wasm
// +build js,wasm

// package runtime -- go2cs converted at 2022 March 06 22:10:28 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_js.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

private static void exit(int code);

private static int write1(System.UIntPtr fd, unsafe.Pointer p, int n) {
    if (fd > 2) {>>MARKER:FUNCTION_exit_BLOCK_PREFIX<<
        throw("runtime.write to fd > 2 is unsupported");
    }
    wasmWrite(fd, p, n);
    return n;

}

// Stubs so tests can link correctly. These should never be called.
private static int open(ptr<byte> _addr_name, int mode, int perm) => func((_, panic, _) => {
    ref byte name = ref _addr_name.val;

    panic("not implemented");
});
private static int closefd(int fd) => func((_, panic, _) => {
    panic("not implemented");
});
private static int read(int fd, unsafe.Pointer p, int n) => func((_, panic, _) => {
    panic("not implemented");
});

//go:noescape
private static void wasmWrite(System.UIntPtr fd, unsafe.Pointer p, int n);

private static void usleep(uint usec);

//go:nosplit
private static void usleep_no_g(uint usec) {
    usleep(usec);
}

private static void exitThread(ptr<uint> wait);

private partial struct mOS {
}

private static void osyield();

//go:nosplit
private static void osyield_no_g() {
    osyield();
}

private static readonly nuint _SIGSEGV = 0xb;



private static void sigpanic() {
    var g = getg();
    if (!canpanic(g)) {>>MARKER:FUNCTION_osyield_BLOCK_PREFIX<<
        throw("unexpected signal during runtime execution");
    }
    g.sig = _SIGSEGV;
    panicmem();

}

private partial struct sigset {
}

// Called to initialize a new m (including the bootstrap m).
// Called on the parent thread (main thread in case of bootstrap), can allocate memory.
private static void mpreinit(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

    mp.gsignal = malg(32 * 1024);
    mp.gsignal.m = mp;
}

//go:nosplit
private static void sigsave(ptr<sigset> _addr_p) {
    ref sigset p = ref _addr_p.val;

}

//go:nosplit
private static void msigrestore(sigset sigmask) {
}

//go:nosplit
//go:nowritebarrierrec
private static void clearSignalHandlers() {
}

//go:nosplit
private static void sigblock(bool exiting) {
}

// Called to initialize a new m (including the bootstrap m).
// Called on the new thread, cannot allocate memory.
private static void minit() {
}

// Called from dropm to undo the effect of an minit.
private static void unminit() {
}

// Called from exitm, but not from drop, to undo the effect of thread-owned
// resources in minit, semacreate, or elsewhere. Do not take locks after calling this.
private static void mdestroy(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;

}

private static void osinit() {
    ncpu = 1;
    getg().m.procid = 2;
    physPageSize = 64 * 1024;
}

// wasm has no signals
private static readonly nint _NSIG = 0;



private static @string signame(uint sig) {
    return "";
}

private static void crash() {
    (int32.val)(null).val;

    0;

}

private static void getRandomData(slice<byte> r);

private static void goenvs() {
    goenvs_unix();
}

private static void initsig(bool preinit) {
}

// May run with m.p==nil, so write barriers are not allowed.
//go:nowritebarrier
private static void newosproc(ptr<m> _addr_mp) => func((_, panic, _) => {
    ref m mp = ref _addr_mp.val;

    panic("newosproc: not implemented");
});

private static void setProcessCPUProfiler(int hz) {
}
private static void setThreadCPUProfiler(int hz) {
}
private static void sigdisable(uint _p0) {
}
private static void sigenable(uint _p0) {
}
private static void sigignore(uint _p0) {
}

//go:linkname os_sigpipe os.sigpipe
private static void os_sigpipe() {
    throw("too many writes on closed pipe");
}

//go:nosplit
private static long cputicks() { 
    // Currently cputicks() is used in blocking profiler and to seed runtime·fastrand().
    // runtime·nanotime() is a poor approximation of CPU ticks that is enough for the profiler.
    return nanotime();

}

//go:linkname syscall_now syscall.now
private static (long, int) syscall_now() {
    long sec = default;
    int nsec = default;

    sec, nsec, _ = time_now();
    return ;
}

// gsignalStack is unused on js.
private partial struct gsignalStack {
}

private static readonly var preemptMSupported = false;



private static void preemptM(ptr<m> _addr_mp) {
    ref m mp = ref _addr_mp.val;
 
    // No threads, so nothing to do.
}

} // end runtime_package
