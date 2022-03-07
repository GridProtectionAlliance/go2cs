// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build aix
// +build aix

// package runtime -- go2cs converted at 2022 March 06 22:08:33 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_aix_ppc64.go


namespace go;

public static partial class runtime_package {

private static readonly nuint _EPERM = 0x1;
private static readonly nuint _ENOENT = 0x2;
private static readonly nuint _EINTR = 0x4;
private static readonly nuint _EAGAIN = 0xb;
private static readonly nuint _ENOMEM = 0xc;
private static readonly nuint _EACCES = 0xd;
private static readonly nuint _EFAULT = 0xe;
private static readonly nuint _EINVAL = 0x16;
private static readonly nuint _ETIMEDOUT = 0x4e;

private static readonly nuint _PROT_NONE = 0x0;
private static readonly nuint _PROT_READ = 0x1;
private static readonly nuint _PROT_WRITE = 0x2;
private static readonly nuint _PROT_EXEC = 0x4;

private static readonly nuint _MAP_ANON = 0x10;
private static readonly nuint _MAP_PRIVATE = 0x2;
private static readonly nuint _MAP_FIXED = 0x100;
private static readonly nuint _MADV_DONTNEED = 0x4;

private static readonly nuint _SIGHUP = 0x1;
private static readonly nuint _SIGINT = 0x2;
private static readonly nuint _SIGQUIT = 0x3;
private static readonly nuint _SIGILL = 0x4;
private static readonly nuint _SIGTRAP = 0x5;
private static readonly nuint _SIGABRT = 0x6;
private static readonly nuint _SIGBUS = 0xa;
private static readonly nuint _SIGFPE = 0x8;
private static readonly nuint _SIGKILL = 0x9;
private static readonly nuint _SIGUSR1 = 0x1e;
private static readonly nuint _SIGSEGV = 0xb;
private static readonly nuint _SIGUSR2 = 0x1f;
private static readonly nuint _SIGPIPE = 0xd;
private static readonly nuint _SIGALRM = 0xe;
private static readonly nuint _SIGCHLD = 0x14;
private static readonly nuint _SIGCONT = 0x13;
private static readonly nuint _SIGSTOP = 0x11;
private static readonly nuint _SIGTSTP = 0x12;
private static readonly nuint _SIGTTIN = 0x15;
private static readonly nuint _SIGTTOU = 0x16;
private static readonly nuint _SIGURG = 0x10;
private static readonly nuint _SIGXCPU = 0x18;
private static readonly nuint _SIGXFSZ = 0x19;
private static readonly nuint _SIGVTALRM = 0x22;
private static readonly nuint _SIGPROF = 0x20;
private static readonly nuint _SIGWINCH = 0x1c;
private static readonly nuint _SIGIO = 0x17;
private static readonly nuint _SIGPWR = 0x1d;
private static readonly nuint _SIGSYS = 0xc;
private static readonly nuint _SIGTERM = 0xf;
private static readonly nuint _SIGEMT = 0x7;
private static readonly nuint _SIGWAITING = 0x27;

private static readonly nuint _FPE_INTDIV = 0x14;
private static readonly nuint _FPE_INTOVF = 0x15;
private static readonly nuint _FPE_FLTDIV = 0x16;
private static readonly nuint _FPE_FLTOVF = 0x17;
private static readonly nuint _FPE_FLTUND = 0x18;
private static readonly nuint _FPE_FLTRES = 0x19;
private static readonly nuint _FPE_FLTINV = 0x1a;
private static readonly nuint _FPE_FLTSUB = 0x1b;

private static readonly nuint _BUS_ADRALN = 0x1;
private static readonly nuint _BUS_ADRERR = 0x2;
private static readonly nuint _BUS_OBJERR = 0x3;
private static readonly _SEGV_MAPERR _ = 0x32;
private static readonly nuint _SEGV_ACCERR = 0x33;

private static readonly nuint _ITIMER_REAL = 0x0;
private static readonly nuint _ITIMER_VIRTUAL = 0x1;
private static readonly nuint _ITIMER_PROF = 0x2;

private static readonly nuint _O_RDONLY = 0x0;
private static readonly nuint _O_NONBLOCK = 0x4;

private static readonly nuint _SS_DISABLE = 0x2;
private static readonly nuint _SI_USER = 0x0;
private static readonly nuint _SIG_BLOCK = 0x0;
private static readonly nuint _SIG_UNBLOCK = 0x1;
private static readonly nuint _SIG_SETMASK = 0x2;

private static readonly nuint _SA_SIGINFO = 0x100;
private static readonly nuint _SA_RESTART = 0x8;
private static readonly nuint _SA_ONSTACK = 0x1;

private static readonly nuint _PTHREAD_CREATE_DETACHED = 0x1;

private static readonly nuint __SC_PAGE_SIZE = 0x30;
private static readonly nuint __SC_NPROCESSORS_ONLN = 0x48;

private static readonly nuint _F_SETFD = 0x2;
private static readonly nuint _F_SETFL = 0x4;
private static readonly nuint _F_GETFD = 0x1;
private static readonly nuint _F_GETFL = 0x3;
private static readonly nuint _FD_CLOEXEC = 0x1;


private partial struct sigset { // : array<ulong>
}

private static sigset sigset_all = new sigset(^uint64(0),^uint64(0),^uint64(0),^uint64(0));

private partial struct siginfo {
    public int si_signo;
    public int si_errno;
    public int si_code;
    public int si_pid;
    public uint si_uid;
    public int si_status;
    public System.UIntPtr si_addr;
    public long si_band;
    public array<int> si_value; // [8]byte
    public int __si_flags;
    public array<int> __pad;
}

private partial struct timespec {
    public long tv_sec;
    public long tv_nsec;
}

//go:nosplit
private static void setNsec(this ptr<timespec> _addr_ts, long ns) {
    ref timespec ts = ref _addr_ts.val;

    ts.tv_sec = ns / 1e9F;
    ts.tv_nsec = ns % 1e9F;
}

private partial struct timeval {
    public long tv_sec;
    public int tv_usec;
    public array<byte> pad_cgo_0;
}

private static void set_usec(this ptr<timeval> _addr_tv, int x) {
    ref timeval tv = ref _addr_tv.val;

    tv.tv_usec = x;
}

private partial struct itimerval {
    public timeval it_interval;
    public timeval it_value;
}

private partial struct stackt {
    public System.UIntPtr ss_sp;
    public System.UIntPtr ss_size;
    public int ss_flags;
    public array<int> __pad;
    public array<byte> pas_cgo_0;
}

private partial struct sigcontext {
    public int sc_onstack;
    public array<byte> pad_cgo_0;
    public sigset sc_mask;
    public int sc_uerror;
    public context64 sc_jmpbuf;
}

private partial struct ucontext {
    public int __sc_onstack;
    public array<byte> pad_cgo_0;
    public sigset uc_sigmask;
    public int __sc_error;
    public array<byte> pad_cgo_1;
    public context64 uc_mcontext;
    public ptr<ucontext> uc_link;
    public stackt uc_stack;
    public System.UIntPtr __extctx; // pointer to struct __extctx but we don't use it
    public int __extctx_magic;
    public int __pad;
}

private partial struct context64 {
    public array<ulong> gpr;
    public ulong msr;
    public ulong iar;
    public ulong lr;
    public ulong ctr;
    public uint cr;
    public uint xer;
    public uint fpscr;
    public uint fpscrx;
    public array<ulong> except;
    public array<double> fpr;
    public byte fpeu;
    public byte fpinfo;
    public byte fpscr24_31;
    public array<byte> pad;
    public int excp_type;
}

private partial struct sigactiont {
    public System.UIntPtr sa_handler; // a union of two pointer
    public sigset sa_mask;
    public int sa_flags;
    public array<byte> pad_cgo_0;
}

private partial struct pthread { // : uint
}
private partial struct pthread_attr { // : ptr<byte>
}

private partial struct semt { // : int
}

} // end runtime_package
