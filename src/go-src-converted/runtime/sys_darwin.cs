// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package runtime -- go2cs converted at 2022 March 06 22:12:08 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sys_darwin.go
using abi = go.@internal.abi_package;
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // The X versions of syscall expect the libc call to return a 64-bit result.
    // Otherwise (the non-X version) expects a 32-bit result.
    // This distinction is required because an error is indicated by returning -1,
    // and we need to know whether to check 32 or 64 bits of the result.
    // (Some libc functions that return 32 bits put junk in the upper 32 bits of AX.)

    //go:linkname syscall_syscall syscall.syscall
    //go:nosplit
    //go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    entersyscall();
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(syscall)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscall();

//go:linkname syscall_syscallX syscall.syscallX
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscallX(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    entersyscall();
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(syscallX)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscallX();

//go:linkname syscall_syscall6 syscall.syscall6
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscall6(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    entersyscall();
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(syscall6)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscall6();

//go:linkname syscall_syscall6X syscall.syscall6X
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscall6X(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    entersyscall();
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(syscall6X)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscall6X();

//go:linkname syscall_syscallPtr syscall.syscallPtr
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_syscallPtr(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    entersyscall();
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(syscallPtr)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscallPtr();

//go:linkname syscall_rawSyscall syscall.rawSyscall
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawSyscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    libcCall(@unsafe.Pointer(abi.FuncPCABI0(syscall)), @unsafe.Pointer(_addr_fn));
    return ;
}

//go:linkname syscall_rawSyscall6 syscall.rawSyscall6
//go:nosplit
//go:cgo_unsafe_args
private static (System.UIntPtr, System.UIntPtr, System.UIntPtr) syscall_rawSyscall6(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;
    System.UIntPtr r2 = default;
    System.UIntPtr err = default;

    libcCall(@unsafe.Pointer(abi.FuncPCABI0(syscall6)), @unsafe.Pointer(_addr_fn));
    return ;
}

// syscallNoErr is used in crypto/x509 to call into Security.framework and CF.

//go:linkname crypto_x509_syscall crypto/x509/internal/macos.syscall
//go:nosplit
//go:cgo_unsafe_args
private static System.UIntPtr crypto_x509_syscall(System.UIntPtr fn, System.UIntPtr a1, System.UIntPtr a2, System.UIntPtr a3, System.UIntPtr a4, System.UIntPtr a5, System.UIntPtr a6) {
    System.UIntPtr r1 = default;

    entersyscall();
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(syscallNoErr)), @unsafe.Pointer(_addr_fn));
    exitsyscall();
    return ;
}
private static void syscallNoErr();

// The *_trampoline functions convert from the Go calling convention to the C calling convention
// and then call the underlying libc function.  They are defined in sys_darwin_$ARCH.s.

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_attr_init(ptr<pthreadattr> _addr_attr) {
    ref pthreadattr attr = ref _addr_attr.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_attr_init_trampoline)), @unsafe.Pointer(_addr_attr));
}
private static void pthread_attr_init_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_attr_getstacksize(ptr<pthreadattr> _addr_attr, ptr<System.UIntPtr> _addr_size) {
    ref pthreadattr attr = ref _addr_attr.val;
    ref System.UIntPtr size = ref _addr_size.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_attr_getstacksize_trampoline)), @unsafe.Pointer(_addr_attr));
}
private static void pthread_attr_getstacksize_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_attr_setdetachstate(ptr<pthreadattr> _addr_attr, nint state) {
    ref pthreadattr attr = ref _addr_attr.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_attr_setdetachstate_trampoline)), @unsafe.Pointer(_addr_attr));
}
private static void pthread_attr_setdetachstate_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_create(ptr<pthreadattr> _addr_attr, System.UIntPtr start, unsafe.Pointer arg) {
    ref pthreadattr attr = ref _addr_attr.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_create_trampoline)), @unsafe.Pointer(_addr_attr));
}
private static void pthread_create_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void raise(uint sig) {
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(raise_trampoline)), @unsafe.Pointer(_addr_sig));
}
private static void raise_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static pthread pthread_self() {
    pthread t = default;

    libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_self_trampoline)), @unsafe.Pointer(_addr_t));
    return ;
}
private static void pthread_self_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void pthread_kill(pthread t, uint sig) {
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_kill_trampoline)), @unsafe.Pointer(_addr_t));
    return ;
}
private static void pthread_kill_trampoline();

// mmap is used to do low-level memory allocation via mmap. Don't allow stack
// splits, since this function (used by sysAlloc) is called in a lot of low-level
// parts of the runtime and callers often assume it won't acquire any locks.
// go:nosplit
private static (unsafe.Pointer, nint) mmap(unsafe.Pointer addr, System.UIntPtr n, int prot, int flags, int fd, uint off) {
    unsafe.Pointer _p0 = default;
    nint _p0 = default;

    ref struct{addrunsafe.Pointernuintptrprot,flags,fdint32offuint32ret1unsafe.Pointerret2int} args = ref heap(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{addrunsafe.Pointernuintptrprot,flags,fdint32offuint32ret1unsafe.Pointerret2int}{addr,n,prot,flags,fd,off,nil,0}, out ptr<struct{addrunsafe.Pointernuintptrprot,flags,fdint32offuint32ret1unsafe.Pointerret2int}> _addr_args);
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(mmap_trampoline)), @unsafe.Pointer(_addr_args));
    return (args.ret1, args.ret2);
}
private static void mmap_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void munmap(unsafe.Pointer addr, System.UIntPtr n) {
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(munmap_trampoline)), @unsafe.Pointer(_addr_addr));
}
private static void munmap_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void madvise(unsafe.Pointer addr, System.UIntPtr n, int flags) {
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(madvise_trampoline)), @unsafe.Pointer(_addr_addr));
}
private static void madvise_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void mlock(unsafe.Pointer addr, System.UIntPtr n) {
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(mlock_trampoline)), @unsafe.Pointer(_addr_addr));
}
private static void mlock_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int read(int fd, unsafe.Pointer p, int n) {
    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(read_trampoline)), @unsafe.Pointer(_addr_fd));
}
private static void read_trampoline();

private static (int, int, int) pipe() {
    int r = default;
    int w = default;
    int errno = default;

    ref array<int> p = ref heap(new array<int>(2), out ptr<array<int>> _addr_p);
    errno = libcCall(@unsafe.Pointer(abi.FuncPCABI0(pipe_trampoline)), noescape(@unsafe.Pointer(_addr_p)));
    return (p[0], p[1], errno);
}
private static void pipe_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int closefd(int fd) {
    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(close_trampoline)), @unsafe.Pointer(_addr_fd));
}
private static void close_trampoline();

//go:nosplit
//go:cgo_unsafe_args
//
// This is exported via linkname to assembly in runtime/cgo.
//go:linkname exit
private static void exit(int code) {
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(exit_trampoline)), @unsafe.Pointer(_addr_code));
}
private static void exit_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void usleep(uint usec) {
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(usleep_trampoline)), @unsafe.Pointer(_addr_usec));
}
private static void usleep_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void usleep_no_g(uint usec) {
    asmcgocall_no_g(@unsafe.Pointer(abi.FuncPCABI0(usleep_trampoline)), @unsafe.Pointer(_addr_usec));
}

//go:nosplit
//go:cgo_unsafe_args
private static int write1(System.UIntPtr fd, unsafe.Pointer p, int n) {
    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(write_trampoline)), @unsafe.Pointer(_addr_fd));
}
private static void write_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int open(ptr<byte> _addr_name, int mode, int perm) {
    int ret = default;
    ref byte name = ref _addr_name.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(open_trampoline)), @unsafe.Pointer(_addr_name));
}
private static void open_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static long nanotime1() {
    ref var r = ref heap(out ptr<var> _addr_r);
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(nanotime_trampoline)), @unsafe.Pointer(_addr_r)); 
    // Note: Apple seems unconcerned about overflow here. See
    // https://developer.apple.com/library/content/qa/qa1398/_index.html
    // Note also, numer == denom == 1 is common.
    var t = r.t;
    if (r.numer != 1) {>>MARKER:FUNCTION_open_trampoline_BLOCK_PREFIX<<
        t *= int64(r.numer);
    }
    if (r.denom != 1) {>>MARKER:FUNCTION_write_trampoline_BLOCK_PREFIX<<
        t /= int64(r.denom);
    }
    return t;

}
private static void nanotime_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static (long, int) walltime() {
    long _p0 = default;
    int _p0 = default;

    ref timespec t = ref heap(out ptr<timespec> _addr_t);
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(walltime_trampoline)), @unsafe.Pointer(_addr_t));
    return (t.tv_sec, int32(t.tv_nsec));
}
private static void walltime_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void sigaction(uint sig, ptr<usigactiont> _addr_@new, ptr<usigactiont> _addr_old) {
    ref usigactiont @new = ref _addr_@new.val;
    ref usigactiont old = ref _addr_old.val;

    libcCall(@unsafe.Pointer(abi.FuncPCABI0(sigaction_trampoline)), @unsafe.Pointer(_addr_sig));
}
private static void sigaction_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void sigprocmask(uint how, ptr<sigset> _addr_@new, ptr<sigset> _addr_old) {
    ref sigset @new = ref _addr_@new.val;
    ref sigset old = ref _addr_old.val;

    libcCall(@unsafe.Pointer(abi.FuncPCABI0(sigprocmask_trampoline)), @unsafe.Pointer(_addr_how));
}
private static void sigprocmask_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void sigaltstack(ptr<stackt> _addr_@new, ptr<stackt> _addr_old) {
    ref stackt @new = ref _addr_@new.val;
    ref stackt old = ref _addr_old.val;

    if (new != null && @new.ss_flags & _SS_DISABLE != 0 && @new.ss_size == 0) {>>MARKER:FUNCTION_sigprocmask_trampoline_BLOCK_PREFIX<< 
        // Despite the fact that Darwin's sigaltstack man page says it ignores the size
        // when SS_DISABLE is set, it doesn't. sigaltstack returns ENOMEM
        // if we don't give it a reasonable size.
        // ref: http://lists.llvm.org/pipermail/llvm-commits/Week-of-Mon-20140421/214296.html
        @new.ss_size = 32768;

    }
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(sigaltstack_trampoline)), @unsafe.Pointer(_addr_new));

}
private static void sigaltstack_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void raiseproc(uint sig) {
    libcCall(@unsafe.Pointer(abi.FuncPCABI0(raiseproc_trampoline)), @unsafe.Pointer(_addr_sig));
}
private static void raiseproc_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void setitimer(int mode, ptr<itimerval> _addr_@new, ptr<itimerval> _addr_old) {
    ref itimerval @new = ref _addr_@new.val;
    ref itimerval old = ref _addr_old.val;

    libcCall(@unsafe.Pointer(abi.FuncPCABI0(setitimer_trampoline)), @unsafe.Pointer(_addr_mode));
}
private static void setitimer_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int sysctl(ptr<uint> _addr_mib, uint miblen, ptr<byte> _addr_oldp, ptr<System.UIntPtr> _addr_oldlenp, ptr<byte> _addr_newp, System.UIntPtr newlen) {
    ref uint mib = ref _addr_mib.val;
    ref byte oldp = ref _addr_oldp.val;
    ref System.UIntPtr oldlenp = ref _addr_oldlenp.val;
    ref byte newp = ref _addr_newp.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(sysctl_trampoline)), @unsafe.Pointer(_addr_mib));
}
private static void sysctl_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int sysctlbyname(ptr<byte> _addr_name, ptr<byte> _addr_oldp, ptr<System.UIntPtr> _addr_oldlenp, ptr<byte> _addr_newp, System.UIntPtr newlen) {
    ref byte name = ref _addr_name.val;
    ref byte oldp = ref _addr_oldp.val;
    ref System.UIntPtr oldlenp = ref _addr_oldlenp.val;
    ref byte newp = ref _addr_newp.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(sysctlbyname_trampoline)), @unsafe.Pointer(_addr_name));
}
private static void sysctlbyname_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int fcntl(int fd, int cmd, int arg) {
    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(fcntl_trampoline)), @unsafe.Pointer(_addr_fd));
}
private static void fcntl_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int kqueue() {
    var v = libcCall(@unsafe.Pointer(abi.FuncPCABI0(kqueue_trampoline)), null);
    return v;
}
private static void kqueue_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int kevent(int kq, ptr<keventt> _addr_ch, int nch, ptr<keventt> _addr_ev, int nev, ptr<timespec> _addr_ts) {
    ref keventt ch = ref _addr_ch.val;
    ref keventt ev = ref _addr_ev.val;
    ref timespec ts = ref _addr_ts.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(kevent_trampoline)), @unsafe.Pointer(_addr_kq));
}
private static void kevent_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_mutex_init(ptr<pthreadmutex> _addr_m, ptr<pthreadmutexattr> _addr_attr) {
    ref pthreadmutex m = ref _addr_m.val;
    ref pthreadmutexattr attr = ref _addr_attr.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_mutex_init_trampoline)), @unsafe.Pointer(_addr_m));
}
private static void pthread_mutex_init_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_mutex_lock(ptr<pthreadmutex> _addr_m) {
    ref pthreadmutex m = ref _addr_m.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_mutex_lock_trampoline)), @unsafe.Pointer(_addr_m));
}
private static void pthread_mutex_lock_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_mutex_unlock(ptr<pthreadmutex> _addr_m) {
    ref pthreadmutex m = ref _addr_m.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_mutex_unlock_trampoline)), @unsafe.Pointer(_addr_m));
}
private static void pthread_mutex_unlock_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_cond_init(ptr<pthreadcond> _addr_c, ptr<pthreadcondattr> _addr_attr) {
    ref pthreadcond c = ref _addr_c.val;
    ref pthreadcondattr attr = ref _addr_attr.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_cond_init_trampoline)), @unsafe.Pointer(_addr_c));
}
private static void pthread_cond_init_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_cond_wait(ptr<pthreadcond> _addr_c, ptr<pthreadmutex> _addr_m) {
    ref pthreadcond c = ref _addr_c.val;
    ref pthreadmutex m = ref _addr_m.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_cond_wait_trampoline)), @unsafe.Pointer(_addr_c));
}
private static void pthread_cond_wait_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_cond_timedwait_relative_np(ptr<pthreadcond> _addr_c, ptr<pthreadmutex> _addr_m, ptr<timespec> _addr_t) {
    ref pthreadcond c = ref _addr_c.val;
    ref pthreadmutex m = ref _addr_m.val;
    ref timespec t = ref _addr_t.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_cond_timedwait_relative_np_trampoline)), @unsafe.Pointer(_addr_c));
}
private static void pthread_cond_timedwait_relative_np_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int pthread_cond_signal(ptr<pthreadcond> _addr_c) {
    ref pthreadcond c = ref _addr_c.val;

    return libcCall(@unsafe.Pointer(abi.FuncPCABI0(pthread_cond_signal_trampoline)), @unsafe.Pointer(_addr_c));
}
private static void pthread_cond_signal_trampoline();

// Not used on Darwin, but must be defined.
private static void exitThread(ptr<uint> _addr_wait) {
    ref uint wait = ref _addr_wait.val;

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

// Tell the linker that the libc_* functions are to be found
// in a system library, with the libc_ prefix missing.

//go:cgo_import_dynamic libc_pthread_attr_init pthread_attr_init "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_attr_getstacksize pthread_attr_getstacksize "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_attr_setdetachstate pthread_attr_setdetachstate "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_create pthread_create "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_self pthread_self "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_kill pthread_kill "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_exit _exit "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_raise raise "/usr/lib/libSystem.B.dylib"

//go:cgo_import_dynamic libc_open open "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_close close "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_read read "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_write write "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pipe pipe "/usr/lib/libSystem.B.dylib"

//go:cgo_import_dynamic libc_mmap mmap "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_munmap munmap "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_madvise madvise "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_mlock mlock "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_error __error "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_usleep usleep "/usr/lib/libSystem.B.dylib"

//go:cgo_import_dynamic libc_mach_timebase_info mach_timebase_info "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_mach_absolute_time mach_absolute_time "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_clock_gettime clock_gettime "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_sigaction sigaction "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_sigmask pthread_sigmask "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_sigaltstack sigaltstack "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_getpid getpid "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_kill kill "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_setitimer setitimer "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_sysctl sysctl "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_sysctlbyname sysctlbyname "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_fcntl fcntl "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_kqueue kqueue "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_kevent kevent "/usr/lib/libSystem.B.dylib"

//go:cgo_import_dynamic libc_pthread_mutex_init pthread_mutex_init "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_mutex_lock pthread_mutex_lock "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_mutex_unlock pthread_mutex_unlock "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_cond_init pthread_cond_init "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_cond_wait pthread_cond_wait "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_cond_timedwait_relative_np pthread_cond_timedwait_relative_np "/usr/lib/libSystem.B.dylib"
//go:cgo_import_dynamic libc_pthread_cond_signal pthread_cond_signal "/usr/lib/libSystem.B.dylib"

} // end runtime_package
