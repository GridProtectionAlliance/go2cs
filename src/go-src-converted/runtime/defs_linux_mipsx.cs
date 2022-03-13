// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

//go:build (mips || mipsle) && linux
// +build mips mipsle
// +build linux

// package runtime -- go2cs converted at 2022 March 13 05:24:24 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_linux_mipsx.go
namespace go;

public static partial class runtime_package {

private static readonly nuint _EINTR = 0x4;
private static readonly nuint _EAGAIN = 0xb;
private static readonly nuint _ENOMEM = 0xc;
private static readonly nuint _ENOSYS = 0x59;

private static readonly nuint _PROT_NONE = 0x0;
private static readonly nuint _PROT_READ = 0x1;
private static readonly nuint _PROT_WRITE = 0x2;
private static readonly nuint _PROT_EXEC = 0x4;

private static readonly nuint _MAP_ANON = 0x800;
private static readonly nuint _MAP_PRIVATE = 0x2;
private static readonly nuint _MAP_FIXED = 0x10;

private static readonly nuint _MADV_DONTNEED = 0x4;
private static readonly nuint _MADV_FREE = 0x8;
private static readonly nuint _MADV_HUGEPAGE = 0xe;
private static readonly nuint _MADV_NOHUGEPAGE = 0xf;

private static readonly nuint _SA_RESTART = 0x10000000;
private static readonly nuint _SA_ONSTACK = 0x8000000;
private static readonly nuint _SA_SIGINFO = 0x8;

private static readonly nuint _SIGHUP = 0x1;
private static readonly nuint _SIGINT = 0x2;
private static readonly nuint _SIGQUIT = 0x3;
private static readonly nuint _SIGILL = 0x4;
private static readonly nuint _SIGTRAP = 0x5;
private static readonly nuint _SIGABRT = 0x6;
private static readonly nuint _SIGEMT = 0x7;
private static readonly nuint _SIGFPE = 0x8;
private static readonly nuint _SIGKILL = 0x9;
private static readonly nuint _SIGBUS = 0xa;
private static readonly nuint _SIGSEGV = 0xb;
private static readonly nuint _SIGSYS = 0xc;
private static readonly nuint _SIGPIPE = 0xd;
private static readonly nuint _SIGALRM = 0xe;
private static readonly nuint _SIGUSR1 = 0x10;
private static readonly nuint _SIGUSR2 = 0x11;
private static readonly nuint _SIGCHLD = 0x12;
private static readonly nuint _SIGPWR = 0x13;
private static readonly nuint _SIGWINCH = 0x14;
private static readonly nuint _SIGURG = 0x15;
private static readonly nuint _SIGIO = 0x16;
private static readonly nuint _SIGSTOP = 0x17;
private static readonly nuint _SIGTSTP = 0x18;
private static readonly nuint _SIGCONT = 0x19;
private static readonly nuint _SIGTTIN = 0x1a;
private static readonly nuint _SIGTTOU = 0x1b;
private static readonly nuint _SIGVTALRM = 0x1c;
private static readonly nuint _SIGPROF = 0x1d;
private static readonly nuint _SIGXCPU = 0x1e;
private static readonly nuint _SIGXFSZ = 0x1f;

private static readonly nuint _FPE_INTDIV = 0x1;
private static readonly nuint _FPE_INTOVF = 0x2;
private static readonly nuint _FPE_FLTDIV = 0x3;
private static readonly nuint _FPE_FLTOVF = 0x4;
private static readonly nuint _FPE_FLTUND = 0x5;
private static readonly nuint _FPE_FLTRES = 0x6;
private static readonly nuint _FPE_FLTINV = 0x7;
private static readonly nuint _FPE_FLTSUB = 0x8;

private static readonly nuint _BUS_ADRALN = 0x1;
private static readonly nuint _BUS_ADRERR = 0x2;
private static readonly nuint _BUS_OBJERR = 0x3;

private static readonly nuint _SEGV_MAPERR = 0x1;
private static readonly nuint _SEGV_ACCERR = 0x2;

private static readonly nuint _ITIMER_REAL = 0x0;
private static readonly nuint _ITIMER_VIRTUAL = 0x1;
private static readonly nuint _ITIMER_PROF = 0x2;

private static readonly nuint _EPOLLIN = 0x1;
private static readonly nuint _EPOLLOUT = 0x4;
private static readonly nuint _EPOLLERR = 0x8;
private static readonly nuint _EPOLLHUP = 0x10;
private static readonly nuint _EPOLLRDHUP = 0x2000;
private static readonly nuint _EPOLLET = 0x80000000;
private static readonly nuint _EPOLL_CLOEXEC = 0x80000;
private static readonly nuint _EPOLL_CTL_ADD = 0x1;
private static readonly nuint _EPOLL_CTL_DEL = 0x2;
private static readonly nuint _EPOLL_CTL_MOD = 0x3;

private partial struct timespec {
    public int tv_sec;
    public int tv_nsec;
}

//go:nosplit
private static void setNsec(this ptr<timespec> _addr_ts, long ns) {
    ref timespec ts = ref _addr_ts.val;

    ts.tv_sec = timediv(ns, 1e9F, _addr_ts.tv_nsec);
}

private partial struct timeval {
    public int tv_sec;
    public int tv_usec;
}

//go:nosplit
private static void set_usec(this ptr<timeval> _addr_tv, int x) {
    ref timeval tv = ref _addr_tv.val;

    tv.tv_usec = x;
}

private partial struct sigactiont {
    public uint sa_flags;
    public System.UIntPtr sa_handler;
    public array<uint> sa_mask; // linux header does not have sa_restorer field,
// but it is used in setsig(). it is no harm to put it here
    public System.UIntPtr sa_restorer;
}

private partial struct siginfo {
    public int si_signo;
    public int si_code;
    public int si_errno; // below here is a union; si_addr is the only field we use
    public uint si_addr;
}

private partial struct itimerval {
    public timeval it_interval;
    public timeval it_value;
}

private partial struct epollevent {
    public uint events;
    public array<byte> pad_cgo_0;
    public ulong data;
}

private static readonly nuint _O_RDONLY = 0x0;
private static readonly nuint _O_NONBLOCK = 0x80;
private static readonly nuint _O_CLOEXEC = 0x80000;
private static readonly nint _SA_RESTORER = 0;

private partial struct stackt {
    public ptr<byte> ss_sp;
    public System.UIntPtr ss_size;
    public int ss_flags;
}

private partial struct sigcontext {
    public uint sc_regmask;
    public uint sc_status;
    public ulong sc_pc;
    public array<ulong> sc_regs;
    public array<ulong> sc_fpregs;
    public uint sc_acx;
    public uint sc_fpc_csr;
    public uint sc_fpc_eir;
    public uint sc_used_math;
    public uint sc_dsp;
    public ulong sc_mdhi;
    public ulong sc_mdlo;
    public uint sc_hi1;
    public uint sc_lo1;
    public uint sc_hi2;
    public uint sc_lo2;
    public uint sc_hi3;
    public uint sc_lo3;
}

private partial struct ucontext {
    public uint uc_flags;
    public ptr<ucontext> uc_link;
    public stackt uc_stack;
    public array<byte> Pad_cgo_0;
    public sigcontext uc_mcontext;
    public array<uint> uc_sigmask;
}

} // end runtime_package
