// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build openbsd && mips64
// +build openbsd,mips64

// package runtime -- go2cs converted at 2022 March 06 22:10:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\os_openbsd_syscall2.go
using @unsafe = go.@unsafe_package;

namespace go;

public static partial class runtime_package {

    //go:noescape
private static void sigaction(uint sig, ptr<sigactiont> @new, ptr<sigactiont> old);

private static int kqueue();

//go:noescape
private static int kevent(int kq, ptr<keventt> ch, int nch, ptr<keventt> ev, int nev, ptr<timespec> ts);

private static void raiseproc(uint sig);

private static int getthrid();
private static void thrkill(int tid, nint sig);

// read calls the read system call.
// It returns a non-negative number of bytes written or a negative errno value.
private static int read(int fd, unsafe.Pointer p, int n);

private static int closefd(int fd);

private static void exit(int code);
private static void usleep(uint usec);

//go:nosplit
private static void usleep_no_g(uint usec) {
    usleep(usec);
}

// write calls the write system call.
// It returns a non-negative number of bytes written or a negative errno value.
//go:noescape
private static int write1(System.UIntPtr fd, unsafe.Pointer p, int n);

//go:noescape
private static int open(ptr<byte> name, int mode, int perm);

// return value is only set on linux to be used in osinit()
private static int madvise(unsafe.Pointer addr, System.UIntPtr n, int flags);

// exitThread terminates the current thread, writing *wait = 0 when
// the stack is safe to reclaim.
//
//go:noescape
private static void exitThread(ptr<uint> wait);

//go:noescape
private static sigset obsdsigprocmask(int how, sigset @new);

//go:nosplit
//go:nowritebarrierrec
private static void sigprocmask(int how, ptr<sigset> _addr_@new, ptr<sigset> _addr_old) {
    ref sigset @new = ref _addr_@new.val;
    ref sigset old = ref _addr_old.val;

    var n = sigset(0);
    if (new != null) {>>MARKER:FUNCTION_obsdsigprocmask_BLOCK_PREFIX<<
        n = new.val;
    }
    var r = obsdsigprocmask(how, n);
    if (old != null) {>>MARKER:FUNCTION_exitThread_BLOCK_PREFIX<<
        old = r;
    }
}

private static (int, int, int) pipe();
private static (int, int, int) pipe2(int flags);

//go:noescape
private static void setitimer(int mode, ptr<itimerval> @new, ptr<itimerval> old);

//go:noescape
private static int sysctl(ptr<uint> mib, uint miblen, ptr<byte> @out, ptr<System.UIntPtr> size, ptr<byte> dst, System.UIntPtr ndst);

// mmap calls the mmap system call. It is implemented in assembly.
// We only pass the lower 32 bits of file offset to the
// assembly routine; the higher bits (if required), should be provided
// by the assembly routine as 0.
// The err result is an OS error code such as ENOMEM.
private static (unsafe.Pointer, nint) mmap(unsafe.Pointer addr, System.UIntPtr n, int prot, int flags, int fd, uint off);

// munmap calls the munmap system call. It is implemented in assembly.
private static void munmap(unsafe.Pointer addr, System.UIntPtr n);

private static long nanotime1();

//go:noescape
private static void sigaltstack(ptr<stackt> @new, ptr<stackt> old);

private static void closeonexec(int fd);
private static void setNonblock(int fd);

private static (long, int) walltime();

} // end runtime_package
