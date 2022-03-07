// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build openbsd && !mips64
// +build openbsd,!mips64

// package runtime -- go2cs converted at 2022 March 06 22:12:09 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\sys_openbsd2.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    // This is exported via linkname to assembly in runtime/cgo.
    //go:linkname exit
    //go:nosplit
    //go:cgo_unsafe_args
private static void exit(int code) {
    libcCall(@unsafe.Pointer(funcPC(exit_trampoline)), @unsafe.Pointer(_addr_code));
}
private static void exit_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int getthrid() {
    int tid = default;

    libcCall(@unsafe.Pointer(funcPC(getthrid_trampoline)), @unsafe.Pointer(_addr_tid));
    return ;
}
private static void getthrid_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void raiseproc(uint sig) {
    libcCall(@unsafe.Pointer(funcPC(raiseproc_trampoline)), @unsafe.Pointer(_addr_sig));
}
private static void raiseproc_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void thrkill(int tid, nint sig) {
    libcCall(@unsafe.Pointer(funcPC(thrkill_trampoline)), @unsafe.Pointer(_addr_tid));
}
private static void thrkill_trampoline();

// mmap is used to do low-level memory allocation via mmap. Don't allow stack
// splits, since this function (used by sysAlloc) is called in a lot of low-level
// parts of the runtime and callers often assume it won't acquire any locks.
// go:nosplit
private static (unsafe.Pointer, nint) mmap(unsafe.Pointer addr, System.UIntPtr n, int prot, int flags, int fd, uint off) {
    unsafe.Pointer _p0 = default;
    nint _p0 = default;

    ref struct{addrunsafe.Pointernuintptrprot,flags,fdint32offuint32ret1unsafe.Pointerret2int} args = ref heap(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{addrunsafe.Pointernuintptrprot,flags,fdint32offuint32ret1unsafe.Pointerret2int}{addr,n,prot,flags,fd,off,nil,0}, out ptr<struct{addrunsafe.Pointernuintptrprot,flags,fdint32offuint32ret1unsafe.Pointerret2int}> _addr_args);
    libcCall(@unsafe.Pointer(funcPC(mmap_trampoline)), @unsafe.Pointer(_addr_args));
    return (args.ret1, args.ret2);
}
private static void mmap_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void munmap(unsafe.Pointer addr, System.UIntPtr n) {
    libcCall(@unsafe.Pointer(funcPC(munmap_trampoline)), @unsafe.Pointer(_addr_addr));
}
private static void munmap_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void madvise(unsafe.Pointer addr, System.UIntPtr n, int flags) {
    libcCall(@unsafe.Pointer(funcPC(madvise_trampoline)), @unsafe.Pointer(_addr_addr));
}
private static void madvise_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int open(ptr<byte> _addr_name, int mode, int perm) {
    int ret = default;
    ref byte name = ref _addr_name.val;

    return libcCall(@unsafe.Pointer(funcPC(open_trampoline)), @unsafe.Pointer(_addr_name));
}
private static void open_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int closefd(int fd) {
    return libcCall(@unsafe.Pointer(funcPC(close_trampoline)), @unsafe.Pointer(_addr_fd));
}
private static void close_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int read(int fd, unsafe.Pointer p, int n) {
    return libcCall(@unsafe.Pointer(funcPC(read_trampoline)), @unsafe.Pointer(_addr_fd));
}
private static void read_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int write1(System.UIntPtr fd, unsafe.Pointer p, int n) {
    return libcCall(@unsafe.Pointer(funcPC(write_trampoline)), @unsafe.Pointer(_addr_fd));
}
private static void write_trampoline();

private static (int, int, int) pipe() {
    int r = default;
    int w = default;
    int errno = default;

    return pipe2(0);
}

private static (int, int, int) pipe2(int flags) {
    int r = default;
    int w = default;
    int errno = default;

    ref array<int> p = ref heap(new array<int>(2), out ptr<array<int>> _addr_p);
    ref struct{punsafe.Pointerflagsint32} args = ref heap(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{punsafe.Pointerflagsint32}{noescape(unsafe.Pointer(&p)),flags}, out ptr<struct{punsafe.Pointerflagsint32}> _addr_args);
    errno = libcCall(@unsafe.Pointer(funcPC(pipe2_trampoline)), @unsafe.Pointer(_addr_args));
    return (p[0], p[1], errno);
}
private static void pipe2_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void setitimer(int mode, ptr<itimerval> _addr_@new, ptr<itimerval> _addr_old) {
    ref itimerval @new = ref _addr_@new.val;
    ref itimerval old = ref _addr_old.val;

    libcCall(@unsafe.Pointer(funcPC(setitimer_trampoline)), @unsafe.Pointer(_addr_mode));
}
private static void setitimer_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void usleep(uint usec) {
    libcCall(@unsafe.Pointer(funcPC(usleep_trampoline)), @unsafe.Pointer(_addr_usec));
}
private static void usleep_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void usleep_no_g(uint usec) {
    asmcgocall_no_g(@unsafe.Pointer(funcPC(usleep_trampoline)), @unsafe.Pointer(_addr_usec));
}

//go:nosplit
//go:cgo_unsafe_args
private static int sysctl(ptr<uint> _addr_mib, uint miblen, ptr<byte> _addr_@out, ptr<System.UIntPtr> _addr_size, ptr<byte> _addr_dst, System.UIntPtr ndst) {
    ref uint mib = ref _addr_mib.val;
    ref byte @out = ref _addr_@out.val;
    ref System.UIntPtr size = ref _addr_size.val;
    ref byte dst = ref _addr_dst.val;

    return libcCall(@unsafe.Pointer(funcPC(sysctl_trampoline)), @unsafe.Pointer(_addr_mib));
}
private static void sysctl_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int fcntl(int fd, int cmd, int arg) {
    return libcCall(@unsafe.Pointer(funcPC(fcntl_trampoline)), @unsafe.Pointer(_addr_fd));
}
private static void fcntl_trampoline();

//go:nosplit
private static long nanotime1() {
    ref timespec ts = ref heap(out ptr<timespec> _addr_ts);
    ref struct{clock_idint32tpunsafe.Pointer} args = ref heap(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{clock_idint32tpunsafe.Pointer}{_CLOCK_MONOTONIC,unsafe.Pointer(&ts)}, out ptr<struct{clock_idint32tpunsafe.Pointer}> _addr_args);
    libcCall(@unsafe.Pointer(funcPC(clock_gettime_trampoline)), @unsafe.Pointer(_addr_args));
    return ts.tv_sec * 1e9F + int64(ts.tv_nsec);
}
private static void clock_gettime_trampoline();

//go:nosplit
private static (long, int) walltime() {
    long _p0 = default;
    int _p0 = default;

    ref timespec ts = ref heap(out ptr<timespec> _addr_ts);
    ref struct{clock_idint32tpunsafe.Pointer} args = ref heap(/* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ struct{clock_idint32tpunsafe.Pointer}{_CLOCK_REALTIME,unsafe.Pointer(&ts)}, out ptr<struct{clock_idint32tpunsafe.Pointer}> _addr_args);
    libcCall(@unsafe.Pointer(funcPC(clock_gettime_trampoline)), @unsafe.Pointer(_addr_args));
    return (ts.tv_sec, int32(ts.tv_nsec));
}

//go:nosplit
//go:cgo_unsafe_args
private static int kqueue() {
    return libcCall(@unsafe.Pointer(funcPC(kqueue_trampoline)), null);
}
private static void kqueue_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static int kevent(int kq, ptr<keventt> _addr_ch, int nch, ptr<keventt> _addr_ev, int nev, ptr<timespec> _addr_ts) {
    ref keventt ch = ref _addr_ch.val;
    ref keventt ev = ref _addr_ev.val;
    ref timespec ts = ref _addr_ts.val;

    return libcCall(@unsafe.Pointer(funcPC(kevent_trampoline)), @unsafe.Pointer(_addr_kq));
}
private static void kevent_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void sigaction(uint sig, ptr<sigactiont> _addr_@new, ptr<sigactiont> _addr_old) {
    ref sigactiont @new = ref _addr_@new.val;
    ref sigactiont old = ref _addr_old.val;

    libcCall(@unsafe.Pointer(funcPC(sigaction_trampoline)), @unsafe.Pointer(_addr_sig));
}
private static void sigaction_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void sigprocmask(uint how, ptr<sigset> _addr_@new, ptr<sigset> _addr_old) {
    ref sigset @new = ref _addr_@new.val;
    ref sigset old = ref _addr_old.val;

    libcCall(@unsafe.Pointer(funcPC(sigprocmask_trampoline)), @unsafe.Pointer(_addr_how));
}
private static void sigprocmask_trampoline();

//go:nosplit
//go:cgo_unsafe_args
private static void sigaltstack(ptr<stackt> _addr_@new, ptr<stackt> _addr_old) {
    ref stackt @new = ref _addr_@new.val;
    ref stackt old = ref _addr_old.val;

    libcCall(@unsafe.Pointer(funcPC(sigaltstack_trampoline)), @unsafe.Pointer(_addr_new));
}
private static void sigaltstack_trampoline();

// Not used on OpenBSD, but must be defined.
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

//go:cgo_import_dynamic libc_errno __errno "libc.so"
//go:cgo_import_dynamic libc_exit exit "libc.so"
//go:cgo_import_dynamic libc_getthrid getthrid "libc.so"
//go:cgo_import_dynamic libc_sched_yield sched_yield "libc.so"
//go:cgo_import_dynamic libc_thrkill thrkill "libc.so"

//go:cgo_import_dynamic libc_mmap mmap "libc.so"
//go:cgo_import_dynamic libc_munmap munmap "libc.so"
//go:cgo_import_dynamic libc_madvise madvise "libc.so"

//go:cgo_import_dynamic libc_open open "libc.so"
//go:cgo_import_dynamic libc_close close "libc.so"
//go:cgo_import_dynamic libc_read read "libc.so"
//go:cgo_import_dynamic libc_write write "libc.so"
//go:cgo_import_dynamic libc_pipe2 pipe2 "libc.so"

//go:cgo_import_dynamic libc_clock_gettime clock_gettime "libc.so"
//go:cgo_import_dynamic libc_setitimer setitimer "libc.so"
//go:cgo_import_dynamic libc_usleep usleep "libc.so"
//go:cgo_import_dynamic libc_sysctl sysctl "libc.so"
//go:cgo_import_dynamic libc_fcntl fcntl "libc.so"
//go:cgo_import_dynamic libc_getpid getpid "libc.so"
//go:cgo_import_dynamic libc_kill kill "libc.so"
//go:cgo_import_dynamic libc_kqueue kqueue "libc.so"
//go:cgo_import_dynamic libc_kevent kevent "libc.so"

//go:cgo_import_dynamic libc_sigaction sigaction "libc.so"
//go:cgo_import_dynamic libc_sigaltstack sigaltstack "libc.so"

//go:cgo_import_dynamic _ _ "libc.so"

} // end runtime_package
