// Generated using cgo, then manually converted into appropriate naming and code
// for the Go runtime.
// go tool cgo -godefs defs_linux.go defs1_linux.go defs2_linux.go

// package runtime -- go2cs converted at 2022 March 06 22:08:34 UTC
// import "runtime" ==> using runtime = go.runtime_package
// Original source: C:\Program Files\Go\src\runtime\defs_linux_riscv64.go


namespace go;

public static partial class runtime_package {

private static readonly nuint _EINTR = 0x4;
private static readonly nuint _EAGAIN = 0xb;
private static readonly nuint _ENOMEM = 0xc;
private static readonly nuint _ENOSYS = 0x26;

private static readonly nuint _PROT_NONE = 0x0;
private static readonly nuint _PROT_READ = 0x1;
private static readonly nuint _PROT_WRITE = 0x2;
private static readonly nuint _PROT_EXEC = 0x4;

private static readonly nuint _MAP_ANON = 0x20;
private static readonly nuint _MAP_PRIVATE = 0x2;
private static readonly nuint _MAP_FIXED = 0x10;

private static readonly nuint _MADV_DONTNEED = 0x4;
private static readonly nuint _MADV_FREE = 0x8;
private static readonly nuint _MADV_HUGEPAGE = 0xe;
private static readonly nuint _MADV_NOHUGEPAGE = 0xf;

private static readonly nuint _SA_RESTART = 0x10000000;
private static readonly nuint _SA_ONSTACK = 0x8000000;
private static readonly nuint _SA_RESTORER = 0x0;
private static readonly nuint _SA_SIGINFO = 0x4;

private static readonly nuint _SIGHUP = 0x1;
private static readonly nuint _SIGINT = 0x2;
private static readonly nuint _SIGQUIT = 0x3;
private static readonly nuint _SIGILL = 0x4;
private static readonly nuint _SIGTRAP = 0x5;
private static readonly nuint _SIGABRT = 0x6;
private static readonly nuint _SIGBUS = 0x7;
private static readonly nuint _SIGFPE = 0x8;
private static readonly nuint _SIGKILL = 0x9;
private static readonly nuint _SIGUSR1 = 0xa;
private static readonly nuint _SIGSEGV = 0xb;
private static readonly nuint _SIGUSR2 = 0xc;
private static readonly nuint _SIGPIPE = 0xd;
private static readonly nuint _SIGALRM = 0xe;
private static readonly nuint _SIGSTKFLT = 0x10;
private static readonly nuint _SIGCHLD = 0x11;
private static readonly nuint _SIGCONT = 0x12;
private static readonly nuint _SIGSTOP = 0x13;
private static readonly nuint _SIGTSTP = 0x14;
private static readonly nuint _SIGTTIN = 0x15;
private static readonly nuint _SIGTTOU = 0x16;
private static readonly nuint _SIGURG = 0x17;
private static readonly nuint _SIGXCPU = 0x18;
private static readonly nuint _SIGXFSZ = 0x19;
private static readonly nuint _SIGVTALRM = 0x1a;
private static readonly nuint _SIGPROF = 0x1b;
private static readonly nuint _SIGWINCH = 0x1c;
private static readonly nuint _SIGIO = 0x1d;
private static readonly nuint _SIGPWR = 0x1e;
private static readonly nuint _SIGSYS = 0x1f;

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
    public long tv_usec;
}

private static void set_usec(this ptr<timeval> _addr_tv, int x) {
    ref timeval tv = ref _addr_tv.val;

    tv.tv_usec = int64(x);
}

private partial struct sigactiont {
    public System.UIntPtr sa_handler;
    public ulong sa_flags;
    public System.UIntPtr sa_restorer;
    public ulong sa_mask;
}

private partial struct siginfo {
    public int si_signo;
    public int si_errno;
    public int si_code; // below here is a union; si_addr is the only field we use
    public ulong si_addr;
}

private partial struct itimerval {
    public timeval it_interval;
    public timeval it_value;
}

private partial struct epollevent {
    public uint events;
    public array<byte> pad_cgo_0;
    public array<byte> data; // unaligned uintptr
}

private static readonly nuint _O_RDONLY = 0x0;
private static readonly nuint _O_NONBLOCK = 0x800;
private static readonly nuint _O_CLOEXEC = 0x80000;


private partial struct user_regs_struct {
    public ulong pc;
    public ulong ra;
    public ulong sp;
    public ulong gp;
    public ulong tp;
    public ulong t0;
    public ulong t1;
    public ulong t2;
    public ulong s0;
    public ulong s1;
    public ulong a0;
    public ulong a1;
    public ulong a2;
    public ulong a3;
    public ulong a4;
    public ulong a5;
    public ulong a6;
    public ulong a7;
    public ulong s2;
    public ulong s3;
    public ulong s4;
    public ulong s5;
    public ulong s6;
    public ulong s7;
    public ulong s8;
    public ulong s9;
    public ulong s10;
    public ulong s11;
    public ulong t3;
    public ulong t4;
    public ulong t5;
    public ulong t6;
}

private partial struct user_fpregs_struct {
    public array<byte> f;
}

private partial struct usigset {
    public array<ulong> us_x__val;
}

private partial struct sigcontext {
    public user_regs_struct sc_regs;
    public user_fpregs_struct sc_fpregs;
}

private partial struct stackt {
    public ptr<byte> ss_sp;
    public int ss_flags;
    public System.UIntPtr ss_size;
}

private partial struct ucontext {
    public ulong uc_flags;
    public ptr<ucontext> uc_link;
    public stackt uc_stack;
    public usigset uc_sigmask;
    public array<byte> uc_x__unused;
    public array<byte> uc_pad_cgo_0;
    public sigcontext uc_mcontext;
}

} // end runtime_package
